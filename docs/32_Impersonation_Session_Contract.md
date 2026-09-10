# 32 — Impersonation session contract

**ADR:** [ADR-001](ADR-001_Administrator_Impersonation.md)  
**DDL:** `db/impersonation_pr1.sql` (inert ledger; PR-1)

## Ledger

`dbo.ImpersonationSessions` (`ImpersonationGovernance.LedgerTable`) is the membership/session ACL for a future impersonation lease. PR-2A does not INSERT, UPDATE, or SELECT it from application code.

Authoritative columns (already in PR-1 DDL): `ImpersonationId`, `CompanyID`, `ActorUserId`, `TargetUserId`, `ActorSessionToken`, `TargetSessionToken`, `StartedAt`, `LastHeartbeat`, `LeaseExpiresAt`, `EndedAt`, `EndReason`, `IsActive`.

`LastHeartbeat` and `LeaseExpiresAt` stay unused until a later PR. Default `IsActive = 0`.

Runtime ERP session remains `tbl_login` + `ActiveSessions` + `Session["USERID"]`. PR-2A does not copy or replace that session.
