# 03 — State Machine

All state is stored as **independent free-text columns** on `tbl_SalesVisitReport`. There is no single unifying "workflow state" column and no database `CHECK` constraint tying the four state fields together — each is set/read by different pages with **different, occasionally mismatched, vocabularies**. This document describes the states and transitions **as implemented**, not as they should ideally be designed.

---

## D.1 `VisitPhase`

| Value | Meaning | Set By |
|---|---|---|
| `Planned` | Visit exists on the calendar but has not happened yet | `daily_rpt.aspx.cs :: btnSubmit_Click` (mode=`plan`); also the hardcoded value for system-generated follow-up rows (`visit_planner.aspx.cs`) |
| `Executed` | Visit has taken place | `daily_rpt.aspx.cs :: btnSubmit_Click` (mode=`past`, set immediately at creation); `visit_planner.aspx.cs :: btnSubmitExecution_Click` (`UPDATE ... SET VisitPhase='Executed'`) |

**Transition diagram:**

```
        (daily_rpt.aspx, mode=plan)
                    │
                    ▼
              ┌───────────┐   btnSubmitExecution_Click       ┌───────────┐
              │  Planned  │ ───────────────────────────────▶ │ Executed  │
              └───────────┘   (visit_planner.aspx calendar)  └───────────┘
                    ▲                                              ▲
                    │                                              │
     (auto follow-up INSERT, ParentVisitId set)            (daily_rpt.aspx, mode=past —
                    │                                        created directly as Executed,
                    └── spawned FROM an Executed visit        bypassing the Planned state
                        when FollowUpRequired='Yes'           entirely)
```

- **One-way only:** there is no code path anywhere in the analyzed workflow that transitions a row from `Executed` back to `Planned`, or introduces any other phase (e.g. `Cancelled`, `Missed`, `No-Show`). A `Planned` visit that is simply never executed has no explicit "expired"/"missed" terminal state — it remains `Planned` indefinitely (ambiguity — see `06_Business_Rules_Requiring_Confirmation.md`).
- **Two independent creation entry points reach different phases:** `daily_rpt.aspx` can create a row in *either* phase depending on `mode`, while `visit_planner.aspx` can only ever *transition* an existing `Planned` row to `Executed` (it never creates brand-new `Planned` rows itself, except indirectly via the auto-follow-up `INSERT`).

## D.2 `Status`

| Value | Where Set | Where Offered as a Choice |
|---|---|---|
| `Pending Execution` | Hardcoded default in `daily_rpt.aspx.cs` when `mode=plan` (`cmd.Parameters.AddWithValue("@Status", "Pending Execution")`) | Offered in `vw_dailyrpts.aspx` **search filter** dropdown (`ddlSearchStatus`) and **edit** dropdown (`edit_ddlStatus`) |
| `Completed` | User-selected in `daily_rpt.aspx` (mode=past) and `visit_planner.aspx` execute modal | Offered everywhere (`ddlStatus`, `ddlExecStatus`, `ddlSearchStatus`, `edit_ddlStatus`) |
| `Pending` | User-selected in `daily_rpt.aspx` (mode=past) and `visit_planner.aspx` execute modal | Offered **only** in `ddlStatus` (`daily_rpt.aspx`) and `ddlExecStatus` (`visit_planner.aspx`) — **NOT offered** in `vw_dailyrpts.aspx`'s `edit_ddlStatus` or `ddlSearchStatus` |
| `Escalated` | User-selected in `daily_rpt.aspx` (mode=past) and `visit_planner.aspx` execute modal | Offered everywhere |

**Confirmed vocabulary mismatch (high-impact):** the value written by the two "execute" flows is the literal string `"Pending"`, but `vw_dailyrpts.aspx`'s own edit dropdown (`edit_ddlStatus`, `vw_dailyrpts.aspx` lines ~213–220) only contains the items `-- Select Status --` / `Completed` / `Pending Execution` / `Escalated` — **it has no `"Pending"` item at all.** When `LoadMegaModal` runs:

```csharp
if (edit_ddlStatus.Items.FindByValue(rdr["Status"].ToString()) != null)
    edit_ddlStatus.SelectedValue = rdr["Status"].ToString();
```
(`vw_dailyrpts.aspx.cs`, `LoadMegaModal`)

`FindByValue("Pending")` returns `null`, so the guard is skipped and the dropdown silently keeps its **first item** (the blank placeholder, value `""`). If the salesperson then clicks "💾 Save Changes" without noticing/touching the Status dropdown, `btnUpdateVisit_Click` will write `Status=""` back to the database, **silently overwriting a legitimate `"Pending"` status with an empty string.** See `05_Potential_Defects.md` D-02 for full detail.

## D.3 `ApprovalStatus`

| Value | Meaning | Set By |
|---|---|---|
| `Pending` | Default/initial state (implied — no INSERT in the 8 in-scope files explicitly sets this column, so its default must come from a DB-level `DEFAULT` constraint not visible in code; every read path treats a visit as actionable when `ApprovalStatus == "Pending"`) | Implicit default |
| `Approved` | Manager approves | `srch_dailyrpts.aspx.cs :: ProcessApproval("Approved")` |
| `Rejected` | Manager rejects | `srch_dailyrpts.aspx.cs :: ProcessApproval("Rejected")` |

**Transition diagram:**

```
   ┌─────────┐   btnMegaApprove_Click   ┌───────────┐
   │ Pending │ ───────────────────────▶ │ Approved  │
   └─────────┘                          └───────────┘
        │
        │  btnMegaReject_Click
        ▼
   ┌───────────┐
   │ Rejected  │
   └───────────┘
```

- **One-way, terminal:** once `Approved` or `Rejected`, there is no code path to revert to `Pending` or to flip between `Approved`/`Rejected` (the UI hides the action buttons once `ApprovalStatus != 'Pending'`, and the only server-side guard on the salesperson's own edit path (`vw_dailyrpts.aspx.cs`) also keys off `ApprovalStatus = 'Pending'`).
- **No idempotency/race guard:** `ProcessApproval`'s `UPDATE` statement does **not** include `AND ApprovalStatus = 'Pending'` in its `WHERE` clause (contrast with `vw_dailyrpts.aspx.cs :: btnUpdateVisit_Click`, which does include such a guard for its own update). Two concurrent approve/reject actions (or a stale/replayed form submission) could each succeed and overwrite each other's `ApprovalStatus`/`ManagerRemarks`/`ApprovedBy`/`ApprovedDate` without detection. See `05_Potential_Defects.md` D-08.
- **Drives the Edit-Lock state machine** on `vw_dailyrpts.aspx` — see Section J in `04_Security_and_Tenant_Audit.md`.

## D.4 `FollowUpRequired`

A **three-valued** field, not a true boolean:

| Value | Meaning | Set By |
|---|---|---|
| `''` (empty string) | Not yet applicable (visit hasn't been executed yet) | `daily_rpt.aspx.cs` (mode=`plan` insert, hardcoded `""`) |
| `No` | Explicitly no follow-up needed | User selection (any of the three data-entry/edit forms) |
| `Yes` | Follow-up needed | User selection — **only this value, combined with a non-null `NextFollowUpDate`, triggers automatic follow-up visit creation, and only from one specific code path (see §I below)** |

No state diagram is meaningful here since this is a flag, not a lifecycle — but its **three-valued domain** (rather than two) is itself worth flagging as a data-modeling inconsistency (see `06_Business_Rules_Requiring_Confirmation.md`).

---

## I. Follow-Up Generation & `ParentVisitId` — Exact Mechanics

### Where follow-up rows ARE created

**Only** inside `visit_planner.aspx.cs :: btnSubmitExecution_Click`, as the second statement of a two-statement `SqlCommand` batch:

```sql
UPDATE tbl_SalesVisitReport
SET VisitPhase = 'Executed', ExecutionDateTime = GETDATE(), Latitude = @Latitude,
    Longitude = @Longitude, DiscussionPoints = @DiscussionPoints, Status = @Status,
    FollowUpRequired = @FollowUpRequired, NextFollowUpDate = @NextFollowUpDate,
    AttachmentName = ISNULL(@AttachmentName, AttachmentName)
WHERE Id = @Id;

-- AUTO FOLLOW-UP LOGIC WITH PARENT LINKAGE
IF @FollowUpRequired = 'Yes' AND @NextFollowUpDate IS NOT NULL
BEGIN
    INSERT INTO tbl_SalesVisitReport (
        VisitDate, VisitEndDate, Salesperson, CustomerName, Department, ContactPerson,
        VisitType, DiscussionPoints, VisitPhase, Status, FollowUpRequired,
        CreatedDate, CreatedByCode, ParentVisitId
    )
    SELECT
        @NextFollowUpDate, DATEADD(hour, 1, @NextFollowUpDate), Salesperson, CustomerName, Department, ContactPerson,
        VisitType, 'Automated Follow-up regarding: ' + @DiscussionPoints,
        'Planned', 'Pending', 'No', GETDATE(), CreatedByCode, @Id
    FROM tbl_SalesVisitReport
    WHERE Id = @Id;
END
```

Mechanics:
1. **Trigger condition:** the *just-submitted* execution form has `FollowUpRequired = 'Yes'` AND a non-null `NextFollowUpDate`.
2. **Source-of-truth for the new row:** the `SELECT ... FROM tbl_SalesVisitReport WHERE Id=@Id` re-reads the row **after** the preceding `UPDATE` in the same batch has already applied — so `Salesperson`, `CustomerName`, `Department`, `ContactPerson`, `VisitType`, and `CreatedByCode` are copied forward from the (now-Executed) parent.
3. **New row's fields:** `VisitDate = @NextFollowUpDate` (the date the salesperson chose), `VisitEndDate = @NextFollowUpDate + 1 hour` (hardcoded 1-hour duration), `DiscussionPoints` is auto-prefixed with `'Automated Follow-up regarding: '`, `VisitPhase = 'Planned'`, `Status = 'Pending'` (hardcoded literal — see D.2 vocabulary mismatch), `FollowUpRequired = 'No'` (hardcoded — the new visit itself is never pre-flagged as needing a further follow-up), `ParentVisitId = @Id` (the ID of the visit that was just executed).
4. **No `CompanyID` is copied forward** (column isn't in the INSERT's column list at all) — compounding the tenancy defect described in `02_Database_Dependency_Map.md` / `05_Potential_Defects.md` D-01.
5. **No `ApprovalStatus` is set explicitly** on the new row — relies on whatever the column's DB-level default is (see D.3 above).
6. **Chaining:** because the newly created row is `Planned`, it can itself later be executed via the same calendar flow and spawn a further child — `ParentVisitId` therefore forms an unbounded linked chain, not a fixed 2-level parent/child structure.

### Where follow-up rows are NOT created (but arguably should be, per the UI's own semantics)

- **`daily_rpt.aspx.cs :: btnSubmit_Click` (mode=`past`)** — a user can log a past visit directly as `Executed` with `FollowUpRequired='Yes'` and a `NextFollowUpDate` filled in, and the `INSERT` statement here **never spawns a corresponding child `Planned` row.** The follow-up intent is recorded only as plain columns on the same row, with no calendar entry ever created for it.
- **`vw_dailyrpts.aspx.cs :: btnUpdateVisit_Click`** — editing an existing visit to *change* `FollowUpRequired` to `'Yes'` and set a `NextFollowUpDate` (e.g., correcting/adding follow-up info after the fact) also never spawns a follow-up row.

This means the "system automatically creates your next visit" behavior only actually happens for the **one specific interaction path** (execute-from-calendar). See `06_Business_Rules_Requiring_Confirmation.md` for whether this divergence is intentional.

### Where `ParentVisitId` is read

**Nowhere.** A repository-wide search of the analyzed files (and their immediate cross-file dependents) found **no `SELECT` statement that ever references `ParentVisitId`.** The column is write-only from the application's perspective. There is no UI affordance (link, breadcrumb, "view follow-up chain" button) anywhere in `visit_planner.aspx`, `vw_dailyrpts.aspx`, or `srch_dailyrpts.aspx` that lets a user navigate from a parent visit to its generated follow-up, or vice versa. See `05_Potential_Defects.md` D-06.
