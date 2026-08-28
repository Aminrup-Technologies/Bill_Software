# 02 — Database Dependency Map

> **Source-of-truth caveat:** No `.sql` DDL files, EF migrations, or stored-procedure definitions exist anywhere in this repository. Every table/column/constraint listed below is **reverse-engineered from ADO.NET `SqlCommand` text** in the 8 in-scope files plus a small number of cross-referenced files (`Create_quotation.aspx.cs`, `AdminAttendanceDashboard.aspx.cs`, `home.aspx.cs`, `AddUser.aspx.cs`, `index.aspx.cs`) needed to disambiguate columns. **Primary keys, foreign keys, indexes, and constraints are inferred from usage patterns only** — none are declared anywhere in code, because SQL Server enforces them out-of-band. Triggers: **none observed or referenced** anywhere in the analyzed code (no `INSTEAD OF`/`AFTER` trigger names, no evidence of trigger-driven side effects). Stored procedures: **none used by this workflow** (the only SP found in the whole codebase, `sp_AllocateEmployeeLeaves`, belongs to the unrelated user-provisioning flow in `AddUser.aspx.cs`).

---

## B.1 Tables Used by This Workflow

### `tbl_SalesVisitReport` (core entity)

Columns observed in use **within the 8 in-scope files**:

| Column | Inferred Type | Observed From | Notes |
|---|---|---|---|
| `Id` | `int` (identity, PK-like) | all files | Used everywhere as `WHERE Id=@Id`; `DataKeyNames="Id"` on `gvSalesVisits`/`gvMegaExpenses` |
| `VisitDate` | `datetime` | all | Planned start time; also the "age" anchor for edit-lock logic |
| `VisitEndDate` | `datetime`, nullable | `visit_planner.aspx.cs`, `daily_rpt.aspx.cs` | Only recently added (see file header comment in `visit_planner.aspx.cs`); not present in every `SELECT`/`UPDATE` (e.g. `vw_dailyrpts` edit UPDATE never touches it) |
| `Salesperson` | `nvarchar` | `daily_rpt.aspx.cs` | Denormalized **display name**, copied at insert time from `tbl_login.Name` — not kept in sync if the user's name later changes |
| `CustomerName` | `nvarchar` | all | |
| `Department` | `nvarchar` | all | |
| `ContactPerson` | `nvarchar` | all | |
| `VisitType` | `nvarchar` | all | Values constrained only by UI dropdown: `Office Visit`, `Plant Visit`, `Online Meeting` |
| `DiscussionPoints` | `nvarchar(max)`-like | all | Free text; doubles as "Agenda" (plan mode) or "Outcome" (executed) |
| `VisitPhase` | `nvarchar` | all | `Planned` \| `Executed` — see `03_State_Machine.md` |
| `Status` | `nvarchar` | all | `Pending Execution` \| `Completed` \| `Pending` \| `Escalated` — **inconsistent vocabulary**, see `03_State_Machine.md` |
| `FollowUpRequired` | `nvarchar` | all | `Yes` \| `No` \| `''` (empty string used as a third "not applicable yet" state) |
| `NextFollowUpDate` | `datetime`, nullable | all | |
| `AttachmentName` | `nvarchar`, nullable | all | Stored filename only; see `07_...` / Section G |
| `ExecutionDateTime` | `datetime`, nullable | all | Set to `GETDATE()` (calendar-execute) or to the *declared visit start* (past-mode log) |
| `CreatedDate` | `datetime`/`date` | `daily_rpt.aspx.cs`, `visit_planner.aspx.cs` | Inconsistently populated with `DateTime.Today` in one path and `GETDATE()` in another |
| `CreatedByCode` | `nvarchar` (FK → `tbl_login.User_Id`) | all | The tenancy/ownership key used by salesperson-facing pages |
| `ParentVisitId` | `int`, nullable (self-referencing FK → `tbl_SalesVisitReport.Id`) | `visit_planner.aspx.cs` (write-only) | Set only on system-generated follow-up rows; **never read/selected anywhere** in the analyzed code — see `05_Potential_Defects.md` D-06 |
| `Latitude` | `decimal`, nullable | `visit_planner.aspx.cs`, `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs` | GPS — see Section H |
| `Longitude` | `decimal`, nullable | same | GPS — see Section H |
| `ApprovalStatus` | `nvarchar` | `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs` | `Pending` \| `Approved` \| `Rejected` (default value not declared in any INSERT observed — see `06_Business_Rules_Requiring_Confirmation.md`) |
| `ManagerRemarks` | `nvarchar`, nullable | `srch_dailyrpts.aspx.cs` | |
| `ApprovedDate` | `datetime`, nullable | `srch_dailyrpts.aspx.cs` | |
| `ApprovedBy` | `nvarchar` (FK → `tbl_login.User_Id`) | `srch_dailyrpts.aspx.cs`, `vw_dailyrpts.aspx.cs` (join for display) | |
| `CompanyID` | `int` (FK → tenant/company table, not in scope) | `srch_dailyrpts.aspx.cs`, and cross-file: `Create_quotation.aspx.cs`, `AdminAttendanceDashboard.aspx.cs` | **Never populated by any `INSERT` statement found anywhere in the repository** — see `04_Security_and_Tenant_Audit.md` and `05_Potential_Defects.md` D-01 |

Additional columns discovered **only via cross-file references** (confirms the table is wider than what these 8 files touch; listed for completeness of the dependency map):
`LinkedQuotationNo`, `RevenueRealized`, `GeoLocationAddress`, `IsProductive` (`home.aspx.cs`, `AdminAttendanceDashboard.aspx.cs`).

### `tbl_SalesVisitResponses` (manager/salesperson chat)

| Column | Inferred Type | Notes |
|---|---|---|
| `Id` | `int` (identity, PK-like) | Not explicitly selected but implied |
| `VisitId` | `int` (FK → `tbl_SalesVisitReport.Id`) | |
| `RespondentRole` | `nvarchar` | `Manager` \| `Salesperson` |
| `RespondentCode` | `nvarchar` (FK → `tbl_login.User_Id`) | |
| `ResponseText` | `nvarchar(max)`-like | |
| `ResponseDate` | `datetime` | Always `GETDATE()` at insert |

### `tbl_Expenses`

| Column | Inferred Type | Notes |
|---|---|---|
| `Id` | `int` (identity, PK) | Used as `DataKeyNames="Id"` in `srch_dailyrpts.aspx` grid and in `UPDATE ... WHERE Id=@Id` |
| `UserCode` | `nvarchar` (FK → `tbl_login.User_Id`) | Set in `expense_entry.aspx.cs`, not read in the 4 in-scope files |
| `ExpenseDate` | `date`/`datetime` | |
| `VisitId` | `int`, nullable (FK → `tbl_SalesVisitReport.Id`) | Nullable — expenses can exist independent of any visit |
| `ExpenseCategory` | `nvarchar` | |
| `Amount` | `decimal`/`money` | |
| `Description` | `nvarchar` | |
| `AttachmentName` | `nvarchar`, nullable | Stored under `~/Uploads/Expenses/` |
| `ApprovalStatus` | `nvarchar` | `Pending` \| `Approved` \| `Rejected` |
| `ApprovedBy` | `nvarchar` (FK → `tbl_login.User_Id`) | |
| `ApprovedDate` | `datetime`, nullable | |
| `CreatedDate` | `datetime` | `GETDATE()` at insert (in `expense_entry.aspx.cs`) |

### `tbl_login` (identity/user directory — read-heavy dependency of this workflow)

| Column | Inferred Type | Notes |
|---|---|---|
| `Id` | `int` (identity, PK) | |
| `User_Id` | `nvarchar` (**natural/business key**, unique by convention, not declared) | The value stored in `Session["USERID"]` and used as the FK target for `CreatedByCode`, `RespondentCode`, `ApprovedBy`, and `ReportingManagerId` |
| `Name` | `nvarchar` | Denormalized into `tbl_SalesVisitReport.Salesperson` at insert time |
| `Email` | `nvarchar`, nullable | Used for all outbound notification routing |
| `Phone_no` | `nvarchar` | Not used by this workflow |
| `Password` / `PasswordHash` / `PasswordSalt` | legacy plaintext / `varbinary` | Not used by this workflow (see `index.aspx.cs`) |
| `IsActive` | `bit` | Filters the manager's salesperson dropdown |
| `CompanyID` | `int` | Tenant key — used to scope the salesperson dropdown and (elsewhere) user provisioning |
| `RoleId`, `DepartmentID`, `DesignationID` | `int` (FKs) | Not used by this workflow |
| `ReportingManagerId` | `nvarchar` (**self-referencing FK → `tbl_login.User_Id`**, NOT `Id`) | Used **exclusively for email-routing lookups** in this workflow (`LEFT JOIN tbl_login Manager ON Manager.User_Id = Creator.ReportingManagerId`) — never used for access control (see `04_Security_and_Tenant_Audit.md`) |

---

## B.2 Foreign Keys (inferred; not declared anywhere in code/DDL)

| FK (child.column → parent.column) | Evidence |
|---|---|
| `tbl_SalesVisitReport.CreatedByCode → tbl_login.User_Id` | Every join/lookup uses `User_Id`, e.g. `srch_dailyrpts.aspx.cs` `GetUserRole`, email routing joins |
| `tbl_SalesVisitReport.ApprovedBy → tbl_login.User_Id` | `GetVisitEmailBody`: `LEFT JOIN tbl_login mgr ON v.ApprovedBy = mgr.User_Id` |
| `tbl_SalesVisitReport.ParentVisitId → tbl_SalesVisitReport.Id` | `visit_planner.aspx.cs` INSERT sets `ParentVisitId=@Id` (self-reference) |
| `tbl_SalesVisitReport.CompanyID → <Company table, out of scope>` | `srch_dailyrpts.aspx.cs`, `Create_quotation.aspx.cs`, `AdminAttendanceDashboard.aspx.cs` |
| `tbl_SalesVisitResponses.VisitId → tbl_SalesVisitReport.Id` | All chat queries |
| `tbl_SalesVisitResponses.RespondentCode → tbl_login.User_Id` | `INNER JOIN tbl_login u ON r.RespondentCode = u.User_Id` |
| `tbl_Expenses.VisitId → tbl_SalesVisitReport.Id` | `expense_entry.aspx.cs`, `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs` |
| `tbl_Expenses.UserCode → tbl_login.User_Id` | `expense_entry.aspx.cs` insert |
| `tbl_login.ReportingManagerId → tbl_login.User_Id` (self-referencing) | Email routing joins in `vw_dailyrpts.aspx.cs` / `srch_dailyrpts.aspx.cs` |
| `tbl_login.RoleId / DepartmentID / DesignationID → Roles / tbl_Departments / tbl_Designations` | `AddUser.aspx.cs` (out of scope but confirms schema) |

**No FK is ever declared as `NOT NULL` with certainty from code** — nullability is inferred only from `DBNull.Value` checks (`rdr["X"] != DBNull.Value`) present in the read paths for: `VisitEndDate`, `ExecutionDateTime`, `NextFollowUpDate`, `AttachmentName`, `Latitude`, `Longitude`, `ApprovedDate`.

## B.3 Indexes / Constraints / Triggers

- **Indexes:** none discoverable — no `.sql`/index-hint code found. Given `Id` lookups dominate (`WHERE Id=@Id`) and `CreatedByCode`/`CompanyID`/`VisitDate` are filtered heavily, a DBA-side index review is recommended but out of scope for this static analysis.
- **Constraints (CHECK/DEFAULT/UNIQUE):** none discoverable. The multiple free-text status columns (`Status`, `VisitPhase`, `ApprovalStatus`, `FollowUpRequired`) are constrained **only by UI dropdown lists**, not by database `CHECK` constraints — the application can and does write values that bypass one page's dropdown vocabulary but not another's (see `03_State_Machine.md`).
- **Triggers:** none referenced or implied anywhere in the analyzed code.
- **Stored Procedures:** none used in this workflow. All data access is inline ad-hoc SQL text via `System.Data.SqlClient.SqlCommand`, executed either as parameterized commands (most paths) or as raw string concatenation (`srch_dailyrpts.aspx.cs :: Binder()`, `daily_rpt.aspx.cs :: GetAdminName()` — see `05_Potential_Defects.md`).

## B.4 Cross-Feature Dependents of `tbl_SalesVisitReport` (outside the 8 in-scope files, discovered during traceability)

| File | Dependency | Purpose |
|---|---|---|
| `expense_entry.aspx.cs` | `SELECT CustomerName, VisitDate, DiscussionPoints FROM tbl_SalesVisitReport WHERE Id=@Id` | Pre-fills the linked-visit banner when adding an expense from a visit |
| `Create_quotation.aspx.cs` | `SELECT CustomerName FROM tbl_SalesVisitReport WHERE Id=@Id AND CompanyID=@CompanyID` | Resolves customer name for quote generation, **tenant-scoped** (contrast with in-scope files, which are not) |
| `home.aspx.cs` | Aggregates `COUNT(Id)`, `SUM(RevenueRealized)` `WHERE CreatedByCode=@UserId` | Dashboard "Today"/"This Month" sales KPIs |
| `AdminAttendanceDashboard.aspx.cs` | Aggregates `FieldSales` CTE `WHERE CompanyID=@CompanyID` | Company-wide attendance/field-sales rollup, also selects `GeoLocationAddress`, `IsProductive`, `RevenueRealized` |

These external dependents establish that `tbl_SalesVisitReport.CompanyID` is a **load-bearing column for other, already-shipped features** — reinforcing why its omission from every `INSERT` in the in-scope workflow (Section B.1, and detailed in `05_Potential_Defects.md` D-01) is a serious cross-cutting defect rather than a cosmetic one.

---

## C. Entity Relationships

### C.1 ASCII Entity-Relationship Diagram (inferred)

```
tbl_login (User Directory / Identity)
 ├─ Id (PK, int identity)
 ├─ User_Id (natural/business key, used as FK target everywhere below)
 ├─ Name, Email, Phone_no, CompanyID, IsActive, RoleId, DepartmentID, DesignationID
 └─ ReportingManagerId  ──self-FK──▶  tbl_login.User_Id   (manager hierarchy; used for e-mail routing ONLY)
        │
        │  User_Id is referenced as a foreign key by:
        │
        ├──▶ tbl_SalesVisitReport.CreatedByCode   (who created/owns the visit — "salesperson")
        ├──▶ tbl_SalesVisitReport.ApprovedBy      (which manager approved/rejected it)
        ├──▶ tbl_SalesVisitResponses.RespondentCode (who wrote a chat message)
        └──▶ tbl_Expenses.UserCode / tbl_Expenses.ApprovedBy

tbl_SalesVisitReport (core visit entity)
 ├─ Id (PK, int identity)
 ├─ ParentVisitId  ──self-FK──▶  tbl_SalesVisitReport.Id   (follow-up chain, write-only — see 03_State_Machine.md §I)
 ├─ CompanyID  ──FK──▶  <Company table, out of scope>       (tenant boundary — NEVER populated by this workflow's INSERTs)
 ├─ CreatedByCode  ──FK──▶  tbl_login.User_Id                (ownership)
 ├─ ApprovedBy     ──FK──▶  tbl_login.User_Id                (nullable until actioned)
 ├─ VisitPhase, Status, ApprovalStatus, FollowUpRequired      (independent free-text state columns — see 03_State_Machine.md)
 └─ Latitude / Longitude (nullable, populated only on calendar-driven execution)
        │
        │  Id is referenced as a foreign key by:
        │
        ├──▶ tbl_SalesVisitResponses.VisitId   (1 visit : N chat messages)
        ├──▶ tbl_Expenses.VisitId              (1 visit : N expenses, nullable — expenses can be visit-less)
        └──▶ tbl_SalesVisitReport.ParentVisitId (1 visit : N generated follow-up visits)

tbl_SalesVisitResponses (chat / manager-salesperson conversation thread)
 ├─ Id (PK, int identity)
 ├─ VisitId        ──FK──▶  tbl_SalesVisitReport.Id
 ├─ RespondentCode ──FK──▶  tbl_login.User_Id
 └─ RespondentRole  (free text: 'Manager' | 'Salesperson' — NOT derived from a role table, just an inferred/typed label; see 04_Security_and_Tenant_Audit.md)

tbl_Expenses (expense claims, may or may not be linked to a visit)
 ├─ Id (PK, int identity)
 ├─ VisitId   ──FK──▶  tbl_SalesVisitReport.Id  (nullable)
 ├─ UserCode  ──FK──▶  tbl_login.User_Id
 └─ ApprovedBy ──FK──▶  tbl_login.User_Id (nullable)
```

### C.2 Relationship Details Requested

- **PK:** `tbl_SalesVisitReport.Id`, `tbl_SalesVisitResponses.Id` (implied), `tbl_Expenses.Id`, `tbl_login.Id` — all inferred `int` identity columns based on `WHERE Id=@Id` single-row lookup patterns and `DataKeyNames="Id"` GridView bindings. **None of these are declared as PK in any code artifact**; this is standard SQL Server DDL knowledge that is absent from the repository.
- **FK:** see table in Section B.2 above and the diagram in C.1.
- **`ParentVisitId` relationship:** self-referencing FK on `tbl_SalesVisitReport` (`Id ← ParentVisitId`). It is a **1-parent : N-children** relationship (a visit can theoretically spawn only one *direct* child per execution event, but a child can itself become a parent when it, too, is executed with `FollowUpRequired='Yes'`, forming a linked list / chain rather than a tree with multiple children per node under the current code). **Critically, no query in the entire analyzed workflow ever `SELECT`s `ParentVisitId`** — the relationship is persisted on write but has no corresponding read/report/UI surface. See `05_Potential_Defects.md` D-06.
- **`VisitId` relationships:** `tbl_SalesVisitResponses.VisitId` and `tbl_Expenses.VisitId` both point to `tbl_SalesVisitReport.Id`, forming two independent 1:N child collections of a visit (chat thread, expense list). Both are always looked up by exact `Id` match with **no ownership or tenant filter** on the child rows themselves (they inherit their effective security boundary — such as it is — solely from whatever filtered the *parent* visit list, which is inconsistent across pages; see `04_Security_and_Tenant_Audit.md`).
- **`CompanyID` relationships:** appears on `tbl_login` (each user belongs to exactly one company) and, per cross-file evidence (`Create_quotation.aspx.cs`, `AdminAttendanceDashboard.aspx.cs`), on `tbl_SalesVisitReport` as well. The intended relationship is "a visit's `CompanyID` should match its creator's `tbl_login.CompanyID`" — but because no `INSERT` into `tbl_SalesVisitReport` in this workflow ever sets `CompanyID`, this relationship is **effectively broken/unenforced** for every row created through `daily_rpt.aspx` or the calendar auto-follow-up path.
- **`CreatedByCode` relationships:** the primary ownership FK (`tbl_SalesVisitReport.CreatedByCode → tbl_login.User_Id`). Used consistently as the "self-service" filter in `visit_planner.aspx` and `vw_dailyrpts.aspx`. Also used to denormalize the `Salesperson` display name at write time (via a separate lookup in `daily_rpt.aspx.cs :: GetAdminName()`), and to infer chat-role (`srch_dailyrpts.aspx.cs :: GetUserRole`, by equality-testing the sender's user code against the visit's `CreatedByCode`).
- **Manager relationships:** modeled by `tbl_login.ReportingManagerId → tbl_login.User_Id` (self-referencing). In this workflow it is used **exclusively to resolve an e-mail address** for notification routing (`LEFT JOIN tbl_login Manager ON Manager.User_Id = Creator.ReportingManagerId`, present in `vw_dailyrpts.aspx.cs :: SendChatEmailNotification` and `srch_dailyrpts.aspx.cs :: SendChatEmailNotification`). **It is never used to gate who is allowed to view, comment on, approve, or reject a given salesperson's visit** — any user who can reach `srch_dailyrpts.aspx` can act on any visit in the same `CompanyID`, regardless of the actual reporting line. See `04_Security_and_Tenant_Audit.md` for the full authorization-gap analysis.
