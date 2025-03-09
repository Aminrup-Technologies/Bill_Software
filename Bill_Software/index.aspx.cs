using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Net; //Include this namespace
namespace Bill_Software
{
    public partial class index : System.Web.UI.Page
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
                    lbl_crntyr.Text = DateTime.Now.Year.ToString();
                    cookie.Expires.AddYears(1);
                    Response.Cookies.Add(cookie);

                }

                IpAddress();
                txtUserName.Focus();
            }
        }
        private void IpAddress()
        {
            string strIpAddress;
            strIpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (strIpAddress == null)
                strIpAddress = Request.ServerVariables["REMOTE_ADDR"];
            lblIP.Text = strIpAddress.ToString();
            lblpcname.Text = Environment.MachineName.ToString();

            //------------------ 23.02.2021 ---------------------------------//


            //string hostName = Dns.GetHostName(); // Retrive the Name of HOST
            //Console.WriteLine(hostName);
            //// Get the IP
            //string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();

            //strComputerName = Environment.MachineName.ToString();
            //lblIP.Text = myIP;
            //Console.ReadKey();
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            HttpCookie myCookie = new HttpCookie("myCookie");
            if (chkRememberMe.Checked == true)
            {
                myCookie.Values.Add("username", txtUserName.Text);
                myCookie.Values.Add("password", txtPassword.Text);
                myCookie.Expires = DateTime.Now.AddDays(30);
                Response.Cookies.Add(myCookie);
            }
            if (cmbLoginAs.SelectedIndex == 0 || cmbLoginAs.SelectedIndex == 1)
            {
                //string cmdString = "select TOP 1 User_Id, Password from tbl_login where User_Id='" + txtUserName.Text.Trim() + "'";

                string cmdString = "SELECT User_Id, Password FROM tbl_login WHERE User_Id = @UserId";
                

                DbCL.Sqlconnection();
                DbCL.ConnectDb();
                //SqlCommand cmd = new SqlCommand(cmdString, DbCL.Conn);
                SqlCommand cmd = new SqlCommand(cmdString, DbCL.Conn);
                cmd.Parameters.AddWithValue("@UserId", txtUserName.Text.Trim());

                SqlDataReader Rdr;
                Rdr = cmd.ExecuteReader();
                if (!Rdr.Read())
                {
                    {
                        PanelError.Visible = true;
                        lblErrorMsg.Text = "Invalid Username...";
                        txtUserName.Focus();
                    }
                }
                else
                {
                    if (Rdr["Password"].ToString() == txtPassword.Text.Trim())
                    {
                        Session["USERID"] = txtUserName.Text;
                        Session["USERTYPE"] = cmbLoginAs.SelectedValue.ToString();
                        Response.Redirect("~/corporate/business/app/home.aspx", false); // Avoids ThreadAbortException
                        //The below line of code is commented by PB #31102024 to avoid the Exception
                        //Response.Redirect("~/corporate/business/app/home.aspx");
                    }
                    else
                    {
                        PanelError.Visible = true;
                        lblErrorMsg.Text = "Wrong Password.. ";
                        txtPassword.Focus();
                    }
                }
            }

        }
    }
}