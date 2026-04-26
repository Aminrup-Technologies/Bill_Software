using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm3 : System.Web.UI.Page
    {
        private string ConnString => ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            if (!IsPostBack)
            {
                BindStates(); // Load dropdown first
                Binddata();
            }
        }

        private void Binddata()
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                // Full-Stack CompanyContext Segregation
                string query = "SELECT ID, City_Name, State_Name FROM tbl_City WHERE CompanyID = @CompanyID AND DeleteMode = 0 ORDER BY City_Name ASC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    conn.Open();
                    DataList1.DataSource = cmd.ExecuteReader();
                    DataList1.DataBind();
                }
            }
        }

        private void BindStates()
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                string query = "SELECT State_Name FROM tbl_State WHERE CompanyID = @CompanyID AND DeleteMode = 0 ORDER BY State_Name ASC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    conn.Open();
                    ddlStateName.DataSource = cmd.ExecuteReader();
                    // We use State_Name for both Display and Value to perfectly match the tbl_City schema
                    ddlStateName.DataTextField = "State_Name";
                    ddlStateName.DataValueField = "State_Name";
                    ddlStateName.DataBind();
                }
            }
            ddlStateName.Items.Insert(0, new ListItem("--Select State--", ""));
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string cityName = txtCityName.Text.Trim();
            string stateName = ddlStateName.SelectedValue;
            int cityId = Convert.ToInt32(hfCityID.Value); // 0 means new insert

            if (string.IsNullOrEmpty(cityName)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();

                    // ==========================================
                    // 1. DUPLICACY CHECKPOINT
                    // ==========================================
                    string dupCheckQuery = @"SELECT COUNT(1) FROM tbl_City 
                                             WHERE City_Name = @CityName AND State_Name = @StateName 
                                             AND CompanyID = @CompanyID AND DeleteMode = 0 AND ID <> @CurrentID";

                    using (SqlCommand cmdDup = new SqlCommand(dupCheckQuery, conn))
                    {
                        cmdDup.Parameters.AddWithValue("@CityName", cityName);
                        cmdDup.Parameters.AddWithValue("@StateName", stateName);
                        cmdDup.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        cmdDup.Parameters.AddWithValue("@CurrentID", cityId);

                        int exists = Convert.ToInt32(cmdDup.ExecuteScalar());
                        if (exists > 0)
                        {
                            PanelError.Visible = true;
                            PanelOK.Visible = false;
                            lblErrorMsg.Text = $"Duplicate Entry: '{cityName}' in '{stateName}' already exists.";
                            return; // Stop execution
                        }
                    }

                    // ==========================================
                    // 2. INSERT OR UPDATE LOGIC
                    // ==========================================
                    string query = "";
                    if (cityId == 0)
                    {
                        query = "INSERT INTO tbl_City (City_Name, State_Name, CompanyID, ViewMode, DeleteMode) VALUES (@CityName, @StateName, @CompanyID, 1, 0)";
                    }
                    else
                    {
                        query = "UPDATE tbl_City SET City_Name = @CityName, State_Name = @StateName WHERE ID = @ID AND CompanyID = @CompanyID";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CityName", cityName);
                        cmd.Parameters.AddWithValue("@StateName", stateName);
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        if (cityId > 0) cmd.Parameters.AddWithValue("@ID", cityId);

                        cmd.ExecuteNonQuery();
                    }
                }

                // ==========================================
                // 3. PROACTIVE NOTIFICATION LOGGING (FIXED)
                // ==========================================
                string action = cityId == 0 ? "Added" : "Updated";

                // ADDED "Success" AS THE SEVERITY
                InsertSystemNotification(
                    $"City Master {action}",
                    $"Location '{cityName}' was successfully {action.ToLower()} in the master list.",
                    "Success",
                    Session["USERID"].ToString()
                );

                PanelOK.Visible = true;
                PanelError.Visible = false;
                lblOk.Text = $"Data {action} Successfully!";
                ResetForm();
                Binddata();
            }
            catch (Exception ex)
            {
                PanelError.Visible = true;
                PanelOK.Visible = false;
                lblErrorMsg.Text = "Error: " + ex.Message;
            }
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "EditRow")
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    string query = "SELECT City_Name, State_Name FROM tbl_City WHERE ID = @ID AND CompanyID = @CompanyID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

                        conn.Open();
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                txtCityName.Text = rdr["City_Name"].ToString();
                                string savedState = rdr["State_Name"].ToString();
                                ddlStateName.ClearSelection();
                                ListItem item = ddlStateName.Items.FindByValue(savedState);
                                if (item != null)
                                {
                                    item.Selected = true;
                                }
                                hfCityID.Value = id.ToString();
                                btnSave.Text = "Update";
                                btnCancel.Visible = true;
                                PanelOK.Visible = false;
                                PanelError.Visible = false;
                            }
                        }
                    }
                }
            }
            else if (e.CommandName == "DeleteRow")
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(ConnString))
                    {
                        string query = "UPDATE tbl_City SET DeleteMode = 1, ViewMode = 0 WHERE ID = @ID AND CompanyID = @CompanyID";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@ID", id);
                            cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

                            conn.Open();
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                // PROACTIVE NOTIFICATION LOGGING (FIXED)
                                // ADDED "Info" AS THE SEVERITY
                                InsertSystemNotification(
                                    "City Removed",
                                    $"A location record (ID: {id}) was removed from the master list.",
                                    "Info",
                                    Session["USERID"].ToString()
                                );

                                PanelOK.Visible = true;
                                PanelError.Visible = false;
                                lblOk.Text = "Data Deleted Successfully!";
                            }
                        }
                    }
                    ResetForm();
                    Binddata();
                }
                catch (Exception ex)
                {
                    PanelError.Visible = true;
                    PanelOK.Visible = false;
                    lblErrorMsg.Text = "Error: " + ex.Message;
                }
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ResetForm();
            PanelOK.Visible = false;
            PanelError.Visible = false;
        }

        private void ResetForm()
        {
            txtCityName.Text = "";
            ddlStateName.SelectedIndex = 0;
            hfCityID.Value = "0";
            btnSave.Text = "Save";
            btnCancel.Visible = false;
        }

        // ==============================================================
        // Helper method for System Notifications (REVISED FOR ENDDATE)
        // ==============================================================
        private void InsertSystemNotification(string title, string message, string severity, string userId)
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                // Mapped StartDate to GETDATE() and EndDate to 7 days in the future
                // CreatedOn and IsActive will automatically use the SQL DEFAULT constraints
                string query = @"INSERT INTO tbl_SystemNotification 
                                (Title, Message, Severity, StartDate, EndDate, CreatedBy, CompanyID) 
                                 VALUES 
                                (@Title, @Message, @Severity, GETDATE(), DATEADD(day, 7, GETDATE()), @CreatedBy, @CompanyID)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Message", message);
                    cmd.Parameters.AddWithValue("@Severity", severity);

                    // Safely handle null userId just in case
                    cmd.Parameters.AddWithValue("@CreatedBy", string.IsNullOrEmpty(userId) ? (object)DBNull.Value : userId);

                    // Strict Multi-Tenant Data Segregation
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}