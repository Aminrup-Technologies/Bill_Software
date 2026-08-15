using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm69 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        string ConnString { get { return ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
                return;
            }
            if (!IsPostBack)
            {
                BindCategories();
                DbCL.FillCombo(cmbtax, "select Vat_Rate from tbl_New_Vat_Master order by ID");
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                BindProductsGrid();
            }
        }

        private void BindCategories()
        {
            cmdProduct.Items.Clear();
            cmdProduct.Items.Add(new ListItem("--Select--", ""));
            using (SqlConnection conn = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT id, ProductOrServiceCat FROM tbl_NewparentProduct WHERE CompanyID=@CompanyID ORDER BY ProductOrServiceCat ASC", conn))
            {
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                        cmdProduct.Items.Add(new ListItem(rdr["ProductOrServiceCat"].ToString(), rdr["id"].ToString()));
                }
            }
        }

        private void BindProductsGrid()
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT Id, ProductID, Product_code, ProductName, ProductOrServiceCat, Brand, Specification, Product_catagory, Type,
                         ISNULL(Sail_Rate,0) AS Sail_Rate, ISNULL(Purches_Rate,0) AS Purches_Rate, ISNULL(Tax_Rate,0) AS Tax_Rate, Unit
                  FROM tbl_NewProduct
                  WHERE CompanyID=@CompanyID AND (DeleteMode=0 OR DeleteMode IS NULL)
                  ORDER BY Id DESC", conn))
            {
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    DataList1.DataSource = rdr;
                    DataList1.DataBind();
                }
            }
        }

        private void Binddata() { BindProductsGrid(); }

        private void BinddataByServiceCategory(int ParentId)
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT Id, ProductID, Product_code, ProductName, ProductOrServiceCat, Brand, Specification, Product_catagory, Type,
                         ISNULL(Sail_Rate,0) AS Sail_Rate, ISNULL(Purches_Rate,0) AS Purches_Rate, ISNULL(Tax_Rate,0) AS Tax_Rate, Unit
                  FROM tbl_NewProduct
                  WHERE parentId=@parentId AND CompanyID=@CompanyID AND (DeleteMode=0 OR DeleteMode IS NULL)
                  ORDER BY Id ASC", conn))
            {
                cmd.Parameters.AddWithValue("@parentId", ParentId);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    DataList1.DataSource = rdr;
                    DataList1.DataBind();
                }
            }
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
            btnSave.Text = "Save";
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
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
            int parentId = 0;
            if (cmdProduct.SelectedValue != null && cmdProduct.SelectedValue != "")
                int.TryParse(cmdProduct.SelectedValue, out parentId);
            if (parentId == 0 && Session["pid"] != null)
                int.TryParse(Session["pid"].ToString(), out parentId);
            string userId = Session["USERID"] != null ? Session["USERID"].ToString() : "0";
            int companyId = CompanyContext.CurrentCompanyID;

            decimal saleRate = 0m;
            decimal taxRate = 0m;
            int quantity = 0;
            int moq = 0;
            Decimal.TryParse(txtSalerate.Text, out saleRate);
            if (cmbtax.SelectedItem != null)
                Decimal.TryParse(cmbtax.SelectedItem.Text, out taxRate);
            Int32.TryParse(TextBox2.Text, out quantity);
            Int32.TryParse(TextBox3.Text, out moq);

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
                            if (CheckDuplicateProduct(category, productName, productid, editId, conn, trans))
                            {
                                trans.Rollback();
                                ShowErr("A product with the same name or Product ID already exists for this company.");
                                return;
                            }

                            if (hfEditProductID.Value == "")
                            {
                                string queryNewProduct = @"INSERT INTO tbl_NewProduct
                                    (Product_code, ProductOrServiceCat, Sail_Rate, Tax_Rate, Product_catagory, ProductName, Type, [Unit], Brand, parentId, Specification, Quantity, MOQ_Value, SaleNote, ExpiryDate, TimeStamp, ProductID, AddedbyUserId, AddedOn, CompanyID, ViewMode, DeleteMode)
                                    VALUES
                                    (@ProductCode, @ProductOrServiceCat, @SaleRate, @TaxRate, @Product_catagory, @ProductName, @Type, @Unit, @Brand, @ParentId, @Specification, @Quantity, @MOQ_Value, @SaleNote, @ExpiryDate, GETDATE(), @ProductID, @AddedbyUserId, GETDATE(), @CompanyID, 1, 0);
                                    SELECT SCOPE_IDENTITY();";

                                int newId = 0;
                                using (SqlCommand cmdNewProduct = new SqlCommand(queryNewProduct, conn, trans))
                                {
                                    cmdNewProduct.Parameters.Add("@ProductCode", SqlDbType.VarChar, 50).Value = (object)productCode ?? DBNull.Value;
                                    cmdNewProduct.Parameters.Add("@ProductOrServiceCat", SqlDbType.VarChar, 100).Value = (object)category ?? DBNull.Value;
                                    cmdNewProduct.Parameters.Add("@SaleRate", SqlDbType.Decimal).Value = saleRate;
                                    cmdNewProduct.Parameters["@SaleRate"].Scale = 2;
                                    cmdNewProduct.Parameters["@SaleRate"].Precision = 18;
                                    cmdNewProduct.Parameters.Add("@TaxRate", SqlDbType.Decimal).Value = taxRate;
                                    cmdNewProduct.Parameters["@TaxRate"].Scale = 2;
                                    cmdNewProduct.Parameters["@TaxRate"].Precision = 5;
                                    cmdNewProduct.Parameters.Add("@Product_catagory", SqlDbType.VarChar, 200).Value = string.IsNullOrEmpty(txtproducttype.Text) ? (object)DBNull.Value : txtproducttype.Text;
                                    cmdNewProduct.Parameters.Add("@ProductName", SqlDbType.NVarChar, 400).Value = productName ?? (object)DBNull.Value;
                                    cmdNewProduct.Parameters.Add("@Type", SqlDbType.VarChar, 100).Value = string.IsNullOrEmpty(type) ? (object)DBNull.Value : type;
                                    cmdNewProduct.Parameters.Add("@Unit", SqlDbType.VarChar, 50).Value = string.IsNullOrEmpty(unit) ? (object)DBNull.Value : unit;
                                    cmdNewProduct.Parameters.Add("@Brand", SqlDbType.VarChar, 100).Value = string.IsNullOrEmpty(brand) ? (object)DBNull.Value : brand;
                                    cmdNewProduct.Parameters.Add("@ParentId", SqlDbType.Int).Value = parentId != 0 ? (object)parentId : DBNull.Value;
                                    cmdNewProduct.Parameters.Add("@Specification", SqlDbType.NVarChar, -1).Value = string.IsNullOrEmpty(TextBox1.Text) ? (object)DBNull.Value : TextBox1.Text;
                                    cmdNewProduct.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = quantity;
                                    cmdNewProduct.Parameters["@Quantity"].Scale = 3;
                                    cmdNewProduct.Parameters["@Quantity"].Precision = 18;
                                    cmdNewProduct.Parameters.Add("@MOQ_Value", SqlDbType.Decimal).Value = moq;
                                    cmdNewProduct.Parameters["@MOQ_Value"].Scale = 3;
                                    cmdNewProduct.Parameters["@MOQ_Value"].Precision = 18;
                                    cmdNewProduct.Parameters.Add("@SaleNote", SqlDbType.NVarChar, -1).Value = string.IsNullOrEmpty(TextBox4.Text) ? (object)DBNull.Value : TextBox4.Text;
                                    cmdNewProduct.Parameters.Add("@ExpiryDate", SqlDbType.DateTime).Value = expiryDate.HasValue ? (object)expiryDate.Value : DBNull.Value;
                                    cmdNewProduct.Parameters.Add("@ProductID", SqlDbType.VarChar, 100).Value = productid;
                                    cmdNewProduct.Parameters.Add("@AddedbyUserId", SqlDbType.VarChar, 100).Value = userId ?? (object)DBNull.Value;
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

                                InsertSystemNotification(
                                    "Product Created",
                                    "Product '" + productName + "' (Id=" + newId + ") was created.",
                                    "Success",
                                    conn, trans);

                                trans.Commit();
                                ShowOk("New product created (Id=" + newId + "). Opening stock recorded.");
                                ClearEditMode();
                            }
                            else
                            {
                                string updateQuery = @"UPDATE tbl_NewProduct SET
                                    Product_code=@ProductCode, ProductOrServiceCat=@ProductOrServiceCat, Sail_Rate=@SaleRate, Tax_Rate=@TaxRate,
                                    Product_catagory=@Product_catagory, ProductName=@ProductName, Type=@Type, [Unit]=@Unit, Brand=@Brand,
                                    Specification=@Specification, Quantity=@Quantity, MOQ_Value=@MOQ_Value, SaleNote=@SaleNote, ExpiryDate=@ExpiryDate,
                                    ModifiedByUserId=@ModifiedByUserId, ModifiedOn=GETDATE()
                                    WHERE Id=@Id AND CompanyID=@CompanyID";

                                int affected;
                                using (SqlCommand cmdUpd = new SqlCommand(updateQuery, conn, trans))
                                {
                                    cmdUpd.Parameters.Add("@ProductCode", SqlDbType.VarChar, 50).Value = (object)productCode ?? DBNull.Value;
                                    cmdUpd.Parameters.Add("@ProductOrServiceCat", SqlDbType.VarChar, 100).Value = (object)category ?? DBNull.Value;
                                    cmdUpd.Parameters.Add("@SaleRate", SqlDbType.Decimal).Value = saleRate;
                                    cmdUpd.Parameters["@SaleRate"].Scale = 2;
                                    cmdUpd.Parameters["@SaleRate"].Precision = 18;
                                    cmdUpd.Parameters.Add("@TaxRate", SqlDbType.Decimal).Value = taxRate;
                                    cmdUpd.Parameters["@TaxRate"].Scale = 2;
                                    cmdUpd.Parameters["@TaxRate"].Precision = 5;
                                    cmdUpd.Parameters.Add("@Product_catagory", SqlDbType.VarChar, 200).Value = string.IsNullOrEmpty(txtproducttype.Text) ? (object)DBNull.Value : txtproducttype.Text;
                                    cmdUpd.Parameters.Add("@ProductName", SqlDbType.NVarChar, 400).Value = productName ?? (object)DBNull.Value;
                                    cmdUpd.Parameters.Add("@Type", SqlDbType.VarChar, 100).Value = string.IsNullOrEmpty(type) ? (object)DBNull.Value : type;
                                    cmdUpd.Parameters.Add("@Unit", SqlDbType.VarChar, 50).Value = string.IsNullOrEmpty(unit) ? (object)DBNull.Value : unit;
                                    cmdUpd.Parameters.Add("@Brand", SqlDbType.VarChar, 100).Value = string.IsNullOrEmpty(brand) ? (object)DBNull.Value : brand;
                                    cmdUpd.Parameters.Add("@Specification", SqlDbType.NVarChar, -1).Value = string.IsNullOrEmpty(TextBox1.Text) ? (object)DBNull.Value : TextBox1.Text;
                                    cmdUpd.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = quantity;
                                    cmdUpd.Parameters["@Quantity"].Scale = 3;
                                    cmdUpd.Parameters["@Quantity"].Precision = 18;
                                    cmdUpd.Parameters.Add("@MOQ_Value", SqlDbType.Decimal).Value = moq;
                                    cmdUpd.Parameters["@MOQ_Value"].Scale = 3;
                                    cmdUpd.Parameters["@MOQ_Value"].Precision = 18;
                                    cmdUpd.Parameters.Add("@SaleNote", SqlDbType.NVarChar, -1).Value = string.IsNullOrEmpty(TextBox4.Text) ? (object)DBNull.Value : TextBox4.Text;
                                    cmdUpd.Parameters.Add("@ExpiryDate", SqlDbType.DateTime).Value = expiryDate.HasValue ? (object)expiryDate.Value : DBNull.Value;
                                    cmdUpd.Parameters.Add("@ModifiedByUserId", SqlDbType.VarChar, 100).Value = userId ?? (object)DBNull.Value;
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

                                InsertSystemNotification(
                                    "Product Updated",
                                    "Product '" + productName + "' (Id=" + editId + ") was updated.",
                                    "Information",
                                    conn, trans);

                                trans.Commit();
                                ShowOk("Product updated successfully.");
                                ClearEditMode();
                            }

                            lblSimilar.Text = "";
                        }
                        catch (SqlException sqlex)
                        {
                            try { trans.Rollback(); } catch { }
                            System.Diagnostics.Debug.WriteLine(sqlex.ToString());
                            if (sqlex.Number == 2627 || sqlex.Number == 2601)
                                ShowErr("A product with the same name was created by someone else just now. Please refresh and try again.");
                            else
                                ShowErr("An error occurred while saving the product. Please try again.");
                            return;
                        }
                        catch (Exception exTrans)
                        {
                            try { trans.Rollback(); } catch { }
                            System.Diagnostics.Debug.WriteLine(exTrans.ToString());
                            ShowErr("An error occurred while saving the product. Please try again.");
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                ShowErr("An error occurred while saving the product. Please try again.");
            }

            BindProductsGrid();
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
                @"SELECT Id, ProductID, Product_code, ProductOrServiceCat, Sail_Rate, Tax_Rate, Product_catagory, ProductName, Type, Unit, Brand,
                         parentId, Specification, Quantity, MOQ_Value, SaleNote, ExpiryDate
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
                    txtProductCode.Text = re["Product_code"] != DBNull.Value ? re["Product_code"].ToString() : "";
                    txtSubProductsName.Text = re["ProductName"] != DBNull.Value ? re["ProductName"].ToString() : "";
                    txtBrand.Text = re["Brand"] != DBNull.Value ? re["Brand"].ToString() : "";
                    txtUnit.Text = re["Unit"] != DBNull.Value ? re["Unit"].ToString() : "";
                    txtSalerate.Text = re["Sail_Rate"] != DBNull.Value ? re["Sail_Rate"].ToString() : "";
                    txtproducttype.Text = re["Product_catagory"] != DBNull.Value ? re["Product_catagory"].ToString() : "";
                    TextBox1.Text = re["Specification"] != DBNull.Value ? re["Specification"].ToString() : "";
                    TextBox2.Text = re["Quantity"] != DBNull.Value ? re["Quantity"].ToString() : "";
                    TextBox3.Text = re["MOQ_Value"] != DBNull.Value ? re["MOQ_Value"].ToString() : "";
                    TextBox4.Text = re["SaleNote"] != DBNull.Value ? re["SaleNote"].ToString() : "N/A";

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
                    ShowOk("Editing product Id=" + idVal + ". Update and save.");
                }
            }
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Id = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "EditProduct" || e.CommandName == "Edit")
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

            if (e.CommandName != "DeleteProduct" && e.CommandName != "Delete")
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
                                  SET DeleteMode=1, ModifiedByUserId=@UserId, ModifiedOn=GETDATE()
                                  WHERE Id=@Id AND CompanyID=@CompanyID", conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@UserId", userId);
                                cmd.Parameters.AddWithValue("@Id", delId);
                                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                                affected = cmd.ExecuteNonQuery();
                            }

                            if (affected == 0)
                            {
                                trans.Rollback();
                                ShowErr("Product not found for this company.");
                                return;
                            }

                            InsertSystemNotification(
                                "Product Deactivated",
                                "Product '" + prodName + "' was deactivated.",
                                "Warning",
                                conn, trans);

                            trans.Commit();
                            ShowOk("Product deactivated successfully.");
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
            if (!string.IsNullOrEmpty(cmdProduct.SelectedValue) && int.TryParse(cmdProduct.SelectedValue, out pid))
            {
                Session["pid"] = pid;
                BinddataByServiceCategory(pid);
                return;
            }

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
            }
        }
    }
}
