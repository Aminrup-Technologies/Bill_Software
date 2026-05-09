using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class MyLeaves : System.Web.UI.Page
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
                LoadLeaveTypes();
                LoadBalances();
                LoadHistory();
            }
        }

        // --- 1. Data Loading (Strictly Segregated by CompanyContext) ---

        private void LoadLeaveTypes()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT LeaveID, LeaveName FROM tbl_LeaveMaster WHERE IsActive = 1";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    ddlLeaveType.DataSource = dt;
                    ddlLeaveType.DataTextField = "LeaveName";
                    ddlLeaveType.DataValueField = "LeaveID";
                    ddlLeaveType.DataBind();
                    ddlLeaveType.Items.Insert(0, new ListItem("-- Select Leave Type --", ""));
                }
            }
        }

        private void LoadBalances()
        {
            string userId = HttpContext.Current.Session["USERID"].ToString();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // FIX: Changed alias from 'AvailableBalance' to 'BalanceDays' to match ASPX
                string query = @"
            SELECT m.LeaveName, b.TotalAllotted, b.UsedDays, 
                   (b.TotalAllotted - b.UsedDays) AS BalanceDays 
            FROM tbl_EmployeeLeaveBalance b
            INNER JOIN tbl_LeaveMaster m ON b.LeaveID = m.LeaveID
            WHERE b.UserCode = @UserCode AND b.CompanyID = @CompanyID AND b.FinancialYear = YEAR(GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserCode", userId);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvBalances.DataSource = dt;
                    gvBalances.DataBind();
                }
            }
        }

        private void LoadHistory()
        {
            string userId = HttpContext.Current.Session["USERID"].ToString();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT r.AppliedOn, m.LeaveName, r.StartDate, r.EndDate, r.TotalDays, r.RequestStatus 
                    FROM tbl_LeaveRequests r
                    INNER JOIN tbl_LeaveMaster m ON r.LeaveID = m.LeaveID
                    WHERE r.UserCode = @UserCode AND r.CompanyID = @CompanyID
                    ORDER BY r.AppliedOn DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserCode", userId);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvHistory.DataSource = dt;
                    gvHistory.DataBind();
                }
            }
        }

        // --- 2. Leave Submission with Schema-Aligned Notifications & Transactions ---

        protected void btnSubmitLeave_Click(object sender, EventArgs e)
        {
            try
            {
                string empId = HttpContext.Current.Session["USERID"].ToString();
                int leaveId = Convert.ToInt32(ddlLeaveType.SelectedValue);
                DateTime startDate = Convert.ToDateTime(txtStartDate.Text);
                DateTime endDate = Convert.ToDateTime(txtEndDate.Text);

                // Simple calculation (Replace with business logic for weekends/holidays if needed)
                decimal totalDays = (decimal)(endDate - startDate).TotalDays + 1;
                string reason = txtReason.Text;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string insertQuery = @"
                        BEGIN TRY
                            BEGIN TRANSACTION;

                            -- 1. Insert Leave Request strictly under current CompanyID
                            INSERT INTO tbl_LeaveRequests (UserCode, LeaveID, StartDate, EndDate, TotalDays, Reason, RequestStatus, AppliedOn, CompanyID)
                            VALUES (@UserCode, @LeaveID, @StartDate, @EndDate, @TotalDays, @Reason, 'Pending', GETDATE(), @CompanyID);

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
                                    ('New Leave Request', 
                                     @EmpName + ' has applied for ' + CAST(@TotalDays AS varchar) + ' days of leave.', 
                                     'HR/Leave', 
                                     'Info', 
                                     GETDATE(), 
                                     DATEADD(day, 30, GETDATE()), 
                                     1, 
                                     @ManagerID, 
                                     @CompanyID);
                            END

                            -- 4. Return ALL required data back to C# (including IDs for the token)
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

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserCode", empId);
                        cmd.Parameters.AddWithValue("@LeaveID", leaveId);
                        cmd.Parameters.AddWithValue("@StartDate", startDate);
                        cmd.Parameters.AddWithValue("@EndDate", endDate);
                        cmd.Parameters.AddWithValue("@TotalDays", totalDays);
                        cmd.Parameters.AddWithValue("@Reason", reason);
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

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

                                // 2. Extract IDs specifically for the Security Token
                                string managerId = reader["ManagerID"]?.ToString();
                                string newRequestID = reader["NewRequestID"]?.ToString();

                                // 3. Generate the Secure Payloads
                                string rawApprove = $"ReqID={newRequestID}&Type=Leave&Action=Approve&ManagerID={managerId}&CompanyID={CompanyContext.CurrentCompanyID}";
                                string rawReject = $"ReqID={newRequestID}&Type=Leave&Action=Reject&ManagerID={managerId}&CompanyID={CompanyContext.CurrentCompanyID}";

                                string tokenApprove = SecurityHelper.EncryptToUrlToken(rawApprove);
                                string tokenReject = SecurityHelper.EncryptToUrlToken(rawReject);

                                // Ensure this base URL points to your live server IP/Domain
                                string baseUrl = "https://exc.aagroupindia.com/corporate/business/app/";
                                string linkApprove = $"{baseUrl}QuickAction.aspx?t={tokenApprove}";
                                string linkReject = $"{baseUrl}QuickAction.aspx?t={tokenReject}";

                                // 4. Build the Rich HTML Email
                                string subject = $"Action Required: Leave Request from {empName}";
                                string htmlMessage = $@"
                                <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f9f9f9; border-radius: 8px;'>
                                    <h2 style='color: #19658A;'>Leave Request Application</h2>
                                    <p><strong>{empName}</strong> has requested leave.</p>
                                    <table style='background: white; padding: 15px; border-radius: 5px; width: 100%; border: 1px solid #ddd; margin-bottom: 20px;'>
                                        <tr><td style='padding: 5px 0;'><strong>Start Date:</strong></td><td>{startDate:dd-MMM-yyyy}</td></tr>
                                        <tr><td style='padding: 5px 0;'><strong>End Date:</strong></td><td>{endDate:dd-MMM-yyyy}</td></tr>
                                        <tr><td style='padding: 5px 0;'><strong>Total Days:</strong></td><td>{totalDays} days</td></tr>
                                        <tr><td style='padding: 5px 0;'><strong>Reason:</strong></td><td>{reason}</td></tr>
                                    </table>
                                    
                                    <div style='margin-top: 20px;'>
                                        <a href='{linkApprove}' style='background-color: #28a745; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold; margin-right: 15px; display: inline-block;'>✅ Approve Leave</a>
                                        <a href='{linkReject}' style='background-color: #dc3545; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>❌ Reject Leave</a>
                                    </div>
                                    <p style='font-size: 12px; color: #777; margin-top: 30px;'>You can approve or reject directly from this email without logging in.</p>
                                </div>";

                                // 5. Fire Gateway (Only if preferences allow)
                                string targetEmail = sendEmail ? mEmail : null;
                                string targetMobile = sendWA ? mMobile : null;

                                if (!string.IsNullOrEmpty(targetEmail) || !string.IsNullOrEmpty(targetMobile))
                                {
                                    CommunicationGateway.SendAlertsAsync(targetEmail, targetMobile, subject, htmlMessage);
                                }
                            }
                        }
                    }
                }

                ShowMessage("✅ Leave application submitted successfully. Your manager has been notified.", true);

                // Clear form and refresh grid
                ddlLeaveType.SelectedIndex = 0;
                txtStartDate.Text = "";
                txtEndDate.Text = "";
                txtReason.Text = "";
                LoadHistory();
            }
            catch (Exception ex)
            {
                ShowMessage("An error occurred: " + ex.Message, false);
            }
        }

        // --- Helper Methods ---

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

        public string GetStatusCssClass(string status)
        {
            switch (status.ToLower())
            {
                case "approved": return "status-approved";
                case "rejected": return "status-rejected";
                default: return "status-pending";
            }
        }
    }
}