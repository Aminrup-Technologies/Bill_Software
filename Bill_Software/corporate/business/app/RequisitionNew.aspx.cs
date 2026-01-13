using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace Bill_Software.corporate.business.app
{
    public partial class RequisitionNew : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        public DataTable first_datatable;
        public static DataTable Dt = new DataTable("Table");
        private List<string> vatRates;
        private List<string> serviceTaxRates;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                Dt = new DataTable("Table");
                DbCL.FillCombo(cmbState, "select State_Name from tbl_State order by State_Name");
                DbCL.FillCombo(cmbcity, "select City_Name from tbl_City order by City_Name");
                DbCL.FillCombo(cmbvendor, "select Vendor_Name from tbl_Vendor order by Vendor_Name");
            }
        }



        private void ShowSuccess(string message)
        {
            PanelOK.Visible = true;
            PanelError.Visible = false;

            lblOk.Text = message;
        }

        private void ShowError(string message)
        {
            PanelError.Visible = true;
            PanelOK.Visible = false;

            lblErrorMsg.Text = message;
        }

        private void ClearMessages()
        {
            PanelOK.Visible = false;
            PanelError.Visible = false;

            lblOk.Text = "";
            lblErrorMsg.Text = "";
        }


        protected void cmbvendor_SelectedIndexChanged(object sender, EventArgs e)
        {
            Label1.Visible = true;
            RadioButtonList1.Visible = true;
            Button1.Visible = true;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Vendor where Vendor_Name='" + cmbvendor.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lbl_vendordbid.Text = re["Id"].ToString();
                lblvendor_id.Text = re["Vendor_Id"].ToString();
                txtAddress1.Text = re["Address1"].ToString();
                txtAddress2.Text = re["Address2"].ToString();
                cmbcity.Text = re["City"].ToString();
                txtPin.Text = re["pin"].ToString();
                cmbState.Text = re["State"].ToString();
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
            }
            DbCL.Conn.Close();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Label1.Visible = false;
            RadioButtonList1.Visible = false;
            Button1.Visible = false;
            Panel1.Visible = true;
            BindListitem();
        }

        private void BindListitem_OLD()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                //cmdstring = "select Product_Name from tbl_Product order by Product_Name";
                cmdstring = "select ProductOrServiceCat from tbl_NewparentProduct order by Id";
            }
            else
            {
                cmdstring = "select Service_name  from tbl_Service order by Service_name";
            }
            cmbproduct_service.Items.Clear();
            cmbproduct_service.Items.Add("--Select--");
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            while (re.Read())
            {
                cmbproduct_service.Items.Add(re.GetValue(0).ToString());
            }
            DbCL.Conn.Close();

        }

        private void BindListitem()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            string cmdstring = "SELECT Id, ProductOrServiceCat FROM tbl_NewparentProduct ORDER BY Id";

            SqlDataAdapter da = new SqlDataAdapter(cmdstring, DbCL.Conn);
            DataTable dt = new DataTable();
            da.Fill(dt);

            cmbproduct_service.DataSource = dt;
            cmbproduct_service.DataTextField = "ProductOrServiceCat";
            cmbproduct_service.DataValueField = "Id";
            cmbproduct_service.DataBind();

            cmbproduct_service.Items.Insert(0, new ListItem("--Select--", "0"));

            DbCL.Conn.Close();
        }


        protected void Button2_Click(object sender, EventArgs e)
        {
            string prevValue = cmbproduct_service.SelectedValue;

            LoadTaxRates();

            int selectedId = Convert.ToInt32(prevValue);
            string selectedText = cmbproduct_service.SelectedItem.Text;

            Panel2.Visible = true;

            if (RadioButtonList1.SelectedIndex == 0)
            {
                Binddata1("SELECT ProductID, ProductName FROM tbl_NewProduct WHERE ParentId = " + selectedId);
            }
            else
            {
                Binddata1("SELECT Service_code, Service_name FROM tbl_Service WHERE Service_name = '" + selectedText + "'");
            }

            gd_Service_Product.DataSource = Dt;
            gd_Service_Product.DataBind();
            ViewState["dt"] = Dt;

            cmbproduct_service.SelectedValue = prevValue; // restore
        }


        private void LoadTaxRates()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            // Fetch VAT Rates
            string vatQuery = "Select Vat_Rate from tbl_Vat_Master";
            SqlCommand vatCmd = new SqlCommand(vatQuery, DbCL.Conn);
            SqlDataReader vatRdr = vatCmd.ExecuteReader();

            vatRates = new List<string> { "NA" }; // Initialize with "NA"
            while (vatRdr.Read())
            {
                vatRates.Add(vatRdr[0].ToString());
            }
            vatRdr.Close();

            // Fetch Service Tax Rates
            string serviceTaxQuery = "Select Service_tax from tbl_Service_master";
            SqlCommand serviceTaxCmd = new SqlCommand(serviceTaxQuery, DbCL.Conn);
            SqlDataReader serviceTaxRdr = serviceTaxCmd.ExecuteReader();

            serviceTaxRates = new List<string> { "NA" }; // Initialize with "NA"
            while (serviceTaxRdr.Read())
            {
                serviceTaxRates.Add(serviceTaxRdr[0].ToString());
            }
            serviceTaxRdr.Close();

            DbCL.Conn.Close();
        }

        private void Binddata1(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            try
            {
                // Create and configure the SqlCommand
                SqlCommand com1 = new SqlCommand(cmdstring, DbCL.Conn);

                // Use SqlDataAdapter to fill the DataTable directly
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(com1);
                da.Fill(dt); // Fill the DataTable with data

                // Check if the DataTable has rows to process
                if (dt.Rows.Count > 0)
                {
                    first_datatable = dt;

                    // Call the appropriate grid function based on Label2.Text
                    if (Label2.Text == "1")
                    {
                        newgrid1();
                    }
                    else
                    {
                        newgrid();
                    }

                    // Update Label2 to ensure the function executes only once
                    Label2.Text = (Convert.ToInt32(Label2.Text) + 1).ToString();
                }
            }
            finally
            {
                // Close the database connection in the finally block to ensure it always closes
                DbCL.Conn.Close();
            }
        }

        private void newgrid1()
        {
            DataTable dt = first_datatable;

            // Ensure Dt is initialized
            if (Dt == null)
                Dt = new DataTable();

            // Clear existing columns and rows if needed
            Dt.Clear();
            Dt.Columns.Clear();

            // Add necessary columns
            Dt.Columns.Add("Ser_pro_code", typeof(string));
            Dt.Columns.Add("Ser_pro_Name", typeof(string));

            //// Add the "Order" column here
            if (!Dt.Columns.Contains("Order"))
            {
                Dt.Columns.Add("Order", typeof(int));
            }

            // Add rows from first_datatable
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = Dt.NewRow();
                dr["Ser_pro_code"] = dt.Rows[i][0].ToString();
                dr["Ser_pro_Name"] = dt.Rows[i][1].ToString();

                // Initialize order, e.g., by default assign sequential order
                //dr["Order"] = i + 1;

                Dt.Rows.Add(dr);
            }
        }

        private void newgrid()
        {
            DataTable dt = first_datatable;

            if (Dt == null)
                Dt = new DataTable();

            Dt.Clear();
            Dt.Columns.Clear();

            // Add columns
            Dt.Columns.Add("Ser_pro_code", typeof(string));
            Dt.Columns.Add("Ser_pro_Name", typeof(string));

            // Add the Order column
            Dt.Columns.Add("Order", typeof(int));

            // Fill rows with default Order values (1-based index)
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = Dt.NewRow();
                dr["Ser_pro_code"] = dt.Rows[i][0].ToString();
                dr["Ser_pro_Name"] = dt.Rows[i][1].ToString();
                //dr["Order"] = i + 1;  // default ordering sequence
                Dt.Rows.Add(dr);
            }
        }

        protected void gd_Service_Product_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DropDownList dp1 = (DropDownList)e.Row.Cells[4].FindControl("vat_parsentage");

                // Clear existing items
                dp1.Items.Clear();

                if (RadioButtonList1.SelectedIndex == 0) // VAT Rates
                {
                    dp1.Items.AddRange(vatRates.Select(rate => new ListItem(rate)).ToArray());
                }
                else // Service Tax Rates
                {
                    dp1.Items.AddRange(serviceTaxRates.Select(rate => new ListItem(rate)).ToArray());
                }
            }
        }

        private string CurrentReqNo
        {
            get { return ViewState["ReqNo"]?.ToString(); }
            set { ViewState["ReqNo"] = value; }
        }

        protected void btnSaveDraft_Click_OLD(object sender, EventArgs e)
        {
            ClearMessages();

            try
            {
                if (!validateModifiedRows_Server())
                {
                    ShowError("Please correct the highlighted fields before saving.");
                    return;
                }

                using (SqlConnection con = new SqlConnection(
                    ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    con.Open();
                    SqlTransaction tran = con.BeginTransaction();

                    try
                    {
                        string reqNo = SaveOrUpdatePRHeader(con, tran);
                        SaveModifiedPRItems(reqNo, con, tran);

                        tran.Commit();

                        lblReqNo.Text = reqNo;
                        lblStatus.Text = "Draft";

                        ShowSuccess("Purchase Requisition saved as Draft successfully.");
                        //ShowSuccess("PR saved successfully.");
                        ScriptManager.RegisterStartupScript(this, GetType(), "scrollMsg", "scrollToMessage();", true);
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        ShowError("Failed to save PR. " + ex.Message);
                        ScriptManager.RegisterStartupScript(this, GetType(), "scrollMsg", "scrollToMessage();", true);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Unexpected error occurred. " + ex.Message);
                ScriptManager.RegisterStartupScript(this, GetType(), "scrollMsg", "scrollToMessage();", true);
            }
        }

        protected void btnSaveDraft_Click(object sender, EventArgs e)
        {
            ClearMessages();

            string userId = Session["USERID"]?.ToString();
            if (string.IsNullOrEmpty(userId))
            {
                ShowError("Session expired.");
                return;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    // 1️⃣ Create PR Header (only once)
                    if (string.IsNullOrEmpty(CurrentReqNo))
                    {
                        SqlCommand cmdHdr = new SqlCommand("sp_Requisition_CreateDraft", con, tran);
                        cmdHdr.CommandType = CommandType.StoredProcedure;

                        cmdHdr.Parameters.Add("@ClientName", SqlDbType.NVarChar, 250).Value = cmbvendor.SelectedItem.Text;
                        int vendorId;
                        if (!int.TryParse(lbl_vendordbid.Text, out vendorId))
                        {
                            ShowError("Invalid Vendor selected.");
                            return;
                        }

                        cmdHdr.Parameters.Add("@VendorId", SqlDbType.Int).Value = vendorId;
                        cmdHdr.Parameters.AddWithValue("@CreatedBy", userId);

                        SqlParameter outReqNo =
                        new SqlParameter("@ReqNo", SqlDbType.VarChar, 250)
                        {
                            Direction = ParameterDirection.Output
                        };

                        cmdHdr.Parameters.Add(outReqNo);
                        cmdHdr.ExecuteNonQuery();

                        CurrentReqNo = outReqNo.Value.ToString();
                        lblReqNo.Text = CurrentReqNo;
                    }

                    // 2️⃣ Save ONLY MODIFIED ITEMS
                    foreach (GridViewRow row in gd_Service_Product.Rows)
                    {
                        HiddenField hdn = (HiddenField)row.FindControl("hdnIsModified");
                        if (hdn?.Value != "1") continue;
                        int selectedId = Convert.ToInt32(cmbproduct_service.SelectedValue);
                        SqlCommand cmdItem = new SqlCommand("sp_RequisitionItem_Upsert", con, tran);
                        cmdItem.CommandType = CommandType.StoredProcedure;
                        cmdItem.Parameters.AddWithValue("@ClientName", cmbvendor.SelectedItem.Text.ToString());
                        cmdItem.Parameters.AddWithValue("@ReqNo", CurrentReqNo);
                        cmdItem.Parameters.AddWithValue("@ProductId",((Label)row.FindControl("Ser_pro_code")).Text);
                        cmdItem.Parameters.AddWithValue("@ParentCategoryId", selectedId); // map if available
                        cmdItem.Parameters.AddWithValue("@Description",((TextBox)row.FindControl("sepecification")).Text);
                        cmdItem.Parameters.AddWithValue("@Qnty",((TextBox)row.FindControl("Quantity")).Text);
                        cmdItem.Parameters.AddWithValue("@Rate",((TextBox)row.FindControl("Vendor_rate")).Text);
                        cmdItem.Parameters.AddWithValue("@GST",GetGSTRate(row, GetTaxApplicable(row)));
                        cmdItem.Parameters.AddWithValue("@ItemOrder",((TextBox)row.FindControl("txtOrder")).Text);
                        cmdItem.Parameters.AddWithValue("@UserId", userId);
                        cmdItem.ExecuteNonQuery();
                    }

                    tran.Commit();

                    lblStatus.Text = "Draft";
                    ShowSuccess("PR saved as Draft successfully.");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ShowError("Save failed: " + ex.Message);
                }
            }
        }


        private string SaveOrUpdatePRHeader(SqlConnection con, SqlTransaction tran)
        {
            string reqNo = CurrentReqNo;

            if (string.IsNullOrEmpty(reqNo))
            {
                // NEW PR
                reqNo = GenerateReqNo(con, tran);

                SqlCommand cmd = new SqlCommand(@"
                INSERT INTO tbl_RequisitionMain
                (ReqNo, VendorId, Vendor, Status, CreatedBy, CreatedOn)
                VALUES
                (@ReqNo, @VendorId, @Vendor, 'Draft', @User, GETDATE())", con, tran);
                cmd.Parameters.AddWithValue("@ReqNo", reqNo);
                cmd.Parameters.AddWithValue("@VendorId", lblvendor_id.Text);
                cmd.Parameters.AddWithValue("@Vendor", cmbvendor.SelectedItem.Text);
                cmd.Parameters.AddWithValue("@User", Session["USERID"].ToString());

                cmd.ExecuteNonQuery();
            }
            else
            {
                // UPDATE EXISTING DRAFT
                SqlCommand cmd = new SqlCommand(@"
                UPDATE tbl_RequisitionMain SET VendorId=@VendorId, Vendor=@Vendor WHERE ReqNo=@ReqNo AND Status='Draft'", con, tran);
                cmd.Parameters.AddWithValue("@ReqNo", reqNo);
                cmd.Parameters.AddWithValue("@VendorId", lblvendor_id.Text);
                cmd.Parameters.AddWithValue("@Vendor", cmbvendor.SelectedItem.Text);
                cmd.ExecuteNonQuery();
            }

            return reqNo;
        }

        private void SaveModifiedPRItems(string reqNo,
                                 SqlConnection con,
                                 SqlTransaction tran)
        {
            foreach (GridViewRow row in gd_Service_Product.Rows)
            {
                HiddenField hdn = (HiddenField)row.FindControl("hdnIsModified");
                if (hdn == null || hdn.Value != "1")
                    continue; // 🚫 skip untouched rows

                string productId = ((Label)row.FindControl("Ser_pro_code")).Text;
                string productName =((Label)row.FindControl("Ser_pro_Name")).Text;
                string spec =((TextBox)row.FindControl("sepecification")).Text;
                int qty =int.Parse(((TextBox)row.FindControl("Quantity")).Text);
                decimal rate = decimal.Parse(((TextBox)row.FindControl("Vendor_rate")).Text);
                int order = int.Parse(((TextBox)row.FindControl("txtOrder")).Text);
                string taxApplicable = GetTaxApplicable(row);
                decimal gstRate = GetGSTRate(row, taxApplicable);

                // Upsert logic (Item may already exist)
                SqlCommand cmd = new SqlCommand(@"
                IF EXISTS (
                    SELECT 1 FROM tbl_RequisitionNew
                    WHERE ReqNo=@ReqNo AND ProductId=@ProductId
                )
                BEGIN
                    UPDATE tbl_RequisitionNew
                    SET Description=@Desc,
                        Qnty=@Qty,
                        Rate=@Rate,
                        gstrate=@GST,
                        ItemOrder=@Order
                    WHERE ReqNo=@ReqNo AND ProductId=@ProductId
                END
                ELSE
                BEGIN
                    INSERT INTO tbl_RequisitionNew
                    (ReqNo, ProductId, Description, Qnty, Rate, gstrate, ItemOrder)
                    VALUES
                    (@ReqNo, @ProductId, @Desc, @Qty, @Rate, @GST, @Order)
                END",
                con, tran);

                cmd.Parameters.AddWithValue("@ReqNo", reqNo);
                cmd.Parameters.AddWithValue("@ProductId", productId);
                cmd.Parameters.AddWithValue("@Desc", spec);
                cmd.Parameters.AddWithValue("@Qty", qty);
                cmd.Parameters.AddWithValue("@Rate", rate);
                cmd.Parameters.AddWithValue("@GST", gstRate);
                cmd.Parameters.AddWithValue("@Order", order);

                cmd.ExecuteNonQuery();
            }
        }

        private string GetTaxApplicable(GridViewRow row)
        {
            RadioButtonList rbl = (RadioButtonList)row.FindControl("RadioButtonList1");
            return rbl?.SelectedValue ?? "No";
        }

        private decimal GetGSTRate(GridViewRow row, string taxApplicable)
        {
            if (taxApplicable != "Yes")
                return 0;

            DropDownList ddl = (DropDownList)row.FindControl("vat_parsentage");
            return ddl == null ? 0 : decimal.Parse(ddl.SelectedValue);
        }

        private string GenerateReqNo(SqlConnection con, SqlTransaction tran)
        {
            SqlCommand cmd = new SqlCommand(
                "SELECT ISNULL(MAX(Id),0)+1 FROM tbl_RequisitionMain",
                con, tran);

            int nextId = (int)cmd.ExecuteScalar();
            return "PR/" + DateTime.Now.Year + "/" + nextId.ToString("D5");
        }

        private bool validateModifiedRows_Server()
        {
            foreach (GridViewRow row in gd_Service_Product.Rows)
            {
                HiddenField hdn = (HiddenField)row.FindControl("hdnIsModified");
                if (hdn?.Value != "1") continue;

                if (string.IsNullOrWhiteSpace(
                    ((TextBox)row.FindControl("Quantity")).Text))
                    return false;
            }
            return true;
        }



        protected void Button3_Click(object sender, EventArgs e)
        {
            btnSubmit_Click(sender, e);
        }

        protected void btnSubmit_Click_OLD(object sender, EventArgs e)
        {
            ClearMessages();

            try
            {
                if (string.IsNullOrEmpty(CurrentReqNo))
                {
                    ShowError("Please save the PR before submitting.");
                    return;
                }

                using (SqlConnection con = new SqlConnection(
                    ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand(
                        "EXEC sp_SubmitRequisition @ReqNo, @UserId",
                        con);

                    cmd.Parameters.AddWithValue("@ReqNo", CurrentReqNo);
                    cmd.Parameters.AddWithValue("@UserId", Session["USERID"].ToString());

                    cmd.ExecuteNonQuery();
                }

                lblStatus.Text = "Submitted";
                //LockUIAfterSubmit();

                ShowSuccess("Purchase Requisition submitted successfully for approval.");
            }
            catch (Exception ex)
            {
                ShowError("Submission failed. " + ex.Message);
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            ClearMessages();

            string userId = Session["USERID"]?.ToString();
            if (string.IsNullOrEmpty(CurrentReqNo))
            {
                ShowError("Please save the PR before submitting.");
                return;
            }

            using (SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_SubmitRequisition", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ReqNo", CurrentReqNo);
                cmd.Parameters.AddWithValue("@UserId", userId);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            lblStatus.Text = "Submitted";
            ApplyStatusUI("Submitted");

            ShowSuccess("PR submitted successfully for approval.");
        }


        private void ApplyStatusUI(string status)
        {
            lblStatus.Text = status;

            btnSaveDraft.Visible = (status == "Draft");
            Button3.Visible = (status == "Draft"); // Save / Submit
            btnReorder.Visible = (status == "Draft");

            // After submit
            if (status != "Draft")
            {
                btnSaveDraft.Enabled = false;
                Button3.Enabled = false;
                btnReorder.Enabled = false;
            }
        }

    }
}