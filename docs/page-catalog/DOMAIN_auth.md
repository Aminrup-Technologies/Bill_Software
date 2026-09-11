# Authentication & session

> Domain key: `auth` · 4 page(s)

Public and keep-alive pages. Shared AuthN lives in docs/14 and docs/22.

**Do not copy** AuthN/AuthZ, tenancy, or Ponytail rules here. Those live in [SHARED_CONTEXT](SHARED_CONTEXT.md).

| Page | Purpose | Master / class | Gate | Tables (unique) | Ops | Narrative |
|------|---------|----------------|------|-----------------|-----|-----------|
| `SessionKeepAlive.aspx` | Session Keep Alive | none / `SessionKeepAlive` | Session<br>— | `tbl_login` | SELECT | this catalog |
| `index.aspx` | index | none / `index` | public<br>— | `tbl_login`, `Roles`, `ActiveSessions` | INSERT, UPDATE | [module](../14_Authentication_Authorization_Architecture.md) |
| `index_start.aspx` | index start | none / `Index` | public<br>— | — | — | this catalog |
| `reset_password.aspx` | reset password<br>QS: `token`, `uid` | none / `reset_password` | public<br>— | — | — | [module](../14_Authentication_Authorization_Architecture.md) |

## Page-unique notes

- `index.aspx`: ERP login / OTP / session issue. Canonical write-up: docs/14 + docs/22. Do not restate PBKDF2 or ActiveSessions here.
- `index_start.aspx`: Empty launcher. ImageButton1 redirects to missing `index_quotation.aspx`; ImageButton2 → `index_card.aspx`.
- `reset_password.aspx`: Public token page (`token`, `uid`). Must not inherit Bill.Master. See docs/14 / Phase 0B.

<!-- NARRATIVE:BEGIN -->

## Page behavior

- **`index.aspx`:** `btnLogin`; lockout flags `MustUpdateUserId` / `MustVerifyContact`; email OTP `btnSendOTP` / `btnVerifyOTP`; forgot-password via `PasswordResetService` (mail/WhatsApp) → `reset_password.aspx`.
- **`reset_password.aspx`:** QS `uid` + `token`; `PasswordResetService.TryCompleteReset`. **`PasswordResetTokens` is not on UAT** ([SHARED_SCHEMA.md](SHARED_SCHEMA.md)).
- **`SessionKeepAlive.aspx`:** ERP timer; concat SQL on `tbl_login` (kiosk twin is `SessionKeepAlive1`).
- **`index_start.aspx`:** dead launcher. ImageButton1 → missing `index_quotation.aspx`; ImageButton2 → `index_card.aspx`.
