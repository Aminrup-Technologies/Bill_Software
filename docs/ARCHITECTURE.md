# Architecture

Entry point for the canonical architecture documentation. The binding decision lives one level down — read it before any schema, C#, ASPX, stored-procedure, view, or migration work.

| Document | Role |
|----------|------|
| [`architecture/MULTI_TENANCY.md`](architecture/MULTI_TENANCY.md) | **Canonical decision (frozen):** shared masters + shared physical inventory vs tenant-owned transactions; tenant switcher as transaction context; shared stock `ProductID + StoreId → Quantity` (no `CompanyID` in `tbl_stock`); product-master quantity is not the physical balance; architecture freeze statement |
| [`architecture/ARCHITECTURE.md`](architecture/ARCHITECTURE.md) | Baseline framing: terminology, non-normative summary, current-state gaps, review ledger |
| [`architecture/OWNERSHIP_AUDIT.md`](architecture/OWNERSHIP_AUDIT.md) | **Evidence-based ownership audit (2026-10-01):** complete master/reference inventory with classifications, transaction→master dependency matrix, stock & product-quantity dependency traces, switcher/inference audits, conflicts vs frozen model, pending validations, and the decisions that must be resolved before implementation. **Pending review — the implementation blocker remains until this audit is reviewed** |

Related canonical contracts:

- [`22_Security_Baseline.md`](22_Security_Baseline.md) — AuthN/AuthZ engineering contract (unchanged by the ownership model)
- [`page-catalog/SHARED_CONTEXT.md`](page-catalog/SHARED_CONTEXT.md) — runtime facts: `CompanyContext`, `SecurePage`, print gate
- [`page-catalog/SHARED_SCHEMA.md`](page-catalog/SHARED_SCHEMA.md) — live UAT schema facts

**Architecture freeze:** implementation work must conform to [`architecture/MULTI_TENANCY.md`](architecture/MULTI_TENANCY.md) unless an explicit, separately recorded architecture decision supersedes it. Implementation is currently blocked pending dependency-complete validation — the inventory-level dependency audit is complete ([`architecture/OWNERSHIP_AUDIT.md`](architecture/OWNERSHIP_AUDIT.md)), but its classification decisions and §13 decision list **remain unreviewed**; see the review ledger in [`architecture/ARCHITECTURE.md`](architecture/ARCHITECTURE.md) and current status in [`HANDOFF.md`](HANDOFF.md).
