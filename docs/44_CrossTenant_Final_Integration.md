# Cross-Tenant Duplication — Final Integration Report

**Date:** 2026-09-19  
**Target branch:** `July_to_Sept26_DevNSupport`  
**Merge SHA:** `577ebcf2fcf303d7007d6294414f9b4b98efdaa8` (`577ebcf`)  
**Status:** Integrated — PR-1 through PR-4 validated stack is on the development branch  
**Merge type:** Non-fast-forward merge (history preserved; no squash)

---

## 1. Executive Summary

The UAT-validated Cross-Tenant Vendor and Customer Duplication stack was merged from `uat-pr4` (`c121344`) into `July_to_Sept26_DevNSupport`. Fast-forward was not possible: the target had continued with Switch User / impersonation work after the common ancestor `ec2aceb`.

The merge preserves:

- Validated vendor, customer, and bulk duplication behavior
- `WriteDuplicationAudit` against the live `tbl_SystemNotification` schema
- `CompanyContext` as the source-tenant authority
- The later Switch User impersonation runtime already UAT-completed on the target (PR #84)

Solution build succeeded before and after integration (zero compile errors). The only remaining planned product enhancement is **Skip Already Duplicated**.

---

## 2. Final Branch SHA

| Item | Value |
|------|-------|
| Branch | `July_to_Sept26_DevNSupport` |
| Integration merge | `577ebcf2fcf303d7007d6294414f9b4b98efdaa8` |
| Documentation commit | `46f65ce5a2ee1e086e8d4c26c4f90238065db9ef` |
| First parent (pre-integration target) | `fa777e9b3d27a513b3ba8dce66fa370e949d85c5` |
| Second parent (validated stack tip) | `c121344a89ff4294cb7c44d64a0359fc6a25ba02` |
| Merge-base | `ec2acebfca2be98967adcdf084aea29881551362` |

---

## 3. Integrated Commit List

Validated ancestry (no missing commits):

```
PR-1  25b1a7f
  ↓
PR-2  c5d415e
  ↓
PR-3  ebbbd95
  ↓
PR-4  c121344
```

Commits brought onto `July_to_Sept26_DevNSupport`:

| SHA | Message | PR |
|-----|---------|----|
| `f50887e` | feat: add cross-tenant duplication infrastructure (PR-1) | PR-1 |
| `199b349` | fix: reconcile PR-1 UAT company switcher and Switch User markup | PR-1 |
| `25b1a7f` | fix: align DuplicationService audit with live notification schema | PR-1 |
| `33c0a24` | feat: harden vendor cross-tenant duplication (PR-2) | PR-2 |
| `c5d415e` | fix: preserve ASP.NET postback while preventing vendor duplicate double-submit | PR-2 |
| `1e7a38d` | feat: add hardened customer duplication with generalized helpers (PR-3) | PR-3 |
| `ebbbd95` | fix: finalize PR-3 Customer Duplication UAT fixes | PR-3 |
| `771d133` | feat: add bulk cross-tenant duplication (PR-4) | PR-4 |
| `2454bd6` | fix: synchronize bulk duplication designer fields | PR-4 |
| `c121344` | fix: complete bulk duplication event flow | PR-4 |
| `577ebcf` | Merge validated Cross-Tenant Duplication stack (PR-1 to PR-4) into July_to_Sept26_DevNSupport | Integration |

---

## 4. Build Verification

Tooling: VS 18 BuildTools MSBuild + `VSToolsPath` v14.0, solution `Visual Studio I2I INC Web Application.sln`, Configuration=Debug.

| Build | Branch / SHA | Result |
|-------|--------------|--------|
| Baseline | `uat-pr4` / `c121344` | **Pass** (`EXIT=0`, `Bill_Software.dll`) |
| Final | `July_to_Sept26_DevNSupport` / `577ebcf` | **Pass** (`EXIT=0`, `Bill_Software.dll`) |

Zero compile errors on both builds. Remaining messages are pre-existing unused-variable warnings (`CS0168`, `CS0219`) unrelated to duplication.

---

## 5. UAT Completion Summary

| PR | Validated SHA | UAT report | Result |
|----|---------------|------------|--------|
| PR-1 infrastructure + live audit schema | `25b1a7f` | `docs/35`, `docs/36`, `docs/37` | Accepted into stack |
| PR-2 Vendor hardening | `c5d415e` (fix on `33c0a24`) | `docs/41_PR2_VendorHardening_UAT.md` | Conditional Pass → accepted after double-submit postback fix |
| PR-3 Customer duplication | `ebbbd95` (fixes on `1e7a38d`) | `docs/42_PR3_CustomerDuplication_UAT.md` | Conditional Pass → accepted after postback, dropdown, and child-schema fixes |
| PR-4 Bulk duplication | `c121344` | `docs/43_PR4_BulkDuplication_UAT.md` | Pass |

UAT evidence folders (`docs/uat-pr2-evidence/`, `docs/uat-pr3-evidence/`, `docs/uat-pr4-evidence/`) remain local-only and were **not** committed.

---

## 6. Vendor Validation

From `docs/41` (Flame-Ex → AA Associates):

- Same-tenant duplication blocked (current company omitted from target dropdown; server equality guard retained).
- Name collision sequence: original → `(Copy)` → `(Copy 2)` → `(Copy 3)`.
- Double-submit: button caption `Duplicating...`; disable deferred with `setTimeout(..., 0)` so ASP.NET postback still fires; one target row per confirm.
- Invalid `Vendor_Id` returns a friendly message (no yellow screen).
- Source vendor unchanged in Company 1; new `AA`-prefixed codes in Company 2.
- Existing Create / Edit / View smoke remained intact.

Post-merge files: `View_vendor.aspx`, `View_vendor.aspx.cs`, `View_vendor.aspx.designer.cs`, `DuplicationService.DuplicateVendor` / `BulkDuplicateVendors`.

---

## 7. Customer Validation

From `docs/42` (fixture Heatworks Pvt. Ltd. `AD423`):

- Single duplication created a new `AD`-prefixed customer in Company 2.
- Child cloning: `tbl_ClientRegAddress`, `tbl_Factory`, `tbl_representative` copied with the target `Client_Id` and Company 2.
- Transaction exclusions: zero rows in `tbl_Quotation`, `tbl_Invoice`, `tbl_Proforma`, `tbl_Chalan`.
- Same-tenant protection: current company omitted; server guard present.
- Rapid double-submit produced one target row and one audit row.
- Child-source reads are scoped by source `CompanyID`.
- Known out-of-scope: `Update_client.aspx` `Invalid column name 'CompanyID'` via `DB_UTILITY.Fillcombo` (existing edit-page issue, not changed).

Post-merge files: `View_client.aspx`, `View_client.aspx.cs`, `View_client.aspx.designer.cs`, `DuplicationService.DuplicateCustomer` / `BulkDuplicateCustomers`.

---

## 8. Bulk Validation

From `docs/43` at `c121344`:

- Designer fields `hfBulkVendorIds` / `hfBulkClientIds` present in markup and `.designer.cs`.
- Event handlers present: `btnBulkDuplicateVendor_Click`, `btnConfirmDuplicateVendor_Click`, `DataList1_ItemDataBound` (vendor); matching client handlers.
- Select All, hidden ID collection, zero-selection block, and shared modal all passed.
- Vendor bulk: two records (`AA491`, `AA492`) with a visible success summary.
- Customer bulk: two records (`AD04`, `AD05`); child copy and transaction exclusions verified.
- Bulk source-tenant checks run inside the active SQL transaction.
- Abort-on-failure / single-transaction model retained.

---

## 9. Audit Verification

`DuplicationService.WriteDuplicationAudit` is preserved and uses the **live** notification schema (not legacy `Module` / `Type` / `UserId`):

```sql
INSERT INTO dbo.tbl_SystemNotification
(Title, Message, ModuleCode, Severity, StartDate, EndDate, IsActive, CreatedBy, CompanyID)
```

| Field | Value |
|-------|-------|
| `Title` | `Vendor Duplicated` / `Customer Duplicated` |
| `ModuleCode` | `Vendor` / `Client` |
| `Severity` | `Success` |
| `CompanyID` | **Target** company |
| `CreatedBy` | acting user |

UAT confirmed one audit row per successfully duplicated record and no audit for blocked / zero-selection operations. Audit writes remain inside the same `SqlTransaction` as the duplication.

---

## 10. Deployment Checklist

1. Deploy `July_to_Sept26_DevNSupport` at `577ebcf` (or a later commit that contains this merge).
2. Confirm `Bill_Software.csproj` includes `DuplicationService.cs`.
3. Confirm IIS / app pool recycles onto the new `Bill_Software.dll`.
4. Do **not** deploy local `Web.config` connection strings.
5. Confirm acting users have `UserCompanyAccess` to the intended target companies.
6. Optional UAT helper only: `Bill_Software/corporate/business/sql/user_company_access_admin_aa_associates.sql` (do not run blindly against live).
7. Smoke after deploy:
   - View Vendor / View Customer load
   - Single vendor duplicate to a second company
   - Single customer duplicate including children
   - Bulk duplicate of two records
   - Confirm `tbl_SystemNotification` rows with `ModuleCode` + target `CompanyID`
   - Confirm Switch User / End Impersonation still behaves as PR #84
8. Do not copy quotation, invoice, proforma, or challan history (by design).

---

## 11. Rollback Reference

To undo this integration while keeping the pre-merge Switch User / impersonation line:

```text
git checkout July_to_Sept26_DevNSupport
git revert -m 1 577ebcf2fcf303d7007d6294414f9b4b98efdaa8
```

`-m 1` selects first parent `fa777e9` (target before duplication). Rebuild the solution after revert.

Do **not** reset or force-push `master`. Do not squash this merge.

Conflict files kept from the target during integration:

- `Bill_Software/corporate/business/app/SwitchUser.aspx`
- `Bill_Software/corporate/business/app/SwitchUser.aspx.cs`

Those files remain the PR #84 impersonation runtime. Rolling back duplication does not require changing them.

---

## 12. Remaining Planned Enhancement

### Skip Already Duplicated

**Status:** Not implemented. Recorded in PR-4 UAT (`docs/43`).

Current bulk behavior creates another suffixed copy when the source name already exists in the target tenant (for example Heatworks Pvt. Ltd. → `Heatworks Pvt. Ltd. (Copy 2)` as `AD04`).

Planned behavior: detect an already-duplicated source in the target company and skip it (`SkippedCount`), rather than inserting another `(Copy N)` row.

This is a separate enhancement. Do not treat it as a merge defect.

Other non-blocking observations (not blockers for this integration):

- Success banner can be overwritten by `BindGrid()` record-count text on single-record paths.
- Customer edit page schema issue (`Update_client.aspx` / `DB_UTILITY.Fillcombo`) is outside duplication scope.

---

## Repository Review (integration session)

### Local changes classified before merge

| Path | Class | Disposition |
|------|-------|-------------|
| `Bill_Software.csproj` SubType noise | tooling | Restored; not committed |
| `Web.config` connection string | environment | Stashed / restored locally; **excluded** |
| `.cursor/*` | tooling | **Excluded** |
| `docs/41`–`docs/43` | documentation | Added with this report |
| `docs/uat-pr*-evidence/` | UAT evidence | **Excluded** |

Implementation working tree was clean at merge time.

### Conflicts

Only `SwitchUser.aspx` and `SwitchUser.aspx.cs`. Resolved by keeping `July_to_Sept26_DevNSupport` (ours): the later impersonation runtime. Duplication files auto-merged from `uat-pr4`.

Auto-merged without conflict: `Bill_Software.csproj`, `AuthGuard.cs`, `Bill.Master.cs`, `Bill.Master.designer.cs`.

Note: `docs/34_*` and `docs/37_*` exist as **two filename families** (Impersonation vs Cross-Tenant). They coexist; they are not the same files.

### Post-merge file presence

| File | Present |
|------|---------|
| `DuplicationService.cs` | Yes |
| `View_vendor.aspx` / `.cs` / `.designer.cs` | Yes |
| `View_client.aspx` / `.cs` / `.designer.cs` | Yes |
| `docs/34_CrossTenant_Duplication_Architecture.md` | Yes |
| `docs/37_CrossTenant_Duplication_Completion.md` | Yes |
| `docs/41_PR2_VendorHardening_UAT.md` | Yes |
| `docs/42_PR3_CustomerDuplication_UAT.md` | Yes |
| `docs/43_PR4_BulkDuplication_UAT.md` | Yes |

Designer references: `hfBulkVendorIds`, `hfBulkClientIds` declared. Event handlers listed in section 8 are present. Notification INSERT uses `ModuleCode` / `Severity` (no legacy schema).

---

*Generated 2026-09-19 during final integration of PR-1–PR-4 onto `July_to_Sept26_DevNSupport`.*
