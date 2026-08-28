# Module 07 — Daily Visit Reports & Manager Approval

> Master Page Menu Position: **Corporate → My Sales Visits** (salesperson) / **Corporate → Manager Dashboard** (manager)

---

## 1. Overview

This module encompasses two complementary views of the Sales Visit workflow:

1. **Daily Visit Reports** (`daily_rpt.aspx`) — Data entry form for creating new visits (planning future visits or logging past visits retroactively).
2. **Sales Visit Management** (`vw_dailyrpts.aspx` + `srch_dailyrpts.aspx`) — Salesperson's own visit list with edit/chat capability, and the manager's cross-employee search/approve/reject dashboard.

Together, these pages implement the full visit lifecycle from creation through review, approval, rejection, and manager-salesperson communication.

---

## 2. Files

| File | Type | Purpose |
|------|------|---------|
| `corporate/business/app/daily_rpt.aspx` | Frontend | Visit creation form (plan mode and past mode) |
| `corporate/business/app/daily_rpt.aspx.cs` | Backend | `btnSubmit_Click` (INSERT), `GetAdminName` (user name lookup) |
| `corporate/business/app/vw_dailyrpts.aspx` | Frontend | "My Sales Visits" grid + mega-modal (4-tab: Details, Location, Expenses, Chat) |
| `corporate/business/app/vw_dailyrpts.aspx.cs` | Backend | `BindSalesVisits`, `LoadMegaModal`, `btnUpdateVisit_Click`, `btnMegaSendChat_Click`, `SendChatEmailNotification`, `GetVisitEmailBody` |
| `corporate/business/app/srch_dailyrpts.aspx` | Frontend | "Manager Dashboard" search grid + mega-modal (4-tab + approval footer) |
| `corporate/business/app/srch_dailyrpts.aspx.cs` | Backend | `Binder`, `LoadMegaModal`, `ProcessApproval`, `gvMegaExpenses_RowCommand`, `GetUserRole`, `SendChatEmailNotification`, `SendApprovalNotification`, `GetVisitEmailBody` |

### Supporting Files

| File | Relationship |
|------|-------------|
| `corporate/business/app/visit_planner.aspx[.cs]` | Calendar-based execution path (alternative entry point for visit execution) |
| `corporate/business/app/expense_entry.aspx[.cs]` | Expense entry linked from mega-modal expenses tab |
| `corporate/business/app/Create_quotation.aspx[.cs]` | Quotation generation linked from executed visit details |
| `DB_UTILITY.cs` | Database connection utilities |
| `Bill.Master.cs` | `CompanyContext.CurrentCompanyID` — used for tenant scoping in manager search |

---

## 3. Core Database Tables

| Table | Usage in This Module |
|-------|---------------------|
| `tbl_SalesVisitReport` | Primary entity — `INSERT` (create visit), `SELECT` (list, detail, search), `UPDATE` (edit, approve/reject) |
| `tbl_SalesVisitResponses` | Chat thread — `INSERT` (post message), `SELECT` (chat history), `COUNT` (manager comment check for edit lock) |
| `tbl_Expenses` | Expense display (read-only in mega-modal) and per-expense approve/reject |
| `tbl_login` | User identity, name resolution, manager hierarchy (`ReportingManagerId`) for email routing |

### Key Columns Referenced

#### `tbl_SalesVisitReport`

| Column | Read/Write | Module |
|--------|-----------|--------|
| `Id` | Read/Write | All — visit PK |
| `VisitDate` | Read/Write | All — visit date |
| `VisitEndDate` | Read/Write | Create/Edit — visit end time |
| `Salesperson` | Read | Display — denormalized name from `tbl_login.Name` at creation |
| `CustomerName` | Read/Write | All — free-text customer name |
| `Department` | Read/Write | Create/Edit |
| `ContactPerson` | Read/Write | Create/Edit |
| `VisitType` | Read/Write | All — `Office Visit`, `Plant Visit`, `Online Meeting` |
| `DiscussionPoints` | Read/Write | All — free text |
| `VisitPhase` | Read/Write | All — `Planned` / `Executed` |
| `Status` | Read/Write | All — `Pending Execution`, `Pending`, `Completed`, `Escalated` |
| `FollowUpRequired` | Read/Write | Create — `Yes` / `No` / `''` |
| `NextFollowUpDate` | Read/Write | Create |
| `AttachmentName` | Read/Write | Create/Edit — file reference |
| `ExecutionDateTime` | Write | Create (past mode) |
| `CreatedDate` | Write | Create |
| `CreatedByCode` | Write | Create — session `USERID` |
| `ApprovalStatus` | Read/Write | Approve/Reject — `Pending` / `Approved` / `Rejected` |
| `ManagerRemarks` | Read/Write | Approve/Reject |
| `ApprovedDate` | Write | Approve/Reject |
| `ApprovedBy` | Write | Approve/Reject — session `USERID` |
| `Latitude` / `Longitude` | Read | Location tab display |
| `CompanyID` | — | **Never set on INSERT** (D-01); **used in manager search list** but **not re-checked on detail/mutation endpoints** |

#### `tbl_SalesVisitResponses`

| Column | Purpose |
|--------|---------|
| `VisitId` | FK → `tbl_SalesVisitReport.Id` |
| `RespondentRole` | `Manager` / `Salesperson` (inferred, not derived from role table) |
| `RespondentCode` | FK → `tbl_login.User_Id` |
| `ResponseText` | Chat message body |
| `ResponseDate` | `GETDATE()` at insert |

#### `tbl_Expenses` (displayed in mega-modal, approved/rejected from manager view)

| Column | Purpose |
|--------|---------|
| `Id` | Expense PK |
| `VisitId` | FK → `tbl_SalesVisitReport.Id` (nullable) |
| `ExpenseCategory` | Category |
| `Amount` | Decimal amount |
| `Description` | Description |
| `AttachmentName` | Receipt file reference |
| `ApprovalStatus` | `Pending` / `Approved` / `Rejected` |

---

## 4. Multi-Tenant Constraints

### `daily_rpt.aspx` (Visit Creation)

| Constraint | Status | Evidence |
|-----------|--------|----------|
| `CreatedByCode` populated from session | ✅ Enforced | `Session["USERID"]` → `CreatedByCode` parameter |
| `CompanyID` populated on INSERT | ❌ **NOT set** | The `INSERT` column list omits `CompanyID` entirely (Defect D-01) |
| Hardcoded fallback `"FLM03"` | ⚠️ Present | If `Session["USERID"]` is null at postback time, falls back to hardcoded `"FLM03"` (Defect D-03) |

### `vw_dailyrpts.aspx` (Salesperson's Own Visits)

| Constraint | Status | Evidence |
|-----------|--------|----------|
| `CreatedByCode` filter on list query | ✅ Enforced | `BindSalesVisits`: `WHERE CreatedByCode = @CreatedByCode` |
| `CreatedByCode` filter on detail fetch | ❌ **NOT enforced** | `LoadMegaModal`: `WHERE v.Id=@Id` only (Defect D-04) |
| `CreatedByCode` filter on UPDATE | ❌ **NOT enforced** | `btnUpdateVisit_Click`: `WHERE Id=@Id AND ApprovalStatus='Pending' AND NOT EXISTS(...)` — business-rule guard only, no ownership check (Defect D-04) |
| `CompanyId` filter | ❌ **Never applied** | This page scopes by ownership only |

### `srch_dailyrpts.aspx` (Manager Dashboard)

| Constraint | Status | Evidence |
|-----------|--------|----------|
| `CompanyId` filter on salesperson dropdown | ✅ Enforced | `WHERE CompanyID = @CompanyID` |
| `CompanyId` filter on search results | ✅ Enforced (but **string-concatenated**, not parameterized — Defect D-05) | `Binder()`: `WHERE CompanyID = <int>` via raw string concatenation |
| `CompanyId` re-check on detail fetch | ❌ **NOT enforced** | `LoadMegaModal`: `WHERE Id=@Id` only (Defect D-04) |
| `CompanyId` re-check on approval UPDATE | ❌ **NOT enforced** | `ProcessApproval`: `WHERE Id=@Id` only (Defect D-04) |
| `ReportingManagerId` check on approval | ❌ **Never used for authorization** | Any user who can reach the page can approve/reject any visit in any company (Defect D-16) |
| `ApprovalStatus='Pending'` guard on approval | ❌ **NOT enforced** | No idempotency guard — can re-approve/re-reject already-actioned visits (Defect D-08) |

---

## 5. Proactive Notification Triggers

| Trigger | Table | Description |
|---------|-------|-------------|
| Chat message sent (salesperson view) | `tbl_SystemNotification` + Email | Notification logged and email sent to manager |
| Chat message sent (manager view) | `tbl_SystemNotification` + Email | Notification logged and email sent to opposite party |
| Visit approved/rejected | `tbl_SystemNotification` + Email | Notification logged and email sent to visit creator |
| Expense approved/rejected | `tbl_SystemNotification` | Notification logged |

### Email Notification Flow

1. **Salesperson sends chat** → `SendChatEmailNotification` routes to manager's email via `ReportingManagerId` join
2. **Manager sends chat** → `SendChatEmailNotification` routes to salesperson's email via `CreatedByCode` join
3. **Manager approves/rejects visit** → `SendApprovalNotification` routes to visit creator's email

**SMTP configuration:** Both `vw_dailyrpts.aspx.cs` and `srch_dailyrpts.aspx.cs` **hardcode** SMTP credentials (`smtp.zoho.in:587`) in source code instead of reading from `ConfigurationManager.AppSettings` (Defect D-11).

---

## 6. Edit-Lock Rules

Edit-lock logic is implemented in `vw_dailyrpts.aspx.cs :: LoadMegaModal` (salesperson's own edit UI). A visit is **locked** (non-editable) when any of these conditions is true:

| Condition | UI Enforced | Server `UPDATE` Guard |
|-----------|:-----------:|:---------------------:|
| `ApprovalStatus != 'Pending'` | ✅ | ✅ |
| Visit age > 45 days (from `VisitDate`) | ✅ | ❌ **Not re-checked** |
| Manager has left at least one comment | ✅ | ✅ |

The 45-day age lock and the ownership check are both **only enforced client-side** — they can be bypassed by submitting a forged postback.

---

## 7. Architectural Notes

### Code Duplication (D-18)

`vw_dailyrpts.aspx.cs` and `srch_dailyrpts.aspx.cs` each independently implement near-duplicate versions of:
- `GetVisitEmailBody` — HTML email template generation
- `SendChatEmailNotification` — chat notification email
- Chat insertion and history rendering

These have already **drifted** (D-07): the salesperson-side implementation lacks email validation that the manager-side has, creating inconsistent failure behavior.

### Status Vocabulary Mismatch (D-02)

The four data-entry surfaces (`daily_rpt.aspx` plan mode, `daily_rpt.aspx` past mode, `visit_planner.aspx` execute modal, `vw_dailyrpts.aspx` edit modal) each define their own `Status` dropdown options independently:
- Execute flows write `"Pending"`
- Edit dropdown only offers `Completed`, `Pending Execution`, `Escalated` (no `Pending`)
- Opening the edit modal on a `"Pending"` visit silently resets the Status dropdown to blank (Defect D-02)

### Validation Gap (D-13)

- Client-side: `validateSalesVisitForm()` enforces required fields on creation
- Server-side: **No re-validation** of required fields — empty fields are persisted if client JS is bypassed
- Edit modal: **No client-side or server-side validation** on `btnUpdateVisit_Click`

### Raw Exception Surfacing (D-14)

All catch blocks surface `ex.Message` directly to users via `alert()` or label text, leaking internal SQL Server error details.

---

## 8. Known Defects

| ID | Severity | Description |
|----|----------|-------------|
| D-01 | Critical | `CompanyID` never populated on INSERT — breaks manager dashboard visibility and downstream features |
| D-02 | Medium | `Status` vocabulary mismatch — `"Pending"` silently lost on edit |
| D-03 | Medium | Hardcoded `"FLM03"` fallback on session expiry |
| D-04 | **Critical** | IDOR across all detail/mutation endpoints — no ownership/tenant check on `WHERE Id=@Id` |
| D-05 | **Critical** | SQL injection in `srch_dailyrpts.aspx.cs :: Binder()` |
| D-07 | Medium | Missing email validation in salesperson-side chat notification |
| D-08 | Medium | No idempotency guard on visit approval |
| D-10 | Medium | Silent notification failures (SMTP errors swallowed) |
| D-11 | High | Hardcoded SMTP credentials in source code |
| D-13 | Medium | No server-side required-field validation |
| D-14 | Low | Raw exception messages shown to end users |
| D-15 | Architectural | Two incompatible tenancy philosophies in one workflow |
| D-16 | Architectural | `ReportingManagerId` used only for routing, never authorization |
| D-18 | Architectural | Duplicated/drifted business logic across two files |
| D-19 | Architectural | Three data-entry surfaces with different field vocabularies |
