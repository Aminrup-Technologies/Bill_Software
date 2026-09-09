# 29 — Implementation Specification: ImpersonationService & SecurityContext (Phases 1–2)

| | |
|---|---|
| **Status** | **Design — server-component implementation specification (contracts, not code)** |
| **Author role** | Implementation Architecture |
| **Depends on** | [`23`](23_Impersonation_Architecture_Review.md) (phasing, §4/§6/§7), [`25`](25_Impersonation_Session_Model.md) (L1–L6, lease, transitions T1–T11), [`26`](26_Impersonation_STRIDE.md) (denial taxonomy), [`27`](27_Impersonation_Database_Review.md) (tables, indexes, transaction boundaries), [`28`](28_Impersonation_MasterPage_UX.md) (UI contracts consumed here) |
| **Grounded against** | `AuthGuard.cs`, `SecurePage.cs`, `Heartbeat.ashx.cs`, `SwitchUser.aspx.cs`, `Bill.Master[.cs]` (all read in full for this spec) |
| **Scope** | Component inventory, `SecurityContext` resolver, `ImpersonationService` acts, `AuthGuard` fork points, master/heartbeat integration, legacy page disposition, test matrix, rollout order |
| **Constraint** | Method **contracts and behavior** only — method bodies are authored at implementation time. SQL shown is specification (as in docs 23/27), not deliverable code. |

---

## 0. Verified Ground Truths — current code

Read directly from source this pass; these override any assumption inherited from earlier docs.

| # | Fact | Source | Consequence |
|---|------|--------|-------------|
| G1 | `Session["UserDbId"]` is **already written at login**; `AuthGuard.ResolveUserDbId(ctx)` reads it and fails closed to `0` | `AuthGuard.cs` (`ResolveUserDbId`) | Doc 23 §4.3's "close the admin business-key ↔ db-id gap" item is **already satisfied** — no login change required. Use the existing key name `UserDbId`. |
| G2 | `AuthGuard.HasPermission(string permissionKey)` is the only overload: joins `Permissions → RolePermissions → UserRoles → tbl_login` on `u.User_Id = @UserId AND u.CompanyID = @CompanyID`, where `@UserId` is `Session["USERID"]` | `AuthGuard.cs` | The fork target exists and is single: one method to fork for the acting identity. No three-argument overload exists anywhere. |
| G3 | The committed `SwitchUser.aspx.cs` calls `AuthGuard.HasPermission(Session["USERID"].ToString(), "SwitchUser", CompanyContext.CurrentCompanyID)` — a signature that **does not exist** | `SwitchUser.aspx.cs` vs `AuthGuard.cs` | The legacy page **does not compile against current source**. It is dead code today; Phase 0 disposition (§7) is formalized around this fact. |
| G4 | `SecurePage.OnInit` → `AuthGuard.EnsurePage` / `EnsurePageAny`; `EnsureWebMethod` / `EnsureWebMethodPermission` guard WebMethods; `EnsurePrint` guards print pages | `SecurePage.cs`, `AuthGuard.cs` | Exactly four gate methods are the authorization choke points; the fork lives in `AuthGuard` behind them — `SecurePage` itself needs **zero changes**. |
| G5 | `TryValidateSession` checks `ActiveSessions.IsActive` by `Session["SessionToken"]` and clears/abandons the session on failure | `AuthGuard.cs` | Unchanged by this feature (INV: authentication leg is never forked). The link lease piggybacks on the same token. |
| G6 | Heartbeat: JSON `{"status":"ok"}` / `{"status":"logout","reason":"superseded"|"timeout"|…}`; validates principal only; updates `LastHeartbeat`; 30-min idle kill | `Heartbeat.ashx.cs` | Additive `linkClosed` field rides the `status:"ok"` response (§6); existing reasons and copy are untouched. |
| G7 | Legacy page behavior confirmed exactly as doc 23 charged it: deactivates the admin's real `ActiveSessions` row, INSERTs a forged row for the target, swaps `Session["USERID"]`/`RoleId`/`CompanyID`, snapshots `Original*`, switch-back replays snapshots, audit = `tbl_SystemNotification` insert inside `catch { }` | `SwitchUser.aspx.cs` | Phase 0 (§7) removes the write paths; the Rewrite (Phase 2) replaces the page as a thin controller. |
| G8 | `Bill.Master.cs` runs per-request session validation + company context + menu permission resolution; `UpdateSwitchUserVisibility` toggles `pnlSwitchUser` | master (doc 25 G6, doc 28 G6) | One injection point for `SecurityContext` ambient resolution + banner (§5). |
| G9 | All data access is synchronous ADO.NET with `using` blocks and parameterized SQL; `C# 5/6`, .NET 4.5.2; no async data access anywhere | repo convention | New components follow the same style: **synchronous, parameterized, `using`-wrapped**. No async, no Dapper/EF, no static mutable state (Ponytail §3). |
| G10 | `db/impersonation_foundation.sql` (doc 27 §9) does not exist yet; nothing reads or writes `ImpersonationLink`/`SecurityAudit` | repo state | Phase 1 ships **DB script + components together**; until both land, the feature is inert (doc 27 §10 fail-closed contract). |

---

## 1. Component Inventory (deliverables and touch list)

| # | Component | New/Modified | Phase | Responsibility |
|---|---|---|---|---|
| 1 | `db/impersonation_foundation.sql` | new | 1 | Doc 27 §9 migration (tables, indexes, FKs, grants, permission seeds) |
| 2 | `SecurityContext.cs` | **new** | 1 | Per-request resolver: acting identity, actor, active link (L3) |
| 3 | `ImpersonationService.cs` | **new** | 1 | TryStart / TryEnd / lazy close / stale reclamation — all gates, one transaction per act (L4+L5 writer) |
| 4 | `AuthGuard.cs` | **modified (minimal)** | 1 | Fork points behind the four gates + `CurrentUserId` (§4); no signature removals, no behavior change without a link |
| 5 | `Bill.Master.cs` / `.Master` | **modified** | 2 | Ambient resolution + banner render + company-dropdown state + logout order (§5) |
| 6 | `Heartbeat.ashx.cs` | **modified (additive)** | 2 | Link liveness check + `linkClosed` field (§6) |
| 7 | `SwitchUser.aspx[.cs]` | **rewritten** | 2 | Thin controller over `ImpersonationService` per doc 28 §3 |
| 8 | `index.aspx.cs` (login) | **unchanged** | — | `UserDbId` already set (G1) |
| 9 | `SecurePage.cs` | **unchanged** | — | Inherits the fork via `AuthGuard` (G4) |

**Explicitly not created:** any page-level permission caching, any second auth stack, any static identity state, any new HttpModule (the master + gates are sufficient injection surface for this app's single-frame architecture).

---

## 2. `SecurityContext` — the resolver (L3)

### 2.1 Contract

```
static class SecurityContext
    // Resolution happens once per request, lazily, into HttpContext.Items["SecurityContext"].
    // No static fields. No caching beyond the request (Ponytail §3).

    bool        IsImpersonating          // link active AND lease valid
    string      ResolveUserId()          // acting business key  (target while observing, else principal)
    int         ResolveUserDbId()        // acting db id         (link.TargetUserId, else Session["UserDbId"])
    string      ResolveActorUserId()     // PRINCIPAL business key — always the admin (attribution anchor)
    int         ResolveActorDbId()       // PRINCIPAL db id — always Session["UserDbId"]
    LinkInfo    TryGetLink()             // null when not impersonating or on any lookup failure (fail closed)
```

`LinkInfo` is an immutable per-request snapshot: `LinkToken`, `AdminUserId`, `TargetUserId`, `TargetUser_Id`, `TargetDisplayName`, `CompanyID`, `Reason`, `StartedUtc`.

### 2.2 Resolution algorithm (normative order)

1. If `HttpContext.Items["SecurityContext"]` present → return it (memoized for this request only).
2. Read `Session["ImpersonationToken"]` (L2). Absent → not impersonating; done.
3. Single query against `UX_ImpersonationLink_Token` (doc 27 §3.1): link by `LinkToken` **AND** `IsActive = 1` **AND** `AdminSessionToken = Session["SessionToken"]` (the lease) **AND** `AdminUserId = Session["UserDbId"]` (bind to the principal, not just the token — doc 25 §1.2).
4. Join target liveness in the same query: `tbl_login.IsActive = 1` and no open `PasswordResetTokens` row for the target (doc 25 F4). If liveness fails → **lazy close** via `ImpersonationService.TryClose(token, reason)` and treat as not impersonating.
5. Any SQL exception → **not impersonating** (F13: degraded availability, never elevated access).
6. Cache the result in `HttpContext.Items` and return.

### 2.3 The one-sentence rule operationalized

> **Reads resolve to the acting identity; writes attribute to the principal.**

`ResolveUserId()` (acting) feeds ownership/visibility predicates — "my visits", "my expenses", home KPIs. `ResolveActorUserId()` (principal) is the value new audit columns and any attribution-sensitive write path must use. Existing pages that read `Session["USERID"]` for writes keep getting the principal — the invariant holds without touching them (doc 23 §8 Phase 2).

### 2.4 Forbidden patterns (spec-level prohibitions)

- Copying the target's identity into `Session["USERID"]` per request or otherwise (doc 23 §4.4 architect's note — identity swap with extra steps).
- Caching a link in `Session` beyond the pointer, in `Application`, or in a static field.
- Authorizing anything from the banner, query string, or ViewState.
- Resolving the link anywhere except through `SecurityContext` (pages and services must not query `ImpersonationLink` directly).

---

## 3. `ImpersonationService` — the acts (L4/L5 writer)

### 3.1 `TryStart` contract

```
StartResult TryStart(int targetDbId, string reason, string intentToken)
StartResult { ok, denialReason }        // denialReason ∈ doc 26/27 taxonomy
```

Ordered gates, **each fail-closed**, all inside one SQL transaction for the final write:

| # | Gate | Source of truth |
|---|---|---|
| 1 | Session valid (`TryValidateSession`) + company context present | G5 |
| 2 | Per-act permission: forked `HasPermission("SwitchUser")` — evaluated for the **principal** (starting is a principal act; the fork in §4 applies to *page* permissions, not to this gate) | doc 23 §2.3 |
| 3 | No active link for the principal — `SecurityContext.IsImpersonating` short-circuit, then DB check via `UX_ImpersonationLink_Admin_Active` | doc 27 §3.1 |
| 4 | Stale-link reclamation: close any principal link whose `AdminSessionToken ≠ current token` with `EndReason='Superseded'` (doc 27 §3.1 companion rule) — executed inside the transaction before the insert | doc 27 §3.1 |
| 5 | Target eligibility — all of doc 23 §4.6: same company + target's active `UserCompanyAccess`; `IsActive = 1`; no pending reset; not currently a principal or target of any active link (`UX_..._Target_Active`); target ≠ principal | doc 27 §4 |
| 6 | **Superset rule**: target's permission key set ⊆ principal's permission key set. Implemented as a set comparison over the existing `UserRoles → RolePermissions → Permissions` join for both parties; fails closed on any query error. Pure function of two key sets → unit-testable (doc 23 Phase 1.5) | doc 23 §2.3 |
| 7 | `IntentToken` unused (single-use — consumed by this transaction; replay yields `intent_used`) | doc 23 §4.7, doc 25 F10 |
| 8 | Write: INSERT `ImpersonationLink` (CSPRNG `LinkToken` + `IntentToken`) + INSERT `SecurityAudit('ImpersonationStart')` — **one transaction, fail-closed** (F7: audit failure aborts the start) | doc 27 §2.2 |
| 9 | Set `Session["ImpersonationToken"]` **only after** commit | doc 25 T2 |

### 3.2 `TryEnd` contract

```
bool TryEnd(string endReason)    // voluntary path: reason 'UserEnded'
```

1. Resolve link via `SecurityContext` (if none → return true; idempotent).
2. Conditional close (`WHERE LinkToken=@t AND IsActive=1`); zero rows affected → already closed → skip audit (exactly-once pair, doc 27 §6).
3. Same transaction: INSERT `SecurityAudit('ImpersonationEnd')`.
4. Clear `Session["ImpersonationToken"]` after commit.
5. Return; caller re-derives principal authorization fresh (doc 23 §7) — a revoked admin is redirected to logout by existing master behavior; nothing is restored from session.

### 3.3 `TryClose(linkToken, endReason)` — internal/lazy

Used by `SecurityContext` (step 4 of §2.2) and Heartbeat (§6) for forced closures (`TargetIneligible`, `TimedOut`, `Revoked`, `Superseded`). Conditional close + audit, idempotent, no session dependency (may run for a link whose session is already gone — the sweeper path).

### 3.4 Sweeper posture

Lazy-only at go-live: closures happen on resolution (§2.2 step 4), heartbeat (§6), and `TryStart` reclamation. No web-tier background thread (G9). The SQL Agent job remains a deferred option (doc 27 §9.3).

---

## 4. `AuthGuard` fork points (minimal-diff specification)

`AuthGuard` is a protected asset (CODEOWNERS) — changes are enumerated per method; anything not listed is untouched.

| Method | Change |
|---|---|
| `TryValidateSession` | **None** (G5) |
| `HasPermission(key)` | When `SecurityContext.IsImpersonating`: evaluate the same join for the **target** (`TargetUserId`, target's company = link `CompanyID`) instead of the principal. Else: unchanged. This single fork powers menu, `SecurePage`, and WebMethod permission gates while observing (INV-4) |
| `EnsurePage` / `EnsurePageAny` | After session validation, no direct change needed — they already delegate to `HasPermission`, which is forked. Company-context checks remain **principal-scoped** (INV-9: the admin's own `UserCompanyAccess` still gates the request; target membership was proven at start and re-checked lazily in §2.2 step 4) |
| `EnsureWebMethod` / `EnsureWebMethodPermission` | Same delegation; no code change beyond what `HasPermission`'s fork provides |
| `CurrentUserId()` | Returns `SecurityContext.ResolveUserId()` (acting) — owner predicates (`UserOwnsVisit`, `VisitExists(requireOwner)`, expense/leave ownership reads) then evaluate against the target's own records while observing, which is the feature's purpose; writes keep reading `Session["USERID"]` = principal (§2.3) |
| `ResolveUserDbId` | **None** — `SecurityContext.ResolveUserDbId()` composes it for the acting side; principal side reads the same session key |

**Invariant checklist for the fork (release blockers):** authentication leg never forked (G5); company membership never forked (INV-9); `HasPermission` for the principal is still what gates *starting* impersonation (§3.1 gate 2); no method returns target identity to a write path.

---

## 5. `Bill.Master` integration (Phase 2)

1. After existing validation (R1–R3), resolve `SecurityContext` once; render per doc 28: banner Z3 when `IsImpersonating`, company dropdown disabled + tooltip (G5 of doc 28), `UpdateSwitchUserVisibility` gains one more condition (`!SecurityContext.IsImpersonating`).
2. Banner data comes from `TryGetLink()` only — no session reads beyond the pointer (L2).
3. **Logout order change** (doc 25 §6): if `IsImpersonating`, `TryEnd("Logout")` first; failure is logged server-side and does **not** block logout steps 2–4 (lease makes the link dead regardless — doc 25 §6.2). One combined confirmation dialog per doc 28 §5.4.
4. No master code writes any identity session key.

---

## 6. Heartbeat integration (additive)

After the existing principal validation and `LastHeartbeat` update (G6):

1. If `Session["ImpersonationToken"]` present: resolve the link (lease = the same `ActiveSessions` row just validated); evaluate max-duration (4 h cap, clock D) and target liveness.
2. On failure: `ImpersonationService.TryClose(token, reason)` and respond `{"status":"ok","linkClosed":"<reason>"}` — the session itself stays valid; only the observation ended.
3. On success: `{"status":"ok","linkClosed":null}`.
4. Client behavior (banner removal + toast) is specified in doc 28 §5.3; the existing `status:"logout"` reasons and copy are untouched.

This makes the heartbeat the effective lazy sweeper for *live* browsers; resolution-time closing (§2.2) covers requests; only abandoned links await optional SQL sweep.

---

## 7. Legacy `SwitchUser.aspx.cs` disposition (Phase 0 — pre-existing condition)

The page is **already dead code** against current source: its `HasPermission(string, string, int)` call has no matching overload (G3) — it cannot compile, and therefore cannot be the shipped binary's behavior *if* the current tree is what builds `flamex_live`. Disposition:

1. **Verify** which source actually built each environment (release history) — the doc 23 Phase 0 evidence requirement (no role holds `SwitchUser`) stands regardless.
2. **Remove or gate** the legacy write paths (`btnSwitch_Click`, `btnSwitchBack_Click` bodies) — they implement the identity-swap defect (G7) and must never be resurrected; keep the page shell + permission seed (inert).
3. The Phase 2 rewrite (§1 item 7) replaces the page wholesale per doc 28 §3: thin controller over `ImpersonationService`, no direct SQL beyond what the service owns.

---

## 8. Test Matrix (maps transitions/failures to tests)

| Area | Cases |
|---|---|
| Superset rule (pure) | equal sets ⊆ ok; target-extra key → deny `broader_permissions`; empty target set ok; query error → deny (fail closed) |
| Eligibility | inactive target; pending reset; target already principal of a link; target already target of a link (`target_in_use`); cross-company id; self |
| `TryStart` concurrency | double-submit (`intent_used`); two admins same target (`target_in_use`); same admin re-login then start (reclamation + fresh link) |
| `TryEnd` | happy close; double-close idempotency (single audit pair); close with dead lease |
| Fork | page permission = target's while observing; menu likewise; principal's `SwitchUser` still gates start; writes attribute to principal (`ResolveActorUserId`); `TryValidateSession` untouched |
| Lease | supersede via second login → next resolution/heartbeat closes `Superseded`; 30-min idle → existing logout UX, link closed |
| Forced closure | 4-h cap (`TimedOut`); target password reset mid-observation (`TargetIneligible`); heartbeat `linkClosed` surfaces each reason |
| Fail-closed | audit insert failure aborts start (F7); link-lookup SQL error → not impersonating (F13); missing tables → deny everything (doc 27 §10) |

---

## 9. Rollout Order (binding)

1. **Phase 0** (§7) — can ship immediately; independent of the DB script.
2. **Phase 1** — `db/impersonation_foundation.sql` + `SecurityContext` + `ImpersonationService` + `AuthGuard` fork. Feature still **inert**: no UI, no grants.
3. **Phase 2** — master/heartbeat integration + `SwitchUser.aspx` rewrite + doc 28 surfaces; then runtime grant of `SwitchUser` to a pilot role (grant is a runtime decision, never in the migration — doc 27 §9.2).
4. **Phase 3** — audit viewer + transparency (doc 23 §6.3/6.4) on the same `SecurityAudit` substrate.

Each phase is independently reversible (doc 27 §9.2 rollback properties hold; the fork degrades to no-link behavior the moment tables are absent).

---

## 10. Requirement Traceability

| Deliverable | § |
|---|---|
| Component inventory & touch list | §1 |
| Session object model realization (L1–L6) | §2 (L2/L3), §3 (L4/L5), G1/G5 (L1/L6) |
| Safe insertion points (doc 24) | §4 fork table, §5, §6 |
| Unsafe interception points avoided | §2.4 prohibitions |
| Rollback sequence (doc 23 §7) | §3.2, §5.3 |
| Audit fail-closed, same-transaction | §3.1 gate 8, §3.3 |
| Phases | §9 |
| Test evidence plan | §8 |

---

*End of document. Contracts and behavior only — method bodies, final SQL, and markup are authored at implementation time against this specification.*
