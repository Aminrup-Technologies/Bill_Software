# 35 — Impersonation runtime engine (dormant)

**ADR:** [ADR-001](ADR-001_Administrator_Impersonation.md)  
**Flag:** [31](31_Impersonation_Feature_Flag.md) (`SwitchUser=false`)

## Types

| Type | Job |
|------|-----|
| `ImpersonationLink` | Serializable actor/target snapshot in session. |
| `ImpersonationIntent` / `ImpersonationIntentToken` | HMAC-SHA256 intent (`UrlTokenAesKey` material). |
| `ImpersonationRuntime` | `IssueIntent`, `Start`, `Close` / `CloseCurrent`, `HeartbeatCurrent`, `BindMasterBanner`. |
| `SecurityAudit` | Parameterized INSERT into `dbo.AuthAudit`. |

## Public gate

First line of every public runtime method: if `!ImpersonationGovernance.IsSwitchUserEnabled` return `Disabled` (no SQL, no session swap).

## Call sites in this PR

| Site | When flag is false |
|------|--------------------|
| `Bill.Master` `BindMasterBanner` | Panel stays `Visible=false`. |
| `lnkEndImpersonation_Click` | `CloseCurrent` no-ops. |
| `btnLogOut_Click` | `CloseCurrent(ActorLogout)` no-ops, then existing logout. |
| `SwitchUser.aspx` | Flag off: unavailable stub. Flag on: `IssueIntent` + `Start`. |
| Users menu `SwitchUser` | Visible only when flag + permission + no active link. |
| `Heartbeat.ashx` | Unchanged. Lease heartbeat is not registered. |

## Actor continuation

`BindMasterBanner` re-checks the actor snapshot on every Master load (`ActorHoldsSwitchUser`). The session continues only while the actor holds **both**:

- `SwitchUser` via `UserRoles` → `RolePermissions`, and
- active `UserCompanyAccess` (`IsActive=1`) for `CompanyContext.CurrentCompanyID`.

Home `tbl_login.CompanyID` is not consulted. Either check failing → `Close(PermissionRevoked)`. A company switch keeps the session only while the actor retains membership in the newly selected company.

UAT grant: `SwitchUser_superadmin_grant_uat.sql` (DBA only). Production flag remains false. See [docs/36](36_Impersonation_UAT_Activation.md).
