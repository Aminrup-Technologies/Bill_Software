using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class AdminLeaveSetup : SecurePage
    {
        protected override string RequiredPermissionKey { get { return "AdminLeaveSetup"; } }

        string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadLeaveData();
            }
        }

        protected void btnSaveLeave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtLeaveName.Text))
                {
                    ShowMessage("Leave name is required.", false);
                    return;
                }

                decimal maxDays;
                if (!decimal.TryParse(txtMaxDays.Text, out maxDays) || maxDays < 0)
                {
                    ShowMessage("Maximum days must be a non-negative number.", false);
                    return;
                }

                int companyId = CompanyContext.CurrentCompanyID;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    if (ViewState["EditLeaveID"] != null)
                    {
                        string updateSql = @"
                            UPDATE tbl_LeaveMaster
                            SET LeaveName = @LeaveName,
                                IsPaid = @IsPaid,
                                MaxDaysPerYear = @MaxDays
                            WHERE LeaveID = @LeaveID
                              AND CompanyID = @CompanyID
                              AND IsActive = 1";

                        using (SqlCommand cmd = new SqlCommand(updateSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@LeaveName", txtLeaveName.Text.Trim());
                            cmd.Parameters.AddWithValue("@IsPaid", Convert.ToInt32(ddlIsPaid.SelectedValue));
                            cmd.Parameters.AddWithValue("@MaxDays", maxDays);
                            cmd.Parameters.AddWithValue("@LeaveID", Convert.ToInt32(ViewState["EditLeaveID"]));
                            cmd.Parameters.AddWithValue("@CompanyID", companyId);

                            int rows = cmd.ExecuteNonQuery();
                            if (rows == 0)
                            {
                                ShowMessage("Leave type not found for this company (or already inactive).", false);
                                return;
                            }
                        }

                        ShowMessage("Leave category updated successfully.", true);
                    }
                    else
                    {
                        string insertSql = @"
                            INSERT INTO tbl_LeaveMaster (LeaveName, IsPaid, MaxDaysPerYear, IsActive, CompanyID)
                            VALUES (@LeaveName, @IsPaid, @MaxDays, 1, @CompanyID)";

                        using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@LeaveName", txtLeaveName.Text.Trim());
                            cmd.Parameters.AddWithValue("@IsPaid", Convert.ToInt32(ddlIsPaid.SelectedValue));
                            cmd.Parameters.AddWithValue("@MaxDays", maxDays);
                            cmd.Parameters.AddWithValue("@CompanyID", companyId);
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("Leave category created successfully.", true);
                    }
                }

                ResetFormState();
                LoadLeaveData();
            }
            catch (Exception ex)
            {
                ShowMessage("Error saving leave category: " + ex.Message, false);
            }
        }

        protected void gvLeaveTypes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "EditLeave" && e.CommandName != "DeactivateLeave")
                return;

            int index = Convert.ToInt32(e.CommandArgument);
            int leaveId = Convert.ToInt32(gvLeaveTypes.DataKeys[index].Value);
            int companyId = CompanyContext.CurrentCompanyID;

            if (e.CommandName == "EditLeave")
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        string sql = @"
                            SELECT LeaveID, LeaveName, IsPaid, MaxDaysPerYear
                            FROM tbl_LeaveMaster
                            WHERE LeaveID = @LeaveID AND CompanyID = @CompanyID AND IsActive = 1";

                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@LeaveID", leaveId);
                            cmd.Parameters.AddWithValue("@CompanyID", companyId);
                            conn.Open();
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    ShowMessage("Leave type not found for this company.", false);
                                    return;
                                }

                                txtLeaveName.Text = reader["LeaveName"].ToString();
                                txtMaxDays.Text = Convert.ToDecimal(reader["MaxDaysPerYear"]).ToString("0.##");
                                string paidVal = Convert.ToBoolean(reader["IsPaid"]) ? "1" : "0";
                                if (ddlIsPaid.Items.FindByValue(paidVal) != null)
                                    ddlIsPaid.SelectedValue = paidVal;

                                ViewState["EditLeaveID"] = leaveId;
                                btnSaveLeave.Text = "Update Leave Type";
                                btnCancelEdit.Style["display"] = "inline-block";
                                lblMessage.Text = "";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage("Error loading leave type: " + ex.Message, false);
                }
                return;
            }

            // DeactivateLeave
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string sql = @"
                        UPDATE tbl_LeaveMaster
                        SET IsActive = 0
                        WHERE LeaveID = @LeaveID
                          AND CompanyID = @CompanyID
                          AND IsActive = 1";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@LeaveID", leaveId);
                        cmd.Parameters.AddWithValue("@CompanyID", companyId);
                        conn.Open();
                        int rows = cmd.ExecuteNonQuery();
                        if (rows == 0)
                        {
                            ShowMessage("Leave type not found for this company (or already inactive).", false);
                            return;
                        }
                    }
                }

                ShowMessage("Leave type deactivated.", true);
                ResetFormState();
                LoadLeaveData();
            }
            catch (Exception ex)
            {
                ShowMessage("Error deactivating leave type: " + ex.Message, false);
            }
        }

        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            ResetFormState();
        }

        private void ResetFormState()
        {
            ViewState["EditLeaveID"] = null;
            btnSaveLeave.Text = "Save Leave Type";
            btnCancelEdit.Style["display"] = "none";
            txtLeaveName.Text = "";
            txtMaxDays.Text = "12.0";
            ddlIsPaid.SelectedIndex = 0;
        }

        private void LoadLeaveData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string query = @"SELECT LeaveID, LeaveName, IsPaid, MaxDaysPerYear 
                                     FROM tbl_LeaveMaster 
                                     WHERE IsActive = 1 AND CompanyID = @CompanyID
                                     ORDER BY IsPaid DESC, LeaveName ASC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        gvLeaveTypes.DataSource = dt;
                        gvLeaveTypes.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading data: " + ex.Message, false);
            }
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            lblMessage.Text = message;
            lblMessage.ForeColor = isSuccess ? System.Drawing.Color.Green : System.Drawing.Color.Red;
        }
    }
}
