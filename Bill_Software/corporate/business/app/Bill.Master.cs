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
    public partial class Bill : System.Web.UI.MasterPage
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        private string ConnString => ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. Memory Check: MUST be the very first thing!
            if (Session["USERID"] == null || Session["SessionToken"] == null)
            {
                Response.Redirect("~/index.aspx", false);
                return;
            }

            // 2. Database Validation Check: Protects against hard page refreshes (F5)
            using (var cn = new SqlConnection(ConnString))
            {
                cn.Open();
                using (var cmd = new SqlCommand("SELECT IsActive FROM dbo.ActiveSessions WHERE SessionToken = @Token", cn))
                {
                    cmd.Parameters.AddWithValue("@Token", Session["SessionToken"].ToString());
                    object result = cmd.ExecuteScalar();

                    // If the token doesn't exist or IsActive is 0, kick them out immediately
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

            // 4. Load UI Elements
            if (!IsPostBack)
            {
                // Dynamic year display
                int currentYear = DateTime.Now.Year;
                lbl_crntyr.Text = $"{currentYear - 2}-{currentYear}";

                GetMenuControl();
            }

            GetAdminName();
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
                            // Classic DBNull checks for ASP.NET 4.5.2
                            lblName.Text = rdr["Name"] != DBNull.Value ? rdr["Name"].ToString() : "Unknown User";

                            // Classic control casting
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
            finally
            {
                DbCL.DisconnectDb();
            }
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

                // 1. Get ALL permissions in the system to know which UI elements we need to manage
                using (var cmdAll = new SqlCommand("SELECT PermissionKey FROM dbo.Permissions", cn))
                using (var rdrAll = cmdAll.ExecuteReader())
                {
                    while (rdrAll.Read())
                    {
                        allSystemPermissions.Add(rdrAll.GetString(0));
                    }
                }

                // 2. Get ONLY the permissions the current user has access to via their assigned roles
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
                        while (rdrUser.Read())
                        {
                            userGrantedPermissions.Add(rdrUser.GetString(0));
                        }
                    }
                }
            }

            // 3. Loop through all system permissions, find their matching UI control, and show/hide it
            foreach (string menuId in allSystemPermissions)
            {
                Control menuControl = FindControlRecursive(this, menuId);

                if (menuControl != null)
                {
                    // If the user's granted permissions list contains this menuId, make it visible. 
                    // Otherwise, hide it.
                    menuControl.Visible = userGrantedPermissions.Contains(menuId);
                }
            }
        }

        // Helper function to deeply search the Master page for controls by ID
        private Control FindControlRecursive(Control rootControl, string controlID)
        {
            if (rootControl.ID == controlID) return rootControl;

            foreach (Control controlToSearch in rootControl.Controls)
            {
                Control controlToReturn = FindControlRecursive(controlToSearch, controlID);
                if (controlToReturn != null)
                {
                    return controlToReturn;
                }
            }
            return null;
        }

        protected void btnLogOut_Click(object sender, EventArgs e)
        {
            // 1. Mark session as dead in the database immediately
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

            // 2. Clear local memory
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/index.aspx", false);
        }

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
                        if (result != null && result != DBNull.Value)
                        {
                            email = result.ToString();
                        }
                    }
                }
            }
            catch
            {
                // If there's an error fetching the email, we'll just return "Not Provided"
                // to ensure the support ticket process doesn't completely fail.
            }
            return email;
        }

        private void NotifyITSupport(string userId, string name, string email, string url, string message, string file1Path, string file2Path)
        {
            // --- 1. Save Ticket to the Database ---
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

                    // Handle optional files: Insert DBNull if the path is empty
                    cmd.Parameters.AddWithValue("@File1", string.IsNullOrEmpty(file1Path) ? (object)DBNull.Value : file1Path);
                    cmd.Parameters.AddWithValue("@File2", string.IsNullOrEmpty(file2Path) ? (object)DBNull.Value : file2Path);

                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            // --- 2. Send the Email Trigger ---
            SendSupportEmail(userId, name, email, url, message, file1Path, file2Path);
        }

        private void SendSupportEmail(string userId, string name, string email, string url, string message, string file1Path, string file2Path)
        {
            try
            {
                // 1. Read SMTP settings from web.config <appSettings>
                string smtpFrom = ConfigurationManager.AppSettings["SmtpFrom"];
                string smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
                string smtpPass = ConfigurationManager.AppSettings["SmtpPass"];
                string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
                int smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]);
                bool enableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["SmtpEnableSsl"]);

                // 2. Configure the Email Message
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(smtpFrom, "IT Support System");

                // Change this to the email address that should RECEIVE the support tickets
                mail.To.Add("it.support@aminruptechnologies.co.in");

                mail.Subject = "New IT Support Concern Raised by " + name;
                mail.IsBodyHtml = true;

                // 3. Construct the email body
                mail.Body = $@"
                    <h2 style='color: #2268a9;'>New IT Support Concern</h2>
                    <hr />
                    <p><strong>User ID:</strong> {userId}</p>
                    <p><strong>Name:</strong> {name}</p>
                    <p><strong>User's Email:</strong> {email}</p>
                    <p><strong>Reported From URL:</strong> <a href='{url}'>{url}</a></p>
                    <br />
                    <p><strong>User Message:</strong></p>
                    <p style='background-color: #f9f9f9; padding: 10px; border-left: 4px solid #2268a9;'>
                        {message.Replace(Environment.NewLine, "<br/>")}
                    </p>
                ";

                // 4. Attach Files if they exist
                if (!string.IsNullOrEmpty(file1Path) && File.Exists(file1Path))
                {
                    mail.Attachments.Add(new Attachment(file1Path));
                }
                if (!string.IsNullOrEmpty(file2Path) && File.Exists(file2Path))
                {
                    mail.Attachments.Add(new Attachment(file2Path));
                }

                // 5. Configure the SMTP Client with your specific settings
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
                // 1. Gather Data
                string userId = Session["USERID"] != null ? Session["USERID"].ToString() : "Unknown User";
                string userName = lblName.Text;
                string currentUrl = Request.Url.AbsoluteUri;
                string userMessage = txtSupportMessage.Text;
                string userEmail = GetUserEmailFromDatabase(userId);

                // 2. Local File Storage
                string saveDirectory = Server.MapPath("~/SupportUploads/");
                if (!System.IO.Directory.Exists(saveDirectory)) System.IO.Directory.CreateDirectory(saveDirectory);

                string file1Path = string.Empty; // For Auto-Screenshot
                string file2Path = string.Empty; // For Manual Upload

                // 3. Process the Auto-Screenshot
                string rawBase64 = hfAutoScreenshot.Value;
                if (!string.IsNullOrEmpty(rawBase64))
                {
                    // Strip header for physical file saving
                    string cleanBase64 = rawBase64.Contains(",") ? rawBase64.Split(',')[1] : rawBase64;
                    byte[] imageBytes = Convert.FromBase64String(cleanBase64);

                    string autoFileName = userId + "_auto_" + DateTime.Now.Ticks + ".png";
                    file1Path = System.IO.Path.Combine(saveDirectory, autoFileName);
                    System.IO.File.WriteAllBytes(file1Path, imageBytes);
                }

                // 4. Handle Manual File Upload
                if (fileScreenshot1.HasFile)
                {
                    string fileName1 = userId + "_manual_" + DateTime.Now.Ticks + "_" + fileScreenshot1.FileName;
                    file2Path = System.IO.Path.Combine(saveDirectory, fileName1);
                    fileScreenshot1.SaveAs(file2Path);
                }

                // --- THE INTEGRATIONS ---

                // A. iTop Integration (Uses modern TLS 1.2)
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                CreateiTopTicket(userId, userName, currentUrl, userMessage, rawBase64);

                // B. SQL & Email Integration
                NotifyITSupport(userId, userName, userEmail, currentUrl, userMessage, file1Path, file2Path);

                // 5. Final Cleanup
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
                // 1. Fetch settings from web.config
                string iTopUrl = ConfigurationManager.AppSettings["iTopUrl"];
                string iTopUser = ConfigurationManager.AppSettings["iTopUser"];
                string iTopPass = ConfigurationManager.AppSettings["iTopPass"];
                string callerEmail = ConfigurationManager.AppSettings["iTopCallerEmail"];
                string orgName = ConfigurationManager.AppSettings["iTopOrgName"];

                // 2. Format the HTML Description (WITHOUT the image string)
                string descriptionHtml = $"<p><strong>Reported By:</strong> {name} (User ID: {userId})</p>";
                descriptionHtml += $"<p><strong>Page URL:</strong> <a href='{currentUrl}'>{currentUrl}</a></p>";
                descriptionHtml += $"<p><strong>Message:</strong><br/>{userMessage.Replace(Environment.NewLine, "<br/>")}</p>";

                // 3. Construct the Ticket JSON Payload
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
                        description = descriptionHtml,

                        //// --- THE FIX: Using the exact names from your database export ---
                        //service_id = "SELECT Service WHERE name = 'AS-Application Support'",
                        //servicesubcategory_id = "SELECT ServiceSubcategory WHERE name = 'Application Support'"

                        // --- THE FIX: Passing the direct integer IDs! ---
                        //service_id = "SELECT Service WHERE id = 2",
                        //servicesubcategory_id = "SELECT ServiceSubcategory WHERE id = 2"
                    }
                };

                string ticketJsonData = js.Serialize(ticketPayload);
                string newTicketId = "";

                // 4. Send the POST request to create the TICKET
                using (var client = new System.Net.WebClient())
                {
                    var reqParm = new System.Collections.Specialized.NameValueCollection();
                    reqParm.Add("version", "1.3");
                    reqParm.Add("auth_user", iTopUser);
                    reqParm.Add("auth_pwd", iTopPass);
                    reqParm.Add("json_data", ticketJsonData);

                    byte[] responseBytes = client.UploadValues(iTopUrl, "POST", reqParm);
                    string responseBody = System.Text.Encoding.UTF8.GetString(responseBytes);

                    // Catch actual iTop rejection errors
                    if (responseBody.Contains("\"code\":") && !responseBody.Contains("\"code\":0"))
                    {
                        throw new Exception("iTop Ticket Creation Error: " + responseBody);
                    }

                    // --- THE BULLETPROOF FIX: Use Regex to find the Ticket ID safely ---
                    // iTop returns something like "key":"1234" in its JSON response. 
                    // This regex safely extracts just the number.
                    System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(responseBody, "\"key\"\\s*:\\s*\"?(\\d+)\"?");
                    if (match.Success)
                    {
                        newTicketId = match.Groups[1].Value;
                    }
                    else
                    {
                        throw new Exception("Ticket created, but could not extract ID to attach image.");
                    }
                }

                // 5. If we have a screenshot AND a successful Ticket ID, send the ATTACHMENT
                if (!string.IsNullOrEmpty(base64Screenshot) && !string.IsNullOrEmpty(newTicketId))
                {
                    // Clean the base64 string to remove the web prefix
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
                            item_id = newTicketId,      // Link exactly to the ticket we just made!
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

                        // Upload the attachment!
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

//using System;
 //using System.Data;
 //using System.Data.SqlClient;
 //using System.Web;
 //using System.Web.UI;

//namespace Bill_Software.corporate.business.app
//{
//    public partial class Bill : System.Web.UI.MasterPage
//    {
//        DB_UTILITY DbCL = new DB_UTILITY();
//        DataTable dtm = new DataTable();

//        protected void Page_Load(object sender, EventArgs e)
//        {
//            if (!IsPostBack)
//            {
//                // Dynamic year display
//                int currentYear = DateTime.Now.Year;
//                lbl_crntyr.Text = $"{currentYear - 1}-{currentYear}";

//                GetMenuControl();
//            }

//            HttpContext.Current.Response.Cache.SetAllowResponseInBrowserHistory(false);
//            HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
//            HttpContext.Current.Response.Cache.SetNoStore();

//            if (HttpContext.Current.Session["USERID"] == null)
//            {
//                Response.Redirect("~/index.aspx", false);
//            }
//            GetAdminName();
//        }

//        private void GetAdminName()
//        {
//            if (Session["USERID"] == null) return;

//            string UserName = Session["USERID"].ToString();
//            string cmdString = "SELECT Name FROM tbl_login WHERE User_Id=@UserId";

//            try
//            {
//                DbCL.Sqlconnection();
//                DbCL.ConnectDb();
//                using (SqlCommand cmd = new SqlCommand(cmdString, DbCL.Conn))
//                {
//                    cmd.Parameters.AddWithValue("@UserId", UserName);
//                    using (SqlDataReader rdr = cmd.ExecuteReader())
//                    {
//                        if (rdr.Read())
//                        {
//                            lblName.Text = rdr["Name"].ToString();
//                        }
//                    }
//                }
//            }
//            finally
//            {
//                DbCL.DisconnectDb(); // Ensures the connection is safely closed
//            }
//        }

//        private void GetMenuControl()
//        {
//            if (Session["USERID"] == null) return;

//            string UserName = Session["USERID"].ToString();
//            string query = "SELECT * FROM vw_FullDesignation WHERE User_Id=@User_Id";
//            SqlParameter[] pram = { new SqlParameter("@User_Id", UserName) };

//            dtm = DbCL.SPreturn_dt(query, pram);

//            if (dtm != null && dtm.Rows.Count > 0)
//            {
//                DataRow row = dtm.Rows[0];

//                // Loop through every column returned from the database
//                foreach (DataColumn column in dtm.Columns)
//                {
//                    string menuId = column.ColumnName;

//                    // Find the HTML Control with the matching ID
//                    Control menuControl = FindControlRecursive(this, menuId);

//                    if (menuControl != null)
//                    {
//                        // Set visibility based on the "Yes" / "No" string in the database
//                        bool isVisible = row[menuId].ToString().Equals("Yes", StringComparison.OrdinalIgnoreCase);
//                        menuControl.Visible = isVisible;
//                    }
//                }
//            }
//        }

//        // Helper function to deeply search the Master page for controls by ID
//        private Control FindControlRecursive(Control rootControl, string controlID)
//        {
//            if (rootControl.ID == controlID) return rootControl;

//            foreach (Control controlToSearch in rootControl.Controls)
//            {
//                Control controlToReturn = FindControlRecursive(controlToSearch, controlID);
//                if (controlToReturn != null)
//                {
//                    return controlToReturn;
//                }
//            }
//            return null;
//        }

//        protected void btnLogOut_Click(object sender, EventArgs e)
//        {
//            Session.Clear();
//            Session.Abandon();
//            Response.Redirect("~/index.aspx", false);
//        }
//    }
//}