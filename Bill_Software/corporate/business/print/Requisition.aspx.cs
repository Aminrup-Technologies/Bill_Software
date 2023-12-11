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
    public partial class Requisition : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        public string taxorvat = "";
        public string proOrser = "";

        public string str = "";

        DataTable dtd = new DataTable();
        DataTable dtc = new DataTable();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string reqno = Request.QueryString["requeno"];
                buindalldata(reqno);
            }
        }

        private void buindalldata(string reqno)
        {
            string query = "select requeno,ProductCode,ProductName,Baserate,quantity,gstper,productAmo,gstamo,cgstamo,sgstmo,productAmoGstmo from tbl_requisition where requeno=@requeno";
            SqlParameter[] pram = {
                new SqlParameter("@requeno",reqno),
            };
            dtd = DbCL.SPreturn_dt(query, pram);
            if (dtd.Rows.Count > 0)
            {
                bindComDetails(reqno);
                

                if (ViewState["cgstorigst"].ToString() == "cgst")
                {
                    double gstper = 0;
                    double productAmo = 0;
                    double gstamo = 0;
                    double cgstamo = 0;
                    double sgstmo = 0;
                    double productAmoGstmo = 0;


                    double productAmoTotal = 0;
                    double gstamoTotal = 0;
                    double cgstamoTotal = 0;
                    double sgstmoTotal = 0;
                    double productAmoGstmoTotal = 0;


                    str += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                    str += "<tr>";
                    str += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px; border:1px solid #bfbfbf; border-right:none; font-weight:bold'>SL NO</td>";
                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:7%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>HSN</td>";
                    str += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:30%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>Particulars</td>";
                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>QNTY</td>";
                    str += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>RATE</td>";
                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>GST</td>";
                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:14%; font-size:13px;border:1px solid #bfbfbf; border-right:none;'>";
                    str += "<table cellpadding='0' cellspacing='0' class='style1'>";
                    str += "<tr>";
                    str += "<td class='tdsty' style='text-align:center;padding:2px 0px 2px 0px; width:14%; font-size:13px;border-bottom:1px solid #bfbfbf;font-weight:bold' colspan='2'>CGST</td>";
                    str += "</tr>";
                    str += "<tr>";
                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border-right:1px solid #bfbfbf;font-weight:bold'>RATE</td>";
                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:9%; font-size:13px;font-weight:bold'>AMOUNT</td>";
                    str += "</tr>";
                    str += "</table>";
                    str += "</td>";

                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:14%; font-size:13px;border:1px solid #bfbfbf; border-right:none;'>";
                    str += "<table cellpadding='0' cellspacing='0' class='style1'>";
                    str += "<tr>";
                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:14%; font-size:13px;border-bottom:1px solid #bfbfbf;font-weight:bold' colspan='2'>SGST</td>";
                    str += "</tr>";
                    str += "<tr>";
                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border-right:1px solid #bfbfbf;font-weight:bold'>RATE</td>";
                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:9%; font-size:13px;font-weight:bold'>AMOUNT</td>";
                    str += "</tr>";
                    str += "</table>";
                    str += "</td>";
                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;font-weight:bold'>AMOUNT</td>";
                    str += "</tr>";
                    str += "</table>";



                    for (int i = 0; i < dtd.Rows.Count; i++)
                    {
                        string ProductCode = dtd.Rows[i]["ProductCode"].ToString();
                        string ProductName = dtd.Rows[i]["ProductName"].ToString();
                        string Baserate = dtd.Rows[i]["Baserate"].ToString();
                        string quantity = dtd.Rows[i]["quantity"].ToString();

                        gstper = Convert.ToDouble(dtd.Rows[i]["gstper"]);
                        double cgstper = Convert.ToDouble(gstper / 2);
                        double sgstper = cgstper;

                        productAmo = Convert.ToDouble(dtd.Rows[i]["productAmo"]);
                        gstamo = Convert.ToDouble(dtd.Rows[i]["gstamo"]);
                        cgstamo = Convert.ToDouble(dtd.Rows[i]["cgstamo"]);
                        sgstmo = Convert.ToDouble(dtd.Rows[i]["sgstmo"]);
                        productAmoGstmo = Convert.ToDouble(dtd.Rows[i]["productAmoGstmo"]);

                        productAmoTotal = productAmoTotal + productAmo;
                        gstamoTotal = gstamoTotal + gstamo;
                        cgstamoTotal = cgstamoTotal + cgstamo;
                        sgstmoTotal = sgstmoTotal + sgstmo;

                        productAmoGstmoTotal = productAmoGstmoTotal + productAmoGstmo;



                        str += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                        str += "<tr>";
                        str += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px; border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + (i + 1).ToString() + "</td>";
                        str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:7%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + ProductCode + "</td>";
                        str += "<td class='tdsty'  style='text-align:left; padding:2px 0px 2px 6px; width:30%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + ProductName + "</td>";
                        str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + quantity.ToString() + "</td>";
                        str += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + Baserate.ToString() + "</td>";
                        str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";

                        str += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + cgstper.ToString() + " %</td>";
                        str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + cgstamo.ToString() + "</td>";
                        str += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + cgstper.ToString() + " %</td>";
                        str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + cgstamo.ToString() + "</td>";

                        str += "<td class='tdsty' style='text-align:left; padding:5px 0px 5px 15px;; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;'>" + productAmo.ToString() + "</td>";
                        str += "</tr>";
                        str += "</table>";
                    }

                    //lblProductlist.Text = str.ToString();
                    //lblbeftax.Text = productAmoTotal.ToString();
                    //lblcgstTotal.Text = cgstamoTotal.ToString();
                    //lblGstTotal.Text = gstamoTotal.ToString();
                    //lbligstTotal.Text = gstamoTotal.ToString();

                    //lblsgstTotal.Text = sgstmoTotal.ToString();
                    //lblTotalAmoGst.Text = productAmoGstmoTotal.ToString();

                    string word = MoneyConvDS.MoneyConvFn(productAmoGstmoTotal.ToString());
                    //lblWord.Text = word;

                    str += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                    str += "<tr>";
                    str += "<td class='tdsty'  style='text-align:center;padding:2px 0px 2px 0px; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;' colspan='7'></td>";
                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none; font-weight:bold'>" + cgstamoTotal.ToString() + "</td>";
                    str += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'></td>";
                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none; font-weight:bold'>" + cgstamoTotal.ToString() + "</td>";

                    str += "<td class='tdsty' style='text-align:left; padding:5px 0px 5px 15px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + productAmoTotal.ToString() + "</td>";
                    str += "</tr>";
                    str += "</table>";

                    str += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                    str += "<tr>";
                    str += "<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:90%; font-size:13px;border:0px;font-weight:bold' colspan='10'>TOTAL AMOUNT BEFORE TAX:</td>";
                    str += "<td class='tdsty' style='text-align:left; padding:5px 0px 5px 15px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + productAmoTotal.ToString() + "</td>";
                    str += "</tr>";
                    str += "</table>";

                    str += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                    str += "<tr>";
                    str += "<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:90%; font-size:13px;border:0px;font-weight:bold' colspan='10'>TOTAL GST:</td>";
                    str += "<td class='tdsty' style='text-align:left; padding:5px 0px 5px 15px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + gstamoTotal.ToString() + "</td>";
                    str += "</tr>";
                    str += "</table>";

                    str += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                    str += "<tr>";
                    str += "<td class='tdsty'  style='text-align:left; padding:3px 15px 3px 15px; width:62%; font-size:13px;border:0px;font-weight:bold;' bgcolor='#dbe5f1'colspan='6'>Amount (In Words):" + word + "</td>";
                    str += "<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:28%;  font-size:13px;border:0px;font-weight:bold' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
                    str += "<td class='tdsty' style='text-align:left; padding:5px 0px 5px 15px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + productAmoGstmoTotal.ToString() + "</td>";
                    str += "</tr>";
                    str += "</table>";
                }
                else
                {

                    double gstper = 0;
                    double productAmo = 0;
                    double gstamo = 0;
                    double cgstamo = 0;
                    double sgstmo = 0;
                    double productAmoGstmo = 0;


                    double productAmoTotal = 0;
                    double gstamoTotal = 0;
                    double cgstamoTotal = 0;
                    double sgstmoTotal = 0;
                    double productAmoGstmoTotal = 0;


                    str += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                    str += "<tr>";
                    str += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px; border:1px solid #bfbfbf; border-right:none; font-weight:bold'>SL NO</td>";
                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:7%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>HSN</td>";
                    str += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:44%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>Particulars</td>";
                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>QNTY</td>";
                    str += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>RATE</td>";
                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; font-weight:bold'>GST</td>";
                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:14%; font-size:13px;border:1px solid #bfbfbf; border-right:none;'>";
                    str += "<table cellpadding='0' cellspacing='0' class='style1'>";
                    str += "<tr>";
                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:14%; font-size:13px;border-bottom:1px solid #bfbfbf;font-weight:bold' colspan='2'>IGST</td>";
                    str += "</tr>";
                    str += "<tr>";
                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border-right:1px solid #bfbfbf;font-weight:bold'>RATE</td>";
                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:9%; font-size:13px;font-weight:bold'>AMOUNT</td>";
                    str += "</tr>";
                    str += "</table>";
                    str += "</td>";


                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf;font-weight:bold'>AMOUNT</td>";
                    str += "</tr>";
                    str += "</table>";


                    for (int i = 0; i < dtd.Rows.Count; i++)
                    {
                        string ProductCode = dtd.Rows[i]["ProductCode"].ToString();
                        string ProductName = dtd.Rows[i]["ProductName"].ToString();
                        string Baserate = dtd.Rows[i]["Baserate"].ToString();
                        string quantity = dtd.Rows[i]["quantity"].ToString();

                        gstper = Convert.ToDouble(dtd.Rows[i]["gstper"]);
                        double cgstper = Convert.ToDouble(gstper / 2);
                        double sgstper = cgstper;

                        productAmo = Convert.ToDouble(dtd.Rows[i]["productAmo"]);
                        gstamo = Convert.ToDouble(dtd.Rows[i]["gstamo"]);
                        cgstamo = Convert.ToDouble(dtd.Rows[i]["cgstamo"]);
                        sgstmo = Convert.ToDouble(dtd.Rows[i]["sgstmo"]);
                        productAmoGstmo = Convert.ToDouble(dtd.Rows[i]["productAmoGstmo"]);

                        productAmoTotal = productAmoTotal + productAmo;
                        gstamoTotal = gstamoTotal + gstamo;
                        cgstamoTotal = cgstamoTotal + cgstamo;
                        sgstmoTotal = sgstmoTotal + sgstmo;

                        productAmoGstmoTotal = productAmoGstmoTotal + productAmoGstmo;



                        str += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                        str += "<tr>";
                        str += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px; border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + (i + 1).ToString() + "</td>";
                        str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:7%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + ProductCode + "</td>";
                        str += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 6px; width:44%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + ProductName + "</td>";
                        str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + quantity.ToString() + "</td>";
                        str += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:10%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + Baserate.ToString() + "</td>";
                        str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";

                        str += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px; width:5%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + gstper.ToString() + " %</td>";
                        str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + gstamo.ToString() + "</td>";


                        str += "<td class='tdsty' style='text-align:left; padding:5px 0px 5px 15px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;'>" + productAmo.ToString() + "</td>";
                        str += "</tr>";
                        str += "</table>";
                    }

                    //lblProductlist.Text = str.ToString();
                    //lblbeftax.Text = productAmoTotal.ToString();
                    //lblcgstTotal.Text = cgstamoTotal.ToString();
                    //lblGstTotal.Text = gstamoTotal.ToString();
                    //lbligstTotal.Text = gstamoTotal.ToString();

                    //lblsgstTotal.Text = sgstmoTotal.ToString();
                    //lblTotalAmoGst.Text = productAmoGstmoTotal.ToString();

                    string word = MoneyConvDS.MoneyConvFn(productAmoGstmoTotal.ToString());
                    //lblWord.Text = word;


                    str += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                    str += "<tr>";

                    str += "<td class='tdsty'  style='text-align:center; padding:2px 0px 2px 0px;  font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;font-weight:bold' colspan='7'></td>";
                    str += "<td class='tdsty' style='text-align:center; padding:2px 0px 2px 0px; width:9%; font-size:13px;border:1px solid #bfbfbf; border-right:none; border-top:none;font-weight:bold'>" + gstamoTotal.ToString() + "</td>";


                    str += "<td class='tdsty' style='text-align:left; padding:5px 0px 5px 15px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + productAmoTotal + "</td>";
                    str += "</tr>";
                    str += "</table>";

                    str += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                    str += "<tr>";
                    str += "<td class='tdsty'  style='text-align:right; padding:3px 15px 3px 0px;  font-size:13px;border:0px;font-weight:bold' colspan='8'>TOTAL AMOUNT BEFORE TAX:</td>";
                    str += "<td class='tdsty' style='text-align:left; padding:5px 0px 5px 15px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + productAmoTotal.ToString() + "</td>";
                    str += "</tr>";
                    str += "</table>";

                    str += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                    str += "<tr>";
                    str += "<td class='tdsty'  style='text-align:right; padding:3px 15px 3px 0px;  font-size:13px;border:0px;font-weight:bold' colspan='8'>TOTAL GST:</td>";
                    str += "<td class='tdsty' style='text-align:left; padding:5px 0px 5px 15px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + gstamoTotal.ToString() + "</td>";
                    str += "</tr>";
                    str += "</table>";

                    str += "<table cellpadding='0' cellspacing='0' class='style1' style='font-family:'Century Gothic''>";
                    str += "<tr>";
                    str += "<td class='tdsty'  style='text-align:justify; padding:5px 15px 5px 15px; width:61%;  font-size:13px;border:0px;font-weight:bold' bgcolor='#dbe5f1' colspan='4'>Amount (In Words):" + word + " </td>";
                    str += "<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 15px; font-size:13px;border:0px;font-weight:bold' colspan='4'>TOTAL AMOUNT AFTER TAX:</td>";
                    str += "<td class='tdsty' style='text-align:left; padding:5px 15px 5px 15px; width:10%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + productAmoGstmoTotal.ToString() + "</td>";
                    str += "</tr>";
                    str += "</table>";
                }

                lblProductlist.Text = str.ToString();
            //lblbeftax.Text = productAmoTotal.ToString();
            //lblcgstTotal.Text = cgstamoTotal.ToString();
            //lblGstTotal.Text = gstamoTotal.ToString();
            //lbligstTotal.Text = gstamoTotal.ToString();

            //lblsgstTotal.Text = sgstmoTotal.ToString();
            //lblTotalAmoGst.Text = productAmoGstmoTotal.ToString();

            //string word = MoneyConvDS.MoneyConvFn(lblTotalAmoGst.Text);
            //lblWord.Text = word;

        }
            
        }

    private void bindComDetails(string reqno)
    {
        string query = "select reqDate,CompName,address,paytype,chkno,bankname,ifscCode,date,cgstorsgst from tbl_requisitionBankDetails where requeno=@requeno";
        SqlParameter[] pram = {
                new SqlParameter("@requeno",reqno)
            };
        dtc = DbCL.SPreturn_dt(query, pram);
        if (dtc.Rows.Count > 0)
        {
            string reqDate = dtc.Rows[0]["reqDate"].ToString();
            string CompName = dtc.Rows[0]["CompName"].ToString();
            string address = dtc.Rows[0]["address"].ToString();
            string paytype = dtc.Rows[0]["paytype"].ToString();
            string chkno = dtc.Rows[0]["chkno"].ToString();
            string bankname = dtc.Rows[0]["bankname"].ToString();

            string ifscCode = dtc.Rows[0]["ifscCode"].ToString();
            string date = dtc.Rows[0]["date"].ToString();
            string cgstorigst = dtc.Rows[0]["cgstorsgst"].ToString();
            ViewState["cgstorigst"] = cgstorigst;

            rDate.Text = reqDate;
            lblCompanyname.Text = CompName;
            lblAddress.Text = address;
            lblIfcCode.Text = ifscCode;
            lblIssueDate.Text = date;
            lblBankName.Text = bankname;
            lblCheckNo.Text = chkno;
        }
    }

    //protected void Button1_Click(object sender, EventArgs e)
    //{

    //}

    //protected void Button2_Click(object sender, EventArgs e)
    //{

    //}
}
}