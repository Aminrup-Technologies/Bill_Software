# 30 — Audit Architecture for Administrative Impersonation

| | |
|---|---|
| **Status** | **Design — canonical audit & attribution architecture for the Link Model** |
| **Author role** | Security / Compliance Architecture |
| **Depends on** | [`25`](25_Impersonation_Session_Model.md) (transitions T1–T11), [`26`](26_Impersonation_STRIDE.md) (repudiation threats R1–R3), [`27`](27_Impersonation_Database_Review.md) (SecurityAudit / ImpersonationLink schema, §5 correlation, §6 rollback), [`29`](29_Impersonation_Implementation_Plan.md) (acts that emit the events) |
| **Scope** | Lifecycle events, correlation IDs, session linkage, financial attribution, login-history integration, rollback logging, reporting queries, example timeline, compliance |
| **Ground rule** | The request "every action during impersonation must remain attributable" is satisfied **by construction** (identity never swaps), not by log volume — §1 defines exactly what is attributable and by which mechanism, including the honest gaps. |

---

## 0. Verified Ground Truths (carried from docs 27/29 code review)

| # | Fact | Consequence here |
|---|---|---|
| G1 | `dbo.SecurityAudit` (append-only, 14 cols incl. `LinkToken`, `SessionToken`, `DenialReason`) and `dbo.ImpersonationLink` (lease `AdminSessionToken`, `EndReason`, `IsActive`) are the doc 27 canonical objects | Event catalog fills **this** schema; no new tables |
| G2 | `dbo.ActiveSessions`: `SessionToken` PK, `UserId`, `LoginTime`, `LastHeartbeat`, `IPAddress`, `UserAgent`, `IsActive`, `CompanyID` (verified from the live INSERT in `SwitchUser.aspx.cs`) | Session linkage (§4) joins on real columns |
| G3 | `dbo.AuthAudit` exists (PK `AuditId`, 6 cols) — authentication events have a home; exact column set to be confirmed at implementation | Login-history integration (§6) is expressed as a contract on `SessionToken` + timestamp |
| G4 | Business attribution columns exist as *business keys* into `tbl_login.User_Id` — e.g. `tbl_SalesVisitReport.CreatedByCode/ApprovedBy`, `tbl_Expenses.UserCode/ApprovedBy`; `User_Id` has **no unique constraint** (UAT #44) | Financial attribution (§5) joins by business key + tenant + time window; uniqueness caveats addressed |
| G5 | Ponytail §2: major CRUD writes `tbl_SystemNotification` **before commit** (transactional audit) | During observation these rows become compensating evidence inside the window (§5.4) |
| G6 | Invariant (doc 23 §8 / doc 29 §2.3): **reads resolve to the acting identity; writes attribute to the principal** | The whole architecture rests on this one rule |
| G7 | End-reason vocabulary: `UserEnded, Logout, Superseded, SessionExpired, IdleTimeout, Recycle, Revoked, TimedOut, TargetIneligible`; denial taxonomy: `permission, target_ineligible, broader_permissions, target_in_use, already_observing, nested, self, intent_used` | Event catalog uses these verbatim |

---

## 1. The Attribution Principle and Its Coverage Map

> **One sentence:** identity never changes, so every write is attributable to the admin *by the same columns that attribute normal work* — impersonation adds a **context layer** (who was observed, when, why), not an attribution layer.

### 1.1 Mechanism inventory

| Action class during observation | Attribution mechanism | Evidence trail |
|---|---|---|
| Business write (visit, expense, quotation, invoice, payment, PO flow) | Existing `CreatedBy`/`ApprovedBy`-style columns record the **principal's** business key (`Session["USERID"]` is never swapped — G6) | Row + `SecurityAudit` window join (§5.3) |
| Approval/rejection executed through a page the target could see but the admin could not open alone | **Impossible to escalate**: the superset rule (doc 23 §2.3) guarantees the admin already holds every permission the target holds — the admin could have performed the identical write from their own session. The audit shows the admin as approver, correctly | Same as above; no "on-behalf" attribution exists by design (doc 23 Phase 2 note) |
| Impersonation lifecycle itself | `SecurityAudit` events (§2) | Start/End/Denied rows + link row |
| Session events (login, supersession, idle timeout, logout) | Existing login history: `AuthAudit` + `ActiveSessions` lifecycle (G2/G3) | §6 |
| Forced closures (target reset, revoke, timeout) | `ImpersonationEnd` with `EndReason` (§7) | Exactly-once pair with the link close |
| **Reads** (viewing the target's data) | **Not logged per page** — deliberately (doc 27 §5.2 residual note: rejected as a DoS/privacy trade). The **observation window** `[StartedUtc, EndUtc)` is the audit boundary for "what admin X could see of Y" | Link row |
| Actions with **no attribution column today** (legacy writes without `CreatedBy`-style fields, exports) | Honest gap — same gap exists for normal use, impersonation does not widen it; flagged in the security notice; future `SecurityAudit` event types (e.g. `ExportPerformed`) extend coverage additively (doc 27 §2.2 extensible `EventType`) | Future events |

### 1.2 What "every action attributable" means operationally

1. **Who acted:** always resolvable → principal business key on the row, or `ActorUserId/ActorUser_Id` on lifecycle events.
2. **Under what context:** link row + window (`LinkToken` groups everything; §3).
3. **On whose data:** target identity denormalized on every lifecycle event; ownership predicates resolved to the target for reads (§1.1).
4. **With what justification:** `Reason` (mandatory ≤500 chars) stored on the link, echoed on the banner (doc 28 §4.3), replayable in reports.
5. **From where:** `IPAddress` + `UserAgent` captured on lifecycle events.

---

## 2. Event Catalog (canonical)

Four event types. Doc 23's fifth name, `ImpersonationExpired`, is **reconciled away**: all closures are `ImpersonationEnd` + `EndReason` — see §2.3.

### 2.1 Catalog

| Event | Written when | Transaction scope | Actor semantics | Target semantics | `LinkToken` | Exactly-once guarantee | Failure semantics |
|---|---|---|---|---|---|---|---|
| `ImpersonationStart` | `TryStart` commit (all 9 gates passed, doc 29 §3.1) | Same TX as the link INSERT | Admin (principal), always populated | Populated | Set | 1:1 with link row (`UX` on link + conditional insert) | **Fail-closed** — audit insert failure aborts the switch (F7) |
| `ImpersonationEnd` | `TryEnd` / `TryClose` / heartbeat forced closure / sweeper close | Same TX as the link close | Admin (principal of the ending link) | Populated | Set | 1:1 with link closure (conditional close skips audit when 0 rows affected — doc 27 §6) | Link closes regardless of UI (lease-first); audit failure is logged server-side, link stays lease-dead (doc 25 §6.2) |
| `ImpersonationDenied` | Any `TryStart` gate failure | Standalone TX (no state changed) | Admin (attemptor) — populated even for unknown targets | Populated when known, NULL when invalid/unknown | **NULL** | One row per denial act | Never blocks the denial path; if this insert fails, the denial still happens and the failure is logged server-side (denials fail **open** for availability, **closed** for authorization) |
| *(reserved)* future types | e.g. `ExportPerformed`, `AuditViewed` | — | — | — | — | — | Additive rows only; no DDL (G1, doc 27 §9.3) |

### 2.2 Field-fill matrix (which columns each event must carry)

| Column | Start | End | Denied |
|---|---|---|---|
| `ActorUserId` / `ActorUser_Id` | **required** | **required** | **required** |
| `TargetUserId` / `TargetUser_Id` | **required** | **required** | required if resolvable, else NULL |
| `CompanyID` | **required** | **required** | **required** |
| `SessionToken` (admin's real session) | **required** | **required** | **required** |
| `LinkToken` | **required** | **required** | NULL |
| `Reason` | admin's justification (≤500) | **`EndReason` code** (machine-readable; original justification lives on the link row) | NULL |
| `DenialReason` | NULL | NULL | **required** (taxonomy G7) |
| `IPAddress` / `UserAgent` | **required** | required (request context when closed via request; NULL for sweeper closes) | **required** |

### 2.3 Vocabulary reconciliation (docs 23/25/27 → this doc)

| Legacy usage | Canonical form |
|---|---|
| `ImpersonationExpired` (doc 23 Phase 1.4 sweeper) | `ImpersonationEnd` with `EndReason ∈ {SessionExpired, IdleTimeout, Recycle, TimedOut}` — one closure event family, no duplicate type |
| `EndReason='TimedOut'` vs clock D | `TimedOut` (4 h cap); `SessionExpired`/`IdleTimeout`/`Recycle` are lease-death paths |
| Denial cause vs end reason | Denials are **attempts** (`ImpersonationDenied`, no link); ends are **state changes** on an existing link — never mixed |

---

## 3. Correlation Architecture

### 3.1 Identifier inventory

| ID | Minted by | Stored in | Lifespan | Correlation role |
|---|---|---|---|---|
| `SessionToken` (GUID) | Login (`index.aspx.cs`) | `ActiveSessions.SessionToken` PK; `Session["SessionToken"]`; `SecurityAudit.SessionToken`; link lease `AdminSessionToken` | Login → logout/supersession | **Principal anchor** — ties every event to a real authenticated session |
| `LinkToken` (GUID, CSPRNG) | `ImpersonationService.TryStart` | `ImpersonationLink.LinkToken` (UQ); `Session["ImpersonationToken"]` (pointer only); `SecurityAudit.LinkToken` | Link start → close | **Grouping key** — all events of one observation act |
| `IntentToken` (hex-64, single-use) | Switch page (GET) | Link row; consumed at `TryStart` | One act | Proves admin intent (anti-CSRF/replay); auditable proof the UI flow was honored |
| `ImpersonationLinkId` / `SecurityAudit.Id` (BIGINT IDENTITY) | Engine | PKs | Permanent | Total ordering; tie-breaker when timestamps collide |

### 3.2 Join graph (the three-key chain, completed)

```
AuthAudit / login event ──(principal)──► SessionToken ──► ActiveSessions row (G2)
                                                │ lease (AdminSessionToken)
                    ImpersonationLink ◄─────────┘   (LinkToken = grouping key)
                          │
        ┌─────────────────┼──────────────────────────────────────┐
        ▼                 ▼                                      ▼
SecurityAudit.Start   SecurityAudit.End                business rows written in
(LinkToken = L)       (LinkToken = L, EndReason)       [StartedUtc, EndUtc) whose
        │                                              CreatedBy-style column =
        └────────────► window join ◄────────────────── principal's business key (§5.3)
```

### 3.3 Correlation rules (no request-ID plumbing exists — deliberately none added)

| Situation | Correlation method |
|---|---|
| All events of one observation act | `LinkToken` equality + `Id` ordering (§3.1) |
| Denials (no `LinkToken`) | `(SessionToken, EventUtc, TargetUser_Id)` proximity — a denial is a point event, not a span |
| Ordered reconstruction | `SecurityAudit.Id` (identity order) as tie-breaker; all clocks are DB-side `SYSUTCDATETIME()` (doc 25 E8) |
| A request ID per HTTP request | **Rejected for v1** — no correlation infrastructure exists in the app today; adding it is a separate platform initiative. `LinkToken` + window provide the required attribution without it |

---

## 4. Session Linkage

### 4.1 Chain (each arrow is a real column join, no denormalized pointers)

```
[login]  AuthAudit event ─┐
                          ├─ SessionToken S1 ─ ActiveSessions(S1: UserId=7, CompanyID=1, IsActive=1)
[browser] Session["SessionToken"]=S1 ─ Session["UserDbId"]=7 ─ Session["ImpersonationToken"]=L (pointer only, L2)
[lease]  ImpersonationLink(LinkToken=L, AdminSessionToken=S1, AdminUserId=7, TargetUserId=12, IsActive=1)
[events] SecurityAudit(SessionToken=S1, LinkToken=L, ActorUserId=7, TargetUserId=12)
```

Properties (verified mechanics):

1. **Lease:** every link resolution requires `AdminSessionToken = S1 AND S1.IsActive = 1` (doc 25 §1.2) — the link is structurally unable to outlive the principal session.
2. **Supersession integration:** a second login kills S1 (last-login-wins, G4 of doc 25) → the next resolution/heartbeat closes the link `Superseded` → the login history itself explains the closure. No special logging needed: the cause lives in session history, the effect in `SecurityAudit`.
3. **One browser, one chain:** two tabs share S1 and L; two browsers cannot (second login supersedes) — the chain is automatically single-threaded per principal.
4. **Kiosk exclusion:** `tbl_card_login` / `index_card` are a disjoint realm (baseline) — they never appear in this chain, and no kiosk event can ever carry a `LinkToken`.

---

## 5. Financial Transaction Attribution

Financial flows (quotation → DPCC/proforma → tax invoice → payment received; vendor PO → purchase payment; expense claims) are the highest-stakes attribution surface.

### 5.1 The rule and why it is safe

Writes during observation record the **admin** in the existing attribution columns (G6). Two questions a regulator asks are answered by two different keys:

| Question | Answer source |
|---|---|
| "Which natural person performed this write?" | The row's `CreatedBy`/`ApprovedBy`-style column → the admin. Always. |
| "Was this write performed while observing someone?" | `SecurityAudit` window join (§5.3): row time ∈ `[StartedUtc, EndUtc)` of a link whose `ActorUserId` = that admin |

### 5.2 Why write-through-observation cannot escalate

The page gate while observing resolves to the **target** (INV-4), so an admin can *open* an approval page the target could see. The write, however, is only executable because the **superset rule** (doc 23 §2.3) guarantees the admin holds every permission the target holds — the admin could have executed the identical write from their own session. Therefore:

- No write exists that observation *enables* but normal authority doesn't.
- No write is ever attributed to the target (the identity-swap failure mode, doc 23 §1.3, is structurally gone).
- "Approve-on-behalf of the target" is **not a feature**: an approval made while observing is the **admin's own approval**, and the audit shows it that way.

### 5.3 The window-join method (canonical reconstruction)

```
For each financial table F with attribution column A (business key) and timestamp T:

  SELECT F.*, L.LinkToken, L.Reason, L.StartedUtc, L.EndUtc
  FROM F
  JOIN ImpersonationLink L
    ON L.AdminUserId = @adminDbId
   AND L.IsActive = 0                        -- closed links only (stable window)
   AND F.T BETWEEN L.StartedUtc AND L.EndUtc
  WHERE F.A = @principalBusinessKey
    AND F.CompanyID = @companyId             -- tenant filter per Ponytail §1
  ORDER BY F.T;
```

Requirements on the data for this to work (all verified or catalog-confirmed):

| Requirement | Status |
|---|---|
| Attribution column holds the **principal's** business key | Guaranteed by G6 (session never swapped) |
| Row timestamp exists (create/approve time) | Present on the financial flows (catalog: `UAT_CATALOG` / `DATA_DICTIONARY`); flows lacking timestamps are §1.1's honest gap |
| Tenant column present | Ponytail §1 — every financial table is company-scoped |
| `User_Id` non-uniqueness (G4) | Joins also filter `CompanyID` (and the audit viewer always presents `ActorUser_Id` + `ActorUserId` together), matching how `AuthGuard.HasPermission` already disambiguates |

### 5.4 Compensating evidence: transactional notifications

Ponytail §2 already forces a `tbl_SystemNotification` row before every major CRUD commit (G5). During observation these rows are written with the principal's identity **inside the window** — giving auditors a second, app-mandated evidence line per financial action that requires no schema change and no new logging code. Reports (§10, Q4) cross-check row-vs-notification counts within a window as a reconciliation control.

### 5.5 Explicit non-designs (recorded so they are not "improved" later)

- **No `PerformedDuringImpersonation` bit / `ActorLinkId` column on business tables** — 111-table churn, write-path changes on every financial page, and a second copy of truth that can drift. The window join is authoritative. (Doc 27 §2.5 stands.)
- **No per-read logging** — doc 27 §5.2 residual note stands; the window is the read boundary.
- **No "on-behalf" approval semantics** — §5.2; if the business later wants approve-as-target, that is a *new* feature with its own consent/audit design, explicitly out of scope.

---

## 6. Login History Integration

### 6.1 Contract

| Integration point | Mechanism |
|---|---|
| Principal authentication | `SecurityAudit.SessionToken` ↔ `ActiveSessions.SessionToken` ↔ login event in `AuthAudit` (G3). The join contract requires only `SessionToken` + a timestamp — properties any login ledger must carry; `AuthAudit`'s exact columns are confirmed at implementation |
| Login of the **target** while being observed | Independent chain — the target's login kills only the target's own prior session rows (G4) and never touches the link (INV-3/8, doc 25 §7). Auditors can see "target logged in at T while admin observed" only as two independent, timestamp-correlatable chains — this is by design (no target-session row is ever forged) |
| Target transparency notice (doc 23 §6.4, Phase 3) | Uses `IX_SecurityAudit_Target` to find `ImpersonationEnd` events for the user and feeds the `tbl_SystemNotification` notice on next real login |
| Supersession / idle-timeout / logout | Existing login-history mechanics; the link closure lands in `SecurityAudit` with the matching `EndReason` (§7) |
| Kiosk logins | Excluded realm — never joined, never attributed (§4.1.4) |

### 6.2 What an auditor can reconstruct about any session

Login time/identity → session lifetime (heartbeat ledger) → whether any observation overlapped (link row + events) → every write by that session inside observation windows (§5.3) → how the session ended. All from existing tables plus the two new ones — no log shipping, no SIEM dependency (SIEM export can consume the same reporting views later; out of scope).

---

## 7. Rollback and Forced-Closure Logging

### 7.1 The exactly-once pair

Every started link ends exactly once, as **one transaction = conditional link close + one `ImpersonationEnd` row** (doc 27 §6):

| Closer | Path | `EndReason` | `UserAgent/IP` |
|---|---|---|---|
| Admin (button) | `TryEnd` | `UserEnded` | request context |
| Admin logging out | master logout step 1 (doc 25 §6) | `Logout` | request context |
| Lease death (second login) | lazy close on resolution/heartbeat | `Superseded` | NULL |
| 30-min idle | heartbeat kill → close | `IdleTimeout` | NULL |
| App-pool recycle orphan | sweeper | `Recycle` | NULL |
| ASP.NET session death | lazy/sweeper | `SessionExpired` | NULL |
| 4-h cap | heartbeat | `TimedOut` | NULL |
| Target reset/deactivate/membership loss | lazy close (§2.2 step 4, doc 29) | `TargetIneligible` | NULL |
| Permission revoked mid-observation | lazy close | `Revoked` | NULL |

### 7.2 Cause vs effect (compliance nuance)

`Revoked` and `TargetIneligible` record the **effect** on the link. The *cause* (who revoked the permission; who initiated the password reset) lives in the RBAC/reset history — `ManageRoles`/`ViewUser` writes and `PasswordResetTokens` respectively. Reports join across these ledgers by timestamp (§10 Q6). `SecurityAudit` deliberately does not duplicate authorship of events owned by other subsystems.

### 7.3 Rollback honesty rules

- A failed close (F8) leaves the banner up; no `ImpersonationEnd` is written until the conditional close affects a row — audit and state can never disagree.
- Re-opening an observation after a rollback is a **new link, new `LinkToken`** — history never chains observations into one id; each act stands alone (repudiation-safe: no "which session was I in?" ambiguity).
- Nothing is ever *restored* on rollback (doc 23 §7) — so there is no "rollback write" to log beyond the closure pair; the next request re-derives the principal fresh.

---

## 8. Audit Schema (as-mounted for this architecture)

### 8.1 Objects (doc 27 §2, unchanged here — restated as the audit surface)

- `dbo.SecurityAudit` — append-only event rows (§2.2 fill matrix). Enforcement: no UPDATE/DELETE grant to the application login; no FKs; DBA-enforced immutability. Indexes: `IX_SecurityAudit_Link (LinkToken) WHERE NOT NULL`, `Actor (ActorUserId, EventUtc DESC)`, `Target (TargetUserId, EventUtc DESC) WHERE NOT NULL`, `Type_Time`, `Time` (doc 27 §3.2).
- `dbo.ImpersonationLink` — constrained state; the grouping backbone. `UX` filtered unique indexes make concurrency events (`target_in_use`, `already_observing`) deterministic.

### 8.2 Retention (doc 27 §7, restated as compliance posture)

| Data | Window | Rationale |
|---|---|---|
| `ImpersonationLink` | 24 months online → archive | Operational telemetry; windows remain joinable via archive (§5.3 note) |
| `SecurityAudit` | Permanent online (≥ 7 years if aligned to financial-audit policy) | Append-only; projected < 1 M rows at 7 years (doc 27 §7.1) |
| Archive tables | Indefinite, cold, restorable into the viewer | No FKs; never altered after close |

---

## 9. Example Audit Timeline (synthetic, internally consistent)

Actors: admin **RAHUL** (`User_Id='FLM01'`, `Id=7`), targets **FATIMA** (`'FLM12'`, `Id=12`, Salesperson) and **FAIZAN** (`'FLM08'`, `Id=8`, Manager). Company `1` ("AA Exports"). Session token abbreviated `S1`, link `L-9F3E`, second link `L-C41D`.

| UTC | Ledger | Row (key fields) |
|---|---|---|
| 09:00:12 | `AuthAudit` / `ActiveSessions` | RAHUL login → `S1` minted, `CompanyID=1`, IP `10.4.2.31` |
| 09:14:03 | `SecurityAudit` | `ImpersonationDenied` · Actor 7/FLM01 · Target 8/FLM08 · `DenialReason='broader_permissions'` · `LinkToken=NULL` · S1 |
| 09:15:02 | `ImpersonationLink` | INSERT `L-9F3E` · Admin 7 · Target 12 · Reason "Q3 expense audit for FLM12" · Intent consumed · `IsActive=1` |
| 09:15:02 | `SecurityAudit` | `ImpersonationStart` · S1 · `L-9F3E` · IP/UA captured |
| 09:22:10 | `tbl_Expenses` | Expense 4021 approved · `ApprovedBy='FLM01'` **(principal, not FLM12)** · CompanyID 1 |
| 09:22:10 | `tbl_SystemNotification` | transactional audit row for the approval (G5) · CreatedBy FLM01 · inside window |
| 09:31:55 | `ImpersonationLink` | `L-9F3E` closed · `EndUtc` · `EndReason='UserEnded'` · `IsActive=0` |
| 09:31:55 | `SecurityAudit` | `ImpersonationEnd` · S1 · `L-9F3E` · `Reason='UserEnded'` |
| 10:02:00 | `ImpersonationLink` | INSERT `L-C41D` · Admin 7 · Target 12 · "Recheck disputed claim" · `IsActive=1` (+ Start event) |
| 11:40:12 | `PasswordResetTokens` | FATIMA requests reset → target ineligible (F4) |
| 11:41:07 | Heartbeat → `SecurityAudit` | `L-C41D` closed `TargetIneligible` · `ImpersonationEnd` · UI toast (doc 28 §5.3) |

Reading the ledger: two denials-vs-ends never mix (§2.3); each observation act has its own token; the expense approval is attributed to FLM01 and contextualized by the window of `L-9F3E`; the target's account actions never appear as impersonation events.

---

## 10. Reporting Queries (reference, read-only, all index-aligned and date-bounded)

```sql
-- Q1: Full timeline of one observation act (viewer default drill-down)
SELECT a.EventType, a.EventUtc, a.ActorUser_Id, a.TargetUser_Id,
       a.DenialReason, a.Reason, a.IPAddress
FROM dbo.SecurityAudit a
WHERE a.LinkToken = @LinkToken
ORDER BY a.Id;                                    -- IX_SecurityAudit_Link

-- Q2: Everything admin X observed, ever (compliance review by actor)
SELECT l.LinkToken, l.StartedUtc, l.EndUtc, l.EndReason,
       l.Reason, t.ActorUser_Id, t.TargetUser_Id
FROM dbo.ImpersonationLink l
JOIN dbo.SecurityAudit t ON t.LinkToken = l.LinkToken AND t.EventType = 'ImpersonationStart'
WHERE l.AdminUserId = @AdminDbId
  AND l.StartedUtc >= @From AND l.StartedUtc < @To
ORDER BY l.StartedUtc DESC;

-- Q3: Who observed user Y, and when (target transparency / HR)
SELECT t.EventUtc, t.EventType, t.ActorUser_Id, l.Reason, l.EndReason
FROM dbo.SecurityAudit t
LEFT JOIN dbo.ImpersonationLink l ON l.LinkToken = t.LinkToken
WHERE t.TargetUserId = @TargetDbId
  AND t.EventUtc >= @From AND t.EventUtc < @To
ORDER BY t.EventUtc;                              -- IX_SecurityAudit_Target

-- Q4: Financial writes performed while observing (window join, §5.3)
SELECT e.Id, e.ApprovedBy, e.UpdatedUtc, l.LinkToken, l.Reason, l.StartedUtc, l.EndUtc
FROM dbo.tbl_Expenses e
JOIN dbo.ImpersonationLink l
  ON l.AdminUserId = @AdminDbId AND l.IsActive = 0
 AND e.UpdatedUtc BETWEEN l.StartedUtc AND l.EndUtc
WHERE e.ApprovedBy = @PrincipalBusinessKey
  AND e.CompanyID = @CompanyId
ORDER BY e.UpdatedUtc;
-- (repeat the pattern per financial table; notification reconciliation:
--  count(tbl_SystemNotification within window, CreatedBy=principal)
--  vs count(F rows in window) as a consistency check)

-- Q5: Denial analytics (control-health monitoring)
SELECT DenialReason, COUNT(*) AS Denials, MAX(EventUtc) AS LastSeen
FROM dbo.SecurityAudit
WHERE EventType = 'ImpersonationDenied'
  AND EventUtc >= @From
GROUP BY DenialReason
ORDER BY Denials DESC;                            -- IX_SecurityAudit_Type_Time

-- Q6: Why did links end this period? (cause joins other ledgers by time)
SELECT l.EndReason, COUNT(*) AS Cnt
FROM dbo.ImpersonationLink l
WHERE l.EndUtc >= @From AND l.EndUtc < @To
GROUP BY l.EndReason;
-- Revoked/TargetIneligible rows: join RBAC-change history /
-- PasswordResetTokens by timestamp for the causing actor (§7.2)

-- Q7: Reconciliation — links without closes (should always be zero rows)
SELECT l.LinkToken, l.StartedUtc
FROM dbo.ImpersonationLink l
LEFT JOIN dbo.SecurityAudit e ON e.LinkToken = l.LinkToken AND e.EventType = 'ImpersonationEnd'
WHERE l.IsActive = 0 AND e.Id IS NULL;           -- integrity alarm feed

-- Q8: Concurrent-observation sanity (one live link per target / per admin)
SELECT TargetUserId, COUNT(*) AS LiveLinks
FROM dbo.ImpersonationLink WHERE IsActive = 1 GROUP BY TargetUserId
HAVING COUNT(*) > 1;                              -- alarm feed (should be empty)
```

Presentation rules: every report displays `ActorUser_Id` **and** `TargetUser_Id` together (never one alone); all queries take mandatory date bounds and `TOP`/paging; the viewer page is gated by `ImpersonationAudit` (SoD, doc 27 §4) and itself becomes a candidate future `AuditViewed` event (§2.1).

---

## 11. Compliance Considerations

| Theme | Position in this design |
|---|---|
| **Attribution to a natural person** | Principal-attribution invariant (G6) + window join — every financial action resolves to the admin, with observation context. Satisfies "who did it" and "under what authority" without per-row markers |
| **Purpose limitation / justification** | Reason mandatory at start, ≤500 chars, non-editable afterwards; echoed on-screen (doc 28) and on the link row. Reports surface it on every act |
| **Integrity of the audit trail** | Append-only enforcement (grants), no app UPDATE/DELETE path, DB clocks only, exactly-once pairs (§7), reconciliation queries Q7/Q8 as continuous alarms |
| **Data minimization** | Captured per event: identity ids, business keys, tenant, tokens, reason, IP, UA. No page-content, no read-level logging (privacy + DoS trade, doc 26), no payloads |
| **Transparency to the data subject** | Target notification on next login (doc 23 §6.4) driven by `IX_SecurityAudit_Target`; observation is discoverable by the person observed |
| **Retention** | Links 24 months online + permanent archive; events permanent/7-year alignable (§8.2) — configurable per company policy at implementation |
| **Separation of duties** | `SwitchUser` ≠ `ImpersonationAudit` (doc 27 §4); those who can observe cannot silently re-shape their own audit surface |
| **Control health** | Denial-rate monitoring (Q5), orphan-link alarm (Q7), concurrency alarm (Q8), notification reconciliation (Q4) — audit *of the mechanism*, not only of the acts |
| **Known, accepted gaps** (must appear in the feature's security notice) | Reads are not per-page logged (window boundary instead); actions in flows without attribution columns/timestamps are not window-joinable (§1.1); who *queried* the audit viewer is not itself audited in v1 (future `AuditViewed`); `AuthAudit` column contract to be confirmed (§6.1) |
| **Out of scope, by boundary** | SIEM/log-shipping integration; DBA operational-access auditing of `SecurityAudit` (a DBA-side control); cross-company impersonation (explicit non-goal, doc 23 §2.9) |

---

## 12. Requirement Traceability

| Requested deliverable | § |
|---|---|
| Impersonation lifecycle events | §2 (catalog, fill matrix, vocabulary reconciliation) |
| Correlation IDs | §3 (inventory, minting, join graph, rules) |
| Session linkage | §4 |
| Financial transaction attribution | §5 (invariant, superset safety, window join, compensation, non-designs) |
| Login history integration | §6 |
| Rollback logging | §7 (exactly-once pairs, cause vs effect) |
| Reporting queries | §10 (Q1–Q8) |
| Event catalog | §2 |
| Audit schema | §8 (as-mounted surface + retention) |
| Example audit timeline | §9 |
| Compliance considerations | §11 |

---

*End of document. Architecture only — no schema was created, no query was executed, and no code was modified in producing this design. The queries in §10 are reference reports to be implemented against the doc 27 schema.*
