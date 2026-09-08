# Daily visit reports & manager approval

Canonical pages: [`page-catalog/DOMAIN_sales-visit.md`](page-catalog/DOMAIN_sales-visit.md).  
Audit snapshot vs current code: [`sales-visit-workflow-audit/00_SNAPSHOT_STATUS.md`](sales-visit-workflow-audit/00_SNAPSHOT_STATUS.md).  
Mailer path: [`docs/11_Communications.md`](11_Communications.md).

This file keeps **leftover facts** that are not in the catalog.

---

## Confirmed tables

| Table | Role |
|-------|------|
| `tbl_SalesVisitReport` | Visit header (`daily_rpt`, `vw_dailyrpts`, `srch_dailyrpts`). |
| `tbl_SalesVisitResponses` | Chat thread. |
| `tbl_Expenses` | Visit-linked claims in mega-modal. |
| `tbl_login` | Names, `ReportingManagerId`, email. |

There is no table named `daily_rpt`. That is the **page** name.

---

## Unique leftovers (do not recopy the audit)

- **D-01 (forward remediating):** new INSERTs set `CompanyID`. Historical NULL rows still miss manager/home/`Create_quotation` filters.
- **D-03 (live):** `daily_rpt` null-coalesces `CreatedByCode` to `"FLM03"`.
- **D-05 / D-04 / D-08 / D-11:** remediating or stale for these files — see `00_SNAPSHOT_STATUS.md`.
- Chat/approval mail uses **`CommunicationGateway`**. Remaining direct `SmtpClient` pages: docs/11.

HR leave approval is **`AdminApprovalDashboard`**, not `srch_dailyrpts`.
