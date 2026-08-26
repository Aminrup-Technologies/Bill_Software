-- When: 2026-08-16
-- Why: Tenant-scope purchase-order header/detail persistence and stable line ordering.
-- What: Idempotently ensure tbl_Quotation.CompanyID, tbl_Quotaion_details.CompanyID (INT NOT NULL DEFAULT 1),
--       and a single INT row-order column on tbl_Quotaion_details (prefer existing Sl_no; else ItemOrder).
--       Preserves existing data; does not add a duplicate ordering column.

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[tbl_Quotation]')
      AND name = N'CompanyID'
)
BEGIN
    ALTER TABLE [dbo].[tbl_Quotation]
    ADD CompanyID INT NOT NULL CONSTRAINT DF_tbl_Quotation_CompanyID DEFAULT (1);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[tbl_Quotaion_details]')
      AND name = N'CompanyID'
)
BEGIN
    ALTER TABLE [dbo].[tbl_Quotaion_details]
    ADD CompanyID INT NOT NULL CONSTRAINT DF_tbl_Quotaion_details_CompanyID DEFAULT (1);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[tbl_Quotaion_details]')
      AND name = N'Sl_no'
)
AND NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[tbl_Quotaion_details]')
      AND name = N'ItemOrder'
)
BEGIN
    ALTER TABLE [dbo].[tbl_Quotaion_details]
    ADD Sl_no INT NULL;
END
GO
