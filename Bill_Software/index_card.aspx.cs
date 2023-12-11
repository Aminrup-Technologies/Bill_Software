using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;

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
            string cmdString = "select User_Id,Password from tbl_card_login where User_Id='" + txtUserName.Text.Trim() + "'";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdString, DbCL.Conn);
            SqlDataReader Rdr;
            Rdr = cmd.ExecuteReader();
            if (!Rdr.Read())
            {
                {
                    //PanelError.Visible = true;
                    lblErrorMsg.Text = "Invalid Username...";
                    txtUserName.Focus();
                }
            }
            else
            {
                if (Rdr["Password"].ToString() == txtPassword.Text.Trim())
                {
                    Session["USERID"] = txtUserName.Text;
                    //Session["USERTYPE"] = cmbLoginAs.SelectedValue.ToString();
                    Response.Redirect("~/admin/home.aspx");
                }
                else
                {
                    //PanelError.Visible = true;
                    lblErrorMsg.Text = "Wrong Password.. ";
                    txtPassword.Focus();
                }
            }
            //}

        }
    }
}