/* ============================================================================
   NAME:        user_roles_reconciliation
   WHEN:        2026-09-07
   WHY:         Phase 1B. tbl_login.RoleId (display) and dbo.UserRoles (grants)
                were written by different screens. Existing users can have a
                display role and no UserRoles row, so menus/SecurePage stay
                empty. Do not run this automatically.
   WHAT:        Report mismatches, then insert missing (UserId, RoleId) pairs
                only. Does not delete extra UserRoles (many-to-many is valid).
                Does not rewrite tbl_login.RoleId.
   ============================================================================ */

-- 1. Report: display role has no matching UserRoles row
SELECT
    u.Id AS UserDbId,
    u.User_Id,
    u.CompanyID,
    u.RoleId AS LoginRoleId,
    r.RoleName AS LoginRoleName
FROM dbo.tbl_login u
LEFT JOIN dbo.Roles r
    ON r.RoleId = u.RoleId
   AND r.CompanyID = u.CompanyID
WHERE u.RoleId IS NOT NULL
  AND u.RoleId > 0
  AND NOT EXISTS (
        SELECT 1
        FROM dbo.UserRoles ur
        WHERE ur.UserId = u.Id
          AND ur.RoleId = u.RoleId
    )
ORDER BY u.CompanyID, u.User_Id;

-- 2. Report: UserRoles rows whose RoleId is not the display role (informational only)
SELECT
    u.Id AS UserDbId,
    u.User_Id,
    u.CompanyID,
    u.RoleId AS LoginRoleId,
    ur.RoleId AS GrantRoleId
FROM dbo.tbl_login u
INNER JOIN dbo.UserRoles ur ON ur.UserId = u.Id
WHERE u.RoleId IS NULL
   OR u.RoleId <> ur.RoleId
ORDER BY u.CompanyID, u.User_Id, ur.RoleId;

-- 3. Apply: seed missing UserRoles from tbl_login.RoleId when the role exists
--    in the user's company. Skip rows that already exist.
INSERT INTO dbo.UserRoles (UserId, RoleId)
SELECT u.Id, u.RoleId
FROM dbo.tbl_login u
INNER JOIN dbo.Roles r
    ON r.RoleId = u.RoleId
   AND r.CompanyID = u.CompanyID
WHERE u.RoleId IS NOT NULL
  AND u.RoleId > 0
  AND NOT EXISTS (
        SELECT 1
        FROM dbo.UserRoles ur
        WHERE ur.UserId = u.Id
          AND ur.RoleId = u.RoleId
    );
GO
