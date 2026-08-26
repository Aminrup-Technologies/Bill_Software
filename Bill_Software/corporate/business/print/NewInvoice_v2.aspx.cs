using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;

namespace Bill_Software.corporate.business.print
{
    public partial class NewInvoice_v2 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["USERID"] == null || Session["CompanyID"] == null)
                {
                    Response.Write("Session Expired.");
                    Response.End();
                    return;
                }

                if (Request.QueryString["id"] != null)
                {
                    LoadInvoiceData(Request.QueryString["id"].ToString());
                }
            }
        }

        private void LoadInvoiceData(string invId)
        {
            int companyId = Convert.ToInt32(Session["CompanyID"]);
            string connString = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string hdrSql = @"
                    SELECT i.Invoice_No, CONVERT(varchar, i.Invoice_Date, 106) AS InvDate, i.Quotation_No, 
                           i.sub_total, i.Service_Tax1 AS TotalTaxAmt, i.Net_Amount AS NetAmount, 
                           ISNULL(i.Delivery_Amount, 0) AS Freight, ISNULL(i.otherAmount1, 0) AS OtherCharges, 
                           i.BillingAddress, i.cgstOrsgst,
                           c.Client_Name, c.Service_tax_no AS GSTIN, c.State,
                           s.SiteAddress AS ShippingAddress,
                           q.PO_Number, CONVERT(varchar, q.PO_Date, 106) AS PODate, q.DO_Number
                    FROM tbl_Invoice i
                    LEFT JOIN tbl_Client c ON i.Client_ID = c.Client_Id AND c.CompanyID = @CompID
                    LEFT JOIN tbl_InvSiteAddress s ON i.Invoice_No = s.invoice_no AND s.CompanyID = @CompID
                    LEFT JOIN tbl_Quotation q ON i.Quotation_No = q.Quotation_no AND q.CompanyID = @CompID
                    WHERE i.ID = @ID AND i.CompanyID = @CompID";

                string invoiceNo = "";
                decimal netAmount = 0;
                decimal totalTaxable = 0;

                decimal grandTotalCGST = 0;
                decimal grandTotalSGST = 0;
                decimal grandTotalIGST = 0;

                string taxType = "NO"; // YES = CGST/SGST, NO = IGST

                using (SqlCommand cmd = new SqlCommand(hdrSql, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", invId);
                    cmd.Parameters.AddWithValue("@CompID", companyId);
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            invoiceNo = dr["Invoice_No"].ToString();
                            lblInvoiceNo.Text = invoiceNo;
                            lblInvoiceDate.Text = dr["InvDate"].ToString();

                            // Safe file name cleaning for PDF download
                            hdnInvoiceFileName.Value = "TaxInvoice_" + invoiceNo.Replace("/", "_").Replace("\\", "_");

                            string poNum = dr["PO_Number"] != DBNull.Value ? dr["PO_Number"].ToString() : "";
                            string doNum = dr["DO_Number"] != DBNull.Value ? dr["DO_Number"].ToString() : "";
                            lblPODONo.Text = !string.IsNullOrWhiteSpace(poNum) ? poNum : (!string.IsNullOrWhiteSpace(doNum) ? doNum : "N/A");

                            string poDate = dr["PODate"] != DBNull.Value ? dr["PODate"].ToString() : "";
                            lblPODate.Text = !string.IsNullOrWhiteSpace(poDate) ? $"[{poDate}]" : "";

                            lblQuoteRef.Text = dr["Quotation_No"] != DBNull.Value ? dr["Quotation_No"].ToString() : "N/A";
                            lblClientName.Text = dr["Client_Name"].ToString();

                            string gstin = dr["GSTIN"] != DBNull.Value ? dr["GSTIN"].ToString().Trim() : "";
                            lblGSTIN.Text = !string.IsNullOrWhiteSpace(gstin) ? gstin : "N/A";
                            lblStateCode.Text = gstin.Length >= 2 ? gstin.Substring(0, 2) : "N/A";
                            lblPOS.Text = dr["State"] != DBNull.Value ? dr["State"].ToString() : "N/A";

                            string billAddr = dr["BillingAddress"] != DBNull.Value ? dr["BillingAddress"].ToString() : "";
                            lblBillingAddress.Text = !string.IsNullOrWhiteSpace(billAddr) && billAddr != "N/A" ? billAddr : "Registered Address Used";

                            string shipAddr = dr["ShippingAddress"] != DBNull.Value ? dr["ShippingAddress"].ToString() : "";
                            lblShippingAddress.Text = !string.IsNullOrWhiteSpace(shipAddr) ? shipAddr : lblBillingAddress.Text;

                            netAmount = Convert.ToDecimal(dr["NetAmount"]);
                            taxType = dr["cgstOrsgst"] != DBNull.Value ? dr["cgstOrsgst"].ToString().ToUpper() : "NO";

                            decimal freightOther = Convert.ToDecimal(dr["Freight"]) + Convert.ToDecimal(dr["OtherCharges"]);
                            lblFreight.Text = freightOther.ToString("F2");
                            lblGrandTotal.Text = netAmount.ToString("F2");

                            hdnQRPayload.Value = $"GSTIN:{lblGSTIN.Text}|INV:{invoiceNo}|DT:{lblInvoiceDate.Text}|VAL:{netAmount}";
                        }
                        else
                        {
                            Response.Write("Invoice Not Found.");
                            Response.End();
                            return;
                        }
                    }
                }

                // LINE ITEMS FETCH & COMPREHENSIVE 15-COLUMN ENGINE
                StringBuilder sbItems = new StringBuilder();
                int slNo = 1;

                string itemSql = @"
                    SELECT Product_name, Product_Code AS HSN, 
                           CAST(Quantity AS DECIMAL(18,2)) AS Qty,
                           CAST(sail_rate AS DECIMAL(18,2)) AS Rate,
                           CAST(Total_sail_rate2 AS DECIMAL(18,2)) AS TaxableAmt,
                           CAST(Service_tax_rate AS DECIMAL(18,2)) AS GSTRate,
                           CAST(Total_sail_rate1 AS DECIMAL(18,2)) AS NetAmt
                    FROM tbl_Invoice_details
                    WHERE Invoice_No = @InvNo AND CompanyID = @CompID
                    ORDER BY ID ASC";

                using (SqlCommand cmdItem = new SqlCommand(itemSql, conn))
                {
                    cmdItem.Parameters.AddWithValue("@InvNo", invoiceNo);
                    cmdItem.Parameters.AddWithValue("@CompID", companyId);

                    using (SqlDataReader drItem = cmdItem.ExecuteReader())
                    {
                        while (drItem.Read())
                        {
                            decimal qty = Convert.ToDecimal(drItem["Qty"]);
                            decimal rate = Convert.ToDecimal(drItem["Rate"]);
                            decimal taxable = Convert.ToDecimal(drItem["TaxableAmt"]);
                            decimal gstRate = Convert.ToDecimal(drItem["GSTRate"]);
                            decimal netRow = Convert.ToDecimal(drItem["NetAmt"]);

                            // Precise calculation of line components
                            decimal grossBeforeDisc = qty * rate;
                            decimal discAmt = grossBeforeDisc - taxable;
                            decimal rowTaxTotal = netRow - taxable;

                            totalTaxable += taxable;

                            // Table Cells mapped directly to CGST/SGST/IGST headers
                            string cgstPer = "-", cgstAmt = "-", sgstPer = "-", sgstAmt = "-", igstPer = "-", igstAmt = "-";

                            if (taxType == "YES")
                            {
                                decimal halfRate = gstRate / 2;
                                decimal halfTax = rowTaxTotal / 2;

                                grandTotalCGST += halfTax;
                                grandTotalSGST += halfTax;

                                cgstPer = halfRate.ToString("F1") + "%";
                                cgstAmt = halfTax.ToString("F2");
                                sgstPer = halfRate.ToString("F1") + "%";
                                sgstAmt = halfTax.ToString("F2");
                            }
                            else
                            {
                                grandTotalIGST += rowTaxTotal;

                                igstPer = gstRate.ToString("F1") + "%";
                                igstAmt = rowTaxTotal.ToString("F2");
                            }

                            sbItems.Append("<tr>");
                            sbItems.Append($"<td class='text-center'>{slNo++}</td>");
                            sbItems.Append($"<td><span class='bold'>{drItem["Product_name"]}</span></td>");
                            sbItems.Append($"<td class='text-center'>{drItem["HSN"]}</td>");
                            sbItems.Append($"<td class='text-center'>{qty.ToString("F2")}</td>");
                            sbItems.Append($"<td class='text-right'>{rate.ToString("F2")}</td>");
                            sbItems.Append($"<td class='text-right'>{grossBeforeDisc.ToString("F2")}</td>");
                            sbItems.Append($"<td class='text-right'>{discAmt.ToString("F2")}</td>");
                            sbItems.Append($"<td class='text-right bold'>{taxable.ToString("F2")}</td>");

                            // Splitting the 6 discrete Tax columns based on the thead
                            sbItems.Append($"<td class='text-center'>{cgstPer}</td><td class='text-right'>{cgstAmt}</td>");
                            sbItems.Append($"<td class='text-center'>{sgstPer}</td><td class='text-right'>{sgstAmt}</td>");
                            sbItems.Append($"<td class='text-center'>{igstPer}</td><td class='text-right'>{igstAmt}</td>");

                            sbItems.Append($"<td class='text-right bold'>{netRow.ToString("F2")}</td>");
                            sbItems.Append("</tr>");
                        }
                    }
                }

                litInvoiceItems.Text = sbItems.ToString();
                lblTotalTaxable.Text = totalTaxable.ToString("F2");

                // Footer Tax Breakdown
                StringBuilder sbTaxes = new StringBuilder();
                if (taxType == "YES")
                {
                    sbTaxes.Append($"<tr><td class='bold' style='padding:4px; border-bottom:1px solid #ccc;'>TOTAL CGST:</td><td class='text-right bold' style='padding:4px; border-bottom:1px solid #ccc;'>{grandTotalCGST.ToString("F2")}</td></tr>");
                    sbTaxes.Append($"<tr><td class='bold' style='padding:4px; border-bottom:1px solid #ccc;'>TOTAL SGST:</td><td class='text-right bold' style='padding:4px; border-bottom:1px solid #ccc;'>{grandTotalSGST.ToString("F2")}</td></tr>");
                }
                else
                {
                    sbTaxes.Append($"<tr><td class='bold' style='padding:4px; border-bottom:1px solid #ccc;'>TOTAL IGST:</td><td class='text-right bold' style='padding:4px; border-bottom:1px solid #ccc;'>{grandTotalIGST.ToString("F2")}</td></tr>");
                }
                litTaxBreakdown.Text = sbTaxes.ToString();

                // Accurate Rupees and Paise Math
                decimal netAmountDec = Math.Round(netAmount, 2);
                long rupees = (long)Math.Floor(netAmountDec);
                int paise = (int)Math.Round((netAmountDec - rupees) * 100);

                string words = "Rupees " + ConvertToWordsIndian(rupees);
                if (paise > 0)
                {
                    words += " and " + ConvertToWordsIndian(paise) + " Paise";
                }
                words += " Only.";

                lblAmountInWords.Text = words;
            }
        }

        #region INDIAN RUPEE NUMBER TO WORDS CONVERTER
        private string ConvertToWordsIndian(long number)
        {
            if (number == 0) return "Zero";
            if (number < 0) return "Minus " + ConvertToWordsIndian(Math.Abs(number));

            string words = "";
            if ((number / 10000000) > 0) { words += ConvertToWordsIndian(number / 10000000) + " Crore "; number %= 10000000; }
            if ((number / 100000) > 0) { words += ConvertToWordsIndian(number / 100000) + " Lakh "; number %= 100000; }
            if ((number / 1000) > 0) { words += ConvertToWordsIndian(number / 1000) + " Thousand "; number %= 1000; }
            if ((number / 100) > 0) { words += ConvertToWordsIndian(number / 100) + " Hundred "; number %= 100; }

            if (number > 0)
            {
                if (words != "") words += " ";
                var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

                if (number < 20) words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0) words += " " + unitsMap[number % 10];
                }
            }
            return words;
        }
        #endregion
    }
}