/* ============================================================================
   NAME:        create_flmx_live_reader
   WHEN:        2026-11-09
   WHY:         FLMX LIVE diagnostics need a dedicated read-only principal, separate from flamex_uat.
   WHAT:        Database user flmx_mcp_live_reader on flamex_live, explicit CONNECT, and SELECT on the five Switch User diagnostic tables.
   ============================================================================ */

-- DBA review only. Do not run from the application or from Cursor.
-- The server login must already exist. This script does not create a login
-- and does not contain a password or connection string.
-- It reads flamex_uat only to prove the login has no user there.
-- Unexpected access aborts. Nothing already granted is revoked.
-- Guest is measured with EXECUTE AS and is not changed.
-- ALTER, CONTROL, and TAKE OWNERSHIP are rejected. IMPERSONATE is read from the catalog.
-- VIEW DEFINITION and column-key metadata are not treated as data access.

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @LoginName sysname = N'flmx_mcp_live_reader';
DECLARE @LoginSid varbinary(85) = SUSER_SID(N'flmx_mcp_live_reader');
DECLARE @Reason nvarchar(2048);

IF DB_ID(N'flamex_live') IS NULL OR DB_ID(N'flamex_uat') IS NULL
BEGIN
    SET @Reason = N'flamex_live or flamex_uat is not on this instance. No change was made.';
    SELECT N'ABORT' AS Result, @Reason AS Detail;
    THROW 51010, @Reason, 1;
END;

IF @LoginSid IS NULL
   OR NOT EXISTS (
        SELECT 1
        FROM sys.server_principals
        WHERE sid = @LoginSid
          AND name = @LoginName
          AND type = N'S'
          AND is_disabled = 0
   )
BEGIN
    SET @Reason = N'SQL login flmx_mcp_live_reader is missing, disabled, or not a SQL login. Create it separately. No change was made.';
    SELECT N'ABORT' AS Result, @Reason AS Detail;
    THROW 51011, @Reason, 1;
END;

IF EXISTS (
    SELECT 1
    FROM sys.server_role_members AS membership
    WHERE membership.member_principal_id = SUSER_ID(@LoginName)
)
BEGIN
    SET @Reason = N'flmx_mcp_live_reader is a member of a server role. Membership was left unchanged.';
    SELECT N'ABORT' AS Result, @Reason AS Detail;
    THROW 51012, @Reason, 1;
END;

IF EXISTS (
    SELECT 1
    FROM sys.server_permissions AS permission
    WHERE permission.grantee_principal_id = SUSER_ID(@LoginName)
      AND NOT (
            permission.class = 100
        AND permission.permission_name = N'CONNECT SQL'
        AND permission.state = N'G'
      )
)
BEGIN
    SET @Reason = N'flmx_mcp_live_reader has a server permission other than CONNECT SQL. It was left unchanged.';
    SELECT N'ABORT' AS Result, @Reason AS Detail;
    THROW 51013, @Reason, 1;
END;

IF EXISTS (
    SELECT 1
    FROM sys.databases
    WHERE owner_sid = @LoginSid
      AND name IN (N'flamex_live', N'flamex_uat')
)
BEGIN
    SET @Reason = N'flmx_mcp_live_reader owns flamex_live or flamex_uat. Ownership was left unchanged.';
    SELECT N'ABORT' AS Result, @Reason AS Detail;
    THROW 51014, @Reason, 1;
END;

IF EXISTS (
    SELECT 1
    FROM flamex_uat.sys.database_principals AS principal
    WHERE principal.sid = @LoginSid
       OR principal.name = @LoginName
)
BEGIN
    SET @Reason = N'flmx_mcp_live_reader already has a database user in flamex_uat. That user was left unchanged.';
    SELECT N'ABORT' AS Result, @Reason AS Detail;
    THROW 51015, @Reason, 1;
END;

IF OBJECT_ID(N'flamex_live.dbo.Permissions', N'U') IS NULL
   OR OBJECT_ID(N'flamex_live.dbo.Roles', N'U') IS NULL
   OR OBJECT_ID(N'flamex_live.dbo.RolePermissions', N'U') IS NULL
   OR OBJECT_ID(N'flamex_live.dbo.UserRoles', N'U') IS NULL
   OR OBJECT_ID(N'flamex_live.dbo.tbl_login', N'U') IS NULL
BEGIN
    SET @Reason = N'One of the Switch User diagnostic tables is missing from flamex_live. No change was made.';
    SELECT N'ABORT' AS Result, @Reason AS Detail;
    THROW 51017, @Reason, 1;
END;

USE flamex_live;

DECLARE @ExistingSid varbinary(85);
DECLARE @UserId int;
DECLARE @GuestConnect int;
DECLARE @ReaderConnect int;
DECLARE @ApprovedSelect int;
DECLARE @Excessive bit;
DECLARE @Approved TABLE (object_id int NOT NULL PRIMARY KEY);

INSERT INTO @Approved (object_id)
SELECT approved.object_id
FROM (VALUES
    (OBJECT_ID(N'dbo.Permissions')),
    (OBJECT_ID(N'dbo.Roles')),
    (OBJECT_ID(N'dbo.RolePermissions')),
    (OBJECT_ID(N'dbo.UserRoles')),
    (OBJECT_ID(N'dbo.tbl_login'))
) AS approved(object_id);

SELECT
    @ExistingSid = principal.sid,
    @UserId = principal.principal_id
FROM sys.database_principals AS principal
WHERE principal.name = @LoginName
  AND principal.type = N'S';

IF @UserId IS NOT NULL AND (@ExistingSid IS NULL OR @ExistingSid <> @LoginSid)
BEGIN
    SET @Reason = N'flamex_live user flmx_mcp_live_reader maps to a different SID. The user was not changed.';
    SELECT N'ABORT' AS Result, @Reason AS Detail;
    THROW 51018, @Reason, 1;
END;

IF EXISTS (
    SELECT 1
    FROM sys.database_principals AS principal
    WHERE principal.sid = @LoginSid
      AND principal.name <> @LoginName
)
BEGIN
    SET @Reason = N'The login SID already maps to a different user in flamex_live. No user was changed.';
    SELECT N'ABORT' AS Result, @Reason AS Detail;
    THROW 51019, @Reason, 1;
END;

IF @UserId IS NOT NULL AND EXISTS (
    SELECT 1
    FROM sys.database_role_members AS membership
    WHERE membership.member_principal_id = @UserId
)
BEGIN
    SET @Reason = N'flmx_mcp_live_reader belongs to a database role other than public. Membership was left unchanged.';
    SELECT N'ABORT' AS Result, @Reason AS Detail;
    THROW 51020, @Reason, 1;
END;

IF @UserId IS NOT NULL
   AND (
        EXISTS (
            SELECT 1
            FROM sys.schemas AS owned_schema
            WHERE owned_schema.principal_id = @UserId
        )
        OR EXISTS (
            SELECT 1
            FROM sys.objects AS owned_object
            WHERE owned_object.principal_id = @UserId
        )
        OR EXISTS (
            SELECT 1
            FROM sys.database_principals AS owned_principal
            WHERE owned_principal.owning_principal_id = @UserId
        )
   )
BEGIN
    SET @Reason = N'flmx_mcp_live_reader owns a schema, object, or principal in flamex_live. Ownership was left unchanged.';
    SELECT N'ABORT' AS Result, @Reason AS Detail;
    THROW 51026, @Reason, 1;
END;

IF @UserId IS NOT NULL AND EXISTS (
    SELECT 1
    FROM sys.database_permissions AS permission
    WHERE permission.grantee_principal_id = @UserId
      AND NOT (
            (permission.class = 0 AND permission.type = N'CO' AND permission.state = N'G')
         OR (
                permission.class = 1
            AND permission.type = N'SL'
            AND permission.state = N'G'
            AND permission.minor_id = 0
            AND permission.major_id IN (SELECT object_id FROM @Approved)
            )
         OR (permission.class = 4 AND permission.type = N'IM')
      )
)
BEGIN
    SET @Reason = N'flmx_mcp_live_reader has a permission outside CONNECT and SELECT on the five diagnostic tables. Grants were left unchanged.';
    SELECT N'ABORT' AS Result, @Reason AS Detail;
    THROW 51021, @Reason, 1;
END;

-- Effective CONNECT, including CONNECT inherited from public.
BEGIN TRY
    EXECUTE AS USER = N'guest';
    SET @GuestConnect = HAS_PERMS_BY_NAME(DB_NAME(), N'DATABASE', N'CONNECT');
    REVERT;
END TRY
BEGIN CATCH
    DECLARE @GuestErrorNumber int = ERROR_NUMBER();
    DECLARE @GuestImpersonated bit = CASE WHEN USER_NAME() = N'guest' THEN 1 ELSE 0 END;

    IF @GuestImpersonated = 1
        REVERT;

    IF @GuestErrorNumber = 916 AND @GuestImpersonated = 0
        SET @GuestConnect = 0;
    ELSE
    BEGIN
        SET @Reason = N'Could not evaluate guest CONNECT. Guest permissions were not changed. ' + ERROR_MESSAGE();
        SELECT N'ABORT' AS Result, @Reason AS Detail;
        THROW;
    END
END CATCH;

IF ISNULL(@GuestConnect, 1) = 1
BEGIN
    SET @Reason = N'guest has effective CONNECT on flamex_live, including permissions inherited from public. Guest permissions were not changed.';
    SELECT N'ABORT' AS Result, @Reason AS Detail;
    THROW 51016, @Reason, 1;
END;

-- Class 4 is DATABASE_PRINCIPAL. DENY on the reader or public overrides GRANT.
IF EXISTS (
    SELECT 1
    FROM sys.database_permissions AS grant_permission
    WHERE grant_permission.class = 4
      AND grant_permission.type = N'IM'
      AND grant_permission.state IN (N'G', N'W')
      AND grant_permission.grantee_principal_id IN (
            DATABASE_PRINCIPAL_ID(@LoginName),
            DATABASE_PRINCIPAL_ID(N'public')
      )
      AND NOT EXISTS (
            SELECT 1
            FROM sys.database_permissions AS deny_permission
            WHERE deny_permission.class = 4
              AND deny_permission.type = N'IM'
              AND deny_permission.state = N'D'
              AND deny_permission.major_id = grant_permission.major_id
              AND deny_permission.grantee_principal_id IN (
                    DATABASE_PRINCIPAL_ID(@LoginName),
                    DATABASE_PRINCIPAL_ID(N'public')
              )
      )
)
BEGIN
    SET @Reason = N'flmx_mcp_live_reader has effective IMPERSONATE on a database principal, including grants inherited from public. Grants were left unchanged.';
    SELECT N'ABORT' AS Result, @Reason AS Detail;
    THROW 51027, @Reason, 1;
END;

BEGIN TRANSACTION;

IF @UserId IS NULL
BEGIN
    CREATE USER [flmx_mcp_live_reader] FOR LOGIN [flmx_mcp_live_reader];
END;

GRANT CONNECT TO [flmx_mcp_live_reader];
GRANT SELECT ON OBJECT::dbo.Permissions TO [flmx_mcp_live_reader];
GRANT SELECT ON OBJECT::dbo.Roles TO [flmx_mcp_live_reader];
GRANT SELECT ON OBJECT::dbo.RolePermissions TO [flmx_mcp_live_reader];
GRANT SELECT ON OBJECT::dbo.UserRoles TO [flmx_mcp_live_reader];
GRANT SELECT ON OBJECT::dbo.tbl_login TO [flmx_mcp_live_reader];

SET @ReaderConnect = 0;
SET @ApprovedSelect = 0;
SET @Excessive = 0;

BEGIN TRY
    EXECUTE AS USER = @LoginName;

    SET @ReaderConnect = ISNULL(HAS_PERMS_BY_NAME(DB_NAME(), N'DATABASE', N'CONNECT'), 0);

    SELECT @ApprovedSelect = COUNT(*)
    FROM (VALUES
        (N'dbo.Permissions'),
        (N'dbo.Roles'),
        (N'dbo.RolePermissions'),
        (N'dbo.UserRoles'),
        (N'dbo.tbl_login')
    ) AS approved(object_name)
    WHERE HAS_PERMS_BY_NAME(approved.object_name, N'OBJECT', N'SELECT') = 1;

    IF EXISTS (
            SELECT 1
            FROM (VALUES (N'SELECT'), (N'INSERT'), (N'UPDATE'), (N'DELETE'), (N'EXECUTE'), (N'ALTER'), (N'CONTROL'), (N'TAKE OWNERSHIP')) AS permission(permission_name)
            WHERE HAS_PERMS_BY_NAME(DB_NAME(), N'DATABASE', permission.permission_name) = 1
        )
        OR EXISTS (
            SELECT 1
            FROM sys.schemas AS sch
            CROSS APPLY (VALUES (N'SELECT'), (N'INSERT'), (N'UPDATE'), (N'DELETE'), (N'EXECUTE'), (N'ALTER'), (N'CONTROL'), (N'TAKE OWNERSHIP')) AS permission(permission_name)
            WHERE HAS_PERMS_BY_NAME(QUOTENAME(sch.name), N'SCHEMA', permission.permission_name) = 1
        )
        OR EXISTS (
            SELECT 1
            FROM sys.objects AS obj
            CROSS APPLY (VALUES (N'SELECT'), (N'INSERT'), (N'UPDATE'), (N'DELETE'), (N'EXECUTE'), (N'ALTER'), (N'CONTROL'), (N'TAKE OWNERSHIP')) AS permission(permission_name)
            WHERE obj.is_ms_shipped = 0
              AND obj.parent_object_id = 0
              AND obj.type IN (N'U', N'V', N'P', N'FN', N'IF', N'TF', N'SN')
              AND HAS_PERMS_BY_NAME(
                    QUOTENAME(SCHEMA_NAME(obj.schema_id)) + N'.' + QUOTENAME(obj.name),
                    N'OBJECT',
                    permission.permission_name
                  ) = 1
              AND (
                    obj.object_id NOT IN (SELECT object_id FROM @Approved)
                 OR permission.permission_name <> N'SELECT'
              )
        )
        OR EXISTS (
            SELECT 1
            FROM sys.columns AS col
            INNER JOIN sys.objects AS obj
                ON obj.object_id = col.object_id
            CROSS APPLY (VALUES (N'SELECT'), (N'INSERT'), (N'UPDATE'), (N'DELETE')) AS permission(permission_name)
            WHERE obj.is_ms_shipped = 0
              AND obj.type IN (N'U', N'V')
              AND HAS_PERMS_BY_NAME(
                    QUOTENAME(SCHEMA_NAME(obj.schema_id)) + N'.' + QUOTENAME(obj.name),
                    N'OBJECT',
                    permission.permission_name,
                    col.name,
                    N'COLUMN'
                  ) = 1
              AND (
                    obj.object_id NOT IN (SELECT object_id FROM @Approved)
                 OR permission.permission_name <> N'SELECT'
              )
        )
        SET @Excessive = 1;

    REVERT;
END TRY
BEGIN CATCH
    IF USER_NAME() = @LoginName
        REVERT;
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    SET @Reason = N'Effective permission check failed. The transaction was rolled back. ' + ERROR_MESSAGE();
    SELECT N'ABORT' AS Result, @Reason AS Detail;
    THROW;
END CATCH;

IF @ReaderConnect <> 1 OR @ApprovedSelect <> 5 OR @Excessive = 1
BEGIN
    ROLLBACK TRANSACTION;
    SET @Reason = N'Effective access is not CONNECT plus SELECT on the five diagnostic tables. The transaction was rolled back.';
    SELECT N'ABORT' AS Result, @Reason AS Detail;
    THROW 51025, @Reason, 1;
END;

COMMIT TRANSACTION;

SELECT
    N'PASS' AS Result,
    @ReaderConnect AS EffectiveConnect,
    @ApprovedSelect AS EffectiveSelectOnApprovedTables,
    principal.name AS DatabaseUser,
    role_principal.name AS DatabaseRole,
    permission.permission_name,
    permission.state_desc,
    permission.class_desc,
    OBJECT_SCHEMA_NAME(permission.major_id) AS ObjectSchema,
    OBJECT_NAME(permission.major_id) AS ObjectName
FROM sys.database_principals AS principal
LEFT JOIN sys.database_role_members AS membership
    ON membership.member_principal_id = principal.principal_id
LEFT JOIN sys.database_principals AS role_principal
    ON role_principal.principal_id = membership.role_principal_id
LEFT JOIN sys.database_permissions AS permission
    ON permission.grantee_principal_id = principal.principal_id
WHERE principal.name = @LoginName
  AND principal.sid = @LoginSid
ORDER BY permission.class_desc, OBJECT_NAME(permission.major_id), permission.permission_name;
