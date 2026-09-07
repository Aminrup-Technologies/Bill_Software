/*
-----------------------------------------------------------------------------------------
-- File Name: index_card.aspx.cs
-- When:      2026-09-07
-- Why:       Isolated kiosk login against tbl_card_login. Not tbl_login / ActiveSessions.
-- What:      Parameterized User_Id lookup. Plaintext password compare unchanged.
--            Do not merge into ERP AuthGuard / PBKDF2. See docs/21_Phase3_Infrastructure_Hardening.md.
-----------------------------------------------------------------------------------------
*/
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bill_Software
{
    public partial class index_card : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.Cookies["myCookie"] != null)
                {
                    HttpCookie cookie = Request.Cookies.Get("myCookie");
                    txtUserName.Text = cookie.Values["username"];
                    txtPassword.Attributes.Add("value", cookie.Values["password"]);
                    cookie.Expires.AddYears(1);
                    Response.Cookies.Add(cookie);

                }


                txtUserName.Focus();
            }

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            HttpCookie myCookie = new HttpCookie("myCookie");
            //if (chkRememberMe.Checked == true)
            //{
            //    myCookie.Values.Add("username", txtUserName.Text);
            //    myCookie.Values.Add("password", txtPassword.Text);
            //    myCookie.Expires = DateTime.Now.AddDays(30);
            //    Response.Cookies.Add(myCookie);
            //}
            //if (cmbLoginAs.SelectedIndex == 0)
            //{
            string userId = txtUserName.Text.Trim();
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            try
            {
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT User_Id, Password FROM tbl_card_login WHERE User_Id = @UserId", DbCL.Conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 100) { Value = userId });
                    using (SqlDataReader Rdr = cmd.ExecuteReader())
                    {
                        if (!Rdr.Read())
                        {
                            lblErrorMsg.Text = "Invalid Username...";
                            txtUserName.Focus();
                        }
                        else
                        {
                            if (Rdr["Password"].ToString() == txtPassword.Text.Trim())
                            {
                                Session["USERID"] = txtUserName.Text;
                                Response.Redirect("~/admin/home.aspx");
                            }
                            else
                            {
                                lblErrorMsg.Text = "Wrong Password.. ";
                                txtPassword.Focus();
                            }
                        }
                    }
                }
            }
            finally
            {
                if (DbCL.Conn != null && DbCL.Conn.State != ConnectionState.Closed)
                    DbCL.Conn.Close();
            }
            //}

        }
    }
}