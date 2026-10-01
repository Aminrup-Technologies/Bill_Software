# Architecture Baseline

**Status:** Frozen baseline for the data-ownership model. The decision itself is [`MULTI_TENANCY.md`](MULTI_TENANCY.md) — that file governs; this file frames it.
**Date:** 2026-10-01
**Audience:** Engineers implementing schema, C#, ASPX, stored-procedure, view, or migration changes. No such change is authorized by this documentation task.

---

## 1. What this baseline is

This folder holds cross-cutting architecture decisions for AminrupERP / Flame-ex (`Bill_Software`). This document establishes the **canonical repository architecture baseline** for:

- **Shared masters** — Product Category, Product, Store
- **Shared physical inventory** — Stock (`tbl_stock`)
- **Tenant-owned transactions** — Purchase, Purchase Detail, Sales, Invoice, Payment, Due, and equivalent business transactions

The binding rules, ownership tables, stock identity, and switcher semantics are defined once in [`MULTI_TENANCY.md`](MULTI_TENANCY.md) and are not restated here. Where any older document appears to conflict (for example module docs `01`–`13`, or dated audit snapshots), this baseline and [`MULTI_TENANCY.md`](MULTI_TENANCY.md) govern.

## 2. Documents in this folder

| Document | Role |
|----------|------|
| [`MULTI_TENANCY.md`](MULTI_TENANCY.md) | **Canonical decision:** shared vs tenant-owned boundary, tenant switcher semantics, shared stock identity (`ProductID + StoreId → Quantity`), product-master-quantity demotion, architecture freeze statement |
| [`ARCHITECTURE.md`](ARCHITECTURE.md) | This file — baseline framing, terminology, gap summary, review ledger |

Related canonical documents outside this folder:

| Document | Relationship |
|----------|--------------|
| [`docs/22_Security_Baseline.md`](../22_Security_Baseline.md) | AuthN/AuthZ contract. Unchanged by this baseline; `CompanyID` handling for tenant-owned reads remains as specified there |
| [`docs/page-catalog/SHARED_CONTEXT.md`](../page-catalog/SHARED_CONTEXT.md) | Runtime facts (CompanyContext, SecurePage, print gate) |
| [`docs/page-catalog/SHARED_SCHEMA.md`](../page-catalog/SHARED_SCHEMA.md) | Live UAT schema facts this baseline's current-state rows cite |
| [`docs/SOLUTION_INDEX.md`](../SOLUTION_INDEX.md) | Router — routes engineers here |
| [`ADR-001_Administrator_Impersonation.md`](../ADR-001_Administrator_Impersonation.md) | Precedent for the decision-record format used here |

## 3. Terminology (preserve these meanings)

Terminology is aligned with the established docs. Do not invent synonyms.

| Term | Meaning |
|------|---------|
| Tenant | A `tbl_Company` row. **Flame-Ex = Tenant 1** (`ID=1`, `ComID=COMP01`, `ShortCode=FE`). **AA Associates = Tenant 2** (`ID=2`, `COMP02`, `AA`) |
| `CompanyID` | `tbl_Company.ID` — the authoritative tenant key on tenant-owned rows |
| Shared master | Product Category, Product, Store — catalog identity, no tenant partition |
| Physical inventory / stock | `tbl_stock` — the authoritative balance domain, keyed `ProductID + StoreId` |
| Tenant-owned transaction | Purchase, Sales, Invoice, Payment, Due, PR/PO, and equivalents — persists `CompanyID` directly |
| Transaction/business context | `CompanyContext.CurrentCompanyID` (the tenant switcher) — selects which tenant owns the next transaction; does not partition shared domains |
| Product master quantity | `tbl_NewProduct.Quantity` / `Quantity_Num` — metadata on the product row; **not** the physical balance |
| Master-data CRUD | Create/update/delete paths on shared masters and vendor/customer masters |
| Dependency-complete validation | Audit of every write/read path in a flow before any schema/behavior change; the gate for lifting the implementation block |

## 4. Baseline summary (non-normative restatement)

For orientation only — the normative text is [`MULTI_TENANCY.md`](MULTI_TENANCY.md) §1–§7:

1. **Governing rule:** tenant determines transaction ownership, not shared master or physical-inventory identity. The same `ProductID` may be transacted by both tenants; the same `StoreId` may be used by both tenants.
2. **Shared:** Category, Product, Store, Stock. No `CompanyID` on `tbl_stock`. No per-tenant duplication via the switcher.
3. **Tenant-owned:** transactions persist `CompanyID` directly — never via vendor/company inference.
4. **Stock identity:** `ProductID + StoreId → Quantity`.
5. **Product master quantity** (`tbl_NewProduct.Quantity` / `Quantity_Num`) is **not** the authoritative physical-inventory balance; stock is.
6. **Architecture freeze:** implementation must conform to this model unless a new explicit architecture decision supersedes it.

## 5. Current state → target state → required review (concise)

Full table with evidence: [`MULTI_TENANCY.md`](MULTI_TENANCY.md) §8. Known gaps, stated without claiming a full audit of unverified dependencies:

| # | Gap | Class | Required review |
|---|-----|-------|-----------------|
| G1 | `tbl_NewProduct.CompanyID` / `tbl_NewparentProduct.CompanyID` (in-repo DDL patches, unapplied on UAT) conflict with the shared-master model | Conflicts with freeze §7.1 | Decide/document patch deprecation before any master-data work |
| G2 | `tbl_Purches` family lacks `CompanyID`; tenant inferred via `Client_Id = Vendor_Id → tbl_Vendor.CompanyID`; some leftover pages have no tenant predicate | Current state violates freeze §7.3 | Schema + migration design gated on full purchase-flow audit |
| G3 | Stock mutation paths (purchase in, sale out, adjustments, `usp_*` reconciliation, `stock_pid_mapping`) not yet dependency-audited | Pending validation | Audit before any stock-domain change |
| G4 | Invoice/sales flows and reconciliation views (`vw_StockReconciliation`, `vw_CategoryStoreStock_Safe`) not yet dependency-audited against the frozen model | Pending validation | Audit before demoting master quantity |
| G5 | Master-data CRUD paths not yet audited for switcher-partitioning behavior | Pending validation | Verify no page filters shared lists by active context |
| G6 | `UserCompanyAccess` not applied on UAT (company gating fails closed) | Pre-existing (docs/22 scope) | Unchanged by this baseline; not an ownership-model item |

Items G3–G5 are **pending validation**, not findings. This baseline does not claim they have been audited.

## 6. Review ledger

Validation status of the implementation blocker ([`MULTI_TENANCY.md`](MULTI_TENANCY.md) §9). Update rows as audits complete; do not mark complete without evidence.

| Flow | Status | Evidence / owner |
|------|--------|------------------|
| Stock mutations | Blocked — pending audit | — |
| Purchase flows | Blocked — pending audit | — |
| Invoice/sales flows | Blocked — pending audit | — |
| Reconciliation views/SPs | Blocked — pending audit | — |
| Master-data CRUD | Blocked — pending audit | — |

Implementation remains blocked until each row can cite a completed dependency-complete validation. Track progress in [`docs/HANDOFF.md`](../HANDOFF.md).
