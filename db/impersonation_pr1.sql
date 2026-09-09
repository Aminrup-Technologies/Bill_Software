-- =============================================================================
-- When: PR-1 impersonation stabilization, before any runtime impersonation work.
-- Why:  The unshipped Switch User prototype does not compile (AuthGuard has no
--       3-parameter HasPermission overload) and must not activate identity swap.
--       Later PRs need an empty, backward-compatible ledger; this script only
--       prepares that object. It does not turn impersonation on.
-- What: Create dbo.ImpersonationSessions if missing. No rows. No permission
--       catalog insert. No RolePermissions grant. No ActiveSessions INSERT
--       or ALTER. Application code in PR-1 does not read or write this table.
-- =============================================================================
-- INERT / DO NOT EXECUTE FROM THE APPLICATION.
-- Canonical EndReason values (11). Documented here and enforced by CHECK.
-- Runtime writers belong to later PRs; do not seed these as rows.
--   1. ManualRollback
--   2. ActorLogout
--   3. TargetLogout
--   4. IdleTimeout
--   5. HeartbeatMissed
--   6. LeaseExpired
--   7. ActorSessionKilled
--   8. TargetSessionKilled
--   9. ForcedRevoke
--  10. PermissionRevoked
--  11. SystemFault
-- =============================================================================

IF OBJECT_ID(N'dbo.ImpersonationSessions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ImpersonationSessions
    (
        ImpersonationId     UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_ImpersonationSessions PRIMARY KEY
            CONSTRAINT DF_ImpersonationSessions_Id DEFAULT (NEWID()),
        CompanyID           INT NOT NULL,
        ActorUserId         INT NOT NULL,                 -- tbl_login.Id of the actor
        TargetUserId        INT NOT NULL,                 -- tbl_login.Id of the target
        ActorSessionToken   UNIQUEIDENTIFIER NOT NULL,    -- existing ActiveSessions token; no FK
        TargetSessionToken  UNIQUEIDENTIFIER NULL,        -- session linking is not activated in PR-1
        StartedAt           DATETIMEOFFSET(7) NOT NULL
            CONSTRAINT DF_ImpersonationSessions_StartedAt DEFAULT (SYSUTCDATETIME()),
        LastHeartbeat       DATETIMEOFFSET(7) NULL,       -- heartbeat PR; unused
        LeaseExpiresAt      DATETIMEOFFSET(7) NULL,       -- lease-probe PR; unused
        EndedAt             DATETIMEOFFSET(7) NULL,
        EndReason           NVARCHAR(32) NULL,
        IsActive            BIT NOT NULL
            CONSTRAINT DF_ImpersonationSessions_IsActive DEFAULT (0), -- inert default
        CONSTRAINT CK_ImpersonationSessions_EndReason CHECK (
            EndReason IS NULL
            OR EndReason IN (
                N'ManualRollback',
                N'ActorLogout',
                N'TargetLogout',
                N'IdleTimeout',
                N'HeartbeatMissed',
                N'LeaseExpired',
                N'ActorSessionKilled',
                N'TargetSessionKilled',
                N'ForcedRevoke',
                N'PermissionRevoked',
                N'SystemFault'
            )
        )
    );

    CREATE NONCLUSTERED INDEX IX_ImpersonationSessions_Company_Active
        ON dbo.ImpersonationSessions (CompanyID, IsActive);

    CREATE NONCLUSTERED INDEX IX_ImpersonationSessions_ActorToken
        ON dbo.ImpersonationSessions (ActorSessionToken);

    CREATE NONCLUSTERED INDEX IX_ImpersonationSessions_TargetToken
        ON dbo.ImpersonationSessions (TargetSessionToken)
        WHERE TargetSessionToken IS NOT NULL;
END
GO
