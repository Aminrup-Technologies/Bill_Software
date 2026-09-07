# Module 06 — Sales Visit Calendar & Planning

> Master Page Menu Position: **Corporate → Sales Visit Planner** (salesperson role)

---

## 1. Overview

The Sales Visit Planner is the primary interface for field-sales personnel to manage their visit schedule. It provides a FullCalendar-based calendar view of planned and executed visits, an itinerary sidebar, and the execution workflow (including GPS geolocation capture and attachment upload). This module also handles follow-up visit auto-generation.

---

## 2. Files

| File | Type | Purpose |
|------|------|---------|
| `corporate/business/app/visit_planner.aspx` | Frontend | FullCalendar UI, itinerary sidebar, Execute modal, Executed Visit Details modal |
| `corporate/business/app/visit_planner.aspx.cs` | Backend | `GetCalendarEvents` (PageMethod), `GetVisitDetails` (PageMethod), `btnSubmitExecution_Click` (execute + follow-up) |

### Supporting Files

| File | Relationship |
|------|-------------|
| `corporate/business/app/daily_rpt.aspx[.cs]` | Navigation target — calendar click redirects here for visit creation |
| `corporate/business/app/expense_entry.aspx[.cs]` | Linked from Executed Visit Details modal (`?visitId=`) |
| `corporate/business/app/Create_quotation.aspx[.cs]` | Linked from Executed Visit Details modal (`?visitId=`) |
| `DB_UTILITY.cs` | Database connection utilities |

---

## 3. Core Database Tables

| Table | Usage in This Module |
|-------|---------------------|
| `tbl_SalesVisitReport` | Primary entity — `SELECT` (calendar events, visit details), `UPDATE` (execute planned visit), `INSERT` (auto-follow-up) |
| `tbl_login` | User identity resolution (for `CreatedByCode` filter) |

### Key Columns Referenced

| Column | Read/Write | Purpose |
|--------|-----------|---------|
| `Id` | Read/Write | Visit PK — used in calendar event IDs, detail fetches, execute updates |
| `VisitDate` | Read/Write | Planned visit date — drives calendar positioning and edit-lock age calculation |
| `VisitEndDate` | Read/Write | Visit end time |
| `CustomerName` | Read | Display in calendar events and detail modal |
| `Department` | Read | Detail modal |
| `ContactPerson` | Read | Detail modal |
| `VisitType` | Read | Display in calendar events and detail modal |
| `DiscussionPoints` | Read/Write | Agenda (plan mode) or Outcome (executed) |
| `VisitPhase` | Read/Write | `Planned` → `Executed` transition |
| `Status` | Read/Write | `Pending Execution` → execution status |
| `FollowUpRequired` | Read/Write | Determines if follow-up is spawned |
| `NextFollowUpDate` | Read/Write | Date for auto-generated follow-up |
| `AttachmentName` | Read/Write | Uploaded file reference |
| `ExecutionDateTime` | Write | Set to `GETDATE()` on calendar execution |
| `CreatedDate` | Write | `DateTime.Today` at creation |
| `CreatedByCode` | Write | Session `USERID` (business key) |
| `ParentVisitId` | Write | Self-referencing FK to parent visit (write-only — never read) |
| `Latitude` | Write | GPS latitude from `navigator.geolocation` |
| `Longitude` | Write | GPS longitude from `navigator.geolocation` |
| `CompanyID` | — | **Never populated by this module** (Defect D-01) |

---

## 4. Multi-Tenant Constraints

| Constraint | Status | Evidence |
|-----------|--------|----------|
| `CreatedByCode` filter on calendar events | ✅ Enforced | `GetCalendarEvents`: `WHERE CreatedByCode = @UserId` |
| `CompanyId` filter on calendar events | ❌ Not applied | Calendar events are scoped by ownership only, not by tenant |
| `CreatedByCode` filter on visit detail fetch | ❌ **NOT enforced** | `GetVisitDetails`: `WHERE Id=@Id` only — any authenticated user can view any visit (Defect D-04) |
| `CreatedByCode` filter on visit execution (UPDATE) | ❌ **NOT enforced** | `btnSubmitExecution_Click`: `UPDATE WHERE Id=@Id` only — any user can execute any visit (Defect D-04) |
| `CompanyId` on auto-follow-up INSERT | ❌ **Never set** | Follow-up row created without `CompanyID` (Defect D-01) |

### Critical Security Gaps

1. **IDOR on `GetVisitDetails`** (PageMethod): No ownership check. Any authenticated user can call this method with any visit ID and retrieve the full detail including GPS coordinates and attachment paths.
2. **IDOR on `btnSubmitExecution_Click`**: No ownership check on the `UPDATE`. A user can execute (and GPS-tag, attach files to, and change the status of) any other user's Planned visit by supplying its ID.
3. **Missing `CompanyID` on follow-up INSERT**: Auto-generated follow-up visits inherit no tenant identifier, making them invisible to the manager dashboard.

---

## 5. Proactive Notification Triggers

| Trigger | Table | Description |
|---------|-------|-------------|
| Visit execution with follow-up | `tbl_SystemNotification` | A notification entry is expected to be logged when a visit is executed and a follow-up is spawned, prior to the transaction commit. |

---

## 6. Architectural Notes

### GPS Capture Flow

1. User clicks "📍 Execute & Tag Location" button
2. Client-side `captureLocationAndSubmit()` calls `navigator.geolocation.getCurrentPosition({ enableHighAccuracy: true, timeout: 10000 })`
3. On success: `latitude`/`longitude` written to hidden fields; hidden `btnSubmitExecution` programmatically clicked
4. On failure: postback **never happens** — GPS is a hard client-side gate
5. Server-side: coordinates converted to `decimal` and written to `tbl_SalesVisitReport.Latitude`/`.Longitude`

**GPS is only captured in this calendar execution path.** Past-logged visits (`daily_rpt.aspx?mode=past`) and edits (`vw_dailyrpts.aspx`) never request or store location.

### Follow-Up Auto-Generation

When `FollowUpRequired='Yes'` and `NextFollowUpDate` is populated, the server executes a single `SqlCommand` containing:
1. `UPDATE` (execute the current visit)
2. Conditional `INSERT` (create a new `Planned` visit as a follow-up, linked via `ParentVisitId`)

These are **not wrapped in an explicit transaction** (Defect D-12) — the `UPDATE` auto-commits independently of the `INSERT`. If the `INSERT` fails after the `UPDATE` succeeds, the visit is left executed with no follow-up, and no error distinguishes this partial failure from success.

### View Modal (Executed Visit Details)

The executed-visit view modal provides:
- Read-only display of all visit fields
- Links to `expense_entry.aspx?visitId=` and `Create_quotation.aspx?visitId=`
- GPS coordinates display (but no map visualization in this modal)

---

## 7. Known Defects

| ID | Severity | Description |
|----|----------|-------------|
| D-01 | Critical | `CompanyID` never populated on follow-up INSERT |
| D-04 | Critical | IDOR — no ownership check on `GetVisitDetails` or `btnSubmitExecution_Click` |
| D-06 | Medium | `ParentVisitId` written but never read anywhere in the application |
| D-12 | Medium | Non-atomic execute + follow-up batch (no explicit transaction) |
| D-17 | Low | Follow-up auto-generation implemented in only one of three plausible trigger points |
