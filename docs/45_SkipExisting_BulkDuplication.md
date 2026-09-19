# PR-4.1 Skip Already Duplicated — Bulk Enhancement

**Date:** 2026-09-19  
**Branch:** `feat/skip-existing-bulk-duplication`  
**Base:** `July_to_Sept26_DevNSupport` @ `bffa253`  
**Status:** Implemented — bulk skip of existing vendor/customer names in the target tenant

---

## Objective

When bulk-duplicating vendors or customers into another tenant, skip records whose **normalized name already exists** in the target `CompanyID`. Duplicate only new names, continue the rest of the batch, and report duplicated / skipped / failed counts.

Single-record duplication (PR-1–PR-3) is unchanged: it still uses `ResolveDuplicateName()` (`(Copy)`, `(Copy 2)`, …).

Audit (`WriteDuplicationAudit`) still runs only for newly inserted rows. Skips write nothing.

---

## Algorithm

Normalization (C# and SQL):

1. `Trim` leading/trailing whitespace.
2. Compare case-insensitively (`ToLowerInvariant` in C#; `LOWER(LTRIM(RTRIM(column)))` in SQL).

Per source row inside `BulkDuplicateVendors` / `BulkDuplicateCustomers` (same open transaction):

```
read source row (CompanyContext.CurrentCompanyID)
  ↓
if invalid id / not in current company / missing row → SkippedCount++ (existing PR-4 guard)
  ↓
if VendorExistsInTarget / CustomerExistsInTarget →
    SkippedCount++
    SkippedItems.Add(source name)
    SkipReason = "Already exists in target company"
    continue   (no Vendor_Id/Client_Id generation, no INSERT, no audit, no child copy)
  ↓
else
    ResolveDuplicateName → GenerateNext*Code → INSERT
    customers: clone children only for this new Client_Id
    WriteDuplicationAudit
    SuccessCount++
```

Genuine exceptions still `Rollback` the whole batch (including any inserts already done). Skip is informational and does not abort.

---

## SQL flow

Existence check (parameterized, target-scoped):

```sql
-- Vendor
SELECT TOP 1 1
FROM tbl_Vendor
WHERE CompanyID = @CompanyID
  AND LOWER(LTRIM(RTRIM(Vendor_Name))) = @Name;

-- Customer
SELECT TOP 1 1
FROM tbl_Client
WHERE CompanyID = @CompanyID
  AND LOWER(LTRIM(RTRIM(Client_Name))) = @Name;
```

`@Name` is the trimmed, lower-cased source name. `@CompanyID` is the **target** tenant.

If the check misses:

- Vendor: `INSERT INTO tbl_Vendor` with a new `AA` code, then `WriteDuplicationAudit`.
- Customer: `INSERT INTO tbl_Client` with a new `AD` code, clone `tbl_ClientRegAddress` / `tbl_Factory` / `tbl_representative` for the new `Client_Id`, then `WriteDuplicationAudit`.

Skipped rows never execute those inserts.

---

## Result model

`BulkDuplicateResult` additions:

| Field | Role |
|-------|------|
| `SkippedCount` | How many rows were skipped (existing name, plus prior invalid-id guards) |
| `SkippedItems` | Source names skipped because they already exist in the target |
| `SkipReason` | `Already exists in target company` when at least one existence skip occurred |

UI (bulk confirm):

- Vendor: `{n} duplicated, {n} skipped, {n} failed.`
- Customer: `{n} duplicated, {n} skipped, {n} failed.`

Examples: `8 duplicated, 2 skipped, 0 failed.` / `5 duplicated, 1 skipped, 0 failed.`

---

## UAT scenarios

| ID | Setup | Expected |
|----|--------|----------|
| V1 | Bulk mix: 8 vendors new in target, 2 whose `Vendor_Name` already exists (trim/case variants) | 8 new `AA` rows; 2 skipped; message `8 duplicated, 2 skipped, 0 failed.`; 8 audit rows |
| V2 | Bulk all-new vendors | All duplicated; `SkippedCount = 0`; audits for each |
| V3 | Bulk all already in target | Zero inserts; `N duplicated, N skipped, 0 failed.`; no audits |
| C1 | Bulk mix: 5 new customers, 1 existing `Client_Name` | 5 new `AD` rows + children; 1 skipped; `5 duplicated, 1 skipped, 0 failed.`; 5 audits |
| C2 | Existing Heatworks Pvt. Ltd. in Company 2 (PR-4 gap) | Skip; do **not** create `(Copy 2)` |
| C3 | New customer with address/factory/representative | Children cloned only for the new `Client_Id` |
| S1 | Single-record Duplicate on an existing name | Still creates `(Copy)` / `(Copy N)` (unchanged) |
| F1 | Forced SQL failure mid-batch after some skips and inserts | Entire transaction rolls back; no leftover new rows; skips remain informational |

Same-tenant protection, `CompanyContext`, and `Session["CompanyID"]` are unchanged.

---

## Expected before / after

| Case | Before (PR-4) | After (PR-4.1) |
|------|----------------|----------------|
| Bulk vendor name already in target | New `AA` row named `(Copy)` / `(Copy N)` + audit | Skip; no new row; no audit |
| Bulk customer name already in target | New `AD` row + children + audit | Skip; no row; no children; no audit |
| Bulk new name | Duplicate | Duplicate (unchanged) |
| Bulk mixed | All inserted or all rolled back on error | Existing skipped, new duplicated, batch continues |
| Single-record existing name | `(Copy)` sequence | Unchanged |
| Genuine failure | Full rollback | Full rollback (skips do not prevent rollback of inserts) |

---

## Files

| File | Change |
|------|--------|
| `DuplicationService.cs` | `SkippedItems` / `SkipReason`; `VendorExistsInTarget` / `CustomerExistsInTarget`; bulk skip-before-code-gen |
| `View_vendor.aspx.cs` | Bulk summary `{n} duplicated, {n} skipped, {n} failed.` |
| `View_client.aspx.cs` | Same summary format |
| `docs/44_CrossTenant_Final_Integration.md` | Section 12 points here |
| `docs/45_SkipExisting_BulkDuplication.md` | This document |

---

*PR-4.1 enhancement on `feat/skip-existing-bulk-duplication`.*
