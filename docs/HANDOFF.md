# Handoff

Lightweight working status. Prefer this file over chat history. Update the sections below when work starts or finishes. Do not duplicate [`ARCHITECTURE.md`](ARCHITECTURE.md) or [`22_Security_Baseline.md`](22_Security_Baseline.md).

---

## Current Status

Cross-Tenant Duplication **v1.1** is **completed** on `July_to_Sept26_DevNSupport` at `88f9d0f` (PR **#92** merge of Skip Already Duplicated). Baseline reports: [`44_CrossTenant_Final_Integration.md`](44_CrossTenant_Final_Integration.md), [`45_SkipExisting_BulkDuplication.md`](45_SkipExisting_BulkDuplication.md).

PR **#84** Switch User work remains on this branch (first parent `fa777e9`; merge commit `d67e854` on `master`). Impersonation runtime was preserved during the duplication merge. ADR-001 unchanged (`AuthGuard` / `SecurePage` / `UserCompanyAccess` / fail-closed). Runtime lives only in `Bill_Software/corporate/business/app/SwitchUser.aspx.cs`.

Switch User post-Start runtime failure is **closed**.

**Root cause:** after `ImpersonationRuntime.Start`, `Session["USERID"]` is the target. `AuthGuard.HasPermission("SwitchUser")` reads that identity, so `EnsurePage(..., PermissionKey)` 403'd the impersonated session. `Redirect(..., false)` without `CompleteRequest()` let the Switch User request keep running.

**Resolution:**
- Permission key is omitted only when `Session[ImpersonationGovernance.SessionLinkKey] != null`. That key is written solely by `ImpersonationRuntime.ApplyTargetSession` and removed on rollback. Session + company still run (`EnsurePage(this, true, null)`).
- Successful Start: `Response.Redirect("~/corporate/business/app/home.aspx", false)` then `Context.ApplicationInstance.CompleteRequest()`.

**PASS matrix:** initiate impersonation; End Impersonation banner (`ActorHoldsSwitchUser` / `CloseCurrent(ManualRollback)`); INV-13 nested (page hides list; `GateActor` still denies); redirect has no handler code after `CompleteRequest`; rollback restores actor identity, `CompanyID` never swapped; non-impersonating users without `SwitchUser` still 403.

---

## Completed Work

- v2.1 Security Foundation (AuthGuard, SecurePage, tenant membership, resource scope phases). Contract: `docs/22`.
- Page catalog + data dictionary + UAT catalog generators.
- Impersonation governance + dormant runtime + UAT activation docs (`ADR-001`, `docs/29`–`docs/37`).
- Switch User runtime patch: `SessionLinkKey` permission exception + `CompleteRequest` redirect. Investigation closed (no remaining Switch User runtime work).
- Cross-Tenant Duplication PR-1–PR-4 merged at `577ebcf` (`DuplicationService`, View Vendor/Customer single + bulk, live `WriteDuplicationAudit`). Docs: `34` CrossTenant, `37` completion, `41`–`43` UAT, `44` integration.
- Cross-Tenant Duplication **v1.1** completed at `88f9d0f` (PR **#92**): bulk Skip Already Duplicated. Spec: `docs/45`.
- Ponytail + solution-docs Cursor rules (existing).
- This bootstrap: `docs/ARCHITECTURE.md`, `docs/HANDOFF.md`, `docs/CURSOR_RULES.md`, `Bill_Software/.cursor/rules/project.mdc`.

---

## Current Task

Switch User deployment contract: canonical [`SWITCHUSER_DEPLOYMENT_CHECKLIST.md`](SWITCHUSER_DEPLOYMENT_CHECKLIST.md). Committed at `1bb4754` on `cursor/impersonation-membership-runtime` (5 ahead / 0 behind `July_to_Sept26_DevNSupport`). **Not pushed.**

Local state (not in git):
- `Bill_Software/Web.config` and `flamexuat … Web Deploy.pubxml` hold local UAT overlay values (connection string, `SwitchUser=true`). Never stage or commit; do not use `git commit -a`.
- `stash@{0}` ("undocumented menu/SwitchUser membership changes … pending review"): still holds `AuthGuard.cs`, `Bill.Master.cs`, `SwitchUser.aspx.cs` edits (stash not mutated).
  - `SwitchUser.aspx.cs` portion restored via `git restore --source` and committed with the Switch User work: target list drops the `tbl_login.CompanyID` home-company filter, matching `ImpersonationRuntime.LoadEligibleTarget` (INV-15, `5e0a550`). Do not re-apply it from the stash.
  - `AuthGuard.cs` (`GetMenuPermissions`) + `Bill.Master.cs` (menu and Switch User link from `UserCompanyAccess`) remain parked. Coupled (Bill.Master calls the new AuthGuard method). Conflicts with ADR-001 "`AuthGuard` unchanged"; needs separate architectural review before apply or drop. Compare `cursor/company-membership-menu` (`cffc720`), which fixes the menu without touching AuthGuard.
- `stash@{1}` Cursor Desktop UAT workspace; `stash@{2}` uat-pr2 challanwriter WIP; `stash@{3}` local docs/config before Cross-Tenant UAT. Stash indices shift when a stash is added or dropped; identify stashes by message.

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

Push of `1bb4754` requires separate explicit approval. Decide the fate of the parked `AuthGuard.cs` + `Bill.Master.cs` changes in `stash@{0}` (separate architectural review task, or drop) before any menu/membership work. Do not reopen Switch User **runtime** unless a new defect is named. Do not push unless separately approved. Cross-Tenant Duplication v1.1 remains closed at `88f9d0f`.

---

## Notes

- Shared AuthN/tenancy lives in `page-catalog/SHARED_CONTEXT.md`; do not copy it into domain catalogs.
- Module docs `01`–`13` are leftover pointers. Sales-visit audit is a dated snapshot (`sales-visit-workflow-audit/00_SNAPSHOT_STATUS.md`).
- Replace the Current Task / Pending Work / Next Action bullets when work is assigned. Delete stale chat-derived claims; this file is the source of session truth.
