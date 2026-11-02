# 49 — FLMX identity-model feasibility: one `tbl_login` identity vs duplicate users per tenant

**Status:** Read-only feasibility and architecture review. No code, schema, or configuration changed.
**Date:** 2026-09-30
**Question:** Can FLMX support one `tbl_login` identity across multiple companies via `dbo.UserCompanyAccess`, or must each tenant/company get a duplicate `tbl_login` user?
**Tags:** **[F]** = confirmed repository/schema fact (cited). **[I]** = architectural inference, clearly labeled.

---

## 1. Conclusion

**FLMX does not require duplicate `tbl_login` users per tenant/company. The single-identity / multi-membership model is already the implemented architecture and is structurally complete: identity (who), membership (where), and authorization (what, per current company) are separable today, and every request path re-validates membership against the current company rather than `tbl_login.CompanyID`.** Duplicate users would be strictly more work and strictly more risk: they would break the identity-global surfaces that are keyed on `User_Id` / `Id` alone (login, password reset, OTP, settings, sessions, audit), and would re-introduce the exact single-company assumptions the Phase 2A / membership work removed.

What remains are **hardening blockers, not architectural blockers**: an undeclared `UNIQUE` on `tbl_login.User_Id`, one authorization fallback that still ignores company, and a residue class of company-scoped user lookups. None require redesigning the identity model. See §9 for the minimum scope and §11 for the deployment call.

---

## 2. Vocabulary (separations the review was asked to draw)

| Concept | Where it lives today | Evidence |
|---|---|---|
| **Identity** = who the user is | `tbl_login.Id` (numeric), `tbl_login.User_Id` (business code, e.g. `FLM021`); credentials, OTP, lockout, `EmailVerified`, `MustChangePassword`, profile | [F] `docs/page-catalog/UAT_CATALOG.md` L977–1024 |
| **Membership** = which companies the identity may enter | `dbo.UserCompanyAccess (UserId → tbl_login.Id, CompanyID → tbl_Company.ID, IsActive)`, `UNIQUE (UserId, CompanyID)` | [F] `Bill_Software/corporate/business/sql/UserCompanyAccess.sql` L16–36 |
| **Authorization** = what the identity may do *in the selected company* | `Session["CompanyID"]` → `CompanyContext.CurrentCompanyID`; `UserRoles` → `RolePermissions` → `Permissions.PermissionKey`; resource scope predicates (`RecordInCompany`, `VisitExists`, …) | [F] `AuthGuard.cs` L219–260, L420–470; `docs/ARCHITECTURE.md` "Request lifecycle" |
| **Home company** = default/preferred company (metadata, not authorization) | `tbl_login.CompanyID` — used by `ResolveInitialCompanyId` (boot default) and by `ViewUser` write-guard messaging | [F] `AuthGuard.cs` L135–160, L583–610; `ViewUser.aspx.cs` L28–45 |
| **Tenant-owned business data** | `CompanyID` on `tbl_Quotation`, `tbl_Invoice`, `tbl_Chalan`, `tbl_Proforma`, `tbl_RequisitionMain`, `tbl_PO_Header`, `tbl_SalesVisitReport`, `tbl_Client`, `tbl_Vendor`, … (gaps documented separately) | [F] `docs/page-catalog/SHARED_SCHEMA.md` §6 |

[F] `docs/22_Security_Baseline.md` L415 already encodes the model: *"No HQ cross-company role. Membership in `UserCompanyAccess` is the only way to hold another `CompanyID`. Do not invent an all-tenant exception."*

---

## 3. Confirmed repository facts

### 3.1 `tbl_login` structure (UAT schema)

[F] (`UAT_CATALOG.md` L977–1024):
- PK `Id` (int identity); `User_Id varchar(50) NULL` — **no declared UNIQUE on `User_Id`**.
- `UNIQUE UQ_User_Email_Filtered (Email) WHERE [Email] IS NOT NULL`; `UNIQUE UQ_User_Phone_Filtered (Phone_no) WHERE [Phone_no] IS NOT NULL` — **email/phone uniqueness is global across all companies, not per company**.
- Credential + lifecycle state is per row: `PasswordHash`, `PasswordSalt`, legacy `Password`, `MustChangePassword`, `EmailVerified`, `OtpCodeHash/OtpExpiry/OtpAttemptCount/OtpSalt`, `FailedAccessCount`, `LockoutEnd`, `LastLogin`, `IsActive`, `LastPasswordChangeDate`, `TermsAcceptedDate`.
- `RoleId` (single display role), `DepartmentID`, `DesignationID`, `ReportingManagerId varchar(50)`, `RequireGeoTagging`, `GeoFence*`, `EnableEmailAlerts`, `EnableWhatsAppAlerts` — HR/display attributes on the identity row.
- `CompanyID int NOT NULL DEFAULT (1)` and legacy `TenantId int NULL`.

### 3.2 Membership table

[F] (`UserCompanyAccess.sql` L16–36): `UserId` FK → `tbl_login.Id`; `CompanyID` FK → `tbl_Company.ID`; `IsActive` bit; `UNIQUE (UserId, CompanyID)`. Script header states the design intent verbatim: *"tbl_login.CompanyID is a single home-tenant column, not a membership list."*

[F] (`db/user_company_access_seed.sql` header evidence, flamex_uat 2026-09-10): 25 users, 24 active, **all home `CompanyID=1`**; two companies (1=Flame-Ex, 2=AA Associates); seed inserts **home-tenant memberships only**, idempotently.

[F] `AuthGuard.TryEnsureHomeMembership` (`AuthGuard.cs` L170–217) runs inside the `AddUser` transaction (`AddUser.aspx.cs` L172–183) so every newly created user gets exactly the home-tenant membership — nothing more.

### 3.3 Login and session (identity-global, company-agnostic)

[F] (`index.aspx.cs` L64–69): login is `SELECT TOP 1 … FROM tbl_login u LEFT JOIN Roles r ON u.RoleId = r.RoleId WHERE u.User_Id = @UserId AND u.IsActive = 1` — **no `CompanyID` predicate and no `ORDER BY`** (correct only if `User_Id` is unique; it is not declared so).
[F] (`index.aspx.cs` L165–200): session = `USERID` (string), `UserDbId` (numeric `Id`), `RoleId`, `RoleName`, `ProfilePic`, `USERTYPE`, `SessionToken`. Prior `ActiveSessions` are deactivated **by `UserId`** — one live session per human identity. `Session["CompanyID"]` is deliberately **not** set at login; `Bill.Master.Page_Load` resolves it (membership-aware), and `SecurePage.OnInit` 401s deep links that arrive before that ([F] `ARCHITECTURE.md` ASP.NET section).

### 3.4 Company-context selection and enforcement

[F] `AuthGuard`:
- `UserCanAccessCompany(companyId)` (L57–89): session valid **AND** active `UserCompanyAccess` row for `(UserDbId, companyId)` **AND** company active. Every company switch is re-validated (`Bill.Master.cs` `ddlCompany_SelectedIndexChanged`).
- `GetAuthorizedCompanies()` (L92–133): the company dropdown's only source (`Bill.Master.cs` `BindCompanies`) — membership list, not `tbl_login.CompanyID`.
- `ResolveInitialCompanyId()` (L135–160): one membership → that company; multiple → home company **only if it is itself an active membership**; otherwise 0 (fail closed, documented "do not pick the first company").
- `EnsureAuthorizedCompanyContext` + `ClearUnauthorizedCompanySession` + `EnsureWebMethod` (→ `UserCanAccessCurrentCompany`): **every** request re-checks membership. Cross-tenant access is therefore *never* unrestricted.

### 3.5 Menu vs page authorization

[F] `Bill.Master.GetMenuControl` (`Bill.Master.cs` ≈L232–245): menu permissions join `Permissions → RolePermissions → Roles → UserRoles → tbl_login → UserCompanyAccess (a.CompanyID = @CompanyID AND a.IsActive = 1)` with the in-code comment *"Roles are global; tenant scope comes from UserCompanyAccess, not tbl_login.CompanyID (home tenant only), so members see their menu in the selected company without a per-company tbl_login row."*

[F] `AuthGuard.HasPermission(userId, key, companyId)` (L226–260): primary statement is company-scoped (`ur → tbl_login u ON ur.UserId = u.Id AND u.CompanyID = @CompanyID`, L228), **but an unconditional global fallback** (same join without the company predicate, L245–259) returns true if the permission exists on any of the user's roles. This is the one place where "authorization is per-current-company" is not yet true end-to-end (see §6).

### 3.6 Impersonation (Switch User)

[F] `ImpersonationRuntime.cs`:
- `LoadEligibleTarget` (≈L419–460): target must have **an active `UserCompanyAccess` membership in the actor's current company**, `IsActive=1`, not locked. Roles resolved per company (`LEFT JOIN Roles r ON r.RoleId = u.RoleId AND r.CompanyID = @CompanyID`).
- `GateActor` (≈L386–411): actor needs `SwitchUser` permission **in the current company context**, no nested impersonation, no self-target.
- Lease `dbo.ImpersonationSessions` stores the actor's **current** `CompanyID` (`InsertLease`); the target's `ActiveSessions` row is inserted with the same company; `CompanyID` is **never swapped** by `ApplyTargetSession` (≈L725–741 — it swaps only `USERID`, `UserDbId`, `RoleId`, `RoleName`, `ProfilePic`, `SessionToken`).
- `ActorHoldsSwitchUser` (≈L562–600) re-checks the **actor's** membership in `CompanyContext.CurrentCompanyID` on every master render, with the explicit comment that `tbl_login.CompanyID` "is the home tenant only". Fail-closed on SQL error.

[F] `SwitchUser.aspx.cs` `BindUsers` (≈L95–110): target list = active `tbl_login` ⋈ `UserCompanyAccess` in the current company, roles joined per company. A target without membership in the actor's current company is simply not listed and cannot be started even by id.

### 3.7 User-management surfaces

[F] `ViewUser.aspx.cs`:
- Listing joins membership (`JoinCurrentCompanyMembers`, L20–25; `BindGrid` ≈L93–113): the grid shows **members of the current company**, including users homed elsewhere.
- All writes (update L≈305, activate/deactivate L556, lock/unlock L578–618, delete L640, password reset L693–700, geo-fence `SaveGeoFence`) are `WHERE Id = @Id AND CompanyID = @CompanyID` — **home-anchored writes** — with `IsCrossCompanyMember`/`BlockedWriteMessage` (L28–45) producing *"…available only from the user's home company."* This is an intentional design: reads are membership-wide, writes are home-scoped.

[F] `AddUser.aspx.cs`: single INSERT site for ERP users (`INSERT INTO tbl_login … CompanyID`, L131–137); role mapping via `UserRoleAssignment.TryEnsureMapping` (L172–178); membership via `TryEnsureHomeMembership` (L181–183); duplicate email/phone pre-check is company-scoped while the DB uniques are global (L301).

[F] `UserRoleAssignment.cs`: `RoleBelongsToCompany` enforces `Roles.CompanyID = @CompanyID` at grant time; `UserRoles` itself has **no company column** (`UQ (UserId, RoleId)`); `tbl_login.RoleId` is kept as the single display role (`TrySyncDisplayRoleChange`, `AlignDisplayRoleFromUserRoles`).

### 3.8 Roles and permissions data

[F] (`UAT_CATALOG.md` L188–215, L2319–2333; `SHARED_SCHEMA.md` §12):
- `Roles`: PK `RoleId`, **`UNIQUE (RoleName)` global** (no company in the unique), plus `CompanyID int NOT NULL DEFAULT 1` (added by `corporate/business/sql/CompanyID_Roles_ActiveSessions.sql`) and legacy `TenantId`.
- `UserRoles`: no `CompanyID`. `RolePermissions`: no `CompanyID`. `Permissions`: unique `PermissionKey` (catalog-wide).
- UAT data: 3 roles **all `CompanyID=1`**; 4 `UserRoles` rows (all Super Admin); `tbl_login.RoleId` NULL on all 25 rows; 137 `RolePermissions`. Per-company role data does not exist yet anywhere.

### 3.9 Foreign keys touching `tbl_login`

[F] (`SHARED_SCHEMA.md` §5): declared FKs into `tbl_login.Id` are only `ActiveSessions.UserId`, `UserRoles.UserId`, and (via DDL) `UserCompanyAccess.UserId`. `ImpersonationSessions.ActorUserId/TargetUserId` are logical references (commented "-- tbl_login.Id"), no declared FK. **No business/transaction table has a declared FK to `tbl_login`.** Attribution is by varchar business code: `tbl_SalesVisitReport.CreatedByCode → tbl_login.User_Id` (undeclared), `tbl_LeaveRequests.UserCode`, `ManagerID`, attendance `UserCode`, `ReportingManagerId`, `tbl_login mgr` self-joins.

### 3.10 Lifecycle / uniqueness facts relevant to the question

[F] Identity-global (keyed by `User_Id` or `Id` alone, no company predicate): login, forgot-password lookup (`index.aspx.cs` L300), `PasswordResetService` (L122), OTP verification update (`index.aspx.cs` ≈L489), `settings.aspx` profile/OTP paths, `Update/*` self-service pages, `home.aspx` profile loads, single-session enforcement.
[F] Company-scoped: HR/user lists and pickers (`AddUser.aspx.cs` L60; `attendance.aspx.cs` L382, L734, L864; `AdminAttendanceDashboard.aspx.cs` L52; `AdminShiftAssignment.aspx.cs` L55; `Add_invoice.aspx.cs` L103; `Manual_Invoice.aspx.cs` L61; `Create_quotation.aspx.cs` L87; `edit_purchaseorder.aspx.cs` L54; `srch_dailyrpts.aspx.cs` L102, L233; `Update_Designation.aspx.cs` L66; `Bill.Master.cs` `GetUserEmailFromDatabase` L336); leave allocation SP `sp_AllocateEmployeeLeaves (@UserCode, @CompanyID)`.
[F] `ManageRoles.aspx.cs` lists roles `WHERE CompanyID = @CompanyID` (L36) but **INSERTs roles without a `CompanyID` column** (L235) → they default to 1; combined with `RoleBelongsToCompany`, a role created while operating company 2 can never be granted there. Residue, not design.

### 3.11 No duplicate-user mechanism exists

[F] `AddUser` is the only `INSERT INTO tbl_login` site in the ERP (`docs/page-catalog/DATA_DICTIONARY.md` L66 + code search). The cross-tenant duplication program (v1.1, `DuplicationService`, PR #92 at `88f9d0f`) copies **vendors and customers**, deliberately never users, and never swaps `Session["CompanyID"]`. There is no per-tenant user provisioning path and no code that creates a second row for the same human.

---

## 4. Answers to the ten required questions

| # | Question | Answer |
|---|---|---|
| 1 | Global identity requirements favoring one row per human | [F] Login, single-session enforcement (`ActiveSessions` killed by `UserId`), password reset tokens, OTP, settings/self-service, and audit (`AuthAudit.UserId` int → `tbl_login.Id`) are all keyed on the identity alone. Duplicating a human splits these invariants across rows. |
| 2 | Tenant-local data/authorization that could require separate rows | [F] Business data is company-scoped by its own `CompanyID` columns, not by the user row. Authorization is re-evaluated per current company (§3.4, §3.6). Nothing found requires the *identity row* to be tenant-local. [I] The only genuine tenant-local candidates are HR attributes (§4.10), which are display data. |
| 3 | Existing uniqueness constraints | [F] Email and Phone are **globally** unique (filtered uniques). `User_Id` has **no declared unique**. `UserCompanyAccess` is unique per (user, company). `UserRoles` unique per (user, role). `Roles.RoleName` unique **globally**. |
| 4 | FKs from business data to `tbl_login` | [F] None, except session/RBAC/membership tables (§3.9). Business attribution is varchar business codes. Duplicate users would therefore not need business-data migration — but they also would not gain integrity from it. |
| 5 | Role/permission ownership: global or company-specific? | **Half-migrated.** [F] `UserRoles`/`RolePermissions`/`Permissions` are global (no company columns); `Roles.CompanyID` exists and `RoleBelongsToCompany` enforces it at grant time; role pickers are company-filtered; the menu applies roles **through** the current-company membership; but `AuthGuard.HasPermission`'s global fallback lets a role granted anywhere authorize a page anywhere (§3.5). Effectively: *assignment surface* is company-scoped, *authorization join* is global. |
| 6 | Can `UserCompanyAccess` fully express membership without duplicating identity? | [F] **Yes** — it is the sole membership authority on every gate that matters: company dropdown, company-switch guard, `SecurePage`/`EnsurePage`, WebMethods, print gate resources, menu, Switch User target list, Switch User actor re-check. Already proven live on UAT: `admin` (home Company 1) switched to Company 2 and received the 117-key menu — one identity, two memberships, zero duplicate rows (`docs/HANDOFF.md` Case 1, commit `5ace354`). |
| 7 | Does impersonation stay coherent across multiple memberships? | [F] **Yes.** Both actor and target are gated on active membership in the **current** company; leases record the company of the impersonation; `CompanyID` is never swapped in session; rollback restores identity + token and leaves company untouched; banner re-validates the actor's membership every render. Multiple memberships per actor or target do not affect any invariant (nested, exactly-once close, replay, fail-closed). |
| 8 | Do password/MFA/audit/activation/lifecycle work with one identity? | [F] **Yes, and better than with duplicates.** Credentials, OTP/`EmailVerified`, `MustChangePassword`, lockout, `LastLogin`, and audit are identity-global state; one row means one password, one lockout, one audit trail per human. Deactivation (`IsActive=0`) removes the person from every company's login, target lists, and pickers at once (all consumers filter `u.IsActive = 1`). [I] Under duplicates these would need synchronization or would drift (§7). |
| 9 | Code paths still using `tbl_login.CompanyID` as authorization? | [F] Two classes remain. (a) `AuthGuard.HasPermission` global fallback (§3.5) — real authorization leakage across the company dimension. (b) Company-scoped **user lists/pickers** (§3.10) — these enumerate colleagues by home company, so a member homed elsewhere is invisible as an assignee/approval subject/IT-support email target; display/UX residue, not a privilege grant. Intentional non-residue: `ViewUser` home-anchored **writes** and `ResolveInitialCompanyId` home-preference boot (metadata, fail-closed). |
| 10 | Tenant/company-specific attributes on `tbl_login` that get ambiguous under multi-membership | [F] `DepartmentID`, `DesignationID`, `ReportingManagerId`, `RequireGeoTagging`/`GeoFence*`, `EnableEmailAlerts`/`EnableWhatsAppAlerts`, display `RoleId` are all single-valued per identity. [I] They are HR/notification/display attributes, not authorization inputs (`ReportingManagerId` is explicitly not ACL — Decision #8). If per-company HR profiles are ever needed, they belong on a per-membership extension (e.g. columns or a 1:1 table keyed on `UserCompanyAccess.UserCompanyAccessId`) — not on duplicate identities. Home `CompanyID` itself stays unambiguous as "home company metadata". |

---

## 5. Evidence that contradicts the conclusion (stated plainly)

1. [F] **`AuthGuard.HasPermission` global fallback** (`AuthGuard.cs` L245–259): a permission held via *any* role authorizes a page in *any* current company. Until closed or made membership-scoped, "authorization is per-current-company" is not literally true at page level (menu already is). This is the strongest counter-evidence and is listed as Blocker B2.
2. [F] **No declared `UNIQUE` on `tbl_login.User_Id`** while login is `SELECT TOP 1 … WHERE User_Id = @UserId` with no `ORDER BY` (`index.aspx.cs` L64–69): the whole model assumes `User_Id` uniqueness the schema does not enforce. Blocker B1. (`docs/08_Cursor_Change_Audit.md` L160 and `docs/14` L107 flag the same risk.)
3. [F] **`Roles.RoleName` is globally unique** while role CRUD is company-filtered and role creation drops `CompanyID` (§3.10): the role system cannot yet represent two companies wanting same-named roles, and company-2 role creation is broken. Residue of the pre-membership design.
4. [F] **Company-scoped user pickers** (§3.10) contradict "membership expresses who is in the company": a cross-company member cannot be assigned visits/invoices/dashboards, and `Bill.Master.GetUserEmailFromDatabase` (L336) returns "Not Provided" for them. Display residue, but it materially degrades the multi-membership experience.
5. [F] **Single display `RoleId` on `tbl_login`** plus `TrySyncDisplayRoleChange` removing the previous mapping: one identity shows exactly one role everywhere, while roles are potentially per-company. Cosmetic ambiguity only (menu does not use it), but it must be acknowledged in any per-membership-role decision.
6. [F] **UAT has zero multi-membership users and zero company-2 roles** (`user_company_access_seed.sql`; `SHARED_SCHEMA.md` §12): the multi-membership path has production-shaped data only for the `admin` UAT case; Case 2 (single-company regression candidate `FLM021`) remains BLOCKED by that user's `MustChangePassword` lock. Multi-membership at scale is exercised, not proven.
7. [F] **`tbl_login.CompanyID` is `NOT NULL DEFAULT (1)`** with no FK to `tbl_Company` in the documented schema: a home company can point at a nonexistent/inactive company; `ResolveInitialCompanyId` handles it fail-closed, and `user_company_access_seed.sql` skips it, but the schema permits drift.

None of items 1–7 justify duplicate identity rows; items 3–5 are migration residue, and items 1–2 are hardening gaps.

---

## 6. Decision matrix

| Dimension | A: One `tbl_login` + `UserCompanyAccess` | B: Duplicate `tbl_login` per company |
|---|---|---|
| Identity uniqueness / login correctness | One row per human; needs **declared `UNIQUE(User_Id)`** (B1) to make `SELECT TOP 1` well-defined | Needs composite uniqueness `(User_Id, CompanyID)`; login must become company-aware (pre-auth company picker or email→rows resolution); `SELECT TOP 1` per company |
| Email/phone uniqueness | Global uniques already enforce one contact per human | Must drop global uniques or make them composite per company; same email duplicated per tenant |
| Password, OTP, `EmailVerified`, `MustChangePassword`, lockout | Single source of truth per human (already keyed identity-global) | Per-row state; requires sync policy (reset in company 1 ≠ company 2?) or per-tenant passwords; `MustChangePassword` per row |
| Deactivation / lifecycle | `IsActive=0` removes the human everywhere at once; per-company exclusion = deactivate the membership row | Must deactivate N rows in the right companies; "delete user" is ambiguous (which tenant owns the history?) |
| Sessions (`ActiveSessions`) | One live session per human (existing kill-by-UserId invariant holds) | N sessions per human; kill-by-UserId must become kill-by-(row, company); impersonation `HasActiveLease` and restore logic must be re-keyed |
| Authorization per company | Membership gate (done) + permission join (B2 cleanup); resource scope already company+owner | Natural per-company rows, but identical RBAC duplication (`UserRoles`, `RolePermissions`, grants) per row; role grant UX must handle N rows per person |
| Roles | [I] Keep global roles (current data); close fallback; optionally add per-membership role layer later | `Roles.CompanyID` semantics become mandatory; `RoleName` global unique must be relaxed; grants multiply |
| Tenant-owned business data | Unchanged — `CompanyID` predicates on business tables; attribution codes stable | Unchanged structurally, but historical `CreatedByCode`/`UserCode` references split across duplicate codes per company |
| Impersonation | Coherent today (§4.7); no change | Target resolution per (company, row); lease `CompanyID` redundant with the row; restore logic re-keyed |
| Audit identity | One `AuthAudit.UserId` per human | Actions attributed to different `Id`s per company; cross-tenant actor correlation requires a new mapping column |
| Migration for existing users | **Already done**: `user_company_access_seed.sql` seeds home memberships idempotently (23 rows seeded on UAT) | One duplicate row per (user × company-of-record); backfill business-code references or accept split histories; password/OTP state copy policy |
| Operational risk | Low: model is implemented and UAT-proven for the flagship case; risks are the named hardening blockers | High: credential sync drift, split audit, split lockout (locked in one tenant, clean in another), doubled user-admin UX, FK/audit ambiguity, impersonation re-keying |
| Code churn | Small, enumerated (§8) | Large: login, sessions, all identity-global surfaces (§3.10), reset/OTP/settings, HR, audit, impersonation |
| Verdict | **Sufficient; adopt as the target model** | Not justified by any subsystem requirement found; would regress identity-global invariants |

---

## 7. Is any subsystem forced to duplicate users?

**No.** The review looked for a subsystem where membership *cannot* express the requirement:

- *Per-tenant credentials/password isolation* — the only plausible candidate. [I] This is a product-policy preference, not an architectural necessity: nothing in the code consumes per-company passwords, and identity-global state (§4.8) is the safer default. If ever mandated, it could be modeled as per-membership credential overrides **without** duplicating identity rows.
- *Per-company HR profile* (department, designation, manager, geo-fence) — [I] real, but display/HR data (Decision #8 keeps `ReportingManagerId` out of ACL); extend the membership side, not identity.
- *Per-company roles* — [F] supported today through the membership-filtered permission join; the global fallback (B2) is the only leak.
- *Per-company activation* — [F] `UserCompanyAccess.IsActive` already is per-company activation/deactivation of access.

---

## 8. Minimum scope to complete the single-identity / multi-membership model

**Blockers (in priority order):**

| ID | Item | Type | Where |
|---|---|---|---|
| B1 | Declare `UNIQUE` on `tbl_login.User_Id` (after a duplicate audit) so login's `SELECT TOP 1` is well-defined | DBA DDL script (live schema is unversioned; script + verification per `SHARED_RUNTIME.md` conventions) | new `db/` script |
| B2 | Close or membership-scope the `AuthGuard.HasPermission` global fallback so page authorization matches the membership-aware menu (note: `AuthGuard` is a protected asset — requires architectural review per `ARCHITECTURE.md`) | Code (one method) + UAT | `AuthGuard.cs` L226–260 |
| B3 | Decide global vs per-membership roles explicitly; if global (recommended for now, matches data), document it next to Decision #9; fix `ManageRoles` role INSERT to write the operating `CompanyID` (or drop per-company role pretense) | Code + doc | `ManageRoles.aspx.cs` L235, `docs/22` |
| B4 | Replace home-company user enumerations with membership joins (pattern exists: `ViewUser.JoinCurrentCompanyMembers`, `SwitchUser.BindUsers`) at the §3.10 sites so cross-company members appear as assignees/approval subjects | Code (mechanical) | listed files |
| B5 | Boot semantics for members whose home company is not among their memberships: `ResolveInitialCompanyId` correctly fails closed to 0; allow explicit first-selection (dropdown pick) and/or a documented tie-break rule | Code (small) + UX note | `AuthGuard.cs` L135–160 |

**Deferred (do now only if a second tenant actually onboards staff):** per-membership HR profile attributes (§4.10); per-membership role layer; company-2 `Roles` data and `RoleName` uniqueness policy.

**Migration implications for existing single-company users:** none beyond what is shipped — `db/user_company_access_seed.sql` already creates each active user's home-tenant membership idempotently and preserves existing rows; identity-keyed surfaces (login, reset, sessions, audit) need no change; business data is untouched because nothing FKs to `tbl_login`. The only data prerequisite is the B1 duplicate audit on `User_Id`.

---

## 9. Minimum scope if duplication were unavoidable (for the record)

1. Composite identity uniqueness `(User_Id, CompanyID)`; drop/replace global email/phone uniques.
2. Company-aware login (pre-auth company selection or identifier→rows resolution); `ActiveSessions` and single-session invariant re-keyed to the duplicate row.
3. Credential/lifecycle sync policy across duplicates (or per-tenant passwords) + per-row `MustChangePassword`/`EmailVerified`/lockout semantics + cross-tenant audit correlation column.
4. Impersonation target resolution per (company, row); lease/restore re-keying.
5. `UserCompanyAccess` becomes redundant (or maps duplicates); every membership join replaced by row filters; all identity-global surfaces in §3.10 re-keyed.

Justification threshold (per the review constraints): a subsystem whose requirement provably cannot live on membership. **None was found.**

---

## 10. PR #94 assessment

**The current membership implementation is sufficient as the foundation; no architectural change is required before deployment.** Note on numbering: the repository records this work as Switch User PR **#84** (first-parent `fa777e9`; merge `d67e854` on `master`) plus the unpushed membership commits (`1bb4754`, `300505a`, `3e04c58`, `fb7de2b`, `5ace354`) on `cursor/impersonation-membership-runtime` / `July_to_Sept26_DevNSupport` (`docs/HANDOFF.md`); this review treats the task's "PR #94" as referring to that same membership implementation. What it already delivers: membership as the sole company authority on every gate; membership-aware menu; impersonation gated on membership on both actor and target with the company never swapped; fail-closed behavior on missing table/rows (UAT confirms fail-closed semantics for missing objects); home-company seed migration.

Pre-deployment conditions (from §8, all non-architectural): apply `user_company_access_seed.sql` (deployment checklist step 1 — already canonical), land B1 before or with rollout, and either land B2 or explicitly accept and document the global fallback as a known limitation until it is closed. Close UAT Case 2 by clearing `FLM021`'s `MustChangePassword` lock so the single-company regression candidate is actually exercised.

---

## 11. Deployment relevance

**This question does not block the current deployment; handle as a subsequent enhancement.**

- The single-identity/membership model is already the shipped architecture; deploying the branch changes nothing about how identities are stored.
- Current UAT data is effectively single-tenant in practice (all 25 users home Company 1; memberships seeded to home only), so the multi-membership path adds no new exposure on deploy day, and every gate fails closed.
- Duplicating users instead would be a larger, riskier change (§6, §9) undertaken purely to match residue that the same deployment removes.
- Sequence: deploy (flag off in production per `SWITCHUSER_DEPLOYMENT_CHECKLIST.md`) → B1 → B2 (+B3) → B4/B5 as the named enhancement backlog; revisit per-membership HR attributes only on a real second-tenant onboarding.

---

## 12. Facts vs inference (summary split)

**Confirmed [F]:** every schema/constraint/FK statement (§3.1–3.2, §3.8–3.9); login/session mechanics (§3.3); company-context enforcement (§3.4); menu vs page authorization behavior incl. the fallback (§3.5); impersonation gating and company immutability (§3.6); ViewUser/AddUser/UserRoleAssignment/ManageRoles behaviors (§3.7, §3.10); absence of duplicate-user provisioning (§3.11); Decision #8/#9 text; UAT Case 1 multi-membership PASS; the residue enumerations in §5.3–5.4.

**Inference [I]:** that the `HasPermission` fallback and `Roles` INSERT omitting `CompanyID` are migration residue rather than intent (contradicted by nothing; contradicts current design statements); that duplicate users' operational risks (credential drift, split audit) follow from the schema; that per-tenant HR attributes belong on membership, not identity; that per-tenant credential isolation would be a product-policy choice; the deployment sequencing in §11.

*No repository state was modified in producing this review.*
