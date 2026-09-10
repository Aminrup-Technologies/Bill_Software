# ADR-001 — Administrator impersonation stays off until explicitly enabled

**Status:** Accepted (PR-2A governance + PR-3 dormant runtime)  
**Date:** 2026-09-10  
**Depends on:** PR-1 (`db/impersonation_pr1.sql`, neutralized `SwitchUser.aspx`)

## Decision

Administrator impersonation stays fail-closed unless **all** of these are true: AppSetting `SwitchUser=true`, Super Admin `RolePermissions` grant, and a caller of `ImpersonationRuntime`.

PR-83 wires UAT callers only. Production `Web.Release.config` remains `SwitchUser=false`. UAT operators set the flag on the UAT host. DBA grant script is not executed by the app.

- `AuthGuard` is unchanged.
- `Heartbeat.ashx` is unchanged.
- Banner uses existing `BindMasterBanner`.
- Menu `SwitchUser` is visible only when the flag is true and the user has the permission.

## Contracts

| Doc | Contract |
|-----|----------|
| [29_Impersonation_Governance.md](29_Impersonation_Governance.md) | Scope and non-goals |
| [30_Impersonation_Permission_Catalog.md](30_Impersonation_Permission_Catalog.md) | Catalog row, no grant |
| [31_Impersonation_Feature_Flag.md](31_Impersonation_Feature_Flag.md) | `SwitchUser=false` fail-closed |
| [32_Impersonation_Session_Contract.md](32_Impersonation_Session_Contract.md) | Ledger + lease lifecycle |
| [33_Impersonation_EndReasons.md](33_Impersonation_EndReasons.md) | 11 EndReason values |
| [34_Impersonation_Invariants_13_17.md](34_Impersonation_Invariants_13_17.md) | INV-13–17 |
| [35_Impersonation_Runtime_Engine.md](35_Impersonation_Runtime_Engine.md) | Start/Rollback engine |
| [36_Impersonation_UAT_Activation.md](36_Impersonation_UAT_Activation.md) | UAT callers + Super Admin grant script |

Code: `ImpersonationGovernance`, `ImpersonationLink`, `ImpersonationIntentToken`, `ImpersonationRuntime`, `SecurityAudit` (`dbo.AuthAudit`).

## Consequences

Existing authentication and authorization stay on the docs/22 six-layer path. Enabling impersonation later is a flag + grant + UI caller change, not a rewrite of the lease engine.
