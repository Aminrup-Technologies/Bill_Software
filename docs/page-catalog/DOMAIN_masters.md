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

<!-- NARRATIVE:BEGIN -->

## Two product catalogs

Live sales/purchase/invoice pages use the **new** chain. Menu still exposes the **old** chain as “Product Catagory Old” / “Product Name Old”.

| Generation | Category page | Item page | Tax table | Stock |
|------------|---------------|-----------|-----------|-------|
| Legacy | `productparent` → `tbl_parentProduct` | `product_master` → `tbl_Product` (edit `Update_product.aspx?Id=`) | `tbl_Vat_Master` | none on insert |
| Current | `newproductparent` → `tbl_NewparentProduct` (CompanyID) | `newproduct_master` → `tbl_NewProduct` + `tbl_stock` | `tbl_New_Vat_Master` | opening row, store `STR001` |

- `ImportProducts` (`uploader`) bulk-loads Tally XML into **new** tables. Category combo is unscoped; inserts omit CompanyID; default tax 18%; expiry hard-coded `2026-03-31`.
- `NewUpdate_product.aspx?Id=` is an orphan editor (`newproduct_master` permission). In-app edit is in-grid on `newproduct_master`.
- Legacy **services** stay on `tbl_Service` / `tbl_Service_master` (IDs `SER####`), separate from new categories.

## Lookup masters

Simple INSERT/DELETE DataList pages, **no CompanyID**: `AddIndustry` (`tbl_Industry`), `AddPrimaryService` (`tbl_PrimaryService`), `PaymentPhase` (`tbl_PaymentPhase`), `Vat_master` (`tbl_Vat_Master`), `Service_Tax_Master` (`tbl_Service_master`).

`PrimaryServiceTerms` dropdown is `tbl_NewparentProduct.ProductOrServiceCat`, not `tbl_PrimaryService`.

## Geography (tenant-scoped)

- `master_State`: soft-delete `DeleteMode=1`, duplicate name per company, writes `tbl_SystemNotification`.
- `master_city`: stores `State_Name` as text (not state id FK); duplicate = city+state per company.

## Page behavior

- **`newproduct_master`:** WebMethods `CheckDuplicateName`, `GetDuplicateInfo`. ProductID `PRD` + pad. Soft-delete. Images packed `T=\|B=\|L=\|R=\|O=` in `ImageUrl`. Notifications `ModuleCode=PRODUCT_MASTER`.
- **`newproductparent`:** hard DELETE categories; notifications `ModuleCode=CATEGORY`.
- **`Service_master`:** auto `SER00n`; Edit → `Service_update.aspx?Id=` (that page has **no** `RequiredPermissionKey`).
- **`product_master`:** Session `ProductCode`/`gstRate`/`pid` from parent combo; no stock row.
- **`ImportProducts`:** Product vs GST radio; `btnInsert` / `btnUpdate` / `btnUpsert` / `btn_GstData_Update`; logs under `~/Uploads/Logs/`.

## Domain quirks

- Two VAT tables (`tbl_Vat_Master` vs `tbl_New_Vat_Master`) must not be mixed.
- Most lookup masters use string-concat SQL and have no tenant filter — unique to this domain vs sales-visit pages.
