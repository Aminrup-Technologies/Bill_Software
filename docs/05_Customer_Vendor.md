# Customer & vendor masters

Canonical page census + mutations: [`page-catalog/DOMAIN_customer.md`](page-catalog/DOMAIN_customer.md) (clients / factories) and [`page-catalog/DOMAIN_vendor-purchase.md`](page-catalog/DOMAIN_vendor-purchase.md) (vendors + vendor purchases).  
Shared AuthN/tenancy: [`page-catalog/SHARED_CONTEXT.md`](page-catalog/SHARED_CONTEXT.md).

This file keeps **leftover facts** that are not in those catalogs.

---

## Tables (confirmed)

| Table | Role |
|-------|------|
| `tbl_Client` | Customer / client master. **Not** `tbl_Customers`. |
| `tbl_Vendor` | Vendor / “Principle” master. **Not** `tbl_Vendors`. |
| `tbl_Purches` | Vendor purchase header. Column **`Client_Id` stores `Vendor_Id`** (name is wrong). |
| `tbl_SalesVisitReport.CustomerName` | Free text on visits. **No FK** to `tbl_Client`. |

`Vendor_quotation.aspx` sits in the quotation catalog but is a vendor-facing quote view — leftover cross-link only.

---

## FK usage (unique)

- Quotations / invoices / DPCC / proforma: `Client_ID` → `tbl_Client`.
- Vendor PO / vendor purchase / vendor payment: vendor id → `tbl_Vendor`.
- Visit planner/reports: no client/vendor FK.

Do not invent a unified party table. There isn’t one.
