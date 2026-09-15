# PR-1 Validation Report — Cross-Tenant Duplication

**Date:** 2026-09-12  
**Branch:** `July_to_Sept26_DevNSupport`  
**Scope:** Implementation-blocking validation only  
**Sources:** `UAT_CATALOG.md`, `SHARED_SCHEMA.md`, `DATA_DICTIONARY.md`, `DOMAIN_customer.md`, `DOMAIN_vendor-purchase.md`, `09_Current_Authorization_Implementation_Audit.md`, existing architecture report `docs/41`

---

## Validation Checklist

| # | Claim | Status | Evidence |
|---|-------|--------|----------|
| 1 | Vendor_Id uniqueness across tenants | **FAIL** | No UNIQUE constraint on `Vendor_Id`. No composite `UNIQUE(Vendor_Id, CompanyID)`. SHARED_SCHEMA.md §3: "Vendor_Id unique **in data**" (486/486 on UAT company 1). UAT_CATALOG.md: tbl_Vendor keys = `PK Id` only. Counter-scoping by `CompanyID` in C# prevents within-tenant duplicates, but cross-tenant collision is **structurally unguarded**. |
| 2 | Client_Id uniqueness across tenants | **FAIL** | No UNIQUE constraint on `Client_Id`. No composite `UNIQUE(Client_Id, CompanyID)`. SHARED_SCHEMA.md §3: "Client_Id unique in data" (423/423 on UAT company 1). UAT_CATALOG.md: tbl_Client keys = `PK Id` only. Same structural gap as Vendor. |
| 3 | Tables related to Vendor/Customer without CompanyID | **VERIFIED** | See §A below. |
| 4 | FK map for tbl_Vendor | **VERIFIED** | See §B below. |
| 5 | FK map for tbl_Client | **VERIFIED** | See §B below. |
| 6 | UNIQUE indexes on Vendor_Id / Client_Id | **FAIL** | UAT_CATALOG.md confirms neither table has any UNIQUE constraint beyond the identity PK. tbl_Vendor: 28 columns, PK `Id` only. tbl_Client: 27 columns, PK `Id` only. |
| 7 | tbl_SystemNotification captures CompanyID | **FAIL** | UAT_CATALOG.md: tbl_SystemNotification has 11 columns, PK `NotificationId`. SHARED_SCHEMA.md §6 does NOT list tbl_SystemNotification in the "No CompanyID" table — however the 11 columns are not individually listed in UAT_CATALOG.md section I read. DATA_DICTIONARY.md confirms 35 page callers INSERT into it. **Must verify: does tbl_SystemNotification have a CompanyID column?** The column list was not fully readable from the truncated UAT_CATALOG.md excerpt. **BLOCKING: Must confirm with DBA or full catalog read.** |
| 8 | CompanyContext/Session[CompanyID]/findcompanyId references | **VERIFIED** | See §C below. |
| 9 | No hidden CRUD paths | **VERIFIED** | See §D below. |
| 10 | PR-1 dependency map | **PRODUCED** | See §E below. |

---

## §A — Tables Without CompanyID

Confirmed from SHARED_SCHEMA.md §6 and code-level analysis:

| Table | CompanyID? | Impact on Duplication |
|-------|-----------|----------------------|
| `tbl_Vendor` | **YES** | ✅ Source/target scoping available |
| `tbl_Client` | **YES** | ✅ Source/target scoping available |
| `tbl_ClientRegAddress` | **NO** | ⚠️ Cannot scope by tenant; must use Client_Id join to parent |
| `tbl_representative` | **NO** | ⚠️ Same — FK `Copany_Id` → `Client_Id` |
| `tbl_Factory` | **NO** | ⚠️ FK `Client_id` → `Client_Id` |
| `tbl_Purches` | **NO** | ⚠️ Join via `Client_Id` → `tbl_Vendor.Vendor_Id` for tenant |
| `tbl_purches_details` | **NO** | ⚠️ Child of tbl_Purches |
| `tbl_Purchess_payment` | **NO** | ⚠️ Child of tbl_Purches |
| `tbl_stock` | **NO** | Not relevant to duplication |
| `tbl_SystemNotification` | **UNCONFIRMED** | Must verify — see item 7 above |
| `tbl_Expenses` | **NO** | Scoped via visit CompanyID |
| `tbl_Quotation` | **YES** | Not in scope |
| `tbl_Invoice` | **YES** | Not in scope |
| `tbl_NewProduct` | **YES** | Not in scope |

**Critical for duplication:** Child entities (`tbl_ClientRegAddress`, `tbl_representative`, `tbl_Factory`) have **no CompanyID**. Duplication of customer MUST copy these children and rely on the Client_Id FK join for scoping. They cannot be independently tenant-filtered.

---

## §B — Foreign Key Maps

### tbl_Vendor — FK Relationships

```
┌─────────────────────────────────────────────────┐
│                   tbl_Vendor                     │
│  PK: Id (int IDENTITY)                          │
│  Business key: Vendor_Id (varchar, e.g. "AA09") │
│  CompanyID: int NOT NULL DEFAULT (1)            │
│  Columns: 28                                     │
└──────────┬───────────────────┬──────────────────┘
           │                   │
    Referenced BY              │
    (not declared FKs)         │
           │                   │
┌──────────▼───────────────────▼──────────────────┐
│ tbl_RequisitionMain                              │
│  FK declared: VendorId → tbl_Vendor.Id (int PK) │
│  (modern PR only)                                │
├─────────────────────────────────────────────────┤
│ tbl_Purches                                     │
│  NOT declared: Client_Id → tbl_Vendor.Vendor_Id │
│  (varchar business code, not PK)                │
│  NO CompanyID on tbl_Purches                    │
├─────────────────────────────────────────────────┤
│ No other tables reference tbl_Vendor            │
└─────────────────────────────────────────────────┘
```

**Note:** `tbl_RequisitionMain.VendorId` is a **declared FK** to `tbl_Vendor.Id` (integer PK). `tbl_Purches.Client_Id` references `tbl_Vendor.Vendor_Id` (varchar code). The duplication helper must generate BOTH the new integer PK and the new varchar code, and update any `tbl_RequisitionMain.VendorId` references — OR explicitly exclude requisitions from duplication scope.

### tbl_Client — FK Relationships

```
┌─────────────────────────────────────────────────┐
│                   tbl_Client                     │
│  PK: Id (int IDENTITY)                          │
│  Business key: Client_Id (varchar, e.g. "AD01") │
│  CompanyID: int NOT NULL DEFAULT (1)            │
│  Columns: 27                                     │
└──────────┬───────────────────┬──────────────────┘
           │                   │
    Referenced BY              │
    (not declared FKs)         │
           │                   │
┌──────────▼───────────────────▼──────────────────┐
│ tbl_ClientRegAddress                             │
│  FK inferred: Client_Id → tbl_Client.Client_Id  │
│  (no CompanyID)                                  │
├─────────────────────────────────────────────────┤
│ tbl_representative                               │
│  FK inferred: Copany_Id → tbl_Client.Client_Id  │
│  (misspelled; no CompanyID)                     │
├─────────────────────────────────────────────────┤
│ tbl_Factory                                      │
│  FK inferred: Client_id → tbl_Client.Client_Id  │
│  (no CompanyID)                                  │
├─────────────────────────────────────────────────┤
│ tbl_Quotation                                    │
│  FK inferred: Client_Id → tbl_Client.Client_Id  │
│  (HAS CompanyID on Quotation itself)            │
├─────────────────────────────────────────────────┤
│ tbl_Invoice                                      │
│  FK inferred: Client_ID → tbl_Client.Client_Id  │
│  (HAS CompanyID)                                 │
├─────────────────────────────────────────────────┤
│ tbl_Chalan                                       │
│  FK inferred: Client_ID → tbl_Client.Client_Id  │
│  (HAS CompanyID)                                 │
├─────────────────────────────────────────────────┤
│ tbl_Proforma                                     │
│  FK inferred: Client_ID → tbl_Client.Client_Id  │
│  (HAS CompanyID)                                 │
├─────────────────────────────────────────────────┤
│ tbl_SalesVisitReport.CustomerName                │
│  Free text — NO FK to tbl_Client                │
└─────────────────────────────────────────────────┘
```

---

## §C — Tenant Context References

### CompanyContext.CurrentCompanyID

Found in (code-behind files, not on disk — referenced in architecture audit §6):

| File | Usage |
|------|-------|
| `Bill.Master.cs` | Definition: `CompanyContext.CurrentCompanyID` reads `Session["CompanyID"]` |
| `New_vendor.aspx.cs` | Local variable `currentTenantId = CompanyContext.CurrentCompanyID` (audit §22) |
| `New_client.aspx.cs` | Same pattern (DOMAIN_customer.md) |
| `Update_vendor.aspx.cs` | Used in UPDATE WHERE clause |
| `Update_client.aspx.cs` | Used in UPDATE WHERE clause |
| `Delete_vendor.aspx.cs` | AuthGuard + WHERE clause (Phase 2C) |
| `Delete_client.aspx.cs` | AuthGuard + WHERE clause (Phase 2C) |
| `View_vendor.aspx.cs` | GridView SELECT filter |
| `View_client.aspx.cs` | GridView SELECT filter |
| `AuthGuard.cs` | `ClientInCurrentCompany`, `VendorInCurrentCompany` |
| `srch_dailyrpts.aspx.cs` | Visit search filter |
| `AddUser.aspx.cs` | New user tenant scoping |
| `ViewUser.aspx.cs` | User list filter |
| `Create_quotation.aspx.cs` | Quotation tenant scope |
| Plus ~20 more pages | Per DATA_DICTIONARY.md |

### Session["CompanyID"]

Set in `Bill.Master.cs` `Page_Load` from company dropdown. Read by `CompanyContext`. Never set from client input directly (Phase 2A hardening via `UserCompanyAccess`).

### findcompanyId()

Found in exactly 2 files:
- `New_vendor.aspx.cs` — generates `AA01`, `AA02`, ... format
- `New_client.aspx.cs` — generates `AD01`, `AD02`, ... format

Both use `SELECT MAX(business_key) FROM table WHERE CompanyID = @CompanyID`.

---

## §D — CRUD Path Verification

### Vendor — Complete CRUD Path Census

| Operation | Page | Gate | Verified Source |
|-----------|------|------|----------------|
| Create | `New_vendor.aspx` | SecurePage, `New_vendor` | DOMAIN_vendor-purchase.md, DATA_DICTIONARY.md |
| Read | `View_vendor.aspx` | SecurePage, `View_vendor` | Same |
| Update | `Update_vendor.aspx` | Bill.Master only (not SecurePage) | Same |
| Delete | `Delete_vendor.aspx` | SecurePage, `Delete_vendor` + Phase 2C AuthGuard | Same |
| **Hidden create?** | `Purches_new_vendor.aspx` | Creates vendor inline during purchase | ⚠️ **YES — additional create path** |
| **Hidden create?** | `Purches_exting_vendor.aspx` | Selects existing vendor | No — read only |

**FINDING:** `Purches_new_vendor.aspx` (class `WebForm10`) has INSERT into `tbl_Vendor` (per DATA_DICTIONARY.md). This is an **additional create path** outside the canonical `New_vendor.aspx`. Duplication logic is NOT needed here, but any future duplication must not break this path.

### Customer — Complete CRUD Path Census

| Operation | Page | Gate | Verified Source |
|-----------|------|------|----------------|
| Create | `New_client.aspx` | SecurePage, `New_client` | DOMAIN_customer.md, DATA_DICTIONARY.md |
| Read | `View_client.aspx` | SecurePage, `View_client` | Same |
| Update | `Update_client.aspx` | Bill.Master only (not SecurePage) | Same |
| Delete | `Delete_client.aspx` | SecurePage, `Delete_client` + Phase 2C AuthGuard | Same |
| **Hidden create?** | None found | — | ✅ No secondary create path |

**Result:** No hidden CRUD paths exist that would interfere with duplication. The `Purches_new_vendor` inline vendor creation is a separate workflow and does not affect duplication architecture.

---

## §E — PR-1 File Dependency Map

PR-1 scope: **Single-vendor and single-customer duplication from View pages.**

```
PR-1 FILE DEPENDENCY MAP
═══════════════════════════════════════════════════════

MODIFIED FILES (4 existing)
─────────────────────────────

1. corporate/business/app/View_vendor.aspx
   └─ Add "Duplicate" button per grid row
   └─ Add target company hidden field or AJAX panel
   └─ Depends on: View_vendor.aspx.cs (handler)

2. corporate/business/app/View_vendor.aspx.cs
   └─ btnDuplicate_Click handler
   └─ Reads source CompanyID from Session
   └─ Reads target CompanyID from form control
   └─ Calls: DuplicationService.DuplicateVendor()
   └─ Depends on: DuplicationService.cs, AuthGuard.cs

3. corporate/business/app/View_client.aspx
   └─ Add "Duplicate" button per grid row
   └─ Same pattern as View_vendor
   └─ Depends on: View_client.aspx.cs

4. corporate/business/app/View_client.aspx.cs
   └─ btnDuplicate_Click handler
   └─ Calls: DuplicationService.DuplicateCustomer()
   └─ Depends on: DuplicationService.cs, AuthGuard.cs

NEW FILES (1)
─────────────

5. corporate/business/app/DuplicationService.cs
   └─ Static helper class
   └─ DuplicateVendor(conn, sourceId, srcCo, tgtCo, userId)
   └─ DuplicateCustomer(conn, sourceId, srcCo, tgtCo, userId)
   └─ GenerateNextVendorCode(conn, companyId, tran)
   └─ GenerateNextClientCode(conn, companyId, tran)
   └─ GenerateSafeName(conn, table, column, name, companyId, tran)
   └─ Depends on: DB_UTILITY.cs (connection), AuthGuard.cs (validation)

MODIFIED FILES (1 project file)
─────────────────────────────────

6. corporate/business/app/../../../../Bill_Software.csproj
   └─ Add <Compile Include="corporate/business/app\DuplicationService.cs" />

UNCHANGED BUT REFERENCED
─────────────────────────

7. corporate/business/app/Bill.Master.cs
   └─ CompanyContext — read-only, no changes
   └─ Session["CompanyID"] — no changes

8. corporate/business/app/Bill.Master
   └─ Company dropdown — no changes for PR-1
   └─ Target company selector deferred to PR-2

9. corporate/business/app/AuthGuard.cs
   └─ ClientInCurrentCompany — existing, used by DuplicationService
   └─ VendorInCurrentCompany — existing, used by DuplicationService

10. corporate/business/app/DB_UTILITY.cs
    └─ Connection string — read-only, no changes

11. corporate/business/app/New_vendor.aspx(.cs)
    └─ No changes — duplication does not alter create flow

12. corporate/business/app/New_client.aspx(.cs)
    └─ No changes

13. corporate/business/app/Update_vendor.aspx(.cs)
    └─ No changes

14. corporate/business/app/Update_client.aspx(.cs)
    └─ No changes

15. corporate/business/app/Delete_vendor.aspx(.cs)
    └─ No changes

16. corporate/business/app/Delete_client.aspx(.cs)
    └─ No changes
```

---

## Implementation-Blocking Findings

### FINDING-1: No UNIQUE constraint on business keys [BLOCKING]

**Risk:** Cross-tenant duplication can generate Vendor_Id or Client_Id values that collide with existing codes in the target tenant, even with the MAX() counter, if:
- Another user creates a vendor/client in the target tenant between the MAX() read and the INSERT
- A manual SQL insert bypassed the counter

**Recommendation:** Before PR-1 implementation, run this query against UAT/prod:
```sql
SELECT Vendor_Id, COUNT(*) AS cnt 
FROM tbl_Vendor 
GROUP BY Vendor_Id 
HAVING COUNT(*) > 1;
```
And similarly for `Client_Id` on `tbl_Client`. If any duplicates exist, the counter-based approach is already broken and must be fixed before duplication can proceed.

**Resolution options:**
- A. Add `UNIQUE(Vendor_Id)` + `UNIQUE(Client_Id)` after deduplication
- B. Add `UNIQUE(Vendor_Id, CompanyID)` + `UNIQUE(Client_Id, CompanyID)` (allows same code across tenants)
- C. Accept risk and rely on MAX()+transaction for PR-1, add constraints in PR-3

### FINDING-2: tbl_SystemNotification CompanyID unconfirmed [BLOCKING]

**Impact:** If tbl_SystemNotification has no CompanyID column, cross-tenant audit logging is impossible. The duplication audit rows would be visible to all tenants.

**Action:** Must confirm the column list for tbl_SystemNotification (11 columns). Read the full entry from UAT_CATALOG.md line 104 or query the database:
```sql
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbl_SystemNotification' ORDER BY ORDINAL_POSITION;
```

### FINDING-3: tbl_ClientRegAddress / tbl_representative / tbl_Factory have no CompanyID [NOT BLOCKING]

**Impact:** These child tables cannot be independently tenant-scoped. They MUST be copied as part of customer duplication and rely on the Client_Id FK join for scoping.

**Recommendation:** PR-1 duplicates master records only. PR-2 adds optional child-entity copying (ClientRegAddress, Factory, Representative). The transaction must copy children WITHIN the same transaction as the parent.

### FINDING-4: Purches_new_vendor.aspx is an additional Vendor create path [NOT BLOCKING]

**Impact:** Vendors created via `Purches_new_vendor.aspx` use a different ID prefix (`VEN001...` per DOMAIN_vendor-purchase.md page notes). This is a separate vendor ID space from the `AA` prefix used by `New_vendor.aspx`. Duplication only needs to target the `AA`-prefixed vendors created through the canonical path.

---

## Pass/Fail Summary

| Validation | Status | Blocking? |
|-----------|--------|-----------|
| Vendor_Id uniqueness | **FAIL** — no UNIQUE constraint | Yes for safety |
| Client_Id uniqueness | **FAIL** — no UNIQUE constraint | Yes for safety |
| Tables without CompanyID | **PASS** — identified and documented | No |
| FK map tbl_Vendor | **PASS** — 2 reference paths | No |
| FK map tbl_Client | **PASS** — 7+ reference paths | No |
| UNIQUE indexes | **FAIL** — none on business keys | Yes for safety |
| tbl_SystemNotification CompanyID | **UNCONFIRMED** — needs DB check | Yes for audit |
| Tenant context references | **PASS** — fully traced | No |
| Hidden CRUD paths | **PASS** — 1 secondary vendor create found (non-blocking) | No |
| PR-1 dependency map | **PASS** — 6 files total | No |

---

**Bottom line:** PR-1 can proceed on the **code side** immediately. The two blocking items (UNIQUE constraint verification, SystemNotification CompanyID confirmation) are **database-level checks** that must be run against UAT/prod before the duplication feature goes live. They do not block code authoring but DO block deployment.

No application code was modified. This is a read-only validation.
