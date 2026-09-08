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

## `QuickAction.aspx`

No master. QS `t` → `SecurityHelper.DecryptFromUrlToken` → `ReqID`, `Type` (`Leave`|`Reg`), `Action` (`Approve`|`Reject`), `ManagerID`, `CompanyID`. Possession of the link is the credential. Updates leave request or regularization, then leave-balance / attendance punch side-effects, then notifies the employee. Issued from `MyLeaves` / `AdminOverride`.
