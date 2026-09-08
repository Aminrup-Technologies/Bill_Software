# Shared schema (UAT)

Live DDL for documentation is **`flamex_uat`**. Treat this file as the schema source of truth. Page catalogs still own page behavior; this file owns **what exists in the database**.

Do **not** paste connection strings, passwords, or other secrets here. Do **not** dump stored-procedure bodies.

Read after [`SHARED_CONTEXT.md`](SHARED_CONTEXT.md) and [`SHARED_RUNTIME.md`](SHARED_RUNTIME.md). Do not copy this file into domain catalogs.

**Snapshot:** 8 Sep 2026. SQL Server **2017 Express** (`WIN-8H6JMQBVGP2\MSSQLSERVER2017`). User objects: **111** tables, **38** procedures, **5** views, **22** foreign keys, **1** table type (`dbo.RequisitionItem_TVP`). Extra schemas besides `dbo`: `flame_ex`, `flamexliveapp`, `flmxuat` (almost empty; `flame_ex.vw_FullDesignation` only).

---

## 1. How to use this file

| Need | Where |
|------|--------|
| AuthN / tenancy / Bill.Master / print gate | [`SHARED_CONTEXT.md`](SHARED_CONTEXT.md) |
| C# SP **call sites**, handlers, in-repo `.sql` patches | [`SHARED_RUNTIME.md`](SHARED_RUNTIME.md) |
| Which page reads/writes an object | [`DATA_DICTIONARY.md`](DATA_DICTIONARY.md) |
| Whether an in-repo `.sql` has been applied | §4 below (UAT gaps) |
| Table / FK / unique-key facts for a domain | This file, then the matching `DOMAIN_*.md` |

In-repo scripts under `Bill_Software/corporate/business/sql/` are **patches**, not a versioned schema. Several have **not** been applied on UAT.

---

## 2. Naming that is real (not typos in the docs)

These spellings are the live object names:

| Live name | Meaning |
|-----------|---------|
| `tbl_Quotaion_details` | Quotation / client-PO lines |
| `tbl_Purches` / `tbl_purches_details` / `tbl_Purchess_payment` | Vendor purchase header / lines / payments |
| `tlb_General_expences` / `tlb_closing_balance` | GL expenses (leading `tlb_`) |
| `tbl_Expences` | GL expense **heads** (not visit expenses) |
| `tbl_patty_cash_expenses` | Petty cash |
| `tbl_Chalan` | DPCC / challan header |
| `tbl_qsHydrentQuotation` / `tbl_HydrentInvoice` | Hydrant track |
| `Sheet2$` | Excel leftover table (quotation-shaped columns) |

There is **no** table named `daily_rpt`. The visit header is **`tbl_SalesVisitReport`**. `daily_rpt.aspx` is the page.

There is **no** `tbl_Customers`, `tbl_Vendors`, `tbl_user_info`, `tbl_PO_Details`, or `tbl_PO_Charges`.

---

## 3. Commercial objects (PKs and business keys)

Two companies on this copy: `tbl_Company.ID` **1** Flame-Ex (`ComID=COMP01`, `ShortCode=FE`) and **2** AA Associates (`COMP02` / `AA`). Column is **`Name`**, not `Company_Name`. Kiosk image handler uses **`ComID`** + **`Signe`** (`image`). Almost all ERP rows sit on `CompanyID=1`. All 25 `tbl_login` rows are company 1.

| Object | PK | Business / unique key | UAT rows (snapshot) |
|--------|----|------------------------|----------------------|
| `tbl_login` | `Id` | `User_Id` unique **in data** (25/25); index `IDX_User_Id` is **not unique**. Filtered unique on `Email` / `Phone_no` when not null. | 25 |
| `tbl_Company` | `ID` | `ShortCode` NOT NULL | 2 |
| `tbl_Client` | `Id` | `Client_Id` unique in data | 423 |
| `tbl_Vendor` | `Id` | `Vendor_Id` unique in data (`AA09` style) | 486 |
| `tbl_NewProduct` | `Id` | **`ProductID` unique constraint** `UQ_tbl_NewProduct_ProductID` | 1441 |
| `tbl_Quotation` | `ID` | `Quotation_no`; `RecordType` | 159 |
| `tbl_Quotaion_details` | `Id` | line of a quotation / client PO | — |
| `tbl_Invoice` | `ID` | `Invoice_No` | 13 |
| `tbl_invoice_payment` | `ID` | `Payment_ID` | **0** |
| `tbl_Chalan` | `ID` | `Chalan_No` | 35 |
| `tbl_Proforma` | `ID` | `Invoice_No` | 7 |
| `tbl_RequisitionMain` | `id` | `ReqNo` NOT NULL | 12 |
| `tbl_PO_Header` | `PO_Id` | **`PO_No` unique** | 9 (all `Released`) |
| `tbl_PO_Items` | `PO_ItemId` | child of `PO_Id` | — |
| `tbl_Purches` | `ID` | `Purches_Id`; **no `CompanyID` column** | 163 |
| `tbl_SalesVisitReport` | `Id` | visit header | 78 |
| `tbl_Expenses` | `Id` | visit claims; **no `CompanyID`** | 2 |
| `tbl_card_login` / `tbl_employee` | `Id` / `ID` | kiosk; **0 rows** | 0 / 0 |
| `Roles` | `RoleId` | unique `RoleName` | 3 |
| `Permissions` | `PermissionId` | unique `PermissionKey` | 131 |
| `UserRoles` | `UserRoleId` | unique `(UserId, RoleId)` | 4 |
| `UserRememberTokens` | `TokenId` | — | 0 |
| `ActiveSessions` | `SessionToken` | — | 232 (stale tokens possible) |

`tbl_Quotation.RecordType` values on UAT are **only** `Quotation` (**75**) and `Purchase Order` (**84**). No third value.

`tbl_RequisitionMain.Status` on UAT: `Approved` (4), `PO_Created` (5), `Cancelled` (2), `Rejected` (1). `PO_Created` is a live status after vendor PO generation (catalog text that lists only Draft/Submitted/Approved/Cancelled/Rejected is incomplete).

---

## 4. Objects the code expects that are **missing on UAT**

Do not apply these unless asked. Document the fail-closed behavior.

| Missing object | Wired from | UAT behavior |
|----------------|------------|--------------|
| `dbo.UserCompanyAccess` | `AuthGuard.UserCanAccessCompany` / `GetAuthorizedCompanies` / `TryEnsureHomeMembership` (AddUser); DDL `…/sql/UserCompanyAccess.sql` | `SqlException` on read → **empty company list / access = false**. AddUser membership INSERT is in the same transaction as `tbl_login` and **fails** until the table exists. Company-gated pages fail closed. |
| `dbo.PasswordResetTokens` | `PasswordResetService`; DDL `…/sql/PasswordResetTokens.sql` | SQL 208 fails closed (already the service contract). |
| `dbo.tbl_requisition` / `dbo.tbl_requisitionBankDetails` | leftover `RequisitionCreate` / `RequisitionView` / `print/Requisition.aspx` | Those pages/print cannot read data. Modern PR uses `tbl_RequisitionMain`. |

Not missing in a way that matters for print: there is **no** `tbl_PrintAudit`. `EnsurePrint` uses `AuthGuard.RecordInCompany` / `TenantSql` against the document table (or a join for payments / purchases / hydrant). Resource `"unmapped"` still returns null SQL and **fails closed**.

Invented names that do **not** exist (and are not referenced as live tables): `tbl_Customers`, `tbl_Vendors`, `tbl_PO_Details`, `tbl_PO_Charges`, `tbl_LeaveTypes`, `tbl_AttendanceRules`, `tbl_ExpenseHead`.

---

## 5. Declared foreign keys (all 22)

Most commercial links are **not** declared. App code joins by business code (`Client_Id`, `Vendor_Id`, `Quotation_no`).

**Declared:**

| Child | Parent |
|-------|--------|
| `ActiveSessions.UserId` | `tbl_login.Id` |
| `UserRoles.UserId` | `tbl_login.Id` |
| `UserRoles.RoleId` / `tbl_login.RoleId` | `Roles.RoleId` |
| `RolePermissions.RoleId` / `.PermissionId` | `Roles` / `Permissions` |
| `tbl_Quotation.CompanyID` | `tbl_Company.ID` |
| `tbl_Quotation.VisitId` | `tbl_SalesVisitReport.Id` |
| `tbl_SalesVisitReport.ParentVisitId` | `tbl_SalesVisitReport.Id` (self) |
| `tbl_SalesVisitResponses.VisitId` | `tbl_SalesVisitReport.Id` |
| `tbl_RequisitionMain.VendorId` | `tbl_Vendor.Id` (**integer PK**) |
| `tbl_RequisitionNew.ProductId` | `tbl_NewProduct.ProductID` (**unique business key**, not `Id`) |
| `tbl_RequisitionNew.ParentCategoryId` | `tbl_NewparentProduct.id` |
| `tbl_PO_Items` / `tbl_PO_PartySnapshot` / `tbl_PO_PaymentDetails` `.PO_Id` | `tbl_PO_Header.PO_Id` |
| `tbl_NewProduct.parentId` | `tbl_NewparentProduct.id` |
| `tbl_Product.parentId` | `tbl_parentProduct.id` |
| `tbl_Attendance.AppliedShiftID` / `tbl_EmployeeShiftMapping.ShiftID` | `tbl_ShiftMaster.ShiftID` |
| `tbl_LeaveRequests.LeaveID` / `tbl_EmployeeLeaveBalance.LeaveID` | `tbl_LeaveMaster.LeaveID` |

**Not declared (inferred from C# + UAT data):**

- `tbl_Quotation.Client_Id` / `tbl_Invoice.Client_ID` / `tbl_Chalan.Client_ID` / `tbl_Proforma.Client_ID` → `tbl_Client.Client_Id` (varchar business code).
- `tbl_Purches.Client_Id` → **`tbl_Vendor.Vendor_Id`** (varchar). UAT: **163/163** purchases match a vendor code; **0** match `tbl_Vendor.Id` as a string. The column name is wrong; it is not `tbl_Client` and not the integer PK.
- `tbl_SalesVisitReport.CreatedByCode` → `tbl_login.User_Id` (no FK).
- `tbl_SalesVisitReport.CustomerName` is **free text** (no FK to `tbl_Client`).
- Visit expenses join `tbl_Expenses.VisitId` → visit `Id` in C# (`UserCanApproveExpense`); **no FK**.

`tbl_Quotation.VisitId` FK exists, but **0 of 159** quotations on this UAT copy have a visit id.

---

## 6. Tenant column gaps

`CompanyID int NOT NULL DEFAULT (1)` is present on many ERP headers (`tbl_Quotation`, `tbl_Invoice`, `tbl_Chalan`, `tbl_Proforma`, `tbl_RequisitionMain`, `tbl_PO_Header`, `tbl_SalesVisitReport`, `tbl_login`, `tbl_Client`, `tbl_Vendor`, `tbl_NewProduct`, …).

**No `CompanyID` (or kiosk `Company_ID`) on UAT**, including commercial tables the app still uses:

| Table | How print / lists isolate today |
|-------|----------------------------------|
| `tbl_Purches` / `tbl_purches_details` / `tbl_Purchess_payment` | Join `p.Client_Id = v.Vendor_Id` and filter `v.CompanyID` (`EnsurePrint` and some list pages). Many leftover search pages have **no** tenant predicate. |
| `tbl_invoice_payment` | `EnsurePrint` joins `tbl_Quotation` on `Quotation_No`. |
| `tbl_Expenses` | Scoped via visit `CompanyID` in AuthGuard. |
| `tbl_HydrentInvoice` / `tbl_qsHydrentQuotation` | Print joins `tbl_Client`. |
| `tlb_General_expences` / `tbl_patty_cash_expenses` / `tbl_Expences` | No tenant column; GL print is `unmapped` / fail-closed. |
| `tbl_stock` | No tenant column. |
| `tbl_Designation` (singular leftover) | No tenant column. |
| `UserRoles` | No `CompanyID` (already documented). |

Kiosk: `tbl_employee.Company_ID` is **varchar**, not ERP `CompanyID`. `tbl_card_login` has no company column.

---

## 7. Visit table (UAT vs audit leftovers)

`tbl_SalesVisitReport` columns that matter for leftovers:

- `CompanyID int NOT NULL DEFAULT (1)` — UAT **78 visits, 0 NULL, all company 1**. The D-01 story of historical NULL `CompanyID` is **not present in this copy**. It can still be true on an older live backup taken before `NOT NULL` / default. New INSERTs from code also set `@CompanyID`.
- `ApprovalStatus varchar(20) NULL DEFAULT ('Pending')` — UAT: Pending 61, Approved 15, Rejected 2.
- `VisitPhase nvarchar(50) NULL DEFAULT ('Planned')` — UAT: **76 NULL**, 2 `Executed`. Default was added later; historical rows were not backfilled.
- `CreatedByCode` — **2 rows = `FLM03`** (D-03 still evidenced). No NULL created-by.
- `ParentVisitId` self-FK exists.

Visit expenses: `tbl_Expenses` (not `tlb_General_expences`). Chat: `tbl_SalesVisitResponses`.

---

## 8. Two designation tables (do not mix)

| Table | Rows | Job |
|-------|-----:|-----|
| `tbl_Designations` | 4 | Job-title master (`DesignationID`, `DesignationName`). Used by `AddUser` / `ViewUser`. |
| `tbl_Designation` | **0** | Leftover **per-user menu-flag** table (`Home`, `Create_quotation`, `SalesTeam`, …). Not job titles. |

Views `dbo.vw_FullDesignation` and `flame_ex.vw_FullDesignation` are `SELECT * FROM dbo.tbl_Designation` (the leftover bitmap).

---

## 9. Dual PO / dual PR (schema)

**Vendor PO:** `tbl_PO_Header` (`PO_Id`, unique `PO_No`, `ReqNo`, `VendorId` int, `PO_Status`, `CompanyID`) + `tbl_PO_Items` + `tbl_PO_PartySnapshot` + `tbl_PO_PaymentDetails`. Also on UAT, unused from C# and **0 rows**: `tbl_PO_Amendment_Header` / `_Items`, `tbl_PO_Cancellation_Request`, `tbl_PO_Item_Cancellation`. View `vw_PO_Items_Effective` subtracts approved item cancellations.

**Client PO:** same as quotation (`tbl_Quotation` / `tbl_Quotaion_details`) with `RecordType = 'Purchase Order'`.

**Modern PR:** `tbl_RequisitionMain` (`VendorId` → `tbl_Vendor.Id`) + `tbl_RequisitionNew` (`ProductId` → `tbl_NewProduct.ProductID`). Numbering on UAT is mixed (`REQ/2026/…`, `PR-…`, `PR/2026/…`).

**Legacy bank PR tables** are not on UAT (§4).

---

## 10. Procedures: on UAT vs called from C#

Call-site index: [`SHARED_RUNTIME.md`](SHARED_RUNTIME.md) §5. `sp_getapplock` is a **system** procedure (not in the 38 user procs).

**Present and called from C#:** `sp_Requisition_CreateDraft`, `sp_RequisitionItem_BulkUpsert` (TVP `RequisitionItem_TVP`), `sp_SubmitRequisition`, `sp_CancelRequisition`, `sp_Requisition_Approve`, `sp_GeneratePO_FromReqNo`, `sp_ReleasePO_Final`, `sp_GetReleasedPO_Details`, `sp_RunAttendanceRulesEngine`, `sp_AllocateEmployeeLeaves`, `sp_GetActiveNotifications`, `sp_MarkNotificationRead`, `sp_SearchProductsFast`, `sp_GetProductStockByStore`, `sp_GetProductCategories`.

**On UAT, not in the C# call-site census** (bodies stay in SQL Server; do not paste them):

| Family | Names |
|--------|--------|
| Alternate PR approve/reject | `sp_ApproveRequisition`, `sp_RejectRequisition` (distinct from `sp_Requisition_Approve`) |
| Alternate PO release | `sp_ReleasePO` (app uses `sp_ReleasePO_Final`) |
| PO amendment / cancel | `sp_CreatePO_Amendment`, `sp_AddPO_AmendmentItem`, `sp_ApprovePO_Amendment`, `sp_RequestPOCancellation`, `sp_ApprovePOCancellation`, `sp_RequestPOItemCancellation`, `sp_ApprovePOItemCancellation`, `sp_CancelPO_Draft`, `sp_CancelPOItem_Draft` |
| Stock / product ops | `sp_SearchStockGrouped`, `InsertOrGetProduct`, `usp_CreateMissingMastersFromStock`, `usp_Reconcile_MasterToStock`, `usp_UpdateZeroMastersFromStock` |
| Other | `Get_Quotation_Details_With_Counts`, `sp_ManagePayrollStatus`, `sp_WriteProcessLog`, `CompareTablesAndGenerateAlter`, `CompareTablesFull` |

TVP `RequisitionItem_TVP` columns: `ProductId`, `ProductName`, `ParentCategoryId`, `HSNCode`, `Description`, `Qnty`, `Rate`, `DiscountPercent`, `DiscountAmount`, `IsTaxApplicable`, `GST`, `ItemOrder`.

---

## 11. Views (5)

| View | Role |
|------|------|
| `dbo.vw_PO_Items_Effective` | Ordered qty minus approved `tbl_PO_Item_Cancellation` |
| `dbo.vw_CategoryStoreStock_Safe` | Stock by parent category / store (`TRY_CAST` qty) |
| `dbo.vw_StockReconciliation` | `tbl_NewProduct.Quantity_Num` vs `tbl_stock` aggregates |
| `dbo.vw_FullDesignation` / `flame_ex.vw_FullDesignation` | Alias of leftover `tbl_Designation` |

---

## 12. Auth / RBAC objects on this copy

| Fact | UAT |
|------|-----|
| Roles | `Super Admin`, `Sales Executive`, `Procurement Officer` (all `CompanyID=1`) |
| `UserRoles` | **4** rows, all Super Admin. `UserRoles.UserId` is numeric `tbl_login.Id`. |
| `tbl_login.RoleId` | **All 25 NULL** (cosmetic FK unused in data). |
| `RolePermissions` | 137 |
| `tbl_Designation` leftover bitmap | 0 rows |

Kiosk `tbl_card_login` is a separate plaintext login table (0 rows here).

---

## 13. Orphan / archive tables (do not document as live UX)

`Sheet2$` (Excel import leftover), `tbl_NewProduct_old` / `_Archive` / `_Archive2`, `tbl_stock_old` and the stock archive / purge / fix / reconciliation audit tables, `stock_pid_mapping`, `tbl_ProcessLogs`, `CompareTables*` procs.

Hydrant tables exist but **0** quotation/invoice rows on this copy.

GL `tlb_General_expences` is **0** rows.

---

## 14. Still not this file

Full column lists for all 111 tables, full SP text, and screenshot manuals stay out of git. Refresh this document from UAT (counts, missing objects, FKs) when the copy is rebuilt — do not check in credentials.
