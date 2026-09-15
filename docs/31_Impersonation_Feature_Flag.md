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

Every public `ImpersonationRuntime` method returns `Disabled` with no database access when this reader is false. Do not treat the catalog row as enablement.

Committed transforms stay `false` (`InsertIfMissing`). UAT-only enablement is an operator AppSetting on the UAT host. Production Release deploys must keep `false`.
