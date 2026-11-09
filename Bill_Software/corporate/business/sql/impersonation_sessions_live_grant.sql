/* ============================================================================
   NAME:        impersonation_sessions_live_grant
   WHEN:        2026-11-09
   WHY:         flamex_live needs a mapped flamexliveapp principal with only the ImpersonationSessions permissions the runtime executes.
   WHAT:        Map login flamexliveapp when absent and grant column SELECT, column UPDATE, and table INSERT. No DELETE.
   ============================================================================ */
-- DBA review only. Run db/impersonation_pr1.sql on flamex_live first. Do not run from the application.
-- Unresolved prerequisites: confirm SUSER_ID(N'flamexliveapp') in a session that can resolve server logins.
-- This script does not create dbo.ImpersonationSessions and does not repair missing indexes.

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'flamex_live'
BEGIN
    RAISERROR('Database guard failed: DB_NAME() must be flamex_live. No grant applied.', 16, 1);
    RETURN;
END;

IF OBJECT_ID(N'dbo.ImpersonationSessions', N'U') IS NULL
BEGIN
    RAISERROR('dbo.ImpersonationSessions is missing. Run db/impersonation_pr1.sql first. No grant applied.', 16, 1);
    RETURN;
END;

IF SUSER_ID(N'flamexliveapp') IS NULL
BEGIN
    RAISERROR('Login flamexliveapp was not found. No user and no grant applied.', 16, 1);
    RETURN;
END;

DECLARE @LoginSid varbinary(85) = SUSER_SID(N'flamexliveapp');

IF @LoginSid IS NULL
BEGIN
    RAISERROR('Login flamexliveapp did not resolve to a SID. No user and no grant applied.', 16, 1);
    RETURN;
END;

IF EXISTS (
    SELECT 1
    FROM sys.database_principals dp
    WHERE dp.sid = @LoginSid
      AND dp.name <> N'flamexliveapp'
)
BEGIN
    RAISERROR('Login flamexliveapp is already mapped to a different database principal. No grant applied.', 16, 1);
    RETURN;
END;

IF USER_ID(N'flamexliveapp') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        WHERE USER_SID(N'flamexliveapp') = @LoginSid
   )
BEGIN
    RAISERROR('Database user flamexliveapp does not match login flamexliveapp, or the user is orphaned. No grant applied.', 16, 1);
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    IF USER_ID(N'flamexliveapp') IS NULL
        CREATE USER [flamexliveapp] FOR LOGIN [flamexliveapp];

    GRANT SELECT ON dbo.ImpersonationSessions
    (
        ImpersonationId,
        CompanyID,
        ActorUserId,
        TargetUserId,
        ActorSessionToken,
        TargetSessionToken,
        LeaseExpiresAt,
        EndedAt,
        IsActive
    ) TO [flamexliveapp];

    GRANT UPDATE ON dbo.ImpersonationSessions
    (
        LastHeartbeat,
        IsActive,
        EndedAt,
        EndReason
    ) TO [flamexliveapp];

    GRANT INSERT ON dbo.ImpersonationSessions TO [flamexliveapp];

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT
    c.column_id,
    c.name AS ColumnName,
    TYPE_NAME(c.user_type_id) AS TypeName,
    c.max_length,
    c.precision,
    c.scale,
    c.is_nullable
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(N'dbo.ImpersonationSessions')
ORDER BY c.column_id;

SELECT kc.name AS ConstraintName, kc.type_desc
FROM sys.key_constraints kc
WHERE kc.parent_object_id = OBJECT_ID(N'dbo.ImpersonationSessions')
UNION ALL
SELECT cc.name, N'CHECK_CONSTRAINT'
FROM sys.check_constraints cc
WHERE cc.parent_object_id = OBJECT_ID(N'dbo.ImpersonationSessions');

SELECT
    dc.name AS DefaultName,
    COL_NAME(dc.parent_object_id, dc.parent_column_id) AS ColumnName,
    dc.definition
FROM sys.default_constraints dc
WHERE dc.parent_object_id = OBJECT_ID(N'dbo.ImpersonationSessions');

WITH ExpectedIndex AS (
    SELECT *
    FROM (VALUES
        (N'IX_ImpersonationSessions_Company_Active', N'NONCLUSTERED', CAST(NULL AS nvarchar(4000))),
        (N'IX_ImpersonationSessions_ActorToken', N'NONCLUSTERED', CAST(NULL AS nvarchar(4000))),
        -- Catalog form of impersonation_pr1.sql: WHERE TargetSessionToken IS NOT NULL
        (N'IX_ImpersonationSessions_TargetToken', N'NONCLUSTERED', N'([TargetSessionToken] IS NOT NULL)')
    ) v(IndexName, TypeDesc, FilterDefinition)
),
ExpectedKey AS (
    SELECT *
    FROM (VALUES
        (N'IX_ImpersonationSessions_Company_Active', 1, N'CompanyID'),
        (N'IX_ImpersonationSessions_Company_Active', 2, N'IsActive'),
        (N'IX_ImpersonationSessions_ActorToken', 1, N'ActorSessionToken'),
        (N'IX_ImpersonationSessions_TargetToken', 1, N'TargetSessionToken')
    ) v(IndexName, KeyOrdinal, ColumnName)
)
SELECT
    e.IndexName,
    CASE
        WHEN i.index_id IS NULL THEN N'MISSING'
        WHEN i.type_desc <> e.TypeDesc THEN N'TYPE_MISMATCH'
        WHEN EXISTS (
            SELECT 1
            FROM sys.index_columns ic
            WHERE ic.object_id = i.object_id
              AND ic.index_id = i.index_id
              AND ic.is_included_column = 1
        ) THEN N'EXTRA_INCLUDED_COLUMN'
        WHEN EXISTS (
            SELECT 1
            FROM ExpectedKey ek
            WHERE ek.IndexName = e.IndexName
              AND NOT EXISTS (
                    SELECT 1
                    FROM sys.index_columns ic
                    INNER JOIN sys.columns c
                        ON c.object_id = ic.object_id
                       AND c.column_id = ic.column_id
                    WHERE ic.object_id = i.object_id
                      AND ic.index_id = i.index_id
                      AND ic.is_included_column = 0
                      AND ic.is_descending_key = 0
                      AND ic.key_ordinal = ek.KeyOrdinal
                      AND c.name = ek.ColumnName
              )
        ) THEN N'KEY_MISMATCH'
        WHEN EXISTS (
            SELECT 1
            FROM sys.index_columns ic
            INNER JOIN sys.columns c
                ON c.object_id = ic.object_id
               AND c.column_id = ic.column_id
            WHERE ic.object_id = i.object_id
              AND ic.index_id = i.index_id
              AND ic.is_included_column = 0
              AND NOT EXISTS (
                    SELECT 1
                    FROM ExpectedKey ek
                    WHERE ek.IndexName = e.IndexName
                      AND ek.KeyOrdinal = ic.key_ordinal
                      AND ek.ColumnName = c.name
                      AND ic.is_descending_key = 0
              )
        ) THEN N'UNEXPECTED_KEY_COLUMN'
        WHEN (
            SELECT COUNT(*)
            FROM sys.index_columns ic
            WHERE ic.object_id = i.object_id
              AND ic.index_id = i.index_id
              AND ic.is_included_column = 0
        ) <> (
            SELECT COUNT(*)
            FROM ExpectedKey ek
            WHERE ek.IndexName = e.IndexName
        ) THEN N'KEY_COUNT_MISMATCH'
        WHEN e.FilterDefinition IS NULL AND i.filter_definition IS NOT NULL THEN N'FILTER_MISMATCH'
        WHEN e.FilterDefinition IS NOT NULL AND ISNULL(i.filter_definition, N'') <> e.FilterDefinition THEN N'FILTER_MISMATCH'
        ELSE N'PRESENT'
    END AS IndexFinding,
    i.type_desc,
    i.filter_definition,
    e.FilterDefinition AS ExpectedFilterDefinition,
    (
        SELECT COUNT(*)
        FROM sys.index_columns ic
        WHERE ic.object_id = i.object_id
          AND ic.index_id = i.index_id
          AND ic.is_included_column = 0
    ) AS ActualKeyCount,
    (
        SELECT COUNT(*)
        FROM ExpectedKey ek
        WHERE ek.IndexName = e.IndexName
    ) AS ExpectedKeyCount
FROM ExpectedIndex e
LEFT JOIN sys.indexes i
    ON i.object_id = OBJECT_ID(N'dbo.ImpersonationSessions')
   AND i.name = e.IndexName;

SELECT
    dp.name AS DatabaseUser,
    dp.sid AS UserSid,
    @LoginSid AS LoginSid,
    CASE
        WHEN dp.sid = @LoginSid THEN N'MATCH'
        ELSE N'MISMATCH'
    END AS SidFinding
FROM sys.database_principals dp
WHERE dp.name = N'flamexliveapp';

SELECT
    N'EXPLICIT' AS GrantSource,
    e.PermissionName,
    e.ColumnName,
    CASE WHEN e.ColumnName = N'(table)' THEN 0 ELSE c.column_id END AS ExpectedMinorId,
    p.minor_id AS ActualMinorId,
    p.state_desc,
    CASE WHEN p.permission_name IS NULL THEN N'MISSING' ELSE N'PRESENT' END AS GrantFinding
FROM (VALUES
    (N'SELECT', N'ImpersonationId'),
    (N'SELECT', N'CompanyID'),
    (N'SELECT', N'ActorUserId'),
    (N'SELECT', N'TargetUserId'),
    (N'SELECT', N'ActorSessionToken'),
    (N'SELECT', N'TargetSessionToken'),
    (N'SELECT', N'LeaseExpiresAt'),
    (N'SELECT', N'EndedAt'),
    (N'SELECT', N'IsActive'),
    (N'UPDATE', N'LastHeartbeat'),
    (N'UPDATE', N'IsActive'),
    (N'UPDATE', N'EndedAt'),
    (N'UPDATE', N'EndReason'),
    (N'INSERT', N'(table)')
) e(PermissionName, ColumnName)
LEFT JOIN sys.columns c
    ON c.object_id = OBJECT_ID(N'dbo.ImpersonationSessions')
   AND c.name = e.ColumnName
LEFT JOIN sys.database_permissions p
    ON p.class_desc = N'OBJECT_OR_COLUMN'
   AND p.grantee_principal_id = USER_ID(N'flamexliveapp')
   AND p.major_id = OBJECT_ID(N'dbo.ImpersonationSessions')
   AND p.permission_name = e.PermissionName
   AND p.state_desc = N'GRANT'
   AND (
        (e.ColumnName = N'(table)' AND p.minor_id = 0)
        OR (e.ColumnName <> N'(table)' AND p.minor_id = c.column_id)
   );

SELECT
    N'DIRECT' AS GrantSource,
    p.permission_name,
    p.state_desc,
    p.minor_id,
    CASE WHEN p.minor_id = 0 THEN N'(table)' ELSE c.name END AS ColumnName,
    CASE
        WHEN p.state_desc = N'GRANT_WITH_GRANT_OPTION' THEN N'GRANT_WITH_GRANT_OPTION'
        WHEN p.permission_name IN (N'INSERT', N'REFERENCES') AND p.minor_id > 0 THEN N'UNEXPECTED_COLUMN_GRANT'
        WHEN p.permission_name IN (N'DELETE', N'ALTER', N'CONTROL', N'EXECUTE', N'REFERENCES') THEN N'UNEXPECTED_PERMISSION'
        WHEN p.minor_id = 0 AND p.permission_name IN (N'SELECT', N'UPDATE') THEN N'UNEXPECTED_TABLE_GRANT'
        ELSE N'UNEXPECTED_DIRECT_GRANT'
    END AS GrantFinding
FROM sys.database_permissions p
LEFT JOIN sys.columns c
    ON c.object_id = p.major_id
   AND c.column_id = p.minor_id
WHERE p.class_desc = N'OBJECT_OR_COLUMN'
  AND p.grantee_principal_id = USER_ID(N'flamexliveapp')
  AND p.major_id = OBJECT_ID(N'dbo.ImpersonationSessions')
  AND NOT EXISTS (
        SELECT 1
        FROM (VALUES
            (N'SELECT', N'ImpersonationId'),
            (N'SELECT', N'CompanyID'),
            (N'SELECT', N'ActorUserId'),
            (N'SELECT', N'TargetUserId'),
            (N'SELECT', N'ActorSessionToken'),
            (N'SELECT', N'TargetSessionToken'),
            (N'SELECT', N'LeaseExpiresAt'),
            (N'SELECT', N'EndedAt'),
            (N'SELECT', N'IsActive'),
            (N'UPDATE', N'LastHeartbeat'),
            (N'UPDATE', N'IsActive'),
            (N'UPDATE', N'EndedAt'),
            (N'UPDATE', N'EndReason'),
            (N'INSERT', N'(table)')
        ) expected(PermissionName, ColumnName)
        LEFT JOIN sys.columns ec
            ON ec.object_id = OBJECT_ID(N'dbo.ImpersonationSessions')
           AND ec.name = expected.ColumnName
        WHERE expected.PermissionName = p.permission_name
          AND p.state_desc = N'GRANT'
          AND (
                (expected.ColumnName = N'(table)' AND p.minor_id = 0)
                OR (expected.ColumnName <> N'(table)' AND ec.column_id = p.minor_id)
          )
  );

-- PARTIAL catalog check only. Database-role rows in sys.database_permissions.
-- This is not a complete effective-permission result.
SELECT
    N'PARTIAL_DATABASE_ROLE' AS GrantSource,
    rolePrincipal.name AS RoleName,
    p.permission_name,
    p.state_desc,
    CASE WHEN p.minor_id = 0 THEN N'(table)' ELSE col.name END AS ColumnName
FROM sys.database_role_members rm
INNER JOIN sys.database_principals rolePrincipal
    ON rolePrincipal.principal_id = rm.role_principal_id
INNER JOIN sys.database_permissions p
    ON p.grantee_principal_id = rolePrincipal.principal_id
   AND p.major_id = OBJECT_ID(N'dbo.ImpersonationSessions')
   AND p.class_desc = N'OBJECT_OR_COLUMN'
LEFT JOIN sys.columns col
    ON col.object_id = p.major_id
   AND col.column_id = p.minor_id
WHERE rm.member_principal_id = USER_ID(N'flamexliveapp');

-- PARTIAL catalog check only. Public object grants.
-- This is not a complete effective-permission result.
SELECT
    N'PARTIAL_PUBLIC' AS GrantSource,
    p.permission_name,
    p.state_desc,
    CASE WHEN p.minor_id = 0 THEN N'(table)' ELSE col.name END AS ColumnName
FROM sys.database_permissions p
LEFT JOIN sys.columns col
    ON col.object_id = p.major_id
   AND col.column_id = p.minor_id
WHERE p.grantee_principal_id = DATABASE_PRINCIPAL_ID(N'public')
  AND p.major_id = OBJECT_ID(N'dbo.ImpersonationSessions')
  AND p.class_desc = N'OBJECT_OR_COLUMN';
GO

/* ============================================================================
   SEPARATE RUNTIME VERIFICATION STEP
   Execute this batch only as database principal flamexliveapp.
   Do not run it as dbo. A dbo session does not prove the grant.
   A full-file execute stops here until the session user is flamexliveapp.
   ============================================================================ */

SET NOCOUNT ON;

IF DB_NAME() <> N'flamex_live'
BEGIN
    RAISERROR('Lease probe database guard failed: DB_NAME() must be flamex_live.', 16, 1);
    RETURN;
END;

IF USER_NAME() <> N'flamexliveapp'
BEGIN
    RAISERROR('Separate runtime verification: execute this batch as database principal flamexliveapp.', 16, 1);
    RETURN;
END;

SELECT TOP (1) 1 AS LeaseProbe
FROM dbo.ImpersonationSessions
WHERE IsActive = 1
  AND EndedAt IS NULL
  AND LeaseExpiresAt > SYSUTCDATETIME()
  AND (
        ActorSessionToken = CONVERT(uniqueidentifier, '00000000-0000-0000-0000-000000000000')
        OR TargetSessionToken = CONVERT(uniqueidentifier, '00000000-0000-0000-0000-000000000000')
        OR ActorUserId = 0
  );
