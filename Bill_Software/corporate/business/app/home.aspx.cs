using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        //protected void Page_Load(object sender, EventArgs e)
        //{
        //    if (HttpContext.Current.Session["USERID"] == null)
        //    {
        //        Response.Redirect("~/index.aspx");
        //    }
        //    if (!IsPostBack)
        //    {
        //        Binddata();
        //        IpAddress();
        //    }
        //}

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
                return;
            }

            // show once per login
            if (Session["HOME_PRPO_NOTICE"] == null)
            {
                pnlNotification.Visible = true;
            }

            // 1. Enforce mandatory update
            if (UserRequiresUpdate())
            {
                // Block page content
                //PanelMain.Visible = false;

                // Open forced-update popup
                OpenForceUpdatePopup();

                return;
            }

            // 2. Normal workflow
            if (!IsPostBack)
            {
                Binddata();
                IpAddress();
            }
        }

        protected void btnDismiss_Click(object sender, EventArgs e)
        {
            Session["HOME_PRPO_NOTICE"] = true;
            pnlNotification.Visible = false;
        }

        private bool UserRequiresUpdate()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            string sql = "SELECT MustChangePassword, EmailVerified, Email FROM tbl_login WHERE User_Id = @UserId";

            using (SqlCommand cmd = new SqlCommand(sql, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@UserId", Session["USERID"].ToString());

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        bool mustChangePassword = dr["MustChangePassword"] != DBNull.Value && (bool)dr["MustChangePassword"];
                        bool emailVerified = dr["EmailVerified"] != DBNull.Value && (bool)dr["EmailVerified"];
                        string email = dr["Email"] != DBNull.Value ? dr["Email"].ToString() : "";

                        // Conditions that require forced update:
                        if (mustChangePassword || !emailVerified || string.IsNullOrEmpty(email))
                            return true;
                    }
                }
            }

            DbCL.DisconnectDb();
            return false;
        }

        private void OpenForceUpdatePopup()
        {
            string popupUrl = "/corporate/business/app/settings.aspx";

            string script = @"
                window.onload = function () {
                    window.open('" + popupUrl + @"',
                                'updatePopup',
                                'width=520,height=450,top=100,left=200,scrollbars=yes');
                };
            ";

            ScriptManager.RegisterStartupScript(this, this.GetType(), "forceUpdatePopup", script, true);
        }

        private void IpAddress()
        {
            string strIpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(strIpAddress))
                strIpAddress = Request.ServerVariables["REMOTE_ADDR"];

            lblIP.Text = strIpAddress;
            lblpcname.Text = Environment.MachineName.ToString();
        }

        private void Binddata()
        {
            string cmdstring = "SELECT Name, Phone_no, Email FROM tbl_login WHERE User_Id = @UserId";

            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@UserId", Session["USERID"].ToString());

                using (SqlDataReader re = cmd.ExecuteReader())
                {
                    if (re.Read())
                    {
                        lblName.Text = re["Name"].ToString();
                        lblContactNo.Text = re["Phone_no"].ToString();
                        lblEmailID.Text = re["Email"].ToString();
                    }
                }
            }

            DbCL.Conn.Close();
        }

        //private void IpAddress()
        //{
        //    string strIpAddress;
        //    strIpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
        //    if (strIpAddress == null)
        //        strIpAddress = Request.ServerVariables["REMOTE_ADDR"];
        //    lblIP.Text = strIpAddress.ToString();
        //    lblpcname.Text = Environment.MachineName.ToString();
        //}

        //private void Binddata()
        //{
        //    string cmdstring = "select Name,Phone_no,Email FROM tbl_login where User_Id='" + Session["USERID"].ToString() + "'";
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
        //    SqlDataReader re = cmd.ExecuteReader();
        //    if (re.Read())
        //    {
        //        lblName.Text = re["Name"].ToString();
        //        lblContactNo.Text = re["Phone_no"].ToString();
        //        lblEmailID.Text = re["Email"].ToString();
        //    }
        //    DbCL.Conn.Close();
        //}
    }
}