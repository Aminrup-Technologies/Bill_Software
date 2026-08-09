# 09 — Current Authorization Implementation Audit

**Read-only inspection.** No file was created, modified, deleted, or renamed as part of this inspection except this single documentation file. No database object was queried or modified (see §0 for the database-access status). No refactoring, fix, or code change of any kind was performed.

---

## 0. Database Access Status

```text
DATABASE ACCESS: NOT AVAILABLE
```

No SQL Server client tooling (`sqlcmd`, `osql`, ODBC/`pyodbc` driver, etc.) is installed in this inspection environment, and no already-configured, ready-to-use database connection exists for this session. Per instructions, no attempt was made to install a client, extract/reuse the plaintext credentials embedded in `Web.config` to establish a new connection, or otherwise configure database access. **All findings in this document are derived exclusively from static source-code inspection.**

One relevant, purely observational note from source inspection (not a live query): the active `Web.config` connection string in this repository points to a database named **`flamex_live`** (with a second, commented-out connection string for `flamex_uat`), not `flamex_uat` as referenced in this task's prompt. This is reported as observed, not reconciled — see §23.

---

## 1. Executive Summary

The application has **two structurally independent role/permission mechanisms** that are never reconciled with each other, and **neither is used to gate any Sales-Visit-workflow server-side action**:

1. **`tbl_login.RoleId` (single-valued)** — populated at login into `Session["RoleId"]`/`Session["RoleName"]`, but **these two session values are never read anywhere else in the codebase** after being set (confirmed by repository-wide search). `RoleId` is otherwise used only for cosmetic display (header role label, admin user-management grids) and is edited independently via the `ViewUser.aspx.cs` ("WebForm80") admin grid.
2. **`UserRoles` (many-to-many) + `RolePermissions` + `Permissions`** — the only genuinely "wired up" authorization mechanism in the entire codebase, but it is used **exclusively to show/hide navigation menu `<li>` elements** in `Bill.Master.cs :: GetMenuControl()`. It is never consulted by any `.aspx.cs` code-behind method to decide whether a requested action should be allowed. `UserRoles` is maintained independently via `Update_Designation.aspx.cs`.

These two mechanisms can silently diverge for the same user (e.g., `tbl_login.RoleId` says one thing, `UserRoles` says another), and **it does not matter for actual authorization, because neither one is checked before any Sales Visit action executes.** The only real gate on every Sales-Visit page and PageMethod is `Session["USERID"] != null` — i.e., **"are you logged in at all"**, with no further RBAC, tenant, or ownership check at the point of action for most operations.

Separately, `CompanyID` (tenant scope) is applied inconsistently — enforced on some list queries (`srch_dailyrpts.aspx.cs`), never enforced on `INSERT` (so every visit created by this workflow has an unpopulated `CompanyID`), and never re-checked on any single-record detail/mutation endpoint. `ReportingManagerId` is used only for e-mail routing, never for authorization. The manager approval dashboard for Sales Visits is `srch_dailyrpts.aspx`, **not** `AdminApprovalDashboard.aspx` — the latter is a **completely separate feature** (Attendance Regularization + Leave approvals) that has no code path touching `tbl_SalesVisitReport` at all. This corrects an assumption in the task prompt; see §9/§14/§22.

---

## 2. Authentication Flow

### Finding: Custom session-based authentication (not ASP.NET Forms Authentication)

Status: **CONFIRMED**

Evidence:
- File: `Bill_Software/index.aspx.cs`
- Class: `Bill_Software.index`
- Method: `btnLogin_Click`
- Database object: `tbl_login` (columns: `Id`, `User_Id`, `Password`, `PasswordHash`, `PasswordSalt`, `MustChangePassword`, `EmailVerified`, `Email`, `Phone_no`, `ProfilePictureUrl`, `RoleId`, `FailedAccessCount`, `LockoutEnd`); `Roles` (`RoleName`, joined via `RoleId`); `dbo.ActiveSessions` (`SessionToken`, `UserId`, `IPAddress`, `UserAgent`, `IsActive`)
- Relevant behavior: `btnLogin_Click` looks up `tbl_login` by `User_Id`, verifies the password via PBKDF2 (`VerifyPasswordPBKDF2`) with a plaintext-`Password`-column fallback path, checks `LockoutEnd`/`FailedAccessCount` for account lockout, and on success populates several `Session[...]` values (see §3) and inserts a row into `dbo.ActiveSessions` with a new GUID `SessionToken`, which is also stored in `Session["SessionToken"]`.
- Impact: Authentication is entirely custom (raw ADO.NET + a hand-rolled session token table), not `System.Web.Security` Forms Authentication (`web.config` `<authentication mode="Forms">` was not found associated with any login-cookie/ticket mechanism in the inspected files — no `FormsAuthentication.SetAuthCookie` or similar call was found anywhere in the repository).

### Finding: Concurrent-session enforcement is centralized in the Master Page, not per-page

Status: **CONFIRMED**

Evidence:
- File: `Bill_Software/corporate/business/app/Bill.Master.cs`
- Class: `Bill_Software.corporate.business.app.Bill` (a `MasterPage`)
- Method: `Page_Load`
- Database object: `dbo.ActiveSessions.IsActive`, `.SessionToken`
- Relevant behavior: On every master-page load, `Page_Load` requires both `Session["USERID"]` and `Session["SessionToken"]` to be non-null, then re-validates the token against `dbo.ActiveSessions.IsActive` on **every single page request** that uses this master page. If the token is missing/inactive, the session is cleared and the user is redirected to `~/index.aspx`.
- Impact: This is the **de facto common authorization/authentication gate** for every page that uses `Bill.Master` (all four Sales Visit pages do — confirmed via each `.aspx` file's `MasterPageFile="~/corporate/business/app/Bill.Master"` directive). It answers only "is this a currently-valid, non-revoked session for *some* logged-in user" — it performs **no** role, permission, or tenant check.
- Architecture interpretation: This is the closest thing to a "BasePage" pattern in the codebase, but it lives in the **Master Page**, not a shared base `Page` class. No `BasePage.cs` or equivalent exists anywhere in the repository (confirmed by search).

### Finding: Individual Sales Visit pages additionally re-check `Session["USERID"]` in their own `Page_Load`

Status: **CONFIRMED**

Evidence:
- Files: `visit_planner.aspx.cs`, `daily_rpt.aspx.cs`, `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs`
- Method: `Page_Load` in each
- Relevant behavior: Each independently contains `if (HttpContext.Current.Session["USERID"] == null) { Response.Redirect("~/index.aspx"); }` — this is **redundant** with the Master Page's stricter check (which also requires `SessionToken` validity against the database), but is not harmful; it does not check `Session["SessionToken"]` itself, relying on the Master Page to have already done so.
- Impact: No unique or additional authorization logic exists at the individual page level beyond "logged in."
- Confidence: Confirmed by direct reading of all four files' `Page_Load` methods (previously verified in this audit series and re-confirmed in this session).

---

## 3. Current User Identity Resolution

### Finding: `Session["USERID"]` holds `tbl_login.User_Id` (the business/natural key), not `tbl_login.Id` (the numeric surrogate key)

Status: **CONFIRMED**

Evidence:
- File: `Bill_Software/index.aspx.cs`
- Method: `btnLogin_Click`
- Relevant behavior: `Session["USERID"] = user.UserId;` where `user.UserId` was populated from `rdr["User_Id"]` (the string business key, e.g. `"FLM035"`), not `rdr["Id"]` (the numeric identity column, separately captured as `user.Id` and stored in `Session["UserDbId"]`).
- Database object: `tbl_login.User_Id` (string business key), `tbl_login.Id` (int surrogate key)
- Impact: **Every** Sales-Visit query that filters "the current user's own records" (e.g. `WHERE CreatedByCode = @UserId`) compares against the **string** `User_Id`, obtained directly from `Session["USERID"]`. This is the authoritative current-user identity used throughout the Sales Visit workflow.

### Finding: A second, numeric identity (`Session["UserDbId"]`) exists but is not used by the Sales Visit workflow

Status: **CONFIRMED**

Evidence:
- File: `Bill_Software/index.aspx.cs` (sets it: `Session["UserDbId"] = user.Id;`)
- Files searched for reads: `visit_planner.aspx.cs`, `daily_rpt.aspx.cs`, `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs` — **no reference to `Session["UserDbId"]` found in any of the four.**
- Impact: The Sales Visit workflow exclusively identifies "current user" via the string `Session["USERID"]` → `tbl_login.User_Id`. The numeric `tbl_login.Id` surrogate key is used elsewhere in the application (e.g. `dbo.ActiveSessions.UserId` is the numeric `Id`, and `UserRoles.UserId` also joins to `tbl_login.Id`, per `Bill.Master.cs :: GetMenuControl`'s `INNER JOIN dbo.tbl_login u ON ur.UserId = u.Id`), but this numeric identity is never the join key inside the four Sales Visit files.

### Complete Identity Path: Login → Session → Page → Database

```text
1. index.aspx (Page_Load / btnLogin_Click)
      ↓  SELECT ... FROM tbl_login WHERE User_Id = @UserId  (parameterized)
2. Session["USERID"]      = tbl_login.User_Id   (string business key)
   Session["UserDbId"]    = tbl_login.Id        (int surrogate key — unused downstream in this workflow)
   Session["RoleId"]      = tbl_login.RoleId    (int — write-only, see §4)
   Session["RoleName"]    = Roles.RoleName      (string — write-only, see §4)
   Session["USERTYPE"]    = cmbLoginAs.SelectedValue ("ADMIN" or "Employee" — write-only; no read found anywhere in the repository)
   Session["SessionToken"]= new Guid            (validated against dbo.ActiveSessions on every subsequent page load)
3. Bill.Master.cs (Page_Load, on every subsequent page)
      ↓  re-validates Session["SessionToken"] against dbo.ActiveSessions.IsActive
4. visit_planner.aspx.cs / daily_rpt.aspx.cs / vw_dailyrpts.aspx.cs / srch_dailyrpts.aspx.cs
      ↓  read Session["USERID"] directly, use it as @CreatedByCode / @UserId / @RespondentCode / @ApprovedBy parameter value
5. tbl_SalesVisitReport / tbl_SalesVisitResponses / tbl_Expenses queries
```

---

## 4. Role Resolution

### Finding: The application maintains two independent role representations for the same user

Status: **CONFIRMED**

Evidence (Representation A — `tbl_login.RoleId`, single-valued):
- File: `Bill_Software/index.aspx.cs` — Method: `btnLogin_Click` — reads `u.RoleId` via `LEFT JOIN Roles r ON u.RoleId = r.RoleId`, stores into `Session["RoleId"]`/`Session["RoleName"]`.
- File: `Bill_Software/corporate/business/app/Bill.Master.cs` — Method: `GetAdminName` — independently re-queries `SELECT u.Name, r.RoleName, ... FROM tbl_login u LEFT JOIN Roles r ON u.RoleId = r.RoleId WHERE u.User_Id=@UserId` on every page load, to display the role name in the page header. (Note: this method queries the database directly; it does **not** read the `Session["RoleName"]` value that was set at login, meaning the header display and the login-time session value are two separate reads of the same underlying column, not guaranteed to be sourced identically if `RoleId` changes mid-session.)
- File: `Bill_Software/corporate/business/app/ViewUser.aspx.cs` (class `WebForm80`) — Method: `lvUsers_ItemUpdating` — admins edit `tbl_login.RoleId` directly via a grid dropdown (`UPDATE dbo.tbl_login SET ... RoleId = @RoleId ... WHERE Id = @Id AND CompanyID = @CompanyID`).
- Database object: `tbl_login.RoleId` (FK → `Roles.RoleId`)

Evidence (Representation B — `UserRoles`, many-to-many):
- File: `Bill_Software/corporate/business/app/Update_Designation.aspx.cs` — Methods: `LoadAvailableRoles`, and the role-save handler — admins assign/revoke roles via `SELECT RoleId FROM dbo.UserRoles WHERE UserId = @NumericId` (read) and `DELETE FROM dbo.UserRoles WHERE UserId = @UserId` + `INSERT INTO dbo.UserRoles (UserId, RoleId) VALUES (...)` (a full delete-and-reinsert per save) — note this table's `UserId` column is the **numeric** `tbl_login.Id`, not the string `User_Id`.
- File: `Bill_Software/corporate/business/app/Bill.Master.cs` — Method: `GetMenuControl` — reads a user's roles (and their merged permissions) via `UserRoles`, see §6.
- Database object: `UserRoles.UserId` (FK → `tbl_login.Id`), `UserRoles.RoleId` (FK → `Roles.RoleId`)

Impact: **These two representations are maintained by two entirely separate admin screens (`ViewUser.aspx` / `WebForm80` vs. `Update_Designation.aspx`) with no code path that keeps them in sync.** An administrator using one screen has no visibility into, or effect on, the data maintained by the other. It is architecturally possible (and, per the task's supplied example of `User_Id = FLM035` holding both `RoleId`-style role `4` and `UserRoles` entries for roles `3` and `4`) for a user's single `tbl_login.RoleId` value to disagree with their full `UserRoles` set.

Architecture interpretation: This is not a "which one is right" ambiguity to be resolved by code inspection — both are real, live, independently-edited data stores. See §5 for what actually happens when a user has multiple `UserRoles` rows, and §23 for the open business-rule question this raises.

### Finding: Neither `Session["RoleId"]` nor `Session["RoleName"]` is read anywhere after being set at login

Status: **CONFIRMED**

Evidence:
- Repository-wide search for `Session["RoleId"]` and `Session["RoleName"]` found exactly one file (`Bill_Software/index.aspx.cs`) containing these two tokens, and only as **write** statements (`Session["RoleId"] = user.RoleId;` / `Session["RoleName"] = ...;`). No other file in the repository reads either value.
- Database object: n/a (session-state only)
- Impact: These two session values are **dead state** from the perspective of the rest of the application — they are computed and stored but never consulted for any decision (display, authorization, or otherwise). The visible role name shown in the page header (`lblRole` in `Bill.Master.cs :: GetAdminName`) is sourced from a **fresh database query**, not from these session values.

### Finding: The Sales Visit workflow's own code (`visit_planner.aspx.cs`, `daily_rpt.aspx.cs`, `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs`) never references `RoleId`, `RoleName`, `UserRoles`, or `Roles` in any form

Status: **CONFIRMED**

Evidence:
- Targeted search for `RoleId|UserRoles|Permission` inside each of the four files individually returned **zero matches** in all four.
- Impact: Role resolution, in any of its two forms, plays **no part whatsoever** in the Sales Visit workflow's own logic. Whatever role(s) a user holds, the four Sales Visit pages behave identically for them (subject only to the `Session["USERID"]` login check and the ad hoc `CompanyID`/`CreatedByCode` filtering already documented in `04_Security_and_Tenant_Audit.md`).

---

## 5. Multi-Role Behavior

### Finding: `Bill.Master.cs :: GetMenuControl` is the only code in the repository that is multi-role-aware, and it merges (unions) permissions across all of a user's roles

Status: **CONFIRMED**

Evidence:
- File: `Bill_Software/corporate/business/app/Bill.Master.cs`
- Class: `Bill_Software.corporate.business.app.Bill`
- Method: `GetMenuControl`
- Database object: `dbo.Permissions.PermissionKey`, `dbo.RolePermissions` (`RoleId`, `PermissionId`), `dbo.UserRoles` (`UserId`, `RoleId`), `tbl_login.Id`, `.User_Id`
- Relevant behavior: The query is:
  ```sql
  SELECT DISTINCT p.PermissionKey
  FROM dbo.Permissions p
  INNER JOIN dbo.RolePermissions rp ON p.PermissionId = rp.PermissionId
  INNER JOIN dbo.UserRoles ur ON rp.RoleId = ur.RoleId
  INNER JOIN dbo.tbl_login u ON ur.UserId = u.Id
  WHERE u.User_Id = @UserId
  ```
  Because the join fans out across **every** row a user has in `UserRoles` (not just one), and the outer `SELECT DISTINCT` de-duplicates the resulting permission keys, **a user with multiple roles receives the union of all permissions granted to any of their roles.** No role "wins" over another; there is no precedence, override, or conflict-resolution logic — it is a pure set union.
- Impact: If the example from the task prompt (`User_Id = FLM035` with `UserRoles` entries for both `RoleId=4` "FLMX_Standard" and `RoleId=3` "Sales Officer") is accurate, this user's menu-visibility permission set would be the union of whatever `RolePermissions` rows exist for role `4` **plus** whatever exist for role `3`. **This cannot be verified against live data because database access is not available (§0)** — this describes the query's behavior, not a confirmed result for that specific user.
- Confidence: **CONFIRMED** for the mechanism (query logic, read directly from source); the specific outcome for `FLM035` is **NOT VERIFIED** (no DB access).

### Finding: This multi-role union behavior applies ONLY to menu-item visibility, not to any Sales Visit action

Status: **CONFIRMED**

Evidence: Same as §4's finding that none of the four Sales Visit files reference `Permission`/`UserRoles`/`RoleId` at all. `GetMenuControl`'s output (`userGrantedPermissions`) is used **only** to set `Control.Visible` on matching master-page menu `<li>` elements (`menuControl.Visible = userGrantedPermissions.Contains(menuId);`) — it is never passed to, or consulted by, any Sales Visit page or method.
- Impact: A user's multi-role permission union determines **only whether they see a navigation link**, not whether the underlying page/action will actually let them do something. Since none of the four Sales Visit `.aspx.cs` files perform any permission check of their own, a user who does **not** have the `visit_planner`/`srch_dailyrpts`/etc. permission key (and therefore does not see the menu link) can still reach and fully use the page **by navigating to its URL directly**, subject only to the `Session["USERID"]` login check described in §2. This is elaborated as a confirmed finding in §15 (IDOR/Object-Level Authorization).

### Finding: `tbl_login.RoleId` (single-valued) has no multi-role concept at all and is never merged with anything

Status: **CONFIRMED** (by absence — this is the natural consequence of `RoleId` being a scalar column with no fan-out join anywhere in the codebase).

---

## 6. Permission Resolution

### Finding: No reusable, callable permission-check helper method exists anywhere in the repository

Status: **CONFIRMED**

Evidence:
- Repository-wide search for the method-name patterns `HasPermission`, `CheckPermission`, `IsAuthorized`, `CanView`, `CanEdit`, `CanDelete`, `CanApprove`, and the general term `Authorize` found **zero matches in any `.cs`/`.aspx`/`.aspx.cs` file** in the entire repository. (The only matches for these terms anywhere in the workspace are inside this audit series' own previously-written documentation files under `docs/`, which are not application code.)
- Impact: There is no `PermissionService.HasPermission(userId, permissionKey)`-style method, static or instance, anywhere in the codebase that any page could call to gate an action. The task's instruction to "identify [an existing reusable mechanism] as the preferred integration point" and "not create a new one" cannot be fulfilled with a true permission-*check* method, because none exists.

### Finding: The closest analog to a "permission resolution" mechanism is the inline query inside `GetMenuControl`, which is purpose-built for menu rendering, not action gating

Status: **CONFIRMED**

Evidence: File/Class/Method as in §5's first finding.
- Relevant behavior: The query resolves "all permission keys granted to this user" as a `HashSet<string>`, then iterates all known permission keys (`SELECT PermissionKey FROM dbo.Permissions`) and looks for a same-named control on the master page. It never takes a single permission key as an input parameter to answer a yes/no question for a specific action — it always computes the *entire* granted set for the *entire* menu in one pass.
- Architecture interpretation: **If** a genuine `HasPermission(userId, permissionKey)` helper were to be introduced in the future, the query embedded in `GetMenuControl` (the `INNER JOIN` chain from `Permissions` → `RolePermissions` → `UserRoles` → `tbl_login`) is the correct, already-proven SQL shape to reuse/extract — it is presented here as the existing pattern to build on, per the task's instruction, without creating any new implementation in this task.

### Finding: `ManageRoles.aspx.cs` and `ManagePermissions.aspx.cs` are administrative CRUD screens for the RBAC tables, not permission-check call sites

Status: **CONFIRMED**

Evidence:
- File: `Bill_Software/corporate/business/app/ManageRoles.aspx.cs` — lets an admin create `Roles` rows and bulk-replace a role's `RolePermissions` rows (`DELETE FROM dbo.RolePermissions WHERE RoleId=@RoleId` then re-`INSERT`).
- File: `Bill_Software/corporate/business/app/ManagePermissions.aspx.cs` — lets an admin create/update/delete `Permissions` rows (columns: `PermissionId`, `PermissionKey`, `ModuleName`, `SubModuleName`, `FeatureName`, `Description`).
- Impact: These two files are the **editors** for the data that `GetMenuControl` later reads; they do not themselves check or enforce any permission (neither file gates its own admin actions with a permission check either — both rely solely on the same `Session["USERID"] == null` login check as every other page).

### Finding: Permission keys are checked by exact string match against control `ID` attributes in `Bill.Master`'s markup, not by any structured "action" concept

Status: **CONFIRMED**

Evidence:
- File: `Bill_Software/corporate/business/app/Bill.Master` (markup)
- Relevant behavior: The Sales Team menu section (lines ~408–416) contains:
  ```html
  <li id="SalesTeam" runat="server">Sales Team
      <ul class="nav first">
          <li id="visit_planner" runat="server"><a href="visit_planner.aspx">Visit Planner</a></li>
          <li id="daily_reporting" runat="server"><a href="daily_rpt.aspx">Daily Reporting</a></li>
          <li id="vw_dailyrpts" runat="server"><a href="vw_dailyrpts.aspx">View Daily Reporting</a></li>
          <li id="AdminApprovalDashboard" runat="server"><a href="AdminApprovalDashboard.aspx">Approvals</a></li>
          <li id="srch_dailyrpts" runat="server"><a href="srch_dailyrpts.aspx">Search Daily Reports</a></li>
      </ul>
  </li>
  ```
- Impact / correction of task-prompt assumption: The task asked to inspect permission keys `SalesTeam`, `DailyReports`, `SalesSubmit`, `SalesView`, `MgmntView`, `visit_planner`, `AdminApprovalDashboard`. Of these seven, only **`SalesTeam`**, **`visit_planner`**, and **`AdminApprovalDashboard`** were found as literal control IDs anywhere in the markup. The actual control IDs for the other two Sales-Visit-related menu items are **`daily_reporting`** (not `DailyReports`) and **`vw_dailyrpts`** / **`srch_dailyrpts`** (not `SalesSubmit`, `SalesView`, or `MgmntView`). No control with `id="DailyReports"`, `id="SalesSubmit"`, `id="SalesView"`, or `id="MgmntView"` exists anywhere in `Bill.Master`'s markup (confirmed by targeted search). It is possible these four values exist as **rows in the `dbo.Permissions` table** with no matching menu control (which would make them permanently ineffective, since `GetMenuControl` silently no-ops — `FindControlRecursive` returns `null` — for any permission key without a matching control ID), or they may not exist in the database at all; **this cannot be confirmed without database access (§0).**
- Confidence: Confirmed for what exists in source-controlled markup; **NOT VERIFIED** for what may or may not exist as rows in `dbo.Permissions` (no DB access).

---

## 7. Company / Tenant Resolution

### Finding: `CompanyID` is resolved from `Session["CompanyID"]` via a static helper class, `CompanyContext`, not from the authenticated user's own record, the database, or the request

Status: **CONFIRMED**

Evidence:
- File: `Bill_Software/corporate/business/app/Bill.Master.cs`
- Class: `Bill_Software.corporate.business.app.CompanyContext` (a `public static class` declared inside `Bill.Master.cs`, in the same namespace)
- Property: `CompanyContext.CurrentCompanyID` — `get { return HttpContext.Current.Session["CompanyID"] != null ? Convert.ToInt32(...) : 0; }`
- Database object: n/a directly (reads only `Session["CompanyID"]`); the session value itself is populated as described next.
- Relevant behavior: `Session["CompanyID"]` is set in exactly two places: (1) `Bill.Master.cs :: Page_Load`, which defaults it to the **first row** of a company dropdown (`BindCompanies()` → `SELECT ID, Name FROM tbl_Company WHERE IsActive = 1 OR IsActive IS NULL ORDER BY ID ASC`) if not already present in session; (2) `Bill.Master.cs :: ddlCompany_SelectedIndexChanged`, when the user manually switches companies via a dropdown in the master page header.
- Impact: `CompanyID` is **not derived from the authenticated user's own `tbl_login.CompanyID` column at all** in this resolution path — it is a **session-scoped, user-switchable UI selection**, defaulting to "the first active company in the `tbl_Company` table" on first login. This means the "current company" for authorization/filtering purposes is whatever the multi-company dropdown in the header currently has selected, not a fixed attribute of the logged-in identity.
- Architecture interpretation: This is a "multi-company workspace switcher" pattern (one login, multiple companies, switchable at will), **not** a tenant-per-login-session model where `CompanyID` is immutably tied to the user's own account. Whether this is the intended tenant-isolation model is a business-rule question — see §23. Separately, `tbl_login.CompanyID` **does exist** as a column (confirmed via `AddUser.aspx.cs`'s `INSERT INTO tbl_login (..., CompanyID) VALUES (..., @CompanyID)`, itself populated from `CompanyContext.CurrentCompanyID` at user-creation time) and is used to scope some queries (e.g., the salesperson dropdown in `srch_dailyrpts.aspx.cs`), but it is not what `CompanyContext.CurrentCompanyID` reads from at runtime — the two are related only in that a user's `tbl_login.CompanyID` was set, at creation time, to whatever `Session["CompanyID"]` happened to be selected by the admin who created them.

### Finding: `CompanyID` is applied inconsistently across Sales Visit `SELECT`/`INSERT`/`UPDATE` operations

Status: **CONFIRMED** (re-verified in this session against the same four files; no change from the prior audit's finding)

Evidence:
- **INSERT** — File: `daily_rpt.aspx.cs`, Method: `btnSubmit_Click` — the `INSERT INTO tbl_SalesVisitReport (...)` column list does **not** include `CompanyID`. Same omission in `visit_planner.aspx.cs :: btnSubmitExecution_Click`'s auto-follow-up `INSERT`.
- **SELECT (list)** — File: `srch_dailyrpts.aspx.cs`, Method: `Binder` — **does** filter `WHERE CompanyID = <companyId>` (companyId sourced from `CompanyContext.CurrentCompanyID`). File: `vw_dailyrpts.aspx.cs`, Method: `BindSalesVisits` — does **not** filter by `CompanyID` at all (filters by `CreatedByCode` only). File: `visit_planner.aspx.cs`, Method: `GetCalendarEvents` — does **not** filter by `CompanyID` (filters by `CreatedByCode` only).
- **SELECT (single-record detail)** — `visit_planner.aspx.cs :: GetVisitDetails`, `vw_dailyrpts.aspx.cs :: LoadMegaModal`, `srch_dailyrpts.aspx.cs :: LoadMegaModal` — **none** of these three filter by `CompanyID`; all use `WHERE Id=@Id` only.
- **UPDATE** — `visit_planner.aspx.cs :: btnSubmitExecution_Click`, `vw_dailyrpts.aspx.cs :: btnUpdateVisit_Click`, `srch_dailyrpts.aspx.cs :: ProcessApproval` — **none** filter by `CompanyID`.
- **APPROVAL** — `srch_dailyrpts.aspx.cs :: ProcessApproval` — no `CompanyID` predicate (see above; same statement).
- Database object: `tbl_SalesVisitReport.CompanyID`
- Impact: This is a widening, not a narrowing, of scope for every write and detail-view operation relative to the one list query (`srch_dailyrpts.aspx.cs :: Binder`) that does enforce it — meaning the enforcement that does exist is easily bypassed simply by acting on a specific `Id` rather than going through the filtered list.

### Finding: Because `CompanyID` is never populated on `INSERT`, the database-side default value of `1` (stated authoritatively in the task prompt) becomes the operative value for every row this workflow creates

Status: **CONFIRMED** (logical consequence of the above; the existence of a default value of `1` itself is taken as given per the task's authoritative database reference and was not independently verified against live DDL, since DB access is unavailable per §0)

Evidence: Same `INSERT` statements as above, combined with the task-provided fact that `tbl_SalesVisitReport.CompanyID` has a database default of `1`.
- Impact: Every visit created via `daily_rpt.aspx` or auto-generated via `visit_planner.aspx.cs`'s follow-up logic is **implicitly** filed under Company `1`, regardless of which company the creating user actually has selected via the multi-company dropdown (§ above) or belongs to per their own `tbl_login.CompanyID`. This means: for any company other than whichever one has `ID=1` in `tbl_Company`, the manager dashboard (`srch_dailyrpts.aspx`, which does filter `WHERE CompanyID=@CompanyID`) will **never show any Sales Visit created through this workflow**, because the created rows all silently default to Company `1` instead of the creator's actual company.
- Confidence: **CONFIRMED** for the code-side mechanism (no explicit supply of `CompanyID` ⇒ SQL Server applies the column default). The exact value `1` and its real-world consequence for non-`1` companies is stated per the task's authoritative reference and could not be independently re-verified live (§0).

---

## 8. Reporting Manager Resolution

### Finding: `ReportingManagerId` is read exclusively from `tbl_login`, is never stored in `Session`, and is used exclusively for e-mail routing — never for authorization or data-scope filtering

Status: **CONFIRMED**

Evidence:
- File: `vw_dailyrpts.aspx.cs`, Method: `SendChatEmailNotification` — `LEFT JOIN tbl_login Manager ON Manager.User_Id = Creator.ReportingManagerId` (resolves the visit creator's manager's e-mail address for a chat-reply notification).
- File: `srch_dailyrpts.aspx.cs`, Methods: `SendChatEmailNotification`, `SendApprovalNotification` — identical join pattern, same purpose.
- File: `AdminApprovalDashboard.aspx.cs`, Method: `ExecuteWorkflowTransaction` — `LEFT JOIN tbl_login m ON e.ReportingManagerId = m.User_Id` to fetch `@MgrEmail` for a CC on attendance/leave notifications (this is the **Attendance/Leave** feature, not Sales Visit — cited here only because it is the same column used the same way).
- Database object: `tbl_login.ReportingManagerId` (self-referencing FK → `tbl_login.User_Id`, per the task's authoritative schema reference)
- Impact: In **every single occurrence found in the codebase**, `ReportingManagerId` is used only inside a `SELECT`/`LEFT JOIN` whose sole purpose is to compute an e-mail address to send a notification to. **No `WHERE` clause anywhere in the repository filters records by `ReportingManagerId`**, and no code path checks "is the acting user the target record's `ReportingManagerId`" before allowing an action.
- Architecture interpretation: This directly answers the task's request to distinguish RBAC from data-scope filtering from organizational hierarchy: `ReportingManagerId` in this codebase is **purely organizational/display/routing metadata**. It is not an authorization mechanism, and it is not currently even a data-scope filter (it doesn't restrict *which* records a manager sees — that's done, inconsistently, by `CompanyID`/`CreatedByCode` alone, per §7). Any manager who reaches `srch_dailyrpts.aspx` can search, view, comment on, approve, and reject **any** Sales Visit belonging to **any** salesperson in the same `CompanyID` (or, per §7, potentially the same default `CompanyID=1` bucket regardless of true company), not only their own direct reports.

### Finding: Manager-hierarchy display (as opposed to enforcement) exists in the Users admin screen

Status: **CONFIRMED** (context, not part of the Sales Visit workflow itself)

Evidence: `ViewUser.aspx.cs` (`WebForm80`) selects `mgr.Name AS ManagerName` via `LEFT JOIN dbo.tbl_login mgr ON u.ReportingManagerId = mgr.User_Id` purely to display an employee's manager's name in the admin grid; `AddUser.aspx.cs` lets an admin assign `ReportingManagerId` when creating a user (`ddlManager` dropdown → `@ManagerId` parameter). Neither of these constitutes an authorization use.

---

## 9. Sales Visit Create Authorization

### Finding: Who can create a visit? — Any authenticated user; no permission or role check is performed

Status: **CONFIRMED**

Evidence:
- File: `daily_rpt.aspx.cs`
- Method: `Page_Load` (gate), `btnSubmit_Click` (create)
- Relevant behavior: `Page_Load` requires only `Session["USERID"] != null`. `btnSubmit_Click` performs no additional check of any kind (no `RoleId`, no `UserRoles`/`Permission` lookup, no `CompanyID` membership check against the acting user) before executing the `INSERT`.
- Database object: `tbl_SalesVisitReport` (all columns in the INSERT column list — see `02_Database_Dependency_Map.md` for the full list)
- Impact: Visit creation authorization = "is logged in." No RBAC integration point (`SalesTeam`/`visit_planner`/`daily_reporting` menu permission keys, or any `RoleId`/`Roles` value) is consulted at the point of actually performing the create action — those permission keys, per §6, only ever affect whether the **menu link** to reach this page is visible; the page and its postback handler themselves enforce nothing beyond login.
- Confidence: Confirmed by direct reading; this matches and is not contradicted by the prior audit turns.

### Finding: Field population sources

Status: **CONFIRMED**

Evidence (all from `daily_rpt.aspx.cs :: btnSubmit_Click` unless noted):
- `CreatedByCode` ← `HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03"` (hardcoded fallback string literal if session is somehow null at postback time — see prior audit finding D-03 in `05_Potential_Defects.md`, re-confirmed present in current source).
- `Salesperson` ← `txtSalesperson.Text` (a read-only textbox populated earlier, in `GetAdminName()`, via `select Name from tbl_login where User_Id='" + UserName + "'"` — **string-concatenated, not parameterized**; `UserName` is `Session["USERID"].ToString()`, i.e., server-controlled, not directly attacker-supplied, which reduces but does not eliminate risk under session-tampering scenarios).
- `CompanyID` ← **not populated at all** (see §7).
- `VisitPhase` ← hardcoded `"Planned"` (mode=`plan`) or `"Executed"` (mode=`past`), based on `Request.QueryString["mode"]`.
- `ApprovalStatus` ← **not present in the `INSERT` column list at all** — relies entirely on whatever default value the column has at the database level (stated by the task as part of the authoritative schema, not independently re-verified here since DB access is unavailable).
- `Status` ← hardcoded `"Pending Execution"` (mode=`plan`) or the user-selected `ddlStatus.SelectedValue` (mode=`past`, one of `Completed`/`Pending`/`Escalated`).
- Confidence: Confirmed by direct reading of `daily_rpt.aspx.cs`.

---

## 10. Sales Visit Execution Authorization

### Finding: Any authenticated user can execute (transition `Planned` → `Executed`) any visit, including another user's, because no ownership check exists on the `UPDATE`

Status: **CONFIRMED**

Evidence:
- File: `visit_planner.aspx.cs`
- Method: `btnSubmitExecution_Click`
- Relevant behavior: `UPDATE tbl_SalesVisitReport SET VisitPhase='Executed', ExecutionDateTime=GETDATE(), Latitude=@Latitude, Longitude=@Longitude, ... WHERE Id=@Id` — the `@Id` value comes directly from a hidden field (`hfExecuteVisitId.Value`) that the client-side JavaScript sets to whatever `event.id` was clicked on the calendar. There is **no** `AND CreatedByCode=@SessionUserId` (or any other ownership/company) predicate anywhere in this statement.
- Database object: `tbl_SalesVisitReport.Id`, `.VisitPhase`, `.ExecutionDateTime`, `.Latitude`, `.Longitude`, `.DiscussionPoints`, `.Status`, `.FollowUpRequired`, `.NextFollowUpDate`, `.AttachmentName`
- Impact: Because the calendar itself only ever renders the current user's **own** events (`GetCalendarEvents` filters `WHERE CreatedByCode = @UserId`), the normal UI flow never presents another user's visit ID to execute. However, the server-side `btnSubmitExecution_Click` handler itself performs **no independent verification** — a forged postback carrying a different, guessable/enumerable `Id` value in the hidden field would succeed against the database exactly as if it were the user's own visit.
- Confidence: **CONFIRMED** for the missing check (directly observable); the practical exploitability depends on standard ASP.NET WebForms postback/ViewState protections not specific to this feature (not independently assessed here, as it is a platform-wide concern, not a Sales-Visit-specific control).

### Finding: `ExecutionDateTime`, `Latitude`, `Longitude`, `VisitEndDate` population

Status: **CONFIRMED**

Evidence: `visit_planner.aspx.cs :: btnSubmitExecution_Click` — `ExecutionDateTime = GETDATE()` (server time, at the moment of the UPDATE); `Latitude`/`Longitude` ← `Convert.ToDecimal(hfLatitude.Value)`/`Convert.ToDecimal(hfLongitude.Value)`, themselves populated client-side by `navigator.geolocation.getCurrentPosition` in `captureLocationAndSubmit()` (JavaScript, in `visit_planner.aspx`); `VisitEndDate` is **not** modified by this method at all (it retains whatever value was set at creation time in `daily_rpt.aspx.cs`).

### Finding: `GeoLocationAddress` is never populated anywhere in the inspected codebase

Status: **NOT FOUND**

Evidence: Repository-wide search for `GeoLocationAddress` found it referenced **only** as a `SELECT`-list column in `AdminAttendanceDashboard.aspx.cs` (read/reporting context) — no `INSERT` or `UPDATE` statement anywhere in the repository writes to this column.
- Database object: `tbl_SalesVisitReport.GeoLocationAddress`
- Impact: This column (present in the task's authoritative schema) has **no discovered write path** in this codebase. It may be populated by a process outside this repository (e.g., a mobile app, an external integration, or a manual/administrative process not present here), or it may be an unimplemented/planned field. This audit does not speculate which; it reports only that no write path was found in the inspected source.

### Finding: `IsProductive`, `LinkedQuotationNo`, `RevenueRealized` also have no discovered write path

Status: **NOT FOUND** (all three)

Evidence: Repository-wide search for each of `IsProductive`, `LinkedQuotationNo`, `RevenueRealized` found them referenced only in `SELECT`/aggregate (`SUM`, `CAST`) contexts within `home.aspx.cs` and `AdminAttendanceDashboard.aspx.cs` — no `INSERT`/`UPDATE` statement anywhere in the repository writes any of the three. Notably, `Create_quotation.aspx.cs` (930 lines, the natural place one would expect `LinkedQuotationNo` to be written back onto the source visit after a quote is generated) contains **no reference to `LinkedQuotationNo` at all** — it only reads `CustomerName` from the visit (`PreFillClientFromVisit`) and does not write anything back to `tbl_SalesVisitReport`.
- Impact: Same as `GeoLocationAddress` above — these are read/aggregated by dashboards but appear to be write-orphaned within this codebase.

---

## 11. Sales Visit Submission Authorization

The task asked to trace a permission/flow named **`SalesSubmit`**.

### Finding: No page, method, permission key, or control named/keyed `SalesSubmit` exists anywhere in the repository

Status: **NOT FOUND**

Evidence: Repository-wide search for the literal string `SalesSubmit` returned zero matches in any file (source, markup, or configuration).
- Impact: There is no dedicated "submission" step distinct from the create (`daily_rpt.aspx`) and execute (`visit_planner.aspx`) flows already documented in §9/§10. If `SalesSubmit` is intended to refer to one of those two existing flows under a different name, or to a not-yet-built feature, that cannot be determined from source code alone — this is carried forward as an open question in §23.
- What *does* exist, closest in spirit: the `btnSubmit` button/`btnSubmit_Click` handler on `daily_rpt.aspx` (documented fully in §9) is the only "submit"-named artifact related to Sales Visit creation.

---

## 12. Sales Visit View / Search Authorization

### Finding: A normal user sees only their own visits; a manager (on `srch_dailyrpts.aspx`) sees all visits in the resolved `CompanyID`; there is no distinct "admin" view

Status: **CONFIRMED**

Evidence:
- File: `visit_planner.aspx.cs`, Method: `GetCalendarEvents` — `WHERE CreatedByCode = @UserId` (own records only; no role/permission gate on who may call this PageMethod beyond `EnableSession=true` + the ambient session check).
- File: `vw_dailyrpts.aspx.cs`, Method: `BindSalesVisits` — `WHERE CreatedByCode = @CreatedByCode` (own records only).
- File: `srch_dailyrpts.aspx.cs`, Method: `Binder` — `WHERE CompanyID = <companyId> [AND CreatedByCode = '<selectedUser>'] [AND VisitDate BETWEEN ...]` — **all** visits in the resolved company (optionally further filtered by a chosen salesperson/date range), regardless of who created them, and regardless of any `ReportingManagerId` relationship (per §8).
- Database object: `tbl_SalesVisitReport` (`CreatedByCode`, `CompanyID`, `VisitDate`)
- Impact: There is no code-level distinction between "manager" and "admin" for this page — **any** authenticated user who can reach `srch_dailyrpts.aspx` (which, per §6, is gated only by whether the `srch_dailyrpts` menu-permission key happens to be granted to *hide the link*, not by any server-side role check on the page itself) can search all visits in the resolved company. Nothing in `srch_dailyrpts.aspx.cs`'s `Page_Load` or `Binder` checks the acting user's role, permission, or `ReportingManagerId` before returning results.
- Confidence: Confirmed by direct reading of all three `Page_Load`/`Binder`/`BindSalesVisits`/`GetCalendarEvents` methods.

### Finding: Queries that retrieve Sales Visit records with insufficient scope predicates

Status: **CONFIRMED** (list below; re-verified in this session, unchanged from prior audit)

| Query | File / Method | Scope predicate present |
|---|---|---|
| `GetVisitDetails` | `visit_planner.aspx.cs` | `Id` only — no `CreatedByCode`, no `CompanyID` |
| `LoadMegaModal` | `vw_dailyrpts.aspx.cs` | `Id` only |
| `LoadMegaModal` | `srch_dailyrpts.aspx.cs` | `Id` only (despite the *list* query one level up being `CompanyID`-scoped) |

Database object: `tbl_SalesVisitReport` (all columns, via `SELECT v.*` / `SELECT *` in two of the three).

---

## 13. Sales Visit Edit Authorization

### Finding: Who can edit? Ownership is checked only in the UI/data-source sense, not in the `UPDATE`'s `WHERE` clause

Status: **CONFIRMED**

Evidence:
- File: `vw_dailyrpts.aspx.cs`
- Method: `btnUpdateVisit_Click`
- Relevant behavior:
  ```sql
  UPDATE tbl_SalesVisitReport
  SET VisitDate=@VisitDate, CustomerName=@CustomerName, ...
  WHERE Id=@Id
    AND ApprovalStatus='Pending'
    AND NOT EXISTS (SELECT 1 FROM tbl_SalesVisitResponses WHERE VisitId=Id AND RespondentRole='Manager')
  ```
- Database object: `tbl_SalesVisitReport.ApprovalStatus`, `.Id`; `tbl_SalesVisitResponses.VisitId`, `.RespondentRole`
- Impact: This `WHERE` clause **does** re-verify two of the three UI-computed edit-lock conditions (see next finding) at the database level, which is good defense-in-depth for those two conditions — but it contains **no `CreatedByCode=@SessionUserId` predicate**, meaning it does not verify that the visit being updated belongs to the user submitting the postback. Combined with the 45-day-age gap below, this is a genuine authorization gap, not merely a UI-only one.

### Finding: Re-verification of the "Pending" and "no-manager-comment" lock conditions is confirmed present; the 45-day age lock is confirmed NOT re-verified server-side

Status: **CONFIRMED** (explicitly re-verified in this session per the task's instruction not to assume the prior finding)

Evidence:
- File: `vw_dailyrpts.aspx.cs`, Method: `LoadMegaModal` (UI-side computation): three conditions computed in order — `ApprovalStatus != "Pending"` → locked; `(DateTime.Now - VisitDate).TotalDays > 45` → locked; a `tbl_SalesVisitResponses` row exists with `RespondentRole='Manager'` for this visit → locked. Sets `pnlEditForm.Enabled`/`btnUpdateVisit.Visible` accordingly.
- File: `vw_dailyrpts.aspx.cs`, Method: `btnUpdateVisit_Click` (server-side re-check, quoted above): re-checks condition 1 (`ApprovalStatus='Pending'`) and condition 3 (`NOT EXISTS (... RespondentRole='Manager')`) directly in the `UPDATE`'s `WHERE` clause. **Condition 2 (45-day age) does not appear anywhere in this `WHERE` clause, nor is it checked in any other line of `btnUpdateVisit_Click`.**
- Database object: `tbl_SalesVisitReport.VisitDate`, `.ApprovalStatus`
- Impact: A postback that bypasses the disabled UI (e.g., via direct form manipulation) targeting a visit older than 45 days but still `ApprovalStatus='Pending'` and with no manager comment would **succeed** at the database level, because nothing in the actual `UPDATE` statement enforces the age rule.
- Confidence: **CONFIRMED**, independently re-derived in this session by re-reading both methods side by side (not merely carried over from the prior turn's conclusion).

### Finding: `"Pending"` vs. `"Pending Execution"` — explicitly re-verified, not assumed

Status: **CONFIRMED** (re-verified with fresh line-level evidence in this session, per the task's explicit instruction to verify rather than assume this)

Evidence:
- File: `vw_dailyrpts.aspx`, lines 125–130 (`ddlSearchStatus`) and lines 214–219 (`edit_ddlStatus`): both dropdowns' `<asp:ListItem>` options are exactly `-- (placeholder) --` / `Completed` / **`Pending Execution`** / `Escalated`. There is **no** list item with the bare value `Pending` in either dropdown.
- File: `daily_rpt.aspx`, lines 161–166 (`ddlStatus`): options are `-- Select Status --` / `Completed` / **`Pending`** / `Escalated`.
- File: `visit_planner.aspx`, lines 316–320 (`ddlExecStatus`): options are `Completed` / **`Pending`** / `Escalated` (no placeholder item).
- File: `daily_rpt.aspx.cs`, `btnSubmit_Click` (mode=`plan`): hardcodes `Status = "Pending Execution"` at creation.
- Database object: `tbl_SalesVisitReport.Status`
- Impact: A visit whose `Status` was set to the bare literal `"Pending"` (via either execution flow) has **no matching item** in `vw_dailyrpts.aspx`'s own `edit_ddlStatus` dropdown. The code that populates the edit form (`LoadMegaModal`: `if (edit_ddlStatus.Items.FindByValue(rdr["Status"].ToString()) != null) edit_ddlStatus.SelectedValue = ...`) silently no-ops in this case, leaving the dropdown on its blank default item. If the user then submits the edit form without touching the Status field, `btnUpdateVisit_Click`'s `UPDATE ... SET Status=@Status` (bound to `edit_ddlStatus.SelectedValue`, which is now `""`) overwrites the prior `"Pending"` value with an empty string.
- Confidence: **CONFIRMED**, re-verified against current source in this session (identical to the prior audit turn's finding — the previous conclusion is upheld, not merely repeated).

### Finding: Can an already-`Approved`/`Rejected` visit be edited? No — but only via the `WHERE`-clause re-check, not a distinct explicit check

Status: **CONFIRMED** — see the `ApprovalStatus='Pending'` predicate above; this doubles as the "is it still editable given its approval state" gate.

### Finding: Can one salesperson edit another salesperson's visit? Can a manager edit a visit (as opposed to approving/rejecting it)?

Status: **CONFIRMED** (salesperson-on-salesperson) / **NOT APPLICABLE** (manager edit)

Evidence: As established above, `btnUpdateVisit_Click`'s `WHERE` clause has no ownership predicate, so — subject to the `ApprovalStatus`/manager-comment guards — any authenticated user submitting this specific postback with a valid, still-`Pending`, not-yet-commented-on `Id` could update it, regardless of whose visit it is. Separately, `srch_dailyrpts.aspx` (the manager-facing page) contains **no** "edit visit details" feature at all — its only mutating actions are Approve/Reject (visit-level) and Approve/Reject (expense-level) and posting a chat message; there is no manager-side equivalent of `btnUpdateVisit_Click`. "Can a manager edit the visit" is therefore **Not Applicable** — the capability does not exist for the manager role at all, in either a permitted or forbidden form.

---

## 14. Sales Visit Approval Authorization

### Finding: `AdminApprovalDashboard.aspx` is NOT the Sales Visit approval dashboard — it handles a completely separate feature (Attendance Regularization + Leave Requests)

Status: **CONFIRMED** — this corrects an assumption embedded in the task prompt

Evidence:
- File: `AdminApprovalDashboard.aspx.cs`
- Class: `Bill_Software.corporate.business.app.AdminApprovalDashboard`
- Methods: `LoadPendingRegularizations` (queries `tbl_AttendanceRegularization`), `LoadPendingLeaves` (queries `tbl_LeaveRequests`, `tbl_LeaveMaster`), `gvRegularizations_RowCommand`/`gvLeaves_RowCommand` → `ExecuteWorkflowTransaction` (a single shared method parameterized by `reqType` of `"Reg"` or `"Leave"`)
- Database objects touched: `tbl_AttendanceRegularization`, `tbl_LeaveRequests`, `tbl_LeaveMaster`, `tbl_EmployeeLeaveBalance`, `tbl_Attendance`, `tbl_SystemNotification`, `tbl_login` (for e-mail/contact lookup)
- **`tbl_SalesVisitReport`, `tbl_SalesVisitResponses`, and `tbl_Expenses` are never referenced anywhere in `AdminApprovalDashboard.aspx.cs` or `AdminApprovalDashboard.aspx`.**
- Impact: The task's Trace 12 instructions ("Start from `AdminApprovalDashboard`... What happens on Approve?... What happens to `ApprovalStatus`?") do not apply to this file for Sales Visit purposes — those `tbl_SalesVisitReport` columns are never touched here. **The actual Sales Visit approval dashboard is `srch_dailyrpts.aspx`**, as established in the prior audit turns and re-confirmed in this session.

### Finding (redirected to the correct file): Who can access the Sales Visit approval capability, and what authorization model governs it?

Status: **CONFIRMED**

Evidence:
- File: `srch_dailyrpts.aspx.cs`
- Methods: `Page_Load` (access gate — `Session["USERID"] == null` only, identical to every other page in this workflow, no role/permission check); `btnMegaApprove_Click`/`btnMegaReject_Click` → `ProcessApproval(string status)`
- Relevant behavior:
  ```sql
  UPDATE tbl_SalesVisitReport
  SET ApprovalStatus = @Status, ManagerRemarks = @Remarks, ApprovedDate = GETDATE(), ApprovedBy = @User
  WHERE Id = @Id
  ```
- Database object: `tbl_SalesVisitReport.ApprovalStatus`, `.ManagerRemarks`, `.ApprovedDate`, `.ApprovedBy`
- Impact: There is **no** `CompanyID` predicate, **no** `ReportingManagerId` relationship check, and **no** `ApprovalStatus='Pending'` predicate (i.e., no re-verification that the visit is still awaiting approval) in this `UPDATE`'s `WHERE` clause — it is `WHERE Id=@Id` alone. Combined with the page-level access gate being "logged in" only:
  - **RBAC only?** No — no role/permission check exists.
  - **RBAC + manager relationship?** No — `ReportingManagerId` is not consulted (§8).
  - **RBAC + CompanyID?** Partially and inconsistently — the *list* that surfaces visits to the manager (`Binder`) is `CompanyID`-scoped, but the actual approval `UPDATE` itself is not; a forged postback carrying an `Id` from a different company would still succeed.
  - **Actual model, as implemented:** "any authenticated user, acting on any `Id` they can supply, regardless of company, role, or reporting relationship, and regardless of whether the visit is still pending."
- Confidence: Confirmed by direct reading; this matches and extends the prior audit's D-04/D-08 findings with the specific "which of the four candidate models" framing the current task requested.

### Finding: Can arbitrary `VisitId` values be submitted to the approval action?

Status: **CONFIRMED — Yes**

Evidence: `hfMegaVisitId` (a `HiddenField`) is set from `e.CommandArgument` in `DataList2_ItemCommand` when a row's "View Complete File" button is clicked, and is then read directly by `ProcessApproval` (`string visitId = hfMegaVisitId.Value;`) with `Convert.ToInt32`-free direct parameter binding (`cmd.Parameters.AddWithValue("@Id", visitId)` — note `visitId` is passed as a `string`, relying on implicit/database-side conversion). No server-side check exists that `visitId` corresponds to a row that was actually part of the most recently rendered, `CompanyID`-filtered `DataList2` result set for this session.

---

## 15. Object-Level Authorization / IDOR Audit

Per-endpoint findings, using the requested `CONFIRMED / PROBABLE / NOT FOUND / NOT APPLICABLE` status values. "Verifies A AND B AND C AND D" means: (authenticated) AND (CompanyID) AND (ownership/manager relationship) AND (required permission).

| Endpoint | File :: Method | Authenticated? | CompanyID checked? | Ownership/manager checked? | Permission checked? | Overall Status |
|---|---|:---:|:---:|:---:|:---:|---|
| View (own calendar detail) | `visit_planner.aspx.cs :: GetVisitDetails` | ✅ (session) | ❌ | ❌ | ❌ | **CONFIRMED** IDOR — any logged-in user, any `visitId` |
| View (own visit list) | `visit_planner.aspx.cs :: GetCalendarEvents` | ✅ | ❌ | ✅ (`CreatedByCode=@UserId`) | ❌ | **NOT APPLICABLE** for IDOR — list is self-scoped by design; no permission gate exists but none is architecturally required for "my own data" |
| View (own file, mega modal) | `vw_dailyrpts.aspx.cs :: LoadMegaModal` | ✅ | ❌ | ❌ | ❌ | **CONFIRMED** IDOR |
| View (manager file, mega modal) | `srch_dailyrpts.aspx.cs :: LoadMegaModal` | ✅ | ❌ | ❌ | ❌ | **CONFIRMED** IDOR (despite the originating list being `CompanyID`-scoped) |
| Edit | `vw_dailyrpts.aspx.cs :: btnUpdateVisit_Click` | ✅ | ❌ | ❌ (only business-state guards, not ownership) | ❌ | **CONFIRMED** IDOR |
| Delete (visit) | *(no such endpoint exists)* | n/a | n/a | n/a | n/a | **NOT APPLICABLE** — no delete capability for `tbl_SalesVisitReport`, `tbl_Expenses`, or `tbl_SalesVisitResponses` was found anywhere in the repository (confirmed by search for `DELETE FROM tbl_SalesVisitReport`/`tbl_Expenses`/`tbl_SalesVisitResponses` — zero matches) |
| Execute | `visit_planner.aspx.cs :: btnSubmitExecution_Click` | ✅ | ❌ | ❌ | ❌ | **CONFIRMED** IDOR |
| Approve | `srch_dailyrpts.aspx.cs :: ProcessApproval("Approved")` | ✅ | ❌ | ❌ | ❌ | **CONFIRMED** IDOR |
| Reject | `srch_dailyrpts.aspx.cs :: ProcessApproval("Rejected")` | ✅ | ❌ | ❌ | ❌ | **CONFIRMED** IDOR |
| Reply (chat, salesperson side) | `vw_dailyrpts.aspx.cs :: btnMegaSendChat_Click` | ✅ | ❌ | ❌ (does not verify the visit belongs to the poster) | ❌ | **CONFIRMED** IDOR |
| Reply (chat, manager side) | `srch_dailyrpts.aspx.cs :: btnMegaSendChat_Click` + `GetUserRole` | ✅ | ❌ | **PARTIAL** — `GetUserRole` checks if the sender equals `CreatedByCode` to label them "Salesperson"; otherwise defaults to labeling them "Manager" with **no verification they are an actual manager or the correct one** | ❌ | **PROBABLE** — role *labeling* has a fallback-to-Manager default that is not itself a security boundary being bypassed (the insert succeeds regardless of the computed label), but it does mean the chat *attribution* can be inaccurate for any third-party user; the insert itself has no ownership/company gate |
| Follow-up (auto-generation) | `visit_planner.aspx.cs :: btnSubmitExecution_Click` (embedded `INSERT`) | ✅ (inherits the caller's already-unchecked execute action) | ❌ (`CompanyID` not copied — see §7) | n/a — it is a system-generated side effect of the (unchecked) Execute action, not a separately user-invoked endpoint | n/a | **NOT APPLICABLE** as a standalone IDOR target (there is no independent `ParentVisitId`-accepting endpoint a user calls directly); its authorization posture is entirely inherited from the Execute finding above |
| Attachment (view visit attachment) | Direct static link `~/Uploads/<file>` (referenced from `visit_planner.aspx`, `vw_dailyrpts.aspx`, `srch_dailyrpts.aspx`, and outbound e-mail bodies) | ❌ — served as a static file with no session check at all | ❌ | ❌ | ❌ | **CONFIRMED** — not IDOR in the "guess another database Id" sense, but a more severe **unauthenticated file disclosure**: no login is required at all to retrieve a file if its name is known/guessed |
| Attachment (view expense attachment) | Direct static link `~/Uploads/Expenses/<file>` (`srch_dailyrpts.aspx`) | ❌ | ❌ | ❌ | ❌ | **CONFIRMED** — same as above |
| Expense Approve/Reject | `srch_dailyrpts.aspx.cs :: gvMegaExpenses_RowCommand` | ✅ | ❌ | ❌ | ❌ | **CONFIRMED** IDOR |

---

## 16. SQL Injection Review

### Finding: `srch_dailyrpts.aspx.cs :: Binder()` constructs its entire query via raw string concatenation

Status: **CONFIRMED**

Evidence:
- File: `srch_dailyrpts.aspx.cs`
- Method: `Binder`
- Exact code (all three branches, lines 175/179/183):
  ```csharp
  cmdstring = "SELECT * FROM tbl_SalesVisitReport WHERE CompanyID = " + companyId + " AND CreatedByCode = '" + selectedUser + "' ORDER BY CAST(VisitDate as date) DESC";
  cmdstring = "SELECT * FROM tbl_SalesVisitReport WHERE CompanyID = " + companyId + " AND CAST(VisitDate as date) BETWEEN '" + fromDateStr + "' AND '" + toDateStr + "' ORDER BY CAST(VisitDate as date) DESC";
  cmdstring = "SELECT * FROM tbl_SalesVisitReport WHERE CompanyID = " + companyId + " AND CreatedByCode = '" + selectedUser + "' AND CAST(VisitDate as date) BETWEEN '" + fromDateStr + "' AND '" + toDateStr + "' ORDER BY CAST(VisitDate as date) DESC";
  ```
  followed by `SqlDataAdapter da = new SqlDataAdapter(cmdstring, conn);` — the string is executed as-is, with **no `SqlParameter` used anywhere in this method.**
- Database object: `tbl_SalesVisitReport`
- Inputs concatenated: `companyId` (an `int`, from `CompanyContext.CurrentCompanyID` — not directly attacker-suppliable through this variable's own type); `selectedUser` (from a server-rendered `DropDownList.SelectedValue`); `fromDateStr`/`toDateStr` (from free-text `TextBox` controls `txtfromDate`/`txttodate`, paired with a client-side jQuery datepicker but with **no server-side format/type validation** before concatenation).
- Impact: A textbook SQL injection vulnerability. `fromDateStr`/`toDateStr` are the most directly exploitable inputs (free text, no whitelist).
- Confidence: **CONFIRMED**, re-verified with exact current line numbers in this session.

### Finding: `daily_rpt.aspx.cs :: GetAdminName()` also uses string concatenation, but with a server-controlled (not directly attacker-supplied) input

Status: **CONFIRMED** (lower severity, as previously noted)

Evidence: `string cmdString = "select Name from tbl_login where User_Id='" + UserName + "'";` where `UserName = Session["USERID"].ToString();` — `UserName` originates from the server-set session value, not a request parameter, reducing direct exploitability absent a separate session-integrity issue.

### Finding: Every other Sales Visit query inspected (all methods in `visit_planner.aspx.cs`, `vw_dailyrpts.aspx.cs`, and the remainder of `srch_dailyrpts.aspx.cs`, plus `expense_entry.aspx.cs` and `Create_quotation.aspx.cs`'s visit-related query) uses `SqlParameter`/`AddWithValue` parameterization

Status: **CONFIRMED** (as the general pattern; the two findings above are the exceptions, not the rule)

Evidence: Every `SELECT`/`INSERT`/`UPDATE` shown in `01_Current_Process_Flow.md` and `02_Database_Dependency_Map.md` from the previous audit turns, re-spot-checked in this session, uses `cmd.Parameters.AddWithValue(...)`.

No stored procedures are used for any Sales Visit query (confirmed: no `CommandType.StoredProcedure` usage found anywhere in the four Sales Visit files or their direct dependents; the only stored-procedure call found anywhere near this feature area is `sp_AllocateEmployeeLeaves`, called from the unrelated `AddUser.aspx.cs` user-provisioning flow).

---

## 17. Attachment Authorization

(Consolidating and re-verifying the prior audit's findings for this task's specific structure.)

### Finding: Upload location and naming

Status: **CONFIRMED**

Evidence:
- Visit attachments: `Server.MapPath("~/Uploads/")`, filename = `yyyyMMddHHmmss_` + `Path.GetFileName(<original name>)` — present identically in `daily_rpt.aspx.cs :: btnSubmit_Click`, `visit_planner.aspx.cs :: btnSubmitExecution_Click`, `vw_dailyrpts.aspx.cs :: btnUpdateVisit_Click`.
- Expense attachments: `Server.MapPath("~/Uploads/Expenses/")`, filename = `"EXP_" + yyyyMMddHHmmss_` + `Path.GetFileName(...)` — `expense_entry.aspx.cs :: btnSubmit_Click`.
- Database object: `tbl_SalesVisitReport.AttachmentName`, `tbl_Expenses.AttachmentName` (filename string only; the binary file lives on disk, not in the database).

### Finding: Download mechanism has no authorization check

Status: **CONFIRMED**

Evidence: Every reference to an uploaded file (`visit_planner.aspx`'s view-modal JS: `"Uploads/" + details.AttachmentName`; `vw_dailyrpts.aspx.cs`'s `hlCurrentAttachment.NavigateUrl = "~/Uploads/" + ...`; `srch_dailyrpts.aspx.cs`'s `hlMegaAttachment.NavigateUrl`/`hlExpReceipt.NavigateUrl`; both `GetVisitEmailBody` implementations' hardcoded `https://www.exc.aagroupindia.com/Uploads/{AttachmentName}` link) is a **direct link to a static file path**. No `.ashx` handler, no code-behind download method, and no `web.config` location-specific authorization rule for the `Uploads/` or `Uploads/Expenses/` folders was found anywhere in the repository.
- Impact: File retrieval requires **no authentication of any kind** — it is governed entirely by IIS's default static-file serving for whatever is physically present under the web root.

### Finding: Path traversal protection

Status: **PROBABLE** (partial protection exists, but is incidental, not purpose-built)

Evidence: `Path.GetFileName(fileUpload.FileName)` is applied to the **client-supplied original filename** before it is used to build the saved path, in all four upload sites. `Path.GetFileName` strips any directory-separator-containing prefix, which incidentally prevents a classic `../../` path-traversal payload embedded in the original filename from escaping the `Uploads/` folder. However: (a) no allow-list/deny-list of file extensions exists, so a file with any extension (including a server-executable one, if IIS is configured to execute scripts from that folder — not verifiable from this codebase) can be saved; (b) no check exists for other unsafe filename characters (e.g., reserved Windows device names, alternate data stream syntax) beyond what `Path.GetFileName` itself happens to strip.
- Impact/Confidence: The specific `../` traversal vector is **probably** mitigated by `Path.GetFileName`'s behavior, but this is not a purpose-built security control and no broader file-safety validation exists.

### Finding: Company/user validation on either upload or retrieval

Status: **NOT FOUND**

Evidence: No upload site checks that the visit/expense being attached to belongs to the uploading user or their company before saving; no retrieval path checks anything at all (per the "download mechanism" finding above).

---

## 18. Database Dependency Map (Sales Visit-relevant objects actually found in code)

| Object | Type | Operation(s) Found | Calling File(s)/Method(s) | Purpose | Relevant Auth/Scope Fields |
|---|---|---|---|---|---|
| `tbl_SalesVisitReport` | Table | SELECT, INSERT, UPDATE (no DELETE found) | All four Sales Visit files, plus `expense_entry.aspx.cs`, `Create_quotation.aspx.cs`, `home.aspx.cs`, `AdminAttendanceDashboard.aspx.cs` | Core visit entity | `CreatedByCode`, `CompanyID` (inconsistently applied — §7); `ApprovalStatus`, `Id` (no ownership predicate on most single-row ops — §15) |
| `tbl_SalesVisitResponses` | Table | SELECT, INSERT (no UPDATE/DELETE found) | `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs` | Chat/comment thread | `VisitId`, `RespondentCode`, `RespondentRole` — none used as an authorization filter, only as data |
| `tbl_Expenses` | Table | SELECT, INSERT, UPDATE (no DELETE found) | `expense_entry.aspx.cs`, `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs` | Expense claims, optionally linked to a visit | `VisitId` (nullable), `UserCode`, `ApprovalStatus` — no `CompanyID` column referenced on this table anywhere in the inspected code |
| `tbl_login` | Table | SELECT, UPDATE (INSERT via `AddUser.aspx.cs`, out of this workflow's direct scope) | All four Sales Visit files (via joins for names/emails), `Bill.Master.cs`, `index.aspx.cs` | Identity/user directory | `User_Id`, `RoleId`, `CompanyID`, `ReportingManagerId`, `IsActive` |
| `Roles` | Table | SELECT (via `ManageRoles.aspx.cs`, `ViewUser.aspx.cs`, `Bill.Master.cs`); INSERT (`ManageRoles.aspx.cs :: btnCreateRole_Click`) | `Bill.Master.cs :: GetAdminName`, `ManageRoles.aspx.cs`, `ViewUser.aspx.cs`, `Update_Designation.aspx.cs`, `AddUser.aspx.cs` | Role catalog | `RoleId`, `RoleName` (`TenantId` column mentioned in the task's schema reference was **not found referenced anywhere in application code** — see §22) |
| `UserRoles` | Table | SELECT, DELETE, INSERT | `Bill.Master.cs :: GetMenuControl` (SELECT only); `Update_Designation.aspx.cs` (full DELETE+re-INSERT per save) | Many-to-many user↔role assignment | `UserId` (→ `tbl_login.Id`), `RoleId` |
| `RolePermissions` | Table | SELECT, DELETE, INSERT | `Bill.Master.cs :: GetMenuControl` (SELECT); `ManageRoles.aspx.cs` (full DELETE+re-INSERT per role save); `ManagePermissions.aspx.cs :: DeletePermission` (DELETE, cascading cleanup) | Role↔permission assignment | `RoleId`, `PermissionId` |
| `Permissions` | Table | SELECT, INSERT, UPDATE, DELETE | `Bill.Master.cs :: GetMenuControl` (SELECT); `ManagePermissions.aspx.cs` (full CRUD) | Permission catalog (`PermissionKey`, `ModuleName`, `SubModuleName`, `FeatureName`) | `PermissionId`, `PermissionKey` |
| `dbo.ActiveSessions` | Table | SELECT, UPDATE, INSERT | `index.aspx.cs`, `Bill.Master.cs` | Concurrent-session/session-token tracking | `SessionToken`, `UserId`, `IsActive` |
| `tbl_Company` | Table | SELECT | `Bill.Master.cs :: BindCompanies`, `LoadCompanyHeader` | Multi-company dropdown source and header display | `ID`, `Name`, `IsActive` — this is the ultimate source of the default `CompanyID` (§7) |
| **Views** | — | — | — | — | **NOT FOUND** — no SQL Server view (`sys.views`-style object) is referenced anywhere in the inspected Sales-Visit-adjacent code (all data access is against base tables) |
| **Stored Procedures** | — | `sp_AllocateEmployeeLeaves` | `AddUser.aspx.cs` (unrelated to Sales Visit — user-provisioning leave allocation) | — | **NOT FOUND** for anything Sales-Visit-related — zero stored procedures are used by any of the four Sales Visit files |
| **Functions** | — | — | — | — | **NOT FOUND** — no user-defined SQL function reference found anywhere in the inspected code |
| **Triggers** | — | — | — | — | **NOT FOUND** — no trigger is referenced, and none can be inferred from application code alone (their existence, if any, could only be confirmed via direct DDL/`sys.triggers` inspection, which was not possible — §0) |

---

## 19. Existing Reusable Security Infrastructure

### Finding: `CompanyContext` (static class) — reusable, and already the standard pattern for tenant resolution elsewhere in the app

Status: **CONFIRMED — should be the preferred integration point for any future `CompanyID` enforcement**

Evidence: `Bill_Software/corporate/business/app/Bill.Master.cs :: CompanyContext.CurrentCompanyID` (and `.CurrentCompanyCode`) — already used consistently by `srch_dailyrpts.aspx.cs`, `AddUser.aspx.cs`, `ViewUser.aspx.cs`, `Create_quotation.aspx.cs`, `AdminAttendanceDashboard.aspx.cs`, `New_vendor.aspx.cs`, and others. **Not currently used** by `daily_rpt.aspx.cs`, `visit_planner.aspx.cs`, or `vw_dailyrpts.aspx.cs` — this is the existing mechanism those three files would integrate with, per the task's "do not create a new one" instruction.

### Finding: `Bill.Master.cs :: GetMenuControl`'s permission-resolution query — the only existing RBAC query, but built for menu rendering

Status: **CONFIRMED** — see §6 for full detail; flagged again here as the pre-existing pattern to extract/reuse (not re-implement) if a genuine permission-check helper is ever introduced.

### Finding: `DB_UTILITY.cs` — a generic ADO.NET helper class with no authorization logic of any kind

Status: **CONFIRMED**

Evidence: `Bill_Software/DB_UTILITY.cs` contains only connection/command/dataset helper wrappers (`Sqlconnection`, `ConnectDb`, `ReturnDataTable`, `ExecuteNonQuery`, `FillCombo*`, etc.) — no method resembling a permission check, session check, or tenant filter exists in this class. It is a data-access convenience class, not a security helper.

### Finding: `CommunicationGateway.cs` — an existing, reusable, config-driven notification helper NOT used by the Sales Visit workflow

Status: **CONFIRMED**

Evidence: `Bill_Software/corporate/business/app/CommunicationGateway.cs`, static class, method `SendAlertsAsync(email, mobile, subject, message, ccEmail)` — reads SMTP settings correctly from `ConfigurationManager.AppSettings` (`SmtpFrom`/`SmtpUser`/`SmtpPass`/`SmtpHost`/`SmtpPort`/`SmtpEnableSsl`) and also supports MSG91 WhatsApp/SMS via `AppSettings["Msg91AuthKey"]`/`["Msg91IntegratedNumber"]`. Used by `AdminOverride.aspx.cs`, `AdminApprovalDashboard.aspx.cs`, `QuickAction.aspx.cs`, `attendance.aspx.cs`, `MyLeaves.aspx.cs` (all Attendance/Leave-domain files) as a fire-and-forget (`Task.Run`) notification dispatcher.
- Impact: This is a directly relevant, pre-existing, config-driven alternative to the hardcoded-SMTP-credential, duplicated `SendChatEmailNotification`/`SendApprovalNotification`/`GetVisitEmailBody` implementations found independently in `vw_dailyrpts.aspx.cs` and `srch_dailyrpts.aspx.cs` (documented in the prior audit's D-11/D-18). **`CommunicationGateway` is a stronger existing-infrastructure match for "do not create a new one" than anything previously identified**, and is the clearest concrete integration point if/when the Sales Visit workflow's notification code is ever revisited.
- Not currently used by any of: `visit_planner.aspx.cs`, `daily_rpt.aspx.cs`, `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs`.

### Finding: No `BasePage` class exists

Status: **NOT FOUND** — confirmed by repository-wide search for `BasePage`; the closest analog to shared cross-page logic is the Master Page's own `Page_Load` (§2), which is a different inheritance mechanism (composition via master page, not a common `Page` base class).

---

## 20. Confirmed Security / Architecture Findings

(Cross-referencing this session's independently re-verified findings to the prior audit's defect IDs where applicable, and adding the newly-surfaced findings from this RBAC-focused pass.)

1. **No reusable permission-check method exists anywhere in the codebase** (§6) — new finding this session.
2. **Two independent, unsynchronized role representations** (`tbl_login.RoleId` vs. `UserRoles`) exist, maintained by two separate, non-communicating admin screens (§4) — new finding this session.
3. **`Session["RoleId"]`/`Session["RoleName"]`/`Session["USERTYPE"]` are write-only/dead session state**, never read for any decision (§3, §4) — new finding this session.
4. **The RBAC permission system (`Permissions`/`RolePermissions`/`UserRoles`) governs only menu-link visibility, never server-side action authorization** (§5, §6) — new finding this session, though it explains and generalizes the prior audit's D-04.
5. **`AdminApprovalDashboard.aspx` is unrelated to Sales Visit approvals** (§14) — corrects a task-prompt assumption; new finding this session.
6. **`CompanyID` is never populated on any Sales Visit `INSERT`**, and the four candidate permission keys `DailyReports`/`SalesSubmit`/`SalesView`/`MgmntView` do not exist as menu control IDs (§6, §7) — re-confirms/extends prior audit D-01, adds new sub-finding on permission-key mismatch.
7. **Broken object-level authorization (IDOR)** across View/Edit/Execute/Approve/Reject/Reply/Expense-Approve endpoints (§15) — re-confirms and formally re-tabulates prior audit D-04, using this task's required CONFIRMED/PROBABLE/NOT FOUND/NOT APPLICABLE format.
8. **SQL injection** in `srch_dailyrpts.aspx.cs :: Binder()` (§16) — re-confirms prior audit D-05 with exact current line numbers.
9. **`"Pending"` vs. `"Pending Execution"` vocabulary mismatch** causing silent data loss on edit (§13) — explicitly re-verified per the task's instruction not to assume the prior finding; **upheld as accurate.**
10. **Unauthenticated attachment retrieval** for both visit and expense files (§17) — re-confirms prior audit D-09's disclosure angle.
11. **`ReportingManagerId` is a routing-only concept, never an authorization boundary**, confirmed with a complete enumeration of every usage site in the repository (§8) — re-confirms and generalizes prior audit D-16.
12. **Several `tbl_SalesVisitReport` columns from the task's authoritative schema (`GeoLocationAddress`, `IsProductive`, `LinkedQuotationNo`, `RevenueRealized`) have no discovered write path anywhere in this codebase** (§10) — new finding this session.
13. **`SalespersonReply`/`SalespersonReplyDate` columns (per the task's schema reference) are not referenced anywhere in the inspected code** — the application uses `tbl_SalesVisitResponses` for all reply/chat functionality instead (§22) — new finding this session.
14. **`CommunicationGateway.cs` is a superior, existing, config-driven notification helper that the Sales Visit workflow does not use**, in favor of hardcoded-credential, duplicated inline SMTP code (§19) — new finding this session, strengthens prior audit D-11/D-18's remediation recommendation.

---

## 21. Probable Findings Requiring Validation

1. **Path-traversal protection via `Path.GetFileName`** (§17) is *probably* sufficient for the specific `../` vector but is incidental, not purpose-built, and does not address extension/content-type risks — requires a decision on acceptable file types, not further code archaeology.
2. **Chat "role" mislabeling for a third-party sender** (§15, `GetUserRole`'s default-to-`"Manager"` fallback) — the practical impact depends on whether any user outside the visit's creator and its intended manager can realistically reach the chat-send action, which in turn depends on the (already-confirmed) absence of a `ReportingManagerId`/`CompanyID` gate on that action — the *labeling* issue is probable/cosmetic on its own, but rides on top of a confirmed access-control gap.
3. **Whether `dbo.Permissions` contains rows for `DailyReports`, `SalesSubmit`, `SalesView`, or `MgmntView`** with no matching control ID (§6) — cannot be confirmed without database access (§0); if such rows exist, they are dead/no-op permissions from the application's perspective.
4. **Whether the specific example user (`FLM035`) actually holds the two stated `UserRoles` rows and what the resulting merged permission set evaluates to** (§5) — the query mechanism is confirmed; the specific data outcome is not verifiable without database access.
5. **Whether any `CHECK` constraint, trigger, or additional index exists on any of the four core tables** that would materially change the risk profile of any finding above (e.g., a trigger that independently enforces `CompanyID`) — not discoverable from application code alone (§0, §18).

---

## 22. Not Found / Not Applicable

- **`SalesSubmit`** — no page, method, control, or permission key with this exact name exists anywhere in the repository (§11). **NOT FOUND.**
- **`DailyReports`, `SalesView`, `MgmntView`** — none exist as control IDs in `Bill.Master`'s markup; their existence as `dbo.Permissions` rows is unconfirmed (§6, §21). **NOT FOUND** (in source/markup); unverified in database.
- **`tbl_login.TenantId`** — not referenced anywhere in application code; every tenant-scoping operation in the codebase uses `CompanyID`/`CompanyContext`/`Session["CompanyID"]` instead. A local C# variable named `currentTenantId` exists in `New_vendor.aspx.cs`, but it is assigned directly from `CompanyContext.CurrentCompanyID`, i.e., it is a naming choice for a `CompanyID` value, not a reference to a distinct `TenantId` column. **NOT FOUND** as an actual referenced database column anywhere in code.
- **`Roles.TenantId`** — not referenced anywhere in application code (only `RoleId`/`RoleName` are ever selected from `Roles`). **NOT FOUND.**
- **A "Delete" capability for any Sales Visit, Expense, or Response record** — no `DELETE` statement targeting any of these three tables exists anywhere in the repository. **NOT APPLICABLE** for the IDOR audit (§15) — there is nothing to authorize because the feature does not exist.
- **A `BasePage` class or any common `Page`-inheritance-based authorization mechanism** — **NOT FOUND** (§19); the closest equivalent is the Master Page's own `Page_Load`.
- **A reusable `HasPermission`/`CheckPermission`/`IsAuthorized`/`CanView`/`CanEdit`/`CanDelete`/`CanApprove` method** — **NOT FOUND** anywhere in the codebase (§6).
- **`SalespersonReply` / `SalespersonReplyDate` columns** (per the task's schema reference) — **NOT FOUND** referenced anywhere in application code; the live chat/reply feature is fully implemented via `tbl_SalesVisitResponses` instead.
- **Forms Authentication (`System.Web.Security.FormsAuthentication`)** — **NOT FOUND**; authentication is fully custom (§2).
- **Any stored procedure, view, function, or trigger touching `tbl_SalesVisitReport`, `tbl_SalesVisitResponses`, or `tbl_Expenses`** — **NOT FOUND** in application code (§18); cannot be ruled out at the database level without direct DDL access (§0).

---

## 23. Business Rules Still Requiring Confirmation

(New items surfaced specifically by this RBAC-focused inspection, in addition to — not replacing — the 10 items already catalogued in `06_Business_Rules_Requiring_Confirmation.md`.)

1. **Which role representation is authoritative — `tbl_login.RoleId` or `UserRoles`?** Both are live, independently-editable data stores (§4). If a future authorization model is built, a decision is needed on whether to retire one, reconcile them, or intentionally keep them serving different purposes (e.g., `RoleId` for display/legacy, `UserRoles` for actual permissioning).
2. **Is the multi-company "workspace switcher" model (`Session["CompanyID"]`, user-changeable via a header dropdown, defaulting to the first active company) the intended tenant-isolation design, or should `CompanyID` instead be immutably tied to the authenticated user's own `tbl_login.CompanyID`?** (§7) This materially changes what "tenant isolation" should even mean for this application before any enforcement work is planned.
3. **What should happen for companies other than the one holding `CompanyID=1`, given that every Sales-Visit `INSERT` currently relies on that column's database default?** (§7) Is silent mis-filing under Company 1 an acceptable historical gap to leave as-is (with enforcement only going forward), or does it require investigation into how many existing rows are affected?
4. **What were `DailyReports`, `SalesSubmit`, `SalesView`, and `MgmntView` intended to be?** (§6, §22) Are they planned-but-unbuilt permission keys, renamed artifacts of the current `daily_reporting`/`vw_dailyrpts`/`srch_dailyrpts` control IDs, or references to a different, not-yet-discovered part of the system?
5. **Are `GeoLocationAddress`, `IsProductive`, `LinkedQuotationNo`, and `RevenueRealized` intended to be populated by a component outside this repository (e.g., a mobile app, a separate integration, or manual back-office entry), or are they incomplete/abandoned features?** (§10) This affects whether their absence of a write path in this codebase is expected or a gap.
6. **Are `SalespersonReply`/`SalespersonReplyDate` (per the task's schema reference) legacy columns superseded by `tbl_SalesVisitResponses`, or are they intended to be used by some other, not-yet-identified part of the system?** (§22)
7. **Should Sales Visit approval authorization be restricted to a salesperson's actual `ReportingManagerId`, restricted only by `CompanyID`, or intentionally open to any authenticated user who can reach the manager dashboard?** (§14) This is the same open question as item 8 in `06_Business_Rules_Requiring_Confirmation.md`, restated here with the additional confirmation that **no RBAC permission of any kind currently gates this action either** — it is not just a `ReportingManagerId` question but also a "should *any* permission be required to reach `ProcessApproval` at all" question.
8. **Which database is authoritative for future validation work — `flamex_uat` (named in this task) or `flamex_live` (the one actually wired into the live `Web.config` connection string, per §0)?** This is a purely observational discrepancy noted during inspection, not resolved here.

---

## 24. Recommended Architecture — NO IMPLEMENTATION

**This section is descriptive only. Nothing below has been built, scaffolded, or scheduled. It restates, in light of this session's new RBAC-specific findings, which of the existing mechanisms documented in `07_Proposed_Target_Architecture.md` should be the integration points if/when remediation work is authorized separately.**

- Any future action-level permission check should be built by **extracting and generalizing the exact join pattern already proven in `Bill.Master.cs :: GetMenuControl`** (`Permissions` → `RolePermissions` → `UserRoles` → `tbl_login`), parameterized by a single `PermissionKey` and returning a boolean, rather than inventing a new schema or query shape. This directly reuses existing, already-populated tables rather than introducing new ones.
- Any future tenant-scoping enforcement should continue to route through **`CompanyContext.CurrentCompanyID`**, the already-established pattern used by `srch_dailyrpts.aspx.cs`, `AddUser.aspx.cs`, `ViewUser.aspx.cs`, `Create_quotation.aspx.cs`, and others — not a new mechanism.
- Any future notification/e-mail work for the Sales Visit workflow should route through **`CommunicationGateway.SendAlertsAsync`**, the already-established, config-driven, non-hardcoded-credential pattern used by the Attendance/Leave domain — not the duplicated, hardcoded-credential `SendChatEmailNotification`/`SendApprovalNotification` methods currently embedded in `vw_dailyrpts.aspx.cs`/`srch_dailyrpts.aspx.cs`.
- Whether to reconcile or retire one of the two role representations (`RoleId` vs. `UserRoles`) is a **business decision** (§23, item 1), not a technical one to be resolved by this document.
- **No code, configuration, or database change is proposed, scaffolded, or scheduled by this section.** This is a restatement of "where existing infrastructure already is," for a future, separately-authorized implementation effort to consult.

---

## 25. Files Inspected

**Sales Visit workflow (primary scope):**
- `Bill_Software/corporate/business/app/visit_planner.aspx`
- `Bill_Software/corporate/business/app/visit_planner.aspx.cs`
- `Bill_Software/corporate/business/app/daily_rpt.aspx`
- `Bill_Software/corporate/business/app/daily_rpt.aspx.cs`
- `Bill_Software/corporate/business/app/vw_dailyrpts.aspx`
- `Bill_Software/corporate/business/app/vw_dailyrpts.aspx.cs`
- `Bill_Software/corporate/business/app/srch_dailyrpts.aspx`
- `Bill_Software/corporate/business/app/srch_dailyrpts.aspx.cs`

**Supporting/cross-referenced (inspected in this session):**
- `Bill_Software/corporate/business/app/Bill.Master` (markup)
- `Bill_Software/corporate/business/app/Bill.Master.cs` (incl. `CompanyContext`)
- `Bill_Software/DB_UTILITY.cs`
- `Bill_Software/index.aspx.cs`
- `Bill_Software/index.aspx` (partial — `cmbLoginAs` markup)
- `Bill_Software/index_card.aspx.cs`
- `Bill_Software/corporate/business/app/expense_entry.aspx.cs` (re-confirmed, prior turns)
- `Bill_Software/corporate/business/app/Create_quotation.aspx.cs` (partial, ~90 of 930 lines, plus targeted searches)
- `Bill_Software/corporate/business/app/AdminAttendanceDashboard.aspx.cs` (targeted sections)
- `Bill_Software/corporate/business/app/home.aspx.cs` (targeted sections)
- `Bill_Software/corporate/business/app/AddUser.aspx.cs` (targeted sections, re-confirmed)
- `Bill_Software/corporate/business/app/AdminApprovalDashboard.aspx`
- `Bill_Software/corporate/business/app/AdminApprovalDashboard.aspx.cs`
- `Bill_Software/corporate/business/app/ManageRoles.aspx.cs`
- `Bill_Software/corporate/business/app/ManagePermissions.aspx.cs`
- `Bill_Software/corporate/business/app/Update_Designation.aspx.cs` (targeted sections)
- `Bill_Software/corporate/business/app/ViewUser.aspx.cs` (full — class `WebForm80`)
- `Bill_Software/corporate/business/app/CommunicationGateway.cs`
- `Bill_Software/corporate/business/app/New_vendor.aspx.cs` (targeted sections — `TenantId` search)
- `Bill_Software/Web.config` (targeted — connection string names/hosts only, no credential reproduced)

**Searched but not exhaustively read (targeted grep only):** `AdminOverride.aspx.cs`, `QuickAction.aspx.cs`, `attendance.aspx.cs`, `MyLeaves.aspx.cs` (all matched on `CommunicationGateway` usage only, confirmed as Attendance/Leave-domain, not Sales-Visit-related).

---

## 26. Database Objects Inspected

**Note:** "Inspected" here means "referenced in application source code and analyzed for usage pattern," not "queried live" — see §0.

- `tbl_SalesVisitReport` (all columns referenced in source; see `02_Database_Dependency_Map.md` for the full column inventory and this document's §10/§18 for newly-confirmed write-path gaps)
- `tbl_SalesVisitResponses`
- `tbl_Expenses`
- `tbl_login` (including `RoleId`, `CompanyID`, `ReportingManagerId`, `User_Id`, `Id`, `IsActive`)
- `Roles` (`RoleId`, `RoleName`)
- `UserRoles` (`UserId`, `RoleId`)
- `RolePermissions` (`RoleId`, `PermissionId`)
- `Permissions` (`PermissionId`, `PermissionKey`, `ModuleName`, `SubModuleName`, `FeatureName`, `Description`)
- `dbo.ActiveSessions` (`SessionToken`, `UserId`, `IsActive`, `IPAddress`, `UserAgent`, `LoginTime`, `LastHeartbeat`)
- `tbl_Company` (`ID`, `Name`, `IsActive`, `Address`, `Signe`, `ShortCode`)
- `tbl_AttendanceRegularization`, `tbl_LeaveRequests`, `tbl_LeaveMaster`, `tbl_EmployeeLeaveBalance`, `tbl_Attendance` (inspected only to confirm `AdminApprovalDashboard.aspx.cs`'s actual domain — §14)
- `tbl_SystemNotification` (inspected only as a side-effect insert in `AdminApprovalDashboard.aspx.cs`/`ViewUser.aspx.cs`/`New_vendor.aspx.cs`, not Sales-Visit-specific)

**Not inspected (no DDL access):** actual column data types, nullability, default constraints, `CHECK` constraints, indexes, foreign-key constraint names, triggers, or stored-procedure bodies for any of the above — all such facts, where stated in this document, are either taken as given from the task's authoritative schema reference (explicitly labeled as such) or inferred from code usage only (also explicitly labeled).

---

## 27. Evidence / Code References

All file/method/line-level evidence is inlined directly within each numbered finding in §2–§17 above, in the required format (File / Class / Method / Database object / Relevant behavior / Impact / Confidence or Status). No separate consolidated evidence appendix is duplicated here to avoid restating the same citations twice; cross-references between sections use explicit `§N` pointers.

---

## Implementation Status

```text
NO APPLICATION CODE WAS MODIFIED.

NO DATABASE OBJECT WAS MODIFIED.

NO CONFIGURATION WAS MODIFIED.

NO EXISTING DOCUMENTATION WAS MODIFIED.

This task was strictly read-only architecture inspection.
```
