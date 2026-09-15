# Cross-Tenant Duplication Architecture — Vendor & Customer

**Date:** 2026-09-12  
**Branch:** `July_to_Sept26_DevNSupport`  
**Status:** Investigation & Architecture (no code modified)

---

## Table of Contents

- [Phase 1 — Vendor CRUD](#phase-1--vendor-crud)
- [Phase 2 — Customer CRUD](#phase-2--customer-crud)
- [Phase 3 — Tenant Architecture](#phase-3--tenant-architecture)
- [Phase 4 — Prefix Generation](#phase-4--prefix-generation)
- [Phase 5 — Duplication Feature Architecture](#phase-5--duplication-feature-architecture)
- [Phase 6 — Repository Impact](#phase-6--repository-impact)

---

# Phase 1 — Vendor CRUD

## 1. UI Controls

**Page:** `Bill_Software/corporate/business/app/New_vendor.aspx` (class `WebForm5`, master `Bill.Master`)

| Control ID | Type | Purpose |
|---|---|---|
| `txtvendorName` | `TextBox` | Vendor/Principle name |
| `txtVendorCode` | `TextBox` | Auto-generated `Vendor_Id` (read-only, e.g. `AA01`, `AA09`) |
| `cmbState` | `DropDownList` | State dropdown (bound from `tbl_State`) |
| `cmbCity` | `DropDownList` | City dropdown (bound from `tbl_City`, dependent on state) |
| `txtAddress` | `TextBox` | Vendor address |
| `txtContactPerson` | `TextBox` | Contact person name |
| `txtPhone` | `TextBox` | Phone number |
| `txtEmail` | `TextBox` | Email address |
| `txtVat_No` | `TextBox` | VAT/Tax registration number |
| `btnSave` | `Button` | Save vendor |
| `lblMsg` | `Label` | Success/error message |

**Gate:** `SecurePage`, permission key `New_vendor`. `CompanyID` set from `Session["CompanyID"]`.

## 2. Save Flow

```
User fills form → Client validation (required fields) → btnSave_Click
  ↓
1. CompanyContext.CurrentCompanyID read from Session["CompanyID"]
2. Duplicate name check:
   SELECT COUNT(*) FROM tbl_Vendor 
   WHERE Vendor_Name = @VendorName AND CompanyID = @CompanyID
3. Generate next Vendor_Id via findcompanyId():
   SELECT MAX(Vendor_Id) FROM tbl_Vendor WHERE CompanyID = @CompanyID
   → Parse numeric suffix → Increment → Prefix "AA" + padded number
4. INSERT INTO tbl_Vendor (Vendor_Id, Vendor_Name, State, City, Address, 
   ContactPerson, Phone, Email, Vat_No, CompanyID)
5. INSERT INTO tbl_SystemNotification (audit row)
6. Response.Redirect to New_vendor.aspx (fresh form)
```

## 3. Edit Flow

**Page:** `Update_vendor.aspx` (class `WebForm12`)  
**QueryString:** `Vendor_Id` (business key, e.g. `AA09`)

```
Page loads → QS["Vendor_Id"] → SELECT vendor from tbl_Vendor WHERE Vendor_Id = @id
  → Populate form fields → User modifies → btnUpdate_Click
  ↓
UPDATE tbl_Vendor SET Vendor_Name=@name, State=@state, City=@city, 
  Address=@address, ... WHERE Vendor_Id = @id AND CompanyID = @companyID
INSERT INTO tbl_SystemNotification (audit)
```

**Known quirk:** `Vat_No` is cleared to empty on save (documented defect in DOMAIN_vendor-purchase.md).

## 4. Delete Flow

**Page:** `Delete_vendor.aspx` (class `WebForm14`)  
**Gate:** `SecurePage`, permission key `Delete_vendor`.

```
Page loads → ComboBox lists all vendors for current CompanyID
  → User selects → btnDelete_Click
  ↓
1. AuthGuard.VendorInCurrentCompany(vendorId, companyID) — Phase 2C hardening
2. DELETE FROM tbl_Vendor WHERE Vendor_Id = @Vendor_Id AND CompanyID = @CompanyID
3. INSERT INTO tbl_SystemNotification (audit)
```

**No cascade** to purchases, purchase payments, or requisitions. `tbl_Purches.Client_Id` pointing to `Vendor_Id` becomes orphaned.

## 5. Grid Binding

**Page:** `View_vendor.aspx` (class `WebForm13`)  
**Gate:** `SecurePage`, permission key `View_vendor`.

```
Page_Load → BindVendors():
  SELECT * FROM tbl_Vendor WHERE CompanyID = @CompanyID 
  ORDER BY Vendor_Name
  → GridView.DataBind()
```

Grid columns: `Vendor_Id`, `Vendor_Name`, `ContactPerson`, `Phone`, `Email`.

## 6. Search/Filter

`View_vendor` uses client-side search (jQuery) filtering the grid. No server-side search method. Autocomplete on vendor names in purchase/quotation pages uses `clientHandlerAdmin.ashx` (no CompanyID filter — documented defect).

## 7. Validation

| Layer | Mechanism |
|---|---|
| Client-side | HTML `required` attributes + jQuery validation |
| Server-side | Duplicate name check (same company); no field-level re-validation in code-behind |

## 8. Stored Procedures

**None.** All Vendor CRUD is inline ADO.NET SQL.

## 9. SQL Tables

| Table | Role |
|---|---|
| `tbl_Vendor` | Master record. PK `Id` (identity int), business key `Vendor_Id` (varchar). Columns include `CompanyID`, `Vendor_Name`, `State`, `City`, `Address`, `ContactPerson`, `Phone`, `Email`, `Vat_No`, `PrincipleVndrCode`. |
| `tbl_SystemNotification` | Audit log (INSERT only, 35 page callers) |
| `tbl_State` | Reference data for state dropdown |
| `tbl_City` | Reference data for city dropdown |

**UAT snapshot:** 486 vendors, all CompanyID=1.

## 10. Vendor ID Generation

Reverse-engineered from `New_vendor.aspx.cs` `findcompanyId()` method:

```csharp
// 1. Get max Vendor_Id for current company
SELECT MAX(Vendor_Id) FROM tbl_Vendor WHERE CompanyID = @CompanyID

// 2. If null, start at "AA01"
// 3. If exists, parse: prefix = "AA", numeric = int.Parse(suffix) + 1
// 4. Format: "AA" + number.ToString().PadLeft(2, '0')
//    Examples: AA01, AA02, ... AA09, AA10, ... AA99, AA100
```

**Pattern:** Two-letter prefix `AA` + zero-padded integer.

## 11. Vendor Prefix Generation

The prefix is **hardcoded** as `AA` in the `findcompanyId()` method. It is **not** tenant-specific — all companies share the `AA` prefix. The counter (`MAX(Vendor_Id)`) is scoped to `CompanyID` in the query, so each tenant maintains its own numeric sequence.

---

## Vendor CRUD Sequence Diagram

```
┌──────┐     ┌──────────────┐     ┌──────────────┐     ┌───────────┐
│ User │     │ New_vendor   │     │ Bill.Master  │     │ SQL Server│
│      │     │   .aspx.cs   │     │   .cs        │     │           │
└──┬───┘     └──────┬───────┘     └──────┬───────┘     └─────┬─────┘
   │                │                    │                    │
   │ GET New_vendor │                    │                    │
   │───────────────>│                    │                    │
   │                │ AuthGuard.EnsurePage("New_vendor")     │
   │                │───────────────────>│                    │
   │                │                    │ SELECT session     │
   │                │                    │───────────────────>│
   │                │                    │<───────────────────│
   │                │ CompanyContext.CurrentCompanyID         │
   │                │<───────────────────│                    │
   │                │                    │                    │
   │                │ SELECT tbl_State   │                    │
   │                │──────────────────────────────────────>│
   │                │<──────────────────────────────────────│
   │                │ SELECT tbl_City (state-dependent)      │
   │                │──────────────────────────────────────>│
   │                │<──────────────────────────────────────│
   │                │                    │                    │
   │<─ Page renders │                    │                    │
   │                │                    │                    │
   │ Fill form +    │                    │                    │
   │ click Save     │                    │                    │
   │───────────────>│                    │                    │
   │                │ SELECT COUNT(*) duplicate name          │
   │                │──────────────────────────────────────>│
   │                │<──────────────────────────────────────│
   │                │                    │                    │
   │                │ SELECT MAX(Vendor_Id) for CompanyID    │
   │                │──────────────────────────────────────>│
   │                │<──────────────────────────────────────│
   │                │                    │                    │
   │                │ findcompanyId() → "AA09"               │
   │                │                    │                    │
   │                │ INSERT INTO tbl_Vendor                  │
   │                │──────────────────────────────────────>│
   │                │<──────────────────────────────────────│
   │                │                    │                    │
   │                │ INSERT INTO tbl_SystemNotification      │
   │                │──────────────────────────────────────>│
   │                │<──────────────────────────────────────│
   │                │                    │                    │
   │<─ Redirect     │                    │                    │
   │                │                    │                    │
```

---

# Phase 2 — Customer CRUD

## 1. UI Controls

**Page:** `Bill_Software/corporate/business/app/New_client.aspx` (class `WebForm15`, master `Bill.Master`)

| Control ID | Type | Purpose |
|---|---|---|
| `txtvendorName` | `TextBox` | Client name (**note:** control ID still says `vendor`) |
| `txtVendorCode` | `TextBox` | Auto-generated `Client_Id` (read-only, e.g. `AD01`) |
| `cmbState` | `DropDownList` | State dropdown |
| `cmbCity` | `DropDownList` | City dropdown |
| `txtAddress` | `TextBox` | Address |
| `txtContactPerson` | `TextBox` | Contact person |
| `txtPhone` | `TextBox` | Phone |
| `txtEmail` | `TextBox` | Email |
| `cmbIndustry` | `DropDownList` | Industry (**commented out / unused** in code) |
| `btnSave` | `Button` | Save client |
| `lblMsg` | `Label` | Message |

**Gate:** `SecurePage`, permission key `New_client`. `CompanyID` set from `Session["CompanyID"]`.

**WebMethod:** `AddNewCityInline` — allows inline city creation without navigating away.

## 2. Save Flow

```
User fills form → btnSave_Click
  ↓
1. CompanyContext.CurrentCompanyID from Session["CompanyID"]
2. Duplicate name check:
   SELECT COUNT(*) FROM tbl_Client 
   WHERE Client_Name = @ClientName AND CompanyID = @CompanyID
3. Generate next Client_Id via findcompanyId():
   SELECT MAX(Client_Id) FROM tbl_Client WHERE CompanyID = @CompanyID
   → Parse: prefix "AD" + padded integer → "AD01", "AD02", ...
4. INSERT INTO tbl_Client (Client_Id, Client_Name, State, City, Address,
   ContactPerson, Phone, Email, CompanyID)
5. INSERT INTO tbl_SystemNotification
6. Response.Redirect to New_client.aspx
```

## 3. Edit Flow

**Page:** `Update_client.aspx` (class `WebForm17`)  
**QueryString:** `Client_Id`

```
Page_Load → Load client + registered address
  → User edits → btnUpdate_Click
  ↓
UPDATE tbl_Client WHERE Client_Id = @id
UPDATE tbl_ClientRegAddress WHERE Client_Id = @id (UPDATE only, no INSERT)
INSERT INTO tbl_SystemNotification
```

**Not a SecurePage** — relies on `Bill.Master` session validation only.

## 4. Delete Flow

**Page:** `Delete_client.aspx` (class `WebForm18`)  
**Gate:** `SecurePage`, permission key `Delete_client`.

```
1. AuthGuard.ClientInCurrentCompany(clientId, companyID)
2. DELETE FROM tbl_Client WHERE Client_Id = @id AND CompanyID = @companyID
3. DELETE FROM tbl_ClientRegAddress WHERE Client_Id = @id
4. INSERT INTO tbl_SystemNotification
```

**No cascade** to factories, representatives, quotations, invoices, or DPCC challans.

## 5. Grid Binding

**Page:** `View_client.aspx` (class `WebForm16`)

```
Page_Load → BindClients():
  SELECT * FROM tbl_Client WHERE CompanyID = @CompanyID
  → GridView with Client_Id, Client_Name, ContactPerson, Phone, Email
```

**CSV Export:** `View_client` can export Master / Reps / Factories data.

**Autocomplete:** `GetClientNames` WebMethod for typeahead on quotation/invoice pages.

## 6. Search/Filter

Client-side jQuery filtering on the grid. `clientHandlerAdmin.ashx` provides typeahead but **has no CompanyID filter** (cross-tenant leak).

## 7. Validation

Same pattern as Vendor: client-side required fields, server-side duplicate name check only.

## 8. Stored Procedures

**None.** All inline ADO.NET SQL.

## 9. SQL Tables

| Table | Role |
|---|---|
| `tbl_Client` | Master record. PK `Id` (identity), business key `Client_Id` (varchar). Includes `CompanyID`, `Client_Name`, state/city/address/contact/email. |
| `tbl_ClientRegAddress` | Registered address (optional, 1:1). Deleted with client. |
| `tbl_SystemNotification` | Audit |
| `tbl_State`, `tbl_City` | Reference data |
| `tbl_Factory` | Factory sites under a client (FK `Client_id`) |
| `tbl_representative` | SPOC persons (FK misspelled as `Copany_Id` = `Client_Id`) |

**UAT snapshot:** 423 clients, all CompanyID=1.

## 10. Client ID Generation

Same pattern as Vendor:

```csharp
// SELECT MAX(Client_Id) FROM tbl_Client WHERE CompanyID = @CompanyID
// prefix = "AD", numeric = parsed + 1
// Format: "AD" + number.ToString().PadLeft(2, '0')
// Examples: AD01, AD02, AD423
```

## 11. Client Prefix

Hardcoded `AD` prefix. Counter scoped by `CompanyID`. Not tenant-configurable.

---

## Vendor vs Customer — Similarities & Differences

| Aspect | Vendor (`tbl_Vendor`) | Customer (`tbl_Client`) |
|---|---|---|
| Business key | `Vendor_Id` (`AA` prefix) | `Client_Id` (`AD` prefix) |
| Integer PK | `Id` (identity) | `Id` (identity) |
| Tenant column | `CompanyID` | `CompanyID` |
| Create page | `New_vendor.aspx` (`WebForm5`) | `New_client.aspx` (`WebForm15`) |
| Edit page | `Update_vendor.aspx` (`WebForm12`) | `Update_client.aspx` (`WebForm17`) |
| Delete page | `Delete_vendor.aspx` (`WebForm14`) | `Delete_client.aspx` (`WebForm18`) |
| View page | `View_vendor.aspx` (`WebForm13`) | `View_client.aspx` (`WebForm16`) |
| SecurePage gate | `New_vendor` / `View_vendor` / `Delete_vendor` | `New_client` / `View_client` / `Delete_client` |
| Edit SecurePage | **No** — relies on Bill.Master only | **No** — relies on Bill.Master only |
| State/City dropdowns | Yes | Yes |
| Industry field | No | Yes (commented out) |
| Inline city creation | No | Yes (`AddNewCityInline` WebMethod) |
| Registered address | No | Yes (`tbl_ClientRegAddress`) |
| Factory sites | No | Yes (`tbl_Factory`) |
| SPOC/Representatives | No | Yes (`tbl_representative`) |
| Purchase linkage | `tbl_Purches.Client_Id` → `Vendor_Id` | Quotations/Invoices → `Client_Id` |
| PR linkage | `tbl_RequisitionMain.VendorId` → `tbl_Vendor.Id` (int PK) | None |
| Duplicate name check | Per company | Per company |
| Concurrency protection | **None** | **None** |
| State/city filtering | City depends on state | City depends on state |

---

# Phase 3 — Tenant Architecture

## Tenant Context Determination — Every Location

### A. Master Page (Bill.Master / Bill.Master.cs)

1. **Session validation:** `AuthGuard.TryValidateSession` in `Page_Load`
2. **Company dropdown:** `UserCompanyAccess` query populates company selector
3. **CompanyContext:** Nested class `CompanyContext` reads `Session["CompanyID"]`
4. **Session["CompanyID"]** set from company dropdown selection
5. **Menu rendering:** `Permissions` ∩ `UserRoles` ∩ `RolePermissions`

### B. Page-Level Session Variables

| Variable | Source | Purpose |
|---|---|---|
| `Session["CompanyID"]` | Bill.Master company dropdown | Current tenant ID (int) |
| `Session["USERID"]` | Login (index.aspx) | User business key (e.g. `AT01`) |
| `Session["SessionToken"]` | Login → ActiveSessions | Session validation token |
| `Session["RoleId"]` | Login → Roles | Display role (cosmetic) |
| `Session["RoleName"]` | Login → Roles | Display name (cosmetic) |

### C. CompanyContext

```csharp
// Nested class in Bill.Master.cs
public static class CompanyContext
{
    public static int CurrentCompanyID
    {
        get { return Convert.ToInt32(HttpContext.Current.Session["CompanyID"]); }
    }
}
```

### D. Tenant Filters in Queries

Every tenant-scoped table uses `WHERE CompanyID = @CompanyID` with `@CompanyID` from `CompanyContext.CurrentCompanyID` or `Session["CompanyID"]`.

### E. AuthGuard Company Checks

| Method | Table | Logic |
|---|---|---|
| `ClientInCurrentCompany` | `tbl_Client` | `Client_Id` + current `CompanyID` |
| `VendorInCurrentCompany` | `tbl_Vendor` | `Vendor_Id` + current `CompanyID` |
| `RecordInCompany` | Various | Generic document-level check |
| `UserCanAccessCompany` | `UserCompanyAccess` | User membership |
| `EnsurePrint` | Various | Print-level tenant gate |

## Tenant Architecture Diagram

```
┌──────────────────────────────────────────────────────────────────┐
│                        Bill.Master                               │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │ Session["CompanyID"]  ◄── Company dropdown (UserCompanyAccess) │
│  │ Session["USERID"]     ◄── Login (tbl_login.User_Id)      │  │
│  │ Session["SessionToken"] ◄── ActiveSessions                │  │
│  └────────────────────────────────────────────────────────────┘  │
│                                                                  │
│  CompanyContext.CurrentCompanyID ──► Session["CompanyID"]        │
└──────────┬───────────────────────────────────┬───────────────────┘
           │                                   │
           ▼                                   ▼
┌──────────────────┐              ┌──────────────────────┐
│  SecurePage       │              │  Page (non-Secure)   │
│  OnInit:          │              │  Bill.Master only    │
│  AuthGuard.       │              │                      │
│  EnsurePage(key)  │              │  Session["CompanyID"]│
└────────┬─────────┘              └──────────┬───────────┘
         │                                   │
         ▼                                   ▼
┌────────────────────────────────────────────────────────────────┐
│                     ADO.NET Queries                             │
│                                                                 │
│  SELECT/INSERT/UPDATE/DELETE                                    │
│  WHERE CompanyID = @CompanyID                                   │
│  @CompanyID = CompanyContext.CurrentCompanyID                    │
│                                                                 │
│  ┌─────────────┐  ┌──────────────┐  ┌─────────────────────┐   │
│  │ tbl_Vendor   │  │ tbl_Client   │  │ tbl_SystemNotification│  │
│  │ CompanyID ✓  │  │ CompanyID ✓  │  │ (audit, no tenant)   │  │
│  └─────────────┘  └──────────────┘  └─────────────────────┘   │
│  ┌─────────────┐  ┌──────────────┐  ┌─────────────────────┐   │
│  │ tbl_Quotation│  │ tbl_Invoice  │  │ tbl_Purches          │  │
│  │ CompanyID ✓  │  │ CompanyID ✓  │  │ NO CompanyID ⚠       │  │
│  └─────────────┘  └──────────────┘  └─────────────────────┘   │
└────────────────────────────────────────────────────────────────┘
```

## Key Tenancy Findings

| Finding | Detail |
|---|---|
| IDs globally unique? | **Yes** — `Vendor_Id` (`AA01`+), `Client_Id` (`AD01`+) are business keys with no explicit global uniqueness constraint, but generated by `MAX()` per company. Cross-tenant collision is possible if two tenants independently generate the same code. |
| Prefixes tenant-specific? | **No** — prefixes `AA` and `AD` are **hardcoded** in the `findcompanyId()` method. All tenants share the same prefix. |
| Numeric counters tenant-specific? | **Yes** — `SELECT MAX(Vendor_Id/Client_Id) WHERE CompanyID = @CompanyID` scopes the counter to the current tenant. |
| Concurrency protection? | **None** — Two simultaneous creates in the same tenant could generate the same ID (no row lock, no unique constraint, no SP with `sp_getapplock`). |
| `tbl_Company` exists? | Yes — UAT has 2 rows (Flame-Ex `COMP01`/`FE` and AA Associates `COMP02`/`AA`). Column is `Name` (not `Company_Name`). |
| Tables without CompanyID | `tbl_Purches`, `tbl_Expenses`, `tbl_invoice_payment`, `tbl_stock`, GL tables. Isolation for purchases is via vendor join. |

---

# Phase 4 — Prefix Generation

## Vendor Prefix

| Attribute | Value |
|---|---|
| **Source table** | `tbl_Vendor` |
| **Counter storage** | Derived from `MAX(Vendor_Id)` — **no dedicated counter table** |
| **Increment logic** | Parse numeric suffix of `MAX(Vendor_Id)`, add 1, reformat |
| **Prefix** | Hardcoded `AA` |
| **Concurrency protection** | **None** — read-then-write race window |
| **Tenant scoping** | `WHERE CompanyID = @CompanyID` on the MAX query |
| **Format** | `"AA" + number.ToString().PadLeft(2, '0')` |

**Example progression (CompanyID=1):**
```
Start:  AA01
Next:   AA02, AA03, ... AA09, AA10, ... AA99, AA100
UAT max: AA09 (486 vendors, codes go up to ~AA486+)
```

## Customer Prefix

| Attribute | Value |
|---|---|
| **Source table** | `tbl_Client` |
| **Counter storage** | Derived from `MAX(Client_Id)` |
| **Increment logic** | Same as Vendor |
| **Prefix** | Hardcoded `AD` |
| **Concurrency protection** | **None** |
| **Tenant scoping** | `WHERE CompanyID = @CompanyID` |
| **Format** | `"AD" + number.ToString().PadLeft(2, '0')` |

**Example:** `AD01`, `AD02`, ... `AD423` (UAT max).

## Critical Gaps for Cross-Tenant Duplication

1. **No tenant-configurable prefix** — the prefix is baked into C# code. Each new tenant would get `AA`/`AD` prefixes, risking global collisions if multiple tenants share the same database.
2. **No counter table** — counters are derived from `MAX()` at runtime. No explicit sequence, no `SEQUENCE` object, no table row to atomically increment.
3. **No concurrency protection** — no `sp_getapplock`, no `ROWLOCK`, no unique constraint on the business key.

---

# Phase 5 — Duplication Feature Architecture

## Requirements Recap

- **Single duplicate:** Copy one vendor/customer from source tenant to target tenant.
- **Bulk duplicate:** Copy multiple vendors/customers at once.
- **Target tenant** selected from Master Page company dropdown.
- **Rules:** No overwrite, new codes in target's prefix scheme, preserve business data, exclude identity fields, handle duplicate names, support transaction rollback.

## Recommended Service Layer

### New File: `DuplicationService.cs`

```csharp
// Location: Bill_Software/corporate/business/app/DuplicationService.cs
// Following existing pattern: AuthGuard.cs, DB_UTILITY.cs, CommunicationGateway.cs
// Static helper class (no instance state — matches Ponytail conventions)

public static class DuplicationService
{
    // ---- VENDOR DUPLICATION ----
    
    /// <summary>
    /// Duplicates a vendor from source tenant to target tenant.
    /// Returns the new Vendor_Id on success, null on failure/skip.
    /// </summary>
    public static string DuplicateVendor(
        SqlConnection conn,
        string sourceVendorId,    // e.g. "AA09" from source company
        int sourceCompanyId,       // source tenant
        int targetCompanyId,       // target tenant
        string userId)            // Session["USERID"] for audit
    {
        // 1. Read source vendor (scoped to source company)
        // 2. Check duplicate name in target (by Vendor_Name + target CompanyID)
        //    If exists → skip, return null (do NOT overwrite)
        // 3. Generate new Vendor_Id for target using target company's MAX(Vendor_Id)
        // 4. INSERT into tbl_Vendor with new code, target CompanyID
        // 5. INSERT tbl_SystemNotification audit
        // 6. Return new Vendor_Id
    }
    
    /// <summary>
    /// Bulk duplicate: loops DuplicateVendor inside a single transaction.
    /// Returns summary: (duplicated count, skipped count, errors).
    /// </summary>
    public static BulkResult DuplicateVendorsBulk(
        string[] sourceVendorIds,
        int sourceCompanyId,
        int targetCompanyId,
        string userId)
    {
        // Using (var conn = new SqlConnection(...))
        // using (var tran = conn.BeginTransaction())
        //   For each vendor: DuplicateVendor inside transaction
        //   On any critical error: tran.Rollback()
        //   On success: tran.Commit()
        // Return result summary
    }
    
    // ---- CUSTOMER DUPLICATION ----
    
    public static string DuplicateCustomer(
        SqlConnection conn,
        string sourceClientId,
        int sourceCompanyId,
        int targetCompanyId,
        string userId)
    {
        // Same pattern as vendor
        // 1. Read tbl_Client + tbl_ClientRegAddress (if exists)
        // 2. Check duplicate name in target
        // 3. Generate new Client_Id for target
        // 4. INSERT tbl_Client with new code
        // 5. INSERT tbl_ClientRegAddress if present
        // 6. INSERT audit
        // NOTE: Does NOT copy tbl_Factory or tbl_representative
        //       (those are child entities — Phase 2 extension)
    }
    
    public static BulkResult DuplicateCustomersBulk(
        string[] sourceClientIds,
        int sourceCompanyId,
        int targetCompanyId,
        string userId)
    {
        // Transactional bulk, same pattern
    }
    
    // ---- SHARED HELPERS ----
    
    /// <summary>
    /// Generates next business key for target tenant.
    /// Reuses existing findcompanyId() pattern but parameterized.
    /// </summary>
    private static string GenerateNextVendorCode(
        SqlConnection conn, int companyId, SqlTransaction tran)
    {
        // SELECT MAX(Vendor_Id) FROM tbl_Vendor 
        //   WHERE CompanyID = @CompanyID
        //   WITH (UPDLOCK, HOLDLOCK)  -- concurrency protection
        // Parse, increment, format
    }
    
    private static string GenerateNextClientCode(
        SqlConnection conn, int companyId, SqlTransaction tran)
    {
        // Same pattern for Client_Id with "AD" prefix
    }
    
    /// <summary>
    /// Generates a suffixed name to handle duplicates safely.
    /// "Acme Corp" → "Acme Corp (Copy)" or "Acme Corp (Copy 2)"
    /// </summary>
    private static string GenerateSafeName(
        SqlConnection conn, string tableName, string nameColumn,
        string nameValue, int companyId, SqlTransaction tran)
    {
        // Check if name exists in target tenant
        // If yes: append "(Copy)", "(Copy 2)", etc. until unique
    }
}

public class BulkResult
{
    public int Duplicated { get; set; }
    public int Skipped { get; set; }  // duplicate name
    public List<string> Errors { get; set; }
}
```

### Key Design Decisions

| Decision | Choice | Rationale |
|---|---|---|
| **No overwrite** | `DuplicateVendor/Customer` checks target name existence before INSERT | Preserves existing data |
| **New codes** | `GenerateNextVendorCode/ClientCode` with `UPDLOCK, HOLDLOCK` | Prevents race conditions; uses target tenant's own sequence |
| **Transaction rollback** | `SqlTransaction` wrapping bulk operations | If any critical failure, entire batch rolls back |
| **Audit** | `tbl_SystemNotification` INSERT for each duplicate | Maintains Ponytail audit standard |
| **Suffix naming** | "(Copy)" / "(Copy N)" appended | Safe, human-readable collision resolution |
| **Concurrency** | `WITH (UPDLOCK, HOLDLOCK)` on MAX query | Prevents two concurrent duplicates from generating the same code |

## UI Changes

### Option A: Duplicate Button on View Pages (Recommended)

**Files to modify:**

1. **`View_vendor.aspx`** — Add "📋 Duplicate to..." button in grid row
2. **`View_client.aspx`** — Same pattern
3. **`Bill.Master`** — Target tenant dropdown already exists (company selector)

**Flow:**
```
View_vendor.aspx → Select vendor → Click "Duplicate"
  → JavaScript confirm dialog: "Duplicate [vendor name] to [target company]?"
  → Postback to code-behind
  → DuplicationService.DuplicateVendor(sourceId, sourceCompany, targetCompany, userId)
  → Success: "Vendor duplicated as AA12 in [target company]"
  → Skip: "Vendor 'Acme Corp' already exists in [target company]"
  → Error: "Duplicate failed: [reason]"
```

### Option B: Dedicated Duplication Page

**New files:**
- `DuplicateEntities.aspx` — Full page with source/target selection, grid with checkboxes for bulk selection

**Recommended for bulk operations.** Can coexist with Option A for single duplicates.

### Master Page Changes

The target tenant is already selectable via the company dropdown in `Bill.Master`. For duplication:

1. **Read the current session company** as the **source** tenant
2. **Present a target company dropdown** (filtered to companies the user has access to via `UserCompanyAccess`)
3. **Store the target temporarily** in a hidden field or ViewState (not `Session["CompanyID"]` — that must remain the user's working context)

## Backend Flow — Single Vendor Duplicate

```
┌──────┐     ┌──────────────┐     ┌──────────────┐     ┌───────────┐
│ User │     │ View_vendor  │     │ Duplication  │     │ SQL Server│
│      │     │   .aspx.cs   │     │  Service.cs  │     │           │
└──┬───┘     └──────┬───────┘     └──────┬───────┘     └─────┬─────┘
   │                │                    │                    │
   │ Select vendor  │                    │                    │
   │ Click Duplicate│                    │                    │
   │───────────────>│                    │                    │
   │                │                    │                    │
   │                │ Read source vendor │                    │
   │                │ FROM tbl_Vendor    │                    │
   │                │ WHERE Vendor_Id=@id│                    │
   │                │ AND CompanyID=@src │                    │
   │                │──────────────────────────────────────>│
   │                │<──────────────────────────────────────│
   │                │                    │                    │
   │                │ Check duplicate name in target          │
   │                │ SELECT COUNT(*) FROM tbl_Vendor         │
   │                │ WHERE Vendor_Name=@name                 │
   │                │ AND CompanyID=@target                   │
   │                │──────────────────────────────────────>│
   │                │<──────────────────────────────────────│
   │                │                    │                    │
   │                │ [If name exists]   │                    │
   │                │ Generate safe name │                    │
   │                │ "Acme Corp (Copy)" │                    │
   │                │                    │                    │
   │                │ BEGIN TRAN         │                    │
   │                │──────────────────────────────────────>│
   │                │                    │                    │
   │                │ SELECT MAX(Vendor_Id) FROM tbl_Vendor  │
   │                │ WHERE CompanyID=@target                 │
   │                │ WITH (UPDLOCK, HOLDLOCK)                │
   │                │──────────────────────────────────────>│
   │                │<──────────────────────────────────────│
   │                │                    │                    │
   │                │ Generate new code: "AA05"               │
   │                │                    │                    │
   │                │ INSERT INTO tbl_Vendor                  │
   │                │ (Vendor_Id, Vendor_Name, ..., CompanyID)│
   │                │ VALUES ("AA05", ..., @target)           │
   │                │──────────────────────────────────────>│
   │                │<──────────────────────────────────────│
   │                │                    │                    │
   │                │ INSERT audit       │                    │
   │                │──────────────────────────────────────>│
   │                │<──────────────────────────────────────│
   │                │                    │                    │
   │                │ COMMIT TRAN        │                    │
   │                │──────────────────────────────────────>│
   │                │                    │                    │
   │<─ "Duplicated  │                    │                    │
   │   as AA05"     │                    │                    │
   │                │                    │                    │
```

## SQL Flow — Bulk Duplicate (Pseudocode)

```sql
-- 1. Begin transaction
BEGIN TRAN

-- 2. For each source vendor (C# loop):
-- 2a. Read source
SELECT * FROM tbl_Vendor 
WHERE Vendor_Id = @SourceVendorId AND CompanyID = @SourceCompanyId

-- 2b. Check duplicate
IF EXISTS (
    SELECT 1 FROM tbl_Vendor 
    WHERE Vendor_Name = @SourceName AND CompanyID = @TargetCompanyId
)
BEGIN
    -- Generate safe name
    SET @SafeName = @SourceName + ' (Copy)'
    -- Check again...
END

-- 2c. Generate new code (with lock)
DECLARE @MaxCode VARCHAR(20)
SELECT @MaxCode = MAX(Vendor_Id) FROM tbl_Vendor 
WHERE CompanyID = @TargetCompanyId
WITH (UPDLOCK, HOLDLOCK)

-- Parse and increment (in C#)
SET @NewCode = 'AA' + RIGHT('00' + CAST(@NextNum AS VARCHAR), ...)

-- 2d. Insert
INSERT INTO tbl_Vendor (
    Vendor_Id, Vendor_Name, State, City, Address, 
    ContactPerson, Phone, Email, Vat_No, CompanyID
)
SELECT 
    @NewCode, @SafeName, State, City, Address,
    ContactPerson, Phone, Email, Vat_No, @TargetCompanyId
FROM tbl_Vendor 
WHERE Vendor_Id = @SourceVendorId AND CompanyID = @SourceCompanyId

-- 2e. Audit
INSERT INTO tbl_SystemNotification (
    Message, UserId, CreatedDate, CompanyID
)
VALUES (
    'Vendor ' + @SourceVendorId + ' duplicated as ' + @NewCode,
    @UserId, GETDATE(), @TargetCompanyId
)

-- 3. Commit or Rollback
COMMIT TRAN
-- On error: ROLLBACK TRAN
```

---

# Phase 6 — Repository Impact

## Files Requiring Modification

### UI Layer

| File | Change | Complexity |
|---|---|---|
| `corporate/business/app/View_vendor.aspx` | Add "Duplicate" button per row + target company modal | Medium |
| `corporate/business/app/View_vendor.aspx.cs` | Duplicate click handler calling DuplicationService | Low |
| `corporate/business/app/View_client.aspx` | Add "Duplicate" button per row + target company modal | Medium |
| `corporate/business/app/View_client.aspx.cs` | Duplicate click handler calling DuplicationService | Low |

### Business Logic (New)

| File | Change | Complexity |
|---|---|---|
| `corporate/business/app/DuplicationService.cs` | **New file** — static helper class for all duplication logic | High |
| `corporate/business/app/Bill_Software.csproj` | Add new `.cs` file reference | Trivial |

### DAL / Data Access

| File | Change | Complexity |
|---|---|---|
| `DB_UTILITY.cs` | Possibly add shared connection factory (optional — existing code uses `new SqlConnection` directly) | Low |
| `corporate/business/app/AuthGuard.cs` | Add `VendorInCurrentCompany` / `ClientInCurrentCompany` validation for target tenant (Phase 2C already added these for source) | Low |

### SQL

| Object | Change | Complexity |
|---|---|---|
| **New DDL** | Consider adding `UNIQUE` constraint on `(Vendor_Id, CompanyID)` and `(Client_Id, CompanyID)` to enforce global business-key uniqueness | Medium |
| **Optional:** `sp_DuplicateVendor` / `sp_DuplicateCustomer` | Stored procedure encapsulating the duplication logic (alternative to C# inline) | Medium |
| **Optional:** Counter table | `tbl_IdCounter (TableName, CompanyID, Prefix, NextValue)` — eliminates `MAX()` race window | Medium |

### Master Page

| File | Change | Complexity |
|---|---|---|
| `corporate/business/app/Bill.Master` | Expose `TargetCompanyId` hidden field or property for duplication pages to read (separate from `Session["CompanyID"]`) | Low |
| `corporate/business/app/Bill.Master.cs` | Add `TargetCompanyId` property accessible to content pages | Low |

### Shared Helpers

| File | Change | Complexity |
|---|---|---|
| `corporate/business/app/DuplicationService.cs` | (See Business Logic above) | — |

## Classification Summary

| Category | Files | Count |
|---|---|---|
| **UI** | View_vendor(.aspx/.cs), View_client(.aspx/.cs) | 4 |
| **Business Logic** | DuplicationService.cs (new) | 1 |
| **DAL** | AuthGuard.cs (minor) | 1 |
| **SQL** | Optional DDL / SP | 1-2 |
| **Master Page** | Bill.Master, Bill.Master.cs | 2 |
| **Project** | Bill_Software.csproj | 1 |
| **Total** | | **10-11** |

## Implementation Complexity Estimate

| Task | Estimate | Risk |
|---|---|---|
| `DuplicationService.cs` core | 2-3 days | Low — follows established ADO.NET patterns |
| View_vendor duplication UI | 1 day | Low — button + postback |
| View_client duplication UI | 1 day | Low — same pattern |
| Bill.Master target company exposure | 0.5 day | Low |
| AuthGuard target validation | 0.5 day | Low — existing methods |
| DDL uniqueness constraint | 0.5 day | Medium — must verify existing data for duplicates first |
| Bulk selection UI (checkboxes) | 1 day | Low — GridView template |
| **Total** | **7-8 days** | |

## Risk Assessment

| Risk | Severity | Mitigation |
|---|---|---|
| **Cross-tenant ID collision** | High | Two tenants with same prefix `AA` could generate identical `Vendor_Id`. Mitigation: Add unique constraint `(Vendor_Id, CompanyID)` or use tenant-scoped prefix. |
| **Concurrent duplicate race** | Medium | Two users duplicating simultaneously could get same `MAX()` result. Mitigation: `WITH (UPDLOCK, HOLDLOCK)` on the SELECT. |
| **Orphaned references** | High | Duplicating vendor without purchase history leaves `tbl_Purches.Client_Id` pointing to old vendor. Mitigation: Document that duplication copies master records only, not transactional history. |
| **Missing cascade for child entities** | Medium | `tbl_ClientRegAddress`, `tbl_Factory`, `tbl_representative` are not duplicated. Mitigation: Phase 2 extension or explicit user notification. |
| **Session["CompanyID"] contamination** | High | Must not change `Session["CompanyID"]` during duplication. Target company must be passed as a parameter, never via session. |
| **Ponytail compliance** | Medium | Must use parameterized SQL, `using` blocks, `tbl_SystemNotification` audit, and tenant-scoped queries. No static variables. |
| **Existing data conflicts** | Low | Target tenant may already have vendors/customers with same names. Handled by "(Copy)" suffix logic. |

## Recommended Implementation Order

1. **`DuplicationService.cs`** — Core service with single-vendor and single-customer methods
2. **Unit of `View_vendor.aspx`** — Add single duplicate button + handler
3. **Test** — Verify single vendor duplication works correctly
4. **`View_client.aspx`** — Same pattern for customers
5. **Bulk selection** — Add checkboxes to grids
6. **Transaction wrapping** — Add bulk methods with rollback
7. **DDL** — Add uniqueness constraints after verifying existing data
8. **Documentation** — Update page catalogs and data dictionary

---

**End of Architecture Report**

*No application code was modified. No database objects were modified. This is a read-only architecture investigation.*
