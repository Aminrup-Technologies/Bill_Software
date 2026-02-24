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

        // FIX: Safe State Management (Replaces static DataTable)
        private DataTable GridData
        {
            get
            {
                if (ViewState["GridData"] == null)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Ser_pro_code", typeof(string));
                    dt.Columns.Add("Ser_pro_Name", typeof(string));
                    dt.Columns.Add("Description", typeof(string));
                    dt.Columns.Add("Qnty", typeof(decimal));
                    dt.Columns.Add("Rate", typeof(decimal));
                    dt.Columns.Add("DiscountPercent", typeof(decimal));
                    dt.Columns.Add("DiscountAmount", typeof(decimal));
                    dt.Columns.Add("IsTaxApplicable", typeof(bool));
                    dt.Columns.Add("GST", typeof(decimal));
                    dt.Columns.Add("ItemOrder", typeof(int));
                    ViewState["GridData"] = dt;
                }
                return (DataTable)ViewState["GridData"];
            }
            set { ViewState["GridData"] = value; }
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
                BindCategories();

                string reqNo = Request.QueryString["reqNo"];
                if (!string.IsNullOrEmpty(reqNo))
                {
                    LoadPR(reqNo);
                }
                else
                {
                    DbCL.FillCombo(cmbvendor, "select Vendor_Name from tbl_Vendor order by Vendor_Name");
                    cmbvendor.Items.Insert(0, new ListItem("--Select Vendor--", "0"));
                }
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

        protected void cmbvendor_SelectedIndexChanged(object sender, EventArgs e)
        {
            hdnActiveStep.Value = "1"; // Force stay on step 1
            if (cmbvendor.SelectedValue == "0") return;

            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Vendor where Vendor_Name='" + cmbvendor.SelectedItem.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lbl_vendordbid.Text = re["Id"].ToString();
                lblvendor_id.Text = re["Vendor_Id"].ToString();
                txtAddress1.Text = re["Address1"].ToString();
                cmbcity.Text = re["City"].ToString();
                cmbState.Text = re["State"].ToString();
                txtEmail.Text = re["Com_email"].ToString();
                txtPhone.Text = re["Com_phone"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void RadioButtonList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindCategories();
            gvProductsToSelect.DataSource = null;
            gvProductsToSelect.DataBind();
            hdnActiveStep.Value = "2"; // Keep user on step 2
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
            hdnActiveStep.Value = "2"; // Ensure Wizard stays on step 2

            if (cmbproduct_service.SelectedValue == "0")
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

            gvProductsToSelect.DataSource = dt;
            gvProductsToSelect.DataBind();

            DbCL.Conn.Close();
        }

        // ADD MULTIPLE SELECTED ITEMS FROM GRID
        protected void Button2_Click(object sender, EventArgs e)
        {
            hdnActiveStep.Value = "2"; // Keep wizard on step 2 so user sees success message
            SyncGridToViewState(); // Save existing user inputs in Step 3 Grid

            DataTable dtItems = GridData;
            int addedCount = 0;

            foreach (GridViewRow row in gvProductsToSelect.Rows)
            {
                CheckBox chkSelect = (CheckBox)row.FindControl("chkSelect");
                if (chkSelect != null && chkSelect.Checked)
                {
                    string itemId = gvProductsToSelect.DataKeys[row.RowIndex].Value.ToString();
                    string itemName = Server.HtmlDecode(row.Cells[2].Text); // Decode html entities if any

                    // Prevent duplicates
                    if (!dtItems.AsEnumerable().Any(r => r.Field<string>("Ser_pro_code") == itemId))
                    {
                        DataRow newRow = dtItems.NewRow();
                        newRow["Ser_pro_code"] = itemId;
                        newRow["Ser_pro_Name"] = itemName;
                        newRow["Description"] = "";
                        newRow["Qnty"] = 1;
                        newRow["Rate"] = 0;
                        newRow["DiscountPercent"] = 0;
                        newRow["DiscountAmount"] = 0;
                        newRow["IsTaxApplicable"] = false;
                        newRow["GST"] = 0;
                        newRow["ItemOrder"] = dtItems.Rows.Count + 1;

                        dtItems.Rows.Add(newRow);
                        addedCount++;
                    }

                    // Uncheck so user knows it was processed
                    chkSelect.Checked = false;
                }
            }

            if (addedCount > 0)
            {
                GridData = dtItems;
                gd_Service_Product.DataSource = GridData;
                gd_Service_Product.DataBind();
                ShowSuccess(addedCount + " item(s) added successfully. Click 'Next' to Review.");
            }
            else
            {
                ShowError("No items were selected, or they already exist in your requisition.");
            }
        }

        // Helper method to retain user inputs in Step 3 when postback occurs
        private void SyncGridToViewState()
        {
            DataTable dt = GridData;
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
                    dt.Rows[i]["IsTaxApplicable"] = ((CheckBox)row.FindControl("chkTaxApplicable")).Checked;

                    DropDownList ddlGST = (DropDownList)row.FindControl("vat_parsentage");
                    decimal gst;
                    dt.Rows[i]["GST"] = decimal.TryParse(ddlGST.SelectedValue, out gst) ? gst : 0;
                    dt.Rows[i]["ItemOrder"] = ToInt(row, "txtOrder");
                }
            }
            GridData = dt;
        }

        protected void gd_Service_Product_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DropDownList dp1 = (DropDownList)e.Row.FindControl("vat_parsentage");
                HiddenField hdnSelectedGST = (HiddenField)e.Row.FindControl("hdnSelectedGST");

                if (dp1 != null)
                {
                    dp1.Items.Clear();
                    dp1.Items.AddRange(TaxRates.Select(rate => new ListItem(rate)).ToArray());

                    if (hdnSelectedGST != null && !string.IsNullOrEmpty(hdnSelectedGST.Value))
                    {
                        string val = Convert.ToDecimal(hdnSelectedGST.Value) == 0 ? "NA" : Convert.ToDecimal(hdnSelectedGST.Value).ToString("0.00");
                        if (dp1.Items.FindByValue(val) != null)
                        {
                            dp1.SelectedValue = val;
                        }
                    }
                }
            }
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

                        DbCL.FillCombo(cmbvendor, "select Vendor_Name from tbl_Vendor order by Vendor_Name");
                        if (cmbvendor.Items.FindByText(dr["Vendor"].ToString()) != null)
                        {
                            cmbvendor.ClearSelection();
                            cmbvendor.Items.FindByText(dr["Vendor"].ToString()).Selected = true;
                            cmbvendor_SelectedIndexChanged(null, null);
                        }
                    }
                }

                SqlDataAdapter da = new SqlDataAdapter("SELECT ProductId as Ser_pro_code, ProductName as Ser_pro_Name, Description, Qnty, Rate, DiscountPercent, DiscountAmount, IsTaxApplicable, GST, ItemOrder FROM tbl_RequisitionNew WHERE ReqNo=@ReqNo ORDER BY ItemOrder", con);
                da.SelectCommand.Parameters.AddWithValue("@ReqNo", reqNo);

                DataTable dt = new DataTable();
                da.Fill(dt);
                GridData = dt;

                gd_Service_Product.DataSource = GridData;
                gd_Service_Product.DataBind();
            }

            ApplyStatusUI(lblStatus.Text);
        }

        protected void btnSaveDraft_Click(object sender, EventArgs e)
        {
            hdnActiveStep.Value = "3"; // Stay on step 3 on save
            ClearMessages();
            string userId = Session["USERID"]?.ToString();
            if (string.IsNullOrEmpty(userId)) { ShowError("Session expired."); return; }

            SyncGridToViewState();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();
                try
                {
                    if (string.IsNullOrEmpty(CurrentReqNo))
                    {
                        SqlCommand cmdHdr = new SqlCommand("sp_Requisition_CreateDraft", con, tran);
                        cmdHdr.CommandType = CommandType.StoredProcedure;
                        cmdHdr.Parameters.AddWithValue("@ClientName", cmbvendor.SelectedItem.Text);
                        cmdHdr.Parameters.AddWithValue("@VendorId", Convert.ToInt32(lbl_vendordbid.Text));
                        cmdHdr.Parameters.AddWithValue("@CreatedBy", userId);
                        SqlParameter outReq = new SqlParameter("@ReqNo", SqlDbType.VarChar, 250) { Direction = ParameterDirection.Output };
                        cmdHdr.Parameters.Add(outReq);
                        cmdHdr.ExecuteNonQuery();

                        CurrentReqNo = outReq.Value.ToString();
                        lblReqNo.Text = CurrentReqNo;
                    }

                    DataTable tvpDt = new DataTable();
                    tvpDt.Columns.Add("ProductId", typeof(string));
                    tvpDt.Columns.Add("ProductName", typeof(string));
                    tvpDt.Columns.Add("ParentCategoryId", typeof(int));
                    tvpDt.Columns.Add("Description", typeof(string));
                    tvpDt.Columns.Add("Qnty", typeof(decimal));
                    tvpDt.Columns.Add("Rate", typeof(decimal));
                    tvpDt.Columns.Add("DiscountPercent", typeof(decimal));
                    tvpDt.Columns.Add("DiscountAmount", typeof(decimal));
                    tvpDt.Columns.Add("IsTaxApplicable", typeof(bool));
                    tvpDt.Columns.Add("GST", typeof(decimal));
                    tvpDt.Columns.Add("ItemOrder", typeof(int));

                    foreach (DataRow row in GridData.Rows)
                    {
                        int parentId = 0;
                        if (cmbproduct_service.SelectedValue != "0" && !string.IsNullOrEmpty(cmbproduct_service.SelectedValue))
                            parentId = Convert.ToInt32(cmbproduct_service.SelectedValue);

                        tvpDt.Rows.Add(
                            row["Ser_pro_code"], row["Ser_pro_Name"], parentId,
                            row["Description"], row["Qnty"], row["Rate"], row["DiscountPercent"], row["DiscountAmount"],
                            row["IsTaxApplicable"], row["GST"], row["ItemOrder"]
                        );
                    }

                    SqlCommand cmd = new SqlCommand("sp_RequisitionItem_BulkUpsert", con, tran);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ClientName", cmbvendor.SelectedItem.Text);
                    cmd.Parameters.AddWithValue("@ReqNo", CurrentReqNo);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    SqlParameter tvp = cmd.Parameters.AddWithValue("@Items", tvpDt);
                    tvp.SqlDbType = SqlDbType.Structured;
                    tvp.TypeName = "dbo.RequisitionItem_TVP";

                    cmd.ExecuteNonQuery();
                    tran.Commit();

                    lblStatus.Text = "Draft";
                    ShowSuccess("Draft saved successfully.");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ShowError("Save failed: " + ex.Message);
                }
            }
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            hdnActiveStep.Value = "3";
            btnSaveDraft_Click(sender, e);

            if (string.IsNullOrEmpty(CurrentReqNo)) return;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_SubmitRequisition", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ReqNo", CurrentReqNo);
                cmd.Parameters.AddWithValue("@UserId", Session["USERID"].ToString());
                con.Open();
                cmd.ExecuteNonQuery();
            }

            ApplyStatusUI("Submitted");
            ShowSuccess("PR submitted successfully for approval.");
        }

        protected void btnCancelPR_Click(object sender, EventArgs e)
        {
            hdnActiveStep.Value = "3";
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

        private void ApplyStatusUI(string status)
        {
            lblStatus.Text = status;
            bool isDraft = (status == "Draft");
            btnSaveDraft.Enabled = isDraft;
            Button3.Enabled = isDraft;
            btnCancelPR.Visible = isDraft;
            gd_Service_Product.Enabled = isDraft;
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
        }

        private decimal? ToDecimal(GridViewRow row, string controlId)
        {
            TextBox txt = row.FindControl(controlId) as TextBox;
            if (txt == null || string.IsNullOrWhiteSpace(txt.Text)) return null;
            decimal val;
            return decimal.TryParse(txt.Text.Trim(), out val) ? val : (decimal?)null;
        }

        private int ToInt(GridViewRow row, string controlId)
        {
            TextBox txt = row.FindControl(controlId) as TextBox;
            if (txt == null || string.IsNullOrWhiteSpace(txt.Text)) return 0;
            int val;
            return int.TryParse(txt.Text.Trim(), out val) ? val : 0;
        }
    }
}