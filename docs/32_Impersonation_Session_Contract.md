# 32 — Impersonation session contract

**ADR:** [ADR-001](ADR-001_Administrator_Impersonation.md)  
**DDL:** `db/impersonation_pr1.sql`  
**Writer:** `ImpersonationRuntime` (only when `SwitchUser` is exactly `true`)

## Ledger

`dbo.ImpersonationSessions` (`ImpersonationGovernance.LedgerTable`) is the lease ACL.

Authoritative columns: `ImpersonationId`, `CompanyID`, `ActorUserId`, `TargetUserId`, `ActorSessionToken`, `TargetSessionToken`, `StartedAt`, `LastHeartbeat`, `LeaseExpiresAt`, `EndedAt`, `EndReason`, `IsActive`.

## Link model

`ImpersonationLink` is stored in `Session["ImpersonationLink"]` after a successful Start. It snapshots actor identity so Rollback can restore `USERID` / `UserDbId` / role / profile without trusting the target session.

## Lease lifecycle

| Step | Behavior when flag is true |
|------|----------------------------|
| Intent | HMAC token embeds a pre-allocated `ImpersonationId`; TTL `IntentTtlMinutes` (2). |
| Start | INSERT lease `IsActive=1`, INSERT target `ActiveSessions`, deactivate actor token, swap session, `AuthAudit` Start. |
| Heartbeat | UPDATE `LastHeartbeat` if lease still unexpired; otherwise Close `LeaseExpired`. Not wired to `Heartbeat.ashx`. |
| Close | UPDATE `IsActive=0` only when `IsActive=1 AND EndedAt IS NULL`; deactivate target token; INSERT restored actor `ActiveSessions`. |

When the flag is false, none of these statements run.

Runtime ERP session remains `tbl_login` + `ActiveSessions` + `Session["USERID"]`.
