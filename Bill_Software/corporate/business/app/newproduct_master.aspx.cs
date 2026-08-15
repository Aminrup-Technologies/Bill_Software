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
                Binddata();
            }
        }

        private void BindCategories()
        {
            cmdProduct.Items.Clear();
            cmdProduct.Items.Add("--Select--");
            using (SqlConnection conn = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT ProductOrServiceCat FROM tbl_NewparentProduct WHERE CompanyID=@CompanyID ORDER BY ProductOrServiceCat", conn))
            {
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                        cmdProduct.Items.Add(rdr[0].ToString());
                }
            }
        }

        private void Binddata()
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT TOP 100 Id, Product_code, ProductOrServiceCat, Product_catagory, Sail_Rate, Tax_Rate, Type, ProductName, Unit, Brand, parentId, ProductID
                  FROM tbl_NewProduct
                  WHERE CompanyID=@CompanyID AND ViewMode=1 AND (DeleteMode=0 OR DeleteMode IS NULL)
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

        private void BinddataByServiceCategory(int ParentId)
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT Id, Product_code, ProductOrServiceCat, Product_catagory, Sail_Rate, Tax_Rate, Type, ProductName, Unit, Brand, parentId, ProductID
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

        private bool CheckDuplicateProduct(string category, string productName, string productId, SqlConnection conn, SqlTransaction trans)
        {
            string normName = (productName ?? string.Empty).Trim().ToLower();
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT COUNT(1) FROM dbo.tbl_NewProduct
                  WHERE CompanyID=@CompanyID
                    AND (DeleteMode=0 OR DeleteMode IS NULL)
                    AND (
                          (LOWER(LTRIM(RTRIM(ProductName))) = @normName AND ProductOrServiceCat = @cat)
                          OR (@productId <> '' AND ProductID = @productId)
                    )", conn, trans))
            {
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                cmd.Parameters.AddWithValue("@cat", category ?? string.Empty);
                cmd.Parameters.AddWithValue("@normName", normName);
                cmd.Parameters.AddWithValue("@productId", productId ?? string.Empty);
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
            int parentId = Session["pid"] != null ? Convert.ToInt32(Session["pid"]) : 0;
            string addedBy = Session["USERID"] != null ? Session["USERID"].ToString() : "0";
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

            try
            {
                List<string> softSimilar = new List<string>();
                using (SqlConnection connPre = new SqlConnection(ConnString))
                {
                    connPre.Open();
                    using (SqlCommand cmdSoft = new SqlCommand(
                        @"SELECT TOP(8) ProductName FROM dbo.tbl_NewProduct
                          WHERE CompanyID=@CompanyID AND ProductOrServiceCat=@cat AND ProductName LIKE @like
                            AND (DeleteMode=0 OR DeleteMode IS NULL)
                          ORDER BY ProductName", connPre))
                    {
                        cmdSoft.Parameters.AddWithValue("@CompanyID", companyId);
                        cmdSoft.Parameters.AddWithValue("@cat", category);
                        cmdSoft.Parameters.AddWithValue("@like", "%" + productName + "%");
                        using (SqlDataReader rdr = cmdSoft.ExecuteReader())
                        {
                            while (rdr.Read()) softSimilar.Add(rdr["ProductName"].ToString());
                        }
                    }
                }
                if (softSimilar.Count > 0)
                    lblSimilar.Text = "Found similar items: " + string.Join(" | ", softSimilar);

                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            if (CheckDuplicateProduct(category, productName, "", conn, transaction))
                            {
                                transaction.Rollback();
                                ShowErr("A product with the same name already exists in this category for this company.");
                                return;
                            }

                            string productid = findProductId(conn, transaction);
                            if (CheckDuplicateProduct(category, productName, productid, conn, transaction))
                            {
                                transaction.Rollback();
                                ShowErr("A product with the same Product ID already exists for this company.");
                                return;
                            }

                            string queryNewProduct = @"INSERT INTO tbl_NewProduct
                                (Product_code, ProductOrServiceCat, Sail_Rate, Tax_Rate, Product_catagory, ProductName, Type, [Unit], Brand, parentId, Specification, Quantity, MOQ_Value, SaleNote, ExpiryDate, TimeStamp, ProductID, AddedbyUserId, AddedOn, CompanyID, ViewMode, DeleteMode)
                                VALUES
                                (@ProductCode, @ProductOrServiceCat, @SaleRate, @TaxRate, @Product_catagory, @ProductName, @Type, @Unit, @Brand, @ParentId, @Specification, @Quantity, @MOQ_Value, @SaleNote, @ExpiryDate, GETDATE(), @ProductID, @AddedbyUserId, GETDATE(), @CompanyID, 1, 0);
                                SELECT SCOPE_IDENTITY();";

                            int newId = 0;
                            using (SqlCommand cmdNewProduct = new SqlCommand(queryNewProduct, conn, transaction))
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
                                cmdNewProduct.Parameters.Add("@AddedbyUserId", SqlDbType.VarChar, 100).Value = addedBy ?? (object)DBNull.Value;
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
                            using (SqlCommand cmdStock = new SqlCommand(queryStock, conn, transaction))
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
                                conn, transaction);

                            transaction.Commit();
                            ShowOk("New product created (Id=" + newId + "). Opening stock recorded.");
                            lblSimilar.Text = "";
                        }
                        catch (SqlException sqlex)
                        {
                            try { transaction.Rollback(); } catch { }
                            System.Diagnostics.Debug.WriteLine(sqlex.ToString());
                            if (sqlex.Number == 2627 || sqlex.Number == 2601)
                                ShowErr("A product with the same name was created by someone else just now. Please refresh and try again.");
                            else
                                ShowErr("An error occurred while saving the product. Please try again.");
                            return;
                        }
                        catch (Exception exTrans)
                        {
                            try { transaction.Rollback(); } catch { }
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

            Binddata();
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
            if (string.IsNullOrWhiteSpace(productName) || string.IsNullOrWhiteSpace(category))
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

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Id = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Edit")
            {
                Response.Redirect("NewUpdate_product.aspx?Id=" + Id);
                return;
            }

            if (e.CommandName != "Delete")
            {
                Binddata();
                return;
            }

            int idVal;
            if (!int.TryParse(Id, out idVal))
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
                                cmdLookup.Parameters.AddWithValue("@Id", idVal);
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
                                  SET ViewMode=0, DeleteMode=1, DeletedOn=GETDATE(), DeletedByUserId=@DeletedByUserId
                                  WHERE Id=@Id AND CompanyID=@CompanyID", conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@DeletedByUserId", userId);
                                cmd.Parameters.AddWithValue("@Id", idVal);
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
                            ShowOk("Data Deleted Successfully...");
                        }
                        catch (Exception ex)
                        {
                            try { trans.Rollback(); } catch { }
                            System.Diagnostics.Debug.WriteLine(ex.ToString());
                            ShowErr("An error occurred while deleting the product. Please try again.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                ShowErr("An error occurred while deleting the product. Please try again.");
            }

            Binddata();
        }

        protected void cmdProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT id FROM tbl_NewparentProduct WHERE ProductOrServiceCat=@ProductOrServiceCat AND CompanyID=@CompanyID", conn))
            {
                cmd.Parameters.AddWithValue("@ProductOrServiceCat", cmdProduct.Text);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                conn.Open();
                object o = cmd.ExecuteScalar();
                if (o != null && o != DBNull.Value)
                {
                    int pid = Convert.ToInt32(o);
                    Session["pid"] = pid;
                    BinddataByServiceCategory(pid);
                }
            }
        }
    }
}
