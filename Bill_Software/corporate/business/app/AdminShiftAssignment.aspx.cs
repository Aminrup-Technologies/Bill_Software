using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace Bill_Software.corporate.business.app
{
    public partial class AdminShiftAssignment : System.Web.UI.Page
    {
        private string ConnString
        {
            get { return ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
                return;
            }

            if (!IsPostBack)
            {
                PopulateYearDropdown();
                LoadDropdowns();

                ddlMonth.SelectedValue = DateTime.Now.Month.ToString();
                ddlYear.SelectedValue = DateTime.Now.Year.ToString();

                LoadAssignmentsGrid();
            }
        }

        private void PopulateYearDropdown()
        {
            int currentYear = DateTime.Now.Year;
            for (int i = currentYear - 1; i <= currentYear + 2; i++)
            {
                ddlYear.Items.Add(new ListItem(i.ToString(), i.ToString()));
            }
        }

        private void LoadDropdowns()
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();

                // 1. Load Employees (Company Isolated)
                string empQuery = "SELECT User_Id, Name + ' [' + User_Id + ']' as DisplayName FROM tbl_login WHERE CompanyID = @CompanyID AND IsActive = 1 AND User_Id NOT IN ('admin', 'AT01') ORDER BY Name";
                using (SqlCommand cmd = new SqlCommand(empQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    ddlEmployee.DataSource = dt;
                    ddlEmployee.DataTextField = "DisplayName";
                    ddlEmployee.DataValueField = "User_Id";
                    ddlEmployee.DataBind();
                }
                // Add BULK assignment option
                //ddlEmployee.Items.Insert(0, new ListItem("-- ALL EMPLOYEES (Bulk Assign) --", "ALL"));

                // 2. Load Active Shifts (Company Isolated)
                string shiftQuery = "SELECT ShiftID, ShiftName + ' (' + CONVERT(varchar(5), StartTime, 108) + ' - ' + CONVERT(varchar(5), EndTime, 108) + ')' as ShiftDisplay FROM tbl_ShiftMaster WHERE CompanyID = @CompanyID AND IsActive = 1";
                using (SqlCommand cmd = new SqlCommand(shiftQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dtShift = new DataTable();
                    da.Fill(dtShift);

                    ddlShift.DataSource = dtShift;
                    ddlShift.DataTextField = "ShiftDisplay";
                    ddlShift.DataValueField = "ShiftID";
                    ddlShift.DataBind();
                }
            }
        }

        protected void btnAssignShift_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(ddlShift.SelectedValue))
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alert", "Swal.fire('Error', 'Please select a Shift to assign.', 'error');", true);
                    return;
                }

                // Verify at least one employee is selected
                bool hasSelection = false;
                foreach (ListItem item in ddlEmployee.Items)
                {
                    if (item.Selected) { hasSelection = true; break; }
                }

                if (!hasSelection)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alert", "Swal.fire('Error', 'Please select at least one Employee.', 'error');", true);
                    return;
                }

                int targetMonth = Convert.ToInt32(ddlMonth.SelectedValue);
                int targetYear = Convert.ToInt32(ddlYear.SelectedValue);
                int shiftId = Convert.ToInt32(ddlShift.SelectedValue);
                int companyId = CompanyContext.CurrentCompanyID;
                string adminId = Session["USERID"].ToString();

                DateTime fromDate = new DateTime(targetYear, targetMonth, 1);
                DateTime toDate = fromDate.AddMonths(1).AddDays(-1);

                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();

                    try
                    {
                        string sqlMerge = @"
                    DELETE FROM tbl_EmployeeShiftMapping 
                    WHERE UserCode = @UserCode 
                      AND MONTH(EffectiveFromDate) = @Month 
                      AND YEAR(EffectiveFromDate) = @Year 
                      AND CompanyID = @CompanyID;

                    INSERT INTO tbl_EmployeeShiftMapping 
                    (UserCode, ShiftID, EffectiveFromDate, EffectiveToDate, AssignedBy, AssignedDate, CompanyID)
                    VALUES 
                    (@UserCode, @ShiftID, @FromDate, @ToDate, @AdminID, GETDATE(), @CompanyID);";

                        using (SqlCommand cmd = new SqlCommand(sqlMerge, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@ShiftID", shiftId);
                            cmd.Parameters.AddWithValue("@FromDate", fromDate);
                            cmd.Parameters.AddWithValue("@ToDate", toDate);
                            cmd.Parameters.AddWithValue("@Month", targetMonth);
                            cmd.Parameters.AddWithValue("@Year", targetYear);
                            cmd.Parameters.AddWithValue("@AdminID", adminId);
                            cmd.Parameters.AddWithValue("@CompanyID", companyId);

                            SqlParameter prmUserCode = cmd.Parameters.Add("@UserCode", SqlDbType.NVarChar, 100);

                            // Loop through the multiselect and save for EVERY checked employee
                            int count = 0;
                            foreach (ListItem item in ddlEmployee.Items)
                            {
                                if (item.Selected)
                                {
                                    prmUserCode.Value = item.Value;
                                    cmd.ExecuteNonQuery();
                                    count++;
                                }
                            }

                            // Notification
                            string notiQuery = @"INSERT INTO tbl_SystemNotification (Title, Message, ModuleCode, Severity, CreatedBy, StartDate, EndDate, IsActive, CompanyID) 
                                         VALUES ('Shift Roster Updated', @Msg, 'Attendance', 'Info', @AdminId, GETDATE(), DATEADD(day, 14, GETDATE()), 1, @CompanyID)";
                            using (SqlCommand cmdNoti = new SqlCommand(notiQuery, conn, tran))
                            {
                                cmdNoti.Parameters.AddWithValue("@Msg", $"Admin {adminId} assigned {ddlShift.SelectedItem.Text} to {count} employee(s) for {ddlMonth.SelectedItem.Text} {targetYear}.");
                                cmdNoti.Parameters.AddWithValue("@AdminId", adminId);
                                cmdNoti.Parameters.AddWithValue("@CompanyID", companyId);
                                cmdNoti.ExecuteNonQuery();
                            }
                        }

                        tran.Commit();
                        ScriptManager.RegisterStartupScript(this, GetType(), "alert", "Swal.fire('Success', 'Shift roster applied successfully!', 'success');", true);

                        LoadAssignmentsGrid();
                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", $"Swal.fire('Error', '{ex.Message.Replace("'", "")}', 'error');", true);
            }
        }

        // ==========================================
        // VIEW ROSTER LOGIC (Filtered)
        // ==========================================
        protected void btnViewAssignment_Click(object sender, EventArgs e)
        {
            try
            {
                int targetMonth = Convert.ToInt32(ddlMonth.SelectedValue);
                int targetYear = Convert.ToInt32(ddlYear.SelectedValue);
                lblCurrentMonth.Text = $"Filtered: {ddlMonth.SelectedItem.Text} {targetYear}";

                // Get the first selected user (if any)
                string selectedUser = null;
                foreach (ListItem item in ddlEmployee.Items)
                {
                    if (item.Selected)
                    {
                        selectedUser = item.Value;
                        break; // Just grab the first one for filtering the view
                    }
                }

                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    // Base query
                    string sql = @"
                SELECT 
                    u.Name AS EmployeeName, 
                    m.UserCode, 
                    s.ShiftName, 
                    m.EffectiveFromDate, 
                    m.EffectiveToDate
                FROM tbl_EmployeeShiftMapping m
                INNER JOIN tbl_login u ON m.UserCode = u.User_Id
                INNER JOIN tbl_ShiftMaster s ON m.ShiftID = s.ShiftID
                WHERE m.CompanyID = @CompanyID 
                  AND MONTH(m.EffectiveFromDate) = @Month 
                  AND YEAR(m.EffectiveFromDate) = @Year";

                    // If a specific user was selected in the ListBox, filter by them
                    if (!string.IsNullOrEmpty(selectedUser))
                    {
                        sql += " AND m.UserCode = @UserCode";
                    }

                    sql += " ORDER BY u.Name ASC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        cmd.Parameters.AddWithValue("@Month", targetMonth);
                        cmd.Parameters.AddWithValue("@Year", targetYear);

                        if (!string.IsNullOrEmpty(selectedUser))
                        {
                            cmd.Parameters.AddWithValue("@UserCode", selectedUser);
                        }

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        gvAssignments.DataSource = dt;
                        gvAssignments.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", $"Swal.fire('Error', 'Failed to filter view: {ex.Message.Replace("'", "")}', 'error');", true);
            }
        }

        // ==========================================
        // ON-DEMAND AUTO-CLOSURE SYNC (Using Stored Procedure)
        // ==========================================
        protected void btnForceSync_Click(object sender, EventArgs e)
        {
            try
            {
                int companyId = CompanyContext.CurrentCompanyID;
                string adminId = Session["USERID"].ToString();
                int targetMonth = Convert.ToInt32(ddlMonth.SelectedValue);
                int targetYear = Convert.ToInt32(ddlYear.SelectedValue);

                // Determine target users
                bool isAllEmployees = false;
                System.Collections.Generic.List<string> selectedUsers = new System.Collections.Generic.List<string>();

                foreach (ListItem item in ddlEmployee.Items)
                {
                    if (item.Selected)
                    {
                        if (item.Value == "ALL") isAllEmployees = true;
                        else selectedUsers.Add(item.Value);
                    }
                }

                if (selectedUsers.Count == 0) isAllEmployees = true;

                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();

                    try
                    {
                        int updatedCount = 0;

                        // CALL THE STORED PROCEDURE
                        using (SqlCommand cmd = new SqlCommand("sp_RunAttendanceRulesEngine", conn, tran))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@CompanyID", companyId);
                            cmd.Parameters.AddWithValue("@Month", targetMonth);
                            cmd.Parameters.AddWithValue("@Year", targetYear);

                            // Pass comma-separated string if specific users selected, else pass NULL
                            if (isAllEmployees)
                            {
                                cmd.Parameters.AddWithValue("@UserCodeList", DBNull.Value);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@UserCodeList", string.Join(",", selectedUsers));
                            }

                            // ExecuteScalar returns the @@ROWCOUNT from the SP
                            updatedCount = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Log the action if records were processed
                        if (updatedCount > 0)
                        {
                            string targetName = isAllEmployees ? "All Employees" : $"{selectedUsers.Count} selected employee(s)";
                            string notiQuery = @"INSERT INTO tbl_SystemNotification (Title, Message, ModuleCode, Severity, CreatedBy, StartDate, EndDate, IsActive, CompanyID) 
                                         VALUES ('Attendance Engine Synced', @Msg, 'Attendance', 'Info', @AdminId, GETDATE(), DATEADD(day, 14, GETDATE()), 1, @CompanyID)";
                            using (SqlCommand cmdNoti = new SqlCommand(notiQuery, conn, tran))
                            {
                                cmdNoti.Parameters.AddWithValue("@Msg", $"Admin {adminId} ran the Rules Engine via SP for {targetName} in {ddlMonth.SelectedItem.Text} {targetYear}. {updatedCount} records evaluated.");
                                cmdNoti.Parameters.AddWithValue("@AdminId", adminId);
                                cmdNoti.Parameters.AddWithValue("@CompanyID", companyId);
                                cmdNoti.ExecuteNonQuery();
                            }
                        }

                        tran.Commit();

                        ScriptManager.RegisterStartupScript(this, GetType(), "alert", $"Swal.fire('Engine Complete', 'Successfully processed {updatedCount} attendance records using the central Stored Procedure.', 'success');", true);
                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", $"Swal.fire('Error', 'Sync failed: {ex.Message.Replace("'", "")}', 'error');", true);
            }
        }

        protected void btnRefreshGrid_Click(object sender, EventArgs e)
        {
            LoadAssignmentsGrid();
        }

        private void LoadAssignmentsGrid()
        {
            try
            {
                int targetMonth = Convert.ToInt32(ddlMonth.SelectedValue);
                int targetYear = Convert.ToInt32(ddlYear.SelectedValue);
                lblCurrentMonth.Text = $"{ddlMonth.SelectedItem.Text} {targetYear}";

                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    string sql = @"
                        SELECT 
                            u.Name AS EmployeeName, 
                            m.UserCode, 
                            s.ShiftName, 
                            m.EffectiveFromDate, 
                            m.EffectiveToDate
                        FROM tbl_EmployeeShiftMapping m
                        INNER JOIN tbl_login u ON m.UserCode = u.User_Id
                        INNER JOIN tbl_ShiftMaster s ON m.ShiftID = s.ShiftID
                        WHERE m.CompanyID = @CompanyID 
                          AND MONTH(m.EffectiveFromDate) = @Month 
                          AND YEAR(m.EffectiveFromDate) = @Year
                        ORDER BY u.Name ASC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        cmd.Parameters.AddWithValue("@Month", targetMonth);
                        cmd.Parameters.AddWithValue("@Year", targetYear);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        gvAssignments.DataSource = dt;
                        gvAssignments.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", $"Swal.fire('Error', 'Failed to load grid: {ex.Message.Replace("'", "")}', 'error');", true);
            }
        }
    }
}