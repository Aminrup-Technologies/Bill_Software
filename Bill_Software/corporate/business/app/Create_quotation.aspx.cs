using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.UI.WebControls;
using System.Text;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm19 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dtphasetype = new DataTable();
        DataTable dtPhasefees = new DataTable();
        DataTable dtPCat = new DataTable();
        DataTable dtPCat1 = new DataTable();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
                return;
            }

            if (!IsPostBack)
            {
                BindDropdowns();
                txtquotationDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");

                // --- PIPELINE INTEGRATION: Capture the Visit ID ---
                if (Request.QueryString["visitId"] != null)
                {
                    int visitId;
                    if (int.TryParse(Request.QueryString["visitId"], out visitId))
                    {
                        // 1. Store the VisitId in a HiddenField
                        hfVisitId.Value = visitId.ToString();

                        // 2. Fetch the customer from the visit
                        PreFillClientFromVisit(visitId);
                    }
                }
            }
        }

        // Helper method to fetch the Customer from the Sales Visit table
        private void PreFillClientFromVisit(int visitId)
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    // Full-Stack CompanyContext segregation fix: Ensure Visit Report matches Company
                    string query = "SELECT CustomerName FROM tbl_SalesVisitReport WHERE Id = @Id AND CompanyID = @CompanyID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", visitId);
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        conn.Open();
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            string customerName = result.ToString();
                            // Logic to auto-select client dropdown goes here if needed
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle silently or log error
            }
        }

        private void BindDropdowns()
        {
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 1. TENANT-SCOPED RESOURCE: Sales Persons
                string salesQuery = "SELECT Id, (Name + ' [' + User_Id + ']') AS DisplayName FROM tbl_login WHERE IsActive = 1 AND CompanyID = @CompanyID AND (User_Id NOT IN ('admin', 'AT01')) ORDER BY Id";
                using (var cmdSales = new SqlCommand(salesQuery, conn))
                {
                    cmdSales.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    using (SqlDataAdapter daSales = new SqlDataAdapter(cmdSales))
                    {
                        DataTable dtSales = new DataTable();
                        daSales.Fill(dtSales);
                        cmbSalesPerson.DataSource = dtSales;
                        cmbSalesPerson.DataTextField = "DisplayName"; // Shows: Anish Dwivedi [FLM03]
                        cmbSalesPerson.DataValueField = "Id";         // Stores: 3 (The PK)
                        cmbSalesPerson.DataBind();
                        cmbSalesPerson.Items.Insert(0, new ListItem("-- Select Sales Person --", "0"));
                    }
                }

                // 2. ISOLATED RESOURCE: Clients (Strictly Filtered by CompanyContext)
                string clientQuery = "SELECT Client_Id, Client_Name FROM tbl_Client WHERE CompanyID = @CompanyID ORDER BY Id";
                using (SqlCommand cmd = new SqlCommand(clientQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dtClients = new DataTable();
                        da.Fill(dtClients);

                        cmbClient.DataSource = dtClients;
                        cmbClient.DataTextField = "Client_Name";
                        cmbClient.DataValueField = "Client_Id"; // Must be Client_Id, not Client_Name
                        cmbClient.DataBind();
                        cmbClient.Items.Insert(0, new ListItem("--Select--", "0"));
                    }
                }
            }

            // Place of supply
            DbCL.FillCombo(ddlPlaceOfSupply, "Select City_Name from tbl_City order by City_Name asc");
        }

        private void ShowAlert(string message, bool isError)
        {
            PanelGlobalAlert.Visible = true;
            lblGlobalAlert.Text = message;
            PanelGlobalAlert.BackColor = isError ? System.Drawing.Color.FromArgb(255, 238, 238) : System.Drawing.Color.FromArgb(238, 255, 221);
            lblGlobalAlert.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.Green;
        }

        // ================= WIZARD NAVIGATION =================

        protected void btnNext1_Click(object sender, EventArgs e)
        {
            PanelGlobalAlert.Visible = false;

            // ==========================================
            // STRICT SERVER-SIDE VALIDATION
            // ==========================================
            if (cmbClient.SelectedValue == "0" || string.IsNullOrWhiteSpace(cmbClient.SelectedValue))
            {
                ShowAlert("Please select a valid Client before proceeding.", true);
                return;
            }
            if (cmbSalesPerson.SelectedValue == "0" || string.IsNullOrWhiteSpace(cmbSalesPerson.SelectedValue))
            {
                ShowAlert("Please assign a Sales Person before proceeding.", true);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtquotationDate.Text))
            {
                ShowAlert("Quotation Date is mandatory.", true);
                return;
            }
            if (ddlPlaceOfSupply.SelectedValue == "0" || string.IsNullOrWhiteSpace(ddlPlaceOfSupply.SelectedValue))
            {
                ShowAlert("Place of Supply is mandatory.", true);
                return;
            }

            // STRICT CHECK: GST Type
            if (RadioButtonGst.SelectedIndex == -1)
            {
                ShowAlert("Please select a GST Type (CGST/SGST or IGST) before proceeding.", true);
                return;
            }

            // STRICT CHECK: Record Type
            if (!rbQt.Checked && !rbPo.Checked)
            {
                ShowAlert("Please select a Record Type (Quotation or Purchase Order) before proceeding.", true);
                return;
            }

            // ==========================================

            // Existing Reference Details Validation
            if (rbYes.Checked) // Or hdnRefOption.Value == "Yes" depending on your exact markup
            {
                if (string.IsNullOrWhiteSpace(txt_clientrefname.Text) || string.IsNullOrWhiteSpace(txt_clientrefid.Text) || string.IsNullOrWhiteSpace(txt_clientrefdate.Text))
                {
                    ShowAlert("Please fill all reference details, or select 'No' to disable them.", true);
                    return;
                }
            }

            // Existing Duplicate Check
            if (IsPurchaseOrderDuplicate()) return;

            // Proceed to Step 2
            BindListitemNew();
            BindclientID();
            WizardMultiView.ActiveViewIndex = 1;
        }

        protected void btnPrev2_Click(object sender, EventArgs e) { WizardMultiView.ActiveViewIndex = 0; }

        protected void Button2_Click(object sender, EventArgs e)
        {
            PanelCatalogGrid.Visible = true;
            // Full-Stack CompanyContext segregation fix: Added CompanyID to Product search
            string cmdstring = "select Id, Product_code, ProductID, ProductOrServiceCat, Brand, ProductName, Specification, Type,Sail_Rate,Tax_Rate,Unit from tbl_NewProduct where ProductOrServiceCat=@ProductOrServiceCat AND CompanyID = @CompanyID order by Id,ProductName";
            SqlParameter[] pram = {
                new SqlParameter("@ProductOrServiceCat", cmbproduct_service.Text),
                new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID)
            };
            DataTable dtproductWithCat = DbCL.SPreturn_dt(cmdstring, pram);
            if (dtproductWithCat.Rows.Count > 0)
            {
                gridProdWithCat.DataSource = dtproductWithCat;
                gridProdWithCat.DataBind();
                ViewState["dtprocat"] = dtproductWithCat;
            }
        }

        // Step 2 -> Step 3: Add to Cart (APPENDS items)
        protected void btnNext2_Click(object sender, EventArgs e)
        {
            PanelGlobalAlert.Visible = false;
            DataTable dtpro = ViewState["dtprocat"] as DataTable;
            if (dtpro == null) return;
            if (ViewState["PhaseProductData"] == null)
            {
                dtPCat.Columns.Add("ProductId", typeof(string));
                dtPCat.Columns.Add("Product_code", typeof(string));
                dtPCat.Columns.Add("ProductName", typeof(string));
                dtPCat.Columns.Add("Specification", typeof(string));
                dtPCat.Columns.Add("Sail_Rate", typeof(string));
                dtPCat.Columns.Add("Discount_Rate", typeof(string));
                dtPCat.Columns.Add("Tax_Rate", typeof(string));
                dtPCat.Columns.Add("Quantity", typeof(string));
                dtPCat.Columns.Add("Brand", typeof(string));
                dtPCat.Columns.Add("Type", typeof(string));
                dtPCat.Columns.Add("Unit", typeof(string));
                dtPCat.Columns.Add("ItemNo", typeof(string));
                dtPCat.Columns.Add("MaterialNo", typeof(string));
                dtPCat.Columns.Add("PackSize", typeof(string));
                dtPCat.Columns.Add("ItemRemarks", typeof(string));
                dtPCat.Columns.Add("ProductOrServiceCat", typeof(string));
                dtPCat.Columns.Add("DeliveryDate", typeof(string));
                dtPCat.Columns.Add("Department", typeof(string));
                ViewState["PhaseProductData"] = dtPCat;
            }
            else
            {
                dtPCat = (DataTable)ViewState["PhaseProductData"];
            }

            bool itemsAdded = false;
            for (int i = 0; i < gridProdWithCat.Rows.Count; i++)
            {
                CheckBox chkdtp = (CheckBox)gridProdWithCat.Rows[i].FindControl("chkdtp");
                if (chkdtp != null && chkdtp.Checked)
                {
                    DataRow dr = dtPCat.NewRow();
                    dr["ProductId"] = ((Label)gridProdWithCat.Rows[i].FindControl("ProductID")).Text;
                    dr["Product_code"] = ((Label)gridProdWithCat.Rows[i].FindControl("Product_code")).Text;
                    dr["ProductName"] = ((Label)gridProdWithCat.Rows[i].FindControl("ProductName")).Text;
                    dr["Brand"] = ((Label)gridProdWithCat.Rows[i].FindControl("Brand")).Text;
                    dr["Specification"] = ((Label)gridProdWithCat.Rows[i].FindControl("Specification")).Text;
                    dr["Quantity"] = "1";
                    dr["Discount_Rate"] = "0";
                    dr["ItemNo"] = "";
                    dr["MaterialNo"] = "";
                    dr["PackSize"] = "";
                    dr["ItemRemarks"] = "";
                    dr["Sail_Rate"] = ((Label)gridProdWithCat.Rows[i].FindControl("Sail_Rate")).Text;
                    dr["Tax_Rate"] = ((Label)gridProdWithCat.Rows[i].FindControl("Tax_Rate")).Text;
                    dr["Type"] = ((Label)gridProdWithCat.Rows[i].FindControl("Type")).Text;
                    dr["Unit"] = ((Label)gridProdWithCat.Rows[i].FindControl("Unit")).Text;
                    dr["ProductOrServiceCat"] = ((Label)gridProdWithCat.Rows[i].FindControl("ProductOrServiceCat")).Text;
                    dr["DeliveryDate"] = "";
                    dr["Department"] = "";

                    dtPCat.Rows.Add(dr);
                    itemsAdded = true;
                    chkdtp.Checked = false;
                }
            }

            if (itemsAdded || dtPCat.Rows.Count > 0)
            {
                ViewState["PhaseProductData"] = dtPCat;
                gd_Service_Product.DataSource = dtPCat;
                gd_Service_Product.DataBind();
                ToggleGridColumns();

                int count1 = ViewState["pService"] != null ? ((DataTable)ViewState["pService"]).Rows.Count + 1 : 1;
                TakePservice(count1, cmbproduct_service.Text);
                WizardMultiView.ActiveViewIndex = 2; // Move to Cart
            }
            else
            {
                ShowAlert("Please check at least one product before proceeding.", true);
            }
        }

        // ================= CART MANAGEMENT (STEP 3) =================

        private void SaveCartToViewState()
        {
            DataTable dt = ViewState["PhaseProductData"] as DataTable;
            if (dt == null) return;

            for (int i = 0; i < gd_Service_Product.Rows.Count; i++)
            {
                GridViewRow row = gd_Service_Product.Rows[i];
                if (row.RowType == DataControlRowType.DataRow)
                {
                    dt.Rows[i]["Quantity"] = ((TextBox)row.FindControl("Quantity")).Text;
                    dt.Rows[i]["Sail_Rate"] = ((TextBox)row.FindControl("Sail_Rate")).Text;
                    dt.Rows[i]["Discount_Rate"] = ((TextBox)row.FindControl("Discount_Rate")).Text;
                    dt.Rows[i]["Specification"] = ((TextBox)row.FindControl("Specification")).Text;
                    dt.Rows[i]["Brand"] = ((TextBox)row.FindControl("Brand")).Text;
                    dt.Rows[i]["ItemNo"] = ((TextBox)row.FindControl("ItemNo"))?.Text ?? "";
                    dt.Rows[i]["MaterialNo"] = ((TextBox)row.FindControl("MaterialNo"))?.Text ?? "";
                    dt.Rows[i]["PackSize"] = ((TextBox)row.FindControl("PackSize"))?.Text ?? "";
                    dt.Rows[i]["ItemRemarks"] = ((TextBox)row.FindControl("ItemRemarks")).Text;
                    if (!rbQt.Checked)
                    {
                        dt.Rows[i]["DeliveryDate"] = ((TextBox)row.FindControl("DeliveryDate")).Text;
                        dt.Rows[i]["Department"] = ((TextBox)row.FindControl("Department")).Text;
                    }
                }
            }
            ViewState["PhaseProductData"] = dt;
        }

        protected void btnAddMoreProducts_Click(object sender, EventArgs e)
        {
            SaveCartToViewState();
            WizardMultiView.ActiveViewIndex = 1; // Back to Step 2
        }

        protected void gd_Service_Product_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            SaveCartToViewState();
            DataTable dt = (DataTable)ViewState["PhaseProductData"];
            int index = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "MoveUp" && index > 0)
            {
                DataRow dr = dt.NewRow();
                dr.ItemArray = dt.Rows[index].ItemArray;
                dt.Rows.RemoveAt(index);
                dt.Rows.InsertAt(dr, index - 1);
            }
            else if (e.CommandName == "MoveDown" && index < dt.Rows.Count - 1)
            {
                DataRow dr = dt.NewRow();
                dr.ItemArray = dt.Rows[index].ItemArray;
                dt.Rows.RemoveAt(index);
                dt.Rows.InsertAt(dr, index + 1);
            }
            else if (e.CommandName == "DeleteRow")
            {
                dt.Rows.RemoveAt(index);
            }

            ViewState["PhaseProductData"] = dt;
            gd_Service_Product.DataSource = dt;
            gd_Service_Product.DataBind();
            ToggleGridColumns();
            ClientScript.RegisterStartupScript(this.GetType(), "recalc", "calculateCart();", true);
        }

        protected void btnPrev3_Click(object sender, EventArgs e)
        {
            SaveCartToViewState();
            WizardMultiView.ActiveViewIndex = 1;
        }

        protected void btnNext3_Click(object sender, EventArgs e)
        {
            PanelGlobalAlert.Visible = false;
            SaveCartToViewState();
            DataTable currentCart = (DataTable)ViewState["PhaseProductData"];

            if (currentCart == null || currentCart.Rows.Count == 0)
            {
                ShowAlert("Cart is empty! Add products to proceed.", true);
                return;
            }

            bindphaseType();
            WizardMultiView.ActiveViewIndex = 3;
        }

        protected void btnPrev4_Click(object sender, EventArgs e) { WizardMultiView.ActiveViewIndex = 2; }


        // ================= DB UTILITIES =================

        private void BindclientID()
        {
            string query = "SELECT Client_Id FROM tbl_Client WHERE Client_Name = @ClientName AND CompanyID = @CompanyID";
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ClientName", cmbClient.Text);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null) lblclientID.Text = result.ToString();
                }
            }
        }

        private void BindListitemNew()
        {
            string cmdstring = "select ProductOrServiceCat from tbl_NewparentProduct WHERE CompanyID = @CompanyID order by ProductOrServiceCat";
            cmbproduct_service.Items.Clear();
            cmbproduct_service.Items.Add("--Select--");
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(cmdstring, con))
            {
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                con.Open();
                using (SqlDataReader re = cmd.ExecuteReader())
                {
                    while (re.Read()) cmbproduct_service.Items.Add(re.GetValue(0).ToString());
                }
            }
        }

        // --- INJECTION 1: Duplicate validation scoped by CompanyID ---
        private bool IsPurchaseOrderDuplicate()
        {
            if (!rbPo.Checked) return false;
            DateTime poDate;
            if (!DateTime.TryParse(txb_podate.Text.Trim(), out poDate))
            {
                ShowAlert("Invalid PO Date format.", true);
                return true;
            }

            string query = @"SELECT COUNT(*) FROM tbl_Quotation WHERE DO_Number = @DO_Number AND PO_Number = @PO_Number AND CONVERT(DATE, PO_Date, 106) = @PO_Date AND RecordType = 'Purchase Order' AND CompanyID = @CompanyID";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@DO_Number", txb_donumber.Text.Trim());
                cmd.Parameters.AddWithValue("@PO_Number", txb_ponumber.Text.Trim());
                cmd.Parameters.AddWithValue("@PO_Date", poDate.ToString("dd MMM yyyy"));
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                conn.Open();
                if ((int)cmd.ExecuteScalar() > 0)
                {
                    ShowAlert("A Purchase Order with the same details already exists in this Company.", true);
                    return true;
                }
            }
            return false;
        }

        protected void ToggleGridColumns()
        {
            if (gd_Service_Product.Columns.Count > 20)
            {
                bool isQuotation = rbQt.Checked;
                gd_Service_Product.Columns[22].Visible = !isQuotation;
                gd_Service_Product.Columns[23].Visible = !isQuotation;
            }
        }

        private void TakePservice(int count1, string pservice)
        {
            if (count1 == 1) dtPCat1.Columns.Add("ProductCatagory", typeof(string));
            if (ViewState["pService"] != null)
            {
                dtPCat1 = (DataTable)ViewState["pService"];
                bool exists = false;
                foreach (DataRow row in dtPCat1.Rows) { if (row["ProductCatagory"].ToString() == pservice) exists = true; }
                if (!exists) dtPCat1.Rows.Add(pservice);
            }
            else { dtPCat1.Rows.Add(pservice); }

            gridps.DataSource = dtPCat1; gridps.DataBind();
            ViewState["pService"] = dtPCat1;
        }

        // ================= WIZARD FINAL SAVE =================

        protected void Button3_Click(object sender, EventArgs e) { MagicianNew(); }

        private void MagicianNew()
        {
            Bindquotationno();
            string CGSTSGSTSTATUS = RadioButtonGst.SelectedIndex == 0 ? "YES" : "";
            string IGSTSTATUS = RadioButtonGst.SelectedIndex != 0 ? "YES" : "";
            int slNo = idreturn() + 1;
            string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";
            DataTable dt1 = (DataTable)ViewState["PhaseProductData"];
            decimal new_sub_total = 0, new_total_Service = 0, new_Gross_amount = 0;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ToString()))
            {
                conn.Open();
                using (SqlCommand lockCmd = new SqlCommand("sp_getapplock", conn))
                {
                    lockCmd.CommandType = CommandType.StoredProcedure;
                    lockCmd.Parameters.AddWithValue("@Resource", "Lock_Quotation_" + lblqno.Text);
                    lockCmd.Parameters.AddWithValue("@LockMode", "Exclusive");
                    lockCmd.Parameters.AddWithValue("@LockOwner", "Session");
                    lockCmd.Parameters.AddWithValue("@DbPrincipal", "public");
                    SqlParameter returnCode = new SqlParameter("@return_value", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                    lockCmd.Parameters.Add(returnCode); lockCmd.ExecuteNonQuery();
                    if ((int)returnCode.Value < 0) { ShowAlert("Unable to acquire lock. Another user may be editing this.", true); return; }
                }

                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    int h = 0;
                    foreach (DataRow row in dt1.Rows)
                    {
                        h++;
                        decimal Quantity = ParseDecimal(row["Quantity"].ToString());
                        decimal Sail_Rate = ParseDecimal(row["Sail_Rate"].ToString());
                        decimal Tax_Rate = ParseDecimal(row["Tax_Rate"].ToString());
                        decimal Discount_Rate = ParseDecimal(row["Discount_Rate"].ToString());
                        decimal discounted_rate = Sail_Rate - (Sail_Rate * Discount_Rate / 100);
                        decimal taxMultiplier = (Tax_Rate + 100) / 100;
                        decimal Total_sail_rate = taxMultiplier * discounted_rate;
                        decimal Total_sail_rate1 = Total_sail_rate * Quantity;
                        decimal Total_sail_rate2 = discounted_rate * Quantity;
                        decimal Service_tax = (Tax_Rate * Quantity * discounted_rate) / 100;

                        new_sub_total += Total_sail_rate2;
                        new_total_Service += Service_tax;
                        new_Gross_amount += Total_sail_rate1;
                        using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO tbl_Quotaion_details 
                        (Sl_no, Quotation_no, Product_id, Product_Code, Product_name, Quantity, sail_rate, Service_tax_rate, Total_sail_rate, Total_sail_rate1, Total_sail_rate2, specification, Misc, InvStatus, Type, Unit, ProductOrServiceCat, discount_rate, new_sailrate, ItemRemarks, ItemNo, MaterialNo, PackSize, DeliveryDate, Department, AddedById, Version, IsDeleted, IsLatest, CompanyID) 
                        VALUES (@Sl_no, @Quotation_no, @Product_id, @Product_Code, @Product_name, @Quantity, @sail_rate, @Service_tax_rate, @Total_sail_rate, @Total_sail_rate1, @Total_sail_rate2, @specification, @Misc, @InvStatus, @Type, @Unit, @ProductOrServiceCat, @discount_rate, @new_sailrate, @ItemRemarks, @ItemNo, @MaterialNo, @PackSize, @DeliveryDate, @Department, @AddedById, 1, 0, 1, @CompanyID)", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@Sl_no", h);
                            cmd.Parameters.AddWithValue("@Quotation_no", lblqno.Text);
                            cmd.Parameters.AddWithValue("@Product_id", row["Product_code"]);
                            cmd.Parameters.AddWithValue("@Product_Code", row["ProductId"]);
                            cmd.Parameters.AddWithValue("@Product_name", row["ProductName"]);
                            cmd.Parameters.AddWithValue("@Quantity", Quantity);
                            cmd.Parameters.AddWithValue("@sail_rate", Sail_Rate);
                            cmd.Parameters.AddWithValue("@Service_tax_rate", Tax_Rate);
                            cmd.Parameters.AddWithValue("@Total_sail_rate", Total_sail_rate);
                            cmd.Parameters.AddWithValue("@Total_sail_rate1", Total_sail_rate1);
                            cmd.Parameters.AddWithValue("@Total_sail_rate2", Total_sail_rate2);
                            cmd.Parameters.AddWithValue("@specification", row["Brand"]);
                            cmd.Parameters.AddWithValue("@Misc", row["Specification"]);
                            cmd.Parameters.AddWithValue("@InvStatus", "No");
                            cmd.Parameters.AddWithValue("@Type", row["Type"]);
                            cmd.Parameters.AddWithValue("@Unit", row["Unit"]);
                            cmd.Parameters.AddWithValue("@ProductOrServiceCat", row["ProductOrServiceCat"]);
                            cmd.Parameters.AddWithValue("@discount_rate", Discount_Rate);
                            cmd.Parameters.AddWithValue("@new_sailrate", discounted_rate);
                            cmd.Parameters.AddWithValue("@ItemRemarks", row["ItemRemarks"]);
                            cmd.Parameters.AddWithValue("@ItemNo", row["ItemNo"]);
                            cmd.Parameters.AddWithValue("@MaterialNo", row["MaterialNo"]);
                            cmd.Parameters.AddWithValue("@PackSize", row["PackSize"]);
                            cmd.Parameters.AddWithValue("@DeliveryDate", rbQt.Checked ? "" : row["DeliveryDate"]);
                            cmd.Parameters.AddWithValue("@Department", rbQt.Checked ? "" : row["Department"]);
                            cmd.Parameters.AddWithValue("@AddedById", userId);
                            cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    int validDays = (int)ParseDecimal(txt_valdays.Text);
                    string deliveryTenure = DDL_DeliveryTerms.SelectedValue == "4" ? txt_deltrms.Text.Trim() : (DDL_DeliveryTerms.SelectedValue != "0" ? DDL_DeliveryTerms.SelectedItem.Text : "");
                    string packageForwarding = DDL_pkgfrwd.SelectedValue == "3" ? txt_pkgfrwd.Text.Trim() : (DDL_pkgfrwd.SelectedValue != "0" ? DDL_pkgfrwd.SelectedItem.Text : "");
                    string remarks = txt_remarks.Text?.Trim();
                    string referenceOption = rbYes.Checked ? "Yes" : "No";
                    string referenceName = referenceOption == "No" ? "N/A" : txt_clientrefname.Text?.Trim();
                    string referenceId = referenceOption == "No" ? "N/A" : txt_clientrefid.Text?.Trim();
                    string referenceDate = referenceOption == "No" ? "1900-01-01" : txt_clientrefdate.Text?.Trim();
                    string recordtyp = rbPo.Checked ? "Purchase Order" : "Quotation";
                    string DO_number = rbPo.Checked ? txb_donumber.Text.Trim() : "N/A";
                    string PO_number = rbPo.Checked ? txb_ponumber.Text.Trim() : "N/A";
                    string PO_Date = rbPo.Checked ? txb_podate.Text.Trim() : "1900-01-01";
                    string ValStart_Date = rbPo.Checked ? txb_strtdt.Text.Trim() : "1900-01-01";
                    string ValEnd_Date = rbPo.Checked ? txb_enddt.Text.Trim() : "1900-01-01";

                    decimal tcsAmount = ParseDecimal(txt_tcs_amnt.Text);
                    decimal tcsPercent = ParseDecimal(txt_tcs_percent.Text);
                    decimal deliveryAmount = ParseDecimal(txt_delivery_amnt.Text);
                    decimal freightPercent = ParseDecimal(txt_freight_percent.Text);
                    decimal otherAmount = ParseDecimal(txt_othr_amnt.Text);
                    decimal final_net_amount = Math.Round(new_Gross_amount, 2);
                    decimal final_service_tax = Math.Round(new_total_Service, 2);

                    // (Assuming cmbSalesPerson exists in ASPX to assign SalesPersonId)
                    string assignedSalesPerson = string.Empty;
                    // assignedSalesPerson = cmbSalesPerson.SelectedValue;

                    // --- INJECTION 2: Inserting the active CompanyID & SalesPersonId when creating the new Quotation/PO ---
                    using (SqlCommand cmd = new SqlCommand(@"INSERT INTO tbl_Quotation 
                    (Quotation_no, Quotation_date, Client_Id, Gross, Service_tax, Net_amount, Status1, Status2, Sl_no, status3, service_tax1, sub_total, cgstOrsgst, igst, PlaceofSupply, PaymentStatus, ReferenceData, ReferenceName, ReferenceId, ReferenceDate, ValidityDays, DeliveryTenure, PackingCharges, Remarks, DetailedView, RecordType, DO_Number, PO_Number, PO_Date, Validity_StartDate, Validity_EndDate, AddedById, DiscountView, TCS_Amount, TCS_Percent, Freight_Amount, Freight_VAT_Percent, OtherCharge_Name, OtherCharge_Amount, VisitId, SalesPersonId, CompanyID)
                    VALUES (@Quotation_no, @Quotation_date, @Client_Id, @Gross, @Service_tax, @Net_amount, 'No', 'No', @Sl_no, 'No', @service_tax1, @sub_total, @cgstOrsgst, 
                    @igst, @PlaceofSupply, 'No', @ReferenceData, @ReferenceName, @ReferenceId, @ReferenceDate, @ValidityDays, @DeliveryTenure, @PackingCharges, @Remarks, @DetailedView, @RecordType, @DO_Number, @PO_Number, @PO_Date, @Validity_StartDate, @Validity_EndDate, @AddedById, @DiscountView, @TCS_Amount, @TCS_Percent, @Freight_Amount, @Freight_VAT_Percent, @OtherCharge_Name, @OtherCharge_Amount, @VisitId, @SalesPersonId, @CompanyID)", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@Quotation_no", lblqno.Text);
                        cmd.Parameters.AddWithValue("@Quotation_date", txtquotationDate.Text);
                        cmd.Parameters.AddWithValue("@Client_Id", lblPreviewERPCode.Text);
                        cmd.Parameters.AddWithValue("@Gross", final_net_amount);
                        cmd.Parameters.AddWithValue("@Service_tax", final_service_tax);
                        cmd.Parameters.AddWithValue("@Net_amount", final_net_amount);
                        cmd.Parameters.AddWithValue("@Sl_no", slNo);
                        cmd.Parameters.AddWithValue("@service_tax1", final_service_tax);
                        cmd.Parameters.AddWithValue("@sub_total", new_sub_total);
                        cmd.Parameters.AddWithValue("@cgstOrsgst", RadioButtonGst.SelectedIndex == 0 ? "YES" : "");
                        cmd.Parameters.AddWithValue("@igst", RadioButtonGst.SelectedIndex != 0 ? "YES" : "");
                        cmd.Parameters.AddWithValue("@PlaceofSupply", ddlPlaceOfSupply.Text);
                        cmd.Parameters.AddWithValue("@ReferenceData", referenceOption);
                        cmd.Parameters.AddWithValue("@ReferenceName", referenceName);
                        cmd.Parameters.AddWithValue("@ReferenceId", referenceId);
                        cmd.Parameters.AddWithValue("@ReferenceDate", referenceDate);
                        cmd.Parameters.AddWithValue("@ValidityDays", validDays);
                        cmd.Parameters.AddWithValue("@DeliveryTenure", deliveryTenure);
                        cmd.Parameters.AddWithValue("@PackingCharges", packageForwarding);
                        cmd.Parameters.AddWithValue("@Remarks", remarks);
                        cmd.Parameters.AddWithValue("@DetailedView", DDL_ItemViewType.SelectedItem.Text?.Trim());
                        cmd.Parameters.AddWithValue("@RecordType", recordtyp);
                        cmd.Parameters.AddWithValue("@DO_Number", DO_number);
                        cmd.Parameters.AddWithValue("@PO_Number", PO_number);
                        cmd.Parameters.AddWithValue("@PO_Date", PO_Date);
                        cmd.Parameters.AddWithValue("@Validity_StartDate", ValStart_Date);
                        cmd.Parameters.AddWithValue("@Validity_EndDate", ValEnd_Date);
                        cmd.Parameters.AddWithValue("@AddedById", userId);
                        cmd.Parameters.AddWithValue("@DiscountView", DDL_DiscountView.SelectedItem.Text?.Trim());
                        cmd.Parameters.AddWithValue("@TCS_Amount", tcsAmount);
                        cmd.Parameters.AddWithValue("@TCS_Percent", tcsPercent);
                        cmd.Parameters.AddWithValue("@Freight_Amount", deliveryAmount);
                        cmd.Parameters.AddWithValue("@Freight_VAT_Percent", freightPercent);
                        cmd.Parameters.AddWithValue("@OtherCharge_Name", TextBox1.Text);
                        cmd.Parameters.AddWithValue("@OtherCharge_Amount", otherAmount);

                        if (!string.IsNullOrEmpty(hfVisitId.Value))
                            cmd.Parameters.AddWithValue("@VisitId", Convert.ToInt32(hfVisitId.Value));
                        else
                            cmd.Parameters.AddWithValue("@VisitId", DBNull.Value);

                        // Uncomment this when cmbSalesPerson is on ASPX
                        // cmd.Parameters.AddWithValue("@SalesPersonId", string.IsNullOrEmpty(assignedSalesPerson) ? DBNull.Value : (object)assignedSalesPerson);
                        //cmd.Parameters.AddWithValue("@SalesPersonId", DBNull.Value);
                        // 2. Map the Sales Person Dropdown (safely handling default "0" selection)
                        if (cmbSalesPerson.SelectedValue == "0" || string.IsNullOrEmpty(cmbSalesPerson.SelectedValue))
                        {
                            cmd.Parameters.AddWithValue("@SalesPersonId", DBNull.Value);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@SalesPersonId", cmbSalesPerson.SelectedValue);
                        }
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

                        cmd.ExecuteNonQuery();
                    }

                    insertPaymentPhaseNew(lblqno.Text, conn, trans);
                    insertprimaryServiceNew(lblqno.Text, conn, trans);

                    // --- CORE RULE: PROACTIVE NOTIFICATION LOGGING ---
                    InsertSystemNotification(trans, conn,
                        "Document Generated",
                        $"Document {lblqno.Text} has been successfully generated.",
                        "SALES", "SUCCESS");

                    trans.Commit();
                    ShowAlert("Document Saved Successfully! ID: " + lblqno.Text, false);
                    Button3.Visible = false;
                }
                catch (Exception ex)
                {
                    try { trans?.Rollback(); } catch { }

                    // Ponytail #3: Never expose raw exception details to client
                    ShowAlert("An unexpected error occurred while saving the document. Please try again.", true);
                }
            }
        }

        // ================= SUPPORT METHODS =================

        private decimal ParseDecimal(string text)
        {
            decimal val;
            if (decimal.TryParse(text, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out val))
                return val;
            return 0m;
        }

        // --- INJECTION 3: Dynamic Prefix using CompanyContext.CurrentCompanyCode ---
        private void Bindquotationno()
        {
            string prefix = rbPo.Checked ? $"PO/{CompanyContext.CurrentCompanyCode}/" : $"QTN/{CompanyContext.CurrentCompanyCode}/";
            string ss = findmonth();
            int j = idreturn_New(prefix + ss);
            string quotationNo;
            do
            {
                j += 1;
                quotationNo = prefix + ss + j.ToString();
            }
            while (QuotationNoExists(quotationNo));
            lblqno.Text = quotationNo;
        }

        // --- INJECTION 4: Scoped No Check ---
        private bool QuotationNoExists(string quotationNo)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM tbl_Quotation WHERE Quotation_no = @QuotationNo AND CompanyID = @CompanyID", con))
            {
                cmd.Parameters.AddWithValue("@QuotationNo", quotationNo);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                con.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        // --- INJECTION 5: Scoped Sequence Generation ---
        private int idreturn_New(string prefix)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 Quotation_no FROM tbl_Quotation WHERE Quotation_no LIKE @Prefix + '%' AND CompanyID = @CompanyID ORDER BY Id DESC", con))
            {
                cmd.Parameters.AddWithValue("@Prefix", prefix);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                con.Open();
                var res = cmd.ExecuteScalar();
                if (res != null)
                {
                    string[] p = res.ToString().Trim().Split('/');
                    int n;
                    if (p.Length >= 4 && int.TryParse(p[p.Length - 1], out n)) return n;
                }
            }
            return 0;
        }

        // --- INJECTION 6: Scoped Serial Isolation ---
        private int idreturn()
        {
            int b = 0;
            string d = txtquotationDate.Text, m = d.Substring(3, 3), y = d.Substring(7, 4), d5, d6;
            if (m == "Jan" || m == "Feb" || m == "Mar")
            {
                d5 = "31-Mar-" + (Convert.ToInt32(y) - 1).ToString(); d6 = "31-Mar-" + y;
            }
            else
            {
                d5 = "31-Mar-" + y; d6 = "31-Mar-" + (Convert.ToInt32(y) + 1).ToString();
            }
            // Ponytail #3: Parameterized query replaces string concatenation
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand(
                "SELECT Sl_no FROM tbl_Quotation WHERE ID = (SELECT MAX(ID) FROM tbl_Quotation WHERE CAST(Quotation_date AS datetime) BETWEEN @Date5 AND @Date6 AND CompanyID = @CompanyID)", conn))
            {
                cmd.Parameters.AddWithValue("@Date5", d5);
                cmd.Parameters.AddWithValue("@Date6", d6);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                conn.Open();
                using (var re = cmd.ExecuteReader())
                {
                    if (re.Read() && re["Sl_no"] != DBNull.Value) b = Convert.ToInt32(re["Sl_no"]);
                }
            }
            return b;
        }

        private string findmonth()
        {
            string m = txtquotationDate.Text.Substring(3, 3), y = txtquotationDate.Text.Substring(9, 2);
            return (m == "Jan" || m == "Feb" || m == "Mar") ?
            (Convert.ToInt32(y) - 1).ToString() + "-" + y + "/" : y + "-" + (Convert.ToInt32(y) + 1).ToString() + "/";
        }

        private void bindphaseType()
        {
            dtphasetype = DbCL.SPreturn_dt("select id, PaymentPhase from tbl_PaymentPhase order by id", null);
            if (dtphasetype.Rows.Count > 0)
            {
                listPhaseType.Items.Clear(); foreach (DataRow r in dtphasetype.Rows) listPhaseType.Items.Add(r["PaymentPhase"].ToString());
            }
        }

        protected void listPhaseType_TextChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < listPhaseType.Items.Count; i++)
            {
                if (listPhaseType.Items[i].Selected)
                {
                    string pt = listPhaseType.Items[i].Text;
                    if (ViewState["phaseAmountData"] != null)
                    {
                        dtPhasefees = (DataTable)ViewState["phaseAmountData"];
                        bool s = false; foreach (DataRow r in dtPhasefees.Rows)
                        {
                            if (r["PaymentPhase"].ToString() == pt) s = true;
                        }
                        if (!s)
                        {
                            DataRow dr = dtPhasefees.NewRow();
                            dr[0] = pt; dr[1] = ""; dr[2] = (pt == "Full & Final Instalment" || pt == "Payment After Delivery" || pt == "100% Against PI") ?
                            "100" : ""; dtPhasefees.Rows.Add(dr);
                        }
                    }
                    else
                    {
                        dtPhasefees.Columns.Add("PaymentPhase");
                        dtPhasefees.Columns.Add("PhaseDesc"); dtPhasefees.Columns.Add("AmountPer");
                        DataRow dr = dtPhasefees.NewRow(); dr[0] = pt; dr[1] = "";
                        dr[2] = (pt == "Full & Final Instalment" || pt == "Payment After Delivery" || pt == "100% Against PI") ?
                        "100" : ""; dtPhasefees.Rows.Add(dr);
                    }
                }
            }
            GridView3.DataSource = dtPhasefees;
            GridView3.DataBind(); ViewState["phaseAmountData"] = dtPhasefees;
        }

        protected void AmountPer_TextChanged(object sender, EventArgs e)
        {
            double t = 0;
            foreach (GridViewRow r in GridView3.Rows)
            {
                if (((Label)r.Cells[1].FindControl("PaymentPhase")).Text != "Full & Final Instalment")
                {
                    double s;
                    if (double.TryParse(((TextBox)r.Cells[0].FindControl("AmountPer")).Text, out s)) t += s;
                }
                else
                {
                    ((TextBox)r.Cells[0].FindControl("AmountPer")).Text = (100 - t).ToString();
                }
            }
        }

        protected void GridView3_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            DataTable d = (DataTable)ViewState["phaseAmountData"];
            d.Rows[e.RowIndex].Delete();
            ViewState["phaseAmountData"] = d.Rows.Count > 0 ? d : null;
            GridView3.DataSource = (DataTable)ViewState["phaseAmountData"]; GridView3.DataBind();
        }

        private void insertPaymentPhaseNew(string qutno, SqlConnection conn, SqlTransaction trans)
        {
            foreach (GridViewRow r in GridView3.Rows)
            {
                using (SqlCommand cmd = new SqlCommand("INSERT INTO tbl_QutPaymentPhase(qut_no, phase_type, PhaseDesc, amountper, TimeStamp, CompanyID) VALUES (@qut_no, @pt, @pd, @ap, GETDATE(), @CompanyID)", conn, trans))
                {
                    cmd.Parameters.AddWithValue("@qut_no", qutno);
                    cmd.Parameters.AddWithValue("@pt", ((Label)r.Cells[1].FindControl("PaymentPhase")).Text);
                    cmd.Parameters.AddWithValue("@pd", ((TextBox)r.Cells[2].FindControl("PhaseDesc")).Text);
                    cmd.Parameters.AddWithValue("@ap", GridView3.Rows.Count == 1 ? "100" : ((TextBox)r.Cells[0].FindControl("AmountPer")).Text);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void insertprimaryServiceNew(string qutno, SqlConnection conn, SqlTransaction trans)
        {
            string ps = "";
            int i = 0;
            foreach (GridViewRow r in gridps.Rows)
            {
                string pc = ((Label)r.Cells[0].FindControl("ProductCatagory")).Text;
                using (SqlCommand cmd = new SqlCommand("INSERT INTO tbl_QutPrimaryService(qut_no, PrimaryService, TimeStamp, CompanyID) VALUES (@q, @ps, GETDATE(), @CompanyID)", conn, trans))
                {
                    cmd.Parameters.AddWithValue("@q", qutno);
                    cmd.Parameters.AddWithValue("@ps", pc);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    cmd.ExecuteNonQuery();
                }

                using (SqlCommand cmd = new SqlCommand("SELECT PrimaryServiceTerms FROM tbl_PrimaryServiceTerms WHERE PrimaryService=@p", conn, trans))
                {
                    cmd.Parameters.AddWithValue("@p", pc);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            using (SqlCommand c = new SqlCommand("INSERT INTO tbl_QuoPserTerm (qutno, PServiceName, PSerTer, TimeStamp, CompanyID) VALUES (@q, @pn, @pt, GETDATE(), @CompanyID)", conn, trans))
                            {
                                c.Parameters.AddWithValue("@q", qutno);
                                c.Parameters.AddWithValue("@pn", pc);
                                c.Parameters.AddWithValue("@pt", dr[0]);
                                c.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                                c.ExecuteNonQuery();
                            }
                        }
                    }
                }
                pc = "“" + pc + "”";
                if (i == 0) ps = pc;
                else if (i == 1) ps += " and " + pc;
                else ps += " , " + pc;
                i++;
            }
            if (!string.IsNullOrEmpty(ps))
            {
                using (SqlCommand cmd = new SqlCommand("INSERT INTO tbl_QuoPriSerTogather (qutno, PServiceName, TimeStamp, CompanyID) VALUES (@q, @ps, GETDATE(), @CompanyID)", conn, trans))
                {
                    cmd.Parameters.AddWithValue("@q", qutno);
                    cmd.Parameters.AddWithValue("@ps", ps);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // --- CORE RULE: PROACTIVE NOTIFICATION HELPER ---
        private void InsertSystemNotification(SqlTransaction trans, SqlConnection conn, string title, string message, string moduleCode, string severity)
        {
            string sql = @"INSERT INTO tbl_SystemNotification 
                           (Title, Message, ModuleCode, Severity, StartDate, EndDate, IsActive, CreatedBy, CreatedOn, CompanyID) 
                           VALUES 
                           (@Title, @Message, @ModuleCode, @Severity, GETDATE(), DATEADD(day, 7, GETDATE()), 1, @CreatedBy, GETDATE(), @CompanyID)";

            using (SqlCommand cmd = new SqlCommand(sql, conn, trans))
            {
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Message", message);
                cmd.Parameters.AddWithValue("@ModuleCode", moduleCode);
                cmd.Parameters.AddWithValue("@Severity", severity);
                cmd.Parameters.AddWithValue("@CreatedBy", Session["USERID"]?.ToString() ?? "System");
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                cmd.ExecuteNonQuery();
            }
        }

        protected void cmbClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Hide preview if they revert to "--Select--"
            if (cmbClient.SelectedValue == "0" || string.IsNullOrEmpty(cmbClient.SelectedValue))
            {
                pnlClientPreview.Visible = false;
                return;
            }

            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Safe parameterized query perfectly mapped to your sample schema
                string query = @"SELECT Client_Id, Client_Name, Address1, City, pin, State, 
                                Service_tax_no, Pan_no, PlaceofSupply 
                         FROM tbl_Client 
                         WHERE (Client_Id = @ClientParam OR Client_Name = @ClientParam) 
                         AND CompanyID = @CompanyID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientParam", cmbClient.SelectedValue);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            // Basic Info
                            lblPreviewName.Text = dr["Client_Name"]?.ToString();

                            // Capture Client_Id securely
                            string clientId = dr["Client_Id"]?.ToString();
                            lblPreviewERPCode.Text = clientId;

                            // ---- NEW: Set the Dynamic Modify Link ----
                            lnkModifyClient.NavigateUrl = $"~/corporate/business/app/Update_client.aspx?Client_Id={clientId}";

                            // Address Formatting (Skipping blank fields)
                            System.Collections.Generic.List<string> addressParts = new System.Collections.Generic.List<string>();
                            if (dr["Address1"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["Address1"].ToString()))
                                addressParts.Add(dr["Address1"].ToString().Trim());
                            if (dr["City"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["City"].ToString()))
                                addressParts.Add(dr["City"].ToString().Trim());

                            string zipCode = dr["pin"] != DBNull.Value ? dr["pin"].ToString().Trim() : "";
                            string finalAddress = string.Join(", ", addressParts);
                            if (!string.IsNullOrWhiteSpace(zipCode)) finalAddress += " - " + zipCode;

                            lblPreviewAddress.Text = string.IsNullOrWhiteSpace(finalAddress) ? "Address not provided." : finalAddress;

                            // Geography Details
                            lblPreviewState.Text = dr["State"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["State"].ToString()) ? dr["State"].ToString() : "N/A";
                            lblPreviewPOS.Text = dr["PlaceofSupply"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["PlaceofSupply"].ToString()) ? dr["PlaceofSupply"].ToString() : "N/A";

                            // Taxation Details (Service_tax_no stores the GSTIN)
                            lblPreviewGST.Text = dr["Service_tax_no"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["Service_tax_no"].ToString()) ? dr["Service_tax_no"].ToString() : "N/A";
                            lblPreviewPAN.Text = dr["Pan_no"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["Pan_no"].ToString()) ? dr["Pan_no"].ToString() : "N/A";

                            // Auto-Select the Place of Supply Dropdown
                            string placeOfSupply = dr["PlaceofSupply"] != DBNull.Value ? dr["PlaceofSupply"].ToString().Trim() : "";
                            if (!string.IsNullOrEmpty(placeOfSupply))
                            {
                                ListItem posItem = ddlPlaceOfSupply.Items.FindByText(placeOfSupply);
                                if (posItem != null)
                                {
                                    ddlPlaceOfSupply.ClearSelection();
                                    posItem.Selected = true;
                                }
                            }

                            // Show the Preview Card
                            pnlClientPreview.Visible = true;
                        }
                        else
                        {
                            pnlClientPreview.Visible = false;
                        }
                    }
                }
            }
        }
    }
}