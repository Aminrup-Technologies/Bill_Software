# Module 03 — Role & Permission Management

> Master Page Menu Position: **Administration → Update Designation** (admin-role only)

---

## 1. Overview

The Role & Permission Management module maintains the application's authorization mechanism: the many-to-many mapping between users and roles (`UserRoles`) and between roles and permissions (`RolePermissions`). This module controls **which navigation menu items** are visible to each user when they log in. It does **not** gate server-side data access — see Architectural Notes below.

---

## 2. Files

| File | Type | Purpose |
|------|------|---------|
| `admin/Update_Designation.aspx` | Frontend | UI for assigning roles to users |
| `admin/Update_Designation.aspx.cs` | Backend | `UserRoles` table maintenance — insert/update/delete user↔role mappings |

### Supporting Files

| File | Relationship |
|------|-------------|
| `Bill.Master.cs :: GetMenuControl()` | **Consumer** — reads `UserRoles` + `RolePermissions` + `Permissions` to render the navigation menu. This is the **only place** in the codebase where the permission system is actually enforced. |
| `admin/ViewUser.aspx[.cs]` | Manages `tbl_login.RoleId` (a separate, cosmetic role assignment that does not interact with `UserRoles`) |
| `corporate/business/app/Bill.Master` | Master Page that hosts the menu control |

---

## 3. Core Database Tables

| Table | Usage in This Module |
|-------|---------------------|
| `UserRoles` | Many-to-many mapping: `UserId` (→ `tbl_login.Id`, numeric) ↔ `RoleId` |
| `RolePermissions` | Many-to-many mapping: `RoleId` ↔ `PermissionId` |
| `Permissions` | Permission definitions (menu items, features) |
| `Roles` | Role definitions (`RoleName`) |
| `tbl_login` | User directory — joined to `UserRoles` via numeric `Id` |

### Key Columns

- `UserRoles.UserId` → `tbl_login.Id` (numeric surrogate PK, **not** `User_Id` business key)
- `RolePermissions.RoleId` → `Roles.RoleId`
- `RolePermissions.PermissionId` → `Permissions.PermissionId`

---

## 4. Multi-Tenant Constraints

| Constraint | Status | Evidence |
|-----------|--------|----------|
| `CompanyID` scoping on role assignment | ⚠️ Not directly enforced | `Update_Designation.aspx.cs` does not appear to scope role assignments by `CompanyID`; an admin user can theoretically assign roles to users in any company |

### Tenant Isolation Pattern

Role assignment is managed at the application level, not the database level. The `UserRoles` table does not include a `CompanyId` column — roles are global, not tenant-scoped. This is acceptable because role definitions represent application-wide capabilities (which menus to show), not data-access boundaries (which data to see). Data isolation is enforced at the query level via `CompanyId` filters, not at the role level.

---

## 5. Proactive Notification Triggers

| Trigger | Table | Description |
|---------|-------|-------------|
| Role assignment change | `tbl_SystemNotification` | Audit logging expected on role mapping modifications |

---

## 6. Architectural Notes

### Two Independent Role Mechanisms (Unreconciled)

The application has **two structurally independent** role/permission systems that are never reconciled:

| Mechanism | Table | Storage | Usage |
|-----------|-------|---------|-------|
| `tbl_login.RoleId` | Single-valued FK | Stored in `Session["RoleId"]` at login | **Cosmetic only** — displayed in header, admin grids. **Never consulted for authorization.** |
| `UserRoles` + `RolePermissions` + `Permissions` | Many-to-many | Queried on every Master Page load | **Controls navigation menu visibility** via `Bill.Master.cs :: GetMenuControl()`. **Never consulted for data-access authorization.** |

These two mechanisms can silently diverge for the same user (e.g., `tbl_login.RoleId` says one thing, `UserRoles` says another). This inconsistency is documented as an architectural concern but has no direct security impact, because:

1. `tbl_login.RoleId` is not used for any access control.
2. `UserRoles` only controls menu visibility — it does not prevent a user from accessing a page by directly navigating to its URL.

### Navigation Menu Rendering

`Bill.Master.cs :: GetMenuControl()` executes a query joining `UserRoles` → `tbl_login` → `RolePermissions` → `Permissions` to determine which `<li>` elements are visible in the sidebar navigation. This is the **sole enforcement point** for the permission system — no server-side code-behind method checks permissions before executing an action.

### Implication

A user who knows (or guesses) the URL of a restricted page can access it directly, regardless of their `UserRoles` entries. All authorization at the data level relies on the `Session["USERID"]` gate and the (inconsistently applied) `CompanyId`/`CreatedByCode` filters documented in the Sales Visit Workflow Audit.
