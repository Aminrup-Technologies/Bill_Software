# Module 08 — Expense Claims & Approval

> Master Page Menu Position: **Corporate → Expenses** (salesperson) / **Corporate → Manager Dashboard** (within mega-modal, manager)

---

## 1. Overview

The Expense Management module handles the submission, tracking, and approval of expense claims by field-sales personnel. Expenses can be linked to specific sales visits (displayed in the visit's mega-modal "Expenses" tab) or submitted independently. Each expense undergoes a manager approval workflow with per-item approve/reject actions.

---

## 2. Files

| File | Type | Purpose |
|------|------|---------|
| `corporate/business/app/expense_entry.aspx` | Frontend | Expense claim submission form |
| `corporate/business/app/expense_entry.aspx.cs` | Backend | `btnSubmit_Click` — expense INSERT with optional visit linkage |
| `corporate/business/app/srch_dailyrpts.aspx` | Frontend | Manager view — expenses displayed in mega-modal "Expenses" tab with per-row approve/reject |
| `corporate/business/app/srch_dailyrpts.aspx.cs` | Backend | `gvMegaExpenses_RowCommand` — per-expense approve/reject |
| `corporate/business/app/vw_dailyrpts.aspx` | Frontend | Salesperson view — expenses displayed read-only in mega-modal "Expenses" tab |
| `corporate/business/app/vw_dailyrpts.aspx.cs` | Backend | Expense `SELECT` for display (read-only, no mutation) |

---

## 3. Core Database Tables

| Table | Usage in This Module |
|-------|---------------------|
| `tbl_Expenses` | Primary entity — `INSERT` (create), `SELECT` (display), `UPDATE` (approve/reject) |
| `tbl_SalesVisitReport` | Referenced for visit pre-fill (`CustomerName`, `VisitDate`, `DiscussionPoints`) and parent visit linkage |
| `tbl_login` | User identity (`UserCode`, `ApprovedBy`) |
| `tbl_SystemNotification` | Audit logging on expense approval/rejection |

### Key Columns — `tbl_Expenses`

| Column | Read/Write | Purpose |
|--------|-----------|---------|
| `Id` | Read/Write | Expense PK |
| `UserCode` | Write | FK → `tbl_login.User_Id` (who submitted) |
| `ExpenseDate` | Write | Date of the expense |
| `VisitId` | Write | FK → `tbl_SalesVisitReport.Id` (**nullable** — expenses can exist without a visit) |
| `ExpenseCategory` | Write | Category of expense |
| `Amount` | Write | Decimal/money amount |
| `Description` | Write | Description text |
| `AttachmentName` | Write | Receipt file reference (`~/Uploads/Expenses/`) |
| `ApprovalStatus` | Read/Write | `Pending` / `Approved` / `Rejected` |
| `ApprovedBy` | Write | FK → `tbl_login.User_Id` (approving manager) |
| `ApprovedDate` | Write | `GETDATE()` at approval |
| `CreatedDate` | Write | `GETDATE()` at creation |

---

## 4. Multi-Tenant Constraints

| Constraint | Status | Evidence |
|-----------|--------|----------|
| `CompanyId` filter on expense display | ❌ **NOT enforced** | Expense queries use `WHERE VisitId=@Id` with no tenant re-check (inherits parent visit's scope only if the parent query was tenant-scoped, which is inconsistent) |
| `CompanyId` filter on expense approval UPDATE | ❌ **NOT enforced** | `gvMegaExpenses_RowCommand`: `UPDATE WHERE Id=@Id` only |
| Ownership check on expense approval | ❌ **NOT enforced** | No check that the approving user is authorized for this expense |

### Tenant Isolation Gap

Expenses are displayed and approved/rejected using bare `Id` lookups with no `CompanyId` or ownership re-verification. Since the manager dashboard's **list** query is `CompanyId`-scoped, the expense rows displayed to a manager are company-filtered. However, a forged postback with an expense `Id` from a different company could still trigger the approval/rejection UPDATE.

---

## 5. Proactive Notification Triggers

| Trigger | Table | Description |
|---------|-------|-------------|
| Expense approved or rejected | `tbl_SystemNotification` | Audit notification logged |
| Expense approval (email) | Email via `SendApprovalNotification` | Email sent to expense submitter (note: the current approval email references the parent visit's overall approval — see note below) |

### Email Notification for Expenses

The current email notification system is **visit-centric**: `SendApprovalNotification` in `srch_dailyrpts.aspx.cs` sends an email about the **visit** approval status change, not specifically about individual expense approvals. Individual expense approve/reject actions via `gvMegaExpenses_RowCommand` do **not** trigger their own email notification — only the `tbl_SystemNotification` audit entry is created.

---

## 6. Architectural Notes

### Visit-Expense Decoupling

Expenses and visit approvals are **explicitly decoupled**:

- Approving/rejecting a **visit** (`ProcessApproval`) does **not** affect its linked expenses (the bulk-update was intentionally removed, per code comment: `// (REMOVED the bulk tbl_Expenses update from here)`).
- Expenses must be approved/rejected **individually** via `gvMegaExpenses_RowCommand`.
- The approval email body still contains the sentence "Any expenses linked to this visit have also been updated to {status}" — this is now **factually incorrect** (Defect noted in `06_Business_Rules_Requiring_Confirmation.md` item #4).

### File Storage

Expense receipts are stored at `~/Uploads/Expenses/` with filenames prefixed by `"EXP_"` + timestamp. Like visit attachments, these are served as **unauthenticated static content** — anyone with the URL can download the receipt without logging in (Defect D-09).

### Independent Expenses

The `VisitId` column is nullable, meaning expenses can exist without being linked to any visit. These "orphan" expenses would only be visible to the manager via a search that includes visit-less expenses — the current mega-modal only shows expenses filtered by `VisitId`, so unlinkable expenses may have limited visibility.

---

## 7. Known Defects

| ID | Severity | Description |
|----|----------|-------------|
| D-04 | Critical | IDOR — expense approve/reject uses bare `WHERE Id=@Id` with no ownership/tenant check |
| D-09 | High | Expense receipts served from unauthenticated static path |
| — | Medium | Approval email text incorrectly claims expenses were auto-updated with visit approval |
| — | Low | No dedicated email notification for individual expense approval/rejection actions |
