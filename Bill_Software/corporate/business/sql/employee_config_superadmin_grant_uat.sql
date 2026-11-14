/* ============================================================================
   NAME:        employee_config_superadmin_grant_uat
   WHEN:        2026-11-14
   WHY:         FEE-16 / attendance-leave gaps P2: EmpConfig SecurePage keys
                (AdminLeaveSetup, AdminShiftSetup, AdminOverride) exist in the
                Permissions catalog via employee_config_permissions.sql but have
                no RolePermissions rows. Super Admin cannot see EmpConfig menus
                on fresh UAT role setups until granted. Pattern matches
                SwitchUser_superadmin_grant_uat.sql (idempotent Super Admin only).
   WHAT:        Idempotent INSERT into dbo.RolePermissions for RoleName =
                N'Super Admin' and PermissionKey in (AdminLeaveSetup,
                AdminShiftSetup, AdminOverride). Requires catalog rows first.
                Runs only when DB_NAME() = N'flamex_uat'. No other roles.
                No UserRoles changes. DBA-executed only; not auto-run by app.
   ============================================================================ */
-- INERT / DO NOT EXECUTE FROM THE APPLICATION.
-- DBA execute only after employee_config_permissions.sql on flamex_uat.
-- Separate approval required before running on flamex_uat. Not for production.

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'flamex_uat'
BEGIN
    RAISERROR('employee_config_superadmin_grant_uat.sql requires DB_NAME() = N''flamex_uat''. No grants applied.', 16, 1);
    RETURN;
END;

IF OBJECT_ID(N'dbo.Permissions', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Roles', N'U') IS NULL
   OR OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL
BEGIN
    RAISERROR('Permissions, Roles, or RolePermissions is missing. No EmpConfig grants applied.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = N'Super Admin')
BEGIN
    RAISERROR('Super Admin role is missing. No EmpConfig grants applied.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE PermissionKey = N'AdminLeaveSetup')
   OR NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE PermissionKey = N'AdminShiftSetup')
   OR NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE PermissionKey = N'AdminOverride')
BEGIN
    RAISERROR('EmpConfig catalog rows missing. Run employee_config_permissions.sql first. No grants applied.', 16, 1);
    RETURN;
END;

BEGIN TRANSACTION;

INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Roles r
CROSS JOIN dbo.Permissions p
WHERE r.RoleName = N'Super Admin'
  AND p.PermissionKey IN (N'AdminLeaveSetup', N'AdminShiftSetup', N'AdminOverride')
  AND NOT EXISTS (
        SELECT 1
        FROM dbo.RolePermissions rp
        WHERE rp.RoleId = r.RoleId
          AND rp.PermissionId = p.PermissionId
  );

COMMIT TRANSACTION;
GO
