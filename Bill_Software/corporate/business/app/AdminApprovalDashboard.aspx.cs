using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;
using System.Web;

namespace Bill_Software.corporate.business.app
{
    public partial class AdminApprovalDashboard : System.Web.UI.Page
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
            int requestId = Convert.ToInt32(e.CommandArgument);
            string managerId = HttpContext.Current.Session["USERID"].ToString();
            string actionStatus = e.CommandName == "ApproveReq" ? "Approved" : "Rejected";

            ExecuteWorkflowTransaction("Reg", actionStatus, requestId, managerId);
            LoadPendingRegularizations();
        }

        // --- 3. Handle Leave Actions ---
        protected void gvLeaves_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int requestId = Convert.ToInt32(e.CommandArgument);
            string managerId = HttpContext.Current.Session["USERID"].ToString();
            string actionStatus = e.CommandName == "ApproveLeave" ? "Approved" : "Rejected";

            ExecuteWorkflowTransaction("Leave", actionStatus, requestId, managerId);
            LoadPendingLeaves();
        }

        // --- 4. The Master Execution Workflow ---
        private void ExecuteWorkflowTransaction(string reqType, string status, int reqId, string managerId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string query = @"
                    SET NOCOUNT ON;   

                    BEGIN TRY
                        BEGIN TRANSACTION;

                        DECLARE @EmpID varchar(50);
                
                        -- A. Update the Request Status
                        " + (reqType == "Leave"
                                    ? "UPDATE tbl_LeaveRequests SET RequestStatus = @Status, ResolvedOn = GETDATE(), ManagerID = @ManagerID WHERE RequestID = @ReqID AND CompanyID = @CompID; SELECT @EmpID = UserCode FROM tbl_LeaveRequests WHERE RequestID = @ReqID;"
                                    : "UPDATE tbl_AttendanceRegularization SET RequestStatus = @Status, ResolvedOn = GETDATE(), ManagerID = @ManagerID WHERE RequestID = @ReqID AND CompanyID = @CompID; SELECT @EmpID = UserCode FROM tbl_AttendanceRegularization WHERE RequestID = @ReqID;") + @"

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
                               @SendEmail AS SendEmail, @SendWA AS SendWA, @MgrEmail AS MgrEmail;

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

                    string reqTypeDesc = reqType == "Leave" ? "leave" : "attendance correction";
                    string moduleCode = reqType == "Leave" ? "HR/Leave" : "HR/Attendance";
                    string severityType = status == "Approved" ? "Success" : "Warning";

                    cmd.Parameters.AddWithValue("@ReqTypeDesc", reqTypeDesc);
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

                            bool sendEmail = reader["SendEmail"] != DBNull.Value && Convert.ToBoolean(reader["SendEmail"]);
                            bool sendWA = reader["SendWA"] != DBNull.Value && Convert.ToBoolean(reader["SendWA"]);

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