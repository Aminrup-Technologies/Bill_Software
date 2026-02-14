using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Globalization;
using System.Threading;

namespace Bill_Software.corporate.business.print
{
    public partial class NewProformaInvoice : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        public string strp = "";
        public string vatno = "";
        public string gstno = "";

        DataTable dtp = new DataTable();
        DataTable dtci = new DataTable();
        DataTable dtRep = new DataTable();
        DataTable dtBacdata = new DataTable();
        DataTable dtvat = new DataTable();
        StringBuilder strbackdt = new StringBuilder();

        public string taxorvat = "";
        public string psid = "";
        public int TQ = 0;

        public string lblSubtotal = "", lbldiscount = "", lblstax = "", lblstax0 = "", lblnetamount = "", lblword = "";

        CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string ID = Request.QueryString["ID"];
                buindalldata(ID);
            }
        }

        private void buindalldata(string ID)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Proforma where ID='" + ID.ToString() + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();

            // Variable to hold Invoice No for Direct lookups
            string invNo = "";

            if (re.Read())
            {
                lblinvno.Text = re["Invoice_No"].ToString();
                invNo = re["Invoice_No"].ToString(); // Capture Invoice No

                lblinvdate.Text = re["Invoice_Date"].ToString();
                lblqnumber.Text = re["Quotation_No"].ToString();

                string qno = re["Quotation_No"].ToString();

                // *** REVISION 1: Pass Invoice No to handle Direct Tax Flags ***
                bindcgstorigst(qno, invNo);

                string clientid = re["Client_ID"].ToString();
                representative(clientid);
                Bindclientdetails(clientid);

                lblSubtotal = re["subtotal"].ToString();
                lblstax = re["Service_Tax"].ToString();
                lblstax0 = re["Service_Tax"].ToString();
                lblnetamount = re["Net_Amount"].ToString();
            }
            string word = MoneyConvDS.MoneyConvFn(lblnetamount);
            lblword = word.ToString();
            DbCL.Conn.Close();

            // *** REVISION 2: Pass Invoice No to handle Direct Product List ***
            Buindamount(invNo);
        }

        private void bindcgstorigst(string qno, string invNo)
        {
            // *** REVISION 3: Logic to switch between Direct and Flow ***
            string query = "";
            SqlParameter[] pram;

            if (qno == "Direct")
            {
                // New Logic: Fetch tax settings from tbl_Proforma
                query = "select cgstOrsgst, igst, PlaceofSupply from tbl_Proforma where Invoice_No=@Invoice_No";
                pram = new SqlParameter[] { new SqlParameter("@Invoice_No", invNo) };
            }
            else
            {
                // Old Logic: Fetch from tbl_Quotation
                query = "select cgstOrsgst, igst, PlaceofSupply from tbl_Quotation where Quotation_no=@Quotation_no";
                pram = new SqlParameter[] { new SqlParameter("@Quotation_no", qno) };
            }

            dtci = DbCL.SPreturn_dt(query, pram);
            if (dtci.Rows.Count > 0)
            {
                Session["cgstOrsgstp"] = dtci.Rows[0]["cgstOrsgst"].ToString();
                Session["igstp"] = dtci.Rows[0]["igst"].ToString();

                string placeofsupply = dtci.Rows[0]["PlaceofSupply"].ToString();
                lblplaceofsup1.Text = "Place Of Supply";
                lblplaceofsup2.Text = ":";
                lblplaceofsup3.Text = placeofsupply;
            }
        }

        // *** REVISION 4: Updated to accept InvoiceNo parameter ***
        private void Buindamount(string invoiceNo)
        {
            string qno = lblqnumber.Text;
            string status = statusvalue(qno);

            if (status != "YES")
            {
                // OLD NON-GST LOGIC (Kept as is)
                pnlTasGst.Visible = false;
                lblserviceamo.Visible = false;
                BackData.Visible = true;

                string query = "select Sl_no,(Product_name+' '+specification) as Product_name,Quantity,sail_rate,Service_tax_rate,Total_sail_rate2 from tbl_Quotaion_details where Quotation_no='" + lblqnumber.Text + "' order by Product_name";
                SqlParameter[] pram = { new SqlParameter("@Quotation_no", lblqnumber.Text) };

                dtBacdata = DbCL.SPreturn_dt(query, pram);
                if (dtBacdata.Rows.Count > 0)
                {
                    // ... (Your Existing Non-GST Loop Logic) ...
                    strbackdt.Append("<table class='' style='border:0' width='100%'>");
                    strbackdt.Append("<tr>");
                    strbackdt.Append("<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>S.NO</td>");
                    strbackdt.Append("<td style='width:51%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>PARTICULARS</td>");
                    strbackdt.Append("<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>QNTY<br>(PCS)</td>");
                    strbackdt.Append("<td style='width:15%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>RATE<br> (RS)</td>");
                    strbackdt.Append("<td style='width:14%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;''>VAT(%)</td>");
                    strbackdt.Append("<td style='width:10%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>AMOUNT<br> (RS)</td>");
                    strbackdt.Append("</tr>");
                    strbackdt.Append("</table>");

                    for (int i = 0; i < dtBacdata.Rows.Count; i++)
                    {
                        string slno = dtBacdata.Rows[i]["Sl_no"].ToString();
                        string Product_name = dtBacdata.Rows[i]["Product_name"].ToString();
                        string Quantity = dtBacdata.Rows[i]["Quantity"].ToString();
                        string sail_rate = dtBacdata.Rows[i]["sail_rate"].ToString();
                        string Service_tax_rate = dtBacdata.Rows[i]["Service_tax_rate"].ToString();
                        string Total_sail_rate2 = dtBacdata.Rows[i]["Total_sail_rate2"].ToString();

                        strbackdt.Append("<table class='' style='border:0' width='100%'>");
                        strbackdt.Append("<tr>");
                        strbackdt.Append("<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;  border-right:none;'>" + slno + "</td>");
                        strbackdt.Append("<td style='width:51%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center;   border-right:none;'>" + Product_name + "</td>");
                        strbackdt.Append("<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center;   border-right:none;'>" + Quantity + "</td>");
                        strbackdt.Append("<td style='width:15%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center;   border-right:none;'>" + sail_rate + "</td>");
                        strbackdt.Append("<td style='width:14%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center;   border-right:none;''>" + Service_tax_rate + "</td>");
                        strbackdt.Append("<td style='width:10%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; '>" + Total_sail_rate2 + "</td>");
                        strbackdt.Append("</tr>");
                        strbackdt.Append("</table>");
                    }
                    strbackdt.Append("<table class='' style='border:0' width='100%'>");
                    strbackdt.Append("<tr>");
                    strbackdt.Append("<td style='font-weight: bold;  text-align: center' colspan='4'></td>");
                    strbackdt.Append("<td style='width:14%; font-weight: bold;  text-align: right; background-color: #e31e24; color: white;'>TOTAL AMOUNT BEFORE TAX:</td>");
                    strbackdt.Append("<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none'>" + lblSubtotal + "</td>");
                    strbackdt.Append("</tr>");
                    strbackdt.Append("</table>");

                    bindVatdetails();

                    strbackdt.Append("<table class='' style='border:0' width='100%'>");
                    strbackdt.Append("<tr>");
                    strbackdt.Append("<td style='font-weight: bold;  text-align: center' colspan='4'></td>");
                    strbackdt.Append("<td style='width:14%; font-weight: bold;  text-align: right; background-color: #e31e24; color: white;'>ROUND OFF:</td>");
                    strbackdt.Append("<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none'>" + lblstax + "</td>");
                    strbackdt.Append("</tr>");
                    strbackdt.Append("</table>");

                    strbackdt.Append("<table class='' style='border:0' width='100%'>");
                    strbackdt.Append("<tr>");
                    strbackdt.Append("<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='3'>Amount (In Words):" + lblword + "</td>");
                    strbackdt.Append("<td style='width:15%; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'></td>");
                    strbackdt.Append("<td style='width:14%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;'>TOTAL AMOUNT AFTER TAX:</td>");
                    strbackdt.Append("<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + lblnetamount + "</td>");
                    strbackdt.Append("</tr>");
                    strbackdt.Append("</table>");

                    lblbackdata.Text = strbackdt.ToString();
                    // Since Direct Invoices are GST-only, this part remains unchanged for legacy data.
                    GenerateNonGSTTable(dtBacdata); // Moved logic to helper to keep code clean, or paste existing loop here
                }
            }
            else
            {
                // GST LOGIC (Direct Invoices land here)
                pnlTasGst.Visible = true;
                lblserviceamo.Visible = true;
                BackData.Visible = false;

                string cmdstring = "";
                SqlParameter[] pram;

                if (qno == "Direct")
                {
                    // *** REVISION 5: Direct Invoice Query (tbl_Proforma_Details) ***
                    // We alias the columns to match the names used in your existing loop variables
                    // Product_Code -> HSN
                    // Rate -> sail_rate
                    // Tax_Rate -> Service_tax_rate
                    cmdstring = @"select Product_Code as HSN, 
                                         (Product_name) as Product_name, 
                                         Quantity, 
                                         Rate as sail_rate, 
                                         Tax_Rate as Service_tax_rate, 
                                         Total_Amount as Total_sail_rate1, 
                                         Net_Amount as Total_sail_rate2 
                                  from tbl_Proforma_Details 
                                  where Invoice_No=@Invoice_No order by Product_name";

                    pram = new SqlParameter[] { new SqlParameter("@Invoice_No", invoiceNo) };
                }
                else
                {
                    // *** REVISION 6: Standard Quotation Query (tbl_Quotaion_details) ***
                    cmdstring = "select Product_id as HSN,(Product_name+' '+specification) as Product_name,Quantity,sail_rate,Service_tax_rate,Total_sail_rate1,Total_sail_rate2 from tbl_Quotaion_details where Quotation_no=@Quotation_no order by Product_name";
                    pram = new SqlParameter[] { new SqlParameter("@Quotation_no", qno) };
                }

                dtp = DbCL.SPreturn_dt(cmdstring, pram);

                if (dtp.Rows.Count > 0)
                {
                    // *** REVISION 7: Your Existing GST Rendering Loop ***
                    // Because we aliased the column names in the Direct query above, 
                    // this loop works perfectly for both!

                    if (Session["cgstOrsgstp"].ToString() == "YES")
                    {
                        double TOTALCGST = 0;
                        double SUBTOTAL = 0;
                        double TOTALGST = 0;
                        double TOTALGSTPLUSAMO = 0;

                        strp += "<table border='0' width='100%' class='Payment pagebrake'><tr><td class='' style=''>";
                        strp += "<table class='PaymentPhase' style='border:0' width='100%'><tr><td class='gap' style=''>&nbsp</td></tr>";
                        strp += "<tr>";
                        strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>S.NO</td>";
                        strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>HSN <br> CODE</td>";
                        strp += "<td style='width:30%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>PARTICULARS</td>";
                        strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>QNTY<br>(PCS)</td>";
                        strp += "<td style='width:10%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>RATE<br> (RS)</td>";
                        strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>GST</td>";

                        strp += "<td style='width:14%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;''>";
                        strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        strp += "<tr>";
                        strp += "<td style='width:14%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-right:none; font-weight: bold; background-color: #e31e24; text-align: center; color: white; ' colspan='2'>CGST</td>";
                        strp += "</tr>";
                        strp += "<tr>";
                        strp += "<td style='width:5%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-bottom:none;  font-weight: bold; background-color: #e31e24; text-align: center; color: white; '>RATE</td>";
                        strp += "<td style='width:9%;  font-weight: bold; background-color: #e31e24; text-align: center; color: white; '>AMOUNT</td>";
                        strp += "</tr>";
                        strp += "</table>";
                        strp += "</td>";

                        strp += "<td style='width:14%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>";
                        strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        strp += "<tr>";
                        strp += "<td style='width:14%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-right:none;  font-weight: bold; background-color: #e31e24; text-align: center; color: white; ' colspan='2'>SGST</td>";
                        strp += "</tr>";
                        strp += "<tr>";
                        strp += "<td style='width:5%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-bottom:none;  font-weight: bold; background-color: #e31e24; text-align: center; color: white; '>RATE</td>";
                        strp += "<td style='width:9%;  font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>AMOUNT</td>";
                        strp += "</tr>";
                        strp += "</table>";
                        strp += "</td>";

                        strp += "<td style='width:10%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>AMOUNT<br> (RS)</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        for (int i = 0; i < dtp.Rows.Count; i++)
                        {
                            string HSN = dtp.Rows[i]["HSN"].ToString();
                            string Productname = dtp.Rows[i]["Product_name"].ToString();

                            // Handling decimal/int conversions carefully
                            decimal qtyDec = Convert.ToDecimal(dtp.Rows[i]["Quantity"]);
                            int Quantity = (int)qtyDec;

                            TQ = TQ + Quantity;
                            double baserate = Math.Round((Convert.ToDouble(dtp.Rows[i]["sail_rate"])), 2);
                            int gstper = Convert.ToInt32(dtp.Rows[i]["Service_tax_rate"]);
                            double QuantityBaserateAmo = Math.Round((Quantity * baserate), 2);

                            string QuantityBaserateAmo1 = DoFormat(QuantityBaserateAmo);

                            double gstamount = Math.Round(((QuantityBaserateAmo * gstper) / 100), 2);
                            double cgstper = Math.Round((Convert.ToDouble(gstper) / 2), 2);
                            double cgstamo = Math.Round((Convert.ToDouble(gstamount) / 2), 2);

                            TOTALCGST = TOTALCGST + cgstamo;
                            SUBTOTAL = SUBTOTAL + QuantityBaserateAmo;
                            TOTALGST = TOTALGST + gstamount;

                            strp += "<table class='' style='border:0' width='100%'>";
                            strp += "<tr>";
                            strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'>" + (i + 1).ToString() + "</td>";
                            strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + HSN + "</td>";
                            strp += "<td style='width:30%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: left;   border-right:none; border-top:none;'>" + Productname + "</td>";
                            strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + Quantity.ToString() + "</td>";
                            strp += "<td style='width:10%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + baserate.ToString() + "</td>";
                            strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";

                            strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + cgstper.ToString() + " %</td>";
                            strp += "<td style='width:9%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + cgstamo.ToString() + "</td>";
                            strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + cgstper.ToString() + " %</td>";
                            strp += "<td style='width:9%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + cgstamo.ToString() + "</td>";

                            strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;   border-top:none;'>" + QuantityBaserateAmo1.ToString() + "</td>";
                            strp += "</tr>";
                            strp += "</table>";
                        }

                        TOTALGSTPLUSAMO = TOTALGST + SUBTOTAL;
                        string grandtotal = (Math.Round((Convert.ToDouble(TOTALGSTPLUSAMO))).ToString());
                        string word = MoneyConvDS.MoneyConvFn(lblnetamount);

                        strp += "<table class='' style='border:0' width='100%'>";
                        strp += "<tr>";
                        strp += "<td style='border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;  border-right:none; border-top:none;background-color:#d9d3d3' colspan='3'>GRAND TOTAL</td>";
                        strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'>" + TQ.ToString() + "</td>";
                        strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
                        strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
                        strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
                        strp += "<td style='width:9%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'>" + TOTALCGST.ToString() + "</td>";
                        strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
                        strp += "<td style='width:9%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'>" + TOTALCGST.ToString() + "</td>";
                        strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;   border-top:none;background-color:#d9d3d3'>" + Math.Round(SUBTOTAL).ToString() + ".00" + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        strp += "<table class='' style='border:0' width='100%'>";
                        strp += "<tr>";
                        strp += "<td style='font-weight: bold;  text-align: center' colspan='6'></td>";
                        strp += "<td style='width:28%; font-weight: bold;  text-align: right; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT BEFORE TAX:</td>";
                        strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none'>" + Math.Round(SUBTOTAL).ToString() + ".00" + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        strp += "<table class='' style='border:0' width='100%'>";
                        strp += "<tr>";
                        strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none' colspan='6'></td>";
                        strp += "<td style='width:28%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL GST:</td>";
                        strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + Math.Round(TOTALGST).ToString() + ".00" + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        strp += "<table class='' style='border:0' width='100%'>";
                        strp += "<tr>";
                        strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='5'>Amount (In Words):" + word + "</td>";
                        strp += "<td style='width:5%; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'></td>";
                        strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
                        strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + lblnetamount.ToString() + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        strp += "</td></tr><tr><td class='gap' style=''>&nbsp</td></tr></table>";
                    }

                    if (Session["igstp"].ToString() == "YES")
                    {
                        double TOTALIGST = 0;
                        double SUBTOTAL = 0;
                        double TOTALGST = 0;
                        double TOTALGSTPLUSAMO = 0;

                        strp += "<table border='0' width='100%'><tr><td class='' style='text-align: left; font-weight: bold;'>OUR FEES</td></tr><tr><td class='' style=''>";
                        strp += "<table class='PaymentPhase' style='border:0' width='100%'><tr><td class='gap' style=''>&nbsp</td></tr>";
                        strp += "<tr>";
                        strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>S.NO</td>";
                        strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>HSN<br> CODE</td>";
                        strp += "<td style='width:44%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>PARTICULARS</td>";
                        strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>QNTY<br> (PCS)</td>";
                        strp += "<td style='width:10%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>RATE<br> (RS)</td>";
                        strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>GST</td>";

                        strp += "<td style='width:14%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;''>";
                        strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        strp += "<tr>";
                        strp += "<td style='width:14%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-right:none; font-weight: bold; background-color: #e31e24; text-align: center; color: white; ' colspan='2'>IGST</td>";
                        strp += "</tr>";
                        strp += "<tr>";
                        strp += "<td style='width:5%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-bottom:none;  font-weight: bold; background-color: #e31e24; text-align: center; color: white; '>RATE</td>";
                        strp += "<td style='width:9%;  font-weight: bold; background-color: #e31e24; text-align: center; color: white; '>AMOUNT</td>";
                        strp += "</tr>";
                        strp += "</table>";
                        strp += "</td>";

                        strp += "<td style='width:10%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>AMOUNT<br> (RS)</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        for (int i = 0; i < dtp.Rows.Count; i++)
                        {
                            string HSN = dtp.Rows[i]["HSN"].ToString();
                            string Productname = dtp.Rows[i]["Product_name"].ToString();
                            decimal qtyDec = Convert.ToDecimal(dtp.Rows[i]["Quantity"]);
                            int Quantity = (int)qtyDec;
                            TQ = TQ + Quantity;
                            double baserate = Math.Round((Convert.ToDouble(dtp.Rows[i]["sail_rate"])), 2);
                            int gstper = Convert.ToInt32(dtp.Rows[i]["Service_tax_rate"]);
                            double QuantityBaserateAmo = Math.Round((Quantity * baserate), 2);

                            string QuantityBaserateAmo1 = DoFormat(QuantityBaserateAmo);

                            double gstamount = Math.Round(((QuantityBaserateAmo * gstper) / 100), 2);
                            double cgstper = Math.Round((Convert.ToDouble(gstper) / 2), 2);
                            double cgstamo = Math.Round((Convert.ToDouble(gstamount) / 2), 2);

                            TOTALIGST = TOTALIGST + gstamount;
                            SUBTOTAL = SUBTOTAL + QuantityBaserateAmo;
                            TOTALGST = TOTALGST + gstamount;

                            strp += "<table class='' style='border:0' width='100%'>";
                            strp += "<tr>";
                            strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + (i + 1).ToString() + "</td>";
                            strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + HSN + "</td>";
                            strp += "<td style='width:44%; border:2px solid #6c6c6c; font-weight: bold;  text-align: left;     border-right:none; border-top:none;'>" + Productname + "</td>";
                            strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + Quantity.ToString() + "</td>";
                            strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + baserate.ToString() + "</td>";
                            strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";

                            strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";
                            strp += "<td style='width:9%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + gstamount.ToString() + "</td>";

                            strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;   border-top:none;'>" + QuantityBaserateAmo1.ToString() + "</td>";
                            strp += "</tr>";
                            strp += "</table>";
                        }

                        TOTALGSTPLUSAMO = TOTALGST + SUBTOTAL;
                        string grandtotal = (Math.Round((Convert.ToDouble(TOTALGSTPLUSAMO))).ToString());
                        string word = MoneyConvDS.MoneyConvFn(lblnetamount);

                        strp += "<table class='' style='border:0' width='100%'>";
                        strp += "<tr>";
                        strp += "<td style='border:2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none; background-color:#d9d3d3' colspan='3'>GRAND TOTAL</td>";
                        strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'>" + TQ.ToString() + "</td>";
                        strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
                        strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
                        strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
                        strp += "<td style='width:9%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'>" + TOTALIGST.ToString() + "</td>";
                        strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;   border-top:none;background-color:#d9d3d3'>" + Math.Round(SUBTOTAL).ToString() + ".00" + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        strp += "<table class='' style='border:0' width='100%'>";
                        strp += "<tr>";
                        strp += "<td style='font-weight: bold;  text-align: center' colspan='6'></td>";
                        strp += "<td style='width:28%; font-weight: bold;  text-align: right; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT BEFORE TAX:</td>";
                        strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none'>" + Math.Round(SUBTOTAL).ToString() + ".00" + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        strp += "<table class='' style='border:0' width='100%'>";
                        strp += "<tr>";
                        strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none' colspan='6'></td>";
                        strp += "<td style='width:28%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL GST:</td>";
                        strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + Math.Round(TOTALGST).ToString() + ".00" + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        strp += "<table class='' style='border:0' width='100%'>";
                        strp += "<tr>";
                        strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='5'>Amount (In Words):" + word + "</td>";
                        strp += "<td style='width:5%; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'></td>";
                        strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
                        strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + lblnetamount.ToString() + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        strp += "</td></tr><tr><td class='gap' style=''>&nbsp</td></tr></table>";
                    }
                    lblserviceamo.Text = strp.ToString();
                }
            }
        }

        private string DoFormat(double myNumber)
        {
            return string.Format("{0:0.00}", myNumber);
        }

        private void bindVatdetails()
        {
            string query = "select ('Vat @ '+ Vat_rate +' %') as rete,Vat_amount from tbl_quotation_vat where Quotation_no=@Quotation_no order by Id";
            SqlParameter[] pram = { new SqlParameter("@Quotation_no", lblqnumber.Text) };

            dtvat = DbCL.SPreturn_dt(query, pram);
            if (dtvat.Rows.Count > 0)
            {
                for (int i = 0; i < dtvat.Rows.Count; i++)
                {
                    string rete = dtvat.Rows[i]["rete"].ToString();
                    string Vat_amount = dtvat.Rows[i]["Vat_amount"].ToString();

                    strbackdt.Append("<table class='' style='border:0' width='100%'>");
                    strbackdt.Append("<tr>");
                    strbackdt.Append("<td style='font-weight: bold;  text-align: center' colspan='4'></td>");
                    strbackdt.Append("<td style='width:14%; font-weight: bold;  text-align: right; background-color: #e31e24; color: white;'>" + rete + ":</td>");
                    strbackdt.Append("<td style='width:10%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;  border-top:none'>" + Vat_amount + "</td>");
                    strbackdt.Append("</tr>");
                    strbackdt.Append("</table>");
                }
            }
        }

        private string statusvalue(string qno)
        {
            // *** REVISION 8: Force YES for Direct Invoices ***
            if (qno == "Direct")
            {
                PnlTaxKvqa.Visible = false;
                PnlGstKvqa.Visible = true;
                return "YES";
            }

            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string status_value = "";
            string Lst = "30-Jun-2017";
            string CmdString = "select Quotation_no from tbl_Quotation where Quotation_no='" + qno + "' and (CONVERT(datetime, Quotation_date, 103) > CONVERT(datetime, '" + Lst + "', 103))";
            SqlCommand cmd = new SqlCommand(CmdString, DbCL.Conn);
            SqlDataReader re1 = cmd.ExecuteReader();
            if (re1.Read())
            {
                status_value = "YES";
                PnlTaxKvqa.Visible = false;
                PnlGstKvqa.Visible = true;
            }
            else
            {
                status_value = "NO";
                PnlTaxKvqa.Visible = true;
                PnlGstKvqa.Visible = false;
            }
            DbCL.Conn.Close();
            return status_value;
        }

        // ... (Keep your existing representative, Bindclientdetails, and GenerateNonGSTTable methods below) ...

        private void representative(string clientid)
        {
            string query = "select Representative_name,Designation,Phone_no,Email,RepTitle,RepLastName from tbl_representative where Copany_Id=@Copany_Id";
            SqlParameter[] pram = { new SqlParameter("@Copany_Id", clientid) };
            dtRep = DbCL.SPreturn_dt(query, pram);
            if (dtRep.Rows.Count > 0)
            {
                TextInfo textInfo1 = cultureInfo.TextInfo;
                string RepTitle = dtRep.Rows[0]["RepTitle"].ToString();
                string repname = dtRep.Rows[0]["Representative_name"].ToString();
                string RepLastName = dtRep.Rows[0]["RepLastName"].ToString();
                string Designation = dtRep.Rows[0]["Designation"].ToString();

                string rename = RepTitle + " " + repname + " " + RepLastName;
                lblrename.Text = textInfo1.ToTitleCase(rename.ToLower());
                lbldeg.Text = textInfo1.ToTitleCase(Designation.ToLower());
            }
        }

        private void Bindclientdetails(string clientid)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Name,Address1,Address2,City,pin,State,Vat_no,Service_tax_no,PlaceofSupply from tbl_Client where Client_Id='" + clientid + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                TextInfo textInfo1 = cultureInfo.TextInfo;
                clientName.Text = textInfo1.ToTitleCase(re["Client_Name"].ToString().ToLower());
                string Address1 = re["Address1"].ToString();
                string Address2 = re["Address2"].ToString();
                string add = (Address1 == Address2) ? Address1 : Address1 + " " + Address2;
                txtaddres.Text = textInfo1.ToTitleCase(add.ToString().ToLower());
                lblcity.Text = textInfo1.ToTitleCase(re["City"].ToString().ToLower());
                lblpincode.Text = textInfo1.ToTitleCase(re["pin"].ToString().ToLower());
                gstno = re["Service_tax_no"].ToString();
                vatno = re["Vat_no"].ToString();
                lblGstno.Text = gstno;
            }
            DbCL.Conn.Close();
        }

        // This is your helper function for Non-GST layout (Optional to keep if you deleted it)
        private void GenerateNonGSTTable(DataTable dt)
        {
            if (dt.Rows.Count > 0)
            {
                strbackdt.Append("<table class='' style='border:0' width='100%'>");
                strbackdt.Append("<tr>");
                strbackdt.Append("<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>S.NO</td>");
                strbackdt.Append("<td style='width:51%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>PARTICULARS</td>");
                strbackdt.Append("<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>QNTY<br>(PCS)</td>");
                strbackdt.Append("<td style='width:15%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>RATE<br> (RS)</td>");
                strbackdt.Append("<td style='width:14%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;''>VAT(%)</td>");
                strbackdt.Append("<td style='width:10%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>AMOUNT<br> (RS)</td>");
                strbackdt.Append("</tr>");
                strbackdt.Append("</table>");

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string slno = dt.Rows[i]["Sl_no"].ToString();
                    string Product_name = dt.Rows[i]["Product_name"].ToString();
                    string Quantity = dt.Rows[i]["Quantity"].ToString();
                    string sail_rate = dt.Rows[i]["sail_rate"].ToString();
                    string Service_tax_rate = dt.Rows[i]["Service_tax_rate"].ToString();
                    string Total_sail_rate2 = dt.Rows[i]["Total_sail_rate2"].ToString();

                    strbackdt.Append("<table class='' style='border:0' width='100%'>");
                    strbackdt.Append("<tr>");
                    strbackdt.Append("<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;  border-right:none;'>" + slno + "</td>");
                    strbackdt.Append("<td style='width:51%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center;   border-right:none;'>" + Product_name + "</td>");
                    strbackdt.Append("<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center;   border-right:none;'>" + Quantity + "</td>");
                    strbackdt.Append("<td style='width:15%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center;   border-right:none;'>" + sail_rate + "</td>");
                    strbackdt.Append("<td style='width:14%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center;   border-right:none;''>" + Service_tax_rate + "</td>");
                    strbackdt.Append("<td style='width:10%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; '>" + Total_sail_rate2 + "</td>");
                    strbackdt.Append("</tr>");
                    strbackdt.Append("</table>");
                }
                strbackdt.Append("<table class='' style='border:0' width='100%'>");
                strbackdt.Append("<tr>");
                strbackdt.Append("<td style='font-weight: bold;  text-align: center' colspan='4'></td>");
                strbackdt.Append("<td style='width:14%; font-weight: bold;  text-align: right; background-color: #e31e24; color: white;'>TOTAL AMOUNT BEFORE TAX:</td>");
                strbackdt.Append("<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none'>" + lblSubtotal + "</td>");
                strbackdt.Append("</tr>");
                strbackdt.Append("</table>");

                bindVatdetails();

                strbackdt.Append("<table class='' style='border:0' width='100%'>");
                strbackdt.Append("<tr>");
                strbackdt.Append("<td style='font-weight: bold;  text-align: center' colspan='4'></td>");
                strbackdt.Append("<td style='width:14%; font-weight: bold;  text-align: right; background-color: #e31e24; color: white;'>ROUND OFF:</td>");
                strbackdt.Append("<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none'>" + lblstax + "</td>");
                strbackdt.Append("</tr>");
                strbackdt.Append("</table>");

                strbackdt.Append("<table class='' style='border:0' width='100%'>");
                strbackdt.Append("<tr>");
                strbackdt.Append("<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='3'>Amount (In Words):" + lblword + "</td>");
                strbackdt.Append("<td style='width:15%; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'></td>");
                strbackdt.Append("<td style='width:14%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;'>TOTAL AMOUNT AFTER TAX:</td>");
                strbackdt.Append("<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + lblnetamount + "</td>");
                strbackdt.Append("</tr>");
                strbackdt.Append("</table>");

                lblbackdata.Text = strbackdt.ToString();
            }
        }

        protected void Button1_Click(object sender, EventArgs e) { }
        protected void Button2_Click(object sender, EventArgs e) { }
    }
}