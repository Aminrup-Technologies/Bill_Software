# ADR-001 — Administrator impersonation stays off until explicitly enabled

**Status:** Accepted (PR-2A)  
**Date:** 2026-09-10  
**Depends on:** PR-1 (`db/impersonation_pr1.sql`, neutralized `SwitchUser.aspx`)

## Decision

Administrator impersonation is a future capability. PR-2A records governance only:

- `Permissions.PermissionKey = SwitchUser` may exist as catalog metadata.
- `RolePermissions` must not grant that key in this PR.
- AppSetting `SwitchUser` defaults to `false`. Missing or any non-`true` value is disabled.
- `AuthGuard` is unchanged.
- No identity swap, heartbeat, banner, or lease writer.

Later PRs may enable runtime impersonation only when **all** of these are true: feature flag `true`, an explicit role grant, and a dedicated writer for `dbo.ImpersonationSessions`.

## Contracts

| Doc | Contract |
|-----|----------|
| [29_Impersonation_Governance.md](29_Impersonation_Governance.md) | PR-2A scope and non-goals |
| [30_Impersonation_Permission_Catalog.md](30_Impersonation_Permission_Catalog.md) | Catalog row, no grant |
| [31_Impersonation_Feature_Flag.md](31_Impersonation_Feature_Flag.md) | `SwitchUser=false` fail-closed |
| [32_Impersonation_Session_Contract.md](32_Impersonation_Session_Contract.md) | Ledger columns; no writers in 2A |
| [33_Impersonation_EndReasons.md](33_Impersonation_EndReasons.md) | 11 EndReason values |

Code: `ImpersonationGovernance` (`PermissionKey`, `FeatureFlagKey`, `EndReason`, `IsSwitchUserEnabled`). Not called from `AuthGuard` or `SwitchUser.aspx` in PR-2A.

## Consequences

Existing authentication and authorization stay on the docs/22 six-layer path. `SwitchUser.aspx` remains a session-only stub: "This function is not available."
