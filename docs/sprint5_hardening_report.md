# Sprint 5 Security & Hardening Completion Report

| Field | Value |
|---|---|
| Repository | `Aminrup-Technologies/Bill_Software` |
| Base branch | `July_to_Sept26_DevNSupport` |
| Base HEAD at report time | `04c43dbf804c840a8abc38984a9fc67703301117` (merge of PR #49; previous #48 `3b4c42b9b8c44b293a0564bfb6aa34b40787ce86`; previous #51 `271c8cdd29db1e253970f92243aeba61fa9a5b72`) |
| Reporting date | 2026-09-06 |
| Scope | Purchase Order and Invoice tenant isolation, SQL parameterization, current-row print/export parity, ViewType persistence (PRs #48–#55) |
| Reviewer | AMINRUP TECHNOLOGIES |
| Status | Security remediation complete (code delivered in PRs #48–#55; merge status below) |

This report is documentation only. It does not change application SQL or schema. Merge SHAs are recorded only where GitHub shows a merge commit on `July_to_Sept26_DevNSupport`. Open PRs list feature HEAD SHAs for traceability, not as merge SHAs.

---

## 1. Merge history

| PR | Area | Merge SHA | Status |
|---|---|---|---|
| #48 | Delete Purchase Order Isolation | `3b4c42b9b8c44b293a0564bfb6aa34b40787ce86` | MERGED 2026-09-05. Feature HEAD `d62bf97072e5e576b5b7f9e8ab8c3bc0bf3f0c63`. https://github.com/Aminrup-Technologies/Bill_Software/pull/48 |
| #49 | Search Purchase Order Isolation | `04c43dbf804c840a8abc38984a9fc67703301117` | MERGED 2026-09-06. Feature HEAD `666f29e7723aa92d76d13b93354943e84745321a`. https://github.com/Aminrup-Technologies/Bill_Software/pull/49 |
| #50 | View Purchase Order Isolation | — (not on base) | OPEN. Feature HEAD `2a1b24e50e75ebf02f152bb54d188a6a4e30f82f`. https://github.com/Aminrup-Technologies/Bill_Software/pull/50 |
| #51 | Edit Purchase Order ViewType + SalesPerson | `271c8cdd29db1e253970f92243aeba61fa9a5b72` | MERGED 2026-09-05. Feature HEAD `b76c09848d483005c40ee2ec89ef68e97309d658`. https://github.com/Aminrup-Technologies/Bill_Software/pull/51 |
| #52 | Print Current-Row Consistency | — (not on base) | OPEN. Feature HEAD `51c23ac43320ae19c7e9bd68623b8863f0222254`. https://github.com/Aminrup-Technologies/Bill_Software/pull/52 |
| #53 | View Export Current-Row Consistency | — (not on base) | OPEN. Feature HEAD `9da3f2308f1a2a470c830852113747f388c5a070`. https://github.com/Aminrup-Technologies/Bill_Software/pull/53 |
| #54 | InvoiceMail Isolation | — (not on base) | OPEN. Feature HEAD `03fb796f0cd55be7ebfac390ea1663ea6da01837`. https://github.com/Aminrup-Technologies/Bill_Software/pull/54 |
| #55 | Delete Invoice Isolation | — (not on base) | OPEN. Feature HEAD `7baf986dc3d36eeab8913ea1ac15804c0c719bbd`. https://github.com/Aminrup-Technologies/Bill_Software/pull/55 |

Recommended merge order remaining: **#50 → #52 → #53 → #54 → #55**. #53 depends on the current-row rule introduced in #52. #50 and #53 both touch `View_PurchaseOrder.aspx.cs` and should be merged sequentially.

---

## 2. Change summary

### PR #48 — Delete Purchase Order Isolation

**Purpose.** Stop cross-tenant Purchase Order search/delete and SQL concatenation on `delete_purchaseorder.aspx.cs`.

**Files changed.** `Bill_Software/corporate/business/app/delete_purchaseorder.aspx.cs`

**Security issue addressed.** Concatenated `Client_Id` / dates / `Quotation_no`; unscoped client combo; DELETE without `CompanyID` on tenant tables; header load by `ID` alone (IDOR).

**Business impact.** Same-company client / date / client+date search and sequential child deletes unchanged. Cross-company rows no longer list or delete. `tbl_quotation_vat` and `tbl_QutSiteAddress` remain parameterized by quotation number only (no `CompanyID` column).

**Risk level.** Critical (before). Residual: tables without `CompanyID` still delete by quotation number after a CompanyID-scoped header check.

**Rollback impact.** Revert the single `.cs` file. No schema.

### PR #49 — Search Purchase Order Isolation

**Purpose.** Parameterize Purchase Order Search and scope client combo, lookup, search, and header load by `CompanyContext.CurrentCompanyID`.

**Files changed.** `Bill_Software/corporate/business/app/Search_purchaseorder.aspx.cs`

**Security issue addressed.** Concatenated client IDs, names, and dates; unscoped client combo and `buindalldata`.

**Business impact.** Client / date / client+date radio behavior preserved. **Export SQL was not modified** (explicit in the PR).

**Risk level.** High (before). **Merged.** Residual: Search Excel export still joins `tbl_Quotaion_details` with `qd.IsDeleted = 0` only (no child `CompanyID`, no `IsLatest`).

**Rollback impact.** Revert merge `04c43dbf804c840a8abc38984a9fc67703301117`. No schema.

### PR #50 — View Purchase Order Isolation

**Purpose.** Block cross-company View click-through and scope autocomplete WebMethods.

**Files changed.** `Bill_Software/corporate/business/app/View_PurchaseOrder.aspx.cs`

**Security issue addressed.** View redirect by ID without `CompanyID`; unscoped `GetClientNames`; concatenated CompanyID in autocomplete; WebMethods without session.

**Business impact.** Same-company View still opens print. Cross-company ID is a no-op. BindData grid and Export were left unchanged in this PR (export current-row is #53).

**Risk level.** High (before).

**Rollback impact.** Revert the single `.cs` file. No schema.

### PR #51 — Edit Purchase Order ViewType + SalesPerson

**Purpose.** Persist `DetailedView` on Save Changes and scope SalesPerson to the active company.

**Files changed.** `Bill_Software/corporate/business/app/edit_purchaseorder.aspx.cs`

**Security issue addressed.** SalesPerson list was global (`IsActive = 1` only). Save Changes did not write `DetailedView`; `--SELECT--` must keep the existing DB value.

**Business impact.** Calculations, MagicianNew, and ownership checks unchanged. Print/export markup unchanged.

**Risk level.** High (before). **Merged.**

**Rollback impact.** Revert merge `271c8cdd29db1e253970f92243aeba61fa9a5b72`. No schema.

### PR #52 — Print Current-Row Consistency

**Purpose.** Align print current-line selection and Create-PO detail writers with MagicianNew flags and Sl_no order.

**Files changed.**
- `Bill_Software/corporate/business/print/PurchaseOrderPrintHelper.cs`
- `Bill_Software/corporate/business/app/Create_quotation.aspx.cs`

**Security issue addressed.** Print used `IsLatest = 1 AND IsDeleted = 0` (NULL flags dropped) and `ORDER BY ItemNo` (string). Create INSERT omitted `Version` / `IsDeleted` / `IsLatest`.

**Business impact.** Current-row rule: `ISNULL(IsLatest,1)=1 AND ISNULL(IsDeleted,0)=0`. Order: `CAST(Sl_no as int)`. Create writes `Version=1, IsDeleted=0, IsLatest=1`. MagicianNew archives stay excluded. No historical backfill. A4 renderer unchanged.

**Risk level.** High (before).

**Rollback impact.** Revert the two `.cs` files. No schema. Historical Create rows remain NULL until a future edit save.

### PR #53 — View Export Current-Row Consistency

**Purpose.** Match View Purchase Order Excel detail JOIN to the print current-row rule from #52.

**Files changed.** `Bill_Software/corporate/business/app/View_PurchaseOrder.aspx.cs`

**Security issue addressed.** Export JOIN was `qd.IsDeleted = 0` only (no child `CompanyID`, no `IsLatest`; NULL `IsDeleted` excluded).

**Business impact.** LEFT JOIN preserved. `ORDER BY CAST(qd.Sl_no as int)` unchanged. BindData, ClosedXML, filenames, and column order unchanged.

**Risk level.** High (before).

**Rollback impact.** Revert the JOIN predicate. No schema.

### PR #54 — InvoiceMail Isolation

**Purpose.** Scope every tenant-owned lookup on Invoice Mail to `CompanyContext.CurrentCompanyID`.

**Files changed.** `Bill_Software/corporate/business/app/InvoiceMail.aspx.cs`

**Security issue addressed.** Unscoped client combo, invoice search, client lookup, client/representative details, Primary Service by `qut_no` only, invoice ID lookup, and `mailStatus` UPDATE.

**Business impact.** Email HTML and subject template unchanged. Invoice snapshot writers (`Add_invoice`) unchanged. Search still concatenates client ID and dates; CompanyID is appended as an integer from session.

**Risk level.** High (before). Residual: remaining concatenation on client/date filters (not CompanyID bypass).

**Rollback impact.** Revert the single `.cs` file. No schema.

### PR #55 — Delete Invoice Isolation

**Purpose.** Parameterize Delete Invoice search/delete and require same-company ownership before quotation status updates.

**Files changed.** `Bill_Software/corporate/business/app/Delete_invoice.aspx.cs`

**Security issue addressed.** Concatenated `Client_Name`, dates, `Invoice_No`, product keys; unscoped client combo; DELETE/UPDATE without `CompanyID`; IDOR on `Invoice_No`.

**Business impact.** Client / date / client+date search preserved (`txttodate` still from, `txtfromDate` still to). Same-company delete still sets quotation `Status2='No'` and detail `InvStatus='No'`, then deletes invoice, details, and `tbl_InvSiteAddress`. Cross-company `Invoice_No` is a no-op (`No Data Found...`). Search/View Invoice and export files unchanged.

**Risk level.** Critical (before).

**Rollback impact.** Revert the single `.cs` file. No schema.

---

## 3. Security matrix

| Control | Before | After | Affected PR |
|---|---|---|---|
| CompanyID isolation | PO delete/search/view/edit SalesPerson, InvoiceMail, Invoice delete, and PO export detail JOIN omitted tenant predicates on one or more queries | Session `CompanyContext.CurrentCompanyID` on tenant tables that have the column | #48, #49, #50, #51, #53, #54, #55 |
| SQL injection removal | Concatenated client, date, quotation, invoice, and product values | Parameterized `@ClientId`, `@FromDate`, `@ToDate`, `@QuotationNo`, `@Invoice_No`, `@prefix`, etc. InvoiceMail search still concatenates client ID and dates with numeric CompanyID | #48, #49, #50, #54, #55 |
| IDOR prevention | PO header/print View and Invoice delete keyed by ID / Invoice_No alone | `ID + CompanyID` before PO print redirect; Invoice delete gated on `Invoice_No + CompanyID` | #48, #50, #55 |
| ViewType persistence | Save Changes did not write `tbl_Quotation.DetailedView` | Save writes `DetailedView=@DView`; `--SELECT--` keeps existing DB value | #51 |
| Primary Service snapshot | Invoice Excel snapshot column and create-time write already on base (PR #45). InvoiceMail read live `tbl_QutPrimaryService` unscoped | InvoiceMail scopes live lookup by `qut_no + CompanyID`. Snapshot INSERT path not changed in #48–#55 | #54 (mail). Snapshot schema/backfill: #45 / `db/pservice_snapshot.sql` |
| Print/Export parity | Print: `IsLatest=1 AND IsDeleted=0`, `ORDER BY ItemNo`. View export: `IsDeleted=0` only | Print and View export: `ISNULL(IsLatest,1)=1 AND ISNULL(IsDeleted,0)=0`, `CAST(Sl_no as int)` | #52, #53 |
| Autocomplete isolation | Client names unscoped; CompanyID concatenated into WebMethod SQL | `EnableSession`, `@CompanyID`, `@prefix`; empty list if no `USERID` | #50 |
| Mail isolation | Primary Service and invoice ID unscoped | All tenant lookups + `mailStatus` UPDATE include CompanyID | #54 |
| Invoice deletion safety | DELETE by `Invoice_No` string; quotation Status2 / InvStatus unscoped | Parameterized DELETE with CompanyID; ownership SELECT before status updates | #55 |

---

## 4. Module coverage

| Module | Status |
|---|---|
| Purchase Order Delete | **Merged** PR #48 (`3b4c42b9b8c44b293a0564bfb6aa34b40787ce86`) |
| Purchase Order Search | **Merged** PR #49 (`04c43dbf804c840a8abc38984a9fc67703301117`). Search/combo/lookup parameterized and CompanyID-scoped. Export SQL not in #49 |
| Purchase Order View | Complete in PR #50 for View click + autocomplete (awaiting merge) |
| Purchase Order Edit | **Merged** PR #51 |
| Purchase Order Print | Complete in PR #52 (awaiting merge) |
| Purchase Order Export | Complete in PR #53 for View export current-row (awaiting merge). Search PO export not in this series |
| Invoice Search | Already parameterized and `a.CompanyID`-scoped on base (prior sprint). Not modified in #48–#55 |
| Invoice View | Already parameterized and `a.CompanyID`-scoped on base (prior sprint). Not modified in #48–#55 |
| Invoice Export | 33 columns, Invoice Source, `a.PServiceName` snapshot, Export_Info, Invoice_Source_Summary already on base (PRs #45/#47). Not modified in #48–#55 |
| Invoice Mail | Complete in PR #54 (awaiting merge) |
| Invoice Delete | Complete in PR #55 (awaiting merge) |

### Remaining technical debt (not security defects in this series)

| Item | Evidence |
|---|---|
| Search Purchase Order Excel export JOIN | PR #49: export SQL explicitly unchanged (`qd.IsDeleted = 0` only) |
| InvoiceMail client/date concatenation | PR #54 adds CompanyID; client ID and dates remain concatenated |
| `tbl_quotation_vat` / `tbl_QutSiteAddress` have no CompanyID | PR #48: parameterized by quotation number only |
| Search/View Invoice grid joins on client / `tbl_QuoPriSerTogather` / quotation | Documented in `docs/invoice_export_data_inventory.md`; header still `a.CompanyID` |
| Historical NULL `IsLatest` on Create-era PO lines | PR #52: no data backfill; ISNULL reader shim + forward writer |
| PR #56 SQL helper standardization | Modernization only; not opened as a merge |
| PR #57 DbCL inventory | Modernization only; not opened as a merge |

---

## 5. Deployment order

PRs #48–#55 contain **no database schema changes**. Application deploy is merge + IIS recycle. Optional backfills below are from **earlier** merged work (PR #44, PR #45), not invented for this series.

1. **Deploy database migrations (already required for invoice snapshot, not #48–#55).**
   - `db/pservice_snapshot.sql` — `ALTER TABLE tbl_Invoice ADD PServiceName NVARCHAR(MAX) NULL;`
   - Review before execute (file header: do not run until reviewed).

2. **Deploy application.**
   - Merge remaining OPEN PRs #50, #52–#55 onto `July_to_Sept26_DevNSupport` in the order in §1.
   - #51 is on base (`271c8cdd29db1e253970f92243aeba61fa9a5b72`). #48 is on base (`3b4c42b9b8c44b293a0564bfb6aa34b40787ce86`). #49 is on base (`04c43dbf804c840a8abc38984a9fc67703301117`).

3. **Optional backfills (review-first; default `ROLLBACK TRAN`).**
   - `docs/qutprimaryservice_companyid_backfill.sql` — after PR #44 writer. Fills NULL `tbl_QutPrimaryService.CompanyID` from unique `tbl_Quotation.CompanyID`.
   - `docs/invoice_pservice_backfill.sql` — after `db/pservice_snapshot.sql` and snapshot INSERT deploy. Fills NULL `tbl_Invoice.PServiceName` from CompanyID-scoped `tbl_QutPrimaryService` when a same-company quotation header exists.

### Verification queries (existing scripts only)

From `docs/qutprimaryservice_companyid_backfill.sql`:

- BEFORE: `NullRowsBefore`, `EligibleRows`, `AmbiguousRows`, `UnmatchedRows`
- AFTER: `RowsUpdated`, `RemainingNullRows` plus remaining-reason list

From `docs/invoice_pservice_backfill.sql`:

- BEFORE: `NullRowsBefore`, `EligibleRows`, `UnmatchedRows`
- AFTER: `RowsUpdated`, `RemainingNullRows` plus remaining-reason list

Do not `COMMIT` either script until AFTER verification is accepted. Both files default to `ROLLBACK TRAN`.

---

## 6. Rollback

| PR | Code rollback | Database rollback | Backfill considerations |
|---|---|---|---|
| #48 | Revert merge `3b4c42b9b8c44b293a0564bfb6aa34b40787ce86` | None | None |
| #49 | Revert merge `04c43dbf804c840a8abc38984a9fc67703301117` | None | None |
| #50 | Revert `View_PurchaseOrder.aspx.cs` (conflicts with #53 if both merged) | None | None |
| #51 | Revert merge `271c8cdd29db1e253970f92243aeba61fa9a5b72` | None | ViewType values written after merge remain in `DetailedView` |
| #52 | Revert `PurchaseOrderPrintHelper.cs` and `Create_quotation.aspx.cs` | None | New PO lines already written with `Version/IsDeleted/IsLatest` stay; no backfill to undo |
| #53 | Revert export JOIN in `View_PurchaseOrder.aspx.cs` | None | None |
| #54 | Revert `InvoiceMail.aspx.cs` | None | None |
| #55 | Revert `Delete_invoice.aspx.cs` | None | Deleted invoices are not restored by code revert |
| PServiceName column | Not part of #48–#55 | Dropping `tbl_Invoice.PServiceName` is a separate schema decision (`db/pservice_snapshot.sql`) | If `docs/invoice_pservice_backfill.sql` was COMMITTED, reverting the column drops snapshot data |
| QutPrimaryService CompanyID backfill | Not part of #48–#55 | If COMMITTED, NULL CompanyID rows were updated in place | Re-run is not defined; do not invent a reverse UPDATE |

If #50 and #53 are both merged, roll back #53 first (export JOIN only), then #50 (View/autocomplete).

---

## 7. Finance UAT checklist

| Check | Pass? | Notes |
|---|---|---|
| Purchase Order Search — client only | | Same-company POs only (#49) |
| Purchase Order Search — date only | | Same-company POs only (#49) |
| Purchase Order Search — client and date | | Same-company POs only (#49) |
| Purchase Order View — open own PO | | Redirect to print after `ID+CompanyID` (#50) |
| Purchase Order View — other-company ID | | No redirect (#50) |
| Purchase Order autocomplete — clients / PO / DO | | Active company only (#50) |
| Purchase Order Export (View) — line order | | `CAST(Sl_no as int)` (#53) |
| Purchase Order Export (View) — current rows | | ISNULL current-row; archives excluded (#53) |
| Purchase Order Print — line order and current rows | | Same rule as #52; A4 unchanged |
| Purchase Order Edit — ViewType after Save | | `DetailedView` persisted (#51) |
| Purchase Order Edit — SalesPerson list | | Active company users only (#51) |
| Invoice Export — 33 columns | | Unchanged in #48–#55 |
| Invoice Source | | Unchanged CASE on export (base #47) |
| Primary Service on invoice Excel | | Snapshot `a.PServiceName` (base #45); optional backfill |
| InvoiceMail — same-company send | | HTML/subject unchanged (#54) |
| InvoiceMail — other-company Primary Service | | Must not appear in subject/body (#54) |
| Delete Invoice — same-company | | Status2 / InvStatus then three DELETEs (#55) |
| Delete Invoice — other-company Invoice_No | | No delete (#55) |
| Cross-company isolation | | Repeat Search/View/Delete/Mail with a second CompanyID session |

---

## 8. Remaining backlog

### Security

**Completed** for the confirmed defects in PRs #48–#55 (three merged: #51, #48, #49; five awaiting merge onto `July_to_Sept26_DevNSupport`).

### Modernization (technical debt, not security defects)

| Item | Notes |
|---|---|
| PR #56 SQL helper standardization | Local `AddCompanyId` / `OpenDb` / filter helpers only. No SQL behavior change. Not opened as a merge. |
| PR #57 DbCL inventory | Catalog `executeRdr` vs `executeRdrNew` vs `SPreturn_dt`. Not started as a PR. |
| Performance review | Out of this series. |
| Shared helper cleanup | Do not introduce a cross-page SQL wrapper unless a later modernization PR requires it. |
| Search PO export current-row | Follow-up to #49/#53 if finance wants Search Excel to match View export. |
| InvoiceMail full parameterization | Follow-up to #54 for remaining client/date concatenation. |

---

## 9. Evidence sources

- GitHub PR API: #48–#55 titles, files, merge state, merge commit, feature HEADs (2026-09-06).
- `git log origin/July_to_Sept26_DevNSupport` — merge SHAs on base: #51 `271c8cdd…`, #48 `3b4c42b9…`, #49 `04c43dbf…`.
- Existing scripts: `db/pservice_snapshot.sql`, `docs/qutprimaryservice_companyid_backfill.sql`, `docs/invoice_pservice_backfill.sql`.
- Existing inventory: `docs/invoice_export_data_inventory.md`.
