# Phase 2C — Remaining Resource Hardening

**Date:** 2026-09-07  
**Depends on:** PR #66–#71. Authentication, membership, `SecurePage` keys, `UserRoles`, and visit owner/manager primitives from Phase 2B are not redesigned.  
**Policy honored:** Decision #8 (`ReportingManagerId`) and Decision #9 (HQ cross-company) remain STOP. Directory delete is **company + page permission**, not owner-based (no owner column is used on these pages).

---

## Before / after

| Surface | Before | After |
|---------|--------|--------|
| `Delete_client` | Concatenated `Client_Id`; all companies listed | Parameterized; list/delete `CompanyID`; `AuthGuard.ClientInCurrentCompany` before DELETE |
| `Delete_vendor` | Concatenated `Vendor_Id`; all companies listed | Parameterized; list/delete `CompanyID`; `AuthGuard.VendorInCurrentCompany` before DELETE |
| `Create_quotation?visitId=` | `CompanyID` on visit SELECT; hidden field set from query string | `UserCanViewVisit` before storing `hfVisitId` and before INSERT `VisitId` |
| `AdminApprovalDashboard` | `CompanyID` on UPDATE; guessed resolved `RequestID` still ran leave-balance SQL | Pending + company EXISTS **before** the batch; UPDATE requires `RequestStatus = 'Pending'`; `@@ROWCOUNT = 0` rolls back and returns **before** leave-balance |

---

## AuthGuard additions

| Method | Meaning |
|--------|---------|
| `ClientInCurrentCompany` | `tbl_Client.Client_Id` + current `CompanyID` |
| `VendorInCurrentCompany` | `tbl_Vendor.Vendor_Id` + current `CompanyID` |
| `UserCanApprovePendingLeave` | `AdminApprovalDashboard` + pending leave in current company |
| `UserCanApprovePendingRegularization` | Same for `tbl_AttendanceRegularization` |

Visit attach on quotation reuses Phase 2B `UserCanViewVisit` (owner **or** `srch_dailyrpts` + company). `Create_quotation` remains a `Page` (not `SecurePage`) so Master can still set `CompanyID` on first GET.

---

## Resources completed

| Resource | Status |
|----------|--------|
| Delete Client (A-21) | PASS |
| Delete Vendor (A-21) | PASS |
| Create Quotation visit scope | PASS (`UserCanViewVisit`; quotation math unchanged) |
| AdminApprovalDashboard replay | PASS (pending authorize + abort before side effects) |

No new `Permissions` rows. Delete order for client (client row, then registered address) is unchanged.

---

## Remaining STOP items

| Item | Why still STOP |
|------|----------------|
| Decision #8 | Leave/attendance and manager visit approve are still **company-wide** for the page permission. `ReportingManagerId` is not an ACL. |
| Decision #9 | No HQ cross-company exception. |
| Owner-based client/vendor delete | Not in source. Company directory + `Delete_client` / `Delete_vendor` is the supported scope. |
| `Create_quotation` page RBAC | Still session + Master only (not Phase 1A `SecurePage`). Visit **attach** is scoped; creating a quote with no visit is unchanged. |
| Other concatenated ERP pages | Out of this phase (invoice/PO/stock/scheduler). |
