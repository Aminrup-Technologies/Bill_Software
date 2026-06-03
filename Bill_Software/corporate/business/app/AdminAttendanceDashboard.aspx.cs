using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;
using System.IO;
using ClosedXML.Excel; // Required for Excel Export

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

                //GenerateReport(); // Auto-load data on first entry
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
                // Rule: Enforce Full-Stack Multi-Tenant Segregation
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

        // ==========================================
        // DATA FETCHING ENGINE (Refactored for Reusability)
        // ==========================================
        private DataTable GetAttendanceData()
        {
            int targetMonth = Convert.ToInt32(ddlMonth.SelectedValue);
            int targetYear = Convert.ToInt32(ddlYear.SelectedValue);
            string selectedEmp = ddlEmployee.SelectedValue;
            int companyId = CompanyContext.CurrentCompanyID; // THE SHIELD

            string sql = @"
                -- 1. Generate every day of the selected month
                WITH DateRange AS (
                    SELECT CAST(DATEFROMPARTS(@Year, @Month, 1) AS DATE) AS CalDate
                    UNION ALL
                    SELECT DATEADD(day, 1, CalDate)
                    FROM DateRange
                    WHERE CalDate < EOMONTH(DATEFROMPARTS(@Year, @Month, 1))
                ),
                -- 2. Fetch Selected Users
                TargetUsers AS (
                    SELECT User_Id, Name FROM tbl_login 
                    WHERE CompanyID = @CompanyID AND (@EmpId = 'ALL' OR User_Id = @EmpId) AND IsActive = 1
                ),
                -- 3. Create Base Matrix (Every User x Every Day)
                UserDates AS (
                    SELECT u.User_Id, u.Name, d.CalDate 
                    FROM TargetUsers u CROSS JOIN DateRange d
                ),
                -- 4. Aggregate Office Attendance
                OfficeAttendance AS (
                    SELECT UserCode, ActivityDate, PunchInTime, PunchOutTime, TotalHoursWorked, SystemCalculatedStatus 
                    FROM tbl_Attendance 
                    WHERE CompanyID = @CompanyID AND MONTH(ActivityDate) = @Month AND YEAR(ActivityDate) = @Year
                ),
                -- 5. Aggregate Field Sales (Mapped via CreatedByCode)
                FieldSales AS (
                    SELECT CreatedByCode as UserCode, CAST(VisitDate AS DATE) as VisitDate, 
                           COUNT(Id) as TotalVisits, SUM(RevenueRealized) as DailyRevenue
                    FROM tbl_SalesVisitReport 
                    WHERE CompanyID = @CompanyID AND MONTH(VisitDate) = @Month AND YEAR(VisitDate) = @Year
                    GROUP BY CreatedByCode, CAST(VisitDate AS DATE)
                ),
                -- 6. Aggregate Approved Leaves
                LeaveData AS (
                    SELECT UserCode, CAST(StartDate AS DATE) as LeaveDate, RequestStatus
                    FROM tbl_LeaveRequests
                    WHERE CompanyID = @CompanyID AND RequestStatus = 'Approved' 
                      AND MONTH(StartDate) = @Month AND YEAR(StartDate) = @Year
                )
                
                -- 7. Stitch the Final Omni-Channel View
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
                    CASE 
                        WHEN ud.CalDate > CAST(GETDATE() AS DATE) THEN 'Upcoming'
                        WHEN ld.RequestStatus = 'Approved' THEN 'Approved Leave'
                        WHEN oa.SystemCalculatedStatus IS NOT NULL THEN 'Office (' + oa.SystemCalculatedStatus + ')'
                        WHEN fs.TotalVisits > 0 THEN 'Field Sales'
                        WHEN DATENAME(weekday, ud.CalDate) = 'Sunday' THEN 'Weekly Off'
                        ELSE 'Absent'
                    END AS CalculatedStatus
                FROM UserDates ud
                LEFT JOIN OfficeAttendance oa ON ud.User_Id = oa.UserCode AND ud.CalDate = oa.ActivityDate
                LEFT JOIN FieldSales fs ON ud.User_Id = fs.UserCode AND ud.CalDate = fs.VisitDate
                LEFT JOIN LeaveData ld ON ud.User_Id = ld.UserCode AND ud.CalDate = ld.LeaveDate
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

        private void GenerateReport()
        {
            DataTable dt = GetAttendanceData();
            gvOmniAttendance.DataSource = dt;
            gvOmniAttendance.DataBind();
            CalculateSummaries(dt);

            // UX FIX: Only show the Export button if there is actual data to export
            if (dt != null && dt.Rows.Count > 0)
            {
                btnExport.Visible = true;
            }
            else
            {
                btnExport.Visible = false;
            }
        }

        private void CalculateSummaries(DataTable dt)
        {
            int officeDays = 0;
            int fieldDays = 0;
            int totalVisits = 0;
            int absents = 0;

            foreach (DataRow row in dt.Rows)
            {
                string status = row["CalculatedStatus"].ToString();

                if (status.Contains("Office")) officeDays++;
                if (status.Contains("Field")) fieldDays++;
                if (status.Contains("Absent")) absents++;

                totalVisits += Convert.ToInt32(row["FieldVisitsLogged"]);
            }

            lblTotalOffice.Text = $"{officeDays} Days";
            lblTotalField.Text = $"{fieldDays} Days";
            lblTotalVisits.Text = totalVisits.ToString();
            lblTotalAbsents.Text = $"{absents} Days";
        }

        // ==========================================
        // EXCEL EXPORT ENGINE (Multi-Sheet)
        // ==========================================
        protected void btnExport_Click(object sender, EventArgs e)
        {
            DataTable dtAttendance = GetAttendanceData();

            if (dtAttendance != null && dtAttendance.Rows.Count > 0)
            {
                // 1. Setup Sheet 1: Attendance Register
                DataTable exportDt = new DataTable("Attendance_Register");
                exportDt.Columns.Add("Date", typeof(string));
                exportDt.Columns.Add("Day", typeof(string));
                exportDt.Columns.Add("Employee Name", typeof(string));
                exportDt.Columns.Add("Daily Status", typeof(string));
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
                        row["PunchInTime"] != DBNull.Value ? Convert.ToDateTime(row["PunchInTime"]).ToString("hh:mm tt") : "-",
                        row["PunchOutTime"] != DBNull.Value ? Convert.ToDateTime(row["PunchOutTime"]).ToString("hh:mm tt") : "-",
                        row["TotalHoursWorked"] != DBNull.Value ? Convert.ToDecimal(row["TotalHoursWorked"]).ToString("F2") : "-",
                        Convert.ToInt32(row["FieldVisitsLogged"]),
                        Convert.ToDecimal(row["DailyRevenue"])
                    );
                }

                // --- Dynamic Filename Logic ---
                string empFileNamePart = "All_Employees";
                if (ddlEmployee.SelectedValue != "ALL")
                {
                    empFileNamePart = ddlEmployee.SelectedItem.Text.Replace(" [", "_").Replace("]", "").Replace(" ", "_");
                }
                string fileName = $"Attendance_{empFileNamePart}_{ddlMonth.SelectedItem.Text}_{ddlYear.SelectedValue}.xlsx";

                // 2. Build the Multi-Sheet Excel File
                using (XLWorkbook wb = new XLWorkbook())
                {
                    // Add First Sheet (Attendance)
                    var ws1 = wb.Worksheets.Add(exportDt);
                    ws1.Columns().AdjustToContents();
                    var header1 = ws1.Row(1);
                    header1.Style.Font.Bold = true;
                    header1.Style.Fill.BackgroundColor = XLColor.FromHtml("#19658A");
                    header1.Style.Font.FontColor = XLColor.White;

                    // 3. Setup Sheet 2: Detailed Sales Visits
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

                        // Add Second Sheet (Sales)
                        var ws2 = wb.Worksheets.Add(exportSalesDt);
                        ws2.Columns().AdjustToContents();
                        // Ensure text wraps for long discussion points
                        ws2.Column(6).Width = 50;
                        ws2.Column(6).Style.Alignment.WrapText = true;

                        var header2 = ws2.Row(1);
                        header2.Style.Font.Bold = true;
                        header2.Style.Fill.BackgroundColor = XLColor.FromHtml("#28a745"); // Green header for sales
                        header2.Style.Font.FontColor = XLColor.White;
                    }

                    // 4. Stream to Browser
                    Response.Clear();
                    Response.Buffer = true;
                    Response.Charset = "";
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment;filename=" + fileName);

                    using (MemoryStream MyMemoryStream = new MemoryStream())
                    {
                        wb.SaveAs(MyMemoryStream);
                        MyMemoryStream.WriteTo(Response.OutputStream);
                        Response.Flush();
                        Response.End();
                    }
                }
            }
        }

        // Helper for UI Dynamic Badges
        public string GetStatusBadgeClass(string status)
        {
            if (status.Contains("Office")) return "badge badge-office";
            if (status.Contains("Field")) return "badge badge-field";
            if (status.Contains("Leave")) return "badge badge-leave";
            if (status.Contains("Weekly Off") || status.Contains("Upcoming")) return "badge badge-off";
            if (status.Contains("Absent")) return "badge badge-absent";
            return "badge";
        }
    }
}