-- Geo-fence fallback override columns for tbl_Attendance
-- Safe to run more than once.

IF COL_LENGTH('dbo.tbl_Attendance', 'IsGeoFenceOverridden') IS NULL
    ALTER TABLE dbo.tbl_Attendance ADD IsGeoFenceOverridden BIT NOT NULL CONSTRAINT DF_tbl_Attendance_IsGeoFenceOverridden DEFAULT (0);

IF COL_LENGTH('dbo.tbl_Attendance', 'LocationTypeOverride') IS NULL
    ALTER TABLE dbo.tbl_Attendance ADD LocationTypeOverride NVARCHAR(100) NULL;

IF COL_LENGTH('dbo.tbl_Attendance', 'OverrideReason') IS NULL
    ALTER TABLE dbo.tbl_Attendance ADD OverrideReason NVARCHAR(500) NULL;
