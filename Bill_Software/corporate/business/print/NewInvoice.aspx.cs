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
        public int TQ = 0;
        public string lblSubtotal ="", lbldiscount="", lblstax="", lblstax0="", lblnetamount="", lblword="";
        CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string ID = Request.QueryString["ID"];
                buindalldata(ID);
                //Bindtaxdata();
                Buindamount();
                //BindVatamount();

            }
        }

        private void buindalldata(string ID)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string query = "SELECT i.ID AS InvoiceID, i.Invoice_No, i.ExtInvoiceNo, i.Invoice_Date, i.Quotation_No, i.Quotation_Date, i.Client_ID, i.addressfor, i.discount, i.sub_total, i.Service_Tax, i.Net_Amount, i.cgstOrsgst, i.igst, q.DO_Number, q.PO_Number, q.PO_Date FROM tbl_Invoice i LEFT JOIN tbl_Quotation q ON i.Quotation_No = q.Quotation_no WHERE i.ID = '" + ID.ToString() + "'";
            //string cmdstring = "select * from tbl_Invoice where ID='" + ID.ToString() + "'";
            SqlCommand cmd = new SqlCommand(query, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblinvno.Text = re["Invoice_No"].ToString();
                lbl_extinvno.Text = re["ExtInvoiceNo"].ToString();
                lblinvdate.Text = re["Invoice_Date"].ToString();
                //lblqnumber.Text = re["Quotation_No"].ToString();
                string quotationNo = re["Quotation_No"] != DBNull.Value ? re["Quotation_No"].ToString() : string.Empty;
                lblqnumber.Text = !string.IsNullOrEmpty(quotationNo) ? quotationNo : "N/A"; // Default value if empty

                Session["cgstOrsgsti"] = re["cgstOrsgst"].ToString();
                Session["igsti"] = re["igst"].ToString();

                string dopono = re["PO_Number"] != DBNull.Value ? re["PO_Number"].ToString() : string.Empty;
                lbl_pono.Text= !string.IsNullOrEmpty(dopono) ? dopono : "N/A";
                lbl_podate.Text = re["PO_Date"].ToString();

                string qno = re["Quotation_No"].ToString();
                bindcgstorigst(qno);

                string clientid = re["Client_ID"].ToString();
                string addressfor = re["addressfor"].ToString();

                representative(clientid);

                Bindclientdetails(clientid, addressfor);

                string discount_amount = re["discount"].ToString();
                
                decimal discount_amount1 = Convert.ToDecimal(discount_amount);
                //if (discount_amount1 == 0)
                //{
                //    discount_row.Visible = false;
                //}
                //else
                //{
                //    discount_row.Visible = true;
                //}

                lblSubtotal = re["sub_total"].ToString();
                lbldiscount = re["discount"].ToString();
                lblstax = re["Service_Tax"].ToString();
                lblstax0 = re["Service_Tax"].ToString();
                lblnetamount = re["Net_Amount"].ToString();

                string invoice_no = lblinvno.Text;
                deliveryAddress(invoice_no);
            }
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
            if (dtRep.Rows.Count>0)
            {
                string RepTitle = dtRep.Rows[0]["RepTitle"].ToString();
                string repname= dtRep.Rows[0]["Representative_name"].ToString();
                string RepLastName = dtRep.Rows[0]["RepLastName"].ToString();
                string Designation = dtRep.Rows[0]["Designation"].ToString();

                //lblrename.Text = RepTitle + repname + RepLastName;
                //lbldeg.Text = Designation;
            }
        }

        private void Bindclientdetails(string clientid, string addressfor)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Id,Client_Name,Address1,Address2,City,pin,State,Vat_no,Service_tax_no,Pan_no,PlaceofSupply from tbl_Client where Client_Id='" + clientid + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                TextInfo textInfo1 = cultureInfo.TextInfo;

                
                //lblrename.Text = textInfo1.ToTitleCase(rename.ToLower());
                string Client_Name= re["Client_Name"].ToString();

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
                    //lbladdress2.Text = re["Address2"].ToString();
                    //if (lbladdress2.Text == "")
                    //{
                    //    lbladdress2.Visible = false;
                    //}
                    //else
                    //{
                    //    lbladdress2.Visible = true;
                    //}
                    string city= re["City"].ToString();
                    string pin = re["pin"].ToString();

                    lblcity.Text = textInfo1.ToTitleCase(city.ToLower());
                    lblpincode.Text = textInfo1.ToTitleCase(pin.ToLower());
                    //lblstate.Text = re["State"].ToString();

                }
                else
                {
                    //Bindaddress(clientid, addressfor);
                }
                
                gstno = re["Service_tax_no"].ToString();
                vatno = re["Vat_no"].ToString();

                lblGstno.Text = gstno;

                //lblClientCode.Text = re["Client_Id"].ToString();
                //lblPanNo.Text= re["Pan_no"].ToString();

                //string placeofsupply = re["PlaceofSupply"].ToString();
                //lblplaceofsup1.Text = "Place Of Supply";
                //lblplaceofsup2.Text = ":";
                //lblplaceofsup3.Text = placeofsupply;
                //if (vatno == "" && gstno == "")
                //{
                //    lblClientVat.Visible = false;
                //}
                //else
                //{
                //    lblClientVat.Visible = true;
                //}
            }
            DbCL.Conn.Close();
        }

        private void bindcgstorigst(string qno)
        {
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
            }
        }

        private void Buindamount()
        {
            string qno = lblqnumber.Text;
            //string status = statusvalue(qno);

            // Determine status
            string status = (lblqnumber.Text == "N/A") ? "YES" : statusvalue(lblqnumber.Text);

            if (status != "YES")
            {
                pnlTasGst.Visible = false;
                lblserviceamo.Visible = false;
                BackData.Visible = true;
                //AMODETAILS.Visible = true;

                string query = "select Sl_no, (Product_name+' '+specification) as Product_name, Quantity, sail_rate, Service_tax_rate, Total_sail_rate2 from tbl_Quotaion_details where Quotation_no='" + lblqnumber.Text + "' order by CAST(Sl_no as int)";
                
                SqlParameter[] pram = {
                    new SqlParameter("@Quotation_no",lblqnumber.Text)
                };
                dtBacdata = DbCL.SPreturn_dt(query, pram);
                if (dtBacdata.Rows.Count>0)
                {
                    strbackdt.Append("<table class='' style='border:0' width='100%'>");
                    strbackdt.Append("<tr>");
                    strbackdt.Append("<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>S.NO</td>");
                    strbackdt.Append("<td style='width:51%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>PARTICULARS</td>");
                    strbackdt.Append("<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>QUANTITY<br>(PCS)</td>");
                    strbackdt.Append("<td style='width:15%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>RATE<br> (RS)</td>");
                    strbackdt.Append("<td style='width:14%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;''>VAT(%)</td>");
                    strbackdt.Append("<td style='width:10%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>AMOUNT<br> (RS)</td>");
                    strbackdt.Append("</tr>");
                    strbackdt.Append("</table>");

                    for (int i=0; i< dtBacdata.Rows.Count; i++)
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


                    if (lbldiscount != "0.00")
                    {
                        strbackdt.Append("<table class='' style='border:0' width='100%'>");
                        strbackdt.Append("<tr>");
                        strbackdt.Append("<td style='font-weight: bold;  text-align: center' colspan='4'></td>");
                        strbackdt.Append("<td style='width:14%; font-weight: bold;  text-align: right; background-color: #e31e24; color: white;'>DISCOUNT:</td>");
                        strbackdt.Append("<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none'>" + lbldiscount + "</td>");
                        strbackdt.Append("</tr>");
                        strbackdt.Append("</table>");
                    }

                    strbackdt.Append("<table class='' style='border:0' width='100%'>");
                    strbackdt.Append("<tr>");
                    strbackdt.Append("<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='3'>Amount (In Words): " + lblword + "</td>");
                    strbackdt.Append("<td style='width:15%; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'></td>");
                    strbackdt.Append("<td style='width:14%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;'>TOTAL AMOUNT AFTER TAX:</td>");
                    strbackdt.Append("<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + lblnetamount+"</td>");
                    strbackdt.Append("</tr>");
                    strbackdt.Append("</table>");
                }
                lblbackdata.Text = strbackdt.ToString();

            }
            else
            {
                pnlTasGst.Visible = true;
                lblserviceamo.Visible = true;
                BackData.Visible = false;

                string cmdstring = "select Product_Code as HSN,(Product_name+' '+specification) as Product_name, Quantity, sail_rate, discountRate, Service_tax_rate,Total_sail_rate1,Total_sail_rate2 from  tbl_Invoice_details where Quotation_no=@Quotation_no and  Invoice_No=@Invoice_No order by CAST(Sl_no as int)";
                SqlParameter[] pram = {
                                          new SqlParameter("@Quotation_no",lblqnumber.Text),
                                          new SqlParameter("@Invoice_No",lblinvno.Text)
                                      };
                dtp = DbCL.SPreturn_dt(cmdstring, pram);
                if (dtp.Rows.Count > 0)
                {
                    if (Session["cgstOrsgsti"].ToString() == "YES")
                    {
                        double TOTALCGST = 0;
                        double SUBTOTAL = 0;
                        double TOTALGST = 0;
                        double TOTALGSTPLUSAMO = 0;

                        ////strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        ////strp += "<tr>";
                        ////strp += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px; border:1px solid #bfbfbf; border-right:none; font-weight:bold'>SL NO</td>";
                        ////strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:7%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>HSN</td>";
                        ////strp += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:30%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>Particulars</td>";
                        ////strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>QNTY</td>";
                        ////strp += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>RATE</td>";
                        ////strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>GST</td>";
                        ////strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:14%; font-size:13px;border:1px solid #bfbfbf; border-right:none;'>";
                        ////strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        ////strp += "<tr>";
                        ////strp += "<td class='tdsty' style='text-align:center;padding:2px 0px 2px 0px; width:14%; font-size:13px;border-bottom:1px solid #bfbfbf;font-weight:bold' colspan='2'>CGST</td>";
                        ////strp += "</tr>";
                        ////strp += "<tr>";
                        ////strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border-right:1px solid #bfbfbf;font-weight:bold'>RATE</td>";
                        ////strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:9%; font-size:13px;font-weight:bold'>AMOUNT</td>";
                        ////strp += "</tr>";
                        ////strp += "</table>";
                        ////strp += "</td>";

                        ////strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:14%; font-size:13px;border:1px solid #bfbfbf; border-right:none;'>";
                        ////strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        ////strp += "<tr>";
                        ////strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:14%; font-size:13px;border-bottom:1px solid #bfbfbf;font-weight:bold' colspan='2'>SGST</td>";
                        ////strp += "</tr>";
                        ////strp += "<tr>";
                        ////strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border-right:1px solid #bfbfbf;font-weight:bold'>RATE</td>";
                        ////strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:9%; font-size:13px;font-weight:bold'>AMOUNT</td>";
                        ////strp += "</tr>";
                        ////strp += "</table>";
                        ////strp += "</td>";
                        ////strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;font-weight:bold'>AMOUNT</td>";
                        ////strp += "</tr>";
                        ////strp += "</table>";


                        //strp += "<table border='0' width='100%' class='Payment pagebrake'><tr><td class='' style='text-align: left; font-weight: bold;'></td></tr><tr><td class='' style=''>";
                        //strp += "<table class='PaymentPhase' style='border:0' width='100%'><tr><td class='gap' style=''>&nbsp</td></tr>";
                        //strp += "<tr>";

                        ////strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>S.NO</td>";

                        ////strp += "<td style='width:30%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>PARTICULARS</td>";
                        ////strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>HSN /<br> SAC <br> CODE</td>";
                        ////strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>QUANTITY<br>(PCS)</td>";
                        ////strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>RATE<br> (RS)</td>";
                        ////strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>GST</td>";

                        ////strp += "<td style='width:14%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;''>";
                        ////strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        ////strp += "<tr>";
                        ////strp += "<td style='width:14%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-right:none; font-weight: bold; background-color: #e31e24; text-align: center; color: white; ' colspan='2'>CGST</td>";
                        ////strp += "</tr>";
                        ////strp += "<tr>";
                        ////strp += "<td style='width:5%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-bottom:none;  font-weight: bold; background-color: #e31e24; text-align: center; color: white; '>RATE</td>";
                        ////strp += "<td style='width:9%;  font-weight: bold; background-color: #e31e24; text-align: center; color: white; '>AMOUNT</td>";
                        ////strp += "</tr>";
                        ////strp += "</table>";
                        ////strp += "</td>";

                        ////strp += "<td style='width:14%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>";
                        ////strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        ////strp += "<tr>";
                        ////strp += "<td style='width:14%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-right:none;  font-weight: bold; background-color: #e31e24; text-align: center; color: white; ' colspan='2'>SGST</td>";
                        ////strp += "</tr>";
                        ////strp += "<tr>";
                        ////strp += "<td style='width:5%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-bottom:none;  font-weight: bold; background-color: #e31e24; text-align: center; color: white; '>RATE</td>";
                        ////strp += "<td style='width:9%;  font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>AMOUNT</td>";
                        ////strp += "</tr>";
                        ////strp += "</table>";
                        ////strp += "</td>";

                        ////strp += "<td style='width:10%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>AMOUNT<br> (RS)</td>";

                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>S.NO</td>";
                        //strp += "<td style='width:28%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>PARTICULARS</td>";
                        //strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>HSN /<br> SAC <br> CODE</td>";
                        //strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>QUANTITY<br>(PCS)</td>";
                        //strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>RATE<br> (RS)</td>";

                        //strp += "<td style='width:6%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>DISCOUNT %</td>";

                        //strp += "<td style='width:6%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>GST</td>";

                        //strp += "<td style='width:12%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>";
                        //strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        //strp += "<tr><td style='width:12%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-right:none; font-weight: bold; background-color: #e31e24; text-align: center; color: white;' colspan='2'>CGST</td></tr>";
                        //strp += "<tr><td style='width:6%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-bottom:none; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>RATE</td>";
                        //strp += "<td style='width:6%; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>AMOUNT</td></tr>";
                        //strp += "</table></td>";

                        //strp += "<td style='width:12%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>";
                        //strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        //strp += "<tr><td style='width:12%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-right:none; font-weight: bold; background-color: #e31e24; text-align: center; color: white;' colspan='2'>SGST</td></tr>";
                        //strp += "<tr><td style='width:6%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-bottom:none; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>RATE</td>";
                        //strp += "<td style='width:6%; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>AMOUNT</td></tr>";
                        //strp += "</table></td>";

                        //strp += "<td style='width:10%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>AMOUNT<br> (RS)</td>";
                        //strp += "</tr>";
                        //strp += "</table>";


                        strp += "<table class='' style='border:2px solid #6c6c6c;' width='100%'>";

                        // Table Header
                        strp += "<tr style='background-color:#e31e24; color:white;'>";
                        strp += "<th style='border:2px solid #6c6c6c; text-align:center;'>S.NO</th>";
                        strp += "<th style='border:2px solid #6c6c6c; text-align:left;'>PARTICULARS</th>";
                        strp += "<th style='border:2px solid #6c6c6c; text-align:center;'>HSN/SAC CODE</th>";
                        strp += "<th style='border:2px solid #6c6c6c; text-align:center;'>QUANTITY (PCS)</th>";
                        strp += "<th style='border:2px solid #6c6c6c; text-align:center;'>RATE (RS)</th>";
                        strp += "<th style='border:2px solid #6c6c6c; text-align:center;'>DISCOUNT %</th>";
                        strp += "<th style='border:2px solid #6c6c6c; text-align:center;'>GST RATE</th>";
                        strp += "<th style='border:2px solid #6c6c6c; text-align:center;'>CGST RATE</th>";
                        strp += "<th style='border:2px solid #6c6c6c; text-align:center;'>CGST AMOUNT</th>";
                        strp += "<th style='border:2px solid #6c6c6c; text-align:center;'>SGST RATE</th>";
                        strp += "<th style='border:2px solid #6c6c6c; text-align:center;'>SGST AMOUNT</th>";
                        strp += "<th style='border:2px solid #6c6c6c; text-align:right;'>AMOUNT (RS)</th>";
                        strp += "</tr>";


                        for (int i = 0; i < dtp.Rows.Count; i++)
                        {
                            //string HSN = dtp.Rows[i]["HSN"].ToString();
                            //string Productname = dtp.Rows[i]["Product_name"].ToString();
                            //int Quantity = Convert.ToInt32(dtp.Rows[i]["Quantity"]);
                            //TQ = TQ + Quantity;
                            //double baserate = Math.Round((Convert.ToDouble(dtp.Rows[i]["sail_rate"])), 2);
                            //double discountrate = Math.Round((Convert.ToDouble(dtp.Rows[i]["discountRate"])), 2);
                            //int gstper = Convert.ToInt32(dtp.Rows[i]["Service_tax_rate"]);
                            //double QuantityBaserateAmo = Math.Round((Quantity * baserate), 2);
                            //double afterdiscountrate = QuantityBaserateAmo - Math.Round((QuantityBaserateAmo * discountrate)/100, 2);
                            //string QuantityBaserateAmo1 = DoFormat(afterdiscountrate);

                            //double gstamount = Math.Round(((afterdiscountrate * gstper) / 100), 2);
                            //double cgstper = Math.Round((Convert.ToDouble(gstper) / 2), 2);
                            //double cgstamo = Math.Round((Convert.ToDouble(gstamount) / 2), 2);

                            //TOTALCGST = TOTALCGST + cgstamo;
                            //SUBTOTAL = SUBTOTAL + afterdiscountrate;
                            //TOTALGST = TOTALGST + gstamount;

                            string HSN = dtp.Rows[i]["HSN"] != DBNull.Value ? dtp.Rows[i]["HSN"].ToString() : string.Empty;
                            string Productname = dtp.Rows[i]["Product_name"] != DBNull.Value ? dtp.Rows[i]["Product_name"].ToString() : string.Empty;

                            int Quantity = dtp.Rows[i]["Quantity"] != DBNull.Value && !string.IsNullOrEmpty(dtp.Rows[i]["Quantity"].ToString()) ? Convert.ToInt32(dtp.Rows[i]["Quantity"]) : 0;
                            TQ += Quantity;

                            double baserate = dtp.Rows[i]["sail_rate"] != DBNull.Value && !string.IsNullOrEmpty(dtp.Rows[i]["sail_rate"].ToString()) ? Math.Round(Convert.ToDouble(dtp.Rows[i]["sail_rate"]), 2) : 0.0;
                            double discountrate = dtp.Rows[i]["discountRate"] != DBNull.Value && !string.IsNullOrEmpty(dtp.Rows[i]["discountRate"].ToString()) ? Math.Round(Convert.ToDouble(dtp.Rows[i]["discountRate"]), 2) : 0.0;
                            int gstper = dtp.Rows[i]["Service_tax_rate"] != DBNull.Value && !string.IsNullOrEmpty(dtp.Rows[i]["Service_tax_rate"].ToString()) ? Convert.ToInt32(dtp.Rows[i]["Service_tax_rate"]): 0;

                            double QuantityBaserateAmo = Math.Round((Quantity * baserate), 2);
                            double afterdiscountrate = Math.Round(QuantityBaserateAmo - Math.Round((QuantityBaserateAmo * discountrate) / 100, 4), 2);
                            string QuantityBaserateAmo1 = DoFormat(afterdiscountrate);

                            double gstamount = Math.Round(((afterdiscountrate * gstper) / 100), 5);
                            double cgstper = Math.Round((Convert.ToDouble(gstper) / 2), 2);
                            double cgstamo = Math.Round((Convert.ToDouble(gstamount) / 2), 2);

                            TOTALCGST += cgstamo;
                            SUBTOTAL += afterdiscountrate;
                            TOTALGST += gstamount;


                            //strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                            //strp += "<tr>";
                            //strp += "<td class='tdsty'  style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px; border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + (i + 1).ToString() + "</td>";
                            //strp += "<td class='tdsty' style='text-align:center; padding:1px 0px 1px 0px; width:7%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + HSN + "</td>";
                            //strp += "<td class='tdsty'  style='text-align:center; padding:1px 0px 1px 0px; width:30%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + Productname + "</td>";
                            //strp += "<td class='tdsty' style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + Quantity.ToString() + "</td>";
                            //strp += "<td class='tdsty'  style='text-align:right; padding:1px 12px 1px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + baserate.ToString() + "</td>";
                            //strp += "<td class='tdsty' style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";

                            //strp += "<td class='tdsty'  style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + cgstper.ToString() + " %</td>";
                            //strp += "<td class='tdsty' style='text-align:right; padding:1px 12px 1px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + cgstamo.ToString() + "</td>";
                            //strp += "<td class='tdsty'  style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + cgstper.ToString() + " %</td>";
                            //strp += "<td class='tdsty' style='text-align:right; padding:1px 12px 1px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + cgstamo.ToString() + "</td>";

                            //strp += "<td class='tdsty' style='text-align:right; padding:1px 12px 1px 0px;; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;'>" + QuantityBaserateAmo.ToString() + "</td>";
                            //strp += "</tr>";
                            //strp += "</table>";

                            //strp += "<table class='' style='border:0' width='100%'>";
                            //strp += "<tr>";
                            //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'>" + (i + 1).ToString() + "</td>";                            
                            //strp += "<td style='width:30%; border:2px solid #6c6c6c; font-weight: bold;  text-align: left;   border-right:none; border-top:none;'>" + Productname + "</td>";
                            //strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + HSN + "</td>";
                            //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + Quantity.ToString() + "</td>";
                            //strp += "<td style='width:7%; border:2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + baserate.ToString() + "</td>";
                            //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";
                            //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + cgstper.ToString() + " %</td>";
                            //strp += "<td style='width:9%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + cgstamo.ToString() + "</td>";
                            //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + cgstper.ToString() + " %</td>";
                            //strp += "<td style='width:9%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + cgstamo.ToString() + "</td>";
                            //strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;   border-top:none;'>" + QuantityBaserateAmo1.ToString() + "</td>";
                            //strp += "</tr>";
                            //strp += "</table>";

                            //strp += "<table class='' style='border:0' width='100%'>";
                            //strp += "<tr>";
                            //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'>" + (i + 1).ToString() + "</td>";
                            //strp += "<td style='width:28%; border:2px solid #6c6c6c; font-weight: bold;  text-align: left;   border-right:none; border-top:none;'>" + Productname + "</td>";
                            //strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + HSN + "</td>";
                            //strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + Quantity.ToString() + "</td>";
                            //strp += "<td style='width:7%; border:2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + baserate.ToString() + "</td>";

                            //strp += "<td style='width:6%; border:2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + discountrate.ToString() + " %</td>";

                            //strp += "<td style='width:6%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";
                            //strp += "<td style='width:6%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + cgstper.ToString() + " %</td>";
                            //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + cgstamo.ToString() + "</td>";
                            //strp += "<td style='width:6%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + cgstper.ToString() + " %</td>";
                            //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + cgstamo.ToString() + "</td>";
                            //strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;   border-top:none;'>" + QuantityBaserateAmo1.ToString() + "</td>";
                            //strp += "</tr>";
                            //strp += "</table>";

                            strp += "<tr>";
                            strp += "<td style='border:2px solid #6c6c6c; text-align:center;'>" + (i + 1).ToString() + "</td>";
                            strp += "<td style='border:2px solid #6c6c6c; text-align:left;'>" + Productname + "</td>";
                            strp += "<td style='border:2px solid #6c6c6c; text-align:center;'>" + HSN + "</td>";
                            strp += "<td style='border:2px solid #6c6c6c; text-align:center;'>" + Quantity.ToString() + "</td>";
                            strp += "<td style='border:2px solid #6c6c6c; text-align:center;'>" + baserate.ToString() + "</td>";
                            strp += "<td style='border:2px solid #6c6c6c; text-align:center;'>" + discountrate.ToString() + "%</td>";
                            strp += "<td style='border:2px solid #6c6c6c; text-align:center;'>" + gstper.ToString() + "%</td>";
                            strp += "<td style='border:2px solid #6c6c6c; text-align:center;'>" + cgstper.ToString() + "%</td>";
                            strp += "<td style='border:2px solid #6c6c6c; text-align:center;'>" + cgstamo.ToString() + "</td>";
                            strp += "<td style='border:2px solid #6c6c6c; text-align:center;'>" + cgstper.ToString() + "%</td>";
                            strp += "<td style='border:2px solid #6c6c6c; text-align:center;'>" + cgstamo.ToString() + "</td>";
                            strp += "<td style='border:2px solid #6c6c6c; text-align:right;'>" + QuantityBaserateAmo1.ToString() + "</td>";
                            strp += "</tr>";

                        }

                        double GrosGST = Math.Ceiling(TOTALGST*100)/100;
                        TOTALGSTPLUSAMO = GrosGST + SUBTOTAL;
                        double gross = Math.Round((Convert.ToDouble(TOTALGSTPLUSAMO)), 2);
                        string grandtotal = gross.ToString();
                        string word = MoneyConvDS.MoneyConvFn(grandtotal);

                        //strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        //strp += "<tr>";
                        //strp += "<td class='tdsty'  style='text-align:center;padding:2px 0px 2px 0px; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;' colspan='7'></td>";
                        //strp += "<td class='tdsty' style='text-align:right; padding:2px 12px 2px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none; font-weight:bold'>" + TOTALCGST.ToString() + "</td>";
                        //strp += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'></td>";
                        //strp += "<td class='tdsty' style='text-align:right; padding:2px 12px 2px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none; font-weight:bold'>" + TOTALCGST.ToString() + "</td>";

                        //strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + SUBTOTAL.ToString() + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";

                        //strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        //strp += "<tr>";
                        //strp += "<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:90%; font-size:13px;border:0px;font-weight:bold' colspan='10'>TOTAL AMOUNT BEFORE TAX:</td>";
                        //strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + SUBTOTAL.ToString() + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";

                        //strp += "<table class='' style='border:0' width='100%'>";
                        //strp += "<tr>";


                        //strp += "<td style='border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;  border-right:none; border-top:none;background-color:#d9d3d3' colspan='3'>GRAND TOTAL</td>";

                        //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'>" + TQ.ToString() + "</td>";
                        //strp += "<td style='width:7%; border:2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";


                        //strp += "<td style='width:9%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'>" + TOTALCGST.ToString() + "</td>";
                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
                        //strp += "<td style='width:9%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'>" + TOTALCGST.ToString() + "</td>";

                        //strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;   border-top:none;background-color:#d9d3d3'>" + DoFormat(SUBTOTAL).ToString() + "</td>";

                        ////strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;   border-top:none;background-color:#d9d3d3'>" + Math.Round(SUBTOTAL,2).ToString() + ".00" + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";



                        //strp += "<table class='' style='border:0' width='100%'>";
                        //strp += "<tr>";
                        //strp += "<td style='font-weight: bold;  text-align: center' colspan='6'></td>";
                        //strp += "<td style='width:28%; font-weight: bold;  text-align: right; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT BEFORE TAX:</td>";
                        //strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none'>" + DoFormat(SUBTOTAL).ToString() + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";

                        ////strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        ////strp += "<tr>";
                        ////strp += "<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:90%; font-size:13px;border:0px;font-weight:bold' colspan='10'>TOTAL GST:</td>";
                        ////strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + TOTALGST.ToString() + "</td>";
                        ////strp += "</tr>";
                        ////strp += "</table>";

                        //strp += "<table class='' style='border:0' width='100%'>";
                        //strp += "<tr>";
                        //strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none' colspan='6'></td>";
                        //strp += "<td style='width:28%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL GST:</td>";
                        //strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + DoFormat(GrosGST).ToString() + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";

                        //if (lbldiscount!= "0.00")
                        //{
                        //    string discount_amount = lbldiscount.ToString();
                        //    decimal discount_amount1 = Convert.ToDecimal(discount_amount);
                        //    if (discount_amount1 != 0)
                        //    {
                        //        //strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        //        //strp += "<tr>";
                        //        //strp += "<td class='tdsty'  style='text-align:right; padding:3px 15px 3px 0px;  font-size:13px;border:0px;font-weight:bold' colspan='8'>DISCOUNT AMOUNT:</td>";
                        //        //strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 15px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + lbldiscount.Text.ToString() + "</td>";
                        //        //strp += "</tr>";
                        //        //strp += "</table>";

                        //        strp += "<table class='' style='border:0' width='100%'>";
                        //        strp += "<tr>";
                        //        strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none' colspan='6'></td>";
                        //        strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>DISCOUNT AMOUNT:</td>";
                        //        strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + lbldiscount.ToString() + "</td>";
                        //        strp += "</tr>";
                        //        strp += "</table>";
                        //    }
                        //}

                        ////strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        ////strp += "<tr>";
                        ////strp += "<td class='tdsty'  style='text-align:left; padding:3px 15px 3px 15px; width:62%; font-size:13px;border:0px;font-weight:bold;' bgcolor='#dbe5f1'colspan='6'>Amount (In Words):" + word + "</td>";
                        ////strp += "<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:28%;  font-size:13px;border:0px;font-weight:bold' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
                        ////strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + lblnetamount.Text.ToString() + "</td>";
                        ////strp += "</tr>";
                        ////strp += "</table>";

                        //strp += "<table class='' style='border:0' width='100%'>";
                        //strp += "<tr>";
                        //strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='5'>Amount (In Words): " + word + "</td>";
                        //strp += "<td style='width:5%; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'></td>";
                        //strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
                        //strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + DoFormat(gross).ToString() + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";

                        //strp += "</td></tr><tr><td class='gap' style=''>&nbsp</td></tr></table>";

                        //strp += "<table class='' style='border:0' width='100%'>";
                        //strp += "<tr>";
                        //strp += "<td style='border: 2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none; background-color:#d9d3d3' colspan='3'>GRAND TOTAL</td>";

                        //strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none; background-color:#d9d3d3'>" + TQ.ToString() + "</td>";
                        //strp += "<td style='width:7%; border:2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none; background-color:#d9d3d3'></td>";

                        //strp += "<td style='width:6%; border:2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none; background-color:#d9d3d3'></td>";
                        //strp += "<td style='width:6%; border:2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none; background-color:#d9d3d3'></td>";
                        //strp += "<td style='width:6%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none; background-color:#d9d3d3'></td>";
                        //strp += "<td style='width:6%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none; background-color:#d9d3d3'></td>";

                        //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none; background-color:#d9d3d3'>" + TOTALCGST.ToString() + "</td>";
                        //strp += "<td style='width:6%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none; background-color:#d9d3d3'></td>";
                        //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none; background-color:#d9d3d3'>" + TOTALCGST.ToString() + "</td>";

                        //strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold; text-align: right; border-top:none; background-color:#d9d3d3'>" + DoFormat(SUBTOTAL).ToString() + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";

                        //strp += "<table class='' style='border:0' width='100%'>";
                        //strp += "<tr>";
                        //strp += "<td style='font-weight: bold; text-align: center' colspan='6'></td>";
                        //strp += "<td style='width:28%; font-weight: bold; text-align: right; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT BEFORE TAX:</td>";
                        //strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold; text-align: right; border-top:none'>" + DoFormat(SUBTOTAL).ToString() + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";

                        //strp += "<table class='' style='border:0' width='100%'>";
                        //strp += "<tr>";
                        //strp += "<td style='font-weight: bold; text-align: center; border-right:none; border-top:none' colspan='6'></td>";
                        //strp += "<td style='width:28%; border:1px solid #bfbfbf; font-weight: bold; text-align: right; border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL GST:</td>";
                        //strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold; text-align: right; border-top:none;'>" + DoFormat(GrosGST).ToString() + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";

                        //strp += "<table class='' style='border:0' width='100%'>";
                        //strp += "<tr>";
                        //strp += "<td style='font-weight: bold; text-align: center; border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='5'>Amount (In Words): " + word + "</td>";
                        //strp += "<td style='width:5%; font-weight: bold; text-align: center; border-right:none; border-top:none;'></td>";
                        //strp += "<td style='width:28%; font-weight: bold; text-align: right; border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
                        //strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold; text-align: right; border-top:none;'>" + DoFormat(gross).ToString() + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";

                        //strp += "</td></tr><tr><td class='gap' style=''>&nbsp</td></tr></table>";

                        // **Grand Total Row**
                        strp += "<tr style='background-color:#d9d3d3; font-weight:bold;'>";
                        strp += "<td colspan='3' style='border:2px solid #6c6c6c; text-align:center;'>GRAND TOTAL</td>";
                        strp += "<td style='border:2px solid #6c6c6c; text-align:center;'>" + TQ.ToString() + "</td>";
                        strp += "<td style='border:2px solid #6c6c6c; text-align:center;'></td>";  // Empty Rate Column
                        strp += "<td style='border:2px solid #6c6c6c; text-align:center;'></td>";
                        strp += "<td style='border:2px solid #6c6c6c; text-align:center;'></td>";
                        strp += "<td style='border:2px solid #6c6c6c; text-align:center;'></td>";
                        strp += "<td style='border:2px solid #6c6c6c; text-align:center;'>" + TOTALCGST + "</td>";
                        strp += "<td style='border:2px solid #6c6c6c; text-align:center;'></td>";
                        strp += "<td style='border:2px solid #6c6c6c; text-align:center;'>" + TOTALCGST + "</td>";
                        strp += "<td style='border:2px solid #6c6c6c; text-align:right;'>" + DoFormat(SUBTOTAL) + "</td>";
                        strp += "</tr>";

                        // **Total Amount Before Tax**
                        strp += "<tr style='color:white; font-weight:bold;'>";
                        strp += "<td style='font-weight: bold;color: white;' colspan='8'></td>";
                        strp += "<td colspan='3' style='border:2px solid #6c6c6c; background-color:#e31e24; text-align:right;'>TOTAL AMOUNT BEFORE TAX:</td>";
                        strp += "<td style='border:2px solid #6c6c6c; background-color:#e31e24; text-align:right;'>" + DoFormat(SUBTOTAL) + "</td>";
                        strp += "</tr>";

                        // **Total GST**
                        strp += "<tr style='color:white; font-weight:bold;'>";
                        strp += "<td style='font-weight: bold; color: white;' colspan='8'></td>";
                        strp += "<td colspan='3' style='border:2px solid #6c6c6c; background-color:#e31e24; text-align:right;'>TOTAL GST:</td>";
                        strp += "<td style='border:2px solid #6c6c6c; background-color:#e31e24; text-align:right;'>" + DoFormat(GrosGST) + "</td>";
                        strp += "</tr>";

                        // **Total Amount After Tax**
                        strp += "<tr style='color:white; font-weight:bold;'>";
                        strp += "<td colspan='6' style='background-color:#e31e24; text-align:left;'>Amount (In Words): " + word + "</td>";
                        strp += "<td style='font-weight: bold; text-align: right; color: white;' colspan='2'></td>";
                        strp += "<td colspan='3' style='border:2px solid #6c6c6c; background-color:#e31e24; text-align:right;'>TOTAL AMOUNT AFTER TAX:</td>";
                        strp += "<td style='border:2px solid #6c6c6c; background-color:#e31e24; text-align:right;'>" + DoFormat(gross) + "</td>";
                        strp += "</tr>";

                        strp += "</table>";


                    }
                    if (Session["igsti"].ToString() == "YES")
                    {

                        double TOTALIGST = 0;
                        double SUBTOTAL = 0;
                        double TOTALGST = 0;
                        double TOTALGSTPLUSAMO = 0;

                        //strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        //strp += "<tr>";
                        //strp += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px; border:1px solid #bfbfbf; border-right:none; font-weight:bold'>SL NO</td>";
                        //strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:7%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>HSN</td>";
                        //strp += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:44%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>Particulars</td>";
                        //strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>QNTY</td>";
                        //strp += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>RATE</td>";
                        //strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>GST</td>";
                        //strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:14%; font-size:13px;border:1px solid #bfbfbf; border-right:none;'>";
                        //strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        //strp += "<tr>";
                        //strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:14%; font-size:13px;border-bottom:1px solid #bfbfbf;font-weight:bold' colspan='2'>IGST</td>";
                        //strp += "</tr>";
                        //strp += "<tr>";
                        //strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border-right:1px solid #bfbfbf;font-weight:bold'>RATE</td>";
                        //strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:9%; font-size:13px;font-weight:bold'>AMOUNT</td>";
                        //strp += "</tr>";
                        //strp += "</table>";
                        //strp += "</td>";


                        //strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;font-weight:bold'>AMOUNT</td>";
                        //strp += "</tr>";
                        //strp += "</table>";


                        ////------------Below is commented on 27-Mar-2025-----------------
                        ////strp += "<table border='0' width='100%'><tr><td class='' style='text-align: left; font-weight: bold;'></td></tr><tr><td class='' style=''>";
                        ////strp += "<table class='PaymentPhase' style='border:0' width='100%'><tr><td class='gap' style=''>&nbsp</td></tr>";
                        ////strp += "<tr>";
                        ////strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>S.NO</td>";

                        ////strp += "<td style='width:44%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>PARTICULARS</td>";
                        ////strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>HSN /<br>SAC <br> CODE</td>";

                        ////strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>QUANTITY<br> (PCS)</td>";
                        ////strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>RATE<br> (RS)</td>";
                        ////strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>GST</td>";

                        ////strp += "<td style='width:14%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;''>";
                        ////strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        ////strp += "<tr>";
                        ////strp += "<td style='width:14%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-right:none; font-weight: bold; background-color: #e31e24; text-align: center; color: white; ' colspan='2'>IGST</td>";
                        ////strp += "</tr>";
                        ////strp += "<tr>";
                        ////strp += "<td style='width:5%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-bottom:none;  font-weight: bold; background-color: #e31e24; text-align: center; color: white; '>RATE</td>";
                        ////strp += "<td style='width:9%;  font-weight: bold; background-color: #e31e24; text-align: center; color: white; '>AMOUNT</td>";
                        ////strp += "</tr>";
                        ////strp += "</table>";
                        ////strp += "</td>";

                        ////strp += "<td style='width:10%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>AMOUNT<br> (RS)</td>";
                        ////strp += "</tr>";
                        ////strp += "</table>";
                        ////------------Above is commented on 27-Mar-2025-----------------

                        //strp += "<table border='0' width='100%'><tr><td class='' style='text-align: left; font-weight: bold;'></td></tr><tr><td class='' style=''>";
                        //strp += "<table class='PaymentPhase' style='border:0' width='100%'><tr><td class='gap' style=''>&nbsp</td></tr>";
                        //strp += "<tr>";
                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>S.NO</td>";
                        //strp += "<td style='width:40%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>PARTICULARS</td>";
                        //strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>HSN /<br>SAC <br> CODE</td>";
                        //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>QUANTITY<br> (PCS)</td>";
                        //strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>RATE<br> (RS)</td>";
                        //strp += "<td style='width:6%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>DISCOUNT %</td>";
                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>GST</td>";
                        //strp += "<td style='width:12%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;''>";
                        //strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        //strp += "<tr>";
                        //strp += "<td style='width:12%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-right:none; font-weight: bold; background-color: #e31e24; text-align: center; color: white; ' colspan='2'>IGST</td>";
                        //strp += "</tr>";
                        //strp += "<tr>";
                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-bottom:none;  font-weight: bold; background-color: #e31e24; text-align: center; color: white; '>RATE</td>";
                        //strp += "<td style='width:7%;  font-weight: bold; background-color: #e31e24; text-align: center; color: white; '>AMOUNT</td>";
                        //strp += "</tr>";
                        //strp += "</table>";
                        //strp += "</td>";
                        //strp += "<td style='width:10%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>AMOUNT<br> (RS)</td>";
                        //strp += "</tr>";
                        //strp += "</table>";

                        strp += "<table class='' style='border:2px solid #6c6c6c;' width='100%'>";

                        strp += "<tr>";
                        strp += "<td style='width:5%; border:2px solid #6c6c6c; font-weight:bold; background-color:#e31e24; text-align:center; color:white; border-right:none;'>S.NO</td>";
                        strp += "<td style='width:34%; border:2px solid #6c6c6c; font-weight:bold; background-color:#e31e24; text-align:center; color:white; border-right:none;'>PARTICULARS</td>";
                        strp += "<td style='width:7%; border:2px solid #6c6c6c; font-weight:bold; background-color:#e31e24; text-align:center; color:white; border-right:none;'>HSN/SAC CODE</td>";
                        strp += "<td style='width:7%; border:2px solid #6c6c6c; font-weight:bold; background-color:#e31e24; text-align:center; color:white; border-right:none;'>QUANTITY (PCS)</td>";
                        strp += "<td style='width:7%; border:2px solid #6c6c6c; font-weight:bold; background-color:#e31e24; text-align:center; color:white; border-right:none;'>RATE (RS)</td>";
                        strp += "<td style='width:6%; border:2px solid #6c6c6c; font-weight:bold; background-color:#e31e24; text-align:center; color:white; border-right:none;'>DISCOUNT (%)</td>";
                        strp += "<td style='width:5%; border:2px solid #6c6c6c; font-weight:bold; background-color:#e31e24; text-align:center; color:white; border-right:none;'>GST (%)</td>";
                        strp += "<td style='width:5%; border:2px solid #6c6c6c; font-weight:bold; background-color:#e31e24; text-align:center; color:white; border-right:none;'>IGST RATE(%)</td>";
                        strp += "<td style='width:8%; border:2px solid #6c6c6c; font-weight:bold; background-color:#e31e24; text-align:center; color:white; border-right:none;'>IGST AMOUNT</td>";
                        strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight:bold; background-color:#e31e24; text-align:center; color:white;'>AMOUNT (RS)</td>";
                        strp += "</tr>";


                        for (int i = 0; i < dtp.Rows.Count; i++)
                        {

                            //string HSN = dtp.Rows[i]["HSN"].ToString();
                            //string Productname = dtp.Rows[i]["Product_name"].ToString();
                            //int Quantity = Convert.ToInt32(dtp.Rows[i]["Quantity"]);
                            //TQ = TQ + Quantity;
                            //double baserate = Math.Round((Convert.ToDouble(dtp.Rows[i]["sail_rate"])), 2);
                            //int gstper = Convert.ToInt32(dtp.Rows[i]["Service_tax_rate"]);
                            //double QuantityBaserateAmo = Math.Round((Quantity * baserate), 2);

                            //double discountrate = dtp.Rows[i]["discountRate"] != DBNull.Value && !string.IsNullOrEmpty(dtp.Rows[i]["discountRate"].ToString()) ? Math.Round(Convert.ToDouble(dtp.Rows[i]["discountRate"]), 2) : 0.0;
                            //string QuantityBaserateAmo1 = DoFormat(QuantityBaserateAmo);

                            //double gstamount = Math.Round(((QuantityBaserateAmo * gstper) / 100), 2);
                            //double cgstper = Math.Round((Convert.ToDouble(gstper) / 2), 2);
                            //double cgstamo = Math.Round((Convert.ToDouble(gstamount) / 2), 2);

                            //TOTALIGST = TOTALIGST + gstamount;
                            //SUBTOTAL = SUBTOTAL + QuantityBaserateAmo;
                            //TOTALGST = TOTALGST + gstamount;

                            string HSN = dtp.Rows[i]["HSN"] != DBNull.Value ? dtp.Rows[i]["HSN"].ToString() : string.Empty;
                            string Productname = dtp.Rows[i]["Product_name"] != DBNull.Value ? dtp.Rows[i]["Product_name"].ToString() : string.Empty;
                            int Quantity = dtp.Rows[i]["Quantity"] != DBNull.Value && !string.IsNullOrEmpty(dtp.Rows[i]["Quantity"].ToString()) ? Convert.ToInt32(dtp.Rows[i]["Quantity"]) : 0;
                            TQ += Quantity;

                            double baserate = dtp.Rows[i]["sail_rate"] != DBNull.Value && !string.IsNullOrEmpty(dtp.Rows[i]["sail_rate"].ToString()) ? Math.Round(Convert.ToDouble(dtp.Rows[i]["sail_rate"]), 2) : 0.0;
                            double discountrate = dtp.Rows[i]["discountRate"] != DBNull.Value && !string.IsNullOrEmpty(dtp.Rows[i]["discountRate"].ToString()) ? Math.Round(Convert.ToDouble(dtp.Rows[i]["discountRate"]), 2) : 0.0;
                            int gstper = dtp.Rows[i]["Service_tax_rate"] != DBNull.Value && !string.IsNullOrEmpty(dtp.Rows[i]["Service_tax_rate"].ToString()) ? Convert.ToInt32(dtp.Rows[i]["Service_tax_rate"]) : 0;

                            double QuantityBaserateAmo = Math.Round((Quantity * baserate), 2);
                            double afterdiscountrate = Math.Round(QuantityBaserateAmo - Math.Round((QuantityBaserateAmo * discountrate) / 100, 4), 2);
                            string QuantityBaserateAmo1 = DoFormat(afterdiscountrate);

                            double gstamount = Math.Round(((afterdiscountrate * gstper) / 100), 5); // Corrected GST calculation based on discount
                            TOTALIGST += gstamount;
                            SUBTOTAL += afterdiscountrate;
                            TOTALGST += gstamount;

                            //strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                            //strp += "<tr>";
                            //strp += "<td class='tdsty'  style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px; border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + (i + 1).ToString() + "</td>";
                            //strp += "<td class='tdsty' style='text-align:center; padding:1px 0px 1px 0px; width:7%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + HSN + "</td>";
                            //strp += "<td class='tdsty'  style='text-align:center; padding:1px 0px 1px 0px; width:44%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + Productname + "</td>";
                            //strp += "<td class='tdsty' style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + Quantity.ToString() + "</td>";
                            //strp += "<td class='tdsty'  style='text-align:right; padding:1px 12px 1px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + baserate.ToString() + "</td>";
                            //strp += "<td class='tdsty' style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";
                            //strp += "<td class='tdsty'  style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";
                            //strp += "<td class='tdsty' style='text-align:right; padding:1px 12px 1px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + gstamount.ToString() + "</td>";
                            //strp += "<td class='tdsty' style='text-align:right; padding:1px 12px 1px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;'>" + QuantityBaserateAmo.ToString() + "</td>";
                            //strp += "</tr>";
                            //strp += "</table>";

                            ////strp += "<table class='' style='border:0' width='100%'>";
                            ////strp += "<tr>";
                            ////strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + (i + 1).ToString() + "</td>";
                            ////strp += "<td style='width:44%; border:2px solid #6c6c6c; font-weight: bold;  text-align: left;     border-right:none; border-top:none;'>" + Productname + "</td>";
                            ////strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + HSN + "</td>";
                            ////strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + Quantity.ToString() + "</td>";
                            ////strp += "<td style='width:7%; border:2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + baserate.ToString() + "</td>";
                            ////strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";
                            ////strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";
                            ////strp += "<td style='width:9%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + gstamount.ToString() + "</td>";
                            ////strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;   border-top:none;'>" + QuantityBaserateAmo1.ToString() + "</td>";
                            ////strp += "</tr>";
                            ////strp += "</table>";

                            //strp += "<table class='' style='border:0' width='100%'>";
                            //strp += "<tr>";
                            //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none;'>" + (i + 1).ToString() + "</td>";
                            //strp += "<td style='width:40%; border:2px solid #6c6c6c; font-weight: bold; text-align: left; border-right:none; border-top:none;'>" + Productname + "</td>";
                            //strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none;'>" + HSN + "</td>";
                            //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none;'>" + Quantity.ToString() + "</td>";
                            //strp += "<td style='width:7%; border:2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none;'>" + baserate.ToString() + "</td>";
                            //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none;'>" + discountrate.ToString() + " %</td>";
                            //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";
                            //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";
                            //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none;'>" + gstamount.ToString() + "</td>";
                            //strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold; text-align: right; border-top:none;'>" + QuantityBaserateAmo1.ToString() + "</td>";
                            //strp += "</tr>";
                            //strp += "</table>";

                            strp += "<tr>";
                            strp += "<td style='width:5%; border:2px solid #6c6c6c; text-align:center; border-right:none; border-top:none;'>" + (i + 1).ToString() + "</td>";
                            strp += "<td style='width:34%; border:2px solid #6c6c6c; text-align:left; border-right:none; border-top:none;'>" + Productname + "</td>";
                            strp += "<td style='width:7%; border:2px solid #6c6c6c; text-align:center; border-right:none; border-top:none;'>" + HSN + "</td>";
                            strp += "<td style='width:7%; border:2px solid #6c6c6c; text-align:center; border-right:none; border-top:none;'>" + Quantity.ToString() + "</td>";
                            strp += "<td style='width:7%; border:2px solid #6c6c6c; text-align:center; border-right:none; border-top:none;'>" + baserate.ToString() + "</td>";
                            strp += "<td style='width:6%; border:2px solid #6c6c6c; text-align:center; border-right:none; border-top:none;'>" + discountrate.ToString() + "</td>";
                            strp += "<td style='width:5%; border:2px solid #6c6c6c; text-align:center; border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";
                            strp += "<td style='width:5%; border:2px solid #6c6c6c; text-align:center; border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>"; // **Added duplicate GST column**
                            strp += "<td style='width:8%; border:2px solid #6c6c6c; text-align:center; border-right:none; border-top:none;'>" + gstamount.ToString() + "</td>";
                            strp += "<td style='width:10%; border:2px solid #6c6c6c; text-align:right; border-top:none;'>" + QuantityBaserateAmo1.ToString() + "</td>";
                            strp += "</tr>";

                        }

                        //TOTALGSTPLUSAMO = TOTALGST + SUBTOTAL;
                        //string grandtotal = (Math.Round((Convert.ToDouble(TOTALGSTPLUSAMO))).ToString());
                        //string word = MoneyConvDS.MoneyConvFn(lblnetamount);
                        double GrosGST = Math.Ceiling(TOTALGST * 100) / 100;  // Rounded GST to nearest 2 decimal places
                        TOTALGSTPLUSAMO = GrosGST + SUBTOTAL;

                        double gross = Math.Round(Convert.ToDouble(TOTALGSTPLUSAMO), 2);
                        string grandtotal = gross.ToString();
                        string word = MoneyConvDS.MoneyConvFn(grandtotal);

                        //strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        //strp += "<tr>";
                        //strp += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px;  font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;font-weight:bold' colspan='7'></td>";
                        //strp += "<td class='tdsty' style='text-align:right; padding:2px 12px 2px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;font-weight:bold'>" + TOTALIGST.ToString() + "</td>";
                        //strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + SUBTOTAL + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";

                        ////strp += "<table class='' style='border:0' width='100%'>";
                        ////strp += "<tr>";                       
                        ////strp += "<td style='border:1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none; background-color:#d9d3d3' colspan='3'>GRAND TOTAL</td>";
                        ////strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'>" + TQ.ToString() + "</td>";
                        ////strp += "<td style='width:7%; border:2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
                        ////strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
                        ////strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
                        ////strp += "<td style='width:9%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'>" + TOTALIGST.ToString() + "</td>";
                        ////strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;   border-top:none;background-color:#d9d3d3'>" + Math.Round(SUBTOTAL).ToString() + ".00" + "</td>";
                        ////strp += "</tr>";
                        ////strp += "</table>";

                        //strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        //strp += "<tr>";
                        //strp += "<td class='tdsty'  style='text-align:right; padding:3px 15px 3px 0px;  font-size:13px;border:0px;font-weight:bold' colspan='8'>TOTAL AMOUNT BEFORE TAX:</td>";
                        //strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + SUBTOTAL.ToString() + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";

                        ////strp += "<table class='' style='border:0' width='100%'>";
                        ////strp += "<tr>";
                        ////strp += "<td style='font-weight: bold;  text-align: center' colspan='6'></td>";
                        ////strp += "<td style='width:28%; font-weight: bold;  text-align: right; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT BEFORE TAX:</td>";
                        ////strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none'>" + Math.Round(SUBTOTAL).ToString() + ".00" + "</td>";
                        ////strp += "</tr>";
                        ////strp += "</table>";

                        //strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        //strp += "<tr>";
                        //strp += "<td class='tdsty'  style='text-align:right; padding:3px 15px 3px 0px;  font-size:13px;border:0px;font-weight:bold' colspan='8'>TOTAL GST:</td>";
                        //strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + TOTALGST.ToString() + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";

                        // Grand Total Row (Fixed)
                        //strp += "<table class='' style='border:0' width='100%'>";
                        //strp += "<tr>";
                        //strp += "<td style='border:1px solid #bfbfbf; font-weight: bold; text-align: center; border-right:none; border-top:none; background-color:#d9d3d3' colspan='4'>GRAND TOTAL</td>";
                        //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none; background-color:#d9d3d3'>" + TQ.ToString() + "</td>";
                        //strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none; background-color:#d9d3d3'></td>";
                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none; background-color:#d9d3d3'></td>";
                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none; background-color:#d9d3d3'></td>";
                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none; background-color:#d9d3d3'></td>";
                        //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; border-right:none; border-top:none; background-color:#d9d3d3'>" + TOTALIGST.ToString() + "</td>";
                        //strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold; text-align: right; border-top:none; background-color:#d9d3d3'>" + Math.Round(SUBTOTAL).ToString() + ".00" + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";


                        // Total Amount Before Tax Row (Fixed)
                        //strp += "<table class='' style='border:0' width='100%'>";
                        //strp += "<tr>";
                        //strp += "<td style='font-weight: bold; text-align: center' colspan='7'></td>";
                        //strp += "<td style='width:23%; font-weight: bold; text-align: right; background-color: #e31e24; color: white;' colspan='3'>TOTAL AMOUNT BEFORE TAX:</td>";
                        //strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold; text-align: right; border-top:none'>" + Math.Round(SUBTOTAL).ToString() + ".00" + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";

                        // Total GST Row (Fixed)
                        //strp += "<table class='' style='border:0' width='100%'>";
                        //strp += "<tr>";
                        //strp += "<td style='font-weight: bold; text-align: center; border-right:none; border-top:none' colspan='7'></td>";
                        //strp += "<td style='width:23%; border:1px solid #bfbfbf; font-weight: bold; text-align: right; border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='3'>TOTAL GST:</td>";
                        //strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold; text-align: right; border-top:none;'>" + Math.Round(TOTALGST).ToString() + ".00" + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";

                        ////strp += "<table class='' style='border:0' width='100%'>";
                        ////strp += "<tr>";
                        ////strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none' colspan='6'></td>";
                        ////strp += "<td style='width:28%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL GST:</td>";
                        ////strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + Math.Round(TOTALGST).ToString() + ".00" + "</td>";
                        ////strp += "</tr>";
                        ////strp += "</table>";

                        if (lbldiscount!= "0.00")
                        {
                            string discount_amount = lbldiscount;
                            decimal discount_amount1 = Convert.ToDecimal(discount_amount);
                            if (discount_amount1 != 0)
                            {
                                //strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                                //strp += "<tr>";
                                //strp += "<td class='tdsty'  style='text-align:right; padding:3px 15px 3px 0px;  font-size:13px;border:0px;font-weight:bold' colspan='8'>DISCOUNT AMOUNT:</td>";
                                //strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + lbldiscount.Text.ToString() + "</td>";
                                //strp += "</tr>";
                                //strp += "</table>";

                                strp += "<table class='' style='border:0' width='100%'>";
                                strp += "<tr>";
                                strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none' colspan='6'></td>";
                                strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>DISCOUNT AMOUNT:</td>";
                                strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + lbldiscount.ToString() + ".00"+ "</td>";
                                strp += "</tr>";
                                strp += "</table>";
                            }
                        }

                        //strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        //strp += "<tr>";
                        //strp += "<td class='tdsty'  style='text-align:justify; padding:5px 15px 5px 15px; width:61%;  font-size:13px;border:0px;font-weight:bold' bgcolor='#dbe5f1' colspan='4'>Amount (In Words):" + word + " </td>";
                        //strp += "<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 15px; font-size:13px;border:0px;font-weight:bold' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
                        //strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + lblnetamount.Text.ToString() + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";

                        // Total Amount After Tax Row (Fixed)
                        //strp += "<table class='' style='border:0' width='100%'>";
                        //strp += "<tr>";
                        //strp += "<td style='font-weight: bold; text-align: left; border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='6'>Amount (In Words): " + word + "</td>";
                        //strp += "<td style='width:23%; font-weight: bold; text-align: right; border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='3'>TOTAL AMOUNT AFTER TAX:</td>";
                        //strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold; text-align: right; border-top:none;'>" + lblnetamount.ToString() + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";
                        //strp += "</td></tr><tr><td class='gap' style=''>&nbsp;</td></tr></table>";

                        ////strp += "<table class='' style='border:0' width='100%'>";
                        ////strp += "<tr>";
                        ////strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='5'>Amount (In Words): " + word + "</td>";
                        ////strp += "<td style='width:5%; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'></td>";
                        ////strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
                        ////strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + lblnetamount.ToString() + "</td>";
                        ////strp += "</tr>";
                        ////strp += "</table>";

                        ////strp += "</td></tr><tr><td class='gap' style=''>&nbsp</td></tr></table>";

                        // **Grand Total Row**
                        strp += "<tr>";
                        strp += "<td style='border:1px solid #bfbfbf; font-weight:bold; text-align:center; border-right:none; border-top:none; background-color:#d9d3d3' colspan='3'>GRAND TOTAL</td>";
                        strp += "<td style='width:7%; border:2px solid #6c6c6c; font-weight:bold; text-align:center; border-right:none; border-top:none;background-color:#d9d3d3'>" + TQ.ToString() + "</td>";
                        strp += "<td colspan='6' style='border:2px solid #6c6c6c; font-weight:bold; text-align:right; border-top:none;background-color:#d9d3d3'>" + DoFormat(SUBTOTAL) + "</td>";
                        strp += "</tr>";

                        // **Total Amount Before Tax**
                        strp += "<tr>";
                        strp += "<td colspan='5'></td>";
                        strp += "<td style='width:28%; font-weight:bold; text-align:right; background-color:#e31e24; color:white;' colspan='4'>TOTAL AMOUNT BEFORE TAX:</td>";
                        strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight:bold; text-align:right; border-top:none'>" + DoFormat(SUBTOTAL).ToString() + "</td>";
                        strp += "</tr>";

                        // **Total GST**
                        strp += "<tr>";
                        strp += "<td colspan='5'></td>";
                        strp += "<td style='width:28%; font-weight:bold; text-align:right; background-color:#e31e24; color:white;' colspan='4'>TOTAL GST:</td>";
                        strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight:bold; text-align:right; border-top:none'>" + DoFormat(GrosGST).ToString() + "</td>";
                        strp += "</tr>";

                        // **Total Amount After Tax**
                        strp += "<tr>";
                        strp += "<td colspan='5' style='font-weight:bold; text-align:left; background-color:#e31e24; color:white;'>Amount (In Words): " + word + "</td>";
                        strp += "<td style='width:28%; font-weight:bold; text-align:right; background-color:#e31e24; color:white;' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
                        strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight:bold; text-align:right; border-top:none'>" + DoFormat(gross).ToString() + "</td>";
                        strp += "</tr>";

                        strp += "</table>"; // Closing table
                    }
                    lblserviceamo.Text = strp.ToString();
                }
            }
        }

        public static string DoFormat(double myNumber)
        {
            var s = string.Format("{0:0.00}", myNumber);

            //if (s.EndsWith("00"))
            //{
            //    return ((int)myNumber).ToString();
            //}
            //else
            //{
            return s;
            //}
        }

        private void bindVatdetails()
        {
            string query = "select ('Vat @ '+ Vat_rate +' %') as rete,Vat_amount from tbl_quotation_vat where Quotation_no=@Quotation_no order by Id";
            SqlParameter[] pram = { new SqlParameter("@Quotation_no",lblqnumber.Text)};
            DataTable dtvat = new DataTable();
            dtvat = DbCL.SPreturn_dt(query, pram);
            if (dtvat.Rows.Count>0)
            {
                for (int i=0; i< dtvat.Rows.Count; i++)
                {
                    string rete = dtvat.Rows[i]["rete"].ToString();
                    string Vat_amount = dtvat.Rows[i]["Vat_amount"].ToString();

                    strbackdt.Append("<table class='' style='border:0' width='100%'>");
                    strbackdt.Append("<tr>");
                    strbackdt.Append("<td style='font-weight: bold;  text-align: center' colspan='4'></td>");
                    strbackdt.Append("<td style='width:14%; font-weight: bold;  text-align: right; background-color: #e31e24; color: white;'>"+ rete + ":</td>");
                    strbackdt.Append("<td style='width:10%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;  border-top:none'>" + Vat_amount + "</td>");
                    strbackdt.Append("</tr>");
                    strbackdt.Append("</table>");
                }
            }
        }

        private string statusvalue(string qno)
        {
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
                //lbltaxstring.Text = "GST Registration No: ";
                //lbltaxno.Text = "19AAEFI5315E1ZL";
                //lblClientVat.Text = "Buyer's GST No: " + gstno;
            }
            else
            {
                status_value = "NO";
                PnlTaxKvqa.Visible = true;
                PnlGstKvqa.Visible = false;
                //lbltaxstring.Text = "Vat No: ";
                //lbltaxno.Text = "19629770012";
                //lblClientVat.Text = "Buyer's Vat No: " + vatno;
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