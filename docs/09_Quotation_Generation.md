# Quotation generation

Canonical pages, tables, and print: [`page-catalog/DOMAIN_quotation.md`](page-catalog/DOMAIN_quotation.md).  
Client PO (same header table, different `RecordType`): [`docs/10_Purchase_Order.md`](10_Purchase_Order.md).  
Commercial spine: [`SOLUTION_INDEX.md`](SOLUTION_INDEX.md).  
Hydrant quotes: [`page-catalog/DOMAIN_hydrant.md`](page-catalog/DOMAIN_hydrant.md) (hidden menu; separate tables).

This file keeps **leftover facts** that are not in the catalog.

---

## Confirmed tables (not “unknown”)

| Table | Role |
|-------|------|
| `tbl_Quotation` | Header. `RecordType` = `Quotation` or `Purchase Order` (same create page, second menu `Li2`). |
| `tbl_Quotaion_details` | Lines (table name misspelled). |
| `tbl_Client` | Customer FK. |
| `tbl_SalesVisitReport` | Optional `visitId` prefill of `CustomerName`. |

Print: `NewQuotation.aspx?ID=` (current) vs legacy `Quotation.aspx?ID=` when date ≤ 12-Jun-2018 on search.

`Set_quatation.aspx` menu says “Set Quotation Permission” but stamps `mailStatus` — mail send is commented out.

---

## Visit leftover (D-01)

`Create_quotation` loads `CustomerName` with `WHERE Id=@Id AND CompanyID=@CompanyID`. Visits inserted without `CompanyID` return no row, so visit → quote prefill fails. That is a visit-insert defect, not a missing quotation table.

Visit reports do **not** insert `tbl_Quotation` rows. A site visit is not a quotation.
