# 36 — Impersonation UAT activation (PR-83)

**ADR:** [ADR-001](ADR-001_Administrator_Impersonation.md)  
**Runtime:** [35](35_Impersonation_Runtime_Engine.md) (unchanged INV-13–17)

## What this PR turns on

| Surface | Gate |
|---------|------|
| `SwitchUser.aspx` | Flag `true` then `AuthGuard.EnsurePage(..., "SwitchUser")`. Calls `IssueIntent` + `Start`. Flag off keeps the unavailable stub. |
| Header Users menu `id="SwitchUser"` | Visible only when flag is true **and** `HasPermission("SwitchUser")` **and** no active `ImpersonationLink`. |
| Bill.Master banner | Existing `BindMasterBanner` (flag + active lease). |

`AuthGuard` and `Heartbeat.ashx` are unchanged. Production `Web.Release.config` still inserts `SwitchUser=false`.

## UAT host (operators)

1. Set AppSetting `SwitchUser` to `true` on the **UAT** `Web.config` only. Do not set this on production.
2. DBA: `SwitchUser_permission.sql` (catalog) if missing, then `SwitchUser_superadmin_grant_uat.sql` (Super Admin grant only). Do not execute from the app.

## Validation notes

| Path | Expected |
|------|----------|
| Start | Super Admin + flag → intent then lease; session becomes target; `AuthAudit` Start. INV-13–16 still deny. |
| Observe | Banner visible while lease is active; header shows target identity. |
| Rollback | Banner **End impersonation** → `ManualRollback`; actor session restored. |
| Logout | `CloseCurrent(ActorLogout)` then existing logout. |
| Permission revocation | Banner bind closes with `PermissionRevoked` if `SwitchUser` is lost. |
| Target disable | Inactive / locked / no `UserCompanyAccess` → INV-15; no Start. |
