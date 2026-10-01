# Multi-Tenancy Architecture — Shared Masters, Shared Physical Inventory, Tenant-Owned Transactions

**Status:** Architecture freeze — frozen target architecture. Implementation is blocked pending dependency-complete validation (see [ARCHITECTURE.md](ARCHITECTURE.md)).
**Scope:** Data-ownership model for AminrupERP / Flame-ex (`Bill_Software`). Documentation only — no schema, C#, ASPX, stored-procedure, view, or data-migration change is authorized by this document.
**Date:** 2026-10-01
**Canonical location:** This file is the canonical decision. [`docs/ARCHITECTURE.md`](../ARCHITECTURE.md) routes here. No parallel ownership model may be introduced.

---

## 1. The governing rule

**Tenant determines transaction ownership, not shared master or physical-inventory identity.**

A business transaction (purchase, sale, invoice, payment, due) belongs to the tenant that recorded it. That ownership is persisted as `CompanyID` on the transaction row, set from `CompanyContext.CurrentCompanyID` at write time. It is **not** derived from the tenant of a shared master record (Product, Store) the transaction references, and **not** from physical-inventory identity.

Consequences, stated explicitly:

- **The same `ProductID` may be transacted by both tenants.** A product is catalog identity, not ownership.
- **The same `StoreId` may be used by both tenants.** A store is a physical location, not ownership.
- Tenant-owned transactions must persist `CompanyID` directly on the transaction row. Inferring tenant indirectly (for example via `tbl_Vendor.CompanyID` join, or via the stock/master the transaction touches) is not acceptable in the target model.

## 2. Tenants

| Tenant ID | Company | `tbl_Company.ComID` | `ShortCode` |
|----------:|---------|---------------------|-------------|
| 1 | Flame-Ex | `COMP01` | `FE` |
| 2 | AA Associates | `COMP02` | `AA` |

Flame-Ex = **Tenant 1**. AA Associates = **Tenant 2**. `tbl_Company.ID` is the single authoritative tenant key (`CompanyID` on transaction, membership, and session rows). `ComID` / `ShortCode` are display-only. UAT snapshot (2026-09-08): 2 companies; nearly all ERP rows on `CompanyID=1`; tenant 2 has essentially no transaction rows yet.

## 3. Ownership boundary

### 3.1 Shared (no `CompanyID` predicate; one row per real-world entity)

| Object | Live object | Shared because… |
|--------|-------------|-----------------|
| Product Category | `tbl_ProductCatagory` / `tbl_NewparentProduct` | Catalog structure is not per-business |
| Product | `tbl_NewProduct` (`ProductID` business key, unique) | One catalog entry per SKU, referenced by both tenants |
| Store | `Stores` (PK `Id`) | Physical-location list, not per-business |
| Stock / physical inventory | `tbl_stock` (`ProductID + StoreId → Quantity`) | Physical reality: what exists, and where |

The company switcher **must not partition or duplicate** these objects. A product or store created while switched to Flame-Ex is visible to AA Associates — the same object, not a tenant-2 copy. Duplicating masters or stock per tenant would double-count physical inventory and fork the catalog.

### 3.2 Tenant-owned (persist `CompanyID` directly)

| Object | Live object(s) | Notes |
|--------|----------------|-------|
| Purchase | `tbl_Purches`, `tbl_purches_details`, `tbl_Purchess_payment` | Purchase, purchase detail, purchase payment carry `CompanyID` |
| Sales | `tbl_Quotation` (incl. `RecordType = 'Purchase Order'` client stack), `tbl_Quotaion_details` | Already `CompanyID`-scoped |
| Invoice | `tbl_Invoice`, `tbl_Invoice_details` | Already `CompanyID`-scoped |
| Payment received | `tbl_invoice_payment` | `CompanyID` persisted on the payment row |
| Due | `tbl_invoice_due`, `tbl_purches_due` | `CompanyID` persisted on the due row |
| PR / PO (modern stack) | `tbl_RequisitionMain`, `tbl_RequisitionNew`, `tbl_PO_Header` + item/party/payment children | Already `CompanyID`-scoped |
| Other transactions | `tbl_Chalan`, `tbl_Proforma`, hydrant transaction tables | `CompanyID`-scoped in the target model |

"Equivalent business transactions" added later inherit the same rule: they are tenant-owned and persist `CompanyID` directly.

### 3.3 Not transactions, not shared masters

- `tbl_login` carries `CompanyID` as the **home tenant** of a user (identity attribute, extended by `UserCompanyAccess` memberships) — see [`22_Security_Baseline.md`](../22_Security_Baseline.md).
- `ActiveSessions.CompanyID` is the **transaction context** captured at sign-in — same semantics as the switcher (§4).

## 4. Tenant switcher = transaction/business context

`CompanyContext.CurrentCompanyID` (from `Session["CompanyID"]`, defined on `Bill.Master.cs`) is the **active transaction/business context**, not a data-partition boundary for shared domains:

- Switching company changes **which tenant owns the next transaction** and which tenant-owned rows are listed/edited.
- Switching company does **not** filter, partition, duplicate, or re-scope shared Product Category, Product, Store, or Stock rows.
- Shared-domain CRUD operates on the shared row regardless of the active context; per-tenant field needs, if any arise, are a schema decision that must come through a new architecture decision, not per-tenant row duplication.

## 5. Shared stock identity

**Shared stock is `ProductID + StoreId → Quantity`.** No `CompanyID` is introduced into `tbl_stock`.

- `ProductID` = `tbl_NewProduct.ProductID` (the unique business key). Live column on `tbl_stock` is `Product_id`.
- `StoreId` = store dimension; live column on `tbl_stock` is `ShippedToStoreId` (varchar), backed by `Stores.Id`.
- `Quantity` = physical balance; live numeric column is `Quantity_Num` (`decimal(18,4)`, default 0). The legacy `Quantity` varchar column exists on UAT and is not authoritative.
- Because stock is shared, both tenants transact against the **same** physical balance for the same `ProductID + StoreId`. If the business later requires per-tenant stock attribution, that is a stock-transaction-ledger design requiring its own architecture decision — not a `CompanyID` column on `tbl_stock`.

## 6. Product master quantity is not physical inventory

**`tbl_NewProduct.Quantity` / `tbl_NewProduct.Quantity_Num` must not be treated as the authoritative physical-inventory balance** under this architecture. Stock (`tbl_stock`, aggregated `ProductID + StoreId`) is the authoritative physical-inventory domain.

- Master quantity fields are metadata/shadow values on the product row. Treating them as the balance makes the product master a second, competing inventory domain and breaks the `ProductID + StoreId` identity.
- The existing reconciliation view `dbo.vw_StockReconciliation` (master `Quantity_Num` vs `tbl_stock` aggregates, plus `Qty_Difference`) is a **diagnostic** surface. Under the frozen model the direction of authority is: `tbl_stock` is the balance; master quantity is at best a cached convenience and must not drive stock decisions.
- `sp_GetProductStockByStore`, `sp_SearchProductsFast`, `sp_SearchStockGrouped`, and stock report pages must source balances from the stock domain.

## 7. Architecture freeze statement

**This ownership model is frozen.** Subsequent implementation work — schema changes, C#, ASPX, stored procedures, views, migrations — must conform to this model unless an explicit, separately recorded architecture decision supersedes it. Concretely:

1. No `CompanyID` column is added to `tbl_stock`, `tbl_NewProduct`, `tbl_ProductCatagory`/`tbl_NewparentProduct`, `tbl_parentProduct`/`tbl_Product`, or `Stores` as part of this model.
2. No per-tenant duplication or partitioning of shared Category/Product/Store/Stock rows is introduced (including via the tenant switcher).
3. Tenant-owned transaction tables persist `CompanyID` directly; new transaction paths must not rely on vendor/company inference.
4. Authoritative physical balance is the stock domain; product-master quantity is not used as the balance.
5. Any exception requires an explicit architecture decision that references and supersedes this document.

## 8. Current state → target state → required review

Current-state facts below are UAT findings ([`SHARED_SCHEMA.md`](../page-catalog/SHARED_SCHEMA.md), snapshot 2026-09-08); they are **not** claims that every production dependency has been audited.

| Area | Current state (UAT evidence) | Target state (frozen) | Required review |
|------|------------------------------|------------------------|-----------------|
| Product / Category / Store | `tbl_NewProduct.CompanyID`, `tbl_NewparentProduct.CompanyID`, `requisition_po_companyid.sql`, `tbl_NewProduct_CompanyID.sql`, `tbl_NewparentProduct_CompanyID.sql` patches exist (not applied on UAT) | Shared: no tenant column, no partitioning | Decide and document deprecation of the in-repo CompanyID DDL patches for these tables |
| Stock | `tbl_stock` has **no** `CompanyID`; store dimension `ShippedToStoreId`; balance `Quantity_Num`; `usp_Reconcile_MasterToStock` / `usp_CreateMissingMastersFromStock` / `usp_UpdateZeroMastersFromStock` on UAT | `ProductID + StoreId → Quantity`, no `CompanyID` | Dependency-complete audit of **every** stock mutation path (purchase in, sale out, adjustments, reconciliation SPs, `stock_pid_mapping`, archive/fix tables) |
| Purchase family | `tbl_Purches` family has **no** `CompanyID`; tenant inferred via `Client_Id = Vendor_Id → tbl_Vendor.CompanyID`; several leftover search pages have no tenant predicate | Direct `CompanyID` on purchase/detail/payment rows | Schema design for direct column + migration; every purchase read/write path audited before change |
| Tenant switcher | `CompanyContext.CurrentCompanyID` from `Session["CompanyID"]`; `UserCompanyAccess` **not applied on UAT** (fail-closed) | Switcher = transaction context; shared domains unpartitioned | Verify no current page uses the switcher to filter stock/master lists |
| Product master quantity | `tbl_NewProduct.Quantity` / `Quantity_Num` maintained as if a balance; `vw_StockReconciliation` compares it to stock | Stock is authoritative; master quantity demoted | Inventory every page/SP that reads or writes master quantity |
| Tenant 2 data | UAT: all ERP rows `CompanyID=1`; tenant 2 exists in `tbl_Company` only | Both tenants transact against shared masters/stock | UAT seeding/validation of tenant-2 transactions after dependency validation |

## 9. Implementation blocker

Implementation remains **blocked** pending dependency-complete validation of:

- Stock mutations — every write/read path through `tbl_stock` and its satellite tables.
- Purchase flows — `tbl_Purches` family, PR → PO paths, purchase payments.
- Invoice/sales flows — quotation, DPCC, proforma, invoice, invoice payments, dues.
- Reconciliation views/SPs — `vw_StockReconciliation`, `vw_CategoryStoreStock_Safe`, `vw_PO_Items_Effective`, the `usp_*` reconciliation family.
- Master-data CRUD — product, category, store, and vendor/customer master create/update/delete paths.

Work on these validations must be tracked in [`HANDOFF.md`](../HANDOFF.md); this document records only the decision and the boundary.
