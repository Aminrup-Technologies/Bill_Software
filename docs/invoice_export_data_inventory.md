# Invoice Export Data Inventory

**When:** 2026-09-04  
**Why:** Column selection for the Search Invoice / View Invoice Excel export must wait until finance and operations can map fields from actual application usage, not from schema existence or speculative enrichment.  
**What:** Read-only inventory of tables and columns involved in invoice generation and in the current Search Invoice / View Invoice display and export SQL, plus a proposed export mapping for finance/operations review. Export SELECT columns are not changed in this iteration.

This document does **not** implement additional export columns.

**Sources (code only, no live database):**

| Surface | Files |
|---|---|
| Search Invoice | `Bill_Software/corporate/business/app/seartch_invoice.aspx` + `.cs` |
| View Invoice | `Bill_Software/corporate/business/app/View_Invoice.aspx` + `.cs` |
| Shared export formatting | `Bill_Software/corporate/business/app/InvoiceListHelper.cs` |
| Invoice create | `Add_invoice.aspx.cs`, `Manual_Invoice.aspx.cs` |
| Invoice print (opened from list `ID`) | `print/NewInvoice.aspx.cs` |
| Quotation / client / login writes | `Create_quotation.aspx.cs`, `New_client.aspx.cs`, `AddUser.aspx.cs` |

**Out of scope for this inventory as Search/View SQL:** `tbl_invoice_payment`, `tbl_InvSiteAddress`, `tbl_representative`, `tbl_NewProduct`. Those are used by payment, print, or stock modules, not by the list/export queries.

---

## 1. How the two pages use data today

| Concern | Search Invoice | View Invoice |
|---|---|---|
| UI grain | One repeater row per invoice header | Same |
| Export grain | One Excel row per `tbl_Invoice_details` line | Same |
| Default dates | Blank (all history until filtered) | Current month (`Page_Load` + Clear) |
| Client filter | Dropdown equality `b.Client_Name = @ClientName` | Text `b.Client_Name LIKE @Client` |
| CompanyID | `WHERE a.CompanyID = @CompanyID` | `WHERE 1=1` then `AND a.CompanyID = @CompanyID` |
| Grid joins | Client, primary service, quotation, login | Same |
| Export joins | Client + details, both with `CompanyID` | Same |
| Notification | Once on search | Once on export, before `ExportXlsx` |
| Sheet / file | `Invoice_Lines` / `{CompanyCode}_Advanced_Search_Invoices_{yyyyMMdd}` | `Invoice_Lines` / `Tax_Invoices_Export_{yyyyMMdd}` |

Grid joins on `tbl_QuoPriSerTogather` are **not** CompanyID-scoped and can duplicate header rows if multiple primary-service rows exist for one quotation. Export does not use that join.

Export joins `tbl_Client` on `Client_Id` **and** `b.CompanyID = @CompanyID`, and `tbl_Invoice_details` on `Invoice_No` **and** `d.CompanyID = @CompanyID`. Header filter remains `a.CompanyID = @CompanyID`. Grid BindData joins are still unscoped (not changed in this iteration).

---

## 2. Current export column order (unchanged)

Both pages, line-item grain:

1. Invoice Number  
2. Invoice Date  
3. ERP Ref  
4. Client Name  
5. Item Code  
6. HSN Code  
7. Item Name  
8. Qty  
9. Rate  
10. Taxable Value  
11. GST %  
12. Item Net Value  
13. Invoice Grand Total  
14. Created By  
15. Quotation Date  
16. Mail Date  
17. Tax Type  
18. Freight  
19. Other Charges  
20. Created Timestamp  

Header amounts (grand total, freight, other charges) and header dates already repeat on every line of a multi-line invoice.

---

## 3. Table inventory

Legend:

| UI | Export | Duplicate on line grain | CompanyID in these flows |
|---|---|---|---|
| Shown on Search/View repeater | In current export SELECT | Yes = same value copied on every line of that invoice | How Search/View currently apply it |

### 3.1 `tbl_Invoice` (alias `a`) — invoice header

Written at create (`Add_invoice` / `Manual_Invoice`). List pages select a subset. Print `NewInvoice` selects another subset.

| Column | Apparent meaning from usage | Class | UI | Export | CompanyID-scoped | Duplicate on line grain |
|---|---|---|---|---|---|---|
| `ID` | Surrogate key; print popup `?ID=` | invoice-header | Print icons only | No | Filter is on `CompanyID`, not `ID` | N/A (not exported) |
| `Invoice_No` | Human invoice number; details join key | invoice-header | Yes | Yes (`Invoice Number`) | Via header `a.CompanyID` | Yes |
| `Invoice_Date` | Invoice date; date filters use `TRY_CONVERT(..., 106)` | invoice-header | Yes (`FmtDate`) | Yes | Via header | Yes |
| `ExtInvoiceNo` | ERP / external invoice ref; Search/View filter | invoice-header | Conditional badge | Yes (`ERP Ref`) | Via header | Yes |
| `ExtInvoiceDate` | External invoice date; written at create | invoice-header | No | No | Written with header | Would duplicate |
| `Quotation_No` | Source quotation / PO / verbal ref (`VERBAL` styled in UI). Manual invoice may store PO here | quotation / reference | Yes | No | Via header | Would duplicate |
| `Quotation_Date` | Date shown as “Quo Date”. **Not** in current `Add_invoice` INSERT | quotation / reference | Yes (`FmtDate`) | Yes | Via header | Yes |
| `Client_ID` | FK to `tbl_Client.Client_Id` | customer | Join only | Join only | Header yes; **export join now also `b.CompanyID`**; grid join still unscoped | Would duplicate |
| `Gross` | Header gross | invoice-header | Yes | No | Via header | Would duplicate |
| `discount` | Header discount; UI if &gt; 0 | invoice-header | Conditional | No | Via header | Would duplicate |
| `sub_total` | Stored header taxable. **Grid ignores this** and shows `Net_Amount - Service_Tax1` | tax | Derived UI, not stored column | No | Via header | Would duplicate |
| `Service_Tax1` | Header GST amount (`Gst` on grid). Create writes this. Print `NewInvoice` reads `Service_Tax` instead | tax | Yes (as `Gst`) | No | Via header | Would duplicate |
| `Service_Tax` | Print tax label; not selected by Search/View | tax | No (list) | No | Unknown in list SQL | Would duplicate |
| `Net_Amount` | Header grand total | invoice-header | Yes | Yes (`Invoice Grand Total`) | Via header | **Yes — repeats per line** |
| `Delivery_Amount` | Freight; create `@Frt`; print freight | logistics | Yes | Yes (`Freight`) | Via header | **Yes — repeats per line** |
| `otherAmount1` | Other charges amount | logistics | Yes | Yes (`Other Charges`) | Via header | **Yes — repeats per line** |
| `otherAmount1_name` | Other-charge label; create + print | logistics | No | No | Via header | Would duplicate |
| `cgstOrsgst` / `igst` | Intra vs inter GST flags (`YES`). UI/export CASE to CGST/SGST, IGST, or TAX | tax | Yes (badge) | Yes (derived `Tax Type`) | Via header | Yes |
| `mailDate` | Set by `InvoiceMail.aspx` with `mailStatus`; list shows mail badge | audit | Yes (`FmtMail`) | Yes | Via header | Yes |
| `mailStatus` | Updated with `mailDate`; not selected by list/export | audit | No | No | Via header | Would duplicate |
| `AddedById` | Creator user id | audit | Hidden (name shown via login) | Yes (raw id as `Created By`) | Via header | Yes |
| `TimeStamp` | Created timestamp; UI `FmtStamp` | audit | Yes | Yes | Via header | Yes |
| `CompanyID` | Tenant key; `CompanyContext.CurrentCompanyID` | reference | Filter only | Filter only | **Yes — header WHERE** | Must not be exported as a business column |
| `status1` | Create inserts `'No'`. Add-invoice source-doc search uses `status1` as **quotation** pending, not invoice state | status/state | No | No | Via header | Would duplicate |
| `status2` | Create inserts `'Active'`. `Add_invoice` pending-qty treats non-`Active` / `Block` as excluded. Search/View do **not** select or filter it | status/state | No | No | Via header | Would duplicate |
| `SalesPersonCode` | Create from sales-person dropdown | audit | No | No | Via header | Would duplicate |
| `BillingAddress` | Create billing-address text | customer | No | No | Via header | Would duplicate |
| `addressfor` | Print uses Corporate vs other address path | customer | No | No | Via header | Would duplicate |
| `Sl_no` | Sequence at create | reference | No | No | Via header | Would duplicate |

`Cancelled` / `Pending` / `Credit` are **not** invoice `status1`/`status2` values in these flows. Do not treat them as export status labels.

### 3.2 `tbl_Invoice_details` (alias `d`) — invoice line

Used only by export on Search/View (not by the repeater). Create writes the line; print reads a similar set.

| Column | Apparent meaning from usage | Class | UI | Export | CompanyID-scoped | Duplicate on line grain |
|---|---|---|---|---|---|---|
| `Invoice_No` | Parent invoice | invoice-header | No | Join only | Not on join | — |
| `Quotation_no` | Source document number on the line (`Add_invoice` `@RefNo`) | quotation / reference | No | No | Not on join | Line-level; may differ from header |
| `Product_id` | Item / product code (`TrueID` at create) | item/product | No | Yes (`Item Code`) | Not on join | No (line) |
| `Product_Code` | HSN/SAC written as `@HSN`; print aliases `HSN` | HSN/SAC/tax | No | Yes (`HSN Code`) | Not on join | No (line) |
| `Product_name` | Item name | item/product | No | Yes (`Item Name`) | Not on join | No (line) |
| `Quantity` | Qty | quantities/rates | No | Yes (`Qty`) | Not on join | No (line) |
| `sail_rate` | Unit rate | quantities/rates | No | Yes (`Rate`) | Not on join | No (line) |
| `Total_sail_rate2` | Taxable base; export `ISNULL(..., qty * rate)` | quantities/rates | No | Yes (`Taxable Value`) | Not on join | No (line) |
| `Service_tax_rate` | Line GST % | HSN/SAC/tax | No | Yes (`GST %`) | Not on join | No (line) |
| `Total_sail_rate1` | Line net (taxable + tax) | quantities/rates | No | Yes (`Item Net Value`) | Not on join | No (line) |
| `discountRate` | Line discount %; create keeps it; print selects it | quantities/rates | No | No | Not on join | No (line) |
| `specification` | Brand/spec; print concatenates into product name | item/product | No | No | Not on join | No (line) |
| `ItemNo` | Line identity for pending-qty matching | item/product | No | No | Not on join | No (line) |
| `AddedById` | Line creator | audit | No | No | Not on join | Would often match header |
| `CompanyID` | Written at create | reference | No | **Join predicate only; not a workbook column** | **Export join `d.CompanyID = @CompanyID`** | Isolation key |
| `Sl_no` | Print orders by `CAST(Sl_no as int)`; create INSERT list does not include it | reference | No | No | Not on join | No (line) |

### 3.3 `tbl_Client` (alias `b`) — customer

Search dropdown: `SELECT Client_Name FROM tbl_Client WHERE CompanyID = @CompanyID`.  
Export join: `b.Client_Id = a.Client_ID AND b.CompanyID = @CompanyID`.  
Grid BindData join: `b.Client_Id = a.Client_ID` only (unchanged).

| Column | Apparent meaning from usage | Class | UI | Export | CompanyID-scoped | Duplicate on line grain |
|---|---|---|---|---|---|---|
| `Client_Id` | Customer key | customer | Join / create lookup | Join only | Dropdown yes; list join no | Would duplicate |
| `Client_Name` | Display name; Search equality / View `LIKE` | customer | Yes | Yes | Dropdown yes; **export join yes**; grid join no | Yes |
| `CompanyID` | Tenant on master (`New_client` insert) | reference | Dropdown filter only | **Export join predicate only** | **Export join yes; grid join no** | — |
| `Address1`, `City`, `pin`, `State` | Address; print + billing lookup on create | customer | No (list) | No | Print join has no CompanyID | Would duplicate |
| `Address2` | Print only | customer | No | No | Not in these flows | Would duplicate |
| `Service_tax_no` / `Pan_no` / `PlaceofSupply` | GSTIN / PAN / POS on print | tax / customer | No | No | Not in these flows | Would duplicate |
| `Vat_no` | Print select | tax | No | No | Not in these flows | Would duplicate |
| `Com_email` / `Com_phone` | Directory; not list/export | customer | No | No | Master has CompanyID | Would duplicate |

### 3.4 `tbl_Quotation` (alias `q`) — quotation / PO header

Search/View **grid only** (not export): `LEFT JOIN ... ON q.Quotation_no = a.Quotation_No` with **no** `q.CompanyID`. Create writes `CompanyID` on insert.

| Column | Apparent meaning from usage | Class | UI | Export | CompanyID-scoped | Duplicate on line grain |
|---|---|---|---|---|---|---|
| `Quotation_no` | Join key to invoice `Quotation_No` | quotation / reference | Via invoice column | No | Join unscoped | Would duplicate |
| `PO_Number` | UI badge `ARC` | PO/DO/reference | Yes | No | Join unscoped | Would duplicate |
| `DO_Number` | UI badge `PO/DO` | PO/DO/reference | Yes | No | Join unscoped | Would duplicate |
| `Validity_StartDate` / `Validity_EndDate` | Validity window | quotation | Yes (`FmtDate`) | No | Join unscoped | Would duplicate |
| `PO_Date` | Print PO date | PO/DO/reference | No (list) | No | Not in list SQL | Would duplicate |
| `PaymentStatus` | Create default `'No'`; quotation commercial | payment | No | No | Not in list SQL | Would duplicate |
| `DeliveryTenure` | Create delivery terms (not a dispatch-mode field) | logistics | No | No | Not in list SQL | Would duplicate |
| `PlaceofSupply` | Quotation POS; print may use it | tax | No | No | Not in list SQL | Would duplicate |
| `CompanyID` | Written at quotation create | reference | No | No | **Not on Search/View join** | — |

No `Dispatch Mode` column appears in Search/View invoice SQL or in the quotation insert list above.

### 3.5 `tbl_QuoPriSerTogather` (alias `c`) — primary service on quotation

Search/View **grid only**. Insert: `(qutno, PServiceName, TimeStamp, CompanyID)` on current quotation create; older edit path omits `CompanyID`.

| Column | Apparent meaning from usage | Class | UI | Export | CompanyID-scoped | Duplicate on line grain |
|---|---|---|---|---|---|---|
| `qutno` | Quotation number | quotation / reference | Join | No | Join unscoped | — |
| `PServiceName` | Primary service caption under quotation | quotation | Yes | No | Join unscoped | Would duplicate **and can fan-out grid rows** |
| `CompanyID` | Written on current create | reference | No | No | **Not on Search/View join** | — |

### 3.6 `tbl_login` (alias `l`) — user

Search/View **grid only**: `LEFT JOIN ... ON l.User_Id = a.AddedById` with **no** `l.CompanyID`. Export uses `a.AddedById`, not `l.Name`.

| Column | Apparent meaning from usage | Class | UI | Export | CompanyID-scoped | Duplicate on line grain |
|---|---|---|---|---|---|---|
| `User_Id` | Join to `AddedById` | audit | Join | No | Join unscoped | — |
| `Name` | Display creator; `ISNULL(l.Name, a.AddedById) AS AddedByName` | audit | Yes | No (export is id) | Join unscoped | Would duplicate |
| `CompanyID` | Written at user create | reference | No | No | **Not on Search/View join** | — |

### 3.7 `tbl_SystemNotification` — audit log (write-only)

Used by these pages for notifications, not as export source.

| Column | Apparent meaning from usage | Class | UI | Export | CompanyID-scoped | Duplicate on line grain |
|---|---|---|---|---|---|---|
| `Title`, `Message`, `ModuleType`, `AlertLevel`, `CreatedBy`, `CreatedDate`, `IsActive` | Search logs on search; View logs on export | audit | No | No | **Insert has no CompanyID** | N/A |

---

## 4. Finance / operations fields — usage only

Facts from current code. Not a column-selection decision.

| Theme | What the app already uses | In current line-item export? | Safety / meaning notes |
|---|---|---|---|
| Invoice identifiers | `Invoice_No`, `ID` (print), `ExtInvoiceNo` | Number and ERP yes; `ID` no | `ID` is print navigation, not a finance document number |
| Quotation / reference | Header `Quotation_No` / `Quotation_Date`; line `Quotation_no` | Date yes; numbers no | Manual invoices can store PO in `Quotation_No`. Line `Quotation_no` is a **new join-free column on details** if ever added, but it is not in the export SELECT today |
| Customer identifiers | `Client_Name`; `Client_ID` only as join | Name yes; id no | Export client join is CompanyID-scoped; grid BindData join is not |
| Item / product | `Product_id`, `Product_name`, `ItemNo`, `specification` | Code and name yes | `ItemNo` / spec used at create and print, not list/export |
| HSN / SAC / tax | Line `Product_Code`, `Service_tax_rate`; header GST flags and `Service_Tax1` | HSN, GST %, Tax Type yes; header GST amount no | Grid taxable is computed `Net_Amount - Service_Tax1`, not stored `sub_total`. Print uses `Service_Tax` |
| Qty / rate / taxable | Line qty, rate, `Total_sail_rate2` / `Total_sail_rate1` | Yes | Keep existing `ISNULL` taxable fallback; do not change math |
| GST components | Flags `cgstOrsgst`/`igst`; no CGST/SGST split amounts in these queries | Type only | No component amount columns in Search/View SQL |
| Freight / other | `Delivery_Amount`, `otherAmount1`, `otherAmount1_name` | Amounts yes; name no | Header values repeat per line |
| PO / DO / reference | Grid `PO_Number`, `DO_Number`, validity dates via quotation join | No | Adding them needs the **existing grid join** (already present on BindData, not on export). That join is not CompanyID-scoped |
| Creation / audit | `AddedById`, login `Name`, `TimeStamp`, `mailDate` | Id + timestamp + mail date yes; display name no | Name needs the login join already used on the grid |
| Status / state | Invoice `status2` Active/Block at create/pending-qty; `status1` `'No'` | No | Search/View do not filter blocked invoices. Values are not Cancelled/Pending/Credit |
| Payment | Quotation `PaymentStatus`; payments live on `tbl_invoice_payment` | No | Not queried by Search/View |
| Dispatch | No dispatch-mode column in these flows; `DeliveryTenure` is quotation delivery terms | No | Do not invent a dispatch column |

---

## 5. Isolation and grain

- Preserve Search filters, View filters, `WHERE 1=1` on View, `LIKE @Client` on View, current-month default, ClosedXML, filenames, sheet name, and notification timing.
- Export tenant predicates (this iteration): `a.CompanyID`, `b.CompanyID`, and `d.CompanyID` all equal `@CompanyID`. `CompanyID` is not selected into the workbook.
- Grid BindData joins for client / quotation / service / login remain unscoped (not part of this export correction).
- Do not add a quotation join to export until a later iteration can constrain `q.CompanyID`.
- Header money fields on a line-item sheet already repeat (`Invoice Grand Total`, `Freight`, `Other Charges`). Further header metadata increases that repetition.

---

## 6. Proposed Export Mapping — Finance / Operations Review

Workbook grain stays **one row per invoice line**. Header-level fields already exported (`Invoice Grand Total`, `Freight`, `Other Charges`, dates, tax type, created-by) repeat on every line. Do not add large amounts of header metadata unless the reporting need outweighs that duplication.

`INCLUDE` / `OPTIONAL` / `EXCLUDE` below are **proposals only**. The live export SELECT list is unchanged in this iteration.

### Header-level candidates

| Source table | Source column | Proposed Excel label | Grain | Why useful | Duplicates per line | CompanyID safely constrained | Recommendation |
|---|---|---|---|---|---|---|---|
| `tbl_Invoice` | `Quotation_No` | Source Reference | Header | Ties the invoice to quotation / PO / `VERBAL`; already on the UI | Yes | Yes — column is on header `a` already filtered by `a.CompanyID` | **INCLUDE** |
| `tbl_Quotation` | `PO_Number` | ARC / PO Number | Header | Shown as ARC on the grid; operations matching | Yes | **Not on export today.** Needs a new `tbl_Quotation` join with `q.CompanyID = @CompanyID` | **OPTIONAL** (blocked until a tenant-safe quotation join) |
| `tbl_Quotation` | `DO_Number` | DO Number | Header | Shown as PO/DO on the grid | Yes | Same as `PO_Number` | **OPTIONAL** (blocked until a tenant-safe quotation join) |
| `tbl_Invoice` | `ExtInvoiceDate` | ERP Date | Header | Pairs with existing `ERP Ref` | Yes | Yes — on `a` | **OPTIONAL** |
| `tbl_Invoice` | `otherAmount1_name` | Other Charges Name | Header | Labels the existing Other Charges amount | Yes | Yes — on `a` | **OPTIONAL** |
| `tbl_Invoice` | `discount` | Invoice Discount | Header | UI already shows it when &gt; 0; explains net vs gross | Yes | Yes — on `a` | **OPTIONAL** |
| `tbl_Invoice` | `Gross` | Invoice Gross | Header | Header commercial total; Grand Total already exported | Yes | Yes — on `a` | **OPTIONAL** |
| `tbl_Invoice` | `Service_Tax1` | Invoice GST Amount | Header | Grid GST amount; no GST component split exists in these queries | Yes | Yes — on `a` | **INCLUDE** |
| `tbl_Invoice` | `SalesPersonCode` | Sales Person | Header | Written at create; ownership / commission | Yes | Yes — on `a` | **OPTIONAL** |
| `tbl_Invoice` | `BillingAddress` | Billing Address | Header | Long text; print/create use it | Yes (wide) | Yes — on `a` | **EXCLUDE** unless finance/operations explicitly need it |
| `tbl_Invoice` | `CompanyID` | — | Header | Tenant key | — | Isolation predicate only | **EXCLUDE** |
| `tbl_Invoice` | `status1` | — | Header | Create inserts `'No'`; not used as invoice workflow status on these pages | Yes | On `a` | **EXCLUDE** |
| `tbl_Invoice` | `status2` | — | Header | Active/Block at create; list does not filter it; values are not Cancelled/Pending/Credit | Yes | On `a` | **EXCLUDE** |
| `tbl_Invoice` | `mailStatus` | — | Header | Mail module only; `mailDate` already exported | Yes | On `a` | **EXCLUDE** |

### Line-level candidates

| Source table | Source column | Proposed Excel label | Grain | Why useful | Duplicates per line | CompanyID safely constrained | Recommendation |
|---|---|---|---|---|---|---|---|
| `tbl_Invoice_details` | `Quotation_no` | Line Source Ref | Line | Source document on the line (`Add_invoice` `@RefNo`); may differ from header `Quotation_No` | No | Yes after export join `d.CompanyID` | **OPTIONAL** (only if it differs from header Source Reference in real data) |
| `tbl_Invoice_details` | `discountRate` | Line Discount % | Line | Create/print already store/read it; explains line taxable | No | Yes — `d.CompanyID` | **INCLUDE** |
| `tbl_Invoice_details` | `specification` | Specification | Line | Print concatenates into product name | No | Yes — `d.CompanyID` | **OPTIONAL** |
| `tbl_Invoice_details` | `ItemNo` | Item No | Line | Pending-qty matching key at create | No | Yes — `d.CompanyID` | **OPTIONAL** |

### Explicit excludes (not invoice-export flow, or duplicate master data)

| Source table | Source column | Proposed Excel label | Grain | Why useful | Duplicates per line | CompanyID safely constrained | Recommendation |
|---|---|---|---|---|---|---|---|
| `tbl_invoice_payment` / `tbl_Quotation` | payment / `PaymentStatus` | — | Header | Payment lives on a different module | Yes | Not in current export flow | **EXCLUDE** |
| `tbl_Quotation` | `DeliveryTenure` | — | Header | Delivery terms, not dispatch mode; no dispatch column in these flows | Yes | Would need quotation join | **EXCLUDE** |
| `tbl_Client` | address / GSTIN / PAN / phone / email | — | Header | Customer master; `Client Name` already exported | Yes | Export client join is scoped; still master duplication | **EXCLUDE** unless an explicit reporting requirement |

### Candidate list summary

**INCLUDE (next implementation, no new tables):**

- Header: `tbl_Invoice.Quotation_No` → Source Reference
- Header: `tbl_Invoice.Service_Tax1` → Invoice GST Amount
- Line: `tbl_Invoice_details.discountRate` → Line Discount %

**OPTIONAL (only if review confirms the reporting need):**

- Header: `ExtInvoiceDate`, `otherAmount1_name`, `discount`, `Gross`, `SalesPersonCode`
- Header: `PO_Number`, `DO_Number` — only after a tenant-safe `tbl_Quotation` join (`q.CompanyID = @CompanyID`)
- Line: `Quotation_no`, `specification`, `ItemNo`

**EXCLUDE:**

- `CompanyID` (any table) as a workbook column
- `status1`, `status2`, `mailStatus`
- `BillingAddress` unless explicitly requested
- Payment / dispatch fields outside the current invoice export flow
- Duplicated customer master fields (address, GSTIN, PAN, contact)

---

## 7. Stop point

Do not change the exported column list until finance/operations accept the INCLUDE set. Tenant-safe export join predicates may ship independently of column enrichment.

