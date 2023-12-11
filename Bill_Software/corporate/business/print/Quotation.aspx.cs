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
    public partial class Quotation : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        public string taxorvat = "";
        public string proOrser = "";

        public string psid = "";

        public string str = "";
        public string strp = "";

        DataTable dtgcs = new DataTable();
        DataTable dtp = new DataTable();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string ID = Request.QueryString["ID"];

                //lblQno.Text = Quotation_no.ToString();
                buindalldata(ID);
                Bindtaxdata();

                Buindamount();
                BindVatamount();
                
            }

        }

        private void Bindtaxdata()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string idstring = "";
            string idstring1 = "";
            string cmdstring = "select Product_id from tbl_Quotaion_details where Quotation_no='" + lblQno.Text + "' order by id";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                idstring = re["Product_id"].ToString();
                idstring1 = idstring.Substring(0, 1);
                proOrser = idstring1;
                if (idstring1 == "P")
                {
                    psid = "SL NO.";
                    taxorvat = "VAT";
                }
                else
                {
                    psid = "SL NO.";
                    taxorvat = "TAX";
                }
            }
            DbCL.Conn.Close();
        }

        private void BindVatamount()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select ('" + taxorvat + " @ '+Vat_rate+' %') as rete,Vat_amount from tbl_quotation_vat where Quotation_no='" + lblQno.Text + "' order by Id";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList2.DataSource = cmd.ExecuteReader();
            DataList2.DataBind();
            DbCL.Conn.Close();

            //string query = "select Quotation_no,Vat_rate,Vat_amount from  tbl_quotation_vat where Quotation_no=@Quotation_no";
            //SqlParameter[] pram ={
            //    new SqlParameter("@Quotation_no",lblQno.Text)
            //};
            //dtgcs = DbCL.SPreturn_dt(query, pram);
            //if (dtgcs.Rows.Count>0)
            //{
            //    for (int i=0; i< dtgcs.Rows.Count; i++)
            //    {

            //        string gstrate = dtgcs.Rows[i]["Vat_rate"].ToString();
            //        double cgst =Math.Round((Convert.ToDouble(gstrate) / 2),2);

            //        string gstamo = dtgcs.Rows[i]["Vat_amount"].ToString();
            //        double cgstamo = Math.Round((Convert.ToDouble(gstamo) / 2), 2);

            //        str += "<table width='100%'  border='0' cellpadding='0' cellspacing='0' class='table1'>";
            //        str += "<tr>";
            //        str += "<td style='width:66.7%;border:none; text-align:right; font:arial; padding:5px 20px 5px 0;font-weight: bold;'>GST: " + gstrate + " %</td>";
            //        str += "<td style='width:33.3%;border-top:none; text-align:right; font:arial; padding:0px 20px 0px 2px;font-weight: bold;'>" + gstamo + "</td>";
            //        str += "</tr>";
            //        str += "</table>";

            //        str += "";

            //        if (Session["cgstOrsgst"].ToString() == "YES")
            //        {
            //            str += "<table width='100%'  border='0' cellpadding='0' cellspacing='0' class='table1'>";
            //            str += "<tr>";
            //            str += "<td style='width:66.7%;border:none; text-align:right; font:arial; padding:5px 20px 5px 0;font-weight: bold;'>CGST: " + cgst.ToString() + " %</td>";
            //            str += "<td style='width:33.3%;border-top:none; text-align:right; font:arial; padding:0px 20px 0px 2px;font-weight: bold;'>" + cgstamo.ToString() + "</td>";
            //            str += "</tr>";
            //            str += "</table>";

            //            str += "<table width='100%'  border='0' cellpadding='0' cellspacing='0' class='table1'>";
            //            str += "<tr>";
            //            str += "<td style='width:66.7%;border:none; text-align:right; font:arial; padding:5px 20px 5px 0;font-weight: bold;'>SGST: " + cgst.ToString() + " %</td>";
            //            str += "<td style='width:33.3%;border-top:none; text-align:right; font:arial; padding:0px 20px 0px 2px;font-weight: bold;'>" + cgstamo.ToString() + "</td>";
            //            str += "</tr>";
            //            str += "</table>";
            //        }
            //        if (Session["igst"].ToString() == "YES")
            //        {
            //            str += "<table width='100%'  border='0' cellpadding='0' cellspacing='0' class='table1'>";
            //            str += "<tr>";
            //            str += "<td style='width:66.7%;border:none; text-align:right; font:arial; padding:5px 20px 5px 0;font-weight: bold;'>IGST: " + gstrate.ToString() + " %</td>";
            //            str += "<td style='width:33.3%;border-top:none; text-align:right; font:arial; padding:0px 20px 0px 2px;font-weight: bold;'>" + gstamo.ToString() + "</td>";
            //            str += "</tr>";
            //            str += "</table>";
            //        }
            //    }

            //    lblgstdetails.Text = str.ToString();

            //}
        }

       

        private void Buindamount()
        {
            string qno = lblQno.Text;
            string status = statusvalue(qno);

            if (status!="YES")
            {
                DataList1.Visible = true;
                lblProductList.Visible = false;

                AMODETAILS.Visible = true;

                DbCL.Sqlconnection();
                DbCL.ConnectDb();
                string cmdstring = "select Sl_no,(Product_name+' '+specification) as Product_name,Quantity,sail_rate,Service_tax_rate,Total_sail_rate2 from tbl_Quotaion_details where Quotation_no='" + lblQno.Text + "' order by Id";

                SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
                DataList1.DataSource = cmd.ExecuteReader();
                DataList1.DataBind();
                DbCL.Conn.Close();
            }
            else {

                DataList1.Visible = false;
                lblProductList.Visible = true;
                AMODETAILS.Visible = false;

                string cmdstring = "select Sl_no,Product_id as HSN,(Product_name+' '+specification) as Product_name,Quantity,sail_rate,Service_tax_rate,Total_sail_rate2 from tbl_Quotaion_details where Quotation_no=@Quotation_no order by Id";
                SqlParameter[] pram = {
                                          new SqlParameter("@Quotation_no",lblQno.Text)
                                      };
                dtp = DbCL.SPreturn_dt(cmdstring, pram);
                if (dtp.Rows.Count > 0)
                {
                    if (Session["cgstOrsgst"].ToString() == "YES")
                    {
                        double TOTALCGST = 0;
                        double SUBTOTAL = 0;
                        double TOTALGST = 0;
                        double TOTALGSTPLUSAMO = 0;

                        strp += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                        strp += "<tr>";
                        strp += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px; border:1px solid #bfbfbf; border-right:none; font-weight:bold'>SL NO</td>";
                        strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:7%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>HSN</td>";
                        strp += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:30%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>Particulars</td>";
                        strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>QNTY</td>";
                        strp += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>RATE</td>";
                        strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>GST</td>";
                        strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:14%; font-size:13px;border:1px solid #bfbfbf; border-right:none;'>";
                        strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        strp += "<tr>";
                        strp += "<td class='tdsty' style='text-align:center;padding:2px 0px 2px 0px; width:14%; font-size:13px;border-bottom:1px solid #bfbfbf;font-weight:bold' colspan='2'>CGST</td>";
                        strp += "</tr>";
                        strp += "<tr>";
                        strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border-right:1px solid #bfbfbf;font-weight:bold'>RATE</td>";
                        strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:9%; font-size:13px;font-weight:bold'>AMOUNT</td>";
                        strp += "</tr>";
                        strp += "</table>";
                        strp += "</td>";

                        strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:14%; font-size:13px;border:1px solid #bfbfbf; border-right:none;'>";
                        strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        strp += "<tr>";
                        strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:14%; font-size:13px;border-bottom:1px solid #bfbfbf;font-weight:bold' colspan='2'>SGST</td>";
                        strp += "</tr>";
                        strp += "<tr>";
                        strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border-right:1px solid #bfbfbf;font-weight:bold'>RATE</td>";
                        strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:9%; font-size:13px;font-weight:bold'>AMOUNT</td>";
                        strp += "</tr>";
                        strp += "</table>";
                        strp += "</td>";
                        strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;font-weight:bold'>AMOUNT</td>";
                        strp += "</tr>";
                        strp += "</table>";


                        for (int i = 0; i < dtp.Rows.Count; i++)
                        {
                            string HSN = dtp.Rows[i]["HSN"].ToString();
                            string Productname = dtp.Rows[i]["Product_name"].ToString();
                            int Quantity = Convert.ToInt32(dtp.Rows[i]["Quantity"]);
                            double baserate = Math.Round((Convert.ToDouble(dtp.Rows[i]["sail_rate"])), 2);
                            int gstper = Convert.ToInt32(dtp.Rows[i]["Service_tax_rate"]);
                            double QuantityBaserateAmo = Math.Round((Quantity * baserate), 2);

                            double gstamount = Math.Round(((QuantityBaserateAmo * gstper) / 100), 2);

                            double cgstper = Math.Round((Convert.ToDouble(gstper) / 2), 2);

                            double cgstamo = Math.Round((Convert.ToDouble(gstamount) / 2), 2);

                            TOTALCGST = TOTALCGST + cgstamo;
                            SUBTOTAL = SUBTOTAL + QuantityBaserateAmo;

                            TOTALGST = TOTALGST + gstamount;



                            strp += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                            strp += "<tr>";
                            strp += "<td class='tdsty'  style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px; border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + (i + 1).ToString() + "</td>";
                            strp += "<td class='tdsty' style='text-align:center; padding:1px 0px 1px 0px; width:7%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + HSN + "</td>";
                            strp += "<td class='tdsty'  style='text-align:left; padding:1px 0px 1px 6px; width:30%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + Productname + "</td>";
                            strp += "<td class='tdsty' style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + Quantity.ToString() + "</td>";
                            strp += "<td class='tdsty'  style='text-align:right; padding:1px 12px 1px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + baserate.ToString() + "</td>";
                            strp += "<td class='tdsty' style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";

                            strp += "<td class='tdsty'  style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + cgstper.ToString() + " %</td>";
                            strp += "<td class='tdsty' style='text-align:right; padding:1px 12px 1px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + cgstamo.ToString() + "</td>";
                            strp += "<td class='tdsty'  style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + cgstper.ToString() + " %</td>";
                            strp += "<td class='tdsty' style='text-align:right; padding:1px 12px 1px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + cgstamo.ToString() + "</td>";

                            strp += "<td class='tdsty' style='text-align:right; padding:1px 12px 1px 15px;; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;'>" + QuantityBaserateAmo.ToString() + "</td>";
                            strp += "</tr>";
                            strp += "</table>";
                        }

                        TOTALGSTPLUSAMO = TOTALGST + SUBTOTAL;

                        string grandtotal = (Math.Round((Convert.ToDouble(TOTALGSTPLUSAMO))).ToString());

                        string word = MoneyConvDS.MoneyConvFn(grandtotal);

                        strp += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                        strp += "<tr>";
                        strp += "<td class='tdsty'  style='text-align:center;padding:2px 0px 2px 0px; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;' colspan='7'></td>";
                        strp += "<td class='tdsty' style='text-align:right; padding:2px 12px 2px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none; font-weight:bold'>" + TOTALCGST.ToString() + "</td>";
                        strp += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'></td>";
                        strp += "<td class='tdsty' style='text-align:right; padding:2px 12px 2px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none; font-weight:bold'>" + TOTALCGST.ToString() + "</td>";

                        strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + SUBTOTAL.ToString() + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        strp += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                        strp += "<tr>";
                        strp += "<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:90%; font-size:13px;border:0px;font-weight:bold' colspan='10'>TOTAL AMOUNT BEFORE TAX:</td>";
                        strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + SUBTOTAL.ToString() + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        strp += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                        strp += "<tr>";
                        strp += "<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:90%; font-size:13px;border:0px;font-weight:bold' colspan='10'>TOTAL GST:</td>";
                        strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + TOTALGST.ToString() + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        strp += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                        strp += "<tr>";
                        strp += "<td class='tdsty'  style='text-align:left; padding:3px 15px 3px 15px; width:62%; font-size:13px;border:0px;font-weight:bold;' bgcolor='#dbe5f1'colspan='6'>Amount (In Words):" + word + "</td>";
                        strp += "<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:28%;  font-size:13px;border:0px;font-weight:bold' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
                        strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + grandtotal.ToString() + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                    }
                    if (Session["igst"].ToString() == "YES")
                    {

                        double TOTALIGST = 0;
                        double SUBTOTAL = 0;
                        double TOTALGST = 0;
                        double TOTALGSTPLUSAMO = 0;

                        strp += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                        strp += "<tr>";
                        strp += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px; border:1px solid #bfbfbf; border-right:none; font-weight:bold'>SL NO</td>";
                        strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:7%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>HSN</td>";
                        strp += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:44%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>Particulars</td>";
                        strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>QNTY</td>";
                        strp += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>RATE</td>";
                        strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>GST</td>";
                        strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:14%; font-size:13px;border:1px solid #bfbfbf; border-right:none;'>";
                        strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        strp += "<tr>";
                        strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:14%; font-size:13px;border-bottom:1px solid #bfbfbf;font-weight:bold' colspan='2'>IGST</td>";
                        strp += "</tr>";
                        strp += "<tr>";
                        strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border-right:1px solid #bfbfbf;font-weight:bold'>RATE</td>";
                        strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:9%; font-size:13px;font-weight:bold'>AMOUNT</td>";
                        strp += "</tr>";
                        strp += "</table>";
                        strp += "</td>";


                        strp += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;font-weight:bold'>AMOUNT</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        for (int i = 0; i < dtp.Rows.Count; i++)
                        {
                            string HSN = dtp.Rows[i]["HSN"].ToString();
                            string Productname = dtp.Rows[i]["Product_name"].ToString();
                            int Quantity = Convert.ToInt32(dtp.Rows[i]["Quantity"]);
                            double baserate = Math.Round((Convert.ToDouble(dtp.Rows[i]["sail_rate"])), 2);
                            int gstper = Convert.ToInt32(dtp.Rows[i]["Service_tax_rate"]);
                            double QuantityBaserateAmo = Math.Round((Quantity * baserate), 2);

                            double gstamount = Math.Round(((QuantityBaserateAmo * gstper) / 100), 2);

                            double cgstper = Math.Round((Convert.ToDouble(gstper) / 2), 2);

                            double cgstamo = Math.Round((Convert.ToDouble(gstamount) / 2), 2);

                            TOTALIGST = TOTALIGST + gstamount;
                            SUBTOTAL = SUBTOTAL + QuantityBaserateAmo;

                            TOTALGST = TOTALGST + gstamount;

                            strp += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                            strp += "<tr>";
                            strp += "<td class='tdsty'  style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px; border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + (i + 1).ToString() + "</td>";
                            strp += "<td class='tdsty' style='text-align:center; padding:1px 0px 1px 0px; width:7%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + HSN + "</td>";
                            strp += "<td class='tdsty'  style='text-align:center; padding:1px 0px 1px 6px; width:44%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + Productname + "</td>";
                            strp += "<td class='tdsty' style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + Quantity.ToString() + "</td>";
                            strp += "<td class='tdsty'  style='text-align:right; padding:1px 12px 1px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + baserate.ToString() + "</td>";
                            strp += "<td class='tdsty' style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";

                            strp += "<td class='tdsty'  style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";
                            strp += "<td class='tdsty' style='text-align:right; padding:1px 12px 1px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + gstamount.ToString() + "</td>";


                            strp += "<td class='tdsty' style='text-align:right; padding:1px 12px 1px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;'>" + QuantityBaserateAmo.ToString() + "</td>";
                            strp += "</tr>";
                            strp += "</table>";
                        }

                        TOTALGSTPLUSAMO = TOTALGST + SUBTOTAL;

                        string grandtotal = (Math.Round((Convert.ToDouble(TOTALGSTPLUSAMO))).ToString());

                        string word = MoneyConvDS.MoneyConvFn(grandtotal);

                        strp += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                        strp += "<tr>";
                       
                        strp += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px;  font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;font-weight:bold' colspan='7'></td>";
                        strp += "<td class='tdsty' style='text-align:right; padding:2px 12px 2px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;font-weight:bold'>" + TOTALIGST.ToString() + "</td>";


                        strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 15px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + SUBTOTAL + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        strp += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                        strp += "<tr>";
                        strp += "<td class='tdsty'  style='text-align:right; padding:3px 15px 3px 0px;  font-size:13px;border:0px;font-weight:bold' colspan='8'>TOTAL AMOUNT BEFORE TAX:</td>";
                        strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + SUBTOTAL.ToString() + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        strp += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                        strp += "<tr>";
                        strp += "<td class='tdsty'  style='text-align:right; padding:3px 15px 3px 0px;  font-size:13px;border:0px;font-weight:bold' colspan='8'>TOTAL GST:</td>";
                        strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + TOTALGST.ToString() + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        strp += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                        strp += "<tr>";
                        strp += "<td class='tdsty'  style='text-align:justify; padding:5px 15px 5px 15px; width:61%;  font-size:13px;border:0px;font-weight:bold' bgcolor='#dbe5f1' colspan='4'>Amount (In Words):" + word + " </td>";
                        strp += "<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 15px; font-size:13px;border:0px;font-weight:bold' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
                        strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + grandtotal.ToString() + "</td>";
                        strp += "</tr>";
                        strp += "</table>";
                    }
                    lblProductList.Text = strp.ToString();
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
            //string CmdString = "select Quotation_no from tbl_Quotation where Quotation_no='" + qno + "' and (CONVERT(DateTime, Quotation_date, 103) > CONVERT(DateTime, '" + Lst + "', 103)) ";
            SqlCommand cmd = new SqlCommand(CmdString, DbCL.Conn);
            SqlDataReader re1 = cmd.ExecuteReader();
            if (re1.Read())
            {
                status_value = "YES";
            }
            else
            {
                status_value = "NO";
            }
            DbCL.Conn.Close();
            return status_value;
        }

        private void buindalldata(string ID)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Quotation_no,Quotation_date,Client_Id,sub_total,Service_tax,Net_amount,cgstOrsgst,igst from tbl_Quotation where ID='" + ID.ToString() + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblQno.Text = re["Quotation_no"].ToString();
                lblQdate.Text = re["Quotation_date"].ToString();
                Session["Quotation_date"] = lblQdate.Text;
                string clientid = re["Client_Id"].ToString();
                Bindclientdetails(clientid);
                lblSubtotal.Text = re["sub_total"].ToString();
                lblstax.Text = re["Service_tax"].ToString();
                lblstax0.Text = re["Service_tax"].ToString();
                lblnetamount.Text = re["Net_amount"].ToString();
                Session["cgstOrsgst"] = re["cgstOrsgst"].ToString();
                Session["igst"] = re["igst"].ToString();

            }
           
            string word = MoneyConvDS.MoneyConvFn(lblnetamount.Text);
            lblword.Text = word.ToString();
            DbCL.Conn.Close();

        }

        

        private void Bindclientdetails(string clientid)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Name,Address1,Address2,City,pin,State,Rep_Name,Rep_Desig from tbl_Client where Client_Id='" + clientid + "'";
            SqlCommand cmd = new SqlCommand(cmdstring,DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if(re.Read())
            {
                lblcompanyName.Text = re["Client_Name"].ToString();
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

        protected void Button2_Click(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {

        }
    }
}