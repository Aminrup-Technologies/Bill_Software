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
