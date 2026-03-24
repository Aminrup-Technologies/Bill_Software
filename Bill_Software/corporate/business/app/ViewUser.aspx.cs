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
        private string ConnString => ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["USERID"] == null || Session["SessionToken"] == null)
            {
                Response.Redirect("~/index.aspx", false);
                return;
            }

            if (!IsPostBack)
            {
                //LoadEmployeeDropdown();
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

        //private void LoadEmployeeDropdown()
        //{
        //    using (var cn = new SqlConnection(ConnString))
        //    using (var cmd = new SqlCommand("SELECT User_Id FROM tbl_login WHERE User_Id NOT IN ('admin', 'AT01') ORDER BY Id", cn))
        //    {
        //        var dt = new DataTable();
        //        var da = new SqlDataAdapter(cmd);
        //        da.Fill(dt);
        //        ddlEmpId.Items.Clear();
        //        ddlEmpId.Items.Add(new ListItem("-- All --", ""));
        //        foreach (DataRow r in dt.Rows)
        //        {
        //            ddlEmpId.Items.Add(new ListItem(r["User_Id"].ToString(), r["User_Id"].ToString()));
        //        }
        //    }
        //}

        protected void ddlEmpId_SelectedIndexChanged(object sender, EventArgs e) => BindGrid();
        protected void btnRefresh_Click(object sender, EventArgs e) => BindGrid();

        private void BindGrid()
        {
            string sql = @"SELECT u.Id, u.User_Id, u.Name, u.Email, u.Phone_no, u.IsActive, u.LockoutEnd, 
                          u.LastLogin, u.CreatedAt, u.MustChangePassword, u.EmailVerified,
                          u.ProfilePictureUrl, u.RoleId, r.RoleName, u.RequireGeoTagging
                   FROM dbo.tbl_login u
                   LEFT JOIN dbo.Roles r ON u.RoleId = r.RoleId
                   WHERE (u.User_Id NOT IN ('admin', 'AT01')) ";

            // --- 1. APPLY BUTTON FILTERS ---
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
                // Safe check for both datetime and datetimeoffset locking mechanisms
                sql += " AND (u.LockoutEnd IS NOT NULL AND u.LockoutEnd > SYSDATETIMEOFFSET()) ";
            }

            // --- 2. APPLY TEXT SEARCH ---
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
            DropDownList ddlGridRole = item.FindControl("ddlGridRole") as DropDownList;

            string newName = txtName != null ? txtName.Text.Trim() : null;
            string newEmail = txtEmail != null ? txtEmail.Text.Trim() : null;
            string newPhone = txtPhone != null ? txtPhone.Text.Trim() : null;
            bool emailVerified = chkEmail != null ? chkEmail.Checked : false;
            bool mustChangePwd = chkPwd != null ? chkPwd.Checked : false;

            // Add this with the other control definitions
            CheckBox chkGeo = item.FindControl("chkRequireGeo") as CheckBox;
            bool requireGeo = chkGeo != null ? chkGeo.Checked : true;

            object roleIdParam = DBNull.Value;
            if (ddlGridRole != null && ddlGridRole.SelectedValue != "0")
            {
                roleIdParam = Convert.ToInt32(ddlGridRole.SelectedValue);
            }

            string updateSql = @"UPDATE dbo.tbl_login 
                         SET Name = @Name, Email = @Email, Phone_no = @Phone, 
                             EmailVerified = @EmailVerified, MustChangePassword = @MustChangePwd, 
                             RoleId = @RoleId, RequireGeoTagging = @RequireGeoTagging
                         WHERE Id = @Id";

            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(updateSql, cn))
            {
                cmd.Parameters.AddWithValue("@Name", newName != null ? (object)newName : DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", newEmail != null ? (object)newEmail : DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", newPhone != null ? (object)newPhone : DBNull.Value);
                cmd.Parameters.AddWithValue("@EmailVerified", emailVerified);
                cmd.Parameters.AddWithValue("@MustChangePwd", mustChangePwd);
                cmd.Parameters.AddWithValue("@RoleId", roleIdParam);
                cmd.Parameters.AddWithValue("@RequireGeoTagging", requireGeo); // NEW
                cmd.Parameters.AddWithValue("@Id", id);

                cn.Open();
                cmd.ExecuteNonQuery();
            }

            ShowOk("User details and roles updated successfully.");
            lvUsers.EditIndex = -1;
            BindGrid();
        }

        protected void lvUsers_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;
                DataRowView drv = (DataRowView)dataItem.DataItem;

                // Populate Dropdown if in Edit Mode
                if (lvUsers.EditIndex == dataItem.DisplayIndex)
                {
                    DropDownList ddlGridRole = e.Item.FindControl("ddlGridRole") as DropDownList;
                    HiddenField hfCurrentRoleId = e.Item.FindControl("hfCurrentRoleId") as HiddenField;

                    if (ddlGridRole != null)
                    {
                        using (var cn = new SqlConnection(ConnString))
                        using (var cmd = new SqlCommand("SELECT RoleId, RoleName FROM Roles ORDER BY RoleName", cn))
                        {
                            var dt = new DataTable();
                            var da = new SqlDataAdapter(cmd);
                            da.Fill(dt);
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
                    return; // Stop here if in edit mode
                }

                // Style the Toggle/Lock Buttons for Normal View Mode
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

        //private void BindGrid()
        //{
        //    // ENRICHED SQL: Joins tbl_login with Roles to fetch RoleName and ProfilePictureUrl
        //    var sql = @"SELECT u.Id, u.User_Id, u.Name, u.Email, u.Phone_no, u.IsActive, u.LockoutEnd, 
        //               u.LastLogin, u.CreatedAt, u.MustChangePassword, u.EmailVerified,
        //               u.ProfilePictureUrl, u.RoleId, r.RoleName
        //        FROM dbo.tbl_login u
        //        LEFT JOIN dbo.Roles r ON u.RoleId = r.RoleId
        //        WHERE (@UserId = '' OR u.User_Id = @UserId)
        //        AND (u.User_Id NOT IN ('admin', 'AT01'))
        //        ORDER BY u.Id";

        //    using (var cn = new SqlConnection(ConnString))
        //    using (var cmd = new SqlCommand(sql, cn))
        //    {
        //        string selectedUser = ddlEmpId.SelectedValue;
        //        cmd.Parameters.AddWithValue("@UserId", selectedUser != null ? selectedUser : string.Empty);

        //        var dt = new DataTable();
        //        var da = new SqlDataAdapter(cmd);
        //        da.Fill(dt);
        //        gvUsers.DataSource = dt;
        //        gvUsers.DataBind();
        //    }
        //}

        #region Edit Grid Events

        //protected void gvUsers_RowEditing(object sender, GridViewEditEventArgs e)
        //{
        //    gvUsers.EditIndex = e.NewEditIndex;
        //    BindGrid();
        //}

        //protected void gvUsers_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        //{
        //    gvUsers.EditIndex = -1;
        //    BindGrid();
        //}

        //protected void gvUsers_RowUpdating(object sender, GridViewUpdateEventArgs e)
        //{
        //    int id = Convert.ToInt32(gvUsers.DataKeys[e.RowIndex].Value);
        //    GridViewRow row = gvUsers.Rows[e.RowIndex];

        //    // Classic C# 5.0 null checks
        //    TextBox txtName = row.FindControl("txtName") as TextBox;
        //    TextBox txtEmail = row.FindControl("txtEmail") as TextBox;
        //    TextBox txtPhone = row.FindControl("txtPhone") as TextBox;
        //    CheckBox chkEmail = row.FindControl("chkEmailVerified") as CheckBox;
        //    CheckBox chkPwd = row.FindControl("chkMustChangePwd") as CheckBox;
        //    DropDownList ddlGridRole = row.FindControl("ddlGridRole") as DropDownList;

        //    string newName = txtName != null ? txtName.Text.Trim() : null;
        //    string newEmail = txtEmail != null ? txtEmail.Text.Trim() : null;
        //    string newPhone = txtPhone != null ? txtPhone.Text.Trim() : null;
        //    bool emailVerified = chkEmail != null ? chkEmail.Checked : false;
        //    bool mustChangePwd = chkPwd != null ? chkPwd.Checked : false;

        //    // Get the selected Role ID
        //    object roleIdParam = DBNull.Value;
        //    if (ddlGridRole != null && ddlGridRole.SelectedValue != "0")
        //    {
        //        roleIdParam = Convert.ToInt32(ddlGridRole.SelectedValue);
        //    }

        //    string updateSql = @"UPDATE dbo.tbl_login 
        //                 SET Name = @Name, 
        //                     Email = @Email, 
        //                     Phone_no = @Phone, 
        //                     EmailVerified = @EmailVerified, 
        //                     MustChangePassword = @MustChangePwd,
        //                     RoleId = @RoleId
        //                 WHERE Id = @Id";

        //    using (var cn = new SqlConnection(ConnString))
        //    using (var cmd = new SqlCommand(updateSql, cn))
        //    {
        //        cmd.Parameters.AddWithValue("@Name", newName != null ? (object)newName : DBNull.Value);
        //        cmd.Parameters.AddWithValue("@Email", newEmail != null ? (object)newEmail : DBNull.Value);
        //        cmd.Parameters.AddWithValue("@Phone", newPhone != null ? (object)newPhone : DBNull.Value);
        //        cmd.Parameters.AddWithValue("@EmailVerified", emailVerified);
        //        cmd.Parameters.AddWithValue("@MustChangePwd", mustChangePwd);
        //        cmd.Parameters.AddWithValue("@RoleId", roleIdParam); // NEW: Saves the role!
        //        cmd.Parameters.AddWithValue("@Id", id);

        //        cn.Open();
        //        cmd.ExecuteNonQuery();
        //    }

        //    ShowOk("User details and roles updated successfully.");
        //    gvUsers.EditIndex = -1; // Exit edit mode
        //    BindGrid();
        //}

        #endregion

        protected void gvUsers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // Skip execution if we are in Edit Mode for this row
            //if (e.Row.RowType == DataControlRowType.DataRow && (e.Row.RowState & DataControlRowState.Edit) > 0)
            //    return;

            // NEW LOGIC: If we are in Edit Mode, populate the Role DropDownList
            if (e.Row.RowType == DataControlRowType.DataRow && (e.Row.RowState & DataControlRowState.Edit) > 0)
            {
                DropDownList ddlGridRole = e.Row.FindControl("ddlGridRole") as DropDownList;
                HiddenField hfCurrentRoleId = e.Row.FindControl("hfCurrentRoleId") as HiddenField;

                if (ddlGridRole != null)
                {
                    // Fetch Roles from Database
                    using (var cn = new SqlConnection(ConnString))
                    {
                        using (var cmd = new SqlCommand("SELECT RoleId, RoleName FROM Roles ORDER BY RoleName", cn))
                        {
                            var dt = new DataTable();
                            var da = new SqlDataAdapter(cmd);
                            da.Fill(dt);
                            ddlGridRole.DataSource = dt;
                            ddlGridRole.DataTextField = "RoleName";
                            ddlGridRole.DataValueField = "RoleId";
                            ddlGridRole.DataBind();
                        }
                    }
                    ddlGridRole.Items.Insert(0, new ListItem("-- Unassigned --", "0"));

                    // Pre-select the user's current Role
                    if (hfCurrentRoleId != null && !string.IsNullOrEmpty(hfCurrentRoleId.Value))
                    {
                        ddlGridRole.SelectedValue = hfCurrentRoleId.Value;
                    }
                }
                return; // Exit here, don't run the rest of the styling logic for the edit row
            }

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

        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // Edit, Cancel, and Update are handled automatically by the GridView events
            if (e.CommandName == "Edit" || e.CommandName == "Cancel" || e.CommandName == "Update")
                return;

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
                        currentlyLocked = ((DateTimeOffset)obj) > DateTimeOffset.Now;
                    else
                        currentlyLocked = Convert.ToDateTime(obj) > DateTime.UtcNow;
                }

                if (currentlyLocked)
                {
                    using (var upd = new SqlCommand("UPDATE dbo.tbl_login SET LockoutEnd = NULL, FailedAccessCount = 0 WHERE Id = @Id", cn))
                    {
                        upd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                        upd.ExecuteNonQuery();
                    }
                    ShowOk("User unlocked.");
                }
                else
                {
                    DateTimeOffset lockUntil = DateTimeOffset.UtcNow.AddYears(100);
                    using (var upd = new SqlCommand("UPDATE dbo.tbl_login SET LockoutEnd = @LockoutEnd WHERE Id = @Id", cn))
                    {
                        upd.Parameters.Add("@LockoutEnd", SqlDbType.DateTimeOffset).Value = lockUntil;
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

        private void ResetPassword(int id)
        {
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

            const string sql = "SELECT User_Id, Email, LastLogin FROM dbo.tbl_login WHERE Id = @Id";
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
            if (int.TryParse(ConfigurationManager.AppSettings["SmtpPort"], out p))
            {
                smtpPortApp = p;
            }

            bool smtpEnableSsl = true;
            bool s;
            if (bool.TryParse(ConfigurationManager.AppSettings["SmtpEnableSsl"], out s))
            {
                smtpEnableSsl = s;
            }

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
        #endregion
    }
}