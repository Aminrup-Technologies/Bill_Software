# Solution documentation map

AminrupERP / Flame-ex (`Bill_Software`) documentation is split so **shared architecture is written once** and **every `.aspx` page is listed once**.

**Coverage:** all **188** pages are in the census (Reviewed). Each of the **19** domains has a workflow narrative. Page↔table/SP verbs: [`page-catalog/DATA_DICTIONARY.md`](page-catalog/DATA_DICTIONARY.md). All UAT columns + SP signatures: [`page-catalog/UAT_CATALOG.md`](page-catalog/UAT_CATALOG.md). Handlers/helpers/SPs: [`page-catalog/SHARED_RUNTIME.md`](page-catalog/SHARED_RUNTIME.md). Live UAT objects: [`page-catalog/SHARED_SCHEMA.md`](page-catalog/SHARED_SCHEMA.md). Leftover `docs/01`–`docs/13` are pointers. Visit audit vs current code: [`sales-visit-workflow-audit/00_SNAPSHOT_STATUS.md`](sales-visit-workflow-audit/00_SNAPSHOT_STATUS.md).

**Still not in-repo:** ERD drawings, stored-procedure **bodies**, screenshot user manuals.

## Commercial spine (ERP)

```
Quotation (or Client PO) [tbl_Quotation]
  → DPCC [tbl_Chalan] and/or Proforma [tbl_Proforma]
  → Tax invoice [tbl_Invoice]
  → Payment received [tbl_invoice_payment]
Vendor PR [tbl_RequisitionMain] → Vendor PO [tbl_PO_Header] → Print_PO
Vendor purchase [tbl_Purches] → purchase payment [tbl_Purchess_payment]
Hydrant track (hidden menu): qsHydrentQuotation → HydrentInvoice
Visit track: visit_planner → daily_rpt → vw/srch_dailyrpts + expense_entry
  (table `tbl_SalesVisitReport`, not a table named daily_rpt)
GL expenses: tlb_General_expences / tbl_patty_cash_expenses (not visit expenses)
Card kiosk: tbl_card_login / tbl_employee (isolated)
```

## Start here

| If you need… | Open |
|--------------|------|
| How to review the next undocumented/stale page | [page-catalog/RECURSIVE_INSTRUCTIONS.md](page-catalog/RECURSIVE_INSTRUCTIONS.md) |
| Visit audit findings vs current `.cs` | [sales-visit-workflow-audit/00_SNAPSHOT_STATUS.md](sales-visit-workflow-audit/00_SNAPSHOT_STATUS.md) |
| Every UAT table column + SP signature | [page-catalog/UAT_CATALOG.md](page-catalog/UAT_CATALOG.md) |
| Which page talks to which table / SP (verbs) | [page-catalog/DATA_DICTIONARY.md](page-catalog/DATA_DICTIONARY.md) |
| Cross-cutting AuthN, tenancy, master page, print gate | [page-catalog/SHARED_CONTEXT.md](page-catalog/SHARED_CONTEXT.md) |
| Live UAT schema (tables, FKs, missing objects, SP inventory) | [page-catalog/SHARED_SCHEMA.md](page-catalog/SHARED_SCHEMA.md) |
| Handlers, ASCX, helpers, SQL scripts, SP call index | [page-catalog/SHARED_RUNTIME.md](page-catalog/SHARED_RUNTIME.md) |
| Census of all pages | [page-catalog/PAGE_INVENTORY.md](page-catalog/PAGE_INVENTORY.md) |
| Product + Ponytail + setup | [README.md](../README.md) |
| Security contract | [22_Security_Baseline.md](22_Security_Baseline.md) |
| Administrator impersonation (UAT flag + grant) | [ADR-001](ADR-001_Administrator_Impersonation.md), [29](29_Impersonation_Governance.md)–[36](36_Impersonation_UAT_Activation.md) |

## Page catalogs (unique facts only)

| Domain | Pages | File |
|--------|------:|------|
| Authentication & session | 4 | [page-catalog/DOMAIN_auth.md](page-catalog/DOMAIN_auth.md) |
| Home dashboard | 2 | [page-catalog/DOMAIN_dashboard.md](page-catalog/DOMAIN_dashboard.md) |
| Sales visit lifecycle | 5 | [page-catalog/DOMAIN_sales-visit.md](page-catalog/DOMAIN_sales-visit.md) |
| Attendance, leaves, shifts | 8 | [page-catalog/DOMAIN_attendance-hr.md](page-catalog/DOMAIN_attendance-hr.md) |
| Users, roles, permissions | 10 | [page-catalog/DOMAIN_users-rbac.md](page-catalog/DOMAIN_users-rbac.md) |
| Master data | 17 | [page-catalog/DOMAIN_masters.md](page-catalog/DOMAIN_masters.md) |
| Customers & factories | 9 | [page-catalog/DOMAIN_customer.md](page-catalog/DOMAIN_customer.md) |
| Principles & purchases | 11 | [page-catalog/DOMAIN_vendor-purchase.md](page-catalog/DOMAIN_vendor-purchase.md) |
| PR / PO | 18 | [page-catalog/DOMAIN_pr-po.md](page-catalog/DOMAIN_pr-po.md) |
| Quotations | 12 | [page-catalog/DOMAIN_quotation.md](page-catalog/DOMAIN_quotation.md) |
| DPCC / challan | 4 | [page-catalog/DOMAIN_dpcc-challan.md](page-catalog/DOMAIN_dpcc-challan.md) |
| Proforma invoices | 6 | [page-catalog/DOMAIN_proforma.md](page-catalog/DOMAIN_proforma.md) |
| Tax invoices | 7 | [page-catalog/DOMAIN_invoice.md](page-catalog/DOMAIN_invoice.md) |
| Hydrant invoices & quotations | 5 | [page-catalog/DOMAIN_hydrant.md](page-catalog/DOMAIN_hydrant.md) |
| Payments received & purchase payments | 12 | [page-catalog/DOMAIN_payments.md](page-catalog/DOMAIN_payments.md) |
| Payments made (GL expenses) | 7 | [page-catalog/DOMAIN_expenses-made.md](page-catalog/DOMAIN_expenses-made.md) |
| Stock & due reports | 5 | [page-catalog/DOMAIN_reports.md](page-catalog/DOMAIN_reports.md) |
| Print layouts | 29 | [page-catalog/DOMAIN_print.md](page-catalog/DOMAIN_print.md) |
| ID-card kiosk (isolated) | 17 | [page-catalog/DOMAIN_card-kiosk.md](page-catalog/DOMAIN_card-kiosk.md) |

## Module narratives (pointers + unique leftovers — do not duplicate catalogs)

| Doc | Topic |
|-----|--------|
| [01_Attendance_Clock.md](01_Attendance_Clock.md) | Attendance leftover (FieldSales CTE, D-01) |
| [02_Employee_Admin.md](02_Employee_Admin.md) | ERP user leftover (`tbl_login`, not `admin/`) |
| [03_Role_Permissions.md](03_Role_Permissions.md) | Dual role systems; SecurePage is a gate |
| [04_Department_Designation.md](04_Department_Designation.md) | No dept CRUD; `Update_Designation` = UserRoles |
| [05_Customer_Vendor.md](05_Customer_Vendor.md) | `tbl_Client` / `tbl_Vendor`; visit name is free text |
| [06_Sales_Visit_Planner.md](06_Sales_Visit_Planner.md) | Calendar GPS + D-06 ParentVisitId |
| [07_Sales_Visit_Reporting.md](07_Sales_Visit_Reporting.md) | Visit leftovers; D-03 live; D-01 historical rows |
| [08_Expense_Management.md](08_Expense_Management.md) | Visit `tbl_Expenses` ≠ GL expenses |
| [09_Quotation_Generation.md](09_Quotation_Generation.md) | Confirmed `tbl_Quotation`; visit prefill D-01 |
| [10_Purchase_Order.md](10_Purchase_Order.md) | Two PO stacks (vendor vs client) |
| [11_Communications.md](11_Communications.md) | Gateway vs direct SmtpClient |
| [12_Home_Dashboard.md](12_Home_Dashboard.md) | `home.aspx` KPIs + QuickAction |
| [13_Invoice_Search_View_PO_Discovery.md](13_Invoice_Search_View_PO_Discovery.md) | Dated discovery snapshot (catalogs win) |
| [sales-visit-workflow-audit/00_SNAPSHOT_STATUS.md](sales-visit-workflow-audit/00_SNAPSHOT_STATUS.md) | Visit audit vs current code |
| [sales-visit-workflow-audit/](sales-visit-workflow-audit/) | Dated visit architecture snapshot |
| [08_Cursor_Change_Audit.md](08_Cursor_Change_Audit.md) | Historical agent-change audit (not a module) |
| [14](14_Authentication_Authorization_Architecture.md)–[21](21_Phase3_Infrastructure_Hardening.md) | AuthZ phases (history) |
| [22_Security_Baseline.md](22_Security_Baseline.md) | Canonical security contract |
| [ADR-001](ADR-001_Administrator_Impersonation.md) | Impersonation stays off until flag + grant |
| [29](29_Impersonation_Governance.md)–[36](36_Impersonation_UAT_Activation.md) | Impersonation governance, runtime, UAT activation |

## Database Migrations

- [`user_company_access_seed.sql`](../db/user_company_access_seed.sql) — Seeds missing home-tenant memberships after Phase 2A rollout.
- [`SwitchUser_permission.sql`](../Bill_Software/corporate/business/sql/SwitchUser_permission.sql) — Catalog-only `SwitchUser` permission (no grant).
- [`SwitchUser_superadmin_grant_uat.sql`](../Bill_Software/corporate/business/sql/SwitchUser_superadmin_grant_uat.sql) — UAT Super Admin grant only (DBA; do not execute from the app).

## Invoice lineage (specialist, not page catalogs)

- [invoice_export_data_inventory.md](invoice_export_data_inventory.md)
- [invoice_insert_data_lineage.md](invoice_insert_data_lineage.md)
