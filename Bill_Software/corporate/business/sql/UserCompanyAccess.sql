/* ============================================================================
   NAME:        user_company_access_schema
   WHEN:        2026-09-07
   WHY:         Phase 0A Task 2 (A-18) cannot bind the company switcher without
                an authoritative per-user company membership table. No
                UserCompanyAccess (or equivalent) table is referenced anywhere
                in this repository. tbl_login.CompanyID is a single home-tenant
                column, not a membership list. Do not invent mappings.
   WHAT:        Required DDL for dbo.UserCompanyAccess. Do not run until
                product confirms this model and a backfill source. Application
                code is NOT wired to this table in Phase 0A.
   ============================================================================ */

/*
IF OBJECT_ID(N'dbo.UserCompanyAccess', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserCompanyAccess
    (
        UserCompanyAccessId INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_UserCompanyAccess PRIMARY KEY,
        UserId              INT NOT NULL,          -- tbl_login.Id
        CompanyID           INT NOT NULL,          -- tbl_Company.ID
        IsActive            BIT NOT NULL
            CONSTRAINT DF_UserCompanyAccess_IsActive DEFAULT (1),
        CONSTRAINT UQ_UserCompanyAccess_User_Company UNIQUE (UserId, CompanyID)
    );

    ALTER TABLE dbo.UserCompanyAccess
        ADD CONSTRAINT FK_UserCompanyAccess_Login
        FOREIGN KEY (UserId) REFERENCES dbo.tbl_login (Id);

    ALTER TABLE dbo.UserCompanyAccess
        ADD CONSTRAINT FK_UserCompanyAccess_Company
        FOREIGN KEY (CompanyID) REFERENCES dbo.tbl_Company (ID);
END
GO

-- Backfill must be a product decision. Do not assume every tbl_login.CompanyID
-- row is the complete membership set (users today can select any company).
*/

-- BLOCKED: company switcher (Bill.Master.BindCompanies / ddlCompany_SelectedIndexChanged)
-- remains unrestricted until this table exists, is backfilled, and AuthGuard
-- membership checks are wired in a later phase.
