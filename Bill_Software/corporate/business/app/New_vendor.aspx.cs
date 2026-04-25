using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm5 : System.Web.UI.Page
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
                DbCL.FillCombo(cmbState, "SELECT State_Name FROM tbl_State ORDER BY State_Name");
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string companyID_Str = findcompanyId();
                int currentTenantId = CompanyContext.CurrentCompanyID;
                string userId = Session["USERID"] != null ? Session["USERID"].ToString() : "System";

                string cmdstring = @"INSERT INTO tbl_Vendor
                    (Vendor_Id, Vendor_Name, Address1, Address2, City, pin, State, 
                     Com_web_site, Com_email, Com_phone, Com_Fax, Rep_Name, Rep_Desig, 
                     Rep_phone, Rep_email, Service_tax_No, Pan_No, Vat_No, PrincipleVndrCode, 
                     BankAccNo, BankIfscCode, AccountName, CompanyID, CreatedBy, CreatedOn)
                    VALUES
                    (@Vendor_Id, @Vendor_Name, @Address1, @Address2, @City, @pin, @State, 
                     @Com_web_site, @Com_email, @Com_phone, @Com_Fax, @Rep_Name, @Rep_Desig, 
                     @Rep_phone, @Rep_email, @Service_tax_No, @Pan_No, @Vat_No, @PrincipleVndrCode, 
                     @BankAccNo, @BankIfscCode, @AccountName, @CompanyID, @CreatedBy, GETDATE())";

                DbCL.Sqlconnection();
                DbCL.ConnectDb();
                using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
                {
                    cmd.Parameters.AddWithValue("@Vendor_Id", companyID_Str);
                    cmd.Parameters.AddWithValue("@Vendor_Name", txtvendorName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Address1", txtAddress1.Text.Trim());
                    cmd.Parameters.AddWithValue("@Address2", txtAddress2.Text.Trim());
                    cmd.Parameters.AddWithValue("@City", txtCity.Text.Trim());
                    cmd.Parameters.AddWithValue("@pin", txtPin.Text.Trim());
                    cmd.Parameters.AddWithValue("@State", cmbState.Text);
                    cmd.Parameters.AddWithValue("@Com_web_site", txtWebsite.Text.Trim());
                    cmd.Parameters.AddWithValue("@Com_email", txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@Com_phone", txtPhone.Text.Trim());
                    cmd.Parameters.AddWithValue("@Com_Fax", txtFax.Text.Trim());
                    cmd.Parameters.AddWithValue("@Rep_Name", txtRepresentativeName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Rep_Desig", txtRepresantativeDesig.Text.Trim());
                    cmd.Parameters.AddWithValue("@Rep_phone", txtRepresentativePhone.Text.Trim());
                    cmd.Parameters.AddWithValue("@Rep_email", txtRepresentativeEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@Service_tax_No", txtservicetaxNo.Text.Trim());
                    cmd.Parameters.AddWithValue("@Pan_No", txtpanNo.Text.Trim());
                    cmd.Parameters.AddWithValue("@Vat_No", string.Empty);
                    cmd.Parameters.AddWithValue("@PrincipleVndrCode", txt_pvc.Text.Trim());
                    cmd.Parameters.AddWithValue("@BankAccNo", txt_vndr_bankacc.Text.Trim());
                    cmd.Parameters.AddWithValue("@BankIfscCode", txt_ifsc.Text.Trim());
                    cmd.Parameters.AddWithValue("@AccountName", txt_accholdername.Text.Trim());

                    // FULL STACK TENANT SEGREGATION & AUDIT
                    cmd.Parameters.AddWithValue("@CompanyID", currentTenantId);
                    cmd.Parameters.AddWithValue("@CreatedBy", userId);

                    cmd.ExecuteNonQuery();
                }

                InsertCity();

                // ---- PROACTIVE NOTIFICATION LOGGING ----
                try
                {
                    string notifMsg = $"New Vendor '{txtvendorName.Text.Trim()}' (ID: {companyID_Str}) was created successfully.";
                    string notifQuery = @"INSERT INTO tbl_SystemNotification 
                                          (CompanyID, Title, Message, Module, Type, UserId, CreatedOn) 
                                          VALUES (@CompanyID, 'Vendor Created', @Message, 'Vendor Management', 'Success', @UserId, GETDATE())";
                    SqlParameter[] notifParam = {
                        new SqlParameter("@CompanyID", currentTenantId),
                        new SqlParameter("@Message", notifMsg),
                        new SqlParameter("@UserId", userId)
                    };
                    DbCL.SPExecDB(notifQuery, notifParam);
                }
                catch { /* Soft catch: don't crash main transaction if logging fails */ }

                DbCL.Conn.Close();

                PanelOK.Visible = true;
                lblOk.Text = "Vendor Data Saved Successfully.";
                btnSave.Visible = false;
            }
            catch (Exception ex)
            {
                // Handle Exceptions gracefully
            }
        }

        private void InsertCity()
        {
            // SECURED: Ensure parameterized checks to avoid Injection
            string checkQuery = "SELECT COUNT(*) FROM tbl_City WHERE City_Name = @CityName AND State_Name = @StateName";
            using (SqlCommand checkCmd = new SqlCommand(checkQuery, DbCL.Conn))
            {
                checkCmd.Parameters.AddWithValue("@CityName", txtCity.Text.Trim());
                checkCmd.Parameters.AddWithValue("@StateName", cmbState.Text);
                int count = (int)checkCmd.ExecuteScalar();

                if (count == 0)
                {
                    string insertQuery = "INSERT INTO tbl_City (City_Name, State_Name) VALUES (@CityName, @StateName)";
                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, DbCL.Conn))
                    {
                        insertCmd.Parameters.AddWithValue("@CityName", txtCity.Text.Trim());
                        insertCmd.Parameters.AddWithValue("@StateName", cmbState.Text);
                        insertCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private string findcompanyId()
        {
            string comId = "AA01";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            // SECURED: Scope MAX lookup to current CompanyID
            string cmdString = "SELECT TOP 1 Vendor_Id FROM tbl_Vendor WHERE CompanyID = @CompanyID ORDER BY Id DESC";
            using (SqlCommand com = new SqlCommand(cmdString, DbCL.Conn))
            {
                com.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                using (SqlDataReader dr = com.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        string vendorId = dr["Vendor_Id"].ToString();
                        if (vendorId.Length > 2)
                        {
                            string numericPart = vendorId.Substring(2); // Extract after 'AA'
                            int k = 0;
                            if (int.TryParse(numericPart, out k))
                            {
                                k++;
                                comId = "AA" + k.ToString("D2"); // Format back to AA02
                            }
                        }
                    }
                }
            }
            DbCL.Conn.Close();
            return comId;
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/New_vendor.aspx");
        }
    }
}