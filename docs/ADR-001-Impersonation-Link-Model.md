# ADR-001 — Administrator Impersonation: Link Model

| | |
|---|---|
| **Status** | **Accepted** |
| **Scope** | Administrator User Impersonation ("Switch User") in FLMX |
| **Review corpus** | Docs [23](23_Impersonation_Architecture_Review.md)–[33](33_Impersonation_PR1_Implementation_Readiness.md) |
| **Supersedes** | The legacy identity-swap design (`SwitchUser.aspx.cs` Original\* snapshot model — a Critical defect, removed in PR-1) |

---

## Decision

Adopt the **Link Model** for administrator impersonation:

1. Impersonation is a **database-anchored lease** (`dbo.ImpersonationLink`), never an identity transfer. The administrator's session, principal keys, and `ActiveSessions` row are never modified by observation (dual identity model).
2. The **effective user** is resolved per request from the link into a request-scoped `SecurityContext`; the **actor** for all writes and audit remains the administrator.
3. Authorization deviation is confined to the **single `AuthGuard.HasPermission` fork**, which activates only when the session holds the `ImpersonationToken` pointer. The authentication leg and company-membership leg stay **principal-scoped** in all cases.
4. Every lifecycle transition is audited **exactly once** in the same transaction that changes link state, into append-only `dbo.SecurityAudit` (DENY UPDATE/DELETE).
5. Implementation proceeds as the canonical sequence **PR-1 → PR-2 → PR-3 → PR-4** (Doc [31](31_Impersonation_Implementation_Roadmap.md)), with activation (`SwitchUser` role grant) as a runtime decision after PR-3 soaks in UAT — never bundled into a PR.

## Context

FLMX is a multi-tenant ASP.NET Web Forms ERP (.NET Framework 4.5.2, ADO.NET, SQL Server) with:

- Custom session-based authentication: `Session["SessionToken"]` ↔ `ActiveSessions` re-validated per request; one live session per user, last login wins.
- Role/permission authorization through the single choke point `AuthGuard.HasPermission` (behind `SecurePage`, WebMethods, print gate, and menu).
- Existing multi-company context switching with membership checks against `UserCompanyAccess` (fail-closed).
- InProc sessions (20-min default), a 30-min DB-side idle timeout, and a client heartbeat (`Heartbeat.ashx`) — no out-of-process components.
- Mature audit obligations: financial transactions, per-transaction `tbl_SystemNotification` logging, compliance-grade attribution.

The superseded identity-swap design (forging an `ActiveSessions` row, swapping `Session["USERID"]`, and restoring an `Original*` snapshot on "switch back") cannot preserve attribution, fails under refresh/recycle/concurrency, and was classified a Critical defect. Docs [23](23_Impersonation_Architecture_Review.md)–[25](25_Impersonation_Session_Model.md) analyzed the lifecycle and session mechanics; Doc [26](26_Impersonation_STRIDE.md) threat-modeled it; Doc [27](27_Impersonation_Database_Review.md) specified the schema; Docs [28](28_Impersonation_MasterPage_UX.md)/[29](29_Impersonation_Implementation_Plan.md)/[30](30_Impersonation_Audit_Architecture.md) specified UI, components, and audit; Doc [31](31_Impersonation_Implementation_Roadmap.md) sequenced the PRs; Doc [32](32_Impersonation_Final_Architecture_Review.md) issued the **Go-with-changes** verdict; Doc [33](33_Impersonation_PR1_Implementation_Readiness.md) closed the open items (HR-1, HR-6, HR-3) and declared the identity/session model **Ready for PR-1**.

## Rationale

- **Attribution is structural, not procedural.** Because the principal is never mutated, every write is attributed to the acting admin by construction — there is no restore step that can fail or be skipped (INV-7).
- **The lease makes orphaned links impossible to use.** Binding the link row to `AdminSessionToken` means any principal death (logout, supersession, idle timeout, recycle) invalidates the link on the next request with zero background infrastructure.
- **Fail-closed economics.** All nine start gates, all probe duties, and the fork itself degrade to denial/no-link on error; availability may degrade, security never does.
- **Zero cost for 100% of current traffic.** Sessions without the pointer hit the fork's early return — byte-identical behavior, no new SQL (Doc 31 zero-SQL proof, merge-blocking in PR-2).
- **Race-safety at the engine level.** One live link per principal and one per target are enforced by DB predicates inside the `TryStart` transaction (INV-15), not by UI discipline.
- **Honest audit semantics.** Lazy closure timestamps record discovery, not occurrence; reconstruction queries therefore join the proven activity window (INV-14), keeping the audit correct in every failure mode.

## Core invariants (INV-13–INV-17)

These extend the lifecycle invariants INV-1…INV-12 of Doc [24](24_Impersonation_Lifecycle.md) and are normative for every implementation PR.

| # | Invariant |
|---|---|
| **INV-13** | Every per-request probe **re-derives the principal's effective permission set**. Loss of `SwitchUser`, or of any permission the target holds (whether by user-role removal or role-permission removal), forces an immediate close (`Revoked`). No stale-granted window exists. *(Supersedes Doc 25 §3 F5 Option B and §4 E4 "observation continues".)* |
| **INV-14** | `EndUtc` is the **closure/discovery** timestamp. The **proven activity window is `[StartedUtc, LastUsedUtc]`**. All attribution and reconstruction queries join the proven window — never `EndUtc`. The ≤120 s forced-rollback claim holds only with a live browser tab (heartbeat); all other failure modes close lazily while the row is lease-inert immediately. |
| **INV-15** | **Exactly one live link per principal and one per target**, enforced inside the `TryStart` transaction by DB predicates (unique filtered indexes) — never by UI logic. |
| **INV-16** | **Only the application's service contract writes `ImpersonationLink`/`SecurityAudit`.** v1 uses no Windows Service and no scheduled SQL-Agent sweeper: closure is probe-, heartbeat-, and lazy-sweep-driven only. Any future out-of-process component is a separate re-reviewed proposal. |
| **INV-17** | All identity joins key on the **numeric `Id`**; the business key `User_Id` is non-unique (non-unique `IDX_User_Id` on UAT) and must not appear in any join in this feature. |

Probe duties (Doc [33](33_Impersonation_PR1_Implementation_Readiness.md)), in order: lease → principal liveness (`PrincipalIneligible`) → permission re-derivation (INV-13 ⇒ `Revoked`) → target liveness (`TargetIneligible`) → 4-hour max duration (`TimedOut`) → tenant consistency (`TenantViolation`). SQL failure at any duty ⇒ fail-closed to `NORMAL_ADMIN`. Canonical `EndReason` set (11 values, pinned in the PR-1 DDL header): `UserEnded, Logout, Superseded, SessionExpired, IdleTimeout, Recycle, Revoked, TimedOut, TargetIneligible, PrincipalIneligible, TenantViolation`; denials are `ImpersonationDenied(cause)` events, never EndReasons.

## Non-goals

- **Not** a second authentication system; `TryValidateSession` and the login path are untouched (the target never receives a session, cookie, or `ActiveSessions` row — INV-3).
- **Not** session-variable-based state: InProc memory is volatile across recycles; the DB row is the only authority, the session holds a GUID pointer only (INV-12).
- **Not** granting elevated capability to the observed account: write attribution bars the admin from acting *as* the target; the superset rule (admin must hold every permission the target holds) is enforced at start and re-derived per request (INV-13).
- **Not** background-service-managed in v1 (INV-16): no service, no SQL Agent job, no out-of-process sweeper.
- **Not** an out-of-the-box activation: `SwitchUser` is granted to zero roles until the PR-3 soak gate; the kiosk realm (`tbl_card_login`) is permanently excluded.
- **Not** per-page read auditing: page-read visibility is window-bounded by design (documented residual risk, Doc 26/Doc 30 §11).

## Superseded assumptions

| Superseded assumption | Superseding decision | Evidence |
|---|---|---|
| Identity swap ("log in as the user") is a viable impersonation design | Rejected — Link Model adopted | Docs 23–24; legacy page write paths removed in PR-1 |
| Session variables are sufficient to carry impersonation state | DB-anchored lease + session pointer (INV-12) | InProc 20-min expiry, recycles (Doc 25 G1) |
| Forced rollback always completes within 120 s | Heartbeat-dependent window; proven-window attribution (INV-14) | Doc 32 HR-1, Doc 33 |
| Observation continues when the admin's permissions change mid-link | Per-request re-derivation forces close (INV-13) | Doc 32 HR-6, Doc 33 |
| A scheduled SQL-Agent sweeper closes expired links | Withdrawn for v1 — lazy/probe/heartbeat only (INV-16) | Doc 32 §5, Doc 33 |
| A Windows Service interacts with sessions and must be integrated | No service exists in-repo; interaction contracts are conditional-only | Doc 32 challenge #0 |
| `tbl_login.User_Id` is unique and safe as a join key | Numeric `Id` joins only (INV-17) | Non-unique `IDX_User_Id` verified on UAT |
| Tests will exist when implementation starts | Explicit decision: minimal unit-test project for pure predicates lands in PR-2; transition matrix is a documented manual UAT script until then | Doc 32 HR-3, Doc 33 |

## References

- [23_Impersonation_Architecture_Review.md](23_Impersonation_Architecture_Review.md) — feature architecture review (Link Model, eligibility gates, permission model)
- [24_Impersonation_Lifecycle.md](24_Impersonation_Lifecycle.md) — lifecycle safety analysis, invariants INV-1…INV-12
- [25_Impersonation_Session_Model.md](25_Impersonation_Session_Model.md) — session object model, lease, state machine, concurrency
- [26_Impersonation_STRIDE.md](26_Impersonation_STRIDE.md) — STRIDE threat model with residuals
- [27_Impersonation_Database_Review.md](27_Impersonation_Database_Review.md) — link/audit schema, migration, retention
- [28_Impersonation_MasterPage_UX.md](28_Impersonation_MasterPage_UX.md) — banner, search modal, rollback UX
- [29_Impersonation_Implementation_Plan.md](29_Impersonation_Implementation_Plan.md) — `SecurityContext`/`ImpersonationService`/`AuthGuard` fork spec
- [30_Impersonation_Audit_Architecture.md](30_Impersonation_Audit_Architecture.md) — event catalog, correlation, reporting queries
- [31_Impersonation_Implementation_Roadmap.md](31_Impersonation_Implementation_Roadmap.md) — PR-1 → PR-4 roadmap
- [32_Impersonation_Final_Architecture_Review.md](32_Impersonation_Final_Architecture_Review.md) — final review gate (Go with changes)
- [33_Impersonation_PR1_Implementation_Readiness.md](33_Impersonation_PR1_Implementation_Readiness.md) — PR-1 readiness gate (Ready); INV-13…17 formalized
