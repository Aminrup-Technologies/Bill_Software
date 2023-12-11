using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm18 : System.Web.UI.Page
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
                Bindcombo();
                BindGrid();
            }
        }
        private void BindGrid()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Id,Client_Name from tbl_Client order by Client_Name";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        private void BindGrid1()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Id,Client_Name from tbl_Client where Client_Name='" + cmbvendor.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        private void Bindcombo()
        {
            cmbvendor.Items.Add("ALL");
            DbCL.FillCombo10(cmbvendor, "select Client_Name from tbl_Client order by Client_Name");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Client_Id = Convert.ToString(e.CommandArgument);


            if (e.CommandName == "Delete")
            {
                DbCL.executeRdr("delete from tbl_Client where Client_Id='" + Client_Id.ToString() + "'");
                DbCL.executeRdr("delete from tbl_ClientRegAddress where Client_Id='" + Client_Id.ToString() + "'");

                PanelOK.Visible = true;
                lblOk.Text = "Data Deleted Successfully...";
                DataList1.Visible = false;
            }

        }

        protected void cmbvendor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbvendor.SelectedIndex == 0)
            {
                BindGrid();


            }
            else
            {
                BindGrid1();
            }
            PanelOK.Visible = false;
        }
    }
}