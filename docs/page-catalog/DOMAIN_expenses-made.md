# Payments made (GL expenses)

> Domain key: `expenses-made` · 7 page(s)

General and petty-cash expense vouchers — not field-visit expenses.

**Do not copy** AuthN/AuthZ, tenancy, or Ponytail rules here. Those live in [SHARED_CONTEXT](SHARED_CONTEXT.md).

| Page | Purpose | Master / class | Gate | Tables (unique) | Ops | Narrative |
|------|---------|----------------|------|-----------------|-----|-----------|
| `corporate/business/app/Delete_general_expencess.aspx` | Delete General Expenses | Bill.Master / `WebForm45` | SecurePage, CompanyID<br>`Delete_general_expencess` | `tbl_Expences`, `tbl_Tieup_Company` | DELETE | this catalog |
| `corporate/business/app/Delete_patty_cash_expenses.aspx` | Delete Petty Cash Expenses | Bill.Master / `WebForm58` | SecurePage, CompanyID<br>`Delete_patty_cash_expenses` | `tbl_Tieup_Company`, `tbl_patty_cash_expenses` | UPDATE, DELETE | this catalog |
| `corporate/business/app/Expenses_Head.aspx` | Expense Head | Bill.Master / `WebForm42` | SecurePage<br>`Expenses_Head` | `tbl_Expences` | INSERT, DELETE | this catalog |
| `corporate/business/app/general_expences.aspx` | General Expenses | Bill.Master / `WebForm43` | SecurePage, CompanyID<br>`general_expences` | `tbl_Expences`, `tbl_Tieup_Company` | INSERT | this catalog |
| `corporate/business/app/patty_cash_expences.aspx` | Petty Cash Expences | Bill.Master / `WebForm56` | SecurePage, CompanyID<br>`patty_cash_expences` | `tbl_Expences`, `tbl_Tieup_Company`, `tbl_patty_cash_expenses` | INSERT, UPDATE | this catalog |
| `corporate/business/app/view_expencess_head.aspx` | View General Expenses | Bill.Master / `WebForm44` | SecurePage<br>`view_expencess_head` | `tbl_Expences` | SELECT | this catalog |
| `corporate/business/app/view_patty_cash_expenses.aspx` | View Petty Cash Expenses | Bill.Master / `WebForm57` | SecurePage, CompanyID<br>`view_patty_cash_expenses` | `tbl_Tieup_Company`, `tbl_patty_cash_expenses` | SELECT | this catalog |

<!-- NARRATIVE:BEGIN -->

## Not visit expenses

Field-visit claims are `expense_entry` → `tbl_Expenses` ([DOMAIN_sales-visit.md](DOMAIN_sales-visit.md)). This domain is GL “Payments Made”.

Heads: `Expenses_Head` CRUD `tbl_Expences.Expencess_Name` (control `txtCityName`).

General: INSERT `tlb_General_expences` (`pament_made_id`, cheque fields). View/delete print `General_expencess_voutcher.aspx?pament_made_id=` — **fail-closed**.

Petty cash: INSERT `tbl_patty_cash_expenses`; UPDATE `tlb_closing_balance` via `cmbcashstatus` in/out. Print `Patty_cash_expencess_voutcher.aspx?payment_id=` — **fail-closed**. Delete rebalances later rows’ `closing_balance`.

Spellings: `tlb_` vs `tbl_`, expencess, pament, patty, voutcher. UI says Petty.
