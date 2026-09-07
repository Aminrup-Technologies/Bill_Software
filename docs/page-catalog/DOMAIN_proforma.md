# Proforma invoices

> Domain key: `proforma` · 6 page(s)

Create/view/search/delete/mailer + prints.

**Do not copy** AuthN/AuthZ, tenancy, or Ponytail rules here. Those live in [SHARED_CONTEXT](SHARED_CONTEXT.md).

| Page | Purpose | Master / class | Gate | Tables (unique) | Ops | Narrative |
|------|---------|----------------|------|-----------------|-----|-----------|
| `corporate/business/app/Add_proforma.aspx` | Create Proforma Invoice | Bill.Master / `WebForm30` | Bill.Master, CompanyID<br>menu `Add_proforma` | `tbl_Client`, `tbl_Quotation`, `tbl_QuoPriSerTogather`, `tbl_Proforma`, `tbl_SalesVisitReport` | INSERT, UPDATE | this catalog |
| `corporate/business/app/Delete_proforma.aspx` | Delete Proforma Invoice | Bill.Master / `WebForm33` | Bill.Master, CompanyID<br>menu `Delete_proforma` | `tbl_Client`, `tbl_Proforma`, `tbl_QuoPriSerTogather`, `tbl_Quotation` | UPDATE, DELETE | this catalog |
| `corporate/business/app/Direct_Proforma.aspx` | Direct Proforma | Bill.Master / `Direct_Proforma` | Bill.Master, CompanyID<br>menu `Manual_proforma` | `tbl_NewProduct`, `tbl_Client`, `tbl_NewparentProduct`, `tbl_Proforma`, `tbl_Proforma_Details` | INSERT, DELETE | this catalog |
| `corporate/business/app/ProformaMail.aspx` | Proforma Invoice Mailer | Bill.Master / `WebForm84` | Bill.Master, CompanyID<br>menu `ProformaMail` | `tbl_Client`, `tbl_Proforma`, `tbl_QuoPriSerTogather`, `tbl_QutPrimaryService`, `tbl_representative` | UPDATE | [module](../11_Communications.md) |
| `corporate/business/app/Seartch_proforma.aspx` | Search Proforma Invoice | Bill.Master / `WebForm32` | Bill.Master, CompanyID<br>menu `Seartch_proforma` | `tbl_Client`, `tbl_Proforma`, `tbl_QuoPriSerTogather` | SELECT | this catalog |
| `corporate/business/app/View_proforma.aspx` | View Proforma Invoice | Bill.Master / `WebForm31` | Bill.Master<br>menu `View_proforma` | `tbl_Proforma`, `tbl_Client`, `tbl_QuoPriSerTogather`, `tbl_Quotation` | SELECT | this catalog |
