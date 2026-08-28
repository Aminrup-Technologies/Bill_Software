# Module 11 — Email & SMS Integration

> Master Page Menu Position: **Corporate → Email** / **Corporate → SMS** (or accessible from administrative areas)

---

## 1. Overview

The Communications module provides email and SMS integration for the ERP system. Email is used for authentication flows (login OTP, password reset), sales visit notifications (chat messages, approval/rejection alerts), and potentially for quotation/PO delivery. SMS integration provides an additional notification channel for time-sensitive communications.

---

## 2. Files

| File | Type | Purpose |
|------|------|---------|
| `corporate/business/app/vw_dailyrpts.aspx.cs` | Backend | `SendChatEmailNotification` — salesperson-side chat notification email |
| `corporate/business/app/srch_dailyrpts.aspx.cs` | Backend | `SendChatEmailNotification`, `SendApprovalNotification` — manager-side notification emails |
| `index.aspx.cs` | Backend | `SendEmail` — authentication-related emails (OTP, password reset) |
| SMS pages (inferred) | Frontend/Backend | SMS sending and history |

### Email Configuration Sources

| Source | Files | Pattern |
|--------|-------|---------|
| **Hardcoded in source** | `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs` | Literal `SmtpClient("smtp.zoho.in", 587)` + `NetworkCredential(email, password)` — **Defect D-11** |
| **`ConfigurationManager.AppSettings`** | `index.aspx.cs` | Reads `SmtpFrom`, `SmtpUser`, `SmtpPass`, `SmtpHost`, `SmtpPort`, `SmtpEnableSsl` from `Web.config` — **correct pattern** |

---

## 3. Core Database Tables

| Table | Usage in This Module |
|-------|---------------------|
| `tbl_login` | Email address resolution for notification recipients |
| `tbl_SalesVisitReport` | Visit data embedded in email body |
| `tbl_SalesVisitResponses` | Chat history embedded in email body |
| `tbl_Expenses` | Expense data embedded in email body |
| `tbl_SystemNotification` | Audit trail for sent notifications |

### Key Queries for Email Recipient Resolution

| Recipient | Resolution Path |
|-----------|----------------|
| Manager (for salesperson chat reply) | `tbl_SalesVisitReport.CreatedByCode` → `tbl_login.User_Id` → `tbl_login.ReportingManagerId` → `Manager.Email` |
| Salesperson (for manager chat reply) | `tbl_SalesVisitReport.CreatedByCode` → `tbl_login.User_Id` → `tbl_login.Email` |
| Visit creator (for approval notification) | `tbl_SalesVisitReport.CreatedByCode` → `tbl_login.Email` |

---

## 4. Multi-Tenant Constraints

| Constraint | Status | Evidence |
|-----------|--------|----------|
| Email recipient resolution scoped by tenant | ❌ **Not enforced** | `ReportingManagerId` join does not include `CompanyID` — a manager in a different company could theoretically be the target if `ReportingManagerId` points to them |

### Tenant Isolation Gap

Email routing uses `ReportingManagerId` (which is a global, non-tenant-scoped column) to resolve the manager's email. If a `ReportingManagerId` value happens to point to a user in a different company, the notification email would be sent across tenant boundaries. This is a low-probability but theoretically possible cross-tenant data leak via email.

---

## 5. Proactive Notification Triggers

| Trigger | Email Subject | Recipient |
|---------|--------------|-----------|
| Salesperson sends chat reply | "Sales Visit Report - Salesperson Reply" | Manager (via `ReportingManagerId`) |
| Manager sends chat reply | "Sales Visit Report - Manager Reply" | Salesperson (via `CreatedByCode`) |
| Visit approved | "Sales Visit & Expenses - Approved (ID: {id})" | Visit creator (via `CreatedByCode`) |
| Visit rejected | "Sales Visit & Expenses - Rejected (ID: {id})" | Visit creator (via `CreatedByCode`) |

---

## 6. Architectural Notes

### Two Incompatible SMTP Configurations

The application uses **two different SMTP configurations** simultaneously:

1. **Authentication emails** (`index.aspx.cs`): Read from `ConfigurationManager.AppSettings` — configurable, rotatable without code changes.
2. **Sales Visit notifications** (`vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs`): **Hardcoded** in source code — requires code change and redeploy to rotate.

This means the Sales Visit workflow's SMTP credentials are:
- **Exposed in source control history** to anyone with repository access.
- **Not rotatable** via configuration — requires a code change and redeployment.
- **Inconsistent** with the application's own established pattern.

### Silent Failure Pattern (D-10)

All three notification-sending methods wrap their entire body in `catch (Exception) { /* fail silently */ }`. If SMTP delivery fails for any reason (credential expiry, network issue, DNS failure, recipient rejection), the user receives **zero indication** that the notification was not delivered. The underlying business action (chat message, approval) still reports success.

### Email Validation Inconsistency (D-07)

- `srch_dailyrpts.aspx.cs`: Validates recipient email with regex `^[^@\s]+@[^@\s]+\.[^@\s]+$` before attempting to send.
- `vw_dailyrpts.aspx.cs`: Only checks `string.IsNullOrWhiteSpace(emailTo)` — no format validation. A malformed address will reach `SmtpClient`, fail, and be silently swallowed (D-10).

### Email Body Content

Email templates embed:
- Full visit record details
- Complete chat history
- Hardcoded absolute attachment URL: `https://www.exc.aagroupindia.com/Uploads/{AttachmentName}`
- **Incorrect claim** that expenses were auto-updated with visit approval (see `08_Expense_Management.md`)

The hardcoded domain in email links means email content is environment-specific and would break if the deployment domain changes.

---

## 7. Known Defects

| ID | Severity | Description |
|----|----------|-------------|
| D-07 | Medium | Missing email format validation in salesperson-side notification |
| D-10 | Medium | Silent notification failures — all SMTP errors swallowed |
| D-11 | **High** | Hardcoded SMTP credentials committed to source control |
| D-18 | Architectural | Duplicated email template logic across two files |
| — | Medium | Email body incorrectly claims expenses were auto-updated |
| — | Low | Hardcoded deployment domain in email attachment links |
