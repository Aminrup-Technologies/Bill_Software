using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm4 : System.Web.UI.Page
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
                Binddata();
            }
        }

        private void Binddata()
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                // Full-Stack Data Segregation
                string query = "SELECT ID, State_Name FROM tbl_State WHERE CompanyID = @CompanyID AND DeleteMode = 0 ORDER BY State_Name ASC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    conn.Open();
                    DataList1.DataSource = cmd.ExecuteReader();
                    DataList1.DataBind();
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string stateName = txtStateName.Text.Trim();
            int stateId = Convert.ToInt32(hfStateID.Value);

            if (string.IsNullOrEmpty(stateName)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();

                    // Duplicacy Checkpoint
                    string dupCheck = "SELECT COUNT(1) FROM tbl_State WHERE State_Name = @StateName AND CompanyID = @CompanyID AND DeleteMode = 0 AND ID <> @CurrentID";
                    using (SqlCommand cmdDup = new SqlCommand(dupCheck, conn))
                    {
                        cmdDup.Parameters.AddWithValue("@StateName", stateName);
                        cmdDup.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        cmdDup.Parameters.AddWithValue("@CurrentID", stateId);

                        if (Convert.ToInt32(cmdDup.ExecuteScalar()) > 0)
                        {
                            PanelError.Visible = true;
                            PanelOK.Visible = false;
                            lblErrorMsg.Text = $"Duplicate Entry: State '{stateName}' already exists.";
                            return;
                        }
                    }

                    // Insert or Update
                    string query = stateId == 0
                        ? "INSERT INTO tbl_State (State_Name, CompanyID, ViewMode, DeleteMode) VALUES (@StateName, @CompanyID, 1, 0)"
                        : "UPDATE tbl_State SET State_Name = @StateName WHERE ID = @ID AND CompanyID = @CompanyID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StateName", stateName);
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        if (stateId > 0) cmd.Parameters.AddWithValue("@ID", stateId);

                        cmd.ExecuteNonQuery();
                    }
                }

                string action = stateId == 0 ? "Added" : "Updated";
                InsertSystemNotification($"State Master {action}", $"State '{stateName}' was successfully {action.ToLower()}.", "Success", Session["USERID"]?.ToString());

                PanelOK.Visible = true;
                PanelError.Visible = false;
                lblOk.Text = $"Data {action} Successfully!";
                ResetForm();
                Binddata();
            }
            catch (Exception ex)
            {
                PanelError.Visible = true; PanelOK.Visible = false;
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
                    using (SqlCommand cmd = new SqlCommand("SELECT State_Name FROM tbl_State WHERE ID = @ID AND CompanyID = @CompanyID", conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        conn.Open();
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                txtStateName.Text = rdr["State_Name"].ToString();
                                hfStateID.Value = id.ToString();
                                btnSave.Text = "Update";
                                btnCancel.Visible = true;
                                PanelOK.Visible = false; PanelError.Visible = false;
                            }
                        }
                    }
                }
            }
            else if (e.CommandName == "DeleteRow")
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE tbl_State SET DeleteMode = 1, ViewMode = 0 WHERE ID = @ID AND CompanyID = @CompanyID", conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        conn.Open();
                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            InsertSystemNotification("State Removed", $"A state record (ID: {id}) was deleted.", "Info", Session["USERID"]?.ToString());
                            PanelOK.Visible = true; PanelError.Visible = false;
                            lblOk.Text = "Data Deleted Successfully!";
                        }
                    }
                }
                ResetForm();
                Binddata();
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ResetForm();
            PanelOK.Visible = false; PanelError.Visible = false;
        }

        private void ResetForm()
        {
            txtStateName.Text = ""; hfStateID.Value = "0"; btnSave.Text = "Save"; btnCancel.Visible = false;
        }

        private void InsertSystemNotification(string title, string message, string severity, string userId)
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                string query = @"INSERT INTO tbl_SystemNotification (Title, Message, Severity, StartDate, EndDate, CreatedBy, CompanyID) 
                                 VALUES (@Title, @Message, @Severity, GETDATE(), DATEADD(day, 7, GETDATE()), @CreatedBy, @CompanyID)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Message", message);
                    cmd.Parameters.AddWithValue("@Severity", severity);
                    cmd.Parameters.AddWithValue("@CreatedBy", string.IsNullOrEmpty(userId) ? (object)DBNull.Value : userId);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}