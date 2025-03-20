using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace Bill_Software.corporate.business.app
{
    public partial class ImportProducts : System.Web.UI.Page
    {
        private readonly string uploadFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads");
        DB_UTILITY DbCL = new DB_UTILITY();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }

            if (!IsPostBack)
            {
                lnkDownload.Visible = false;
                Panel1.Visible = false;

                //HL_InsertSucessLog.Visible = false;
                //HL_InsertFailureLog.Visible = false;

                //HL_UpdateSucessLog.Visible = false;
                //HL_UpdateFailureLog.Visible = false;

                //HL_UpsertSucessLog.Visible = false;
                //HL_UpsertFailureLog.Visible = false;

                row1.Visible = false;
                row2.Visible = false;
                row3.Visible = false;
                row4.Visible = false;
                row5.Visible = false;
            }
        }

        private void BindListitem()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "SELECT id, ProductOrServiceCat FROM tbl_NewparentProduct ORDER BY id";

            cmbproduct_service.Items.Clear();
            cmbproduct_service.Items.Add(new ListItem("--Select--", "0"));

            using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
            {
                using (SqlDataReader re = cmd.ExecuteReader())
                {
                    while (re.Read())
                    {
                        ListItem item = new ListItem(re["ProductOrServiceCat"].ToString(), re["id"].ToString());
                        cmbproduct_service.Items.Add(item);
                    }
                }
            }

            DbCL.Conn.Close();
        }

        private void BindListitem_Old()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "";
            cmdstring = "select ProductOrServiceCat from tbl_NewparentProduct order by Id";
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

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            if (fileUploader.HasFile)
            {
                try
                {
                    // Define upload path
                    string uploadFolder = Server.MapPath("~/Uploads/");
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    // Get the original file name from the uploaded file
                    string originalfullFileName = Path.GetFileName(fileUploader.FileName);
                    string originalFileName = Path.GetFileNameWithoutExtension(fileUploader.FileName);
                    // Save file
                    //string filePath = Path.Combine(uploadFolder, fileUploader.FileName);
                    //string filePath = Path.Combine(uploadFolder, originalfullFileName);
                    string filePath = Path.Combine(uploadFolder, originalFileName + Path.GetExtension(fileUploader.FileName));
                    fileUploader.SaveAs(filePath);

                    // Store path in session for reloading data
                    //Session["UploadedFilePath"] = filePath;

                    // Load and display data
                    string logfilepath = string.Empty;
                    string logfilename = string.Empty;

                    if (rb1.Checked)
                    {
                        LoadXMLData(filePath, originalFileName, ref logfilepath, ref logfilename);
                        PopulateColumnMappings();

                        Row_Panel1.Visible = true;
                        Panel1.Visible = true;

                        PanelOK.Visible = true;
                        lblOk.Text = "Product Data : Upload & Read Successful";
                    }
                    else if (rb2.Checked)
                    {
                        LoadGSTXMLData(filePath, originalFileName, ref logfilepath, ref logfilename);
                        PopulateColumnMappingsFromXML();
                        Row_Panel2.Visible = true;
                        Panel2.Visible = true;

                        PanelOK.Visible = true;
                        lblOk.Text = "Product GST : Upload & Read Successful";
                    }
                        
                    //DownloadFile(logfilepath);
                    // Store the log file path in ViewState for download
                    //ViewState["DownloadFilePath"] = logfilepath;
                    //string relativePath = ViewState["DownloadFilePath"].ToString();

                    string relativePath = logfilepath.Replace(Server.MapPath("~/"), "~/").Replace("\\", "/");
                    lnkDownload.NavigateUrl = relativePath;
                    lnkDownload.Attributes["download"] = Path.GetFileName(relativePath);
                    lnkDownload.Visible = true;

                    BindListitem();

                    btnUpload.Enabled = false;
                    fileUploader.Enabled = false;
                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('Error: " + ex.Message + "');</script>");
                }
            }
            else
            {
                Response.Write("<script>alert('Please select a file.');</script>");
            }          
        }

        private void PopulateColumnMappingsFromXML()
        {
            if (GridView2.Rows.Count > 0 && GridView2.HeaderRow != null)
            {
                // Clear existing items before adding new ones
                ddl_ProductName.Items.Clear();
                ddl_HSNCode.Items.Clear();
                ddl_GSTRate.Items.Clear();
                //ddl_CGST.Items.Clear();
                //ddl_SGST.Items.Clear();
                //ddl_IGST.Items.Clear();

                // Add default "--Select--" option
                ddl_ProductName.Items.Add(new ListItem("--Select--", ""));
                ddl_HSNCode.Items.Add(new ListItem("--Select--", ""));
                ddl_GSTRate.Items.Add(new ListItem("--Select--", ""));
                //ddl_CGST.Items.Add(new ListItem("--Select--", ""));
                //ddl_SGST.Items.Add(new ListItem("--Select--", ""));
                //ddl_IGST.Items.Add(new ListItem("--Select--", ""));

                // Loop through GridView2 header cells
                foreach (TableCell cell in GridView2.HeaderRow.Cells)
                {
                    string colName = cell.Text.Trim();

                    ddl_ProductName.Items.Add(new ListItem(colName, colName));
                    ddl_HSNCode.Items.Add(new ListItem(colName, colName));
                    ddl_GSTRate.Items.Add(new ListItem(colName, colName));
                    //ddl_CGST.Items.Add(new ListItem(colName, colName));
                    //ddl_SGST.Items.Add(new ListItem(colName, colName));
                    //ddl_IGST.Items.Add(new ListItem(colName, colName));
                }
            }
        }



        private void PopulateColumnMappings()
        {
            if (GridView1.Rows.Count > 0)
            {
                // Clear existing items before adding new ones
                ddlProductName.Items.Clear();
                ddlQuantity.Items.Clear();
                ddlUnit.Items.Clear();
                ddlRate.Items.Clear();

                // Add default "--Select--" option
                ddlProductName.Items.Add(new ListItem("--Select--", ""));
                ddlQuantity.Items.Add(new ListItem("--Select--", ""));
                ddlUnit.Items.Add(new ListItem("--Select--", ""));
                ddlRate.Items.Add(new ListItem("--Select--", ""));

                foreach (DataControlField column in GridView1.Columns)
                {
                    string colName = column.HeaderText;
                    ddlProductName.Items.Add(new ListItem(colName, colName));
                    ddlQuantity.Items.Add(new ListItem(colName, colName));
                    ddlUnit.Items.Add(new ListItem(colName, colName));
                    ddlRate.Items.Add(new ListItem(colName, colName));
                }

                // Make rows visible
                row1.Visible = true;
                row2.Visible = true;
                row3.Visible = true;
                row4.Visible = true;
                row5.Visible = true;
            }
        }


        private void PopulateColumnMappings_Old()
        {
            if (GridView1.Rows.Count > 0)
            {
                foreach (DataControlField column in GridView1.Columns)
                {
                    string colName = column.HeaderText;
                    ddlProductName.Items.Add(new ListItem(colName, colName));
                    ddlQuantity.Items.Add(new ListItem(colName, colName));
                    ddlUnit.Items.Add(new ListItem(colName, colName));
                    ddlRate.Items.Add(new ListItem(colName, colName));
                }

                row1.Visible = true;
                row2.Visible = true;
                row3.Visible = true;
                row4.Visible = true;
                row5.Visible = true;
            }
        }

        private void LoadXMLDataOld(string xmlPath)
        {
            try
            {
                XDocument xdoc = XDocument.Load(xmlPath);

                DataTable dt = new DataTable();
                dt.Columns.Add("Product Name");
                dt.Columns.Add("Quantity");
                dt.Columns.Add("Unit");
                dt.Columns.Add("Rate");
                dt.Columns.Add("Amount");

                List<string> successLogEntries = new List<string>();

                var items = xdoc.Descendants("DSPACCNAME").Zip(
                    xdoc.Descendants("DSPSTKCL"),
                    (name, stock) => new
                    {
                        ProductName = name.Element("DSPDISPNAME")?.Value,
                        Quantity = stock.Element("DSPCLQTY")?.Value,
                        Rate = stock.Element("DSPCLRATE")?.Value,
                        Amount = stock.Element("DSPCLAMTA")?.Value
                    }
                );

                foreach (var item in items)
                {
                    if (!string.IsNullOrEmpty(item.ProductName))
                    {
                        string[] qtyParts = item.Quantity?.Split(' ') ?? new string[] { "0", "" };
                        string quantity = qtyParts[0];
                        string unit = qtyParts.Length > 1 ? qtyParts[1] : "";

                        dt.Rows.Add(item.ProductName, quantity, unit, item.Rate, item.Amount);

                        // Prepare success log entry
                        successLogEntries.Add($"Product: {item.ProductName}, Quantity: {quantity} {unit}, Rate: {item.Rate}, Amount: {item.Amount}");
                    }
                }

                GridView1.DataSource = dt;
                GridView1.DataBind();

                // Log success message with parsed details
                //LogSuccess("READER : Successfully parsed XML file: " + xmlPath, successLogEntries, "readexml.txt");
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error: " + ex.Message;
                PanelError.Visible = true;
                LogError("READER : Error in LoadXMLData: " + ex.Message + " | StackTrace: " + ex.StackTrace);
                Response.Write("<script>alert('An error occurred while processing the XML file. Please check the log file.');</script>");
            }
        }

        private void LoadXMLData(string xmlPath, string originalFileName, ref string logfilepath, ref string logfilename)
        {
            try
            {
                XDocument xdoc = XDocument.Load(xmlPath);

                DataTable dt = new DataTable();
                dt.Columns.Add("SL");
                dt.Columns.Add("Product Name");
                dt.Columns.Add("Quantity");
                dt.Columns.Add("Unit");
                dt.Columns.Add("Rate");
                dt.Columns.Add("Amount");

                List<string> successLogEntries = new List<string>();

                var items = xdoc.Descendants("DSPACCNAME").Zip(
                    xdoc.Descendants("DSPSTKCL"),
                    (name, stock) => new
                    {
                        ProductName = name.Element("DSPDISPNAME")?.Value,
                        Quantity = stock.Element("DSPCLQTY")?.Value,
                        Rate = stock.Element("DSPCLRATE")?.Value
                    }
                );

                Int32 sl = 0;
                foreach (var item in items)
                {
                    if (!string.IsNullOrEmpty(item.ProductName))
                    {
                        sl++;
                        // Extract numeric part from Quantity (if formatted as "12.5 KG")
                        string[] qtyParts = item.Quantity?.Split(' ') ?? new string[] { "0", "" };
                        string quantityStr = qtyParts[0];
                        string unit = qtyParts.Length > 1 ? qtyParts[1] : "";

                        decimal q, r = 0.0m;
                        // Parse values safely
                        decimal quantity = decimal.TryParse(quantityStr, out q) ? q : 0;
                        decimal rate = decimal.TryParse(item.Rate, out r) ? r : 0;

                        // Calculate amount
                        decimal amount = 0.0m;
                        amount = quantity * rate;
                        string amountStr = amount.ToString("F2"); // Format to 2 decimal places

                        dt.Rows.Add(sl, item.ProductName, quantityStr, unit, item.Rate, amountStr);

                        // Prepare success log entry
                        successLogEntries.Add($"READER : Sl: {sl}, Product: {item.ProductName}, Quantity: {quantity} {unit}, Rate: {item.Rate}, Amount: {amountStr}");
                    }
                }

                Row_GridView.Visible = true;
                GridView1.Visible = true;
                GridView1.DataSource = dt;
                GridView1.DataBind();



                // Log success message with parsed details
                LogSuccess("READER : Successfully parsed XML file: " + xmlPath, successLogEntries, originalFileName, ref logfilepath, ref logfilename);
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error: " + ex.Message;
                PanelError.Visible = true;
                LogError("READER : Error in LoadXMLData: " + ex.Message + " | StackTrace: " + ex.StackTrace);
                //Response.Write("<script>alert('An error occurred while processing the XML file. Please check the log file.');</script>");
            }
        }

        private void LoadGSTXMLData(string xmlPath, string originalFileName, ref string logfilepath, ref string logfilename)
        {
            try
            {
                XDocument xdoc = XDocument.Load(xmlPath);
                DataTable dt = new DataTable();
                dt.Columns.Add("SL");
                dt.Columns.Add("Product Name");
                dt.Columns.Add("HSN Code");
                dt.Columns.Add("GST Rate");
                dt.Columns.Add("CGST");
                dt.Columns.Add("SGST");
                dt.Columns.Add("IGST");

                List<string> successLogEntries = new List<string>();

                var gstEntries = xdoc.Descendants("GSTMASTERITEMSRATE")
                    .Zip(xdoc.Descendants("GSTHSNCODE"), (product, hsn) => new
                    {
                        ProductName = product.Value,
                        HSNCode = hsn.Value,
                        CGSTRate = product.ElementsAfterSelf("CGSTRATE").FirstOrDefault()?.Value?.Replace("%", "") ?? "0",
                        SGSTRate = product.ElementsAfterSelf("SGSTRATE").FirstOrDefault()?.Value?.Replace("%", "") ?? "0",
                        IGSTRate = product.ElementsAfterSelf("IGSTRATE").FirstOrDefault()?.Value?.Replace("%", "") ?? "0"
                    });

                int sl = 0;
                foreach (var item in gstEntries)
                {
                    if (!string.IsNullOrEmpty(item.ProductName))
                    {
                        sl++;
                        decimal c, s, i = 0.0m;

                        decimal cgst = decimal.TryParse(item.CGSTRate, out c) ? c : 0;
                        decimal sgst = decimal.TryParse(item.SGSTRate, out s) ? s : 0;
                        decimal igst = decimal.TryParse(item.IGSTRate, out i) ? i : 0;
                        decimal gstRate = cgst + sgst;

                        dt.Rows.Add(sl, item.ProductName, item.HSNCode, gstRate + "%", item.CGSTRate + "%", item.SGSTRate + "%", item.IGSTRate + "%");

                        successLogEntries.Add($"GST READER : Sl: {sl}, Product: {item.ProductName}, HSN: {item.HSNCode}, GST: {gstRate}%, CGST: {item.CGSTRate}%, SGST: {item.SGSTRate}%, IGST: {item.IGSTRate}%");
                    }
                }

                Row2_Gridview.Visible = true;
                GridView2.Visible = true;
                GridView2.DataSource = dt;
                GridView2.DataBind();

                LogSuccess("GST READER : Successfully parsed GST XML file: " + xmlPath, successLogEntries, originalFileName, ref logfilepath, ref logfilename);
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error: " + ex.Message;
                PanelError.Visible = true;
                LogError("GST READER : Error in LoadGSTXMLData: " + ex.Message + " | StackTrace: " + ex.StackTrace);
            }
        }


        protected void btnInsert_Click(object sender, EventArgs e)
        {
            InsertDataIntoDatabase();

            //string connString = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            //using (SqlConnection conn = new SqlConnection(connString))
            //{
            //    conn.Open();

            //    foreach (GridViewRow row in GridView1.Rows)
            //    {
            //        string productName = row.Cells[GetColumnIndex(ddlProductName.SelectedValue)].Text;
            //        string quantity = row.Cells[GetColumnIndex(ddlQuantity.SelectedValue)].Text;
            //        string unit = row.Cells[GetColumnIndex(ddlUnit.SelectedValue)].Text;
            //        string rate = row.Cells[GetColumnIndex(ddlRate.SelectedValue)].Text;

            //        string query = "INSERT INTO tbl_NewProduct (ProductName, Quantity, Unit, Purches_Rate) " +
            //                       "VALUES (@ProductName, @Quantity, @Unit, @Rate)";

            //        using (SqlCommand cmd = new SqlCommand(query, conn))
            //        {
            //            cmd.Parameters.AddWithValue("@ProductName", productName);
            //            cmd.Parameters.AddWithValue("@Quantity", quantity);
            //            cmd.Parameters.AddWithValue("@Unit", unit);
            //            cmd.Parameters.AddWithValue("@Rate", rate);

            //            //cmd.ExecuteNonQuery();
            //        }
            //    }
            //}
        }

        public void InsertDataIntoDatabase_Old()
        {
            try
            {
                // Get the connection string from web.config
                string connString = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    foreach (GridViewRow row in GridView1.Rows)
                    {
                        string productName = row.Cells[GetColumnIndex("Product Name")].Text;
                        string quantity = row.Cells[GetColumnIndex("Quantity")].Text;
                        string unit = row.Cells[GetColumnIndex("Unit")].Text;
                        string rate = row.Cells[GetColumnIndex("Rate")].Text;
                        //string amount = row.Cells[GetColumnIndex("Amount")].Text;

                        string query = @"INSERT INTO tbl_NewProduct (ProductName, Quantity, Unit, Sail_Rate) VALUES (@ProductName, @Quantity, @Unit, @SailRate)";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@ProductName", productName);
                            cmd.Parameters.AddWithValue("@Quantity", quantity);
                            cmd.Parameters.AddWithValue("@Unit", unit);
                            cmd.Parameters.AddWithValue("@SailRate", rate);
                            //cmd.Parameters.AddWithValue("@Quantity", int.TryParse(quantity, out int qty) ? qty : 0);
                            //cmd.Parameters.AddWithValue("@PurchesRate", decimal.TryParse(rate, out decimal pr) ? pr : 0);
                            //cmd.Parameters.AddWithValue("@SailRate", decimal.TryParse(amount, out decimal sr) ? sr : 0);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // Log success message
                LogSuccessInsert("INSERT : Data inserted successfully from GridView into tbl_NewProduct.");
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error: " + ex.Message;
                PanelError.Visible = true;
                // Log error message
                LogError($"INSERT : Error inserting data: {ex.Message} | StackTrace: {ex.StackTrace}");
            }
        }

        public void InsertDataIntoDatabase_Old2()
        {
            try
            {
                string connString = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string selectedProductColumn = ddlProductName.SelectedValue;
                    string selectedQuantityColumn = ddlQuantity.SelectedValue;
                    string selectedUnitColumn = ddlUnit.SelectedValue;
                    string selectedRateColumn = ddlRate.SelectedValue;

                    int slNo = 1;

                    foreach (GridViewRow row in GridView1.Rows)
                    {
                        string productName = row.Cells[GetColumnIndex(selectedProductColumn)].Text.Trim();
                        string quantity = row.Cells[GetColumnIndex(selectedQuantityColumn)].Text.Trim();
                        string unit = row.Cells[GetColumnIndex(selectedUnitColumn)].Text.Trim();
                        string rate = row.Cells[GetColumnIndex(selectedRateColumn)].Text.Trim();

                        decimal parsedQty , parsedRate  = 0.0m;
                        decimal qty = decimal.TryParse(quantity, out parsedQty) ? parsedQty : 0;
                        decimal sailRate = decimal.TryParse(rate, out parsedRate) ? parsedRate : 0;

                        string query = @"INSERT INTO tbl_NewProduct (ProductName, Quantity, Unit, Sail_Rate) VALUES (@ProductName, @Quantity, @Unit, @SailRate)";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@ProductName", productName);
                            cmd.Parameters.AddWithValue("@Quantity", qty);
                            cmd.Parameters.AddWithValue("@Unit", unit);
                            cmd.Parameters.AddWithValue("@SailRate", sailRate);

                            cmd.ExecuteNonQuery();
                        }
                        LogSuccessInsert($"INSERT [{slNo}] : Product: {productName}, Quantity: {qty}, Unit: {unit}, Sail Rate: {sailRate}");
                        slNo++;
                    }
                }
                LogSuccessInsert("INSERT : Data insertion completed successfully.");
            }
            catch (Exception ex)
            {
                LogError($"INSERT : Error inserting data: {ex.Message} | StackTrace: {ex.StackTrace}");
            }
        }

        public void InsertDataIntoDatabase()
        {
            try
            {
                string connString = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();
                    string selectedProductColumn = ddlProductName.SelectedValue;
                    string selectedQuantityColumn = ddlQuantity.SelectedValue;
                    string selectedUnitColumn = ddlUnit.SelectedValue;
                    string selectedRateColumn = ddlRate.SelectedValue;

                    int slNo = 1;

                    foreach (GridViewRow row in GridView1.Rows)
                    {
                        try
                        {
                            string productName = row.Cells[GetColumnIndex(selectedProductColumn)].Text.Trim();
                            string quantity = row.Cells[GetColumnIndex(selectedQuantityColumn)].Text.Trim();
                            string unit = row.Cells[GetColumnIndex(selectedUnitColumn)].Text.Trim();
                            string rate = row.Cells[GetColumnIndex(selectedRateColumn)].Text.Trim();

                            string productCode = string.Empty;
                            string productOrServiceCat = cmbproduct_service.SelectedItem?.Text ?? "DefaultCategory";
                            int parentId = (cmbproduct_service.SelectedItem != null && !string.IsNullOrEmpty(cmbproduct_service.SelectedValue) && cmbproduct_service.SelectedValue != "--Select--") ? Convert.ToInt32(cmbproduct_service.SelectedValue) : (Session["pid"] != null ? Convert.ToInt32(Session["pid"]) : 0);

                            string saleRate = string.IsNullOrEmpty(rate) ? "0" : rate;
                            string taxRate = "18";
                            string productCategory = "DefaultCategory";
                            string type = "Product";
                            string brand = "DefaultBrand";
                            string specification = string.Empty;
                            string qty = string.IsNullOrEmpty(quantity) ? "0" : quantity;
                            string moqValue = "1";
                            string saleNote = string.Empty;

                            // Generate Product ID
                            string productId = findProductId();

                            // Insert into tbl_NewProduct
                            string queryNewProduct = @"INSERT INTO tbl_NewProduct (Product_code, ProductOrServiceCat, Sail_Rate, Tax_Rate, Product_catagory, ProductName, Type, Unit, Brand, parentId, Specification, Quantity, MOQ_Value, SaleNote, ExpiryDate, TimeStamp, ProductID, AddedbyUserId, AddedOn) VALUES 
                            (@ProductCode, @ProductOrServiceCat, @SaleRate, @TaxRate, @Product_catagory, @ProductName, @Type, @Unit, @Brand, @ParentId, @Specification, @Quantity, @MOQ_Value, @SaleNote, @ExpiryDate, GETDATE(), @ProductID, @AddedbyUserId, @AddedOn)";

                            using (SqlCommand cmdNewProduct = new SqlCommand(queryNewProduct, conn, transaction))
                            {
                                cmdNewProduct.Parameters.AddWithValue("@ProductCode", productCode);
                                cmdNewProduct.Parameters.AddWithValue("@ProductOrServiceCat", productOrServiceCat);
                                cmdNewProduct.Parameters.AddWithValue("@SaleRate", saleRate);
                                cmdNewProduct.Parameters.AddWithValue("@TaxRate", taxRate);
                                cmdNewProduct.Parameters.AddWithValue("@Product_catagory", productCategory);
                                cmdNewProduct.Parameters.AddWithValue("@ProductName", productName);
                                cmdNewProduct.Parameters.AddWithValue("@Type", type);
                                cmdNewProduct.Parameters.AddWithValue("@Unit", unit);
                                cmdNewProduct.Parameters.AddWithValue("@Brand", brand);
                                cmdNewProduct.Parameters.AddWithValue("@ParentId", parentId);
                                cmdNewProduct.Parameters.AddWithValue("@Specification", specification);
                                cmdNewProduct.Parameters.AddWithValue("@Quantity", qty);
                                cmdNewProduct.Parameters.AddWithValue("@MOQ_Value", moqValue);
                                cmdNewProduct.Parameters.AddWithValue("@SaleNote", saleNote);
                                cmdNewProduct.Parameters.AddWithValue("@ProductID", productId);
                                cmdNewProduct.Parameters.AddWithValue("@AddedbyUserId", Session["USERID"].ToString());
                                cmdNewProduct.Parameters.AddWithValue("@AddedOn", DateTime.Now);
                                cmdNewProduct.Parameters.AddWithValue("@ExpiryDate", new DateTime(2026, 3, 31));

                                cmdNewProduct.ExecuteNonQuery();
                            }

                            string queryStock = @"INSERT INTO tbl_stock (Product_id, Product_name, Quantity, Sail_Rate, Service_tax_rate) VALUES (@ProductID, @ProductName, @Quantity, @SaleRate, @TaxRate)";

                            using (SqlCommand cmdStock = new SqlCommand(queryStock, conn, transaction))
                            {
                                cmdStock.Parameters.AddWithValue("@ProductID", productId);
                                cmdStock.Parameters.AddWithValue("@ProductName", productName);
                                cmdStock.Parameters.AddWithValue("@Quantity", qty);
                                cmdStock.Parameters.AddWithValue("@SaleRate", saleRate);
                                cmdStock.Parameters.AddWithValue("@TaxRate", taxRate);
                                cmdStock.ExecuteNonQuery();
                            }
                            transaction.Commit();

                            LogSuccessInsert($"INSERT [{slNo}] : Product: {productName}, Product Code: {productCode}, Quantity: {quantity}, Unit: {unit}, Sale Rate: {saleRate}, Tax Rate: {taxRate}");

                            slNo++;
                        }
                        catch (Exception rowEx)
                        {
                            transaction.Rollback();
                            LogError($"INSERT [{slNo}] - ERROR: Failed to insert Product: {rowEx.Message}");
                        }
                    }
                }

                LogSuccessInsert("INSERT : Data insertion process completed.");
            }
            catch (Exception ex)
            {
                LogError($"INSERT - ERROR: {ex.Message} | StackTrace: {ex.StackTrace}");
            }
        }


        public void UpdateDataInDatabase_Old()
        {
            try
            {
                string connString = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    foreach (GridViewRow row in GridView1.Rows)
                    {
                        string productName = row.Cells[GetColumnIndex("Product Name")].Text;
                        string quantity = row.Cells[GetColumnIndex("Quantity")].Text;
                        string unit = row.Cells[GetColumnIndex("Unit")].Text;
                        string rate = row.Cells[GetColumnIndex("Rate")].Text;
                        string amount = row.Cells[GetColumnIndex("Amount")].Text;

                        string query = @"UPDATE tbl_NewProduct SET Quantity = @Quantity, Unit = @Unit, Sail_Rate = @SailRate, ModifiedOn = @ModifiedOn, ModifiedByUserId = @ModifiedByUserId  WHERE ProductName = @ProductName";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@ProductName", productName);
                            cmd.Parameters.AddWithValue("@Quantity", quantity);
                            cmd.Parameters.AddWithValue("@Unit", unit);
                            cmd.Parameters.AddWithValue("@SailRate", rate);

                            //cmd.Parameters.AddWithValue("@ProductName", productName);
                            //cmd.Parameters.AddWithValue("@Quantity", int.TryParse(quantity, out int qty) ? qty : 0);
                            //cmd.Parameters.AddWithValue("@Unit", unit);
                            //cmd.Parameters.AddWithValue("@PurchesRate", decimal.TryParse(rate, out decimal pr) ? pr : 0);
                            //cmd.Parameters.AddWithValue("@SailRate", decimal.TryParse(amount, out decimal sr) ? sr : 0);
                            cmd.Parameters.AddWithValue("@ModifiedByUserId", Session["USERID"].ToString());
                            cmd.Parameters.AddWithValue("@ModifiedOn", DateTime.Now);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                LogSuccessNew($"UPDATE : Updated product: {productName}, Quantity: {quantity}, Unit: {unit}, Purches_Rate: {rate}, Sail_Rate: {amount}");
                            }
                            else
                            {
                                LogError($"UPDATE : No update made for product: {productName} (not found in database)");
                            }
                        }
                    }
                }

                LogSuccessNew("Update operation completed.");
            }
            catch (Exception ex)
            {
                LogError($"UPDATE : Error updating data: {ex.Message} | StackTrace: {ex.StackTrace}");
                lblErrorMsg.Text = "Error: " + ex.Message;
                PanelError.Visible = true;
            }
        }

        public void UpdateDataInDatabase_Old2()
        {
            try
            {
                string connString = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    // Get the column names selected in the dropdowns
                    string selectedProductColumn = ddlProductName.SelectedValue;
                    string selectedQuantityColumn = ddlQuantity.SelectedValue;
                    string selectedUnitColumn = ddlUnit.SelectedValue;
                    string selectedRateColumn = ddlRate.SelectedValue;

                    foreach (GridViewRow row in GridView1.Rows)
                    {
                        // Fetch values based on selected column mappings
                        string productName = row.Cells[GetColumnIndex(selectedProductColumn)].Text;
                        string quantity = row.Cells[GetColumnIndex(selectedQuantityColumn)].Text;
                        string unit = row.Cells[GetColumnIndex(selectedUnitColumn)].Text;
                        string rate = row.Cells[GetColumnIndex(selectedRateColumn)].Text;

                        string query = @"UPDATE tbl_NewProduct SET Quantity = @Quantity, Unit = @Unit, Sail_Rate = @SailRate, ModifiedOn = @ModifiedOn, ModifiedByUserId = @ModifiedByUserId WHERE ProductName = @ProductName";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@ProductName", productName);
                            cmd.Parameters.AddWithValue("@Quantity", quantity);
                            cmd.Parameters.AddWithValue("@Unit", unit);
                            cmd.Parameters.AddWithValue("@SailRate", rate);
                            cmd.Parameters.AddWithValue("@ModifiedByUserId", Session["USERID"].ToString());
                            cmd.Parameters.AddWithValue("@ModifiedOn", DateTime.Now);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                LogSuccessNew($"UPDATE : Updated product: {productName}, Quantity: {quantity}, Unit: {unit}, Sail_Rate: {rate}");
                            }
                            else
                            {
                                LogError($"UPDATE : No update made for product: {productName} (not found in database)");
                            }
                        }
                    }
                }

                Row_Uploader.Visible = false;
                Row_XMLType.Visible = false;
                Row_UploaderBtns.Visible = false;

                Row_Panel1.Visible = false;
                Row_Panel1.Visible = false;
                PanelOK.Visible = true;

                row1.Visible = false;
                row2.Visible = false;
                row3.Visible = false;
                row4.Visible = false;

                lblOk.Text = "UPDATE : Product and stock updated successfully!";

                LogSuccessNew("Update operation completed.");
            }
            catch (Exception ex)
            {
                LogError($"UPDATE : Error updating data: {ex.Message} | StackTrace: {ex.StackTrace}");
            }
        }

        public void UpdateDataInDatabase_old3()
        {
            SqlTransaction transaction = null;

            try
            {
                string connString = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    string selectedProductColumn = ddlProductName.SelectedValue;
                    string selectedQuantityColumn = ddlQuantity.SelectedValue;
                    string selectedUnitColumn = ddlUnit.SelectedValue;
                    string selectedRateColumn = ddlRate.SelectedValue;

                    foreach (GridViewRow row in GridView1.Rows)
                    {
                        string productName = row.Cells[GetColumnIndex(selectedProductColumn)].Text;
                        string quantity = row.Cells[GetColumnIndex(selectedQuantityColumn)].Text;
                        string unit = row.Cells[GetColumnIndex(selectedUnitColumn)].Text;
                        string rate = row.Cells[GetColumnIndex(selectedRateColumn)].Text;

                        string updateQueryProduct = @"UPDATE tbl_NewProduct SET Quantity = @Quantity, Unit = @Unit, Sail_Rate = @SailRate, ModifiedOn = GETDATE(), ModifiedByUserId = @ModifiedByUserId WHERE ProductName = @ProductName";
                        string updateQueryStock = @"UPDATE tbl_stock SET Quantity = @Quantity, Sail_Rate = @SailRate, Service_tax_rate = '18', ModifiedOn = GETDATE(), ModifiedByUserId = @ModifiedByUserId WHERE Product_name = @ProductName";

                        using (SqlCommand cmdUpdateProduct = new SqlCommand(updateQueryProduct, conn, transaction))
                        {
                            cmdUpdateProduct.Parameters.AddWithValue("@ProductName", productName);
                            cmdUpdateProduct.Parameters.AddWithValue("@Quantity", quantity);
                            cmdUpdateProduct.Parameters.AddWithValue("@Unit", unit);
                            cmdUpdateProduct.Parameters.AddWithValue("@SailRate", rate);
                            cmdUpdateProduct.Parameters.AddWithValue("@ModifiedByUserId", Session["USERID"].ToString());

                            int rowsAffectedProduct = cmdUpdateProduct.ExecuteNonQuery();
                            LogSuccessNew($"UPDATE: tbl_NewProduct updated {rowsAffectedProduct} row(s) for Product: {productName}");
                        }

                        using (SqlCommand cmdUpdateStock = new SqlCommand(updateQueryStock, conn, transaction))
                        {
                            cmdUpdateStock.Parameters.AddWithValue("@ProductName", productName);
                            cmdUpdateStock.Parameters.AddWithValue("@Quantity", quantity);
                            cmdUpdateStock.Parameters.AddWithValue("@SailRate", rate);
                            cmdUpdateStock.Parameters.AddWithValue("@ModifiedByUserId", Session["USERID"].ToString());

                            int rowsAffectedStock = cmdUpdateStock.ExecuteNonQuery();
                            LogSuccessNew($"UPDATE: tbl_stock updated {rowsAffectedStock} row(s) for Product: {productName}");
                        }
                    }

                    transaction.Commit();
                    transaction = null;

                    HideElements();

                    btnUpdate.Enabled = false;
                    btnUpdate.Text = "UPDATED";

                    PanelOK.Visible = true;
                    lblOk.Text = "UPDATE: Product and stock updated successfully!";
                }
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception rollbackEx)
                    {
                        LogError($"UPDATE ROLLBACK ERROR: {rollbackEx.Message}");
                    }
                }

                LogError($"UPDATE ERROR: {ex.Message} | StackTrace: {ex.StackTrace}");
                lblErrorMsg.Text = "Error: " + ex.Message;
                PanelError.Visible = true;
            }
        }

        public void UpdateDataInDatabase()
        {
            SqlTransaction transaction = null;

            try
            {
                string connString = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    transaction = conn.BeginTransaction(); // Start transaction

                    // Get selected column mappings
                    string selectedProductColumn = ddlProductName.SelectedValue;
                    string selectedQuantityColumn = ddlQuantity.SelectedValue;
                    string selectedUnitColumn = ddlUnit.SelectedValue;
                    string selectedRateColumn = ddlRate.SelectedValue;

                    foreach (GridViewRow row in GridView1.Rows)
                    {
                        string productName = row.Cells[GetColumnIndex(selectedProductColumn)].Text;
                        string quantity = row.Cells[GetColumnIndex(selectedQuantityColumn)].Text;
                        string unit = row.Cells[GetColumnIndex(selectedUnitColumn)].Text;
                        string rate = row.Cells[GetColumnIndex(selectedRateColumn)].Text;

                        // First, check if the product exists in tbl_NewProduct
                        string checkQueryProduct = "SELECT COUNT(*) FROM tbl_NewProduct WHERE ProductName = @ProductName";
                        string checkQueryStock = "SELECT COUNT(*) FROM tbl_stock WHERE Product_name = @ProductName";

                        bool productExists = false;
                        bool stockExists = false;

                        using (SqlCommand checkCmd = new SqlCommand(checkQueryProduct, conn, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@ProductName", productName);
                            productExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                        }

                        using (SqlCommand checkCmd = new SqlCommand(checkQueryStock, conn, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@ProductName", productName);
                            stockExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                        }

                        if (productExists)
                        {
                            // Update `tbl_NewProduct`
                            string updateQueryProduct = @"UPDATE tbl_NewProduct SET Quantity = @Quantity, Unit = @Unit, Sail_Rate = @SailRate, ModifiedOn = GETDATE(), ModifiedByUserId = @ModifiedByUserId WHERE ProductName = @ProductName";

                            using (SqlCommand cmdUpdateProduct = new SqlCommand(updateQueryProduct, conn, transaction))
                            {
                                cmdUpdateProduct.Parameters.AddWithValue("@ProductName", productName);
                                cmdUpdateProduct.Parameters.AddWithValue("@Quantity", quantity);
                                cmdUpdateProduct.Parameters.AddWithValue("@Unit", unit);
                                cmdUpdateProduct.Parameters.AddWithValue("@SailRate", rate);
                                cmdUpdateProduct.Parameters.AddWithValue("@ModifiedByUserId", Session["USERID"].ToString());

                                int rowsAffectedProduct = cmdUpdateProduct.ExecuteNonQuery();
                                LogSuccessNew($"UPDATE: tbl_NewProduct updated {rowsAffectedProduct} row(s) for Product: {productName}");
                            }
                        }
                        else
                        {
                            LogError($"UPDATE ERROR: Product '{productName}' not found in tbl_NewProduct.");
                        }

                        if (stockExists)
                        {
                            // Update `tbl_stock`
                            string updateQueryStock = @"UPDATE tbl_stock SET Quantity = @Quantity, Sail_Rate = @SailRate, Service_tax_rate = '18', ModifiedOn = GETDATE(), ModifiedByUserId = @ModifiedByUserId WHERE Product_name = @ProductName";

                            using (SqlCommand cmdUpdateStock = new SqlCommand(updateQueryStock, conn, transaction))
                            {
                                cmdUpdateStock.Parameters.AddWithValue("@ProductName", productName);
                                cmdUpdateStock.Parameters.AddWithValue("@Quantity", quantity);
                                cmdUpdateStock.Parameters.AddWithValue("@SailRate", rate);
                                cmdUpdateStock.Parameters.AddWithValue("@ModifiedByUserId", Session["USERID"].ToString());

                                int rowsAffectedStock = cmdUpdateStock.ExecuteNonQuery();
                                LogSuccessNew($"UPDATE: tbl_stock updated {rowsAffectedStock} row(s) for Product: {productName}");
                            }
                        }
                        else
                        {
                            LogError($"UPDATE ERROR: Product '{productName}' not found in tbl_stock.");
                        }
                    }

                    // Commit transaction if everything succeeds
                    transaction.Commit();
                    transaction = null; // Prevents rollback from executing later

                    // UI updates
                    PanelOK.Visible = true;
                    lblOk.Text = "UPDATE: Product and stock updated successfully!";
                }
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception rollbackEx)
                    {
                        LogError($"UPDATE ROLLBACK ERROR: {rollbackEx.Message}");
                    }
                }

                LogError($"UPDATE ERROR: {ex.Message} | StackTrace: {ex.StackTrace}");
                lblOk.Text = "Error: " + ex.Message;
                PanelOK.Visible = true;
            }
        }


        private int GetColumnIndex(string columnName)
        {
            foreach (DataControlField column in GridView1.Columns)
            {
                if (column.HeaderText == columnName)
                {
                    return GridView1.Columns.IndexOf(column);
                }
            }
            return -1;
        }

        //private int GetColumnIndex(string columnName)
        //{
        //    foreach (DataControlField column in GridView1.Columns)
        //    {
        //        if (column.HeaderText == columnName)
        //        {
        //            return GridView1.Columns.IndexOf(column);
        //        }
        //    }
        //    return -1;
        //}


        private void LogError(string message)
        {
            try
            {
                string logFolder = Server.MapPath("~/Uploads/Logs/");
                if (!Directory.Exists(logFolder))
                {
                    Directory.CreateDirectory(logFolder);
                }

                string logFilePath = Path.Combine(logFolder, "ErrorLog.txt");
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine(new string('-', 50)); // Separator for readability
                    writer.WriteLine($"{DateTime.Now}: {message}");
                    writer.WriteLine(new string('-', 50)); // Separator for readability
                }
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error: " + ex.Message;
                PanelError.Visible = true;
            }
        }

        private void LogSuccessNew(string message)
        {
            try
            {
                string logFolder = Server.MapPath("~/Uploads/Logs/");
                if (!Directory.Exists(logFolder))
                {
                    Directory.CreateDirectory(logFolder);
                }

                string logFilePath = Path.Combine(logFolder, "UpdateLogs.txt");
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine(new string('-', 50)); // Separator for readability
                    writer.WriteLine($"{DateTime.Now}: {message}");
                    writer.WriteLine(new string('-', 50)); // Separator for readability
                }
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error: " + ex.Message;
                PanelError.Visible = true;
            }
        }


        //-------It will update matching records against a key and add new records which are NOT present in DB ---- OLD
        public void UpsertDataInDatabase_Old()
        {
            try
            {
                string connString = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    foreach (GridViewRow row in GridView1.Rows)
                    {
                        string productName = row.Cells[GetColumnIndex("Product Name")].Text;
                        string quantity = row.Cells[GetColumnIndex("Quantity")].Text;
                        string unit = row.Cells[GetColumnIndex("Unit")].Text;
                        string rate = row.Cells[GetColumnIndex("Rate")].Text;
                        string amount = row.Cells[GetColumnIndex("Amount")].Text;

                        string checkQuery = "SELECT COUNT(*) FROM tbl_NewProduct WHERE ProductName = @ProductName";
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@ProductName", productName);
                            int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                            if (count > 0)
                            {
                                UpdateProduct(conn, productName, quantity, unit, rate, amount);
                            }
                            else
                            {
                                // Product does not exist, insert it
                                InsertProduct(conn, productName, quantity, unit, rate, amount);
                            }
                        }
                    }
                }

                LogError("Upsert operation completed.");
                HideElements();
            }
            catch (Exception ex)
            {
                LogError($"UPSERT : Error upserting data: {ex.Message} | StackTrace: {ex.StackTrace}");
            }
        }

        //-------It will update matching records against a key and add new records which are NOT present in DB----- NEW
        public void UpsertDataInDatabase()
        {
            List<string> logEntries = new List<string>(); // Log collector

            try
            {
                string connString = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    int rowCount = 1; // Start row count from 1

                    foreach (GridViewRow row in GridView1.Rows)
                    {
                        string productName = row.Cells[GetColumnIndex("Product Name")].Text;
                        string quantity = row.Cells[GetColumnIndex("Quantity")].Text;
                        string unit = row.Cells[GetColumnIndex("Unit")].Text;
                        string rate = row.Cells[GetColumnIndex("Rate")].Text;
                        string amount = row.Cells[GetColumnIndex("Amount")].Text;

                        logEntries.Add($"Row {rowCount}: Processing Product - {productName}, Quantity: {quantity}, Rate: {rate}");

                        string checkQuery = "SELECT COUNT(*) FROM tbl_NewProduct WHERE ProductName = @ProductName";
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@ProductName", productName);
                            int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                            if (count > 0)
                            {
                                logEntries.Add($"Row {rowCount}: Updating Product - {productName}");
                                UpdateProduct(conn, productName, quantity, unit, rate, amount);
                                logEntries.Add($"Row {rowCount}: Product Updated Successfully.");
                            }
                            else
                            {
                                logEntries.Add($"Row {rowCount}: Inserting New Product - {productName}");
                                InsertProduct(conn, productName, quantity, unit, rate, amount);
                                logEntries.Add($"Row {rowCount}: Product Inserted Successfully.");
                            }
                        }

                        rowCount++; // Increment row count
                    }
                }

                // Save the full log at the end
                LogSuccess(string.Join("\n", logEntries));
                HideElements();
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error: " + ex.Message;
                PanelError.Visible = true;
                logEntries.Add($"UPSERT ERROR: {ex.Message}");
                LogError(string.Join("\n", logEntries));
            }
        }

        public void LogSuccess(string logMessage, string logFileName = "UpsertLog")
        {
            try
            {
                string logDirectory = Server.MapPath("~/Logs"); // Change path if needed

                // Ensure the directory exists
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // Create log file with timestamp
                string filePath = Path.Combine(logDirectory, $"{logFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.log");

                // Write the log message
                File.AppendAllText(filePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {logMessage}\n");

                Console.WriteLine($"✅ Log saved: {filePath}"); // Debug message (can remove in production)
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error: " + ex.Message;
                PanelError.Visible = true;
                Console.WriteLine($"❌ Error writing log: {ex.Message}");
            }
        }



        private void HideElements()
        {
            Row_Uploader.Visible = false;
            Row_XMLType.Visible = false;
            Row_UploaderBtns.Visible = false;
            Row_GridView.Visible = false;
            Row2_Gridview.Visible = false;

            Panel1.Visible = false;
            row1.Visible = false;
            row2.Visible = false;
            row3.Visible = false;
            row4.Visible = false;
            Row_Panel1.Visible = false;
            GridView1.Visible = false;
        }


        private void UpdateProductOld(SqlConnection conn, string productName, string quantity, string unit, string rate, string amount)
        {
            string updateQuery = @"UPDATE tbl_NewProduct SET Quantity = @Quantity, Unit = @Unit, Sail_Rate = @SailRate WHERE ProductName = @ProductName";

            using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
            {
                cmd.Parameters.AddWithValue("@ProductName", productName);
                cmd.Parameters.AddWithValue("@Quantity", quantity);
                cmd.Parameters.AddWithValue("@Unit", unit);
                cmd.Parameters.AddWithValue("@SailRate", rate);
                cmd.Parameters.AddWithValue("@ModifiedByUserId", Session["USERID"].ToString());
                cmd.Parameters.AddWithValue("@ModifiedOn", DateTime.Now);

                cmd.ExecuteNonQuery();
                LogSuccessNew($"UPSERT : Updated product: {productName}, Quantity: {quantity}, Unit: {unit}, Purches_Rate: {rate}, Sail_Rate: {amount}");
            }
        }

        private void UpdateProduct(SqlConnection conn, string productName, string quantity, string unit, string rate, string amount)
        {
            string productCode = string.Empty;
            string productOrServiceCat = cmbproduct_service.SelectedItem?.Text ?? "DefaultCategory";
            int parentId = (cmbproduct_service.SelectedItem != null && !string.IsNullOrEmpty(cmbproduct_service.SelectedValue) && cmbproduct_service.SelectedValue != "--Select--") ? Convert.ToInt32(cmbproduct_service.SelectedValue) : (Session["pid"] != null ? Convert.ToInt32(Session["pid"]) : 0);
            string saleRate = string.IsNullOrEmpty(rate) ? "0" : rate;

            string taxRate = "18";
            //string productCategory = "DefaultCategory";
            //string type = "Product";
            //string brand = "DefaultBrand";
            string specification = string.Empty;
            string qty = string.IsNullOrEmpty(quantity) ? "0" : quantity;
            //string moqValue = "1";
            string saleNote = string.Empty;

            string updateQueryProduct = @"UPDATE tbl_NewProduct SET Sail_Rate = @SaleRate,  Unit = @Unit, Quantity = @Quantity, ModifiedByUserId = @ModifiedByUserId, ModifiedOn = GETDATE() WHERE ProductName = @ProductName";

            string updateQueryStock = @"UPDATE tbl_stock SET Quantity = @Quantity, Sail_Rate = @SaleRate, Service_tax_rate = @TaxRate, ModifiedByUserId = @ModifiedByUserId, ModifiedOn = GETDATE() WHERE Product_name = @ProductName";

            if (conn.State != ConnectionState.Open)
            {
                conn.Open();  //Open only if it's not already open
            }

            // Execute update queries within a transaction
            using (SqlTransaction transaction = conn.BeginTransaction())
            {
                try
                {
                    // Execute tbl_NewProduct update
                    using (SqlCommand cmdUpdateProduct = new SqlCommand(updateQueryProduct, conn, transaction))
                    {
                        //cmdUpdateProduct.Parameters.AddWithValue("@ProductCode", productCode);
                        //cmdUpdateProduct.Parameters.AddWithValue("@ProductOrServiceCat", productOrServiceCat);
                        cmdUpdateProduct.Parameters.AddWithValue("@SaleRate", saleRate);
                        //cmdUpdateProduct.Parameters.AddWithValue("@TaxRate", taxRate);
                        //cmdUpdateProduct.Parameters.AddWithValue("@Product_catagory", string.IsNullOrEmpty(productCategory) ? "" : productCategory);
                        cmdUpdateProduct.Parameters.AddWithValue("@ProductName", productName);
                        //cmdUpdateProduct.Parameters.AddWithValue("@Type", type);
                        cmdUpdateProduct.Parameters.AddWithValue("@Unit", unit);
                        //cmdUpdateProduct.Parameters.AddWithValue("@Brand", brand);
                        //cmdUpdateProduct.Parameters.AddWithValue("@ParentId", parentId);
                        //cmdUpdateProduct.Parameters.AddWithValue("@Specification", specification);
                        cmdUpdateProduct.Parameters.AddWithValue("@Quantity", quantity);
                        //cmdUpdateProduct.Parameters.AddWithValue("@MOQ_Value", moqValue);
                        //cmdUpdateProduct.Parameters.AddWithValue("@SaleNote", saleNote);
                        cmdUpdateProduct.Parameters.AddWithValue("@ModifiedByUserId", Session["USERID"].ToString());
                        //cmdUpdateProduct.Parameters.AddWithValue("@ModifiedOn", DateTime.Now);
                        //DateTime expiry = new DateTime(2026, 3, 31, 0, 0, 0);
                        //cmdUpdateProduct.Parameters.AddWithValue("@ExpiryDate", expiry);

                        int rowsAffectedProduct = cmdUpdateProduct.ExecuteNonQuery();
                        LogSuccessNew($"UPSERT-UPDATE: tbl_NewProduct updated {rowsAffectedProduct} row(s) for ProductName: {productName}");
                    }

                    // Execute tbl_stock update
                    using (SqlCommand cmdUpdateStock = new SqlCommand(updateQueryStock, conn, transaction))
                    {
                        cmdUpdateStock.Parameters.AddWithValue("@ProductName", productName);
                        cmdUpdateStock.Parameters.AddWithValue("@Quantity", string.IsNullOrEmpty(quantity) ? "0" : quantity);  // Keep as string
                        cmdUpdateStock.Parameters.AddWithValue("@SaleRate", string.IsNullOrEmpty(rate) ? "0" : rate);  // Keep as string
                        cmdUpdateStock.Parameters.AddWithValue("@TaxRate", string.IsNullOrEmpty(taxRate) ? "0" : taxRate);  // Keep as string
                        cmdUpdateStock.Parameters.AddWithValue("@ModifiedByUserId", Session["USERID"].ToString());
                        //cmdUpdateStock.Parameters.AddWithValue("@ModifiedOn", DateTime.Now);

                        int rowsAffectedStock = cmdUpdateStock.ExecuteNonQuery();
                        LogSuccessNew($"UPSERT-UPDATE: tbl_stock updated {rowsAffectedStock} row(s) for ProductName: {productName}");
                    }

                    // Commit transaction if both updates succeed
                    transaction.Commit();
                    PanelOK.Visible = true;
                    lblOk.Text = "UPSERT-Product and stock updated successfully!";
                }
                catch (Exception ex)
                {
                    // Rollback transaction if any error occurs
                    transaction.Rollback();

                    // Log failure
                    LogError($"UPSERT-UPDATE ERROR: Failed to update ProductName: {productName}. Error: {ex.Message}");

                    lblOk.Text = "Error: " + ex.Message;
                    PanelOK.Visible = true;
                }
            }

        }


        private void InsertProductOld(SqlConnection conn, string productName, string quantity, string unit, string rate, string amount)
        {
            string insertQuery = @"INSERT INTO tbl_NewProduct (ProductName, Quantity, Unit, Sail_Rate) VALUES (@ProductName, @Quantity, @Unit, @SailRate)";

            using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
            {
                cmd.Parameters.AddWithValue("@ProductName", productName);
                cmd.Parameters.AddWithValue("@Quantity", quantity);
                cmd.Parameters.AddWithValue("@Unit", unit);
                cmd.Parameters.AddWithValue("@SailRate", rate);

                //cmd.ExecuteNonQuery();
                LogSuccessInsert($"UPSERT : Inserted new product: {productName}, Quantity: {quantity}, Unit: {unit}, Purches_Rate: {rate}, Sail_Rate: {amount}");
            }
        }

        private string findProductId()
        {
            string PurID = "";
            string aa = "";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            string cmdString1 = "SELECT Id, ProductID FROM tbl_NewProduct WHERE Id = (SELECT MAX(Id) FROM tbl_NewProduct)";
            SqlCommand com1 = new SqlCommand(cmdString1, DbCL.Conn);
            SqlDataReader DR1 = com1.ExecuteReader();

            if (DR1.Read())
            {
                aa = DR1.GetValue(1).ToString();

                if (!string.IsNullOrEmpty(aa) && aa.Length >= 3)  // ✅ Ensure it's not null and has at least "PRD" + number
                {
                    int k;
                    string numericPart = aa.Substring(3); // Get numeric part
                    if (int.TryParse(numericPart, out k)) // ✅ Ensure it's a valid number
                    {
                        k = k + 1; // Increment
                        PurID = "PRD" + k.ToString("D4"); // ✅ Ensure it's always 4-digit format (e.g., PRD0001, PRD0023)
                    }
                    else
                    {
                        LogError($"findProductId(): Invalid numeric part in ProductID: '{aa}'");
                        PurID = "PRD0001"; // ✅ Fallback to safe value
                    }
                }
                else
                {
                    LogError("findProductId(): ProductID is too short or null.");
                    PurID = "PRD0001"; // ✅ Start fresh
                }
            }
            else
            {
                PurID = "PRD0001"; // ✅ If no records exist
            }

            DbCL.Conn.Close();
            return PurID;
        }

        private string findProductId_Old()
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

        private void InsertProduct(SqlConnection conn, string productName, string quantity, string unit, string rate, string amount)
        {
            // Declare local variables with correct data type handling
            string productCode = string.Empty;  // No product code passed in parameters
            string productOrServiceCat = cmbproduct_service.SelectedItem?.Text ?? "DefaultCategory"; // Assign default if no selection

            // Ensure parentId is assigned based on cmbproduct_service value
            int parentId = (cmbproduct_service.SelectedItem != null
                            && !string.IsNullOrEmpty(cmbproduct_service.SelectedValue)
                            && cmbproduct_service.SelectedValue != "--Select--")
                            ? Convert.ToInt32(cmbproduct_service.SelectedValue)
                            : (Session["pid"] != null ? Convert.ToInt32(Session["pid"]) : 0);

            // Sale Rate and Tax Rate should be stored as strings (as per DB schema)
            string saleRate = string.IsNullOrEmpty(rate) ? "0.00" : rate;
            string taxRate = "18";

            string productCategory = string.Empty;
            string type = "Product";
            string brand = string.IsNullOrWhiteSpace(txt_ProductBrand.Text) ? string.Empty : txt_ProductBrand.Text;
            string specification = string.Empty; // Default empty if no specification

            // Quantity and MOQ should be stored as strings (as per DB schema)
            string qty = string.IsNullOrEmpty(quantity) ? "0" : quantity;
            string moqValue = "1"; // Default MOQ if not provided
            string saleNote = string.Empty; // Default empty if missing

            SqlTransaction transaction = null;
            try
            {
                // Generate Product ID
                string productId = findProductId();

                // Start a new transaction
                transaction = conn.BeginTransaction();

                // SQL query for tbl_NewProduct
                string queryNewProduct = @"INSERT INTO tbl_NewProduct (Product_code, ProductOrServiceCat, Sail_Rate, Tax_Rate, Product_catagory, ProductName, Type, Unit, Brand, parentId, Specification, Quantity, MOQ_Value, SaleNote, ExpiryDate, TimeStamp, ProductID, AddedbyUserId, AddedOn) VALUES (@ProductCode, @ProductOrServiceCat, @SaleRate, @TaxRate, @Product_catagory, @ProductName, @Type, @Unit, @Brand, @ParentId, @Specification, @Quantity, @MOQ_Value, @SaleNote, @ExpiryDate, GETDATE(), @ProductID, @AddedbyUserId, @AddedOn)";

                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();  // ✅ Open only if it's not already open
                }

                using (SqlCommand cmdNewProduct = new SqlCommand(queryNewProduct, conn, transaction))
                {
                    cmdNewProduct.Parameters.AddWithValue("@ProductCode", productCode);
                    cmdNewProduct.Parameters.AddWithValue("@ProductOrServiceCat", productOrServiceCat);
                    cmdNewProduct.Parameters.AddWithValue("@SaleRate", saleRate); // Storing as varchar
                    cmdNewProduct.Parameters.AddWithValue("@TaxRate", taxRate); // Storing as varchar
                    cmdNewProduct.Parameters.AddWithValue("@Product_catagory", productCategory);
                    cmdNewProduct.Parameters.AddWithValue("@ProductName", productName);
                    cmdNewProduct.Parameters.AddWithValue("@Type", type);
                    cmdNewProduct.Parameters.AddWithValue("@Unit", unit);
                    cmdNewProduct.Parameters.AddWithValue("@Brand", brand);
                    cmdNewProduct.Parameters.AddWithValue("@ParentId", parentId);
                    cmdNewProduct.Parameters.AddWithValue("@Specification", specification);
                    cmdNewProduct.Parameters.AddWithValue("@Quantity", qty); // Storing as varchar
                    cmdNewProduct.Parameters.AddWithValue("@MOQ_Value", moqValue); // Storing as varchar
                    cmdNewProduct.Parameters.AddWithValue("@SaleNote", saleNote);
                    cmdNewProduct.Parameters.AddWithValue("@ProductID", productId);
                    cmdNewProduct.Parameters.AddWithValue("@AddedbyUserId", Session["USERID"].ToString());
                    cmdNewProduct.Parameters.AddWithValue("@AddedOn", DateTime.Now);

                    // Fix Expiry Date format
                    DateTime expiry = new DateTime(2026, 3, 31, 0, 0, 0);
                    cmdNewProduct.Parameters.AddWithValue("@ExpiryDate", expiry);

                    // Execute tbl_NewProduct Insert
                    cmdNewProduct.ExecuteNonQuery();
                }

                // SQL query for tbl_stock
                string queryStock = @"INSERT INTO tbl_stock (Product_id, Product_name, Quantity, Sail_Rate, Service_tax_rate) VALUES (@ProductID, @ProductName, @Quantity, @SaleRate, @TaxRate)";

                using (SqlCommand cmdStock = new SqlCommand(queryStock, conn, transaction))
                {
                    cmdStock.Parameters.AddWithValue("@ProductID", productId);
                    cmdStock.Parameters.AddWithValue("@ProductName", productName);
                    cmdStock.Parameters.AddWithValue("@Quantity", string.IsNullOrEmpty(quantity) ? "0" : quantity);  // Keep as string
                    cmdStock.Parameters.AddWithValue("@SaleRate", string.IsNullOrEmpty(rate) ? "0" : rate);  // Keep as string
                    cmdStock.Parameters.AddWithValue("@TaxRate", string.IsNullOrEmpty(taxRate) ? "0" : taxRate);  // Keep as string

                    // Execute tbl_stock Insert
                    cmdStock.ExecuteNonQuery();
                }

                // Commit transaction if both insertions succeed
                transaction.Commit();

                LogSuccessInsert($"UPSERT: Inserted new product: {productName}, Product Code: {productCode}, Quantity: {quantity}, Unit: {unit}, Sale Rate: {saleRate}, Tax Rate: {taxRate}");
            }
            catch (Exception ex)
            {
                // Rollback the transaction if an error occurs
                transaction?.Rollback();
                LogError("UPSERT-Error in InsertProduct: " + ex.Message);
            }
        }

        private void LogSuccessInsert(string message)
        {
            try
            {
                string logFolder = Server.MapPath("~/Uploads/Logs/");
                if (!Directory.Exists(logFolder))
                {
                    Directory.CreateDirectory(logFolder);
                }

                string logFilePath = Path.Combine(logFolder, "InsertLogs.txt");
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine(new string('-', 50)); // Separator for readability
                    writer.WriteLine($"{DateTime.Now}: {message}");
                    writer.WriteLine(new string('-', 50)); // Separator for readability
                }
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error: " + ex.Message;
                PanelError.Visible = true;
            }
        }


        private void LogSuccess_old(string message, List<string> details)
        {
            try
            {
                string logFolder = Server.MapPath("~/Uploads/Logs/");
                if (!Directory.Exists(logFolder))
                {
                    Directory.CreateDirectory(logFolder);
                }

                string logFilePath = Path.Combine(logFolder, "ReadSuccessLog.txt");
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine(new string('-', 50)); // Separator for readability
                    writer.WriteLine($"{DateTime.Now}: {message}");
                    foreach (string detail in details)
                    {
                        writer.WriteLine($"    {detail}");
                    }
                    writer.WriteLine(new string('-', 50)); // Separator for readability
                }
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error: " + ex.Message;
                PanelError.Visible = true;
            }
        }

        private void LogSuccess(string message, List<string> details, string customFileName, ref string logFilePath, ref string flnm)
        {

            try
            {
                string logFolder = Server.MapPath("~/Uploads/Logs/");
                flnm = customFileName + ".txt";
                if (!Directory.Exists(logFolder))
                {
                    Directory.CreateDirectory(logFolder);
                }

                logFilePath = Path.Combine(logFolder, flnm);

                // Write log data
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine(new string('-', 50)); // Separator for readability
                    writer.WriteLine($"{DateTime.Now}: {message}");
                    foreach (string detail in details)
                    {
                        writer.WriteLine($"    {detail}");
                    }
                    writer.WriteLine(new string('-', 50)); // Separator for readability
                }

                // Trigger file download
                //DownloadFile(logFilePath);
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error: " + ex.Message;
                PanelError.Visible = true;
            }
        }

        // Function to trigger file download
        private void DownloadFile(string filePath)
        {
            //FileInfo file = new FileInfo(filePath);
            //if (file.Exists)
            //{
            //    Response.Clear();
            //    Response.ContentType = "text/plain"; // Set appropriate content type
            //    Response.AppendHeader("Content-Disposition", $"attachment; filename={file.Name}");
            //    Response.WriteFile(file.FullName);
            //    Response.Flush();
            //    Response.End();
            //}

            try
            {
                FileInfo file = new FileInfo(filePath);
                if (file.Exists)
                {
                    Response.Clear();
                    Response.ContentType = "application/octet-stream";
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + file.Name);
                    Response.WriteFile(file.FullName);
                    Response.Flush();

                    // Instead of Response.End(), use CompleteRequest()
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    Response.Write("<script>alert('File not found!');</script>");
                }
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error: " + ex.Message;
                PanelError.Visible = true;
                Response.Write("<script>alert('Error downloading file: " + ex.Message + "');</script>");
            }
        }




        private void LoadXMLData_Old(string xmlPath)
        {
            XDocument xdoc = XDocument.Load(xmlPath);

            DataTable dt = new DataTable();
            dt.Columns.Add("Product Name");
            dt.Columns.Add("Quantity");
            dt.Columns.Add("Unit");
            dt.Columns.Add("Rate");
            dt.Columns.Add("Amount");

            var items = xdoc.Descendants("DSPACCNAME").Zip(
                xdoc.Descendants("DSPSTKCL"),
                (name, stock) => new
                {
                    ProductName = name.Element("DSPDISPNAME")?.Value,
                    Quantity = stock.Element("DSPCLQTY")?.Value,
                    Rate = stock.Element("DSPCLRATE")?.Value,
                    Amount = stock.Element("DSPCLAMTA")?.Value
                }
            );

            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.ProductName))
                {
                    string[] qtyParts = item.Quantity.Split(' ');
                    string quantity = qtyParts[0];
                    string unit = qtyParts.Length > 1 ? qtyParts[1] : "";

                    dt.Rows.Add(item.ProductName, quantity, unit, item.Rate, item.Amount);
                }
            }

            GridView1.DataSource = dt;
            GridView1.DataBind();
        }

        protected void btnUpload_Click_Old(object sender, EventArgs e)
        {
            if (fileUploader.HasFile)
            {
                string uploadFolder = Server.MapPath("~/Uploads");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                string filePath = Path.Combine(uploadFolder, Path.GetFileName(fileUploader.FileName));
                fileUploader.SaveAs(filePath);

                // Process the file based on extension
                List<Product> allProducts = new List<Product>();
                string extension = Path.GetExtension(filePath).ToLower();

                if (extension == ".xml")
                {
                    allProducts.AddRange(ParseXML(File.ReadAllText(filePath)));
                }
                else if (extension == ".csv" || extension == ".txt")
                {
                    allProducts.AddRange(ParseCSV(filePath));
                }

                GridView1.DataSource = allProducts;
                GridView1.DataBind();
            }
            else
            {
                // Show error message if no file is selected
            }
        }

        private void ProcessFile(string filePath)
        {
            List<Product> allProducts = new List<Product>();
            string extension = Path.GetExtension(filePath).ToLower();

            if (extension == ".xml")
            {
                string xmlData = File.ReadAllText(filePath);
                allProducts.AddRange(ParseXML(xmlData));
            }
            else if (extension == ".csv" || extension == ".txt")
            {
                allProducts.AddRange(ParseCSV(filePath));
            }

            GridView1.DataSource = allProducts;
            GridView1.DataBind();
        }

        private void BindGridView()
        {
            string filePath = Server.MapPath("~/D:/StkGrpSum.xml"); // Update with actual path
            //string filePath = Server.MapPath("~/Uploads/StkGrpSum.xml");
            List<Product> allProducts = new List<Product>();

            if (!File.Exists(filePath))
            {
                // Handle the missing file scenario
                Console.WriteLine("File not found: " + filePath);
                return;
            }

            string extension = Path.GetExtension(filePath).ToLower();

            try
            {
                if (extension == ".xml")
                {
                    allProducts.AddRange(ParseXML(File.ReadAllText(filePath)));
                }
                else if (extension == ".csv" || extension == ".txt") // Assuming .txt might also be CSV formatted
                {
                    allProducts.AddRange(ParseCSV(filePath));
                }
                else
                {
                    Console.WriteLine("Unsupported file format: " + extension);
                    return;
                }

                GridView1.DataSource = allProducts;
                GridView1.DataBind();
            }
            catch (Exception ex)
            {
                lblErrorMsg.Text = "Error: " + ex.Message;
                PanelError.Visible = true;
                Console.WriteLine("Error processing file: " + ex.Message);
            }
        }


        // ✅ Method to Bind Data to GridView
        //private void BindGridView()
        //{
        //    string csvFilePath = Server.MapPath("~/D:/StkGrpSum.xml"); // Update with actual path

        //    List<Product> allProducts = new List<Product>();
        //    allProducts.AddRange(ParseXML(csvFilePath));
        //    allProducts.AddRange(ParseCSV(csvFilePath));

        //    GridView1.DataSource = allProducts;
        //    GridView1.DataBind();
        //}

        // ✅ Product Class
        public class Product
        {
            public string Name { get; set; }
            public string Quantity { get; set; }
            public string Unit { get; set; }
            public string Rate { get; set; }
            public string Amount { get; set; }
        }

        // ✅ Extract Quantity & Unit using Regex
        private void ExtractQuantityAndUnit_Old(string input, ref string quantity, ref string unit)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                quantity = "";
                unit = "";
                return;
            }

            Match match = Regex.Match(input, @"([\d\.]+)\s*([A-Za-z]*)");
            quantity = match.Groups[1].Value;
            unit = match.Groups[2].Success ? match.Groups[2].Value : "";
        }


        // ✅ Parse XML
        private List<Product> ParseXML(string xmlData)
        {
            XDocument xdoc = XDocument.Parse(xmlData);
            var products = new List<Product>();

            foreach (var p in xdoc.Descendants("Product"))
            {
                string quantity = "", unit = "";

                // Extract quantity and unit using ref
                ExtractQuantityAndUnit(p.Element("DSPCLQTY")?.Value, ref quantity, ref unit);

                products.Add(new Product
                {
                    Name = p.Element("ProductName")?.Value,
                    Quantity = quantity,
                    Unit = unit,
                    Rate = p.Element("Rate")?.Value,
                    Amount = p.Element("Amount")?.Value
                });
            }

            return products;
        }


        // ✅ Parse CSV
        private List<Product> ParseCSV(string filePath)
        {
            var products = new List<Product>();
            if (!File.Exists(filePath))
            {
                return products;
            }

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length < 4) continue;

                    string name = parts[0].Trim('"');
                    string quantity = "", unit = "";

                    // Extract quantity and unit using ref
                    ExtractQuantityAndUnit(parts[1], ref quantity, ref unit);

                    string rate = parts[2].Trim();
                    string amount = parts[3].Trim();

                    products.Add(new Product
                    {
                        Name = name,
                        Quantity = quantity,
                        Unit = unit,
                        Rate = rate,
                        Amount = amount
                    });
                }
            }

            return products;
        }

        private void ExtractQuantityAndUnit(string input, ref string quantity, ref string unit)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                quantity = "";
                unit = "";
                return;
            }

            Match match = Regex.Match(input, @"([\d\.]+)\s*([A-Za-z]*)");
            quantity = match.Groups[1].Value;
            unit = match.Groups[2].Success ? match.Groups[2].Value : "";
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            UpdateDataInDatabase();
        }

        //-------UPSERT Button Click Event
        //-------It will update matching records against a key and add new records which are NOT present in DB
        protected void btnUpsert_Click(object sender, EventArgs e)
        {
            UpsertDataInDatabase();
        }

        protected void btn_GstData_Update_Click(object sender, EventArgs e)
        {
            UpdateGSTDataInDatabase();
        }

        public void UpdateGSTDataInDatabase()
        {
            // UI updates
            PanelOK.Visible = false;
            lblOk.Text = "";

            SqlTransaction transaction = null;

            try
            {
                string connString = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    transaction = conn.BeginTransaction(); // Start transaction

                    // Get selected column mappings
                    string selectedProductColumn = ddl_ProductName.SelectedValue;
                    string selectedHSNColumn = ddl_HSNCode.SelectedValue;
                    string selectedGSTRateColumn = ddl_GSTRate.SelectedValue;

                    foreach (GridViewRow row in GridView2.Rows)
                    {
                        string productName = row.Cells[GetColumnIndex2(selectedProductColumn)].Text;
                        string hsnCode = row.Cells[GetColumnIndex2(selectedHSNColumn)].Text;
                        string gstRate = row.Cells[GetColumnIndex2(selectedGSTRateColumn)].Text;

                        // Remove '%' from gstRate
                        gstRate = gstRate.Replace("%", "").Trim();

                        // First, check if the product exists in tbl_NewProduct
                        string checkQueryProduct = "SELECT COUNT(*) FROM tbl_NewProduct WHERE ProductName = @ProductName";
                        string checkQueryStock = "SELECT COUNT(*) FROM tbl_stock WHERE Product_name = @ProductName";

                        bool productExists = false;
                        bool stockExists = false;

                        using (SqlCommand checkCmd = new SqlCommand(checkQueryProduct, conn, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@ProductName", productName);
                            productExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                        }

                        using (SqlCommand checkCmd = new SqlCommand(checkQueryStock, conn, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@ProductName", productName);
                            stockExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                        }

                        if (productExists)
                        {
                            // Update `tbl_NewProduct`
                            string updateQueryProduct = @"UPDATE tbl_NewProduct SET Product_code = @HSNCode, Tax_Rate = @GSTRate, ModifiedOn = GETDATE(), ModifiedByUserId = @ModifiedByUserId WHERE ProductName = @ProductName";

                            using (SqlCommand cmdUpdateProduct = new SqlCommand(updateQueryProduct, conn, transaction))
                            {
                                cmdUpdateProduct.Parameters.AddWithValue("@ProductName", productName);
                                cmdUpdateProduct.Parameters.AddWithValue("@HSNCode", hsnCode);
                                cmdUpdateProduct.Parameters.AddWithValue("@GSTRate", gstRate);
                                cmdUpdateProduct.Parameters.AddWithValue("@ModifiedByUserId", Session["USERID"].ToString());

                                int rowsAffectedProduct = cmdUpdateProduct.ExecuteNonQuery();
                                LogSuccessNew($"UPDATE: tbl_NewProduct updated {rowsAffectedProduct} row(s) for Product: {productName}");
                            }
                        }
                        else
                        {
                            LogError($"UPDATE ERROR: Product '{productName}' not found in tbl_NewProduct.");
                        }

                        if (stockExists)
                        {
                            // Update `tbl_stock`
                            string updateQueryStock = @"UPDATE tbl_stock 
                                                SET Service_tax_rate = @GSTRate, 
                                                    ModifiedOn = GETDATE(), 
                                                    ModifiedByUserId = @ModifiedByUserId 
                                                WHERE Product_name = @ProductName";

                            using (SqlCommand cmdUpdateStock = new SqlCommand(updateQueryStock, conn, transaction))
                            {
                                cmdUpdateStock.Parameters.AddWithValue("@ProductName", productName);
                                cmdUpdateStock.Parameters.AddWithValue("@GSTRate", gstRate);
                                cmdUpdateStock.Parameters.AddWithValue("@ModifiedByUserId", Session["USERID"].ToString());

                                int rowsAffectedStock = cmdUpdateStock.ExecuteNonQuery();
                                LogSuccessNew($"UPDATE: tbl_stock updated {rowsAffectedStock} row(s) for Product: {productName}");
                            }
                        }
                        else
                        {
                            LogError($"UPDATE ERROR: Product '{productName}' not found in tbl_stock.");
                        }
                    }

                    // Commit transaction if everything succeeds
                    transaction.Commit();
                    transaction = null; // Prevents rollback from executing later

                    // UI updates
                    PanelOK.Visible = true;
                    Row_Panel2.Visible = false;
                    lblOk.Text = "UPDATE: GST details updated successfully!";
                }
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception rollbackEx)
                    {
                        LogError($"UPDATE ROLLBACK ERROR: {rollbackEx.Message}");
                    }
                }

                LogError($"UPDATE ERROR: {ex.Message} | StackTrace: {ex.StackTrace}");
                lblOk.Text = "Error: " + ex.Message;
                PanelOK.Visible = true;
            }
        }


        private int GetColumnIndex2(string columnName)
        {
            for (int i = 0; i < GridView2.Columns.Count; i++)
            {
                if (GridView2.Columns[i].HeaderText.Trim().Equals(columnName.Trim(), StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1; // Return -1 if column not found
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/ImportProducts.aspx");
        }
    }
}