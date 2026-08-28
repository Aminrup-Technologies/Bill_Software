using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Bill_Software.corporate.business.app
{
    public partial class AdminLeaveSetup : System.Web.UI.Page
    {
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
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string query = @"INSERT INTO tbl_LeaveMaster (LeaveName, IsPaid, MaxDaysPerYear, IsActive) 
                                     VALUES (@LeaveName, @IsPaid, @MaxDays, 1)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@LeaveName", txtLeaveName.Text.Trim());
                        cmd.Parameters.AddWithValue("@IsPaid", Convert.ToInt32(ddlIsPaid.SelectedValue));
                        cmd.Parameters.AddWithValue("@MaxDays", Convert.ToDecimal(txtMaxDays.Text));

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                lblMessage.Text = "✅ Leave Category created successfully!";
                lblMessage.ForeColor = System.Drawing.Color.Green;

                // Reset fields
                txtLeaveName.Text = "";
                txtMaxDays.Text = "12.0";
                ddlIsPaid.SelectedIndex = 0;

                LoadLeaveData(); // Refresh the grid
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Error saving leave category: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void LoadLeaveData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    string query = @"SELECT LeaveID, LeaveName, IsPaid, MaxDaysPerYear 
                                     FROM tbl_LeaveMaster 
                                     WHERE IsActive = 1 
                                     ORDER BY IsPaid DESC, LeaveName ASC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
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
                lblMessage.Text = "❌ Error loading data: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }
    }
}