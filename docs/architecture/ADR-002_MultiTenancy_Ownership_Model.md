# ADR-002 — Multi-tenancy target ownership model (architecture freeze)

**Status:** Ratified. Architecture frozen 2026-10-01.  
**Branch at freeze:** `July_to_Sept26_DevNSupport` at `0c5cc72`.  
**Scope:** Target ownership of masters, reference data, stock, and transactions across `tbl_Company` tenants.  
**Implementation:** Blocked. See [`UAT_MIGRATION_GOVERNANCE.md`](UAT_MIGRATION_GOVERNANCE.md). Gate 1 (this freeze) is complete. Gate 2 is not complete: the supplied 2026-11-05 `flamex_uat` collection recorded 22/22 module bodies in the impact report §12.2, SQL Agent was not queried, live UAT re-verification and production profiling remain open, and business decisions X-23, X-24, X-25, X-28, X-31 are open (report §16; none recorded by this ADR). Re-baseline at published HEAD `4918b977`: `dbo.tbl_CrossTenantDuplication` is report §16.6 row 29 and is not classified here. This sentence is not repository-owner approval. Gate 3 is not authorized.  
**Amended:** 2026-10-01, Amendment A1 (below).

This ADR amends the multi-tenant isolation principles in [`../ARCHITECTURE.md`](../ARCHITECTURE.md). It does not replace them. Authentication, `UserCompanyAccess` membership, `AuthGuard` / `SecurePage`, Decision #8, Decision #9, and the [`../22_Security_Baseline.md`](../22_Security_Baseline.md) contract are unchanged.

`docs/architecture/MULTI_TENANCY.md`, `docs/architecture/ARCHITECTURE.md`, and `docs/41_CrossTenant_Duplication_Architecture.md` do not exist in the tree. This ADR does not create them. The existing cross-tenant architecture file is [`../34_CrossTenant_Duplication_Architecture.md`](../34_CrossTenant_Duplication_Architecture.md); Cross-Tenant Duplication v1.1 stays closed.

## Evidence and decision are separate

| Source | What it holds | Authority |
|--------|---------------|-----------|
| [`OWNERSHIP_AUDIT.md`](OWNERSHIP_AUDIT.md) (2026-10-01, `flamex_uat`, SELECT only) | Observed live-DB facts | Evidence. Not rewritten by this ADR. Its D1–D8 rows say NOT_VERIFIABLE_FROM_DB because the decision text did not exist when the audit ran. This ADR is now that text. |
| [`../page-catalog/SHARED_SCHEMA.md`](../page-catalog/SHARED_SCHEMA.md), [`../page-catalog/UAT_CATALOG.md`](../page-catalog/UAT_CATALOG.md) (8 Sep 2026 snapshot) | Observed schema at snapshot time | Evidence. Where the audit disagrees, the audit is the newer fact. |
| This ADR | Ratified target architecture | Normative. |

An observed fact that conflicts with this ADR is a **migration exception**. It is not a reason to reopen a decision. Exceptions are registered in [`UAT_MIGRATION_GOVERNANCE.md`](UAT_MIGRATION_GOVERNANCE.md) §4.

## Ratified decisions

These are business decisions. They are not reopened or reinterpreted by later analysis.

| ID | Decision |
|----|----------|
| D1 | Vendor, Client, Factory, Representative, and Client Registration Address are **TENANT_MASTER**. |
| D2 | VAT / tax master is **GLOBAL_REFERENCE** (shared). |
| D3 | State and City are **GLOBAL_REFERENCE** (shared). |
| D4 | `ProductID` / Product Code is unique enterprise-wide, across both tenants. |
| D5 | Every physical inventory quantity belongs to a Store / Warehouse. |
| D6 | `tbl_Product` and `tbl_parentProduct` are not part of the target architecture. Retirement happens only after dependency validation (governance Gate 2 and the retirement gate in §7 of the governance plan). |
| D7 | Every tenant-owned transaction root carries a direct `CompanyID`. Ownership is never inferred from Vendor, Client, Product, or any other master. |
| D8 | The target ownership model is the four classes GLOBAL_REFERENCE / SHARED_MASTER / TENANT_MASTER / TRANSACTION_OWNED (below). |
| S1 | `tbl_stock` is the sole authoritative physical-stock ledger. |
| S2 | `tbl_NewProduct.Quantity` and `tbl_NewProduct.Quantity_Num` are not authoritative physical stock. |

The freeze instruction also ratifies: Product identity, Category identity, and Store identity are shared; stock grain is `ProductID + StoreID`; shared Product / Category / Store / Stock identity carries no `CompanyID`. Those are recorded as invariants in §3.

### Amendment A1 (2026-10-01)

Ratified by the repository owner in the Gate 2 instruction. Added by amendment, per governance §3. A1 closes the decision part of X-12, X-14, and X-16. Residual data and application work stays registered.

| ID | Decision |
|----|----------|
| A1-1 | `tbl_invoice_payment`, `tbl_invoice_due`, and `tbl_invoice_payment_tds` are **children of `tbl_Invoice`**. Each row's owner is the `CompanyID` of the invoice it belongs to (§4 rule 2). |
| A1-2 | `tbl_StockAdjustments` records actual physical stock adjustments. It is a **tenant-owned inventory movement** (TRANSACTION_OWNED root, direct `CompanyID`). Each adjustment changes the shared `tbl_stock` row for (`ProductID`, `StoreID`) (§4 rule 7). |
| A1-3 | `tbl_stock.Quantity_Num` is the **sole authoritative physical quantity**. `tbl_stock.Quantity` is legacy compatibility data only. |

## 1. Ownership classes

| Class | Meaning | Tenant partition |
|-------|---------|------------------|
| **GLOBAL_REFERENCE** | Static reference / lookup data used by every tenant (geography, tax rates). Not a business entity owned by anyone. | None. No `CompanyID` filter on read. No tenant-specific copies. |
| **SHARED_MASTER** | Enterprise business master data and the physical-stock ledger, shared by every tenant with a single identity. | None. One row per identity across the enterprise. |
| **TENANT_MASTER** | Business master data owned by exactly one tenant. | Direct non-null `CompanyID` on every row. |
| **TRANSACTION_OWNED** | A business transaction (root) or a line / child of one. | Root: direct non-null `CompanyID`. Child: ownership follows its root (§4). |

Security and tenancy infrastructure (`tbl_Company`, `tbl_login`, `UserCompanyAccess`, `Roles`, `UserRoles`, `RolePermissions`, `Permissions`, `ActiveSessions`) is governed by [`../22_Security_Baseline.md`](../22_Security_Baseline.md) and [`../ARCHITECTURE.md`](../ARCHITECTURE.md). It is outside the four classes and is not reclassified here. The card kiosk stays isolated.

## 2. Target ownership matrix

“Observed” is from [`OWNERSHIP_AUDIT.md`](OWNERSHIP_AUDIT.md) unless marked *(8 Sep catalog)*, which means [`../page-catalog/UAT_CATALOG.md`](../page-catalog/UAT_CATALOG.md) / [`../page-catalog/SHARED_SCHEMA.md`](../page-catalog/SHARED_SCHEMA.md) and not re-verified live on 2026-10-01. “Exception” ids point at the governance register.

### 2.1 Reference and master objects

| Object | Target class | Basis | Observed `CompanyID` | Gap vs target | Exception |
|--------|--------------|-------|----------------------|---------------|-----------|
| `tbl_State` | GLOBAL_REFERENCE | D3 | `int NOT NULL DEFAULT (1)` *(8 Sep catalog)*; distribution not verified | Tenant column present on global reference | X-20 |
| `tbl_City` | GLOBAL_REFERENCE | D3 | `int NOT NULL DEFAULT (1)` *(8 Sep catalog)*; distribution not verified | Tenant column present on global reference | X-20 |
| `tbl_Vat_Master` | GLOBAL_REFERENCE | D2 | `int NOT NULL DEFAULT (1)` *(8 Sep catalog)*; distribution not verified | Tenant column present on global reference | X-20 |
| `tbl_NewProduct` | SHARED_MASTER (Product identity) | D4, freeze invariant | NOT NULL; 1441 rows, all company 1 | Tenant column present on shared identity; master quantity columns present | X-21, X-04, X-05 |
| `tbl_NewparentProduct` | SHARED_MASTER (Category identity) | freeze invariant | NOT NULL; 24 rows, all company 1 | Tenant column present on shared identity | X-21 |
| `Stores` | SHARED_MASTER (Store identity) | D5, freeze invariant | None; 3 rows | None for tenancy | — |
| `tbl_stock` | SHARED_MASTER (physical-stock ledger) | S1, D5, freeze invariant | None | Grain not enforced; rows without store; orphan products | X-01, X-02, X-03, X-16, X-17 |
| `tbl_Vendor` | TENANT_MASTER | D1 | NOT NULL; 486 / 6 on companies 1 / 2 | `Vendor_Id` not constrained unique | — |
| `tbl_Client` | TENANT_MASTER | D1 | NOT NULL; 423 / 4 | Same `Client_Id` exists in both companies | X-10 |
| `tbl_Factory` | TENANT_MASTER | D1 | `int NOT NULL DEFAULT (1)` *(8 Sep catalog)* | Parent-client consistency unverified | X-10 |
| `tbl_representative` | TENANT_MASTER | D1 | `int NOT NULL DEFAULT (1)` *(8 Sep catalog)* | Parent-client consistency unverified | X-10 |
| `tbl_ClientRegAddress` | TENANT_MASTER | D1 | `int NOT NULL DEFAULT (1)` *(8 Sep catalog)* | Parent-client consistency unverified | X-10 |
| `tbl_Product` | Not in target (legacy) | D6 | None; 0 rows | Retirement pending dependency validation | X-22 |
| `tbl_parentProduct` | Not in target (legacy) | D6 | None; 0 rows | Retirement pending dependency validation | X-22 |
| `tbl_HydrantProduct` | **Unclassified** | — | None; 0 rows | Class not decided | X-23 |
| `tbl_Departments` | **Unclassified** | — | NULL; 4 rows, all NULL | Class not decided | X-24 |
| `tbl_Designations` | **Unclassified** | — | NULL; 4 rows, all NULL | Class not decided | X-24 |
| `tbl_Designation` (singular leftover) | **Unclassified** | — | None; 0 rows | Class not decided | X-24 |
| `tbl_Tieup_Company` | Not applicable | — | Object absent on UAT | Nothing to classify on UAT | — |

### 2.2 Transaction objects

| Object | Role | Target class | Observed `CompanyID` | Gap vs target | Exception |
|--------|------|--------------|----------------------|---------------|-----------|
| `tbl_Quotation` | Root | TRANSACTION_OWNED | NOT NULL; 158 / 1 | None for root column | — |
| `tbl_Quotaion_details` | Child of quotation | TRANSACTION_OWNED | Present *(8 Sep catalog)* | Parent consistency unverified | — (R-controls) |
| `tbl_Invoice` | Root | TRANSACTION_OWNED | NOT NULL; 13, all company 1 | Joins to client by `Client_ID` alone are ambiguous for 4 invoices | X-10 |
| `tbl_Invoice_details` | Child of invoice | TRANSACTION_OWNED | NOT NULL; 38, all company 1 | 3 lines have no header | X-09 |
| `tbl_invoice_payment` | Child of invoice (A1-1) | TRANSACTION_OWNED | None; 0 rows | No deterministic invoice reference; rows created by quotation number | X-28 |
| `tbl_invoice_due` | Child of invoice (A1-1) | TRANSACTION_OWNED | None; 0 rows (heap) | Same; written by quotation number only | X-28 |
| `tbl_invoice_payment_tds` | Child of invoice (A1-1) | TRANSACTION_OWNED | None; 0 rows (heap) | Invoice number only; not tenant-unique | X-28 |
| `tbl_Chalan` | Root | TRANSACTION_OWNED | Present *(8 Sep catalog)* | None for root column | — |
| `tbl_Proforma` / `tbl_Proforma_Details` | Root / child | TRANSACTION_OWNED | Present on both *(8 Sep catalog)* | Parent consistency unverified | — (R-controls) |
| `tbl_RequisitionMain` | Root | TRANSACTION_OWNED | NOT NULL; 12, all company 1 | None for root column | — |
| `tbl_RequisitionNew` | Child of requisition | TRANSACTION_OWNED | Present *(8 Sep catalog)* | Parent consistency unverified | — (R-controls) |
| `tbl_PO_Header` | Root | TRANSACTION_OWNED | NOT NULL; 9, all company 1 | None for root column | — |
| `tbl_PO_Items` | Child of PO | TRANSACTION_OWNED | Present *(8 Sep catalog)* | Parent consistency unverified | — (R-controls) |
| `tbl_PO_PartySnapshot` / `tbl_PO_PaymentDetails` | Child of PO (declared FK) | TRANSACTION_OWNED | None *(8 Sep catalog)* | Follows parent via FK | — |
| `tbl_Purches` | Root | TRANSACTION_OWNED | **None**; 163 rows | Root lacks direct `CompanyID` (D7) | X-06 |
| `tbl_purches_details` | Child of purchase | TRANSACTION_OWNED | None; 340 rows | 19 lines reference products absent from master | X-08 |
| `tbl_Purchess_payment` | Child of purchase (1 row matches 1 header) | TRANSACTION_OWNED | None; 1 row | Follows parent once root is owned | X-06 |
| `tbl_purches_due` | Child of purchase | TRANSACTION_OWNED | None; 165 rows (heap) | 2 rows have no header | X-07 |
| `tbl_SalesVisitReport` | Root | TRANSACTION_OWNED | NOT NULL; 78, all company 1 | None for root column | — |
| `tbl_SalesVisitResponses` / `tbl_Expenses` | Child of visit | TRANSACTION_OWNED | None *(8 Sep catalog)* | Follows visit | — |
| `tbl_HydrentInvoice` | Root | TRANSACTION_OWNED | **None**; 0 rows | Root lacks direct `CompanyID` (D7) | X-13 |
| `tbl_qsHydrentQuotation` / `tbl_qsHydrentDetails` | Root / child | TRANSACTION_OWNED | **None**; 0 rows | Root lacks direct `CompanyID` (D7) | X-13 |
| `tbl_StockAdjustments` | Root; physical movement on (`ProductID`, `StoreID`) (A1-2) | TRANSACTION_OWNED | None; 126 rows, no store column | Root lacks direct `CompanyID` (D7) and `StoreID` (D5) | X-14, X-15 |

Objects not classified by the audit (for example GL `tlb_General_expences`, `tbl_patty_cash_expenses`, `tbl_Expences`) are outside this freeze. Gate 2 lists them ([report](GATE2_DEPENDENCY_IMPACT_REPORT.md) §16.6, X-31); classifying them needs a separate decision recorded by amendment.

## 3. Target invariants

Each invariant is written so that a read-only query can return its violating rows. Zero violating rows means the invariant holds. Gate 6 (UAT reconciliation) runs every invariant.

| ID | Invariant | Pass condition |
|----|-----------|----------------|
| INV-P1 | Product identity is shared. One `tbl_NewProduct` row per `ProductID` enterprise-wide. | `ProductID` is NOT NULL and enforced unique. No duplicate `ProductID` rows. |
| INV-P2 | Shared Product identity carries no tenant partition. | `tbl_NewProduct` has no `CompanyID` column, or no read / write path filters or keys Product identity by it (Gate 3 decides which form; Gate 6 tests it). |
| INV-C1 | Category identity is shared. | One `tbl_NewparentProduct` row per category key. Every `tbl_NewProduct.parentId` resolves to exactly one category. |
| INV-C2 | Shared Category identity carries no tenant partition. | Same test as INV-P2 against `tbl_NewparentProduct`. |
| INV-S1 | Store identity is shared. | Every store key is unique in `Stores`. `Stores` has no `CompanyID`. |
| INV-K1 | Stock grain is `ProductID + StoreID`. | At most one `tbl_stock` row per (`Product_id`, `ShippedToStoreId`), enforced by a unique constraint. |
| INV-K2 | Every physical stock quantity belongs to a store (D5). | Zero `tbl_stock` rows with NULL or blank store id. Every store id resolves to `Stores`. |
| INV-K3 | Every stock product resolves to the shared Product master. | Zero `tbl_stock` rows whose `Product_id` is absent from `tbl_NewProduct.ProductID`. |
| INV-K4 | Stock carries no tenant partition. | `tbl_stock` has no `CompanyID`. |
| INV-K5 | `tbl_stock` is the only physical-stock authority (S1, S2). | No application path, procedure, view, or trigger reads `tbl_NewProduct.Quantity` / `Quantity_Num` as on-hand stock or writes stock movements to them. Verified by the Gate 2 dependency inventory and Gate 8 regression. |
| INV-K6 | `tbl_stock.Quantity_Num` is the quantity of record (A1-3). | Zero NULL `Quantity_Num` on stock rows. No path writes a movement to `Quantity` without `Quantity_Num`. Where `Quantity` is retained, zero rows where it differs from `Quantity_Num`. |
| INV-K7 | Every stock adjustment is a tenant-owned movement at stock grain (A1-2). | Every `tbl_StockAdjustments` row has `CompanyID int NOT NULL` with FK to `tbl_Company.ID`, a non-null `StoreID` that resolves to `Stores`, and a `ProductID` that resolves to `tbl_NewProduct`. |
| INV-T1 | Every tenant-owned transaction root carries a direct `CompanyID` (D7). | Every root in §2.2 has a `CompanyID int NOT NULL` with FK to `tbl_Company.ID`. Zero NULL values. |
| INV-T2 | Root ownership is never derived. | No migration step or application path sets a root `CompanyID` from Vendor, Client, Product, Store, or Stock. |
| INV-T3 | Child ownership follows the root (§4). | Every child row resolves to exactly one root. Where the child stores its own `CompanyID`, it equals the root's `CompanyID`. |
| INV-M1 | Every TENANT_MASTER row carries a direct `CompanyID` (D1). | `CompanyID int NOT NULL` on each D1 table. Zero NULL values. |
| INV-M2 | TENANT_MASTER children agree with their parent tenant. | Factory, Representative, and Client Registration Address rows match a `tbl_Client` row on (`Client_Id`, `CompanyID`). |
| INV-G1 | GLOBAL_REFERENCE carries no tenant partition (D2, D3). | `tbl_State`, `tbl_City`, `tbl_Vat_Master` have no `CompanyID`, or no path filters by it (Gate 3 decides which form). One row per reference value. |
| INV-Q1 | Product master quantity is not inventory authority (S2). | Same test as INV-K5. Master quantity columns are either retired or documented as non-stock attributes in Gate 3. |

## 4. `CompanyID` propagation rules

1. **Roots.** A transaction root receives `CompanyID` from the authenticated working tenant (`CompanyContext.CurrentCompanyID`) at creation. It is never taken from the client request, and never computed from a referenced master.
2. **Children.** A child row's owner is the owner of the root it belongs to, reached through its declared or documented parent key (for example `Invoice_No`, `Purches_Id`, `PO_Id`, `VisitId`). Ownership is resolved through that parent link only. The link must resolve to exactly one root; `Invoice_No` is a per-company serial, so an invoice link needs the invoice's `CompanyID` or `tbl_Invoice.ID` ([Gate 2 report](GATE2_DEPENDENCY_IMPACT_REPORT.md) §8).
3. **No inference from masters.** Vendor, Client, Product, Store, and Stock are never ownership sources for a transaction or its children. A join to them may be reported as evidence. It may not set or change `CompanyID`.
4. **Child columns.** Where a child already stores `CompanyID` (for example `tbl_Invoice_details`, `tbl_PO_Items`), it must equal its root. Whether to add `CompanyID` to children that lack it is a Gate 3 design choice; adding it never changes the ownership source.
5. **Orphans.** A child that resolves to no root has no derivable owner. It is a registered data exception. It is not assigned a tenant by default.
6. **Shared rows inside tenant transactions.** A tenant transaction line may reference shared Product, Category, or Store identity. That reference does not partition the shared row.
7. **Stock movements.** A tenant transaction that moves stock changes the shared `tbl_stock` row for (`ProductID`, `StoreID`). The transaction carries the tenant; the stock row does not.

## 5. Consequences

- Every exception in the governance register must close before Gate 5 (UAT migration) runs.
- Application adaptation (Gate 7) follows the UAT schema and data migration. It is not started first.
- Production migration is a replay of the proven UAT migration (governance §8).
- Ownership, stock-tenancy, and legacy-retirement implementation stays blocked until Gate 2 completes and Gate 3 is approved.
