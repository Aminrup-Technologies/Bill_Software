# Shared context (read once)

This file is the **only** place page-catalog documents may point for cross-cutting behavior.
Do **not** restate these rules inside domain catalogs or per-page notes.

## Where the canonical text lives

| Topic | Canonical document | What it covers |
|-------|--------------------|----------------|
| Solution overview, stack, repo layout | [README.md](../../README.md) | Product identity, Ponytail standards, setup |
| Ponytail coding rules | [Bill_Software/.cursor/rules/ponytail.md](../../Bill_Software/.cursor/rules/ponytail.md) | No static leaks, parameterized SQL, tenant scope |
| AuthN / AuthZ contract | [22_Security_Baseline.md](../22_Security_Baseline.md) | Engineering contract for every future change |
| AuthN / AuthZ review | [14_Authentication_Authorization_Architecture.md](../14_Authentication_Authorization_Architecture.md) | Defects A-01…A-31, dual role systems |
| Phase history 0B–3 | [15](../15_Phase0B_Secrets_Deployment.md)–[21](../21_Phase3_Infrastructure_Hardening.md) | Implementation notes — not page catalogs |
| Sales-visit deep dive | [sales-visit-workflow-audit/](../sales-visit-workflow-audit/) | State machine, tables, defects D-* |

## Runtime facts used by every ERP page (do not copy)

1. **Master:** `corporate/business/app/Bill.Master` validates `AuthGuard.TryValidateSession`, binds `UserCompanyAccess` companies, renders menu from `Permissions` ∩ `UserRoles` ∩ `RolePermissions`.
2. **Page gate:** pages that inherit `SecurePage` call `AuthGuard.EnsurePage` / `EnsurePageAny` in `OnInit` using `RequiredPermissionKey` (must match the `id` on the menu `<li>` in Bill.Master).
3. **Print gate:** `corporate/business/print/*.aspx` have **no master**. They call `AuthGuard.EnsurePrint(this, resourceKey, queryValue)`. Resource `"unmapped"` **fails closed**.
4. **Tenant:** `CompanyContext.CurrentCompanyID` from `Session["CompanyID"]`. Queries on tenant tables must use `@CompanyID`.
5. **Identity:** `Session["USERID"]` (business key) + `Session["SessionToken"]` (`dbo.ActiveSessions`).
6. **Card kiosk** (`index_card.aspx`, `/admin/*`, `Print/id_card1.aspx`) is a **separate** plaintext `tbl_card_login` app. Do not document it as ERP RBAC.

## What a page entry may contain

Only facts that differ from the rows above:

- Purpose / menu label
- Permission key or print resource
- QueryString / WebMethods unique to the page
- Tables and CRUD verbs **this file** uses
- Links to an existing narrative module doc
- Defects or quirks **unique** to this page (misspelled filename, fail-closed print, no CompanyID, leftover page)

If a sentence would be true for most pages in the domain, put it in the domain blurb (one sentence) or leave it here.
