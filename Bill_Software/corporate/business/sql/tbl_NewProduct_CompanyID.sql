IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[tbl_NewProduct]')
    AND name = 'CompanyID'
)
BEGIN
    ALTER TABLE [dbo].[tbl_NewProduct]
    ADD CompanyID INT NOT NULL CONSTRAINT DF_tbl_NewProduct_CompanyID DEFAULT (1);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[tbl_NewparentProduct]')
    AND name = 'CompanyID'
)
BEGIN
    ALTER TABLE [dbo].[tbl_NewparentProduct]
    ADD CompanyID INT NOT NULL CONSTRAINT DF_tbl_NewparentProduct_CompanyID DEFAULT (1);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[tbl_NewProduct]')
    AND name = 'Purches_Rate'
)
BEGIN
    ALTER TABLE [dbo].[tbl_NewProduct]
    ADD Purches_Rate DECIMAL(18,2) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[tbl_NewProduct]')
    AND name = 'OEMUrl'
)
BEGIN
    ALTER TABLE [dbo].[tbl_NewProduct]
    ADD OEMUrl NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[tbl_NewProduct]')
    AND name = 'ProductImage'
)
BEGIN
    ALTER TABLE [dbo].[tbl_NewProduct]
    ADD ProductImage NVARCHAR(500) NULL;
END
GO
