/* **************************************************************************
   NAME:        admin_leave_balance_superadmin_grant_uat
   WHEN:        2026-11-14
   WHY:         Grant Super Admin RolePermissions for AdminLeaveBalance after
                admin_leave_balance_permission.sql on flamex_uat.
   WHAT:        Idempotent INSERT into dbo.RolePermissions for RoleName =
                N'Super Admin' and PermissionKey AdminLeaveBalance.
                DBA-executed only; not auto-run.
   ************************************************************************** */
-- INERT / DO NOT EXECUTE FROM THE APPLICATION.

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'flamex_uat'
BEGIN
    RAISERROR('admin_leave_balance_superadmin_grant_uat.sql requires DB_NAME() = N''flamex_uat''. No grant applied.', 16, 1);
    RETURN;
END;

IF OBJECT_ID(N'dbo.Permissions', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Roles', N'U') IS NULL
   OR OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL
BEGIN
    RAISERROR('Permissions, Roles, or RolePermissions is missing. No AdminLeaveBalance grant applied.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE PermissionKey = N'AdminLeaveBalance')
BEGIN
    RAISERROR('AdminLeaveBalance catalog row missing. Run admin_leave_balance_permission.sql first.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = N'Super Admin')
BEGIN
    RAISERROR('Super Admin role is missing. No AdminLeaveBalance grant applied.', 16, 1);
    RETURN;
END;

BEGIN TRANSACTION;

INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Roles r
INNER JOIN dbo.Permissions p ON p.PermissionKey = N'AdminLeaveBalance'
WHERE r.RoleName = N'Super Admin'
  AND NOT EXISTS (
        SELECT 1
        FROM dbo.RolePermissions rp
        WHERE rp.RoleId = r.RoleId
          AND rp.PermissionId = p.PermissionId
  );

COMMIT TRANSACTION;
GO
