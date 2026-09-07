# Principles & purchases

> Domain key: `vendor-purchase` · 11 page(s)

Vendor (Principle) master and purchase-from-vendor. Narrative: docs/05, docs/10.

**Do not copy** AuthN/AuthZ, tenancy, or Ponytail rules here. Those live in [SHARED_CONTEXT](SHARED_CONTEXT.md).

| Page | Purpose | Master / class | Gate | Tables (unique) | Ops | Narrative |
|------|---------|----------------|------|-----------------|-----|-----------|
| `corporate/business/app/Delete_purtches.aspx` | Delete Purchase | Bill.Master / `WebForm22` | Bill.Master, CompanyID<br>menu `Delete_purtches` | `tbl_Vendor`, `tbl_Purches`, `tbl_purches_details`, `tbl_purches_due`, `tbl_stock`, `tbl_NewProduct` | UPDATE, DELETE | this catalog |
| `corporate/business/app/Delete_vendor.aspx` | Delete Principle | Bill.Master / `WebForm14` | SecurePage, CompanyID<br>`Delete_vendor` | `tbl_Vendor` | DELETE | [module](../05_Customer_Vendor.md) |
| `corporate/business/app/EditPurchase.aspx` | Edit Purchase | Bill.Master / `EditPurchase` | Bill.Master, CompanyID<br>menu `Edit_Purches` | `tbl_Vendor`, `tbl_Purches`, `tbl_Vat_Master`, `tbl_Service_master`, `tbl_purches_details`, `tbl_stock`, `tbl_NewparentProduct`, `tbl_Service` | INSERT, UPDATE, DELETE | this catalog |
| `corporate/business/app/New_vendor.aspx` | Create Vendor | Bill.Master / `WebForm5` | SecurePage, CompanyID<br>`New_vendor` | `tbl_State`, `tbl_Vendor`, `tbl_SystemNotification`, `tbl_City` | INSERT | [module](../05_Customer_Vendor.md) |
| `corporate/business/app/Purches_exting_vendor.aspx` | Purchase Wizard | Bill.Master / `WebForm11` | Bill.Master<br>menu `Purches_exting_vendor` | `tbl_Vendor`, `tbl_NewparentProduct`, `tbl_Vat_Master`, `tbl_NewProduct`, `tbl_Service`, `tbl_Purches`, `tbl_Purchess_payment`, `tbl_purches_details` | INSERT, UPDATE | [module](../05_Customer_Vendor.md) |
| `corporate/business/app/Purches_new_vendor.aspx` | Purches new vendor | Bill.Master / `WebForm10` | Bill.Master, CompanyID<br>— | `tbl_State`, `tbl_City`, `tbl_Vendor`, `tbl_Product`, `tbl_Service`, `tbl_Vat_Master`, `tbl_Service_master`, `tbl_purches_details` | INSERT, UPDATE | [module](../05_Customer_Vendor.md) |
| `corporate/business/app/Update_vendor.aspx` | Update Vendor<br>QS: `Vendor_Id` | Bill.Master / `WebForm12` | Bill.Master, CompanyID<br>— | `tbl_State`, `tbl_City`, `tbl_Vendor`, `tbl_SystemNotification` | INSERT, UPDATE | [module](../05_Customer_Vendor.md) |
| `corporate/business/app/View_purches.aspx` | View Purchase | Bill.Master / `WebForm20` | Bill.Master, CompanyID<br>menu `View_purches` | `tbl_Purches`, `tbl_Vendor` | SELECT | this catalog |
| `corporate/business/app/View_vendor.aspx` | View Vendors | Bill.Master / `WebForm13` | SecurePage, CompanyID<br>`View_vendor` | `tbl_Vendor`, `tbl_SystemNotification` | INSERT | [module](../05_Customer_Vendor.md) |
| `corporate/business/app/search_products.aspx` | Search Products | Bill.Master / `search_products` | SecurePage, CompanyID<br>`Search_Products` | `tbl_NewparentProduct`, `tbl_NewProduct` | SELECT | this catalog |
| `corporate/business/app/seartch_purtch.aspx` | Search Purchase | Bill.Master / `WebForm21` | Bill.Master, CompanyID<br>menu `seartch_purtch` | `tbl_Vendor`, `tbl_Purches` | SELECT | this catalog |

## Page-unique notes

- `corporate/business/app/seartch_purtch.aspx`: Filename misspelled `seartch_purtch` (purchase search).
