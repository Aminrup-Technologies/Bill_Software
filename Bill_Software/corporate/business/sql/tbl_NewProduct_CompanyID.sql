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
