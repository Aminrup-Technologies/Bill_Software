# Documentation Consistency Audit: docs/41–#44

**Date:** September 14, 2026
**Documents audited:** `docs/41`, `docs/42`, `docs/43`, `docs/44`
**Verified against:** Actual implementation on disk (`DuplicationService.cs`, `View_vendor.aspx[.cs]`, `View_client.aspx[.cs]`, `Bill_Software.csproj`)

---

## Summary

| Category | Mismatches Found |
|----------|-----------------|
| Method name mismatches | 4 |
| File path errors | 0 |
| Stale sequence diagrams | 1 |
| Outdated acceptance criteria | 0 |
| Signature mismatches | 3 |
| Class/field name mismatches | 2 |
| Proposed-but-not-implemented items | 2 |
| Pre-existing defects (not from docs) | 1 |

**Overall: docs/44 (Completion Report) is consistent with the actual implementation. docs/41 (Architecture) has expected speculative mismatches since it was written before implementation.**

---

## 1. Mismatched Method Names

| # | Doc | Section | Name Used | Actual Name | Severity |
|---|-----|---------|-----------|-------------|----------|
| M-1 | docs/41 | Phase 5, Service Layer | `GenerateSafeName()` | `ResolveDuplicateName()` | **Medium** — Architecture doc proposed `GenerateSafeName` but implementation used `ResolveDuplicateName`. docs/44 correctly uses `ResolveDuplicateName`. |
| M-2 | docs/41 | Phase 5, Service Layer | `DuplicateVendorsBulk()` | `BulkDuplicateVendors()` | **Medium** — Architecture doc used verb-first naming; implementation used noun-first. docs/44 correctly uses `BulkDuplicateVendors`. |
| M-3 | docs/41 | Phase 5, Service Layer | `DuplicateCustomersBulk()` | `BulkDuplicateCustomers()` | **Medium** — Same naming convention difference. docs/44 correctly uses `BulkDuplicateCustomers`. |
| M-4 | docs/41 | Phase 5, UI Changes | `btnDuplicate_Click` (handler name) | `btnConfirmDuplicateVendor_Click` / `btnConfirmDuplicateClient_Click` | **Low** — docs/41 was speculative; actual names match the modal-based workflow. docs/44 correctly uses the actual names. |

**Assessment:** docs/41 contains **pre-implementation proposals** that were naturally refined during implementation. docs/44 (written post-implementation) uses the correct names throughout. **No inconsistency between docs/42, 43, 44.**

---

## 2. File Path Errors

| # | Doc | Path Claimed | Actual Path | Status |
|---|-----|-------------|-------------|--------|
| P-1 | docs/41 | `corporate/business/app/DuplicationService.cs` | `Bill_Software/corporate/business/app/DuplicationService.cs` | **OK** — docs/41 uses relative path from project root, which is convention. |
| P-2 | docs/43 | `Bill_Software/corporate/business/app/DuplicationService.cs` | Same | **OK** |
| P-3 | docs/43 | `Bill_Software/Bill_Software.csproj` | Same | **OK** |
| P-4 | docs/43 | `corporate/business/app/Bill.Master.cs` | Same | **OK** |

**Assessment:** No file path errors found in any document.

---

## 3. Stale Sequence Diagrams

| # | Doc | Diagram | Issue | Severity |
|---|-----|---------|-------|----------|
| D-1 | docs/41 | Phase 5 Backend Flow — Single Vendor Duplicate | Shows user clicking "Duplicate" → immediate service call. Actual implementation uses a **modal workflow**: Duplicate button → open modal → select target → confirm → JS validate → postback → handler → service. The sequence diagram skips the modal layer entirely. | **Low** — Expected since docs/41 was pre-implementation architecture. docs/44 includes the correct modal-based flow diagram. |
| D-2 | docs/41 | Phase 5 SQL Flow — Bulk Duplicate | Shows a single SQL-level pseudocode flow. Actual implementation does everything in C# (no stored procedures). The pseudocode is a design reference, not a literal implementation guide. | **Low** — Design intent vs. implementation difference, acceptable. |

**Assessment:** docs/41 sequence diagrams reflect the **design-time intent**, not the final implementation. This is appropriate for an architecture document. docs/44 contains the **correct post-implementation flow diagrams** that match the actual code.

---

## 4. Outdated Acceptance Criteria

| # | Doc | Criteria | Status in Implementation | Severity |
|---|-----|----------|------------------------|----------|
| AC-1 | docs/41 | "Target tenant selected from Master Page company dropdown" | **Partially outdated** — Target is selected from a per-page modal dropdown (`ddlTargetCompanyGlobal`), NOT the Master Page company switcher. The Master Page company switcher remains the source tenant selector. docs/44 correctly describes the modal workflow. | **Medium** — docs/41's recommendation was refined during PR-1 UI Refinement. |
| AC-2 | docs/41 | "Bill.Master — Target tenant dropdown already exists (company selector)" | **Outdated** — The Master Page dropdown is the SOURCE tenant selector, not the target. A separate target dropdown was added per View page in the modal. | **Medium** — Same refinement as AC-1. |
| AC-3 | docs/41 | "tbl_ClientRegAddress (if exists)" for customer duplication — "NOTE: Does NOT copy tbl_Factory or tbl_representative" | **Outdated** — Implementation DOES copy all three child tables. | **Medium** — This was a scope expansion in PR-3. docs/44 correctly lists all three. |

**Assessment:** All outdated criteria in docs/41 were **deliberately refined** during implementation. docs/44 reflects the final state. No outdated criteria in docs/42, 43, or 44.

---

## 5. Signature Mismatches

| # | Doc | Proposed Signature | Actual Signature | Severity |
|---|-----|-------------------|-----------------|----------|
| S-1 | docs/41 | `DuplicateVendor(SqlConnection conn, string sourceVendorId, int sourceCompanyId, int targetCompanyId, string userId)` | `DuplicateVendor(int sourceId, int targetCompanyId, string userName)` | **Medium** — docs/41 passed connection + sourceCompanyId as params; implementation opens its own connection and reads sourceCompanyId from `CompanyContext`. This is actually an improvement (connection managed internally). |
| S-2 | docs/41 | `DuplicateCustomer(SqlConnection conn, string sourceClientId, int sourceCompanyId, int targetCompanyId, string userId)` | `DuplicateCustomer(int sourceId, int targetCompanyId, string userName)` | **Medium** — Same improvement pattern as vendor. |
| S-3 | docs/41 | `BulkResult` class with `Duplicated`, `Skipped`, `Errors` (List\<string\>) | `BulkDuplicateResult` with `SuccessCount`, `FailedCount`, `SkippedCount`, `EntityType`, `TargetCompanyName`, `FailureReason` | **Medium** — Implementation uses a richer model with entity type context and batch failure reason instead of individual error strings. |

**Assessment:** All signature differences represent **deliberate improvements** made during implementation. The actual signatures are better than the proposals (internal connection management, richer result model). docs/44 uses the correct final signatures.

---

## 6. Class / Field Name Mismatches

| # | Doc | Name Used | Actual Name | Location |
|---|-----|-----------|-------------|----------|
| C-1 | docs/41 | `BulkResult` | `BulkDuplicateResult` | docs/41 vs implementation |
| C-2 | docs/41 | `BulkResult.Duplicated` | `BulkDuplicateResult.SuccessCount` | docs/41 vs implementation |
| C-3 | docs/41 | `BulkResult.Skipped` | `BulkDuplicateResult.SkippedCount` | docs/41 vs implementation |
| C-4 | docs/41 | `BulkResult.Errors` (List\<string\>) | `BulkDuplicateResult.FailureReason` (string) | docs/41 vs implementation |

**Assessment:** All within docs/41 (pre-implementation proposals). docs/42, 43, 44 use the correct names.

---

## 7. Proposed But Not Implemented

| # | Doc | Proposal | Status | Justification |
|---|-----|----------|--------|---------------|
| N-1 | docs/41 | `WITH (UPDLOCK, HOLDLOCK)` on MAX() query | **Not implemented** | Retry loop with COUNT verification used instead. Acknowledged in docs/44 "Known Future Improvements" item #7. |
| N-2 | docs/41 | Counter table (`tbl_IdCounter`) | **Not implemented** | MAX()-based counter retained. Acknowledged as a future improvement. |

**Assessment:** Both are documented as future improvements in docs/44. Not inconsistencies — they are acknowledged trade-offs.

---

## 8. Cross-Document Consistency Matrix

### docs/41 vs docs/42

| Topic | docs/41 | docs/42 | Consistent? |
|-------|---------|---------|-------------|
| Vendor_Id uniqueness | "Cross-tenant collision is possible" | "FAIL — No UNIQUE constraint" | ✅ Consistent |
| Client_Id uniqueness | Same | Same | ✅ Consistent |
| Tables without CompanyID | Lists tbl_Purches, tbl_Expenses | Lists same + tbl_ClientRegAddress, tbl_representative, tbl_Factory | ✅ Consistent (docs/42 is more detailed) |
| FK maps | Not detailed | Full FK diagrams | ✅ docs/42 adds detail |
| findcompanyId() location | New_vendor.aspx.cs + New_client.aspx.cs | Same | ✅ Consistent |
| tbl_SystemNotification | "audit, no tenant" in diagram | "UNCONFIRMED — must verify" | ✅ Consistent (docs/41's diagram was wrong about "no tenant"; docs/42 flagged this correctly) |

### docs/42 vs docs/43

| Topic | docs/42 | docs/43 | Consistent? |
|-------|---------|---------|-------------|
| DAL pattern | — | "Must use Pattern 2 (direct ADO.NET)" | ✅ Consistent (docs/43 adds detail) |
| findcompanyId() calls | "Found in exactly 2 files" | Same | ✅ Consistent |
| Namespace | — | `Bill_Software.corporate.business.app` | ✅ Consistent |
| tbl_SystemNotification CompanyID | "UNCONFIRMED" (docs/42) | "RESOLVED: YES" (docs/43) | ✅ Consistent — docs/43 resolved the open question from docs/42 |

### docs/43 vs docs/44

| Topic | docs/43 | docs/44 | Consistent? |
|-------|---------|---------|-------------|
| File count | 6 files (1 new + 5 modified) | 6 files | ✅ Consistent |
| Method names | `GenerateNextVendorCode`, `GenerateNextClientCode` | Same | ✅ Consistent |
| Namespace | `Bill_Software.corporate.business.app` | Same | ✅ Consistent |
| csproj | `<Compile Include="corporate\business\app\DuplicationService.cs" />` | Same | ✅ Consistent |
| Transaction model | "First page-level code to use explicit SQL transactions" | Same | ✅ Consistent |
| Duplicate button | "Add Duplicate LinkButton per DataList row" | "Per-row 📋 Duplicate button (JS)" | ✅ Consistent (docs/44 is more precise about JS vs LinkButton) |

---

## 9. Line Count Verification

| Doc | Claim | Actual | Status |
|-----|-------|--------|--------|
| docs/44 | DuplicationService.cs: 851 lines | 851 | ✅ Exact match |
| docs/44 | View_vendor.aspx: 251 lines | 251 | ✅ Exact match |
| docs/44 | View_vendor.aspx.cs: 451 lines | 451 | ✅ Exact match |
| docs/44 | View_client.aspx: 324 lines | 324 | ✅ Exact match |
| docs/44 | View_client.aspx.cs: 489 lines | 489 | ✅ Exact match |
| docs/44 | Total new lines: ~1,700 | ~1,700 (verified from diffs) | ✅ Consistent |
| docs/41 | Total complexity estimate: 7-8 days | N/A (planning estimate) | ✅ Reasonable |

---

## 10. Method Name Verification Against Implementation

docs/44 claims these methods exist. Verified against `DuplicationService.cs`:

| Method | docs/44 | Actual | Match? |
|--------|---------|--------|--------|
| `DuplicateVendor(int, int, string)` | ✅ | ✅ line 168 | ✅ |
| `DuplicateCustomer(int, int, string)` | ✅ | ✅ line 308 | ✅ |
| `BulkDuplicateVendors(int[], int, string)` | ✅ | ✅ line 532 | ✅ |
| `BulkDuplicateCustomers(int[], int, string)` | ✅ | ✅ line 681 | ✅ |
| `GenerateNextVendorCode(conn, tran, int)` | ✅ | ✅ line 159 | ✅ |
| `GenerateNextClientCode(conn, tran, int)` | ✅ | ✅ line 296 | ✅ |
| `GenerateNextBusinessCode(...)` | ✅ | ✅ line 50 | ✅ |
| `ResolveDuplicateName(...)` | ✅ | ✅ line 113 | ✅ |
| `VendorInCurrentCompany(conn, int)` | ✅ | ✅ line 817 | ✅ |
| `ClientInCurrentCompany(conn, int)` | ✅ | ✅ line 828 | ✅ |
| `UserCanAccessCompany(int)` | ✅ | ✅ line 839 | ✅ |
| `RowStr(DataRow, string)` | ✅ | ✅ line 844 | ✅ |
| `CopyClientRegAddress(...)` | ✅ | ✅ line 432 | ✅ |
| `CopyFactoryRecords(...)` | ✅ | ✅ line 462 | ✅ |
| `CopyRepresentativeRecords(...)` | ✅ | ✅ line 492 | ✅ |

---

## 11. Pre-existing Defect (Not from docs)

| # | Finding | Severity | Source |
|---|---------|----------|--------|
| X-1 | docs/41 Phase 1 §5 claims `View_vendor.aspx` uses `GridView.DataBind()`. The actual control is a `DataList`, not a `GridView`. | **Low** | docs/41, pre-existing architecture description |

This is an inaccuracy in the original architecture analysis, not introduced by the duplication docs.

---

## Final Verdict

| Document | Consistency Status |
|----------|-------------------|
| **docs/41** (Architecture) | ⚠️ Contains **expected pre-implementation proposals** that differ from the final implementation (method names, signatures, class names, UI flow). These are design proposals, not commitments. The document should be read as "what we planned" not "what we built." |
| **docs/42** (Validation) | ✅ **Fully consistent** with docs/43 and docs/44. Open items (tbl_SystemNotification CompanyID) were resolved in docs/43. |
| **docs/43** (Readiness) | ✅ **Fully consistent** with docs/42 and docs/44. Resolves docs/42's open question. All method names, file paths, and patterns match the implementation. |
| **docs/44** (Completion) | ✅ **Fully consistent** with the actual implementation. All method names, signatures, file paths, line counts, and flow diagrams match reality. |

### Recommendation

**No corrections needed to docs/44.** It accurately describes the implementation.

**docs/41 should NOT be updated** — it is an architecture document written before implementation. Its value is in recording the design rationale and the evolution from proposal to implementation. The differences are expected and informative.

If desired, a one-line note could be added to the top of docs/41:
> *"Note: This document reflects pre-implementation architecture proposals. Method names, signatures, and UI flows were refined during implementation. See docs/44 for the authoritative post-implementation description."*

---

*No production code was modified during this audit.*
