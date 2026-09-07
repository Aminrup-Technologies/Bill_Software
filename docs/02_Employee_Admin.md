# Module 02 — Employee Administration (User Provisioning)

> Master Page Menu Position: **Administration → Add Employee** / **Administration → View Employees** (admin-role only)

---

## 1. Overview

The Employee Administration module handles user account provisioning, employee profile management, and user lifecycle operations. It is the primary interface for creating new employee accounts, assigning them to departments and designations, and managing their active/inactive status.

---

## 2. Files

| File | Type | Purpose |
|------|------|---------|
| `admin/AddUser.aspx` | Frontend | New employee account creation form |
| `admin/AddUser.aspx.cs` | Backend | User insertion logic, department/designation lookups, initial password setup |
| `admin/ViewUser.aspx` | Frontend | User management grid (list, edit roles, view details) |
| `admin/ViewUser.aspx.cs` | Backend | User listing, role assignment, profile management |

### Supporting Files

| File | Relationship |
|------|-------------|
| `admin/Update/` | Sub-pages for user update workflows |
| `DB_UTILITY.cs` | Shared database utilities |
| `Bill.Master.cs` | Menu visibility — `UserRoles` maintained here affects what this module can manage |

---

## 3. Core Database Tables

| Table | Usage in This Module |
|-------|---------------------|
| `tbl_login` | Primary entity — employee account records |
| `tbl_Departments` | Department lookup for dropdown population |
| `tbl_Designations` | Designation lookup for dropdown population |
| `Roles` | Role lookup for role assignment |
| `UserRoles` | Many-to-many user↔role mapping (maintained by `Update_Designation.aspx.cs`) |
| `tbl_SystemNotification` | Audit logging for user creation/updates |

### Key Columns Referenced

- `tbl_login.Id` — numeric surrogate PK (used as FK target for `UserRoles.UserId`)
- `tbl_login.User_Id` — business/natural key (string, e.g., "FLM035")
- `tbl_login.Name` — display name
- `tbl_login.Email` — email address
- `tbl_login.Password` — legacy plaintext password column
- `tbl_login.PasswordHash` / `PasswordSalt` — PBKDF2 hashed password
- `tbl_login.CompanyID` — tenant assignment
- `tbl_login.RoleId` — role assignment (cosmetic/display)
- `tbl_login.DepartmentID` — department assignment
- `tbl_login.DesignationID` — designation assignment
- `tbl_login.IsActive` — active/inactive flag
- `tbl_login.ReportingManagerId` — self-referencing FK to manager

### Stored Procedure

- **`sp_AllocateEmployeeLeaves`** — called from `AddUser.aspx.cs`; the only stored procedure observed in the entire codebase. Allocates initial leave balances for a newly created employee.

---

## 4. Multi-Tenant Constraints

| Constraint | Status | Evidence |
|-----------|--------|----------|
| `CompanyID` filter on user creation | ✅ Enforced | `AddUser.aspx.cs` populates `tbl_login.CompanyID` from `CompanyContext.CurrentCompanyID` on new user insert |
| `CompanyID` filter on user listing | ✅ Enforced | New user creation scoped to current company |

### Tenant Isolation Pattern

User provisioning is correctly tenant-scoped: new employees are created within the current company context. The `CompanyID` is populated from `CompanyContext.CurrentCompanyID` at insert time, ensuring new users belong to the correct tenant.

---

## 5. Proactive Notification Triggers

| Trigger | Table | Description |
|---------|-------|-------------|
| User creation | `tbl_SystemNotification` | A notification entry is logged when a new user account is provisioned, prior to the transaction commit. |

---

## 6. Architectural Notes

- `AddUser.aspx.cs` is one of the files that correctly demonstrates the `CompanyId` segregation pattern (referenced in code comments as "Full-Stack CompanyContext segregation fix").
- The stored procedure `sp_AllocateEmployeeLeaves` is the **only SP** observed in the entire codebase — all other data access is inline ad-hoc SQL via ADO.NET.
- `ViewUser.aspx.cs` manages the `tbl_login.RoleId` field, which is populated into `Session["RoleId"]`/`Session["RoleName"]` at login — but these session values are **never read elsewhere** for authorization. Role-based access is instead controlled by the `UserRoles`/`RolePermissions` mechanism in `Bill.Master.cs`.
