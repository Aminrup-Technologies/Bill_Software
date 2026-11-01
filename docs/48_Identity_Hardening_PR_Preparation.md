# 48 — Identity hardening PR preparation (post-go-live)

**Status:** Prepared artifacts for a future Cursor-executed hardening PR. The identity-hardening worktree baseline is `8330a48f1595464379980b5ccd904d61f589ba4f` (PR #94 merge; `cursor/identity-hardening-postgolive`). `July_to_Sept26_DevNSupport` remains at `4a37676`. Not deployed, not merged, no push.
**Date:** 2026-09-30
**Branch:** `cursor/identity-hardening-postgolive`
**Scope:** B1 (`User_Id` uniqueness), B2 (`AuthGuard.HasPermission` company scoping — design in §3 only; `AuthGuard.cs` is not modified), B4 (membership-aware user enumeration), plus the discovered `Edit_quatation_v2.aspx.cs` global enumeration.
**Explicitly excluded:** B3 role architecture, per-membership HR attributes, B5 initial-company UX, `srch_dailyrpts` Name→User_Id identity resolution, duplicate-user provisioning, tenancy abstraction.
**PR #94 deployment is frozen** — every artifact here is additive or documentation; the deployment branch worktree was never touched.

---

## 1. Changed-file manifest

| File | Change | Why |
|---|---|---|
| `db/user_id_uniqueness_hardening.sql` | **New.** PASS 1 read-only duplicate audit; PASS 2 conditional filtered UNIQUE index `UQ_tbl_login_User_Id` on `tbl_login(User_Id) WHERE User_Id IS NOT NULL`; never mutates data | B1. Login is `SELECT TOP 1 … WHERE User_Id=@UserId` with no ORDER BY (`index.aspx.cs` L64–69); the model requires `User_Id` uniqueness the schema does not declare. Filtered because the column is nullable and a plain unique index would cap NULLs at one row. |
| `Bill_Software/corporate/business/app/AddUser.aspx.cs` | Manager picker query (L60): home-company predicate → `UserCompanyAccess` join | B4. Manager assignment must reach members of the current company, not only users homed there. Value column `User_Id` preserved. |
| `Bill_Software/corporate/business/app/Add_invoice.aspx.cs` | `LoadSalesPersons` (L103): same conversion | B4. Salesperson assignment picker. Value `User_Id`. |
| `Bill_Software/corporate/business/app/Manual_Invoice.aspx.cs` | `LoadSalesPersons` (L61): same conversion | B4. Same surface as Add_invoice. Value `User_Id`. |
| `Bill_Software/corporate/business/app/Create_quotation.aspx.cs` | `BindDropdowns` salesperson query (L87): same conversion | B4. Value column is numeric **`Id`** — preserved exactly; only the membership predicate changes. |
| `Bill_Software/corporate/business/app/edit_purchaseorder.aspx.cs` | `BindDropdowns` salesperson query (L54): same conversion | B4. Value **`Id`** — preserved. |
| `Bill_Software/corporate/business/app/AdminAttendanceDashboard.aspx.cs` | `LoadEmployees` (L52): same conversion | B4. Attendance register employee selector. Value `User_Id`. |
| `Bill_Software/corporate/business/app/AdminShiftAssignment.aspx.cs` | `LoadDropdowns` employee query (L55): same conversion | B4. Shift mapping targets. Value `User_Id`. |
| `Bill_Software/corporate/business/app/Bill.Master.cs` | `GetUserEmailFromDatabase` (L336): home predicate → membership join | B4. IT-support email routing for the current-company user; personalization only, existing "Not Provided" default unchanged. |
| `Bill_Software/corporate/business/app/Edit_quatation_v2.aspx.cs` | `BindDropdowns` salesperson query (L46): global all-user enumeration → current-company membership join | Discovered issue. The only enumeration with **no company predicate at all**: it lists every active user in every company, with numeric `Id` values, so a Company-2 member's Id could be bound as Company-1's salesperson. The file's other tenancy queries use `CompanyContext.CurrentCompanyID` (L67, L119, …), so the correction matches file-local conventions. |
| `docs/49_Identity_Model_Feasibility.md` | **New.** The feasibility review (prior session) | Evidence base for docs/48; its B1–B5 IDs are referenced throughout. |
| `docs/48_Identity_Hardening_PR_Preparation.md` | **New.** This file | Manifest, B2 design, regression matrix, deferred items, PR description. |

`AuthGuard.cs` is not a pending change. Its diff is empty: this patch contains no comment marker and no behavior change. B2 stays a documented design in §3.

## 2. B1 — `User_Id` uniqueness (DBA / data-integrity; audit first)

1. **Prerequisite:** run PASS 1 (duplicate audit) on the target database. Zero rows → run PASS 2. Non-empty → **STOP**; remediation is a data-owner decision and is out of scope; the script never deduplicates and never mutates user data.
2. PASS 2 re-verifies duplicates inside its transaction and fails closed with a clear message, so running it on a dirty database cannot deploy the constraint. Creation is skipped entirely if the index already exists (idempotent).
3. The index is **filtered** (`WHERE User_Id IS NOT NULL`): SQL Server unique indexes permit only one NULL row, `tbl_login.User_Id` is nullable, and legacy paths may insert NULL; excluding NULLs preserves legacy behavior exactly.
4. No application change is needed for B1: the constraint is what makes the existing `SELECT TOP 1` login well-defined.
5. **DBA execution required after go-live** (mark in the PR). Run against `flamex_uat` first; re-run PASS 1 after any UAT refresh; record audit output in the PR before PASS 2. Never execute from the application; never against `flamex_live` from development.

## 3. B2 — `AuthGuard.HasPermission` fallback (preparation only; no behavior change)

**Current behavior (AuthGuard.cs L226–260):** the primary statement is company-scoped (`INNER JOIN dbo.tbl_login u ON ur.UserId = u.Id AND u.CompanyID = @CompanyID`), but the unconditional fallback (L245–259) runs the same join **without** the company predicate and returns true if the permission exists on **any** role in **any** company. Consequence: a user granted a role holding permission X in Company 1 passes `EnsurePage`/`EnsurePageAny`/`EnsureWebMethodPermission` for X while operating Company 2 — even with no `UserCompanyAccess` row for Company 2 (company membership itself is still independently gated by `EnsureAuthorizedCompanyContext`, so the fallback widens *what* a member can do in a company they can enter; it cannot by itself grant entry).

**Proposed minimal fix (implementation-ready design, NOT applied here):** scope the fallback by membership — replace the home predicate with a `UserCompanyAccess` join, exactly the chain `Bill.Master.GetMenuControl` already uses:

```sql
SELECT TOP 1 1
FROM dbo.Permissions p
INNER JOIN dbo.RolePermissions rp ON p.PermissionId = rp.PermissionId
INNER JOIN dbo.UserRoles ur ON rp.RoleId = ur.RoleId
INNER JOIN dbo.tbl_login u ON ur.UserId = u.Id
INNER JOIN dbo.UserCompanyAccess a
    ON a.UserId = u.Id AND a.CompanyID = @CompanyID AND a.IsActive = 1
WHERE u.User_Id = @UserId AND p.PermissionKey = @Key
```

Both statements then answer "permission in the current company," differing only in home-scoped vs membership-scoped role reach. Alternative (only after an add-per-caller audit proves no caller relies on the fallback): delete the fallback and fail closed.

**Affected callers (verified on this baseline):** `EnsurePage` / `EnsurePageAny` (`SecurePage` page gates), `EnsureWebMethodPermission` (WebMethod gates), `BindSwitchUserMenu` + `GateActor` (`SwitchUser` permission, `Bill.Master.cs` / `ImpersonationRuntime.cs`), `UserCanManageCompanyVisits` (visit-manager scope, `AuthGuard` internal), `Bill.Master.GetMenuControl` is **not** a caller (it has its own membership join). No caller was found that depends on the cross-company fallback for a legitimate flow; every caller already sits behind `EnsureAuthorizedCompanyContext`, so membership-scoping the fallback cannot lock out a member who legitimately holds the role in the current company.

**Required approval:** `AuthGuard` is protected (`docs/ARCHITECTURE.md`; `HANDOFF.md` records a prior `AuthGuard.GetMenuPermissions` change rejected under ADR-001). B2 implementation requires explicit architectural-review sign-off. This pending patch does not modify `AuthGuard.cs` (empty diff): no comment marker and no behavior change. The gap and the proposed fix stay in this document. If review surfaces a dependent caller, document it here instead of forcing the change.

## 4. B4 — conversion rule and boundary decisions

**Rule applied:** convert only queries whose business question is *"who belongs to the company I am operating"* (assignment/approval/selection surfaces). Preserve untouched: home-company **write authorization** (`ViewUser` writes + `BlockedWriteMessage` — intentional), home-company **boot preference** (`ResolveInitialCompanyId` — intentional), and name-resolution lookups (deferred, §6). The join pattern is the established one (`ViewUser.JoinCurrentCompanyMembers`, `SwitchUser.BindUsers`, `Bill.Master.GetMenuControl`): `INNER JOIN dbo.UserCompanyAccess a ON a.UserId = u.Id AND a.CompanyID = @CompanyID AND a.IsActive = 1`, keeping each consumer's `@CompanyID = CompanyContext.CurrentCompanyID` parameter, `IsActive = 1`, `NOT IN ('admin', 'AT01')` exclusions, ordering, and value/display columns byte-identical.

Per-site value-column preservation: `AddUser`, `Add_invoice`, `Manual_Invoice`, `AdminAttendanceDashboard`, `AdminShiftAssignment`, `Bill.Master` → `User_Id` (string). `Create_quotation`, `edit_purchaseorder`, `Edit_quatation_v2` → numeric `Id`. **No value column, DataTextField/DataValueField, or exclusion list changes.**

**Not converted (intentionally):** `attendance.aspx.cs` L382/L734/L864 and `MyLeaves`/`srch_dailyrpts` self-scoped lookups operate on the *session's own* `User_Id` (the actor's own record, identity-global semantics) rather than enumerating colleagues — membership does not apply; `ViewUser` joins stay home-scoped by design (§ above); `Update_Designation.aspx.cs` L66 resolves the page's own `User_Id` query-string subject — out of enumeration scope.

## 5. Regression matrix (UAT, after B2 lands — preserved verbatim from prior preparation)

| Case | Actor | Expected |
|---|---|---|
| R1 | User with role only in Company 1; switch to Company 2 with no membership there | Company switch refused (`UserCanAccessCompany`); pages 403 under Company 2 context; menu empty in Co. 2. **Currently FAILS on fallback — this is the B2 proof case.** |
| R2 | `admin` (home 1, seeded memberships 1+2), Super Admin role granted both companies | Menu + pages work in both companies (117-key both, per HANDOFF Case 1) |
| R3 | Single-company user, home-only workflow | No change vs pre-B2 (primary statement already covered them) |
| R4 | Impersonation: actor with `SwitchUser` in Co. 1 → target member of Co. 1 | Start/End unchanged; `CompanyID` never swapped; banner persists (`ActorHoldsSwitchUser` already membership-scoped) |
| R5 | Target member of Co. 2 only while actor sits in Co. 1 | Target not listed (`SwitchUser.BindUsers` membership join); direct id deny `INV-COMPANY` |
| R6 | Print gate (`EnsurePrint`) + WebMethods (`EnsureWebMethodPermission`) | Unchanged; fail-closed |
| R7 | Deep link before company resolution | 401 (SecurePage ordering unchanged) |

B4 smoke (post-conversion): as a user homed in Company 1 with an active membership in Company 2, operating Company 2 — each converted picker must list that user (AddUser manager picker, both invoice salesperson pickers, quotation/PO salesperson pickers, attendance/shift dropdowns); operating Company 1 with no Company-2 membership — each picker must exclude them. `Edit_quatation_v2` additionally: no cross-company user may appear under any current-company context.

## 6. Deferred (do not fix here)

1. **`srch_dailyrpts.aspx.cs` L233 Name → User_Id identity resolution:** `SELECT User_Id FROM tbl_login WHERE Name = @Name AND CompanyID = @CompanyID` — first row wins on duplicate display names and feeds a later concatenated statement. Membership conversion does not fix Name-ambiguity; needs a PK-keyed follow-up. **Explicitly deferred per task constraints.**
2. Role architecture (B3): global vs per-company roles, `Roles.RoleName` global unique, `ManageRoles.aspx.cs` L235 INSERT missing `CompanyID` — separate PR with its own decision record.
3. Per-membership HR attributes (department, designation, reporting manager, geo-fence).
4. B5 initial-company UX for members whose home company is not among their memberships.
5. Exclusion-list drift (`attendance.aspx.cs` hardcodes `NOT IN ('admin','AT01')`, omits `IT01` seen elsewhere) — single-source later.
6. Duplicate-user provisioning / tenancy abstraction — rejected by the feasibility review (docs/49 §1, §6).

## 7. Validation plan (Cursor, post-go-live)

1. DBA on UAT: PASS 1 → record → PASS 2 → verification select (header of `db/user_id_uniqueness_hardening.sql`).
2. Build `Visual Studio I2I INC Web Application.sln` targeting VS 2015 / .NET 4.5.2 (local/UAT harness; no production execution).
3. Run §5 regression matrix R1–R7 on UAT after B2 lands; record PASS/FAIL per case in HANDOFF.
4. Run the B4 smoke in §5 for every converted consumer.
5. Confirm PR #94 behaviors intact: Switch User UAT cases 1/3/4, INV-13 nested deny, banner, `CompanyID` never swapped.
6. Verify `git status` shows only the manifest files; `Web.config`, `*.pubxml`, stashes, and the deployment branch untouched.

## 8. PR-ready description (paste)

> **Identity hardening (post-go-live) — B1, B4, Edit_quatation_v2 enumeration; B2 preparation only.** Identity-hardening worktree baseline is PR #94 merge `8330a48`; `July_to_Sept26_DevNSupport` remains at `4a37676`. Adds `db/user_id_uniqueness_hardening.sql` (audit-first, DBA-only, no data mutation) so login's `SELECT TOP 1` on `User_Id` becomes well-defined; converts nine legacy user enumerations from the home-company predicate to the established `UserCompanyAccess` membership join so members of the operated company appear in assignment/approval surfaces (value columns and exclusions unchanged); fixes the `Edit_quatation_v2.BindDropdowns` global all-user enumeration (only enumeration with no company boundary) to the current-company membership join without activating commented binding code; records the `AuthGuard.HasPermission` fallback gap and an implementation-ready membership-scoped design, plus regression matrix R1–R7, in this document only — `AuthGuard.cs` is not modified (empty diff; no comment marker and no behavior change; protected asset; requires architectural review). Deferred: role architecture (B3), `srch_dailyrpts` Name→User_Id resolution, per-membership HR attributes, B5 UX. Deployment configuration untouched; deployment branch untouched; no merge/push performed.

## 9. Approvals required before execution

| Item | Approval | Executor |
|---|---|---|
| B1 PASS 1 + PASS 2 on UAT, then production | DBA (post-go-live window) | DBA |
| B2 behavior change (fallback scoping) | Architectural review (protected `AuthGuard`; ADR-001 precedent) | Cursor, after sign-off |
| Push / open PR for this branch | Repository owner | — |
| Any production execution | Explicit operator approval, never from this task | — |
