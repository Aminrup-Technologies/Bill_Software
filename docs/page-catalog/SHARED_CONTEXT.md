# Shared context (read once)

This file is the **only** place page-catalog documents may point for AuthN, tenancy, Bill.Master, and print-gate behavior.
Handlers, helpers, SQL scripts, and SP call sites live in [SHARED_RUNTIME.md](SHARED_RUNTIME.md).
Live UAT objects, FKs, and code-vs-database gaps live in [SHARED_SCHEMA.md](SHARED_SCHEMA.md).
Page ↔ table/SP verbs live in [DATA_DICTIONARY.md](DATA_DICTIONARY.md).
Do **not** restate these rules inside domain catalogs or per-page notes.

## Where the canonical text lives

| Topic | Canonical document | What it covers |
|-------|--------------------|----------------|
| Solution overview, stack, repo layout | [README.md](../../README.md) | Product identity, Ponytail standards, setup |
| Ponytail coding rules | [Bill_Software/.cursor/rules/ponytail.md](../../Bill_Software/.cursor/rules/ponytail.md) | No static leaks, parameterized SQL, tenant scope |
| AuthN / AuthZ contract | [22_Security_Baseline.md](../22_Security_Baseline.md) | Engineering contract for every future change |
| AuthN / AuthZ review | [14_Authentication_Authorization_Architecture.md](../14_Authentication_Authorization_Architecture.md) | Defects A-01…A-31, dual role systems |
| Phase history 0B–3 | [15](../15_Phase0B_Secrets_Deployment.md)–[21](../21_Phase3_Infrastructure_Hardening.md) | Implementation notes — not page catalogs |
| Sales-visit deep dive | [sales-visit-workflow-audit/00_SNAPSHOT_STATUS.md](../sales-visit-workflow-audit/00_SNAPSHOT_STATUS.md) | Which D-* findings are remediating vs still live |
| Handlers, helpers, SQL, SP callers | [SHARED_RUNTIME.md](SHARED_RUNTIME.md) | Non-page surface — do not copy into domains |
| Live UAT schema | [SHARED_SCHEMA.md](SHARED_SCHEMA.md) | Tables, FKs, missing objects vs code, UAT-only procs |
| Page ↔ table/SP dictionary | [DATA_DICTIONARY.md](DATA_DICTIONARY.md) | Per-object SELECT/INSERT/UPDATE/DELETE/EXEC |
| UAT columns + SP signatures | [UAT_CATALOG.md](UAT_CATALOG.md) | All 111 tables; 38 procs (no bodies) |

## Runtime facts used by every ERP page (do not copy)

1. **Master:** `corporate/business/app/Bill.Master` validates `AuthGuard.TryValidateSession`, binds `UserCompanyAccess` companies, renders menu from `Permissions` ∩ `UserRoles` ∩ `RolePermissions`. **UAT does not have `UserCompanyAccess`** — membership queries fail closed ([SHARED_SCHEMA.md](SHARED_SCHEMA.md) §4).
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
