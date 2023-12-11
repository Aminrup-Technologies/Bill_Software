using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace Bill_Software.Update
{
    public partial class password : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (Session["USERID"].ToString() == "admin")
            {
                AuthenticateADMIN();
            }
        }

        private void AuthenticateADMIN()
        {

            string cmdString = "select Password from tbl_card_login where User_Id='" + Session["USERID"].ToString() + "' and Password='" + txtCrntPassword.Text.Trim() + "'";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdString, DbCL.Conn);
            SqlDataReader Rdr;
            Rdr = cmd.ExecuteReader();
            if (Rdr.Read())
            {
                UpdatePassword();
            }
            else
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "Your current Password doesn't match with the Password you have entered.";
                btnReset.Visible = true;
                btnUpdate.Visible = false;
            }
            DbCL.Conn.Close();
        }

        private void UpdatePassword()
        {
            if (Session["USERID"].ToString() == "admin")
            {

                DbCL.executeRdr("UPDATE tbl_card_login SET Password='" + txtConfNewPassword.Text.Trim() + "' WHERE User_Id='" + Session["USERID"].ToString() + "'");
                PanelOk.Visible = true;
                LabelOk.Text = "Password changed successfully.";
                btnUpdate.Visible = false;


            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/admin/Update/password.aspx");
        }
    }
}