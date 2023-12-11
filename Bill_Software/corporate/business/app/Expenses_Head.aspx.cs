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
    public partial class WebForm42 : System.Web.UI.Page
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
                Binddata();
            }

        }
        private void Binddata()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select ID,Expencess_Name from tbl_Expences";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            DbCL.executeRdr("insert into tbl_Expences(Expencess_Name) values ('" + txtCityName.Text + "')");
            PanelOK.Visible = true;
            lblOk.Text = "Data Save Successfully...";
            DbCL.Conn.Close();
            txtCityName.Text = "";
            Binddata();

        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string ID = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Delete")
            {
                DbCL.executeRdr("delete from tbl_Expences where ID='" + ID + "'");
                PanelOK.Visible = true;
                lblOk.Text = "Data Deleted Successfully...";
            }
            Binddata();

        }
    }
}