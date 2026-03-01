using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI;

namespace Bill_Software
{
    public partial class index : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();

        private class UserModel
        {
            public int Id { get; set; }
            public string UserId { get; set; }
            public string PasswordPlain { get; set; }
            public byte[] PasswordHash { get; set; }
            public byte[] PasswordSalt { get; set; }
            public bool MustChangePassword { get; set; }
            public bool EmailVerified { get; set; }
            public string Email { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Dynamic financial year logic
                int currentYear = DateTime.Now.Year;
                lbl_crntyr.Text = $"{currentYear - 1}-{currentYear}"; // e.g., 2025-2026

                if (Request.Cookies["myCookie"] != null)
                {
                    HttpCookie cookie = Request.Cookies.Get("myCookie");
                    txtUserName.Text = cookie.Values["username"];
                    txtPassword.Attributes.Add("value", cookie.Values["password"]);
                    cookie.Expires = DateTime.Now.AddYears(1);
                    Response.Cookies.Add(cookie);
                }

                IpAddress();
                txtUserName.Focus();
            }
        }

        private void IpAddress()
        {
            string strIpAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(strIpAddress))
            {
                strIpAddress = Request.ServerVariables["REMOTE_ADDR"];
            }
            lblIP.Text = strIpAddress;
            lblpcname.Text = Environment.MachineName;
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (chkRememberMe.Checked)
            {
                HttpCookie myCookie = new HttpCookie("myCookie");
                myCookie.Values.Add("username", txtUserName.Text);
                myCookie.Expires = DateTime.Now.AddDays(30);
                Response.Cookies.Add(myCookie);
            }

            if (cmbLoginAs.SelectedIndex == 0 || cmbLoginAs.SelectedIndex == 1)
            {
                const string cmdString = "SELECT TOP 1 Id, User_Id, Password, PasswordHash, PasswordSalt, MustChangePassword, EmailVerified, Email FROM tbl_login WHERE User_Id = @UserId AND IsActive = 1";

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
                                ShowError("Invalid Username or User is Inactive...");
                                txtUserName.Focus();
                                return;
                            }

                            var user = new UserModel
                            {
                                Id = rdr["Id"] != DBNull.Value ? Convert.ToInt32(rdr["Id"]) : 0,
                                UserId = rdr["User_Id"]?.ToString() ?? string.Empty,
                                PasswordPlain = rdr["Password"]?.ToString() ?? string.Empty,
                                MustChangePassword = rdr["MustChangePassword"] != DBNull.Value && Convert.ToBoolean(rdr["MustChangePassword"]),
                                EmailVerified = rdr["EmailVerified"] != DBNull.Value && Convert.ToBoolean(rdr["EmailVerified"]),
                                Email = rdr["Email"]?.ToString() ?? string.Empty
                            };

                            // Safe reading for Hash/Salt
                            if (rdr["PasswordHash"] is byte[]) { user.PasswordHash = (byte[])rdr["PasswordHash"]; }
                            else if (rdr["PasswordHash"] != DBNull.Value) { user.PasswordHash = SafeBase64Decode(rdr["PasswordHash"].ToString()); }

                            if (rdr["PasswordSalt"] is byte[]) { user.PasswordSalt = (byte[])rdr["PasswordSalt"]; }
                            else if (rdr["PasswordSalt"] != DBNull.Value) { user.PasswordSalt = SafeBase64Decode(rdr["PasswordSalt"].ToString()); }

                            bool isPasswordValid = false;

                            if (user.PasswordHash != null && user.PasswordSalt != null)
                            {
                                try { isPasswordValid = VerifyPasswordPBKDF2(txtPassword.Text.Trim(), user.PasswordHash, user.PasswordSalt); }
                                catch { isPasswordValid = false; }
                            }

                            // Fallback to plain text
                            if (!isPasswordValid && !string.IsNullOrEmpty(user.PasswordPlain) && user.PasswordPlain == txtPassword.Text.Trim())
                            {
                                isPasswordValid = true;
                            }

                            if (!isPasswordValid)
                            {
                                ShowError("Wrong Password..");
                                txtPassword.Focus();
                                return;
                            }

                            // ====== SUCCESSFUL LOGIN ======
                            Session["USERID"] = user.UserId;
                            Session["USERTYPE"] = cmbLoginAs.SelectedValue;
                            Session["UserDbId"] = user.Id;

                            // ====== START: NEW SINGLE SESSION LOGIC ======
                            string newToken = Guid.NewGuid().ToString();
                            string ipAddr = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                            if (string.IsNullOrEmpty(ipAddr)) ipAddr = Request.ServerVariables["REMOTE_ADDR"];
                            string userAgent = Request.UserAgent ?? "Unknown";

                            using (var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                            {
                                cn.Open();

                                // 1. Invalidate all previous sessions for this user (Forces logout on other devices)
                                using (var cmdKill = new SqlCommand("UPDATE dbo.ActiveSessions SET IsActive = 0 WHERE UserId = @UserId", cn))
                                {
                                    cmdKill.Parameters.AddWithValue("@UserId", user.Id);
                                    cmdKill.ExecuteNonQuery();
                                }

                                // 2. Create the new session
                                string sqlInsert = @"INSERT INTO dbo.ActiveSessions (SessionToken, UserId, IPAddress, UserAgent, IsActive) 
                                             VALUES (@Token, @UserId, @IP, @UA, 1)";
                                using (var cmdIns = new SqlCommand(sqlInsert, cn))
                                {
                                    cmdIns.Parameters.AddWithValue("@Token", newToken);
                                    cmdIns.Parameters.AddWithValue("@UserId", user.Id);
                                    cmdIns.Parameters.AddWithValue("@IP", ipAddr);
                                    cmdIns.Parameters.AddWithValue("@UA", userAgent);
                                    cmdIns.ExecuteNonQuery();
                                }
                            }

                            Session["SessionToken"] = newToken;
                            // ====== END: NEW SINGLE SESSION LOGIC ======


                            if (user.MustChangePassword || !user.EmailVerified || string.IsNullOrEmpty(user.Email))
                            {
                                Session["MustUpdateUserId"] = user.Id;
                                Session["MustUpdateUser_UserId"] = user.UserId;
                                Session["MustUpdateUser_Email"] = user.Email;
                                Response.Redirect("~/corporate/business/app/settings.aspx", false);
                                return;
                            }

                            Response.Redirect("~/corporate/business/app/home.aspx", false);
                        }
                    }
                }
                catch (Exception)
                {
                    ShowError("An error occurred during login. Please contact admin.");
                }
                finally
                {
                    DbCL.DisconnectDb();
                }
            }
        }

        protected void btnLogin_Click_OLD(object sender, EventArgs e)
        {
            if (chkRememberMe.Checked)
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
                                ShowError("Invalid Username...");
                                txtUserName.Focus();
                                return;
                            }

                            var user = new UserModel
                            {
                                Id = rdr["Id"] != DBNull.Value ? Convert.ToInt32(rdr["Id"]) : 0,
                                UserId = rdr["User_Id"]?.ToString() ?? string.Empty,
                                PasswordPlain = rdr["Password"]?.ToString() ?? string.Empty,
                                MustChangePassword = rdr["MustChangePassword"] != DBNull.Value && Convert.ToBoolean(rdr["MustChangePassword"]),
                                EmailVerified = rdr["EmailVerified"] != DBNull.Value && Convert.ToBoolean(rdr["EmailVerified"]),
                                Email = rdr["Email"]?.ToString() ?? string.Empty
                            };

                            // Safe reading for Hash/Salt (Compatible with older C# versions)
                            if (rdr["PasswordHash"] is byte[])
                            {
                                user.PasswordHash = (byte[])rdr["PasswordHash"];
                            }
                            else if (rdr["PasswordHash"] != DBNull.Value)
                            {
                                user.PasswordHash = SafeBase64Decode(rdr["PasswordHash"].ToString());
                            }

                            if (rdr["PasswordSalt"] is byte[])
                            {
                                user.PasswordSalt = (byte[])rdr["PasswordSalt"];
                            }
                            else if (rdr["PasswordSalt"] != DBNull.Value)
                            {
                                user.PasswordSalt = SafeBase64Decode(rdr["PasswordSalt"].ToString());
                            }

                            bool isPasswordValid = false;

                            if (user.PasswordHash != null && user.PasswordSalt != null)
                            {
                                try { isPasswordValid = VerifyPasswordPBKDF2(txtPassword.Text.Trim(), user.PasswordHash, user.PasswordSalt); }
                                catch { isPasswordValid = false; }
                            }

                            // Fallback to plain text
                            if (!isPasswordValid && !string.IsNullOrEmpty(user.PasswordPlain) && user.PasswordPlain == txtPassword.Text.Trim())
                            {
                                isPasswordValid = true;
                            }

                            if (!isPasswordValid)
                            {
                                ShowError("Wrong Password..");
                                txtPassword.Focus();
                                return;
                            }

                            // Success
                            Session["USERID"] = user.UserId;
                            Session["USERTYPE"] = cmbLoginAs.SelectedValue;
                            Session["UserDbId"] = user.Id;

                            if (user.MustChangePassword || !user.EmailVerified || string.IsNullOrEmpty(user.Email))
                            {
                                Session["MustUpdateUserId"] = user.Id;
                                Session["MustUpdateUser_UserId"] = user.UserId;
                                Session["MustUpdateUser_Email"] = user.Email;
                                Response.Redirect("~/corporate/business/app/settings.aspx", false);
                                return;
                            }

                            Response.Redirect("~/corporate/business/app/home.aspx", false);
                        }
                    }
                }
                catch (Exception)
                {
                    ShowError("An error occurred during login. Please contact admin.");
                }
                finally
                {
                    DbCL.DisconnectDb();
                }
            }
        }

        private void ShowError(string message)
        {
            PanelError.Visible = true;
            lblErrorMsg.Text = message;
        }

        private byte[] SafeBase64Decode(string base64)
        {
            try { return Convert.FromBase64String(base64); } catch { return null; }
        }

        #region Password Verification Helpers (PBKDF2)
        private bool VerifyPasswordPBKDF2(string password, byte[] storedHash, byte[] storedSalt, int iterations = 100000)
        {
            if (storedHash == null || storedSalt == null) return false;
            using (var derive = new Rfc2898DeriveBytes(password, storedSalt, iterations))
            {
                var computed = derive.GetBytes(storedHash.Length);
                return AreByteArraysEqual(computed, storedHash);
            }
        }

        private bool AreByteArraysEqual(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
        #endregion
    }
}