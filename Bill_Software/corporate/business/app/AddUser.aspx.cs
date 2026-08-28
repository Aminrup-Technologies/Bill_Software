using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm79 : System.Web.UI.Page
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
                LoadDropdowns();
                BindGrid();
            }
        }

        private void LoadDropdowns()
        {
            using (var cn = new SqlConnection(ConnString))
            {
                cn.Open();

                // 1. Load Roles (Ponytail #1: Tenant-scoped)
                using (var cmd = new SqlCommand("SELECT RoleId, RoleName FROM Roles WHERE CompanyID = @CompanyID ORDER BY RoleName", cn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    var dt = new DataTable(); new SqlDataAdapter(cmd).Fill(dt);
                    ddlRole.DataSource = dt; ddlRole.DataTextField = "RoleName"; ddlRole.DataValueField = "RoleId"; ddlRole.DataBind();
                    ddlRole.Items.Insert(0, new ListItem("-- Select System Role --", ""));
                }

                // 2. Load Departments (Ponytail #1: Tenant-scoped)
                using (var cmd = new SqlCommand("SELECT DepartmentID, DepartmentName FROM tbl_Departments WHERE IsActive = 1 AND CompanyID = @CompanyID ORDER BY DepartmentName", cn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    var dt = new DataTable(); new SqlDataAdapter(cmd).Fill(dt);
                    ddlDepartment.DataSource = dt; ddlDepartment.DataTextField = "DepartmentName"; ddlDepartment.DataValueField = "DepartmentID"; ddlDepartment.DataBind();
                    ddlDepartment.Items.Insert(0, new ListItem("-- None --", ""));
                }

                // 3. Load Designations (Ponytail #1: Tenant-scoped)
                using (var cmd = new SqlCommand("SELECT DesignationID, DesignationName FROM tbl_Designations WHERE IsActive = 1 AND CompanyID = @CompanyID ORDER BY DesignationName", cn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    var dt = new DataTable(); new SqlDataAdapter(cmd).Fill(dt);
                    ddlDesignation.DataSource = dt; ddlDesignation.DataTextField = "DesignationName"; ddlDesignation.DataValueField = "DesignationID"; ddlDesignation.DataBind();
                    ddlDesignation.Items.Insert(0, new ListItem("-- None --", ""));
                }

                // 4. Load Managers (Ponytail #1: Tenant-scoped)
                using (var cmd = new SqlCommand("SELECT User_Id, Name FROM tbl_login WHERE IsActive = 1 AND CompanyID = @CompanyID AND User_Id NOT IN ('admin', 'AT01') ORDER BY Name", cn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    var dt = new DataTable(); new SqlDataAdapter(cmd).Fill(dt);
                    ddlManager.DataSource = dt; ddlManager.DataTextField = "Name"; ddlManager.DataValueField = "User_Id"; ddlManager.DataBind();
                    ddlManager.Items.Insert(0, new ListItem("-- Select Manager --", ""));
                }
            }
        }

        private void BindGrid()
        {
            using (var cn = new SqlConnection(ConnString))
            {
                string cmdstring = @"
                    SELECT 
                        u.Id, 
                        u.User_Id, 
                        u.Name, 
                        u.Phone_no, 
                        u.Email, 
                        r.RoleName, 
                        d.DepartmentName,
                        des.DesignationName,
                        mgr.Name AS ManagerName
                    FROM tbl_login u
                    LEFT JOIN Roles r ON u.RoleId = r.RoleId
                    LEFT JOIN tbl_Departments d ON u.DepartmentID = d.DepartmentID
                    LEFT JOIN tbl_Designations des ON u.DesignationID = des.DesignationID
                    LEFT JOIN tbl_login mgr ON u.ReportingManagerId = mgr.User_Id
                    WHERE u.User_Id NOT IN ('admin', 'AT01') 
                      AND u.IsActive = 1 
                      AND u.CompanyID = @CompanyID
                    ORDER BY u.Id DESC";

                using (var cmd = new SqlCommand(cmdstring, cn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    gvRecentUsers.DataSource = dt;
                    gvRecentUsers.DataBind();
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Format for WhatsApp API: Strip spaces and dashes
                string cleanPhone = txtPhno.Text.Trim().Replace(" ", "").Replace("-", "");

                string idvalue = "FLM0" + GetNextIdValue();
                string tempPassword = txtPass.Text.Trim();

                byte[] passwordHash;
                byte[] passwordSalt;
                CreatePasswordHash(tempPassword, out passwordHash, out passwordSalt);

                // 1. Database Transaction (Wrap in using to ensure cleanup)
                using (var cn = new SqlConnection(ConnString))
                {
                    cn.Open();

                    // We use a Transaction so if the Leave Allocation fails, the User isn't created half-baked
                    using (SqlTransaction tran = cn.BeginTransaction())
                    {
                        try
                        {
                            // --- FIX 1: Added CompanyID to the INSERT statement ---
                            string query = @"INSERT INTO tbl_login 
                            (User_Id, Name, Phone_no, Email, PasswordHash, PasswordSalt, 
                             MustChangePassword, EmailVerified, IsActive, CreatedAt, 
                             RoleId, DepartmentID, DesignationID, ReportingManagerId, CompanyID) 
                             VALUES 
                            (@User_Id, @Name, @Phone_no, @Email, @PasswordHash, @PasswordSalt, 
                             1, 0, 1, sysutcdatetime(), 
                             @RoleId, @DeptId, @DesigId, @ManagerId, @CompanyID)";

                            using (var cmd = new SqlCommand(query, cn, tran))
                            {
                                cmd.Parameters.AddWithValue("@User_Id", idvalue);
                                cmd.Parameters.AddWithValue("@Name", txtEmployee.Text.Trim());
                                cmd.Parameters.AddWithValue("@Phone_no", cleanPhone);
                                cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                                cmd.Parameters.Add("@PasswordHash", SqlDbType.VarBinary, 256).Value = passwordHash;
                                cmd.Parameters.Add("@PasswordSalt", SqlDbType.VarBinary, 128).Value = passwordSalt;
                                cmd.Parameters.AddWithValue("@RoleId", ddlRole.SelectedValue);
                                cmd.Parameters.AddWithValue("@DeptId", string.IsNullOrEmpty(ddlDepartment.SelectedValue) ? (object)DBNull.Value : ddlDepartment.SelectedValue);
                                cmd.Parameters.AddWithValue("@DesigId", string.IsNullOrEmpty(ddlDesignation.SelectedValue) ? (object)DBNull.Value : ddlDesignation.SelectedValue);
                                cmd.Parameters.AddWithValue("@ManagerId", string.IsNullOrEmpty(ddlManager.SelectedValue) ? (object)DBNull.Value : ddlManager.SelectedValue);

                                // Strict Tenant Segregation
                                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

                                cmd.ExecuteNonQuery();
                            }

                            // --- 2. Allocate Leaves (Calendar Year) ---
                            int currentYear = DateTime.Now.Year;
                            using (SqlCommand cmdLeave = new SqlCommand("sp_AllocateEmployeeLeaves", cn, tran))
                            {
                                cmdLeave.CommandType = CommandType.StoredProcedure;
                                cmdLeave.Parameters.AddWithValue("@UserCode", idvalue);
                                cmdLeave.Parameters.AddWithValue("@FinancialYear", currentYear);
                                cmdLeave.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                                cmdLeave.ExecuteNonQuery();
                            }

                            // --- FIX 2: Proactive Notification Logging ---
                            string notifQuery = @"INSERT INTO tbl_SystemNotification 
                                        (Title, Message, ModuleCode, Severity, StartDate, EndDate, IsActive, CreatedBy, CompanyID)
                                        VALUES 
                                        ('New User Created', 'Account created for ' + @EmpName + ' (' + @EmpId + ')', 'Admin/Users', 'Success', GETDATE(), DATEADD(day, 30, GETDATE()), 1, @AdminId, @CompanyID)";

                            using (SqlCommand cmdNotif = new SqlCommand(notifQuery, cn, tran))
                            {
                                cmdNotif.Parameters.AddWithValue("@EmpName", txtEmployee.Text.Trim());
                                cmdNotif.Parameters.AddWithValue("@EmpId", idvalue);
                                cmdNotif.Parameters.AddWithValue("@AdminId", HttpContext.Current.Session["USERID"].ToString());
                                cmdNotif.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                                cmdNotif.ExecuteNonQuery();
                            }

                            // Commit the transaction if all 3 queries succeed
                            tran.Commit();
                        }
                        catch (Exception)
                        {
                            tran.Rollback();
                            throw; // Re-throw to be caught by outer handler
                        }
                    }
                }

                ShowMessage("✅ User created successfully! They will be asked to verify their email and reset password on first login.", true);
                ClearFields();
                BindGrid();
            }
            catch (Exception)
            {
                // Ponytail Standard #3: Never expose raw exception details to client
                ShowMessage("An unexpected error occurred while creating the user account. Please try again.", false);
            }
        }

        protected void gvRecentUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Inactivate")
            {
                int id = Convert.ToInt32(e.CommandArgument);
                using (var cn = new SqlConnection(ConnString))
                {
                    string query = "UPDATE tbl_login SET IsActive = 0 WHERE Id = @Id AND CompanyID = @CompanyID";
                    using (var cmd = new SqlCommand(query, cn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        cn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                ShowMessage("User removed and marked as inactive successfully.", true);
                BindGrid();
            }
        }

        private string GetNextIdValue()
        {
            string idvalue = "1";
            string query = "SELECT ISNULL(MAX(id), 0) + 1 FROM tbl_login";
            try
            {
                using (var cn = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand(query, cn))
                {
                    cn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null) idvalue = result.ToString();
                }
            }
            catch { }
            return idvalue;
        }

        private void ClearFields()
        {
            txtEmployee.Text = "";
            txtPhno.Text = "";
            txtEmail.Text = "";
            txtPass.Text = "";
            ddlRole.SelectedIndex = 0;
            ddlDepartment.SelectedIndex = 0;
            ddlDesignation.SelectedIndex = 0;
            ddlManager.SelectedIndex = 0;
        }

        private void ShowMessage(string msg, bool isSuccess)
        {
            if (isSuccess)
            {
                PanelOK.Visible = true;
                lblOk.Text = msg;
                PanelError.Visible = false;
            }
            else
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = msg;
                PanelOK.Visible = false;
            }
        }

        [System.Web.Services.WebMethod(EnableSession = true)]
        public static string CheckDuplicates(string email, string phone)
        {
            // Clean the phone number for strict matching
            string cleanPhone = phone.Trim().Replace(" ", "").Replace("-", "");
            string cleanEmail = email.Trim();

            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (var cn = new SqlConnection(connStr))
            {
                string query = "SELECT Email, Phone_no FROM tbl_login WHERE CompanyID = @CompanyID AND (Email = @Email OR Phone_no = @Phone)";
                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    cmd.Parameters.AddWithValue("@Email", cleanEmail);
                    cmd.Parameters.AddWithValue("@Phone", cleanPhone);

                    cn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string existingEmail = reader["Email"].ToString();
                            string existingPhone = reader["Phone_no"].ToString();

                            if (existingEmail.Equals(cleanEmail, StringComparison.OrdinalIgnoreCase))
                                return "Error: An employee with this Email Address already exists.";

                            if (existingPhone.Equals(cleanPhone, StringComparison.OrdinalIgnoreCase))
                                return "Error: An employee with this Phone Number already exists.";
                        }
                    }
                }
            }
            return "Valid";
        }

        #region Security Helpers
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                passwordSalt = new byte[128 / 8];
                rng.GetBytes(passwordSalt);
            }
            using (var derive = new Rfc2898DeriveBytes(password, passwordSalt, 100000))
            {
                passwordHash = derive.GetBytes(256 / 8);
            }
        }
        #endregion
    }
}