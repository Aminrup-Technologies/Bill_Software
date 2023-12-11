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
    public partial class WebForm62 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/corporate/CustomError/CustomError.aspx");
            }
            if (!IsPostBack)
            {

                string Client_Id = Request.QueryString["Client_Id"];
                lblComId.Text = Client_Id.ToString();
                Bindcompany();
                Buinddata();
            }

        }
        private void Buinddata()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Factory where Client_id='" + lblComId.Text + "' order by ID";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        private void Bindcompany()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Name from tbl_Client where Client_Id='" + lblComId.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblCompanyGroupName.Text = re["Client_Name"].ToString();

            }
            DbCL.Conn.Close();
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/View_client.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string ID = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Delete")
            {
                DbCL.executeRdr("delete from tbl_Factory where ID='" + ID + "'");
                PanelOK.Visible = true;
                lblOk.Text = "Data Deleted Successfully...";
                Buinddata();
            }
            else if (e.CommandName == "Edit")
            {
                Response.Redirect("Update_factory.aspx?ID=" + ID);
            }
            
        }
    }
}