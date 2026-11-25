# Product Catalog & Inventory — Documentation Audit (read-only baseline)

| Item | Value |
|------|--------|
| Audit date | 2026-11-17 |
| Mode | **READ-ONLY.** No application code, SQL, configuration, project file or existing document was modified. No stash/checkout/reset/commit/push/merge. |
| Branch | `July_to_Sept26_DevNSupport` |
| HEAD at audit | `bed7daef98fadac3f061a1fdee0b2d52eb3484ee` |
| Scope | Product Catalog, Product Master, Product Category, Product Stock / Inventory, and every directly coupled concept (opening stock, stock adjustment, transfer, warehouse, GRN, sales deduction, returns, reorder level) |
| New artefact | `docs/PRODUCT_CATALOG_INVENTORY_DOCUMENTATION_AUDIT.md` (this file) — the only file created |
| Location convention | Markdown sources have **no page numbers**. Every citation is `path` + `##` heading (+ table/section within it). Line numbers are cited only where a document itself cites them. |

---

## 0. Method

1. Captured branch, HEAD, `git status --porcelain -uall`, index state and `git diff --stat` **before** any tool wrote anything.
2. Recursively discovered documentation: `**/*.{md,txt,pdf,docx,xlsx,doc,xls,csv,rtf,pptx,rdlc}` over the whole tree.
3. Ran terminology scans over all 111 Markdown files under `docs/` plus root/external docs using:
   `Product Master`, `Product/Service Master`, `Product Catagory`/`Product Category`, `Product Parent`, `Product Stock`, `Inventory`, `Stock Ledger`, `Opening Stock`, `Stock Adjustment`, `Stock Transfer`, `Warehouse`, `Re-order/Reorder`, `GRN`, `Goods Receipt`, `Purchase Receipt`, `Sales Deduction`, `Return`.
4. Read in depth every file that returned a relevant hit. Files whose only hits were unrelated homonyms (e.g. "secret inventory", "file inventory", "mailer inventory") were verified by reading surrounding context and excluded with that reason recorded in §3.3.
5. Recovered requirements were labelled from the document's **own** stated status. Where a document says "PENDING VERIFICATION", "Gap", "Open", "BLOCKED", "NOT GRANTED" or "Proposed only", that wording is preserved rather than resolved.
6. No implementation file was inspected. Code paths appear only where a source document already names them (the business-process and Gate 2 documents are themselves source reviews, so their `file:line` citations are reproduced as *reported by the document*, not as my own reading).

### Terminology notes (recovered as-is, not normalised)

- The live page is spelled **`newproductparent` / "Product Catagory"** (misspelled in the product). `DOMAIN_masters.md` → "Two product catalogs".
- **"Product Parent"** and **"Product/Service Master"** do not appear anywhere in the documentation. The category concept is documented as `tbl_NewparentProduct`, `parentId`, `ParentCategoryId`, `ParentCategory` (`ADR-002 §3`, `UAT_CATALOG.md` table list, `SHARED_SCHEMA.md` FK list). This is recorded as a terminology gap, not translated.
- **"Reorder level" / "Re-order"**: **zero occurrences** in any document. See §6.9.
- **"Purchase Receipt"** and **"Sales Deduction"** as literal phrases: **zero occurrences**. The concepts exist only as "goods receipt / GRN" and "inventory deduction on Tax Invoice". See §6.6 and §6.8.

---

## 1. Git baseline

### 1.1 Initial state (captured before any read/write action)

```
branch : July_to_Sept26_DevNSupport
HEAD   : bed7daef98fadac3f061a1fdee0b2d52eb3484ee
status :
 M .freebuff/worktrees/ca4a92f5-6dfc-43ef-ad46-89d02457dda0
 M "Bill_Software/Properties/PublishProfiles/exc.aagroupindia.com - Web Deploy.pubxml"
 M Bill_Software/Web.config
 M docs/HANDOFF.md
 M docs/SOLUTION_INDEX.md
?? Bill_Software/corporate/business/sql/category_merger_live_verify.sql
```

These six entries were **pre-existing**. They were left untouched. Two of them matter for evidence reading: `docs/HANDOFF.md` and `docs/SOLUTION_INDEX.md` on disk differ from HEAD (`git diff --stat`: HANDOFF +32/−? lines, SOLUTION_INDEX 2 lines). The differences are Wave-1 Attendance/Leave status wording only — **no Product/Inventory text changed** — so the register below is valid for both disk and HEAD content on this topic. See §8 (C-09) for one HANDOFF statement this audit found contradicted by the tree.

### 1.2 Final state

Recorded in §10. `docs/PRODUCT_CATALOG_INVENTORY_DOCUMENTATION_AUDIT.md` is the only addition.

---

## 2. Headline finding (read this first)

**No Product Catalog / Inventory roadmap with dated phases, milestones, priorities or owners exists in this repository.**

What *does* exist, and must not be mistaken for a roadmap:

| Artefact | What it actually is | Implementation authority |
|----------|---------------------|--------------------------|
| `docs/architecture/FLMX_Inventory_Management_Architecture_v1.0.md` | "Baseline for Section-by-Section Architecture Review" (2026-11-07). Business-logic register approved **as review input**; section reviews **pending** | **"Implementation Authorization: NOT GRANTED"** |
| `docs/architecture/ADR-002_MultiTenancy_Ownership_Model.md` | Ratified **ownership model** (frozen 2026-10-01), not a feature plan | Normative for ownership only; implementation **Blocked** |
| `docs/architecture/UAT_MIGRATION_GOVERNANCE.md` | 11-gate migration sequence for multi-tenancy | Gate 1 complete, **Gate 2 BLOCKED**, Gate 3 not authorized |
| `docs/business_process/01`–`10` | As-implemented process documentation (2026-11-06) | Documentation only; explicitly "Database Verification Pending" |
| `docs/architecture/OWNERSHIP_AUDIT.md`, `GATE2_DEPENDENCY_IMPACT_REPORT.md` | Live-UAT evidence and dependency inventory | Evidence, not plan |

Consequences that follow directly from the documents:

- The only **sequencing** that applies to Product/Inventory work is the multi-tenancy gate ladder (§7.1) — and it is stopped at Gate 2.
- The only **priority statement** anywhere is `docs/HANDOFF.md` → "Current Status" / "Current Task": *"1. Bug fixing and remediation of already implemented features. 2. Development of approved new features. 3. Regression verification and documentation of completed changes."* It names no Product/Inventory item.
- `docs/HANDOFF.md` → "Pending Work" states: *"Product, stock, purchase, and invoice ownership implementation stays blocked."*
- No document assigns a wave, sprint, milestone, date, or owner to any Product Catalog/Inventory capability. The "Wave 1 / Wave 2" programme in `HANDOFF.md` is **Attendance & Leave** only and is unrelated.

---

## 3. Documentation inventory

### 3.1 Inspected in depth (evidence-bearing for this audit)

| # | Path | Role in this audit |
|--:|------|--------------------|
| 1 | `docs/SOLUTION_INDEX.md` | Documentation map; "Product / stock / purchase ownership, live UAT verification (implementation blocked)"; archive-proposal header |
| 2 | `docs/ARCHITECTURE.md` | Canonical evergreen architecture; multi-tenant isolation principles incl. target Product/Category/Store/stock ownership |
| 3 | `docs/HANDOFF.md` | Working status; "Blocked", "GATE 2 BLOCKED", known-debt list, execution priority |
| 4 | `docs/CURSOR_RULES.md` | Agent rulebook; no Product requirements; SQL/parameterisation constraints that govern inventory work |
| 5 | `docs/CURSOR_SESSION.md` | Session bootstrap; confirms read order and no Product commitments |
| 6 | `docs/business_process/BUSINESS_PROCESS_MASTER_INDEX.md` | Lifecycle, dependency, inventory-effect and status matrix (2026-11-06, HEAD `026b36e`) |
| 7 | `docs/business_process/01_product_category_and_products.md` | **Primary** Product Category + Product Master evidence |
| 8 | `docs/business_process/02_purchase_and_inventory_receipt.md` | Direct-purchase stock posting; `tbl_stock` write paths |
| 9 | `docs/business_process/03_purchase_payment_ap.md` | Confirms "no stock movement" |
| 10 | `docs/business_process/04_purchase_requisition_and_po.md` | PO release is not receipt; no stock SQL |
| 11 | `docs/business_process/05_goods_receipt_grn.md` | **GRN gap** + proposed (non-committed) target flow |
| 12 | `docs/business_process/06_client_quotation_and_purchase_order.md` | No stock update |
| 13 | `docs/business_process/07_delivery_challan_dpcc.md` | DPCC does not deduct stock |
| 14 | `docs/business_process/08_proforma_invoice_pi.md` | PI does not touch stock |
| 15 | `docs/business_process/09_tax_invoice_ti.md` | **Sales deduction** rule + DPCC-skip defect |
| 16 | `docs/business_process/10_payment_received_ar.md` | No stock movement |
| 17 | `docs/architecture/FLMX_Inventory_Management_Architecture_v1.0.md` | **Target inventory requirements** §2–§18, P01–P11 |
| 18 | `docs/architecture/ADR-002_MultiTenancy_Ownership_Model.md` | Ratified D1–D8, S1, S2, A1-1–A1-3, INV-*, ownership matrix |
| 19 | `docs/architecture/OWNERSHIP_AUDIT.md` | Live `flamex_uat` facts (2026-10-01), stock grain/data anomalies |
| 20 | `docs/architecture/UAT_MIGRATION_GOVERNANCE.md` | Gates 1–11, exception register X-01–X-24, legacy retirement gate |
| 21 | `docs/architecture/GATE2_DEPENDENCY_IMPACT_REPORT.md` | Stock mutation inventory M-01–M-07, reads R-01–R-08, master writes Q-01–Q-09, X-25–X-31, §16 decisions (all Open) |
| 22 | `docs/page-catalog/DOMAIN_masters.md` | 17 master pages; two product catalogs; ImportProducts; orphan editor |
| 23 | `docs/page-catalog/DOMAIN_reports.md` | `Product_stock`, `Service_stock` pages |
| 24 | `docs/page-catalog/DATA_DICTIONARY.md` | Page↔table↔SP verbs for `tbl_stock`, `tbl_NewProduct`, `tbl_NewparentProduct`, legacy pair |
| 25 | `docs/page-catalog/SHARED_SCHEMA.md` | Stock/product schema facts, SP list, "Stock / product ops" out-of-caller list |
| 26 | `docs/page-catalog/UAT_CATALOG.md` | 113-table inventory incl. 14 `tbl_stock*` tables, `tbl_StockAdjustments`, SP signatures |
| 27 | `docs/page-catalog/PAGE_INVENTORY.md` | Page census entries for all product/stock pages |
| 28 | `docs/live_28_catalog_seed_evidence.md` | Permission-key evidence incl. `Search_Products`, `Service_stock`, `uploader` |
| 29 | `docs/14_Authentication_Authorization_Architecture.md` | A-20 Critical: unauthenticated product/stock WebMethods |
| 30 | `docs/22_Security_Baseline.md` | "Invoice / PO / **stock** / scheduler concatenated SQL is historical debt" |
| 31 | `docs/sprint5_hardening_report.md` | Historical parameterisation of product keys |
| 32 | `docs/MILESTONE_v2.1_Security_Foundation_Closure.md` | Records stock concatenated SQL as a **documented exception, not backlog** |
| 33 | `docs/invoice_insert_data_lineage.md` | Invoice→product→stock write trace; DPCC skip |
| 34 | `docs/invoice_export_data_inventory.md` | `tbl_NewProduct` explicitly out of scope of export |
| 35 | `docs/13_Invoice_Search_View_PO_Discovery.md` | Dated discovery snapshot (self-declared superseded by catalogs) |
| 36 | `README.md` | Project overview; no Product/Inventory requirement (term scan clean) |
| 37 | `CONTRIBUTING.md` | Contribution rules only |
| 38 | `Output-Build.txt` | Build output log; no requirements |
| 39 | `Bill_Software/.cursor/rules/ponytail.md` | Agent style rule; control-ID/`txtParentProducts` integrity only |
| 40 | `Bill_Software/.cursor/rules/solution-docs.md` | Doc-update rule only |
| 41 | `Bill_Software/.cursor/rules/project.mdc` | Git/execution guardrails; no Product content |
| 42 | `.github/pull_request_template.md` | PR checklist mentions "stock workflows unchanged" — control, not requirement |

### 3.2 Read for status only (archived / non-commitment)

| Path | Why it is not requirement evidence |
|------|-----------------------------------|
| `docs/architecture/review-archive/2026-11-07/REVIEW_REPORT_FLMX_v1.0.md` | Header: "**UNAPPROVED VALIDATION REPORT — Not Approved Specification**" |
| `docs/architecture/review-archive/2026-11-07/SECTION_1_REVIEW_FLMX_v1.0.md` | Header: "**UNAPPROVED REVIEW MATRIX** … Do not use for implementation decisions or authorization" |
| `docs/architecture/review-archive/2026-11-07/Section1_Validation_Diff.md` | Header: "**ARCHIVE EVIDENCE — Unaccepted Supporting Evidence** … counts and conclusions are NOT validated" |
| `docs/architecture/review-archive/2026-11-07/VERIFICATION_REPORT_FLMX_v1.0.md` | Header: "**UNACCEPTED VERIFICATION REPORT** … Verification incomplete" |

`docs/SOLUTION_INDEX.md` top block ("Solution documentation map — ARCHIVE PROPOSAL (7 November 2026)") governs these: they are "preserved for governance review purposes only and do not constitute approved specifications or implementation directives." They are therefore registered as **category 4/5 evidence (not commitments)**, never as requirements.

### 3.3 Excluded — with reason

| Path / group | Reason for exclusion |
|--------------|----------------------|
| `docs/01`–`docs/13` module files, except `13_` | `SOLUTION_INDEX.md`: "Leftover `docs/01`–`docs/13` are pointers." Term scan found no Product/Inventory content (e.g. `10_Purchase_Order.md` covers the two PO stacks only). |
| `docs/08_Cursor_Change_Audit.md` | Historical **agent-change** audit; "inventory" here means document inventory. Not a module. |
| `docs/11_Communications.md` | "mailer inventory" — homonym verified in context |
| `docs/21_Phase3_Infrastructure_Hardening.md` | "secret inventory"; "stock" appears only in a list of inherited surfaces |
| `docs/36_PR1_Implementation_Readiness.md` | "file inventory" |
| `docs/37_CrossTenant_Duplication_Completion.md` | "file inventory", "production code" |
| `docs/12_Home_Dashboard.md` | No term hits |
| `docs/04_Department_Designation.md`, `docs/05_Customer_Vendor.md`, `docs/06_Sales_Visit_Planner.md`, `docs/07_Sales_Visit_Reporting.md`, `docs/09_Quotation_Generation.md`, `docs/10_Purchase_Order.md`, `docs/15`–`docs/20`, `docs/29`–`docs/35`, `docs/41`–`docs/47`, `docs/ADR-001*`, `docs/MILESTONE*`(see §3.1), `docs/SWITCHUSER_DEPLOYMENT_CHECKLIST.md`, `docs/uat-wave1-pr120-disclosure-uat.md`, `docs/sales-visit-workflow-audit/**`, `docs/page-catalog/SHARED_CONTEXT.md`, `SHARED_RUNTIME.md`, `RECURSIVE_INSTRUCTIONS.md`, `DOMAIN_*` other than `masters`/`reports` | No Product/Inventory requirement content (verified by term scan). `SHARED_RUNTIME.md` was used only indirectly via Gate 2's citations. |
| `Bill_Software/Uploads/**/Log*.txt` | Runtime application logs, not planning documents |
| `Bill_Software/.freebuff/worktrees/**` | Duplicate checkout of the same repository; not authoritative, and inside an agent worktree |
| `packages/`, `obj/`, `bin/`, `.vs/`, `.git/` | Build/vendor/tooling artefacts |
| **No binary/office/PDF documents exist** | `find` for `*.pdf *.docx *.xlsx *.doc *.xls *.pptx *.rdlc *.vsdx *.csv` over the project (excluding `packages/`) returned **zero** files. There is no non-Markdown planning corpus to recover. |
| `db/*.sql`, `Bill_Software/corporate/business/sql/*.sql` | SQL patches/snapshots, not specifications. Gate 2 §1 independently states "`db/*.sql` has no reference to stock, stock adjustments, or either product chain." Not read as requirements. |

**Totals:** 111 `.md` under `docs/` — all scanned; 35 read in depth; 4 archived (status only); remainder excluded above. 4 `.md` outside `docs/` (README, CONTRIBUTING, 2 `.cursor` rules) + 1 `.mdc` + 1 PR template inspected for relevance.

---

## 4. Status classification key

Every register row carries exactly one label:

| Label | Meaning in this report |
|-------|------------------------|
| **(1) DONE** | Documented as implemented/completed, with the document's own status wording and evidence citation |
| **(2) PEND** | Documented and explicitly pending / blocked / deferred / "NOT GRANTED" / open decision |
| **(3) UNK** | Documented, but the documents state the status cannot be determined ("PENDING VERIFICATION", "NOT_VERIFIABLE_FROM_DB", "NOT INDEPENDENTLY CONFIRMED") |
| **(4) SUPERSEDED** | Documented then replaced, retired, or labelled unapproved/archived |
| **(5) AGENT-PROPOSED** | Proposed only by an agent (this audit or a review artefact), never a project commitment |

**Nothing in §6 is category (5).** The register contains only items that a project document states. Where I judged something technically desirable but undocumented, it is listed in §9 as *not* a requirement.

---

## 5. Module register

### 5.1 Product Category (documented name: "Product Catagory")

| ID | Requirement / finding (documented wording preserved) | Status | Evidence |
|----|------------------------------------------------------|--------|----------|
| PC-01 | List, create, delete product category; `SecurePage` + `CompanyID`; blank name rejected ("Provide Products Name."); duplicate is company + trimmed name ("Category already exists for this company."); writes `tbl_SystemNotification` `ModuleCode=CATEGORY` in the same transaction; delete deletes by `id` + `CompanyID`, zero rows rolls back | **(1) DONE** | `business_process/01` §2, §5 (Category), §9; master index §9 row 01 "Implemented"; `page-catalog/DOMAIN_masters.md` "Page behavior" |
| PC-02 | **Open:** "The delete does not query `tbl_NewProduct`. Whether a category in use can be deleted depends on the database. **PENDING VERIFICATION.**" | **(3) UNK** | `business_process/01` §5, §16 |
| PC-03 | Category identity must be **shared** (no tenant partition) per freeze invariant. Current: `CompanyID NOT NULL`, 24 rows all company 1; every filtered path partitions it (contradicts INV-C2) | **(2) PEND** | `ADR-002` §2.1; `INV-C2` §3; `GATE2` §2.2; exception **X-21** |
| PC-04 | INV-C1 pass condition: "One `tbl_NewparentProduct` row per category key. Every `tbl_NewProduct.parentId` resolves to exactly one category." To be tested at Gate 6 | **(2) PEND** | `ADR-002` §3 INV-C1 |
| PC-05 | Legacy category page `productparent` (`tbl_parentProduct`) remains in the menu as "Product Catagory Old"; `tbl_parentProduct` is "not part of the target architecture" | **(4) SUPERSEDED** (retirement gated, not performed) | `page-catalog/DOMAIN_masters.md`; `ADR-002` D6; `UAT_MIGRATION_GOVERNANCE.md` §7 |
| PC-06 | Category ownership across tenants is undecidable on current data: "Category ownership across tenants cannot be observed because company 2 has no category rows. Whether categories are tenant-owned is BUSINESS_DECISION_REQUIRED." | **(3) UNK** | `OWNERSHIP_AUDIT.md` → "Products and categories" |

### 5.2 Product Master

| ID | Requirement / finding | Status | Evidence |
|----|-----------------------|--------|----------|
| PM-01 | Product list/filter/search/create/edit/soft-delete; duplicate check (company + active + (category + normalised name) or `ProductID`); `ProductID` = `PRD` + numeric tail + 1; insert `tbl_NewProduct` then `tbl_stock` then notification then commit; soft-delete sets `DeleteMode=1`, `DeletedOn`, `DeletedByUserId`; images packed `T=\|B=\|L=\|R=\|O=` in `ImageUrl` | **(1) DONE** | `business_process/01` §2, §5 (Product), §9, §15; `DOMAIN_masters.md` |
| PM-02 | `ProductID` unique enterprise-wide; enforced unique constraint present: "1441 rows, 1441 distinct `ProductID`, 0 NULL"; 0 duplicate across companies | **(1) DONE** | `ADR-002` D4, INV-P1; `OWNERSHIP_AUDIT.md` → "Products and categories"; `SHARED_SCHEMA.md` `UQ_tbl_NewProduct_ProductID` |
| PM-03 | "**Product-master edits must not silently modify inventory balances**" | **(2) PEND** — rule documented, implementation contradicts it (product edit overwrites `Quantity` **and** `Quantity_Num`) | `FLMX … v1.0` §10; `business_process/01` §11; `GATE2` Q-01 |
| PM-04 | `tbl_NewProduct.Quantity` and `Quantity_Num` are **not** authoritative physical stock | **(2) PEND** — ratified decision, remediation outstanding (X-04, X-05, X-27; Q-01–Q-09) | `ADR-002` S2, A1-3, INV-Q1; `GATE2` §5.2 |
| PM-05 | Product identity must carry no tenant partition; current column is `CompanyID NOT NULL DEFAULT (1)` | **(2) PEND** | `ADR-002` INV-P2, §2.1; `GATE2` §2.1; X-21 |
| PM-06 | Product identity is **not** the product name. 12 duplicate `NormalizedProductName` groups "are not identity" | **(1) DONE** (documented non-requirement) | `UAT_MIGRATION_GOVERNANCE.md` §4 closing note; `OWNERSHIP_AUDIT.md` |
| PM-07 | Product UOM model: base UOM per product; purchasing/sales UOM via configured conversions; all movements converted to base UOM before posting; fractional precision configurable by product UOM; batch/lot tracking with expiry and serial-number tracking; product tracking configuration selects mode; inactive products need authorization for exceptional transactions with historical movements retained | **(2) PEND** — approved target rules, no repository evidence, no design | `FLMX … v1.0` §4, §10; P02 open |
| PM-08 | Whether one product may use batch **and** serial tracking simultaneously | **(2) PEND** (P02 "Pending; repository does not establish this fact") | `FLMX … v1.0` §4, §15 P02 |
| PM-09 | Services are excluded from inventory: "All tangible products are inventory-managed; services are automatically excluded"; "Services have no physical inventory effect" | **(2) PEND** — business rule documented; observed purchase create does not stock-post service lines, but purchase **delete** runs the stock update for every matching detail row | `FLMX … v1.0` §8, §10; `business_process/02` §11 |
| PM-10 | Bulk product import (Tally XML): `ImportProducts` radio Product vs GST, `btnInsert`/`btnUpdate`/`btnUpsert`/`btn_GstData_Update`, logs under `~/Uploads/Logs/` | **(1) DONE** (exists) but **(2) PEND** remediation: "Import must target (`ProductID`, `StoreID`) and post a signed movement, or stop writing stock"; updates keyed by `ProductName` | `DOMAIN_masters.md`; `GATE2` M-05, R-05, Q-07, X-26 |
| PM-11 | `NewUpdate_product.aspx?Id=` is an orphan product editor under `newproduct_master` permission ("In-app edit is in-grid on `newproduct_master`") | **(3) UNK** — anomaly documented; no retention/removal decision recorded | `DOMAIN_masters.md` → "Two product catalogs" note |
| PM-12 | Read-only product catalog browser `search_products.aspx` with permission key `Search_Products` | **(1) DONE** for the key's catalog presence (final LIVE postflight 114/114); **(3) UNK** for the standalone PR #116 seed transcript | `live_28_catalog_seed_evidence.md`; `HANDOFF.md` → Current Status (LIVE RBAC) |
| PM-13 | Two VAT masters exist and "must not be mixed": `tbl_Vat_Master` (legacy) vs `tbl_New_Vat_Master` (current) | **(1) DONE** (documented quirk/constraint) | `DOMAIN_masters.md` → "Domain quirks" |
| PM-14 | `tbl_HydrantProduct` ownership class undecided; D4 neither includes nor excludes it | **(2) PEND** — X-23 **Open** | `ADR-002` §2.1; `GATE2` §16.2 |

### 5.3 Product Stock / Inventory ledger

| ID | Requirement / finding | Status | Evidence |
|----|-----------------------|--------|----------|
| PS-01 | "`tbl_stock` is the sole authoritative physical-stock ledger" | **(2) PEND** — ratified; on-hand is still read from the master on invoice pages (R-08) and five writers change legacy `Quantity` only | `ADR-002` S1; `GATE2` §4 R-08, §5.1, X-27 |
| PS-02 | "`tbl_stock.Quantity_Num` is the **sole authoritative physical quantity**. `tbl_stock.Quantity` is legacy compatibility data only." | **(2) PEND** — `Quantity` still read by `sp_GetProductStockByStore`, `sp_SearchStockGrouped`, `vw_CategoryStoreStock_Safe`, `Service_stock` | `ADR-002` A1-3, INV-K6; `GATE2` §5.1, §12.2 |
| PS-03 | Stock grain is `ProductID + StoreID`, enforced by a unique constraint (INV-K1) | **(2) PEND** — "Not enforced. Index is non-unique and includes `Quantity`"; one (`Product_id`,`ShippedToStoreId`) group has 2 rows | `ADR-002` INV-K1; `OWNERSHIP_AUDIT.md` → "Stock grain"; X-03 |
| PS-04 | Every physical quantity belongs to a store (D5/INV-K2): zero NULL/blank store ids, every store id resolves to `Stores` | **(2) PEND** — 137 rows blank store; 56 orphan product ids | `ADR-002` INV-K2, INV-K3; `OWNERSHIP_AUDIT.md`; X-01, X-02 |
| PS-05 | Stock carries no tenant partition (INV-K4) | **(1) DONE** — "No tenant column: Confirmed" on `tbl_stock`; no repository path partitions it | `ADR-002` INV-K4; `OWNERSHIP_AUDIT.md`; `GATE2` §2.4 |
| PS-06 | Stock reads/writes must be keyed by (`ProductID`,`StoreID`) and write `Quantity_Num`; `ISNUMERIC(Quantity)=1` guards and name-keyed updates removed | **(2) PEND** | `GATE2` §3 M-02, M-04, M-05, M-06; §4 R-03, R-05, R-06 |
| PS-07 | Stock reporting surface: `Product_stock` (`sp_SearchProductsFast`, `sp_GetProductStockByStore`, `sp_GetProductCategories`) and `Service_stock` (`tbl_stock where Product_id NOT LIKE 'P%'`, unscoped) | **(1) DONE** (pages exist, census "Reviewed"); **(2) PEND** quality: stock-by-store sums legacy `Quantity` grouped by `ShippedToStoreName` | `DOMAIN_reports.md`; `GATE2` R-01, R-02; `UAT_CATALOG.md` SP signatures |
| PS-08 | **Security finding (documented, not a backlog item):** `Product_stock.aspx.cs` `SearchProducts`/`GetStock`/`GetCategories` and `search_products.aspx.cs` `GetProductDetails`/`SearchProducts` are WebMethods with "No session, no CompanyID"; A-20 rated **Critical CONFIRMED**: "Unauthenticated WebMethods expose products/clients/vendors/stock" | **(2) PEND** — finding recorded; no remediation, ticket or decision documented | `docs/14_Authentication_Authorization_Architecture.md` §7.2, A-20 |
| PS-09 | Required inventory reports: "balance, ledger, valuation, reconciliation, aging/slow-moving, transit, quarantine/rejected stock"; availability must use the canonical balance "not legacy quantity columns"; warehouse-level and consolidated company-level views per permissions | **(2) PEND** — "identified as required but may not all be implemented" | `FLMX … v1.0` §13 |
| PS-10 | "Normal issue cannot reduce usable stock below zero without approval"; every transaction increasing a negative balance needs individual approval before posting; "No automatic negative-stock limits or pre-approved allowances" | **(2) PEND** — observed: "Negative stock is not blocked" | `FLMX … v1.0` §9; `business_process/02` §11 |
| PS-11 | Maker-checker: "No self-approval for stock adjustment or negative override" | **(2) PEND** | `FLMX … v1.0` §2, §9, §14 |
| PS-12 | "Posted movements are immutable; corrections use reversal/repost"; "Failed posting must not leave partial inventory changes" | **(2) PEND** | `FLMX … v1.0` §2, §14 |
| PS-13 | "Prevent duplicate posting through unique source references and idempotent posting" | **(2) PEND** | `FLMX … v1.0` §2, §6, §14 |
| PS-14 | Valuation: hybrid approved; FIFO / Weighted Average Cost / Standard Cost are supported candidates; physical quantity and valuation separate; method assignment, effective dates, inter-company costing, landed cost, standard-cost variance, accounting integration remain pending | **(2) PEND** (P01) | `FLMX … v1.0` §11, §15 |
| PS-15 | Audit trail: original/changed values, source, actor, approver, timestamps, reversal references; retained permanently subject to retention policy | **(2) PEND** | `FLMX … v1.0` §14 |
| PS-16 | Stock/product concatenated SQL is **historical debt**, and new statements must still follow the parameterised baseline | **(1) DONE** (documented constraint, explicitly not a backlog item) | `docs/22_Security_Baseline.md`; `MILESTONE_v2.1…` "Documented … decisions. Not unfinished engineering backlog."; `CURSOR_RULES.md` |
| PS-17 | Reconciliation artifacts exist but have no repository writer: `tbl_stock_fix_audit`, `tbl_StockReassignmentAudit`, `tbl_StockReconciliationAudit`, `stock_pid_mapping`, 7 archive tables; "Writers out-of-repo (X-11)" | **(3) UNK** | `GATE2` §7.3, §16.6 rows 16–27; `UAT_CATALOG.md` table list |

### 5.4 Opening Stock

| ID | Requirement / finding | Status | Evidence |
|----|-----------------------|--------|----------|
| OS-01 | Observed: product create inserts `tbl_stock` with `ShippedToStoreId` literal `STR001`, `ShippedToStoreName` literal `Central Warehouse`, `Quantity` only, **no** `Quantity_Num`, **no** `CompanyID` | **(1) DONE** (as-is behaviour) | `business_process/01` §8, §11; `GATE2` M-06 |
| OS-02 | Approved rule contradicts the implementation: "**Product creation does not independently create physical stock.** Opening stock follows an approved opening-balance process." | **(2) PEND** — see contradiction C-05 | `FLMX … v1.0` §5 |
| OS-03 | Opening-balance process: hybrid physical verification; legacy quantities are "reconciliation references, not automatically accepted opening balances"; verification grain is Company + Product + Warehouse; variances investigated and approved; approved balances post through centralized posting; historical movements migrated selectively with legacy records retained; parallel operation with explicit movement ownership, duplicate-post prevention, reconciliation, exception reporting, authorized correction; new system becomes sole posting authority only after reconciliation + approval + cutover | **(2) PEND** — no process, script, owner or date documented; P09, P10 open | `FLMX … v1.0` §12, §15 |
| OS-04 | Opening stock migration is inside the multi-tenancy gate ladder (Gate 3 writes scripts, Gate 5 first runs them on `flamex_uat`) | **(2) PEND** — Gate 2 BLOCKED, Gate 3 not authorized | `UAT_MIGRATION_GOVERNANCE.md` §2, §5 |
| OS-05 | Whether `STR001` equals the store a purchase uses is unresolved: "Whether `STR001` is the same store is **PENDING VERIFICATION**" | **(3) UNK** | `business_process/01` §16 |

### 5.5 Stock Adjustment

| ID | Requirement / finding | Status | Evidence |
|----|-----------------------|--------|----------|
| SA-01 | `tbl_StockAdjustments` records actual physical stock adjustments; **tenant-owned inventory movement** (TRANSACTION_OWNED root, direct `CompanyID`); each adjustment changes the shared `tbl_stock` row for (`ProductID`,`StoreID`) | **(2) PEND** — decision resolved by A1-2; residual: no `StoreID`, no `CompanyID`, 126 rows, 18 orphan products | `ADR-002` A1-2, INV-K7; `GATE2` §6, §13 (X-14 "Resolved (decision A1-2); residual Blocked"), X-15 |
| SA-02 | "Stock adjustments require reason and authorization"; "Every manual stock increase/decrease requires authorization before posting" | **(2) PEND** | `FLMX … v1.0` §5, §9; `business_process/01`? (n/a — see GATE2 §6 for columns `Reason`, `Applied`, `AppliedAt`) |
| SA-03 | `Applied`/`AppliedAt` semantics undefined; applied rows must not be re-posted; writer is outside the repository | **(3) UNK** — "The writer and the process that sets `Applied` / `AppliedAt` are outside the repository" | `GATE2` §6 |
| SA-04 | Roles approving negative stock, adjustments, reversals and transfer closure | **(2) PEND** (P11) | `FLMX … v1.0` §15 P11 |

### 5.6 Goods Receipt / GRN (and "Purchase Receipt")

| ID | Requirement / finding | Status | Evidence |
|----|-----------------------|--------|----------|
| GR-01 | **No GRN exists.** "No page named GRN, goods receipt, or material receipt was in the required file list"; "No GRN table, status, or procedure name appears in the reviewed PO or purchase code-behind"; module status **Gap**; create/edit/view/search/delete/approve-GRN, quality/warehouse allocation screen — all "Not identified" | **(2) PEND (documented gap)** | `business_process/05` §2, §3; master index §9 row 05 "Gap" |
| GR-02 | The flow in `business_process/05` §6 is explicitly *not* a commitment: "The following is a recommended future process. It is not confirmed behaviour"; "The diagram in section 6 is a target. It is not a description of a page that exists — **Proposed only**" | **(4) NOT A COMMITMENT** (recommendation inside a project doc) | `business_process/05` §2 table row 6, §6 |
| GR-03 | Approved GRN rules (target): must reference an existing PO; multiple partial GRNs; stock increases by **actual accepted** quantity; over-receipt requires authorized approval with evidence (PO/line, ordered qty, previously received qty, additional qty, requester, approver, decision, timestamp); GRN against closed/cancelled PO blocked; damaged → quarantine excluded from usable stock; rejected recorded separately; direct purchase keeps equivalent treatment; duplicate receipt posting prevented; posted GRNs not silently editable (correction by auditable reversal/adjustment) | **(2) PEND** — approved business rules; implementation NOT GRANTED | `FLMX … v1.0` §6 |
| GR-04 | "PO release is **not** goods receipt"; `sp_ReleasePO_Final` contains no stock statement (body verified as UAT module) | **(1) DONE** (documented constraint, verified in module body) | `business_process/04` §1, §12; `FLMX … v1.0` §5, §6; `GATE2` §12.2 |
| GR-05 | Direct purchase posts stock immediately on save and "remains a separate receipt route"; "A future GRN must not be documented as a replacement until a design says so" | **(1) DONE** (implemented) + constraint on future work | `business_process/02` §1; `business_process/05` §7; `FLMX … v1.0` §5 |
| GR-06 | Open GRN requirements (explicitly "open requirements, not designed fields"): partial receipt per PO line; over-/short-receipt; damaged/rejected quantity; warehouse or store allocation; PO cancellation after release; purchase returns; "A stock ledger that stays aligned with `tbl_stock` and `tbl_NewProduct`"; accounting integration | **(2) PEND** | `business_process/05` §9 |
| GR-07 | Approver roles for over-receipt (P05), quarantine release authorization (P06), PO closure with unreceived quantities (P07) | **(2) PEND** | `FLMX … v1.0` §6, §15 |
| GR-08 | "Do not add status values such as Received, QC Hold, or Posted until a specification names them. Do not invent a GRN header or detail table." | **(1) DONE** (explicit prohibition on the record) | `business_process/05` §10 |

### 5.7 Warehouse / Store

| ID | Requirement / finding | Status | Evidence |
|----|-----------------------|--------|----------|
| WH-01 | Store identity is shared; `Stores` has no `CompanyID`; 3 rows | **(1) DONE** (tenancy fact) | `ADR-002` §2.1, INV-S1; `OWNERSHIP_AUDIT.md` ("`Stores` has 3 rows and no `CompanyID`") |
| WH-02 | Two store keys are in use — `Stores.StoreId` (varchar, on stock/purchase paths) and `Stores.Id` (int, on PO snapshot); `StoreId` has no unique constraint | **(2) PEND** — X-30 "DEPENDENCY_VALIDATION_REQUIRED (live uniqueness), then Gate 3 design" | `GATE2` §2.3, X-30 |
| WH-03 | Company-exclusive warehouse ownership + `CompanyStore` mapping: "Each physical warehouse belongs exclusively to one company. A company cannot use another company's warehouse"; "CompanyStore maps a company to its authorized warehouses"; inventory balance by Company + Product + Warehouse | **(2) PEND** — approved target logic that **requires formal reconciliation** with ADR-002's shared-store invariants; see C-02/C-03 | `FLMX … v1.0` §2, §3, §13, §16 |
| WH-04 | ADR-002 S1/D5 side: every physical quantity belongs to a Store; store identity shared; stock grain `ProductID + StoreID` | **(1) DONE** (ratified decision) | `ADR-002` D5, INV-S1, INV-K1 |
| WH-05 | **No warehouse/store master CRUD page is documented.** `DOMAIN_masters.md` lists 17 master pages, none for `Stores`; `DATA_DICTIONARY.md` shows `Stores` only as `SELECT` from `Purches_exting_vendor`, `EditPurchase`, and print. Gate 2: store reads are unfiltered; store is a dropdown (`DDL_ShippedTo`) whose "text" is not "a warehouse master" | **(2) PEND / MISSING SPEC** | `DOMAIN_masters.md`; `DATA_DICTIONARY.md`; `business_process/02` §4; `GATE2` §2.3 |
| WH-06 | One `ShippedToStoreId` carries more than one distinct `ShippedToStoreName` | **(2) PEND** — X-17 DATA_REPAIR_REQUIRED; "Choosing a name by frequency" not admissible | `OWNERSHIP_AUDIT.md`; `UAT_MIGRATION_GOVERNANCE.md` §4 |

### 5.8 Stock Transfer

| ID | Requirement / finding | Status | Evidence |
|----|-----------------------|--------|----------|
| TR-01 | Transfers supported: two-step transfers maintain stock-in-transit; partial receipts and multiple receipts per transfer; shortages investigated first, unresolved → authorized adjustment; damaged/rejected → quarantine excluded from usable stock; closure requires authorized approval; source/destination ownership history preserved; dispatch, receipt, discrepancy, adjustment and closure auditable; instant transfer posts source OUT and destination IN atomically with company-wide net change zero; all transfers traceable and auditable | **(2) PEND** — approved logic; no page, table, procedure or design documented | `FLMX … v1.0` §2, §3, §5, §7 |
| TR-02 | Inter-company transfers are permitted and "change ownership; it is not an ordinary warehouse transfer" | **(2) PEND** — P03 (ownership-change point: dispatch or destination receipt) and P04 (documents/approval workflow) both **Pending** | `FLMX … v1.0` §3, §7, §15 |
| TR-03 | **No transfer object exists in the schema inventory.** `UAT_CATALOG.md` lists 113 tables including 14 `tbl_stock*` variants and `tbl_StockAdjustments`, but no transfer/in-transit table; Gate 2's stock-mutation inventory (M-01–M-07) contains no transfer writer | **MISSING SPEC** (documented requirement, no implementation artefact) | `UAT_CATALOG.md` table index; `GATE2` §3 |

### 5.9 Sales deduction (Tax Invoice)

| ID | Requirement / finding | Status | Evidence |
|----|-----------------------|--------|----------|
| SD-01 | Business rule: "every Tax Invoice deducts the invoiced quantity from stock, whether the source is a quotation, a proforma, a manual invoice, or a delivery challan (DPCC)" | **(1) DONE** (rule documented and ratified as target) / **(2) PEND** (implementation) | master index §1, §7; `business_process/09` §1; `FLMX … v1.0` §8 |
| SD-02 | **Documented defect, not an approved exception:** `Add_invoice.Button1_Click` "subtracts `tbl_NewProduct.Quantity` only when `docType != "Delivery Challan"`. That skip is an implementation defect." Module status "Partially Implemented" | **(2) PEND** — defect recorded with no fix authorisation, ticket or date anywhere | `business_process/09` §1, §15; master index §9 row 09; `invoice_insert_data_lineage.md` §… (flow: "Skip stock") |
| SD-03 | Observed deduction writes the **master**, not the ledger: `UPDATE tbl_NewProduct SET Quantity = … WHERE ProductID = @TruePID AND CompanyID = @CompanyID`; `Quantity_Num` and `tbl_stock` are not in the statement; no `Quantity >= @Qty` predicate; rows affected not checked | **(2) PEND** — contradicts S1/S2/D5/INV-K5 | `business_process/09` §9; `GATE2` §3 (sales row), X-25 |
| SD-04 | "Inventory deduction occurs only when the Tax Invoice is finally posted/confirmed"; deduction linked to invoice **and invoice line**; user selects issuing warehouse from company-authorised warehouses; insufficient stock blocked by default subject to approved override | **(2) PEND** — invoice carries no store at all | `FLMX … v1.0` §8; `GATE2` §16.4 ("no sales path captures a store") |
| SD-05 | Editing a posted invoice requires reversing the original movement and posting the corrected one; cancellation reverses the **exact** original movement | **(2) PEND** — "Block and delete do not restore quantity"; invoice delete does not reverse the save-time decrement | `FLMX … v1.0` §8; `business_process/09` §12; `GATE2` §16.4 |
| SD-06 | **Unresolved decision (Open):** which store a sale issues from; issue event (invoice vs DPCC vs first); document scope (manual / proforma-sourced / quotation-sourced; services); reversal on delete and on `status2='Block'`; negative stock allowance; whether historical invoiced quantities must be posted to `tbl_stock` | **(2) PEND** — "X-25 Open … The invoice `StoreID` business rule is **unresolved**: the repository does not establish it" | `GATE2` §16.4, §13.1 X-25 |
| SD-07 | Invoice `StoreID` may not default to `STR001`, "Central Warehouse", the product's only stock row, the largest-stock store, a quantity split, or the item's purchase store | **(1) DONE** (explicit prohibition) | `GATE2` §13.1, §16.4 |
| SD-08 | Quotation, PI, DPCC save and payment must **not** deduct stock | **(1) DONE** (verified behaviour, documented) | `business_process/06` §1, `07` §1, `08` §1, `10` §1, `10` §12; master index §7 |

### 5.10 Returns

| ID | Requirement / finding | Status | Evidence |
|----|-----------------------|--------|----------|
| RT-01 | "Customer returns support quarantine-first or direct usable-stock receipt according to inspection/condition"; "Invoice cancellation reversal is distinct from physical customer return; prevent double counting" | **(2) PEND** — P08 open ("Customer-return linkage, inspection, and disposition workflow … not documented in repository") | `FLMX … v1.0` §8, §15 |
| RT-02 | Purchase returns listed among open requirements ("These are open requirements, not designed fields") | **(2) PEND** | `business_process/05` §9 |
| RT-03 | **No return table, page or procedure appears in any document.** No `tbl_*Return*` in the `UAT_CATALOG.md` index; Gate 2's mutation inventory has no return path | **MISSING SPEC** | `UAT_CATALOG.md`; `GATE2` §3 |

### 5.11 Reorder level

| ID | Requirement / finding | Status | Evidence |
|----|-----------------------|--------|----------|
| RL-01 | **No reorder level, minimum stock, safety stock, or replenishment concept is documented anywhere.** Term scan for `Reorder` / `Re-order` across all documentation returned **zero** files. `FLMX … v1.0` §13's required-report list (balance, ledger, valuation, reconciliation, aging/slow-moving, transit, quarantine/rejected) does not include it. Nothing in `DOMAIN_reports.md`, `DOMAIN_masters.md`, `ADR-002`, `GATE2`, or `business_process/**` mentions it. `MOQ_Value` exists as a `tbl_NewProduct` column only (product insert list, `business_process/01` §8) with no documented reorder semantics | **NOT DOCUMENTED** | `business_process/01` §8; `FLMX … v1.0` §13; full-corpus term scan |

### 5.12 Legacy product chain retirement

| ID | Requirement / finding | Status | Evidence |
|----|-----------------------|--------|----------|
| LG-01 | `tbl_Product` and `tbl_parentProduct` are **not part of the target architecture**; retirement "happens only after dependency validation (governance Gate 2 and the retirement gate in §7)" | **(2) PEND** | `ADR-002` D6; `UAT_MIGRATION_GOVERNANCE.md` §7 |
| LG-02 | Retirement sequence: Gate 2 lists every reader/writer; Gate 7 removes/redirects dependencies; Gate 8 proves pages; retirement is "a separate, later change with its own UAT proof and its own production approval"; same gate applies to `tbl_NewProduct.Quantity`/`Quantity_Num` and to `CompanyID` on shared Product/Category/reference tables | **(2) PEND** | `UAT_MIGRATION_GOVERNANCE.md` §7 |
| LG-03 | Documented defect: `RequisitionCreate.aspx.cs` reads both legacy tables **filtered by `CompanyID`**, "a column neither table has … → SQL error 207 at runtime"; page also targets missing `tbl_requisition` | **(2) PEND** — defect recorded, no fix plan | `GATE2` §11; `business_process/01`? (n/a) |
| LG-04 | Legacy pair is empty and has no tenant column; `FK_tbl_Product_tbl_parentProduct` remains declared | **(1) DONE** (fact) | `OWNERSHIP_AUDIT.md`; `UAT_MIGRATION_GOVERNANCE.md` X-22 |

### 5.13 Cross-cutting: multi-tenancy ownership of Product/Stock

| ID | Requirement / finding | Status | Evidence |
|----|-----------------------|--------|----------|
| MT-01 | Target classes: `tbl_NewProduct` = SHARED_MASTER (Product identity); `tbl_NewparentProduct` = SHARED_MASTER (Category identity); `Stores` = SHARED_MASTER; `tbl_stock` = SHARED_MASTER (physical-stock ledger); `tbl_StockAdjustments` = TRANSACTION_OWNED | **(1) DONE** (ratified) | `ADR-002` §2.1, §2.2, D8 |
| MT-02 | Implementation sequence is Gates 1–11; Gate 1 complete (2026-10-01); **Gate 2 BLOCKED**; Gate 3 not authorized | **(2) PEND** | `UAT_MIGRATION_GOVERNANCE.md` §2; `HANDOFF.md` → Current Task/Pending Work; `SOLUTION_INDEX.md` |
| MT-03 | Gate 2 blockers on record: SQL Agent job-step text unreadable (`execute_query is disabled`); no `flamex_live` census (X-18); X-19 not re-run; X-20/21/22 unchanged; repository-owner approval absent; X-23/24/25/28/31 open | **(2) PEND** | `GATE2` §15, §16.1; `HANDOFF.md` |
| MT-04 | Current tables keep their `CompanyID` columns and tenant queries keep the `CompanyID` filter until Gate 7 | **(1) DONE** (standing constraint) | `ARCHITECTURE.md` → "Multi-tenant isolation principles" |
| MT-05 | Purchase root lacks a direct `CompanyID`, which is the tenant carrier for every stock-posting transaction (`tbl_Purches`, 163 rows) | **(2) PEND** — X-06 BACKFILL_RULE_REQUIRED; vendor join "not admissible" as the backfill source | `ADR-002` §2.2, D7; `UAT_MIGRATION_GOVERNANCE.md` X-06; `GATE2` §7.1 |
| MT-06 | "D1–D8, S1, S2 are not reopened" and a conflicting observed fact "is a registered exception", not a reason to reopen | **(1) DONE** (governance rule) | `UAT_MIGRATION_GOVERNANCE.md` §1 rule 4; `ADR-002` intro |

### 5.14 RBAC keys on the Product/Stock surface

| ID | Requirement / finding | Status | Evidence |
|----|-----------------------|--------|----------|
| RB-01 | Permission keys exist and are enforced through `SecurePage`: `newproduct_master` (product master + orphan editor), `newproductparent` (category), `product_master` (legacy), `productparent` (legacy), `Service_master`, `product_stock`, `Service_stock` (Bill.Master menu keys), `Search_Products`, `uploader` | **(1) DONE** | `DOMAIN_masters.md` "Gate" column; `DOMAIN_reports.md`; `PAGE_INVENTORY.md`; `ARCHITECTURE.md` |
| RB-02 | `Product_stock` and `Service_stock` are gated only by `Bill.Master` menu key, while their WebMethods are unprotected (PS-08) | **(2) PEND** | `DOMAIN_reports.md`; `docs/14` §7.2 |
| RB-03 | 28-key catalog seed covered `Search_Products`, `Service_stock`, `uploader` with **newly derived** Module/Sub/Feature/Description ("not recovered historical `dbo.Permissions` tuples"); catalog-only, no grants | **(1) DONE** for catalog presence (final LIVE postflight 114/114); **(3) UNK** for the standalone PR #116 execution transcript | `live_28_catalog_seed_evidence.md`; `HANDOFF.md` LIVE RBAC table; `SOLUTION_INDEX.md` |

---

## 6. Contradictions found (recorded, not reconciled)

Per instruction, both sides are cited and neither is silently harmonised. Where a document itself declares precedence, that declaration is quoted.

| ID | Statement A | Statement B | Documented precedence, if any |
|----|-------------|-------------|-------------------------------|
| **C-01** | On-hand authority is **undetermined**: "The authoritative stock balance and the read path that uses it remain **PENDING VERIFICATION**. Nothing below infers a trigger or a sync job." (`business_process/BUSINESS_PROCESS_MASTER_INDEX.md` §7 "Inventory write-path consistency"; repeated in `01` §11, `02` §11, `09` §12) | **Ratified:** "`tbl_stock` is the sole authoritative physical-stock ledger" (S1) and "`tbl_stock.Quantity_Num` is the **sole authoritative physical quantity**" (A1-3) (`ADR-002` "Ratified decisions", "Amendment A1") | `ADR-002` intro: "Observed DB facts are not edited to match the target. Before and after states are both recorded." `FLMX … v1.0` §16: ADR-002 "is ratified and normative. It must not be silently overwritten." → **B is normative for target; A remains the accurate description of current code.** |
| **C-02** | Stock grain: `ProductID + StoreID` (INV-K1, `ADR-002` §3) | Inventory balance maintained by **Company + Product + Warehouse** (`FLMX … v1.0` §2, §3, §13) | `FLMX … v1.0` §3, §16: "ADR-002 reconciliation required … These concepts must be formally reconciled against ADR-002 before architecture ratification or implementation." → **Unresolved.** |
| **C-03** | Store/Warehouse identity is shared and carries no `CompanyID` (INV-S1, INV-K4, `ADR-002` §2.1) | "Each physical warehouse belongs exclusively to one company. A company cannot use another company's warehouse"; `CompanyStore` maps a company to its authorized warehouses (`FLMX … v1.0` §2, §3, §7) | `FLMX … v1.0` §16: "how CompanyStore coexists with ADR-002's shared-store invariants (INV-K2, INV-K4) must be decided." → **Unresolved.** |
| **C-04** | "`tbl_StockAdjustments` … is a **tenant-owned inventory movement** … Each adjustment changes the shared `tbl_stock` row for (`ProductID`, `StoreID`)" (`ADR-002` A1-2, INV-K7) | `UAT_MIGRATION_GOVERNANCE.md` §4 lists **X-14** as "BUSINESS_DECISION_REQUIRED … Whether its rows are physical inventory quantities under D5, and which ownership class it belongs to, **is not decided**" | `UAT_MIGRATION_GOVERNANCE.md` §4 preamble: "Amendment A1 resolves the decision part of X-12, X-14, and X-16"; `GATE2` §13 records X-14 "**Resolved** (decision A1-2)". → **A resolves it; the §4 row is stale by the governance document's own precedence rule ("where they differ, §13 is current").** |
| **C-05** | "**Product creation does not independently create physical stock.** Opening stock follows an approved opening-balance process." (`FLMX … v1.0` §5) | Product create "Inserts `tbl_NewProduct.Quantity` / `Quantity_Num` and `tbl_stock.Quantity` at store `STR001`" (master index §7; `business_process/01` §11; `GATE2` M-06) | None. FLMX §5 records both columns (rule vs repository evidence) without resolving; M-06 remediation is Gate 7. → **Open conflict between approved rule and behaviour.** |
| **C-06** | "All tangible products are inventory-managed; services are automatically excluded" (`FLMX … v1.0` §10) | Purchase **delete** "still runs the stock update for every detail row that matches a product and store" including service lines (`business_process/02` §11); `Service_stock` reads `tbl_stock where Product_id NOT LIKE 'P%'` (`DOMAIN_reports.md`) | None stated. |
| **C-07** | Invoice pages present `tbl_NewProduct.Quantity AS AvailableStock` (R-08, `GATE2` §4) and invoice save decrements it (Q-04/Q-05) | "`tbl_NewProduct.Quantity` and `Quantity_Num` are **not** authoritative physical stock" (S2) and "Availability uses canonical inventory balance, **not legacy quantity columns**" (`FLMX … v1.0` §13) | `GATE2` §5.2/§16.4 target: "Invoice reads → `tbl_stock.Quantity_Num`". Decision X-25 **Open**. → **Unresolved implementation.** |
| **C-08** | `docs/HANDOFF.md` → Pending Work: "Multi-tenancy target (working tree, not published HEAD): … **Those files are untracked, so Gate 1 is not merged at `4918b977`.**" | `git ls-files docs/architecture/` shows `ADR-002_MultiTenancy_Ownership_Model.md`, `UAT_MIGRATION_GOVERNANCE.md`, `GATE2_DEPENDENCY_IMPACT_REPORT.md`, `OWNERSHIP_AUDIT.md`, `FLMX_Inventory_Management_Architecture_v1.0.md` and all four archive files **tracked and clean** at HEAD `bed7dae`; `ADR-002` header states "**Gate 1 … is complete**" | None. **Recorded as-is; the tree state contradicts the HANDOFF sentence and the audit does not resolve which is authoritative.** |
| **C-09** | `SHARED_SCHEMA.md` (8 Sep snapshot) §4: `dbo.UserCompanyAccess` "missing on UAT"; 22 FKs; client 423 / vendor 486 rows | `OWNERSHIP_AUDIT.md` (2026-10-01): table exists with 28 rows; 24 FKs; client 427 / vendor 492; status CONTRADICTED_BY_DB | `OWNERSHIP_AUDIT.md` intro: "Where the live database disagrees with that snapshot, both statements stay visible." `ADR-002` table: "Where the audit disagrees, the audit is the newer fact." |
| **C-10** | `SOLUTION_INDEX.md` top block declares the 7 Nov 2026 FLMX review artefacts archived / "do not constitute approved specifications" | `docs/architecture/review-archive/2026-11-07/*` contain 47 gap findings, 52 classification entries and 13 GAP entries that read like requirements | `SOLUTION_INDEX.md` archive header + each file's own "UNAPPROVED / UNACCEPTED / Not Approved Specification" header. → **C-10 is resolved by the documents themselves; recorded so no future reader mistakes the archive for a backlog.** |
| **C-11** | `business_process/05` §2: "A trigger or unreviewed page could still post stock on PO release — **PENDING VERIFICATION**" | `GATE2` §12.2 (UAT bodies, 22/22): `sp_ReleasePO_Final` "Reads the updated header row … Writes `UPDATE tbl_PO_Header`"; `trg_Block_PO_Update` writes no table | Later evidence (2026-11-05 collection) is narrower than the 2026-11-06 process doc's own caveat. **Both retained.** |

---

## 7. Phase, priority, dependency and status as documented

### 7.1 The only documented sequencing that governs Product/Inventory work

`UAT_MIGRATION_GOVERNANCE.md` §2 gate ladder (strictly ordered; a gate starts only when the previous gate's exit criteria are met **and recorded in `HANDOFF.md`**):

| Gate | Name | Documented status (2026-11-17) |
|-----:|------|-------------------------------|
| 1 | Architecture freeze | **Complete (2026-10-01)** |
| 2 | Repository dependency & migration-impact analysis | **BLOCKED** |
| 3 | UAT migration design | **Not authorized** |
| 4 | UAT backup / checkpoint | Not started |
| 5 | UAT schema / data migration | Not started (scripts are written in Gate 3, first run here) |
| 6 | UAT reconciliation (every ADR-002 invariant returns zero violating rows) | Not started |
| 7 | Application adaptation (stock by `ProductID + StoreID`, no master-quantity authority, direct `CompanyID` on roots) | Not started |
| 8 | Full UAT / regression | Not started |
| 9 | UAT sign-off & migration freeze | Not started |
| 10 | Production migration package | Not started |
| 11 | Separate production approval & execution | Not started |

### 7.2 Documented dependency order **inside** the migration (Gate 2 §14 "Ordering")

1. Close X-11, X-18, X-19 first.
2. Decide the store key (X-30), prove uniqueness, add the unique constraint on `Stores` **before** any FK from stock or adjustments.
3. Repair orphan product ids (X-02, X-08, X-15) **before** FKs to `tbl_NewProduct.ProductID` (type change `varchar(50|100)` → `nvarchar(250)` precedes the FK).
4. Stock repair (X-01, X-03, X-16, X-17) **before** the INV-K1 unique key and before making the store column NOT NULL.
5. `tbl_StockAdjustments`: add `StoreID`/`CompanyID` as NULL → signed sets → prove applied rows are not re-posted → NOT NULL + FKs.
6. Roots without `CompanyID` (`tbl_Purches`, hydrant pair, `tbl_StockAdjustments`): NULL → signed ownership sets → NOT NULL + FK.
7. Roots with `CompanyID` but no FK: add FKs; add per-tenant uniqueness where numbering is per tenant.
8. Invoice children: deterministic invoice reference **after** the X-28 decision.
9. Child equality checks (INV-T3).
10. Shared master / global reference `CompanyID` columns stay through Gate 5; defaults resolved by lookup, not literal constraint name.

### 7.3 Documented priority (project-wide, not Product-specific)

`HANDOFF.md` → Current Status: (1) bug fixing/remediation of already-implemented features → (2) approved new features → (3) regression verification and documentation; "Prioritize work by business impact, defect severity, dependencies, and implementation scope." No Product/Inventory item appears in that ordering.

### 7.4 Explicitly deferred / parked (not Product/Inventory, recorded to prevent misattribution)

Attendance & Leave Wave 2 is **DEFERRED/PARKED**; Attendance rules-engine resilience validation deferred (§`HANDOFF.md`). These carry no Product/Inventory scope. No Product/Inventory wave, sprint or milestone exists to compare against.

---

## 8. Missing information and unresolved decisions (not filled)

### 8.1 Open decisions as the documents record them

`FLMX … v1.0` §15 Pending-Decision Register (all "Pending"; §17: "All pending decisions (P01-P11) remain open until formally resolved by repository owner or governance process"):

| ID | Question | Status |
|----|----------|--------|
| P01 | Valuation method assignment by product/category | Pending; no assignment in repository |
| P02 | Batch **and** serial tracking simultaneously | Pending; repository does not establish it |
| P03 | Inter-company ownership-change point: dispatch or destination receipt | Pending; ADR-002 does not specify |
| P04 | Inter-company transfer documents and approval workflow | Pending; not documented |
| P05 | Exact over-receipt approver roles/evidence | Pending |
| P06 | Quarantine release authorization | Pending |
| P07 | PO closure behaviour with unreceived quantities | Pending |
| P08 | Customer-return linkage, inspection, disposition | Pending |
| P09 | Parallel legacy/new movement ownership and synchronization | Pending |
| P10 | Controlled migration exception approval and go-live criteria | Pending |
| P11 | Roles approving negative stock, adjustments, reversals, transfer closure | Pending |

`GATE2` §16 decision records — all **Open** (none "Resolved"): X-23 (`tbl_HydrantProduct` class), X-24 (departments/designations), **X-25 (invoice stock handling / sale-store rule — "unresolved")**, X-28 (invoice-child linkage, pre-invoice receipts), X-31 (out-of-freeze objects, 24 open rows).

### 8.2 Blocked evidence (status cannot be determined from documentation)

| Blocked item | What the documents say | Consequence for Product/Inventory |
|--------------|------------------------|-----------------------------------|
| X-11 | 22/22 UAT module bodies recorded (2026-11-05); `msdb.dbo.sysjobsteps` "could not be read (`execute_query is disabled`)"; "Do not treat the rejected query as proof that no job touches stock" | **Unknown whether any SQL Agent job writes stock or master quantity** — an "unbounded rollback risk" (Gate 2 §14) |
| X-18 | `flamex_live` census of every ADR-002 §2/§7 object **not run**; "UAT counts are not production counts" | Production row counts for `tbl_stock`, `tbl_StockAdjustments`, invoice children, legacy product pair: **unknown** |
| X-19 | 8 Sep catalog facts (e.g. `CompanyID` on `tbl_Factory`, transaction children, `Stores.StoreId` uniqueness, `tbl_StockAdjustments.Applied` distribution) **not re-verified** | Several ADR-002 matrix cells rest on an unverified snapshot |
| X-30 | `Stores.StoreId` uniqueness unverified live | Stock grain constraint cannot be designed |
| Body-level unknowns | `tbl_NewProduct.Quantity` / `Quantity_Num` SQL column types "**PENDING VERIFICATION**" (`business_process/01` §8); "Whether one product has one stock row per store is PENDING VERIFICATION" (`02` §15) | Cannot state column semantics from documents |

### 8.3 Missing specifications (documented need, no specification exists)

1. **Reorder level / minimum / safety stock** — no document anywhere (§5.11).
2. **Warehouse master** — no CRUD page, no `CompanyStore` table, no authorised-warehouse mapping object in the schema inventory (§5.7 WH-05).
3. **Stock transfer** — approved rules only; no page, table, in-transit object, or workflow (§5.8 TR-03).
4. **GRN** — no page, table, status vocabulary or procedure; §6 of `business_process/05` is explicitly "Proposed only" (§5.6).
5. **Returns** (customer and purchase) — approved principles only, no table/page/procedure (§5.10).
6. **Stock ledger** — required as a report (`FLMX … v1.0` §13) and demanded as alignment with `tbl_stock` (`business_process/05` §9), but no ledger table exists; `tbl_stock` is a balance table, plus 7 archive/audit tables with no repository writer (§5.3 PS-17).
7. **Sale-store rule** — "the repository does not establish it" (X-25) (§5.9 SD-06).
8. **Quarantine / transit holding** — required by `FLMX … v1.0` §7/§8/§13; no object.
9. **UOM conversion / batch-lot-serial model** — approved rules only; no conversion table or tracking object documented (§5.2 PM-07).
10. **Role matrix for stock approvals** — P11 open; no roles named anywhere.

---

## 9. Documented commitments vs. recommendations (kept separate)

### 9.1 Original project commitments (documented, not agent-generated)

Ratified/approved in project documents:

- ADR-002 D1–D8, S1, S2 and Amendment A1-1/A1-2/A1-3 (ratified 2026-10-01).
- ADR-002 §3 invariants INV-P1, INV-P2, INV-C1, INV-C2, INV-S1, INV-K1…INV-K7, INV-T1…INV-T3, INV-M1, INV-M2, INV-G1, INV-Q1.
- The gate sequence Gate 1–11 in `UAT_MIGRATION_GOVERNANCE.md` §2.
- The exception register X-01–X-31 (target classifications and "not admissible" repair sources).
- `FLMX … v1.0` §2–§14 approved business-logic rules (warehouse exclusivity, transaction-effect matrix, GRN rules, transfer rules, invoice deduction rules, negative-stock/maker-checker rules, UOM/tracking rules, valuation candidates, opening-stock process, availability rules, governance rules).
- Business-process facts and the **documented defect** list in `business_process/01`–`10` (2026-11-06).
- Standing constraints: `HANDOFF.md` "Constraints", `CURSOR_RULES.md`, `docs/22` security baseline, `docs/14` A-20 finding.

### 9.2 Recommendations inside project documents, **not** commitments

| Item | Source | Why it is not a commitment |
|------|--------|----------------------------|
| GRN target flow (record goods against PO → quantity/condition check → post accepted → complete) | `business_process/05` §6, §7 | Document says: "recommended future process. It is not confirmed behaviour"; "Proposed only" |
| The 04 → 05 handoff chain ("Released PO -> Goods receipt -> quantity and quality check -> stock posting -> receipt complete") | `business_process/05` §7 | "Only 'Released PO' is implemented… The rest of the chain is proposed." |
| Gate 2 §14 "Ordering" and the "Target mapping" columns | `GATE2` | Design inputs to Gate 3, explicitly "no scripts"; not approved |
| 29 out-of-freeze classifications (rows 1–27, 29) | `GATE2` §16.6 | "Proposed only; the row closes when recorded by ADR-002 amendment" |
| 47 gap findings / 52 classifications / 13 GAP entries | `review-archive/2026-11-07/*` | Headers: "UNAPPROVED", "UNACCEPTED", "NOT validated", "do not use for implementation decisions" |

### 9.3 Proposed by the Agent only — explicitly **not** raised in this audit

To keep the baseline clean, this audit asserts **no** new requirements. The following commonly-suggested Product/Inventory improvements were **not found in any document** and are therefore *not* part of the register: adding a reorder-level field; adding a stock-transfer screen; adding a GRN page; adding a warehouse master; adding a stock ledger table; adding batch/serial support; adding a returns module; fixing the DPCC invoice stock skip; fixing the unauthenticated stock WebMethods. Each is a **candidate for a future, separately authorised decision**, not a recovered requirement. (`docs/business_process/05` §10 and `FLMX … v1.0` §18 both prohibit implementing these without authorisation.)

---

## 10. Source traceability matrix

| Req ID | Source path | Exact location | Recovered wording / evidence |
|--------|-------------|----------------|------------------------------|
| PC-01 | `docs/business_process/01_product_category_and_products.md` | §2 "Sub-modules and pages"; §5 "Category"; §9 "Writes and transactions" | "List, create, delete category"; "Category already exists for this company."; `tbl_SystemNotification` `ModuleCode` `CATEGORY` |
| PC-01 | `docs/business_process/BUSINESS_PROCESS_MASTER_INDEX.md` | §9 "Status matrix", row 01 | "Implemented" |
| PC-02 | `docs/business_process/01_product_category_and_products.md` | §5 "Category" ¶5; §16 "Defects, risks, and open questions" | "The delete does not query `tbl_NewProduct`… **PENDING VERIFICATION.**" |
| PC-03 | `docs/architecture/ADR-002_MultiTenancy_Ownership_Model.md` | §2.1 row `tbl_NewparentProduct`; §3 INV-C2 | SHARED_MASTER (Category identity); "CompanyID NOT NULL; 24 rows, all company 1" |
| PC-04 | same | §3 INV-C1 | "One `tbl_NewparentProduct` row per category key…" |
| PC-05 | `docs/page-catalog/DOMAIN_masters.md` | "Two product catalogs" table; `productparent` row | Legacy `productparent` → `tbl_parentProduct`; menu label "Product Catagory Old" |
| PC-06 | `docs/architecture/OWNERSHIP_AUDIT.md` | "Products and categories" ¶2 | "Whether categories are tenant-owned is BUSINESS_DECISION_REQUIRED." |
| PM-01 | `docs/business_process/01_product_category_and_products.md` | §5 "Product"; §8 "`tbl_NewProduct` insert"; §15 "Image serialization" | insert order, duplicate rule, `PRD` id, `DeleteMode=1`, `T=\|B=\|L=\|R=\|O=` |
| PM-02 | `docs/architecture/OWNERSHIP_AUDIT.md` | "Products and categories" table row 1 | "Constraint present. 1441 rows, 1441 distinct `ProductID`, 0 NULL" |
| PM-03 | `docs/architecture/FLMX_Inventory_Management_Architecture_v1.0.md` | §10 "Product, UOM, and Tracking", row "Product-master edits must not silently modify inventory balances" | approved rule; "Observed implementation may silently modify balances; must be validated" |
| PM-04 | `docs/architecture/ADR-002_MultiTenancy_Ownership_Model.md` | "Ratified decisions" S2; "Amendment A1" A1-3; §3 INV-Q1 | "not authoritative physical stock" |
| PM-05 | `docs/architecture/ADR-002_MultiTenancy_Ownership_Model.md` §2.1 + `docs/architecture/GATE2_DEPENDENCY_IMPACT_REPORT.md` §2.1 | table + "Finding" | "the repository partitions shared Product identity by tenant on every 'filtered / stamped' path" |
| PM-06 | `docs/architecture/UAT_MIGRATION_GOVERNANCE.md` | §4 closing note after X-24 | "Informational, not an exception: 12 duplicate `NormalizedProductName` groups… product names are not identity." |
| PM-07 | `docs/architecture/FLMX_Inventory_Management_Architecture_v1.0.md` | §4 "Logical Inventory Entities"; §10 | base UOM, conversions, fractional precision, batch/lot, serial, tracking configuration, inactive products |
| PM-08 | same | §4 "Batch + serial simultaneously"; §15 P02 | "remains pending (P02)" |
| PM-09 | same | §8 "Services have no physical inventory effect"; §10 row 1 | "services are automatically excluded" |
| PM-10 | `docs/page-catalog/DOMAIN_masters.md` | `ImportProducts` row; "Page behavior" | Tally XML, unscoped category, inserts omit CompanyID, VAT 18%, expiry `2026-03-31` |
| PM-10 | `docs/architecture/GATE2_DEPENDENCY_IMPACT_REPORT.md` | §3 M-05; §4 R-05; §5.2 Q-07 | "Import must target (`ProductID`, `StoreID`)… or stop writing stock" |
| PM-11 | `docs/page-catalog/DOMAIN_masters.md` | "Two product catalogs" bullets | "`NewUpdate_product.aspx?Id=` is an orphan editor" |
| PM-12 | `docs/live_28_catalog_seed_evidence.md` | "Evidence matrix" row `Search_Products` | "Read-only new product catalog browser"; "Newly derived" |
| PM-13 | `docs/page-catalog/DOMAIN_masters.md` | "Domain quirks" | "Two VAT tables … must not be mixed." |
| PM-14 | `docs/architecture/GATE2_DEPENDENCY_IMPACT_REPORT.md` | §16.2 | "D4 neither includes nor excludes it"; status Open |
| PS-01 | `docs/architecture/ADR-002_MultiTenancy_Ownership_Model.md` | "Ratified decisions" S1 | "`tbl_stock` is the sole authoritative physical-stock ledger." |
| PS-02 | same | "Amendment A1" A1-3; §3 INV-K6 | "`tbl_stock.Quantity_Num` is the sole authoritative physical quantity" |
| PS-03 | `docs/architecture/OWNERSHIP_AUDIT.md` + `ADR-002` §3 INV-K1 | "Stock grain" table | "Uniqueness enforced on product + store: Not enforced. Index is non-unique and includes `Quantity`" |
| PS-04 | `docs/architecture/OWNERSHIP_AUDIT.md` | "Stock grain" table rows "Blank store", "Stock product id not in `tbl_NewProduct.ProductID`" | 137 / 56 rows |
| PS-05 | `docs/architecture/ADR-002_MultiTenancy_Ownership_Model.md` INV-K4 + `OWNERSHIP_AUDIT.md` | "No tenant column: Confirmed" | — |
| PS-06 | `docs/architecture/GATE2_DEPENDENCY_IMPACT_REPORT.md` | §3 M-02/M-04/M-05/M-06; §4 R-03/R-05/R-06 | "`Quantity_Num` never changed"; "No store (X-01 source); name key" |
| PS-07 | `docs/page-catalog/DOMAIN_reports.md` | page table + "Page behavior" | `Product_stock` WebMethods; `Service_stock` `Product_id NOT LIKE 'P%'`, "Unscoped" |
| PS-08 | `docs/14_Authentication_Authorization_Architecture.md` | §7.2 table; findings table A-20 | "No session, no CompanyID"; "**Critical** / CONFIRMED" |
| PS-09 | `docs/architecture/FLMX_Inventory_Management_Architecture_v1.0.md` | §13 "Availability and Reporting" | required report list; "may not all be implemented" |
| PS-10 | same §9; `docs/business_process/02_purchase_and_inventory_receipt.md` §11 | "Negative stock is not blocked." |
| PS-11 | same §2, §9, §14 | "Maker-checker applies; no self-approval" |
| PS-12 | same §2, §14 | "Posted movements are immutable; corrections use reversal/repost" |
| PS-13 | same §2, §6, §14 | "Prevent duplicate posting…" |
| PS-14 | same §11 | hybrid valuation; candidates; P01 |
| PS-15 | same §14 | audit-field list |
| PS-16 | `docs/22_Security_Baseline.md`; `docs/MILESTONE_v2.1_Security_Foundation_Closure.md` | "Invoice / PO / stock / scheduler concatenated SQL is historical debt"; "Documented product and operational decisions. Not unfinished engineering backlog." |
| PS-17 | `docs/architecture/GATE2_DEPENDENCY_IMPACT_REPORT.md` | §16.6 rows 16–27 | "No repository reference"; "Writers out-of-repo (X-11)" |
| OS-01 | `docs/business_process/01_product_category_and_products.md` | §8 "`tbl_stock` insert on product create" | "`ShippedToStoreId` is the literal `STR001`. `ShippedToStoreName` is the literal `Central Warehouse`… does not set `Quantity_Num` or `CompanyID`." |
| OS-02 | `docs/architecture/FLMX_Inventory_Management_Architecture_v1.0.md` | §5 "Transaction Effect Matrix", row "Product creation" | "Does not independently create physical stock." |
| OS-03 | same | §12 "Opening Stock and Migration" | nine approved rules incl. "Legacy quantities are reconciliation references" |
| OS-04 | `docs/architecture/UAT_MIGRATION_GOVERNANCE.md` | §2 Gate 3 / Gate 5 | "Scripts are written in Gate 3 and run first in Gate 5, on `flamex_uat` only." |
| OS-05 | `docs/business_process/01_product_category_and_products.md` | §16 bullet 3 | "Whether `STR001` is the same store is **PENDING VERIFICATION**." |
| SA-01 | `docs/architecture/ADR-002_MultiTenancy_Ownership_Model.md` | A1-2; INV-K7; §2.2 row | "tenant-owned inventory movement" |
| SA-02 | `docs/architecture/FLMX_Inventory_Management_Architecture_v1.0.md` | §5 row "Stock adjustment" | "Stock adjustments require reason and authorization." |
| SA-03 | `docs/architecture/GATE2_DEPENDENCY_IMPACT_REPORT.md` | §6 "Already posted to stock?" row | "`Applied` distribution not profiled"; writer outside repository |
| SA-04 | same §15 P11 | — |
| GR-01 | `docs/business_process/05_goods_receipt_grn.md` | §2 "Evidence" table; §3; master index §9 row 05 | "No page named GRN…"; Status "Gap" |
| GR-02 | same | §2 row 6; §6 | "Proposed only"; "recommended future process" |
| GR-03 | `docs/architecture/FLMX_Inventory_Management_Architecture_v1.0.md` | §6 "Purchase and GRN" table | eleven rules incl. over-receipt evidence fields |
| GR-04 | `docs/business_process/04_purchase_requisition_and_po.md` | §1, §12 | "Release does not receive goods and does not post stock." |
| GR-05 | `docs/business_process/05_goods_receipt_grn.md` | §7 "Intended handoff" ¶2 | "A future GRN must not be documented as a replacement until a design says so." |
| GR-06 | same | §9 "Unresolved requirements" | eight bullets |
| GR-07 | `docs/architecture/FLMX_Inventory_Management_Architecture_v1.0.md` | §6 "Pending decisions"; §15 | P05, P06, P07 |
| GR-08 | `docs/business_process/05_goods_receipt_grn.md` | §10 "What must not be assumed" | prohibition list |
| WH-01 | `docs/architecture/ADR-002_MultiTenancy_Ownership_Model.md` §2.1; `OWNERSHIP_AUDIT.md` "Stock grain" | `Stores` row: "None; 3 rows" |
| WH-02 | `docs/architecture/GATE2_DEPENDENCY_IMPACT_REPORT.md` | §2.3 "Store — `Stores`"; X-30 | "Two different store keys are in use" |
| WH-03 | `docs/architecture/FLMX_Inventory_Management_Architecture_v1.0.md` | §2 principles; §3 table; §7; §13; §16 | company-exclusive warehouse; CompanyStore; Company+Product+Warehouse |
| WH-04 | `docs/architecture/ADR-002_MultiTenancy_Ownership_Model.md` | D5; INV-S1; INV-K1 | — |
| WH-05 | `docs/page-catalog/DOMAIN_masters.md`; `docs/architecture/GATE2_DEPENDENCY_IMPACT_REPORT.md` §2.3 | 17-page table (no `Stores` page); "This review does not treat the dropdown text as a warehouse master" (`business_process/02` §4) |
| WH-06 | `docs/architecture/UAT_MIGRATION_GOVERNANCE.md` | §4 X-17 | "1 `ShippedToStoreId` carries more than one distinct `ShippedToStoreName`." |
| TR-01 | `docs/architecture/FLMX_Inventory_Management_Architecture_v1.0.md` | §3; §5 (instant transfer row); §7 | full transfer rule set |
| TR-02 | same | §3 row "Inter-company transfers"; §7; §15 P03/P04 | "changes ownership; it is not an ordinary warehouse transfer" |
| TR-03 | `docs/page-catalog/UAT_CATALOG.md` table index; `GATE2` §3 | 113-table list (no transfer table); M-01–M-07 (no transfer writer) |
| SD-01 | `docs/business_process/BUSINESS_PROCESS_MASTER_INDEX.md` | §1 third bullet; §7 rows | "every Tax Invoice deducts the invoiced quantity from stock, whether the source is a quotation, a proforma, a manual invoice, or a delivery challan (DPCC)" |
| SD-02 | `docs/business_process/09_tax_invoice_ti.md` | §1; §15 bullet 1; master index §9 row 09 | "That skip is an implementation defect. It is not an approved business exception." |
| SD-03 | `docs/business_process/09_tax_invoice_ti.md` | §9 "Stock update" | `UPDATE tbl_NewProduct SET Quantity = …`; "`Quantity_Num` and `tbl_stock` are not in the statement" |
| SD-04 | `docs/architecture/FLMX_Inventory_Management_Architecture_v1.0.md` §8; `GATE2` §16.4 | "no sales path captures a store" |
| SD-05 | `docs/business_process/09_tax_invoice_ti.md` §12; `FLMX` §8 | "Block or delete TI … No stock reversal in the reviewed methods" |
| SD-06 | `docs/architecture/GATE2_DEPENDENCY_IMPACT_REPORT.md` | §16.4 "Decision required" 1–6; §13.1 X-25 | "The invoice `StoreID` business rule is **unresolved**" |
| SD-07 | same | §13.1 X-25 "Not admissible"; §16.4 | prohibition list |
| SD-08 | `docs/business_process/06` §1, `07` §1, `08` §1, `10` §1/§12 | "No stock column is updated. **CONFIRMED IN SOURCE.**" etc. |
| RT-01 | `docs/architecture/FLMX_Inventory_Management_Architecture_v1.0.md` | §8 rows "Customer returns", "Invoice cancellation distinct from customer return"; §15 P08 | — |
| RT-02 | `docs/business_process/05_goods_receipt_grn.md` §9 | "Purchase returns" |
| RT-03 | `docs/page-catalog/UAT_CATALOG.md`; `GATE2` §3 | absent |
| RL-01 | full-documentation term scan | zero matches for `Reorder`/`Re-order`; `MOQ_Value` only in `business_process/01` §8 insert list |
| LG-01 | `docs/architecture/ADR-002_MultiTenancy_Ownership_Model.md` | D6 | "not part of the target architecture" |
| LG-02 | `docs/architecture/UAT_MIGRATION_GOVERNANCE.md` | §7 "Legacy Product-chain retirement gate" | four numbered steps + rule 4 |
| LG-03 | `docs/architecture/GATE2_DEPENDENCY_IMPACT_REPORT.md` | §11 table row `RequisitionCreate.aspx.cs` | "→ SQL error 207 at runtime" |
| LG-04 | `docs/architecture/OWNERSHIP_AUDIT.md` | "Products and categories" ¶3 | "have 0 rows" |
| MT-01 | `docs/architecture/ADR-002_MultiTenancy_Ownership_Model.md` | §2.1, §2.2, D8 | class table |
| MT-02 | `docs/architecture/UAT_MIGRATION_GOVERNANCE.md` §2; `docs/HANDOFF.md` Current Task | gate statuses |
| MT-03 | `docs/architecture/GATE2_DEPENDENCY_IMPACT_REPORT.md` | §15 "Gate 2 exit status"; §16.1 | "Gate 2: not complete." |
| MT-04 | `docs/ARCHITECTURE.md` | "Multi-tenant isolation principles" | "Current tables keep their `CompanyID` columns… until Gate 7." |
| MT-05 | `docs/architecture/UAT_MIGRATION_GOVERNANCE.md` §4 X-06; `GATE2` §7.1 row `tbl_Purches` | "Not admissible: Using the vendor join…" |
| MT-06 | `docs/architecture/UAT_MIGRATION_GOVERNANCE.md` §1 rule 4; `ADR-002` intro | "Decisions stay closed." / "An observed fact that conflicts with this ADR is a **migration exception**." |
| RB-01 | `docs/page-catalog/DOMAIN_masters.md`, `DOMAIN_reports.md`, `PAGE_INVENTORY.md` | "Gate" columns | keys per page |
| RB-02 | `docs/14_Authentication_Authorization_Architecture.md` §7.2 | unprotected WebMethods |
| RB-03 | `docs/live_28_catalog_seed_evidence.md`; `docs/HANDOFF.md` | scope notes; LIVE RBAC table | "All Description / ModuleName / SubModuleName / FeatureName values are **newly derived**"; "#116 standalone seed execution output **Not independently confirmed**" |
| C-01…C-11 | see §6 | both columns cited in-row | — |

---

## 11. Evidence-quality limitations of this audit

1. **Everything here is documentation-derived.** No application code, SQL object, database or build was inspected, by design. Where a document reports code behaviour (`CONFIRMED IN SOURCE`) that claim is reproduced as the document's finding, not re-verified.
2. **Two dates of truth coexist.** `business_process/**` describes code at HEAD `026b36e` (2026-11-06); `GATE2`'s `file:line` citations refer to `0c5cc72` and "were not re-walked" at the 2026-11-05 re-baseline. Nothing in this audit assumes they agree.
3. **No pagination.** Markdown has no page numbers, so traceability is path + heading; PDF/DOCX/XLSX pagination does not exist because **no such files are present** in the repository.
4. **Archived review artefacts were not used as requirements** even where they contain requirement-shaped statements (47 gap findings etc.), because their own headers and `SOLUTION_INDEX.md` disclaim them.
5. **Working tree vs HEAD.** `docs/HANDOFF.md` and `docs/SOLUTION_INDEX.md` carry pre-existing uncommitted edits (Wave-1 attendance wording only). This register was read from the working tree; no Product/Inventory sentence differs between the two versions. One HANDOFF statement is contradicted by tree state (§6 C-08) and was left unresolved.
6. **The `flamex_live` database was not consulted** — and the documentation says a live census has never been run (X-18). Any statement about production inventory data is therefore unavailable, not merely unverified.

---

## 12. Conclusion

- A **substantial, coherent document set** exists for Product Category, Product Master, Product Stock and the surrounding inventory model — but its character is **as-is evidence + a ratified ownership model + a not-yet-authorised target architecture**, not a delivery plan.
- **Product Category and Product Master are the only two areas documented as implemented** (`business_process/01` "Implemented"; master index §9). Everything downstream of them is "Partially Implemented" or "Gap".
- **Inventory authority is decided but not implemented.** The documents ratify `tbl_stock.Quantity_Num` as the sole quantity of record and simultaneously record that current code writes three different quantity columns through separate paths, reads the master as "available stock", and posts sales against the master instead of the ledger.
- **The three largest documented holes in this area are:** no GRN (explicit gap), no warehouse/`CompanyStore` master or stock-transfer object (approved logic, zero artefacts), and no sale-store rule (decision Open). Returns, reorder level and a stock ledger table are entirely absent from the documentation.
- **All productive work is gated.** Gate 2 is BLOCKED on unreadable SQL Agent job steps, a missing `flamex_live` census, an un-re-run 8 Sep snapshot, and absent repository-owner approval; Gate 3 is not authorised; `FLMX … v1.0` states "Implementation Authorization: NOT GRANTED".
- **No Product Catalog/Inventory roadmap, milestone plan, priority ranking, sprint, date or named owner exists.** The finding is reported explicitly rather than filled: the documents inspected for that purpose are `SOLUTION_INDEX.md`, `ARCHITECTURE.md`, `HANDOFF.md`, `CURSOR_RULES.md`, `CURSOR_SESSION.md`, all of `business_process/**`, all of `architecture/**` (including the 4 archived reviews), `page-catalog/**`, `docs/14`, `docs/22`, `docs/13`, `docs/sprint5_hardening_report.md`, `docs/MILESTONE_v2.1_Security_Foundation_Closure.md`, `docs/live_28_catalog_seed_evidence.md`, `docs/invoice_insert_data_lineage.md`, `docs/invoice_export_data_inventory.md`, `README.md`, `CONTRIBUTING.md`, `Output-Build.txt`, and the `.cursor` rule files.
