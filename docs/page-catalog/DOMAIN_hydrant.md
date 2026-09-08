# Hydrant invoices & quotations

> Domain key: `hydrant` · 5 page(s)

Hydrant-product commercial documents (parallel to tax invoice).

**Do not copy** AuthN/AuthZ, tenancy, or Ponytail rules here. Those live in [SHARED_CONTEXT](SHARED_CONTEXT.md).

| Page | Purpose | Master / class | Gate | Tables (unique) | Ops | Narrative |
|------|---------|----------------|------|-----------------|-----|-----------|
| `corporate/business/app/Deletehydrent_invoice.aspx` | Delete New Invoice | Bill.Master / `Deletehydrent_invoice` | Bill.Master<br>menu `Deletehydrent_invoice` | `tbl_Client`, `tbl_HydrentInvoice`, `tbl_qsHydrentQuotation` | UPDATE, DELETE | this catalog |
| `corporate/business/app/HydrantProduct.aspx` | Hydrant Product | Bill.Master / `HydrantProduct` | Bill.Master<br>— | `tbl_Vat_Master`, `tbl_HydrantProduct` | INSERT, DELETE | this catalog |
| `corporate/business/app/Searchhydrent_invoice.aspx` | Search New Invoice | Bill.Master / `Searchhydrent_invoice` | Bill.Master<br>menu `Searchhydrent_invoice` | `tbl_Client`, `tbl_HydrentInvoice` | SELECT | this catalog |
| `corporate/business/app/Viewhydrent_invoice.aspx` | View New Invoice | Bill.Master / `Viewhydrent_invoice` | Bill.Master<br>menu `Viewhydrent_invoice` | `tbl_HydrentInvoice`, `tbl_Client` | SELECT | this catalog |
| `corporate/business/app/hydrent_invoice.aspx` | Create New Invoice | Bill.Master / `hydrent_invoice` | Bill.Master, CompanyID<br>menu `hydrent_invoice` | `tbl_Client`, `tbl_Quotation`, `tbl_qsHydrentQuotation`, `tbl_Factory`, `tbl_qsHydrentDetails`, `tbl_HydrentInvoice` | INSERT, UPDATE | this catalog |

<!-- NARRATIVE:BEGIN -->

## Parallel commercial track

Not `tbl_Quotation` / `tbl_Invoice`. Menu group `Hydrent` is `visible="false"` but pages remain.

`HydrantProduct` CRUD → `tbl_HydrantProduct` (tax `tbl_Vat_Master`).

Quotes live in [DOMAIN_quotation.md](DOMAIN_quotation.md) (`tbl_qsHydrentQuotation`, print `QuotationHydrent.aspx?Quotation_no=`).

Invoice: `hydrent_invoice` takes quotes with `invStatus='No'` → INSERT `tbl_HydrentInvoice` (discount, `addressfor` from `tbl_Factory`) → `invStatus='Yes'`. Numbering `INV/{clientInitials}/{fy}/{n}` (not tax `INV/C/...`).

View prints `hydrentInvoice.aspx?ID=` (buyer, label typo Orginal) and `hydrentInvoiceSeller.aspx?ID=`. Search still has dead `Invoice.aspx` links plus live hydrant prints. Delete restores `invStatus='No'`. Delete-quote also deletes linked hydrant invoices.

Spelling: Hydrent / Quatation throughout filenames.
