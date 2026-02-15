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
    public partial class Direct_Proforma : System.Web.UI.Page
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
                txtinvoiceDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                BindClients();
                BindCategories();
            }
        }

        // ==========================================================================================
        // SECTION 1: WIZARD NAVIGATION LOGIC
        // ==========================================================================================

        protected void btnNextToProd_Click(object sender, EventArgs e)
        {
            // 1. Validate Client Selection
            if (cmbClient.SelectedIndex == 0)
            {
                lblStep1Error.Text = "Please select a client to proceed.";
                return;
            }

            // 2. Validate Tax Type Selection (New)
            if (RadioButtonGst.SelectedIndex == -1)
            {
                lblStep1Error.Text = "Please select a Tax Type (Intra-State or Inter-State).";
                return;
            }

            // 3. Clear errors and proceed
            lblStep1Error.Text = "";
            mvInvoice.ActiveViewIndex = 1;
            UpdateStepIndicator(2);
        }

        protected void btnBackToSetup_Click(object sender, EventArgs e)
        {
            mvInvoice.ActiveViewIndex = 0;
            UpdateStepIndicator(1);
        }

        protected void btnReview_Click(object sender, EventArgs e)
        {
            DataTable dt = ViewState["PhaseProductData"] as DataTable;
            if (dt == null || dt.Rows.Count == 0)
            {
                lblStep2Msg.Text = "Please add at least one product to the list.";
                lblStep2Msg.ForeColor = System.Drawing.Color.Red;
                return;
            }
            lblStep2Msg.Text = "";
            mvInvoice.ActiveViewIndex = 2;
            UpdateStepIndicator(3);
        }

        protected void btnBackToProd_Click(object sender, EventArgs e)
        {
            mvInvoice.ActiveViewIndex = 1;
            UpdateStepIndicator(2);
        }

        private void UpdateStepIndicator(int step)
        {
            step1.Attributes["class"] = "step-item";
            step2.Attributes["class"] = "step-item";
            step3.Attributes["class"] = "step-item";

            if (step == 1) step1.Attributes["class"] += " active";
            if (step == 2)
            {
                step1.Attributes["class"] += " completed";
                step2.Attributes["class"] += " active";
            }
            if (step == 3)
            {
                step1.Attributes["class"] += " completed";
                step2.Attributes["class"] += " completed";
                step3.Attributes["class"] += " active";
            }
        }

        // ==========================================================================================
        // SECTION 2: SEARCH & FILTER LOGIC
        // ==========================================================================================

        protected void btnSearchProduct_Click(object sender, EventArgs e)
        {
            string keyword = txtSearchProduct.Text.Trim();
            if (string.IsNullOrEmpty(keyword)) return;

            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            string sql = @"SELECT * FROM tbl_NewProduct 
                           WHERE (ProductName LIKE @Search 
                              OR ProductID LIKE @Search 
                              OR Product_code LIKE @Search 
                              OR Brand LIKE @Search) 
                           AND DeleteMode = 0 
                           ORDER BY ProductName";

            SqlParameter[] pram = { new SqlParameter("@Search", "%" + keyword + "%") };
            DataTable dt = DbCL.SPreturn_dt(sql, pram);

            if (dt.Rows.Count > 0)
            {
                gridProdWithCat.PageIndex = 0;
                gridProdWithCat.DataSource = dt;
                gridProdWithCat.DataBind();
                ViewState["dtprocat"] = dt;
                lblMessage.Text = "";
                PanelMsg.Visible = false;
            }
            else
            {
                gridProdWithCat.DataSource = null;
                gridProdWithCat.DataBind();
                lblMessage.Text = "No products found matching: " + keyword;
                lblMessage.ForeColor = System.Drawing.Color.Red;
                PanelMsg.Visible = true;
            }
            DbCL.Conn.Close();
        }

        protected void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtSearchProduct.Text = "";
            if (cmbproduct_service.SelectedIndex > 0) BindProducts();
            else
            {
                gridProdWithCat.DataSource = null;
                gridProdWithCat.DataBind();
                ViewState["dtprocat"] = null;
            }
            lblMessage.Text = "";
            PanelMsg.Visible = false;
        }

        protected void gridProdWithCat_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gridProdWithCat.PageIndex = e.NewPageIndex;
            if (ViewState["dtprocat"] != null)
            {
                gridProdWithCat.DataSource = (DataTable)ViewState["dtprocat"];
                gridProdWithCat.DataBind();
            }
        }

        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            gridProdWithCat.PageSize = int.Parse(ddlPageSize.SelectedValue);
            gridProdWithCat.PageIndex = 0;
            if (ViewState["dtprocat"] != null)
            {
                gridProdWithCat.DataSource = (DataTable)ViewState["dtprocat"];
                gridProdWithCat.DataBind();
            }
        }

        // ==========================================================================================
        // SECTION 3: DATA BINDING (Clients & Products)
        // ==========================================================================================

        private void BindClients()
        {
            DbCL.FillCombo(cmbClient, "select Client_Name from tbl_Client order by Client_Name");
            cmbClient.Items.Insert(0, "-- Select Client --");
        }

        private void BindCategories()
        {
            DbCL.FillCombo(cmbproduct_service, "select ProductOrServiceCat from tbl_NewparentProduct order by ProductOrServiceCat");
            cmbproduct_service.Items.Insert(0, "-- Select Category --");
        }

        protected void cmbClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbClient.SelectedIndex > 0)
            {
                DbCL.Sqlconnection();
                DbCL.ConnectDb();

                // Fetch Client Details
                string query = @"SELECT Client_Id, Address1, Address2, City, State, pin, Service_tax_no 
                         FROM tbl_Client 
                         WHERE Client_Name=@Name";

                SqlParameter[] pram = { new SqlParameter("@Name", cmbClient.Text) };

                // Use a DataReader or Adapter to get the row
                // Note: Assuming DbCL.SPreturn_dt or standard ADO.NET
                DataTable dt = DbCL.SPreturn_dt(query, pram);

                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    lblclientID.Text = dr["Client_Id"].ToString();

                    // Format Address
                    string addr = dr["Address1"].ToString();
                    if (!string.IsNullOrEmpty(dr["Address2"].ToString())) addr += ", " + dr["Address2"].ToString();
                    if (!string.IsNullOrEmpty(dr["pin"].ToString())) addr += " - " + dr["pin"].ToString();

                    // Populate Labels for Display
                    lblClientAddress.Text = addr;
                    lblClientGST.Text = dr["Service_tax_no"].ToString();

                    // Display State & Place of Supply (City)
                    lblClientState.Text = dr["State"].ToString();
                    lblPlaceOfSupply.Text = dr["City"].ToString(); // As per your requirement

                    // Show the info panel
                    pnlClientInfo.Visible = true;
                }
                DbCL.Conn.Close();
            }
            else
            {
                lblclientID.Text = "-";
                pnlClientInfo.Visible = false;
            }
        }

        protected void Button3_Click(object sender, EventArgs e) { BindProducts(); }
        protected void cmbproduct_service_SelectedIndexChanged(object sender, EventArgs e) { BindProducts(); }

        private void BindProducts()
        {
            if (cmbproduct_service.SelectedIndex <= 0) return;
            DataTable dt = new DataTable();
            string sql = "select * from tbl_NewProduct where ProductOrServiceCat=@Cat AND DeleteMode=0 order by ProductName";
            SqlParameter[] pram = { new SqlParameter("@Cat", cmbproduct_service.Text) };
            dt = DbCL.SPreturn_dt(sql, pram);

            if (dt.Rows.Count > 0)
            {
                gridProdWithCat.PageIndex = 0;
                gridProdWithCat.DataSource = dt;
                gridProdWithCat.DataBind();
                ViewState["dtprocat"] = dt;
                lblMessage.Text = "";
                PanelMsg.Visible = false;
            }
            else
            {
                gridProdWithCat.DataSource = null;
                gridProdWithCat.DataBind();
                lblMessage.Text = "No products found.";
                PanelMsg.Visible = true;
            }
        }

        // ==========================================================================================
        // SECTION 4: ADD TO CART LOGIC
        // ==========================================================================================

        protected void btnAddProduct_Click(object sender, EventArgs e)
        {
            DataTable dtPCat = ViewState["PhaseProductData"] as DataTable;
            if (dtPCat == null)
            {
                dtPCat = new DataTable();
                dtPCat.Columns.Add("ProductId"); dtPCat.Columns.Add("Product_code");
                dtPCat.Columns.Add("ProductName"); dtPCat.Columns.Add("Sail_Rate");
                dtPCat.Columns.Add("Tax_Rate"); dtPCat.Columns.Add("SQuantity");
                dtPCat.Columns.Add("Brand"); dtPCat.Columns.Add("Type");
                dtPCat.Columns.Add("Unit"); dtPCat.Columns.Add("ProductOrServiceCat");
                dtPCat.Columns.Add("Discount_Rate"); dtPCat.Columns.Add("ItemRemarks");
            }

            int addedCount = 0;
            foreach (GridViewRow row in gridProdWithCat.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    CheckBox chk = (CheckBox)row.FindControl("chkdtp");
                    if (chk != null && chk.Checked)
                    {
                        Label lblId = (Label)row.FindControl("ProductID");
                        Label lblCode = (Label)row.FindControl("Product_code");
                        Label lblName = (Label)row.FindControl("ProductName");
                        Label lblBrand = (Label)row.FindControl("Brand");
                        Label lblType = (Label)row.FindControl("Type");
                        Label lblUnit = (Label)row.FindControl("Unit");
                        Label lblCat = (Label)row.FindControl("ProductOrServiceCat");
                        Label lblTax = (Label)row.FindControl("Tax_Rate");
                        TextBox txtRate = (TextBox)row.FindControl("Sail_Rate");
                        TextBox txtQty = (TextBox)row.FindControl("IQuantity");
                        TextBox txtDisc = (TextBox)row.FindControl("Discount_Rate");
                        TextBox txtRem = (TextBox)row.FindControl("ItemRemarks");

                        if (lblId != null)
                        {
                            string pid = lblId.Text;
                            bool exists = dtPCat.AsEnumerable().Any(r => r["ProductId"].ToString() == pid);
                            if (!exists)
                            {
                                DataRow dr = dtPCat.NewRow();
                                dr["ProductId"] = pid;
                                dr["Product_code"] = lblCode != null ? lblCode.Text : "";
                                dr["ProductName"] = lblName != null ? lblName.Text : "";
                                dr["Brand"] = lblBrand != null ? lblBrand.Text : "";
                                dr["Type"] = lblType != null ? lblType.Text : "";
                                dr["Unit"] = lblUnit != null ? lblUnit.Text : "";
                                dr["ProductOrServiceCat"] = lblCat != null ? lblCat.Text : "";
                                dr["Tax_Rate"] = lblTax != null ? lblTax.Text : "0";
                                dr["Sail_Rate"] = txtRate != null ? txtRate.Text : "0";
                                dr["SQuantity"] = txtQty != null ? txtQty.Text : "1";
                                dr["Discount_Rate"] = txtDisc != null ? txtDisc.Text : "0";
                                dr["ItemRemarks"] = txtRem != null ? txtRem.Text : "";
                                dtPCat.Rows.Add(dr);
                                addedCount++;
                            }
                        }
                    }
                }
            }

            ViewState["PhaseProductData"] = dtPCat;
            gd_Service_Product.DataSource = dtPCat;
            gd_Service_Product.DataBind();

            if (addedCount > 0)
            {
                lblStep2Msg.Text = addedCount + " products added. Click 'Review' when done.";
                lblStep2Msg.ForeColor = System.Drawing.Color.Green;
            }
        }

        // ==========================================================================================
        // SECTION 5: FINAL SAVE
        // ==========================================================================================

        protected void btn_finalsave_Click(object sender, EventArgs e)
        {
            if (cmbClient.SelectedIndex == 0) return;
            if (gd_Service_Product.Rows.Count == 0) return;

            StringBuilder logBuilder = new StringBuilder();
            logBuilder.AppendLine($"Log Start: {DateTime.Now}");

            try
            {
                decimal totalBeforeTax = 0, totalTax = 0, totalNet = 0;
                string invoiceNo = BindInvoiceNo();
                int slNo = idreturn() + 1;

                // 1. Calc Totals
                foreach (GridViewRow row in gd_Service_Product.Rows)
                {
                    decimal qty = Convert.ToDecimal(((TextBox)row.FindControl("IQuantity")).Text);
                    decimal rate = Convert.ToDecimal(((TextBox)row.FindControl("Sail_Rate")).Text);
                    decimal taxPer = Convert.ToDecimal(((Label)row.FindControl("Tax_Rate")).Text);
                    decimal discPer = Convert.ToDecimal(((TextBox)row.FindControl("Discount_Rate")).Text);

                    decimal amountBeforeTax = Math.Round(qty * rate, 2);
                    decimal discAmt = Math.Round((amountBeforeTax * discPer) / 100, 2);
                    decimal amtAfterDisc = amountBeforeTax - discAmt;
                    decimal taxAmt = Math.Round((amtAfterDisc * taxPer) / 100, 2);
                    decimal netAmt = amtAfterDisc + taxAmt;

                    totalBeforeTax += amountBeforeTax;
                    totalTax += taxAmt;
                    totalNet += netAmt;
                }

                // 2. Insert Header
                string taxType = RadioButtonGst.SelectedValue;
                string cgstFlag = (taxType == "1") ? "YES" : null;
                string igstFlag = (taxType == "0") ? "YES" : null;

                string queryHeader = @"INSERT INTO tbl_Proforma 
                                       (Invoice_No, Invoice_Date, Quotation_No, Quotation_Date, Client_ID, 
                                        Gross, Service_Tax, Net_Amount, Sl_no, subtotal, 
                                        cgstOrsgst, igst, PlaceofSupply, mailStatus) 
                                       VALUES 
                                       (@InvNo, @InvDate, 'Direct', '', @ClientID, 
                                        @Gross, @Tax, @Net, @SlNo, @SubTotal, 
                                        @CGST, @IGST, @Place, 'Pending')";

                List<SqlParameter> headerParams = new List<SqlParameter> {
                    new SqlParameter("@InvNo", invoiceNo),
                    new SqlParameter("@InvDate", txtinvoiceDate.Text),
                    new SqlParameter("@ClientID", lblclientID.Text),
                    new SqlParameter("@Gross", totalNet),
                    new SqlParameter("@Tax", totalTax),
                    new SqlParameter("@Net", totalNet),
                    new SqlParameter("@SlNo", slNo),
                    new SqlParameter("@SubTotal", totalBeforeTax),
                    new SqlParameter("@CGST", (object)cgstFlag ?? DBNull.Value),
                    new SqlParameter("@IGST", (object)igstFlag ?? DBNull.Value),
                    new SqlParameter("@Place", lblPlaceOfSupply.Text.ToString())
                };
                DbCL.SPExecDB(queryHeader, headerParams.ToArray());

                // 3. Insert Details (tbl_Proforma_Details)

                int detailSlNo = 1; // <--- FIX START: Initialize Counter

                foreach (GridViewRow row in gd_Service_Product.Rows)
                {
                    // Retrieve existing controls
                    string pid = ((Label)row.FindControl("ProductID")).Text;
                    string pcode = ((Label)row.FindControl("Product_code")).Text;
                    string pname = row.Cells[0].Text; // BoundField Name

                    // --- NEW: Retrieve Missing Data ---
                    string unit = ((Label)row.FindControl("Unit")).Text;
                    string cat = ((Label)row.FindControl("ProductOrServiceCat")).Text;
                    // ----------------------------------

                    decimal qty = Convert.ToDecimal(((TextBox)row.FindControl("IQuantity")).Text);
                    decimal rate = Convert.ToDecimal(((TextBox)row.FindControl("Sail_Rate")).Text);
                    decimal taxPer = Convert.ToDecimal(((Label)row.FindControl("Tax_Rate")).Text);

                    decimal amountBeforeTax = Math.Round(qty * rate, 2);
                    decimal taxAmt = Math.Round((amountBeforeTax * taxPer) / 100, 2);
                    decimal netAmt = amountBeforeTax + taxAmt;

                    // UPDATED QUERY: Added Unit, ProductOrServiceCat, AddedById
                    string queryDet = @"INSERT INTO tbl_Proforma_Details 
                        (Invoice_No, Sl_no, Product_id, Product_Code, Product_name, 
                         Quantity, Rate, Tax_Rate, Tax_Amount, Total_Amount, Net_Amount,
                         Unit, ProductOrServiceCat, AddedById)
                        VALUES 
                        (@InvNo, @Sl, @Pid, @Pcode, @Pname, 
                         @Qty, @Rate, @TaxPer, @TaxAmt, @Total, @Net,
                         @Unit, @Cat, @UserId)";

                    List<SqlParameter> detParams = new List<SqlParameter> {
                        new SqlParameter("@InvNo", invoiceNo),
                        new SqlParameter("@Sl", detailSlNo), // <--- FIX: Use Variable
                        new SqlParameter("@Pid", pid),
                        new SqlParameter("@Pcode", pcode),
                        new SqlParameter("@Pname", pname),
                        new SqlParameter("@Qty", qty),
                        new SqlParameter("@Rate", rate),
                        new SqlParameter("@TaxPer", taxPer),
                        new SqlParameter("@TaxAmt", taxAmt),
                        new SqlParameter("@Total", amountBeforeTax),
                        new SqlParameter("@Net", netAmt),
                        new SqlParameter("@Unit", unit),
                        new SqlParameter("@Cat", cat),
                        new SqlParameter("@UserId", Session["USERID"].ToString())
                    };
                    DbCL.SPExecDB(queryDet, detParams.ToArray());
                    detailSlNo++; // <--- FIX END: Increment Counter
                }

                WriteLog(logBuilder.ToString());
                lblMessage.Text = "Proforma Saved: " + invoiceNo;
                lblMessage.ForeColor = System.Drawing.Color.Green;
                PanelMsg.Visible = true;
                btn_finalsave.Visible = false;
            }
            catch (Exception ex)
            {
                WriteLog("Error: " + ex.Message);
                lblMessage.Text = "Error: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
                PanelMsg.Visible = true;
            }
        }

        // ==========================================================================================
        // SECTION 6: HELPERS
        // ==========================================================================================

        private string BindInvoiceNo()
        {
            string c = cmbClient.Text.Trim();
            string f = c.Substring(0, 1);
            f = "PINV/" + f + "/";
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

            string cmdstring = "select Sl_no from tbl_Proforma where ID=(select max(ID) from tbl_Proforma where cast(Invoice_Date as datetime) between '" + date5 + "' and '" + date6 + "')";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                a = re["Sl_no"].ToString();
                b = Convert.ToInt32(a);
            }
            else b = 0;
            DbCL.Conn.Close();
            return b;
        }

        private string findmonth()
        {
            string MonthName = "-";
            string a = txtinvoiceDate.Text.Substring(3, 3);
            string b = txtinvoiceDate.Text.Substring(9, 2);
            if (a == "Jan" || a == "Feb" || a == "Mar")
                MonthName = (Convert.ToInt32(b) - 1).ToString() + "-" + b + "/";
            else
                MonthName = b + "-" + (Convert.ToInt32(b) + 1).ToString() + "/";
            return MonthName;
        }

        private void WriteLog(string content)
        {
            try
            {
                string logDir = Server.MapPath("~/Uploads/ProformaLogs");
                if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
                string path = Path.Combine(logDir, $"Log_{DateTime.Now:yyyyMMddHHmmss}.txt");
                File.WriteAllText(path, content);
            }
            catch { }
        }
    }
}