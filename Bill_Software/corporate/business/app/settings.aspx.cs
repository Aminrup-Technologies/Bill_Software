using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm2 : System.Web.UI.Page
    {
        private string ConnString => ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

        // Use properties to safely track state across postbacks
        private int CurrentUserId
        {
            get { return ViewState["CurrentUserId"] != null ? (int)ViewState["CurrentUserId"] : 0; }
            set { ViewState["CurrentUserId"] = value; }
        }

        private string CurrentUserEmail
        {
            get { return ViewState["CurrentUserEmail"] != null ? (string)ViewState["CurrentUserEmail"] : ""; }
            set { ViewState["CurrentUserEmail"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["USERID"] == null || Session["SessionToken"] == null)
            {
                Response.Redirect("~/index.aspx", false);
                return;
            }

            if (!IsPostBack)
            {
                DeterminePageState();
            }
        }

        private void DeterminePageState()
        {
            using (var cn = new SqlConnection(ConnString))
            {
                string sql = "SELECT Id, Name, Phone_no, Email, EmailVerified, MustChangePassword FROM tbl_login WHERE User_Id = @UserId";
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@UserId", Session["USERID"].ToString());
                    cn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            CurrentUserId = Convert.ToInt32(rdr["Id"]);
                            CurrentUserEmail = rdr["Email"].ToString();

                            bool emailVerified = rdr["EmailVerified"] != DBNull.Value && Convert.ToBoolean(rdr["EmailVerified"]);
                            bool mustChangePwd = rdr["MustChangePassword"] != DBNull.Value && Convert.ToBoolean(rdr["MustChangePassword"]);

                            // Route the user to the correct panel
                            if (!emailVerified)
                            {
                                ShowEmailVerificationPanel();
                            }
                            else if (mustChangePwd)
                            {
                                ShowPasswordChangePanel();
                            }
                            else
                            {
                                // All good! Show standard settings
                                ShowStandardSettings(rdr["Name"].ToString(), rdr["Phone_no"].ToString(), CurrentUserEmail);
                            }
                        }
                    }
                }
            }
        }

        #region Display Logic
        private void ShowEmailVerificationPanel()
        {
            PanelVerifyEmail.Visible = true;
            PanelChangePassword.Visible = false;
            PanelStandardSettings.Visible = false;
            lblVerifyEmailDisplay.Text = CurrentUserEmail;

            if (string.IsNullOrEmpty(CurrentUserEmail))
            {
                ShowError("No email address is associated with this account. Please contact an Administrator.");
                btnSendOtp.Enabled = false;
            }
        }

        private void ShowPasswordChangePanel()
        {
            PanelVerifyEmail.Visible = false;
            PanelChangePassword.Visible = true;
            PanelStandardSettings.Visible = false;
        }

        private void ShowStandardSettings(string name, string phone, string email)
        {
            PanelVerifyEmail.Visible = false;
            PanelChangePassword.Visible = false;
            PanelStandardSettings.Visible = true;

            lblName.Text = name;
            lblContactNo.Text = phone;
            lblEmailID.Text = email;
        }
        #endregion

        #region OTP Logic
        protected void btnSendOtp_Click(object sender, EventArgs e)
        {
            // Generate 6-digit OTP
            string otp = new Random().Next(100000, 999999).ToString();

            byte[] otpHash;
            byte[] otpSalt;
            CreateHash(otp, out otpHash, out otpSalt);

            DateTime expiry = DateTime.UtcNow.AddMinutes(15);

            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand("UPDATE tbl_login SET OtpCodeHash = @Hash, OtpSalt = @Salt, OtpExpiry = @Expiry, OtpAttemptCount = 0 WHERE Id = @Id", cn))
            {
                cmd.Parameters.AddWithValue("@Hash", otpHash);
                cmd.Parameters.AddWithValue("@Salt", otpSalt);
                cmd.Parameters.AddWithValue("@Expiry", expiry);
                cmd.Parameters.AddWithValue("@Id", CurrentUserId);
                cn.Open();
                cmd.ExecuteNonQuery();
            }

            if (SendOtpEmail(CurrentUserEmail, otp))
            {
                PanelSendOtp.Visible = false;
                PanelEnterOtp.Visible = true;
                ShowSuccess("OTP sent successfully to " + CurrentUserEmail);
            }
            else
            {
                ShowError("Failed to send OTP email. Please try again.");
            }
        }

        protected void btnVerifyOtp_Click(object sender, EventArgs e)
        {
            string enteredOtp = txtOtp.Text.Trim();
            if (string.IsNullOrEmpty(enteredOtp))
            {
                ShowError("Please enter the OTP.");
                return;
            }

            using (var cn = new SqlConnection(ConnString))
            {
                cn.Open();
                string sql = "SELECT OtpCodeHash, OtpSalt, OtpExpiry, OtpAttemptCount FROM tbl_login WHERE Id = @Id";

                byte[] dbHash = null;
                byte[] dbSalt = null;
                DateTime dbExpiry = DateTime.MinValue;
                int attempts = 0;

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@Id", CurrentUserId);
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            if (rdr["OtpCodeHash"] != DBNull.Value) dbHash = (byte[])rdr["OtpCodeHash"];
                            if (rdr["OtpSalt"] != DBNull.Value) dbSalt = (byte[])rdr["OtpSalt"];
                            if (rdr["OtpExpiry"] != DBNull.Value) dbExpiry = Convert.ToDateTime(rdr["OtpExpiry"]);
                            if (rdr["OtpAttemptCount"] != DBNull.Value) attempts = Convert.ToInt32(rdr["OtpAttemptCount"]);
                        }
                    }
                }

                if (dbHash == null || dbSalt == null)
                {
                    ShowError("No OTP was requested. Please click 'Send OTP'.");
                    return;
                }

                if (DateTime.UtcNow > dbExpiry)
                {
                    ShowError("OTP has expired. Please request a new one.");
                    return;
                }

                if (attempts >= 5)
                {
                    ShowError("Too many invalid attempts. Please request a new OTP.");
                    return;
                }

                if (VerifyHash(enteredOtp, dbHash, dbSalt))
                {
                    // Success! Mark email as verified and clear OTP data
                    using (var cmdUpd = new SqlCommand("UPDATE tbl_login SET EmailVerified = 1, OtpCodeHash = NULL, OtpSalt = NULL WHERE Id = @Id", cn))
                    {
                        cmdUpd.Parameters.AddWithValue("@Id", CurrentUserId);
                        cmdUpd.ExecuteNonQuery();
                    }
                    ShowSuccess("Email successfully verified!");

                    // Proceed to next step
                    DeterminePageState();
                }
                else
                {
                    // Increment attempt count
                    using (var cmdFail = new SqlCommand("UPDATE tbl_login SET OtpAttemptCount = OtpAttemptCount + 1 WHERE Id = @Id", cn))
                    {
                        cmdFail.Parameters.AddWithValue("@Id", CurrentUserId);
                        cmdFail.ExecuteNonQuery();
                    }
                    ShowError("Invalid OTP. Please try again.");
                }
            }
        }
        #endregion

        #region Password Change Logic
        protected void btnSavePassword_Click(object sender, EventArgs e)
        {
            string newPass = txtNewPass.Text.Trim();
            string confirmPass = txtConfirmPass.Text.Trim();

            if (newPass.Length < 6)
            {
                ShowError("Password must be at least 6 characters long.");
                return;
            }
            if (newPass != confirmPass)
            {
                ShowError("Passwords do not match.");
                return;
            }

            byte[] pwdHash;
            byte[] pwdSalt;
            CreateHash(newPass, out pwdHash, out pwdSalt);

            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand("UPDATE tbl_login SET PasswordHash = @Hash, PasswordSalt = @Salt, MustChangePassword = 0, Password = NULL WHERE Id = @Id", cn))
            {
                cmd.Parameters.AddWithValue("@Hash", pwdHash);
                cmd.Parameters.AddWithValue("@Salt", pwdSalt);
                cmd.Parameters.AddWithValue("@Id", CurrentUserId);

                cn.Open();
                cmd.ExecuteNonQuery();
            }

            ShowSuccess("Password saved successfully! Account setup is complete.");
            DeterminePageState(); // Will route them to the standard settings view
        }
        #endregion

        #region Helpers & Security
        private void ShowSuccess(string msg)
        {
            PanelMsg.Visible = true;
            lblMsg.Text = $"<div class='alert-success'>{msg}</div>";
        }

        private void ShowError(string msg)
        {
            PanelMsg.Visible = true;
            lblMsg.Text = $"<div class='alert-danger'>{msg}</div>";
        }

        private void CreateHash(string plainText, out byte[] hash, out byte[] salt)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                salt = new byte[16];
                rng.GetBytes(salt);
            }
            using (var derive = new Rfc2898DeriveBytes(plainText, salt, 100000))
            {
                hash = derive.GetBytes(32);
            }
        }

        private bool VerifyHash(string plainText, byte[] storedHash, byte[] storedSalt)
        {
            using (var derive = new Rfc2898DeriveBytes(plainText, storedSalt, 100000))
            {
                byte[] computed = derive.GetBytes(32);
                if (computed.Length != storedHash.Length) return false;
                int diff = 0;
                for (int i = 0; i < computed.Length; i++) diff |= computed[i] ^ storedHash[i];
                return diff == 0;
            }
        }

        private bool SendOtpEmail(string toEmail, string otp)
        {
            try
            {
                string fromApp = ConfigurationManager.AppSettings["SmtpFrom"] ?? "Flame-Ex ERP";
                string smtpUserApp = ConfigurationManager.AppSettings["SmtpUser"];
                string smtpPassApp = ConfigurationManager.AppSettings["SmtpPass"];
                string smtpHostApp = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.zoho.in";

                int smtpPortApp = 587;
                int p;
                if (int.TryParse(ConfigurationManager.AppSettings["SmtpPort"], out p)) smtpPortApp = p;

                bool smtpEnableSsl = true;
                bool s;
                if (bool.TryParse(ConfigurationManager.AppSettings["SmtpEnableSsl"], out s)) smtpEnableSsl = s;

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(smtpUserApp, fromApp);
                    message.To.Add(toEmail);
                    message.Subject = "Your Flame-Ex ERP Verification Code";
                    message.IsBodyHtml = true;
                    message.Body = $"<div style='font-family:Arial; padding:20px; border:1px solid #ccc; max-width:500px;'>" +
                                   $"<h2 style='color:#006699;'>Email Verification</h2>" +
                                   $"<p>Use the following 6-digit code to verify your email address:</p>" +
                                   $"<h1 style='letter-spacing:5px; color:#d9534f; background:#f9f9f9; padding:10px; text-align:center;'>{otp}</h1>" +
                                   $"<p>This code will expire in 15 minutes.</p></div>";

                    using (var smtp = new SmtpClient(smtpHostApp, smtpPortApp))
                    {
                        smtp.EnableSsl = smtpEnableSsl;
                        smtp.Credentials = new NetworkCredential(smtpUserApp, smtpPassApp);

                        try { System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12; } catch { }

                        smtp.Send(message);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                // In production, log the exact exception message
                return false;
            }
        }
        #endregion
    }
}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.UI;
//using System.Web.UI.WebControls;
//using System.Data.SqlClient;

//namespace Bill_Software.corporate.business.app
//{
//    public partial class WebForm2 : System.Web.UI.Page
//    {
//        DB_UTILITY DbCL = new DB_UTILITY();
//        protected void Page_Load(object sender, EventArgs e)
//        {
//            if (HttpContext.Current.Session["USERID"] == null)
//            {
//                Response.Redirect("~/index.aspx");
//            }
//            if (!IsPostBack)
//            {
//                Binddata();

//            }
//        }
//        private void Binddata()
//        {
//            string cmdstring = "select Name,Phone_no,Email FROM tbl_login where User_Id='" + Session["USERID"].ToString() + "'";
//            DbCL.Sqlconnection();
//            DbCL.ConnectDb();
//            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
//            SqlDataReader re = cmd.ExecuteReader();
//            if (re.Read())
//            {
//                lblName.Text = re["Name"].ToString();
//                lblContactNo.Text = re["Phone_no"].ToString();
//                lblEmailID.Text = re["Email"].ToString();
//            }
//            DbCL.Conn.Close();

//        }
//    }
//}