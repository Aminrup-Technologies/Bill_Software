using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace Bill_Software.admin
{
    public partial class WebForm8 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Displayname();

            }
        }
        private void Displayname()
        {
            string cmdstring = "select Name,Phone_no,Email FROM tbl_card_login where User_Id='" + Session["USERID"].ToString() + "'";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblName.Text = re["Name"].ToString();
                lblContactNo.Text = re["Phone_no"].ToString();
                lblEmailID.Text = re["Email"].ToString();
            }
            DbCL.Conn.Close();

        }
    }
}