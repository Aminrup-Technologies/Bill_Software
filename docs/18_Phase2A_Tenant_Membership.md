# Phase 2A — Tenant Membership Foundation (A-18)

**Date:** 2026-09-07  
**Depends on:** PR #66 / #67 / #68 / #69. `AuthGuard.HasPermission` SQL, `SecurePage` permission keys, `UserRoles` / `tbl_login.RoleId`, login, ActiveSessions, and ERP `CompanyID` filters are not changed.  
**STOP honored:** `Session["CompanyID"]` remains the runtime tenant. `dbo.UserCompanyAccess` is the membership ACL. No all-company fallback. No invented multi-company default.

---

## What changed

The company switcher is no longer bound to every active `tbl_Company` row. `Session["CompanyID"]` is set only when `UserId + CompanyID` is an **active** `UserCompanyAccess` row for an existing active company.

Downstream ERP queries still read `Session["CompanyID"]` / `CompanyContext.CurrentCompanyID`. They were not rewritten.

---

## Membership contract

| Column | Meaning |
|--------|---------|
| `UserCompanyAccess.UserId` | `tbl_login.Id` (numeric), same key as `Session["UserDbId"]` |
| `UserCompanyAccess.CompanyID` | `tbl_Company.ID` |
| Unique | `(UserId, CompanyID)` |
| `IsActive` | membership must be 1 |

Company rows still follow the previous dropdown predicate: `IsActive = 1 OR IsActive IS NULL`. Inactive membership or inactive company is rejected. Presence in `tbl_Company` alone is not access.

---

## AuthGuard API (centralized)

| Method | Behavior |
|--------|----------|
| `GetAuthorizedCompanies()` | Active memberships joined to active companies. Empty when unauthenticated, `UserDbId` missing, table missing, or lookup fails. |
| `UserCanAccessCompany(id)` | Database EXISTS check. Does not trust dropdown, ViewState, query string, or posted values. |
| `UserCanAccessCurrentCompany()` | Same check for `Session["CompanyID"]`. |
| `ResolveInitialCompanyId()` | 0 memberships → 0. 1 → that company. Many → `tbl_login.CompanyID` **only if it is already a membership**. Otherwise 0 (selector stays; no first-item default). |
| `ClearUnauthorizedCompanySession()` | Drops `Session["CompanyID"]` when membership is gone. |
| `TryEnsureHomeMembership` | AddUser only: insert the same home company already written to `tbl_login.CompanyID`. |

`EnsurePage` / `EnsurePageAny` / `EnsureWebMethod` still require a session. When company is required:

- missing `CompanyID` → **401** (same as Phase 0A)
- `CompanyID` set but not an active membership → clear session company, **403**

Permission keys are unchanged. Membership is not a `PermissionKey`.

---

## Company switcher (`Bill.Master`)

| Before | After |
|--------|--------|
| `SELECT ID, Name FROM tbl_Company WHERE IsActive = 1 OR IsActive IS NULL` | `AuthGuard.GetAuthorizedCompanies()` |
| Empty session → `tbl_login.CompanyID`, else **first dropdown item** | Empty/unauthorized session → `ResolveInitialCompanyId()` only |
| `SelectedIndexChanged` writes `SelectedValue` | Parse int, `UserCanAccessCompany`, then write. Reject otherwise. |

Control, display field (`Name`), session key, AutoPostBack, and redirect-to-current-URL are preserved. `GetMenuControl` is unchanged.

---

## Initial company after login

Login still does **not** set `CompanyID`. The first Master `Page_Load` (`home.aspx` is not a `SecurePage`) binds memberships and resolves an initial company as above.

**Lifecycle (pre-existing, not changed):** `SecurePage.OnInit` runs before Master `Page_Load`. Deep-linking to a `SecurePage` immediately after login, before any Master page has set `CompanyID`, still 401s. Do not invent company selection inside `EnsurePage`.

---

## Provisioning and reconciliation

- **AddUser** inserts `(newUserDbId, current CompanyID)` into `UserCompanyAccess` in the same transaction as `tbl_login`. That is the home tenant already stored on the login row, not every company.
- **`UserCompanyAccess.sql`** is the CREATE script. Run by a DBA. Not executed by the app.
- **`user_company_access_reconciliation.sql`** reports gaps. Optional INSERT (commented) copies `tbl_login.CompanyID` only. It does not grant all companies. Do not run from the application.

Existing users have **no** membership rows until that optional INSERT is reviewed. The switcher then shows an empty list and protected pages fail closed. That is intentional.

---

## Out of scope (unchanged)

Login credentials, password reset, OTP, ActiveSessions, `HasPermission` join, `SecurePage` permission properties, RoleId/UserRoles, menu rendering, print maps, invoice/PO/stock/visit/scheduler/reporting query bodies, card kiosk.
