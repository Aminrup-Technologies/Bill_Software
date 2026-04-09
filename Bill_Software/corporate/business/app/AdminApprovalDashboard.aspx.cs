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

        // --- 1. Load Data ---

        private void LoadPendingRegularizations()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT r.RequestID, r.UserCode, l.Name, r.AttendanceDate, 
                           CONVERT(varchar(15), CAST(r.RequestedInTime AS TIME), 100) AS RequestedInTime, 
                           CONVERT(varchar(15), CAST(r.RequestedOutTime AS TIME), 100) AS RequestedOutTime, 
                           r.Reason 
                    FROM tbl_AttendanceRegularization r
                    INNER JOIN tbl_login l ON r.UserCode = l.User_Id
                    WHERE r.RequestStatus = 'Pending'
                    ORDER BY r.AppliedOn DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
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
                    SELECT r.RequestID, r.UserCode, l.Name, m.LeaveName, r.StartDate, r.TotalDays, r.Reason 
                    FROM tbl_LeaveRequests r
                    INNER JOIN tbl_login l ON r.UserCode = l.User_Id
                    INNER JOIN tbl_LeaveMaster m ON r.LeaveID = m.LeaveID
                    WHERE r.RequestStatus = 'Pending'
                    ORDER BY r.AppliedOn DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
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

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                if (e.CommandName == "ApproveReq")
                {
                    // Update Request Status and Recalculate Attendance Table
                    string query = @"
                        BEGIN TRANSACTION;
                        
                        -- 1. Mark as Approved
                        UPDATE tbl_AttendanceRegularization 
                        SET RequestStatus = 'Approved', ResolvedOn = GETDATE(), ManagerID = @ManagerID 
                        WHERE RequestID = @ReqID;

                        -- 2. Fetch the requested details
                        DECLARE @EmpID varchar(50), @Date date, @InTime time, @OutTime time;
                        SELECT @EmpID = UserCode, @Date = AttendanceDate, @InTime = RequestedInTime, @OutTime = RequestedOutTime 
                        FROM tbl_AttendanceRegularization WHERE RequestID = @ReqID;

                        -- 3. Upsert into Attendance (Create if missing, update if exists)
                        IF EXISTS (SELECT 1 FROM tbl_Attendance WHERE UserCode = @EmpID AND ActivityDate = @Date)
                        BEGIN
                            UPDATE tbl_Attendance 
                            SET PunchInTime = CAST(@Date AS DATETIME) + CAST(@InTime AS DATETIME),
                                PunchOutTime = CAST(@Date AS DATETIME) + CAST(@OutTime AS DATETIME)
                            WHERE UserCode = @EmpID AND ActivityDate = @Date;
                        END
                        ELSE
                        BEGIN
                            INSERT INTO tbl_Attendance (UserCode, ActivityDate, PunchInTime, PunchOutTime, AppliedShiftID)
                            VALUES (@EmpID, @Date, CAST(@Date AS DATETIME) + CAST(@InTime AS DATETIME), CAST(@Date AS DATETIME) + CAST(@OutTime AS DATETIME), 1);
                        END

                        -- 4. Recalculate Hours and Status for this record (Using Standard Shift logic)
                        UPDATE tbl_Attendance
                        SET 
                            TotalHoursWorked = CAST(DATEDIFF(MINUTE, PunchInTime, PunchOutTime) / 60.0 AS DECIMAL(5,2)),
                            SystemCalculatedStatus = CASE 
                                WHEN CAST(DATEDIFF(MINUTE, PunchInTime, PunchOutTime) / 60.0 AS DECIMAL(5,2)) < 4.0 THEN 'Absent (Short Hours)'
                                WHEN CAST(DATEDIFF(MINUTE, PunchInTime, PunchOutTime) / 60.0 AS DECIMAL(5,2)) < 8.0 THEN 'Half-Day'
                                ELSE 'Present (Regularized)'
                            END
                        WHERE UserCode = @EmpID AND ActivityDate = @Date;

                        COMMIT TRANSACTION;
                    ";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ReqID", requestId);
                        cmd.Parameters.AddWithValue("@ManagerID", managerId);
                        cmd.ExecuteNonQuery();
                    }
                    ShowMessage("✅ Regularization Approved and Attendance Updated.", true);
                }
                else if (e.CommandName == "RejectReq")
                {
                    string query = "UPDATE tbl_AttendanceRegularization SET RequestStatus = 'Rejected', ResolvedOn = GETDATE(), ManagerID = @ManagerID WHERE RequestID = @ReqID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ReqID", requestId);
                        cmd.Parameters.AddWithValue("@ManagerID", managerId);
                        cmd.ExecuteNonQuery();
                    }
                    ShowMessage("❌ Regularization Request Rejected.", false);
                }
            }
            LoadPendingRegularizations();
        }

        // --- 3. Handle Leave Actions ---

        protected void gvLeaves_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int requestId = Convert.ToInt32(e.CommandArgument);
            string managerId = HttpContext.Current.Session["USERID"].ToString();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                if (e.CommandName == "ApproveLeave")
                {
                    string query = @"
                        BEGIN TRANSACTION;

                        -- 1. Mark as Approved
                        UPDATE tbl_LeaveRequests 
                        SET RequestStatus = 'Approved', ResolvedOn = GETDATE(), ManagerID = @ManagerID 
                        WHERE RequestID = @ReqID;

                        -- 2. Deduct from Balance
                        DECLARE @EmpID varchar(50), @LeaveID int, @Days decimal(5,2), @StartDate date;
                        SELECT @EmpID = UserCode, @LeaveID = LeaveID, @Days = TotalDays, @StartDate = StartDate 
                        FROM tbl_LeaveRequests WHERE RequestID = @ReqID;

                        UPDATE tbl_EmployeeLeaveBalance 
                        SET UsedDays = UsedDays + @Days 
                        WHERE UserCode = @EmpID AND LeaveID = @LeaveID AND FinancialYear = YEAR(GETDATE());

                        -- 3. Update Attendance Calendar
                        IF EXISTS (SELECT 1 FROM tbl_Attendance WHERE UserCode = @EmpID AND ActivityDate = @StartDate)
                        BEGIN
                            UPDATE tbl_Attendance SET SystemCalculatedStatus = 'On Approved Leave' WHERE UserCode = @EmpID AND ActivityDate = @StartDate;
                        END
                        ELSE
                        BEGIN
                            INSERT INTO tbl_Attendance (UserCode, ActivityDate, SystemCalculatedStatus, AppliedShiftID)
                            VALUES (@EmpID, @StartDate, 'On Approved Leave', 1);
                        END

                        COMMIT TRANSACTION;
                    ";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ReqID", requestId);
                        cmd.Parameters.AddWithValue("@ManagerID", managerId);
                        cmd.ExecuteNonQuery();
                    }
                    ShowMessage("✅ Leave Approved. Balance deducted and Calendar updated.", true);
                }
                else if (e.CommandName == "RejectLeave")
                {
                    string query = "UPDATE tbl_LeaveRequests SET RequestStatus = 'Rejected', ResolvedOn = GETDATE(), ManagerID = @ManagerID WHERE RequestID = @ReqID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ReqID", requestId);
                        cmd.Parameters.AddWithValue("@ManagerID", managerId);
                        cmd.ExecuteNonQuery();
                    }
                    ShowMessage("❌ Leave Application Rejected.", false);
                }
            }
            LoadPendingLeaves();
        }

        private void ShowMessage(string msg, bool isSuccess)
        {
            if (isSuccess)
            {
                PanelOK.Visible = true;
                lblOk.Text = msg;

                // Hide the error panel
                PanelError.Visible = false;
            }
            else
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = msg;

                // Hide the success panel
                PanelOK.Visible = false;
            }
        }
    }
}