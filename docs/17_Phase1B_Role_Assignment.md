# Phase 1B — Role Assignment Consistency

**Date:** 2026-09-07  
**Depends on:** PR #66 / #67 / #68. `AuthGuard.HasPermission`, `Bill.Master` menu filtering, `Permissions`, and `RolePermissions` are not changed.  
**STOP honored:** `UserRoles` remains many-to-many (`Update_Designation` checkboxes). `tbl_login.RoleId` remains the display/primary role (login session + header). Neither column is removed.

---

## Task 1 — Role lifecycle (as-is, then sync)

| Source | Writes | Reads | Effect before Phase 1B |
|--------|--------|-------|------------------------|
| `AddUser.aspx.cs` | `tbl_login.RoleId` | Roles dropdown | New user had a header role and **no** `UserRoles` row |
| `ViewUser.aspx.cs` | `tbl_login.RoleId` | Roles dropdown | Display role could change without touching grants |
| `Update_Designation.aspx.cs` | `UserRoles` (delete-all + insert selected) | `UserRoles` | Menu/SecurePage grants; did **not** update `RoleId` |
| `index.aspx.cs` login | — | `tbl_login.RoleId` | `Session["RoleId"]` / `RoleName` (cosmetic) |
| `Bill.Master.GetAdminName` | — | `tbl_login.RoleId` | Header label |
| `Bill.Master.GetMenuControl` | — | `UserRoles` → `RolePermissions` → `Permissions` | Menu `<li>` visibility |
| `AuthGuard.HasPermission` | — | same join as menu | Phase 0A/1A page 403 |

Phase 1B only adds writes so the display role is also present in `UserRoles`. Permission evaluation is still the existing join.

---

## Synchronization rules

| Event | `tbl_login.RoleId` | `UserRoles` |
|-------|--------------------|-------------|
| Add User with a selected role | written as today | insert `(Id, RoleId)` if missing; role must belong to current company |
| Add User with no role | NULL | no row (fail closed; empty menu) |
| View User changes display role A → B | B | insert B if missing; **delete only A**; extra Update_Designation roles stay |
| View User clears display role | NULL | **not wiped** (do not strip many-to-many grants) |
| Update Designation save | aligned: keep RoleId if still granted, else `MIN(RoleId)` of remaining grants, else NULL | delete-all then insert selected; skip duplicates; reject roles not in current company |

Do not execute `user_roles_reconciliation.sql` from the application.

---

## Assumptions

- `UserRoles.UserId` is `tbl_login.Id` (numeric), already used by `Update_Designation` and `HasPermission`.
- `Roles.CompanyID` is the tenant of a role. Mappings are refused when `Roles.CompanyID` ≠ current company.
- Extra `UserRoles` rows are intentional. Reconciliation **inserts missing display mappings only** and never deletes grants.
