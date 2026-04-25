using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm59 : System.Web.UI.Page
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
                gvReps.DataSource = null;
                gvReps.DataBind();
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

            BindRepGrid();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (cmbvendor.SelectedValue == "0") return;

            string userId = Session["USERID"] != null ? Session["USERID"].ToString() : "System";
            string selectedClientId = cmbvendor.SelectedValue;

            DbCL.Sqlconnection(); DbCL.ConnectDb();
            // SECURED & AUDITABLE INSERT
            string cmdstring = @"INSERT INTO tbl_representative 
                (Copany_Id, Representative_name, Designation, Phone_no, Email, RepTitle, RepLastName, CompanyID, CreatedBy, CreatedOn) 
                VALUES (@ClientId, @Name, @Desig, @Phone, @Email, @Title, @LastName, @CompanyID, @CreatedBy, GETDATE())";

            using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@ClientId", selectedClientId);
                cmd.Parameters.AddWithValue("@Name", txtRepresentativeName.Text.Trim());
                cmd.Parameters.AddWithValue("@Desig", txtRepresantativeDesig.Text.Trim());
                cmd.Parameters.AddWithValue("@Phone", txtRepresentativePhone.Text.Trim());
                cmd.Parameters.AddWithValue("@Email", txtRepresentativeEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@Title", ddlRepTitle.Text);
                cmd.Parameters.AddWithValue("@LastName", txtLastName.Text.Trim());
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                cmd.Parameters.AddWithValue("@CreatedBy", userId);
                cmd.ExecuteNonQuery();
            }

            // PROACTIVE NOTIFICATION LOGGING (Soft Catch)
            try
            {
                string notifMsg = $"New Representative '{txtRepresentativeName.Text.Trim()}' added for Client ID: {selectedClientId}.";
                string notifQuery = @"INSERT INTO tbl_SystemNotification (CompanyID, Title, Message, Module, Type, UserId, CreatedOn) 
                                      VALUES (@CompanyID, 'Representative Added', @Message, 'Client Management', 'Info', @UserId, GETDATE())";
                SqlParameter[] notifParam = {
                    new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID),
                    new SqlParameter("@Message", notifMsg),
                    new SqlParameter("@UserId", userId)
                };
                DbCL.SPExecDB(notifQuery, notifParam);
            }
            catch { /* Ignore notification crash to protect transaction */ }

            DbCL.Conn.Close();

            PanelOK.Visible = true;
            lblOk.Text = "Representative Saved Successfully!";

            // Clean up form
            txtRepresantativeDesig.Text = ""; txtRepresentativeEmail.Text = "";
            txtRepresentativeName.Text = ""; txtRepresentativePhone.Text = ""; txtLastName.Text = "";

            BindRepGrid();
        }

        private void BindRepGrid()
        {
            // SECURED SELECTION + AUDIT DISPLAY
            string query = "SELECT ID, RepTitle, Representative_name, RepLastName, Designation, Phone_no, Email, CreatedBy, CreatedOn FROM tbl_representative WHERE CompanyID = @CompanyID AND Copany_Id = @ClientId ORDER BY ID DESC";
            SqlParameter[] pram = {
                new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID),
                new SqlParameter("@ClientId", cmbvendor.SelectedValue)
            };
            DataTable dt = DbCL.SPreturn_dt(query, pram);
            gvReps.DataSource = dt;
            gvReps.DataBind();
        }

        protected void gvReps_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteRep")
            {
                string id = e.CommandArgument.ToString();
                string userId = Session["USERID"] != null ? Session["USERID"].ToString() : "System";

                // SECURED DELETION
                string delQuery = "DELETE FROM tbl_representative WHERE ID = @Id AND CompanyID = @CompanyID";
                SqlParameter[] param = { new SqlParameter("@Id", id), new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID) };
                DbCL.SPExecDB(delQuery, param);

                // AUDIT LOG DELETION
                try
                {
                    string notifQuery = @"INSERT INTO tbl_SystemNotification (CompanyID, Title, Message, Module, Type, UserId, CreatedOn) 
                                          VALUES (@CompanyID, 'Representative Deleted', @Message, 'Client Management', 'Warning', @UserId, GETDATE())";
                    SqlParameter[] notifParam = {
                        new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID),
                        new SqlParameter("@Message", $"A representative was removed from Client {cmbvendor.SelectedValue}."),
                        new SqlParameter("@UserId", userId)
                    };
                    DbCL.SPExecDB(notifQuery, notifParam);
                }
                catch { }

                PanelOK.Visible = true;
                lblOk.Text = "Representative Removed.";
                BindRepGrid();
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("View_client.aspx");
        }
    }
}