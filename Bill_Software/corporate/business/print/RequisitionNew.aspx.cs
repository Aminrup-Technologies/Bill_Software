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

    public partial class RequisitionNew : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        StringBuilder strp = new StringBuilder();

        DataTable dtp = new DataTable();
        DataTable dtci = new DataTable();
        DataTable cad = new DataTable();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string ReqNo = Request.QueryString["ReqNo"];
                //lblInvoiceNo.Text = Invoice_No.ToString();
                buindalldata(ReqNo);
                bindProductDetails(ReqNo);
            }
        }

        

        private void buindalldata(string reqNo)
        {
            string query = "select clientName,CheckNo,IssueDate,BankName,IFSCode,GstRate,Date,Address,ReqNo,Vendor from tbl_RequisitionMain where ReqNo=@ReqNo";
            SqlParameter[] pram = {
                new SqlParameter("@ReqNo",reqNo)
            };
            dtp = DbCL.SPreturn_dt(query, pram);
            if (dtp.Rows.Count>0)
            {
                string clientName = dtp.Rows[0]["clientName"].ToString();
                bindCompanyaddress(clientName);
                lblcompanyName.Text = clientName;
               
                string CheckNo = dtp.Rows[0]["CheckNo"].ToString();
                string IssueDate = dtp.Rows[0]["IssueDate"].ToString();
                string BankName = dtp.Rows[0]["BankName"].ToString();
                string IFSCode = dtp.Rows[0]["IFSCode"].ToString();
                string GstRate = dtp.Rows[0]["GstRate"].ToString();
                string Date = dtp.Rows[0]["Date"].ToString();
                string Vendor = dtp.Rows[0]["Vendor"].ToString();
                if (Vendor!="")
                {
                    lblVendor.Text = "Vendor: " + Vendor;
                }
                lblBankName.Text = BankName;
                lblCheckNo.Text = CheckNo;
                lbldate.Text = Date;
                lblIFSCode.Text = IFSCode;
                lblIssueDate.Text = IssueDate;
            }
        }

        private void bindCompanyaddress(string clientName)
        {
            string query = "select Address1+' '+City+'-'+pin+', '+State as addre from tbl_Client where Client_Name=@Client_Name";
            SqlParameter[] pram = {
                new SqlParameter("@Client_Name",clientName)
            };
            cad = DbCL.SPreturn_dt(query, pram);
            if (cad.Rows.Count > 0)
            {
                lbladdress1.Text = cad.Rows[0]["addre"].ToString();
            }
        }

        private void bindProductDetails(string reqNo)
        {
            string query = "select Clientname,Description,Size,Qnty,Rate,amount,date,gstrate,ReqNo from tbl_RequisitionNew where ReqNo=@ReqNo";
            SqlParameter[] pram = {
                new SqlParameter("@ReqNo",reqNo)
            };
            dtci = DbCL.SPreturn_dt(query, pram);
            if (dtci.Rows.Count > 0)
            {
                strp.Append("<table cellpadding='0' cellspacing='0' class='style1'>");
                strp.Append("<tr>");
                strp.Append("<td class='tdsty'  style='text-align:center; padding:4px 0px 4px 0px; width:5%; font-size:14px; border:1px solid #bfbfbf; border-right:none; font-weight:bold'>SL NO</td>");
                strp.Append("<td class='tdsty'  style='text-align:center; padding:4px 0px 4px 0px; width:34%; font-size:14px; border:1px solid #bfbfbf; border-right:none; font-weight:bold'>Description</td>");
                strp.Append("<td class='tdsty'  style='text-align:center; padding:4px 0px 4px 0px; width:8%; font-size:14px; border:1px solid #bfbfbf; border-right:none; font-weight:bold'>Size</td>");
                strp.Append("<td class='tdsty'  style='text-align:center; padding:4px 0px 4px 0px; width:10%; font-size:14px; border:1px solid #bfbfbf; border-right:none; font-weight:bold'>Qnty</td>");
                strp.Append("<td class='tdsty'  style='text-align:center; padding:4px 0px 4px 0px; width:5%; font-size:14px; border:1px solid #bfbfbf; border-right:none; font-weight:bold'>Rate</td>");
                strp.Append("<td class='tdsty'  style='text-align:center; padding:4px 0px 4px 0px; width:10%; font-size:14px; border:1px solid #bfbfbf; font-weight:bold'>Amount</td>");
                strp.Append("</tr>");
                strp.Append("</table>");

                strp.Append("<div class='jkhj'>");
               

                
                

                double totalamount = 0;
                double gstrate = 0;
                double gstamount = 0;
                double totaAmoAftTax = 0;

                for (int i=0; i < dtci.Rows.Count; i++)
                {
                    string clientName = dtci.Rows[i]["Clientname"].ToString();
                    string Description = dtci.Rows[i]["Description"].ToString();
                    string Size = dtci.Rows[i]["Size"].ToString();

                    int Qnty = Convert.ToInt32(dtci.Rows[i]["Qnty"]);
                    double Rate = Convert.ToDouble(dtci.Rows[i]["Rate"]);
                    double amount = Convert.ToDouble(dtci.Rows[i]["amount"]);
                    totalamount = totalamount + amount;
                    string date = dtci.Rows[i]["date"].ToString();

                    gstrate = Convert.ToDouble(dtci.Rows[i]["gstrate"]);

                    

                    strp.Append("<table cellpadding='0' cellspacing='0' class='style1'>");
                    strp.Append("<tr>");
                    strp.Append("<td class='tdsty'  style='text-align:center; padding:4px 0px 4px 0px; width:5%; font-size:14px; border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + (i+1).ToString()+"</td>");
                    strp.Append("<td class='tdsty'  style='text-align:center; padding:4px 0px 4px 0px; width:34%; font-size:14px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + Description.ToString() + "</td>");
                    strp.Append("<td class='tdsty' style='text-align:center; padding:4px 0px 4px 0px; width:8%; font-size:14px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + Size.ToString()+"</td>");
                    strp.Append("<td class='tdsty'  style='text-align:right; padding:4px 12px 4px 0px; width:10%; font-size:14px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + Qnty.ToString() + "</td>");
                    strp.Append("<td class='tdsty' style='text-align:center; padding:4px 0px 4px 0px; width:5%; font-size:14px;border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + Rate+"</td>");
                    strp.Append("<td class='tdsty' style='text-align:right; padding:4px 12px 4px 0px;; width:10%; font-size:14px;border:1px solid #bfbfbf;border-top:none;'>" + amount.ToString()+"</td>");
                    strp.Append("</tr>");
                    strp.Append("</table>");

                }

                int count = dtci.Rows.Count;
                
                for (int j = count; j < 9; j++)
                {
                    strp.Append("<table cellpadding='0' cellspacing='0' class='style1'>");
                    strp.Append("<tr>");
                    strp.Append("<td class='tdsty'  style='text-align:center; padding:4px 0px 4px 0px; width:5%; font-size:14px; border:1px solid #bfbfbf; border-right:none; border-top:none;'>" + (j + 1).ToString() + "</td>");
                    strp.Append("<td class='tdsty'  style='text-align:center; padding:4px 0px 4px 0px; width:34%; font-size:14px;border:1px solid #bfbfbf; border-right:none; border-top:none; color:#fff'>&nbsp;</td>");
                    strp.Append("<td class='tdsty' style='text-align:center; padding:4px 0px 4px 0px; width:8%; font-size:14px;border:1px solid #bfbfbf; border-right:none; border-top:none;' color:#fff>&nbsp;</td>");
                    strp.Append("<td class='tdsty'  style='text-align:right; padding:4px 12px 4px 0px; width:10%; font-size:14px;border:1px solid #bfbfbf; border-right:none; border-top:none; color:#fff'>&nbsp;</td>");
                    strp.Append("<td class='tdsty' style='text-align:center; padding:4px 0px 4px 0px; width:5%; font-size:14px;border:1px solid #bfbfbf; border-right:none; border-top:none; color:#fff'>&nbsp;</td>");
                    strp.Append("<td class='tdsty' style='text-align:right; padding:4px 12px 4px 0px;; width:10%; font-size:14px;border:1px solid #bfbfbf;border-top:none;color:#fff'>&nbsp;</td>");
                    strp.Append("</tr>");
                    strp.Append("</table>");
                }
               
                string totalamobeforetax = (Math.Round((Convert.ToDouble(totalamount))).ToString());

                if (gstrate!=0)
                {
                    gstamount = Math.Round(((totalamount * gstrate) / 100));
                }
                totaAmoAftTax = totalamount + gstamount;
                string word = MoneyConvDS.MoneyConvFn(totaAmoAftTax.ToString());

                strp.Append("<table cellpadding='0' cellspacing='0' class='style1'>");
                strp.Append("<tr>");
                strp.Append("<td class='tdsty'  style='text-align:left; padding:3px 15px 3px 15px; width:60%; font-size:14px;border:0px;font-weight:bold;' colspan='6'>Paid By - Cheque / Cash</td>");
                strp.Append("<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:19.5%;  font-size:13px;border:0px;font-weight:bold' colspan='4'>Gross: </td>");
                strp.Append("<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:20.5%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + totalamobeforetax.ToString() + "</td>");
                strp.Append("</tr>");
                strp.Append("</table>");

                

                strp.Append("<table cellpadding='0' cellspacing='0' class='style1'>");
                strp.Append("<tr>");
                strp.Append("<td class='tdsty'  style='text-align:left; padding:3px 15px 3px 15px; width:60%; font-size:13px;border:0px;font-weight:bold;' colspan='6'>Amount (In Words): " + word.ToString() + "</td>");
                strp.Append("<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:19.5%;  font-size:13px;border:0px;font-weight:bold' colspan='4'>GST: " + gstrate + " %</td>");
                strp.Append("<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:20.5%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>" + gstamount + "</td>");
                strp.Append("</tr>");
                strp.Append("</table>");

                strp.Append("</div>");

                strp.Append("<table cellpadding='0' cellspacing='0' class='style1'>");
                strp.Append("<tr>");
                strp.Append("<td class='tdsty'  style='text-align:left; padding:3px 15px 3px 15px; width:60%; font-size:13px;border:0px;font-weight:bold;' colspan='6'>&nbsp;</td>");
                strp.Append("<td class='tdsty'  style='text-align:right; padding:5px 15px 5px 0px; width:19.5%;  font-size:13px;border:0px;font-weight:bold' colspan='4'>Total: </td>");
                strp.Append("<td class='tdsty' style='text-align:right; padding:5px 12px 5px 0px; width:20.5%; font-size:13px;border:1px solid #bfbfbf;border-top:none;font-weight:bold'>"+ totaAmoAftTax.ToString() + "</td>");
                strp.Append("</tr>");
                strp.Append("</table>");

                lblProductList.Text = strp.ToString();
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