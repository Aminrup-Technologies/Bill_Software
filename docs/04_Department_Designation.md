# Module 04 — Department & Designation Management

> Master Page Menu Position: **Administration → Departments** / **Administration → Designations** (admin-role only)

---

## 1. Overview

The Department & Designation Management module maintains the organizational structure reference tables (`tbl_Departments`, `tbl_Designations`) that are used throughout the application for employee classification. These lookup tables feed dropdowns in user provisioning, reporting, and potentially in visit/customer records.

---

## 2. Files

| File | Type | Purpose |
|------|------|---------|
| `admin/Update/` subdirectory | Backend | Department and designation CRUD operations (exact file names inferred from directory structure) |

### Supporting Files

| File | Relationship |
|------|-------------|
| `admin/AddUser.aspx[.cs]` | Consumes `tbl_Departments` and `tbl_Designations` for dropdown population during user creation |
| `admin/ViewUser.aspx[.cs]` | Displays department/designation in user management grid |

---

## 3. Core Database Tables

| Table | Usage in This Module |
|-------|---------------------|
| `tbl_Departments` | Department directory — CRUD managed by this module |
| `tbl_Designations` | Designation directory — CRUD managed by this module |
| `tbl_login` | References `DepartmentID` and `DesignationID` as FKs |

### Key Columns

- `tbl_Departments` — department ID, name, and associated metadata
- `tbl_Designations` — designation ID, name, and associated metadata
- `tbl_login.DepartmentID` → `tbl_Departments` (FK)
- `tbl_login.DesignationID` → `tbl_Designations` (FK)

---

## 4. Multi-Tenant Constraints

| Constraint | Status | Evidence |
|-----------|--------|----------|
| `CompanyID` scoping on department/designation CRUD | ⚠️ Likely not enforced | These appear to be global reference tables, not tenant-scoped; all companies share the same department/designation vocabulary |

### Tenant Isolation Pattern

Departments and designations are treated as **global reference data** — a shared vocabulary across all tenants. This is a common ERP pattern: the organizational taxonomy is consistent across the company group, while individual employee assignments are tenant-scoped via `tbl_login.CompanyID`.

---

## 5. Proactive Notification Triggers

| Trigger | Table | Description |
|---------|-------|-------------|
| Department/Designation creation or modification | `tbl_SystemNotification` | Audit logging expected on reference data changes |

---

## 6. Architectural Notes

- These reference tables are small and relatively static — they are populated once during initial setup and updated infrequently.
- The department/designation IDs are used as display attributes in user management and potentially in visit reports, but they do not participate in any query-level authorization or tenancy logic.
- No defects or security concerns have been identified in this module. It is the simplest CRUD module in the application.
