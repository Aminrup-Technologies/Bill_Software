# 33 — Impersonation EndReason values

**ADR:** [ADR-001](ADR-001_Administrator_Impersonation.md)  
**Enforced by:** `CK_ImpersonationSessions_EndReason` in `db/impersonation_pr1.sql`  
**C#:** `ImpersonationGovernance.EndReason`

| # | Value |
|---|--------|
| 1 | ManualRollback |
| 2 | ActorLogout |
| 3 | TargetLogout |
| 4 | IdleTimeout |
| 5 | HeartbeatMissed |
| 6 | LeaseExpired |
| 7 | ActorSessionKilled |
| 8 | TargetSessionKilled |
| 9 | ForcedRevoke |
| 10 | PermissionRevoked |
| 11 | SystemFault |

PR-3 writes `EndReason` only from `ImpersonationRuntime.Close` when the feature flag is on. Do not invent additional values without changing the CHECK constraint and this list together.
