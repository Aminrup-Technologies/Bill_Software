# ADR-001 — Administrator impersonation stays off until explicitly enabled

**Status:** Accepted (PR-2A governance + PR-3 dormant runtime)  
**Date:** 2026-09-10  
**Depends on:** PR-1 (`db/impersonation_pr1.sql`, neutralized `SwitchUser.aspx`)

## Decision

Administrator impersonation is a future capability. The runtime engine exists in-process and stays inert until **all** of these are true: feature flag `SwitchUser=true`, an explicit `RolePermissions` grant, and a caller of `ImpersonationRuntime` other than the dormant banner/logout hooks.

- `Permissions.PermissionKey = SwitchUser` may exist as catalog metadata.
- `RolePermissions` must not grant that key in this PR.
- AppSetting `SwitchUser` defaults to `false`. Missing or any non-`true` value is disabled.
- `AuthGuard` is unchanged.
- `ImpersonationRuntime` public entry points return `Disabled` with no SQL when the flag is off.
- Bill.Master banner is `Visible=false` unless the flag is on **and** a lease is active.
- `SwitchUser.aspx` remains a session-only stub: "This function is not available."

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

Code: `ImpersonationGovernance`, `ImpersonationLink`, `ImpersonationIntentToken`, `ImpersonationRuntime`, `SecurityAudit` (`dbo.AuthAudit`).

## Consequences

Existing authentication and authorization stay on the docs/22 six-layer path. Enabling impersonation later is a flag + grant + UI caller change, not a rewrite of the lease engine.
