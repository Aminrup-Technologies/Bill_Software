# Cross-Tenant Duplication v1.1 — Release Record

**Date:** 2026-09-19  
**Branch:** `July_to_Sept26_DevNSupport`  
**Release tag:** `cross-tenant-v1.1` → `88f9d0f`  
**Documentation HEAD:** `c11f619`  
**Status:** Complete — signed off

---

## 1. Executive Summary

Cross-Tenant Duplication **v1.1** is the closed release of vendor and customer copy across tenants on `July_to_Sept26_DevNSupport`.

v1.0 (PR-1–PR-4, merge `577ebcf`) delivered single-record and bulk duplication, live `WriteDuplicationAudit`, and UAT-validated vendor/customer/bulk paths. v1.1 (PR **#92**, tag `cross-tenant-v1.1` at `88f9d0f`) adds **Skip Already Duplicated** for bulk: existing names in the target company are skipped, new names are copied, the batch continues, and the UI reports duplicated / skipped / failed.

Single-record `(Copy)` / `(Copy N)` behavior, `CompanyContext`, `Session["CompanyID"]`, child cloning for **new** customers, and Switch User (PR #84) are unchanged. Documentation baseline after tag is `c11f619`.

---

## 2. Release timeline (PR-1 → PR-4.1)

```
PR-1  f50887e / 199b349 / 25b1a7f     infrastructure + live audit schema
  ↓
PR-2  33c0a24 / c5d415e               vendor hardening + postback-safe double-submit
  ↓
PR-3  1e7a38d / ebbbd95               customer duplication + child clone + UAT fixes
  ↓
PR-4  771d133 / 2454bd6 / c121344     bulk duplication + designer + handlers
  ↓
      577ebcf                         merge PR-1–PR-4 into July_to_Sept26_DevNSupport
  ↓
      46f65ce / bffa253               integration docs (44) + SHA table
  ↓
PR-4.1 e3a0b21                        Skip Already Duplicated
  ↓
      88f9d0f  tag cross-tenant-v1.1  PR #92 merge
  ↓
      c11f619                         v1.1 documentation baseline
```

UAT reports: `docs/41` (PR-2), `docs/42` (PR-3), `docs/43` (PR-4). Specs: `docs/44` (integration), `docs/45` (skip). This file is the v1.1 release record.

---

## 3. Branch and commit table

| SHA | Role |
|-----|------|
| `577ebcf` (`577ebcf2fcf303d7007d6294414f9b4b98efdaa8`) | Merge of validated PR-1–PR-4 into `July_to_Sept26_DevNSupport` |
| `bffa253` (`bffa2532a0884246c086f6a8c9710ec0be8187ef`) | Integration documentation close (`docs/44` SHA table) |
| `88f9d0f` (`88f9d0f545492cefcfab1c491f219fc6c592d87b`) | **Tag `cross-tenant-v1.1`** — PR #92 merge of Skip Already Duplicated |
| `c11f619` (`c11f619b2eaaccba7b891f9879d72df53fc41b93`) | Docs: finalize Cross-Tenant Duplication v1.1 baseline (current branch HEAD) |

Supporting SHAs (not release markers): stack tip `c121344`; skip implementation `e3a0b21`.

---

## 4. Delivered capability matrix

| Capability | v1.0 (`577ebcf`) | v1.1 (`cross-tenant-v1.1` / `88f9d0f`) |
|------------|------------------|----------------------------------------|
| Single vendor duplicate (`AA` code, target `CompanyID`) | Yes | Yes (unchanged) |
| Single customer duplicate (`AD` code + children) | Yes | Yes (unchanged) |
| Same-tenant blocked | Yes | Yes |
| Name collision `(Copy)` / `(Copy N)` on **single** record | Yes | Yes |
| Bulk vendor / customer, one transaction | Yes | Yes |
| Bulk skip existing target name (trim, case-insensitive) | No (created `(Copy N)`) | **Yes** |
| Bulk summary duplicated / skipped / failed | Partial | **Yes** |
| Audit only newly created rows | Yes | Yes (skips not audited) |
| Quotation / invoice / proforma / challan not copied | Yes | Yes |
| `CompanyContext` / `WriteDuplicationAudit` | Yes | Yes |

---

## 5. Vendor validation summary

From `docs/41` (Flame-Ex → AA Associates), retained in v1.1:

- Same-tenant duplication blocked (dropdown omits current company; server equality guard).
- Single-record collision: original → `(Copy)` → `(Copy 2)` → `(Copy 3)`.
- Double-submit: `Duplicating...` with deferred `disabled` so ASP.NET postback fires; one target row.
- Invalid `Vendor_Id`: friendly message, no yellow screen.
- Source vendor unchanged; new `AA` codes in the target tenant.

v1.1 bulk: if normalized `Vendor_Name` already exists in the target `CompanyID`, skip before `Vendor_Id` generation.

---

## 6. Customer validation summary

From `docs/42` (Heatworks Pvt. Ltd. `AD423`), retained in v1.1:

- New `AD`-prefixed customer in the target tenant.
- Children cloned: `tbl_ClientRegAddress`, `tbl_Factory`, `tbl_representative` with target `Client_Id` and target `CompanyID`.
- Zero rows in `tbl_Quotation`, `tbl_Invoice`, `tbl_Proforma`, `tbl_Chalan`.
- Same-tenant protection; rapid double-submit produced one row and one audit.
- Child-source reads scoped by source `CompanyID`.
- Out of scope: `Update_client.aspx` `Invalid column name 'CompanyID'` via `DB_UTILITY.Fillcombo`.

v1.1 bulk: existing `Client_Name` in the target is skipped; children are cloned only for newly created customers.

---

## 7. Bulk duplication validation summary

From `docs/43` at `c121344`, retained in v1.1:

- Designer fields `hfBulkVendorIds` / `hfBulkClientIds` and bulk/confirm/`ItemDataBound` handlers present.
- Select All, hidden IDs, zero-selection block, shared modal.
- Vendor bulk (`AA491`, `AA492`) and customer bulk (`AD04`, `AD05`) with child copy and transaction exclusions.
- Source-tenant checks inside the active SQL transaction.
- Genuine failure still rolls back the entire batch.

---

## 8. Skip Already Duplicated summary

From `docs/45`, merged at `88f9d0f` (tag `cross-tenant-v1.1`):

- Detect: trimmed, case-insensitive `Vendor_Name` / `Client_Name` within **target** `CompanyID`.
- Skip: no code generation, no INSERT, no audit, no child copy; `SkippedCount` / `SkippedItems` / `SkipReason`.
- Continue: remaining rows still process.
- UI: `{n} duplicated, {n} skipped, {n} failed.`
- Single-record path still uses `ResolveDuplicateName()`.
- Closes the PR-4 gap (Heatworks already in Company 2 no longer creates `(Copy 2)` on bulk).

---

## 9. Security and audit summary

- Target company access: `AuthGuard.UserCanAccessCompany`.
- Source ownership: `CompanyContext.CurrentCompanyID` (not a swapped session).
- Same-tenant equality guard retained.
- Parameterized SQL; skip checks use `@CompanyID` + `@Name`.
- `WriteDuplicationAudit` live schema: `Title`, `Message`, `ModuleCode`, `Severity`, `StartDate`, `EndDate`, `IsActive`, `CreatedBy`, `CompanyID` (not legacy `Module` / `Type` / `UserId`).
- Audit `CompanyID` is the **target**; one row per successful insert; **no** audit for skips or blocked/zero-selection operations.
- Switch User impersonation (PR #84) preserved; `CompanyID` is not swapped during impersonation.

---

## 10. Build verification

Tooling: VS 18 BuildTools MSBuild + `VSToolsPath` v14.0, `Visual Studio I2I INC Web Application.sln`, Configuration=Debug.

| Build | SHA | Result |
|-------|-----|--------|
| PR-4 stack tip | `c121344` | Pass (`EXIT=0`, `Bill_Software.dll`) |
| Post PR-1–PR-4 merge | `577ebcf` | Pass |
| Post PR-4.1 implementation | `e3a0b21` | Pass |

Zero compile errors. Remaining messages are pre-existing unused-variable warnings (`CS0168`, `CS0219`).

---

## 11. Rollback guidance

**v1.1 only** (remove Skip Already Duplicated, keep PR-1–PR-4):

```text
git checkout July_to_Sept26_DevNSupport
git revert -m 1 88f9d0f545492cefcfab1c491f219fc6c592d87b
```

`-m 1` selects the first parent of the PR #92 merge (pre-skip line). Rebuild after revert. Tag `cross-tenant-v1.1` remains a historical pointer at `88f9d0f`.

**Full duplication stack** (also undo PR-1–PR-4):

```text
git revert -m 1 577ebcf2fcf303d7007d6294414f9b4b98efdaa8
```

Do **not** reset or force-push `master`. Do not squash these merges. Switch User files stay on the PR #84 runtime.

---

## 12. Remaining local-only artifacts

These must **not** be committed or deployed:

| Artifact | Class |
|----------|--------|
| `Bill_Software/Web.config` | environment (connection strings) |
| `Bill_Software/.cursor/*` | tooling |
| `docs/uat-pr2-evidence/` | UAT evidence |
| `docs/uat-pr3-evidence/` | UAT evidence |
| `docs/uat-pr4-evidence/` | UAT evidence |

---

## 13. Final sign-off

Cross-Tenant Duplication **v1.1** is **complete**.

- Stack PR-1–PR-4 is on `July_to_Sept26_DevNSupport` at `577ebcf`.
- Skip Already Duplicated is on the same branch at `88f9d0f`, tagged **`cross-tenant-v1.1`**.
- Documentation baseline is `c11f619`.
- Vendor, customer, bulk, skip, audit, and build gates recorded above have passed or been accepted.
- No further Cross-Tenant Duplication v1.1 product work is open.

Sign-off date: 2026-09-19.

---

*Release record for tag `cross-tenant-v1.1` (`88f9d0f`) on `July_to_Sept26_DevNSupport`.*
