/* ============================================================================
   NAME:        user_id_uniqueness_hardening
   WHEN:        2026-09-30
   WHY:         Login authenticates with SELECT TOP 1 ... WHERE User_Id=@UserId
                (index.aspx.cs) and identity self-service is keyed on User_Id;
                the model requires User_Id to be unique but no constraint is
                declared (docs/49 item B1; docs/48 section 3, item B1).
   WHAT:        Duplicate audit (PASS 1, read-only) and conditional creation of
                the filtered UNIQUE index dbo.tbl_login(User_Id) WHERE
                User_Id IS NOT NULL (PASS 2). No data is ever altered.
   DEPLOY:      DBA execution only, after go-live. Never from the application.
                Run PASS 1 first. Only when PASS 1 returns zero rows, run
                PASS 2. PASS 2 re-verifies inside its transaction and fails
                closed if duplicates exist.
   ============================================================================ */

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.tbl_login', N'U') IS NULL
BEGIN
    RAISERROR('dbo.tbl_login does not exist on this database. Nothing done.', 16, 1);
    RETURN;
END;

-- ----------------------------------------------------------------------------
-- PASS 1 (read-only): duplicate audit. Expect zero rows before PASS 2.
-- A duplicate blocks the constraint; it does NOT block PR #94 deployment.
-- Remediation of any duplicate is a data-owner decision; this script never
-- deduplicates and never mutates user data.
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

-- ----------------------------------------------------------------------------
-- PASS 2: constraint creation. Filtered to exclude NULLs because SQL Server
-- unique indexes permit only one NULL row and tbl_login.User_Id is nullable
-- (legacy paths may leave it NULL); excluding NULLs preserves legacy behavior.
-- ----------------------------------------------------------------------------
BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'UQ_tbl_login_User_Id'
          AND object_id = OBJECT_ID(N'dbo.tbl_login'))
    BEGIN
        IF EXISTS (
            SELECT 1 FROM dbo.tbl_login
            WHERE User_Id IS NOT NULL
            GROUP BY User_Id
            HAVING COUNT(*) > 1)
        BEGIN
            RAISERROR('Duplicates detected in tbl_login.User_Id. Run PASS 1 and remediate with data-owner sign-off before creating UQ_tbl_login_User_Id. No constraint created.', 16, 1);
        END;

        CREATE UNIQUE INDEX UQ_tbl_login_User_Id
            ON dbo.tbl_login (User_Id)
            WHERE User_Id IS NOT NULL;
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
--   SELECT name, is_unique, has_filter FROM sys.indexes
--   WHERE object_id = OBJECT_ID(N'dbo.tbl_login');
--   Expected: UQ_tbl_login_User_Id | is_unique = 1 | has_filter = 1
