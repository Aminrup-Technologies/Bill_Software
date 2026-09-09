# 24 — Authentication Lifecycle & Impersonation Safety Analysis

| | |
|---|---|
| **Status** | **Approved for review** — analysis of the existing authentication lifecycle against impersonation requirements |
| **Author role** | Senior Identity Architect |
| **Grounded against** | `index.aspx.cs` (login), `Bill.Master.cs` (per-request validation), `SecurePage.cs` (page gate), `AuthGuard.cs`, `Web.config`, `UAT_CATALOG.md`, `docs/22_Security_Baseline.md`, `docs/23_Impersonation_Architecture_Review.md` |
| **Question answered** | *Can the existing authentication lifecycle safely support impersonation?* |
| **Answer** | **Yes — conditionally.** The lifecycle has a clean three-ring structure with one authoritative choke point (`Bill.Master` validation). It supports the **Link Model** (impersonation as an observed context, never an identity swap). It does **not** safely support identity-swap impersonation, and four structural gaps must be closed before any impersonation code runs. |
| **Companion doc** | Design + implementation spec: [`23_Impersonation_Architecture_Review.md`](23_Impersonation_Architecture_Review.md). This document is the **lifecycle analysis**; doc 23 is the **feature design**. |

---

## 0. Executive Summary

The architecture is a **two-token, one-choke-point** model:

- **Token 1 (cookie, 2880 min):** `FormsAuthentication` ticket naming the user
- **Token 2 (DB, per login):** `ActiveSessions.SessionToken` — the *actual* proof of an authenticated browser session, re-validated on every master-page load
- **Choke point:** `Bill.Master.cs` is the only component that turns "some request" into "an authenticated principal + company context." Everything downstream assumes it ran.

Verdict:

1. **The choke point is real and reliable** — every ERP page derives from `Bill.Master`, and `SecurePage.OnInit` runs after master validation. Impersonation can be inserted **after** these points without touching the auth primitive.
2. **Session-based identity is compatible with impersonation** *only* if the principal in session is never overwritten. The safe pattern is a per-request resolver (`SecurityContext`), not a session rewrite — session keys must be **read-only during impersonation**.
3. **Four structural gaps** block impersonation today: no `User_Db_Id` at login (exists — gap closed? **verified present**, see §3.1), audit is fail-open notification rows, no CSRF/intent proof, and no way for `TryValidateSession` to distinguish a *forged* session row. Three of these are closed by doc 23's `SecurityAudit` + `ImpersonationLink` design; the fourth (no machine-readable session provenance) is closed by simply never creating forged rows.
4. **Identity-swap impersonation is architecturally unsafe** — it creates sessions the target never authenticated, destroys write attribution across 111 tables that key ownership to `tbl_login.User_Id`, and rollback-from-snapshot can resurrect revoked grants. The Link Model avoids all three.

---

## 1. Complete Request Lifecycle (as built today)

### 1.1 Login sequence diagram

```
Browser                      index.aspx.cs                        SQL Server
   │                               │                                   │
   │  POST User_Id + Password      │                                   │
   │  + LoginAs (cmbLoginAs)       │                                   │
   │──────────────────────────────▶│                                   │
   │                               │  1. fetch user by User_Id          │
   │                               │──────────────────────────────────▶│
   │                               │◀────── tbl_login row ─────────────│
   │                               │  2. verify password                │
   │                               │     (PasswordHasher: PBKDF2        │
   │                               │      verify; legacy plaintext      │
   │                               │      with hash-upgrade path)       │
   │                               │                                   │
   │                               │  3. KILL all prior sessions:       │
   │                               │     UPDATE ActiveSessions          │
   │                               │     SET IsActive = 0               │
   │                               │     WHERE UserId = @UserId         │
   │                               │──────────────────────────────────▶│
   │                               │  4. INSERT ActiveSessions          │
   │                               │     (new GUID SessionToken,        │
   │                               │      UserId, IP, UA, IsActive=1)   │
   │                               │──────────────────────────────────▶│
   │                               │  5. set Session state:             │
   │                               │       USERID (business key)        │
   │                               │       UserDbId (tbl_login.Id)      │
   │                               │       USERTYPE, RoleId, RoleName   │
   │                               │       ProfilePic                   │
   │                               │  6. FormsAuthentication.           │
   │                               │     SetAuthCookie(UserId, false)   │
   │◀────────── redirect home ─────│  7. 302 → home.aspx                │
   │                               │     (or settings.aspx if           │
   │                               │      password/contact must-update) │
```

**Key facts (verified in code):**
- Login **kills all prior `ActiveSessions` rows for the user** (single-session semantics per user) — this is the invariant that a forged impersonation session would violate (a session the user never created would die only when the *user* logs in again, never when the *admin* finishes impersonating).
- `FormsAuthentication.SetAuthCookie(user.UserId, false)` — non-persistent, 2880-min sliding per `Web.config` (`<forms loginUrl="~/index.aspx" timeout="2880" />`).
- `Session["UserDbId"]` **is set at login** (verified line 142) — the doc-23 "close the business-key ↔ db-id gap" improvement is already in place.
- Password states that must block impersonation are established here: `IsActive`, pending `PasswordResetTokens`, plaintext-password lockout (`Session["MustUpdateUserId"]` → settings redirect).

### 1.2 Authenticated request sequence (every ERP page)

```
Browser                Bill.Master.cs                    SecurePage.cs / AuthGuard        Page code-behind
   │                         │                                     │                              │
   │  GET page.aspx          │                                     │                              │
   │────────────────────────▶│                                     │                              │
   │                         │ R1. TryValidateSession(HttpContext) │                              │
   │                         │     → SELECT ActiveSessions         │                              │
   │                         │       WHERE SessionToken=@tok       │                              │
   │                         │         AND UserId=@USERID          │                              │
   │                         │         AND IsActive=1              │                              │
   │                         │     fail → 302 index.aspx           │                              │
   │                         │ R2. lockout gates (MustUpdate*)     │                              │
   │                         │ R3. CompanyContext resolution       │                              │
   │                         │     (Session["CompanyID"] or        │                              │
   │                         │      first UserCompanyAccess row)   │                              │
   │                         │ R4. menu render w/ per-user         │                              │
   │                         │     permissions; company ddl        │                              │
   │                         │────────────────────────────────────▶│ P1. SecurePage.OnInit        │
   │                         │                                     │     → AuthGuard.EnsurePage    │
   │                         │                                     │       (RequiredPermissionKey)│
   │                         │                                     │     fail → deny/redirect      │
   │                         │                                     │─────────────────────────────▶│ P2. Page_Load etc.
```

**Ring structure:** **R1–R4 (master)** → **P1 (page gate)** → **P2 (page logic)**. `SecurePage` consumers derive from it, so the page gate runs *after* master validation on every secured page. Resource-level checks (`AuthGuard.UserOwnsVisit`, `UserCanViewVisit`, etc.) run inside P2 where needed.

### 1.3 Session ownership model

Two independent token systems coexist — this is the most important structural fact:

| Token | Issued by | Lives in | Bound to | Validated by | Lifetime / kill behavior |
|---|---|---|---|---|---|
| **Auth cookie** (Forms ticket) | `FormsAuthentication.SetAuthCookie` | Browser cookie | `User_Id` string | Framework decrypt (machineKey SHA1) | 2880 min sliding |
| **Session token** (`ActiveSessions.SessionToken`) | Login (index.aspx.cs) | `Session["SessionToken"]` + DB row | `UserId` + browser session | `AuthGuard.TryValidateSession` every request (master) | Session timeout (InProc); killed by new login, logout, or DBA |
| **ASP.NET Session state** (`HttpContext.Session`) | ASP.NET InProc | Server memory keyed by `ASP.NET_SessionId` cookie | Browser | Not re-validated per key | 540-min timeout (commented config; InProc default applies) |

**Ownership rules (current, verified):**

1. **One live `ActiveSessions` row per user.** Login `UPDATE ... SET IsActive=0 WHERE UserId=@UserId` before insert. Consequence: the DB *is* the authority on "which browser sessions exist for user X" — and it has **no concept of a session the user did not create**.
2. `Session["USERID"]` is the **de-facto principal** — read by 188 pages, used as ownership key (`CreatedByCode`, `ApprovedBy`, `UserCode`), approval identity, chat respondent, notification recipient, print gate. **Whatever value sits in this key at write time becomes permanent attribution.**
3. `Session["UserDbId"]` carries the numeric `tbl_login.Id`, set once at login.
4. `Session["RoleName"]`/`RoleId` are **cosmetic** — server gates use `UserRoles` via `AuthGuard`. They are display state, never trust anchors.
5. Company context (`Session["CompanyID"]`) is **derived, not asserted**: master resolves it from `UserCompanyAccess` if absent, and the dropdown postback re-resolves. `CompanyContext.CurrentCompanyID` is the only sanctioned tenant accessor.
6. The principal is **never re-derived from the cookie** — `TryValidateSession` matches the *session-stored* `SessionToken` against `ActiveSessions`. The cookie alone is not sufficient (no page trusts only `HttpContext.User`).

**Impersonation implication:** all state that defines "who the user is" lives in exactly one place (`Session`) and is validated against exactly one table (`ActiveSessions`) at exactly one point (`Bill.Master`). This concentration is what makes a *safe* insertion point possible — and what makes a session-rewrite impersonation catastrophic (§4).

---

## 2. Authentication Boundaries

### 2.1 Boundary map

```
┌──────────────────────────────────────────────────────────────────────────────┐
│ B0 — UNAUTHENTICATED ZONE                                                     │
│ index.aspx, index_start.aspx, reset_password.aspx, SessionKeepAlive*.aspx     │
├──────────────────────────────────────────────────────────────────────────────┤
│ ▲ crossing: login (index.aspx.cs) — password verify + session mint            │
│ ▲ crossing: TryValidateSession — every request, master ring R1                │
├──────────────────────────────────────────────────────────────────────────────┤
│ B1 — AUTHENTICATED (principal established, company unresolved)                │
│ post-validation, pre-company-resolution state inside master                   │
├──────────────────────────────────────────────────────────────────────────────┤
│ ▲ crossing: CompanyContext resolution (R3)                                    │
├──────────────────────────────────────────────────────────────────────────────┤
│ B2 — AUTHENTICATED + TENANT-SCOPED                                            │
│ all master-rendered pages; CompanyContext.CurrentCompanyID valid              │
├──────────────────────────────────────────────────────────────────────────────┤
│ ▲ crossing: SecurePage.OnInit → AuthGuard.EnsurePage (P1)                     │
├──────────────────────────────────────────────────────────────────────────────┤
│ B3 — PAGE-AUTHORIZED                                                          │
│ page code-behind runs; still subject to resource checks (UserOwnsVisit etc.)  │
├──────────────────────────────────────────────────────────────────────────────┤
│ ▲ crossing: resource authorization (P2, per-operation)                        │
├──────────────────────────────────────────────────────────────────────────────┤
│ B4 — BUSINESS OPERATION                                                       │
│ writes carry attribution from Session["USERID"]; audit via tbl_SystemNotif.   │
└──────────────────────────────────────────────────────────────────────────────┘
```

### 2.2 The four trust anchors — and what they anchor

| # | Anchor | Anchors what | Where validated | Impersonation relevance |
|---|---|---|---|---|
| A1 | `ActiveSessions` row ↔ `Session["SessionToken"]` | *liveness* of an authenticated browser session | `AuthGuard.TryValidateSession` (R1) | Must never be forged for a target; link model never touches it |
| A2 | `Session["USERID"]` (+ `UserDbId`) | *identity + attribution* of the principal | Everywhere (pages read it directly) | **Must remain the admin for the whole lifecycle** |
| A3 | `CompanyContext.CurrentCompanyID` | *tenant scope* | Master R3 + every tenant SQL predicate | Must remain the target's membership scope during observation, re-validated per act |
| A4 | `UserRoles`→`RolePermissions` joins | *page + resource authorization* | `AuthGuard.EnsurePage` / `HasPermission` (P1/P2) | While observing, page permissions must resolve against the **target** |

### 2.3 Boundary-crossing rules that already hold

- **B0→B2 only via two crossings** (login or TryValidateSession). No page bypasses the master — `Bill.Master` is the app's only ERP master page, and `SecurePage` consumers all derive from it.
- **Menu visibility is not authorization** (baseline): rendering the link in R4 never grants B3 — the gate is `EnsurePage`.
- **CompanyID never trusted from client input** — dropdown postback re-resolves against `UserCompanyAccess`.
- **Kiosk isolation:** `tbl_card_login`/`admin/*` pages use a separate auth path entirely; never inside B1–B4.

---

## 3. Safe Insertion Points for Impersonation

Ranked by safety; the doc-23 design uses exactly I1 + I2 (+ I3 for UX).

### I1 — Post-validation, pre-page (master, after R1–R3) — **the primary insertion point**

**Where:** inside `Bill.Master.Page_Load`, immediately after `TryValidateSession` succeeds and `CompanyContext` is resolved.

**Why it is safe:**
- The admin principal is fully established (A1+A2 validated) *before* the link is consulted — the link can never create or extend authentication.
- The link is a **read-only lookup** at this point: `SELECT ... FROM ImpersonationLink WHERE LinkToken=@Session["ImpersonationToken"] AND IsActive=1 AND AdminUserId=@UserDbId` — fail-closed (absent/expired token ⇒ normal admin session).
- It cannot be bypassed: every ERP page crosses this code path.
- It cannot escalate: it only *narrows* page permission resolution (to the target's) and never *widens* the principal.

**What happens there (link model):** load link → build `SecurityContext` (acting identity = target, actor = admin) → store in `HttpContext.Items` → banner state computed. **No session key is written.**

### I2 — Page gate (SecurePage.OnInit / EnsurePage) — **the authorization fork**

**Where:** inside `AuthGuard.EnsurePage`, when an active link exists.

**Why it is safe:** `SecurePage.OnInit` runs after master validation (guaranteed by the derivation chain), so the link is already loaded and validated *this request*. The fork is: page permission resolves against the **target's** permission set, never the admin's. Fail-closed: any link error ⇒ evaluate against principal, deny by default.

**What must NOT happen here:** consulting the link for **write** authorization or attribution. Writes stay principal-bound (§5, INV-1).

### I3 — Master UI layer (banner + nav) — **transparency only**

**Where:** `UpdateSwitchUserVisibility` and the banner render, both already in master. Safe because it is display-only; the invariant "menu visibility is not authorization" applies unchanged to the banner.

### I4 — Login (index.aspx.cs) — **one-time, additive change only**

The only *construction-time* insertion: persist `Session["UserDbId"]` (already present) and nothing else. No impersonation logic may live in login; login remains the only credential-verification crossing.

---

## 4. Unsafe Interception Points (must never host impersonation logic)

| # | Location | Why unsafe | Failure mode |
|---|---|---|---|
| U1 | **`Session["USERID"]` / any session key write** during impersonation | Overwrites the attribution anchor (A2); every page reads it as principal | Permanent misattribution of business writes to the victim; rollback-from-snapshot resurrects revoked grants |
| U2 | **`ActiveSessions` INSERT for a target user** | Forges A1: a liveness row for a user who never authenticated; dies only when the *user* logs in again; indistinguishable from real login | Session that survives the target's own password reset; audit cannot tell "observed" from "acting" |
| U3 | **`TryValidateSession` / AuthGuard primitives** (special-casing "impersonation mode") | Per CONTRIBUTING.md, AuthGuard changes require architectural review; a second validation path in the hot primitive forks the trust anchor | Divergent validation semantics; one path is always weaker; untestable combinatorics |
| U4 | **Client-side state** (query string, cookie, ViewState, hidden field) as the link carrier | Trivially forgeable; `CompanyID`-from-client rule already establishes this class of defense | Cross-user/cross-company impersonation by parameter tampering |
| U5 | **Page-level code-behind** (each of 188 pages deciding its own impersonation behavior) | No single choke point; 188 independent chances to get it wrong; unreviewable | Inconsistent enforcement; guaranteed missed pages (orphan pages, WM endpoints, print pages) |
| U6 | **`Session_Start`/`Session_End`** global events | InProc lifetime events are unreliable (recycle, timeout racing requests); no request context | Phantom or missed link teardown; app-pool-recycle orphans |
| U7 | **`master` cookie / FormsAuthentication layer** (e.g., issuing a ticket for the target) | Re-runs the login crossing without credential verification — identical failure to U2 via a different door | Forged authentication leg; defeats password-reset session-kill invariant |
| U8 | **Kiosk module (`tbl_card_login`, `admin/*`)** | Separate auth realm by baseline decision; no shared identity semantics | Cross-realm identity confusion; kiosk has no RBAC |

**Rule of thumb:** impersonation logic may live **downstream of trust anchors, consuming them read-only**. Any code that *produces* or *mutates* a trust anchor during impersonation (U1, U2, U3, U7) is an unsafe interception point by definition.

---

## 5. Required Invariants (must never change)

These are the load-bearing walls. Any PR that violates one is rejected regardless of tests passing.

### INV-1 — Principal immutability during impersonation
`Session["USERID"]`, `Session["UserDbId"]`, `Session["SessionToken"]`, and company context **remain the admin's values** for the entire session lifetime. No code path may write identity keys while a link is active. *(Violation = U1.)*

### INV-2 — Read/write asymmetry
**Reads resolve to the acting identity; writes attribute to the principal.** Display/self-scoped queries may use `SecurityContext.ResolveUserId()`; every `INSERT/UPDATE` attribution column (`CreatedByCode`, `ApprovedBy`, `UserCode`, ...) resolves to the **admin**. There is no exception, including "the admin was reproducing the user's action."

### INV-3 — No forged authentication artifacts
Impersonation must never create or modify rows in `ActiveSessions`, never issue a Forms ticket for anyone but the principal, never mint tokens that a page would treat as proof of the target's authentication. *(Violation = U2/U7.)*

### INV-4 — Authorization fork is target-side, page-side unchanged
While a link is active, `SecurePage`/`AuthGuard.EnsurePage` resolve page permission against the **target's** permission set; with no link, behavior is byte-identical to today. `AuthGuard` semantics are extended **only** at the documented choke point, never forked. *(Violation = U3.)*

### INV-5 — Fail-closed link validation
Link lookup is read-only, single-query, and any error (missing, expired, admin mismatch, DB error) ⇒ behave as a normal admin session. The link can narrow but never widen; it can fail only toward *less* access.

### INV-6 — Audit before effect, fail-closed
Link start/end writes and `SecurityAudit` inserts commit **in one transaction**, and audit failure aborts the operation. Notification rows (`tbl_SystemNotification`) remain user-facing only — never the security audit record.

### INV-7 — Rollback is re-derivation, never restoration
Ending a link re-validates the admin's session/company/roles fresh from the DB. No identity value is ever restored from a session snapshot. The admin session was never mutated, so there is nothing to restore.

### INV-8 — Single-session semantics preserved
Login still kills all of the *user's* `ActiveSessions` rows. Since impersonation creates none, a target user's password reset/logout behaves exactly as today. **Corollary:** ending a link must never deactivate the admin's own `ActiveSessions` row (the current committed code deactivates the admin's token — that is an INV-3/INV-8 violation and is removed in Phase 0).

### INV-9 — Tenant scope is the target's membership, re-proven per act
The link exists only when both admin `UserCompanyAccess` (already validated by master) **and** target active `UserCompanyAccess` for that company hold *at act time*. Company context during observation resolves through the target's membership — cross-company impersonation is a documented non-goal.

### INV-10 — Kiosk exclusion
`tbl_card_login` identities are never impersonation sources or targets. The kiosk auth realm remains fully disjoint.

### INV-11 — One choke point
Impersonation resolution exists in exactly two server components (master post-validation + `SecurePage`/`EnsurePage` fork) plus the `ImpersonationService` act endpoints. No page implements its own impersonation behavior. *(Violation = U5.)*

### INV-12 — Session-state storage limits
`Session` may gain exactly one new key (`Session["ImpersonationToken"]`) and it holds only the link GUID — never roles, permissions, or identity of the target. All deeper state is fetched per request from the link row (baseline §3: minimum identity info in session; fetch the rest fresh).

---

## 6. Verdict: Can the Lifecycle Support Impersonation Safely?

**Yes — the Link Model fits the existing lifecycle without structural change:**

| Lifecycle element | Impersonation compatibility | Notes |
|---|---|---|
| Two-token model (cookie + `ActiveSessions`) | ✅ | Link model adds a *third*, non-authenticating token (`LinkToken`) that never crosses A1's validation path |
| Single choke point (`Bill.Master` R1–R3) | ✅ | I1 insertion is invisible to pages; 188 pages require zero edits |
| `SecurePage` derivation chain | ✅ | I2 fork is local, fail-closed, and downstream of R1 |
| Session-based identity | ✅ *conditional* | Safe **iff** INV-1/INV-12 hold — session keys are read-only during observation |
| Single-session-per-user semantics | ✅ | Preserved because no target sessions are created (INV-3, INV-8) |
| Company context switching | ✅ | Target-membership re-proof per act (INV-9); existing `UserCompanyAccess` flow unchanged |
| Notification-based audit | ⚠️ | Must be supplemented by `SecurityAudit` (INV-6); notification table remains user-facing |
| Session-identity-swap pattern (committed `SwitchUser.aspx.cs`) | ❌ | Violates INV-1/3/7/8; Phase 0 of doc 23 neutralizes it |

**Three gaps to close before any link goes live** (all specified in doc 23):
1. `dbo.ImpersonationLink` + `dbo.SecurityAudit` tables (fail-closed, same-transaction audit).
2. `SecurityContext` resolver wired at I1/I2 (no session writes).
3. Target eligibility + superset rule + intent token, enforced server-side in `ImpersonationService`.

**Hard preconditions (from §5):** Phase 0 neutralization of the committed write paths, verified no role grants `SwitchUser` in UAT *and* `flamex_live`, and the INV list adopted into `docs/22_Security_Baseline.md` as the impersonation section.

---

*End of document. Architecture documentation only — no production code was written or modified as part of this analysis.*
