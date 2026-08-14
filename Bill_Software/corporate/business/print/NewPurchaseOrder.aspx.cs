using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Bill_Software.corporate.business.app; // Required for CompanyContext (multi-tenant enforcement)

namespace Bill_Software.corporate.business.print
{
    /// <summary>
    /// Renders a true Procure-to-Pay Purchase Order document sourced from tbl_PO_Header /
    /// tbl_PO_Items / tbl_Vendor. This page was originally cloned from the Quotation print
    /// page; all Quotation/Challan/Payment-Phase specific binding logic has been removed.
    /// </summary>
    public partial class NewPurchaseOrder : System.Web.UI.Page
    {
        private readonly DB_UTILITY DbCL = new DB_UTILITY();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Accept "PO_Id" as the canonical key, falling back to "poId" for
                // compatibility with the naming used elsewhere in the PO module.
                string poId = Request.QueryString["PO_Id"];
                if (string.IsNullOrEmpty(poId))
                {
                    poId = Request.QueryString["poId"];
                }

                if (string.IsNullOrEmpty(poId))
                {
                    ShowDocumentError("No Purchase Order reference was supplied in the URL.");
                    return;
                }

                BindPOData(poId);
            }
        }

        #region PO Header / Vendor / Items Binding

        /// <summary>
        /// Loads the tbl_PO_Header record for the given PO_Id, binds it to the Document
        /// Information + Terms & Conditions labels, then cascades into BindVendorDetails
        /// and BindPOItems. Finally logs a "viewed/printed" system notification.
        /// </summary>
        private void BindPOData(string poId)
        {
            int poIdValue;
            if (!int.TryParse(poId, out poIdValue))
            {
                ShowDocumentError("Invalid Purchase Order reference.");
                return;
            }

            // Multi-Tenant Data Segregation: every query in this page is strictly scoped
            // to the current tenant via a parameterized CompanyID filter.
            string query = @"
                SELECT PO_Id, PO_No, PO_Date, ReqNo, EngineerName, VendorId,
                       DispatchMode, DispatchUpto, DeliveryBasis, FreightTerms, Remarks
                FROM tbl_PO_Header
                WHERE PO_Id = @PO_Id AND CompanyID = @CompanyID";

            SqlParameter[] pram = {
                new SqlParameter("@PO_Id", poIdValue),
                new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID)
            };

            DataTable dtHeader = DbCL.SPreturn_dt(query, pram);
            if (dtHeader.Rows.Count == 0)
            {
                ShowDocumentError("Purchase Order not found, or you do not have access to it.");
                return;
            }

            DataRow row = dtHeader.Rows[0];

            string poNo = row["PO_No"].ToString();
            lblPONo.Text = poNo;

            DateTime poDate;
            lblPODate.Text = DateTime.TryParse(row["PO_Date"].ToString(), out poDate)
                ? poDate.ToString("dd-MMM-yyyy")
                : "No Data";

            lblReqNo.Text = row["ReqNo"].ToString();
            lblEngineerName.Text = row["EngineerName"].ToString();

            // --- Terms & Conditions ---
            string dispatchMode = row["DispatchMode"].ToString();
            string dispatchUpto = row["DispatchUpto"].ToString();
            lblDispatchMode.Text = string.IsNullOrWhiteSpace(dispatchMode)
                ? "N/A"
                : (string.IsNullOrWhiteSpace(dispatchUpto) ? dispatchMode : $"{dispatchMode} (Upto: {dispatchUpto})");

            string deliveryBasis = row["DeliveryBasis"].ToString();
            lblDeliveryBasis.Text = string.IsNullOrWhiteSpace(deliveryBasis) ? "N/A" : deliveryBasis;

            string freightTerms = row["FreightTerms"].ToString();
            lblFreightTerms.Text = string.IsNullOrWhiteSpace(freightTerms) ? "N/A" : freightTerms;

            string remarks = row["Remarks"].ToString();
            if (string.IsNullOrWhiteSpace(remarks))
            {
                // No special instructions were recorded for this PO, so hide the row
                // entirely rather than printing a redundant "N/A" line.
                tblSpecialInstructions.Visible = false;
            }
            else
            {
                lblRemarks.Text = remarks;
            }

            string vendorId = row["VendorId"].ToString();
            BindVendorDetails(vendorId);
            BindPOItems(poId);

            // Proactive Notification Logging: every view/print of a PO is captured on the
            // Auto-Scrolling Notification Dashboard (home.aspx) in real-time.
            LogSystemNotification(poNo);
        }

        /// <summary>
        /// Binds the "Vendor Details (To)" panel from tbl_Vendor.
        /// </summary>
        private void BindVendorDetails(string vendorId)
        {
            int vendorIdValue;
            if (!int.TryParse(vendorId, out vendorIdValue))
            {
                return;
            }

            string query = @"
                SELECT Vendor_Name, Address1, Address2, City, State, pin, Pan_No, Vat_No, Com_email, Rep_phone
                FROM tbl_Vendor
                WHERE Id = @VendorId AND CompanyID = @CompanyID";

            SqlParameter[] pram = {
                new SqlParameter("@VendorId", vendorIdValue),
                new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID)
            };

            DataTable dtVendor = DbCL.SPreturn_dt(query, pram);
            if (dtVendor.Rows.Count == 0)
            {
                return;
            }

            DataRow row = dtVendor.Rows[0];

            string address1 = row["Address1"].ToString();
            string address2 = row["Address2"].ToString();
            string fullAddress = string.IsNullOrWhiteSpace(address2) ? address1 : $"{address1} {address2}";

            lblVendorName.Text = row["Vendor_Name"].ToString();
            lblVendorAddress.Text = fullAddress;
            lblVendorCity.Text = row["City"].ToString();
            lblVendorState.Text = row["State"].ToString();
            lblVendorPincode.Text = row["pin"].ToString();

            string panNo = row["Pan_No"].ToString();
            lblVendorPAN.Text = string.IsNullOrWhiteSpace(panNo) ? "N/A" : panNo;

            string gstNo = row["Vat_No"].ToString();
            lblVendorGST.Text = string.IsNullOrWhiteSpace(gstNo) ? "N/A" : gstNo;

            string email = row["Com_email"].ToString();
            string phone = row["Rep_phone"].ToString();
            lblVendorContact.Text = $"Email: {email} | Phone: {phone}";
        }

        /// <summary>
        /// Binds tbl_PO_Items into a dynamically generated HTML table (S.No, Product Name,
        /// Product ID, Quantity, Rate, Tax Rate, Net Amount) and computes the Grand Total.
        /// </summary>
        private void BindPOItems(string poId)
        {
            int poIdValue;
            if (!int.TryParse(poId, out poIdValue))
            {
                lblPOItems.Text = "<i>Invalid Purchase Order reference.</i>";
                return;
            }

            string query = @"
                SELECT ItemOrder, ProductId, ProductName, Quantity, Rate, TaxRate, NetAmount
                FROM tbl_PO_Items
                WHERE PO_Id = @PO_Id AND CompanyID = @CompanyID
                ORDER BY ItemOrder";

            SqlParameter[] pram = {
                new SqlParameter("@PO_Id", poIdValue),
                new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID)
            };

            DataTable dtItems = DbCL.SPreturn_dt(query, pram);

            if (dtItems.Rows.Count == 0)
            {
                lblPOItems.Text = "<i>No line items found for this Purchase Order.</i>";
                return;
            }

            StringBuilder strItems = new StringBuilder();
            decimal grandTotal = 0;

            strItems.Append("<table style='border-collapse:collapse; width:100%; border:2px solid #6c6c6c;'>");
            strItems.Append("<thead style='background-color:#24285F; color:white; font-weight:bold; font-size:11px;'>");
            strItems.Append("<tr>");
            strItems.Append("<th style='width:5%; border:1px solid #6c6c6c; padding:6px;'>S.No</th>");
            strItems.Append("<th style='width:35%; border:1px solid #6c6c6c; padding:6px;'>Product Name</th>");
            strItems.Append("<th style='width:12%; border:1px solid #6c6c6c; padding:6px;'>Product ID</th>");
            strItems.Append("<th style='width:12%; border:1px solid #6c6c6c; padding:6px;'>Quantity</th>");
            strItems.Append("<th style='width:12%; border:1px solid #6c6c6c; padding:6px;'>Rate (\u20B9)</th>");
            strItems.Append("<th style='width:10%; border:1px solid #6c6c6c; padding:6px;'>Tax Rate (%)</th>");
            strItems.Append("<th style='width:14%; border:1px solid #6c6c6c; padding:6px;'>Net Amount (\u20B9)</th>");
            strItems.Append("</tr></thead><tbody>");

            int slNo = 1;
            foreach (DataRow row in dtItems.Rows)
            {
                string productId = row["ProductId"].ToString();
                string productName = row["ProductName"].ToString();

                decimal quantity = ToSafeDecimal(row["Quantity"]);
                decimal rate = ToSafeDecimal(row["Rate"]);
                decimal taxRate = ToSafeDecimal(row["TaxRate"]);
                decimal netAmount = ToSafeDecimal(row["NetAmount"]);

                grandTotal += netAmount;

                strItems.Append("<tr>");
                strItems.Append($"<td style='text-align:center; border:1px solid #6c6c6c; padding:5px; font-size:10.5px;'>{slNo}</td>");
                strItems.Append($"<td style='text-align:left; border:1px solid #6c6c6c; padding:5px; font-size:10.5px;'>{productName}</td>");
                strItems.Append($"<td style='text-align:center; border:1px solid #6c6c6c; padding:5px; font-size:10.5px;'>{productId}</td>");
                strItems.Append($"<td style='text-align:center; border:1px solid #6c6c6c; padding:5px; font-size:10.5px;'>{quantity:0.##}</td>");
                strItems.Append($"<td style='text-align:right; border:1px solid #6c6c6c; padding:5px; font-size:10.5px;'>{rate:0.00}</td>");
                strItems.Append($"<td style='text-align:center; border:1px solid #6c6c6c; padding:5px; font-size:10.5px;'>{taxRate:0.##}%</td>");
                strItems.Append($"<td style='text-align:right; border:1px solid #6c6c6c; padding:5px; font-size:10.5px;'>{netAmount:0.00}</td>");
                strItems.Append("</tr>");

                slNo++;
            }

            strItems.Append("</tbody>");
            strItems.Append("<tfoot>");
            strItems.Append("<tr style='background-color:#d9d3d3; font-weight:bold;'>");
            strItems.Append("<td colspan='6' style='border:1px solid #6c6c6c; text-align:right; padding:6px;'>GRAND TOTAL</td>");
            strItems.Append($"<td style='border:1px solid #6c6c6c; text-align:right; padding:6px;'>\u20B9{grandTotal:0.00}</td>");
            strItems.Append("</tr>");

            string amountInWords = MoneyConvDS.MoneyConvFn(Math.Round(grandTotal, 2).ToString());
            strItems.Append($"<tr><td colspan='7' style='background-color:#24285F; color:white; text-align:left; padding:6px; font-size:11px;'>Amount (In Words): {amountInWords}</td></tr>");
            strItems.Append("</tfoot></table>");

            lblPOItems.Text = strItems.ToString();
        }

        private static decimal ToSafeDecimal(object value)
        {
            decimal result;
            return decimal.TryParse(value?.ToString(), out result) ? result : 0;
        }

        private void ShowDocumentError(string message)
        {
            lblPONo.Text = "N/A";
            lblPOItems.Text = $"<i>{message}</i>";
        }

        #endregion

        #region Proactive Notification Logging

        /// <summary>
        /// Inserts a system-wide notification so the Auto-Scrolling Notification Dashboard
        /// on home.aspx reflects this PO having been viewed/printed, in real-time.
        /// </summary>
        private void LogSystemNotification(string poNo)
        {
            string sql = @"INSERT INTO tbl_SystemNotification 
                           (Title, Message, ModuleCode, Severity, StartDate, EndDate, IsActive, CreatedBy, CompanyID) 
                           VALUES 
                           (@Title, @Msg, @Mod, @Type, GETDATE(), DATEADD(day, 30, GETDATE()), 1, @User, @Comp)";

            SqlParameter[] pram = {
                new SqlParameter("@Title", "Purchase Order Viewed"),
                new SqlParameter("@Msg", $"Purchase Order {poNo} was viewed/printed."),
                new SqlParameter("@Mod", "PURCHASE_ORDER"),
                new SqlParameter("@Type", "Info"),
                new SqlParameter("@User", Session["USERID"] != null ? Session["USERID"].ToString() : "System"),
                new SqlParameter("@Comp", CompanyContext.CurrentCompanyID)
            };

            DbCL.ExecuteNonQuery(sql, pram);
        }

        #endregion

        protected void Button1_Click(object sender, EventArgs e)
        {
            // Print Without Letterhead: handled entirely client-side via OnClientClick.
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            // Print With Letterhead: handled entirely client-side via OnClientClick.
        }
    }
}
