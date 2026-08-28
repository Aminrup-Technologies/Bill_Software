using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using System.Configuration;
using System.Linq;

namespace Bill_Software.Update
{
    public partial class password : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        // OTP settings
        private readonly int OtpExpiryMinutes = 10;
        private readonly int OtpLength = 6;
        private readonly int MaxOtpAttempts = 5;
        private readonly int Pbkdf2Iterations = 100000; // tune for your server

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["USERID"] == null)
            {
                // Not logged in, close popup and send to login
                ClientScript.RegisterStartupScript(this.GetType(), "close", "window.close();", true);
                return;
            }

            if (!IsPostBack)
            {
                LoadUserEmailAndPanels();
            }
        }

        private void LoadUserEmailAndPanels()
        {
            string userId = Session["USERID"].ToString();

            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            string sql = "SELECT Email FROM tbl_login WHERE User_Id = @UserId";
            using (SqlCommand cmd = new SqlCommand(sql, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);

                object obj = cmd.ExecuteScalar();
                string email = obj != DBNull.Value && obj != null ? obj.ToString() : string.Empty;

                if (string.IsNullOrEmpty(email))
                {
                    // Ask for email first
                    pnlEmailEntry.Visible = true;
                    pnlSendOtp.Visible = false;
                    pnlVerifyOtp.Visible = false;
                    pnlChangePassword.Visible = false;
                }
                else
                {
                    // Email exists: show send-otp panel
                    lblUserEmail.Text = email;
                    pnlEmailEntry.Visible = false;
                    pnlSendOtp.Visible = true;
                    pnlVerifyOtp.Visible = false;
                    pnlChangePassword.Visible = false;
                }
            }

            DbCL.DisconnectDb();
        }

        protected void btnSaveEmail_Click(object sender, EventArgs e)
        {
            string userId = Session["USERID"].ToString();
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrEmpty(email))
            {
                ShowError("Please enter a valid email.");
                return;
            }

            // Save email to DB
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            string sqlUpdate = "UPDATE tbl_login SET Email = @Email, EmailVerified = 0 WHERE User_Id = @UserId";
            using (SqlCommand cmd = new SqlCommand(sqlUpdate, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.ExecuteNonQuery();
            }

            DbCL.DisconnectDb();

            // move to send-otp panel and automatically send otp
            lblUserEmail.Text = email;
            pnlEmailEntry.Visible = false;
            pnlSendOtp.Visible = true;

            // Send OTP immediately
            SendOtpToUserEmail(userId, email);
        }

        protected void btnSendOtp_Click(object sender, EventArgs e)
        {
            string userId = Session["USERID"].ToString();
            string email = lblUserEmail.Text.Trim();
            SendOtpToUserEmail(userId, email);
        }

        private void SendOtpToUserEmail(string userId, string email)
        {
            try
            {
                string otp = GenerateNumericOtp(OtpLength);

                // Create salt and hash OTP
                byte[] salt = new byte[16];
                using (var rng = new RNGCryptoServiceProvider())
                    rng.GetBytes(salt);

                byte[] otpHash = HashWithSalt(otp, salt);

                DateTime expiry = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);

                // Store hash, salt, expiry, reset attempts in DB
                DbCL.Sqlconnection();
                DbCL.ConnectDb();

                string sql = @"UPDATE tbl_login
                               SET OtpCodeHash = @OtpHash, OtpSalt = @OtpSalt, OtpExpiry = @OtpExpiry, OtpAttemptCount = 0
                               WHERE User_Id = @UserId";
                using (SqlCommand cmd = new SqlCommand(sql, DbCL.Conn))
                {
                    cmd.Parameters.AddWithValue("@OtpHash", otpHash);
                    cmd.Parameters.AddWithValue("@OtpSalt", salt);
                    cmd.Parameters.AddWithValue("@OtpExpiry", expiry);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.ExecuteNonQuery();
                }

                DbCL.DisconnectDb();

                // Send OTP by email
                SendOtpEmail(email, otp);

                // Show verify panel
                pnlSendOtp.Visible = false;
                pnlVerifyOtp.Visible = true;
                pnlChangePassword.Visible = false;
                PanelOk.Visible = true;
                PanelError.Visible = false;
                LabelOk.Text = "OTP sent to your email. Please check and enter the OTP.";
            }
            catch (Exception ex)
            {
                ShowError("Failed to send OTP. Please contact admin.");
                // log ex if you have logger
            }
        }

        protected void btnVerifyOtp_Click(object sender, EventArgs e)
        {
            string userId = Session["USERID"].ToString();
            string enteredOtp = txtOtp.Text.Trim();

            if (string.IsNullOrEmpty(enteredOtp))
            {
                ShowError("Please enter the OTP.");
                return;
            }

            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            string sql = "SELECT OtpCodeHash, OtpSalt, OtpExpiry, OtpAttemptCount, Email FROM tbl_login WHERE User_Id = @UserId";
            using (SqlCommand cmd = new SqlCommand(sql, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (!dr.Read())
                    {
                        ShowError("User not found.");
                        DbCL.DisconnectDb();
                        return;
                    }

                    if (dr["OtpCodeHash"] == DBNull.Value || dr["OtpSalt"] == DBNull.Value || dr["OtpExpiry"] == DBNull.Value)
                    {
                        ShowError("No OTP found. Please request a new OTP.");
                        DbCL.DisconnectDb();
                        return;
                    }

                    byte[] otpHash = (byte[])dr["OtpCodeHash"];
                    byte[] otpSalt = (byte[])dr["OtpSalt"];
                    DateTime expiry = Convert.ToDateTime(dr["OtpExpiry"]);
                    int attempts = dr["OtpAttemptCount"] != DBNull.Value ? Convert.ToInt32(dr["OtpAttemptCount"]) : 0;
                    string email = dr["Email"] != DBNull.Value ? dr["Email"].ToString() : "";

                    if (DateTime.UtcNow > expiry)
                    {
                        ShowError("OTP expired. Please request a new OTP.");
                        DbCL.DisconnectDb();
                        return;
                    }

                    if (attempts >= MaxOtpAttempts)
                    {
                        ShowError("Maximum OTP attempts exceeded. Please request a new OTP.");
                        DbCL.DisconnectDb();
                        return;
                    }

                    // Compute hash of entered OTP with stored salt and compare
                    byte[] enteredHash = HashWithSalt(enteredOtp, otpSalt);

                    bool ok = AreByteArraysEqual(enteredHash, otpHash);

                    if (!ok)
                    {
                        // increment attempt count
                        DbCL.DisconnectDb();
                        DbCL.ConnectDb();
                        string up = "UPDATE tbl_login SET OtpAttemptCount = OtpAttemptCount + 1 WHERE User_Id = @UserId";
                        using (SqlCommand cmd2 = new SqlCommand(up, DbCL.Conn))
                        {
                            cmd2.Parameters.AddWithValue("@UserId", userId);
                            cmd2.ExecuteNonQuery();
                        }
                        DbCL.DisconnectDb();

                        ShowError("Invalid OTP. Please try again.");
                        return;
                    }

                    // OTP verified — mark EmailVerified = 1
                    DbCL.DisconnectDb();
                    DbCL.ConnectDb();
                    string upd = "UPDATE tbl_login SET EmailVerified = 1, OtpCodeHash = NULL, OtpSalt = NULL, OtpExpiry = NULL, OtpAttemptCount = 0 WHERE User_Id = @UserId";
                    using (SqlCommand cmd3 = new SqlCommand(upd, DbCL.Conn))
                    {
                        cmd3.Parameters.AddWithValue("@UserId", userId);
                        cmd3.ExecuteNonQuery();
                    }
                    DbCL.DisconnectDb();

                    // Allow password change
                    pnlVerifyOtp.Visible = false;
                    pnlChangePassword.Visible = true;
                    PanelOk.Visible = true;
                    PanelError.Visible = false;
                    LabelOk.Text = "Email verified. Now enter your current and new password to update.";
                }
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            string userId = Session["USERID"].ToString();
            string currentPwd = txtCrntPassword.Text.Trim();
            string newPwd = txtNewPassword.Text.Trim();
            string confirmPwd = txtConfNewPassword.Text.Trim();

            if (newPwd != confirmPwd)
            {
                ShowError("New password and confirmation do not match.");
                return;
            }

            // Verify current password
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            string sqlGet = "SELECT Password, PasswordHash, PasswordSalt FROM tbl_login WHERE User_Id = @UserId";
            using (SqlCommand cmd = new SqlCommand(sqlGet, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (!dr.Read())
                    {
                        ShowError("User not found.");
                        DbCL.DisconnectDb();
                        return;
                    }

                    // Try to validate using stored hash+salt if present
                    bool ok = false;
                    if (dr["PasswordHash"] != DBNull.Value && dr["PasswordSalt"] != DBNull.Value)
                    {
                        byte[] storedHash = (byte[])dr["PasswordHash"];
                        byte[] storedSalt = (byte[])dr["PasswordSalt"];

                        ok = VerifyPasswordPBKDF2(currentPwd, storedHash, storedSalt, Pbkdf2Iterations);
                    }

                    // Fallback to legacy plaintext password column (if present)
                    if (!ok && dr["Password"] != DBNull.Value)
                    {
                        string storedPlain = dr["Password"].ToString();
                        if (!string.IsNullOrEmpty(storedPlain) && storedPlain == currentPwd)
                            ok = true;
                    }

                    if (!ok)
                    {
                        DbCL.DisconnectDb();
                        ShowError("Current password is incorrect.");
                        return;
                    }
                }
            }

            // Hash new password with salt and store (prefer PasswordHash+PasswordSalt)
            byte[] newSalt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(newSalt);

            byte[] newHash;
            using (var derive = new Rfc2898DeriveBytes(newPwd, newSalt, Pbkdf2Iterations))
            {
                newHash = derive.GetBytes(32); // 256-bit
            }

            string sqlUpdate = @"UPDATE tbl_login
                                 SET PasswordHash = @Hash, PasswordSalt = @Salt, Password = @PlainFallback,
                                     MustChangePassword = 0, EmailVerified = 1
                                 WHERE User_Id = @UserId";
            using (SqlCommand cmd2 = new SqlCommand(sqlUpdate, DbCL.Conn))
            {
                cmd2.Parameters.AddWithValue("@Hash", newHash);
                cmd2.Parameters.AddWithValue("@Salt", newSalt);
                // optional: update legacy Password column as fallback (you may remove later)
                cmd2.Parameters.AddWithValue("@PlainFallback", newPwd);
                cmd2.Parameters.AddWithValue("@UserId", userId);
                cmd2.ExecuteNonQuery();
            }

            DbCL.DisconnectDb();

            PanelOk.Visible = true;
            PanelError.Visible = false;
            LabelOk.Text = "Password changed successfully.";

            // Optionally close popup and refresh opener (Settings.aspx) — emulate existing close behaviour
            ClientScript.RegisterStartupScript(this.GetType(), "closeAndRefresh", "if (opener && !opener.closed) opener.location = '/corporate/business/app/Setting.aspx'; window.close();", true);
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtCrntPassword.Text = string.Empty;
            txtNewPassword.Text = string.Empty;
            txtConfNewPassword.Text = string.Empty;
        }

        #region Helpers

        private string GenerateNumericOtp(int length)
        {
            var digits = new char[length];
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] buffer = new byte[length];
                rng.GetBytes(buffer);
                for (int i = 0; i < length; i++)
                {
                    digits[i] = (char)('0' + (buffer[i] % 10));
                }
            }
            return new string(digits);
        }

        private byte[] HashWithSalt(string text, byte[] salt)
        {
            using (var sha = SHA256.Create())
            {
                byte[] plain = Encoding.UTF8.GetBytes(text);
                byte[] combined = salt.Concat(plain).ToArray(); // requires using System.Linq

                return sha.ComputeHash(combined);
            }
        }

        private bool AreByteArraysEqual(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }

        private void SendOtpEmail(string toEmail, string otp)
        {
            // Uses SMTP settings in web.config under <system.net><mailSettings>
            string fromAddr = ConfigurationManager.AppSettings["FromEmail"] ?? "it.support@aminruptechnologies.co.in";
            var mail = new MailMessage();
            mail.To.Add(toEmail);
            mail.From = new MailAddress(fromAddr);
            mail.Subject = "Your OTP for account verification";
            mail.Body = $"Your OTP is: {otp}. It will expire in {OtpExpiryMinutes} minutes.";

            using (var smtp = new SmtpClient())
            {
                smtp.EnableSsl = true; // ensure your web.config matches the provider
                smtp.Send(mail);
            }
        }

        private void ShowError(string message)
        {
            PanelError.Visible = true;
            PanelOk.Visible = false;
            lblErrorMsg.Text = message;
        }

        private void ShowInfo(string message)
        {
            PanelError.Visible = false;
            PanelOk.Visible = true;
            LabelOk.Text = message;
        }

        // PBKDF2 verify (compatible with older .NET frameworks)
        private bool VerifyPasswordPBKDF2(string password, byte[] storedHash, byte[] storedSalt, int iterations)
        {
            using (var derive = new Rfc2898DeriveBytes(password, storedSalt, iterations))
            {
                var computed = derive.GetBytes(storedHash.Length);
                return AreByteArraysEqual(computed, storedHash);
            }
        }

        #endregion

        //-----------OLD Below-------------------------------------//

        protected void btnUpdate_Click_OLD(object sender, EventArgs e)
        {
            if (Session["USERID"].ToString() == "admin")
            {
                AuthenticateADMIN();
            }
        }

        private void AuthenticateADMIN()
        {

            string cmdString = "select Password from tbl_card_login where User_Id='" + Session["USERID"].ToString() + "' and Password='" + txtCrntPassword.Text.Trim() + "'";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdString, DbCL.Conn);
            SqlDataReader Rdr;
            Rdr = cmd.ExecuteReader();
            if (Rdr.Read())
            {
                UpdatePassword();
            }
            else
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "Your current Password doesn't match with the Password you have entered.";
                btnReset.Visible = true;
                btnUpdate.Visible = false;
            }
            DbCL.Conn.Close();
        }

        private void UpdatePassword()
        {
            if (Session["USERID"].ToString() == "admin")
            {

                DbCL.executeRdr("UPDATE tbl_card_login SET Password='" + txtConfNewPassword.Text.Trim() + "' WHERE User_Id='" + Session["USERID"].ToString() + "'");
                PanelOk.Visible = true;
                LabelOk.Text = "Password changed successfully.";
                btnUpdate.Visible = false;


            }
        }

        protected void btnReset_Click_OLD(object sender, EventArgs e)
        {
            Response.Redirect("~/admin/Update/password.aspx");
        }
    }
}