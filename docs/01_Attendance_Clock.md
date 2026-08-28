# Module 01 — Attendance & Clock-In/Out

> Master Page Menu Position: **Administration → Attendance Dashboard** (admin-role only)

---

## 1. Overview

The Attendance module provides clock-in/clock-out functionality with GPS geolocation capture for field-sales personnel. The Admin Attendance Dashboard aggregates attendance data across all employees within a company, providing daily attendance rollups and field-sales activity summaries.

---

## 2. Files

| File | Type | Purpose |
|------|------|---------|
| `admin/AdminAttendanceDashboard.aspx` | Frontend | Admin-facing attendance grid and field-sales rollup |
| `admin/AdminAttendanceDashboard.aspx.cs` | Backend | ADO.NET queries for attendance aggregation, field-sales CTE |

### Supporting Files

| File | Relationship |
|------|-------------|
| `corporate/business/app/visit_planner.aspx[.cs]` | Visit execution path captures GPS data that feeds into attendance metrics |
| `DB_UTILITY.cs` | Database connection helpers used by attendance queries |

---

## 3. Core Database Tables

| Table | Usage in This Module |
|-------|---------------------|
| `tbl_SalesVisitReport` | Queried for field-sales activity metrics (`GeoLocationAddress`, `IsProductive`, `RevenueRealized`); scoped by `CompanyID` |
| `tbl_login` | Employee directory; filtered by `CompanyID` for the attendance grid |
| `ActiveSessions` | Session validation (indirect, via Master Page) |

### Key Columns Referenced

- `tbl_SalesVisitReport.CompanyID` — tenant scoping for aggregated queries
- `tbl_SalesVisitReport.GeoLocationAddress` — field location text
- `tbl_SalesVisitReport.IsProductive` — visit productivity flag
- `tbl_SalesVisitReport.RevenueRealized` — revenue associated with a visit
- `tbl_login.CompanyID` — tenant scoping for employee list
- `tbl_login.IsActive` — active employee filter

---

## 4. Multi-Tenant Constraints

| Constraint | Status | Evidence |
|-----------|--------|----------|
| `CompanyID` filter on `tbl_SalesVisitReport` queries | ✅ Enforced | `AdminAttendanceDashboard.aspx.cs` includes `WHERE CompanyID = @CompanyID` in the FieldSales CTE aggregation |
| `CompanyID` filter on `tbl_login` queries | ✅ Enforced | Employee list filtered by `CompanyID` |

### Tenant Isolation Pattern

The admin attendance dashboard is correctly tenant-scoped. All aggregation queries use `CompanyID` as a boundary, ensuring each company sees only its own attendance and field-sales data. This follows the standard `CompanyContext.CurrentCompanyID` pattern.

---

## 5. Proactive Notification Triggers

| Trigger | Table | Description |
|---------|-------|-------------|
| None identified | — | This module is read-only (dashboard/aggregation). No CRUD operations trigger `tbl_SystemNotification` inserts. |

---

## 6. Architectural Notes

- The `FieldSales` CTE in `AdminAttendanceDashboard.aspx.cs` cross-references `tbl_SalesVisitReport` with `tbl_login` using a `CompanyID`-scoped join.
- Attendance metrics include a `COUNT(Id)` and `SUM(RevenueRealized)` aggregation, making this module sensitive to the D-01 defect (`CompanyID` not populated on Sales Visit inserts) — visits created by the Sales Visit workflow without a `CompanyID` will be **invisible** to this dashboard.
- The module references `tbl_SalesVisitReport` columns (`GeoLocationAddress`, `IsProductive`, `RevenueRealized`) that are **not** used by the Sales Visit workflow pages themselves — these are additional columns on the table used only by this dashboard and possibly other admin views.
