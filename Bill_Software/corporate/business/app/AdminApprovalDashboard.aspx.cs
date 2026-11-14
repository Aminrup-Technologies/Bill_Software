using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;
using System.Web;

namespace Bill_Software.corporate.business.app
{
    public partial class AdminApprovalDashboard : SecurePage
    {
        protected override string RequiredPermissionKey { get { return "AdminApprovalDashboard"; } }

        string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }

            if (!IsPostBack)
            {
                LoadPendingRegularizations();
                LoadPendingLeaves();
            }
        }

        // --- 1. Load Data (Strictly Filtered by CompanyContext) ---

        private void LoadPendingRegularizations()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Note: To restrict to a specific manager, you could add: AND r.ManagerID = @UserID
                string query = @"
                SELECT r.RequestID, r.UserCode, l.Name, r.AttendanceDate,
                       CONVERT(varchar(15), CAST(r.RequestedInTime AS TIME), 100) AS RequestedInTime,
                       CONVERT(varchar(15), CAST(r.RequestedOutTime AS TIME), 100) AS RequestedOutTime,
                       r.Reason  -- <--- ENSURE THIS IS HERE
                FROM tbl_AttendanceRegularization r
                INNER JOIN tbl_login l ON r.UserCode = l.User_Id
                WHERE r.RequestStatus = 'Pending' AND r.CompanyID = @CompanyID
                ORDER BY r.AppliedOn DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvRegularizations.DataSource = dt;
                    gvRegularizations.DataBind();
                }
            }
        }

        private void LoadPendingLeaves()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT r.RequestID, r.UserCode, l.Name, m.LeaveName, r.StartDate, r.EndDate, r.TotalDays,
                           r.Reason  -- <--- ENSURE THIS IS HERE
                    FROM tbl_LeaveRequests r
                    INNER JOIN tbl_login l ON r.UserCode = l.User_Id
                    INNER JOIN tbl_LeaveMaster m ON r.LeaveID = m.LeaveID
                    WHERE r.RequestStatus = 'Pending' AND r.CompanyID = @CompanyID
                    ORDER BY r.AppliedOn DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvLeaves.DataSource = dt;
                    gvLeaves.DataBind();
                }
            }
        }

        // --- 2. Handle Regularization Actions ---
        protected void gvRegularizations_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "ApproveReq" && e.CommandName != "RejectReq")
                return;

            int requestId;
            if (!AuthGuard.TryParsePositiveInt(Convert.ToString(e.CommandArgument), out requestId)
                || !AuthGuard.UserCanApprovePendingRegularization(requestId))
                return;

            string managerId = HttpContext.Current.Session["USERID"].ToString();
            string actionStatus = e.CommandName == "ApproveReq" ? "Approved" : "Rejected";

            ExecuteWorkflowTransaction("Reg", actionStatus, requestId, managerId);
            LoadPendingRegularizations();
        }

        // --- 3. Handle Leave Actions ---
        protected void gvLeaves_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "ApproveLeave" && e.CommandName != "RejectLeave")
                return;

            int requestId;
            if (!AuthGuard.TryParsePositiveInt(Convert.ToString(e.CommandArgument), out requestId)
                || !AuthGuard.UserCanApprovePendingLeave(requestId))
                return;

            string managerId = HttpContext.Current.Session["USERID"].ToString();
            string actionStatus = e.CommandName == "ApproveLeave" ? "Approved" : "Rejected";

            ExecuteWorkflowTransaction("Leave", actionStatus, requestId, managerId);
            LoadPendingLeaves();
        }

        // --- 4. The Master Execution Workflow ---
        private void ExecuteWorkflowTransaction(string reqType, string status, int reqId, string managerId)
        {
            int companyId = CompanyContext.CurrentCompanyID;
            string reqTypeDesc = reqType == "Leave" ? "leave" : "attendance correction";
            string moduleCode = reqType == "Leave" ? "HR/Leave" : "HR/Attendance";
            string severityType = status == "Approved" ? "Success" : "Warning";

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    string eEmail = null, eMobile = null, empName = null, mgrEmail = null;
                    bool sendEmail = false, sendWA = false;
                    string workflowEmpId = null;
                    DateTime? workflowAttDate = null;

                    try
                    {
                        // Ambient SqlTransaction owns consistency; do not BEGIN TRAN inside the batch.
                        string query = @"
                            SET NOCOUNT ON;

                            DECLARE @EmpID varchar(50);
                            DECLARE @AttDate date = NULL;

                            -- A. Update the Request Status (Pending guard; zero rows → THROW so concurrent approvers cannot reach the engine / Commit)
                            " + (reqType == "Leave"
                                        ? "UPDATE tbl_LeaveRequests SET RequestStatus = @Status, ResolvedOn = GETDATE(), ManagerID = @ManagerID WHERE RequestID = @ReqID AND CompanyID = @CompID AND RequestStatus = 'Pending'; IF @@ROWCOUNT = 0 THROW 50001, N'Request is no longer pending.', 1; SELECT @EmpID = UserCode FROM tbl_LeaveRequests WHERE RequestID = @ReqID AND CompanyID = @CompID;"
                                        : "UPDATE tbl_AttendanceRegularization SET RequestStatus = @Status, ResolvedOn = GETDATE(), ManagerID = @ManagerID WHERE RequestID = @ReqID AND CompanyID = @CompID AND RequestStatus = 'Pending'; IF @@ROWCOUNT = 0 THROW 50001, N'Request is no longer pending.', 1; SELECT @EmpID = UserCode FROM tbl_AttendanceRegularization WHERE RequestID = @ReqID AND CompanyID = @CompID;") + @"

                            -- B. Execute Business Logic if APPROVED
                            IF @Status = 'Approved'
                            BEGIN
                                " + (reqType == "Leave" ? @"
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
                                    DECLARE @InTime time, @OutTime time;
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
                                ") + @"
                            END

                            -- C. Fetch Employee Contact Info & Manager's Email (for CC)
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
                                 'Your ' + @ReqTypeDesc + ' request has been ' + LOWER(@Status) + ' by your manager.',
                                 @ModuleCode,
                                 @SeverityType,
                                 GETDATE(),
                                 DATEADD(day, 30, GETDATE()),
                                 1,
                                 @EmpID,
                                 @CompID);

                            -- E. Return Employee Data AND Manager Email to C#
                            SELECT @EmpEmail AS EmpEmail, @EmpMobile AS EmpMobile, @EmpName AS EmpName,
                                   @SendEmail AS SendEmail, @SendWA AS SendWA, @MgrEmail AS MgrEmail,
                                   @EmpID AS WorkflowEmpID, @AttDate AS WorkflowAttDate;
                        ";

                        using (SqlCommand cmd = new SqlCommand(query, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@Status", status);
                            cmd.Parameters.AddWithValue("@ManagerID", managerId);
                            cmd.Parameters.AddWithValue("@ReqID", reqId);
                            cmd.Parameters.AddWithValue("@CompID", companyId);
                            cmd.Parameters.AddWithValue("@ReqTypeDesc", reqTypeDesc);
                            cmd.Parameters.AddWithValue("@ModuleCode", moduleCode);
                            cmd.Parameters.AddWithValue("@SeverityType", severityType);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    tran.Rollback();
                                    ShowMessage("Unable to process this request.", false);
                                    return;
                                }

                                eEmail = reader["EmpEmail"]?.ToString();
                                eMobile = reader["EmpMobile"]?.ToString();
                                empName = reader["EmpName"]?.ToString();
                                mgrEmail = reader["MgrEmail"]?.ToString();
                                sendEmail = reader["SendEmail"] != DBNull.Value && Convert.ToBoolean(reader["SendEmail"]);
                                sendWA = reader["SendWA"] != DBNull.Value && Convert.ToBoolean(reader["SendWA"]);
                                workflowEmpId = reader["WorkflowEmpID"] != DBNull.Value ? reader["WorkflowEmpID"].ToString() : null;
                                if (reader["WorkflowAttDate"] != DBNull.Value)
                                    workflowAttDate = Convert.ToDateTime(reader["WorkflowAttDate"]);
                            }
                        }

                        // Recalc only after successful regularization approval (not leave / not reject).
                        if (reqType != "Leave" && status == "Approved"
                            && !string.IsNullOrEmpty(workflowEmpId) && workflowAttDate.HasValue)
                        {
                            RunAttendanceRulesEngine(conn, tran, companyId, workflowAttDate.Value, workflowEmpId);
                        }

                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        try { if (tran.Connection != null) tran.Rollback(); } catch { /* already rolled back */ }
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                        SqlException sqlEx = ex as SqlException;
                        if (sqlEx != null && sqlEx.Number == 50001)
                        {
                            // Concurrent dual-approve lost the Pending row — no engine, no Commit, no success alert.
                            ShowMessage("This request is no longer pending. Another approver may have already acted on it.", false);
                        }
                        else
                        {
                            // Engine failure or other batch error — approval write rolled back; no success alert.
                            ShowMessage("Unable to complete this request. No changes were saved.", false);
                        }
                        return;
                    }

                    string subject = $"Update: Your {reqTypeDesc} request was {status}";
                    string htmlMessage = $@"
                            <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f9f9f9; border-radius: 8px;'>
                                <h2 style='color: {(status == "Approved" ? "#28a745" : "#dc3545")};'>Request {status}</h2>
                                <p>Hello <strong>{empName}</strong>,</p>
                                <p>Your recent {reqTypeDesc} request has been <strong>{status.ToLower()}</strong> by your manager.</p>
                                <p style='margin-top: 20px;'><a href='https://exc.aagroupindia.com/index.aspx' style='color: #19658A; text-decoration: underline;'>Log in to Flame-Ex</a> to view your updated dashboard.</p>
                            </div>";

                    string targetEmail = sendEmail ? eEmail : null;
                    string targetMobile = sendWA ? eMobile : null;

                    if (!string.IsNullOrEmpty(targetEmail) || !string.IsNullOrEmpty(targetMobile))
                    {
                        CommunicationGateway.SendAlertsAsync(targetEmail, targetMobile, subject, htmlMessage, mgrEmail);
                    }

                    ShowMessage($"✅ Request has been successfully {status.ToLower()}.", true);
                }
            }
        }

        /// <summary>
        /// Same contract as attendance.aspx ProcessPunch: parameterized month/year/user for dbo.sp_RunAttendanceRulesEngine.
        /// Must run on the approval SqlTransaction so a rules failure rolls back the approval write.
        /// </summary>
        private static void RunAttendanceRulesEngine(SqlConnection conn, SqlTransaction tran, int companyId, DateTime attendanceDate, string userCode)
        {
            using (SqlCommand engineCmd = new SqlCommand("sp_RunAttendanceRulesEngine", conn, tran))
            {
                engineCmd.CommandType = CommandType.StoredProcedure;
                engineCmd.Parameters.AddWithValue("@CompanyID", companyId);
                engineCmd.Parameters.AddWithValue("@Month", attendanceDate.Month);
                engineCmd.Parameters.AddWithValue("@Year", attendanceDate.Year);
                engineCmd.Parameters.AddWithValue("@UserCodeList", userCode);
                engineCmd.ExecuteNonQuery();
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