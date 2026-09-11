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

`home.aspx` sales KPIs filter `CompanyID = CompanyContext.CurrentCompanyID`. **New** visit INSERTs populate `CompanyID` (`daily_rpt`, planner follow-up). On **`flamex_uat`** the column is `int NOT NULL DEFAULT (1)` and **all 78 visits are CompanyID=1 with 0 NULLs** — the historical-NULL D-01 leftover is not in this copy. It can still exist on an older live backup. Schema: [`SHARED_SCHEMA.md`](page-catalog/SHARED_SCHEMA.md).

**D-03** is still live: `daily_rpt` null-coalesces `CreatedByCode` to `"FLM03"`. UAT has **2** visit rows with that code.

`LinkedQuotationNo` drives the quotes count — not a join to `tbl_Quotation`.

---

## `QuickAction.aspx`

Same domain, **not** the dashboard UI. AES token `t`. Helpers: [`SHARED_RUNTIME.md`](page-catalog/SHARED_RUNTIME.md).
