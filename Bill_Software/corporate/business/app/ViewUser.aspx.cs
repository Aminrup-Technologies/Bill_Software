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
using System.Web.Services;
using System.Web;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm80 : System.Web.UI.Page
    {
        // FIX 1: C# 5.0 Compatible Property
        private string ConnString
        {
            get { return ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString; }
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
                BindGrid();
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton != null)
            {
                string selectedFilter = clickedButton.CommandArgument;
                ViewState["CurrentFilter"] = selectedFilter;

                // Reset all buttons to default style
                btnFilterAll.CssClass = "filter-btn";
                btnFilterActive.CssClass = "filter-btn";
                btnFilterInactive.CssClass = "filter-btn";
                btnFilterLocked.CssClass = "filter-btn";

                // Apply 'active' style to the clicked button
                clickedButton.CssClass = "filter-btn active";

                // Close any open edit rows and refresh the data
                lvUsers.EditIndex = -1;
                BindGrid();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            lvUsers.EditIndex = -1; // Close any open edit panels when searching
            BindGrid();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            ViewState["CurrentFilter"] = "All";

            btnFilterAll.CssClass = "filter-btn active";
            btnFilterActive.CssClass = "filter-btn";
            btnFilterInactive.CssClass = "filter-btn";
            btnFilterLocked.CssClass = "filter-btn";

            lvUsers.EditIndex = -1;
            BindGrid();
        }

        protected void ddlEmpId_SelectedIndexChanged(object sender, EventArgs e) { BindGrid(); }
        protected void btnRefresh_Click(object sender, EventArgs e) { BindGrid(); }

        private void BindGrid()
        {
            // 1. Check the logged-in User ID from Session
            string loggedInUser = Session["USERID"] != null ? Session["USERID"].ToString() : "";

            // 2. Base Query strictly enforcing CompanyContext
            string sql = @"SELECT u.Id, u.User_Id, u.Name, u.Email, u.Phone_no, u.IsActive, u.LockoutEnd, 
                          u.LastLogin, u.CreatedAt, u.MustChangePassword, u.EmailVerified,
                          u.ProfilePictureUrl, u.RoleId, r.RoleName, u.RequireGeoTagging, u.EnableEmailAlerts, u.EnableWhatsAppAlerts,
                          u.DepartmentID, d.DepartmentName, 
                          u.DesignationID, des.DesignationName, 
                          u.ReportingManagerId, mgr.Name AS ManagerName,
                            u.GeoFenceLat, u.GeoFenceLng, u.GeoFenceRadius,
                            ISNULL(u.AllowGeoFenceOverride, 1) AS AllowGeoFenceOverride,
                            ISNULL(u.MaxGeoFenceAttempts, 3) AS MaxGeoFenceAttempts,
                          (SELECT TOP 1 LastHeartbeat FROM ActiveSessions s WHERE s.UserId = u.Id ORDER BY LastHeartbeat DESC) AS LatestHeartbeat
                   FROM dbo.tbl_login u
                   LEFT JOIN dbo.Roles r ON u.RoleId = r.RoleId
                   LEFT JOIN dbo.tbl_Departments d ON u.DepartmentID = d.DepartmentID
                   LEFT JOIN dbo.tbl_Designations des ON u.DesignationID = des.DesignationID
                   LEFT JOIN dbo.tbl_login mgr ON u.ReportingManagerId = mgr.User_Id
                   WHERE u.CompanyID = @CompanyID ";

            // 3. Dynamic Admin Filter
            if (!loggedInUser.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                // Hide super accounts from normal users/managers
                sql += " AND u.User_Id NOT IN ('admin', 'AT01') ";
            }

            // 4. Status Filter Logic
            string currentFilter = ViewState["CurrentFilter"] as string;
            if (currentFilter == "Active")
            {
                sql += " AND u.IsActive = 1 ";
            }
            else if (currentFilter == "Inactive")
            {
                sql += " AND u.IsActive = 0 ";
            }
            else if (currentFilter == "Locked")
            {
                sql += " AND (u.LockoutEnd IS NOT NULL AND u.LockoutEnd > SYSDATETIMEOFFSET()) ";
            }

            // 5. Search Box Filter Logic
            string searchTerm = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                sql += @" AND (
                    u.User_Id LIKE '%' + @SearchTerm + '%' OR 
                    u.Name LIKE '%' + @SearchTerm + '%' OR 
                    u.Email LIKE '%' + @SearchTerm + '%' OR 
                    u.Phone_no LIKE '%' + @SearchTerm + '%'
                  ) ";
            }

            sql += " ORDER BY u.Id";

            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                // 6. Parameter Injection
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", searchTerm);
                }

                var dt = new DataTable();
                var da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                lvUsers.DataSource = dt;
                lvUsers.DataBind();
            }
        }

        private void BindGrid_OLD()
        {
            // NEW: Geo-Fence columns added to SELECT, and CompanyContext applied to WHERE
            string sql = @"SELECT u.Id, u.User_Id, u.Name, u.Email, u.Phone_no, u.IsActive, u.LockoutEnd, 
                                  u.LastLogin, u.CreatedAt, u.MustChangePassword, u.EmailVerified,
                                  u.ProfilePictureUrl, u.RoleId, r.RoleName, u.RequireGeoTagging, u.EnableEmailAlerts, u.EnableWhatsAppAlerts,
                                  u.DepartmentID, d.DepartmentName, 
                                  u.DesignationID, des.DesignationName, 
                                  u.ReportingManagerId, mgr.Name AS ManagerName,
                                  u.GeoFenceLat, u.GeoFenceLng, u.GeoFenceRadius,
                                  (SELECT TOP 1 LastHeartbeat FROM ActiveSessions s WHERE s.UserId = u.Id ORDER BY LastHeartbeat DESC) AS LatestHeartbeat
                           FROM dbo.tbl_login u
                           LEFT JOIN dbo.Roles r ON u.RoleId = r.RoleId
                           LEFT JOIN dbo.tbl_Departments d ON u.DepartmentID = d.DepartmentID
                           LEFT JOIN dbo.tbl_Designations des ON u.DesignationID = des.DesignationID
                           LEFT JOIN dbo.tbl_login mgr ON u.ReportingManagerId = mgr.User_Id
                           WHERE (u.User_Id NOT IN ('admin', 'AT01')) 
                             AND u.CompanyID = @CompanyID ";

            string currentFilter = ViewState["CurrentFilter"] as string;
            if (currentFilter == "Active")
            {
                sql += " AND u.IsActive = 1 ";
            }
            else if (currentFilter == "Inactive")
            {
                sql += " AND u.IsActive = 0 ";
            }
            else if (currentFilter == "Locked")
            {
                sql += " AND (u.LockoutEnd IS NOT NULL AND u.LockoutEnd > SYSDATETIMEOFFSET()) ";
            }

            string searchTerm = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                sql += @" AND (
                    u.User_Id LIKE '%' + @SearchTerm + '%' OR 
                    u.Name LIKE '%' + @SearchTerm + '%' OR 
                    u.Email LIKE '%' + @SearchTerm + '%' OR 
                    u.Phone_no LIKE '%' + @SearchTerm + '%'
                  ) ";
            }

            sql += " ORDER BY u.Id";

            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", searchTerm);
                }

                var dt = new DataTable();
                var da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                lvUsers.DataSource = dt;
                lvUsers.DataBind();
            }
        }

        #region ListView Events

        protected void lvUsers_ItemEditing(object sender, ListViewEditEventArgs e)
        {
            lvUsers.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        protected void lvUsers_ItemCanceling(object sender, ListViewCancelEventArgs e)
        {
            lvUsers.EditIndex = -1;
            BindGrid();
        }

        protected void lvUsers_ItemUpdating(object sender, ListViewUpdateEventArgs e)
        {
            int id = Convert.ToInt32(lvUsers.DataKeys[e.ItemIndex].Value);
            ListViewItem item = lvUsers.Items[e.ItemIndex];

            TextBox txtName = item.FindControl("txtName") as TextBox;
            TextBox txtEmail = item.FindControl("txtEmail") as TextBox;
            TextBox txtPhone = item.FindControl("txtPhone") as TextBox;

            CheckBox chkEmail = item.FindControl("chkEmailVerified") as CheckBox;
            CheckBox chkPwd = item.FindControl("chkMustChangePwd") as CheckBox;
            CheckBox chkGeo = item.FindControl("chkRequireGeo") as CheckBox;
            CheckBox chkEmails = item.FindControl("chkEmails") as CheckBox;
            CheckBox chkWhatsApp = item.FindControl("chkWhatsApp") as CheckBox;

            DropDownList ddlGridRole = item.FindControl("ddlGridRole") as DropDownList;
            DropDownList ddlDepartment = item.FindControl("ddlDepartment") as DropDownList;
            DropDownList ddlDesignation = item.FindControl("ddlDesignation") as DropDownList;
            DropDownList ddlManager = item.FindControl("ddlManager") as DropDownList;

            string newName = txtName != null ? txtName.Text.Trim() : "";
            string newEmail = txtEmail != null ? txtEmail.Text.Trim() : "";
            string newPhone = txtPhone != null ? txtPhone.Text.Trim() : "";

            bool emailVerified = chkEmail != null ? chkEmail.Checked : false;
            bool mustChangePwd = chkPwd != null ? chkPwd.Checked : false;
            bool requireGeo = chkGeo != null ? chkGeo.Checked : false;
            bool requireEmails = chkEmails != null ? chkEmails.Checked : false;
            bool requireWhatsApp = chkWhatsApp != null ? chkWhatsApp.Checked : false;

            object roleIdParam = DBNull.Value;
            if (ddlGridRole != null && ddlGridRole.SelectedValue != "0")
            {
                roleIdParam = Convert.ToInt32(ddlGridRole.SelectedValue);
            }

            string updateSql = @"UPDATE dbo.tbl_login 
                         SET Name = @Name, Email = @Email, Phone_no = @Phone, 
                             EmailVerified = @EmailVerified, MustChangePassword = @MustChangePwd, 
                             RoleId = @RoleId, RequireGeoTagging = @RequireGeoTagging, 
                             EnableEmailAlerts = @EnableEmailAlerts, EnableWhatsAppAlerts = @EnableWhatsAppAlerts,
                             DepartmentID = @DeptId, DesignationID = @DesigId, ReportingManagerId = @ManagerId
                         WHERE Id = @Id AND CompanyID = @CompanyID"; // COMPANYCONTEXT SHIELD

            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(updateSql, cn))
            {
                cmd.Parameters.AddWithValue("@Name", string.IsNullOrWhiteSpace(newName) ? DBNull.Value : (object)newName);
                cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(newEmail) ? DBNull.Value : (object)newEmail);

                string cleanPhone = newPhone.Replace(" ", "").Replace("-", "");
                cmd.Parameters.AddWithValue("@Phone", string.IsNullOrWhiteSpace(cleanPhone) ? DBNull.Value : (object)cleanPhone);

                cmd.Parameters.AddWithValue("@DeptId", (ddlDepartment != null && !string.IsNullOrEmpty(ddlDepartment.SelectedValue)) ? (object)Convert.ToInt32(ddlDepartment.SelectedValue) : DBNull.Value);
                cmd.Parameters.AddWithValue("@DesigId", (ddlDesignation != null && !string.IsNullOrEmpty(ddlDesignation.SelectedValue)) ? (object)Convert.ToInt32(ddlDesignation.SelectedValue) : DBNull.Value);
                cmd.Parameters.AddWithValue("@ManagerId", (ddlManager != null && !string.IsNullOrEmpty(ddlManager.SelectedValue)) ? (object)ddlManager.SelectedValue : DBNull.Value);

                cmd.Parameters.AddWithValue("@EmailVerified", emailVerified);
                cmd.Parameters.AddWithValue("@MustChangePwd", mustChangePwd);
                cmd.Parameters.AddWithValue("@RoleId", roleIdParam);
                cmd.Parameters.AddWithValue("@RequireGeoTagging", requireGeo);
                cmd.Parameters.AddWithValue("@EnableEmailAlerts", requireEmails);
                cmd.Parameters.AddWithValue("@EnableWhatsAppAlerts", requireWhatsApp);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

                cn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0)
                {
                    InsertSystemNotification("User Profile Updated", $"Profile updated for User: {newName}.", "User Management", "Info", Session["USERID"]?.ToString() ?? "System");
                }
            }

            ShowOk("User details updated successfully.");
            lvUsers.EditIndex = -1;
            BindGrid();
        }

        protected void lvUsers_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;
                DataRowView drv = (DataRowView)dataItem.DataItem;

                if (lvUsers.EditIndex == dataItem.DisplayIndex)
                {
                    using (var cn = new SqlConnection(ConnString))
                    {
                        // Secure Dropdown Lookups using CompanyContext
                        int compId = CompanyContext.CurrentCompanyID;

                        DropDownList ddlGridRole = e.Item.FindControl("ddlGridRole") as DropDownList;
                        HiddenField hfCurrentRoleId = e.Item.FindControl("hfCurrentRoleId") as HiddenField;

                        if (ddlGridRole != null)
                        {
                            using (var cmd = new SqlCommand("SELECT RoleId, RoleName FROM Roles ORDER BY RoleName", cn))
                            {
                                var dt = new DataTable();
                                new SqlDataAdapter(cmd).Fill(dt);
                                ddlGridRole.DataSource = dt;
                                ddlGridRole.DataTextField = "RoleName";
                                ddlGridRole.DataValueField = "RoleId";
                                ddlGridRole.DataBind();
                            }
                            ddlGridRole.Items.Insert(0, new ListItem("-- Unassigned --", "0"));

                            if (hfCurrentRoleId != null && !string.IsNullOrEmpty(hfCurrentRoleId.Value))
                            {
                                ddlGridRole.SelectedValue = hfCurrentRoleId.Value;
                            }
                        }

                        DropDownList ddlDept = e.Item.FindControl("ddlDepartment") as DropDownList;
                        HiddenField hfDeptId = e.Item.FindControl("hfDeptId") as HiddenField;
                        if (ddlDept != null)
                        {
                            using (var cmdD = new SqlCommand("SELECT DepartmentID, DepartmentName FROM tbl_Departments WHERE IsActive = 1 AND CompanyID = @CompID", cn))
                            {
                                cmdD.Parameters.AddWithValue("@CompID", compId);
                                var dtD = new DataTable();
                                new SqlDataAdapter(cmdD).Fill(dtD);
                                ddlDept.DataSource = dtD;
                                ddlDept.DataTextField = "DepartmentName";
                                ddlDept.DataValueField = "DepartmentID";
                                ddlDept.DataBind();
                            }
                            ddlDept.Items.Insert(0, new ListItem("-- Select Dept --", ""));
                            if (hfDeptId != null && !string.IsNullOrEmpty(hfDeptId.Value)) ddlDept.SelectedValue = hfDeptId.Value;
                        }

                        DropDownList ddlDesig = e.Item.FindControl("ddlDesignation") as DropDownList;
                        HiddenField hfDesigId = e.Item.FindControl("hfDesigId") as HiddenField;
                        if (ddlDesig != null)
                        {
                            using (var cmdD = new SqlCommand("SELECT DesignationID, DesignationName FROM tbl_Designations WHERE IsActive = 1 AND CompanyID = @CompID", cn))
                            {
                                cmdD.Parameters.AddWithValue("@CompID", compId);
                                var dtD = new DataTable();
                                new SqlDataAdapter(cmdD).Fill(dtD);
                                ddlDesig.DataSource = dtD;
                                ddlDesig.DataTextField = "DesignationName";
                                ddlDesig.DataValueField = "DesignationID";
                                ddlDesig.DataBind();
                            }
                            ddlDesig.Items.Insert(0, new ListItem("-- Select Desig --", ""));
                            if (hfDesigId != null && !string.IsNullOrEmpty(hfDesigId.Value)) ddlDesig.SelectedValue = hfDesigId.Value;
                        }

                        DropDownList ddlManager = e.Item.FindControl("ddlManager") as DropDownList;
                        HiddenField hfManagerId = e.Item.FindControl("hfManagerId") as HiddenField;
                        if (ddlManager != null)
                        {
                            // Get the logged-in user to determine access level
                            string loggedInUser = Session["USERID"] != null ? Session["USERID"].ToString() : "";

                            // Base query restricted by CompanyContext
                            string managerSql = "SELECT User_Id, Name FROM tbl_login WHERE IsActive = 1 AND CompanyID = @CompID";

                            // Append exclusion if the current user is NOT admin
                            if (!loggedInUser.Equals("admin", StringComparison.OrdinalIgnoreCase))
                            {
                                managerSql += " AND User_Id NOT IN ('admin', 'AT01')";
                            }

                            using (var cmdM = new SqlCommand(managerSql, cn))
                            {
                                cmdM.Parameters.AddWithValue("@CompID", CompanyContext.CurrentCompanyID);
                                var dtM = new DataTable();
                                new SqlDataAdapter(cmdM).Fill(dtM);
                                ddlManager.DataSource = dtM;
                                ddlManager.DataTextField = "Name";
                                ddlManager.DataValueField = "User_Id";
                                ddlManager.DataBind();
                            }

                            ddlManager.Items.Insert(0, new ListItem("-- No Manager --", ""));
                            if (hfManagerId != null && !string.IsNullOrEmpty(hfManagerId.Value))
                            {
                                ddlManager.SelectedValue = hfManagerId.Value;
                            }
                        }
                    }
                    return;
                }

                bool isActive = !drv.Row.IsNull("IsActive") && Convert.ToBoolean(drv["IsActive"]);
                bool isLocked = false;
                if (!drv.Row.IsNull("LockoutEnd"))
                {
                    var val = drv["LockoutEnd"];
                    if (val is DateTimeOffset) isLocked = ((DateTimeOffset)val) > DateTimeOffset.UtcNow;
                    else isLocked = Convert.ToDateTime(val) > DateTime.UtcNow;
                }

                LinkButton lnkToggle = e.Item.FindControl("lnkToggleActive") as LinkButton;
                if (lnkToggle != null)
                {
                    lnkToggle.Text = isActive ? "Deactivate" : "Activate";
                    lnkToggle.CssClass = isActive ? "action-btn btn-danger" : "action-btn btn-success";
                }

                LinkButton lnkLock = e.Item.FindControl("lnkLock") as LinkButton;
                if (lnkLock != null)
                {
                    lnkLock.Text = isLocked ? "Unlock" : "Lock";
                    lnkLock.CssClass = isLocked ? "action-btn btn-warning" : "action-btn btn-warning";
                }
            }
        }

        protected void lvUsers_ItemCommand(object sender, ListViewCommandEventArgs e)
        {
            if (e.CommandName == "Edit" || e.CommandName == "Cancel" || e.CommandName == "Update")
                return;

            int id = Convert.ToInt32(e.CommandArgument);
            switch (e.CommandName)
            {
                case "ToggleActive": ToggleActive(id); break;
                case "ResetPassword": ResetPassword(id); break;
                case "DeleteUser": DeleteUser(id); break;
                case "ToggleLock": ToggleLock(id); break;
                case "MenuEdit":
                    string userId = GetUserIdById(id);
                    Response.Redirect("~/corporate/business/app/Update_Designation.aspx?User_Id=" + userId, false);
                    break;
            }
            BindGrid();
        }
        #endregion

        private string GetUserIdById(int id)
        {
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand("SELECT User_Id FROM dbo.tbl_login WHERE Id = @Id AND CompanyID = @CompanyID", cn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                cn.Open();
                var obj = cmd.ExecuteScalar();
                return obj == null ? "" : obj.ToString();
            }
        }

        private void ToggleActive(int id)
        {
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand("UPDATE dbo.tbl_login SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END WHERE Id = @Id AND CompanyID = @CompanyID", cn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                cn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0)
                {
                    InsertSystemNotification("User Status Changed", $"An employee account status was toggled.", "User Management", "Warning", Session["USERID"]?.ToString() ?? "System");
                    ShowOk("User active status updated.");
                }
            }
        }

        private void ToggleLock(int id)
        {
            const string sqlSelect = "SELECT LockoutEnd FROM dbo.tbl_login WHERE Id = @Id AND CompanyID = @CompanyID";
            using (SqlConnection cn = new SqlConnection(ConnString))
            {
                cn.Open();
                bool currentlyLocked = false;

                using (SqlCommand cmd = new SqlCommand(sqlSelect, cn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    object obj = cmd.ExecuteScalar();

                    if (obj != null && obj != DBNull.Value)
                    {
                        if (obj is DateTimeOffset) currentlyLocked = ((DateTimeOffset)obj) > DateTimeOffset.UtcNow;
                        else currentlyLocked = Convert.ToDateTime(obj) > DateTime.UtcNow;
                    }
                }

                if (currentlyLocked)
                {
                    using (var upd = new SqlCommand("UPDATE dbo.tbl_login SET LockoutEnd = NULL, FailedAccessCount = 0 WHERE Id = @Id AND CompanyID = @CompanyID", cn))
                    {
                        upd.Parameters.AddWithValue("@Id", id);
                        upd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        upd.ExecuteNonQuery();
                    }
                    InsertSystemNotification("User Unlocked", $"An employee account was manually unlocked.", "Security", "Info", Session["USERID"]?.ToString() ?? "System");
                    ShowOk("User unlocked.");
                }
                else
                {
                    DateTimeOffset lockUntil = DateTimeOffset.UtcNow.AddYears(100);
                    using (var upd = new SqlCommand("UPDATE dbo.tbl_login SET LockoutEnd = @LockoutEnd WHERE Id = @Id AND CompanyID = @CompanyID", cn))
                    {
                        upd.Parameters.AddWithValue("@LockoutEnd", lockUntil);
                        upd.Parameters.AddWithValue("@Id", id);
                        upd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        upd.ExecuteNonQuery();
                    }
                    InsertSystemNotification("User Locked", $"An employee account was manually locked.", "Security", "Danger", Session["USERID"]?.ToString() ?? "System");
                    ShowOk("User locked.");
                }
            }
        }

        private void DeleteUser(int id)
        {
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand("DELETE FROM dbo.tbl_login WHERE Id = @Id AND CompanyID = @CompanyID", cn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                cn.Open();
                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                {
                    InsertSystemNotification("User Deleted", $"An employee account was deleted from the system.", "User Management", "Danger", Session["USERID"]?.ToString() ?? "System");
                    ShowOk("User deleted.");
                }
                else ShowError("User not found or access denied.");
            }
        }

        private void ResetPassword(int id)
        {
            string tempPassword = GenerateTempPassword(10);
            byte[] salt = GenerateSalt(16);
            byte[] hash = HashPassword(tempPassword, salt);

            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(@"UPDATE dbo.tbl_login 
                                              SET PasswordHash = @Hash, PasswordSalt = @Salt, MustChangePassword = 1
                                              WHERE Id = @Id AND CompanyID = @CompanyID", cn))
            {
                cmd.Parameters.AddWithValue("@Hash", hash);
                cmd.Parameters.AddWithValue("@Salt", salt);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                cn.Open();

                if (cmd.ExecuteNonQuery() > 0)
                {
                    InsertSystemNotification("Password Reset", $"Admin triggered a password reset for an employee.", "Security", "Warning", Session["USERID"]?.ToString() ?? "System");
                }
            }

            string userId, email;
            bool isFirstTime;
            GetUserRecordById(id, out userId, out email, out isFirstTime);

            if (!string.IsNullOrEmpty(email))
            {
                try
                {
                    SendTempPasswordEmail(email, userId, tempPassword, isFirstTime);
                    ShowOk("Credentials generated and successfully emailed to user.");
                }
                catch (Exception ex)
                {
                    ShowError("Credentials reset, but failed to send email: " + ex.Message);
                }
            }
            else
            {
                ShowOk("Credentials reset. Note: User has no email address configured.");
            }
        }

        private void GetUserRecordById(int id, out string userId, out string email, out bool isFirstTime)
        {
            userId = string.Empty;
            email = string.Empty;
            isFirstTime = true;

            const string sql = "SELECT User_Id, Email, LastLogin FROM dbo.tbl_login WHERE Id = @Id AND CompanyID = @CompanyID";
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                cn.Open();
                using (var rdr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (rdr.Read())
                    {
                        int idxUser = rdr.GetOrdinal("User_Id");
                        int idxEmail = rdr.GetOrdinal("Email");
                        int idxLastLogin = rdr.GetOrdinal("LastLogin");

                        userId = rdr.IsDBNull(idxUser) ? string.Empty : rdr.GetString(idxUser);
                        email = rdr.IsDBNull(idxEmail) ? string.Empty : rdr.GetString(idxEmail);
                        isFirstTime = rdr.IsDBNull(idxLastLogin);
                    }
                }
            }
        }

        private void SendTempPasswordEmail(string toEmail, string userId, string tempPassword, bool isFirstTime)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new Exception("Cannot send email: user has no email.");

            string fromApp = ConfigurationManager.AppSettings["SmtpFrom"] ?? "Flame-Ex ERP Mailer | Aminrup Technologies";
            string smtpUserApp = ConfigurationManager.AppSettings["SmtpUser"] ?? "it.support@aminruptechnologies.co.in";
            string smtpPassApp = ConfigurationManager.AppSettings["SmtpPass"] ?? "TPw800QrVMU2";
            string smtpHostApp = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.zoho.in";

            int smtpPortApp = 587;
            int p;
            if (int.TryParse(ConfigurationManager.AppSettings["SmtpPort"], out p)) smtpPortApp = p;

            bool smtpEnableSsl = true;
            bool s;
            if (bool.TryParse(ConfigurationManager.AppSettings["SmtpEnableSsl"], out s)) smtpEnableSsl = s;

            string fromAddress = !string.IsNullOrWhiteSpace(fromApp) ? fromApp : smtpUserApp;
            string subject = isFirstTime ? "Welcome to Flame-Ex ERP - Your Login Credentials" : "Password Reset for your Flame-Ex ERP Account";
            string contextMessage = isFirstTime
                ? "An account has been created for you. Use the temporary credentials below to sign in for the first time."
                : "An administrator has reset your password. Use the temporary credentials below to regain access.";

            string plainTextBody = string.Format(
                "Hello {0},\r\n\r\n{1}\r\n\r\nUser Id: {2}\r\nTemporary Password: {3}\r\n\r\nFor security, you will be required to change your password immediately upon login.\r\n\r\n--\r\nThis is an automated message. Do not reply.",
                userId, contextMessage, userId, tempPassword);

            string htmlBody = string.Format(
                "<html><body style='font-family: Arial, sans-serif; color: #333;'><div style='border:1px solid #ddd; padding:20px; border-radius:5px; max-width:600px;'><h2 style='color:#006699;'>Flame-Ex ERP Access</h2><p>Hello <strong>{0}</strong>,</p><p>{1}</p><div style='background:#f9f9f9; padding:15px; border-left:4px solid #006699; margin:20px 0;'><strong>User Id:</strong> {2}<br/><strong>Temporary Password:</strong> <span style='color:#d9534f; font-weight:bold;'>{3}</span></div><p>For security, you will be required to configure a new password immediately upon login.</p><hr style='border:none; border-top:1px solid #eee; margin-top:20px;'/><p style='font-size:11px;color:#999'>This is an automated message from Aminrup Technologies. Please do not reply.</p></div></body></html>",
                userId, contextMessage, userId, tempPassword);

            try { System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12; } catch { }

            using (var message = new MailMessage())
            {
                message.From = new MailAddress(fromAddress);
                message.To.Add(toEmail);
                message.Subject = subject;
                message.SubjectEncoding = Encoding.UTF8;
                message.Body = plainTextBody;
                message.BodyEncoding = Encoding.UTF8;
                message.IsBodyHtml = false;

                message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(plainTextBody, Encoding.UTF8, MediaTypeNames.Text.Plain));
                message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, MediaTypeNames.Text.Html));

                using (var smtp = new SmtpClient(smtpHostApp, smtpPortApp))
                {
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.EnableSsl = smtpEnableSsl;
                    smtp.Timeout = 20000;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(smtpUserApp, smtpPassApp);
                    smtp.Send(message);
                }
            }
        }

        public string GetOnlineStatusHtml(object heartbeatObj)
        {
            if (heartbeatObj == null || heartbeatObj == DBNull.Value)
                return "<span class='badge' style='background:#f1f3f5; color:#666; border:1px solid #ddd;'>⚪ Offline</span>";

            DateTime utcHeartbeat;
            if (heartbeatObj is DateTimeOffset) utcHeartbeat = ((DateTimeOffset)heartbeatObj).UtcDateTime;
            else
            {
                utcHeartbeat = Convert.ToDateTime(heartbeatObj);
                if (utcHeartbeat.Kind == DateTimeKind.Unspecified) utcHeartbeat = DateTime.SpecifyKind(utcHeartbeat, DateTimeKind.Utc);
            }

            if (utcHeartbeat > DateTime.UtcNow.AddMinutes(5)) utcHeartbeat = utcHeartbeat.AddHours(-5).AddMinutes(-30);

            TimeSpan diff = DateTime.UtcNow - utcHeartbeat;

            if (diff.TotalMinutes >= 0 && diff.TotalMinutes <= 10)
            {
                return "<span class='badge' style='background:#e6f9ed; color:#2b8a3e; border:1px solid #b2f2bb; cursor:pointer;' title='Click to view history'>🟢 Online</span>";
            }
            else
            {
                TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime istTime = TimeZoneInfo.ConvertTimeFromUtc(utcHeartbeat, istZone);
                return string.Format("<span class='badge' style='background:#f1f3f5; color:#666; border:1px solid #ddd; cursor:pointer;' title='Last active: {0:dd MMM, hh:mm tt}'>⚪ Offline</span>", istTime);
            }
        }

        [System.Web.Services.WebMethod(EnableSession = true)]
        public static string GetSessionHistory(int userId)
        {
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            System.Collections.Generic.List<object> sessions = new System.Collections.Generic.List<object>();

            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            using (SqlConnection cn = new SqlConnection(connStr))
            {
                string sql = @"
                    SELECT TOP 15 LoginTime, LastHeartbeat, IPAddress, UserAgent, IsActive 
                    FROM ActiveSessions 
                    WHERE UserId = @UserId 
                    ORDER BY LoginTime DESC";

                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            DateTime utcLogin;
                            object loginObj = rdr["LoginTime"];
                            if (loginObj is DateTimeOffset) utcLogin = ((DateTimeOffset)loginObj).UtcDateTime;
                            else utcLogin = DateTime.SpecifyKind(Convert.ToDateTime(loginObj), DateTimeKind.Utc);

                            DateTime utcHeartbeat;
                            object hbObj = rdr["LastHeartbeat"];
                            if (hbObj is DateTimeOffset) utcHeartbeat = ((DateTimeOffset)hbObj).UtcDateTime;
                            else utcHeartbeat = DateTime.SpecifyKind(Convert.ToDateTime(hbObj), DateTimeKind.Utc);

                            if (utcLogin > DateTime.UtcNow.AddMinutes(5)) utcLogin = utcLogin.AddHours(-5).AddMinutes(-30);
                            if (utcHeartbeat > DateTime.UtcNow.AddMinutes(5)) utcHeartbeat = utcHeartbeat.AddHours(-5).AddMinutes(-30);

                            DateTime loginIst = TimeZoneInfo.ConvertTimeFromUtc(utcLogin, istZone);
                            DateTime heartbeatIst = TimeZoneInfo.ConvertTimeFromUtc(utcHeartbeat, istZone);

                            sessions.Add(new
                            {
                                LoginTime = loginIst.ToString("dd MMM yyyy, hh:mm tt"),
                                LastHeartbeat = heartbeatIst.ToString("dd MMM yyyy, hh:mm tt"),
                                IPAddress = rdr["IPAddress"] != DBNull.Value ? rdr["IPAddress"].ToString() : "Unknown",
                                UserAgent = rdr["UserAgent"] != DBNull.Value ? rdr["UserAgent"].ToString() : "Unknown",
                                IsActive = Convert.ToBoolean(rdr["IsActive"])
                            });
                        }
                    }
                }
            }
            return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(sessions);
        }

        #region Helpers
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

        [WebMethod]
        public static string SaveGeoFence(int userId, decimal lat, decimal lng, int radius, bool allowFallback, int maxAttempts)
        {
            try
            {
                int currentCompanyId = CompanyContext.CurrentCompanyID;
                string currentUserId = HttpContext.Current.Session["USERID"] != null ? HttpContext.Current.Session["USERID"].ToString() : "System";

                if (radius < 10) radius = 10;
                if (radius > 10000) radius = 10000;
                if (maxAttempts < 1) maxAttempts = 1;
                if (maxAttempts > 10) maxAttempts = 10;

                string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string query = @"
                UPDATE tbl_login 
                SET GeoFenceLat = @Lat, 
                    GeoFenceLng = @Lng, 
                    GeoFenceRadius = @Radius,
                    RequireGeoTagging = 1,
                    AllowGeoFenceOverride = @AllowFallback,
                    MaxGeoFenceAttempts = @MaxAttempts
                WHERE Id = @UserId 
                  AND CompanyID = @CompanyID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Lat", lat);
                        cmd.Parameters.AddWithValue("@Lng", lng);
                        cmd.Parameters.AddWithValue("@Radius", radius);
                        cmd.Parameters.AddWithValue("@AllowFallback", allowFallback);
                        cmd.Parameters.AddWithValue("@MaxAttempts", maxAttempts);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = currentCompanyId });

                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            InsertSystemNotification(
                                "Geo-Fence Configured",
                                $"Geo-Fence configured for User {userId} with a {radius}m radius. Override Allowed: {allowFallback}.",
                                "Attendance Settings",
                                "Info",
                                currentUserId
                            );

                            return "Success";
                        }
                        else
                        {
                            return "Update failed. User not found or belongs to a different Company.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private static void InsertSystemNotification(string title, string message, string moduleCode, string severity, string userId)
        {
            // 1. Enforce Multi-Tenant Isolation
            int currentCompanyId = CompanyContext.CurrentCompanyID;
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // 2. Aligned to exact LIVE database schema for tbl_SystemNotification
                string query = @"INSERT INTO tbl_SystemNotification 
                            (Title, Message, ModuleCode, Severity, CreatedBy, StartDate, EndDate, IsActive, CompanyID) 
                         VALUES 
                            (@Title, @Message, @ModuleCode, @Severity, @CreatedBy, GETDATE(), DATEADD(day, 30, GETDATE()), 1, @CompanyID)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Message", message);
                    cmd.Parameters.AddWithValue("@ModuleCode", moduleCode);
                    cmd.Parameters.AddWithValue("@Severity", severity); // e.g., 'Info', 'Success', 'Warning', 'Danger'

                    // Handle potentially null user IDs gracefully
                    cmd.Parameters.AddWithValue("@CreatedBy", string.IsNullOrEmpty(userId) ? (object)DBNull.Value : userId);

                    // Securely lock the notification to the active Company
                    cmd.Parameters.AddWithValue("@CompanyID", currentCompanyId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
        #endregion
    }
}