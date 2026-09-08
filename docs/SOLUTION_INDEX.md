# Solution documentation map

AminrupERP / Flame-ex (`Bill_Software`) documentation is split so **shared architecture is written once** and **every `.aspx` page is listed once**.

**Coverage:** all **188** pages are in the census. Each of the **19** domains has a workflow + page-behavior narrative (not a duplicate of AuthN/tenancy). Deep specialist write-ups remain in `docs/01`–`docs/22` and are linked from the catalogs.

## Commercial spine (ERP)

```
Quotation (or Client PO) [tbl_Quotation]
  → DPCC [tbl_Chalan] and/or Proforma [tbl_Proforma]
  → Tax invoice [tbl_Invoice]
  → Payment received [tbl_invoice_payment]
Vendor PR [tbl_RequisitionMain] → Vendor PO [tbl_PO_Header] → Print_PO
Vendor purchase [tbl_Purches] → purchase payment [tbl_Purchess_payment]
Hydrant track (hidden menu): qsHydrentQuotation → HydrentInvoice
Visit track: planner → daily_rpt → vw/srch_dailyrpts + expense_entry
GL expenses: tlb_General_expences / tbl_patty_cash_expenses (not visit expenses)
Card kiosk: tbl_card_login / tbl_employee (isolated)
```

## Start here

| If you need… | Open |
|--------------|------|
| How to review the next undocumented/stale page | [page-catalog/RECURSIVE_INSTRUCTIONS.md](page-catalog/RECURSIVE_INSTRUCTIONS.md) |
| Cross-cutting AuthN, tenancy, master page, print gate | [page-catalog/SHARED_CONTEXT.md](page-catalog/SHARED_CONTEXT.md) |
| Census of all pages | [page-catalog/PAGE_INVENTORY.md](page-catalog/PAGE_INVENTORY.md) |
| Product + Ponytail + setup | [README.md](../README.md) |
| Security contract | [22_Security_Baseline.md](22_Security_Baseline.md) |

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

## Module narratives (do not duplicate into catalogs)

| Doc | Topic |
|-----|--------|
| [01_Attendance_Clock.md](01_Attendance_Clock.md) | Attendance |
| [02_Employee_Admin.md](02_Employee_Admin.md) | User provisioning |
| [03_Role_Permissions.md](03_Role_Permissions.md) | Roles / permissions |
| [04_Department_Designation.md](04_Department_Designation.md) | Dept / designation |
| [05_Customer_Vendor.md](05_Customer_Vendor.md) | Customer / vendor directory |
| [06_Sales_Visit_Planner.md](06_Sales_Visit_Planner.md) | Visit calendar |
| [07_Sales_Visit_Reporting.md](07_Sales_Visit_Reporting.md) | Daily reports / approval |
| [08_Expense_Management.md](08_Expense_Management.md) | Visit expenses |
| [09_Quotation_Generation.md](09_Quotation_Generation.md) | Quotations |
| [10_Purchase_Order.md](10_Purchase_Order.md) | Purchase orders |
| [11_Communications.md](11_Communications.md) | Email / SMS |
| [12_Home_Dashboard.md](12_Home_Dashboard.md) | Home KPIs |
| [13_Invoice_Search_View_PO_Discovery.md](13_Invoice_Search_View_PO_Discovery.md) | Invoice search/view discovery |
| [sales-visit-workflow-audit/](sales-visit-workflow-audit/) | Visit architecture audit |
| [14](14_Authentication_Authorization_Architecture.md)–[21](21_Phase3_Infrastructure_Hardening.md) | AuthZ phases (history) |
| [22_Security_Baseline.md](22_Security_Baseline.md) | Canonical security contract |

## Invoice lineage (specialist, not page catalogs)

- [invoice_export_data_inventory.md](invoice_export_data_inventory.md)
- [invoice_insert_data_lineage.md](invoice_insert_data_lineage.md)
