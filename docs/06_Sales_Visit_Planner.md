# Sales visit planner

Canonical pages: [`page-catalog/DOMAIN_sales-visit.md`](page-catalog/DOMAIN_sales-visit.md).  
State machine, columns, defects: [`sales-visit-workflow-audit/`](sales-visit-workflow-audit/).  
AuthN/tenancy: [`page-catalog/SHARED_CONTEXT.md`](page-catalog/SHARED_CONTEXT.md).

This file keeps **leftover facts** that are not in the catalog.

---

## Confirmed entity

Live table is **`tbl_SalesVisitReport`**. Page file is `visit_planner.aspx` (not `planner.aspx`). Calendar WM: `GetCalendarEvents`.

GPS (`Latitude` / `Longitude`) is written on **calendar execute** (`btnSubmitExecution_Click`). Past-logged visits (`daily_rpt.aspx?mode=past`) and owner edits (`vw_dailyrpts`) do not capture GPS.

Follow-up rows set `ParentVisitId` and **nothing in the app reads it** (audit **D-06**).

---

## Downstream links (unique)

Executed-visit UI can deep-link `expense_entry.aspx?visitId=` and `Create_quotation.aspx?visitId=`. Quote prefill filters `tbl_SalesVisitReport` by `CompanyID` — visit INSERTs that omit `CompanyID` (**D-01**) silently fail to prefill.
