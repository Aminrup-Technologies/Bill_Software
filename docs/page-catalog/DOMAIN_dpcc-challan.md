# DPCC / challan

> Domain key: `dpcc-challan` · 4 page(s)

Delivery challan (DPCC) create/view/search/delete + prints.

**Do not copy** AuthN/AuthZ, tenancy, or Ponytail rules here. Those live in [SHARED_CONTEXT](SHARED_CONTEXT.md).

| Page | Purpose | Master / class | Gate | Tables (unique) | Ops | Narrative |
|------|---------|----------------|------|-----------------|-----|-----------|
| `corporate/business/app/Delete_chalan.aspx` | Delete DPCC | Bill.Master / `WebForm41` | Bill.Master, CompanyID<br>menu `Delete_chalan` | `tbl_Client`, `tbl_Chalan`, `tbl_QuoPriSerTogather`, `tbl_Challan_details`, `tbl_ChaSiteAddress`, `tbl_Quotation` | UPDATE, DELETE | this catalog |
| `corporate/business/app/View_chalan.aspx` | View DPCC | Bill.Master / `WebForm39` | Bill.Master, CompanyID<br>menu `View_chalan` | `tbl_Client`, `tbl_Chalan`, `tbl_Quotation`, `tbl_QuoPriSerTogather` | SELECT | this catalog |
| `corporate/business/app/add_chalan.aspx` | Create DPCC | Bill.Master / `WebForm38` | Bill.Master, CompanyID<br>menu `add_chalan` | `tbl_Client`, `tbl_QuoPriSerTogather`, `tbl_Quotation`, `tbl_Challan_details`, `tbl_Chalan`, `tbl_Quotaion_details`, `tbl_Factory`, `tbl_ClientRegAddress` | INSERT, UPDATE | this catalog |
| `corporate/business/app/seartch_chalan.aspx` | Search DPCC | Bill.Master / `WebForm40` | Bill.Master, CompanyID<br>menu `seartch_chalan` | `tbl_Client`, `tbl_Chalan`, `tbl_QuoPriSerTogather` | SELECT | this catalog |
