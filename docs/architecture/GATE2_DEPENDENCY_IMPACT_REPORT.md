# Gate 2 — Dependency and migration-impact report (multi-tenancy)

**Run:** 2026-10-01. Read-only. No application, SQL, schema, data, procedure, view, or trigger change was made. No migration script was written or run.  
**Branch / commit:** Original inventory run: `July_to_Sept26_DevNSupport` at `0c5cc72` (2026-10-01). Repository re-baseline: published HEAD `4918b977` (2026-11-05). `file:line` citations in §2–§16.5 still refer to `0c5cc72` and were not re-walked. The §16.6 row for `dbo.tbl_CrossTenantDuplication` is evidence from `282cfce`, which is contained in `4918b977`. Working-tree overlays were not read. Database modules were not inventoried.  
**Normative inputs:** [`ADR-002`](ADR-002_MultiTenancy_Ownership_Model.md) including Amendment A1, [`UAT_MIGRATION_GOVERNANCE.md`](UAT_MIGRATION_GOVERNANCE.md).  
**Evidence inputs:** this repository; [`OWNERSHIP_AUDIT.md`](OWNERSHIP_AUDIT.md) (live `flamex_uat`, 2026-10-01); [`../page-catalog/UAT_CATALOG.md`](../page-catalog/UAT_CATALOG.md) and [`../page-catalog/SHARED_SCHEMA.md`](../page-catalog/SHARED_SCHEMA.md) (8 Sep 2026 snapshot); [`../page-catalog/SHARED_RUNTIME.md`](../page-catalog/SHARED_RUNTIME.md).

**Result:** Repository-side inventory includes `dbo.tbl_CrossTenantDuplication` (§16.6 row 29). The supplied 2026-11-05 UAT collection recorded 22/22 module bodies (§12.2). Database-side inventory is still incomplete: the 2026-11-05 `flamex_uat` MCP job-step query was rejected because `execute_query` is disabled, and production was not profiled. **Gate 2 exit criteria are not met.** Gate 3 must not start. See §15.  
**Addendum (2026-10-01, `0c5cc72`, read-only):** §16 records decision analysis for business / design blockers X-23, X-24, X-25, X-28, X-31. No decision has been recorded by the repository / project owner; all five stay **Open**. X-11, X-18, X-19 stay **Blocked**.  
**Re-baseline (2026-11-05, HEAD `4918b977`, read-only):** §7.3 and §16.6 row 29 add `dbo.tbl_CrossTenantDuplication` from `282cfce`. Classification is **Open**. This note is not repository-owner approval.  
**UAT module bodies (supplied flamex_uat MCP collection, `2026-11-05T02:55:01.975Z`):** server `WIN-8H6JMQBVGP2\MSSQLSERVER2017`, database `flamex_uat`, SQL Server 2017 Express `14.0.2037.2`. `OBJECT_DEFINITION()` returned 13 procedures, 5 views, and 4 enabled triggers (22/22). Reader/writer detail is §12.2. This is UAT evidence, not a production census. A read-only `msdb.dbo.sysjobsteps` query was rejected: `execute_query is disabled`. No job-step rows were returned. X-11 stays **Blocked** on that gap. This collection is not repository-owner approval. Gate 3 is not authorized.

Paths below are relative to `Bill_Software/corporate/business/` unless they start with `docs/` or `db/`.

---

## 1. Evidence access in this run

| Source | Status | Detail |
|--------|--------|--------|
| Repository C#, ASPX, ASHX, print pages, in-repo `.sql` | INSPECTED | Text search over `Bill_Software/` and `db/`. `db/*.sql` has no reference to stock, stock adjustments, or either product chain. |
| Live `flamex_uat` | **NOT ACCESSED** | No database MCP server is loaded in this session. The MCP reader configuration (`Bill_Software/.cursor/mcp.json` / `mcp.local.json`) is not present in this working tree. The catalog generator (`docs/page-catalog/_catalog.py`) needs `UAT_*` credentials that are not available. The application `DbConn` login is not an authorized read-only profiling login and was not used. |
| Procedure / view / trigger bodies | **UAT BODIES RECORDED; JOB STEPS UNREADABLE** | 2026-10-01 audit: `VIEW DEFINITION` denied for the reader login. Supplied 2026-11-05 `flamex_uat` collection: 22/22 §12 module bodies via `OBJECT_DEFINITION()` (§12.2). The same MCP rejected `execute_query` against `msdb.dbo.sysjobsteps` (`execute_query is disabled`). No job-step rows were returned. Production was not read. |
| `flamex_live` | **NOT PROFILED** | X-18 requires a read-only production census of every ADR-002 §2 and §7 object, including tables empty on UAT (governance X-18). The connected MCP is `flamex_uat` only. UAT counts are not production counts. No production value appears in this report. |
| 8 Sep catalog re-verification (X-19) | **NOT PERFORMED** | Requires live UAT access. Facts marked *(8 Sep catalog)* below are unverified. |

---

## 2. Shared master boundaries (Product, Category, Store, Stock)

Target: shared identity, no `CompanyID` partition (ADR-002 INV-P2, INV-C2, INV-S1, INV-K4).

### 2.1 Product — `tbl_NewProduct`

| Use | Paths | Conforms to INV-P2? |
|-----|-------|---------------------|
| Read filtered by `CompanyID` | `app/search_products.aspx.cs:92,137`; `app/View_PR_Details.aspx.cs:413,540,1063,1171`; `app/RequisitionNew.aspx.cs:237,526,715,1046`; `app/Create_quotation.aspx.cs:205`; `app/edit_purchaseorder.aspx.cs:704`; `app/Edit_quatation_v2.aspx.cs:225,385`; `app/Manual_Invoice.aspx.cs:167`; `app/Add_invoice.aspx.cs:661,675,691` | **No** |
| Write filtered / stamped by `CompanyID` | `app/newproduct_master.aspx.cs:725-739` (INSERT with `@CompanyID`), `:783-792` (UPDATE `WHERE Id AND CompanyID`); `app/Add_invoice.aspx.cs:1178`; `app/Manual_Invoice.aspx.cs:474` | **No** |
| Read without tenant filter | `app/search_products.aspx.cs:39,70`; `app/Edit_quatation.aspx.cs:887`; `app/Direct_Proforma.aspx.cs:86,211`; `app/EditPurchase.aspx.cs:989`; `app/NewUpdate_product.aspx.cs:59`; `app/ImportProducts.aspx.cs:966,1153,1213,1430,1474,2038` | Yes |
| Write without tenant column (relies on DB default) | `app/ImportProducts.aspx.cs:504,559,665,1409,1531` (INSERT, no `CompanyID`); `:744,808,880,987,1300,1332,2059` (UPDATE `WHERE ProductName`) | Column-neutral; keyed by name, not `ProductID` |
| DDL in repo | `sql/tbl_NewProduct_CompanyID.sql` adds `CompanyID INT NOT NULL DEFAULT (1)` to `tbl_NewProduct` and `tbl_NewparentProduct` | Contradicts target; retained until retirement gate |

Finding: the repository partitions shared Product identity by tenant on every “filtered / stamped” path above. On current data (all 1441 rows company 1, audit) a company-2 session receives empty product results on every filtered path, and cannot invoice a product (`Add_invoice` / `Manual_Invoice` stock UPDATE matches 0 rows). `ImportProducts` updates by `ProductName`, which is not identity (12 duplicate name groups, audit). → X-21, X-26.

### 2.2 Category — `tbl_NewparentProduct`

| Use | Paths |
|-----|-------|
| Filtered by `CompanyID` | `app/newproductparent.aspx.cs:32,47,116 (INSERT with CompanyID),170,186`; `app/newproduct_master.aspx.cs:257,1265`; `app/View_PR_Details.aspx.cs:262,358`; `app/RequisitionNew.aspx.cs:187`; `app/Create_quotation.aspx.cs:415`; `app/edit_purchaseorder.aspx.cs:686`; `app/Manual_Invoice.aspx.cs:144`; `app/Edit_quatation_v2.aspx.cs:368` |
| Unfiltered | `app/search_products.aspx.cs:28,48`; `app/ImportProducts.aspx.cs:55,80`; `app/Edit_quatation.aspx.cs:583`; `app/Direct_Proforma.aspx.cs:161`; `app/Purches_exting_vendor.aspx.cs:93`; `app/EditPurchase.aspx.cs:963`; `app/PrimaryServiceTerms.aspx.cs:24` |
| Out-of-repo | `sp_GetProductCategories` (caller `app/Product_stock.aspx.cs:123`); body BLOCKED |

Finding: Category is tenant-partitioned on every “filtered” path above, including category creation per company. Contradicts INV-C2. → X-21.

### 2.3 Store — `Stores`

| Use | Paths | Key used |
|-----|-------|----------|
| Read (no tenant filter) | `app/Purches_exting_vendor.aspx.cs:121`; `app/EditPurchase.aspx.cs:132`; `print/purches_bill.aspx.cs:114` | `StoreId` (varchar) |
| Read (no tenant filter) | `app/Generate_PO_Preview.aspx.cs:60,79,431-449` → `tbl_PO_PartySnapshot.SourceId` | `Id` (int) |

Finding: no path partitions `Stores` by tenant (conforms to INV-S1 for tenancy). Two different store keys are in use. *(8 Sep catalog)* `Stores.StoreId` is `varchar(50) NOT NULL` with no unique constraint; PK is `Id`. Stock grain (INV-K1) and an FK from stock to `Stores` need one unique store key. → X-30.

### 2.4 Stock — `tbl_stock`

No repository path reads or writes a `CompanyID` on `tbl_stock` (conforms to INV-K4). Grain, store, and quantity-column violations are inventoried in §3–§5.

---

## 3. Stock mutation inventory (`tbl_stock`) and target mapping

Target for every movement: change exactly one `tbl_stock` row identified by (`ProductID`, `StoreID`), on `Quantity_Num` (ADR-002 §4.7, Amendment A1-3). The tenant is carried by the transaction, never by the stock row.

| ID | Path | Trigger | Key used | Column changed | Gap vs target | Target mapping |
|----|------|---------|----------|----------------|---------------|----------------|
| M-01 | `app/Purches_exting_vendor.aspx.cs:533` → `UpdateStock` `:628-668` | Purchase save (product lines) | `Product_id` + `ShippedToStoreId` | `Quantity_Num += q`; `Quantity` rewritten from `Quantity_Num` | UPDATE-then-INSERT without lock or unique key (duplicate risk, X-03). Also writes master quantity (Q-02). Stores UI store name text (X-17). | Conforms on key and column. Keep; remove master write; rely on INV-K1 unique key. |
| M-02 | `app/EditPurchase.aspx.cs:812-854` (`AdjustStockDelta`), callers `:634` (edit save), `:1270` (line delete) | Purchase edit / line delete | `Product_id` + `ShippedToStoreId` **and** `ISNUMERIC(Quantity)=1` | `Quantity` only | `Quantity_Num` never changed. If the existing row's `Quantity` is non-numeric, `EXISTS` is false and a **second row** is inserted for the same grain when `delta > 0`. INSERT omits `Quantity_Num` (default 0). | Re-key to (`ProductID`, `StoreID`) only; change `Quantity_Num`. X-27, X-03. |
| M-03 | `app/Delete_purtches.aspx.cs:166-205` (`DeletePurchaseWithStockUpdate`) | Purchase delete | `Product_id` + `ShippedToStoreId = pd.ShippedToLoc` | `Quantity` only (stock) and `tbl_NewProduct.Quantity` (Q-03) | `Quantity_Num` never changed. | Reverse movement on `Quantity_Num`; remove master write. X-27. |
| M-04 | `app/Purches_new_vendor.aspx.cs:397` → `updatestock` `:448-464` | Purchase save (legacy page) | Existence by `Product_id`; UPDATE by `Product_id` + `Product_name`; INSERT without store | `Quantity` only (`CAST(Quantity AS int)`) | No store (X-01 source). Name in key. Concatenated SQL. Header `:426` and lines `:395` carry no store. | Page must supply `StoreID` or be retired (Gate 3 decision). X-26, X-27. |
| M-05 | `app/ImportProducts.aspx.cs:692,1567` (INSERT); `:881,1009,1334` (UPDATE quantity); `:2080` (UPDATE tax rate only) | Bulk product / Tally import | INSERT without store; UPDATE `WHERE Product_name` | `Quantity` only | No store (X-01 source); name key can hit several rows across stores; overwrites quantity (not a movement). | Import must target (`ProductID`, `StoreID`) and post a signed movement, or stop writing stock. X-26, X-27. |
| M-06 | `app/newproduct_master.aspx.cs:750-767` | New product (opening stock) | Hard-coded `STR001` / `"Central Warehouse"` | `Quantity` only | Store not chosen by user (D5 needs a real store). `Quantity_Num` left at default 0. | Opening stock = movement on a user-selected `StoreID`, written to `Quantity_Num`. X-26, X-27. |
| M-07 | Out-of-repo: `usp_CreateMissingMastersFromStock`, `usp_Reconcile_MasterToStock`, `usp_UpdateZeroMastersFromStock`, `InsertOrGetProduct`, `trg_tbl_NewProduct_Normalize`; writers of `tbl_stock_fix_audit`, `tbl_StockReassignmentAudit`, `tbl_StockReconciliationAudit`, `stock_pid_mapping`, stock archive tables; applier of `tbl_StockAdjustments` | No C# caller for the three procedures | UAT bodies §12.2 | `InsertOrGetProduct` inserts `Quantity`. `@Apply = 1` sets `Quantity_Num`. Trigger sets `NormalizedProductName`. | Those five bodies do not write `tbl_stock` or `tbl_StockAdjustments`. Archive writers and job steps were not retrieved. | Named modules are inventoried. Jobs and archive writers remain unknown. |
| — | **Sales do not move `tbl_stock`.** `app/Add_invoice.aspx.cs:1175-1184` and `app/Manual_Invoice.aspx.cs:473-479` decrement `tbl_NewProduct.Quantity` (filtered by `CompanyID`) | Invoice save | `ProductID` + `CompanyID` on the **master** | Master `Quantity` | Physical issue is recorded on the master, not the ledger; no store on the invoice. Contradicts S1, S2, D5, INV-K5. | Sale = movement on (`ProductID`, `StoreID`) in `tbl_stock`; store source needs a decision. X-25. |
| — | No stock effect (despite method name `updatestock1`): `app/Delete_invoice.aspx.cs:176-201`, `app/Delete_chalan.aspx.cs:122-136`, `app/Delete_proforma.aspx.cs:122-136`, `app/Deletehydrent_invoice.aspx.cs:97-111` | Document delete | — | Quotation status flags only | Invoice delete does not reverse the master decrement made at invoice save. | Gate 3 defines reversal semantics for sales once X-25 is decided. |
| — | Dead code: `app/Delete_invoice.aspx.cs:249` (commented); `app/Delete_purtches.aspx.cs:228-274` (call commented at `:134`; hard-coded `'PR0042'`) | — | — | — | Not reachable. | Remove in Gate 7 or leave; no migration impact. |

DPCC challan (`add_chalan`), proforma, quotation, client PO, vendor PR/PO, and hydrant pages have **no** stock writer in the repository. `Add_invoice` skips stock when the document type is `"Delivery Challan"` (`:1176`).

## 4. Stock read inventory

| ID | Path | Reads | Gap vs target | Target |
|----|------|-------|---------------|--------|
| R-01 | `app/Product_stock.aspx.cs:36,82,123` → `sp_SearchProductsFast`, `sp_GetProductStockByStore` (returns `ShippedToStoreName`, `StockQty`), `sp_GetProductCategories` | Procedure output | UAT bodies §12.2. Stock-by-store sums legacy `tbl_stock.Quantity` and groups by `ShippedToStoreName`, not `Quantity_Num` or `ShippedToStoreId`. No `CompanyID` predicate. Page base is `System.Web.UI.Page` with a session check (`:10,15`), not `SecurePage`. Missing `CompanyID` on these shared reads is an ownership-impact observation, not a confirmed security defect. | Aggregate `Quantity_Num` by (`ProductID`, `StoreID`). |
| R-02 | `app/Service_stock.aspx.cs:30` (`select * from tbl_stock where Product_id not like('P%')`), bound at `app/Service_stock.aspx:86` (`Eval("Quantity")`) | Legacy `Quantity` | Reads the non-authoritative column; no store grouping. | Read `Quantity_Num` per store. X-27. |
| R-03 | `app/EditPurchase.aspx.cs:820` | `ISNUMERIC(Quantity)` existence guard | Legacy column as row selector (M-02). | Remove. |
| R-04 | `app/Delete_purtches.aspx.cs:180-182` | Join on product + store | Reads/writes `Quantity` (M-03). | `Quantity_Num`. |
| R-05 | `app/ImportProducts.aspx.cs:967,2039` | Existence by `Product_name` | Name is not identity. | By `ProductID` (+ `StoreID`). |
| R-06 | `app/Purches_new_vendor.aspx.cs:452` | Existence by `Product_id` | Store dropped. | By (`ProductID`, `StoreID`). |
| R-07 | `vw_StockReconciliation`, `vw_CategoryStoreStock_Safe` | View output | No repository caller. UAT bodies §12.2. Reconciliation sums `Quantity_Num`, with master quantity at product grain and stock at store grain. The category view sums legacy `Quantity` by `ShippedToStoreId` and store name. No `CompanyID`. | Under S1/A1-3 a master-vs-stock comparison has no authority meaning. |
| R-08 | Master quantity presented as stock: `app/Add_invoice.aspx.cs:658,672,686` (`np.Quantity AS AvailableStock`); `app/Manual_Invoice.aspx.cs:167` | `tbl_NewProduct.Quantity` | Contradicts INV-K5. | Read `tbl_stock.Quantity_Num` (per store or summed, Gate 3 decides display). |

## 5. `Quantity` vs `Quantity_Num` inventory and authority mapping

Authority (ADR-002 S1, S2, Amendment A1-3): `tbl_stock.Quantity_Num` is the only quantity of record. `tbl_stock.Quantity` is legacy compatibility data. `tbl_NewProduct.Quantity` / `Quantity_Num` are not stock.

### 5.1 `tbl_stock`

| Path | Writes `Quantity_Num` | Writes `Quantity` | Reads | Target |
|------|:---------------------:|:-----------------:|-------|--------|
| M-01 `Purches_exting_vendor` | yes | yes (derived from `Quantity_Num`) | — | Conforms. |
| M-02 `EditPurchase` | **no** | yes | `Quantity` | Write `Quantity_Num`; `Quantity` only as derived copy, if kept. |
| M-03 `Delete_purtches` | **no** | yes | `Quantity` | Same. |
| M-04 `Purches_new_vendor` | **no** | yes | — | Same, plus store. |
| M-05 `ImportProducts` | **no** | yes | — | Same, plus store and identity. |
| M-06 `newproduct_master` | **no** (default 0) | yes | — | Same, plus selected store. |
| R-02 `Service_stock` | — | — | `Quantity` | Read `Quantity_Num`. |
| Out-of-repo modules (M-07, R-01, R-07) | unknown | unknown | unknown | BLOCKED (X-11). |

Factual conflict, not resolved here: five writer paths change `Quantity` without `Quantity_Num`. For rows they touched, `Quantity_Num` may be the stale value. The audit found only 5 divergent rows, which suggests either low use of those paths or an out-of-repo sync (trigger, procedure, or the one-off fixes recorded in `tbl_stock_fix_audit`); which one is NOT_VERIFIABLE until X-11 closes. A1-3 fixes which column is authoritative. It does not attest that the current `Quantity_Num` value is physically correct for those rows. → X-16 (residual), X-27.

### 5.2 `tbl_NewProduct` (not stock)

| ID | Path | Effect | Target |
|----|------|--------|--------|
| Q-01 | `app/newproduct_master.aspx.cs:725-728` (INSERT `Quantity`, `Quantity_Num`), `:783-792` (UPDATE overwrites both, no stock movement) | Master quantity set from form | Stop writing; opening stock goes to `tbl_stock` only (M-06). |
| Q-02 | `app/Purches_exting_vendor.aspx.cs:670-684` | `Quantity_Num += q`, `Quantity` derived | Remove. |
| Q-03 | `app/Delete_purtches.aspx.cs:185-190` | `Quantity -= pd.Quantity` (text only) | Remove. |
| Q-04 | `app/Add_invoice.aspx.cs:1178` | `Quantity -= q` filtered by `CompanyID` | Replace with stock movement (X-25). |
| Q-05 | `app/Manual_Invoice.aspx.cs:474` | Same | Same. |
| Q-06 | `app/NewUpdate_product.aspx.cs:59,75,95-128` | Reads (concatenated SQL) and overwrites `Quantity` | Remove field or relabel as non-stock attribute (Gate 3). |
| Q-07 | `app/ImportProducts.aspx.cs:504,559,665,1409,1531` (INSERT); `:744,808,880,987,1300,1332` (UPDATE by name) | Sets `Quantity` only | Remove. |
| Q-08 | Reads: `app/Add_invoice.aspx.cs:658,672,686`; `app/Manual_Invoice.aspx.cs:167`; `app/View_PR_Details.aspx.cs:404,1166,1204`; `app/RequisitionNew.aspx.cs:228,1041,1079` | Master `Quantity` returned to UI (invoice: as available stock; PR: as product `qty` attribute) | Invoice reads → `tbl_stock.Quantity_Num`. PR reads → Gate 3 decides whether the field is stock (then `tbl_stock`) or a non-stock attribute. |
| Q-09 | Out-of-repo: `usp_UpdateZeroMastersFromStock`, `usp_Reconcile_MasterToStock`, `InsertOrGetProduct` (`@Quantity int`), `trg_tbl_NewProduct_Normalize` | UAT bodies §12.2. `@Apply = 1` updates `tbl_NewProduct.Quantity_Num` from the all-store `tbl_stock.Quantity_Num` sum. `InsertOrGetProduct` inserts `Quantity`, not `Quantity_Num`. The trigger writes `NormalizedProductName` only. | Write-capable when `@Apply = 1`. Not a `tbl_stock` writer. |

`search_products.aspx.cs:39,70` select `*` but `search_products.aspx` binds no quantity field.

## 6. `tbl_StockAdjustments` mapping

Target (Amendment A1-2): TRANSACTION_OWNED root. Each row is a physical movement of `AdjustmentQty` on the shared `tbl_stock` row (`ProductID`, `StoreID`), applied to `Quantity_Num`. The row carries a direct non-null `CompanyID` (the tenant that performed the adjustment) and a non-null `StoreID`. The stock row carries no `CompanyID`.

| Aspect | Observed | Target gap |
|--------|----------|-----------|
| Columns *(8 Sep catalog)* | `AdjustmentId` PK, `ProductID varchar(50)`, `AdjustmentQty decimal(18,4)`, `Reason`, `CreatedBy varchar(100)`, `CreatedAt`, `Applied bit DEFAULT 0`, `AppliedAt` | No `StoreID`, no `CompanyID`. `ProductID` type differs from `tbl_NewProduct.ProductID nvarchar(250)` (FK needs matching type). |
| Rows (audit) | 126; 108 resolve to `tbl_NewProduct`, 18 do not (X-15) | Every row needs a store and an owner. |
| Repository readers / writers | **None.** No match for `StockAdjust` in `Bill_Software/` or `db/`. | The writer and the process that sets `Applied` / `AppliedAt` are outside the repository. |
| Already posted to stock? | `Applied` distribution not profiled. | If an applied row already changed a `tbl_stock` row, the migration must not post it again, and its `StoreID` must be the store of the row it actually changed. Evidence may exist in `tbl_stock_fix_audit` / `tbl_StockReconciliationAudit`; unverified. |
| Required backfill source — `StoreID` | — | Business-signed repair data set per `AdjustmentId` (DATA_REPAIR_REQUIRED). Not admissible: default store, the product's only stock row, quantity split. |
| Required backfill source — `CompanyID` | — | Business-signed ownership data set per `AdjustmentId` (BACKFILL_RULE_REQUIRED). Not admissible: Product, Store, Stock, or the creating login's home company (`CreatedBy` → `tbl_login.CompanyID`); a login may hold several memberships, and D7 forbids master-derived ownership. |
| Future writes | — | `CompanyID` from `CompanyContext.CurrentCompanyID`; `StoreID` required; apply as a `Quantity_Num` movement in the same transaction, or define `Applied` semantics in Gate 3. |

## 7. Transaction roots and children

“Rule” is the propagation (new rows) and backfill (existing rows) rule required by ADR-002 §4. Status of `CompanyID` is from the audit unless marked *(8 Sep catalog)*.

### 7.1 Roots

| Root | `CompanyID` | Rows (UAT) | Writers (repository) | Rule |
|------|-------------|-----------:|----------------------|------|
| `tbl_Quotation` | NOT NULL, FK declared | 159 | `Create_quotation`, `Edit_quatation_v2`, `edit_purchaseorder` (via `CompanyContext`) | Compliant column. Gate 3: FK exists; add per-tenant business-key uniqueness. |
| `tbl_Invoice` | NOT NULL, no FK | 13 | `app/Add_invoice.aspx.cs:1094`, `app/Manual_Invoice.aspx.cs:403` | Compliant column. Add FK to `tbl_Company.ID`. `Invoice_No` is a per-company serial (`INV/C/<FY>/<n>`, `Add_invoice.aspx.cs:1081-1088`, `Manual_Invoice.aspx.cs:395,554-563`), so uniqueness must be (`CompanyID`, `Invoice_No`). |
| `tbl_Chalan` | NOT NULL DEFAULT (1) *(8 Sep catalog)* | 35 *(8 Sep)* | `add_chalan` | Compliant column (verify live, X-19). Add FK. |
| `tbl_Proforma` | NOT NULL DEFAULT (1) *(8 Sep catalog)* | 7 *(8 Sep)* | proforma pages | Same. |
| `tbl_RequisitionMain` | NOT NULL | 12 | `sp_Requisition_CreateDraft` (body BLOCKED) | Compliant column. Add FK. |
| `tbl_PO_Header` | NOT NULL | 9 | `sp_GeneratePO_FromReqNo` (body BLOCKED) | Compliant column. Add FK. |
| `tbl_SalesVisitReport` | NOT NULL | 78 | visit pages | Compliant column. Add FK. |
| `tbl_Purches` | **none** | 163 | `app/Purches_exting_vendor.aspx.cs:545`, `app/Purches_new_vendor.aspx.cs:426` (no `CompanyID`) | New rows: `CompanyContext.CurrentCompanyID`. Existing rows: business-signed ownership data set per `Purches_Id` (X-06). Not admissible: `Client_Id` → `tbl_Vendor`, product, store, or `AddedById` → login home company. The vendor join may be shown to the business as a cross-check only. `Purches_Id` is generated globally (`GenerateID`, `Purches_exting_vendor.aspx.cs:463,687-707`). |
| `tbl_HydrentInvoice` | **none** | 0 | `app/hydrent_invoice.aspx.cs:201` | New rows: `CompanyContext`. Existing rows: none on UAT; production unknown (X-18). If production has rows, same signed-data-set rule; not `Client_ID`. |
| `tbl_qsHydrentQuotation` | **none** | 0 | `app/CreateHydrentQuatation.aspx.cs:144` | Same. |
| `tbl_StockAdjustments` | **none** | 126 | none in repository | §6. |

### 7.2 Children

| Child | Parent key | Own `CompanyID` | Rule |
|-------|-----------|-----------------|------|
| `tbl_Quotaion_details` | `Quotation_no` | NOT NULL *(8 Sep catalog)* | Must equal root (INV-T3). Gate 3 reconciliation query; parent key must include `CompanyID` because quotation numbering is per tenant (verify). |
| `tbl_Invoice_details` | `Invoice_No` | NOT NULL | Must equal root. Join must be (`Invoice_No`, `CompanyID`). 3 orphans (X-09). |
| `tbl_invoice_payment` | invoice (A1-1) | none | §8. |
| `tbl_invoice_due` | invoice (A1-1) | none | §8. |
| `tbl_invoice_payment_tds` | invoice (A1-1) | none | §8. |
| `tbl_Proforma_Details` | proforma `Invoice_No` | NOT NULL DEFAULT (1) *(8 Sep catalog)* | Must equal root. |
| `tbl_RequisitionNew` | `ReqNo` | NOT NULL DEFAULT (1) *(8 Sep catalog)* | Must equal root. |
| `tbl_PO_Items` | `PO_Id` (FK) | NOT NULL DEFAULT (1) *(8 Sep catalog)* | Must equal root. |
| `tbl_PO_PartySnapshot`, `tbl_PO_PaymentDetails` | `PO_Id` (FK) | none | Follows root through FK. No backfill needed. |
| `tbl_purches_details` | `Purches_id` | none | Follows root once root is owned. 19 orphan products (X-08). |
| `tbl_Purchess_payment` | `Purchess_ID` | none | Follows root. |
| `tbl_purches_due` | `Purches_Id` | none | Follows root. 2 orphans (X-07). |
| `tbl_SalesVisitResponses` | `VisitId` (FK) | none | Follows root. |
| `tbl_Expenses` | `VisitId` (no FK) | none | Follows root. |
| `tbl_qsHydrentDetails` | `Quotation_no` | none | Follows hydrant quotation once owned. |

Whether a child without `CompanyID` gets the column is a Gate 3 design choice (ADR-002 §4.4). If added, it is copied from the root at write time and backfilled only by joining the root on a parent key that resolves to exactly one root; any other child is an orphan exception.

### 7.3 Objects outside the freeze (listed, not classified)

| Object | `CompanyID` *(8 Sep catalog)* | Observed role |
|--------|-------------------------------|---------------|
| `tbl_InvSiteAddress` | NOT NULL DEFAULT (1) | Written with invoice (`Add_invoice.aspx.cs:1191`) |
| `tbl_QuoPriSerTogather`, `tbl_QuoPserTerm`, `tbl_QutPaymentPhase` | NOT NULL DEFAULT (1) | Keyed by quotation number |
| `tbl_quotation_vat` | none | Keyed by `Quotation_no` |
| `tbl_Requisition_Approvals` | none | Keyed by `ReqNo` |
| `tbl_PO_Amendment_Header` / `_Items`, `tbl_PO_Cancellation_Request`, `tbl_PO_Item_Cancellation` | none | 0 rows, no C# caller |
| `tbl_Service` | NOT NULL DEFAULT (1) | Service master |
| `tbl_ProductCatagory` | none | Category leftover |
| `tlb_General_expences`, `tbl_patty_cash_expenses`, `tbl_Expences` | none | GL |
| `tbl_stock_archive`, `tbl_stock_duplicates_archive`, `tbl_stock_fix_audit`, `tbl_stock_old`, `tbl_stock_purge_archive`, `tbl_stock_review_missing_qty_rate`, `tbl_StockReassignmentAudit`, `tbl_StockReconciliationAudit`, `stock_pid_mapping`, `tbl_NewProduct_old` / `_Archive` / `_Archive2` | none | Repair / archive artifacts; no C# caller |
| `tbl_CrossTenantDuplication` | Not in the 8 Sep catalog. Script at `282cfce` defines `SourceCompanyID` and `TargetCompanyID` | Cross-tenant vendor/customer link. §16.6 row 29. Not classified |

→ X-31. Per-object inventory, evidence, and classification status: §16.6.

## 8. Invoice payment, due, and TDS inheritance

Rule (Amendment A1-1): `tbl_invoice_payment`, `tbl_invoice_due`, and `tbl_invoice_payment_tds` are children of `tbl_Invoice`. Their owner is the `CompanyID` of the single invoice row they belong to. They are never owned through Client, Quotation, or any master.

Observed write paths (`app/add_payment.aspx.cs`):

| Step | Line | Key written |
|------|------|-------------|
| Payment INSERT | `:323` | `Quotation_No`, `Client_Id`; **no `Invoice_No`** |
| Invoice number stamped on payments | `:364` (only where `Due_amount='0.00'`, joined to `tbl_Invoice` on quotation and `CompanyID`), `:394` (`WHERE Quotation_No`) | `Invoice_No` after the fact |
| TDS INSERT | `:403` | `invoice_no`, `quotation_no` |
| Due read / UPDATE / INSERT | `:173,430,442,451` | `qutation_no` only. `tbl_invoice_due.Invoice_no` exists *(8 Sep catalog)* and is never written. |
| Other due / payment access by quotation number | `app/Edit_quatation.aspx.cs:1167,1177`; `app/Edit_quatation_v2.aspx.cs:1227,1234`; `app/edit_purchaseorder.aspx.cs:1515,1532` | Quotation number, concatenated SQL |
| Reads without tenant predicate | `app/PaymentMail.aspx.cs:52,58,65,137,292,335`; `app/seartch_payment.aspx.cs:37,43,50`; `print/bill.aspx.cs:91,156`; `print/billduplicate.aspx.cs:92,158`; `print/NewPaymentInvoice.aspx.cs:64,90,795`; `print/NewPaymentInvoiceDuplicate.aspx.cs:63,89,781` | — |
| Scoped read | `app/View_payment.aspx.cs:31` (via `tbl_Quotation.CompanyID`) | — |
| Print authorization | `app/AuthGuard.cs:645-648` (via `tbl_Quotation`) | — |

Consequences for Gate 3 (registered as X-28):

1. `Invoice_No` is not unique across tenants by construction (§7.1). A child that references its invoice only by number cannot resolve to exactly one invoice once company 2 invoices. Gate 3 must give each child a deterministic invoice reference (`tbl_Invoice.ID`, or `Invoice_No` + `CompanyID` copied from the invoice at write time).
2. A payment or due recorded against a quotation before any invoice exists has no invoice parent. Under A1-1 it has no derivable owner. Whether such pre-invoice receipts exist as a business process, and how they are owned, is not covered by the ratified decisions → BUSINESS_DECISION_REQUIRED. Using `tbl_Quotation.CompanyID` instead would be a new decision (Quotation is not a master, so D7 does not forbid it, but A1-1 names Invoice) and needs an ADR amendment.
3. Backfill: UAT has 0 rows in all three tables, so nothing is backfilled on UAT. For production rows (X-18), no admissible derivation rule exists: `Invoice_No` alone is not a tenant-unique key even where a production number currently matches one invoice (§16.5). Each production row needs a signed link / ownership data set; an exact invoice-number match may be shown to the business as supporting evidence only. (Supersedes the earlier number-match rule in this item.)
4. Print and list paths must resolve ownership through the invoice, not the quotation (X-29).

## 9. Ownership inference in runtime code (D7, INV-T2)

| Path | Inference | Exception |
|------|-----------|-----------|
| `app/AuthGuard.cs:649-652` (`tbl_Purches.Purches_Id`) | Purchase tenant from `tbl_Vendor.CompanyID` | X-29 |
| `app/AuthGuard.cs:653-660` (`tbl_HydrentInvoice.ID`, `tbl_qsHydrentQuotation.Quotation_no`) | Hydrant tenant from `tbl_Client.CompanyID` | X-29 |
| `app/AuthGuard.cs:645-648` (`tbl_invoice_payment.Payment_ID`) | Payment tenant from `tbl_Quotation`, not `tbl_Invoice` | X-29 |
| `app/View_purches.aspx.cs:137-141` | Purchase list scoped by vendor join | X-29 |

These are read-time scoping, not data writes; no repository path writes a root `CompanyID` from a master. `AuthGuard` changes need architectural review ([`../HANDOFF.md`](../HANDOFF.md) Constraints). Today the inference returns the right tenant for current UAT data (all purchases join one company-1 vendor, audit). It becomes wrong as soon as two tenants share a vendor or client code: vendor codes are generated per company (`app/New_vendor.aspx.cs:158`), and 4 client codes already exist in both companies (audit).

## 10. Tenant masters and global reference

### 10.1 Tenant masters (D1)

`tbl_Client`, `tbl_Vendor`: `CompanyID` NOT NULL (audit). `tbl_Factory`, `tbl_representative`, `tbl_ClientRegAddress`: NOT NULL DEFAULT (1) *(8 Sep catalog)*, unverified live (X-19).

Client-code-only reads of tenant-master children (no `CompanyID`): `app/add_chalan.aspx.cs:1235,1253`; `app/ShowFactory.aspx.cs:37,70`; `app/Show_representative.aspx.cs:34,62`; `app/Update_factory.aspx.cs:53,123` (by row `ID` only); `app/hydrent_invoice.aspx.cs:132`; `app/PaymentMail.aspx.cs:182`; `app/ProformaMail.aspx.cs:256`; `app/Set_quatation.aspx.cs:215`; `print/NewInvoice.aspx.cs:174`; `print/NewInvoiceDuplicate.aspx.cs:161`; `print/NewQuotation.aspx.cs:1680`; `print/NewProformaInvoice.aspx.cs:568`; `print/NewPaymentInvoice.aspx.cs:185`; `print/NewPaymentInvoiceDuplicate.aspx.cs:186`; `print/NewChhalan.aspx.cs:105`; `print/NewChhalanDuplicate.aspx.cs:325`; `print/NewChhalanTriplicate.aspx.cs:123`; `print/Invoice.aspx.cs:526`; `print/Invoiceduplicate.aspx.cs:531`; `print/chhalan.aspx.cs:103`; `print/hydrentInvoice.aspx.cs:275`; `print/hydrentInvoiceSeller.aspx.cs:241`.

Scoped correctly: `app/Representative.aspx.cs:160,178`; `app/AddFactory.aspx.cs:163,181`; `app/Update_client.aspx.cs:77,173`; `app/Delete_client.aspx.cs:98`; `app/Add_invoice.aspx.cs:170`; `app/Manual_Invoice.aspx.cs:116`; `app/InvoiceMail.aspx.cs:188`; `print/PurchaseOrderPrintHelper.cs:724`; `app/DuplicationService.cs:456,487,519`.

→ X-10 (validated in code).

### 10.2 Global reference (D2, D3)

| Object | Filtered by `CompanyID` | Unfiltered | Tenant-scoped writes |
|--------|------------------------|------------|----------------------|
| `tbl_State` | `master_State.aspx.cs:32,57,112,135`; `New_client.aspx.cs:42`; `New_vendor.aspx.cs:23`; `Update_vendor.aspx.cs:29`; `Update_client.aspx.cs:27,29`; `master_city.aspx.cs:49` | `AddFactory.aspx.cs:24`; `Update_factory.aspx.cs:25`; `Purches_new_vendor.aspx.cs:30` | `master_State.aspx.cs:75,76` |
| `tbl_City` | `master_city.aspx.cs:34,81,161`; `New_client.aspx.cs:60,217`; `New_vendor.aspx.cs:129`; `Update_vendor.aspx.cs:30`; `Update_client.aspx.cs:28,30` | `Create_quotation.aspx.cs:123`; `edit_purchaseorder.aspx.cs:96`; `Edit_quatation.aspx.cs:45`; `Edit_quatation_v2.aspx.cs:88`; `AddFactory.aspx.cs:25`; `Update_factory.aspx.cs:26`; `Purches_new_vendor.aspx.cs:31` | `master_city.aspx.cs:108,112,196`; `New_client.aspx.cs:226`; `New_vendor.aspx.cs:139` |
| `tbl_Vat_Master` | none | `Vat_master.aspx.cs:32,43,58`; `productparent.aspx.cs:25`; `Update_product.aspx.cs:25`; `View_PR_Details.aspx.cs:114`; `RequisitionNew.aspx.cs:138`; `Purches_exting_vendor.aspx.cs:109`; `EditPurchase.aspx.cs:157`; `Purches_new_vendor.aspx.cs:269`; `HydrantProduct.aspx.cs:22` | `Vat_master.aspx.cs:43` (no `CompanyID`; relies on default) |

All paths above are under `app/`. State and City are tenant-partitioned on master and client / vendor pages and shared on factory, quotation, PO, and legacy purchase pages. City rows are created per company, so the same reference value can exist once per tenant. Client / vendor / factory rows store state and city as text (`State_Name`, `City_Name`), not as keys, so merging reference rows does not break a declared FK; live distribution is still unverified (X-19, X-20). VAT is never filtered and conforms to INV-G1 in code.

## 11. Legacy Product chain (`tbl_Product`, `tbl_parentProduct`)

| Path | Use |
|------|-----|
| `app/productparent.aspx.cs:34,45,67` | Read / INSERT / DELETE `tbl_parentProduct` (DELETE concatenated) |
| `app/product_master.aspx.cs:25,33,50,95,108` | Read both; INSERT / DELETE `tbl_Product` (concatenated) |
| `app/Update_product.aspx.cs:36,55` | Read / UPDATE `tbl_Product` (concatenated) |
| `app/RequisitionCreate.aspx.cs:107,133` | Read both **filtered by `CompanyID`**, a column neither table has (audit) → SQL error 207 at runtime. The page also targets missing `tbl_requisition` (SHARED_SCHEMA §4). |
| `app/Purches_new_vendor.aspx.cs:120,148` | Read `tbl_Product` (concatenated) to build purchase lines |
| Commented only | `app/Edit_quatation.aspx.cs:696,913`; `app/Service_update.aspx.cs:54`; `app/product_master.aspx.cs:68` |
| FK | `FK_tbl_Product_tbl_parentProduct` (declared) |
| Out-of-repo | Unknown (X-11) |

No transaction table references `tbl_Product` by FK *(8 Sep catalog)*. Both tables are empty on UAT (audit); production counts unknown (X-18). `tbl_HydrantProduct` is a separate hydrant master (`app/HydrantProduct.aspx.cs`, read by `app/CreateHydrentQuatation.aspx.cs`), not part of either product chain. → X-22, X-23.

## 12. Procedures, views, triggers

| Object | Repository caller | Definition |
|--------|-------------------|------------|
| `sp_SearchProductsFast`, `sp_GetProductStockByStore`, `sp_GetProductCategories` | `app/Product_stock.aspx.cs:36,82,123` | UAT body §12.2 |
| `sp_SearchStockGrouped`, `InsertOrGetProduct`, `usp_CreateMissingMastersFromStock`, `usp_Reconcile_MasterToStock`, `usp_UpdateZeroMastersFromStock` | none | UAT body §12.2 |
| `sp_Requisition_CreateDraft`, `sp_RequisitionItem_BulkUpsert`, `sp_GeneratePO_FromReqNo`, `sp_ReleasePO_Final`, `sp_GetReleasedPO_Details` (write / read PR, PO, product rows) | per [`../page-catalog/SHARED_RUNTIME.md`](../page-catalog/SHARED_RUNTIME.md) §5 | UAT body §12.2 |
| `vw_StockReconciliation`, `vw_CategoryStoreStock_Safe`, `vw_PO_Items_Effective`, `vw_FullDesignation` (2) | none found | UAT body §12.2 |
| `trg_tbl_NewProduct_Normalize`, `trg_Block_PO_Update`, `trg_Invoice_MarkPC_RealizeRev`, `trg_Quotation_LinkVisit` | fire implicitly on product / PO / invoice / quotation writes | UAT body §12.2; all four enabled |
| SQL Agent jobs | No job, scheduler, or timer reference in the repository (`msdb`, `sp_add_job`, timers: 0 matches) | 2026-11-05 `flamex_uat` MCP: `execute_query` of `msdb.dbo.sysjobsteps` rejected (`execute_query is disabled`). No job-step rows returned. Absence of job dependencies is not established. |

The 22 module bodies below are the supplied `flamex_uat` collection. SQL Agent step text was not returned. → X-11 stays **Blocked** for that job gap.

### 12.1 Read-only DBA evidence request

Module portion supplied, not re-executed for this edit. Context: `WIN-8H6JMQBVGP2\MSSQLSERVER2017`, `flamex_uat`, SQL Server 2017 Express `14.0.2037.2`, `2026-11-05T02:55:01.975Z`, `OBJECT_DEFINITION()`. Coverage: 13 procedures, 5 views, 4 enabled triggers, 22/22. SQL Agent job-step text was not returned: `execute_query` is disabled. This record is not repository-owner approval.

### 12.2 UAT reader/writer classifications (22/22)

No `EXEC`, `sp_executesql`, linked server, synonym, or cross-database reference was identified in these 22 bodies. `tbl_StockAdjustments`, `tbl_Product`, and `tbl_parentProduct` do not appear. Missing `CompanyID` on shared stock and product modules is an ownership-impact observation, not a confirmed security defect. `tbl_PO_Item_Cancellation` and `tbl_StockReconciliationAudit` stay **Open** in §16.6.

| Object | Reads | Writes | CompanyID / grain |
|---|---|---|---|
| `sp_SearchProductsFast` | `tbl_NewProduct`, `tbl_NewparentProduct` | none | none. `DeleteMode = 0` |
| `sp_GetProductStockByStore` | `tbl_stock` | none | none. Legacy `Quantity`, grouped by `ShippedToStoreName` |
| `sp_GetProductCategories` | `tbl_NewparentProduct` | none | none |
| `sp_SearchStockGrouped` | `tbl_NewProduct`, `tbl_NewparentProduct`, `tbl_stock` | none | none. Legacy `Quantity`, grouped by `ShippedToStoreName` |
| `InsertOrGetProduct` | `tbl_NewProduct` | `INSERT tbl_NewProduct` | none. Inserts `Quantity`, not `Quantity_Num`. `BEGIN TRAN` / `COMMIT`; unique-key 2627/2601 rolls back and re-reads; other errors `ROLLBACK` |
| `usp_CreateMissingMastersFromStock` | `tbl_stock.Quantity_Num`, `tbl_NewProduct.Quantity_Num` | `@Apply = 0`: preview only. `@Apply = 1`: `INSERT tbl_NewProduct`, `INSERT tbl_StockReconciliationAudit`, and `CREATE TABLE dbo.tbl_StockReconciliationAudit` when that table is absent | none. Write-capable at `@Apply = 1`. Per-product transaction; failure rolls that product back and writes an unapplied audit row |
| `usp_Reconcile_MasterToStock` | both `Quantity_Num` totals | `@Apply = 0`: preview. `@Apply = 1`: `UPDATE tbl_NewProduct.Quantity_Num` to the all-store stock sum; `INSERT` audit | none. Write-capable at `@Apply = 1`. Same per-row transaction and rollback |
| `usp_UpdateZeroMastersFromStock` | both totals where master is 0 and stock is greater than 0 | `@Apply = 1`: `UPDATE tbl_NewProduct.Quantity_Num`; `INSERT` audit | none. Write-capable at `@Apply = 1`. Same transaction pattern |
| `vw_StockReconciliation` | `tbl_stock.Quantity_Num` by product and store; `tbl_NewProduct.Quantity_Num` summed by product | none | none. Master quantity is product-level; stock quantity is store-level |
| `vw_CategoryStoreStock_Safe` | `tbl_stock.Quantity`, `tbl_NewProduct`, `tbl_NewparentProduct` | none | none. Legacy `Quantity`. Groups by `ShippedToStoreId` and store name |
| `vw_PO_Items_Effective` | `tbl_PO_Items`, `tbl_PO_Item_Cancellation` | none | none. Approved cancellations only. Cancellation class stays Open |
| `dbo.vw_FullDesignation` | `tbl_Designation` | none | none. `SELECT *` |
| `flame_ex.vw_FullDesignation` | `dbo.tbl_Designation` | none | none. Same database, `SELECT *` |
| `sp_Requisition_CreateDraft` | `tbl_RequisitionMain` | `INSERT tbl_RequisitionMain` | `CompanyID = @CompanyID` on the sequence read; value stamped on insert. `UPDLOCK, HOLDLOCK` |
| `sp_RequisitionItem_BulkUpsert` | `tbl_RequisitionMain`, table-valued `@Items` | `DELETE` and `MERGE tbl_RequisitionNew` | parent and line match on `CompanyID`. `THROW 50001` if the PR is missing. No explicit transaction |
| `sp_GeneratePO_FromReqNo` | `tbl_RequisitionMain`, `tbl_RequisitionNew`, `tbl_PO_Header` | `INSERT tbl_PO_Header`, `INSERT tbl_PO_Items` | every read and both inserts use `@CompanyID`. `THROW 50010`–`50012` |
| `sp_ReleasePO_Final` | the updated header row | `UPDATE tbl_PO_Header` | `PO_Id` and `CompanyID`. `THROW 50020` when no draft matches. `trg_Block_PO_Update` can roll this back |
| `sp_GetReleasedPO_Details` | `tbl_PO_Header`, `tbl_PO_PartySnapshot`, `tbl_PO_Items`, `tbl_PO_PaymentDetails` | none | **no independent `CompanyID` predicate.** Filters `PO_Id`; header also requires `PO_Status = 'Released'`. Ownership-impact review finding, not a confirmed security vulnerability |
| `trg_tbl_NewProduct_Normalize` | `inserted` | `UPDATE tbl_NewProduct.NormalizedProductName` | none. Enabled. `AFTER INSERT, UPDATE` on `tbl_NewProduct`. Runs for the product inserts and updates above |
| `trg_Block_PO_Update` | `inserted`, `deleted` | no table write | none. Enabled. `AFTER UPDATE` on `tbl_PO_Header`. If `IsLocked = 1` and session context `ALLOW_PO_UPDATE` is not 1: `RAISERROR` and `ROLLBACK TRANSACTION` |
| `trg_Invoice_MarkPC_RealizeRev` | `inserted`, `tbl_Quotation`, `tbl_Invoice` | `UPDATE tbl_SalesVisitReport` | none. Enabled. `AFTER INSERT, UPDATE` on `tbl_Invoice`, only when `Quotation_No` or `Net_Amount` is updated. Quote match is by number only. Does not write `tbl_stock` |
| `trg_Quotation_LinkVisit` | `inserted` | `UPDATE tbl_SalesVisitReport.LinkedQuotationNo` | none. Enabled. `AFTER INSERT, UPDATE` on `tbl_Quotation`, only when `VisitId` or `Quotation_no` is updated |

Legacy `tbl_stock.Quantity` consumers in this set: `sp_GetProductStockByStore`, `sp_SearchStockGrouped`, `vw_CategoryStoreStock_Safe`. `Quantity_Num` consumers and writers: the three reconciliation procedures and `vw_StockReconciliation`.

## 13. Exception reclassification

Status values: **Resolved** (decision recorded; no residual on this ID), **Validated** (fact confirmed by Gate 2 evidence; ID stays open under the classification shown), **Blocked** (finding cannot be completed without the named dependency). Owner is the role that must act; names are recorded in [`../HANDOFF.md`](../HANDOFF.md) when the gate closes.

| ID | Gate 2 status | Classification after Gate 2 | Evidence | Owner |
|----|---------------|-----------------------------|----------|-------|
| X-01 | Validated | DATA_REPAIR_REQUIRED + application remediation under X-26 | Store-less writers M-04, M-05 | Business owner (repair set); repository owner |
| X-02 | Validated | DATA_REPAIR_REQUIRED | `stock_pid_mapping` / `tbl_StockReassignmentAudit` exist *(8 Sep catalog)*; provenance unknown; usable only as input to a signed repair set | Business owner |
| X-03 | Validated | DATA_REPAIR_REQUIRED; unique key in Gate 3 after repair | No unique key; M-01 UPDATE-then-INSERT; M-02 second-row insert | Business owner; repository owner |
| X-04 | Validated | APPLICATION_REMEDIATION_REQUIRED | Master writers Q-01–Q-07; out-of-repo Q-09 Blocked | Repository owner |
| X-05 | Validated | APPLICATION_REMEDIATION_REQUIRED | Same | Repository owner |
| X-06 | Validated | BACKFILL_RULE_REQUIRED + APPLICATION_REMEDIATION_REQUIRED | §7.1 rule; writers omit `CompanyID`; read scope via vendor (§9) | Business owner (signed set); repository owner |
| X-07 | Validated | DATA_REPAIR_REQUIRED | Audit counts | Business owner |
| X-08 | Validated | DATA_REPAIR_REQUIRED | Audit counts; source path of orphan codes not determined | Business owner |
| X-09 | Validated | DATA_REPAIR_REQUIRED | Audit counts; `app/Delete_invoice.aspx.cs:166-168` deletes header and lines as separate, non-transactional statements (a possible orphan source) | Business owner |
| X-10 | Validated | APPLICATION_REMEDIATION_REQUIRED | §10.1 client-code-only readers; per-company vendor codes (`New_vendor.aspx.cs:158`) | Repository owner |
| X-11 | **Blocked** (22/22 UAT module bodies recorded; job steps unreadable) | DEPENDENCY_VALIDATION_REQUIRED | §12.2, supplied `flamex_uat` collection `2026-11-05T02:55:01.975Z`. `msdb.dbo.sysjobsteps` query rejected: `execute_query is disabled`. No rows returned. | DBA (job inventory); repository owner |
| X-12 | **Resolved** | — (decision A1-1) | Residual write-path and pre-invoice issues moved to X-28 | — |
| X-13 | Validated | APPLICATION_REMEDIATION_REQUIRED; backfill only if production has rows | Writers `hydrent_invoice.aspx.cs:201`, `CreateHydrentQuatation.aspx.cs:144,184` omit `CompanyID`; scope via client (§9) | Repository owner; X-18 |
| X-14 | **Resolved** (decision A1-2); residual **Blocked** | DATA_REPAIR_REQUIRED (`StoreID`), BACKFILL_RULE_REQUIRED (`CompanyID`), DEPENDENCY_VALIDATION_REQUIRED (writer, `Applied` semantics) | §6 | Business owner; DBA |
| X-15 | Validated | DATA_REPAIR_REQUIRED | Audit counts | Business owner |
| X-16 | **Resolved** (decision A1-3); residual Validated | DATA_REPAIR_REQUIRED for the 5 rows (attest or accept `Quantity_Num`) + APPLICATION_REMEDIATION_REQUIRED under X-27 | §5.1 conflict | Business owner; repository owner |
| X-17 | Validated | DATA_REPAIR_REQUIRED | Writers store UI or hard-coded store names (M-01, M-02, M-06) | Business owner |
| X-18 | **Blocked** | DEPENDENCY_VALIDATION_REQUIRED | Governance X-18: read-only `flamex_live` census of every ADR-002 §2 and §7 object, including tables empty on UAT. The connected MCP is `flamex_uat` only. No production rows were read. | DBA; repository owner |
| X-19 | **Blocked** | DEPENDENCY_VALIDATION_REQUIRED | §1; live UAT not accessible in this run | DBA / MCP reader; repository owner |
| X-20 | Validated (repository); live **Blocked** | APPLICATION_REMEDIATION_REQUIRED + DEPENDENCY_VALIDATION_REQUIRED (distribution, X-19) | §10.2 | Repository owner; DBA |
| X-21 | Validated (repository); definitions **Blocked** | APPLICATION_REMEDIATION_REQUIRED + DEPENDENCY_VALIDATION_REQUIRED (X-11) | §2.1, §2.2 | Repository owner; DBA |
| X-22 | Validated (repository); definitions and production **Blocked** | DEPENDENCY_VALIDATION_REQUIRED | §11 | Repository owner; DBA |
| X-23 | Validated; decision **Open** | BUSINESS_DECISION_REQUIRED | §11; separate hydrant master, no stock path. Decision record §16.2 | Business owner |
| X-24 | Validated; decision **Open** | BUSINESS_DECISION_REQUIRED | Readers `AddUser.aspx.cs`, `ViewUser.aspx.cs`, `home.aspx.cs`. Decision record §16.3 | Business owner |

### 13.1 New exceptions registered by Gate 2

| ID | Observed fact | Invariant / decision | Classification | Not admissible | Owner |
|----|---------------|----------------------|----------------|----------------|-------|
| X-25 | Invoice save decrements `tbl_NewProduct.Quantity` filtered by `CompanyID` and never moves `tbl_stock` (`Add_invoice.aspx.cs:1175-1184`, `Manual_Invoice.aspx.cs:473-479`). Invoices carry no store. Invoice delete does not reverse it. Decision record §16.4 (Open; invoice-store rule missing). | S1, S2, D5, INV-K5, A1-3 | APPLICATION_REMEDIATION_REQUIRED + BUSINESS_DECISION_REQUIRED (which store a sale issues from) | Defaulting to `STR001` or any single store; keeping master decrement as stock. | Business owner; repository owner |
| X-26 | Stock writers that drop or hard-code the store or key by name: M-04, M-05, M-06. | D5, INV-K1, INV-K2 | APPLICATION_REMEDIATION_REQUIRED | Hard-coded store; name-keyed updates. | Repository owner |
| X-27 | Stock writers / readers that use legacy `Quantity` without `Quantity_Num`: M-02–M-06, R-02. | A1-3, INV-K6 | APPLICATION_REMEDIATION_REQUIRED | Treating `Quantity` as authority in any new code. | Repository owner |
| X-28 | Invoice-child linkage: payments created by quotation number without invoice number; dues written only by quotation number; `Invoice_No` is a per-company serial, so number-only joins are tenant-ambiguous (§8). Decision record §16.5 (Open). | A1-1, INV-T3 | APPLICATION_REMEDIATION_REQUIRED + BUSINESS_DECISION_REQUIRED (pre-invoice receipts, allocation, due granularity) | Owning children through Client; through Quotation without an ADR amendment; resolving the parent by `Invoice_No` alone, including where it is currently unique. | Business owner; repository owner |
| X-29 | Runtime ownership inference through masters or a non-parent root (§9). | D7, INV-T2, A1-1 | APPLICATION_REMEDIATION_REQUIRED (needs architectural review of `AuthGuard`) | Keeping master joins once roots carry `CompanyID`. | Repository owner (architecture review) |
| X-30 | Two store keys in use (`Stores.StoreId` varchar on stock / purchase paths; `Stores.Id` int on PO snapshot). `StoreId` has no unique constraint *(8 Sep catalog)*. | INV-S1, INV-K1, INV-K2 | DEPENDENCY_VALIDATION_REQUIRED (live uniqueness), then Gate 3 design | Choosing a key without live uniqueness evidence. | Repository owner; DBA |
| X-31 | Objects outside the freeze listed in §7.3 have no ownership class. Inventory and decision record §16.6 (Open). | D8 | BUSINESS_DECISION_REQUIRED | Classifying by analogy or by current `CompanyID` default. | Business owner |

## 14. Migration-impact dependencies (inputs to Gate 3 design; no scripts)

**Ordering**

1. Close X-11, X-18, X-19 first. Gate 3 cannot size repairs or confirm absence of out-of-repo writers without them.
2. Store key (X-30): decide the single `StoreID`, prove uniqueness, add a unique constraint on it in `Stores` before any FK from stock or adjustments.
3. Product identity: repair orphan product ids (X-02, X-08, X-15) before FKs from `tbl_stock.Product_id`, `tbl_purches_details.Product_id`, `tbl_StockAdjustments.ProductID` to `tbl_NewProduct.ProductID`. Those columns are `varchar(50|100)`; the target key is `nvarchar(250)`, so a type change precedes the FK. (`tbl_RequisitionNew.ProductId` already has this FK.)
4. Stock repair (X-01, X-03, X-16, X-17) before the INV-K1 unique key on (`Product_id`, `ShippedToStoreId`) and before making the store column NOT NULL. Replace non-unique `IX_stock_ProductId (Product_id, ShippedToStoreName, Quantity)`.
5. `tbl_StockAdjustments`: add `StoreID` and `CompanyID` as NULL; apply signed sets; prove `Applied` rows are not re-posted; then NOT NULL + FKs.
6. Roots without `CompanyID` (`tbl_Purches`, hydrant pair, `tbl_StockAdjustments`): add NULL → apply signed ownership sets → NOT NULL + FK to `tbl_Company.ID` + index on (`CompanyID`, business key).
7. Roots with `CompanyID` but no FK (`tbl_Invoice`, `tbl_Chalan`, `tbl_Proforma`, `tbl_RequisitionMain`, `tbl_PO_Header`, `tbl_SalesVisitReport`): add FKs (only `tbl_Quotation.CompanyID` is declared). Add per-tenant uniqueness where numbering is per tenant (at least `tbl_Invoice` (`CompanyID`, `Invoice_No`)).
8. Invoice children: add the deterministic invoice reference after the X-28 decision; backfill only by the §8 rule.
9. Child equality checks (INV-T3) for every child that stores `CompanyID` (§7.2).
10. Shared master and global reference `CompanyID` columns stay in place through Gate 5 (governance §7). Defaults have system-generated names (for example `DF__tmp_ms_xx__Compa__2C738AF2`) and must be resolved by lookup, not by literal name, in the retirement change.

**Application remediation scope (Gate 7, from this inventory)**: product / category filters (§2.1, §2.2); stock writers M-01–M-06 and readers R-02–R-08; master quantity writers Q-01–Q-07 and readers Q-08; invoice stock path (X-25); purchase and hydrant writers (`CompanyID` stamping); invoice-child write paths (§8); client-code-only master reads (§10.1); State / City filters (§10.2); legacy product pages (§11); `AuthGuard.RecordInCompany` cases (§9, architecture review).

**Rollback and deployment**

- After the INV-K1 unique key exists, M-01 and M-02 INSERT branches can fail on duplicates; their Gate 7 change must ship with the schema change. Production deploys the database replay and the adapted build together (governance §8).
- Between Gate 5 and Gate 7 on UAT, `Add_invoice`, `Manual_Invoice`, purchase, and product pages may fail. Expected on UAT only.
- Out-of-repo procedures, triggers, and jobs that write stock or master quantity (M-07, Q-09) must be changed or disabled in the same package; until X-11 closes, they are an unbounded rollback risk.
- UAT rollback is checkpoint restore (governance §6); no partial patching.

## 15. Gate 2 exit status

| Exit criterion (governance §2) | Status |
|--------------------------------|--------|
| Dependency inventory written | Met for repository objects at `0c5cc72`, `dbo.tbl_CrossTenantDuplication` (§16.6 row 29), and the 22 UAT module bodies (§12.2). Not met for SQL Agent job steps (`execute_query` disabled; no rows returned). |
| Every object in ADR-002 §2 has a reader / writer list | Met for the repository (§2–§11) and for the 22 UAT modules (§12.2). Job readers and writers unknown. |
| X-11, X-18, X-19, X-20, X-21, X-22 have findings recorded | Recorded. X-11 is partial: module bodies are in; job-step text was rejected. X-18 stays blocked (no `flamex_live` census). X-19 stays blocked. X-20, X-21, and X-22 are unchanged. |
| Every exception in §4 has an owner | Met (role owners, §13). Names pending. |
| Impact report approved | **Pending** repository owner. Approval is an explicit statement by that owner, recorded in [`../HANDOFF.md`](../HANDOFF.md). This re-baseline does not grant it. No approver name is recorded. |

**Gate 2: not complete.**

### Remaining Gate 3 blockers

1. X-11 remainder — `msdb.dbo.sysjobsteps` could not be read (`execute_query is disabled`). The 22 §12 module bodies are recorded in §12.2. Do not treat the rejected query as proof that no job touches stock.
2. X-19 — live `flamex_uat` re-verification of: `CompanyID` presence and distribution on `tbl_State`, `tbl_City`, `tbl_Vat_Master`, `tbl_Factory`, `tbl_representative`, `tbl_ClientRegAddress`, and every §7.2 child; duplicate reference values across companies; `tbl_StockAdjustments.Applied` distribution; `Stores.StoreId` uniqueness; `tbl_Product` / `tbl_parentProduct` row counts.
3. X-18 — read-only `flamex_live` census of every object in ADR-002 §2 and §7, including tables empty on UAT. UAT evidence does not satisfy this.
4. Repository-owner approval of this report.
5. Business decisions: X-23, X-24, X-25 (sale store), X-28 (pre-invoice receipts), X-31. Analyzed in §16; all **Open** — no decision recorded by the repository / project owner. Each closes only when recorded by ADR-002 amendment (governance §3).
6. Signed data sets / rules before Gate 3 closes each item: X-01, X-02, X-03, X-06, X-07, X-08, X-09, X-14, X-15, X-16, X-17.

## 16. Business / design blocker decision records (X-23, X-24, X-25, X-28, X-31)

Added 2026-10-01 at the same commit, read-only. Inputs: the evidence already cited in §2–§13, re-read at the cited lines. No live database access was available, so no live fact below is new; X-11, X-18, X-19 stay **Blocked**. *(audit)* = [`OWNERSHIP_AUDIT.md`](OWNERSHIP_AUDIT.md); *(8 Sep catalog)* = unverified snapshot (X-19).

Labels inside each record: **Fact** (repository or audit observation), **Finding** (what facts imply against ADR-002), **Decision required** (not yet made), **Dependency** (blocked input).

Decision status values:

- **Open**: evidence and the required decision are recorded; the repository / project owner has not recorded the decision. The exception stays open.
- **Resolved**: the decision is recorded by ADR-002 amendment. None of the five records below is Resolved. A proposed design does not resolve an exception.

### 16.1 Summary

| ID | Decision status | Blocking question | Classification (unchanged) |
|----|-----------------|-------------------|----------------------------|
| X-23 | Open | Ownership class of `tbl_HydrantProduct` | BUSINESS_DECISION_REQUIRED |
| X-24 | Open | Class of `tbl_Departments` and of `tbl_Designations`; disposition of singular `tbl_Designation` | BUSINESS_DECISION_REQUIRED |
| X-25 | Open | Store a sale issues from (missing rule); issue event; reversal; historical sales | APPLICATION_REMEDIATION_REQUIRED + BUSINESS_DECISION_REQUIRED |
| X-28 | Open | Pre-invoice receipts; payment-to-invoice allocation; due granularity; TDS retention | APPLICATION_REMEDIATION_REQUIRED + BUSINESS_DECISION_REQUIRED |
| X-31 | Open | Class of each out-of-freeze object (§16.6) | BUSINESS_DECISION_REQUIRED |

### 16.2 X-23 — `tbl_HydrantProduct`

**Current evidence**

- Fact *(audit)*: PK `Id`; nullable `nvarchar(max)` `Product_code`, `Product_Name`, `short_form`, `Tax_Rate`, `base_rate`; no `CompanyID`; no FK; 0 rows on UAT. Production unknown (X-18).
- Fact: the only writer is `app/HydrantProduct.aspx.cs`: INSERT `:46` (no tenant column), DELETE `:105` (concatenated, by `Id`), unfiltered read `:34`. `Product_code` is derived from the globally latest row (`Findproductcode`, `:72-78`, `max(Id)` with no tenant predicate).
- Fact: the only other reader is `app/CreateHydrentQuatation.aspx.cs:54` (unfiltered). The same grid loads `tbl_Service` instead when the other radio option is selected (`:58`), and the chosen code is written to `tbl_qsHydrentDetails.Product_id` (`:184`). One column therefore holds either a hydrant product code or a service code, with no type marker.
- Fact: both pages derive from `System.Web.UI.Page` with a session check only (`HydrantProduct.aspx.cs:11,16`; `CreateHydrentQuatation.aspx.cs:12,29`); neither uses `SecurePage` or `CompanyContext`.
- Fact: no link to `tbl_NewProduct`, `tbl_stock`, `tbl_StockAdjustments`, or any stock writer (§3, §11). The code series is independent of `tbl_NewProduct.ProductID`.

**Architectural conflict**

- D4 governs Product identity on `tbl_NewProduct`. `tbl_HydrantProduct` has its own code series and no link to it, so D4 neither includes nor excludes it. D8 requires one of four classes; it has none.
- The code behaves as enterprise-wide (no tenant column, global code series). That is absence of tenant hardening, not a decision. The roots it feeds (`tbl_HydrentInvoice`, `tbl_qsHydrentQuotation`) are TRANSACTION_OWNED (X-13); the master's class decides whether one tenant's hydrant quotation may use another tenant's hydrant products.

**Decision required** (business owner)

1. SHARED_MASTER (one identity across tenants) or TENANT_MASTER (direct `CompanyID`)? GLOBAL_REFERENCE does not fit: rows carry a commercial `base_rate`.
2. If SHARED_MASTER: is the code inside the D4 enterprise Product Code namespace (no collision with `tbl_NewProduct.ProductID`) or a separate namespace?
3. Is the hydrant product master retained at all?

Not admissible: deriving the class from Client, Invoice, Quotation, hydrant transactions, or any other master; folding into `tbl_NewProduct`; reading "no `CompanyID`" as shared.

**Decision status:** Open. Dependency: X-18 (production rows; under TENANT_MASTER they need a signed ownership set).

**Migration / application consequence**

- TENANT_MASTER: Gate 3 adds `CompanyID` (NULL → signed set → NOT NULL + FK); Gate 7 stamps and filters by `CompanyContext` in both pages; code generation becomes per tenant or stays global by explicit rule.
- SHARED_MASTER: no tenant column; Gate 3 adds a unique key on the code.
- Either way, the mixed hydrant / service reference in `tbl_qsHydrentDetails.Product_id` needs a Gate 3 design, coupled to `tbl_Service` (§16.6 row 11).
- No D5 / INV-K consequence unless the business states hydrant products are physical inventory (a new decision).

### 16.3 X-24 — `tbl_Departments`, `tbl_Designations`, `tbl_Designation`

The plural and singular designation tables are different objects ([`../04_Department_Designation.md`](../04_Department_Designation.md), audit). `tbl_Departments` and `tbl_Designations` are the department and job-title lists for ERP logins. `tbl_Designation` (singular) is a leftover per-user menu-flag table, not a designation list. They are decided separately and must not be merged.

**Current evidence**

- Fact *(audit)*: `tbl_Departments`, `tbl_Designations`: `CompanyID int NULL` with named defaults (expressions unreadable); 4 rows each, every `CompanyID` NULL. `tbl_login.DepartmentID` / `DesignationID` nullable; 2 logins reference each list; 0 dangling ids; no declared FK.
- Fact: no repository writer for either list; `docs/04` states rows are added by SQL.
- Fact: selector reads are tenant-filtered: `app/AddUser.aspx.cs:42,51` and `app/ViewUser.aspx.cs:562,580` use `WHERE IsActive = 1 AND CompanyID = @CompanyID` from `CompanyContext` (`AddUser.aspx.cs:41,50` comment "Tenant-scoped").
- Fact: display reads are not tenant-filtered: `app/ViewUser.aspx.cs:197-198,279-280`, `app/AddUser.aspx.cs:87-88`, `app/home.aspx.cs:82-83` join by id only.
- Finding: every row is NULL and `CompanyID = @x` never matches NULL, so the AddUser / ViewUser selectors return no department and no designation in either tenant today. How the 2 existing links were set is not determinable from the repository.
- Fact *(audit)*: `tbl_Designation` (singular): PK `id`, no `CompanyID`, 0 rows. No repository reader or writer. Supplied 2026-11-05 UAT bodies: `dbo.vw_FullDesignation` and `flame_ex.vw_FullDesignation` are each `SELECT * FROM dbo.tbl_Designation` (§12.2). Class stays open.

**Architectural conflict**

- D8 needs one class per object. The data (all NULL) reads as unassigned or shared; the selector code reads as TENANT_MASTER; the display joins read as shared. None is a decision, and the register forbids reading NULL as shared or copying the login's company onto these rows.
- ADR-002 §1 places `tbl_login` in security / tenancy infrastructure outside the four classes. Whether its lookup lists follow it out of scope or sit inside D8 is also undecided.

**Decision required** (business owner)

1. `tbl_Departments`: GLOBAL_REFERENCE, TENANT_MASTER, or security-infrastructure lookup outside D8?
2. `tbl_Designations`: the same question, decided separately.
3. Under TENANT_MASTER, the owning tenant of each existing row (signed set; the referencing logins' `CompanyID` is not admissible).
4. `tbl_Designation` (singular): retain or retire.

**Decision status:** Open. The two view bodies are now known (§12.2). Retirement still depends on X-18 and a recorded decision. X-11's remaining gap is the unreadable SQL Agent inventory, not these two views.

**Migration / application consequence**

- GLOBAL_REFERENCE: tenant column retired under governance §7 rule 4; Gate 7 removes the `CompanyID` predicate at `AddUser.aspx.cs:42,51`, `ViewUser.aspx.cs:562,580`.
- TENANT_MASTER: signed per-row set, then `CompanyID NOT NULL` + FK; display joins add `CompanyID`; Gate 3 defines a login-vs-list tenant check (analogous to INV-M2).
- Until decided, the selectors stay empty (current behavior).
- `tbl_Designation`: retirement only through a gate like governance §7, after X-11.

### 16.4 X-25 — Invoice stock handling

**Required target behavior (fixed by ratified decisions; not open)**

- Invoice root and lines stay TRANSACTION_OWNED with direct `CompanyID` (D7). `tbl_Invoice` and `tbl_Invoice_details` already comply.
- Physical stock stays SHARED_MASTER; the stock row carries no tenant (INV-K4, ADR-002 §4 rule 7).
- A sale that issues goods is a movement on exactly one `tbl_stock` row identified by `ProductID + StoreID`, applied to `Quantity_Num` (D5, S1, A1-3, INV-K1).
- `tbl_NewProduct.Quantity` / `Quantity_Num` are neither read as available stock nor decremented by a sale (S2, INV-K5, INV-Q1).

**Current evidence**

- Fact: `app/Add_invoice.aspx.cs:1175-1184` decrements `tbl_NewProduct.Quantity` by `ProductID` + `CompanyID` inside the invoice transaction, skipped when the source is a Delivery Challan (`:1176`). `app/Manual_Invoice.aspx.cs:472-479` always decrements. Neither writes `tbl_stock`. All products are company 1 *(audit)*, so a company-2 invoice changes no quantity.
- Fact: no sales path captures a store. Text search finds no store reference in `Add_invoice`, `Manual_Invoice`, `Create_quotation`, `Edit_quatation_v2`, `edit_purchaseorder`, `add_chalan`, `Delete_invoice`. The invoice header INSERT (`Add_invoice.aspx.cs:1093`) has no store column. Only purchase paths are store-aware (§3 M-01–M-03).
- Fact: `add_chalan` has no stock writer (§3). With `:1176`, goods invoiced through a Delivery Challan reduce no quantity anywhere.
- Fact: invoice delete (`app/Delete_invoice.aspx.cs:176-201`) does not reverse the decrement. Invoices carry `status2` `Active` / `Block` (`Add_invoice.aspx.cs:774,1093`); no path reverses on Block.
- Fact: available stock shown on invoice pages is the master quantity (R-08).
- Fact (UAT body §12.2): `trg_Invoice_MarkPC_RealizeRev` updates `tbl_SalesVisitReport` and does not write `tbl_stock`. A job that might move stock was not readable (`execute_query` disabled).
- Finding: historical invoices never moved `tbl_stock` through the repository. Whether current `Quantity_Num` reflects sales through an out-of-repo sync is unknown (X-11).

**Architectural conflict**

The repository records sales issue on the shared Product master, partitioned by tenant, with no store (S1, S2, D5, INV-K5, INV-P2). The target needs a `StoreID` for every issued line. The repository has no source for that store.

**Decision required** (business owner; the repository establishes none of these)

1. **Sale store rule — missing.** For each invoiced line, which store issues the goods, and who selects it? There is no invoice-store field, default, or product-to-store rule in the repository. Not admissible: `STR001`, "Central Warehouse", the product's only stock row, the largest-stock store, a quantity split, or the item's purchase store.
2. **Issue event.** Is the physical issue posted at the invoice, at the Delivery Challan, or at whichever is first?
3. **Document scope.** Do Manual, Proforma-sourced, and Quotation / PO-sourced invoices all issue stock? Do service lines?
4. **Reversal.** Do invoice delete and `status2 = 'Block'` reverse the movement, to the same store?
5. **Negative stock.** May a sale take a store's `Quantity_Num` below zero?
6. **Historical invoices.** Must already-invoiced quantities be posted to `tbl_stock` in the migration? If yes, a signed store per historical line is required (DATA_REPAIR_REQUIRED; volume needs X-18).

Technical decision for Gate 3, only after 1–3: where `StoreID` is stored (header, line, or both) and its key type (X-30).

**Decision status:** Open. The invoice `StoreID` business rule is **unresolved**: the repository does not establish it, and Gate 2 records it as missing rather than defaulting it.

**Migration / application consequence**

- Gate 3 cannot design the invoice stock movement, any invoice `StoreID` column, or reversal semantics until questions 1–4 are answered and X-30 fixes the store key.
- Gate 7: remove Q-04 / Q-05 master decrements and R-08 master reads; post `ProductID + StoreID` movements on `Quantity_Num` in the invoice transaction; stop filtering Product by `CompanyID` (X-21).
- Historical master decrements are not copied into stock (X-04 / X-05). Only question 6 can authorize posting historical sales, and only from a signed set.

### 16.5 X-28 — Invoice payment / due / TDS linkage

A1-1 is fixed: the three tables are children of `tbl_Invoice`, owned by the `CompanyID` of the one invoice each row belongs to. This record covers the parent link and the rows that have no invoice.

**Current evidence**

- Fact: payment entry is source-document driven. `app/add_payment.aspx.cs:204` loads `tbl_Quotation` by `Quotation_no` + `CompanyID`. The payment INSERT (`:323`) writes `Quotation_No`, `Client_Id`, and amounts; no `Invoice_No`, invoice id, or `CompanyID`. `Button1_Click` (`:244-264`) does not require an invoice to exist.
- Fact: the invoice number is stamped afterwards:
  - `:364` joins `tbl_Invoice` on `Quotation_No` (invoice side filtered by `CompanyID`, payment side unfiltered), only for rows with `Due_amount = '0.00'` and `Invoice_No IS NULL`.
  - `:379-394` reads the first matching `tbl_Invoice` row (no `ORDER BY`, single `Read()`), then updates **every** payment row `WHERE Quotation_No = @Quotation_No`, with no `CompanyID`.
- Fact: one source document can have several invoices. `app/Add_invoice.aspx.cs:78-82` lists invoices `WHERE Quotation_No = @RefNo AND CompanyID = @CompanyID ORDER BY ID DESC`; `ReconcilePendingQuantities` (`:706`) bills the remaining quantity across prior invoices. `tbl_Invoice.Quotation_No` stores the source number, which can be a quotation, proforma, or challan (`:1093,1098`).
- Fact: invoice numbers repeat across tenants by construction: `"INV/C/" + FY + "/" + slNo`, with `slNo` = per-company `MAX(Sl_no)+1` (`Add_invoice.aspx.cs:1081-1088`). `C` is a literal, not a company code, so each company produces `INV/C/<FY>/1`.
- Fact: `tbl_invoice_due` is read, updated, and inserted only by `qutation_no` (`add_payment.aspx.cs:173,430,442,451`); with no row, the balance defaults to the document net amount (`:183`). `Invoice_no` *(8 Sep catalog)* is never written. The due row is a per-source-document running balance.
- Fact: the `tbl_invoice_payment_tds` writer `insertdsdetails` (`add_payment.aspx.cs:399-423`) has no caller in the repository; its `@invoice_no` comes from `lblinvoiceno.Text`, which no code assigns.
- Fact *(audit)*: 0 rows in all three tables on UAT. Production unknown (X-18).
- Fact: `AuthGuard.cs:645-648` scopes payment through `tbl_Quotation` (X-29).

**Architectural conflict**

1. A1-1 makes each row a child of exactly one invoice. The code makes payment and due children of the source document. Where that document has several invoices, a document-keyed row has no deterministic invoice parent, and `:379-394` picks one arbitrarily.
2. `Invoice_No` alone is not a cross-tenant key. A child resolved by number alone becomes ambiguous as soon as a second tenant invoices, even if the number is unique today.
3. A payment recorded before any invoice exists is a reachable code path. Under A1-1 it has no parent and no derivable owner. Owning it through `tbl_Quotation.CompanyID` is outside A1-1 and needs an amendment.
4. `:394` updates payment rows without `CompanyID`, so it can stamp one tenant's invoice number onto another tenant's payment with the same document number.

**Deterministic parent-link requirement** (technical; follows from A1-1 and ADR-002 §4 rule 2)

- Every invoice-child row stores a reference resolving to exactly one `tbl_Invoice` row: `tbl_Invoice.ID`, or (`CompanyID`, `Invoice_No`) backed by a unique constraint on (`CompanyID`, `Invoice_No`) (§7.1). Gate 3 chooses the form.
- The reference is written at insert time from an invoice selected in the working tenant, never stamped later by document number.
- Not admissible as parent link or ownership source: `Invoice_No` alone (including "unique today"), `Quotation_No`, `Client_Id`, or any master.
- A row resolving to zero or several invoices is an exception row (§4 rule 5), never assigned a tenant by default.

**Existing invoice children vs pre-invoice receipts**

| Case | Parent | Ownership |
|------|--------|-----------|
| Payment / due / TDS recorded for an existing invoice | The one `tbl_Invoice` row, through the reference above | Invoice `CompanyID` (A1-1) |
| Receipt recorded before any invoice exists | None | Not covered by A1-1. Not an invoice child until an invoice exists; not owned through the quotation without an amendment. BUSINESS_DECISION_REQUIRED. |

**Decision required** (business owner)

1. Are pre-invoice receipts (advance against a quotation / PO) a process to keep? If no: payment entry must require a selected invoice (Gate 7) and existing pre-invoice rows go to a signed repair set. If yes: their ownership and later allocation to invoices need an ADR-002 amendment. Gate 2 does not choose.
2. When a document has several invoices, is a payment applied to one user-selected invoice, or allocated across invoices?
3. Is `tbl_invoice_due` a per-invoice balance (consistent with A1-1) or a per-document balance (needs an amendment)?
4. Is `tbl_invoice_payment_tds` still in use? Its only writer is unreachable.

**Decision status:** Open. Dependency: X-18 (production rows in all three tables).

**Migration / application consequence**

- Gate 3: add the (`CompanyID`, `Invoice_No`) unique key on `tbl_Invoice` first; add the invoice reference to the three tables (and `CompanyID` if chosen under §4 rule 4) only after questions 1–3.
- Backfill: UAT has 0 rows. Production rows have no invoice id, no `CompanyID`, and at most a stamped `Invoice_No`; no admissible derivation rule exists (§8 item 3). Each needs a signed link / ownership set.
- Gate 7: rewrite `add_payment` write paths (`:323,364,394,430-451`), document-number readers (§8 table), print paths, and `AuthGuard.cs:645-648` (architecture review, X-29) to resolve through the invoice reference.

### 16.6 X-31 — Objects outside the freeze

Inventory: every object Gate 2 found that ADR-002 §2 does not list, excluding security / tenancy infrastructure (ADR-002 §1) and procedures / views / triggers (X-11).

Classification values:

- **Repository-supported**: written in the same transaction as, and keyed by, a ratified TRANSACTION_OWNED root, with `CompanyID` stamped from `CompanyContext`. Proposed only; the row closes when recorded by ADR-002 amendment.
- **Open**: repository evidence does not support a class. BUSINESS_DECISION_REQUIRED.
- **Not applicable on UAT**: object absent; nothing to classify unless production has it.

| # | Object | `CompanyID` *(8 Sep catalog)* | Repository evidence | Classification | Open question / dependency |
|--:|--------|-------------------------------|---------------------|----------------|----------------------------|
| 1 | `tbl_InvSiteAddress` | NOT NULL DEFAULT (1) | INSERT in the invoice transaction with `invoice_no` + `CompanyContext`: `app/Add_invoice.aspx.cs:1191,1204`, `app/Manual_Invoice.aspx.cs:487`. Reads by `invoice_no` only: `print/NewInvoice.aspx.cs:155`, `print/NewInvoiceDuplicate.aspx.cs:144`, `print/NewPaymentInvoice.aspx.cs:166`, `print/NewPaymentInvoiceDuplicate.aspx.cs:167`; scoped: `print/NewInvoice_v2.aspx.cs:43` | Repository-supported: TRANSACTION_OWNED child of `tbl_Invoice` | Link must be (`CompanyID`, `invoice_no`) or invoice id (§16.5 rule). X-19. |
| 2 | `tbl_QuoPriSerTogather` | NOT NULL DEFAULT (1) | INSERT in the quotation transaction with `CompanyContext`: `app/Create_quotation.aspx.cs:918`. Reads by `qutno` only: `app/add_chalan.aspx.cs:57`, `app/View_chalan.aspx.cs:122`, `app/View_PurchaseOrder.aspx.cs:69`, `app/seartch_invoice.aspx.cs:85`, `app/delete_purchaseorder.aspx.cs:47`; scoped: `app/add_payment.aspx.cs:52` | Repository-supported: TRANSACTION_OWNED child of `tbl_Quotation` | Readers need (`CompanyID`, quotation no). X-19. |
| 3 | `tbl_QuoPserTerm` | NOT NULL DEFAULT (1) | INSERT `app/Create_quotation.aspx.cs:899`; DELETE scoped `app/delete_purchaseorder.aspx.cs:207`; read scoped `print/PurchaseOrderPrintHelper.cs:437`, unscoped `print/NewQuotation.aspx.cs:423` | Repository-supported: TRANSACTION_OWNED child of `tbl_Quotation` | Same as 2. |
| 4 | `tbl_QutPaymentPhase` | NOT NULL DEFAULT (1) | INSERT `app/Create_quotation.aspx.cs:863`; DELETE scoped `app/delete_purchaseorder.aspx.cs:204`; read scoped `print/PurchaseOrderPrintHelper.cs:625`, unscoped `print/NewQuotation.aspx.cs:1514` | Repository-supported: TRANSACTION_OWNED child of `tbl_Quotation` | Same as 2. |
| 5 | `tbl_quotation_vat` | none | INSERT `app/Edit_quatation.aspx.cs:1212` (concatenated, number only, no tenant); DELETE `app/delete_purchaseorder.aspx.cs:199` by number only; read by number only in 11 print pages, including quotation, invoice, and proforma prints (`print/Quotation.aspx.cs:78`, `print/Invoice.aspx.cs:79`, `print/proforma_invoice.aspx.cs:78`, `print/NewProformaInvoice.aspx.cs:509`) | Open | No `CompanyID` stamping; proforma prints read the same column, so the parent document type is not established. |
| 6 | `tbl_Requisition_Approvals` | none | No repository reference; "keyed by `ReqNo`" is catalog-only | Open | Writers out-of-repo (X-11). |
| 7 | `tbl_PO_Amendment_Header` | none | No repository reference; 0 rows *(8 Sep catalog)* | Open | X-11, X-18, X-19 (FK to `tbl_PO_Header` unverified). |
| 8 | `tbl_PO_Amendment_Items` | none | Same | Open | Same. |
| 9 | `tbl_PO_Cancellation_Request` | none | Same | Open | Same. |
| 10 | `tbl_PO_Item_Cancellation` | none | Same | Open | Same. |
| 11 | `tbl_Service` | NOT NULL DEFAULT (1) | `app/Service_master.aspx.cs:34,46,90` read / INSERT / DELETE without tenant (concatenated); code from global `max(Id)` (`:63`); category list in `app/View_PR_Details.aspx.cs:359,422`, `app/RequisitionNew.aspx.cs:188,246`; hydrant line source `app/CreateHydrentQuatation.aspx.cs:58` | Open | Column default says tenant; code says shared; neither is a decision. Coupled to X-23. |
| 12 | `tbl_ProductCatagory` | none | No repository reference | Open | Retain or retire (D6 names only `tbl_Product` / `tbl_parentProduct`). X-11, X-18. |
| 13 | `tlb_General_expences` | none | INSERT `app/general_expences.aspx.cs:107` without tenant; id from global `max(id)` (`:138`); unscoped reads / delete `app/view_expencess_head.aspx.cs:40,46`, `app/Delete_general_expencess.aspx.cs:39,45,93`, `print/General_expencess_voutcher.aspx.cs:31` | Open | Tenant-owned root (then D7 `CompanyID` + signed backfill set) or enterprise-level ledger? |
| 14 | `tbl_patty_cash_expenses` | none | INSERT `app/patty_cash_expences.aspx.cs:136` without tenant; `closing_balance` is a running balance over all rows, and delete rewrites every later row (`app/Delete_patty_cash_expenses.aspx.cs:115,121`) | Open | Same as 13. If tenant-owned, the running balance must be partitioned per tenant. |
| 15 | `tbl_Expences` (expense heads) | none | Read / INSERT / DELETE unscoped: `app/Expenses_Head.aspx.cs:33,44,59`; lookup for 13 and 14 | Open | GLOBAL_REFERENCE or TENANT_MASTER. |
| 16 | `tbl_stock_archive` | none | No repository reference | Open | Retain as repair evidence (input to X-02 / X-16 sets) or retire. Writers out-of-repo (X-11). Classifying by analogy to `tbl_stock` / `tbl_NewProduct` is not admissible. Applies to rows 16–27. |
| 17 | `tbl_stock_duplicates_archive` | none | Same | Open | Same. |
| 18 | `tbl_stock_fix_audit` | none | Same | Open | Same. |
| 19 | `tbl_stock_old` | none | Same | Open | Same. |
| 20 | `tbl_stock_purge_archive` | none | Same | Open | Same. |
| 21 | `tbl_stock_review_missing_qty_rate` | none | Same | Open | Same. |
| 22 | `tbl_StockReassignmentAudit` | none | Same | Open | Same. |
| 23 | `tbl_StockReconciliationAudit` | none | Same | Open | Same. |
| 24 | `stock_pid_mapping` | none | Same | Open | Same. |
| 25 | `tbl_NewProduct_old` | none | Same | Open | Same. |
| 26 | `tbl_NewProduct_Archive` | none | Same | Open | Same. |
| 27 | `tbl_NewProduct_Archive2` | none | Same | Open | Same. |
| 28 | `tbl_requisition` | — | Referenced by `app/RequisitionCreate.aspx.cs` (§11); absent on UAT *(audit)* | Not applicable on UAT | X-18: classify only if production has it. |
| 29 | `tbl_CrossTenantDuplication` | Not in the 8 Sep catalog. `sql/create_cross_tenant_duplication.sql:18-32` at `282cfce`: `SourceCompanyID` and `TargetCompanyID` are `INT NOT NULL`, FKs to `dbo.tbl_Company(ID)` (`:48-49`, `:56-57`). No single `CompanyID` column. | INSERT inside the vendor/customer copy transaction: `app/DuplicationService.cs:286` and `:683` (Vendor), `:427` and `:839` (Customer), through `WriteDuplicationRelationship` (`:996-1028`). `SourceCompanyID` is `CompanyContext.CurrentCompanyID` (`:1017`); `TargetCompanyID` is the duplication target. Reads for the current company: `GetAuthorizedCounterpartLinks` (`:890-918`). File evidence is `282cfce`, contained in HEAD `4918b977`. Live presence was not checked. | Open | Two company FKs on one row, written with a TENANT_MASTER copy, not as a child of a TRANSACTION_OWNED root. ADR-002 §1 does not list it as security infrastructure. Classifying it from the company columns is not admissible. Live existence is X-19. |

Totals: 29 objects. 4 repository-supported (rows 1–4, proposed, not ratified), 24 open, 1 not applicable on UAT. **Decision status:** Open for every row until recorded by ADR-002 amendment. Row 29 does not change that status, and this re-baseline is not approval.

**Migration / application consequence:** Gate 3 cannot include any of rows 5–27 or row 29 in target DDL or backfill until classified. Rows 1–4, once ratified, follow ADR-002 §4 rules 2 and 4 (child equals root; readers gain the root `CompanyID`). Rows 13–14, if tenant-owned, need signed ownership sets for existing rows (D7 forbids deriving them from `emp_id` or any master).
