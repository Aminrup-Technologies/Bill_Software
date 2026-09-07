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
