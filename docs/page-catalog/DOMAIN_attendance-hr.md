# Attendance, leaves, shifts

> Domain key: `attendance-hr` · 8 page(s)

Clock-in, leave, shift setup, admin override. Narrative: docs/01.

**Do not copy** AuthN/AuthZ, tenancy, or Ponytail rules here. Those live in [SHARED_CONTEXT](SHARED_CONTEXT.md).

| Page | Purpose | Master / class | Gate | Tables (unique) | Ops | Narrative |
|------|---------|----------------|------|-----------------|-----|-----------|
| `corporate/business/app/AdminApprovalDashboard.aspx` | Approvals Dashboard | Bill.Master / `AdminApprovalDashboard` | SecurePage, CompanyID<br>`AdminApprovalDashboard` | `tbl_AttendanceRegularization`, `tbl_login`, `tbl_LeaveRequests`, `tbl_LeaveMaster`, `tbl_EmployeeLeaveBalance`, `tbl_Attendance`, `tbl_SystemNotification`, `sp_RunAttendanceRulesEngine()` | INSERT, UPDATE | this catalog |
| `corporate/business/app/AdminAttendanceDashboard.aspx` | Enterprise Attendance Register | Bill.Master / `AdminAttendanceDashboard` | SecurePage, CompanyID<br>`AdminAttendanceDashboard` | `tbl_login`, `tbl_Attendance`, `tbl_SalesVisitReport`, `tbl_LeaveRequests`, `tbl_LeaveMaster`, `tbl_AttendanceRegularization`, `tbl_SystemNotification` | INSERT, UPDATE, DELETE | [module](../01_Attendance_Clock.md) |
| `corporate/business/app/AdminLeaveSetup.aspx` | Leave Policy Setup | Bill.Master / `AdminLeaveSetup` | SecurePage<br>`AdminLeaveSetup` | `tbl_LeaveMaster` | INSERT | this catalog |
| `corporate/business/app/AdminOverride.aspx` | HR Override Dashboard | Bill.Master / `AdminOverride` | SecurePage, CompanyID<br>`AdminOverride` | `roles`, `tbl_LeaveRequests`, `tbl_login`, `tbl_LeaveMaster`, `tbl_AttendanceRegularization`, `tbl_EmployeeLeaveBalance`, `tbl_Attendance`, `tbl_SystemNotification`, `sp_RunAttendanceRulesEngine()` | INSERT, UPDATE | this catalog |
| `corporate/business/app/AdminShiftAssignment.aspx` | Shift Assignment | Bill.Master / `AdminShiftAssignment` | SecurePage, CompanyID<br>`AdminShiftSetup` | `tbl_login`, `tbl_ShiftMaster`, `tbl_EmployeeShiftMapping`, `tbl_SystemNotification`, `sp_RunAttendanceRulesEngine()` | INSERT, DELETE | this catalog |
| `corporate/business/app/AdminShiftSetup.aspx` | Shift Setup | Bill.Master / `AdminShiftSetup` | SecurePage, CompanyID<br>`AdminShiftSetup` | `tbl_ShiftMaster`, `tbl_SystemNotification` | INSERT, UPDATE | this catalog |
| `corporate/business/app/MyLeaves.aspx` | My Leaves | Bill.Master / `MyLeaves` | SecurePage, CompanyID<br>`my_leaves` | `tbl_LeaveMaster`, `tbl_EmployeeLeaveBalance`, `tbl_LeaveRequests`, `tbl_login`, `tbl_SystemNotification` | INSERT | this catalog |
| `corporate/business/app/attendance.aspx` | Daily Attendance<br>WM: `GetMonthlyData`, `GetCalendarData`, `ProcessPunch`, `GetActiveLeaveTypes`, `GetAttendanceDetails`, `GetShiftTimings` | Bill.Master / `attendance` | SecurePage, CompanyID, WebMethod<br>`daily_attendance` | `tbl_ShiftMaster`, `tbl_EmployeeShiftMapping`, `tbl_Attendance`, `tbl_AttendanceRegularization`, `tbl_HolidayMaster`, `tbl_LeaveRequests`, `tbl_LeaveMaster`, `tbl_login` | INSERT, UPDATE | [module](../01_Attendance_Clock.md) |

## Page-unique notes

- `corporate/business/app/AdminApprovalDashboard.aspx`: HR approvals (leave + regularization), not sales-visit approval. Sales-visit approval is `srch_dailyrpts`. `sp_RunAttendanceRulesEngine` runs only after a successful regularization approval, on the same `SqlTransaction` as the approval write. The UPDATE requires `RequestStatus = 'Pending'`; `@@ROWCOUNT = 0` returns no result set, C# rolls back, and the engine and `Commit` are not reached. Leave approve/reject and regularization reject do not call the engine.

<!-- NARRATIVE:BEGIN -->

## Clock and calendar

`attendance.aspx` is the employee punch UI. Gated by `SecurePage` / `RequiredPermissionKey = daily_attendance` (same menu `id`). WebMethods keep `AuthGuard.EnsureWebMethod` (session + company membership; not permission-key). Methods: `GetMonthlyData`, `GetCalendarData`, `ProcessPunch`, `GetActiveLeaveTypes`, `GetAttendanceDetails`, `GetShiftTimings`, plus `GetMyGeoFence`, `SubmitRegularization`, `SubmitLeave`. Tables: `tbl_Attendance`, `tbl_AttendanceRegularization`, `tbl_ShiftMaster`, `tbl_EmployeeShiftMapping`, `tbl_HolidayMaster`, `tbl_LeaveRequests`. Dashboard rollup: [docs/01](../01_Attendance_Clock.md) (`AdminAttendanceDashboard` — FieldSales CTE; can INSERT/UPDATE/DELETE attendance; SecurePage key `AdminAttendanceDashboard`; EmpConfig menu).

## Leave and shifts

- **`MyLeaves`:** apply leave; emails manager a `QuickAction.aspx?t=` token (Approve/Reject). Gated by `SecurePage` / `RequiredPermissionKey = my_leaves` (same menu `id`).
- **`AdminLeaveSetup`:** INSERT `tbl_LeaveMaster` with **no CompanyID**.
- **`AdminShiftSetup`:** CRUD `tbl_ShiftMaster` + notifications.
- **`AdminShiftAssignment`:** maps employees; permission key is **`AdminShiftSetup`** (shared). May run `sp_RunAttendanceRulesEngine` after a shift-map change (not an approval path).
- **`AdminAttendanceDashboard`:** enterprise attendance register (INSERT/UPDATE/DELETE). SecurePage key **`AdminAttendanceDashboard`**; EmpConfig menu id matches. Catalog row in `employee_config_permissions.sql` (no RolePermissions grant).
- **`AdminOverride`:** HR resolve leave/reg; can mint QuickAction tokens for managers. `sp_RunAttendanceRulesEngine` runs only after a successful regularization approval, on the approval `SqlTransaction`. UPDATE requires `RequestStatus = 'Pending'`; zero rows `THROW 50001` into the C# rollback handler (no engine, no `Commit`, no success alert). Leave approve/reject and regularization reject do not call the engine.
- **`AdminApprovalDashboard`:** pending-only approve/reject. Same engine rule (successful regularization approval only). Zero pending rows abort via `@@ROWCOUNT = 0` (no result set → rollback) before the engine and `Commit`. Leave approve/reject and regularization reject do not call the engine.
- **`QuickAction`:** token page is catalogued in [DOMAIN_dashboard.md](DOMAIN_dashboard.md). `Type=Reg` + Approve calls `sp_RunAttendanceRulesEngine` on the approval `SqlTransaction`. UPDATE requires `RequestStatus = 'Pending'`; zero rows `THROW 50001` into the C# rollback handler. Leave approve/reject and regularization reject do not call the engine.

Sales-visit approval remains `srch_dailyrpts`. Working status and deferred resilience checks: [HANDOFF.md](../HANDOFF.md).
