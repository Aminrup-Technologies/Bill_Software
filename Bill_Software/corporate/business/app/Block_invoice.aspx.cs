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
    public partial class WebForm64 : System.Web.UI.Page
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
                DbCL.FillCombo(cmbvendor, "select Client_Name from tbl_Client order by Client_Name");
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
                cmdstring = "select  tbl_Invoice.ID,tbl_Invoice.Invoice_No,tbl_Invoice.Invoice_Date,tbl_Invoice.Quotation_No,tbl_Invoice.Quotation_Date,tbl_Invoice.Net_Amount,tbl_Client.Client_Name from tbl_Invoice inner join tbl_Client on tbl_Invoice.Client_ID=tbl_Client.Client_Id where tbl_Invoice.Client_ID='" + lblclientId.Text + "' and tbl_Invoice.status2='Active' order by cast(tbl_Invoice.Invoice_Date as datetime) desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                cmdstring = "select  tbl_Invoice.ID,tbl_Invoice.Invoice_No,tbl_Invoice.Invoice_Date,tbl_Invoice.Quotation_No,tbl_Invoice.Quotation_Date,tbl_Invoice.Net_Amount,tbl_Client.Client_Name from tbl_Invoice inner join tbl_Client on tbl_Invoice.Client_ID=tbl_Client.Client_Id where cast(tbl_Invoice.Invoice_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' and tbl_Invoice.status2='Active' order by cast(tbl_Invoice.Invoice_Date as datetime) desc";
                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                cmdstring = "select  tbl_Invoice.ID,tbl_Invoice.Invoice_No,tbl_Invoice.Invoice_Date,tbl_Invoice.Quotation_No,tbl_Invoice.Quotation_Date,tbl_Invoice.Net_Amount,tbl_Client.Client_Name from tbl_Invoice inner join tbl_Client on tbl_Invoice.Client_ID=tbl_Client.Client_Id where tbl_Invoice.Client_ID='" + lblclientId.Text + "' and cast(tbl_Invoice.Invoice_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' and tbl_Invoice.status2='Active' order by cast(tbl_Invoice.Invoice_Date as datetime) desc";
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
            Response.Redirect("~/corporate/business/app/Block_invoice.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Invoice_No = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Delete")
            {
                DbCL.executeRdr("update tbl_Invoice set status2='Block' where Invoice_No='" + Invoice_No + "'");
                PanelOK.Visible = true;
                lblOk.Text = "Invoice is Block...";
                DataList1.Visible = false;
            }
        }
    }
}