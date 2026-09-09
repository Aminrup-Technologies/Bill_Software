# 32 — Final Architecture Review: Administrator Impersonation × FLMX

| | |
|---|---|
| **Status** | **Final review gate — Go/No-Go over the completed design (docs 23–31)** |
| **Author role** | Principal Software Architect (ERP, ASP.NET Web Forms, .NET Framework, C#, SQL Server) |
| **Reviewed** | The Link-Model proposal: docs [23](23_Impersonation_Architecture_Review.md)–[31](31_Impersonation_Implementation_Roadmap.md), fitted to FLMX as verified in-repo |
| **Verdict** | **Go with changes** (see §1) |

> **Input caveat (challenge #0).** The review brief references an "Attached Windows Service Architecture Analysis." **No Windows Service exists in this repository.** Verified: the solution contains a single web project (`Visual Studio I2I INC Web Application.sln` → `Bill_Software.csproj` only); there is no `ServiceBase`, `System.ServiceProcess`, `InstallUtil`, `Topshelf`, or Quartz reference anywhere in the code. The only "service"-named artifacts are `db/pservice_snapshot.sql` (**P**rimary **Service** = a quotation field snapshot column, unrelated) and the in-process `CommunicationGateway` (SMTP/MSG91). If a service runs at the deployment tier, it is outside this repo and invisible to review. This review therefore evaluates Windows-Service *interaction* as: "what the design must guarantee if/when one exists," not "how to integrate with an existing one." That is the honest reading of the evidence.

---

## Executive Verdict — **GO WITH CHANGES**

The proposed Link Model fits FLMX **without weakening** authentication, authorization, tenant isolation, or auditability — *provided* five changes are treated as release blockers, not follow-ups:

1. **PR-1 executes first and repairs the build** — the committed `SwitchUser.aspx.cs` cannot compile against current source (it calls a three-argument `AuthGuard.HasPermission(...)` overload that does not exist in `AuthGuard.cs`). The legacy page's identity-swap write paths must be removed before anything else ships. This is not hygiene; the current trunk is not in a shippable state for this feature area.
2. **`UserCompanyAccess` DDL is applied to UAT before PR-3 activation.** Verified absent from `flamex_uat` ([03](03_Role_Permissions.md), [18](18_Phase2A_Tenant_Membership.md)); `AuthGuard` company membership currently fails closed. The design is correct *and* its tenant-isolation leg is presently unenforceable. Activation is gated on evidence, not intent.
3. **A test harness decision is made explicit.** The repository has **zero test infrastructure** (no test project, no test files). Docs 29/31 specify a "unit-testable pure function" (superset rule) and a transition/failure test matrix — against a harness that does not exist. Either stand up a minimal test project in PR-2 or re-classify the matrix as a documented manual UAT script. Do not leave the assumption implicit.
4. **No Windows Service is introduced for v1.** The design deliberately chose a lazy sweeper (request-time close) over an out-of-process component. Keep it that way. See §5.
5. **Activation discipline stands:** no PR grants `SwitchUser` to any role; the grant is a runtime decision after PR-3 soaks in UAT.

**Why not plain Go:** the design is sound, but two of its load-bearing facts (company-membership DDL, build integrity of the legacy page) are currently false in the repo, and one of its specified controls (automated tests) has no host. **Why not No-Go:** nothing in the design weakens the security foundation; every identified gap has a named, small, ordered fix (PR-1..4 in [31](31_Impersonation_Implementation_Roadmap.md)).

---

## Architecture Score

Scored as: *the proposed impersonation architecture, as fitted to the verified FLMX architecture* — not the aspiration, not the current trunk.

| Dimension | Score | Basis |
|---|---:|---|
| **Authentication** | **9** | `TryValidateSession` is never modified; no forged `ActiveSessions` rows (the legacy page's fatal move is deleted in PR-1); identity never swaps; lease rides the existing session token. One point held back: forced-close timing depends on a *client-driven* heartbeat (§4, HR-1). |
| **Authorization** | **9** | Exactly one choke point (`AuthGuard.HasPermission`) with a fail-closed fork; absent pointer ⇒ byte-identical behavior for 100% of current traffic; superset rule prevents escalation through the target. Held back: mid-link role revocation semantics must be pinned in the lease probe (§4, HR-6). |
| **Session Design** | **8** | Pointer + DB lease (`ImpersonationLink.AdminSessionToken`), single-use `IntentToken`, no nested impersonation, refresh-safe, logout-clean. Held back: exactly-once closure relies on lazy sweep when sessions die abruptly, so `End` audit timestamps are approximate, not instantaneous. |
| **Tenant Isolation** | **8** | Company fixed at link start; membership checks stay principal-scoped (INV-9); dropdown disabled while observing. Held back: `UserCompanyAccess` is not on UAT yet — the isolation leg is *designed* correct but *presently unenforceable* until DDL lands (change #2). |
| **Auditability** | **9** | Append-only `SecurityAudit` (DENY UPDATE/DELETE), exactly-once `Start`/`End` pair, three-key correlation (principal → session → link), financial window-join reconstruction. Held back: reads are window-bounded, not per-page logged — accepted, must appear in the security notice (doc 30 §11). |
| **Scalability** | **8** | O(1) indexed probe per request while observing; zero overhead for normal sessions; throttled `LastUsedUtc`; no out-of-process dependency. Ample for an internal ERP of this size; nothing here is a scale risk. |
| **Maintainability** | **7** | Minimal-diff fork on one method; inert-until-granted schema; four small PRs with rollback plans. Held back: zero test harness (change #3), the dead legacy page must be excised not patched, and the fork lives in a CODEOWNERS-protected asset — future maintainers need the doc-29 fork table kept current or the design will drift. |
| **Overall** | **8.3 / 10** | Go with changes. |

---

## Review Areas 1–10: Findings

### 1. Authentication lifecycle compatibility — PASS
Verified against code: `Session["UserDbId"]` already exists at login (`AuthGuard.ResolveUserDbId` reads it), so the design needs **zero login-path changes**. The request lifecycle (login → `ActiveSessions` row → per-request `TryValidateSession` → page permission) is untouched. The impersonation pointer is additive session state that the validator ignores. Safe insertion points and forbidden interception points were mapped in [24](24_Impersonation_Lifecycle.md) and are consistent with the code.

### 2. Session architecture — PASS with one honesty correction
The pointer-plus-lease model survives refresh, concurrent tabs (single-browser chain, doc 25 §7), and logout. **Correction to the marketing claim:** "forced rollback within 120 s" is only true while at least one browser tab is open. A closed laptop, killed tab, or crashed browser stops the heartbeat; the lease then expires and closure happens *lazily on the next request or sweeper pass*, which may be minutes or hours later. The audit record remains correct (exactly-once, with `EndReason`), but the *enforcement window* is not 120 s in all failure modes. State this in the security notice.

### 3. Authorization integrity — PASS
The fork is confined to `HasPermission`; `SecurePage`, `EnsureWebMethod`, `EnsurePrint`, and the menu inherit it without modification. The superset rule (admin must hold every permission the target holds) is checked at `TryStart` and makes write-through-observation non-escalating. Two pinned requirements: (a) the auth leg and company-membership leg are **never** forked — they stay principal-scoped; (b) the lease probe must re-derive the principal's current permission state, so a role revoked *during* an observation degrades to fail-closed, not to stale-granted (HR-6).

### 4. Multi-company isolation — PASS, prerequisite unmet
The link freezes the tenant context at start (`TargetCompanyId`), disables company switching while observing, and keeps membership checks principal-scoped so an admin cannot observe a target in a company the *admin* is not a member of. This is architecturally correct. **But** the membership leg reads `UserCompanyAccess`, which does not exist on UAT — until the DDL is applied, `AuthGuard` fails closed on company membership for everyone. The feature cannot activate in that state (change #2). Also verified: `tbl_login.User_Id` is **not unique** (non-unique `IDX_User_Id` on UAT); all identity keys in the design are the numeric `Id` — any implementation PR that introduces a `User_Id` join anywhere in this feature must be rejected at review.

### 5. Windows Service interaction — NONE REQUIRED (and none exists)
This is the area the brief over-weights. FLMX is a single-AppDomain web application under the Ponytail philosophy; all "background" semantics are client-driven (`Heartbeat.ashx` poll, `SessionKeepAlive`) plus request-time lazy processing. The Link Model was designed service-free on purpose: lease expiry is enforced at *read* time (lease check on the per-request probe), not by a timer. The architectural guarantees this creates, which any future service must preserve:

- **No out-of-process component may mutate `ImpersonationLink` or `SecurityAudit` state directly.** Closure, supersession, and lease transitions go through the application's service contract only (doc 29). A future service that "helpfully" expires links in bulk would break exactly-once audit semantics.
- **Service-initiated writes are outside impersonation attribution — by design.** A service has no `Session`; its writes are attributed to the service identity. The doc-30 window-join must **not** be extended to claim service writes on behalf of an observing admin. Documented non-feature, not a gap.
- If the referenced "Windows Service Architecture Analysis" describes a *planned* service (sweeper, scheduled exports), it is out of scope for v1 and should be re-reviewed as its own proposal when it exists in-repo.

### 6. Audit trail consistency — PASS
`SecurityAudit` is append-only with no FKs and DENY UPDATE/DELETE; `Start`/`End` are an exactly-once pair across all nine closers (voluntary, forced, logout, session death, target ineligibility, …). `tbl_SystemNotification` remains the business-transactional log per the Ponytail rule; `SecurityAudit` is the security ledger — doc 30 defines the reconciliation control between them. Residual, accepted: DBA-level direct table edits bypass the ledger (append-only mitigates tampering, not administrative side channels); this belongs in the operational runbook.

### 7. Security boundaries — PASS
Boundaries hold: ERP auth vs. kiosk (`tbl_card_login`) is untouched and excluded; `machineKey` rotation (a documented boundary) is irrelevant here because session validity is DB-validated, not cookie-derived; `CompanyID` is never client-trusted; the kiosk and kiosk sessions can never be impersonation targets or actors. STRIDE coverage (doc 26) is complete with residual risks named.

### 8. Performance impact — PASS
One indexed read per request **while observing only** (covering probe on `AdminSessionToken`); zero for the 100% of traffic that never impersonates; throttled `LastUsedUtc` write (bounded staleness, documented); sweeper is lazy. The heartbeat cadence is unchanged. No connection-pool or lock-profile concerns: `ImpersonationLink` rows are short, hot on narrow unique filtered indexes only.

### 9. Failure recovery — PASS with named residuals
Every failure mode has a defined path: target disabled/deleted mid-link → forced close with `EndReason`; admin session death → lazy close on lease expiry; DB unavailable mid-link → fail-closed (no probe, no page); double-click/duplicate intent → single-use token; concurrent link attempts → unique filtered indexes make one-link-per-principal race-safe at the engine level, not by application discipline. Residual: `End` timestamps in lazy closes record *when the lie was discovered*, not when it occurred — accepted and covered by the lease window in reconstruction queries.

### 10. Backward compatibility — PASS
Purely additive: two new tables, two seeded permissions, no changes to existing tables, indexes, procs, or the login path. New code fails closed against an old DB; old code is unaffected by the new schema. The single compatibility landmine is the non-compiling legacy page (change #1) — that is a current defect, not a compatibility risk introduced by this design.

---

## Hidden Risks

| # | Risk | Severity | Why it hides | Mitigation / owner |
|---|------|----------|--------------|--------------------|
| HR-1 | **Forced-rollback SLA is heartbeat-dependent.** "≤120 s" holds only with a live tab; laptop-close/crash defers closure to lazy sweep. | Medium | The design docs state the 120 s number without the failure-mode caveat. | Amend doc 25/30 with the honest window; add to the security notice. |
| HR-2 | **The trunk does not compile in the feature area.** Legacy `SwitchUser.aspx.cs` references a non-existent `AuthGuard.HasPermission` overload. | High | The committed `Output-Build.txt` shows a successful build — from a *developer machine state that is not in git*, masking the breakage. | PR-1 (change #1). CI-less repo means compile evidence must be attached to each PR. |
| HR-3 | **Zero test infrastructure** while the spec leans on unit-testable gates and a transition matrix. | Medium | `detected_test_files: 0`; the .sln has one project. Tests specified in doc 29 have no host. | Decide in PR-2: minimal test project vs. re-classified manual UAT script. Make the choice explicit in the PRD. |
| HR-4 | **`UserCompanyAccess` absent on UAT** — tenant-isolation leg fails closed globally today. | High (for activation) | Easy to forget because the *web* works: the failure is silent for admins who are also implicitly authorized elsewhere. | Hard gate before PR-3 activation; evidence = DDL applied + membership probe green for a test matrix. |
| HR-5 | **Non-unique `User_Id` business key** — any helper code that joins by `User_Id` instead of `Id` can silently match multiple rows under impersonation's increased join surface. | Medium | Pre-existing latent defect; impersonation merely raises the traffic on identity joins. | Feature code keys on numeric `Id` only (already specified); add a review checklist line rejecting `User_Id` joins. |
| HR-6 | **Mid-link role/permission changes** — admin's roles revoked *during* an observation. | Medium | The superset rule is enforced at `TryStart`; nothing enforces it at minute 40 unless the lease probe re-derives. | Pin the invariant in doc 29: lease probe re-checks principal permission; revocation ⇒ forced close (`PrincipalIneligible`). |
| HR-7 | **Dual-audit divergence** — `tbl_SystemNotification` (business) vs `SecurityAudit` (security) can drift if a future PR writes one and not the other. | Low | Two ledgers with different owners and lifecycles. | Doc-30 reconciliation query Q-series scheduled as a periodic control-health check. |
| HR-8 | **Out-of-repo deployment components** (referenced Windows Service analysis, DBA jobs, SQL Agent schedules) are invisible to this review; procs are catalogued by signature only, bodies absent. | Medium | Anything that writes to impersonation tables outside the app's service contract escapes audit semantics. | Operational runbook: enumerate all writers of `ImpersonationLink`/`SecurityAudit`; assert only the app service contract writes them. |
| HR-9 | **Fork drift** — future `AuthGuard` edits that bypass or reorder the impersonation fork. | Medium | The fork lives inside a protected asset; CODEOWNERS review is the only structural defense. | Keep doc 29's fork table normative; CODEOWNERS review must diff against it. |

---

## Recommended Architectural Changes

1. **Execute PR-1 as a build-repair release blocker** — neutralize the legacy page (denial-only), remove its forged-session and identity-swap write paths, restore compilation. Everything else sequences behind it ([31](31_Impersonation_Implementation_Roadmap.md) already orders it first; this review elevates it from "step 1" to "gate").
2. **Keep the design service-free for v1** — reaffirm lazy sweep; codify the two service-interaction contracts in §5 into the operational runbook.
3. **Pin HR-6 in the implementation spec** — one paragraph in doc 29: the lease probe re-derives principal permission; any mid-link demotion forces closure. This is the only underspecified transition found in the whole series.
4. **Correct the forced-close claim** — doc 25/30 amended with the heartbeat-dependent enforcement window (HR-1); the security notice carries it.
5. **Make the test-harness decision explicit** (HR-3) — minimal test project in PR-2 or a formal manual UAT script; no silent assumption.
6. **Hold the activation gate** — `SwitchUser` is granted to zero roles until: DDL applied (HR-4), PR-3 soaked in UAT, and the reconciliation control (HR-7) is scheduled.
7. **Add the `User_Id` review rule** (HR-5) — one checklist line in the PR template for this feature: numeric `Id` keys only.

No architectural *redesign* is recommended. The Link Model, the single-choke-point fork, the inert schema, and the PR sequence all stand.

---

## Implementation Prerequisites

| # | Prerequisite | Evidence required | Blocks |
|---|--------------|-------------------|--------|
| P-1 | PR-1 merged (legacy page neutralized, inert DDL: `ImpersonationLink` + `SecurityAudit`) | Compile log from a clean checkout; zero-SQL proof (merge-blocking) | All |
| P-2 | `UserCompanyAccess` DDL applied to UAT | Membership probe green for the PR-3 test matrix | PR-3 activation |
| P-3 | Test-harness decision recorded | Test project in .sln **or** approved manual UAT script in `docs/` | PR-2 merge |
| P-4 | Doc 29 amended (HR-6 invariant) + docs 25/30 amended (HR-1 window) | One PR, docs only | PR-2 merge |
| P-5 | CODEOWNERS review slots booked for `AuthGuard.cs` (PR-2) and `Bill.Master` (PR-3), with docs 23–32 as the review corpus | Reviewer assignment | PR-2 / PR-3 |
| P-6 | Operational runbook section: enumerated writers of the two new tables; DBA retention jobs; reconciliation control schedule | Runbook page | PR-3 activation |
| P-7 | Activation decision: grant `SwitchUser` post-soak | UAT soak report | Production release |

---

## Assumption Challenge Table

| Proposal claim | Verdict | Evidence |
|---|---|---|
| "Existing authentication can host impersonation without login changes." | **Verified** | `Session["UserDbId"]` exists at login; `TryValidateSession` untouched by design. |
| "Authorization has a single fork point." | **Verified** | `AuthGuard.HasPermission` sits behind `SecurePage`, `EnsureWebMethod`, `EnsurePrint`, menu. |
| "Forced rollback completes within 120 s." | **Challenged — qualified** | True only with a live browser tab (HR-1); lazy sweep otherwise. |
| "Company isolation is enforced by membership checks." | **Verified but currently unenforceable** | `UserCompanyAccess` absent from UAT (HR-4). |
| "A Windows Service interacts with sessions and must be considered." | **Rejected as stated** | No service exists in-repo (challenge #0); interaction contracts specified conditionally (§5). |
| "Tests will verify the superset rule and transitions." | **Challenged — unresolved** | No test infrastructure exists (HR-3); decision required. |
| "Schema changes are purely additive and backward compatible." | **Verified** | Two new tables + two seed permissions; no existing object touched. |
| "Audit is exactly-once and tamper-evident." | **Verified with residuals** | Append-only + unique constraints; residuals: lazy-close timestamps, DBA side channel (§6, §9). |

---

## Series Traceability

Docs 23–31 stand as authored; this document amends two claims (HR-1, HR-6), elevates PR-1 to a release-blocker gate, and adds prerequisites P-3, P-6, P-7. The execution path remains [31](31_Impersonation_Implementation_Roadmap.md) PR-1 → PR-4 with the gates in §Implementation Prerequisites above.
