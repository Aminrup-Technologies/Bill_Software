# Switch User deployment contract

**Status:** Canonical host + DBA steps  
**Branch:** `July_to_Sept26_DevNSupport`  
**ADR:** [ADR-001](ADR-001_Administrator_Impersonation.md)  
**History:** [36 UAT activation](36_Impersonation_UAT_Activation.md), [37 UAT completion](37_SwitchUser_UAT_Completion.md)

This is the **only** place that lists enablement, database, verification, and rollback steps. Implementation history stays in docs/36–37. Architecture stays in ADR-001 and [ARCHITECTURE.md](ARCHITECTURE.md).

Do not execute any `.sql` in this contract from the application. Do not commit `SwitchUser=true` to `Web.config`. Do not copy UAT `Web.config` secrets onto production.

---

## Purpose and scope

Switch User **code is on this branch**. Runtime stays **fail-closed** until **all** of: host AppSetting `SwitchUser=true`, Super Admin `RolePermissions` grant, `dbo.ImpersonationSessions`, and a caller of `ImpersonationRuntime` (`SwitchUser.aspx`).

This contract covers **UAT enablement** and **production remain-off**. It does not change `AuthGuard`, `SecurePage`, or `Heartbeat.ashx`.

---

## UAT vs production

| Environment | Flag | Grant | Ledger | Expected UI |
|-------------|------|-------|--------|-------------|
| **UAT** | Host overlay `SwitchUser=true` (not in git) | `SwitchUser_superadmin_grant_uat.sql` | `impersonation_pr1.sql` | Super Admin sees Users → Switch User |
| **Production** | `Web.Release.config` `InsertIfMissing` `false` | Do **not** run the UAT grant | Apply ledger DDL if missing (still inert) | Menu hidden; page shows “This function is not available.” |
| **Local / untransformed `Web.config`** | Key **absent** | — | — | Disabled (`IsSwitchUserEnabled` is false when missing/empty/non-`true`) |

Committed `Bill_Software/Web.config` must **not** contain `SwitchUser=true`. `Web.Debug.config` and `Web.Release.config` insert `false` if the key is missing.

---

## Runtime readiness matrix

| Prerequisite | In repo | Who applies | Required for Start |
|--------------|---------|-------------|--------------------|
| `SwitchUser.aspx` + `ImpersonationRuntime` (flag off → `Disabled`, no SQL) | Code | Deploy branch | Yes (compiled) |
| AppSetting `SwitchUser` exactly `true` | **Host only** | UAT operator | Yes on UAT |
| `Permissions.SwitchUser` catalog | `SwitchUser_permission.sql` | DBA | Yes |
| Super Admin `RolePermissions` | `SwitchUser_superadmin_grant_uat.sql` | DBA (UAT only) | Yes on UAT |
| `dbo.ImpersonationSessions` | `db/impersonation_pr1.sql` | DBA | Yes (`HasActiveLease` SQL 208 → INV-13 deny) |
| `dbo.UserCompanyAccess` + home-tenant rows | `db/user_company_access_seed.sql` | DBA | Yes (else company 403) |
| `dbo.AuthAudit` | **External** (UAT catalog; **no CREATE in this repo**) | DBA / already on UAT | Yes (intent INSERT fail-closed) |
| Session `USERID`, `UserDbId`, `CompanyID`, `SessionToken` | Login + Master | Runtime | Yes |
| `UrlTokenAesKey` | Empty in git is OK | — | HMAC uses compiled fallback if empty |

---

## Host configuration checklist

UAT host overlay only. Do not commit.

```xml
<appSettings>
  <add key="SwitchUser" value="true" />
</appSettings>
```

- Recycle the UAT app pool after the change.
- Production publish must keep `Web.Release.config` `SwitchUser=false`.
- The stash "Local docs and config before Cross-Tenant UAT" (currently `stash@{3}`; indices shift) holds `SwitchUser=true` as a **local overlay**. Restore that key on the UAT host if needed; do **not** restore `Web.config` connection strings or MCP files from the stash.

---

## Database deployment checklist

Run as DBA, in order, against the target database (`flamex_uat` for UAT). Scripts are idempotent.

1. Confirm `dbo.UserCompanyAccess` exists. If active users 403 on company-gated pages, run [`user_company_access_seed.sql`](../db/user_company_access_seed.sql).
2. Run [`impersonation_pr1.sql`](../db/impersonation_pr1.sql) (`SET QUOTED_IDENTIFIER ON` is required for `IX_ImpersonationSessions_TargetToken`).
3. Run [`SwitchUser_permission.sql`](../Bill_Software/corporate/business/sql/SwitchUser_permission.sql).
4. **UAT only:** run [`SwitchUser_superadmin_grant_uat.sql`](../Bill_Software/corporate/business/sql/SwitchUser_superadmin_grant_uat.sql). Grants **every** `RoleName = N'Super Admin'` row (not company-filtered). Do not run on production.
5. Confirm `dbo.AuthAudit` exists (`UserId`, `EventType`, `IPAddress`, `Details`). There is **no** AuthAudit CREATE script in this repository. If missing, Start fails with intent audit fault.

If `ImpersonationSessions` already exists without the filtered index, create it separately with `SET QUOTED_IDENTIFIER ON` — the `IF OBJECT_ID IS NULL` block will not re-run.

---

## Verification checklist

1. Super Admin with home-tenant membership: Users menu shows **Switch User**.
2. Non-granted user: menu hidden; direct URL 403 when flag is on.
3. Flag off (or missing): page stub “This function is not available.”; no SQL from `ImpersonationRuntime`.
4. Start → banner + target identity; `CompanyID` **unchanged**.
5. Nested Start: INV-13; list hidden; `GateActor` still denies.
6. End impersonation: actor identity restored without F5.
7. Logout while observing: `ActorLogout` then normal logout.
8. `ImpersonationSessions`: one active lease during observe; `EndReason=ManualRollback` (or `ActorLogout`) and `IsActive=0` after close.
9. `AuthAudit`: Intent, Start, Closed (one each per successful cycle).

---

## Rollback checklist

1. Set UAT host `SwitchUser` to `false` or remove the key. Recycle the app pool.
2. Do **not** delete `Permissions.SwitchUser` or `ImpersonationSessions` as a first step — flag off is sufficient (runtime returns `Disabled` with no SQL).
3. Optional UAT-only: delete the Super Admin `RolePermissions` row for `SwitchUser` (menu stays hidden even if someone re-enables the flag).
4. Production: leave Release transform `false`; do not apply the UAT grant.

---

## Known limitations

- `Heartbeat.ashx` is **not** wired to impersonation lease close (`IdleTimeout` / `HeartbeatMissed` / `LeaseExpired` are EndReason values only). Idle leases stay until End impersonation or actor logout.
- Nested impersonation remains INV-13.
- `AuthAudit` is an external UAT object; this repo does not version its CREATE.

---

## References

| Document / script | Role |
|-------------------|------|
| [ADR-001](ADR-001_Administrator_Impersonation.md) | Fail-closed decision |
| [31_Impersonation_Feature_Flag.md](31_Impersonation_Feature_Flag.md) | Flag reader |
| [36_Impersonation_UAT_Activation.md](36_Impersonation_UAT_Activation.md) | PR-83 callers (historical) |
| [37_SwitchUser_UAT_Completion.md](37_SwitchUser_UAT_Completion.md) | UAT evidence (historical) |
| `Bill_Software/Web.Debug.config`, `Web.Release.config` | `InsertIfMissing` `SwitchUser=false` |
