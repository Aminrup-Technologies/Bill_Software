# Stock & due reports

> Domain key: `reports` · 5 page(s)

Product/service stock and due summaries.

**Do not copy** AuthN/AuthZ, tenancy, or Ponytail rules here. Those live in [SHARED_CONTEXT](SHARED_CONTEXT.md).

| Page | Purpose | Master / class | Gate | Tables (unique) | Ops | Narrative |
|------|---------|----------------|------|-----------------|-----|-----------|
| `corporate/business/app/Payment_due.aspx` | Payments Due | Bill.Master / `WebForm52` | Bill.Master, CompanyID<br>menu `Payment_due` | `tbl_Client`, `tbl_Invoice`, `tbl_invoice_due`, `tbl_invoice_payment` | SELECT | this catalog |
| `corporate/business/app/Product_stock.aspx` | Product Stock | Bill.Master / `WebForm50` | Bill.Master<br>menu `product_stock` | `sp_SearchProductsFast()`, `sp_GetProductStockByStore()`, `sp_GetProductCategories()` | — | this catalog |
| `corporate/business/app/Purchess_due.aspx` | Purchesse Due | Bill.Master / `WebForm53` | Bill.Master<br>menu `Purchess_due` | `tbl_Purches`, `tbl_Vendor`, `tbl_purches_due`, `tbl_login` | SELECT | this catalog |
| `corporate/business/app/Service_stock.aspx` | Service Stock | Bill.Master / `WebForm51` | Bill.Master<br>menu `Service_stock` | `tbl_stock` | SELECT | this catalog |
| `corporate/business/app/rpts_vw_qtn_po_counts.aspx` | QTN/ PO Added | Bill.Master / `rpts_vw_qtn_po_counts` | Bill.Master<br>menu `rpts_vw_qtn_po_counts` | — | — | this catalog |

## Page-unique notes

- `corporate/business/app/rpts_vw_qtn_po_counts.aspx`: Stub: empty `Page_Load`, no SQL. Menu still lists it as QTN/PO Added.
