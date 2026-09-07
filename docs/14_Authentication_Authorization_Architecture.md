# Authentication & Authorization Architecture Review

**Status:** Read-only analysis of the current codebase. No application code was changed as part of this review.  
**Date:** 2026-09-07  
**Branch context:** `July_to_Sept26_DevNSupport`  
**Scope:** Whole-application AuthN/AuthZ (login, session, RBAC, tenant isolation, endpoint bypasses). Sales-visit-specific IDOR details remain in `docs/sales-visit-workflow-audit/`; this document is the cross-cutting architecture view.

**Follow-on implementation:** Phase 0A authorization gates are in PR #66. Phase 0B credential/secrets hardening is in `docs/15_Phase0B_Secrets_Deployment.md`. Phase 1A page-level RBAC rollout is in `docs/16_Phase1A_SecurePage_RBAC.md`. Phase 1B role-assignment sync is in `docs/17_Phase1B_Role_Assignment.md`. Phase 2A tenant membership (A-18 company switcher ACL) is in `docs/18_Phase2A_Tenant_Membership.md`. This file remains the original as-is review.

**Method:** Static source inspection. Database access was not available; schema facts are inferred from ADO.NET usage and the SQL scripts under `Bill_Software/corporate/business/sql/`. Findings marked **CONFIRMED** are visible in source. Findings marked **PROBABLE** depend on live DDL or IIS configuration that is not in this repository.

Secrets found in `Web.config` are cited by **location only** and are not reproduced here.

---

## 1. Executive Summary

The application does **not** implement ASP.NET Forms Authentication, Membership, or Role Manager, despite those sections existing in `Web.config`. Identity is a **custom session token** (`Session["USERID"]` + `dbo.ActiveSessions`). Authorization is a **navigation cosmetic**: `UserRoles` → `RolePermissions` → `Permissions` only show/hide `<li>` menu items in `Bill.Master.GetMenuControl()`. No code-behind action consults a permission key before executing.

That design produces three independent failure modes:

1. **Authentication is stronger than it used to be, but incomplete.** Login uses PBKDF2, lockout, and a concurrent-session table — then still accepts and sometimes **rewrites** plaintext passwords, uses a non-CSPRNG OTP, and leaves a parallel card-admin login with SQL injection.
2. **Authorization is not enforced at the point of action.** Knowing a URL, calling a `[WebMethod]`, or opening a print page bypasses the menu. Admin pages that mutate roles, users, and permissions require only “is logged in.”
3. **Tenant isolation is a session variable the user can change.** `CompanyContext.CurrentCompanyID` is whatever the header dropdown last selected. The dropdown is bound to **every** active company, with no membership check.

The intended Ponytail rule (“every query is parameterized and tenant-scoped”) is being applied incrementally on some modules (sales-visit lists, `ViewUser` mutations). It is **not** a substitute for authorization: a user who can switch `Session["CompanyID"]` inherits that tenant’s data even when queries filter correctly.

---

## 2. As-Is Architecture

```
                    ┌──────────────────────────────────────────────┐
                    │  index.aspx  (custom login)                  │
                    │  tbl_login + PBKDF2 / plaintext fallback     │
                    │  Guid token → dbo.ActiveSessions             │
                    │  Session[USERID, UserDbId, RoleId, Token]    │
                    │  CompanyID is NOT set at login               │
                    └───────────────────┬──────────────────────────┘
                                        │
                    ┌───────────────────▼──────────────────────────┐
                    │  Bill.Master (de facto gate)                 │
                    │  1. Session USERID + SessionToken present?   │
                    │  2. ActiveSessions.IsActive == 1?            │
                    │  3. Force settings.aspx if pwd/contact lock  │
                    │  4. Bind ALL companies → Session[CompanyID]  │
                    │  5. GetMenuControl() hides <li> by perm key  │
                    └───────────────────┬──────────────────────────┘
                                        │
          ┌───────────────┬─────────────┼──────────────┬───────────────┐
          ▼               ▼             ▼              ▼               ▼
   Page_Load         [WebMethod]    print/*.aspx    /Uploads/      QuickAction
   USERID check      often no auth  27/30 no auth   static files   AES token
   no PermissionKey  skip Master    skip Master     skip all auth  skip session
   some CompanyID    some CompanyID no CompanyID
```

### 2.1 What exists (and works)

| Layer | Implementation | Files |
|-------|----------------|-------|
| Login lookup | Parameterized `SELECT` on `tbl_login` where `IsActive = 1` | `Bill_Software/index.aspx.cs` `btnLogin_Click` |
| Password hash | PBKDF2 (`Rfc2898DeriveBytes`, 100k iterations) + constant-time compare | `index.aspx.cs` `VerifyPasswordPBKDF2` |
| Lockout | 5 failures → 15-minute `LockoutEnd` | `index.aspx.cs` `HandleFailedLoginAttempt` |
| Concurrent session | New login deactivates prior `ActiveSessions` rows for that numeric `Id` | `index.aspx.cs` ~161–176 |
| Session re-validation | Every `Bill.Master` request re-reads `IsActive` | `Bill.Master.cs` 47–77 |
| Idle timeout | Heartbeat every ~120s; 30-minute idle deactivates token | `Heartbeat.ashx.cs` |
| Forced password / contact | Master redirects to `settings.aspx` when session flags are set | `Bill.Master.cs` 88–105 |
| Logout (ERP) | Deactivates token, `Session.Clear` / `Abandon` | `Bill.Master.cs` `btnLogOut_Click` |
| Settings password write | Hash stored, `Password = NULL` | `settings.aspx.cs` ~139 |

### 2.2 What is configured but unused

| Artifact | Why it does not protect the app |
|----------|----------------------------------|
| `<authentication mode="Forms">` | No `FormsAuthentication.SetAuthCookie` anywhere. Timeout 2880 is dead config. |
| `<membership>` / `<roleManager>` | Default providers point at `DefaultConnection`, which is not the ERP database. |
| `App_Start/AuthConfig.cs` | Empty `RegisterOpenAuth()` stub. |
| `<authorization>` / `<location>` | **Absent.** Anonymous requests are not denied at the pipeline. |

There is no `BasePage`, no `IHttpModule` auth filter, and no `[Authorize]` equivalent. Protection is whatever each page, master, or WebMethod remembers to check.

---

## 3. Identity Model

### 3.1 Two user identifiers

| Token | Source | Used for |
|-------|--------|----------|
| `tbl_login.User_Id` (string, e.g. `FLM03`) | `Session["USERID"]` | Almost all “current user” SQL (`CreatedByCode`, chat, ApprovedBy) |
| `tbl_login.Id` (int PK) | `Session["UserDbId"]` | `ActiveSessions.UserId`, `UserRoles.UserId`, settings password update |

`UserRoles` joins `ur.UserId = tbl_login.Id`. Sales-visit ownership joins `CreatedByCode = Session["USERID"]`. These are different keys; mixing them is a recurring source of bugs.

### 3.2 Login does not bind tenant

`btnLogin_Click` sets `USERID`, `USERTYPE`, `UserDbId`, `RoleId`, `RoleName`, `ProfilePic`, `SessionToken`. It does **not** set `CompanyID`.

`Bill.Master.EnsureSessionCompanyId()` later:

1. Reads `tbl_login.CompanyID` for the string `User_Id` (no `TOP 1`, no `ORDER BY`).
2. If that is null, takes **the first company in the dropdown** (all active companies).

`GetAdminName` uses `UNION ALL` to fall back to a `tbl_login` row whose `CompanyID` does **not** match the selected company. That is evidence the same `User_Id` can exist in more than one company row, **or** that users routinely operate with a selected company that is not their home company.

Login itself is `SELECT TOP 1 … WHERE User_Id = @UserId AND IsActive = 1` with **no `ORDER BY` and no `CompanyID`**. If `User_Id` is not unique, which row is authenticated is undefined. **PROBABLE High** pending confirmation of a `UNIQUE` constraint on `tbl_login.User_Id` (also flagged in `docs/08_Cursor_Change_Audit.md`).

### 3.3 `cmbLoginAs` is a no-op

The login form offers ADMIN vs Employee. `btnLogin_Click` runs the same `tbl_login` query for both `SelectedIndex` values and only stores the label in `Session["USERTYPE"]`. That session value is not used as an authorization gate. **CONFIRMED.**

---

## 4. Authentication Bugs

Severity uses: **Critical** = credential theft, unauthenticated data access, or trivial remote exploit; **High** = authenticated privilege abuse or weak secrets that enable takeover; **Medium** = defense-in-depth / operational; **Low** = dead code / inconsistency.

| ID | Severity | Status | Finding |
|----|----------|--------|---------|
| A-01 | **Critical** | CONFIRMED | Plaintext password column is still accepted at login (`index.aspx.cs` 133–136). Forgot-password **nulls the hash** and stores a temp password in `Password` (`index.aspx.cs` 343–345). The voluntary password page **re-writes plaintext** into `Password` while also hashing (`Update/password.aspx.cs` 343–352). `settings.aspx` is the only path that correctly sets `Password = NULL`. |
| A-02 | **Critical** | CONFIRMED | `index_card.aspx.cs` 47 concatenates username into SQL against `tbl_card_login`. Classic SQL injection on a login form. Password compare is plaintext. |
| A-03 | **Critical** | CONFIRMED | `Web.config` holds live DB, SMTP, iTop, and Msg91 secrets in source. `machineKey` is a hardcoded SHA1 key (`Web.config` ~36). Anyone with repo access can forge ViewState and read production systems. |
| A-04 | **High** | CONFIRMED | Login/forgot OTP uses `new Random().Next(100000, 999999)` (`index.aspx.cs` 583–586). Login OTP is stored in session plaintext with **no expiry and no attempt limit** (`btnSendOTP_Click` / `btnVerifyOTP_Click`). |
| A-05 | **High** | CONFIRMED | Forgot-password resets `FailedAccessCount` and `LockoutEnd`, so lockout is bypassable. Distinct error strings enumerate valid / inactive User IDs. |
| A-06 | **High** | CONFIRMED | `admin/card.Master.cs` logout only redirects; it does not `Session.Clear` / `Abandon` and does not touch `ActiveSessions`. |
| A-07 | **High** | CONFIRMED | No URL authorization. Pages that omit a session check (or are not under `Bill.Master`) are anonymously reachable. Print pages and WebMethods are the practical bypass. |
| A-08 | **High** | CONFIRMED | `QuickAction.aspx` approves leave/regularization from a URL token. AES key is a 32-byte string literal in `SecurityHelper.cs` line 12. Possession of a mailed link (or the key) is authorization. Tokens have no server-side single-use / expiry table. |
| A-09 | **Medium** | CONFIRMED | Remember-me cookie stores username (card login still *reads* a `password` cookie value). No `<httpCookies httpOnlyCookies="true" requireSSL="true" />`. No `SameSite`. |
| A-10 | **Medium** | CONFIRMED | `customErrors mode="Off"` and `compilation debug="true"` leak stack traces. |
| A-11 | **Medium** | CONFIRMED | `SessionKeepAlive.aspx.cs` / `SessionKeepAlive1.aspx.cs` concatenate `Session["USERID"]` into SQL. Legacy keep-alive is not the Heartbeat path, but the pages still exist. |
| A-12 | **Medium** | CONFIRMED | InProc session state is not multi-server safe. Heartbeat idle is 30 minutes; ASP.NET default session timeout is 20 minutes unless raised. Heartbeat posts keep the session alive in practice, so the 30-minute rule is the real idle policy **if Heartbeat succeeds**. |
| A-13 | **Medium** | PROBABLE | `ActiveSessions` INSERT does not set `LastHeartbeat`. `Heartbeat.ashx.cs` 53 calls `rdr.GetDateTimeOffset(1)` which throws on `NULL`. First heartbeat after login can fail depending on column default. |
| A-14 | **Low** | CONFIRMED | `AuthConfig`, Membership, RoleManager, and unused Forms timeout add a false sense of pipeline protection. |
| A-15 | **Medium** | CONFIRMED | `SendWhatsAppMessageAsync` always returns `true` after fire-and-forget (`index.aspx.cs` 589–597). Reset UI can claim WhatsApp delivery that never happened. The password **has already been replaced** by then. |
| A-16 | **Medium** | CONFIRMED | `Update/password.aspx.cs` stores OTP expiry with `DateTime.Now` and compares with `DateTime.UtcNow` — expiry window is wrong by local offset. |

---

## 5. Authorization Model (RBAC)

### 5.1 Two role systems that never meet

| System | Storage | Written by | Read by | Effect |
|--------|---------|------------|---------|--------|
| **A. Display role** | `tbl_login.RoleId` → `Roles` | `AddUser.aspx.cs`, `ViewUser.aspx.cs` | Login → `Session["RoleId"]` / `RoleName`; `GetAdminName` header label | Cosmetic. `Session["RoleId"]` is **never read** for a decision. |
| **B. Menu role** | `UserRoles` (user PK ↔ role) + `RolePermissions` + `Permissions` | **Only** `Update_Designation.aspx.cs` | **Only** `Bill.Master.GetMenuControl()` | Hides menu `<li>` whose `id` matches `PermissionKey`. |

`AddUser` writes `RoleId` and never inserts `UserRoles`. A newly provisioned user can log in, see a role in the header, and get an empty menu until someone opens View User → MenuEdit → `Update_Designation.aspx`.

The two assignments can diverge indefinitely. That is not currently a privilege-escalation *by itself*, because **neither system is checked before an action**. It is an operational failure and a landmine for any future “check RoleId” patch that would still ignore menu grants (or vice versa).

### 5.2 `GetMenuControl` is the only enforcement point — and it is leaky

```257:291:Bill_Software/corporate/business/app/Bill.Master.cs
                string sqlUserPerms = @"
                    SELECT DISTINCT p.PermissionKey 
                    FROM dbo.Permissions p
                    INNER JOIN dbo.RolePermissions rp ON p.PermissionId = rp.PermissionId
                    INNER JOIN dbo.UserRoles ur ON rp.RoleId = ur.RoleId
                    INNER JOIN dbo.tbl_login u ON ur.UserId = u.Id AND u.CompanyID = @CompanyID
                    WHERE u.User_Id = @UserId";
                // ...
                if (userGrantedPermissions.Count == 0)
                {
                    const string sqlUserPermsFallback = @"
                        SELECT DISTINCT p.PermissionKey 
                        ...
                        INNER JOIN dbo.tbl_login u ON ur.UserId = u.Id
                        WHERE u.User_Id = @UserId";
```

Implications:

- Permission keys are HTML control IDs, not resource/action names (`visit_planner`, `srch_dailyrpts`, …).
- Direct URL access is unrestricted.
- Fallback **drops CompanyID**, so empty grants in the selected tenant load another tenant’s menu roles (**CONFIRMED High**, cross-tenant permission bleed).
- Menu is computed only on `!IsPostBack`. Role changes mid-session do not apply until a full GET.

### 5.3 Admin surfaces have no admin permission

These pages check session (via own `Page_Load` and/or `Bill.Master`) and then allow the action:

| Page | What any logged-in user can do if they know the URL |
|------|------------------------------------------------------|
| `Update_Designation.aspx` | Reassign any user’s `UserRoles` (`?User_Id=`) |
| `ManageRoles.aspx` | Rewrite `RolePermissions`; create roles |
| `ManagePermissions.aspx` | CRUD the global permission catalog |
| `AddUser.aspx` / `ViewUser.aspx` | Provision, lock, delete, reset password, geo-fence (ViewUser mutations are CompanyID-scoped) |
| `AdminLeaveSetup.aspx` | Insert `tbl_LeaveMaster` rows (Master still requires login; **no permission check**) |

`ManageRoles.btnCreateRole_Click` inserts `Roles (RoleName, Description)` **without `CompanyID`**, despite `Roles.CompanyID` existing (`CompanyID_Roles_ActiveSessions.sql`). New roles become default-tenant or constraint-fail depending on DDL. **CONFIRMED.**

`Update_Designation` filters the role list by `CompanyContext.CurrentCompanyID` in the UI but does not re-validate posted `RoleId` values against that company.

### 5.4 `ReportingManagerId` is not authorization

It is used for e-mail routing and display (Add/View User). It is **not** applied as “this manager may only approve their reports.” Combined with menu-only RBAC, any user who can load `srch_dailyrpts.aspx` can approve any visit they can address by `Id` (see A-22). Whether company-wide manager access is *intended* is still an open product decision (`docs/sales-visit-workflow-audit/07_Proposed_Target_Architecture.md` Decision #8).

---

## 6. Tenant Isolation vs Authorization

Ponytail Standard #1 requires `CompanyID = @CompanyID` on tenant-scoped queries. That is **data scoping**, not **who may act**.

### 6.1 The company switcher defeats query-level tenancy

```150:198:Bill_Software/corporate/business/app/Bill.Master.cs
        private void BindCompanies()
        {
            // SELECT ID, Name FROM tbl_Company WHERE IsActive = 1 OR IsActive IS NULL
            // — no user/tenant membership predicate
        }
        protected void ddlCompany_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["CompanyID"] = ddlCompany.SelectedValue;
            Response.Redirect(Request.RawUrl, false);
        }
```

**CONFIRMED Critical.** Any authenticated user can set `Session["CompanyID"]` to any active company. Pages that correctly filter by `CompanyContext.CurrentCompanyID` then return **that** company’s data. This is the highest-leverage authz bug in the system: one control undoes every subsequent `AND CompanyID = @CompanyID`.

`CompanyContext.CurrentCompanyID` returns `0` when unset (not a hard fail). `CurrentCompanyCode` falls back to `"FE"`. Queries with `CompanyID = 0` either return nothing or hit a junk bucket.

### 6.2 Partial remediation (do not treat the audit as fully current)

Since the sales-visit audit documents were written, several list/detail queries **have** gained `CompanyID` parameters (`visit_planner.GetCalendarEvents` / `GetVisitDetails`, `vw_dailyrpts` bind/update, `daily_rpt` insert, `srch_dailyrpts` Binder — now parameterized, which **closes D-05 as originally described**).

Gaps that remain on that workflow:

| Location | Still missing |
|----------|----------------|
| `visit_planner` execute `UPDATE … WHERE Id = @Id` | CompanyID and ownership on the **UPDATE** (follow-up INSERT is company-scoped) |
| `srch_dailyrpts.ProcessApproval` | `WHERE Id = @Id` only — no CompanyID, no manager relationship, no pending-state guard |
| `srch_dailyrpts` expense approve/reject | `WHERE Id = @Id` only |
| Detail methods with CompanyID | Still no `CreatedByCode` / permission check — intra-tenant IDOR |

### 6.3 Modules that still ignore CompanyID

Examples (not exhaustive): `Delete_client.aspx.cs` lists/deletes `tbl_Client` with no company predicate; `Delete_vendor.aspx.cs` same pattern; many Hydrent/legacy pages; most `print/` pages. `CompanyContext` returning `0` plus unscoped `SELECT *` is a cross-tenant dump.

---

## 7. Bypass Surfaces (gates that never run)

`Bill.Master.Page_Load` does not run for `[WebMethod]` static methods, HTTP handlers, static files, or pages without that master.

### 7.1 Print documents — unauthenticated IDOR

Of 30 code-behinds under `corporate/business/print/` (plus `Print/id_card1.aspx.cs`), **only three** check `Session["USERID"]`:

- `NewPurchaseOrder.aspx.cs`
- `NewPurchaseOrder_Print.aspx.cs`
- `NewInvoice_v2.aspx.cs`

The rest, including `NewInvoice.aspx.cs`, `Quotation.aspx.cs`, `Invoice.aspx.cs`, load by `?ID=` with no session and often **string-concatenated SQL**:

```48:70:Bill_Software/corporate/business/print/NewInvoice.aspx.cs
                string ID = Request.QueryString["ID"];
                // ...
                "WHERE i.ID = '" + ID.ToString() + "'";
```

**CONFIRMED Critical:** unauthenticated financial-document disclosure **and** SQL injection.

### 7.2 WebMethods — catalog and stock without login

`[WebMethod]` defaults to `EnableSession = false`. Methods that never check `Session["USERID"]` are callable with a raw POST.

| File | Methods | Notes |
|------|---------|--------|
| `Product_stock.aspx.cs` | `SearchProducts`, `GetStock`, `GetCategories` | No session, no CompanyID |
| `search_products.aspx.cs` | `GetProductDetails`, `SearchProducts` | No session, no CompanyID |
| `View_purches.aspx.cs` | suggestion methods | No session, no CompanyID |
| `View_chalan.aspx.cs` / `add_chalan.aspx.cs` | client/document lookups | No session, no CompanyID |
| `visit_planner.aspx.cs` | `GetVisitDetails` | `EnableSession=true`, CompanyID only, no USERID / ownership |

**None** of the WebMethods in the repository check `RolePermissions`.

### 7.3 Static uploads and ashx

`~/Uploads/` (visit photos, expense receipts, profiles, invoice logs) and `~/SupportUploads/` are served as static content. No `Uploads/web.config` denying anonymous or script execution was found. Filenames are timestamp + original name — guessable if an e-mail notification leaked the name, and **unauthenticated** if guessed.

`admin/Company.ashx.cs` and `admin/personal_image.ashx.cs` stream bytes by ID with no session check.

Upload sites do not enforce extension allow-lists. Whether `.aspx` in `Uploads/` is executable is **PROBABLE** pending IIS handler mapping.

### 7.4 Card-admin island

`index_card.aspx` + `admin/card.Master.cs` is a second authentication domain (`tbl_card_login`), weaker than ERP login, sharing the same `Session["USERID"]` key. A card session can look like an ERP session to any page that only tests `Session["USERID"] != null` and does not require `SessionToken`. **CONFIRMED High** confusion / possible gate skip on pages that do not use `Bill.Master`.

---

## 8. Finding Index (authorization & tenancy)

| ID | Severity | Status | Finding |
|----|----------|--------|---------|
| A-17 | **Critical** | CONFIRMED | RBAC is menu-only. Direct URL / WebMethod / print / upload is the real ACL. |
| A-18 | **Critical** | CONFIRMED | Company dropdown has no membership ACL (see §6.1). |
| A-19 | **Critical** | CONFIRMED | Unauthenticated print IDOR + SQLi (§7.1). |
| A-20 | **Critical** | CONFIRMED | Unauthenticated WebMethods expose products/clients/vendors/stock (§7.2). |
| A-21 | **Critical** | CONFIRMED | `Delete_client` / `Delete_vendor` unscoped lists and deletes. |
| A-22 | **High** | CONFIRMED | Visit/expense **approval** `UPDATE` has no CompanyID, ownership, or manager check (`srch_dailyrpts.ProcessApproval`). |
| A-23 | **High** | CONFIRMED | Dual role systems; `AddUser` never seeds `UserRoles`. |
| A-24 | **High** | CONFIRMED | `GetMenuControl` CompanyID fallback bleeds permissions. |
| A-25 | **High** | CONFIRMED | Role/permission/user-admin pages lack a permission gate. |
| A-26 | **High** | CONFIRMED | Static `/Uploads` and image ashx handlers. |
| A-27 | **High** | CONFIRMED | QuickAction token + hardcoded AES key (A-08). |
| A-28 | **Medium** | CONFIRMED | Intra-tenant IDOR on visit detail/execute/update (CompanyID present, `CreatedByCode` absent). |
| A-29 | **Medium** | CONFIRMED | `ManageRoles` create-role omits `CompanyID`. |
| A-30 | **Medium** | CONFIRMED | `AdminLeaveSetup` has no permission check (login required via Master only). |
| A-31 | **Low** | CONFIRMED | `GetMenuControl` only on first GET; `CompanyCode` `"FE"` fallback. |

---

## 9. What “good” looks like for this stack

A rewrite off Web Forms is out of scope. The target is **incremental extraction** so existing `.aspx.cs` files cannot bypass policy.

```
  Page / WebMethod / ashx / print
              │
              ▼
     AuthenticatedPrincipal
       (User_Id, Id, CompanyIDs[], PermissionKeys[], SessionToken)
              │
              ▼
     AuthorizationPolicy.Can(user, action, resource)
       — permission key
       — tenant membership (not session dropdown)
       — ownership or manager scope when required
              │
              ▼
     Repository query always AND CompanyID IN (user.CompanyIDs)
```

Concrete components (names illustrative):

1. **`BasePage` / master + WebMethod helper** that refuse the request unless session token is active **and** `HasPermission(key)` — menu visibility becomes a *projection* of the same set.
2. **Single role source.** On `AddUser` / `ViewUser` role change, write **both** `tbl_login.RoleId` (display) and `UserRoles` (grants), or drop `RoleId` entirely. Remove the `GetMenuControl` no-company fallback.
3. **Company membership.** Table or join that lists which companies a user may select. Bind `ddlCompany` from that list. Changing company must fail if not a member. Super-admin (if it exists) is an explicit permission, not “show every row in `tbl_Company`.”
4. **Authenticated file handler** for `/Uploads` (and deny anonymous in `web.config` `<location>`). Extension allow-list on upload.
5. **Print and WebMethod wrappers** that require session + CompanyID + parameterized `Id`.
6. **Password pipeline:** one write path (hash only, `Password` always NULL), CSPRNG OTPs with expiry and attempt caps, lockout that forgot-password cannot silently clear without a one-time token.

Open product decisions that block a complete authz matrix (do not invent answers in code):

- Is cross-company access a real HQ role? (Decision #9 in the sales-visit proposal.)
- Does `ReportingManagerId` restrict approvals, or is company-wide manager access intended? (Decision #8.)
- Is `tbl_login.User_Id` unique globally? If not, login and `ActiveSessions` must key off `(User_Id, CompanyID)` or the numeric `Id` consistently.

---

## 10. Improvement Scope (phased)

Effort is described by **surface area**, not calendar time.

### Phase 0 — Stop unauthenticated and cross-tenant leaks

Touch: `Web.config` location rules, `Bill.Master` company bind, print pages, WebMethods, `Uploads`, password writers, secret storage.

| Work item | Closes | Invasiveness |
|-----------|--------|----------------|
| Restrict `ddlCompany` to companies the user belongs to; refuse `SelectedIndexChanged` if not a member | A-18 | Small UI + one membership query; **requires a membership source** (column, bridge table, or explicit super-admin permission) |
| Add session + `CompanyID` + parameterized ID to all `print/` pages (copy the three pages that already check session) | A-19 | Many files, mechanical |
| Shared `WebMethodAuth.RequireUser()` used as first line of every `[WebMethod]` | A-20 | Mechanical; must set `EnableSession = true` |
| `<location path="Uploads">` deny anonymous; serve via `.ashx` that checks session | A-26 | Config + one handler + link rewrites |
| Stop writing `Password` plaintext; hashed reset tokens instead of temp password in the password column | A-01, A-05 | Login/forgot/Update/password |
| Parameterize `index_card` or disable the page if unused | A-02 | Tiny |
| Remove secrets from the repo; rotate everything that has been committed | A-03 | Ops + `Web.Release.config` / env |
| Gate `Update_Designation`, `ManageRoles`, `ManagePermissions`, `AddUser`, `ViewUser`, `AdminLeaveSetup` on a permission key (even a single `admin_users` key) | A-25, A-30 | Small if key exists; otherwise add one permission and assign it |

Phase 0 does **not** require unifying the two role tables. It does require **one** real permission check on admin URLs so menu-hiding is not the ACL.

### Phase 1 — Make RBAC real

Touch: `Bill.Master`, `AddUser`, `ViewUser`, `Update_Designation`, new `Authorization` helper.

| Work item | Closes | Invasiveness |
|-----------|--------|----------------|
| `HasPermission(key)` used by Master **and** `Page_Load` / postbacks | A-17 | Needs a map of page → permission key (today the key *is* the menu control id — reuse it) |
| Seed `UserRoles` when `AddUser` / `ViewUser` sets `RoleId`; optionally sync the reverse on `Update_Designation` | A-23 | Two write paths |
| Delete the no-CompanyID menu fallback | A-24 | One branch in `GetMenuControl` |
| `Roles` INSERT always includes `CompanyID` | A-29 | One statement |
| Replace `System.Random` OTP with `RNGCryptoServiceProvider`; add expiry + attempt limit on login OTP | A-04 | Login + settings (settings contact OTP already has expiry/attempts; generation is still weak) |
| Cookie flags, `customErrors`, `debug="false"` for non-dev | A-09, A-10 | Config |
| Card logout `Session.Abandon`; require `SessionToken` on any page that currently checks only `USERID` | A-06, card island | Master + stragglers |
| QuickAction: store token hash server-side, expiry, single-use; move AES key to config | A-08 / A-27 | Leave/reg e-mail links |

### Phase 2 — Resource-level authorization

Touch: visit/expense/invoice/client repositories.

| Work item | Closes | Invasiveness |
|-----------|--------|----------------|
| Every mutation `WHERE Id=@Id AND CompanyID=@CompanyID` plus ownership or manager predicate | A-21, A-22, A-28 | Per-module; sales visit is the template |
| Resolve Decision #8 before coding manager scope | A-22 product rule | Policy, then one helper |
| Confirm `User_Id` uniqueness; if not unique, bind login to numeric `Id` + company | §3.2 | Login + session shape |
| CSRF: anti-forgery on WebMethods (custom header) in addition to session cookie | E.3 in tenant audit | JS + server |
| Admin UI to revoke `ActiveSessions` (ViewUser currently history-only) | Session ops | Small |

### Phase 3 — Housekeeping (after the gates exist)

- Delete or isolate `AuthConfig`, unused Membership/RoleManager, `SessionKeepAlive*.aspx` if Heartbeat is canonical.
- Deprecate `tbl_card_login` / `index_card` or bring it up to the ERP password/session bar.
- Stop treating `PermissionKey == menu control ID` as the long-term model; introduce `resource.action` keys (`invoice.read`, `visit.approve`) and map menus onto them.
- Out of process session (SQLServer/StateServer) if more than one IIS worker/server is required.

---

## 11. Suggested Implementation Order (PRs)

These are **proposed** PR boundaries, not work performed here.

1. **Config & secrets:** strip committed credentials, cookie/customErrors/debug, deny anonymous on `Uploads` (ops rotation in parallel).
2. **Company membership bind** — highest leverage; unblocks honest tenant filters.
3. **WebMethod + print auth helper** — one shared method, then mechanical application.
4. **Admin permission gate** on the six admin pages in Phase 0.
5. **Password/OTP hardening** (single write path, CSPRNG, hashed reset).
6. **Unify UserRoles seeding** + remove menu fallback.
7. **Resource-level WHERE clauses** module by module (sales visit approvals first — A-22).
8. **Authenticated download handler** + upload allow-list.
9. **QuickAction token store** and card-admin hardening.

Do not start (7) under the assumption that `CompanyID` on the query equals security while (2) is still open.

---

## 12. Relationship to Existing Docs

| Document | Role vs this review |
|----------|---------------------|
| `docs/03_Role_Permissions.md` | Module how-to for `Update_Designation`; still accurate that permissions are menu-only |
| `docs/sales-visit-workflow-audit/09_Current_Authorization_Implementation_Audit.md` | Deep sales-visit endpoint matrix; some CompanyID filters have landed since that audit |
| `docs/sales-visit-workflow-audit/04_Security_and_Tenant_Audit.md` | Attachment + CSRF + visit IDOR detail |
| `docs/sales-visit-workflow-audit/07_Proposed_Target_Architecture.md` | Target components and open business decisions (#8, #9) this review depends on |
| README Security Notice | Still lists D-05 as open; **Binder is parameterized in current source** — treat README’s D-05 row as stale |

---

## 13. Positive Controls (keep)

- Parameterized modern login SQL.
- PBKDF2 100k + XOR constant-time compare on the hash path.
- Single concurrent logical session via `ActiveSessions`.
- Master-page token re-check on every full page.
- Forced password and contact completion before the rest of the UI.
- `settings.aspx` clearing the plaintext password column.
- Incremental `CompanyID` parameterization already present on many sales-visit **reads** and on `ViewUser` mutations — the pattern to copy, once the company switcher is bound to membership.
