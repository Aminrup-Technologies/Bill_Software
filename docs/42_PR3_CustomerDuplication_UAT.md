# PR-3 Customer Duplication — UAT Report

**Date:** 2026-09-16  
**Branch:** `uat-pr3`  
**Baseline HEAD:** `1e7a38da10d789f97db9da72d4da7cf6d552b1d5`  
**Application:** `http://localhost:1216/`  
**Result:** **Conditional Pass** — core duplication passes with the minimal
uncommitted defect fixes described below.

## Scope and environment

- Solution build passed at the baseline HEAD.
- Final build passed after the UAT defect fixes.
- Source company: Flame-Ex (`CompanyID=1`).
- Target company: AA Associates (`CompanyID=2`).
- Fixture: Heatworks Pvt. Ltd. (`AD423`), containing one registration address,
  one factory, and one representative.
- Requested prerequisite documents `docs/38_CrossTenant_UAT_Checklist.md` and
  `docs/39_Security_DataIntegrity_Audit.md` were not present on this branch.

## UAT results

### Single customer duplication — Pass after fixes

- New target code: `AD02`.
- Target company: `CompanyID=2`.
- Customer master data was created.
- The transaction rolled back cleanly during the failed schema attempt; no
  partial target customer or child rows remained.

### Child-table cloning — Pass

The source and each target copy contain:

- `tbl_ClientRegAddress`: 1 row.
- `tbl_Factory`: 1 row.
- `tbl_representative`: 1 row.

Target child rows use the generated target `Client_Id` and `CompanyID=2`.

### Transaction exclusions — Pass

Both target copies have zero rows in:

- `tbl_Quotation`
- `tbl_Invoice`
- `tbl_Proforma`
- `tbl_Chalan`

### Same-tenant protection — Pass with UX observation

- The current company is omitted from the target-company dropdown.
- A tampered same-tenant option was rejected by ASP.NET event validation before
  the server handler executed.
- The handler also contains an explicit source/target equality guard.
- Observation: a tampered postback displays the framework error page rather
  than a friendly validation message.

### Rapid double-submit — Pass

Confirm was invoked twice rapidly. Exactly one target row (`AD03`) and one audit
row were created. The second target was named `Heatworks Pvt. Ltd. (Copy)` due
to the existing `AD02` record.

### Audit logging — Pass

Notifications `46` and `47` were written to `tbl_SystemNotification` with:

- Title: `Customer Duplicated`
- Module: `Client`
- Severity: `Success`
- Company: `2`
- User: `admin`

## Confirmed PR-3 defects fixed locally

### DEF-PR3-001 — synchronous disable cancelled ASP.NET postback

`View_client.aspx` disabled the submit input synchronously. The handler now
changes the caption immediately and defers `disabled=true` with
`setTimeout(..., 0)`.

### DEF-PR3-002 — postback erased the selected target company

`View_client.aspx.cs` repopulated the target dropdown during every postback,
resetting the submitted value. Population now occurs only on initial load.

### DEF-PR3-003 — registration-address clone used non-existent columns

`DuplicationService.CopyClientRegAddress` targeted columns that are not present
in the UAT schema. It now maps the live columns (`Address`, `Phno`, etc.), sets
the target company and audit fields, and retains the existing transaction.

All three child-source reads are now scoped by source `CompanyID` to avoid
cross-tenant child mixing when client codes overlap.

## CRUD smoke

- View Customer: **Pass**.
- Create Customer: **Pass** — disposable customer `AD424` created.
- Delete Customer: **Pass** — disposable customer deleted; SQL count is zero.
- Edit Customer: **Blocked by unrelated existing issue** —
  `Update_client.aspx` raises `Invalid column name 'CompanyID'` while loading a
  dropdown through `DB_UTILITY.Fillcombo`.

## Open observations

- After a successful duplicate, `BindGrid()` overwrites the success text with
  the normal record-count label. The database operation succeeds, but the
  expected success message is not visible.
- The edit-page schema issue is outside PR-3 duplication scope and was not
  changed.
- `Bill_Software.csproj`, `Web.config`, and `.cursor/mcp.local.json` remain
  outside the PR-3 defect diff.

## Evidence

- SQL script:
  `docs/uat-pr3-evidence/01_customer_duplication_validation.sql`
- SQL results:
  `docs/uat-pr3-evidence/02_customer_duplication_results.md`
- Dashboard:
  `C:/Users/ANMOL/AppData/Local/Temp/cursor/screenshots/page-2026-09-16T02-28-26-341Z.png`
- Initial child-schema failure:
  `C:/Users/ANMOL/AppData/Local/Temp/cursor/screenshots/page-2026-09-16T02-43-26-789Z.png`
- Successful corrected duplication:
  `C:/Users/ANMOL/AppData/Local/Temp/cursor/screenshots/page-2026-09-16T02-47-53-108Z.png`
- Same-tenant tamper rejection:
  `C:/Users/ANMOL/AppData/Local/Temp/cursor/screenshots/page-2026-09-16T02-49-29-446Z.png`
- Rapid double-submit:
  `C:/Users/ANMOL/AppData/Local/Temp/cursor/screenshots/page-2026-09-16T02-50-10-817Z.png`
- CRUD create:
  `C:/Users/ANMOL/AppData/Local/Temp/cursor/screenshots/page-2026-09-16T02-59-16-610Z.png`
- CRUD delete:
  `C:/Users/ANMOL/AppData/Local/Temp/cursor/screenshots/page-2026-09-16T03-02-30-153Z.png`

## Recommendation

Commit only the three PR-3 implementation files after review, keeping local
configuration and unrelated project-file changes excluded. Once that commit is
propagated through the stack, PR-3 customer duplication is suitable for final
acceptance; track the success-message and edit-page issues separately.
