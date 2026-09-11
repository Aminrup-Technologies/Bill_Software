# Print layouts

> Domain key: `print` · 29 page(s)

Standalone (no Bill.Master). Auth via AuthGuard.EnsurePrint. Shared print gate: docs/16, docs/22.

**Do not copy** AuthN/AuthZ, tenancy, or Ponytail rules here. Those live in [SHARED_CONTEXT](SHARED_CONTEXT.md).

| Page | Purpose | Master / class | Gate | Tables (unique) | Ops | Narrative |
|------|---------|----------------|------|-----------------|-----|-----------|
| `corporate/business/print/General_expencess_voutcher.aspx` | General expencess voutcher<br>QS: `pament_made_id` | none / `General_expencess_voutcher` | EnsurePrint<br>print **fail-closed** (`unmapped`) | — | SELECT | this catalog |
| `corporate/business/print/Invoice.aspx` | Invoice<br>QS: `ID` | none / `Invoice` | EnsurePrint<br>print `tbl_Invoice.ID` | `tbl_Invoice`, `tbl_Quotaion_details`, `tbl_quotation_vat`, `tbl_Invoice_details`, `tbl_Quotation`, `tbl_Client`, `tbl_Factory` | SELECT | this catalog |
| `corporate/business/print/Invoiceduplicate.aspx` | Invoiceduplicate<br>QS: `ID` | none / `Invoiceduplicate` | EnsurePrint<br>print `tbl_Invoice.ID` | `tbl_Invoice`, `tbl_Quotaion_details`, `tbl_quotation_vat`, `tbl_Invoice_details`, `tbl_Quotation`, `tbl_Client`, `tbl_Factory` | SELECT | this catalog |
| `corporate/business/print/NewChhalan.aspx` | New Chhalan<br>QS: `Chalan_No` | none / `NewChhalan` | EnsurePrint<br>print `tbl_Chalan.Chalan_No` | `tbl_Chalan`, `tbl_ChaSiteAddress`, `tbl_Quotation`, `tbl_representative`, `tbl_Challan_details`, `tbl_Quotaion_details`, `tbl_Client` | SELECT | this catalog |
| `corporate/business/print/NewChhalanDuplicate.aspx` | New Chhalan Duplicate<br>QS: `Chalan_No` | none / `NewChhalanDuplicate` | EnsurePrint<br>print `tbl_Chalan.Chalan_No` | `tbl_Chalan`, `tbl_ChaSiteAddress`, `tbl_Challan_details`, `tbl_Quotaion_details`, `tbl_Quotation`, `tbl_Client`, `tbl_representative` | SELECT | this catalog |
| `corporate/business/print/NewChhalanTriplicate.aspx` | New Chhalan Triplicate<br>QS: `Chalan_No` | none / `NewChhalanTriplicate` | EnsurePrint<br>print `tbl_Chalan.Chalan_No` | `tbl_Chalan`, `tbl_ChaSiteAddress`, `tbl_Quotation`, `tbl_representative`, `tbl_Challan_details`, `tbl_Quotaion_details`, `tbl_Client` | SELECT | this catalog |
| `corporate/business/print/NewInvoice.aspx` | New Invoice<br>QS: `ID` | none / `NewInvoice` | EnsurePrint<br>print `tbl_Invoice.ID` | `tbl_Invoice`, `tbl_Quotation`, `tbl_InvSiteAddress`, `tbl_representative`, `tbl_Client`, `tbl_Quotaion_details`, `tbl_Invoice_details`, `tbl_quotation_vat` | SELECT | this catalog |
| `corporate/business/print/NewInvoiceDuplicate.aspx` | New Invoice Duplicate<br>QS: `ID` | none / `NewInvoiceDuplicate` | EnsurePrint<br>print `tbl_Invoice.ID` | `tbl_Invoice`, `tbl_Quotation`, `tbl_InvSiteAddress`, `tbl_representative`, `tbl_Client`, `tbl_Quotaion_details`, `tbl_Invoice_details`, `tbl_quotation_vat` | SELECT | this catalog |
| `corporate/business/print/NewInvoice_v2.aspx` | New Invoice v2<br>QS: `id` | none / `NewInvoice_v2` | EnsurePrint, CompanyID<br>print `tbl_Invoice.ID` | `tbl_Invoice`, `tbl_Client`, `tbl_InvSiteAddress`, `tbl_Quotation`, `tbl_Invoice_details` | SELECT | this catalog |
| `corporate/business/print/NewPaymentInvoice.aspx` | New Payment Invoice<br>QS: `Payment_ID` | none / `NewPaymentInvoice` | EnsurePrint<br>print `tbl_invoice_payment.Payment_ID` | `tbl_invoice_payment`, `tbl_InvSiteAddress`, `tbl_representative`, `tbl_Quotaion_details`, `tbl_quotation_vat`, `tbl_Quotation`, `tbl_Client` | SELECT | this catalog |
| `corporate/business/print/NewPaymentInvoiceDuplicate.aspx` | New Payment Invoice Duplicate<br>QS: `Payment_ID` | none / `NewPaymentInvoiceDuplicate` | EnsurePrint<br>print `tbl_invoice_payment.Payment_ID` | `tbl_invoice_payment`, `tbl_InvSiteAddress`, `tbl_representative`, `tbl_Quotaion_details`, `tbl_quotation_vat`, `tbl_Quotation`, `tbl_Client` | SELECT | this catalog |
| `corporate/business/print/NewProformaInvoice.aspx` | New Proforma Invoice<br>QS: `ID` | none / `NewProformaInvoice` | EnsurePrint<br>print `tbl_Proforma.ID` | `tbl_Proforma`, `tbl_Quotation`, `tbl_Quotaion_details`, `tbl_Proforma_Details`, `tbl_quotation_vat`, `tbl_representative`, `tbl_Client` | SELECT | this catalog |
| `corporate/business/print/NewPurchaseOrder.aspx` | New Purchase Order<br>QS: `ID` | none / `NewPurchaseOrder` | EnsurePrint<br>print `tbl_Quotation.ID` | `tbl_Quotation` | — | [module](../10_Purchase_Order.md) |
| `corporate/business/print/NewPurchaseOrder_Print.aspx` | New Purchase Order Print<br>QS: `ID`, `autoprint`, `letterhead` | none / `NewPurchaseOrder_Print` | EnsurePrint<br>print `tbl_Quotation.ID` | `tbl_Quotation` | — | [module](../10_Purchase_Order.md) |
| `corporate/business/print/NewQuotation.aspx` | New Quotation<br>QS: `ID` | none / `NewQuotation` | EnsurePrint<br>print `tbl_Quotation.ID` | `tbl_Quotation`, `tbl_Financials`, `tbl_QutPrimaryService`, `tbl_PrimaryServiceTerms`, `tbl_QuoPserTerm`, `tbl_Quotaion_details`, `tbl_QutPaymentPhase`, `tbl_Client` | SELECT | [module](../09_Quotation_Generation.md) |
| `corporate/business/print/Patty_cash_expencess_voutcher.aspx` | Patty cash expencess voutcher<br>QS: `payment_id` | none / `Patty_cash_expencess_voutcher` | EnsurePrint<br>print **fail-closed** (`unmapped`) | `tbl_patty_cash_expenses` | SELECT | this catalog |
| `corporate/business/print/Print_PO.aspx` | Print PO<br>QS: `poId` | none / `Print_PO` | EnsurePrint<br>print `tbl_PO_Header.PO_Id` | `tbl_PO_Header`, `sp_GetReleasedPO_Details()` | SELECT | this catalog |
| `corporate/business/print/Quotation.aspx` | Quotation<br>QS: `ID` | none / `Quotation` | EnsurePrint<br>print `tbl_Quotation.ID` | `tbl_Quotation`, `tbl_Quotaion_details`, `tbl_quotation_vat`, `tbl_Client` | SELECT | [module](../09_Quotation_Generation.md) |
| `corporate/business/print/QuotationHydrent.aspx` | Quotation Hydrent<br>QS: `Quotation_no` | none / `QuotationHydrent` | EnsurePrint<br>print `tbl_qsHydrentQuotation.Quotation_no` | `tbl_qsHydrentQuotation`, `tbl_qsHydrentDetails`, `tbl_Client` | SELECT | [module](../09_Quotation_Generation.md) |
| `corporate/business/print/Requisition.aspx` | Requisition<br>QS: `requeno` | none / `Requisition` | EnsurePrint<br>print **fail-closed** (`unmapped`) | `tbl_requisition`, `tbl_requisitionBankDetails` | SELECT | this catalog |
| `corporate/business/print/RequisitionNew.aspx` | Create Purchase Requisition<br>QS: `ReqNo` | none / `RequisitionNew` | EnsurePrint<br>print `tbl_RequisitionMain.ReqNo` | `tbl_RequisitionMain`, `tbl_Client`, `tbl_RequisitionNew` | SELECT | this catalog |
| `corporate/business/print/bill.aspx` | bill<br>QS: `Payment_ID` | none / `bill` | EnsurePrint<br>print `tbl_invoice_payment.Payment_ID` | `tbl_invoice_payment`, `tbl_Quotaion_details`, `tbl_quotation_vat`, `tbl_Quotation`, `tbl_Client`, `tbl_Invoice_details` | SELECT | this catalog |
| `corporate/business/print/billduplicate.aspx` | billduplicate<br>QS: `Payment_ID` | none / `billduplicate` | EnsurePrint<br>print `tbl_invoice_payment.Payment_ID` | `tbl_invoice_payment`, `tbl_Quotaion_details`, `tbl_quotation_vat`, `tbl_Quotation`, `tbl_Client`, `tbl_Invoice_details` | SELECT | this catalog |
| `corporate/business/print/chhalan.aspx` | chhalan<br>QS: `Chalan_No` | none / `chhalan` | EnsurePrint<br>print `tbl_Chalan.Chalan_No` | `tbl_Chalan`, `tbl_Challan_details`, `tbl_Client`, `tbl_Factory` | SELECT | this catalog |
| `corporate/business/print/hydrentInvoice.aspx` | hydrent Invoice<br>QS: `ID` | none / `hydrentInvoice` | EnsurePrint<br>print `tbl_HydrentInvoice.ID` | `tbl_HydrentInvoice`, `tbl_qsHydrentDetails`, `tbl_Client`, `tbl_Factory` | SELECT | this catalog |
| `corporate/business/print/hydrentInvoiceSeller.aspx` | hydrent Invoice Seller<br>QS: `ID` | none / `hydrentInvoiceSeller` | EnsurePrint<br>print `tbl_HydrentInvoice.ID` | `tbl_HydrentInvoice`, `tbl_qsHydrentDetails`, `tbl_Client`, `tbl_Factory` | SELECT | this catalog |
| `corporate/business/print/proforma_invoice.aspx` | proforma invoice<br>QS: `ID` | none / `proforma_invoice` | EnsurePrint<br>print `tbl_Proforma.ID` | `tbl_Proforma`, `tbl_Quotaion_details`, `tbl_quotation_vat`, `tbl_Quotation`, `tbl_Client` | SELECT | this catalog |
| `corporate/business/print/purches_bill.aspx` | purches bill<br>QS: `Purches_Id` | none / `purches_bill` | EnsurePrint<br>print `tbl_Purches.Purches_Id` | `tbl_Purches`, `tbl_purches_details`, `tbl_Vendor` | SELECT | this catalog |
| `corporate/business/print/purchess_payment.aspx` | purchess payment<br>QS: `Payment_ID` | none / `purchess_payment` | EnsurePrint<br>print **fail-closed** (`unmapped`) | `tbl_purches_details`, `tbl_Purchess_payment`, `tbl_Vendor` | SELECT | this catalog |

## Page-unique notes

- `corporate/business/print/General_expencess_voutcher.aspx`: `EnsurePrint` resource `unmapped` — **fails closed** (Phase 0A).
- `corporate/business/print/NewInvoice_v2.aspx`: CompanyID-aware print variant; live search popups still open `NewInvoice.aspx` / `NewInvoiceDuplicate.aspx`.
- `corporate/business/print/NewPurchaseOrder.aspx`: Prints from `tbl_Quotation.ID` (quotation-backed PO), not `tbl_PO_Header`.
- `corporate/business/print/Patty_cash_expencess_voutcher.aspx`: `EnsurePrint` resource `unmapped` — **fails closed** (Phase 0A).
- `corporate/business/print/Requisition.aspx`: `EnsurePrint` resource `unmapped` — **fails closed** (Phase 0A).
- `corporate/business/print/purchess_payment.aspx`: `EnsurePrint` resource `unmapped` — **fails closed** (Phase 0A).

<!-- NARRATIVE:BEGIN -->

## How prints are used

App pages open these as popups. Gate is `EnsurePrint(resource, QS)` — see [SHARED_CONTEXT](SHARED_CONTEXT.md). Only **`NewInvoice_v2`** applies `CompanyID` in its own code-behind (`QS id` lowercase) and offers buyer/transporter/supplier copies on one page. Live invoice search still opens `NewInvoice` / `NewInvoiceDuplicate`.

### Tax invoice

| Copy | Live URL | QS |
|------|----------|-----|
| Buyer | `NewInvoice.aspx` | `ID` |
| Seller | `NewInvoiceDuplicate.aspx` | `ID` |
| Legacy + factory | `Invoice.aspx` / `Invoiceduplicate.aspx` | `ID` |

`bill` / `billduplicate` are **payment-receipt** tax invoices (`Payment_ID`), not `tbl_Invoice.ID`.

### Quotation / client PO

`NewQuotation.aspx?ID=` (terms/phases). Legacy `Quotation.aspx` for dates ≤ 12-Jun-2018. Client PO: `NewPurchaseOrder.aspx?ID=` / `NewPurchaseOrder_Print` (`letterhead`, `autoprint`) — EnsurePrint resource is **`tbl_Quotation.ID`**, not `tbl_PO_Header`. Vendor PO: `Print_PO.aspx?poId=` → `sp_GetReleasedPO_Details`.

### Challan / proforma / payment / hydrant / purchase

- Challan: `NewChhalan` consignee, `Duplicate` transporter, `Triplicate` consignor (`Chalan_No`). Legacy `chhalan.aspx`.
- Proforma: `NewProformaInvoice` vs legacy `proforma_invoice`.
- Customer payment: `NewPaymentInvoice` / `Duplicate` (`Payment_ID`).
- Hydrant: `QuotationHydrent?Quotation_no=`; invoice `hydrentInvoice` / `Seller` (`ID`).
- Vendor purchase bill: `purches_bill?Purches_Id=`.

### Fail-closed (do not treat as authorized)

`Requisition.aspx` (`requeno`), `Patty_cash_expencess_voutcher.aspx` (`payment_id`), `General_expencess_voutcher.aspx` (`pament_made_id`), `purchess_payment.aspx` (`Payment_ID`). Modern PR print is `RequisitionNew.aspx?ReqNo=` (`tbl_RequisitionMain.ReqNo`) and joins `tbl_Client` **by name** with no tenant predicate.
