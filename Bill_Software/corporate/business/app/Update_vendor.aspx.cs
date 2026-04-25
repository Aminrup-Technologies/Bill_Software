using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm12 : System.Web.UI.Page
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
                string Vendor_Id = Request.QueryString["Vendor_Id"];
                if (string.IsNullOrEmpty(Vendor_Id))
                {
                    Response.Redirect("~/corporate/business/app/View_vendor.aspx");
                }

                lblvendor_id.Text = Vendor_Id;
                DbCL.FillCombo(cmbState, "SELECT State_Name FROM tbl_State ORDER BY State_Name");
                DbCL.FillCombo(cmbcity, "SELECT City_Name FROM tbl_City ORDER BY City_Name");

                BindData();
            }
        }

        private void BindData()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            // SECURED: Full-Stack Tenant Segregation
            string cmdstring = "SELECT * FROM tbl_Vendor WHERE Vendor_Id = @VendorId AND CompanyID = @CompanyID";
            using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@VendorId", lblvendor_id.Text);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

                using (SqlDataReader re = cmd.ExecuteReader())
                {
                    if (re.Read())
                    {
                        txtvendorName.Text = re["Vendor_Name"].ToString();
                        txtAddress1.Text = re["Address1"].ToString();
                        txtAddress2.Text = re["Address2"].ToString();

                        SetDropdownSafe(cmbcity, re["City"].ToString());
                        SetDropdownSafe(cmbState, re["State"].ToString());

                        txtPin.Text = re["pin"].ToString();
                        txtWebsite.Text = re["Com_web_site"].ToString();
                        txtEmail.Text = re["Com_email"].ToString();
                        txtPhone.Text = re["Com_phone"].ToString();
                        txtFax.Text = re["Com_Fax"].ToString();
                        txtRepresentativeName.Text = re["Rep_Name"].ToString();
                        txtRepresantativeDesig.Text = re["Rep_Desig"].ToString();
                        txtRepresentativePhone.Text = re["Rep_phone"].ToString();
                        txtRepresentativeEmail.Text = re["Rep_email"].ToString();
                        txtservicetaxNo.Text = re["Service_tax_No"].ToString();
                        txtpanNo.Text = re["Pan_No"].ToString();
                        txtvat.Text = re["Vat_No"].ToString();
                        txt_pvc.Text = re["PrincipleVndrCode"].ToString();
                        txt_vndr_bankacc.Text = re["BankAccNo"].ToString();
                        txt_ifsc.Text = re["BankIfscCode"].ToString();
                        txt_accholdername.Text = re["AccountName"].ToString();
                    }
                }
            }
            DbCL.Conn.Close();
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            string userId = Session["USERID"] != null ? Session["USERID"].ToString() : "System";

            // SECURED: Parameterized and isolated by CompanyID
            string cmdstring = @"UPDATE tbl_Vendor SET 
                                    Vendor_Name=@Vendor_Name, Address1=@Address1, Address2=@Address2, 
                                    City=@City, pin=@pin, State=@State, Com_web_site=@Com_web_site, 
                                    Com_email=@Com_email, Com_phone=@Com_phone, Com_Fax=@Com_Fax, 
                                    Rep_Name=@Rep_Name, Rep_Desig=@Rep_Desig, Rep_phone=@Rep_phone, 
                                    Rep_email=@Rep_email, Service_tax_No=@Service_tax_No, Pan_No=@Pan_No, 
                                    Vat_No=@Vat_No, PrincipleVndrCode=@PrincipleVndrCode, BankAccNo=@BankAccNo, 
                                    BankIfscCode=@BankIfscCode, AccountName=@AccountName,
                                    UpdatedBy=@UpdatedBy, UpdatedOn=GETDATE()
                                 WHERE Vendor_Id=@Vendor_Id AND CompanyID=@CompanyID";

            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@Vendor_Name", txtvendorName.Text.Trim());
                cmd.Parameters.AddWithValue("@Address1", txtAddress1.Text.Trim());
                cmd.Parameters.AddWithValue("@Address2", txtAddress2.Text.Trim());
                cmd.Parameters.AddWithValue("@City", cmbcity.Text);
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
                cmd.Parameters.AddWithValue("@Vat_No", txtvat.Text.Trim());
                cmd.Parameters.AddWithValue("@PrincipleVndrCode", txt_pvc.Text.Trim());
                cmd.Parameters.AddWithValue("@BankAccNo", txt_vndr_bankacc.Text.Trim());
                cmd.Parameters.AddWithValue("@BankIfscCode", txt_ifsc.Text.Trim());
                cmd.Parameters.AddWithValue("@AccountName", txt_accholdername.Text.Trim());

                cmd.Parameters.AddWithValue("@UpdatedBy", userId);
                cmd.Parameters.AddWithValue("@Vendor_Id", lblvendor_id.Text);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

                cmd.ExecuteNonQuery();
            }
            DbCL.Conn.Close();

            // ========================================================================
            // FIXED PROACTIVE NOTIFICATION: Try/Catch Safety Net + Safe Parameters
            // ========================================================================
            try
            {
                string notifMsg = $"Vendor profile '{txtvendorName.Text.Trim()}' (ID: {lblvendor_id.Text}) was updated.";
                string notifQuery = @"INSERT INTO tbl_SystemNotification 
                                      (CompanyID, Title, Message, Module, Type, UserId, CreatedOn) 
                                      VALUES (@CompanyID, 'Vendor Updated', @Message, 'Vendor Management', 'Info', @UserId, GETDATE())";

                SqlParameter[] notifParam = {
                    new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID),
                    new SqlParameter("@Message", notifMsg),
                    new SqlParameter("@UserId", userId)
                };
                DbCL.SPExecDB(notifQuery, notifParam);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Notification Logging Failed: " + ex.Message);
            }

            PanelOK.Visible = true;
            lblOk.Text = "Vendor Details Updated Successfully!";
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/View_vendor.aspx");
        }

        // Helper to safely bind dropdowns and prevent crashes if DB string doesn't match list items
        private void SetDropdownSafe(DropDownList ddl, string val)
        {
            if (!string.IsNullOrEmpty(val) && ddl.Items.FindByValue(val) != null)
            {
                ddl.SelectedValue = val;
            }
        }
    }
}