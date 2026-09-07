# Phase 3 — Security Infrastructure & Legacy Boundary Hardening

**Date:** 2026-09-07  
**Depends on:** PR #66–#72. The ERP authentication pipeline (`tbl_login`, PBKDF2, OTP, password reset, `ActiveSessions`, `SecurePage`, `UserRoles`, `UserCompanyAccess`, resource authorization) is **not** changed.  
**Policy:** Preserve runtime compatibility. Do not rotate secrets or `machineKey` in this change. Do not merge the card kiosk into ERP auth.

Secret **values** are not listed here. Do not paste production credentials into source control or pull-request text.

---

## 1. Isolated kiosk boundary (A-02)

`index_card.aspx` + `tbl_card_login` + `admin/card.Master` is a **separate security domain**. It is not `tbl_login`.

| Topic | Kiosk (`index_card`) | ERP (`index.aspx`) |
|-------|----------------------|--------------------|
| Store | `tbl_card_login` | `tbl_login` |
| Password | Plaintext column compare | PBKDF2 (`PasswordHash` / `PasswordSalt`) |
| SQL | **Phase 3:** parameterized `User_Id` | Parameterized (Phase 0B) |
| Session | `Session["USERID"]` only | `USERID` + `UserDbId` + `SessionToken` + `ActiveSessions` |
| Master | `admin/card.Master` | `Bill.Master` |
| Permissions | None (`GetMenuControl` is empty) | `UserRoles` → `RolePermissions` → `Permissions` |
| Company | Not bound | `UserCompanyAccess` + `Session["CompanyID"]` |
| Logout | Redirect to `~/index.aspx` (ERP login); **does not** `Session.Clear` / deactivate a token | Deactivates `ActiveSessions`, clears session |
| After login | `~/admin/home.aspx` | `~/corporate/business/app/home.aspx` |

**Do not claim parity.** A kiosk session that sets `Session["USERID"]` can look like an ERP user to any page that only tests `USERID != null` and does not require `SessionToken`. ERP pages that use `AuthGuard.TryValidateSession` reject that session. Pages that only check `USERID` (including some kiosk pages) do not.

### What Phase 3 changed in code

- `index_card.aspx.cs` login `SELECT` uses `@UserId`. Concatenation on the **login path** is removed.
- Password compare remains plaintext. This codebase has **no** `PasswordHash` / `PasswordSalt` columns on `tbl_card_login`. Inventing PBKDF2 would lock the kiosk. **STOP.**

### Remaining kiosk debt (not this PR)

Concatenated `tbl_card_login` SQL still exists on `admin/card.Master.cs`, `admin/home.aspx.cs`, `admin/Setting.aspx.cs`, `admin/Update/*.aspx.cs`, `SessionKeepAlive1.aspx.cs`. Those are the same isolated boundary; they were not rewritten so kiosk profile/password screens stay behavior-identical.

`card.Master` missing-session redirect goes to **ERP** `index.aspx`, not `index_card.aspx`. Logout does not abandon session. Do not “fix” those without a kiosk product decision.

---

## 2. Secret inventory (names only)

Runtime resolution is already **configuration-first** (`ConfigurationManager` / `AppSecrets`). This PR does **not** delete values from committed `Web.config` (that would break current IIS/UAT). Operators should relocate values when they can coordinate a deploy.

| Name | Where resolved | Purpose | Rotation in this PR |
|------|----------------|---------|---------------------|
| `DbConn` | `connectionStrings` | SQL Server | **No.** Ops window. |
| `SmtpFrom`, `SmtpUser`, `SmtpPass`, `SmtpHost`, `SmtpPort`, `SmtpEnableSsl` | `appSettings` | Auth and `CommunicationGateway` mail | **No.** |
| `iTopUrl`, `iTopUser`, `iTopPass`, `iTopCallerEmail`, `iTopOrgName` | `appSettings` | iTop tickets | **No.** |
| `Msg91AuthKey`, `Msg91IntegratedNumber`, `Msg91OtpTemplateId` | `appSettings` | WhatsApp/SMS | **No.** |
| `UrlTokenAesKey` | `appSettings` via `AppSecrets.GetUrlTokenAesKey` | AES-256 for QuickAction URL tokens | **No.** Empty key keeps compiled fallback so mailed links still decrypt. |
| `<machineKey>` validationKey / decryptionKey | `system.web` | ViewState + Forms cookie MAC | **No.** See §3. |
| Compiled AES fallback in `AppSecrets` | source | Pre-Phase 0B QuickAction key | **Do not drop** until `UrlTokenAesKey` is set to that same operational value on every server. |

Out of this phase: hardcoded SMTP in `vw_dailyrpts.aspx.cs` / `srch_dailyrpts.aspx.cs` (D-11).

---

## 3. machineKey governance

**Do not rotate in this change.** A new validation/decryption key pair without a full-farm cutover:

1. Invalidates in-flight ViewState (postbacks fail).
2. Invalidates ASP.NET Forms authentication cookies (ERP companion cookie from Phase 0A).
3. Does **not** by itself clear `dbo.ActiveSessions`; users may still have InProc session until recycle.

### Rotation procedure (ops, later)

1. Schedule an IIS recycle window for **all** servers that share this app.
2. Generate new keys (`validation="SHA1"` unless the farm is also moving to HMACSHA256 in a planned upgrade).
3. Place keys in **IIS machine-level** `web.config` or `Web.Release.config` — not a developer commit of production material.
4. Deploy the same keys to every node, then recycle together.
5. Expect users to sign in again. Warn that open forms will lose postback state.
6. Confirm ERP login + a ViewState postback + `/Uploads` Forms cookie still work.

Until then, keep the committed keys as the current operational set.

---

## 4. Crypto compatibility (no link invalidation)

| Mechanism | Algorithm | Config | Compatibility |
|-----------|-----------|--------|----------------|
| QuickAction URL tokens | AES, random IV prepended, URL-safe Base64 | `UrlTokenAesKey` or compiled 32-byte fallback | Existing mailed `?t=` links decrypt if the **same** 32-byte key is used. Empty AppSetting = fallback. Wrong length fails closed. |
| Password reset | CSPRNG token, SHA-256 hex in `PasswordResetTokens` | `PasswordResetTokenMinutes` (default 30) | Server-side table; not AES. Independent of `UrlTokenAesKey`. |
| Login OTP | CSPRNG, session expiry/attempts | `LoginOtpExpiryMinutes` (default 10) | Session-only; not `machineKey`. |
| ERP passwords | PBKDF2 100k | n/a | Unchanged this phase. |
| Kiosk passwords | Plaintext `tbl_card_login.Password` | n/a | Unchanged (no hash schema). |

### AES cutover (later, coordinated)

1. Set `UrlTokenAesKey` on the server to the **current** operational 32 UTF-8 bytes (the compiled fallback), **not** a new random string.
2. Confirm a known QuickAction leave/reg link still works.
3. Only then generate a new key, accept that outstanding emails fail, and remove the compiled fallback in a dedicated PR.

---

## 5. Operational cutover checklist

- [ ] ERP login (`index.aspx`) still PBKDF2 + `ActiveSessions` (this PR must not have touched it).
- [ ] Card kiosk login still works with existing `tbl_card_login` passwords.
- [ ] QuickAction leave/reg email links still decrypt.
- [ ] Forgot-password tokens still one-time SHA-256 (table already deployed from Phase 0B).
- [ ] `machineKey` **not** changed in production except in a scheduled farm window.
- [ ] Relocate `DbConn` / SMTP / iTop / Msg91 out of source when ops can restart IIS with server-level config.
- [ ] Do not drop `AppSecrets` AES fallback until `UrlTokenAesKey` matches the live key everywhere.

---

## 6. STOP register

| Item | Why stopped |
|------|-------------|
| PBKDF2 for `tbl_card_login` | No hash columns in this repository. |
| Strip secrets from `Web.config` | Would break current UAT/IIS until server config is ready. Resolution is already `ConfigurationManager`. |
| Rotate `machineKey` | Invalidates ViewState and Forms cookies across the farm. |
| Rotate `UrlTokenAesKey` to a new random value | Invalidates outstanding QuickAction links. |
| Merge kiosk into `AuthGuard` / `ActiveSessions` | Different table, no token, empty menu. Product isolation. |
| Parameterize remaining kiosk profile SQL | Behavior-preserving login-path only this phase. |

---

## Out of scope (unchanged)

ERP login, PBKDF2, reset, OTP, ActiveSessions, SecurePage, UserRoles, UserCompanyAccess, company switcher, menu, print, invoice/PO/stock/scheduler, sales-visit/quotation/leave authorization from Phases 2B–2C.
