# Employee administration (ERP users)

Canonical pages: [`page-catalog/DOMAIN_users-rbac.md`](page-catalog/DOMAIN_users-rbac.md).  
AuthN/RBAC contract: [`docs/22_Security_Baseline.md`](22_Security_Baseline.md).  
Leave allocation SP: [`page-catalog/SHARED_RUNTIME.md`](page-catalog/SHARED_RUNTIME.md).

This file keeps **leftover facts** that are not in those catalogs.

---

## Paths and tables

| Item | Fact |
|------|------|
| Pages | `corporate/business/app/AddUser.aspx`, `ViewUser.aspx` — **not** `admin/AddUser.aspx`. |
| Account table | **`tbl_login`** (not `tbl_user_info`, not `tbl_Customers`). |
| Lookups | `tbl_Departments` / `tbl_Designations` dropdowns only — no dept CRUD UI ([docs/04](04_Department_Designation.md)). |
| Roles on create | `Roles` lookup; `tbl_login.RoleId` is display/session cosmetic. Server gates use `UserRoles` ([docs/03](03_Role_Permissions.md)). |

There are no `Add_user.ascx` hosts. Profile popups live under `corporate/business/app/Update/` (ERP) vs `admin/Update/` (kiosk) — different tables.

---

## Unique leftovers

- `sp_AllocateEmployeeLeaves` runs on AddUser insert. It is **not** the only stored procedure in the solution.
- `ViewUser` WebMethod `SaveGeoFence`.
- `tbl_login.Password` is the legacy plaintext column; lockout completion that nulls it and writes hash/salt is **`settings.aspx`**, not the Update popups.
