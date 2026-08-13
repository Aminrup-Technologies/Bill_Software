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
                LoadRegularizationHistory(); // Still needed for the ASP.NET GridView
                // Note: The main Monthly History & Calendar is now loaded instantly via AJAX!
            }
        }

        // ==========================================
        // 1. PAGE LOAD HELPERS (Shift & Status)
        // ==========================================
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

        private void LoadRegularizationHistory()
        {
            string userId = HttpContext.Current.Session["USERID"].ToString();
            int companyId = CompanyContext.CurrentCompanyID;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT 
                        RequestID, 
                        AppliedOn,
                        AttendanceDate, 
                        CONVERT(varchar(15), CAST(RequestedInTime AS TIME), 100) AS RequestedInTime, 
                        CONVERT(varchar(15), CAST(RequestedOutTime AS TIME), 100) AS RequestedOutTime, 
                        Reason, 
                        RequestStatus, 
                        ManagerRemarks 
                    FROM tbl_AttendanceRegularization 
                    WHERE UserCode = @UserId AND CompanyID = @CompanyID 
                    ORDER BY AppliedOn DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@CompanyID", companyId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvRegularizations.DataSource = dt;
                    gvRegularizations.DataBind();
                }
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


        // ==========================================
        // 2. UNIFIED MONTHLY DATA SYNC (Calendar + Grid + Cards)
        // ==========================================
        [WebMethod(EnableSession = true)]
        public static string GetMonthlyData(int month, int year)
        {
            if (HttpContext.Current.Session["USERID"] == null) return "[]";

            string userId = HttpContext.Current.Session["USERID"].ToString();
            int companyId = CompanyContext.CurrentCompanyID;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    WITH DateRange AS (
                        SELECT CAST(DATEFROMPARTS(@Year, @Month, 1) AS DATE) AS CalDate
                        UNION ALL
                        SELECT DATEADD(day, 1, CalDate) FROM DateRange WHERE CalDate < EOMONTH(DATEFROMPARTS(@Year, @Month, 1))
                    ),
                    OfficeAttendance AS (
                        SELECT Id AS AttendanceID, ActivityDate, PunchInTime, PunchOutTime, TotalHoursWorked, 
                               SystemCalculatedStatus, AttendanceCode, PayableDay, LateByMins, EarlyOutByMins, OvertimeMins
                        FROM tbl_Attendance 
                        WHERE CompanyID = @CompanyID AND UserCode = @UserId AND MONTH(ActivityDate) = @Month AND YEAR(ActivityDate) = @Year
                    ),
                    HolidayData AS (
                        SELECT HolidayDate, HolidayName, HolidayType
                        FROM tbl_HolidayMaster
                        WHERE CompanyID = @CompanyID AND MONTH(HolidayDate) = @Month AND YEAR(HolidayDate) = @Year
                    ),
                    LeaveData AS (
                        SELECT lr.UserCode, d.CalDate AS LeaveDate, lr.RequestStatus, lm.LeaveName
                        FROM tbl_LeaveRequests lr
                        LEFT JOIN tbl_LeaveMaster lm ON lr.LeaveID = lm.LeaveID
                        CROSS JOIN DateRange d
                        WHERE lr.CompanyID = @CompanyID AND lr.UserCode = @UserId
                          AND lr.RequestStatus = 'Approved'
                          AND d.CalDate BETWEEN lr.StartDate AND lr.EndDate
                    )
                    SELECT 
                        d.CalDate AS ActivityDate,
                        DATENAME(weekday, d.CalDate) AS DayOfWeek,
                        oa.AttendanceID, oa.PunchInTime, oa.PunchOutTime, oa.TotalHoursWorked,
                        ISNULL(oa.AttendanceCode, '-') AS AttendanceCode,
                        ISNULL(oa.PayableDay, 0.0) AS PayableDay,
                        ISNULL(oa.LateByMins, 0) AS LateByMins,
                        ISNULL(oa.EarlyOutByMins, 0) AS EarlyOutByMins,
                        ISNULL(oa.OvertimeMins, 0) AS OvertimeMins,
                        
                        CASE 
                            WHEN d.CalDate > CAST(GETDATE() AS DATE) THEN 'Upcoming'
                            WHEN d.CalDate = CAST(GETDATE() AS DATE) AND oa.PunchInTime IS NOT NULL AND oa.PunchOutTime IS NULL THEN 'Working (Since ' + LTRIM(RIGHT(CONVERT(VARCHAR(20), oa.PunchInTime, 100), 7)) + ')'
                            WHEN d.CalDate = CAST(GETDATE() AS DATE) AND oa.PunchInTime IS NULL THEN 'Not Punched In'
                            
                            WHEN ld.RequestStatus = 'Approved' THEN 'On Leave (' + ISNULL(ld.LeaveName, 'Leave') + ')'
                            WHEN hd.HolidayType IS NOT NULL THEN 'Holiday (' + hd.HolidayName + ')'
                            WHEN oa.SystemCalculatedStatus IS NOT NULL THEN oa.SystemCalculatedStatus
                            WHEN DATENAME(weekday, d.CalDate) = 'Sunday' THEN 'Weekly Off'
                            ELSE 'Absent'
                        END AS CalculatedStatus
                    FROM DateRange d
                    LEFT JOIN OfficeAttendance oa ON d.CalDate = oa.ActivityDate
                    LEFT JOIN HolidayData hd ON d.CalDate = hd.HolidayDate
                    LEFT JOIN LeaveData ld ON d.CalDate = ld.LeaveDate
                    ORDER BY d.CalDate DESC
                    OPTION (MAXRECURSION 31);
                ";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@CompanyID", companyId);
                    cmd.Parameters.AddWithValue("@Month", month);
                    cmd.Parameters.AddWithValue("@Year", year);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var events = new List<object>();
                        var gridRows = new List<object>();

                        int totalMonthDays = 0, presentCount = 0, halfDayCount = 0, absentCount = 0, offCount = 0;
                        decimal totalPayable = 0;

                        while (reader.Read())
                        {
                            DateTime date = Convert.ToDateTime(reader["ActivityDate"]);
                            string status = reader["CalculatedStatus"].ToString();
                            string attCode = reader["AttendanceCode"].ToString();
                            string inTime = reader["PunchInTime"] != DBNull.Value ? Convert.ToDateTime(reader["PunchInTime"]).ToString("hh:mm tt") : "-";
                            string outTime = reader["PunchOutTime"] != DBNull.Value ? Convert.ToDateTime(reader["PunchOutTime"]).ToString("hh:mm tt") : "-";

                            decimal payable = reader["PayableDay"] != DBNull.Value ? Convert.ToDecimal(reader["PayableDay"]) : 0m;
                            int lateMins = reader["LateByMins"] != DBNull.Value ? Convert.ToInt32(reader["LateByMins"]) : 0;
                            int earlyMins = reader["EarlyOutByMins"] != DBNull.Value ? Convert.ToInt32(reader["EarlyOutByMins"]) : 0;
                            int otMins = reader["OvertimeMins"] != DBNull.Value ? Convert.ToInt32(reader["OvertimeMins"]) : 0;
                            string attId = reader["AttendanceID"] != DBNull.Value ? reader["AttendanceID"].ToString() : "";

                            if (status != "Upcoming")
                            {
                                totalMonthDays++;
                                totalPayable += payable;

                                if (attCode == "P" || attCode == "NHP" || attCode == "FLP") presentCount++;
                                else if (attCode == "HD") halfDayCount++;
                                else if (attCode == "A" || attCode == "LWP") absentCount++;
                                else if (status.Contains("Off") || status.Contains("Holiday") || attCode == "L") offCount++;
                            }

                            string color = "#19658A";
                            if (status.Contains("Present") || status.Contains("Working")) color = "#28a745";
                            else if (status.Contains("Half-Day")) color = "#ff9800";
                            else if (status.Contains("Absent")) color = "#dc3545";
                            else if (status.Contains("Leave")) color = "#9c27b0";

                            string desc = $"<b>Status:</b> {status}<br/><b>In:</b> {inTime} &nbsp;|&nbsp; <b>Out:</b> {outTime}<br/>";
                            if (lateMins > 0) desc += $"<span style='color:#dc3545;'>Late: {lateMins}m</span><br/>";

                            events.Add(new
                            {
                                id = attId,
                                title = status.Contains("Working") ? "Working" : (attCode != "-" ? attCode : status),
                                start = date.ToString("yyyy-MM-dd"),
                                backgroundColor = color,
                                borderColor = color,
                                description = desc
                            });

                            gridRows.Add(new
                            {
                                Date = date.ToString("dd-MMM-yyyy"),
                                Day = reader["DayOfWeek"].ToString(),
                                Status = status,
                                Code = attCode,
                                Payable = payable.ToString("0.0"),
                                In = inTime,
                                Out = outTime,
                                Hrs = reader["TotalHoursWorked"] != DBNull.Value ? Convert.ToDecimal(reader["TotalHoursWorked"]).ToString("F2") : "-",
                                Late = lateMins,
                                Early = earlyMins,
                                OT = otMins,
                                Id = attId
                            });
                        }

                        var result = new
                        {
                            Events = events,
                            Grid = gridRows,
                            Summary = new { TotalDays = totalMonthDays, PayableDays = totalPayable, Present = presentCount, HalfDays = halfDayCount, Absent = absentCount, Offs = offCount }
                        };

                        return new JavaScriptSerializer().Serialize(result);
                    }
                }
            }
        }


        // ==========================================
        // 3. GEO-FENCE & PUNCH LOGIC 
        // ==========================================
        [WebMethod(EnableSession = true)]
        public static string ProcessPunch(string action, double lat, double lng, string address)
        {
            if (HttpContext.Current.Session["USERID"] == null)
                return PunchJson("error", "Session expired.");

            string userId = HttpContext.Current.Session["USERID"].ToString();
            int companyId = CompanyContext.CurrentCompanyID;
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // 1. Fetch Geo-Rules (multi-tenant isolated)
                    string geoQuery = @"SELECT RequireGeoTagging, GeoFenceLat, GeoFenceLng, GeoFenceRadius, 
                                   ISNULL(IsOfficePunchInMandatory, 1) AS IsOfficePunchInMandatory, 
                                   ISNULL(AllowRemotePunchOut, 0) AS AllowRemotePunchOut
                            FROM tbl_login WHERE User_Id = @UserId AND CompanyID = @CompanyID";

                    bool requireGeoTagging = false;
                    bool isOfficeInMandatory = true;
                    bool allowRemoteOut = false;
                    double officeLat = 0.0, officeLng = 0.0;
                    int allowedRadius = 50;

                    using (SqlCommand cmdGeo = new SqlCommand(geoQuery, conn))
                    {
                        cmdGeo.Parameters.AddWithValue("@UserId", userId);
                        cmdGeo.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId });
                        using (SqlDataReader reader = cmdGeo.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                requireGeoTagging = Convert.ToBoolean(reader["RequireGeoTagging"]);
                                isOfficeInMandatory = Convert.ToBoolean(reader["IsOfficePunchInMandatory"]);
                                allowRemoteOut = Convert.ToBoolean(reader["AllowRemotePunchOut"]);

                                if (reader["GeoFenceLat"] != DBNull.Value) officeLat = Convert.ToDouble(reader["GeoFenceLat"]);
                                if (reader["GeoFenceLng"] != DBNull.Value) officeLng = Convert.ToDouble(reader["GeoFenceLng"]);
                                if (reader["GeoFenceRadius"] != DBNull.Value) allowedRadius = Convert.ToInt32(reader["GeoFenceRadius"]);
                            }
                        }
                    }

                    // 2. Validate GPS Restrictions
                    if (requireGeoTagging)
                    {
                        if (lat == 0 || lng == 0) return PunchJson("error", "Location data is required for your profile.");

                        double distance = CalculateDistanceInMeters(lat, lng, officeLat, officeLng);
                        int distanceMeters = (int)Math.Round(distance, MidpointRounding.AwayFromZero);

                        if (action == "IN" && isOfficeInMandatory && distance > allowedRadius)
                        {
                            InsertSystemNotification(
                                "Unauthorized Punch-In Attempt",
                                $"User {userId} attempted to punch IN from outside the authorized geo-fence ({distanceMeters}m away).",
                                "Attendance", "Danger", userId, companyId, conn);
                            return PunchJson("error",
                                $"Punch-In Rejected. You are currently {distanceMeters} meters away from the authorized zone. Limit is {allowedRadius} meters.");
                        }
                        if (action == "OUT" && !allowRemoteOut && distance > allowedRadius)
                        {
                            InsertSystemNotification(
                                "Unauthorized Punch-Out Attempt",
                                $"User {userId} attempted to punch OUT from outside the authorized geo-fence ({distanceMeters}m away).",
                                "Attendance", "Danger", userId, companyId, conn);
                            return PunchJson("error",
                                $"Punch-Out Rejected. You are currently {distanceMeters} meters away from the authorized zone. Limit is {allowedRadius} meters.");
                        }
                    }

                    // 3. Process the DB Update
                    int rowsAffected = 0;
                    if (action == "IN")
                    {
                        string checkQuery = "SELECT Id FROM tbl_Attendance WHERE UserCode = @UserId AND ActivityDate = @Today AND CompanyID = @CompanyID";
                        using (SqlCommand chkCmd = new SqlCommand(checkQuery, conn))
                        {
                            chkCmd.Parameters.AddWithValue("@UserId", userId);
                            chkCmd.Parameters.AddWithValue("@Today", today);
                            chkCmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId });
                            if (chkCmd.ExecuteScalar() != null) return PunchJson("error", "You have already punched in for today.");
                        }

                        string insertQuery = @"INSERT INTO tbl_Attendance (UserCode, ActivityDate, PunchInTime, StartLatitude, StartLongitude, CompanyID) 
                                               VALUES (@UserId, @Today, GETDATE(), @Lat, @Lng, @CompanyID)";
                        using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.Parameters.AddWithValue("@Today", today);
                            cmd.Parameters.AddWithValue("@Lat", lat);
                            cmd.Parameters.AddWithValue("@Lng", lng);
                            cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId });
                            rowsAffected = cmd.ExecuteNonQuery();
                        }
                    }
                    else if (action == "OUT")
                    {
                        string updateQuery = @"UPDATE tbl_Attendance 
                                               SET PunchOutTime = GETDATE(), EndLatitude = @Lat, EndLongitude = @Lng,
                                                   TotalHoursWorked = CAST(DATEDIFF(MINUTE, PunchInTime, GETDATE()) / 60.0 AS DECIMAL(5,2))
                                               WHERE UserCode = @UserId AND ActivityDate = @Today AND CompanyID = @CompanyID AND PunchOutTime IS NULL";
                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.Parameters.AddWithValue("@Today", today);
                            cmd.Parameters.AddWithValue("@Lat", lat);
                            cmd.Parameters.AddWithValue("@Lng", lng);
                            cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId });
                            rowsAffected = cmd.ExecuteNonQuery();
                        }

                        if (rowsAffected == 0) return PunchJson("error", "Could not punch out. You must punch in first.");
                    }

                    if (rowsAffected > 0)
                    {
                        using (SqlCommand engineCmd = new SqlCommand("sp_RunAttendanceRulesEngine", conn))
                        {
                            engineCmd.CommandType = CommandType.StoredProcedure;
                            engineCmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId });
                            engineCmd.Parameters.AddWithValue("@Month", DateTime.Now.Month);
                            engineCmd.Parameters.AddWithValue("@Year", DateTime.Now.Year);
                            engineCmd.Parameters.AddWithValue("@UserCodeList", userId);

                            engineCmd.ExecuteNonQuery();
                        }

                        InsertSystemNotification(
                            $"Attendance Punched {action}",
                            $"Employee {userId} successfully punched {action.ToLower()} from an authorized location.",
                            "Attendance", "Success", userId, companyId, conn);

                        return PunchJson("success", "Punch recorded successfully!");
                    }

                    return PunchJson("error", "Database transaction failed.");
                }
            }
            catch (Exception ex)
            {
                string safeError = ex.Message.Replace("\"", "'").Replace("\r", " ").Replace("\n", " ");
                return PunchJson("error", "System Error: " + safeError);
            }
        }


        // ==========================================
        // 4. MODALS & FORMS HELPER METHODS
        // ==========================================
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
                                 FROM tbl_Attendance WHERE Id = @Id AND UserCode = @UserId AND CompanyID = @CompanyID";
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
                            var timings = new { InTime = rdr["StartTime"].ToString(), OutTime = rdr["EndTime"].ToString() };
                            return new JavaScriptSerializer().Serialize(timings);
                        }
                    }
                }
            }
            return "{}";
        }

        [WebMethod(EnableSession = true)]
        public static string GetMyGeoFence()
        {
            if (HttpContext.Current.Session["USERID"] == null) return "{}";

            int companyId = CompanyContext.CurrentCompanyID;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT RequireGeoTagging, GeoFenceLat, GeoFenceLng, GeoFenceRadius 
                                 FROM tbl_login 
                                 WHERE User_Id = @UserId AND CompanyID = @CompanyID AND IsActive = 1";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", HttpContext.Current.Session["USERID"].ToString());
                    cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId });

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
                            return new JavaScriptSerializer().Serialize(boundaryData);
                        }
                    }
                }
            }
            return "{}";
        }


        // ==========================================
        // 5. REGULARIZATION & LEAVE SUBMISSION
        // ==========================================
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

                            -- 2. Fetch Manager Details
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
                                INSERT INTO tbl_SystemNotification 
                                    (Title, Message, ModuleCode, Severity, CreatedBy, StartDate, EndDate, IsActive, CompanyID)
                                VALUES 
                                    ('Attendance Correction', 
                                     @EmpName + ' requested regularization for ' + CONVERT(varchar, CAST(@Date AS DATE), 106), 
                                     'Attendance', 'Info', @ManagerID, GETDATE(), DATEADD(day, 30, GETDATE()), 1, @CompanyID);
                            END

                            -- 4. Output Data
                            SELECT 
                                @ManagerEmail AS ManagerEmail, @ManagerMobile AS ManagerMobile, @EmpName AS EmpName, 
                                @SendEmail AS SendEmail, @SendWA AS SendWA, @ManagerID AS ManagerID, @NewReqID AS NewRequestID;

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

                            INSERT INTO tbl_LeaveRequests (UserCode, LeaveID, StartDate, EndDate, TotalDays, Reason, RequestStatus, AppliedOn, CompanyID)
                            VALUES (@UserCode, @LeaveID, @Date, @Date, 1.0, @Reason, 'Pending', GETDATE(), @CompanyID);

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

                            IF @ManagerID IS NOT NULL
                            BEGIN
                                INSERT INTO tbl_SystemNotification 
                                    (Title, Message, ModuleCode, Severity, CreatedBy, StartDate, EndDate, IsActive, CompanyID)
                                VALUES 
                                    ('Quick Leave Request', 
                                     @EmpName + ' applied for leave on ' + CONVERT(varchar, CAST(@Date AS DATE), 106), 
                                     'Leave', 'Info', @ManagerID, GETDATE(), DATEADD(day, 30, GETDATE()), 1, @CompanyID);
                            END

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

        // ==========================================
        // UTILITY METHODS
        // ==========================================
        private static string PunchJson(string status, string message)
        {
            return new JavaScriptSerializer().Serialize(new { status = status, message = message });
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            return CalculateDistanceInMeters(lat1, lon1, lat2, lon2);
        }

        private static double CalculateDistanceInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            if (lat1 == 0.0 || lon1 == 0.0 || lat2 == 0.0 || lon2 == 0.0) return double.MaxValue;

            double earthRadiusMeters = 6371000.0;
            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0) +
                       Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                       Math.Sin(dLon / 2.0) * Math.Sin(dLon / 2.0);

            double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
            return earthRadiusMeters * c;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

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
                cmd.Parameters.AddWithValue("@Severity", severity);
                cmd.Parameters.AddWithValue("@CreatedBy", string.IsNullOrEmpty(userId) ? (object)DBNull.Value : userId);
                cmd.Parameters.AddWithValue("@CompanyID", companyId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}