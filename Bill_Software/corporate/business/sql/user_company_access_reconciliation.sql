/* ============================================================================
   NAME:        user_company_access_reconciliation
   WHEN:        2026-09-07
   WHY:         Phase 2A (A-18). UserCompanyAccess starts empty. The company
                switcher now fails closed unless the user has an active
                membership row. Existing product data only clearly names a
                single home tenant (tbl_login.CompanyID), not every company.
   WHAT:        Report missing / inactive / orphaned memberships. Optional
                INSERT of home-tenant rows only. Does not grant all companies.
                Does not execute from the application. DBA review required.
   ============================================================================ */

SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.UserCompanyAccess', N'U') IS NULL
BEGIN
    RAISERROR('dbo.UserCompanyAccess does not exist. Run UserCompanyAccess.sql first. No backfill applied.', 16, 1);
    RETURN;
END

-- 1. Report: login home CompanyID has no active membership row
SELECT
    l.Id AS UserDbId,
    l.User_Id,
    l.CompanyID AS HomeCompanyID,
    c.Name AS HomeCompanyName,
    c.IsActive AS CompanyIsActive
FROM dbo.tbl_login AS l
LEFT JOIN dbo.tbl_Company AS c
    ON c.ID = l.CompanyID
WHERE l.CompanyID IS NOT NULL
  AND l.CompanyID > 0
  AND NOT EXISTS (
        SELECT 1
        FROM dbo.UserCompanyAccess AS a
        WHERE a.UserId = l.Id
          AND a.CompanyID = l.CompanyID
          AND a.IsActive = 1
    )
ORDER BY l.Id;

-- 2. Report: active memberships whose company is missing or inactive
SELECT
    a.UserCompanyAccessId,
    a.UserId,
    a.CompanyID,
    a.IsActive AS MembershipIsActive,
    c.ID AS CompanyRowId,
    c.IsActive AS CompanyIsActive
FROM dbo.UserCompanyAccess AS a
LEFT JOIN dbo.tbl_Company AS c
    ON c.ID = a.CompanyID
WHERE a.IsActive = 1
  AND (c.ID IS NULL OR ISNULL(c.IsActive, 1) = 0)
ORDER BY a.UserId, a.CompanyID;

-- 3. Report: memberships whose UserId is not a tbl_login.Id
SELECT
    a.UserCompanyAccessId,
    a.UserId,
    a.CompanyID
FROM dbo.UserCompanyAccess AS a
WHERE NOT EXISTS (
        SELECT 1
        FROM dbo.tbl_login AS l
        WHERE l.Id = a.UserId
    )
ORDER BY a.UserId;

-- 4. Report: users with more than one active membership (selector required)
SELECT
    a.UserId,
    l.User_Id,
    COUNT(*) AS ActiveMembershipCount
FROM dbo.UserCompanyAccess AS a
INNER JOIN dbo.tbl_login AS l
    ON l.Id = a.UserId
WHERE a.IsActive = 1
GROUP BY a.UserId, l.User_Id
HAVING COUNT(*) > 1
ORDER BY a.UserId;

-- 5. Optional INSERT: home tenant only (tbl_login.CompanyID).
-- Skipped when CompanyID is null, company is missing, or a row already exists.
-- Does NOT insert every company for every user.
-- Uncomment after reviewing the reports above.

/*
INSERT INTO dbo.UserCompanyAccess (UserId, CompanyID, IsActive)
SELECT
    l.Id,
    l.CompanyID,
    1
FROM dbo.tbl_login AS l
INNER JOIN dbo.tbl_Company AS c
    ON c.ID = l.CompanyID
WHERE l.CompanyID IS NOT NULL
  AND l.CompanyID > 0
  AND ISNULL(c.IsActive, 1) = 1
  AND NOT EXISTS (
        SELECT 1
        FROM dbo.UserCompanyAccess AS a
        WHERE a.UserId = l.Id
          AND a.CompanyID = l.CompanyID
    );
*/
