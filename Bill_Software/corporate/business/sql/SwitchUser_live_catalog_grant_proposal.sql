/* ============================================================================
   NAME:        switchuser_live_catalog_grant
   WHEN:        2026-11-09
   WHY:         LIVE dbo.Permissions has no SwitchUser key. Super Admin RoleId 1, CompanyID 1 is the confirmed production role.
   WHAT:        Idempotent insert of one dbo.Permissions row and one dbo.RolePermissions row for RoleId 1. Rollback unless those counts are exact.
   ============================================================================ */

-- DBA execute only. Do not run from the application.

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'flamex_live'
BEGIN
    RAISERROR('Database guard failed: DB_NAME() must be flamex_live. No SwitchUser grant applied.', 16, 1);
    RETURN;
END;

IF OBJECT_ID(N'dbo.Permissions', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Roles', N'U') IS NULL
   OR OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL
BEGIN
    RAISERROR('Permissions, Roles, or RolePermissions is missing. No SwitchUser grant applied.', 16, 1);
    RETURN;
END;

DECLARE @RoleId int;
DECLARE @CatalogCount int;
DECLARE @SuperAdminMapCount int;
DECLARE @OtherRoleMapCount int;

SELECT @RoleId = r.RoleId
FROM dbo.Roles r
WHERE r.RoleId = 1
  AND r.RoleName = N'Super Admin'
  AND r.CompanyID = 1;

IF @@ROWCOUNT <> 1 OR @RoleId IS NULL
BEGIN
    RAISERROR('Precondition failed: expected exactly RoleId 1, RoleName Super Admin, CompanyID 1. No grant applied.', 16, 1);
    RETURN;
END;

IF EXISTS (
    SELECT 1
    FROM dbo.Permissions
    WHERE PermissionKey = N'SwitchUser'
      AND (
            ISNULL(ModuleName, N'') <> N'Administration'
         OR ISNULL(SubModuleName, N'') <> N'User Management'
         OR ISNULL(FeatureName, N'') <> N'Switch User'
      )
)
BEGIN
    RAISERROR('Precondition failed: existing SwitchUser catalog metadata does not match Administration / User Management / Switch User. No grant applied.', 16, 1);
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

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
            N'Administrator impersonation. Gates the Switch User menu and page.',
            N'Administration',
            N'User Management',
            N'Switch User'
        );
END;

INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
SELECT @RoleId, p.PermissionId
FROM dbo.Permissions p
WHERE p.PermissionKey = N'SwitchUser'
  AND NOT EXISTS (
        SELECT 1
        FROM dbo.RolePermissions rp
        WHERE rp.RoleId = @RoleId
          AND rp.PermissionId = p.PermissionId
  );

SELECT
    @CatalogCount = (
        SELECT COUNT(*)
        FROM dbo.Permissions
        WHERE PermissionKey = N'SwitchUser'
    ),
    @SuperAdminMapCount = (
        SELECT COUNT(*)
        FROM dbo.RolePermissions rp
        INNER JOIN dbo.Permissions p ON p.PermissionId = rp.PermissionId
        INNER JOIN dbo.Roles r ON r.RoleId = rp.RoleId
        WHERE p.PermissionKey = N'SwitchUser'
          AND r.RoleId = @RoleId
          AND r.RoleName = N'Super Admin'
          AND r.CompanyID = 1
    ),
    @OtherRoleMapCount = (
        SELECT COUNT(*)
        FROM dbo.RolePermissions rp
        INNER JOIN dbo.Permissions p ON p.PermissionId = rp.PermissionId
        INNER JOIN dbo.Roles r ON r.RoleId = rp.RoleId
        WHERE p.PermissionKey = N'SwitchUser'
          AND (r.RoleId <> @RoleId OR r.RoleName <> N'Super Admin' OR r.CompanyID <> 1)
    );

IF @CatalogCount <> 1 OR @SuperAdminMapCount <> 1 OR @OtherRoleMapCount <> 0
    BEGIN
        THROW 50001, 'Postcondition failed: require one SwitchUser catalog row, one Super Admin mapping, and zero mappings on every other role. Transaction rolled back.', 1;
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
