using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm48 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                DbCL.FillCombo(cmbvendor, "select Vendor_Name from tbl_Vendor order by Vendor_Name");
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");

            }

        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                BuindCompanyId();
                cmdstring = "select tbl_Vendor.Vendor_Name,tbl_Purchess_payment.Payment_ID,tbl_Purchess_payment.Payment_Date,tbl_Purchess_payment.Purchess_ID,tbl_Purchess_payment.Purchess_Date,tbl_Purchess_payment.Net_amount,tbl_Purchess_payment.Given_amount,tbl_Purchess_payment.type,tbl_Purchess_payment.Ch_no,tbl_Purchess_payment.Ch_date from tbl_Purchess_payment inner join tbl_Vendor on tbl_Purchess_payment.Client_Id=tbl_Vendor.Vendor_Id where tbl_Purchess_payment.Client_Id='" + lblclientId.Text + "' order by cast(tbl_Purchess_payment.Payment_Date as datetime) desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                cmdstring = "select tbl_Vendor.Vendor_Name,tbl_Purchess_payment.Payment_ID,tbl_Purchess_payment.Payment_Date,tbl_Purchess_payment.Purchess_ID,tbl_Purchess_payment.Purchess_Date,tbl_Purchess_payment.Net_amount,tbl_Purchess_payment.Given_amount,tbl_Purchess_payment.type,tbl_Purchess_payment.Ch_no,tbl_Purchess_payment.Ch_date from tbl_Purchess_payment inner join tbl_Vendor on tbl_Purchess_payment.Client_Id=tbl_Vendor.Vendor_Id where cast(tbl_Purchess_payment.Payment_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(tbl_Purchess_payment.Payment_Date as datetime) desc";
                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                cmdstring = "select tbl_Vendor.Vendor_Name,tbl_Purchess_payment.Payment_ID,tbl_Purchess_payment.Payment_Date,tbl_Purchess_payment.Purchess_ID,tbl_Purchess_payment.Purchess_Date,tbl_Purchess_payment.Net_amount,tbl_Purchess_payment.Given_amount,tbl_Purchess_payment.type,tbl_Purchess_payment.Ch_no,tbl_Purchess_payment.Ch_date from tbl_Purchess_payment inner join tbl_Vendor on tbl_Purchess_payment.Client_Id=tbl_Vendor.Vendor_Id where tbl_Purchess_payment.Client_Id='" + lblclientId.Text + "' and cast(tbl_Purchess_payment.Payment_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(tbl_Purchess_payment.Payment_Date as datetime) desc";
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
            string cmdstring = "select Vendor_Id from tbl_Vendor where Vendor_Name='" + cmbvendor.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblclientId.Text = re["Vendor_Id"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/Seartch_purchess_payments.aspx");
        }
    }
}