/*
   NAME:        live_nav_28_catalog_seed
   WHEN:        2026-11-15
   WHY:         Complete missing Permission catalog metadata for the 28 unresolved navigation permissions.
   WHAT:
   - Insert 28 repository-derived Permission catalog records.
   - Validate existing metadata and fail on conflicting records.
   - Classify conflicts/missing rows and insert under one SERIALIZABLE transaction
     with UPDLOCK/HOLDLOCK on matching Permissions rows (race-safe; idempotent).
   - Reject ambient transactions (@@TRANCOUNT <> 0) before session mutation / DML.
   - Capture and restore caller XACT_ABORT and isolation level (success + failure).
*/
-- INERT / DO NOT EXECUTE FROM THE APPLICATION.
-- Separate written approval required before any flamex_live execution.
-- Catalog only. No RolePermissions / UserRoles. No DDL / DELETE / UPDATE.
-- Metadata is NEWLY DERIVED from Bill.Master + page evidence (not recovered historical tuples).
-- Excludes AdminAttendanceDashboard (grant-owned), AdminShiftAssignment, AdminOverride (PR #115 seed).
-- PermissionId is IDENTITY and is never supplied.
-- #Expected column lengths match UAT_CATALOG dbo.Permissions (200/500/100/100/100).

-----------------------------------------------------------------------------
-- 0. Database guard (fail closed)
-----------------------------------------------------------------------------
IF DB_NAME() <> N'flamex_live'
BEGIN
    RAISERROR('live_nav_28_catalog_seed.sql requires DB_NAME() = N''flamex_live''. No changes applied.', 16, 1);
    RETURN;
END;

IF OBJECT_ID(N'dbo.Permissions', N'U') IS NULL
BEGIN
    RAISERROR('dbo.Permissions does not exist. No 28-key LIVE catalog rows applied.', 16, 1);
    RETURN;
END;

-----------------------------------------------------------------------------
-- 1. Required columns present
-----------------------------------------------------------------------------
IF COL_LENGTH(N'dbo.Permissions', N'PermissionId') IS NULL
   OR COL_LENGTH(N'dbo.Permissions', N'PermissionKey') IS NULL
   OR COL_LENGTH(N'dbo.Permissions', N'Description') IS NULL
   OR COL_LENGTH(N'dbo.Permissions', N'ModuleName') IS NULL
   OR COL_LENGTH(N'dbo.Permissions', N'SubModuleName') IS NULL
   OR COL_LENGTH(N'dbo.Permissions', N'FeatureName') IS NULL
BEGIN
    RAISERROR('Precondition failed: dbo.Permissions is missing one or more required columns (PermissionId, PermissionKey, Description, ModuleName, SubModuleName, FeatureName). Aborting.', 16, 1);
    RETURN;
END;

-----------------------------------------------------------------------------
-- 2. PermissionId IDENTITY (do not force PermissionId)
-----------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns c
    WHERE c.object_id = OBJECT_ID(N'dbo.Permissions')
      AND c.name = N'PermissionId'
      AND c.is_identity = 1
)
BEGIN
    RAISERROR('Precondition failed: dbo.Permissions.PermissionId is not IDENTITY. Aborting (do not force PermissionId).', 16, 1);
    RETURN;
END;

-----------------------------------------------------------------------------
-- 3. PermissionKey uniqueness preflight
-----------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes i
    WHERE i.object_id = OBJECT_ID(N'dbo.Permissions')
      AND i.is_unique = 1
      AND i.is_disabled = 0
      AND i.is_hypothetical = 0
      AND i.has_filter = 0
      AND (
            SELECT COUNT(*)
            FROM sys.index_columns ic
            WHERE ic.object_id = i.object_id
              AND ic.index_id = i.index_id
              AND ic.is_included_column = 0
        ) = 1
      AND EXISTS (
            SELECT 1
            FROM sys.index_columns ic
            INNER JOIN sys.columns c
                ON c.object_id = ic.object_id
               AND c.column_id = ic.column_id
            WHERE ic.object_id = i.object_id
              AND ic.index_id = i.index_id
              AND ic.is_included_column = 0
              AND ic.key_ordinal = 1
              AND c.name = N'PermissionKey'
        )
)
BEGIN
    RAISERROR('Precondition failed: dbo.Permissions lacks an enabled, non-hypothetical, unfiltered unique single-column index on PermissionKey only. Aborting.', 16, 1);
    RETURN;
END;

IF EXISTS (
    SELECT PermissionKey
    FROM dbo.Permissions
    GROUP BY PermissionKey
    HAVING COUNT(*) > 1
)
BEGIN
    RAISERROR('Precondition failed: dbo.Permissions already contains duplicate PermissionKey rows. Aborting.', 16, 1);
    RETURN;
END;

-----------------------------------------------------------------------------
-- 4. Expected 28-key catalog (NEWLY DERIVED metadata)
-----------------------------------------------------------------------------
IF OBJECT_ID(N'tempdb..#Expected') IS NOT NULL
    DROP TABLE #Expected;

CREATE TABLE #Expected
(
    PermissionKey  nvarchar(200) COLLATE DATABASE_DEFAULT NOT NULL PRIMARY KEY,
    Description    nvarchar(500) COLLATE DATABASE_DEFAULT NOT NULL,
    ModuleName     nvarchar(100) COLLATE DATABASE_DEFAULT NOT NULL,
    SubModuleName  nvarchar(100) COLLATE DATABASE_DEFAULT NOT NULL,
    FeatureName    nvarchar(100) COLLATE DATABASE_DEFAULT NOT NULL
);

INSERT INTO #Expected (PermissionKey, Description, ModuleName, SubModuleName, FeatureName)
VALUES
    (N'Add_DirectInvoice', N'Creates a tax invoice with no source document (Manual_Invoice.aspx). Newly derived catalog metadata.', N'Tax Invoice', N'General', N'Create Direct Tax Invoice'),
    (N'CreateHydrentQuatation', N'Creates a hydrant quotation (CreateHydrentQuatation.aspx). Newly derived catalog metadata.', N'Hydrent', N'General', N'Create New Quotation'),
    (N'daily_reporting', N'Creates or submits a sales-visit daily report (daily_rpt.aspx). Newly derived catalog metadata.', N'Sales Team', N'General', N'Daily Reporting'),
    (N'Deletehydrent_invoice', N'Deletes a hydrant invoice (Deletehydrent_invoice.aspx). Newly derived catalog metadata.', N'Hydrent', N'General', N'Delete New Invoice'),
    (N'DeleteHydrentQuatation', N'Deletes a hydrant quotation (DeleteHydrentQuatation.aspx). Newly derived catalog metadata.', N'Hydrent', N'General', N'Delete New Quotation'),
    (N'hydrent_invoice', N'Creates a hydrant invoice from uninvoiced hydrant quotes (hydrent_invoice.aspx). Newly derived catalog metadata.', N'Hydrent', N'General', N'Create New Invoice'),
    (N'Li2', N'Creates a client purchase order via Create_quotation.aspx (Client PO leaf; distinct from Create_quotation). Newly derived catalog metadata.', N'Client PO', N'General', N'Create Purchase Order'),
    (N'Li3', N'Searches client purchase orders (Search_purchaseorder.aspx). Newly derived catalog metadata.', N'Client PO', N'General', N'Search Purchase Order'),
    (N'Li4', N'Deletes client purchase orders (delete_purchaseorder.aspx). Newly derived catalog metadata.', N'Client PO', N'General', N'Delete Purchase Order'),
    (N'Li5', N'Edits client purchase orders (edit_purchaseorder.aspx). Newly derived catalog metadata.', N'Client PO', N'General', N'Edit Purchase Order'),
    (N'Manual_proforma', N'Creates a proforma invoice without a quotation (Direct_Proforma.aspx). Newly derived catalog metadata.', N'Proforma Invoice', N'General', N'Manual Proforma Invoice'),
    (N'my_leaves', N'Employee leave application and balance view (MyLeaves.aspx). Newly derived catalog metadata.', N'Home', N'General', N'My Leaves'),
    (N'po_generate', N'Lists approved purchase requisitions eligible for vendor PO generation (Generate_PO_From_PR.aspx). Newly derived catalog metadata.', N'PR / PO', N'Purchase Order', N'Generate Purchase Order'),
    (N'po_view', N'Lists vendor purchase orders for review and release (View_PO.aspx). Distinct from View_po. Newly derived catalog metadata.', N'PR / PO', N'Purchase Order', N'Review & Release PO'),
    (N'pr_approve', N'Approves or rejects submitted purchase requisitions (Approve_PR.aspx). Newly derived catalog metadata.', N'PR / PO', N'Purchase Requisition', N'Approve / Reject PR'),
    (N'pr_create', N'Creates purchase requisitions (RequisitionNew.aspx). Newly derived catalog metadata.', N'PR / PO', N'Purchase Requisition', N'Create Purchase Requisition'),
    (N'pr_view', N'Views purchase requisitions (View_PR.aspx). Newly derived catalog metadata.', N'PR / PO', N'Purchase Requisition', N'View Purchase Requisition'),
    (N'rpts_vw_qtn_po_counts', N'Hosts the embedded Superset QTN/PO Added report iframe (rpts_vw_qtn_po_counts.aspx). Newly derived catalog metadata.', N'Reports', N'General', N'QTN/ PO Added'),
    (N'Search_Products', N'Read-only browser of the new product catalog used while purchasing (search_products.aspx). Newly derived catalog metadata.', N'Principle', N'General', N'Search Products'),
    (N'Searchhydrent_invoice', N'Searches hydrant invoices (Searchhydrent_invoice.aspx). Newly derived catalog metadata.', N'Hydrent', N'General', N'Search New Invoice'),
    (N'SearchHydrentQuatation', N'Searches hydrant quotations (SearchHydrentQuatation.aspx). Newly derived catalog metadata.', N'Hydrent', N'General', N'Search New Quotation'),
    (N'Service_stock', N'Lists service stock rows from tbl_stock (Service_stock.aspx). Newly derived catalog metadata.', N'Reports', N'General', N'Service Stock'),
    (N'srch_dailyrpts', N'Manager search and review of sales-visit daily reports (srch_dailyrpts.aspx). Newly derived catalog metadata.', N'Sales Team', N'General', N'Search Daily Reports'),
    (N'uploader', N'Imports products from XML into product and stock tables (ImportProducts.aspx). Newly derived catalog metadata.', N'Dashboard', N'General', N'Product Import (xml)'),
    (N'View_po', N'Views client purchase orders (View_PurchaseOrder.aspx). Distinct from po_view. Newly derived catalog metadata.', N'Client PO', N'General', N'View Purchase Order'),
    (N'Viewhydrent_invoice', N'Views hydrant invoices (Viewhydrent_invoice.aspx). Newly derived catalog metadata.', N'Hydrent', N'General', N'View New Invoice'),
    (N'ViewHydrentQuatation', N'Views hydrant quotations (ViewHydrentQuatation.aspx). Newly derived catalog metadata.', N'Hydrent', N'General', N'View New Quotation'),
    (N'vw_dailyrpts', N'Lists the current user sales-visit reports with responses and expenses (vw_dailyrpts.aspx). Newly derived catalog metadata.', N'Sales Team', N'General', N'View Daily Reporting');

IF (SELECT COUNT(*) FROM #Expected) <> 28
BEGIN
    RAISERROR('Internal error: #Expected must contain exactly 28 rows. Aborting.', 16, 1);
    RETURN;
END;

IF EXISTS (
    SELECT 1
    FROM #Expected
    WHERE PermissionKey IN (N'AdminAttendanceDashboard', N'AdminShiftAssignment', N'AdminOverride')
)
BEGIN
    RAISERROR('Internal error: #Expected must not include AdminAttendanceDashboard, AdminShiftAssignment, or AdminOverride. Aborting.', 16, 1);
    RETURN;
END;

-- Fail closed if any expected value exceeds dbo.Permissions column lengths (UAT_CATALOG).
IF EXISTS (
    SELECT 1
    FROM #Expected
    WHERE LEN(PermissionKey) > 200
       OR LEN(Description) > 500
       OR LEN(ModuleName) > 100
       OR LEN(SubModuleName) > 100
       OR LEN(FeatureName) > 100
)
BEGIN
    RAISERROR('Internal error: one or more #Expected values exceed dbo.Permissions column lengths (200/500/100/100/100). Aborting.', 16, 1);
    RETURN;
END;

-----------------------------------------------------------------------------
-- 5. Informational PRE snapshot (outside transaction; not authoritative)
-----------------------------------------------------------------------------
DECLARE @ExpectedCount int = 28;
DECLARE @ExistingMatching int;
DECLARE @ExistingConflict int;
DECLARE @MissingCount int;
DECLARE @InsertedCount int = 0;
DECLARE @InformationalMatching int;
DECLARE @InformationalConflict int;
DECLARE @InformationalMissing int;

;WITH InformalClassified AS (
    SELECT
        e.PermissionKey,
        CASE
            WHEN p.PermissionKey IS NULL THEN N'MISSING'
            WHEN (
                    (p.Description = e.Description OR (p.Description IS NULL AND e.Description IS NULL))
                AND (p.ModuleName = e.ModuleName OR (p.ModuleName IS NULL AND e.ModuleName IS NULL))
                AND (p.SubModuleName = e.SubModuleName OR (p.SubModuleName IS NULL AND e.SubModuleName IS NULL))
                AND (p.FeatureName = e.FeatureName OR (p.FeatureName IS NULL AND e.FeatureName IS NULL))
            ) THEN N'MATCH'
            ELSE N'CONFLICT'
        END AS StatusCode
    FROM #Expected e
    LEFT JOIN dbo.Permissions p
        ON p.PermissionKey = e.PermissionKey
)
SELECT
    @InformationalMatching = SUM(CASE WHEN StatusCode = N'MATCH' THEN 1 ELSE 0 END),
    @InformationalConflict = SUM(CASE WHEN StatusCode = N'CONFLICT' THEN 1 ELSE 0 END),
    @InformationalMissing = SUM(CASE WHEN StatusCode = N'MISSING' THEN 1 ELSE 0 END)
FROM InformalClassified;

SELECT
    N'PRE_INFORMATIONAL' AS Phase,
    @ExpectedCount AS ExpectedCount,
    ISNULL(@InformationalMatching, 0) AS ExistingMatchingCount,
    ISNULL(@InformationalConflict, 0) AS ExistingConflictCount,
    ISNULL(@InformationalMissing, 0) AS MissingCount,
    ISNULL(@InformationalMissing, 0) AS RemainingToInsertCount;

-----------------------------------------------------------------------------
-- 6. Authoritative classify + conflict fail-closed + insert (one transaction)
--    Fail closed if an ambient transaction is already open. Capture/restore
--    caller XACT_ABORT and isolation level after COMMIT and after ROLLBACK
--    (reTHROW after restore; cleanup must not mask the original error).
-----------------------------------------------------------------------------
IF @@TRANCOUNT <> 0
BEGIN
    RAISERROR('live_nav_28_catalog_seed.sql refuses to run inside an ambient transaction (@@TRANCOUNT <> 0). No session settings or DML applied.', 16, 1);
    RETURN;
END;

DECLARE @XactAbortWasOn bit =
    CASE WHEN (@@OPTIONS & 16384) = 16384 THEN 1 ELSE 0 END;

DECLARE @OriginalIsolationLevel sql_variant =
    (SELECT transaction_isolation_level FROM sys.dm_exec_sessions WHERE session_id = @@SPID);
DECLARE @RestoreIsolationSql nvarchar(200);

SET @RestoreIsolationSql =
    CASE CONVERT(int, @OriginalIsolationLevel)
        WHEN 1 THEN N'SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;'
        WHEN 2 THEN N'SET TRANSACTION ISOLATION LEVEL READ COMMITTED;'
        WHEN 3 THEN N'SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;'
        WHEN 4 THEN N'SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;'
        WHEN 5 THEN N'SET TRANSACTION ISOLATION LEVEL SNAPSHOT;'
        ELSE N'SET TRANSACTION ISOLATION LEVEL READ COMMITTED;' -- 0/unspecified or unexpected
    END;

BEGIN TRY
    SET XACT_ABORT ON;
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRANSACTION;

    -- Lock any already-present expected keys for the duration of this transaction.
    -- SERIALIZABLE also protects missing-key ranges via the PermissionKey unique index.
    IF OBJECT_ID(N'tempdb..#LockedExpected') IS NOT NULL
        DROP TABLE #LockedExpected;

    SELECT p.PermissionId
    INTO #LockedExpected
    FROM dbo.Permissions p WITH (UPDLOCK, HOLDLOCK)
    INNER JOIN #Expected e
        ON e.PermissionKey = p.PermissionKey;

    ;WITH Classified AS (
        SELECT
            e.PermissionKey,
            CASE
                WHEN p.PermissionKey IS NULL THEN N'MISSING'
                WHEN (
                        (p.Description = e.Description OR (p.Description IS NULL AND e.Description IS NULL))
                    AND (p.ModuleName = e.ModuleName OR (p.ModuleName IS NULL AND e.ModuleName IS NULL))
                    AND (p.SubModuleName = e.SubModuleName OR (p.SubModuleName IS NULL AND e.SubModuleName IS NULL))
                    AND (p.FeatureName = e.FeatureName OR (p.FeatureName IS NULL AND e.FeatureName IS NULL))
                ) THEN N'MATCH'
                ELSE N'CONFLICT'
            END AS StatusCode
        FROM #Expected e
        LEFT JOIN dbo.Permissions p
            ON p.PermissionKey = e.PermissionKey
    )
    SELECT
        @ExistingMatching = SUM(CASE WHEN StatusCode = N'MATCH' THEN 1 ELSE 0 END),
        @ExistingConflict = SUM(CASE WHEN StatusCode = N'CONFLICT' THEN 1 ELSE 0 END),
        @MissingCount = SUM(CASE WHEN StatusCode = N'MISSING' THEN 1 ELSE 0 END)
    FROM Classified;

    SELECT
        N'PRE_TXN' AS Phase,
        @ExpectedCount AS ExpectedCount,
        ISNULL(@ExistingMatching, 0) AS ExistingMatchingCount,
        ISNULL(@ExistingConflict, 0) AS ExistingConflictCount,
        ISNULL(@MissingCount, 0) AS MissingCount,
        ISNULL(@MissingCount, 0) AS RemainingToInsertCount;

    SELECT
        N'PRE_CONFLICT_DETAIL' AS ResultSet,
        e.PermissionKey,
        p.Description AS ExistingDescription,
        e.Description AS ExpectedDescription,
        p.ModuleName AS ExistingModuleName,
        e.ModuleName AS ExpectedModuleName,
        p.SubModuleName AS ExistingSubModuleName,
        e.SubModuleName AS ExpectedSubModuleName,
        p.FeatureName AS ExistingFeatureName,
        e.FeatureName AS ExpectedFeatureName
    FROM #Expected e
    INNER JOIN dbo.Permissions p
        ON p.PermissionKey = e.PermissionKey
    WHERE NOT (
            (p.Description = e.Description OR (p.Description IS NULL AND e.Description IS NULL))
        AND (p.ModuleName = e.ModuleName OR (p.ModuleName IS NULL AND e.ModuleName IS NULL))
        AND (p.SubModuleName = e.SubModuleName OR (p.SubModuleName IS NULL AND e.SubModuleName IS NULL))
        AND (p.FeatureName = e.FeatureName OR (p.FeatureName IS NULL AND e.FeatureName IS NULL))
    );

    IF ISNULL(@ExistingConflict, 0) > 0
    BEGIN
        THROW 50040, 'Existing PermissionKey metadata conflict versus newly derived expected tuples. No UPDATE performed. See PRE_CONFLICT_DETAIL. Aborting.', 1;
    END;

    INSERT INTO dbo.Permissions
        (PermissionKey, Description, ModuleName, SubModuleName, FeatureName)
    SELECT
        e.PermissionKey,
        e.Description,
        e.ModuleName,
        e.SubModuleName,
        e.FeatureName
    FROM #Expected e
    WHERE NOT EXISTS (
        SELECT 1
        FROM dbo.Permissions p
        WHERE p.PermissionKey = e.PermissionKey
    );

    SET @InsertedCount = @@ROWCOUNT;

    IF @InsertedCount <> ISNULL(@MissingCount, 0)
    BEGIN
        THROW 50041, 'Insert rowcount mismatch versus in-transaction MissingCount. Rolling back.', 1;
    END;

    -- Postcondition: all 28 exist with exact expected metadata.
    IF EXISTS (
        SELECT 1
        FROM #Expected e
        LEFT JOIN dbo.Permissions p
            ON p.PermissionKey = e.PermissionKey
        WHERE p.PermissionKey IS NULL
           OR NOT (
                    (p.Description = e.Description OR (p.Description IS NULL AND e.Description IS NULL))
                AND (p.ModuleName = e.ModuleName OR (p.ModuleName IS NULL AND e.ModuleName IS NULL))
                AND (p.SubModuleName = e.SubModuleName OR (p.SubModuleName IS NULL AND e.SubModuleName IS NULL))
                AND (p.FeatureName = e.FeatureName OR (p.FeatureName IS NULL AND e.FeatureName IS NULL))
           )
    )
    BEGIN
        THROW 50042, 'Postcondition failed: one or more of the 28 expected PermissionKeys are missing or metadata-mismatched after catalog seed.', 1;
    END;

    SELECT
        N'POST' AS Phase,
        @ExpectedCount AS ExpectedCount,
        ISNULL(@ExistingMatching, 0) AS ExistingMatchingBefore,
        @InsertedCount AS InsertedCount,
        0 AS RemainingMissingCount,
        (SELECT COUNT(*) FROM #Expected e INNER JOIN dbo.Permissions p ON p.PermissionKey = e.PermissionKey) AS PresentAfterCount;

    SELECT
        N'POST_INSERTED_OR_MATCHING' AS ResultSet,
        e.PermissionKey,
        e.ModuleName,
        e.SubModuleName,
        e.FeatureName
    FROM #Expected e
    ORDER BY e.PermissionKey;

    COMMIT TRANSACTION;

    -- Restore caller's isolation level and XACT_ABORT after successful commit.
    BEGIN TRY
        EXEC sys.sp_executesql @RestoreIsolationSql;
    END TRY
    BEGIN CATCH
        -- Isolation restore failure must not replace success/original error path.
    END CATCH;

    BEGIN TRY
        IF @XactAbortWasOn = 1
            SET XACT_ABORT ON;
        ELSE
            SET XACT_ABORT OFF;
    END TRY
    BEGIN CATCH
        -- XACT_ABORT restore failure must not replace success/original error path.
    END CATCH;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    -- Restore session settings before re-THROW so the original error is not masked.
    BEGIN TRY
        EXEC sys.sp_executesql @RestoreIsolationSql;
    END TRY
    BEGIN CATCH
        -- Isolation restore failure must not replace the original error.
    END CATCH;

    BEGIN TRY
        IF @XactAbortWasOn = 1
            SET XACT_ABORT ON;
        ELSE
            SET XACT_ABORT OFF;
    END TRY
    BEGIN CATCH
        -- XACT_ABORT restore failure must not replace the original error.
    END CATCH;

    THROW;
END CATCH;

-- No INSERT into dbo.RolePermissions.
-- No INSERT into dbo.UserRoles.
-- No DELETE / UPDATE / DDL.
-- AdminAttendanceDashboard / AdminShiftAssignment / AdminOverride intentionally excluded.
-- PermissionId is IDENTITY and is not supplied.
