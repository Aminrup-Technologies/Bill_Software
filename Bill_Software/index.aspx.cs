using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI;
using System.Net;
using System.Net.Mail;

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

            // NEW ENRICHMENT PROPERTIES
            public string ProfilePictureUrl { get; set; }
            public int? RoleId { get; set; }
            public string RoleName { get; set; }
        } 

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int currentYear = DateTime.Now.Year;
                lbl_crntyr.Text = $"{currentYear - 2}-{currentYear}";

                // BUG FIX: Only load the username. Do not attempt to load a password from the cookie.
                if (Request.Cookies["myCookie"] != null)
                {
                    HttpCookie cookie = Request.Cookies.Get("myCookie");
                    txtUserName.Text = cookie.Values["username"];
                }

                IpAddress();
                txtUserName.Focus();
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            // Reset error panel on new attempt
            PanelError.Visible = false;

            if (chkRememberMe.Checked)
            {
                HttpCookie myCookie = new HttpCookie("myCookie");
                myCookie.Values.Add("username", txtUserName.Text);
                myCookie.Expires = DateTime.Now.AddDays(30);
                Response.Cookies.Add(myCookie);
            }

            if (cmbLoginAs.SelectedIndex == 0 || cmbLoginAs.SelectedIndex == 1)
            {
                //const string cmdString = "SELECT TOP 1 Id, User_Id, Password, PasswordHash, PasswordSalt, MustChangePassword, EmailVerified, Email FROM tbl_login WHERE User_Id = @UserId AND IsActive = 1";
                const string cmdString = @"
                    SELECT TOP 1 
                        u.Id, u.User_Id, u.Password, u.PasswordHash, u.PasswordSalt, 
                        u.MustChangePassword, u.EmailVerified, u.Email, u.ProfilePictureUrl,
                        u.RoleId, r.RoleName
                    FROM tbl_login u
                    LEFT JOIN Roles r ON u.RoleId = r.RoleId
                    WHERE u.User_Id = @UserId AND u.IsActive = 1";
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

                            // 1. Map the User Data (Including new enriched fields)
                            var user = new UserModel
                            {
                                Id = rdr["Id"] != DBNull.Value ? Convert.ToInt32(rdr["Id"]) : 0,
                                UserId = rdr["User_Id"]?.ToString() ?? string.Empty,
                                PasswordPlain = rdr["Password"]?.ToString() ?? string.Empty,
                                MustChangePassword = rdr["MustChangePassword"] != DBNull.Value && Convert.ToBoolean(rdr["MustChangePassword"]),
                                EmailVerified = rdr["EmailVerified"] != DBNull.Value && Convert.ToBoolean(rdr["EmailVerified"]),
                                Email = rdr["Email"]?.ToString() ?? string.Empty,

                                // Read enriched data safely
                                ProfilePictureUrl = rdr["ProfilePictureUrl"]?.ToString() ?? string.Empty,
                                RoleId = rdr["RoleId"] != DBNull.Value ? (int?)Convert.ToInt32(rdr["RoleId"]) : null,
                                RoleName = rdr["RoleName"]?.ToString() ?? string.Empty
                            };

                            if (rdr["PasswordHash"] is byte[]) { user.PasswordHash = (byte[])rdr["PasswordHash"]; }
                            else if (rdr["PasswordHash"] != DBNull.Value) { user.PasswordHash = SafeBase64Decode(rdr["PasswordHash"].ToString()); }

                            if (rdr["PasswordSalt"] is byte[]) { user.PasswordSalt = (byte[])rdr["PasswordSalt"]; }
                            else if (rdr["PasswordSalt"] != DBNull.Value) { user.PasswordSalt = SafeBase64Decode(rdr["PasswordSalt"].ToString()); }

                            // 2. Validate Password
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

                            // 3. Set Base Session Variables
                            Session["USERID"] = user.UserId;
                            Session["USERTYPE"] = cmbLoginAs.SelectedValue;
                            Session["UserDbId"] = user.Id;

                            // NEW: Store Enriched Data in Session
                            Session["RoleId"] = user.RoleId;
                            Session["RoleName"] = string.IsNullOrEmpty(user.RoleName) ? "Standard User" : user.RoleName;

                            // If no profile pic exists, provide a default fallback image path
                            Session["ProfilePic"] = string.IsNullOrEmpty(user.ProfilePictureUrl)
                                ? "~/corporate/business/WebImages/default-avatar.png"
                                : user.ProfilePictureUrl;

                            // 4. Create Single Active Session (Log out from other devices)
                            string newToken = Guid.NewGuid().ToString();
                            string ipAddr = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                            if (string.IsNullOrEmpty(ipAddr)) ipAddr = Request.ServerVariables["REMOTE_ADDR"];
                            string userAgent = Request.UserAgent ?? "Unknown";

                            using (var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                            {
                                cn.Open();
                                // Invalidate old sessions
                                using (var cmdKill = new SqlCommand("UPDATE dbo.ActiveSessions SET IsActive = 0 WHERE UserId = @UserId", cn))
                                {
                                    cmdKill.Parameters.AddWithValue("@UserId", user.Id);
                                    cmdKill.ExecuteNonQuery();
                                }

                                // Create new session
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

                                // ==========================================
                                // NEW LOGIC: Update LastLogin & Reset Lockouts
                                // ==========================================
                                string sqlLastLogin = @"UPDATE tbl_login 
                                    SET LastLogin = @LastLogin, 
                                        FailedAccessCount = 0, 
                                        LockoutEnd = NULL 
                                    WHERE Id = @UserId";

                                using (var cmdUpdateLogin = new SqlCommand(sqlLastLogin, cn))
                                {
                                    cmdUpdateLogin.Parameters.AddWithValue("@LastLogin", DateTimeOffset.Now);
                                    cmdUpdateLogin.Parameters.AddWithValue("@UserId", user.Id);
                                    cmdUpdateLogin.ExecuteNonQuery();
                                }
                                // ==========================================
                            }
                            Session["SessionToken"] = newToken;

                            // 5. Check if user MUST change password (e.g., after using Forgot Password)
                            if (user.MustChangePassword)
                            {
                                Session["MustUpdateUserId"] = user.Id;
                                Session["MustUpdateUser_UserId"] = user.UserId;
                                Session["MustUpdateUser_Email"] = user.Email;
                                Response.Redirect("~/corporate/business/app/settings.aspx", false);
                                return;
                            }

                            // 6. Check if email needs verification
                            if (!user.EmailVerified || string.IsNullOrEmpty(user.Email))
                            {
                                pnlLogin.Visible = false;
                                pnlEmailVerification.Visible = true;
                                txtVerifyEmail.Text = user.Email; // Let them see and correct their email
                                return;
                            }

                            // 7. SUCCESSFUL LOGIN! Redirect to home page
                            Response.Redirect("~/corporate/business/app/home.aspx", false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError("An error occurred during login. Please contact admin.");
                    LogError(ex);
                }
                finally
                {
                    DbCL.DisconnectDb();
                }
            }
        }

        // --- NEW EVENT HANDLERS ---

        protected void lnkForgotPassword_Click(object sender, EventArgs e)
        {
            pnlLogin.Visible = false;
            pnlForgotPassword.Visible = true;
            PanelError.Visible = false;
        }

        protected void lnkBackToLogin_Click(object sender, EventArgs e)
        {
            pnlForgotPassword.Visible = false;
            pnlLogin.Visible = true;
            PanelError.Visible = false;
        }

        protected void btnSendReset_Click(object sender, EventArgs e)
        {
            string forgotUserId = txtForgotUserId.Text.Trim();

            if (string.IsNullOrEmpty(forgotUserId))
            {
                ShowError("Please enter your User ID.");
                return;
            }

            try
            {
                using (SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    cn.Open();

                    // TODO Completed: Look up txtForgotUserId.Text in the database to get their Email
                    string getEmailQuery = "SELECT Email FROM tbl_login WHERE User_Id = @UserId AND IsActive = 1";
                    string userEmail = string.Empty;

                    using (SqlCommand cmd = new SqlCommand(getEmailQuery, cn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", forgotUserId);
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            userEmail = result.ToString();
                        }
                    }

                    // Stop if the user doesn't exist or has no email
                    if (string.IsNullOrEmpty(userEmail))
                    {
                        ShowError("No active user found with that ID, or the user has no registered email.");
                        return;
                    }

                    // TODO Completed: Generate a reset token (temporary password) and send an email
                    // We use your GenerateOTP() method and add a string to make it a secure temporary password
                    string tempPassword = GenerateOTP() + "Tmp!";

                    // Update the database: Set the plain text password and force them to change it on next login
                    string updateQuery = "UPDATE tbl_login SET Password = @TempPassword, MustChangePassword = 1 WHERE User_Id = @UserId";
                    using (SqlCommand cmdUpdate = new SqlCommand(updateQuery, cn))
                    {
                        cmdUpdate.Parameters.AddWithValue("@TempPassword", tempPassword);
                        cmdUpdate.Parameters.AddWithValue("@UserId", forgotUserId);
                        cmdUpdate.ExecuteNonQuery();
                    }

                    // Construct and send the email
                    string subject = "FLAME-EX ERP - Password Reset Request";
                    string body = $"<h2>Password Reset</h2>" +
                                  $"<p>Hello,</p>" +
                                  $"<p>A password reset was requested for User ID: <strong>{forgotUserId}</strong></p>" +
                                  $"<p>Your temporary password is: <strong>{tempPassword}</strong></p>" +
                                  $"<p>Please log in using this temporary password. You will be prompted to create a new, secure password immediately after logging in.</p>";

                    SendEmail(userEmail, subject, body);

                    // Update the UI
                    ShowError("A temporary password has been sent to your registered email address.");

                    // Switch back to the main login panel so they can log in
                    pnlForgotPassword.Visible = false;
                    pnlLogin.Visible = true;
                }
            }
            catch (Exception ex)
            {
                ShowError("An error occurred while processing your request. Please try again.");
                LogError(ex);
            }
        }

        protected void btnSendOTP_Click(object sender, EventArgs e)
        {
            string emailToVerify = txtVerifyEmail.Text.Trim();

            if (string.IsNullOrEmpty(emailToVerify))
            {
                ShowError("Please enter a valid email address.");
                return;
            }

            try
            {
                // 1. Generate a random 6-digit OTP
                string otp = GenerateOTP();

                // 2. Save OTP and the target email to the session for verification later
                Session["GeneratedOTP"] = otp;
                Session["EmailToVerify"] = emailToVerify;

                // 3. Construct the email content
                string subject = "Your FLAME-EX ERP Verification OTP";
                string body = $"<h2>Email Verification</h2>" +
                              $"<p>Your One-Time Password (OTP) is: <strong>{otp}</strong></p>" +
                              $"<p>Please enter this code on the login page to verify your email address.</p>";

                // 4. Send the email
                SendEmail(emailToVerify, subject, body);

                // 5. Update the UI
                pnlEnterOTP.Visible = true;
                ShowError("OTP sent successfully to " + emailToVerify);
            }
            catch (Exception)
            {
                ShowError("Could not send OTP email. Please check your network or try again later.");
            }
        }

        protected void btnVerifyOTP_Click(object sender, EventArgs e)
        {
            string enteredOTP = txtOTP.Text.Trim();
            string generatedOTP = Session["GeneratedOTP"] as string;
            string emailToVerify = Session["EmailToVerify"] as string;
            string userId = Session["USERID"] as string;

            // SAFETY CHECK: Ensure session hasn't expired
            if (string.IsNullOrEmpty(userId))
            {
                ShowError("Your session has expired. Please refresh the page and log in again.");
                pnlEmailVerification.Visible = false;
                pnlLogin.Visible = true;
                return;
            }

            // Check if the OTP entered matches the one we generated
            if (!string.IsNullOrEmpty(enteredOTP) && enteredOTP == generatedOTP)
            {
                try
                {
                    using (SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                    {
                        cn.Open();
                        string updateQuery = "UPDATE tbl_login SET Email = @Email, EmailVerified = 1 WHERE User_Id = @UserId";

                        using (SqlCommand cmd = new SqlCommand(updateQuery, cn))
                        {
                            cmd.Parameters.AddWithValue("@Email", emailToVerify);
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Clear the session variables now that we are done with them
                    Session.Remove("GeneratedOTP");
                    Session.Remove("EmailToVerify");

                    // Success! Proceed to the home page
                    Response.Redirect("~/corporate/business/app/home.aspx", false);
                }
                catch (Exception ex)
                {
                    ShowError("An error occurred while saving your verification. Please try again.");
                    LogError(ex);
                }
            }
            else
            {
                ShowError("Invalid OTP. Please check the code and try again.");
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

        private void ShowError(string message, Exception ex)
        {
            //lblError.Text = message;

            // Logging layer
            LogError(ex);
        }

        private void LogError(Exception ex)
        {
            string logMessage = $"Message: {ex.Message}\nStackTrace: {ex.StackTrace}\nInnerException: {ex.InnerException}";

            // Write to file / DB / logging framework
            System.Diagnostics.Trace.WriteLine(logMessage);
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

        private void SendEmail(string toAddress, string subject, string body)
        {
            try
            {
                // Read SMTP settings from web.config
                string fromAddress = ConfigurationManager.AppSettings["SmtpFrom"];
                string smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
                string smtpPass = ConfigurationManager.AppSettings["SmtpPass"];
                string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
                int smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]);
                bool enableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["SmtpEnableSsl"]);

                // Configure the email message
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(fromAddress, "FLAME-EX ERP");
                mail.To.Add(toAddress);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true; // Allows us to use HTML formatting in the email body

                // Configure the SMTP client
                SmtpClient smtp = new SmtpClient(smtpHost, smtpPort);
                smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
                smtp.EnableSsl = enableSsl;

                // Send the email
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw new Exception("Failed to send email.");
            }
        }

        private string GenerateOTP()
        {
            // Generates a random number between 100000 and 999999
            Random rand = new Random();
            return rand.Next(100000, 999999).ToString();
        }
        #endregion
    }
}