/* **************************************************************************
   NAME:        empconfig_override_shift_live_catalog_seed
   WHEN:        2026-11-15
   WHY:         flamex_live dbo.Permissions is missing AdminOverride and
                AdminShiftAssignment (LIVE MISSING_ONLY vs 114-key Super Admin
                nav set). Both are EmpConfig Master leaves with SecurePage
                gates. Catalog metadata is proven in
                employee_config_permissions.sql (UAT draft). AdminAttendance
                Dashboard is intentionally omitted — live_superadmin_nav_grant
                inserts that key. No RolePermissions in this script.
   WHAT:        On DB_NAME()=N'flamex_live' only: idempotent INSERT of exactly
                two dbo.Permissions rows (PermissionKey AdminShiftAssignment,
                AdminOverride) with Description/ModuleName/SubModuleName/
                FeatureName copied from employee_config_permissions.sql.
                PermissionId is IDENTITY and is not supplied. Existing rows
                are not updated. No RolePermissions / UserRoles. SQL Server
                2017/2019 compatible. DBA-executed only; never from the app.
   ************************************************************************** */
-- INERT / DO NOT EXECUTE FROM THE APPLICATION.
-- Separate written approval required before any flamex_live execution.
-- Catalog only. Grants for RoleId 1 remain in live_superadmin_nav_grant.sql.

SET NOCOUNT ON;
SET XACT_ABORT ON;

-----------------------------------------------------------------------------
-- 0. Database guard
-----------------------------------------------------------------------------
IF DB_NAME() <> N'flamex_live'
BEGIN
    RAISERROR('empconfig_override_shift_live_catalog_seed.sql requires DB_NAME() = N''flamex_live''. No changes applied.', 16, 1);
    RETURN;
END;

IF OBJECT_ID(N'dbo.Permissions', N'U') IS NULL
BEGIN
    RAISERROR('dbo.Permissions does not exist. No EmpConfig LIVE catalog rows applied.', 16, 1);
    RETURN;
END;

-----------------------------------------------------------------------------
-- 1. Schema preflight: PermissionId IDENTITY (do not force PermissionId)
-----------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns c
    WHERE c.object_id = OBJECT_ID(N'dbo.Permissions')
      AND c.name = N'PermissionId'
      AND c.is_identity = 1
)
BEGIN
    RAISERROR('Precondition failed: dbo.Permissions.PermissionId is not IDENTITY. Aborting (do not force PermissionId).', 16, 1);
    RETURN;
END;

-----------------------------------------------------------------------------
-- 2. Idempotent catalog insert (two keys only)
-----------------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Permissions
        WHERE PermissionKey = N'AdminShiftAssignment'
    )
    BEGIN
        INSERT INTO dbo.Permissions
            (PermissionKey, Description, ModuleName, SubModuleName, FeatureName)
        VALUES
            (
                N'AdminShiftAssignment',
                N'Shift assignment / employee-shift mapping (AdminShiftAssignment.aspx). Catalog only; not granted. Distinct from AdminShiftSetup.',
                N'Dashboard',
                N'Employee Config',
                N'Shift Assignment'
            );
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Permissions
        WHERE PermissionKey = N'AdminOverride'
    )
    BEGIN
        INSERT INTO dbo.Permissions
            (PermissionKey, Description, ModuleName, SubModuleName, FeatureName)
        VALUES
            (
                N'AdminOverride',
                N'HR override dashboard (AdminOverride.aspx). Catalog only; not granted.',
                N'Dashboard',
                N'Employee Config',
                N'Admin Override'
            );
    END;

    -- Fail closed: both keys must exist after insert attempts.
    IF NOT EXISTS (
            SELECT 1 FROM dbo.Permissions WHERE PermissionKey = N'AdminShiftAssignment'
        )
       OR NOT EXISTS (
            SELECT 1 FROM dbo.Permissions WHERE PermissionKey = N'AdminOverride'
        )
    BEGIN
        THROW 50001, 'Postcondition failed: AdminShiftAssignment and/or AdminOverride missing from dbo.Permissions after catalog seed.', 1;
    END;

    -- Reject wrong metadata on pre-existing rows (do not UPDATE; abort for DBA review).
    IF EXISTS (
        SELECT 1
        FROM dbo.Permissions
        WHERE PermissionKey = N'AdminShiftAssignment'
          AND (
                ISNULL(ModuleName, N'') <> N'Dashboard'
             OR ISNULL(SubModuleName, N'') <> N'Employee Config'
             OR ISNULL(FeatureName, N'') <> N'Shift Assignment'
          )
    )
    BEGIN
        THROW 50002, 'Precondition failed: existing AdminShiftAssignment metadata is not exactly Dashboard / Employee Config / Shift Assignment. Aborting.', 1;
    END;

    IF EXISTS (
        SELECT 1
        FROM dbo.Permissions
        WHERE PermissionKey = N'AdminOverride'
          AND (
                ISNULL(ModuleName, N'') <> N'Dashboard'
             OR ISNULL(SubModuleName, N'') <> N'Employee Config'
             OR ISNULL(FeatureName, N'') <> N'Admin Override'
          )
    )
    BEGIN
        THROW 50003, 'Precondition failed: existing AdminOverride metadata is not exactly Dashboard / Employee Config / Admin Override. Aborting.', 1;
    END;

    SELECT
        N'EMPCONFIG_LIVE_CATALOG_SEED' AS ResultSet,
        (SELECT COUNT(*) FROM dbo.Permissions WHERE PermissionKey = N'AdminShiftAssignment') AS AdminShiftAssignmentCount,
        (SELECT COUNT(*) FROM dbo.Permissions WHERE PermissionKey = N'AdminOverride') AS AdminOverrideCount,
        (SELECT COUNT(*) FROM dbo.Permissions WHERE PermissionKey = N'AdminAttendanceDashboard') AS AdminAttendanceDashboardCount_Informational;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;

-- No INSERT into dbo.RolePermissions.
-- No INSERT into dbo.UserRoles.
-- AdminAttendanceDashboard intentionally not seeded here (grant script owns that insert).
-- PermissionId is IDENTITY and is not supplied.
