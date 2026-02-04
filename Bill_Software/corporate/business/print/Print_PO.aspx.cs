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
        decimal _totalQty = 0;
        decimal _totalDisc = 0;
        decimal _totalTaxable = 0;
        decimal _totalGST = 0;
        decimal _grandTotal = 0;
        decimal _totalGSTAmount = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["poId"] == null)
                    return;
                int poId;
                if (!int.TryParse(Request.QueryString["poId"], out poId))
                    return;

                LoadPODetails(poId);
            }
        }

        private void LoadPODetails(int poId)
        {
            DataSet ds = GetPOData(poId);

            if (ds.Tables.Count < 3 || ds.Tables[0].Rows.Count == 0)
                return;

            DataRow hdr = ds.Tables[0].Rows[0];
            DataTable partyTable = ds.Tables[1];
            DataTable itemTable = ds.Tables[2];

            // 1️⃣ Header (PO No, Date, Engineer, Dispatch, etc.)
            BindHeader(ds.Tables[0]);

            // 2️⃣ Parties (Vendor / Bill To / Ship To)
            BindParties(partyTable);

            // 3️⃣ Commercial / Logistics Terms  ✅ (THIS WAS MISSING)
            BindCommercialTerms(hdr, partyTable);

            // 4️⃣ Items (bind grid + accumulate totals)
            BindItems(itemTable);

            // 5️⃣ Totals (if you still need this method)
            BindTotalsFromItems(itemTable);

            // 6️⃣ GST Split (uses final _totalGST + party states)
            BindGSTSplit(partyTable);
        }



        #region DB

        private DataSet GetPOData(int poId)
        {
            DataSet ds = new DataSet();

            using (SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                using (SqlCommand cmd =
                    new SqlCommand("sp_GetReleasedPO_Details", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PO_Id", poId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                }
            }

            return ds;
        }

        private void BindGSTSplit(DataTable partyTable)
        {
            decimal totalGST = _totalGST; // already calculated from items

            lblTotalGST.Text = totalGST.ToString("N2");

            if (IsIntraState(partyTable))
            {
                decimal halfGST = totalGST / 2;

                lblCGST.Text = halfGST.ToString("N2");
                lblSGST.Text = halfGST.ToString("N2");

                //lblCGST.Text = $"@{_gstRate / 2}% {halfGST:N2}";
                //lblSGST.Text = $"@{_gstRate / 2}% {halfGST:N2}";

                phCGSTSGST.Visible = true;
                phIGST.Visible = false;
            }
            else
            {
                lblIGST.Text = totalGST.ToString("N2");

                phIGST.Visible = true;
                phCGSTSGST.Visible = false;
            }
        }


        #endregion

        #region Header

        private bool IsIntraState(DataTable partyTable)
        {
            string vendorState = "";
            string billToState = "";

            foreach (DataRow r in partyTable.Rows)
            {
                if (r["PartyRole"].ToString() == "Vendor")
                    vendorState = r["State"].ToString().Trim();

                if (r["PartyRole"].ToString() == "BillTo")
                    billToState = r["State"].ToString().Trim();
            }

            return !string.IsNullOrEmpty(vendorState)
                && vendorState.Equals(billToState, StringComparison.OrdinalIgnoreCase);
        }


        private void BindHeader(DataTable dt)
        {
            DataRow r = dt.Rows[0];

            lblPONo.Text = r["PO_No"].ToString();
            lblReqNo.Text = r["ReqNo"].ToString();
            lblPODate.Text =
                Convert.ToDateTime(r["PO_Date"]).ToString("dd-MMM-yyyy");

            lblEngineer.Text = r["EngineerName"].ToString();
            lblDispatchMode.Text = r["DispatchMode"].ToString();
            lblDeliveryBasis.Text = r["DeliveryBasis"].ToString();

            lblPreparedBy.Text = r["CreatedBy"].ToString();
            lblRemarks.Text = r["Remarks"].ToString();
        }

        #endregion

        #region Parties

        private void BindParties(DataTable dt)
        {
            litVendor.Text = BuildPartyHtml(dt, "Vendor");
            litBillTo.Text = BuildPartyHtml(dt, "BillTo");
            litShipTo.Text = BuildPartyHtml(dt, "ShipTo");
        }

        private string BuildPartyHtml(DataTable dt, string role)
        {
            DataRow[] rows = dt.Select($"PartyRole='{role}'");
            if (rows.Length == 0) return string.Empty;

            DataRow r = rows[0];
            StringBuilder sb = new StringBuilder();

            /* =========================
               VENDOR – 2 COLUMN LAYOUT
               ========================= */
            if (role == "Vendor")
            {
                sb.Append("<table width='100%' style='border-collapse:collapse;'>");
                sb.Append("<tr>");

                /* LEFT COLUMN : NAME + ADDRESS */
                sb.Append("<td width='70%' valign='top'>");

                sb.Append("<b>" + r["Name"] + "</b><br/>");

                if (!string.IsNullOrWhiteSpace(r["Address"].ToString()))
                    sb.Append(
                        HttpUtility.HtmlEncode(r["Address"].ToString())
                        .Replace(",", ",<br/>") + "<br/>"
                    );

                string city = r["City"].ToString();
                string state = r["State"].ToString();
                string pin = r["Pin"].ToString();

                if (!string.IsNullOrWhiteSpace(city) || !string.IsNullOrWhiteSpace(state))
                    sb.Append($"{city}{(city != "" && state != "" ? ", " : "")}{state}<br/>");

                if (!string.IsNullOrWhiteSpace(pin))
                    sb.Append(pin + "<br/>");

                if (!string.IsNullOrWhiteSpace(r["GSTNo"].ToString()))
                    sb.Append("<b>GSTIN:</b> " + r["GSTNo"] + "<br/>");

                if (!string.IsNullOrWhiteSpace(r["PANNo"].ToString()))
                    sb.Append("<b>PAN:</b> " + r["PANNo"] + "<br/>");

                sb.Append("</td>");

                /* RIGHT COLUMN : CONTACT DETAILS */
                sb.Append("<td width='30%' valign='top' style='text-align:right; padding-top:10px;'>");

                if (!string.IsNullOrWhiteSpace(r["ContactPerson"].ToString()))
                    sb.Append("<b>Contact:</b> " + r["ContactPerson"] + "<br/>");

                if (!string.IsNullOrWhiteSpace(r["ContactNo"].ToString()))
                    sb.Append("<b>Phone:</b> " + r["ContactNo"] + "<br/>");

                sb.Append("</td>");

                sb.Append("</tr>");
                sb.Append("</table>");

                return sb.ToString();
            }

            /* =========================
               BILL TO / SHIP TO – SAME AS OLD
               ========================= */

            sb.Append("<b>" + r["Name"] + "</b><br/>");

            if (!string.IsNullOrWhiteSpace(r["Address"].ToString()))
                sb.Append(
                    HttpUtility.HtmlEncode(r["Address"].ToString())
                    .Replace(",", ",<br/>") + "<br/>"
                );

            string c = r["City"].ToString();
            string s = r["State"].ToString();
            string p = r["Pin"].ToString();

            if (!string.IsNullOrWhiteSpace(c) || !string.IsNullOrWhiteSpace(s))
                sb.Append($"{c}{(c != "" && s != "" ? ", " : "")}{s}<br/>");

            if (!string.IsNullOrWhiteSpace(p))
                sb.Append(p + "<br/>");

            if (!string.IsNullOrWhiteSpace(r["GSTNo"].ToString()))
                sb.Append("<b>GSTIN:</b> " + r["GSTNo"] + "<br/>");

            if (!string.IsNullOrWhiteSpace(r["PANNo"].ToString()))
                sb.Append("<b>PAN:</b> " + r["PANNo"] + "<br/>");

            if (!string.IsNullOrWhiteSpace(r["ContactPerson"].ToString()))
                sb.Append("<b>Contact:</b> " + r["ContactPerson"] + "<br/>");

            if (!string.IsNullOrWhiteSpace(r["ContactNo"].ToString()))
                sb.Append("<b>Phone:</b> " + r["ContactNo"] + "<br/>");

            return sb.ToString();
        }



        private string BuildPartyHtml_OLD(DataTable dt, string role)
        {
            DataRow[] rows = dt.Select($"PartyRole='{role}'");
            if (rows.Length == 0) return string.Empty;

            DataRow r = rows[0];
            StringBuilder sb = new StringBuilder();

            sb.Append("<b>" + r["Name"] + "</b><br/>");

            //if (!string.IsNullOrWhiteSpace(r["Address"].ToString()))
            //    sb.Append(r["Address"] + "<br/>");

            if (!string.IsNullOrWhiteSpace(r["Address"].ToString()))
                //sb.Append(r["Address"].ToString().Replace(",", ",<br/>") + "<br/>");
                sb.Append(HttpUtility.HtmlEncode(r["Address"].ToString()).Replace(",", ",<br/>"));

            //sb.Append($"{r["City"]}, {r["State"]} {r["Pin"]}<br/>");

            string city = r["City"].ToString();
            string state = r["State"].ToString();
            string pin = r["Pin"].ToString();

            if (!string.IsNullOrWhiteSpace(city) || !string.IsNullOrWhiteSpace(state))
            {
                sb.Append($"{city}{(city != "" && state != "" ? ", " : "")}{state}<br/>");
            }

            if (!string.IsNullOrWhiteSpace(pin))
                sb.Append(pin + "<br/>");


            //if (!string.IsNullOrEmpty(r["GSTNo"].ToString()))
            //    sb.Append("<b>GST:</b> " + r["GSTNo"] + "<br/>");

            //if (!string.IsNullOrEmpty(r["ContactPerson"].ToString()))
            //    sb.Append("<b>Contact:</b> " + r["ContactPerson"] + "<br/>");

            //if (!string.IsNullOrEmpty(r["ContactNo"].ToString()))
            //    sb.Append("<b>Phone:</b> " + r["ContactNo"] + "<br/>");

            //if (!string.IsNullOrWhiteSpace(r["GSTNo"].ToString()))
            //    sb.Append("<b>GST:</b> " + r["GSTNo"] + "<br/>");

            if (!string.IsNullOrWhiteSpace(r["GSTNo"].ToString()))
                sb.Append("<b>GSTIN:</b> " + r["GSTNo"] + "<br/>");

            if (!string.IsNullOrWhiteSpace(r["PANNo"].ToString()))
                sb.Append("<b>PAN:</b> " + r["PANNo"] + "<br/>");

            if (!string.IsNullOrWhiteSpace(r["ContactPerson"].ToString()))
                sb.Append("<b>Contact:</b> " + r["ContactPerson"] + "<br/>");

            if (!string.IsNullOrWhiteSpace(r["ContactNo"].ToString()))
                sb.Append("<b>Phone:</b> " + r["ContactNo"] + "<br/>");

            return sb.ToString();
        }

        #endregion

        #region Items

        private void BindItems(DataTable dt)
        {
            // 🔹 RESET TOTALS (VERY IMPORTANT)
            _totalQty = 0;
            _totalDisc = 0;
            _totalTaxable = 0;
            _totalGST = 0;
            _grandTotal = 0;

            gvItems.DataSource = dt;
            gvItems.DataBind();
        }


        #endregion

        #region Totals (Derived from Items)

        private void BindTotalsFromItems(DataTable dt)
        {
            decimal totalQty = 0;
            decimal totalGST = 0;
            decimal grandTotal = 0;

            foreach (DataRow r in dt.Rows)
            {
                totalQty += Convert.ToDecimal(r["Quantity"]);
                totalGST += Convert.ToDecimal(r["TaxAmount"]);
                grandTotal += Convert.ToDecimal(r["NetAmount"]);
            }

            //lblTotalQty.Text = totalQty.ToString("N2");
            //lblTotalGST.Text = totalGST.ToString("N2");
            //lblGrandTotal.Text = grandTotal.ToString("N2");
        }

        #endregion

        //protected void gvItems_RowDataBound1(object sender, System.Web.UI.WebControls.GridViewRowEventArgs e)
        //{
        //    // Data rows
        //    if (e.Row.RowType == DataControlRowType.DataRow)
        //    {
        //        _totalQty += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "Quantity"));
        //        _totalDisc += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "DiscountAmount"));
        //        _totalTaxable += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "TaxableAmount"));
        //        _totalGST += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "TaxAmount"));
        //        _grandTotal += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "NetAmount"));
        //    }

        //    // Footer row
        //    if (e.Row.RowType == DataControlRowType.Footer)
        //    {
        //        e.Row.Cells[1].Text = "<b>TOTAL</b>";
        //        e.Row.Cells[1].HorizontalAlign = HorizontalAlign.Right;

        //        e.Row.Cells[2].Text = _totalQty.ToString("N2");
        //        e.Row.Cells[4].Text = _totalDisc.ToString("N2");
        //        e.Row.Cells[5].Text = _totalTaxable.ToString("N2");
        //        e.Row.Cells[6].Text = _totalGST.ToString("N2");
        //        e.Row.Cells[7].Text = _grandTotal.ToString("N2");

        //        e.Row.Font.Bold = true;
        //    }
        //    lblAmountInWords.Text = AmountInWords(Convert.ToDecimal(_grandTotal));

        //}

        protected void gvItems_RowDataBound1(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                _totalQty += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "Quantity"));
                _totalDisc += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "DiscountAmount"));
                _totalTaxable += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "TaxableAmount"));
                _totalGST += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "TaxAmount"));
                _grandTotal += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "NetAmount"));
            }

            if (e.Row.RowType == DataControlRowType.Footer)
            {
                e.Row.Cells[1].Text = "<b>TOTAL</b>";
                e.Row.Cells[1].HorizontalAlign = HorizontalAlign.Right;

                e.Row.Cells[2].Text = _totalQty.ToString("N2");
                e.Row.Cells[4].Text = _totalDisc.ToString("N2");
                e.Row.Cells[5].Text = _totalTaxable.ToString("N2");
                e.Row.Cells[6].Text = _totalGST.ToString("N2");
                e.Row.Cells[7].Text = _grandTotal.ToString("N2");

                e.Row.Font.Bold = true;

                // ✅ Amount in Words — ONCE, final value
                lblAmountInWords.Text = AmountInWords(_grandTotal);
            }
        }


        private string AmountInWords(decimal amount)
        {
            return "Rupees " + NumberToWords((long)amount) + " Only";
        }

        private string NumberToWords(long number)
        {
            if (number == 0)
                return "Zero";

            if (number < 0)
                return "Minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 10000000) > 0)
            {
                words += NumberToWords(number / 10000000) + " Crore ";
                number %= 10000000;
            }

            if ((number / 100000) > 0)
            {
                words += NumberToWords(number / 100000) + " Lakh ";
                number %= 100000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                string[] unitsMap = { "Zero","One","Two","Three","Four","Five","Six","Seven","Eight","Nine","Ten",
            "Eleven","Twelve","Thirteen","Fourteen","Fifteen","Sixteen","Seventeen","Eighteen","Nineteen" };

                string[] tensMap = { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                    words += tensMap[number / 10] + " " + unitsMap[number % 10];
            }

            return words.Trim();
        }

        private string Safe(object o)
        {
            return o == null ? "" : o.ToString().Trim();
        }

        private void BindCommercialTerms(DataRow hdr, DataTable partyTable)
        {
            /* ===============================
               Rates / Special Rates
               =============================== */

            lblRateRef.Text = LineIfEmpty(Safe(hdr["SpecialRateRef"]));
            lblSpecialRateApprovedBy.Text = LineIfEmpty(Safe(hdr["SpecialRateApprovedBy"]));

            /* ===============================
               Freight Charges
               =============================== */

            lblFreightTerms.Text = NAIfEmpty(Safe(hdr["FreightTerms"]));

            /* ===============================
               Mode of Despatch + Dispatch Upto
               =============================== */

            lblDispatchModeText.Text = NAIfEmpty(Safe(hdr["DispatchMode"]));

            //if (hdr["DispatchUpto"] != DBNull.Value)
            //    lblDispatchUptoText.Text =
            //        Convert.ToDateTime(hdr["DispatchUpto"]).ToString("dd-MMM-yyyy");
            //else
            //    lblDispatchUptoText.Text = "______________";

            // Dispatch Upto (SAFE HANDLING)
            string dispatchUptoRaw = Safe(hdr["DispatchUpto"]);

            DateTime dispatchDate;
            if (!string.IsNullOrWhiteSpace(dispatchUptoRaw) &&
                DateTime.TryParse(dispatchUptoRaw, out dispatchDate))
            {
                lblDispatchUptoText.Text = dispatchDate.ToString("dd-MMM-yyyy");
            }
            else
            {
                lblDispatchUptoText.Text = "______________";
            }

            /* ===============================
               Delivery Basis
               =============================== */

            lblDeliveryBasisText.Text = NAIfEmpty(Safe(hdr["DeliveryBasis"]));

            /* ===============================
               Bill Sent To / LR Sent To
               =============================== */

            string billTo = "";
            string shipTo = "";

            foreach (DataRow r in partyTable.Rows)
            {
                if (r["PartyRole"].ToString() == "BillTo")
                    billTo = Safe(r["Name"]);

                if (r["PartyRole"].ToString() == "ShipTo")
                    shipTo = Safe(r["Name"]);
            }

            lblBillSentTo.Text = NAIfEmpty(billTo);
            lblLRSentTo.Text = NAIfEmpty(shipTo);
        }


        private string LineIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "<span style='display:inline-block; width:140px; border-bottom:1px solid #000;'>&nbsp;</span>"
                : value.Trim();
        }


        private string NAIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "<span style='display:inline-block; min-width:80px; text-align:center;'>NA</span>"
                : value.Trim();
        }

    }
}
