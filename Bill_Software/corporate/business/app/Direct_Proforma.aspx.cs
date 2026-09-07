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
using System.Configuration;

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
        // SECTION 1: WIZARD NAVIGATION
        // ==========================================================================================
        protected void btnNextToProd_Click(object sender, EventArgs e)
        {
            if (cmbClient.SelectedIndex == 0)
            {
                lblStep1Error.Text = "Please select a client to proceed."; return;
            }
            if (RadioButtonGst.SelectedIndex == -1)
            {
                lblStep1Error.Text = "Please select a Tax Type."; return;
            }
            lblStep1Error.Text = "";
            mvInvoice.ActiveViewIndex = 1;
            UpdateStepIndicator(2);
        }

        protected void btnBackToSetup_Click(object sender, EventArgs e) { mvInvoice.ActiveViewIndex = 0; UpdateStepIndicator(1); }

        protected void btnReview_Click(object sender, EventArgs e)
        {
            DataTable dt = ViewState["PhaseProductData"] as DataTable;
            if (dt == null || dt.Rows.Count == 0)
            {
                lblStep2Msg.Text = "Please add items first."; lblStep2Msg.ForeColor = System.Drawing.Color.Red; return;
            }
            lblStep2Msg.Text = "";
            mvInvoice.ActiveViewIndex = 2;
            UpdateStepIndicator(3);
            BindReviewGrid(); // Ensure grid is bound so JS can calculate
        }

        protected void btnBackToProd_Click(object sender, EventArgs e) { mvInvoice.ActiveViewIndex = 1; UpdateStepIndicator(2); }

        private void UpdateStepIndicator(int step)
        {
            step1.Attributes["class"] = step2.Attributes["class"] = step3.Attributes["class"] = "step-item";
            if (step == 1) step1.Attributes["class"] += " active";
            if (step == 2) { step1.Attributes["class"] += " completed"; step2.Attributes["class"] += " active"; }
            if (step == 3) { step1.Attributes["class"] += " completed"; step2.Attributes["class"] += " completed"; step3.Attributes["class"] += " active"; }
        }

        // ==========================================================================================
        // SECTION 2: SEARCH & FILTER
        // ==========================================================================================
        protected void btnSearchProduct_Click(object sender, EventArgs e)
        {
            string keyword = txtSearchProduct.Text.Trim();
            if (string.IsNullOrEmpty(keyword)) return;

            DbCL.Sqlconnection(); DbCL.ConnectDb();
            // Visible Columns are: Checkbox(0), Name(1), Spec(2), HSN(3)... in JS
            string sql = @"SELECT * FROM tbl_NewProduct WHERE (ProductName LIKE @Search OR ProductID LIKE @Search OR Product_code LIKE @Search OR Brand LIKE @Search) AND DeleteMode = 0 ORDER BY ProductName";
            SqlParameter[] pram = { new SqlParameter("@Search", "%" + keyword + "%") };
            DataTable dt = DbCL.SPreturn_dt(sql, pram);

            if (dt.Rows.Count > 0)
            {
                gridProdWithCat.PageIndex = 0;
                gridProdWithCat.DataSource = dt;
                gridProdWithCat.DataBind();
                ViewState["dtprocat"] = dt;
                lblStep2Msg.Text = "";
            }
            else
            {
                gridProdWithCat.DataSource = null;
                gridProdWithCat.DataBind();
                lblStep2Msg.Text = "No results found.";
            }
            DbCL.Conn.Close();
        }

        protected void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtSearchProduct.Text = "";
            if (cmbproduct_service.SelectedIndex > 0) BindProducts();
            else { gridProdWithCat.DataSource = null; gridProdWithCat.DataBind(); ViewState["dtprocat"] = null; }
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
        // SECTION 3: DATA BINDING
        // ==========================================================================================
        private void BindClients()
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                string query = "SELECT Client_Name FROM tbl_Client WHERE CompanyID = @CompanyID ORDER BY Client_Name";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        cmbClient.DataSource = rdr;
                        cmbClient.DataTextField = "Client_Name";
                        cmbClient.DataBind();
                    }
                }
            }
            cmbClient.Items.Insert(0, "-- Select Client --");
        }
        private void BindCategories()
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                string query = "SELECT ProductOrServiceCat FROM tbl_NewparentProduct ORDER BY ProductOrServiceCat";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        cmbproduct_service.DataSource = rdr;
                        cmbproduct_service.DataTextField = "ProductOrServiceCat";
                        cmbproduct_service.DataBind();
                    }
                }
            }
            cmbproduct_service.Items.Insert(0, "-- Select Category --");
        }

        protected void cmbClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbClient.SelectedIndex > 0)
            {
                DbCL.Sqlconnection(); DbCL.ConnectDb();
                string query = "SELECT Client_Id, Address1, Address2, City, State, pin, Service_tax_no FROM tbl_Client WHERE Client_Name=@Name AND CompanyID = @CompanyID";
                SqlParameter[] pram = { new SqlParameter("@Name", cmbClient.Text), new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID) };
                DataTable dt = DbCL.SPreturn_dt(query, pram);
                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    lblclientID.Text = dr["Client_Id"].ToString();
                    lblClientGST.Text = dr["Service_tax_no"].ToString();
                    string addr = dr["Address1"].ToString();
                    if (!string.IsNullOrEmpty(dr["Address2"].ToString())) addr += ", " + dr["Address2"].ToString();
                    if (!string.IsNullOrEmpty(dr["pin"].ToString())) addr += " - " + dr["pin"].ToString();
                    lblClientAddress.Text = addr;
                    lblClientState.Text = dr["State"].ToString();
                    lblPlaceOfSupply.Text = dr["City"].ToString();
                    pnlClientInfo.Visible = true;
                }
                DbCL.Conn.Close();
            }
            else
            {
                lblclientID.Text = "-"; pnlClientInfo.Visible = false;
            }
        }

        protected void Button3_Click(object sender, EventArgs e) { BindProducts(); }
        protected void cmbproduct_service_SelectedIndexChanged(object sender, EventArgs e) { BindProducts(); }

        private void BindProducts()
        {
            if (cmbproduct_service.SelectedIndex <= 0) return;
            string sql = "select * from tbl_NewProduct where ProductOrServiceCat=@Cat AND DeleteMode=0 order by ProductName";
            SqlParameter[] pram = { new SqlParameter("@Cat", cmbproduct_service.Text) };
            DataTable dt = DbCL.SPreturn_dt(sql, pram);
            if (dt.Rows.Count > 0)
            {
                gridProdWithCat.PageIndex = 0;
                gridProdWithCat.DataSource = dt;
                gridProdWithCat.DataBind();
                ViewState["dtprocat"] = dt;
            }
            else
            {
                gridProdWithCat.DataSource = null; gridProdWithCat.DataBind();
            }
        }

        // ==========================================================================================
        // SECTION 4: ADD TO CART
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
                        string pid = lblId != null ? lblId.Text : "";

                        bool exists = dtPCat.AsEnumerable().Any(r => r["ProductId"].ToString() == pid);
                        if (!exists && !string.IsNullOrEmpty(pid))
                        {
                            DataRow dr = dtPCat.NewRow();
                            dr["ProductId"] = pid;
                            dr["Product_code"] = ((Label)row.FindControl("Product_code")).Text;
                            dr["ProductName"] = ((Label)row.FindControl("ProductName")).Text;
                            dr["Brand"] = ((Label)row.FindControl("Brand")).Text;
                            dr["Type"] = ((Label)row.FindControl("Type")).Text;
                            dr["Unit"] = ((Label)row.FindControl("Unit")).Text;
                            dr["ProductOrServiceCat"] = ((Label)row.FindControl("ProductOrServiceCat")).Text;
                            dr["Tax_Rate"] = ((Label)row.FindControl("Tax_Rate")).Text;
                            dr["Sail_Rate"] = ((TextBox)row.FindControl("Sail_Rate")).Text;
                            dr["SQuantity"] = ((TextBox)row.FindControl("IQuantity")).Text;
                            dr["Discount_Rate"] = ((TextBox)row.FindControl("Discount_Rate")).Text;
                            dr["ItemRemarks"] = ((TextBox)row.FindControl("ItemRemarks")).Text;
                            dtPCat.Rows.Add(dr);
                            addedCount++;
                        }
                    }
                }
            }
            ViewState["PhaseProductData"] = dtPCat;
            if (addedCount > 0)
            {
                lblStep2Msg.Text = addedCount + " items added. Click 'Review' when done.";
                lblStep2Msg.ForeColor = System.Drawing.Color.Green;
            }
        }

        // ==========================================================================================
        // SECTION 5: REORDER & DELETE LOGIC
        // ==========================================================================================

        // 1. Sync User Input from Grid back to DataTable (Critical before reorder/delete/save)
        private void UpdateDataTableFromGrid()
        {
            DataTable dt = ViewState["PhaseProductData"] as DataTable;
            if (dt != null)
            {
                for (int i = 0; i < gd_Service_Product.Rows.Count; i++)
                {
                    GridViewRow row = gd_Service_Product.Rows[i];
                    dt.Rows[i]["SQuantity"] = ((TextBox)row.FindControl("IQuantity")).Text;
                    dt.Rows[i]["Sail_Rate"] = ((TextBox)row.FindControl("Sail_Rate")).Text;
                    dt.Rows[i]["Discount_Rate"] = ((TextBox)row.FindControl("Discount_Rate")).Text;
                }
                ViewState["PhaseProductData"] = dt;
            }
        }

        private void BindReviewGrid()
        {
            gd_Service_Product.DataSource = (DataTable)ViewState["PhaseProductData"];
            gd_Service_Product.DataBind();
        }

        // 2. Handle Reordering (Up/Down)
        protected void gd_Service_Product_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Delete") return; // Let RowDeleting handle this

            if (e.CommandName == "MoveUp" || e.CommandName == "MoveDown")
            {
                UpdateDataTableFromGrid(); // Save current values first
                int index = Convert.ToInt32(e.CommandArgument);
                DataTable dt = ViewState["PhaseProductData"] as DataTable;

                if (e.CommandName == "MoveUp" && index > 0)
                {
                    DataRow row = dt.Rows[index];
                    DataRow newRow = dt.NewRow();
                    newRow.ItemArray = row.ItemArray; // Clone
                    dt.Rows.RemoveAt(index);
                    dt.Rows.InsertAt(newRow, index - 1);
                }
                else if (e.CommandName == "MoveDown" && index < dt.Rows.Count - 1)
                {
                    DataRow row = dt.Rows[index];
                    DataRow newRow = dt.NewRow();
                    newRow.ItemArray = row.ItemArray; // Clone
                    dt.Rows.RemoveAt(index);
                    dt.Rows.InsertAt(newRow, index + 1);
                }
                ViewState["PhaseProductData"] = dt;
                BindReviewGrid();
            }
        }

        // 3. Handle Deletion
        protected void gd_Service_Product_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            UpdateDataTableFromGrid();
            DataTable dt = ViewState["PhaseProductData"] as DataTable;
            if (dt != null && e.RowIndex < dt.Rows.Count)
            {
                dt.Rows.RemoveAt(e.RowIndex);
                ViewState["PhaseProductData"] = dt;
                BindReviewGrid();
            }
        }

        // ==========================================================================================
        // SECTION 6: FINAL SAVE
        // ==========================================================================================
        protected void btn_finalsave_Click(object sender, EventArgs e)
        {
            UpdateDataTableFromGrid(); // Sync last second edits
            DataTable dtFinal = ViewState["PhaseProductData"] as DataTable;
            if (dtFinal == null || dtFinal.Rows.Count == 0) return;

            StringBuilder logBuilder = new StringBuilder();
            logBuilder.AppendLine($"Log Start: {DateTime.Now}");

            try
            {
                decimal totalBeforeTax = 0, totalTax = 0, totalNet = 0;
                string invoiceNo = BindInvoiceNo();
                int slNo = idreturn() + 1;
                string userId = Session["USERID"]?.ToString() ?? "FLM03";

                // 1. Calculate Totals
                foreach (DataRow dr in dtFinal.Rows)
                {
                    decimal qty = Convert.ToDecimal(dr["SQuantity"]);
                    decimal rate = Convert.ToDecimal(dr["Sail_Rate"]);
                    decimal taxPer = Convert.ToDecimal(dr["Tax_Rate"]);
                    decimal discPer = Convert.ToDecimal(dr["Discount_Rate"]);

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

                string queryHeader = @"INSERT INTO tbl_Proforma (Invoice_No, Invoice_Date, Quotation_No, Quotation_Date, Client_ID, Gross, Service_Tax, Net_Amount, Sl_no, subtotal, cgstOrsgst, igst, PlaceofSupply, mailStatus) VALUES (@InvNo, @InvDate, 'Direct', '', @ClientID, @Gross, @Tax, @Net, @SlNo, @SubTotal, @CGST, @IGST, @Place, 'Pending')";

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
                    new SqlParameter("@Place", lblPlaceOfSupply.Text)
                };
                DbCL.SPExecDB(queryHeader, headerParams.ToArray());

                // 3. Insert Details
                int itemSl = 1; // FIX: Correct sequence counter
                foreach (DataRow dr in dtFinal.Rows)
                {
                    decimal qty = Convert.ToDecimal(dr["SQuantity"]);
                    decimal rate = Convert.ToDecimal(dr["Sail_Rate"]);
                    decimal taxPer = Convert.ToDecimal(dr["Tax_Rate"]);

                    decimal amountBeforeTax = Math.Round(qty * rate, 2);
                    decimal taxAmt = Math.Round((amountBeforeTax * taxPer) / 100, 2);
                    decimal netAmt = amountBeforeTax + taxAmt;

                    string queryDet = @"INSERT INTO tbl_Proforma_Details (Invoice_No, Sl_no, Product_id, Product_Code, Product_name, Quantity, Rate, Tax_Rate, Tax_Amount, Total_Amount, Net_Amount, Unit, ProductOrServiceCat, AddedById) VALUES (@InvNo, @Sl, @Pid, @Pcode, @Pname, @Qty, @Rate, @TaxPer, @TaxAmt, @Total, @Net, @Unit, @Cat, @UserId)";

                    List<SqlParameter> detParams = new List<SqlParameter> {
                        new SqlParameter("@InvNo", invoiceNo),
                        new SqlParameter("@Sl", itemSl),
                        new SqlParameter("@Pid", dr["ProductId"]),
                        new SqlParameter("@Pcode", dr["Product_code"]),
                        new SqlParameter("@Pname", dr["ProductName"]),
                        new SqlParameter("@Qty", qty),
                        new SqlParameter("@Rate", rate),
                        new SqlParameter("@TaxPer", taxPer),
                        new SqlParameter("@TaxAmt", taxAmt),
                        new SqlParameter("@Total", amountBeforeTax),
                        new SqlParameter("@Net", netAmt),
                        new SqlParameter("@Unit", dr["Unit"]),
                        new SqlParameter("@Cat", dr["ProductOrServiceCat"]),
                        new SqlParameter("@UserId", userId)
                    };
                    DbCL.SPExecDB(queryDet, detParams.ToArray());
                    itemSl++;
                }

                logBuilder.AppendLine($"Success: {invoiceNo}");
                WriteLog(logBuilder.ToString());

                lblMessage.Text = "Proforma Saved: " + invoiceNo;
                lblMessage.ForeColor = System.Drawing.Color.Green;
                PanelMsg.Visible = true;
                btn_finalsave.Visible = false;
                Panel2.Enabled = false;
            }
            catch (Exception ex)
            {
                WriteLog("Error: " + ex.Message);
                lblMessage.Text = "An error occurred while saving the proforma invoice. Please try again.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                PanelMsg.Visible = true;
            }
        }

        // ==========================================================================================
        // SECTION 7: HELPERS
        // ==========================================================================================
        private string BindInvoiceNo()
        {
            string c = cmbClient.Text.Trim();
            string f = c.Substring(0, 1);
            return "PINV/" + f + "/" + findmonth() + (idreturn() + 1);
        }

        private int idreturn()
        {
            string date1 = txtinvoiceDate.Text;
            string date2 = date1.Substring(3, 3);
            string date3 = date1.Substring(7, 4);
            string date5, date6;

            if (date2 == "Jan" || date2 == "Feb" || date2 == "Mar")
            {
                string prevYear = (Convert.ToInt32(date3) - 1).ToString();
                date5 = "31-Mar-" + prevYear; date6 = "31-Mar-" + date3;
            }
            else
            {
                string nextYear = (Convert.ToInt32(date3) + 1).ToString();
                date5 = "31-Mar-" + date3; date6 = "31-Mar-" + nextYear;
            }

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                string cmdstring = "SELECT Sl_no FROM tbl_Proforma WHERE ID = (SELECT MAX(ID) FROM tbl_Proforma WHERE CAST(Invoice_Date AS datetime) BETWEEN @Date5 AND @Date6)";
                using (SqlCommand cmd = new SqlCommand(cmdstring, conn))
                {
                    cmd.Parameters.AddWithValue("@Date5", date5);
                    cmd.Parameters.AddWithValue("@Date6", date6);
                    conn.Open();
                    object res = cmd.ExecuteScalar();
                    return (res != null && res != DBNull.Value) ? Convert.ToInt32(res) : 0;
                }
            }
        }

        private string findmonth()
        {
            DateTime dt = DateTime.Parse(txtinvoiceDate.Text);
            string yr = dt.Year.ToString().Substring(2, 2);
            string yrNext = (dt.Year + 1).ToString().Substring(2, 2);
            string yrPrev = (dt.Year - 1).ToString().Substring(2, 2);

            if (dt.Month >= 4) return yr + "-" + yrNext + "/"; // Apr-Dec
            else return yrPrev + "-" + yr + "/"; // Jan-Mar
        }

        private void WriteLog(string content)
        {
            try
            {
                string logDir = Server.MapPath("~/Uploads/ProformaLogs");
                if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
                File.WriteAllText(Path.Combine(logDir, $"Log_{DateTime.Now:yyyyMMddHHmmss}.txt"), content);
            }
            catch { }
        }
    }
}