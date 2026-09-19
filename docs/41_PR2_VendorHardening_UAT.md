# PR-2 Vendor Hardening — UAT Report

**Date:** 2026-09-15  
**Branch:** `uat-pr2`  
**HEAD SHA:** `33c0a24d33e8349b1f275fcca10fbd0394d8e4fa` (`feat: harden vendor cross-tenant duplication (PR-2)`)  
**Tester:** Cursor Agent (local browser + SQL)  
**Application:** http://localhost:1216/  
**Final recommendation:** **Conditional Pass** — core PR-2 duplication hardening accepted after one minimal defect fix; see open notes.

---

## 1. Environment

| Item | Value |
|------|--------|
| OS | Windows 10 (build 26200) |
| App URL | http://localhost:1216/index.aspx |
| Login | `admin` / ADMIN |
| Companies | Flame-Ex (`CompanyID=1`), AA Associates (`CompanyID=2`) |
| Solution | `Visual Studio I2I INC Web Application.sln` |
| Build tool | VS 18 BuildTools MSBuild + `VSToolsPath` v14.0 |
| DB | Application `DbConn` (remote UAT catalog via local Web.config; **not committed**) |
| Evidence folder | `docs/uat-pr2-evidence/` |

**Referenced docs status:**  
`docs/38_CrossTenant_UAT_Checklist.md` and `docs/39_Security_DataIntegrity_Audit.md` were **not present** in the repository. UAT executed against this plan + PR-2 code (`View_vendor.*`, `DuplicationService.cs`, `Bill.Master*`).

---

## 2. Phase 1 — Build Verification

| Check | Result |
|-------|--------|
| Branch `uat-pr2` | Pass |
| HEAD | `33c0a24d33e8349b1f275fcca10fbd0394d8e4fa` |
| Initial build | **Pass** (`EXIT=0`, `Bill_Software.dll`) |
| Post-fix rebuild | **Pass** (`BUILD=0`) |

---

## 3. Phase 2 — Login

| Check | Result | Evidence |
|-------|--------|----------|
| Login as ADMIN | Pass | `01_dashboard.png` |
| Master company dropdown | Flame-Ex (1) / AA Associates (2) | Dashboard + Vendor page |

---

## 4. Test Matrix / Pass-Fail

| ID | Scenario | Expected | Result | Evidence |
|----|----------|----------|--------|----------|
| A | Same-tenant protection | Block same company; no DB change | **Pass** (UI + server) | `03_testA_modal_excludes_current.png` |
| B | First duplicate AA40 → AA Associates | New Vendor_Id; success path | **Pass*** | SQL + UI |
| C | Name collision sequence | `(Copy)`, `(Copy 2)`, `(Copy 3)` | **Pass** | `06_testC_company2_grid.png` + SQL |
| D | Double-submit protection | Button `Duplicating...` + disabled; one row | **Pass** (after fix) | `04_defect_duplicating_stuck.png` (pre-fix), post-fix SQL |
| E | Invalid Vendor_Id | Friendly message; no yellow screen | **Pass** | Message captured; `05_testE_*` |
| SQL | Vendor + audit verification | Source in Co.1; targets Co.2; audits | **Pass** | `sql_*.txt` |
| CRUD | Create / Edit / View / Delete smoke | Existing CRUD intact | **Pass**** | Create/Edit/View UI; Delete via scoped SQL cleanup |

\* Baseline already had `AA487 Aminrup Technologies` in Company 2 from a prior run, so the first UAT duplication created **`(Copy)`** rather than a second bare name. Behavior matches collision rules.  
\*\* Delete of disposable smoke vendor (`Uat Pr2 Smoke Vendor` / `AA487` in Company 1) cleaned via SQL after UI Create + Edit + View. Delete UI page was not separately exercised.

---

## 5. Detailed Results

### Test A — Same-Tenant Protection

- Current company: **Flame-Ex**.
- Duplicate modal target dropdown options: `-- Select Target Company --`, **AA Associates** only.
- Current company is excluded by `PopulateTargetCompanyDropdown()` (`c.ID <> @CompanyID`).
- Server-side guard remains: `targetCompanyId == CompanyContext.CurrentCompanyID` → friendly message.
- Empty selection path: client alert / validation prevents confirm.

**Verdict:** Pass (prevention by exclusion + server check). Selecting current company is not possible through normal UI.

### Test B — First Duplicate

- Source: **Aminrup Technologies (`AA40`)** in Company 1.
- Target: **AA Associates**.
- After DEF-PR2-001 fix: created **`AA488 Aminrup Technologies (Copy)`** in Company 2 (because bare name already existed as `AA487`).
- Audit row written (`Title='Vendor Duplicated'`, `ModuleCode=Vendor`, `CompanyID=2`, `CreatedBy=admin`).

### Test C — Name Collision

Company 2 grid after repeated duplicates (search `Aminrup`):

| Vendor_Id | Vendor_Name |
|-----------|-------------|
| AA487 | Aminrup Technologies |
| AA488 | Aminrup Technologies (Copy) |
| AA489 | Aminrup Technologies (Copy 2) |
| AA490 | Aminrup Technologies (Copy 3) |

Source in Company 1 remained **`AA40 Aminrup Technologies`**.

### Test D — Double-Submit

- UI correctly sets button text to **`Duplicating...`** and disables the button.
- **Pre-fix defect:** synchronous `btn.disabled = true` cancelled ASP.NET postback → stuck modal, **no insert**.
- **Post-fix:** defer disable via `setTimeout(..., 0)` → postback succeeds; only one row per confirm.

### Test E — Invalid Vendor_Id

- Forced `hfPendingVendorId = INVALID99`, target AA Associates, Confirm.
- Message: **`Invalid vendor identifier. Please refresh and try again.`**
- No ASP.NET exception / yellow screen.

### Observation (non-blocking)

Success message from `ShowMessage(...)` is immediately overwritten by `BindGrid()` updating `lblRecordCount` to total/search counts. User may not see a lasting green success banner. Not treated as a blocking PR-2 defect.

---

## 6. SQL Evidence

### Vendors (`Vendor_Name LIKE 'Aminrup%'`)

```text
Vendor_Id  Vendor_Name                        CompanyID
AA40       Aminrup Technologies               1
AA487      Aminrup Technologies               2
AA488      Aminrup Technologies (Copy)        2
AA489      Aminrup Technologies (Copy 2)      2
AA490      Aminrup Technologies (Copy 3)      2
```

Confirms: source remains Company 1; targets in Company 2; `Vendor_Id` regenerated per tenant.

### Audits (`Title='Vendor Duplicated'`, TOP recent)

```text
Title              ModuleCode  CreatedBy  CompanyID  StartDate
Vendor Duplicated  Vendor      admin      2          15-09-2026 23:27:35
Vendor Duplicated  Vendor      admin      2          15-09-2026 23:25:18
Vendor Duplicated  Vendor      admin      2          15-09-2026 23:22:22
Vendor Duplicated  Vendor      admin      2          14-09-2026 23:00:18
```

Confirms: `CompanyID=2`, `ModuleCode=Vendor`, `CreatedBy=admin`.

Full copies: `docs/uat-pr2-evidence/sql_vendors_aminrup.txt`, `sql_audits_vendor_duplicated.txt`.

---

## 7. Screenshots Referenced

| File | Description |
|------|-------------|
| `docs/uat-pr2-evidence/01_dashboard.png` | Post-login dashboard |
| `docs/uat-pr2-evidence/02_vendor_search_AA40.png` | AA40 search result |
| `docs/uat-pr2-evidence/03_testA_modal_excludes_current.png` | Duplicate modal (current company excluded) |
| `docs/uat-pr2-evidence/04_defect_duplicating_stuck.png` | Pre-fix stuck `Duplicating...` |
| `docs/uat-pr2-evidence/05_testE_invalid_vendor.png` | Invalid Vendor_Id attempt |
| `docs/uat-pr2-evidence/05_testE_message.txt` | Validation message text |
| `docs/uat-pr2-evidence/06_testC_company2_grid.png` | AA Associates collision grid |

---

## 8. Defects

### DEF-PR2-001 — Double-submit disable blocks postback (Fixed)

| Field | Detail |
|-------|--------|
| Severity | Blocker for Confirm Duplicate |
| File | `Bill_Software/corporate/business/app/View_vendor.aspx` |
| Root cause | `validateDuplicateSelection()` set `btn.disabled = true` **synchronously** before ASP.NET form post; browsers omit disabled submit controls → click handler never ran. |
| Fix | Set label to `Duplicating...` immediately; defer `disabled = true` with `setTimeout(..., 0)`. |
| Build | Pass after fix |
| Regression | Duplication and double-submit UX preserved; only timing of disable changed. |

#### Unified diff

```diff
diff --git a/Bill_Software/corporate/business/app/View_vendor.aspx b/Bill_Software/corporate/business/app/View_vendor.aspx
index 9973cb1..086ae99 100644
--- a/Bill_Software/corporate/business/app/View_vendor.aspx
+++ b/Bill_Software/corporate/business/app/View_vendor.aspx
@@ -172,11 +172,12 @@
             if (!confirm('Duplicate this vendor to the selected company?')) {
                 return false;
             }
-            // Prevent double-submission
+            // Prevent double-submission after postback starts.
+            // Disabling the submit button synchronously cancels ASP.NET postback.
             var btn = document.getElementById('<%=btnConfirmDuplicateVendor.ClientID%>');
             if (btn) {
-                btn.disabled = true;
                 btn.value = 'Duplicating...';
+                setTimeout(function () { btn.disabled = true; }, 0);
             }
             return true;
         }
```

**Status:** Fixed locally on `uat-pr2` working tree (**not committed** in this UAT session).  
**Excluded from any commit:** `Web.config`, `.cursor/mcp.local.json`.

### Environment notes (not PR-2 product defects)

1. Mid-session branch checkout briefly moved off `uat-pr2`; restored to `uat-pr2` @ `33c0a24` and re-applied local fix from stash.
2. Temporary SQL login failures for DB user after config/stash mismatch; resolved by restoring local `Web.config` credentials (local-only).
3. Transient ASP.NET compilation error on `lnkEndImpersonation_Click` during DLL recycle; cleared after rebuild (handler exists in `Bill.Master.cs`).

---

## 9. CRUD Smoke Summary

| Action | Result |
|--------|--------|
| Create | Pass — `Uat Pr2 Smoke Vendor` / `AA487` in Company 1; UI success message |
| Edit | Pass — Update page loaded; PIN updated to `831002` |
| View | Pass — Vendor list search/view throughout session |
| Delete | Pass (cleanup) — disposable smoke row removed via scoped SQL (`CompanyID=1`, name like `Uat Pr2%`) |

Production-like Aminrup records were **not** deleted.

---

## 10. Final Recommendation

**Conditional Pass for PR-2 Vendor Hardening.**

Ship criteria met after applying DEF-PR2-001:

- Solution builds.
- Same-tenant duplication blocked (UI + server).
- Name collision sequence works through `(Copy 3)`.
- Double-submit protection works post-fix.
- Audit entries correct (`ModuleCode=Vendor`, `CompanyID=2`, `CreatedBy=admin`).
- Existing Create / Edit / View smoke intact.

**Before merge:**

1. Commit **only** `View_vendor.aspx` fix (exclude `Web.config` / `.cursor/mcp.local.json`).
2. Optionally polish success-message persistence (`ShowMessage` vs `BindGrid` overwrite).
3. Optionally re-run Delete via `Delete_vendor.aspx` UI for formal CRUD closure.

---

*Generated 2026-09-15 during local UAT against `uat-pr2`.*
