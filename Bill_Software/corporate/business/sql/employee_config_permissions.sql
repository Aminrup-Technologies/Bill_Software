/* ============================================================================
   NAME:        employee_config_permissions
   WHEN:        2026-11-14
   WHY:         SecurePage gates AdminLeaveSetup.aspx, AdminShiftSetup.aspx,
                AdminShiftAssignment.aspx (shared key AdminShiftSetup),
                AdminOverride.aspx, and AdminAttendanceDashboard.aspx on
                Bill.Master menu ids. Those PermissionKey values must exist in
                the UAT permission catalog (docs/16_Phase1A_SecurePage_RBAC.md)
                for menu visibility and EnsurePage. EmpConfig is a parent
                navigation container (href="#"), not an independently protected
                page, so it is not a catalog key.
   WHAT:        Idempotent INSERT into dbo.Permissions of exactly four rows:
                PermissionKey AdminLeaveSetup, AdminShiftSetup, AdminOverride,
                AdminAttendanceDashboard. Column list matches
                SwitchUser_permission.sql. PermissionId is int IDENTITY
                (docs/page-catalog/UAT_CATALOG.md) and is not supplied.
                Description nvarchar(500), ModuleName nvarchar(100),
                SubModuleName nvarchar(100), and FeatureName nvarchar(100) are
                supplied within those lengths. Runs only when
                DB_NAME() = N'flamex_uat'. Existing rows are not updated.
                No INSERT into dbo.RolePermissions or dbo.UserRoles. No grant
                to Super Admin or any other role. DBA-executed only. Requires
                separate approval before it is run. Do not run from the
                application.
                Concurrency: IF NOT EXISTS then INSERT is not atomic. Unique
                constraint UQ__tmp_ms_x__8884ABD46831F704 on PermissionKey
                rejects the losing insert, so a duplicate key is not stored.
                Re-run after that error inserts nothing for keys already present.
   ============================================================================ */
-- INERT / DO NOT EXECUTE FROM THE APPLICATION.
-- DBA execute only. Separate approval is required before running on flamex_uat.
-- UAT catalog: dbo.Permissions.PermissionKey is unique
-- (UQ__tmp_ms_x__8884ABD46831F704). Overlapping runs can both pass IF NOT EXISTS;
-- the unique constraint fails the second insert. It does not insert a second row.

SET NOCOUNT ON;

IF DB_NAME() <> N'flamex_uat'
BEGIN
    RAISERROR('employee_config_permissions.sql requires DB_NAME() = N''flamex_uat''. No catalog rows applied.', 16, 1);
    RETURN;
END;

IF OBJECT_ID(N'dbo.Permissions', N'U') IS NULL
BEGIN
    RAISERROR('dbo.Permissions does not exist. No Employee Config catalog rows applied.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Permissions
    WHERE PermissionKey = N'AdminLeaveSetup'
)
BEGIN
    INSERT INTO dbo.Permissions
        (PermissionKey, Description, ModuleName, SubModuleName, FeatureName)
    VALUES
        (
            N'AdminLeaveSetup',
            N'Leave policy setup (AdminLeaveSetup.aspx). Catalog only; not granted.',
            N'Dashboard',
            N'Employee Config',
            N'Leave Setup'
        );
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Permissions
    WHERE PermissionKey = N'AdminShiftSetup'
)
BEGIN
    INSERT INTO dbo.Permissions
        (PermissionKey, Description, ModuleName, SubModuleName, FeatureName)
    VALUES
        (
            N'AdminShiftSetup',
            N'Shift setup (AdminShiftSetup.aspx). Same key gates AdminShiftAssignment.aspx. Catalog only; not granted.',
            N'Dashboard',
            N'Employee Config',
            N'Shift Setup'
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

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Permissions
    WHERE PermissionKey = N'AdminAttendanceDashboard'
)
BEGIN
    INSERT INTO dbo.Permissions
        (PermissionKey, Description, ModuleName, SubModuleName, FeatureName)
    VALUES
        (
            N'AdminAttendanceDashboard',
            N'Enterprise attendance register (AdminAttendanceDashboard.aspx). Catalog only; not granted.',
            N'Dashboard',
            N'Employee Config',
            N'Attendance Register'
        );
END;

-- No INSERT into dbo.RolePermissions.
-- No INSERT into dbo.UserRoles.
-- EmpConfig is not registered. AdminShiftAssignment is not a separate key.
-- PermissionId is IDENTITY and is not supplied.
GO
