using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app
{
    public partial class QuickAction : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string token = Request.QueryString["t"];
                if (string.IsNullOrEmpty(token))
                {
                    ShowMessage("error", "Invalid Link", "No secure token was provided.");
                    return;
                }

                ProcessAction(token);
            }
        }

        private void ProcessAction(string token)
        {
            // Decrypt: Expected format -> "ReqID=45&Type=Leave&Action=Approve&ManagerID=ADM01&CompanyID=1"
            string decryptedData = SecurityHelper.DecryptFromUrlToken(token);

            if (string.IsNullOrEmpty(decryptedData))
            {
                ShowMessage("error", "Tampered Link", "This link is invalid or corrupted.");
                return;
            }

            // Parse the data safely
            var parameters = System.Web.HttpUtility.ParseQueryString(decryptedData);
            string reqId = parameters["ReqID"];
            string reqType = parameters["Type"]; // "Leave" or "Reg"
            string action = parameters["Action"]; // "Approve" or "Reject"
            string managerId = parameters["ManagerID"];
            string companyIdRaw = parameters["CompanyID"];

            int companyId;
            if (!int.TryParse(companyIdRaw, out companyId) || companyId <= 0)
            {
                ShowMessage("error", "Invalid Link", "Company context on this link is invalid.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 1. Verify the request is still pending
                string checkQuery = reqType == "Leave"
                    ? "SELECT RequestStatus, UserCode FROM tbl_LeaveRequests WHERE RequestID = @ReqID AND CompanyID = @CompID"
                    : "SELECT RequestStatus, UserCode FROM tbl_AttendanceRegularization WHERE RequestID = @ReqID AND CompanyID = @CompID";

                string employeeId = "";

                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@ReqID", reqId);
                    checkCmd.Parameters.AddWithValue("@CompID", companyId);

                    using (SqlDataReader checkReader = checkCmd.ExecuteReader())
                    {
                        if (checkReader.Read())
                        {
                            string currentStatus = checkReader["RequestStatus"].ToString();
                            employeeId = checkReader["UserCode"].ToString();

                            if (currentStatus != "Pending")
                            {
                                ShowMessage("error", "Action Already Taken", $"This request has already been {currentStatus}.");
                                return;
                            }
                        }
                        else
                        {
                            ShowMessage("error", "Not Found", "We could not locate this request.");
                            return;
                        }
                    }
                }

                string newStatus = action == "Approve" ? "Approved" : "Rejected";
                string reqTypeDesc = reqType == "Leave" ? "leave" : "attendance correction";
                string moduleCode = reqType == "Leave" ? "HR/Leave" : "HR/Attendance";
                string severityType = action == "Approve" ? "Success" : "Warning";

                string eEmail = null, eMobile = null, empName = null;
                bool sendEmail = false, sendWA = false;
                DateTime? workflowAttDate = null;
                bool processed = false;

                // 2. Execute the Approval/Rejection, Update Balances, rules engine (Reg approve), & Notify
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        // Ambient SqlTransaction owns consistency; do not BEGIN TRAN inside the batch.
                        string updateQuery = @"
                            SET NOCOUNT ON;

                            DECLARE @AttDate date = NULL;

                            -- A. Update the Request Status
                            " + (reqType == "Leave"
                                ? "UPDATE tbl_LeaveRequests SET RequestStatus = @Status, ResolvedOn = GETDATE(), ManagerID = @ManagerID WHERE RequestID = @ReqID AND CompanyID = @CompID AND RequestStatus = 'Pending'; IF @@ROWCOUNT = 0 THROW 50001, N'Request is no longer pending.', 1;"
                                : "UPDATE tbl_AttendanceRegularization SET RequestStatus = @Status, ResolvedOn = GETDATE(), ManagerID = @ManagerID WHERE RequestID = @ReqID AND CompanyID = @CompID AND RequestStatus = 'Pending'; IF @@ROWCOUNT = 0 THROW 50001, N'Request is no longer pending.', 1;") + @"

                            -- B. Execute Business Logic if APPROVED
                            IF @Status = 'Approved'
                            BEGIN
                                " + (reqType == "Leave" ? @"
                                    -- Leave Business Logic: Deduct Balance & Update Calendar
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
                                    -- Regularization Business Logic: Update Attendance Punches
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

                            -- C. Fetch Employee Contact Info & Preferences
                            DECLARE @EmpEmail varchar(150), @EmpMobile varchar(20), @EmpName varchar(50);
                            DECLARE @SendEmail bit, @SendWA bit;

                            SELECT
                                @EmpName = Name,
                                @EmpEmail = Email,
                                @EmpMobile = Phone_no,
                                @SendEmail = EnableEmailAlerts,
                                @SendWA = EnableWhatsAppAlerts
                            FROM tbl_login
                            WHERE User_Id = @EmpID AND CompanyID = @CompID AND IsActive = 1;

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

                            -- E. Return Employee Data to C#
                            SELECT @EmpEmail AS EmpEmail, @EmpMobile AS EmpMobile, @EmpName AS EmpName,
                                   @SendEmail AS SendEmail, @SendWA AS SendWA, @AttDate AS WorkflowAttDate;
                        ";

                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn, tran))
                        {
                            updateCmd.Parameters.AddWithValue("@Status", newStatus);
                            updateCmd.Parameters.AddWithValue("@ManagerID", managerId);
                            updateCmd.Parameters.AddWithValue("@ReqID", reqId);
                            updateCmd.Parameters.AddWithValue("@CompID", companyId);
                            updateCmd.Parameters.AddWithValue("@EmpID", employeeId);
                            updateCmd.Parameters.AddWithValue("@ReqTypeDesc", reqTypeDesc);
                            updateCmd.Parameters.AddWithValue("@ModuleCode", moduleCode);
                            updateCmd.Parameters.AddWithValue("@SeverityType", severityType);

                            using (SqlDataReader reader = updateCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    processed = true;
                                    eEmail = reader["EmpEmail"]?.ToString();
                                    eMobile = reader["EmpMobile"]?.ToString();
                                    empName = reader["EmpName"]?.ToString();
                                    sendEmail = reader["SendEmail"] != DBNull.Value && Convert.ToBoolean(reader["SendEmail"]);
                                    sendWA = reader["SendWA"] != DBNull.Value && Convert.ToBoolean(reader["SendWA"]);
                                    if (reader["WorkflowAttDate"] != DBNull.Value)
                                        workflowAttDate = Convert.ToDateTime(reader["WorkflowAttDate"]);
                                }
                            }
                        }

                        if (!processed)
                        {
                            tran.Rollback();
                            ShowMessage("error", "Unable to Process", "We could not complete this request.");
                            return;
                        }

                        // Recalc only after successful regularization approval (not leave / not reject).
                        if (reqType != "Leave" && newStatus == "Approved"
                            && !string.IsNullOrEmpty(employeeId) && workflowAttDate.HasValue)
                        {
                            RunAttendanceRulesEngine(conn, tran, companyId, workflowAttDate.Value, employeeId);
                        }

                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        try { if (tran.Connection != null) tran.Rollback(); } catch { /* already rolled back */ }
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                        ShowMessage("error", "Unable to Complete", "Unable to complete this request. No changes were saved.");
                        return;
                    }
                }

                string subject = $"Update: Your {reqTypeDesc} request was {newStatus}";
                string htmlMessage = $@"
                            <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f9f9f9; border-radius: 8px;'>
                                <h2 style='color: {(action == "Approve" ? "#28a745" : "#dc3545")};'>Request {newStatus}</h2>
                                <p>Hello <strong>{empName}</strong>,</p>
                                <p>Your recent {reqTypeDesc} request has been <strong>{newStatus.ToLower()}</strong> by your manager.</p>
                                <p style='margin-top: 20px;'><a href='https://exc.aagroupindia.com/index.aspx' style='color: #19658A; text-decoration: underline;'>Log in to Flame-Ex</a> to view your updated dashboard.</p>
                            </div>";

                string targetEmail = sendEmail ? eEmail : null;
                string targetMobile = sendWA ? eMobile : null;

                if (!string.IsNullOrEmpty(targetEmail) || !string.IsNullOrEmpty(targetMobile))
                {
                    CommunicationGateway.SendAlertsAsync(targetEmail, targetMobile, subject, htmlMessage);
                }

                ShowMessage("success", $"Request {newStatus}", $"You have successfully {newStatus.ToLower()}ed this {reqType} request.");
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

        private void ShowMessage(string type, string title, string message)
        {
            litIcon.Text = type == "success"
                ? "<div class='icon success'>✔️</div>"
                : "<div class='icon error'>✖️</div>";

            lblTitle.Text = title;
            lblMessage.Text = message;
        }
    }
}
