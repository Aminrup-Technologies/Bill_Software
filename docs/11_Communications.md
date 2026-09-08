# Communications (email / SMS)

AuthN/tenancy: [`page-catalog/SHARED_CONTEXT.md`](page-catalog/SHARED_CONTEXT.md).  
SMTP keys + `CommunicationGateway` + reset tokens: [`page-catalog/SHARED_RUNTIME.md`](page-catalog/SHARED_RUNTIME.md).  
Mailer **pages**: invoice [`DOMAIN_invoice.md`](page-catalog/DOMAIN_invoice.md), proforma [`DOMAIN_proforma.md`](page-catalog/DOMAIN_proforma.md), payment [`DOMAIN_payments.md`](page-catalog/DOMAIN_payments.md).

This file is the **mailer inventory**. Do not copy page census here.

---

## Two send paths (current code)

| Path | Config | Used by |
|------|--------|---------|
| **`CommunicationGateway`** | `Smtp*` AppSettings + MSG91 | Visit chat/approval (`vw_dailyrpts`, `srch_dailyrpts`), `index` reset mail, `settings` verification, `MyLeaves` / `AdminOverride` / `AdminApprovalDashboard` / `attendance` / `QuickAction` |
| **Direct `SmtpClient`** | Mix of AppSettings and leftover construction | `InvoiceMail`, `ProformaMail`, `PaymentMail`, `Set_quatation` (send commented out), `ViewUser`, `Update/password` |

Visit pages **no longer hardcode Zoho credentials** in source (audit **D-11 is stale** for `vw_dailyrpts` / `srch_dailyrpts`). They call `CommunicationGateway.SendCustomEmail`. Credential values still live in `Web.config` AppSettings — do not paste them into docs.

---

## Mailer pages / services

| Surface | Unique job |
|---------|------------|
| `InvoiceMail.aspx` | Stamps `tbl_Invoice.mailStatus` / `mailDate`. UI title wrongly “Set Quotation”; grid print wrongly `proforma_invoice.aspx`. Menu `InvoiceMail`. Not a `SecurePage`. |
| `ProformaMail.aspx` | Stamps proforma `mailStatus` / `mail_Date`. Menu `ProformaMail`. |
| `PaymentMail.aspx` | Stamps payment mail fields. Body historically hard-codes host `i2isoft.aminruptechnologies.co.in`. Menu `PaymentMail`. |
| `Set_quatation.aspx` | Menu “Set Quotation Permission”; stamps quote mail flags; **SendMail commented out**. |
| `QuickAction.aspx` | AES token `t` (leave/reg approve). Emails via gateway. |
| `PasswordResetService` | `dbo.PasswordResetTokens`; `{reset_password.aspx}?token=` + `uid`. |
| MSG91 | WhatsApp/SMS from `CommunicationGateway.SendAlertsAsync` when a mobile is passed. |
| iTop AppSettings | Support-ticket REST — **not** SMTP. |

PDF bytes typically come from the matching **print** page, not a third renderer.

---

## Not email

`GlobalNotification.ascx` is in-app (and currently throws in `OnInit`) — [`SHARED_RUNTIME.md`](page-catalog/SHARED_RUNTIME.md). `tbl_SystemNotification` INSERTs are audit/feed rows, not SMTP.
