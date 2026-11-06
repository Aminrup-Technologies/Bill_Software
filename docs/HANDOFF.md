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

Add Proforma company-scoped client join is **completed** (investigation, implementation, browser UAT; 2026-11-06). **Not committed:** the change is in the working tree of `feature/add-proforma-ui-refactor` at `9140326`, alongside the earlier uncommitted Add Proforma service-summary `OUTER APPLY` fix in the same file.

**Defect:** the three quotation searches in `Bill_Software/corporate/business/app/Add_proforma.aspx.cs` (Only Client, Only Date, Client & Date) joined `tbl_Client` on `Client_Id` alone. `tbl_Client` has no unique key on `Client_Id` or (`Client_Id`, `CompanyID`). Read-only `flamex_uat` checks as `flmx_mcp_reader` found 7 codes in both companies, one row each (AD02, AD03, AD04, AD05, AD06, AD08, AD09); no same-company duplicates. Five Company 1 quotations used three of those codes, so each returned a second row with the Company 2 client name: `PO/FE/26-27/2`, `QTN/FE/25-26/13`, `/54`, `/60`, `/61`. The single Company 2 quotation `QTN/AA/26-27/1` uses `AD01`, which exists only in Company 1 (`3AP POLYMERS`).

**Resolution:** all three joins are `LEFT OUTER JOIN tbl_Client c ON qut.Client_Id = c.Client_Id AND c.CompanyID = qut.CompanyID`. Projections, filters, parameters, ordering, and binding are unchanged. LEFT JOIN keeps quotations without a same-company client visible with a blank client name; there is no fallback to another company's client. MSBuild 14 Debug build succeeded.

**Browser UAT PASS** (Search and Reset only; no quotation selected, no Save, no business data modified):

| # | Company | Search | Result |
|---|---|---|---|
| 1 | 1 | Only Date, 01-Jan-2020 to 06-Nov-2026 | PASS: 158 rows, 158 distinct quotations, no duplicates, no blank client names |
| 2 | 1 | Only Client, ACC LIMITED (Sindri) | PASS: 3 (`QTN/FE/25-26/61`, `/60`, `/13`) |
| 3 | 1 | Only Client, ACC Limited (CHAIBASA) | PASS: 1 (`PO/FE/26-27/2`) |
| 4 | 1 | Only Client, AMALGAM STEEL & POWER LIMITED | PASS: 1 (`QTN/FE/25-26/54`) |
| 5 | 1 | Client & Date, ACC LIMITED (Sindri), 01-Jul-2025 to 31-Jul-2025 | PASS: 2 (`QTN/FE/25-26/61`, `/60`) |
| 6 | 1 | Client & Date, ACC LIMITED (Sindri), 01-Jan-2020 to 31-Dec-2020 | PASS: "No Data Found..." |
| 7 | 2 | Only Date, 01-Jan-2020 to 06-Nov-2026 | PASS: `QTN/AA/26-27/1` visible with a blank client name; `3AP POLYMERS` not shown |
| 8 | 2 | Only Client, Heatworks Pvt. Ltd. (`AD02`) | PASS: "No Data Found..."; Company 1 `PO/FE/26-27/2` not returned |

The Company 1 full-range count fell from 163 rows to 158: the five removed rows were the cross-company client matches. `QTN/FE/25-26/62`, `/63`, `/64` legitimately show "Heatworks Pvt. Ltd." because they use Company 1's own client `AD423`; that is not cross-company leakage. `PO/FE/25-26/19` (TATA STEEL LTD.(Jamshedpur), SUPPLY OF LUBRICANT) and `QTN/FE/25-26/69` (2M INTERNATIONAL, SUPPLY OF LIGHTS) were also correct.

**Limitations:** blank service categories on `QTN/FE/25-26/31A`, `PO/FE/25-26/59`, `PO/FE/25-26/60` are pre-existing and unrelated to the client join. `QTN/AA/26-27/1` keeps `Client_Id = AD01` and can be found in Company 2 only by Only Date; repointing it is a separate data change needing its own write approval. Nothing in the schema prevents future duplicate (`Client_Id`, `CompanyID`) rows.

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
- Add Proforma company-scoped client join (2026-11-06; uncommitted on `feature/add-proforma-ui-refactor`): all three searches in `Add_proforma.aspx.cs` join `tbl_Client` on `qut.Client_Id = c.Client_Id AND c.CompanyID = qut.CompanyID`. Read-only `flamex_uat` investigation, MSBuild 14 Debug build, and eight-test browser UAT in Company 1 and Company 2 all PASS. Details and limitations: Current Status above.

---

## Current Task

**GATE 2 BLOCKED.** Attendance rules-engine integration stays closed. Resilience validation stays deferred (see Next Action). Do not start Gate 3.

---

## Pending Work

- Known debt (do not start unless requested): leftover concatenated SQL on invoice / PO / stock / scheduler surfaces; kiosk plaintext; some direct `SmtpClient` pages; UAT objects not applied.
- Product, stock, purchase, and invoice ownership implementation stays blocked. Live read-only check of `flamex_uat` on 2026-10-01 is in [`architecture/OWNERSHIP_AUDIT.md`](architecture/OWNERSHIP_AUDIT.md). D1–D8 labels were not in the tree when the audit ran; they are now ratified in [`architecture/ADR-002_MultiTenancy_Ownership_Model.md`](architecture/ADR-002_MultiTenancy_Ownership_Model.md). `UserCompanyAccess` is present on this UAT copy (28 active rows), which contradicts the 8 Sep `SHARED_SCHEMA.md` “missing on UAT” sentence. Do not treat that snapshot sentence as current. Do not start schema or tenancy code from the verification.
- Multi-tenancy target (working tree, not published HEAD): D1–D8, S1, S2, and Amendment A1 in [`architecture/ADR-002_MultiTenancy_Ownership_Model.md`](architecture/ADR-002_MultiTenancy_Ownership_Model.md) and [`architecture/UAT_MIGRATION_GOVERNANCE.md`](architecture/UAT_MIGRATION_GOVERNANCE.md), dated 2026-10-01. Those files are untracked, so Gate 1 is not merged at `4918b977`. No approver name is recorded. A1 stands: invoice payment / due / TDS are children of the invoice; `tbl_StockAdjustments` is a tenant-owned movement on shared stock at `ProductID + StoreID`; `tbl_stock.Quantity_Num` is the only quantity of record.
- **GATE 2 BLOCKED** (2026-11-05). Exit criteria are the Gate 2 column in governance §2. Inventory: [`architecture/GATE2_DEPENDENCY_IMPACT_REPORT.md`](architecture/GATE2_DEPENDENCY_IMPACT_REPORT.md). `dbo.tbl_CrossTenantDuplication` is §16.6 row 29 (Open; not classified; not approved). Supplied `flamex_uat` collection `2026-11-05T02:55:01.975Z` recorded 22/22 module bodies in report §12.2 (`OBJECT_DEFINITION()`, UAT only). Failing criteria: SQL Agent job-step text was not returned (`flamex_uat` MCP `execute_query` is disabled; no `msdb.dbo.sysjobsteps` rows); X-18 read-only `flamex_live` census of every ADR-002 §2 and §7 object, including tables empty on UAT, was not run; repository-owner approval, which is an explicit statement in this file and has not been given. X-19 live re-verification was not run. X-20, X-21, and X-22 are unchanged. Gate 3 decisions X-23, X-24, X-25, X-28, X-31 stay open (report §16). Scripts are written in Gate 3 and first run in Gate 5 on `flamex_uat` only. Application adaptation is Gate 7. Production is a replay under its own approval. Gate 3 is not authorized. `employee_config_permissions.sql` is parked and is outside this gate.
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

Verified repository state: published HEAD is `4918b977` (`Recalculate attendance when a regularization is approved.`) on `July_to_Sept26_DevNSupport`. Attendance rules-engine integration is in that commit. `4a37676` is an ancestor, not HEAD. `5ace354` (menu membership by company) stays historical. Do not reopen Switch User **runtime** unless a new defect is named. Cross-Tenant Duplication v1.1 remains closed at `88f9d0f`. Cross-Tenant Duplication relationships are closed and published at `282cfce`. Optional follow-up only: exercise Customer-side R7 with a restricted role that can open `View_client.aspx`. Gate 2 disposition did not edit `Web.config`, the UAT publish profile, `AdminShiftAssignment.aspx.cs`, or `newproduct_master.aspx`.

---

## Notes

- Shared AuthN/tenancy lives in `page-catalog/SHARED_CONTEXT.md`; do not copy it into domain catalogs.
- Module docs `01`–`13` are leftover pointers. Sales-visit audit is a dated snapshot (`sales-visit-workflow-audit/00_SNAPSHOT_STATUS.md`).
- Replace the Current Task / Pending Work / Next Action bullets when work is assigned. Delete stale chat-derived claims; this file is the source of session truth.
