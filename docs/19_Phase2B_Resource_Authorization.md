# Phase 2B — Resource-Level Authorization

**Date:** 2026-09-07  
**Depends on:** PR #66 / #67 / #68 / #69 / #70. Authentication, `UserRoles` / `RolePermissions` / `Permissions`, `UserCompanyAccess`, `SecurePage` lifecycle, and login/ActiveSessions are not changed.  
**Policy honored:** Decision #8 and Decision #9 are **not** resolved in code. `ReportingManagerId` is not used as an authorization predicate.

---

## Resource-scope model

```
AUTHENTICATION (tbl_login + ActiveSessions)
    → COMPANY MEMBERSHIP (UserCompanyAccess + Session["CompanyID"])
    → PAGE PERMISSION (existing PermissionKey)
    → RESOURCE SCOPE (owner or company-manager-dashboard)
    → BUSINESS OPERATION
```

| Scope | Existing evidence | Enforcement |
|-------|-------------------|-------------|
| **Owner** | `visit_planner` calendar and `vw_dailyrpts` lists use `CreatedByCode = Session["USERID"]` + `CompanyID` | Execute, owner detail, owner edit, owner chat, expense entry |
| **Company manager dashboard** | `srch_dailyrpts.Binder` lists **all** visits in current `CompanyID`; employee dropdown is all active company users | View/chat/approve on that page via `srch_dailyrpts` permission + visit in current company |
| **Reporting line** | `ReportingManagerId` is email/display only (`docs/14` §5.4) | **Not used.** Decision #8 STOP |
| **HQ / cross-company** | No role in these pages grants another `CompanyID` | **Not used.** Decision #9 STOP |

VIEW (`srch_dailyrpts`) is not treated as owner EDIT (`visit_planner` / `vw_dailyrpts`). Approve uses the manager-dashboard key, not the salesperson keys.

---

## Before / after

| Surface | Before | After |
|---------|--------|--------|
| `GetVisitDetails` | `Id` + `CompanyID` | Owner (`CreatedByCode`) + company; empty `{}` if not owner |
| Execute visit | `WHERE Id = @Id` | Owner + `CompanyID` on UPDATE and follow-up SELECT |
| `vw_dailyrpts` modal / edit / chat | CompanyID (edit) or VisitId only (chat) | Owner + company; chat insert refused if not owner |
| `expense_entry?visitId=` | CompanyID on the visit | Owner + company; hidden VisitId re-checked on INSERT |
| `srch_dailyrpts` list | Company-wide (unchanged) | Unchanged (Decision #8) |
| Visit approve | `WHERE Id = @Id` | `srch_dailyrpts` + visit in current company + pending-or-null status |
| Expense approve | `WHERE Id = @Id` | Expense linked to the modal visit **and** that visit’s `CompanyID` |
| `AdminApprovalDashboard` | Company-wide pending + `CompanyID` on UPDATE | Unchanged SQL (see STOP) |

---

## AuthGuard primitives

| Method | Meaning |
|--------|---------|
| `CurrentUserId()` | `Session["USERID"]` or null |
| `VisitBelongsToCurrentCompany` | Visit `Id` + current `CompanyID` |
| `UserOwnsVisit` | Same plus `CreatedByCode` |
| `UserCanViewVisit` | Owner **or** (`srch_dailyrpts` and company visit) |
| `UserCanEditOwnVisit` | Owner only |
| `UserCanManageCompanyVisits` | `srch_dailyrpts` after session + membership |
| `UserCanApproveVisit` | Manage key + company visit. **Not** reporting-line |
| `UserCanApproveExpense` | Manage key + expense.VisitId + visit in current company |

Lookup failure returns false. No “all visits” fallback.

---

## Resources hardened

| Resource | Pages | Read | Mutation |
|----------|-------|------|----------|
| Sales visit (owner) | `visit_planner`, `vw_dailyrpts`, `daily_rpt` (create-as-self, unchanged) | Own + company | Execute/edit/chat: own + company |
| Sales visit (manager dashboard) | `srch_dailyrpts` | Company + `srch_dailyrpts` | Approve: same + pending guard |
| Visit expense | `expense_entry`, `srch_dailyrpts` | Owner for entry; manager dashboard for approve | Insert requires owned visit; approve requires linked visit in company |
| Leave / attendance approval | `AdminApprovalDashboard` | Company pending queue (existing) | Existing `CompanyID` UPDATE only |

No new `Permissions` rows.

---

## STOP register

### Decision #8 — `ReportingManagerId` vs company-wide manager access

| | |
|--|--|
| **Resource** | Manager dashboard visits/expenses (`srch_dailyrpts`); leave/attendance (`AdminApprovalDashboard`) |
| **Current behavior** | Anyone with the page permission sees/approves **all current-company** pending work. `ReportingManagerId` is not in the WHERE clause. |
| **Missing rule** | Should a manager be limited to direct reports? |
| **Boundary now** | `CompanyID` + page `PermissionKey`. Cross-company IDs are rejected. |
| **Decision required** | Product must choose Variant A (company-wide, current lists) vs Variant B (join `CreatedByCode` → `tbl_login.ReportingManagerId`). Do not code Variant B until that answer exists. |

### Decision #9 — HQ / cross-company approval

| | |
|--|--|
| **Resource** | Any tenant-scoped visit/expense |
| **Current behavior** | `Session["CompanyID"]` after Phase 2A membership is the tenant. |
| **Missing rule** | Is there an HQ role that may act in another company without switching membership? |
| **Boundary now** | No cross-company path. Membership remains the tenant ACL. |
| **Decision required** | Confirm no HQ exception before adding one. |

### A-21 — `Delete_client` / `Delete_vendor`

| | |
|--|--|
| **Resource** | Client / vendor directory |
| **Current behavior** | Lists and deletes all rows; concatenated `Client_Id` / `Vendor_Id`; no `CompanyID`. |
| **Missing rule** | Company directory vs owner-of-record. Fixing it requires replacing unparameterized `DbCL` SQL, not a one-predicate patch. |
| **Boundary now** | Page permission only (Phase 1A). **Not hardened in Phase 2B.** |
| **Decision required** | Authorize parameterized `CompanyID` rewrite of list+delete as its own change. |

### `AdminApprovalDashboard` pending-ID replay

| | |
|--|--|
| **Resource** | Leave / regularization `RequestID` |
| **Current behavior** | UPDATE is company-scoped. A guessed already-resolved ID can still run the rest of the batch (leave-balance side effects). |
| **Missing rule** | Whether resolved rows may be replayed; whether approver must be `ReportingManagerId`. |
| **Boundary now** | `CompanyID` on UPDATE (pre-existing). Pending-state was **not** added: a 0-row UPDATE would still execute the approval business SQL in the same batch. |
| **Decision required** | Wrap the batch in `@@ROWCOUNT` / transaction abort before changing pending semantics. |

### `Create_quotation.aspx` visit ID

| | |
|--|--|
| **Resource** | `tbl_SalesVisitReport` via `?Id=` / load by Id |
| **Current behavior** | `CompanyID` only. |
| **Missing rule** | May a quoter use any company visit, or only owned / manager-visible visits? |
| **Boundary now** | Quotation workflow is out of Phase 2B. **Not changed.** |
| **Decision required** | Owner vs company-wide quote-from-visit. |

---

## Out of scope (unchanged)

Login, PBKDF2/reset, OTP, ActiveSessions, UserRoles/RolePermissions/Permissions schema, UserCompanyAccess, company dropdown, menu rendering, SecurePage lifecycle, print maps, scheduler, invoice/PO/stock query bodies, card kiosk.
