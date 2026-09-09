# 25 — Session Architecture for Administrative Impersonation

| | |
|---|---|
| **Status** | **Design — session mechanics for the Link Model** |
| **Author role** | Identity Architecture |
| **Depends on** | [`23_Impersonation_Architecture_Review.md`](23_Impersonation_Architecture_Review.md) (feature design), [`24_Impersonation_Lifecycle.md`](24_Impersonation_Lifecycle.md) (lifecycle analysis, invariants INV-1…INV-12) |
| **Scope** | Session object model, state machine, failure recovery, timeouts, logout, concurrency |
| **Constraint** | Architecture documentation only — no production code |

---

## 0. Ground Truths (verified in code, drive every decision below)

| # | Fact | Source | Architectural consequence |
|---|------|--------|---------------------------|
| G1 | `sessionState mode="InProc"`, no `timeout` attribute → **ASP.NET session expires after 20 min** of no request touching it; `customProvider` (SQL) is configured but **inactive** under `InProc` | `Web.config` | All session state is lost on app-pool recycle — **impersonation state must be DB-anchored**, session holds only a pointer |
| G2 | Forms cookie: `<forms timeout="2880" />` sliding per-request | `Web.config` | Cookie can outlive the server session — cookie alone authenticates nothing (validated only via `Session["SessionToken"]` ↔ `ActiveSessions`) |
| G3 | Heartbeat handler: validates `SessionToken` ↔ `ActiveSessions.IsActive`, enforces **30-min DB-side idle timeout**, updates `LastHeartbeat` per poll, detects **"superseded"** sessions | `Heartbeat.ashx.cs` | Existing client-polling loop is the natural place to surface impersonation liveness to the UI; link expiry piggybacks on it |
| G4 | Login: `UPDATE ActiveSessions SET IsActive=0 WHERE UserId=@UserId` → **one live session per user, last login wins** | `index.aspx.cs` | A second browser logging in as the same admin **supersedes** the first — link must be bound to the *session token*, not just the admin user |
| G5 | Logout: kill `ActiveSessions` row by token → `Session.Clear()` → `Session.Abandon()` → `FormsAuthentication.SignOut()`; DB errors **swallowed** | `Bill.Master.cs` `btnLogOut_Click` | Logout must end the link first; local session teardown already guarantees the link becomes unusable even if the DB step fails |
| G6 | Every ERP request crosses `Bill.Master` validation (R1) → page gate (`SecurePage.OnInit`) | docs/24 | Refresh, new tab, deep link — all re-resolve impersonation state server-side per request. **Browser refresh survival is free** if state is server+DB side |
| G7 | `Session["UserDbId"]` set at login; `Session["USERID"]` is the attribution anchor | `index.aspx.cs` | Admin principal values are written once at login and **never modified** during impersonation (INV-1) |

---

## 1. Session Object Model

### 1.1 Object inventory

Four layers, each with a distinct lifetime. **The DB row is the canonical state; everything else is a cache or a pointer.**

| Layer | Object | Location | Lifetime | Volatility | Role in impersonation |
|---|---|---|---|---|---|
| L1 | **Admin principal keys** — `USERID`, `UserDbId`, `SessionToken`, `RoleId`, `RoleName`, `USERTYPE`, `ProfilePic`, `CompanyID`, `CompanyCode` | ASP.NET Session (`InProc` memory) | Login → logout/recycle | **Written once at login**; never modified during impersonation | The immutable principal (INV-1). Lost on recycle → re-login |
| L2 | **`Session["ImpersonationToken"]`** (the *only* new key) | ASP.NET Session | Link start → link end | Cleared on rollback/logout | **Pointer** to the canonical L4 row. Holds a GUID — no identity, no roles (INV-12) |
| L3 | **`SecurityContext`** (per-request) | `HttpContext.Items` | One HTTP request | Rebuilt every request | Acting identity / actor / link resolved fresh from L1+L2+L4. Never cached beyond the request |
| L4 | **`dbo.ImpersonationLink`** row (canonical) | SQL Server | Link start → end (+ sweeper grace) | Survives recycle, refresh, browser close | Source of truth: `LinkToken`, `AdminUserId`, `AdminSessionToken`, `TargetUserId`, `CompanyID`, `Reason`, `StartedUtc`, `LastUsedUtc`, `EndUtc`, `EndReason`, `IsActive`, `IntentToken` |
| L5 | **`dbo.SecurityAudit`** rows | SQL Server | Append-only, permanent | — | Event log for every transition (§2). Never the state authority |
| L6 | **`dbo.ActiveSessions`** row (admin) | SQL Server | Login → logout/supersession/30-min idle | Killed by new login (G4) | Principal liveness — the link's lease anchor |

### 1.2 Key design rule: session-holds-pointer, DB-holds-state

```
Browser ── ASP.NET_SessionId cookie ──▶ InProc Session (L1 principal + L2 pointer)
                                              │ L2 = LinkToken GUID only
                                              ▼
                     Bill.Master (per request): SELECT * FROM ImpersonationLink
                     WHERE LinkToken=@L2 AND IsActive=1
                       AND AdminSessionToken=@L1.SessionToken   ← lease check (G4)
                                              │ valid
                                              ▼
                     HttpContext.Items["SecurityContext"] (L3, request-scoped)
                                              │
                        pages read acting identity; writes use principal
```

**Why the lease check matters:** binding the link row to `AdminSessionToken` means that when the admin's `ActiveSessions` row dies (new login in another browser, 30-min idle, logout), the link is **automatically invalid** on the very next request — no orphan can ever be presented by a live session. The sweeper only closes rows for audit completeness.

### 1.3 Effective-user vs. original-identity (requirement mapping)

| Requirement | Mechanism | Where |
|---|---|---|
| Preserve original administrator identity | L1 keys written at login, immutable thereafter (INV-1) | Session + `ActiveSessions` |
| Track effective user separately | `SecurityContext.ResolveUserId()` resolves the **target** from L4 while `Session["USERID"]` keeps the admin; `ResolveActor()` always returns the admin | L3 per request |
| Prevent nested impersonation | `TryStart` refuses when the *principal* already has an active link (session key + DB check), and refuses when the *target* is currently a principal in any active link. One active link per principal, one per target — enforced in one component, server-side | `ImpersonationService` |
| Support rollback | End link (transactional, audited) → principal was never mutated → nothing to restore (INV-7) | §4, §6 |
| Preserve existing authorization | `AuthGuard`/`SecurePage` semantics unchanged; page permission fork resolves against the target only while a link is live (INV-4) | doc 24 §3-I2 |
| Maintain tenant isolation | Link valid only while **both** admin `UserCompanyAccess` (master-validated) and **target** active `UserCompanyAccess` for `CompanyID` hold; company context resolution untouched | doc 24 INV-9 |
| Survive browser refresh | L2 (server session) + L4 (DB) — refresh re-runs master → link re-resolved; no client state involved | G6 |
| Terminate cleanly on logout | Link end in the logout path before session teardown; lease check makes the link dead immediately even if the DB write fails | §6 |

---

## 2. State Transition Diagram

### 2.1 Session-level state machine (one browser session of one admin)

```
                                ┌─────────────────────────────────────────────┐
                                │                LOGIN (index)                │
                                │  principal written (L1) · ActiveSessions    │
                                │  row minted (L6) · prior rows killed (G4)   │
                                └──────────────────┬──────────────────────────┘
                                                   ▼
                                     ┌───────────────────────────┐
                     ┌──────────────▶│        NORMAL_ADMIN       │
                     │               │  L2 absent · SecurityContext│
                     │   rollback /  │  = principal · pages gated  │
                     │   end-observation             │ by admin perms│
                     │               └───────────┬───────────────┘
                     │                           │ TryStart(target, reason)
                     │                           │ per-act SwitchUser check ·
                     │                           │ eligibility · superset · intent token
                     │                           ▼
             ┌───────────────────┐   ┌───────────────────────────┐
             │  IMPERSONATING    │◀──│        LINK_ACTIVE        │
             │ L2 = LinkToken    │   │ L4 row IsActive=1 · audit │
             │ banner on · page  │──▶│ ImpersonationStart committed│
             │ perms = target's  │   └───────────┬───────────────┘
             └─────────┬─────────┘               │
                       │                         │ lease dies (any of):
                       │                         │  · logout (§6)
                       │                         │  · superseded by new login (G4)
                       │                         │  · 30-min idle / ASP.NET 20-min (§5)
                       │                         │  · app-pool recycle (L2 lost)
                       │                         ▼
             ┌─────────┴───────────────────────────────────────┐
             │                  LINK_CLOSED                     │
             │  EndUtc/EndReason set · audit ImpersonationEnd   │
             │  EndReason ∈ { UserEnded, Logout, Superseded,    │
             │                SessionExpired, IdleTimeout,      │
             │                Recycle, Revoked, TimedOut,       │
             │                TargetIneligible }                │
             └────────────────────────┬─────────────────────────┘
                                      │ next request resolves NORMAL_ADMIN
                                      ▼
             ┌───────────────────────────────────────────────────┐
             │                     LOGGED_OUT                    │
             │  ActiveSessions killed · Session.Abandon · SignOut│
             └───────────────────────────────────────────────────┘
```

### 2.2 Transition table

| # | From | To | Trigger | Server actions (order matters) | Audit |
|---|------|----|---------|-------------------------------|-------|
| T1 | — | NORMAL_ADMIN | Successful login | Kill prior session rows → mint L6 → write L1 → cookie | (existing login audit path) |
| T2 | NORMAL_ADMIN | LINK_ACTIVE | Admin completes switch (search → select → reason → confirm) | Per-act `HasPermission` → target eligibility (§ doc 23 4.6) → superset rule → **one transaction**: INSERT L4 + audit `ImpersonationStart` → set L2 | `ImpersonationStart` (fail-closed: any failure ⇒ stay NORMAL_ADMIN) |
| T3 | LINK_ACTIVE | NORMAL_ADMIN | Admin clicks **End Observation** | Validate L2+lease → **one transaction**: close L4 (`UserEnded`) + audit → clear L2 → re-derive admin auth fresh (INV-7); if revoked ⇒ redirect logout | `ImpersonationEnd` |
| T4 | LINK_ACTIVE | LINK_CLOSED(Logout) | `btnLogOut_Click` while impersonating | End L4 (`Logout`) + audit → then existing logout sequence (G5) | `ImpersonationEnd` |
| T5 | LINK_ACTIVE | LINK_CLOSED(Superseded) | New login by same admin (another browser) kills L6 (G4) | Next request/heartbeat in old browser fails lease → close L4 (`Superseded`) + audit → treat as logout | `ImpersonationEnd` |
| T6 | LINK_ACTIVE | LINK_CLOSED(SessionExpired/IdleTimeout) | 30-min idle (G3) or ASP.NET 20-min session death | Sweeper/lazy check closes L4 + audit; no live session can present it anyway (lease) | `ImpersonationEnd` |
| T7 | LINK_ACTIVE | LINK_CLOSED(Recycle) | App-pool recycle wipes L2 (G1) | Admin re-logs in (normal); sweeper closes the orphan row | `ImpersonationEnd` |
| T8 | LINK_ACTIVE | LINK_CLOSED(TargetIneligible) | Target password reset initiated / deactivated / company access revoked mid-observation | Lazy per-request target re-check (cheap: part of L4 lookup join) → close L4 + audit → NORMAL_ADMIN | `ImpersonationEnd` |
| T9 | LINK_ACTIVE | LINK_CLOSED(TimedOut) | Max link duration exceeded (recommended: 4 h hard cap) | Same as T6, distinct reason | `ImpersonationEnd` |
| T10 | NORMAL_ADMIN | NORMAL_ADMIN (denied) | Switch attempt fails any gate | No state change; denial surfaced to UI | `ImpersonationDenied` (+ machine-readable cause) |
| T11 | any | LOGGED_OUT | Logout (§6) | Existing sequence; link ended first if active | `ImpersonationEnd` if active |

**Invariants across all transitions:** the principal (L1) is written only at T1 and only read everywhere else (INV-1); every LINK_ACTIVE→* transition is audited exactly once; no transition ever writes a session identity key (INV-12).

---

## 3. Failure Recovery Scenarios

The design goal: **every failure converges to a safe state with a complete audit trail**, and the DB row + lease binding make recovery deterministic rather than heuristic.

| # | Failure | Immediate effect | Recovery path | Worst case if unmitigated | Mitigation built into model |
|---|---------|------------------|---------------|---------------------------|------------------------------|
| F1 | **App-pool recycle** mid-impersonation | L1+L2 vanish (G1); L4 row remains `IsActive=1` | Admin is at login page (normal WebForms behavior). Old L4 row closed by sweeper (`Recycle`); it can never be resurrected because no live session can present both L2 and a matching L6 token | Orphan row blocks the target for one-link-per-target checks | Sweeper + lease: row is inert to all authorization; only target-exclusivity is affected, and the sweeper window is bounded (session timeout + grace) |
| F2 | **DB row alive but session object gone** (web-tier memory lost while L4/L6 rows persist — the inverse of F1) | Link unusable on next request: no live session can present L2 + matching L6 token (lease fails) | Close row (`SessionExpired`) | — | Lease check in the L4 lookup |
| F3 | **Second browser logs in as same admin** | First browser's L6 row deactivated (G4); first browser heartbeat returns `superseded` → local kill | First browser: forced logout UX (existing). L4 row closed `Superseded` on detection/sweep. Second browser: clean NORMAL_ADMIN, may start a *new* link | Two browsers believing they hold the same link | Lease binds link to `AdminSessionToken`; supersession detector (G3) already exists |
| F4 | **Target initiates password reset mid-observation** | Target eligibility broken | T8: lazy target re-check on next master request closes the link (`TargetIneligible`) | Admin observes a compromised-credential account after reset invalidates its sessions | Per-request target re-check joins `PasswordResetTokens`/`IsActive` into the link lookup — no sweeper latency |
| F5 | **Admin's `SwitchUser` permission revoked mid-observation** | Link remains — permission gates *starting*, not continuing (deliberate: avoids yanking an audit mid-review) | Option A (recommended): lazy re-check on link lookup; if revoked, close `Revoked`. Option B: allow graceful completion of current link only | Silent continued observation by a permissionless admin | Decision documented; A is default — fail-closed |
| F6 | **Admin's company access revoked mid-observation** | Master R3 already re-derives company from `UserCompanyAccess` per request | Master validation fails the request → effectively forced logout; link closed by lease/sweep | — | Existing master behavior; no new code |
| F7 | **Audit write fails during TryStart** | — | **Abort the switch** (fail-closed, INV-6). No L2, no L4 | Unaudited impersonation | Single transaction covers L4+audit |
| F8 | **Audit/L4 write fails during rollback (T3)** | Admin still sees banner | Admin retries; if the row actually committed and the response was lost, L2 is already cleared or lease-consistent — next request resolves NORMAL_ADMIN | Banner without a link — cosmetic only | Rollback clears L2 *after* commit; commit itself is idempotent to retry (row closed check) |
| F9 | **Network drop between commit and response on T2** | DB has LINK_ACTIVE and L2 is set (both server-side, committed before response); only the browser's view is stale | Refresh: banner appears (L2+L4 consistent). Admin can End Observation immediately | None — server-side set makes this safe | L2 set server-side before response; refresh re-syncs UI |
| F10 | **Double-click / race on Switch confirm** | Two concurrent TryStart requests | Intent token consumed exactly once (single-use, first commit wins); second fails `ImpersonationDenied(intent_used)` | Duplicate links to same target | One active link per principal — DB-level check in same transaction |
| F11 | **Two admins switch to the same target concurrently** | Second TryStart denied | Clear denial reason (`target_in_use`) surfaced in UI | Conflicting observation sessions on one account | One active link per target (doc 23 §4.6.3) |
| F12 | **Browser crash, admin reopens and re-logs-in** | Same as T1 (fresh session; old rows killed) | Old link closed `Superseded` | — | G4 + lease |
| F13 | **DB connection failure during link lookup** | Master cannot resolve L3 | **Fail closed to NORMAL_ADMIN** (link treated as absent); normal page authorization proceeds — availability degraded, security intact | Elevated access during DB outage | Lookup error ⇒ no link, never ⇒ link granted |

---

## 4. Edge Cases

| # | Case | Resolution |
|---|------|------------|
| E1 | Admin opens `SwitchUser.aspx` while impersonating | Header link hidden (existing `UpdateSwitchUserVisibility`); direct URL: page loads but TryStart is refused (principal already has an active link) → `ImpersonationDenied(nested)` |
| E2 | Target is another admin who is currently impersonating a third user | Denied (E1's rule from the other side): one link per principal, one per target. Prevents chains A→B→C entirely at the act level, not just the UI level |
| E3 | Self-switch (admin targets own account) | Denied (existing check) — meaningless under the link model and would confuse one-link-per-target |
| E4 | Admin's roles *change* mid-observation (loses a permission the target holds) | Observation continues under the already-proven superset (evaluated at T2). New writes still attribute to the admin — a now-under-privileged admin cannot *acquire* anything via the link; page gates resolve to the target, which is unchanged. Recommendation: next TryStart re-checks; ongoing link unaffected |
| E5 | Long-running postback / export crosses link end (End Observation clicked in another tab) | Server-side state per request: the in-flight request already resolved L3 and completes under it; the *next* request sees NORMAL_ADMIN. No torn state — L3 is request-scoped |
| E6 | Print pages, ashx handlers, WebMethods (paths that don't derive from `SecurePage` or skip master) | Print gate (`EnsurePrint`/`AuthGuard`) and every handler that validates identity must use `SecurityContext`/`AuthGuard` — the audit (doc 15/19) already mandates `AuthGuard` at these boundaries; the link lookup lives inside AuthGuard's shared resolution, not in master alone. No unauthenticated/under-authorized path exists to bypass the fork |
| E7 | `SessionKeepAlive`/heartbeat traffic during impersonation | Heartbeat validates **principal only** (G3) — correct and unchanged. It must NOT consult or extend links: the link has its own lease and max-duration. Banner liveness to the UI can ride the same poll response (additive JSON field) |
| E8 | Clock skew between web servers and SQL Server | All link comparisons use DB time (`SYSUTCDATETIME()`), never web-server clocks; `StartedUtc`/`LastUsedUtc` are written by SQL defaults |
| E9 | Link to a user who is later deleted / deactivated in `tbl_login` | Target re-check (F4 mechanism) closes the link; audit rows are denormalized (doc 23 §5.1) so history survives |
| E10 | Admin impersonates, then the *target's* permissions expand mid-observation | Observation continues; effective page access = target's at request time. Acceptable: superset rule constrains who could *start*; the target's own account evolving is the target's normal state. Write attribution still bars the admin from acting as target |
| E11 | Same browser, two tabs: End Observation in tab 2, continue working in tab 1 | Next request in tab 1 resolves NORMAL_ADMIN; the stale banner in tab 1 disappears on its next server interaction. No action needed (state is server-side) |
| E12 | Cookie exists but session is gone (G2 cookie > 20-min session) | Existing behavior: master redirects to login. No impersonation state exists outside the session — cookie can never carry a link (INV-12, U4) |
| E13 | Impersonation start and end in the same request (switch then immediate End) | Legal: two independent acts, two audit rows; idempotent close |

---

## 5. Session Timeout Behavior

### 5.1 The four clocks

```
Clock A: Forms cookie            2880 min, sliding per request   (Web.config)
Clock B: ASP.NET InProc session   20 min, sliding per request*   (default; G1)
Clock C: DB idle (LastHeartbeat)  30 min                          (Heartbeat.ashx.cs G3)
Clock D: Link max duration         4 h, absolute (recommended)    (ImpersonationLink.TimedOut)

* every authenticated page request and every heartbeat poll touches
  the session, so while the tab is open, B keeps refreshing.
```

Effective lifetime while the browser tab is open: governed by **C** (heartbeat stops only if the tab closes or the network dies). If the tab is closed: B expires after 20 min; C kills the DB row at the next check (30 min); A may still be valid but is meaningless alone (G2).

### 5.2 Timeout interactions during impersonation

| Clock death | Effect on principal | Effect on link | UX |
|---|---|---|---|
| **B** (InProc 20 min — only if no requests hit the server; heartbeat normally prevents this) | Session gone → next request redirects to login | Orphan row → sweeper (`SessionExpired`) | Forced re-login (existing) |
| **C** (30-min idle) | Heartbeat kills local session + deactivates L6 row (`timeout`) | Lease dead → link closed `IdleTimeout` | Forced re-login (existing) |
| **D** (link 4 h cap) | **None** — principal untouched | Link closed `TimedOut` on next request; banner disappears; admin continues as self | Transparent: admin notices banner gone; audit records why |
| **A** (cookie 2880 min) | If A dies but B lives: `TryValidateSession` still passes?? — **no**: A is not consulted by the app (G2); principal is B+C anchored. A dying mid-session has no effect until a new login is needed | None | Cookie expiry ≈ irrelevant while browsing; surfaces only at next navigation after B death |

**Design rule:** link lifetime must be **≤ every principal lifetime mechanism**, never the reverse — a link can never outlive the principal that created it. The lease (`AdminSessionToken` match) enforces this structurally; clock D merely adds an *early* human-scale bound.

### 5.3 Sweeper

- Lazy mode (recommended initial): the per-request L4 lookup treats any link whose L6 row is inactive/stale as absent, and a cheap probabilistic close (e.g., on 1-in-20 requests) marks rows closed.
- Scheduled mode (optional): SQL Agent job closing rows where `IsActive=1 AND LastUsedUtc < SYSUTCDATETIME() - @grace` or L6 dead. Audit rows `ImpersonationExpired` per doc 23 §5.2.
- Grace = ASP.NET session timeout + one heartbeat interval (≈ 50 min) to avoid racing a live session.

---

## 6. Logout Behavior

### 6.1 Sequence (normative — order is security-relevant)

```
Admin clicks Logout (banner or header)
   │
   ▼
1. L2 present? ── yes ─▶ close L4 (EndReason='Logout') + audit ImpersonationEnd
   │                     in ONE transaction                        (INV-6)
   │              no ─▶ skip
   ▼
2. UPDATE ActiveSessions SET IsActive=0 WHERE SessionToken=@Token   (existing G5)
   ▼
3. Session.Clear() → Session.Abandon()                              (existing)
   ▼
4. FormsAuthentication.SignOut()                                    (existing)
   ▼
5. 302 → index.aspx
```

### 6.2 Properties

| Property | Guarantee |
|---|---|
| **Idempotency** | Logout from two tabs, or double-click: step 1 is a no-op if L2/L4 already gone; step 2 already-dead rows are unaffected; steps 3–4 are inherently idempotent |
| **DB failure in step 1** | Do **not** swallow silently for the link step: attempt it, log the failure server-side, then proceed — steps 2–3 make the link lease-dead regardless, and the sweeper closes the row. Security does not depend on step 1 succeeding (lease-first design), but audit completeness does — surface a warning in server logs (contrast with current blanket `catch { }`, G5) |
| **DB failure in step 2** | Same as today (row stays active until supersession/idle sweep); local session teardown still ends the browser identity |
| **Impersonation state leakage** | Impossible: L2 dies with the session; L4 is lease-dead; nothing about the link lives in cookies or browser storage (U4) |
| **Target-side effect** | None — target never had a session, row, or token minted (INV-3). Their next login behaves exactly as always |
| **Banner-logout edge** | Logout while impersonating ends the *link*, not "logout as target" — there is no target session to log out of (conceptual clarity the identity-swap design could not offer) |

---

## 7. Concurrent Browser Behavior

| Scenario | What happens | Why it is safe |
|---|---|---|
| **Same admin, two tabs, same browser** | One ASP.NET session (same `ASP.NET_SessionId`), one L6 row, one link. Both tabs share the banner and the acting context | Single-session object; transitions are server-side and serialized per session |
| **Same admin, two browsers / incognito** | Second login kills first browser's L6 row (G4, last-login-wins). First browser: heartbeat returns `superseded` → forced logout; its link closes `Superseded` (F3) | Existing single-session-per-user semantics carry over unchanged; the lease makes the first browser's link inert immediately |
| **Same admin attempts switch in browser 2 while browser 1 observes** | Browser 1 is gone by then (supersession). Browser 2 starts fresh — may start a new link | One link per principal holds trivially |
| **Two different admins, same target simultaneously** | Second TryStart denied: `target_in_use` (F11) | One active link per target |
| **Two different admins, different targets** | Fully independent links, independent audits | No shared state; per-request L3 |
| **Admin and the target both logged in** | Independent users, independent sessions; target's work and sessions are never touched (no rows minted/killed for the target) | INV-3; target's own login supersedes only the *target's* rows |
| **Target logs in while being observed** | Unaffected — observation is a link row, not a session. Target's normal login proceeds (kills only target's prior rows) | INV-3/INV-8 |
| **Same admin on kiosk module simultaneously** | Kiosk realm is disjoint (doc 24 U8); no shared session or identity | INV-10 |

**Concurrency rule summary:** concurrency is governed by two levers only — **one live link per principal** and **one live link per target** — both enforced inside `ImpersonationService` in the same transaction as the insert (race-safe), never in UI logic.

---

## 8. Requirement Traceability

| Requirement | § Where satisfied | Invariants invoked |
|---|---|---|
| Preserve original administrator identity | §1.1 L1, §2 T1 | INV-1 |
| Track effective user separately | §1.1 L2/L3/L4, §1.3 | INV-2, INV-12 |
| Prevent nested impersonation | §4 E1/E2, §7 | — (act-level, `ImpersonationService`) |
| Support rollback | §2 T3, §6 | INV-7 |
| Preserve existing authorization | §1.3, §2 T2/T3 (fork only) | INV-4 |
| Maintain tenant isolation | §1.3, §4 F6 | INV-9 |
| Survive browser refresh | §1.2, §3 F9, §7 tab cases | G6 |
| Terminate cleanly on logout | §6 | INV-6, INV-8, INV-3 |

---

*End of document. Architecture documentation only — no production code was written or modified. Companion docs: 23 (feature design + spec), 24 (lifecycle analysis + invariants).*
