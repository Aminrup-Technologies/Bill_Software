# PR / PO

> Domain key: `pr-po` · 18 page(s)

Purchase requisition → purchase order. Narrative: docs/10.

**Do not copy** AuthN/AuthZ, tenancy, or Ponytail rules here. Those live in [SHARED_CONTEXT](SHARED_CONTEXT.md).

| Page | Purpose | Master / class | Gate | Tables (unique) | Ops | Narrative |
|------|---------|----------------|------|-----------------|-----|-----------|
| `corporate/business/app/Approve_PR.aspx` | Approve / Reject PR | Bill.Master / `Approve_PR` | Bill.Master, CompanyID<br>menu `pr_approve` | `tbl_RequisitionMain`, `tbl_SystemNotification`, `sp_Requisition_Approve()` | INSERT | this catalog |
| `corporate/business/app/Generate_PO_From_PR.aspx` | Generate Purchase Order | Bill.Master / `Generate_PO_From_PR` | Bill.Master, CompanyID<br>menu `po_generate` | `tbl_RequisitionMain`, `tbl_PO_Header` | SELECT | [module](../10_Purchase_Order.md) |
| `corporate/business/app/Generate_PO_Preview.aspx` | Generate PO Preview<br>QS: `reqNo` | Bill.Master / `Generate_PO_Preview` | Bill.Master, CompanyID<br>— | `tbl_Company`, `tbl_Client`, `tbl_RequisitionMain`, `tbl_RequisitionNew`, `tbl_PO_Header`, `tbl_PO_Items`, `tbl_PO_PartySnapshot`, `tbl_Vendor` | INSERT, UPDATE, DELETE | [module](../10_Purchase_Order.md) |
| `corporate/business/app/RequisitionCreate.aspx` | Requisition Create | Bill.Master / `WebForm66` | Bill.Master, CompanyID<br>— | `tbl_Client`, `tbl_parentProduct`, `tbl_Service`, `tbl_Product`, `tbl_requisitionBankDetails`, `tbl_requisition` | INSERT | this catalog |
| `corporate/business/app/RequisitionManual.aspx` | Create | Bill.Master / `WebForm71` | Bill.Master<br>menu `RequisitionManual` | `tbl_Client`, `tbl_RequisitionNew`, `tbl_RequisitionMain` | INSERT | this catalog |
| `corporate/business/app/RequisitionManualDelete.aspx` | Delete | Bill.Master / `WebForm74` | Bill.Master, CompanyID<br>menu `RequisitionManualDelete` | `tbl_Client`, `tbl_RequisitionMain`, `tbl_RequisitionNew` | DELETE | this catalog |
| `corporate/business/app/RequisitionManualSearch.aspx` | Search | Bill.Master / `WebForm73` | Bill.Master, CompanyID<br>menu `RequisitionManualSearch` | `tbl_Client`, `tbl_RequisitionMain` | SELECT | this catalog |
| `corporate/business/app/RequisitionManualView.aspx` | View | Bill.Master / `WebForm72` | Bill.Master<br>menu `RequisitionManualView` | `tbl_RequisitionMain` | SELECT | this catalog |
| `corporate/business/app/RequisitionNew.aspx` | Create Requisition<br>QS: `reqNo`<br>WM: `GetProductDetail` | Bill.Master / `RequisitionNew` | Bill.Master, CompanyID, WebMethod<br>menu `pr_create` | `tbl_Vendor`, `tbl_Vat_Master`, `tbl_NewparentProduct`, `tbl_Service`, `tbl_NewProduct`, `tbl_RequisitionMain`, `tbl_RequisitionNew`, `tbl_SystemNotification` | INSERT, UPDATE | this catalog |
| `corporate/business/app/RequisitionView.aspx` | Requisition View | Bill.Master / `WebForm67` | Bill.Master<br>— | `tbl_requisitionBankDetails` | SELECT | this catalog |
| `corporate/business/app/Search_purchaseorder.aspx` | Search Purchase Order | Bill.Master / `Search_purchaseorder` | Bill.Master, CompanyID<br>menu `Li3` | `tbl_Client`, `tbl_QuoPriSerTogather`, `tbl_Quotation`, `tbl_Quotaion_details` | SELECT | [module](../10_Purchase_Order.md) |
| `corporate/business/app/View_PO.aspx` | Review & Release PO | Bill.Master / `View_PO` | Bill.Master, CompanyID<br>menu `po_view` | `tbl_PO_Header`, `tbl_Vendor`, `tbl_login` | SELECT | [module](../10_Purchase_Order.md) |
| `corporate/business/app/View_PO_Details.aspx` | View PO Details<br>QS: `poId` | Bill.Master / `View_PO_Details` | Bill.Master, CompanyID<br>— | `tbl_PO_Header`, `tbl_Vendor`, `tbl_PO_PartySnapshot`, `tbl_Company`, `tbl_PO_Items`, `sp_ReleasePO_Final()` | SELECT | [module](../10_Purchase_Order.md) |
| `corporate/business/app/View_PR.aspx` | View Purchase Requisition | Bill.Master / `View_PR` | Bill.Master, CompanyID<br>menu `pr_view` | `tbl_RequisitionMain`, `tbl_login` | SELECT | this catalog |
| `corporate/business/app/View_PR_Details.aspx` | Modify/View PR<br>QS: `mode`, `reqNo`<br>WM: `GetProductDetail` | Bill.Master / `View_PR_Details` | Bill.Master, CompanyID, WebMethod<br>— | `tbl_Vendor`, `tbl_Vat_Master`, `tbl_RequisitionMain`, `tbl_RequisitionNew`, `tbl_NewparentProduct`, `tbl_Service`, `tbl_NewProduct`, `sp_SubmitRequisition()` | UPDATE, DELETE | this catalog |
| `corporate/business/app/View_PurchaseOrder.aspx` | View Purchase Orders | Bill.Master / `View_PurchaseOrder` | Bill.Master, CompanyID<br>menu `View_po` | `tbl_QuoPriSerTogather`, `tbl_Quotation`, `tbl_Client`, `tbl_login`, `tbl_Quotaion_details` | SELECT | [module](../10_Purchase_Order.md) |
| `corporate/business/app/delete_purchaseorder.aspx` | Delete Purchase Order | Bill.Master / `delete_purchaseorder` | Bill.Master, CompanyID<br>menu `Li4` | `tbl_Client`, `tbl_QuoPriSerTogather`, `tbl_Quotation`, `tbl_Quotaion_details`, `tbl_quotation_vat`, `tbl_QutPrimaryService`, `tbl_QutPaymentPhase`, `tbl_QuoPserTerm` | DELETE | [module](../10_Purchase_Order.md) |
| `corporate/business/app/edit_purchaseorder.aspx` | Edit Purchase Order | Bill.Master / `edit_purchaseorder` | Bill.Master, CompanyID<br>menu `Li5` | `tbl_login`, `tbl_Client`, `tbl_City`, `tbl_Quotation`, `tbl_QuoPriSerTogather`, `tbl_Quotaion_details`, `tbl_NewparentProduct`, `tbl_NewProduct` | INSERT, UPDATE, DELETE | [module](../10_Purchase_Order.md) |

## Page-unique notes

- `corporate/business/app/View_PO_Details.aspx`: PO line/header detail + `sp_ReleasePO_Final`. Opened from `View_PO.aspx` with QS `poId`.
