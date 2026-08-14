-- Per-user geo-fence fallback controllers on tbl_login
-- Safe to run more than once.

IF COL_LENGTH('dbo.tbl_login', 'AllowGeoFenceOverride') IS NULL
    ALTER TABLE dbo.tbl_login ADD AllowGeoFenceOverride BIT NOT NULL CONSTRAINT DF_tbl_login_AllowGeoFenceOverride DEFAULT (1);

IF COL_LENGTH('dbo.tbl_login', 'MaxGeoFenceAttempts') IS NULL
    ALTER TABLE dbo.tbl_login ADD MaxGeoFenceAttempts INT NOT NULL CONSTRAINT DF_tbl_login_MaxGeoFenceAttempts DEFAULT (3);
