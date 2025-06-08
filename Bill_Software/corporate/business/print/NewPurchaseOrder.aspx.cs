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
    public partial class NewPurchaseOrder : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();

        public string taxorvat = "";
        public string proOrser = "";
        public string psid = "";
        public string str = "";
        public string strp = "";

        DataTable dtgcs = new DataTable();
        DataTable dtp = new DataTable();
        StringBuilder strProduct = new StringBuilder();
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
        public static string viewtype = string.Empty;

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
            string query = "SELECT ID, Quotation_no, Quotation_date, Client_Id, PlaceofSupply, ReferenceName, ReferenceData, ReferenceId, ReferenceDate, ValidityDays, DeliveryTenure, PackingCharges, Remarks, DetailedView, DO_Number, PO_Number, PO_Date, Validity_StartDate, Validity_EndDate, TimsStamp, sub_total, Net_amount, cgstOrsgst, igst from tbl_Quotation where ID=@id";
            SqlParameter[] pram = {
            new SqlParameter("@id",id)
            };
            dtmain = DbCL.SPreturn_dt(query, pram);
            if (dtmain.Rows.Count > 0)
            {
                string qutno = dtmain.Rows[0]["Quotation_no"].ToString();
                lblqnumber.Text = qutno;

                DateTime qdate;
                if (DateTime.TryParse(dtmain.Rows[0]["Quotation_date"].ToString(), out qdate))
                    lbldate.Text = qdate.ToString("dd-MMM-yyyy");
                else
                    lbldate.Text = "No Data";

                Session["Quotation_date"] = lbldate.Text;

                string clientid = dtmain.Rows[0]["Client_Id"].ToString();
                lblClientCode.Text = clientid;

                lbl_ponumber.Text = dtmain.Rows[0]["PO_Number"].ToString();

                DateTime podate;
                if (DateTime.TryParse(dtmain.Rows[0]["PO_Date"].ToString(), out podate))
                    lbl_podate.Text = podate.ToString("dd-MMM-yyyy");
                else
                    lbl_podate.Text = "No Data";

                lbl_donumber.Text = dtmain.Rows[0]["DO_Number"].ToString();

                string refYesno = dtmain.Rows[0]["ReferenceData"].ToString();
                string refname = dtmain.Rows[0]["ReferenceName"].ToString();
                string refid = dtmain.Rows[0]["ReferenceId"].ToString();
                string refdate = dtmain.Rows[0]["ReferenceDate"].ToString();

                DateTime parsedRefDate;
                string formattedRefDate = (DateTime.TryParse(refdate, out parsedRefDate) && parsedRefDate != DateTime.MinValue && parsedRefDate.ToString("yyyy-MM-dd") != "1900-01-01")
                    ? parsedRefDate.ToString("dd-MMM-yyyy")
                    : "No Data";

                if (refYesno == "Yes")
                {
                    lbl_refname.Text = refname;
                    lbl_refid.Text = refid;
                    lbl_refdate.Text = formattedRefDate;

                    if (string.IsNullOrEmpty(refname) || refname == "N/A")
                        lbl_refname.Visible = false;

                    if (string.IsNullOrEmpty(refid) || refid == "N/A")
                        Tr2.Visible = false;

                    if (string.IsNullOrEmpty(formattedRefDate) || formattedRefDate == "No Data")
                        Tr3.Visible = false;
                }
                else
                {
                    lbl_refname.Text = refname;
                    lbl_refid.Text = refid;
                    lbl_refdate.Text = formattedRefDate;

                    if (string.IsNullOrEmpty(refname) || refname == "N/A")
                        lbl_refname.Visible = false;

                    if (string.IsNullOrEmpty(refid) || refid == "N/A")
                        Tr2.Visible = false;

                    if (string.IsNullOrEmpty(formattedRefDate) || formattedRefDate == "No Data")
                        Tr3.Visible = false;
                }

                //lbl_valdays.Text = dtmain.Rows[0]["ValidityDays"].ToString();
                //lbl_deliverytrms.Text = dtmain.Rows[0]["DeliveryTenure"].ToString();
                //lbl_pkging.Text = dtmain.Rows[0]["PackingCharges"].ToString();
                //lbl_remarks.Text = dtmain.Rows[0]["Remarks"].ToString();

                string sub_total = dtmain.Rows[0]["sub_total"].ToString();
                netamount = dtmain.Rows[0]["Net_amount"].ToString();

                Session["cgstOrsgst"] = dtmain.Rows[0]["cgstOrsgst"].ToString();
                Session["igst"] = dtmain.Rows[0]["igst"].ToString();
                viewtype = dtmain.Rows[0]["DetailedView"].ToString();

                string word = MoneyConvDS.MoneyConvFn(netamount);
                //blword.Text = word.ToString();

                Bindclientdetails(clientid);
                BindRepresentative(clientid);
                BindService(qutno);
                Buindamount(qutno);
                bindpayment(qutno);
                BuindamountByQuotation(qutno);
                bindPrimaryServiceTerms(qutno);

                string placeofsupply = dtmain.Rows[0]["PlaceofSupply"].ToString();
                lblplaceofsup1.Text = "Place Of Supply";
                lblplaceofsup2.Text = ":";
                lblplaceofsup3.Text = placeofsupply;

            }
        }

        private void BuindamountByQuotation(string quotationNo)
        {
            string chalanQuery = "SELECT Chalan_No, Chalan_Date FROM tbl_Chalan WHERE Quotation_No = @Quotation_No ORDER BY Chalan_Date";
                    SqlParameter[] chalanParam = {
                new SqlParameter("@Quotation_No", quotationNo),
            };

            DataTable dtChalanList = DbCL.SPreturn_dt(chalanQuery, chalanParam);
            if (dtChalanList.Rows.Count > 0)
            {
                strProduct.Clear(); // Reset the string builder in case it's reused
                

                foreach (DataRow chRow in dtChalanList.Rows)
                {
                    int serial = 1;

                    string chalanNo = chRow["Chalan_No"].ToString();
                    string chalanDateStr = chRow["Chalan_Date"].ToString();
                    DateTime chDate;
                    //string formattedDate = DateTime.TryParse(chalanDateStr, out chDate) ? chDate.ToString("dd-MMM-yyyy") : "";

                    string formattedDate = "";
                    if (DateTime.TryParse(chalanDateStr, out chDate))
                    {
                        formattedDate = chDate.ToString("dd-MMM-yyyy");

                        int daysDiff = (DateTime.Today - chDate.Date).Days;
                        if (daysDiff == 0)
                        {
                            formattedDate += " (Today)";
                        }
                        else if (daysDiff > 0)
                        {
                            formattedDate += $" ({daysDiff} day{(daysDiff > 1 ? "s" : "")} ago)";
                        }
                        else
                        {
                            formattedDate += $" ({Math.Abs(daysDiff)} day{(Math.Abs(daysDiff) > 1 ? "s" : "")} left)";
                        }
                    }

                    strProduct.Append("<div style='margin-bottom:10px;'>");
                    strProduct.Append("<b>Challan No:</b> " + chalanNo + " &nbsp;&nbsp; <b>Date:</b> " + formattedDate);
                    strProduct.Append("</div>");

                    //string detailQuery = "SELECT Sl_no, Product_id, Product_code, Product_name, Quantity FROM tbl_Challan_details WHERE Challan_no = @Challan_no ORDER BY Product_name";
                    //        SqlParameter[] detailParam = {
                    //    new SqlParameter("@Challan_no", chalanNo)
                    //};

                    string detailQuery = @"
                        SELECT 
                            cd.Sl_no, cd.Product_id,
                            cd.Product_code, cd.Product_name,
                            cd.Quantity, qd.specification,
                            qd.ItemNo,
                            qd.MaterialNo,
                            qd.PackSize,
                            qd.Department
                        FROM tbl_Challan_details cd
                        INNER JOIN tbl_Chalan c ON cd.Challan_no = c.Chalan_No
                        INNER JOIN tbl_Quotaion_details qd 
                            ON cd.Product_id = qd.Product_Code AND c.Quotation_No = qd.Quotation_no
                        WHERE cd.Challan_no = @Challan_no
                        ORDER BY qd.Sl_no;";

                                            SqlParameter[] detailParam = {
                            new SqlParameter("@Challan_no", chalanNo)
                        };

                    DataTable dtDetails = DbCL.SPreturn_dt(detailQuery, detailParam);
                    if (dtDetails.Rows.Count > 0)
                    {
                        int totalQty = 0;

                        strProduct.Append("<table style='border-collapse:collapse; width:100%; border:0'>");
                        strProduct.Append("<tr>");
                        strProduct.Append("<th style='width:5%; border: 1px solid #bfbfbf; background-color: #24285F; color:white;'>S.NO</th>");
                        strProduct.Append("<th style='width:30%; border: 1px solid #bfbfbf; background-color: #24285F; color:white;'>PARTICULARS</th>");
                        strProduct.Append("<th style='width:5%; border: 1px solid #bfbfbf; background-color: #24285F; color:white;'>PRODUCT ID</th>");
                        strProduct.Append("<th style='width:10%; border: 1px solid #bfbfbf; background-color: #24285F; color:white;'>HSN CODE</th>");
                        strProduct.Append("<th style='width:10%; border: 1px solid #bfbfbf; background-color: #24285F; color:white;'>QTY</th>");
                        strProduct.Append("<th style='width:10%; border: 1px solid #bfbfbf; background-color: #24285F; color:white;'>PACK SIZE</th>");
                        strProduct.Append("<th style='width:10%; border: 1px solid #bfbfbf; background-color: #24285F; color:white;'>ITEM NO</th>");
                        strProduct.Append("<th style='width:10%; border: 1px solid #bfbfbf; background-color: #24285F; color:white;'>MATERIAL NO</th>");
                        strProduct.Append("<th style='width:10%; border: 1px solid #bfbfbf; background-color: #24285F; color:white;'>DEPT</th>");
                        strProduct.Append("</tr>");

                        foreach (DataRow dRow in dtDetails.Rows)
                        {
                            string productName = dRow["Product_name"].ToString();
                            string productId = dRow["Product_id"].ToString();
                            string hsn = dRow["Product_code"].ToString();
                            string qtyStr = dRow["Quantity"].ToString();
                            int parsedQty = 0;
                            int qty = int.TryParse(qtyStr, out parsedQty) ? parsedQty : 0;
                            string specification = dRow["specification"].ToString();
                            string itemNo = dRow["ItemNo"].ToString();
                            string MaterialNo = dRow["MaterialNo"].ToString();
                            string packSize = dRow["PackSize"].ToString();
                            string department = dRow["Department"].ToString();

                            strProduct.Append("<tr>");
                            strProduct.Append("<td style='border: 1px solid #bfbfbf; text-align: center;'>" + serial + "</td>");
                            strProduct.Append("<td style='border: 1px solid #bfbfbf;'>" + productName + "</td>");
                            strProduct.Append("<td style='border: 1px solid #bfbfbf;'>" + productId + "</td>");
                            strProduct.Append("<td style='border: 1px solid #bfbfbf; text-align: center;'>" + hsn + "</td>");
                            strProduct.Append("<td style='border: 1px solid #bfbfbf; text-align: center;'>" + qty + "</td>");
                            strProduct.Append("<td style='border: 1px solid #bfbfbf; text-align: center;'>" + packSize + "</td>");
                            strProduct.Append("<td style='border: 1px solid #bfbfbf; text-align: center;'>" + itemNo + "</td>");
                            strProduct.Append("<td style='border: 1px solid #bfbfbf; text-align: center;'>" + MaterialNo + "</td>");
                            strProduct.Append("<td style='border: 1px solid #bfbfbf; text-align: center;'>" + department + "</td>");
                            strProduct.Append("</tr>");

                            totalQty += qty;
                            serial++;
                        }

                        // Total row
                        strProduct.Append("<tr>");
                        strProduct.Append("<td colspan='3' style='border: 1px solid #bfbfbf;'></td>");
                        strProduct.Append("<td style='border: 1px solid #bfbfbf; font-weight: bold; text-align: right; background-color: #24285F; color: white;'>Total Quantity</td>");
                        strProduct.Append("<td style='border: 1px solid #bfbfbf; text-align: center; font-weight: bold; background-color: #24285F; color: white;'>" + totalQty + "</td>");
                        strProduct.Append("<td colspan='4' style='border: 1px solid #bfbfbf;'></td>");
                        strProduct.Append("</tr>");

                        strProduct.Append("</table>");
                        strProduct.Append("<br/>");
                        strProduct.Append("<br/>");
                    }
                }

                lblProductDetails.Text = strProduct.ToString();
            }
            else
            {
                lblProductDetails.Text = "<i>No challan data found for this quotation.</i>";
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
                    if (primaryserviceDetails != "")
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
            string cmdstring = "select Sl_no,Product_id as HSN,Product_name, specification, Quantity,sail_rate,Service_tax_rate,Total_sail_rate2, discount_rate, new_sailrate, ItemRemarks, ItemNo, MaterialNo, PackSize, Department, DeliveryDate from tbl_Quotaion_details where Quotation_no=@Quotation_no order by ItemNo";
            SqlParameter[] pram = {
                                          new SqlParameter("@Quotation_no",qutno)
                                      };
            dtp = DbCL.SPreturn_dt(cmdstring, pram);
            if (dtp.Rows.Count > 0)
            {
                if (Session["cgstOrsgst"].ToString() == "YES")
                {
                    double new_TOTALCGST = 0;
                    string new_TOTALCGST1 = string.Empty;
                    double new_SUBTOTAL = 0;
                    double new_TOTALGST = 0;
                    string new_TOTALGST1 = string.Empty;
                    double new_TOTALGSTPLUSAMO = 0;
                    string new_TOTALGSTPLUSAMO1 = string.Empty;

                    // Start Table
                    strp += "<table class='' style='border:2px solid #6c6c6c; border-collapse: collapse; width:100%;'>";

                    // HEADER ROW
                    strp += "<tr style='background-color:#d9d3d3; text-align:center; background-color: #24285F; font-weight:bold; color: white; font-size: 10px;'>";
                    strp += "<th style='width:3%; border:2px solid #6c6c6c; '>Sl</th>";
                    strp += "<th style='width:27%; border:2px solid #6c6c6c;'>Product Name & Specification</th>";
                    strp += "<th style='width:8%; border:2px solid #6c6c6c;'>HSN Code</th>";
                    strp += "<th style='width:6%; border:2px solid #6c6c6c;'>Qty (PCS)</th>";
                    strp += "<th style='width:5%; border:2px solid #6c6c6c;'>Base Rate</th>";
                    strp += "<th style='width:5%; border:2px solid #6c6c6c;'>Disc (%)</th>";
                    strp += "<th style='width:5%; border:2px solid #6c6c6c;'>Disc Rate</th>";
                    strp += "<th style='width:5%; border:2px solid #6c6c6c;'>GST (%)</th>";
                    strp += "<th style='width:8%; border:2px solid #6c6c6c;'>Amount (₹)</th>";
                    strp += "<th style='width:13%; border:2px solid #6c6c6c;'>Remarks</th>";
                    strp += "<th style='width:5%; border:2px solid #6c6c6c;'>Department & Delivery Date</th>";
                    strp += "</tr>";

                    for (int i = 0; i < dtp.Rows.Count; i++)
                    {
                        string HSN = dtp.Rows[i]["HSN"].ToString();
                        string Productname = dtp.Rows[i]["Product_name"].ToString();
                        string specification = dtp.Rows[i]["specification"].ToString();
                        string itemno = dtp.Rows[i]["ItemNo"].ToString();
                        string materialno = dtp.Rows[i]["MaterialNo"].ToString();
                        string packsize = dtp.Rows[i]["PackSize"].ToString();
                        double baserate = Math.Round((Convert.ToDouble(dtp.Rows[i]["sail_rate"])), 2);
                        double discountrate = Math.Round((Convert.ToDouble(dtp.Rows[i]["new_sailrate"])), 2);
                        int gstper = Convert.ToInt32(dtp.Rows[i]["Service_tax_rate"]);
                        int discper = Convert.ToInt32(dtp.Rows[i]["discount_rate"]);
                        string itemremarks = dtp.Rows[i]["ItemRemarks"].ToString();

                        string dept = dtp.Rows[i]["Department"].ToString();
                        string delvdateRaw = dtp.Rows[i]["DeliveryDate"].ToString();
                        DateTime delvDateParsed;
                        string delvdate = DateTime.TryParse(delvdateRaw, out delvDateParsed)
                            ? delvDateParsed.ToString("dd-MMM-yyyy")
                            : "No Data";


                        int Quantity = Convert.ToInt32(dtp.Rows[i]["Quantity"]);
                        TQ = TQ + Quantity;

                        double new_QuantityBaserateAmo = Math.Round((Quantity * discountrate), 2);
                        string new_QuantityBaserateAmo1 = DoFormat(new_QuantityBaserateAmo);
                        double new_gstamount = Math.Round(((new_QuantityBaserateAmo * gstper) / 100), 2);
                        double cgstper = Math.Round((Convert.ToDouble(gstper) / 2), 2);
                        double new_cgstamo = Math.Round((Convert.ToDouble(new_gstamount) / 2), 2);
                        string new_cgstamo1 = DoFormat(new_cgstamo);

                        new_TOTALCGST = new_TOTALCGST + new_cgstamo;
                        new_TOTALCGST1 = DoFormat(new_TOTALCGST);
                        new_SUBTOTAL = new_SUBTOTAL + new_QuantityBaserateAmo;
                        new_TOTALGST = new_TOTALGST + new_gstamount;
                        new_TOTALGST1 = DoFormat(new_TOTALGST);

                        strp += "<tr>";
                        strp += "<td style='text-align:center; border:2px solid #6c6c6c; font-size: 10.5px;'>" + (i + 1) + "</td>";
                        //strp += "<td style='text-align:left; border:2px solid #6c6c6c; font-size: 10.5px;'>" + Productname + "<br>" + specification + "</td>";
                        if (viewtype == "Detailed")
                        {
                            strp += string.Format("<td style='border: 2px solid #6c6c6c; text-align: left; padding: 5px;'>" +
                                   "<div><span style='font-weight: bold; color: black;'>{0}</span></div>" +
                                   "<div style='font-style: italic; font-size: 10px; color: gray;'>Make: {1}</div>" +
                                   "<div style='font-style: italic; font-size: 10px; color: gray;'>Item No: {2}</div>" +
                                   "<div style='font-style: italic; font-size: 10px; color: gray;'>Material No: {3}</div>" +
                                   "<div style='font-style: italic; font-size: 10px; color: gray;'>Pack Size: {4}</div>" +
                                   "</td>", Productname, specification, itemno, materialno, packsize);
                        }
                        else if (viewtype == "Simple")
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
                        strp += "<td style='text-align:center; border:2px solid #6c6c6c; font-size: 10.5px;'>" + dept + "<br/><br/>" + delvdate + "</td>";
                        strp += "</tr>";
                    }

                    new_TOTALGSTPLUSAMO = new_TOTALGST + new_SUBTOTAL;
                    new_TOTALGSTPLUSAMO1 = DoFormat(new_TOTALGSTPLUSAMO);
                    string grandtotal = (Math.Round((Convert.ToDouble(new_TOTALGSTPLUSAMO)), 2).ToString());
                    string word = MoneyConvDS.MoneyConvFn(grandtotal);

                    string new_SUBTOTAL1 = DoFormat(new_SUBTOTAL);

                    // **Footer Row**
                    strp += "<tfoot>";
                    strp += "<tr style='background-color:#d9d3d3; font-weight: bold;'>";
                    strp += "<td colspan='3' style='border: 2px solid #6c6c6c; text-align: center;'>GRAND TOTAL</td>";
                    strp += $"<td style='border: 2px solid #6c6c6c; text-align: center;'>{TQ}</td>";
                    strp += "<td colspan='4' style='border: 2px solid #6c6c6c;'></td>";
                    //strp += $"<td style='border: 2px solid #6c6c6c; text-align: center;'>{new_TOTALIGST}</td>";
                    strp += $"<td style='border: 2px solid #6c6c6c; text-align: right;'>{new_SUBTOTAL1}</td>";
                    strp += "<td style='border: 2px solid #6c6c6c;'></td>";
                    strp += "<td style='border: 2px solid #6c6c6c;'></td>";
                    strp += "</tr>";

                    // **Total Amount Before Tax Row**
                    strp += "<tr>";
                    strp += "<td colspan='4'></td>";
                    strp += "<td colspan='4' style='background-color: #24285F; color: white; text-align: right;'>TOTAL AMOUNT BEFORE TAX:</td>";
                    strp += $"<td style='border: 2px solid #6c6c6c; text-align: right;'>{new_SUBTOTAL1}</td>";
                    strp += "</tr>";

                    // **Total GST Row**
                    strp += "<tr>";
                    strp += "<td colspan='4' style='background-color: #24285F; color: white; text-align: left;'>Amount (In Words): " + word + "</td>";
                    //strp += "<td></td>";
                    strp += "<td colspan='4' style='background-color: #24285F; color: white; text-align: right;'>TOTAL GST:</td>";
                    strp += $"<td style='border: 2px solid #6c6c6c; text-align: right;'>{new_TOTALGST1}</td>";
                    strp += "</tr>";

                    // **Total Amount After Tax Row**
                    strp += "<tr>";
                    strp += "<td colspan='4'></td>";
                    strp += "<td colspan='4' style='background-color: #24285F; color: white; text-align: right;'>TOTAL AMOUNT AFTER TAX:</td>";
                    strp += $"<td style='border: 2px solid #6c6c6c; text-align: right;'>{new_TOTALGSTPLUSAMO1}</td>";
                    strp += "</tr>";

                    strp += "</tfoot>";
                    strp += "</table>";

                }
                if (Session["igst"].ToString() == "YES")
                {
                    double new_TOTALIGST = 0;
                    double new_SUBTOTAL = 0;
                    double new_TOTALGST = 0;
                    double new_TOTALGSTPLUSAMO = 0;

                    // Start Table
                    strp += "<table class='' style='border-collapse: collapse; width:100%; border: 2px solid #6c6c6c;'>";

                    // **Table Header**
                    strp += "<thead>";
                    strp += "<tr style='background-color:#d9d3d3; font-weight: bold; text-align: center;'>";
                    strp += "<th style='width:3%; border: 2px solid #6c6c6c; background-color: #24285F; color: white;'>Sl</th>";
                    strp += "<th style='width:27%; border: 2px solid #6c6c6c; background-color: #24285F; color: white;'>Product Name & Specification</th>";
                    strp += "<th style='width:7%; border: 2px solid #6c6c6c; background-color: #24285F; color: white;'>HSN</th>";
                    strp += "<th style='width:8%; border: 2px solid #6c6c6c; background-color: #24285F; color: white;'>Qty</th>";
                    strp += "<th style='width:10%; border: 2px solid #6c6c6c; background-color: #24285F; color: white;'>BASE RATE <br> (RS)</th>";
                    strp += "<th style='width:5%; border: 2px solid #6c6c6c; background-color: #24285F; color: white;'>DISC<br> (%)</th>";
                    strp += "<th style='width:10%; border: 2px solid #6c6c6c; background-color: #24285F; color: white;'>NEW RATE (RS)</th>";
                    strp += "<th style='width:5%; border: 2px solid #6c6c6c; background-color: #24285F; color: white;'>GST %</th>";
                    //strp += "<th style='width:9%; border: 2px solid #6c6c6c;'>IGST</th>";
                    strp += "<th style='width:10%; border: 2px solid #6c6c6c; background-color: #24285F; color: white;'>AMOUNT<br> (RS)</th>";
                    strp += "<th style='width:19%; border: 2px solid #6c6c6c; background-color: #24285F; color: white;'>Remarks</th>";
                    strp += "<th style='width:5%; border: 2px solid #6c6c6c; background-color: #24285F; color: white;'>Department & Delivery Date</th>";
                    strp += "</tr>";
                    strp += "</thead>";

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
                        string dept = dtp.Rows[i]["Department"].ToString();
                        string delvdateRaw = dtp.Rows[i]["DeliveryDate"].ToString();
                        DateTime delvDateParsed;
                        string delvdate = DateTime.TryParse(delvdateRaw, out delvDateParsed)
                            ? delvDateParsed.ToString("dd-MMM-yyyy")
                            : "No Data";
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
                        if (viewtype == "Detailed")
                        {
                            strp += string.Format("<td style='border: 2px solid #6c6c6c; text-align: left; padding: 5px;'>" +
                                   "<div><span style='font-weight: bold; color: black;'>{0}</span></div>" +
                                   "<div style='font-style: italic; font-size: 10px; color: gray;'>Make: {1}</div>" +
                                   "<div style='font-style: italic; font-size: 10px; color: gray;'>Item No: {2}</div>" +
                                   "<div style='font-style: italic; font-size: 10px; color: gray;'>Material No: {3}</div>" +
                                   "<div style='font-style: italic; font-size: 10px; color: gray;'>Pack Size: {4}</div>" +
                                   "</td>", Productname, specification, itemno, materialno, packsize);
                        }
                        else if (viewtype == "Simple")
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
                        strp += "<td style='text-align:center; border:2px solid #6c6c6c; font-size: 10.5px;'>" + dept + "<br/><br/>" + delvdate + "</td>";
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
                    strp += "<td style='border: 2px solid #6c6c6c;'></td>";
                    strp += "</tr>";

                    // **Total Amount Before Tax Row**
                    strp += "<tr>";
                    strp += "<td colspan='4'></td>";
                    strp += "<td colspan='4' style='background-color: #24285F; color: white; text-align: right;'>TOTAL AMOUNT BEFORE TAX:</td>";
                    strp += $"<td style='border: 2px solid #6c6c6c; text-align: right;'>{new_SUBTOTAL1}</td>";
                    strp += "</tr>";

                    // **Total GST Row**
                    strp += "<tr>";
                    strp += "<td colspan='4' style='background-color: #24285F; color: white; text-align: left;'>Amount (In Words): " + word + "</td>";
                    //strp += "<td></td>";
                    strp += "<td colspan='4' style='background-color: #24285F; color: white; text-align: right;'>TOTAL GST:</td>";
                    strp += $"<td style='border: 2px solid #6c6c6c; text-align: right;'>{new_TOTALGST}</td>";
                    strp += "</tr>";

                    // **Total Amount After Tax Row**
                    strp += "<tr>";
                    strp += "<td colspan='4'></td>";
                    strp += "<td colspan='4' style='background-color: #24285F; color: white; text-align: right;'>TOTAL AMOUNT AFTER TAX:</td>";
                    strp += $"<td style='border: 2px solid #6c6c6c; text-align: right;'>{final_new_TOTALGSTPLUSAMO}</td>";
                    strp += "</tr>";
                    strp += "</tfoot>";
                    strp += "</table>";
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
                strPayment.Append("<th style='width: 10%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #24285F; text-align: center; color: white;'>S.NO</th>");
                strPayment.Append("<th style='width: 70%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #24285F; text-align: center; color: white;'>PAYMENT PHASE</th>");
                strPayment.Append("<th style='width: 20%; border: 2px solid #6c6c6c; font-weight: bold; background-color: #24285F; text-align: center; color: white;'>AMOUNT (INR)</th>");
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
                if (Address1 == Address2 || Address2 == "(Blank)" || Address2 == "N/A" || Address2 == string.Empty)
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
                name = "“" + name + "”";
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