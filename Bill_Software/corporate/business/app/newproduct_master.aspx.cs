using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using System.Web.Script.Services;
using System.Configuration;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm69 : System.Web.UI.Page
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
                DbCL.FillCombo(cmdProduct, "select ProductOrServiceCat from tbl_NewparentProduct order by id");
                DbCL.FillCombo(cmbtax, "select Vat_Rate from tbl_New_Vat_Master order by ID");
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");

                //Products pre-loading in Page laod have been disabled initially and later only top 100 items are laoded
                Binddata();
            }

        }

        private void Binddata()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select top 100 Id,Product_code,ProductOrServiceCat,Product_catagory,Sail_Rate,Tax_Rate,Type,ProductName,Unit,Brand,parentId from tbl_NewProduct where ViewMode=1 and DeleteMode=0 order by Id desc";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        private string findProductId()
        {
            string PurID = "";
            string aa = "";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdString1 = "select Id,ProductID from tbl_NewProduct where Id=(select max(Id)from tbl_NewProduct)";
            SqlCommand com1 = new SqlCommand(cmdString1, DbCL.Conn);
            SqlDataReader DR1 = com1.ExecuteReader();
            if (DR1.Read())
            {
                aa = DR1.GetValue(1).ToString();
                string bb = aa.Substring(3); // Get the numeric part of ProductID, skipping "PRD"
                int k = Convert.ToInt32(bb);
                k = k + 1; // Increment the numeric part
                PurID = "PRD" + k.ToString().PadLeft(2, '0'); // Pad with leading zeros to ensure two digits
            }
            else
            {
                PurID = "PRD01"; // Start with "PRD01" if no records exist
            }

            DbCL.Conn.Close();
            return PurID;
        }

        protected void btnSave_Click_OLD(object sender, EventArgs e)
        {
            //if (txtProductCode.Text != "")
            //{
            //    //string product_code = Session["ProductCode"].ToString();
            //    //string gstRate = Session["gstRate"].ToString();
            //    DbCL.Sqlconnection();
            //    DbCL.ConnectDb();
            //    DbCL.executeRdr("insert into tbl_NewProduct(Product_code,ProductOrServiceCat,Sail_Rate,Tax_Rate,ProductName,Type,Unit,Brand,parentId) values ('" + txtProductCode.Text.ToString() + "','" + cmdProduct.Text + "','" + txtSalerate.Text + "','" + cmbtax.Text.ToString() + "','" + txtSubProductsName.Text + "','" + ddlProOrSer.Text + "','" + txtUnit.Text + "','" + txtBrand.Text + "','" + Convert.ToInt32(Session["pid"]) + "')");
            //    PanelOK.Visible = true;
            //    lblOk.Text = "Data Save Successfully...";
            //    DbCL.Conn.Close();
            //}

            if (!string.IsNullOrEmpty(txtProductCode.Text))
            {
                SqlTransaction transaction = null;

                try
                {
                    string productid = findProductId();

                    // Initialize database connection
                    DbCL.Sqlconnection();
                    DbCL.ConnectDb();

                    // Start a new transaction
                    transaction = DbCL.Conn.BeginTransaction();

                    // SQL query for tbl_NewProduct
                    string queryNewProduct = @"
                            INSERT INTO tbl_NewProduct 
                            (Product_code, ProductOrServiceCat, Sail_Rate, Tax_Rate, Product_catagory, ProductName, 
                            Type, Unit, Brand, parentId, Specification, Quantity, MOQ_Value, SaleNote, ExpiryDate, 
                            TimeStamp, ProductID, AddedbyUserId, AddedOn) 
                            VALUES 
                            (@ProductCode, @ProductOrServiceCat, @SaleRate, @TaxRate, @Product_catagory, @ProductName, 
                            @Type, @Unit, @Brand, @ParentId, @Specification, @Quantity, @MOQ_Value, @SaleNote, @ExpiryDate, 
                            GETDATE(), @ProductID, @AddedbyUserId, @AddedOn)";

                    SqlCommand cmdNewProduct = new SqlCommand(queryNewProduct, DbCL.Conn, transaction);
                    cmdNewProduct.Parameters.AddWithValue("@ProductCode", txtProductCode.Text);
                    cmdNewProduct.Parameters.AddWithValue("@ProductOrServiceCat", cmdProduct.SelectedItem.Text);
                    cmdNewProduct.Parameters.AddWithValue("@SaleRate", Convert.ToDecimal(txtSalerate.Text));
                    cmdNewProduct.Parameters.AddWithValue("@TaxRate", Convert.ToDecimal(cmbtax.SelectedItem.Text));
                    cmdNewProduct.Parameters.AddWithValue("@Product_catagory", txtproducttype.Text);
                    cmdNewProduct.Parameters.AddWithValue("@ProductName", txtSubProductsName.Text);
                    cmdNewProduct.Parameters.AddWithValue("@Type", ddlProOrSer.SelectedItem.Text);
                    cmdNewProduct.Parameters.AddWithValue("@Unit", txtUnit.Text);
                    cmdNewProduct.Parameters.AddWithValue("@Brand", txtBrand.Text);
                    cmdNewProduct.Parameters.AddWithValue("@ParentId", Convert.ToInt32(Session["pid"]));
                    cmdNewProduct.Parameters.AddWithValue("@Specification", TextBox1.Text);
                    cmdNewProduct.Parameters.AddWithValue("@Quantity", Convert.ToInt32(TextBox2.Text));
                    cmdNewProduct.Parameters.AddWithValue("@MOQ_Value", Convert.ToInt32(TextBox3.Text));
                    cmdNewProduct.Parameters.AddWithValue("@SaleNote", TextBox4.Text);
                    cmdNewProduct.Parameters.AddWithValue("@ProductID", productid);
                    cmdNewProduct.Parameters.AddWithValue("@AddedbyUserId", Session["USERID"].ToString());
                    cmdNewProduct.Parameters.AddWithValue("@AddedOn", DateTime.Now);

                    DateTime expiryDate;
                    if (DateTime.TryParse(txtfromDate.Text, out expiryDate))
                    {
                        cmdNewProduct.Parameters.AddWithValue("@ExpiryDate", expiryDate);
                    }
                    else
                    {
                        cmdNewProduct.Parameters.AddWithValue("@ExpiryDate", DBNull.Value);
                    }

                    // Execute tbl_NewProduct Insert
                    cmdNewProduct.ExecuteNonQuery();

                    // SQL query for tbl_stock
                    string queryStock = @"
                    INSERT INTO tbl_stock 
                    (Product_id, Product_name, Quantity, Sail_Rate, Service_tax_rate) 
                    VALUES 
                    (@ProductID, @ProductName, @Quantity, @SaleRate, @TaxRate)";

                    SqlCommand cmdStock = new SqlCommand(queryStock, DbCL.Conn, transaction);
                    cmdStock.Parameters.AddWithValue("@ProductID", productid);
                    cmdStock.Parameters.AddWithValue("@ProductName", txtSubProductsName.Text);
                    cmdStock.Parameters.AddWithValue("@Quantity", Convert.ToInt32(TextBox2.Text));
                    cmdStock.Parameters.AddWithValue("@SaleRate", Convert.ToDecimal(txtSalerate.Text));
                    cmdStock.Parameters.AddWithValue("@TaxRate", Convert.ToDecimal(cmbtax.SelectedItem.Text));

                    // Execute tbl_stock Insert
                    cmdStock.ExecuteNonQuery();

                    // Commit transaction if both insertions succeed
                    transaction.Commit();

                    PanelOK.Visible = true;
                    lblOk.Text = "Data saved successfully into both tables!";
                }
                catch (Exception ex)
                {
                    // Rollback the transaction if an error occurs
                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }

                    lblOk.Text = "Error: " + ex.Message;
                    PanelOK.Visible = true;
                }
                finally
                {
                    // Close the connection
                    DbCL.Conn.Close();
                }
            }

            else
            {
                lblOk.Text = "Please enter a Product Code.";
                PanelOK.Visible = true;
            }


            Binddata();

        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            // Basic guard
            if (string.IsNullOrWhiteSpace(txtProductCode.Text))
            {
                lblOk.Text = "Please enter a Product Code.";
                PanelOK.Visible = true;
                lblSimilar.Text = "";
                return;
            }

            // Normalize and read inputs
            string productCode = txtProductCode.Text.Trim();
            string category = cmdProduct.SelectedItem?.Text?.Trim() ?? "";
            string productName = txtSubProductsName.Text.Trim();
            string normProductName = productName.ToLower().Trim();
            string type = ddlProOrSer.SelectedItem?.Text ?? "";
            string unit = txtUnit.Text.Trim();
            string brand = txtBrand.Text.Trim();
            int parentId = Session["pid"] != null ? Convert.ToInt32(Session["pid"]) : 0;
            string addedBy = Session["USERID"] != null ? Session["USERID"].ToString() : "0";

            decimal saleRate = 0m;
            decimal taxRate = 0m;
            int quantity = 0;
            int moq = 0;

            Decimal.TryParse(txtSalerate.Text, out saleRate);
            Decimal.TryParse(cmbtax.SelectedItem?.Text, out taxRate);
            Int32.TryParse(TextBox2.Text, out quantity);
            Int32.TryParse(TextBox3.Text, out moq);

            DateTime? expiryDate = null;
            DateTime tmpDt;
            if (DateTime.TryParse(txtfromDate.Text, out tmpDt))
                expiryDate = tmpDt;

            // Clear UI messages
            PanelOK.Visible = false;
            lblOk.Text = "";
            lblSimilar.Text = "";

            try
            {
                // Ensure DB connection helper opens connection (same as you used before)
                DbCL.Sqlconnection();
                DbCL.ConnectDb();

                // 1) Exact existence check (normalized)
                int? existingId = null;
                string existingProductIDStr = null;
                using (var cmdCheck = new SqlCommand(@"
            SELECT TOP(1) Id, ProductID, ProductName
            FROM dbo.tbl_NewProduct
            WHERE ProductOrServiceCat = @cat
              AND LOWER(LTRIM(RTRIM(ProductName))) = @normName
            ", DbCL.Conn))
                {
                    cmdCheck.Parameters.AddWithValue("@cat", category);
                    cmdCheck.Parameters.AddWithValue("@normName", normProductName);

                    using (var rdr = cmdCheck.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            existingId = rdr["Id"] != DBNull.Value ? (int?)Convert.ToInt32(rdr["Id"]) : null;
                            existingProductIDStr = rdr["ProductID"] != DBNull.Value ? rdr["ProductID"].ToString() : null;
                        }
                    }
                }

                if (existingId.HasValue)
                {
                    // Exact duplicate found — do NOT insert. Inform user and show similar suggestions.
                    PanelError.Visible = true;
                    lblErrorMsg.Text = $"A product with the same name already exists in this category. Existing Product Id: {existingId.Value}.";

                    // Show productID too if available
                    if (!string.IsNullOrEmpty(existingProductIDStr))
                    {
                        PanelError.Visible = true;
                        lblErrorMsg.Text += $" (ProductID: {existingProductIDStr})";
                    }

                    // Also fetch a few similar products to help the user
                    List<string> similar = new List<string>();
                    using (var cmdSim = new SqlCommand(@"
                        SELECT TOP(10) ProductName
                        FROM dbo.tbl_NewProduct
                        WHERE ProductOrServiceCat = @cat
                          AND ProductName LIKE @like
                        ORDER BY ProductName", DbCL.Conn))
                    {
                        cmdSim.Parameters.AddWithValue("@cat", category);
                        cmdSim.Parameters.AddWithValue("@like", "%" + productName + "%");
                        using (var rdr = cmdSim.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                similar.Add(rdr["ProductName"].ToString());
                            }
                        }
                    }

                    if (similar.Count > 0)
                    {
                        lblSimilar.Text = "Similar products: " + string.Join(" | ", similar);
                    }
                    else
                    {
                        lblSimilar.Text = "";
                    }

                    // Optionally: you can pre-fill the form with existing product details here
                    // LoadProductIntoForm(existingId.Value);

                    return; // stop - no insert
                }

                // 2) Not existing — optionally check for "very similar" matches (informational only)
                List<string> softSimilar = new List<string>();
                using (var cmdSoft = new SqlCommand(@"
                SELECT TOP(8) ProductName
                FROM dbo.tbl_NewProduct
                WHERE ProductOrServiceCat = @cat
                  AND ProductName LIKE @like
                ORDER BY ProductName", DbCL.Conn))
                {
                    cmdSoft.Parameters.AddWithValue("@cat", category);
                    cmdSoft.Parameters.AddWithValue("@like", "%" + productName + "%");
                    using (var rdr = cmdSoft.ExecuteReader())
                    {
                        while (rdr.Read()) softSimilar.Add(rdr["ProductName"].ToString());
                    }
                }
                if (softSimilar.Count > 0)
                {
                    // show suggestions but proceed with insert
                    lblSimilar.Text = "Found similar items: " + string.Join(" | ", softSimilar);
                    //PanelOK.Visible = true;
                    //lblOk.Text = "No exact duplicate found. Proceeding to insert new product (but please review the similar items listed).";
                    PanelError.Visible = true;
                    lblErrorMsg.Text = "No exact duplicate found. Proceeding to insert new product (but please review the similar items listed).";
                }

                if (DbCL.Conn == null || DbCL.Conn.State != ConnectionState.Open)
                    throw new InvalidOperationException("Database connection could not be opened.");

                //// 3) Proceed to insert product + stock in a single DB transaction (preserve behavior)
                //using (SqlTransaction transaction = DbCL.Conn.BeginTransaction())
                //{
                //    try
                //    {
                //        // generate your productid string
                //        string productid = findProductId();

                //        // Insert into tbl_NewProduct
                //        string queryNewProduct = @"
                //            INSERT INTO tbl_NewProduct 
                //            (Product_code, ProductOrServiceCat, Sail_Rate, Tax_Rate, Product_catagory, ProductName, Type, Unit, Brand, parentId, Specification, Quantity, MOQ_Value, SaleNote, ExpiryDate, TimeStamp, ProductID, AddedbyUserId, AddedOn) 
                //            VALUES 
                //            (@ProductCode, @ProductOrServiceCat, @SaleRate, @TaxRate, @Product_catagory, @ProductName, @Type, @Unit, @Brand, @ParentId, @Specification, @Quantity, @MOQ_Value, @SaleNote, @ExpiryDate, 
                //             GETDATE(), @ProductID, @AddedbyUserId, @AddedOn);
                //            SELECT SCOPE_IDENTITY();";

                //        int newId = 0;
                //        using (var cmdNewProduct = new SqlCommand(queryNewProduct, DbCL.Conn, transaction))
                //        {
                //            cmdNewProduct.Parameters.AddWithValue("@ProductCode", productCode);
                //            cmdNewProduct.Parameters.AddWithValue("@ProductOrServiceCat", category);
                //            cmdNewProduct.Parameters.AddWithValue("@SaleRate", saleRate);
                //            cmdNewProduct.Parameters.AddWithValue("@TaxRate", taxRate);
                //            cmdNewProduct.Parameters.AddWithValue("@Product_catagory", string.IsNullOrEmpty(txtproducttype.Text) ? (object)DBNull.Value : txtproducttype.Text);
                //            cmdNewProduct.Parameters.AddWithValue("@ProductName", productName);
                //            cmdNewProduct.Parameters.AddWithValue("@Type", string.IsNullOrEmpty(type) ? (object)DBNull.Value : type);
                //            cmdNewProduct.Parameters.AddWithValue("@Unit", string.IsNullOrEmpty(unit) ? (object)DBNull.Value : unit);
                //            cmdNewProduct.Parameters.AddWithValue("@Brand", string.IsNullOrEmpty(brand) ? (object)DBNull.Value : brand);
                //            cmdNewProduct.Parameters.AddWithValue("@ParentId", parentId);
                //            cmdNewProduct.Parameters.AddWithValue("@Specification", string.IsNullOrEmpty(TextBox1.Text) ? (object)DBNull.Value : TextBox1.Text);
                //            cmdNewProduct.Parameters.AddWithValue("@Quantity", quantity);
                //            cmdNewProduct.Parameters.AddWithValue("@MOQ_Value", moq);
                //            cmdNewProduct.Parameters.AddWithValue("@SaleNote", string.IsNullOrEmpty(TextBox4.Text) ? (object)DBNull.Value : TextBox4.Text);
                //            cmdNewProduct.Parameters.AddWithValue("@ProductID", productid);
                //            cmdNewProduct.Parameters.AddWithValue("@AddedbyUserId", addedBy);
                //            cmdNewProduct.Parameters.AddWithValue("@AddedOn", DateTime.Now);
                //            cmdNewProduct.Parameters.AddWithValue("@ExpiryDate", expiryDate.HasValue ? (object)expiryDate.Value : DBNull.Value);

                //            //object scopeObj = cmdNewProduct.ExecuteScalar();
                //            //newId = scopeObj != null ? Convert.ToInt32(scopeObj) : 0;

                //            object scopeObj = cmdNewProduct.ExecuteScalar();
                //            if (scopeObj != null && scopeObj != DBNull.Value)
                //            {
                //                // SCOPE_IDENTITY() returns decimal (SqlDecimal), so convert via Decimal first
                //                newId = Convert.ToInt32(Convert.ToDecimal(scopeObj));
                //            }
                //        }

                //        // Insert into tbl_stock using ProductID (string) or newId as needed
                //        string queryStock = @"
                //            INSERT INTO tbl_stock 
                //            (Product_id, Product_name, Quantity, Sail_Rate, Service_tax_rate, ShippedToStoreId, ShippedToStoreName) 
                //            VALUES 
                //            (@ProductID, @ProductName, @Quantity, @SaleRate, @TaxRate, @ShippedToStoreId, @ShippedToStoreName)";

                //        using (var cmdStock = new SqlCommand(queryStock, DbCL.Conn, transaction))
                //        {
                //            // Decision: use ProductID string (productid) as your other code did
                //            cmdStock.Parameters.AddWithValue("@ProductID", productid);
                //            cmdStock.Parameters.AddWithValue("@ProductName", productName);
                //            cmdStock.Parameters.AddWithValue("@Quantity", quantity);
                //            cmdStock.Parameters.AddWithValue("@SaleRate", saleRate);
                //            cmdStock.Parameters.AddWithValue("@TaxRate", taxRate);
                //            cmdStock.Parameters.AddWithValue("@ShippedToStoreId", "STR001");
                //            cmdStock.Parameters.AddWithValue("@ShippedToStoreName", "Central Warehouse");

                //            cmdStock.ExecuteNonQuery();
                //        }

                //        // Commit both inserts together
                //        transaction.Commit();

                //        PanelOK.Visible = true;
                //        lblOk.Text = $"New product created (Id={newId}). Opening stock recorded.";
                //        // Clear similar label because insert successful
                //        lblSimilar.Text = "";
                //    }
                //    catch (SqlException sqlex)
                //    {
                //        // Handle unique-key race condition (someone inserted concurrently)
                //        try { transaction.Rollback(); } catch { }

                //        if (sqlex.Number == 2627 || sqlex.Number == 2601)
                //        {
                //            PanelError.Visible = true;
                //            lblErrorMsg.Text = "A product with the same name was created by someone else just now. Please refresh and try again (or choose the existing product).";
                //            // Optionally, show similar/exact now
                //        }
                //        else
                //        {
                //            PanelError.Visible = true;
                //            lblErrorMsg.Text = "SQL Error: " + sqlex.Message;
                //        }
                //        return;
                //    }
                //    catch (Exception exTrans)
                //    {
                //        try { transaction.Rollback(); } catch { }
                //        PanelError.Visible = true;
                //        lblErrorMsg.Text = "Error: " + exTrans.Message;
                //        return;
                //    }
                //} // end using transaction

                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open(); // IMPORTANT: ensure connection is open before BeginTransaction

                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // generate your productid string
                            string productid = findProductId();

                            // 1) Insert into tbl_NewProduct and return identity
                            string queryNewProduct = @"INSERT INTO tbl_NewProduct (Product_code, ProductOrServiceCat, Sail_Rate, Tax_Rate, Product_catagory, ProductName, Type, [Unit], Brand, parentId, Specification, Quantity, MOQ_Value, SaleNote, ExpiryDate, TimeStamp, ProductID, AddedbyUserId, AddedOn) VALUES (@ProductCode, @ProductOrServiceCat, @SaleRate, @TaxRate, @Product_catagory, @ProductName, @Type, @Unit, @Brand, @ParentId, @Specification, @Quantity, @MOQ_Value, @SaleNote, @ExpiryDate, GETDATE(), @ProductID, @AddedbyUserId, @AddedOn); SELECT SCOPE_IDENTITY();";

                            int newId = 0;
                            using (var cmdNewProduct = new SqlCommand(queryNewProduct, conn, transaction))
                            {
                                // NOTE: change SqlDbType where appropriate for your schema (example types used)
                                cmdNewProduct.Parameters.Add("@ProductCode", SqlDbType.VarChar, 50).Value = (object)productCode ?? DBNull.Value;
                                cmdNewProduct.Parameters.Add("@ProductOrServiceCat", SqlDbType.VarChar, 100).Value = (object)category ?? DBNull.Value;
                                cmdNewProduct.Parameters.Add("@SaleRate", SqlDbType.Decimal).Value = saleRate; // ensure saleRate is decimal
                                cmdNewProduct.Parameters["@SaleRate"].Scale = 2; cmdNewProduct.Parameters["@SaleRate"].Precision = 18;
                                cmdNewProduct.Parameters.Add("@TaxRate", SqlDbType.Decimal).Value = taxRate;     // ensure taxRate is decimal
                                cmdNewProduct.Parameters["@TaxRate"].Scale = 2; cmdNewProduct.Parameters["@TaxRate"].Precision = 5;
                                cmdNewProduct.Parameters.Add("@Product_catagory", SqlDbType.VarChar, 200).Value = string.IsNullOrEmpty(txtproducttype.Text) ? (object)DBNull.Value : txtproducttype.Text;
                                cmdNewProduct.Parameters.Add("@ProductName", SqlDbType.NVarChar, 400).Value = productName ?? (object)DBNull.Value;
                                cmdNewProduct.Parameters.Add("@Type", SqlDbType.VarChar, 100).Value = string.IsNullOrEmpty(type) ? (object)DBNull.Value : type;
                                cmdNewProduct.Parameters.Add("@Unit", SqlDbType.VarChar, 50).Value = string.IsNullOrEmpty(unit) ? (object)DBNull.Value : unit;
                                cmdNewProduct.Parameters.Add("@Brand", SqlDbType.VarChar, 100).Value = string.IsNullOrEmpty(brand) ? (object)DBNull.Value : brand;
                                cmdNewProduct.Parameters.Add("@ParentId", SqlDbType.Int).Value = parentId != 0 ? (object)parentId : DBNull.Value; // adjust type if parentId is varchar
                                cmdNewProduct.Parameters.Add("@Specification", SqlDbType.NVarChar, -1).Value = string.IsNullOrEmpty(TextBox1.Text) ? (object)DBNull.Value : TextBox1.Text;
                                cmdNewProduct.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = quantity; // ensure quantity numeric
                                cmdNewProduct.Parameters["@Quantity"].Scale = 3; cmdNewProduct.Parameters["@Quantity"].Precision = 18;
                                cmdNewProduct.Parameters.Add("@MOQ_Value", SqlDbType.Decimal).Value = moq;
                                cmdNewProduct.Parameters["@MOQ_Value"].Scale = 3; cmdNewProduct.Parameters["@MOQ_Value"].Precision = 18;
                                cmdNewProduct.Parameters.Add("@SaleNote", SqlDbType.NVarChar, -1).Value = string.IsNullOrEmpty(TextBox4.Text) ? (object)DBNull.Value : TextBox4.Text;
                                cmdNewProduct.Parameters.Add("@ExpiryDate", SqlDbType.DateTime).Value = expiryDate.HasValue ? (object)expiryDate.Value : DBNull.Value;
                                cmdNewProduct.Parameters.Add("@ProductID", SqlDbType.VarChar, 100).Value = productid;
                                cmdNewProduct.Parameters.Add("@AddedbyUserId", SqlDbType.VarChar, 100).Value = addedBy ?? (object)DBNull.Value;
                                cmdNewProduct.Parameters.Add("@AddedOn", SqlDbType.DateTime).Value = DateTime.Now;

                                object scopeObj = cmdNewProduct.ExecuteScalar();
                                if (scopeObj != null && scopeObj != DBNull.Value)
                                {
                                    // SCOPE_IDENTITY() returns numeric (decimal) for SQL Server — convert safely
                                    decimal scopeDec;
                                    if (decimal.TryParse(scopeObj.ToString(), out scopeDec))
                                    {
                                        newId = Convert.ToInt32(scopeDec);
                                    }
                                }
                            }

                            // 2) Insert into tbl_stock (use string ProductID field)
                            string queryStock = @"INSERT INTO tbl_stock (Product_id, Product_name, Quantity, Sail_Rate, Service_tax_rate, ShippedToStoreId, ShippedToStoreName) VALUES (@ProductID, @ProductName, @Quantity, @SaleRate, @TaxRate, @ShippedToStoreId, @ShippedToStoreName);";

                            using (var cmdStock = new SqlCommand(queryStock, conn, transaction))
                            {
                                cmdStock.Parameters.Add("@ProductID", SqlDbType.VarChar, 100).Value = productid ?? (object)DBNull.Value;
                                cmdStock.Parameters.Add("@ProductName", SqlDbType.NVarChar, 400).Value = productName ?? (object)DBNull.Value;
                                cmdStock.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = quantity;
                                cmdStock.Parameters["@Quantity"].Scale = 3; cmdStock.Parameters["@Quantity"].Precision = 18;
                                cmdStock.Parameters.Add("@SaleRate", SqlDbType.Decimal).Value = saleRate;
                                cmdStock.Parameters["@SaleRate"].Scale = 2; cmdStock.Parameters["@SaleRate"].Precision = 18;
                                cmdStock.Parameters.Add("@TaxRate", SqlDbType.Decimal).Value = taxRate;
                                cmdStock.Parameters["@TaxRate"].Scale = 2; cmdStock.Parameters["@TaxRate"].Precision = 5;
                                cmdStock.Parameters.Add("@ShippedToStoreId", SqlDbType.VarChar, 50).Value = "STR001";
                                cmdStock.Parameters.Add("@ShippedToStoreName", SqlDbType.VarChar, 200).Value = "Central Warehouse";

                                cmdStock.ExecuteNonQuery();
                            }

                            // 3) Commit the transaction if both inserts succeeded
                            transaction.Commit();

                            PanelOK.Visible = true;
                            lblOk.Text = $"New product created (Id={newId}). Opening stock recorded.";
                            lblSimilar.Text = ""; // clear similar message
                        }
                        catch (SqlException sqlex)
                        {
                            try
                            {
                                transaction.Rollback();
                            }
                            catch { /* ignore rollback failures */ }

                            if (sqlex.Number == 2627 || sqlex.Number == 2601)
                            {
                                PanelError.Visible = true;
                                lblErrorMsg.Text = "A product with the same name was created by someone else just now. Please refresh and try again (or choose the existing product).";
                            }
                            else
                            {
                                PanelError.Visible = true;
                                lblErrorMsg.Text = "SQL Error: " + sqlex.Message;
                            }
                            return;
                        }
                        catch (Exception exTrans)
                        {
                            try { transaction.Rollback(); } catch { }
                            PanelError.Visible = true;
                            lblErrorMsg.Text = "Error: " + exTrans.Message;
                            return;
                        }
                    } // using transaction
                } // using conn
            }
            catch (Exception ex)
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "Error: " + ex.Message;
            }
            finally
            {
                try { if (DbCL.Conn != null && DbCL.Conn.State == ConnectionState.Open) DbCL.Conn.Close(); } catch { }
            }

            // Refresh UI list / grid
            Binddata();
        }

        public class DuplicateInfoResult
        {
            public bool foundExact { get; set; }
            public int existingId { get; set; }            // Id if foundExact==true
            public string productID { get; set; }          // ProductID string if available
            public List<string> similar { get; set; }      // similar product names (informational)
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static DuplicateInfoResult GetDuplicateInfo(string productName, string category)
        {
            var result = new DuplicateInfoResult { foundExact = false, existingId = 0, productID = null, similar = new List<string>() };

            if (string.IsNullOrWhiteSpace(productName) || string.IsNullOrWhiteSpace(category))
                return result;

            string cs = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

            // normalize incoming name for exact compare
            string normName = productName.Trim().ToLower();

            using (var conn = new SqlConnection(cs))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();

                // 1) exact match check (normalized)
                cmd.CommandText = @"
            SELECT TOP(1) Id, ProductID, ProductName
            FROM dbo.tbl_NewProduct
            WHERE ProductOrServiceCat = @cat
              AND LOWER(LTRIM(RTRIM(ProductName))) = @normName";
                cmd.Parameters.AddWithValue("@cat", category);
                cmd.Parameters.AddWithValue("@normName", normName);

                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        result.foundExact = true;
                        result.existingId = rdr["Id"] != DBNull.Value ? Convert.ToInt32(rdr["Id"]) : 0;
                        result.productID = rdr["ProductID"] != DBNull.Value ? rdr["ProductID"].ToString() : null;
                        // we can return early or still fill similar items below (I will still fill similar after closing reader)
                    }
                }

                // 2) fetch similar names (informational)
                using (var cmd2 = conn.CreateCommand())
                {
                    cmd2.CommandText = @"
                SELECT TOP(10) ProductName
                FROM dbo.tbl_NewProduct
                WHERE ProductOrServiceCat = @cat
                  AND ProductName LIKE @like
                ORDER BY ProductName";
                    cmd2.Parameters.AddWithValue("@cat", category);
                    cmd2.Parameters.AddWithValue("@like", "%" + productName + "%");

                    using (var rdr2 = cmd2.ExecuteReader())
                    {
                        while (rdr2.Read())
                        {
                            result.similar.Add(rdr2["ProductName"].ToString());
                        }
                    }
                }
            }

            return result;
        }


        //protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        //{
        //    string Id = Convert.ToString(e.CommandArgument);

        //    if (e.CommandName == "Delete")
        //    {
        //        DbCL.executeRdr("UPDATE tbl_NewProduct SET ViewMode=0, DeleteMode=1, DeletedOn=@DeletedOn, DeletedByUserId=@DeletedByUserId where Id='" + Id + "'");
        //        PanelOK.Visible = true;
        //        lblOk.Text = "Data Deleted Successfully...";
        //    }
        //    else if (e.CommandName == "Edit")
        //    {
        //        Response.Redirect("NewUpdate_product.aspx?Id=" + Id);
        //    }
        //    Binddata();
        //}

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Id = Convert.ToString(e.CommandArgument);

            try
            {
                if (e.CommandName == "Delete")
                {
                    string query = "UPDATE tbl_NewProduct SET ViewMode=0, DeleteMode=1, DeletedOn=@DeletedOn, DeletedByUserId=@DeletedByUserId WHERE Id=@Id";

                    // Initialize database connection
                    DbCL.Sqlconnection();
                    DbCL.ConnectDb();

                    using (SqlCommand cmd = new SqlCommand(query, DbCL.Conn))
                    {
                        cmd.Parameters.AddWithValue("@DeletedOn", DateTime.Now);
                        cmd.Parameters.AddWithValue("@DeletedByUserId", Session["USERID"].ToString());
                        cmd.Parameters.AddWithValue("@Id", Id);

                        cmd.ExecuteNonQuery();
                    }

                    PanelOK.Visible = true;
                    lblOk.Text = "Data Deleted Successfully...";
                }
                else if (e.CommandName == "Edit")
                {
                    Response.Redirect("NewUpdate_product.aspx?Id=" + Id);
                }
            }
            catch (Exception ex)
            {
                PanelOK.Visible = true;
                lblOk.Text = "Error: " + ex.Message;
            }
            finally
            {
                // Ensure proper database connection handling
                if (DbCL.Conn != null && DbCL.Conn.State == ConnectionState.Open)
                {
                    DbCL.Conn.Close();
                }
            }

            Binddata();
        }


        protected void cmdProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            string query = "select id from tbl_NewparentProduct where ProductOrServiceCat=@ProductOrServiceCat";
            SqlParameter[] pram = {
                new SqlParameter("@ProductOrServiceCat",cmdProduct.Text)
            };
            DataTable dt = new DataTable();
            dt = DbCL.SPreturn_dt(query, pram);
            if (dt.Rows.Count > 0)
            {
                int pid = Convert.ToInt32(dt.Rows[0]["id"]);
                //string ProductCode = dt.Rows[0]["ProductCode"].ToString();
                //string gstRate = dt.Rows[0]["gstRate"].ToString();
                //Session["ProductCode"] = ProductCode;
                //Session["gstRate"] = gstRate;
                Session["pid"] = pid;
                BinddataByServiceCategory(pid);
            }
        }

        private void BinddataByServiceCategory(int ParentId)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Id,Product_code,ProductOrServiceCat,Product_catagory,Sail_Rate,Tax_Rate,Type,ProductName,Unit,Brand,parentId from tbl_NewProduct where parentId=@parentId order by Id asc";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.Parameters.AddWithValue("@parentId", ParentId);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

    }
}