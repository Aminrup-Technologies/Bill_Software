# Phase 0B — Credential Hardening and Secrets Deployment

**Status:** Application code for Phase 0B. Does not rotate production secrets.  
**Date:** 2026-09-07  
**Depends on:** Phase 0A (PR #66) authorization gates. Do not regress those gates.  
**Auth model:** Unchanged — `tbl_login` + `Session` + `dbo.ActiveSessions`. No ASP.NET Identity. No JWT.

Secret **values** are not listed in this document. Configure them on the server (IIS application settings, `Web.Release.config`, or machine-level `web.config`). Do not paste production values into source control or pull-request text.

---

## 1. Required database change (must apply)

Forgot-password now fails closed unless this table exists. Login still works without it.

Run `Bill_Software/corporate/business/sql/PasswordResetTokens.sql` on each environment (UAT, then production) before users rely on “Forgot Password”.

| Object | Purpose |
|--------|---------|
| `dbo.PasswordResetTokens` | Stores SHA-256 hex of a CSPRNG reset token, UTC expiry, and `UsedAtUtc` for one-time use |

The application **does not** write a temporary password and **does not** null `PasswordHash` during a reset request. The stored hash changes only after `reset_password.aspx` verifies an unused, unexpired token.

If the table is missing, users see a generic “temporarily unavailable” message. There is no plaintext fallback.

---

## 2. Authentication behavior (tbl_login)

| Situation | Result |
|-----------|--------|
| `PasswordHash` + `PasswordSalt` present | PBKDF2 only (`Rfc2898DeriveBytes`, 100000 iterations). Plaintext column is ignored. |
| Hash missing, leftover `Password` matches | One-time explicit upgrade: write PBKDF2 hash, set `Password = NULL`, then continue the existing session/`ActiveSessions` login path. |
| Hash missing and no matching plaintext | Fail closed. User must complete token reset or an admin reset. |
| Hash present and verify fails | Fail closed. Plaintext is never tried. |

Session creation (`USERID`, `SessionToken`, kill/insert `ActiveSessions`, Forms cookie) is unchanged after a successful verify.

---

## 3. AppSettings keys (Phase 0B)

Add these in server configuration. Empty `UrlTokenAesKey` preserves currently mailed QuickAction links.

| Key | Required | Meaning |
|-----|----------|---------|
| `UrlTokenAesKey` | Operational | Exactly 32 UTF-8 bytes. Used by `SecurityHelper` for leave/regularization URL tokens. If omitted or empty, the previously compiled key is used so existing links still decrypt. **Do not put a new random value here until operators are ready to invalidate outstanding QuickAction links.** First set this key to the same 32-byte value that is currently compiled, then rotate later in a coordinated change. |
| `PasswordResetTokenMinutes` | Optional | Reset token lifetime. Default `30`. |
| `LoginOtpExpiryMinutes` | Optional | Login email-OTP lifetime. Default `10`. |

Misconfigured `UrlTokenAesKey` (wrong length) fails closed at encrypt/decrypt time.

---

## 4. Secrets already resolved from configuration (do not strip)

These are already read at runtime via `ConfigurationManager`. This PR does **not** remove them from the committed `Web.config` because that would break current UAT/IIS deployments. Operators should move values out of source control into environment-specific config when they can coordinate a deploy.

| Setting | Location | Notes |
|---------|----------|--------|
| `DbConn` | `connectionStrings` | Database credential. Keep the live value on the server. Do not rotate in this change. |
| `SmtpFrom`, `SmtpUser`, `SmtpPass`, `SmtpHost`, `SmtpPort`, `SmtpEnableSsl` | `appSettings` | Auth-flow and `CommunicationGateway` email. Already config-driven. |
| `iTopUrl`, `iTopUser`, `iTopPass`, `iTopCallerEmail`, `iTopOrgName` | `appSettings` | iTop ticket API. Already config-driven. |
| `Msg91AuthKey`, `Msg91IntegratedNumber`, `Msg91OtpTemplateId` | `appSettings` | WhatsApp/SMS. Already config-driven. |
| `<machineKey>` | `system.web` | ASP.NET ViewState / Forms cookie protection. **Do not rotate in this PR.** Changing validation/decryption keys without a full-farm cutover invalidates sessions and ViewState. Move the existing keys to IIS machine-level or `Web.Release.config` only as a coordinated ops task. |

Hardcoded SMTP inside sales-visit pages (`vw_dailyrpts.aspx.cs`, `srch_dailyrpts.aspx.cs`) is defect D-11 and is **out of Phase 0B scope**.

---

## 5. Out of scope / stopped

| Item | Why stopped |
|------|-------------|
| `index_card.aspx.cs` / `tbl_card_login` | Separate kiosk login. Plaintext compare, concatenated SQL, no hash columns in this codebase. Inventing a hash schema would lock the kiosk. Tracked as A-02. Not part of `tbl_login` Phase 0B. |
| `UserCompanyAccess` wiring | Blocked until product backfill (Phase 0A). |
| AES key rotation | Would break outstanding QuickAction email links. Operators copy the current operational key into `UrlTokenAesKey` first. |
| Connection string / machineKey / SMTP value removal from `Web.config` | Would break current deployments. Resolution is already configuration-based; relocation is an ops change. |

---

## 6. Deploy order

1. Keep Phase 0A gates deployed.  
2. Apply `PasswordResetTokens.sql`.  
3. Deploy the Phase 0B application bits.  
4. Optionally set `UrlTokenAesKey` to the **current** 32-byte operational value (not a new random key).  
5. Confirm a known PBKDF2 user can log in, OTP email still works, and a forgot-password token can be used once.  
6. Plan a later secret-rotation window (SMTP, API keys, machineKey, AES) separately from this code change.
