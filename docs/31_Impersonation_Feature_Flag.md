# 31 — Impersonation feature flag

**ADR:** [ADR-001](ADR-001_Administrator_Impersonation.md)

## Flag

| Item | Value |
|------|--------|
| AppSettings key | `SwitchUser` |
| Committed default | `false` via `Web.Debug.config` / `Web.Release.config` (`InsertIfMissing`). Missing AppSettings key is also disabled. |
| C# constant | `ImpersonationGovernance.FeatureFlagKey` |
| Enabled only when | trimmed value equals `true` (ordinal, ignore case) |

Missing, empty, `false`, `0`, or any other string → disabled (`ImpersonationGovernance.IsSwitchUserEnabled` is false).

PR-2A does not read this flag on the request path. A later PR may gate runtime impersonation on it. Do not treat the catalog row as enablement.
