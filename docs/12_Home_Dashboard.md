# Homepage dashboard

Canonical pages: [`page-catalog/DOMAIN_dashboard.md`](page-catalog/DOMAIN_dashboard.md).  
AuthN/tenancy: [`page-catalog/SHARED_CONTEXT.md`](page-catalog/SHARED_CONTEXT.md).

This file keeps **leftover facts** that are not in the catalog.

---

## Paths

Menu id **`home1`**. Not `admin/home.aspx` (kiosk).

Chart.js is **not** the current KPI implementation.

---

## Historical visit rows

`home.aspx` sales KPIs filter `CompanyID = CompanyContext.CurrentCompanyID`. **New** visit INSERTs populate `CompanyID` (`daily_rpt`, planner follow-up). Rows created before that change with NULL `CompanyID` still vanish from these KPIs (and from FieldSales CTE / manager list). That is leftover data, not a missing INSERT column.

**D-03** is still live: `daily_rpt` null-coalesces `CreatedByCode` to `"FLM03"`.

`LinkedQuotationNo` drives the quotes count — not a join to `tbl_Quotation`.

---

## `QuickAction.aspx`

Same domain, **not** the dashboard UI. AES token `t`. Helpers: [`SHARED_RUNTIME.md`](page-catalog/SHARED_RUNTIME.md).
