# 34 — Impersonation invariants INV-13 through INV-17

**ADR:** [ADR-001](ADR-001_Administrator_Impersonation.md)  
**Code:** `ImpersonationRuntime` / `ImpersonationGovernance.Inv*`

These gates run only after `IsSwitchUserEnabled` is true. While `SwitchUser=false`, Issue/Start/Close return `Disabled` and do not evaluate INV-13–17 against the database.

| ID | Name | Behavior |
|----|------|----------|
| INV-13 | Nested impersonation | Reject Issue/Start when `Session["ImpersonationLink"]` is set or an active ledger row exists for the actor token / target token / actor user. |
| INV-14 | Self-impersonation | Reject when `targetUserId == Session["UserDbId"]`. |
| INV-15 | Target company | Reject unless the target is `tbl_login.IsActive=1`, home `CompanyID` equals the actor's current company, not locked out, and has active `UserCompanyAccess` for that company. Missing membership table → reject. |
| INV-16 | Intent token | Reject when HMAC fails, payload is malformed, TTL elapsed, actor/company/session mismatch, or Start INSERT hits the pre-allocated `ImpersonationId` PK (replay). |
| INV-17 | Exactly-once close | `UPDATE … WHERE ImpersonationId=@Id AND IsActive=1 AND EndedAt IS NULL`. Zero rows → `ClosedNoOp`, no `ActiveSessions` or identity mutation. One row → restore actor once. |

Denials (when the flag is on) write `dbo.AuthAudit` event `ImpersonationStartDenied` or `ImpersonationCloseNoOp`. Audit failure on Start/Issue fails closed (no token / no lease).
