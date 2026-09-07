/* ============================================================================
   NAME:        user_company_access_schema
   WHEN:        2026-09-07
   WHY:         Phase 2A (A-18) binds the company switcher to per-user membership.
                tbl_login.CompanyID is a single home-tenant column, not a
                membership list. Session["CompanyID"] remains the runtime tenant.
   WHAT:        DDL for dbo.UserCompanyAccess. Application code is wired in
                Phase 2A and fails closed when this table is missing or empty.
                Do not execute from the application. Do not grant all users
                all companies. Optional home-tenant inserts are in
                user_company_access_reconciliation.sql (DBA review only).
   ============================================================================ */

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
