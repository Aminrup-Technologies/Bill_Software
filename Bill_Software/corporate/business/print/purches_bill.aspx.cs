using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.print
{
    public partial class purches_bill : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        public decimal totalQuantity = 0;
        public decimal totalTaxableAmount = 0;
        public decimal totalTaxAmount = 0;
        public decimal grandTotal = 0;
        public decimal ProdcutTaxes = 0;
        public decimal ProdcutTaxable = 0;

        public decimal NewTaxable = 0;
        public decimal NewTax = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                pnlContent.Visible = true;
                pnlError.Visible = false;

                string Purches_Id = Request.QueryString["Purches_Id"];
                if (!string.IsNullOrEmpty(Purches_Id))
                {
                    lblpurches_id.Text = Purches_Id.ToString();
                    Buindamount();
                    DataList1.ItemDataBound -= new DataListItemEventHandler(DataList1_ItemDataBound); // Avoid double binding
                    DataList1.ItemDataBound += new DataListItemEventHandler(DataList1_ItemDataBound);
                    buindalldata();
                }
            }
            catch (Exception ex)
            {
                // BLOCK CONTENT IF CRASH
                pnlContent.Visible = false;
                pnlError.Visible = true;
                lblErrorMsg.Text = "System Error: " + ex.Message;
            }
        }

        private void Buindamount()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select sl_no, Product_id, (Product_name+':'+specification) as Product_name, Quantity, vendor_rate, purches_rate, DiscountPercent, DiscountAmount,TaxableAmount, tax_rate, vat_amount, total_purches_rate from tbl_purches_details where Purches_id='" + lblpurches_id.Text + "' order by CAST(Sl_no as int)";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        protected void DataList1_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Label lblQuantity = (Label)e.Item.FindControl("Label5");
                Label lblTaxableAmount = (Label)e.Item.FindControl("Label10");
                Label lblTaxAmount = (Label)e.Item.FindControl("Label7");
                Label lblTotalAmount = (Label)e.Item.FindControl("Label14");

                decimal qty = 0, taxable = 0, tax = 0, total = 0;
                decimal.TryParse(lblQuantity.Text, out qty);
                decimal.TryParse(lblTaxableAmount.Text, out taxable);
                decimal.TryParse(lblTaxAmount.Text, out tax);
                decimal.TryParse(lblTotalAmount.Text, out total);

                totalQuantity += qty;
                totalTaxableAmount += taxable;
                totalTaxAmount += tax;
                grandTotal += total;
            }

            if (e.Item.ItemType == ListItemType.Footer)
            {
                Label lblTotalQuantity = (Label)e.Item.FindControl("lblTotalQuantity");
                Label lblTotalTaxableAmount = (Label)e.Item.FindControl("lblTotalTaxableAmount");
                Label lblTotalTaxAmount = (Label)e.Item.FindControl("lblTotalTaxAmount");
                Label lblGrandTotal = (Label)e.Item.FindControl("lblGrandTotal");

                lblTotalQuantity.Text = totalQuantity.ToString("N2");
                lblTotalTaxableAmount.Text = totalTaxableAmount.ToString("N2");
                lblTotalTaxAmount.Text = totalTaxAmount.ToString("N2");
                lblGrandTotal.Text = grandTotal.ToString("N2");

                ProdcutTaxes = totalTaxAmount;
                ProdcutTaxable = totalTaxableAmount;
                lblstax0.Text = ProdcutTaxes.ToString("N2");
            }
        }

        private void buindalldata()
        {
            try
            {
                DbCL.Sqlconnection();
                DbCL.ConnectDb();

                // Re-aligned query joining with Stores to get the 'To' address
                string cmdstring = @"SELECT p.*, s.StoreAddress, s.StoreCity, s.StorePin 
                            FROM tbl_Purches p 
                            LEFT JOIN Stores s ON p.ShippedToStoreId = s.StoreId 
                            WHERE p.Purches_Id = @pid";

                SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
                cmd.Parameters.AddWithValue("@pid", lblpurches_id.Text.Trim());
                SqlDataReader re = cmd.ExecuteReader();

                if (re.Read())
                {
                    // --- 1. INVOICE & REFERENCE DETAILS ---
                    lbl_invoicedate.Text = re["Purches_date"].ToString();
                    lbl_invoiceno.Text = re["Invoice_No"].ToString();
                    lbl_stockaddedon.Text = re["Stock_Add_Date"].ToString();
                    lbl_narration.Text = re["Narration"].ToString();

                    // Buyer Order Info
                    Label15.Text = re["BuyerOrderNo"]?.ToString() ?? "N/A";
                    Label16.Text = re["OrderDate"] != DBNull.Value ? re["OrderDate"].ToString() : "N/A";
                    lblpurches_date.Text = re["TimeStamp"] != DBNull.Value ?
                        Convert.ToDateTime(re["TimeStamp"]).ToString("dd-MMM-yyyy hh:mm tt") : "N/A";

                    // --- 2. 'TO' ADDRESS (Your Store) ---
                    lblcompanyNameTo.Text = re["ShippedToStoreName"].ToString();
                    Label10.Text = re["ShippedToStoreName"].ToString(); // Secondary label in tableOne
                    lbladdress1To.Text = re["StoreAddress"]?.ToString() ?? "";
                    lblcityTo.Text = re["StoreCity"]?.ToString() ?? "";
                    lblPinTo.Text = re["StorePin"]?.ToString() ?? "";
                    // If you have a state column in Stores table, map it here:
                    // lblstateTo.Text = re["StoreState"]?.ToString() ?? "";

                    // --- 3. FREIGHT / DELIVERY ---
                    decimal dfreightCharges = 0;
                    decimal.TryParse(re["Delivery_Amount"]?.ToString(), out dfreightCharges);
                    lblFreightCharges.Text = dfreightCharges.ToString("N2");

                    string frtRateStr = re["Delivery_Rate"]?.ToString()?.Trim();
                    decimal frtRate = 0;
                    if (!string.IsNullOrEmpty(frtRateStr) && !frtRateStr.Equals("NA", StringComparison.OrdinalIgnoreCase))
                        decimal.TryParse(frtRateStr, out frtRate);

                    lbl_frtrate.Text = frtRate.ToString("0.##");
                    decimal ftax = (dfreightCharges * frtRate) / 100;
                    lblfttax.Text = ftax.ToString("N2");

                    // --- 4. OTHER CHARGE 1 (Taxable) ---
                    lblOtherCharges1name.Text = re["otherAmount1_name"]?.ToString()?.Trim();
                    if (string.IsNullOrEmpty(lblOtherCharges1name.Text)) lblOtherCharges1name.Text = "Other Charge 1";

                    decimal dotherCharges1 = 0;
                    decimal.TryParse(re["otherAmount1"]?.ToString(), out dotherCharges1);
                    lblOtherCharges1.Text = dotherCharges1.ToString("N2");

                    // Assuming 18% GST for taxable other charges
                    decimal dotherCharges1_tax = dotherCharges1 * 18 / 100;
                    lbl_othr1_tax.Text = dotherCharges1_tax.ToString("N2");

                    // --- 5. OTHER CHARGE 2 (Non-Taxable) ---
                    lblOtherCharges2name.Text = re["otherAmount2_name"]?.ToString()?.Trim();
                    if (string.IsNullOrEmpty(lblOtherCharges2name.Text)) lblOtherCharges2name.Text = "Other Charge 2";

                    decimal dotherCharges2 = 0;
                    decimal.TryParse(re["otherAmount2"]?.ToString(), out dotherCharges2);
                    lblOtherCharges2.Text = dotherCharges2.ToString("N2");

                    // --- 6. TOTAL CALCULATIONS ---
                    // Sum of Tax: Product Tax + Freight Tax + Other1 Tax
                    decimal ttltax = ProdcutTaxes + ftax + dotherCharges1_tax;
                    lbl_ttltax.Text = ttltax.ToString("N2");

                    // Taxable Value: Product Taxable + Freight Amt + Other1 Amt
                    NewTaxable = ProdcutTaxable + dfreightCharges + dotherCharges1;
                    lblTaxableValue.Text = NewTaxable.ToString("N2");

                    // Net Amount (Total Purchase)
                    decimal ttl_purchase = NewTaxable + ttltax;
                    lblnetamount.Text = ttl_purchase.ToString("N2");
                    lbl_ttl1word.Text = ConvertAmountToWords(ttl_purchase) + " Only";

                    // --- 7. TCS & FINAL GRAND TOTAL ---
                    string tcsRateStr = re["TCS_Rate"]?.ToString()?.Trim();
                    decimal tcsRate = 0; decimal.TryParse(tcsRateStr, out tcsRate);
                    lbl_tcsrate.Text = tcsRate.ToString("0.##");

                    decimal dtcsAmount = 0;
                    decimal.TryParse(re["TCS_Amount"]?.ToString(), out dtcsAmount);
                    // Fallback: Recalculate if amount is 0 but rate exists
                    if (dtcsAmount == 0 && tcsRate > 0) dtcsAmount = Math.Round((ttl_purchase * tcsRate) / 100, 2);
                    lblTCSAmount.Text = dtcsAmount.ToString("N2");

                    decimal total2 = dtcsAmount + dotherCharges2;
                    lbl_ttl2amnt.Text = total2.ToString("N2");
                    lbl_ttl2word.Text = ConvertAmountToWords(total2) + " Only";

                    decimal grandttl = ttl_purchase + total2;
                    lblGrandTotalMain.Text = grandttl.ToString("N2");
                    lblGrandTotalWord.Text = ConvertAmountToWords(grandttl) + " Only";

                    // --- 8. VENDOR DETAILS ---
                    Bindclientdetails(re["Client_Id"].ToString());
                }
            }
            catch (Exception ex)
            {
                // Handle error (e.g., Log it)
                throw ex;
            }
            finally
            {
                if (DbCL.Conn.State == System.Data.ConnectionState.Open)
                    DbCL.Conn.Close();
            }
        }

        private void Bindclientdetails(string clientid)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Vendor where Vendor_Id='" + clientid + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblcompanyName.Text = re["Vendor_Name"].ToString();
                lbladdress1.Text = re["Address1"].ToString();
                lbladdress2.Text = re["Address2"].ToString();
                lbladdress2.Visible = !string.IsNullOrEmpty(lbladdress2.Text);
                lblcity.Text = re["City"].ToString();
                lblPin.Text = re["pin"].ToString();
                lblstate.Text = re["State"].ToString();
            }
            DbCL.Conn.Close();
        }

        public string ConvertNumberToWords(long number)
        {
            if (number == 0) return "Zero";
            if (number < 0) return "Minus " + ConvertNumberToWords(Math.Abs(number));

            string words = "";

            // Indian Numbering System: Crore (1,00,00,000)
            if ((number / 10000000) > 0)
            {
                words += ConvertNumberToWords(number / 10000000) + " Crore ";
                number %= 10000000;
            }

            // Lakh (1,00,000)
            if ((number / 100000) > 0)
            {
                words += ConvertNumberToWords(number / 100000) + " Lakh ";
                number %= 100000;
            }

            // Thousand (1,000)
            if ((number / 1000) > 0)
            {
                words += ConvertNumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            // Hundred (100)
            if ((number / 100) > 0)
            {
                words += ConvertNumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "") words += "and ";

                string[] unitsMap = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                string[] tensMap = { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += " " + unitsMap[number % 10]; // Changed '-' to space for cleaner look
                }
            }

            return words.Trim();
        }

        public string ConvertAmountToWords(decimal amount)
        {
            long integerPart = (long)Math.Floor(amount);
            int decimalPart = (int)Math.Round((amount - integerPart) * 100);

            string words = ConvertNumberToWords(integerPart) + " Rupees";

            if (decimalPart > 0)
            {
                words += " and " + ConvertNumberToWords(decimalPart) + " Paise";
            }
            return words;
        }

        protected void Button1_Click(object sender, EventArgs e) { }
        protected void Button2_Click(object sender, EventArgs e) { }
    }
}