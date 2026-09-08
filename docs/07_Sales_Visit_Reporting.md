# Daily visit reports & manager approval

Canonical pages: [`page-catalog/DOMAIN_sales-visit.md`](page-catalog/DOMAIN_sales-visit.md).  
Defects D-01…: [`sales-visit-workflow-audit/05_Potential_Defects.md`](sales-visit-workflow-audit/05_Potential_Defects.md).  
Mailer path: [`docs/11_Communications.md`](11_Communications.md).

This file keeps **leftover facts** that are not in the catalog.

---

## Confirmed tables

| Table | Role |
|-------|------|
| `tbl_SalesVisitReport` | Visit header (create `daily_rpt`, owner list `vw_dailyrpts`, manager `srch_dailyrpts`). |
| `tbl_SalesVisitResponses` | Chat thread. |
| `tbl_Expenses` | Visit-linked claims shown in mega-modal. |
| `tbl_login` | Names, `ReportingManagerId`, email. |

There is no table named `daily_rpt`. That is the **page** name.

---

## Unique leftovers (do not recopy the audit)

- **D-01:** visit INSERT omits `CompanyID`; manager list and quote prefill filter on it.
- **D-02:** `Status` vocabulary `Pending` vs `Pending Execution` can blank on owner save.
- **D-03:** `daily_rpt` null-coalesces `CreatedByCode` to hardcoded `"FLM03"`.
- Chat/approval mail now goes through **`CommunicationGateway`** (AppSettings SMTP). The audit’s **D-11 hardcoded Zoho in these two files is stale**; see docs/11 for remaining direct `SmtpClient` pages.

HR leave approval is **`AdminApprovalDashboard`**, not `srch_dailyrpts`.
