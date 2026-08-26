using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class AdminShiftSetup : System.Web.UI.Page
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
                LoadShiftData();
            }
        }

        protected void btnSaveShift_Click(object sender, EventArgs e)
        {
            try
            {
                int companyId = CompanyContext.CurrentCompanyID;
                string adminId = Session["USERID"].ToString();

                if (string.IsNullOrWhiteSpace(txtShiftName.Text) || string.IsNullOrWhiteSpace(txtStartTime.Text) || string.IsNullOrWhiteSpace(txtEndTime.Text))
                {
                    ShowMessage("Please fill in all required base fields.", false);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();

                    try
                    {
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = conn;
                        cmd.Transaction = tran;

                        // Check if we are Editing or Creating
                        if (ViewState["EditShiftID"] != null)
                        {
                            // === UPDATE EXISTING SHIFT ===
                            cmd.CommandText = @"
                                UPDATE tbl_ShiftMaster SET 
                                    ShiftName = @ShiftName, StartTime = @StartTime, EndTime = @EndTime, 
                                    GracePeriodLateInMins = @GraceLate, GracePeriodEarlyOutMins = @GraceEarly, 
                                    HalfDayWorkingHours = @HalfDay, FullDayWorkingHours = @FullDay,
                                    MaxLateDaysAllowed = @MaxLate, LatePenalty = @LatePen,
                                    MaxEarlyOutDaysAllowed = @MaxEarly, EarlyOutPenalty = @EarlyPen,
                                    IsAutoRegularizeEnabled = @AutoReg, IsOutPunchMandatory = @OutPunchReq
                                WHERE ShiftID = @ShiftID AND CompanyID = @CompanyID";

                            cmd.Parameters.AddWithValue("@ShiftID", Convert.ToInt32(ViewState["EditShiftID"]));
                        }
                        else
                        {
                            // === INSERT NEW SHIFT ===
                            cmd.CommandText = @"
                                INSERT INTO tbl_ShiftMaster 
                                (ShiftName, StartTime, EndTime, GracePeriodLateInMins, GracePeriodEarlyOutMins, HalfDayWorkingHours, FullDayWorkingHours, 
                                 MaxLateDaysAllowed, LatePenalty, MaxEarlyOutDaysAllowed, EarlyOutPenalty, IsAutoRegularizeEnabled, IsOutPunchMandatory, IsActive, CompanyID) 
                                VALUES 
                                (@ShiftName, @StartTime, @EndTime, @GraceLate, @GraceEarly, @HalfDay, @FullDay, 
                                 @MaxLate, @LatePen, @MaxEarly, @EarlyPen, @AutoReg, @OutPunchReq, 1, @CompanyID)";
                        }

                        // Bind common parameters safely
                        cmd.Parameters.AddWithValue("@ShiftName", txtShiftName.Text.Trim());
                        cmd.Parameters.AddWithValue("@StartTime", TimeSpan.Parse(txtStartTime.Text));
                        cmd.Parameters.AddWithValue("@EndTime", TimeSpan.Parse(txtEndTime.Text));
                        cmd.Parameters.AddWithValue("@GraceLate", Convert.ToInt32(txtGraceLate.Text));
                        cmd.Parameters.AddWithValue("@GraceEarly", Convert.ToInt32(txtGraceEarly.Text));
                        cmd.Parameters.AddWithValue("@HalfDay", Convert.ToDecimal(txtHalfDayHours.Text));
                        cmd.Parameters.AddWithValue("@FullDay", Convert.ToDecimal(txtFullDayHours.Text));

                        // New Compliance Parameters
                        cmd.Parameters.AddWithValue("@MaxLate", Convert.ToInt32(txtMaxLateDays.Text));
                        cmd.Parameters.AddWithValue("@LatePen", ddlLatePenalty.SelectedValue);
                        cmd.Parameters.AddWithValue("@MaxEarly", Convert.ToInt32(txtMaxEarlyDays.Text));
                        cmd.Parameters.AddWithValue("@EarlyPen", ddlEarlyPenalty.SelectedValue);
                        cmd.Parameters.AddWithValue("@AutoReg", chkAutoRegularize.Checked ? 1 : 0);
                        cmd.Parameters.AddWithValue("@OutPunchReq", chkOutPunchMandatory.Checked ? 1 : 0);
                        cmd.Parameters.AddWithValue("@CompanyID", companyId);

                        cmd.ExecuteNonQuery();

                        // Notification Logging
                        string action = ViewState["EditShiftID"] != null ? "updated" : "created";
                        SqlCommand cmdNoti = new SqlCommand(@"INSERT INTO tbl_SystemNotification (Title, Message, ModuleCode, Severity, CreatedBy, StartDate, EndDate, IsActive, CompanyID) VALUES ('Shift Rule Modified', @Msg, 'Attendance', 'Info', @AdminId, GETDATE(), DATEADD(day, 14, GETDATE()), 1, @CompanyID)", conn, tran);
                        cmdNoti.Parameters.AddWithValue("@Msg", $"Admin {adminId} {action} the shift rule: {txtShiftName.Text.Trim()}.");
                        cmdNoti.Parameters.AddWithValue("@AdminId", adminId);
                        cmdNoti.Parameters.AddWithValue("@CompanyID", companyId);
                        cmdNoti.ExecuteNonQuery();

                        tran.Commit();

                        ShowMessage(ViewState["EditShiftID"] != null ? "✅ Shift updated successfully!" : "✅ Shift created successfully!", true);
                        ResetFormState();
                        LoadShiftData();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw new Exception("Transaction failed: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("❌ Error: " + ex.Message, false);
            }
        }

        // ==========================================
        // EDIT GRIDVIEW LOGIC
        // ==========================================
        protected void gvShifts_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditShift")
            {
                int index = Convert.ToInt32(e.CommandArgument);
                int shiftId = Convert.ToInt32(gvShifts.DataKeys[index].Value);

                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    string sql = "SELECT * FROM tbl_ShiftMaster WHERE ShiftID = @ShiftID AND CompanyID = @CompanyID";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ShiftID", shiftId);
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Populate Basic Fields
                                txtShiftName.Text = reader["ShiftName"].ToString();
                                txtStartTime.Text = reader["StartTime"].ToString();
                                txtEndTime.Text = reader["EndTime"].ToString();
                                txtGraceLate.Text = reader["GracePeriodLateInMins"].ToString();
                                txtGraceEarly.Text = reader["GracePeriodEarlyOutMins"].ToString();
                                txtHalfDayHours.Text = reader["HalfDayWorkingHours"].ToString();
                                txtFullDayHours.Text = reader["FullDayWorkingHours"].ToString();

                                // Populate Compliance Fields
                                txtMaxLateDays.Text = reader["MaxLateDaysAllowed"].ToString();
                                if (ddlLatePenalty.Items.FindByValue(reader["LatePenalty"].ToString()) != null)
                                    ddlLatePenalty.SelectedValue = reader["LatePenalty"].ToString();

                                txtMaxEarlyDays.Text = reader["MaxEarlyOutDaysAllowed"].ToString();
                                if (ddlEarlyPenalty.Items.FindByValue(reader["EarlyOutPenalty"].ToString()) != null)
                                    ddlEarlyPenalty.SelectedValue = reader["EarlyOutPenalty"].ToString();

                                chkAutoRegularize.Checked = Convert.ToBoolean(reader["IsAutoRegularizeEnabled"]);
                                chkOutPunchMandatory.Checked = Convert.ToBoolean(reader["IsOutPunchMandatory"]);

                                // Set UI State to Edit Mode
                                ViewState["EditShiftID"] = shiftId;
                                btnSaveShift.Text = "🔄 Update Shift Rule";
                                btnCancelEdit.Style["display"] = "inline-block";
                                lblMessage.Text = "";
                            }
                        }
                    }
                }
            }
        }

        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            ResetFormState();
        }

        private void ResetFormState()
        {
            ViewState["EditShiftID"] = null;
            btnSaveShift.Text = "💾 Save Shift Rule";
            btnCancelEdit.Style["display"] = "none";
            lblMessage.Text = "";

            txtShiftName.Text = "";
            txtStartTime.Text = "";
            txtEndTime.Text = "";
            txtGraceLate.Text = "15";
            txtGraceEarly.Text = "10";
            txtHalfDayHours.Text = "4.0";
            txtFullDayHours.Text = "8.0";
            txtMaxLateDays.Text = "3";
            ddlLatePenalty.SelectedValue = "Half-Day";
            txtMaxEarlyDays.Text = "3";
            ddlEarlyPenalty.SelectedValue = "Half-Day";
            chkAutoRegularize.Checked = true;
        }

        private void LoadShiftData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    string query = @"
                        SELECT ShiftID, ShiftName, 
                        CONVERT(varchar(15), StartTime, 100) AS StartTime, 
                        CONVERT(varchar(15), EndTime, 100) AS EndTime, 
                        GracePeriodLateInMins, MaxLateDaysAllowed, LatePenalty
                        FROM tbl_ShiftMaster 
                        WHERE IsActive = 1 AND CompanyID = @CompanyID
                        ORDER BY ShiftName ASC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        gvShifts.DataSource = dt;
                        gvShifts.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("❌ Error loading data: " + ex.Message, false);
            }
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            lblMessage.Text = message;
            lblMessage.ForeColor = isSuccess ? System.Drawing.Color.Green : System.Drawing.Color.Red;
        }
    }
}