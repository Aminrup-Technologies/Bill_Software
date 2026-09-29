# 37 — Switch User UAT completion (Phase 2A)

**Baseline:** `efc2099a4e0dc9426a4c1b1f0cc34325ee2fdb77` (`July_to_Sept26_DevNSupport`, merge of [PR #83](https://github.com/Aminrup-Technologies/Bill_Software/pull/83))  
**ADR:** [ADR-001](ADR-001_Administrator_Impersonation.md)

## 1. Executive summary

Administrator Switch User is **complete on UAT** and **off in production**. The dormant engine (PR #82) is wired behind `SwitchUser=true` plus a Super Admin grant (PR #83). Home-tenant membership (PR #81) and the inert ledger DDL (PR #77 / `db/impersonation_pr1.sql`) are prerequisites.

| PR | Role |
|----|------|
| [#77](https://github.com/Aminrup-Technologies/Bill_Software/pull/77) | Neutralize prototype; `ImpersonationSessions` DDL (inert) |
| [#81](https://github.com/Aminrup-Technologies/Bill_Software/pull/81) | Home-tenant `UserCompanyAccess` seed |
| [#82](https://github.com/Aminrup-Technologies/Bill_Software/pull/82) | Dormant runtime (link, intent, lease, INV-13–17, audit) |
| [#83](https://github.com/Aminrup-Technologies/Bill_Software/pull/83) | UAT callers, Super Admin grant script, actor-check and menu-refresh fixes |

## 2. Architecture delivered

| Piece | Contract |
|-------|----------|
| Actor / Target | `ImpersonationLink` snapshots actor identity. Start swaps `USERID` / `UserDbId` to the target. Rollback restores the actor. Observed users do not need `SwitchUser`. |
| `UserCompanyAccess` | Company gate before `HasPermission`. Target must have active membership in the actor’s current company (INV-15); home `tbl_login.CompanyID` is not checked. Actor continuation also requires `SwitchUser` plus active membership in `CompanyContext.CurrentCompanyID` ([35](35_Impersonation_Runtime_Engine.md#actor-continuation)). |
| `ImpersonationSessions` | Lease: `IsActive`, `EndedAt`, `EndReason`, tokens, `LeaseExpiresAt`. Empty until Start. |
| `AuthAudit` | `SecurityAudit` writes Intent / Start / Closed in the same transaction as the lease. Exactly-once close: second close is `ClosedNoOp` with no extra session mutation. |

`AuthGuard` is unchanged. `Heartbeat.ashx` is unchanged. Production `Web.Release.config` still inserts `SwitchUser=false`.

## 3. UAT evidence

| Cycle | Result |
|-------|--------|
| admin → AT01 → End | `ManualRollback`. Actor menu restored. |
| admin → FLM025 | Observation stays open. Empty menu is least-privilege (no `UserRoles`). |
| End Impersonation | ADMIN header **and** admin menu without F5 (`0daf8b1`). |
| Audit | One Intent, one Start, one Closed per cycle. |
| Leases | Latest success: ImpersonationId `8D3759DA-ACAA-4994-9C81-FCD22E2313F6`, `EndReason=ManualRollback`, `IsActive=0`. No orphan active rows. |

Catalog: `Permissions.SwitchUser` = **PermissionId 132**. Super Admin granted on UAT only.

## 4. Defects found during UAT

| Defect | Symptom |
|--------|---------|
| Missing `UserCompanyAccess` | Company gate 403 before any Switch User work (fixed by PR #81 + DBA seed). |
| Missing `SwitchUser` catalog / grant | `HasPermission` 403 on `SwitchUser.aspx`. |
| Missing `ImpersonationSessions` | First Start → INV-13 (`HasActiveLease` fail-closed on SQL 208). |
| Actor permission check | admin → FLM025 closed in ~2s with `PermissionRevoked` (`BindMasterBanner` used target `USERID`). |
| Rollback menu | Header restored to ADMIN; menu stayed empty until F5 (`GetMenuControl` skipped on postback). |

## 5. Resolution summary

| Commit | Fix |
|--------|-----|
| `3a770cb` | `ActorHoldsSwitchUser` during observation; INV-13 unchanged. |
| `0daf8b1` | `GetMenuControl` + `BindSwitchUserMenu` after `CloseCurrent(ManualRollback)`. |
| `efc2099` | Merge PR #83 into `July_to_Sept26_DevNSupport`. |

Host/DBA steps (including `QUOTED_IDENTIFIER ON` for the filtered index) are canonical in [`SWITCHUSER_DEPLOYMENT_CHECKLIST.md`](SWITCHUSER_DEPLOYMENT_CHECKLIST.md), not here.

## 6. Production readiness

**Deployment prerequisite ([PR #94](https://github.com/Aminrup-Technologies/Bill_Software/pull/94)):** complete `UserCompanyAccess` reconciliation (`user_company_access_reconciliation.sql`, DBA review) before deploying, so every existing actor account holding `SwitchUser` has an active membership for each company it operates in. Missing membership fails closed: targets are rejected (INV-15) and actor sessions end with `PermissionRevoked`.

UAT was enabled on the host; production stays fail-closed. Current enable/remain-off contract: [`SWITCHUSER_DEPLOYMENT_CHECKLIST.md`](SWITCHUSER_DEPLOYMENT_CHECKLIST.md). Heartbeat lease pings remain unwired (`Heartbeat.ashx` unchanged). Nested impersonation stays INV-13.
