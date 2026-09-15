-- =============================================================================
-- NAME:        SwitchUser_superadmin_grant_uat
-- When:        PR-83 UAT-only impersonation activation
-- Why:         Super Admin must hold SwitchUser in RolePermissions for the
--              menu + SwitchUser.aspx gate. Feature flag remains an AppSetting
--              on the UAT host (SwitchUser=true). Production Release transform
--              stays false. Do not run this on production.
-- What:        Idempotent INSERT of RolePermissions for RoleName = N'Super Admin'
--              and PermissionKey = N'SwitchUser'. No other roles. DBA execute
--              only. Do not run from the application.
-- =============================================================================
-- INERT / DO NOT EXECUTE FROM THE APPLICATION.

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.Permissions', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Roles', N'U') IS NULL
   OR OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL
BEGIN
    RAISERROR('Permissions, Roles, or RolePermissions is missing. No SwitchUser grant applied.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE PermissionKey = N'SwitchUser')
BEGIN
    RAISERROR('SwitchUser permission catalog row is missing. Run SwitchUser_permission.sql first.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = N'Super Admin')
BEGIN
    RAISERROR('Super Admin role is missing. No SwitchUser grant applied.', 16, 1);
    RETURN;
END;

BEGIN TRANSACTION;

INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Roles r
INNER JOIN dbo.Permissions p ON p.PermissionKey = N'SwitchUser'
WHERE r.RoleName = N'Super Admin'
  AND NOT EXISTS (
        SELECT 1
        FROM dbo.RolePermissions rp
        WHERE rp.RoleId = r.RoleId
          AND rp.PermissionId = p.PermissionId
  );

COMMIT TRANSACTION;
GO
