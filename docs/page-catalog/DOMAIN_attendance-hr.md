# Attendance, leaves, shifts

> Domain key: `attendance-hr` · 8 page(s)

Clock-in, leave, shift setup, admin override. Narrative: docs/01.

**Do not copy** AuthN/AuthZ, tenancy, or Ponytail rules here. Those live in [SHARED_CONTEXT](SHARED_CONTEXT.md).

| Page | Purpose | Master / class | Gate | Tables (unique) | Ops | Narrative |
|------|---------|----------------|------|-----------------|-----|-----------|
| `corporate/business/app/AdminApprovalDashboard.aspx` | Approvals Dashboard | Bill.Master / `AdminApprovalDashboard` | SecurePage, CompanyID<br>`AdminApprovalDashboard` | `tbl_AttendanceRegularization`, `tbl_login`, `tbl_LeaveRequests`, `tbl_LeaveMaster`, `tbl_EmployeeLeaveBalance`, `tbl_Attendance`, `tbl_SystemNotification` | INSERT, UPDATE | this catalog |
| `corporate/business/app/AdminAttendanceDashboard.aspx` | Enterprise Attendance Register | Bill.Master / `AdminAttendanceDashboard` | Bill.Master, CompanyID<br>— | `tbl_login`, `tbl_Attendance`, `tbl_SalesVisitReport`, `tbl_LeaveRequests`, `tbl_LeaveMaster`, `tbl_AttendanceRegularization`, `tbl_SystemNotification` | INSERT, UPDATE, DELETE | [module](../01_Attendance_Clock.md) |
| `corporate/business/app/AdminLeaveSetup.aspx` | Leave Policy Setup | Bill.Master / `AdminLeaveSetup` | SecurePage<br>`AdminLeaveSetup` | `tbl_LeaveMaster` | INSERT | this catalog |
| `corporate/business/app/AdminOverride.aspx` | HR Override Dashboard | Bill.Master / `AdminOverride` | SecurePage, CompanyID<br>`AdminOverride` | `roles`, `tbl_LeaveRequests`, `tbl_login`, `tbl_LeaveMaster`, `tbl_AttendanceRegularization`, `tbl_EmployeeLeaveBalance`, `tbl_Attendance`, `tbl_SystemNotification` | INSERT, UPDATE | this catalog |
| `corporate/business/app/AdminShiftAssignment.aspx` | Shift Assignment | Bill.Master / `AdminShiftAssignment` | SecurePage, CompanyID<br>`AdminShiftSetup` | `tbl_login`, `tbl_ShiftMaster`, `tbl_EmployeeShiftMapping`, `tbl_SystemNotification`, `sp_RunAttendanceRulesEngine()` | INSERT, DELETE | this catalog |
| `corporate/business/app/AdminShiftSetup.aspx` | Shift Setup | Bill.Master / `AdminShiftSetup` | SecurePage, CompanyID<br>`AdminShiftSetup` | `tbl_ShiftMaster`, `tbl_SystemNotification` | INSERT, UPDATE | this catalog |
| `corporate/business/app/MyLeaves.aspx` | My Leaves | Bill.Master / `MyLeaves` | Bill.Master, CompanyID<br>menu `my_leaves` | `tbl_LeaveMaster`, `tbl_EmployeeLeaveBalance`, `tbl_LeaveRequests`, `tbl_login`, `tbl_SystemNotification` | INSERT | this catalog |
| `corporate/business/app/attendance.aspx` | Daily Attendance<br>WM: `GetMonthlyData`, `GetCalendarData`, `ProcessPunch`, `GetActiveLeaveTypes`, `GetAttendanceDetails`, `GetShiftTimings` | Bill.Master / `attendance` | Bill.Master, CompanyID, WebMethod<br>menu `daily_attendance` | `tbl_ShiftMaster`, `tbl_EmployeeShiftMapping`, `tbl_Attendance`, `tbl_AttendanceRegularization`, `tbl_HolidayMaster`, `tbl_LeaveRequests`, `tbl_LeaveMaster`, `tbl_login` | INSERT, UPDATE | [module](../01_Attendance_Clock.md) |

## Page-unique notes

- `corporate/business/app/AdminApprovalDashboard.aspx`: HR approvals (leave + regularization), not sales-visit approval. Sales-visit approval is `srch_dailyrpts`.
