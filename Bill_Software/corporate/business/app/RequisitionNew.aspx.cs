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
    public partial class RequisitionNew : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();

        [Serializable]
        private class CartLineSnap
        {
            public string Name;
            public string HSN;
            public string Description;
            public decimal Rate;
            public decimal DiscountPercent;
            public decimal DiscountAmount;
            public bool IsTaxApplicable;
            public decimal GST;
            public int ItemOrder;
            public int ParentCategoryId;
            public bool IsProduct;
        }

        private class ProductImages
        {
            public string Top;
            public string Bottom;
            public string Left;
            public string Right;
            public string Oem;
        }

        private Dictionary<string, decimal> CartQuantities
        {
            get
            {
                var d = ViewState["CartQty"] as Dictionary<string, decimal>;
                if (d == null) { d = new Dictionary<string, decimal>(); ViewState["CartQty"] = d; }
                return d;
            }
            set { ViewState["CartQty"] = value; }
        }

        private List<string> CartOrder
        {
            get
            {
                var list = ViewState["CartOrder"] as List<string>;
                if (list == null) { list = new List<string>(); ViewState["CartOrder"] = list; }
                return list;
            }
            set { ViewState["CartOrder"] = value; }
        }

        private Dictionary<string, CartLineSnap> CartSnaps
        {
            get
            {
                var d = ViewState["CartSnap"] as Dictionary<string, CartLineSnap>;
                if (d == null) { d = new Dictionary<string, CartLineSnap>(); ViewState["CartSnap"] = d; }
                return d;
            }
            set { ViewState["CartSnap"] = value; }
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
                return;
            }

            if (!IsPostBack)
            {
                txtPRDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                LoadTaxRates();
                BindCategories();
                SetWizardTabs(0);

                string reqNo = Request.QueryString["reqNo"];
                if (!string.IsNullOrEmpty(reqNo))
                {
                    LoadPR(reqNo);
                }
                else
                {
                    BindVendors();
                }
            }
        }

        private void BindVendors()
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
        }

        private void LoadTaxRates()
        {
            List<string> rates = new List<string> { "NA" };
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT Vat_Rate FROM tbl_Vat_Master", con))
            {
                con.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read()) rates.Add(rdr[0].ToString());
                }
            }
            TaxRates = rates;
        }

        protected void cmbvendor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbvendor.SelectedValue == "0") return;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT Id, Vendor_Id, Address1, City, State, Com_email, Com_phone FROM tbl_Vendor WHERE Vendor_Name = @VendorName AND CompanyID = @CompanyID", con))
            {
                cmd.Parameters.AddWithValue("@VendorName", cmbvendor.SelectedItem.Text);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                con.Open();
                using (SqlDataReader re = cmd.ExecuteReader())
                {
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
                }
            }
        }

        protected void RadioButtonList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindCategories();
            gvProductsToSelect.DataSource = null;
            gvProductsToSelect.DataBind();
            SetWizardTabs(0);
        }

        private void BindCategories()
        {
            string sql = RadioButtonList1.SelectedValue == "Product"
                ? "SELECT Id, ProductOrServiceCat as CategoryName FROM tbl_NewparentProduct WHERE CompanyID = @CompanyID ORDER BY ProductOrServiceCat"
                : "SELECT Id, Service_name as CategoryName FROM tbl_Service ORDER BY Service_name";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
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
            SetWizardTabs(0);
            if (cmbproduct_service.SelectedValue == "0")
            {
                gvProductsToSelect.DataSource = null;
                gvProductsToSelect.DataBind();
                return;
            }

            string sql = RadioButtonList1.SelectedValue == "Product"
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

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@ParentId", Convert.ToInt32(cmbproduct_service.SelectedValue));
                if (RadioButtonList1.SelectedValue == "Product")
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    EnrichProductRows(dt);
                    gvProductsToSelect.DataSource = dt;
                    gvProductsToSelect.DataBind();
                }
            }
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

        protected void btnGoCart_Click(object sender, EventArgs e)
        {
            BindCartGrid();
            mvRequisition.ActiveViewIndex = 1;
            SetWizardTabs(1);
        }

        protected void btnGoSelect_Click(object sender, EventArgs e)
        {
            SyncCartFromGrid();
            mvRequisition.ActiveViewIndex = 0;
            SetWizardTabs(0);
        }

        private void SetWizardTabs(int cartIndex)
        {
            lblTabSelect.CssClass = cartIndex == 0 ? "wizard-step active" : "wizard-step";
            lblTabCart.CssClass = cartIndex == 1 ? "wizard-step active" : "wizard-step";
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            SyncCartFromGrid();
            var qtys = CartQuantities;
            var order = CartOrder;
            var snaps = CartSnaps;
            int addedCount = 0;

            foreach (GridViewRow row in gvProductsToSelect.Rows)
            {
                CheckBox chkSelect = (CheckBox)row.FindControl("chkSelect");
                if (chkSelect == null || !chkSelect.Checked) continue;

                string itemId = gvProductsToSelect.DataKeys[row.RowIndex].Value.ToString();
                Label lblName = (Label)row.FindControl("lblItemName");
                Label lblHsn = (Label)row.FindControl("lblHsn");
                string itemName = lblName != null ? lblName.Text : "";
                string hsn = lblHsn != null ? lblHsn.Text : "";

                if (!qtys.ContainsKey(itemId))
                {
                    int parentId = 0;
                    if (cmbproduct_service.SelectedValue != "0" && !string.IsNullOrEmpty(cmbproduct_service.SelectedValue))
                        parentId = Convert.ToInt32(cmbproduct_service.SelectedValue);
                    bool isProduct = RadioButtonList1.SelectedValue == "Product";

                    qtys[itemId] = 1m;
                    order.Add(itemId);
                    snaps[itemId] = new CartLineSnap
                    {
                        Name = itemName,
                        HSN = hsn,
                        Description = "",
                        Rate = 0,
                        DiscountPercent = 0,
                        DiscountAmount = 0,
                        IsTaxApplicable = false,
                        GST = 0,
                        ItemOrder = order.Count,
                        ParentCategoryId = parentId,
                        IsProduct = isProduct
                    };
                    addedCount++;
                }
                chkSelect.Checked = false;
            }

            CartQuantities = qtys;
            CartOrder = order;
            CartSnaps = snaps;

            if (addedCount > 0)
            {
                BindCartGrid();
                mvRequisition.ActiveViewIndex = 1;
                SetWizardTabs(1);
                ShowSuccess(addedCount + " item(s) added. Review quantities before saving.");
            }
            else
            {
                SetWizardTabs(0);
                ShowError("No items were selected, or they already exist in your requisition.");
            }
        }

        private void SyncCartFromGrid()
        {
            if (gd_Service_Product.Rows.Count == 0) return;

            var qtys = CartQuantities;
            var snaps = CartSnaps;
            var order = new List<string>();

            foreach (GridViewRow row in gd_Service_Product.Rows)
            {
                string code = ((Label)row.FindControl("Ser_pro_code")).Text;
                string name = ((Label)row.FindControl("Ser_pro_Name")).Text;
                decimal qty = ToDecimal(row, "Quantity") ?? 0m;
                if (qty <= 0m) qty = 1m;

                qtys[code] = qty;
                order.Add(code);

                DropDownList ddlGST = (DropDownList)row.FindControl("vat_parsentage");
                decimal gst;
                string existingHsn = "";
                Label lblCartHsn = (Label)row.FindControl("lblCartHsn");
                if (lblCartHsn != null && !string.IsNullOrWhiteSpace(lblCartHsn.Text))
                    existingHsn = lblCartHsn.Text;
                else if (snaps.ContainsKey(code) && !string.IsNullOrEmpty(snaps[code].HSN))
                    existingHsn = snaps[code].HSN;
                else
                    existingHsn = LookupHsn(code);

                int parentId = 0;
                if (snaps.ContainsKey(code) && snaps[code] != null)
                    parentId = snaps[code].ParentCategoryId;
                bool isProduct = snaps.ContainsKey(code) && snaps[code] != null && snaps[code].IsProduct;

                snaps[code] = new CartLineSnap
                {
                    Name = name,
                    HSN = existingHsn,
                    Description = ((TextBox)row.FindControl("sepecification")).Text,
                    Rate = ToDecimal(row, "Vendor_rate") ?? 0,
                    DiscountPercent = ToDecimal(row, "DiscountPercent") ?? 0,
                    DiscountAmount = ToDecimal(row, "DiscountAmount") ?? 0,
                    IsTaxApplicable = ((CheckBox)row.FindControl("chkTaxApplicable")).Checked,
                    GST = decimal.TryParse(ddlGST.SelectedValue, out gst) ? gst : 0,
                    ItemOrder = ToInt(row, "txtOrder"),
                    ParentCategoryId = parentId,
                    IsProduct = isProduct
                };
            }

            CartQuantities = qtys;
            CartOrder = order;
            CartSnaps = snaps;
        }

        private DataTable BuildCartTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Ser_pro_code", typeof(string));
            dt.Columns.Add("Ser_pro_Name", typeof(string));
            dt.Columns.Add("HSN", typeof(string));
            dt.Columns.Add("Description", typeof(string));
            dt.Columns.Add("Qnty", typeof(decimal));
            dt.Columns.Add("Rate", typeof(decimal));
            dt.Columns.Add("DiscountPercent", typeof(decimal));
            dt.Columns.Add("DiscountAmount", typeof(decimal));
            dt.Columns.Add("IsTaxApplicable", typeof(bool));
            dt.Columns.Add("GST", typeof(decimal));
            dt.Columns.Add("ItemOrder", typeof(int));
            dt.Columns.Add("IsProduct", typeof(int));

            var qtys = CartQuantities;
            var snaps = CartSnaps;
            int i = 1;
            foreach (string id in CartOrder)
            {
                if (!qtys.ContainsKey(id)) continue;
                CartLineSnap snap;
                snaps.TryGetValue(id, out snap);
                DataRow r = dt.NewRow();
                r["Ser_pro_code"] = id;
                r["Ser_pro_Name"] = snap != null ? snap.Name : id;
                r["HSN"] = snap != null && !string.IsNullOrEmpty(snap.HSN) ? snap.HSN : LookupHsn(id);
                r["Description"] = snap != null ? snap.Description : "";
                r["Qnty"] = qtys[id];
                r["Rate"] = snap != null ? snap.Rate : 0;
                r["DiscountPercent"] = snap != null ? snap.DiscountPercent : 0;
                r["DiscountAmount"] = snap != null ? snap.DiscountAmount : 0;
                r["IsTaxApplicable"] = snap != null && snap.IsTaxApplicable;
                r["GST"] = snap != null ? snap.GST : 0;
                r["ItemOrder"] = snap != null && snap.ItemOrder > 0 ? snap.ItemOrder : i;
                r["IsProduct"] = snap != null && snap.IsProduct ? 1 : 0;
                dt.Rows.Add(r);
                i++;
            }
            return dt;
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

        private void BindCartGrid()
        {
            gd_Service_Product.DataSource = BuildCartTable();
            gd_Service_Product.DataBind();
        }

        protected void gd_Service_Product_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            CheckBox chkTax = (CheckBox)e.Row.FindControl("chkTaxApplicable");
            if (chkTax != null)
                chkTax.InputAttributes["class"] = "tax-check";

            DropDownList dp1 = (DropDownList)e.Row.FindControl("vat_parsentage");
            HiddenField hdnSelectedGST = (HiddenField)e.Row.FindControl("hdnSelectedGST");
            if (dp1 == null) return;

            dp1.Items.Clear();
            dp1.Items.AddRange(TaxRates.Select(rate => new ListItem(rate)).ToArray());

            if (hdnSelectedGST != null && !string.IsNullOrEmpty(hdnSelectedGST.Value))
            {
                string val = Convert.ToDecimal(hdnSelectedGST.Value) == 0 ? "NA" : Convert.ToDecimal(hdnSelectedGST.Value).ToString("0.00");
                if (dp1.Items.FindByValue(val) != null) dp1.SelectedValue = val;
            }
        }

        protected void gd_Service_Product_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "DeleteItem") return;

            SyncCartFromGrid();
            string itemId = Convert.ToString(e.CommandArgument);
            if (string.IsNullOrEmpty(itemId)) return;

            var qtys = CartQuantities;
            var snaps = CartSnaps;
            var order = CartOrder;

            qtys.Remove(itemId);
            snaps.Remove(itemId);
            order.Remove(itemId);

            int i = 1;
            foreach (string id in order)
            {
                CartLineSnap snap;
                if (snaps.TryGetValue(id, out snap) && snap != null)
                    snap.ItemOrder = i;
                i++;
            }

            CartQuantities = qtys;
            CartSnaps = snaps;
            CartOrder = order;

            BindCartGrid();
            SetWizardTabs(1);
            ClearMessages();
            ShowSuccess("Item removed from cart.");
        }

        private void LoadPR(string reqNo)
        {
            CurrentReqNo = reqNo;
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                con.Open();
                using (SqlCommand cmdHdr = new SqlCommand(
                    "SELECT * FROM tbl_RequisitionMain WHERE ReqNo = @ReqNo AND CompanyID = @CompanyID", con))
                {
                    cmdHdr.Parameters.AddWithValue("@ReqNo", reqNo);
                    cmdHdr.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    using (SqlDataReader dr = cmdHdr.ExecuteReader())
                    {
                        if (!dr.Read())
                        {
                            ShowError("PR not found for this company.");
                            return;
                        }

                        lblReqNo.Text = reqNo;
                        lblStatus.Text = dr["Status"].ToString();
                        if (HasColumn(dr, "Date") && dr["Date"] != DBNull.Value)
                            txtPRDate.Text = Convert.ToDateTime(dr["Date"]).ToString("dd-MMM-yyyy");
                        else if (HasColumn(dr, "CreatedOn") && dr["CreatedOn"] != DBNull.Value)
                            txtPRDate.Text = Convert.ToDateTime(dr["CreatedOn"]).ToString("dd-MMM-yyyy");

                        if (HasColumn(dr, "ExternalERPNo") && dr["ExternalERPNo"] != DBNull.Value)
                            txtExternalPRNo.Text = dr["ExternalERPNo"].ToString();
                        if (HasColumn(dr, "Remarks") && dr["Remarks"] != DBNull.Value)
                            txtRemarks.Text = dr["Remarks"].ToString();

                        string vendorName = dr["Vendor"].ToString();
                        BindVendors();
                        if (cmbvendor.Items.FindByText(vendorName) != null)
                        {
                            cmbvendor.ClearSelection();
                            cmbvendor.Items.FindByText(vendorName).Selected = true;
                        }
                    }
                }

                if (cmbvendor.SelectedValue != "0")
                    cmbvendor_SelectedIndexChanged(null, null);

                using (SqlCommand cmdItems = new SqlCommand(@"
                    SELECT ProductId as Ser_pro_code, ProductName as Ser_pro_Name, ParentCategoryId, Description, Qnty, Rate,
                           DiscountPercent, DiscountAmount, IsTaxApplicable, gstrate AS GST, ItemOrder
                    FROM tbl_RequisitionNew
                    WHERE ReqNo = @ReqNo AND CompanyID = @CompanyID
                    ORDER BY ItemOrder", con))
                {
                    cmdItems.Parameters.AddWithValue("@ReqNo", reqNo);
                    cmdItems.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmdItems))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        RestoreCartFromTable(dt);
                    }
                }
            }

            BindCartGrid();
            mvRequisition.ActiveViewIndex = 1;
            SetWizardTabs(1);
            ApplyStatusUI(lblStatus.Text);
        }

        private static bool HasColumn(SqlDataReader dr, string name)
        {
            for (int i = 0; i < dr.FieldCount; i++)
                if (string.Equals(dr.GetName(i), name, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private void RestoreCartFromTable(DataTable dt)
        {
            var qtys = new Dictionary<string, decimal>();
            var order = new List<string>();
            var snaps = new Dictionary<string, CartLineSnap>();
            foreach (DataRow row in dt.Rows)
            {
                string id = row["Ser_pro_code"].ToString();
                decimal qty = row["Qnty"] == DBNull.Value ? 1m : Convert.ToDecimal(row["Qnty"]);
                if (qty <= 0m) qty = 1m;
                int parentId = 0;
                if (dt.Columns.Contains("ParentCategoryId") && row["ParentCategoryId"] != DBNull.Value)
                    parentId = Convert.ToInt32(row["ParentCategoryId"]);
                string hsn = LookupHsn(id);
                qtys[id] = qty;
                order.Add(id);
                snaps[id] = new CartLineSnap
                {
                    Name = row["Ser_pro_Name"].ToString(),
                    HSN = hsn,
                    Description = row["Description"] == DBNull.Value ? "" : row["Description"].ToString(),
                    Rate = row["Rate"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Rate"]),
                    DiscountPercent = row["DiscountPercent"] == DBNull.Value ? 0 : Convert.ToDecimal(row["DiscountPercent"]),
                    DiscountAmount = row["DiscountAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["DiscountAmount"]),
                    IsTaxApplicable = row["IsTaxApplicable"] != DBNull.Value && Convert.ToBoolean(row["IsTaxApplicable"]),
                    GST = row["GST"] == DBNull.Value ? 0 : Convert.ToDecimal(row["GST"]),
                    ItemOrder = row["ItemOrder"] == DBNull.Value ? order.Count : Convert.ToInt32(row["ItemOrder"]),
                    ParentCategoryId = parentId,
                    IsProduct = ProductExistsForCompany(id)
                };
            }
            CartQuantities = qtys;
            CartOrder = order;
            CartSnaps = snaps;
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

        private bool ValidateBeforePersist(bool submit, out string error)
        {
            error = null;
            if (Session["USERID"] == null) { error = "Session expired."; return false; }
            if (CompanyContext.CurrentCompanyID <= 0) { error = "Company context missing."; return false; }
            if (cmbvendor.SelectedValue == "0" || string.IsNullOrEmpty(lbl_vendordbid.Text))
            { error = "Select a vendor."; return false; }
            if (CartOrder.Count == 0) { error = "Add at least one product."; return false; }

            var qtys = CartQuantities;
            var snaps = CartSnaps;
            foreach (string id in CartOrder)
            {
                decimal qty;
                if (!qtys.TryGetValue(id, out qty) || qty <= 0m)
                { error = "Quantity must be greater than zero."; return false; }

                if (!submit) continue;

                CartLineSnap snap;
                if (!snaps.TryGetValue(id, out snap) || snap == null) continue;
                if (snap.Rate <= 0) { error = "Rate must be greater than zero."; return false; }
                if (!snap.IsTaxApplicable) { error = "Tax Applicable must be checked for all items."; return false; }
                if (snap.GST <= 0) { error = "Please select a GST percentage for all items."; return false; }
            }
            return true;
        }

        protected void btnSaveDraft_Click(object sender, EventArgs e)
        {
            PersistRequisition(submit: false);
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            PersistRequisition(submit: true);
        }

        private void PersistRequisition(bool submit)
        {
            SetWizardTabs(1);
            ClearMessages();
            SyncCartFromGrid();

            string err;
            if (!ValidateBeforePersist(submit, out err)) { ShowError(err); return; }

            string userId = Session["USERID"].ToString();
            int companyId = CompanyContext.CurrentCompanyID;
            DataTable tvpDt = BuildTvpFromCart();
            bool isNewDraft = string.IsNullOrEmpty(CurrentReqNo);

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();
                try
                {
                    if (isNewDraft)
                    {
                        using (SqlCommand cmdHdr = new SqlCommand("sp_Requisition_CreateDraft", con, tran))
                        {
                            cmdHdr.CommandType = CommandType.StoredProcedure;
                            cmdHdr.Parameters.AddWithValue("@ClientName", cmbvendor.SelectedItem.Text);
                            cmdHdr.Parameters.AddWithValue("@VendorId", Convert.ToInt32(lbl_vendordbid.Text));
                            cmdHdr.Parameters.AddWithValue("@CreatedBy", userId);
                            cmdHdr.Parameters.AddWithValue("@CompanyID", companyId);
                            SqlParameter outReq = new SqlParameter("@ReqNo", SqlDbType.VarChar, 250) { Direction = ParameterDirection.Output };
                            cmdHdr.Parameters.Add(outReq);
                            cmdHdr.ExecuteNonQuery();
                            CurrentReqNo = outReq.Value.ToString();
                            lblReqNo.Text = CurrentReqNo;
                        }
                    }
                    else if (!OwnsRequisition(con, tran, CurrentReqNo, companyId))
                    {
                        throw new Exception("PR does not belong to this company.");
                    }

                    using (SqlCommand cmdUpdateHdr = new SqlCommand(@"
                        UPDATE tbl_RequisitionMain
                        SET ExternalERPNo = @ExtERP, Remarks = @Remarks, Date = @Date
                        WHERE ReqNo = @ReqNo AND CompanyID = @CompanyID", con, tran))
                    {
                        cmdUpdateHdr.Parameters.AddWithValue("@ExtERP", txtExternalPRNo.Text.Trim());
                        cmdUpdateHdr.Parameters.AddWithValue("@Remarks", txtRemarks.Text.Trim());
                        cmdUpdateHdr.Parameters.AddWithValue("@Date", txtPRDate.Text.Trim());
                        cmdUpdateHdr.Parameters.AddWithValue("@ReqNo", CurrentReqNo);
                        cmdUpdateHdr.Parameters.AddWithValue("@CompanyID", companyId);
                        cmdUpdateHdr.ExecuteNonQuery();
                    }

                    using (SqlCommand cmd = new SqlCommand("sp_RequisitionItem_BulkUpsert", con, tran))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ClientName", cmbvendor.SelectedItem.Text);
                        cmd.Parameters.AddWithValue("@ReqNo", CurrentReqNo);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@CompanyID", companyId);
                        SqlParameter tvp = cmd.Parameters.AddWithValue("@Items", tvpDt);
                        tvp.SqlDbType = SqlDbType.Structured;
                        tvp.TypeName = "dbo.RequisitionItem_TVP";
                        cmd.ExecuteNonQuery();
                    }

                    if (submit)
                    {
                        using (SqlCommand cmdSubmit = new SqlCommand("sp_SubmitRequisition", con, tran))
                        {
                            cmdSubmit.CommandType = CommandType.StoredProcedure;
                            cmdSubmit.Parameters.AddWithValue("@ReqNo", CurrentReqNo);
                            cmdSubmit.Parameters.AddWithValue("@UserId", userId);
                            cmdSubmit.Parameters.AddWithValue("@CompanyID", companyId);
                            cmdSubmit.ExecuteNonQuery();
                        }
                        InsertSystemNotification("PR Submitted",
                            $"PR {CurrentReqNo} submitted for approval.", "SUCCESS", con, tran);
                        tran.Commit();
                        ApplyStatusUI("Submitted");
                        ShowSuccess("PR submitted successfully for approval.");
                    }
                    else
                    {
                        if (isNewDraft)
                        {
                            InsertSystemNotification("PR Draft Created",
                                $"PR {CurrentReqNo} draft saved.", "INFO", con, tran);
                        }
                        tran.Commit();
                        lblStatus.Text = "Draft";
                        ShowSuccess(isNewDraft ? "Draft saved successfully." : "Draft updated successfully.");
                    }
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    // Ponytail #3: Never expose raw exception details to client
                    ShowError((submit ? "Submit" : "Save") + " failed. Please try again.");
                }
            }
        }

        private DataTable BuildTvpFromCart()
        {
            DataTable tvpDt = new DataTable();
            tvpDt.Columns.Add("ProductId", typeof(string));
            tvpDt.Columns.Add("ProductName", typeof(string));
            tvpDt.Columns.Add("ParentCategoryId", typeof(int));
            tvpDt.Columns.Add("HSNCode", typeof(string));
            tvpDt.Columns.Add("Description", typeof(string));
            tvpDt.Columns.Add("Qnty", typeof(decimal));
            tvpDt.Columns.Add("Rate", typeof(decimal));
            tvpDt.Columns.Add("DiscountPercent", typeof(decimal));
            tvpDt.Columns.Add("DiscountAmount", typeof(decimal));
            tvpDt.Columns.Add("IsTaxApplicable", typeof(bool));
            tvpDt.Columns.Add("GST", typeof(decimal));
            tvpDt.Columns.Add("ItemOrder", typeof(int));

            int parentIdFallback = 0;
            if (cmbproduct_service.SelectedValue != "0" && !string.IsNullOrEmpty(cmbproduct_service.SelectedValue))
                parentIdFallback = Convert.ToInt32(cmbproduct_service.SelectedValue);

            var qtys = CartQuantities;
            var snaps = CartSnaps;
            foreach (string id in CartOrder)
            {
                if (!qtys.ContainsKey(id)) continue;
                CartLineSnap snap;
                snaps.TryGetValue(id, out snap);
                object hsn = snap != null && !string.IsNullOrEmpty(snap.HSN) ? (object)snap.HSN : DBNull.Value;
                object desc = snap != null && snap.Description != null ? (object)snap.Description : DBNull.Value;
                int parentId = snap != null && snap.ParentCategoryId > 0 ? snap.ParentCategoryId : parentIdFallback;
                tvpDt.Rows.Add(
                    id,
                    snap != null ? snap.Name : id,
                    parentId,
                    hsn,
                    desc,
                    qtys[id],
                    snap != null ? snap.Rate : 0m,
                    snap != null ? snap.DiscountPercent : 0m,
                    snap != null ? snap.DiscountAmount : 0m,
                    snap != null && snap.IsTaxApplicable,
                    snap != null ? snap.GST : 0m,
                    snap != null ? snap.ItemOrder : tvpDt.Rows.Count + 1
                );
            }
            return tvpDt;
        }

        private static bool OwnsRequisition(SqlConnection con, SqlTransaction tran, string reqNo, int companyId)
        {
            using (SqlCommand cmd = new SqlCommand(
                "SELECT 1 FROM tbl_RequisitionMain WHERE ReqNo = @ReqNo AND CompanyID = @CompanyID", con, tran))
            {
                cmd.Parameters.AddWithValue("@ReqNo", reqNo);
                cmd.Parameters.AddWithValue("@CompanyID", companyId);
                return cmd.ExecuteScalar() != null;
            }
        }

        protected void btnCancelPR_Click(object sender, EventArgs e)
        {
            SetWizardTabs(1);
            if (Session["USERID"] == null) { ShowError("Session expired."); return; }
            if (string.IsNullOrEmpty(CurrentReqNo)) { ShowError("No PR to cancel."); return; }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();
                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_CancelRequisition", con, tran))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ReqNo", CurrentReqNo);
                        cmd.Parameters.AddWithValue("@CancelledBy", Session["USERID"].ToString());
                        cmd.Parameters.AddWithValue("@CancelReason", "Cancelled by user");
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        cmd.ExecuteNonQuery();
                    }
                    InsertSystemNotification("PR Cancelled",
                        $"PR {CurrentReqNo} cancelled.", "WARNING", con, tran);
                    tran.Commit();
                    ApplyStatusUI("Cancelled");
                    ShowSuccess("PR cancelled successfully.");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    // Ponytail #3: Never expose raw exception details to client
                    ShowError("Cancel failed. Please try again.");
                }
            }
        }

        private void InsertSystemNotification(string title, string message, string severity, SqlConnection conn, SqlTransaction trans)
        {
            string sql = @"INSERT INTO tbl_SystemNotification
                           (Title, Message, ModuleCode, Severity, StartDate, EndDate, IsActive, CreatedBy, CompanyID)
                           VALUES
                           (@Title, @Msg, @Mod, @Severity, GETDATE(), DATEADD(day, 30, GETDATE()), 1, @User, @Comp)";
            using (SqlCommand cmd = new SqlCommand(sql, conn, trans))
            {
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Msg", message);
                cmd.Parameters.AddWithValue("@Mod", "PURCHASE");
                cmd.Parameters.AddWithValue("@Severity", severity);
                cmd.Parameters.AddWithValue("@User", Session["USERID"] != null ? Session["USERID"].ToString() : "System");
                cmd.Parameters.AddWithValue("@Comp", CompanyContext.CurrentCompanyID);
                cmd.ExecuteNonQuery();
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
            Button2.Enabled = isDraft;
            gvProductsToSelect.Enabled = isDraft;
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
