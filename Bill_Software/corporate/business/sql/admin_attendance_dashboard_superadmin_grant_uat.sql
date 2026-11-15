/* **************************************************************************
   NAME:        admin_attendance_dashboard_superadmin_grant_uat
   WHEN:        2026-11-15
   WHY:         FEE-7 / FEE-27: AdminAttendanceDashboard (PermissionId 138) is
                catalogued but has zero RolePermissions on flamex_uat. Super
                Admin (RoleId 1) cannot open AdminAttendanceDashboard.aspx.
                ManageRoles UI is unreachable for User_Id=admin (missing
                ManageRoles grant). Grant the single Attendance Register key
                via the audited Super Admin UAT INSERT pattern.
   WHAT:        Idempotent INSERT into dbo.RolePermissions for RoleId = 1
                (RoleName N'Super Admin', CompanyID = 1) and PermissionId = 138
                (PermissionKey N'AdminAttendanceDashboard') when
                DB_NAME() = N'flamex_uat'. Preflight asserts tbl_login.Id = 1 /
                User_Id = N'admin' UserRoles mapping. No other keys. No
                UserRoles changes. DBA-executed only; not auto-run by app.
   ************************************************************************** */
-- INERT / DO NOT EXECUTE FROM THE APPLICATION.
-- DBA execute only after employee_config_permissions.sql catalog row exists.
-- Separate approval required before running on flamex_uat. Not for production.
-- Explicitly approved key in this artifact: AdminAttendanceDashboard only.
-- Do NOT extend this script with ManageRoles, ManagePermissions,
-- AdminShiftAssignment, SwitchUser, or other keys without a new approval.

SET NOCOUNT ON;
SET XACT_ABORT ON;

-----------------------------------------------------------------------------
-- 0. Database guard
-----------------------------------------------------------------------------
IF DB_NAME() <> N'flamex_uat'
BEGIN
    RAISERROR('admin_attendance_dashboard_superadmin_grant_uat.sql requires DB_NAME() = N''flamex_uat''. No grant applied.', 16, 1);
    RETURN;
END;

IF OBJECT_ID(N'dbo.Permissions', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Roles', N'U') IS NULL
   OR OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL
   OR OBJECT_ID(N'dbo.UserRoles', N'U') IS NULL
   OR OBJECT_ID(N'dbo.tbl_login', N'U') IS NULL
BEGIN
    RAISERROR('Required RBAC tables missing. No AdminAttendanceDashboard grant applied.', 16, 1);
    RETURN;
END;

-----------------------------------------------------------------------------
-- 1. Preflight: catalog PermissionId 138 / key AdminAttendanceDashboard
-----------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM dbo.Permissions
    WHERE PermissionId = 138
      AND PermissionKey = N'AdminAttendanceDashboard'
)
BEGIN
    RAISERROR('Permissions row PermissionId=138 / AdminAttendanceDashboard missing or mismatched. Run employee_config_permissions.sql first. No grant applied.', 16, 1);
    RETURN;
END;

-----------------------------------------------------------------------------
-- 2. Preflight: Super Admin RoleId 1 / CompanyID 1 (Flame-Ex)
-----------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM dbo.Roles
    WHERE RoleId = 1
      AND RoleName = N'Super Admin'
      AND CompanyID = 1
)
BEGIN
    RAISERROR('Roles RoleId=1 Super Admin CompanyID=1 missing or mismatched. No grant applied.', 16, 1);
    RETURN;
END;

-----------------------------------------------------------------------------
-- 3. Preflight: admin user + UserRoles (Id=1 / User_Id=admin → RoleId 1)
-----------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM dbo.tbl_login u
    WHERE u.Id = 1
      AND u.User_Id = N'admin'
      AND u.CompanyID = 1
)
BEGIN
    RAISERROR('tbl_login Id=1 User_Id=admin CompanyID=1 missing or mismatched. No grant applied.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.UserRoles ur
    WHERE ur.UserId = 1
      AND ur.RoleId = 1
)
BEGIN
    RAISERROR('UserRoles mapping UserId=1 → RoleId=1 missing. No grant applied (UserRoles not modified by this script).', 16, 1);
    RETURN;
END;

-----------------------------------------------------------------------------
-- 4. Pre-change snapshot (read-only; for DBA transcript)
-----------------------------------------------------------------------------
SELECT
    N'PRE' AS SnapshotPhase,
    (SELECT COUNT_BIG(*) FROM dbo.RolePermissions WHERE RoleId = 1) AS SuperAdminRolePermissionCount,
    (SELECT COUNT_BIG(*) FROM dbo.RolePermissions WHERE RoleId = 1 AND PermissionId = 138) AS ExistingGrant138,
    (SELECT COUNT_BIG(*) FROM dbo.RolePermissions WHERE PermissionId = 138) AS GlobalGrant138;

-----------------------------------------------------------------------------
-- 5. Grant (idempotent; preserves all other RolePermissions)
-----------------------------------------------------------------------------
BEGIN TRANSACTION;

DECLARE @Inserted INT = 0;

INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
SELECT r.RoleId, p.PermissionId
FROM dbo.Roles r
INNER JOIN dbo.Permissions p
    ON p.PermissionId = 138
   AND p.PermissionKey = N'AdminAttendanceDashboard'
WHERE r.RoleId = 1
  AND r.RoleName = N'Super Admin'
  AND r.CompanyID = 1
  AND NOT EXISTS (
        SELECT 1
        FROM dbo.RolePermissions rp
        WHERE rp.RoleId = r.RoleId
          AND rp.PermissionId = p.PermissionId
  );

SET @Inserted = @@ROWCOUNT;

-----------------------------------------------------------------------------
-- 6. Post-change assertions (rollback on failure)
-----------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM dbo.RolePermissions
    WHERE RoleId = 1
      AND PermissionId = 138
)
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR('Post-grant assert failed: RolePermissions (1,138) still missing. Rolled back.', 16, 1);
    RETURN;
END;

-- AuthGuard-shaped probe: admin must resolve AdminAttendanceDashboard via Super Admin
IF NOT EXISTS (
    SELECT TOP 1 1
    FROM dbo.Permissions p
    INNER JOIN dbo.RolePermissions rp ON p.PermissionId = rp.PermissionId
    INNER JOIN dbo.UserRoles ur ON rp.RoleId = ur.RoleId
    INNER JOIN dbo.tbl_login u ON ur.UserId = u.Id AND u.CompanyID = 1
    WHERE u.User_Id = N'admin'
      AND u.Id = 1
      AND p.PermissionKey = N'AdminAttendanceDashboard'
      AND p.PermissionId = 138
)
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR('Post-grant AuthGuard-shaped assert failed for admin / AdminAttendanceDashboard. Rolled back.', 16, 1);
    RETURN;
END;

-- Narrow insert: at most one new row for this key on Super Admin
IF @Inserted NOT IN (0, 1)
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR('Unexpected insert rowcount for AdminAttendanceDashboard grant. Rolled back.', 16, 1);
    RETURN;
END;

COMMIT TRANSACTION;

SELECT
    N'POST' AS SnapshotPhase,
    @Inserted AS RowsInsertedThisRun,
    (SELECT COUNT_BIG(*) FROM dbo.RolePermissions WHERE RoleId = 1) AS SuperAdminRolePermissionCount,
    (SELECT COUNT_BIG(*) FROM dbo.RolePermissions WHERE RoleId = 1 AND PermissionId = 138) AS ExistingGrant138;

PRINT N'admin_attendance_dashboard_superadmin_grant_uat: OK (inserted='
    + CONVERT(nvarchar(11), @Inserted)
    + N'). Existing RolePermissions for other roles/keys preserved.';
GO

-----------------------------------------------------------------------------
-- 7. Narrow rollback (manual; do not run with the grant unless reversing)
--    Deletes ONLY Super Admin × PermissionId 138. Preserves all other rows.
-----------------------------------------------------------------------------
/*
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'flamex_uat'
BEGIN
    RAISERROR('Rollback requires DB_NAME() = N''flamex_uat''.', 16, 1);
    RETURN;
END;

BEGIN TRANSACTION;

DELETE FROM dbo.RolePermissions
WHERE RoleId = 1
  AND PermissionId = 138;

IF @@ROWCOUNT > 1
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR('Rollback deleted unexpected rowcount. Rolled back delete.', 16, 1);
    RETURN;
END;

COMMIT TRANSACTION;
*/
GO
