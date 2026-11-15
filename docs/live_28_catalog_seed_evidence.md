# LIVE 28-key catalog seed — evidence

**Date:** 2026-11-15
**Seed:** [`Bill_Software/corporate/business/sql/live_nav_28_catalog_seed.sql`](../Bill_Software/corporate/business/sql/live_nav_28_catalog_seed.sql)
**Master:** [`Bill_Software/corporate/business/app/Bill.Master`](../Bill_Software/corporate/business/app/Bill.Master)
**Column contract:** [`docs/page-catalog/UAT_CATALOG.md`](page-catalog/UAT_CATALOG.md) (`dbo.Permissions` nvarchar 200/500/100/100/100)

All Description / ModuleName / SubModuleName / FeatureName values are **newly derived** from repository evidence (Bill.Master + page purpose). They are **not** recovered historical `dbo.Permissions` tuples.

## Inventory (historical LIVE census — not a fresh verify)

| Item | Count |
|------|------:|
| Expected Super Admin nav `#TargetKeys` | 114 |
| PresentKeys (historical census) | 83 |
| MissingKeys | 31 |
| This seed | **28** |
| PR #115 (`AdminOverride`, `AdminShiftAssignment`) | 2 |
| PR #114 / grant-owned `AdminAttendanceDashboard` | 1 |

Check: 31 = 28 + 2 + 1. Check: 83 + 28 + 2 + 1 = 114. No overlap between this seed and the EmpConfig/AAD three.

## Evidence matrix (exactly 28)

| PermissionKey | Source page | PermissionKey source | Description rationale | ModuleName evidence | SubModuleName evidence | FeatureName evidence | Observed vs newly derived |
|---|---|---|---|---|---|---|---|
| `Add_DirectInvoice` | `Manual_Invoice.aspx` (session `Page`, `Manual_Invoice.aspx.cs` L13) | Master L434 `id="Add_DirectInvoice"` | Creates tax invoice with no source document | Master L431 parent `Invoice` display **Tax Invoice** | Inferred `General` (UAT default; no mid-group) | Master L434 link **Create Direct Tax Invoice** | Newly derived |
| `CreateHydrentQuatation` | `CreateHydrentQuatation.aspx` (session `Page`) | Master L500 | Creates hydrant quotation | Master L498 `id="Hydrent"` (not display Quotation) | Inferred `General` | Master L500 **Create New Quotation** | Newly derived |
| `daily_reporting` | `daily_rpt.aspx` | Master L475 + SecurePage `daily_rpt.aspx.cs` L25 | Creates/submits sales-visit daily report | Master L472 parent **Sales Team** | Inferred `General` | Master L475 **Daily Reporting** | Newly derived |
| `Deletehydrent_invoice` | `Deletehydrent_invoice.aspx` (session `Page`) | Master L507 | Deletes hydrant invoice | Master L498 `id="Hydrent"` | Inferred `General` | Master L507 **Delete New Invoice** | Newly derived |
| `DeleteHydrentQuatation` | `DeleteHydrentQuatation.aspx` (session `Page`) | Master L503 | Deletes hydrant quotation | Master L498 `id="Hydrent"` | Inferred `General` | Master L503 **Delete New Quotation** | Newly derived |
| `hydrent_invoice` | `hydrent_invoice.aspx` (session `Page`) | Master L504 | Creates hydrant invoice from uninvoiced quotes | Master L498 `id="Hydrent"` | Inferred `General` | Master L504 **Create New Invoice** | Newly derived |
| `Li2` | `Create_quotation.aspx` (session `Page`, L11) | Master L406 `id="Li2"` (distinct from `Create_quotation`) | Client PO create via shared ASPX | Master L404 parent **Client PO** | Inferred `General` | Master L406 **Create Purchase Order** | Newly derived |
| `Li3` | `Search_purchaseorder.aspx` (session `Page`) | Master L408 | Searches client purchase orders | Master L404 **Client PO** | Inferred `General` | Master L408 **Search Purchase Order** | Newly derived |
| `Li4` | `delete_purchaseorder.aspx` (session `Page`) | Master L410 | Deletes client purchase orders | Master L404 **Client PO** | Inferred `General` | Master L410 **Delete Purchase Order** | Newly derived |
| `Li5` | `edit_purchaseorder.aspx` (session `Page`) | Master L409 | Edits client purchase orders | Master L404 **Client PO** | Inferred `General` | Master L409 **Edit Purchase Order** | Newly derived |
| `Manual_proforma` | `Direct_Proforma.aspx` (session `Page`, L15) | Master L424 | Proforma without quotation | Master L421 parent **Proforma Invoice** | Inferred `General` | Master L424 **Manual Proforma Invoice** | Newly derived |
| `my_leaves` | `MyLeaves.aspx` | Master L309 + SecurePage `MyLeaves.aspx.cs` L12 | Employee leave application and balance | Master L305 parent **Home** | Inferred `General` | Master L309 **My Leaves** | Newly derived |
| `po_generate` | `Generate_PO_From_PR.aspx` (session `Page`) | Master L371 | Lists approved PRs eligible for vendor PO | Master L360 **PR / PO** | Master L369 mid-group **Purchase Order** | Master L371 **Generate Purchase Order** | Newly derived |
| `po_view` | `View_PO.aspx` (session `Page`, L10) | Master L372 (≠ `View_po`) | Vendor PO review and release | Master L360 **PR / PO** | Master L369 **Purchase Order** | Master L372 **Review & Release PO** | Newly derived |
| `pr_approve` | `Approve_PR.aspx` (session `Page`) | Master L366 | Approve/reject submitted PRs | Master L360 **PR / PO** | Master L362 **Purchase Requisition** | Master L366 **Approve / Reject PR** | Newly derived |
| `pr_create` | `RequisitionNew.aspx` (session `Page`) | Master L364 | Creates purchase requisitions | Master L360 **PR / PO** | Master L362 **Purchase Requisition** | Master L364 **Create Purchase Requisition** | Newly derived |
| `pr_view` | `View_PR.aspx` (session `Page`) | Master L365 | Views purchase requisitions | Master L360 **PR / PO** | Master L362 **Purchase Requisition** | Master L365 **View Purchase Requisition** | Newly derived |
| `rpts_vw_qtn_po_counts` | `rpts_vw_qtn_po_counts.aspx` (session `Page`) | Master L469 | Hosts Superset QTN/PO Added iframe | Master L463 parent **Reports** | Inferred `General` | Master L469 **QTN/ PO Added** | Newly derived |
| `Search_Products` | `search_products.aspx` | Master L354 + SecurePage `search_products.aspx.cs` L17 | Read-only new product catalog browser | Master L347 parent display **Principle** (`id=Vendor`) | Inferred `General` | Master L354 **Search Products** | Newly derived |
| `Searchhydrent_invoice` | `Searchhydrent_invoice.aspx` (session `Page`) | Master L506 | Searches hydrant invoices | Master L498 `id="Hydrent"` | Inferred `General` | Master L506 **Search New Invoice** | Newly derived |
| `SearchHydrentQuatation` | `SearchHydrentQuatation.aspx` (session `Page`) | Master L502 | Searches hydrant quotations | Master L498 `id="Hydrent"` | Inferred `General` | Master L502 **Search New Quotation** | Newly derived |
| `Service_stock` | `Service_stock.aspx` (session `Page`) | Master L466 | Lists service stock from `tbl_stock` | Master L463 **Reports** | Inferred `General` | Master L466 **Service Stock** | Newly derived |
| `srch_dailyrpts` | `srch_dailyrpts.aspx` | Master L478 + SecurePage `srch_dailyrpts.aspx.cs` L14 | Manager search of daily reports | Master L472 **Sales Team** | Inferred `General` | Master L478 **Search Daily Reports** | Newly derived |
| `uploader` | `ImportProducts.aspx` | Master L328 + SecurePage `ImportProducts.aspx.cs` L17 | XML product import | Master L313 parent **Dashboard** | Inferred `General` | Master L328 **Product Import (xml)** | Newly derived |
| `View_po` | `View_PurchaseOrder.aspx` (session `Page`, L21) | Master L407 (≠ `po_view`) | Views client purchase orders | Master L404 **Client PO** | Inferred `General` | Master L407 **View Purchase Order** | Newly derived |
| `Viewhydrent_invoice` | `Viewhydrent_invoice.aspx` (session `Page`) | Master L505 | Views hydrant invoices | Master L498 `id="Hydrent"` | Inferred `General` | Master L505 **View New Invoice** | Newly derived |
| `ViewHydrentQuatation` | `ViewHydrentQuatation.aspx` (session `Page`) | Master L501 | Views hydrant quotations | Master L498 `id="Hydrent"` | Inferred `General` | Master L501 **View New Quotation** | Newly derived |
| `vw_dailyrpts` | `vw_dailyrpts.aspx` | Master L476 + SecurePage `vw_dailyrpts.aspx.cs` L15 | Current user sales-visit reports | Master L472 **Sales Team** | Inferred `General` | Master L476 **View Daily Reporting** | Newly derived |

### Inferred fields (honest)

- **SubModuleName = `General`**: used when Master has no intermediate `<ul>` between top-level module and leaf; matches `UAT_CATALOG.md` default for `SubModuleName`.
- **ModuleName = `Hydrent`**: taken from Master control `id="Hydrent"` (L498), not the display text `Quotation`, to avoid colliding with the top-level Quotation menu.

## Scope notes

- Catalog-only seed. No `RolePermissions` / `UserRoles`. No hardcoded `PermissionId`.
- Excludes `AdminAttendanceDashboard`, `AdminOverride`, `AdminShiftAssignment`.
- All of PR #115 EmpConfig seed, this 28-key seed, and the 114-key grant remain **proposals** until separately DBA-approved and executed.
