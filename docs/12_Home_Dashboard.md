# Homepage dashboard

Canonical pages: [`page-catalog/DOMAIN_dashboard.md`](page-catalog/DOMAIN_dashboard.md) (`home.aspx`, `QuickAction.aspx`).  
AuthN/tenancy: [`page-catalog/SHARED_CONTEXT.md`](page-catalog/SHARED_CONTEXT.md).

This file keeps **leftover facts** that are not in the catalog.

---

## What `home.aspx` actually aggregates

Menu id **`home1`** (label “My Profile”; title “Dashboard”). Not `admin/home.aspx` (that is the **kiosk** home).

| KPI | Source |
|-----|--------|
| Punch status / days present | `tbl_Attendance` (`UserCode`, `AttendanceStatus`) |
| Visits / quotes / revenue today and this month | `tbl_SalesVisitReport` filtered by **`CreatedByCode` and `CompanyID`** |
| Toasts | `tbl_SystemNotification` |

There is no `tbl_SalesVisitReport`-only dashboard, and Chart.js is not the current KPI implementation to document as required.

Visit counts still depend on **D-01** (NULL `CompanyID` visits drop out of these queries) and **D-03** (`FLM03` mis-attribution). `LinkedQuotationNo` drives the “quotes” KPI — not a join to `tbl_Quotation`.

---

## `QuickAction.aspx`

Same domain, **not** the dashboard UI. Unauthenticated AES token `t` for leave / attendance-regularization approve/reject. Helpers: [`SHARED_RUNTIME.md`](page-catalog/SHARED_RUNTIME.md).
