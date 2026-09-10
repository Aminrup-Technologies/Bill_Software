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
| `SwitchUser.aspx` | Does **not** call the engine. Stub copy unchanged. |
| `Heartbeat.ashx` | Unchanged. Lease heartbeat is not registered. |

## Not in this PR

Menu item for Switch User, `RolePermissions` grants, AuthGuard API changes, UAT SQL execution, live identity swap.
