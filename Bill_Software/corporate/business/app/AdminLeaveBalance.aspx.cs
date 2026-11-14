using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class AdminLeaveBalance : SecurePage
    {
        protected override string RequiredPermissionKey { get { return "AdminLeaveBalance"; } }

        private string ConnString
        {
            get { return ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindYears();
                LoadEmployees();
                if (ddlEmployee.Items.Count > 0)
                    LoadBalances();
            }
        }

        private void BindYears()
        {
            int currentYear = DateTime.Now.Year;
            ddlYear.Items.Clear();
            for (int i = currentYear - 1; i <= currentYear + 1; i++)
                ddlYear.Items.Add(new ListItem(i.ToString(), i.ToString()));
            ddlYear.SelectedValue = currentYear.ToString();
        }

        private void LoadEmployees()
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                string empQuery = @"
                    SELECT u.User_Id, u.Name + ' [' + u.User_Id + ']' AS DisplayName
                    FROM tbl_login u
                    INNER JOIN dbo.UserCompanyAccess uca
                        ON uca.UserId = u.Id
                       AND uca.CompanyID = @CompanyID
                       AND uca.IsActive = 1
                    WHERE u.IsActive = 1
                      AND u.User_Id NOT IN ('admin', 'AT01')
                    ORDER BY u.Name";

                using (SqlCommand cmd = new SqlCommand(empQuery, conn))
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
            }

            if (ddlEmployee.Items.Count == 0)
                ddlEmployee.Items.Insert(0, new ListItem("-- No employees in this company --", ""));
        }

        protected void btnLoad_Click(object sender, EventArgs e)
        {
            LoadBalances();
        }

        protected void btnAllocate_Click(object sender, EventArgs e)
        {
            try
            {
                string userCode = ddlEmployee.SelectedValue;
                if (string.IsNullOrWhiteSpace(userCode))
                {
                    ShowMessage("Select an employee first.", false);
                    return;
                }

                int year = Convert.ToInt32(ddlYear.SelectedValue);
                int companyId = CompanyContext.CurrentCompanyID;

                if (!EmployeeBelongsToCompany(userCode, companyId))
                {
                    ShowMessage("Employee is not an active member of this company.", false);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_AllocateEmployeeLeaves", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserCode", userCode);
                        cmd.Parameters.AddWithValue("@FinancialYear", year);
                        cmd.Parameters.AddWithValue("@CompanyID", companyId);
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage("Allocation SP completed for " + userCode + " / FY " + year + ".", true);
                LoadBalances();
            }
            catch (Exception ex)
            {
                ShowMessage("Allocation failed: " + ex.Message, false);
            }
        }

        protected void gvBalances_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "SaveAllotted")
                return;

            try
            {
                int index = Convert.ToInt32(e.CommandArgument);
                int balanceId = Convert.ToInt32(gvBalances.DataKeys[index].Value);
                GridViewRow row = gvBalances.Rows[index];
                TextBox txtAllotted = (TextBox)row.FindControl("txtAllotted");

                decimal allotted;
                if (txtAllotted == null || !decimal.TryParse(txtAllotted.Text, out allotted) || allotted < 0)
                {
                    ShowMessage("Total Allotted must be a non-negative number.", false);
                    return;
                }

                string userCode = ddlEmployee.SelectedValue;
                int year = Convert.ToInt32(ddlYear.SelectedValue);
                int companyId = CompanyContext.CurrentCompanyID;

                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    string sql = @"
                        UPDATE tbl_EmployeeLeaveBalance
                        SET TotalAllotted = @TotalAllotted
                        WHERE BalanceID = @BalanceID
                          AND CompanyID = @CompanyID
                          AND UserCode = @UserCode
                          AND FinancialYear = @FinancialYear";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@TotalAllotted", allotted);
                        cmd.Parameters.AddWithValue("@BalanceID", balanceId);
                        cmd.Parameters.AddWithValue("@CompanyID", companyId);
                        cmd.Parameters.AddWithValue("@UserCode", userCode);
                        cmd.Parameters.AddWithValue("@FinancialYear", year);
                        conn.Open();
                        if (cmd.ExecuteNonQuery() == 0)
                        {
                            ShowMessage("Balance row not found for this company/employee/year.", false);
                            return;
                        }
                    }
                }

                ShowMessage("Total Allotted updated.", true);
                LoadBalances();
            }
            catch (Exception ex)
            {
                ShowMessage("Error saving allotment: " + ex.Message, false);
            }
        }

        private void LoadBalances()
        {
            try
            {
                string userCode = ddlEmployee.SelectedValue;
                if (string.IsNullOrWhiteSpace(userCode))
                {
                    gvBalances.DataSource = null;
                    gvBalances.DataBind();
                    return;
                }

                int year = Convert.ToInt32(ddlYear.SelectedValue);
                int companyId = CompanyContext.CurrentCompanyID;

                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    string sql = @"
                        SELECT b.BalanceID, m.LeaveName, b.TotalAllotted, b.UsedDays
                        FROM tbl_EmployeeLeaveBalance b
                        INNER JOIN tbl_LeaveMaster m
                            ON m.LeaveID = b.LeaveID
                           AND m.CompanyID = b.CompanyID
                        WHERE b.CompanyID = @CompanyID
                          AND b.UserCode = @UserCode
                          AND b.FinancialYear = @FinancialYear
                        ORDER BY m.IsPaid DESC, m.LeaveName ASC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CompanyID", companyId);
                        cmd.Parameters.AddWithValue("@UserCode", userCode);
                        cmd.Parameters.AddWithValue("@FinancialYear", year);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        gvBalances.DataSource = dt;
                        gvBalances.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading balances: " + ex.Message, false);
            }
        }

        private bool EmployeeBelongsToCompany(string userCode, int companyId)
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                string sql = @"
                    SELECT COUNT(1)
                    FROM tbl_login u
                    INNER JOIN dbo.UserCompanyAccess uca
                        ON uca.UserId = u.Id
                       AND uca.CompanyID = @CompanyID
                       AND uca.IsActive = 1
                    WHERE u.User_Id = @UserCode
                      AND u.IsActive = 1";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", companyId);
                    cmd.Parameters.AddWithValue("@UserCode", userCode);
                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            lblMessage.Text = message;
            lblMessage.ForeColor = isSuccess ? System.Drawing.Color.Green : System.Drawing.Color.Red;
        }
    }
}
