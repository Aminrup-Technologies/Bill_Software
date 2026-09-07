# Module 10 — Purchase Order Management

> Master Page Menu Position: **Corporate → Purchase Orders** (or similar navigation item)

---

## 1. Overview

The Purchase Order Management module handles procurement workflows — creating, tracking, and managing purchase orders for goods and services. While not directly analyzed in the Sales Visit Workflow Audit, this module is part of the core ERP ecosystem and interacts with the customer/vendor directory and potentially the quotation module.

---

## 2. Files

| File | Type | Purpose |
|------|------|---------|
| Corporate business pages (inferred) | Frontend/Backend | Purchase order CRUD operations |

> **Note:** Specific `.aspx`/`.aspx.cs` file names for this module were not directly referenced in the Sales Visit Workflow Audit. File paths are inferred from the overall application structure.

---

## 3. Core Database Tables

| Table | Usage in This Module |
|-------|---------------------|
| Purchase order table(s) (name not confirmed) | PO creation and tracking |
| `tbl_login` | User identity for PO author and approver |
| Customer/Vendor directory tables | Supplier reference for PO line items |
| `tbl_SystemNotification` | Audit logging on PO status changes |

---

## 4. Multi-Tenant Constraints

| Constraint | Status | Evidence |
|-----------|--------|----------|
| `CompanyID` scoping | ⚠️ Not directly verified | Expected to follow the standard `CompanyContext.CurrentCompanyID` pattern based on other modules in the corporate business area |

---

## 5. Proactive Notification Triggers

| Trigger | Table | Description |
|---------|-------|-------------|
| PO creation/status change | `tbl_SystemNotification` | Audit notification logged |
| PO approval | `tbl_SystemNotification` + Email | Notification and email to relevant parties |

---

## 6. Architectural Notes

- Purchase orders likely follow the same ADO.NET + parameterized query patterns established throughout the application.
- The module may reference `tbl_SalesVisitReport` indirectly (e.g., linking a PO to a customer relationship established during a sales visit).
- Full documentation of this module's implementation details requires direct source code analysis beyond the Sales Visit Workflow Audit scope.
