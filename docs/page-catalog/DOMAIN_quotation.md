# Quotations

> Domain key: `quotation` · 12 page(s)

Create/view/search/edit/delete + hydrant quotations. Narrative: docs/09.

**Do not copy** AuthN/AuthZ, tenancy, or Ponytail rules here. Those live in [SHARED_CONTEXT](SHARED_CONTEXT.md).

| Page | Purpose | Master / class | Gate | Tables (unique) | Ops | Narrative |
|------|---------|----------------|------|-----------------|-----|-----------|
| `corporate/business/app/CreateHydrentQuatation.aspx` | Create New Quotation | Bill.Master / `CreateHydrentQuatation` | Bill.Master<br>menu `CreateHydrentQuatation` | `tbl_Client`, `tbl_HydrantProduct`, `tbl_Service`, `tbl_Quotaion_details`, `tbl_qsHydrentQuotation`, `tbl_Quotation`, `tbl_qsHydrentDetails` | INSERT | [module](../09_Quotation_Generation.md) |
| `corporate/business/app/Create_quotation.aspx` | Create Quotations<br>QS: `visitId` | Bill.Master / `WebForm19` | Bill.Master, CompanyID<br>menu `Create_quotation` | `tbl_SalesVisitReport`, `tbl_login`, `tbl_Client`, `tbl_City`, `tbl_NewProduct`, `tbl_NewparentProduct`, `tbl_Quotation`, `tbl_Quotaion_details` | INSERT | [module](../09_Quotation_Generation.md) |
| `corporate/business/app/DeleteHydrentQuatation.aspx` | Delete New Quotation | Bill.Master / `DeleteHydrentQuatation` | Bill.Master<br>menu `DeleteHydrentQuatation` | `tbl_Client`, `tbl_qsHydrentQuotation`, `tbl_qsHydrentDetails`, `tbl_HydrentInvoice` | DELETE | [module](../09_Quotation_Generation.md) |
| `corporate/business/app/Delete_Quotation.aspx` | Delete Quotation | Bill.Master / `WebForm25` | Bill.Master, CompanyID<br>menu `Delete_Quotation` | `tbl_Client`, `tbl_QuoPriSerTogather`, `tbl_Quotation`, `tbl_Quotaion_details`, `tbl_quotation_vat`, `tbl_QutPrimaryService`, `tbl_QutPaymentPhase`, `tbl_QuoPserTerm` | DELETE | [module](../09_Quotation_Generation.md) |
| `corporate/business/app/Edit_quatation.aspx` | Edit quatation | Bill.Master / `WebForm65` | Bill.Master, CompanyID<br>— | `tbl_Client`, `tbl_City`, `tbl_Quotation`, `tbl_QuoPriSerTogather`, `tbl_Quotaion_details`, `tbl_QutPaymentPhase`, `tbl_QutPrimaryService`, `tbl_PaymentPhase` | INSERT, UPDATE, DELETE | [module](../09_Quotation_Generation.md) |
| `corporate/business/app/Edit_quatation_v2.aspx` | Edit Quotation | Bill.Master / `Edit_quatation_v2` | Bill.Master, CompanyID<br>menu `Edit_quatation` | `tbl_login`, `tbl_Client`, `tbl_City`, `tbl_Quotation`, `tbl_QuoPriSerTogather`, `tbl_Quotaion_details`, `tbl_NewProduct`, `tbl_NewparentProduct` | INSERT, UPDATE, DELETE | [module](../09_Quotation_Generation.md) |
| `corporate/business/app/SearchHydrentQuatation.aspx` | Search New Quotation | Bill.Master / `SearchHydrentQuatation` | Bill.Master, CompanyID<br>menu `SearchHydrentQuatation` | `tbl_Client`, `tbl_qsHydrentQuotation` | SELECT | [module](../09_Quotation_Generation.md) |
| `corporate/business/app/Seartch_quotation.aspx` | Search Quotation | Bill.Master / `WebForm24` | Bill.Master, CompanyID<br>menu `Seartch_quotation` | `tbl_Client`, `tbl_Quotation`, `tbl_QuoPriSerTogather`, `tbl_Quotaion_details` | SELECT | [module](../09_Quotation_Generation.md) |
| `corporate/business/app/Set_quatation.aspx` | Set Quotation Permission | Bill.Master / `WebForm82` | Bill.Master, CompanyID<br>menu `SetQuatation` | `tbl_Client`, `tbl_Quotation`, `tbl_QutPrimaryService`, `tbl_representative`, `tbl_QuoPriSerTogather` | UPDATE | [module](../09_Quotation_Generation.md) |
| `corporate/business/app/Vendor_quotation.aspx` | Vendor quotation | Bill.Master / `WebForm89` | Bill.Master, CompanyID<br>— | `tbl_Client`, `tbl_PrimaryServiceTerms`, `tbl_Quotation`, `tbl_QuoPriSerTogather`, `tbl_Vendor` | SELECT | [module](../05_Customer_Vendor.md) |
| `corporate/business/app/ViewHydrentQuatation.aspx` | View New Quotation | Bill.Master / `ViewHydrentQuatation` | Bill.Master<br>menu `ViewHydrentQuatation` | `tbl_Client`, `tbl_qsHydrentQuotation` | SELECT | [module](../09_Quotation_Generation.md) |
| `corporate/business/app/View_quotation.aspx` | View Quotation | Bill.Master / `WebForm23` | Bill.Master, CompanyID<br>menu `View_quotation` | `tbl_Quotation`, `tbl_Client`, `tbl_QuoPriSerTogather`, `tbl_Quotaion_details` | SELECT | [module](../09_Quotation_Generation.md) |

## Page-unique notes

- `corporate/business/app/Create_quotation.aspx`: Also linked from menu id `Li2` as Create Purchase Order (same page, second entry).
- `corporate/business/app/Edit_quatation.aspx`: Leftover v1 editor. Live menu `Edit_quatation` points at `Edit_quatation_v2.aspx`.
