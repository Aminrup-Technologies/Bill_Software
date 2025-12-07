using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm80 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dtuse = new DataTable();
        private string ConnString => ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString; // update name

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadEmployeeDropdown();
                BindGrid();
            }
        }

        private void LoadEmployeeDropdown()
        {
            // fill ddlEmpId if you need filtering by employee/user
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand("SELECT User_Id FROM tbl_login where IsActive = 1 ORDER BY Id", cn))
            {
                var dt = new DataTable();
                var da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                ddlEmpId.Items.Clear();
                ddlEmpId.Items.Add(new System.Web.UI.WebControls.ListItem("-- All --", ""));
                foreach (DataRow r in dt.Rows)
                {
                    ddlEmpId.Items.Add(new ListItem(r["User_Id"].ToString(), r["User_Id"].ToString()));
                }
            }
        }

        protected void ddlEmpId_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGrid();
        }

        protected void btnRefresh_Click(object sender, EventArgs e) => BindGrid();

        private void BindGrid()
        {
            var sql = @"SELECT Id, User_Id, Name, Email, Phone_no, IsActive, LockoutEnd, LastLogin, CreatedAt
                    FROM dbo.tbl_login
                    WHERE (@UserId = '' OR User_Id = @UserId)
                    AND (User_Id NOT IN ('admin', 'AT01') AND IsActive = 1)
                    ORDER BY Id";

            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@UserId", ddlEmpId.SelectedValue ?? string.Empty);
                var dt = new DataTable();
                var da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                gvUsers.DataSource = dt;
                gvUsers.DataBind();
            }
        }

        protected void gvUsers_RowDataBound_OLD(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            DataRowView drv = (DataRowView)e.Row.DataItem;

            // ----- Active / Inactive Row Color -----
            bool isActive = false;
            if (!drv.Row.IsNull("IsActive"))
                isActive = Convert.ToBoolean(drv["IsActive"]);
            e.Row.CssClass = isActive ? "grid-active" : "grid-inactive";

            // ----- Lock / Unlock Button -----
            LinkButton lnkLock = e.Row.FindControl("lnkLock") as LinkButton;
            if (lnkLock != null)
            {
                bool isLocked = false;

                if (!drv.Row.IsNull("LockoutEnd"))
                {
                    object val = drv["LockoutEnd"];

                    // If DB type is datetimeoffset, the value will be DateTimeOffset
                    DateTimeOffset lockoutEnd;
                    if (val is DateTimeOffset)
                    {
                        lockoutEnd = (DateTimeOffset)val;
                    }
                    else
                    {
                        // Fallback if the provider returned DateTime
                        lockoutEnd = new DateTimeOffset(Convert.ToDateTime(val));
                    }

                    // Consider account locked only if LockoutEnd is in the future
                    isLocked = lockoutEnd > DateTimeOffset.UtcNow;
                }

                lnkLock.Text = isLocked ? "Unlock" : "Lock";
            }

            // ----- Activate / Deactivate -----
            LinkButton lnkToggle = e.Row.FindControl("lnkToggleActive") as LinkButton;
            if (lnkToggle != null)
            {
                lnkToggle.Text = isActive ? "Deactivate" : "Activate";
            }
        }

        protected void gvUsers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            DataRowView drv = (DataRowView)e.Row.DataItem;

            bool isActive = false;
            if (!drv.Row.IsNull("IsActive"))
                isActive = Convert.ToBoolean(drv["IsActive"]);

            bool isLocked = false;
            if (!drv.Row.IsNull("LockoutEnd"))
            {
                var val = drv["LockoutEnd"];
                if (val is DateTimeOffset)
                    isLocked = ((DateTimeOffset)val) > DateTimeOffset.UtcNow;
                else
                    isLocked = Convert.ToDateTime(val) > DateTime.UtcNow;
            }

            // === Activate/Deactivate button ===
            LinkButton lnkToggle = e.Row.FindControl("lnkToggleActive") as LinkButton;
            if (lnkToggle != null)
            {
                if (isActive)
                {
                    lnkToggle.Text = "Deactivate";
                    lnkToggle.CssClass = "action-btn btn-deactivate";
                }
                else
                {
                    lnkToggle.Text = "Activate";
                    lnkToggle.CssClass = "action-btn btn-activate";
                }
            }

            // === Lock/Unlock button ===
            LinkButton lnkLock = e.Row.FindControl("lnkLock") as LinkButton;
            if (lnkLock != null)
            {
                if (isLocked)
                {
                    lnkLock.Text = "Unlock";
                    lnkLock.CssClass = "action-btn btn-unlock";
                }
                else
                {
                    lnkLock.Text = "Lock";
                    lnkLock.CssClass = "action-btn btn-lock";
                }
            }
        }


        protected void gvUsers_RowCommand(object sender, System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            switch (e.CommandName)
            {
                case "ToggleActive":
                    ToggleActive(id);
                    break;
                case "ResetPassword":
                    ResetPassword(id);
                    break;
                case "DeleteUser":
                    DeleteUser(id);
                    break;
                case "ToggleLock":
                    ToggleLock(id);
                    break;
                case "MenuEdit":
                    string userId = GetUserIdById(id);
                    Response.Redirect("~/corporate/business/app/Update_Designation.aspx?User_Id=" + userId, false);
                    break;
            }
            BindGrid();
        }

        private string GetUserIdById(int id)
        {
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand("SELECT User_Id FROM dbo.tbl_login WHERE Id = @Id", cn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cn.Open();
                var obj = cmd.ExecuteScalar();
                return obj == null ? "" : obj.ToString();
            }
        }

        private void ToggleActive(int id)
        {
            // flip IsActive bit
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand("UPDATE dbo.tbl_login SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END WHERE Id = @Id", cn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cn.Open();
                cmd.ExecuteNonQuery();
                ShowOk("User active status updated.");
            }
        }

        private void ToggleLock(int id)
        {
            const string sqlSelect = "SELECT LockoutEnd FROM dbo.tbl_login WHERE Id = @Id";
            using (SqlConnection cn = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(sqlSelect, cn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                cn.Open();
                object obj = cmd.ExecuteScalar();

                bool currentlyLocked = false;
                if (obj != null && obj != DBNull.Value)
                {
                    if (obj is DateTimeOffset)
                    {
                        currentlyLocked = ((DateTimeOffset)obj) > DateTimeOffset.Now;
                    }
                    else
                    {
                        // fallback if stored as DateTime
                        currentlyLocked = Convert.ToDateTime(obj) > DateTime.UtcNow;
                    }
                }

                if (currentlyLocked)
                {
                    // unlock: set LockoutEnd = NULL and reset FailedAccessCount
                    using (var upd = new SqlCommand("UPDATE dbo.tbl_login SET LockoutEnd = NULL, FailedAccessCount = 0 WHERE Id = @Id", cn))
                    {
                        upd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                        upd.ExecuteNonQuery();
                    }
                    ShowOk("User unlocked.");
                }
                else
                {
                    // lock: set LockoutEnd to far future (or choose policy)
                    DateTimeOffset lockUntil = DateTimeOffset.UtcNow.AddYears(100);

                    using (var upd = new SqlCommand("UPDATE dbo.tbl_login SET LockoutEnd = @LockoutEnd WHERE Id = @Id", cn))
                    {
                        var p = upd.Parameters.Add("@LockoutEnd", SqlDbType.DateTimeOffset);
                        p.Value = lockUntil;
                        upd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                        upd.ExecuteNonQuery();
                    }
                    ShowOk("User locked.");
                }
            }
        }


        private void DeleteUser(int id)
        {
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand("DELETE FROM dbo.tbl_login WHERE Id = @Id", cn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cn.Open();
                int rows = cmd.ExecuteNonQuery();
                if (rows > 0) ShowOk("User deleted.");
                else ShowError("User not found.");
            }
        }

        public class UserRecord
        {
            public string UserId { get; set; }
            public string Email { get; set; }
        }

        private void ResetPassword(int id)
        {
            // Generate a temp password and store its hash+sault. Force MustChangePassword = 1
            string tempPassword = GenerateTempPassword(10);
            byte[] salt = GenerateSalt(16);
            byte[] hash = HashPassword(tempPassword, salt);

            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(@"UPDATE dbo.tbl_login 
                                         SET PasswordHash = @Hash, PasswordSalt = @Salt, MustChangePassword = 1
                                         WHERE Id = @Id", cn))
            {
                cmd.Parameters.AddWithValue("@Hash", hash);
                cmd.Parameters.AddWithValue("@Salt", salt);
                cmd.Parameters.AddWithValue("@Id", id);
                cn.Open();
                cmd.ExecuteNonQuery();
            }

            // Optionally: send temp password to user's email - implement SendTempPasswordEmail
            string userId, email;
            GetUserRecordById(id, out userId, out email);

            if (!string.IsNullOrEmpty(email))
            {
                try
                {
                    SendTempPasswordEmail(email, userId, tempPassword);
                    ShowOk("Password reset; temp password emailed.");
                }
                catch
                {
                    ShowOk("Password reset, but failed to send email.");
                }
            }
            else
            {
                ShowOk("Password reset. User has no email.");
            }
        }

        private void GetUserRecordById(int id, out string userId, out string email)
        {
            userId = string.Empty;
            email = string.Empty;

            const string sql = "SELECT User_Id, Email FROM dbo.tbl_login WHERE Id = @Id";

            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                cn.Open();
                using (var rdr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (rdr.Read())
                    {
                        int idxUser = rdr.GetOrdinal("User_Id");
                        int idxEmail = rdr.GetOrdinal("Email");

                        userId = rdr.IsDBNull(idxUser) ? string.Empty : rdr.GetString(idxUser);
                        email = rdr.IsDBNull(idxEmail) ? string.Empty : rdr.GetString(idxEmail);
                    }
                }
            }
        }



        #region Helpers - UI
        private void ShowOk(string msg)
        {
            PanelOK.Visible = true;
            PanelError.Visible = false;
            lblOk.Text = msg;
        }
        private void ShowError(string msg)
        {
            PanelOK.Visible = false;
            PanelError.Visible = true;
            lblErrorMsg.Text = msg;
        }
        #endregion

        #region Password helpers (PBKDF2)
        private static byte[] GenerateSalt(int size = 16)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var salt = new byte[size];
                rng.GetBytes(salt);
                return salt;
            }
        }

        private static byte[] HashPassword(string password, byte[] salt, int iterations = 100000, int hashBytes = 32)
        {
            // .NET 4.5.2 uses PBKDF2-SHA1 only
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                return pbkdf2.GetBytes(hashBytes);
            }
        }


        private static string GenerateTempPassword(int length = 10)
        {
            const string valid = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var res = new StringBuilder();
            using (var rng = new RNGCryptoServiceProvider())
            {
                var uintBuffer = new byte[4];
                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res.Append(valid[(int)(num % (uint)valid.Length)]);
                }
            }
            return res.ToString();
        }
        #endregion

        private void SendTempPasswordEmail(string toEmail, string userId, string tempPassword)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                ShowError("Cannot send temp password: user has no email.");
                return;
            }

            // Read SMTP settings from web.config (fall back to defaults)
            string fromApp = ConfigurationManager.AppSettings["SmtpFrom"] ?? "Flame-Ex ERP Mailer | Aminrup Technologies";
            string smtpUserApp = ConfigurationManager.AppSettings["SmtpUser"] ?? "it.support@aminruptechnologies.co.in";
            string smtpPassApp = ConfigurationManager.AppSettings["SmtpPass"] ?? "TPw800QrVMU2";
            string smtpHostApp = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.zoho.in";

            int smtpPortApp;
            if (!int.TryParse(ConfigurationManager.AppSettings["SmtpPort"], out smtpPortApp))
                smtpPortApp = 587;

            bool smtpEnableSsl;
            if (!bool.TryParse(ConfigurationManager.AppSettings["SmtpEnableSsl"], out smtpEnableSsl))
                smtpEnableSsl = true;

            string fromAddress = !string.IsNullOrWhiteSpace(fromApp) ? fromApp : smtpUserApp;
            string subject = "Temporary password for your account";

            // Build bodies — do NOT log tempPassword anywhere
            string plainTextBody = string.Format(
                "Hello {0},\r\n\r\n" +
                "A temporary password has been generated for your account. Use the credentials below to sign in and you will be required to change your password on first login.\r\n\r\n" +
                "User Id: {1}\r\n" +
                "Temporary Password: {2}\r\n\r\n" +
                "For security, this temporary password will expire in 30 minutes. If you did not request this change, please contact support immediately.\r\n\r\n" +
                "--\r\nThis is an automated message. Do not reply.", userId, userId, tempPassword);

            string htmlBody = string.Format(
                "<html><body>" +
                "<p>Hello <strong>{0}</strong>,</p>" +
                "<p>A temporary password has been generated for your account. Use the credentials below to sign in and you will be required to change your password on first login.</p>" +
                "<p><strong>User Id:</strong> {1}<br/><strong>Temporary Password:</strong> {2}</p>" +
                "<p>This temporary password will expire in <strong>30 minutes</strong>. If you did not request this change, please contact support immediately.</p>" +
                "<hr/>" +
                "<p style='font-size:11px;color:#666'>This is an automated message. Please do not reply.</p>" +
                "</body></html>", userId, userId, tempPassword);

            // Retry settings
            int maxAttempts = 3;
            int attempt = 0;
            int baseDelayMs = 1000;

            // Ensure TLS1.2 where possible
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            }
            catch { /* ignore if runtime doesn't support */ }

            Exception lastEx = null;
            while (attempt < maxAttempts)
            {
                attempt++;
                try
                {
                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress(fromAddress);
                        message.To.Add(toEmail);
                        message.Subject = subject;
                        message.SubjectEncoding = Encoding.UTF8;

                        // Keep main Body as plain text (safe default)
                        message.Body = plainTextBody;
                        message.BodyEncoding = Encoding.UTF8;
                        message.IsBodyHtml = false;

                        // Add alternate views (plain + html)
                        var plainView = AlternateView.CreateAlternateViewFromString(plainTextBody, Encoding.UTF8, MediaTypeNames.Text.Plain);
                        var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, MediaTypeNames.Text.Html);
                        plainView.TransferEncoding = TransferEncoding.QuotedPrintable;
                        htmlView.TransferEncoding = TransferEncoding.QuotedPrintable;
                        message.AlternateViews.Add(plainView);
                        message.AlternateViews.Add(htmlView);

                        using (var smtp = new SmtpClient())
                        {
                            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                            smtp.Host = smtpHostApp;
                            smtp.Port = smtpPortApp;
                            smtp.EnableSsl = smtpEnableSsl;
                            smtp.Timeout = 20000;
                            smtp.UseDefaultCredentials = false;
                            smtp.Credentials = new NetworkCredential(smtpUserApp, smtpPassApp);

                            smtp.Send(message);
                        }
                    }

                    // success
                    ShowOk("Temporary password emailed to user.");
                    return;
                }
                catch (SmtpFailedRecipientsException frex)
                {
                    // Permanent failure for recipient -> don't retry
                    lastEx = frex;
                    ShowError("Failed to send email to recipient: " + frex.Message);
                    break;
                }
                catch (SmtpException sex)
                {
                    lastEx = sex;
                    // non-recoverable hint: stop immediately for certain status codes
                    if (sex.StatusCode == SmtpStatusCode.MustIssueStartTlsFirst ||
                        sex.StatusCode == SmtpStatusCode.ClientNotPermitted ||
                        sex.StatusCode == SmtpStatusCode.GeneralFailure)
                    {
                        ShowError(string.Format("SMTP error: {0} - {1}", sex.StatusCode, sex.Message));
                        break;
                    }
                    // otherwise continue to retry
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                }

                if (attempt < maxAttempts)
                {
                    try { System.Threading.Thread.Sleep(baseDelayMs * (int)Math.Pow(2, attempt - 1)); } catch { }
                }
            }

            // If we reached here, sending failed
            if (lastEx != null)
            {
                // Do not include sensitive details (like the tempPassword) in errors or logs.
                ShowError("Failed to email temporary password. " + lastEx.Message);
            }
            else
            {
                ShowError("Failed to email temporary password.");
            }
        }

    }
}