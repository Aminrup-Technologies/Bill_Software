using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Net.Mail;
using System.IO;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public static class CompanyContext
    {
        public static int CurrentCompanyID
        {
            get
            {
                if (HttpContext.Current.Session["CompanyID"] != null)
                {
                    return Convert.ToInt32(HttpContext.Current.Session["CompanyID"]);
                }
                return 0;
            }
        }
        public static string CurrentCompanyCode
        {
            get
            {
                return HttpContext.Current.Session["CompanyCode"] != null
                    ? HttpContext.Current.Session["CompanyCode"].ToString()
                    : "FE"; // Fallback to "FE" if session is somehow lost
            }
        }
    }

    public partial class Bill : System.Web.UI.MasterPage
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        private string ConnString => ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. Validate Core Session
            if (Session["USERID"] == null || Session["SessionToken"] == null)
            {
                Response.Redirect("~/index.aspx", false);
                return;
            }

            // 2. Validate Active Session Token (Concurrent Login Check)
            using (var cn = new SqlConnection(ConnString))
            {
                cn.Open();
                using (var cmd = new SqlCommand("SELECT IsActive FROM dbo.ActiveSessions WHERE SessionToken = @Token", cn))
                {
                    cmd.Parameters.AddWithValue("@Token", Session["SessionToken"].ToString());
                    object result = cmd.ExecuteScalar();

                    if (result == null || Convert.ToBoolean(result) == false)
                    {
                        Session.Clear();
                        Session.Abandon();
                        Response.Redirect("~/index.aspx", false);
                        return;
                    }
                }
            }

            // 3. Prevent Caching
            HttpContext.Current.Response.Cache.SetAllowResponseInBrowserHistory(false);
            HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            HttpContext.Current.Response.Cache.SetNoStore();

            // ==========================================
            // FORCE PASSWORD & CONTACT VERIFICATION GLOBAL LOCKOUT
            // ==========================================
            bool isLockedOut = Session["MustUpdateUserId"] != null || Session["MustVerifyContact"] != null;

            if (isLockedOut)
            {
                // Get the current page filename 
                string currentPage = System.IO.Path.GetFileName(Request.Url.AbsolutePath).ToLower();

                // If they are NOT on the settings page, force them there immediately
                if (currentPage != "settings.aspx")
                {
                    Response.Redirect("~/corporate/business/app/settings.aspx", false);
                    return;
                }

                // IMPORTANT: If they ARE on settings.aspx and locked out, we return immediately.
                // We do NOT want to load the menu, company dropdown, or header if they are locked out.
                return;
            }
            // ==========================================

            // 4. Load Normal UI Elements (Only executes if NOT locked out)
            if (!IsPostBack)
            {
                int currentYear = DateTime.Now.Year;
                lbl_crntyr.Text = $"{currentYear - 2}-{currentYear}";

                GetMenuControl();

                // MULTI-COMPANY INITIALIZATION
                BindCompanies();

                if (Session["CompanyID"] != null)
                {
                    ddlCompany.SelectedValue = Session["CompanyID"].ToString();
                }
                else if (ddlCompany.Items.Count > 0)
                {
                    Session["CompanyID"] = ddlCompany.Items[0].Value;
                }

                LoadCompanyHeader();
            }
            else
            {
                // Ensure dynamic header loads on every postback
                LoadCompanyHeader();
            }

            GetAdminName();
        }

        // --- MULTI-COMPANY METHODS ---

        private void BindCompanies()
        {
            using (SqlConnection con = new SqlConnection(ConnString))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT ID, Name FROM tbl_Company WHERE IsActive = 1 OR IsActive IS NULL ORDER BY ID ASC", con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlCompany.DataSource = dt;
                ddlCompany.DataTextField = "Name";
                ddlCompany.DataValueField = "ID";
                ddlCompany.DataBind();
            }
        }

        private void LoadCompanyHeader()
        {
            if (Session["CompanyID"] == null) return;

            int companyId = Convert.ToInt32(Session["CompanyID"]);

            using (SqlConnection con = new SqlConnection(ConnString))
            {
                // ADDED 'ShortCode' to the SELECT query
                SqlCommand cmd = new SqlCommand("SELECT Name, Address, Signe, ShortCode FROM tbl_Company WHERE ID=@ID", con);
                cmd.Parameters.AddWithValue("@ID", companyId);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    lblCompanyName.Text = dr["Name"].ToString();

                    // --- NEW: Save the ShortCode into the Session ---
                    Session["CompanyCode"] = dr["ShortCode"] != DBNull.Value ? dr["ShortCode"].ToString() : "FE";

                    // Convert raw binary image to base64 for seamless display
                    if (!Convert.IsDBNull(dr["Signe"]))
                    {
                        byte[] bytes = (byte[])dr["Signe"];
                        string base64 = Convert.ToBase64String(bytes);
                        Image2.ImageUrl = "data:image/png;base64," + base64;
                    }
                    else
                    {
                        // Fallback image if company has no logo configured
                        Image2.ImageUrl = "../WebImages/aagrouplogo.png";
                    }
                }
            }
        }

        protected void ddlCompany_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["CompanyID"] = ddlCompany.SelectedValue;
            Response.Redirect(Request.RawUrl, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void GetAdminName()
        {
            if (Session["USERID"] == null) return;

            string UserName = Session["USERID"].ToString();
            string cmdString = @"
                SELECT u.Name, r.RoleName, u.ProfilePictureUrl 
                FROM tbl_login u 
                LEFT JOIN Roles r ON u.RoleId = r.RoleId 
                WHERE u.User_Id=@UserId";

            try
            {
                DbCL.Sqlconnection();
                DbCL.ConnectDb();
                using (SqlCommand cmd = new SqlCommand(cmdString, DbCL.Conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", UserName);
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            lblName.Text = rdr["Name"] != DBNull.Value ? rdr["Name"].ToString() : "Unknown User";

                            Label lblRole = this.FindControl("lblRole") as Label;
                            if (lblRole != null)
                            {
                                string role = rdr["RoleName"] != DBNull.Value ? rdr["RoleName"].ToString() : "";
                                lblRole.Text = string.IsNullOrEmpty(role) ? "Standard User" : role;
                            }

                            Image imgProfile = this.FindControl("imgProfile") as Image;
                            if (imgProfile != null)
                            {
                                string picUrl = rdr["ProfilePictureUrl"] != DBNull.Value ? rdr["ProfilePictureUrl"].ToString() : "";
                                imgProfile.ImageUrl = string.IsNullOrEmpty(picUrl)
                                    ? "~/corporate/business/WebImages/representative.png"
                                    : picUrl;
                            }
                        }
                    }
                }
            }
            finally { DbCL.DisconnectDb(); }
        }

        private void GetMenuControl()
        {
            if (Session["USERID"] == null) return;
            string UserName = Session["USERID"].ToString();

            List<string> allSystemPermissions = new List<string>();
            HashSet<string> userGrantedPermissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using (var cn = new SqlConnection(ConnString))
            {
                cn.Open();
                using (var cmdAll = new SqlCommand("SELECT PermissionKey FROM dbo.Permissions", cn))
                using (var rdrAll = cmdAll.ExecuteReader())
                {
                    while (rdrAll.Read()) allSystemPermissions.Add(rdrAll.GetString(0));
                }

                string sqlUserPerms = @"
                    SELECT DISTINCT p.PermissionKey 
                    FROM dbo.Permissions p
                    INNER JOIN dbo.RolePermissions rp ON p.PermissionId = rp.PermissionId
                    INNER JOIN dbo.UserRoles ur ON rp.RoleId = ur.RoleId
                    INNER JOIN dbo.tbl_login u ON ur.UserId = u.Id
                    WHERE u.User_Id = @UserId";

                using (var cmdUser = new SqlCommand(sqlUserPerms, cn))
                {
                    cmdUser.Parameters.AddWithValue("@UserId", UserName);
                    using (var rdrUser = cmdUser.ExecuteReader())
                    {
                        while (rdrUser.Read()) userGrantedPermissions.Add(rdrUser.GetString(0));
                    }
                }
            }

            foreach (string menuId in allSystemPermissions)
            {
                Control menuControl = FindControlRecursive(this, menuId);
                if (menuControl != null)
                {
                    menuControl.Visible = userGrantedPermissions.Contains(menuId);
                }
            }
        }

        private Control FindControlRecursive(Control rootControl, string controlID)
        {
            if (rootControl.ID == controlID) return rootControl;
            foreach (Control controlToSearch in rootControl.Controls)
            {
                Control controlToReturn = FindControlRecursive(controlToSearch, controlID);
                if (controlToReturn != null) return controlToReturn;
            }
            return null;
        }

        protected void btnLogOut_Click(object sender, EventArgs e)
        {
            if (Session["SessionToken"] != null)
            {
                try
                {
                    using (var cn = new SqlConnection(ConnString))
                    {
                        cn.Open();
                        using (var cmd = new SqlCommand("UPDATE dbo.ActiveSessions SET IsActive = 0 WHERE SessionToken = @Token", cn))
                        {
                            cmd.Parameters.AddWithValue("@Token", Session["SessionToken"].ToString());
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch { /* Ignore DB errors on logout */ }
            }

            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/index.aspx", false);
        }

        // --- IT SUPPORT SYSTEM METHODS ---

        private string GetUserEmailFromDatabase(string userId)
        {
            string email = "Not Provided";
            string query = "SELECT Email FROM tbl_login WHERE User_Id = @UserId";
            try
            {
                using (var cn = new SqlConnection(ConnString))
                {
                    using (var cmd = new SqlCommand(query, cn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cn.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value) email = result.ToString();
                    }
                }
            }
            catch { }
            return email;
        }

        private void NotifyITSupport(string userId, string name, string email, string url, string message, string file1Path, string file2Path)
        {
            string insertQuery = @"
            INSERT INTO tbl_ITSupportTickets (UserId, UserName, UserEmail, PageUrl, UserMessage, Attachment1Path, Attachment2Path, CreatedDate)
            VALUES (@UserId, @UserName, @UserEmail, @PageUrl, @Message, @File1, @File2, GETDATE())";

            using (var cn = new SqlConnection(ConnString))
            {
                using (var cmd = new SqlCommand(insertQuery, cn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@UserName", name);
                    cmd.Parameters.AddWithValue("@UserEmail", email);
                    cmd.Parameters.AddWithValue("@PageUrl", url);
                    cmd.Parameters.AddWithValue("@Message", message);
                    cmd.Parameters.AddWithValue("@File1", string.IsNullOrEmpty(file1Path) ? (object)DBNull.Value : file1Path);
                    cmd.Parameters.AddWithValue("@File2", string.IsNullOrEmpty(file2Path) ? (object)DBNull.Value : file2Path);
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            SendSupportEmail(userId, name, email, url, message, file1Path, file2Path);
        }

        private void SendSupportEmail(string userId, string name, string email, string url, string message, string file1Path, string file2Path)
        {
            try
            {
                string smtpFrom = ConfigurationManager.AppSettings["SmtpFrom"];
                string smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
                string smtpPass = ConfigurationManager.AppSettings["SmtpPass"];
                string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
                int smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]);
                bool enableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["SmtpEnableSsl"]);

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(smtpFrom, "IT Support System");
                mail.To.Add("it.support@aminruptechnologies.co.in");
                mail.Subject = "New IT Support Concern Raised by " + name;
                mail.IsBodyHtml = true;

                mail.Body = $@"
                    <h2 style='color: #2268a9;'>New IT Support Concern</h2><hr />
                    <p><strong>User ID:</strong> {userId}</p>
                    <p><strong>Name:</strong> {name}</p>
                    <p><strong>User's Email:</strong> {email}</p>
                    <p><strong>Reported From URL:</strong> <a href='{url}'>{url}</a></p><br />
                    <p><strong>User Message:</strong></p>
                    <p style='background-color: #f9f9f9; padding: 10px; border-left: 4px solid #2268a9;'>
                        {message.Replace(Environment.NewLine, "<br/>")}
                    </p>";

                if (!string.IsNullOrEmpty(file1Path) && File.Exists(file1Path)) mail.Attachments.Add(new Attachment(file1Path));
                if (!string.IsNullOrEmpty(file2Path) && File.Exists(file2Path)) mail.Attachments.Add(new Attachment(file2Path));

                using (SmtpClient smtp = new SmtpClient(smtpHost, smtpPort))
                {
                    smtp.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass);
                    smtp.EnableSsl = enableSsl;
                    smtp.Send(mail);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ticket saved, but email notification failed: " + ex.Message);
            }
        }

        protected void btnSubmitSupport_Click(object sender, EventArgs e)
        {
            try
            {
                string userId = Session["USERID"] != null ? Session["USERID"].ToString() : "Unknown User";
                string userName = lblName.Text;
                string currentUrl = Request.Url.AbsoluteUri;
                string userMessage = txtSupportMessage.Text;
                string userEmail = GetUserEmailFromDatabase(userId);

                string saveDirectory = Server.MapPath("~/SupportUploads/");
                if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);

                string file1Path = string.Empty;
                string file2Path = string.Empty;

                string rawBase64 = hfAutoScreenshot.Value;
                if (!string.IsNullOrEmpty(rawBase64))
                {
                    string cleanBase64 = rawBase64.Contains(",") ? rawBase64.Split(',')[1] : rawBase64;
                    byte[] imageBytes = Convert.FromBase64String(cleanBase64);
                    string autoFileName = userId + "_auto_" + DateTime.Now.Ticks + ".png";
                    file1Path = Path.Combine(saveDirectory, autoFileName);
                    File.WriteAllBytes(file1Path, imageBytes);
                }

                if (fileScreenshot1.HasFile)
                {
                    string fileName1 = userId + "_manual_" + DateTime.Now.Ticks + "_" + fileScreenshot1.FileName;
                    file2Path = Path.Combine(saveDirectory, fileName1);
                    fileScreenshot1.SaveAs(file2Path);
                }

                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                CreateiTopTicket(userId, userName, currentUrl, userMessage, rawBase64);
                NotifyITSupport(userId, userName, userEmail, currentUrl, userMessage, file1Path, file2Path);

                txtSupportMessage.Text = "";
                hfAutoScreenshot.Value = "";

                string successScript = "alert('Success! Your ticket is logged in iTop and Support has been notified.'); document.getElementById('supportModal').style.display = 'none'; document.getElementById('imgScreenshotPreview').style.display = 'none';";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "FinalSuccess", successScript, true);
            }
            catch (Exception ex)
            {
                lblSupportStatus.Text = "Error: " + ex.Message;
                lblSupportStatus.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void CreateiTopTicket(string userId, string name, string currentUrl, string userMessage, string base64Screenshot)
        {
            try
            {
                string iTopUrl = ConfigurationManager.AppSettings["iTopUrl"];
                string iTopUser = ConfigurationManager.AppSettings["iTopUser"];
                string iTopPass = ConfigurationManager.AppSettings["iTopPass"];
                string callerEmail = ConfigurationManager.AppSettings["iTopCallerEmail"];
                string orgName = ConfigurationManager.AppSettings["iTopOrgName"];

                string descriptionHtml = $"<p><strong>Reported By:</strong> {name} (User ID: {userId})</p>";
                descriptionHtml += $"<p><strong>Page URL:</strong> <a href='{currentUrl}'>{currentUrl}</a></p>";
                descriptionHtml += $"<p><strong>Message:</strong><br/>{userMessage.Replace(Environment.NewLine, "<br/>")}</p>";

                System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
                js.MaxJsonLength = int.MaxValue;

                var ticketPayload = new
                {
                    operation = "core/create",
                    comment = "Ticket created via FLAME-EX Portal API",
                    @class = "UserRequest",
                    output_fields = "id",
                    fields = new
                    {
                        org_id = $"SELECT Organization WHERE name = '{orgName}'",
                        caller_id = $"SELECT Person WHERE email = '{callerEmail}'",
                        title = $"FLAME-EX Issue: {name} ({userId})",
                        description = descriptionHtml
                    }
                };

                string ticketJsonData = js.Serialize(ticketPayload);
                string newTicketId = "";

                using (var client = new System.Net.WebClient())
                {
                    var reqParm = new System.Collections.Specialized.NameValueCollection();
                    reqParm.Add("version", "1.3");
                    reqParm.Add("auth_user", iTopUser);
                    reqParm.Add("auth_pwd", iTopPass);
                    reqParm.Add("json_data", ticketJsonData);

                    byte[] responseBytes = client.UploadValues(iTopUrl, "POST", reqParm);
                    string responseBody = System.Text.Encoding.UTF8.GetString(responseBytes);

                    if (responseBody.Contains("\"code\":") && !responseBody.Contains("\"code\":0"))
                    {
                        throw new Exception("iTop Ticket Creation Error: " + responseBody);
                    }

                    System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(responseBody, "\"key\"\\s*:\\s*\"?(\\d+)\"?");
                    if (match.Success) newTicketId = match.Groups[1].Value;
                    else throw new Exception("Ticket created, but could not extract ID to attach image.");
                }

                if (!string.IsNullOrEmpty(base64Screenshot) && !string.IsNullOrEmpty(newTicketId))
                {
                    string cleanBase64 = base64Screenshot.Contains(",") ? base64Screenshot.Split(',')[1] : base64Screenshot;

                    var attachmentPayload = new
                    {
                        operation = "core/create",
                        comment = "Auto-captured screenshot from FLAME-EX",
                        @class = "Attachment",
                        output_fields = "id",
                        fields = new
                        {
                            item_class = "UserRequest",
                            item_id = newTicketId,
                            contents = new
                            {
                                data = cleanBase64,
                                mimetype = "image/png",
                                filename = "Auto_Screenshot.png"
                            }
                        }
                    };

                    string attachmentJsonData = js.Serialize(attachmentPayload);

                    using (var client = new System.Net.WebClient())
                    {
                        var reqParm = new System.Collections.Specialized.NameValueCollection();
                        reqParm.Add("version", "1.3");
                        reqParm.Add("auth_user", iTopUser);
                        reqParm.Add("auth_pwd", iTopPass);
                        reqParm.Add("json_data", attachmentJsonData);
                        client.UploadValues(iTopUrl, "POST", reqParm);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}