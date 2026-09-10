# 29 — Impersonation governance

**Status:** Governance + dormant runtime. Live impersonation is off.  
**ADR:** [ADR-001_Administrator_Impersonation.md](ADR-001_Administrator_Impersonation.md)

## In tree

- Catalog metadata for `SwitchUser` (SQL, DBA-executed, no grant).
- Disabled feature flag `SwitchUser=false`.
- `ImpersonationGovernance` constants aligned with `db/impersonation_pr1.sql`.
- `ImpersonationRuntime` Start/Rollback engine, gated on the flag (no SQL when disabled).
- Bill.Master banner plumbing, hidden unless flag **and** active lease.
- Documentation: ADR-001 and docs 30–35.

## Still not activated

Menu enablement, `RolePermissions` grants, `AuthGuard` contract changes, `Heartbeat.ashx` lease pings, `SwitchUser.aspx` identity swap.

`SwitchUser.aspx` remains PR-1: `AuthGuard.EnsurePage(this, false, null)` and "This function is not available."
