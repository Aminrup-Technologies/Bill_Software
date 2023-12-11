using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Bill_Software.corporate.business.print
{
    public partial class NewPaymentInvoiceDuplicate : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        public string vatno = "";
        public string gstno = "";
        public string strp = "";
        public string PAYAMO = "";
        DataTable dtp = new DataTable();
        DataTable dtci = new DataTable();
        DataTable dtBacdata = new DataTable();
        DataTable dtRep = new DataTable();
        DataTable dtChadd = new DataTable();
        double TOTALGSTPLUSAMO = 0;
        StringBuilder strbackdt = new StringBuilder();
        StringBuilder paymentreceived = new StringBuilder();

        string lblSubtotal = "", lblstax = "", lblstax0 = "", lblnetamount = "", lbldue = "", lbldiscount = "", lblword = "", lblpayment = "";
        public int TQ = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            string Payment_ID = Request.QueryString["Payment_ID"];

            buindalldata(Payment_ID);


            //BindVatamount();
            //Bindtaxdata();
        }

        
        
        protected void Button2_Click(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {

        }

        private void buindpaymentamount(string iddetails, string invno)
        {

            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select sum(cast(Given_amount as real)) as amount from tbl_invoice_payment where Quotation_No='" + invno + "' and ID<=" + iddetails + "";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblpayment = re["amount"].ToString();
                //lbldue = (Convert.ToInt32(TOTALGSTPLUSAMO) - Convert.ToInt32(lblpayment)).ToString();
                lblpayment = lblpayment + ".00";
                //lbldue = lbldue + "0.00";
            }


            
            DbCL.Conn.Close();

            //double a = Convert.ToDouble(lblnetamount) - Convert.ToDouble(lbldue);
            //a = Math.Round(a);
            //string b = Convert.ToString(a);
            //b = b + ".00";
            //lblpayment = b.ToString();
            //PAYAMO = lblpayment.Text;
        }
        private void buindalldata(string payment_ID)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_invoice_payment where Payment_ID='" + payment_ID.ToString() + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                string Due_amount = re["Due_amount"].ToString();
                if (Due_amount == "0.00")
                {
                    inv.Visible = true;
                    delAddress.Visible = true;
                }
                else
                {
                    inv.Visible = false;
                    delAddress.Visible = false;
                }

                lblinvno.Text = re["Invoice_No"].ToString();
                lblpaydate.Text = re["Payment_Date"].ToString();
                lblqnumber.Text = re["Quotation_No"].ToString();
                string qno = re["Quotation_No"].ToString();

                bindcgstorigst(qno);


                string clientid = re["Client_Id"].ToString();
                //lblClientCode.Text = clientid;

                Bindclientdetails(clientid);

                lblSubtotal = re["subtotal"].ToString();
                lblstax = re["Service_tax"].ToString();
                lblstax0 = re["Service_tax"].ToString();
                lblnetamount = re["Net_amount"].ToString();
                //lbldue = re["Due_amount"].ToString();

                string Iddetails = re["ID"].ToString();
                string invno = lblinvno.Text;

               


                deliveryAddress(invno);

                representative(clientid);
                buindpaymentdetails(Iddetails, qno);
                buindpaymentamount(Iddetails, qno);

                Buindamount(qno);
                

                if (lbldue != "0.00")
                {
                    string word = MoneyConvDS.MoneyConvFn(lbldue);
                    lblword = word.ToString();
                }
                else
                {
                    lblword = "Nil";
                }
                //lbldue = (TOTALGSTPLUSAMO-Convert.ToDouble(lblpayment)).ToString();
                //decimal discount_amount1 = Convert.ToDecimal(discount_amount);
                //if (discount_amount1 == 0)
                //{
                //    discount_row.Visible = false;
                //}
                //else
                //{
                //    discount_row.Visible = true;
                //}

                DbCL.Conn.Close();
            }

        }

        private void deliveryAddress(string invno)
        {
            string query = "select SiteAddress from tbl_InvSiteAddress where invoice_no=@invoice_no order by id";
            SqlParameter[] pram = {
                new SqlParameter("@invoice_no",invno)
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
                string RepTitle = dtRep.Rows[0]["RepTitle"].ToString();
                string repname = dtRep.Rows[0]["Representative_name"].ToString();
                string RepLastName = dtRep.Rows[0]["RepLastName"].ToString();
                string Designation = dtRep.Rows[0]["Designation"].ToString();

                //lblrename.Text = RepTitle + " " + repname + " " + RepLastName;
                //lbldeg.Text = Designation;
            }
        }

        private void Buindamount(string qno)
        {
            string status = statusvalue(qno);

            if (status != "YES")
            {
                pnlTasGst.Visible = false;
                lblserviceamo.Visible = false;
                BackData.Visible = true;
                //AMODETAILS.Visible = true;

                string query = "select Sl_no,(Product_name+' '+specification) as Product_name,Quantity,sail_rate,Service_tax_rate,Total_sail_rate2 from tbl_Quotaion_details where Quotation_no='" + lblqnumber.Text + "' order by Product_name";

                SqlParameter[] pram = {
                    new SqlParameter("@Quotation_no",lblqnumber.Text)
                };
                dtBacdata = DbCL.SPreturn_dt(query, pram);
                if (dtBacdata.Rows.Count > 0)
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

                    for (int i = 0; i < dtBacdata.Rows.Count; i++)
                    {
                        string slno = dtBacdata.Rows[i]["Sl_no"].ToString();
                        string Product_name = dtBacdata.Rows[i]["Product_name"].ToString();
                        int Quantity = Convert.ToInt32(dtBacdata.Rows[i]["Quantity"]);
                        TQ = TQ + Quantity;
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
                    strbackdt.Append("<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + lblnetamount + "</td>");
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

                string cmdstring = "select Product_id as HSN,(Product_name+' '+specification) as Product_name,Quantity,sail_rate,Service_tax_rate,Total_sail_rate1,Total_sail_rate2 from  tbl_Quotaion_details where Quotation_no=@Quotation_no  order by Product_name";
                SqlParameter[] pram = {
                                          new SqlParameter("@Quotation_no",lblqnumber.Text),
                                          //new SqlParameter("@Invoice_No",lblinvno.Text)
                                      };

                dtp = DbCL.SPreturn_dt(cmdstring, pram);
                if (dtp.Rows.Count > 0)
                {

                    if (Session["cgstOrsgstb"].ToString() == "YES")
                    {
                        double TOTALCGST = 0;
                        double SUBTOTAL = 0;
                        double TOTALGST = 0;
                        
                        //double DUEAMOUNT = 0;

                        strp += "<table border='0' width='100%' class='Payment pagebrake'><tr><td class='' style=''>";
                        strp += "<table class='PaymentPhase' style='border:0' width='100%'><tr><td class='gap' style=''>&nbsp</td></tr>";
                        strp += "<tr>";
                        strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>S.NO</td>";
                        strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>HSN/<BR>SAC <br> CODE</td>";
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
                            int Quantity = Convert.ToInt32(dtp.Rows[i]["Quantity"]);
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
                            strp += "<td style='width:30%; border:2px solid #6c6c6c; font-weight: bold;  text-align: left;   border-right:none; border-top:none;'>" + Productname + "</td>";
                            strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + Quantity.ToString() + "</td>";
                            strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + baserate.ToString() + "</td>";
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
                        lbldue = Math.Round((Convert.ToDouble(TOTALGSTPLUSAMO) - Convert.ToDouble(lblpayment))).ToString();

                        double lbsd = Convert.ToDouble(lbldue);
                        lbldue = DoFormat(lbsd);

                        if (lbldue != "0.00")
                        {
                            //string word = MoneyConvDS.MoneyConvFn(lbldue);
                            lblword = MoneyConvDS.MoneyConvFn(lbldue).ToString();
                        }
                        else
                        {
                            lblword = "Nil";
                        }

                        string grandtotal = (Math.Round((Convert.ToDouble(TOTALGSTPLUSAMO))).ToString());

                        string word = MoneyConvDS.MoneyConvFn(grandtotal);

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


                        //if (lbldiscount!= "0.00")
                        //{
                        //    string discount_amount = lbldiscount.ToString();
                        //    decimal discount_amount1 = Convert.ToDecimal(discount_amount);
                        //    if (discount_amount1 != 0)
                        //    {
                        //        strp += "<table class='' style='border:0' width='100%'>";
                        //        strp += "<tr>";
                        //        strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none' colspan='6'></td>";
                        //        strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>DISCOUNT AMOUNT:</td>";
                        //        strp += "<td style='width:10%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;  border-top:none;'>" + lbldiscount.ToString() + ".00" + "</td>";
                        //        strp += "</tr>";
                        //        strp += "</table>";
                        //    }

                        //}

                        //strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        //strp += "<tr>";
                        //strp += "<td class='tdsty'  style='text-align:left; padding:3px 15px 3px 15px; width:62%; font-size:13px;border:0px;font-weight:bold;' bgcolor='#dbe5f1'colspan='6'>Amount (In Words):" + lblword.Text.ToString() + "</td>";
                        //strp += "<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:28%;  font-size:13px;border:0px;font-weight:bold' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
                        //strp += "<td class='tdsty' style='text-align:left; padding:5px 0px 5px 15px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + lblnetamount.Text.ToString() + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";

                        strp += "<table class='' style='border:0' width='100%'>";
                        strp += "<tr>";
                        strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='5'>Amount (In Words): " + lblword + "</td>";
                        strp += "<td style='width:5%; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'></td>";
                        strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
                        strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + Math.Round(TOTALGSTPLUSAMO).ToString() + ".00" + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        //strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        //strp += "<tr>";
                        //strp += "<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:90%; font-size:13px;border:0px;font-weight:bold' colspan='10'>PAYMENT AMOUNT:</td>";
                        //strp += "<td class='tdsty' style='text-align:left; padding:5px 0px 5px 15px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + lblpayment.Text.ToString() + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";

                        strp += "<table class='' style='border:0' width='100%'>";
                        strp += "<tr>";
                        strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none' colspan='6'></td>";
                        strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>PAYMENT AMOUNT:</td>";
                        strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + lblpayment.ToString() + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        //strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        //strp += "<tr>";
                        //strp += "<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:90%; font-size:13px;border:0px;font-weight:bold' colspan='10'>DUE AMOUNT:</td>";
                        //strp += "<td class='tdsty' style='text-align:left; padding:5px 0px 5px 15px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + lbldue.Text + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";

                        strp += "<table class='' style='border:0' width='100%'>";
                        strp += "<tr>";
                        strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none' colspan='6'></td>";
                        strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>DUE AMOUNT:</td>";
                        strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + lbldue.ToString() + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                    }
                    if (Session["igstb"].ToString() == "YES")
                    {

                        double TOTALIGST = 0;
                        double SUBTOTAL = 0;
                        double TOTALGST = 0;
                        //double TOTALGSTPLUSAMO = 0;

                        strp += "<table border='0' width='100%'><tr><td class='' style=''>";
                        strp += "<table class='PaymentPhase' style='border:0' width='100%'><tr><td class='gap' style=''>&nbsp</td></tr>";
                        strp += "<tr>";
                        strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>S.NO</td>";
                        strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>HSN/<BR>SAC <br> CODE</td>";
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
                            int Quantity = Convert.ToInt32(dtp.Rows[i]["Quantity"]);
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
                        lbldue =Math.Round((Convert.ToDouble(TOTALGSTPLUSAMO) - Convert.ToDouble(lblpayment))).ToString();

                        double lbsd = Convert.ToDouble(lbldue);
                        lbldue = DoFormat(lbsd);

                        if (lbldue != "0.00")
                        {
                            //string word = MoneyConvDS.MoneyConvFn(lbldue);
                            lblword = MoneyConvDS.MoneyConvFn(lbldue).ToString();
                        }
                        else
                        {
                            lblword = "Nil";
                        }

                        string grandtotal = (Math.Round((Convert.ToDouble(TOTALGSTPLUSAMO))).ToString());

                        string word = MoneyConvDS.MoneyConvFn(grandtotal);

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


                        if (lbldiscount != "")
                        {
                            string discount_amount = lbldiscount.ToString();
                            decimal discount_amount1 = Convert.ToDecimal(discount_amount);
                            if (discount_amount1 != 0)
                            {

                                //strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                                //strp += "<tr>";
                                //strp += "<td class='tdsty'  style='text-align:right; padding:3px 15px 3px 0px;  font-size:13px;border:0px;font-weight:bold' colspan='8'>Discount::</td>";
                                //strp += "<td class='tdsty' style='text-align:left; padding:5px 0px 5px 15px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + lbldiscount.Text.ToString() + "</td>";
                                //strp += "</tr>";
                                //strp += "</table>";
                                strp += "<table class='' style='border:0' width='100%'>";
                                strp += "<tr>";
                                strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none' colspan='6'></td>";
                                strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>DISCOUNT AMOUNT:</td>";
                                strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + lbldiscount.ToString() + ".00" + "</td>";
                                strp += "</tr>";
                                strp += "</table>";
                            }

                        }

                        strp += "<table class='' style='border:0' width='100%'>";
                        strp += "<tr>";
                        strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='5'>Amount (In Words): " + lblword + "</td>";
                        strp += "<td style='width:5%; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'></td>";
                        strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
                        strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + Math.Round(TOTALGSTPLUSAMO).ToString() + ".00" + "</td>";
                        strp += "</tr>";
                        strp += "</table>";


                        

                        strp += "<table class='' style='border:0' width='100%'>";
                        strp += "<tr>";
                        strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none' colspan='6'></td>";
                        strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>PAYMENT AMOUNT:</td>";
                        strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + lblpayment.ToString() + "</td>";
                        strp += "</tr>";
                        strp += "</table>";


                        

                        strp += "<table class='' style='border:0' width='100%'>";
                        strp += "<tr>";
                        strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none' colspan='6'></td>";
                        strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>DUE AMOUNT:</td>";
                        strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + lbldue.ToString() + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                    }

                    lblserviceamo.Text = strp.ToString();
                }
            }
        }

        private string DoFormat(double myNumber)
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
            SqlParameter[] pram = { new SqlParameter("@Quotation_no", lblqnumber.Text) };
            DataTable dtvat = new DataTable();
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

        private void buindpaymentdetails(string iddetails, string invno)
        {


            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Given_amount,type,Ch_no,Ch_bank,Ch_date from tbl_invoice_payment where Quotation_No='" + invno + "' and ID<=" + iddetails + " order by ID";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);

            //DataList1.DataSource = cmd.ExecuteReader();
            //DataList1.DataBind();
            SqlDataReader rdrprepayment = cmd.ExecuteReader();
            if (rdrprepayment.HasRows)
            {
                paymentreceived.Append("<table border='0' width='100%'>");
                paymentreceived.Append("<tr><td class='gap' style=''>&nbsp</td></tr>");
                paymentreceived.Append("<tr><td class='qno' width='100%'>");
                paymentreceived.Append("<table border='0' width='100%' class=''>");
                paymentreceived.Append("<tr><td class='' style='background-color:#c8152a; color:white; text-align:center;'>PAYMENTS RECEIVED</td></tr>");
                while (rdrprepayment.Read())
                {

                    string fds = "";
                    if (rdrprepayment["type"].ToString() == "Online Transaction")
                    {
                        fds = "Payment of Rs " + rdrprepayment["Given_amount"].ToString() + " Recieved, Online Transaction Id: " + rdrprepayment["Ch_no"].ToString() + " " + rdrprepayment["Ch_date"].ToString() + ", " + rdrprepayment["Ch_bank"].ToString() + "";
                    }
                    else if (rdrprepayment["type"].ToString() == "Cheque")
                    {
                        fds = "Payment of Rs " + rdrprepayment["Given_amount"].ToString() + " Recieved, Cheque Number: " + rdrprepayment["Ch_no"].ToString() + " " + rdrprepayment["Ch_date"].ToString() + ", " + rdrprepayment["Ch_bank"].ToString() + "";
                    }
                    else if (rdrprepayment["type"].ToString() == "Cash")
                    {
                        fds = "Payment of Rs " + rdrprepayment["Given_amount"].ToString() + " Recieved, " + rdrprepayment["Ch_no"].ToString() + " " + rdrprepayment["Ch_date"].ToString() + "";
                    }
                    else if (rdrprepayment["type"].ToString() == "DD")
                    {
                        fds = "Payment of Rs " + rdrprepayment["Given_amount"].ToString() + " Recieved, DD Number:" + rdrprepayment["Ch_no"].ToString() + " " + rdrprepayment["Ch_date"].ToString() + ", " + rdrprepayment["Ch_bank"].ToString() + "";
                    }

                    paymentreceived.Append("<tr><td class='' style='text-align:left; border:1px solid #bfbfbf;'>" + fds.ToString() + "</td></tr>");

                    //paymentreceived.Append("<tr><td class='' style='text-align:left; border:1px solid #bfbfbf;'>" + rdrprepayment["Given_amount"].ToString() + "  " + rdrprepayment["Ch_no"].ToString() + "  " + rdrprepayment["Ch_date"].ToString() + "  " + rdrprepayment["Ch_bank"].ToString() + " </td></tr>");
                }
                paymentreceived.Append("</table></td></tr><tr><td class='gap' style=''>&nbsp</td></tr></table>");
                lblPaymentReceived.Text = paymentreceived.ToString();
            }
            DbCL.Conn.Close();
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
                clientName.Text = re["Client_Name"].ToString();
                //if (addressfor == "Corporate office")
                //{

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
                txtaddres.Text = add.ToString();
                //lbladdress2.Text = re["Address2"].ToString();
                //if (lbladdress2.Text == "")
                //{
                //    lbladdress2.Visible = false;
                //}
                //else
                //{
                //    lbladdress2.Visible = true;
                //}
                lblcity.Text = re["City"].ToString();
                lblpincode.Text = re["pin"].ToString();
                //lblstate.Text = re["State"].ToString();

                //}
                //else
                //{
                //Bindaddress(clientid, addressfor);
                //}

                gstno = re["Service_tax_no"].ToString();
                vatno = re["Vat_no"].ToString();

                lblGstno.Text = gstno;

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
                Session["cgstOrsgstb"] = dtci.Rows[0]["cgstOrsgst"].ToString();
                Session["igstb"] = dtci.Rows[0]["igst"].ToString();

                string placeofsupply = dtci.Rows[0]["PlaceofSupply"].ToString();
                lblplaceofsup1.Text = "Place Of Supply";
                lblplaceofsup2.Text = ":";
                lblplaceofsup3.Text = placeofsupply;

            }
        }
    }
}