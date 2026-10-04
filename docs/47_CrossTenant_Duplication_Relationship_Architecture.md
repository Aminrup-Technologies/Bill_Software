# Cross-Tenant Duplication Relationship Architecture

**Date:** 2026-10-02  
**Branch:** `July_to_Sept26_DevNSupport`  
**Scope:** Persistent Vendor / Customer duplication relationships (follows v1.1, [`46_CrossTenant_Release_v1.1.md`](46_CrossTenant_Release_v1.1.md), which is unchanged)

---

## Objective

Cross-Tenant Duplication v1.1 recorded each duplication only as a free-text audit row in `tbl_SystemNotification`. This change adds a structured source → target link so both the source and the target record can show their counterpart.

`tbl_SystemNotification` remains audit / notification only. It is not read for relationships.

---

## Schema

Patch: `Bill_Software/corporate/business/sql/create_cross_tenant_duplication.sql` (idempotent; apply before deploying the application change).

`dbo.tbl_CrossTenantDuplication`

| Column | Type | Notes |
|--------|------|-------|
| `Id` | `INT IDENTITY` | PK |
| `EntityType` | `VARCHAR(20)` | `CHECK IN ('Vendor','Customer')` |
| `SourceCompanyID` | `INT` | FK → `dbo.tbl_Company(ID)` |
| `SourceRecordID` | `INT` | `tbl_Vendor.Id` / `tbl_Client.Id` in the source tenant |
| `SourceBusinessCode` | `VARCHAR(50)` | `Vendor_Id` / `Client_Id` snapshot |
| `TargetCompanyID` | `INT` | FK → `dbo.tbl_Company(ID)` |
| `TargetRecordID` | `INT` | New PK in the target tenant (`SCOPE_IDENTITY()`; `tbl_Vendor.Id` / `tbl_Client.Id` are identity PKs per [`34`](34_CrossTenant_Duplication_Architecture.md) and `page-catalog/UAT_CATALOG.md`) |
| `TargetBusinessCode` | `VARCHAR(50)` | New `AA` / `AD` code snapshot |
| `DuplicatedOn` | `DATETIME` | `GETDATE()` |
| `DuplicatedBy` | `VARCHAR(50)` | Acting `Session["USERID"]` |

Constraints and indexes:

- `UQ_CrossTenantDuplication_Pair` — unique `(EntityType, SourceCompanyID, SourceRecordID, TargetCompanyID, TargetRecordID)`.
- `IX_CrossTenantDuplication_Source` — `(EntityType, SourceCompanyID, SourceRecordID)`.
- `IX_CrossTenantDuplication_Target` — `(EntityType, TargetCompanyID, TargetRecordID)`.

Deliberately absent:

- No FK on `SourceRecordID` / `TargetRecordID` (polymorphic across `tbl_Vendor` / `tbl_Client`).
- No status / lifecycle columns and no cascading deletes. Business codes are snapshots, so a link stays readable if a record is later edited.
- No changes to `tbl_Vendor` / `tbl_Client`.

---

## Write path (`DuplicationService.cs`)

`WriteDuplicationRelationship` inserts one row per successful duplication, on the same `SqlConnection` / `SqlTransaction` as the master, child and audit inserts:

```
validate source (CompanyContext.CurrentCompanyID) + AuthGuard.UserCanAccessCompany(target)   (unchanged)
  ↓
INSERT master; SELECT CAST(SCOPE_IDENTITY() AS INT)  → target PK
  ↓
customers: clone ClientRegAddress / Factory / Representative                                 (unchanged)
  ↓
WriteDuplicationRelationship (Source = current company, Target = selected company)
  ↓
WriteDuplicationAudit                                                                         (unchanged)
  ↓
Commit
```

| Path | Relationship rows |
|------|-------------------|
| `DuplicateVendor` / `DuplicateCustomer` success | 1 |
| `BulkDuplicateVendors` / `BulkDuplicateCustomers` | 1 per `SuccessCount` item |
| Skipped item (invalid id, not in current company, name exists in target) | 0 |
| Any exception | 0 — rolled back with master / child / audit inserts |

No new authorization mechanism is introduced.

---

## Read path (View pages)

`DuplicationService.GetAuthorizedCounterpartLinks(entityType)` runs once per grid bind:

```sql
-- {RecordTable} = tbl_Vendor for 'Vendor', tbl_Client for 'Customer' (C# whitelist; anything else throws)
SELECT d.SourceRecordID, d.TargetCompanyID, d.TargetBusinessCode, 1,
       CASE WHEN x.Id IS NULL THEN 0 ELSE 1 END, d.DuplicatedOn
FROM dbo.tbl_CrossTenantDuplication d
LEFT JOIN dbo.{RecordTable} x ON x.Id = d.TargetRecordID AND x.CompanyID = d.TargetCompanyID
WHERE d.EntityType = @EntityType AND d.SourceCompanyID = @CompanyID
UNION ALL
SELECT d.TargetRecordID, d.SourceCompanyID, d.SourceBusinessCode, 0,
       CASE WHEN x.Id IS NULL THEN 0 ELSE 1 END, d.DuplicatedOn
FROM dbo.tbl_CrossTenantDuplication d
LEFT JOIN dbo.{RecordTable} x ON x.Id = d.SourceRecordID AND x.CompanyID = d.SourceCompanyID
WHERE d.EntityType = @EntityType AND d.TargetCompanyID = @CompanyID
ORDER BY DuplicatedOn;
```

- `@CompanyID` = `CompanyContext.CurrentCompanyID`. Only links touching the current tenant are read.
- The `LEFT JOIN` only checks whether the counterpart record still exists (same `Id` **and** same `CompanyID`). No counterpart columns are read.
- A link is shown only if the counterpart company is in `AuthGuard.GetAuthorizedCompanies()` **and** `AuthGuard.UserCanAccessCompany(counterpart)` is true. Otherwise the counterpart company name and business code are never emitted.
- Results are keyed by the local record Id and matched against grid rows that are already filtered by `CompanyID`.

Display (identity cell, `lblDuplicationLink`, HTML-encoded, no new grid column):

| Viewing | Text |
|---------|------|
| Source record | `Duplicated → {Target company} ({Target code})` |
| Target record | `Duplicated ← {Source company} ({Source code})` |
| Counterpart deleted | Same line plus ` — historical, record no longer exists` |

Codes are always the duplication-time snapshot. Rows are never updated or deleted, so a link to a removed counterpart remains as history and is labeled as such. It is not presented as a live record. Links where the local record itself was deleted don't appear, because only existing grid rows are matched.

Multiple links (one source duplicated to several tenants) are listed one per line.

Pages: `View_vendor.aspx(.cs)` (`EntityType = 'Vendor'`), `View_client.aspx(.cs)` (`EntityType = 'Customer'`).

---

## Verification

| ID | Scenario | Expected |
|----|----------|----------|
| R1 | Single vendor duplicate A → B | 1 row `Vendor`, source/target Ids + codes correct |
| R2 | Single customer duplicate A → B | 1 row `Customer`; children cloned as before |
| R3 | Bulk 5 selected, 2 names exist in B | 3 rows; 2 skipped create none |
| R4 | Forced failure mid-batch | 0 rows; no master / child / audit rows |
| R5 | View in A | Source row shows `Duplicated → B (AAnn)` |
| R6 | View in B | Target row shows `Duplicated ← A (AAnn)` |
| R7 | User without access to counterpart company | No relationship text shown |
| R8 | Re-run SQL patch | No errors, no duplicate objects |
| R9 | Delete the target record in B, view source in A | `Duplicated → B (AAnn) — historical, record no longer exists` |

---

## Files

| File | Change |
|------|--------|
| `corporate/business/sql/create_cross_tenant_duplication.sql` | New relationship table, constraints, indexes |
| `corporate/business/app/DuplicationService.cs` | Capture target PK; `WriteDuplicationRelationship`; `GetAuthorizedCounterpartLinks` |
| `corporate/business/app/View_vendor.aspx(.cs)` | Counterpart line in Vendor Identity cell |
| `corporate/business/app/View_client.aspx(.cs)` | Counterpart line in Client Identity cell |
| `docs/47_CrossTenant_Duplication_Relationship_Architecture.md` | This document |
