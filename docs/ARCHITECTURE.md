# Project_FLMX Architecture

**Status:** Canonical evergreen architecture  
**Product:** Project_FLMX / AminrupERP / Flame-ex  
**Host:** ASP.NET Web Forms on IIS (.NET Framework 4.5.2)  
**Data:** Microsoft SQL Server via ADO.NET (`DbConn`)

This file is the stable architecture map for Cursor and humans. Documentation map: [`SOLUTION_INDEX.md`](SOLUTION_INDEX.md). Security contract: [`22_Security_Baseline.md`](22_Security_Baseline.md). Page census: [`page-catalog/PAGE_INVENTORY.md`](page-catalog/PAGE_INVENTORY.md). Shared AuthN/tenancy facts: [`page-catalog/SHARED_CONTEXT.md`](page-catalog/SHARED_CONTEXT.md). Non-page runtime: [`page-catalog/SHARED_RUNTIME.md`](page-catalog/SHARED_RUNTIME.md).

Do not invent layers, hosts, or services that are not in this repository.

---

## System overview

FLMX is a multi-tenant field-sales and commercial ERP. It covers visit planning/execution (GPS), manager review, visit expenses, quotations, vendor PR/PO, invoices/proforma/DPCC, payments, attendance/HR, masters, and communications.

Primary roles: **salesperson** (own visits, expenses, quotes) and **manager** (company-scoped review). ERP user admin lives under `corporate/business/app/`, not `admin/`.

`admin/` plus `index_card.aspx` is a **separate ID-card kiosk** (`tbl_card_login`). It is not ERP AuthN/AuthZ.

There is **no** Windows Service, Hangfire, Quartz, or out-of-process job host in this repository. Work runs in IIS request pipelines and SQL Server objects invoked from those requests.

---

## Solution structure

```
Bill_Software/                          # git root
├── docs/                               # architecture, security, page catalogs
├── db/                                 # patch/seed SQL (not live DDL)
├── Visual Studio I2I INC Web Application.sln
└── Bill_Software/                      # Web Forms project
    ├── index.aspx                      # ERP login
    ├── Web.config / Web.Release.config
    ├── DB_UTILITY.cs / MoneyConvDS.cs
    ├── corporate/business/app/         # primary ERP (Bill.Master)
    ├── corporate/business/print/       # print pages (no master)
    ├── corporate/business/sql/         # in-repo SQL patches
    ├── admin/                          # kiosk (card.Master)
    └── Uploads/                        # attachments
```

Open `Bill_Software/Bill_Software.csproj` (VS 2015). NuGet: `packages.config`.

Live schema is **not** versioned. Infer tables from ADO.NET and UAT catalogs. Do not treat `docs/01`–`docs/13` as a second census.

---

## Module boundaries

Commercial spine (tables, not page names):

```
Quotation / client PO [tbl_Quotation]
  → DPCC [tbl_Chalan] and/or Proforma [tbl_Proforma]
  → Tax invoice [tbl_Invoice]
  → Payment received [tbl_invoice_payment]
Vendor PR [tbl_RequisitionMain] → Vendor PO [tbl_PO_Header]
Vendor purchase [tbl_Purches] → purchase payment [tbl_Purchess_payment]
Visit: visit_planner → daily_rpt → vw/srch_dailyrpts + expense_entry
  (tbl_SalesVisitReport; visit expenses tbl_Expenses ≠ GL expenses)
Hydrant: qsHydrentQuotation → HydrentInvoice (hidden menu)
Kiosk: tbl_card_login / tbl_employee (isolated)
```

Domain catalogs (`docs/page-catalog/DOMAIN_*.md`) own unique page facts. Do not copy AuthN, tenancy, or Bill.Master into domain files.

Do not merge kiosk code into ERP `AuthGuard` / `ActiveSessions`. Do not introduce a second AuthN stack (no ASP.NET Identity, no JWT).

Protected assets (architectural review required): `AuthGuard.cs`, `SecurePage.cs`, `UserRoleAssignment.cs`, `UserCompanyAccess.sql`, `docs/22_Security_Baseline.md`.

---

## ASP.NET Web Forms architecture

- Pages: `.aspx` + `.aspx.cs`. ERP chrome: `Bill.Master`. Kiosk: `card.Master`. Print: no master.
- Postback lifecycle and existing control IDs/event signatures are part of the contract. Do not auto-bulk-fill grids; selection stays manual.
- Identity: `Session["USERID"]` (`tbl_login.User_Id`) + `Session["SessionToken"]` (`dbo.ActiveSessions`). Numeric PK: `Session["UserDbId"]`.
- Tenant: `Session["CompanyID"]` via `CompanyContext.CurrentCompanyID` (nested on `Bill.Master.cs`). Login does **not** set `CompanyID`; Master `Page_Load` does. `SecurePage.OnInit` runs before that — deep-link without company 401s.
- Page gate: `SecurePage` → `AuthGuard.EnsurePage` / `EnsurePageAny` using `RequiredPermissionKey` (must match Bill.Master menu `id`).
- Print gate: `AuthGuard.EnsurePrint(this, resourceKey, queryValue)`. `"unmapped"` fails closed.
- Web.config Forms/Membership/RoleManager sections are **not** the ERP auth model. Companion Forms cookie exists so `/Uploads` anonymous deny works.
- **No public static DataTables, collections, or user objects in `.aspx.cs`** (IIS AppDomain leak).

Request lifecycle (contract): Authentication → ActiveSessions → company membership (`UserCompanyAccess`) → page permission → resource authorization → business operation. Menu visibility is not authorization.

---

## Business layer responsibilities

There is no separate BLL/DAL project. Behavior lives in page code-behind plus a small helper set. Do not add wrapper facades around existing ADO.NET.

| Type | Responsibility |
|------|----------------|
| `.aspx.cs` | Page workflow, validation, SQL for that surface |
| `AuthGuard` / `SecurePage` | Session, permission, print gates |
| `CompanyContext` | Current tenant id from session |
| `AppSecrets` | `DbConn` and typed AppSettings |
| `DB_UTILITY` | Legacy ADO helper; many pages still `new SqlConnection` |
| `PasswordHasher` / `PasswordResetService` | PBKDF2 login/reset (`dbo.PasswordResetTokens`) |
| `CommunicationGateway` | Config SMTP + MSG91 (some pages still use direct `SmtpClient`) |
| `UserRoleAssignment` | Seed/sync `UserRoles` from `tbl_login.RoleId` |
| Impersonation types | Fail-closed `SwitchUser`; production flag off |
| `DuplicationService` | Cross-tenant vendor/customer copy. **v1.1 complete** at `88f9d0f` (PR **#92**, tag `cross-tenant-v1.1`): single + bulk + Skip Already Duplicated. Docs: [`34`](34_CrossTenant_Duplication_Architecture.md), [`44`](44_CrossTenant_Final_Integration.md)–[`46`](46_CrossTenant_Release_v1.1.md). |
| `InvoiceListHelper` / `PurchaseOrderPrintHelper` / `MoneyConvDS` | Shared list/print/amount-in-words |

CRUD that mutates business data should log `tbl_SystemNotification` before commit (Ponytail). Preserve existing transaction boundaries.

Business rules stay unless a change explicitly states otherwise. Decision #8: `ReportingManagerId` is not ACL. Decision #9: no HQ all-tenant role.

---

## SQL Server interaction principles

- Parameterized `SqlCommand` only. No concatenation of user, session, or tenant values into SQL.
- `using` on `SqlConnection`, `SqlCommand`, `SqlDataReader`.
- Tenant tables: `AND CompanyID = @CompanyID` (or documented ownership predicate). Never trust client `CompanyID`.
- Prefer existing stored procedures where the page already calls them; bodies live in SQL Server, not this repo. Call-site index: `SHARED_RUNTIME.md`.
- Invoice / PO / stock / scheduler concatenated SQL is **historical debt**. New statements in those areas still follow this baseline.
- `.sql` files are patches/snapshots, not the live database. UAT may lack objects (`UserCompanyAccess`, `PasswordResetTokens`) — missing membership/reset **fails closed**.

---

## Authentication and authorization overview

- Login: `index.aspx` against `tbl_login` (PBKDF2; leftover plaintext upgraded in place). Concurrent session: deactivate prior `ActiveSessions`. Idle: `Heartbeat.ashx` (~30 minutes).
- Re-validate `ActiveSessions.IsActive` on Master load.
- Authorization: `UserRoles` → `RolePermissions` → `Permissions.PermissionKey` at **action** (`EnsurePage` / WebMethod / `EnsurePrint`), not only menu hide.
- Resource scope: company + owner or manager-dashboard as documented in `docs/19` / `docs/22`. Guessed ids must not cross company/owner.
- Kiosk login is plaintext `tbl_card_login`, parameterized SQL, no `ActiveSessions`. Isolated.
- Impersonation: ADR-001. Stays off unless `SwitchUser=true` **and** Super Admin grant **and** an explicit runtime caller. Do not rewrite AuthGuard for impersonation.

---

## Multi-tenant isolation principles

- Tenant key: `CompanyID` / `CompanyId` on tenant-scoped tables.
- Membership: `dbo.UserCompanyAccess` for `Session["UserDbId"]` + `Session["CompanyID"]`. Unauthorized switch rejected.
- Cross-Tenant Duplication **v1.1** is closed (`88f9d0f`). `DuplicationService` copies vendor/customer rows into a target `CompanyID` without swapping `Session["CompanyID"]`. Do not reopen unless a new defect is named.
- Self-service lists often also filter `CreatedByCode = Session["USERID"]`.
- Same `User_Id` string and numeric `Id` are different keys; do not mix in joins.
- Card-kiosk `tbl_Company` is not `CompanyContext`.

---

## Configuration approach

| Store | Use |
|-------|-----|
| `Web.config` `<connectionStrings>` `DbConn` | ERP database |
| `Web.config` `<appSettings>` | SMTP, MSG91, iTop, token TTLs, `UrlTokenAesKey`, `SwitchUser` |
| `Web.Release.config` | Publish transforms (production `SwitchUser` remains false) |
| `AppSecrets` | Read config; empty `UrlTokenAesKey` keeps legacy QuickAction decrypt |

Do not commit new secrets. Do not rotate `machineKey` or live `DbConn`/SMTP/iTop/Msg91 in a feature change. `machineKey` rotation is a full-farm IIS recycle.

---

## Scheduler / service architecture summary

**Baseline:** this solution is an IIS Web Forms app plus SQL Server. No Windows Service project exists here.

In-process / request-tied behavior that must not be replaced with a new host:

| Mechanism | Role |
|-----------|------|
| IIS / `Page_Load` / postback / `[WebMethod]` | All ERP and kiosk UI work |
| `Heartbeat.ashx` | Session idle; `ActiveSessions` heartbeat |
| Page-invoked SPs (e.g. `sp_RunAttendanceRulesEngine`) | Recalc when the page calls them |
| `sp_getapplock` | Quote / client-PO save serialization |
| SQL Server | Persistence, procedures, constraints |

Do not add a background service, in-process timer farm, or second scheduler stack unless a documented architecture change says so. “Scheduler” in security-phase notes means leftover ERP pages with concatenated SQL, not a Windows Service.

---

## Repository conventions

- Trunk: `master`. Feature branches from `master`. Active integration branch: `July_to_Sept26_DevNSupport`. Do not rewrite `v2.1-security-foundation` history.
- PRs: Security, Data, Regression, Scope gates. Merge commit unless a later program says otherwise.
- Page docs: `RECURSIVE_INSTRUCTIONS.md`; unique facts only in `DOMAIN_*.md`. Do not create one markdown file per page.
- Cursor: Ponytail (`.cursor/rules/ponytail.md`) + this file + [`CURSOR_RULES.md`](CURSOR_RULES.md). Working status: [`HANDOFF.md`](HANDOFF.md).
- SQL scripts: header NAME / WHEN / WHY / WHAT (see `CURSOR_RULES.md`).
- Test against `flamex_uat` (or local), never `flamex_live` from development.
- Target VS 2015 / .NET 4.5.2. Native Web Forms controls over new frameworks.
