IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('tbl_NewparentProduct') 
    AND name = 'CompanyID'
)
BEGIN
    ALTER TABLE tbl_NewparentProduct 
    ADD CompanyID INT NOT NULL DEFAULT (1);
END
