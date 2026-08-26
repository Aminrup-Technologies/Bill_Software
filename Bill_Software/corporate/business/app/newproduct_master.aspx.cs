using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm69 : System.Web.UI.Page
    {
        protected HiddenField hfFormState;
        protected DropDownList ddlFilterType;
        protected DropDownList ddlFilterCategory;
        DB_UTILITY DbCL = new DB_UTILITY();
        string ConnString { get { return ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
                return;
            }
            if (Page.Form != null)
                Page.Form.Enctype = "multipart/form-data";

            if (!IsPostBack)
            {
                BindCategories();
                DbCL.FillCombo(cmbtax, "select Vat_Rate from tbl_New_Vat_Master order by ID");
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                BindProductsGrid();
            }
        }

        protected string FormatImageLink(object imageUrl)
        {
            ProductImages imgs = ParseProductImages(imageUrl == null || imageUrl == DBNull.Value ? string.Empty : Convert.ToString(imageUrl));
            string u = imgs.Oem;
            if (string.IsNullOrEmpty(u) || !(u.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || u.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                return string.Empty;
            string enc = HttpUtility.HtmlAttributeEncode(u);
            return "<div class=\"rate-sub\"><a href=\"" + enc + "\" target=\"_blank\" rel=\"noopener noreferrer\">OEM</a></div>";
        }

        protected string PrimaryImageUrl(object imageUrl)
        {
            ProductImages imgs = ParseProductImages(imageUrl == null || imageUrl == DBNull.Value ? string.Empty : Convert.ToString(imageUrl));
            string u = imgs.PrimaryLocal();
            return string.IsNullOrEmpty(u) ? string.Empty : u;
        }

        protected bool HasPrimaryImage(object imageUrl)
        {
            return !string.IsNullOrEmpty(PrimaryImageUrl(imageUrl));
        }

        protected string ViewImageUrl(object imageUrl, string viewKey)
        {
            ProductImages imgs = ParseProductImages(imageUrl == null || imageUrl == DBNull.Value ? string.Empty : Convert.ToString(imageUrl));
            string path = null;
            if (string.Equals(viewKey, "T", StringComparison.OrdinalIgnoreCase)) path = imgs.Top;
            else if (string.Equals(viewKey, "B", StringComparison.OrdinalIgnoreCase)) path = imgs.Bottom;
            else if (string.Equals(viewKey, "L", StringComparison.OrdinalIgnoreCase)) path = imgs.Left;
            else if (string.Equals(viewKey, "R", StringComparison.OrdinalIgnoreCase)) path = imgs.Right;
            else if (string.Equals(viewKey, "O", StringComparison.OrdinalIgnoreCase)) path = imgs.Oem;
            if (string.IsNullOrEmpty(path)) return string.Empty;
            if (IsHttpUrl(path) || path.StartsWith("~/", StringComparison.Ordinal))
                return HttpUtility.HtmlAttributeEncode(IsHttpUrl(path) ? path : ResolveUrl(path));
            return HttpUtility.HtmlAttributeEncode(ResolveUrl(path));
        }

        protected string FormatExpiry(object expiry)
        {
            if (expiry == null || expiry == DBNull.Value) return string.Empty;
            DateTime dt;
            if (DateTime.TryParse(Convert.ToString(expiry), out dt))
                return dt.ToString("dd-MMM-yyyy");
            return Convert.ToString(expiry);
        }

        protected string FormatAuditTrail(object user, object when)
        {
            string u = user == null || user == DBNull.Value ? "" : Convert.ToString(user).Trim();
            string d = "";
            if (when != null && when != DBNull.Value)
            {
                DateTime dt;
                if (when is DateTime)
                    dt = (DateTime)when;
                else if (!DateTime.TryParse(Convert.ToString(when), out dt))
                    return string.IsNullOrEmpty(u) ? "—" : u;
                d = dt.ToString("dd-MMM-yyyy hh:mm tt");
            }
            if (string.IsNullOrEmpty(u) && string.IsNullOrEmpty(d)) return "—";
            if (string.IsNullOrEmpty(d)) return u;
            if (string.IsNullOrEmpty(u)) return d;
            return u + " · " + d;
        }

        private static string NormalizeKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return value.Trim().ToLowerInvariant();
        }

        private static string Clip(string value, int maxLen)
        {
            if (string.IsNullOrEmpty(value) || maxLen <= 0) return value;
            return value.Length <= maxLen ? value : value.Substring(0, maxLen);
        }

        private static object DbStr(string value)
        {
            return string.IsNullOrEmpty(value) ? (object)DBNull.Value : value;
        }

        private void TryInsertSystemNotification(string title, string message, string severity, SqlConnection conn, SqlTransaction trans)
        {
            try { InsertSystemNotification(title, message, severity, conn, trans); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Notification skipped: " + ex);
            }
        }

        private void BindSharedProductParams(SqlCommand cmd, string productCode, string category, string purchaseRate,
            decimal saleRate, decimal taxRate, string productName, string type, string unit, string brand,
            int parentId, decimal quantity, decimal moq, DateTime? expiryDate, string imagePath,
            bool isNewFlag, bool isFastMoving, string normName)
        {
            // Align with flamex_uat column types to avoid truncation / conversion failures on update.
            cmd.Parameters.Add("@ProductCode", SqlDbType.VarChar, 50).Value = DbStr(Clip(productCode, 50));
            cmd.Parameters.Add("@ProductOrServiceCat", SqlDbType.VarChar, -1).Value = DbStr(category);
            cmd.Parameters.Add("@PurchesRate", SqlDbType.VarChar, -1).Value = DbStr(purchaseRate);
            cmd.Parameters.Add("@SaleRate", SqlDbType.VarChar, -1).Value = saleRate.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
            cmd.Parameters.Add("@TaxRate", SqlDbType.VarChar, -1).Value = taxRate.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
            cmd.Parameters.Add("@Product_catagory", SqlDbType.VarChar, -1).Value = DbStr(txtproducttype.Text != null ? txtproducttype.Text.Trim() : "");
            cmd.Parameters.Add("@ProductName", SqlDbType.VarChar, -1).Value = DbStr(productName);
            cmd.Parameters.Add("@Type", SqlDbType.NVarChar, 250).Value = DbStr(Clip(type, 250));
            cmd.Parameters.Add("@Unit", SqlDbType.NVarChar, -1).Value = DbStr(unit);
            cmd.Parameters.Add("@Brand", SqlDbType.NVarChar, -1).Value = DbStr(brand);
            cmd.Parameters.Add("@ParentId", SqlDbType.Int).Value = parentId != 0 ? (object)parentId : DBNull.Value;
            cmd.Parameters.Add("@Specification", SqlDbType.NVarChar, 100).Value = DbStr(Clip(TextBox1.Text != null ? TextBox1.Text.Trim() : "", 100));
            cmd.Parameters.Add("@Quantity", SqlDbType.NVarChar, 100).Value = quantity.ToString(System.Globalization.CultureInfo.InvariantCulture);
            cmd.Parameters.Add("@QuantityNum", SqlDbType.Decimal).Value = quantity;
            cmd.Parameters["@QuantityNum"].Precision = 18;
            cmd.Parameters["@QuantityNum"].Scale = 3;
            cmd.Parameters.Add("@MOQ_Value", SqlDbType.NVarChar, 100).Value = moq.ToString(System.Globalization.CultureInfo.InvariantCulture);
            cmd.Parameters.Add("@SaleNote", SqlDbType.NVarChar, 100).Value = DbStr(Clip(TextBox4.Text != null ? TextBox4.Text.Trim() : "", 100));
            if (expiryDate.HasValue)
            {
                DateTime e = expiryDate.Value;
                cmd.Parameters.Add("@ExpiryDate", SqlDbType.SmallDateTime).Value = new DateTime(e.Year, e.Month, e.Day, e.Hour, e.Minute, 0);
            }
            else
                cmd.Parameters.Add("@ExpiryDate", SqlDbType.SmallDateTime).Value = DBNull.Value;
            cmd.Parameters.Add("@ImageUrl", SqlDbType.VarChar, 500).Value = DbStr(Clip(imagePath, 500));
            cmd.Parameters.Add("@IsNew", SqlDbType.Bit).Value = isNewFlag;
            cmd.Parameters.Add("@IsFastMoving", SqlDbType.Bit).Value = isFastMoving;
            // NormalizedProductID is a computed column — do not write it.
            cmd.Parameters.Add("@NormName", SqlDbType.VarChar, 500).Value = DbStr(Clip(normName, 500));
        }

        private class ProductImages
        {
            public string Top;
            public string Bottom;
            public string Left;
            public string Right;
            public string Oem;

            public string PrimaryLocal()
            {
                if (IsLocalPath(Top)) return Top;
                if (IsLocalPath(Bottom)) return Bottom;
                if (IsLocalPath(Left)) return Left;
                if (IsLocalPath(Right)) return Right;
                return string.Empty;
            }
        }

        private static bool IsHttpUrl(string u)
        {
            return !string.IsNullOrEmpty(u)
                && (u.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    || u.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsLocalPath(string u)
        {
            return !string.IsNullOrEmpty(u) && !IsHttpUrl(u);
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
            if (IsHttpUrl(s)) p.Oem = s;
            else p.Top = s;
            return p;
        }

        private static string SerializeProductImages(ProductImages p)
        {
            if (p == null) return string.Empty;
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(p.Top)) parts.Add("T=" + p.Top);
            if (!string.IsNullOrEmpty(p.Bottom)) parts.Add("B=" + p.Bottom);
            if (!string.IsNullOrEmpty(p.Left)) parts.Add("L=" + p.Left);
            if (!string.IsNullOrEmpty(p.Right)) parts.Add("R=" + p.Right);
            if (!string.IsNullOrEmpty(p.Oem)) parts.Add("O=" + p.Oem);
            return string.Join("|", parts.ToArray());
        }

        private void BindCategories()
        {
            cmdProduct.Items.Clear();
            cmdProduct.Items.Add(new ListItem("--Select--", ""));
            ddlFilterCategory.Items.Clear();
            ddlFilterCategory.Items.Insert(0, new ListItem("All Categories", "0"));
            using (SqlConnection conn = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT id, ProductOrServiceCat FROM tbl_NewparentProduct WHERE CompanyID=@CompanyID ORDER BY ProductOrServiceCat ASC", conn))
            {
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        string text = rdr["ProductOrServiceCat"].ToString();
                        string val = rdr["id"].ToString();
                        cmdProduct.Items.Add(new ListItem(text, val));
                        ddlFilterCategory.Items.Add(new ListItem(text, val));
                    }
                }
            }
        }

        private void BindProductsGrid()
        {
            int pageSize;
            if (!int.TryParse(ddlPageSize.SelectedValue, out pageSize) || pageSize <= 0)
                pageSize = 10;
            gridProducts.PageSize = pageSize;

            string search = txtGlobalSearch.Text != null ? txtGlobalSearch.Text.Trim() : string.Empty;
            int parentId = 0;
            bool hasParent = ViewState["FilterParentId"] != null && int.TryParse(Convert.ToString(ViewState["FilterParentId"]), out parentId) && parentId > 0;

            string sql = @"SELECT Id, ProductID, Product_code, ProductName, ProductOrServiceCat, Brand, Specification, Product_catagory, Type,
                                  ISNULL(Sail_Rate,0) AS Sail_Rate, ISNULL(Purches_Rate,0) AS Purches_Rate, ISNULL(Tax_Rate,0) AS Tax_Rate, Unit,
                                  ISNULL(Quantity,0) AS Quantity, ISNULL(MOQ_Value,0) AS MOQ_Value, SaleNote, ExpiryDate, ImageUrl,
                                  ISNULL(IsNew,0) AS IsNew, ISNULL(IsFastMoving,0) AS IsFastMoving,
                                  AddedbyUserId, AddedOn, ModifiedByUserId, ModifiedOn
                           FROM tbl_NewProduct
                           WHERE CompanyID=@CompanyID AND (DeleteMode=0 OR DeleteMode IS NULL)";
            if (hasParent)
                sql += " AND parentId=@parentId";
            bool filterType = ddlFilterType.SelectedIndex > 0;
            if (filterType)
                sql += " AND Type=@FilterType";
            bool filterCat = ddlFilterCategory.SelectedIndex > 0;
            if (filterCat)
                sql += " AND ProductOrServiceCat=@FilterCat";
            if (search.Length > 0)
                sql += @" AND (ProductName LIKE '%' + @Search + '%' OR Product_code LIKE '%' + @Search + '%'
                              OR ProductID LIKE '%' + @Search + '%' OR Brand LIKE '%' + @Search + '%'
                              OR ProductOrServiceCat LIKE '%' + @Search + '%')";
            sql += " ORDER BY Id DESC";

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                if (hasParent)
                    cmd.Parameters.AddWithValue("@parentId", parentId);
                if (filterType)
                    cmd.Parameters.AddWithValue("@FilterType", ddlFilterType.SelectedItem.Text);
                if (filterCat)
                    cmd.Parameters.AddWithValue("@FilterCat", ddlFilterCategory.SelectedItem.Text);
                if (search.Length > 0)
                    cmd.Parameters.AddWithValue("@Search", search);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }
            gridProducts.DataSource = dt;
            gridProducts.DataBind();
        }

        private void Binddata() { BindProductsGrid(); }

        private void BinddataByServiceCategory(int ParentId)
        {
            ViewState["FilterParentId"] = ParentId;
            gridProducts.PageIndex = 0;
            BindProductsGrid();
        }

        private string findProductId(SqlConnection conn, SqlTransaction trans)
        {
            string PurID = "PRD01";
            using (SqlCommand com1 = new SqlCommand(
                @"SELECT TOP 1 ProductID FROM tbl_NewProduct
                  WHERE CompanyID=@CompanyID AND ProductID LIKE 'PRD%'
                  ORDER BY Id DESC", conn, trans))
            {
                com1.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                object o = com1.ExecuteScalar();
                if (o != null && o != DBNull.Value)
                {
                    string aa = o.ToString();
                    if (aa.Length > 3)
                    {
                        int k;
                        if (int.TryParse(aa.Substring(3), out k))
                            PurID = "PRD" + (k + 1).ToString().PadLeft(2, '0');
                    }
                }
            }
            return PurID;
        }

        private bool CheckDuplicateProduct(string category, string productName, string productId, int excludeId, SqlConnection conn, SqlTransaction trans)
        {
            string normName = (productName ?? string.Empty).Trim().ToLower();
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT COUNT(1) FROM dbo.tbl_NewProduct
                  WHERE CompanyID=@CompanyID
                    AND (DeleteMode=0 OR DeleteMode IS NULL)
                    AND (@excludeId=0 OR Id<>@excludeId)
                    AND (
                          (LOWER(LTRIM(RTRIM(ProductName))) = @normName AND ProductOrServiceCat = @cat)
                          OR (@productId <> '' AND ProductID = @productId)
                    )", conn, trans))
            {
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                cmd.Parameters.AddWithValue("@cat", category ?? string.Empty);
                cmd.Parameters.AddWithValue("@normName", normName);
                cmd.Parameters.AddWithValue("@productId", productId ?? string.Empty);
                cmd.Parameters.AddWithValue("@excludeId", excludeId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
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
                cmd.Parameters.AddWithValue("@Mod", "PRODUCT_MASTER");
                cmd.Parameters.AddWithValue("@Severity", severity);
                cmd.Parameters.AddWithValue("@User", Session["USERID"] != null ? Session["USERID"].ToString() : "System");
                cmd.Parameters.AddWithValue("@Comp", CompanyContext.CurrentCompanyID);
                cmd.ExecuteNonQuery();
            }
        }

        private void ShowOk(string msg)
        {
            PanelError.Visible = true;
            PanelError.Style["display"] = "none";
            PanelOK.Visible = true;
            PanelOK.Style["display"] = "block";
            lblOk.Text = msg;
        }

        private void ShowErr(string msg)
        {
            PanelOK.Visible = true;
            PanelOK.Style["display"] = "none";
            PanelError.Visible = true;
            PanelError.Style["display"] = "block";
            lblErrorMsg.Text = msg;
        }

        private void ClearEditMode()
        {
            hfEditProductID.Value = "";
            txtProductID.Text = "";
            hfProductImage.Value = "";
            txtOemUrl.Text = "";
            btnSave.Text = "Save";
            pnlAuditTrail.Visible = false;
            lblAuditCreated.Text = "—";
            lblAuditModified.Text = "—";
        }

        private void ApplyFormDefaults()
        {
            ClearEditMode();
            txtSubProductsName.Text = "";
            txtproducttype.Text = "";
            TextBox1.Text = "";
            txtBrand.Text = "";
            txtProductCode.Text = "";
            txtUnit.Text = "";
            TextBox2.Text = "";
            TextBox3.Text = "";
            txtSalerate.Text = "";
            txtPurchaseRate.Text = "";
            TextBox4.Text = "N/A";
            txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            txtOemUrl.Text = "";
            txtGlobalSearch.Text = "";
            chkIsNew.Checked = false;
            chkIsFastMoving.Checked = false;

            if (cmdProduct.Items.Count > 0) { cmdProduct.ClearSelection(); cmdProduct.SelectedIndex = 0; }
            if (ddlProOrSer.Items.Count > 0) { ddlProOrSer.ClearSelection(); ddlProOrSer.SelectedIndex = 0; }
            if (cmbtax.Items.Count > 0) { cmbtax.ClearSelection(); cmbtax.SelectedIndex = 0; }
            if (ddlPageSize.Items.FindByValue("10") != null)
            {
                ddlPageSize.ClearSelection();
                ddlPageSize.Items.FindByValue("10").Selected = true;
            }

            ViewState["FilterParentId"] = null;
            Session.Remove("pid");
            gridProducts.PageIndex = 0;
            PanelOK.Style["display"] = "none";
            PanelError.Style["display"] = "none";
            lblSimilar.Text = "";
            lblDupMessage.Text = "";
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            ApplyFormDefaults();
            BindProductsGrid();
            hfFormState.Value = "expanded";
        }

        private bool TryValidateUpload(FileUpload fu, string label, out string ext)
        {
            ext = null;
            if (fu == null || !fu.HasFile) return true;
            ext = Path.GetExtension(fu.FileName);
            if (string.IsNullOrEmpty(ext))
            {
                ShowErr("Invalid image file for " + label + ".");
                return false;
            }
            ext = ext.ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp")
            {
                ShowErr("Only .jpg, .png, .webp images are allowed (" + label + ").");
                return false;
            }
            if (ext == ".jpeg") ext = ".jpg";
            return true;
        }

        private bool TryValidateProductImages()
        {
            string ext;
            if (!TryValidateUpload(fuImgTop, "Top View", out ext)) return false;
            if (!TryValidateUpload(fuImgBottom, "Bottom View", out ext)) return false;
            if (!TryValidateUpload(fuImgLeft, "Left View", out ext)) return false;
            if (!TryValidateUpload(fuImgRight, "Right View", out ext)) return false;
            return true;
        }

        private string SaveViewImage(FileUpload fu, string token, out string physicalPath)
        {
            physicalPath = null;
            if (fu == null || !fu.HasFile) return null;

            string ext = Path.GetExtension(fu.FileName);
            if (string.IsNullOrEmpty(ext)) return null;
            ext = ext.ToLowerInvariant();
            if (ext == ".jpeg") ext = ".jpg";

            string virtualDir = "~/Uploads/Products/";
            string physicalDir = Server.MapPath(virtualDir);
            if (!Directory.Exists(physicalDir))
                Directory.CreateDirectory(physicalDir);

            string fileName = Guid.NewGuid().ToString("N") + (string.IsNullOrEmpty(token) ? "" : ("_" + token)) + ext;
            physicalPath = Path.Combine(physicalDir, fileName);
            fu.SaveAs(physicalPath);
            return virtualDir + fileName;
        }

        private static void TryDeleteFile(string physicalPath)
        {
            if (string.IsNullOrEmpty(physicalPath)) return;
            try { if (File.Exists(physicalPath)) File.Delete(physicalPath); } catch { }
        }

        private void RegisterViewPreviews(ProductImages imgs)
        {
            if (imgs == null) return;
            var sb = new System.Text.StringBuilder();
            AppendPreviewScript(sb, "imgPrevTop", imgs.Top);
            AppendPreviewScript(sb, "imgPrevBottom", imgs.Bottom);
            AppendPreviewScript(sb, "imgPrevLeft", imgs.Left);
            AppendPreviewScript(sb, "imgPrevRight", imgs.Right);
            if (sb.Length == 0) return;
            ClientScript.RegisterStartupScript(GetType(), "prevImgs", sb.ToString(), true);
        }

        private void AppendPreviewScript(System.Text.StringBuilder sb, string elId, string path)
        {
            if (!IsLocalPath(path)) return;
            string clientUrl = ResolveUrl(path).Replace("\\", "\\\\").Replace("'", "\\'");
            sb.Append("var el=document.getElementById('").Append(elId).Append("');");
            sb.Append("if(el){el.src='").Append(clientUrl).Append("';el.className='img-preview is-on';}");
        }

        protected void FilterGrid_Changed(object sender, EventArgs e)
        {
            hfFormState.Value = "collapsed";
            gridProducts.PageIndex = 0;
            BindProductsGrid();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            hfFormState.Value = "collapsed";
            gridProducts.PageIndex = 0;
            BindProductsGrid();
        }

        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            hfFormState.Value = "collapsed";
            gridProducts.PageIndex = 0;
            BindProductsGrid();
        }

        protected void gridProducts_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            hfFormState.Value = "collapsed";
            gridProducts.PageIndex = e.NewPageIndex;
            BindProductsGrid();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            hfFormState.Value = "expanded";
            if (string.IsNullOrWhiteSpace(txtProductCode.Text))
            {
                ShowErr("Please enter a Product Code.");
                lblSimilar.Text = "";
                return;
            }

            string productCode = txtProductCode.Text.Trim();
            string category = cmdProduct.SelectedItem != null ? (cmdProduct.SelectedItem.Text ?? "").Trim() : "";
            string productName = txtSubProductsName.Text.Trim();
            string type = ddlProOrSer.SelectedItem != null ? ddlProOrSer.SelectedItem.Text : "";
            string unit = txtUnit.Text.Trim();
            string brand = txtBrand.Text.Trim();
            string externalImageUrl = txtOemUrl.Text != null ? txtOemUrl.Text.Trim() : "";
            int parentId = 0;
            if (!string.IsNullOrEmpty(cmdProduct.SelectedValue))
                int.TryParse(cmdProduct.SelectedValue, out parentId);
            string userId = Session["USERID"] != null ? Session["USERID"].ToString() : "0";
            int companyId = CompanyContext.CurrentCompanyID;

            decimal saleRate = 0m;
            decimal taxRate = 0m;
            decimal quantity = 0m;
            decimal moq = 0m;
            Decimal.TryParse(txtSalerate.Text, out saleRate);
            if (cmbtax.SelectedItem != null)
                Decimal.TryParse(cmbtax.SelectedItem.Text, out taxRate);
            Decimal.TryParse(TextBox2.Text, out quantity);
            Decimal.TryParse(TextBox3.Text, out moq);
            string purchaseRate = txtPurchaseRate.Text != null ? txtPurchaseRate.Text.Trim() : "";
            bool isNewFlag = chkIsNew.Checked;
            bool isFastMoving = chkIsFastMoving.Checked;

            DateTime? expiryDate = null;
            DateTime tmpDt;
            if (DateTime.TryParse(txtfromDate.Text, out tmpDt))
                expiryDate = tmpDt;

            lblSimilar.Text = "";
            bool isEdit = !string.IsNullOrEmpty(hfEditProductID.Value);
            int editId = 0;
            if (isEdit && !int.TryParse(hfEditProductID.Value, out editId))
            {
                ShowErr("Invalid product edit state.");
                return;
            }

            if (!TryValidateProductImages())
                return;

            ProductImages imgs = ParseProductImages(hfProductImage.Value ?? "");
            var newImagePhysicals = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    using (SqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            string productid = isEdit ? (txtProductID.Text ?? "").Trim() : findProductId(conn, trans);
                            string normName = NormalizeKey(productName);
                            if (CheckDuplicateProduct(category, productName, productid, editId, conn, trans))
                            {
                                trans.Rollback();
                                ShowErr("A product with the same name or Product ID already exists for this company.");
                                return;
                            }

                            List<string> similarNames = FindSimilarProductNames(conn, trans, category, productName, editId, 10);
                            if (!isEdit && similarNames.Count > 0)
                            {
                                trans.Rollback();
                                string preview = string.Join("; ", similarNames);
                                lblSimilar.Text = preview;
                                ShowErr("Similar product(s) already exist. Change the name to continue. Matches: " + preview);
                                return;
                            }

                            string phys;
                            string saved;
                            if (fuImgTop.HasFile)
                            {
                                saved = SaveViewImage(fuImgTop, "TopView", out phys);
                                if (saved == null) { trans.Rollback(); return; }
                                imgs.Top = saved;
                                if (!string.IsNullOrEmpty(phys)) newImagePhysicals.Add(phys);
                            }
                            if (fuImgBottom.HasFile)
                            {
                                saved = SaveViewImage(fuImgBottom, "BottomView", out phys);
                                if (saved == null) { trans.Rollback(); foreach (string f in newImagePhysicals) TryDeleteFile(f); return; }
                                imgs.Bottom = saved;
                                if (!string.IsNullOrEmpty(phys)) newImagePhysicals.Add(phys);
                            }
                            if (fuImgLeft.HasFile)
                            {
                                saved = SaveViewImage(fuImgLeft, "LeftView", out phys);
                                if (saved == null) { trans.Rollback(); foreach (string f in newImagePhysicals) TryDeleteFile(f); return; }
                                imgs.Left = saved;
                                if (!string.IsNullOrEmpty(phys)) newImagePhysicals.Add(phys);
                            }
                            if (fuImgRight.HasFile)
                            {
                                saved = SaveViewImage(fuImgRight, "RightView", out phys);
                                if (saved == null) { trans.Rollback(); foreach (string f in newImagePhysicals) TryDeleteFile(f); return; }
                                imgs.Right = saved;
                                if (!string.IsNullOrEmpty(phys)) newImagePhysicals.Add(phys);
                            }

                            if (!string.IsNullOrEmpty(externalImageUrl))
                            {
                                if (!(externalImageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                                    || externalImageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                                    || externalImageUrl.StartsWith("~/", StringComparison.Ordinal)))
                                {
                                    trans.Rollback();
                                    foreach (string f in newImagePhysicals) TryDeleteFile(f);
                                    ShowErr("OEM Reference URL must start with http://, https://, or ~/.");
                                    return;
                                }
                                imgs.Oem = externalImageUrl;
                            }
                            else
                            {
                                imgs.Oem = "";
                            }

                            string imagePath = SerializeProductImages(imgs);
                            if (imagePath.Length > 500)
                            {
                                trans.Rollback();
                                foreach (string f in newImagePhysicals) TryDeleteFile(f);
                                ShowErr("Image data exceeds storage limit. Use shorter OEM URL or fewer/smaller path images.");
                                return;
                            }

                            if (hfEditProductID.Value == "")
                            {
                                string queryNewProduct = @"INSERT INTO tbl_NewProduct
                                    (Product_code, ProductOrServiceCat, Purches_Rate, Sail_Rate, Tax_Rate, Product_catagory, ProductName, Type, [Unit], Brand, parentId, Specification, Quantity, Quantity_Num, MOQ_Value, SaleNote, ExpiryDate, TimeStamp, ProductID, AddedbyUserId, AddedOn, CompanyID, ViewMode, DeleteMode, ImageUrl, IsNew, IsFastMoving, NormalizedProductName)
                                    VALUES
                                    (@ProductCode, @ProductOrServiceCat, @PurchesRate, @SaleRate, @TaxRate, @Product_catagory, @ProductName, @Type, @Unit, @Brand, @ParentId, @Specification, @Quantity, @QuantityNum, @MOQ_Value, @SaleNote, @ExpiryDate, GETDATE(), @ProductID, @AddedbyUserId, GETDATE(), @CompanyID, 1, 0, @ImageUrl, @IsNew, @IsFastMoving, @NormName);
                                    SELECT SCOPE_IDENTITY();";

                                int newId = 0;
                                using (SqlCommand cmdNewProduct = new SqlCommand(queryNewProduct, conn, trans))
                                {
                                    BindSharedProductParams(cmdNewProduct, productCode, category, purchaseRate, saleRate, taxRate,
                                        productName, type, unit, brand, parentId, quantity, moq, expiryDate, imagePath,
                                        isNewFlag, isFastMoving, normName);
                                    cmdNewProduct.Parameters.Add("@ProductID", SqlDbType.NVarChar, 250).Value = DbStr(Clip(productid, 250));
                                    cmdNewProduct.Parameters.Add("@AddedbyUserId", SqlDbType.VarChar, 50).Value = DbStr(Clip(userId, 50));
                                    cmdNewProduct.Parameters.Add("@CompanyID", SqlDbType.Int).Value = companyId;

                                    object scopeObj = cmdNewProduct.ExecuteScalar();
                                    if (scopeObj != null && scopeObj != DBNull.Value)
                                    {
                                        decimal scopeDec;
                                        if (decimal.TryParse(scopeObj.ToString(), out scopeDec))
                                            newId = Convert.ToInt32(scopeDec);
                                    }
                                }

                                string queryStock = @"INSERT INTO tbl_stock (Product_id, Product_name, Quantity, Sail_Rate, Service_tax_rate, ShippedToStoreId, ShippedToStoreName)
                                                      VALUES (@ProductID, @ProductName, @Quantity, @SaleRate, @TaxRate, @ShippedToStoreId, @ShippedToStoreName);";
                                using (SqlCommand cmdStock = new SqlCommand(queryStock, conn, trans))
                                {
                                    cmdStock.Parameters.Add("@ProductID", SqlDbType.VarChar, 100).Value = productid ?? (object)DBNull.Value;
                                    cmdStock.Parameters.Add("@ProductName", SqlDbType.NVarChar, 400).Value = productName ?? (object)DBNull.Value;
                                    cmdStock.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = quantity;
                                    cmdStock.Parameters["@Quantity"].Scale = 3;
                                    cmdStock.Parameters["@Quantity"].Precision = 18;
                                    cmdStock.Parameters.Add("@SaleRate", SqlDbType.Decimal).Value = saleRate;
                                    cmdStock.Parameters["@SaleRate"].Scale = 2;
                                    cmdStock.Parameters["@SaleRate"].Precision = 18;
                                    cmdStock.Parameters.Add("@TaxRate", SqlDbType.Decimal).Value = taxRate;
                                    cmdStock.Parameters["@TaxRate"].Scale = 2;
                                    cmdStock.Parameters["@TaxRate"].Precision = 5;
                                    cmdStock.Parameters.Add("@ShippedToStoreId", SqlDbType.VarChar, 50).Value = "STR001";
                                    cmdStock.Parameters.Add("@ShippedToStoreName", SqlDbType.VarChar, 200).Value = "Central Warehouse";
                                    cmdStock.ExecuteNonQuery();
                                }

                                TryInsertSystemNotification(
                                    "Product Created",
                                    "New Product '" + productName + "' added with HSN " + productCode + ".",
                                    "Success",
                                    conn, trans);

                                trans.Commit();
                                ApplyFormDefaults();
                                ShowOk("New product created (Id=" + newId + "). Opening stock recorded.");
                            }
                            else
                            {
                                // Id and ProductID are immutable — never updated.
                                string updateQuery = @"UPDATE tbl_NewProduct SET
                                    Product_code=@ProductCode, ProductOrServiceCat=@ProductOrServiceCat, Purches_Rate=@PurchesRate,
                                    Sail_Rate=@SaleRate, Tax_Rate=@TaxRate, Product_catagory=@Product_catagory, ProductName=@ProductName,
                                    Type=@Type, [Unit]=@Unit, Brand=@Brand, parentId=@ParentId,
                                    Specification=@Specification, Quantity=@Quantity, Quantity_Num=@QuantityNum, MOQ_Value=@MOQ_Value,
                                    SaleNote=@SaleNote, ExpiryDate=@ExpiryDate, ImageUrl=@ImageUrl,
                                    IsNew=@IsNew, IsFastMoving=@IsFastMoving,
                                    NormalizedProductName=@NormName,
                                    ModifiedByUserId=@ModifiedByUserId, ModifiedOn=GETDATE()
                                    WHERE Id=@Id AND CompanyID=@CompanyID";

                                int affected;
                                using (SqlCommand cmdUpd = new SqlCommand(updateQuery, conn, trans))
                                {
                                    BindSharedProductParams(cmdUpd, productCode, category, purchaseRate, saleRate, taxRate,
                                        productName, type, unit, brand, parentId, quantity, moq, expiryDate, imagePath,
                                        isNewFlag, isFastMoving, normName);
                                    cmdUpd.Parameters.Add("@ModifiedByUserId", SqlDbType.VarChar, 50).Value = DbStr(Clip(userId, 50));
                                    cmdUpd.Parameters.Add("@Id", SqlDbType.Int).Value = editId;
                                    cmdUpd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = companyId;
                                    affected = cmdUpd.ExecuteNonQuery();
                                }

                                if (affected == 0)
                                {
                                    trans.Rollback();
                                    ShowErr("Product not found for this company.");
                                    return;
                                }

                                TryInsertSystemNotification(
                                    "Product Updated",
                                    "Product '" + productName + "' specifications or pricing updated.",
                                    "Information",
                                    conn, trans);

                                trans.Commit();
                                ApplyFormDefaults();
                                ShowOk("Product updated successfully.");
                            }
                        }
                        catch (SqlException sqlex)
                        {
                            try { trans.Rollback(); } catch { }
                            foreach (string f in newImagePhysicals) TryDeleteFile(f);
                            System.Diagnostics.Debug.WriteLine(sqlex.ToString());
                            if (sqlex.Number == 2627 || sqlex.Number == 2601)
                                ShowErr("A product with the same name was created by someone else just now. Please refresh and try again.");
                            else if (sqlex.Number == 8152)
                                ShowErr("One or more fields are too long for the database (Specification / Sale Note max 100 chars). Shorten and retry.");
                            else
                                ShowErr("Save failed: " + Clip(sqlex.Message, 180));
                            return;
                        }
                        catch (Exception exTrans)
                        {
                            try { trans.Rollback(); } catch { }
                            foreach (string f in newImagePhysicals) TryDeleteFile(f);
                            System.Diagnostics.Debug.WriteLine(exTrans.ToString());
                            ShowErr("Save failed: " + Clip(exTrans.Message, 180));
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                foreach (string f in newImagePhysicals) TryDeleteFile(f);
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                ShowErr("Save failed: " + Clip(ex.Message, 180));
            }

            BindProductsGrid();
            hfFormState.Value = "expanded";
        }

        public class NameAvailabilityResult
        {
            public bool checkedOk { get; set; }
            public bool isDuplicate { get; set; }
            public bool hasSimilar { get; set; }
            public List<string> similar { get; set; }
        }

        private static List<string> FindSimilarProductNames(SqlConnection conn, SqlTransaction trans, string category, string productName, int excludeId, int take)
        {
            var list = new List<string>();
            string trimmed = (productName ?? string.Empty).Trim();
            if (trimmed.Length < 3 || string.IsNullOrWhiteSpace(category) || category == "--Select--")
                return list;

            string sql = @"
                SELECT TOP(@take) ProductName, ProductID
                FROM dbo.tbl_NewProduct
                WHERE CompanyID=@CompanyID
                  AND (DeleteMode=0 OR DeleteMode IS NULL)
                  AND (@excludeId=0 OR Id<>@excludeId)
                  AND ProductOrServiceCat=@cat
                  AND ProductName LIKE @like
                ORDER BY
                  CASE WHEN LOWER(LTRIM(RTRIM(ProductName))) = @normName THEN 0 ELSE 1 END,
                  ProductName";

            using (SqlCommand cmd = trans != null
                ? new SqlCommand(sql, conn, trans)
                : new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@take", take);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                cmd.Parameters.AddWithValue("@excludeId", excludeId);
                cmd.Parameters.AddWithValue("@cat", category.Trim());
                cmd.Parameters.AddWithValue("@like", "%" + trimmed + "%");
                cmd.Parameters.AddWithValue("@normName", trimmed.ToLower());
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        string name = rdr["ProductName"] != DBNull.Value ? rdr["ProductName"].ToString() : "";
                        string pid = rdr["ProductID"] != DBNull.Value ? rdr["ProductID"].ToString() : "";
                        if (string.IsNullOrEmpty(name)) continue;
                        list.Add(string.IsNullOrEmpty(pid) ? name : (name + " [" + pid + "]"));
                    }
                }
            }
            return list;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static NameAvailabilityResult CheckDuplicateName(string productName, string category, int excludeId)
        {
            var result = new NameAvailabilityResult
            {
                checkedOk = false,
                isDuplicate = false,
                hasSimilar = false,
                similar = new List<string>()
            };
            try
            {
                if (string.IsNullOrWhiteSpace(productName) || string.IsNullOrWhiteSpace(category) || category == "--Select--")
                    return result;

                string normName = productName.Trim().ToLower();
                string cs = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(cs))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(
                        @"SELECT COUNT(1) FROM dbo.tbl_NewProduct
                          WHERE CompanyID=@CompanyID
                            AND (DeleteMode=0 OR DeleteMode IS NULL)
                            AND (@excludeId=0 OR Id<>@excludeId)
                            AND LOWER(LTRIM(RTRIM(ProductName)))=@normName
                            AND ProductOrServiceCat=@cat", conn))
                    {
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        cmd.Parameters.AddWithValue("@cat", category.Trim());
                        cmd.Parameters.AddWithValue("@normName", normName);
                        cmd.Parameters.AddWithValue("@excludeId", excludeId);
                        result.isDuplicate = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }

                    result.similar = FindSimilarProductNames(conn, null, category, productName, excludeId, 15);
                    result.hasSimilar = result.similar.Count > 0;
                    result.checkedOk = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return result;
            }
        }

        public class DuplicateInfoResult
        {
            public bool foundExact { get; set; }
            public int existingId { get; set; }
            public string productID { get; set; }
            public List<string> similar { get; set; }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static DuplicateInfoResult GetDuplicateInfo(string productName, string category)
        {
            var result = new DuplicateInfoResult { foundExact = false, existingId = 0, productID = null, similar = new List<string>() };
            if (string.IsNullOrWhiteSpace(productName) || string.IsNullOrWhiteSpace(category) || category == "--Select--")
                return result;

            string cs = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            string normName = productName.Trim().ToLower();
            int companyId = CompanyContext.CurrentCompanyID;

            using (SqlConnection conn = new SqlConnection(cs))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT TOP(1) Id, ProductID, ProductName
                        FROM dbo.tbl_NewProduct
                        WHERE CompanyID=@CompanyID
                          AND ProductOrServiceCat=@cat
                          AND LOWER(LTRIM(RTRIM(ProductName)))=@normName
                          AND (DeleteMode=0 OR DeleteMode IS NULL)";
                    cmd.Parameters.AddWithValue("@CompanyID", companyId);
                    cmd.Parameters.AddWithValue("@cat", category);
                    cmd.Parameters.AddWithValue("@normName", normName);
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            result.foundExact = true;
                            result.existingId = rdr["Id"] != DBNull.Value ? Convert.ToInt32(rdr["Id"]) : 0;
                            result.productID = rdr["ProductID"] != DBNull.Value ? rdr["ProductID"].ToString() : null;
                        }
                    }
                }

                using (SqlCommand cmd2 = conn.CreateCommand())
                {
                    cmd2.CommandText = @"
                        SELECT TOP(10) ProductName
                        FROM dbo.tbl_NewProduct
                        WHERE CompanyID=@CompanyID
                          AND ProductOrServiceCat=@cat
                          AND ProductName LIKE @like
                          AND (DeleteMode=0 OR DeleteMode IS NULL)
                        ORDER BY ProductName";
                    cmd2.Parameters.AddWithValue("@CompanyID", companyId);
                    cmd2.Parameters.AddWithValue("@cat", category);
                    cmd2.Parameters.AddWithValue("@like", "%" + productName + "%");
                    using (SqlDataReader rdr2 = cmd2.ExecuteReader())
                    {
                        while (rdr2.Read())
                            result.similar.Add(rdr2["ProductName"].ToString());
                    }
                }
            }
            return result;
        }

        private void LoadProductForEdit(int idVal)
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT Id, ProductID, Product_code, ProductOrServiceCat, Purches_Rate, Sail_Rate, Tax_Rate, Product_catagory, ProductName, Type, Unit, Brand,
                         parentId, Specification, Quantity, MOQ_Value, SaleNote, ExpiryDate, ImageUrl,
                         ISNULL(IsNew,0) AS IsNew, ISNULL(IsFastMoving,0) AS IsFastMoving,
                         AddedbyUserId, AddedOn, ModifiedByUserId, ModifiedOn
                  FROM tbl_NewProduct WHERE Id=@Id AND CompanyID=@CompanyID AND (DeleteMode=0 OR DeleteMode IS NULL)", conn))
            {
                cmd.Parameters.AddWithValue("@Id", idVal);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                conn.Open();
                using (SqlDataReader re = cmd.ExecuteReader())
                {
                    if (!re.Read())
                    {
                        ShowErr("Product not found for this company.");
                        return;
                    }

                    hfEditProductID.Value = re["Id"].ToString();
                    txtProductID.Text = re["ProductID"] != DBNull.Value ? re["ProductID"].ToString() : "";
                    txtProductID.ReadOnly = true;
                    txtProductCode.Text = re["Product_code"] != DBNull.Value ? re["Product_code"].ToString() : "";
                    txtSubProductsName.Text = re["ProductName"] != DBNull.Value ? re["ProductName"].ToString() : "";
                    txtBrand.Text = re["Brand"] != DBNull.Value ? re["Brand"].ToString() : "";
                    txtUnit.Text = re["Unit"] != DBNull.Value ? re["Unit"].ToString() : "";
                    txtSalerate.Text = re["Sail_Rate"] != DBNull.Value ? re["Sail_Rate"].ToString() : "";
                    txtPurchaseRate.Text = re["Purches_Rate"] != DBNull.Value ? re["Purches_Rate"].ToString() : "";
                    txtproducttype.Text = re["Product_catagory"] != DBNull.Value ? re["Product_catagory"].ToString() : "";
                    TextBox1.Text = re["Specification"] != DBNull.Value ? re["Specification"].ToString() : "";
                    TextBox2.Text = re["Quantity"] != DBNull.Value ? re["Quantity"].ToString() : "";
                    TextBox3.Text = re["MOQ_Value"] != DBNull.Value ? re["MOQ_Value"].ToString() : "";
                    TextBox4.Text = re["SaleNote"] != DBNull.Value ? re["SaleNote"].ToString() : "N/A";
                    chkIsNew.Checked = re["IsNew"] != DBNull.Value && Convert.ToBoolean(re["IsNew"]);
                    chkIsFastMoving.Checked = re["IsFastMoving"] != DBNull.Value && Convert.ToBoolean(re["IsFastMoving"]);
                    string imgPath = re["ImageUrl"] != DBNull.Value ? re["ImageUrl"].ToString() : "";
                    hfProductImage.Value = imgPath;
                    ProductImages imgs = ParseProductImages(imgPath);
                    txtOemUrl.Text = imgs.Oem ?? "";
                    RegisterViewPreviews(imgs);

                    lblAuditCreated.Text = FormatAuditTrail(re["AddedbyUserId"], re["AddedOn"]);
                    lblAuditModified.Text = FormatAuditTrail(re["ModifiedByUserId"], re["ModifiedOn"]);
                    pnlAuditTrail.Visible = true;

                    string cat = re["ProductOrServiceCat"] != DBNull.Value ? re["ProductOrServiceCat"].ToString() : "";
                    string typ = re["Type"] != DBNull.Value ? re["Type"].ToString() : "";
                    string tax = re["Tax_Rate"] != DBNull.Value ? re["Tax_Rate"].ToString() : "";
                    string pid = re["parentId"] != DBNull.Value ? re["parentId"].ToString() : "";

                    DateTime expiryDate = re["ExpiryDate"] == DBNull.Value ? DateTime.Today : Convert.ToDateTime(re["ExpiryDate"]);
                    txtfromDate.Text = expiryDate.ToString("dd-MMM-yyyy");

                    if (!string.IsNullOrEmpty(pid))
                    {
                        ListItem li = cmdProduct.Items.FindByValue(pid);
                        if (li != null) cmdProduct.ClearSelection();
                        if (li != null) li.Selected = true;
                        else
                        {
                            ListItem byText = cmdProduct.Items.FindByText(cat);
                            if (byText != null) { cmdProduct.ClearSelection(); byText.Selected = true; }
                        }
                        Session["pid"] = pid;
                    }
                    else if (!string.IsNullOrEmpty(cat))
                    {
                        ListItem byText = cmdProduct.Items.FindByText(cat);
                        if (byText != null) { cmdProduct.ClearSelection(); byText.Selected = true; }
                    }

                    if (!string.IsNullOrEmpty(typ))
                    {
                        ListItem tLi = ddlProOrSer.Items.FindByText(typ);
                        if (tLi != null) { ddlProOrSer.ClearSelection(); tLi.Selected = true; }
                    }
                    if (!string.IsNullOrEmpty(tax))
                    {
                        ListItem taxLi = cmbtax.Items.FindByText(tax);
                        if (taxLi == null) taxLi = cmbtax.Items.FindByValue(tax);
                        if (taxLi != null) { cmbtax.ClearSelection(); taxLi.Selected = true; }
                    }

                    btnSave.Text = "Update Product";
                    ShowOk("Editing product Id=" + idVal + " (Product ID locked). Update and save.");
                    hfFormState.Value = "expanded";
                }
            }
        }

        protected void btnModalEdit_Click(object sender, EventArgs e)
        {
            HandleProductCommand("EditProduct", hfModalEditId.Value);
            hfModalEditId.Value = "";
            ClientScript.RegisterStartupScript(GetType(), "scrollEditForm",
                "try{var el=document.querySelector('.page-header')||document.getElementById('" + txtSubProductsName.ClientID + "');if(el)el.scrollIntoView({behavior:'smooth',block:'start'});}catch(ex){}", true);
        }

        protected void gridProducts_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            HandleProductCommand(e.CommandName, Convert.ToString(e.CommandArgument));
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            HandleProductCommand(e.CommandName, Convert.ToString(e.CommandArgument));
        }

        private void HandleProductCommand(string commandName, string Id)
        {
            if (commandName == "EditProduct" || commandName == "Edit")
            {
                int idVal;
                if (!int.TryParse(Id, out idVal))
                {
                    ShowErr("Invalid product id.");
                    return;
                }
                LoadProductForEdit(idVal);
                BindProductsGrid();
                return;
            }

            if (commandName != "DeleteProduct" && commandName != "Delete")
            {
                BindProductsGrid();
                return;
            }

            int delId;
            if (!int.TryParse(Id, out delId))
            {
                ShowErr("Invalid product id.");
                return;
            }

            string userId = Session["USERID"] != null ? Session["USERID"].ToString() : "System";

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    using (SqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            string prodName = "";
                            using (SqlCommand cmdLookup = new SqlCommand(
                                "SELECT ProductName FROM tbl_NewProduct WHERE Id=@Id AND CompanyID=@CompanyID", conn, trans))
                            {
                                cmdLookup.Parameters.AddWithValue("@Id", delId);
                                cmdLookup.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                                object o = cmdLookup.ExecuteScalar();
                                if (o == null || o == DBNull.Value)
                                {
                                    trans.Rollback();
                                    ShowErr("Product not found for this company.");
                                    return;
                                }
                                prodName = o.ToString();
                            }

                            int affected;
                            using (SqlCommand cmd = new SqlCommand(
                                @"UPDATE tbl_NewProduct
                                  SET DeleteMode=1, DeletedOn=GETDATE(), DeletedByUserId=@UserId,
                                      ModifiedByUserId=@UserId, ModifiedOn=GETDATE()
                                  WHERE Id=@Id AND CompanyID=@CompanyID AND (DeleteMode=0 OR DeleteMode IS NULL)", conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@UserId", userId);
                                cmd.Parameters.AddWithValue("@Id", delId);
                                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                                affected = cmd.ExecuteNonQuery();
                            }

                            if (affected == 0)
                            {
                                trans.Rollback();
                                ShowErr("Product not found or already deactivated.");
                                return;
                            }

                            TryInsertSystemNotification(
                                "Product Soft-Deleted",
                                "Product '" + prodName + "' was soft-deleted (deactivated).",
                                "Warning",
                                conn, trans);

                            trans.Commit();
                            ShowOk("Product soft-deleted successfully. It is hidden from the catalog.");
                        }
                        catch (Exception ex)
                        {
                            try { trans.Rollback(); } catch { }
                            System.Diagnostics.Debug.WriteLine(ex.ToString());
                            ShowErr("An error occurred while deactivating the product. Please try again.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                ShowErr("An error occurred while deactivating the product. Please try again.");
            }

            BindProductsGrid();
        }

        protected void cmdProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            int pid;
            if (string.IsNullOrEmpty(cmdProduct.SelectedValue))
            {
                ViewState["FilterParentId"] = null;
                Session.Remove("pid");
                gridProducts.PageIndex = 0;
                BindProductsGrid();
                return;
            }

            if (int.TryParse(cmdProduct.SelectedValue, out pid))
            {
                Session["pid"] = pid;
                BinddataByServiceCategory(pid);
                return;
            }

            ViewState["FilterParentId"] = null;
            Session.Remove("pid");
            using (SqlConnection conn = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT id FROM tbl_NewparentProduct WHERE ProductOrServiceCat=@ProductOrServiceCat AND CompanyID=@CompanyID", conn))
            {
                cmd.Parameters.AddWithValue("@ProductOrServiceCat", cmdProduct.SelectedItem != null ? cmdProduct.SelectedItem.Text : cmdProduct.Text);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                conn.Open();
                object o = cmd.ExecuteScalar();
                if (o != null && o != DBNull.Value)
                {
                    pid = Convert.ToInt32(o);
                    Session["pid"] = pid;
                    BinddataByServiceCategory(pid);
                }
                else
                {
                    gridProducts.PageIndex = 0;
                    BindProductsGrid();
                }
            }
        }
    }
}
