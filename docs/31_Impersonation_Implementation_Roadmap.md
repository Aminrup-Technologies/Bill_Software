# 31 — Impersonation Implementation Roadmap (PR-1 → PR-4)

| | |
|---|---|
| **Status** | **Production-ready engineering roadmap** |
| **Author role** | Delivery / Implementation Architecture |
| **Synthesizes** | [`23`](23_Impersonation_Architecture_Review.md) (phases), [`25`](25_Impersonation_Session_Model.md) (lease/timeouts), [`27`](27_Impersonation_Database_Review.md) (migration §9), [`28`](28_Impersonation_MasterPage_UX.md) (UI), [`29`](29_Impersonation_Implementation_Plan.md) (components, §0 ground truths, §8 test matrix), [`30`](30_Impersonation_Audit_Architecture.md) (events/reports) |
| **Process frame** | [`CONTRIBUTING.md`](../CONTRIBUTING.md): four gates (**Security / Data / Regression / Scope**) per PR, merge commits, protected assets (`AuthGuard.cs`, `SecurePage.cs`, `UserCompanyAccess.sql`, `docs/22`) require architectural review — docs 23–30 **are** that review and must be referenced in each PR description |
| **Environment order** | Every PR: build → `flamex_uat` → soak → `flamex_live` (doc 27 §9.2) |

---

## 0. Constraints → Mechanism Map (how each PR honors the three invariants)

| Constraint | Mechanism (verified in docs 27/29) | PRs affected |
|---|---|---|
| **Authentication stays backward compatible** | `AuthGuard.TryValidateSession` is never modified (doc 29 G5). No forged `ActiveSessions` rows; the target never gets a session | 1–4 |
| **Authorization keeps working** | The `HasPermission` fork activates **only** when `Session["ImpersonationToken"]` exists — absent pointer ⇒ early return ⇒ byte-identical behavior for 100% of current traffic. `SecurePage.cs` untouched | 2 (fork), 3 (consumers) |
| **Company switching must not regress** | `ddlCompany`/`UserCompanyAccess` logic unchanged; membership checks remain **principal-scoped** (INV-9). While observing, the dropdown renders disabled — a UI state, not a code path change; any company-switch attempt mid-observation fails the lease/tenant check and closes the link (`TargetIneligible` path, doc 25 T8) | 3 (render only) |

**Sequencing logic:** PR-1 makes the tree safe and lays inert DB objects; PR-2 adds dormant server capability; PR-3 is the only user-visible PR; PR-4 adds oversight. No PR grants `SwitchUser` to any role — **activation is a runtime admin decision after PR-3 soaks in UAT** (doc 27 §9.2 step 3).

---

## PR-1 — Neutralize Legacy SwitchUser + Inert Database Foundation

**Maps to:** doc 23 Phase 0 + doc 27 §9 migration. Ships independently and immediately.

### Scope

1. **Neutralize the legacy identity-swap page.** Verified fact (doc 29 G3): `SwitchUser.aspx.cs` calls a three-argument `AuthGuard.HasPermission(...)` overload that does not exist — the page **cannot compile against current source**; it also implements the Critical defect set (forged `ActiveSessions` row, session swap, snapshot rollback, `catch { }` audit). This PR: fix the compile by calling the existing single-argument overload; **remove the write paths** (`btnSwitch_Click` / `btnSwitchBack_Click` bodies) and the `Original*` session-snapshot logic; replace with fail-closed denial labels ("Switch User is not enabled"). Keep the page shell, search grid, and permission seed — the PR-3 rewrite will reuse the shell.
2. **Land `db/impersonation_foundation.sql`** per doc 27 §9: `ImpersonationLink` + indexes/FKs, `SecurityAudit` + indexes + `DENY UPDATE, DELETE` to the app login, permission seed rows (`SwitchUser` ensured, `ImpersonationAudit` inserted — **no role grants**), `UserCompanyAccess`-presence verification step, and a clearly marked reviewed-rollback section (drop tables + seed rows). House style per [`db/pservice_snapshot.sql`](../db/pservice_snapshot.sql): `When/Why/What` header, "DO NOT execute until reviewed", `IF NOT EXISTS` idempotency.
3. **Evidence gate (pre-merge):** run in **both** `flamex_uat` and `flamex_live`: the catalog query proving **no role holds `SwitchUser`** (doc 23 Phase 0.4). If any environment shows a grant, revoke before merge and record it in the PR.

### Files likely affected

| File | Change |
|---|---|
| `Bill_Software/corporate/business/app/SwitchUser.aspx.cs` | Write paths removed; permission call fixed; denial UI wiring |
| `Bill_Software/corporate/business/app/SwitchUser.aspx` | At most: banner panel (`pnlImpersonating`) removed/hidden — no new markup |
| `db/impersonation_foundation.sql` | **New** |
| `docs/SOLUTION_INDEX.md` / `docs/22_Security_Baseline.md` | Baseline gains the one-paragraph impersonation note (read/write invariant + "identity swap is a defect") — satisfies doc 23 Go-condition 5 early |

**Database changes:** the foundation script (above). Inert: nothing in the running app reads or writes the new objects after this PR.

### Validation checklist

- [ ] Solution builds clean (this PR is expected to *repair* the build — record before/after build evidence)
- [ ] `SwitchUser.aspx` reachable by a permitted test account (temp grant on UAT only, then revoke) shows denial state; **no** `ActiveSessions` INSERT occurs (SQL Profiler/Extended Events evidence)
- [ ] Four gates: **Security** — write paths gone, evidence query output attached; **Data** — script reviewed, idempotent, re-runnable; **Regression** — zero behavior change for users without `SwitchUser`; **Scope** — no other page touched
- [ ] Script executed on `flamex_uat`; objects + DENY grants verified; script **not** executed on live until review sign-off
- [ ] `UserCompanyAccess` DDL presence check executed (gate for PR-2, recorded here)

### Rollback plan

- Code: revert the commit (page returns to its dead/uncompilable state — never shipped behavior).
- DB: run the script's marked rollback section (drop the two tables + delete the two seed rows). Zero application impact — nothing references the objects.

### Regression risks

| Risk | Mitigation |
|---|---|
| Environments where `SwitchUser` *was* granted historically (page was live) | Evidence gate (§Scope 3) — revoke before merge |
| csproj/SQL script review friction | Script is additive-only; page diff is deletions + one overload fix |
| Accidental behavior change for non-privileged users | None possible — permission unheld ⇒ page unreachable (unchanged) |

---

## PR-2 — Phase 1 Server Foundation: SecurityContext + ImpersonationService + AuthGuard Fork

**Maps to:** doc 23 Phase 1, doc 29 §2–§4. Fully dormant: no UI, no grants, no behavior change without a link.

### Scope

1. **`SecurityContext.cs`** (new): per-request resolver exactly per doc 29 §2 — memoized in `HttpContext.Items`, lease + principal binding + target-liveness in one covering-index query, lazy close on liveness failure, fail-closed on SQL error. Prohibited patterns (doc 29 §2.4) enforced in code review.
2. **`ImpersonationService.cs`** (new): `TryStart` (9 ordered fail-closed gates incl. superset rule as a pure, unit-testable function; single transaction for link + audit), `TryEnd` (idempotent exactly-once pair), `TryClose` (forced closures), stale-link reclamation (doc 27 §3.1).
3. **`AuthGuard.cs` fork** (protected asset — the minimal diff enumerated in doc 29 §4): `HasPermission` evaluates the target's grants only while a link is active; `CurrentUserId()` returns the acting identity. Everything else untouched. **No method bodies change when no link exists.**
4. Unit-test project addition (or documented test harness) for the superset rule and eligibility predicates (doc 23 Phase 1.5) — pure functions, no DB required.

### Files likely affected

| File | Change |
|---|---|
| `Bill_Software/corporate/business/app/SecurityContext.cs` | **New** |
| `Bill_Software/corporate/business/app/ImpersonationService.cs` | **New** |
| `Bill_Software/corporate/business/app/AuthGuard.cs` | Fork in `HasPermission` + `CurrentUserId` only; header comment linking docs 23–30 (CODEOWNERS review) |
| `Bill_Software/Bill_Software.csproj` | `<Compile Include>` entries for the two new files |

**Database changes:** none in this PR. **Deployment dependency:** `db/impersonation_foundation.sql` must be applied to an environment before/with this PR's deploy — the service fails closed if tables are missing (doc 27 §10), so a missed step degrades to "feature unavailable", never to misbehavior.

### Validation checklist

- [ ] Build clean; csproj entries correct (missing-entry verification: new files compile in a fresh clone)
- [ ] **Zero-SQL proof:** with no `ImpersonationToken` in session, a traced request executes **no** new queries vs pre-PR baseline (the fork's early return)
- [ ] Superset/eligibility unit tests green, including fail-closed-on-error cases (doc 29 §8 rows 1–2)
- [ ] Table-driven `TryStart` concurrency tests on UAT: double-submit (`intent_used`), two admins one target (`target_in_use`) (doc 29 §8 row 3)
- [ ] Four gates, with **Security** explicitly referencing docs 23/26 threat IDs closed by this PR (T1, T2b, T4, T7, T9)

### Rollback plan

Revert the commit. The fork degrades to no-link behavior instantly; DB objects remain (harmless, unpopulated). No data cleanup required.

### Regression risks

| Risk | Mitigation |
|---|---|
| `AuthGuard` is the hottest path in the app (protected asset) | Diff limited to two methods + early return; the zero-SQL proof is a merge-blocking check |
| Fork accidentally consulted for *starting* impersonation | Gate 2 of `TryStart` evaluates the **principal's** permission explicitly (doc 29 §3.1) — tested |
| Company-membership drift | Membership checks untouched (principal-scoped); covered by constraint map §0 |

---

## PR-3 — Phase 2 User-Facing Integration: Master, Heartbeat, SwitchUser Rewrite

**Maps to:** doc 23 Phase 2, doc 28 (all), doc 29 §5–§7. The only PR that changes what users see. Still inert until a runtime grant.

### Scope

1. **`Bill.Master`**: impersonation banner row (flex stack between header and `.app-content`, doc 28 §4), company dropdown disabled + tooltip while observing, `UpdateSwitchUserVisibility` gains `!IsImpersonating`, **logout ordering** (end link before session teardown; combined confirm dialog per doc 28 §5.4), banner styling in the master's existing style block.
2. **`Heartbeat.ashx.cs`**: additive `linkClosed` field on the `status:"ok"` response after max-duration/target-liveness checks; `ImpersonationService.TryClose` on failure (doc 29 §6). Existing `logout` reasons/copy untouched.
3. **`SwitchUser.aspx` rewrite**: thin controller over `ImpersonationService` implementing doc 28 §3 — search modal states A–D, per-row eligibility badges, mandatory reason + attribution acknowledgement, intent-token consumption, denial rendering. No direct link/audit SQL in the page.
4. Heartbeat client snippet in master gains the passive banner-dismissal + toast on `linkClosed` (doc 28 §5.3) — additive JSON field only.

### Files likely affected

| File | Change |
|---|---|
| `Bill_Software/corporate/business/app/Bill.Master` | Banner markup/style, dropdown state, trigger state |
| `Bill_Software/corporate/business/app/Bill.Master.cs` | Ambient `SecurityContext` resolution, banner data binding, `UpdateSwitchUserVisibility` condition, logout order |
| `Bill_Software/corporate/business/app/Heartbeat.ashx.cs` | Additive liveness check + `linkClosed` |
| `Bill_Software/corporate/business/app/SwitchUser.aspx[.cs][.designer.cs]` | Full rewrite (shell retained from PR-1) |

**Database changes:** none.

### Validation checklist

- [ ] **Layout regression sweep:** all 19 menu domains load with the banner active *and* inactive; flex stack intact at 1920/1366/1024/640 widths (doc 28 §6)
- [ ] Banner states: appears only for the observing admin; target and other users see nothing; header identity remains the admin (doc 28 §7 matrix)
- [ ] Heartbeat: `status:"ok"` consumers unaffected; forced-closure reasons (`TimedOut`, `TargetIneligible`) each produce `linkClosed` + self-removing banner + toast
- [ ] Logout-while-observing: link closes `Logout` + audit before session teardown (doc 25 §6 order), existing non-impersonation logout byte-identical
- [ ] Company switching: pre/post PR, a normal admin switches companies as before (constraint §0); while observing, dropdown disabled and a forced company switch closes the link
- [ ] Full doc 29 §8 rows for fork/lease/forced-closure executed on UAT
- [ ] Soak: feature **not granted** in UAT until this PR soaks ≥ 1 week; pilot grant then end-to-end smoke (doc 28 §9 flows)

### Rollback plan

Revert the commit → PR-2's dormant capability remains, invisible to users. Any links in `ImpersonationLink` become unreachable (lease requires the master/heartbeat integration to matter) and the sweeper/lazy logic closes them `SessionExpired`. Optionally revoke the pilot grant first (pure data action).

### Regression risks

| Risk | Mitigation |
|---|---|
| Master touches **every page** — markup/layout breakage | Banner is in-flow (no z-index/overlay); sweep checklist above; GlobalNotification slot untouched (doc 28 G9) |
| Heartbeat JSON contract break | Additive field only; existing keys/reasons byte-identical; tested |
| Logout ordering change affects error paths | Lease-first design: even a failed link-end cannot leave a usable link (doc 25 §6.2); tested |
| Menu re-render under observation confuses users | Expected behavior per doc 28 §7 note 2 — banner explains context; include in soak feedback |
| `ddlCompany` postback while disabled | Disabled control cannot post back; re-enabled state re-tested |

---

## PR-4 — Phase 3 Oversight: Audit Viewer + Target Transparency

**Maps to:** doc 23 §6.3/§6.4, doc 30 §10–§11. After the mechanism has soaked; adds visibility, not capability.

### Scope

1. **`ImpersonationAudit.aspx`** (new, `SecurePage`-derived, `RequiredPermissionKey = "ImpersonationAudit"`): read-only viewer implementing doc 30 §10 reports Q1–Q3/Q5/Q6 with mandatory date bounds + paging; `ActorUser_Id` and `TargetUser_Id` always shown together; live-links panel (optional, Q8-derived).
2. **Menu item** in the Users dropdown, permission-gated (SoD: distinct from `SwitchUser`).
3. **Target transparency notice** (doc 23 §6.4): on next real login, a non-blocking `tbl_SystemNotification` notice listing recent observation windows targeting that user — rate-limited to once per login, additive post-login check in `index.aspx.cs`.
4. **Integrity alarm feed** (doc 30 Q7/Q8) as a maintenance query documented for ops (not a page).

### Files likely affected

| File | Change |
|---|---|
| `Bill_Software/corporate/business/app/ImpersonationAudit.aspx[.cs][.designer.cs]` | **New** |
| `Bill_Software/corporate/business/app/Bill.Master` | One permission-gated `<li>` |
| `Bill_Software/Bill_Software.csproj` | Compile/Content entries |
| `Bill_Software/index.aspx.cs` | Additive post-login transparency hook (guard-failed silently if `SecurityAudit` absent) |
| `docs/30_Impersonation_Audit_Architecture.md` | Mark §10 queries as implemented by the viewer |

**Database changes:** none (viewer reads only; notices reuse `tbl_SystemNotification`).

### Validation checklist

- [ ] Viewer reachable only with `ImpersonationAudit`; 403 otherwise; SoD verified (holder of `SwitchUser` alone cannot see it)
- [ ] All queries date-bounded + paged; no unbounded `SecurityAudit` scan possible from the UI
- [ ] Transparency notice fires once per login, only for observed users, and never blocks login (try/catch with server-side log)
- [ ] Four gates; **Security** notes the viewer is read-only and grant-gated; **Data** notes zero DDL

### Rollback plan

Revert the commit; remove the `ImpersonationAudit` grant (data action). Audit data persists untouched. The transparency hook fails silently to off.

### Regression risks

| Risk | Mitigation |
|---|---|
| Login-path regression (transparency hook in `index.aspx.cs`) | Strictly additive, exception-guarded, zero effect when no rows match; tested for non-observed users |
| Viewer performance on growing `SecurityAudit` | Index-aligned queries (doc 30 §10) + enforced bounds; projected < 1 M rows at 7 years (doc 27 §7.1) |
| Notice fatigue for frequently-observed users | One-notice-per-login cap; review after soak |

---

## 1. Roadmap Summary

| PR | Theme | User-visible? | DB objects | Highest risk |
|---|---|---|---|---|
| **PR-1** | Neutralize legacy + inert foundation | No (denial page only) | Creates both tables (inert) | Historical grant evidence |
| **PR-2** | Dormant server capability | No | None | `AuthGuard` fork (zero-SQL proof) |
| **PR-3** | The feature | **Yes** | None | Master-wide layout/behavior |
| **PR-4** | Oversight & transparency | Yes (auditors/targets) | None | Login-path hook |

**Program-level controls:**

1. **Activation discipline:** `SwitchUser` is granted only after PR-3 soaks in UAT; grant to a pilot role first; `flamex_live` activation is a change-record decision, never bundled into a PR.
2. **Gate dependency chain:** PR-1's `UserCompanyAccess` presence check is a **hard prerequisite** for PR-3 activation (target eligibility fails closed without it — doc 18/doc 27 G5).
3. **Protected-asset reviews:** PR-2 (`AuthGuard.cs`) and PR-3 (`Bill.Master`) each cite docs 23–31 in the PR description; CODEOWNERS reviewer sign-off recorded.
4. **Continuous integrity:** from PR-3 onward, doc 30 Q7/Q8 reconciliation queries run on the ops cadence; denial analytics (Q5) reviewed weekly during the pilot.
5. **Definition of done (program):** all doc 29 §8 test rows green on UAT, live soak complete, baseline `docs/22` impersonation section merged (PR-1), audit viewer live (PR-4), and the security notice for end users drafted (doc 30 §11 known-gaps list).

---

## 2. Requirement Traceability

| Requested item | Where |
|---|---|
| Backward-compatible authentication | §0 row 1; PR-2 validation (zero-SQL proof, `TryValidateSession` untouched) |
| Existing authorization continues | §0 row 2; PR-2 fork design + tests |
| Company switching no-regress | §0 row 3; PR-3 checklist |
| Small PRs, per-PR scope/files/DB/validation/rollback/regressions | PR-1…PR-4 sections; summary table §1 |
| Production-ready process | CONTRIBUTING gates, environment order, activation discipline (§1 controls) |

---

*End of roadmap. No code was written or modified in producing this document; each PR is specified for implementation against docs 27–30.*
