# Tax invoices

> Domain key: `invoice` · 7 page(s)

Create/view/search/delete/block/mailer + prints. Discovery: docs/13.

**Do not copy** AuthN/AuthZ, tenancy, or Ponytail rules here. Those live in [SHARED_CONTEXT](SHARED_CONTEXT.md).

| Page | Purpose | Master / class | Gate | Tables (unique) | Ops | Narrative |
|------|---------|----------------|------|-----------------|-----|-----------|
| `corporate/business/app/Add_invoice.aspx` | Create Tax Invoice (From Source)<br>WM: `GetReconciliation` | Bill.Master / `WebForm26` | Bill.Master, CompanyID, WebMethod<br>menu `Add_invoice` | `tbl_Client`, `tbl_Invoice`, `tbl_login`, `tbl_ClientRegAddress`, `tbl_Quotation`, `tbl_Chalan`, `tbl_Proforma`, `tbl_Invoice_details` | INSERT, UPDATE, DELETE | this catalog |
| `corporate/business/app/Block_invoice.aspx` | Block Invoice | Bill.Master / `WebForm64` | Bill.Master, CompanyID<br>menu `Block_invoice` | `tbl_Client`, `tbl_Invoice` | UPDATE | this catalog |
| `corporate/business/app/Delete_invoice.aspx` | Delete Tax Invoice | Bill.Master / `WebForm29` | Bill.Master, CompanyID<br>menu `Delete_invoice` | `tbl_Client`, `tbl_Invoice`, `tbl_QuoPriSerTogather`, `tbl_Invoice_details`, `tbl_InvSiteAddress`, `tbl_Quotation`, `tbl_Quotaion_details`, `tbl_stock` | UPDATE, DELETE | this catalog |
| `corporate/business/app/InvoiceMail.aspx` | Tax Invoice Mailer | Bill.Master / `WebForm83` | Bill.Master, CompanyID<br>menu `InvoiceMail` | `tbl_Client`, `tbl_Invoice`, `tbl_representative`, `tbl_QutPrimaryService`, `tbl_Proforma` | UPDATE | [module](../11_Communications.md) |
| `corporate/business/app/Manual_Invoice.aspx` | Manual Tax Invoice | Bill.Master / `Manual_Invoice` | Bill.Master, CompanyID<br>menu `Add_DirectInvoice` | `tbl_Client`, `tbl_login`, `tbl_ClientRegAddress`, `tbl_NewparentProduct`, `tbl_NewProduct`, `tbl_Invoice`, `tbl_Invoice_details`, `tbl_InvSiteAddress` | INSERT, UPDATE | this catalog |
| `corporate/business/app/View_Invoice.aspx` | View Tax Invoices | Bill.Master / `WebForm27` | Bill.Master, CompanyID<br>menu `View_Invoice` | `tbl_Invoice`, `tbl_Client`, `tbl_QuoPriSerTogather`, `tbl_Quotation`, `tbl_login`, `tbl_Proforma`, `tbl_Chalan`, `tbl_Invoice_details` | INSERT | [module](../13_Invoice_Search_View_PO_Discovery.md) |
| `corporate/business/app/seartch_invoice.aspx` | Advanced Search Invoice | Bill.Master / `WebForm28` | Bill.Master, CompanyID<br>menu `seartch_invoice` | `tbl_Client`, `tbl_Invoice`, `tbl_QuoPriSerTogather`, `tbl_Quotation`, `tbl_login`, `tbl_Proforma`, `tbl_Chalan`, `tbl_Invoice_details` | INSERT | [module](../13_Invoice_Search_View_PO_Discovery.md) |

## Page-unique notes

- `corporate/business/app/seartch_invoice.aspx`: Filename misspelled `seartch`. Discovery: docs/13.
