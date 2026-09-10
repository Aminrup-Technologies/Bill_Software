# 29 — Impersonation governance (PR-2A)

**Status:** Governance only. Runtime impersonation is off.  
**ADR:** [ADR-001_Administrator_Impersonation.md](ADR-001_Administrator_Impersonation.md)

## In this PR

- Catalog metadata for `SwitchUser` (SQL, DBA-executed, no grant).
- Disabled feature flag `SwitchUser=false`.
- `ImpersonationGovernance` constants aligned with `db/impersonation_pr1.sql`.
- Documentation: ADR-001 and docs 30–33.

## Not in this PR

Session switching, heartbeat, impersonation banner, lease timers, `AuthGuard` changes, `RolePermissions` grants, writers to `dbo.ImpersonationSessions`.

`SwitchUser.aspx` is unchanged from PR-1: `AuthGuard.EnsurePage(this, false, null)` and no identity swap.
