# FLMX Inventory Management Architecture v1.0

## Status and Authorization

- **Document:** FLMX Inventory Management Architecture v1.0
- **Status:** Baseline for Section-by-Section Architecture Review
- **Business Logic Register:** Approved as review input
- **Architecture Section Reviews:** Pending
- **Implementation Authorization:** NOT GRANTED
- **ADR-002 reconciliation:** Required before ratification/implementation

This document reconstructs the FLMX Inventory Management Architecture v1.0 baseline from approved business-logic decisions and repository evidence. It is intended for fresh, section-by-section architecture review only. No implementation, migration design, DDL, application changes, or database execution is authorized through this document.

---

## 1. Purpose and Scope

This document provides the approved inventory management architecture baseline for the FLMX (Flame-ex / AminurpERP) system. It section-by-section documents the approved business logic, repository observations, ADR-002 governance decisions, and pending decisions that must be resolved before implementation.

**Scope covers:**
- Inventory transaction effects (creation, PO, GRN, PI, DPCC, direct purchase, adjustments, transfers, cancellations)
- Company ownership and warehouse model (CompanyStore, inventory balance by Company + Product + Warehouse, inter-company transfers)
- Transfer receipt and discrepancy handling (two-step transfers, partial receipts, quarantine, closure)
- Purchase and GRN (PO reference, partial GRNs, over-receipt, damage/rejection, duplicate prevention, auditable corrections)
- Sales and Tax Invoice (inventory deduction on invoice posting, negative stock override, invoice cancellation)
- Negative stock and adjustments (approval requirements, role-based approval, maker-checker)
- Opening stock and migration (physical verification, legacy reconciliation, parallel operation)
- Availability and reporting (usable stock, quarantine, transit, reservation deferred, required reports)
- Product, UOM, and tracking (base UOM, purchasing/sales UOM conversion, batch/lot tracking)
- Valuation (FIFO, Weighted Average Cost, Standard Cost candidates)
- Governance and audit (role-based permissions, maker-checker, immutable posted movements, duplicate prevention, audit history)

**Scope does NOT cover:**
- SQL schema definition or DDL
- Application code changes
- Migration scripts
- Database execution
- ADR-002 modification (governance document must not be silently overwritten)
- Any implementation or deployment

---

## 2. Architecture Principles

| Principle | Description |
|-----------|-------------|
| **Company-exclusive warehouse** | Each physical warehouse belongs exclusively to one company. A company cannot use another company's warehouse. |
| **CompanyStore mapping** | CompanyStore maps a company to its authorized warehouses. |
| **Inventory balance grain** | Inventory balance is maintained by Company + Product + Warehouse. |
| **Atomic invoice posting** | Inventory deduction occurs only when the Tax Invoice is finally posted/confirmed. |
| **Authorized reversal** | Purchase and invoice cancellations require controlled reversal when the original movement was posted. |
| **Authorization for every manual change** | Every manual stock increase/decrease requires authorization before posting. |
| **Maker-checker enforcement** | No self-approval for stock adjustment or negative override. |
| **Immutable posted movements** | Posted movements are immutable; corrections use reversal/repost. |
| **Duplicate-post prevention** | Prevent duplicate posting through unique source references and idempotent posting. |
| **Audit trail permanence** | Audit history is retained permanently, subject to applicable retention/archival policy. |
| **Shared master data invariants** | Product identity, Category identity, and Store identity are shared across tenants; stock grain is ProductID + StoreID; shared identity carries no CompanyID (per ADR-002 invariants INV-K1 through INV-K7). |
| **Transactional ownership** | Every tenant-owned transaction root carries a direct CompanyID (D7). Ownership is never inferred from Vendor, Client, Product, or any other master. |

---

## 3. Inventory Ownership and Warehouse Model

| Concept | Approved Rule | Repository Evidence | Status |
|---------|---------------|--------------------|--------|
| **Company + Product + Warehouse inventory balance** | Inventory balance is maintained by Company + Product + Warehouse. | `CompanyStore` maps a company to its authorized warehouses. | Approved business rule |
| **Warehouse ownership** | Each physical warehouse belongs exclusively to one company. A company cannot use another company's warehouse. | ADR-002 D5: Every physical inventory quantity belongs to a Store/Warehouse. | ADR-002 ratified |
| **Inter-company transfers** | Inter-company transfers are permitted. Inter-company transfer changes ownership; it is not an ordinary warehouse transfer. | ADR-002 §2.2 transaction objects: inter-company transfer targets classified. | Approved; P03 pending |
| **Two-step transfers** | Two-step transfers maintain stock-in-transit. | Approved business logic Section 3. | Approved |
| **Instant transfer** | Instant transfer posts source OUT and destination IN atomically; company-wide net quantity change is zero. | Approved business logic Section 1. | Approved |
| **Transfer traceability** | Both instant and two-step transfers are supported. All transfers must be traceable and auditable. | Approved business logic Section 3. | Approved |
| **Dispatch/receipt audit** | Dispatch, receipt, discrepancy, adjustment, and closure are auditable. | Approved business logic Section 3. | Approved |

**ADR-002 reconciliation required:**
- ADR-002 establishes that shared Product / Category / Store / Stock identity carries no CompanyID (INV-K1 through INV-K7).
- The approved business logic describes CompanyStore mapping, Company + Product + Warehouse inventory balance, and company-exclusive warehouse ownership.
- These concepts must be formally reconciled against ADR-002 before architecture ratification or implementation.
- OWNERSHIP_AUDIT.md is a SELECT-only UAT verification dated 2026-10-01, not a production census.
- `tbl_stock` currently has no CompanyID and no enforced unique Product + Store key.
- Existing `tbl_stock` rows include blank store identifiers and duplicate product/store groups.

---

## 4. Logical Inventory Entities and Movement Concepts

| Entity | Description | Approved Rule | Repository Evidence |
|--------|-------------|---------------|---------------------|
| **Product** | All tangible products are inventory-managed; services are automatically excluded. | Section 9: All tangible products are inventory-managed; services are automatically excluded. | Current product catalog uses `tbl_NewProduct` and `tbl_NewparentProduct`; legacy `tbl_Product`/`tbl_parentProduct` also exist. |
| **Base UOM** | Each product has a base UOM. | Section 9: Each product has a base UOM. | `sp_GetProductCategories`, `sp_SearchProductsFast`, `sp_GetProductStockByStore` procs. |
| **Purchasing/sales UOM** | Purchasing/sales UOM may differ through configured conversions. | Section 9: Purchasing/sales UOM may differ through configured conversions. | Conversions are configurable; all movements converted to base UOM before posting. |
| **Fractional precision** | Fractional precision is configurable by product UOM. | Section 9: Fractional precision is configurable by product UOM. | Configurable per product UOM. |
| **Batch/lot tracking** | Support batch/lot tracking with expiry and serial-number tracking. | Section 9: Support batch/lot tracking with expiry and serial-number tracking. | Product tracking configuration determines applicable tracking mode. |
| **Batch + serial simultaneously** | Whether one product can simultaneously use both tracking modes remains pending (P02). | Pending decision P02. | Repository does not establish this fact. |
| **Product identity** | Product identity is globally unique, consistent with ADR-002 D4. | Section 9: Product identity is globally unique, consistent with ADR-002. | ADR-002 D4: ProductID / Product Code is unique enterprise-wide, across both tenants. |
| **Inactive products** | Inactive products require authorization for exceptional transactions; historical movements remain accessible. | Section 9: Inactive products require authorization for exceptional transactions. | Repository evidence describes implementation as it exists; must not be rewritten to pretend approved target behavior is already implemented. |
| **Product-master edits** | Product-master edits must not silently modify inventory balances. | Section 9: Product-master edits must not silently modify inventory balances. | Observed implementation may silently modify balances; must be validated. |

**Pending decisions:**
- P02: Whether a product can use batch and serial tracking simultaneously.
- P04: Inter-company transfer documents and approval workflow.

---

## 5. Transaction Effect Matrix

| Event | Stock Movement | Approved Rule | Repository Evidence |
|-------|---------------|---------------|---------------------|
| **Product creation** | Does not independently create physical stock. | Opening stock follows an approved opening-balance process. | `newproduct_master` inserts `tbl_stock` only with opening stock row, store `STR001` hardcoded. |
| **PO creation/release** | Does not move stock. | PO release is not goods receipt. | `sp_ReleasePO_Final` contains no stock statement (business_process/04). |
| **PO-based GRN** | Increases stock by actual accepted quantity. | GRN must reference an existing PO. Stock increases by actual accepted quantity, not ordered quantity. | Gap: no GRN page identified; proposed target flow in business_process/05. |
| **Direct purchase** | Increases stock immediately on save. | Direct purchase remains a separate receipt route. | `Purches_exting_vendor.UpdateStock` posts stock on save without PO reference (business_process/02). |
| **PI (Proforma Invoice)** | No stock movement. | PI has no stock movement. | `08_proforma_invoice_pi.md`: Proforma save does not deduct stock. |
| **DPCC** | No independent stock deduction. | Every Tax Invoice, including DPCC-sourced, deducts actual invoiced product quantities. | `09_tax_invoice_ti.md`: DPCC records delivered quantity only; does not deduct stock. |
| **Tax Invoice posting** | Deducts actual invoiced product quantities. | Every posted Tax Invoice deducts actual invoiced product quantities regardless of DPCC, PI, or manual source. | `09_tax_invoice_ti.md`: `UPDATE tbl_NewProduct SET Quantity = CAST(... AS DECIMAL(18,2)) - @Qty WHERE ProductID = @TruePID AND CompanyID = @CompanyID`. |
| **Purchase cancellation** | Requires controlled reversal when original movement was posted. | Purchase and invoice cancellations require controlled reversal when the original movement was posted. | `Delete_purtches` subtracts detail quantity from `tbl_stock.Quantity` and `tbl_NewProduct.Quantity` then deletes header, lines, and due. |
| **Invoice cancellation** | Reverses the exact original movement. | Cancellation reverses the exact original movement. | `Delete_invoice` uses `updatestock1` (commented out) and does not change stock. Stock restore on delete is commented out. |
| **Stock adjustment** | Requires reason and authorization. | Stock adjustments require reason and authorization. | `tbl_StockAdjustments` has `ProductID` but no store column and no CompanyID (OWNERSHIP_AUDIT.md). |
| **Warehouse transfer** | Supported. | Warehouse transfers are supported. | Observed `ShippedToStoreId` / `ShippedToLoc` on purchase details. |
| **Instant transfer** | Source OUT and destination IN atomically; company-wide net quantity change is zero. | Instant transfer posts source OUT and destination IN atomically; company-wide net quantity change is zero. | Approved business logic Section 1. |
| **Over-receipt** | Requires authorized approval. | Over-receipt requires authorized approval. | Section 4: Over-receipt approval evidence must identify PO/line, ordered quantity, previously received quantity, additional quantity, requester, approver, decision, and timestamp. Exact approver roles remain pending (P05). |
| **Under-receipt/shortage** | Investigated first; unresolved requires authorized adjustment. | Shortages are investigated first; unresolved shortages require authorized adjustment. | Section 3: Shortages are investigated first; unresolved shortages require authorized adjustment. |
| **Damaged goods** | Enter quarantine; excluded from usable stock. | Damaged goods enter quarantine and are excluded from usable stock. | Section 3: Damaged/rejected quantities enter quarantine and are excluded from usable stock. |
| **Rejected goods** | Recorded separately; excluded from usable stock. | Rejected goods are recorded separately and excluded from usable stock. | Section 4: Rejected goods are recorded separately and excluded from usable stock. |

---

## 6. Purchase and GRN

| Rule | Description | Repository Evidence |
|------|-------------|---------------------|
| **GRN must reference an existing PO** | GRN must reference an existing PO. | Section 4: GRN must reference an existing PO. |
| **Multiple partial GRNs supported** | Multiple partial GRNs are supported. | Section 4: Multiple partial GRNs are supported. |
| **Stock increases by actual accepted quantity** | Stock increases by actual accepted quantity, not ordered quantity. | Section 4: Stock increases by actual accepted quantity, not ordered quantity. |
| **Over-receipt requires authorized approval** | Over-receipt requires authorized approval. | Section 4: Over-receipt requires authorized approval. |
| **GRN against closed/cancelled PO is blocked** | GRN against closed/cancelled PO is blocked. | Section 4: GRN against closed/cancelled PO is blocked. |
| **Damaged goods enter quarantine** | Damaged goods enter quarantine. | Section 4: Damaged goods enter quarantine. |
| **Rejected goods recorded separately** | Rejected goods are recorded separately and excluded from usable stock. | Section 4: Rejected goods are recorded separately and excluded from usable stock. |
| **Direct purchase supported** | Direct purchase remains supported with equivalent acceptance/damage/rejection treatment. | Section 4: Direct purchase remains supported with equivalent acceptance/damage/rejection treatment. |
| **Duplicate receipt posting prevented** | Duplicate receipt posting is prevented. | Section 4: Duplicate receipt posting is prevented. |
| **Posted GRNs cannot be silently edited** | Correction uses auditable reversal/adjustment. | Section 4: Posted GRNs cannot be silently edited; correction uses auditable reversal/adjustment. |
| **Over-receipt approval evidence** | Must identify PO/line, ordered quantity, previously received quantity, additional quantity, requester, approver, decision, and timestamp. | Section 4: Over-receipt approval evidence must identify PO/line, ordered quantity, previously received quantity, additional quantity, requester, approver, decision, and timestamp. Exact approver roles remain pending (P05). |

**Repository evidence:**
- No GRN page, table, or procedure exists in the reviewed source (business_process/05).
- PO release (`sp_ReleasePO_Final`) contains no stock statement (business_process/04).
- Direct purchase posts stock on save without a PO reference (business_process/02).
- The GRN node is a target state; it is not implemented behaviour (business_process/05).

**Pending decisions:**
- P05: Exact over-receipt approver roles/evidence.
- P06: Quarantine release authorization.
- P07: PO closure behavior with unreceived quantities.

---

## 7. Warehouse and Inter-Company Transfers

| Concept | Approved Rule | Repository Evidence |
|---------|---------------|---------------------|
| **Two-step transfers maintain stock-in-transit** | Two-step transfers maintain stock-in-transit. | Section 3: Two-step transfers maintain stock-in-transit. |
| **Partial receipts supported** | Partial receipts and multiple receipts per transfer are supported. | Section 3: Partial receipts and multiple receipts per transfer are supported. |
| **Shortage investigation** | Shortages are investigated first; unresolved shortages require authorized adjustment. | Section 3: Shortages are investigated first; unresolved shortages require authorized adjustment. |
| **Damaged/rejected to quarantine** | Damaged/rejected quantities enter quarantine and are excluded from usable stock. | Section 3: Damaged/rejected quantities enter quarantine and are excluded from usable stock. |
| **Transfer closure requires authorized approval** | Transfer closure requires authorized approval. | Section 3: Transfer closure requires authorized approval. |
| **Source/destination ownership history preserved** | Source/destination ownership history is preserved. | Section 3: Source/destination ownership history is preserved. |
| **Dispatch/receipt/adjustment/closure audit** | Dispatch, receipt, discrepancy, adjustment, and closure are auditable. | Section 3: Dispatch, receipt, discrepancy, adjustment, and closure are auditable. |
| **Inter-company transfers change ownership** | Inter-company transfer changes ownership; it is not an ordinary warehouse transfer. | Section 2: Inter-company transfer changes ownership; it is not an ordinary warehouse transfer. |
| **All transfers must be traceable and auditable** | All transfers must be traceable and auditable. | Section 2: All transfers must be traceable and auditable. |
| **Company warehouse exclusivity** | Each physical warehouse belongs exclusively to one company. A company cannot use another company's warehouse. | Section 2: Each physical warehouse belongs exclusively to one company. |
| **CompanyStore mapping** | CompanyStore maps a company to its authorized warehouses. | Section 2: CompanyStore maps a company to its authorized warehouses. |
| **Inter-company transfers are permitted** | Inter-company transfers are permitted. | Section 2: Inter-company transfers are permitted. |

**ADR-002 reconciliation required:**
- ADR-002 S1: `tbl_stock` is the sole authoritative physical-stock ledger.
- ADR-002 S2: `tbl_NewProduct.Quantity` and `Quantity_Num` are not authoritative physical stock.
- ADR-002 D5: Every physical inventory quantity belongs to a Store/Warehouse.
- The approved business logic describes Company + Product + Warehouse inventory balance.
- `tbl_stock` currently has no CompanyID and no enforced unique Product + Store key.
- Existing `tbl_stock` rows include blank store identifiers and duplicate product/store groups.
- These conflicts must be explicitly identified and reconciled before ratification.

**Pending decisions:**
- P03: Inter-company ownership-change point: dispatch or destination receipt.
- P04: Inter-company transfer documents and approval workflow.

---

## 8. Sales and Tax Invoice

| Concept | Approved Rule | Repository Evidence |
|---------|---------------|---------------------|
| **Inventory deduction on Tax Invoice posting** | Inventory deduction occurs only when the Tax Invoice is finally posted/confirmed. | Section 5: Inventory deduction occurs only when the Tax Invoice is finally posted/confirmed. |
| **Every posted Tax Invoice deducts actual invoiced quantities** | Every posted Tax Invoice deducts actual invoiced product quantities regardless of DPCC, PI, or manual source. | Section 5: Every posted Tax Invoice deducts actual invoiced product quantities regardless of DPCC, PI, or manual source. |
| **DPCC does not deduct stock** | DPCC itself does not deduct stock. | Section 5: DPCC itself does not deduct stock. |
| **Services have no physical inventory effect** | Services have no physical inventory effect. | Section 5: Services have no physical inventory effect. |
| **Warehouse selection** | User selects issuing warehouse from company-authorized warehouses. | Section 5: User selects issuing warehouse from company-authorized warehouses. |
| **Insufficient stock blocked** | Insufficient stock is blocked by default, subject to approved negative-stock override. | Section 5: Insufficient stock is blocked by default, subject to approved negative-stock override. |
| **Deduction linked to invoice and invoice line** | Deduction is linked to invoice and invoice line. | Section 5: Deduction is linked to invoice and invoice line. |
| **Editing a posted invoice** | Requires reversing the original movement and posting the corrected movement. | Section 5: Editing a posted invoice requires reversing the original movement and posting the corrected movement. |
| **Cancellation reverses exact original movement** | Cancellation reverses the exact original movement. | Section 5: Cancellation reverses the exact original movement. |
| **Customer returns** | Support quarantine-first or direct usable-stock receipt according to inspection/condition. | Section 5: Customer returns support quarantine-first or direct usable-stock receipt according to inspection/condition. |
| **Invoice cancellation distinct from customer return** | Invoice cancellation reversal is distinct from physical customer return; prevent double counting. | Section 5: Invoice cancellation reversal is distinct from physical customer return; prevent double counting. |

**Repository evidence:**
- `Add_invoice.Button1_Click` subtracts `tbl_NewProduct.Quantity` when `docType != "Delivery Challan"` (business_process/09).
- Delivery Challan skip is an implementation defect, not an approved exception (business_process/09).
- Manual `Invoice.aspx` always runs the subtraction (business_process/09).
- Stock update: `UPDATE tbl_NewProduct SET Quantity = CAST(... AS DECIMAL(18,2)) - @Qty WHERE ProductID = @TruePID AND CompanyID = @CompanyID` (business_process/09).
- `Quantity_Num` and `tbl_stock` are not in the statement (business_process/09).
- No `Quantity >= @Qty` predicate (business_process/09).
- Block and delete do not restore quantity (business_process/09).
- Delete does not check for payments (business_process/09).

**Pending decisions:**
- P08: Customer-return linkage, inspection, and disposition workflow.

---

## 9. Negative Stock and Adjustments

| Concept | Approved Rule | Repository Evidence |
|---------|---------------|---------------------|
| **Normal issue cannot reduce usable stock below zero without approval** | Normal issue cannot reduce usable stock below zero without approval. | Section 6: Normal issue cannot reduce usable stock below zero without approval. |
| **Every transaction creating/ increasing negative balance requires individual approval** | Every transaction that creates or increases a negative balance requires individual approval. | Section 6: Every transaction that creates or increases a negative balance requires individual approval. |
| **Approval is role-based** | Approval is role-based. | Section 6: Approval is role-based. |
| **No automatic negative-stock limits or pre-approved allowances** | No automatic negative-stock limits or pre-approved allowances. | Section 6: No automatic negative-stock limits or pre-approved allowances. |
| **Approval must occur before posting** | Approval must occur before posting. | Section 6: Approval must occur before posting. |
| **Approval cannot bypass company/product/warehouse ownership validation** | Approval cannot bypass company/product/warehouse ownership validation. | Section 6: Approval cannot bypass company/product/warehouse ownership validation. |
| **Every manual stock increase/decrease requires authorization** | Every manual stock increase/decrease requires authorization before posting. | Section 6: Every manual stock increase/decrease requires authorization before posting. |
| **Audit history retained permanently** | Audit history is retained permanently, subject to applicable retention/archival policy. | Section 6: Audit history is retained permanently, subject to applicable retention/archival policy. |
| **Maker-checker applies; no self-approval** | Maker-checker applies; no self-approval. | Section 6: Maker-checker applies; no self-approval. |
| **If any invoice line requires negative-stock approval, entire invoice inventory posting remains atomic** | If any invoice line requires negative-stock approval, the entire invoice inventory posting remains atomic until all approvals are obtained. No partial posting. | Section 6: If any invoice line requires negative-stock approval, the entire invoice inventory posting remains atomic until all approvals are obtained. No partial posting. |

**Repository evidence:**
- `UpdateStock` matches `tbl_NewProduct.ProductID` with no company filter (business_process/02).
- Whether `ProductID` is unique across companies is PENDING VERIFICATION (business_process/02).
- Zero affected stock rows do not fail the delete (business_process/02).
- Negative stock is not blocked (business_process/02).
- Rows where `Quantity` is not numeric are not updated by the delete or by `AdjustStockDelta` (business_process/02).
- The authoritative stock balance and the read path that uses it remain PENDING VERIFICATION (business_process/02).

**Pending decisions:**
- P01: Valuation method assignment by product/category (though this is primarily a valuation section concern).
- P09: Parallel legacy/new movement ownership and synchronization.
- P11: Roles approving negative stock, adjustments, reversals, and transfer closure.

---

## 10. Product, UOM, and Tracking

| Concept | Approved Rule | Repository Evidence |
|---------|---------------|---------------------|
| **All tangible products are inventory-managed; services automatically excluded** | All tangible products are inventory-managed; services are automatically excluded. | Section 9: All tangible products are inventory-managed; services are automatically excluded. |
| **Each product has a base UOM** | Each product has a base UOM. | Section 9: Each product has a base UOM. |
| **Purchasing/sales UOM may differ through configured conversions** | Purchasing/sales UOM may differ through configured conversions. | Section 9: Purchasing/sales UOM may differ through configured conversions. |
| **Convert all movements to base UOM before posting** | Convert all movements to base UOM before posting. | Section 9: Convert all movements to base UOM before posting. |
| **Fractional precision configurable by product UOM** | Fractional precision is configurable by product UOM. | Section 9: Fractional precision is configurable by product UOM. |
| **Batch/lot tracking with expiry and serial number** | Support batch/lot tracking with expiry and serial-number tracking. | Section 9: Support batch/lot tracking with expiry and serial-number tracking. |
| **Product tracking configuration determines applicable tracking mode** | Product tracking configuration determines applicable tracking mode. | Section 9: Product tracking configuration determines applicable tracking mode. |
| **Batch and serial tracking simultaneously** | Whether one product can simultaneously use both tracking modes remains pending (P02). | Pending decision P02. |
| **Inactive products require authorization for exceptional transactions** | Inactive products require authorization for exceptional transactions; historical movements remain accessible. | Section 9: Inactive products require authorization for exceptional transactions. |
| **Product identity globally unique, consistent with ADR-002** | Product identity is globally unique, consistent with ADR-002. | Section 9: Product identity is globally unique, consistent with ADR-002. ADR-002 D4. |
| **Product-master edits must not silently modify inventory balances** | Product-master edits must not silently modify inventory balances. | Section 9: Product-master edits must not silently modify inventory balances. |

**Repository evidence:**
- Current product catalog uses `tbl_NewProduct` and `tbl_NewparentProduct`; legacy `tbl_Product`/`tbl_parentProduct` also exist (SOLUTION_INDEX.md, OWNERSHIP_AUDIT.md).
- `tbl_NewProduct` has 1441 rows, all company 1 (ADR-002 §2.1).
- `tbl_NewparentProduct` has 24 rows, all company 1 (ADR-002 §2.1).
- `Stores` table has 3 rows, no CompanyID (ADR-002 §2.1).
- `tbl_stock` has no CompanyID; grain not enforced; rows without store; orphan products (ADR-002 §2.1, OWNERSHIP_AUDIT.md).
- `tbl_StockAdjustments` has `ProductID` but no store column and no CompanyID (OWNERSHIP_AUDIT.md).
- Existing repository evidence describes implementation as it exists; must not be rewritten to pretend approved target behavior is already implemented.

**Pending decisions:**
- P02: Whether a product can use batch and serial tracking simultaneously.

---

## 11. Valuation

| Concept | Approved Rule | Repository Evidence |
|---------|---------------|---------------------|
| **Hybrid inventory valuation approved** | Hybrid inventory valuation is approved. | Section 11: Hybrid inventory valuation is approved. |
| **FIFO, Weighted Average Cost, Standard Cost supported candidates** | FIFO, Weighted Average Cost, and Standard Cost are supported candidates. | Section 11: FIFO, Weighted Average Cost, and Standard Cost are supported candidates. |
| **Method assignment by product/category pending** | Method assignment by product/category remains pending. | Section 11: Method assignment by product/category remains pending. Pending decision P01. |
| **Physical quantity and valuation separate** | Physical quantity and valuation are separate. | Section 11: Physical quantity and valuation are separate. |
| **Effective date of method changes pending** | Effective date of method changes remains pending. | Section 11: Effective date of method changes, inter-company transfer costing, landed cost/freight/tax allocation, standard-cost variance, and accounting integration remain pending. |
| **Inter-company transfer costing pending** | Inter-company transfer costing remains pending. | Section 11: Effective date of method changes, inter-company transfer costing, landed cost/freight/tax allocation, standard-cost variance, and accounting integration remain pending. |
| **Landed cost/freight/tax allocation pending** | Landed cost/freight/tax allocation remains pending. | Section 11: Effective date of method changes, inter-company transfer costing, landed cost/freight/tax allocation, standard-cost variance, and accounting integration remain pending. |
| **Standard-cost variance pending** | Standard-cost variance remains pending. | Section 11: Effective date of method changes, inter-company transfer costing, landed cost/freight/tax allocation, standard-cost variance, and accounting integration remain pending. |
| **Accounting integration pending** | Accounting integration remains pending. | Section 11: Effective date of method changes, inter-company transfer costing, landed cost/freight/tax allocation, standard-cost variance, and accounting integration remain pending. |

**Repository evidence:**
- ADR-002 INV-Q1: Product master quantity is not inventory authority (S2). Same test as INV-K5.
- Current implementation writes to `tbl_NewProduct.Quantity` and `tbl_NewProduct.Quantity_Num` and `tbl_stock.Quantity` via separate paths.
- The authoritative balance and read path remain PENDING VERIFICATION.
- No single canonical valuation method is assigned by product or category.

**Pending decisions:**
- P01: Valuation method assignment by product/category.

---

## 12. Opening Stock and Migration

| Concept | Approved Rule | Repository Evidence |
|---------|---------------|---------------------|
| **Hybrid physical verification** | Use hybrid physical verification; perform physical counts wherever possible. | Section 7: Use hybrid physical verification; perform physical counts wherever possible. |
| **Legacy quantities are reconciliation references** | Legacy quantities are reconciliation references, not automatically accepted opening balances. | Section 7: Legacy quantities are reconciliation references, not automatically accepted opening balances. |
| **Verification grain is Company + Product + Warehouse** | Verification grain is Company + Product + Warehouse. | Section 7: Verification grain is Company + Product + Warehouse. |
| **Physical variances require investigation and approval** | Physical variances require investigation and approval. | Section 7: Physical variances require investigation and approval. |
| **Approved opening balances post through centralized posting** | Approved opening balances post through centralized posting. | Section 7: Approved opening balances post through centralized posting. |
| **Migrate historical movements selectively** | Migrate historical movements selectively where reliable and justified; retain legacy records for reference. | Section 7: Migrate historical movements selectively where reliable and justified. |
| **Legacy and new systems run in parallel** | Parallel operation requires explicit movement ownership, duplicate-post prevention, reconciliation, exception reporting, and authorized correction. | Section 7: Parallel duration and synchronization mechanism remain pending. |
| **Unresolved exceptions may carry through controlled approval** | Unresolved exceptions may carry through controlled approval, with affected inventory restricted. | Section 7: Unresolved exceptions may carry through controlled approval, with affected inventory restricted. |
| **New system becomes sole posting authority only after reconciliation** | New system becomes sole posting authority only after reconciliation, exception review, opening-balance approval, and cutover approval. | Section 7: New system becomes sole posting authority only after reconciliation, exception review, opening-balance approval, and cutover approval. |

**Repository evidence:**
- `newproduct_master` hardcodes store `STR001` and `Quantity_Num` at default 0 (GATE2_DEPENDENCY_IMPACT_REPORT.md §M-06).
- Product opening stock hardcodes `STR001` / `"Central Warehouse"` (business_process/02, business_process/01).
- If the purchase store differs, create inserts a second stock row and still adds to the product total (business_process/02).
- `ShippedToLoc` on the detail is the dropdown value; a renamed or mismatched store does not reverse the row (business_process/02).
- Whether one product has one stock row per store is PENDING VERIFICATION (business_process/02).
- `public static DataTable Dt` on the edit page is shared mutable state (business_process/02).

**Pending decisions:**
- P07: PO closure behavior with unreceived quantities.
- P09: Parallel legacy/new movement ownership and synchronization.
- P10: Controlled migration exception approval and go-live criteria.

---

## 13. Availability and Reporting

| Concept | Approved Rule | Repository Evidence |
|---------|---------------|---------------------|
| **Inventory maintained by Company + Product + Warehouse** | Inventory is maintained by Company + Product + Warehouse. | Section 8: Inventory is maintained by Company + Product + Warehouse. |
| **Usable stock available for normal issue** | Usable stock is available for normal issue. | Section 8: Usable stock is available for normal issue. |
| **Quarantine excluded from usable stock** | Quarantine is excluded from usable stock. | Section 8: Quarantine is excluded from usable stock. |
| **Transit tracked separately and unavailable for normal issue** | Transit is tracked separately and unavailable for normal issue. | Section 8: Transit is tracked separately and unavailable for normal issue. |
| **Reservation management deferred** | Reservation management is deferred from initial release; no reservation deduction. | Section 8: Reservation management is deferred from initial release; no reservation deduction. |
| **Availability uses canonical inventory balance** | Availability uses canonical inventory balance, not legacy quantity columns. | Section 8: Availability uses canonical inventory balance, not legacy quantity columns. |
| **Warehouse-level and company-level views according to permissions** | Provide warehouse-level and consolidated company-level views according to permissions. | Section 8: Provide warehouse-level and consolidated company-level views according to permissions. |
| **Historical movement access is company-scoped** | Historical movement access is company-scoped. | Section 8: Historical movement access is company-scoped. |
| **Required reports** | Balance, ledger, valuation, reconciliation, aging/slow-moving, transit, quarantine/rejected stock. | Section 8: Required reports: balance, ledger, valuation, reconciliation, aging/slow-moving, transit, quarantine/rejected stock. |

**Repository evidence:**
- `Product_stock` page uses `sp_SearchProductsFast`, `sp_GetProductStockByStore`, `sp_GetProductCategories` (SHARED_RUNTIME.md, DATA_DICTIONARY.md).
- `Service_stock`: `tbl_stock` where `Product_id NOT LIKE 'P%'` (excludes `PRD*` / `PRO*` product stock). Unscoped (DOMAIN_reports.md).
- `vw_StockReconciliation` views `tbl_NewProduct.Quantity_Num` vs `tbl_stock` aggregates (SHARED_SCHEMA.md).
- Various audit tables exist: `tbl_stock_fix_audit`, `tbl_StockReassignmentAudit`, `tbl_StockReconciliationAudit` (UAT_CATALOG.md).
- Reports: balance, ledger, valuation, reconciliation, aging/slow-moving, transit, quarantine/rejected stock are identified as required but may not all be implemented.

**Pending decisions:** None specific to this section beyond general governance.

---

## 14. Governance and Audit

| Concept | Approved Rule | Repository Evidence |
|---------|---------------|---------------------|
| **Role-based permissions within each company** | Role-based permissions apply within each company. | Section 11: Role-based permissions apply within each company. |
| **Maker-checker applies; no self-approval** | Maker-checker applies; no self-approval for stock adjustment or negative override. | Section 6, Section 11: Maker-checker applies; no self-approval for stock adjustment or negative override. |
| **Posted movements are immutable** | Posted movements are immutable; corrections use reversal/repost. | Section 11: Posted movements are immutable; corrections use reversal/repost. |
| **Prevent duplicate posting** | Prevent duplicate posting through unique source references and idempotent posting. | Section 11: Prevent duplicate posting through unique source references and idempotent posting. |
| **Audit includes original/changed values, source, actor, approver, timestamps, reversal references** | Audit includes original/changed values, source, actor, approver, timestamps, and reversal references. | Section 11: Audit includes original/changed values, source, actor, approver, timestamps, and reversal references. |
| **Failed posting must not leave partial inventory changes** | Failed posting must not leave partial inventory changes. | Section 11: Failed posting must not leave partial inventory changes. |
| **Movements must be traceable and reconcilable** | Movements must be traceable and reconcilable. | Section 11: Movements must be traceable and reconcilable. |
| **Cross-company inventory access is restricted** | Cross-company inventory access is restricted. | Section 11: Cross-company inventory access is restricted. |

**Repository evidence:**
- `CompanyID` is passed on source search, line load, invoice header/details, stock update, and addresses on add and manual pages (business_process/09).
- Block search and update do not pass `CompanyID` (business_process/09).
- Delete search and deletes do pass `CompanyID` (business_process/09).
- Quotation line load does not add `IsLatest`, so older versions with the same company id are included (business_process/09).
- `public static DataTable Dt` on the edit page is shared mutable state shared by later requests and sessions in that process (business_process/02).
- No `CompanyID` predicate or column appears in the purchase insert, `UpdateStock`, edit search, line update, `AdjustStockDelta`, or delete (business_process/02).
- Vendor, category, and product lookups on purchase pages are also unscoped (business_process/02).

**Pending decisions:**
- P11: Roles approving negative stock, adjustments, reversals, and transfer closure.

---

## 15. Pending-Decision Register

| ID | Decision Question | Current Status |
|----|-------------------|----------------|
| P01 | Valuation method assignment by product/category. | Pending; no assignment in repository. |
| P02 | Whether a product can use batch and serial tracking simultaneously. | Pending; repository does not establish this fact. |
| P03 | Inter-company ownership-change point: dispatch or destination receipt. | Pending; ADR-002 does not specify. |
| P04 | Inter-company transfer documents and approval workflow. | Pending; not documented in repository. |
| P05 | Exact over-receipt approver roles/evidence. | Pending; Section 4 says "Exact approver roles remain pending." |
| P06 | Quarantine release authorization. | Pending; not documented in repository. |
| P07 | PO closure behavior with unreceived quantities. | Pending; not documented in repository. |
| P08 | Customer-return linkage, inspection, and disposition workflow. | Pending; not documented in repository. |
| P09 | Parallel legacy/new movement ownership and synchronization. | Pending; Section 7 says "Parallel duration and synchronization mechanism remain pending." |
| P10 | Controlled migration exception approval and go-live criteria. | Pending; not documented in repository. |
| P11 | Roles approving negative stock, adjustments, reversals, and transfer closure. | Pending; Section 6 says "Approval is role-based" but exact roles not specified. |

---

## 16. ADR-002 Governance Conflict — Must Be Explicit

The existing ADR-002 is ratified and normative. It must not be silently overwritten or reinterpreted.

**ADR-002 establishes (ratified 2026-10-01):**
- D4: Product identity is enterprise-wide unique.
- D5: Every physical inventory quantity belongs to a Store/Warehouse.
- D7: Every tenant-owned transaction root carries direct CompanyID.
- S1: `tbl_stock` is the sole authoritative physical-stock ledger.
- S2: `tbl_NewProduct.Quantity` and `Quantity_Num` are not authoritative physical stock.
- A1-2: `tbl_StockAdjustments` is a tenant-owned movement on shared stock.
- A1-3: `tbl_stock.Quantity_Num` is the sole authoritative physical quantity.
- Product, Category, Store, and stock identity are shared; target stock grain is ProductID + StoreID; shared stock carries no CompanyID.

**Approved business-logic baseline describes:**
- CompanyStore mapping: a company to its authorized warehouses.
- Company + Product + Warehouse inventory balance.
- Company-exclusive warehouse ownership: each physical warehouse belongs exclusively to one company.
- Inter-company transfers change ownership; not an ordinary warehouse transfer.

**These concepts must be formally reconciled against ADR-002 before architecture ratification or implementation:**
- ADR-002 declares shared master data (Product, Category, Store, Stock) carries no CompanyID.
- The approved baseline describes company-exclusive warehouse ownership and CompanyStore mapping.
- These are not inherently contradictory but require formal mapping: how CompanyStore coexists with ADR-002's shared-store invariants (INV-K2, INV-K4) must be decided.
- `tbl_stock` currently has no CompanyID and no enforced unique Product + Store key.
- Existing `tbl_stock` rows include blank store identifiers and duplicate product/store groups.
- `tbl_StockAdjustments` currently has `ProductID` but no store column and no CompanyID.
- Existing product catalog uses `tbl_NewProduct` and `tbl_NewparentProduct`; legacy `tbl_Product`/`tbl_parentProduct` also exist.
- **Do not modify ADR-002, OWNERSHIP_AUDIT.md, or UAT_MIGRATION_GOVERNANCE.md.**

**Required reconciliation items (not resolved by this document):**
- How CompanyStore authorized-warehouse mapping interfaces with ADR-002's shared Store identity (INV-S1, INV-K2).
- Whether company-exclusive warehouse ownership means a warehouse's StoreID is implicitly filtered by the working company, or whether the StoreID itself carries no CompanyID and enforcement happens at the application level.
- How the `tbl_stock` grain of ProductID + StoreID interacts with company-scoped access when `tbl_stock` has no CompanyID.
- How `tbl_StockAdjustments` as TRANSACTION_OWNED (A1-2) with direct CompanyID interfaces with shared `tbl_stock` updates (A1-3).

---

## 17. Section-by-Section Review and Change-Control Policy

### Change-Control Policy

Each section of this document is reviewed, validated, and implemented only within its approved scope. The following policies apply:

| Policy | Description |
|--------|-------------|
| **Section-level review** | Each section is independently reviewed against the approved business-logic baseline and repository evidence. |
| **Cross-section impacts identified** | Any cross-section impact is explicitly identified. Changes to one section may require validation of related sections. |
| **No silent modification of approved decisions** | Never silently modify an already approved business decision. All changes must be recorded as proposals with impact and approval status. |
| **New requirements as proposals** | New requirements must be recorded as proposals with impact assessment and approval status. They are not assumed or inferred. |
| **Repository owner approval for cross-section scope changes** | Any cross-section scope change requires repository-owner approval. |
| **Implementation not granted** | This document does not authorize implementation, migration design, DDL, application changes, or database execution. |
| **ADR-002 preservation** | ADR-002, OWNERSHIP_AUDIT.md, and UAT_MIGRATION_GOVERNANCE.md must not be modified. |
| **Pending decisions remain open** | All pending decisions (P01-P11) remain open until formally resolved by repository owner or governance process. |

### Cross-Section Impact Identification

When a change affects multiple sections, the impact must be documented as follows:

1. **Identify the originating section** — which section's change triggers the cross-section impact.
2. **List affected sections** — which other sections are impacted by the change.
3. **Document the nature of the impact** — how the change affects each section's approved rules.
4. **Record approval status** — whether each affected section's approved rules need modification, and by whom.

### Example Change-Control Record

| Change | Originating Section | Affected Sections | Impact | Approval Status |
|--------|--------------------|-------------------|--------|-----------------|
| Add CompanyID to tbl_stock | Section 2 (Ownership) | Sections 4, 6, 8, 11 | Changes stock grain enforcement, adjustment ownership, audit trail, access control | Requires repository-owner approval; pending (P03, P11) |

---

## 18. Implementation Authorization Boundary

This document establishes the **architecture baseline** but does not authorize any of the following:

- **Implementation**: No application code, page changes, or business-logic implementation is authorized.
- **Migration design**: No DDL, schema changes, data migration scripts, or database execution is authorized.
- **Database execution**: No stored procedures, triggers, or direct database manipulation is authorized.
- **ADR-002 modification**: The ADR-002 governance document must not be silently overwritten or reinterpreted.
- **OWNERSHIP_AUDIT.md modification**: The ownership audit document must not be modified.
- **UAT_MIGRATION_GOVERNANCE.md modification**: The UAT migration governance document must not be modified.
- **Pending decision resolution**: Pending decisions (P01-P11) must be resolved through the formal governance process, not by inference or assumption in this document.
- **Implementation of approved rules**: The approved business rules documented herein serve as the target architecture; their implementation is a separate process that requires formal authorization outside this document.

**The implementation boundary is defined by:**
- This document is read-only for the purposes of architecture review.
- All implementation must follow the approved business-logic baseline as validated through the governance gates (Gate 1 complete, Gate 2 not complete, Gate 3 not authorized).
- The target branch is `July_to_Sept26_DevNSupport` at the architecture freeze commit `0c5cc72`.
- Any implementation must first complete the unresolved Gate 2 criteria and gain Gate 3 approval.

---

## Revision History

| Date | Version | Description |
|------|---------|-------------|
| 2026-11-07 | v1.0 | Initial baseline reconstruction from approved business-logic decisions and repository evidence. |

**File location:** `docs/architecture/FLMX_Inventory_Management_Architecture_v1.0.md`
**Target branch:** `July_to_Sept26_DevNSupport`
**Status:** Baseline for Section-by-Section Architecture Review
**Implementation Authorization:** NOT GRANTED