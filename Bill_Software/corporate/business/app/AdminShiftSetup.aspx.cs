using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Bill_Software.corporate.business.app
{
    public partial class AdminShiftSetup : System.Web.UI.Page
    {
        // Getting the connection string just like in your attendance page
        string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadShiftData();
            }
        }

        // Method to insert a new shift into the database
        protected void btnSaveShift_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string query = @"INSERT INTO tbl_ShiftMaster 
                                     (ShiftName, StartTime, EndTime, GracePeriodLateInMins, GracePeriodEarlyOutMins, HalfDayWorkingHours, FullDayWorkingHours, IsActive) 
                                     VALUES 
                                     (@ShiftName, @StartTime, @EndTime, @GraceLate, @GraceEarly, @HalfDay, @FullDay, 1)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ShiftName", txtShiftName.Text.Trim());
                        cmd.Parameters.AddWithValue("@StartTime", txtStartTime.Text);
                        cmd.Parameters.AddWithValue("@EndTime", txtEndTime.Text);
                        cmd.Parameters.AddWithValue("@GraceLate", Convert.ToInt32(txtGraceLate.Text));
                        cmd.Parameters.AddWithValue("@GraceEarly", Convert.ToInt32(txtGraceEarly.Text));
                        cmd.Parameters.AddWithValue("@HalfDay", Convert.ToDecimal(txtHalfDayHours.Text));
                        cmd.Parameters.AddWithValue("@FullDay", Convert.ToDecimal(txtFullDayHours.Text));

                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }

                // Show success message and clear form
                lblMessage.Text = "✅ Shift created successfully!";
                lblMessage.ForeColor = System.Drawing.Color.Green;
                ClearForm();
                LoadShiftData(); // Refresh the grid
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Error saving shift: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        // Method to fetch and display existing shifts in the GridView
        private void LoadShiftData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    // Formatting the TIME columns in SQL so they display nicely (e.g., 09:30 AM)
                    string query = @"SELECT ShiftID, ShiftName, 
                                     CONVERT(varchar(15), StartTime, 100) AS StartTime, 
                                     CONVERT(varchar(15), EndTime, 100) AS EndTime, 
                                     GracePeriodLateInMins, HalfDayWorkingHours, FullDayWorkingHours 
                                     FROM tbl_ShiftMaster 
                                     WHERE IsActive = 1 
                                     ORDER BY ShiftID ASC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
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
                lblMessage.Text = "❌ Error loading data: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        // Helper method to reset the form inputs
        private void ClearForm()
        {
            txtShiftName.Text = "";
            txtStartTime.Text = "";
            txtEndTime.Text = "";
            txtGraceLate.Text = "15";
            txtGraceEarly.Text = "15";
            txtHalfDayHours.Text = "4.0";
            txtFullDayHours.Text = "8.0";
        }
    }
}