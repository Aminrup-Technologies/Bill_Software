# Purchase orders — two stacks

There are **two unrelated PO implementations**. Do not merge them. A third thing named “purchase” is vendor goods received (`tbl_Purches`).

Canonical vendor PR/PO pages: [`page-catalog/DOMAIN_pr-po.md`](page-catalog/DOMAIN_pr-po.md).  
Canonical quotation / client-PO pages: [`page-catalog/DOMAIN_quotation.md`](page-catalog/DOMAIN_quotation.md).  
Print pages: [`page-catalog/DOMAIN_print.md`](page-catalog/DOMAIN_print.md).  
SPs / print helper: [`page-catalog/SHARED_RUNTIME.md`](page-catalog/SHARED_RUNTIME.md).

---

## Stack A — vendor PO (procurement)

| Item | Fact |
|------|------|
| Tables | `tbl_PO_Header` (PK `PO_Id`, unique `PO_No`), `tbl_PO_Items`, `tbl_PO_PartySnapshot` (not `tbl_PO_Details` / `tbl_PO_Charges`) |
| Upstream | `tbl_RequisitionMain` (`VendorId` → `tbl_Vendor.Id`) / `tbl_RequisitionNew` (`ProductId` → `tbl_NewProduct.ProductID`) via `sp_GeneratePO_FromReqNo` |
| Live pages | `RequisitionNew` → `View_PR` / `View_PR_Details` → `Approve_PR` → `Generate_PO_From_PR` → `Generate_PO_Preview?reqNo=` → `View_PO` / `View_PO_Details?poId=` |
| Print | `Print_PO.aspx?poId=` — `EnsurePrint` on `tbl_PO_Header.PO_Id`; payload `sp_GetReleasedPO_Details` |
| Also | Three PR implementations (modern SP path, hidden Manual*, leftover bank `tbl_requisition`) — details in the catalog |

There is no live `PurchaseOrder.aspx` in this tree.

UAT also has **amendment/cancellation** tables and SPs (`tbl_PO_Amendment_*`, `tbl_PO_Cancellation_Request`, `sp_CreatePO_Amendment`, …) with **0 rows** and **no C# callers**. Status after generate-PO includes **`PO_Created`** on the PR. Schema: [`SHARED_SCHEMA.md`](page-catalog/SHARED_SCHEMA.md). Legacy `tbl_requisition` / `tbl_requisitionBankDetails` are **not** on UAT.

---

## Stack B — client PO (sales document)

| Item | Fact |
|------|------|
| Tables | **`tbl_Quotation` + `tbl_Quotaion_details`** with `RecordType` = `Purchase Order` (list pages also use `RecordType != 'Quotation'`) |
| Create | **Same page as quotation:** `Create_quotation.aspx` (menu id `Li2`) |
| List / edit / delete | `View_PurchaseOrder`, `Search_purchaseorder`, `edit_purchaseorder`, `delete_purchaseorder` |
| Print | `NewPurchaseOrder.aspx?ID=` and `NewPurchaseOrder_Print.aspx?ID=` — id is **quotation header id**. `PurchaseOrderPrintHelper` binds this stack, not vendor `tbl_PO_Header`. |

---

## Not this module

- Vendor **purchase** (`tbl_Purches`) = goods received / bill from vendor — [`DOMAIN_vendor-purchase.md`](page-catalog/DOMAIN_vendor-purchase.md).
- Client **tax invoice** = `tbl_Invoice` — [`DOMAIN_invoice.md`](page-catalog/DOMAIN_invoice.md).
