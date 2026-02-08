using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.print
{
    public partial class Print_PO : System.Web.UI.Page
    {
        decimal _totalQty = 0, _totalTaxable = 0, _totalGST = 0, _grandTotal = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Request.QueryString["poId"] != null)
            {
                int poId;
                if (int.TryParse(Request.QueryString["poId"], out poId))
                    LoadPODetails(poId);
            }
        }

        private void LoadPODetails(int poId)
        {
            DataSet ds = GetPOData(poId);
            if (ds == null || ds.Tables.Count < 3 || ds.Tables[0].Rows.Count == 0) return;

            DataRow hdr = ds.Tables[0].Rows[0];
            DataTable partyTable = ds.Tables[1];
            DataTable itemTable = ds.Tables[2];

            // 1. Map Header & Terms
            BindHeaderAndTerms(hdr);

            // 2. Map Address Blocks
            BindAddresses(partyTable);

            // 3. Map Payment
            BindPaymentDetails(hdr);

            // 4. Bind Grid (Accumulates Totals)
            _totalQty = 0; _totalTaxable = 0; _totalGST = 0; _grandTotal = 0;
            gvItems.DataSource = itemTable;
            gvItems.DataBind();

            // 5. Finalize Summary & Words
            lblGrandTotal.Text = _grandTotal.ToString("N2");
            lblAmountInWords.Text = "Rupees " + NumberToWords((long)_grandTotal) + " Only";

            // 6. GST Calculation
            BindGSTSummary(partyTable);
        }

        private void BindHeaderAndTerms(DataRow r)
        {
            lblPONo.Text = Safe(r["PO_No"]);
            lblPODate.Text = FormatDate(r["PO_Date"]);
            lblReqNo.Text = Safe(r["ReqNo"]);
            lblEngineer.Text = Safe(r["EngineerName"]);
            lblRemarks.Text = Safe(r["Remarks"]);
            lblPreparedBy.Text = Safe(r["CreatedBy"]);

            lblRateRef.Text = Safe(r["SpecialRateRef"]);
            lblApprovedBy.Text = Safe(r["SpecialRateApprovedBy"]);
            lblFreight.Text = Safe(r["FreightTerms"]);
            lblDispatchMode.Text = Safe(r["DispatchMode"]);

            string disp = Safe(r["DispatchUpto"]);
            DateTime dDisp;
            lblDispatchUpto.Text = DateTime.TryParse(disp, out dDisp) ? dDisp.ToString("dd-MMM-yyyy") : disp;

            lblDeliveryBasis.Text = Safe(r["DeliveryBasis"]);
        }

        private void BindAddresses(DataTable dt)
        {
            // Vendor (Two Columns)
            DataRow[] vendors = dt.Select("PartyRole='Vendor'");
            if (vendors.Length > 0)
            {
                DataRow v = vendors[0];
                // Left Column: Name & Address
                StringBuilder sbLeft = new StringBuilder();
                sbLeft.Append($"<div style='font-size:13px; font-weight:bold; margin-bottom:4px;'>{Safe(v["Name"])}</div>");
                sbLeft.Append($"{Safe(v["Address"]).Replace(",", ",<br/>")}<br/>");
                sbLeft.Append($"{Safe(v["City"])}, {Safe(v["State"])} - {Safe(v["Pin"])}");
                litVendorLeft.Text = sbLeft.ToString();

                // Right Column: Contact, Email, GST, PAN
                StringBuilder sbRight = new StringBuilder();
                sbRight.Append(VendorRow("Contact Person", Safe(v["ContactPerson"])));
                sbRight.Append(VendorRow("Contact No.", Safe(v["ContactNo"])));
                sbRight.Append(VendorRow("Email ID", Safe(v["Email"])));
                sbRight.Append(VendorRow("PAN No", Safe(v["PANNo"])));
                sbRight.Append(VendorRow("GSTIN", Safe(v["GSTNo"])));
                litVendorRight.Text = sbRight.ToString();
            }

            // Bill To & Ship To
            litBillTo.Text = BuildAddressLabelFormat(dt, "BillTo");
            litShipTo.Text = BuildAddressLabelFormat(dt, "ShipTo");
        }

        private string VendorRow(string label, string val)
        {
            if (string.IsNullOrEmpty(val)) return "";
            return $"<div style='margin-bottom:3px;'><span class='info-label'>{label}:</span> {val}</div>";
        }

        private string BuildAddressLabelFormat(DataTable dt, string role)
        {
            DataRow[] rows = dt.Select($"PartyRole='{role}'");
            if (rows.Length == 0) return "";
            DataRow r = rows[0];

            StringBuilder sb = new StringBuilder();
            sb.Append(VendorRow("Company Name", Safe(r["Name"])));
            sb.Append(VendorRow("Address", $"{Safe(r["Address"])} {Safe(r["City"])} {Safe(r["Pin"])}"));
            sb.Append(VendorRow("Contact Person", Safe(r["ContactPerson"])));
            sb.Append(VendorRow("Contact No.", Safe(r["ContactNo"])));
            sb.Append(VendorRow("Email ID", Safe(r["Email"])));
            sb.Append(VendorRow("PAN No", Safe(r["PANNo"])));
            sb.Append(VendorRow("GSTIN", Safe(r["GSTNo"])));
            return sb.ToString();
        }

        private void BindPaymentDetails(DataRow r)
        {
            try
            {
                lblPayMode.Text = Safe(r["PaymentMode"]);
                lblChequeNo.Text = Safe(r["ChequeNo"]);
                lblChequeDate.Text = FormatDate(r["ChequeDate"]);
                lblPayAmount.Text = Safe(r["Amount"]) != "" ? Convert.ToDecimal(r["Amount"]).ToString("N2") : "-";
                lblBankName.Text = Safe(r["BankName"]);
            }
            catch { }
        }

        private void BindGSTSummary(DataTable dt)
        {
            string vState = "", bState = "";
            DataRow[] v = dt.Select("PartyRole='Vendor'");
            DataRow[] b = dt.Select("PartyRole='BillTo'");
            if (v.Length > 0) vState = Safe(v[0]["State"]);
            if (b.Length > 0) bState = Safe(b[0]["State"]);

            decimal taxPer = _totalTaxable > 0 ? (_totalGST / _totalTaxable) * 100 : 0;
            bool isIntra = !string.IsNullOrEmpty(vState) && vState.Equals(bState, StringComparison.OrdinalIgnoreCase);

            StringBuilder sb = new StringBuilder();
            if (isIntra)
            {
                decimal halfAmt = Math.Round(_totalGST / 2, 2);
                decimal halfPer = taxPer / 2;
                sb.Append($"<tr><td>CGST</td><td class='text-center'>{halfPer:0.##}%</td><td class='text-right'>{halfAmt:N2}</td></tr>");
                sb.Append($"<tr><td>SGST</td><td class='text-center'>{halfPer:0.##}%</td><td class='text-right'>{halfAmt:N2}</td></tr>");
            }
            else
            {
                sb.Append($"<tr><td>IGST</td><td class='text-center'>{taxPer:0.##}%</td><td class='text-right'>{_totalGST:N2}</td></tr>");
            }
            phGST.Controls.Add(new LiteralControl(sb.ToString()));
        }

        protected void gvItems_RowDataBound1(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                _totalQty += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "Quantity"));
                _totalTaxable += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "TaxableAmount"));
                _totalGST += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "TaxAmount"));
                _grandTotal += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "NetAmount"));
            }
            else if (e.Row.RowType == DataControlRowType.Footer)
            {
                e.Row.Cells[1].Text = "TOTAL";
                e.Row.Cells[1].Font.Bold = true;
                e.Row.Cells[2].Text = _totalQty.ToString("N2");
                e.Row.Cells[2].CssClass = "text-center bold";
                e.Row.Cells[5].Text = _totalTaxable.ToString("N2");
                e.Row.Cells[5].CssClass = "text-right bold";
                e.Row.Cells[6].Text = _totalGST.ToString("N2");
                e.Row.Cells[6].CssClass = "text-right bold";
                e.Row.Cells[7].Text = _grandTotal.ToString("N2");
                e.Row.Cells[7].CssClass = "text-right bold";
            }
        }

        private DataSet GetPOData(int poId)
        {
            DataSet ds = new DataSet();
            string cs = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(cs))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetReleasedPO_Details", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PO_Id", poId);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                }
            }
            return ds;
        }

        private string Safe(object o) { return o == null || o == DBNull.Value ? "" : o.ToString().Trim(); }
        private string FormatDate(object o)
        {
            DateTime dt;
            return DateTime.TryParse(Safe(o), out dt) ? dt.ToString("dd-MMM-yyyy") : Safe(o);
        }

        private string NumberToWords(long number)
        {
            if (number == 0) return "Zero";
            if (number < 0) return "Minus " + NumberToWords(Math.Abs(number));
            string words = "";
            if ((number / 10000000) > 0) { words += NumberToWords(number / 10000000) + " Crore "; number %= 10000000; }
            if ((number / 100000) > 0) { words += NumberToWords(number / 100000) + " Lakh "; number %= 100000; }
            if ((number / 1000) > 0) { words += NumberToWords(number / 1000) + " Thousand "; number %= 1000; }
            if ((number / 100) > 0) { words += NumberToWords(number / 100) + " Hundred "; number %= 100; }
            if (number > 0)
            {
                if (words != "") words += "and ";
                var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
                if (number < 20) words += unitsMap[number];
                else words += tensMap[number / 10] + " " + unitsMap[number % 10];
            }
            return words.Trim();
        }
    }
}