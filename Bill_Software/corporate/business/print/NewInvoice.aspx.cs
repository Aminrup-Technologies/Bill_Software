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
    public partial class NewInvoice : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        public string strp = "";

        DataTable dtp = new DataTable();
        DataTable dtci = new DataTable();
        DataTable dtBacdata = new DataTable();
        DataTable dtRep = new DataTable();
        DataTable dtChadd = new DataTable();

        StringBuilder strbackdt = new StringBuilder();

        public string vatno = "";
        public string gstno = "";

        public string taxorvat = "";
        public string proOrser = "";
        public string psid = "";
        public double TQ = 0;
        public string lblSubtotal = "", lbldiscount = "", lblstax = "", lblstax0 = "", lblnetamount = "", lblword = "";

        // New fields for Freight/Other
        public double freight = 0;
        public double other = 0;
        public string otherName = "";

        CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string ID = Request.QueryString["ID"];
                if (!string.IsNullOrEmpty(ID))
                {
                    buindalldata(ID);
                    //Bindtaxdata();
                    Buindamount();
                    //BindVatamount();
                }
            }
        }

        private void buindalldata(string ID)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            // Updated query to fetch Freight, Other Charges, and PO details
            string query = "SELECT i.ID AS InvoiceID, i.Invoice_No, i.ExtInvoiceNo, i.Invoice_Date, i.Quotation_No, " +
                           "i.Quotation_Date, i.Client_ID, i.addressfor, i.discount, i.sub_total, i.Service_Tax, i.Net_Amount, " +
                           "i.cgstOrsgst, i.igst, i.Delivery_Amount, i.otherAmount1, i.otherAmount1_name, " +
                           "q.DO_Number, q.PO_Number, q.PO_Date " +
                           "FROM tbl_Invoice i LEFT JOIN tbl_Quotation q ON i.Quotation_No = q.Quotation_no " +
                           "WHERE i.ID = '" + ID.ToString() + "'";

            SqlCommand cmd = new SqlCommand(query, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblinvno.Text = re["Invoice_No"].ToString();
                lbl_extinvno.Text = re["ExtInvoiceNo"] != DBNull.Value ? re["ExtInvoiceNo"].ToString() : "";

                DateTime invDt;
                if (DateTime.TryParse(re["Invoice_Date"].ToString(), out invDt))
                    lblinvdate.Text = invDt.ToString("dd-MMM-yyyy");
                else
                    lblinvdate.Text = re["Invoice_Date"].ToString();

                string quotationNo = re["Quotation_No"] != DBNull.Value ? re["Quotation_No"].ToString() : string.Empty;

                Session["cgstOrsgsti"] = re["cgstOrsgst"].ToString();
                Session["igsti"] = re["igst"].ToString();

                // Logic for PO Number: 
                // If it's a Manual Invoice, the PO Number is stored in the Quotation_No column.
                // If it's a Process Invoice, it comes from the joined tbl_Quotation table.
                string dbPO = re["PO_Number"] != DBNull.Value ? re["PO_Number"].ToString() : string.Empty;

                if (string.IsNullOrEmpty(dbPO) && quotationNo != "N/A" && !quotationNo.StartsWith("QTN"))
                {
                    // Likely Manual Invoice where PO is in Quotation_No field
                    lblqnumber.Text = "N/A";
                    lbl_pono.Text = quotationNo;
                }
                else
                {
                    // Likely Process Invoice
                    lblqnumber.Text = !string.IsNullOrEmpty(quotationNo) ? quotationNo : "N/A";
                    lbl_pono.Text = !string.IsNullOrEmpty(dbPO) ? dbPO : "N/A";

                    if (re["PO_Date"] != DBNull.Value)
                    {
                        DateTime poDate;
                        if (DateTime.TryParse(re["PO_Date"].ToString(), out poDate))
                            lbl_podate.Text = poDate.ToString("dd-MMM-yyyy");
                        else
                            lbl_podate.Text = re["PO_Date"].ToString();
                    }
                }

                // Retrieve Extra Costs
                double.TryParse(re["Delivery_Amount"].ToString(), out freight);
                double.TryParse(re["otherAmount1"].ToString(), out other);
                otherName = re["otherAmount1_name"].ToString();

                // Place of Supply Logic
                // Try to get from Quotation first
                string qno = re["Quotation_No"].ToString();
                bool posFound = bindcgstorigst(qno);

                string clientid = re["Client_ID"].ToString();
                string addressfor = re["addressfor"].ToString();

                representative(clientid);
                // This fetches details + POS from Client Master if not found in Quotation
                Bindclientdetails(clientid, addressfor, !posFound);

                lblSubtotal = re["sub_total"].ToString();
                lbldiscount = re["discount"].ToString();
                lblstax = re["Service_Tax"].ToString(); // Total Tax
                lblnetamount = re["Net_Amount"].ToString();

                string invoice_no = lblinvno.Text;
                deliveryAddress(invoice_no);
            }
            // DbCL.Conn.Close(); // Handled inside Bindclientdetails if open

            string word = MoneyConvDS.MoneyConvFn(lblnetamount);
            lblword = word.ToString();
        }

        private void deliveryAddress(string invoice_no)
        {
            string query = "select SiteAddress from tbl_InvSiteAddress where invoice_no=@invoice_no order by id";
            SqlParameter[] pram = {
                new SqlParameter("@invoice_no",invoice_no)
            };
            dtChadd = DbCL.SPreturn_dt(query, pram);
            if (dtChadd.Rows.Count > 0)
            {
                string SiteAddress = "";
                for (int i = 0; i < dtChadd.Rows.Count; i++)
                {
                    string add = dtChadd.Rows[i]["SiteAddress"].ToString();
                    SiteAddress += add + "<br>";
                }
                lblAddress.Text = SiteAddress.ToString();
            }
        }

        private void representative(string clientid)
        {
            string query = "select Representative_name,Designation,Phone_no,Email,RepTitle,RepLastName from tbl_representative where Copany_Id=@Copany_Id";
            SqlParameter[] pram = {
                new SqlParameter("@Copany_Id",clientid)
            };
            dtRep = DbCL.SPreturn_dt(query, pram);
            if (dtRep.Rows.Count > 0)
            {
                // Logic kept from original
            }
        }

        private void Bindclientdetails(string clientid, string addressfor, bool setPOS)
        {
            if (DbCL.Conn.State == ConnectionState.Closed)
            {
                DbCL.Sqlconnection();
                DbCL.ConnectDb();
            }
            string cmdstring = "select Client_Id,Client_Name,Address1,Address2,City,pin,State,Vat_no,Service_tax_no,Pan_no,PlaceofSupply from tbl_Client where Client_Id='" + clientid + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                TextInfo textInfo1 = cultureInfo.TextInfo;

                string Client_Name = re["Client_Name"].ToString();
                clientName.Text = textInfo1.ToTitleCase(Client_Name.ToLower());

                if (addressfor == "Corporate office")
                {
                    string Address1 = re["Address1"].ToString();
                    string Address2 = re["Address2"].ToString();
                    string add = "";
                    if (Address1 == Address2)
                    {
                        add = Address1;
                    }
                    else
                    {
                        add = Address1 + " " + Address2;
                    }

                    txtaddres.Text = textInfo1.ToTitleCase(add.ToLower());

                    string city = re["City"].ToString();
                    string pin = re["pin"].ToString();

                    lblcity.Text = textInfo1.ToTitleCase(city.ToLower());
                    lblpincode.Text = textInfo1.ToTitleCase(pin.ToLower());
                }

                gstno = re["Service_tax_no"].ToString();
                vatno = re["Vat_no"].ToString();
                lblGstno.Text = gstno;

                if (setPOS)
                {
                    string placeofsupply = re["PlaceofSupply"].ToString();
                    lblplaceofsup1.Text = "Place Of Supply";
                    lblplaceofsup2.Text = ":";
                    lblplaceofsup3.Text = placeofsupply;
                }
            }
            re.Close(); // Important if reusing connection
            DbCL.Conn.Close();
        }

        private bool bindcgstorigst(string qno)
        {
            bool found = false;
            string query = "select cgstOrsgst,igst,PlaceofSupply from tbl_Quotation where Quotation_no=@Quotation_no";
            SqlParameter[] pram = {
                new SqlParameter("@Quotation_no",qno)
            };
            dtci = DbCL.SPreturn_dt(query, pram);
            if (dtci.Rows.Count > 0)
            {
                Session["cgstOrsgsti"] = dtci.Rows[0]["cgstOrsgst"].ToString();
                Session["igsti"] = dtci.Rows[0]["igst"].ToString();

                string placeofsupply = dtci.Rows[0]["PlaceofSupply"].ToString();
                lblplaceofsup1.Text = "Place Of Supply";
                lblplaceofsup2.Text = ":";
                lblplaceofsup3.Text = placeofsupply;
                found = true;
            }
            return found;
        }

        private void Buindamount()
        {
            string qno = lblqnumber.Text;

            // Determine Flow Logic
            // Process Flow = Exists in tbl_Quotation and meets date criteria
            string status = (qno == "N/A" || string.IsNullOrEmpty(qno)) ? "YES" : statusvalue(qno);

            if (status != "YES")
            {
                // ============================================
                // PROCESS FLOW (Original Logic for Quotation)
                // ============================================
                pnlTasGst.Visible = false;
                lblserviceamo.Visible = false;
                BackData.Visible = true;

                string query = "select Sl_no, (Product_name+' '+specification) as Product_name, Quantity, sail_rate, Service_tax_rate, Total_sail_rate2 from tbl_Quotaion_details where Quotation_no='" + lblqnumber.Text + "' order by CAST(Sl_no as int)";

                SqlParameter[] pram = {
                    new SqlParameter("@Quotation_no",lblqnumber.Text)
                };
                dtBacdata = DbCL.SPreturn_dt(query, pram);
                if (dtBacdata.Rows.Count > 0)
                {
                    strbackdt.Append("<table class='' style='border:2px solid #6c6c6c; border-collapse:collapse;' width='100%'>");
                    strbackdt.Append("<tr style='background-color:#e31e24; color:white;'>");
                    strbackdt.Append("<td style='width:5%; border:1px solid #6c6c6c; font-weight:bold; text-align:center;'>S.NO</td>");
                    strbackdt.Append("<td style='width:51%; border:1px solid #6c6c6c; font-weight:bold; text-align:center;'>PARTICULARS</td>");
                    strbackdt.Append("<td style='width:5%; border:1px solid #6c6c6c; font-weight:bold; text-align:center;'>QTY</td>");
                    strbackdt.Append("<td style='width:15%; border:1px solid #6c6c6c; font-weight:bold; text-align:center;'>RATE</td>");
                    strbackdt.Append("<td style='width:14%; border:1px solid #6c6c6c; font-weight:bold; text-align:center;'>TAX(%)</td>");
                    strbackdt.Append("<td style='width:10%; border:1px solid #6c6c6c; font-weight:bold; text-align:right;'>AMOUNT</td>");
                    strbackdt.Append("</tr>");

                    for (int i = 0; i < dtBacdata.Rows.Count; i++)
                    {
                        strbackdt.Append("<tr>");
                        strbackdt.Append("<td style='border:1px solid #6c6c6c; text-align:center;'>" + dtBacdata.Rows[i]["Sl_no"] + "</td>");
                        strbackdt.Append("<td style='border:1px solid #6c6c6c; text-align:left; padding-left:5px;'>" + dtBacdata.Rows[i]["Product_name"] + "</td>");
                        strbackdt.Append("<td style='border:1px solid #6c6c6c; text-align:center;'>" + dtBacdata.Rows[i]["Quantity"] + "</td>");
                        strbackdt.Append("<td style='border:1px solid #6c6c6c; text-align:center;'>" + dtBacdata.Rows[i]["sail_rate"] + "</td>");
                        strbackdt.Append("<td style='border:1px solid #6c6c6c; text-align:center;'>" + dtBacdata.Rows[i]["Service_tax_rate"] + "</td>");
                        strbackdt.Append("<td style='border:1px solid #6c6c6c; text-align:right; padding-right:5px;'>" + dtBacdata.Rows[i]["Total_sail_rate2"] + "</td>");
                        strbackdt.Append("</tr>");
                    }
                    strbackdt.Append("</table>");

                    // Footer for Process Flow (Simpler)
                    strbackdt.Append("<table style='width:100%; border:0;'>");
                    strbackdt.Append("<tr><td style='text-align:right; font-weight:bold; width:85%;'>TOTAL AMOUNT BEFORE TAX:</td><td style='text-align:right; width:15%;'>" + lblSubtotal + "</td></tr>");

                    bindVatdetails(); // Adds VAT lines if any

                    if (lbldiscount != "0.00" && lbldiscount != "0")
                    {
                        strbackdt.Append("<tr><td style='text-align:right; font-weight:bold;'>DISCOUNT:</td><td style='text-align:right;'>" + lbldiscount + "</td></tr>");
                    }

                    strbackdt.Append("<tr><td style='text-align:right; font-weight:bold;'>TOTAL TAX:</td><td style='text-align:right;'>" + lblstax + "</td></tr>");

                    // Grand Total
                    strbackdt.Append("<tr><td colspan='2'><hr/></td></tr>");
                    strbackdt.Append("<tr><td style='text-align:left; font-weight:bold; background-color:#e31e24; color:white;'>Amount (In Words): " + lblword + "</td>");
                    strbackdt.Append("<td style='text-align:right; font-weight:bold; background-color:#e31e24; color:white;'>" + lblnetamount + "</td></tr>");
                    strbackdt.Append("</table>");
                }
                lblbackdata.Text = strbackdt.ToString();
            }
            else
            {
                // ============================================
                // DIRECT FLOW (MANUAL INVOICE) 
                // ============================================
                pnlTasGst.Visible = true;
                lblserviceamo.Visible = true;
                BackData.Visible = false;

                string cmdstring = "select Product_Code as HSN,(Product_name+' '+specification) as Product_name, Quantity, sail_rate, discountRate, Service_tax_rate, Total_sail_rate1, Total_sail_rate2 from tbl_Invoice_details where Invoice_No=@Invoice_No order by CAST(Sl_no as int)";
                // Note: Filter by Invoice_No for reliability
                SqlParameter[] pram = {
                     new SqlParameter("@Invoice_No", lblinvno.Text)
                };

                dtp = DbCL.SPreturn_dt(cmdstring, pram);

                // If dtp is empty, try fallback with Quotation_No if applicable, but Invoice_No is safer.
                if (dtp.Rows.Count == 0 && lblqnumber.Text != "N/A")
                {
                    cmdstring = "select Product_Code as HSN,(Product_name+' '+specification) as Product_name, Quantity, sail_rate, discountRate, Service_tax_rate, Total_sail_rate1, Total_sail_rate2 from tbl_Invoice_details where Quotation_no=@Quotation_no order by CAST(Sl_no as int)";
                    pram = new SqlParameter[] { new SqlParameter("@Quotation_no", lblqnumber.Text) };
                    dtp = DbCL.SPreturn_dt(cmdstring, pram);
                }

                if (dtp.Rows.Count > 0)
                {
                    double TOTALCGST = 0, TOTALIGST = 0;
                    double SUBTOTAL = 0;
                    double TOTALGST = 0;

                    // Determine if Intra (CGST+SGST) or Inter (IGST) based on Session
                    bool isIntra = (Session["cgstOrsgsti"] != null && Session["cgstOrsgsti"].ToString() == "YES");

                    // Start Table
                    strp += "<table class='' style='border:2px solid #6c6c6c; border-collapse:collapse;' width='100%'>";

                    // HEADER
                    strp += "<tr style='background-color:#e31e24; color:white;'>";
                    strp += "<th style='border:1px solid #6c6c6c; width:5%;'>S.NO</th>";
                    strp += "<th style='border:1px solid #6c6c6c; width:30%; text-align:left;'>PARTICULARS</th>";
                    strp += "<th style='border:1px solid #6c6c6c; width:8%;'>HSN</th>";
                    strp += "<th style='border:1px solid #6c6c6c; width:7%;'>QTY</th>";
                    strp += "<th style='border:1px solid #6c6c6c; width:10%;'>RATE</th>";
                    strp += "<th style='border:1px solid #6c6c6c; width:7%;'>DISC %</th>";

                    if (isIntra)
                    {
                        strp += "<th style='border:1px solid #6c6c6c; width:5%;'>CGST%</th>";
                        strp += "<th style='border:1px solid #6c6c6c; width:8%;'>AMT</th>";
                        strp += "<th style='border:1px solid #6c6c6c; width:5%;'>SGST%</th>";
                        strp += "<th style='border:1px solid #6c6c6c; width:8%;'>AMT</th>";
                    }
                    else
                    {
                        strp += "<th style='border:1px solid #6c6c6c; width:10%;'>IGST%</th>";
                        strp += "<th style='border:1px solid #6c6c6c; width:15%;'>IGST AMT</th>";
                    }
                    strp += "<th style='border:1px solid #6c6c6c; width:12%; text-align:right;'>AMOUNT</th>";
                    strp += "</tr>";

                    // ROWS
                    for (int i = 0; i < dtp.Rows.Count; i++)
                    {
                        string HSN = dtp.Rows[i]["HSN"].ToString();
                        string Productname = dtp.Rows[i]["Product_name"].ToString();
                        double Quantity = Convert.ToDouble(dtp.Rows[i]["Quantity"]);
                        TQ += Quantity;

                        double baserate = Math.Round(Convert.ToDouble(dtp.Rows[i]["sail_rate"]), 2);
                        double discountrate = Math.Round(Convert.ToDouble(dtp.Rows[i]["discountRate"]), 2);
                        double gstper = Convert.ToDouble(dtp.Rows[i]["Service_tax_rate"]);

                        // Calc
                        double rowGross = Quantity * baserate;
                        double rowDisc = Math.Round((rowGross * discountrate) / 100, 2);
                        double taxable = rowGross - rowDisc;
                        double gstAmount = Math.Round((taxable * gstper) / 100, 2);

                        SUBTOTAL += taxable;
                        TOTALGST += gstAmount;

                        strp += "<tr>";
                        strp += "<td style='border:1px solid #6c6c6c; text-align:center;'>" + (i + 1) + "</td>";
                        strp += "<td style='border:1px solid #6c6c6c; text-align:left;'>" + Productname + "</td>";
                        strp += "<td style='border:1px solid #6c6c6c; text-align:center;'>" + HSN + "</td>";
                        strp += "<td style='border:1px solid #6c6c6c; text-align:center;'>" + Quantity + "</td>";
                        strp += "<td style='border:1px solid #6c6c6c; text-align:center;'>" + baserate + "</td>";
                        strp += "<td style='border:1px solid #6c6c6c; text-align:center;'>" + discountrate + "</td>";

                        if (isIntra)
                        {
                            double halfRate = gstper / 2;
                            double halfAmt = Math.Round(gstAmount / 2, 2);
                            TOTALCGST += halfAmt; // Accumulate for later if needed

                            strp += "<td style='border:1px solid #6c6c6c; text-align:center;'>" + halfRate + "</td>";
                            strp += "<td style='border:1px solid #6c6c6c; text-align:center;'>" + halfAmt + "</td>";
                            strp += "<td style='border:1px solid #6c6c6c; text-align:center;'>" + halfRate + "</td>";
                            strp += "<td style='border:1px solid #6c6c6c; text-align:center;'>" + halfAmt + "</td>";
                        }
                        else
                        {
                            TOTALIGST += gstAmount;
                            strp += "<td style='border:1px solid #6c6c6c; text-align:center;'>" + gstper + "</td>";
                            strp += "<td style='border:1px solid #6c6c6c; text-align:center;'>" + gstAmount + "</td>";
                        }

                        strp += "<td style='border:1px solid #6c6c6c; text-align:right;'>" + DoFormat(taxable) + "</td>";
                        strp += "</tr>";
                    }

                    // FOOTER
                    int colspan = isIntra ? 10 : 8; // Adjust spanning based on columns shown

                    // 1. Taxable Total
                    strp += "<tr><td colspan='" + colspan + "' style='border:1px solid #6c6c6c; text-align:right; font-weight:bold;'>TOTAL TAXABLE VALUE:</td>";
                    strp += "<td style='border:1px solid #6c6c6c; text-align:right; font-weight:bold;'>" + DoFormat(SUBTOTAL) + "</td></tr>";

                    // 2. Total Tax
                    strp += "<tr><td colspan='" + colspan + "' style='border:1px solid #6c6c6c; text-align:right; font-weight:bold;'>TOTAL TAX:</td>";
                    strp += "<td style='border:1px solid #6c6c6c; text-align:right; font-weight:bold;'>" + DoFormat(TOTALGST) + "</td></tr>";

                    // 3. Freight
                    if (freight > 0)
                    {
                        strp += "<tr><td colspan='" + colspan + "' style='border:1px solid #6c6c6c; text-align:right; font-weight:bold;'>ADD: FREIGHT CHARGES:</td>";
                        strp += "<td style='border:1px solid #6c6c6c; text-align:right; font-weight:bold;'>" + DoFormat(freight) + "</td></tr>";
                    }

                    // 4. Other Charges
                    if (other > 0)
                    {
                        string lblOther = !string.IsNullOrEmpty(otherName) ? otherName.ToUpper() : "OTHER CHARGES";
                        strp += "<tr><td colspan='" + colspan + "' style='border:1px solid #6c6c6c; text-align:right; font-weight:bold;'>ADD: " + lblOther + ":</td>";
                        strp += "<td style='border:1px solid #6c6c6c; text-align:right; font-weight:bold;'>" + DoFormat(other) + "</td></tr>";
                    }

                    // 5. Grand Total
                    double finalGrand = Math.Round(SUBTOTAL + TOTALGST + freight + other, 2);
                    strp += "<tr style='background-color:#d9d3d3;'>";
                    strp += "<td colspan='" + (colspan - 2) + "' style='border:1px solid #6c6c6c; text-align:left; font-weight:bold;'>Amount (In Words): " + MoneyConvDS.MoneyConvFn(finalGrand.ToString()) + "</td>";
                    strp += "<td colspan='2' style='border:1px solid #6c6c6c; text-align:right; font-weight:bold;'>GRAND TOTAL:</td>";
                    strp += "<td style='border:1px solid #6c6c6c; text-align:right; font-weight:bold;'>" + DoFormat(finalGrand) + "</td>";
                    strp += "</tr>";

                    strp += "</table>";
                    lblserviceamo.Text = strp;
                }
            }
        }

        public static string DoFormat(double myNumber)
        {
            return string.Format("{0:0.00}", myNumber);
        }

        private void bindVatdetails()
        {
            string query = "select ('Vat @ '+ Vat_rate +' %') as rete,Vat_amount from tbl_quotation_vat where Quotation_no=@Quotation_no order by Id";
            SqlParameter[] pram = { new SqlParameter("@Quotation_no", lblqnumber.Text) };
            DataTable dtvat = DbCL.SPreturn_dt(query, pram);
            if (dtvat.Rows.Count > 0)
            {
                strbackdt.Append("<table style='width:100%;'>");
                for (int i = 0; i < dtvat.Rows.Count; i++)
                {
                    string rete = dtvat.Rows[i]["rete"].ToString();
                    string Vat_amount = dtvat.Rows[i]["Vat_amount"].ToString();
                    strbackdt.Append("<tr><td style='text-align:right; font-weight:bold; width:85%;'>" + rete + ":</td><td style='text-align:right; width:15%;'>" + Vat_amount + "</td></tr>");
                }
                strbackdt.Append("</table>");
            }
        }

        private string statusvalue(string qno)
        {
            if (DbCL.Conn.State == ConnectionState.Closed)
            {
                DbCL.Sqlconnection();
                DbCL.ConnectDb();
            }
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

        protected void Button1_Click(object sender, EventArgs e)
        {

        }

        protected void Button2_Click(object sender, EventArgs e)
        {

        }
    }
}