# Page inventory

Total `.aspx` pages: **188**. Census status is Reviewed (every page has a catalog row). Domain workflows live after `<!-- NARRATIVE:BEGIN -->` in each `DOMAIN_*.md`. See [SHARED_CONTEXT.md](SHARED_CONTEXT.md) for AuthN/tenancy, [SHARED_RUNTIME.md](SHARED_RUNTIME.md) for handlers/helpers/SPs, [SHARED_SCHEMA.md](SHARED_SCHEMA.md) for live UAT objects, and [DATA_DICTIONARY.md](DATA_DICTIONARY.md) for page↔table/SP verbs.

Shared architecture is not recorded here. See [SHARED_CONTEXT.md](SHARED_CONTEXT.md).

| # | Page | Domain | Menu | Gate | Status |
|---|------|--------|------|------|--------|
| 1 | `Print/id_card1.aspx` | `card-kiosk` | `—` | no-page-gate | Reviewed |
| 2 | `SessionKeepAlive.aspx` | `auth` | `—` | Session | Reviewed |
| 3 | `SessionKeepAlive1.aspx` | `card-kiosk` | `—` | Session | Reviewed |
| 4 | `admin/Delete_date.aspx` | `card-kiosk` | `—` | card.Master | Reviewed |
| 5 | `admin/Setting.aspx` | `card-kiosk` | `—` | card.Master | Reviewed |
| 6 | `admin/Show_data1.aspx` | `card-kiosk` | `—` | card.Master, CompanyID | Reviewed |
| 7 | `admin/Total_id_card.aspx` | `card-kiosk` | `—` | no-page-gate, CompanyID | Reviewed |
| 8 | `admin/Update/contactno.aspx` | `card-kiosk` | `—` | Session | Reviewed |
| 9 | `admin/Update/emailid.aspx` | `card-kiosk` | `—` | Session | Reviewed |
| 10 | `admin/Update/name.aspx` | `card-kiosk` | `—` | Session | Reviewed |
| 11 | `admin/Update/password.aspx` | `card-kiosk` | `—` | Session | Reviewed |
| 12 | `admin/Update_Company.aspx` | `card-kiosk` | `—` | card.Master | Reviewed |
| 13 | `admin/Upload_data.aspx` | `card-kiosk` | `—` | card.Master | Reviewed |
| 14 | `admin/add_company.aspx` | `card-kiosk` | `—` | card.Master | Reviewed |
| 15 | `admin/home.aspx` | `card-kiosk` | `home1` | card.Master | Reviewed |
| 16 | `admin/show_date.aspx` | `card-kiosk` | `—` | card.Master, CompanyID | Reviewed |
| 17 | `admin/update_image.aspx` | `card-kiosk` | `—` | card.Master, CompanyID | Reviewed |
| 18 | `corporate/business/app/AddFactory.aspx` | `customer` | `AddFactory` | SecurePage, CompanyID | Reviewed |
| 19 | `corporate/business/app/AddIndustry.aspx` | `masters` | `AddIndustry` | Bill.Master | Reviewed |
| 20 | `corporate/business/app/AddPrimaryService.aspx` | `masters` | `AddPrimaryService` | Bill.Master | Reviewed |
| 21 | `corporate/business/app/AddUser.aspx` | `users-rbac` | `AddUser` | SecurePage, CompanyID | Reviewed |
| 22 | `corporate/business/app/Add_invoice.aspx` | `invoice` | `Add_invoice` | Bill.Master, CompanyID, WebMethod | Reviewed |
| 23 | `corporate/business/app/Add_proforma.aspx` | `proforma` | `Add_proforma` | Bill.Master, CompanyID | Reviewed |
| 24 | `corporate/business/app/AdminApprovalDashboard.aspx` | `attendance-hr` | `AdminApprovalDashboard` | SecurePage, CompanyID | Reviewed |
| 25 | `corporate/business/app/AdminAttendanceDashboard.aspx` | `attendance-hr` | `—` | Bill.Master, CompanyID | Reviewed |
| 26 | `corporate/business/app/AdminLeaveSetup.aspx` | `attendance-hr` | `AdminLeaveSetup` | SecurePage | Reviewed |
| 27 | `corporate/business/app/AdminOverride.aspx` | `attendance-hr` | `AdminOverride` | SecurePage, CompanyID | Reviewed |
| 28 | `corporate/business/app/AdminShiftAssignment.aspx` | `attendance-hr` | `—` | SecurePage, CompanyID | Reviewed |
| 29 | `corporate/business/app/AdminShiftSetup.aspx` | `attendance-hr` | `AdminShiftSetup` | SecurePage, CompanyID | Reviewed |
| 30 | `corporate/business/app/Approve_PR.aspx` | `pr-po` | `pr_approve` | Bill.Master, CompanyID | Reviewed |
| 31 | `corporate/business/app/Block_invoice.aspx` | `invoice` | `Block_invoice` | Bill.Master, CompanyID | Reviewed |
| 32 | `corporate/business/app/CreateHydrentQuatation.aspx` | `quotation` | `CreateHydrentQuatation` | Bill.Master | Reviewed |
| 33 | `corporate/business/app/Create_quotation.aspx` | `quotation` | `Create_quotation` | Bill.Master, CompanyID | Reviewed |
| 34 | `corporate/business/app/DeleteHydrentQuatation.aspx` | `quotation` | `DeleteHydrentQuatation` | Bill.Master | Reviewed |
| 35 | `corporate/business/app/Delete_Quotation.aspx` | `quotation` | `Delete_Quotation` | Bill.Master, CompanyID | Reviewed |
| 36 | `corporate/business/app/Delete_chalan.aspx` | `dpcc-challan` | `Delete_chalan` | Bill.Master, CompanyID | Reviewed |
| 37 | `corporate/business/app/Delete_client.aspx` | `customer` | `Delete_client` | SecurePage, CompanyID | Reviewed |
| 38 | `corporate/business/app/Delete_general_expencess.aspx` | `expenses-made` | `Delete_general_expencess` | SecurePage, CompanyID | Reviewed |
| 39 | `corporate/business/app/Delete_invoice.aspx` | `invoice` | `Delete_invoice` | Bill.Master, CompanyID | Reviewed |
| 40 | `corporate/business/app/Delete_patty_cash_expenses.aspx` | `expenses-made` | `Delete_patty_cash_expenses` | SecurePage, CompanyID | Reviewed |
| 41 | `corporate/business/app/Delete_payment.aspx` | `payments` | `Delete_payment` | Bill.Master, CompanyID | Reviewed |
| 42 | `corporate/business/app/Delete_proforma.aspx` | `proforma` | `Delete_proforma` | Bill.Master, CompanyID | Reviewed |
| 43 | `corporate/business/app/Delete_purches_payment.aspx` | `payments` | `Delete_purches_payment` | Bill.Master, CompanyID | Reviewed |
| 44 | `corporate/business/app/Delete_purtches.aspx` | `vendor-purchase` | `Delete_purtches` | Bill.Master, CompanyID | Reviewed |
| 45 | `corporate/business/app/Delete_vendor.aspx` | `vendor-purchase` | `Delete_vendor` | SecurePage, CompanyID | Reviewed |
| 46 | `corporate/business/app/Deletehydrent_invoice.aspx` | `hydrant` | `Deletehydrent_invoice` | Bill.Master | Reviewed |
| 47 | `corporate/business/app/Direct_Proforma.aspx` | `proforma` | `Manual_proforma` | Bill.Master, CompanyID | Reviewed |
| 48 | `corporate/business/app/EditPurchase.aspx` | `vendor-purchase` | `Edit_Purches` | Bill.Master, CompanyID | Reviewed |
| 49 | `corporate/business/app/Edit_quatation.aspx` | `quotation` | `—` | Bill.Master, CompanyID | Reviewed |
| 50 | `corporate/business/app/Edit_quatation_v2.aspx` | `quotation` | `Edit_quatation` | Bill.Master, CompanyID | Reviewed |
| 51 | `corporate/business/app/Expenses_Head.aspx` | `expenses-made` | `Expenses_Head` | SecurePage | Reviewed |
| 52 | `corporate/business/app/FinalPaymentInvoice.aspx` | `payments` | `FinalPaymentInvoice` | Bill.Master, CompanyID | Reviewed |
| 53 | `corporate/business/app/Generate_PO_From_PR.aspx` | `pr-po` | `po_generate` | Bill.Master, CompanyID | Reviewed |
| 54 | `corporate/business/app/Generate_PO_Preview.aspx` | `pr-po` | `—` | Bill.Master, CompanyID | Reviewed |
| 55 | `corporate/business/app/HydrantProduct.aspx` | `hydrant` | `—` | Bill.Master | Reviewed |
| 56 | `corporate/business/app/ImportProducts.aspx` | `masters` | `uploader` | SecurePage | Reviewed |
| 57 | `corporate/business/app/InvoiceMail.aspx` | `invoice` | `InvoiceMail` | Bill.Master, CompanyID | Reviewed |
| 58 | `corporate/business/app/ManagePermissions.aspx` | `users-rbac` | `ManagePermissions` | SecurePage | Reviewed |
| 59 | `corporate/business/app/ManageRoles.aspx` | `users-rbac` | `ManageRoles` | SecurePage, CompanyID | Reviewed |
| 60 | `corporate/business/app/Manual_Invoice.aspx` | `invoice` | `Add_DirectInvoice` | Bill.Master, CompanyID | Reviewed |
| 61 | `corporate/business/app/MyLeaves.aspx` | `attendance-hr` | `my_leaves` | Bill.Master, CompanyID | Reviewed |
| 62 | `corporate/business/app/NewUpdate_product.aspx` | `masters` | `—` | SecurePage | Reviewed |
| 63 | `corporate/business/app/New_client.aspx` | `customer` | `New_client` | SecurePage, CompanyID, WebMethod | Reviewed |
| 64 | `corporate/business/app/New_vendor.aspx` | `vendor-purchase` | `New_vendor` | SecurePage, CompanyID | Reviewed |
| 65 | `corporate/business/app/PaymentMail.aspx` | `payments` | `PaymentMail` | Bill.Master, CompanyID | Reviewed |
| 66 | `corporate/business/app/PaymentPhase.aspx` | `masters` | `PaymentPhase` | Bill.Master | Reviewed |
| 67 | `corporate/business/app/Payment_due.aspx` | `reports` | `Payment_due` | Bill.Master, CompanyID | Reviewed |
| 68 | `corporate/business/app/PaymentsDue.aspx` | `payments` | `PaymentsDue` | Bill.Master, CompanyID | Reviewed |
| 69 | `corporate/business/app/PaymentsReceived.aspx` | `payments` | `—` | Bill.Master, CompanyID | Reviewed |
| 70 | `corporate/business/app/PrimaryServiceTerms.aspx` | `masters` | `PrimaryServiceTerms` | Bill.Master | Reviewed |
| 71 | `corporate/business/app/Product_stock.aspx` | `reports` | `product_stock` | Bill.Master | Reviewed |
| 72 | `corporate/business/app/ProformaMail.aspx` | `proforma` | `ProformaMail` | Bill.Master, CompanyID | Reviewed |
| 73 | `corporate/business/app/Purches_exting_vendor.aspx` | `vendor-purchase` | `Purches_exting_vendor` | Bill.Master | Reviewed |
| 74 | `corporate/business/app/Purches_new_vendor.aspx` | `vendor-purchase` | `—` | Bill.Master, CompanyID | Reviewed |
| 75 | `corporate/business/app/Purchess_due.aspx` | `reports` | `Purchess_due` | Bill.Master | Reviewed |
| 76 | `corporate/business/app/QuickAction.aspx` | `dashboard` | `—` | token-link, CompanyID | Reviewed |
| 77 | `corporate/business/app/Representative.aspx` | `customer` | `Representative` | SecurePage, CompanyID | Reviewed |
| 78 | `corporate/business/app/RequisitionCreate.aspx` | `pr-po` | `—` | Bill.Master, CompanyID | Reviewed |
| 79 | `corporate/business/app/RequisitionManual.aspx` | `pr-po` | `RequisitionManual` | Bill.Master | Reviewed |
| 80 | `corporate/business/app/RequisitionManualDelete.aspx` | `pr-po` | `RequisitionManualDelete` | Bill.Master, CompanyID | Reviewed |
| 81 | `corporate/business/app/RequisitionManualSearch.aspx` | `pr-po` | `RequisitionManualSearch` | Bill.Master, CompanyID | Reviewed |
| 82 | `corporate/business/app/RequisitionManualView.aspx` | `pr-po` | `RequisitionManualView` | Bill.Master | Reviewed |
| 83 | `corporate/business/app/RequisitionNew.aspx` | `pr-po` | `pr_create` | Bill.Master, CompanyID, WebMethod | Reviewed |
| 84 | `corporate/business/app/RequisitionView.aspx` | `pr-po` | `—` | Bill.Master | Reviewed |
| 85 | `corporate/business/app/SearchHydrentQuatation.aspx` | `quotation` | `SearchHydrentQuatation` | Bill.Master, CompanyID | Reviewed |
| 86 | `corporate/business/app/Search_purchaseorder.aspx` | `pr-po` | `Li3` | Bill.Master, CompanyID | Reviewed |
| 87 | `corporate/business/app/Searchhydrent_invoice.aspx` | `hydrant` | `Searchhydrent_invoice` | Bill.Master | Reviewed |
| 88 | `corporate/business/app/Seartch_proforma.aspx` | `proforma` | `Seartch_proforma` | Bill.Master, CompanyID | Reviewed |
| 89 | `corporate/business/app/Seartch_purchess_payments.aspx` | `payments` | `Seartch_purchess_payments` | Bill.Master, CompanyID | Reviewed |
| 90 | `corporate/business/app/Seartch_quotation.aspx` | `quotation` | `Seartch_quotation` | Bill.Master, CompanyID | Reviewed |
| 91 | `corporate/business/app/Service_Tax_Master.aspx` | `masters` | `Service_Tax_Master` | Bill.Master | Reviewed |
| 92 | `corporate/business/app/Service_master.aspx` | `masters` | `Service_master` | SecurePage | Reviewed |
| 93 | `corporate/business/app/Service_stock.aspx` | `reports` | `Service_stock` | Bill.Master | Reviewed |
| 94 | `corporate/business/app/Service_update.aspx` | `masters` | `—` | Bill.Master | Reviewed |
| 95 | `corporate/business/app/Set_quatation.aspx` | `quotation` | `SetQuatation` | Bill.Master, CompanyID | Reviewed |
| 96 | `corporate/business/app/ShowFactory.aspx` | `customer` | `—` | SecurePage | Reviewed |
| 97 | `corporate/business/app/Show_representative.aspx` | `customer` | `—` | Bill.Master | Reviewed |
| 98 | `corporate/business/app/Update/contactno.aspx` | `users-rbac` | `—` | Session | Reviewed |
| 99 | `corporate/business/app/Update/emailid.aspx` | `users-rbac` | `—` | Session | Reviewed |
| 100 | `corporate/business/app/Update/name.aspx` | `users-rbac` | `—` | Session | Reviewed |
| 101 | `corporate/business/app/Update/password.aspx` | `users-rbac` | `—` | Session | Reviewed |
| 102 | `corporate/business/app/Update_Designation.aspx` | `users-rbac` | `—` | SecurePage, CompanyID | Reviewed |
| 103 | `corporate/business/app/Update_client.aspx` | `customer` | `—` | Bill.Master, CompanyID | Reviewed |
| 104 | `corporate/business/app/Update_factory.aspx` | `customer` | `—` | Bill.Master | Reviewed |
| 105 | `corporate/business/app/Update_product.aspx` | `masters` | `—` | SecurePage | Reviewed |
| 106 | `corporate/business/app/Update_vendor.aspx` | `vendor-purchase` | `—` | Bill.Master, CompanyID | Reviewed |
| 107 | `corporate/business/app/Vat_master.aspx` | `masters` | `Vat_master` | Bill.Master | Reviewed |
| 108 | `corporate/business/app/Vendor_quotation.aspx` | `quotation` | `—` | Bill.Master, CompanyID | Reviewed |
| 109 | `corporate/business/app/ViewHydrentQuatation.aspx` | `quotation` | `ViewHydrentQuatation` | Bill.Master | Reviewed |
| 110 | `corporate/business/app/ViewUser.aspx` | `users-rbac` | `ViewUser` | SecurePage, CompanyID, WebMethod | Reviewed |
| 111 | `corporate/business/app/View_Invoice.aspx` | `invoice` | `View_Invoice` | Bill.Master, CompanyID | Reviewed |
| 112 | `corporate/business/app/View_PO.aspx` | `pr-po` | `po_view` | Bill.Master, CompanyID | Reviewed |
| 113 | `corporate/business/app/View_PO_Details.aspx` | `pr-po` | `—` | Bill.Master, CompanyID | Reviewed |
| 114 | `corporate/business/app/View_PR.aspx` | `pr-po` | `pr_view` | Bill.Master, CompanyID | Reviewed |
| 115 | `corporate/business/app/View_PR_Details.aspx` | `pr-po` | `—` | Bill.Master, CompanyID, WebMethod | Reviewed |
| 116 | `corporate/business/app/View_PurchaseOrder.aspx` | `pr-po` | `View_po` | Bill.Master, CompanyID | Reviewed |
| 117 | `corporate/business/app/View_chalan.aspx` | `dpcc-challan` | `View_chalan` | Bill.Master, CompanyID | Reviewed |
| 118 | `corporate/business/app/View_client.aspx` | `customer` | `View_client` | SecurePage, CompanyID | Reviewed |
| 119 | `corporate/business/app/View_payment.aspx` | `payments` | `View_payment` | Bill.Master, CompanyID | Reviewed |
| 120 | `corporate/business/app/View_proforma.aspx` | `proforma` | `View_proforma` | Bill.Master | Reviewed |
| 121 | `corporate/business/app/View_purches.aspx` | `vendor-purchase` | `View_purches` | Bill.Master, CompanyID | Reviewed |
| 122 | `corporate/business/app/View_purchess_payment.aspx` | `payments` | `View_purchess_payment` | Bill.Master | Reviewed |
| 123 | `corporate/business/app/View_quotation.aspx` | `quotation` | `View_quotation` | Bill.Master, CompanyID | Reviewed |
| 124 | `corporate/business/app/View_vendor.aspx` | `vendor-purchase` | `View_vendor` | SecurePage, CompanyID | Reviewed |
| 125 | `corporate/business/app/Viewhydrent_invoice.aspx` | `hydrant` | `Viewhydrent_invoice` | Bill.Master | Reviewed |
| 126 | `corporate/business/app/add_chalan.aspx` | `dpcc-challan` | `add_chalan` | Bill.Master, CompanyID | Reviewed |
| 127 | `corporate/business/app/add_payment.aspx` | `payments` | `add_payment` | Bill.Master, CompanyID | Reviewed |
| 128 | `corporate/business/app/add_payment_purchess.aspx` | `payments` | `add_payment_purchess` | Bill.Master, CompanyID | Reviewed |
| 129 | `corporate/business/app/attendance.aspx` | `attendance-hr` | `daily_attendance` | Bill.Master, CompanyID, WebMethod | Reviewed |
| 130 | `corporate/business/app/daily_rpt.aspx` | `sales-visit` | `daily_reporting` | SecurePage, CompanyID | Reviewed |
| 131 | `corporate/business/app/delete_purchaseorder.aspx` | `pr-po` | `Li4` | Bill.Master, CompanyID | Reviewed |
| 132 | `corporate/business/app/edit_purchaseorder.aspx` | `pr-po` | `Li5` | Bill.Master, CompanyID | Reviewed |
| 133 | `corporate/business/app/expense_entry.aspx` | `sales-visit` | `—` | SecurePage, CompanyID | Reviewed |
| 134 | `corporate/business/app/general_expences.aspx` | `expenses-made` | `general_expences` | SecurePage, CompanyID | Reviewed |
| 135 | `corporate/business/app/home.aspx` | `dashboard` | `home1` | Bill.Master, CompanyID | Reviewed |
| 136 | `corporate/business/app/hydrent_invoice.aspx` | `hydrant` | `hydrent_invoice` | Bill.Master, CompanyID | Reviewed |
| 137 | `corporate/business/app/master_State.aspx` | `masters` | `master_State` | Bill.Master, CompanyID | Reviewed |
| 138 | `corporate/business/app/master_city.aspx` | `masters` | `master_city` | Bill.Master, CompanyID | Reviewed |
| 139 | `corporate/business/app/newproduct_master.aspx` | `masters` | `newproduct_master` | SecurePage, CompanyID | Reviewed |
| 140 | `corporate/business/app/newproductparent.aspx` | `masters` | `newproductparent` | SecurePage, CompanyID | Reviewed |
| 141 | `corporate/business/app/patty_cash_expences.aspx` | `expenses-made` | `patty_cash_expences` | SecurePage, CompanyID | Reviewed |
| 142 | `corporate/business/app/product_master.aspx` | `masters` | `product_master` | SecurePage | Reviewed |
| 143 | `corporate/business/app/productparent.aspx` | `masters` | `productparent` | SecurePage | Reviewed |
| 144 | `corporate/business/app/rpts_vw_qtn_po_counts.aspx` | `reports` | `rpts_vw_qtn_po_counts` | Bill.Master | Reviewed |
| 145 | `corporate/business/app/search_products.aspx` | `vendor-purchase` | `Search_Products` | SecurePage, CompanyID | Reviewed |
| 146 | `corporate/business/app/seartch_chalan.aspx` | `dpcc-challan` | `seartch_chalan` | Bill.Master, CompanyID | Reviewed |
| 147 | `corporate/business/app/seartch_invoice.aspx` | `invoice` | `seartch_invoice` | Bill.Master, CompanyID | Reviewed |
| 148 | `corporate/business/app/seartch_payment.aspx` | `payments` | `seartch_payment` | Bill.Master, CompanyID | Reviewed |
| 149 | `corporate/business/app/seartch_purtch.aspx` | `vendor-purchase` | `seartch_purtch` | Bill.Master, CompanyID | Reviewed |
| 150 | `corporate/business/app/settings.aspx` | `users-rbac` | `settings` | Bill.Master | Reviewed |
| 151 | `corporate/business/app/srch_dailyrpts.aspx` | `sales-visit` | `srch_dailyrpts` | SecurePage, CompanyID | Reviewed |
| 152 | `corporate/business/app/view_expencess_head.aspx` | `expenses-made` | `view_expencess_head` | SecurePage | Reviewed |
| 153 | `corporate/business/app/view_patty_cash_expenses.aspx` | `expenses-made` | `view_patty_cash_expenses` | SecurePage, CompanyID | Reviewed |
| 154 | `corporate/business/app/visit_planner.aspx` | `sales-visit` | `visit_planner` | SecurePage, CompanyID, WebMethod | Reviewed |
| 155 | `corporate/business/app/vw_dailyrpts.aspx` | `sales-visit` | `vw_dailyrpts` | SecurePage, CompanyID | Reviewed |
| 156 | `corporate/business/print/General_expencess_voutcher.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 157 | `corporate/business/print/Invoice.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 158 | `corporate/business/print/Invoiceduplicate.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 159 | `corporate/business/print/NewChhalan.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 160 | `corporate/business/print/NewChhalanDuplicate.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 161 | `corporate/business/print/NewChhalanTriplicate.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 162 | `corporate/business/print/NewInvoice.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 163 | `corporate/business/print/NewInvoiceDuplicate.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 164 | `corporate/business/print/NewInvoice_v2.aspx` | `print` | `—` | EnsurePrint, CompanyID | Reviewed |
| 165 | `corporate/business/print/NewPaymentInvoice.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 166 | `corporate/business/print/NewPaymentInvoiceDuplicate.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 167 | `corporate/business/print/NewProformaInvoice.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 168 | `corporate/business/print/NewPurchaseOrder.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 169 | `corporate/business/print/NewPurchaseOrder_Print.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 170 | `corporate/business/print/NewQuotation.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 171 | `corporate/business/print/Patty_cash_expencess_voutcher.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 172 | `corporate/business/print/Print_PO.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 173 | `corporate/business/print/Quotation.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 174 | `corporate/business/print/QuotationHydrent.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 175 | `corporate/business/print/Requisition.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 176 | `corporate/business/print/RequisitionNew.aspx` | `print` | `pr_create` | EnsurePrint | Reviewed |
| 177 | `corporate/business/print/bill.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 178 | `corporate/business/print/billduplicate.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 179 | `corporate/business/print/chhalan.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 180 | `corporate/business/print/hydrentInvoice.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 181 | `corporate/business/print/hydrentInvoiceSeller.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 182 | `corporate/business/print/proforma_invoice.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 183 | `corporate/business/print/purches_bill.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 184 | `corporate/business/print/purchess_payment.aspx` | `print` | `—` | EnsurePrint | Reviewed |
| 185 | `index.aspx` | `auth` | `—` | public | Reviewed |
| 186 | `index_card.aspx` | `card-kiosk` | `—` | public | Reviewed |
| 187 | `index_start.aspx` | `auth` | `—` | public | Reviewed |
| 188 | `reset_password.aspx` | `auth` | `—` | public | Reviewed |

## Counts by domain

| Domain | Pages | Catalog |
|--------|------:|---------|
| `auth` | 4 | [DOMAIN_auth.md](DOMAIN_auth.md) |
| `dashboard` | 2 | [DOMAIN_dashboard.md](DOMAIN_dashboard.md) |
| `sales-visit` | 5 | [DOMAIN_sales-visit.md](DOMAIN_sales-visit.md) |
| `attendance-hr` | 8 | [DOMAIN_attendance-hr.md](DOMAIN_attendance-hr.md) |
| `users-rbac` | 10 | [DOMAIN_users-rbac.md](DOMAIN_users-rbac.md) |
| `masters` | 17 | [DOMAIN_masters.md](DOMAIN_masters.md) |
| `customer` | 9 | [DOMAIN_customer.md](DOMAIN_customer.md) |
| `vendor-purchase` | 11 | [DOMAIN_vendor-purchase.md](DOMAIN_vendor-purchase.md) |
| `pr-po` | 18 | [DOMAIN_pr-po.md](DOMAIN_pr-po.md) |
| `quotation` | 12 | [DOMAIN_quotation.md](DOMAIN_quotation.md) |
| `dpcc-challan` | 4 | [DOMAIN_dpcc-challan.md](DOMAIN_dpcc-challan.md) |
| `proforma` | 6 | [DOMAIN_proforma.md](DOMAIN_proforma.md) |
| `invoice` | 7 | [DOMAIN_invoice.md](DOMAIN_invoice.md) |
| `hydrant` | 5 | [DOMAIN_hydrant.md](DOMAIN_hydrant.md) |
| `payments` | 12 | [DOMAIN_payments.md](DOMAIN_payments.md) |
| `expenses-made` | 7 | [DOMAIN_expenses-made.md](DOMAIN_expenses-made.md) |
| `reports` | 5 | [DOMAIN_reports.md](DOMAIN_reports.md) |
| `print` | 29 | [DOMAIN_print.md](DOMAIN_print.md) |
| `card-kiosk` | 17 | [DOMAIN_card-kiosk.md](DOMAIN_card-kiosk.md) |

Pages with a Bill.Master menu id: **113**.

## ERP app pages not on the primary menu

These are reachable by redirect, popup, leftover workflow, or secondary links. They still need a catalog row.

| Page | Domain | Notes |
|------|--------|-------|
| `corporate/business/app/AdminAttendanceDashboard.aspx` | `attendance-hr` | Enterprise Attendance Register |
| `corporate/business/app/AdminShiftAssignment.aspx` | `attendance-hr` | Shift Assignment |
| `corporate/business/app/Edit_quatation.aspx` | `quotation` | Edit quatation |
| `corporate/business/app/Generate_PO_Preview.aspx` | `pr-po` | Generate PO Preview · QS reqNo |
| `corporate/business/app/HydrantProduct.aspx` | `hydrant` | Hydrant Product |
| `corporate/business/app/NewUpdate_product.aspx` | `masters` | New Update product · QS Id |
| `corporate/business/app/PaymentsReceived.aspx` | `payments` | Payments Received |
| `corporate/business/app/Purches_new_vendor.aspx` | `vendor-purchase` | Purches new vendor |
| `corporate/business/app/QuickAction.aspx` | `dashboard` | Quick Action · QS t |
| `corporate/business/app/RequisitionCreate.aspx` | `pr-po` | Requisition Create |
| `corporate/business/app/RequisitionView.aspx` | `pr-po` | Requisition View |
| `corporate/business/app/Service_update.aspx` | `masters` | Service update · QS Id |
| `corporate/business/app/ShowFactory.aspx` | `customer` | Show Factory · QS Client_Id |
| `corporate/business/app/Show_representative.aspx` | `customer` | Show representative · QS Client_Id |
| `corporate/business/app/Update/contactno.aspx` | `users-rbac` | contactno |
| `corporate/business/app/Update/emailid.aspx` | `users-rbac` | emailid |
| `corporate/business/app/Update/name.aspx` | `users-rbac` | name |
| `corporate/business/app/Update/password.aspx` | `users-rbac` | password |
| `corporate/business/app/Update_Designation.aspx` | `users-rbac` | Assign User Roles · QS User_Id |
| `corporate/business/app/Update_client.aspx` | `customer` | Update Client · QS Client_Id |
| `corporate/business/app/Update_factory.aspx` | `customer` | Update factory · QS ID |
| `corporate/business/app/Update_product.aspx` | `masters` | Update product · QS Id |
| `corporate/business/app/Update_vendor.aspx` | `vendor-purchase` | Update Vendor · QS Vendor_Id |
| `corporate/business/app/Vendor_quotation.aspx` | `quotation` | Vendor quotation |
| `corporate/business/app/View_PO_Details.aspx` | `pr-po` | View PO Details · QS poId |
| `corporate/business/app/View_PR_Details.aspx` | `pr-po` | Modify/View PR · QS mode, reqNo |
| `corporate/business/app/expense_entry.aspx` | `sales-visit` | Add Expense · QS visitId |
