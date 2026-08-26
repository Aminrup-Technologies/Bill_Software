using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
                return;
            }

            if (UserRequiresUpdate())
            {
                OpenForceUpdatePopup();
                return;
            }

            if (!IsPostBack)
            {
                LoadUserProfile();
                LoadDashboardMetrics();
                LoadSystemNotifications();
            }
        }

        private void LoadSystemNotifications()
        {
            string query = @"
                SELECT TOP 15 
                    Title, Message, Severity, CreatedOn 
                FROM tbl_SystemNotification 
                WHERE IsActive = 1 
                  AND StartDate <= GETDATE() 
                  AND EndDate >= GETDATE() 
                ORDER BY CreatedOn DESC";

            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            using (SqlCommand cmd = new SqlCommand(query, DbCL.Conn))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dtNoti = new DataTable();
                    da.Fill(dtNoti);

                    if (dtNoti.Rows.Count > 0)
                    {
                        rptNotifications.DataSource = dtNoti;
                        rptNotifications.DataBind();
                        lblNoNotifications.Visible = false;
                    }
                    else
                    {
                        rptNotifications.DataSource = null;
                        rptNotifications.DataBind();
                        lblNoNotifications.Visible = true;
                    }
                }
            }

            DbCL.DisconnectDb();
        }

        private void LoadUserProfile()
        {
            string cmdstring = @"
                SELECT 
                    u.Name, u.Phone_no, u.Email, u.ProfilePictureUrl, u.RequireGeoTagging, u.EmailVerified,
                    ISNULL(des.DesignationName, 'Unassigned Designation') AS Designation,
                    ISNULL(dep.DepartmentName, 'Unassigned Department') AS Department
                FROM tbl_login u 
                LEFT JOIN tbl_Departments dep ON u.DepartmentID = dep.DepartmentID
                LEFT JOIN tbl_Designations des ON u.DesignationID = des.DesignationID
                WHERE u.User_Id = @UserId";

            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@UserId", Session["USERID"].ToString());
                using (SqlDataReader re = cmd.ExecuteReader())
                {
                    if (re.Read())
                    {
                        lblName.Text = re["Name"] != DBNull.Value ? re["Name"].ToString() : "Unknown User";
                        lblContactNo.Text = re["Phone_no"] != DBNull.Value ? re["Phone_no"].ToString() : "N/A";
                        lblEmailID.Text = re["Email"] != DBNull.Value ? re["Email"].ToString() : "N/A";

                        lblDesignation.Text = re["Designation"].ToString();
                        lblDepartment.Text = re["Department"].ToString();

                        bool emailVerified = re["EmailVerified"] != DBNull.Value && Convert.ToBoolean(re["EmailVerified"]);
                        if (emailVerified)
                        {
                            litEmailVerified.Text = "<span class='verified-badge' title='Verified via OTP'>✔️</span>";
                        }
                        else
                        {
                            litEmailVerified.Text = "<span style='color:#dc3545; font-size:12px; margin-left:auto;' title='Unverified'>⚠️</span>";
                        }

                        bool requireGeo = re["RequireGeoTagging"] != DBNull.Value ? Convert.ToBoolean(re["RequireGeoTagging"]) : true;
                        hfRequireGeo.Value = requireGeo.ToString().ToLower();

                        string picUrl = re["ProfilePictureUrl"] != DBNull.Value ? re["ProfilePictureUrl"].ToString() : "";
                        imgIdProfile.ImageUrl = string.IsNullOrEmpty(picUrl)
                            ? "~/corporate/business/WebImages/default-avatar.png"
                            : picUrl;
                    }
                }
            }
            DbCL.DisconnectDb();
        }

        private void LoadDashboardMetrics()
        {
            string userId = Session["USERID"].ToString();
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            // 1. Last Login
            using (SqlCommand cmd = new SqlCommand("SELECT LastLogin FROM tbl_login WHERE User_Id = @UserId", DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    if (result is DateTimeOffset)
                    {
                        DateTimeOffset dto = (DateTimeOffset)result;
                        lblLastLogin.Text = dto.DateTime.ToString("dd MMM yyyy, hh:mm tt");
                    }
                    else
                    {
                        lblLastLogin.Text = Convert.ToDateTime(result).ToString("dd MMM yyyy, hh:mm tt");
                    }
                }
                else
                {
                    lblLastLogin.Text = "First Login";
                }
            }

            // 2. Today's Attendance
            using (SqlCommand cmd = new SqlCommand("SELECT PunchInTime, AttendanceStatus FROM tbl_Attendance WHERE UserCode = @UserId AND ActivityDate = CAST(GETDATE() AS DATE)", DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        lblAttStatus.Text = dr["AttendanceStatus"].ToString();
                        lblAttTime.Text = "Punched In at: " + Convert.ToDateTime(dr["PunchInTime"]).ToString("hh:mm tt");

                        if (lblAttStatus.Text.ToLower() == "present")
                            lblAttStatus.ForeColor = System.Drawing.Color.Green;
                    }
                }
            }

            // 3. Monthly Attendance Count
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM tbl_Attendance WHERE UserCode = @UserId AND MONTH(ActivityDate) = MONTH(GETDATE()) AND YEAR(ActivityDate) = YEAR(GETDATE()) AND AttendanceStatus = 'Present'", DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                int daysPresent = Convert.ToInt32(cmd.ExecuteScalar());
                lblDaysPresent.Text = daysPresent + " Days";
            }

            // 4. Sales Visits TODAY
            string todaySalesQuery = @"
                SELECT 
                    COUNT(Id) AS TotalVisits,
                    SUM(CASE WHEN LinkedQuotationNo IS NOT NULL AND LinkedQuotationNo <> '' THEN 1 ELSE 0 END) AS TotalQuotes,
                    SUM(ISNULL(RevenueRealized, 0)) AS TotalRevenue
                FROM tbl_SalesVisitReport 
                WHERE CreatedByCode = @UserId AND CAST(VisitDate AS DATE) = CAST(GETDATE() AS DATE)";

            using (SqlCommand cmd = new SqlCommand(todaySalesQuery, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        lblVisitsToday.Text = dr["TotalVisits"].ToString();
                        lblQuotesToday.Text = dr["TotalQuotes"].ToString();

                        decimal revToday = dr["TotalRevenue"] != DBNull.Value ? Convert.ToDecimal(dr["TotalRevenue"]) : 0;
                        lblRevenueToday.Text = "₹" + revToday.ToString("N2");
                    }
                }
            }

            // 5. Sales Visits MONTH
            string monthSalesQuery = @"
                SELECT 
                    COUNT(Id) AS TotalVisits,
                    SUM(CASE WHEN LinkedQuotationNo IS NOT NULL AND LinkedQuotationNo <> '' THEN 1 ELSE 0 END) AS TotalQuotes,
                    SUM(ISNULL(RevenueRealized, 0)) AS TotalRevenue
                FROM tbl_SalesVisitReport 
                WHERE CreatedByCode = @UserId AND MONTH(VisitDate) = MONTH(GETDATE()) AND YEAR(VisitDate) = YEAR(GETDATE())";

            using (SqlCommand cmd = new SqlCommand(monthSalesQuery, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        lblVisitsMonth.Text = dr["TotalVisits"].ToString();
                        lblQuotesMonth.Text = dr["TotalQuotes"].ToString();

                        decimal revMonth = dr["TotalRevenue"] != DBNull.Value ? Convert.ToDecimal(dr["TotalRevenue"]) : 0;
                        lblRevenueMonth.Text = "₹" + revMonth.ToString("N2");
                    }
                }
            }

            DbCL.DisconnectDb();
        }

        private bool UserRequiresUpdate()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            string sql = "SELECT MustChangePassword, EmailVerified, Email FROM tbl_login WHERE User_Id = @UserId";
            using (SqlCommand cmd = new SqlCommand(sql, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@UserId", Session["USERID"].ToString());
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        bool mustChangePassword = dr["MustChangePassword"] != DBNull.Value && (bool)dr["MustChangePassword"];
                        bool emailVerified = dr["EmailVerified"] != DBNull.Value && (bool)dr["EmailVerified"];
                        string email = dr["Email"] != DBNull.Value ? dr["Email"].ToString() : "";

                        if (mustChangePassword || !emailVerified || string.IsNullOrEmpty(email))
                        {
                            DbCL.DisconnectDb();
                            return true;
                        }
                    }
                }
            }
            DbCL.DisconnectDb();
            return false;
        }

        private void OpenForceUpdatePopup()
        {
            string popupUrl = "/corporate/business/app/settings.aspx";
            string script = @"
                window.onload = function () {
                    window.open('" + popupUrl + @"', 'updatePopup', 'width=520,height=450,top=100,left=200,scrollbars=yes');
                };
            ";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "forceUpdatePopup", script, true);
        }
    }
}