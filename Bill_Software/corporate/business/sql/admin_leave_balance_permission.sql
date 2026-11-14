/* **************************************************************************
   NAME:        admin_leave_balance_permission
   WHEN:        2026-11-14
   WHY:         FEE-16: AdminLeaveBalance.aspx SecurePage + EmpConfig menu need
                PermissionKey AdminLeaveBalance in the UAT catalog.
   WHAT:        Idempotent INSERT into dbo.Permissions for AdminLeaveBalance when
                DB_NAME() = N'flamex_uat'. No RolePermissions in this script.
                DBA-executed only; not auto-run.
   ************************************************************************** */
-- INERT / DO NOT EXECUTE FROM THE APPLICATION.

SET NOCOUNT ON;

IF DB_NAME() <> N'flamex_uat'
BEGIN
    RAISERROR('admin_leave_balance_permission.sql requires DB_NAME() = N''flamex_uat''. No catalog row applied.', 16, 1);
    RETURN;
END;

IF OBJECT_ID(N'dbo.Permissions', N'U') IS NULL
BEGIN
    RAISERROR('dbo.Permissions does not exist. No AdminLeaveBalance catalog row applied.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (
    SELECT 1 FROM dbo.Permissions WHERE PermissionKey = N'AdminLeaveBalance'
)
BEGIN
    INSERT INTO dbo.Permissions
        (PermissionKey, Description, ModuleName, SubModuleName, FeatureName)
    VALUES
        (
            N'AdminLeaveBalance',
            N'Leave balance allot/adjust (AdminLeaveBalance.aspx). Catalog only; not granted.',
            N'Dashboard',
            N'Employee Config',
            N'Leave Balances'
        );
END;
GO
