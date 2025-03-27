using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.IO;

namespace Bill_Software.corporate.business.app
{
    public partial class Manual_Invoice : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dtPCat = new DataTable();
        DataTable dtPservice = new DataTable();

        DataTable dtPCat1 = new DataTable();


        private List<string> vatRates;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                DbCL.FillCombo(cmbClient, "select Client_Name from tbl_Client order by Client_Name");
            }
        }

        private void LoadTaxRates()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string vatQuery = "Select Vat_Rate from tbl_Vat_Master";
            SqlCommand vatCmd = new SqlCommand(vatQuery, DbCL.Conn);
            SqlDataReader vatRdr = vatCmd.ExecuteReader();
            vatRates = new List<string> { "NA" };
            while (vatRdr.Read())
            {
                vatRates.Add(vatRdr[0].ToString());
            }
            vatRdr.Close();
            DbCL.Conn.Close();
        }

        private void bindFactoryAddress(string clientcode)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Address1+', '+City+', '+pin+', '+State from tbl_Client where Client_Id='" + clientcode + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            SqlDataReader DR1 = cmd.ExecuteReader();
            while (DR1.Read())
            {
                FactoryAddress.Items.Add(DR1.GetValue(0).ToString());
            }
            DbCL.Conn.Close();
            bindRegAddress(clientcode);
            bindAddress(clientcode);
        }

        private void bindRegAddress(string clientcode)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Address+', '+State+', '+City+', '+pin as regadd from tbl_ClientRegAddress where Client_Id='" + clientcode + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            SqlDataReader DR1 = cmd.ExecuteReader();
            while (DR1.Read())
            {
                FactoryAddress.Items.Add(DR1.GetValue(0).ToString());
            }
            DbCL.Conn.Close();
        }

        private void bindAddress(string clientcode)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select [Address1] +', '+ [Address2]+', '+[city]+', '+[State]+', '+[pin] as address from tbl_Factory where Client_id='" + clientcode + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            SqlDataReader DR1 = cmd.ExecuteReader();
            while (DR1.Read())
            {
                FactoryAddress.Items.Add(DR1.GetValue(0).ToString());
            }
            DbCL.Conn.Close();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            AddLoader();
        }

        private void AddLoader()
        {
            if (cmbClient.SelectedIndex != 0)
            {
                BindclientID();
            }
            else
            {
                cmbClient.Focus();
            }
        }

        private void BindclientID()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Id from tbl_Client where Client_Name='" + cmbClient.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                string clientid = re["Client_Id"].ToString();
                lblclientID.Text = clientid;

                bindFactoryAddress(clientid);
            }
            DbCL.Conn.Close();

        }

        private void BindListitemNew()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select ProductOrServiceCat from tbl_NewparentProduct order by ProductOrServiceCat";
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

        protected void Button2_Click(object sender, EventArgs e)
        {
            BindListitemNew();
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            BindProducts();
            LoadTaxRates();
            DDL_vat_parsentage.Items.AddRange(vatRates.Select(rate => new ListItem(rate)).ToArray());
        }

        private void BindProducts()
        {
            DataTable dtproductWithCat = new DataTable();
            string cmdstring = "select * from tbl_NewProduct where ProductOrServiceCat=@ProductOrServiceCat order by Type,ProductName";

            SqlParameter[] pram = {
                new SqlParameter("@ProductOrServiceCat",cmbproduct_service.Text)
            };
            dtproductWithCat = DbCL.SPreturn_dt(cmdstring, pram);
            if (dtproductWithCat.Rows.Count > 0)
            {
                gridProdWithCat.DataSource = dtproductWithCat;
                gridProdWithCat.DataBind();
                ViewState["dtprocat"] = dtproductWithCat;
            }
        }

        protected void btnAddProduct_Click(object sender, EventArgs e)
        {
            ProductsBinderforTaxInv();
        }

        protected void cmbClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            AddLoader();
            BindProducts();
        }

        protected void cmbproduct_service_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindProducts();
        }


        private void ProductsBinderforTaxInv()
        {
            gridProdWithCat.Visible = true;
            if (ViewState["dtprocat"] != null)
            {
                DataTable dtpro = new DataTable();
                dtpro = ViewState["dtprocat"] as DataTable;

                string HSN = ""; //HSN Code
                string ProductId = ""; // Product ID
                string ProductName = "";
                string Brandspecification = "";
                string Type = "";
                string Sail_Rate = "";
                string Tax_Rate = "";
                string Unit = "";
                string Quantity = "";
                //string IQuantity = "";
                string ProductOrServiceCat = "";


                for (int i = 0; i < dtpro.Rows.Count; i++)
                {
                    CheckBox chkdtp = (CheckBox)(gridProdWithCat.Rows[i].FindControl("chkdtp"));
                    if (chkdtp.Checked == true)
                    {
                        ProductId = ((Label)gridProdWithCat.Rows[i].FindControl("ProductID")).Text; // ProductID
                        HSN = ((Label)gridProdWithCat.Rows[i].FindControl("Product_code")).Text; // HSN
                        ProductName = ((Label)gridProdWithCat.Rows[i].FindControl("ProductName")).Text;
                        Brandspecification = ((Label)gridProdWithCat.Rows[i].FindControl("Brand")).Text;
                        Quantity = ((Label)gridProdWithCat.Rows[i].FindControl("Quantity")).Text;
                        //IQuantity = ((Label)gridProdWithCat.Rows[i].FindControl("IQuantity")).Text;
                        Sail_Rate = ((Label)gridProdWithCat.Rows[i].FindControl("Sail_Rate")).Text;
                        Tax_Rate = ((Label)gridProdWithCat.Rows[i].FindControl("Tax_Rate")).Text;
                        Type = ((Label)gridProdWithCat.Rows[i].FindControl("Type")).Text;
                        Unit = ((Label)gridProdWithCat.Rows[i].FindControl("Unit")).Text;
                        ProductOrServiceCat = ((Label)gridProdWithCat.Rows[i].FindControl("ProductOrServiceCat")).Text;

                        if (ViewState["PhaseProductData"] != null)
                        {
                            dtPCat = (DataTable)ViewState["PhaseProductData"];
                            int count = dtPCat.Rows.Count + 1;

                            SearchProductCatwise(count, HSN, ProductId, ProductName, Brandspecification, Quantity, Sail_Rate, Tax_Rate, Type, Unit, ProductOrServiceCat);

                        }
                        else
                        {
                            SearchProductCatwise(1, HSN, ProductId, ProductName, Brandspecification, Quantity, Sail_Rate, Tax_Rate, Type, Unit, ProductOrServiceCat);
                        }
                    }
                }
            }
            //gridProdWithCat.Visible = false;
            //btnAddProduct.Enabled = false;
        }


        private void SearchProductCatwise(int count, string HSN, string ProductId, string ProductName,
                                  string Brandspecification, string Quantity,
                                  string Sail_Rate, string Tax_Rate, string Type, string Unit,
                                  string ProductOrServiceCat)
        {
            DataTable dtPCat = ViewState["PhaseProductData"] as DataTable;

            if (dtPCat == null || count == 1)
            {
                dtPCat = new DataTable();
                dtPCat.Columns.Add("ProductId", typeof(string));
                dtPCat.Columns.Add("Product_code", typeof(string));
                dtPCat.Columns.Add("ProductName", typeof(string));
                dtPCat.Columns.Add("Sail_Rate", typeof(string));
                dtPCat.Columns.Add("Tax_Rate", typeof(string));
                dtPCat.Columns.Add("SQuantity", typeof(string)); // Matches GridView: "Quantity"
                dtPCat.Columns.Add("IQuantity", typeof(string));
                dtPCat.Columns.Add("Brand", typeof(string));
                dtPCat.Columns.Add("Type", typeof(string));
                dtPCat.Columns.Add("Unit", typeof(string));
                dtPCat.Columns.Add("ProductOrServiceCat", typeof(string));
                dtPCat.Columns.Add("ItemNo", typeof(string));   // Added for "Item No"
                dtPCat.Columns.Add("MaterialNo", typeof(string)); // Added for "Material No"
                dtPCat.Columns.Add("PackSize", typeof(string)); // Added for "Pack Size"
                dtPCat.Columns.Add("Discount_Rate", typeof(string)); // Added for "Discount (%)"
                dtPCat.Columns.Add("ItemRemarks", typeof(string)); // Added for "Remarks"
            }

            // Prevent duplicate ProductId
            bool exists = dtPCat.AsEnumerable().Any(row => row["ProductId"].ToString() == ProductId);

            if (!exists)
            {
                DataRow dr = dtPCat.NewRow();
                dr["ProductId"] = ProductId;
                dr["Product_code"] = HSN;
                dr["ProductName"] = ProductName;
                dr["Sail_Rate"] = Sail_Rate;
                dr["Tax_Rate"] = Tax_Rate;
                dr["SQuantity"] = Quantity;
                dr["IQuantity"] = "";
                dr["Brand"] = Brandspecification;
                dr["Type"] = Type;
                dr["Unit"] = Unit;
                dr["ProductOrServiceCat"] = ProductOrServiceCat;
                dr["ItemNo"] = "";  // Initialize to empty; can be updated later
                dr["MaterialNo"] = "";
                dr["PackSize"] = "";
                dr["Discount_Rate"] = "0"; // Default value
                dr["ItemRemarks"] = "";

                dtPCat.Rows.Add(dr);
            }

            // Bind updated DataTable to GridView
            gd_Service_Product.DataSource = dtPCat;
            gd_Service_Product.DataBind();

            // Store updated DataTable in ViewState
            ViewState["PhaseProductData"] = dtPCat;
        }

        protected void btn_finalsave_Click(object sender, EventArgs e)
        {
            FetchSelectedProductsWithCalculations();
        }

        private void FetchSelectedProductsWithCalculations()
        {
            StringBuilder logBuilder = new StringBuilder();
            logBuilder.AppendLine("Invoice Processing Log");
            logBuilder.AppendLine("===================================");
            logBuilder.AppendLine($"Timestamp: {DateTime.Now}");

            List<string> errorMessages = new List<string>();
            List<string> successLogs = new List<string>();
            string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";
            DataTable dtSelectedProducts = new DataTable();

            // Define columns for DataTable
            dtSelectedProducts.Columns.Add("ItemNo", typeof(string));
            dtSelectedProducts.Columns.Add("ProductID", typeof(string));
            dtSelectedProducts.Columns.Add("Product_code", typeof(string));
            dtSelectedProducts.Columns.Add("ProductName", typeof(string));
            dtSelectedProducts.Columns.Add("Brand", typeof(string));
            dtSelectedProducts.Columns.Add("IQuantity", typeof(decimal));
            dtSelectedProducts.Columns.Add("Sail_Rate", typeof(decimal));
            dtSelectedProducts.Columns.Add("Tax_Rate", typeof(decimal));
            dtSelectedProducts.Columns.Add("Discount_Rate", typeof(decimal));
            dtSelectedProducts.Columns.Add("AmountBeforeTax", typeof(decimal));
            dtSelectedProducts.Columns.Add("TaxAmount", typeof(decimal));
            dtSelectedProducts.Columns.Add("DiscountAmount", typeof(decimal));
            dtSelectedProducts.Columns.Add("AmountAfterTax", typeof(decimal));
            dtSelectedProducts.Columns.Add("Remarks", typeof(string));

            string invoiceNo = BindInvoiceNo();
            int serialNo = idreturn() + 1;

            if (string.IsNullOrWhiteSpace(invoiceNo))
            {
                logBuilder.AppendLine("ERROR-0: Invoice number is null or empty.");
                WriteLog(logBuilder.ToString());
                return;
            }

            decimal totalAmountBeforeTax = 0, totalTaxAmount = 0, totalDiscountAmount = 0, totalAmountAfterTax = 0;

            foreach (GridViewRow row in gd_Service_Product.Rows)
            {
                try
                {
                    CheckBox chk = (CheckBox)row.FindControl("chk");
                    if (chk != null && chk.Checked)
                    {
                        string productId = ((Label)row.FindControl("ProductID")).Text;
                        string HSN = ((Label)row.FindControl("Product_code")).Text;
                        string productName = ((Label)row.FindControl("ProductName")).Text;
                        string brand = ((Label)row.FindControl("Brand")).Text;
                        string remarks = ((TextBox)row.FindControl("ItemRemarks")).Text;

                        decimal iQuantity = Convert.ToDecimal(((TextBox)row.FindControl("IQuantity")).Text);
                        decimal sailRate = Convert.ToDecimal(((TextBox)row.FindControl("Sail_Rate")).Text);
                        decimal taxRate = Convert.ToDecimal(((Label)row.FindControl("Tax_Rate")).Text);
                        decimal discountRate = Convert.ToDecimal(((TextBox)row.FindControl("Discount_Rate")).Text);

                        // Ensure valid values to avoid division errors
                        sailRate = sailRate > 0 ? sailRate : 0;
                        taxRate = taxRate > 0 ? taxRate : 0;
                        discountRate = discountRate > 0 ? discountRate : 0;

                        // Calculate values with rounding for better accuracy
                        decimal amountBeforeTax = Math.Round(iQuantity * sailRate, 2);
                        decimal discountAmount = Math.Round((amountBeforeTax * discountRate) / 100, 2);
                        decimal amountAfterDiscount = Math.Round(amountBeforeTax - discountAmount, 2);
                        decimal taxAmount = Math.Round((amountAfterDiscount * taxRate) / 100, 2);
                        decimal amountAfterTax = Math.Round(amountAfterDiscount + taxAmount, 2);

                        //decimal amountBeforeTax = iQuantity * sailRate;
                        //decimal discountAmount = (amountBeforeTax * discountRate) / 100;
                        //decimal amountAfterDiscount = amountBeforeTax - discountAmount;
                        //decimal taxAmount = (amountAfterDiscount * taxRate) / 100;
                        //decimal amountAfterTax = amountAfterDiscount + taxAmount;

                        DataRow dr = dtSelectedProducts.NewRow();
                        dr["ProductID"] = productId;
                        dr["Product_code"] = HSN;
                        dr["ProductName"] = productName;
                        dr["Brand"] = brand;
                        dr["IQuantity"] = iQuantity;
                        dr["Sail_Rate"] = sailRate;
                        dr["Tax_Rate"] = taxRate;
                        dr["Discount_Rate"] = discountRate;
                        dr["AmountBeforeTax"] = amountBeforeTax;
                        dr["DiscountAmount"] = discountAmount;
                        dr["TaxAmount"] = taxAmount;
                        dr["AmountAfterTax"] = amountAfterTax;
                        dr["Remarks"] = remarks;
                        dtSelectedProducts.Rows.Add(dr);

                        totalAmountBeforeTax += amountBeforeTax;
                        totalDiscountAmount += discountAmount;
                        totalTaxAmount += taxAmount;
                        totalAmountAfterTax += amountAfterTax;

                        Session["InvTotalAmountWithGst"] = totalAmountAfterTax;
                        Session["InvTotalAmountWithOutGst"] = totalAmountBeforeTax;
                        Session["invTotalGstAmount"] = totalTaxAmount;

                        string queryDetails = @"INSERT INTO tbl_Invoice_details (Invoice_No, Quotation_no, Product_id, Product_Code, Product_name, Quantity, sail_rate, Service_tax_rate, discountRate, Total_sail_rate1, Total_sail_rate2, specification, AddedById) 
                                        VALUES (@Invoice_No, @Quotation_no, @Product_id, @Product_Code, @Product_name, @Quantity, @sail_rate, @Service_tax_rate, @discountRate, @Total_sail_rate1, @Total_sail_rate2, @specification, @AddedById)";

                        List<SqlParameter> pram = new List<SqlParameter>
                        {
                            new SqlParameter("@Invoice_No", invoiceNo),
                            new SqlParameter("@Quotation_no", "N/A"),
                            new SqlParameter("@Product_id", productId),
                            new SqlParameter("@Product_Code", HSN),
                            new SqlParameter("@Product_name", productName),
                            new SqlParameter("@Quantity", iQuantity),
                            new SqlParameter("@sail_rate", sailRate),
                            new SqlParameter("@Service_tax_rate", taxRate),
                            new SqlParameter("@discountRate", discountRate),
                            new SqlParameter("@Total_sail_rate1", amountAfterTax),
                            new SqlParameter("@Total_sail_rate2", amountBeforeTax),
                            new SqlParameter("@specification", brand),
                            new SqlParameter("@AddedById", userId),
                        };

                        DbCL.SPExecDB(queryDetails, pram.ToArray());
                        logBuilder.AppendLine($"Product {productName}, ProductId {productId}  added to invoice {invoiceNo} successfully.");
                        WriteLog(logBuilder.ToString());
                    }
                }
                catch (Exception ex)
                {
                    logBuilder.AppendLine($"ERROR-1: Processing product row failed - {ex.Message}");
                    WriteLog(logBuilder.ToString());
                }
            }

            try
            {
                //double invTotalWithGst = Session["InvTotalAmountWithGst"] != null ? Convert.ToDouble(Session["InvTotalAmountWithGst"]) : 0;
                //double invTotalWithoutGst = Session["InvTotalAmountWithOutGst"] != null ? Convert.ToDouble(Session["InvTotalAmountWithOutGst"]) : 0;
                //double totalGstAmount = Session["invTotalGstAmount"] != null ? Convert.ToDouble(Session["invTotalGstAmount"]) : 0;
                //double totalNetAmount = Session["NetAmount"] != null ? Convert.ToDouble(Session["NetAmount"]) : 0;
                //double discount = 0;
                //string selectedValue = RadioButtonGst.SelectedValue;
                //totalNetAmount = Math.Round(invTotalWithGst) - discount;

                string selectedValue = RadioButtonGst.SelectedValue;
                // Declare variables before using TryParse
                double invTotalWithGst = 0;
                double invTotalWithoutGst = 0;
                double totalGstAmount = 0;
                double totalNetAmount = 0;
                double discount = 0;

                // Safely retrieve session values and parse them
                double.TryParse(Session["InvTotalAmountWithGst"]?.ToString(), out invTotalWithGst);
                double.TryParse(Session["InvTotalAmountWithOutGst"]?.ToString(), out invTotalWithoutGst);
                double.TryParse(Session["invTotalGstAmount"]?.ToString(), out totalGstAmount);
                double.TryParse(Session["NetAmount"]?.ToString(), out totalNetAmount);
                double.TryParse(Session["DiscountAmount"]?.ToString(), out discount);

                // Calculate net amount correctly
                totalNetAmount = Math.Round(invTotalWithGst - discount, 2);


                decimal tcsAmount = 0.00m, tcsrate = 0.00m, deliveryAmount = 0.00m, otherAmount1 = 0.00m;
                decimal.TryParse(txt_tcs_amnt.Text.Trim(), out tcsAmount);
                decimal.TryParse(txt_tcs_percent.Text.Trim(), out tcsrate);
                decimal.TryParse(txt_delivery_amnt.Text.Trim(), out deliveryAmount);
                decimal.TryParse(txt_othr_amnt.Text.Trim(), out otherAmount1);
                

                if (invTotalWithGst > 0)
                {
                    string query = "INSERT INTO tbl_Invoice (Invoice_No, Invoice_Date, Quotation_No, Quotation_Date, Client_ID, Gross, Net_Amount, Sl_no, Service_Tax1, sub_total, discount, addressfor, status1, status2, TCS_Amount, TCS_Rate, Delivery_Amount, Delivery_Rate, otherAmount1_name, otherAmount1, AddedById, cgstOrsgst, igst, PServiceName) " +
                                   "VALUES (@Invoice_No, @Invoice_Date, @Quotation_No, @Quotation_Date, @Client_ID, @Gross, @Net_Amount, @Sl_no, @Service_Tax1, @sub_total, @discount, @addressfor, 'No', 'Active', @TCS_Amount, @TCS_Rate, @Delivery_Amount, @Delivery_Rate, @otherAmount1_name, @otherAmount1, @AddedById, @cgstOrsgst, @igst, @PServiceName)";

                    SqlParameter[] parameters = {
                        new SqlParameter("@Invoice_No", invoiceNo),
                        new SqlParameter("@Invoice_Date", txtinvoiceDate.Text),
                        new SqlParameter("@Quotation_No", "N/A"),
                        new SqlParameter("@Quotation_Date", string.Empty),
                        new SqlParameter("@Client_ID", lblclientID.Text),
                        new SqlParameter("@Gross", invTotalWithGst),
                        new SqlParameter("@Net_Amount", totalNetAmount),
                        new SqlParameter("@Sl_no", serialNo),
                        new SqlParameter("@Service_Tax1", totalGstAmount),
                        new SqlParameter("@sub_total", invTotalWithoutGst),
                        new SqlParameter("@discount", discount),
                        new SqlParameter("@addressfor", "Corporate office"),
                        new SqlParameter("@TCS_Amount", tcsAmount),
                        new SqlParameter("@TCS_Rate", tcsrate),
                        new SqlParameter("@Delivery_Amount", deliveryAmount),
                        new SqlParameter("@Delivery_Rate", DDL_vat_parsentage.SelectedValue),
                        new SqlParameter("@otherAmount1_name", TextBox1.Text.Trim()),
                        new SqlParameter("@otherAmount1", otherAmount1),
                        new SqlParameter("@AddedById", userId),
                        new SqlParameter("@cgstOrsgst", selectedValue == "1" ? "YES" : (object)DBNull.Value),
                        new SqlParameter("@igst", selectedValue == "0" ? "YES" : (object)DBNull.Value),
                        new SqlParameter("@PServiceName", cmbproduct_service.SelectedItem.Text.ToString())
                    };
                    DbCL.SPExecDB(query, parameters);
                    logBuilder.AppendLine($"Invoice {invoiceNo} inserted successfully.");

                    insertCorRegFacAddress(invoiceNo);
                }
            }
            catch (Exception ex)
            {
                logBuilder.AppendLine($"ERROR-2: Failed to insert invoice - {ex.Message}");
            }
            finally
            {
                WriteLog(logBuilder.ToString());
            }
        }


        private void WriteLog(string logContent)
        {
            try
            {
                // Define the log directory inside Uploads/Logs
                string logDir = HttpContext.Current.Server.MapPath("~/Uploads/InvoiceLogs");

                // Ensure the directory exists
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                // Generate a unique log file name using invoice number and timestamp
                string logFileName = $"InvoiceLog_{DateTime.Now:yyyyMMddHHmmss}.txt";
                string logFilePath = Path.Combine(logDir, logFileName);

                // Write log content to the file
                File.WriteAllText(logFilePath, logContent);

                // Optional: Debugging Console Log
                System.Diagnostics.Debug.WriteLine($"Log file created: {logFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error writing log: {ex.Message}");
            }
        }

        private string BindInvoiceNo()
        {
            string c = cmbClient.Text.Trim();
            string f = c.Substring(0, 1);
            f = "INV/" + f + "/";
            string ss = findmonth();
            f = f + ss;
            int j = idreturn();
            j = j + 1;
            f = f + j.ToString();
            return f;
        }

        private int idreturn()
        {
            string a = null;
            int b = 0;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string date1 = txtinvoiceDate.Text;
            string date2 = date1.Substring(3, 3);
            string date3 = date1.Substring(7, 4);
            string date4, date5, date6;
            if (date2 == "Jan" || date2 == "Feb" || date2 == "Mar")
            {
                date4 = ((Convert.ToInt32(date3) - 1)).ToString();
                date5 = "31-Mar-" + date4;
                date6 = "31-Mar-" + date3;
            }
            else
            {
                date4 = ((Convert.ToInt32(date3) + 1)).ToString();
                date5 = "31-Mar-" + date3;
                date6 = "31-Mar-" + date4;
            }
            string cmdstring = "select Sl_no from tbl_Invoice where ID=(select max(ID) from tbl_Invoice where cast(Invoice_Date as datetime) between '" + date5.ToString() + "' and '" + date6.ToString() + "')";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                a = re["Sl_no"].ToString();
                b = Convert.ToInt32(a);
            }
            else
            {
                b = 0;
            }
            DbCL.Conn.Close();
            return b;

        }

        private string findmonth()
        {
            string MonthName = "-";
            string a = txtinvoiceDate.Text.Substring(3, 3);
            string b = txtinvoiceDate.Text.Substring(9, 2);
            if (a == "Jan" || a == "Feb" || a == "Mar")
            {
                MonthName = (Convert.ToInt32(b) - 1).ToString() + "-" + b + "/";
            }
            else
            {
                MonthName = b + "-" + (Convert.ToInt32(b) + 1).ToString() + "/";
            }
            return MonthName;
        }


        private void insertCorRegFacAddress(string invoice_no)
        {
            int selectedSite = 0;

            string listsite_details = null;
            int slno22 = 1;
            for (int i = 0; i < FactoryAddress.Items.Count; i++)
            {
                if (FactoryAddress.Items[i].Selected)
                {
                    selectedSite = selectedSite + 1;
                    listsite_details = FactoryAddress.Items[i].Text;

                    string query = "insert into tbl_InvSiteAddress(invoice_no,SiteAddress) values (@invoice_no,@SiteAddress)";
                    SqlParameter[] pram = {
                         new SqlParameter("@invoice_no",invoice_no),
                         new SqlParameter("@SiteAddress",listsite_details)
                    };

                    DbCL.SPExecDB(query, pram);
                    slno22 = slno22 + 1;
                }
            }
        }

        private string bindpaymentDetails(string quno)
        {
            string due = "";
            string query = "select Due_amount from tbl_invoice_payment where Quotation_No=@Quotation_No";
            SqlParameter[] pram = {
                new SqlParameter("@Quotation_No",quno)
            };
            SqlDataReader rdr = DbCL.SPReturnRdr(query, pram);
            while (rdr.Read())
            {
                due = rdr["Due_amount"].ToString();
            }
            return due;
        }

    }
}