using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm61 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }

            if (!IsPostBack)
            {
                DbCL.FillCombo(cmbState, "select State_Name from tbl_State order by State_Name");
                DbCL.FillCombo(cmbcity, "select City_Name from tbl_City order by City_Name");

                BindClientCombo();

                // SMART INTER-LINKING: Auto-select if arriving from View_client.aspx
                if (Request.QueryString["Client_Id"] != null)
                {
                    string passedClientId = Request.QueryString["Client_Id"].ToString();
                    if (cmbvendor.Items.FindByValue(passedClientId) != null)
                    {
                        cmbvendor.SelectedValue = passedClientId;
                        LoadClientContext();
                    }
                }
            }
        }

        private void BindClientCombo()
        {
            DbCL.Sqlconnection(); DbCL.ConnectDb();
            // SECURED: Enforce Tenant Segregation
            string cmdText = "SELECT Client_Id, Client_Name FROM tbl_Client WHERE CompanyID = @CompanyID ORDER BY Client_Name";
            using (SqlCommand cmd = new SqlCommand(cmdText, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    cmbvendor.DataSource = dt;
                    cmbvendor.DataTextField = "Client_Name";
                    cmbvendor.DataValueField = "Client_Id"; // Bind Secure ID
                    cmbvendor.DataBind();
                }
            }
            cmbvendor.Items.Insert(0, new ListItem("-- Select Client --", "0"));
            DbCL.Conn.Close();
        }

        protected void cmbvendor_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadClientContext();
        }

        // Drives the Contextual Summary & Grid
        private void LoadClientContext()
        {
            if (cmbvendor.SelectedValue == "0")
            {
                pnlClientSummary.Visible = false;
                gvFactories.DataSource = null;
                gvFactories.DataBind();
                return;
            }

            DbCL.Sqlconnection(); DbCL.ConnectDb();

            // Fetch Client Basic Details securely
            string cmdText = "SELECT Client_Name, Com_phone, Com_email, Address1, City, State FROM tbl_Client WHERE Client_Id = @ClientId AND CompanyID = @CompanyID";
            using (SqlCommand cmd = new SqlCommand(cmdText, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@ClientId", cmbvendor.SelectedValue);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        litClientName.Text = dr["Client_Name"].ToString();
                        litClientId.Text = cmbvendor.SelectedValue;
                        litPhone.Text = string.IsNullOrEmpty(dr["Com_phone"].ToString()) ? "N/A" : dr["Com_phone"].ToString();
                        litEmail.Text = string.IsNullOrEmpty(dr["Com_email"].ToString()) ? "N/A" : dr["Com_email"].ToString();
                        litAddress.Text = $"{dr["Address1"]}, {dr["City"]}, {dr["State"]}";
                        pnlClientSummary.Visible = true;
                    }
                }
            }
            DbCL.Conn.Close();

            BindFactoryGrid();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (cmbvendor.SelectedValue == "0" || ddlfactoryName.SelectedValue == "0") return;

            string userId = Session["USERID"] != null ? Session["USERID"].ToString() : "System";
            string selectedClientId = cmbvendor.SelectedValue;

            DbCL.Sqlconnection(); DbCL.ConnectDb();
            // SECURED & AUDITABLE INSERT
            string cmdstring = @"INSERT INTO tbl_Factory 
                (Client_id, Factory_name, Address1, Address2, city, State, pin, CompanyID, CreatedBy, CreatedOn) 
                VALUES (@Client_id, @Factory_name, @Address1, @Address2, @city, @State, @pin, @CompanyID, @CreatedBy, GETDATE())";

            using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@Client_id", selectedClientId);
                cmd.Parameters.AddWithValue("@Factory_name", ddlfactoryName.Text);
                cmd.Parameters.AddWithValue("@Address1", txtAddress1.Text.Trim());
                cmd.Parameters.AddWithValue("@Address2", txtaddress2.Text.Trim());
                cmd.Parameters.AddWithValue("@city", cmbcity.Text);
                cmd.Parameters.AddWithValue("@State", cmbState.Text);
                cmd.Parameters.AddWithValue("@pin", txtpin.Text.Trim());
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                cmd.Parameters.AddWithValue("@CreatedBy", userId);
                cmd.ExecuteNonQuery();
            }

            // PROACTIVE NOTIFICATION LOGGING
            try
            {
                string notifMsg = $"Factory Unit '{ddlfactoryName.Text}' added for Client ID: {selectedClientId}.";
                string notifQuery = @"INSERT INTO tbl_SystemNotification (CompanyID, Title, Message, Module, Type, UserId, CreatedOn) 
                                      VALUES (@CompanyID, 'Factory Added', @Message, 'Client Management', 'Info', @UserId, GETDATE())";
                SqlParameter[] notifParam = {
                    new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID),
                    new SqlParameter("@Message", notifMsg),
                    new SqlParameter("@UserId", userId)
                };
                DbCL.SPExecDB(notifQuery, notifParam);
            }
            catch { /* Soft catch */ }

            DbCL.Conn.Close();

            PanelOK.Visible = true;
            lblOk.Text = "Factory Unit Saved Successfully!";

            // Clean up form
            txtAddress1.Text = ""; txtaddress2.Text = ""; txtpin.Text = "";
            ddlfactoryName.SelectedIndex = 0; cmbcity.SelectedIndex = 0; cmbState.SelectedIndex = 0;

            BindFactoryGrid();
        }

        private void BindFactoryGrid()
        {
            // SECURED SELECTION + AUDIT DISPLAY
            string query = "SELECT Id, Factory_name, Address1, city, State, pin, CreatedBy, CreatedOn FROM tbl_Factory WHERE CompanyID = @CompanyID AND Client_id = @ClientId ORDER BY Id DESC";
            SqlParameter[] pram = {
                new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID),
                new SqlParameter("@ClientId", cmbvendor.SelectedValue)
            };
            DataTable dt = DbCL.SPreturn_dt(query, pram);
            gvFactories.DataSource = dt;
            gvFactories.DataBind();
        }

        protected void gvFactories_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteUnit")
            {
                string id = e.CommandArgument.ToString();
                string userId = Session["USERID"] != null ? Session["USERID"].ToString() : "System";

                // SECURED DELETION
                string delQuery = "DELETE FROM tbl_Factory WHERE Id = @Id AND CompanyID = @CompanyID";
                SqlParameter[] param = { new SqlParameter("@Id", id), new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID) };
                DbCL.SPExecDB(delQuery, param);

                // AUDIT LOG DELETION
                try
                {
                    string notifQuery = @"INSERT INTO tbl_SystemNotification (CompanyID, Title, Message, Module, Type, UserId, CreatedOn) 
                                          VALUES (@CompanyID, 'Factory Deleted', @Message, 'Client Management', 'Warning', @UserId, GETDATE())";
                    SqlParameter[] notifParam = {
                        new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID),
                        new SqlParameter("@Message", $"A factory unit was deleted for Client {cmbvendor.SelectedValue}."),
                        new SqlParameter("@UserId", userId)
                    };
                    DbCL.SPExecDB(notifQuery, notifParam);
                }
                catch { }

                PanelOK.Visible = true;
                lblOk.Text = "Factory Unit Removed.";
                BindFactoryGrid();
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("View_client.aspx");
        }
    }
}