using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;

namespace Bill_Software.corporate.business.app
{
    public partial class SwitchUser : Page
    {
        private string ConnString => ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Guard: must be logged in
            if (Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx", false);
                return;
            }

            // Guard: must have SwitchUser permission
            if (!AuthGuard.HasPermission(Session["USERID"].ToString(), "SwitchUser", CompanyContext.CurrentCompanyID))
            {
                Response.Redirect("~/corporate/business/app/home.aspx", false);
                return;
            }

            if (!IsPostBack)
            {
                ShowImpersonationBanner();
                BindDefaultUsers();
            }
        }

        private void ShowImpersonationBanner()
        {
            bool isImpersonating = Session["SwitchedFrom"] != null && (bool)Session["SwitchedFrom"];
            pnlImpersonating.Visible = isImpersonating;
            if (isImpersonating)
            {
                lblImpersonatingUser.Text = Session["OriginalUserId"] != null
                    ? Session["OriginalUserId"].ToString()
                    : "Unknown";
            }
        }

        private void BindDefaultUsers()
        {
            // Show recent/active users when no search term
            BindUserGrid("SELECT TOP 50 u.User_Id, u.Name, ISNULL(r.RoleName, '') AS RoleName " +
                         "FROM tbl_login u " +
                         "LEFT JOIN Roles r ON u.RoleId = r.RoleId AND r.CompanyID = @CompanyID " +
                         "WHERE u.CompanyID = @CompanyID AND u.User_Id <> @CurrentUserId " +
                         "ORDER BY u.Name");
        }

        protected void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string search = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(search))
            {
                BindDefaultUsers();
                return;
            }

            BindUserGrid("SELECT TOP 50 u.User_Id, u.Name, ISNULL(r.RoleName, '') AS RoleName " +
                         "FROM tbl_login u " +
                         "LEFT JOIN Roles r ON u.RoleId = r.RoleId AND r.CompanyID = @CompanyID " +
                         "WHERE u.CompanyID = @CompanyID AND u.User_Id <> @CurrentUserId " +
                         "AND (u.Name LIKE @Search OR u.User_Id LIKE @Search) " +
                         "ORDER BY u.Name");
        }

        private void BindUserGrid(string sql)
        {
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyContext.CurrentCompanyID });
                cmd.Parameters.Add(new SqlParameter("@CurrentUserId", SqlDbType.NVarChar, 100) { Value = Session["USERID"].ToString() });

                if (sql.Contains("@Search"))
                {
                    cmd.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar, 100)
                    {
                        Value = "%" + txtSearch.Text.Trim() + "%"
                    });
                }

                cn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    DataTable dt = new DataTable();
                    dt.Load(rdr);
                    rptUsers.DataSource = dt;
                    rptUsers.DataBind();
                    lblNoResults.Visible = dt.Rows.Count == 0;
                }
            }
        }

        protected void btnSwitch_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string targetUserId = btn.CommandArgument;

            if (string.IsNullOrEmpty(targetUserId)) return;

            // Don't allow switching to yourself
            if (string.Equals(targetUserId, Session["USERID"].ToString(), StringComparison.OrdinalIgnoreCase))
            {
                lblStatus.Text = "You are already logged in as this user.";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                lblStatus.Visible = true;
                return;
            }

            // Validate target user exists and belongs to current company
            string targetName = ValidateTargetUser(targetUserId);
            if (string.IsNullOrEmpty(targetName))
            {
                lblStatus.Text = "User not found in the current company.";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                lblStatus.Visible = true;
                return;
            }

            // Save original user state (only if not already impersonating — prevent nested switches)
            if (Session["SwitchedFrom"] == null || !(bool)Session["SwitchedFrom"])
            {
                Session["OriginalUserId"] = Session["USERID"];
                Session["OriginalRoleId"] = Session["RoleId"];
                Session["OriginalRoleName"] = Session["RoleName"];
                Session["OriginalCompanyId"] = Session["CompanyID"];
            }

            // Resolve the target user's numeric ID, role, and company
            int targetNumericId = 0;
            string targetRoleId = null;
            string targetRoleName = null;
            int targetCompanyId = CompanyContext.CurrentCompanyID;

            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(
                "SELECT u.Id, u.RoleId, ISNULL(r.RoleName, '') AS RoleName " +
                "FROM tbl_login u " +
                "LEFT JOIN Roles r ON u.RoleId = r.RoleId AND r.CompanyID = @CompanyID " +
                "WHERE u.User_Id = @UserId AND u.CompanyID = @CompanyID", cn))
            {
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 100) { Value = targetUserId });
                cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = targetCompanyId });
                cn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        targetNumericId = rdr.GetInt32(0);
                        targetRoleId = rdr["RoleId"] != DBNull.Value ? rdr["RoleId"].ToString() : null;
                        targetRoleName = rdr["RoleName"].ToString();
                    }
                }
            }

            if (targetNumericId == 0)
            {
                lblStatus.Text = "User not found.";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                lblStatus.Visible = true;
                return;
            }

            // Create a new ActiveSessions entry for the target user
            Guid sessionToken = Guid.NewGuid();
            using (var cn = new SqlConnection(ConnString))
            {
                cn.Open();

                // Deactivate old session for this browser
                if (Session["SessionToken"] != null)
                {
                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.ActiveSessions SET IsActive = 0 WHERE SessionToken = @Token", cn))
                    {
                        cmd.Parameters.Add(new SqlParameter("@Token", SqlDbType.UniqueIdentifier)
                        {
                            Value = new Guid(Session["SessionToken"].ToString())
                        });
                        cmd.ExecuteNonQuery();
                    }
                }

                // Insert new session for target user
                using (var cmd = new SqlCommand(
                    "INSERT INTO dbo.ActiveSessions (SessionToken, UserId, LoginTime, LastHeartbeat, IPAddress, UserAgent, IsActive, CompanyID) " +
                    "VALUES (@Token, @UserId, SYSUTCDATETIME(), SYSUTCDATETIME(), @IP, @UA, 1, @CompanyID)", cn))
                {
                    cmd.Parameters.Add(new SqlParameter("@Token", SqlDbType.UniqueIdentifier) { Value = sessionToken });
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = targetNumericId });
                    cmd.Parameters.Add(new SqlParameter("@IP", SqlDbType.NVarChar, 50)
                    {
                        Value = HttpContext.Current.Request.UserHostAddress ?? ""
                    });
                    cmd.Parameters.Add(new SqlParameter("@UA", SqlDbType.NVarChar, 500)
                    {
                        Value = (HttpContext.Current.Request.UserAgent ?? "").Substring(0,
                            Math.Min(HttpContext.Current.Request.UserAgent?.Length ?? 0, 500))
                    });
                    cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = targetCompanyId });
                    cmd.ExecuteNonQuery();
                }
            }

            // Swap session identity
            Session["USERID"] = targetUserId;
            Session["SessionToken"] = sessionToken.ToString();
            Session["RoleId"] = targetRoleId;
            Session["RoleName"] = targetRoleName;
            Session["CompanyID"] = targetCompanyId;
            Session["SwitchedFrom"] = true;

            // Log the switch
            LogSwitch(Session["OriginalUserId"].ToString(), targetUserId);

            // Redirect to home as the new user
            Response.Redirect("~/corporate/business/app/home.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnSwitchBack_Click(object sender, EventArgs e)
        {
            if (Session["OriginalUserId"] == null) return;

            string originalUserId = Session["OriginalUserId"].ToString();

            // Deactivate current (impersonated) session
            if (Session["SessionToken"] != null)
            {
                using (var cn = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand(
                    "UPDATE dbo.ActiveSessions SET IsActive = 0 WHERE SessionToken = @Token", cn))
                {
                    cmd.Parameters.Add(new SqlParameter("@Token", SqlDbType.UniqueIdentifier)
                    {
                        Value = new Guid(Session["SessionToken"].ToString())
                    });
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            // Resolve original user's details
            int originalNumericId = 0;
            int originalCompanyId = Session["OriginalCompanyId"] != null
                ? Convert.ToInt32(Session["OriginalCompanyId"])
                : CompanyContext.CurrentCompanyID;

            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(
                "SELECT Id FROM tbl_login WHERE User_Id = @UserId AND CompanyID = @CompanyID", cn))
            {
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 100) { Value = originalUserId });
                cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = originalCompanyId });
                cn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read()) originalNumericId = rdr.GetInt32(0);
                }
            }

            if (originalNumericId == 0)
            {
                // Original user not found — force logout
                Session.Abandon();
                Response.Redirect("~/index.aspx", false);
                return;
            }

            // Create new session for original user
            Guid sessionToken = Guid.NewGuid();
            using (var cn = new SqlConnection(ConnString))
            {
                cn.Open();
                using (var cmd = new SqlCommand(
                    "INSERT INTO dbo.ActiveSessions (SessionToken, UserId, LoginTime, LastHeartbeat, IPAddress, UserAgent, IsActive, CompanyID) " +
                    "VALUES (@Token, @UserId, SYSUTCDATETIME(), SYSUTCDATETIME(), @IP, @UA, 1, @CompanyID)", cn))
                {
                    cmd.Parameters.Add(new SqlParameter("@Token", SqlDbType.UniqueIdentifier) { Value = sessionToken });
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = originalNumericId });
                    cmd.Parameters.Add(new SqlParameter("@IP", SqlDbType.NVarChar, 50)
                    {
                        Value = HttpContext.Current.Request.UserHostAddress ?? ""
                    });
                    cmd.Parameters.Add(new SqlParameter("@UA", SqlDbType.NVarChar, 500)
                    {
                        Value = (HttpContext.Current.Request.UserAgent ?? "").Substring(0,
                            Math.Min(HttpContext.Current.Request.UserAgent?.Length ?? 0, 500))
                    });
                    cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = originalCompanyId });
                    cmd.ExecuteNonQuery();
                }
            }

            // Restore original session
            Session["USERID"] = originalUserId;
            Session["SessionToken"] = sessionToken.ToString();
            Session["RoleId"] = Session["OriginalRoleId"];
            Session["RoleName"] = Session["OriginalRoleName"];
            Session["CompanyID"] = Session["OriginalCompanyId"];

            // Clear impersonation state
            Session["SwitchedFrom"] = null;
            Session["OriginalUserId"] = null;
            Session["OriginalRoleId"] = null;
            Session["OriginalRoleName"] = null;
            Session["OriginalCompanyId"] = null;

            Response.Redirect("~/corporate/business/app/home.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private string ValidateTargetUser(string targetUserId)
        {
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(
                "SELECT Name FROM tbl_login WHERE User_Id = @UserId AND CompanyID = @CompanyID", cn))
            {
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 100) { Value = targetUserId });
                cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyContext.CurrentCompanyID });
                cn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    return rdr.Read() ? rdr["Name"].ToString() : null;
                }
            }
        }

        private void LogSwitch(string fromUserId, string toUserId)
        {
            try
            {
                using (var cn = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand(
                    "INSERT INTO dbo.tbl_SystemNotification (Title, Message, ModuleCode, Severity, StartDate, EndDate, IsActive, CreatedBy, CompanyID) " +
                    "VALUES (@Title, @Message, 'Admin', 'Info', GETDATE(), DATEADD(DAY, 7, GETDATE()), 1, @CreatedBy, @CompanyID)", cn))
                {
                    cmd.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 200)
                    {
                        Value = "User Switch Performed"
                    });
                    cmd.Parameters.Add(new SqlParameter("@Message", SqlDbType.NVarChar)
                    {
                        Value = $"Admin {fromUserId} switched to user {toUserId}"
                    });
                    cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.NVarChar, 50) { Value = fromUserId });
                    cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyContext.CurrentCompanyID });
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // Audit log failure should not block the switch
            }
        }
    }
}
