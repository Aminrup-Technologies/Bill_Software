using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Web.Services;

namespace Bill_Software.corporate.business.app
{
    public partial class View_PR_Details : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();

        private enum PageMode { Draft, View, Approve }

        private class ProductImages
        {
            public string Top;
            public string Bottom;
            public string Left;
            public string Right;
            public string Oem;
        }

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
                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT Vendor_Name FROM tbl_Vendor WHERE CompanyID = @CompanyID ORDER BY Vendor_Name", con))
                    {
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        con.Open();
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            cmbvendor.DataSource = rdr;
                            cmbvendor.DataTextField = "Vendor_Name";
                            cmbvendor.DataValueField = "Vendor_Name";
                            cmbvendor.DataBind();
                        }
                    }
                    cmbvendor.Items.Insert(0, new ListItem("--Select Vendor--", "0"));
                    LoadPR(reqNo);
                    ApplyModeUI();
                }
            }
        }

        protected void btnBackToList_Click(object sender, EventArgs e)
        {
            if (CurrentMode == PageMode.Approve)
                Response.Redirect("Approve_PR.aspx");
            else
                Response.Redirect("View_PR.aspx");
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
            int companyId = CompanyContext.CurrentCompanyID;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                con.Open();
                SqlCommand cmdHdr = new SqlCommand(
                    "SELECT * FROM tbl_RequisitionMain WHERE ReqNo=@ReqNo AND CompanyID=@CompanyID", con);
                cmdHdr.Parameters.AddWithValue("@ReqNo", reqNo);
                cmdHdr.Parameters.AddWithValue("@CompanyID", companyId);

                using (SqlDataReader dr = cmdHdr.ExecuteReader())
                {
                    if (!dr.Read())
                    {
                        ShowError("PR not found for this company.");
                        return;
                    }

                    lblReqNo.Text = reqNo;
                    lblStatus.Text = dr["Status"].ToString();
                    string vendorId = dr["VendorId"].ToString();
                    BindVendor(vendorId);
                }

                SqlCommand cmdItems = new SqlCommand(
                    @"SELECT id, ProductId AS Ser_pro_code, ProductName as Ser_pro_Name, ParentCategoryId, Description,
                             Qnty, Rate, DiscountPercent, DiscountAmount, TaxableAmount, IsTaxApplicable, gstrate, ItemOrder
                      FROM tbl_RequisitionNew
                      WHERE ReqNo = @ReqNo AND CompanyID = @CompanyID
                      ORDER BY ItemOrder", con);
                cmdItems.Parameters.AddWithValue("@ReqNo", reqNo);
                cmdItems.Parameters.AddWithValue("@CompanyID", companyId);

                SqlDataAdapter da = new SqlDataAdapter(cmdItems);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (!dt.Columns.Contains("IsModified")) dt.Columns.Add("IsModified", typeof(bool));
                if (!dt.Columns.Contains("HSN")) dt.Columns.Add("HSN", typeof(string));
                if (!dt.Columns.Contains("IsProduct")) dt.Columns.Add("IsProduct", typeof(int));
                foreach (DataRow r in dt.Rows)
                {
                    r["IsModified"] = false;
                    string code = r["Ser_pro_code"].ToString();
                    r["HSN"] = LookupHsn(code);
                    r["IsProduct"] = ProductExistsForCompany(code) ? 1 : 0;
                }

                PRItems = dt;
                BindGridFromViewState();

                if (lblStatus.Text == "Draft" && dt.Rows.Count > 0 && dt.Rows[0]["ParentCategoryId"] != DBNull.Value)
                {
                    string parentCatId = dt.Rows[0]["ParentCategoryId"].ToString();

                    bool isProductCat = true;
                    using (SqlCommand cmdCat = new SqlCommand(
                        "SELECT COUNT(1) FROM tbl_NewparentProduct WHERE Id = @Id AND CompanyID = @CompanyID", con))
                    {
                        cmdCat.Parameters.AddWithValue("@Id", parentCatId);
                        cmdCat.Parameters.AddWithValue("@CompanyID", companyId);
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
            BindVendorByName(cmbvendor.SelectedItem.Text);
        }

        protected void BindVendor(String VendorId)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT Id, Vendor_Id, Vendor_Name, Address1, City, State, Com_email, Com_phone FROM tbl_Vendor WHERE Id = @Id AND CompanyID = @CompanyID", con))
            {
                cmd.Parameters.AddWithValue("@Id", VendorId);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                con.Open();
                using (SqlDataReader re = cmd.ExecuteReader())
                {
                    if (re.Read())
                    {
                        ApplyVendorReader(re);
                    }
                }
            }
        }

        private void BindVendorByName(string vendorName)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT Id, Vendor_Id, Vendor_Name, Address1, City, State, Com_email, Com_phone FROM tbl_Vendor WHERE Vendor_Name = @VendorName AND CompanyID = @CompanyID", con))
            {
                cmd.Parameters.AddWithValue("@VendorName", vendorName);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                con.Open();
                using (SqlDataReader re = cmd.ExecuteReader())
                {
                    if (re.Read())
                    {
                        ApplyVendorReader(re);
                    }
                }
            }
        }

        private void ApplyVendorReader(SqlDataReader re)
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

        protected void RadioButtonList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindCategories();
            gvProductsToSelect.DataSource = null;
            gvProductsToSelect.DataBind();
            hdnActiveStep.Value = "2";
        }

        private void BindCategories()
        {
            string cmdstring = RadioButtonList1.SelectedValue == "Product"
                ? "SELECT Id, ProductOrServiceCat as CategoryName FROM tbl_NewparentProduct WHERE CompanyID = @CompanyID ORDER BY ProductOrServiceCat"
                : "SELECT Id, Service_name as CategoryName FROM tbl_Service ORDER BY Service_name";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(cmdstring, con))
            {
                if (RadioButtonList1.SelectedValue == "Product")
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    cmbproduct_service.DataSource = dt;
                    cmbproduct_service.DataTextField = "CategoryName";
                    cmbproduct_service.DataValueField = "Id";
                    cmbproduct_service.DataBind();
                }
            }
            cmbproduct_service.Items.Insert(0, new ListItem("--Select Category--", "0"));
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

            string cmdstring = RadioButtonList1.SelectedValue == "Product"
                ? @"SELECT ProductID as ItemId, ProductName as ItemName,
                          ISNULL(Product_code,'') AS HSN,
                          ISNULL(ProductOrServiceCat,'') AS Category,
                          ISNULL(Type,'') AS Type,
                          ISNULL(Brand,'') AS Brand,
                          ISNULL(Unit,'') AS Unit,
                          ISNULL(Sail_Rate,'') AS Sail_Rate,
                          ISNULL(Purches_Rate,'') AS Purches_Rate,
                          ISNULL(Tax_Rate,'') AS Tax_Rate,
                          ISNULL(Quantity,0) AS Quantity,
                          ISNULL(MOQ_Value,0) AS MOQ_Value,
                          ISNULL(SaleNote,'') AS SaleNote,
                          ISNULL(Product_catagory,'') AS Remarks,
                          ISNULL(Specification,'') AS Specification,
                          ISNULL(ImageUrl,'') AS ImageUrl,
                          ExpiryDate,
                          1 AS IsProduct
                   FROM tbl_NewProduct
                   WHERE ParentId = @ParentId AND CompanyID = @CompanyID
                     AND (DeleteMode = 0 OR DeleteMode IS NULL)
                   ORDER BY ProductName"
                : @"SELECT Service_code as ItemId, Service_name as ItemName,
                          '' AS HSN, '' AS Category, 'Service' AS Type, '' AS Brand, '' AS Unit,
                          '' AS Sail_Rate, '' AS Purches_Rate, '' AS Tax_Rate,
                          0 AS Quantity, 0 AS MOQ_Value, '' AS SaleNote, '' AS Remarks,
                          '' AS Specification, '' AS ImageUrl, NULL AS ExpiryDate,
                          0 AS IsProduct
                   FROM tbl_Service WHERE Id = @ParentId";

            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(cmdstring, con))
            {
                cmd.Parameters.AddWithValue("@ParentId", Convert.ToInt32(cmbproduct_service.SelectedValue));
                if (RadioButtonList1.SelectedValue == "Product")
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            EnrichProductRows(dt);

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
                    dtFiltered.ImportRow(row);
            }

            gvProductsToSelect.DataSource = dtFiltered;
            gvProductsToSelect.DataBind();
        }

        private void EnrichProductRows(DataTable dt)
        {
            if (!dt.Columns.Contains("ExpiryText")) dt.Columns.Add("ExpiryText", typeof(string));
            if (!dt.Columns.Contains("OemUrl")) dt.Columns.Add("OemUrl", typeof(string));
            if (!dt.Columns.Contains("ImgTop")) dt.Columns.Add("ImgTop", typeof(string));
            if (!dt.Columns.Contains("ImgBottom")) dt.Columns.Add("ImgBottom", typeof(string));
            if (!dt.Columns.Contains("ImgLeft")) dt.Columns.Add("ImgLeft", typeof(string));
            if (!dt.Columns.Contains("ImgRight")) dt.Columns.Add("ImgRight", typeof(string));

            foreach (DataRow row in dt.Rows)
            {
                if (row["ExpiryDate"] != DBNull.Value)
                {
                    DateTime exp;
                    row["ExpiryText"] = DateTime.TryParse(Convert.ToString(row["ExpiryDate"]), out exp)
                        ? exp.ToString("dd-MMM-yyyy") : Convert.ToString(row["ExpiryDate"]);
                }
                else row["ExpiryText"] = "";

                ProductImages imgs = ParseProductImages(row["ImageUrl"] == DBNull.Value ? "" : Convert.ToString(row["ImageUrl"]));
                row["OemUrl"] = imgs.Oem ?? "";
                row["ImgTop"] = ResolveImg(imgs.Top);
                row["ImgBottom"] = ResolveImg(imgs.Bottom);
                row["ImgLeft"] = ResolveImg(imgs.Left);
                row["ImgRight"] = ResolveImg(imgs.Right);
            }
        }

        private string ResolveImg(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return path;
            return ResolveUrl(path.StartsWith("~/") ? path : "~/" + path.TrimStart('/'));
        }

        private static ProductImages ParseProductImages(string raw)
        {
            var p = new ProductImages();
            if (string.IsNullOrWhiteSpace(raw)) return p;
            string s = raw.Trim();
            bool packed = s.IndexOf('|') >= 0
                || s.StartsWith("T=", StringComparison.OrdinalIgnoreCase)
                || s.StartsWith("B=", StringComparison.OrdinalIgnoreCase)
                || s.StartsWith("L=", StringComparison.OrdinalIgnoreCase)
                || s.StartsWith("R=", StringComparison.OrdinalIgnoreCase)
                || s.StartsWith("O=", StringComparison.OrdinalIgnoreCase);
            if (packed)
            {
                string[] parts = s.Split('|');
                for (int i = 0; i < parts.Length; i++)
                {
                    string part = parts[i];
                    int eq = part.IndexOf('=');
                    if (eq <= 0) continue;
                    string key = part.Substring(0, eq).Trim().ToUpperInvariant();
                    string val = part.Substring(eq + 1).Trim();
                    if (key == "T") p.Top = val;
                    else if (key == "B") p.Bottom = val;
                    else if (key == "L") p.Left = val;
                    else if (key == "R") p.Right = val;
                    else if (key == "O") p.Oem = val;
                }
                return p;
            }
            if (s.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || s.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                p.Oem = s;
            else p.Top = s;
            return p;
        }

        private string LookupHsn(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return "";
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT TOP 1 ISNULL(Product_code,'') FROM tbl_NewProduct WHERE ProductID = @ProductID AND CompanyID = @CompanyID", con))
            {
                cmd.Parameters.AddWithValue("@ProductID", productId);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                con.Open();
                object o = cmd.ExecuteScalar();
                return o == null || o == DBNull.Value ? "" : o.ToString();
            }
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            hdnActiveStep.Value = "2";
            SyncGridToViewState();

            DataTable dtItems = PRItems;
            if (!dtItems.Columns.Contains("HSN")) dtItems.Columns.Add("HSN", typeof(string));
            if (!dtItems.Columns.Contains("IsProduct")) dtItems.Columns.Add("IsProduct", typeof(int));
            int addedCount = 0;
            bool isProduct = RadioButtonList1.SelectedValue == "Product";

            foreach (GridViewRow row in gvProductsToSelect.Rows)
            {
                CheckBox chkSelect = (CheckBox)row.FindControl("chkSelect");
                if (chkSelect != null && chkSelect.Checked)
                {
                    string itemId = gvProductsToSelect.DataKeys[row.RowIndex].Value.ToString();
                    Label lblName = (Label)row.FindControl("lblItemName");
                    Label lblHsn = (Label)row.FindControl("lblHsn");
                    string itemName = lblName != null ? lblName.Text : "";
                    string hsn = lblHsn != null ? lblHsn.Text : "";

                    if (!dtItems.AsEnumerable().Any(r => r.Field<string>("Ser_pro_code") == itemId))
                    {
                        DataRow newRow = dtItems.NewRow();
                        newRow["id"] = 0;
                        newRow["Ser_pro_code"] = itemId;
                        newRow["Ser_pro_Name"] = itemName;
                        newRow["ParentCategoryId"] = Convert.ToInt32(cmbproduct_service.SelectedValue);
                        newRow["HSN"] = hsn;
                        newRow["Description"] = "";
                        newRow["Qnty"] = 1m;
                        newRow["Rate"] = 0;
                        newRow["DiscountPercent"] = 0;
                        newRow["DiscountAmount"] = 0;
                        newRow["TaxableAmount"] = 0;
                        newRow["IsTaxApplicable"] = false;
                        newRow["gstrate"] = 0;
                        newRow["ItemOrder"] = dtItems.Rows.Count + 1;
                        newRow["IsModified"] = true;
                        newRow["IsProduct"] = isProduct ? 1 : 0;

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
                PopulateProductGrid();
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

                CheckBox chkTax = (CheckBox)e.Row.FindControl("chkTaxApplicable");
                if (chkTax != null)
                    chkTax.InputAttributes["class"] = "tax-check";

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
                    SqlCommand cmd = new SqlCommand(
                        "DELETE FROM tbl_RequisitionNew WHERE id=@id AND CompanyID=@CompanyID AND ReqNo=@ReqNo", con);
                    cmd.Parameters.AddWithValue("@id", rowId);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    cmd.Parameters.AddWithValue("@ReqNo", CurrentReqNo);
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

            SaveModifications(false);
        }

        private DataTable BuildRequisitionItemTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ProductId", typeof(string));
            dt.Columns.Add("ProductName", typeof(string));
            dt.Columns.Add("ParentCategoryId", typeof(int));
            dt.Columns.Add("HSNCode", typeof(string));
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

        private bool ValidateBeforePersist(bool isSubmit, out string error)
        {
            error = null;
            if (Session["USERID"] == null) { error = "Session expired."; return false; }
            if (CompanyContext.CurrentCompanyID <= 0) { error = "Company context missing."; return false; }
            if (cmbvendor.SelectedValue == "0" || string.IsNullOrEmpty(lbl_vendordbid.Text))
            { error = "Select a vendor."; return false; }
            if (PRItems.Rows.Count == 0) { error = "Add at least one product."; return false; }

            foreach (DataRow row in PRItems.Rows)
            {
                decimal qty = row["Qnty"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Qnty"]);
                if (qty <= 0) { error = "Quantity must be greater than zero."; return false; }

                if (!isSubmit) continue;

                decimal rate = row["Rate"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Rate"]);
                bool tax = row["IsTaxApplicable"] != DBNull.Value && Convert.ToBoolean(row["IsTaxApplicable"]);
                decimal gst = row["gstrate"] == DBNull.Value ? 0 : Convert.ToDecimal(row["gstrate"]);
                if (rate <= 0) { error = "Rate must be greater than zero."; return false; }
                if (!tax) { error = "Tax Applicable must be checked for all items."; return false; }
                if (gst <= 0) { error = "Please select a GST percentage for all items."; return false; }
            }
            return true;
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            if (lblStatus.Text != "Draft") { ShowError("This PR can no longer be submitted."); return; }
            hdnActiveStep.Value = "3";
            ClearMessages();

            bool saveSuccess = SaveModifications(true);

            if (!saveSuccess) return;

            if (string.IsNullOrEmpty(CurrentReqNo)) return;

            int companyId = CompanyContext.CurrentCompanyID;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();
                try
                {
                    UpdatePRTotals_OnSubmit(con, tran, CurrentReqNo);

                    SqlCommand cmd = new SqlCommand("sp_SubmitRequisition", con, tran);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ReqNo", CurrentReqNo);
                    cmd.Parameters.AddWithValue("@UserId", Session["USERID"].ToString());
                    cmd.Parameters.AddWithValue("@CompanyID", companyId);
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

        private bool SaveModifications(bool isSubmit)
        {
            string userId = Session["USERID"]?.ToString();
            if (string.IsNullOrEmpty(userId)) { ShowError("Session expired."); return false; }

            SyncGridToViewState();

            string err;
            if (!ValidateBeforePersist(isSubmit, out err)) { ShowError(err); return false; }

            bool anyModified = false;
            foreach (DataRow row in PRItems.Rows)
            {
                if (row["IsModified"] != DBNull.Value && Convert.ToBoolean(row["IsModified"]))
                {
                    anyModified = true;
                    break;
                }
            }

            if (!anyModified)
            {
                if (!isSubmit)
                {
                    ShowError("No modified rows to save.");
                }
                return true;
            }

            DataTable dt = BuildRequisitionItemTable();
            foreach (DataRow row in PRItems.Rows)
            {
                string hsnVal = "";
                if (PRItems.Columns.Contains("HSN") && row["HSN"] != DBNull.Value)
                    hsnVal = Convert.ToString(row["HSN"]);
                object hsn = string.IsNullOrWhiteSpace(hsnVal) ? (object)DBNull.Value : hsnVal;

                dt.Rows.Add(
                    row["Ser_pro_code"], row["Ser_pro_Name"], row["ParentCategoryId"],
                    hsn,
                    row["Description"], row["Qnty"], row["Rate"], row["DiscountPercent"], row["DiscountAmount"],
                    row["IsTaxApplicable"], row["gstrate"], row["ItemOrder"]
                );
            }

            int companyId = CompanyContext.CurrentCompanyID;

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
                    cmd.Parameters.AddWithValue("@CompanyID", companyId);

                    SqlParameter tvp = cmd.Parameters.AddWithValue("@Items", dt);
                    tvp.SqlDbType = SqlDbType.Structured;
                    tvp.TypeName = "dbo.RequisitionItem_TVP";

                    cmd.ExecuteNonQuery();
                    tran.Commit();

                    if (!isSubmit)
                    {
                        ShowSuccess("Modified items saved successfully.");
                    }

                    LoadPR(CurrentReqNo);
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
            btnCancelPR.Enabled = isDraft;

            btnSaveDraft.Visible = isDraft;
            Button3.Visible = isDraft;
            btnCancelPR.Visible = isDraft;

            gd_Service_Product.Enabled = isDraft;
            gvProductsToSelect.Enabled = isDraft;
            Button2.Enabled = isDraft;
            Button2.Visible = isDraft;
            SearchBox_Row.Visible = isDraft;

            if (!isDraft)
                MakeReadOnly();
        }

        protected void btnCancelPR_Click(object sender, EventArgs e)
        {
            if (lblStatus.Text != "Draft")
            {
                MakeReadOnly();
                ApplyStatusUI(lblStatus.Text);
                Response.Redirect("View_PR.aspx");
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_CancelRequisition", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ReqNo", CurrentReqNo);
                    cmd.Parameters.AddWithValue("@CancelledBy", Session["USERID"].ToString());
                    cmd.Parameters.AddWithValue("@CancelReason", "Cancelled by user");
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                MakeReadOnly();
                ApplyStatusUI("Cancelled");
                Response.Redirect("View_PR.aspx");
            }
            catch (Exception ex)
            {
                ShowError("Cancel failed: " + ex.Message);
            }
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
                FROM tbl_RequisitionNew WHERE ReqNo = @ReqNo AND CompanyID = @CompanyID;", con);

                cmd.Parameters.AddWithValue("@ReqNo", reqNo);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
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
                FROM tbl_RequisitionNew WHERE ReqNo = @ReqNo AND CompanyID = @CompanyID", con, tran);

            cmdCalc.Parameters.Add("@ReqNo", SqlDbType.VarChar, 50).Value = reqNo;
            cmdCalc.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
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
                WHERE ReqNo = @ReqNo AND CompanyID = @CompanyID", con, tran);

            cmdUpdate.Parameters.AddWithValue("@Gross", gross);
            cmdUpdate.Parameters.AddWithValue("@Discount", discount);
            cmdUpdate.Parameters.AddWithValue("@Taxable", taxable);
            cmdUpdate.Parameters.AddWithValue("@GST", gst);
            cmdUpdate.Parameters.AddWithValue("@Net", taxable + gst);
            cmdUpdate.Parameters.AddWithValue("@ReqNo", reqNo);
            cmdUpdate.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
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
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
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
            dt.Columns.Add("HSN", typeof(string));
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
            dt.Columns.Add("IsProduct", typeof(int));
            return dt;
        }

        private bool ProductExistsForCompany(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return false;
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT TOP 1 1 FROM tbl_NewProduct WHERE ProductID = @ProductID AND CompanyID = @CompanyID", con))
            {
                cmd.Parameters.AddWithValue("@ProductID", productId);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                con.Open();
                return cmd.ExecuteScalar() != null;
            }
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
            if (!dt.Columns.Contains("HSN")) dt.Columns.Add("HSN", typeof(string));

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

                    Label lblCartHsn = (Label)row.FindControl("lblCartHsn");
                    if (lblCartHsn != null)
                        dt.Rows[i]["HSN"] = lblCartHsn.Text;

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

        [WebMethod(EnableSession = true)]
        public static object GetProductDetail(string productId)
        {
            if (HttpContext.Current == null || HttpContext.Current.Session == null || HttpContext.Current.Session["USERID"] == null)
                return new { ok = false, message = "Session expired." };
            if (string.IsNullOrWhiteSpace(productId))
                return new { ok = false, message = "Product id required." };

            int companyId = CompanyContext.CurrentCompanyID;
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT TOP 1 ProductID, ProductName, ISNULL(Product_code,'') AS HSN,
                       ISNULL(ProductOrServiceCat,'') AS Category, ISNULL(Type,'') AS Type,
                       ISNULL(Brand,'') AS Brand, ISNULL(Unit,'') AS Unit,
                       ISNULL(Sail_Rate,'') AS Sail_Rate, ISNULL(Purches_Rate,'') AS Purches_Rate,
                       ISNULL(Tax_Rate,'') AS Tax_Rate, ISNULL(Quantity,0) AS Quantity,
                       ISNULL(MOQ_Value,0) AS MOQ_Value, ISNULL(SaleNote,'') AS SaleNote,
                       ISNULL(Product_catagory,'') AS Remarks, ISNULL(Specification,'') AS Specification,
                       ISNULL(ImageUrl,'') AS ImageUrl, ExpiryDate
                FROM tbl_NewProduct
                WHERE ProductID = @ProductID AND CompanyID = @CompanyID
                  AND (DeleteMode = 0 OR DeleteMode IS NULL)", con))
            {
                cmd.Parameters.AddWithValue("@ProductID", productId.Trim());
                cmd.Parameters.AddWithValue("@CompanyID", companyId);
                con.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                        return new { ok = false, message = "Product not found for this company." };

                    string expiry = "";
                    if (r["ExpiryDate"] != DBNull.Value)
                    {
                        DateTime exp;
                        expiry = DateTime.TryParse(Convert.ToString(r["ExpiryDate"]), out exp)
                            ? exp.ToString("dd-MMM-yyyy") : Convert.ToString(r["ExpiryDate"]);
                    }

                    ProductImages imgs = ParseProductImages(r["ImageUrl"] == DBNull.Value ? "" : Convert.ToString(r["ImageUrl"]));
                    return new
                    {
                        ok = true,
                        pid = Convert.ToString(r["ProductID"]),
                        name = Convert.ToString(r["ProductName"]),
                        hsn = Convert.ToString(r["HSN"]),
                        cat = Convert.ToString(r["Category"]),
                        type = Convert.ToString(r["Type"]),
                        brand = Convert.ToString(r["Brand"]),
                        unit = Convert.ToString(r["Unit"]),
                        srate = Convert.ToString(r["Sail_Rate"]),
                        prate = Convert.ToString(r["Purches_Rate"]),
                        tax = Convert.ToString(r["Tax_Rate"]),
                        qty = Convert.ToString(r["Quantity"]),
                        moq = Convert.ToString(r["MOQ_Value"]),
                        expiry = expiry,
                        salenote = Convert.ToString(r["SaleNote"]),
                        remarks = Convert.ToString(r["Remarks"]),
                        spec = Convert.ToString(r["Specification"]),
                        oem = imgs.Oem ?? "",
                        imgtop = ResolveImgStatic(imgs.Top),
                        imgbottom = ResolveImgStatic(imgs.Bottom),
                        imgleft = ResolveImgStatic(imgs.Left),
                        imgright = ResolveImgStatic(imgs.Right)
                    };
                }
            }
        }

        private static string ResolveImgStatic(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return path;
            string appRel = path.StartsWith("~/") ? path : "~/" + path.TrimStart('/');
            return VirtualPathUtility.ToAbsolute(appRel);
        }
    }
}
