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
    public partial class View_PR_Details : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();

        private enum PageMode { Draft, View, Approve }

        private DataTable PRItems
        {
            get
            {
                if (ViewState["PR_ITEMS"] == null) ViewState["PR_ITEMS"] = CreatePRItemTable();
                return (DataTable)ViewState["PR_ITEMS"];
            }
            set { ViewState["PR_ITEMS"] = value; }
        }

        private List<string> TaxRates
        {
            get { return ViewState["TaxRates"] as List<string> ?? new List<string> { "NA" }; }
            set { ViewState["TaxRates"] = value; }
        }

        private string CurrentReqNo
        {
            get { return ViewState["ReqNo"]?.ToString(); }
            set { ViewState["ReqNo"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }

            if (!IsPostBack)
            {
                LoadTaxRates();
                string reqNo = Request.QueryString["reqNo"];
                if (!string.IsNullOrEmpty(reqNo))
                {
                    DbCL.FillCombo(cmbvendor, "select Vendor_Name from tbl_Vendor order by Vendor_Name");
                    LoadPR(reqNo);
                    ApplyModeUI();
                }
            }
        }

        private PageMode CurrentMode
        {
            get
            {
                string mode = Request.QueryString["mode"];
                if (string.IsNullOrEmpty(mode)) return PageMode.Draft;

                mode = mode.ToLower();
                if (mode == "approve") return PageMode.Approve;
                if (mode == "view") return PageMode.View;

                return PageMode.Draft;
            }
        }

        private void LoadTaxRates()
        {
            List<string> rates = new List<string> { "NA" };
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            SqlCommand cmd = new SqlCommand("Select Vat_Rate from tbl_Vat_Master", DbCL.Conn);
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                while (rdr.Read()) rates.Add(rdr[0].ToString());
            }
            DbCL.Conn.Close();
            TaxRates = rates;
        }

        private void ApplyModeUI()
        {
            bool isDraft = (lblStatus.Text == "Draft");

            // Toggle visibility using the actual server-side tabs
            tab2.Visible = isDraft;
            step2.Visible = isDraft;

            btnNextToStep2.Visible = isDraft;
            btnNextToStep3From1.Visible = !isDraft;
            btnBackToStep2.Visible = isDraft;
            btnBackToStep1.Visible = !isDraft;

            switch (CurrentMode)
            {
                case PageMode.Draft:
                    if (!isDraft) MakeReadOnly();
                    break;
                case PageMode.View:
                    MakeReadOnly();
                    break;
                case PageMode.Approve:
                    MakeReadOnly();
                    ShowApprovalPanel();
                    break;
            }
        }

        private void MakeReadOnly()
        {
            cmbvendor.Enabled = false;
            btnSaveDraft.Visible = false;
            Button3.Visible = false;
            btnCancelPR.Visible = false;

            Step3SearchDiv.Visible = false;
            Modifier_Msg_Row.Visible = false;

            MakeGridReadOnly(gd_Service_Product);
        }

        private void MakeGridReadOnly(GridView grid)
        {
            foreach (GridViewRow row in grid.Rows)
            {
                LinkButton lnkDelete = row.FindControl("lnkDelete") as LinkButton;
                if (lnkDelete != null) lnkDelete.Visible = false;
                LockControlsRecursive(row);
            }
        }

        private void LockControlsRecursive(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                TextBox tb = ctrl as TextBox;
                if (tb != null) tb.ReadOnly = true;

                DropDownList ddl = ctrl as DropDownList;
                if (ddl != null) ddl.Enabled = false;

                CheckBox chk = ctrl as CheckBox;
                if (chk != null) chk.Enabled = false;

                if (ctrl.HasControls()) LockControlsRecursive(ctrl);
            }
        }

        private void ShowApprovalPanel()
        {
            if (lblStatus.Text != "Submitted")
            {
                ShowError("This PR is not pending approval.");
                pnlApproval.Visible = false;
                return;
            }
            pnlApproval.Visible = true;
            divActionButtons.Visible = false;
        }

        private void LoadPR(string reqNo)
        {
            CurrentReqNo = reqNo;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                con.Open();
                SqlCommand cmdHdr = new SqlCommand("SELECT * FROM tbl_RequisitionMain WHERE ReqNo=@ReqNo", con);
                cmdHdr.Parameters.AddWithValue("@ReqNo", reqNo);

                using (SqlDataReader dr = cmdHdr.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        lblReqNo.Text = reqNo;
                        lblStatus.Text = dr["Status"].ToString();
                        BindVendor(dr["VendorId"].ToString());
                    }
                }

                SqlDataAdapter da = new SqlDataAdapter("SELECT id, ProductId AS Ser_pro_code, ProductName as Ser_pro_Name, ParentCategoryId, Description, Qnty, Rate, DiscountPercent, DiscountAmount, TaxableAmount, IsTaxApplicable, gstrate, ItemOrder FROM tbl_RequisitionNew WHERE ReqNo = @ReqNo ORDER BY ItemOrder", con);
                da.SelectCommand.Parameters.AddWithValue("@ReqNo", reqNo);

                DataTable dt = new DataTable();
                da.Fill(dt);

                if (!dt.Columns.Contains("IsModified")) dt.Columns.Add("IsModified", typeof(bool));
                foreach (DataRow r in dt.Rows) r["IsModified"] = false;

                PRItems = dt;
                BindGridFromViewState();

                // AUTOMATICALLY PRE-LOAD CATEGORY FOR STEP 2
                if (lblStatus.Text == "Draft" && dt.Rows.Count > 0 && dt.Rows[0]["ParentCategoryId"] != DBNull.Value)
                {
                    string parentCatId = dt.Rows[0]["ParentCategoryId"].ToString();

                    bool isProductCat = true;
                    using (SqlCommand cmdCat = new SqlCommand("SELECT COUNT(1) FROM tbl_NewparentProduct WHERE Id = @Id", con))
                    {
                        cmdCat.Parameters.AddWithValue("@Id", parentCatId);
                        isProductCat = Convert.ToInt32(cmdCat.ExecuteScalar()) > 0;
                    }

                    RadioButtonList1.SelectedValue = isProductCat ? "Product" : "Service";
                    BindCategories();

                    ListItem catItem = cmbproduct_service.Items.FindByValue(parentCatId);
                    if (catItem != null)
                    {
                        cmbproduct_service.ClearSelection();
                        catItem.Selected = true;
                        PopulateProductGrid();
                    }
                }
            }
            CalculatePRSummary_DB(CurrentReqNo);
        }

        protected void cmbvendor_SelectedIndexChanged(object sender, EventArgs e)
        {
            hdnActiveStep.Value = "1";
            if (cmbvendor.SelectedValue == "0") return;
            BindVendor(cmbvendor.SelectedValue);
        }

        protected void BindVendor(String VendorId)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Vendor where Id='" + VendorId + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            using (SqlDataReader re = cmd.ExecuteReader())
            {
                if (re.Read())
                {
                    lbl_vendordbid.Text = re["Id"].ToString();
                    lblvendor_id.Text = re["Vendor_Id"].ToString();

                    if (cmbvendor.Items.FindByText(re["Vendor_Name"].ToString()) != null)
                    {
                        cmbvendor.ClearSelection();
                        cmbvendor.Items.FindByText(re["Vendor_Name"].ToString()).Selected = true;
                    }

                    txtAddress1.Text = re["Address1"].ToString();
                    cmbcity.Text = re["City"].ToString();
                    cmbState.Text = re["State"].ToString();
                    txtEmail.Text = re["Com_email"].ToString();
                    txtPhone.Text = re["Com_phone"].ToString();
                }
            }
            DbCL.Conn.Close();
        }

        protected void RadioButtonList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindCategories();
            gvProductsToSelect.DataSource = null;
            gvProductsToSelect.DataBind();
            hdnActiveStep.Value = "2";
        }

        private void BindCategories()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            string cmdstring = RadioButtonList1.SelectedValue == "Product"
                ? "SELECT Id, ProductOrServiceCat as CategoryName FROM tbl_NewparentProduct ORDER BY ProductOrServiceCat"
                : "SELECT Id, Service_name as CategoryName FROM tbl_Service ORDER BY Service_name";

            SqlDataAdapter da = new SqlDataAdapter(cmdstring, DbCL.Conn);
            DataTable dt = new DataTable();
            da.Fill(dt);

            cmbproduct_service.DataSource = dt;
            cmbproduct_service.DataTextField = "CategoryName";
            cmbproduct_service.DataValueField = "Id";
            cmbproduct_service.DataBind();
            cmbproduct_service.Items.Insert(0, new ListItem("--Select Category--", "0"));
            DbCL.Conn.Close();
        }

        protected void cmbproduct_service_SelectedIndexChanged(object sender, EventArgs e)
        {
            hdnActiveStep.Value = "2";
            PopulateProductGrid();
        }

        private void PopulateProductGrid()
        {
            if (cmbproduct_service.SelectedValue == "0" || string.IsNullOrEmpty(cmbproduct_service.SelectedValue))
            {
                gvProductsToSelect.DataSource = null;
                gvProductsToSelect.DataBind();
                return;
            }

            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            string cmdstring = RadioButtonList1.SelectedValue == "Product"
                ? "SELECT ProductID as ItemId, ProductName as ItemName FROM tbl_NewProduct WHERE ParentId = " + cmbproduct_service.SelectedValue + " ORDER BY ProductName"
                : "SELECT Service_code as ItemId, Service_name as ItemName FROM tbl_Service WHERE Id = " + cmbproduct_service.SelectedValue;

            SqlDataAdapter da = new SqlDataAdapter(cmdstring, DbCL.Conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            DbCL.Conn.Close();

            // Check what is already in the view state grid
            DataTable dtFiltered = dt.Clone();
            foreach (DataRow row in dt.Rows)
            {
                string currentItemId = row["ItemId"].ToString();
                bool alreadyExists = false;

                foreach (DataRow prRow in PRItems.Rows)
                {
                    if (prRow["Ser_pro_code"].ToString() == currentItemId)
                    {
                        alreadyExists = true;
                        break;
                    }
                }

                if (!alreadyExists)
                {
                    dtFiltered.ImportRow(row);
                }
            }

            gvProductsToSelect.DataSource = dtFiltered;
            gvProductsToSelect.DataBind();
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            hdnActiveStep.Value = "2";
            SyncGridToViewState();

            DataTable dtItems = PRItems;
            int addedCount = 0;

            foreach (GridViewRow row in gvProductsToSelect.Rows)
            {
                CheckBox chkSelect = (CheckBox)row.FindControl("chkSelect");
                if (chkSelect != null && chkSelect.Checked)
                {
                    string itemId = gvProductsToSelect.DataKeys[row.RowIndex].Value.ToString();
                    string itemName = Server.HtmlDecode(row.Cells[2].Text);

                    if (!dtItems.AsEnumerable().Any(r => r.Field<string>("Ser_pro_code") == itemId))
                    {
                        DataRow newRow = dtItems.NewRow();
                        newRow["id"] = 0;
                        newRow["Ser_pro_code"] = itemId;
                        newRow["Ser_pro_Name"] = itemName;
                        newRow["ParentCategoryId"] = Convert.ToInt32(cmbproduct_service.SelectedValue);
                        newRow["Description"] = "";
                        newRow["Qnty"] = 1;
                        newRow["Rate"] = 0;
                        newRow["DiscountPercent"] = 0;
                        newRow["DiscountAmount"] = 0;
                        newRow["TaxableAmount"] = 0;
                        newRow["IsTaxApplicable"] = false;
                        newRow["gstrate"] = 0;
                        newRow["ItemOrder"] = dtItems.Rows.Count + 1;
                        newRow["IsModified"] = true;

                        dtItems.Rows.Add(newRow);
                        addedCount++;
                    }
                    chkSelect.Checked = false;
                }
            }

            if (addedCount > 0)
            {
                PRItems = dtItems;
                NormalizeItemOrder();
                BindGridFromViewState();
                PopulateProductGrid(); // Remove the selected items from Step 2 grid
                ShowSuccess(addedCount + " item(s) added successfully. Click 'Next' to Review.");
            }
            else
            {
                ShowError("No items were selected, or they already exist in your requisition.");
            }
        }

        protected void gd_Service_Product_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (lblStatus.Text == "Draft") WireModificationTracking(e.Row);

                DropDownList dp1 = (DropDownList)e.Row.FindControl("vat_parsentage");
                HiddenField hdnSelectedGST = (HiddenField)e.Row.FindControl("hdnSelectedGST");

                if (dp1 != null)
                {
                    dp1.Items.Clear();
                    dp1.Items.AddRange(TaxRates.Select(rate => new ListItem(rate)).ToArray());

                    if (hdnSelectedGST != null && !string.IsNullOrEmpty(hdnSelectedGST.Value))
                    {
                        decimal gstVal;
                        if (decimal.TryParse(hdnSelectedGST.Value, out gstVal))
                        {
                            ListItem item = dp1.Items.FindByValue(gstVal.ToString("0.##"))
                                         ?? dp1.Items.FindByValue(gstVal.ToString("0.00"))
                                         ?? dp1.Items.FindByValue(gstVal.ToString("0"));

                            if (item == null && gstVal == 0) item = dp1.Items.FindByValue("NA");

                            if (item != null)
                            {
                                dp1.ClearSelection();
                                item.Selected = true;
                            }
                        }
                    }
                }
            }
        }

        private void WireModificationTracking(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                TextBox tb = ctrl as TextBox;
                if (tb != null && !tb.ReadOnly && tb.ID != "TaxableAmount")
                {
                    tb.Attributes["onkeyup"] = "markRowModified(this); calculateDiscount(this);";
                }
                if (ctrl.HasControls()) WireModificationTracking(ctrl);
            }
        }

        protected void gd_Service_Product_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "DeleteItem") return;
            if (lblStatus.Text != "Draft") { ShowError("This PR can no longer be modified."); return; }

            SyncGridToViewState();
            hdnActiveStep.Value = "3";

            int rowId = Convert.ToInt32(e.CommandArgument);

            if (rowId > 0)
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("DELETE FROM tbl_RequisitionNew WHERE id=@id", con);
                    cmd.Parameters.AddWithValue("@id", rowId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            GridViewRow row = (GridViewRow)(((LinkButton)e.CommandSource).NamingContainer);
            int rowIndex = row.RowIndex;
            if (rowIndex >= 0 && rowIndex < PRItems.Rows.Count)
            {
                PRItems.Rows.RemoveAt(rowIndex);
            }

            NormalizeItemOrder();
            BindGridFromViewState();
            PopulateProductGrid();
            CalculatePRSummary_DB(CurrentReqNo);
        }

        protected void btnSaveEdit_Click(object sender, EventArgs e)
        {
            if (lblStatus.Text != "Draft") { ShowError("This PR can no longer be modified."); return; }
            hdnActiveStep.Value = "3";
            ClearMessages();

            // False = Triggered by "Save Edits" button. It will show an error if nothing was modified.
            SaveModifications(false);
        }

        private DataTable BuildRequisitionItemTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ProductId", typeof(string));
            dt.Columns.Add("ProductName", typeof(string));
            dt.Columns.Add("ParentCategoryId", typeof(int));
            dt.Columns.Add("Description", typeof(string));
            dt.Columns.Add("Qnty", typeof(decimal));
            dt.Columns.Add("Rate", typeof(decimal));
            dt.Columns.Add("DiscountPercent", typeof(decimal));
            dt.Columns.Add("DiscountAmount", typeof(decimal));
            dt.Columns.Add("IsTaxApplicable", typeof(bool));
            dt.Columns.Add("GST", typeof(decimal));
            dt.Columns.Add("ItemOrder", typeof(int));
            return dt;
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            if (lblStatus.Text != "Draft") { ShowError("This PR can no longer be submitted."); return; }
            hdnActiveStep.Value = "3";
            ClearMessages();

            // True = Triggered by "Submit" button. It will skip saving if nothing was modified and proceed to submit.
            bool saveSuccess = SaveModifications(true);

            // If there was a critical SQL error during save, stop the submission process
            if (!saveSuccess) return;

            if (string.IsNullOrEmpty(CurrentReqNo)) return;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();
                try
                {
                    // Calculate and lock final totals
                    UpdatePRTotals_OnSubmit(con, tran, CurrentReqNo);

                    // Submit PR
                    SqlCommand cmd = new SqlCommand("sp_SubmitRequisition", con, tran);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ReqNo", CurrentReqNo);
                    cmd.Parameters.AddWithValue("@UserId", Session["USERID"].ToString());
                    cmd.ExecuteNonQuery();

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ShowError("Submit failed: " + ex.Message);
                    return;
                }
            }

            ApplyStatusUI("Submitted");
            ShowSuccess("PR submitted successfully with locked totals.");
        }

        // --- NEW HELPER METHOD ---
        private bool SaveModifications(bool isSubmit)
        {
            string userId = Session["USERID"]?.ToString();
            if (string.IsNullOrEmpty(userId)) { ShowError("Session expired."); return false; }

            SyncGridToViewState();
            DataTable dt = BuildRequisitionItemTable();

            foreach (DataRow row in PRItems.Rows)
            {
                bool isModified = row["IsModified"] != DBNull.Value && Convert.ToBoolean(row["IsModified"]);
                int id = row["id"] != DBNull.Value ? Convert.ToInt32(row["id"]) : 0;

                // Skip items that exist in the DB and haven't been touched
                if (!isModified && id != 0) continue;

                dt.Rows.Add(
                    row["Ser_pro_code"], row["Ser_pro_Name"], row["ParentCategoryId"],
                    row["Description"], row["Qnty"], row["Rate"], row["DiscountPercent"], row["DiscountAmount"],
                    row["IsTaxApplicable"], row["gstrate"], row["ItemOrder"]
                );
            }

            // If 0 rows were modified...
            if (dt.Rows.Count == 0)
            {
                if (!isSubmit)
                {
                    ShowError("No modified rows to save.");
                }
                // Return true so the Submit process knows it's safe to continue!
                return true;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_RequisitionItem_BulkUpsert", con, tran);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ClientName", cmbvendor.SelectedItem.Text);
                    cmd.Parameters.AddWithValue("@ReqNo", lblReqNo.Text);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    SqlParameter tvp = cmd.Parameters.AddWithValue("@Items", dt);
                    tvp.SqlDbType = SqlDbType.Structured;
                    tvp.TypeName = "dbo.RequisitionItem_TVP";

                    cmd.ExecuteNonQuery();
                    tran.Commit();

                    if (!isSubmit)
                    {
                        ShowSuccess("Modified items saved successfully.");
                    }

                    LoadPR(CurrentReqNo); // Reload to get fresh DB IDs for newly added items
                    return true;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ShowError("Save failed: " + ex.Message);
                    return false;
                }
            }
        }

        private void ApplyStatusUI(string status)
        {
            lblStatus.Text = status;
            bool isDraft = (status == "Draft");

            btnSaveDraft.Enabled = isDraft;
            Button3.Enabled = isDraft;
            btnCancelPR.Visible = isDraft;

            gd_Service_Product.Enabled = isDraft;
            gvProductsToSelect.Enabled = isDraft;

            if (!isDraft)
            {
                SearchBox_Row.Visible = false;
                Button2.Visible = false;
            }
        }

        protected void btnCancelPR_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_CancelRequisition", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ReqNo", CurrentReqNo);
                cmd.Parameters.AddWithValue("@CancelledBy", Session["USERID"].ToString());
                cmd.Parameters.AddWithValue("@CancelReason", "Cancelled by user");
                con.Open();
                cmd.ExecuteNonQuery();
            }

            ApplyStatusUI("Cancelled");
            ShowSuccess("PR cancelled successfully.");
        }

        private void CalculatePRSummary_DB(string reqNo)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(@"
                SELECT 
                    CAST(SUM(Qnty * Rate) AS DECIMAL(18,2)) AS GrossAmount,
                    CAST(SUM(ISNULL(DiscountAmount,0)) AS DECIMAL(18,2)) AS DiscountAmount,
                    CAST(SUM(TaxableAmount) AS DECIMAL(18,2)) AS TaxableAmount,
                    CAST(SUM(CASE WHEN IsTaxApplicable = 1 THEN TaxableAmount * gstrate / 100 ELSE 0 END) AS DECIMAL(18,2)) AS GSTAmount
                FROM tbl_RequisitionNew WHERE ReqNo = @ReqNo;", con);

                cmd.Parameters.AddWithValue("@ReqNo", reqNo);
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        decimal gross = dr.IsDBNull(0) ? 0 : dr.GetDecimal(0);
                        decimal discount = dr.IsDBNull(1) ? 0 : dr.GetDecimal(1);
                        decimal taxable = dr.IsDBNull(2) ? 0 : dr.GetDecimal(2);
                        decimal gst = dr.IsDBNull(3) ? 0 : dr.GetDecimal(3);

                        lblGross.Text = gross.ToString("N2");
                        lblDiscount.Text = discount.ToString("N2");
                        lblTaxable.Text = taxable.ToString("N2");
                        lblGST.Text = gst.ToString("N2");
                        lblNet.Text = (taxable + gst).ToString("N2");
                    }
                }
            }
        }

        private void UpdatePRTotals_OnSubmit(SqlConnection con, SqlTransaction tran, string reqNo)
        {
            SqlCommand cmdCalc = new SqlCommand(@"
                SELECT 
                    CAST(SUM(CAST(Qnty AS DECIMAL(18,3)) * CAST(Rate AS DECIMAL(18,2))) AS DECIMAL(18,2)),
                    CAST(SUM(ISNULL(CAST(DiscountAmount AS DECIMAL(18,2)), 0)) AS DECIMAL(18,2)),
                    CAST(SUM(CAST(TaxableAmount AS DECIMAL(18,2))) AS DECIMAL(18,2)),
                    CAST(SUM(CASE WHEN IsTaxApplicable = 1 THEN CAST(TaxableAmount AS DECIMAL(18,2)) * ISNULL(CAST(gstrate AS DECIMAL(5,2)),0) / 100 ELSE 0 END) AS DECIMAL(18,2))
                FROM tbl_RequisitionNew WHERE ReqNo = @ReqNo", con, tran);

            cmdCalc.Parameters.Add("@ReqNo", SqlDbType.VarChar, 50).Value = reqNo;
            decimal gross = 0, discount = 0, taxable = 0, gst = 0;

            using (SqlDataReader dr = cmdCalc.ExecuteReader())
            {
                if (dr.Read())
                {
                    gross = dr.IsDBNull(0) ? 0 : dr.GetDecimal(0);
                    discount = dr.IsDBNull(1) ? 0 : dr.GetDecimal(1);
                    taxable = dr.IsDBNull(2) ? 0 : dr.GetDecimal(2);
                    gst = dr.IsDBNull(3) ? 0 : dr.GetDecimal(3);
                }
            }

            if (taxable <= 0 && gross <= 0) throw new Exception("PR total amount must be greater than zero.");

            SqlCommand cmdUpdate = new SqlCommand(@"
                UPDATE tbl_RequisitionMain 
                SET GrossAmount = @Gross, DiscountAmount = @Discount, TaxableAmount = @Taxable, GSTAmount = @GST, NetAmount = @Net 
                WHERE ReqNo = @ReqNo", con, tran);

            cmdUpdate.Parameters.AddWithValue("@Gross", gross);
            cmdUpdate.Parameters.AddWithValue("@Discount", discount);
            cmdUpdate.Parameters.AddWithValue("@Taxable", taxable);
            cmdUpdate.Parameters.AddWithValue("@GST", gst);
            cmdUpdate.Parameters.AddWithValue("@Net", taxable + gst);
            cmdUpdate.Parameters.AddWithValue("@ReqNo", reqNo);
            cmdUpdate.ExecuteNonQuery();
        }

        protected void btnApprove_Click(object sender, EventArgs e) { ProcessApproval("Approved"); }
        protected void btnReject_Click(object sender, EventArgs e) { ProcessApproval("Rejected"); }

        private void ProcessApproval(string action)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Requisition_Approve", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ReqNo", lblReqNo.Text);
                cmd.Parameters.AddWithValue("@ApproverUserId", Session["USERID"].ToString());
                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@Remarks", txtApprovalRemarks.Text);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            Response.Redirect("Approve_PR.aspx");
        }

        private DataTable CreatePRItemTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("id", typeof(int));
            dt.Columns.Add("Ser_pro_code", typeof(string));
            dt.Columns.Add("Ser_pro_Name", typeof(string));
            dt.Columns.Add("ParentCategoryId", typeof(int));
            dt.Columns.Add("Description", typeof(string));
            dt.Columns.Add("Qnty", typeof(decimal));
            dt.Columns.Add("Rate", typeof(decimal));
            dt.Columns.Add("DiscountPercent", typeof(decimal));
            dt.Columns.Add("DiscountAmount", typeof(decimal));
            dt.Columns.Add("TaxableAmount", typeof(decimal));
            dt.Columns.Add("IsTaxApplicable", typeof(bool));
            dt.Columns.Add("gstrate", typeof(decimal));
            dt.Columns.Add("ItemOrder", typeof(int));
            dt.Columns.Add("IsModified", typeof(bool));
            return dt;
        }

        private void BindGridFromViewState()
        {
            gd_Service_Product.DataSource = PRItems;
            gd_Service_Product.DataBind();
        }

        private void NormalizeItemOrder()
        {
            int i = 1;
            foreach (DataRow r in PRItems.Rows) r["ItemOrder"] = i++;
        }

        private void ShowSuccess(string msg) { PanelOK.Visible = true; PanelError.Visible = false; lblOk.Text = msg; }
        private void ShowError(string msg) { PanelError.Visible = true; PanelOK.Visible = false; lblErrorMsg.Text = msg; }
        private void ClearMessages() { PanelOK.Visible = false; PanelError.Visible = false; }

        private decimal? ToDecimal(GridViewRow row, string controlId)
        {
            TextBox txt = row.FindControl(controlId) as TextBox;
            if (txt == null || string.IsNullOrWhiteSpace(txt.Text)) return null;

            decimal val;
            if (decimal.TryParse(txt.Text.Trim(), out val)) return val;
            return null;
        }

        private int ToInt(GridViewRow row, string controlId)
        {
            TextBox txt = row.FindControl(controlId) as TextBox;
            if (txt == null || string.IsNullOrWhiteSpace(txt.Text)) return 0;

            int val;
            if (int.TryParse(txt.Text.Trim(), out val)) return val;
            return 0;
        }

        private void SyncGridToViewState()
        {
            DataTable dt = PRItems;
            for (int i = 0; i < gd_Service_Product.Rows.Count; i++)
            {
                GridViewRow row = gd_Service_Product.Rows[i];
                if (i < dt.Rows.Count)
                {
                    dt.Rows[i]["Description"] = ((TextBox)row.FindControl("sepecification")).Text;
                    dt.Rows[i]["Qnty"] = ToDecimal(row, "Quantity") ?? 0;
                    dt.Rows[i]["Rate"] = ToDecimal(row, "Vendor_rate") ?? 0;
                    dt.Rows[i]["DiscountPercent"] = ToDecimal(row, "DiscountPercent") ?? 0;
                    dt.Rows[i]["DiscountAmount"] = ToDecimal(row, "DiscountAmount") ?? 0;

                    CheckBox chkTax = (CheckBox)row.FindControl("chkTaxApplicable");
                    dt.Rows[i]["IsTaxApplicable"] = chkTax != null && chkTax.Checked;

                    DropDownList ddlGST = (DropDownList)row.FindControl("vat_parsentage");
                    decimal gst;
                    if (ddlGST != null && ddlGST.SelectedValue != "NA" && !string.IsNullOrEmpty(ddlGST.SelectedValue))
                    {
                        if (decimal.TryParse(ddlGST.SelectedValue, out gst)) dt.Rows[i]["gstrate"] = gst;
                        else dt.Rows[i]["gstrate"] = 0;
                    }
                    else
                    {
                        dt.Rows[i]["gstrate"] = 0;
                    }

                    dt.Rows[i]["ItemOrder"] = ToInt(row, "txtOrder");

                    HiddenField hdnModified = (HiddenField)row.FindControl("hdnIsModified");
                    if (hdnModified != null && hdnModified.Value == "1") dt.Rows[i]["IsModified"] = true;
                }
            }
            PRItems = dt;
        }
    }
}