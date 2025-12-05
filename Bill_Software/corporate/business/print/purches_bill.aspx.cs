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
            string Purches_Id = Request.QueryString["Purches_Id"];

            lblpurches_id.Text = Purches_Id.ToString();
            Buindamount();
            DataList1.ItemDataBound += new DataListItemEventHandler(DataList1_ItemDataBound);
            buindalldata();
            

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
                // Get values from current row
                Label lblQuantity = (Label)e.Item.FindControl("Label5");
                Label lblTaxableAmount = (Label)e.Item.FindControl("Label10");
                Label lblTaxAmount = (Label)e.Item.FindControl("Label7");
                Label lblTotalAmount = (Label)e.Item.FindControl("Label14");

                // Add to total variables
                totalQuantity += Convert.ToDecimal(lblQuantity.Text);
                totalTaxableAmount += Convert.ToDecimal(lblTaxableAmount.Text);
                totalTaxAmount += Convert.ToDecimal(lblTaxAmount.Text);
                grandTotal += Convert.ToDecimal(lblTotalAmount.Text);
            }

            // Display total in footer
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

                //lblTaxableValue.Text = totalTaxableAmount.ToString("N2");
                ProdcutTaxes = totalTaxAmount;
                ProdcutTaxable = totalTaxableAmount;
                lblstax0.Text = ProdcutTaxes.ToString("N2");
                //lblnetamount.Text = grandTotal.ToString("N2");
                //lbl_ttl1word.Text = ConvertNumberToWords((int)grandTotal) + " Only";
                lbl_ttl1word.Text = ConvertAmountToWords((long)grandTotal) + " Only";
            }
        }

        public string ConvertNumberToWords(long number)
        {
            if (number == 0)
                return "Zero";

            string words = "";
            if (number < 0)
            {
                words = "Minus ";
                number = -number;
            }

            string[] unitsMap = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
                            "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            string[] tensMap = { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            if ((number / 10000000) > 0)
            {
                words += ConvertNumberToWords(number / 10000000) + " Crore ";
                number %= 10000000;
            }

            if ((number / 100000) > 0)
            {
                words += ConvertNumberToWords(number / 100000) + " Lakh ";
                number %= 100000;
            }

            if ((number / 1000) > 0)
            {
                words += ConvertNumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += ConvertNumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }


        public string ConvertAmountToWords(decimal amount)
        {
            if (amount == 0)
                return "Zero Rupees";

            long integerPart = (long)amount;
            int decimalPart = (int)((amount - integerPart) * 100);

            string words = ConvertNumberToWords(integerPart) + " Rupees";

            if (decimalPart > 0)
            {
                words += " and " + ConvertNumberToWords(decimalPart) + " Paise";
            }

            return words;
        }



        private void buindalldata()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Purches where Purches_Id='" + lblpurches_id.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lbl_invoicedate.Text = re["Purches_date"].ToString();
                Label15.Text = re["BuyerOrderNo"].ToString();
                Label16.Text = re["OrderDate"] != DBNull.Value ? Convert.ToDateTime(re["OrderDate"]).ToString("dd-MM-yyyy") : "N/A";
                lblpurches_date.Text = re["TimeStamp"] != DBNull.Value ? Convert.ToDateTime(re["TimeStamp"]).ToString("dd-MMM-yyyy hh:mm:ss tt") : "N/A";
                //lblpurches_rate.Text = re["Total_purches_rate"].ToString();
                Label10.Text = lblcompanyNameTo.Text = re["ShippedToStoreName"].ToString();

                //Newly added on 22-02-205
                lbl_invoiceno.Text = re["Invoice_No"].ToString();
                lbl_stockaddedon.Text = re["Stock_Add_Date"].ToString();
                lbl_narration.Text = re["Narration"].ToString();

                decimal dtcsAmount = Convert.ToDecimal(re["TCS_Amount"] ?? 0);
                string tcsRateStr = re["TCS_Rate"]?.ToString()?.Trim(); // Get value and trim spaces
                lbl_tcsrate.Text = tcsRateStr.ToString();

                

                decimal dfreightCharges = Convert.ToDecimal(re["Delivery_Amount"] ?? 0);
                string frtRateStr = re["Delivery_Rate"]?.ToString()?.Trim(); // Get value and trim spaces
                lbl_frtrate.Text = frtRateStr.ToString();

                //decimal ftax = dfreightCharges * 0.18m;
                decimal ftax = 0;
                if (string.IsNullOrEmpty(frtRateStr))
                {
                    ftax = dfreightCharges * 18 / 100; // Use 18 as the default rate
                }
                else if (frtRateStr.Equals("NA", StringComparison.OrdinalIgnoreCase))
                {
                    ftax = dfreightCharges * 18 / 100; // Use 18 as the default rate
                }
                else
                {
                    ftax = dfreightCharges * Convert.ToDecimal(frtRateStr) / 100; // Convert and calculate
                }

                lblfttax.Text = ftax.ToString();

                string dotherCharges1name = re["Purches_date"].ToString();

                //--------------------TAXABLE--------------------------------------------//
                lblOtherCharges1name.Text = re["otherAmount1_name"]?.ToString()?.Trim();
                string otherAmount1Str = re["otherAmount1"]?.ToString()?.Trim();

                decimal dotherCharges1 = 0;
                decimal parsedValue = 0;

                if (decimal.TryParse(otherAmount1Str, out parsedValue))
                {
                    dotherCharges1 = parsedValue;
                }

                if (string.IsNullOrEmpty(lblOtherCharges1name.Text) || dotherCharges1 == 0)
                {
                    lblOtherCharges1name.Text = "";  // Hide the label text
                    dotherCharges1 = 0;  // Reset the amount to 0
                }

                lblOtherCharges1.Text = dotherCharges1.ToString("N2");
                decimal dotherCharges1_tax = dotherCharges1 * 18 / 100;
                lbl_othr1_tax.Text = dotherCharges1_tax.ToString("N2");
                //--------------------TAXABLE--------------------------------------------//


                //-------------- NON - TAXABLE--------------------------------------------//
                lblOtherCharges2name.Text = re["otherAmount2_name"]?.ToString()?.Trim();
                string otherAmount2Str = re["otherAmount2"]?.ToString()?.Trim();
                decimal dotherCharges2 = 0;
                decimal parsedValue2 = 0;

                if (decimal.TryParse(otherAmount2Str, out parsedValue2))
                {
                    dotherCharges2 = parsedValue2;
                }

                if (string.IsNullOrEmpty(lblOtherCharges2name.Text) || dotherCharges2 == 0)
                {
                    lblOtherCharges2name.Text = "";  // Hide the label text
                    dotherCharges2 = 0;  // Reset the amount to 0
                }
                lblOtherCharges2.Text = dotherCharges2.ToString("N2");
                //-------------- NON - TAXABLE--------------------------------------------//

                

                decimal ttltax = ProdcutTaxes + ftax + dotherCharges1_tax;
                lbl_ttltax.Text = ttltax.ToString("N2");

                NewTaxable = dfreightCharges + ProdcutTaxable + dotherCharges1;
                lblTaxableValue.Text = NewTaxable.ToString("N2");

                decimal ttl_purchase = NewTaxable + ttltax; 
                lblnetamount.Text = ttl_purchase.ToString("N2");

                

                decimal cal_tcs = Math.Round(Convert.ToDecimal(tcsRateStr) * ttl_purchase / 100, 2);

                if (dtcsAmount == cal_tcs)
                {
                    lblTCSAmount.Text = dtcsAmount.ToString("N2");
                }
                else
                {
                    lblTCSAmount.Text = cal_tcs.ToString("N2");
                    dtcsAmount = cal_tcs;
                }

                decimal total2 = dtcsAmount + dotherCharges2;
                lbl_ttl2amnt.Text = total2.ToString("N2");
                lbl_ttl2word.Text = ConvertAmountToWords((decimal)total2) + " Only";

                // Assign formatted values to labels
                //lblTCSAmount.Text = dtcsAmount.ToString("N2");
                lblFreightCharges.Text = dfreightCharges.ToString("N2");

                //decimal grandttl = ttl_purchase + total2 + dotherCharges2;
                decimal grandttl = ttl_purchase + total2;
                lblGrandTotal.Text = grandttl.ToString("N2");
                lblGrandTotalWord.Text = ConvertAmountToWords((decimal)grandttl) + " Only";

                string type = re["Purches_Type"].ToString();
                if (type == "Product")
                {
                    //labeltax1.Text = "Vat";
                    //labeltax1.Text = "GST";
                }
                else
                {
                    //labeltax1.Text = "Service Tax";
                    //labeltax1.Text = "GST";
                }
                string clientid = re["Client_Id"].ToString();
                Bindclientdetails(clientid);
                

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
                if (lbladdress2.Text == "")
                {
                    lbladdress2.Visible = false;
                }
                else
                {
                    lbladdress2.Visible = true;
                }
                lblcity.Text = re["City"].ToString();
                lblPin.Text = re["pin"].ToString();
                lblstate.Text = re["State"].ToString();
                //lblrepresentativeName.Text = re["Rep_Name"].ToString();
                //lblrepresentativedesignation.Text = re["Rep_Desig"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {

        }

        protected void Button2_Click(object sender, EventArgs e)
        {

        }
    }
}