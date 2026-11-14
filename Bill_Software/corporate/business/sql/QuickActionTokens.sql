/* ============================================================================
   NAME:        quick_action_tokens_schema
   WHEN:        2026-11-14
   WHY:         FEE-14 / A-08 — emailed QuickAction approve/reject links must be
                time-bounded and single-use. Possession of a forwarded link must
                not remain a durable credential. Application fails closed (SQL 208)
                when this table is missing.
   WHAT:        Required DDL for dbo.QuickActionTokens. Apply on UAT/production
                before relying on hardened MyLeaves / attendance / AdminOverride
                email action links.
   ============================================================================ */

IF OBJECT_ID(N'dbo.QuickActionTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.QuickActionTokens
    (
        TokenId       INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_QuickActionTokens PRIMARY KEY,
        TokenHash     CHAR(64) NOT NULL,            -- SHA-256 hex of the URL token
        RequestID     INT NOT NULL,
        RequestType   VARCHAR(10) NOT NULL,         -- Leave | Reg
        Action        VARCHAR(10) NOT NULL,         -- Approve | Reject
        ManagerID     NVARCHAR(50) NOT NULL,
        CompanyID     INT NOT NULL,
        ExpiresAtUtc  DATETIME NOT NULL,
        UsedAtUtc     DATETIME NULL,
        CreatedAtUtc  DATETIME NOT NULL
            CONSTRAINT DF_QuickActionTokens_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_QuickActionTokens_TokenHash UNIQUE (TokenHash)
    );

    CREATE INDEX IX_QuickActionTokens_Request_Active
        ON dbo.QuickActionTokens (CompanyID, RequestType, RequestID, Action, UsedAtUtc, ExpiresAtUtc);
END
GO
