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
    public partial class WebForm80 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dtuse = new DataTable();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                Binddata();
            }
        }

        private void Binddata()
        {
            DbCL.FillCombo(ddlEmpId, "select User_Id from tbl_login where User_Id not in('superadmin', 'uat')");
            string cmdstring = "select id,User_Id,Password,Name,Phone_no,Email from tbl_login where User_Id not in ('superadmin', 'uat')";
            BindDataGrig(cmdstring);
            ddlEmpId.SelectedIndex = 0;
        }

        private void BindDataGrig(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string User_Id = Convert.ToString(e.CommandArgument);
            if (e.CommandName == "Delete")
            {
                DbCL.executeRdr("delete from tbl_login where User_Id='" + User_Id + "'");
                DbCL.executeRdr("delete from tbl_Designation where User_Id='" + User_Id + "'");
                PanelOK.Visible = true;
                lblOk.Text = "Data Deleted Successfully...";
            }
            else if (e.CommandName == "Menu Edit")
            {
                Response.Redirect("~/corporate/business/app/Update_Designation.aspx?User_Id=" + User_Id);
            }
            Binddata();
        }

        protected void ddlEmpId_TextChanged(object sender, EventArgs e)
        {
            if (ddlEmpId.Text!="--Select--")
            {
                string query = "select id,User_Id,Password,Name,Phone_no,Email from tbl_login where User_Id=@User_Id";
                SqlParameter[] pram = {
                    new SqlParameter("@User_Id",ddlEmpId.Text)
                };
                dtuse = DbCL.SPreturn_dt(query, pram);
                if (dtuse.Rows.Count>0)
                {
                    DataList1.DataSource = dtuse;
                    DataList1.DataBind();
                }
            }
        }
    }
}