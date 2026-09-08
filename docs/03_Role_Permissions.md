# Roles and permissions

Canonical pages: [`page-catalog/DOMAIN_users-rbac.md`](page-catalog/DOMAIN_users-rbac.md) (`ManageRoles`, `ManagePermissions`, `Update_Designation`).  
Whole-app AuthZ: [`docs/14_Authentication_Authorization_Architecture.md`](14_Authentication_Authorization_Architecture.md) and [`docs/22_Security_Baseline.md`](22_Security_Baseline.md).

This file keeps **leftover facts** that are not in those catalogs.

---

## Two independent role mechanisms (still unreconciled)

| Mechanism | Storage | What it actually does |
|-----------|---------|------------------------|
| `tbl_login.RoleId` | Single FK; copied to `Session["RoleId"]` at login | Cosmetic (header / grids). **Not** the page gate. |
| `UserRoles` + `RolePermissions` + `Permissions` | M2M; `UserRoles.UserId` → `tbl_login.Id` (numeric), **not** `User_Id` | Menu visibility in Bill.Master **and** `SecurePage` / `AuthGuard.EnsurePage` when `RequiredPermissionKey` matches `Permissions.PermissionKey` / menu `<li id>`. |

Do **not** document Bill.Master as the only enforcement point. That was true before `SecurePage`; it is stale.

---

## Filename leftover

`Update_Designation.aspx` assigns **`UserRoles`** (permission `ViewUser`). It is not a designation master ([docs/04](04_Department_Designation.md)).

`UserRoles` has **no** `CompanyID` column. Tenant isolation is query-level, not role-row-level.

Pages: `corporate/business/app/Update_Designation.aspx` (not `admin/`).
