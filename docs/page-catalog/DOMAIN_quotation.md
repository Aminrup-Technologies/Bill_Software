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

<!-- NARRATIVE:BEGIN -->

## Standard quotation (`tbl_Quotation`)

`Create_quotation` inserts `RecordType` = `Quotation` **or** `Purchase Order` (second menu `Li2`). QS `visitId` prefills / stores `VisitId`. Doc nos `QTN/{CompanyCode}/{FY}/n` or `PO/{CompanyCode}/{FY}/n`. Lines `tbl_Quotaion_details` (`IsLatest=1`); also `tbl_QutPaymentPhase`, `tbl_QutPrimaryService`. Notification module `SALES`. `sp_getapplock` around save. DeliveryDate/Department blanked for quotation lines. Client PO duplicate check on DO+PO+PO_Date.

Downstream flags (set by other domains): `Status1` proforma done; `Status3` cleared on DPCC delete; `PaymentStatus` on receipt.

### Pages

- **`View_quotation`:** current calendar month only; Excel export; print `NewQuotation.aspx?ID=` (no date gate).
- **`Seartch_quotation`:** Client / Date / both / QutNo. Print: date > 12-Jun-2018 → `NewQuotation`; else `Quotation.aspx`.
- **`Edit_quatation_v2`:** live editor. Update Existing (soft-delete latest lines, bump `Version`) or Save as New (archive old `IsLatest=0`, new `Quotation_no`). Print popup still `NewQuotation` even for PO rows.
- **`Edit_quatation`:** v1 leftover; search has no `RecordType` filter.
- **`Delete_Quotation`:** blocked if Status1/Status2/PaymentStatus = Yes; deletes quotation family tables. Search **not** limited to `RecordType='Quotation'` (can hit Client POs).
- **`Set_quatation`:** menu “Set Quotation Permission” actually stamps `mailStatus`/`mailStatusDate`. `SendMail` call is **commented out**; UI still says email sent.
- **`Vendor_quotation`:** orphan WIP — pick quotation + vendor GST, nothing persisted.

## Hydrant quotations (parallel schema)

Hidden Hydrent menu. `CreateHydrentQuatation` → `tbl_qsHydrentQuotation` / `tbl_qsHydrentDetails` (`invStatus='No'`). Numbering `I2I/{clientInitials}/{fy}/{n}`. Product from `tbl_HydrantProduct` or `tbl_Service`. Print `QuotationHydrent.aspx?Quotation_no=`. Delete also removes linked `tbl_HydrentInvoice`. Invoicing those quotes: [DOMAIN_hydrant.md](DOMAIN_hydrant.md).
