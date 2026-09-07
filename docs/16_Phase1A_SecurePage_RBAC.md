# Phase 1A — SecurePage RBAC Rollout

**Date:** 2026-09-07  
**Depends on:** PR #66 (`AuthGuard` / `SecurePage`), PR #67 (credential hardening). Those behaviors are not changed.  
**Auth model:** Unchanged — `tbl_login` + `Session` + `dbo.ActiveSessions`. Permission catalog is still `UserRoles` → `RolePermissions` → `Permissions.PermissionKey`.

This phase binds the same menu `PermissionKey` values (HTML `id`s on `Bill.Master`) to page `OnInit` via `SecurePage`. Menu markup, URLs, and workflow code are not changed.

`SecurePage.OnInit` runs on every GET and postback, so button events cannot skip the check. `[WebMethod]` calls skip page `OnInit`; converted pages that already had `AuthGuard.EnsureWebMethod()` now call `EnsureWebMethodPermission(key)` with the same key as the page.

---

## Page → PermissionKey map (reused menu ids)

| Page | PermissionKey | Notes |
|------|---------------|--------|
| `AddUser.aspx` | `AddUser` | Phase 0A; unchanged |
| `ViewUser.aspx` | `ViewUser` | Phase 0A; unchanged |
| `Update_Designation.aspx` | `ViewUser` | Phase 0A; unchanged (opened from View User) |
| `ManageRoles.aspx` | `ManageRoles` | Phase 0A; unchanged |
| `ManagePermissions.aspx` | `ManagePermissions` | Phase 0A; unchanged |
| `productparent.aspx` | `productparent` | |
| `product_master.aspx` | `product_master` | |
| `Update_product.aspx` | `product_master` | Child editor; no own menu id |
| `newproductparent.aspx` | `newproductparent` | |
| `newproduct_master.aspx` | `newproduct_master` | |
| `NewUpdate_product.aspx` | `newproduct_master` | Child editor; no own menu id |
| `ImportProducts.aspx` | `uploader` | Menu id is `uploader`, href is ImportProducts |
| `Service_master.aspx` | `Service_master` | |
| `search_products.aspx` | `Search_Products` | Principle submenu |
| `New_client.aspx` | `New_client` | |
| `View_client.aspx` | `View_client` | |
| `Delete_client.aspx` | `Delete_client` | |
| `Representative.aspx` | `Representative` | |
| `AddFactory.aspx` | `AddFactory` | |
| `ShowFactory.aspx` | `AddFactory` | Child view; no own menu id |
| `New_vendor.aspx` | `New_vendor` | |
| `View_vendor.aspx` | `View_vendor` | |
| `Delete_vendor.aspx` | `Delete_vendor` | |
| `visit_planner.aspx` | `visit_planner` | WebMethods gated with same key |
| `daily_rpt.aspx` | `daily_reporting` | Filename ≠ menu id |
| `vw_dailyrpts.aspx` | `vw_dailyrpts` | |
| `srch_dailyrpts.aspx` | `srch_dailyrpts` | |
| `AdminApprovalDashboard.aspx` | `AdminApprovalDashboard` | |
| `expense_entry.aspx` | `visit_planner` **or** `vw_dailyrpts` | No menu id; both parents link here |
| `general_expences.aspx` | `general_expences` | |
| `patty_cash_expences.aspx` | `patty_cash_expences` | |
| `view_expencess_head.aspx` | `view_expencess_head` | |
| `view_patty_cash_expenses.aspx` | `view_patty_cash_expenses` | |
| `Delete_general_expencess.aspx` | `Delete_general_expencess` | |
| `Delete_patty_cash_expenses.aspx` | `Delete_patty_cash_expenses` | |
| `Expenses_Head.aspx` | `Expenses_Head` | |
| `AdminLeaveSetup.aspx` | `AdminLeaveSetup` | |
| `AdminShiftSetup.aspx` | `AdminShiftSetup` | |
| `AdminShiftAssignment.aspx` | `AdminShiftSetup` | No menu id; shift-admin child |
| `AdminOverride.aspx` | `AdminOverride` | |

No new `Permissions` rows were invented. Filename/menu-id mismatches above are documented, not renamed.

---

## STOP (not converted)

| Page / area | Why |
|-------------|-----|
| `index.aspx`, `reset_password.aspx`, `index_card.aspx` | Public authentication surfaces |
| `settings.aspx`, `Update/password.aspx` | Forced password / contact lockout must remain reachable without a menu grant |
| `home.aspx` | Login landing page. `CompanyID` is first set in `Bill.Master` `Page_Load` on this GET. `SecurePage` + `RequireCompanyContext` in `OnInit` would 401 before the master runs. `home1` is also not guaranteed for every authenticated user |
| `admin/add_company.aspx`, `admin/Update_Company.aspx` (and other `card.Master` pages) | Separate `tbl_card_login` kiosk/admin host. No `ActiveSessions` token and no `UserRoles` grants. Forcing `SecurePage` would lock the module |
| `print/*.aspx` | Phase 0A `EnsurePrint` tenant checks; no master, no menu key |
| Invoice, Client PO, DPCC, Proforma, Payments Received, PR/PO, stock reports, scheduler | Out of Phase 1A priority. Business flows must stay unchanged; later phase |
| `attendance.aspx`, `MyLeaves.aspx` | Not in the Phase 1A priority list |

---

## Inconsistent keys (not silently renamed)

| Menu `PermissionKey` | Page file | Action taken |
|----------------------|-----------|--------------|
| `daily_reporting` | `daily_rpt.aspx` | Reused menu id |
| `uploader` | `ImportProducts.aspx` | Reused menu id |
| `Search_Products` | `search_products.aspx` | Reused menu id |
| `Li2`–`Li5` | Client PO pages | Not converted (PO out of scope) |
| none | `expense_entry.aspx` | OR of `visit_planner` and `vw_dailyrpts` |
| none | `Update_product.aspx` / `NewUpdate_product.aspx` / `ShowFactory.aspx` / `AdminShiftAssignment.aspx` | Parent menu key |

`EnsurePage` used by Phase 0A pages is unchanged. `EnsurePageAny` is additive for the dual-parent expense page only.
