# Departments & designations

There is **no** dedicated department/designation CRUD module.

Canonical user-admin pages: [`page-catalog/DOMAIN_users-rbac.md`](page-catalog/DOMAIN_users-rbac.md).  
HR / punch pages: [`page-catalog/DOMAIN_attendance-hr.md`](page-catalog/DOMAIN_attendance-hr.md).  
Kiosk employee photos: [`page-catalog/DOMAIN_card-kiosk.md`](page-catalog/DOMAIN_card-kiosk.md).

---

## What the tables are for

| Table | Used on | Unique job |
|-------|---------|------------|
| `tbl_Departments` | `AddUser.aspx`, `ViewUser.aspx` dropdowns | Label on **ERP login accounts** (`tbl_login`). |
| `tbl_Designations` | Same | Label on ERP login accounts. |

No `Add_department.aspx` / `Add_designation.aspx`. New rows are expected via SQL. Do not document `admin/Update/` or `app/Update/` as that UI — those are **profile popups** (kiosk vs ERP).

---

## What `Update_Designation.aspx` actually does

Assigns **`UserRoles`** (permission **`ViewUser`**). It is **not** a designation master.

---

## HR vs ERP vs kiosk labels

`tbl_employee` (card kiosk) is a separate person list (`imgdata`, `ComID`). Attendance/leave uses `tbl_login.User_Id` as `UserCode`. Do not join `tbl_Departments` to `tbl_employee` as one org chart unless a live FK is proven.
