/* **************************************************************************
   NAME:        admin_holiday_setup_permission
   WHEN:        2026-11-14
   WHY:         FEE-16: AdminHolidaySetup.aspx is SecurePage with PermissionKey
                AdminHolidaySetup and a Bill.Master EmpConfig menu id. Catalog
                row is required before RolePermissions grants / menu visibility.
   WHAT:        Idempotent INSERT into dbo.Permissions for PermissionKey
                AdminHolidaySetup when DB_NAME() = N'flamex_uat'. No
                RolePermissions grant in this script (use ManagePermissions or
                extend employee_config_superadmin_grant_uat after catalog apply).
                DBA-executed only; not auto-run by the application.
   ************************************************************************** */
-- INERT / DO NOT EXECUTE FROM THE APPLICATION.
-- DBA execute only. Separate approval required before running on flamex_uat.

SET NOCOUNT ON;

IF DB_NAME() <> N'flamex_uat'
BEGIN
    RAISERROR('admin_holiday_setup_permission.sql requires DB_NAME() = N''flamex_uat''. No catalog row applied.', 16, 1);
    RETURN;
END;

IF OBJECT_ID(N'dbo.Permissions', N'U') IS NULL
BEGIN
    RAISERROR('dbo.Permissions does not exist. No AdminHolidaySetup catalog row applied.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Permissions
    WHERE PermissionKey = N'AdminHolidaySetup'
)
BEGIN
    INSERT INTO dbo.Permissions
        (PermissionKey, Description, ModuleName, SubModuleName, FeatureName)
    VALUES
        (
            N'AdminHolidaySetup',
            N'Company holiday calendar (AdminHolidaySetup.aspx). Catalog only; not granted.',
            N'Dashboard',
            N'Employee Config',
            N'Holiday Setup'
        );
END;
GO
