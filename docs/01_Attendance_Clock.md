# Attendance & clock-in/out

Canonical pages: [`page-catalog/DOMAIN_attendance-hr.md`](page-catalog/DOMAIN_attendance-hr.md).  
AuthN/tenancy: [`page-catalog/SHARED_CONTEXT.md`](page-catalog/SHARED_CONTEXT.md).  
SPs: [`page-catalog/SHARED_RUNTIME.md`](page-catalog/SHARED_RUNTIME.md).

This file keeps **leftover facts** that are not in those catalogs.

---

## Paths (not `admin/`)

Live files are under `corporate/business/app/`: `attendance.aspx` (employee punch, menu `daily_attendance`) and `AdminAttendanceDashboard.aspx` (enterprise register). There is no `admin/AdminAttendanceDashboard.aspx`.

`AdminAttendanceDashboard` is **not** a `SecurePage` — it relies on Bill.Master session only.

---

## Tables (unique leftover)

| Table | Role here |
|-------|-----------|
| `tbl_Attendance` | Punches / daily status. |
| `tbl_SalesVisitReport` | **FieldSales CTE only** on the admin dashboard (`GeoLocationAddress`, `IsProductive`, `RevenueRealized`). Not the punch UI. |
| `tbl_login` | Employee list for the register. |

Visit GPS is captured on `visit_planner` execute, not on `attendance.aspx`. Join the two only in that CTE.

---

## Tenant leftover

Admin FieldSales aggregation filters `tbl_SalesVisitReport.CompanyID`. Visit INSERTs historically omitted `CompanyID` (audit **D-01**), so the rollup can under-count even when the CTE is correctly scoped.
