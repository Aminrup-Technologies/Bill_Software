using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;
using System.IO;
using ClosedXML.Excel;
using System.Web.UI;

namespace Bill_Software.corporate.business.app
{
    public partial class AdminAttendanceDashboard : System.Web.UI.Page
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
                LoadEmployees();

                ddlMonth.SelectedValue = DateTime.Now.Month.ToString();
                ddlYear.SelectedValue = DateTime.Now.Year.ToString();

                //GenerateReport();
            }
        }

        private void PopulateYearDropdown()
        {
            int currentYear = DateTime.Now.Year;
            for (int i = currentYear - 2; i <= currentYear; i++)
            {
                ddlYear.Items.Add(new ListItem(i.ToString(), i.ToString()));
            }
        }

        private void LoadEmployees()
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                string query = "SELECT User_Id, Name + ' [' + User_Id + ']' as DisplayName FROM tbl_login WHERE CompanyID = @CompanyID AND IsActive = 1 AND User_Id NOT IN ('admin', 'AT01') ORDER BY Name";
                using (SqlCommand cmd = new SqlCommand(query, conn))
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
                ddlEmployee.Items.Insert(0, new ListItem("-- All Employees --", "ALL"));
            }
        }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            GenerateReport();
        }

        private DataTable GetAttendanceData()
        {
            int targetMonth = Convert.ToInt32(ddlMonth.SelectedValue);
            int targetYear = Convert.ToInt32(ddlYear.SelectedValue);
            string selectedEmp = ddlEmployee.SelectedValue;
            int companyId = CompanyContext.CurrentCompanyID;

            string sql = @"
                WITH DateRange AS (
                    SELECT CAST(DATEFROMPARTS(@Year, @Month, 1) AS DATE) AS CalDate
                    UNION ALL
                    SELECT DATEADD(day, 1, CalDate)
                    FROM DateRange
                    WHERE CalDate < EOMONTH(DATEFROMPARTS(@Year, @Month, 1))
                ),
                TargetUsers AS (
                    SELECT User_Id, Name FROM tbl_login 
                    WHERE CompanyID = @CompanyID AND (@EmpId = 'ALL' OR User_Id = @EmpId) AND IsActive = 1
                ),
                UserDates AS (
                    SELECT u.User_Id, u.Name, d.CalDate 
                    FROM TargetUsers u CROSS JOIN DateRange d
                ),
                OfficeAttendance AS (
                    SELECT UserCode, ActivityDate, PunchInTime, PunchOutTime, TotalHoursWorked, 
                           SystemCalculatedStatus, AttendanceCode, PayableDay 
                    FROM tbl_Attendance 
                    WHERE CompanyID = @CompanyID AND MONTH(ActivityDate) = @Month AND YEAR(ActivityDate) = @Year
                ),
                FieldSales AS (
                    SELECT CreatedByCode as UserCode, CAST(VisitDate AS DATE) as VisitDate, 
                           COUNT(Id) as TotalVisits, SUM(RevenueRealized) as DailyRevenue
                    FROM tbl_SalesVisitReport 
                    WHERE CompanyID = @CompanyID AND MONTH(VisitDate) = @Month AND YEAR(VisitDate) = @Year
                    GROUP BY CreatedByCode, CAST(VisitDate AS DATE)
                ),
                LeaveData AS (
                    SELECT lr.UserCode, d.CalDate AS LeaveDate, lr.RequestStatus, lm.LeaveName AS LeaveTypeName
                    FROM tbl_LeaveRequests lr
                    LEFT JOIN tbl_LeaveMaster lm ON lr.LeaveID = lm.LeaveID
                    CROSS JOIN DateRange d
                    WHERE lr.CompanyID = @CompanyID 
                      AND lr.RequestStatus = 'Approved'
                      AND d.CalDate BETWEEN lr.StartDate AND lr.EndDate
                ),
                RegData AS (
                    -- Grab the latest regularization request per day based on AppliedOn
                    SELECT UserCode, CAST(AttendanceDate AS DATE) as RegDate, RequestStatus,
                           ROW_NUMBER() OVER(PARTITION BY UserCode, CAST(AttendanceDate AS DATE) ORDER BY AppliedOn DESC) as rn
                    FROM tbl_AttendanceRegularization
                    WHERE CompanyID = @CompanyID AND MONTH(AttendanceDate) = @Month AND YEAR(AttendanceDate) = @Year
                )
        
                SELECT 
                    ud.User_Id AS UserCode,
                    ud.CalDate AS ActivityDate,
                    DATENAME(weekday, ud.CalDate) AS DayOfWeek,
                    ud.Name AS EmployeeName,
                    oa.PunchInTime,
                    oa.PunchOutTime,
                    oa.TotalHoursWorked,
                    ISNULL(fs.TotalVisits, 0) AS FieldVisitsLogged,
                    ISNULL(fs.DailyRevenue, 0) AS DailyRevenue,
            
                    ISNULL(oa.AttendanceCode, '-') AS AttendanceCode,
                    ISNULL(oa.PayableDay, 0.0) AS PayableDay,
            
                    -- THE UNIFIED OMNI-STATUS HIERARCHY
                    CASE 
                        WHEN ud.CalDate > CAST(GETDATE() AS DATE) THEN 'Upcoming'
                        WHEN ld.RequestStatus = 'Approved' THEN 'On Leave (' + ISNULL(ld.LeaveTypeName, 'Leave') + ')'
                        WHEN rd.RequestStatus = 'Pending' THEN 'Regularization Pending'
                        WHEN rd.RequestStatus = 'Approved' AND oa.SystemCalculatedStatus IS NULL THEN 'Regularized (Awaiting Sync)'
                        WHEN oa.SystemCalculatedStatus IS NOT NULL THEN 'Office (' + oa.SystemCalculatedStatus + ')'
                        WHEN fs.TotalVisits > 0 THEN 'Field Sales'
                        WHEN DATENAME(weekday, ud.CalDate) = 'Sunday' THEN 'Weekly Off'
                        ELSE 'Absent'
                    END AS CalculatedStatus
            
                FROM UserDates ud
                LEFT JOIN OfficeAttendance oa ON ud.User_Id = oa.UserCode AND ud.CalDate = oa.ActivityDate
                LEFT JOIN FieldSales fs ON ud.User_Id = fs.UserCode AND ud.CalDate = fs.VisitDate
                LEFT JOIN LeaveData ld ON ud.User_Id = ld.UserCode AND ud.CalDate = ld.LeaveDate
                LEFT JOIN RegData rd ON ud.User_Id = rd.UserCode AND ud.CalDate = rd.RegDate AND rd.rn = 1
                ORDER BY ud.Name ASC, ud.CalDate ASC
                OPTION (MAXRECURSION 31);
            ";

            using (SqlConnection conn = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@CompanyID", companyId);
                cmd.Parameters.AddWithValue("@Month", targetMonth);
                cmd.Parameters.AddWithValue("@Year", targetYear);
                cmd.Parameters.AddWithValue("@EmpId", selectedEmp);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        private void GenerateReport()
        {
            DataTable dt = GetAttendanceData();
            gvOmniAttendance.DataSource = dt;
            gvOmniAttendance.DataBind();
            //CalculateSummaries(dt);
            CalculateSummaryCards(dt);
            if (dt != null && dt.Rows.Count > 0)
            {
                btnExport.Visible = true;
            }
            else
            {
                btnExport.Visible = false;
            }
        }

        // ==========================================
        // DYNAMIC GRID UI (Injecting Actions)
        // ==========================================
        protected void gvOmniAttendance_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // 1. Safely extract all underlying data values
                string status = DataBinder.Eval(e.Row.DataItem, "CalculatedStatus").ToString();
                string punchIn = DataBinder.Eval(e.Row.DataItem, "PunchInTime").ToString();
                string punchOut = DataBinder.Eval(e.Row.DataItem, "PunchOutTime").ToString();
                string userCode = DataBinder.Eval(e.Row.DataItem, "UserCode").ToString();
                DateTime activityDate = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "ActivityDate"));
                string dateStr = activityDate.ToString("yyyy-MM-dd");

                bool hasPunchIn = !string.IsNullOrWhiteSpace(punchIn);
                bool hasPunchOut = !string.IsNullOrWhiteSpace(punchOut);

                // ==========================================
                // 2. APPLY VISUAL INTELLIGENCE (Row Colors)
                // ==========================================
                if (status.Contains("Absent"))
                {
                    e.Row.BackColor = System.Drawing.Color.FromName("#ffebee"); // Light Red
                }
                else if (status.Contains("Regularization Pending"))
                {
                    e.Row.BackColor = System.Drawing.Color.FromName("#fff8e1"); // Light Orange/Yellow
                }
                else if (status.Contains("On Leave"))
                {
                    e.Row.BackColor = System.Drawing.Color.FromName("#f3e5f5"); // Light Purple
                }
                else if (status == "Upcoming" || status == "Weekly Off")
                {
                    e.Row.ForeColor = System.Drawing.Color.Gray; // Fade out non-working days
                }


                // ==========================================
                // 3. RENDER HR ACTION BUTTONS (With Safeguards)
                // ==========================================
                Literal litActions = (Literal)e.Row.FindControl("litActions");

                if (litActions != null)
                {
                    litActions.Text = ""; // Default to blank

                    // ENHANCEMENT: Prevent HR from overriding Future Dates, Approved Leaves, or Pending Regularizations!
                    bool isModifiable = activityDate <= DateTime.Now.Date
                                     && !status.Contains("On Leave")
                                     && !status.Contains("Regularization Pending");

                    if (isModifiable)
                    {
                        // Scenario 1: Completely Absent (No Data)
                        if (status.Equals("Absent", StringComparison.OrdinalIgnoreCase))
                        {
                            string presentBtn = $"<button type='button' class='btn-action-approve' style='padding:5px 10px; font-size:11px;' onclick=\"return openActionModal('{userCode}', '{dateStr}', 'Present');\">✔ Present</button>";
                            string absentBtn = $"<button type='button' class='btn-action-reject' style='padding:5px 10px; font-size:11px; margin-left:5px;' onclick=\"return openActionModal('{userCode}', '{dateStr}', 'Absent');\">✖ Absent</button>";

                            litActions.Text = presentBtn + absentBtn;
                        }

                        // Scenario 2: ORPHANED PUNCH (In exists, Out is missing)
                        // Note: If the Nightly Engine already Auto-Closed it, PunchOut won't be empty, so this won't trigger (which is correct!).
                        else if (hasPunchIn && !hasPunchOut)
                        {
                            string forceOutBtn = $"<button type='button' class='btn-action-approve' style='background-color:#fd7e14; padding:5px 10px; font-size:11px; margin-right:5px;' onclick=\"return openActionModal('{userCode}', '{dateStr}', 'ForceOut');\">⏱ Force Checkout</button>";
                            string absentBtn = $"<button type='button' class='btn-action-reject' style='padding:5px 10px; font-size:11px;' onclick=\"return openActionModal('{userCode}', '{dateStr}', 'Absent');\">✖ Mark Absent</button>";

                            litActions.Text = forceOutBtn + absentBtn;
                        }
                    }
                    else if (status.Contains("Regularization Pending"))
                    {
                        // Give HR a helpful hint instead of buttons
                        litActions.Text = "<span style='color:#fd7e14; font-size:11px; font-weight:bold;'>Awaiting Manager</span>";
                    }
                }
            }
        }

        // ==========================================
        // HR OVERRIDE ACTION HANDLER
        // ==========================================
        protected void btnConfirmAction_Click(object sender, EventArgs e)
        {
            try
            {
                string targetUser = hdnTargetUser.Value;
                string targetDate = hdnTargetDate.Value;
                string actionType = hdnActionType.Value; // 'Present', 'Absent', or 'ForceOut'
                string remarks = txtAdminRemarks.Text.Trim();
                string adminId = Session["USERID"].ToString();
                int companyId = CompanyContext.CurrentCompanyID;

                // 1. Universal Validation
                if (string.IsNullOrEmpty(remarks))
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alert", "Swal.fire('Error', 'Remarks are mandatory for manual overrides.', 'error');", true);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(ConnString))
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    conn.Open();

                    try
                    {
                        // ==========================================
                        // PATH A: FORCE CHECKOUT (Fixing an Orphaned Punch)
                        // ==========================================
                        if (actionType == "ForceOut")
                        {
                            string outTimeInput = txtManualOutTime.Text; // e.g., "18:30"

                            if (string.IsNullOrEmpty(outTimeInput))
                            {
                                ScriptManager.RegisterStartupScript(this, GetType(), "alert", "Swal.fire('Error', 'Please provide a valid Checkout Time.', 'error');", true);
                                tran.Rollback();
                                return;
                            }

                            // Safely stitch the Date and the HR's inputted Time together
                            DateTime combinedOutDateTime = Convert.ToDateTime($"{targetDate} {outTimeInput}:00");

                            // REMOVED 'Remarks' column, added 'AttendanceStatus' update
                            string updateQuery = @"UPDATE tbl_Attendance 
                                           SET PunchOutTime = @OutTime,
                                               TotalHoursWorked = CAST(DATEDIFF(MINUTE, PunchInTime, @OutTime) / 60.0 AS DECIMAL(5,2)),
                                               SystemCalculatedStatus = 'Manual Checkout (HR)',
                                               AttendanceStatus = 'Present'
                                           WHERE UserCode = @UserCode AND ActivityDate = @Date AND CompanyID = @CompanyID";

                            using (SqlCommand cmdUpdate = new SqlCommand(updateQuery, conn, tran))
                            {
                                cmdUpdate.Parameters.AddWithValue("@OutTime", combinedOutDateTime);
                                cmdUpdate.Parameters.AddWithValue("@UserCode", targetUser);
                                cmdUpdate.Parameters.AddWithValue("@Date", targetDate);
                                cmdUpdate.Parameters.AddWithValue("@CompanyID", companyId);
                                cmdUpdate.ExecuteNonQuery();
                            }
                        }
                        // ==========================================
                        // PATH B: FULL DAY OVERRIDE (Marking Present/Absent)
                        // ==========================================
                        else
                        {
                            // 1. Delete any existing partial/junk record for this user/date combo
                            string delQuery = "DELETE FROM tbl_Attendance WHERE UserCode = @UserCode AND ActivityDate = @Date AND CompanyID = @CompanyID";
                            using (SqlCommand cmdDel = new SqlCommand(delQuery, conn, tran))
                            {
                                cmdDel.Parameters.AddWithValue("@UserCode", targetUser);
                                cmdDel.Parameters.AddWithValue("@Date", targetDate);
                                cmdDel.Parameters.AddWithValue("@CompanyID", companyId);
                                cmdDel.ExecuteNonQuery();
                            }

                            // 2. Insert the Clean Override Record (Strictly matching your DB Schema)
                            string insQuery = @"INSERT INTO tbl_Attendance 
                                        (UserCode, ActivityDate, AttendanceStatus, SystemCalculatedStatus, TotalHoursWorked, CreatedDate, CompanyID) 
                                        VALUES 
                                        (@UserCode, @Date, @AttStatus, @SysStatus, @Hours, GETDATE(), @CompanyID)";

                            using (SqlCommand cmdIns = new SqlCommand(insQuery, conn, tran))
                            {
                                cmdIns.Parameters.AddWithValue("@UserCode", targetUser);
                                cmdIns.Parameters.AddWithValue("@Date", targetDate);
                                cmdIns.Parameters.AddWithValue("@CompanyID", companyId);

                                if (actionType == "Present")
                                {
                                    cmdIns.Parameters.AddWithValue("@AttStatus", "Present");
                                    cmdIns.Parameters.AddWithValue("@SysStatus", "HR Override - Present");
                                    cmdIns.Parameters.AddWithValue("@Hours", 8.0); // Defaulting to 8 hours for a forced present
                                }
                                else
                                {
                                    cmdIns.Parameters.AddWithValue("@AttStatus", "Absent");
                                    cmdIns.Parameters.AddWithValue("@SysStatus", "HR Override - Absent");
                                    cmdIns.Parameters.AddWithValue("@Hours", 0.0);
                                }

                                cmdIns.ExecuteNonQuery();
                            }
                        }

                        // ==========================================
                        // COMMON: NOTIFICATIONS & AUDIT TRAIL
                        // ==========================================
                        // The Remarks are safely saved here instead of tbl_Attendance!
                        string notiQuery = @"INSERT INTO tbl_SystemNotification 
                                    (Title, Message, ModuleCode, Severity, CreatedBy, StartDate, EndDate, IsActive, CompanyID) 
                                    VALUES 
                                    (@Title, @Message, 'Attendance', 'Warning', @AdminId, GETDATE(), DATEADD(day, 30, GETDATE()), 1, @CompanyID)";

                        using (SqlCommand cmdNoti = new SqlCommand(notiQuery, conn, tran))
                        {
                            cmdNoti.Parameters.AddWithValue("@Title", "Attendance Manual Override");
                            cmdNoti.Parameters.AddWithValue("@Message", $"Admin {adminId} manually marked {targetUser} as {actionType} for {Convert.ToDateTime(targetDate):dd-MMM-yyyy}. Remarks: {remarks}");
                            cmdNoti.Parameters.AddWithValue("@AdminId", adminId);
                            cmdNoti.Parameters.AddWithValue("@CompanyID", companyId);
                            cmdNoti.ExecuteNonQuery();
                        }

                        // Commit the Transaction securely
                        tran.Commit();

                        ScriptManager.RegisterStartupScript(this, GetType(), "alert", "Swal.fire('Success', 'Attendance processed successfully.', 'success');", true);

                        // Clear all modal inputs to prevent accidental re-submissions
                        hdnTargetUser.Value = "";
                        hdnTargetDate.Value = "";
                        hdnActionType.Value = "";
                        txtAdminRemarks.Text = "";
                        txtManualOutTime.Text = "";

                        // Refresh the Grid
                        GenerateReport();
                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception)
            {
                // Ponytail Standard #3: Never expose raw exception details to client
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", "Swal.fire('Error', 'An unexpected error occurred while processing attendance. Please try again.', 'error');", true);
            }
        }

        // ==========================================
        // EXCEL EXPORT ENGINE
        // ==========================================
        private DataTable GetSalesVisitData()
        {
            int targetMonth = Convert.ToInt32(ddlMonth.SelectedValue);
            int targetYear = Convert.ToInt32(ddlYear.SelectedValue);
            string selectedEmp = ddlEmployee.SelectedValue;
            int companyId = CompanyContext.CurrentCompanyID; // Strict Security

            string sql = @"
                SELECT 
                    sv.VisitDate,
                    u.Name AS SalespersonName,
                    sv.CustomerName,
                    sv.Department,
                    sv.ContactPerson,
                    sv.VisitType,
                    sv.DiscussionPoints,
                    sv.VisitPhase,
                    sv.GeoLocationAddress,
                    CAST(sv.IsProductive AS INT) AS IsProductive,
                    sv.RevenueRealized
                FROM tbl_SalesVisitReport sv
                LEFT JOIN tbl_login u ON sv.CreatedByCode = u.User_Id
                WHERE sv.CompanyID = @CompanyID 
                  AND MONTH(sv.VisitDate) = @Month 
                  AND YEAR(sv.VisitDate) = @Year
                  AND (@EmpId = 'ALL' OR sv.CreatedByCode = @EmpId)
                ORDER BY sv.VisitDate DESC";

            using (SqlConnection conn = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@CompanyID", companyId);
                cmd.Parameters.AddWithValue("@Month", targetMonth);
                cmd.Parameters.AddWithValue("@Year", targetYear);
                cmd.Parameters.AddWithValue("@EmpId", selectedEmp);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            DataTable dtAttendance = GetAttendanceData();

            if (dtAttendance != null && dtAttendance.Rows.Count > 0)
            {
                DataTable exportDt = new DataTable("Attendance_Register");
                exportDt.Columns.Add("Date", typeof(string));
                exportDt.Columns.Add("Day", typeof(string));
                exportDt.Columns.Add("Employee Name", typeof(string));
                exportDt.Columns.Add("Daily Status", typeof(string));

                // NEW COLUMNS ADDED HERE
                exportDt.Columns.Add("Attendance Code", typeof(string));
                exportDt.Columns.Add("Payable Day", typeof(decimal));

                exportDt.Columns.Add("Office IN", typeof(string));
                exportDt.Columns.Add("Office OUT", typeof(string));
                exportDt.Columns.Add("Total Hrs", typeof(string));
                exportDt.Columns.Add("Visits Logged", typeof(int));
                exportDt.Columns.Add("Revenue (INR)", typeof(decimal));

                foreach (DataRow row in dtAttendance.Rows)
                {
                    exportDt.Rows.Add(
                        Convert.ToDateTime(row["ActivityDate"]).ToString("dd-MMM-yyyy"),
                        row["DayOfWeek"].ToString(),
                        row["EmployeeName"].ToString(),
                        row["CalculatedStatus"].ToString(),

                        // DATA MAPPING FOR NEW COLUMNS
                        row["AttendanceCode"].ToString(),
                        Convert.ToDecimal(row["PayableDay"]),

                        row["PunchInTime"] != DBNull.Value ? Convert.ToDateTime(row["PunchInTime"]).ToString("hh:mm tt") : "-",
                        row["PunchOutTime"] != DBNull.Value ? Convert.ToDateTime(row["PunchOutTime"]).ToString("hh:mm tt") : "-",
                        row["TotalHoursWorked"] != DBNull.Value ? Convert.ToDecimal(row["TotalHoursWorked"]).ToString("F2") : "-",
                        Convert.ToInt32(row["FieldVisitsLogged"]),
                        Convert.ToDecimal(row["DailyRevenue"])
                    );
                }

                string empFileNamePart = "All_Employees";
                if (ddlEmployee.SelectedValue != "ALL")
                {
                    empFileNamePart = ddlEmployee.SelectedItem.Text.Replace(" [", "_").Replace("]", "").Replace(" ", "_");
                }
                string fileName = $"Attendance_{empFileNamePart}_{ddlMonth.SelectedItem.Text}_{ddlYear.SelectedValue}.xlsx";

                using (XLWorkbook wb = new XLWorkbook())
                {
                    var ws1 = wb.Worksheets.Add(exportDt);
                    ws1.Columns().AdjustToContents();
                    var header1 = ws1.Row(1);
                    header1.Style.Font.Bold = true;
                    header1.Style.Fill.BackgroundColor = XLColor.FromHtml("#19658A");
                    header1.Style.Font.FontColor = XLColor.White;

                    DataTable dtSales = GetSalesVisitData();
                    if (dtSales != null && dtSales.Rows.Count > 0)
                    {
                        DataTable exportSalesDt = new DataTable("Sales_Visits");
                        exportSalesDt.Columns.Add("Visit Date & Time", typeof(string));
                        exportSalesDt.Columns.Add("Salesperson", typeof(string));
                        exportSalesDt.Columns.Add("Customer / Client", typeof(string));
                        exportSalesDt.Columns.Add("Contact Person", typeof(string));
                        exportSalesDt.Columns.Add("Visit Type", typeof(string));
                        exportSalesDt.Columns.Add("Discussion Points", typeof(string));
                        exportSalesDt.Columns.Add("Phase", typeof(string));
                        exportSalesDt.Columns.Add("Productive?", typeof(string));
                        exportSalesDt.Columns.Add("Revenue (INR)", typeof(decimal));
                        exportSalesDt.Columns.Add("GPS Location", typeof(string));

                        foreach (DataRow row in dtSales.Rows)
                        {
                            exportSalesDt.Rows.Add(
                                row["VisitDate"] != DBNull.Value ? Convert.ToDateTime(row["VisitDate"]).ToString("dd-MMM-yyyy hh:mm tt") : "-",
                                row["SalespersonName"].ToString(),
                                row["CustomerName"].ToString(),
                                row["ContactPerson"].ToString(),
                                row["VisitType"].ToString(),
                                row["DiscussionPoints"].ToString(),
                                row["VisitPhase"].ToString(),
                                Convert.ToInt32(row["IsProductive"]) == 1 ? "Yes" : "No",
                                Convert.ToDecimal(row["RevenueRealized"]),
                                row["GeoLocationAddress"].ToString()
                            );
                        }

                        var ws2 = wb.Worksheets.Add(exportSalesDt);
                        ws2.Columns().AdjustToContents();
                        ws2.Column(6).Width = 50;
                        ws2.Column(6).Style.Alignment.WrapText = true;

                        var header2 = ws2.Row(1);
                        header2.Style.Font.Bold = true;
                        header2.Style.Fill.BackgroundColor = XLColor.FromHtml("#28a745");
                        header2.Style.Font.FontColor = XLColor.White;
                    }

                    Response.Clear();
                    Response.Buffer = true;
                    Response.Charset = "";
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment;filename=" + fileName);

                    using (System.IO.MemoryStream MyMemoryStream = new System.IO.MemoryStream())
                    {
                        wb.SaveAs(MyMemoryStream);
                        MyMemoryStream.WriteTo(Response.OutputStream);
                        Response.Flush();
                        Response.End();
                    }
                }
            }
        }


        private void CalculateSummaryCards(DataTable dtAttendance)
        {
            // The summary cards only make mathematical sense if we are looking at a SINGLE employee.
            // If HR selected "ALL EMPLOYEES", we hide the summary panel to prevent showing 300+ Payable Days.
            if (ddlEmployee.SelectedValue == "ALL" || dtAttendance == null || dtAttendance.Rows.Count == 0)
            {
                SummaryPanel.Visible = false;
                return;
            }

            SummaryPanel.Visible = true;

            int totalMonthDays = 0;
            decimal totalPayable = 0;
            int presentCount = 0;
            int halfDayCount = 0;
            int absentCount = 0;
            int offCount = 0;

            foreach (DataRow row in dtAttendance.Rows)
            {
                string status = row["CalculatedStatus"].ToString();
                string attCode = row["AttendanceCode"].ToString();

                // Skip future dates that haven't happened yet (so we don't count them as Absent)
                if (status == "Upcoming") continue;

                totalMonthDays++; // Count elapsed days

                // Sum the exact payroll multiplier from the database
                if (row["PayableDay"] != DBNull.Value)
                {
                    totalPayable += Convert.ToDecimal(row["PayableDay"]);
                }

                // Categorize based on the precise Attendance Code we built
                if (attCode == "P" || attCode == "NHP" || attCode == "FLP")
                {
                    presentCount++;
                }
                else if (attCode == "HD")
                {
                    halfDayCount++;
                }
                else if (attCode == "A" || attCode == "LWP") // Include Unpaid leaves as Absences visually
                {
                    absentCount++;
                }
                else if (status.Contains("Off") || attCode == "NH" || attCode == "FL" || attCode == "L")
                {
                    // Add Paid Leaves ('L') to the Holidays/Offs counter
                    offCount++;
                }
            }

            // Bind to UI
            lblTotalDays.Text = totalMonthDays.ToString();
            lblPayableDays.Text = totalPayable.ToString("0.0");
            lblPresent.Text = presentCount.ToString();
            lblHalfDays.Text = halfDayCount.ToString();
            lblAbsent.Text = absentCount.ToString();
            lblOffs.Text = offCount.ToString();
        }

        public string GetStatusBadgeClass(string status)
        {
            if (status.Contains("Office") || status.Contains("HR Override - Present")) return "badge badge-office";
            if (status.Contains("Field")) return "badge badge-field";
            if (status.Contains("Leave")) return "badge badge-leave";
            if (status.Contains("Weekly Off") || status.Contains("Upcoming")) return "badge badge-off";
            if (status.Contains("Absent")) return "badge badge-absent";
            return "badge";
        }
    }
}