using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app
{
    public partial class hydrent_invoice : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();

        DataTable dtamo = new DataTable();

        public int gdfdh = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                DbCL.FillCombo(cmbvendor, "select Client_Name from tbl_Client order by Client_Name");
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtinvoiceDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            }
        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                BuindCompanyId();
                //cmdstring = "select tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Client_Id='" + lblclientId.Text + "' and tbl_Quotation.Status2='No' order by cast(tbl_Quotation.Quotation_date as datetime) desc";
                cmdstring = "select b.id,a.Client_Name,b.Quotation_no,b.Q_date from tbl_Client as a inner join tbl_qsHydrentQuotation as b on a.Client_Id=b.ClientId where b.ClientId='"+ lblclientId.Text + "' and b.invStatus='No' order by cast(b.Q_date as datetime) desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                //cmdstring = "select tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Status2='No' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(tbl_Quotation.Quotation_date as datetime) desc";
                cmdstring = "select b.id,a.Client_Name,b.Quotation_no,b.Q_date from tbl_Client as a inner join tbl_qsHydrentQuotation as b on a.Client_Id=b.ClientId where b.invStatus='No' and cast(b.Q_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(b.Q_date as datetime) desc";

                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                //cmdstring = "select tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Status2='No' and tbl_Quotation.Client_Id='" + lblclientId.Text + "' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(tbl_Quotation.Quotation_date as datetime) desc";

                cmdstring = "select b.id,a.Client_Name,b.Quotation_no,b.Q_date from tbl_Client as a inner join tbl_qsHydrentQuotation as b on a.Client_Id=b.ClientId where b.invStatus='No' and b.ClientId='" + lblclientId.Text + "' and cast(b.Q_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(b.Q_date as datetime) desc";

                Buinddatagrid(cmdstring);
            }
            btnSertch.Visible = false;
        }

        private void Buinddatagrid(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                Buinddatagrid1(cmdstring);
            }
            else
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "No Data Found...";

            }
            DbCL.Conn.Close();
        }

        private void Buinddatagrid1(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            SqlCommand cmd1 = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd1.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        private void BuindCompanyId()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Id from tbl_Client where Client_Name='" + cmbvendor.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblclientId.Text = re["Client_Id"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/hydrent_invoice.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Quotation_no = Convert.ToString(e.CommandArgument);
            if (e.CommandName == "Select")
            {
                Panel1.Visible = true;
                Binddetails(Quotation_no);

            }
        }

        private void Binddetails(string quotation_no)
        {

            buindalldata(quotation_no);

            bindSubTotalandGrandtotal(quotation_no);

            BindclientName();
            cmbaddressfor.Items.Add("Corporate office");
            DbCL.FillCombo10(cmbaddressfor, "select Factory_name from tbl_Factory where Client_id='" + lblClient_Id.Text + "' order by Factory_name");
        }

        private void bindSubTotalandGrandtotal(string quotation_no)
        {
            string query = " SELECT SUM(Service_tax_Amount_total) as 'Service_tax_Amount_total', SUM(total_amount) as 'total_amount',(SUM(Service_tax_Amount_total) + SUM(total_amount)) as 'Total' FROM tbl_qsHydrentDetails where Quotation_no =@Quotation_no";
            SqlParameter[] pram = { new SqlParameter("@Quotation_no", quotation_no) };
            dtamo = DbCL.SPreturn_dt(query, pram);
            if (dtamo.Rows.Count > 0)
            {

                lblsubtotal.Text = dtamo.Rows[0]["total_amount"].ToString();

                double grandtotal = Math.Round(Convert.ToDouble(dtamo.Rows[0]["Total"]), 2);

                double decimalpoints = Math.Abs(grandtotal - Math.Floor(grandtotal));

                if (decimalpoints > 0.5)
                {
                    gdfdh = (int)Math.Round(grandtotal);
                }
                else
                {
                    gdfdh = (int)Math.Floor(grandtotal);
                }

                lblNet_amount.Text = gdfdh.ToString();
            }
        }

        private void buindalldata(object quotation_no)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Quotation_no,Q_date,ClientId from tbl_qsHydrentQuotation where Quotation_no='" + quotation_no + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblClient_Id.Text = re["ClientId"].ToString();
                lblQuotation_no.Text = re["Quotation_no"].ToString();
                lblQuotation_date.Text = re["Q_date"].ToString();
            }
            DbCL.Conn.Close();
        }

       

        private void BindclientName()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Name from tbl_Client where Client_Id='" + lblClient_Id.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblClientName.Text = re["Client_Name"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            string invoice_no = BindInvoiceNo();
            int j = idreturn();
            j = j + 1;
            double Net_amountafterdiscount = Convert.ToDouble(lblNet_amount.Text) - Convert.ToDouble(txtDiscount.Text);

            string query = "insert into tbl_HydrentInvoice(Invoice_No,Invoice_Date,Quotation_No,Quotation_Date,Client_ID,Net_amount,Sl_no,DiscountAmount,addressfor) values (@Invoice_No,@Invoice_Date,@Quotation_No,@Quotation_Date,@Client_ID,@Net_amount,@Sl_no,@DiscountAmount,@addressfor)";
            SqlParameter[] pram = {
                new SqlParameter("@Invoice_No",invoice_no.ToString()),
                new SqlParameter("@Invoice_Date",txtinvoiceDate.Text),
                new SqlParameter("@Quotation_No",lblQuotation_no.Text),
                new SqlParameter("@Quotation_Date",lblQuotation_date.Text),
                new SqlParameter("@Client_ID",lblClient_Id.Text),
                new SqlParameter("@Net_amount",Net_amountafterdiscount),
                new SqlParameter("@Sl_no",j.ToString()),
                new SqlParameter("@DiscountAmount",Convert.ToDouble(txtDiscount.Text)),
                new SqlParameter("@addressfor",cmbaddressfor.Text)
            };

            DbCL.SPExecDB(query, pram);


           // DbCL.executeRdr("insert into tbl_HydrentInvoice(Invoice_No,Invoice_Date,Quotation_No,Quotation_Date,Client_ID,Net_amount,Sl_no,Net_amountafterdiscount,addressfor)values('" + invoice_no.ToString() + "','" + txtinvoiceDate.Text + "','" + lblQuotation_no.Text + "','" + lblQuotation_date.Text + "','" + lblClient_Id.Text + "','" + Net_amountafterdiscount + "','" + j.ToString() + "','" + txtDiscount.Text + "','" + cmbaddressfor.Text + "')");
            DbCL.executeRdr("update tbl_qsHydrentQuotation set invStatus='Yes' where Quotation_no='" + lblQuotation_no.Text + "'");
            
            PanelOK.Visible = true;
            lblOk.Text = "Data Save Successfull...";
            
            Button1.Visible = false;
        }

        private string BindInvoiceNo()
        {
            string p = null;
            string c = lblClientName.Text.Trim();
            string f = c.Substring(0, 1);
            string tt;
            for (int i = 0; i < c.Length; i++)
            {
                p = c.Substring(i, 1);
                if (p == " ")
                {
                    tt = c.Substring((i + 1), 1);
                    if (tt == "(")
                    {
                        tt = c.Substring((i + 2), 1);
                    }
                    f = f + tt;
                }
            }
            f = "INV/" + f + "/";
            string ss = findmonth();
            f = f + ss;
            int j = idreturn();
            j = j + 1;
            f = f + j.ToString();
            return f;
        }

        private int idreturn()
        {
            string a = null;
            int b = 0;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string date1 = txtinvoiceDate.Text;
            string date2 = date1.Substring(3, 3);
            string date3 = date1.Substring(7, 4);
            string date4, date5, date6;
            if (date2 == "Jan" || date2 == "Feb" || date2 == "Mar")
            {
                date4 = ((Convert.ToInt32(date3) - 1)).ToString();
                date5 = "31-Mar-" + date4;
                date6 = "31-Mar-" + date3;
            }
            else
            {
                date4 = ((Convert.ToInt32(date3) + 1)).ToString();
                date5 = "31-Mar-" + date3;
                date6 = "31-Mar-" + date4;
            }
            string cmdstring = "select Sl_no from tbl_HydrentInvoice where ID=(select max(ID) from tbl_HydrentInvoice where cast(Invoice_Date as datetime) between '" + date5.ToString() + "' and '" + date6.ToString() + "')";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                a = re["Sl_no"].ToString();
                b = Convert.ToInt32(a);
            }
            else
            {
                b = 0;
            }
            DbCL.Conn.Close();
            return b;
        }

        private string findmonth()
        {
            string MonthName = "-";
            string a = txtinvoiceDate.Text.Substring(3, 3);
            string b = txtinvoiceDate.Text.Substring(9, 2);
            if (a == "Jan" || a == "Feb" || a == "Mar")
            {
                MonthName = (Convert.ToInt32(b) - 1).ToString() + "-" + b + "/";
            }
            else
            {
                MonthName = b + "-" + (Convert.ToInt32(b) + 1).ToString() + "/";
            }
            return MonthName;
        }
    }
}