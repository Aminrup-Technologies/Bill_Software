/* ============================================================================
   NAME:        requisition_po_companyid
   WHEN:        2026-08-16
   WHY:         Tenant-isolate PR/PO; backfill CompanyID from creator/vendor; align TVP HSNCode
   WHAT:        CompanyID columns + backfill; PR/PO SPs with @CompanyID; rebuild RequisitionItem_TVP
   ============================================================================ */

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[tbl_RequisitionMain]') AND name = N'CompanyID'
)
BEGIN
    ALTER TABLE [dbo].[tbl_RequisitionMain]
    ADD CompanyID INT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[tbl_RequisitionNew]') AND name = N'CompanyID'
)
BEGIN
    ALTER TABLE [dbo].[tbl_RequisitionNew]
    ADD CompanyID INT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[tbl_PO_Header]') AND name = N'CompanyID'
)
BEGIN
    ALTER TABLE [dbo].[tbl_PO_Header]
    ADD CompanyID INT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[tbl_PO_Items]') AND name = N'CompanyID'
)
BEGIN
    ALTER TABLE [dbo].[tbl_PO_Items]
    ADD CompanyID INT NULL;
END
GO

-- Remap from creator company (fixes DEFAULT(1) legacy stamp and NULL adds)
IF COL_LENGTH(N'dbo.tbl_RequisitionMain', N'CompanyID') IS NOT NULL
   AND COL_LENGTH(N'dbo.tbl_login', N'CompanyID') IS NOT NULL
BEGIN
    UPDATE m
    SET m.CompanyID = L.CompanyID
    FROM dbo.tbl_RequisitionMain m
    INNER JOIN dbo.tbl_login L ON L.User_Id = m.CreatedBy
    WHERE L.CompanyID IS NOT NULL AND L.CompanyID <> 0
      AND ISNULL(m.CompanyID, 0) <> L.CompanyID;
END
GO

-- Else remap from vendor company when creator has no usable CompanyID
IF COL_LENGTH(N'dbo.tbl_RequisitionMain', N'CompanyID') IS NOT NULL
   AND COL_LENGTH(N'dbo.tbl_Vendor', N'CompanyID') IS NOT NULL
BEGIN
    UPDATE m
    SET m.CompanyID = V.CompanyID
    FROM dbo.tbl_RequisitionMain m
    INNER JOIN dbo.tbl_Vendor V ON V.Id = m.VendorId
    WHERE V.CompanyID IS NOT NULL AND V.CompanyID <> 0
      AND ISNULL(m.CompanyID, 0) <> V.CompanyID
      AND NOT EXISTS (
          SELECT 1
          FROM dbo.tbl_login L
          WHERE L.User_Id = m.CreatedBy
            AND L.CompanyID IS NOT NULL
            AND L.CompanyID <> 0
      );
END
GO

UPDATE dbo.tbl_RequisitionMain SET CompanyID = 1 WHERE CompanyID IS NULL;
GO

IF COL_LENGTH(N'dbo.tbl_RequisitionNew', N'CompanyID') IS NOT NULL
   AND COL_LENGTH(N'dbo.tbl_RequisitionMain', N'CompanyID') IS NOT NULL
BEGIN
    UPDATE I
    SET I.CompanyID = M.CompanyID
    FROM dbo.tbl_RequisitionNew I
    INNER JOIN dbo.tbl_RequisitionMain M ON M.ReqNo = I.ReqNo
    WHERE ISNULL(I.CompanyID, 0) <> M.CompanyID;
END
GO

UPDATE dbo.tbl_RequisitionNew SET CompanyID = 1 WHERE CompanyID IS NULL;
GO

-- PO header: prefer linked PR, then creator, then vendor
IF COL_LENGTH(N'dbo.tbl_PO_Header', N'CompanyID') IS NOT NULL
   AND COL_LENGTH(N'dbo.tbl_RequisitionMain', N'CompanyID') IS NOT NULL
BEGIN
    UPDATE H
    SET H.CompanyID = M.CompanyID
    FROM dbo.tbl_PO_Header H
    INNER JOIN dbo.tbl_RequisitionMain M ON M.ReqNo = H.ReqNo
    WHERE H.ReqNo IS NOT NULL AND LTRIM(RTRIM(H.ReqNo)) <> ''
      AND ISNULL(H.CompanyID, 0) <> M.CompanyID;
END
GO

IF COL_LENGTH(N'dbo.tbl_PO_Header', N'CompanyID') IS NOT NULL
   AND COL_LENGTH(N'dbo.tbl_login', N'CompanyID') IS NOT NULL
BEGIN
    UPDATE H
    SET H.CompanyID = L.CompanyID
    FROM dbo.tbl_PO_Header H
    INNER JOIN dbo.tbl_login L ON L.User_Id = H.CreatedBy
    WHERE L.CompanyID IS NOT NULL AND L.CompanyID <> 0
      AND ISNULL(H.CompanyID, 0) <> L.CompanyID;
END
GO

IF COL_LENGTH(N'dbo.tbl_PO_Header', N'CompanyID') IS NOT NULL
   AND COL_LENGTH(N'dbo.tbl_Vendor', N'CompanyID') IS NOT NULL
BEGIN
    UPDATE H
    SET H.CompanyID = V.CompanyID
    FROM dbo.tbl_PO_Header H
    INNER JOIN dbo.tbl_Vendor V ON V.Id = H.VendorId
    WHERE V.CompanyID IS NOT NULL AND V.CompanyID <> 0
      AND ISNULL(H.CompanyID, 0) <> V.CompanyID
      AND NOT EXISTS (
          SELECT 1
          FROM dbo.tbl_login L
          WHERE L.User_Id = H.CreatedBy
            AND L.CompanyID IS NOT NULL
            AND L.CompanyID <> 0
      )
      AND (
          H.ReqNo IS NULL OR LTRIM(RTRIM(H.ReqNo)) = ''
          OR NOT EXISTS (
              SELECT 1 FROM dbo.tbl_RequisitionMain M WHERE M.ReqNo = H.ReqNo
          )
      );
END
GO

UPDATE dbo.tbl_PO_Header SET CompanyID = 1 WHERE CompanyID IS NULL;
GO

IF COL_LENGTH(N'dbo.tbl_PO_Items', N'CompanyID') IS NOT NULL
   AND COL_LENGTH(N'dbo.tbl_PO_Header', N'CompanyID') IS NOT NULL
BEGIN
    UPDATE I
    SET I.CompanyID = H.CompanyID
    FROM dbo.tbl_PO_Items I
    INNER JOIN dbo.tbl_PO_Header H ON H.PO_Id = I.PO_Id
    WHERE ISNULL(I.CompanyID, 0) <> H.CompanyID;
END
GO

UPDATE dbo.tbl_PO_Items SET CompanyID = 1 WHERE CompanyID IS NULL;
GO

-- Harden to NOT NULL + default for new rows
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[tbl_RequisitionMain]') AND name = N'CompanyID' AND is_nullable = 1
)
    ALTER TABLE [dbo].[tbl_RequisitionMain] ALTER COLUMN CompanyID INT NOT NULL;
GO
IF NOT EXISTS (
    SELECT 1
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.tbl_RequisitionMain')
      AND c.name = N'CompanyID'
)
   AND COL_LENGTH(N'dbo.tbl_RequisitionMain', N'CompanyID') IS NOT NULL
    ALTER TABLE [dbo].[tbl_RequisitionMain] ADD CONSTRAINT DF_tbl_RequisitionMain_CompanyID DEFAULT (1) FOR CompanyID;
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[tbl_RequisitionNew]') AND name = N'CompanyID' AND is_nullable = 1
)
    ALTER TABLE [dbo].[tbl_RequisitionNew] ALTER COLUMN CompanyID INT NOT NULL;
GO
IF NOT EXISTS (
    SELECT 1
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.tbl_RequisitionNew')
      AND c.name = N'CompanyID'
)
   AND COL_LENGTH(N'dbo.tbl_RequisitionNew', N'CompanyID') IS NOT NULL
    ALTER TABLE [dbo].[tbl_RequisitionNew] ADD CONSTRAINT DF_tbl_RequisitionNew_CompanyID DEFAULT (1) FOR CompanyID;
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[tbl_PO_Header]') AND name = N'CompanyID' AND is_nullable = 1
)
    ALTER TABLE [dbo].[tbl_PO_Header] ALTER COLUMN CompanyID INT NOT NULL;
GO
IF NOT EXISTS (
    SELECT 1
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.tbl_PO_Header')
      AND c.name = N'CompanyID'
)
   AND COL_LENGTH(N'dbo.tbl_PO_Header', N'CompanyID') IS NOT NULL
    ALTER TABLE [dbo].[tbl_PO_Header] ADD CONSTRAINT DF_tbl_PO_Header_CompanyID DEFAULT (1) FOR CompanyID;
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[tbl_PO_Items]') AND name = N'CompanyID' AND is_nullable = 1
)
    ALTER TABLE [dbo].[tbl_PO_Items] ALTER COLUMN CompanyID INT NOT NULL;
GO
IF NOT EXISTS (
    SELECT 1
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.tbl_PO_Items')
      AND c.name = N'CompanyID'
)
   AND COL_LENGTH(N'dbo.tbl_PO_Items', N'CompanyID') IS NOT NULL
    ALTER TABLE [dbo].[tbl_PO_Items] ADD CONSTRAINT DF_tbl_PO_Items_CompanyID DEFAULT (1) FOR CompanyID;
GO

DECLARE @TvpNeedsRebuild BIT = 0;
IF TYPE_ID(N'dbo.RequisitionItem_TVP') IS NULL
    SET @TvpNeedsRebuild = 1;
ELSE IF NOT EXISTS (
    SELECT 1
    FROM sys.table_types tt
    INNER JOIN sys.columns c ON c.object_id = tt.type_table_object_id
    WHERE tt.name = N'RequisitionItem_TVP'
      AND SCHEMA_NAME(tt.schema_id) = N'dbo'
      AND c.name = N'HSNCode'
)
    SET @TvpNeedsRebuild = 1;

IF @TvpNeedsRebuild = 1
BEGIN
    IF OBJECT_ID(N'dbo.sp_RequisitionItem_BulkUpsert', N'P') IS NOT NULL
        DROP PROCEDURE dbo.sp_RequisitionItem_BulkUpsert;

    IF TYPE_ID(N'dbo.RequisitionItem_TVP') IS NOT NULL
        DROP TYPE dbo.RequisitionItem_TVP;

    CREATE TYPE dbo.RequisitionItem_TVP AS TABLE
    (
        ProductId         VARCHAR(250)  NULL,
        ProductName       VARCHAR(250)  NULL,
        ParentCategoryId  INT           NULL,
        HSNCode           VARCHAR(50)   NULL,
        Description       NVARCHAR(MAX) NULL,
        Qnty              DECIMAL(18,2) NULL,
        Rate              DECIMAL(18,2) NULL,
        DiscountPercent   DECIMAL(9,2)  NULL,
        DiscountAmount    DECIMAL(18,2) NULL,
        IsTaxApplicable   BIT           NULL,
        GST               DECIMAL(5,2)  NULL,
        ItemOrder         INT           NULL
    );
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Requisition_CreateDraft
    @ClientName VARCHAR(250),
    @VendorId   INT,
    @CreatedBy  VARCHAR(100),
    @ReqNo      VARCHAR(250) OUTPUT,
    @CompanyID  INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Seq INT;
    SELECT @Seq = ISNULL(MAX(TRY_CAST(RIGHT(ReqNo, 6) AS INT)), 0) + 1
    FROM dbo.tbl_RequisitionMain WITH (UPDLOCK, HOLDLOCK)
    WHERE CompanyID = @CompanyID;

    SET @ReqNo = 'PR/' + CONVERT(VARCHAR(8), GETDATE(), 112) + '/' + RIGHT('000000' + CAST(@Seq AS VARCHAR(6)), 6);

    INSERT INTO dbo.tbl_RequisitionMain
    (
        clientName, Vendor, VendorId, ReqNo, Status,
        CreatedBy, CreatedOn, Date, CompanyID
    )
    VALUES
    (
        @ClientName, @ClientName, @VendorId, @ReqNo, 'Draft',
        @CreatedBy, GETDATE(), CONVERT(VARCHAR(30), GETDATE(), 106), @CompanyID
    );
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_RequisitionItem_BulkUpsert
    @ClientName VARCHAR(250),
    @ReqNo      VARCHAR(250),
    @UserId     VARCHAR(100),
    @Items      dbo.RequisitionItem_TVP READONLY,
    @CompanyID  INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.tbl_RequisitionMain
        WHERE ReqNo = @ReqNo AND CompanyID = @CompanyID
    )
        THROW 50001, 'Requisition not found for company.', 1;

    ;WITH Parent AS (
        SELECT ReqNo, CompanyID
        FROM dbo.tbl_RequisitionMain
        WHERE ReqNo = @ReqNo AND CompanyID = @CompanyID
    )
    DELETE I
    FROM dbo.tbl_RequisitionNew I
    INNER JOIN Parent P ON P.ReqNo = I.ReqNo AND P.CompanyID = I.CompanyID
    WHERE NOT EXISTS (
        SELECT 1 FROM @Items T WHERE T.ProductId = I.ProductId
    );

    MERGE dbo.tbl_RequisitionNew AS T
    USING (
        SELECT
            @ReqNo AS ReqNo,
            @CompanyID AS CompanyID,
            @ClientName AS Clientname,
            I.ProductId,
            I.ProductName,
            I.ParentCategoryId,
            I.Description,
            I.Qnty,
            I.Rate,
            I.DiscountPercent,
            I.DiscountAmount,
            I.IsTaxApplicable,
            I.GST,
            I.ItemOrder,
            CAST((ISNULL(I.Qnty, 0) * ISNULL(I.Rate, 0)) - ISNULL(I.DiscountAmount, 0) AS DECIMAL(18,2)) AS TaxableAmount
        FROM @Items I
    ) AS S
    ON T.ReqNo = S.ReqNo
       AND T.CompanyID = S.CompanyID
       AND T.ProductId = S.ProductId
    WHEN MATCHED THEN
        UPDATE SET
            ProductName = S.ProductName,
            ParentCategoryId = S.ParentCategoryId,
            Description = S.Description,
            Qnty = S.Qnty,
            Rate = S.Rate,
            DiscountPercent = S.DiscountPercent,
            DiscountAmount = S.DiscountAmount,
            IsTaxApplicable = S.IsTaxApplicable,
            gstrate = S.GST,
            ItemOrder = S.ItemOrder,
            TaxableAmount = S.TaxableAmount,
            Clientname = S.Clientname
    WHEN NOT MATCHED THEN
        INSERT
        (
            ReqNo, CompanyID, Clientname, ProductId, ProductName, ParentCategoryId,
            Description, Qnty, Rate, DiscountPercent, DiscountAmount,
            IsTaxApplicable, gstrate, ItemOrder, TaxableAmount
        )
        VALUES
        (
            S.ReqNo, S.CompanyID, S.Clientname, S.ProductId, S.ProductName, S.ParentCategoryId,
            S.Description, S.Qnty, S.Rate, S.DiscountPercent, S.DiscountAmount,
            S.IsTaxApplicable, S.GST, S.ItemOrder, S.TaxableAmount
        );
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_SubmitRequisition
    @ReqNo     VARCHAR(250),
    @UserId    VARCHAR(100),
    @CompanyID INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tbl_RequisitionMain
    SET Status = 'Submitted',
        SubmittedBy = @UserId,
        SubmittedOn = GETDATE()
    WHERE ReqNo = @ReqNo
      AND CompanyID = @CompanyID
      AND Status = 'Draft';

    IF @@ROWCOUNT = 0
        THROW 50002, 'Submit failed: draft not found for company.', 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_CancelRequisition
    @ReqNo        VARCHAR(250),
    @CancelledBy  VARCHAR(100),
    @CancelReason VARCHAR(500) = NULL,
    @CompanyID    INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tbl_RequisitionMain
    SET Status = 'Cancelled'
    WHERE ReqNo = @ReqNo
      AND CompanyID = @CompanyID
      AND Status IN ('Draft', 'Submitted');

    IF @@ROWCOUNT = 0
        THROW 50003, 'Cancel failed: requisition not found for company.', 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Requisition_Approve
    @ReqNo          VARCHAR(250),
    @ApproverUserId VARCHAR(100),
    @Action         VARCHAR(50),
    @Remarks        VARCHAR(500) = NULL,
    @CompanyID      INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NewStatus VARCHAR(50) =
        CASE WHEN @Action = 'Approved' THEN 'Approved' ELSE 'Rejected' END;

    UPDATE dbo.tbl_RequisitionMain
    SET Status = @NewStatus,
        ApprovedBy = @ApproverUserId,
        ApprovedOn = GETDATE()
    WHERE ReqNo = @ReqNo
      AND CompanyID = @CompanyID
      AND Status = 'Submitted';

    IF @@ROWCOUNT = 0
        THROW 50004, 'Approval failed: submitted requisition not found for company.', 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_GeneratePO_FromReqNo
    @ReqNo     VARCHAR(250),
    @UserId    VARCHAR(100),
    @CompanyID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.tbl_RequisitionMain
        WHERE ReqNo = @ReqNo AND CompanyID = @CompanyID AND Status = 'Approved'
    )
        THROW 50010, 'Approved PR not found for company.', 1;

    IF EXISTS (
        SELECT 1 FROM dbo.tbl_PO_Header
        WHERE ReqNo = @ReqNo AND CompanyID = @CompanyID
    )
        THROW 50011, 'A PO already exists for this PR.', 1;

    DECLARE @VendorId INT;
    SELECT @VendorId = VendorId
    FROM dbo.tbl_RequisitionMain
    WHERE ReqNo = @ReqNo AND CompanyID = @CompanyID;

    DECLARE @Seq INT, @PONo VARCHAR(250);
    SELECT @Seq = ISNULL(MAX(TRY_CAST(RIGHT(PO_No, 6) AS INT)), 0) + 1
    FROM dbo.tbl_PO_Header WITH (UPDLOCK, HOLDLOCK)
    WHERE CompanyID = @CompanyID;

    SET @PONo = 'PO/' + CONVERT(VARCHAR(8), GETDATE(), 112) + '/' + RIGHT('000000' + CAST(@Seq AS VARCHAR(6)), 6);

    IF NOT EXISTS (
        SELECT 1 FROM dbo.tbl_RequisitionNew
        WHERE ReqNo = @ReqNo AND CompanyID = @CompanyID
    )
        THROW 50012, 'No PR line items found for company.', 1;

    DECLARE @PO_Id INT;

    INSERT INTO dbo.tbl_PO_Header
    (PO_No, ReqNo, VendorId, PO_Date, PO_Status, IsLocked, CreatedBy, CreatedOn, CompanyID)
    VALUES
    (@PONo, @ReqNo, @VendorId, GETDATE(), 'Draft', 0, @UserId, GETDATE(), @CompanyID);

    SET @PO_Id = SCOPE_IDENTITY();

    INSERT INTO dbo.tbl_PO_Items
    (PO_Id, ProductId, ProductName, Quantity, Rate, DiscountPercent, DiscountAmount,
     TaxableAmount, TaxRate, TaxAmount, NetAmount, ItemOrder, CompanyID)
    SELECT
        @PO_Id,
        ISNULL(R.ProductId, ''),
        R.ProductName,
        R.Qnty,
        R.Rate,
        ISNULL(R.DiscountPercent, 0),
        ISNULL(R.DiscountAmount, 0),
        ISNULL(R.TaxableAmount, 0),
        ISNULL(R.gstrate, 0),
        CAST(CASE WHEN ISNULL(R.IsTaxApplicable, 0) = 1
             THEN ISNULL(R.TaxableAmount, 0) * ISNULL(CAST(R.gstrate AS DECIMAL(5,2)), 0) / 100.0
             ELSE 0 END AS DECIMAL(18,2)),
        CAST(ISNULL(R.TaxableAmount, 0) +
             CASE WHEN ISNULL(R.IsTaxApplicable, 0) = 1
                  THEN ISNULL(R.TaxableAmount, 0) * ISNULL(CAST(R.gstrate AS DECIMAL(5,2)), 0) / 100.0
                  ELSE 0 END AS DECIMAL(18,2)),
        R.ItemOrder,
        @CompanyID
    FROM dbo.tbl_RequisitionNew R
    WHERE R.ReqNo = @ReqNo AND R.CompanyID = @CompanyID;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_ReleasePO_Final
    @PO_Id     INT,
    @UserId    VARCHAR(100),
    @CompanyID INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.tbl_PO_Header
    SET PO_Status = 'Released',
        IsLocked = 1
    WHERE PO_Id = @PO_Id
      AND CompanyID = @CompanyID
      AND PO_Status = 'Draft'
      AND IsLocked = 0;

    IF @@ROWCOUNT = 0
        THROW 50020, 'Release failed: draft PO not found for company.', 1;
END
GO
