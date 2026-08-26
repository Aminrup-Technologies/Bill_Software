# 06 — Business Rules Requiring Confirmation

This document lists behaviors that are **implemented consistently enough to be intentional-looking**, but whose underlying business intent is ambiguous, self-contradictory across pages, or not stated anywhere in the codebase (no requirements docs, tickets, or design comments were found alongside these files beyond the terse changelog headers already cited). Each item is phrased as a question for a product owner / business stakeholder, with the supporting code evidence.

---

## 1. Is GPS capture mandatory for *all* visit executions, or only for calendar-driven ones?

- **Evidence:** `visit_planner.aspx`'s "Execute & Tag Location" flow hard-blocks the postback client-side unless `navigator.geolocation.getCurrentPosition` succeeds (`captureLocationAndSubmit`). But `daily_rpt.aspx?mode=past` lets a user record a visit as already `Executed` with **no GPS fields on the form at all** — meaning a user can always sidestep the GPS requirement entirely by using the "log a past visit" form instead of the calendar's "Execute" button, even for a visit that just happened.
- **Question:** Should `daily_rpt.aspx`'s past-mode form also require (or at least offer) GPS capture, or is retroactive logging intentionally exempt from location verification (e.g., because the user may be filling it in from a desk, later, not from the field)?

## 2. Is the 45-day edit-lock window meant to apply only to already-executed visits?

- **Evidence:** `vw_dailyrpts.aspx.cs :: LoadMegaModal` computes `(DateTime.Now - VisitDate).TotalDays > 45`. Because `VisitDate` is the *planned* date (which is in the future for not-yet-executed `Planned` visits), this subtraction is negative for those rows and can never trigger the lock — meaning a `Planned` visit, no matter how old it was originally scheduled relative to today (if somehow still Planned and stale), is never locked by *this* rule (only by the other two: approval status or manager comment).
- **Question:** Is the 45-day rule intentionally scoped to "don't let people rewrite history more than 45 days after the fact," and therefore correctly ignores not-yet-happened `Planned` visits? Or should there also be a separate staleness rule for `Planned` visits that were never executed (see item 5)?

## 3. Should Manager Remarks be mandatory before Approve/Reject?

- **Evidence:** `srch_dailyrpts.aspx`'s remarks textbox placeholder reads *"Enter Official Manager Remarks here before approving/rejecting the overall visit..."*, strongly implying remarks should be required. However, `srch_dailyrpts.aspx.cs :: ProcessApproval` performs no check that `remarks` is non-empty — a manager can approve or reject with a blank remarks field.
- **Question:** Should the remarks field be enforced as mandatory (client- and server-side) before an approval/rejection action is accepted?

## 4. Was removing the bulk expense-approval cascade from visit approval intentional?

- **Evidence:** `srch_dailyrpts.aspx.cs :: ProcessApproval` contains the in-line comment `// (REMOVED the bulk tbl_Expenses update from here)`, and expenses are now only approvable/rejectable individually via `gvMegaExpenses_RowCommand`. The outbound approval e-mail (`SendApprovalNotification`) still contains the sentence *"Note: Any expenses linked to this visit have also been updated to {status}."* — which is now **factually incorrect**, since expenses are no longer auto-updated alongside the visit.
- **Question:** Was decoupling visit approval from expense approval an intentional product decision? If so, the e-mail copy in `GetVisitEmailBody`/`SendApprovalNotification` needs to be corrected to stop claiming expenses were auto-updated, since it currently misinforms the recipient.

## 5. What is the intended terminal state for a `Planned` visit that is never executed?

- **Evidence:** `VisitPhase` only ever transitions `Planned → Executed`. There is no `Cancelled`, `Missed`, `Expired`, or `No-Show` state anywhere in the schema usage or UI. A `Planned` visit whose date has long passed without ever being executed remains indefinitely `Planned` with `Status='Pending Execution'`, showing up forever in `visit_planner.aspx`'s calendar/list and in `vw_dailyrpts.aspx`'s search (if within the date-range filter).
- **Question:** Is there an intended (even if manual, out-of-band) process for closing out stale unexecuted plans, or should the application track this explicitly (e.g., an automatic "Missed" state after N days past `VisitDate` with no execution)?

## 6. Should automatic follow-up visit generation apply uniformly, regardless of how the parent visit was executed?

- **Evidence:** Automatic `ParentVisitId`-linked follow-up creation happens **only** inside `visit_planner.aspx.cs :: btnSubmitExecution_Click` (the calendar "Execute" flow). It does **not** happen when: (a) a visit is logged directly as already-executed via `daily_rpt.aspx?mode=past` with `FollowUpRequired='Yes'` and a `NextFollowUpDate`, or (b) an existing visit is later edited in `vw_dailyrpts.aspx` to set `FollowUpRequired='Yes'` with a `NextFollowUpDate` (e.g., correcting an earlier omission).
- **Question:** Should follow-up auto-generation be triggered consistently by *any* path that results in `FollowUpRequired='Yes'` + a populated `NextFollowUpDate`, rather than only the one specific calendar-execute code path? If the current behavior is intentional (e.g., "past" visits are backfilled data-entry and shouldn't spawn new calendar noise), that rationale should be documented so future maintainers don't "fix" it as a bug.

## 7. Is the three-valued `FollowUpRequired` domain (`''` / `'No'` / `'Yes'`) intentional, or should it be a strict two-valued flag?

- **Evidence:** `daily_rpt.aspx.cs` (plan mode) writes an empty string `""` for `FollowUpRequired` because the field genuinely doesn't apply yet (visit hasn't happened). Every other write path uses `'Yes'`/`'No'`. Different dropdowns across the three data-entry surfaces have different item sets (e.g., `visit_planner.aspx`'s `ddlExecFollowUp` has no blank/"" option; `daily_rpt.aspx`'s `ddlFollowUp` does).
- **Question:** Is `''` meant to be a distinct, permanent "not applicable" state (e.g., for `Planned`-but-not-yet-executed visits), or should the column instead default to `'No'` until explicitly changed, simplifying it back to a true two-valued flag?

## 8. Who is actually authorized to approve/reject a given salesperson's visit?

- **Evidence:** The data model includes `tbl_login.ReportingManagerId`, strongly implying a designed concept of "each salesperson has one direct manager who reviews their visits." In practice, `srch_dailyrpts.aspx` (and its underlying `ProcessApproval`/`gvMegaExpenses_RowCommand` methods) allow **any** user who can reach the manager dashboard to search, view, comment on, approve, and reject **any** visit/expense belonging to **any** salesperson in the same `CompanyID` — the `ReportingManagerId` relationship is never consulted for this purpose (see `04_Security_and_Tenant_Audit.md` §E, and `05_Potential_Defects.md` D-16).
- **Question:** Is company-wide manager access to all reports' visits an intentional flat/shared-oversight model (e.g., any manager can review any salesperson company-wide), or was the `ReportingManagerId` hierarchy intended to restrict a given manager to *only their own direct reports'* visits, with the current unrestricted behavior being an unintended gap? This materially affects whether `04_Security_and_Tenant_Audit.md` D-04/D-16 should be treated as an access-control defect to remediate, or documented as accepted/by-design behavior.

## 9. Is cross-company visibility ever intended for any role in this workflow?

- **Evidence:** `srch_dailyrpts.aspx`'s *list* query is `CompanyID`-scoped, implying visits should never be visible cross-company. But none of the *detail* endpoints (`LoadMegaModal` in either file, `GetVisitDetails`, `ProcessApproval`, expense approval) re-check `CompanyID` on the specific row being acted upon.
- **Question:** Should cross-company access ever be permitted (e.g., for a super-admin/HQ role not otherwise visible in these 8 files), or should `CompanyID` be enforced as an absolute boundary on every single query touching `tbl_SalesVisitReport`/`tbl_Expenses`, with no exceptions? This determines whether the missing `CompanyID` checks documented in `05_Potential_Defects.md` D-04 are a uniform hardening item or need role-aware nuance.

## 10. What is the intended relationship between `ExecutionDateTime` and the actual moment of data entry for past-logged visits?

- **Evidence:** `daily_rpt.aspx.cs :: btnSubmit_Click` (mode=`past`) sets `ExecutionDateTime = Convert.ToDateTime(txtVisitStart.Text.Trim())` — i.e., the *declared, user-entered* start time of the visit — rather than `GETDATE()`/`DateTime.Now` (the actual moment of form submission), which is what the calendar-execute flow uses instead.
- **Question:** Is `ExecutionDateTime` intended to represent "when the visit actually took place" (in which case the past-mode behavior is correct) or "when the system recorded/confirmed the execution" (in which case it should also capture `DateTime.Now`, perhaps in a separate `RecordedDate`/`LoggedAt` column, to distinguish backfilled entries from real-time ones)? This also interacts with the edit-lock 45-day rule (item 2), which is keyed off `VisitDate`, not `ExecutionDateTime` — clarifying the intended semantics of each date column would help resolve both ambiguities together.
