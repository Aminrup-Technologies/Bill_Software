# Multi-tenancy migration governance (UAT first)

**Status:** Governance plan. Frozen 2026-10-01 with [`ADR-002_MultiTenancy_Ownership_Model.md`](ADR-002_MultiTenancy_Ownership_Model.md).  
**Branch at freeze:** `July_to_Sept26_DevNSupport` at `0c5cc72`.  
**Current gate:** Gate 1 complete. Gate 2 **BLOCKED**. Repository inventory in [`GATE2_DEPENDENCY_IMPACT_REPORT.md`](GATE2_DEPENDENCY_IMPACT_REPORT.md). 22/22 UAT module bodies are in §12.2. The 2026-11-05 `flamex_uat` MCP job-step query was rejected (`execute_query is disabled`); no job-step rows were returned, so X-11 stays incomplete. X-18 remains a read-only `flamex_live` census of every ADR-002 §2 and §7 object, including tables empty on UAT; it was not run. X-19 was not re-run. X-20, X-21, and X-22 are unchanged. Repository-owner approval is pending: no explicit approval statement is in `HANDOFF.md`. Business decisions X-23, X-24, X-25, X-28, X-31 stay open (report §16). Gate 3 is not authorized.  
**Implementation:** Blocked. No migration SQL, schema change, procedure / trigger / view change, or application change is authorized by this document.

This plan sequences the move from the observed schema ([`OWNERSHIP_AUDIT.md`](OWNERSHIP_AUDIT.md)) to the ratified target ([`ADR-002`](ADR-002_MultiTenancy_Ownership_Model.md)). It does not contain executable SQL. Scripts are written in Gate 3 and run first in Gate 5, on `flamex_uat` only.

## 1. Principles

1. **UAT first.** Every schema and data step runs on `flamex_uat` first. Nothing runs on `flamex_live` until UAT is reconciled (Gate 6), functionally tested (Gate 8), and signed off (Gate 9), and a separate production approval exists (Gate 11).
2. **Production is a replay.** The production migration is the exact UAT-proven package, replayed. It is not designed separately (§8).
3. **No invented values.** A missing tenant, store, product, or quantity is never filled by default, by heuristic, or by inference from a master. Every repair value has a named source and an approver.
4. **Decisions stay closed.** D1–D8, S1, and S2 are not reopened. A conflicting fact is a registered exception (§4).
5. **Evidence stays separate.** Observed DB facts are not edited to match the target. Before and after states are both recorded.
6. **Restorable at every step.** Gate 5 starts only from a verified checkpoint. A failed run is restored to the checkpoint and rerun in full. It is never patched forward in place.
7. **Retire last.** Legacy objects are retired only after dependency validation (§7). Nothing is dropped during this plan's early gates.
8. **Repository rules apply.** `.sql` files use the NAME / WHEN / WHY / WHAT header ([`../CURSOR_RULES.md`](../CURSOR_RULES.md)). Application SQL stays parameterized. Commit and push follow the controlled Git workflow and need separate approvals.

## 2. Gate sequence

Gates run strictly in order. A gate starts only when the previous gate's exit criteria are met and recorded in [`../HANDOFF.md`](../HANDOFF.md).

| # | Gate | Entry | Work | Exit criteria (all required) |
|--:|------|-------|------|------------------------------|
| 1 | **Architecture freeze** | Ratified D1–D8, S1, S2 | ADR-002 and this plan | ADR-002 and this plan merged into the docs map. **Status: complete (2026-10-01).** |
| 2 | **Repository dependency and migration-impact analysis** | Gate 1 | Inventory every reader and writer of the objects in ADR-002 §2 (C#, ASPX, handlers, print pages, procedures, views, triggers). Obtain definitions with a login that has `VIEW DEFINITION` (X-11). Profile production read-only (X-18). Re-verify the 8 Sep catalog facts used in ADR-002 (X-19). Classify the objects ADR-002 left outside the freeze. | Dependency inventory written. Every object in ADR-002 §2 has a reader / writer list. X-11, X-18, X-19, X-20, X-21, X-22 have findings recorded. Every exception in §4 has an owner. Impact report approved. **Status: BLOCKED. 22/22 UAT module bodies are in report §12.2. SQL Agent job steps were unreadable (`execute_query is disabled`; no rows returned). X-18 `flamex_live` census not run. X-19 not run. X-20, X-21, X-22 unchanged. Repository-owner approval pending. Business decisions X-23, X-24, X-25, X-28, X-31 open. Gate 3 is not authorized.** |
| 3 | **UAT migration design** | Gate 2 | Target DDL per table, data-repair specifications, backfill rules, ordered script list, reconciliation queries for every ADR-002 invariant, rollback procedure, application-adaptation scope. | Every §4 exception is **Closed for design**: a repair data set signed by the business owner, an approved backfill rule, an approved application change, a dependency finding, or a recorded business decision. No exception is closed by assumption. Design approved. |
| 4 | **UAT backup / checkpoint** | Gate 3 | Full checkpoint of `flamex_uat` and baseline capture (§5.1). | Backup verified by restore test. Baseline counts, exception census, and invariant results captured and stored. Restore path rehearsed. |
| 5 | **UAT schema / data migration** | Gate 4 | Run the Gate 3 script set end to end on `flamex_uat` (§5.2). | Every script completed in order. Run log captured. No manual intervention outside the script set. On any failure: restore Gate 4 checkpoint, fix in Gate 3 artifacts, rerun from the start. |
| 6 | **UAT reconciliation** | Gate 5 | Run every reconciliation control (§5.4). | Every ADR-002 invariant returns zero violating rows. Every count, sum, and ownership check reconciles to the baseline or to an approved repair. Differences are explained line by line. Failure: restore checkpoint and return to Gate 3. |
| 7 | **Application adaptation** | Gate 6 | Change readers and writers found in Gate 2 to the target model (stock by `ProductID + StoreID`, no master-quantity authority, direct `CompanyID` on roots, global reference without tenant filter). | Changes reviewed under the repository's Security / Data / Regression / Scope gates. No AuthN / AuthZ architecture change. Build passes. |
| 8 | **Full UAT / regression testing** | Gate 7 | Functional and regression test of every affected page and print path, per tenant, plus cross-tenant isolation tests. | Test matrix executed with recorded PASS for each case. Invariants re-run after testing and still at zero violations. No open defect at blocking severity. |
| 9 | **UAT sign-off and migration freeze** | Gate 8 | Business and technical sign-off. Freeze script set and application commit. | Script set hashes and application commit SHA recorded. No further change to either without restarting from Gate 3. |
| 10 | **Production migration package** | Gate 9 | Assemble the replay package (§8). | Package complete. Production census matches the UAT-proven exception profile, or differences are routed back to Gate 3 and re-proven on UAT. |
| 11 | **Separate production approval and execution** | Gate 10 | Explicit production approval, then execution and production reconciliation. | Approval recorded separately from all earlier approvals. Production reconciliation passes the same controls as Gate 6. |

UAT reconciliation (Gate 6) and full UAT functional testing (Gate 8) must both succeed before production migration can be authorized. No production step is authorized by completing any single UAT gate.

Between Gate 5 and the end of Gate 7, UAT pages that depend on the old model may fail. That is expected on UAT only. In production, the database replay and the adapted application build are deployed as one change (§8).

## 3. Exception classifications

| Classification | Meaning | Closes when |
|----------------|---------|-------------|
| DATA_REPAIR_REQUIRED | Existing rows violate a target invariant. The correct value is not derivable. | The business owner supplies a row-level repair data set (the value for each affected row, or an explicit exclusion), and Gate 3 approves it. |
| BACKFILL_RULE_REQUIRED | A new or newly mandatory column needs values for existing rows. | An explicit rule naming an admissible source is approved. Sources forbidden by D7 (Vendor, Client, Product, Store, Stock) are not admissible. |
| APPLICATION_REMEDIATION_REQUIRED | Data can migrate, but application or SQL behaviour must change to honor the target. | The change is scoped in Gate 3 and delivered in Gate 7. |
| DEPENDENCY_VALIDATION_REQUIRED | A change cannot be designed until readers, writers, definitions, or production data are known. | Gate 2 records the finding and the exception is reclassified or closed. |
| BUSINESS_DECISION_REQUIRED | The ratified decisions do not cover this point. | The business records a decision. That decision is added to ADR-002 by amendment, not by silent edit. |

## 4. Exception register

Counts are from `flamex_uat` on 2026-10-01 ([`OWNERSHIP_AUDIT.md`](OWNERSHIP_AUDIT.md)) unless marked *(8 Sep catalog)*. No resolution is implied. “Not admissible” lists repairs this plan rules out because they would invent data or break D7.

The rows below are the Gate 1 register, kept as written. Gate 2 status, current classification, evidence, and owner for every ID are in [`GATE2_DEPENDENCY_IMPACT_REPORT.md`](GATE2_DEPENDENCY_IMPACT_REPORT.md) §13; where they differ, §13 is current. ADR-002 Amendment A1 resolves the decision part of X-12, X-14, and X-16; their residual work stays open there and in §4.2.

| ID | Observed fact | Invariant / decision affected | Classification | Not admissible |
|----|---------------|-------------------------------|----------------|----------------|
| X-01 | 137 `tbl_stock` rows have blank `ShippedToStoreId` and blank `ShippedToStoreName`. | D5, INV-K2 | DATA_REPAIR_REQUIRED | Assigning a default store; splitting quantity across stores; deleting rows without a recorded decision. |
| X-02 | 56 `tbl_stock` rows (56 distinct ids) have a `Product_id` absent from `tbl_NewProduct.ProductID`. | D4, INV-K3 | DATA_REPAIR_REQUIRED | Auto-creating masters from stock (including `usp_CreateMissingMastersFromStock`); matching by name; deleting rows without a decision. |
| X-03 | One (`Product_id`, `ShippedToStoreId`) group has 2 rows. One (`Product_id`, `ShippedToStoreName`) group has 2 rows. | INV-K1 | DATA_REPAIR_REQUIRED | Summing or keeping the larger quantity without a business-attested figure. |
| X-04 | 5 products where `tbl_NewProduct.Quantity_Num` differs from the summed `tbl_stock.Quantity_Num` (1 master greater, 4 stock greater; total absolute difference 16.0000; largest 10.0000). | S1, S2, INV-K5, INV-Q1 | APPLICATION_REMEDIATION_REQUIRED | Copying master quantity into stock; copying stock into master to make them agree. Under S1 the stock ledger is the authority; physical corrections go through a stock-level repair, not through the master. |
| X-05 | 13 products have NULL master `Quantity_Num` while stock rows exist. The master also has 13 unparsable `Quantity` text values and 8 rows where `Quantity` and `Quantity_Num` both parse and differ. | S2, INV-Q1 | APPLICATION_REMEDIATION_REQUIRED | Backfilling master quantity from stock as if it were authority. |
| X-06 | `tbl_Purches` (163 rows), `tbl_purches_details` (340), `tbl_Purchess_payment` (1), and `tbl_purches_due` (165) have no direct `CompanyID`. All 163 headers join to exactly one company-1 vendor by `Vendor_Id` (observed fact only). | D7, INV-T1, INV-T3 | BACKFILL_RULE_REQUIRED | Using the vendor join (or Client, Product, Store, Stock) as the backfill source. Children are covered by their header once the header is owned (ADR-002 §4). |
| X-07 | 2 `tbl_purches_due` rows match no purchase header. | INV-T3 | DATA_REPAIR_REQUIRED | Assigning a tenant to an orphan by default. |
| X-08 | 19 `tbl_purches_details` rows reference a `Product_id` absent from `tbl_NewProduct`. | D4 | DATA_REPAIR_REQUIRED | Auto-creating masters; name matching. |
| X-09 | 3 `tbl_Invoice_details` rows match no invoice header (their own `CompanyID` is 1). | INV-T3 | DATA_REPAIR_REQUIRED | Treating the line's own `CompanyID` as proof of a parent. |
| X-10 | 4 `Client_Id` codes exist once in each company. 4 of 13 invoices use such a code. Invoice `CompanyID` is populated, so ownership is not in doubt; a join to client on `Client_Id` alone is ambiguous. Factory, Representative, and Client Registration Address keyed by client code inherit the same ambiguity. | D1, D7, INV-M2 | APPLICATION_REMEDIATION_REQUIRED | Re-assigning invoice ownership from the client row; renaming client codes without a decision. |
| X-11 | The MCP reader login has no `VIEW DEFINITION`. Bodies of `sp_SearchProductsFast`, `sp_GetProductStockByStore`, `sp_GetProductCategories`, `sp_SearchStockGrouped`, `InsertOrGetProduct`, `usp_CreateMissingMastersFromStock`, `usp_Reconcile_MasterToStock`, `usp_UpdateZeroMastersFromStock`, the five views, and the four triggers (including `trg_tbl_NewProduct_Normalize`) are unreadable. | INV-K5, INV-P2, INV-C2, D6 | DEPENDENCY_VALIDATION_REQUIRED | Assuming a procedure is absent or harmless because its body is hidden. |
| X-12 | `tbl_invoice_payment`, `tbl_invoice_due`, `tbl_invoice_payment_tds` have no `CompanyID` (0 rows on UAT). Whether each is a transaction root or a child of the invoice is not decided. | D7, ADR-002 §4 | BUSINESS_DECISION_REQUIRED | Choosing root or child by convenience. Production row counts are unknown (X-18). |
| X-13 | `tbl_HydrentInvoice`, `tbl_qsHydrentQuotation`, `tbl_qsHydrentDetails` have no `CompanyID` (0 rows on UAT). Roots lack the column D7 requires. | D7, INV-T1 | APPLICATION_REMEDIATION_REQUIRED | Deriving ownership from `Client_ID`. If production has rows, a backfill rule becomes necessary (X-18). |
| X-14 | `tbl_StockAdjustments` (126 rows) has `ProductID` and no store column and no `CompanyID`. Whether its rows are physical inventory quantities under D5, and which ownership class it belongs to, is not decided. | D5, D8 | BUSINESS_DECISION_REQUIRED | Assigning a store or tenant to historical adjustments. |
| X-15 | 18 `tbl_StockAdjustments` rows have a `ProductID` absent from `tbl_NewProduct`. | D4 | DATA_REPAIR_REQUIRED | Auto-creating masters; name matching. |
| X-16 | 5 `tbl_stock` rows where varchar `Quantity` disagrees with `Quantity_Num`. Which column is the quantity of record inside the authoritative ledger is not decided. | S1, INV-K1 | BUSINESS_DECISION_REQUIRED | Picking a column per row. |
| X-17 | 1 `ShippedToStoreId` carries more than one distinct `ShippedToStoreName`. | INV-S1 | DATA_REPAIR_REQUIRED | Choosing a name by frequency. |
| X-18 | Every count in this register is from UAT. `flamex_live` has not been profiled. Production may contain rows in tables that are empty on UAT (invoice payment / due / TDS, hydrant, legacy product pair) and different exception counts. | All | DEPENDENCY_VALIDATION_REQUIRED | Treating UAT counts as production counts. |
| X-19 | Several facts in ADR-002 §2 come from the 8 Sep catalog, not the 2026-10-01 live check (`CompanyID` on `tbl_State`, `tbl_City`, `tbl_Vat_Master`, `tbl_Factory`, `tbl_representative`, `tbl_ClientRegAddress`, transaction children). The audit already found drift since 8 Sep (client and vendor counts, `UserCompanyAccess`, FK count). | All | DEPENDENCY_VALIDATION_REQUIRED | Designing from the 8 Sep snapshot without re-verification. |
| X-20 | `tbl_State`, `tbl_City`, and `tbl_Vat_Master` carry `CompanyID int NOT NULL DEFAULT (1)` *(8 Sep catalog)*. Row distribution by company and duplicate values across companies are not verified. | D2, D3, INV-G1 | DEPENDENCY_VALIDATION_REQUIRED | Merging same-named rows across companies, or dropping the column, before readers and writers are known. |
| X-21 | `tbl_NewProduct.CompanyID` and `tbl_NewparentProduct.CompanyID` are NOT NULL with named defaults. All rows are company 1. | INV-P2, INV-C2 | DEPENDENCY_VALIDATION_REQUIRED | Dropping or ignoring the column before every reader, writer, procedure, and trigger is known. |
| X-22 | `tbl_Product` and `tbl_parentProduct` exist with 0 rows, no `CompanyID`, and FK `FK_tbl_Product_tbl_parentProduct`. | D6 | DEPENDENCY_VALIDATION_REQUIRED | Dropping either table in this plan's early gates (§7). |
| X-23 | `tbl_HydrantProduct` exists with 0 rows and no `CompanyID`. Its ownership class is not decided. Whether it falls under D4 product identity is not decided. | D4, D8 | BUSINESS_DECISION_REQUIRED | Folding it into `tbl_NewProduct` or classifying it by assumption. |
| X-24 | `tbl_Departments` and `tbl_Designations` have nullable `CompanyID`, 4 rows each, all NULL; 2 logins reference them. `tbl_Designation` (singular) has 0 rows and no `CompanyID`. Ownership class is not decided. | D8 | BUSINESS_DECISION_REQUIRED | Reading NULL as shared, or copying the login company onto them. |

Informational, not an exception: 12 duplicate `NormalizedProductName` groups exist in company 1. D4 governs `ProductID`, which is unique; product names are not identity.

### 4.1 Gate 2 leads (analyzed 2026-10-01)

While reading the catalogs for this freeze, a search returned these application call sites. Gate 2 analyzed every lead; findings are in [`GATE2_DEPENDENCY_IMPACT_REPORT.md`](GATE2_DEPENDENCY_IMPACT_REPORT.md) §2–§11. Each lead is confirmed. `EditPurchase.aspx.cs` and `Delete_purtches.aspx.cs` key by store but change only the legacy `Quantity` column (report §3, M-02, M-03).

- `ImportProducts.aspx.cs` inserts `tbl_stock` without store columns and updates `tbl_stock` by `Product_name` (X-01, INV-K1, INV-K2).
- `Purches_new_vendor.aspx.cs` inserts and updates `tbl_stock` without store columns, using concatenated SQL (X-01, INV-K2).
- `newproduct_master.aspx.cs` writes `tbl_NewProduct.Quantity` / `Quantity_Num` and inserts `tbl_stock` (S2, INV-K5).
- `Purches_exting_vendor.aspx.cs`, `EditPurchase.aspx.cs`, `Delete_purtches.aspx.cs` change `tbl_stock` with a store key (INV-K1).
- `Product_stock.aspx.cs` calls `sp_SearchProductsFast` and `sp_GetProductStockByStore` (X-11).
- `productparent.aspx.cs`, `product_master.aspx.cs`, `Update_product.aspx.cs`, `Purches_new_vendor.aspx.cs`, `RequisitionCreate.aspx.cs` read or write `tbl_Product` / `tbl_parentProduct` (X-22). `RequisitionCreate.aspx.cs` filters those tables by `CompanyID`, a column the audit says they do not have.
- `New_vendor.aspx.cs` and `New_client.aspx.cs` read and insert `tbl_State` / `tbl_City` filtered by `CompanyID` (X-20).
- `productparent.aspx.cs`, `Update_product.aspx.cs`, `View_PR_Details.aspx.cs`, `RequisitionNew.aspx.cs` read `tbl_Vat_Master` without a tenant filter (X-20).

### 4.2 Exceptions registered by Gate 2

Full rows (observed fact, not admissible, owner) are in [`GATE2_DEPENDENCY_IMPACT_REPORT.md`](GATE2_DEPENDENCY_IMPACT_REPORT.md) §13.1. Decision records for X-23, X-24, X-25, X-28, X-31 are in report §16; all are open, and each closes only by a business decision added to ADR-002 by amendment (§3).

| ID | Summary | Classification |
|----|---------|----------------|
| X-25 | Invoice save decrements `tbl_NewProduct.Quantity` (filtered by `CompanyID`) and never moves `tbl_stock`; invoices carry no store. | APPLICATION_REMEDIATION_REQUIRED + BUSINESS_DECISION_REQUIRED |
| X-26 | Stock writers that drop or hard-code the store or key by product name. | APPLICATION_REMEDIATION_REQUIRED |
| X-27 | Stock writers / readers that use legacy `Quantity` without `Quantity_Num`. | APPLICATION_REMEDIATION_REQUIRED |
| X-28 | Invoice payment / due / TDS have no deterministic invoice reference; `Invoice_No` is a per-company serial; pre-invoice receipts have no invoice parent. | APPLICATION_REMEDIATION_REQUIRED + BUSINESS_DECISION_REQUIRED |
| X-29 | `AuthGuard` and purchase list scoping infer ownership through Vendor, Client, or Quotation. | APPLICATION_REMEDIATION_REQUIRED (architecture review) |
| X-30 | Two store keys (`Stores.StoreId`, `Stores.Id`); `StoreId` not constrained unique. | DEPENDENCY_VALIDATION_REQUIRED |
| X-31 | Objects outside the freeze have no ownership class. | BUSINESS_DECISION_REQUIRED |

## 5. UAT controls

### 5.1 Pre-migration (Gate 4)

- Full `COPY_ONLY` backup of `flamex_uat`, verified with `RESTORE VERIFYONLY` and an actual test restore under a different database name.
- Record the backup file name, size, checksum, and time in [`../HANDOFF.md`](../HANDOFF.md). No credentials in the record.
- Baseline capture, stored outside the migrated database: row counts per object in ADR-002 §2; `CompanyID` distribution per tenant table; sums of `tbl_stock.Quantity_Num` by (`Product_id`, store) and in total; sums of `tbl_NewProduct.Quantity_Num`; transaction totals per root (count and amount by `CompanyID` where present).
- Re-run the §4 exception census. Counts must match the Gate 3 design inputs. Any difference returns to Gate 3.
- Run every ADR-002 invariant and store the violating-row results as the “before” state.
- Confirm the run login, the target server and database name, and that no application session is writing to UAT during the run.

### 5.2 Migration (Gate 5)

- Run only the frozen Gate 3 script set, in its recorded order. Each script has the NAME / WHEN / WHY / WHAT header.
- Each script is idempotent or guarded so that a rerun after restore produces the same result.
- Each data step logs affected row counts. Counts are compared with the design's expected counts before the next step.
- Repairs apply only the business-signed repair data sets. No script contains a literal tenant, store, or product value that is not in a signed data set.
- No step drops a legacy object (§7). Columns that become redundant are retained until the retirement gate.
- Stop on the first unexpected count or error. Restore the checkpoint. Do not edit data by hand.

### 5.3 Post-migration (end of Gate 5)

- Schema check: every target column, constraint, and FK from Gate 3 exists with the designed nullability and type.
- Constraint check: the stock grain constraint (INV-K1), `CompanyID NOT NULL` on roots (INV-T1) and tenant masters (INV-M1), and FKs to `tbl_Company.ID` are enabled and trusted.
- Legacy objects still present and unchanged (`tbl_Product`, `tbl_parentProduct`, master quantity columns), unless Gate 3 recorded otherwise.
- Run log archived with script hashes.

### 5.4 Reconciliation (Gate 6)

- Every ADR-002 invariant returns zero violating rows.
- Row counts per object equal the baseline, minus or plus only the rows named in signed repair data sets.
- Total stock quantity equals the baseline total, adjusted only by signed repairs. Every (`ProductID`, `StoreID`) quantity is traceable to its baseline rows.
- Every transaction root has exactly one `CompanyID`. Per-tenant root counts and amounts equal the baseline, or the difference is a signed repair. Every child resolves to one root and agrees with its `CompanyID` (INV-T3).
- No root `CompanyID` came from a master join (INV-T2): the backfill log shows the approved rule as the source of each value.
- Tenant master children agree with parent client tenant (INV-M2).
- Global reference tables return the same reference values to every tenant (INV-G1).
- Results are recorded case by case. A failure is not waived; it returns to Gate 3 with a restore to the checkpoint.

## 6. Rollback

- **UAT:** rollback is restore of the Gate 4 checkpoint. After a restore, the full script set reruns from the first script.
- **Production:** the package defines a pre-execution backup, the restore procedure, the decision owner, and the latest time at which rollback is still permitted. Rollback after the application cutover also reverts the application to the prior build.

## 7. Legacy Product-chain retirement gate

D6 says `tbl_Product` and `tbl_parentProduct` are not required in the target. Retirement is a gated step, not an immediate action.

1. Gate 2 lists every reader and writer of `tbl_Product`, `tbl_parentProduct`, and `FK_tbl_Product_tbl_parentProduct` across C#, ASPX, print pages, procedures, views, and triggers (X-11, X-22), and production row counts (X-18).
2. Gate 7 removes or redirects every dependency. Gate 8 proves the affected pages.
3. Retirement (rename, revoke, or drop) is a separate, later change with its own UAT proof and its own production approval. It is not part of the first migration package unless Gate 3 includes it and Gate 9 signs it off.
4. The same gate applies to `tbl_NewProduct.Quantity` / `Quantity_Num` and to `CompanyID` on shared Product / Category / reference tables: no drop until dependencies are proven gone.

## 8. Production migration package (Gate 10)

The production package is the UAT-proven migration, replayed. It contains:

- The frozen Gate 9 script set, byte-identical, with the hashes recorded at Gate 9.
- The application build at the Gate 9 commit SHA. The database replay and this build deploy together.
- The production exception census (run read-only against `flamex_live` by an authorized DBA) compared with the UAT profile. Any production exception not proven on UAT returns to Gate 3, gets a signed repair or rule, and is re-proven on UAT before the package is accepted.
- Production repair data sets for production rows, signed by the business owner, produced by the same rules proven on UAT.
- The pre-migration controls (§5.1), migration controls (§5.2), post-migration controls (§5.3), and reconciliation controls (§5.4), applied unchanged to production.
- The rollback procedure (§6).

Nothing in the package is designed only for production. A production-only script, value, or step means the package is not a replay and is rejected.

## 9. Approvals

| Point | Approval |
|-------|----------|
| Gate 2 impact report | Repository owner |
| Gate 3 design, each exception closure | Repository owner; business owner for every repair data set, backfill rule, and business decision |
| Gate 9 UAT sign-off | Business owner and repository owner |
| Gate 11 production execution | Separate explicit approval, recorded after Gate 10. Earlier approvals do not imply it. |
| Commit / push of any artifact | Controlled Git workflow; commit approval is not push approval |

Approver names are recorded in [`../HANDOFF.md`](../HANDOFF.md) when each gate closes.
