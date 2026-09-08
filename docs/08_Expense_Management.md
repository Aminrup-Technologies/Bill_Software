# Visit expense claims

Canonical pages: [`page-catalog/DOMAIN_sales-visit.md`](page-catalog/DOMAIN_sales-visit.md) (`expense_entry`).  
GL “Payments Made” (different module): [`page-catalog/DOMAIN_expenses-made.md`](page-catalog/DOMAIN_expenses-made.md).

This file keeps **leftover facts** that are not in those catalogs.

---

## Two expense stacks (do not merge)

| Stack | Table | Pages |
|-------|-------|-------|
| Field-visit claims | `tbl_Expenses` | `expense_entry.aspx?visitId=`; mega-modal on `vw_dailyrpts` / `srch_dailyrpts` |
| GL payments made | `tlb_General_expences` / `tbl_patty_cash_expenses` / heads `tbl_Expences` | `general_expences.aspx`, `patty_cash_expences.aspx`, … |

`VisitId` on `tbl_Expenses` is **nullable** — a claim can exist without a visit. Uploads: `~/Uploads/Expenses/` (any extension; audit **D-09**).

`expense_entry` is dual-parent: `RequiredAnyPermissionKeys` = `visit_planner` **or** `vw_dailyrpts`.

Manager per-row approve/reject of visit expenses is `srch_dailyrpts` (`gvMegaExpenses_RowCommand`), not the GL delete pages.
