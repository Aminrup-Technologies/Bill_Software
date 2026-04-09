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
                // Calculates the remaining balance dynamically
                string query = @"
                    SELECT m.LeaveName, b.TotalAllotted, b.UsedDays, 
                           (b.TotalAllotted - b.UsedDays) AS BalanceDays 
                    FROM tbl_EmployeeLeaveBalance b
                    INNER JOIN tbl_LeaveMaster m ON b.LeaveID = m.LeaveID
                    WHERE b.UserCode = @UserCode AND b.FinancialYear = YEAR(GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserCode", userId);
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
                    WHERE r.UserCode = @UserCode
                    ORDER BY r.AppliedOn DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserCode", userId);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvHistory.DataSource = dt;
                    gvHistory.DataBind();
                }
            }
        }

        protected void btnSubmitLeave_Click(object sender, EventArgs e)
        {
            try
            {
                string userId = HttpContext.Current.Session["USERID"].ToString();
                DateTime start = Convert.ToDateTime(txtStartDate.Text);
                DateTime end = Convert.ToDateTime(txtEndDate.Text);

                if (start > end)
                {
                    ShowMessage("End Date cannot be earlier than Start Date.", false);
                    return;
                }

                // Simple calculation for total days (you can enhance this later to skip weekends/holidays)
                decimal totalDays = (decimal)(end - start).TotalDays + 1;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string query = @"INSERT INTO tbl_LeaveRequests 
                                     (UserCode, LeaveID, StartDate, EndDate, TotalDays, Reason, RequestStatus) 
                                     VALUES (@UserCode, @LeaveID, @Start, @End, @Days, @Reason, 'Pending')";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserCode", userId);
                        cmd.Parameters.AddWithValue("@LeaveID", ddlLeaveType.SelectedValue);
                        cmd.Parameters.AddWithValue("@Start", start);
                        cmd.Parameters.AddWithValue("@End", end);
                        cmd.Parameters.AddWithValue("@Days", totalDays);
                        cmd.Parameters.AddWithValue("@Reason", txtReason.Text.Trim());

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage("✅ Leave application submitted successfully. It is now pending manager approval.", true);

                // Clear form
                ddlLeaveType.SelectedIndex = 0;
                txtStartDate.Text = "";
                txtEndDate.Text = "";
                txtReason.Text = "";

                // Refresh grid
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

        // Used by the GridView to color-code the status text
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