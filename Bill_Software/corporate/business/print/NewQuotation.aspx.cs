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
    public partial class NewQuotation : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        public string taxorvat = "";
        public string proOrser = "";

        public string psid = "";

        public string str = "";
        public string strp = "";

        DataTable dtgcs = new DataTable();
        DataTable dtp = new DataTable();

        DataTable dtmain = new DataTable();
        DataTable dtClient = new DataTable();
        DataTable dtRepre = new DataTable();
        DataTable dtService = new DataTable();

        DataTable dtpayphase = new DataTable();
        StringBuilder strPayment = new StringBuilder();

        StringBuilder strServTerm = new StringBuilder();
        DataTable dtpr = new DataTable();
        DataTable dtpSer = new DataTable();

        public string netamount = "";
        public int TQ = 0;

        CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string ID = Request.QueryString["ID"];
                buindalldata(ID);

                //Bindtaxdata();
                //Buindamount();
                //BindVatamount();
            }
        }

        private void buindalldata(string id)
        {
            string query = "select Quotation_no,Quotation_date,Client_Id,sub_total,Service_tax,Net_amount,cgstOrsgst,igst,PlaceofSupply,ReferenceName,ReferenceId,ReferenceDate,ValidityDays,DeliveryTenure,PackingCharges,Remarks,DetailedView  from tbl_Quotation where ID=@ID";
            SqlParameter[] pram = {
            new SqlParameter("@id",id)
            };
            dtmain = DbCL.SPreturn_dt(query, pram);
            if (dtmain.Rows.Count > 0)
            {
                string qutno = dtmain.Rows[0]["Quotation_no"].ToString();
                lblqnumber.Text = qutno;
                lbldate.Text = dtmain.Rows[0]["Quotation_date"].ToString();
                Session["Quotation_date"] = lbldate.Text;
                string clientid = dtmain.Rows[0]["Client_Id"].ToString();
                lblClientCode.Text = clientid;

                string refname = dtmain.Rows[0]["ReferenceName"].ToString();
                lbl_refname.Text = refname;
                string refid = dtmain.Rows[0]["ReferenceId"].ToString();
                lbl_refid.Text = refid;
                string refdate = dtmain.Rows[0]["ReferenceDate"].ToString();
                lbl_refdate.Text = refdate;

                string valdays = dtmain.Rows[0]["ValidityDays"].ToString();
                lbl_valdays.Text = valdays;
                string deliverytrms = dtmain.Rows[0]["DeliveryTenure"].ToString();
                lbl_deliverytrms.Text = deliverytrms;
                string packingchrges = dtmain.Rows[0]["PackingCharges"].ToString();
                lbl_pkging.Text = packingchrges;
                string rmrks = dtmain.Rows[0]["Remarks"].ToString();
                lbl_remarks.Text = rmrks;

                string sub_total = dtmain.Rows[0]["sub_total"].ToString();
                //lblqnumber.Text = dtmain.Rows[0]["Service_tax"].ToString();
                netamount = dtmain.Rows[0]["Net_amount"].ToString();
                //lblqnumber.Text = dtmain.Rows[0]["cgstOrsgst"].ToString();
                //lblqnumber.Text = dtmain.Rows[0]["igst"].ToString();
                Session["cgstOrsgst"] = dtmain.Rows[0]["cgstOrsgst"].ToString();
                Session["igst"] = dtmain.Rows[0]["igst"].ToString();
                Session["viewtype"] = dtmain.Rows[0]["DetailedView"].ToString();

                string word = MoneyConvDS.MoneyConvFn(netamount);
                //blword.Text = word.ToString();

                Bindclientdetails(clientid);
                BindRepresentative(clientid);
                BindService(qutno);

                Buindamount(qutno);
                bindpayment(qutno);

                bindPrimaryServiceTerms(qutno);

                string placeofsupply = dtmain.Rows[0]["PlaceofSupply"].ToString();
                lblplaceofsup1.Text = "Place Of Supply";
                lblplaceofsup2.Text = ":";
                lblplaceofsup3.Text = placeofsupply;
            }
        }

        private void bindPrimaryServiceTerms(string qutno)
        {
            string cmdstring = "select PrimaryService from  tbl_QutPrimaryService where qut_no=@qut_no";
            SqlParameter[] pram = {
                                     new SqlParameter("@qut_no",qutno)
                                  };
            dtpr = DbCL.SPreturn_dt(cmdstring, pram);
            if (dtpr.Rows.Count > 0)
            {
                for (int i = 0; i < dtpr.Rows.Count; i++)
                {
                    string pserv = dtpr.Rows[i]["PrimaryService"].ToString();
                    string primaryserviceDetails = bindterms1(pserv);
                    if (primaryserviceDetails!="")
                    {
                        strServTerm.Append("<table border='0' width='100%' class='PrimaryService'>");
                        strServTerm.Append("<tr><td colspan='2' class='' style='text-align: left; font-weight: bold;'>");
                        strServTerm.Append("" + "SPECIFIC TERMS FOR " + pserv.ToUpper() + "");
                        strServTerm.Append("</td></tr>");
                        strServTerm.Append("<tr><td colspan='2' class='gap' style=''>&nbsp</td></tr>");
                        bindterms(pserv, qutno);
                        strServTerm.Append("<tr><td colspan='2' class='gap' style=''>&nbsp</td></tr>");
                        strServTerm.Append(" </table>");
                    }
                    
                }
                //TextInfo textInfo1 = cultureInfo.TextInfo;
                if (Session["pserTerm"] != null)
                {
                    lblPrimaryServicePoint.Text = strServTerm.ToString();
                }

            }
        }

        private string bindterms1(string pserv)
        {

            String details = "";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdString = "select PrimaryServiceTerms from tbl_PrimaryServiceTerms where PrimaryService=@PrimaryService";
            SqlCommand cmd = new SqlCommand(cmdString, DbCL.Conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 0;
            cmd.Parameters.AddWithValue("@PrimaryService", pserv.ToString());
            
            SqlDataReader re = cmd.ExecuteReader();

            if (re.Read())
            {
                details = re["PrimaryServiceTerms"].ToString();
            }
            DbCL.Conn.Close();
            return details;
        }

        private void bindterms(string pserv, string qutno)
        {
            string cmdstring = "select PSerTer from tbl_QuoPserTerm where qutno=@qutno and PServiceName=@PServiceName";
            SqlParameter[] pram = {
                                     new SqlParameter("@qutno",qutno),
                                     new SqlParameter("@PServiceName",pserv),
                                  };
            dtpSer = DbCL.SPreturn_dt(cmdstring, pram);
            if (dtpSer.Rows.Count > 0)
            {
                for (int i = 0; i < dtpSer.Rows.Count; i++)
                {
                    string pserTerm = dtpSer.Rows[i]["PSerTer"].ToString();
                    Session["pserTerm"] = pserTerm;
                    strServTerm.Append("<tr><td class='' style='text-align: justify; font-weight: 100;  vertical-align: top'><i class='fa fa-arrow-circle-right' style='color: #c8152a'></i></td>");
                    strServTerm.Append("<td class='' style='text-align: justify;  font-weight: 100'>");
                    strServTerm.Append("" + pserTerm + "");
                    strServTerm.Append("</td>");
                    strServTerm.Append("</tr>");
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

        //private void Buindamount(string qutno)
        //{
        //    string cmdstring = "select Sl_no,Product_id as HSN,(Product_name+' '+specification) as Product_name,Quantity,sail_rate,Service_tax_rate,Total_sail_rate2 from tbl_Quotaion_details where Quotation_no=@Quotation_no order by Product_name";
        //    SqlParameter[] pram = {
        //                                  new SqlParameter("@Quotation_no",qutno)
        //                              };
        //    dtp = DbCL.SPreturn_dt(cmdstring, pram);
        //    if (dtp.Rows.Count > 0)
        //    {
        //        if (Session["cgstOrsgst"].ToString() == "YES")
        //        {
        //            double TOTALCGST = 0;
        //            double SUBTOTAL = 0;
        //            double TOTALGST = 0;
        //            double TOTALGSTPLUSAMO = 0;

        //            strp += "<table border='0' width='100%' class='Payment pagebrake'><tr><td class='' style='text-align: left; font-weight: bold;'>OUR QUOTE</td></tr><tr><td class='' style=''>";
        //            strp += "<table class='PaymentPhase' style='border:0' width='100%'><tr><td class='gap' style=''>&nbsp</td></tr>";
        //            strp += "<tr>";
        //            strp += "<td style='width:5%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>S.NO</td>";
        //            strp += "<td style='width:30%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>PARTICULARS</td>";
        //            strp += "<td style='width:7%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>HSN <br> CODE</td>";
        //            strp += "<td style='width:8%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>QUANTITY<br>(PCS)</td>";
        //            strp += "<td style='width:7%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>RATE<br> (RS)</td>";
        //            strp += "<td style='width:5%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>GST</td>";

        //            strp += "<td style='width:14%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;''>";
        //            strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
        //            strp += "<tr>";
        //            strp += "<td style='width:14%; border: 1px solid #bfbfbf;border-top:none; border-left:none; border-right:none; font-weight: bold; background-color: #e31e24; text-align: center; color: white; ' colspan='2'>CGST</td>";
        //            strp += "</tr>";
        //            strp += "<tr>";
        //            strp += "<td style='width:5%; border: 1px solid #bfbfbf;border-top:none; border-left:none; border-bottom:none;  font-weight: bold; background-color: #e31e24; text-align: center; color: white; '>RATE</td>";
        //            strp += "<td style='width:9%;  font-weight: bold; background-color: #e31e24; text-align: center; color: white; '>AMOUNT</td>";
        //            strp += "</tr>";
        //            strp += "</table>";
        //            strp += "</td>";

        //            strp += "<td style='width:14%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>";
        //            strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
        //            strp += "<tr>";
        //            strp += "<td style='width:14%; border: 1px solid #bfbfbf;border-top:none; border-left:none; border-right:none;  font-weight: bold; background-color: #e31e24; text-align: center; color: white; ' colspan='2'>SGST</td>";
        //            strp += "</tr>";
        //            strp += "<tr>";
        //            strp += "<td style='width:5%; border: 1px solid #bfbfbf;border-top:none; border-left:none; border-bottom:none;  font-weight: bold; background-color: #e31e24; text-align: center; color: white; '>RATE</td>";
        //            strp += "<td style='width:9%;  font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>AMOUNT</td>";
        //            strp += "</tr>";
        //            strp += "</table>";
        //            strp += "</td>";

        //            strp += "<td style='width:10%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>AMOUNT<br> (RS)</td>";
        //            strp += "</tr>";
        //            strp += "</table>";


        //            //strp += "</td></tr></table>";


        //            for (int i = 0; i < dtp.Rows.Count; i++)
        //            {
        //                string HSN = dtp.Rows[i]["HSN"].ToString();
        //                string Productname = dtp.Rows[i]["Product_name"].ToString();
        //                int Quantity = Convert.ToInt32(dtp.Rows[i]["Quantity"]);
        //                TQ = TQ + Quantity;
        //                double baserate = Math.Round((Convert.ToDouble(dtp.Rows[i]["sail_rate"])), 2);
        //                int gstper = Convert.ToInt32(dtp.Rows[i]["Service_tax_rate"]);
        //                double QuantityBaserateAmo = Math.Round((Quantity * baserate), 2);

        //                string QuantityBaserateAmo1 = DoFormat(QuantityBaserateAmo);

        //                //double QuantityBaserateAmo1 = Math.Round(QuantityBaserateAmo);

        //                double gstamount = Math.Round(((QuantityBaserateAmo * gstper) / 100), 2);

        //                double cgstper = Math.Round((Convert.ToDouble(gstper) / 2), 2);

        //                double cgstamo = Math.Round((Convert.ToDouble(gstamount) / 2), 2);

        //                TOTALCGST = TOTALCGST + cgstamo;

        //                SUBTOTAL = SUBTOTAL + QuantityBaserateAmo;

        //                TOTALGST = TOTALGST + gstamount;



        //                strp += "<table class='' style='border:0' width='100%'>";
        //                strp += "<tr>";
        //                strp += "<td style='width:5%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'>" + (i + 1).ToString() + "</td>";

        //                strp += "<td style='width:30%; border:1px solid #bfbfbf; font-weight: bold;  text-align: left;   border-right:none; border-top:none;'>" + Productname + "</td>";
        //                strp += "<td style='width:7%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + HSN + "</td>";

        //                strp += "<td style='width:8%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + Quantity.ToString() + "</td>";
        //                strp += "<td style='width:7%; border:1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + baserate.ToString() + "</td>";
        //                strp += "<td style='width:5%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";

        //                strp += "<td style='width:5%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + cgstper.ToString() + " %</td>";
        //                strp += "<td style='width:9%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + cgstamo.ToString() + "</td>";
        //                strp += "<td style='width:5%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + cgstper.ToString() + " %</td>";
        //                strp += "<td style='width:9%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + cgstamo.ToString() + "</td>";

        //                strp += "<td style='width:10%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;   border-top:none;'>" + QuantityBaserateAmo1.ToString() + "</td>";
        //                strp += "</tr>";
        //                strp += "</table>";
        //            }

        //            TOTALGSTPLUSAMO = TOTALGST + SUBTOTAL;

        //            string grandtotal = (Math.Round((Convert.ToDouble(TOTALGSTPLUSAMO))).ToString());

        //            string word = MoneyConvDS.MoneyConvFn(grandtotal);

        //            string SUBTOTAL1 = DoFormat(SUBTOTAL);
        //            strp += "<table class='' style='border:0' width='100%'>";
        //            strp += "<tr>";

        //            strp += "<td style='border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;  border-right:none; border-top:none;background-color:#d9d3d3' colspan='3'>GRAND TOTAL</td>";

        //            strp += "<td style='width:8%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'>" + TQ.ToString() + "</td>";
        //            strp += "<td style='width:7%; border:1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
        //            strp += "<td style='width:5%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
        //            strp += "<td style='width:5%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";


        //            //strp += "<td style='border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;' colspan='7'></td>";
        //            strp += "<td style='width:9%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'>" + TOTALCGST.ToString() + "</td>";
        //            strp += "<td style='width:5%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
        //            strp += "<td style='width:9%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'>" + TOTALCGST.ToString() + "</td>";

        //            strp += "<td style='width:10%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;   border-top:none;background-color:#d9d3d3'>" + Math.Round(SUBTOTAL).ToString() + ".00" + "</td>";
        //            strp += "</tr>";
        //            strp += "</table>";


        //            strp += "<table class='' style='border:0' width='100%'>";
        //            strp += "<tr>";
        //            strp += "<td style='font-weight: bold;  text-align: center' colspan='6'></td>";
        //            strp += "<td style='width:28%; font-weight: bold;  text-align: right; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT BEFORE TAX:</td>";
        //            strp += "<td style='width:10%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;  border-top:none'>" + Math.Round(SUBTOTAL).ToString() + ".00" + "</td>";
        //            strp += "</tr>";
        //            strp += "</table>";

        //            //strp += "<table class='' style='border:0' width='100%'>";
        //            //strp += "<tr>";
        //            //strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none' colspan='6'></td>";
        //            //strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL GST:</td>";
        //            //strp += "<td style='width:10%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;  border-top:none;'>" + Math.Round(TOTALGST).ToString()+".00" + "</td>";
        //            //strp += "</tr>";
        //            //strp += "</table>";

        //            //strp += "<table class='' style='border:0' width='100%'>";
        //            //strp += "<tr>";
        //            //strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='5'>Amount (In Words):" + word + "</td>";
        //            //strp += "<td style='width:5%; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'></td>";
        //            //strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
        //            //strp += "<td style='width:10%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;  border-top:none;'>" + grandtotal.ToString()+".00" + "</td>";
        //            //strp += "</tr>";
        //            //strp += "</table>";

        //            strp += "<table class='' style='border:0' width='100%'>";
        //            strp += "<tr>";
        //            strp += "<td style='font-weight: bold;  text-align: justify;  border-right:none; border-top:none; background-color: #e31e24; color: white; vertical-align: top;' rowspan='2' colspan='5'>Amount (In Words):" + word + "</td>";
        //            strp += "<td style='width:5%; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'></td>";
        //            strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL GST:</td>";
        //            strp += "<td style='width:10%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;  border-top:none;'>" + Math.Round(TOTALGST).ToString() + ".00" + "</td>";
        //            strp += "</tr>";

        //            strp += "<tr>";
        //            //strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='5'>Amount (In Words):" + word + "</td>";
        //            strp += "<td style='width:5%; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'></td>";
        //            strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
        //            strp += "<td style='width:10%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;  border-top:none;'>" + grandtotal.ToString() + ".00" + "</td>";
        //            strp += "</tr>";
        //            strp += "</table>";


        //            //< table border = "0" width = "100%" class="FORCE MAJEURE">
        //            //        <tr>
        //            //            <td class="" style="text-align: justify; font-weight: 100;  vertical-align: top;" rowspan="2">1 ST1 ST</td>
        //            //            <td class="" style="text-align: justify;  font-weight: 100">
        //            //                 	The Company will
        //            //            </td>
        //            //        </tr>
        //            //        <tr>
        //            //            <td class="" style="text-align: justify;  font-weight: 100">
        //            //                 	The Company will
        //            //            </td>
        //            //        </tr>

        //            //    </table>

        //            strp += "</td></tr><tr><td class='gap' style=''>&nbsp</td></tr></table>";

        //        }
        //        if (Session["igst"].ToString() == "YES")
        //        {

        //            double TOTALIGST = 0;
        //            double SUBTOTAL = 0;
        //            double TOTALGST = 0;
        //            double TOTALGSTPLUSAMO = 0;

        //            strp += "<table border='0' width='100%'><tr><td class='' style='text-align: left; font-weight: bold;'>OUR QUOTE</td></tr><tr><td class='' style=''>";
        //            strp += "<table class='PaymentPhase' style='border:0' width='100%'><tr><td class='gap' style=''>&nbsp</td></tr>";
        //            strp += "<tr>";
        //            strp += "<td style='width:5%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>S.NO</td>";

        //            strp += "<td style='width:44%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>PARTICULARS</td>";
        //            strp += "<td style='width:7%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>HSN<br> CODE</td>";

        //            strp += "<td style='width:8%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>QUANTITY<br> (PCS)</td>";
        //            strp += "<td style='width:7%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>RATE<br> (RS)</td>";
        //            strp += "<td style='width:5%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>GST</td>";

        //            strp += "<td style='width:14%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;''>";
        //            strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
        //            strp += "<tr>";
        //            strp += "<td style='width:14%; border: 1px solid #bfbfbf;border-top:none; border-left:none; border-right:none; font-weight: bold; background-color: #e31e24; text-align: center; color: white; ' colspan='2'>IGST</td>";
        //            strp += "</tr>";
        //            strp += "<tr>";
        //            strp += "<td style='width:5%; border: 1px solid #bfbfbf;border-top:none; border-left:none; border-bottom:none;  font-weight: bold; background-color: #e31e24; text-align: center; color: white; '>RATE</td>";
        //            strp += "<td style='width:9%;  font-weight: bold; background-color: #e31e24; text-align: center; color: white; '>AMOUNT</td>";
        //            strp += "</tr>";
        //            strp += "</table>";
        //            strp += "</td>";

        //            strp += "<td style='width:10%; border: 1px solid #bfbfbf; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>AMOUNT<br> (RS)</td>";
        //            strp += "</tr>";
        //            strp += "</table>";


        //            for (int i = 0; i < dtp.Rows.Count; i++)
        //            {
        //                string HSN = dtp.Rows[i]["HSN"].ToString();
        //                string Productname = dtp.Rows[i]["Product_name"].ToString();
        //                int Quantity = Convert.ToInt32(dtp.Rows[i]["Quantity"]);
        //                TQ = TQ + Quantity;
        //                double baserate = Math.Round((Convert.ToDouble(dtp.Rows[i]["sail_rate"])), 2);
        //                int gstper = Convert.ToInt32(dtp.Rows[i]["Service_tax_rate"]);
        //                double QuantityBaserateAmo = Math.Round((Quantity * baserate), 2);
        //                string QuantityBaserateAmo1 = DoFormat(QuantityBaserateAmo);

        //                double gstamount = Math.Round(((QuantityBaserateAmo * gstper) / 100), 2);

        //                double cgstper = Math.Round((Convert.ToDouble(gstper) / 2), 2);

        //                double cgstamo = Math.Round((Convert.ToDouble(gstamount) / 2), 2);

        //                TOTALIGST = TOTALIGST + gstamount;
        //                SUBTOTAL = SUBTOTAL + QuantityBaserateAmo;

        //                TOTALGST = TOTALGST + gstamount;



        //                strp += "<table class='' style='border:0' width='100%'>";
        //                strp += "<tr>";
        //                strp += "<td style='width:5%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + (i + 1).ToString() + "</td>";

        //                strp += "<td style='width:44%; border:1px solid #bfbfbf; font-weight: bold;  text-align: left;     border-right:none; border-top:none;'>" + Productname + "</td>";
        //                strp += "<td style='width:7%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + HSN + "</td>";

        //                strp += "<td style='width:8%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + Quantity.ToString() + "</td>";
        //                strp += "<td style='width:7%; border:1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + baserate.ToString() + "</td>";
        //                strp += "<td style='width:5%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";

        //                strp += "<td style='width:5%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";
        //                strp += "<td style='width:9%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + gstamount.ToString() + "</td>";

        //                strp += "<td style='width:10%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;   border-top:none;'>" + QuantityBaserateAmo1.ToString() + "</td>";
        //                strp += "</tr>";
        //                strp += "</table>";
        //            }

        //            TOTALGSTPLUSAMO = TOTALGST + SUBTOTAL;

        //            string grandtotal = (Math.Round((Convert.ToDouble(TOTALGSTPLUSAMO))).ToString());

        //            string grandtotal1 = DoFormat(Math.Round((Convert.ToDouble(TOTALGSTPLUSAMO)))).ToString();

        //            string word = MoneyConvDS.MoneyConvFn(grandtotal);



        //            strp += "<table class='' style='border:0' width='100%'>";
        //            strp += "<tr>";



        //            strp += "<td style='border:1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none; background-color:#d9d3d3' colspan='3'>GRAND TOTAL</td>";
        //            strp += "<td style='width:8%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'>" + TQ.ToString() + "</td>";
        //            strp += "<td style='width:7%; border:1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
        //            strp += "<td style='width:5%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";

        //            strp += "<td style='width:5%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";


        //            strp += "<td style='width:9%; border: 1px solid #bfbfbf; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'>" + TOTALIGST.ToString() + "</td>";

        //            strp += "<td style='width:10%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;   border-top:none;background-color:#d9d3d3'>" + Math.Round(SUBTOTAL).ToString() + ".00" + "</td>";
        //            strp += "</tr>";
        //            strp += "</table>";


        //            strp += "<table class='' style='border:0' width='100%'>";
        //            strp += "<tr>";
        //            strp += "<td style='font-weight: bold;  text-align: center' colspan='6'></td>";
        //            strp += "<td style='width:28%; font-weight: bold;  text-align: right; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT BEFORE TAX:</td>";
        //            strp += "<td style='width:10%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;  border-top:none'>" + Math.Round(SUBTOTAL).ToString() + ".00" + "</td>";
        //            strp += "</tr>";
        //            strp += "</table>";



        //            //strp += "<table class='' style='border:0' width='100%'>";
        //            //strp += "<tr>";
        //            //strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none' colspan='6'></td>";
        //            //strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL GST:</td>";
        //            //strp += "<td style='width:10%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;  border-top:none;'>" + Math.Round(TOTALGST).ToString()+".00" + "</td>";
        //            //strp += "</tr>";
        //            //strp += "</table>";

        //            //strp += "<table class='' style='border:0' width='100%'>";
        //            //strp += "<tr>";
        //            //strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='5'>Amount (In Words):" + word + "</td>";
        //            //strp += "<td style='width:5%; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'></td>";
        //            //strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
        //            //strp += "<td style='width:10%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;  border-top:none;'>" + grandtotal.ToString()+".00" + "</td>";
        //            //strp += "</tr>";
        //            //strp += "</table>";

        //            strp += "<table class='' style='border:0' width='100%'>";
        //            strp += "<tr>";
        //            strp += "<td style='font-weight: bold;  text-align: justify;  border-right:none; border-top:none; background-color: #e31e24; color: white; vertical-align: top;' rowspan='2' colspan='5'>Amount (In Words):" + word + "</td>";
        //            strp += "<td style='width:5%; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'></td>";
        //            strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL GST:</td>";
        //            strp += "<td style='width:10%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;  border-top:none;'>" + Math.Round(TOTALGST).ToString() + ".00" + "</td>";
        //            strp += "</tr>";

        //            strp += "<tr>";
        //            strp += "<td style='width:5%; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'></td>";
        //            strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
        //            strp += "<td style='width:10%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;  border-top:none;'>" + grandtotal1 + "</td>";
        //            strp += "</tr>";
        //            strp += "</table>";

        //            strp += "</td></tr><tr><td class='gap' style=''>&nbsp</td></tr></table>";
        //        }
        //        lblserviceamo.Text = strp.ToString();
        //    }
        //}

        private void Buindamount(string qutno)
        {
            //string cmdstring = "select Sl_no,Product_id as HSN,(Product_name+' '+specification) as Product_name,Quantity,sail_rate,Service_tax_rate,Total_sail_rate2, discount_rate, new_sailrate from tbl_Quotaion_details where Quotation_no=@Quotation_no order by Id";
            string cmdstring = "select Sl_no,Product_id as HSN,Product_name, specification, Quantity,sail_rate,Service_tax_rate,Total_sail_rate2, discount_rate, new_sailrate, ItemRemarks, ItemNo, MaterialNo, PackSize from tbl_Quotaion_details where Quotation_no=@Quotation_no order by Id";
            SqlParameter[] pram = {
                                          new SqlParameter("@Quotation_no",qutno)
                                      };
            dtp = DbCL.SPreturn_dt(cmdstring, pram);
            if (dtp.Rows.Count > 0)
            {
                if (Session["cgstOrsgst"].ToString() == "YES")
                {
                    //double TOTALCGST = 0;
                    //string TOTALCGST1 = string.Empty;
                    //double SUBTOTAL = 0;
                    //double TOTALGST = 0;
                    //string TOTALGST1 = string.Empty;
                    //double TOTALGSTPLUSAMO = 0;
                    //string TOTALGSTPLUSAMO1 = string.Empty;

                    double new_TOTALCGST = 0;
                    string new_TOTALCGST1 = string.Empty;
                    double new_SUBTOTAL = 0;
                    double new_TOTALGST = 0;
                    string new_TOTALGST1 = string.Empty;
                    double new_TOTALGSTPLUSAMO = 0;
                    string new_TOTALGSTPLUSAMO1 = string.Empty;


                    //strp += "<table border='0' width='100%' class='Payment pagebrake'><tr><td class='' style='text-align: left; font-weight: bold;'>OUR QUOTE</td></tr><tr><td class='' style=''>";
                    //strp += "<table class='PaymentPhase' style='border:0' width='100%'><tr><td class='gap' style=''>&nbsp</td></tr>";
                    //strp += "<tr>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; font-size: 10px; color: white; border-right:none;'>S.NO</td>";
                    //strp += "<td style='width:25%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; font-size: 10px; color: white;  border-right:none;'>PARTICULARS</td>";
                    //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; font-size: 10px; color: white;  border-right:none;'>HSN <br> CODE</td>";
                    //strp += "<td style='width:6%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; font-size: 10px; color: white;  border-right:none;'>QTY <br> (PCS)</td>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; color: white; font-size: 10px;  border-right:none;'>BASE RATE <br> (RS)</td>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; color: white; font-size: 10px;  border-right:none;'>DISC<br> (%)</td>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; color: white; font-size: 10px;  border-right:none;'>NEW <br> RATE (RS)</td>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; color: white; font-size: 10px;  border-right:none;'>GST</td>";
                    //strp += "<td style='width:14%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; color: white; font-size: 10px;  border-right:none;''>";
                    //strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                    //strp += "<tr>";
                    //strp += "<td style='width:14%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-right:none; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; color: white; font-size: 10px;' colspan='2'>CGST</td>";
                    //strp += "</tr>";
                    //strp += "<tr>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-bottom:none;  font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; color: white; font-size: 10px;'>RATE</td>";
                    //strp += "<td style='width:9%;  font-weight: bold; background-color: #e31e24; text-align: center; color: white; font-size: 10px; '>AMOUNT</td>";
                    //strp += "</tr>";
                    //strp += "</table>";
                    //strp += "</td>";
                    //strp += "<td style='width:14%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; color: white; font-size: 10px;  border-right:none;'>";
                    //strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                    //strp += "<tr>";
                    //strp += "<td style='width:14%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-right:none;  font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; color: white; font-size: 10px;' colspan='2'>SGST</td>";
                    //strp += "</tr>";
                    //strp += "<tr>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c;border-top:none; border-left:none; border-bottom:none;  font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; color: white; font-size: 10px; '>RATE</td>";
                    //strp += "<td style='width:9%;  font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; color: white; font-size: 10px;  border-right:none;'>AMOUNT</td>";
                    //strp += "</tr>";
                    //strp += "</table>";
                    //strp += "</td>";
                    //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; color: white; font-size: 10px;'>AMOUNT<br> (RS)</td>";
                    //strp += "<td style='width:10%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; color: white; font-size: 10px;'>Remarks</td>";
                    //strp += "</tr>";
                    //strp += "</table>";

                    //strp += "<table border='0' width='100%' class='Payment pagebrake'><tr><td class='' style='text-align: left; font-weight: bold;'>OUR QUOTE</td></tr><tr><td class='' style=''>";
                    //strp += "<table class='PaymentPhase' style='border:0' width='100%'><tr><td class='gap' style=''>&nbsp</td></tr>";
                    //strp += "<tr>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; font-size: 10px; color: white; border-right:none;'>S.NO</td>";
                    //strp += "<td style='width:25%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; font-size: 10px; color: white; border-right:none;'>PARTICULARS</td>";
                    //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; font-size: 10px; color: white; border-right:none;'>HSN <br> CODE</td>";
                    //strp += "<td style='width:6%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; font-size: 10px; color: white; border-right:none;'>QTY <br> (PCS)</td>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; color: white; font-size: 10px; border-right:none;'>BASE RATE <br> (RS)</td>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; color: white; font-size: 10px; border-right:none;'>DISC<br> (%)</td>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; color: white; font-size: 10px; border-right:none;'>NEW <br> RATE (RS)</td>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; color: white; font-size: 10px;'>GST</td>";
                    //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; color: white; font-size: 10px;'>AMOUNT<br> (RS)</td>";
                    //strp += "<td style='width:10%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; word-wrap: break-word; text-align: center; color: white; font-size: 10px;'>Remarks</td>";
                    //strp += "</tr>";
                    //strp += "</table>";

                    // Start Table
                    strp += "<table class='' style='border:2px solid #6c6c6c; border-collapse: collapse; width:100%;'>";

                    // HEADER ROW
                    strp += "<tr style='background-color:#d9d3d3; text-align:center; background-color: #e31e24; font-weight:bold; color: white; font-size: 10px;'>";
                    strp += "<th style='width:5%; border:2px solid #6c6c6c; '>S.No</th>";
                    strp += "<th style='width:30%; border:2px solid #6c6c6c;'>Product Name & Specification</th>";
                    strp += "<th style='width:8%; border:2px solid #6c6c6c;'>HSN Code</th>";
                    strp += "<th style='width:6%; border:2px solid #6c6c6c;'>Qty (PCS)</th>";
                    strp += "<th style='width:5%; border:2px solid #6c6c6c;'>Base Rate</th>";
                    strp += "<th style='width:5%; border:2px solid #6c6c6c;'>Disc (%)</th>";
                    strp += "<th style='width:5%; border:2px solid #6c6c6c;'>Disc Rate</th>";
                    strp += "<th style='width:5%; border:2px solid #6c6c6c;'>GST (%)</th>";
                    strp += "<th style='width:8%; border:2px solid #6c6c6c;'>Amount (₹)</th>";
                    strp += "<th style='width:13%; border:2px solid #6c6c6c;'>Remarks</th>";
                    strp += "</tr>";


                    //strp += "</td></tr></table>";


                    for (int i = 0; i < dtp.Rows.Count; i++)
                    {
                        string HSN = dtp.Rows[i]["HSN"].ToString();
                        string Productname = dtp.Rows[i]["Product_name"].ToString();
                        string specification = dtp.Rows[i]["specification"].ToString();
                        string itemno = dtp.Rows[i]["ItemNo"].ToString();
                        string materialno = dtp.Rows[i]["MaterialNo"].ToString();
                        string packsize = dtp.Rows[i]["PackSize"].ToString();

                        int Quantity = Convert.ToInt32(dtp.Rows[i]["Quantity"]);
                        TQ = TQ + Quantity;
                        double baserate = Math.Round((Convert.ToDouble(dtp.Rows[i]["sail_rate"])), 2);
                        double discountrate = Math.Round((Convert.ToDouble(dtp.Rows[i]["new_sailrate"])), 2);
                        int gstper = Convert.ToInt32(dtp.Rows[i]["Service_tax_rate"]);
                        int discper = Convert.ToInt32(dtp.Rows[i]["discount_rate"]);
                        //double QuantityBaserateAmo = Math.Round((Quantity * baserate), 2);
                        //string QuantityBaserateAmo1 = DoFormat(QuantityBaserateAmo);

                        //individual items remarks for display
                        string itemremarks = dtp.Rows[i]["ItemRemarks"].ToString();

                        double new_QuantityBaserateAmo = Math.Round((Quantity * discountrate), 2);
                        string new_QuantityBaserateAmo1 = DoFormat(new_QuantityBaserateAmo);

                        //double QuantityBaserateAmo1 = Math.Round(QuantityBaserateAmo);

                        //double gstamount = Math.Round(((QuantityBaserateAmo * gstper) / 100), 2);
                        double new_gstamount = Math.Round(((new_QuantityBaserateAmo * gstper) / 100), 2);

                        double cgstper = Math.Round((Convert.ToDouble(gstper) / 2), 2);

                        //double cgstamo = Math.Round((Convert.ToDouble(gstamount) / 2), 2);
                        double new_cgstamo = Math.Round((Convert.ToDouble(new_gstamount) / 2), 2);

                        //string cgstamo1 = DoFormat(cgstamo);
                        string new_cgstamo1 = DoFormat(new_cgstamo);

                        //TOTALCGST = TOTALCGST + cgstamo;
                        //TOTALCGST1 = DoFormat(TOTALCGST);

                        new_TOTALCGST = new_TOTALCGST + new_cgstamo;
                        new_TOTALCGST1 = DoFormat(new_TOTALCGST);

                        //SUBTOTAL = SUBTOTAL + QuantityBaserateAmo;
                        new_SUBTOTAL = new_SUBTOTAL + new_QuantityBaserateAmo;

                        //TOTALGST = TOTALGST + gstamount;
                        //TOTALGST1 = DoFormat(TOTALGST);

                        new_TOTALGST = new_TOTALGST + new_gstamount;
                        new_TOTALGST1 = DoFormat(new_TOTALGST);


                        //strp += "<table class='' style='border:0' width='100%'>";
                        //strp += "<tr>";
                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none;'>" + (i + 1).ToString() + "</td>";
                        //strp += "<td style='width:25%; border:2px solid #6c6c6c; font-weight: bold;  text-align: left; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none;'>" + Productname +"<br>" + specification + "</td>";
                        //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none;'>" + HSN + "</td>";
                        //strp += "<td style='width:6%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none;'>" + Quantity.ToString() + "</td>";
                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none;'>" + baserate.ToString() + "</td>";
                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none;'>" + discper.ToString() + "%</td>";
                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none;'>" + discountrate.ToString() + "</td>";
                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";
                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center; font-size: 10px; word-wrap: break-word;  border-right:none; border-top:none;'>" + cgstper.ToString() + " %</td>";
                        //strp += "<td style='width:9%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center; font-size: 10px; word-wrap: break-word;  border-right:none; border-top:none;'>" + new_cgstamo1.ToString() + "</td>";
                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none;'>" + cgstper.ToString() + " %</td>";
                        //strp += "<td style='width:9%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none;'>" + new_cgstamo1.ToString() + "</td>";
                        //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: right; font-size: 10px; word-wrap: break-word; border-top:none;'>" + new_QuantityBaserateAmo1.ToString() + "</td>";
                        //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: right; font-size: 10px; word-wrap: break-word; border-top:none;'>" + itemremarks.ToString() + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";

                        //strp += "<table class='' style='border:0' width='100%'>";
                        //strp += "<tr>";
                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none;'>" + (i + 1).ToString() + "</td>";
                        //strp += "<td style='width:30%; border:2px solid #6c6c6c; font-weight: bold; text-align: left; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none;'>" + Productname + "<br>" + specification + "</td>";
                        //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none;'>" + HSN + "</td>";
                        //strp += "<td style='width:6%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none;'>" + Quantity.ToString() + "</td>";
                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none;'>" + baserate.ToString() + "</td>";
                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none;'>" + discper.ToString() + "%</td>";
                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none;'>" + discountrate.ToString() + "</td>";
                        //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";
                        //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold; text-align: right; font-size: 10px; word-wrap: break-word; border-top:none;'>" + new_QuantityBaserateAmo1.ToString() + "</td>";
                        //strp += "<td style='width:13%; border: 2px solid #6c6c6c; font-weight: bold; text-align: right; font-size: 10px; word-wrap: break-word; border-top:none;'>" + itemremarks.ToString() + "</td>";
                        //strp += "</tr>";
                        //strp += "</table>";

                        strp += "<tr>";
                        strp += "<td style='text-align:center; border:2px solid #6c6c6c; font-size: 10.5px;'>" + (i + 1) + "</td>";
                        //strp += "<td style='text-align:left; border:2px solid #6c6c6c; font-size: 10.5px;'>" + Productname + "<br>" + specification + "</td>";
                        if (Session["viewtype"].ToString() == "Yes")
                        {
                            strp += string.Format("<td style='border: 2px solid #6c6c6c; text-align: left; padding: 5px;'>" +
                                   "<div><span style='font-weight: bold; color: black;'>{0}</span></div>" +
                                   "<div style='font-style: italic; font-size: 10px; color: gray;'>Make: {1}</div>" +
                                   "<div style='font-style: italic; font-size: 10px; color: gray;'>Item No: {2}</div>" +
                                   "<div style='font-style: italic; font-size: 10px; color: gray;'>Material No: {3}</div>" +
                                   "<div style='font-style: italic; font-size: 10px; color: gray;'>Pack Size: {4}</div>" +
                                   "</td>", Productname, specification, itemno, materialno, packsize);
                        }
                        else
                        {
                            strp += $"<td style='border: 2px solid #6c6c6c; text-align: left;'><span style='font-weight: bold; color: black;'>{Productname}</span><br>&nbsp;&nbsp;<span style='font-style: italic; font-size: 10px; color: gray;'>Make:{specification}</span></td>";
                        }
                        strp += "<td style='text-align:center; border:2px solid #6c6c6c; font-size: 10.5px;'>" + HSN + "</td>";
                        strp += "<td style='text-align:center; border:2px solid #6c6c6c; font-size: 10.5px;'>" + Quantity + "</td>";
                        strp += "<td style='text-align:center; border:2px solid #6c6c6c; font-size: 10.5px;'>" + baserate + "</td>";
                        strp += "<td style='text-align:center; border:2px solid #6c6c6c; font-size: 10.5px;'>" + discper + "%</td>";
                        strp += "<td style='text-align:center; border:2px solid #6c6c6c; font-size: 10.5px;'>" + discountrate + "</td>";
                        strp += "<td style='text-align:center; border:2px solid #6c6c6c; font-size: 10.5px;'>" + gstper + "%</td>";
                        strp += "<td style='text-align:right; border:2px solid #6c6c6c; font-size: 10.5px;'>" + new_QuantityBaserateAmo1 + "</td>";
                        strp += "<td style='text-align:left; border:2px solid #6c6c6c; font-size: 10.5px;'>" + itemremarks + "</td>";
                        strp += "</tr>";
                    }

                    //TOTALGSTPLUSAMO = TOTALGST + SUBTOTAL;
                    //TOTALGSTPLUSAMO1 = DoFormat(TOTALGSTPLUSAMO);
                    //string grandtotal = (Math.Round((Convert.ToDouble(TOTALGSTPLUSAMO)),2).ToString());

                    new_TOTALGSTPLUSAMO = new_TOTALGST + new_SUBTOTAL;
                    new_TOTALGSTPLUSAMO1 = DoFormat(new_TOTALGSTPLUSAMO);
                    string grandtotal = (Math.Round((Convert.ToDouble(new_TOTALGSTPLUSAMO)), 2).ToString());

                    string word = MoneyConvDS.MoneyConvFn(grandtotal);

                    //string SUBTOTAL1 = DoFormat(SUBTOTAL);
                    string new_SUBTOTAL1 = DoFormat(new_SUBTOTAL);

                    //strp += "<table class='' style='border:0' width='100%'>";
                    //strp += "<tr>";
                    //strp += "<td style='border: 2px solid #6c6c6c; font-weight: bold;  text-align: center; font-size: 10px; border-right:none; word-wrap: break-word; border-top:none; background-color:#d9d3d3' colspan='4'>GRAND TOTAL</td>";
                    //strp += "<td style='width:6%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none; background-color:#d9d3d3'>" + TQ.ToString() + "</td>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none; background-color:#d9d3d3'></td>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;  font-size: 10px; word-wrap: break-word; border-right:none; border-top:none; background-color:#d9d3d3'></td>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center; font-size: 10px; word-wrap: break-word;  border-right:none; border-top:none; background-color:#d9d3d3'></td>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center; font-size: 10px; word-wrap: break-word;  border-right:none; border-top:none; background-color:#d9d3d3'></td>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center; font-size: 10px; word-wrap: break-word;  border-right:none; border-top:none; background-color:#d9d3d3'></td>";
                    //strp += "<td style='width:9%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center; font-size: 10px; word-wrap: break-word;  border-right:none; border-top:none; background-color:#d9d3d3'>" + new_TOTALCGST1.ToString() + "</td>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none; background-color:#d9d3d3'></td>";
                    //strp += "<td style='width:9%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none; background-color:#d9d3d3'>" + new_TOTALCGST1.ToString() + "</td>";
                    //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: right; font-size: 10px; word-wrap: break-word; border-top:none; background-color:#d9d3d3'>" + new_SUBTOTAL1.ToString() + "</td>";
                    //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center; font-size: 10px; word-wrap: break-word;  border-right:none; border-top:none; background-color:#d9d3d3'></td>";
                    //strp += "</tr>";
                    //strp += "</table>";

                    //strp += "<table class='' style='border:2px solid #6c6c6c; border-collapse: collapse;' width='100%'>";
                    //strp += "<tr style='background-color:#d9d3d3;'>";
                    //strp += "<td style='border: 2px solid #6c6c6c; font-weight: bold; text-align: center; font-size: 10px; border-right:none; word-wrap: break-word; border-top:none;' colspan='3'>GRAND TOTAL</td>";
                    //strp += "<td style='width:6%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none;'>" + TQ.ToString() + "</td>";
                    //strp += "<td style='width:20%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; font-size: 10px; word-wrap: break-word; border-right:none; border-top:none;' colspan='4'></td>";
                    //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold; text-align: right; font-size: 10px; word-wrap: break-word; border-top:none;'>" + new_SUBTOTAL1.ToString() + "</td>";
                    //strp += "<td style='width:13%; border: 2px solid #6c6c6c; font-weight: bold; text-align: center; font-size: 10px; word-wrap: break-word; border-top:none;'></td>";
                    //strp += "</tr>";
                    //strp += "</table>";

                    //// GRAND TOTAL ROW
                    //strp += "<tr style='background-color:#d9d3d3; font-weight:bold;'>";
                    //strp += "<td colspan='3' style='border:2px solid #6c6c6c; text-align:center;'>GRAND TOTAL</td>";
                    //strp += "<td style='border:2px solid #6c6c6c; text-align:center;'>" + TQ + "</td>";
                    //strp += "<td colspan='4' style='border:2px solid #6c6c6c;'></td>";
                    //strp += "<td style='border:2px solid #6c6c6c; text-align:right;'>" + new_SUBTOTAL1 + "</td>";
                    //strp += "<td style='border:2px solid #6c6c6c;'>&nbsp;</td>";
                    //strp += "</tr>";

                    //strp += "</table>";



                    //strp += "<table class='' style='border:0' width='100%'>";
                    //strp += "<tr>";
                    //strp += "<td style='font-weight: bold; text-align: center' colspan='3'></td>";
                    //strp += "<td style='width:28%; font-weight: bold;  text-align: right; font-size: 10px; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT BEFORE TAX:</td>";
                    //strp += "<td style='width:8%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none'>" + new_SUBTOTAL1.ToString() + "</td>";
                    //strp += "<td style='border: 2px solid #6c6c6c; border-top:none'></td>";
                    //strp += "</tr>";
                    //strp += "</table>";

                    //strp += "<table class='' style='border:0' width='100%'>";
                    //strp += "<tr>";
                    //strp += "<td style='font-weight: bold;  text-align: justify;  border-right:none; border-top:none; background-color: #e31e24; color: white; vertical-align: center;' rowspan='2' colspan='3'>Amount (In Words):" + word + "</td>";
                    //strp += "<td style='width:5%; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'></td>";
                    //strp += "<td style='width:28%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL GST:</td>";
                    //strp += "<td style='width:8%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + new_TOTALGST1.ToString() + "</td>";
                    //strp += "</tr>";


                    //strp += "<tr>";
                    //strp += "<td style='width:5%; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'></td>";
                    ////strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
                    ////strp += "<td style='width:8%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + new_TOTALGSTPLUSAMO1.ToString() + "</td>";
                    //strp += "<td style='width:28%; font-weight: bold; text-align: right; font-size: 10px; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
                    //strp += "<td style='width:8%; border:2px solid #6c6c6c; font-weight: bold; text-align: right; border-top:none;'>" + new_TOTALGSTPLUSAMO1.ToString() + "</td>";
                    //strp += "</tr>";
                    //strp += "</table>";

                    // **Footer Row**
                    strp += "<tfoot>";
                    strp += "<tr style='background-color:#d9d3d3; font-weight: bold;'>";
                    strp += "<td colspan='3' style='border: 2px solid #6c6c6c; text-align: center;'>GRAND TOTAL</td>";
                    strp += $"<td style='border: 2px solid #6c6c6c; text-align: center;'>{TQ}</td>";
                    strp += "<td colspan='4' style='border: 2px solid #6c6c6c;'></td>";
                    //strp += $"<td style='border: 2px solid #6c6c6c; text-align: center;'>{new_TOTALIGST}</td>";
                    strp += $"<td style='border: 2px solid #6c6c6c; text-align: right;'>{new_SUBTOTAL1}</td>";
                    strp += "<td style='border: 2px solid #6c6c6c;'></td>";
                    strp += "</tr>";

                    // **Total Amount Before Tax Row**
                    strp += "<tr>";
                    strp += "<td colspan='4'></td>";
                    strp += "<td colspan='4' style='background-color: #e31e24; color: white; text-align: right;'>TOTAL AMOUNT BEFORE TAX:</td>";
                    strp += $"<td style='border: 2px solid #6c6c6c; text-align: right;'>{new_SUBTOTAL1}</td>";
                    strp += "</tr>";

                    // **Total GST Row**
                    strp += "<tr>";
                    strp += "<td colspan='4' style='background-color: #e31e24; color: white; text-align: left;'>Amount (In Words): " + word + "</td>";
                    //strp += "<td></td>";
                    strp += "<td colspan='4' style='background-color: #e31e24; color: white; text-align: right;'>TOTAL GST:</td>";
                    strp += $"<td style='border: 2px solid #6c6c6c; text-align: right;'>{new_TOTALGST1}</td>";
                    strp += "</tr>";

                    // **Total Amount After Tax Row**
                    strp += "<tr>";
                    strp += "<td colspan='4'></td>";
                    strp += "<td colspan='4' style='background-color: #e31e24; color: white; text-align: right;'>TOTAL AMOUNT AFTER TAX:</td>";
                    strp += $"<td style='border: 2px solid #6c6c6c; text-align: right;'>{new_TOTALGSTPLUSAMO1}</td>";
                    strp += "</tr>";

                    strp += "</tfoot>";

                    // **Close Table**
                    strp += "</table>";


                    //strp += "</td></tr><tr><td class='gap' style=''>&nbsp</td></tr></table>";

                }
                if (Session["igst"].ToString() == "YES")
                {

                    //double TOTALIGST = 0;
                    //double SUBTOTAL = 0;
                    //double TOTALGST = 0;
                    //double TOTALGSTPLUSAMO = 0;

                    double new_TOTALIGST = 0;
                    double new_SUBTOTAL = 0;
                    double new_TOTALGST = 0;
                    double new_TOTALGSTPLUSAMO = 0;

                    //strp += "<table border='0' width='100%'><tr><td class='' style='text-align: left; font-weight: bold;'>OUR QUOTE</td></tr><tr><td class='' style=''>";
                    //strp += "<table class='PaymentPhase' style='border:0' width='100%'><tr><td class='gap' style=''>&nbsp</td></tr>";
                    //strp += "<tr>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white; border-right:none;'>S.NO</td>";

                    //strp += "<td style='width:32%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>PARTICULARS</td>";
                    //strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>HSN<br> CODE</td>";
                    //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>QTY<br> (PCS)</td>";
                    //strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>BASE RATE <br> (RS)</td>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>DISC<br> (%)</td>";
                    //strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>NEW <br> RATE (RS)</td>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;  border-right:none;'>GST</td>";
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
                    //strp += "<td style='width:10%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>AMOUNT<br> (RS)</td>";
                    //strp += "<td style='width:14%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>Remarks</td>";
                    //strp += "</tr>";
                    //strp += "</table>";

                    // Start Table
                    strp += "<table class='' style='border-collapse: collapse; width:100%; border: 2px solid #6c6c6c;'>";

                    // **Table Header**
                    strp += "<thead>";
                    strp += "<tr style='background-color:#d9d3d3; font-weight: bold; text-align: center;'>";
                    strp += "<th style='width:5%; border: 2px solid #6c6c6c; background-color: #e31e24; color: white;'>S.No.</th>";
                    strp += "<th style='width:30%; border: 2px solid #6c6c6c; background-color: #e31e24; color: white;'>Product Name & Specification</th>";
                    strp += "<th style='width:7%; border: 2px solid #6c6c6c; background-color: #e31e24; color: white;'>HSN</th>";
                    strp += "<th style='width:8%; border: 2px solid #6c6c6c; background-color: #e31e24; color: white;'>Qty</th>";
                    strp += "<th style='width:10%; border: 2px solid #6c6c6c; background-color: #e31e24; color: white;'>BASE RATE <br> (RS)</th>";
                    strp += "<th style='width:5%; border: 2px solid #6c6c6c; background-color: #e31e24; color: white;'>DISC<br> (%)</th>";
                    strp += "<th style='width:10%; border: 2px solid #6c6c6c; background-color: #e31e24; color: white;'>NEW RATE (RS)</th>";
                    strp += "<th style='width:5%; border: 2px solid #6c6c6c; background-color: #e31e24; color: white;'>GST %</th>";
                    //strp += "<th style='width:9%; border: 2px solid #6c6c6c;'>IGST</th>";
                    strp += "<th style='width:10%; border: 2px solid #6c6c6c; background-color: #e31e24; color: white;'>AMOUNT<br> (RS)</th>";
                    strp += "<th style='width:19%; border: 2px solid #6c6c6c; background-color: #e31e24; color: white;'>Remarks</th>";
                    strp += "</tr>";
                    strp += "</thead>";



                    //for (int i = 0; i < dtp.Rows.Count; i++)
                    //{
                    //    string HSN = dtp.Rows[i]["HSN"].ToString();
                    //    string Productname = dtp.Rows[i]["Product_name"].ToString();
                    //    string specification = dtp.Rows[i]["specification"].ToString();
                    //    int Quantity = Convert.ToInt32(dtp.Rows[i]["Quantity"]);
                    //    TQ = TQ + Quantity;
                    //    double baserate = Math.Round((Convert.ToDouble(dtp.Rows[i]["sail_rate"])), 2);
                    //    double discountrate = Math.Round((Convert.ToDouble(dtp.Rows[i]["new_sailrate"])), 2);
                    //    int gstper = Convert.ToInt32(dtp.Rows[i]["Service_tax_rate"]);
                    //    int discper = Convert.ToInt32(dtp.Rows[i]["discount_rate"]);
                    //    //double QuantityBaserateAmo = Math.Round((Quantity * baserate), 2);
                    //    double new_QuantityBaserateAmo = Math.Round((Quantity * discountrate), 2);
                    //    string itemremarks = dtp.Rows[i]["ItemRemarks"].ToString();
                    //    //string QuantityBaserateAmo1 = DoFormat(QuantityBaserateAmo);
                    //    string new_QuantityBaserateAmo1 = DoFormat(new_QuantityBaserateAmo);

                    //    //double gstamount = Math.Round(((new_QuantityBaserateAmo * gstper) / 100), 2);
                    //    double new_gstamount = Math.Round(((new_QuantityBaserateAmo * gstper) / 100), 2);

                    //    double cgstper = Math.Round((Convert.ToDouble(gstper) / 2), 2);

                    //    double cgstamo = Math.Round((Convert.ToDouble(new_gstamount) / 2), 2);

                    //    //TOTALIGST = TOTALIGST + gstamount;
                    //    //SUBTOTAL = SUBTOTAL + QuantityBaserateAmo;
                    //    //TOTALGST = TOTALGST + gstamount;

                    //    new_TOTALIGST = new_TOTALIGST + new_gstamount;
                    //    new_SUBTOTAL = new_SUBTOTAL + new_QuantityBaserateAmo;

                    //    new_TOTALGST = new_TOTALGST + new_gstamount;


                    //    strp += "<table class='' style='border:0' width='100%'>";
                    //    strp += "<tr>";
                    //    strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + (i + 1).ToString() + "</td>";
                    //    strp += "<td style='width:32%; border:2px solid #6c6c6c; font-weight: bold;  text-align: left;     border-right:none; border-top:none;'>" + Productname + "<br>" + specification + "</td>";
                    //    strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + HSN + "</td>";
                    //    strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + Quantity.ToString() + "</td>";
                    //    strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + baserate.ToString() + "</td>";
                    //    strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + discper.ToString() + "</td>";
                    //    strp += "<td style='width:7%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + discountrate.ToString() + "</td>";
                    //    strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";
                    //    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";
                    //    //strp += "<td style='width:9%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;'>" + new_gstamount.ToString() + "</td>";
                    //    strp += "<td style='width:14%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;   border-top:none;'>" + new_QuantityBaserateAmo1.ToString() + "</td>";
                    //    strp += "<td style='width:14%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;   border-top:none;'>" + itemremarks + "</td>";
                    //    strp += "</tr>";
                    //    strp += "</table>";
                    //}

                    // **Table Body - Item Rows**
                    strp += "<tbody>";

                    for (int i = 0; i < dtp.Rows.Count; i++)
                    {
                        string HSN = dtp.Rows[i]["HSN"].ToString();
                        string Productname = dtp.Rows[i]["Product_name"].ToString();
                        string specification = dtp.Rows[i]["specification"].ToString();
                        string itemno = dtp.Rows[i]["ItemNo"].ToString();
                        string materialno = dtp.Rows[i]["MaterialNo"].ToString();
                        string packsize = dtp.Rows[i]["PackSize"].ToString();

                        int Quantity = Convert.ToInt32(dtp.Rows[i]["Quantity"]);
                        TQ += Quantity;
                        double baserate = Math.Round(Convert.ToDouble(dtp.Rows[i]["sail_rate"]), 2);
                        double discountrate = Math.Round(Convert.ToDouble(dtp.Rows[i]["new_sailrate"]), 2);
                        int gstper = Convert.ToInt32(dtp.Rows[i]["Service_tax_rate"]);
                        int discper = Convert.ToInt32(dtp.Rows[i]["discount_rate"]);
                        double new_QuantityBaserateAmo = Math.Round(Quantity * discountrate, 2);
                        string itemremarks = dtp.Rows[i]["ItemRemarks"].ToString();
                        string new_QuantityBaserateAmo1 = DoFormat(new_QuantityBaserateAmo);
                        double new_gstamount = Math.Round((new_QuantityBaserateAmo * gstper) / 100, 2);
                        new_TOTALIGST += new_gstamount;
                        new_SUBTOTAL += new_QuantityBaserateAmo;
                        new_TOTALGST += new_gstamount;

                        strp += "<tr>";
                        strp += $"<td style='border: 2px solid #6c6c6c; text-align: center;'>{i + 1}</td>";
                        //strp += $"<td style='border: 2px solid #6c6c6c; text-align: left;'>{Productname}<br>{specification}</td>";
                        if (Session["viewtype"].ToString() == "Yes")
                        {
                            strp += string.Format("<td style='border: 2px solid #6c6c6c; text-align: left; padding: 5px;'>" +
                                   "<div><span style='font-weight: bold; color: black;'>{0}</span></div>" +
                                   "<div style='font-style: italic; font-size: 10px; color: gray;'>Make: {1}</div>" +
                                   "<div style='font-style: italic; font-size: 10px; color: gray;'>Item No: {2}</div>" +
                                   "<div style='font-style: italic; font-size: 10px; color: gray;'>Material No: {3}</div>" +
                                   "<div style='font-style: italic; font-size: 10px; color: gray;'>Pack Size: {4}</div>" +
                                   "</td>", Productname, specification, itemno, materialno, packsize);
                        }
                        else
                        {
                            strp += $"<td style='border: 2px solid #6c6c6c; text-align: left;'><span style='font-weight: bold; color: black;'>{Productname}</span><br>&nbsp;&nbsp;<span style='font-style: italic; font-size: 10px; color: gray;'>Make:{specification}</span></td>";
                        }

                        strp += $"<td style='border: 2px solid #6c6c6c; text-align: center;'>{HSN}</td>";
                        strp += $"<td style='border: 2px solid #6c6c6c; text-align: center;'>{Quantity}</td>";
                        strp += $"<td style='border: 2px solid #6c6c6c; text-align: center;'>{baserate}</td>";
                        strp += $"<td style='border: 2px solid #6c6c6c; text-align: center;'>{discper}</td>";
                        strp += $"<td style='border: 2px solid #6c6c6c; text-align: center;'>{discountrate}</td>";
                        strp += $"<td style='border: 2px solid #6c6c6c; text-align: center;'>{gstper}%</td>";
                        //strp += $"<td style='border: 2px solid #6c6c6c; text-align: center;'>{new_gstamount}</td>";
                        strp += $"<td style='border: 2px solid #6c6c6c; text-align: right;'>{new_QuantityBaserateAmo1}</td>";
                        strp += $"<td style='border: 2px solid #6c6c6c; text-align: left;'>{itemremarks}</td>";
                        strp += "</tr>";
                    }

                    strp += "</tbody>";

                    // **Footer Calculations**
                    new_TOTALGSTPLUSAMO = new_TOTALGST + new_SUBTOTAL;
                    string new_SUBTOTAL1 = DoFormat(new_SUBTOTAL);
                    string final_new_TOTALGSTPLUSAMO = DoFormat(new_TOTALGSTPLUSAMO);
                    string grandtotal = Math.Round(Convert.ToDouble(new_TOTALGSTPLUSAMO)).ToString();
                    string word = MoneyConvDS.MoneyConvFn(final_new_TOTALGSTPLUSAMO);

                    // **Footer Row**
                    strp += "<tfoot>";
                    strp += "<tr style='background-color:#d9d3d3; font-weight: bold;'>";
                    strp += "<td colspan='3' style='border: 2px solid #6c6c6c; text-align: center;'>GRAND TOTAL</td>";
                    strp += $"<td style='border: 2px solid #6c6c6c; text-align: center;'>{TQ}</td>";
                    strp += "<td colspan='4' style='border: 2px solid #6c6c6c;'></td>";
                    //strp += $"<td style='border: 2px solid #6c6c6c; text-align: center;'>{new_TOTALIGST}</td>";
                    strp += $"<td style='border: 2px solid #6c6c6c; text-align: right;'>{new_SUBTOTAL1}</td>";
                    strp += "<td style='border: 2px solid #6c6c6c;'></td>";
                    strp += "</tr>";

                    // **Total Amount Before Tax Row**
                    strp += "<tr>";
                    strp += "<td colspan='4'></td>";
                    strp += "<td colspan='4' style='background-color: #e31e24; color: white; text-align: right;'>TOTAL AMOUNT BEFORE TAX:</td>";
                    strp += $"<td style='border: 2px solid #6c6c6c; text-align: right;'>{new_SUBTOTAL1}</td>";
                    strp += "</tr>";

                    // **Total GST Row**
                    strp += "<tr>";
                    strp += "<td colspan='4' style='background-color: #e31e24; color: white; text-align: left;'>Amount (In Words): " + word + "</td>";
                    //strp += "<td></td>";
                    strp += "<td colspan='4' style='background-color: #e31e24; color: white; text-align: right;'>TOTAL GST:</td>";
                    strp += $"<td style='border: 2px solid #6c6c6c; text-align: right;'>{new_TOTALGST}</td>";
                    strp += "</tr>";

                    // **Total Amount After Tax Row**
                    strp += "<tr>";
                    strp += "<td colspan='4'></td>";
                    strp += "<td colspan='4' style='background-color: #e31e24; color: white; text-align: right;'>TOTAL AMOUNT AFTER TAX:</td>";
                    strp += $"<td style='border: 2px solid #6c6c6c; text-align: right;'>{final_new_TOTALGSTPLUSAMO}</td>";
                    strp += "</tr>";

                    strp += "</tfoot>";

                    // **Close Table**
                    strp += "</table>";


                    ////TOTALGSTPLUSAMO = TOTALGST + SUBTOTAL;
                    //new_TOTALGSTPLUSAMO = new_TOTALGST + new_SUBTOTAL;
                    //string new_SUBTOTAL1 = DoFormat(new_SUBTOTAL);
                    //string final_new_TOTALGSTPLUSAMO = DoFormat(new_TOTALGSTPLUSAMO);

                    //string grandtotal = (Math.Round((Convert.ToDouble(new_TOTALGSTPLUSAMO))).ToString());
                    //string grandtotal1 = DoFormat(Math.Round((Convert.ToDouble(new_TOTALGSTPLUSAMO)))).ToString();
                    //string word = MoneyConvDS.MoneyConvFn(final_new_TOTALGSTPLUSAMO);

                    //strp += "<table class='' style='border:0' width='100%'>";
                    //strp += "<tr>";
                    //strp += "<td style='border:2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none; background-color:#d9d3d3' colspan='3'>GRAND TOTAL</td>";
                    //strp += "<td style='width:8%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'>" + TQ.ToString() + "</td>";
                    //strp += "<td style='width:7%; border:2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
                    //strp += "<td style='width:7%; border:2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
                    //strp += "<td style='width:5%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'></td>";
                    //strp += "<td style='width:9%; border: 2px solid #6c6c6c; font-weight: bold;  text-align: center;   border-right:none; border-top:none;background-color:#d9d3d3'>" + new_TOTALIGST.ToString() + "</td>";
                    ////strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;   border-top:none;background-color:#d9d3d3'>" + Math.Round(SUBTOTAL,2).ToString() + ".00" + "</td>";
                    //strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;   border-top:none;background-color:#d9d3d3'>" + new_SUBTOTAL1.ToString() + "</td>";
                    //strp += "<td style='width:14%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;   border-top:none;background-color:#d9d3d3'>&nbsp;</td>";
                    //strp += "</tr>";
                    //strp += "</table>";


                    //strp += "<table class='' style='border:0' width='100%'>";
                    //strp += "<tr>";
                    //strp += "<td style='font-weight: bold;  text-align: center' colspan='6'></td>";
                    //strp += "<td style='width:28%; font-weight: bold;  text-align: right; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT BEFORE TAX:</td>";
                    ////strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none'>" + Math.Round(SUBTOTAL,2).ToString() + ".00" + "</td>";
                    //strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none'>" + new_SUBTOTAL1.ToString() + "</td>";
                    //strp += "</tr>";
                    //strp += "</table>";



                    ////strp += "<table class='' style='border:0' width='100%'>";
                    ////strp += "<tr>";
                    ////strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none' colspan='6'></td>";
                    ////strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL GST:</td>";
                    ////strp += "<td style='width:10%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;  border-top:none;'>" + Math.Round(TOTALGST).ToString()+".00" + "</td>";
                    ////strp += "</tr>";
                    ////strp += "</table>";

                    ////strp += "<table class='' style='border:0' width='100%'>";
                    ////strp += "<tr>";
                    ////strp += "<td style='font-weight: bold;  text-align: center;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='5'>Amount (In Words):" + word + "</td>";
                    ////strp += "<td style='width:5%; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'></td>";
                    ////strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
                    ////strp += "<td style='width:10%; border:1px solid #bfbfbf; font-weight: bold;  text-align: right;  border-top:none;'>" + grandtotal.ToString()+".00" + "</td>";
                    ////strp += "</tr>";
                    ////strp += "</table>";

                    //strp += "<table class='' style='border:0' width='100%'>";
                    //strp += "<tr>";
                    //strp += "<td style='font-weight: bold;  text-align: justify;  border-right:none; border-top:none; background-color: #e31e24; color: white; vertical-align: top;' rowspan='2' colspan='5'>Amount (In Words):" + word + "</td>";
                    //strp += "<td style='width:5%; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'></td>";
                    //strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL GST:</td>";
                    ////strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + Math.Round(TOTALGST,2).ToString() + ".00" + "</td>";
                    //strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + new_TOTALGST.ToString() + "</td>";
                    //strp += "</tr>";

                    //strp += "<tr>";
                    //strp += "<td style='width:5%; font-weight: bold;  text-align: center;  border-right:none; border-top:none;'></td>";
                    //strp += "<td style='width:28%; font-weight: bold;  text-align: right;  border-right:none; border-top:none; background-color: #e31e24; color: white;' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
                    //strp += "<td style='width:10%; border:2px solid #6c6c6c; font-weight: bold;  text-align: right;  border-top:none;'>" + final_new_TOTALGSTPLUSAMO + "</td>";
                    //strp += "</tr>";
                    //strp += "</table>";

                    //strp += "</td></tr><tr><td class='gap' style=''>&nbsp</td></tr></table>";
                }
                lblserviceamo.Text = strp.ToString();
            }
        }


        private void bindpayment(string qutno)
        {
            string cmdstring = "select phase_type,PhaseDesc,amountper from tbl_QutPaymentPhase where qut_no=@qut_no order by id";
            SqlParameter[] pram = {
                 new SqlParameter("@qut_no",qutno)
            };
            dtpayphase = DbCL.SPreturn_dt(cmdstring, pram);

            if (dtpayphase.Rows.Count > 0)
            {
                //strPayment.Append("<table border='0' width='100%' class='Payment pagebrake'><tr><td class='' style='text-align: left; font-weight: bold; '>SCHEDULE OF PAYMENTS</td></tr><tr><td class='' style=''>");
                //strPayment.Append("<table class='PaymentPhase' style='border:0' width='100%'><tr><td class='gap' style=''>&nbsp</td></tr>");
                //strPayment.Append("<tr>");
                //strPayment.Append("<td style='width: 10%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>S.NO</td>");
                //strPayment.Append("<td style='width: 70%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>PAYMENT PHASE</td>");
                //strPayment.Append("<td style='width: 20%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>AMOUNT (INR)</td>");
                //strPayment.Append("</tr>");

                //for (int i = 0; i < dtpayphase.Rows.Count; i++)
                //{
                //    string amoper = dtpayphase.Rows[i]["amountper"].ToString();
                //    double amountper = Convert.ToDouble(amoper);
                //    double netamo = Convert.ToDouble(netamount);
                //    double amount = (netamo * amountper) / 100;

                //    double finalamo = Math.Round(amount,2);
                //    //double finalamo = Math.Round(amount);
                //    string finalamo1 = DoFormat(finalamo);

                //    strPayment.Append("<tr>");
                //    strPayment.Append("<td style='width: 10%; border: 2px solid #6c6c6c; text-align:center;'>" + (i + 1).ToString() + "</td>");
                //    strPayment.Append("<td style='width: 70%; border: 2px solid #6c6c6c; text-align:left; '>" + dtpayphase.Rows[i]["phase_type"].ToString() + " " + dtpayphase.Rows[i]["PhaseDesc"].ToString() + "</td>");
                //    strPayment.Append("<td style='width: 20%; border: 2px solid #6c6c6c; text-align:center;'>" + finalamo1.ToString() + "</td>");
                //    strPayment.Append("</tr>");
                //}

                //strPayment.Append("</table>");
                //strPayment.Append("</td></tr><tr><td class='gap' style=''>&nbsp</td></tr></table>");

                strPayment.Append("<table border='0' width='100%' class='Payment pagebrake' style='border-collapse: collapse;'>");
                strPayment.Append("<tr><td style='text-align: left; font-weight: bold;'>SCHEDULE OF PAYMENTS</td></tr>");
                strPayment.Append("<tr><td>");

                // Inner Table (Actual Payment Table)
                strPayment.Append("<table class='PaymentPhase' width='100%' style='border-collapse: collapse;'>");

                // Header Row
                strPayment.Append("<tr>");
                strPayment.Append("<th style='width: 10%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>S.NO</th>");
                strPayment.Append("<th style='width: 70%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>PAYMENT PHASE</th>");
                strPayment.Append("<th style='width: 20%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #e31e24; text-align: center; color: white;'>AMOUNT (INR)</th>");
                strPayment.Append("</tr>");

                // Data Rows
                for (int i = 0; i < dtpayphase.Rows.Count; i++)
                {
                    string amoper = dtpayphase.Rows[i]["amountper"].ToString();
                    double amountper = Convert.ToDouble(amoper);
                    double netamo = Convert.ToDouble(netamount);
                    double amount = (netamo * amountper) / 100;

                    double finalamo = Math.Round(amount, 2);
                    string finalamo1 = DoFormat(finalamo);

                    strPayment.Append("<tr>");
                    strPayment.Append("<td style='width: 10%; border: 2px solid #6c6c6c; text-align: center;'>" + (i + 1) + "</td>");
                    strPayment.Append("<td style='width: 70%; border: 2px solid #6c6c6c; text-align: left;'>" + dtpayphase.Rows[i]["phase_type"].ToString() + " " + dtpayphase.Rows[i]["PhaseDesc"].ToString() + "</td>");
                    strPayment.Append("<td style='width: 20%; border: 2px solid #6c6c6c; text-align: center;'>" + finalamo1 + "</td>");
                    strPayment.Append("</tr>");
                }

                // Close Inner Table
                strPayment.Append("</table>");
                strPayment.Append("</td></tr>");
                strPayment.Append("</table>");

                lblPayment.Text = strPayment.ToString();

            }
            DbCL.Conn.Close();
        }

        private void Bindclientdetails(string clientid)
        {
            string query = "select Client_Name,Address1,Address2,City,pin,State,Service_tax_no,Pan_no,PlaceofSupply from tbl_Client where Client_Id=@Client_Id";
            SqlParameter[] pram = {
            new SqlParameter("@Client_Id",clientid)
            };
            dtClient = DbCL.SPreturn_dt(query, pram);
            if (dtClient.Rows.Count > 0)
            {
                string Client_Name = dtClient.Rows[0]["Client_Name"].ToString();
                string Address1 = dtClient.Rows[0]["Address1"].ToString();
                string Address2 = dtClient.Rows[0]["Address2"].ToString();
                string add = "";
                if (Address1 == Address2 || Address2 =="(Blank)" || Address2 == "N/A" || Address2 == string.Empty)
                {
                    add = Address1;
                }
                else
                {
                    add = Address1 + " " + Address2;
                }

                string City = dtClient.Rows[0]["City"].ToString();
                string pin = dtClient.Rows[0]["pin"].ToString();
                string State = dtClient.Rows[0]["State"].ToString();
                string Gstno = dtClient.Rows[0]["Service_tax_no"].ToString();
                string Panno = dtClient.Rows[0]["Pan_no"].ToString();

                lblPanno.Text = Panno;
                lblGstno.Text = Gstno;

                TextInfo textInfo1 = cultureInfo.TextInfo;
                lblClient.Text = textInfo1.ToTitleCase(Client_Name.ToLower());

                txtaddres.Text = textInfo1.ToTitleCase(add.ToLower());
                lblcity.Text = textInfo1.ToTitleCase(City.ToLower());
                lblpincode.Text = textInfo1.ToTitleCase((pin + "-" + State).ToLower());


                //string placeofsupply = dtClient.Rows[0]["PlaceofSupply"].ToString();
                //lblplaceofsup1.Text = "Place Of Supply";
                //lblplaceofsup2.Text = ":";
                //lblplaceofsup3.Text = placeofsupply;

            }
        }

        private void BindRepresentative(string clientid)
        {
            string query = "select Representative_name,Designation,Phone_no,Email,RepTitle,RepLastName from tbl_representative where Copany_Id=@Copany_Id";
            SqlParameter[] pram = {
            new SqlParameter("@Copany_Id",clientid)
            };
            dtRepre = DbCL.SPreturn_dt(query, pram);
            if (dtRepre.Rows.Count > 0)
            {
                string Representative_name = dtRepre.Rows[0]["Representative_name"].ToString();
                string Designation = dtRepre.Rows[0]["Designation"].ToString();
                string Phone_no = dtRepre.Rows[0]["Phone_no"].ToString();
                string Email = dtRepre.Rows[0]["Email"].ToString();

                string RepTitle = dtRepre.Rows[0]["RepTitle"].ToString();
                string RepLastName = dtRepre.Rows[0]["RepLastName"].ToString();


                TextInfo textInfo1 = cultureInfo.TextInfo;
                string rename = RepTitle + " " + Representative_name + " " + RepLastName;

                lblrename.Text = textInfo1.ToTitleCase(rename.ToLower());
                lbltital.Text = RepTitle;
                lbllname.Text = RepLastName;
                lbldeg.Text = textInfo1.ToTitleCase(Designation.ToLower());
            }
        }

        private void BindService(string qutno)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            string cmdstring = "select count(*) from tbl_QutPrimaryService where qut_no='" + qutno.ToString() + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            Int32 count = Convert.ToInt32(cmd.ExecuteScalar());
            generatelavel(count, qutno);
            DbCL.Conn.Close();

            /*string query = "select PrimaryService from tbl_QutPrimaryService where qut_no=@qut_no order by id";
            SqlParameter[] pram = {
            new SqlParameter("@qut_no",qutno)
            };
            dtService = DbCL.SPreturn_dt(query, pram);
            if (dtService.Rows.Count > 0)
            {
                string PrimaryService = "";
                for (int i=0; i < dtService.Rows.Count; i++)
                {
                    string Service = dtService.Rows[i]["PrimaryService"].ToString();
                    Service = "“" + Service + "”";
                    if (i == 0)
                    {
                        PrimaryService = Service;
                    }
                    else if (i == 1)
                    {
                        PrimaryService = PrimaryService +" and "+ Service;
                    }
                    else
                    {
                        PrimaryService = PrimaryService +" , "+ Service;
                    }*/
          }

        private void generatelavel(int count, string qutno)
        {
            string PrimaryService = "";

            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string service = null;
            int flag = 1;
            string cmdstring = "select PrimaryService from tbl_QutPrimaryService where qut_no='" + qutno.ToString() + "' order by id";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            while (re.Read())
            {
                string name = re["PrimaryService"].ToString();
                name= "“"+name + "”";
                if (count == 1 || flag == 1)
                {
                    //service = "“" + re["PrimaryService"].ToString() + "”";
                    service = name;
                    flag = flag + 1;
                }
                
                else if (flag == count)
                {
                    service = service + " & " + name.ToString();
                    flag = flag + 1;

                }
                else
                {
                    service = service + ", " + name.ToString();
                    flag = flag + 1;
                }
            }
            DbCL.Conn.Close();
            PrimaryService = service.ToString();
            TextInfo textInfo1 = cultureInfo.TextInfo;
            lblservice.Text = textInfo1.ToTitleCase(PrimaryService.ToString().ToLower());
            lblPrimaryService.Text = textInfo1.ToTitleCase(PrimaryService.ToString().ToLower());
        }
    
        protected void Button1_Click(object sender, EventArgs e)
        {

        }

        protected void Button2_Click(object sender, EventArgs e)
        {

        }
    }
}
