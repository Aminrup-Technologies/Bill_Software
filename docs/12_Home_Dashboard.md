# Module 12 — Homepage Dashboard & KPIs

> Master Page Menu Position: **Home** (primary navigation item, all authenticated users)

---

## 1. Overview

The Homepage Dashboard provides at-a-glance KPIs and summaries for field-sales personnel. It displays today's visit count, monthly visit totals, revenue realized, and other operational metrics drawn from the `tbl_SalesVisitReport` table. The dashboard serves as the landing page after login and the primary orientation point for the application.

---

## 2. Files

| File | Type | Purpose |
|------|------|---------|
| `corporate/business/app/home.aspx` | Frontend | Dashboard layout, KPI cards, chart containers |
| `corporate/business/app/home.aspx.cs` | Backend | KPI aggregation queries (`COUNT(Id)`, `SUM(RevenueRealized)`) |

### Supporting Files

| File | Relationship |
|------|-------------|
| `Bill.Master[.cs]` | Master Page — shell layout, navigation, session validation |
| `Chart.js` (Scripts/) | Client-side charting library for KPI visualization |
| `DB_UTILITY.cs` | Database connection utilities |

---

## 3. Core Database Tables

| Table | Usage in This Module |
|-------|---------------------|
| `tbl_SalesVisitReport` | Primary data source — `COUNT(Id)` for visit counts, `SUM(RevenueRealized)` for revenue KPIs |
| `tbl_login` | Current user identity for scoping queries |

### Key Queries

| Query | Purpose | Filter |
|-------|---------|--------|
| `COUNT(Id)` | Today's visit count | `WHERE CreatedByCode = @UserId AND CAST(VisitDate AS DATE) = @Today` |
| `COUNT(Id)` | This month's visit count | `WHERE CreatedByCode = @UserId AND MONTH(VisitDate) = @Month AND YEAR(VisitDate) = @Year` |
| `SUM(RevenueRealized)` | Revenue realized (this month) | `WHERE CreatedByCode = @UserId AND ...` |

---

## 4. Multi-Tenant Constraints

| Constraint | Status | Evidence |
|-----------|--------|----------|
| `CreatedByCode` filter on KPI queries | ✅ Enforced | Dashboard queries scope by the current user's `CreatedByCode` |
| `CompanyID` filter | ❌ **Not applied** | Dashboard is self-service (own data only), so `CompanyID` scoping is not strictly necessary — but consistency with the broader pattern would recommend it |

### Tenant Isolation Pattern

The dashboard is a **self-service view** — it shows only the current user's own KPIs, scoped by `CreatedByCode = @UserId`. Since the query already filters to a single user, `CompanyID` scoping is redundant (a user can only belong to one company). However, the `RevenueRealized` metric is impacted by the D-01 defect: visits created without a `CompanyID` will still appear in this user-scoped query, but they may be absent from company-level rollups (e.g., `AdminAttendanceDashboard`).

---

## 5. Proactive Notification Triggers

| Trigger | Table | Description |
|---------|-------|-------------|
| None | — | This module is read-only (dashboard/aggregation). No CRUD operations trigger `tbl_SystemNotification` inserts. |

---

## 6. Architectural Notes

### KPI Accuracy

Dashboard KPIs are derived from `tbl_SalesVisitReport` aggregations. The accuracy of these KPIs depends on:

1. **`CreatedByCode` consistency** — visits created with the hardcoded `"FLM03"` fallback (Defect D-03) during session expiry would appear under the wrong user's dashboard.
2. **`RevenueRealized` population** — this column is not referenced by the Sales Visit Workflow Audit's analyzed files, so its population source and completeness are uncertain.
3. **Visit status filtering** — the dashboard appears to count all visits regardless of `ApprovalStatus` or `VisitPhase`, meaning Planned visits that were never executed are counted alongside completed visits.

### Navigation Hub

The homepage serves as the navigation hub connecting to all major modules:
- Sales Visit Planner (calendar view)
- My Sales Visits (visit list)
- Manager Dashboard (for authorized users)
- Expense Management
- Admin modules (for admin users)

### No Server-Side Validation Concerns

As a read-only dashboard, this module has no server-side validation or mutation concerns. The primary risk is data accuracy based on upstream data quality issues (D-01, D-03).
