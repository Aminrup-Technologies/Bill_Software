# Payments received & purchase payments

> Domain key: `payments` · 12 page(s)

Collections, due lists, mailers, purchase-side payments.

**Do not copy** AuthN/AuthZ, tenancy, or Ponytail rules here. Those live in [SHARED_CONTEXT](SHARED_CONTEXT.md).

| Page | Purpose | Master / class | Gate | Tables (unique) | Ops | Narrative |
|------|---------|----------------|------|-----------------|-----|-----------|
| `corporate/business/app/Delete_payment.aspx` | Delete Payment | Bill.Master / `WebForm37` | Bill.Master, CompanyID<br>menu `Delete_payment` | `tbl_Client`, `tbl_invoice_payment`, `tbl_QuoPriSerTogather`, `tbl_Quotation`, `tbl_Invoice`, `tbl_invoice_due` | UPDATE, DELETE | this catalog |
| `corporate/business/app/Delete_purches_payment.aspx` | Delete Payment | Bill.Master / `WebForm49` | Bill.Master, CompanyID<br>menu `Delete_purches_payment` | `tbl_Vendor`, `tbl_Purchess_payment`, `tbl_purches_due` | UPDATE, DELETE | this catalog |
| `corporate/business/app/FinalPaymentInvoice.aspx` | Final Payment Invoice | Bill.Master / `WebForm86` | Bill.Master, CompanyID<br>menu `FinalPaymentInvoice` | `tbl_Client`, `tbl_invoice_payment`, `tbl_QuoPriSerTogather` | SELECT | this catalog |
| `corporate/business/app/PaymentMail.aspx` | Payment Invoice Mailer | Bill.Master / `WebForm85` | Bill.Master, CompanyID<br>menu `PaymentMail` | `tbl_Client`, `tbl_invoice_payment`, `tbl_QuoPriSerTogather`, `tbl_representative`, `tbl_QutPrimaryService`, `tbl_Proforma` | UPDATE | [module](../11_Communications.md) |
| `corporate/business/app/PaymentsDue.aspx` | Payments Due | Bill.Master / `WebForm88` | Bill.Master, CompanyID<br>menu `PaymentsDue` | `tbl_Client`, `tbl_invoice_payment`, `tbl_QuoPriSerTogather`, `tbl_Quotation` | SELECT | this catalog |
| `corporate/business/app/PaymentsReceived.aspx` | Payments Received | Bill.Master / `WebForm87` | Bill.Master, CompanyID<br>— | `tbl_Client`, `tbl_invoice_payment`, `tbl_QuoPriSerTogather` | SELECT | this catalog |
| `corporate/business/app/Seartch_purchess_payments.aspx` | Search Payment | Bill.Master / `WebForm48` | Bill.Master, CompanyID<br>menu `Seartch_purchess_payments` | `tbl_Vendor`, `tbl_Purchess_payment` | SELECT | this catalog |
| `corporate/business/app/View_payment.aspx` | View Payment | Bill.Master / `WebForm35` | Bill.Master, CompanyID<br>menu `View_payment` | `tbl_Client`, `tbl_invoice_payment`, `tbl_QuoPriSerTogather`, `tbl_Quotation` | SELECT | this catalog |
| `corporate/business/app/View_purchess_payment.aspx` | View Payment against Purchase | Bill.Master / `WebForm47` | Bill.Master<br>menu `View_purchess_payment` | `tbl_Vendor`, `tbl_Purchess_payment` | SELECT | this catalog |
| `corporate/business/app/add_payment.aspx` | Add Payment | Bill.Master / `WebForm34` | Bill.Master, CompanyID<br>menu `add_payment` | `tbl_Client`, `tbl_Quotation`, `tbl_QuoPriSerTogather`, `tbl_invoice_due`, `tbl_invoice_payment`, `tbl_Invoice`, `tbl_invoice_payment_tds` | INSERT, UPDATE | this catalog |
| `corporate/business/app/add_payment_purchess.aspx` | Add Payment against Purchase | Bill.Master / `WebForm46` | Bill.Master, CompanyID<br>menu `add_payment_purchess` | `tbl_Vendor`, `tbl_Purches`, `tbl_purches_due`, `tbl_Purchess_payment` | INSERT, UPDATE | this catalog |
| `corporate/business/app/seartch_payment.aspx` | Search Payment | Bill.Master / `WebForm36` | Bill.Master, CompanyID<br>menu `seartch_payment` | `tbl_Client`, `tbl_invoice_payment`, `tbl_QuoPriSerTogather` | SELECT | this catalog |

<!-- NARRATIVE:BEGIN -->

## Customer receipts

Receipts are against **quotation**, not an invoice picker. IDs `P00{n}`. Modes Cash/Cheque/DD.

`add_payment`: INSERT `tbl_invoice_payment` (TDS; column typo `currancy`); upsert `tbl_invoice_due` (`qutation_no`); optional `tbl_invoice_payment_tds`; when due=`0.00` stamps `Invoice_No` from `tbl_Invoice`; `PaymentStatus='Yes'`.

Print pair: `NewPaymentInvoice.aspx?Payment_ID=` (buyer) / `NewPaymentInvoiceDuplicate` (seller). Legacy `bill.aspx` is the same document type (payment-backed).

- **`View_payment`:** joined to existing quotations.
- **`seartch_payment` / `Delete_payment`:** delete may set `PaymentStatus='No'`, `tbl_Invoice.status1='No'`, adjust due.
- **`FinalPaymentInvoice` / `PaymentsReceived`:** `Due_amount='0.00'` only. Titles both say “Search Payment”.
- **`PaymentsDue`:** remaining due per quotation `Net_amount - SUM(Given_amount)`. No live print on the grid. Distinct from report `Payment_due.aspx`.
- **`PaymentMail`:** `mailStatus`/`mailDate` on payment. Email body hard-codes host `i2isoft.aminruptechnologies.co.in`. [docs/11](../11_Communications.md).

## Vendor purchase payments

`add_payment_purchess` → `tbl_Purchess_payment` + UPDATE `tbl_purches_due`. List/search/delete print `purchess_payment.aspx?Payment_ID=` which is **EnsurePrint unmapped (fail-closed)**. Delete restores due (`Due_amount += Given_amount`).
