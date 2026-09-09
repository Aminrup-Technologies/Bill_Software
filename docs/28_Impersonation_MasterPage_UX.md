# 28 — MasterPage UI Integration: Impersonation (Switch User)

| | |
|---|---|
| **Status** | **Design — wireframes & interaction flow for the Link Model UI** |
| **Author role** | UX / Front-of-Frame Architecture |
| **Depends on** | [`23_Impersonation_Architecture_Review.md`](23_Impersonation_Architecture_Review.md) (§6 UI model), [`25_Impersonation_Session_Model.md`](25_Impersonation_Session_Model.md) (states, lease, heartbeat), [`26_Impersonation_STRIDE.md`](26_Impersonation_STRIDE.md) (denial taxonomy), [`27_Impersonation_Database_Review.md`](27_Impersonation_Database_Review.md) (eligibility & denial reasons) |
| **Grounded against** | `Bill_Software/corporate/business/app/Bill.Master` (verified anatomy, this doc §0) |
| **Scope** | Header placement, search modal, impersonation banner, rollback UX, mobile behavior, permission visibility, accessibility |
| **Constraint** | Wireframes and interaction specification only — **no ASPX / C# / JS code is produced here.** |

---

## 0. Verified Ground Truths — `Bill.Master` anatomy

Everything below was read from the master page source; the recommendations are fitted to **this** frame, not a generic one.

| # | Fact | Source region | UI consequence |
|---|------|---------------|----------------|
| G1 | Header = flex row `.top-header-bar` (min-height 45px, `#2268a9`, z-index 999) with three zones: `.header-brand` (logo + company), `.main-nav` (`#nav`, flex-wrap, hover dropdowns), `.header-profile` (clock, company dropdown, user block, **`pnlSwitchUser` already exists here**, logout) | `<header>` block | Switch User trigger **keeps its current home** (`.header-profile`) — placement is already correct; this doc defines its states |
| G2 | Body is `form { display:flex; flex-direction:column; height:100vh }`: header → `.app-content` (flex:1, own scroll) → `.app-footer`. All flex-shrink:0 except content | `<style>` | A **banner row inserted between header and `.app-content`** participates in the flex stack: always visible, never scrolls away, no z-index negotiation needed |
| G3 | Existing modal pattern: `.support-btn-floating` (fixed, z-1000) + `.support-modal-overlay` (fixed, z-1001) + centered `.support-modal-box` (400px) | support modal | The **search modal reuses this established pattern** (overlay + centered box) for visual consistency — same overlay idiom, not a new framework |
| G4 | Heartbeat loop polls `Heartbeat.ashx` every 120 s and already handles `status: logout` with reasons (`superseded`, `timeout`) | head script | Forced link closures (`TimedOut`, `Revoked`, `Superseded`…) surface through **the same poll** as an additive response field driving a passive banner dismissal — no new polling |
| G5 | `ddlCompany` (AutoPostBack) sits in `.header-profile` | header | While observing, the link's `CompanyID` is fixed (doc 27 §2.1) → the dropdown must render **disabled with an explanatory title**, not merely hidden |
| G6 | Menu is permission-gated `<li id="…">` items; `Bill.Master.cs` `UpdateSwitchUserVisibility` already toggles `pnlSwitchUser` | nav + code-behind | Visibility mechanics exist; this doc adds the **state matrix** (§7) for what is visible in each session state |
| G7 | **No `<meta name="viewport">`** exists; menus open on `li:hover` only; touch targets in header are ~18–30px | head + nav CSS | Mobile findings are **pre-existing debts**; the impersonation UI must not repeat them (§6) and recommends the minimal viewport/meta fix |
| G8 | Accessibility baseline: semantic `<header>/<nav>/<footer>` exist, but no skip link, no labels on `ddlCompany`, emoji-only affordances (🔄, 💬), no `aria-live` anywhere | whole file | §8 specifies AA-level treatment for the three new components and lists the adjacent pre-existing gaps without expanding scope |
| G9 | `GlobalNotification.ascx` renders at the top of `ContentPlaceHolder1` (known to throw in `OnInit` per docs/11) | content placeholder | The banner **must not** be implemented inside the content placeholder or depend on that control — it lives in the master's flex stack (G2) |
| G10 | Logout link already uses a `confirm()` dialog; doc 25 §6 mandates link-end **before** logout | `.header-profile` | Logout-while-observing gets one combined confirmation that names both actions (§5.4) |

**Z-index map (existing):** header 999 · support button 1000 · support modal 1001. The banner (G2) sits **in flow**, so it does not join this ladder; only the search modal does, at the support-modal tier.

---

## 1. Integration Zones — annotated frame wireframe

```
┌──────────────────────────────────────────────────────────────────────────────┐
│ Z1 HEADER  .top-header-bar                                                   │
│ [logo] [Company]   …permission-gated menu (#nav)…   [clock] [company ⌄]      │
│                                                     [Hi, ADMIN] [avatar]     │
│                                                     [🔄 Switch User]  [Logout]│  ← Z2 trigger (Z1a disabled while observing)
├──────────────────────────────────────────────────────────────────────────────┤
│ Z3 IMPERSONATION BANNER  (flex row, only when a link is active)              │
│ 👁 You are viewing as FATIMA (Salesperson) · Reason: "Q3 expense audit"      │
│ · started 14:02 ·                                    [ End Observation ]     │
├──────────────────────────────────────────────────────────────────────────────┤
│ Z4 CONTENT  .app-content (scrolls)                                           │
│   ┌ page content … GlobalNotification (G9) … ┐                               │
│   │                                                                 │        │
│   └─────────────────────────────────────────────────────────────────┘        │
│                                                       [💬 Raise a Concern]   │ ← floating (unchanged)
├──────────────────────────────────────────────────────────────────────────────┤
│ Z5 FOOTER  .app-footer                                                       │
└──────────────────────────────────────────────────────────────────────────────┘
     Z2 SEARCH MODAL (overlay z-1001, support-modal pattern, opened from Z1a)
```

Zone rules:

| Zone | Rendered when | Owner |
|---|---|---|
| Z1a `Switch User` trigger | `NORMAL_ADMIN` + `SwitchUser` permission held | master (`pnlSwitchUser`, existing) |
| Z3 banner | `LINK_ACTIVE` — resolved **per request** from the DB link (doc 25 L4); never from client state | master |
| Z2 modal | Opened only from Z1a; closed by Esc / ✕ / overlay click / successful act | master |
| Company dropdown | Disabled + explained while `LINK_ACTIVE` (G5) | master |

---

## 2. Header Placement (Z1/Z1a)

### 2.1 Decision

**Keep the trigger in `.header-profile`, exactly where `pnlSwitchUser` sits today.** Rationale: identity actions (user block → switch → logout) form a natural cluster at the right edge; the existing `UpdateSwitchUserVisibility` code-behind hook already lives there; and menu real estate in `.main-nav` is contested (15 top-level items already wrap).

### 2.2 Wireframe — the two header states

```
NORMAL_ADMIN (SwitchUser granted)
…  [14:02]  [AA Exports ⌄]   Hi, RAHUL          [🔄 Switch User]  Logout
                             Super Admin         ← Z1a visible
                                              (impersonating OTHER admins of
                                               same company: also eligible)

LINK_ACTIVE
…  [14:02]  [AA Exports ⌄†]  Hi, RAHUL                            Logout
                             Super Admin          ← Z1a HIDDEN (existing
                                                     UpdateSwitchUserVisibility)
   † dropdown disabled, tooltip: "Company switching is unavailable while
     observing another user"
```

### 2.3 Labeling and affordance rules

| Rule | Specification |
|---|---|
| Label | "Switch User" — matches the permission key and existing link text; no emoji-only control (🔄 retained as decorative, with real text — G8) |
| Control semantics | Rendered as a **button-like link** with `aria-haspopup="dialog"`; it opens Z2, it does not navigate-and-hope |
| Visibility ≠ authorization | Header link visible is a convenience only; `SecurePage`/`AuthGuard` re-check `SwitchUser` server-side on the modal's every act (baseline: menu visibility is not authorization) |
| Not-impersonating guard | If the admin already holds a live link (e.g., restored session edge), Z1a stays hidden — one link per principal is a DB constraint (doc 27 §3.1), the UI mirrors it |
| Company dropdown | Disabled during `LINK_ACTIVE` (G5) with explanatory tooltip; still visible so the layout doesn't shift |

---

## 3. Search Modal (Z2)

### 3.1 Pattern and placement

Reuses the support-modal idiom (G3): full-viewport overlay `rgba(0,0,0,.6)` + centered box. Width **520px** desktop / full-screen sheet ≤ 640px (§6). Opened from Z1a; **all switching acts happen here**, never in-page redirects — a one-flow, server-validated sequence.

### 3.2 Wireframe — four states

```
STATE A — SEARCH                                  STATE B — RESULTS (filtered)
┌─ Switch User ────────────────────────── ✕ ┐    ┌─ Switch User ──────────── ✕ ┐
│ Find a user in AA Exports to observe.     │    │ 12 matches for "fat"  [⌄ filter: all]
│ ┌───────────────────────────────┐ [Search]│    │ ┌─────────────────────────┐  │
│ │ 🔍 name or user id…           │         │    │ │ Fatima N  · FLM12       │  │
│ └───────────────────────────────┘         │    │ │ Salesperson     ✓ Eligible│ │
│ Same company only · excludes you          │    │ ├─────────────────────────┤  │
│                                           │    │ │ Faizan K · FLM07        │  │
│ [Cancel]                                  │    │ │ Manager ✗ Blocked:      │  │
└───────────────────────────────────────────┘    │ │   broader permissions   │  │
                                                 │ ├─────────────────────────┤  │
                                                 │ │ (top 50 · per doc 23 §6.1)│ │
                                                 │ └─────────────────────────┘  │
                                                 │ Selecting a row = confirmation step, no one-click switch
                                                 └──────────────────────────────┘

STATE C — CONFIRM (reason + intent)               STATE D — DENIED / FAILURE
┌─ Switch User ────────────────────────── ✕ ┐    ┌─ Switch User ──────────── ✕ ┐
│ You are about to view data as:            │    │ ✗ Switch not performed.      │
│ ┌───────────────────────────────────┐     │    │ Reason: target_in_use —      │
│ │ 👤 Fatima N · FLM12 · Salesperson │     │    │ another administrator is     │
│ └───────────────────────────────────┘     │    │ already observing this user. │
│ Reason (required, ≤500 chars):            │    │                              │
│ ┌───────────────────────────────────┐     │    │ [Back to search] [Close]     │
│ │ Q3 expense audit for FLM12        │     │    │ (denial is audited server-   │
│ └───────────────────────────────────┘     │    │  side as ImpersonationDenied │
│ ☐ I understand my name is recorded as     │    │  — doc 27 §2.2)              │
│   the actor on every change I make        │    └──────────────────────────────┘
│                                           │
│           [Cancel]  [Start Observation]   │
└───────────────────────────────────────────┘
```

### 3.3 Interaction flow (happy path)

```
Click Z1a → modal opens (focus lands in search box)
  → type ≥2 chars → Search (or debounce 300ms)
  → server: same-company, active, not-self, TOP 50, per-row eligibility (doc 27)
  → render rows with eligibility badges
  → select an ELIGIBLE row → confirm state C
  → type reason → check acknowledgement → Start Observation
  → server single transaction: per-act permission · eligibility · superset rule ·
    intent token · INSERT link + audit (doc 27 §9) → success
  → modal closes, banner Z3 appears, company dropdown disables (G5)
```

Rules per state:

| Rule | Specification |
|---|---|
| Eligibility badges | `✓ Eligible` / `✗ Blocked: <reason>` — blocked rows render **non-interactive** with the denial reason from the DB-backed check (`broader_permissions`, `pending_password_reset`, `target_in_use`, `inactive`…). Never render a Switch affordance for an ineligible row (doc 23 §6.1) |
| Reason field | Mandatory, ≤500 chars, pre-filled empty; the acknowledgement checkbox restates the read/write invariant in plain language ("my name is recorded as the actor on every change I make") — this is the attribution contract made visible |
| No one-click switching | Search → select → confirm is always 3 steps (T5 mitigation, intent token consumed server-side once) |
| Denials | State D renders the machine denial cause in human text; the server has already written `ImpersonationDenied` — the modal is display-only at that point |
| Double-click / race | Second submit shows a transient "Processing…" disabled state; a duplicate that loses the unique-index race lands in State D (`already_observing` / `intent_used`) — no error styling beyond the standard message |
| Progress fidelity | Buttons disable while the act is in flight; on success the **server-rendered** next page carries the banner — the modal never fakes state client-side |

---

## 4. Active Impersonation Banner (Z3)

### 4.1 Placement decision

**A dedicated flex row between header and `.app-content`** (G2) — not `position: fixed`, not a toast, not inside the content placeholder (G9).

Why this beats the alternatives:

| Alternative | Verdict |
|---|---|
| Fixed overlay floating over content | Occludes page controls; needs a new z-index tier above header (999); risks covering dialogs — rejected |
| Sticky bar inside `.app-content` | Disappears during in-content scroll on short viewports; also lives adjacent to the fragile `GlobalNotification` slot (G9) — rejected |
| Color-only recoloring of the header | Fails "color is not the only indicator" (§8); buries the acting identity in the same bar that shows the admin's own name — rejected |
| **Flex row (chosen)** | Guaranteed pixels on every page, zero overlap, prints correctly, no scroll-away, participates in the existing layout contract |

### 4.2 Wireframe

```
Desktop (banner row, full width):
┌──────────────────────────────────────────────────────────────────────────────┐
│ ⚠ 👁 OBSERVING AS: Fatima N (FLM12 · Salesperson)                            │
│    Reason: "Q3 expense audit for FLM12" · Since 14:02                        │
│                                                     [ End Observation ]      │
└──────────────────────────────────────────────────────────────────────────────┘
   styling contract: high-contrast amber/slate bar, dark text, left 4px accent
   border, distinct from the blue chrome of Z1 — non-dismissible, no ✕ control

Compact (single line, ≤ 1024px):
│ ⚠ Viewing as Fatima N · "Q3 expense audit"      [ End Observation ]         │
```

### 4.3 Content contract

| Element | Source | Rule |
|---|---|---|
| Acting identity + role | Link row (`TargetUser_Id`, role via target's roles) | Header user block (G1) **continues to show the admin** — principal and acting identity are deliberately in different bars (doc 25 L1 vs L3 made visible) |
| Reason (echo) | Link row `Reason` | Required by doc 23 §6.2 — the admin sees the justification they typed, on every page |
| Start time | Link `StartedUtc` (DB clock, doc 25 E8) | Shown as local time; optional elapsed minutes via the existing clock (clock.js) — enhancement, not requirement |
| `End Observation` | Opens rollback confirm (§5) | Primary action of the banner; 44px target (§8) |
| Liveness | Server-rendered per request; heartbeat (G4) updates only for forced closures | No client timer drives state — the banner is a view of the DB link |

### 4.4 What the banner must never do

- Never dismissible by the user (no ✕, no "don't show again") — it is a safety instrument, not a notification.
- Never rendered for users **without** an active link (other admins, the target, everyone else see nothing).
- Never rendered inside content pages' partial HTML (WebMethod partial updates) — it is master-frame chrome; content-region updates don't re-render it, and don't need to.
- Never the only place a forced closure is communicated — see §5.3.

---

## 5. Rollback UX

### 5.1 Voluntary rollback (End Observation)

```
[End Observation] click
  → confirm dialog (role=dialog): "End observation of Fatima N?
     You will return to your own view. This action is recorded."
      [Cancel]  [End Observation]
  → server: conditional close + audit (doc 27 §6, idempotent) → success
  → banner row disappears (flex stack reflows), company dropdown re-enables,
     menu re-resolves to the ADMIN's permissions on the next render
  → focus returns to the header user block; a brief "Now viewing as yourself"
     toast confirms the transition (aria-live=polite)
```

**No reason is requested on rollback** (reason is a start-time obligation). The confirm dialog exists to prevent accidental clicks on a large touch target, not to gather data.

### 5.2 Rollback failure (rare)

If the close transaction fails (F8), the banner **stays** and the toast says the end was not completed — retry by clicking again. The UI never hides the banner on client optimism; the server render is the truth.

### 5.3 Forced rollback (link dies under the admin)

Causes: `TimedOut` (4 h), `Revoked`, `TargetIneligible` (password reset initiated), lease death (`Superseded`, `IdleTimeout`) — taxonomy per doc 25 §2.1.

```
Heartbeat poll (G4, every 120s) returns additive field: linkClosed = <reason>
  → banner removes itself; polite toast: "Observation ended — <human reason>.
     You are viewing as yourself."
  → company dropdown re-enables; page content itself is NOT auto-refreshed
     (data already fetched shows as fetched; next navigation is principal-scoped)
```

Humanized reason copy (server-provided or mapped client-side):

| EndReason | Toast copy |
|---|---|
| `TimedOut` | "Observation ended: maximum observation time reached." |
| `Revoked` | "Observation ended by an administrator." |
| `TargetIneligible` | "Observation ended: this user's account state changed." |
| `Superseded` / `IdleTimeout` / `SessionExpired` | Handled by the **existing** logout heartbeat copy (G4) — the link closure rides the same forced-logout path, no separate UX |

### 5.4 Logout while observing

The existing logout `confirm()` (G10) becomes state-aware while a link is live:

```
"Impersonation is active. Logging out will end your observation of Fatima N
 and sign you out. Continue?"        [Cancel]  [Log out]
```

One confirmation, two named consequences — matching the doc 25 §6 sequence (end link → session teardown) without a second dialog.

### 5.5 Two tabs, one session

State is server-side (doc 25 E11): ending observation in tab 2 leaves tab 1's banner stale until its next server interaction, then it disappears silently. No sync machinery, no warning copy — specified behavior, documented here so it isn't "fixed" into complexity.

---

## 6. Mobile Behavior

### 6.1 Pre-existing constraints (verified, G7)

- **No viewport meta** → most phones render the 1000px+ desktop layout zoomed-out; the flex header still stacks because `.main-nav` wraps.
- **Hover-only dropdowns** (`#nav li:hover > ul`) are unusable on touch — this is an existing defect in the frame, **not introduced by this feature**, but the impersonation UI must not depend on hover anywhere.
- Header controls are small (~18–30px) — below the 44px touch minimum.

### 6.2 Recommendations for the impersonation surfaces

**Minimal frame fix (recommended, small, benefits everything):** add the standard viewport meta and a `:focus-within`/tap fallback for the top-level nav menus. Out of this feature's scope to redesign nav; without at least the viewport meta, none of the following renders as intended on phones.

```
Phone (≤ 640px) — stacked state:
┌────────────────────────────┐
│ [logo] AA Exports     [≡]  │   header condenses (menu collapses per frame fix)
├────────────────────────────┤
│ ⚠ Viewing as Fatima N      │   Z3 banner, stacked:
│    "Q3 expense audit"      │   identity + reason on lines 1–2,
│    [ End Observation ]     │   action button full-width (44px)
├────────────────────────────┤
│  content…                  │
└────────────────────────────┘

Search modal on phone → full-screen sheet:
┌────────────────────────────┐
│ Switch User            ✕   │
│ ┌────────────────────────┐ │
│ │ 🔍 search users…       │ │
│ └────────────────────────┘ │
│ ┌────────────────────────┐ │
│ │ Fatima N · FLM12       │ │
│ │ Salesperson ✓ Eligible │ │   56px rows, full-width tappable
│ ├────────────────────────┤ │
│ │ Faizan K ✗ Blocked:    │ │
│ │ broader permissions    │ │   blocked rows: no tap affordance
│ └────────────────────────┘ │
│        [Cancel]            │
└────────────────────────────┘
```

| Rule | Specification |
|---|---|
| Touch targets | All impersonation controls ≥ 44×44px (trigger, banner button, modal rows) |
| Modal on small screens | Full-screen sheet (not centered box); ✕ top-right; body scroll locked while open |
| Banner on small screens | Two-line stacked layout; the End Observation button remains visible without scrolling (it is the emergency exit — never pushed below the fold) |
| No hover dependence | Eligibility details appear on tap/expand, not `:hover`; the modal works with keyboard and touch symmetrically |
| Orientation change | Banner reflows within the flex stack naturally; no fixed positioning to break |
| Feature phone / JS-off fallback | The trigger is a server-rendered control: without JS it navigates to the SwitchUser page flow rather than opening the modal — degraded but functional, all server-side gates unchanged |

---

## 7. Permission Visibility (state matrix)

One table governs every visible surface. State is resolved server-side per request; the matrix is what the master renders, not what it checks.

| Surface | NORMAL_ADMIN (has `SwitchUser`) | NORMAL_ADMIN (no `SwitchUser`) | LINK_ACTIVE (observing admin) | Target user / everyone else |
|---|---|---|---|---|
| Z1a Switch User trigger | visible | **hidden** | **hidden** (existing `UpdateSwitchUserVisibility`) | hidden |
| Z3 banner | absent | absent | **visible** (self only) | absent |
| Company dropdown | enabled | enabled | **disabled + tooltip** | per own session |
| Header user block | "Hi, {admin}" + admin role | same | **still "Hi, {admin}"** — principal never changes | own identity |
| Menu (`#nav`) | admin's permissions | own | **target's permissions** (doc 23 §8: page gates resolve to the target while observing) | own |
| `lblRole` (header) | admin's role | own | admin's role (banner carries the target's role) | own |
| SwitchUser.aspx direct URL | page renders | 403 by `SecurePage` | page renders; any TryStart refuses (`nested`/`already_observing`) → State D | 403 |
| `ImpersonationAudit` menu item (Phase 3) | visible iff `ImpersonationAudit` held | per grant | visible iff held (audit ≠ switch — separation of duties, doc 27 §4) | per grant |

Design notes:

1. **The header never lies about who is signed in.** Even mid-observation, "Hi, RAHUL" persists; the acting identity lives in the banner. This is the session model (doc 25 §1.3) expressed spatially — and it prevents the "which am I?" confusion that identity-swap designs create.
2. **Menu mutation is expected and honest.** The admin sees the menu narrow/expand to the target's grants while observing. This is the point of the feature (reproduce their view) and is self-explanatory given the banner above it.
3. **Separation of duties** stays: seeing the banner, having `SwitchUser`, and having `ImpersonationAudit` are three independent grants; no surface assumes two of them together.

---

## 8. Accessibility (WCAG 2.1 AA targets for the new surfaces)

| Component | Requirement | Specification |
|---|---|---|
| **Z3 banner** | Announced without stealing focus | `role="status"` + `aria-live="polite"` on the banner container; when it appears after a switch, the acting-identity text is announced once. Forced closures announce via the toast (`aria-live="polite"`); **no `assertive`/`alert`** — the user is mid-task, not in danger |
| Z3 banner | Contrast | Dark text on amber ≥ 4.5:1; the 4px accent border is decorative — color and icon are never the sole signal; the word "OBSERVING AS" carries the meaning in text |
| Z3 banner | Persistent | Non-dismissible by design (§4.4); remains in the accessibility tree on every page — screen-reader users hear the acting context per page load |
| **Z2 modal** | Semantics | `role="dialog"`, `aria-modal="true"`, labelled by the "Switch User" heading; ✕ is a real button with `aria-label="Close"` |
| Z2 modal | Focus management | On open: focus → search input. On close (Esc / ✕ / overlay / success): focus returns to Z1a. Focus is **trapped** inside while open; the overlay click-closes affordance never swallows focus silently |
| Z2 modal | Results list | Rows are a listbox pattern (arrow-key navigation, Enter selects) with the eligibility badge read as part of the row's accessible name: "Fatima N, FLM12, Salesperson, Eligible" / "…, Blocked, broader permissions" |
| Z2 modal | Form fields | Search input has a visible label ("Find a user…"); reason textarea labelled and announced with its 500-char limit; acknowledgement checkbox has an explicit label (the attribution sentence) |
| Z2 modal | Errors | State D text is announced (`role="alert"` scoped inside the dialog is acceptable at act time); the offending field (empty reason) gets `aria-invalid` + inline message |
| **Z1a trigger** | Control role | Button semantics with `aria-haspopup="dialog"`; the 🔄 emoji is `aria-hidden` decoration |
| Disabled company dropdown (G5) | Explained | `aria-disabled="true"` + visually-hidden or tooltip text "Company switching is unavailable while observing another user"; never a bare disabled select with no context |
| Toasts (§5.1/§5.3) | Announced | `aria-live="polite"` region reused for both success and forced-closure messages |
| Motion | Respect reduced motion | Banner/toast appear without animation when the OS requests reduced motion |
| Target size | 44px min | All interactive impersonation controls (§6.2), including in the desktop banner (pointer accessibility) |

**Adjacent pre-existing gaps (logged, not in scope):** no skip-link to content; `ddlCompany` has no label; support modal lacks focus trap; header links under 44px. These should be batched into a separate accessibility-hardening pass — this feature must not wait on them, but must not worsen them.

---

## 9. End-to-End Interaction Flows (summary sequences)

### 9.1 Start observation

```
ADMIN            MASTER(Z1a)        MODAL(Z2)         SERVER                DB
  │ click Switch ──▶│                 │                  │                    │
  │                 │ open dialog ───▶│                  │                    │
  │  type "fat" ───────────────────▶  │ GET results ────▶│ eligibility query  │
  │                 │                 │ ◀── rows+badges ──│ (doc 27 §3.1 idx)  │
  │  pick Fatima, reason, ☐, confirm ─▶│ POST act ──────▶│ gates+superset     │
  │                 │                 │                  │ TX: link+audit ───▶│
  │                 │                 │ ◀── ok ──────────│                    │
  │                 │ ◀─ re-render: banner ON, dropdown disabled             │
```

### 9.2 Voluntary rollback

```
  │ click End Observation (Z3) ──▶ confirm dialog ──▶ POST end ──▶ server TX
  │                                                          close+audit ──▶ DB
  │ ◀─ re-render: banner OFF, dropdown enabled, toast "viewing as yourself"
```

### 9.3 Forced rollback (no user action)

```
HEARTBEAT(120s) ──▶ server: lease/max-duration/target check fails
                     close link (reason) ──▶ DB        (doc 25 T6–T9)
  ◀─ additive response field linkClosed=<reason>
BANNER removes itself · toast (human reason) · dropdown re-enables
Next full navigation re-renders principal-scoped menu
```

### 9.4 Denied start

```
  │ confirm in modal ──▶ POST act ──▶ server gate FAILS ──▶ audit
  │                                     ImpersonationDenied ──▶ DB
  │ ◀─ modal State D with humanized cause (display-only; no state change)
```

---

## 10. Boundary Notes (explicitly out of the banner's frame)

| Surface | Treatment |
|---|---|
| Print templates (`corporate/business/print/*`), ashx handlers | No master frame → **no banner**. Authorization at these boundaries is `AuthGuard`'s job (doc 25 E6), not the banner's; the banner is UI transparency, not a control |
| `SessionKeepAlive*.aspx`, Heartbeat | No chrome, no banner (doc 25 E7 — heartbeat validates the principal only; the additive `linkClosed` field is the only UI touchpoint) |
| Kiosk (`admin/`) | Different frame, different realm — nothing impersonation-related renders there (G8 of doc 27) |
| Popups that don't inherit the master | inherit nothing → no banner; acceptable, since every act server-side still resolves `SecurityContext` |

---

## 11. Requirement Traceability

| Requested item | § Where |
|---|---|
| Header placement | §2 (keeps existing `pnlSwitchUser` zone; state-dependent visibility) |
| Search modal | §3 (pattern, 4 states, flow, denial handling) |
| Active impersonation banner | §4 (placement decision, wireframe, content contract) |
| Rollback UX | §5 (voluntary, failure, forced, logout, two-tab) |
| Mobile behavior | §6 (viewport fix, sheet modal, touch targets, no-hover rule) |
| Permission visibility | §7 (full state matrix incl. audit separation of duties) |
| Accessibility | §8 (AA targets per component + pre-existing gap log) |
| Wireframes with interaction flow | §1–§6 wireframes; §9 flows |
| No ASPX code | Honored — structural specification only |

---

*End of document. Wireframes and interaction architecture only — no ASPX, C#, JavaScript, or CSS was written or modified in producing this design.*
