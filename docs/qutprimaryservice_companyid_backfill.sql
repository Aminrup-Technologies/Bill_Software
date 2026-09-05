-- =============================================================================
-- Backfill: tbl_QutPrimaryService.CompanyID from tbl_Quotation.CompanyID
-- =============================================================================
-- When: After deploying the legacy Edit_quatation CompanyID writer (PR #44).
--       Review the BEFORE preview first. The UPDATE batch defaults to ROLLBACK.
-- Why:  Legacy Edit_quatation INSERTs omitted CompanyID. Invoice Excel export
--       already filters tbl_QutPrimaryService by CompanyID, so those rows
--       appear as blank Primary Service.
-- What: UPDATE only NULL CompanyID on tbl_QutPrimaryService from the unique
--       tbl_Quotation.CompanyID for the same qut_no. Skip ambiguous and
--       unmatched keys. No application or export SQL changes.
--
-- DO NOT execute the UPDATE until the BEFORE verification result is reviewed.
-- Scope:
--   UPDATE only rows where tbl_QutPrimaryService.CompanyID IS NULL.
--   Match on qut_no = tbl_Quotation.Quotation_No.
--   Skip qut_no values that map to more than one distinct quotation CompanyID
--   (no cross-company updates).
-- =============================================================================

SET NOCOUNT ON;

DECLARE @NullRowsBefore int,
        @EligibleRows int,
        @AmbiguousRows int,
        @UnmatchedRows int,
        @RowsUpdated int,
        @RemainingNullRows int;

-- ---------------------------------------------------------------------------
-- BEFORE — review PlannedAction before running the UPDATE
-- ---------------------------------------------------------------------------
SELECT
    p.qut_no,
    p.PrimaryService,
    p.CompanyID AS ServiceCompanyID,
    src.CompanyID AS QuotationCompanyID,
    CASE
        WHEN src.Quotation_No IS NULL THEN 'SKIP_NO_QUOTATION_MATCH'
        WHEN amb.Quotation_No IS NOT NULL THEN 'SKIP_AMBIGUOUS_QUT_NO'
        ELSE 'WILL_UPDATE'
    END AS PlannedAction
FROM tbl_QutPrimaryService AS p
LEFT JOIN (
    SELECT Quotation_No, MIN(CompanyID) AS CompanyID
    FROM tbl_Quotation
    WHERE CompanyID IS NOT NULL
    GROUP BY Quotation_No
    HAVING COUNT(DISTINCT CompanyID) = 1
) AS src
    ON src.Quotation_No = p.qut_no
LEFT JOIN (
    SELECT Quotation_No
    FROM tbl_Quotation
    WHERE CompanyID IS NOT NULL
    GROUP BY Quotation_No
    HAVING COUNT(DISTINCT CompanyID) > 1
) AS amb
    ON amb.Quotation_No = p.qut_no
WHERE p.CompanyID IS NULL
ORDER BY PlannedAction, p.qut_no;

-- Summary counts (before)
SELECT
    @NullRowsBefore = SUM(CASE WHEN p.CompanyID IS NULL THEN 1 ELSE 0 END),
    @EligibleRows = SUM(CASE
        WHEN p.CompanyID IS NULL
         AND src.Quotation_No IS NOT NULL
        THEN 1 ELSE 0
    END),
    @AmbiguousRows = SUM(CASE
        WHEN p.CompanyID IS NULL
         AND amb.Quotation_No IS NOT NULL
        THEN 1 ELSE 0
    END),
    @UnmatchedRows = SUM(CASE
        WHEN p.CompanyID IS NULL
         AND src.Quotation_No IS NULL
         AND amb.Quotation_No IS NULL
        THEN 1 ELSE 0
    END)
FROM tbl_QutPrimaryService AS p
LEFT JOIN (
    SELECT Quotation_No
    FROM tbl_Quotation
    WHERE CompanyID IS NOT NULL
    GROUP BY Quotation_No
    HAVING COUNT(DISTINCT CompanyID) = 1
) AS src
    ON src.Quotation_No = p.qut_no
LEFT JOIN (
    SELECT Quotation_No
    FROM tbl_Quotation
    WHERE CompanyID IS NOT NULL
    GROUP BY Quotation_No
    HAVING COUNT(DISTINCT CompanyID) > 1
) AS amb
    ON amb.Quotation_No = p.qut_no;

SELECT
    ISNULL(@NullRowsBefore, 0) AS NullRowsBefore,
    ISNULL(@EligibleRows, 0) AS EligibleRows,
    ISNULL(@AmbiguousRows, 0) AS AmbiguousRows,
    ISNULL(@UnmatchedRows, 0) AS UnmatchedRows;

-- ---------------------------------------------------------------------------
-- UPDATE — explicit transaction; ROLLBACK is the default
-- Review AFTER verification, then uncomment COMMIT and comment ROLLBACK.
-- ---------------------------------------------------------------------------
BEGIN TRAN;

UPDATE p
SET p.CompanyID = src.CompanyID
FROM tbl_QutPrimaryService AS p
INNER JOIN (
    SELECT Quotation_No, MIN(CompanyID) AS CompanyID
    FROM tbl_Quotation
    WHERE CompanyID IS NOT NULL
    GROUP BY Quotation_No
    HAVING COUNT(DISTINCT CompanyID) = 1
) AS src
    ON src.Quotation_No = p.qut_no
WHERE p.CompanyID IS NULL;

SET @RowsUpdated = @@ROWCOUNT;

-- ---------------------------------------------------------------------------
-- AFTER — classify remaining NULL rows
-- ---------------------------------------------------------------------------
SELECT
    p.qut_no,
    p.PrimaryService,
    p.CompanyID AS ServiceCompanyID,
    src.CompanyID AS QuotationCompanyID,
    CASE
        WHEN amb.Quotation_No IS NOT NULL THEN 'Ambiguous quotation mapping'
        WHEN src.Quotation_No IS NULL THEN 'No quotation match'
        ELSE 'Unexpected remaining NULL'
    END AS RemainingReason
FROM tbl_QutPrimaryService AS p
LEFT JOIN (
    SELECT Quotation_No, MIN(CompanyID) AS CompanyID
    FROM tbl_Quotation
    WHERE CompanyID IS NOT NULL
    GROUP BY Quotation_No
    HAVING COUNT(DISTINCT CompanyID) = 1
) AS src
    ON src.Quotation_No = p.qut_no
LEFT JOIN (
    SELECT Quotation_No
    FROM tbl_Quotation
    WHERE CompanyID IS NOT NULL
    GROUP BY Quotation_No
    HAVING COUNT(DISTINCT CompanyID) > 1
) AS amb
    ON amb.Quotation_No = p.qut_no
WHERE p.CompanyID IS NULL
ORDER BY RemainingReason, p.qut_no;

SELECT @RemainingNullRows = COUNT(*)
FROM tbl_QutPrimaryService
WHERE CompanyID IS NULL;

SELECT
    ISNULL(@NullRowsBefore, 0) AS NullRowsBefore,
    ISNULL(@EligibleRows, 0) AS EligibleRows,
    ISNULL(@AmbiguousRows, 0) AS AmbiguousRows,
    ISNULL(@UnmatchedRows, 0) AS UnmatchedRows,
    ISNULL(@RowsUpdated, 0) AS RowsUpdated,
    ISNULL(@RemainingNullRows, 0) AS RemainingNullRows;

-- COMMIT TRAN;   -- uncomment to persist after AFTER verification is accepted
ROLLBACK TRAN;    -- default: discard the UPDATE after review
