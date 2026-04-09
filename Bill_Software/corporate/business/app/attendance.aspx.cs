using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;

namespace Bill_Software.corporate.business.app
{
    public partial class attendance : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

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
                LoadRegularizationHistory(); // <-- NEW ADDITION
            }
        }

        // New Helper to show the user what shift they are assigned to today
        private void DisplayAssignedShift()
        {
            string userId = HttpContext.Current.Session["USERID"].ToString();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT TOP 1 ShiftName, StartTime, EndTime FROM tbl_ShiftMaster
                    WHERE ShiftID = ISNULL((
                        SELECT TOP 1 ShiftID FROM tbl_EmployeeShiftMapping 
                        WHERE UserCode = @UserCode AND EffectiveFromDate <= CAST(GETDATE() AS DATE) 
                        AND (EffectiveToDate IS NULL OR EffectiveToDate >= CAST(GETDATE() AS DATE))
                        ORDER BY EffectiveFromDate DESC
                    ), 1)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserCode", userId);
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            TimeSpan start = (TimeSpan)rdr["StartTime"];
                            TimeSpan end = (TimeSpan)rdr["EndTime"];
                            lblAssignedShift.Text = $"Current Shift: {rdr["ShiftName"]} ({DateTime.Today.Add(start).ToString("hh:mm tt")} to {DateTime.Today.Add(end).ToString("hh:mm tt")})";
                        }
                    }
                }
            }
        }

        // --- NEW: Load Employee's Regularization History ---
        private void LoadRegularizationHistory()
        {
            try
            {
                string userId = HttpContext.Current.Session["USERID"].ToString();
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string query = @"
                        SELECT AppliedOn, AttendanceDate, 
                               CONVERT(varchar(15), CAST(RequestedInTime AS TIME), 100) AS RequestedInTime, 
                               CONVERT(varchar(15), CAST(RequestedOutTime AS TIME), 100) AS RequestedOutTime, 
                               RequestStatus 
                        FROM tbl_AttendanceRegularization 
                        WHERE UserCode = @UserCode 
                        ORDER BY AppliedOn DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserCode", userId);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        gvRegHistory.DataSource = dt;
                        gvRegHistory.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "Error loading correction history: " + ex.Message;
            }
        }

        // --- NEW: Color Code the Status Text ---
        public string GetStatusColor(string status)
        {
            switch (status.ToLower())
            {
                case "approved": return "color: #28a745; font-weight: bold;";
                case "rejected": return "color: #dc3545; font-weight: bold;";
                default: return "color: #fd7e14; font-weight: bold;"; // Pending (Orange)
            }
        }

        private void CheckTodayStatus()
        {
            string userId = HttpContext.Current.Session["USERID"].ToString();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT PunchInTime, PunchOutTime FROM tbl_Attendance 
                                 WHERE UserCode = @UserCode AND ActivityDate = CAST(GETDATE() AS DATE)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserCode", userId);
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            string inTime = rdr["PunchInTime"] != DBNull.Value ? Convert.ToDateTime(rdr["PunchInTime"]).ToString("hh:mm tt") : "";
                            lblPunchInTime.Text = "Punched In at: " + inTime;

                            if (rdr["PunchOutTime"] == DBNull.Value)
                            {
                                lblStatusBadge.Text = "Status: Punched IN (Active Shift)";
                                lblStatusBadge.CssClass = "status-badge status-in";
                                btnHtmlPunchIn.Disabled = true;
                                btnHtmlPunchOut.Disabled = false;
                            }
                            else
                            {
                                string outTime = Convert.ToDateTime(rdr["PunchOutTime"]).ToString("hh:mm tt");
                                lblPunchOutTime.Text = "Punched Out at: " + outTime;
                                lblStatusBadge.Text = "Status: Shift Completed";
                                lblStatusBadge.CssClass = "status-badge status-completed";
                                btnHtmlPunchIn.Disabled = true;
                                btnHtmlPunchOut.Disabled = true;
                            }
                        }
                        else
                        {
                            lblStatusBadge.Text = "Status: Not Punched In";
                            lblStatusBadge.CssClass = "status-badge status-out";
                            lblPunchInTime.Text = "";
                            lblPunchOutTime.Text = "";
                            btnHtmlPunchIn.Disabled = false;
                            btnHtmlPunchOut.Disabled = true;
                        }
                    }
                }
            }
        }

        protected void btnProcessServerPunch_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            string action = hfPunchAction.Value; // "IN" or "OUT"
            string lat = hfLatitude.Value;
            string lon = hfLongitude.Value;
            string userId = HttpContext.Current.Session["USERID"].ToString();

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    if (action == "IN")
                    {
                        // 1. Determine which shift rule applies to them today
                        string shiftQuery = @"
                            SELECT TOP 1 ShiftID FROM tbl_ShiftMaster
                            WHERE ShiftID = ISNULL((
                                SELECT TOP 1 ShiftID FROM tbl_EmployeeShiftMapping 
                                WHERE UserCode = @UserCode AND EffectiveFromDate <= CAST(GETDATE() AS DATE) 
                                AND (EffectiveToDate IS NULL OR EffectiveToDate >= CAST(GETDATE() AS DATE))
                                ORDER BY EffectiveFromDate DESC
                            ), 1)";

                        int appliedShiftId = 1; // Default
                        using (SqlCommand cmdShift = new SqlCommand(shiftQuery, conn))
                        {
                            cmdShift.Parameters.AddWithValue("@UserCode", userId);
                            object result = cmdShift.ExecuteScalar();
                            if (result != null) appliedShiftId = Convert.ToInt32(result);
                        }

                        // 2. Insert Punch IN with the AppliedShiftID
                        string query = @"INSERT INTO tbl_Attendance (UserCode, ActivityDate, PunchInTime, StartLatitude, StartLongitude, AppliedShiftID, SystemCalculatedStatus) 
                                         VALUES (@UserCode, CAST(GETDATE() AS DATE), GETDATE(), @Lat, @Lon, @ShiftId, 'In-Progress')";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@UserCode", userId);
                            cmd.Parameters.AddWithValue("@Lat", string.IsNullOrEmpty(lat) ? (object)DBNull.Value : Convert.ToDecimal(lat));
                            cmd.Parameters.AddWithValue("@Lon", string.IsNullOrEmpty(lon) ? (object)DBNull.Value : Convert.ToDecimal(lon));
                            cmd.Parameters.AddWithValue("@ShiftId", appliedShiftId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else if (action == "OUT")
                    {
                        // 1. Gather IN data and Shift Rules for calculation
                        DateTime punchInTime = DateTime.Now;
                        int appliedShiftId = 1;

                        string getDataQuery = "SELECT PunchInTime, AppliedShiftID FROM tbl_Attendance WHERE UserCode = @UserCode AND ActivityDate = CAST(GETDATE() AS DATE)";
                        using (SqlCommand cmdGet = new SqlCommand(getDataQuery, conn))
                        {
                            cmdGet.Parameters.AddWithValue("@UserCode", userId);
                            using (SqlDataReader rdr = cmdGet.ExecuteReader())
                            {
                                if (rdr.Read())
                                {
                                    punchInTime = Convert.ToDateTime(rdr["PunchInTime"]);
                                    appliedShiftId = rdr["AppliedShiftID"] != DBNull.Value ? Convert.ToInt32(rdr["AppliedShiftID"]) : 1;
                                }
                            }
                        }

                        // 2. Fetch the rules for this specific shift
                        TimeSpan startTime = new TimeSpan(9, 30, 0);
                        TimeSpan endTime = new TimeSpan(17, 30, 0);
                        int graceLate = 15, graceEarly = 15;
                        decimal halfDayHrs = 4.0m, fullDayHrs = 8.0m;

                        string ruleQuery = "SELECT StartTime, EndTime, GracePeriodLateInMins, GracePeriodEarlyOutMins, HalfDayWorkingHours, FullDayWorkingHours FROM tbl_ShiftMaster WHERE ShiftID = @ShiftID";
                        using (SqlCommand cmdRule = new SqlCommand(ruleQuery, conn))
                        {
                            cmdRule.Parameters.AddWithValue("@ShiftID", appliedShiftId);
                            using (SqlDataReader rdr = cmdRule.ExecuteReader())
                            {
                                if (rdr.Read())
                                {
                                    startTime = (TimeSpan)rdr["StartTime"];
                                    endTime = (TimeSpan)rdr["EndTime"];
                                    graceLate = Convert.ToInt32(rdr["GracePeriodLateInMins"]);
                                    graceEarly = Convert.ToInt32(rdr["GracePeriodEarlyOutMins"]);
                                    halfDayHrs = Convert.ToDecimal(rdr["HalfDayWorkingHours"]);
                                    fullDayHrs = Convert.ToDecimal(rdr["FullDayWorkingHours"]);
                                }
                            }
                        }

                        // 3. Perform Calculations in C#
                        DateTime punchOutTime = DateTime.Now;
                        TimeSpan timeWorked = punchOutTime - punchInTime;
                        decimal totalHours = (decimal)timeWorked.TotalHours;

                        int lateByMins = 0;
                        if (punchInTime.TimeOfDay > startTime.Add(TimeSpan.FromMinutes(graceLate)))
                        {
                            lateByMins = (int)(punchInTime.TimeOfDay - startTime).TotalMinutes;
                        }

                        int earlyOutMins = 0;
                        if (punchOutTime.TimeOfDay < endTime.Subtract(TimeSpan.FromMinutes(graceEarly)))
                        {
                            earlyOutMins = (int)(endTime - punchOutTime.TimeOfDay).TotalMinutes;
                        }

                        int overTimeMins = 0;
                        if (punchOutTime.TimeOfDay > endTime)
                        {
                            overTimeMins = (int)(punchOutTime.TimeOfDay - endTime).TotalMinutes;
                        }

                        // Logic for final Status String
                        string finalStatus = "Present";
                        if (totalHours < halfDayHrs)
                            finalStatus = "Absent (Short Hours)";
                        else if (totalHours < fullDayHrs)
                            finalStatus = "Half-Day";

                        if (lateByMins > 0 && finalStatus == "Present")
                            finalStatus = "Present (Late)";

                        // 4. Update the Record
                        string updateQuery = @"
                            UPDATE tbl_Attendance 
                            SET PunchOutTime = @OutTime, EndLatitude = @Lat, EndLongitude = @Lon, 
                                TotalHoursWorked = @TotalHrs, LateByMins = @LateMins, EarlyOutByMins = @EarlyMins, 
                                OvertimeMins = @OTMins, SystemCalculatedStatus = @FinalStatus 
                            WHERE UserCode = @UserCode AND ActivityDate = CAST(GETDATE() AS DATE)";

                        using (SqlCommand cmdUpdate = new SqlCommand(updateQuery, conn))
                        {
                            cmdUpdate.Parameters.AddWithValue("@OutTime", punchOutTime);
                            cmdUpdate.Parameters.AddWithValue("@Lat", string.IsNullOrEmpty(lat) ? (object)DBNull.Value : Convert.ToDecimal(lat));
                            cmdUpdate.Parameters.AddWithValue("@Lon", string.IsNullOrEmpty(lon) ? (object)DBNull.Value : Convert.ToDecimal(lon));
                            cmdUpdate.Parameters.AddWithValue("@TotalHrs", totalHours);
                            cmdUpdate.Parameters.AddWithValue("@LateMins", lateByMins);
                            cmdUpdate.Parameters.AddWithValue("@EarlyMins", earlyOutMins);
                            cmdUpdate.Parameters.AddWithValue("@OTMins", overTimeMins);
                            cmdUpdate.Parameters.AddWithValue("@FinalStatus", finalStatus);
                            cmdUpdate.Parameters.AddWithValue("@UserCode", userId);
                            cmdUpdate.ExecuteNonQuery();
                        }
                    }
                }

                CheckTodayStatus();
                LoadAttendanceHistory();
            }
            catch (Exception ex)
            {
                lblError.Text = "An error occurred capturing attendance: " + ex.Message;
            }
        }

        private void LoadAttendanceHistory()
        {
            try
            {
                string userId = HttpContext.Current.Session["USERID"].ToString();
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    // Updated to fetch the newly calculated columns
                    string query = @"SELECT Id, ActivityDate, PunchInTime, PunchOutTime, TotalHoursWorked, LateByMins, SystemCalculatedStatus 
                                     FROM tbl_Attendance 
                                     WHERE UserCode = @UserCode 
                                     ORDER BY ActivityDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserCode", userId);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        gvAttendanceHistory.DataSource = dt;
                        gvAttendanceHistory.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "Error loading history: " + ex.Message;
            }
        }

        [System.Web.Services.WebMethod(EnableSession = true)]
        public static string GetAttendanceDetails(int id)
        {
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT ActivityDate, PunchInTime, PunchOutTime, StartLatitude, StartLongitude, EndLatitude, EndLongitude FROM tbl_Attendance WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            var details = new
                            {
                                Date = Convert.ToDateTime(rdr["ActivityDate"]).ToString("dd-MMM-yyyy"),
                                InTime = rdr["PunchInTime"] != DBNull.Value ? Convert.ToDateTime(rdr["PunchInTime"]).ToString("hh:mm tt") : "-",
                                OutTime = rdr["PunchOutTime"] != DBNull.Value ? Convert.ToDateTime(rdr["PunchOutTime"]).ToString("hh:mm tt") : "-",
                                InLat = rdr["StartLatitude"] != DBNull.Value ? rdr["StartLatitude"].ToString() : "",
                                InLon = rdr["StartLongitude"] != DBNull.Value ? rdr["StartLongitude"].ToString() : "",
                                OutLat = rdr["EndLatitude"] != DBNull.Value ? rdr["EndLatitude"].ToString() : "",
                                OutLon = rdr["EndLongitude"] != DBNull.Value ? rdr["EndLongitude"].ToString() : ""
                            };

                            JavaScriptSerializer js = new JavaScriptSerializer();
                            return js.Serialize(details);
                        }
                    }
                }
            }
            return "{}";
        }

        // 1. Updated class to include the record ID
        public class CalendarEvent
        {
            public int id { get; set; } // NEW: Used to fetch details on click
            public string title { get; set; }
            public string start { get; set; }
            public string color { get; set; }
            public string description { get; set; }
        }

        // 2. Updated WebMethod
        [System.Web.Services.WebMethod(EnableSession = true)]
        public static string GetMonthlyCalendarData(int month, int year)
        {
            if (HttpContext.Current.Session["USERID"] == null) return "[]";

            string userId = HttpContext.Current.Session["USERID"].ToString();
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

            System.Collections.Generic.List<CalendarEvent> events = new System.Collections.Generic.List<CalendarEvent>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // NEW: Added 'Id' to the SELECT statement
                string query = @"
                    SELECT Id, ActivityDate, SystemCalculatedStatus, TotalHoursWorked 
                    FROM tbl_Attendance 
                    WHERE UserCode = @UserCode 
                    AND MONTH(ActivityDate) = @Month 
                    AND YEAR(ActivityDate) = @Year";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserCode", userId);
                    cmd.Parameters.AddWithValue("@Month", month);
                    cmd.Parameters.AddWithValue("@Year", year);

                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            DateTime actDate = Convert.ToDateTime(rdr["ActivityDate"]);
                            string status = rdr["SystemCalculatedStatus"].ToString();
                            decimal hours = rdr["TotalHoursWorked"] != DBNull.Value ? Convert.ToDecimal(rdr["TotalHoursWorked"]) : 0;

                            CalendarEvent ev = new CalendarEvent();
                            ev.id = Convert.ToInt32(rdr["Id"]); // NEW: Assign the ID
                            ev.start = actDate.ToString("yyyy-MM-dd");

                            if (status.Contains("Present"))
                            {
                                ev.color = status.Contains("Late") ? "#fd7e14" : "#28a745";
                                ev.title = $"Present ({hours:F1}h)";
                            }
                            else if (status.Contains("Half-Day"))
                            {
                                ev.color = "#ffc107";
                                ev.title = $"Half-Day ({hours:F1}h)";
                            }
                            else
                            {
                                ev.color = "#dc3545";
                                ev.title = status;
                            }

                            events.Add(ev);
                        }
                    }
                }
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(events);
        }


        // --- NEW: APIs for Leave and Regularization ---

        [System.Web.Services.WebMethod(EnableSession = true)]
        public static string GetActiveLeaveTypes()
        {
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            System.Collections.Generic.List<object> leaves = new System.Collections.Generic.List<object>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Fetch active leave types to populate the dropdown
                string query = "SELECT LeaveID, LeaveName FROM tbl_LeaveMaster WHERE IsActive = 1";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            leaves.Add(new { ID = rdr["LeaveID"], Name = rdr["LeaveName"].ToString() });
                        }
                    }
                }
            }
            return new JavaScriptSerializer().Serialize(leaves);
        }

        [System.Web.Services.WebMethod(EnableSession = true)]
        public static string SubmitRegularization(string reqDate, string inTime, string outTime, string reason)
        {
            if (HttpContext.Current.Session["USERID"] == null) return "Session Expired";

            try
            {
                string userId = HttpContext.Current.Session["USERID"].ToString();
                string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string query = @"INSERT INTO tbl_AttendanceRegularization 
                                     (UserCode, AttendanceDate, RequestedInTime, RequestedOutTime, Reason, RequestStatus) 
                                     VALUES (@UserCode, @Date, @InTime, @OutTime, @Reason, 'Pending')";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserCode", userId);
                        cmd.Parameters.AddWithValue("@Date", Convert.ToDateTime(reqDate));
                        // Handling optional times (e.g., if they only forgot to punch out)
                        cmd.Parameters.AddWithValue("@InTime", string.IsNullOrEmpty(inTime) ? (object)DBNull.Value : TimeSpan.Parse(inTime));
                        cmd.Parameters.AddWithValue("@OutTime", string.IsNullOrEmpty(outTime) ? (object)DBNull.Value : TimeSpan.Parse(outTime));
                        cmd.Parameters.AddWithValue("@Reason", reason);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                return "Success";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        [System.Web.Services.WebMethod(EnableSession = true)]
        public static string SubmitLeave(string reqDate, int leaveId, string reason)
        {
            if (HttpContext.Current.Session["USERID"] == null) return "Session Expired";

            try
            {
                string userId = HttpContext.Current.Session["USERID"].ToString();
                string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string query = @"INSERT INTO tbl_LeaveRequests 
                                     (UserCode, LeaveID, StartDate, EndDate, TotalDays, Reason, RequestStatus) 
                                     VALUES (@UserCode, @LeaveID, @Date, @Date, 1.0, @Reason, 'Pending')";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserCode", userId);
                        cmd.Parameters.AddWithValue("@LeaveID", leaveId);
                        cmd.Parameters.AddWithValue("@Date", Convert.ToDateTime(reqDate));
                        cmd.Parameters.AddWithValue("@Reason", reason);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                return "Success";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        [System.Web.Services.WebMethod(EnableSession = true)]
        public static string GetShiftTimings(string reqDate)
        {
            if (HttpContext.Current.Session["USERID"] == null) return "{}";

            string userId = HttpContext.Current.Session["USERID"].ToString();
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Find the shift mapped to this user on this specific date (fallback to ShiftID 1)
                string query = @"
                    SELECT TOP 1 StartTime, EndTime FROM tbl_ShiftMaster
                    WHERE ShiftID = ISNULL((
                        SELECT TOP 1 ShiftID FROM tbl_EmployeeShiftMapping 
                        WHERE UserCode = @UserCode AND EffectiveFromDate <= @Date 
                        AND (EffectiveToDate IS NULL OR EffectiveToDate >= @Date)
                        ORDER BY EffectiveFromDate DESC
                    ), 1)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserCode", userId);
                    cmd.Parameters.AddWithValue("@Date", Convert.ToDateTime(reqDate));
                    conn.Open();

                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            // HTML <input type="time"> expects 24-hour HH:mm format
                            var timings = new
                            {
                                InTime = ((TimeSpan)rdr["StartTime"]).ToString(@"hh\:mm"),
                                OutTime = ((TimeSpan)rdr["EndTime"]).ToString(@"hh\:mm")
                            };
                            return new JavaScriptSerializer().Serialize(timings);
                        }
                    }
                }
            }
            return "{}";
        }
    }
}