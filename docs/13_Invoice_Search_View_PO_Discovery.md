# Discovery Report: Invoice Search, Invoice View, and Purchase Order Print

**Pass type:** Read-only discovery. No code was modified.  
**Date:** 2026-09-03  
**Branch context:** `July_to_Sept26_DevNSupport`  
**Scope files:**

| Requested | Actual path | Notes |
|-----------|-------------|-------|
| `search_invoice.aspx` / `.cs` | `Bill_Software/corporate/business/app/seartch_invoice.aspx` (+ `.cs`) | Filename is misspelled `seartch`. Class `WebForm28`. Master: `Bill.Master`. |
| `View_Invoice.aspx` / `.cs` | `Bill_Software/corporate/business/app/View_Invoice.aspx` (+ `.cs`) | Class `WebForm27`. Master: `Bill.Master`. |
| `print/NewPurchaseOrder.aspx` / `.cs` | `Bill_Software/corporate/business/print/NewPurchaseOrder.aspx` (+ `.cs`) | Standalone page (no master). Class `NewPurchaseOrder`. |

Designer files were read for control inventory. Related callers (`View_PurchaseOrder.aspx.cs`, `Search_purchaseorder.aspx.cs`, `NewInvoice.aspx.cs`, `NewInvoice_v2.aspx.cs`, `Add_invoice.aspx.cs`, `Bill.Master.cs`, `DB_UTILITY.cs`) were consulted only to verify isolation, export patterns, and schema.

**Key finding up front:** Neither invoice list page uses `GridView`. Both use an HTML table + `asp:Repeater`. There are **no stored procedures** in these three pages. All SQL is inline `CommandType.Text` (including `DB_UTILITY.SPreturn_dt`, which is a misnomer). Excel buttons emit **CSV**, not `.xlsx`. `NewPurchaseOrder.aspx` has **no `CompanyID` filter and no session auth**.

---

## 1. Complete execution flow for each page

### 1.1 Advanced Search Invoice — `seartch_invoice.aspx` (`WebForm28`)

```
GET /corporate/business/app/seartch_invoice.aspx
  → Bill.Master Page_Load
      Session["USERID"] required, else redirect ~/index.aspx
      Session["SessionToken"] validated against dbo.ActiveSessions
      CompanyContext.CurrentCompanyID from Session["CompanyID"] (0 if missing)
  → WebForm28.Page_Load
      Session["USERID"] null → redirect ~/index.aspx + CompleteRequest
      !IsPostBack → LoadClients()
          SELECT Client_Name FROM tbl_Client WHERE CompanyID = @CompanyID ORDER BY Client_Name
          Bind cmbvendor with "-- All Clients --" + names
      Dates left blank (intentional: query all history unless user filters)
      Repeater is empty until Search is clicked
  → Client scripts
      jQuery UI datepicker (dd-M-yy) on .datepicker
      Select2 on cmbvendor ("Search Client...")
      Sys.WebForms.PageRequestManager endRequest rebind (UpdatePanel-ready; page has no UpdatePanel)

POST Search (btnSertch_Click)
  → BindData()
      Parameterized SELECT (see §3) filtered by CompanyID + optional client/invoice/ERP/dates
      rptInvoices.DataSource = dt; DataBind()
      ShowMsg("Search completed. Found N records.", ok)
      InsertSystemNotification("Advanced Invoice Search", ..., swallow errors)

POST Clear (btnClear_Click)
  → Reset filters, unbind repeater, hide message panel

POST Export Excel (btnExport_Click)
  → Separate line-item query (see §3, §6)
      If rows > 0 → ExportDataTableToCsv(...) then Response.End()
      Else ShowMsg("No records found to export.")
```

**Navigation out:** Buyer icon opens popup `/corporate/business/print/NewInvoice.aspx?ID={ID}`. Seller icon opens `/corporate/business/print/NewInvoiceDuplicate.aspx?ID={ID}`. Those print pages are **outside this discovery set**. Current live print pages do **not** apply `CompanyID` (unlike unused `NewInvoice_v2.aspx.cs`).

### 1.2 View Tax Invoices — `View_Invoice.aspx` (`WebForm27`)

```
GET /corporate/business/app/View_Invoice.aspx
  → Bill.Master (same session/token/company as above)
  → WebForm27.Page_Load
      Session["USERID"] null → redirect
      !IsPostBack:
          txtFromDate = 1st of current month (dd-MMM-yyyy)
          txtToDate   = today
          BindData()   ← auto-loads current month (unlike search page)

POST Search (btnSearch_Click) → BindData()
POST Clear (btnClear_Click)
  → Clear invoice/ERP/client text
  → Reset dates to current month
  → BindData() again (does not empty the grid)
POST Export Excel (btnExport_Click)
  → Line-item query + ExportDataTableToExcel() which is actually CSV
  → On success: InsertSystemNotification("Invoices Exported", ...)
```

UI is almost identical to Advanced Search except:

- No client dropdown / Select2; free-text `txtSearchClient` (`LIKE`).
- Default date window + auto-bind on first load.
- Export notification is logged on export, not on search.

### 1.3 Purchase Order print — `print/NewPurchaseOrder.aspx`

**Entry points (outside target files, verified):**

- `View_PurchaseOrder.aspx.cs` `rptPurchaseOrders_ItemCommand` → `Response.Redirect(...?ID=)`
- `edit_purchaseorder.aspx` popup `window.open(...?ID=)`
- `delete_purchaseorder.aspx.cs` redirect
- `Search_purchaseorder.aspx.cs` builds print URL

```
GET /corporate/business/print/NewPurchaseOrder.aspx?ID={quotation.ID}
  → NO master page
  → NO Session["USERID"] check
  → NO CompanyID check
  → Page_Load !IsPostBack:
        ID = Request.QueryString["ID"]   (null/empty still passed through)
        buindalldata(ID)
            SELECT header from tbl_Quotation WHERE ID=@id
            if rows:
                bind labels / Session bags
                Bindclientdetails(clientid)
                BindRepresentative(clientid)
                BindService(qutno)            ← SQL concatenation
                Buindamount(qutno)            ← commercial line HTML → lblserviceamo
                bindpayment(qutno)            ← payment schedule HTML → lblPayment
                BuindamountByQuotation(qutno) ← challan delivery HTML → lblProductDetails
                bindPrimaryServiceTerms(qutno)→ lblPrimaryServicePoint

Print Without Letterhead (Button1)
  OnClientClick: set thead/tfoot className to 'header'/'footer' (CSS visibility:hidden), then window.print()
  OnClick Button1_Click: empty. Causes postback; IsPostBack skips rebind; ViewState keeps labels.

Print With Letterhead (Button2)
  OnClientClick: window.print() only (letterhead remains visible because .header/.footer classes are not applied)
  OnClick Button2_Click: empty (same postback).
```

If `ID` is missing or no row is found, the page still renders the empty A4 shell. No error UI.

---

## 2. All data-binding methods

### 2.1 `seartch_invoice.aspx.cs`

| Method | Bound control | Trigger |
|--------|---------------|---------|
| `LoadClients()` | `cmbvendor` | First GET |
| `BindData()` | `rptInvoices` | Search |
| `btnClear_Click` | `rptInvoices` = null | Clear |
| `btnExport_Click` | none (streams CSV) | Export |
| Repeater `Eval(...)` | HTML cells | DataBind |

No `RowDataBound`, no `ItemDataBound`, no `GridView`.

### 2.2 `View_Invoice.aspx.cs`

| Method | Bound control | Trigger |
|--------|---------------|---------|
| `BindData()` | `rptInvoices` | First GET, Search, Clear |
| `btnExport_Click` | none (streams CSV) | Export |

Same Repeater `Eval` template as search.

### 2.3 `NewPurchaseOrder.aspx.cs`

| Method | Output | Query |
|--------|--------|-------|
| `buindalldata(id)` | Header labels + Session bags | `tbl_Quotation` by `ID` |
| `Bindclientdetails(clientid)` | Company/address/contact/PAN/GST labels | `tbl_Client` by `Client_Id` |
| `BindRepresentative(clientid)` | Name/title/designation | `tbl_representative` where `Copany_Id=@Copany_Id` (typo in column) |
| `BindService` + `generatelavel` | `lblservice`, `lblPrimaryService` | `tbl_QutPrimaryService` (concatenated SQL) |
| `Buindamount(qutno)` | HTML string → `lblserviceamo` | `tbl_Quotaion_details` (`IsLatest=1 AND IsDeleted=0`) |
| `bindpayment(qutno)` | HTML string → `lblPayment` | `tbl_QutPaymentPhase` |
| `BuindamountByQuotation(qutno)` | HTML string → `lblProductDetails` | `tbl_Chalan` + `tbl_Challan_details` ⨝ `tbl_Quotaion_details` |
| `bindPrimaryServiceTerms` / `bindterms1` / `bindterms` | HTML string → `lblPrimaryServicePoint` | `tbl_QutPrimaryService`, `tbl_PrimaryServiceTerms`, `tbl_QuoPserTerm` |

`lblserviceamo` / `lblProductDetails` / `lblPayment` / `lblPrimaryServicePoint` are Labels filled with **raw HTML** (`<table>`, `<div>`), not data-bound controls.

---

## 3. Stored procedures, SQL queries, and joined tables

**Stored procedures:** none. `DB_UTILITY.SPreturn_dt` always uses `CommandType.Text`.

### 3.1 Search — grid query (`BindData`)

```sql
SELECT a.ID, a.Invoice_No, a.Invoice_Date, a.Quotation_No, a.Quotation_Date,
       a.ExtInvoiceNo, a.Gross, a.discount, a.Delivery_Amount, a.otherAmount1,
       a.cgstOrsgst, a.igst,
       TRY_CAST(a.Net_Amount AS FLOAT) AS Net_Amount,
       TRY_CAST(a.Service_Tax1 AS FLOAT) AS Gst,
       a.mailDate,
       TRY_CAST(a.Net_Amount AS FLOAT) - TRY_CAST(a.Service_Tax1 AS FLOAT) AS sub_total,
       b.Client_Name, c.PServiceName, q.PO_Number, q.DO_Number,
       q.Validity_StartDate, q.Validity_EndDate, a.AddedById,
       ISNULL(l.Name, a.AddedById) AS AddedByName, a.TimeStamp
FROM tbl_Invoice AS a
LEFT JOIN tbl_Client AS b ON b.Client_Id = a.Client_ID
LEFT JOIN tbl_QuoPriSerTogather AS c ON a.Quotation_No = c.qutno
LEFT JOIN tbl_Quotation AS q ON q.Quotation_no = a.Quotation_No
LEFT JOIN tbl_login AS l ON l.User_Id = a.AddedById
WHERE a.CompanyID = @CompanyID
  -- optional: AND b.Client_Name = @ClientName
  -- optional: AND a.Invoice_No LIKE @InvNo
  -- optional: AND a.ExtInvoiceNo LIKE @ExtNo
  -- optional: AND TRY_CONVERT(DATE, a.Invoice_Date, 106) >= TRY_CONVERT(DATE, @FromDate, 106)
  -- optional: AND TRY_CONVERT(DATE, a.Invoice_Date, 106) <= TRY_CONVERT(DATE, @ToDate, 106)
ORDER BY TRY_CONVERT(DATE, a.Invoice_Date, 106) DESC, a.ID DESC
```

**Style 106** = `dd mon yyyy`, matching datepicker `dd-M-yy`.

### 3.2 View — grid query (`BindData`)

Identical SELECT/JOINs. Differences:

- Starts `WHERE 1=1` then `AND a.CompanyID = @CompanyID`.
- Client filter is `b.Client_Name LIKE @Client` (substring), not exact dropdown match.
- Stale comment still claims `tbl_Invoice` may not have `CompanyID`; the filter **is** enabled. `Add_invoice.aspx.cs` INSERT confirms the column exists.

### 3.3 Search + View — export query (`btnExport_Click`)

```sql
SELECT
    a.Invoice_No AS [Invoice Number],
    a.Invoice_Date AS [Invoice Date],
    a.ExtInvoiceNo AS [ERP Ref],
    b.Client_Name AS [Client Name],
    d.Product_id AS [Item Code],
    d.Product_Code AS [HSN Code],
    d.Product_name AS [Item Name],
    d.Quantity AS [Qty],
    d.sail_rate AS [Rate],
    ISNULL(d.Total_sail_rate2, (TRY_CAST(d.Quantity AS FLOAT) * TRY_CAST(d.sail_rate AS FLOAT))) AS [Taxable Value],
    d.Service_tax_rate AS [GST %],
    d.Total_sail_rate1 AS [Item Net Value],
    a.Net_Amount AS [Invoice Grand Total],
    a.AddedById AS [Created By]
FROM tbl_Invoice AS a
LEFT JOIN tbl_Client AS b ON b.Client_Id = a.Client_ID
LEFT JOIN tbl_Invoice_details AS d ON d.Invoice_No = a.Invoice_No
WHERE a.CompanyID = @CompanyID
  -- same optional filters as the grid
ORDER BY TRY_CONVERT(DATE, a.Invoice_Date, 106) DESC, a.Invoice_No DESC
```

View adds `WHERE 1=1` then the same CompanyID + filters.

### 3.4 Search — client dropdown

```sql
SELECT Client_Name FROM tbl_Client WHERE CompanyID = @CompanyID ORDER BY Client_Name
```

### 3.5 Search + View — notification insert

```sql
INSERT INTO tbl_SystemNotification
    (Title, Message, ModuleType, AlertLevel, CreatedBy, CreatedDate, IsActive)
VALUES (@Title, @Message, @ModuleType, @AlertLevel, @CreatedBy, GETDATE(), 1)
```

No `CompanyID` column in this insert. Other modules (e.g. `Add_invoice.aspx.cs`, `AdminShiftSetup.aspx.cs`) insert `CompanyID`. Schema of `tbl_SystemNotification` is inconsistent across the codebase.

### 3.6 Purchase Order print queries

| # | SQL (summarized) | Tables |
|---|------------------|--------|
| 1 | `SELECT ID, Quotation_no, Quotation_date, Client_Id, PlaceofSupply, ReferenceName, ReferenceData, ReferenceId, ReferenceDate, ValidityDays, DeliveryTenure, PackingCharges, Remarks, DetailedView, DO_Number, PO_Number, PO_Date, Validity_StartDate, Validity_EndDate, TimsStamp, sub_total, Net_amount, cgstOrsgst, igst, DiscountView, TCS_Amount, Freight_Amount, OtherCharge_Name, OtherCharge_Amount, RecordType FROM tbl_Quotation WHERE ID=@id` | `tbl_Quotation` |
| 2 | `SELECT Client_Name, Address1, Address2, City, pin, State, Service_tax_no, Pan_no, PlaceofSupply, Com_email, Com_phone FROM tbl_Client WHERE Client_Id=@Client_Id` | `tbl_Client` |
| 3 | `SELECT Representative_name, Designation, Phone_no, Email, RepTitle, RepLastName FROM tbl_representative WHERE Copany_Id=@Copany_Id` | `tbl_representative` |
| 4 | `SELECT count(*) FROM tbl_QutPrimaryService WHERE qut_no='{qutno}'` **concatenated** | `tbl_QutPrimaryService` |
| 5 | `SELECT PrimaryService FROM tbl_QutPrimaryService WHERE qut_no='{qutno}' ORDER BY id` **concatenated** | `tbl_QutPrimaryService` |
| 6 | `SELECT Sl_no, Product_id AS HSN, Product_name, specification, Misc, Quantity, Unit, sail_rate, Service_tax_rate, Total_sail_rate2, discount_rate, new_sailrate, ItemRemarks, ItemNo, MaterialNo, PackSize, Department, DeliveryDate FROM tbl_Quotaion_details WHERE Quotation_no=@Quotation_no AND IsLatest=1 AND IsDeleted=0 ORDER BY ItemNo` | `tbl_Quotaion_details` (table name misspelled in DB) |
| 7 | `SELECT phase_type, PhaseDesc, amountper FROM tbl_QutPaymentPhase WHERE qut_no=@qut_no ORDER BY id` | `tbl_QutPaymentPhase` |
| 8 | `SELECT Chalan_No, Chalan_Date FROM tbl_Chalan WHERE Quotation_No=@Quotation_No ORDER BY Chalan_Date` | `tbl_Chalan` |
| 9 | Challan lines: `tbl_Challan_details cd INNER JOIN tbl_Chalan c ON cd.Challan_no=c.Chalan_No INNER JOIN tbl_Quotaion_details qd ON cd.Product_id=qd.Product_Code AND c.Quotation_No=qd.Quotation_no AND cd.ItemNo=qd.ItemNo WHERE cd.Challan_no=@Challan_no AND qd.IsDeleted!=1 AND qd.IsLatest=1 ORDER BY CAST(qd.Sl_no AS int)` | `tbl_Challan_details`, `tbl_Chalan`, `tbl_Quotaion_details` |
| 10 | `SELECT PrimaryService FROM tbl_QutPrimaryService WHERE qut_no=@qut_no` | `tbl_QutPrimaryService` |
| 11 | `SELECT PrimaryServiceTerms FROM tbl_PrimaryServiceTerms WHERE PrimaryService=@PrimaryService` | `tbl_PrimaryServiceTerms` |
| 12 | `SELECT PSerTer FROM tbl_QuoPserTerm WHERE qutno=@qutno AND PServiceName=@PServiceName` | `tbl_QuoPserTerm` |

**Join graph (invoice lists):**

```
tbl_Invoice (a)
  ├─ tbl_Client (b)              ON Client_Id = Client_ID          [no CompanyID on join]
  ├─ tbl_QuoPriSerTogather (c)   ON Quotation_No = qutno           [no timestamp; fan-out risk]
  ├─ tbl_Quotation (q)           ON Quotation_no = Quotation_No    [no CompanyID on join]
  └─ tbl_login (l)               ON User_Id = AddedById            [no CompanyID on join]

Export extra:
  tbl_Invoice_details (d)        ON Invoice_No = Invoice_No        [no CompanyID on join]
```

**Join graph (PO print):**

```
tbl_Quotation
  ├─ tbl_Client                  ON Client_Id
  ├─ tbl_representative          ON Copany_Id = Client_Id
  ├─ tbl_QutPrimaryService       ON qut_no
  ├─ tbl_Quotaion_details        ON Quotation_no (IsLatest/IsDeleted)
  ├─ tbl_QutPaymentPhase         ON qut_no
  ├─ tbl_Chalan                  ON Quotation_No
  │    └─ tbl_Challan_details    ON Challan_no
  │         └─ tbl_Quotaion_details ON Product_id/Product_Code + ItemNo + Quotation_no
  ├─ tbl_PrimaryServiceTerms     ON PrimaryService name
  └─ tbl_QuoPserTerm             ON qutno + PServiceName
```

None of the PO-print queries include `CompanyID`.

---

## 4. GridView column inventory

**There is no `GridView` on any of the three target pages.**

Invoice lists use a sticky-header HTML `<table class="styled-table">` with `asp:Repeater ID="rptInvoices"`. PO print builds HTML tables in `StringBuilder` and assigns them to Labels.

### 4.1 Invoice Repeater columns (Search and View — identical markup)

| # | Header | Width | Bound expression | Alignment | Notes |
|---|--------|-------|------------------|-----------|-------|
| 1 | Sl | 3% | `Container.ItemIndex + 1` | center | Not from DB |
| 2 | Customer Name | 14% | `Eval("Client_Name")` | left | Bold |
| 3 | Inv Date | 9% | `Eval("Invoice_Date")` | center | Raw DB string, not reformatted |
| 4 | Invoice / Quotation Info | 16% | `Invoice_No`, `ExtInvoiceNo` (conditional), `Quotation_No` (yellow if `VERBAL`), `PServiceName` | left | Compound cell |
| 5 | ARC / PO / DO | 12% | `PO_Number` as ARC, `DO_Number` as PO/DO | left | Labels inverted vs typical PO/DO naming |
| 6 | Amount Summary | 16% | `Gross`, `discount` (if > 0), `sub_total`, tax badge from `cgstOrsgst`/`igst` + `Gst`, Frt/Oth from `Delivery_Amount`+`otherAmount1`, `Net_Amount` | right | Discount/freight rows omitted when zero |
| 7 | Validity | 11% | `Validity_StartDate` / `Validity_EndDate` | center | From quotation, not invoice |
| 8 | Created By | 11% | `AddedByName` + `TimeStamp` formatted `dd-MMM-yyyy hh:mm tt` | center | `Convert.ToDateTime` will throw if TimeStamp null |
| 9 | Buyer | 4% | popup `NewInvoice.aspx?ID=` | center | Icon only |
| 10 | Seller | 4% | popup `NewInvoiceDuplicate.aspx?ID=` | center | Icon only |

Footer: “No Invoices Found…” when repeater item count is 0.

### 4.2 PO print — commercial items table (`Buindamount` → `lblserviceamo`)

| Column | Always? | Source |
|--------|---------|--------|
| Sl | yes | loop index |
| Product Name & Specification | yes | `Product_name`; Detailed view also Make=`specification`, Specification=`Misc`, Item No, Material No, Pack Size |
| HSN Code | yes | `Product_id AS HSN` |
| Qty | yes | `Quantity` + `Unit` |
| Base Rate | yes | `sail_rate` |
| Disc (%) | if `DiscountView == "Yes"` | `discount_rate` |
| Disc Rate | if `DiscountView == "Yes"` | `new_sailrate` |
| GST (%) | yes | `Service_tax_rate` |
| Amount (₹) | yes | qty × discounted rate |
| Remarks | yes | `ItemRemarks` |
| Department & Delivery Date | yes | `Department`, `DeliveryDate` |

Footer rows: GRAND TOTAL, TOTAL AMOUNT BEFORE TAX, TOTAL GST + amount in words, optional FREIGHT / TCS / OTHER CHARGES, TOTAL AMOUNT AFTER TAX.

### 4.3 PO print — challan / delivery table (`BuindamountByQuotation` → `lblProductDetails`)

Per challan: S.NO, PARTICULARS, PRODUCT ID, HSN CODE, QTY, PACK SIZE, ITEM NO, MATERIAL NO, DEPT, plus Total Quantity row. Empty state: *No challan data found for this quotation.*

### 4.4 PO print — payment schedule (`bindpayment` → `lblPayment`)

S.NO, PAYMENT PHASE (`phase_type` + `PhaseDesc`), AMOUNT (INR) = `Net_amount * amountper / 100`. Rendered only if phase rows exist.

---

## 5. Hidden fields available but not displayed

No `asp:HiddenField` controls exist on any of the three pages.

### 5.1 Invoice grid — selected in SQL, not shown as their own column

| Result-set column | Visible? | Where it goes |
|-------------------|----------|---------------|
| `ID` | No as text | Buyer/Seller print URLs only |
| `Quotation_Date` | No | Not used in template |
| `mailDate` | No | Selected, never Eval’d |
| `AddedById` | Indirect | Fallback inside `AddedByName`; export uses raw ID |
| `cgstOrsgst` / `igst` | Badge only | Not exported |
| `Delivery_Amount` / `otherAmount1` | Combined “Frt/Oth” only if sum > 0 | Not separately labeled; other charge **name** never selected |
| `discount` | Only if > 0 | Hidden when zero |
| `ExtInvoiceNo` | Only if non-blank | |
| `sub_total` | Yes, but **computed** `Net_Amount - Service_Tax1`, not `tbl_Invoice.sub_total` | |
| `Gst` | Yes (alias of `Service_Tax1`) | |

### 5.2 Invoice header columns known from INSERT (`Add_invoice`) but never queried by these pages

`status1`, `status2`, `SalesPersonCode`, `ExtInvoiceDate`, `BillingAddress`, `otherAmount1_name`, `Service_Tax` (vs `Service_Tax1`), `Sl_no`, `CompanyID` (filter only).

**Blocked invoices (`status2='Block'`) still appear.** `Block_invoice.aspx` filters `status2='Active'`; these two pages do not.

### 5.3 Purchase Order print — markup `Visible="false"` or never shown

| Control | Markup | Code sets visible? |
|---------|--------|--------------------|
| `lblrename` | `Visible="false"` | Never. Representative name is built but not shown in the Name row (`lbl_refname` is the quotation reference name). |
| `ref_desg` + `lbldeg` | row `visible="false"` | Never set true. Designation is bound but hidden. |
| `lblClientCode` | `Visible="false"` | Bound to `Client_Id`; also copied to visible `Label2` in the header as `[clientid]`. |
| `pnlPanGst` + `lblPanno` + `lblGstno` | panel `Visible="false"` | PAN/GST **are bound** in `Bindclientdetails`; panel is **never** set `Visible=true`. Dead UI. |
| `lbl_val_dates` | `Visible="false"` | Intended visible when `RecordType == "Purchase Order"` via `FindControl` (likely fails; see §10). |
| `Table2` (MATERIAL ACCEPTANCE) | `visible="false"` | Always hidden. |
| `lblprimary_service` | visible | **Never assigned** in code-behind. |
| Representative `Phone_no` / `Email` | n/a | Selected, not rendered (client email/phone from `tbl_Client` go to `lblContact`). |
| `ReferenceData` | n/a | Read only to drive empty checks. |
| `TimsStamp` | n/a | Selected, unused. |
| `PlaceofSupply` on client | n/a | Selected; header uses quotation `PlaceofSupply` instead. |

CSS-hidden at print time: `#print-controls`; optionally thead/tfoot when Button1 assigns `.header` / `.footer`.

---

## 6. Existing Excel export implementation

### 6.1 Invoice Search and View (in scope)

| Item | Search (`WebForm28`) | View (`WebForm27`) |
|------|----------------------|--------------------|
| Button text | Export Excel | Export Excel |
| Handler | `btnExport_Click` | `btnExport_Click` |
| Helper | `ExportDataTableToCsv` | `ExportDataTableToExcel` (**CSV despite the name**) |
| Content-Type | `text/csv` | `text/csv` |
| Filename | `Advanced_Search_Invoices_yyyyMMdd.csv` | `Tax_Invoices_Export_yyyyMMdd.csv` |
| Library | none (StringBuilder) | none |
| Grain | **Line item** (`tbl_Invoice_details`), not one row per invoice | same |
| Filters | Copied from UI (client exact / LIKE, inv, ERP, dates) + `CompanyID` | same |
| CSV escaping | Quote wrap if `,` `"` CR LF; `"` doubled | identical |
| Empty set | Message panel | Message panel |
| Notification | none on export | `InsertSystemNotification` on success |
| `Response.End()` | yes | yes |

Export columns: Invoice Number, Invoice Date, ERP Ref, Client Name, Item Code, HSN Code, Item Name, Qty, Rate, Taxable Value, GST %, Item Net Value, Invoice Grand Total, Created By (**user id**, not display name).

**Not in export:** Validity, ARC/PO/DO, quotation no, service name, tax type (CGST/IGST), discount, freight, timestamp, AddedByName, blocked status.

### 6.2 Adjacent ClosedXML pattern (not in target files, relevant for refactor)

`View_PurchaseOrder.aspx.cs` and `Search_purchaseorder.aspx.cs` already emit real `.xlsx` via ClosedXML (`XLWorkbook`, freeze header, numeric formats, `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`). Invoice export has **not** been upgraded to that pattern.

`NewPurchaseOrder.aspx` has **no export**.

---

## 7. Purchase Order rendering pipeline

```
QueryString ID
    │
    ▼
tbl_Quotation header  ──► labels (DO, ARC/PO, dates, refs, place of supply, terms knobs)
    │                     Session: cgstOrsgst, igst, DiscountView, TCS, Freight, OtherCharge_*, Quotation_date
    │                     static viewtype = DetailedView   ◄── process-wide static
    │
    ├─ tbl_Client ────────► Label1 / lblClient (TitleCase name)
    │                       address / city / pin-state
    │                       lblContact = Email | Phone
    │                       PAN/GST bound but panel stays hidden
    │                       Label2 = Client_Id shown in header as [id]
    │
    ├─ tbl_representative ► lbltital + lbllname (greeting “To Flame-ex Team, {title} {lastname}”)
    │                       lbldeg / lblrename bound, markup hidden
    │
    ├─ tbl_QutPrimaryService ► quoted service names on subject line + body
    │
    ├─ tbl_Quotaion_details  ► HTML commercial table (lblserviceamo)
    │         uses Session DiscountView / TCS / Freight / Other
    │         Detailed vs compact from static viewtype
    │         totals recomputed in C# (not DB Net_amount except payment %)
    │
    ├─ tbl_QutPaymentPhase   ► HTML payment table (lblPayment)
    │         amount = header Net_amount × amountper%
    │
    ├─ tbl_Chalan + details  ► HTML delivery schedules (lblProductDetails)
    │         relative “Today / N days ago / N days left”
    │
    └─ primary service terms ► HTML (lblPrimaryServicePoint)
              only if Session["pserTerm"] set during bindterms

Terms block (mostly static copy + a few labels):
    VALIDITY OF OFFER     ValidityDays, or date range if RecordType=="Purchase Order"
    GST APPLICABILITY     hardcoded “charged extra”
    DELIVERY TERMS        DeliveryTenure
    MATERIAL ACCEPTANCE   hidden
    PACKING & FORWARDING  PackingCharges
    SPECIAL INSTRUCTIONS  Remarks

Fixed assets:
    ../WebImages/flame-ex_hdrtop.png
    ../WebImages/flame-ex_hdrbtm.png
    ../WebImages/flmx_authsign.png
```

RecordType handling: if `"Purchase Order"`, code tries to hide the “valid for N Days” labels and show `lbl_val_dates` (`Valid from {start} to {end}`). That uses `FindControl` from the Page, which does **not** search nested naming containers, so the switch is likely a no-op and the default “15 Days” copy remains unless ViewState already has the right values.

Challan vs commercial table: both always run. Commercial table is the priced offer; challan table is delivery schedule. If there are no challans, an italic empty message still appears under “Delivery Schedules”.

---

## 8. Print CSS analysis

All print CSS is **inline in `NewPurchaseOrder.aspx` `<head>`**. Invoice search/view pages have screen-only table CSS and **no `@media print`**.

### 8.1 Screen

- `body`: Century Gothic 13px, `#f4f4f4` canvas, 20px vertical padding.
- `.a4-container`: `max-width: 844px`, centered white sheet, `20px 40px` padding, box-shadow.
- `.header, .footer, .hide { visibility: hidden; }` — **class** selectors. Initial markup uses `id="header"` / `id="footer"` **without** those classes, so letterhead is visible on screen.

### 8.2 `@media print`

| Rule | Effect |
|------|--------|
| `body` background transparent, padding 0 | Drops gray canvas |
| `.a4-container` shadow none, padding 0, max-width 100% | Fills printable area |
| `* { -webkit-print-color-adjust: exact; print-color-adjust: exact }` | Keeps `#24285F` header fills and `#e31e24` accents |
| `#print-controls { display: none !important }` | Hides both print buttons |
| `thead { display: table-header-group }` `tfoot { display: table-footer-group }` | Native repeating letterhead |
| `tr { page-break-inside: avoid }` | Tries to keep rows together |
| `.pagebrake { page-break-inside: avoid }` `.pagebrake1 { page-break-before: always }` | Custom breaks (`.pagebrake1` unused in markup) |
| `thead img, tfoot img { width: 100%; max-width: 844px }` | Letterhead images |

`@page { margin: 8mm 10mm; }` — uniform page box; no named pages, no `size: A4`.

### 8.3 Letterhead toggle (fragile)

- **Print With Letterhead (Button2):** `window.print()` only. thead/tfoot stay unclassed → visible. Matches button label.
- **Print Without Letterhead (Button1):** JS assigns `className='header'` / `'footer'` then prints. CSS hides them. **Also posts back** via empty server handler; after postback ViewState restores markup **without** those classes, so the on-screen copy shows letterhead again (usually acceptable).
- Both buttons cause a full postback around `window.print()`, which is a common “print dialog then blank/reload” race.

Gaps: no `size: A4`; no orphan/widow control beyond `page-break-inside: avoid`; dynamically injected tables in Labels are not `page-break` aware except parent `.pagebrake` on some wrappers; Font Awesome is loaded but used only in injected terms HTML.

Invoice list CSS (not print): brand blues `#19658A` / `#006699`, sticky thead, 600px scroll body, Select2 contrast fixes on search page only.

---

## 9. CompanyID isolation verification

`CompanyContext.CurrentCompanyID` reads `Session["CompanyID"]`, else **0**. Defined in `Bill.Master.cs`. Print page does not use the master.

| Surface | Isolated? | Evidence |
|---------|-----------|----------|
| Search grid | **Partial pass** | `WHERE a.CompanyID = @CompanyID` parameterized |
| Search client dropdown | **Pass** | `tbl_Client WHERE CompanyID = @CompanyID` |
| Search export | **Partial pass** | Header `a.CompanyID`; details join **without** `d.CompanyID` |
| View grid | **Partial pass** | Same header filter; stale comment is wrong — column is used |
| View export | **Partial pass** | Same as search export |
| View client filter | **Weak** | `LIKE` on name, join not `AND b.CompanyID = @CompanyID` |
| Quotation / service / login joins (both lists) | **Fail** | No `CompanyID` on `tbl_Quotation`, `tbl_QuoPriSerTogather`, `tbl_login`, `tbl_Invoice_details` |
| Notifications | **Fail** | Insert omits `CompanyID` |
| Invoice print popups (`NewInvoice.aspx`) | **Fail** | SQL `WHERE i.ID = '{ID}'` concatenated; no CompanyID. `NewInvoice_v2.aspx.cs` **does** isolate but is not what the Repeater opens |
| `NewPurchaseOrder.aspx` | **Fail** | No session check, no CompanyID on any of 12 queries. IDOR by guessing `tbl_Quotation.ID` |
| `CompanyID = 0` fallback | **Risk** | Lost session on a master page still runs queries for company 0 |

Contrast: `View_PurchaseOrder.aspx.cs` list/export **does** use `tbl_Quotation.CompanyID = @CompanyID`, then redirects to the unscoped print page.

`tbl_QuoPriSerTogather` is joined only on `qutno`. `View_PurchaseOrder` also matches `TimeStamp = TimsStamp`. Invoice pages do not, so multiple primary-service rows duplicate invoices in the grid.

---

## 10. Risks for future refactoring

1. **Filename / type names.** `seartch_invoice.aspx`, `WebForm27` / `WebForm28`, `Buindamount`, `buindalldata`, `tbl_Quotaion_details`, `Copany_Id`, `TimsStamp`. Any rename breaks routing, Inherits, and bookmarks.

2. **Duplicated twins.** Search and View share ~90% of SQL, Repeater markup, CSS, and CSV helpers. Diverging them further (already: default dates, client control, notification timing, helper names) will make a later “one export” change miss a copy.

3. **Not a GridView.** A refactor that assumes `GridView` columns / `AllowPaging` / `Export` via grid will miss the Repeater + separate export query design.

4. **Export grain ≠ UI grain.** UI is one row per invoice; export is one row per line item. Replacing CSV with ClosedXML (as PO view already does) must keep that product decision explicit.

5. **Join fan-out.** `tbl_QuoPriSerTogather` without timestamp/CompanyID can duplicate invoices. `tbl_Invoice_details` join without CompanyID can leak or duplicate lines if invoice numbers collide across tenants.

6. **Computed vs stored `sub_total`.** Grid shows `Net_Amount - Service_Tax1`, not `tbl_Invoice.sub_total`. Freight/other sit outside that taxable figure. Totals on PO print are recomputed in C# from line rates, then payment % uses DB `Net_amount` — two sources of truth.

7. **No `status2` filter.** Blocked invoices remain searchable/exportable/printable from these pages.

8. **Static leak.** `public static string viewtype` on `NewPurchaseOrder` violates the project “Zero Static Leaks” rule and can mix Detailed/compact layout across concurrent users.

9. **Session as scratchpad.** PO print writes `Session["DiscountView"]`, `TCS_Amount`, `Freight_Amount`, `cgstOrsgst`, `pserTerm`, `Quotation_date`. Concurrent tabs overwrite each other.

10. **`FindControl` for validity labels** is non-recursive; Purchase Order date-range validity likely never toggles.

11. **SQL concatenation** in `BindService` / `generatelavel` (`qut_no='" + qutno + "'`). Quotation numbers are usually system-generated, but this is still injection surface. Invoice print popups concatenate `ID`.

12. **IDOR on print.** `NewPurchaseOrder.aspx?ID=` and `NewInvoice.aspx?ID=` have no auth/tenant check. Master-page token checks do not apply.

13. **PAN/GST panel dead.** Bound but `pnlPanGst.Visible` never set true. A “show GST on PO” request is a one-line visibility change, not a data change.

14. **`lblprimary_service` unbound.** Subject line has a trailing empty label.

15. **Print postback.** Empty `Button1_Click` / `Button2_Click` force postback around `window.print()`.

16. **CSV labeled Excel.** Users/Excel will open CSV; rupee amounts, dates (`dd-MMM-yyyy` strings), and `AddedById` will not be typed. ClosedXML already exists in the PO list pages.

17. **Notification schema drift.** These pages insert `ModuleType` / `AlertLevel` / `CreatedBy` without `CompanyID`. Other pages use different column sets. A shared helper will break one of the inserts.

18. **Date nulls.** `Convert.ToDateTime(Eval("TimeStamp"))` in the Repeater has no null guard.

19. **Client match inconsistency.** Search: exact `Client_Name` after CompanyID-scoped dropdown. View: `LIKE` without client-table CompanyID. Same customer name in two companies can cross-hit on View.

20. **`Response.End()`** throws `ThreadAbortException`; catch on export will sometimes show a false error after a successful download.

21. **Challan join** `cd.Product_id = qd.Product_Code` (id vs code) plus `ItemNo` — fragile if either is blank; commercial table still renders.

22. **Letterhead CSS vs JS.** Button1 relies on class names matching `.header`/`.footer`. Changing those class names for layout will invert “with/without letterhead”.

---

## Field-mapping matrix

**Legend — Currently Visible:** Yes = shown in the default UI; Conditional = shown only when data/flags allow; Hidden = bound or queried but not shown; No = not queried by that page.  
**Export Candidate:** Yes = already in invoice CSV; Strong = useful and already in the result set or an adjacent ClosedXML export; Possible = available in DB used by these flows but not exported; No = UI-only / not a data field.

### A. Advanced Search Invoice + View Tax Invoices

| UI Element | DB Table | DB Column | Currently Visible | Export Candidate |
|------------|----------|-----------|-------------------|------------------|
| Sl (row number) | — | — | Yes | No |
| Customer Name | `tbl_Client` | `Client_Name` | Yes | Yes (CSV: Client Name) |
| Inv Date | `tbl_Invoice` | `Invoice_Date` | Yes (raw) | Yes (CSV: Invoice Date) |
| Invoice No | `tbl_Invoice` | `Invoice_No` | Yes | Yes (CSV: Invoice Number) |
| Ext Ref (ERP) | `tbl_Invoice` | `ExtInvoiceNo` | Conditional (badge if non-blank) | Yes (CSV: ERP Ref) |
| Quotation No | `tbl_Invoice` | `Quotation_No` | Yes (yellow if VERBAL) | Strong |
| Quotation Date | `tbl_Invoice` | `Quotation_Date` | Hidden (selected, not bound) | Possible |
| Primary service | `tbl_QuoPriSerTogather` | `PServiceName` | Yes (small text) | Strong |
| ARC | `tbl_Quotation` | `PO_Number` | Yes | Strong |
| PO/DO | `tbl_Quotation` | `DO_Number` | Yes | Strong |
| Gross | `tbl_Invoice` | `Gross` | Yes | Possible |
| Discount | `tbl_Invoice` | `discount` | Conditional (> 0) | Possible |
| Taxable (sub_total) | derived | `Net_Amount - Service_Tax1` | Yes | Possible (not stored column) |
| Tax type badge | `tbl_Invoice` | `cgstOrsgst` / `igst` | Yes (CGST/SGST vs IGST vs TAX) | Possible |
| GST amount | `tbl_Invoice` | `Service_Tax1` AS `Gst` | Yes | Possible |
| Freight/Other | `tbl_Invoice` | `Delivery_Amount` + `otherAmount1` | Conditional (sum > 0) | Possible |
| Other charge name | `tbl_Invoice` | `otherAmount1_name` | No | Possible |
| Total / Net | `tbl_Invoice` | `Net_Amount` | Yes | Yes (CSV: Invoice Grand Total) |
| Validity start | `tbl_Quotation` | `Validity_StartDate` | Yes | Strong |
| Validity end | `tbl_Quotation` | `Validity_EndDate` | Yes | Strong |
| Created By (name) | `tbl_login` | `Name` (`ISNULL` → `AddedById`) | Yes | Strong (CSV currently exports ID) |
| Created By (id) | `tbl_Invoice` | `AddedById` | Hidden | Yes (CSV: Created By) |
| Created timestamp | `tbl_Invoice` | `TimeStamp` | Yes (formatted) | Strong |
| Mail date | `tbl_Invoice` | `mailDate` | Hidden | Possible |
| Buyer print | `tbl_Invoice` | `ID` | Icon only | No |
| Seller print | `tbl_Invoice` | `ID` | Icon only | No |
| Item Code | `tbl_Invoice_details` | `Product_id` | No (export only) | Yes |
| HSN Code | `tbl_Invoice_details` | `Product_Code` | No (export only) | Yes |
| Item Name | `tbl_Invoice_details` | `Product_name` | No (export only) | Yes |
| Qty | `tbl_Invoice_details` | `Quantity` | No (export only) | Yes |
| Rate | `tbl_Invoice_details` | `sail_rate` | No (export only) | Yes |
| Taxable Value | `tbl_Invoice_details` | `Total_sail_rate2` (fallback qty×rate) | No (export only) | Yes |
| GST % | `tbl_Invoice_details` | `Service_tax_rate` | No (export only) | Yes |
| Item Net Value | `tbl_Invoice_details` | `Total_sail_rate1` | No (export only) | Yes |
| Search: Client dropdown | `tbl_Client` | `Client_Name` | Yes (filter) | No |
| View: Client textbox | `tbl_Client` | `Client_Name` LIKE | Yes (filter) | No |
| Invoice status | `tbl_Invoice` | `status2` | No | Strong (blocked vs active) |
| Payment status | `tbl_Invoice` | `status1` | No | Possible |
| Sales person | `tbl_Invoice` | `SalesPersonCode` | No | Possible |
| Billing address | `tbl_Invoice` | `BillingAddress` | No | Possible |
| Ext invoice date | `tbl_Invoice` | `ExtInvoiceDate` | No | Possible |
| Company | `tbl_Invoice` | `CompanyID` | Filter only | No (isolation key) |

### B. Purchase Order print (`NewPurchaseOrder`)

| UI Element | DB Table | DB Column | Currently Visible | Export Candidate |
|------------|----------|-----------|-------------------|------------------|
| Header client name | `tbl_Client` | `Client_Name` | Yes (`Label1`) | Strong (PO ClosedXML has Client Name) |
| Header `[Client_Id]` | `tbl_Quotation` / `tbl_Client` | `Client_Id` | Yes (`Label2`) | No |
| Customer Name (From) | `tbl_Quotation` | `ReferenceName` | Conditional (`lbl_refname`; hidden if empty/N/A) | Strong (Client Ref Name) |
| Representative full name | `tbl_representative` | `RepTitle`+`Representative_name`+`RepLastName` | Hidden (`lblrename`) | Possible |
| Designation | `tbl_representative` | `Designation` | Hidden (`ref_desg`) | Possible |
| Company (From) | `tbl_Client` | `Client_Name` | Yes (`lblClient`) | Strong |
| Address | `tbl_Client` | `Address1` / `Address2` | Yes | Possible |
| City | `tbl_Client` | `City` | Yes | Possible |
| Pin – State | `tbl_Client` | `pin`, `State` | Yes (concatenated) | Possible |
| Email / Phone | `tbl_Client` | `Com_email`, `Com_phone` | Yes (`lblContact`) | Possible |
| D.O. / P.O. No | `tbl_Quotation` | `DO_Number` | Yes | Strong |
| ARC No | `tbl_Quotation` | `PO_Number` | Yes | Strong |
| Date (PO) | `tbl_Quotation` | `PO_Date` | Yes (`No Data` if unparsable) | Strong |
| ERP Record | `tbl_Quotation` | `Quotation_no` | Yes | Strong (Document Number) |
| ERP Record date | `tbl_Quotation` | `Quotation_date` | Yes | Strong |
| Ref ID | `tbl_Quotation` | `ReferenceId` | Conditional (row hidden if ID and date both empty) | Strong |
| Ref Date | `tbl_Quotation` | `ReferenceDate` | Conditional | Possible |
| Supply Place | `tbl_Quotation` | `PlaceofSupply` | Yes | Strong |
| Client PAN | `tbl_Client` | `Pan_no` | Hidden (`pnlPanGst`) | Possible |
| Client GST | `tbl_Client` | `Service_tax_no` | Hidden (`pnlPanGst`) | Possible |
| Subject services | `tbl_QutPrimaryService` | `PrimaryService` | Yes | Possible |
| Greeting title/last name | `tbl_representative` | `RepTitle`, `RepLastName` | Yes | No |
| Line: Product / spec | `tbl_Quotaion_details` | `Product_name`, `specification`, `Misc` | Yes (Detailed vs compact via `DetailedView`) | Strong |
| Line: HSN | `tbl_Quotaion_details` | `Product_id` (aliased HSN) | Yes | Strong |
| Line: Qty + Unit | `tbl_Quotaion_details` | `Quantity`, `Unit` | Yes | Strong |
| Line: Base Rate | `tbl_Quotaion_details` | `sail_rate` | Yes | Strong |
| Line: Disc % / Disc Rate | `tbl_Quotaion_details` | `discount_rate`, `new_sailrate` | Conditional (`DiscountView=Yes`) | Strong |
| Line: GST % | `tbl_Quotaion_details` | `Service_tax_rate` | Yes | Strong |
| Line: Amount | derived | qty × `new_sailrate` | Yes | Strong |
| Line: Remarks | `tbl_Quotaion_details` | `ItemRemarks` | Yes | Strong (Doc Remarks is header-level in ClosedXML) |
| Line: Dept / Delivery | `tbl_Quotaion_details` | `Department`, `DeliveryDate` | Yes | Possible |
| Line: ItemNo / MaterialNo / PackSize | `tbl_Quotaion_details` | `ItemNo`, `MaterialNo`, `PackSize` | Conditional (Detailed view only) | Possible |
| Freight | `tbl_Quotation` | `Freight_Amount` | Conditional (> 0) | Possible |
| TCS | `tbl_Quotation` | `TCS_Amount` | Conditional (> 0) | Possible |
| Other charges | `tbl_Quotation` | `OtherCharge_Name`, `OtherCharge_Amount` | Conditional (amount > 0) | Possible |
| Amount in words | derived | C# `MoneyConvDS.MoneyConvFn` | Yes | No |
| Payment phase | `tbl_QutPaymentPhase` | `phase_type`, `PhaseDesc`, `amountper` | Conditional (if rows) | Possible |
| Payment amount | derived | header `Net_amount` × `%` | Conditional | Possible |
| Challan No / Date | `tbl_Chalan` | `Chalan_No`, `Chalan_Date` | Conditional | Possible |
| Challan product / HSN / qty / pack / item / material / dept | `tbl_Challan_details` + `tbl_Quotaion_details` | `Product_name`, `Product_id`, `Product_code`, `Quantity`, `PackSize`, `ItemNo`, `MaterialNo`, `Department` | Conditional | Possible |
| Specific service terms | `tbl_QuoPserTerm` | `PSerTer` | Conditional | No |
| Validity days | `tbl_Quotation` | `ValidityDays` | Yes unless PO date-range path works | Strong |
| Validity date range | `tbl_Quotation` | `Validity_StartDate`, `Validity_EndDate` | Intended for `RecordType=Purchase Order`; likely still hidden | Strong |
| Delivery tenure | `tbl_Quotation` | `DeliveryTenure` | Yes | Strong |
| Packing charges text | `tbl_Quotation` | `PackingCharges` | Yes | Possible |
| Special instructions | `tbl_Quotation` | `Remarks` | Yes | Strong |
| GST applicability copy | — | hardcoded | Yes | No |
| Material acceptance | — | hardcoded | Hidden | No |
| Record type | `tbl_Quotation` | `RecordType` | Hidden (logic only) | Strong |
| Detailed view flag | `tbl_Quotation` | `DetailedView` | Hidden (layout switch) | No |
| Discount view flag | `tbl_Quotation` | `DiscountView` | Hidden (column switch) | No |
| Net amount (header) | `tbl_Quotation` | `Net_amount` | Indirect (payments / words path) | Strong |
| Subtotal (header) | `tbl_Quotation` | `sub_total` | Hidden (session/local only) | Strong |
| CompanyID | `tbl_Quotation` | `CompanyID` | **Not queried** | Isolation gap |

---

## Appendix: file and class index

| File | Class | Master | Auth |
|------|-------|--------|------|
| `Bill_Software/corporate/business/app/seartch_invoice.aspx.cs` | `WebForm28` | `Bill.Master` | `Session["USERID"]` + master token |
| `Bill_Software/corporate/business/app/View_Invoice.aspx.cs` | `WebForm27` | `Bill.Master` | same |
| `Bill_Software/corporate/business/print/NewPurchaseOrder.aspx.cs` | `NewPurchaseOrder` | none | **none** |

Helper: `DB_UTILITY.SPreturn_dt` → `CommandType.Text`. Tenant helper: `CompanyContext.CurrentCompanyID` in `Bill.Master.cs`.
