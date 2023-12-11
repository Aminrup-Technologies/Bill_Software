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
    public partial class WebForm74 : System.Web.UI.Page
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
                cmdstring = "select clientName,CheckNo,IssueDate,BankName,IFSCode,GstRate,Date,Address,ReqNo from tbl_RequisitionMain where clientName='" + cmbvendor.Text + "' order by cast(Date as datetime) desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                cmdstring = "select clientName,CheckNo,IssueDate,BankName,IFSCode,GstRate,Date,Address,ReqNo from tbl_RequisitionMain where cast(Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(Date as datetime) desc";
                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                cmdstring = "select clientName,CheckNo,IssueDate,BankName,IFSCode,GstRate,Date,Address,ReqNo from tbl_RequisitionMain where clientName='" + cmbvendor.Text + "' and cast(Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(Date as datetime) desc";
                Buinddatagrid(cmdstring);
            }
            btnSertch.Visible = false;
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

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/RequisitionManualDelete.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string ReqNo = Convert.ToString(e.CommandArgument);
            if (e.CommandName == "Delete")
            {
                DbCL.executeRdr("delete from tbl_RequisitionMain where ReqNo='" + ReqNo + "'");
                DbCL.executeRdr("delete from tbl_RequisitionNew where ReqNo='" + ReqNo + "'");
                PanelOK.Visible = true;
                lblOk.Text = "Data Deleted Successfully...";
                DataList1.Visible = false;
            }
        }
    }
}