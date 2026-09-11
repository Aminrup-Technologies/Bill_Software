# 30 — Impersonation permission catalog

**ADR:** [ADR-001](ADR-001_Administrator_Impersonation.md)  
**Script:** `Bill_Software/corporate/business/sql/SwitchUser_permission.sql` (DBA only; not executed by the app)

## Catalog row

| Column | Value |
|--------|--------|
| PermissionKey | `SwitchUser` |
| ModuleName | Administration |
| SubModuleName | User Management |
| FeatureName | Switch User |

Idempotent `INSERT` when the key is missing. **No** `RolePermissions` insert. Super Admin does not receive the key from this script.

`ImpersonationGovernance.PermissionKey` is the C# constant. Catalog script does not grant. UAT Super Admin grant is `SwitchUser_superadmin_grant_uat.sql` (DBA only).
