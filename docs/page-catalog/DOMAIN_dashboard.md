# Home dashboard

> Domain key: `dashboard` · 2 page(s)

Landing KPIs. Narrative: docs/12_Home_Dashboard.md.

**Do not copy** AuthN/AuthZ, tenancy, or Ponytail rules here. Those live in [SHARED_CONTEXT](SHARED_CONTEXT.md).

| Page | Purpose | Master / class | Gate | Tables (unique) | Ops | Narrative |
|------|---------|----------------|------|-----------------|-----|-----------|
| `corporate/business/app/QuickAction.aspx` | Quick Action<br>QS: `t` | none / `QuickAction` | token-link, CompanyID<br>— | `tbl_LeaveRequests`, `tbl_AttendanceRegularization`, `tbl_EmployeeLeaveBalance`, `tbl_Attendance`, `tbl_login`, `tbl_SystemNotification` | INSERT, UPDATE | this catalog |
| `corporate/business/app/home.aspx` | Dashboard / My Profile | Bill.Master / `WebForm1` | Bill.Master, CompanyID<br>menu `home1` | `tbl_SystemNotification`, `tbl_login`, `tbl_Departments`, `tbl_Designations`, `tbl_Attendance`, `tbl_SalesVisitReport` | SELECT | [module](../12_Home_Dashboard.md) |

## Page-unique notes

- `corporate/business/app/QuickAction.aspx`: Unauthenticated email action: QS `t` decrypts to ReqID/Type/Action/ManagerID/CompanyID for leave or regularization approve/reject.
- `corporate/business/app/home.aspx`: Menu id `home1` label is My Profile; page title is Dashboard. Same file.

<!-- NARRATIVE:BEGIN -->

## `home.aspx`

Menu id **`home1`** (label “My Profile”; title “Dashboard”). Not kiosk `admin/home.aspx`. Labels, not Chart.js.

| KPI | Source |
|-----|--------|
| Punch status / days present | `tbl_Attendance` (`UserCode`) |
| Visits / quotes / revenue today and this month | `tbl_SalesVisitReport` where `CreatedByCode` **and** `CompanyID` |
| Toasts | `tbl_SystemNotification` |

Quote KPI is `LinkedQuotationNo` (non-blank), not a join to `tbl_Quotation`. New visit INSERTs set `CompanyID`. **UAT has no NULL visit CompanyID** (all 78 = 1). `FLM03` fallback on `daily_rpt` (**D-03**, 2 UAT rows) can still mis-attribute. Leftover: [docs/12](../12_Home_Dashboard.md). Schema: [SHARED_SCHEMA.md](SHARED_SCHEMA.md).

## `QuickAction.aspx`

No master. QS `t` → `SecurityHelper.DecryptFromUrlToken` → `ReqID`, `Type` (`Leave`|`Reg`), `Action` (`Approve`|`Reject`), `ManagerID`, `CompanyID`. Possession of the link is the credential. Updates leave request or regularization, then leave-balance / attendance punch side-effects, then notifies the employee. Issued from `MyLeaves` / `AdminOverride`.
