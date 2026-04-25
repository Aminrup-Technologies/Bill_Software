using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm17 : System.Web.UI.Page
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
                string Client_Id = Request.QueryString["Client_Id"];
                if (string.IsNullOrEmpty(Client_Id)) Response.Redirect("~/corporate/business/app/View_client.aspx");

                lblvendor_id.Text = Client_Id;

                // Load Dropdowns
                DbCL.FillCombo(cmbState, "select State_Name from tbl_State order by State_Name");
                DbCL.FillCombo(cmbcity, "select City_Name from tbl_City order by City_Name");
                DbCL.FillCombo(ddlRegState, "select State_Name from tbl_State order by State_Name");
                DbCL.FillCombo(ddlRegCity, "select City_Name from tbl_City order by City_Name");
                DbCL.FillCombo(cmbIndustry, "select IndustryName from tbl_Industry");

                Binddate();
            }
        }

        private void Binddate()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            // SECURED: Enforce CompanyID
            string cmdstring = "SELECT * FROM tbl_Client WHERE Client_Id = @ClientId AND CompanyID = @CompanyID";
            using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@ClientId", lblvendor_id.Text);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                using (SqlDataReader re = cmd.ExecuteReader())
                {
                    if (re.Read())
                    {
                        txtvendorName.Text = re["Client_Name"].ToString();
                        txtAddress1.Text = re["Address1"].ToString();
                        txtPhone.Text = re["Com_phone"].ToString();

                        SetDropdownSafe(cmbcity, re["City"].ToString());
                        SetDropdownSafe(cmbState, re["State"].ToString());
                        SetDropdownSafe(cmbIndustry, re["Industry"].ToString());

                        txtPin.Text = re["pin"].ToString();
                        txtWebsite.Text = re["Com_web_site"].ToString();
                        txtEmail.Text = re["Com_email"].ToString();
                        txtFax.Text = re["Com_Fax"].ToString();
                        txtservicetax_no.Text = re["Service_tax_no"].ToString();
                        txtpanno.Text = re["Pan_no"].ToString();
                    }
                }
            }
            DbCL.Conn.Close();
            bindregaddress();
        }

        private void bindregaddress()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            // SECURED: Enforce CompanyID
            string cmdstring = "SELECT Address, State, City, Phno, pin FROM tbl_ClientRegAddress WHERE Client_Id = @ClientId AND CompanyID = @CompanyID";
            using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@ClientId", lblvendor_id.Text);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                using (SqlDataReader re = cmd.ExecuteReader())
                {
                    if (re.Read())
                    {
                        txtRegAddress.Text = re["Address"].ToString();
                        SetDropdownSafe(ddlRegState, re["State"].ToString());
                        SetDropdownSafe(ddlRegCity, re["City"].ToString());
                        txtRegPin.Text = re["pin"].ToString();
                        txtRegPhno.Text = re["Phno"].ToString();
                    }
                }
            }
            DbCL.Conn.Close();
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            string userId = Session["USERID"] != null ? Session["USERID"].ToString() : "System";

            string cmdstring = @"UPDATE tbl_Client SET 
                     Client_Name=@Client_Name, Address1=@Address1, City=@City, pin=@pin, State=@State, 
                     Com_web_site=@Com_web_site, Com_email=@Com_email, Com_phone=@Com_phone, Com_Fax=@Com_Fax, 
                     Service_tax_no=@Service_tax_no, Pan_no=@Pan_no, Industry=@Industry, 
                     UpdatedBy=@UpdatedBy, UpdatedOn=GETDATE() 
                     WHERE Client_Id=@ClientId AND CompanyID=@CompanyID";

            using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@Client_Name", txtvendorName.Text.Trim());
                cmd.Parameters.AddWithValue("@Address1", txtAddress1.Text.Trim());
                cmd.Parameters.AddWithValue("@City", cmbcity.Text);
                cmd.Parameters.AddWithValue("@pin", txtPin.Text.Trim());
                cmd.Parameters.AddWithValue("@State", cmbState.Text);
                cmd.Parameters.AddWithValue("@Com_web_site", txtWebsite.Text.Trim());
                cmd.Parameters.AddWithValue("@Com_email", txtEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@Com_phone", txtPhone.Text.Trim());
                cmd.Parameters.AddWithValue("@Com_Fax", txtFax.Text.Trim());
                cmd.Parameters.AddWithValue("@Service_tax_no", txtservicetax_no.Text.Trim());
                cmd.Parameters.AddWithValue("@Pan_no", txtpanno.Text.Trim());
                cmd.Parameters.AddWithValue("@Industry", cmbIndustry.Text);
                cmd.Parameters.AddWithValue("@ClientId", lblvendor_id.Text);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                cmd.Parameters.AddWithValue("@UpdatedBy", userId);
                cmd.ExecuteNonQuery();
            }
            DbCL.Conn.Close();

            updateregaddress();

            // ========================================================================
            // FIXED PROACTIVE NOTIFICATION: Correct Columns + Try/Catch Safety Net
            // ========================================================================
            try
            {
                string notifMsg = $"Client '{txtvendorName.Text.Trim()}' (ID: {lblvendor_id.Text}) profile was updated.";

                // Using standard schema columns to ensure the auto-scrolling dashboard maps properly
                string notifQuery = @"INSERT INTO tbl_SystemNotification 
                              (CompanyID, Title, Message, Module, Type, UserId, CreatedOn) 
                              VALUES (@CompanyID, @Title, @Message, @Module, @Type, @UserId, GETDATE())";

                SqlParameter[] notifParam = {
                    new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID),
                    new SqlParameter("@Title", "Client Updated"),
                    new SqlParameter("@Message", notifMsg),
                    new SqlParameter("@Module", "Client Management"),
                    new SqlParameter("@Type", "Info"),
                    new SqlParameter("@UserId", Session["USERID"] != null ? Session["USERID"].ToString() : "System")
                };

                DbCL.SPExecDB(notifQuery, notifParam);
            }
            catch (Exception ex)
            {
                // Soft-catch: If the notification table schema varies, it will fail silently 
                // without crashing the actual Client Update transaction above.
                System.Diagnostics.Debug.WriteLine("Notification Logging Failed: " + ex.Message);
            }

            PanelOK.Visible = true;
            lblOk.Text = "Client Details Updated Successfully.";
        }

        private void updateregaddress()
        {
            string userId = Session["USERID"] != null ? Session["USERID"].ToString() : "System";

            string query = "UPDATE tbl_ClientRegAddress SET Address=@Address, State=@State, City=@City, Phno=@Phno, pin=@pin, UpdatedBy=@UpdatedBy, UpdatedOn=GETDATE() WHERE Client_Id=@ClientId AND CompanyID=@CompanyID";
            SqlParameter[] pram = {
                new SqlParameter("@Address", txtRegAddress.Text.Trim()),
                new SqlParameter("@State", ddlRegState.Text),
                new SqlParameter("@City", ddlRegCity.Text),
                new SqlParameter("@Phno", txtRegPhno.Text.Trim()),
                new SqlParameter("@pin", txtRegPin.Text.Trim()),
                new SqlParameter("@ClientId", lblvendor_id.Text),
                new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID),
                new SqlParameter("@UpdatedBy", userId) // <--- AUDIT TRAIL INJECTED
            };
            DbCL.SPExecDB(query, pram);
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/View_client.aspx");
        }

        private void SetDropdownSafe(DropDownList ddl, string val)
        {
            if (!string.IsNullOrEmpty(val) && ddl.Items.FindByValue(val) != null)
            {
                ddl.SelectedValue = val;
            }
        }
    }
}