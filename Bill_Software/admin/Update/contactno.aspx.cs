using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace Bill_Software.Update
{
    public partial class contactno : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Binddata();

            }
        }
        private void Binddata()
        {

            if (Session["USERID"].ToString() == "admin")
            {
                string cmdString = "select Phone_no from tbl_card_login where User_Id='" + Session["USERID"].ToString() + "'";
                DbCL.Sqlconnection();
                DbCL.ConnectDb();
                SqlCommand cmd = new SqlCommand(cmdString, DbCL.Conn);
                SqlDataReader Rdr;
                Rdr = cmd.ExecuteReader();
                if (Rdr.Read())
                {
                    lblCrntContactNo.Text = Rdr["Phone_no"].ToString();
                }
                DbCL.Conn.Close();
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (Session["USERID"].ToString() == "admin")
            {
                DbCL.executeRdr("UPDATE tbl_card_login SET Phone_no='" + txtnewContactNo.Text.Trim() + "' where User_Id='" + Session["USERID"].ToString() + "'");
            }
            Response.Redirect("~/admin/Update/contactno.aspx");
        }
    }
}