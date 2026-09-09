# 23 — Administrator User Impersonation (Switch User): Architecture Review & Implementation Specification

| | |
|---|---|
| **Status** | **CONDITIONAL NO-GO** — see §9 |
| **Scope** | Feature: Administrator User Impersonation ("Switch User") |
| **Grounded against** | `SwitchUser.aspx.cs` (commit `ec2aceb`), `Bill.Master[.cs]`, `SwitchUser_permission.sql`, `AuthGuard.cs`, `SecurePage.cs`, `docs/22_Security_Baseline.md`, `docs/14_Authentication_Authorization_Architecture.md` |
| **Review role** | Principal Architect review — specification only, no production code |
| **Governance** | Per `CONTRIBUTING.md`, this feature touches `AuthGuard`/session/RBAC — architectural review required. This document is that review. |

---

## 1. Executive Assessment

The proposal — "admin logs in, searches for a user, switches into that account, and can switch back" — is a **legitimate enterprise support/diagnostic capability** (equivalent to support agents in helpdesks, "act as user" in ERPs). The codebase's existing security foundation (`AuthGuard`, `SecurePage`, `ActiveSessions`, `UserCompanyAccess`) is mature enough to host it.

However, the **first implementation committed as `SwitchUser.aspx.cs` is not that capability**. It is an **identity swap** that, in the current state, constitutes a **Critical-severity privilege-escalation and audit-integrity defect**:

1. **It mints real sessions for the target user** (`INSERT INTO dbo.ActiveSessions ... VALUES (@Token, @UserId(target)...)`). The impersonated session is indistinguishable from a genuine login by that user. Nothing downstream can tell "manager reviewing" from "the actual user acting."
2. **It deactivates the target user's other live sessions** only coincidentally — actually it deactivates *the admin's own* session token and creates a fresh one for the target. But because the new token is inserted *without validating the target's password, MFA, or even existence* — the session-lifecycle invariants of the baseline (`Authentication → ActiveSessions → company membership → page permission → resource authorization`) are preserved in *form* but the *authentication* leg is forged: a session exists that the target user never created.
3. **Attribution is destroyed at write time.** Every business table records `CreatedByCode` / `ApprovedBy` etc. as `tbl_login.User_Id` business keys. After a switch, `Session["USERID"]` *is* the target. Any visit approved, expense approved, quotation created, PO generated, email sent is attributed to the **victim** user forever. There is no actor column anywhere in the 111-table catalog to recover the truth.
4. **The audit record is a notification row, not an audit row.** `tbl_SystemNotification` is a user-facing notification/announcement table (`Title`, `Message`, `Severity`, `StartDate`, `EndDate`, `IsActive`, 7-day expiry), inserted inside a `try/catch` that **swallows failure by design** ("audit log failure should not block the switch"). The PR checklist requires notification logging *before* transaction commit; here the *security event itself* is logged to a table users can see, may expire, and may silently not be written.
5. **Rollback restores a stale snapshot.** `OriginalRoleId`/`OriginalRoleName`/`OriginalCompanyId` are copied from session at switch time and replayed on switch-back. If the admin's role changed mid-impersonation (role revocation, company access revoked), rollback **resurrects the old identity grant** from session state — exactly the class of stale-state escalation the baseline forbids.
6. **Nested impersonation guard relies on client-controlled page navigation only.** The `SwitchedFrom` flag lives in session; any code path that re-enters `btnSwitch_Click` while impersonating re-snapshots "original" state — and since the second switch's "original" is the *impersonated* user, one mis-click chains A→B→C and rollback returns to B, with A's `Original*` values lost. The code partially guards this but the invariant is enforced only in the page, not in a single authoritative component.

The feature concept is approved **in principle**; the current implementation must not ship. The remediation is not a patch list — it is a **session-model change** (§4) plus an **audit subsystem** (§5). This is deliberately *not* the impersonation pattern used by Google/Atlassian (no fresh login required), because this ERP has no per-user challenge infrastructure; instead we adopt the **"Admin real session + observed session link"** model described in §4, which preserves attribution and auditability with the least new moving parts.

---

## 2. Evaluated Dimensions — Findings

### 2.1 Authentication lifecycle — FINDING: forged authentication leg

Baseline contract: `Authentication → ActiveSessions → company membership → page permission → resource authorization → business operation`.

The current code inserts an `ActiveSessions` row for the **target user** with `LoginTime = now`, no credential event. Consequences:

- If the target user is *disabled, locked, pending-password-reset, or flagged* (`tbl_login.IsActive`, `PasswordHash IS NULL` pending upgrade etc. — schema per `UAT_CATALOG`), impersonation **bypasses those states**. The current code checks only existence + same-company.
- Password reset flows (`PasswordResetService`, `PasswordResetTokens`) issue "log out all sessions" semantics in spirit: a user resetting a compromised password expects all sessions to die. A forged session survives (or is re-mintable) and **negates the user's password reset**.
- `TryValidateSession` checks only `IsActive` on the token; it has no concept of "this session was not created by its named user."

**Required change:** impersonation must never create an `ActiveSessions` row whose `UserId` is the target. §4.

### 2.2 Session architecture — FINDING: identity swap breaks the "session = one identity" invariant

Current session keys after switch: `USERID` ← target, `SessionToken` ← new token (target), `RoleId/RoleName/CompanyID` ← target, plus `Original*` snapshot + `SwitchedFrom` flag. This makes `Session["USERID"]` mean **"acting identity"** — but the 188 pages read `Session["USERID"]` as the *authenticated principal* for ownership (`CreatedByCode = @UserId`), approval identity, chat respondent identity, attendance, notification recipients, print gate `EnsurePrint`, and `tbl_SystemNotification.CreatedBy`.

**Required change:** the authenticated principal must remain the admin for the life of the browser session; the acting identity becomes explicit, separate state (§4.3). Pages continue reading `Session["USERID"]` **only through a single resolver**, `SecurityContext`, so no page-level edits are needed (§4.4).

### 2.3 Authorization integrity — FINDING: permission checked once, at switch time; page-level gates still fine, but "who may be impersonated" is unbounded

- `AuthGuard.HasPermission(USERID, "SwitchUser", CompanyID)` at page load and `UpdateSwitchUserVisibility` in master — good pattern, but it gates **entry to the feature**, not **each impersonation act**, and nothing gates **per-target**.
- Unbounded target list: all users of the current company are selectable, including other admins, super-admins, and (if a company has one) users holding more permissions than the switcher. This is **lateral/vertical escalation**: a low-privilege holder of `SwitchUser` (e.g., a support user) can become any user, including a Super Admin. Current code has no target screening.
- Target screening by raw string comparison of role names would be naive (`Roles.RoleName` is per-company data); it must be permission-set-based (§4.2).

**Required change:** per-act authorization + target eligibility rule: **"switcher's permission set must be a superset of target's permission set"** (equality/subset also allowed; the rule is: impersonation must never yield privileges the switcher does not already hold). Enforced server-side per act.

### 2.4 Company isolation — FINDING: preserved, but fragile

Good news: target selection and validation are scoped `WHERE u.CompanyID = @CompanyID`, tenant filter uses `CompanyContext.CurrentCompanyID`, impersonated session keeps `CompanyID` unchanged, `UserCompanyAccess` still applies to the target's access. Because identity swap keeps the same company, tenant isolation is **structurally intact** in the current code.

Fragility: `Session["OriginalCompanyId"]` snapshot is used in rollback, not `UserCompanyAccess`; if the admin's access is revoked mid-impersonation, rollback **re-creates an authorized session for a user who no longer has company access**. Also, cross-company impersonation is possible in the code path (it resolves the target strictly within `CurrentCompanyID`, which is correct) — but nothing *documented* forbids extending it, and the `OriginalCompanyId` replay is the escape hatch.

**Required change:** rollback must re-derive the original identity from the database, never from session snapshot (§4.5/§7); company of the impersonated context must be the **target's** company membership — i.e., impersonation must validate `UserCompanyAccess` for the **target**, not assume same-company.

### 2.5 Rollback mechanism — FINDING: snapshot restore, not re-derivation

Covered in §1.5, §2.4. Rollback must be: deactivate impersonation link → re-derive admin identity from `dbo.tbl_login` + `UserCompanyAccess` + `UserRoles` **fresh** → issue new link token. No session snapshot replay of roles.

### 2.6 Audit strategy — FINDING: notification row ≠ audit event

- Wrong table, wrong semantics (§1.4): `tbl_SystemNotification` is user-visible, expires, and the insert is swallowed on failure.
- No `end` event, no reason, no company, no IP/UA on the switch event itself, no linkage to the impersonated session token.
- Baseline requires transactional audit **before commit** and the PR checklist requires `tbl_SystemNotification` for CRUD; a security event log must be **durable, append-only, non-expiring, and fail-closed**.

**Required change:** dedicated `SecurityAudit` table (§5), fail-closed (abort the switch if audit write fails), written in the same transaction as the session-link write.

### 2.7 Security threats — summary matrix

| # | Threat | Current code | Severity |
|---|--------|--------------|----------|
| T1 | Privilege escalation to any user incl. admins | No target screening; any user in company selectable | **Critical** |
| T2 | Attribution loss on business writes | `Session["USERID"]` becomes target; all writes attributed to victim | **Critical** |
| T2b | "Became the user" session is indistinguishable from real login | Forged `ActiveSessions` row | **High** |
| T3 | Stale-snapshot rollback resurrection | `OriginalRoleId/CompanyId` replay | **High** |
| T4 | Audit loss (swallowed exceptions) | `catch {}` around audit insert | **High** |
| T5 | CSRF on switch/rollback endpoints | No anti-CSRF token on the buttons; postback does not prove admin intent | **High** |
| T6 | Session fixation on rollback | Rollback mints new token; fixation low; but forged target session token is a *new* secret delivered over the same channel — acceptable only with the §4 model change | Medium |
| T7 | Nested/chained impersonation | Guard only in page code; `SwitchedFrom` flag is app-controlled | Medium |
| T8 | Concurrent impersonation of same target (two admins) | Nothing prevents it; each admin's act invisible to the other | Medium |
| T9 | Impersonating a user with pending password reset / disabled | Not checked | High |
| T10 | Audit log poisoning via notification table visibility | `tbl_SystemNotification` is user-visible | Medium |

T5 note: WebForms postbacks are protected by EventValidation + ViewStateUserKey patterns **only where implemented**; the codebase does not set a global `ViewStateUserKey`. CSRF defense here = per-request confirmation token bound to the impersonation link row (§4.6), which is what actually proves admin intent.

### 2.8 Performance implications — LOW, with two caveats

- Per-request cost of the §4 model is **one indexed lookup** on the impersonation link table by token (see §5 index) — comparable to the existing `ActiveSessions` check on every master-page load. Acceptable.
- Caveat 1: `Bill.Master.cs` already does session validation + company context + menu permission SQL per request; adding a third per-request lookup to a hot path used by all pages. Acceptable at this scale (intranet ERP, hundreds of users); **do not** add caching that outlives the request.
- Caveat 2: `LogSwitch` currently swallows exceptions — audit writes must be part of the main transaction (single round-trip, no extra connection churn).
- Non-goal: do not persist impersonation state in `Application`/static caches (baseline §3 prohibits shared static identity state).

### 2.9 Future extensibility — good, if the §4 model lands

- **Self-approval workflows are the highest-value downstream use**: reports show "acting manager was impersonated" — a per-write attribution mechanism also unlocks "approve-on-behalf" without the security downsides.
- The same `SecurityAudit` subsystem later serves password resets, role changes, company-access grants/revokes, export events.
- Do **not** design for cross-company impersonation (Decision #9 HQ cross-company is a *read* path, not an identity path) — explicit non-goal, keeps the design small.

### 2.10 What the current implementation gets right

Credit where due — the foundation is reusable:

- Tenant-scoped target search (company filter, parameterized SQL, `using` blocks).
- Reuse of `AuthGuard.HasPermission` for the feature gate.
- No-impersonate-self, no nested-switch intent.
- Non-impersonated-state cleanup on rollback; `Session.Abandon()` fallback if original user vanished.
- UX model (search → select → switch; banner while impersonating; rollback affordance) is right and retained in §6.

---

## 3. Required Architectural Changes (summary)

| # | Change | Why | Non-negotiable? |
|---|--------|-----|-----------------|
| C1 | **Session model**: keep admin as authenticated principal; acting identity = explicit link record + resolver | Attribution, auditability, rollback safety, no forged sessions | **Yes** |
| C2 | **No forged `ActiveSessions` rows for the target** | Authentication lifecycle integrity | **Yes** |
| C3 | **`SecurityAudit` append-only audit subsystem**, fail-closed, same-transaction | Auditability | **Yes** |
| C4 | **Per-act authorization + target superset rule** | Prevent privilege escalation | **Yes** |
| C5 | **Rollback re-derives identity from DB**, not session snapshot | Stale-grant resurrection | **Yes** |
| C6 | **`SecurityContext` resolver**: single accessor for acting identity, injected at one point (`Bill.Master` + `SecurePage.OnInit`), pages unchanged | Containment; 188 pages must not each be edited | Yes |
| C7 | **CSRF-proof intent token** per impersonation link | T5 | Yes |
| C8 | Target eligibility checks (active, no pending reset, not kiosk, not another impersonation holder) | T1, T9 | Yes |
| C9 | Admin-banner + target-user-visible notification preserved (notification table is *fine for user-visible notice*, wrong for audit) | Transparency | Recommended |
| C10 | Concurrency rule: one active impersonation link per admin session; per-target links allowed | T8 | Recommended |

---

## 4. Session Model Recommendation

### 4.1 Principle

> **One browser session = one authenticated principal (the admin). Impersonation is an observation context, not a login.**

### 4.2 What an impersonation act is

An **ImpersonationLink** row: `LinkToken` (random 256-bit), `AdminUserId` (db id), `TargetUserId` (db id), `CompanyID`, `StartedUtc`, optional `EndUtc`/`EndReason`, `IsActive`. Created only via the Switch User page after: (a) re-validated admin session, (b) per-act `HasPermission("SwitchUser")` (already needed for page), (c) target eligibility (§4.6), (d) superset rule (§2.3).

### 4.3 What the admin's session keeps vs. gains

- Keeps: `Session["USERID"]` = **admin's** user id (unchanged through the whole lifecycle), `Session["SessionToken"]` = admin's real token, roles, company context.
- Gains (only while a link is active): `Session["ImpersonationToken"]` = `LinkToken`.
- Adds: `Session["User_Db_Id"]` — **close the admin business-key ↔ db-id gap**. Today `ResolveUserDbId` re-queries on every `AuthGuard` call. Store the admin's `tbl_login.Id` at login and reuse. This is a baseline-quality improvement on its own (removes one query from every AuthGuard call today).

### Session keys after the change:

| Key | During impersonation | Normal session |
|-----|----------------------|----------------|
| `USERID` | admin (never changes) | user |
| `SessionToken` | admin's real token | user's token |
| `User_Db_Id` | admin db id (cached at login) | user db id |
| `ImpersonationToken` | LinkToken | absent |
| `SwitchedFrom` / `Original*` snapshot keys | **removed** (dead) | n/a |

### 4.4 Identity resolution — the single choke point

New static resolver class `SecurityContext` (next to `AuthGuard`):

```
ResolveUserId()      → impersonation link active ? target.User_Id : Session["USERID"]
ResolveUserDbId()    → link active ? link.TargetUserId : Session["User_Db_Id"]
ResolveRoleContext() → link active ? target's roles/permissions : admin's
ResolveActor()       → admin's User_Id ALWAYS (for audit columns)
ResolveLink()        → the active link or null (single query per request, no caching beyond request)
```

Injection points (the only two places that consume it):
1. `Bill.Master.cs` — sets a per-request ambient context (`HttpContext.Items["SecurityContext"]`) after its existing validation; pages do not change.
2. `SecurePage.OnInit` (and `AuthGuard.EnsurePage`) — validates that when a link is active, the **target** holds the page's `RequiredPermissionKey`; the admin's own permissions are irrelevant to page access while observing.

Pages keep reading `Session["USERID"]` today — but the standard for new code and for the pages touched by later phases is `SecurityContext.ResolveUserId()`. A migration is a **find-replace of `Session["USERID"]` → `SecurityContext.ResolveUserId()`** per page, phaseable page-family by page-family. (Deployment note: master and SecurePage never write session keys — the per-request swap shortcut is forbidden; pages that need the acting identity call the resolver. Phase-scoped rollout is specified in §8 Phase 2.)

> **Architect's note (self-correction, kept for reviewers):** the tempting shortcut — have `Bill.Master` copy the target's identity into `Session["USERID"]` per request — is explicitly **forbidden**. That re-creates identity swap with extra steps and destroys attribution again. The resolver must be called at the point of use.

### Session-flow diagram

```
Browser (admin) ──cookies──▶ IIS worker ──▶ Session state (admin principal, never swapped)
                                                │
                        Bill.Master / SecurePage (OnInit) ── one query: link by ImpersonationToken
                                                │
                                HttpContext.Items["SecurityContext"]
                                                │
            ┌───────────────────────────────────┴───────────────────────────────────┐
            ▼                                     ▼                                 ▼
   page code-behind reads                resource/owner checks               audit columns
   SecurityContext.ResolveUserId()       via SecurityContext                 ResolveActor() = admin
```

### 4.5 Rollback re-derivation (no snapshots)

Rollback: end link (set `EndUtc`, `EndReason='UserEnded'`, `IsActive=0`) → **re-derive** admin session validity via existing `TryValidateSession` + `UserCompanyAccess` + `UserRoles` → if admin's access was revoked mid-impersonation, treat as logout (fail closed). Nothing is restored from session snapshot. The admin's real session was never modified, so there is nothing to restore.

### 4.6 Target eligibility rule (per act, server-side)

A target is eligible only if ALL:
1. Same company as the impersonation context (`tbl_login.CompanyID = @CompanyID`) AND admin has `UserCompanyAccess` to that company (already true) AND target has an active `UserCompanyAccess` row for that company (new check — prevents impersonating users who can no longer access the company).
2. `tbl_login.IsActive = 1` and no active `PasswordResetTokens` row (no pending password reset).
3. Target is not currently impersonating anyone (target is principal in no active link) and is not a target of another admin's active link — i.e., **one active link per target** (prevents confusing multi-admin observation of same account; T8 → design decision, not just perf).
4. **Superset rule**: every PermissionKey the target holds (via `UserRoles → RolePermissions`) is also held by the admin. Fails closed on any query error. Rationale: impersonation must not be a privilege *acquisition* path.
5. Target is not the card-kiosk identity space (`tbl_card_login` is a separate schema — never part of ERP impersonation; baseline).
6. Target ≠ admin (existing check).

### 4.7 CSRF / intent token

The switch act and the rollback act each carry a per-link random token (`IntentToken`), validated server-side against the expected link state; the switch page's `IntentToken` is issued on `GET` and consumed on the next post. Do not rely on EventValidation alone.

---

## 5. Database Changes

All changes additive; no alteration of existing columns. Idempotent scripts in `db/`, follow the `db/pservice_snapshot.sql` style (When/Why/What header, "DO NOT execute until reviewed").

### 5.1 New table `dbo.SecurityAudit` (append-only)

| Column | Type | Notes |
|---|---|---|
| `Id` | BIGINT IDENTITY PK | |
| `EventUtc` | DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME() | |
| `EventType` | NVARCHAR(50) NOT NULL | `ImpersonationStart`, `ImpersonationEnd`, `ImpersonationDenied`, `ImpersonationExpired` |
| `ActorUserId` | INT NOT NULL | admin `tbl_login.Id` |
| `ActorUser_Id` | NVARCHAR(100) NOT NULL | business key, denormalized for readability |
| `TargetUserId` | INT NULL | target db id, NULL for denied |
| `TargetUser_Id` | NVARCHAR(100) NULL | business key, denormalized |
| `CompanyID` | INT NOT NULL | |
| `SessionToken` | UNIQUEIDENTIFIER NOT NULL | admin's real session token |
| `LinkToken` | UNIQUEIDENTIFIER NULL | impersonation link, NULL when denied |
| `Reason` | NVARCHAR(500) NULL | admin-supplied reason, required on start |
| `DenialReason` | NVARCHAR(100) NULL | machine-readable denial cause |
| `IPAddress` | NVARCHAR(50) NULL | |
| `UserAgent` | NVARCHAR(500) NULL | |

Rules: no UPDATE/DELETE grants to the app login (DBA-enforced), no FK to `tbl_login` (append-only must survive user deletion), index on `(EventType, EventUtc)`, `(ActorUserId, EventUtc)`, `(TargetUserId, EventUtc)`.

### 5.2 New table `dbo.ImpersonationLink`

| Column | Type | Notes |
|---|---| hot |
| `LinkToken` | UNIQUEIDENTIFIER PK (newid, app-generated) | the only secret; rotate per link |
| `AdminUserId` | INT NOT NULL | db id |
| `TargetUserId` | INT NOT NULL | db id |
| `CompanyID` | INT NOT NULL | |
| `IntentToken` | CHAR(64) random hex | CSRF/intent proof, validated once |
| `StartedUtc` | DATETIME2(3) NOT NULL | start of the link |
| `LastUsedUtc` | DATETIME2(3) NULL | touched per request (see §8 perf note) |
| `EndUtc` / `EndReason` | nullable | `UserEnded`, `SessionEnded`, `AdminSessionExpired`, `Revoked`, `TimedOut` reasons |
| `IsActive` | BIT NOT NULL | |
| `Reason` | NVARCHAR(500) NOT NULL | required at start |

Index: `IX_ImpersonationLink_Token (LinkToken) INCLUDE (AdminUserId, TargetUserId, CompanyID, IsActive)` — the per-request lookup; plus `(TargetUserId) WHERE IsActive=1` and `(AdminUserId) WHERE IsActive=1` filtered indexes for eligibility rule §4.6.3.

### 5.3 Column additions

- `tbl_login.ActiveLinkToken UNIQUEIDENTIFIER NULL` — **No.** Rejected: hot-path denormalization that can go stale. Do not add.

### 5.4 Permission seed

- `Permissions.PermissionKey = 'SwitchUser'` (the existing `SwitchUser_permission.sql` is correct in shape; keep it, add module path `Administration > User Management > Switch User`).
- Add `ImpersonationAudit` permission key for the audit viewer page (§6.3) — grant to auditors/compliance, distinct from the switch permission (separation of duties: those who can audit need not be able to impersonate).

### 5.5 Explicit non-changes

- `ActiveSessions` — **no schema change**. Forged rows (current code) are eliminated by design; `TryValidateSession` unchanged.
- No triggers on `tbl_login` or business tables.
- No changes to `UserRoles`/`RolePermissions`/`UserCompanyAccess`/`Roles`/`Permissions` other than the seed row.

---

## 6. UI Interaction Model

### 6.1 Entry — existing pattern retained

Header link (admin, not currently impersonating) → `SwitchUser.aspx`:
- **Reason is mandatory** (free text, 500 chars, logged on `SecurityAudit`), provided on the switch confirmation step.
- Search (name / User_Id, `LIKE` with leading wildcard on exact columns, TOP 50, same-company, excludes self) — current implementation's grid is fine.
- Eligibility badge per row (e.g., "Eligible" / "Blocked: pending password reset" / "Blocked: broader permissions") computed server-side per §4.6, rendered as disabled row + reason. Never render a Switch button for ineligible targets.
- **Confirmation step**: selected user + reason text + explicit confirm button (intent token consumed). No one-click switching.

### 6.2 While impersonating

- Persistent **banner** on every page (master-level), high-contrast, non-dismissible: "You are observing as **{target}** · Reason: {reason} · [End Observation]" — the §6.1 reason shown back to the admin. The current banner (only on SwitchUser page) is insufficient; move to master.
- The Switch User header link **hides** while a link is active (current `UpdateSwitchUserVisibility` behavior — retained).
- Logout as admin while observing: **ends the link first, then logs out** (link + audit `EndReason='SessionEnded'` in same transaction).

### 6.3 Audit visibility (Phase 3)

Read-only page (e.g., `ImpersonationAudit.aspx`) gated by `ImpersonationAudit` permission: filter by actor/target/company/date, shows `SecurityAudit` events. Optional: show a "current impersonations" live panel (from active links) for security admins.

### 6.4 Target-user transparency (Phase 3, recommended)

Optional per-company setting: on the target's next real login, show a non-blocking notice: "Your account was observed by an administrator on {date} for: {reason}." Uses the notification subsystem (correct use of `tbl_SystemNotification`).

---

## 7. Rollback Sequence (normative)

**Trigger:** admin clicks "End Observation" (banner) — or automatically: admin session expiry/logout, admin permission revoked, admin company access revoked, link timeout, target ineligibility arises (e.g., password reset initiated mid-observation → revoke link).

1. Validate admin session (`TryValidateSession`) — if invalid, the link is already doomed; just end it.
2. Load active link by `ImpersonationToken` **AND** `AdminUserId = current principal` (prevents ending someone else's link).
3. Server-side: `IsActive=0`, `EndUtc=now`, `EndReason` per trigger.
4. `SecurityAudit` insert: `ImpersonationEnd` (same transaction).
5. Clear `Session["ImpersonationToken"]`.
6. Re-derive admin authorization *fresh* (`UserCompanyAccess`, `UserRoles`): if admin now fails, **do not attempt restore** — redirect to logout. If pass, redirect to home as admin.
7. If the admin session is abandoned mid-request (e.g., app-pool recycle), the link remains active but is revalidated lazily: no valid admin session may present that token, and the sweeper (§5.2/§8 Phase 1) ends it after the session-timeout window.

**Critical invariant:** at no point during rollback is any identity value written from session snapshot. The admin's real session was never mutated; there is nothing to restore.

---

## 8. Implementation Phases

> Each phase is independently releasable, testable, and reversible. Phase 1 must land **before** any UI ships. Do not ship the current `SwitchUser.aspx.cs` in any environment.

### Phase 0 — Immediate: neutralize the committed implementation (hotfix)
1. **Remove the switch/rollback write paths** (`btnSwitch_Click`/`btnSwitchBack_Click` bodies) from `SwitchUser.aspx.cs` — or gate the page out of the menu and deny it in `SecurePage` (fail closed). Keep the page shell + permission seed.
2. Remove `Bill.Master` header link (or leave behind permission gate — permission is not yet granted to any role, so page is unreachable: verify no role has `SwitchUser` in UAT; **verify in `flamex_live`**).
3. No DB changes required; `SwitchUser_permission.sql` is inert until granted.
4. **Evidence required:** grep-based proof that no role has been granted `SwitchUser` in each environment before deploy; if granted anywhere, revoke before deploy.

### Phase 1 — Foundation (server-only, no UI)
1. `dbo.SecurityAudit` + `dbo.ImpersonationLink` (§5.1, §5.2) with indexes.
2. `SecurityContext` resolver class (§4.4) + `Session["User_Db_Id"]` set at login (index/login change only).
3. `ImpersonationService` (server-side component): `TryStart(admin, target, reason, intentToken)` and `TryEnd(linkToken)` implementing §4.2, §4.6, §7 — **all authorization, eligibility, audit in one component**; the page becomes a thin controller. All in one SQL transaction per act.4. Link sweeper (can be a lazy check in `SecurityContext` or a SQL Agent job): end stale links older than session timeout + grace. `ImpersonationExpired` audit events.
5. Unit-testable: eligibility/superset rule as pure functions over permission sets.

### Phase 2 — Page + master integration
1. Rewrite `SwitchUser.aspx` UI per §6.1 (search, eligibility badges, reason + confirm with intent token).
2. `Bill.Master`: banner (§6.2) driven by `SecurityContext.ResolveLink()`; header link visibility per §6.2.
3. `SecurePage`/`AuthGuard.EnsurePage`: when a link is active, page permissions resolve against the **target**; `ResolveActor()` used for all new audit columns.
4. **Page rollout scope for Phase 2** — the resolver migrates page-family by page-family; the decision and its reasoning are recorded below.

> **Resolution of the §4.4 note:** Phase 2 scope decision — `Session["USERID"]` remains the **admin's** value at all times (never overwritten). Pages that need the acting identity will read `Session["USERID"]` and get the admin's — which is **correct for attribution** (writes are attributed to the admin — the observer — not the target). For pages whose *display* must reflect the target (e.g., "My Sales Visits" showing the target's visits), the page must call `SecurityContext.ResolveUserId()`. Therefore:
> - **Phase 2 ships with resolver-based display scope on the self-service page family only** (`vw_dailyrpts`, `expense_entry`, attendance, home KPIs, SwitchUser itself) — the "self" page family. Manager/admin pages and all writes remain principal-based (attribution to admin) — deliberate: an impersonating admin **must not** be able to approve visits/expenses *as* the target (that is the T2/T1 mitigation; the admin would need their own `SwitchUser`+approval permissions anyway).
> - Wait — for the admin to *reproduce the target's view* (the point of the feature), the self-family must read the acting identity. For writes, they run as the admin. If a write endpoint reads `Session["USERId"]` for `CreatedByCode`, it records the **admin** — correct. If the same endpoint *reads* owner-scoped data via `SecurityContext`, it sees the target's data — correct. **The invariant: reads resolve to the acting identity; writes attribute to the principal.** This is the one-sentence design rule of the whole feature.

**Invariant (repeated for emphasis): READS resolve to the acting identity; WRITES attribute to the principal.**

### Phase 3 — Audit UI + transparency
1. `ImpersonationAudit.aspx` (§6.3) + `ImpersonationAudit` permission.
2. Target-user transparency notice (§6.4).
3. Optional: notify the target's reporting manager? — **No** (noisy). Keep to target + audit.

### Phase 4 (optional, later) — Dry-run telemetry
Count how often the feature is used, reasons distribution, and elapsed times from `SecurityAudit` (no code on business pages). Feeds the go/no-go for deeper "act-on-behalf" workflows.

---

## 9. Go/No-Go Recommendation

**Recommendation: CONDITIONAL NO-GO on the current implementation. GO for the re-architected design in this document.**

| Dimension | Current code | Spec (this doc) |
|---|---|---|
| Attribution on writes | Attributed to victim | Attributed to admin (principal) |
| Authenticated principal | Swapped to target | Admin always |
| Target session forged | Yes (`ActiveSessions` for target) | No — link table |
| Audit | Notification row, fail-open | `SecurityAudit`, fail-closed, append-only |
| Rollback | Session snapshot replay | DB re-derivation |
| Privilege escalation | Any user incl. admins | Superset rule, eligibility checks |
| CSRF/intent | None | Intent token per act |
| Per-request cost | — | One indexed link lookup |

**Conditions for GO:**
1. Ship Phase 0 (neutralize) immediately; the committed code must not reach `flamex_live` with write paths intact.
2. Land Phase 1 (session model + audit + service) before any impersonation UI reaches users.
3. Superset rule + eligibility + fail-closed audit are release blockers for Phase 2.
4. Impersonation is **never** permitted to grant write-authority over resources the admin could not already act on (superset rule enforces this).
5. `docs/22_Security_Baseline.md` §"Impersonation" section added with the read/write invariant, so future contributors cannot reintroduce identity swap.

**Final note:** the value of this feature is diagnostic *visibility*, not identity *possession*. The moment the system lets an administrator *be* another user, auditability and attribution are gone by construction. The link model gives support teams everything they need — see the user's data, reproduce their view — without ever becoming them.

---

*End of document. This is a specification — no production code was written or modified as part of this review.*
