-- =============================================================================
-- SwitchUser Permission
-- Run on each environment (UAT, then production).
-- Idempotent: safe to re-run.
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM dbo.Permissions WHERE PermissionKey = 'SwitchUser')
BEGIN
    INSERT INTO dbo.Permissions (PermissionKey, Description, ModuleName, SubModuleName, FeatureName)
    VALUES ('SwitchUser', 'Switch to another user account for support/diagnosis', 'Administration', 'User Management', 'Switch User');
END
GO

-- Grant SwitchUser to Super Admin role (CompanyID=1 assumed; adjust per environment)
-- Only grant if the role has the permission already via RolePermissions
-- This is informational — actual grants are done via ManagePermissions UI.
PRINT 'SwitchUser permission key ensured. Grant via ManagePermissions for desired roles.';
GO
