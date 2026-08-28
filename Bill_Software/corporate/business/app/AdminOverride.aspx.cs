using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;
using System.Web;
using System.Web.UI;

namespace Bill_Software.corporate.business.app
{
    public partial class AdminOverride : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Security: Ensure only specific roles (e.g., HR or SuperAdmin) can access this
            if (HttpContext.Current.Session["USERID"] == null)
                Response.Redirect("~/index.aspx");

            if (!IsPostBack)
            {
                LoadAllLeaves();
                LoadAllRegs();
            }
        }

        private void LoadAllLeaves()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // NO Manager Filter -> Pulls everything for the Company
                string query = @"
                    SELECT r.RequestID, r.UserCode, l.Name AS EmpName, m.LeaveName, r.StartDate, r.EndDate, r.TotalDays, r.Reason, 
                           mgr.Name AS ManagerName, l.ReportingManagerId AS ManagerID
                    FROM tbl_LeaveRequests r
                    INNER JOIN tbl_login l ON r.UserCode = l.User_Id
                    INNER JOIN tbl_LeaveMaster m ON r.LeaveID = m.LeaveID
                    LEFT JOIN tbl_login mgr ON l.ReportingManagerId = mgr.User_Id
                    WHERE r.RequestStatus = 'Pending' AND r.CompanyID = @CompanyID
                    ORDER BY r.AppliedOn DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvAllLeaves.DataSource = dt;
                    gvAllLeaves.DataBind();
                }
            }
        }

        private void LoadAllRegs()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT r.RequestID, r.UserCode, l.Name AS EmpName, r.AttendanceDate, 
                           CONVERT(varchar(15), CAST(r.RequestedInTime AS TIME), 100) AS RequestedInTime, 
                           CONVERT(varchar(15), CAST(r.RequestedOutTime AS TIME), 100) AS RequestedOutTime, r.Reason,
                           mgr.Name AS ManagerName, l.ReportingManagerId AS ManagerID
                    FROM tbl_AttendanceRegularization r
                    INNER JOIN tbl_login l ON r.UserCode = l.User_Id
                    LEFT JOIN tbl_login mgr ON l.ReportingManagerId = mgr.User_Id
                    WHERE r.RequestStatus = 'Pending' AND r.CompanyID = @CompanyID
                    ORDER BY r.AppliedOn DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvAllRegs.DataSource = dt;
                    gvAllRegs.DataBind();
                }
            }
        }

        // --- GRID ROW COMMANDS ---

        protected void gvAllLeaves_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int rowIndex = ((GridViewRow)((Control)e.CommandSource).NamingContainer).RowIndex;
            int reqId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "ForceApprove" || e.CommandName == "ForceReject")
            {
                string status = e.CommandName == "ForceApprove" ? "Approved" : "Rejected";
                // Pass the HR/Admin's ID as the manager who resolved it
                ExecuteWorkflowTransaction("Leave", status, reqId, HttpContext.Current.Session["USERID"].ToString());
            }
            else if (e.CommandName == "ResendAlert")
            {
                string managerId = gvAllLeaves.DataKeys[rowIndex]["ManagerID"].ToString();
                string empName = gvAllLeaves.DataKeys[rowIndex]["EmpName"].ToString();
                string startDate = Convert.ToDateTime(gvAllLeaves.DataKeys[rowIndex]["StartDate"]).ToString("dd-MMM-yyyy");
                string totalDays = gvAllLeaves.DataKeys[rowIndex]["TotalDays"].ToString();

                string msgDetails = $"Start: {startDate} | Total Days: {totalDays}";
                ResendManagerAlert("Leave", reqId, managerId, empName, msgDetails);
            }
            LoadAllLeaves();
        }

        protected void gvAllRegs_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int rowIndex = ((GridViewRow)((Control)e.CommandSource).NamingContainer).RowIndex;
            int reqId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "ForceApprove" || e.CommandName == "ForceReject")
            {
                string status = e.CommandName == "ForceApprove" ? "Approved" : "Rejected";
                ExecuteWorkflowTransaction("Reg", status, reqId, HttpContext.Current.Session["USERID"].ToString());
            }
            else if (e.CommandName == "ResendAlert")
            {
                string managerId = gvAllRegs.DataKeys[rowIndex]["ManagerID"].ToString();
                string empName = gvAllRegs.DataKeys[rowIndex]["EmpName"].ToString();
                string attDate = Convert.ToDateTime(gvAllRegs.DataKeys[rowIndex]["AttendanceDate"]).ToString("dd-MMM-yyyy");
                string inTime = gvAllRegs.DataKeys[rowIndex]["RequestedInTime"].ToString();

                string msgDetails = $"Date: {attDate} | Req. In: {inTime}";
                ResendManagerAlert("Reg", reqId, managerId, empName, msgDetails);
            }
            LoadAllRegs();
        }


        // --- NOTIFICATION RESEND ENGINE ---

        private void ResendManagerAlert(string type, int reqId, string managerId, string empName, string specificDetails)
        {
            if (string.IsNullOrEmpty(managerId))
            {
                ShowMessage("Cannot send alert: Employee has no assigned manager.", false);
                return;
            }

            int companyId = CompanyContext.CurrentCompanyID;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // 1. Build the dynamic columns
                string optionalColumns = type == "Leave"
                    ? "CAST(req.EndDate AS DATETIME) AS EndDate, (SELECT LeaveName FROM tbl_LeaveMaster WHERE LeaveID = req.LeaveID) AS RequestTypeName"
                    : "NULL AS EndDate, 'Attendance Correction' AS RequestTypeName";

                string tableName = type == "Leave" ? "tbl_LeaveRequests" : "tbl_AttendanceRegularization";

                // 2. Fetch Manager AND Employee Emails
                string query = $@"
                    SELECT mgr.Email, mgr.Phone_no, mgr.EnableEmailAlerts, mgr.EnableWhatsAppAlerts,
                           req.Reason,
                           {optionalColumns},
                           emp.Email AS EmpEmail
                    FROM tbl_login mgr
                    LEFT JOIN {tableName} req ON req.RequestID = @ReqID AND req.CompanyID = @CompID
                    LEFT JOIN tbl_login emp ON req.UserCode = emp.User_Id
                    WHERE mgr.User_Id = @MgrID AND mgr.CompanyID = @CompID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MgrID", managerId);
                    cmd.Parameters.AddWithValue("@CompID", companyId);
                    cmd.Parameters.AddWithValue("@ReqID", reqId);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool sendEmail = reader["EnableEmailAlerts"] != DBNull.Value && Convert.ToBoolean(reader["EnableEmailAlerts"]);
                            bool sendWA = reader["EnableWhatsAppAlerts"] != DBNull.Value && Convert.ToBoolean(reader["EnableWhatsAppAlerts"]);

                            string email = reader["Email"]?.ToString();
                            string phone = reader["Phone_no"]?.ToString();
                            string empEmail = reader["EmpEmail"]?.ToString();

                            string reason = reader["Reason"]?.ToString() ?? "No reason provided.";
                            string requestTypeName = reader["RequestTypeName"]?.ToString() ?? type;

                            // Parse details
                            string[] detailsParts = specificDetails.Split('|');
                            string detailRow1 = detailsParts.Length > 0 ? detailsParts[0].Trim() : "";
                            string detailRow2 = detailsParts.Length > 1 ? detailsParts[1].Trim() : "";

                            string endDateRow = "";
                            if (type == "Leave" && reader["EndDate"] != DBNull.Value)
                            {
                                string endDate = Convert.ToDateTime(reader["EndDate"]).ToString("dd-MMM-yyyy");
                                endDateRow = $@"
                                    <tr>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee;'><strong>End Date:</strong></td>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee;'>{endDate}</td>
                                    </tr>";
                            }

                            // Generate fresh secure tokens ONLY for the manager
                            string rawApprove = $"ReqID={reqId}&Type={type}&Action=Approve&ManagerID={managerId}&CompanyID={companyId}";
                            string rawReject = $"ReqID={reqId}&Type={type}&Action=Reject&ManagerID={managerId}&CompanyID={companyId}";

                            string baseUrl = "https://exc.aagroupindia.com/corporate/business/app/";
                            string linkApprove = $"{baseUrl}QuickAction.aspx?t={SecurityHelper.EncryptToUrlToken(rawApprove)}";
                            string linkReject = $"{baseUrl}QuickAction.aspx?t={SecurityHelper.EncryptToUrlToken(rawReject)}";

                            // Shared Data Table HTML
                            string dataTableHtml = $@"
                                <table style='background: white; padding: 15px; border-radius: 5px; width: 100%; border: 1px solid #ddd; margin-bottom: 25px; font-size: 14px; color: #444; border-collapse: collapse;'>
                                    <tr>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee; width: 40%;'><strong>Employee:</strong></td>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee;'>{empName}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee;'><strong>Request Type:</strong></td>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee;'>{requestTypeName}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee;'><strong>Start Date/Details:</strong></td>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee;'>{detailRow1}<br/>{detailRow2}</td>
                                    </tr>
                                    {endDateRow}
                                    <tr>
                                        <td style='padding: 8px; vertical-align: top;'><strong>Reason Given:</strong></td>
                                        <td style='padding: 8px; font-style: italic; color: #666;'>""{reason}""</td>
                                    </tr>
                                </table>";

                            // --- 1. EMAIL FOR MANAGER (WITH ACTION BUTTONS) ---
                            string managerSubject = $"Action Required: Pending {requestTypeName} Request for {empName}";
                            string managerHtml = $@"
                            <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f9f9f9; border-radius: 8px; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0;'>
                                <div style='text-align: center; margin-bottom: 20px;'>
                                    <h2 style='color: #d9534f; margin: 0;'>Action Required: Approval Reminder</h2>
                                </div>
                                <p style='color: #333; font-size: 15px;'>Hello Manager,</p>
                                <p style='color: #333; font-size: 15px;'>This is a reminder that you have a pending <strong>{requestTypeName.ToLower()}</strong> request from <strong>{empName}</strong> that requires your attention.</p>
                                {dataTableHtml}
                                <div style='margin-top: 20px; text-align: center;'>
                                    <a href='{linkApprove}' style='background-color: #28a745; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold; margin-right: 15px; display: inline-block; font-size: 14px;'>✅ Approve</a>
                                    <a href='{linkReject}' style='background-color: #dc3545; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block; font-size: 14px;'>❌ Reject</a>
                                </div>
                            </div>";

                            // --- 2. EMAIL FOR REQUESTOR (NO ACTION BUTTONS) ---
                            string empSubject = $"FYI: Reminder sent to your manager for {requestTypeName}";
                            string empHtml = $@"
                            <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f9f9f9; border-radius: 8px; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0;'>
                                <div style='text-align: center; margin-bottom: 20px;'>
                                    <h2 style='color: #19658A; margin: 0;'>Manager Reminder Sent</h2>
                                </div>
                                <p style='color: #333; font-size: 15px;'>Hello <strong>{empName}</strong>,</p>
                                <p style='color: #333; font-size: 15px;'>This is to notify you that HR/Admin has sent a reminder to your manager regarding your pending request. We will notify you once they take action.</p>
                                {dataTableHtml}
                            </div>";

                            // --- 3. DISPATCH SECURELY ---
                            string targetEmail = sendEmail ? email : null;
                            string targetMobile = sendWA ? phone : null;

                            if (!string.IsNullOrEmpty(targetEmail) || !string.IsNullOrEmpty(targetMobile))
                            {
                                // Send to Manager (With Buttons)
                                CommunicationGateway.SendAlertsAsync(targetEmail, targetMobile, managerSubject, managerHtml);

                                // Send to Employee (No Buttons)
                                if (!string.IsNullOrEmpty(empEmail))
                                {
                                    CommunicationGateway.SendAlertsAsync(empEmail, null, empSubject, empHtml);
                                }

                                ShowMessage("✅ Reminder sent successfully to the manager. Requestor notified.", true);
                            }
                            else
                            {
                                ShowMessage("Manager has notifications disabled in their settings.", false);
                            }
                        }
                    }
                }
            }
        }

        // --- NOTIFICATION RESEND ENGINE ---
        private void ResendManagerAlert_OLD(string type, int reqId, string managerId, string empName, string specificDetails)
        {
            if (string.IsNullOrEmpty(managerId))
            {
                ShowMessage("Cannot send alert: Employee has no assigned manager.", false);
                return;
            }

            int companyId = CompanyContext.CurrentCompanyID;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // 1. Build the dynamic columns based strictly on the table type to avoid SQL compilation errors
                string optionalColumns = type == "Leave"
                    ? "CAST(req.EndDate AS DATETIME) AS EndDate, (SELECT LeaveName FROM tbl_LeaveMaster WHERE LeaveID = req.LeaveID) AS RequestTypeName"
                    : "NULL AS EndDate, 'Attendance Correction' AS RequestTypeName";

                string tableName = type == "Leave" ? "tbl_LeaveRequests" : "tbl_AttendanceRegularization";

                // 2. Build the final query
                string query = $@"
                    SELECT mgr.Email, mgr.Phone_no, mgr.EnableEmailAlerts, mgr.EnableWhatsAppAlerts,
                           req.Reason,
                           {optionalColumns},
                           emp.Email AS EmpEmail
                    FROM tbl_login mgr
                    LEFT JOIN {tableName} req ON req.RequestID = @ReqID AND req.CompanyID = @CompID
                    LEFT JOIN tbl_login emp ON req.UserCode = emp.User_Id
                    WHERE mgr.User_Id = @MgrID AND mgr.CompanyID = @CompID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MgrID", managerId);
                    cmd.Parameters.AddWithValue("@CompID", companyId);
                    cmd.Parameters.AddWithValue("@ReqID", reqId);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool sendEmail = reader["EnableEmailAlerts"] != DBNull.Value && Convert.ToBoolean(reader["EnableEmailAlerts"]);
                            bool sendWA = reader["EnableWhatsAppAlerts"] != DBNull.Value && Convert.ToBoolean(reader["EnableWhatsAppAlerts"]);

                            string email = reader["Email"]?.ToString();
                            string phone = reader["Phone_no"]?.ToString();
                            string empEmail = reader["EmpEmail"]?.ToString();
                            string reason = reader["Reason"]?.ToString() ?? "No reason provided.";
                            string requestTypeName = reader["RequestTypeName"]?.ToString() ?? type;

                            // Generate fresh secure tokens
                            string rawApprove = $"ReqID={reqId}&Type={type}&Action=Approve&ManagerID={managerId}&CompanyID={companyId}";
                            string rawReject = $"ReqID={reqId}&Type={type}&Action=Reject&ManagerID={managerId}&CompanyID={companyId}";

                            string tokenApprove = SecurityHelper.EncryptToUrlToken(rawApprove);
                            string tokenReject = SecurityHelper.EncryptToUrlToken(rawReject);

                            string baseUrl = "https://exc.aagroupindia.com/corporate/business/app/";
                            string linkApprove = $"{baseUrl}QuickAction.aspx?t={tokenApprove}";
                            string linkReject = $"{baseUrl}QuickAction.aspx?t={tokenReject}";

                            // Build the Enriched HTML Email
                            string subject = $"REMINDER: Pending {requestTypeName} Request for {empName}";

                            // Parse specificDetails to make it look nicer in the table
                            string[] detailsParts = specificDetails.Split('|');
                            string detailRow1 = detailsParts.Length > 0 ? detailsParts[0].Trim() : "";
                            string detailRow2 = detailsParts.Length > 1 ? detailsParts[1].Trim() : "";

                            string endDateRow = "";
                            if (type == "Leave" && reader["EndDate"] != DBNull.Value)
                            {
                                string endDate = Convert.ToDateTime(reader["EndDate"]).ToString("dd-MMM-yyyy");
                                endDateRow = $@"
                                    <tr>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee;'><strong>End Date:</strong></td>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee;'>{endDate}</td>
                                    </tr>";
                            }

                            string htmlMessage = $@"
                            <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f9f9f9; border-radius: 8px; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0;'>
                                <div style='text-align: center; margin-bottom: 20px;'>
                                    <h2 style='color: #d9534f; margin: 0;'>Action Required: Approval Reminder</h2>
                                </div>
                                
                                <p style='color: #333; font-size: 15px;'>Hello Manager,</p>
                                <p style='color: #333; font-size: 15px;'>This is a reminder that you have a pending <strong>{requestTypeName.ToLower()}</strong> request from <strong>{empName}</strong> that requires your attention.</p>
                                
                                <table style='background: white; padding: 15px; border-radius: 5px; width: 100%; border: 1px solid #ddd; margin-bottom: 25px; font-size: 14px; color: #444; border-collapse: collapse;'>
                                    <tr>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee; width: 40%;'><strong>Employee:</strong></td>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee;'>{empName}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee;'><strong>Request Type:</strong></td>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee;'>{requestTypeName}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee;'><strong>Start Date/Details:</strong></td>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee;'>{detailRow1}<br/>{detailRow2}</td>
                                    </tr>
                                    {endDateRow}
                                    <tr>
                                        <td style='padding: 8px; vertical-align: top;'><strong>Reason Given:</strong></td>
                                        <td style='padding: 8px; font-style: italic; color: #666;'>""{reason}""</td>
                                    </tr>
                                </table>
                                
                                <div style='margin-top: 20px; text-align: center;'>
                                    <a href='{linkApprove}' style='background-color: #28a745; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold; margin-right: 15px; display: inline-block; font-size: 14px;'>✅ Approve</a>
                                    <a href='{linkReject}' style='background-color: #dc3545; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block; font-size: 14px;'>❌ Reject</a>
                                </div>
                                <p style='font-size: 12px; color: #777; margin-top: 30px; text-align: center;'>You can approve or reject directly from this email without logging in.</p>
                            </div>";

                            string targetEmail = sendEmail ? email : null;
                            string targetMobile = sendWA ? phone : null;

                            if (!string.IsNullOrEmpty(targetEmail) || !string.IsNullOrEmpty(targetMobile))
                            {
                                CommunicationGateway.SendAlertsAsync(targetEmail, targetMobile, subject, htmlMessage, empEmail);
                                ShowMessage("✅ Reminder sent successfully to the manager (Requestor CC'd).", true);
                            }
                            else
                            {
                                ShowMessage("Manager has notifications disabled in their settings.", false);
                            }
                        }
                        else
                        {
                            ShowMessage("Manager not found or inactive.", false);
                        }
                    }
                }
            }
        }

        // --- 4. The Master Execution Workflow ---

        private void ExecuteWorkflowTransaction(string reqType, string status, int reqId, string managerId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Safely build table-specific columns to prevent SQL compilation errors
                string tableName = reqType == "Leave" ? "tbl_LeaveRequests" : "tbl_AttendanceRegularization";
                string dateColumn = reqType == "Leave" ? "req.StartDate" : "req.AttendanceDate";
                string optionalColumns = reqType == "Leave"
                    ? "CONVERT(varchar, req.EndDate, 106) AS EndDateStr, CAST(req.TotalDays AS varchar) + ' Days' AS SpecificDetails, (SELECT LeaveName FROM tbl_LeaveMaster WHERE LeaveID = req.LeaveID) AS RequestTypeName"
                    : "NULL AS EndDateStr, CONVERT(varchar(15), CAST(req.RequestedInTime AS TIME), 100) + ' to ' + CONVERT(varchar(15), CAST(req.RequestedOutTime AS TIME), 100) AS SpecificDetails, 'Attendance Correction' AS RequestTypeName";

                conn.Open();
                string query = $@"
                    SET NOCOUNT ON;

                    BEGIN TRY
                        BEGIN TRANSACTION;

                        DECLARE @EmpID varchar(50);
                        
                        -- A. Update the Request Status
                        {(reqType == "Leave"
                            ? "UPDATE tbl_LeaveRequests SET RequestStatus = @Status, ResolvedOn = GETDATE(), ManagerID = @ManagerID WHERE RequestID = @ReqID AND CompanyID = @CompID; SELECT @EmpID = UserCode FROM tbl_LeaveRequests WHERE RequestID = @ReqID;"
                            : "UPDATE tbl_AttendanceRegularization SET RequestStatus = @Status, ResolvedOn = GETDATE(), ManagerID = @ManagerID WHERE RequestID = @ReqID AND CompanyID = @CompID; SELECT @EmpID = UserCode FROM tbl_AttendanceRegularization WHERE RequestID = @ReqID;")}

                        -- B. Execute Business Logic if APPROVED
                        IF @Status = 'Approved'
                        BEGIN
                            {(reqType == "Leave" ? @"
                                DECLARE @LeaveID int, @Days decimal(5,2), @StartDate date;
                                SELECT @LeaveID = LeaveID, @Days = TotalDays, @StartDate = StartDate FROM tbl_LeaveRequests WHERE RequestID = @ReqID;

                                UPDATE tbl_EmployeeLeaveBalance 
                                SET UsedDays = UsedDays + @Days 
                                WHERE UserCode = @EmpID AND LeaveID = @LeaveID AND FinancialYear = YEAR(GETDATE()) AND CompanyID = @CompID;

                                IF EXISTS (SELECT 1 FROM tbl_Attendance WHERE UserCode = @EmpID AND ActivityDate = @StartDate AND CompanyID = @CompID)
                                BEGIN
                                    UPDATE tbl_Attendance SET SystemCalculatedStatus = 'On Approved Leave' WHERE UserCode = @EmpID AND ActivityDate = @StartDate AND CompanyID = @CompID;
                                END
                                ELSE
                                BEGIN
                                    INSERT INTO tbl_Attendance (UserCode, ActivityDate, SystemCalculatedStatus, AppliedShiftID, CompanyID)
                                    VALUES (@EmpID, @StartDate, 'On Approved Leave', 1, @CompID);
                                END
                            " : @"
                                DECLARE @AttDate date, @InTime time, @OutTime time;
                                SELECT @AttDate = AttendanceDate, @InTime = RequestedInTime, @OutTime = RequestedOutTime FROM tbl_AttendanceRegularization WHERE RequestID = @ReqID;

                                IF EXISTS (SELECT 1 FROM tbl_Attendance WHERE UserCode = @EmpID AND ActivityDate = @AttDate AND CompanyID = @CompID)
                                BEGIN
                                    UPDATE tbl_Attendance 
                                    SET PunchInTime = CAST(@AttDate AS DATETIME) + CAST(@InTime AS DATETIME),
                                        PunchOutTime = CAST(@AttDate AS DATETIME) + CAST(@OutTime AS DATETIME),
                                        TotalHoursWorked = CAST(DATEDIFF(MINUTE, CAST(@AttDate AS DATETIME) + CAST(@InTime AS DATETIME), CAST(@AttDate AS DATETIME) + CAST(@OutTime AS DATETIME)) / 60.0 AS DECIMAL(5,2)),
                                        SystemCalculatedStatus = 'Present (Regularized)'
                                    WHERE UserCode = @EmpID AND ActivityDate = @AttDate AND CompanyID = @CompID;
                                END
                                ELSE
                                BEGIN
                                    INSERT INTO tbl_Attendance (UserCode, ActivityDate, PunchInTime, PunchOutTime, TotalHoursWorked, SystemCalculatedStatus, AppliedShiftID, CompanyID)
                                    VALUES (@EmpID, @AttDate, CAST(@AttDate AS DATETIME) + CAST(@InTime AS DATETIME), CAST(@AttDate AS DATETIME) + CAST(@OutTime AS DATETIME), 
                                            CAST(DATEDIFF(MINUTE, CAST(@AttDate AS DATETIME) + CAST(@InTime AS DATETIME), CAST(@AttDate AS DATETIME) + CAST(@OutTime AS DATETIME)) / 60.0 AS DECIMAL(5,2)), 
                                            'Present (Regularized)', 1, @CompID);
                                END
                            ")}
                        END

                        -- C. Fetch Employee Contact Info & Manager Email
                        DECLARE @EmpEmail varchar(150), @EmpMobile varchar(20), @EmpName varchar(50);
                        DECLARE @SendEmail bit, @SendWA bit;
                        DECLARE @MgrEmail varchar(150);

                        SELECT 
                            @EmpName = e.Name,
                            @EmpEmail = e.Email, 
                            @EmpMobile = e.Phone_no,
                            @SendEmail = e.EnableEmailAlerts,
                            @SendWA = e.EnableWhatsAppAlerts,
                            @MgrEmail = m.Email
                        FROM tbl_login e
                        LEFT JOIN tbl_login m ON e.ReportingManagerId = m.User_Id
                        WHERE e.User_Id = @EmpID AND e.CompanyID = @CompID AND e.IsActive = 1;

                        -- D. Insert Proactive UI Notification Logging for the Employee
                        INSERT INTO tbl_SystemNotification 
                            (Title, Message, ModuleCode, Severity, StartDate, EndDate, IsActive, CreatedBy, CompanyID)
                        VALUES 
                            ('Request ' + @Status, 
                             'Your request has been ' + LOWER(@Status) + ' by your manager.', 
                             @ModuleCode, 
                             @SeverityType, 
                             GETDATE(), 
                             DATEADD(day, 30, GETDATE()), 
                             1, 
                             @EmpID, 
                             @CompID);

                        -- E. Return Employee Data, Manager Email AND Request Details to C#
                        SELECT 
                            @EmpEmail AS EmpEmail, @EmpMobile AS EmpMobile, @EmpName AS EmpName, 
                            @SendEmail AS SendEmail, @SendWA AS SendWA, @MgrEmail AS MgrEmail,
                            req.Reason,
                            CONVERT(varchar, {dateColumn}, 106) AS StartDateStr,
                            {optionalColumns}
                        FROM {tableName} req
                        WHERE req.RequestID = @ReqID AND req.CompanyID = @CompID;

                        COMMIT TRANSACTION;
                    END TRY
                    BEGIN CATCH
                        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                        THROW;
                    END CATCH;
                ";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@ManagerID", managerId);
                    cmd.Parameters.AddWithValue("@ReqID", reqId);
                    cmd.Parameters.AddWithValue("@CompID", CompanyContext.CurrentCompanyID);

                    string moduleCode = reqType == "Leave" ? "HR/Leave" : "HR/Attendance";
                    string severityType = status == "Approved" ? "Success" : "Warning";

                    cmd.Parameters.AddWithValue("@ModuleCode", moduleCode);
                    cmd.Parameters.AddWithValue("@SeverityType", severityType);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string eEmail = reader["EmpEmail"]?.ToString();
                            string eMobile = reader["EmpMobile"]?.ToString();
                            string empName = reader["EmpName"]?.ToString();
                            string mgrEmail = reader["MgrEmail"]?.ToString();

                            string reason = reader["Reason"]?.ToString() ?? "N/A";
                            string requestTypeName = reader["RequestTypeName"]?.ToString();
                            string startDateStr = reader["StartDateStr"]?.ToString();
                            string endDateStr = reader["EndDateStr"]?.ToString();
                            string specificDetails = reader["SpecificDetails"]?.ToString();

                            bool sendEmail = reader["SendEmail"] != DBNull.Value && Convert.ToBoolean(reader["SendEmail"]);
                            bool sendWA = reader["SendWA"] != DBNull.Value && Convert.ToBoolean(reader["SendWA"]);

                            string actionColor = status == "Approved" ? "#28a745" : "#dc3545";
                            string subject = $"Update: Your {requestTypeName} request was {status}";

                            // Only generate the End Date row if it's a multi-day leave
                            string endDateRow = "";
                            if (!string.IsNullOrEmpty(endDateStr))
                            {
                                endDateRow = $@"
                                <tr>
                                    <td style='padding: 8px; border-bottom: 1px solid #eee;'><strong>End Date:</strong></td>
                                    <td style='padding: 8px; border-bottom: 1px solid #eee;'>{endDateStr}</td>
                                </tr>";
                            }

                            string htmlMessage = $@"
                            <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f9f9f9; border-radius: 8px; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0;'>
                                <div style='text-align: center; margin-bottom: 20px;'>
                                    <h2 style='color: {actionColor}; margin: 0;'>Request {status}</h2>
                                </div>
                                
                                <p style='color: #333; font-size: 15px;'>Hello <strong>{empName}</strong>,</p>
                                <p style='color: #333; font-size: 15px;'>Your recent request has been <strong style='color: {actionColor};'>{status.ToLower()}</strong>.</p>
                                
                                <table style='background: white; padding: 15px; border-radius: 5px; width: 100%; border: 1px solid #ddd; margin-bottom: 25px; font-size: 14px; color: #444; border-collapse: collapse;'>
                                    <tr>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee; width: 40%;'><strong>Request Type:</strong></td>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee;'>{requestTypeName}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee;'><strong>Date / Start Date:</strong></td>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee;'>{startDateStr}</td>
                                    </tr>
                                    {endDateRow}
                                    <tr>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee;'><strong>Details (Time/Days):</strong></td>
                                        <td style='padding: 8px; border-bottom: 1px solid #eee;'>{specificDetails}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 8px; vertical-align: top;'><strong>Your Reason:</strong></td>
                                        <td style='padding: 8px; font-style: italic; color: #666;'>""{reason}""</td>
                                    </tr>
                                </table>
                                
                                <p style='font-size: 13px; color: #555; margin-top: 20px; text-align: center;'>
                                    <a href='https://exc.aagroupindia.com/index.aspx' style='color: #19658A; text-decoration: underline; font-weight: bold;'>Log in to Flame-Ex</a> to view your updated dashboard.
                                </p>
                            </div>";

                            string targetEmail = sendEmail ? eEmail : null;
                            string targetMobile = sendWA ? eMobile : null;

                            if (!string.IsNullOrEmpty(targetEmail) || !string.IsNullOrEmpty(targetMobile))
                            {
                                // Fire email to Employee, CC'ing the Manager!
                                CommunicationGateway.SendAlertsAsync(targetEmail, targetMobile, subject, htmlMessage, mgrEmail);
                            }
                        }
                    }
                }

                ShowMessage($"✅ Request has been successfully {status.ToLower()}.", true);
            }
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
    }
}