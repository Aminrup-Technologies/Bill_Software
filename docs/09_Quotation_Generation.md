# Module 09 — Quotation Generation

> Master Page Menu Position: **Corporate → Quotations** (or linked from Sales Visit executed-visit details modal)

---

## 1. Overview

The Quotation Generation module creates customer quotations linked to sales visit records. It pre-fills the customer name from the visit's `CustomerName` field and generates a quotation document that can be printed or emailed to the customer. This module bridges the sales visit workflow with the commercial/sales pipeline.

---

## 2. Files

| File | Type | Purpose |
|------|------|---------|
| `corporate/business/app/Create_quotation.aspx` | Frontend | Quotation creation form |
| `corporate/business/app/Create_quotation.aspx.cs` | Backend | Quotation generation logic, customer name resolution from visit |

### Supporting Files

| File | Relationship |
|------|-------------|
| `corporate/business/app/visit_planner.aspx` | "📄 Generate Quote" button in Executed Visit Details modal navigates to `Create_quotation.aspx?visitId=` |
| `corporate/business/app/daily_rpt.aspx[.cs]` | Source of `CustomerName` data (free text, not from customer directory) |
| `DB_UTILITY.cs` | Database connection utilities |

---

## 3. Core Database Tables

| Table | Usage in This Module |
|-------|---------------------|
| `tbl_SalesVisitReport` | `SELECT` — resolves `CustomerName` from the linked visit |
| Quotation table(s) (name not confirmed from audit scope) | `INSERT` — quotation record creation |
| `tbl_login` | User identity for quotation author |

### Key Query

```sql
SELECT CustomerName
FROM tbl_SalesVisitReport
WHERE Id = @Id AND CompanyID = @CompanyID
```

This is one of the **correctly tenant-scoped** queries in the codebase — it includes both `Id` and `CompanyID` in the `WHERE` clause, following the "Full-Stack CompanyContext segregation fix" pattern.

---

## 4. Multi-Tenant Constraints

| Constraint | Status | Evidence |
|-----------|--------|----------|
| `CompanyID` filter on customer name resolution | ✅ Enforced | `WHERE Id=@Id AND CompanyID=@CompanyID` |
| `CompanyID` on quotation INSERT | ✅ Expected | Follows the segregation fix pattern |

### Tenant Isolation Pattern

This module correctly implements the full `CompanyContext.CurrentCompanyID` pattern. The customer name lookup includes both the visit ID and the company ID, preventing cross-tenant quotation generation.

### Impact of D-01

Despite the correct query pattern, this module is **adversely affected** by Defect D-01: because `CompanyID` is never populated on Sales Visit INSERTs (in `daily_rpt.aspx` and `visit_planner.aspx`), the `WHERE CompanyID=@CompanyID` filter will return **zero rows** for any visit created by the Sales Visit workflow. The "📄 Generate Quote" button linked from the executed visit details modal will silently fail to pre-fill the customer name on every quote generated from a sales visit.

---

## 5. Proactive Notification Triggers

| Trigger | Table | Description |
|---------|-------|-------------|
| Quotation creation | `tbl_SystemNotification` | Audit notification logged prior to transaction commit |

---

## 6. Architectural Notes

- The quotation module is one of the **best-implemented** modules in terms of tenant isolation — it was part of the "Full-Stack CompanyContext segregation fix" effort referenced in code comments.
- The module's effectiveness is undermined by the D-01 defect upstream: quotations cannot be generated from sales visits because the `CompanyId` filter finds no matching rows.
- Quotation data likely flows into downstream modules (printing, PDF generation, email delivery) that were not directly analyzed in the Sales Visit Workflow Audit.
