# Module 05 — Customer & Vendor Directory

> Master Page Menu Position: **Corporate → Customers** / **Corporate → Vendors** (or similar navigation item)

---

## 1. Overview

The Customer & Vendor Directory module maintains the master records for customers and vendors that are referenced throughout the ERP system — in quotations, purchase orders, sales visits, and communications. Customer names entered in sales visit forms are free-text denormalized copies of records from this directory.

---

## 2. Files

| File | Type | Purpose |
|------|------|---------|
| Corporate business pages (inferred) | Frontend/Backend | Customer and vendor CRUD operations |

> **Note:** Specific `.aspx`/`.aspx.cs` file names for this module were not directly referenced in the Sales Visit Workflow Audit. File paths are inferred from the overall application structure and the `Create_quotation.aspx` cross-reference, which reads `CustomerName` from `tbl_SalesVisitReport`.

### Supporting Files

| File | Relationship |
|------|-------------|
| `corporate/business/app/Create_quotation.aspx[.cs]` | References `CustomerName` from visit records; likely links to the customer directory |
| `corporate/business/app/daily_rpt.aspx[.cs]` | Customer name is entered as free text during visit creation (not selected from a dropdown — a known data-quality concern) |
| `corporate/business/app/srch_dailyrpts.aspx[.cs]` | Displays `CustomerName` in search results |

---

## 3. Core Database Tables

| Table | Usage in This Module |
|-------|---------------------|
| Customer master table (name inferred, likely `tbl_Customers` or similar) | Customer directory CRUD |
| Vendor master table (name inferred, likely `tbl_Vendors` or similar) | Vendor directory CRUD |
| `tbl_SalesVisitReport` | Contains `CustomerName` as denormalized free text — **not** a FK to the customer directory |

### Data Quality Concern

The Sales Visit workflow writes customer names as **free text** (`CustomerName` column) rather than selecting from the customer directory. This means:
- Visit records can contain misspelled, inconsistent, or fabricated customer names.
- The customer directory and visit records are **not referentially linked** — there is no FK from `tbl_SalesVisitReport.CustomerName` to the customer directory.
- Quotation generation (`Create_quotation.aspx.cs`) reads the customer name from the visit record, not from the directory, propagating any data-quality issues.

---

## 4. Multi-Tenant Constraints

| Constraint | Status | Evidence |
|-----------|--------|----------|
| `CompanyID` scoping on customer/vendor records | ⚠️ Inferred but not confirmed from audit scope | Customer/vendor directories in multi-tenant ERPs are typically tenant-scoped, but this was not directly verified in the Sales Visit Workflow Audit |

---

## 5. Proactive Notification Triggers

| Trigger | Table | Description |
|---------|-------|-------------|
| Customer/Vendor creation or modification | `tbl_SystemNotification` | Audit logging expected on directory record changes |

---

## 6. Architectural Notes

- The disconnect between the customer directory and the `SalesVisitReport.CustomerName` free-text field is a systemic data-quality issue across the application.
- A future improvement would be to replace the free-text `CustomerName` with a FK reference to the customer directory, with autocomplete/dropdown selection during visit creation.
- Purchase order and quotation modules likely have tighter integration with the customer/vendor directory than the Sales Visit workflow does.
