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
    public partial class Invoice : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        public string strp = "";

        DataTable dtp = new DataTable();
        DataTable dtci = new DataTable();

        public string vatno = "";
        public string gstno = "";

        public string taxorvat = "";
        public string proOrser = "";
        public string psid = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string ID = Request.QueryString["ID"];
                //lblInvoiceNo.Text = Invoice_No.ToString();
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
            string cmdstring = "select Product_id from tbl_Quotaion_details where Quotation_no='" + lblQno.Text + "' order by Id";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                idstring = re["Product_id"].ToString();
                idstring1 = idstring.Substring(0,1);


                if (idstring1 == "P")
                {

                    psid = "SL NO.";
                    taxorvat = "Vat";
                }
                else
                {

                    psid = "SL NO.";
                    taxorvat = "Tax";
                }
            }
            DbCL.Conn.Close();
            
        }
        private void BindVatamount()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select ('Vat @ '+ Vat_rate +' %') as rete,Vat_amount from tbl_quotation_vat where Quotation_no='" + lblQno.Text + "' order by Id";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList2.DataSource = cmd.ExecuteReader();
            DataList2.DataBind();
            DbCL.Conn.Close();
        }
        private void Buindamount()
        {
            string qno = lblQno.Text;
            string status = statusvalue(qno);

            if (status != "YES")
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
            else
            {

                DataList1.Visible = false;
                lblProductList.Visible = true;
                AMODETAILS.Visible = false;

                //string cmdstring = "select Sl_no,Product_id as HSN,(Product_name+' '+specification) as Product_name,Quantity,sail_rate,Service_tax_rate,Total_sail_rate2 from tbl_Quotaion_details where Quotation_no=@Quotation_no order by Id";
                string cmdstring = "select Product_id as HSN,Product_name,Quantity,sail_rate,Service_tax_rate,Total_sail_rate1,Total_sail_rate2 from  tbl_Invoice_details where Quotation_no=@Quotation_no and  Invoice_No=@Invoice_No order by Id";
                SqlParameter[] pram = {
                                          new SqlParameter("@Quotation_no",lblQno.Text),
                                          new SqlParameter("@Invoice_No",lblInvoiceNo.Text)
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

                        strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
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



                            strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                            strp += "<tr>";
                            strp += "<td class='tdsty'  style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px; border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + (i + 1).ToString() + "</td>";
                            strp += "<td class='tdsty' style='text-align:center; padding:1px 0px 1px 0px; width:7%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + HSN + "</td>";
                            strp += "<td class='tdsty'  style='text-align:center; padding:1px 0px 1px 0px; width:30%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + Productname + "</td>";
                            strp += "<td class='tdsty' style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + Quantity.ToString() + "</td>";
                            strp += "<td class='tdsty'  style='text-align:right; padding:1px 12px 1px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + baserate.ToString() + "</td>";
                            strp += "<td class='tdsty' style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";

                            strp += "<td class='tdsty'  style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + cgstper.ToString() + " %</td>";
                            strp += "<td class='tdsty' style='text-align:right; padding:1px 12px 1px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + cgstamo.ToString() + "</td>";
                            strp += "<td class='tdsty'  style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + cgstper.ToString() + " %</td>";
                            strp += "<td class='tdsty' style='text-align:right; padding:1px 12px 1px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + cgstamo.ToString() + "</td>";

                            strp += "<td class='tdsty' style='text-align:right; padding:1px 12px 1px 0px;; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;'>" + QuantityBaserateAmo.ToString() + "</td>";
                            strp += "</tr>";
                            strp += "</table>";
                        }

                        TOTALGSTPLUSAMO = TOTALGST + SUBTOTAL;

                        string grandtotal = (Math.Round((Convert.ToDouble(TOTALGSTPLUSAMO))).ToString());

                        string word = MoneyConvDS.MoneyConvFn(lblnetamount.Text);

                        strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        strp += "<tr>";
                        strp += "<td class='tdsty'  style='text-align:center;padding:2px 0px 2px 0px; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;' colspan='7'></td>";
                        strp += "<td class='tdsty' style='text-align:right; padding:2px 12px 2px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none; font-weight:bold'>" + TOTALCGST.ToString() + "</td>";
                        strp += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'></td>";
                        strp += "<td class='tdsty' style='text-align:right; padding:2px 12px 2px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none; font-weight:bold'>" + TOTALCGST.ToString() + "</td>";

                        strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + SUBTOTAL.ToString() + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        strp += "<tr>";
                        strp += "<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:90%; font-size:13px;border:0px;font-weight:bold' colspan='10'>TOTAL AMOUNT BEFORE TAX:</td>";
                        strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + SUBTOTAL.ToString() + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        strp += "<tr>";
                        strp += "<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:90%; font-size:13px;border:0px;font-weight:bold' colspan='10'>TOTAL GST:</td>";
                        strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + TOTALGST.ToString() + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        if (lbldiscount.Text != "")
                        {
                            string discount_amount = lbldiscount.Text.ToString();
                            decimal discount_amount1 = Convert.ToDecimal(discount_amount);
                            if (discount_amount1 != 0)
                            {
                                strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                                strp += "<tr>";
                                strp += "<td class='tdsty'  style='text-align:right; padding:3px 15px 3px 0px;  font-size:13px;border:0px;font-weight:bold' colspan='8'>DISCOUNT AMOUNT:</td>";
                                strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 15px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + lbldiscount.Text.ToString() + "</td>";
                                strp += "</tr>";
                                strp += "</table>";
                            }
                        }

                        strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        strp += "<tr>";
                        strp += "<td class='tdsty'  style='text-align:left; padding:3px 15px 3px 15px; width:62%; font-size:13px;border:0px;font-weight:bold;' bgcolor='#dbe5f1'colspan='6'>Amount (In Words):" + word + "</td>";
                        strp += "<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:28%;  font-size:13px;border:0px;font-weight:bold' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
                        strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + lblnetamount.Text.ToString() + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                    }
                    if (Session["igsti"].ToString() == "YES")
                    {

                        double TOTALIGST = 0;
                        double SUBTOTAL = 0;
                        double TOTALGST = 0;
                        double TOTALGSTPLUSAMO = 0;

                        strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
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

                            strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                            strp += "<tr>";
                            strp += "<td class='tdsty'  style='text-align:center; padding:1px 0px 1px 0px; width:5%; font-size:13px; border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + (i + 1).ToString() + "</td>";
                            strp += "<td class='tdsty' style='text-align:center; padding:1px 0px 1px 0px; width:7%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + HSN + "</td>";
                            strp += "<td class='tdsty'  style='text-align:center; padding:1px 0px 1px 0px; width:44%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + Productname + "</td>";
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

                        string word = MoneyConvDS.MoneyConvFn(lblnetamount.Text);

                        strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        strp += "<tr>";

                        strp += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px;  font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;font-weight:bold' colspan='7'></td>";
                        strp += "<td class='tdsty' style='text-align:right; padding:2px 12px 2px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;font-weight:bold'>" + TOTALIGST.ToString() + "</td>";


                        strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + SUBTOTAL + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        strp += "<tr>";
                        strp += "<td class='tdsty'  style='text-align:right; padding:3px 15px 3px 0px;  font-size:13px;border:0px;font-weight:bold' colspan='8'>TOTAL AMOUNT BEFORE TAX:</td>";
                        strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + SUBTOTAL.ToString() + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        strp += "<tr>";
                        strp += "<td class='tdsty'  style='text-align:right; padding:3px 15px 3px 0px;  font-size:13px;border:0px;font-weight:bold' colspan='8'>TOTAL GST:</td>";
                        strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + TOTALGST.ToString() + "</td>";
                        strp += "</tr>";
                        strp += "</table>";

                        if (lbldiscount.Text != "")
                        {
                            string discount_amount = lbldiscount.Text.ToString();
                            decimal discount_amount1 = Convert.ToDecimal(discount_amount);
                            if (discount_amount1 != 0)
                            {
                                strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                                strp += "<tr>";
                                strp += "<td class='tdsty'  style='text-align:right; padding:3px 15px 3px 0px;  font-size:13px;border:0px;font-weight:bold' colspan='8'>DISCOUNT AMOUNT:</td>";
                                strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + lbldiscount.Text.ToString() + "</td>";
                                strp += "</tr>";
                                strp += "</table>";
                            }
                        }

                        strp += "<table cellpadding='0' cellspacing='0' class='style1'>";
                        strp += "<tr>";
                        strp += "<td class='tdsty'  style='text-align:justify; padding:5px 15px 5px 15px; width:61%;  font-size:13px;border:0px;font-weight:bold' bgcolor='#dbe5f1' colspan='4'>Amount (In Words):" + word + " </td>";
                        strp += "<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 15px; font-size:13px;border:0px;font-weight:bold' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
                        strp += "<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + lblnetamount.Text.ToString() + "</td>";
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
            SqlCommand cmd = new SqlCommand(CmdString, DbCL.Conn);
            SqlDataReader re1 = cmd.ExecuteReader();
            if (re1.Read())
            {
                status_value = "YES";
                lbltaxstring.Text = "GST Registration No: ";
                lbltaxno.Text = "19AAEFI5315E1ZL";
                lblClientVat.Text = "Buyer's GST No: " + gstno;
            }
            else
            {
                status_value = "NO";
                lbltaxstring.Text = "Vat No: ";
                lbltaxno.Text = "19629770012";
                lblClientVat.Text = "Buyer's Vat No: " + vatno;
            }
            DbCL.Conn.Close();
            return status_value;
        }

        private void buindalldata(string ID)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Invoice where ID='" + ID.ToString() + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblInvoiceNo.Text = re["Invoice_No"].ToString();
                lbldate.Text = re["Invoice_Date"].ToString();
                lblQno.Text = re["Quotation_No"].ToString();
                string qno= re["Quotation_No"].ToString();
                bindcgstorigst(qno);
                string clientid = re["Client_ID"].ToString();
                string addressfor = re["addressfor"].ToString();
                Bindclientdetails(clientid, addressfor);
                lblSubtotal.Text = re["sub_total"].ToString();
                lbldiscount.Text = re["discount"].ToString();
                string discount_amount = re["discount"].ToString();
                lblstax.Text = re["Service_Tax"].ToString();
                lblstax0.Text = re["Service_Tax"].ToString();
                lblnetamount.Text = re["Net_Amount"].ToString();
                decimal discount_amount1 = Convert.ToDecimal(discount_amount);
                if (discount_amount1 == 0)
                {
                    discount_row.Visible = false;
                }
                else
                {
                    discount_row.Visible = true;
                }

            }
            string word = MoneyConvDS.MoneyConvFn(lblnetamount.Text);
            lblword.Text = word.ToString();
            DbCL.Conn.Close();

        }

        private void bindcgstorigst(string qno)
        {
            string query = "select cgstOrsgst,igst from tbl_Quotation where Quotation_no=@Quotation_no";
            SqlParameter[] pram = {
                new SqlParameter("@Quotation_no",qno)
            };
            dtci = DbCL.SPreturn_dt(query, pram);
            if (dtci.Rows.Count > 0)
            {
                Session["cgstOrsgsti"] = dtci.Rows[0]["cgstOrsgst"].ToString();
                Session["igsti"] = dtci.Rows[0]["igst"].ToString();
            }
        }

        private void Bindclientdetails(string clientid, string addressfor)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Name,Address1,Address2,City,pin,State,Rep_Name,Rep_Desig,Vat_no,Service_tax_no from tbl_Client where Client_Id='" + clientid + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblcompanyName.Text = re["Client_Name"].ToString();
                if (addressfor == "Corporate office")
                {
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

                }
                else
                {
                    Bindaddress(clientid, addressfor);
                }
     
                //lblrepresentativeName.Text = re["Rep_Name"].ToString();
                //lblrepresentativedesignation.Text = re["Rep_Desig"].ToString();
                gstno= re["Service_tax_no"].ToString();
                vatno =re["Vat_no"].ToString();


                if(vatno=="" && gstno == "")
                {
                    lblClientVat.Visible=false;
                }
                else
                {
                    lblClientVat.Visible = true;
                }
            }
            DbCL.Conn.Close();
        }

        private void Bindaddress(string clientid, string addressfor)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Address1,Address2,city,State,pin from tbl_Factory where Client_id='" + clientid.ToString() + "' and Factory_name='" + addressfor.ToString() + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
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
                lblcity.Text = re["city"].ToString();
                lblPin.Text = re["pin"].ToString();
                lblstate.Text = re["State"].ToString();
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