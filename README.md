# Project_FLMX — AminrupERP / Flame-ex

> Legacy enterprise ASP.NET Web Forms ERP application for field-sales management, attendance tracking, customer/vendor administration, quotations, purchase orders, email/SMS communications, and expense management.

---

## Table of Contents

- [System Overview](#system-overview)
- [Technology Stack](#technology-stack)
- [Repository Structure](#repository-structure)
- [Core Architectural Standards (The "Ponytail" Philosophy)](#core-architectural-standards-the-ponytail-philosophy)
- [Setup and Compilation](#setup-and-compilation)
- [Database Overview](#database-overview)
- [Module Reference](#module-reference)
- [Security Notice](#security-notice)
- [Development Guidelines](#development-guidelines)

---

## System Overview

**Project_FLMX** (also referred to as **AminrupERP** / **Flame-ex** in the build configuration) is a multi-tenant ERP system built on ASP.NET Web Forms. It manages the complete field-sales lifecycle: visit planning and execution with GPS capture, daily visit reporting, manager approval workflows, expense tracking and reimbursement, quotation generation, and real-time manager-salesperson communication.

The application serves two primary user roles:

- **Salesperson** — Plans visits on a calendar, executes them with GPS-tagged confirmation, logs past visits, manages expenses, and communicates with managers via an integrated chat thread.
- **Manager** — Reviews, approves/rejects sales visits and associated expenses across their company's sales team through a search-filtered dashboard.

Additional modules handle attendance (clock-in/clock-out with GPS), employee administration (user provisioning, department/designation management, role-based menu access), customer/vendor records, purchase orders, email/SMS communications, and a homepage dashboard with KPIs.

### Key Business Domains

| Domain | Description |
|--------|-------------|
| **Sales Visit Lifecycle** | Full calendar-based planning → GPS execution → manager review/approval → follow-up generation |
| **Attendance & Field Tracking** | Clock-in/clock-out with geolocation, daily attendance dashboard for admins |
| **Expense Management** | Per-visit expense claims with receipts, manager approval workflow |
| **Quotation Generation** | Customer-quote creation linked to visit records |
| **Purchase Orders** | Procurement workflow management |
| **Customer & Vendor Directory** | Customer and vendor record management |
| **Communication** | SMTP email notifications, SMS integration for visit updates and approvals |
| **Administration** | User provisioning, department/designation management, role-based menu access control |

---

## Technology Stack

| Component | Technology | Version/Details |
|-----------|-----------|-----------------|
| **Runtime** | ASP.NET Web Forms | .NET Framework 4.5.2 |
| **Language** | C# | 5.0 / 6.0 |
| **IDE** | Visual Studio | 2015 |
| **Build System** | MSBuild | 14.0 (`Microsoft.Common.CurrentVersion.targets`) |
| **Database** | Microsoft SQL Server | MS SQL Server (production DB: `flamex_live`) |
| **Data Access** | ADO.NET | `SqlConnection`, `SqlCommand`, `SqlDataAdapter`, `SqlDataReader` |
| **Authentication** | Custom session-based | `tbl_login` + `ActiveSessions` token table + `Session["USERID"]` |
| **Frontend Libraries** | jQuery, jQuery UI | Datepicker, dialogs, Select2 dropdowns |
| **Calendar** | FullCalendar | Visit planner calendar widget |
| **Charts** | Chart.js | Dashboard KPI visualizations |
| **Email** | System.Net.Mail | `SmtpClient` / `MailMessage` (Zoho SMTP for Sales Visit notifications; config-driven for auth flows) |
| **CSS Frameworks** | jQuery UI Themes | `ui-lightness` and `base` themes |
| **Package Manager** | NuGet | `packages.config` |

---

## Repository Structure

```
Bill_Software/
├── index.aspx[.cs]                    # Authentication (login, password reset, OTP, session management)
├── Web.config                         # Application configuration (connection strings, AppSettings)
├── Web.Release.config                 # Publish-time transformations
├── DB_UTILITY.cs                      # Shared database utility / connection helpers
├── MoneyConvDS.cs                     # Currency conversion dataset
├── Bill_Software.csproj               # Project file (Visual Studio 2015)
├── packages.config                    # NuGet package references
│
├── corporate/
│   ├── business/
│   │   ├── app/                       # ◄ PRIMARY APPLICATION MODULE
│   │   │   ├── Bill.Master[.cs]       # Master Page — navigation, session validation, menu rendering
│   │   │   ├── home.aspx[.cs]         # Dashboard — KPIs, visit summaries, revenue
│   │   │   ├── visit_planner.aspx[.cs]# Sales Visit Calendar (FullCalendar)
│   │   │   ├── daily_rpt.aspx[.cs]    # Visit entry (plan future / log past)
│   │   │   ├── vw_dailyrpts.aspx[.cs] # "My Sales Visits" — salesperson's own visit list + mega-modal
│   │   │   ├── srch_dailyrpts.aspx[.cs]# Manager Dashboard — search, review, approve/reject
│   │   │   ├── expense_entry.aspx[.cs]# Expense claim submission (linked to visits)
│   │   │   ├── Create_quotation.aspx[.cs] # Quotation generation from visit records
│   │   │   ├── calender/              # Calendar-related sub-assets
│   │   │   ├── Update/                # Update-related sub-pages
│   │   │   └── WebImages/             # Application images
│   │   ├── print/                     # Print templates / report layouts
│   │   └── WebProperty/               # CSS, JS, and image assets for business module
│   │       ├── css/
│   │       ├── js/
│   │       └── images/
│   └── WebProperty/                   # Shared CSS, JS, and images (root-level)
│       ├── css/
│       ├── js/
│       └── images/
│
├── admin/                             # ◄ ADMINISTRATION MODULE
│   ├── AdminAttendanceDashboard.aspx[.cs] # Company-wide attendance & field-sales rollup
│   ├── AddUser.aspx[.cs]              # User provisioning (new employee accounts)
│   ├── ViewUser.aspx[.cs]             # User management grid (edit roles, view details)
│   ├── Update_Designation.aspx[.cs]   # Role/permission assignment (UserRoles maintenance)
│   └── Update/                        # Admin update sub-pages
│
├── Scripts/                           # JavaScript libraries (jQuery, jQuery UI, MS Ajax)
│   └── WebForms/MSAjax/              # ASP.NET AJAX framework scripts
├── Content/                           # jQuery UI theme CSS and images
│   └── themes/
├── Images/                            # Global image assets
├── Print/                             # Global print layout assets
├── WebProperty/                       # Global CSS, JS, and image assets
├── bin/                               # Compiled assemblies
└── Uploads/                           # User-uploaded files (visit photos, expense receipts)
    ├── InvoiceLogs/
    ├── ProformaLogs/
    └── (dynamic visit/expense attachment directories)
```

---

## Core Architectural Standards (The "Ponytail" Philosophy)

These enterprise rules are **mandatory boundaries** for all development work within this codebase. Violations of any standard must be treated as defects and fixed before code reaches the main branch.

### 1. Multi-Tenant Data Isolation

All data access layers **must** execute strict tenant segregation using parameterized queries mapped to `CompanyContext.CurrentCompanyID`.

```csharp
// ✅ CORRECT — parameterized, tenant-scoped
cmd.CommandText = "SELECT * FROM tbl_SalesVisitReport WHERE CompanyID = @CompanyID";
cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

// ❌ WRONG — unscoped, breaks multi-tenancy
cmd.CommandText = "SELECT * FROM tbl_SalesVisitReport";
```

Every `SELECT`, `INSERT`, `UPDATE`, and `DELETE` that touches tenant-scoped tables **must** include `AND CompanyID = @CompanyID` (or the equivalent ownership predicate mapped to the current user's context). Failure to do so constitutes a cross-tenant data leak and is a **Critical severity** defect.

### 2. Transactional Auditing

Every major CRUD operation **must** include proactive notification logging via `tbl_SystemNotification` **prior to transaction commit**. This ensures that even if the subsequent operation fails, a record of the attempted action exists for audit and recovery purposes.

### 3. State & Memory Management

- **Prohibition:** Public static variables in code-behind files are **strictly prohibited** to prevent IIS cross-session data leaks. Static fields in a WebForms code-behind are shared across all sessions under the same AppDomain; a single user's data will leak to every other user.
- **ViewState:** Minimize ViewState serialization. Avoid storing large objects, user-specific data, or sensitive information in ViewState. Use `ControlState` only for the minimal state required for control functionality.
- **Session:** Store only the minimum required identity information in `Session`. The application uses `Session["USERID"]` (business key), `Session["SessionToken"]` (active-session validation), and `Session["RoleId"]`/`Session["RoleName"]` (cosmetic display). All other data must be fetched fresh from the database on each request.

### 4. Ironclad SQL Security

- **Exclusive** use of parameterized queries for all ADO.NET data access (`SqlConnection`, `SqlCommand`, `SqlParameter`).
- **Absolute prohibition** of string concatenation for constructing SQL queries with any user-supplied or session-derived input.
- All ADO.NET objects (`SqlConnection`, `SqlCommand`, `SqlDataReader`) **must** be wrapped in `using` blocks to guarantee deterministic resource disposal.

```csharp
// ✅ CORRECT — parameterized, using block
using (var conn = new SqlConnection(connectionString))
using (var cmd = new SqlCommand("SELECT * FROM tbl_login WHERE User_Id = @UserId", conn))
{
    cmd.Parameters.AddWithValue("@UserId", Session["USERID"]);
    conn.Open();
    using (var reader = cmd.ExecuteReader())
    {
        // process results
    }
}

// ❌ WRONG — string concatenation (SQL injection risk)
string query = "SELECT * FROM tbl_login WHERE User_Id='" + userId + "'";
```

### 5. Workflow Integrity

- Grids (`GridView`) and `DataList` controls rely strictly on **manual item selection workflows** — users must explicitly select/act on individual items.
- **Automatic bulk-insertion** of items into carts, grids, or data structures is **prohibited**. All data mutations must be initiated by an explicit user action (button click, command event) with server-side validation.

### 6. UI/UX Standards

- The application prioritizes **highly responsive, previously established UI/UX grid layouts** over untested visual proposals.
- Any proposed visual change must be validated against the existing responsive grid system before implementation.
- Changes to established UI patterns require stakeholder review and sign-off before deployment.

---

## Setup and Compilation

### Prerequisites

1. **Visual Studio 2015** (or later, with .NET Framework 4.5.2 targeting pack installed)
2. **MS SQL Server** (SQL Server 2012 or later recommended)
3. **IIS** with ASP.NET 4.5.2 enabled (for local hosting)
4. **NuGet package restore** enabled

### Steps

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   ```

2. **Open the solution** in Visual Studio 2015:
   - Open `Bill_Software/Bill_Software.csproj` (or the `.sln` if present)
   - Allow NuGet package restore to complete

3. **Configure the database connection:**
   - Open `Bill_Software/Web.config`
   - Update the `<connectionStrings>` section with your local/development SQL Server instance details
   - **CRITICAL:** Never commit production connection strings, credentials, or API keys to source control

4. **Set up the database:**
   - Create a new database on your SQL Server instance (e.g., `flamex_dev` or `flamex_uat`)
   - If DDL scripts are available from the DBA team, run them to create the schema
   - If no DDL is available, the schema must be reverse-engineered from application code (see the Sales Visit Workflow Audit documentation in `docs/`)

5. **Configure SMTP (optional, for email notifications):**
   - Add the following keys to `Web.config` `<appSettings>` if not already present:
     ```xml
     <add key="SmtpFrom" value="your-sender@example.com" />
     <add key="SmtpUser" value="your-smtp-username" />
     <add key="SmtpPass" value="your-smtp-password" />
     <add key="SmtpHost" value="smtp.example.com" />
     <add key="SmtpPort" value="587" />
     <add key="SmtpEnableSsl" value="true" />
     ```
   - **Note:** The Sales Visit workflow currently hardcodes SMTP credentials in `vw_dailyrpts.aspx.cs` and `srch_dailyrpts.aspx.cs` instead of using these `AppSettings` keys. This is a known defect (D-11) pending remediation.

6. **Build:**
   ```
   Build → Build Solution (Ctrl+Shift+B)
   ```
   The build output confirms: `Bill_Software -> bin\Bill_Software.dll`

7. **Publish (for deployment):**
   ```
   Build → Publish Web App
   ```
   Configure the publish profile to target your IIS deployment path.

### Known Build Warning

```
warning MSB3277: Found conflicts between different versions of the same dependent assembly
```

This is a known assembly version conflict warning. Review detailed build log output to identify the conflicting assemblies and consider binding redirects in `Web.config`.

---

## Database Overview

> **Source-of-truth caveat:** No `.sql` DDL files, EF migrations, or stored-procedure definitions exist in this repository. All schema information below is **reverse-engineered from ADO.NET `SqlCommand` text** in the application code.

### Core Tables

| Table | Purpose | Key Relationships |
|-------|---------|-------------------|
| `tbl_login` | User directory / identity | `User_Id` (business key, FK target for all ownership columns), `ReportingManagerId` (self-FK for manager hierarchy), `CompanyId` (tenant boundary) |
| `tbl_SalesVisitReport` | Core visit entity | `CreatedByCode` → `tbl_login.User_Id`, `ApprovedBy` → `tbl_login.User_Id`, `ParentVisitId` (self-FK for follow-up chain), `CompanyId` |
| `tbl_SalesVisitResponses` | Manager/salesperson chat thread per visit | `VisitId` → `tbl_SalesVisitReport.Id`, `RespondentCode` → `tbl_login.User_Id` |
| `tbl_Expenses` | Expense claims (may or may not link to a visit) | `VisitId` → `tbl_SalesVisitReport.Id` (nullable), `UserCode`/`ApprovedBy` → `tbl_login.User_Id` |
| `tbl_SystemNotification` | Transactional audit notifications | Inserted prior to major CRUD operations |
| `ActiveSessions` | Active session tracking | `SessionToken`, `UserId`, `IsActive` — re-validated on every Master Page load |
| `UserRoles` | Many-to-many user↔role mapping | Controls navigation menu visibility via `Bill.Master.cs :: GetMenuControl()` |
| `RolePermissions` | Role↔permission mapping | Joined with `UserRoles` to determine menu item visibility |
| `Roles` | Role definitions | `RoleName`, joined via `tbl_login.RoleId` |
| `tbl_Departments` | Department directory | Referenced by `tbl_login.DepartmentID` |
| `tbl_Designations` | Designation directory | Referenced by `tbl_login.DesignationID` |

### Tenancy Model

Every query against tenant-scoped tables must include a `CompanyId` filter. The application resolves the current tenant via `CompanyContext.CurrentCompanyID` (defined in `Bill.Master.cs`). Self-service pages scope by `CreatedByCode` (ownership); manager pages scope by `CompanyId` (tenant-wide visibility).

---

## Module Reference

Detailed module documentation is organized by the navigation sequence found in the Master Page (`~/corporate/business/app/Bill.Master`). Each module document covers:

- Associated frontend (`.aspx`) and backend (`.aspx.cs`) files
- Core database tables involved
- Multi-tenant constraints and proactive notification triggers

See the `docs/` directory for module-specific documentation:

| Doc | Module |
|-----|--------|
| `docs/01_Attendance_Clock.md` | Attendance & Clock-In/Out |
| `docs/02_Employee_Admin.md` | Employee Administration (User Provisioning) |
| `docs/03_Role_Permissions.md` | Role & Permission Management |
| `docs/04_Department_Designation.md` | Department & Designation Management |
| `docs/05_Customer_Vendor.md` | Customer & Vendor Directory |
| `docs/06_Sales_Visit_Planner.md` | Sales Visit Calendar & Planning |
| `docs/07_Sales_Visit_Reporting.md` | Daily Visit Reports & Manager Approval |
| `docs/08_Expense_Management.md` | Expense Claims & Approval |
| `docs/09_Quotation_Generation.md` | Quotation Generation |
| `docs/10_Purchase_Order.md` | Purchase Order Management |
| `docs/11_Communications.md` | Email & SMS Integration |
| `docs/12_Home_Dashboard.md` | Homepage Dashboard & KPIs |

---

## Security Notice

### Credentials and Secrets

- **NEVER** commit production connection strings, passwords, API keys, SMTP credentials, or any other secrets to this repository.
- The `Web.config` file contains connection strings that must be managed outside source control (use `Web.Release.config` transformations or environment-specific configuration).
- **Known issue:** SMTP credentials for the Sales Visit notification workflow are hardcoded in `vw_dailyrpts.aspx.cs` and `srch_dailyrpts.aspx.cs`. These must be rotated immediately and migrated to `ConfigurationManager.AppSettings` as part of remediation (defect D-11).
- The production deployment URL (`https://www.exc.aagroupindia.com`) is referenced in email notification templates — do not alter without coordinating with the deployment team.

### Authentication

- The application uses **custom session-based authentication** (not ASP.NET Forms Authentication).
- Session validity is re-validated on every Master Page load against `dbo.ActiveSessions`.
- Login credentials are verified via PBKDF2 password hashing with a legacy plaintext fallback path (see `index.aspx.cs`).

### Known Security Defects (see `docs/` for full details)

| ID | Severity | Description |
|----|----------|-------------|
| D-04 | **Critical** | Broken access control (IDOR) — any authenticated user can view/modify/approve any visit by supplying its sequential `Id` |
| D-05 | **Critical** | SQL Injection in `srch_dailyrpts.aspx.cs :: Binder()` via string concatenation |
| D-09 | **High** | Unauthenticated static file retrieval — uploaded attachments accessible without login |
| D-11 | **High** | Hardcoded SMTP credentials committed to source control |
| D-10 | **Medium** | Silent notification failures — SMTP errors swallowed with no logging or user feedback |

---

## Development Guidelines

### Before Making Changes

1. Read the **Core Architectural Standards** above — violations are defects.
2. Check if the module you are modifying has documentation in `docs/`.
3. Review the Sales Visit Workflow Audit (`docs/sales-visit-workflow-audit/`) for precedent on how enterprise rules are applied.

### Code Quality Rules

- **Parameterize all SQL.** No exceptions. No string concatenation for queries.
- **Use `using` blocks** for all `SqlConnection`, `SqlCommand`, `SqlDataReader`, and `StreamWriter` objects.
- **No public static fields** in code-behind files.
- **No bulk-insert patterns** in GridView/DataList handlers — manual selection only.
- **Log to `tbl_SystemNotification`** before committing transactional changes.
- **Include `CompanyId`/`CreatedByCode` filters** on every query against tenant-scoped tables.
- **Test against `flamex_uat`** (or your local dev database), never against `flamex_live`.

### Commit Conventions

- Use descriptive commit messages that reference the module and the change type.
- Reference defect IDs where applicable (e.g., "Fix SQL injection in manager search — D-05").
- Do not commit connection strings, credentials, or API keys under any circumstances.

### Pull Request Checklist

- [ ] All SQL is parameterized (no string concatenation)
- [ ] All ADO.NET objects are in `using` blocks
- [ ] No public static fields introduced in code-behind
- [ ] Tenant isolation (`CompanyId`/`CreatedByCode`) maintained on all queries
- [ ] `tbl_SystemNotification` logging included for CRUD operations
- [ ] No secrets, credentials, or connection strings in code or configuration
- [ ] Client-side validation is mirrored server-side (defense-in-depth)
- [ ] Error messages shown to users are generic (no raw exception details)
