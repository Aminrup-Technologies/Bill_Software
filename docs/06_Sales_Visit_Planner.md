# Sales visit planner

Canonical pages: [`page-catalog/DOMAIN_sales-visit.md`](page-catalog/DOMAIN_sales-visit.md).  
Snapshot vs current code: [`sales-visit-workflow-audit/00_SNAPSHOT_STATUS.md`](sales-visit-workflow-audit/00_SNAPSHOT_STATUS.md).  
AuthN/tenancy: [`page-catalog/SHARED_CONTEXT.md`](page-catalog/SHARED_CONTEXT.md).

This file keeps **leftover facts** that are not in the catalog.

---

## Confirmed entity

Live table is **`tbl_SalesVisitReport`**. Page file is `visit_planner.aspx` (not `planner.aspx`). Calendar WM: `GetCalendarEvents`. Execute/detail paths call `AuthGuard.UserOwnsVisit`.

GPS (`Latitude` / `Longitude`) is written on **calendar execute**. Past-logged visits (`daily_rpt.aspx?mode=past`) and owner edits (`vw_dailyrpts`) do not capture GPS.

Follow-up INSERT copies `@CompanyID` and sets `ParentVisitId`. **Nothing in the UI reads `ParentVisitId`** (audit D-06 — not re-verified this pass).

---

## Downstream links (unique)

Executed-visit UI can deep-link `expense_entry.aspx?visitId=` and `Create_quotation.aspx?visitId=`. Quote prefill still filters by `CompanyID`; **historical NULL `CompanyID` visits** fail to prefill. New INSERTs set the column.
