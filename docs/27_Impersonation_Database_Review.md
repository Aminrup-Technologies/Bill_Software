# 27 — Database Architecture for Administrative Impersonation

| | |
|---|---|
| **Status** | **Design — canonical database specification for the Link Model** |
| **Author role** | Database / Identity Architecture |
| **Depends on** | [`23_Impersonation_Architecture_Review.md`](23_Impersonation_Architecture_Review.md) (feature design, §4/§5), [`24_Impersonation_Lifecycle.md`](24_Impersonation_Lifecycle.md) (invariants INV-1…INV-12), [`25_Impersonation_Session_Model.md`](25_Impersonation_Session_Model.md) (session mechanics, L1–L6 layers), [`26_Impersonation_STRIDE.md`](26_Impersonation_STRIDE.md) (threat mitigations) |
| **Scope** | ER model, new tables, indexes, foreign keys, permission model, audit correlation, rollback tracking, retention, migration plan, backward compatibility, performance |
| **Constraint** | Architecture documentation only. **No implementation SQL in this document.** Column tables and index specifications are the deliverable; the executable script (`db/impersonation_foundation.sql`) is authored at implementation time in the [`db/pservice_snapshot.sql`](../db/pservice_snapshot.sql) house style. |
| **Supersedes** | §5 sketches in doc 23 where they differ (see §2.4 reconciliation) |

---

## 0. Design Basis — verified ground truths

Every decision below is anchored to schema facts from [`page-catalog/UAT_CATALOG.md`](page-catalog/UAT_CATALOG.md) (live `flamex_uat`, 111 tables) and the phase docs.

| # | Fact | Source | Consequence for this design |
|---|------|--------|------------------------------|
| G1 | `dbo.tbl_login`: PK `Id` (numeric), `UQ Email`, `UQ Phone_no`; **`User_Id` has no unique constraint** (non-unique `IDX_User_Id`) | UAT_CATALOG #44, [`docs/02`](02_Employee_Admin.md) | All new-table references to users must use **`Id`**, never `User_Id`. `User_Id` is stored denormalized for readability only. |
| G2 | `dbo.Permissions`: PK `PermissionId`, `UQ PermissionKey` (6 cols). `RolePermissions`: `UQ (RoleId, PermissionId)`. `dbo.Roles`: 5 cols, `UQ RoleName` | UAT_CATALOG #3–5 | Permission seeding is a single-row insert per key; no schema change needed for the permission model. |
| G3 | `dbo.ActiveSessions`: PK `SessionToken`, 8 cols — one live session per user, last login wins | UAT_CATALOG #1, doc 25 G4 | Link table must carry `AdminSessionToken` (the **lease**) so a link dies with its principal's session row. |
| G4 | `dbo.AuthAudit` exists (PK `AuditId`, 6 cols) — authentication events already have a home | UAT_CATALOG #2 | `SecurityAudit` is the **authorization/impersonation sibling** of the existing audit family, not a replacement; login audit stays in `AuthAudit`. |
| G5 | `dbo.tbl_Company`: PK `ID`. `UserCompanyAccess` contract: `UserId` + `CompanyID` unique, `IsActive` membership — **not present on UAT yet**; `AuthGuard` fails closed without it | UAT_CATALOG #21, [`docs/18`](18_Phase2A_Tenant_Membership.md) | Eligibility checks depend on `UserCompanyAccess`; its DDL is a **hard deployment prerequisite** for granting `SwitchUser`. |
| G6 | No triggers / no SPs on the identity tables are acceptable to modify; all app SQL is parameterized ADO.NET; audit before commit is the baseline rule | [`README.md`](../README.md) Ponytail §2/§4, [`CONTRIBUTING.md`](../CONTRIBUTING.md) §4 | New objects are **tables + indexes only**; the act logic stays in `ImpersonationService` (app tier, one SQL transaction per act). |
| G7 | `PasswordResetTokens` exists (`PasswordResetService` writes it) | [`docs/11`](11_Communications.md), doc 25 F4 | Target-ineligibility re-check joins this table; no schema change to it. |
| G8 | Kiosk realm (`tbl_card_login` / `tbl_employee`) is isolated from ERP auth | baseline; doc 23 §4.6.5 | No kiosk table appears anywhere in this ER model. |

**Existing concepts reused as-is (no alteration):** Users (`tbl_login`), Roles (`Roles`, `UserRoles`), Permissions (`Permissions`, `RolePermissions`), Companies (`tbl_Company`, `UserCompanyAccess`), login audit (`AuthAudit`), session liveness (`ActiveSessions`), password-reset state (`PasswordResetTokens`).

---

## 1. Entity-Relationship Model

### 1.1 ER diagram (new objects in relation to existing anchors)

```mermaid
erDiagram
    tbl_login ||--o{ ImpersonationLink : "AdminUserId (FK)"
    tbl_login ||--o{ ImpersonationLink : "TargetUserId (FK)"
    tbl_Company ||--o{ ImpersonationLink : "CompanyID (FK)"
    ActiveSessions ||..o{ ImpersonationLink : "AdminSessionToken (lease, no FK)"
    PasswordResetTokens ||..o{ ImpersonationLink : "target eligibility (query-time)"
    UserCompanyAccess ||..o{ ImpersonationLink : "target membership (query-time)"

    tbl_login ||--o{ SecurityAudit : "ActorUserId (no FK)"
    tbl_login ||..o{ SecurityAudit : "TargetUserId (denormalized, no FK)"
    ImpersonationLink ||..o{ SecurityAudit : "LinkToken (correlation, no FK)"

    ImpersonationLink {
        bigint ImpersonationLinkId PK "clustered identity"
        uniqueidentifier LinkToken UK "the only secret; 256-bit"
        uniqueidentifier AdminSessionToken "lease to ActiveSessions"
        int AdminUserId FK
        int TargetUserId FK
        int CompanyID FK
        char64 IntentToken "single-use CSRF/intent proof"
        nvarchar500 Reason "mandatory at start"
        datetime2_3 StartedUtc
        datetime2_3 LastUsedUtc "throttled per request"
        datetime2_3 EndUtc
        nvarchar30 EndReason
        bit IsActive
    }
    SecurityAudit {
        bigint Id PK "append-only identity"
        datetime2_3 EventUtc
        nvarchar50 EventType
        int ActorUserId "admin (principal) always"
        nvarchar100 ActorUser_Id "denormalized business key"
        int TargetUserId "NULL for denied"
        nvarchar100 TargetUser_Id "denormalized"
        int CompanyID
        uniqueidentifier SessionToken "admin's real session"
        uniqueidentifier LinkToken "NULL when denied"
        nvarchar500 Reason
        nvarchar100 DenialReason
        nvarchar50 IPAddress
        nvarchar500 UserAgent
    }

    tbl_login {
        int Id PK
        nvarchar_User_Id "business key - NOT unique (G1)"
        int CompanyID
    }
    tbl_Company {
        int ID PK
    }
    ActiveSessions {
        uniqueidentifier SessionToken PK
        int UserId
        bit IsActive
    }
    Permissions {
        int PermissionId PK
        nvarchar PermissionKey UK
    }
    UserCompanyAccess {
        int UserId
        int CompanyID
        bit IsActive
    }
    PasswordResetTokens {
        int TokenId PK
        int UserId
    }
```

ASCII fallback (same model, left = constrained state, right = append-only history):

```
                        CONSTRAINED STATE (one live link max per admin, per target)
┌───────────────┐ 1..1  ┌──────────────────────────────────────────────┐
│ ActiveSessions│◄─lease─┤              dbo.ImpersonationLink           │
│ SessionToken  │        │ LinkToken (UK, secret)  AdminSessionToken    │
└──────┬────────┘        │ AdminUserId ──FK──► tbl_login.Id             │
       │ principal       │ TargetUserId ─FK──► tbl_login.Id             │
       ▼                 │ CompanyID ────FK──► tbl_Company.ID           │
┌───────────────┐        │ IntentToken · Reason · StartedUtc            │
│   tbl_login   │        │ LastUsedUtc · EndUtc · EndReason · IsActive  │
│ Id (PK)       │        └──────────────────────────────────────────────┘
│ User_Id (n.u.)│                  │ 1..n (same LinkToken)
│ CompanyID     │                  ▼
└───┬───────┬───┘        ┌──────────────────────────────────────────────┐
    │       │            │                dbo.SecurityAudit             │
    ▼       ▼            │ Id · EventUtc · EventType                    │
 UserRoles  UserCompany  │ ActorUserId/ActorUser_Id  (= principal)      │
 → Roles    Access        │ TargetUserId/TargetUser_Id (= observed)      │
 → RolePerm  (UAT: DDL    │ CompanyID · SessionToken · LinkToken         │
 → Perm.     pending G5)  │ Reason · DenialReason · IP · UserAgent       │
                          │  APPEND-ONLY: no FK, no UPDATE/DELETE grant  │
                          └──────────────────────────────────────────────┘
```

### 1.2 Why the model splits into exactly two new tables

| Concern | Home | Rule |
|---|---|---|
| "Is this session observing, as whom?" | `ImpersonationLink` | Mutable while live, immutable once closed; **the only authorization input** for the acting identity |
| "Who did what, when, why, from where?" | `SecurityAudit` | Insert-only; never read by the authorization path |

This split implements the doc 25 layering directly: L4 (canonical link state) and L5 (event log) are separate tables because they have different mutability, different retention, and different consumers. Everything else in the ER model is **pre-existing** — deliberately zero new columns on any existing table (§2.5).

---

## 2. New Tables

### 2.1 `dbo.ImpersonationLink` — canonical impersonation state (L4)

One row per impersonation act. Created only inside `ImpersonationService.TryStart`'s single transaction; closed by `TryEnd`/sweeper/logout paths.

| Column | Type | Null | Notes |
|---|---|---|---|
| `ImpersonationLinkId` | BIGINT IDENTITY | NOT NULL | **Clustered PK.** Surrogate chosen over `LinkToken`-as-clustered to avoid random-GUID insert fragmentation (delta vs doc 23 §5.2 — see §2.4). |
| `LinkToken` | UNIQUEIDENTIFIER (app-generated CSPRNG) | NOT NULL | UNIQUE (nonclustered, covering — §3.1). The only secret in the design; session holds nothing but this pointer (L2). |
| `AdminUserId` | INT | NOT NULL | FK → `tbl_login.Id`. The **principal**. |
| `AdminSessionToken` | UNIQUEIDENTIFIER | NOT NULL | **Lease** (doc 25 §1.2): must equal the `ActiveSessions.SessionToken` of a live admin row on every lookup. No FK (session rows are killed/re-minted by login/logout — referential enforcement would fight G3 last-login-wins semantics). |
| `TargetUserId` | INT | NOT NULL | FK → `tbl_login.Id`. The **observed** user. Never a session principal of its own (INV-3). |
| `CompanyID` | INT | NOT NULL | FK → `tbl_Company.ID`. Tenant of the observation; equals the tenant both parties hold active `UserCompanyAccess` for (query-time check, G5). |
| `IntentToken` | CHAR(64) (256-bit hex) | NOT NULL | Single-use CSRF/intent proof (doc 23 §4.7). Consumed exactly once by `TryStart`; stored on the row for audit replay. |
| `Reason` | NVARCHAR(500) | NOT NULL | Mandatory admin-supplied justification; mirrored into `SecurityAudit`. |
| `StartedUtc` | DATETIME2(3) | NOT NULL | SQL-side default `SYSUTCDATETIME()` (doc 25 E8 — DB clock only). |
| `LastUsedUtc` | DATETIME2(3) | NULL | Lease-freshness + max-duration input. **Write-throttled** (≥60 s staleness) — see §8.2. |
| `EndUtc` | DATETIME2(3) | NULL | Set exactly once, at closure. |
| `EndReason` | NVARCHAR(30) | NULL | `UserEnded`, `Logout`, `Superseded`, `SessionExpired`, `IdleTimeout`, `Recycle`, `Revoked`, `TimedOut`, `TargetIneligible` (doc 25 §2.1 vocabulary — keep as a lookup-checked list in the service, not a DB constraint, to allow future reasons without migration). |
| `IsActive` | BIT | NOT NULL | 1 only between start-commit and closure. Drives both filtered unique indexes (§3.1) and the sweeper. |

**Semantics guarded by the schema itself:**
- *One live link per principal* and *one per target* are enforced by **unique filtered indexes**, not application checks (§3.1) — the doc 25 requirement that concurrency rules be "race-safe in the same transaction."
- *A link cannot outlive its principal* is enforced structurally by the lease (`AdminSessionToken` match against `ActiveSessions`), not by any column on this table — there is deliberately no "expected principal expiry" column to go stale.

### 2.2 `dbo.SecurityAudit` — append-only security event log (L5)

Sibling of the existing `AuthAudit` (G4). Written inside the same transaction as the link mutation it describes (fail-closed, INV-6). **No FKs, no UPDATE/DELETE grants to the application login — DBA-enforced** (doc 23 §5.1 retained verbatim as policy).

| Column | Type | Null | Notes |
|---|---|---|---|
| `Id` | BIGINT IDENTITY | NOT NULL | Clustered PK (monotonic insert-friendly). |
| `EventUtc` | DATETIME2(3) | NOT NULL | Default `SYSUTCDATETIME()`. |
| `EventType` | NVARCHAR(50) | NOT NULL | `ImpersonationStart`, `ImpersonationEnd`, `ImpersonationDenied`, `ImpersonationExpired` (doc 23 §5.1 set; extensible to later security events — password resets, role changes — without migration). |
| `ActorUserId` | INT | NOT NULL | Admin `tbl_login.Id` — **always the principal**, even on denied attempts. |
| `ActorUser_Id` | NVARCHAR(100) | NOT NULL | Denormalized business key: history must read cleanly even after user deletion (E9). |
| `TargetUserId` | INT | NULL | NULL for denials (target may be unknown/invalid). |
| `TargetUser_Id` | NVARCHAR(100) | NULL | Denormalized target business key. |
| `CompanyID` | INT | NOT NULL | Tenant of the attempt. |
| `SessionToken` | UNIQUEIDENTIFIER | NOT NULL | Admin's real `ActiveSessions` token — joins audit to the principal's session row for forensics. |
| `LinkToken` | UNIQUEIDENTIFIER | NULL | **Correlation pivot** (§5). NULL on `ImpersonationDenied` (no link existed). |
| `Reason` | NVARCHAR(500) | NULL | Admin-supplied reason (start) / closure reason context (end). |
| `DenialReason` | NVARCHAR(100) | NULL | Machine-readable cause (`permission`, `target_ineligible`, `broader_permissions`, `target_in_use`, `already_observing`, `nested`, `self`, `intent_used`, …) from the STRIDE denial taxonomy. |
| `IPAddress` | NVARCHAR(50) | NULL | From the request at act time. |
| `UserAgent` | NVARCHAR(500) | NULL | From the request at act time. |

**Deliberate properties:** no FK to `tbl_login` (append-only must survive user deletion — doc 23 §5.1); no triggers (G6); no checksum/HMAC column — tamper evidence is provided by DB-level grant revocation, not application-layer hashing (application-controlled integrity columns are themselves tamperable; if DBA-side cryptographic logging is later required, it is a DBA concern, out of app scope).

### 2.3 Archive tables (deferred, not created in the foundation migration)

`ImpersonationLink_Archive` (same shape minus the filtered unique indexes and FKs) is created **only when the first retention window matures** (§7.3). Creating it now would be speculative schema.

### 2.4 Reconciliation with doc 23 §5 sketches

Doc 23 §5.2 was a sketch; doc 25 later added the lease. This document is the canonical DB spec:

| Item | Doc 23 §5 sketch | **Doc 27 (authoritative)** | Why |
|---|---|---|---|
| PK | `LinkToken UNIQUEIDENTIFIER PK` | Surrogate clustered `ImpersonationLinkId` + UNIQUE nonclustered covering `LinkToken` | Random GUIDs fragment a clustered index; the per-request lookup must be a single covering probe (§8.1) |
| `AdminSessionToken` | absent | present, NOT NULL | Doc 25 lease (L4) — binds link to the principal's `ActiveSessions` row (G3) |
| Admin/target exclusivity | non-unique filtered indexes `(TargetUserId) WHERE IsActive=1`, `(AdminUserId) WHERE IsActive=1` | **UNIQUE** filtered indexes (§3.1) + stale-link reclamation at `TryStart` | DB-enforced one-per-principal/one-per-target makes the concurrency rule race-safe rather than check-then-insert |
| `LastUsedUtc` | "touched per request" | throttled update (≥60 s) | Removes a per-request UPDATE from the hot path (§8.2) |
| `SecurityAudit` shape | as specified | unchanged, plus `LinkToken` filtered index | Correlation pivot needs its own index (§3.2) |
| `tbl_login.ActiveLinkToken` | rejected | **still rejected** | Hot-path denormalization that can go stale; the lease makes it unnecessary |

### 2.5 Explicit non-changes (backwards-compatibility contract)

- **No column is added to any existing table.** In particular: no `ActiveSessions` change (forged rows eliminated by design), no `tbl_login` change, no `UserRoles`/`RolePermissions`/`Permissions`/`UserCompanyAccess`/`Roles`/`tbl_Company` change beyond the permission **seed rows** (§4).
- No triggers, no new views, no stored procedures required for the feature.
- `Web.config`, session provider, `machineKey`: untouched.
- Business tables: no `ActorUserId` columns added — attribution to the principal is achieved by the resolver invariant (reads act as target; writes attribute to admin), not by schema (doc 23 §8 Phase 2 rule).

---

## 3. Indexes

### 3.1 `ImpersonationLink`

| Index | Definition | Purpose |
|---|---|---|
| PK | CLUSTERED (`ImpersonationLinkId`) | Sequential inserts; no GUID fragmentation |
| `UX_ImpersonationLink_Token` | UNIQUE (`LinkToken`) INCLUDE (`AdminUserId`, `AdminSessionToken`, `TargetUserId`, `CompanyID`, `IntentToken`, `Reason`, `StartedUtc`, `LastUsedUtc`, `EndUtc`, `EndReason`, `IsActive`) | **The per-request probe.** One seek returns everything `SecurityContext` needs — no bookmark lookup on the hot path |
| `UX_ImpersonationLink_Admin_Active` | UNIQUE (`AdminUserId`) **WHERE `IsActive = 1`** | DB-enforced one-live-link-per-principal; concurrent `TryStart` from the same admin loses with a unique-key violation → surfaced as `already_observing`, never a duplicate |
| `UX_ImpersonationLink_Target_Active` | UNIQUE (`TargetUserId`) **WHERE `IsActive = 1`** | DB-enforced one-live-link-per-target; concurrent attempt by a second admin surfaces `target_in_use` (doc 25 F11) |
| `IX_ImpersonationLink_Sweeper` | (`IsActive`, `LastUsedUtc`) INCLUDE (`StartedUtc`, `AdminUserId`, `AdminSessionToken`, `TargetUserId`, `CompanyID`) WHERE `IsActive = 1` | Sweeper/lazy-close probe: find stale rows without touching closed history |

The filtered unique indexes contain only live links (single-digit rowcounts at ERP scale), so the two eligibility `EXISTS` checks inside `TryStart` are effectively free and cannot deadlock the business tables.

**Stale-link reclamation (required companion rule):** because the lease can die (supersession G3, recycle F1) while a row is still `IsActive = 1`, `TryStart` must, inside its transaction, close any link where `AdminUserId = @admin AND AdminSessionToken <> @currentSessionToken AND IsActive = 1` with `EndReason = 'Superseded'` **before** inserting the new row. This keeps `UX_..._Admin_Active` from blocking a re-login admin on a dead row and is self-healing; the sweeper remains for rows whose admin never returns.

### 3.2 `SecurityAudit`

| Index | Definition | Purpose |
|---|---|---|
| PK | CLUSTERED (`Id`) | Append-only monotonic |
| `IX_SecurityAudit_Link` | (`LinkToken`) **WHERE `LinkToken IS NOT NULL`** | The correlation pivot — "the full history of this impersonation act" in one seek |
| `IX_SecurityAudit_Actor` | (`ActorUserId`, `EventUtc` DESC) INCLUDE (`EventType`, `TargetUserId`, `CompanyID`) | Viewer page: filter by admin |
| `IX_SecurityAudit_Target` | (`TargetUserId`, `EventUtc` DESC) INCLUDE (`EventType`, `ActorUserId`, `CompanyID`) WHERE `TargetUserId IS NOT NULL` | Viewer page: filter by observed user; target-transparency notice lookup |
| `IX_SecurityAudit_Type_Time` | (`EventType`, `EventUtc`) | Denial-rate monitoring, `ImpersonationExpired` sweeps |
| `IX_SecurityAudit_Time` | (`EventUtc`) INCLUDE (`EventType`) | Retention/archival cursor, timeline export |

### 3.3 Existing indexes — none added, none changed

`tbl_login.Id` (PK) already serves every FK; `Permissions.PermissionKey` (UQ) already serves seed idempotency. No index on `tbl_login.User_Id` is required by this feature (it is display-only denormalization).

---

## 4. Foreign Keys

| FK (on `ImpersonationLink`) | References | Enforcement | Rationale |
|---|---|---|---|
| `FK_ImpersonationLink_AdminUser` | `tbl_login.Id` | NO ACTION | A live link to a deleted principal is a corrupt state; blocking is correct. Closed links older than retention are archived before user hard-deletes (rare; soft `IsActive` is the norm) |
| `FK_ImpersonationLink_TargetUser` | `tbl_login.Id` | NO ACTION | Same; also guarantees eligibility queries cannot hit dangling ids |
| `FK_ImpersonationLink_Company` | `tbl_Company.ID` | NO ACTION | Tenant integrity: a link can never reference a nonexistent company |

| FK (on `SecurityAudit`) | — | **None** | Append-only history must survive user deletion and company archival; denormalized business keys (`ActorUser_Id`, `TargetUser_Id`) carry the readability (E9). This mirrors the existing `AuthAudit` family behavior |

No FKs are added **to** any existing table (existing objects untouched — §2.5). Query-time dependencies (lease vs `ActiveSessions.SessionToken`; target eligibility vs `UserCompanyAccess`, `PasswordResetTokens`, `tbl_login.IsActive`) are intentionally *not* FKs: they are liveness/eligibility predicates evaluated per act, not structural references.

---

## 5. Audit Correlation

### 5.1 The three-key chain

Every impersonation event is reconstructible through three keys, each already present on the rows:

```
AuthAudit / login ──(principal)──► SessionToken ──► ActiveSessions row
                                         │
ImpersonationLink (LinkToken) ◄──────────┘ lease (AdminSessionToken)
        │
        ├── ImpersonationStart   (SecurityAudit, LinkToken = L)
        ├── ImpersonationEnd     (SecurityAudit, LinkToken = L, EndReason)
        │        └─ business rows written during [StartedUtc, EndUtc)
        │           carry ActorUserId = principal  (resolver invariant)
        └── ImpersonationDenied  (SecurityAudit, LinkToken = NULL)
```

### 5.2 Canonical audit questions and their answers

| Question | Answer path |
|---|---|
| "What did admin X observe, when, and why?" | `IX_SecurityAudit_Actor` on X → `ImpersonationStart/End` rows → `LinkToken` → `ImpersonationLink` row (reason, window, target) |
| "Whose data did admin X *see* while observing Y?" | `LinkToken` L window `[StartedUtc, EndUtc)` — the observation window; **reads leave no row** (design property: observation is not logged per-read; see residual-risk note below) |
| "What did admin X *change* while impersonating?" | Business tables where the attribution column (e.g. `CreatedByCode`, `ApprovedBy`) resolves to X **and** event time ∈ L's window; join via `ActorUserId` + window, not via target |
| "Which admins observed user Y, ever?" | `IX_SecurityAudit_Target` on Y → ordered event history |
| "Was a switch attempted and refused? Why?" | `ImpersonationDenied` rows: `ActorUserId` + `DenialReason` + `TargetUser_Id` |
| "Who closed this link and how?" | `ImpersonationEnd` row's `EndReason` + matching link row's `EndUtc` — they are written in one transaction and cannot diverge |

**Residual-risk acknowledgment (carried from STRIDE doc 26):** the design audits *acts and windows*, not individual page reads. Per-read logging was explicitly rejected as a DoS/privacy trade; the window + business-write correlation above is the agreed audit granularity. This must be stated in the security notice for the feature.

---

## 6. Rollback Tracking

**Design position: rollback is an *event*, not a *state restore*.** The admin's principal (L1) was never mutated, so there is no original-state snapshot to persist anywhere — which is precisely why **no rollback/snapshot columns exist on any table**. The doc 23 defect (stale `Original*` session snapshot replay) is eliminated by having nothing to replay.

What the database records for a rollback:

| Artifact | Where | Content |
|---|---|---|
| Link closure | `ImpersonationLink` row | `IsActive` 1→0, `EndUtc` = DB now, `EndReason` (e.g. `UserEnded`) |
| Audit event | `SecurityAudit` row | `ImpersonationEnd`, same `LinkToken`, same transaction (INV-6) |
| Post-rollback re-derivation | *no table* | Admin identity/roles re-derived fresh from `tbl_login` + `UserCompanyAccess` + `UserRoles` on the next request (doc 23 §7) — if that fails, logout; nothing is written from session |

**Idempotent close:** the close statement is conditional (`WHERE LinkToken = @t AND IsActive = 1`); zero rows affected ⇒ already closed (doc 25 F8 retry case) ⇒ the audit insert is skipped on that path so the pair (close + end-event) remains exactly-once.

**Stale-link reclamation** (§3.1) also serves rollback tracking: a superseded browser's link closes with `EndReason = 'Superseded'` even if the admin never clicked "End Observation" — every started link gets exactly one closure event by one of: user, logout path, lease check, or sweeper.

---

## 7. Historical Retention

### 7.1 Growth model (intranet ERP, hundreds of users)

| Driver | Assumption | Volume |
|---|---|---|
| Impersonation acts | ≤ 50/day peak (support diagnostics) | ~18 k link rows/year |
| Audit rows per act | start + end (+ occasional denials) ≈ 2.5 | ~45 k audit rows/year |
| Row sizes | ~0.5 KB link, ~0.7 KB audit (incl. indexes) | ~25 MB link, ~40 MB audit per year |

This is **two orders of magnitude** below SQL Server tuning thresholds. No partitioning, no compression, and no aggressive purge are justified at go-live.

### 7.2 Retention policy

| Data | Online window | Action at expiry |
|---|---|---|
| `ImpersonationLink` | 24 months online | Archive to `ImpersonationLink_Archive` (no FK, no filtered indexes), then delete from online table. DBA-run SQL Agent job, never app code (G6) |
| `SecurityAudit` | **Permanent online** (or per company financial-audit policy, ≥ 7 years) | Never updated or deleted (append-only contract). At projected volume the table stays well under 1 M rows even at 7 years |
| Archive tables | Indefinite, cold | Restorable into the audit viewer as an optional toggle (doc 23 §6.3), default off |

### 7.3 What is deliberately *not* built now

- No partition scheme, no columnstore, no In-Memory OLTP — premature at this volume.
- No `ImpersonationLink_Archive` table at deploy time (§2.3).
- No automated purge of `SecurityAudit` **ever** — the append-only guarantee is the feature's auditability foundation (doc 23 §2.6/T4).

---

## 8. Performance Considerations

### 8.1 Per-request cost (hot path, every ERP page)

| Step | Cost | Basis |
|---|---|---|
| Link resolution | 1 seek on `UX_ImpersonationLink_Token` (covering) | Same magnitude as the existing `ActiveSessions` check `Bill.Master` already runs per request (doc 23 §2.8). No caching beyond the request (baseline §3 prohibition) |
| Lease check | same seek (`AdminSessionToken` is an INCLUDE column) | No second query |
| Target-liveness re-check | folded into the same lookup as a join to `tbl_login.IsActive`/`PasswordResetTokens` (doc 25 F4) | Optional 1-in-N lazy variant if profiling demands |
| `LastUsedUtc` touch | **throttled** UPDATE, ≥ 60 s staleness (§8.2) | Prevents a per-request write; stale rows are swept anyway, so a skipped touch only widens the sweeper window marginally |

Result: impersonation adds **one indexed read and (at most) one throttled write** per request, only while a link is active. Normal sessions: zero new queries.

### 8.2 The `LastUsedUtc` throttle

Per-request writes would be pure log-churn (one 13-byte column on a row only its own session reads). The update is executed only when `LastUsedUtc` is ≥ 60 s old; correctness is unaffected because `LastUsedUtc` feeds the *sweeper* (grace ≈ 50 min, doc 25 §5.3) and the 4-hour max-duration cap, both far coarser than 60 s.

### 8.3 Write-path contention

- `TryStart` inserts one row; the two filtered unique indexes serialize concurrent starts per admin/per target at the engine level. Contention window is the transaction itself (a handful of eligibility reads); the loser receives a clean denial (`already_observing` / `target_in_use`), never a hang.
- The single per-act transaction (link insert + audit insert) is the same pattern the app already uses for business writes + `tbl_SystemNotification` (Ponytail §2) — one connection, no new deadlock surface. No locks are held on `tbl_login` beyond the eligibility reads.
- Closure is a single-row conditional UPDATE; the sweeper touches only rows in `IX_ImpersonationLink_Sweeper` (live rows), never scans history.

### 8.4 Read-path (audit viewer)

The viewer page (doc 23 §6.3, Phase 3) reads only via the §3.2 indexes with TOP/paging and mandatory date bounds; unbounded scans of `SecurityAudit` are denied at the page level (permission `ImpersonationAudit` + date-range defaults). No reporting workload is placed on the OLTP tables in this design.

### 8.5 Explicit non-goals

- No application/static caching of links (`Application[...]` is prohibited — Ponytail §3; also multi-instance unsafe).
- No denormalized "active link" pointer on `tbl_login` or `ActiveSessions` (doc 23 §5.3 rejection stands — the lease supersedes it).
- No new background thread in the web tier for sweeping; lazy per-request close first, SQL Agent job optional later (doc 25 §5.3).

---

## 9. Migration Plan

### 9.1 Deliverable and house style

One idempotent script, `db/impersonation_foundation.sql`, authored at implementation time in the [`db/pservice_snapshot.sql`](../db/pservice_snapshot.sql) convention: `When / Why / What` header, "DO NOT execute until reviewed" banner, `IF NOT EXISTS` guards on every object, no data backfill (all objects are new).

### 9.2 Steps (ordered; each independently reversible)

| # | Step (Phase mapping from doc 23 §8) | Objects | Gate |
|---|---|---|---|
| 0 | **Verify Phase 0 evidence** in the target environment: no role holds `SwitchUser` (grep/SELECT proof per doc 23 Phase 0.4) | — | Hard gate before anything below touches `flamex_live` |
| 1 | Create `dbo.ImpersonationLink` + all §3.1 indexes + §4 FKs | Table | Review + backup |
| 2 | Create `dbo.SecurityAudit` + all §3.2 indexes; **DENY UPDATE, DELETE** on it to the application login | Table + grants | DBA sign-off (append-only contract §2.2) |
| 3 | Seed permissions: ensure `Permissions.PermissionKey = 'SwitchUser'` row (add module path metadata per doc 23 §5.4) and insert `ImpersonationAudit` | 0–2 rows in `Permissions` (G2: UQ key) | **Inert until granted** — no role grants are made by the script (granting is a runtime admin decision) |
| 4 | Verify `UserCompanyAccess` DDL exists in the target DB | check only | **Hard gate** (G5): without it, target eligibility fails closed and the feature must not be granted |
| 5 | (Doc 23 Phase 1) App-side `ImpersonationService` + `SecurityContext` land against these objects | no DDL | — |

Rollback of the migration: drop the two tables and the seeded permission rows. Until doc 23 Phase 2 UI ships, **nothing in the running application reads or writes these objects**, so rollback is zero-impact. After Phase 2, rollback of DDL alone still fails safe: `ImpersonationService` must treat missing objects as denial (same fail-closed pattern `AuthGuard` already uses for missing `UserCompanyAccess`, doc 18).

Environment order: `flamex_uat` → soak → `flamex_live`, per the repo's UAT-first practice ([`docs/18`](18_Phase2A_Tenant_Membership.md) records the UAT ground truth this design was verified against).

### 9.3 Deferred migrations (not in the foundation script)

| Later migration | Trigger |
|---|---|
| `ImpersonationLink_Archive` + archive job | First 24-month retention boundary approaches (§7.3) |
| Optional SQL Agent sweeper job | Only if lazy per-request closing proves insufficient in ops data (doc 25 §5.3) |
| Additional `SecurityAudit` event types | Later security subsystems (password resets, role changes, exports) — additive rows, no DDL |

---

## 10. Backward Compatibility Analysis

| Dimension | Impact | Analysis |
|---|---|---|
| Existing schema objects | **None** | Zero column/index/constraint/trigger/SP changes to any of the 111 UAT tables (§2.5). New objects are additive and namespaced |
| Old builds against new DB | **None** | Current application code never references the new tables; behavior is byte-identical |
| New code against old DB | **Fails closed** | `ImpersonationService` denies (and logs server-side) when `ImpersonationLink`/`SecurityAudit` are missing — the established `AuthGuard` pattern for absent objects (G5). A partially-migrated environment can never half-enable impersonation |
| Session state | Additive only | New keys `UserDbId` (set at login, doc 23 §4.3) and `ImpersonationToken` (pointer, doc 25 L2). No provider change, no `Web.config` change; pre-existing sessions are unaffected because nothing reads the new keys unless the link model is active |
| RBAC data | Seed-only | `Permissions` gains ≤ 2 rows; no role is auto-granted; `UserRoles`/`RolePermissions` untouched (G2). Existing permission joins are unaffected |
| Business pages | Unaffected until resolver rollout | Pages keep reading `Session["USERID"]` (the principal). The read/write invariant changes behavior only in code that opts into `SecurityContext` (doc 23 §8 Phase 2 page families) |
| Multi-company switching | Unaffected | Link `CompanyID` is fixed at start; company switcher continues to validate `UserCompanyAccess` per request (doc 18). A company switch mid-observation is rejected by the lease/tenant checks and closes the link (`TargetIneligible` path) |
| Kiosk realm | None | No kiosk object participates (G8) |
| Notifications | Optional, additive | Target transparency (doc 23 §6.4) uses the existing `tbl_SystemNotification` subsystem for its correct purpose; no schema change |

**Compatibility verdict:** the migration is purely additive, inert until app code (Phase 1) and permission grants (runtime) activate it, and reversible at every stage.

---

## 11. Requirement Traceability

| Requested deliverable | § Where |
|---|---|
| New tables | §2.1, §2.2 (+ §2.3 deferred archive) |
| New indexes | §3.1, §3.2 (+ §3.3 no-change statement) |
| Foreign keys | §4 |
| Permission model | §4 seed rows, §9.2 step 3; SoD via `ImpersonationAudit` ≠ `SwitchUser` |
| Audit correlation | §5 |
| Rollback tracking | §6 |
| Historical retention | §7 |
| ER diagram | §1 |
| Migration plan | §9 |
| Backward compatibility analysis | §10 |
| Performance considerations | §8 |
| No implementation SQL | Honored throughout; executable script deferred to `db/impersonation_foundation.sql` |

---

*End of document. Specification only — no schema was created or modified, and no SQL was executed in producing this design. The executable migration is authored at implementation time per §9.*
