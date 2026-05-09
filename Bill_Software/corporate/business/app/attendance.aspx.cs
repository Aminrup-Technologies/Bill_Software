using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Collections.Generic;

namespace Bill_Software.corporate.business.app
{
    public partial class attendance : System.Web.UI.Page
    {
        static string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }

            if (!IsPostBack)
            {
                lblCurrentDate.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");
                DisplayAssignedShift();
                CheckTodayStatus();
                LoadAttendanceHistory();
                LoadRegularizationHistory();
            }
        }

        // --- 1. Data Loading (Strictly Segregated by CompanyContext) ---

        private void DisplayAssignedShift()
        {
            string userId = HttpContext.Current.Session["USERID"].ToString();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT TOP 1 ShiftName, StartTime, EndTime FROM tbl_ShiftMaster
                    WHERE ShiftID = ISNULL((
                        SELECT TOP 1 ShiftID FROM tbl_EmployeeShiftMapping 
                        WHERE UserCode = @UserCode 
                        AND EffectiveFromDate <= CAST(GETDATE() AS DATE) 
                        AND (EffectiveToDate IS NULL OR EffectiveToDate >= CAST(GETDATE() AS DATE))
                    ), 1)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserCode", userId);
                    // CompanyID not typically needed for ShiftMaster read unless shifts are tenant-specific
                    conn.Open();
                    // Assuming you have a label for this on the frontend (e.g., lblShiftInfo)
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            // Example output: "General Shift (09:00 - 18:00)"
                            // lblShiftInfo.Text = $"{rdr["ShiftName"]} ({rdr["StartTime"]} - {rdr["EndTime"]})";
                        }
                    }
                }
            }
        }

        private void CheckTodayStatus()
        {
            string userId = HttpContext.Current.Session["USERID"].ToString();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT PunchInTime, PunchOutTime FROM tbl_Attendance WHERE UserCode = @UserCode AND ActivityDate = CAST(GETDATE() AS DATE) AND CompanyID = @CompanyID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserCode", userId);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            if (rdr["PunchInTime"] != DBNull.Value) btnHtmlPunchIn.Disabled = true;
                            if (rdr["PunchOutTime"] != DBNull.Value) btnHtmlPunchOut.Disabled = true;
                        }
                    }
                }
            }
        }

        private void LoadAttendanceHistory()
        {
            string userId = HttpContext.Current.Session["USERID"].ToString();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // FIX: Added LateByMins, EarlyOutByMins, and OvertimeMins to the SELECT list
                string query = @"SELECT TOP 30 
                            Id,
                            ActivityDate, 
                            PunchInTime, 
                            PunchOutTime, 
                            TotalHoursWorked, 
                            LateByMins, 
                            EarlyOutByMins, 
                            OvertimeMins, 
                            SystemCalculatedStatus 
                         FROM tbl_Attendance 
                         WHERE UserCode = @UserCode AND CompanyID = @CompanyID
                         ORDER BY ActivityDate DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserCode", userId);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvAttendanceHistory.DataSource = dt;
                    gvAttendanceHistory.DataBind(); // This will now find 'LateByMins' successfully
                }
            }
        }

        private void LoadRegularizationHistory()
        {
            string userId = HttpContext.Current.Session["USERID"].ToString();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT TOP 10 AttendanceDate, RequestedInTime, RequestedOutTime, RequestStatus, AppliedOn 
                                 FROM tbl_AttendanceRegularization 
                                 WHERE UserCode = @UserCode AND CompanyID = @CompanyID
                                 ORDER BY AppliedOn DESC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserCode", userId);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvRegHistory.DataSource = dt;
                    gvRegHistory.DataBind();
                }
            }
        }

        // --- 2. Daily Punches (Silent & Secured) ---

        protected void btnProcessServerPunch_Click(object sender, EventArgs e)
        {
            string userId = HttpContext.Current.Session["USERID"].ToString();
            string action = hfPunchAction.Value; // Expected "IN" or "OUT"
            decimal lat = 0;
            decimal lon = 0;
            decimal.TryParse(hfLatitude.Value, out lat);
            decimal.TryParse(hfLongitude.Value, out lon);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string query = "";

                if (action == "IN")
                {
                    query = @"IF NOT EXISTS (SELECT 1 FROM tbl_Attendance WHERE UserCode = @UserCode AND ActivityDate = CAST(GETDATE() AS DATE) AND CompanyID = @CompanyID)
                              BEGIN
                                  INSERT INTO tbl_Attendance (UserCode, ActivityDate, PunchInTime, StartLatitude, StartLongitude, CompanyID, SystemCalculatedStatus)
                                  VALUES (@UserCode, CAST(GETDATE() AS DATE), GETDATE(), @Lat, @Lon, @CompanyID, 'Present');
                              END";
                }
                else if (action == "OUT")
                {
                    query = @"UPDATE tbl_Attendance 
                              SET PunchOutTime = GETDATE(), EndLatitude = @Lat, EndLongitude = @Lon, 
                                  TotalHoursWorked = CAST(DATEDIFF(MINUTE, PunchInTime, GETDATE()) / 60.0 AS DECIMAL(5,2))
                              WHERE UserCode = @UserCode AND ActivityDate = CAST(GETDATE() AS DATE) AND CompanyID = @CompanyID";
                }

                if (!string.IsNullOrEmpty(query))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserCode", userId);
                        cmd.Parameters.AddWithValue("@Lat", lat);
                        cmd.Parameters.AddWithValue("@Lon", lon);
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID); // STRICT ISOLATION
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            CheckTodayStatus();
            LoadAttendanceHistory();
        }

        // --- 3. Exception WebMethods (Transactions, Notifications & Gateway) ---

        [WebMethod(EnableSession = true)]
        public static string SubmitRegularization(string reqDate, string inTime, string outTime, string reason)
        {
            if (HttpContext.Current.Session["USERID"] == null) return "Session Expired";

            string empId = HttpContext.Current.Session["USERID"].ToString();
            int companyId = CompanyContext.CurrentCompanyID;

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = @"
                        BEGIN TRY
                            BEGIN TRANSACTION;

                            -- 1. Insert Request
                            INSERT INTO tbl_AttendanceRegularization (UserCode, AttendanceDate, RequestedInTime, RequestedOutTime, Reason, RequestStatus, AppliedOn, CompanyID)
                            VALUES (@UserCode, @Date, @InTime, @OutTime, @Reason, 'Pending', GETDATE(), @CompanyID);

                            -- Capture the newly created RequestID immediately
                            DECLARE @NewReqID INT = SCOPE_IDENTITY();

                            -- 2. Fetch Schema-Aligned Manager Details
                            DECLARE @ManagerID varchar(50), @ManagerEmail varchar(150), @ManagerMobile varchar(20), @EmpName varchar(50);
                            DECLARE @SendEmail bit, @SendWA bit;

                            SELECT @ManagerID = ReportingManagerId, @EmpName = Name 
                            FROM tbl_login WHERE User_Id = @UserCode AND CompanyID = @CompanyID;

                            SELECT 
                                @ManagerEmail = Email, 
                                @ManagerMobile = Phone_no,
                                @SendEmail = EnableEmailAlerts,
                                @SendWA = EnableWhatsAppAlerts
                            FROM tbl_login 
                            WHERE User_Id = @ManagerID AND CompanyID = @CompanyID AND IsActive = 1;

                            -- 3. Proactive UI Notification Logging (Aligned to your exact schema)
                            IF @ManagerID IS NOT NULL
                            BEGIN
                                INSERT INTO tbl_SystemNotification 
                                    (Title, Message, ModuleCode, Severity, StartDate, EndDate, IsActive, CreatedBy, CompanyID)
                                VALUES 
                                    ('Attendance Correction', 
                                     @EmpName + ' requested regularization for ' + CONVERT(varchar, CAST(@Date AS DATE), 106), 
                                     'HR/Attendance', 
                                     'Info', 
                                     GETDATE(), 
                                     DATEADD(day, 30, GETDATE()), 
                                     1, 
                                     @ManagerID, 
                                     @CompanyID);
                            END

                            -- 4. Output Manager Data for External API
                            SELECT 
                                @ManagerEmail AS ManagerEmail, 
                                @ManagerMobile AS ManagerMobile, 
                                @EmpName AS EmpName, 
                                @SendEmail AS SendEmail, 
                                @SendWA AS SendWA,
                                @ManagerID AS ManagerID,
                                @NewReqID AS NewRequestID;

                            COMMIT TRANSACTION;
                        END TRY
                        BEGIN CATCH
                            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                            THROW;
                        END CATCH;
                    ";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserCode", empId);
                        cmd.Parameters.AddWithValue("@Date", Convert.ToDateTime(reqDate));
                        cmd.Parameters.AddWithValue("@InTime", inTime);
                        cmd.Parameters.AddWithValue("@OutTime", outTime);
                        cmd.Parameters.AddWithValue("@Reason", reason);
                        cmd.Parameters.AddWithValue("@CompanyID", companyId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // 1. Extract Contact & Preference Info
                                string mEmail = reader["ManagerEmail"]?.ToString();
                                string mMobile = reader["ManagerMobile"]?.ToString();
                                string empName = reader["EmpName"]?.ToString();

                                bool sendEmail = reader["SendEmail"] != DBNull.Value && Convert.ToBoolean(reader["SendEmail"]);
                                bool sendWA = reader["SendWA"] != DBNull.Value && Convert.ToBoolean(reader["SendWA"]);

                                // 2. Extract IDs for the Security Token
                                string managerId = reader["ManagerID"]?.ToString();
                                string newRequestID = reader["NewRequestID"]?.ToString();

                                // 3. Generate the Secure Payloads (Notice Type=Reg)
                                string rawApprove = $"ReqID={newRequestID}&Type=Reg&Action=Approve&ManagerID={managerId}&CompanyID={companyId}";
                                string rawReject = $"ReqID={newRequestID}&Type=Reg&Action=Reject&ManagerID={managerId}&CompanyID={companyId}";

                                string tokenApprove = SecurityHelper.EncryptToUrlToken(rawApprove);
                                string tokenReject = SecurityHelper.EncryptToUrlToken(rawReject);

                                string baseUrl = "https://exc.aagroupindia.com/corporate/business/app/";
                                string linkApprove = $"{baseUrl}QuickAction.aspx?t={tokenApprove}";
                                string linkReject = $"{baseUrl}QuickAction.aspx?t={tokenReject}";

                                // 4. Build the Rich HTML Email
                                string subject = $"Action Required: Attendance Regularization ({empName})";
                                string htmlMessage = $@"
                                <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f9f9f9; border-radius: 8px;'>
                                    <h2 style='color: #19658A;'>Attendance Regularization Request</h2>
                                    <p><strong>{empName}</strong> has requested a correction to their attendance.</p>
                                    <table style='background: white; padding: 15px; border-radius: 5px; width: 100%; border: 1px solid #ddd; margin-bottom: 20px;'>
                                        <tr><td style='padding: 5px 0;'><strong>Date:</strong></td><td>{Convert.ToDateTime(reqDate):dd-MMM-yyyy}</td></tr>
                                        <tr><td style='padding: 5px 0;'><strong>Requested In-Time:</strong></td><td>{inTime}</td></tr>
                                        <tr><td style='padding: 5px 0;'><strong>Requested Out-Time:</strong></td><td>{outTime}</td></tr>
                                        <tr><td style='padding: 5px 0;'><strong>Reason:</strong></td><td>{reason}</td></tr>
                                    </table>
                                    
                                    <div style='margin-top: 20px;'>
                                        <a href='{linkApprove}' style='background-color: #28a745; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold; margin-right: 15px; display: inline-block;'>✅ Approve Correction</a>
                                        <a href='{linkReject}' style='background-color: #dc3545; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>❌ Reject</a>
                                    </div>
                                    <p style='font-size: 12px; color: #777; margin-top: 30px;'>You can approve or reject directly from this email without logging in.</p>
                                </div>";

                                string targetEmail = sendEmail ? mEmail : null;
                                string targetMobile = sendWA ? mMobile : null;

                                if (!string.IsNullOrEmpty(targetEmail) || !string.IsNullOrEmpty(targetMobile))
                                {
                                    CommunicationGateway.SendAlertsAsync(targetEmail, targetMobile, subject, htmlMessage);
                                }
                            }
                        }
                    }
                    return "Success";
                }
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        [WebMethod(EnableSession = true)]
        public static string SubmitLeave(string reqDate, int leaveId, string reason)
        {
            if (HttpContext.Current.Session["USERID"] == null) return "Session Expired";

            string empId = HttpContext.Current.Session["USERID"].ToString();
            int companyId = CompanyContext.CurrentCompanyID;
            DateTime targetDate = Convert.ToDateTime(reqDate);

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = @"
                        BEGIN TRY
                            BEGIN TRANSACTION;

                            -- 1. Insert Leave (Quick 1-Day Leave from Calendar)
                            INSERT INTO tbl_LeaveRequests (UserCode, LeaveID, StartDate, EndDate, TotalDays, Reason, RequestStatus, AppliedOn, CompanyID)
                            VALUES (@UserCode, @LeaveID, @Date, @Date, 1.0, @Reason, 'Pending', GETDATE(), @CompanyID);

                            -- 2. Fetch Schema-Aligned Manager Details
                            DECLARE @ManagerID varchar(50), @ManagerEmail varchar(150), @ManagerMobile varchar(20), @EmpName varchar(50);
                            DECLARE @SendEmail bit, @SendWA bit;

                            SELECT @ManagerID = ReportingManagerId, @EmpName = Name 
                            FROM tbl_login WHERE User_Id = @UserCode AND CompanyID = @CompanyID;

                            SELECT 
                                @ManagerEmail = Email, 
                                @ManagerMobile = Phone_no,
                                @SendEmail = EnableEmailAlerts,
                                @SendWA = EnableWhatsAppAlerts
                            FROM tbl_login 
                            WHERE User_Id = @ManagerID AND CompanyID = @CompanyID AND IsActive = 1;

                            -- 3. Proactive UI Notification Logging
                            IF @ManagerID IS NOT NULL
                            BEGIN
                                INSERT INTO tbl_SystemNotification (Title, Message, Module, Type, UserID, CreatedDate, IsRead, CompanyID)
                                VALUES ('Quick Leave Request', @EmpName + ' applied for leave on ' + CONVERT(varchar, CAST(@Date AS DATE), 106), 'HR/Leave', 'Info', @ManagerID, GETDATE(), 0, @CompanyID);
                            END

                            -- 4. Output Manager Data
                            SELECT @ManagerEmail AS ManagerEmail, @ManagerMobile AS ManagerMobile, @EmpName AS EmpName, @SendEmail AS SendEmail, @SendWA AS SendWA;

                            COMMIT TRANSACTION;
                        END TRY
                        BEGIN CATCH
                            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                            THROW;
                        END CATCH;
                    ";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserCode", empId);
                        cmd.Parameters.AddWithValue("@LeaveID", leaveId);
                        cmd.Parameters.AddWithValue("@Date", targetDate);
                        cmd.Parameters.AddWithValue("@Reason", reason);
                        cmd.Parameters.AddWithValue("@CompanyID", companyId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string mEmail = reader["ManagerEmail"]?.ToString();
                                string mMobile = reader["ManagerMobile"]?.ToString();
                                string empName = reader["EmpName"]?.ToString();

                                bool sendEmail = reader["SendEmail"] != DBNull.Value && Convert.ToBoolean(reader["SendEmail"]);
                                bool sendWA = reader["SendWA"] != DBNull.Value && Convert.ToBoolean(reader["SendWA"]);

                                string subject = $"Leave Request Action Required: {empName}";
                                string message = $"Hello Manager, {empName} has requested a leave for {targetDate:dd-MMM}. Please log in to approve or reject.";

                                string targetEmail = sendEmail ? mEmail : null;
                                string targetMobile = sendWA ? mMobile : null;

                                if (!string.IsNullOrEmpty(targetEmail) || !string.IsNullOrEmpty(targetMobile))
                                {
                                    CommunicationGateway.SendAlertsAsync(targetEmail, targetMobile, subject, message);
                                }
                            }
                        }
                    }
                    return "Success";
                }
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        // Called by the ASPX GridView to color-code attendance statuses
        public string GetStatusColor(string status)
        {
            if (string.IsNullOrEmpty(status)) return "color: #333;";

            string s = status.ToLower();
            if (s.Contains("present"))
                return "color: #28a745; font-weight: bold;"; // Green
            if (s.Contains("absent"))
                return "color: #dc3545; font-weight: bold;"; // Red
            if (s.Contains("half") || s.Contains("short"))
                return "color: #fd7e14; font-weight: bold;"; // Orange
            if (s.Contains("leave"))
                return "color: #17a2b8; font-weight: bold;"; // Blue

            return "color: #333;"; // Default Dark Gray
        }

        [WebMethod]
        public static string GetActiveLeaveTypes()
        {
            List<object> leaves = new List<object>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT LeaveID, LeaveName FROM tbl_LeaveMaster WHERE IsActive = 1";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            leaves.Add(new { id = rdr["LeaveID"], name = rdr["LeaveName"].ToString() });
                        }
                    }
                }
            }
            return new JavaScriptSerializer().Serialize(leaves);
        }

        [WebMethod(EnableSession = true)]
        public static object GetMonthlyCalendarData()
        {
            if (HttpContext.Current.Session["USERID"] == null) return null;

            string userId = HttpContext.Current.Session["USERID"].ToString();
            int companyId = CompanyContext.CurrentCompanyID;
            List<object> events = new List<object>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Fetch Attendance and Leave Statuses
                string query = @"
                    SELECT ActivityDate as StartDate, SystemCalculatedStatus as Status, 
                           PunchInTime, PunchOutTime 
                    FROM tbl_Attendance 
                    WHERE UserCode = @UserCode AND CompanyID = @CompanyID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserCode", userId);
                    cmd.Parameters.AddWithValue("@CompanyID", companyId);
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            string status = rdr["Status"].ToString();
                            string color = "#6c757d"; // Default Gray

                            if (status.Contains("Present")) color = "#28a745";
                            else if (status.Contains("Absent")) color = "#dc3545";
                            else if (status.Contains("Leave")) color = "#17a2b8";
                            else if (status.Contains("Half")) color = "#fd7e14";

                            events.Add(new
                            {
                                title = status,
                                start = Convert.ToDateTime(rdr["StartDate"]).ToString("yyyy-MM-dd"),
                                backgroundColor = color,
                                borderColor = color,
                                extendedProps = new
                                {
                                    punchIn = rdr["PunchInTime"] != DBNull.Value ? Convert.ToDateTime(rdr["PunchInTime"]).ToString("HH:mm") : "N/A",
                                    punchOut = rdr["PunchOutTime"] != DBNull.Value ? Convert.ToDateTime(rdr["PunchOutTime"]).ToString("HH:mm") : "N/A"
                                }
                            });
                        }
                    }
                }
            }
            return events;
        }
    }
}