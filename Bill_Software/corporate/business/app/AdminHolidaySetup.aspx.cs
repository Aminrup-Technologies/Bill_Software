using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class AdminHolidaySetup : SecurePage
    {
        protected override string RequiredPermissionKey { get { return "AdminHolidaySetup"; } }

        private string ConnString
        {
            get { return ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (string.IsNullOrWhiteSpace(txtFilterYear.Text))
                    txtFilterYear.Text = DateTime.Now.Year.ToString();
                LoadHolidays();
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            LoadHolidays();
        }

        protected void btnSaveHoliday_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime holidayDate;
                if (!DateTime.TryParse(txtHolidayDate.Text, out holidayDate))
                {
                    ShowMessage("Holiday date is required.", false);
                    return;
                }

                string name = (txtHolidayName.Text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    ShowMessage("Holiday name is required.", false);
                    return;
                }

                string holidayType = ddlHolidayType.SelectedValue;
                if (string.IsNullOrWhiteSpace(holidayType))
                {
                    ShowMessage("Holiday type is required.", false);
                    return;
                }

                int companyId = CompanyContext.CurrentCompanyID;

                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();

                    if (ViewState["EditHolidayID"] != null)
                    {
                        string updateSql = @"
                            UPDATE tbl_HolidayMaster
                            SET HolidayDate = @HolidayDate,
                                HolidayName = @HolidayName,
                                HolidayType = @HolidayType
                            WHERE HolidayID = @HolidayID
                              AND CompanyID = @CompanyID";

                        using (SqlCommand cmd = new SqlCommand(updateSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@HolidayDate", holidayDate.Date);
                            cmd.Parameters.AddWithValue("@HolidayName", name);
                            cmd.Parameters.AddWithValue("@HolidayType", holidayType);
                            cmd.Parameters.AddWithValue("@HolidayID", Convert.ToInt32(ViewState["EditHolidayID"]));
                            cmd.Parameters.AddWithValue("@CompanyID", companyId);

                            if (cmd.ExecuteNonQuery() == 0)
                            {
                                ShowMessage("Holiday not found for this company.", false);
                                return;
                            }
                        }

                        ShowMessage("Holiday updated successfully.", true);
                    }
                    else
                    {
                        string insertSql = @"
                            INSERT INTO tbl_HolidayMaster (HolidayDate, HolidayName, HolidayType, CompanyID)
                            VALUES (@HolidayDate, @HolidayName, @HolidayType, @CompanyID)";

                        using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@HolidayDate", holidayDate.Date);
                            cmd.Parameters.AddWithValue("@HolidayName", name);
                            cmd.Parameters.AddWithValue("@HolidayType", holidayType);
                            cmd.Parameters.AddWithValue("@CompanyID", companyId);
                            cmd.ExecuteNonQuery();
                        }

                        ShowMessage("Holiday created successfully.", true);
                    }
                }

                ResetFormState();
                if (string.IsNullOrWhiteSpace(txtFilterYear.Text))
                    txtFilterYear.Text = holidayDate.Year.ToString();
                LoadHolidays();
            }
            catch (Exception ex)
            {
                ShowMessage("Error saving holiday: " + ex.Message, false);
            }
        }

        protected void gvHolidays_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "EditHoliday" && e.CommandName != "DeleteHoliday")
                return;

            int index = Convert.ToInt32(e.CommandArgument);
            int holidayId = Convert.ToInt32(gvHolidays.DataKeys[index].Value);
            int companyId = CompanyContext.CurrentCompanyID;

            if (e.CommandName == "EditHoliday")
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(ConnString))
                    {
                        string sql = @"
                            SELECT HolidayID, HolidayDate, HolidayName, HolidayType
                            FROM tbl_HolidayMaster
                            WHERE HolidayID = @HolidayID AND CompanyID = @CompanyID";

                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@HolidayID", holidayId);
                            cmd.Parameters.AddWithValue("@CompanyID", companyId);
                            conn.Open();
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    ShowMessage("Holiday not found for this company.", false);
                                    return;
                                }

                                DateTime d = Convert.ToDateTime(reader["HolidayDate"]);
                                txtHolidayDate.Text = d.ToString("yyyy-MM-dd");
                                txtHolidayName.Text = reader["HolidayName"].ToString();
                                string typeVal = reader["HolidayType"].ToString();
                                if (ddlHolidayType.Items.FindByValue(typeVal) != null)
                                    ddlHolidayType.SelectedValue = typeVal;
                                else
                                {
                                    ddlHolidayType.Items.Insert(0, new ListItem(typeVal, typeVal));
                                    ddlHolidayType.SelectedValue = typeVal;
                                }

                                ViewState["EditHolidayID"] = holidayId;
                                btnSaveHoliday.Text = "Update Holiday";
                                btnCancelEdit.Style["display"] = "inline-block";
                                lblMessage.Text = "";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage("Error loading holiday: " + ex.Message, false);
                }
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    string sql = @"
                        DELETE FROM tbl_HolidayMaster
                        WHERE HolidayID = @HolidayID AND CompanyID = @CompanyID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@HolidayID", holidayId);
                        cmd.Parameters.AddWithValue("@CompanyID", companyId);
                        conn.Open();
                        if (cmd.ExecuteNonQuery() == 0)
                        {
                            ShowMessage("Holiday not found for this company.", false);
                            return;
                        }
                    }
                }

                ShowMessage("Holiday deleted.", true);
                ResetFormState();
                LoadHolidays();
            }
            catch (Exception ex)
            {
                ShowMessage("Error deleting holiday: " + ex.Message, false);
            }
        }

        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            ResetFormState();
        }

        private void ResetFormState()
        {
            ViewState["EditHolidayID"] = null;
            btnSaveHoliday.Text = "Save Holiday";
            btnCancelEdit.Style["display"] = "none";
            txtHolidayDate.Text = "";
            txtHolidayName.Text = "";
            ddlHolidayType.SelectedIndex = 0;
        }

        private void LoadHolidays()
        {
            try
            {
                int year;
                if (!int.TryParse(txtFilterYear.Text, out year) || year < 2000 || year > 2100)
                    year = DateTime.Now.Year;

                txtFilterYear.Text = year.ToString();

                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    string sql = @"
                        SELECT HolidayID, HolidayDate, HolidayName, HolidayType
                        FROM tbl_HolidayMaster
                        WHERE CompanyID = @CompanyID
                          AND YEAR(HolidayDate) = @Year
                        ORDER BY HolidayDate ASC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        cmd.Parameters.AddWithValue("@Year", year);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        gvHolidays.DataSource = dt;
                        gvHolidays.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading holidays: " + ex.Message, false);
            }
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            lblMessage.Text = message;
            lblMessage.ForeColor = isSuccess ? System.Drawing.Color.Green : System.Drawing.Color.Red;
        }
    }
}
