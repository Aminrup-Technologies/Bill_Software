# ID-card kiosk (isolated)

> Domain key: `card-kiosk` · 17 page(s)

Separate `tbl_card_login` app under `/admin` + `card.Master`. Not ERP RBAC. See docs/21.

**Do not copy** AuthN/AuthZ, tenancy, or Ponytail rules here. Those live in [SHARED_CONTEXT](SHARED_CONTEXT.md).

| Page | Purpose | Master / class | Gate | Tables (unique) | Ops | Narrative |
|------|---------|----------------|------|-----------------|-----|-----------|
| `Print/id_card1.aspx` | id card1<br>QS: `ID` | none / `id_card1` | no-page-gate<br>— | `tbl_employee`, `tbl_Company` | SELECT | this catalog |
| `SessionKeepAlive1.aspx` | Session Keep Alive1 | none / `SessionKeepAlive1` | Session<br>— | `tbl_card_login` | SELECT | this catalog |
| `admin/Delete_date.aspx` | Delete date | card.Master / `WebForm7` | card.Master<br>— | `tbl_Company`, `tbl_employee` | DELETE | this catalog |
| `admin/Setting.aspx` | Setting | card.Master / `WebForm8` | card.Master<br>— | `tbl_card_login` | SELECT | this catalog |
| `admin/Show_data1.aspx` | Show data1 | card.Master / `WebForm9` | card.Master, CompanyID<br>— | `tbl_employee` | SELECT | this catalog |
| `admin/Total_id_card.aspx` | Total id card | none / `Total_id_card` | no-page-gate, CompanyID<br>— | `tbl_employee`, `tbl_Company` | SELECT | this catalog |
| `admin/Update/contactno.aspx` | contactno | none / `contactno` | Session<br>— | `tbl_card_login` | UPDATE | this catalog |
| `admin/Update/emailid.aspx` | emailid | none / `emailid` | Session<br>— | `tbl_login` | UPDATE | this catalog |
| `admin/Update/name.aspx` | name | none / `name` | Session<br>— | `tbl_card_login` | UPDATE | this catalog |
| `admin/Update/password.aspx` | password | none / `password` | Session<br>— | `tbl_login`, `tbl_card_login` | UPDATE | this catalog |
| `admin/Update_Company.aspx` | Update Company<br>QS: `ComID` | card.Master / `WebForm4` | card.Master<br>— | `tbl_Company` | UPDATE | this catalog |
| `admin/Upload_data.aspx` | Upload data | card.Master / `WebForm6` | card.Master<br>— | `tbl_Company`, `tbl_employee` | INSERT | this catalog |
| `admin/add_company.aspx` | add company | card.Master / `WebForm1` | card.Master<br>— | `tbl_Company`, `tbl_employee` | INSERT, DELETE | this catalog |
| `admin/home.aspx` | My Profile | card.Master / `WebForm2` | card.Master<br>menu `home1` | `tbl_card_login` | SELECT | this catalog |
| `admin/show_date.aspx` | show date | card.Master / `WebForm3` | card.Master, CompanyID<br>— | `tbl_Company`, `tbl_employee` | SELECT | this catalog |
| `admin/update_image.aspx` | update image<br>QS: `ID` | card.Master / `WebForm5` | card.Master, CompanyID<br>— | `tbl_employee` | UPDATE | this catalog |
| `index_card.aspx` | index card | none / `index_card` | public<br>— | `tbl_card_login`, `tbl_login`, `ActiveSessions` | SELECT | this catalog |

## Page-unique notes

- `SessionKeepAlive1.aspx`: Keep-alive for the card kiosk session (`tbl_card_login`), not ERP `ActiveSessions`.
- `index_card.aspx`: Isolated kiosk login against `tbl_card_login` (plaintext). Not ERP RBAC. See docs/21 (A-02).
