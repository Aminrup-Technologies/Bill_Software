-- =============================================================================
-- When: After db/pservice_snapshot.sql and the invoice snapshot INSERT deploy.
-- Why:  Existing invoices have NULL PServiceName. Export will read the snapshot
--       column, so historical Auto Quotation / Auto PO rows need one fill.
-- What: UPDATE only NULL tbl_Invoice.PServiceName from CompanyID-scoped
--       tbl_QutPrimaryService (same STUFF aggregate the old export used).
--       Skip keys that do not uniquely map inside the invoice company.
-- =============================================================================
-- DO NOT persist until the BEFORE result is reviewed.
-- Default: ROLLBACK. Uncomment COMMIT after AFTER verification.

SET NOCOUNT ON;

DECLARE @NullRowsBefore int,
        @EligibleRows int,
        @UnmatchedRows int,
        @RowsUpdated int,
        @RemainingNullRows int;

-- ---------------------------------------------------------------------------
-- BEFORE — preview PlannedAction
-- ---------------------------------------------------------------------------
SELECT
    i.Invoice_No,
    i.Invoice_Date,
    i.Quotation_No,
    i.CompanyID,
    i.PServiceName AS InvoicePServiceName,
    src.PrimaryService AS LookupPrimaryService,
    CASE
        WHEN src.qut_no IS NULL THEN 'SKIP_NO_COMPANY_MATCH'
        ELSE 'WILL_UPDATE'
    END AS PlannedAction
FROM tbl_Invoice AS i
LEFT JOIN (
    SELECT
        p1.qut_no,
        p1.CompanyID,
        STUFF((
            SELECT ', ' + p2.PrimaryService
            FROM tbl_QutPrimaryService AS p2
            WHERE p2.qut_no = p1.qut_no
              AND p2.CompanyID = p1.CompanyID
            FOR XML PATH(''), TYPE
        ).value('.', 'nvarchar(max)'), 1, 2, '') AS PrimaryService
    FROM tbl_QutPrimaryService AS p1
    WHERE p1.CompanyID IS NOT NULL
    GROUP BY p1.qut_no, p1.CompanyID
) AS src
    ON src.qut_no = i.Quotation_No
   AND src.CompanyID = i.CompanyID
WHERE i.PServiceName IS NULL
ORDER BY PlannedAction, i.Invoice_No;

SELECT
    @NullRowsBefore = SUM(CASE WHEN i.PServiceName IS NULL THEN 1 ELSE 0 END),
    @EligibleRows = SUM(CASE
        WHEN i.PServiceName IS NULL
         AND src.qut_no IS NOT NULL
         AND NULLIF(LTRIM(RTRIM(src.PrimaryService)), '') IS NOT NULL
        THEN 1 ELSE 0
    END),
    @UnmatchedRows = SUM(CASE
        WHEN i.PServiceName IS NULL
         AND (
                src.qut_no IS NULL
             OR NULLIF(LTRIM(RTRIM(src.PrimaryService)), '') IS NULL
         )
        THEN 1 ELSE 0
    END)
FROM tbl_Invoice AS i
LEFT JOIN (
    SELECT
        p1.qut_no,
        p1.CompanyID,
        STUFF((
            SELECT ', ' + p2.PrimaryService
            FROM tbl_QutPrimaryService AS p2
            WHERE p2.qut_no = p1.qut_no
              AND p2.CompanyID = p1.CompanyID
            FOR XML PATH(''), TYPE
        ).value('.', 'nvarchar(max)'), 1, 2, '') AS PrimaryService
    FROM tbl_QutPrimaryService AS p1
    WHERE p1.CompanyID IS NOT NULL
    GROUP BY p1.qut_no, p1.CompanyID
) AS src
    ON src.qut_no = i.Quotation_No
   AND src.CompanyID = i.CompanyID;

SELECT
    ISNULL(@NullRowsBefore, 0) AS NullRowsBefore,
    ISNULL(@EligibleRows, 0) AS EligibleRows,
    ISNULL(@UnmatchedRows, 0) AS UnmatchedRows;

-- ---------------------------------------------------------------------------
-- UPDATE — explicit transaction; ROLLBACK is the default
-- ---------------------------------------------------------------------------
BEGIN TRAN;

UPDATE i
SET i.PServiceName = src.PrimaryService
FROM tbl_Invoice AS i
INNER JOIN (
    SELECT
        p1.qut_no,
        p1.CompanyID,
        STUFF((
            SELECT ', ' + p2.PrimaryService
            FROM tbl_QutPrimaryService AS p2
            WHERE p2.qut_no = p1.qut_no
              AND p2.CompanyID = p1.CompanyID
            FOR XML PATH(''), TYPE
        ).value('.', 'nvarchar(max)'), 1, 2, '') AS PrimaryService
    FROM tbl_QutPrimaryService AS p1
    WHERE p1.CompanyID IS NOT NULL
    GROUP BY p1.qut_no, p1.CompanyID
) AS src
    ON src.qut_no = i.Quotation_No
   AND src.CompanyID = i.CompanyID
WHERE i.PServiceName IS NULL
  AND NULLIF(LTRIM(RTRIM(src.PrimaryService)), '') IS NOT NULL;

SET @RowsUpdated = @@ROWCOUNT;

-- ---------------------------------------------------------------------------
-- AFTER — remaining NULL rows should be unmatched source keys only
-- ---------------------------------------------------------------------------
SELECT
    i.Invoice_No,
    i.Invoice_Date,
    i.Quotation_No,
    i.CompanyID,
    i.PServiceName,
    CASE
        WHEN src.qut_no IS NULL THEN 'No quotation service match'
        WHEN NULLIF(LTRIM(RTRIM(src.PrimaryService)), '') IS NULL THEN 'Empty quotation service'
        ELSE 'Unexpected remaining NULL'
    END AS RemainingReason
FROM tbl_Invoice AS i
LEFT JOIN (
    SELECT
        p1.qut_no,
        p1.CompanyID,
        STUFF((
            SELECT ', ' + p2.PrimaryService
            FROM tbl_QutPrimaryService AS p2
            WHERE p2.qut_no = p1.qut_no
              AND p2.CompanyID = p1.CompanyID
            FOR XML PATH(''), TYPE
        ).value('.', 'nvarchar(max)'), 1, 2, '') AS PrimaryService
    FROM tbl_QutPrimaryService AS p1
    WHERE p1.CompanyID IS NOT NULL
    GROUP BY p1.qut_no, p1.CompanyID
) AS src
    ON src.qut_no = i.Quotation_No
   AND src.CompanyID = i.CompanyID
WHERE i.PServiceName IS NULL
ORDER BY RemainingReason, i.Invoice_No;

SELECT @RemainingNullRows = COUNT(*)
FROM tbl_Invoice
WHERE PServiceName IS NULL;

SELECT
    ISNULL(@NullRowsBefore, 0) AS NullRowsBefore,
    ISNULL(@EligibleRows, 0) AS EligibleRows,
    ISNULL(@UnmatchedRows, 0) AS UnmatchedRows,
    ISNULL(@RowsUpdated, 0) AS RowsUpdated,
    ISNULL(@RemainingNullRows, 0) AS RemainingNullRows;

-- COMMIT TRAN;   -- uncomment to persist after AFTER verification is accepted
ROLLBACK TRAN;    -- default: discard the UPDATE after review
