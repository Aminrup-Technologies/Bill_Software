# PR-4 Bulk Cross-Tenant Duplication UAT

**Date:** 2026-09-16  
**Branch:** `uat-pr4`  
**Baseline SHA:** `771d133`  
**Designer-fix SHA:** `2454bd6`  
**Bulk-handler implementation SHA:** `c121344`  
**Application:** `http://localhost:1216/index.aspx`  
**Result:** **Pass — bulk Vendor and Customer duplication verified**

## Environment and scope

- Source company: Flame-Ex (`CompanyID=1`).
- Intended target company: AA Associates (`CompanyID=2`).
- Login: established UAT administrator.
- The existing modified `Bill_Software.csproj`, modified `Web.config`,
  `.cursor` files, and prior UAT evidence were preserved.
- The requested prerequisite documents
  `docs/38_CrossTenant_UAT_Checklist.md` and
  `docs/39_Security_DataIntegrity_Audit.md` are not present on `uat-pr4`.

## Designer synchronization

Both controls already existed in markup with `runat="server"`:

- `View_vendor.aspx`: `hfBulkVendorIds`
- `View_client.aspx`: `hfBulkClientIds`

Only the matching `HiddenField` declarations were added to:

- `View_vendor.aspx.designer.cs`
- `View_client.aspx.designer.cs`

No markup, code-behind, service, configuration, or unrelated designer content
was included in commit `2454bd6`.

## Build confirmation

| Build | Result | Evidence |
|---|---|---|
| Baseline at `771d133` | Fail | Four `CS0103` errors for the two missing hidden-field declarations |
| After designer synchronization | Pass | `Bill_Software.dll` produced; existing unrelated warnings only |
| Handler implementation at `c121344` | Pass | `Bill_Software.dll` produced |
| Final build | Pass | No errors; existing unrelated warnings only |

The solution build does not compile each ASPX page's event wiring. The runtime
page compilation therefore exposed additional PR-4 defects after the normal
MSBuild passed.

## Root cause and implementation completion

### DEF-PR4-001 — Vendor bulk button references a missing handler

`View_vendor.aspx` declares:

`OnClick="btnBulkDuplicateVendor_Click"`

The click handler and the referenced `DataList1_ItemDataBound` handler were
both absent from `View_vendor.aspx.cs`. Runtime page compilation therefore
produced `CS1061`.

### DEF-PR4-002 — Customer bulk button references a missing handler

`View_client.aspx` declares:

`OnClick="btnBulkDuplicateClient_Click"`

The click handler and the referenced `DataList1_ItemDataBound` handler were
both absent from `View_client.aspx.cs`. Runtime page compilation therefore
produced `CS1061`.

Commit `c121344` restores all four referenced server handlers. The item-bound
handlers map each displayed business code to the numeric source primary key
required by `BulkDuplicateVendors()` and `BulkDuplicateCustomers()`.

Live checkbox testing also found that the JavaScript used an ends-with selector
for generated row control IDs. ASP.NET appends a row index, so both selectors
were corrected to match IDs containing `hfVendorId` / `hfClientId`.

Finally, the bulk source-tenant checks now receive the active SQL transaction.
This removes the pending-local-transaction failure while preserving the
single-record calls, `CompanyContext`, target access checks, and
`WriteDuplicationAudit()`.

### Files changed in implementation commit

- `corporate/business/app/View_vendor.aspx`
- `corporate/business/app/View_vendor.aspx.cs`
- `corporate/business/app/View_client.aspx`
- `corporate/business/app/View_client.aspx.cs`
- `corporate/business/app/DuplicationService.cs`

## Test matrix

| Area | Test | Result | Evidence / observation |
|---|---|---|---|
| Vendor | Row checkbox rendering | Pass | 486 row checkboxes rendered |
| Vendor | Header Select All | Pass | 486 of 486 rows selected |
| Vendor | Hidden selected IDs | Pass | Two selected rows produced numeric IDs `2054,2053` |
| Vendor | Existing Duplicate path | Pass | Opens shared modal with single pending ID and empty bulk field |
| Vendor | Zero selection/no postback | Pass | Friendly alert, `false` return, URL unchanged |
| Vendor | Same-tenant protection | Pass | Company 1 excluded from dropdown; server equality guard preserved |
| Vendor | Bulk duplicate and summary | Pass | `AA491` and `AA492`; visible two-record success summary |
| Customer | Row checkbox rendering | Pass | 423 row checkboxes rendered |
| Customer | Header Select All | Pass | 423 of 423 rows selected |
| Customer | Hidden selected IDs | Pass | Two selected rows produced numeric IDs `2156,2155` |
| Customer | Existing Duplicate path | Pass | Opens shared modal with single pending ID and empty bulk field |
| Customer | Zero selection/no postback | Pass | Friendly alert, `false` return, URL unchanged |
| Customer | Same-tenant protection | Pass | Company 1 excluded from dropdown; server equality guard preserved |
| Customer | Bulk duplicate and summary | Pass | `AD04` and `AD05`; visible two-record success summary |
| Customer | Child-table copying | Pass | `AD04`: 1 address, 1 factory, 1 representative |
| Customer | Transaction exclusions | Pass | New customers have zero quotation/invoice/proforma/chalan rows |
| Audit | One audit per duplicated record | Pass | Two Vendor and two Customer audit rows |
| Audit | No audit for blocked operations | Pass | Zero-selection tests produced no audit |
| Transaction | Forced rollback | Not forced | Atomic transaction/catch path retained; no safe schema-free injected SQL failure used |

## SQL evidence

Evidence files:

- `docs/uat-pr4-evidence/01_pr4_verification.sql`
- `docs/uat-pr4-evidence/02_pr4_sql_results.txt`
- `docs/uat-pr4-evidence/run_pr4_verification.ps1`

Observed PR-4 target inserts:

- Vendor `AA491`: Rahul Udyog.
- Vendor `AA492`: ZENTECH SYSTEMS & SOLUTIONS.
- Customer `AD04`: Heatworks Pvt. Ltd. (Copy 2).
- Customer `AD05`: DD INTERNATIONAL.
- `AD04` copied one registration address, one factory, and one representative.
- `AD05` copied its one registration address and correctly had no source
  factory or representative to copy.
- Both new customers have zero quotation, invoice, proforma, and chalan rows.
- Audits since UAT start: two Vendor and two Customer rows, matching the four
  committed records exactly.

## Screenshots

- Vendor runtime compilation error:
  `C:/Users/ANMOL/AppData/Local/Temp/cursor/screenshots/pr4_vendor_compilation_error.png`
- Customer runtime compilation error:
  `C:/Users/ANMOL/AppData/Local/Temp/cursor/screenshots/pr4_customer_compilation_error.png`
- Vendor Select All:
  `C:/Users/ANMOL/AppData/Local/Temp/cursor/screenshots/page-2026-09-16T05-42-54-549Z.png`
- Vendor bulk success:
  `C:/Users/ANMOL/AppData/Local/Temp/cursor/screenshots/page-2026-09-16T05-45-51-235Z.png`
- Customer Select All:
  `C:/Users/ANMOL/AppData/Local/Temp/cursor/screenshots/page-2026-09-16T05-46-30-110Z.png`
- Customer bulk success:
  `C:/Users/ANMOL/AppData/Local/Temp/cursor/screenshots/page-2026-09-16T05-47-45-439Z.png`

## Known gap: Skip Already Duplicated

Bulk duplication of Heatworks Pvt. Ltd., already present in Company 2, created
`Heatworks Pvt. Ltd. (Copy 2)` as `AD04`. Current bulk behavior therefore
creates another suffixed copy rather than skipping an already duplicated
record.

The planned enhancement remains:

**Skip Already Duplicated**

It was not implemented during this UAT.

## Final assessment

The designer synchronization and missing bulk implementation are verified.
Vendor and Customer bulk duplication, selection behavior, child copying,
transaction exclusions, success summaries, and audit cardinality pass.

PR-4 is accepted as UAT complete. The only planned follow-up is the separate
idempotency enhancement, **Skip Already Duplicated**.
