using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;

namespace Bill_Software.corporate.business.app
{
    public partial class attendance : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }

            if (!IsPostBack)
            {
                lblCurrentDate.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");
                CheckTodayStatus();
                LoadAttendanceHistory();
            }
        }

        private void CheckTodayStatus()
        {
            string userId = HttpContext.Current.Session["USERID"].ToString();
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT PunchInTime, PunchOutTime 
                                 FROM tbl_Attendance 
                                 WHERE UserCode = @UserCode AND ActivityDate = CAST(GETDATE() AS DATE)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserCode", userId);
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            string inTime = rdr["PunchInTime"] != DBNull.Value ? Convert.ToDateTime(rdr["PunchInTime"]).ToString("hh:mm tt") : "";
                            lblPunchInTime.Text = "Punched In at: " + inTime;

                            if (rdr["PunchOutTime"] == DBNull.Value)
                            {
                                lblStatusBadge.Text = "Status: Punched IN (Active Shift)";
                                lblStatusBadge.CssClass = "status-badge status-in";

                                btnHtmlPunchIn.Disabled = true;
                                btnHtmlPunchOut.Disabled = false;
                            }
                            else
                            {
                                string outTime = Convert.ToDateTime(rdr["PunchOutTime"]).ToString("hh:mm tt");
                                lblPunchOutTime.Text = "Punched Out at: " + outTime;

                                lblStatusBadge.Text = "Status: Shift Completed";
                                lblStatusBadge.CssClass = "status-badge status-completed";

                                btnHtmlPunchIn.Disabled = true;
                                btnHtmlPunchOut.Disabled = true;
                            }
                        }
                        else
                        {
                            lblStatusBadge.Text = "Status: Not Punched In";
                            lblStatusBadge.CssClass = "status-badge status-out";

                            lblPunchInTime.Text = "";
                            lblPunchOutTime.Text = "";

                            btnHtmlPunchIn.Disabled = false;
                            btnHtmlPunchOut.Disabled = true;
                        }
                    }
                }
            }
        }

        protected void btnProcessServerPunch_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            string action = hfPunchAction.Value; // "IN" or "OUT"
            string lat = hfLatitude.Value;
            string lon = hfLongitude.Value;
            string userId = HttpContext.Current.Session["USERID"].ToString();

            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    if (action == "IN")
                    {
                        string query = @"INSERT INTO tbl_Attendance (UserCode, ActivityDate, PunchInTime, StartLatitude, StartLongitude, AttendanceStatus) 
                                         VALUES (@UserCode, CAST(GETDATE() AS DATE), GETDATE(), @Lat, @Lon, 'Present')";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@UserCode", userId);
                            cmd.Parameters.AddWithValue("@Lat", string.IsNullOrEmpty(lat) ? (object)DBNull.Value : Convert.ToDecimal(lat));
                            cmd.Parameters.AddWithValue("@Lon", string.IsNullOrEmpty(lon) ? (object)DBNull.Value : Convert.ToDecimal(lon));
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else if (action == "OUT")
                    {
                        string query = @"UPDATE tbl_Attendance 
                                         SET PunchOutTime = GETDATE(), EndLatitude = @Lat, EndLongitude = @Lon 
                                         WHERE UserCode = @UserCode AND ActivityDate = CAST(GETDATE() AS DATE)";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@UserCode", userId);
                            cmd.Parameters.AddWithValue("@Lat", string.IsNullOrEmpty(lat) ? (object)DBNull.Value : Convert.ToDecimal(lat));
                            cmd.Parameters.AddWithValue("@Lon", string.IsNullOrEmpty(lon) ? (object)DBNull.Value : Convert.ToDecimal(lon));
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                CheckTodayStatus();
                LoadAttendanceHistory();
            }
            catch (Exception ex)
            {
                lblError.Text = "An error occurred capturing attendance: " + ex.Message;
            }
        }

        private void LoadAttendanceHistory()
        {
            try
            {
                string userId = HttpContext.Current.Session["USERID"].ToString();
                string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    // Added 'Id' to the SELECT statement
                    string query = @"SELECT Id, ActivityDate, PunchInTime, PunchOutTime, AttendanceStatus 
                                     FROM tbl_Attendance 
                                     WHERE UserCode = @UserCode 
                                     ORDER BY ActivityDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserCode", userId);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        gvAttendanceHistory.DataSource = dt;
                        gvAttendanceHistory.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                lblError.Text = "Error loading history: " + ex.Message;
            }
        }

        // NEW: WebMethod to fetch coordinates for the Map Modal
        [System.Web.Services.WebMethod(EnableSession = true)]
        public static string GetAttendanceDetails(int id)
        {
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT ActivityDate, PunchInTime, PunchOutTime, StartLatitude, StartLongitude, EndLatitude, EndLongitude FROM tbl_Attendance WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            var details = new
                            {
                                Date = Convert.ToDateTime(rdr["ActivityDate"]).ToString("dd-MMM-yyyy"),
                                InTime = rdr["PunchInTime"] != DBNull.Value ? Convert.ToDateTime(rdr["PunchInTime"]).ToString("hh:mm tt") : "-",
                                OutTime = rdr["PunchOutTime"] != DBNull.Value ? Convert.ToDateTime(rdr["PunchOutTime"]).ToString("hh:mm tt") : "-",
                                InLat = rdr["StartLatitude"] != DBNull.Value ? rdr["StartLatitude"].ToString() : "",
                                InLon = rdr["StartLongitude"] != DBNull.Value ? rdr["StartLongitude"].ToString() : "",
                                OutLat = rdr["EndLatitude"] != DBNull.Value ? rdr["EndLatitude"].ToString() : "",
                                OutLon = rdr["EndLongitude"] != DBNull.Value ? rdr["EndLongitude"].ToString() : ""
                            };

                            JavaScriptSerializer js = new JavaScriptSerializer();
                            return js.Serialize(details);
                        }
                    }
                }
            }
            return "{}";
        }
    }
}