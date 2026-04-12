using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Linq;

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
            public string PhoneNo { get; set; }
            public string ProfilePictureUrl { get; set; }
            public int? RoleId { get; set; }
            public string RoleName { get; set; }
            public int FailedAccessCount { get; set; }
            public DateTime? LockoutEnd { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int currentYear = DateTime.Now.Year;
                lbl_crntyr.Text = $"{currentYear - 2}-{currentYear}";

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
                const string cmdString = @"
                    SELECT TOP 1 
                        u.Id, u.User_Id, u.Password, u.PasswordHash, u.PasswordSalt, 
                        u.MustChangePassword, u.EmailVerified, u.Email, u.Phone_no, u.ProfilePictureUrl,
                        u.RoleId, r.RoleName, u.FailedAccessCount, u.LockoutEnd
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
                                ShowError("Invalid Username or User is Inactive.");
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
                                Email = rdr["Email"]?.ToString() ?? string.Empty,
                                PhoneNo = rdr["Phone_no"]?.ToString() ?? string.Empty,
                                ProfilePictureUrl = rdr["ProfilePictureUrl"]?.ToString() ?? string.Empty,
                                RoleId = rdr["RoleId"] != DBNull.Value ? (int?)Convert.ToInt32(rdr["RoleId"]) : null,
                                RoleName = rdr["RoleName"]?.ToString() ?? string.Empty,
                                FailedAccessCount = rdr["FailedAccessCount"] != DBNull.Value ? Convert.ToInt32(rdr["FailedAccessCount"]) : 0,
                                LockoutEnd = rdr["LockoutEnd"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(rdr["LockoutEnd"]) : null
                            };

                            if (rdr["PasswordHash"] is byte[]) { user.PasswordHash = (byte[])rdr["PasswordHash"]; }
                            else if (rdr["PasswordHash"] != DBNull.Value) { user.PasswordHash = SafeBase64Decode(rdr["PasswordHash"].ToString()); }

                            if (rdr["PasswordSalt"] is byte[]) { user.PasswordSalt = (byte[])rdr["PasswordSalt"]; }
                            else if (rdr["PasswordSalt"] != DBNull.Value) { user.PasswordSalt = SafeBase64Decode(rdr["PasswordSalt"].ToString()); }

                            rdr.Close();

                            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.Now)
                            {
                                TimeSpan lockoutRemaining = user.LockoutEnd.Value - DateTime.Now;
                                ShowError($"Account locked due to multiple failed login attempts. Please try again in {lockoutRemaining.Minutes} minutes.");
                                return;
                            }

                            bool isPasswordValid = false;
                            if (user.PasswordHash != null && user.PasswordSalt != null)
                            {
                                try { isPasswordValid = VerifyPasswordPBKDF2(txtPassword.Text.Trim(), user.PasswordHash, user.PasswordSalt); }
                                catch { isPasswordValid = false; }
                            }

                            if (!isPasswordValid && !string.IsNullOrEmpty(user.PasswordPlain) && user.PasswordPlain == txtPassword.Text.Trim())
                            {
                                isPasswordValid = true;
                            }

                            if (!isPasswordValid)
                            {
                                HandleFailedLoginAttempt(user);
                                return;
                            }

                            Session["USERID"] = user.UserId;
                            Session["USERTYPE"] = cmbLoginAs.SelectedValue;
                            Session["UserDbId"] = user.Id;
                            Session["RoleId"] = user.RoleId;
                            Session["RoleName"] = string.IsNullOrEmpty(user.RoleName) ? "Standard User" : user.RoleName;
                            Session["ProfilePic"] = string.IsNullOrEmpty(user.ProfilePictureUrl)
                                ? "~/corporate/business/WebImages/default-avatar.png"
                                : user.ProfilePictureUrl;

                            string newToken = Guid.NewGuid().ToString();
                            string ipAddr = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                            if (string.IsNullOrEmpty(ipAddr)) ipAddr = Request.ServerVariables["REMOTE_ADDR"];
                            string userAgent = Request.UserAgent ?? "Unknown";

                            using (var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                            {
                                cn.Open();
                                using (var cmdKill = new SqlCommand("UPDATE dbo.ActiveSessions SET IsActive = 0 WHERE UserId = @UserId", cn))
                                {
                                    cmdKill.Parameters.AddWithValue("@UserId", user.Id);
                                    cmdKill.ExecuteNonQuery();
                                }

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

                                string sqlLastLogin = @"UPDATE tbl_login 
                                    SET LastLogin = @LastLogin, FailedAccessCount = 0, LockoutEnd = NULL 
                                    WHERE Id = @UserId";

                                using (var cmdUpdateLogin = new SqlCommand(sqlLastLogin, cn))
                                {
                                    cmdUpdateLogin.Parameters.AddWithValue("@LastLogin", DateTimeOffset.Now);
                                    cmdUpdateLogin.Parameters.AddWithValue("@UserId", user.Id);
                                    cmdUpdateLogin.ExecuteNonQuery();
                                }
                            }
                            Session["SessionToken"] = newToken;

                            if (user.MustChangePassword)
                            {
                                Session["MustUpdateUserId"] = user.Id;
                                Session["MustUpdateUser_UserId"] = user.UserId;
                                Session["MustUpdateUser_Email"] = user.Email;
                                Response.Redirect("~/corporate/business/app/settings.aspx", false);
                                return;
                            }

                            // 7. NEW: Centralized Contact Verification Lockout
                            bool missingContact = string.IsNullOrEmpty(user.Email) || !user.EmailVerified || string.IsNullOrEmpty(user.PhoneNo);
                            if (missingContact)
                            {
                                Session["MustVerifyContact"] = true;
                                Response.Redirect("~/corporate/business/app/settings.aspx", false);
                                return;
                            }

                            if (!user.EmailVerified || string.IsNullOrEmpty(user.Email))
                            {
                                pnlLogin.Visible = false;
                                pnlEmailVerification.Visible = true;
                                txtVerifyEmail.Text = user.Email;
                                return;
                            }

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

        private void HandleFailedLoginAttempt(UserModel user)
        {
            int newFailCount = user.FailedAccessCount + 1;
            DateTime? newLockoutEnd = null;

            if (newFailCount >= 5)
            {
                newLockoutEnd = DateTime.Now.AddMinutes(15);
            }

            try
            {
                using (var cn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    cn.Open();
                    string updateSql = "UPDATE tbl_login SET FailedAccessCount = @FailCount, LockoutEnd = @LockoutEnd WHERE Id = @UserId";
                    using (var cmd = new SqlCommand(updateSql, cn))
                    {
                        cmd.Parameters.AddWithValue("@FailCount", newFailCount);
                        cmd.Parameters.AddWithValue("@LockoutEnd", (object)newLockoutEnd ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@UserId", user.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            if (newLockoutEnd.HasValue)
            {
                ShowError("Account locked due to too many failed login attempts. Please try again in 15 minutes.");
            }
            else
            {
                ShowError($"Invalid Password. Attempt {newFailCount} of 5.");
            }
            txtPassword.Focus();
        }

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

        protected async void btnSendReset_Click(object sender, EventArgs e)
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

                    string query = "SELECT Id, IsActive, Email, Phone_no, Name FROM tbl_login WHERE User_Id = @UserId";

                    int userId;
                    bool isActive = false;
                    string userEmail = string.Empty;
                    string userMobile = string.Empty;
                    string userName = string.Empty;

                    using (SqlCommand cmd = new SqlCommand(query, cn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", forgotUserId);
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (!rdr.Read())
                            {
                                ShowError("The specified User ID does not exist in our system.");
                                return;
                            }

                            userId = Convert.ToInt32(rdr["Id"]);
                            isActive = Convert.ToBoolean(rdr["IsActive"]);
                            userEmail = rdr["Email"]?.ToString() ?? "";
                            userMobile = rdr["Phone_no"]?.ToString() ?? "";
                            userName = rdr["Name"]?.ToString();
                            if (string.IsNullOrEmpty(userName)) userName = forgotUserId;
                        }
                    }

                    if (!isActive)
                    {
                        ShowError("This account is currently marked as Inactive. Please contact your system administrator.");
                        return;
                    }

                    if (string.IsNullOrEmpty(userEmail) && string.IsNullOrEmpty(userMobile))
                    {
                        ShowError("Reset Failed: No email address or mobile number is registered to this account. Please contact HR to update your profile.");
                        return;
                    }

                    string tempPassword = GenerateOTP() + "Tmp!";

                    string updateQuery = "UPDATE tbl_login SET Password = @TempPassword, PasswordHash = NULL, PasswordSalt = NULL, MustChangePassword = 1, FailedAccessCount = 0, LockoutEnd = NULL WHERE Id = @Id";
                    using (SqlCommand cmdUpdate = new SqlCommand(updateQuery, cn))
                    {
                        cmdUpdate.Parameters.AddWithValue("@TempPassword", tempPassword);
                        cmdUpdate.Parameters.AddWithValue("@Id", userId);
                        cmdUpdate.ExecuteNonQuery();
                    }

                    bool emailSent = false;
                    bool waSent = false;

                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        string subject = "FLAME-EX ERP - Password Reset Request";
                        string body = $"<h2>Password Reset</h2>" +
                                      $"<p>Hello,</p>" +
                                      $"<p>A password reset was requested for User ID: <strong>{forgotUserId}</strong></p>" +
                                      $"<p>Your temporary password is: <strong>{tempPassword}</strong></p>" +
                                      $"<p>Please log in using this temporary password. You will be prompted to create a new, secure password immediately after logging in.</p>";

                        // Assign the real success value
                        emailSent = SendEmail(userEmail, subject, body);
                    }

                    if (!string.IsNullOrEmpty(userMobile))
                    {
                        // Await and assign the real success value
                        waSent = await SendWhatsAppMessageAsync(userMobile, userName, tempPassword);
                    }

                    // Evaluate actual success based on real network responses
                    if (emailSent && waSent)
                    {
                        ShowError($"A temporary password has been sent to your registered email and WhatsApp number ending in {GetMaskedMobile(userMobile)}.");
                    }
                    else if (emailSent)
                    {
                        ShowError($"A temporary password has been sent to your registered email address.");
                    }
                    else if (waSent)
                    {
                        ShowError($"A temporary password has been sent via WhatsApp to the number ending in {GetMaskedMobile(userMobile)}.");
                    }
                    else
                    {
                        // BOTH FAILED! Do not redirect. Warn the user.
                        ShowError("Failed to send the temporary password. Please check your network connection or contact the administrator.");
                        return;
                    }

                    pnlForgotPassword.Visible = false;
                    pnlLogin.Visible = true;
                }
            }
            catch (Exception ex)
            {
                ShowError("An error " + ex.Message + " occurred while processing your request. Please try again.");
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
                string otp = GenerateOTP();
                Session["GeneratedOTP"] = otp;
                Session["EmailToVerify"] = emailToVerify;

                string subject = "Your FLAME-EX ERP Verification OTP";
                string body = $"<h2>Email Verification</h2>" +
                              $"<p>Your One-Time Password (OTP) is: <strong>{otp}</strong></p>" +
                              $"<p>Please enter this code on the login page to verify your email address.</p>";

                SendEmail(emailToVerify, subject, body);

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

            if (string.IsNullOrEmpty(userId))
            {
                ShowError("Your session has expired. Please refresh the page and log in again.");
                pnlEmailVerification.Visible = false;
                pnlLogin.Visible = true;
                return;
            }

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

                    Session.Remove("GeneratedOTP");
                    Session.Remove("EmailToVerify");

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
            LogError(ex);
        }

        //private void LogError(Exception ex)
        //{
        //    string logMessage = $"Message: {ex.Message}\nStackTrace: {ex.StackTrace}\nInnerException: {ex.InnerException}";
        //    System.Diagnostics.Trace.WriteLine(logMessage);
        //}

        private void LogError(Exception ex)
        {
            try
            {
                string logDirectory = Server.MapPath("~/App_Data/ErrorLogs/");
                if (!System.IO.Directory.Exists(logDirectory))
                {
                    System.IO.Directory.CreateDirectory(logDirectory);
                }

                string filePath = System.IO.Path.Combine(logDirectory, $"SystemErrors_{DateTime.Now:yyyy-MM-dd}.txt");

                string logMessage = $"[{DateTime.Now:HH:mm:ss}] Message: {ex.Message}\nStackTrace: {ex.StackTrace}\nInnerException: {ex.InnerException}\n--------------------------\n";

                System.IO.File.AppendAllText(filePath, logMessage);
            }
            catch
            {
                // Fail silently so the app doesn't crash if logging fails
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

        private string GetMaskedMobile(string mobile)
        {
            if (string.IsNullOrEmpty(mobile) || mobile.Length < 4) return "****";
            return "****" + mobile.Substring(mobile.Length - 4);
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

        private bool SendEmail(string toAddress, string subject, string body)
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                string fromAddress = ConfigurationManager.AppSettings["SmtpFrom"];
                string smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
                string smtpPass = ConfigurationManager.AppSettings["SmtpPass"];
                string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
                int smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]);
                bool enableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["SmtpEnableSsl"]);

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(fromAddress, "FLAME-EX ERP");
                mail.To.Add(toAddress);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient(smtpHost, smtpPort);
                smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
                smtp.EnableSsl = enableSsl;

                smtp.Send(mail);

                return true; // Successfully sent
            }
            catch (Exception ex)
            {
                LogError(ex);
                return false; // Failed to send
            }
        }

        private string GenerateOTP()
        {
            Random rand = new Random();
            return rand.Next(100000, 999999).ToString();
        }

        private async System.Threading.Tasks.Task<bool> SendWhatsAppMessageAsync(string targetPhoneNumber, string userName, string tempPassword)
        {
            try
            {
                // Force TLS 1.2 for the MSG91 API connection
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                string authKey = ConfigurationManager.AppSettings["Msg91AuthKey"];
                string integratedNumber = ConfigurationManager.AppSettings["Msg91IntegratedNumber"];
                string templateName = "erp_system_access";

                if (string.IsNullOrEmpty(authKey) || string.IsNullOrEmpty(integratedNumber))
                {
                    System.Diagnostics.Debug.WriteLine("MSG91 skipped: AuthKey or IntegratedNumber missing.");
                    return false; // Failed (missing config)
                }

                string cleanPhone = new string(targetPhoneNumber.Where(char.IsDigit).ToArray());
                if (cleanPhone.Length == 10) cleanPhone = "91" + cleanPhone;

                string url = "https://api.msg91.com/api/v5/whatsapp/whatsapp-outbound-message/bulk/";

                // 1. Updated Namespace to match the approved template
                // 2. Updated components to use body_1 (text) and button_1 (url text variable)
                string jsonPayload = $@"{{
                    ""integrated_number"": ""{integratedNumber}"",
                    ""content_type"": ""template"",
                    ""payload"": {{
                        ""messaging_product"": ""whatsapp"",
                        ""type"": ""template"",
                        ""template"": {{
                            ""name"": ""{templateName}"",
                            ""language"": {{
                                ""code"": ""en_US"",
                                ""policy"": ""deterministic""
                            }},
                            ""namespace"": ""af05507b_02e4_4d95_8f8c_164ce03fc2df"",
                            ""to_and_components"": [
                                {{
                                    ""to"": [
                                        ""{cleanPhone}""
                                    ],
                                    ""components"": {{
                                        ""body_1"": {{
                                            ""type"": ""text"",
                                            ""value"": ""{tempPassword}"" 
                                        }},
                                        ""button_1"": {{
                                            ""subtype"": ""url"",
                                            ""type"": ""text"",
                                            ""value"": ""{tempPassword}""
                                        }}
                                    }}
                                }}
                            ]
                        }}
                    }}
                }}";

                using (var httpClient = new System.Net.Http.HttpClient())
                {
                    var content = new System.Net.Http.StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

                    using (var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, url))
                    {
                        request.Headers.Add("authkey", authKey);
                        request.Content = content;

                        using (System.Net.Http.HttpResponseMessage response = await httpClient.SendAsync(request))
                        {
                            string result = await response.Content.ReadAsStringAsync();

                            if (!response.IsSuccessStatusCode)
                            {
                                System.Diagnostics.Debug.WriteLine($"MSG91 API Error: {result}");
                                LogError(new Exception($"WhatsApp API Failed: {result}"));
                                return false; // Failed (API rejected it)
                            }

                            return true; // Successfully sent
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(new Exception("WhatsApp MSG91 Code Error: " + ex.Message, ex));
                return false; // Failed (Code/Network error)
            }
        }
        #endregion
    }
}