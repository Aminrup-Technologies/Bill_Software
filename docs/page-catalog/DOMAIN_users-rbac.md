# Users, roles, permissions

> Domain key: `users-rbac` · 10 page(s)

Provisioning and RBAC admin. Narrative: docs/02, docs/03, docs/04, docs/16–18.

**Do not copy** AuthN/AuthZ, tenancy, or Ponytail rules here. Those live in [SHARED_CONTEXT](SHARED_CONTEXT.md).

| Page | Purpose | Master / class | Gate | Tables (unique) | Ops | Narrative |
|------|---------|----------------|------|-----------------|-----|-----------|
| `corporate/business/app/AddUser.aspx` | Add Employee | Bill.Master / `WebForm79` | SecurePage, CompanyID<br>`AddUser` | `Roles`, `tbl_Departments`, `tbl_Designations`, `tbl_login`, `tbl_SystemNotification`, `sp_AllocateEmployeeLeaves()` | INSERT, UPDATE | [module](../02_Employee_Admin.md) |
| `corporate/business/app/ManagePermissions.aspx` | Manage Pages & Permissions | Bill.Master / `ManagePermissions` | SecurePage<br>`ManagePermissions` | `Permissions`, `RolePermissions` | INSERT, UPDATE, DELETE | [module](../03_Role_Permissions.md) |
| `corporate/business/app/ManageRoles.aspx` | Manage Roles | Bill.Master / `ManageRoles` | SecurePage, CompanyID<br>`ManageRoles` | `permissions`, `Roles`, `RolePermissions` | INSERT, DELETE | [module](../03_Role_Permissions.md) |
| `corporate/business/app/Update/contactno.aspx` | contactno | none / `contactno` | Session<br>— | `tbl_login` | UPDATE | this catalog |
| `corporate/business/app/Update/emailid.aspx` | emailid | none / `emailid` | Session<br>— | `tbl_login` | UPDATE | this catalog |
| `corporate/business/app/Update/name.aspx` | name | none / `name` | Session<br>— | `tbl_login` | UPDATE | this catalog |
| `corporate/business/app/Update/password.aspx` | password | none / `password` | Session<br>— | `tbl_login` | UPDATE | this catalog |
| `corporate/business/app/Update_Designation.aspx` | Assign User Roles<br>QS: `User_Id` | Bill.Master / `WebForm81` | SecurePage, CompanyID<br>`ViewUser` | `Roles`, `tbl_login`, `UserRoles` | DELETE | [module](../04_Department_Designation.md) |
| `corporate/business/app/ViewUser.aspx` | View<br>WM: `SaveGeoFence` | Bill.Master / `WebForm80` | SecurePage, CompanyID, WebMethod<br>`ViewUser` | `ActiveSessions`, `tbl_login`, `Roles`, `tbl_Departments`, `tbl_Designations`, `tbl_SystemNotification` | INSERT, UPDATE, DELETE | [module](../02_Employee_Admin.md) |
| `corporate/business/app/settings.aspx` | Settings<br>QS: `pwd` | Bill.Master / `WebForm2` | Bill.Master<br>menu `settings` | `tbl_login` | UPDATE | this catalog |

## Page-unique notes

- `corporate/business/app/Update/contactno.aspx`: Legacy profile popup (no master). Same pattern as `Update/emailid`, `Update/name`, `Update/password`. Forced lockout uses `settings.aspx`.
- `corporate/business/app/Update/password.aspx`: Profile popup (no master). Forced-password flow redirects to `settings.aspx`, not these Update/* pages.
- `corporate/business/app/settings.aspx`: Lockout landing when `MustUpdateUserId` or `MustVerifyContact` is set (Bill.Master).
