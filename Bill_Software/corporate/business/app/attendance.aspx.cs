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
                return;
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
                    WHERE CompanyID = @CompanyID AND ShiftID = ISNULL((
                        SELECT TOP 1 ShiftID FROM tbl_EmployeeShiftMapping 
                        WHERE UserCode = @UserCode AND CompanyID = @CompanyID
                        AND EffectiveFromDate <= CAST(GETDATE() AS DATE) 
                        AND (EffectiveToDate IS NULL OR EffectiveToDate >= CAST(GETDATE() AS DATE))
                    ), 1)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserCode", userId);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    conn.Open();

                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            lblAssignedShift.Text = $"Shift: {rdr["ShiftName"]} ({rdr["StartTime"]} - {rdr["EndTime"]})";
                        }
                        else
                        {
                            lblAssignedShift.Text = "Shift: Not Assigned";
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
                            if (rdr["PunchInTime"] != DBNull.Value)
                            {
                                btnHtmlPunchIn.Disabled = true;
                                lblPunchInTime.Text = "Punched IN at: " + Convert.ToDateTime(rdr["PunchInTime"]).ToString("hh:mm tt");
                                lblStatusBadge.CssClass = "status-badge status-in";
                                lblStatusBadge.Text = "Status: Punched IN";
                            }
                            if (rdr["PunchOutTime"] != DBNull.Value)
                            {
                                btnHtmlPunchOut.Disabled = true;
                                lblPunchOutTime.Text = "Punched OUT at: " + Convert.ToDateTime(rdr["PunchOutTime"]).ToString("hh:mm tt");
                                lblStatusBadge.CssClass = "status-badge status-completed";
                                lblStatusBadge.Text = "Status: Shift Completed";
                            }
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
                string query = @"SELECT TOP 30 
                            Id, ActivityDate, PunchInTime, PunchOutTime, TotalHoursWorked, 
                            LateByMins, EarlyOutByMins, OvertimeMins, SystemCalculatedStatus 
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
                    gvAttendanceHistory.DataBind();
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

        // --- 2. AJAX Form Endpoints (Calendar & Maps) ---

        [WebMethod(EnableSession = true)]
        public static string GetActiveLeaveTypes()
        {
            if (HttpContext.Current.Session["USERID"] == null) return "[]";

            List<object> leaves = new List<object>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT LeaveID, LeaveName FROM tbl_LeaveMaster WHERE IsActive = 1 AND CompanyID = @CompanyID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
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
        public static string GetAttendanceDetails(int id)
        {
            if (HttpContext.Current.Session["USERID"] == null) return "{}";

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT ActivityDate, PunchInTime, PunchOutTime, StartLatitude, StartLongitude, EndLatitude, EndLongitude
                                 FROM tbl_Attendance
                                 WHERE Id = @Id AND UserCode = @UserId AND CompanyID = @CompanyID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@UserId", HttpContext.Current.Session["USERID"].ToString());
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            var details = new
                            {
                                Date = Convert.ToDateTime(rdr["ActivityDate"]).ToString("dd-MMM-yyyy"),
                                InTime = rdr["PunchInTime"] != DBNull.Value ? Convert.ToDateTime(rdr["PunchInTime"]).ToString("hh:mm tt") : "N/A",
                                OutTime = rdr["PunchOutTime"] != DBNull.Value ? Convert.ToDateTime(rdr["PunchOutTime"]).ToString("hh:mm tt") : "N/A",
                                InLat = rdr["StartLatitude"] != DBNull.Value ? rdr["StartLatitude"].ToString() : "",
                                InLon = rdr["StartLongitude"] != DBNull.Value ? rdr["StartLongitude"].ToString() : "",
                                OutLat = rdr["EndLatitude"] != DBNull.Value ? rdr["EndLatitude"].ToString() : "",
                                OutLon = rdr["EndLongitude"] != DBNull.Value ? rdr["EndLongitude"].ToString() : ""
                            };
                            return new JavaScriptSerializer().Serialize(details);
                        }
                    }
                }
            }
            return "{}";
        }

        [WebMethod(EnableSession = true)]
        public static string GetShiftTimings(string reqDate)
        {
            if (HttpContext.Current.Session["USERID"] == null) return "{}";

            string userId = HttpContext.Current.Session["USERID"].ToString();
            DateTime date = Convert.ToDateTime(reqDate);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT TOP 1 StartTime, EndTime FROM tbl_ShiftMaster
                    WHERE CompanyID = @CompanyID AND ShiftID = ISNULL((
                        SELECT TOP 1 ShiftID FROM tbl_EmployeeShiftMapping
                        WHERE UserCode = @UserCode AND CompanyID = @CompanyID
                        AND EffectiveFromDate <= @ReqDate
                        AND (EffectiveToDate IS NULL OR EffectiveToDate >= @ReqDate)
                    ), 1)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserCode", userId);
                    cmd.Parameters.AddWithValue("@ReqDate", date);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            var timings = new
                            {
                                InTime = rdr["StartTime"].ToString(),
                                OutTime = rdr["EndTime"].ToString()
                            };
                            return new JavaScriptSerializer().Serialize(timings);
                        }
                    }
                }
            }
            return "{}";
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
                string query = @"
                    SELECT Id, ActivityDate as StartDate, SystemCalculatedStatus as Status, 
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
                            string color = "#6c757d";

                            if (status.Contains("Present")) color = "#28a745";
                            else if (status.Contains("Absent")) color = "#dc3545";
                            else if (status.Contains("Leave")) color = "#17a2b8";
                            else if (status.Contains("Half")) color = "#fd7e14";

                            events.Add(new
                            {
                                id = rdr["Id"].ToString(),
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

                            -- 3. Proactive UI Notification Logging (FIXED SCHEMA)
                            IF @ManagerID IS NOT NULL
                            BEGIN
                                INSERT INTO tbl_SystemNotification 
                                    (Title, Message, ModuleCode, Severity, CreatedBy, StartDate, EndDate, IsActive, CompanyID)
                                VALUES 
                                    ('Attendance Correction', 
                                     @EmpName + ' requested regularization for ' + CONVERT(varchar, CAST(@Date AS DATE), 106), 
                                     'Attendance', 
                                     'Info', 
                                     @ManagerID, 
                                     GETDATE(), 
                                     DATEADD(day, 30, GETDATE()), 
                                     1, 
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
                                string mEmail = reader["ManagerEmail"]?.ToString();
                                string mMobile = reader["ManagerMobile"]?.ToString();
                                string empName = reader["EmpName"]?.ToString();

                                bool sendEmail = reader["SendEmail"] != DBNull.Value && Convert.ToBoolean(reader["SendEmail"]);
                                bool sendWA = reader["SendWA"] != DBNull.Value && Convert.ToBoolean(reader["SendWA"]);

                                string managerId = reader["ManagerID"]?.ToString();
                                string newRequestID = reader["NewRequestID"]?.ToString();

                                string rawApprove = $"ReqID={newRequestID}&Type=Reg&Action=Approve&ManagerID={managerId}&CompanyID={companyId}";
                                string rawReject = $"ReqID={newRequestID}&Type=Reg&Action=Reject&ManagerID={managerId}&CompanyID={companyId}";

                                string tokenApprove = SecurityHelper.EncryptToUrlToken(rawApprove);
                                string tokenReject = SecurityHelper.EncryptToUrlToken(rawReject);

                                string baseUrl = "https://exc.aagroupindia.com/corporate/business/app/";
                                string linkApprove = $"{baseUrl}QuickAction.aspx?t={tokenApprove}";
                                string linkReject = $"{baseUrl}QuickAction.aspx?t={tokenReject}";

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

                            -- 1. Insert Leave
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

                            -- 3. Proactive UI Notification Logging (FIXED SCHEMA)
                            IF @ManagerID IS NOT NULL
                            BEGIN
                                INSERT INTO tbl_SystemNotification 
                                    (Title, Message, ModuleCode, Severity, CreatedBy, StartDate, EndDate, IsActive, CompanyID)
                                VALUES 
                                    ('Quick Leave Request', 
                                     @EmpName + ' applied for leave on ' + CONVERT(varchar, CAST(@Date AS DATE), 106), 
                                     'Leave', 
                                     'Info', 
                                     @ManagerID, 
                                     GETDATE(), 
                                     DATEADD(day, 30, GETDATE()), 
                                     1, 
                                     @CompanyID);
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

        public string GetStatusColor(string status)
        {
            if (string.IsNullOrEmpty(status)) return "color: #333;";

            string s = status.ToLower();
            if (s.Contains("present")) return "color: #28a745; font-weight: bold;";
            if (s.Contains("absent")) return "color: #dc3545; font-weight: bold;";
            if (s.Contains("half") || s.Contains("short")) return "color: #fd7e14; font-weight: bold;";
            if (s.Contains("leave")) return "color: #17a2b8; font-weight: bold;";

            return "color: #333;";
        }

        #region Geo-Fenced Attendance Logic

        private static double CalculateDistanceInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371e3; // Earth's radius in meters
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        [WebMethod(EnableSession = true)]
        public static string ProcessPunch_OLD(string punchType, double currentLat, double currentLng)
        {
            try
            {
                if (HttpContext.Current.Session["USERID"] == null) return "Session Expired. Please login again.";

                string userId = HttpContext.Current.Session["USERID"].ToString();
                int currentCompanyId = CompanyContext.CurrentCompanyID;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string userQuery = @"SELECT RequireGeoTagging, GeoFenceLat, GeoFenceLng, GeoFenceRadius 
                                         FROM tbl_login 
                                         WHERE User_Id = @UserId AND CompanyID = @CompanyID AND IsActive = 1";

                    bool requireGeo = false;
                    double? targetLat = null;
                    double? targetLng = null;
                    int radius = 100;

                    using (SqlCommand cmd = new SqlCommand(userQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@CompanyID", currentCompanyId);

                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                requireGeo = Convert.ToBoolean(rdr["RequireGeoTagging"]);
                                if (rdr["GeoFenceLat"] != DBNull.Value) targetLat = Convert.ToDouble(rdr["GeoFenceLat"]);
                                if (rdr["GeoFenceLng"] != DBNull.Value) targetLng = Convert.ToDouble(rdr["GeoFenceLng"]);
                                if (rdr["GeoFenceRadius"] != DBNull.Value) radius = Convert.ToInt32(rdr["GeoFenceRadius"]);
                            }
                            else
                            {
                                return "Error: User account not found or inactive.";
                            }
                        }
                    }

                    if (requireGeo)
                    {
                        if (targetLat == null || targetLng == null)
                            return "Error: Geo-Fence is required but not configured by Admin. Please contact HR.";

                        double distance = CalculateDistanceInMeters(targetLat.Value, targetLng.Value, currentLat, currentLng);

                        if (distance > radius)
                        {
                            //return $"Geo-Fence Violation: You are {Math.Round(distance)} meters away from the allowed location. Limit is {radius} meters.";
                            return $"Geo-Fence Violation: You are {Math.Round(distance)} meters away from the allowed location.";
                        }
                    }

                    string today = DateTime.Now.ToString("yyyy-MM-dd");
                    int rowsAffected = 0;

                    if (punchType == "IN")
                    {
                        string checkQuery = "SELECT Id FROM tbl_Attendance WHERE UserCode = @UserId AND ActivityDate = @Today AND CompanyID = @CompanyID";
                        using (SqlCommand chkCmd = new SqlCommand(checkQuery, conn))
                        {
                            chkCmd.Parameters.AddWithValue("@UserId", userId);
                            chkCmd.Parameters.AddWithValue("@Today", today);
                            chkCmd.Parameters.AddWithValue("@CompanyID", currentCompanyId);
                            if (chkCmd.ExecuteScalar() != null) return "Error: You have already punched in for today.";
                        }

                        string insertQuery = @"INSERT INTO tbl_Attendance (UserCode, ActivityDate, PunchInTime, StartLatitude, StartLongitude, CompanyID) 
                                       VALUES (@UserId, @Today, GETDATE(), @Lat, @Lng, @CompanyID)";
                        using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.Parameters.AddWithValue("@Today", today);
                            cmd.Parameters.AddWithValue("@Lat", currentLat);
                            cmd.Parameters.AddWithValue("@Lng", currentLng);
                            cmd.Parameters.AddWithValue("@CompanyID", currentCompanyId);
                            rowsAffected = cmd.ExecuteNonQuery();
                        }
                    }
                    else if (punchType == "OUT")
                    {
                        string updateQuery = @"UPDATE tbl_Attendance 
                                       SET PunchOutTime = GETDATE(), EndLatitude = @Lat, EndLongitude = @Lng,
                                           TotalHoursWorked = CAST(DATEDIFF(MINUTE, PunchInTime, GETDATE()) / 60.0 AS DECIMAL(5,2))
                                       WHERE UserCode = @UserId AND ActivityDate = @Today AND CompanyID = @CompanyID";
                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.Parameters.AddWithValue("@Today", today);
                            cmd.Parameters.AddWithValue("@Lat", currentLat);
                            cmd.Parameters.AddWithValue("@Lng", currentLng);
                            cmd.Parameters.AddWithValue("@CompanyID", currentCompanyId);
                            rowsAffected = cmd.ExecuteNonQuery();
                        }

                        if (rowsAffected == 0) return "Error: Could not punch out. You must punch in first.";
                    }

                    if (rowsAffected > 0)
                    {
                        // BUG FIX: Schema aligned Notification query
                        string query = @"INSERT INTO tbl_SystemNotification 
                                            (Title, Message, ModuleCode, Severity, CreatedBy, StartDate, EndDate, IsActive, CompanyID) 
                                         VALUES 
                                            (@Title, @Message, @ModuleCode, @Severity, @CreatedBy, GETDATE(), DATEADD(day, 30, GETDATE()), 1, @CompanyID)";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Title", $"Attendance Punched {punchType}");
                            cmd.Parameters.AddWithValue("@Message", $"Employee {userId} successfully punched {punchType.ToLower()} from an authorized location.");
                            cmd.Parameters.AddWithValue("@ModuleCode", "Attendance");
                            cmd.Parameters.AddWithValue("@Severity", "Success");
                            cmd.Parameters.AddWithValue("@CreatedBy", userId);
                            cmd.Parameters.AddWithValue("@CompanyID", currentCompanyId);
                            cmd.ExecuteNonQuery();
                        }
                        return "Success: Attendance recorded securely.";
                    }

                    return "Error: Database transaction failed.";
                }
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        [WebMethod(EnableSession = true)]
        public static string ProcessPunch(string punchType, double currentLat, double currentLng)
        {
            try
            {
                if (HttpContext.Current.Session["USERID"] == null) return "Session Expired. Please login again.";

                string userId = HttpContext.Current.Session["USERID"].ToString();
                int currentCompanyId = CompanyContext.CurrentCompanyID; // Strict Tenant Isolation

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // 1. Fetch User Geo-Fence Settings strictly for current Company
                    string userQuery = @"SELECT RequireGeoTagging, GeoFenceLat, GeoFenceLng, GeoFenceRadius 
                                 FROM tbl_login 
                                 WHERE User_Id = @UserId AND CompanyID = @CompanyID AND IsActive = 1";

                    bool requireGeo = false;
                    double? targetLat = null;
                    double? targetLng = null;
                    int radius = 100;

                    using (SqlCommand cmd = new SqlCommand(userQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@CompanyID", currentCompanyId);

                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                requireGeo = Convert.ToBoolean(rdr["RequireGeoTagging"]);
                                if (rdr["GeoFenceLat"] != DBNull.Value) targetLat = Convert.ToDouble(rdr["GeoFenceLat"]);
                                if (rdr["GeoFenceLng"] != DBNull.Value) targetLng = Convert.ToDouble(rdr["GeoFenceLng"]);
                                if (rdr["GeoFenceRadius"] != DBNull.Value) radius = Convert.ToInt32(rdr["GeoFenceRadius"]);
                            }
                            else
                            {
                                return "Error: User account not found or inactive.";
                            }
                        }
                    }

                    // 2. Validate Geo-Fence Limits & Log Failures
                    if (requireGeo)
                    {
                        if (targetLat == null || targetLng == null)
                        {
                            InsertSystemNotification("Config Error", $"Employee {userId} attempted to punch, but their Geo-Fence is not configured.", "Attendance", "Warning", userId, currentCompanyId, conn);
                            return "Error: Geo-Fence is required but not configured by Admin. Please contact HR.";
                        }

                        double distance = CalculateDistanceInMeters(targetLat.Value, targetLng.Value, currentLat, currentLng);

                        if (distance > radius)
                        {
                            // LOG THE FRAUD / FAILURE ATTEMPT
                            string violationMsg = $"Geo-Fence Violation: {userId} attempted to punch {punchType} from {Math.Round(distance)}m away.";
                            InsertSystemNotification("Geo-Fence Violation", violationMsg, "Attendance", "Danger", userId, currentCompanyId, conn);

                            return $"Geo-Fence Violation: You are {Math.Round(distance)} meters away from the allowed location. Limit is {radius} meters.";
                        }
                    }

                    // 3. Process Attendance
                    string today = DateTime.Now.ToString("yyyy-MM-dd");
                    int rowsAffected = 0;

                    if (punchType == "IN")
                    {
                        string checkQuery = "SELECT Id FROM tbl_Attendance WHERE UserCode = @UserId AND ActivityDate = @Today AND CompanyID = @CompanyID";
                        using (SqlCommand chkCmd = new SqlCommand(checkQuery, conn))
                        {
                            chkCmd.Parameters.AddWithValue("@UserId", userId);
                            chkCmd.Parameters.AddWithValue("@Today", today);
                            chkCmd.Parameters.AddWithValue("@CompanyID", currentCompanyId);
                            if (chkCmd.ExecuteScalar() != null)
                            {
                                // LOG THE DUPLICATE ATTEMPT
                                InsertSystemNotification("Duplicate Punch Attempt", $"Employee {userId} attempted to punch IN again.", "Attendance", "Warning", userId, currentCompanyId, conn);
                                return "Error: You have already punched in for today.";
                            }
                        }

                        string insertQuery = @"INSERT INTO tbl_Attendance (UserCode, ActivityDate, PunchInTime, StartLatitude, StartLongitude, CompanyID) 
                                       VALUES (@UserId, @Today, GETDATE(), @Lat, @Lng, @CompanyID)";
                        using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.Parameters.AddWithValue("@Today", today);
                            cmd.Parameters.AddWithValue("@Lat", currentLat);
                            cmd.Parameters.AddWithValue("@Lng", currentLng);
                            cmd.Parameters.AddWithValue("@CompanyID", currentCompanyId);
                            rowsAffected = cmd.ExecuteNonQuery();
                        }
                    }
                    else if (punchType == "OUT")
                    {
                        string updateQuery = @"UPDATE tbl_Attendance 
                                       SET PunchOutTime = GETDATE(), EndLatitude = @Lat, EndLongitude = @Lng,
                                           TotalHoursWorked = CAST(DATEDIFF(MINUTE, PunchInTime, GETDATE()) / 60.0 AS DECIMAL(5,2))
                                       WHERE UserCode = @UserId AND ActivityDate = @Today AND CompanyID = @CompanyID";
                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.Parameters.AddWithValue("@Today", today);
                            cmd.Parameters.AddWithValue("@Lat", currentLat);
                            cmd.Parameters.AddWithValue("@Lng", currentLng);
                            cmd.Parameters.AddWithValue("@CompanyID", currentCompanyId);
                            rowsAffected = cmd.ExecuteNonQuery();
                        }

                        if (rowsAffected == 0)
                        {
                            InsertSystemNotification("Sequence Error", $"Employee {userId} attempted to punch OUT without punching IN.", "Attendance", "Warning", userId, currentCompanyId, conn);
                            return "Error: Could not punch out. You must punch in first.";
                        }
                    }

                    // 4. Log the Successful Punch
                    if (rowsAffected > 0)
                    {
                        InsertSystemNotification(
                            $"Attendance Punched {punchType}",
                            $"Employee {userId} successfully punched {punchType.ToLower()} from an authorized location.",
                            "Attendance",
                            "Success",
                            userId,
                            currentCompanyId,
                            conn
                        );
                        return "Success: Attendance recorded securely.";
                    }

                    return "Error: Database transaction failed.";
                }
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        [WebMethod(EnableSession = true)]
        public static string GetMyGeoFence()
        {
            if (HttpContext.Current.Session["USERID"] == null) return "{}";

            string userId = HttpContext.Current.Session["USERID"].ToString();
            int companyId = CompanyContext.CurrentCompanyID;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT RequireGeoTagging, GeoFenceLat, GeoFenceLng, GeoFenceRadius 
                         FROM tbl_login 
                         WHERE User_Id = @UserId AND CompanyID = @CompanyID AND IsActive = 1";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@CompanyID", companyId);

                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            var boundaryData = new
                            {
                                Required = Convert.ToBoolean(rdr["RequireGeoTagging"]),
                                Lat = rdr["GeoFenceLat"] != DBNull.Value ? rdr["GeoFenceLat"].ToString() : "",
                                Lng = rdr["GeoFenceLng"] != DBNull.Value ? rdr["GeoFenceLng"].ToString() : "",
                                Radius = rdr["GeoFenceRadius"] != DBNull.Value ? rdr["GeoFenceRadius"].ToString() : "100"
                            };
                            return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(boundaryData);
                        }
                    }
                }
            }
            return "{}";
        }

        // Helper specific to attendance file mapping (Requires 7 Arguments)
        private static void InsertSystemNotification(string title, string message, string moduleCode, string severity, string userId, int companyId, SqlConnection conn)
        {
            string query = @"INSERT INTO tbl_SystemNotification 
                        (Title, Message, ModuleCode, Severity, CreatedBy, StartDate, EndDate, IsActive, CompanyID) 
                     VALUES 
                        (@Title, @Message, @ModuleCode, @Severity, @CreatedBy, GETDATE(), DATEADD(day, 30, GETDATE()), 1, @CompanyID)";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Message", message);
                cmd.Parameters.AddWithValue("@ModuleCode", moduleCode);
                cmd.Parameters.AddWithValue("@Severity", severity); // 'Info', 'Success', 'Warning', 'Danger'

                // Handle potentially null user IDs gracefully
                cmd.Parameters.AddWithValue("@CreatedBy", string.IsNullOrEmpty(userId) ? (object)DBNull.Value : userId);

                cmd.Parameters.AddWithValue("@CompanyID", companyId);

                // Notice: We do NOT call conn.Open() here because 'conn' is already open from ProcessPunch!
                cmd.ExecuteNonQuery();
            }
        }
        #endregion
    }
}