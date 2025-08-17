using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app.Update
{
    public partial class emailid : System.Web.UI.Page
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
                string cmdString = "select Email from tbl_login where User_Id='" + Session["USERID"].ToString() + "'";
                DbCL.Sqlconnection();
                DbCL.ConnectDb();
                SqlCommand cmd = new SqlCommand(cmdString, DbCL.Conn);
                SqlDataReader Rdr;
                Rdr = cmd.ExecuteReader();
                if (Rdr.Read())
                {
                    lblCrntEmailId.Text = Rdr["Email"].ToString();
                }
                DbCL.Conn.Close();
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (Session["USERID"].ToString() == "admin")
            {
                DbCL.executeRdr("UPDATE tbl_login SET Email='" + txtEmailId.Text.Trim() + "' where User_Id='" + Session["USERID"].ToString() + "'");
            }
            Response.Redirect("~/corporate/business/app/Update/emailid.aspx");

        }
    }
}