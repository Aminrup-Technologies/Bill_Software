using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Services;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm15 : System.Web.UI.Page
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
                BindStates();
                BindCities();
                // BindIndustry(); // Ensure this is bound if you have a method for it
                findcompanyId(); // Call the corrected ID generator
            }
        }

        // ==========================================
        // 1. MASTER DATA BINDING
        // ==========================================
        private void BindStates()
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                string query = "SELECT State_Name FROM tbl_State WHERE CompanyID = @CompanyID AND DeleteMode = 0 ORDER BY State_Name ASC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    conn.Open();
                    cmbState.DataSource = cmd.ExecuteReader();
                    cmbState.DataTextField = "State_Name";
                    cmbState.DataValueField = "State_Name";
                    cmbState.DataBind();
                }
            }
            cmbState.Items.Insert(0, new ListItem("--Select--", ""));
        }

        private void BindCities()
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                string query = "SELECT City_Name FROM tbl_City WHERE CompanyID = @CompanyID AND DeleteMode = 0 ORDER BY City_Name ASC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    conn.Open();
                    ddlCity.DataSource = cmd.ExecuteReader();
                    ddlCity.DataTextField = "City_Name";
                    ddlCity.DataValueField = "City_Name";
                    ddlCity.DataBind();
                }
            }
            ddlCity.Items.Insert(0, new ListItem("--Select or Type New City--", ""));
        }

        // CORRECTED: Alphanumeric ID Generator
        private string findcompanyId()
        {
            string ComId = "AD01";
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                string query = "SELECT TOP 1 Client_Id FROM tbl_Client WHERE CompanyID = @CompanyID ORDER BY Id DESC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        string lastClientId = result.ToString();
                        if (lastClientId.Length > 2)
                        {
                            string numberPart = lastClientId.Substring(2);
                            int k;
                            if (int.TryParse(numberPart, out k))
                            {
                                k = k + 1;
                                ComId = "AD" + k.ToString();
                            }
                        }
                    }
                }
            }
            lbl_nxtclientid.Text = ComId;
            return ComId;
        }

        // TEXT SANITIZER (Forces Proper Noun Capitalization)
        private string SanitizeName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            input = input.Trim().ToLower();
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input);
        }

        // ==========================================
        // 2. MAIN SAVE LOGIC
        // ==========================================
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string sanitizedName = SanitizeName(txtvendorName.Text);

                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();

                    // Duplicate Client Name Check (Gatekeeper)
                    string checkQuery = "SELECT COUNT(1) FROM tbl_Client WHERE Client_Name = @ClientName AND CompanyID = @CompanyID";
                    using (SqlCommand cmdCheck = new SqlCommand(checkQuery, conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@ClientName", sanitizedName);
                        cmdCheck.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        if (Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0)
                        {
                            PanelError.Visible = true;
                            PanelOK.Visible = false;
                            lblErrorMsg.Text = $"A client named '{sanitizedName}' already exists.";
                            return;
                        }
                    }

                    string newClientId = findcompanyId(); // Grab fresh ID safely

                    // CORRECTED SCHEMA: Using exact table columns
                    string insertQuery = @"
                        INSERT INTO tbl_Client (
                            Client_Id, Client_Name, Industry, Address1, State, City, pin, 
                            Com_phone, Com_Fax, Com_web_site, Com_email, Service_tax_no, Pan_no, PlaceofSupply, 
                            CompanyID, CreatedBy, CreatedOn
                        ) VALUES (
                            @ClientId, @ClientName, @Industry, @Address, @State, @City, @Pin, 
                            @Phone, @Fax, @Website, @Email, @GST, @PAN, @POS, 
                            @CompanyID, @CreatedBy, GETDATE()
                        )";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@ClientId", newClientId);
                        cmd.Parameters.AddWithValue("@ClientName", sanitizedName); // Clean Name!
                        cmd.Parameters.AddWithValue("@Industry", cmbIndustry.SelectedValue);
                        cmd.Parameters.AddWithValue("@Address", txtAddress1.Text.Trim());
                        cmd.Parameters.AddWithValue("@State", cmbState.SelectedValue);
                        cmd.Parameters.AddWithValue("@City", ddlCity.SelectedValue); // Inline City
                        cmd.Parameters.AddWithValue("@Pin", txtPin.Text.Trim());
                        cmd.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@Fax", txtFax.Text.Trim());
                        cmd.Parameters.AddWithValue("@Website", txtWebsite.Text.Trim());
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@GST", txtservicetax_no.Text.Trim().ToUpper()); // Clean GST!
                        cmd.Parameters.AddWithValue("@PAN", txtpanno.Text.Trim().ToUpper()); // Clean PAN!
                        cmd.Parameters.AddWithValue("@POS", txtplaceofSupply.Text.Trim());

                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        cmd.Parameters.AddWithValue("@CreatedBy", Session["USERID"].ToString());

                        cmd.ExecuteNonQuery();
                    }
                }

                // Standardized Dashboard Notification Logging
                InsertSystemNotification(
                    "New Client Onboarded",
                    $"Client '{sanitizedName}' was successfully created.",
                    "Client Management",
                    "Success",
                    Session["USERID"].ToString()
                );

                PanelOK.Visible = true;
                PanelError.Visible = false;
                lblOk.Text = "Client profile created successfully!";
                btnReset_Click(null, null); // Clear the form
                findcompanyId(); // Refresh the ID for the next entry
            }
            catch (Exception ex)
            {
                PanelError.Visible = true;
                PanelOK.Visible = false;
                lblErrorMsg.Text = "An error occurred while saving: " + ex.Message;
            }
        }

        // ==========================================
        // 3. AJAX WEB METHOD & HELPERS
        // ==========================================
        [WebMethod]
        public static string AddNewCityInline(string cityName, string stateName)
        {
            if (HttpContext.Current.Session["USERID"] == null) return "ERROR: Session expired.";

            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string dupCheck = "SELECT COUNT(1) FROM tbl_City WHERE City_Name = @CityName AND State_Name = @StateName AND CompanyID = @CompanyID AND DeleteMode = 0";
                    using (SqlCommand cmdDup = new SqlCommand(dupCheck, conn))
                    {
                        cmdDup.Parameters.AddWithValue("@CityName", cityName.Trim());
                        cmdDup.Parameters.AddWithValue("@StateName", stateName);
                        cmdDup.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        if (Convert.ToInt32(cmdDup.ExecuteScalar()) > 0) return $"ERROR: '{cityName}' already exists in '{stateName}'.";
                    }

                    string insertQuery = "INSERT INTO tbl_City (City_Name, State_Name, CompanyID, ViewMode, DeleteMode) VALUES (@CityName, @StateName, @CompanyID, 1, 0); SELECT SCOPE_IDENTITY();";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@CityName", cityName.Trim());
                        cmd.Parameters.AddWithValue("@StateName", stateName);
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        return Convert.ToInt32(cmd.ExecuteScalar()).ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtvendorName.Text = ""; txtAddress1.Text = ""; txtPin.Text = "";
            txtPhone.Text = ""; txtFax.Text = ""; txtWebsite.Text = ""; txtEmail.Text = "";
            txtservicetax_no.Text = ""; txtpanno.Text = ""; txtplaceofSupply.Text = "";

            if (cmbState.Items.Count > 0) cmbState.SelectedIndex = 0;
            ddlCity.ClearSelection();
            if (cmbIndustry.Items.Count > 0) cmbIndustry.SelectedIndex = 0;
        }

        // CORRECTED SCHEMA: Notification Logger
        private void InsertSystemNotification(string title, string message, string module, string type, string userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    string query = @"INSERT INTO tbl_SystemNotification (CompanyID, Title, Message, Module, Type, UserId, CreatedOn) 
                                     VALUES (@CompanyID, @Title, @Message, @Module, @Type, @UserId, GETDATE())";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        cmd.Parameters.AddWithValue("@Title", title);
                        cmd.Parameters.AddWithValue("@Message", message);
                        cmd.Parameters.AddWithValue("@Module", module);
                        cmd.Parameters.AddWithValue("@Type", type);
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { /* Soft catch for audit logs */ }
        }
    }
}