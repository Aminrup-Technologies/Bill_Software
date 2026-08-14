-- Tenant isolation columns for lookup / session tables used by ViewUser and attendance.
-- Safe to run more than once. Existing rows receive CompanyID = 1.

IF COL_LENGTH('dbo.Roles', 'CompanyID') IS NULL
    ALTER TABLE dbo.Roles ADD CompanyID INT NOT NULL CONSTRAINT DF_Roles_CompanyID DEFAULT (1);

IF COL_LENGTH('dbo.ActiveSessions', 'CompanyID') IS NULL
    ALTER TABLE dbo.ActiveSessions ADD CompanyID INT NOT NULL CONSTRAINT DF_ActiveSessions_CompanyID DEFAULT (1);
