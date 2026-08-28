using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI.WebControls;
using System.Linq;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm2 : System.Web.UI.Page
    {
        private string ConnString => ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
        private static readonly System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // 1. Session Validation First!
                if (Session["USERID"] == null && Session["MustUpdateUserId"] == null)
                {
                    // Note: Assuming your login page is index.aspx based on previous files
                    Response.Redirect("~/index.aspx", false);
                    return; // CRITICAL: Stop executing the rest of the page lifecycle
                }

                // 2. Catch the redirect from a voluntary password update and show success
                if (Request.QueryString["pwd"] == "success")
                {
                    ShowMessage("Password updated securely.", "success");
                }

                // 3. Password Lockout takes ultimate priority
                if (Session["MustUpdateUserId"] != null)
                {
                    pnlStandardProfile.Visible = false;
                    pnlChangePassword.Visible = true;
                    lblPasswordLockoutWarning.Visible = true;
                }
                else
                {
                    // Hide password panel, show standard profile
                    pnlChangePassword.Visible = false;
                    pnlStandardProfile.Visible = true;

                    // 4. Check if they are locked here for Contact Verification
                    if (Session["MustVerifyContact"] != null)
                    {
                        lblContactLockoutWarning.Visible = true;
                    }

                    LoadUserProfile();
                }
            }
        }

        private void LoadUserProfile()
        {
            string userId = Session["USERID"].ToString();

            using (SqlConnection con = new SqlConnection(ConnString))
            {
                // 1. Added [EmailVerified] to the SELECT query
                string query = "SELECT [Name], [Phone_no], [Email], [EmailVerified], [EnableEmailAlerts], [EnableWhatsAppAlerts], [ProfilePictureUrl] FROM tbl_login WHERE User_Id = @UserId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string picUrl = reader["ProfilePictureUrl"].ToString();
                            if (!string.IsNullOrEmpty(picUrl))
                            {
                                imgProfile.ImageUrl = picUrl;
                            }
                            else
                            {
                                imgProfile.ImageUrl = "https://ui-avatars.com/api/?name=" + Uri.EscapeDataString(reader["Name"].ToString()) + "&background=0056b3&color=fff";
                            }

                            txtName.Text = reader["Name"].ToString();
                            ViewState["OrigName"] = txtName.Text;

                            // 2. Load Phone and Check Implicit Verification
                            txtPhone.Text = reader["Phone_no"].ToString();
                            ViewState["OrigPhone"] = txtPhone.Text;
                            // Phone is always verified if it exists, since OTP is required to set it
                            lblPhoneVerified.Visible = !string.IsNullOrEmpty(txtPhone.Text);

                            // 3. Load Email and Check Explicit Verification Flag
                            txtEmail.Text = reader["Email"].ToString();
                            ViewState["OrigEmail"] = txtEmail.Text;
                            bool isEmailVerified = reader["EmailVerified"] != DBNull.Value && Convert.ToBoolean(reader["EmailVerified"]);
                            lblEmailVerified.Visible = isEmailVerified && !string.IsNullOrEmpty(txtEmail.Text);

                            chkEmailAlerts.Checked = reader["EnableEmailAlerts"] != DBNull.Value && Convert.ToBoolean(reader["EnableEmailAlerts"]);
                            ViewState["OrigEmailAlerts"] = chkEmailAlerts.Checked.ToString();

                            chkWhatsAppAlerts.Checked = reader["EnableWhatsAppAlerts"] != DBNull.Value && Convert.ToBoolean(reader["EnableWhatsAppAlerts"]);
                            ViewState["OrigWhatsAppAlerts"] = chkWhatsAppAlerts.Checked.ToString();
                        }
                    }
                }
            }
        }

        // ==========================================
        // NEW PASSWORD UPDATE LOGIC
        // ==========================================
        protected void btnUpdatePassword_Click(object sender, EventArgs e)
        {
            string newPass = txtNewPassword.Text.Trim();
            string confPass = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrEmpty(newPass) || newPass != confPass)
            {
                ShowMessage("Passwords do not match or are empty.", "danger");
                return;
            }

            int dbId = Session["MustUpdateUserId"] != null ? Convert.ToInt32(Session["MustUpdateUserId"]) : Convert.ToInt32(Session["UserDbId"]);
            string userIdStr = Session["USERID"] != null ? Session["USERID"].ToString() : Session["MustUpdateUser_UserId"].ToString();

            byte[] saltBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider()) { rng.GetBytes(saltBytes); }

            // CRITICAL FIX: Use PBKDF2 to match index.aspx.cs logic
            byte[] hashBytes = GeneratePasswordHashPBKDF2(newPass, saltBytes);

            using (SqlConnection con = new SqlConnection(ConnString))
            {
                string query = "UPDATE tbl_login SET PasswordHash = @Hash, PasswordSalt = @Salt, Password = NULL, MustChangePassword = 0, LastPasswordChangeDate = GETUTCDATE() WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@Hash", SqlDbType.VarBinary).Value = hashBytes;
                    cmd.Parameters.Add("@Salt", SqlDbType.VarBinary).Value = saltBytes;
                    cmd.Parameters.AddWithValue("@Id", dbId);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            WriteAuditLog(userIdStr, "- User successfully updated their password.");

            // Clear the lockout session variables
            Session.Remove("MustUpdateUserId");
            Session.Remove("MustUpdateUser_UserId");
            Session.Remove("MustUpdateUser_Email");

            // Check if they were trapped here purely for the password change
            if (pnlStandardProfile.Visible == false)
            {
                // If they ALSO have a contact verification lock pending, reload settings.aspx
                // to show them the standard profile panel with the contact warnings.
                if (Session["MustVerifyContact"] != null)
                {
                    Response.Redirect("~/corporate/business/app/settings.aspx", false);
                }
                // Otherwise, they are fully verified, send them to the dashboard.
                else
                {
                    Response.Redirect("~/corporate/business/app/home.aspx", false);
                }
            }
            else
            {
                // They changed their password voluntarily from the settings menu.
                // Use Response.Redirect back to the same page with a query string parameter 
                // to trigger the success message AFTER the full page reload.
                Response.Redirect("~/corporate/business/app/settings.aspx?pwd=success", false);
            }
        }

        private byte[] GeneratePasswordHashPBKDF2(string password, byte[] salt, int iterations = 100000)
        {
            using (var derive = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                return derive.GetBytes(32); // Creates a 256-bit hash to match the login logic
            }
        }

        protected async void btnSave_Click(object sender, EventArgs e)
        {
            string userId = Session["USERID"].ToString();
            StringBuilder logBuilder = new StringBuilder();

            bool emailChanged = ViewState["OrigEmail"].ToString() != txtEmail.Text;
            bool phoneChanged = ViewState["OrigPhone"].ToString() != txtPhone.Text;

            if (emailChanged && phoneChanged)
            {
                ShowMessage("For security verification, please update your Email Address and Phone Number one at a time. Please save one, verify it, and then change the other.", "warning");
                txtEmail.Text = ViewState["OrigEmail"].ToString();
                txtPhone.Text = ViewState["OrigPhone"].ToString();
                return;
            }

            string profilePicPath = null;
            if (fuProfilePic.HasFile)
            {
                try
                {
                    string ext = Path.GetExtension(fuProfilePic.FileName).ToLower();
                    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                    {
                        string directoryPath = Server.MapPath("~/Uploads/Profiles/");
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        string filename = userId + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ext;
                        string savePath = Path.Combine(directoryPath, filename);
                        fuProfilePic.SaveAs(savePath);

                        profilePicPath = "~/Uploads/Profiles/" + filename;
                        imgProfile.ImageUrl = profilePicPath;

                        logBuilder.AppendLine($"- ProfilePictureUrl updated to {profilePicPath}");
                    }
                    else
                    {
                        ShowMessage("Only JPG and PNG files are allowed for profile pictures.", "danger");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage("Error uploading file. Please try again.", "danger");
                    return;
                }
            }

            if (ViewState["OrigName"].ToString() != txtName.Text) logBuilder.AppendLine($"- Name changed from '{ViewState["OrigName"]}' to '{txtName.Text}'");
            if (ViewState["OrigEmailAlerts"].ToString() != chkEmailAlerts.Checked.ToString()) logBuilder.AppendLine($"- EnableEmailAlerts changed from '{ViewState["OrigEmailAlerts"]}' to '{chkEmailAlerts.Checked}'");
            if (ViewState["OrigWhatsAppAlerts"].ToString() != chkWhatsAppAlerts.Checked.ToString()) logBuilder.AppendLine($"- EnableWhatsAppAlerts changed from '{ViewState["OrigWhatsAppAlerts"]}' to '{chkWhatsAppAlerts.Checked}'");

            UpdateStandardFields(userId, profilePicPath);

            if (emailChanged || phoneChanged)
            {
                string otpCode = GenerateRandomOTP();
                SaveOtpToDatabase(userId, otpCode);

                if (emailChanged)
                {
                    SendEmailOTP(txtEmail.Text, otpCode);
                    ViewState["PendingEmail"] = txtEmail.Text;
                    logBuilder.AppendLine($"- Requested Email change from '{ViewState["OrigEmail"]}' to '{txtEmail.Text}' (OTP Sent)");
                }
                else if (phoneChanged)
                {
                    // MODIFIED: Pass the UserName as well so the MSG91 template populates correctly
                    await SendWhatsAppOTPAsync(txtPhone.Text, otpCode);
                    ViewState["PendingPhone"] = txtPhone.Text;
                    logBuilder.AppendLine($"- Requested Phone change from '{ViewState["OrigPhone"]}' to '{txtPhone.Text}' (OTP Sent)");
                }

                pnlOtp.Visible = true;
                btnSave.Enabled = false;
                ShowMessage("Standard profile data updated. Please verify your new contact detail with the OTP sent to you.", "warning");
            }
            else
            {
                LoadUserProfile();
                ShowMessage("Profile updated successfully.", "success");
            }

            if (logBuilder.Length > 0)
            {
                WriteAuditLog(userId, logBuilder.ToString());
            }
        }

        protected void btnVerifyOtp_Click(object sender, EventArgs e)
        {
            string userId = Session["USERID"].ToString();
            string inputOtp = txtOtp.Text.Trim();

            if (ValidateOtpFromDatabase(userId, inputOtp))
            {
                string updateQuery = "UPDATE tbl_login SET ";
                bool needsComma = false;

                using (SqlConnection con = new SqlConnection(ConnString))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        if (ViewState["PendingEmail"] != null)
                        {
                            updateQuery += "[Email] = @Email, [EmailVerified] = 1 ";
                            cmd.Parameters.AddWithValue("@Email", ViewState["PendingEmail"].ToString());
                            needsComma = true;
                        }

                        if (ViewState["PendingPhone"] != null)
                        {
                            if (needsComma) updateQuery += ", ";
                            updateQuery += "[Phone_no] = @Phone ";
                            cmd.Parameters.AddWithValue("@Phone", ViewState["PendingPhone"].ToString());
                        }

                        updateQuery += " WHERE User_Id = @UserId";
                        cmd.CommandText = updateQuery;
                        cmd.Connection = con;
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        con.Open();
                        cmd.ExecuteNonQuery();

                        ViewState.Remove("PendingEmail");
                        ViewState.Remove("PendingPhone");

                        ClearOtpInDatabase(userId, con);

                        WriteAuditLog(userId, "- OTP Verified successfully. Contact details committed to database.");

                        pnlOtp.Visible = false;
                        btnSave.Enabled = true;
                        txtOtp.Text = "";
                        LoadUserProfile();
                        ShowMessage("Contact details updated successfully!", "success");
                        // Check if both contacts are now fully verified
                        EvaluateContactLockout(userId);
                    }
                }
            }
            else
            {
                ShowMessage("Invalid or Expired OTP. Please try again.", "danger");
            }
        }

        private void EvaluateContactLockout(string userId)
        {
            if (Session["MustVerifyContact"] != null)
            {
                using (SqlConnection con = new SqlConnection(ConnString))
                {
                    string query = "SELECT [Email], [EmailVerified], [Phone_no] FROM tbl_login WHERE User_Id = @UserId";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string em = reader["Email"]?.ToString();
                                bool ev = reader["EmailVerified"] != DBNull.Value && Convert.ToBoolean(reader["EmailVerified"]);
                                string ph = reader["Phone_no"]?.ToString();

                                // If all conditions are satisfied, release the lock!
                                if (!string.IsNullOrEmpty(em) && ev && !string.IsNullOrEmpty(ph))
                                {
                                    Session.Remove("MustVerifyContact");
                                    Response.Redirect("~/corporate/business/app/home.aspx", false);
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void btnCancelOtp_Click(object sender, EventArgs e)
        {
            ViewState.Remove("PendingEmail");
            ViewState.Remove("PendingPhone");
            pnlOtp.Visible = false;
            btnSave.Enabled = true;
            txtOtp.Text = "";
            LoadUserProfile();
            ShowMessage("Contact detail update cancelled.", "warning");
        }

        private void UpdateStandardFields(string userId, string profilePicPath)
        {
            using (SqlConnection con = new SqlConnection(ConnString))
            {
                string updateQuery = @"UPDATE tbl_login SET [Name] = @Name, [EnableEmailAlerts] = @EmailAlerts, [EnableWhatsAppAlerts] = @WhatsAppAlerts";

                if (profilePicPath != null)
                {
                    updateQuery += ", [ProfilePictureUrl] = @ProfilePic";
                }

                updateQuery += " WHERE User_Id = @UserId";

                using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Name", txtName.Text);
                    cmd.Parameters.AddWithValue("@EmailAlerts", chkEmailAlerts.Checked);
                    cmd.Parameters.AddWithValue("@WhatsAppAlerts", chkWhatsAppAlerts.Checked);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    if (profilePicPath != null)
                    {
                        cmd.Parameters.AddWithValue("@ProfilePic", profilePicPath);
                    }

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private string GenerateRandomOTP()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        private void SaveOtpToDatabase(string userId, string plainOtp)
        {
            byte[] saltBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider()) { rng.GetBytes(saltBytes); }
            byte[] hashBytes = ComputeSha256HashBytes(plainOtp, saltBytes);
            DateTime expiry = DateTime.UtcNow.AddMinutes(10);

            using (SqlConnection con = new SqlConnection(ConnString))
            {
                string query = "UPDATE tbl_login SET OtpCodeHash = @Hash, OtpSalt = @Salt, OtpExpiry = @Expiry, OtpAttemptCount = 0 WHERE User_Id = @UserId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@Hash", SqlDbType.VarBinary).Value = hashBytes;
                    cmd.Parameters.Add("@Salt", SqlDbType.VarBinary).Value = saltBytes;
                    cmd.Parameters.Add("@Expiry", SqlDbType.DateTime).Value = expiry;
                    cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = userId;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private bool ValidateOtpFromDatabase(string userId, string inputOtp)
        {
            using (SqlConnection con = new SqlConnection(ConnString))
            {
                string query = "SELECT OtpCodeHash, OtpSalt, OtpExpiry, OtpAttemptCount FROM tbl_login WHERE User_Id = @UserId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader["OtpExpiry"] == DBNull.Value || Convert.ToDateTime(reader["OtpExpiry"]) < DateTime.UtcNow)
                                return false;

                            int attemptCount = reader["OtpAttemptCount"] != DBNull.Value ? Convert.ToInt32(reader["OtpAttemptCount"]) : 0;
                            if (attemptCount >= 3) return false;

                            byte[] storedHash = reader["OtpCodeHash"] as byte[];
                            byte[] storedSalt = reader["OtpSalt"] as byte[];

                            if (storedHash == null || storedSalt == null) return false;

                            byte[] inputHashBytes = ComputeSha256HashBytes(inputOtp, storedSalt);

                            bool isValid = true;
                            if (storedHash.Length != inputHashBytes.Length)
                            {
                                isValid = false;
                            }
                            else
                            {
                                for (int i = 0; i < storedHash.Length; i++)
                                {
                                    if (storedHash[i] != inputHashBytes[i]) isValid = false;
                                }
                            }

                            if (isValid)
                            {
                                return true;
                            }
                            else
                            {
                                reader.Close();
                                using (SqlCommand incCmd = new SqlCommand("UPDATE tbl_login SET OtpAttemptCount = OtpAttemptCount + 1 WHERE User_Id = @UserId", con))
                                {
                                    incCmd.Parameters.AddWithValue("@UserId", userId);
                                    incCmd.ExecuteNonQuery();
                                }
                                return false;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private void ClearOtpInDatabase(string userId, SqlConnection con)
        {
            string query = "UPDATE tbl_login SET OtpCodeHash = NULL, OtpSalt = NULL, OtpExpiry = NULL, OtpAttemptCount = 0 WHERE User_Id = @UserId";
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.ExecuteNonQuery();
            }
        }

        private byte[] ComputeSha256HashBytes(string rawData, byte[] salt)
        {
            byte[] rawBytes = Encoding.UTF8.GetBytes(rawData);
            byte[] combinedBytes = new byte[rawBytes.Length + salt.Length];
            System.Buffer.BlockCopy(rawBytes, 0, combinedBytes, 0, rawBytes.Length);
            System.Buffer.BlockCopy(salt, 0, combinedBytes, rawBytes.Length, salt.Length);

            using (SHA256 sha256Hash = SHA256.Create())
            {
                return sha256Hash.ComputeHash(combinedBytes);
            }
        }

        private void SendEmailOTP(string toEmail, string otp)
        {
            try
            {
                // Secrets Management: Use CommunicationGateway (reads from Web.config), never hardcode credentials
                string body = $"Your One Time Password (OTP) for profile updates is: <b>{otp}</b>. This code expires in 10 minutes.";
                CommunicationGateway.SendCustomEmail(toEmail, "Project FLMX - Security Verification", body);
            }
            catch (Exception)
            {
                // Ponytail #3: Never expose raw exception details to client
                ShowMessage("Failed to send OTP email. Please try again.", "danger");
            }
        }

        private async System.Threading.Tasks.Task SendWhatsAppOTPAsync(string targetPhoneNumber, string otp)
        {
            try
            {
                // Force TLS 1.2 for the MSG91 API connection
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                string authKey = ConfigurationManager.AppSettings["Msg91AuthKey"];
                string integratedNumber = ConfigurationManager.AppSettings["Msg91WaNumber"]; // Ensure this matches your web.config key
                string templateName = ConfigurationManager.AppSettings["Msg91OtpTemplateId"];

                if (string.IsNullOrEmpty(authKey) || string.IsNullOrEmpty(integratedNumber) || string.IsNullOrEmpty(templateName))
                {
                    ShowMessage("MSG91 skipped: AuthKey, IntegratedNumber, or TemplateId missing in config.", "danger");
                    return;
                }

                string cleanPhone = targetPhoneNumber.Replace("+", "").Replace(" ", "").Trim();
                if (!cleanPhone.StartsWith("91")) cleanPhone = "91" + cleanPhone;

                string url = "https://api.msg91.com/api/v5/whatsapp/whatsapp-outbound-message/bulk/";

                // Standard MSG91 Authentication Template Payload (OTP in Body and Copy Code Button)
                string jsonPayload = $@"{{
                    ""integrated_number"": ""{integratedNumber}"",
                    ""content_type"": ""template"",
                    ""payload"": {{
                        ""messaging_product"": ""whatsapp"",
                        ""type"": ""template"",
                        ""template"": {{
                            ""name"": ""{templateName}"",
                            ""language"": {{
                                ""code"": ""en"",
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
                                            ""value"": ""{otp}""
                                        }},
                                        ""button_1"": {{
                                            ""subtype"": ""url"",
                                            ""type"": ""text"",
                                            ""value"": ""{otp}""
                                        }}
                                    }}
                                }}
                            ]
                        }}
                    }}
                }}";

                var content = new System.Net.Http.StringContent(jsonPayload, Encoding.UTF8, "application/json");

                using (var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, url))
                {
                    request.Headers.Add("authkey", authKey);
                    request.Content = content;

                    using (System.Net.Http.HttpResponseMessage response = await httpClient.SendAsync(request))
                    {
                        string result = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            // Surface the exact MSG91 API rejection reason to the UI
                        // Ponytail #3: Never expose raw API error details to client
                        ShowMessage("Failed to send WhatsApp OTP. Please try again.", "danger");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Ponytail #3: Never expose raw exception details to client
                ShowMessage("Failed to send WhatsApp OTP. Please try again.", "danger");
            }
        }

        private void WriteAuditLog(string userId, string changes)
        {
            try
            {
                string logDirectory = Server.MapPath("~/App_Data/AuditLogs/");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                string filePath = Path.Combine(logDirectory, $"Employee_{userId}_Log.txt");

                StringBuilder logEntry = new StringBuilder();
                logEntry.AppendLine("=========================================");
                logEntry.AppendLine($"TIMESTAMP : {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                logEntry.AppendLine($"ACTION BY : {userId} (Self-Update via Settings UI)");
                logEntry.AppendLine("CHANGES   :");
                logEntry.Append(changes);
                logEntry.AppendLine("=========================================\n");

                File.AppendAllText(filePath, logEntry.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to write audit log: " + ex.Message);
            }
        }

        private void ShowMessage(string message, string type)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = $"alert alert-{type}";
        }
    }
}