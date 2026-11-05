# Ownership audit — live DB verification

**Verified:** 2026-10-01  
**Database:** `flamex_uat` on `WIN-8H6JMQBVGP2\MSSQLSERVER2017` (SQL Server 2017 Express 14.0.2037.2)  
**Access:** SELECT only, through the MCP-configured reader login. The database is not marked read-only. `VIEW DEFINITION` on the database is denied.  
**Repository baseline:** `docs/page-catalog/SHARED_SCHEMA.md` snapshot dated 8 Sep 2026, plus the checks named in the verification request.  
**Branch at verification:** `July_to_Sept26_DevNSupport` at `0c5cc72`.  
**Later object:** `dbo.tbl_CrossTenantDuplication` is not in this audit. Its create script is commit `282cfce` (2026-10-02), after this verification. This audit does not assert that the table is absent.  
**Later definitions:** this 2026-10-01 reader login could not read module text. A supplied 2026-11-05 `flamex_uat` collection, recorded in [`GATE2_DEPENDENCY_IMPACT_REPORT.md`](GATE2_DEPENDENCY_IMPACT_REPORT.md) §12.2, retrieved 22/22 of the named procedure, view, and trigger bodies. That collection is not part of this audit and is not a production census.

No schema, procedure, or application change was made. This section does not replace the 8 Sep snapshot. Where the live database disagrees with that snapshot, both statements stay visible.

## Checklist source

`docs/architecture/OWNERSHIP_AUDIT.md` had no prior body in git or the working tree. `docs/architecture/MULTI_TENANCY.md` and `docs/architecture/ARCHITECTURE.md` are also absent. `docs/41_CrossTenant_Duplication_Architecture.md` is absent; the cross-tenant architecture file in the tree is `docs/34_CrossTenant_Duplication_Architecture.md`.

Named items **D1–D8** and **I1–I7** are therefore **NOT_VERIFIABLE_FROM_DB**. They are not contradicted, because the statements those labels refer to were not available to compare. The checks below are the concrete validations from the verification request and from `SHARED_SCHEMA.md`. Implementation of ownership or stock tenancy stays blocked.

## How to read a status

| Status | Meaning |
|--------|---------|
| CONFIRMED_BY_DB | Live schema or aggregate data matches the repository claim. |
| CONTRADICTED_BY_DB | Live schema or aggregate data disagrees with a specific repository claim. The repository text is left unchanged. |
| PARTIALLY_CONFIRMED | Part of the claim matches; a named part does not, or only object existence was visible. |
| REPOSITORY_ONLY | Stated in the repository; this login could not see the object or its body. |
| BUSINESS_DECISION_REQUIRED | Schema or counts are recorded. They do not decide ownership. |
| NOT_VERIFIABLE_FROM_DB | The reader login cannot see the definition, or the source claim is not in the tree. |

Observed row counts are not an ownership rule.

## Permission limit

`HAS_PERMS_BY_NAME(..., 'VIEW DEFINITION')` = 0. `sys.procedures` count = 0. `sys.sql_modules` returned 9 rows (4 triggers and 5 views) and `DATALENGTH(definition)` for `dbo.vw_StockReconciliation` is NULL. `OBJECT_ID` is NULL for every procedure named below. That hides procedure bodies. It does not prove the procedures were dropped. Trigger and view **names** are visible. Their **bodies** are not.

## D1–D8

| ID | Status | Evidence |
|----|--------|----------|
| D1 | NOT_VERIFIABLE_FROM_DB | Label not present in the repository. |
| D2 | NOT_VERIFIABLE_FROM_DB | Label not present in the repository. |
| D3 | NOT_VERIFIABLE_FROM_DB | Label not present in the repository. |
| D4 | NOT_VERIFIABLE_FROM_DB | Label not present in the repository. |
| D5 | NOT_VERIFIABLE_FROM_DB | Label not present in the repository. |
| D6 | NOT_VERIFIABLE_FROM_DB | Label not present in the repository. |
| D7 | NOT_VERIFIABLE_FROM_DB | Label not present in the repository. |
| D8 | NOT_VERIFIABLE_FROM_DB | Label not present in the repository. |

## Catalog claims that the live database contradicts

These are contradictions of `SHARED_SCHEMA.md`. They are not silent edits of that file.

| Snapshot claim | Live evidence | Status |
|----------------|---------------|--------|
| §4 `dbo.UserCompanyAccess` missing on UAT | Table exists. Columns: `UserCompanyAccessId` int PK, `UserId` int NOT NULL, `CompanyID` int NOT NULL, `IsActive` bit NOT NULL, default constraint `DF_UserCompanyAccess_IsActive` (expression not readable). Unique constraint `UQ_UserCompanyAccess_User_Company` on `(UserId, CompanyID)`. FKs `FK_UserCompanyAccess_Login` → `tbl_login.Id` and `FK_UserCompanyAccess_Company` → `tbl_Company.ID`. 28 rows, all `IsActive = 1`: CompanyID 1 = 24, CompanyID 2 = 4. | CONTRADICTED_BY_DB |
| §5 22 foreign keys | 24 foreign keys. The two additions are the `UserCompanyAccess` FKs above. The other 22 names match the snapshot list. | CONTRADICTED_BY_DB |
| §3 `tbl_Client` 423 rows | 427 rows: CompanyID 1 = 423, CompanyID 2 = 4. Zero NULL `CompanyID`. | CONTRADICTED_BY_DB |
| §3 `tbl_Vendor` 486 rows | 492 rows: CompanyID 1 = 486, CompanyID 2 = 6. Zero NULL `CompanyID`. | CONTRADICTED_BY_DB |

Company-1 counts for client and vendor equal the 8 Sep totals. Company-2 rows are additional. This verification does not attribute those rows to a feature.

`dbo.PasswordResetTokens` and `dbo.tbl_requisition` are still absent from the 113 visible user tables. That agrees with §4. Status for those two absences: CONFIRMED_BY_DB.

## CompanyID presence and stored values

Two `tbl_Company` rows exist (`ID` 1 and `ID` 2). `CompanyID` below is `int` unless noted. Default **expressions** are not readable. A named default constraint is not evidence of the value `(1)`.

| Table | Company column | Nullability | Named default | Stored distribution |
|-------|----------------|-------------|---------------|---------------------|
| `tbl_NewProduct` | `CompanyID` | NOT NULL | yes (`DF__tmp_ms_xx__Compa__2C738AF2`) | 1441 rows, all CompanyID 1, 0 NULL |
| `tbl_NewparentProduct` | `CompanyID` | NOT NULL | yes | 24 rows, all CompanyID 1, 0 NULL |
| `tbl_Client` | `CompanyID` | NOT NULL | yes | 423 / 4 on companies 1 / 2 |
| `tbl_Vendor` | `CompanyID` | NOT NULL | yes | 486 / 6 on companies 1 / 2 |
| `tbl_login` | `CompanyID` | NOT NULL | not in this export | 25 rows, all CompanyID 1 |
| `tbl_Invoice` | `CompanyID` | NOT NULL | yes | 13 rows, all CompanyID 1 |
| `tbl_Invoice_details` | `CompanyID` | NOT NULL | yes | 38 rows, all CompanyID 1 |
| `tbl_Quotation` | `CompanyID` | NOT NULL | not exported here | 158 on company 1, 1 on company 2 (159 total, same total as the snapshot) |
| `tbl_PO_Header` | `CompanyID` | NOT NULL | — | 9 rows, all CompanyID 1 |
| `tbl_RequisitionMain` | `CompanyID` | NOT NULL | — | 12 rows, all CompanyID 1 |
| `tbl_SalesVisitReport` | `CompanyID` | NOT NULL | — | 78 rows, all CompanyID 1 |
| `Roles` | `CompanyID` | NOT NULL | — | 3 rows, all CompanyID 1 |
| `tbl_Departments` | `CompanyID` | NULL | `DF_tbl_Departments_CompanyID` | 4 rows, all NULL |
| `tbl_Designations` | `CompanyID` | NULL | `DF_tbl_Designations_CompanyID` | 4 rows, all NULL |
| `UserCompanyAccess` | `CompanyID` | NOT NULL | no | 24 / 4, all active |

No `CompanyID`, `CompanyId`, or `Company_ID` on: `tbl_stock`, `tbl_Purches`, `tbl_purches_details`, `tbl_Purchess_payment`, `tbl_purches_due`, `tbl_invoice_payment`, `tbl_invoice_due`, `tbl_invoice_payment_tds`, `tbl_HydrantProduct`, `tbl_HydrentInvoice`, `tbl_qsHydrentQuotation`, `tbl_qsHydrentDetails`, `tbl_Product`, `tbl_parentProduct`, `tbl_Designation`, `Stores`, `tbl_StockAdjustments`.

`tbl_Company.ComID` is `varchar(50)` NULL (business code, not the tenant int). `tbl_employee.Company_ID` is `varchar(50)`.

Status of the snapshot sentence that ERP headers such as quotation, invoice, product, client, vendor, login, visit, PR, and PO carry `CompanyID`: CONFIRMED_BY_DB for those tables. Status of “almost all rows are company 1”: PARTIALLY_CONFIRMED. It still describes the bulk of rows. It is no longer true that client, vendor, quotation, and membership rows are exclusively company 1.

## Products and categories

`tbl_NewProduct` PK is clustered `Id`. `ProductID` is `nvarchar(250)` NOT NULL. Unique constraint `UQ_tbl_NewProduct_ProductID` on `ProductID`. Non-unique indexes: `IX_NewProduct_NormalizedProductID` on `NormalizedProductID`; two non-unique indexes on `NormalizedProductName` (one includes `ProductID, ProductName, parentId`). FK `FK_tbl_NewProduct_tbl_NewparentProduct` (`parentId` → `tbl_NewparentProduct.id`).

| Check | Result | Status |
|-------|--------|--------|
| `ProductID` unique in schema and data | Constraint present. 1441 rows, 1441 distinct `ProductID`, 0 NULL | CONFIRMED_BY_DB |
| Same `ProductID` on more than one company | 0 | CONFIRMED_BY_DB |
| Any product row on company 2 | 0 | CONFIRMED_BY_DB |
| `NormalizedProductID` duplicate groups | 0 | CONFIRMED_BY_DB |
| `NormalizedProductName` duplicate groups | 12 groups inside the only populated company | PARTIALLY_CONFIRMED — name is not unique. This is not cross-tenant duplication. |
| Parent link | 1441 products, 0 NULL `parentId`, 0 missing parent, 0 parent in a different company | CONFIRMED_BY_DB |
| `Quantity` vs `Quantity_Num` on the master | 13 NULL `Quantity_Num`, 13 unparsable `Quantity` text, 8 rows where both parse and differ by more than 0.0001, 0 blank `Quantity` text | CONFIRMED_BY_DB as an internal divergence. Not an ownership rule. |

`tbl_NewparentProduct`: PK `id`. `CompanyID` NOT NULL. 24 rows, all company 1. Category ownership across tenants cannot be observed because company 2 has no category rows. Whether categories are tenant-owned is BUSINESS_DECISION_REQUIRED. The column and the single-company data are CONFIRMED_BY_DB.

`tbl_Product` and `tbl_parentProduct` exist, have no `CompanyID`, and have 0 rows. FK `FK_tbl_Product_tbl_parentProduct` remains. Status: CONFIRMED_BY_DB that the legacy pair is empty and has no tenant column.

## Stock grain

`tbl_stock` has no `CompanyID`. PK `PK_tbl_stock` is clustered `Id` only. The only other index is non-unique `IX_stock_ProductId` on `(Product_id, ShippedToStoreName, Quantity)`. There is no unique key on product, on product + store id, or on product + store name.

Columns include nullable `Product_id` varchar(50), nullable `Product_name` varchar(max), nullable `Quantity` varchar(50), nullable `Quantity_Num` decimal(18,4), nullable `ShippedToStoreId` varchar(50), nullable `ShippedToStoreName` varchar(200). `column_id` jumps from 12 to 18.

| Check | Result | Status |
|-------|--------|--------|
| No tenant column | Confirmed | CONFIRMED_BY_DB |
| Uniqueness enforced on product + store | Not enforced. Index is non-unique and includes `Quantity` | CONFIRMED_BY_DB |
| Data uniqueness of `(Product_id, ShippedToStoreId)` | 1 group has 2 rows (max 2) | PARTIALLY_CONFIRMED — almost unique in data, not constrained, one collision |
| Data uniqueness of `(Product_id, ShippedToStoreName)` | 1 group has 2 rows | same |
| Rows per `Product_id` | 26 product ids have more than one row (53 rows in those groups, max 3) | CONFIRMED_BY_DB — multiple stock rows per product exist |
| Name-only rows | 0 rows with blank `Product_id`. 0 rows that have a name and no product id | PARTIALLY_CONFIRMED — `Product_name` exists; current rows are not name-only |
| Blank store | 137 rows with blank `ShippedToStoreId` and 137 with blank `ShippedToStoreName` | CONFIRMED_BY_DB — those rows are product-scoped in the data |
| Store id in `Stores` | 1387 non-blank store ids match `Stores.StoreId`. 0 do not. `Stores` has 3 rows and no `CompanyID` | CONFIRMED_BY_DB |
| One store id, several names | 1 `ShippedToStoreId` has more than one distinct `ShippedToStoreName` | CONFIRMED_BY_DB |
| Stock product id not in `tbl_NewProduct.ProductID` | 56 rows, 56 distinct ids | CONFIRMED_BY_DB |
| Master products with no stock id | 0 of 1441 | CONFIRMED_BY_DB |

`tbl_StockAdjustments` has `ProductID` varchar(50) and no store column and no `CompanyID`. 126 rows, 0 blank `ProductID`, 108 match `tbl_NewProduct.ProductID`, 18 do not. A product-only stock-adjustment table exists. Status: CONFIRMED_BY_DB. Whether adjustments should be per store is BUSINESS_DECISION_REQUIRED.

## Reconciliation of master quantity and stock

Comparison used, because `vw_StockReconciliation` body is not readable: for each non-blank `tbl_stock.Product_id`, sum `Quantity_Num` (NULL treated as 0 in the sum) and compare to `tbl_NewProduct.Quantity_Num` on `ProductID`, tolerance 0.0001.

| Measure | Value |
|---------|------:|
| Master products | 1441 |
| Masters with at least one stock product id | 1441 |
| Equal `Quantity_Num` vs stock sum | 1423 |
| Master `Quantity_Num` NULL while stock rows exist | 13 |
| Divergent products | 5 (1 master greater, 4 stock sum greater) |
| Sum of absolute differences on those 5 | 16.0000 |
| Largest absolute difference | 10.0000 |
| Stock rows whose varchar `Quantity` disagrees with `Quantity_Num` | 5 (0 NULL `Quantity_Num`, 0 unparsable text) |

Status: CONFIRMED_BY_DB that master `Quantity_Num` and the sum of stock `Quantity_Num` diverge for 5 products, and that 13 masters have no numeric quantity while stock rows exist. This is not a statement of which number is correct.

## Purchase ownership

No `CompanyID` on `tbl_Purches`, `tbl_purches_details`, `tbl_Purchess_payment`, or `tbl_purches_due`. Header PK is `ID`. `Purches_Id` is not a unique constraint. In data, 163 headers have 163 distinct non-blank `Purches_Id`.

`tbl_Vendor.Vendor_Id`: 492 rows, 492 distinct, 0 blank, 0 codes present in both companies. There is no unique constraint on `Vendor_Id` (PK is `Id` only).

Join `tbl_Purches.Client_Id` = `tbl_Vendor.Vendor_Id`: 163 of 163 purchases match exactly one vendor row, that vendor’s `CompanyID` is 1, and 0 purchases match more than one company. 0 match company 2.

| Child | Rows | Header match |
|-------|-----:|--------------|
| `tbl_purches_details` | 340 | 340 match exactly one header. 0 blank `Product_id`. 321 product ids exist on `tbl_NewProduct`. 19 do not. |
| `tbl_Purchess_payment` | 1 | 1 matches exactly one header |
| `tbl_purches_due` | 165 | 163 match exactly one header. 2 match none. `tbl_purches_due` is a heap. `tbl_Purchess_payment` has PK `ID`. |

Status: CONFIRMED_BY_DB that these four tables have no direct tenant column. CONFIRMED_BY_DB that every current header joins to exactly one vendor and that vendor is company 1. Using that join as tenant ownership is BUSINESS_DECISION_REQUIRED. Current rows do not show a company-2 purchase.

## Invoice ownership

`tbl_Invoice` and `tbl_Invoice_details` have `CompanyID` NOT NULL. All 13 headers and all 38 detail rows are company 1. Header `Invoice_No` is unique in data (13/13) and is not the PK (`ID` is).

Detail join on `Invoice_No`: 35 details match one header with the same `CompanyID`. 0 match a header in another company. 3 details match no header. Those 3 still have their own `CompanyID` = 1.

`tbl_invoice_payment` (0 rows, PK `ID`), `tbl_invoice_due` (0 rows, heap, no PK), and `tbl_invoice_payment_tds` (0 rows, heap) have no `CompanyID`.

`tbl_Client.Client_Id`: 427 rows, 423 distinct codes, 0 blank. 4 codes occur twice, once in each company. 4 of 13 invoices use a `Client_ID` that exists on both companies. Invoice `CompanyID` is still populated on those headers. A join from invoice to client on `Client_ID` alone is ambiguous for those 4 invoices.

Status: CONFIRMED_BY_DB that header and detail rows have a direct non-null tenant value, all currently 1. CONFIRMED_BY_DB that payment, due, and TDS tables have no direct tenant column and no rows, so current rows there do not demonstrate ownership. Whether payment rows should inherit the invoice company is BUSINESS_DECISION_REQUIRED.

Hydrant invoice and quotation tables have no `CompanyID` and 0 rows. `tbl_HydrentInvoice` has nullable `Client_ID`. No ownership inference is made.

## Procedures, views, triggers

| Object | Visible | Body | Status |
|--------|---------|------|--------|
| `sp_SearchProductsFast` | `OBJECT_ID` NULL | no | NOT_VERIFIABLE_FROM_DB. Call site remains REPOSITORY_ONLY (`Product_stock.aspx.cs` per snapshot §10). |
| `sp_GetProductStockByStore` | `OBJECT_ID` NULL | no | NOT_VERIFIABLE_FROM_DB. REPOSITORY_ONLY call site. |
| `sp_GetProductCategories` | `OBJECT_ID` NULL | no | NOT_VERIFIABLE_FROM_DB. REPOSITORY_ONLY call site. |
| `sp_SearchStockGrouped` | `OBJECT_ID` NULL | no | NOT_VERIFIABLE_FROM_DB. Named in snapshot §10. |
| `InsertOrGetProduct` | `OBJECT_ID` NULL | no | NOT_VERIFIABLE_FROM_DB |
| `usp_CreateMissingMastersFromStock` | `OBJECT_ID` NULL | no | NOT_VERIFIABLE_FROM_DB |
| `usp_Reconcile_MasterToStock` | `OBJECT_ID` NULL | no | NOT_VERIFIABLE_FROM_DB |
| `usp_UpdateZeroMastersFromStock` | `OBJECT_ID` NULL | no | NOT_VERIFIABLE_FROM_DB |
| `dbo.vw_StockReconciliation` | yes | definition NULL | PARTIALLY_CONFIRMED — object exists; snapshot description of the comparison is REPOSITORY_ONLY |
| `dbo.vw_CategoryStoreStock_Safe` | yes | definition NULL | PARTIALLY_CONFIRMED — object exists; body REPOSITORY_ONLY |
| `dbo.vw_PO_Items_Effective` | yes | definition NULL | object exists; body NOT_VERIFIABLE_FROM_DB |
| `dbo.vw_FullDesignation` and `flame_ex.vw_FullDesignation` | yes | definition NULL | names match the snapshot; bodies NOT_VERIFIABLE_FROM_DB |
| `trg_tbl_NewProduct_Normalize` | yes | definition NULL | NOT_VERIFIABLE_FROM_DB whether it reads or writes `Quantity` / `Quantity_Num` |
| `trg_Block_PO_Update`, `trg_Invoice_MarkPC_RealizeRev`, `trg_Quotation_LinkVisit` | yes | definition NULL | not stock objects; bodies not readable |

No function was visible. A text search of module bodies for `tbl_stock`, `Quantity`, or `Quantity_Num` could not run, because every visible definition is NULL. Status of “which modules mutate stock or product quantity”: NOT_VERIFIABLE_FROM_DB.

Schema evidence that does not depend on those bodies: stock is not uniquely keyed by product + store; 137 stock rows have no store; `tbl_StockAdjustments` has product and no store; `tbl_stock.Product_name` exists and is unused as the only identifier in current rows.

## Four unresolved masters

| Object | What the database shows | What is not decided | Status |
|--------|-------------------------|---------------------|--------|
| `tbl_HydrantProduct` | Exists. PK `Id`. Nullable `Product_code`, `Product_Name`, `short_form`, `Tax_Rate`, `base_rate` (`nvarchar(max)`). No `CompanyID`. No FK. 0 rows. | Who owns a hydrant product | CONFIRMED_BY_DB for shape and emptiness. BUSINESS_DECISION_REQUIRED for ownership. |
| `tbl_Tieup_Company` | No user object whose name contains `Tieup`. `OBJECT_ID` NULL. | — | CONFIRMED_BY_DB that it is absent. Ownership NOT_VERIFIABLE_FROM_DB. |
| `tbl_Departments` | PK `DepartmentID`. `DepartmentName` varchar(100) NOT NULL. `IsActive` bit NOT NULL with an unreadable default. `CompanyID` int NULL with default `DF_tbl_Departments_CompanyID` (expression unreadable). 4 rows, `CompanyID` all NULL. No FK to `tbl_login`. | Whether NULL company means shared, unassigned, or unused | Schema CONFIRMED_BY_DB. Ownership BUSINESS_DECISION_REQUIRED. |
| `tbl_Designations` | PK `DesignationID`. `DesignationName` varchar(100) NOT NULL. `IsActive` bit NOT NULL with an unreadable default. `CompanyID` int NULL with default `DF_tbl_Designations_CompanyID` (expression unreadable). 4 rows, `CompanyID` all NULL. No FK from `tbl_login`. | Same as departments | Schema CONFIRMED_BY_DB. Ownership BUSINESS_DECISION_REQUIRED. |

`tbl_login` has nullable `DepartmentID` and `DesignationID` and NOT NULL `CompanyID`. Of 25 logins, 23 have NULL department and 23 have NULL designation. 2 department ids and 2 designation ids match the master tables. 0 point at a missing master row. Those master rows themselves have NULL `CompanyID`. That is a link in the data. It does not decide that departments or designations are global.

`tbl_Designation` (singular) still has no `CompanyID`, 0 rows, and PK `id`. Snapshot §8 row count and “leftover bitmap” description: CONFIRMED_BY_DB for emptiness and lack of a tenant column. The view body that the snapshot says aliases this table is NOT_VERIFIABLE_FROM_DB.

## Inference risks visible in relationships

I1–I7 could not be matched to text. The following are observed join facts only.

| Observed fact | Why it can be misread as ownership | Status |
|---------------|------------------------------------|--------|
| Purchase headers have no `CompanyID`, and all 163 match one company-1 vendor by `Vendor_Id` | Vendor company can be treated as purchase company without a decision | BUSINESS_DECISION_REQUIRED |
| 19 purchase lines have a product id that is not in `tbl_NewProduct` | Product id is not a guaranteed master key on those lines | CONFIRMED_BY_DB |
| 2 purchase-due rows match no header | Due amount is not always reachable from a purchase | CONFIRMED_BY_DB |
| 4 client codes exist in both companies; 4 invoices use such a code | `Client_ID` without `CompanyID` can attach an invoice to two clients | CONFIRMED_BY_DB |
| 3 invoice lines match no header, while line `CompanyID` is 1 | Line company and header company are not the same join | CONFIRMED_BY_DB |
| Stock has no company; 56 stock product ids are not in the master; 137 stock rows have no store | Stock cannot be assigned to a tenant or a store from the key alone | CONFIRMED_BY_DB for the gap. Tenant assignment BUSINESS_DECISION_REQUIRED |
| 2 logins point at department and designation rows whose `CompanyID` is NULL, while the login `CompanyID` is 1 | Login company does not copy onto those masters | CONFIRMED_BY_DB for the nulls. Meaning BUSINESS_DECISION_REQUIRED |
| `UserCompanyAccess` has 4 company-2 rows while every `tbl_login.CompanyID` is 1 | Home company and membership are different columns | CONFIRMED_BY_DB. This matches the membership model already in `docs/ARCHITECTURE.md`. It contradicts only the snapshot sentence that the table is missing. |

## Blocked

Ownership and stock-tenancy implementation stays blocked.

Still unresolved:

- D1–D8 and I1–I7 text, because it was not in the repository.
- Procedure, view, and trigger bodies, until a login with `VIEW DEFINITION` is used.
- Whether purchase, purchase payment, purchase due, invoice payment, and invoice due are owned directly or only through a parent.
- Whether NULL `CompanyID` on departments and designations is shared, unassigned, or leftover.
- Whether stock identity is product + store, product only, or both, given the schema allows all three and the data contains all three patterns.
- Which of master `Quantity_Num` or summed `tbl_stock.Quantity_Num` is authoritative for the 5 divergent products and the 13 NULL master quantities.

---

## Live DB definition access check

**Checked:** 2026-10-01, `flamex_uat`, same MCP reader login. SELECT against catalog views and permission functions only. No GRANT, impersonation, or other escalation was attempted. Result: **no definition is readable**, so definition inspection stopped here. A `Live DB Definition Verification` section was not added because there was nothing to verify.

### Permission boundary

| Check | Result |
|-------|--------|
| Login / database user | `flmx_mcp_reader` / `flmx_mcp_reader` |
| Role membership | `mcp_readonly` only. Not `sysadmin`, `db_owner`, `db_datareader`, or `db_ddladmin` |
| `fn_my_permissions(NULL,'DATABASE')` | `CONNECT`, `SELECT`, `VIEW ANY COLUMN ENCRYPTION KEY DEFINITION`, `VIEW ANY COLUMN MASTER KEY DEFINITION` |
| `fn_my_permissions(NULL,'SERVER')` | `CONNECT SQL`, `VIEW ANY DATABASE` |
| `VIEW DEFINITION` on database, `dbo` schema, `flame_ex` schema | 0, 0, 0 |
| `VIEW ANY DEFINITION` on server | 0 |
| `sys.parameters` rows visible | 0 |

Without `VIEW DEFINITION`, SQL Server hides procedure metadata entirely and returns NULL for `OBJECT_DEFINITION` on views and triggers. That is why the eight procedures below return `OBJECT_ID` NULL. Hidden metadata does not show that a procedure is missing.

### Object-by-object status

| Object | Visible to this login | Definition | Status |
|--------|-----------------------|------------|--------|
| `dbo.sp_SearchProductsFast` | No (`OBJECT_ID` NULL) | inaccessible | NOT_VERIFIABLE_FROM_DB |
| `dbo.sp_GetProductStockByStore` | No | inaccessible | NOT_VERIFIABLE_FROM_DB |
| `dbo.sp_GetProductCategories` | No | inaccessible | NOT_VERIFIABLE_FROM_DB |
| `dbo.sp_SearchStockGrouped` | No | inaccessible | NOT_VERIFIABLE_FROM_DB |
| `dbo.InsertOrGetProduct` | No | inaccessible | NOT_VERIFIABLE_FROM_DB |
| `dbo.usp_CreateMissingMastersFromStock` | No | inaccessible | NOT_VERIFIABLE_FROM_DB |
| `dbo.usp_Reconcile_MasterToStock` | No | inaccessible | NOT_VERIFIABLE_FROM_DB |
| `dbo.usp_UpdateZeroMastersFromStock` | No | inaccessible | NOT_VERIFIABLE_FROM_DB |
| `dbo.vw_StockReconciliation` | Yes (view) | NULL | NOT_VERIFIABLE_FROM_DB |
| `dbo.vw_CategoryStoreStock_Safe` | Yes (view) | NULL | NOT_VERIFIABLE_FROM_DB |
| `dbo.vw_PO_Items_Effective` | Yes (view) | NULL | NOT_VERIFIABLE_FROM_DB |
| `dbo.vw_FullDesignation` | Yes (view) | NULL | NOT_VERIFIABLE_FROM_DB |
| `flame_ex.vw_FullDesignation` | Yes (view) | NULL | NOT_VERIFIABLE_FROM_DB |
| `dbo.trg_tbl_NewProduct_Normalize` | Yes. AFTER INSERT, UPDATE on `tbl_NewProduct`, enabled | NULL | NOT_VERIFIABLE_FROM_DB |
| `dbo.trg_Block_PO_Update` | Yes. AFTER UPDATE on `tbl_PO_Header`, enabled | NULL | NOT_VERIFIABLE_FROM_DB |
| `dbo.trg_Invoice_MarkPC_RealizeRev` | Yes. AFTER INSERT, UPDATE on `tbl_Invoice`, enabled | NULL | NOT_VERIFIABLE_FROM_DB |
| `dbo.trg_Quotation_LinkVisit` | Yes. AFTER INSERT, UPDATE on `tbl_Quotation`, enabled | NULL | NOT_VERIFIABLE_FROM_DB |

`sys.sql_expression_dependencies` could not be used as a substitute. The reader's query failed on a column name before returning rows, and dependency rows are themselves filtered by `VIEW DEFINITION`.

### Output-column metadata (not definitions)

`sys.columns` is readable for views. These are **output column names only**. They do not show joins, filters, aggregation, or source tables.

| View | Output columns |
|------|----------------|
| `dbo.vw_StockReconciliation` | `ProductID` varchar, `SampleProductName` varchar, `SampleProductCode` varchar, `StoreID` varchar, `StoreName` varchar, `Master_Qty_Num` decimal, `Stock_Qty_Num` decimal, `Qty_Difference` decimal |
| `dbo.vw_CategoryStoreStock_Safe` | `ParentCategoryId` int, `ParentCategory` nvarchar, `ShippedToStoreId` varchar, `ShippedToStoreName` varchar, `TotalStockQty` decimal |
| `dbo.vw_PO_Items_Effective` | `PO_ItemId`, `PO_Id`, `ProductId` varchar, `OrderedQty`, `CancelledQty`, `EffectiveQty`, `Rate`, `TaxRate` |
| `dbo.vw_FullDesignation`, `flame_ex.vw_FullDesignation` | 98 columns each, starting `id`, `User_Id`, `Name`, `Home`, … — the same leading names as `tbl_Designation` |

What those names show, without inferring behavior:

- `vw_StockReconciliation` returns a store id and store name next to a master quantity and a stock quantity. The earlier section of this audit reconciled at **product** grain because the body was unreadable. Whether the view compares the whole master quantity against each store row, or something else, is NOT_VERIFIABLE_FROM_DB. This is not a contradiction of the earlier product-grain counts. Those counts came from this audit's own query, not from the view.
- No `CompanyID` column is in the output of either stock view. That does not show whether the view body filters on `CompanyID`.
- `vw_CategoryStoreStock_Safe` outputs category and store, which is consistent with the snapshot description “stock by parent category / store”. Status for that description: PARTIALLY_CONFIRMED (output shape only).
- `vw_FullDesignation` leading column names match `tbl_Designation`. Status for the snapshot claim that it is `SELECT * FROM dbo.tbl_Designation`: PARTIALLY_CONFIRMED (column names only, body unreadable).

### Required behavior questions

| Question | Status |
|----------|--------|
| Does any procedure, view, or trigger filter shared product, category, or stock data by tenant? | NOT_VERIFIABLE_FROM_DB |
| Does any of them treat `tbl_NewProduct.Quantity` or `Quantity_Num` as authoritative stock? | NOT_VERIFIABLE_FROM_DB |
| Does any of them derive stock ownership from `CompanyID`? | NOT_VERIFIABLE_FROM_DB |
| Does any of them drop the store dimension when aggregating or changing stock? | NOT_VERIFIABLE_FROM_DB. The two stock views output a store column; procedure behavior is unseen. |
| Does any of them create or repair master rows from stock? | NOT_VERIFIABLE_FROM_DB. The names `usp_CreateMissingMastersFromStock`, `usp_Reconcile_MasterToStock`, and `usp_UpdateZeroMastersFromStock` come from the snapshot and are REPOSITORY_ONLY. |
| Does any of them create or change product or category ownership? | NOT_VERIFIABLE_FROM_DB. `trg_tbl_NewProduct_Normalize` fires on every product insert and update; what it writes is unseen. |
| Is there hidden tenant behavior inconsistent with the frozen architecture? | NOT_VERIFIABLE_FROM_DB |

### Effect on earlier sections

No earlier finding in this file changes. Every row in “Procedures, views, triggers” above stays NOT_VERIFIABLE_FROM_DB or PARTIALLY_CONFIRMED as written. Ownership and stock implementation stays blocked.

To finish definition verification, a DBA would run the same read-only checks with a login that has `VIEW DEFINITION` on `flamex_uat`. Granting that permission is outside this audit.
