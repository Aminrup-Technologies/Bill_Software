/* ============================================================================
   NAME:        password_reset_tokens_schema
   WHEN:        2026-09-07
   WHY:         Phase 0B forgot-password must not null PasswordHash or store a
                temporary password. Reset requires a hashed, expiring, one-time
                token table. Application code is wired to dbo.PasswordResetTokens
                and fails closed if this object is missing.
   WHAT:        Required DDL for dbo.PasswordResetTokens. Apply on UAT/production
                before enabling the new reset flow for end users.
   ============================================================================ */

IF OBJECT_ID(N'dbo.PasswordResetTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PasswordResetTokens
    (
        TokenId       INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_PasswordResetTokens PRIMARY KEY,
        UserDbId      INT NOT NULL,                 -- tbl_login.Id
        UserId        NVARCHAR(128) NOT NULL,       -- tbl_login.User_Id
        TokenHash     NVARCHAR(64) NOT NULL,        -- SHA-256 hex of the raw token
        ExpiresAtUtc  DATETIME NOT NULL,
        UsedAtUtc     DATETIME NULL,
        CreatedAtUtc  DATETIME NOT NULL
            CONSTRAINT DF_PasswordResetTokens_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_PasswordResetTokens_Login
            FOREIGN KEY (UserDbId) REFERENCES dbo.tbl_login (Id)
    );

    CREATE INDEX IX_PasswordResetTokens_UserId_Active
        ON dbo.PasswordResetTokens (UserId, UsedAtUtc, ExpiresAtUtc);
END
GO
