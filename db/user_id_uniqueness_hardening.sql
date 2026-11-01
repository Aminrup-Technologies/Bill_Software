/* ============================================================================
   NAME:        user_id_uniqueness_hardening
   WHEN:        2026-11-01
   WHY:         Login authenticates with SELECT TOP 1 ... WHERE User_Id=@UserId
                (index.aspx.cs) and identity self-service is keyed on User_Id;
                the model requires User_Id to be unique but no constraint is
                declared (docs/49_Identity_Model_Feasibility.md item B1;
                docs/48_Identity_Hardening_PR_Preparation.md section 2).
   WHAT:        PASS 1 is a read-only duplicate audit and does not create an
                index. PASS 2 runs only when the DBA sets @ExecutePass2 = 1,
                then creates filtered UNIQUE index UQ_tbl_login_User_Id on
                dbo.tbl_login(User_Id) WHERE User_Id IS NOT NULL, or recognizes
                that exact enabled index. No data is ever altered.
   DEPLOY:      DBA execution only, after go-live. Never from the application.
                Run PASS 1 first and leave @ExecutePass2 = 0. Only after
                reviewing that audit and confirming zero rows, set
                @ExecutePass2 = 1 and re-run the whole script (PASS 1 repeats,
                then PASS 2 runs). PASS 2 re-verifies inside its transaction
                and fails closed if duplicates exist or the existing index
                definition differs. Then run the read-only verification
                queries at the end of this script.
   ============================================================================ */

SET NOCOUNT ON;
SET XACT_ABORT ON;

-- DBA execution gate. Leave 0 to run PASS 1 only.
-- Set to 1 only after the DBA has reviewed the PASS 1 audit and verified that
-- it returned zero rows.
DECLARE @ExecutePass2 bit = 0;

IF OBJECT_ID(N'dbo.tbl_login', N'U') IS NULL
BEGIN
    RAISERROR('dbo.tbl_login does not exist on this database. Nothing done.', 16, 1);
    RETURN;
END;

-- ----------------------------------------------------------------------------
-- PASS 1 (read-only): duplicate audit. Expect zero rows before PASS 2.
-- A duplicate blocks the constraint (PASS 2) only.
-- Remediation of any duplicate is a data-owner decision; this script never
-- deduplicates and never mutates user data. This batch never creates an index.
-- ----------------------------------------------------------------------------
SELECT l.User_Id, COUNT(*) AS DuplicateCount, MIN(l.Id) AS KeepCandidateId,
       STUFF((SELECT CAST(', ' AS varchar(4)) + CAST(d.Id AS varchar(12))
              FROM dbo.tbl_login d
              WHERE d.User_Id = l.User_Id
              ORDER BY d.Id
              FOR XML PATH('')), 1, 2, '') AS AllIds
FROM dbo.tbl_login l
WHERE l.User_Id IS NOT NULL
GROUP BY l.User_Id
HAVING COUNT(*) > 1
ORDER BY l.User_Id;

IF @ExecutePass2 = 0
BEGIN
    PRINT 'PASS 1 complete. PASS 2 was not executed. Review the audit; only if it returned zero rows, set @ExecutePass2 = 1 and re-run the whole script.';
    RETURN;
END;

-- ----------------------------------------------------------------------------
-- PASS 2: explicit DBA opt-in. Filtered to exclude NULLs because SQL Server
-- unique indexes permit only one NULL row and tbl_login.User_Id is nullable
-- (legacy paths may leave it NULL); excluding NULLs preserves legacy behavior.
-- ----------------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @indexId int;
    DECLARE @definitionOk bit;

    SELECT @indexId = i.index_id
    FROM sys.indexes i
    WHERE i.name = N'UQ_tbl_login_User_Id'
      AND i.object_id = OBJECT_ID(N'dbo.tbl_login');

    SET @definitionOk = CASE
        WHEN @indexId IS NOT NULL
         AND EXISTS (
            SELECT 1
            FROM sys.indexes i
            WHERE i.object_id = OBJECT_ID(N'dbo.tbl_login')
              AND i.index_id = @indexId
              AND i.is_unique = 1
              AND i.has_filter = 1
              AND i.is_disabled = 0
              AND i.is_primary_key = 0
              AND i.is_unique_constraint = 0
              AND i.is_hypothetical = 0
              AND i.type = 2
              AND UPPER(REPLACE(REPLACE(REPLACE(ISNULL(i.filter_definition, N''), N' ', N''), N'[', N''), N']', N''))
                  IN (N'(USER_IDISNOTNULL)', N'USER_IDISNOTNULL'))
         AND (
            SELECT COUNT(*)
            FROM sys.index_columns ic
            WHERE ic.object_id = OBJECT_ID(N'dbo.tbl_login')
              AND ic.index_id = @indexId) = 1
         AND EXISTS (
            SELECT 1
            FROM sys.index_columns ic
            INNER JOIN sys.columns c
                ON c.object_id = ic.object_id
               AND c.column_id = ic.column_id
            WHERE ic.object_id = OBJECT_ID(N'dbo.tbl_login')
              AND ic.index_id = @indexId
              AND ic.key_ordinal = 1
              AND ic.is_included_column = 0
              AND ic.is_descending_key = 0
              AND c.name = N'User_Id')
        THEN 1
        ELSE 0
    END;

    IF @indexId IS NOT NULL AND @definitionOk = 0
    BEGIN
        RAISERROR('UQ_tbl_login_User_Id exists on dbo.tbl_login with an unexpected definition. Expected an enabled nonclustered unique index on User_Id only, filtered by User_Id IS NOT NULL. No change made.', 16, 1);
    END;

    IF @indexId IS NULL
    BEGIN
        IF EXISTS (
            SELECT 1 FROM dbo.tbl_login
            WHERE User_Id IS NOT NULL
            GROUP BY User_Id
            HAVING COUNT(*) > 1)
        BEGIN
            RAISERROR('Duplicates detected in tbl_login.User_Id. Run PASS 1 and remediate with data-owner sign-off before creating UQ_tbl_login_User_Id. No constraint created.', 16, 1);
        END
        ELSE
        BEGIN
            CREATE UNIQUE INDEX UQ_tbl_login_User_Id
                ON dbo.tbl_login (User_Id)
                WHERE User_Id IS NOT NULL;
        END;
    END;

    COMMIT TRANSACTION;
    PRINT 'UQ_tbl_login_User_Id present (created if missing).';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @msg nvarchar(2048) = ERROR_MESSAGE();
    RAISERROR('%s', 16, 1, @msg);
END CATCH;
GO

-- Verification (read-only), expected after PASS 2:
--   SELECT i.name, i.type_desc, i.is_unique, i.has_filter, i.is_disabled,
--          i.is_hypothetical, i.is_primary_key, i.is_unique_constraint,
--          i.filter_definition
--   FROM sys.indexes i
--   WHERE i.object_id = OBJECT_ID(N'dbo.tbl_login')
--     AND i.name = N'UQ_tbl_login_User_Id';
--   Expected: exactly one row; type_desc = NONCLUSTERED, is_unique = 1,
--             has_filter = 1, is_disabled = 0, is_hypothetical = 0,
--             is_primary_key = 0, is_unique_constraint = 0,
--             filter_definition = ([User_Id] IS NOT NULL).
--
--   SELECT c.name, ic.key_ordinal, ic.is_descending_key, ic.is_included_column
--   FROM sys.indexes i
--   INNER JOIN sys.index_columns ic
--       ON ic.object_id = i.object_id AND ic.index_id = i.index_id
--   INNER JOIN sys.columns c
--       ON c.object_id = ic.object_id AND c.column_id = ic.column_id
--   WHERE i.object_id = OBJECT_ID(N'dbo.tbl_login')
--     AND i.name = N'UQ_tbl_login_User_Id';
--   Expected: exactly one row; User_Id, key_ordinal = 1,
--             is_descending_key = 0, is_included_column = 0.
