# Handoff

Lightweight working status. Prefer this file over chat history. Update the sections below when work starts or finishes. Do not duplicate [`ARCHITECTURE.md`](ARCHITECTURE.md) or [`22_Security_Baseline.md`](22_Security_Baseline.md).

---

## Current Status

Cross-Tenant Duplication **v1.1** is **completed** on `July_to_Sept26_DevNSupport` at `88f9d0f` (PR **#92** merge of Skip Already Duplicated). Baseline reports: [`44_CrossTenant_Final_Integration.md`](44_CrossTenant_Final_Integration.md), [`45_SkipExisting_BulkDuplication.md`](45_SkipExisting_BulkDuplication.md).

Cross-Tenant Duplication **relationships** are **completed and published** on `July_to_Sept26_DevNSupport` at `282cfce67a96f0a057cda7553beceb136ef15e50`. The push succeeded, and local HEAD and `origin/July_to_Sept26_DevNSupport` are synchronized. Delivered: persistent `dbo.tbl_CrossTenantDuplication` schema, relationship write inside the existing duplication transaction, authorized bidirectional counterpart display on View Vendor / View Customer, and historical labeling when the counterpart record no longer exists. UAT validated. Limitation: Customer-side R7 (no counterpart shown without access to the counterpart company) was not independently exercised, because the restricted UAT role could not open `View_client.aspx`. That is a test-access gap, not an implementation failure. Record: [`47_CrossTenant_Duplication_Relationship_Architecture.md`](47_CrossTenant_Duplication_Relationship_Architecture.md).

PR **#84** Switch User work remains on this branch (first parent `fa777e9`; merge commit `d67e854` on `master`). Impersonation runtime was preserved during the duplication merge. ADR-001 unchanged (`AuthGuard` / `SecurePage` / `UserCompanyAccess` / fail-closed). Runtime lives only in `Bill_Software/corporate/business/app/SwitchUser.aspx.cs`.

Switch User post-Start runtime failure is **closed**.

**Root cause:** after `ImpersonationRuntime.Start`, `Session["USERID"]` is the target. `AuthGuard.HasPermission("SwitchUser")` reads that identity, so `EnsurePage(..., PermissionKey)` 403'd the impersonated session. `Redirect(..., false)` without `CompleteRequest()` let the Switch User request keep running.

**Resolution:**
- Permission key is omitted only when `Session[ImpersonationGovernance.SessionLinkKey] != null`. That key is written solely by `ImpersonationRuntime.ApplyTargetSession` and removed on rollback. Session + company still run (`EnsurePage(this, true, null)`).
- Successful Start: `Response.Redirect("~/corporate/business/app/home.aspx", false)` then `Context.ApplicationInstance.CompleteRequest()`.

**PASS matrix:** initiate impersonation; End Impersonation banner (`ActorHoldsSwitchUser` / `CloseCurrent(ManualRollback)`); INV-13 nested (page hides list; `GateActor` still denies); redirect has no handler code after `CompleteRequest`; rollback restores actor identity, `CompanyID` never swapped; non-impersonating users without `SwitchUser` still 403.

Attendance regularization → rules-engine integration is **closed** (implementation + functional UAT). Architecture: [`ARCHITECTURE.md`](ARCHITECTURE.md) scheduler row. Callers: [`page-catalog/SHARED_RUNTIME.md`](page-catalog/SHARED_RUNTIME.md), [`page-catalog/UAT_CATALOG.md`](page-catalog/UAT_CATALOG.md). Domain pages: [`page-catalog/DOMAIN_attendance-hr.md`](page-catalog/DOMAIN_attendance-hr.md).

---

## Completed Work

- v2.1 Security Foundation (AuthGuard, SecurePage, tenant membership, resource scope phases). Contract: `docs/22`.
- Page catalog + data dictionary + UAT catalog generators.
- Impersonation governance + dormant runtime + UAT activation docs (`ADR-001`, `docs/29`–`docs/37`).
- Switch User runtime patch: `SessionLinkKey` permission exception + `CompleteRequest` redirect. Investigation closed (no remaining Switch User runtime work).
- Cross-Tenant Duplication PR-1–PR-4 merged at `577ebcf` (`DuplicationService`, View Vendor/Customer single + bulk, live `WriteDuplicationAudit`). Docs: `34` CrossTenant, `37` completion, `41`–`43` UAT, `44` integration.
- Cross-Tenant Duplication **v1.1** completed at `88f9d0f` (PR **#92**): bulk Skip Already Duplicated. Spec: `docs/45`.
- Cross-Tenant Duplication relationships published at `282cfce` (persistent source → target links, transactional write, authorized bidirectional display, historical counterpart handling; UAT validated with Customer-side R7 not independently exercised). Spec: `docs/47`.
- Ponytail + solution-docs Cursor rules (existing).
- This bootstrap: `docs/ARCHITECTURE.md`, `docs/HANDOFF.md`, `docs/CURSOR_RULES.md`, `Bill_Software/.cursor/rules/project.mdc`.
- Attendance regularization approval → `dbo.sp_RunAttendanceRulesEngine` (same C# `SqlTransaction` as the approval write) on `AdminApprovalDashboard` / `AdminOverride` / `QuickAction` when **Reg + Approved** only. Leave approve/reject and Reg reject do **not** call the engine. `attendance.aspx` punch path unchanged. `AdminOverride` / `QuickAction`: `RequestStatus = 'Pending'` on UPDATE; zero rows → SQL `THROW 50001` into C# rollback + controlled UI error (`Debug.WriteLine` keeps the exception server-side). `AdminApprovalDashboard` retains its existing Pending + `@@ROWCOUNT` abort. Alerts only after `Commit`. Request rows in `tbl_AttendanceRegularization` remain independently auditable. Debug and Release builds succeeded. **UAT PASS (functional):** Reg approval recalculates dependent fields; calendar / Day Detail consistent; Reg rejection and leave approval unchanged (no engine). **Deferred validation (not PASS):** failure-injection rollback of the engine, simultaneous dual-approval race, AdminApprovalDashboard concurrency review.

---

## Current Task

None. Attendance rules-engine integration milestone is closed. Optional next work is resilience validation only (see Next Action).

---

## Pending Work

- Known debt (do not start unless requested): leftover concatenated SQL on invoice / PO / stock / scheduler surfaces; kiosk plaintext; some direct `SmtpClient` pages; UAT objects not applied.
- Product stops still closed: Decision #8 (`ReportingManagerId` ACL), Decision #9 (HQ cross-company), `machineKey` rotation, secret cutover ops.

---

## Constraints

- Repository-first. Ignore chat unless summarized here or in `ARCHITECTURE.md` / `CURSOR_RULES.md`.
- Preserve architecture in `ARCHITECTURE.md` and `docs/22`. No parallel AuthN, no JWT, no Identity, no Windows Service host.
- Modify only files explicitly referenced. Unified diffs only.
- Parameterized SQL + `CompanyID` (or documented ownership). No static user state in `.aspx.cs`.
- Do not change `AuthGuard`, `SecurePage`, `UserRoleAssignment`, or `UserCompanyAccess` without architectural review.
- Do not edit README.md or CONTRIBUTING.md unless a task names them.
- No secrets in diffs, docs, or commits.

---

## Next Action

Optional resilience validation for attendance Reg approval (explicitly **deferred**, not claimed as UAT PASS): (1) inject `sp_RunAttendanceRulesEngine` failure and confirm approval rolls back with controlled UI error and no success alert; (2) simultaneous dual approval (Override vs QuickAction or two managers) and confirm only one wins Pending + no double engine run; (3) AdminApprovalDashboard concurrency review against the same Pending race model. Do not start unless requested.

Verified repository state: HEAD is `4a37676` (`Document published cross-tenant duplication relationships`) on `July_to_Sept26_DevNSupport`, synchronized with `origin/July_to_Sept26_DevNSupport` (0 ahead / 0 behind). `5ace354` (menu membership by company) is historical only — an ancestor of HEAD, ten commits behind the tip; its menu UAT (Case 2 **BLOCKED**) stays historical and is not the current publication state. Attendance rules-engine integration (callers, Pending/`THROW` guards, these docs) is an uncommitted working-tree change on top of `4a37676`. Commit and push still need separate approval. Do not reopen Switch User **runtime** unless a new defect is named. Cross-Tenant Duplication v1.1 remains closed at `88f9d0f`. Cross-Tenant Duplication relationships are closed and published at `282cfce` (included in HEAD `4a37676`); optional follow-up only: exercise Customer-side R7 with a restricted role that can open `View_client.aspx`.

---

## Notes

- Shared AuthN/tenancy lives in `page-catalog/SHARED_CONTEXT.md`; do not copy it into domain catalogs.
- Module docs `01`–`13` are leftover pointers. Sales-visit audit is a dated snapshot (`sales-visit-workflow-audit/00_SNAPSHOT_STATUS.md`).
- Replace the Current Task / Pending Work / Next Action bullets when work is assigned. Delete stale chat-derived claims; this file is the source of session truth.
