/* ============================================================================
   NAME:        switchuser_live_auth_diagnostic
   WHEN:        2026-11-09
   WHY:         Diagnose LIVE SwitchUser authorization for one login without granting the permission or changing data.
   WHAT:        Read-only SELECT of dbo.Permissions, dbo.Roles, dbo.RolePermissions, dbo.UserRoles, and dbo.tbl_login.
   ============================================================================ */

-- Read-only. Does not run SwitchUser_superadmin_grant_uat.sql.
-- @LoginUserId is dbo.tbl_login.User_Id (Session USERID), not a RoleName.
-- @CompanyID is the company context argument of AuthGuard.HasPermission.

SET NOCOUNT ON;

IF DB_NAME() <> N'flamex_live'
BEGIN
    RAISERROR('Incorrect database. Expected flamex_live. No changes applied.', 16, 1);
    RETURN;
END;

DECLARE @LoginUserId nvarchar(100) = N'';
DECLARE @CompanyID int = 0;

IF OBJECT_ID(N'dbo.Permissions', N'U') IS NULL
   OR OBJECT_ID(N'dbo.Roles', N'U') IS NULL
   OR OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL
   OR OBJECT_ID(N'dbo.UserRoles', N'U') IS NULL
   OR OBJECT_ID(N'dbo.tbl_login', N'U') IS NULL
BEGIN
    SELECT
        v.ObjectName,
        CASE WHEN OBJECT_ID(v.ObjectName, N'U') IS NULL THEN N'MISSING_OBJECT' ELSE N'PRESENT' END AS Finding
    FROM (VALUES
        (N'dbo.Permissions'),
        (N'dbo.Roles'),
        (N'dbo.RolePermissions'),
        (N'dbo.UserRoles'),
        (N'dbo.tbl_login')
    ) v(ObjectName);
    RETURN;
END

SELECT
    CASE WHEN p.PermissionId IS NULL THEN N'MISSING_CATALOG_PERMISSION' ELSE N'CATALOG_PRESENT' END AS CatalogFinding,
    p.PermissionId,
    p.PermissionKey,
    p.Description,
    p.ModuleName,
    p.SubModuleName,
    p.FeatureName
FROM (SELECT 1 AS Anchor) a
LEFT JOIN dbo.Permissions p ON p.PermissionKey = N'SwitchUser';

IF @LoginUserId IS NULL OR @LoginUserId = N''
BEGIN
    SELECT
        N'INPUT_REQUIRED' AS AuthorizationGap,
        N'Set @LoginUserId to dbo.tbl_login.User_Id. Do not substitute a RoleName.' AS GapDetail;
    RETURN;
END

SELECT
    u.Id,
    COALESCE(u.User_Id, i.LoginUserId) AS User_Id,
    u.Name,
    u.RoleId,
    r.RoleName,
    u.CompanyID,
    u.IsActive,
    CASE WHEN u.Id IS NULL THEN N'ACCOUNT_NOT_FOUND' ELSE N'ACCOUNT_FOUND' END AS AccountFinding,
    CASE
        WHEN u.Id IS NULL THEN N'ACCOUNT_NOT_FOUND'
        WHEN u.RoleId IS NULL THEN N'NO_LOGIN_ROLE'
        WHEN EXISTS (
            SELECT 1
            FROM dbo.UserRoles ur
            WHERE ur.UserId = u.Id
              AND ur.RoleId = u.RoleId
        ) THEN N'LOGIN_ROLE_IN_USERROLES'
        ELSE N'MISSING_USER_ROLE_ASSIGNMENT'
    END AS LoginRoleAssignment,
    CASE
        WHEN u.Id IS NULL THEN N'ACCOUNT_NOT_FOUND'
        WHEN @CompanyID IS NULL OR @CompanyID <= 0 OR u.CompanyID <> @CompanyID THEN N'COMPANY_CONTEXT_MISMATCH'
        ELSE N'COMPANY_CONTEXT_MATCH'
    END AS CompanyContextFinding
FROM (SELECT @LoginUserId AS LoginUserId) i
LEFT JOIN dbo.tbl_login u ON u.User_Id = i.LoginUserId
LEFT JOIN dbo.Roles r ON r.RoleId = u.RoleId;

SELECT
    u.Id,
    u.User_Id,
    ur.UserRoleId,
    ur.RoleId,
    roleRow.RoleName,
    CASE
        WHEN u.Id IS NULL THEN N'ACCOUNT_NOT_FOUND'
        WHEN ur.UserRoleId IS NULL THEN N'MISSING_USER_ROLE_ASSIGNMENT'
        WHEN p.PermissionId IS NULL THEN N'MISSING_CATALOG_PERMISSION'
        WHEN rp.RolePermissionId IS NULL THEN N'MISSING_ROLE_GRANT'
        ELSE N'GRANTS_SWITCHUSER'
    END AS RoleGrantFinding
FROM (SELECT @LoginUserId AS LoginUserId) i
LEFT JOIN dbo.tbl_login u ON u.User_Id = i.LoginUserId
LEFT JOIN dbo.UserRoles ur ON ur.UserId = u.Id
LEFT JOIN dbo.Roles roleRow ON roleRow.RoleId = ur.RoleId
LEFT JOIN dbo.Permissions p ON p.PermissionKey = N'SwitchUser'
LEFT JOIN dbo.RolePermissions rp
    ON rp.RoleId = ur.RoleId
   AND rp.PermissionId = p.PermissionId;

SELECT
    s.LoginUserId,
    s.CompanyID,
    s.AccountFound,
    s.CatalogPresent,
    s.UserRoleAssigned,
    s.FallbackHit AS AssignedRoleGrantsSwitchUser,
    s.CompanyScopedHit,
    s.FallbackHit,
    CASE
        WHEN s.CompanyID IS NULL OR s.CompanyID <= 0 THEN 0
        WHEN s.CompanyScopedHit = 1 THEN 1
        WHEN s.FallbackHit = 1 THEN 1
        ELSE 0
    END AS HasPermissionResult,
    CASE
        WHEN s.AccountFound = 0 THEN N'ACCOUNT_NOT_FOUND'
        WHEN s.CatalogPresent = 0 THEN N'MISSING_CATALOG_PERMISSION'
        WHEN s.UserRoleAssigned = 0 THEN N'MISSING_USER_ROLE_ASSIGNMENT'
        WHEN s.FallbackHit = 0 THEN N'MISSING_ROLE_GRANT'
        WHEN s.CompanyScopedHit = 0 THEN N'COMPANY_CONTEXT_MISMATCH'
        ELSE N'AUTHORIZED_COMPANY_SCOPED'
    END AS AuthorizationGap,
    CASE
        WHEN s.AccountFound = 0 THEN N'No tbl_login row matches @LoginUserId.'
        WHEN s.CatalogPresent = 0 THEN N'dbo.Permissions has no PermissionKey SwitchUser.'
        WHEN s.UserRoleAssigned = 0 THEN N'No dbo.UserRoles row exists for this tbl_login.Id.'
        WHEN s.FallbackHit = 0 THEN N'Assigned UserRoles do not have a RolePermissions row for SwitchUser.'
        WHEN s.CompanyID IS NULL OR s.CompanyID <= 0 THEN N'@CompanyID is not positive, so HasPermission returns false before either query.'
        WHEN s.CompanyScopedHit = 0 THEN N'tbl_login.CompanyID does not match @CompanyID. The fallback query still finds the grant, and HasPermission returns true.'
        ELSE N'Company-scoped HasPermission query finds SwitchUser for this User_Id and @CompanyID.'
    END AS GapDetail
FROM (
    SELECT
        @LoginUserId AS LoginUserId,
        @CompanyID AS CompanyID,
        CASE WHEN EXISTS (
            SELECT 1 FROM dbo.tbl_login u WHERE u.User_Id = @LoginUserId
        ) THEN 1 ELSE 0 END AS AccountFound,
        CASE WHEN EXISTS (
            SELECT 1 FROM dbo.Permissions p WHERE p.PermissionKey = N'SwitchUser'
        ) THEN 1 ELSE 0 END AS CatalogPresent,
        CASE WHEN EXISTS (
            SELECT 1
            FROM dbo.UserRoles ur
            INNER JOIN dbo.tbl_login u ON ur.UserId = u.Id
            WHERE u.User_Id = @LoginUserId
        ) THEN 1 ELSE 0 END AS UserRoleAssigned,
        CASE WHEN @CompanyID > 0 AND EXISTS (
            SELECT TOP 1 1
            FROM dbo.Permissions p
            INNER JOIN dbo.RolePermissions rp ON p.PermissionId = rp.PermissionId
            INNER JOIN dbo.UserRoles ur ON rp.RoleId = ur.RoleId
            INNER JOIN dbo.tbl_login u ON ur.UserId = u.Id AND u.CompanyID = @CompanyID
            WHERE u.User_Id = @LoginUserId AND p.PermissionKey = N'SwitchUser'
        ) THEN 1 ELSE 0 END AS CompanyScopedHit,
        CASE WHEN EXISTS (
            SELECT TOP 1 1
            FROM dbo.Permissions p
            INNER JOIN dbo.RolePermissions rp ON p.PermissionId = rp.PermissionId
            INNER JOIN dbo.UserRoles ur ON rp.RoleId = ur.RoleId
            INNER JOIN dbo.tbl_login u ON ur.UserId = u.Id
            WHERE u.User_Id = @LoginUserId AND p.PermissionKey = N'SwitchUser'
        ) THEN 1 ELSE 0 END AS FallbackHit
) s;
