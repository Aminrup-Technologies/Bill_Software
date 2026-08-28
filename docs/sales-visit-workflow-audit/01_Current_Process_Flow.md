# 01 — Current Process Flow: Sales Visit Workflow

**Scope (read-only architecture discovery):**
- `Bill_Software/corporate/business/app/visit_planner.aspx` (+ `.cs`)
- `Bill_Software/corporate/business/app/daily_rpt.aspx` (+ `.cs`)
- `Bill_Software/corporate/business/app/vw_dailyrpts.aspx` (+ `.cs`)
- `Bill_Software/corporate/business/app/srch_dailyrpts.aspx` (+ `.cs`)

Supporting files referenced for traceability only (not modified): `DB_UTILITY.cs`, `Bill.Master.cs` (`CompanyContext`), `index.aspx.cs`, `expense_entry.aspx.cs`, `Create_quotation.aspx.cs`, `AdminAttendanceDashboard.aspx.cs`, `home.aspx.cs`.

No SQL/DDL scripts, migrations, or stored-procedure definitions exist in this repository for the Sales Visit workflow. All schema facts below are **inferred from ADO.NET code** (column names used in `SELECT`/`INSERT`/`UPDATE` statements), not from an authoritative schema source.

---

## A. Page Flow Table

| # | Page | User Action | JavaScript Function | ASP.NET Control / Event | C# Method | SQL Query (summarized) | DB Table(s) | Result | Next Page/State |
|---|------|-------------|----------------------|--------------------------|-----------|--------------------------|-------------|--------|------------------|
| 1 | `visit_planner.aspx` | Loads "My Visit Calendar" | `DOMContentLoaded` → FullCalendar `events` callback | PageMethod `GetCalendarEvents` (WebMethod, static) | `visit_planner.aspx.cs :: GetCalendarEvents()` | `SELECT Id, VisitDate, VisitEndDate, CustomerName, VisitPhase FROM tbl_SalesVisitReport WHERE CreatedByCode=@UserId` | `tbl_SalesVisitReport` | JSON array of calendar events (also drives the side "My Itinerary" list) | Renders calendar/list in place |
| 2 | `visit_planner.aspx` | Drags/clicks an empty calendar slot | FullCalendar `select` callback | none (client redirect) | — | — | — | Confirms past vs future via `confirm()` | Navigates to `daily_rpt.aspx?start=..&end=..&mode=plan\|past` |
| 3 | `visit_planner.aspx` | Clicks an **Executed** event/card | `eventClick` → `handleVisitClick(id,'Executed')` | PageMethod `GetVisitDetails` | `visit_planner.aspx.cs :: GetVisitDetails(int visitId)` | `SELECT ... FROM tbl_SalesVisitReport WHERE Id=@Id` (no ownership filter — see `04_Security_and_Tenant_Audit.md`) | `tbl_SalesVisitReport` | JSON visit detail incl. Lat/Long, attachment, follow-up | Opens read-only "Executed Visit Details" modal with links to `expense_entry.aspx?visitId=` and `Create_quotation.aspx?visitId=` |
| 4 | `visit_planner.aspx` | Clicks a **Planned** event/card | `eventClick` → `handleVisitClick(id,'Planned')` | opens `#executeModal` | — | — | — | — | Shows Execute form (discussion, status, follow-up, attachment) |
| 5 | `visit_planner.aspx` | Clicks "📍 Execute & Tag Location" | `captureLocationAndSubmit()` → `navigator.geolocation.getCurrentPosition` → sets hidden fields → programmatically clicks hidden `btnSubmitExecution` | `asp:Button btnSubmitExecution` (`OnClick`) | `visit_planner.aspx.cs :: btnSubmitExecution_Click` | (1) `UPDATE tbl_SalesVisitReport SET VisitPhase='Executed', ExecutionDateTime=GETDATE(), Latitude=@Latitude, Longitude=@Longitude, DiscussionPoints=..., Status=..., FollowUpRequired=..., NextFollowUpDate=..., AttachmentName=ISNULL(@AttachmentName, AttachmentName) WHERE Id=@Id`; (2) conditional `INSERT INTO tbl_SalesVisitReport (... ParentVisitId) SELECT ... FROM tbl_SalesVisitReport WHERE Id=@Id` when `FollowUpRequired='Yes' AND NextFollowUpDate IS NOT NULL` | `tbl_SalesVisitReport` | Visit becomes `VisitPhase='Executed'`; optional new `Planned` child row linked via `ParentVisitId` | `Response.Redirect(Request.RawUrl)` → calendar reloads |
| 6 | `daily_rpt.aspx?mode=plan` | Fills in future visit form, clicks "Save Record" | `validateSalesVisitForm()` (client required-field + chronology checks) | `asp:Button btnSubmit` (`OnClientClick` + `OnClick`) | `daily_rpt.aspx.cs :: btnSubmit_Click` | `INSERT INTO tbl_SalesVisitReport (VisitDate, VisitEndDate, Salesperson, CustomerName, Department, ContactPerson, VisitType, DiscussionPoints, VisitPhase='Planned', Status='Pending Execution', FollowUpRequired='', NextFollowUpDate=NULL, AttachmentName=NULL, ExecutionDateTime=NULL, CreatedDate=Today, CreatedByCode)` | `tbl_SalesVisitReport` | New `Planned` visit row created (note: **`CompanyID` is never populated** — see `05_Potential_Defects.md` D-01) | JS alert + redirect to `visit_planner.aspx` |
| 7 | `daily_rpt.aspx?mode=past` | Logs an already-completed visit | Same `validateSalesVisitForm()`, additionally requires Follow-Up & Status | Same `btnSubmit` | `daily_rpt.aspx.cs :: btnSubmit_Click` | Same `INSERT`, but `VisitPhase='Executed'`, `ExecutionDateTime = VisitStart` (not "now"), `Status`, `FollowUpRequired`, `NextFollowUpDate`, `AttachmentName` from form | `tbl_SalesVisitReport` | New `Executed` visit row, **no GPS captured**, **no automatic follow-up row spawned** even if `FollowUpRequired='Yes'` | JS alert + redirect to `visit_planner.aspx` |
| 8 | `daily_rpt.aspx` (both modes) | Page load | — | `Page_Load` | `daily_rpt.aspx.cs :: GetAdminName()` | `select Name from tbl_login where User_Id='<Session USERID>'` (string-concatenated, not parameterized) | `tbl_login` | Populates read-only Salesperson name field | — |
| 9 | `vw_dailyrpts.aspx` | Loads "My Sales Visits" | jQuery UI datepicker init | `Page_Load` (`!IsPostBack`) | `vw_dailyrpts.aspx.cs :: BindSalesVisits()` | `SELECT Id, VisitDate, CustomerName, VisitType, Status, ApprovalStatus FROM tbl_SalesVisitReport WHERE CreatedByCode=@CreatedByCode [AND date range] [AND Status]` | `tbl_SalesVisitReport` | Grid of the salesperson's own visits (last 30 days default) | — |
| 10 | `vw_dailyrpts.aspx` | Clicks "👁️ View / Edit File" | `GridView` `OnRowCommand` | `gvSalesVisits_RowCommand` → `LoadMegaModal(visitId)` | `vw_dailyrpts.aspx.cs :: LoadMegaModal` | `SELECT v.*, (SELECT COUNT(*) FROM tbl_SalesVisitResponses r WHERE r.VisitId=v.Id AND r.RespondentRole='Manager') FROM tbl_SalesVisitReport v WHERE v.Id=@Id`; + `SELECT ... FROM tbl_Expenses WHERE VisitId=@Id`; + chat query (below) | `tbl_SalesVisitReport`, `tbl_Expenses`, `tbl_SalesVisitResponses`, `tbl_login` | Populates 4-tab "Visit File" modal (Details/Location/Expenses/Chat); computes **edit-lock** state (see `03_State_Machine.md` / `04_Security_and_Tenant_Audit.md`) | Opens `#megaModal` |
| 11 | `vw_dailyrpts.aspx` | Edits details, clicks "💾 Save Changes" | none (no client validator wired) | `asp:Button btnUpdateVisit` | `vw_dailyrpts.aspx.cs :: btnUpdateVisit_Click` | `UPDATE tbl_SalesVisitReport SET VisitDate=..., CustomerName=..., ... WHERE Id=@Id AND ApprovalStatus='Pending' AND NOT EXISTS (SELECT 1 FROM tbl_SalesVisitResponses WHERE VisitId=Id AND RespondentRole='Manager')` | `tbl_SalesVisitReport` | Row updated only if still `Pending` and no manager comment exists (server re-checks 2 of 3 lock conditions — **45-day rule is NOT re-checked server-side**) | Success panel + modal closes + grid rebinds |
| 12 | `vw_dailyrpts.aspx` | Types a reply, clicks "➤ Reply" | none | `asp:Button btnMegaSendChat` | `vw_dailyrpts.aspx.cs :: btnMegaSendChat_Click` | `INSERT INTO tbl_SalesVisitResponses (VisitId, RespondentRole='Salesperson', RespondentCode, ResponseText, ResponseDate=GETDATE())` | `tbl_SalesVisitResponses` | Chat message stored | Triggers `SendChatEmailNotification` (see `06`); modal reopens on Chat tab |
| 13 | `srch_dailyrpts.aspx` | Loads "Manager Dashboard" | Select2 init on salesperson dropdown | `Page_Load` | inline in `Page_Load` | `SELECT User_Id, Name+' ['+User_Id+']' FROM tbl_login WHERE CompanyID=@CompanyID AND IsActive=1 AND User_Id NOT IN ('admin','AT01')` | `tbl_login` | Populates salesperson filter dropdown (tenant-scoped) | — |
| 14 | `srch_dailyrpts.aspx` | Sets filters, clicks "🔍 Search" | none | `asp:Button btnSertch` | `srch_dailyrpts.aspx.cs :: Binder()` | `SELECT * FROM tbl_SalesVisitReport WHERE CompanyID=<int> [AND CreatedByCode='<val>'] [AND VisitDate BETWEEN '<val>' AND '<val>']` — **built via raw string concatenation, not parameterized** | `tbl_SalesVisitReport` | `DataList2` populated with matching visits | — |
| 15 | `srch_dailyrpts.aspx` | Clicks "👁️ View Complete File" | `DataList` `OnItemCommand` | `DataList2_ItemCommand` → `LoadMegaModal(visitId)` | `srch_dailyrpts.aspx.cs :: LoadMegaModal` | `SELECT * FROM tbl_SalesVisitReport WHERE Id=@Id` (no CompanyID re-check); `SELECT Id, ExpenseDate, ExpenseCategory, Description, Amount, AttachmentName, ApprovalStatus FROM tbl_Expenses WHERE VisitId=@Id`; chat query | `tbl_SalesVisitReport`, `tbl_Expenses`, `tbl_SalesVisitResponses`, `tbl_login` | Populates 4-tab modal + approval panel (visible only if `ApprovalStatus='Pending'`) | Opens `#megaModal` |
| 16 | `srch_dailyrpts.aspx` | Clicks "✔ Approve Visit" / "✖ Reject Visit" | none | `btnMegaApprove` / `btnMegaReject` | `ProcessApproval("Approved"\|"Rejected")` | `UPDATE tbl_SalesVisitReport SET ApprovalStatus=@Status, ManagerRemarks=@Remarks, ApprovedDate=GETDATE(), ApprovedBy=@User WHERE Id=@Id` (no `ApprovalStatus='Pending'` guard, no manager-relationship check, no CompanyID check) | `tbl_SalesVisitReport` | Visit's `ApprovalStatus` transitions | Triggers `SendApprovalNotification`; modal hides; grid rebinds |
| 17 | `srch_dailyrpts.aspx` | Clicks ✔/✖ on an individual expense row | `GridView` `OnRowCommand` | `gvMegaExpenses_RowCommand` | inline | `UPDATE tbl_Expenses SET ApprovalStatus=@Status, ApprovedBy=@User, ApprovedDate=GETDATE() WHERE Id=@Id` | `tbl_Expenses` | Expense approval status changes individually (bulk cascade from visit approval was explicitly removed — see code comment `// (REMOVED the bulk tbl_Expenses update from here)`) | Expenses tab re-renders |
| 18 | `srch_dailyrpts.aspx` | Types a reply, clicks "➤ Send" | none | `asp:Button btnMegaSendChat` | `btnMegaSendChat_Click` → `GetUserRole()` | `INSERT INTO tbl_SalesVisitResponses (...)`; role resolved via `SELECT CreatedByCode FROM tbl_SalesVisitReport WHERE Id=@Id` (equality with sender ⇒ "Salesperson", else defaults to "Manager") | `tbl_SalesVisitResponses`, `tbl_SalesVisitReport` | Chat message stored, labeled by inferred role | Triggers `SendChatEmailNotification`; grid + modal refresh |

---

## Narrative Walkthrough

### 1. Planning a visit
Salesperson opens `visit_planner.aspx` → clicks an empty future date/time on the FullCalendar → confirms via `confirm()` dialog → browser navigates to `daily_rpt.aspx?start=...&end=...&mode=plan`. The form pre-fills start/end from the query string (with defensive parsing for URL-encoded timezone offsets — see code comment header in `daily_rpt.aspx.cs`). Submitting creates a `tbl_SalesVisitReport` row with `VisitPhase='Planned'`, `Status='Pending Execution'`.

### 2. Logging a visit that already happened
Same page, `mode=past` (reached either via calendar click on a past date + confirm, or by direct navigation). Adds the Execution panel (Follow-Up, Next Follow-Up, Status, Attachment). On submit, the row is created **directly** as `VisitPhase='Executed'` with `ExecutionDateTime` set to the *declared start time of the visit*, not the moment of data entry. **No GPS is requested or stored in this path.**

### 3. Executing a Planned visit from the calendar
Clicking a `Planned` calendar entry opens the Execute modal on `visit_planner.aspx` itself. Submission is gated behind the browser's HTML5 Geolocation API (`captureLocationAndSubmit`): the visible button is disabled and its label changes to "📍 Acquiring GPS..." while `getCurrentPosition` resolves; only on success are the hidden Latitude/Longitude fields populated and the real (hidden) ASP.NET button programmatically clicked. If geolocation fails or is unsupported, the postback never happens — GPS is a **hard client-side gate** for this specific path only.

The server-side handler runs a single `SqlCommand` containing two T-SQL statements back to back (an `UPDATE` followed by a conditional `INSERT`), not wrapped in an explicit `BEGIN TRANSACTION`. See `05_Potential_Defects.md` for the atomicity implication.

### 4. Reviewing / editing one's own visit history
`vw_dailyrpts.aspx` ("My Sales Visits") lists the current user's own visits (filtered by `CreatedByCode`, not `CompanyID`). Opening a row's "Visit File" shows four tabs: Edit Details, Location Map, Claimed Expenses (read-only here), and Manager Chat. Editability is computed in `LoadMegaModal` based on `ApprovalStatus`, visit age, and whether a manager has commented (see `03_State_Machine.md`).

### 5. Manager review / approval
`srch_dailyrpts.aspx` ("Manager Dashboard") lets any user who can reach the page search all visits within their `CompanyID`, optionally filtered by salesperson and/or date range. Opening a visit's file shows the same four-tab layout plus an Approve/Reject footer (visible only while `ApprovalStatus='Pending'`) and per-expense Approve/Reject actions. Approving/rejecting the visit does not automatically approve/reject its linked expenses (explicitly decoupled per in-code comment).

### 6. Follow-ups and downstream features
- Attaching an expense (`expense_entry.aspx?visitId=`) and generating a quotation (`Create_quotation.aspx?visitId=`) are reachable only from the **Executed Visit Details** view on `visit_planner.aspx`.
- Follow-up visits are spawned **only** by the calendar "Execute" path (§3), never by `daily_rpt.aspx?mode=past` or by editing a visit in `vw_dailyrpts.aspx` — see `06_Business_Rules_Requiring_Confirmation.md`.

---

## F. Validation (Client-side vs. Server-side)

### Client-side (JavaScript) validation

| Page | Function | Rules Enforced |
|---|---|---|
| `daily_rpt.aspx` | `validateSalesVisitForm()` | Start/End required; End strictly after Start (`new Date(visitEnd) > new Date(visitStart)`); Customer Name, Department, Contact Person, Visit Type required; Discussion/Agenda required (label differs by mode); **only in `mode=past`**: Follow-Up selection and Status selection required. Blocks postback via `OnClientClick="return validateSalesVisitForm();"` |
| `visit_planner.aspx` | `captureLocationAndSubmit()` | Discussion text required (`trim() !== ''`) before allowing geolocation capture/submission; blocks entirely if `navigator.geolocation` unsupported or the user denies/times out the permission prompt |
| `vw_dailyrpts.aspx` (Edit tab) | *(none)* | No client-side validator is wired to `btnUpdateVisit` — the Save Changes button has no `OnClientClick` guard at all |
| `srch_dailyrpts.aspx` (Approve/Reject) | *(none)* | No client-side check that Manager Remarks is non-empty before approving/rejecting, despite the placeholder text implying it should be provided |

### Server-side validation

| Page / Method | What is actually validated |
|---|---|
| `daily_rpt.aspx.cs :: btnSubmit_Click` | **No explicit re-validation** of required fields. Relies solely on `Convert.ToDateTime(...)` / implicit string assignment throwing an unhandled-looking exception (caught by a blanket `try/catch` that surfaces `ex.Message` directly to the user) if a date field is unparsable. Text fields (`CustomerName`, `Department`, etc.) are **never re-checked for emptiness** server-side — if client JS is bypassed, blank values are persisted as valid rows. |
| `visit_planner.aspx.cs :: btnSubmitExecution_Click` | No server-side re-check that `txtExecDiscussion` is non-empty (client-side check only). `Convert.ToDecimal(latitude)` / `Convert.ToInt32(visitId)` will throw if hidden fields are tampered with or empty — again surfaced as a raw exception message via `Response.Write` script alert. |
| `vw_dailyrpts.aspx.cs :: btnUpdateVisit_Click` | No field-level validation at all. The **only** enforcement is the `UPDATE ... WHERE Id=@Id AND ApprovalStatus='Pending' AND NOT EXISTS(...)` guard, which is a *business-rule* (edit-lock) check, not an input-validation check. |
| `srch_dailyrpts.aspx.cs :: ProcessApproval` | No validation that `remarks` is non-empty. No validation that `status` is one of the two expected literals (method is only ever called internally with `"Approved"`/`"Rejected"`, so not directly attacker-reachable, but there is no defensive check either). |

**Gap:** every server-side data-entry method in this workflow trusts client-side validation for required-field completeness. There is no shared/reusable server-side validation layer (e.g. Data Annotations, FluentValidation, or a common helper) — see `05_Potential_Defects.md`.

---

## H. GPS Capture — Exactly When Latitude/Longitude Are Captured and Stored

1. **Capture trigger:** GPS is captured **only** when a user clicks "📍 Execute & Tag Location" inside the Execute modal on `visit_planner.aspx` (i.e., only for `Planned → Executed` transitions performed through the calendar UI).
2. **Mechanism:** `captureLocationAndSubmit()` calls the browser's `navigator.geolocation.getCurrentPosition(...)` with `{ enableHighAccuracy: true, timeout: 10000, maximumAge: 0 }`. On success, `position.coords.latitude`/`longitude` are written into hidden fields `hfLatitude`/`hfLongitude`, and only then is the hidden server button (`btnSubmitExecution`) programmatically clicked to trigger the postback.
3. **Persistence:** `visit_planner.aspx.cs :: btnSubmitExecution_Click` converts the hidden-field strings to `decimal` and writes them into `tbl_SalesVisitReport.Latitude` / `.Longitude` as part of the same `UPDATE` that flips `VisitPhase` to `Executed`.
4. **Where GPS is *not* captured:**
   - `daily_rpt.aspx?mode=past` — logging an already-executed visit retroactively never asks for or stores location, even though the resulting row is immediately `VisitPhase='Executed'`.
   - Auto-generated follow-up visits (`ParentVisitId` children) — correctly, since they are created as `Planned` (not yet executed) and inherit no Lat/Long.
   - `vw_dailyrpts.aspx` edits — the edit form has no location fields; Lat/Long are display-only (Location Map tab) and cannot be modified or backfilled after the fact.
5. **Failure handling:** if geolocation is denied/times out, the modal shows an alert and re-enables the button; **the visit cannot be executed via the calendar path without granting location access.** This is a hard client-side requirement with no server-side enforcement fallback (i.e., nothing prevents bypassing it by using the `daily_rpt.aspx?mode=past` path instead — see `06_Business_Rules_Requiring_Confirmation.md`).
