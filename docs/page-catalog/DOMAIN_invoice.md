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

<!-- NARRATIVE:BEGIN -->

## Workflow

Tax invoice `INV/C/{fy}/{sl}`, `status1='No'`, `status2='Active'`. Does **not** set quotation `Status2` on create; delete sets `Status2='No'` and `InvStatus='No'` on `tbl_Quotaion_details`. Stock restore on delete is **commented out**.

Lineage / export: [docs/13](../13_Invoice_Search_View_PO_Discovery.md), [invoice_insert_data_lineage.md](../invoice_insert_data_lineage.md), [invoice_export_data_inventory.md](../invoice_export_data_inventory.md).

- **`Add_invoice`:** source `ddlDocType` = Quotation | Purchase Order | Delivery Challan | Proforma. WM `GetReconciliation`. Pending qty vs active invoices (`status2<>'Block'`). INSERT `tbl_Invoice`, `tbl_Invoice_details` (`Quotation_no` = source doc#), `tbl_InvSiteAddress`. Stock deduct `tbl_NewProduct` **except** when source is Delivery Challan. History prints `NewInvoice` / `NewInvoiceDuplicate` `?ID=`.
- **`Manual_Invoice`:** no source; `Quotation_No` = PO text or `N/A`; always stock deduct; blocks zero total / missing tax type. Menu `Add_DirectInvoice`.
- **`View_Invoice` / `seartch_invoice`:** list vs advanced search + Excel (`InvoiceListHelper.ExportXlsx`). docs/13 is the deep pass for these two.
- **`Block_invoice`:** `status2='Block'` (search Active only). Print mix of legacy `Invoice.aspx` and `NewInvoice.aspx`.
- **`InvoiceMail`:** stamps `mailStatus`/`mailDate`. UI title wrongly “Set Quotation”; grid print wrongly `proforma_invoice.aspx`.

Hydrant tax invoices are a **different** number series `INV/{clientInitials}/…` — [DOMAIN_hydrant.md](DOMAIN_hydrant.md).
