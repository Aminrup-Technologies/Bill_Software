using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Net; //Include this namespace
using System.Text;

namespace Bill_Software
{
    public partial class index : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();

        private class UserModel
        {
            public int Id { get; set; }
            public string UserId { get; set; }
            public string PasswordPlain { get; set; }           // legacy column
            public byte[] PasswordHash { get; set; }            // optional, if you migrate to binary storage
            public byte[] PasswordSalt { get; set; }            // optional
            public bool MustChangePassword { get; set; }
            public bool EmailVerified { get; set; }
            public string Email { get; set; }
        }

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

        protected void btnLogin_Click_OLD(object sender, EventArgs e)
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

                string cmdString = "SELECT * FROM tbl_login WHERE User_Id = @UserId";
                

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

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            // Save username in cookie if remember checked (not password).
            if (chkRememberMe.Checked == true)
            {
                HttpCookie myCookie = new HttpCookie("myCookie");
                myCookie.Values.Add("username", txtUserName.Text);
                myCookie.Expires = DateTime.Now.AddDays(30);
                Response.Cookies.Add(myCookie);
            }

            if (cmbLoginAs.SelectedIndex == 0 || cmbLoginAs.SelectedIndex == 1)
            {
                const string cmdString = "SELECT TOP 1 Id, User_Id, Password, PasswordHash, PasswordSalt, MustChangePassword, EmailVerified, Email FROM tbl_login WHERE User_Id = @UserId";

                try
                {
                    DbCL.Sqlconnection();
                    DbCL.ConnectDb();

                    using (SqlCommand cmd = new SqlCommand(cmdString, DbCL.Conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", txtUserName.Text.Trim());

                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (!rdr.Read())
                            {
                                PanelError.Visible = true;
                                lblErrorMsg.Text = "Invalid Username...";
                                txtUserName.Focus();
                                return;
                            }

                            // Build the user model
                            var user = new UserModel();
                            user.Id = rdr["Id"] != DBNull.Value ? Convert.ToInt32(rdr["Id"]) : 0;
                            user.UserId = rdr["User_Id"] != DBNull.Value ? rdr["User_Id"].ToString() : string.Empty;
                            user.PasswordPlain = rdr["Password"] != DBNull.Value ? rdr["Password"].ToString() : string.Empty;

                            // Try reading PasswordHash / PasswordSalt if present (could be varbinary or base64 string)
                            if (rdr["PasswordHash"] != DBNull.Value)
                            {
                                // If column is varbinary in DB it will come as byte[], if varchar then string — handle both.
                                if (rdr["PasswordHash"] is byte[])
                                    user.PasswordHash = (byte[])rdr["PasswordHash"];
                                else
                                {
                                    // try parse as base64
                                    try
                                    {
                                        user.PasswordHash = Convert.FromBase64String(rdr["PasswordHash"].ToString());
                                    }
                                    catch
                                    {
                                        user.PasswordHash = null;
                                    }
                                }
                            }

                            if (rdr["PasswordSalt"] != DBNull.Value)
                            {
                                if (rdr["PasswordSalt"] is byte[])
                                    user.PasswordSalt = (byte[])rdr["PasswordSalt"];
                                else
                                {
                                    try
                                    {
                                        user.PasswordSalt = Convert.FromBase64String(rdr["PasswordSalt"].ToString());
                                    }
                                    catch
                                    {
                                        user.PasswordSalt = null;
                                    }
                                }
                            }

                            user.MustChangePassword = (rdr["MustChangePassword"] != DBNull.Value) && Convert.ToBoolean(rdr["MustChangePassword"]);
                            user.EmailVerified = (rdr["EmailVerified"] != DBNull.Value) && Convert.ToBoolean(rdr["EmailVerified"]);
                            user.Email = rdr["Email"] != DBNull.Value ? rdr["Email"].ToString() : string.Empty;

                            // Verify password:
                            bool isPasswordValid = false;

                            // Prefer hashed verification if both hash+salt exist
                            if (user.PasswordHash != null && user.PasswordSalt != null)
                            {
                                try
                                {
                                    isPasswordValid = VerifyPasswordPBKDF2(txtPassword.Text.Trim(), user.PasswordHash, user.PasswordSalt);
                                }
                                catch
                                {
                                    // if PBKDF2 verification throws, fallback to plaintext check below
                                    isPasswordValid = false;
                                }
                            }

                            // Fallback to legacy plaintext comparison if secure hash not used or verification failed
                            if (!isPasswordValid)
                            {
                                // Note: existing DB appears to use plaintext Password field; this is fallback
                                if (!string.IsNullOrEmpty(user.PasswordPlain) && user.PasswordPlain == txtPassword.Text.Trim())
                                    isPasswordValid = true;
                            }

                            if (!isPasswordValid)
                            {
                                PanelError.Visible = true;
                                lblErrorMsg.Text = "Wrong Password.. ";
                                txtPassword.Focus();
                                return;
                            }

                            // Successful auth: set session values
                            Session["USERID"] = user.UserId;
                            Session["USERTYPE"] = cmbLoginAs.SelectedValue.ToString();
                            Session["UserDbId"] = user.Id;   // numeric pk

                            // Enforce update flow: must change password OR email not verified OR no email recorded
                            if (user.MustChangePassword || !user.EmailVerified || string.IsNullOrEmpty(user.Email))
                            {
                                // Pass required data to the update page via session
                                Session["MustUpdateUserId"] = user.Id;
                                Session["MustUpdateUser_UserId"] = user.UserId;
                                Session["MustUpdateUser_Email"] = user.Email ?? string.Empty;
                                Response.Redirect("~/corporate/business/app/settings.aspx", false);
                                return;
                            }

                            // else allow normal access
                            Response.Redirect("~/corporate/business/app/home.aspx", false); // Avoids ThreadAbortException
                        } // using reader
                    } // using cmd
                }
                catch (Exception ex)
                {
                    // Log exception per your logging framework; show friendly message
                    PanelError.Visible = true;
                    lblErrorMsg.Text = "An error occurred during login. Please contact admin.";
                    // Optionally: log ex.Message somewhere
                }
                finally
                {
                    DbCL.DisconnectDb(); // implement safe disconnect in your DB utility
                }
            }
        }

        #region Password Verification Helpers (PBKDF2)

        // Verifies a PBKDF2 hashed password. Assumes storedHash and storedSalt are byte[].
        // Iteration count and hash algorithm should match what you used when hashing originally.
        // Adjust the iterations parameter to your environment. 100000 is an example baseline.
        private bool VerifyPasswordPBKDF2(string password, byte[] storedHash, byte[] storedSalt, int iterations = 100000)
        {
            if (storedHash == null || storedSalt == null) return false;

            using (var derive = new Rfc2898DeriveBytes(password, storedSalt, iterations))
            {
                var computed = derive.GetBytes(storedHash.Length);
                return AreByteArraysEqual(computed, storedHash);
            }
        }

        //public static (byte[] hash, byte[] salt) HashPassword(string password, int iterations = 100000, int bytes = 32)
        //{
        //    using (var rng = new RNGCryptoServiceProvider())
        //    {
        //        byte[] salt = new byte[16];
        //        rng.GetBytes(salt);

        //        using (var derive = new Rfc2898DeriveBytes(password, salt, iterations))
        //        {
        //            return (derive.GetBytes(bytes), salt);
        //        }
        //    }
        //}



        private bool AreByteArraysEqual(byte[] a, byte[] b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;

            // Constant-time comparison
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }

        #endregion
    }
}