# 07 — Proposed Target Architecture (Sales Visit Workflow)

**Status: PROPOSAL ONLY. READ-ONLY DELIVERABLE.**
No source code, database object, or configuration has been changed as part of producing this document. No SQL migrations are included or implied to be ready-to-run. This document is an input to a future planning/implementation decision, not an implementation.

## 0. Preconditions and Caveats (read first)

- **DDL not available in this session.** The task requested this proposal be based on "the confirmed SQL Server DDL" for `dbo.tbl_SalesVisitReport`, `dbo.tbl_Expenses`, `dbo.tbl_login`, and `dbo.tbl_SalesVisitResponses`. No `.sql`/DDL file exists anywhere in this repository (confirmed by repository-wide search), and no DDL text was supplied in this conversation. **This proposal is therefore built entirely on the schema as inferred from application code in `02_Database_Dependency_Map.md`** (column names/usage observed in `SELECT`/`INSERT`/`UPDATE` statements), not on an authoritative DDL export.
- Every place where an actual DDL fact (exact data type, nullability, default constraint, existing index, existing FK/CHECK constraint) would materially change a recommendation is explicitly marked **`ASSUMPTION`** below. **Before implementing anything in this document, the actual DDL should be pulled from SQL Server (e.g. via `sp_help`, SSMS "Script Table as CREATE", or `INFORMATION_SCHEMA`) and reconciled against these assumptions.**
- This document **proposes** designs and sequencing. It does not implement, migrate, or configure anything. Per task instructions, no business requirement is invented — every place a business decision is required, it is listed as an open decision (Section 2) rather than resolved unilaterally.
- All "proposed behavior" descriptions below are **candidate designs for stakeholder review**, not committed direction.

---

## 1. Confirmed Technical Defects

This is a consolidated, architecture-planning-oriented restatement of the defects already fully detailed (with file/method/code-area/DB-object/impact/confidence) in `05_Potential_Defects.md`. Refer to that document for full evidence; this section exists to show which proposed changes (Sections 10–12) each defect maps to.

| ID | Defect | Full detail | Addressed by proposed change(s) |
|---|---|---|---|
| D-01 | `CompanyID` never populated on `tbl_SalesVisitReport` INSERT | `05_Potential_Defects.md` D-01 | §6 (tenant isolation), §12 PR-2 |
| D-02 | `Status` vocabulary mismatch causes silent data loss on edit | `05_Potential_Defects.md` D-02 | §5 (state matrix), §9 (validation), §12 PR-4 |
| D-03 | Hardcoded `"FLM03"` fallback misattributes ownership | `05_Potential_Defects.md` D-03 | §10, §12 PR-1 |
| D-04 | Broken access control (IDOR) across nearly every detail/mutation endpoint | `05_Potential_Defects.md` D-04 | §4 (authz matrix), §10, §12 PR-3 |
| D-05 | SQL injection in `srch_dailyrpts.aspx.cs :: Binder()` (+ minor instance in `GetAdminName`) | `05_Potential_Defects.md` D-05 | §10, §12 PR-1 (highest priority) |
| D-06 | `ParentVisitId` written but never read | `05_Potential_Defects.md` D-06 | §7 (follow-up lifecycle), §12 PR-6 |
| D-07 | Missing recipient-email validation / drifted chat-notification logic | `05_Potential_Defects.md` D-07 | §11 (refactor boundary: `NotificationService`), §12 PR-5 |
| D-08 | No idempotency/concurrency guard on visit approval | `05_Potential_Defects.md` D-08 | §5, §8 (approval lifecycle), §12 PR-4 |
| D-09 | Unrestricted file upload + unauthenticated static retrieval | `05_Potential_Defects.md` D-09 | §10, §12 PR-7 |
| D-10 | Notification failures always swallowed silently | `05_Potential_Defects.md` D-10 | §11, §12 PR-5 |
| D-11 | Hardcoded, source-committed SMTP credentials | `05_Potential_Defects.md` D-11 | §10, §12 PR-5 (**credential rotation is an operational action outside this repo's change process — flagged, not performed here**) |
| D-12 | Non-atomic two-statement execute+follow-up batch | `05_Potential_Defects.md` D-12 | §7, §12 PR-6 |
| D-13 | No server-side re-validation of required fields | `05_Potential_Defects.md` D-13 | §9, §12 PR-4 |
| D-14 | Raw exception messages surfaced to end users | `05_Potential_Defects.md` D-14 | §10, §11, §12 PR-8 |
| D-15 | Two incompatible tenancy philosophies in one workflow | `05_Potential_Defects.md` D-15 | §6, §12 PR-2/PR-3 |
| D-16 | `ReportingManagerId` used only for routing, never authorization | `05_Potential_Defects.md` D-16 | §4, depends on Decision #8 (§2) |
| D-17 | Follow-up auto-generation implemented in only one of three plausible trigger points | `05_Potential_Defects.md` D-17 | §7, depends on Decision #6 (§2) |
| D-18 | Duplicated/drifted business logic between `vw_dailyrpts.aspx.cs` and `srch_dailyrpts.aspx.cs` | `05_Potential_Defects.md` D-18 | §11 |
| D-19 | Three data-entry surfaces define overlapping fields with different vocabularies | `05_Potential_Defects.md` D-19 | §5, §9, §11 |

**No new defects are introduced in this document.** Section 1 exists purely to link each already-confirmed defect to the part of the proposal that addresses it, so that reviewers can trace remediation coverage.

---

## 2. Business-Rule Decisions Required

These are the open questions from `06_Business_Rules_Requiring_Confirmation.md`, restated as **decisions that block specific parts of the proposed architecture below**. Nothing in this document resolves them — each is referenced by number from the parts of Sections 3–12 that depend on it. **No proposed change in this document assumes a specific answer beyond what is explicitly marked `ASSUMPTION` (and each such assumption is the conservative/least-behavior-changing option, clearly reversible once the real decision is made).**

| # | Decision | Blocks |
|---|---|---|
| 1 | Is GPS capture mandatory for all executions, or only calendar-driven ones? | §7 (follow-up lifecycle unification touches the same execute pathway), §9 (validation rules for `daily_rpt.aspx` past-mode) |
| 2 | Is the 45-day edit-lock meant to key off `VisitDate` only, or should stale `Planned` visits also lock/expire? | §5 (state matrix), §9 |
| 3 | Should Manager Remarks be mandatory before Approve/Reject? | §8 (expense/visit approval lifecycle), §9 |
| 4 | Was decoupling expense approval from visit approval intentional? | §8 |
| 5 | What is the intended terminal state for a `Planned` visit that's never executed? | §5 (whether a new `Expired`/`Cancelled` state is needed) |
| 6 | Should follow-up auto-generation apply uniformly across all three write paths? | §7 |
| 7 | Is the three-valued `FollowUpRequired` domain (`''`/`'No'`/`'Yes'`) intentional? | §5, §9 |
| 8 | Should `ReportingManagerId` gate manager authorization (restrict a manager to only their direct reports), or is company-wide manager access intentional? | §4 (authorization matrix — this is the single most consequential open decision, since it determines whether D-04/D-16 remediation is "restrict to CompanyID" or "restrict to direct reports") |
| 9 | Is cross-company access ever intended for any role (e.g. an HQ/super-admin role not visible in these 8 files)? | §4, §6 |
| 10 | What is the intended semantic difference between `VisitDate` and `ExecutionDateTime` for past-logged visits? | §5, §9 |

**These decisions should be obtained from a product owner/business stakeholder before any implementation PR in §12 that depends on them is started.** Where a proposed change below cannot avoid taking a position pending an answer, it is marked `ASSUMPTION` with the specific fallback chosen and why it is the lowest-risk placeholder.

---

## 3. Recommended Architecture

### 3.1 Current-state characterization (from the audit)

The workflow is implemented as four largely independent ASP.NET WebForms code-behind files, each directly opening `SqlConnection`/`SqlCommand` against `tbl_SalesVisitReport`/`tbl_Expenses`/`tbl_SalesVisitResponses`/`tbl_login`, with:
- No shared data-access layer (each file writes its own SQL text).
- No shared authorization/policy layer (each file's `Page_Load` only checks `Session["USERID"] != null`; nothing else is centralized).
- No shared validation layer (each `.aspx.cs` re-implements its own required-field checks, inconsistently, and mostly client-side only).
- No shared notification/email layer (`vw_dailyrpts.aspx.cs` and `srch_dailyrpts.aspx.cs` each carry their own near-duplicate `SendChatEmailNotification`/`GetVisitEmailBody`).
- No shared file-storage abstraction (each upload site repeats the same `Path.GetFileName` + timestamp-prefix pattern inline).
- Status/vocabulary constants (`"Planned"`, `"Executed"`, `"Pending"`, `"Pending Execution"`, `"Yes"/"No"/""`) are hardcoded as string literals scattered across markup dropdown items and code-behind parameter assignments, with no single canonical source.

### 3.2 Target-state principles (proposed, non-invasive to the WebForms hosting model)

The recommendation is **not** a rewrite/replatform. It is a set of **extracted, shared, testable components** that the existing four `.aspx.cs` files call into, so that:

1. **A single place decides "can this user see/act on this visit?"** — a `SalesVisitAuthorizationPolicy` (or similarly named) class, consulted by every page before any read or write of a specific `Id`.
2. **A single place builds and executes the visit/expense/response queries**, always parameterized, always including the resolved tenant/ownership predicate — a `SalesVisitRepository` (and `ExpenseRepository`, `SalesVisitResponseRepository`).
3. **A single place owns the state-vocabulary constants** (`VisitPhase`, `Status`, `ApprovalStatus`, `FollowUpRequired` allowed values) — e.g. a `SalesVisitStatusCodes` static class — referenced by both server-side code and (via a shared data source, e.g. one dropdown-population helper) the `.aspx` markup, eliminating the D-02/D-19 vocabulary drift at the source.
4. **A single place sends notification email** — a `SalesVisitNotificationService`, reading SMTP settings from configuration (not hardcoded), with structured logging of failures (addressing D-10 without necessarily surfacing failures to the end user, pending Decision-adjacent UX input).
5. **A single place handles file upload/retrieval** — a `VisitAttachmentStorageService` enforcing an extension allow-list and routing retrieval through an authenticated handler rather than a raw static path.
6. **A single place implements follow-up generation**, invoked identically regardless of which page/mode triggered the "visit executed with follow-up requested" event (subject to Decision #6).

This is an incremental, in-place refactor strategy: each of the four `.aspx.cs` files keeps its existing UI/postback structure but delegates data access, authorization, validation, notification, and status-vocabulary concerns to shared classes. This minimizes regression risk relative to a full rewrite while directly eliminating the duplication (D-18) and drift (D-02, D-07, D-19) findings.

### 3.3 High-Level Component Diagram (proposed)

```
                     ┌─────────────────────────────────────────────┐
                     │        ASP.NET WebForms Pages (existing)      │
                     │  visit_planner.aspx / daily_rpt.aspx /        │
                     │  vw_dailyrpts.aspx / srch_dailyrpts.aspx /    │
                     │  expense_entry.aspx                           │
                     └───────────────┬───────────────────────────────┘
                                      │  (calls into, does not bypass)
        ┌─────────────────┬──────────┼───────────┬───────────────────┬──────────────────┐
        ▼                 ▼          ▼           ▼                   ▼                  ▼
┌───────────────┐ ┌──────────────┐ ┌───────────┐ ┌────────────────┐ ┌────────────────┐ ┌───────────────┐
│ SalesVisit     │ │ SalesVisit   │ │ SalesVisit│ │ SalesVisit     │ │ VisitAttachment│ │ SalesVisit    │
│ AuthorizationPolicy│ │ Repository │ │StatusCodes│ │ NotificationSvc│ │ StorageService │ │ ValidationSvc │
└───────────────┘ └──────────────┘ └───────────┘ └────────────────┘ └────────────────┘ └───────────────┘
        │                 │                              │                   │
        ▼                 ▼                              ▼                   ▼
   Session/Company   tbl_SalesVisitReport          SMTP (config-driven)   ~/Uploads/*  (served via
   Context resolution tbl_Expenses                                        authenticated handler,
                     tbl_SalesVisitResponses                              not raw static link)
                     tbl_login
```

No new services/processes are introduced (no microservices, no new hosting model) — these are proposed **in-process C# classes** within the same ASP.NET application, callable from the existing pages.

---

## 4. Proposed Authorization Matrix

This matrix is the direct remediation target for D-04 and D-16, and is **explicitly contingent on Decision #8 (§2)**. Two variants are given because the correct one cannot be chosen without that business decision; both are shown so the eventual choice only changes the `ReportingManagerId` column, not the rest of the design.

### 4.1 Actions inventory

| Action | Current entry point(s) |
|---|---|
| View own visit list | `visit_planner.aspx :: GetCalendarEvents`, `vw_dailyrpts.aspx :: BindSalesVisits` |
| View any visit detail ("mega modal" / view modal) | `visit_planner.aspx :: GetVisitDetails`, `vw_dailyrpts.aspx :: LoadMegaModal`, `srch_dailyrpts.aspx :: LoadMegaModal` |
| Create a new visit (plan or past) | `daily_rpt.aspx :: btnSubmit_Click` |
| Execute a Planned visit (calendar flow) | `visit_planner.aspx :: btnSubmitExecution_Click` |
| Edit visit details (post-creation) | `vw_dailyrpts.aspx :: btnUpdateVisit_Click` |
| Post a chat message | `vw_dailyrpts.aspx :: btnMegaSendChat_Click`, `srch_dailyrpts.aspx :: btnMegaSendChat_Click` |
| Approve / Reject a visit | `srch_dailyrpts.aspx :: ProcessApproval` |
| Approve / Reject an individual expense | `srch_dailyrpts.aspx :: gvMegaExpenses_RowCommand` |
| Search across all salespeople's visits | `srch_dailyrpts.aspx :: Binder` |

### 4.2 Proposed matrix — Variant A (`ASSUMPTION`: manager access is CompanyID-wide, matching the current list-query scope; this is the **lowest-behavior-change** option since it matches what `srch_dailyrpts.aspx`'s existing list query already does)

| Action | Salesperson (owner) | Salesperson (non-owner, same company) | Manager (any, same company) | Manager (different company) |
|---|:---:|:---:|:---:|:---:|
| View own visit list | ✅ | n/a | n/a | n/a |
| View any visit detail | ✅ (own only) | ❌ | ✅ (same `CompanyID` only) | ❌ |
| Create a new visit | ✅ (as self) | n/a | n/a | n/a |
| Execute a Planned visit | ✅ (own only) | ❌ | ❌ | ❌ |
| Edit visit details | ✅ (own only, subject to edit-lock, §5/§J) | ❌ | ❌ | ❌ |
| Post a chat message | ✅ (own visit only) | ❌ | ✅ (same `CompanyID` only) | ❌ |
| Approve / Reject a visit | ❌ | ❌ | ✅ (same `CompanyID` only) | ❌ |
| Approve / Reject an expense | ❌ | ❌ | ✅ (same `CompanyID`, and only for expenses linked to a visit in that company) | ❌ |
| Search across all visits | ❌ | ❌ | ✅ (same `CompanyID` only) | ❌ |

### 4.3 Proposed matrix — Variant B (if Decision #8 resolves to "manager access should be restricted to direct reports")

Identical to Variant A except every "✅ (same `CompanyID` only)" cell for a Manager becomes **"✅ (same `CompanyID` **AND** target visit's `CreatedByCode` resolves to a `tbl_login` row whose `ReportingManagerId = <acting manager's User_Id>`)"**. This requires no schema change (the `ReportingManagerId` column already exists per `02_Database_Dependency_Map.md`), only an additional join/predicate in each manager-facing query — see §12 PR-3.

### 4.4 Mapping to enforcement points

| Enforcement point | Current state | Proposed state |
|---|---|---|
| `GetVisitDetails` (PageMethod) | `WHERE Id=@Id` only | Add `AND CreatedByCode=@SessionUserId` (self) OR, if a manager-view variant is ever needed here, delegate to `SalesVisitAuthorizationPolicy.CanView(userId, visitId)` |
| `vw_dailyrpts.aspx :: LoadMegaModal` | `WHERE v.Id=@Id` only | Add `AND CreatedByCode=@SessionUserId` |
| `srch_dailyrpts.aspx :: LoadMegaModal` | `WHERE Id=@Id` only | Add `AND CompanyID=@CompanyID` (Variant A) or `AND CompanyID=@CompanyID AND CreatedByCode IN (<direct reports subquery>)` (Variant B) |
| `visit_planner.aspx :: btnSubmitExecution_Click` (UPDATE) | `WHERE Id=@Id` only | Add `AND CreatedByCode=@SessionUserId` |
| `vw_dailyrpts.aspx :: btnUpdateVisit_Click` (UPDATE) | `WHERE Id=@Id AND ApprovalStatus='Pending' AND NOT EXISTS(...)` | Add `AND CreatedByCode=@SessionUserId` |
| `srch_dailyrpts.aspx :: ProcessApproval` (UPDATE) | `WHERE Id=@Id` only | Add `AND CompanyID=@CompanyID` (Variant A) or manager-hierarchy predicate (Variant B) — **and** re-add `AND ApprovalStatus='Pending'` (also fixes D-08) |
| `srch_dailyrpts.aspx :: gvMegaExpenses_RowCommand` (UPDATE) | `WHERE Id=@Id` only | Join to `tbl_SalesVisitReport` via `VisitId` and apply the same `CompanyID`/manager-hierarchy predicate |

**Affected files:** `visit_planner.aspx.cs`, `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs`.
**Affected DB tables/columns:** `tbl_SalesVisitReport.CreatedByCode`, `.CompanyID`, `.Id`; `tbl_login.User_Id`, `.ReportingManagerId`, `.CompanyID`; `tbl_Expenses.VisitId`.
**Current behavior:** no ownership/tenant predicate on any listed `WHERE` clause (D-04).
**Proposed behavior:** every listed statement gains an explicit ownership or tenant (and, pending Decision #8, manager-hierarchy) predicate.
**Regression risk:** **Medium.** Because `CompanyID` is not currently populated on any row created by this workflow (D-01), adding a `CompanyID` predicate to these queries **before** D-01 is fixed would make every existing/newly-created row invisible to the manager dashboard (a worse regression than the current IDOR). **This is why §12 sequences the D-01 fix and any historical-data backfill decision *before* the authorization-matrix enforcement PR.**
**Dependencies:** Decision #8 and #9 (§2); D-01 remediation (§12 PR-2) must land first or be backfilled; a backfill of existing `NULL`-`CompanyID` rows is a **data change**, not a schema change, and is called out separately in §12 as requiring explicit stakeholder sign-off since it touches production data (this document does not propose *how* to backfill, only that a decision is needed on whether/how to do so).
**Database modification required:** **No schema change** required for the authorization matrix itself (all referenced columns already exist per the inferred schema). A **data backfill** (not a schema change) may be required for historical rows with `NULL CompanyID` — see §12 PR-2.

---

## 5. Proposed State-Transition Matrix

### 5.1 `VisitPhase`

| From | To | Trigger | Proposed Guard (new) |
|---|---|---|---|
| *(none — creation)* | `Planned` | `daily_rpt.aspx` create, mode=plan | none beyond existing required-field validation (§9) |
| *(none — creation)* | `Executed` | `daily_rpt.aspx` create, mode=past | **ASSUMPTION** (pending Decision #1): if GPS is later made mandatory for all executions, this creation path would need a guard requiring Lat/Long; until decided, no new guard proposed here beyond existing behavior |
| `Planned` | `Executed` | `visit_planner.aspx :: btnSubmitExecution_Click` | Add ownership guard (§4.4); no change to GPS-mandatory behavior proposed pending Decision #1 |
| `Executed` | `Planned` | *(none exists; none proposed)* | N/A — no business need identified for this reversal; if ever needed, would be a new decision, not assumed here |
| `Planned` | **`Expired`/`Cancelled`** *(proposed new value, contingent on Decision #5)* | time-based (e.g. a scheduled check) or explicit user action | **Not proposed unless Decision #5 confirms a terminal state is wanted.** If confirmed, this would require a new allowed value for `VisitPhase` (or a new column) — see §5.4 for the schema-impact note. |

### 5.2 `Status`

**Proposed canonical vocabulary** (resolving D-02/D-19 by picking **one** vocabulary and mapping the others onto it — `ASSUMPTION`: standardize on the fuller phrase already used by `vw_dailyrpts.aspx`'s dropdowns, i.e. `"Pending Execution"`, `"Completed"`, `"Pending"`, `"Escalated"`, because it is the vocabulary a *stakeholder-facing* filter dropdown already exposes; the alternative — standardizing on the shorter `"Pending"` used by the execute-flow — is equally valid and should be confirmed, not assumed, before implementation):

| Value | Meaning | Valid `VisitPhase` |
|---|---|---|
| `Pending Execution` | Default at creation, not yet executed | `Planned` only |
| `Pending` *(ASSUMPTION: proposed to be renamed/merged into a single canonical value — see note above)* | Executed, outcome still open | `Executed` only |
| `Completed` | Executed, outcome resolved favorably | `Executed` only |
| `Escalated` | Executed, needs manager attention | `Executed` only |

**Proposed guard:** every dropdown (`ddlStatus`, `ddlExecStatus`, `ddlSearchStatus`, `edit_ddlStatus`) is populated from **one shared list** (`SalesVisitStatusCodes`, §3.2/§11) instead of four independently hand-typed `<asp:ListItem>` blocks, eliminating the possibility of D-02 recurring for any future status value.

### 5.3 `ApprovalStatus`

| From | To | Trigger | Proposed Guard (new) |
|---|---|---|---|
| *(implicit default)* | `Pending` | row creation | **`ASSUMPTION`**: confirm/add an explicit default value at the point of insert (currently relies on an undocumented DB-level default per `05_Potential_Defects.md` D-03's sibling note in `03_State_Machine.md` §D.3) rather than an inferred one — this is a documentation/explicitness improvement, not a behavior change, **unless** DDL review reveals there is in fact no default, in which case this becomes a required behavior fix (flagged, not assumed) |
| `Pending` | `Approved` | `ProcessApproval("Approved")` | Add `AND ApprovalStatus='Pending'` to the `WHERE` clause (fixes D-08) |
| `Pending` | `Rejected` | `ProcessApproval("Rejected")` | Add `AND ApprovalStatus='Pending'` to the `WHERE` clause (fixes D-08) |
| `Approved` | *(any)* | *(none exists; none proposed)* | Explicitly blocked by the guard above — no "re-open" path is proposed absent a business decision to add one |
| `Rejected` | *(any)* | *(none exists; none proposed)* | Same as above |

### 5.4 `FollowUpRequired`

**Contingent on Decision #7.** Two proposed options, neither assumed as the answer:
- **Option A (two-valued):** collapse `''` into `'No'` at the point of creation (`daily_rpt.aspx` plan-mode insert would write `'No'` instead of `''`). This is a **behavior change requiring the decision**, not proposed as default.
- **Option B (keep three-valued):** formally document `''` as "Not Yet Applicable" and ensure every dropdown across all three surfaces includes a matching blank/placeholder item mapped to `''` (currently `visit_planner.aspx`'s `ddlExecFollowUp` does not — see D-19), removing the *inconsistency* without removing the third state.

Neither option is implemented or chosen here; §12 sequences this work as a small, low-risk, decision-gated PR.

### 5.5 Schema-impact note (only if Decision #5 requires a new terminal `VisitPhase`/state)

If a `Cancelled`/`Expired` terminal state is confirmed as needed (Decision #5), the **lowest-risk** proposed approach (`ASSUMPTION`: to be confirmed against actual DDL/constraints) is to add a new allowed string value to the existing `VisitPhase` column (no schema change, since it is presumed to be an unconstrained `nvarchar`) rather than adding a new column — **but this cannot be confirmed without the actual DDL**, since if a `CHECK CONSTRAINT` already restricts `VisitPhase` to `('Planned','Executed')` (unknown — DDL not available), a schema modification (`ALTER TABLE ... DROP/ADD CONSTRAINT`) would be required. **This is explicitly flagged as `whether database modification is required: UNKNOWN pending DDL confirmation.`**

---

## 6. Proposed Tenant-Isolation Rules

| Rule | Current State | Proposed Rule | Affected Files | Affected DB Columns | DB Modification Required |
|---|---|---|---|---|---|
| Every `INSERT` into `tbl_SalesVisitReport` must populate `CompanyID` | Never populated (D-01) | `CompanyID` is resolved from `CompanyContext.CurrentCompanyID` (already used elsewhere in the app, e.g. `srch_dailyrpts.aspx.cs`, `AddUser.aspx.cs`) and included in every `INSERT`, including the auto-follow-up `INSERT` inside `btnSubmitExecution_Click` | `daily_rpt.aspx.cs`, `visit_planner.aspx.cs` | `tbl_SalesVisitReport.CompanyID` | **No** (column already exists per inferred schema) — but see backfill note below |
| Every `SELECT`/`UPDATE` against a single `tbl_SalesVisitReport.Id` must be scoped by an ownership or tenant predicate | Never applied (D-04) | See §4.4 enforcement-point table | `visit_planner.aspx.cs`, `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs` | `tbl_SalesVisitReport.CreatedByCode`, `.CompanyID` | **No** |
| `tbl_Expenses` actions must inherit their parent visit's tenant scope | Never applied | Every `tbl_Expenses` query/update that is reached from a visit context should join to `tbl_SalesVisitReport` and apply the same `CompanyID`/ownership predicate as the parent visit action | `srch_dailyrpts.aspx.cs`, `vw_dailyrpts.aspx.cs`, `expense_entry.aspx.cs` (out of original scope but shares the pattern) | `tbl_Expenses.VisitId`, joined to `tbl_SalesVisitReport.CompanyID`/`.CreatedByCode` | **No** |
| `tbl_SalesVisitResponses` (chat) actions must inherit their parent visit's tenant scope | Never applied | Same join-and-scope pattern as expenses | `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs` | `tbl_SalesVisitResponses.VisitId`, joined to `tbl_SalesVisitReport` | **No** |
| Historical rows with `NULL`/missing `CompanyID` | N/A (defect, not yet remediated) | **Decision required, not assumed here:** either (a) backfill `CompanyID` on existing rows by resolving each row's `CreatedByCode → tbl_login.CompanyID`, or (b) leave historical rows as-is and only enforce the new rule going forward (meaning old visits would remain invisible to the manager dashboard/tenant-scoped reports). **This document does not choose between (a) and (b).** | N/A (data operation, not a code change) | `tbl_SalesVisitReport.CompanyID` | **Yes, if backfill (a) is chosen — this is a data UPDATE, not a schema/migration change, but it is explicitly called out because the task instructions prohibit proposing migrations; this row exists to flag that a decision + a carefully-reviewed one-time data-correction script (outside this document's scope) would be needed, not to propose the script itself.** |

---

## 7. Proposed Follow-Up Lifecycle

**Contingent on Decision #6.** This section proposes a **unified mechanism**, without assuming which trigger points should invoke it (that is Decision #6) — it only proposes that whichever trigger points are approved should all call the *same* code path, rather than the current single-path implementation.

| Aspect | Current Behavior | Proposed Behavior | Affected Files | Affected DB Objects | Regression Risk | Dependencies | DB Modification Required |
|---|---|---|---|---|---|---|---|
| Follow-up creation trigger | Only inside `visit_planner.aspx.cs :: btnSubmitExecution_Click`'s SQL batch | Extract into a single shared method (e.g. `SalesVisitFollowUpGenerator.CreateFollowUpIfRequired(visitId, followUpRequired, nextFollowUpDate)`), callable from any approved trigger point (Decision #6) | `visit_planner.aspx.cs` (extract from), potentially `daily_rpt.aspx.cs` and `vw_dailyrpts.aspx.cs` (call into, **pending Decision #6 — not proposed as automatic**) | `tbl_SalesVisitReport` (INSERT) | **Low** if only extracting the existing single trigger point into a shared method with no behavior change; **Medium** if Decision #6 approves adding new trigger points, since that changes user-visible behavior (more calendar entries appear than before) and needs UAT |
| Atomicity | Single `SqlCommand` with `UPDATE` + conditional `INSERT`, no explicit transaction (D-12) | Wrap the execute-and-possibly-spawn-follow-up operation in an explicit `SqlTransaction` (or equivalent `TransactionScope`), so a failure in the follow-up `INSERT` rolls back the `UPDATE` too, surfacing a single clear error instead of a silently partial state | `visit_planner.aspx.cs` | `tbl_SalesVisitReport` | **Low** — purely a reliability improvement, same net successful-path behavior | None beyond standard `SqlTransaction` usage | **No** |
| `CompanyID` propagation | Not copied to the new row (part of D-01) | New row's `INSERT` includes `CompanyID` copied from the parent (or resolved via the same `CompanyContext` used elsewhere) | `visit_planner.aspx.cs` | `tbl_SalesVisitReport.CompanyID` | **Low** | §6 (tenant isolation rule) | **No** |
| `ParentVisitId` visibility | Written, never read (D-06) | Add a read-side surface: at minimum, expose "Follow-up of visit #N" / "Follow-up visit: #M" as a display field in the existing view/edit modals (`visit_planner.aspx` view modal, `vw_dailyrpts.aspx` / `srch_dailyrpts.aspx` mega-modal Details tab), via a `SELECT` that joins `tbl_SalesVisitReport` to itself on `ParentVisitId` | `visit_planner.aspx.cs`/`.aspx`, `vw_dailyrpts.aspx.cs`/`.aspx`, `srch_dailyrpts.aspx.cs`/`.aspx` | `tbl_SalesVisitReport.ParentVisitId` (read-only addition) | **Low** — additive, read-only UI change | None | **No** |
| Chain depth | Unbounded (a follow-up can itself spawn a follow-up indefinitely) | **No change proposed** unless a business decision imposes a cap — not assumed here; flagged as a potential future business rule if unbounded chains prove to be an operational problem | — | — | N/A | New decision if pursued | N/A |
| `Status`/`FollowUpRequired` literals on the generated row | Hardcoded `'Pending'` / `'No'` | Reference the shared `SalesVisitStatusCodes` constants (§11) instead of inline string literals, so future vocabulary changes (§5.2) propagate automatically | `visit_planner.aspx.cs` | `tbl_SalesVisitReport.Status`, `.FollowUpRequired` | **Low** | §11 refactor | **No** |

---

## 8. Proposed Expense Approval Lifecycle

**Contingent on Decision #4.** Two candidate designs are proposed; neither is assumed as correct.

### 8.1 Current behavior (baseline)

- `tbl_Expenses.ApprovalStatus` is independent of `tbl_SalesVisitReport.ApprovalStatus`.
- Approving/rejecting a **visit** (`srch_dailyrpts.aspx.cs :: ProcessApproval`) does **not** touch `tbl_Expenses` at all (cascade explicitly removed per code comment).
- Approving/rejecting an **individual expense** (`gvMegaExpenses_RowCommand`) never checks the parent visit's `ApprovalStatus`.
- The approval notification e-mail (`GetVisitEmailBody`/`SendApprovalNotification`) still contains copy claiming linked expenses were updated — **this is a confirmed content/behavior mismatch (see `06_Business_Rules_Requiring_Confirmation.md` #4) that should be corrected regardless of which lifecycle design is chosen**, since the e-mail is currently inaccurate under the *existing* (already-shipped) behavior.

### 8.2 Option A — Keep decoupled (formalize the current behavior as intentional)

| Aspect | Proposed Behavior |
|---|---|
| Visit approval | No change — remains independent of expense approval |
| Expense approval | No change — remains independently actionable regardless of visit `ApprovalStatus` |
| Notification copy | **Fix the e-mail text** in `GetVisitEmailBody`/`SendApprovalNotification` to remove the now-inaccurate "expenses have also been updated" sentence |
| Affected files | `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs` (both copies of the email-body builder — see D-18) |
| Affected DB objects | None (text-only change) |
| Regression risk | **Very low** |
| Dependencies | Decision #4 confirming decoupling is intentional |
| DB modification required | **No** |

### 8.3 Option B — Reintroduce a cascade (visit rejection also blocks/rejects pending linked expenses; visit approval does not auto-approve expenses, since financial approval may need independent scrutiny — `ASSUMPTION` framing only, not a recommendation)

| Aspect | Proposed Behavior |
|---|---|
| Visit rejection | When a visit is rejected, any of its linked `tbl_Expenses` rows still in `ApprovalStatus='Pending'` are also transitioned to `Rejected` (rationale: an expense tied to a rejected visit likely shouldn't remain independently payable) — **this is one plausible policy, not the only one; the opposite policy (expenses always independent) is equally defensible and is Option A** |
| Visit approval | **No automatic cascade proposed even in this option** — expense approval is a distinct financial control and arguably should remain a deliberate, separate manager action even when the visit itself is approved |
| Affected files | `srch_dailyrpts.aspx.cs :: ProcessApproval` |
| Affected DB objects | `tbl_Expenses.ApprovalStatus`, `.ApprovedBy`, `.ApprovedDate` (additional `UPDATE` from the same method, ideally in the same transaction as the visit's own `ApprovalStatus` update) |
| Regression risk | **Medium** — changes existing expense records' state as a side effect of an action (visit rejection) that previously had no effect on `tbl_Expenses`; needs explicit UAT and stakeholder sign-off since it is a behavior change, not just a defect fix |
| Dependencies | Decision #4 (must be explicitly chosen, not inferred) |
| DB modification required | **No** (uses existing columns) |

**This document does not select between 8.2 and 8.3.** Both are presented so that whichever direction Decision #4 confirms, the affected-files/impact/risk analysis is already available.

---

## 9. Proposed Validation Rules

Consolidating the client-side rules already documented in `01_Current_Process_Flow.md` §F into a **single, shared, server-side-enforced rule set**, closing the D-13 defense-in-depth gap without changing any currently-enforced client-side rule (this is additive server-side hardening, not a UX change, **except** where explicitly marked as decision-contingent).

| Field(s) | Current Enforcement | Proposed Enforcement | Affected Files | Affected DB Columns | Regression Risk | Dependencies | DB Modification Required |
|---|---|---|---|---|---|---|---|
| Start/End time required + End > Start | Client-only (`daily_rpt.aspx :: validateSalesVisitForm`) | Add equivalent server-side check in `btnSubmit_Click` before the `INSERT`, returning the same error panel already used for exceptions (`PanelError`/`lblErrorMsg`) rather than a raw exception | `daily_rpt.aspx.cs` | `tbl_SalesVisitReport.VisitDate`, `.VisitEndDate` | **Low** — only rejects requests that were already invalid per the existing client rule; no legitimate request is newly blocked | None | **No** |
| Customer Name / Department / Contact Person / Visit Type / Discussion required | Client-only | Add server-side non-empty checks in `btnSubmit_Click`, `btnSubmitExecution_Click`, and `btnUpdateVisit_Click` | `daily_rpt.aspx.cs`, `visit_planner.aspx.cs`, `vw_dailyrpts.aspx.cs` | corresponding `nvarchar` columns on `tbl_SalesVisitReport` | **Low** | None | **No** (`ASSUMPTION`: these columns are nullable/unconstrained today per D-13; if DDL review reveals `NOT NULL` constraints already exist, this proposed change becomes purely defensive/redundant rather than closing an actual gap) |
| Follow-Up + Status required (past-mode only) | Client-only, mode-dependent | Mirror the same mode-dependent requirement server-side in `btnSubmit_Click` | `daily_rpt.aspx.cs` | `tbl_SalesVisitReport.Status`, `.FollowUpRequired` | **Low** | None | **No** |
| Status/FollowUp value must be one of the canonical set (§5.2/§5.4) | Not checked anywhere (any string could theoretically be persisted if a request bypasses the dropdown) | Validate submitted values against `SalesVisitStatusCodes` allowed lists before every `INSERT`/`UPDATE` | `daily_rpt.aspx.cs`, `visit_planner.aspx.cs`, `vw_dailyrpts.aspx.cs` | `tbl_SalesVisitReport.Status`, `.FollowUpRequired`, `.VisitPhase` | **Low-Medium** — could reject a currently-tolerated stray value if one exists in production data or is somehow still reachable; recommend logging-only ("would reject") mode first, per §12 | §5.2/§5.4 vocabulary decision | **No** |
| Manager Remarks required before Approve/Reject | Not enforced (D-14 in `06`, item 3) | **Decision-contingent (Decision #3).** If confirmed required: add both a client hint (already partially present via placeholder text) and a server-side non-empty check in `ProcessApproval` | `srch_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx` | `tbl_SalesVisitReport.ManagerRemarks` | **Low** if confirmed; **do not implement** until Decision #3 is answered | Decision #3 | **No** |
| GPS required for execution | Client-only, calendar-flow only (D-... / Decision #1) | **Decision-contingent (Decision #1).** No change proposed until answered; if confirmed mandatory for all executions, `daily_rpt.aspx`'s past-mode form would need new required Lat/Long inputs plus server-side enforcement | `daily_rpt.aspx`/`.cs` | `tbl_SalesVisitReport.Latitude`, `.Longitude` | **Medium** if implemented, since it changes a currently-optional data-entry path into a mandatory one, likely requiring UI changes (browser geolocation isn't naturally available mid-form on a desktop past-mode entry the way it is on the calendar's live-execute flow) | Decision #1 | **No** |
| Attachment extension allow-list | Not enforced (D-09) | See §10.3 (security remediation) — cross-referenced here since it is also a validation rule | `daily_rpt.aspx.cs`, `visit_planner.aspx.cs`, `vw_dailyrpts.aspx.cs`, `expense_entry.aspx.cs` | `tbl_SalesVisitReport.AttachmentName`, `tbl_Expenses.AttachmentName` | **Low-Medium** — could reject legitimate but unusual file types already in use; recommend confirming the real-world set of file types users currently upload before finalizing the allow-list | Operational input (what file types are actually used today) — **not assumed here** | **No** |

---

## 10. Proposed Security Remediation

| # | Issue | Current Behavior | Proposed Behavior | Affected Files | Affected DB Objects | Regression Risk | Dependencies | DB Modification Required |
|---|---|---|---|---|---|---|---|---|
| S-1 | SQL injection (D-05) | `srch_dailyrpts.aspx.cs :: Binder()` builds `cmdstring` via raw string concatenation of `companyId`, `selectedUser`, `fromDateStr`, `toDateStr` | Rewrite using parameterized `SqlCommand` + `SqlParameter`s for every variable input, matching the pattern already used correctly in `vw_dailyrpts.aspx.cs :: BindSalesVisits` | `srch_dailyrpts.aspx.cs` | `tbl_SalesVisitReport` (read-only) | **Very low** — behaviorally equivalent query, different construction mechanism; standard, well-understood fix | None | **No** |
| S-2 | SQL injection (minor) (D-05 sibling) | `daily_rpt.aspx.cs :: GetAdminName()` builds query via string concatenation of the session's own `USERID` | Rewrite using a parameterized query | `daily_rpt.aspx.cs` | `tbl_login` (read-only) | **Very low** | None | **No** |
| S-3 | Broken access control / IDOR (D-04) | See §4 | See §4 | See §4 | See §4 | See §4 | Decision #8, D-01 remediation | **No** |
| S-4 | Unrestricted file upload extension (D-09) | No allow-list/deny-list on any upload site | Introduce a shared extension allow-list (e.g. common image/document types) enforced before `SaveAs` in every upload site | `daily_rpt.aspx.cs`, `visit_planner.aspx.cs`, `vw_dailyrpts.aspx.cs`, `expense_entry.aspx.cs` | `tbl_SalesVisitReport.AttachmentName`, `tbl_Expenses.AttachmentName` | **Low-Medium** (see §9 note on confirming real-world file types first) | Operational input on allowed types | **No** |
| S-5 | Unauthenticated static file retrieval (D-09) | Files under `~/Uploads/` and `~/Uploads/Expenses/` are served as raw static content with no session check | Introduce an authenticated retrieval endpoint (e.g. a `.ashx` generic handler or a dedicated download page) that checks `Session["USERID"]` **and** re-applies the same ownership/tenant check as the record the attachment belongs to, before streaming the file; update all `href`/`NavigateUrl` references to point at the new handler instead of the raw `~/Uploads/...` path | `visit_planner.aspx`/`.cs`, `vw_dailyrpts.aspx`/`.cs`, `srch_dailyrpts.aspx`/`.cs`, `GetVisitEmailBody` (both copies), plus a new handler file | `tbl_SalesVisitReport.AttachmentName`, `.Id`, `.CreatedByCode`, `.CompanyID`; `tbl_Expenses.AttachmentName`, `.VisitId` | **Medium** — changes every attachment link's URL shape; must ensure the e-mail-embedded links (which are consumed by users outside an authenticated browser session, e.g. via a mail client) still work, which likely requires either (a) a short-lived signed/token URL scheme, or (b) accepting that e-mail links will now require the recipient to log in first. **This nuance requires a decision, not an assumption**, since it changes how notification emails function today (currently: click and view immediately; proposed: click, then log in) | S-4 (ideally paired), decision on e-mail-link UX | **No** |
| S-6 | Hardcoded SMTP credentials (D-11) | Literal credential in `vw_dailyrpts.aspx.cs` and `srch_dailyrpts.aspx.cs` | Migrate both files to read `SmtpFrom`/`SmtpUser`/`SmtpPass`/`SmtpHost`/`SmtpPort`/`SmtpEnableSsl` from `ConfigurationManager.AppSettings`, mirroring the pattern already used correctly in `index.aspx.cs :: SendEmail` | `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs` | None | **Low** for the code change itself; **the credential itself should be rotated as a separate operational action** (outside the scope of a code PR) since it is already exposed in git history | **Configuration change is required** (adding `AppSettings` keys) — **explicitly out of scope for this document per task instructions ("do not change configuration"); flagged here as a necessary follow-up action, not performed** | **No** (configuration, not schema) |
| S-7 | Silent notification failures (D-10) | `catch (Exception) { /* fail silently */ }` in all notification-sending methods | Log the exception (e.g. to the existing `App_Data/ErrorLogs/` mechanism already used in `index.aspx.cs :: LogError`, reusing that pattern rather than inventing a new one) instead of swallowing it silently; whether to also surface a non-blocking warning to the end user is a UX decision, not assumed here | `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs` | None | **Very low** | None | **No** |
| S-8 | Raw exception messages shown to users (D-14) | `ex.Message` echoed directly into UI | Log full exception details server-side (reusing `LogError` pattern); show a generic, non-sensitive message to the user | `daily_rpt.aspx.cs`, `visit_planner.aspx.cs`, `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs` | None | **Low** | None | **No** |
| S-9 | Non-atomic execute+follow-up batch (D-12) | See §7 | See §7 | `visit_planner.aspx.cs` | `tbl_SalesVisitReport` | **Low** | None | **No** |
| S-10 | Hardcoded fallback user `"FLM03"` (D-03) | `Session["USERID"]?.ToString() ?? "FLM03"` | Remove the fallback; if `Session["USERID"]` is null at postback time, redirect to login (matching the `Page_Load` guard's own intent) instead of silently attributing the record to a hardcoded user | `daily_rpt.aspx.cs` | `tbl_SalesVisitReport.CreatedByCode` | **Low** — this path should be unreachable in normal operation per the existing `Page_Load` guard; removing the fallback only changes behavior in the already-anomalous session-expired-mid-postback case | None | **No** |
| S-11 | No CSRF/anti-forgery protection noted on state-changing postbacks or PageMethods | None found | **Flagged for awareness only — no specific mechanism is proposed here**, since ASP.NET WebForms' built-in ViewState MAC provides partial (not complete) protection and a full CSRF-hardening pass is a cross-cutting, application-wide concern beyond this one workflow's scope | *(cross-cutting, out of scope for this document)* | N/A | N/A | Application-wide security review | **No** |

---

## 11. Proposed Refactoring Boundaries

These are **proposed extraction boundaries only** — no code is created here. Each box below identifies what would move out of the existing `.aspx.cs` files and into a shared class, without changing the pages' external behavior.

| Proposed Component | Responsibility | Extracted From | Consumed By | Regression Risk of Extraction |
|---|---|---|---|---|
| `SalesVisitRepository` | All parameterized `SELECT`/`INSERT`/`UPDATE` against `tbl_SalesVisitReport`, always including the resolved ownership/tenant predicate (§4) | `visit_planner.aspx.cs`, `daily_rpt.aspx.cs`, `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs` | same four files | **Medium** — this is the single largest extraction; must be done method-by-method with behavioral parity checks (ideally against a staging DB) rather than as one large change; see §12 sequencing |
| `SalesVisitAuthorizationPolicy` | Answers "can `userId` view/edit/execute/approve `visitId`?" per §4's matrix | Implicit/missing logic in all four files | `SalesVisitRepository` (to build `WHERE` predicates) and page code-behinds (to short-circuit before even querying) | **Medium** — behavior-defining; must be reviewed against the finalized Decision #8/#9 answers before being trusted as "the" authorization source |
| `SalesVisitStatusCodes` | Canonical `VisitPhase`/`Status`/`ApprovalStatus`/`FollowUpRequired` allowed-value lists and display labels | Hardcoded string literals and `<asp:ListItem>` blocks across `daily_rpt.aspx`, `visit_planner.aspx`, `vw_dailyrpts.aspx` | All four `.aspx`/`.aspx.cs` files (dropdown population + validation) | **Low** — purely a constants/lookup extraction, provided the exact same literal values are reused (no vocabulary change bundled into this step; vocabulary *unification* per §5.2 is a separate, later, decision-gated step) |
| `SalesVisitNotificationService` | Building visit-detail HTML e-mail bodies and sending chat/approval notifications | `vw_dailyrpts.aspx.cs :: GetVisitEmailBody`/`SendChatEmailNotification`, `srch_dailyrpts.aspx.cs :: GetVisitEmailBody`/`SendChatEmailNotification`/`SendApprovalNotification` | `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs` | **Medium** — must reconcile the two currently-drifted implementations (D-07); requires a decision on which behavior "wins" where they differ (e.g. the stricter regex validation) — proposed default: **adopt the stricter/more-defensive behavior from each divergent pair**, since that is the lower-risk direction (fails safe rather than fails open); flagged as `ASSUMPTION` |
| `VisitAttachmentStorageService` | Upload (with extension allow-list, §10 S-4) and authenticated retrieval (§10 S-5) of attachments | Inline `SaveAs`/path-building logic in `daily_rpt.aspx.cs`, `visit_planner.aspx.cs`, `vw_dailyrpts.aspx.cs`, `expense_entry.aspx.cs` | same files, plus a new retrieval handler | **Medium** (tied to S-5's URL-shape change) |
| `SalesVisitFollowUpGenerator` | Follow-up visit creation logic (§7) | `visit_planner.aspx.cs :: btnSubmitExecution_Click`'s embedded SQL batch | `visit_planner.aspx.cs`, and (pending Decision #6) `daily_rpt.aspx.cs`/`vw_dailyrpts.aspx.cs` | **Low** for the extraction itself; **Medium** if new trigger points are added per Decision #6 |
| `SalesVisitValidationService` (or per-field validators) | Server-side required-field and vocabulary validation (§9) | New logic, not previously present server-side | `daily_rpt.aspx.cs`, `visit_planner.aspx.cs`, `vw_dailyrpts.aspx.cs` | **Low-Medium** (see §9 per-rule risk notes) |

**Sequencing note:** extraction should follow behavior-preserving refactor discipline — each component should be introduced by moving existing logic verbatim first (no behavior change), with the actual defect fixes (parameterization, ownership predicates, vocabulary unification, etc.) applied as clearly separate, reviewable diffs afterward. This is reflected in the PR sequence (§12).

---

## 12. Proposed PR / Change Sequence

Ordered to front-load **zero-ambiguity, zero-decision-dependency security fixes** first, then defect fixes that are self-contained, then the decision-gated / higher-regression-risk items last — so that value is delivered incrementally without blocking on business decisions that may take time to obtain.

| PR | Title | Addresses | Affected Files | Affected DB Objects | Regression Risk | Dependencies | DB Modification Required |
|---|---|---|---|---|---|---|---|
| **PR-1** | Parameterize the two remaining raw-SQL-concatenation queries | D-05 (S-1, S-2) | `srch_dailyrpts.aspx.cs`, `daily_rpt.aspx.cs` | `tbl_SalesVisitReport`, `tbl_login` (read-only) | Very low | None | No |
| **PR-2** | Populate `CompanyID` on every `tbl_SalesVisitReport` INSERT going forward | D-01 (§6) | `daily_rpt.aspx.cs`, `visit_planner.aspx.cs` | `tbl_SalesVisitReport.CompanyID` | Low (forward-only; does not touch existing rows) | None for the code change; **a separate, explicitly-approved decision + data-correction exercise is needed for historical rows** (not proposed here) | No (schema); historical backfill, if approved, is a data change, not a migration |
| **PR-3** | Add ownership/tenant predicates to all detail/mutation endpoints (§4) | D-04, D-15, D-16 (partially, pending Decision #8) | `visit_planner.aspx.cs`, `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs` | `tbl_SalesVisitReport`, `tbl_Expenses` | Medium (must land after PR-2, else legitimate current-company visits with `NULL CompanyID` would become invisible) | **PR-2 must land first**; Decision #8 determines whether the manager-facing predicate is `CompanyID`-only (Variant A) or also `ReportingManagerId`-scoped (Variant B) | No |
| **PR-4** | Fix `ApprovalStatus` idempotency guard + unify `Status` vocabulary | D-08, D-02, D-19 | `srch_dailyrpts.aspx.cs`, `vw_dailyrpts.aspx.cs`, `daily_rpt.aspx.cs`, `visit_planner.aspx.cs` (markup dropdowns) | `tbl_SalesVisitReport.ApprovalStatus`, `.Status` | Medium (vocabulary unification is a user-visible dropdown-options change; needs the vocabulary decision noted in §5.2 confirmed first) | Vocabulary choice confirmation (§5.2) | No |
| **PR-5** | Externalize SMTP config + consolidate notification logic + stop silent-failure swallowing | D-10, D-11 (code portion only — credential rotation is a separate operational action), D-07, D-18 (notification portion) | `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs` | None | Medium (behavioral reconciliation of the two drifted implementations per §11's `ASSUMPTION`) | **Requires a configuration change** (new `AppSettings` keys) — flagged as out-of-scope for a code-only PR per task instructions; must be coordinated with whoever owns `Web.config`/deployment config, and the **credential itself must be rotated** as a prerequisite/concurrent operational step, not part of this PR's code diff | No (schema); Yes (configuration — explicitly called out, not performed here) |
| **PR-6** | Wrap execute+follow-up in a transaction; extract `SalesVisitFollowUpGenerator`; add read-side `ParentVisitId` display | D-12, D-06 | `visit_planner.aspx.cs`, `visit_planner.aspx`, `vw_dailyrpts.aspx`/`.cs`, `srch_dailyrpts.aspx`/`.cs` | `tbl_SalesVisitReport` | Low | None (this PR does **not** add new follow-up trigger points — that is deferred to a decision-gated follow-up PR, see PR-9) | No |
| **PR-7** | File upload extension allow-list + authenticated attachment retrieval handler | D-09 | `daily_rpt.aspx.cs`, `visit_planner.aspx.cs`, `vw_dailyrpts.aspx.cs`, `expense_entry.aspx.cs`, plus a new retrieval handler; all `.aspx` files with attachment links; both `GetVisitEmailBody` copies | `tbl_SalesVisitReport.AttachmentName`, `tbl_Expenses.AttachmentName` | Medium-High (changes the URL shape used in already-sent historical e-mails and any bookmarked links; needs explicit decision on e-mail-link UX per §10 S-5) | Decision on e-mail-link UX (log-in-required vs. signed URL); ideally sequenced after PR-5 (shared notification service) so the new link format is generated in one place | No |
| **PR-8** | Server-side validation parity (required fields, vocabulary enforcement) + replace raw exception surfacing with generic messages + server logging | D-13, D-14 | `daily_rpt.aspx.cs`, `visit_planner.aspx.cs`, `vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs` | `tbl_SalesVisitReport` (validation only, no schema impact) | Low-Medium (per §9 per-rule notes; recommend a "log would-reject" soft-launch period before hard-enforcing vocabulary checks, to catch any stray legacy data/values) | None strictly, but benefits from PR-4's vocabulary decision being finalized first | No |
| **PR-9** *(decision-gated, not scheduled until business input received)* | Expand follow-up-generation trigger points per Decision #6; adjust `FollowUpRequired` domain per Decision #7; implement chosen expense-approval lifecycle option per Decision #4; add Manager-Remarks-required check per Decision #3; add GPS-mandatory enforcement per Decision #1; add stale-`Planned`-visit terminal state per Decision #5 | D-17, plus items 3/4/5/6/7/1 from `06_Business_Rules_Requiring_Confirmation.md` | Varies per sub-item (see §5, §7, §8, §9 tables above) | Varies per sub-item | Varies (each sub-item independently assessed above) — **recommend splitting PR-9 into one PR per decision once each is answered, rather than one combined PR**, so that unrelated decisions don't block each other's delivery | **All of Decisions #1, #3, #4, #5, #6, #7 (§2)** — none are assumed; this PR (or its split sub-PRs) cannot start until answered | Varies; flagged per sub-item as "UNKNOWN pending DDL confirmation" for the `VisitPhase`-terminal-state case (§5.5) specifically |

### 12.1 Sequencing rationale summary

1. **PR-1** first: highest-severity, zero-ambiguity, zero-dependency fix (SQL injection).
2. **PR-2 → PR-3** in strict order: fixing the authorization gap *before* fixing the missing tenant column would make the manager dashboard show nothing at all for existing/new data; the order shown avoids that regression.
3. **PR-4 through PR-8**: self-contained defect fixes and hardening that do not require new business decisions beyond what's already narrowly scoped in each row (e.g., PR-4's vocabulary choice is a small, focused decision, not a broad one).
4. **PR-9**: everything that depends on the ten open business-rule decisions in §2 is deliberately pushed last and explicitly not scheduled, per the task instruction not to invent business requirements.

---

## Appendix: Full Traceability Back to Source Audit Documents

| This document's section | Primary source(s) |
|---|---|
| §1 Confirmed technical defects | `05_Potential_Defects.md` (all D-01…D-19) |
| §2 Business-rule decisions | `06_Business_Rules_Requiring_Confirmation.md` (items 1–10) |
| §3 Recommended architecture | `01_Current_Process_Flow.md`, `02_Database_Dependency_Map.md`, `04_Security_and_Tenant_Audit.md` (synthesized) |
| §4 Authorization matrix | `04_Security_and_Tenant_Audit.md` §E, §J; `05_Potential_Defects.md` D-04, D-16 |
| §5 State-transition matrix | `03_State_Machine.md` §D.1–D.4; `05_Potential_Defects.md` D-02, D-08 |
| §6 Tenant-isolation rules | `04_Security_and_Tenant_Audit.md` §E; `05_Potential_Defects.md` D-01, D-15 |
| §7 Follow-up lifecycle | `03_State_Machine.md` §I; `05_Potential_Defects.md` D-06, D-12, D-17 |
| §8 Expense approval lifecycle | `01_Current_Process_Flow.md` (PR removal comment); `06_Business_Rules_Requiring_Confirmation.md` item 4 |
| §9 Validation rules | `01_Current_Process_Flow.md` §F; `05_Potential_Defects.md` D-13 |
| §10 Security remediation | `04_Security_and_Tenant_Audit.md` §G, §K; `05_Potential_Defects.md` D-03…D-11, D-14 |
| §11 Refactoring boundaries | `05_Potential_Defects.md` D-18, D-19; synthesized architectural recommendation |
| §12 PR/change sequence | Synthesized from all of the above |
