/* ============================================================================
   NAME:        user_company_access_admin_aa_associates
   WHEN:        2026-09-15
   WHY:         Master company switcher binds only UserCompanyAccess memberships.
                admin (tbl_login.Id = 1) had home-tenant Flame-Ex (CompanyID 1)
                only. AA Associates (CompanyID 2) was active in tbl_Company but
                had zero membership rows, so the dropdown could not show it
                without weakening authorization.
   WHAT:        Idempotent INSERT of active membership for admin → CompanyID 2.
                Does not grant all companies to all users. DBA review / UAT apply.
   ============================================================================ */

SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.UserCompanyAccess', N'U') IS NULL
BEGIN
    RAISERROR('dbo.UserCompanyAccess does not exist. Run UserCompanyAccess.sql first.', 16, 1);
    RETURN;
END

DECLARE @AdminUserDbId INT = NULL;
DECLARE @TargetCompanyId INT = 2;

SELECT @AdminUserDbId = l.Id
FROM dbo.tbl_login AS l
WHERE l.User_Id = N'admin'
  AND l.IsActive = 1;

IF @AdminUserDbId IS NULL
BEGIN
    RAISERROR('Active admin login not found in dbo.tbl_login.', 16, 1);
    RETURN;
END

IF NOT EXISTS (
    SELECT 1
    FROM dbo.tbl_Company AS c
    WHERE c.ID = @TargetCompanyId
      AND (c.IsActive = 1 OR c.IsActive IS NULL)
)
BEGIN
    RAISERROR('Target company 2 is missing or inactive in dbo.tbl_Company.', 16, 1);
    RETURN;
END

IF NOT EXISTS (
    SELECT 1
    FROM dbo.UserCompanyAccess AS a
    WHERE a.UserId = @AdminUserDbId
      AND a.CompanyID = @TargetCompanyId
)
BEGIN
    INSERT INTO dbo.UserCompanyAccess (UserId, CompanyID, IsActive)
    VALUES (@AdminUserDbId, @TargetCompanyId, 1);
END
ELSE
BEGIN
    UPDATE dbo.UserCompanyAccess
    SET IsActive = 1
    WHERE UserId = @AdminUserDbId
      AND CompanyID = @TargetCompanyId
      AND IsActive = 0;
END

-- Evidence
SELECT
    a.UserCompanyAccessId,
    a.UserId,
    l.User_Id,
    a.CompanyID,
    c.Name AS CompanyName,
    a.IsActive
FROM dbo.UserCompanyAccess AS a
INNER JOIN dbo.tbl_login AS l ON l.Id = a.UserId
INNER JOIN dbo.tbl_Company AS c ON c.ID = a.CompanyID
WHERE a.UserId = @AdminUserDbId
  AND a.IsActive = 1
ORDER BY a.CompanyID;
