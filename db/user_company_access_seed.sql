-- ============================================================================
-- NAME:        user_company_access_seed
-- When:        2026-09-10
-- Why:         After the UserCompanyAccess rollout, company-gated pages fail
--              closed unless the user has an active home-tenant membership.
--              UAT already has ADMIN (UserId=1, CompanyID=1). Remaining active
--              users still lack a home-tenant row and will 403 on SecurePage.
-- What:        Idempotent INSERT of missing home-tenant memberships only
--              (tbl_login.CompanyID). Preserves existing rows. Does not grant
--              every company. Does not seed inactive users. DBA execute only.
-- Evidence:    flamex_uat 2026-09-10
--                tbl_login: 25 rows, 24 active, all home CompanyID=1
--                tbl_Company: 1=Flame-Ex (active), 2=AA Associates (active)
--                UserCompanyAccess: 1 row (UserId=1, CompanyID=1, IsActive=1)
--                Missing active home-tenant rows: 23
--                Inactive skipped: UserId=7 (FLM07)
-- ============================================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.UserCompanyAccess', N'U') IS NULL
BEGIN
    RAISERROR('dbo.UserCompanyAccess does not exist. Run UserCompanyAccess.sql first. No seed applied.', 16, 1);
    RETURN;
END;

BEGIN TRANSACTION;

INSERT INTO dbo.UserCompanyAccess (UserId, CompanyID, IsActive)
SELECT
    l.Id,
    l.CompanyID,
    1
FROM dbo.tbl_login AS l
INNER JOIN dbo.tbl_Company AS c
    ON c.ID = l.CompanyID
WHERE l.IsActive = 1
  AND l.CompanyID IS NOT NULL
  AND l.CompanyID > 0
  AND ISNULL(c.IsActive, 1) = 1
  AND NOT EXISTS (
        SELECT 1
        FROM dbo.UserCompanyAccess AS a
        WHERE a.UserId = l.Id
          AND a.CompanyID = l.CompanyID
    );

PRINT CONCAT('user_company_access_seed rows inserted: ', @@ROWCOUNT);

COMMIT TRANSACTION;
GO

-- ============================================================================
-- VERIFICATION (read-only; run after the INSERT)
-- Expected on flamex_uat after a clean first run:
--   1. TotalActiveUsers              = 24
--   2. TotalActiveMembershipRows     = 24
--   3. ActiveUsersMissingHomeTenant  = 0 rows
--   4. DuplicateUserCompanyPairs     = 0 rows
--   5. UserId=1                      = one row, CompanyID=1, IsActive=1
-- ============================================================================

-- 1. Total active users
SELECT COUNT(*) AS TotalActiveUsers
FROM dbo.tbl_login
WHERE IsActive = 1;

-- 2. Total active membership rows
SELECT COUNT(*) AS TotalActiveMembershipRows
FROM dbo.UserCompanyAccess
WHERE IsActive = 1;

-- 3. Active users still missing a home-tenant row
SELECT
    l.Id AS UserId,
    l.User_Id,
    l.Name,
    l.CompanyID AS HomeCompanyID,
    c.Name AS CompanyName
FROM dbo.tbl_login AS l
LEFT JOIN dbo.tbl_Company AS c
    ON c.ID = l.CompanyID
WHERE l.IsActive = 1
  AND l.CompanyID IS NOT NULL
  AND l.CompanyID > 0
  AND NOT EXISTS (
        SELECT 1
        FROM dbo.UserCompanyAccess AS a
        WHERE a.UserId = l.Id
          AND a.CompanyID = l.CompanyID
          AND a.IsActive = 1
    )
ORDER BY l.Id;

-- 4. Duplicate (UserId, CompanyID) pairs
SELECT
    UserId,
    CompanyID,
    COUNT(*) AS DuplicateCount
FROM dbo.UserCompanyAccess
GROUP BY UserId, CompanyID
HAVING COUNT(*) > 1
ORDER BY UserId, CompanyID;

-- 5. Sample verification for UserId = 1
SELECT
    a.UserCompanyAccessId,
    a.UserId,
    l.User_Id,
    l.Name,
    a.CompanyID,
    c.Name AS CompanyName,
    a.IsActive
FROM dbo.UserCompanyAccess AS a
INNER JOIN dbo.tbl_login AS l
    ON l.Id = a.UserId
INNER JOIN dbo.tbl_Company AS c
    ON c.ID = a.CompanyID
WHERE a.UserId = 1;
