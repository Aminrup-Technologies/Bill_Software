using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm16 : System.Web.UI.Page
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
            string cmdstring = "select Client_Id,Client_Name,Industry from tbl_Client order by Client_Id asc";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        private void BindGrid1()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Id,Client_Name,Industry from tbl_Client where Client_Name='" + cmbvendor.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        private void Bindcombo()
        {
            cmbvendor.Items.Add("ALL");
            DbCL.FillCombo10(cmbvendor, "select Client_Name from tbl_Client order by Client_Id asc");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Client_Id = Convert.ToString(e.CommandArgument);


            if (e.CommandName == "Edit")
            {
                Response.Redirect("Update_client.aspx?Client_Id=" + Client_Id);

            }
            else if (e.CommandName == "Representative")
            {
                Response.Redirect("Show_representative.aspx?Client_Id=" + Client_Id);

            }
            else if (e.CommandName == "Factioy")
            {
                Response.Redirect("ShowFactory.aspx?Client_Id=" + Client_Id);
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

        }
    }
}