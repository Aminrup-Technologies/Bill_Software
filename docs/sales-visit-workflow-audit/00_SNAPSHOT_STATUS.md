# Visit audit — snapshot vs current code

The files in this folder are a **dated architecture snapshot**. They are not the page census.

Canonical current facts: [`DOMAIN_sales-visit.md`](../page-catalog/DOMAIN_sales-visit.md), leftover pointers [`docs/06`](../06_Sales_Visit_Planner.md) / [`docs/07`](../07_Sales_Visit_Reporting.md), mailers [`docs/11`](../11_Communications.md), SP index [`SHARED_RUNTIME.md`](../page-catalog/SHARED_RUNTIME.md).

Do **not** copy this folder into domain catalogs. If a D-* finding below is marked remediating, trust current `.aspx.cs` over the original defect write-up.

---

## Findings re-checked against current source

| Id | Original claim | Current code |
|----|----------------|--------------|
| **D-01** | Visit INSERT omits `CompanyID` | **Remediating (forward).** `daily_rpt` INSERT and planner follow-up INSERT set `@CompanyID`. Historical NULL rows can still miss `CompanyID` filters (home KPIs, FieldSales CTE, manager list, quote prefill). |
| **D-03** | `CreatedByCode` fallback `"FLM03"` | **Still live** in `daily_rpt.aspx.cs`. |
| **D-04** | IDOR on almost every visit mutation | **Remediating.** `AuthGuard.UserOwnsVisit` / `UserCanViewVisit` / `UserCanApproveVisit` on planner, `vw_dailyrpts`, `srch_dailyrpts`, `expense_entry`, `Create_quotation`. Company-wide manager view is permission `srch_dailyrpts` (Decision #8: not reporting-line ACL). |
| **D-05** | String-concat SQL in `Binder()` | **Remediating.** Parameterized `@CompanyID` / `@User` / `@FromDate` / `@ToDate`. |
| **D-08** | Approval UPDATE with no Pending guard | **Remediating.** `WHERE … CompanyID` and `ISNULL(ApprovalStatus,'Pending')='Pending'` plus `UserCanApproveVisit`. |
| **D-11** | Hardcoded Zoho in `vw_dailyrpts` / `srch_dailyrpts` | **Stale for those files.** They call `CommunicationGateway`. Direct `SmtpClient` leftovers: [`docs/11`](../11_Communications.md). Git history may still contain old literals. |
| **SP inventory in `02_*.md`** | “Only SP in the solution is `sp_AllocateEmployeeLeaves`” | **Stale.** Call-site index: [`SHARED_RUNTIME.md`](../page-catalog/SHARED_RUNTIME.md). Visit SQL itself is still inline. |

Not re-verified in this pass: D-02, D-06, D-07, D-09, D-10, D-12+. Treat those write-ups as **unchecked** until a new code pass, not as current confirmed bugs.

---

## What this folder is still good for

State machine vocabulary, column usage map, upload-path notes, and the original IDOR/SQL/SMTP analysis **as of the audit date**. Use it as history, then confirm against `DOMAIN_sales-visit.md` and the `.cs` files above.
