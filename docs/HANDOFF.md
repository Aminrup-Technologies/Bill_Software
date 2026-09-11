# Handoff

Lightweight working status. Prefer this file over chat history. Update the sections below when work starts or finishes. Do not duplicate [`ARCHITECTURE.md`](ARCHITECTURE.md) or [`22_Security_Baseline.md`](22_Security_Baseline.md).

---

## Current Status

Switch User post-Start runtime failure is **closed**. ADR-001 unchanged (`AuthGuard` / `SecurePage` / `UserCompanyAccess` / fail-closed). Runtime lives only in `Bill_Software/corporate/business/app/SwitchUser.aspx.cs`.

**Root cause:** after `ImpersonationRuntime.Start`, `Session["USERID"]` is the target. `AuthGuard.HasPermission("SwitchUser")` reads that identity, so `EnsurePage(..., PermissionKey)` 403’d the impersonated session. `Redirect(..., false)` without `CompleteRequest()` let the Switch User request keep running.

**Resolution:**
- Permission key is omitted only when `Session[ImpersonationGovernance.SessionLinkKey] != null`. That key is written solely by `ImpersonationRuntime.ApplyTargetSession` and removed on rollback. Session + company still run (`EnsurePage(this, true, null)`).
- Successful Start: `Response.Redirect("~/corporate/business/app/home.aspx", false)` then `Context.ApplicationInstance.CompleteRequest()`.

**PASS matrix:** initiate impersonation; End Impersonation banner (`ActorHoldsSwitchUser` / `CloseCurrent(ManualRollback)`); INV-13 nested (page hides list; `GateActor` still denies); redirect has no handler code after `CompleteRequest`; rollback restores actor identity, `CompanyID` never swapped; non-impersonating users without `SwitchUser` still 403.

---

## Completed Work

- v2.1 Security Foundation (AuthGuard, SecurePage, tenant membership, resource scope phases). Contract: `docs/22`.
- Page catalog + data dictionary + UAT catalog generators.
- Impersonation governance + dormant runtime + UAT activation docs (`ADR-001`, `docs/29`–`docs/37`).
- Switch User runtime patch: `SessionLinkKey` permission exception + `CompleteRequest` redirect. Investigation closed (no remaining Switch User runtime work).
- Ponytail + solution-docs Cursor rules (existing).
- This bootstrap: `docs/ARCHITECTURE.md`, `docs/HANDOFF.md`, `docs/CURSOR_RULES.md`, `Bill_Software/.cursor/rules/project.mdc`.

---

## Current Task

None. Switch User runtime investigation is closed.

---

## Pending Work

- _TBD — next scoped change (not Switch User runtime)._
- Known debt (do not start unless requested): leftover concatenated SQL on invoice / PO / stock / scheduler surfaces; kiosk plaintext; some direct `SmtpClient` pages; UAT objects not applied.
- Product stops still closed: Decision #8 (`ReportingManagerId` ACL), Decision #9 (HQ cross-company), `machineKey` rotation, secret cutover ops.

---

## Constraints

- Repository-first. Ignore chat unless summarized here or in `ARCHITECTURE.md` / `CURSOR_RULES.md`.
- Preserve architecture in `ARCHITECTURE.md` and `docs/22`. No parallel AuthN, no JWT, no Identity, no Windows Service host.
- Modify only files explicitly referenced. Unified diffs only.
- Parameterized SQL + `CompanyID` (or documented ownership). No static user state in `.aspx.cs`.
- Do not change `AuthGuard`, `SecurePage`, `UserRoleAssignment`, or `UserCompanyAccess` without architectural review.
- Do not edit README.md or CONTRIBUTING.md unless a task names them.
- No secrets in diffs, docs, or commits.

---

## Next Action

Wait for an explicit `@filename` task. On start: read `ARCHITECTURE.md` → this file → `CURSOR_RULES.md`, then only the referenced implementation files. Do not reopen Switch User runtime unless a new defect is named.

---

## Notes

- Shared AuthN/tenancy lives in `page-catalog/SHARED_CONTEXT.md`; do not copy it into domain catalogs.
- Module docs `01`–`13` are leftover pointers. Sales-visit audit is a dated snapshot (`sales-visit-workflow-audit/00_SNAPSHOT_STATUS.md`).
- Replace the Current Task / Pending Work / Next Action bullets when work is assigned. Delete stale chat-derived claims; this file is the source of session truth.
