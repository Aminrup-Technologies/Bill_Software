-- =============================================================================
-- Backfill: tbl_QutPrimaryService.CompanyID from tbl_Quotation.CompanyID
-- =============================================================================
-- DO NOT execute until the BEFORE verification result is reviewed.
--
-- Scope:
--   UPDATE only rows where tbl_QutPrimaryService.CompanyID IS NULL.
--   Match on qut_no = tbl_Quotation.Quotation_No.
--   Skip qut_no values that map to more than one distinct quotation CompanyID
--   (no cross-company updates).
-- =============================================================================

SET NOCOUNT ON;

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

-- Counts before
SELECT
    SUM(CASE WHEN p.CompanyID IS NULL THEN 1 ELSE 0 END) AS NullCompanyIDRows,
    SUM(CASE
            WHEN p.CompanyID IS NULL
             AND src.Quotation_No IS NOT NULL
            THEN 1 ELSE 0
        END) AS EligibleToUpdate,
    SUM(CASE
            WHEN p.CompanyID IS NULL
             AND src.Quotation_No IS NULL
            THEN 1 ELSE 0
        END) AS UnmatchedOrAmbiguous
FROM tbl_QutPrimaryService AS p
LEFT JOIN (
    SELECT Quotation_No
    FROM tbl_Quotation
    WHERE CompanyID IS NOT NULL
    GROUP BY Quotation_No
    HAVING COUNT(DISTINCT CompanyID) = 1
) AS src
    ON src.Quotation_No = p.qut_no;

-- ---------------------------------------------------------------------------
-- UPDATE — NULL CompanyID only; unique quotation CompanyID only
-- ---------------------------------------------------------------------------
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

-- ---------------------------------------------------------------------------
-- AFTER — remaining NULL rows should be unmatched or ambiguous only
-- ---------------------------------------------------------------------------
SELECT
    p.qut_no,
    p.PrimaryService,
    p.CompanyID AS ServiceCompanyID,
    q.CompanyID AS QuotationCompanyID
FROM tbl_QutPrimaryService AS p
LEFT JOIN tbl_Quotation AS q
    ON q.Quotation_No = p.qut_no
WHERE p.CompanyID IS NULL
ORDER BY p.qut_no;

SELECT
    SUM(CASE WHEN CompanyID IS NULL THEN 1 ELSE 0 END) AS RemainingNullCompanyIDRows,
    SUM(CASE WHEN CompanyID IS NOT NULL THEN 1 ELSE 0 END) AS RowsWithCompanyID
FROM tbl_QutPrimaryService;
