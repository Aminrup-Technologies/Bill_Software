# Ownership Audit — Multi-Tenancy Dependency Analysis

**Status:** Analysis only. No schema, C#, ASPX/ASCX, stored-procedure, view, trigger, migration, or configuration change is made or authorized by this document.
**Date:** 2026-10-01
**Frozen target architecture:** [`MULTI_TENANCY.md`](MULTI_TENANCY.md) — unchanged by this audit. Where this audit observes current-state behavior that differs from the frozen model, that is a **conflict to be decided**, not a license to implement.
**Evidence sources:** `docs/page-catalog/DATA_DICTIONARY.md` (generated reverse index, 188 pages), `docs/page-catalog/UAT_CATALOG.md` (generated, `flamex_uat` snapshot 2026-09-08), `docs/page-catalog/SHARED_SCHEMA.md`, direct C# traces under `Bill_Software/corporate/business/app/` (2026-10-01). Generated catalogs were read, not edited.

**Classification summary:** `SHARED_MASTER` **7** · `TENANT_MASTER` **5** · `TRANSACTION_OWNED` **38** · `GLOBAL_REFERENCE` **10** · `PENDING_VALIDATION` **4** — 64 objects across §4–§6 (legacy chain under `SHARED_MASTER`; §6 transaction headers/details/children under `TRANSACTION_OWNED`; VAT and geography under their recommended `GLOBAL_REFERENCE` targets pending §13-D2/D3).

---

## 1. Executive Summary

- The frozen model's **shared stock rule holds in the schema and in most code paths**: `tbl_stock` has no `CompanyID`; purchase-entry upserts key on `Product_id + ShippedToStoreId` (`EditPurchase`, `Purches_exting_vendor`, `Purches_new_vendor`, `Delete_purtches` reversal). But **legacy/import paths mutate stock by `Product_name` alone** (`ImportProducts`, `Purches_new_vendor` service path), ignoring the store dimension — a data-integrity defect inside the shared domain, not a tenancy violation.
- **Shared-stock conformance holds at the schema level, not the switcher level.** No stock read is scoped by `CompanyContext.CurrentCompanyID` (verified: zero `CompanyID` matches in all `tbl_stock`-touching pages). The switcher currently does not partition stock — conformant by inaction.
- **The largest conflict is Product/Category:** `newproduct_master`, `newproductparent`, and invoice pages already filter `tbl_NewProduct` / `tbl_NewparentProduct` by `CompanyContext.CurrentCompanyID` and generate `PRD` codes per-tenant. The current implementation is **tenant-partitioned product**, which directly conflicts with the frozen shared-master model. Multi-tenant data does not yet exist (UAT rows are ~all `CompanyID=1`), so the conflict is latent, not yet visible in data.
- **Vendor and Client are already de-facto tenant-owned masters** (schema `CompanyID NOT NULL DEFAULT 1`; full CRUD/list/typeahead scoped by `CompanyContext`; `AuthGuard.VendorInCurrentCompany` / `ClientInCurrentCompany`; vendor code generator scoped per company). Their ownership question is therefore a ratification decision, not a remediation.
- **The purchase stack is the widest transaction-ownership gap:** `tbl_Purches`, `tbl_purches_details`, `tbl_purches_due`, `tbl_Purchess_payment`, `tbl_PO_PartySnapshot`, `tbl_invoice_due`, `tbl_invoice_payment`, `tbl_invoice_payment_tds` have **no `CompanyID`**; tenant is derived through `tbl_Vendor.CompanyID` joins in `AuthGuard`/print paths, and several pages (`Service_stock`, `Payment_due`, `Purchess_due`, `seartch_purtch`) are unscoped.
- **The frozen demotion of product-master quantity is contradicted by live code:** `Manual_Invoice` and `Add_invoice` decrement `tbl_NewProduct.Quantity` on sale, but `Delete_invoice` no longer restores it (restoration SQL is commented out) — the master balance is authoritative in write paths yet non-reconcilable on reversal.
- **4 master/reference objects could not be classified from repository evidence** (`tbl_HydrantProduct`, `tbl_Tieup_Company`, `tbl_Departments`, `tbl_Designations` — §5.12); §12 additionally records 12 validation unknowns — chiefly SP bodies (not in repo), trigger surface, `tbl_PO_PartySnapshot` provenance, and the UAT-vs-live split for several masters.
- **No implementation may begin** until the §13 decisions are resolved; the sequencing prerequisites are in §14.

## 2. Frozen Architecture Reference

Normative text lives in [`MULTI_TENANCY.md`](MULTI_TENANCY.md). Binding points used by this audit:

| Ref | Rule |
|-----|------|
| §1 | Tenant determines transaction ownership, not shared master or physical-inventory identity. Same `ProductID` / `StoreId` may be transacted by both tenants. Transactions persist `CompanyID` directly. |
| §2 | Flame-Ex = Tenant 1 (`ID=1`, `COMP01`, `FE`); AA Associates = Tenant 2 (`ID=2`, `COMP02`, `AA`). |
| §3.1 | Shared: Product Category, Product, Store, Stock. No `CompanyID` predicate; no per-tenant duplication. |
| §3.2 | Tenant-owned: Purchase, Sales, Invoice, Payment, Due, PR/PO, Challan, Proforma, hydrant — `CompanyID` persisted directly. |
| §4 | `CompanyContext.CurrentCompanyID` is transaction/business context; it must not partition shared domains. |
| §5 | Shared stock = `ProductID + StoreId → Quantity` (`tbl_stock.Product_id` + `tbl_stock.ShippedToStoreId` + `Quantity_Num`). No `CompanyID` in `tbl_stock`. |
| §6 | `tbl_NewProduct.Quantity` / `Quantity_Num` are **not** the authoritative balance; `tbl_stock` is. `vw_StockReconciliation` is diagnostic. |
| §7 | Freeze: no `CompanyID` added to `tbl_stock` / product masters / `Stores`; no per-tenant duplication; transactions persist `CompanyID` directly; exceptions only via explicit superseding decision. |

## 3. Ownership Classification Rules

Applied in order; the first rule whose evidence is conclusive wins.

| # | Rule | Test |
|---|------|------|
| R1 | Transaction row | Is the table a header/detail/child of a business transaction? → `TRANSACTION_OWNED` (must carry `CompanyID` directly). |
| R2 | Physical inventory | Is the row a physical fact (what exists, where)? → `SHARED_MASTER` (stock domain); no tenant column. |
| R3 | Enterprise identity | Would a per-tenant copy create a duplicate real-world identity (same SKU, same physical store)? → `SHARED_MASTER`. |
| R4 | Party/actor owned per tenant | Is the record a business relationship (vendor, customer, employee actor) whose identity, code sequence, and lifecycle belong to one tenant's books — i.e., transacting the same real-world party from two tenants would require separate ledger identities? → `TENANT_MASTER`. |
| R5 | Common static data | Shared constants (payment phases, terms text, lookup lists) identical for all tenants → `GLOBAL_REFERENCE`; no tenant column. |
| R6 | Insufficient evidence | CRUD/transaction reference cannot be traced in repo or schema is ambiguous → `PENDING_VALIDATION`. |

Column presence is **not** a classifier (per constraint): `tbl_NewProduct` has `CompanyID` but R3 says shared; conversely `tbl_Quotaion_details` has `CompanyID` and R1 says transaction-owned — both classifications ignore the column and cite behavior instead.

Per-classification record format: **Current state** / **Evidence** / **Target classification** / **Required change** / **Confidence**.

## 4. Complete Master/Reference Inventory

Method: every master/reference table discoverable from the generated reverse index (`DATA_DICTIONARY.md`), the UAT catalog (111 tables), and direct C# traces. Unsupported requested categories are listed at the end. Kiosk tables (`tbl_card_login`, `tbl_employee`) and security tables (`Roles`, `Permissions`, `UserRoles`, `UserCompanyAccess`, `ActiveSessions`, `PasswordResetTokens`) are out of scope for ownership (identity/auth surface, not business masters) — noted, not classified.

| Object | Role | Has `CompanyID` (UAT) | Referenced by transactions | Classification (target) |
|--------|------|:---------------------:|----------------------------|--------------------------|
| `tbl_ProductCatagory` | Legacy product category | Yes | Not referenced by live tx pages (only `tbl_Product` era) | `SHARED_MASTER` (legacy) |
| `tbl_parentProduct` | Legacy product category (old chain) | No | `product_master`, `RequisitionCreate` (legacy PR) | `SHARED_MASTER` (legacy) |
| `tbl_NewparentProduct` | Product Category (current) | Yes | Quotation, PO, PR, invoice, purchase, stock pages | `SHARED_MASTER` |
| `tbl_Product` | Legacy product | No | `product_master`, `Service_update`, legacy PR | `SHARED_MASTER` (legacy) |
| `tbl_NewProduct` | Product (current) | Yes | Quotation, PO, PR, invoice, purchase, stock pages | `SHARED_MASTER` |
| `Stores` | Store / warehouse | **No** | Purchase entry (`EditPurchase`, `Purches_*_vendor`, `purches_bill`), PO preview | `SHARED_MASTER` |
| `tbl_stock` | Physical inventory | **No** | Every purchase/sale/import/reversal path | `SHARED_MASTER` |
| `tbl_Vendor` | Vendor / supplier ("Principle") | Yes | Purchase, purchase payment, PO, PR, `Purchess_due` | `TENANT_MASTER` (pending §13-D1 ratification) |
| `tbl_Client` | Customer / client | Yes | Quotation, Challan, Proforma, Invoice, payment, due, hydrant | `TENANT_MASTER` (pending §13-D1 ratification) |
| `tbl_Factory` | Client's factory (ship-to site) | Yes | Challan, invoice, hydrant invoice | `TENANT_MASTER` |
| `tbl_ClientRegAddress` | Client registered address | Yes | Challan, invoice | `TENANT_MASTER` (child of Client) |
| `tbl_representative` | Client-side representative | Yes | Invoice/proforma/payment mailers, quotation terms, challan print | `TENANT_MASTER` |
| `tbl_InvSiteAddress` / `tbl_QutSiteAddress` / `tbl_ChaSiteAddress` | Per-document site address | 3 / 2 / **0** | Invoice / quotation / challan | `TRANSACTION_OWNED` (snapshot children) |
| `tbl_Vat_Master` | GST/VAT rate (legacy chain) | Yes | Product pages, purchase, PR | `GLOBAL_REFERENCE` (pending §13-D2) |
| `tbl_New_Vat_Master` | GST rate (current chain) | Yes | `newproduct_master`, `NewUpdate_product` | `GLOBAL_REFERENCE` (pending §13-D2) |
| `tbl_Service_master` | Service tax master | No | Service pages, purchase entry | `GLOBAL_REFERENCE` (pending §13-D2 confirm) |
| `tbl_Industry` | Industry lookup | No | Client CRUD (`New_client`, `Update_client`) | `GLOBAL_REFERENCE` |
| `tbl_State` | State master | Yes | Vendor/Client/Factory CRUD, City master | `GLOBAL_REFERENCE` (pending §13-D3) |
| `tbl_City` | City master | Yes | Vendor/Client/Factory CRUD | `GLOBAL_REFERENCE` (pending §13-D3) |
| `tbl_PaymentPhase` | Payment phase lookup | **No** | Quotation, PO, invoice payment pages | `GLOBAL_REFERENCE` |
| `tbl_PrimaryService` | Primary service lookup | **No** | Quotation (service), `PrimaryServiceTerms` | `GLOBAL_REFERENCE` |
| `tbl_PrimaryServiceTerms` | Terms text lookup | **No** | Quotation, PO, print | `GLOBAL_REFERENCE` |
| `tbl_Expences` | GL expense heads | **No** | `general_expences`, `patty_cash_expences` | `GLOBAL_REFERENCE` (heads list) |
| `tbl_HydrantProduct` | Hydrant product master | **No** | Hydrant quotation, `HydrantProduct` page | `PENDING_VALIDATION` |
| `tbl_Departments` / `tbl_Designations` | Org lookups for users | 1 / 1 | `AddUser`, `ViewUser`, `home` | `PENDING_VALIDATION` (identity surface) |
| `tbl_Tieup_Company` | Tie-up company lookup | **No** | GL/petty expense pages | `PENDING_VALIDATION` |
| `tbl_Company` | The tenant registry itself | n/a | Switcher, print letterhead, PO preview | Out of scope (it *is* the tenant dimension) |

Unsupported / not found in repository (explicitly): **Unit/UOM master** (only `tbl_NewProduct.[Unit]` column), **Brand master** (only `tbl_NewProduct.Brand` column), **Product type master** (only `tbl_NewProduct.Type` / `ProductOrServiceCat`), **bank/account masters** (bank fields are inline on `tbl_Company` and payment rows: `Ch_bank`), **transport/carrier master** (transport amounts are inline on `tbl_Purches`), **payment-mode master** (mode is free-text `type`/`Narration` on payments), **separate warehouse master** (`Stores` is the only location master), **ledger master** (none). These are column values or free text today, not tables — no ownership classification applies until such tables exist.

## 5. Master Ownership Matrix

Each row: Current state → Target classification with the five required distinctions.

### 5.1 `tbl_NewparentProduct` — Product Category (current)

- **Current state:** Schema has `CompanyID int NOT NULL DEFAULT (1)` (unapplied in-repo patch `tbl_NewparentProduct_CompanyID.sql` exists for older copies). CRUD and dropdown reads filter `WHERE CompanyID = @CompanyID`.
- **Evidence:** `newproductparent.aspx.cs` lines 32, 48, 116–120, 170, 186 (list/dup-check/insert/update/delete all `@CompanyID = CompanyContext.CurrentCompanyID`); `newproduct_master.aspx.cs` line 257 (category combo scoped); `ImportProducts.aspx.cs` narrative in `DOMAIN_masters.md` ("Category combo is unscoped; inserts omit CompanyID" — importer is the exception).
- **Target classification:** `SHARED_MASTER` (frozen §3.1).
- **Required change:** Remove per-tenant filtering from category CRUD/combo reads; decide deprecation of the two in-repo CompanyID DDL patches; `ImportProducts` behavior under shared model must be defined (§13-D4).
- **Confidence:** High on current-state code; the *target* is frozen policy, not a discovery.

### 5.2 `tbl_NewProduct` — Product (current)

- **Current state:** `CompanyID int NOT NULL DEFAULT (1)`; unique business key `ProductID` (`UQ_tbl_NewProduct_ProductID`). CRUD (insert/update/grid/duplicate-check/soft-delete) filters by `@CompanyID`; invoice paths join `np.CompanyID = @CompanyID`; `PRD###` codes generated per company.
- **Evidence:** `newproduct_master.aspx.cs` lines 257, 291, 342, 366, 602, 726–739, 792, 877, 989, 1038, 1181, 1200, 1265; `Add_invoice.aspx.cs` 661/675/691 (`np.ProductID = qd.Product_Code AND np.CompanyID = @CompanyID`); `Manual_Invoice.aspx.cs` 167.
- **Target classification:** `SHARED_MASTER` — same SKU must be one row for both tenants (frozen §3.1; duplicate would double-count catalog + stock identity).
- **Required change:** Remove `CompanyID` predicates from product CRUD/combo/joins; redesign `PRD` code generation to enterprise-wide uniqueness (currently per-tenant-scoped `MAX(ProductID)` would collide); define the frozen model's answer to per-tenant product metadata (none exists — any such need is a new decision).
- **Confidence:** High (current state); target is frozen policy.

### 5.3 `Stores` — Store / warehouse

- **Current state:** Schema has **no `CompanyID`** (14 columns, none tenant). Referenced by purchase entry (`EditPurchase`, both `Purches_*_vendor` pages, `purches_bill` print, `Generate_PO_Preview`). No CRUD page exists — rows are provisioned outside the app (UAT store `STR001` is hard-coded in `newproduct_master` opening-stock insert).
- **Evidence:** `UAT_CATALOG.md` §`dbo.Stores` (columns 1–14, no CompanyID); `DATA_DICTIONARY.md` reverse index (`Stores` → 5 callers, SELECT/REF only); `newproduct_master.aspx.cs` line 765 (`@ShippedToStoreId` = `"STR001"`).
- **Target classification:** `SHARED_MASTER` (frozen §3.1). Conformant today at schema and code level.
- **Required change:** None for tenancy. Gap: no CRUD surface means store creation is unmanaged (§12).
- **Confidence:** High.

### 5.4 `tbl_stock` — Physical inventory

- **Current state:** No `CompanyID` (columns §7). Keyed de-facto by `Product_id + ShippedToStoreId` in modern paths; `Quantity` varchar + `Quantity_Num` decimal. Store dimension is nullable, and legacy paths ignore it.
- **Evidence:** `UAT_CATALOG.md` §`dbo.tbl_stock`; mutation traces in §7.
- **Target classification:** `SHARED_MASTER` (frozen §5: `ProductID + StoreId → Quantity`).
- **Required change:** None for tenancy (conformant). Integrity remediation (store-dimension enforcement) is gated on §13-D5.
- **Confidence:** High.

### 5.5 `tbl_Vendor` — Vendor / supplier — **analyzed, not assumed**

- **Current state:** `CompanyID int NOT NULL DEFAULT (1)`; business key `Vendor_Id` (`AA09` style, unique in data). All vendor CRUD, lists, and typeahead are scoped by `CompanyContext.CurrentCompanyID` (`New_vendor` insert + `findcompanyId` `MAX(Vendor_Id)` scoped per company; `View_vendor`, `Update_vendor`, `Delete_vendor` all `@CompanyID`). `AuthGuard.VendorInCurrentCompany` fails closed. Purchase transactions reference `tbl_Purches.Client_Id = tbl_Vendor.Vendor_Id` and derive tenant via `v.CompanyID`.
- **Evidence:** `New_vendor.aspx.cs` 31–32, 151–180; `View_vendor.aspx.cs` (23 CompanyID refs); `Update_vendor.aspx.cs` (10); `Delete_vendor.aspx.cs` (8); `AuthGuard.cs` 430–436 (`VendorInCurrentCompany`), 649–652 (print inference join); `docs/41_CrossTenant_Duplication_Architecture.md` (duplicate-by-company feature design: `WHERE Vendor_Name=@VendorName AND CompanyID=@CompanyID`, code sequence per company).
- **Business-identity test (R4):** A vendor record is a ledger relationship with tenant-issued code (`AA*`), tenant-scoped duplicate-name rules, and tenant-scoped payment/due aggregates (`Purchess_due` joins vendor for tenant). The repository's own v2.1 cross-tenant duplication feature institutionalizes per-tenant vendor copies for the *same real-world principle*. Transacting vendor AA09 from tenant 2 would require tenant-2 ledger identity and code sequence — the same vendor code cannot be shared without breaking the per-tenant numbering scheme. → `TENANT_MASTER`.
- **Frozen-model tension:** The frozen rule (§1) prohibits deriving **transaction** ownership from `Vendor.CompanyID`. That rule is about transactions, not masters; it does not itself decide vendor ownership. But `SHARED_SCHEMA.md` §6 documents `tbl_Purches` "Join `p.Client_Id = v.Vendor_Id` and filter `v.CompanyID`" as the *current* isolation mechanism, which the frozen model outlaws for transactions. Vendor as `TENANT_MASTER` plus direct `CompanyID` on purchase rows satisfies both: vendor ownership is explicit, transaction ownership is direct, and vendor inference is retired.
- **Target classification:** `TENANT_MASTER` — requires ratification (§13-D1) because the frozen doc is silent on parties and the current `CompanyID` column alone was not to be treated as proof.
- **Required change:** Ratify in a superseding note or amend MULTI_TENANCY.md through a new decision; then add direct `CompanyID` to the purchase stack and retire vendor-join inference.
- **Confidence:** High on current state; medium on target (business decision pending).

### 5.6 `tbl_Client` — Customer / client — **analyzed, not assumed**

- **Current state:** `CompanyID int NOT NULL DEFAULT (1)`; business key `Client_Id` unique in data. CRUD/list/typeahead scoped (`New_client` INSERT carries `CompanyID`; `View_client` grid/search/WebMethods all `@CompanyID`; `Update_client`, `Delete_client` scoped). `AuthGuard.ClientInCurrentCompany` fails closed. Sales-side transactions reference `Client_ID` codes; print gate infers tenant via `c.CompanyID` joins.
- **Evidence:** `New_client.aspx.cs` 146–158; `View_client.aspx.cs` 46, 94–96, 133–134, 349; `AuthGuard.cs` 422–428, 653–660; `clientHandlerAdmin.ashx` is the documented exception (typeahead without CompanyID — `SHARED_RUNTIME.md` §1).
- **Business-identity test (R4):** Same reasoning as vendor: tenant-issued `Client_Id`, tenant-scoped duplicate checks, cross-tenant duplication feature (`docs/41`–`44`) explicitly duplicates customers per tenant, invoice numbering and due ledgers are per-tenant. → `TENANT_MASTER`.
- **Frozen-model tension:** identical to vendor's; resolved the same way (direct transaction `CompanyID`, retire client-join inference).
- **Target classification:** `TENANT_MASTER` — ratification pending (§13-D1).
- **Required change:** As vendor; plus fix `clientHandlerAdmin.ashx` unscoped typeahead (pre-existing defect D-* family) and decide hydrant sales scoping (§12).
- **Confidence:** High (current); medium (target decision pending).

### 5.7 `tbl_Factory`, `tbl_ClientRegAddress`, `tbl_representative`

- **Current state:** All three carry `CompanyID NOT NULL DEFAULT (1)` and are CRUD-scoped (Factory CRUD `AddFactory`/`ShowFactory`/`Update_factory`; client address CRUD in client pages; representative CRUD `Representative`/`Show_representative`).
- **Evidence:** `DATA_DICTIONARY.md` reverse index rows; `DOMAIN_customer.md` (factories/representatives as client sub-entities).
- **Business-identity test:** All are sub-entities of a specific client relationship (ship-to factory, registered address, contact person). If Client is tenant-owned, these inherit tenant ownership; a shared factory row would leak one tenant's delivery sites into another's challan/invoice flows.
- **Target classification:** `TENANT_MASTER` (children of Client).
- **Required change:** Follow the Client decision (§13-D1).
- **Confidence:** Medium-high (inherits Client's decision).

### 5.8 `tbl_Vat_Master`, `tbl_New_Vat_Master`, `tbl_Service_master` — tax/rate masters

- **Current state:** `tbl_Vat_Master` and `tbl_New_Vat_Master` have `CompanyID NOT NULL DEFAULT (1)`; `tbl_Service_master` does not (2 columns: `ID`, `Service_tax`). CRUD: `Vat_master.aspx` INSERT/DELETE (no CompanyID filter found in catalog narrative); product pages select the tax dropdowns. Tax rates are snapshot-copied onto transaction lines at write time (`GST`/`Tax_Rate` columns on quotation/invoice details), so rate changes don't rewrite history.
- **Evidence:** `UAT_CATALOG.md` (New_Vat_Master = 2 columns; Vat_Master shows CompanyID col 3); `DOMAIN_masters.md` ("Two VAT tables must not be mixed"; lookup masters "no CompanyID").
- **Business-identity test (R5 vs R4):** A GST rate (e.g., 18%) is identical for both tenants — static reference, not a business relationship. Current `CompanyID` on the VAT tables is a partitioning artifact, not evidence of per-tenant rates. However, per-tenant rate *overrides* would be a pricing policy this audit may not infer.
- **Target classification:** `GLOBAL_REFERENCE` for both VAT tables and `tbl_Service_master`, pending §13-D2 confirmation that no tenant-specific rates are required. Marked lower confidence than lookups because the column exists and its data role is unverified.
- **Required change:** §13-D2 decision; then drop partitioning (or keep as tenant override with explicit rationale) and rely on transaction-line rate snapshots.
- **Confidence:** Medium (data behavior unverified — PENDING_VALIDATION aspects in §12).

### 5.9 `tbl_State`, `tbl_City` — geography

- **Current state:** Both `CompanyID NOT NULL DEFAULT (1)`; `master_State` / `master_city` CRUD is tenant-scoped (dup checks "per company", soft-delete, `@CompanyID` everywhere). City stores `State_Name` as free text, not FK.
- **Evidence:** `master_State.aspx.cs` 32, 57, 75–76, 112; `master_city.aspx.cs` (16 refs); `DOMAIN_masters.md` "Geography (tenant-scoped)".
- **Business-identity test (R3 vs R4):** A city is a physical fact shared by both tenants (like a store). Per-tenant city lists duplicate real-world identity and desync address entry. But the current per-company duplicate-name rules are the *existing business rule*; changing them changes documented behavior (docs/22: "Do not change business rules unless the PR states that change explicitly").
- **Target classification:** `GLOBAL_REFERENCE` (R3), **pending §13-D3** — this is the clearest case where current code conflicts with physical-identity reasoning, and the audit refuses to silently override an existing business rule.
- **Required change:** §13-D3 decision (keep per-tenant geography as a business rule vs unify as shared reference).
- **Confidence:** High (current state); target is a business decision.

### 5.10 Lookup masters — `tbl_Industry`, `tbl_PaymentPhase`, `tbl_PrimaryService`, `tbl_PrimaryServiceTerms`, `tbl_Expences`

- **Current state:** No `CompanyID` columns (UAT schema); CRUD pages do INSERT/DELETE without tenant predicate (`DOMAIN_masters.md`: "no CompanyID"). Referenced by quotation/PO/payment flows as dropdown sources. `tbl_Expences` is the expense-head list (2-column leftover table).
- **Evidence:** `UAT_CATALOG.md` (0 CompanyID on all five); `DOMAIN_masters.md` lookup-masters narrative; reverse index (CRUD pages have no CompanyID refs).
- **Business-identity test (R5):** Payment phases, service names, terms text, industries, expense heads are identical reference data for all tenants. No tenant code sequences. No duplication semantics.
- **Target classification:** `GLOBAL_REFERENCE` (conformant today).
- **Required change:** None for tenancy. Catalog note stands: these pages use string-concat SQL (pre-existing debt, out of scope).
- **Confidence:** High.

### 5.11 Legacy chain — `tbl_Product`, `tbl_parentProduct`, `tbl_ProductCatagory`

- **Current state:** `tbl_Product` has `CompanyID NOT NULL DEFAULT (1)` (column 24 per §3 catalog row: Product shows CompanyID=1 col); `tbl_parentProduct` has no `CompanyID`; `tbl_ProductCatagory` has it. Legacy pages (`product_master`, `productparent`, `Update_product`) are menu-exposed but the commercial spine uses the new chain.
- **Evidence:** `UAT_CATALOG.md`; `DOMAIN_masters.md` two-catalog narrative.
- **Target classification:** `SHARED_MASTER` (legacy) — same SKU identity reasoning; but no live transaction dependency except legacy PR (`RequisitionCreate` → missing `tbl_requisition`).
- **Required change:** Decide retirement of the legacy chain (§13-D6) rather than remediating tenancy on dead surfaces.
- **Confidence:** Medium (usage extent of legacy pages unverified).

### 5.12 `tbl_HydrantProduct`, `tbl_Departments`, `tbl_Designations`, `tbl_Tieup_Company`

- **Current state:** `tbl_HydrantProduct` has no `CompanyID`; CRUD page exists, referenced by hydrant quotation. `tbl_Departments`/`tbl_Designations` have nullable `CompanyID`; used only for user admin. `tbl_Tieup_Company` has no `CompanyID`; used only by GL/petty expense pages.
- **Evidence:** `UAT_CATALOG.md`; reverse index (5 callers for Tieup; department/designation callers are user-admin pages only).
- **Business-identity test:** Insufficient evidence — hydrant products may be tenant-specific offerings or shared; tie-up companies are third-party entities whose ownership is unclear; org lookups' intended ownership is not documented anywhere.
- **Target classification:** `PENDING_VALIDATION` (all four).
- **Required change:** §12 validation items V-4, V-5.
- **Confidence:** n/a (unclassified by design).

## 6. Transaction → Master Dependency Matrix

Transactions and their master/reference dependencies. `CID` = has `CompanyID` on UAT (H = header, D = detail/child). "Direct CID?" asks whether the **frozen model's requirement** (transaction rows carry `CompanyID` directly) is met today.

| Transaction | Tables | Reads/writes masters | Direct CID today? | Tenant inference used today |
|-------------|--------|----------------------|-------------------|------------------------------|
| Purchase | `tbl_Purches` (H) | `tbl_Vendor` (TENANT), `Stores` (SHARED), `tbl_Vat_Master`, `tbl_Service*`, `tbl_NewProduct`/`tbl_Product` (SHARED) | **No** — no CompanyID column | Vendor join (`Client_Id = Vendor_Id → v.CompanyID`) in AuthGuard/print; some pages unscoped |
| Purchase Detail | `tbl_purches_details` (D) | `tbl_NewProduct` (SHARED), `tbl_stock` (SHARED, via `ShippedToLoc`) | **No** | none found |
| Purchase Payment | `tbl_Purchess_payment` (H) | `tbl_Purches` → vendor | **No** | Via purchase → vendor chain |
| Purchase Due | `tbl_purches_due` (D) | `tbl_Purches`, `tbl_Vendor` | **No** | Via purchase → vendor chain |
| Requisition / PR | `tbl_RequisitionMain` (H, CID), `tbl_RequisitionNew` (D, CID) | `tbl_Vendor` (TENANT), `tbl_NewProduct`, `tbl_NewparentProduct`, `tbl_Service`, `tbl_Vat_Master` | **Yes** (both) | None needed; direct |
| Purchase Order / PO | `tbl_PO_Header` (H, CID), `tbl_PO_Items` (D, CID), `tbl_PO_PartySnapshot` (**no CID**), `tbl_PO_PaymentDetails` | `tbl_Vendor`, `tbl_Client`, `tbl_Company`, `Stores`, PR tables | Header+items **Yes**; PartySnapshot **No** | PartySnapshot tenant via PO parent |
| Sales / Quotation (incl. client PO via `RecordType`) | `tbl_Quotation` (H, CID), `tbl_Quotaion_details` (D, CID), `tbl_QutPrimaryService` (CID), `tbl_QutPaymentPhase` (CID), `tbl_QuoPriSerTogather` (CID), `tbl_QuoPserTerm` (CID), `tbl_quotation_vat` (CID), `tbl_QutSiteAddress` (CID) | `tbl_Client` (TENANT), `tbl_NewProduct`, `tbl_NewparentProduct`, `tbl_PaymentPhase`, `tbl_PrimaryServiceTerms`, `tbl_SalesVisitReport` | **Yes** (all children except none missing) | None needed |
| Invoice | `tbl_Invoice` (H, CID), `tbl_Invoice_details` (D, CID), `tbl_InvSiteAddress` (CID) | `tbl_Client`, `tbl_ClientRegAddress` (TENANT), `tbl_Quotation` (upstream), `tbl_Chalan`, `tbl_Proforma`, `tbl_NewProduct` (qty decrement) | **Yes** | None needed; direct |
| Invoice Payment | `tbl_invoice_payment` (H, **no CID**), `tbl_invoice_payment_tds` (D, **no CID**) | `tbl_Quotation` (CID) via `Quotation_No`, `tbl_Client` | **No** | Quotation join (`p.Quotation_No = q.Quotation_no AND q.CompanyID=@CompanyID` — AuthGuard 645–648) |
| Invoice Due | `tbl_invoice_due` (D, **no CID**) | `tbl_Invoice`, `tbl_Quotation` chain | **No** | Via invoice/quotation parent |
| Challan / Delivery | `tbl_Chalan` (H, CID), `tbl_Challan_details` (D, CID), `tbl_ChaSiteAddress` (D, **no CID**) | `tbl_Client`, `tbl_Factory`, `tbl_Quotation` (upstream) | **Yes** (except ChaSiteAddress) | None needed |
| Proforma | `tbl_Proforma` (H, CID), `tbl_Proforma_Details` (D, CID) | `tbl_Client`, `tbl_Quotation` upstream, `tbl_SalesVisitReport` | **Yes** | None needed |
| Stock adjustment / inventory movement | `tbl_stock` (SHARED) + purchase/sale/reversal flows | `tbl_NewProduct` (opening qty), `Stores` | n/a — shared by design | Must never be tenant-scoped (frozen §5) |
| Sales visit (transaction-like) | `tbl_SalesVisitReport` (CID), `tbl_Expenses` (**no CID**), `tbl_SalesVisitResponses` (**no CID**) | `tbl_login` (user actor) | Visit **Yes**; expenses/responses **No** — scoped via visit CompanyID (`AuthGuard` expense check) | Visit-owned children infer via `VisitId` |
| GL expenses (payments made) | `tlb_General_expences` (CID), `tbl_patty_cash_expenses` (**no CID**), `tlb_closing_balance` (**no CID**) | `tbl_Expences` heads (GLOBAL), `tbl_Tieup_Company` (PENDING) | General **Yes**; petty cash/closing **No** — `general_expences` pages unscoped | none found |
| Hydrant quotation | `tbl_qsHydrentQuotation` (**no CID**), `tbl_qsHydrentDetails` (**no CID**) | `tbl_Client` (TENANT), `tbl_HydrantProduct` (PENDING), `tbl_Service`, `tbl_Quotation` (writes a row) | **No** | Client join (AuthGuard 657–660) |
| Hydrant invoice | `tbl_HydrentInvoice` (**no CID**) | `tbl_Client`, `tbl_Factory` | **No** | Client join (AuthGuard 653–656) |

Coverage note: every transaction-facing table in the reverse index is listed above or classified in §4; SPs that mutate transactions (`sp_Requisition_*`, `sp_GeneratePO_FromReqNo`, `sp_ReleasePO_Final`, `sp_GetReleasedPO_Details`, `sp_SubmitRequisition`, `sp_CancelRequisition`, `sp_RequisitionItem_BulkUpsert`) have **bodies only in SQL Server, not in repo** (SHARED_RUNTIME §5) — their internal tenancy behavior is PENDING_VALIDATION (§12 V-1).

## 7. Stock Dependency Audit

Every discovered read/write of `tbl_stock` (traced in code + reverse index; `rg` evidence dated 2026-10-01):

| # | Path | Verb | Key predicate | Store dimension | Quantity semantic | Conflict? |
|---|------|------|---------------|-----------------|-------------------|-----------|
| S1 | `newproduct_master.aspx.cs:750` (opening stock on product create) | INSERT | `Product_id` + `ShippedToStoreId='STR001'` (hard-coded) | Yes (hard-coded STR001) | Writes both `Quantity` + `Quantity_Num` | Opening stock only; hard-coded store |
| S2 | `ImportProducts.aspx.cs` (Tally XML import — insert path) | INSERT | `Product_id`, **no store** | **No** | `Quantity` varchar only | Legacy-shape row; no store dimension |
| S3 | `ImportProducts.aspx.cs` (update paths ×3: lines 881, 1009, 1334) | UPDATE | `WHERE Product_name = @ProductName` | **No** | Overwrites `Quantity` (absolute, not delta) | **Violates store-keyed identity**; name-keyed; absolute overwrite |
| S4 | `ImportProducts.aspx.cs` (GST-only update, line 2080) | UPDATE | `Product_name` | No | Touches rate only | Name-keyed |
| S5 | `Purches_new_vendor.aspx.cs:452–462` (product purchase path) | SELECT + UPDATE/INSERT | `Product_id` (concatenated SQL), no store | **No** | `cast(Quantity as int)+qty` (delta) | **No store dimension**; SQL injection-class concatenation |
| S6 | `Purches_new_vendor.aspx.cs` (service purchase path) | SELECT + UPDATE/INSERT | `Product_id` of service | **No** | delta | Same as S5 |
| S7 | `Purches_exting_vendor.aspx.cs:628–663` (`UpdateStock`) | UPDATE (else INSERT) | `Product_id = @p AND ShippedToStoreId = @s` | **Yes** | `Quantity_Num = ISNULL(Quantity_Num,0)+@q`, mirrors into `Quantity` varchar | **Conformant** (model path) |
| S8 | `EditPurchase.aspx.cs:818–840` (purchase edit delta) | UPDATE (else INSERT when delta>0) | `Product_id=@Product_id AND ShippedToStoreId=@StoreId AND ISNUMERIC(Quantity)=1` | **Yes** | `CAST(Quantity DECIMAL)+@Delta`, `Sail_Rate`/tax overwritten | **Conformant**; note: overwrites rate/tax on stock row (out of tenancy scope) |
| S9 | `EditPurchase.aspx.cs` (delete-delta paths via detail diff) | UPDATE | same composite key | Yes | delta | Conformant |
| S10 | `Delete_purtches.aspx.cs:180–251` (purchase deletion reversal) | SELECT + UPDATE | `s.Product_id = pd.Product_id AND s.ShippedToStoreId = pd.ShippedToLoc` | **Yes** | `Quantity - pd.Quantity` (varchar cast), mirrors to `np.Quantity` on `tbl_NewProduct` | **Conformant**; also decrements master quantity (§8) |
| S11 | `Delete_invoice.aspx.cs:249` | UPDATE | — | — | — | **Commented out** — invoice deletion no longer restores stock (see §8 and §11-C6) |
| S12 | `Service_stock.aspx.cs:30` | SELECT | `Product_id NOT LIKE 'P%'` | No | read-only display | Unscoped read (conformant to sharing; but no tenant-owned *sales* stock exists) |
| S13 | `Product_stock.aspx` (reports) | EXEC `sp_GetProductStockByStore`, `sp_SearchProductsFast`, `sp_GetProductCategories` | SP bodies not in repo | presumed store-keyed by name | read-only | **V-1**: bodies unverifiable |
| S14 | DB side (UAT inventory, SHARED_SCHEMA §10/§11) | SP/view | `sp_SearchStockGrouped`, `usp_Reconcile_MasterToStock`, `usp_CreateMissingMastersFromStock`, `usp_UpdateZeroMastersFromStock`, `vw_CategoryStoreStock_Safe`, `vw_StockReconciliation`, `stock_pid_mapping`, archive/fix tables | — | — | Not called from C#; behavior PENDING_VALIDATION (V-2) |

**Tenancy conclusion:** no stock path is tenant-scoped; no `CompanyID` exists or is proposed for `tbl_stock` — schema and access conform to frozen §5. **Integrity conclusion (not tenancy):** S2–S6 key stock by product name/id without the store dimension and S3 overwrites absolutely — under the frozen identity (`ProductID + StoreId → Quantity`) these paths can misplace quantities across stores or duplicate rows. All stock remediation is blocked behind §13-D5 (dependency-complete validation), consistent with MULTI_TENANCY §9.

## 8. Product Quantity Dependency Audit

Every discovered read/write of `tbl_NewProduct.Quantity` / `Quantity_Num`:

| # | Path | Verb | Direction | Evidence |
|---|------|------|-----------|----------|
| Q1 | `newproduct_master.aspx.cs` create (line 726–728) | INSERT | Sets both `Quantity` + `Quantity_Num` = opening qty (same value written to stock row S1) | code trace |
| Q2 | `newproduct_master.aspx.cs` in-grid update (line 787) | UPDATE | Edits `Quantity=@Quantity, Quantity_Num=@QuantityNum` on product row | code trace |
| Q3 | `newproduct_master.aspx.cs` read for edit (lines 287, 1062) | SELECT | Displays `ISNULL(Quantity,0)` | code trace |
| Q4 | `Manual_Invoice.aspx.cs:474` | UPDATE | **Decrements**: `Quantity = CAST(... AS DECIMAL(18,2)) - @Qty WHERE ProductID=@PID AND CompanyID=@CompanyID` | code trace |
| Q5 | `Add_invoice.aspx.cs:1178` | UPDATE | **Decrements** on invoice creation (same pattern, `@TruePID`) | code trace |
| Q6 | `Add_invoice.aspx.cs:661–691` | SELECT (JOIN) | Reads master qty alongside line qty during invoice assembly | code trace |
| Q7 | `Delete_invoice.aspx.cs:249` | — | Stock-restore SQL commented out; **no** master-quantity restore found either | code trace |
| Q8 | `Delete_purtches.aspx.cs:185–186` | UPDATE | **Decrements master quantity** when deleting a purchase (`np.Quantity = ... - pd.Quantity`) | code trace |
| Q9 | `ImportProducts.aspx.cs` update paths | UPDATE | Overwrites `tbl_NewProduct` Quantity (absolute) alongside stock | code trace |
| Q10 | `NewUpdate_product.aspx` (orphan editor) | UPDATE | Edits product row (qty fields included per catalog) | DOMAIN_masters |
| Q11 | DB: `vw_StockReconciliation` | VIEW | Compares `Master_Qty_Num` vs `Stock_Qty_Num` with `Qty_Difference` | UAT_CATALOG §views |

**Finding (conflict with frozen §6):** Q4/Q5 make the product master a **live decrementing balance** at sale time; Q8 mutates it on purchase reversal; Q7 shows reversals are asymmetric (sale-side quantity never restored). The master column is *de facto* a competing inventory ledger — exactly what frozen §6 prohibits treating as authoritative. The demotion itself is frozen policy; the remediation (which of Q1–Q10 to retire) is gated behind §13-D5 and requires the reconciliation data from Q11/V-2 to size the damage.

## 9. Tenant Switcher / `CompanyContext` Audit

How `CompanyContext.CurrentCompanyID` (from `Session["CompanyID"]`, `Bill.Master.cs`) is used today, per domain:

| Domain | Switcher used to… | Conformance to frozen §4 |
|--------|--------------------|--------------------------|
| Shared masters — Product/Category | **Filter and create** (`newproduct_master`, `newproductparent`, combos, dup-checks) | **Violation**: partitions a shared domain (C1) |
| Shared masters — Stock | Never (verified: zero CompanyID refs in any `tbl_stock`-touching page) | Conformant |
| Shared masters — Store (`Stores`) | Never (no CRUD exists) | Conformant |
| TENANT masters — Vendor / Client / Factory / Representative / addresses | Full CRUD scoping + code generation | Conformant with TENANT_MASTER target (§5.5–5.7) |
| Geography — State/City | Full CRUD scoping | Conformant *only if* §13-D3 ratifies per-tenant geography; otherwise violation |
| Transactions — Quotation/PO/PR/Invoice/Chalan/Proforma | Write `CompanyID` on tenant-owned headers; lists scoped | Conformant where CID exists; gaps in §6 |
| Reports — `Payment_due`, `Purchess_due`, `Service_stock`, `seartch_purtch` | Not used (unscoped pages) | Violates *tenant-owned* scoping expectations for due reports (C4); conformant for stock display |
| Identity — `AddUser`, `ViewUser`, `home` | Filters user lists (`tbl_login.CompanyID`) | Out of ownership scope (docs/22 surface) |
| Duplication feature (docs/41–44) | Seeds per-tenant vendor/customer copies | Consistent with TENANT_MASTER parties; would be inconsistent if parties were shared |

**Key asymmetry:** the switcher is *over*-used on product/category (shared) and *under*-used on due reports/legacy purchase searches (tenant-owned). Both directions are recorded as conflicts in §11.

## 10. Indirect Tenant-Inference Audit

Patterns by which tenant ownership is currently derived rather than persisted directly (all conflict with frozen §1):

| ID | Mechanism | Where observed | Affects |
|----|-----------|----------------|---------|
| I1 | `tbl_Purches.Client_Id = tbl_Vendor.Vendor_Id AND v.CompanyID = @CompanyID` | `AuthGuard.TenantSql` case `tbl_Purches.Purches_Id` (lines 649–652); purchase print `purches_bill`; per SHARED_SCHEMA §6 also some list pages | Purchase, purchase payment/due |
| I2 | `tbl_invoice_payment.Quotation_No = tbl_Quotation.Quotation_no AND q.CompanyID = @CompanyID` | `AuthGuard` 645–648 | Invoice payment |
| I3 | `tbl_HydrentInvoice.Client_ID = tbl_Client.Client_Id AND c.CompanyID = @CompanyID` | `AuthGuard` 653–656 | Hydrant invoice |
| I4 | `tbl_qsHydrentQuotation.ClientId = tbl_Client.Client_Id AND c.CompanyID = @CompanyID` | `AuthGuard` 657–660 | Hydrant quotation |
| I5 | `tbl_Expenses.VisitId → tbl_SalesVisitReport.CompanyID` (visit-owned child) | `AuthGuard.UserCanApproveExpense`; SHARED_SCHEMA §6 | Visit expenses |
| I6 | Parent-child inference for `tbl_purches_due`, `tbl_invoice_due`, `tbl_purches_details`, `tbl_PO_PartySnapshot`, `tbl_invoice_payment_tds`, `tbl_qsHydrentDetails` | Schema: child rows lack `CompanyID`; scoping only via parent join in code | All listed |
| I7 | `tbl_NewProduct.CompanyID` join on transaction reads (`np.CompanyID = @CompanyID`) | `Add_invoice` 661–691; `Manual_Invoice` 167 | Invoice assembly — this *product* inference would become invalid once products are shared (C2) |
| I8 | Session/static tenant state (`Session["CompanyID"]` read at write time) | Legitimate per frozen §1 when writing **tenant-owned** rows; not an inference defect | — |

I1–I6 are the remediation targets for direct-`CompanyID` persistence (frozen §7.3); I7 becomes moot under the shared-product decision.

## 11. Current-State Conflicts

Separated findings (verified in code/schema) from hypotheses (unverified). Confidence: **verified** = traced in code or UAT catalog this pass; **suspected** = documented but not re-traced.

| ID | Conflict | Frozen ref | Status | Confidence |
|----|----------|------------|--------|------------|
| C1 | Product & category CRUD/combo is tenant-scoped (`CompanyID` filter + per-tenant code generation) — a partitioned shared domain | §3.1, §7.2 | Verified | High |
| C2 | Invoice/purchase joins derive scope from `tbl_NewProduct.CompanyID` (I7) — breaks once products are shared | §1 | Verified | High |
| C3 | Purchase stack (`tbl_Purches`, details, due, payment, `tbl_PO_PartySnapshot`) lacks direct `CompanyID`; ownership inferred via vendor (I1) | §1, §7.3 | Verified | High |
| C4 | Invoice payment/due stack lacks `CompanyID` (I2, I6); due report pages unscoped | §1, §7.3 | Verified (schema) | High |
| C5 | Hydrant quotation/invoice lack `CompanyID` (I3, I4) | §1, §7.3 | Verified (schema) | High |
| C6 | Product-master quantity is live-decremented at sale (Q4/Q5) and never restored on invoice delete (Q7); purchase delete decrements master (Q8) | §6 | Verified | High |
| C7 | Stock paths S2–S6 key/overwrite by `Product_name`/id without store dimension | §5 identity | Verified | High |
| C8 | Unscoped CRUD/read surfaces on tenant-owned or inference-owned data: `clientHandlerAdmin.ashx` (client typeahead), `Service_stock`, `Payment_due`, `Purchess_due`, `seartch_purtch`, GL/petty expense pages | §1 | Verified (per catalogs) | Medium-High |
| C9 | Geography is per-tenant (State/City CRUD) — may or may not be the intended business rule | §3.1-adjacent (R3) | Verified behavior; business intent unknown | High (behavior), Low (intent) |
| C10 | In-repo DDL patches (`tbl_NewProduct_CompanyID.sql`, `tbl_NewparentProduct_CompanyID.sql`) institutionalize tenant columns on frozen-shared masters | §7.1 | Verified (files exist; unapplied on UAT) | High |
| C11 | Legacy purchase paths are concatenated-SQL and unscoped (S5/S6; `seartch_purtch`) | §7.3-adjacent | Verified | High |
| H1 | Suspected: production (`flamex_live`) may hold tenant-2 transaction rows or NULL CompanyIDs that UAT does not (UAT shows ~all rows `CompanyID=1`) | — | Suspected | Low — V-7 |
| H2 | Suspected: SP bodies may contain additional tenant predicates or inference not visible in repo | — | Suspected | Low — V-1 |

## 12. Pending Validation / Unknowns

| ID | Unknown | Why it matters | How to validate |
|----|---------|----------------|-----------------|
| V-1 | Bodies of all 38 user SPs on UAT (`sp_Requisition_*`, `sp_GeneratePO_FromReqNo`, `sp_ReleasePO_Final`, `sp_GetReleasedPO_Details`, stock `usp_*` family, `sp_SearchProductsFast`, `sp_GetProductStockByStore`, `sp_GetProductCategories`, `sp_SearchStockGrouped`) | SPs mutate PR/PO/stock; tenancy behavior inside them is invisible to repo analysis | DBA-extracted signatures + behavior review on UAT (bodies stay out of git per repo rule) |
| V-2 | `vw_StockReconciliation` / `vw_CategoryStoreStock_Safe` current divergence data | Sizes the C6/C7 damage before any change | Run views on UAT read-only; record row counts/differences |
| V-3 | `tbl_PO_PartySnapshot` provenance & semantics (who inserts, what tenant fields mean) | It is the only PO child without CompanyID; classification TRANSACTION_OWNED is provisional | Trace `Generate_PO_Preview` insert columns; confirm snapshot scope |
| V-4 | `tbl_HydrantProduct` ownership intent (shared offering vs tenant catalog) | Hydrant flows reference it; no CompanyID and no docs | Business owner question; UAT row/usage review |
| V-5 | `tbl_Tieup_Company` role and intended ownership | GL expense pages select it; nothing documents it | Business owner question |
| V-6 | `tbl_Departments` / `tbl_Designations` intended ownership (org lookups vs tenant lists) | User admin depends on them | Business owner question (identity surface) |
| V-7 | Production data reality: tenant-2 rows, NULL CompanyIDs, duplicate product names across (future) tenants | Migration/decision risk | DBA read-only audit of `flamex_live` (not possible from repo) |
| V-8 | Whether VAT tables' `CompanyID` holds distinct rates per company or all rows are company-1 | Decides §13-D2 | Read-only data check on UAT |
| V-9 | Whether any page passes the switcher value into shared-master *create* in a way that forked data already exists | C1's blast radius | Read-only data check: products/categories with CompanyID=2 |
| V-10 | `tbl_Quotation.CompanyID` vs hydrant quotation row written by `CreateHydrentQuatation` (it INSERTs into `tbl_Quotation`) | Hydrant tenancy semantics | Trace `CreateHydrentQuatation.aspx.cs` insert |
| V-11 | Triggers / background jobs touching audited tables | Repo shows none discoverable; static review can't exclude them | `sys.triggers` / SQL Agent review on UAT (same caveat as sales-visit audit §8) |
| V-12 | Extent of legacy-chain (`tbl_Product`/`tbl_parentProduct`) live usage | Decides §13-D6 | Page access logs / data recency check |

No item above has been validated by this audit; none of the classifications in §4/§5 silently assume their resolution.

## 13. Required Architecture Decisions Before Implementation

Decisions that must be **explicitly resolved** (and recorded — e.g., as ADRs extending `docs/architecture/`) before any schema or application implementation begins:

| ID | Decision | Options | Audit input |
|----|----------|---------|-------------|
| D1 | **Ratify Vendor / Client / Factory / Representative / client addresses as `TENANT_MASTER`** (or overturn) | (a) ratify per-tenant parties; (b) shared party registry with per-tenant ledger aliases | §5.5–5.7: current code and v2.1 duplication feature assume (a); frozen doc is silent on parties |
| D2 | **Rate/tax master ownership** — `GLOBAL_REFERENCE` vs per-tenant rate overrides | shared static rates; tenant overrides | §5.8; transaction lines already snapshot rates |
| D3 | **Geography (`tbl_State`, `tbl_City`) ownership** — shared reference vs per-tenant (current) | unify; keep per-tenant | §5.9; current code is per-tenant; physical-identity reasoning says shared |
| D4 | **Shared-product mechanics under current codegen** — how `PRD###` codes, duplicate-name checks, and `ImportProducts` behave enterprise-wide | enterprise-wide code sequence; import reconciliation rules | §5.2, C1; newproduct codegen currently tenant-scoped |
| D5 | **Stock integrity remediation scope** (precondition to lifting the implementation block): retire/replace name-keyed paths (S2–S6), define reversal behavior (S11/Q7), decide `Quantity` varchar vs `Quantity_Num` authority inside `tbl_stock` | phased fix vs big-bang; which column is canonical | §7, §8 |
| D6 | **Legacy product chain retirement** (`tbl_Product`, `tbl_parentProduct`, `tbl_ProductCatagory`) | retire; keep frozen as shared-legacy | §5.11, V-12 |
| D7 | **Direct `CompanyID` addition list** for the transaction stack — confirm the exact tables from §6 marked "No" (purchase family, PO PartySnapshot, invoice payment/tds/due, hydrant family, visit expenses/responses, petty cash) | all at once vs per-flow | frozen §7.3 + §6; no migration design here |
| D8 | **Supersede note for the frozen doc**: MULTI_TENANCY.md §3.1 lists Product as shared without stating party-master ownership; if D1 ratifies TENANT parties, record it as an explicit amendment (not a silent edit) | amend MULTI_TENANCY via new ADR; leave doc and rely on this audit | §5.5–5.6 tension analysis |

Each decision requires an owner and a recorded outcome; none may be resolved implicitly by an implementation PR (docs/22 §Regression rule).

## 14. Implementation Sequencing Prerequisites

Ordered prerequisites; implementation of the frozen model may not begin until all are satisfied:

1. **Resolve §13 decisions D1–D8** and record outcomes (ADRs or a superseding note in `docs/architecture/`).
2. **Complete V-1 and V-2** (SP behavior + reconciliation divergence) — these are the two unknowns that can invalidate D5/D7 scope.
3. **Fill the review ledger** in [`ARCHITECTURE.md`](ARCHITECTURE.md) §6 (stock mutations, purchase flows, invoice/sales flows, reconciliation views/SPs, master-data CRUD) — this document supplies the inventory; the ledger still requires evidence-based sign-off per row.
4. **V-7 production data audit** (DBA, read-only) before any migration design; UAT-only conclusions are not production conclusions.
5. Only then: schema/application implementation per flow (purchase stack first per §6's widest gap), each PR satisfying docs/22 gates and preserving `tbl_SystemNotification`/transaction behavior.

This audit itself changes no behavior and commits to no option in D1–D8.
