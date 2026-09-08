# Customers & factories

> Domain key: `customer` · 9 page(s)

Customer CRUD, SPOC, factory sites. Narrative: docs/05.

**Do not copy** AuthN/AuthZ, tenancy, or Ponytail rules here. Those live in [SHARED_CONTEXT](SHARED_CONTEXT.md).

| Page | Purpose | Master / class | Gate | Tables (unique) | Ops | Narrative |
|------|---------|----------------|------|-----------------|-----|-----------|
| `corporate/business/app/AddFactory.aspx` | Manage Client Factories<br>QS: `Client_Id` | Bill.Master / `WebForm61` | SecurePage, CompanyID<br>`AddFactory` | `tbl_State`, `tbl_City`, `tbl_Client`, `tbl_Factory`, `tbl_SystemNotification` | INSERT, DELETE | this catalog |
| `corporate/business/app/Delete_client.aspx` | Delete Customer | Bill.Master / `WebForm18` | SecurePage, CompanyID<br>`Delete_client` | `tbl_Client`, `tbl_ClientRegAddress` | DELETE | [module](../05_Customer_Vendor.md) |
| `corporate/business/app/New_client.aspx` | Create Client<br>WM: `AddNewCityInline` | Bill.Master / `WebForm15` | SecurePage, CompanyID, WebMethod<br>`New_client` | `tbl_State`, `tbl_City`, `tbl_Client`, `tbl_SystemNotification` | INSERT | [module](../05_Customer_Vendor.md) |
| `corporate/business/app/Representative.aspx` | Manage Representatives<br>QS: `Client_Id` | Bill.Master / `WebForm59` | SecurePage, CompanyID<br>`Representative` | `tbl_Client`, `tbl_representative`, `tbl_SystemNotification` | INSERT, DELETE | this catalog |
| `corporate/business/app/ShowFactory.aspx` | Show Factory<br>QS: `Client_Id` | Bill.Master / `WebForm62` | SecurePage<br>`AddFactory` | `tbl_Factory`, `tbl_Client` | DELETE | this catalog |
| `corporate/business/app/Show_representative.aspx` | Show representative<br>QS: `Client_Id` | Bill.Master / `WebForm60` | Bill.Master<br>— | `tbl_Representative`, `tbl_Client` | DELETE | this catalog |
| `corporate/business/app/Update_client.aspx` | Update Client<br>QS: `Client_Id` | Bill.Master / `WebForm17` | Bill.Master, CompanyID<br>— | `tbl_State`, `tbl_City`, `tbl_Industry`, `tbl_Client`, `tbl_ClientRegAddress`, `tbl_SystemNotification` | INSERT, UPDATE | [module](../05_Customer_Vendor.md) |
| `corporate/business/app/Update_factory.aspx` | Update factory<br>QS: `ID` | Bill.Master / `WebForm63` | Bill.Master<br>— | `tbl_State`, `tbl_City`, `tbl_Client`, `tbl_Factory` | UPDATE | this catalog |
| `corporate/business/app/View_client.aspx` | View Clients | Bill.Master / `WebForm16` | SecurePage, CompanyID<br>`View_client` | `tbl_Client`, `tbl_representative`, `tbl_Factory`, `tbl_SystemNotification` | INSERT | [module](../05_Customer_Vendor.md) |

<!-- NARRATIVE:BEGIN -->

## Model

- **Customer** `tbl_Client` — buyer; IDs `AD…`; optional `tbl_ClientRegAddress`.
- **Factory / Unit** `tbl_Factory` — site under `Client_id`. Add page limits names to Unit 1–6.
- **SPOC** `tbl_representative` — person under the client. FK column is misspelled `Copany_Id` (= `Client_Id`).

Hub: `View_client` → `Update_client.aspx?Client_Id=` / `AddFactory.aspx?Client_Id=` / `Representative.aspx?Client_Id=`.

## Page behavior

- **`New_client`:** WebMethod `AddNewCityInline`. Next id `findcompanyId()` (`AD01`+). Duplicate name check. Control IDs still say `txtvendorName` / industry combo is unused (`BindIndustry` commented).
- **`View_client`:** `GetClientNames` autocomplete; CSV export Master / Reps / Factories (export is the only write — notification).
- **`Update_client`:** not SecurePage. QS `Client_Id` required. Updates client + registered address; does **not** update `PlaceofSupply`; reg address UPDATE only (no insert).
- **`Delete_client`:** deletes `tbl_Client` then `tbl_ClientRegAddress`. Does **not** cascade factories or reps. Combo `cmbvendor`.
- **`Representative`:** titles Mr/Mrs/Ms/Dr/Md/Col. Live SPOC UI.
- **`Show_representative`:** leftover list/delete; string-concat SQL; session miss → `CustomError.aspx`. Prefer `Representative.aspx`.
- **`AddFactory`:** state/city combos are **not** company-filtered. Permission `AddFactory`.
- **`ShowFactory`:** leftover; same permission as add; Edit → `Update_factory.aspx?ID=`.
- **`Update_factory`:** factory name read-only; back to `ShowFactory.aspx?Client_Id=`.

## Domain quirks

- Visit `CustomerName` is still free text (see docs/05) — no FK from `tbl_SalesVisitReport` to `tbl_Client`.
- Many combos are named `cmbvendor` on customer pages.
