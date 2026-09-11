-- =============================================================================
-- NAME:        SwitchUser_permission
-- When:        PR-2A impersonation governance foundation
-- Why:         PermissionKey = 'SwitchUser' must exist in dbo.Permissions so
--              later PRs can gate the page. Catalog metadata only.
-- What:        Idempotent INSERT of the SwitchUser permission row.
--              No RolePermissions grant. No UserRoles change. DBA execute only.
--              Do not run from the application. Impersonation stays disabled
--              until the SwitchUser feature flag is true AND a later PR grants
--              the key. See docs/ADR-001_Administrator_Impersonation.md,
--              docs/30_Impersonation_Permission_Catalog.md.
-- =============================================================================

SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.Permissions', N'U') IS NULL
BEGIN
    RAISERROR('dbo.Permissions does not exist. No SwitchUser catalog row applied.', 16, 1);
    RETURN;
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Permissions
    WHERE PermissionKey = N'SwitchUser'
)
BEGIN
    INSERT INTO dbo.Permissions
        (PermissionKey, Description, ModuleName, SubModuleName, FeatureName)
    VALUES
        (
            N'SwitchUser',
            N'Administrator impersonation. Catalog only in PR-2A; not granted; feature flag defaults false.',
            N'Administration',
            N'User Management',
            N'Switch User'
        );
END;

-- No INSERT into dbo.RolePermissions.
GO
