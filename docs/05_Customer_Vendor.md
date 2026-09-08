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
| `tbl_Purches` | Vendor purchase header. Column **`Client_Id` stores `tbl_Vendor.Vendor_Id`** (varchar business code such as `AA132`, **not** integer `tbl_Vendor.Id`). Name is wrong. No `CompanyID` on the purchase table — lists/print join the vendor for tenant. Live UAT: [`SHARED_SCHEMA.md`](page-catalog/SHARED_SCHEMA.md). |
| `tbl_SalesVisitReport.CustomerName` | Free text on visits. **No FK** to `tbl_Client`. |

`Vendor_quotation.aspx` sits in the quotation catalog but is a vendor-facing quote view — leftover cross-link only.

---

## FK usage (unique)

- Quotations / invoices / DPCC / proforma: `Client_ID` / `Client_Id` → `tbl_Client.Client_Id` (varchar; **not** a declared FK).
- Modern vendor PR: `tbl_RequisitionMain.VendorId` → `tbl_Vendor.Id` (declared FK).
- Vendor purchase / purchase payment: `Client_Id` → `tbl_Vendor.Vendor_Id` (code, not PK).
- Visit planner/reports: no client/vendor FK.

Do not invent a unified party table. There isn’t one.
