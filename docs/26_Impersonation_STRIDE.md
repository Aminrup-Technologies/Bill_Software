# 26 — STRIDE Threat Model: Administrator Impersonation (Switch User)

| | |
|---|---|
| **Status** | **Threat model — release-gate document** |
| **Author role** | Security Architecture (threat modeling) |
| **Method** | Microsoft STRIDE, applied per asset boundary rather than per page |
| **Scope** | The Link Model impersonation feature as specified in docs 23–25: `ImpersonationLink`, `SecurityAudit`, `SecurityContext` resolver, master-page insertion point I1, `SecurePage` fork I2, per-act intent token, superset rule, target eligibility |
| **Grounded against** | `SwitchUser.aspx.cs` (committed, pre-remediation), `index.aspx.cs`, `Bill.Master.cs`, `SecurePage.cs`, `AuthGuard.cs`, `Web.config` (`InProc` session, 2880-min Forms cookie), `UAT_CATALOG.md` (111 tables), `docs/22_Security_Baseline.md`, docs 23–25 |
| **Environment assumptions** | Internal ERP (intranet, AD-managed staff), multiple companies (tenants) in one database, financial transactions (quotations → PO → invoice → payment), regulatory-grade audit expectations |
| **Companion docs** | Doc 23 (feature design) · Doc 24 (lifecycle + invariants) · Doc 25 (session mechanics) · Doc 22 (baseline) |

---

## 0. How to read this document

- Each threat is numbered `S#`, `T#`, `R#`, `I#`, `D#`, `E#` by STRIDE category. Risk levels are **Critical / High / Medium / Low** and assume the **mitigations of docs 23–25 are implemented as specified** — the residual-risk column states what remains.
- Where the committed `SwitchUser.aspx.cs` code differs from the doc 23–25 design, the threat rows cite both: **[AS-COMMITTED]** (the identity-swap code that exists on disk today) and **[AS-DESIGNED]** (the Link Model). Threats marked *AS-COMMITTED only* vanish with the Phase 0 hotfix; the rest shape the design permanently.
- "Business write" means any INSERT/UPDATE/DELETE on the 111 tenant-scoped business tables (visits, expenses, quotations, POs, invoices, payments, users). "Principal" = the logged-in admin. "Target" = the impersonated user. "Act" = one server-side impersonation transition (open link / end link).

**Trust boundaries (TB):**

| Boundary | Crossing |
|---|---|
| **TB1** | Browser ⇄ IIS (cookie, ViewState, postback parameters, query strings) |
| **TB2** | IIS ⇄ SQL Server (parameterized ADO.NET) |
| **TB3** | Company A principal ⇄ Company B data (tenant isolation) |
| **TB4** | Admin principal ⇄ target user's identity and permissions |
| **TB5** | ERP app ⇄ kiosk app (`tbl_card_login` / `index_card`) — isolated realms |

---

## 1. Spoofing (S) — "who is really acting?"

**Assets at stake:** `Session["USERID"]` + `Session["SessionToken"]` (attribution anchor), `ActiveSessions.SessionToken`, `FormsAuthentication` cookie, `ImpersonationLink.LinkToken`, audit attribution.

### S1 — Forged session for the target user
| | |
|---|---|
| **Risk** | **Critical** |
| **Attack** | [AS-COMMITTED] Switch mints a real `ActiveSessions` row with the *target's* `UserId` without any credential event. The row is indistinguishable from a genuine login — `TryValidateSession` accepts it, and it survives the target's password reset because the reset flow kills sessions by identity, not by actor. |
| **Detection** | Diff `ActiveSessions` rows against `SecurityAudit` login events — any `ActiveSessions.UserId` with no matching authentication event is forged. One-off forensic query plus a nightly reconciliation job. |
| **Mitigation** | [AS-DESIGNED] Never create `ActiveSessions` rows for targets (doc 23 §4). The principal's own row is the only session row in play; the link is a separate table. Phase 0 hotfix: neutralize the write path in `SwitchUser.aspx.cs` before UAT → live promotion. |
| **Residual risk** | Low — a compromised *admin* account can still observe the target (that is S2, by design); it cannot fabricate a session that outlives the admin's own session (lease check, doc 25). |

### S2 — Legitimate admin misattributed as the target on writes
| | |
|---|---|
| **Risk** | **Critical** |
| **Attack** | [AS-COMMITTED] `Session["USERID"]` becomes the target at switch; every business write (approval, quotation, payment) is recorded as the victim. In an internal ERP this is not just an integrity bug — it is the *carrier* for repudiation and fraud (R1, E1). |
| **Detection** | Currently **none exists** — that is the defect. Post-remediation detection: any business row whose `CreatedBy`/`ApprovedBy` does not resolve to an `ActiveSessions` row alive at write time; and audit-viewer drill-down (doc 23 Phase 3) pairing writes with open links. |
| **Mitigation** | INV-1/INV-2 (doc 24): session keys are immutable during impersonation; reads resolve to the acting identity, writes attribute to the principal. `SecurityContext` is the only reader of `Session["USERID"]` for new code. |
| **Residual risk** | Low — misattribution becomes impossible by construction. Note the deliberate flip: post-remediation, an admin acting as a salesperson writes *as the admin*, so reports show a manager in a salesperson's pipeline. This is correct and must be explained to users (doc 23 §6). |

### S3 — Admin credential theft = target identity theft
| | |
|---|---|
| **Risk** | **High** |
| **Attack** | Attacker steals an admin's cookie/token (shared workstation, cookie theft, session hijack on TB1). With the committed code they become any user; with the Link Model they can only observe users *within the superset rule* — but they still act with the admin's full write authority. |
| **Detection** | Anomaly on `SecurityAudit`: impersonation acts from new IP/UA vs the admin's historical fingerprint; switch-rate anomalies (e.g. > N distinct targets/hour); switch immediately after admin login from a new device. |
| **Mitigation** | Link cap (doc 25: max 4h), one live link per principal/target, per-act intent token (each act is a deliberate server-validated postback, not a URL), audit every act with reason text. Reuse existing baseline: `machineKey` pinned so cookies are not portable across servers; non-persistent cookie. |
| **Residual risk** | Medium — an active attacker holding a live admin session is inside the trust boundary; detection is detective, not preventive. Additional control if needed later: re-auth prompt on switch (out of scope for MVP per doc 23 §9). |

### S4 — Link-token guessing / fixation
| | |
|---|---|
| **Risk** | **Low** |
| **Attack** | Attacker guesses or fixes another principal's `ImpersonationLink.LinkToken` to inherit an active link, or plants a token in their own session via URL. |
| **Detection** | AuthGuard failure spikes on `ImpersonationLink` lookups from one IP. |
| **Mitigation** | `LinkToken` = `CryptoRandom` 128-bit GUID (same primitive as `PasswordResetTokens`); token is **session-private** — lookup always joins `AdminSessionToken = Session["SessionToken"]` (the doc 25 lease check), so a stolen token alone is inert outside the principal's own session. Never carried in query strings (doc 24 U-list: client-side carriers forbidden). |
| **Residual risk** | Negligible. |

### S5 — Kiosk-realm cross-spoofing
| | |
|---|---|
| **Risk** | **Low** |
| **Attack** | Impersonation machinery invoked in the kiosk realm (`index_card`, `tbl_card_login`) to masquerade as an ERP user or vice versa. |
| **Detection** | Any `ImpersonationLink`/`SecurityAudit` write whose request path is outside `corporate/business/app/`. |
| **Mitigation** | Impersonation pages derive from `Bill.Master` only; `SecurePage` (which kiosk pages do not use) gates every act; doc 24 lists the kiosk realm as unsafe point U8 — no shared session semantics. |
| **Residual risk** | Negligible. |

---

## 2. Tampering (T) — "can state be changed illegitimately?"

**Assets at stake:** `ImpersonationLink` rows, `SecurityAudit` rows, session state, `ViewState`/event validation, business rows written while observing.

### T1 — Tamper with link state (extend, re-point, or resurrect)
| | |
|---|---|
| **Risk** | **High** |
| **Attack** | SQL injection or direct DB access (insider, compromised service account) modifies `ImpersonationLink`: extends `ExpiresAtUtc`, re-points `TargetUserDbId`, or flips `Status` back to active after end. |
| **Detection** | Audit chain gap: any `SecurityAudit` lifecycle event sequence with a gap (e.g. `LINK_OPENED` never followed by an end event, but row shows `Status='Ended'`); row-level change detection on the link table (timestamp mismatch vs audit). |
| **Mitigation** | Least-privilege DB user (no DDL/DML on link/audit tables outside the service); parameterized SQL everywhere (baseline §4 — no concatenation anywhere in the feature); the link is dead *structurally* when the principal session ends (lease check) so resurrection requires also faking a live `ActiveSessions` row; link cap re-checked per request (a tampered `ExpiresAtUtc` only helps until the next lease evaluation). |
| **Residual risk** | Low — direct-DB insider tampering defeats application-level controls in *any* design; compensating control is DB permissions + audit chain. |

### T2 — Tamper with audit records (forge or erase evidence)
| | |
|---|---|
| **Risk** | **High** (regulatory implication) |
| **Attack** | Insider with DB write access deletes/edits `SecurityAudit` rows after an abusive impersonation act; or [AS-COMMITTED] the feature itself writes audit to a user-visible expiring table (`tbl_SystemNotification`) inside a swallowing try/catch — *self-tampering by design*. |
| **Detection** | Gaps in the append-only sequence; nightly reconciliation of audit row counts vs business activity on flagged accounts; external replica/ship of the audit table (DBA task) so deletion on primary is detectable. |
| **Mitigation** | `SecurityAudit` is **append-only**: no UPDATE/DELETE grants to the app service account; INSERT-only. Fail-closed (doc 23 §5): audit write in the same transaction as the link transition — if it fails, the act never happened. Optional hash-chaining column (`PrevRowHash`) if the compliance program later requires tamper-evidence, not MVP. |
| **Residual risk** | Medium — a determined DBA/sysadmin can always defeat in-database controls; hash chaining + off-host copy is the escalation path. Documented as accepted residual for internal ERPs. |

### T3 — Client-side tampering with switch parameters
| | |
|---|---|
| **Risk** | **Medium** |
| **Attack** | Forged postback alters the target id or intent token; crafted URL carries a target id; replay of an old intent token. |
| **Detection** | AuthGuard denial spikes on the switch endpoints with mismatched tokens. |
| **Mitigation** | Per-act **intent token** (doc 23 §4.6): a fresh single-use token rendered into the page, validated server-side at the act, bound to the current link session — replay fails, forgery fails (not stored client-side, not in URL). WebForms EventValidation stays enabled. Target id is never accepted from query string (doc 24 U-list). Superset rule re-evaluated **per act**, so even a forged postback cannot exceed switcher privileges. |
| **Residual risk** | Low. |

### T4 — Tampering with business data under cover of impersonation
| | |
|---|---|
| **Risk** | **High** |
| **Attack** | Admin opens a link to a finance user and edits an invoice/payment; defense of "the target did it" — exactly what identity-swap enabled; under the Link Model the writes are attributed to the admin, but the *target's record* is still what changed. |
| **Detection** | Cross-reference: business-row `ModifiedBy`/`CreatedBy` = principal while an `ImpersonationLink` (principal → target, same company) was active, and the row belongs to the target's normal workflow scope. This is the core query of the Phase 3 audit viewer. |
| **Mitigation** | Attribution invariant (writes = principal) + audit viewer surfacing "writes performed while observing X"; read-only-mode profile for high-value pages is a documented option (doc 23 §9, not MVP); four-eyes on financial posts remains a business-process control outside this feature. |
| **Residual risk** | Medium — impersonation by definition grants the switcher the target's *view*; the superset rule caps *privilege gain* but does not make legitimate admin writes disappear. The control is detection + accountability, not prevention. |

### T5 — ViewState/postback tampering on the switch page
| | |
|---|---|
| **Risk** | **Low** |
| **Attack** | Crafted ViewState or control-tree manipulation on `SwitchUser.aspx`. |
| **Detection** | EventValidation/mac failures log on the endpoint. |
| **Mitigation** | Keep `enableViewStateMac` (default), `machineKey` pinned, EventValidation on; the page exposes no state a forged postback can convert into privilege (all decisions re-validated server-side per act). |
| **Residual risk** | Negligible. |

---

## 3. Repudiation (R) — "can anyone later deny the act?"

**Assets at stake:** `SecurityAudit`, business-row attribution, notification semantics.

### R1 — Admin denies having acted as the target
| | |
|---|---|
| **Risk** | **Critical** (without the fix) |
| **Attack** | [AS-COMMITTED] Identity swap means the evidence trail names the *target* for every write. The admin plausibly denies everything; the victim cannot prove innocence. In a financial-transaction ERP this is the single worst outcome of the feature. |
| **Detection** | None available post-hoc under identity swap — the data is gone at write time. (This is why doc 23 calls it Critical.) |
| **Mitigation** | INV-2: writes attribute to the principal, forever. `SecurityAudit` records open/end with reason, IP, UA, company, actor, target. No snapshot replay; no forged rows; the evidence chain is complete by construction. |
| **Residual risk** | Low — attribution is now provable; disputes resolve by querying two tables. |

### R2 — "I didn't open that link" (contested switch event)
| | |
|---|---|
| **Risk** | **Medium** |
| **Attack** | Admin claims a link was opened by someone else (shared workstation, stolen cookie — S3 overlap). |
| **Detection** | `SecurityAudit` carries IP + UA per act; correlation with `ActiveSessions` login IP/UA and with physical/VPN logs (intranet) typically resolves contested acts. |
| **Mitigation** | Per-act intent token proves a postback from the principal's own session state; link cap and single-session semantics (`ActiveSessions` killed on new login) narrow the window; reason text is mandatory, making casual switches self-witnessed. |
| **Residual risk** | Medium — shared-workstation intranets can never fully separate "cookie holder" from "user"; mitigations are process (workstation policy) + detective (IP/UA anomaly). Accepted residual. |

### R3 — Audit record exists but is not trusted (semantics, not tampering)
| | |
|---|---|
| **Risk** | **Medium** |
| **Attack** | Not an attack — a *gap*: audit rows exist but the process owner cannot answer "who approved invoice 123 while impersonating?" because notification rows expired (7-day) or the viewer lacks the join. |
| **Detection** | Compliance spot-check: sample five impersonation-linked writes; if any cannot be reconstructed, control fails. |
| **Mitigation** | Dedicated non-expiring `SecurityAudit`; Phase 3 viewer joins link ↔ writes; reason + link token stored on every act; retention policy set with finance (no expiry). |
| **Residual risk** | Low. |

---

## 4. Information Disclosure (I) — "what can be seen that shouldn't be?"

**Assets at stake:** target's records, cross-company data, passwords/tokens, audit data itself.

### I1 — Unauthorized cross-user data exposure (the core disclosure risk)
| | |
|---|---|
| **Risk** | **High** |
| **Attack** | A `SwitchUser` holder with narrow business duties (e.g., HR support) opens a link to a finance user and reads payroll/payment records they were never entitled to see. Under the superset rule the *privilege set* cannot expand, but **data visibility follows the target's role data scope**, which can still exceed the switcher's *effective* visibility (permissions ≠ row-level entitlements in this codebase — no row-level ACLs exist beyond `UserCompanyAccess` + ownership predicates). |
| **Detection** | Audit viewer: read volume anomalies per link (pages visited, rows listed while linked); disclosure-shaped patterns (list pages, export/print endpoints) vs the reason text supplied at switch. |
| **Mitigation** | Per-act authorization + superset rule caps *capabilities*; mandatory reason text creates a deterrent + audit trail; eligibility checks (active, not pending reset); link cap 4h; Phase 3 viewer gives compliance a working tool. Longer term (documented option): read-only profile per target for sensitive domains. |
| **Residual risk** | **Medium — accepted with compensating controls.** Impersonation inherently discloses the target's data to the switcher; the design question is *who may observe whom* (superset) and *how visible the observation is* (audit, cap, reason). This residual is explicitly accepted by management in the Go/No-Go. |

### I2 — Cross-company exposure (tenant isolation)
| | |
|---|---|
| **Risk** | **Medium** |
| **Attack** | Link opened across company boundary: switch target search unscoped, or the impersonated context lets a Company-A principal read Company-B rows. |
| **Detection** | `SecurityAudit` rows where `TargetCompanyID ≠ PrincipalCompanyID` (should be impossible → alerting event); cross-tenant read anomalies while linked. |
| **Mitigation** | Target search strictly scoped `WHERE CompanyID = @CurrentCompanyID`; impersonated context company = target's validated `UserCompanyAccess` row; baseline tenant predicate unchanged on every page; link table stores both company ids so any cross-company attempt is *evidence*, not just a failure. Cross-company impersonation is an explicit non-goal (doc 23 §2.9). |
| **Residual risk** | Low — the same tenant predicate that protects all 111 tables protects this feature; the new surface adds no bypass. |

### I3 — Disclosure of secrets via impersonated context (password hashes, tokens, reset links)
| | |
|---|---|
| **Risk** | **Medium** |
| **Attack** | Observing a user-administration persona (or the audit viewer itself) while impersonating exposes password hashes, `PasswordResetTokens` values, or audit rows containing IP/UA of others. |
| **Detection** | Access pattern on secret-bearing pages while a link is active. |
| **Mitigation** | Superset rule prevents observing a *more privileged* persona; `ImpersonationAudit` (viewer permission) is **separated from SwitchUser** (doc 23 Phase 3) — a switcher without the viewer permission sees no audit data; eligibility rule blocks targets with **pending password reset** (their reset token is live); secret-bearing pages are themselves `SecurePage`-gated. |
| **Residual risk** | Low. |

### I4 — Audit data as an attack surface (who watches the watchers)
| | |
|---|---|
| **Risk** | **Low** |
| **Attack** | Audit viewer (or its exports) becomes a directory of sensitive pairs: "admin X observed finance user Y on date Z" — internal politics / targeting data. |
| **Detection** | Export/print anomalies on the viewer page. |
| **Mitigation** | Viewer requires its own permission, distinct from `SwitchUser`; date-range + subject filters logged; no bulk export in MVP (doc 23 §9); retention aligned with policy rather than "keep forever in UI". |
| **Residual risk** | Low. |

### I5 — [AS-COMMITTED] Notification-table leak
| | |
|---|---|
| **Risk** | **Medium** |
| **Attack** | The switch event written to `tbl_SystemNotification` is *user-visible*: targets (and sometimes whole companies) can see "Admin impersonated you" — noisy at best; at worst it discloses *who is under observation* and defeats covert-support use cases, and its 7-day expiry destroys evidence (T2 overlap). |
| **Detection** | n/a — behavioral. |
| **Mitigation** | Move security events out of the notification domain entirely (`SecurityAudit`); the target-transparency notice is a **controlled UX element** (doc 23 Phase 3: banner shown to the *target* at their next login), not a broadcast row. |
| **Residual risk** | Negligible after remediation; keep as a regression test: no impersonation event may be written to user-visible tables. |

---

## 5. Denial of Service (D) — "can the feature be exhausted or wedged?"

**Assets at stake:** link-table hot path in `Bill.Master` (every page request), audit volume, connection pool.

### D1 — Link-table hot-path degradation
| | |
|---|---|
| **Risk** | **Medium** |
| **Attack** | Not adversarial in the classic sense — volume: the master page already runs session + company + menu SQL per request; a link lookup adds a third hot-path query. A pathological deployment (thousands of stale ended-link rows, missing index) turns every page load into a table scan. |
| **Detection** | Query-plan/latency monitoring on the master-page SQL; blocked-process and index-missing alerts on `ImpersonationLink`. |
| **Mitigation** | Covering index on `(AdminSessionToken, Status)` (doc 23 §5); link rows are **few** (≤ 1 live per principal + capped history); sweeper archives ended links (doc 25 grace window = session timeout + one heartbeat); per-request lookup is one indexed seek, comparable to the existing `ActiveSessions` check. No caching beyond request scope (baseline §3 — no static identity state). |
| **Residual risk** | Low. |

### D2 — Audit write amplification (audit failure wedges the ERP)
| | |
|---|---|
| **Risk** | **Medium** |
| **Attack** | Fail-closed audit means every impersonation act requires a successful audit insert; if the audit table is full, locked, or the DB is degraded, switches fail — availability risk by design. A DBA outage becomes a business "impersonation is down" incident. |
| **Detection** | Insert-latency and failure counters on `SecurityAudit`; alert on act-failure rate. |
| **Mitigation** | Fail-closed applies **to the act** (open/end link) only — never to the 188 ordinary pages; ordinary pages do no audit writes for impersonation (only the cheap link lookup). Table sized/monitored like any transactional table; acts are rare (support events), so volume is trivial. If audit insert fails, the *link* fails, the ERP keeps running. |
| **Residual risk** | Low — accepted: a feature that cannot be audited must not be usable. |

### D3 — Target lockout via forced supersession churn
| | |
|---|---|
| **Risk** | **Low** |
| **Attack** | Repeated open/end cycling on one target by several admins (or a script through the page) spams supersession events; noise degrades UX for the target (their banner/notice churns) and floods audit. |
| **Detection** | Act-rate anomaly per target and per principal (doc 23 detection thresholds). |
| **Mitigation** | One live link per target (doc 25 concurrency rule) makes churn *denied*, not processed; per-act intent token prevents scripted cycling; rate-limit/alert on repeated denied acts from one principal. |
| **Residual risk** | Negligible. |

### D4 — Sweeper/heartbeat interaction (stale links masquerading as load)
| | |
|---|---|
| **Risk** | **Low** |
| **Attack** | Mass stale links (e.g., after an IIS recycle storm) cause a sweeper backlog or misleading idle-timeout traffic. |
| **Detection** | Sweeper batch-size metrics; ended-reason distribution (`Recycle`/`IdleTimeout` spikes). |
| **Mitigation** | Lease check makes stale links inert before the sweeper ever sees them (no cleanup required for correctness — doc 25 §5); sweeper is hygiene only; grace window prevents tight-loop sweeping. |
| **Residual risk** | Negligible. |

---

## 6. Elevation of Privilege (E) — "can the actor get more than they came with?"

**Assets at stake:** permission sets, role grants, company access, the feature's own permission key.

### E1 — Vertical escalation via unscreened target
| | |
|---|---|
| **Risk** | **Critical** [AS-COMMITTED]; **High** if unscreened in any future variant |
| **Attack** | [AS-COMMITTED] Any `SwitchUser` holder can become a Super Admin — the switcher's permission set expands to the target's. This is the highest-value single attack in the model: a low-duty support account silently becomes a super-user, and the audit trail names the *victim*. |
| **Detection** | `SecurityAudit`: any open-link act where target permission set ⊄ switcher permission set (post-remediation this combination is *impossible*, so any attempt is a strong intrusion signal); pre-remediation, correlate `Session["USERID"]` changes with role snapshots. |
| **Mitigation** | **Superset rule** (doc 23 §4.2) enforced server-side **per act**, permission-set-based (never role-name string compare — roles are per-company data): *impersonation must never yield privileges the switcher does not already hold*. Combined with per-act authorization and target eligibility. Phase 0: verify no role currently holds `SwitchUser` in UAT **and** live before the code reaches users. |
| **Residual risk** | Low — capability gain is structurally zero; what remains is S3 (stolen admin session) and I1 (data visibility), both separately accepted/detected. |

### E2 — Horizontal escalation across companies
| | |
|---|---|
| **Risk** | **Medium** |
| **Attack** | Switch into a same-named user in another company; or use a mid-link company-context change to pivot tenants while observing. |
| **Detection** | Audit: link rows with `PrincipalCompanyID ≠ TargetCompanyID` (must be zero); company-switch event while a link is active (alert). |
| **Mitigation** | Target search scoped to current company; impersonated context keeps tenant predicate of the target's validated membership; **company-switch control disabled while a link is active** (doc 24 I1 insertion point covers the master's company resolver — the link must be ended before switching company); cross-company impersonation is a non-goal. |
| **Residual risk** | Low. |

### E3 — Rollback resurrection of revoked grants
| | |
|---|---|
| **Risk** | **High** [AS-COMMITTED] |
| **Attack** | [AS-COMMITTED] `OriginalRoleId/RoleName/CompanyId` session snapshots replayed at switch-back re-grant a role or company access revoked *while observing* (e.g., admin's access pulled mid-link; rollback resurrects it). This is the classic stale-state escalation the baseline forbids. |
| **Detection** | Post-remediation: impossible by construction — any code path writing identity session keys outside login is a defect (doc 24 INV-1; add a code-review gate: only `index.aspx.cs` writes principal keys). Pre-remediation: correlate revoked grants with rollback events. |
| **Mitigation** | **Rollback = DB re-derivation** (doc 23 §7, doc 25 T9→T10): never replay session snapshots; re-read `tbl_login` + `UserRoles` + `UserCompanyAccess` fresh at rollback; a principal who lost the *SwitchUser* permission mid-link loses the ability to end it manually → link expires via lease (they were already forced back by per-act checks on next request; the link cannot be used for privilege, only ended/expired). |
| **Residual risk** | Low. |

### E4 — Nested / chained impersonation (A→B→C)
| | |
|---|---|
| **Risk** | **Medium** |
| **Attack** | While observing B, open a link to C; the second link's "original" is B; rollback chains and the audit graph becomes a maze — and privilege travels along the chain. |
| **Detection** | `SecurityAudit` open-act while a link is already active for the principal (should be denied; any occurrence = defect). |
| **Mitigation** | **One live link per principal** (doc 25) — enforced in the same transaction as link open, so the race is structurally closed; nested acts are refused at the act level, not merely hidden in UI (doc 25 E1); the switch page itself is `SecurePage`-gated and the superset rule re-evaluates per act, so even a UI bypass gains nothing. |
| **Residual risk** | Low. |

### E5 — Feature-permission persistence (revocation actually works)
| | |
|---|---|
| **Risk** | **Medium** |
| **Attack** | An admin's `SwitchUser` permission is revoked, but a live link (or the master-page visibility flag cached in session) keeps the feature usable. |
| **Detection** | Denied acts from principals whose permission was recently revoked (audit join against current permission state). |
| **Mitigation** | Permission is re-evaluated **per act** (never cached beyond request scope — baseline §3 static prohibition); master visibility flag re-rendered per request; an expired-permission principal's link dies via lease/next-request check. Session keys are not consulted for authorization anywhere (they are cosmetic — baseline). |
| **Residual risk** | Low. |

### E6 — [AS-COMMITTED] Direct act without authentication integrity (auth bypass through the forged session)
| | |
|---|---|
| **Risk** | **High** |
| **Attack** | Because the forged target session is a *real* `ActiveSessions` row, all downstream gates (`SecurePage`, resource authorization) run against it normally — the feature's bypass is delivered with valid-looking credentials at every layer. Nothing downstream can resist it; the only correct control is upstream: don't create the row. |
| **Detection** | S1's reconciliation job (sessions without authentication events). |
| **Mitigation** | The Link Model: no target `ActiveSessions` rows, ever (doc 23 §4, doc 24 §0.4). Phase 0 neutralization. |
| **Residual risk** | Negligible post-remediation. |

---

## 7. Residual-risk register (management-facing summary)

| ID | Residual risk | Level | Compensating control | Acceptance |
|---|---|---|---|---|
| RR-1 | Admin session theft grants observation authority (S3) | Medium | Anomaly detection, link cap, per-act intent | Accepted — intranet trust boundary |
| RR-2 | In-database insider tampers with audit (T2) | Medium | Append-only grants, audit-chain gap detection, optional off-host copy | Accepted for MVP; hash-chain if compliance requires |
| RR-3 | Impersonation inherently discloses target data to switcher (I1) | Medium | Superset rule, reason text, eligibility, 4h cap, audit viewer | Accepted by management at Go/No-Go |
| RR-4 | Shared-workstation ambiguity in contested acts (R2) | Medium | IP/UA correlation, mandatory reason, process policy | Accepted — process control |
| RR-5 | Admin writes into target workflow while observing (T4) | Medium | Attribution invariant + Phase 3 viewer + optional read-only profile | Accepted; revisit after Phase 3 telemetry |

**No other category carries accepted residual risk** — the design eliminates the rest by construction (lease check, superset rule, DB re-derivation, fail-closed audit, no forged rows).

---

## 8. Traceability

| STRIDE ref | Mitigating mechanism | Specified in |
|---|---|---|
| S1, S2, E6, R1 | No forged `ActiveSessions` rows; writes attribute to principal (INV-1/INV-2) | Doc 23 §4; Doc 24 §4 |
| S3, R2 | Per-act intent token, link cap, anomaly detection thresholds | Doc 23 §4.6, §8; Doc 25 §6 |
| S4 | `CryptoRandom` LinkToken + lease join on `AdminSessionToken` | Doc 23 §5; Doc 25 §2 |
| S5, I2, E2 | Tenant scoping, kiosk isolation, company-switch disabled during link | Doc 22 §3; Doc 24 U-list/I1 |
| T1, D1, D4 | Link schema, covering index, sweeper grace, lease-expiry | Doc 23 §5; Doc 25 §5 |
| T2, R3, D2 | `SecurityAudit` append-only, fail-closed, same-transaction | Doc 23 §5 |
| T3, T5, D3 | Per-act authorization + intent token; EventValidation retained; one-link-per-target | Doc 23 §4.6; Doc 25 §7 |
| T4 | Attribution invariant + Phase 3 audit viewer | Doc 23 §9; Doc 24 INV-2 |
| I1 | Superset rule + eligibility + reason + cap | Doc 23 §4.2 |
| I3, I4 | Permission separation (`ImpersonationAudit`), pending-reset eligibility block | Doc 23 §9 |
| I5 | Security events out of `tbl_SystemNotification`; target notice as controlled UX | Doc 23 §6/§9 |
| E1 | Superset rule per act; Phase 0 permission sweep | Doc 23 §4.2, §9 (Phase 0) |
| E3, E4, E5 | Rollback = DB re-derivation; one live link per principal; per-act permission re-check | Doc 23 §7; Doc 25 §3 |

**Verification checklist (pre-release):** Phase 0 neutralization confirmed on disk · no role holds `SwitchUser` in UAT/live until Phase 2 · superset rule unit-tested with adversarial fixtures · audit fail-closed demonstrated (kill audit table perms → act must fail) · lease expiry demonstrated (kill admin session row mid-link → next request is NORMAL_ADMIN) · forged-session reconciliation query returns zero · no impersonation write reaches `tbl_SystemNotification`.
