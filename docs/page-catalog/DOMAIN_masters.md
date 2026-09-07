# Master data

> Domain key: `masters` · 17 page(s)

Products, services, tax, geography, expense heads, employee config.

**Do not copy** AuthN/AuthZ, tenancy, or Ponytail rules here. Those live in [SHARED_CONTEXT](SHARED_CONTEXT.md).

| Page | Purpose | Master / class | Gate | Tables (unique) | Ops | Narrative |
|------|---------|----------------|------|-----------------|-----|-----------|
| `corporate/business/app/AddIndustry.aspx` | Add Industry | Bill.Master / `WebForm75` | Bill.Master<br>menu `AddIndustry` | `tbl_Industry` | INSERT, DELETE | this catalog |
| `corporate/business/app/AddPrimaryService.aspx` | Add Primary Service | Bill.Master / `WebForm76` | Bill.Master<br>menu `AddPrimaryService` | `tbl_PrimaryService` | INSERT, DELETE | this catalog |
| `corporate/business/app/ImportProducts.aspx` | XML Uploader for Products | Bill.Master / `ImportProducts` | SecurePage<br>`uploader` | `tbl_NewparentProduct`, `tbl_NewProduct`, `tbl_stock` | INSERT, UPDATE | this catalog |
| `corporate/business/app/NewUpdate_product.aspx` | New Update product<br>QS: `Id` | Bill.Master / `WebForm70` | SecurePage<br>`newproduct_master` | `tbl_New_Vat_Master`, `tbl_NewProduct` | UPDATE | this catalog |
| `corporate/business/app/PaymentPhase.aspx` | Add Payment Phase | Bill.Master / `WebForm77` | Bill.Master<br>menu `PaymentPhase` | `tbl_PaymentPhase` | INSERT, DELETE | this catalog |
| `corporate/business/app/PrimaryServiceTerms.aspx` | Specific Terms | Bill.Master / `WebForm78` | Bill.Master<br>menu `PrimaryServiceTerms` | `tbl_PrimaryService`, `tbl_NewparentProduct`, `tbl_PrimaryServiceTerms` | INSERT, DELETE | this catalog |
| `corporate/business/app/Service_Tax_Master.aspx` | Service Tax Master | Bill.Master / `WebForm8` | Bill.Master<br>menu `Service_Tax_Master` | `tbl_Service_master` | INSERT, DELETE | this catalog |
| `corporate/business/app/Service_master.aspx` | Service | Bill.Master / `WebForm7` | SecurePage<br>`Service_master` | `tbl_Service_master`, `tbl_Service` | INSERT, DELETE | this catalog |
| `corporate/business/app/Service_update.aspx` | Service update<br>QS: `Id` | Bill.Master / `WebForm55` | Bill.Master<br>— | `tbl_Service_master`, `tbl_Service`, `tbl_Product` | INSERT, UPDATE | this catalog |
| `corporate/business/app/Update_product.aspx` | Update product<br>QS: `Id` | Bill.Master / `WebForm54` | SecurePage<br>`product_master` | `tbl_Vat_Master`, `tbl_Product` | INSERT, UPDATE | this catalog |
| `corporate/business/app/Vat_master.aspx` | GST Rate Master | Bill.Master / `WebForm9` | Bill.Master<br>menu `Vat_master` | `tbl_Vat_Master` | INSERT, DELETE | this catalog |
| `corporate/business/app/master_State.aspx` | Manage State Master | Bill.Master / `WebForm4` | Bill.Master, CompanyID<br>menu `master_State` | `tbl_State`, `tbl_SystemNotification` | INSERT, UPDATE | this catalog |
| `corporate/business/app/master_city.aspx` | Manage City Master | Bill.Master / `WebForm3` | Bill.Master, CompanyID<br>menu `master_city` | `tbl_City`, `tbl_State`, `tbl_SystemNotification` | INSERT, UPDATE | this catalog |
| `corporate/business/app/newproduct_master.aspx` | Add New Products | Bill.Master / `WebForm69` | SecurePage, CompanyID<br>`newproduct_master` | `tbl_New_Vat_Master`, `tbl_NewparentProduct`, `tbl_NewProduct`, `tbl_SystemNotification`, `tbl_stock` | INSERT, UPDATE | this catalog |
| `corporate/business/app/newproductparent.aspx` | Product Catagory | Bill.Master / `WebForm68` | SecurePage, CompanyID<br>`newproductparent` | `tbl_NewparentProduct`, `tbl_SystemNotification` | INSERT, DELETE | this catalog |
| `corporate/business/app/product_master.aspx` | Product Name Old | Bill.Master / `WebForm6` | SecurePage<br>`product_master` | `tbl_parentProduct`, `tbl_Product` | INSERT, DELETE | this catalog |
| `corporate/business/app/productparent.aspx` | Product Catagory Old | Bill.Master / `productparent` | SecurePage<br>`productparent` | `tbl_Vat_Master`, `tbl_parentProduct` | INSERT, DELETE | this catalog |
