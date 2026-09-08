# Sales visit lifecycle

> Domain key: `sales-visit` · 5 page(s)

Planner, daily report, manager search, visit expenses. Narrative: docs/06, docs/07, docs/08 + sales-visit-workflow-audit/.

**Do not copy** AuthN/AuthZ, tenancy, or Ponytail rules here. Those live in [SHARED_CONTEXT](SHARED_CONTEXT.md).

| Page | Purpose | Master / class | Gate | Tables (unique) | Ops | Narrative |
|------|---------|----------------|------|-----------------|-----|-----------|
| `corporate/business/app/daily_rpt.aspx` | Sales Visit<br>QS: `date`, `end`, `mode`, `start` | Bill.Master / `daily_rpt` | SecurePage, CompanyID<br>`daily_reporting` | `tbl_login`, `tbl_SalesVisitReport`, `tbl_SystemNotification` | INSERT | [module](../07_Sales_Visit_Reporting.md) |
| `corporate/business/app/expense_entry.aspx` | Add Expense<br>QS: `visitId` | Bill.Master / `expense_entry` | SecurePage, CompanyID<br>any: visit_planner, vw_dailyrpts | `tbl_SalesVisitReport`, `tbl_Expenses` | INSERT | [module](../08_Expense_Management.md) |
| `corporate/business/app/srch_dailyrpts.aspx` | Search Daily Reports<br>QS: `dt`, `emp` | Bill.Master / `srch_dailyrpts` | SecurePage, CompanyID<br>`srch_dailyrpts` | `tbl_login`, `tbl_SalesVisitReport`, `tbl_SalesVisitResponses`, `tbl_Expenses`, `tbl_SystemNotification` | INSERT, UPDATE | [module](../07_Sales_Visit_Reporting.md) |
| `corporate/business/app/visit_planner.aspx` | My Visit Calendar<br>WM: `GetCalendarEvents` | Bill.Master / `visit_planner` | SecurePage, CompanyID, WebMethod<br>`visit_planner` | `tbl_SalesVisitReport`, `tbl_SystemNotification` | INSERT, UPDATE | [module](../06_Sales_Visit_Planner.md) |
| `corporate/business/app/vw_dailyrpts.aspx` | My Sales Visits | Bill.Master / `vw_dailyrpts` | SecurePage, CompanyID<br>`vw_dailyrpts` | `tbl_SalesVisitReport`, `tbl_SalesVisitResponses`, `tbl_Expenses`, `tbl_login` | INSERT, UPDATE | [module](../07_Sales_Visit_Reporting.md) |

## Page-unique notes

- `corporate/business/app/expense_entry.aspx`: Child of two menus: `RequiredAnyPermissionKeys` = visit_planner OR vw_dailyrpts.

<!-- NARRATIVE:BEGIN -->

## Page map (do not recopy the audit)

`visit_planner` (calendar WM `GetCalendarEvents`) → `daily_rpt` (QS `date`/`start`/`end`/`mode`) → `vw_dailyrpts` (owner list + thread) → manager `srch_dailyrpts` (QS `dt`/`emp`). Visit expenses `expense_entry?visitId=` upload `~/Uploads/Expenses/` — **not** GL `general_expences`.

SMTP still hardcoded on `vw_dailyrpts` / `srch_dailyrpts` (defect D-11 in the audit).
