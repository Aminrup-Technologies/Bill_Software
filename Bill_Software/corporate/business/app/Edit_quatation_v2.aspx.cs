/*
======================================================================================
When: 01-Mar-2026
Why:  1. To refactor Edit_Quotation.aspx into a high-performance Wizard UI matching the Create flow.
      2. Removed heavy C# 7.0 inline variables (out decimal) to ensure compilation.
      3. Fixed data mapping to prevent Product_id (HSN) and Product_Code (PRD) swapping.
      4. Ensured robust logic for both 'Update' and 'Recreate New Version' features.
======================================================================================
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.UI.WebControls;
using System.Text;

namespace Bill_Software.corporate.business.app
{
    public partial class Edit_quatation_v2 : System.Web.UI.Page
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
            }
            if (!IsPostBack)
            {
                DbCL.FillCombo(cmbvendor, "select Client_Name from tbl_Client order by Client_Name");
                DbCL.FillComboNew(cmbClient, "SELECT Client_Id, Client_Name FROM tbl_Client ORDER by Client_Name");
                DbCL.FillCombo(ddlPlaceOfSupply, "Select City_Name from tbl_City order by City_Name asc");
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                WizardMultiView.ActiveViewIndex = 0;
            }
        }

        private void ShowAlert(string message, bool isError)
        {
            PanelGlobalAlert.Visible = true;
            lblGlobalAlert.Text = message;
            PanelGlobalAlert.BackColor = isError ? System.Drawing.Color.FromArgb(255, 238, 238) : System.Drawing.Color.FromArgb(238, 255, 221);
            lblGlobalAlert.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.Green;
        }

        // ================= WIZARD VIEW 0: SEARCH & SELECT =================

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string clientId = GetClientId(cmbvendor.Text);

            // We start with a clean, parameterized base query using OUTER APPLY to grab the newest service
            string cmdstring = @"
            SELECT 
                s.PServiceName,
                q.ID,
                q.service_tax1,
                q.sub_total,
                q.Quotation_no,
                q.Quotation_date,
                q.Gross,
                q.Service_tax,
                q.Net_amount,
                q.mailStatusDate,
                c.Client_Name, 
                q.RecordType 
            FROM tbl_Quotation q
            LEFT OUTER JOIN tbl_Client c ON q.Client_Id = c.Client_Id 
            OUTER APPLY (
                SELECT TOP 1 PServiceName 
                FROM tbl_QuoPriSerTogather 
                WHERE qutno = q.Quotation_no 
                ORDER BY TimeStamp DESC
            ) s
            WHERE 1=1 "; // 1=1 makes appending conditions easier!

            SqlCommand cmd = new SqlCommand();

            // Safely append conditions and parameters based on the radio button selected
            if (RadioButtonList1.SelectedIndex == 0) // Only Client
            {
                cmdstring += " AND q.Client_Id = @ClientId ";
                cmd.Parameters.AddWithValue("@ClientId", clientId);
            }
            else if (RadioButtonList1.SelectedIndex == 1) // Only Date
            {
                cmdstring += " AND CAST(q.Quotation_date AS DATETIME) BETWEEN @FromDate AND @ToDate ";
                cmd.Parameters.AddWithValue("@FromDate", txtfromDate.Text);
                cmd.Parameters.AddWithValue("@ToDate", txttodate.Text);
            }
            else // Client & Date
            {
                cmdstring += " AND q.Client_Id = @ClientId ";
                cmdstring += " AND CAST(q.Quotation_date AS DATETIME) BETWEEN @FromDate AND @ToDate ";
                cmd.Parameters.AddWithValue("@ClientId", clientId);
                cmd.Parameters.AddWithValue("@FromDate", txtfromDate.Text);
                cmd.Parameters.AddWithValue("@ToDate", txttodate.Text);
            }

            // Append the final ordering logic
            cmdstring += " ORDER BY q.ID DESC";

            cmd.CommandText = cmdstring;

            // Database connection and execution
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            cmd.Connection = DbCL.Conn;

            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    DataList1.DataSource = dt;
                    DataList1.DataBind();
                    PanelGlobalAlert.Visible = false;
                }
                else
                {
                    DataList1.DataSource = null;
                    DataList1.DataBind();
                    ShowAlert("No records found.", true);
                }
            }

            // Clean up
            DbCL.Conn.Close();
            cmd.Dispose();
        }

        private string GetClientId(string clientName)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT Client_Id FROM tbl_Client WHERE Client_Name=@name", con))
            {
                cmd.Parameters.AddWithValue("@name", clientName);
                con.Open();
                object res = cmd.ExecuteScalar();
                return res != null ? res.ToString() : "";
            }
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/Edit_quatation_v2.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            if (e.CommandName == "Select")
            {
                string Quotation_no = Convert.ToString(e.CommandArgument);
                lbl_recordno.Text = Quotation_no;
                lblqno.Text = Quotation_no;

                // Load the Cart Data into ViewState
                string cmdstring = "select Product_Code as ProductId, Product_id as Product_code, Product_name as ProductName, Type, sail_rate as Sail_Rate, Service_tax_rate as Tax_Rate, Unit, Quantity, ProductOrServiceCat, specification as Brand, Misc as Specification, ItemNo, MaterialNo, PackSize, ItemRemarks, discount_rate as Discount_Rate, Sl_no, DeliveryDate, Department from tbl_Quotaion_details where Quotation_no=@Quotation_no AND IsLatest = 1 AND IsDeleted = 0 order by CAST(Sl_no as int)";
                SqlParameter[] pram = { new SqlParameter("@Quotation_no", Quotation_no) };
                DataTable dtLoaded = DbCL.SPreturn_dt(cmdstring, pram);

                ViewState["PhaseProductData"] = dtLoaded;
                gd_Service_Product.DataSource = dtLoaded;
                gd_Service_Product.DataBind();

                Bindcombo();

                // =========================================================================
                // NEW UX LOGIC: Pre-select Category & Auto-Load Grid in Step 2 
                // based on the original data loaded in the cart.
                // =========================================================================
                if (dtLoaded != null && dtLoaded.Rows.Count > 0)
                {
                    string originalCategory = dtLoaded.Rows[0]["ProductOrServiceCat"].ToString();
                    ListItem catItem = cmbproduct_service.Items.FindByText(originalCategory);

                    if (catItem == null)
                    {
                        catItem = cmbproduct_service.Items.FindByValue(originalCategory);
                    }

                    if (catItem != null)
                    {
                        cmbproduct_service.ClearSelection();
                        catItem.Selected = true;

                        // Auto-load the Catalog Grid for this category
                        PanelCatalogGrid.Visible = true;
                        string catQuery = "select Id, Product_code, ProductID, ProductOrServiceCat, Brand, ProductName, Specification, Type,Sail_Rate,Tax_Rate,Unit from tbl_NewProduct where ProductOrServiceCat=@Cat order by Id,ProductName";
                        SqlParameter[] catParam = { new SqlParameter("@Cat", originalCategory) };
                        DataTable dtproductWithCat = DbCL.SPreturn_dt(catQuery, catParam);

                        if (dtproductWithCat.Rows.Count > 0)
                        {
                            gridProdWithCat.DataSource = dtproductWithCat;
                            gridProdWithCat.DataBind();
                            ViewState["dtprocat"] = dtproductWithCat;
                        }
                    }
                }
                // =========================================================================

                BindQuotationDetails(Quotation_no);
                BindPaymentPhases(Quotation_no);
                LoadPrimaryServices(Quotation_no);
                ToggleGridColumns();

                WizardMultiView.ActiveViewIndex = 1; // Move to Step 1
            }
        }

        // ================= WIZARD VIEW 1: BASIC DETAILS =================

        protected void btnNext1_Click(object sender, EventArgs e)
        {
            PanelGlobalAlert.Visible = false;
            WizardMultiView.ActiveViewIndex = 2; // Move to Step 2 (Catalog)
        }

        private void BindQuotationDetails(string quotationNo)
        {
            string query = "SELECT * FROM tbl_Quotation WHERE Quotation_no = @Quotation_no";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Quotation_no", quotationNo);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        DateTime tempDate;
                        if (DateTime.TryParseExact(reader["Quotation_date"].ToString(), "dd-MMM-yyyy", null, System.Globalization.DateTimeStyles.None, out tempDate))
                            txtquotationDate.Text = tempDate.ToString("dd-MMM-yyyy");
                        else
                            txtquotationDate.Text = reader["Quotation_date"].ToString();

                        string cid = reader["Client_Id"].ToString();
                        if (cmbClient.Items.FindByValue(cid) != null) cmbClient.SelectedValue = cid;

                        txt_valdays.Text = reader["ValidityDays"].ToString();
                        SetDropdownValue(DDL_ItemViewType, reader["DetailedView"].ToString());
                        SetDropdownValue(DDL_DiscountView, reader["DiscountView"].ToString());
                        SetDropdownValue(ddlPlaceOfSupply, reader["PlaceofSupply"].ToString());

                        string delTenure = reader["DeliveryTenure"].ToString();
                        if (DDL_DeliveryTerms.Items.FindByText(delTenure) != null)
                        {
                            SetDropdownValue(DDL_DeliveryTerms, delTenure);
                            manualInputRow.Style["display"] = "none";
                        }
                        else
                        {
                            DDL_DeliveryTerms.SelectedValue = "4";
                            txt_deltrms.Text = delTenure;
                            manualInputRow.Style["display"] = "";
                        }

                        string pkgCharges = reader["PackingCharges"].ToString();
                        if (DDL_pkgfrwd.Items.FindByText(pkgCharges) != null)
                        {
                            SetDropdownValue(DDL_pkgfrwd, pkgCharges);
                            manualInputPkgRow.Style["display"] = "none";
                        }
                        else
                        {
                            DDL_pkgfrwd.SelectedValue = "3";
                            txt_pkgfrwd.Text = pkgCharges;
                            manualInputPkgRow.Style["display"] = "";
                        }

                        txt_remarks.Text = reader["Remarks"].ToString();
                        txt_tcs_amnt.Text = reader["TCS_Amount"] != DBNull.Value ? Convert.ToDecimal(reader["TCS_Amount"]).ToString("0.00") : "0.00";
                        txt_tcs_percent.Text = reader["TCS_Percent"] != DBNull.Value ? Convert.ToDecimal(reader["TCS_Percent"]).ToString("0.00") : "0.00";
                        txt_delivery_amnt.Text = reader["Freight_Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Freight_Amount"]).ToString("0.00") : "0.00";
                        txt_freight_percent.Text = reader["Freight_VAT_Percent"] != DBNull.Value ? Convert.ToDecimal(reader["Freight_VAT_Percent"]).ToString("0.00") : "0.00";
                        TextBox1.Text = reader["OtherCharge_Name"].ToString();
                        txt_othr_amnt.Text = reader["OtherCharge_Amount"] != DBNull.Value ? Convert.ToDecimal(reader["OtherCharge_Amount"]).ToString("0.00") : "0.00";

                        string rType = reader["RecordType"].ToString();
                        if (rType == "Quotation")
                        {
                            rbQt.Checked = true; rbPo.Checked = false; PO_DataInputs.Visible = false;
                        }
                        else
                        {
                            rbQt.Checked = false; rbPo.Checked = true; PO_DataInputs.Visible = true;
                            txb_ponumber.Text = reader["PO_Number"].ToString();
                            txb_donumber.Text = reader["DO_Number"].ToString();
                            txb_podate.Text = FormatDate(reader["PO_Date"].ToString());
                            txb_strtdt.Text = FormatDate(reader["Validity_StartDate"].ToString());
                            txb_enddt.Text = FormatDate(reader["Validity_EndDate"].ToString());
                        }

                        RadioButtonGst.SelectedValue = reader["cgstOrsgst"].ToString().Trim().ToUpper() == "YES" ? "1" : "0";

                        if (reader["ReferenceData"].ToString() == "Yes")
                        {
                            rbYes.Checked = true; rbNo.Checked = false;
                            txt_clientrefname.Text = reader["ReferenceName"].ToString();
                            txt_clientrefid.Text = reader["ReferenceId"].ToString();
                            txt_clientrefdate.Text = FormatDate(reader["ReferenceDate"].ToString());
                        }
                        else
                        {
                            rbYes.Checked = false; rbNo.Checked = true;
                            txt_clientrefname.Text = "N/A"; txt_clientrefid.Text = "N/A"; txt_clientrefdate.Text = "01-Jan-2000";
                        }
                    }
                }
            }
        }

        private void SetDropdownValue(DropDownList ddl, string text)
        {
            ListItem item = ddl.Items.FindByText(text);
            ddl.ClearSelection();
            if (item != null) item.Selected = true;
        }

        // ================= WIZARD VIEW 2: CATALOG =================

        private void Bindcombo()
        {
            string cmdstring = "select ProductOrServiceCat from tbl_NewparentProduct order by ProductOrServiceCat asc";
            cmbproduct_service.Items.Clear();
            cmbproduct_service.Items.Add("--Select--");
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(cmdstring, con))
            {
                con.Open();
                using (SqlDataReader re = cmd.ExecuteReader()) { while (re.Read()) cmbproduct_service.Items.Add(re.GetValue(0).ToString()); }
            }
        }

        protected void btnPrev2_Click(object sender, EventArgs e) { WizardMultiView.ActiveViewIndex = 1; }

        protected void Button2_Click(object sender, EventArgs e)
        {
            PanelCatalogGrid.Visible = true;
            string cmdstring = "select Id, Product_code, ProductID, ProductOrServiceCat, Brand, ProductName, Specification, Type,Sail_Rate,Tax_Rate,Unit from tbl_NewProduct where ProductOrServiceCat=@Cat order by Id,ProductName";
            SqlParameter[] pram = { new SqlParameter("@Cat", cmbproduct_service.Text) };
            DataTable dtproductWithCat = DbCL.SPreturn_dt(cmdstring, pram);
            if (dtproductWithCat.Rows.Count > 0)
            {
                gridProdWithCat.DataSource = dtproductWithCat;
                gridProdWithCat.DataBind();
                ViewState["dtprocat"] = dtproductWithCat;
            }
        }

        protected void btnNext2_Click(object sender, EventArgs e)
        {
            PanelGlobalAlert.Visible = false;
            DataTable dtpro = ViewState["dtprocat"] as DataTable;

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
                dtPCat.Columns.Add("Sl_no", typeof(string));
                ViewState["PhaseProductData"] = dtPCat;
            }
            else
            {
                dtPCat = (DataTable)ViewState["PhaseProductData"];
            }

            if (dtpro != null)
            {
                for (int i = 0; i < gridProdWithCat.Rows.Count; i++)
                {
                    CheckBox chkdtp = (CheckBox)gridProdWithCat.Rows[i].FindControl("chkdtp");
                    if (chkdtp != null && chkdtp.Checked)
                    {
                        DataRow dr = dtPCat.NewRow();
                        dr["ProductId"] = ((Label)gridProdWithCat.Rows[i].FindControl("ProductID")).Text; // PRD 
                        dr["Product_code"] = ((Label)gridProdWithCat.Rows[i].FindControl("Product_code")).Text; // HSN
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

                        if (dtPCat.Columns.Contains("DeliveryDate")) dr["DeliveryDate"] = "";
                        if (dtPCat.Columns.Contains("Department")) dr["Department"] = "";
                        if (dtPCat.Columns.Contains("Sl_no")) dr["Sl_no"] = "0";

                        dtPCat.Rows.Add(dr);
                        chkdtp.Checked = false;
                    }
                }
            }

            ViewState["PhaseProductData"] = dtPCat;
            gd_Service_Product.DataSource = dtPCat;
            gd_Service_Product.DataBind();
            ToggleGridColumns();
            TakePservice(cmbproduct_service.Text);
            WizardMultiView.ActiveViewIndex = 3; // Move to Cart
        }

        private void TakePservice(string pservice)
        {
            if (string.IsNullOrWhiteSpace(pservice) || pservice == "--Select--") return;

            if (ViewState["pService"] == null)
            {
                dtPCat1.Columns.Add("ProductCatagory", typeof(string));
                ViewState["pService"] = dtPCat1;
            }
            dtPCat1 = (DataTable)ViewState["pService"];

            bool exists = false;
            foreach (DataRow row in dtPCat1.Rows) { if (row["ProductCatagory"].ToString() == pservice) exists = true; }
            if (!exists)
            {
                DataRow dr = dtPCat1.NewRow();
                dr["ProductCatagory"] = pservice;
                dtPCat1.Rows.Add(dr);
            }
            gridps.DataSource = dtPCat1; gridps.DataBind();
            ViewState["pService"] = dtPCat1;
        }

        // ================= WIZARD VIEW 3: CART =================

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
                    dt.Rows[i]["ItemNo"] = ((TextBox)row.FindControl("ItemNo"))?.Text ?? "";
                    dt.Rows[i]["MaterialNo"] = ((TextBox)row.FindControl("MaterialNo"))?.Text ?? "";
                    dt.Rows[i]["PackSize"] = ((TextBox)row.FindControl("PackSize"))?.Text ?? "";
                    dt.Rows[i]["ItemRemarks"] = ((TextBox)row.FindControl("ItemRemarks")).Text;

                    if (!rbQt.Checked && dt.Columns.Contains("DeliveryDate"))
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
            WizardMultiView.ActiveViewIndex = 2; // Back to Step 2
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
            WizardMultiView.ActiveViewIndex = 2;
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

            bindphaseType(lblqno.Text);
            WizardMultiView.ActiveViewIndex = 4; // Move to Terms
        }

        protected void btnPrev4_Click(object sender, EventArgs e) { WizardMultiView.ActiveViewIndex = 3; }


        // ================= WIZARD VIEW 4: TERMS & SAVE =================

        protected void btnSabe_Click(object sender, EventArgs e) { DataUpdaterMethod(); }
        protected void btnNew_Click(object sender, EventArgs e) { MagicianNew(); }

        private void DataUpdaterMethod()
        {
            string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";
            string qno = lblqno.Text;

            // 1. Validation Check (Proforma/Tax/Payment)
            string query = "select Status1, Status2, PaymentStatus from tbl_Quotation where Quotation_no=@Quotation_no";
            SqlParameter[] checkParam = { new SqlParameter("@Quotation_no", qno) };
            DataTable dtProInvPay = DbCL.SPreturn_dt(query, checkParam);

            if (dtProInvPay.Rows.Count > 0)
            {
                string pro = dtProInvPay.Rows[0]["Status1"].ToString();
                string inv = dtProInvPay.Rows[0]["Status2"].ToString();
                string pay = dtProInvPay.Rows[0]["PaymentStatus"].ToString();

                if (pro == "Yes" || inv == "Yes" || pay == "Yes")
                {
                    ShowAlert("Cannot update. Please delete associated invoices first.", true);
                    return;
                }
            }

            decimal new_sub_total = 0, new_total_Service = 0, new_Gross_amount = 0;
            string cnnString = ConfigurationManager.ConnectionStrings["DbConn"].ToString();

            using (SqlConnection conn = new SqlConnection(cnnString))
            {
                conn.Open();
                using (SqlCommand lockCmd = new SqlCommand("sp_getapplock", conn))
                {
                    lockCmd.CommandType = CommandType.StoredProcedure;
                    lockCmd.Parameters.AddWithValue("@Resource", "Lock_Quotation_" + qno);
                    lockCmd.Parameters.AddWithValue("@LockMode", "Exclusive");
                    lockCmd.Parameters.AddWithValue("@LockOwner", "Session");
                    lockCmd.Parameters.AddWithValue("@DbPrincipal", "public");

                    SqlParameter returnCode = new SqlParameter("@return_value", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                    lockCmd.Parameters.Add(returnCode); lockCmd.ExecuteNonQuery();

                    if ((int)returnCode.Value < 0) { ShowAlert("System is currently processing this record. Try again.", true); return; }
                }

                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    // 2. Determine Next Integer Version
                    string versionQuery = "SELECT ISNULL(MAX(Version), 0) + 1 FROM tbl_Quotaion_details WHERE Quotation_no = @Quotation_no";
                    using (SqlCommand vCmd = new SqlCommand(versionQuery, conn, trans))
                    {
                        vCmd.Parameters.AddWithValue("@Quotation_no", qno);
                        int newVersion = Convert.ToInt32(vCmd.ExecuteScalar());

                        // 3. Soft delete old latest records
                        string softDeleteQuery = @"UPDATE tbl_Quotaion_details SET IsDeleted = 1, IsLatest = 0, DeletedById = @DeletedById, DeletedOn = GETDATE() WHERE Quotation_no = @Quotation_no AND IsDeleted = 0 AND IsLatest = 1";
                        using (SqlCommand sCmd = new SqlCommand(softDeleteQuery, conn, trans))
                        {
                            sCmd.Parameters.AddWithValue("@Quotation_no", qno);
                            sCmd.Parameters.AddWithValue("@DeletedById", userId);
                            sCmd.ExecuteNonQuery();
                        }

                        // 4. Insert New Details
                        int h = 0;
                        foreach (GridViewRow row in gd_Service_Product.Rows)
                        {
                            CheckBox chk = (CheckBox)row.FindControl("chk"); // hidden in cart, forced checked
                            if (chk != null && chk.Checked)
                            {
                                h++;
                                decimal Quantity = ParseDecimal(((TextBox)row.FindControl("Quantity")).Text);
                                decimal Sail_Rate = ParseDecimal(((TextBox)row.FindControl("Sail_Rate")).Text);
                                decimal Tax_Rate = ParseDecimal(((Label)row.FindControl("Tax_Rate")).Text);
                                decimal Discount_Rate = ParseDecimal(((TextBox)row.FindControl("Discount_Rate")).Text);

                                decimal discounted_rate = Sail_Rate - (Sail_Rate * Discount_Rate / 100);
                                decimal taxMultiplier = (Tax_Rate + 100) / 100;
                                decimal Total_sail_rate = taxMultiplier * discounted_rate;
                                decimal Total_sail_rate1 = Total_sail_rate * Quantity;
                                decimal Total_sail_rate2 = discounted_rate * Quantity;
                                decimal Service_tax = (Tax_Rate * Quantity * discounted_rate) / 100;

                                new_sub_total += Total_sail_rate2;
                                new_total_Service += Service_tax;
                                new_Gross_amount += Total_sail_rate1;

                                using (SqlCommand cmd = new SqlCommand(@"INSERT INTO tbl_Quotaion_details 
                                (Sl_no, Quotation_no, Product_id, Product_Code, Product_name, Quantity, sail_rate, Service_tax_rate, discount_rate, new_sailrate, 
                                 Total_sail_rate, Total_sail_rate1, Total_sail_rate2, specification, Misc, InvStatus, Type, Unit, ProductOrServiceCat, 
                                 ItemRemarks, ItemNo, MaterialNo, PackSize, DeliveryDate, Department, AddedById, AddedOn, Version, IsDeleted, IsLatest)
                                VALUES 
                                (@Sl_no, @Quotation_no, @Product_id, @Product_Code, @Product_name, @Quantity, @sail_rate, @Service_tax_rate, @discount_rate, @new_sailrate, 
                                 @Total_sail_rate, @Total_sail_rate1, @Total_sail_rate2, @specification, @Misc, 'No', @Type, @Unit, @ProductOrServiceCat, 
                                 @ItemRemarks, @ItemNo, @MaterialNo, @PackSize, @DeliveryDate, @Department, @AddedById, GETDATE(), @Version, 0, 1)", conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@Sl_no", h);
                                    cmd.Parameters.AddWithValue("@Quotation_no", qno);
                                    cmd.Parameters.AddWithValue("@Product_id", ((Label)row.FindControl("Product_code"))?.Text ?? ""); // HSN Code mapped to Product_id
                                    cmd.Parameters.AddWithValue("@Product_Code", ((Label)row.FindControl("ProductID"))?.Text ?? ""); // PRD Custom ID mapped to Product_Code
                                    cmd.Parameters.AddWithValue("@Product_name", ((Label)row.FindControl("ProductName"))?.Text ?? "");
                                    cmd.Parameters.AddWithValue("@Quantity", Quantity);
                                    cmd.Parameters.AddWithValue("@sail_rate", Sail_Rate);
                                    cmd.Parameters.AddWithValue("@Service_tax_rate", Tax_Rate);
                                    cmd.Parameters.AddWithValue("@discount_rate", Discount_Rate);
                                    cmd.Parameters.AddWithValue("@new_sailrate", discounted_rate);
                                    cmd.Parameters.AddWithValue("@Total_sail_rate", Total_sail_rate);
                                    cmd.Parameters.AddWithValue("@Total_sail_rate1", Total_sail_rate1);
                                    cmd.Parameters.AddWithValue("@Total_sail_rate2", Total_sail_rate2);
                                    cmd.Parameters.AddWithValue("@specification", ((TextBox)row.FindControl("Brand"))?.Text ?? "");
                                    cmd.Parameters.AddWithValue("@Misc", ((TextBox)row.FindControl("Specification"))?.Text ?? "~");
                                    cmd.Parameters.AddWithValue("@Type", ((Label)row.FindControl("Type"))?.Text ?? "");
                                    cmd.Parameters.AddWithValue("@Unit", ((Label)row.FindControl("Unit"))?.Text ?? "");
                                    cmd.Parameters.AddWithValue("@ProductOrServiceCat", ((Label)row.FindControl("ProductOrServiceCat"))?.Text ?? "");
                                    cmd.Parameters.AddWithValue("@ItemRemarks", ((TextBox)row.FindControl("ItemRemarks"))?.Text ?? "");
                                    cmd.Parameters.AddWithValue("@ItemNo", ((TextBox)row.FindControl("ItemNo"))?.Text ?? "");
                                    cmd.Parameters.AddWithValue("@MaterialNo", ((TextBox)row.FindControl("MaterialNo"))?.Text ?? "");
                                    cmd.Parameters.AddWithValue("@PackSize", ((TextBox)row.FindControl("PackSize"))?.Text ?? "");
                                    cmd.Parameters.AddWithValue("@DeliveryDate", rbQt.Checked ? "" : ((TextBox)row.FindControl("DeliveryDate"))?.Text ?? "");
                                    cmd.Parameters.AddWithValue("@Department", rbQt.Checked ? "" : ((TextBox)row.FindControl("Department"))?.Text ?? "");
                                    cmd.Parameters.AddWithValue("@AddedById", userId);
                                    cmd.Parameters.AddWithValue("@Version", newVersion);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        // 5. Update Header Table (tbl_Quotation)
                        decimal tcsAmount = ParseDecimal(txt_tcs_amnt.Text);
                        decimal tcsPercent = ParseDecimal(txt_tcs_percent.Text);
                        decimal deliveryAmount = ParseDecimal(txt_delivery_amnt.Text);
                        decimal freightPercent = ParseDecimal(txt_freight_percent.Text);
                        decimal otherAmount = ParseDecimal(txt_othr_amnt.Text);
                        decimal finalNet = Math.Round(new_Gross_amount + tcsAmount + deliveryAmount + otherAmount, 2);

                        string updateHeader = @"UPDATE tbl_Quotation SET 
                        Gross = @Gross, Service_tax = @STax, Net_amount = @Net, service_tax1 = @STax1, sub_total = @SubT,
                        ValidityDays = @VDays, DeliveryTenure = @DTenure, PackingCharges = @PCharge, 
                        cgstOrsgst = @CGST, igst = @IGST, PlaceofSupply = @POS, 
                        ReferenceData = @RefD, ReferenceName = @RefN, ReferenceId = @RefI, ReferenceDate = @RefDt,
                        Remarks = @Remarks, DetailedView = @DView, DiscountView = @DiscView, 
                        RecordType = @RType, DO_Number = @DONum, PO_Number = @PONum, PO_Date = @PODt, 
                        Validity_StartDate = @VStart, Validity_EndDate = @VEnd,
                        TCS_Amount = @TCSA, TCS_Percent = @TCSP, Freight_Amount = @FrA, Freight_VAT_Percent = @FrP, 
                        OtherCharge_Name = @OthName, OtherCharge_Amount = @OthAmnt,
                        ModifiedById = @ModBy, ModifiedOn = GETDATE() 
                        WHERE Quotation_no = @QNo";

                        using (SqlCommand hCmd = new SqlCommand(updateHeader, conn, trans))
                        {
                            hCmd.Parameters.AddWithValue("@Gross", Math.Round(new_Gross_amount, 2));
                            hCmd.Parameters.AddWithValue("@STax", Math.Round(new_total_Service, 2));
                            hCmd.Parameters.AddWithValue("@Net", finalNet);
                            hCmd.Parameters.AddWithValue("@STax1", Math.Round(new_total_Service, 2));
                            hCmd.Parameters.AddWithValue("@SubT", new_sub_total);
                            hCmd.Parameters.AddWithValue("@VDays", txt_valdays.Text);
                            hCmd.Parameters.AddWithValue("@DTenure", DDL_DeliveryTerms.SelectedValue == "4" ? txt_deltrms.Text : DDL_DeliveryTerms.SelectedItem.Text);
                            hCmd.Parameters.AddWithValue("@PCharge", DDL_pkgfrwd.SelectedValue == "3" ? txt_pkgfrwd.Text : DDL_pkgfrwd.SelectedItem.Text);
                            hCmd.Parameters.AddWithValue("@CGST", RadioButtonGst.SelectedValue == "1" ? "YES" : "");
                            hCmd.Parameters.AddWithValue("@IGST", RadioButtonGst.SelectedValue == "0" ? "YES" : "");
                            hCmd.Parameters.AddWithValue("@POS", ddlPlaceOfSupply.SelectedItem.Text);
                            hCmd.Parameters.AddWithValue("@RefD", rbYes.Checked ? "Yes" : "No");
                            hCmd.Parameters.AddWithValue("@RefN", rbYes.Checked ? txt_clientrefname.Text : "N/A");
                            hCmd.Parameters.AddWithValue("@RefI", rbYes.Checked ? txt_clientrefid.Text : "N/A");
                            hCmd.Parameters.AddWithValue("@RefDt", rbYes.Checked ? GetSafeDate(txt_clientrefdate.Text) : (object)"1900-01-01");
                            hCmd.Parameters.AddWithValue("@Remarks", txt_remarks.Text);
                            hCmd.Parameters.AddWithValue("@DView", DDL_ItemViewType.SelectedItem.Text);
                            hCmd.Parameters.AddWithValue("@DiscView", DDL_DiscountView.SelectedItem.Text);
                            hCmd.Parameters.AddWithValue("@RType", rbPo.Checked ? "Purchase Order" : "Quotation");
                            hCmd.Parameters.AddWithValue("@DONum", rbPo.Checked ? txb_donumber.Text : "N/A");
                            hCmd.Parameters.AddWithValue("@PONum", rbPo.Checked ? txb_ponumber.Text : "N/A");
                            hCmd.Parameters.AddWithValue("@PODt", rbPo.Checked ? GetSafeDate(txb_podate.Text) : (object)"1900-01-01");
                            hCmd.Parameters.AddWithValue("@VStart", rbPo.Checked ? GetSafeDate(txb_strtdt.Text) : (object)"1900-01-01");
                            hCmd.Parameters.AddWithValue("@VEnd", rbPo.Checked ? GetSafeDate(txb_enddt.Text) : (object)"1900-01-01");
                            hCmd.Parameters.AddWithValue("@TCSA", tcsAmount);
                            hCmd.Parameters.AddWithValue("@TCSP", tcsPercent);
                            hCmd.Parameters.AddWithValue("@FrA", deliveryAmount);
                            hCmd.Parameters.AddWithValue("@FrP", freightPercent);
                            hCmd.Parameters.AddWithValue("@OthName", TextBox1.Text.Trim());
                            hCmd.Parameters.AddWithValue("@OthAmnt", otherAmount);
                            hCmd.Parameters.AddWithValue("@ModBy", userId);
                            hCmd.Parameters.AddWithValue("@QNo", qno);
                            hCmd.ExecuteNonQuery();
                        }

                        // Rebuild auxiliary data
                        insertPaymentPhaseNew(qno, conn, trans);
                        insertprimaryServiceNew(qno, conn, trans);

                        trans.Commit();
                        updatedueamountdetails(Convert.ToDouble(finalNet));

                        ShowAlert("Data Updated Successfully! Version: " + newVersion, false);
                        btnSabe.Visible = btnNew.Visible = false;
                    }
                }
                catch (Exception ex)
                {
                    try { trans?.Rollback(); } catch { }
                    StringBuilder errorMsg = new StringBuilder();
                    errorMsg.AppendLine("An error occurred: " + ex.Message);
                    if (ex.InnerException != null) errorMsg.AppendLine("<br/>Inner: " + ex.InnerException.ToString());
                    ShowAlert(errorMsg.ToString(), true);
                }
            }
        }

        private void MagicianNew()
        {
            Bindquotationno();
            string newRecordID = lblqno.Text;
            int slNo = idreturn() + 1;
            string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";

            decimal new_sub_total = 0, new_total_Service = 0, new_Gross_amount = 0;
            string cnnString = ConfigurationManager.ConnectionStrings["DbConn"].ToString();

            using (SqlConnection conn = new SqlConnection(cnnString))
            {
                conn.Open();
                using (SqlCommand lockCmd = new SqlCommand("sp_getapplock", conn))
                {
                    lockCmd.CommandType = CommandType.StoredProcedure;
                    lockCmd.Parameters.AddWithValue("@Resource", "Lock_Quotation_" + newRecordID);
                    lockCmd.Parameters.AddWithValue("@LockMode", "Exclusive");
                    lockCmd.Parameters.AddWithValue("@LockOwner", "Session");
                    lockCmd.Parameters.AddWithValue("@DbPrincipal", "public");
                    SqlParameter returnCode = new SqlParameter("@return_value", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                    lockCmd.Parameters.Add(returnCode); lockCmd.ExecuteNonQuery();

                    if ((int)returnCode.Value < 0) { ShowAlert("System processing. Try again.", true); return; }
                }

                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    int h = 0;
                    foreach (GridViewRow row in gd_Service_Product.Rows)
                    {
                        CheckBox chk = (CheckBox)row.FindControl("chk");
                        if (chk != null && chk.Checked)
                        {
                            h++;
                            decimal Quantity = ParseDecimal(((TextBox)row.FindControl("Quantity")).Text);
                            decimal Sail_Rate = ParseDecimal(((TextBox)row.FindControl("Sail_Rate")).Text);
                            decimal Tax_Rate = ParseDecimal(((Label)row.FindControl("Tax_Rate")).Text);
                            decimal Discount_Rate = ParseDecimal(((TextBox)row.FindControl("Discount_Rate")).Text);

                            decimal discounted_rate = Sail_Rate - (Sail_Rate * Discount_Rate / 100);
                            decimal taxMultiplier = (Tax_Rate + 100) / 100;
                            decimal Total_sail_rate = taxMultiplier * discounted_rate;
                            decimal Total_sail_rate1 = Total_sail_rate * Quantity;
                            decimal Total_sail_rate2 = discounted_rate * Quantity;
                            decimal Service_tax = (Tax_Rate * Quantity * discounted_rate) / 100;

                            new_sub_total += Total_sail_rate2;
                            new_total_Service += Service_tax;
                            new_Gross_amount += Total_sail_rate1;

                            using (SqlCommand cmd = new SqlCommand(@"INSERT INTO tbl_Quotaion_details 
                            (Sl_no, Quotation_no, Product_id, Product_Code, Product_name, Quantity, sail_rate, Service_tax_rate, discount_rate, new_sailrate, 
                             Total_sail_rate, Total_sail_rate1, Total_sail_rate2, specification, Misc, InvStatus, Type, Unit, ProductOrServiceCat, 
                             ItemRemarks, ItemNo, MaterialNo, PackSize, DeliveryDate, Department, AddedById, AddedOn, Version, IsDeleted, IsLatest) 
                            VALUES 
                            (@Sl_no, @Quotation_no, @Product_id, @Product_Code, @Product_name, @Quantity, @sail_rate, @Service_tax_rate, @discount_rate, @new_sailrate, 
                             @Total_sail_rate, @Total_sail_rate1, @Total_sail_rate2, @specification, @Misc, 'No', @Type, @Unit, @ProductOrServiceCat, 
                             @ItemRemarks, @ItemNo, @MaterialNo, @PackSize, @DeliveryDate, @Department, @AddedById, GETDATE(), 1, 0, 1)", conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@Sl_no", h);
                                cmd.Parameters.AddWithValue("@Quotation_no", newRecordID);
                                cmd.Parameters.AddWithValue("@Product_id", ((Label)row.FindControl("Product_code"))?.Text ?? ""); // HSN Code mapped to Product_id
                                cmd.Parameters.AddWithValue("@Product_Code", ((Label)row.FindControl("ProductID"))?.Text ?? ""); // PRD Custom ID mapped to Product_Code
                                cmd.Parameters.AddWithValue("@Product_name", ((Label)row.FindControl("ProductName"))?.Text ?? "");
                                cmd.Parameters.AddWithValue("@Quantity", Quantity);
                                cmd.Parameters.AddWithValue("@sail_rate", Sail_Rate);
                                cmd.Parameters.AddWithValue("@Service_tax_rate", Tax_Rate);
                                cmd.Parameters.AddWithValue("@discount_rate", Discount_Rate);
                                cmd.Parameters.AddWithValue("@new_sailrate", discounted_rate);
                                cmd.Parameters.AddWithValue("@Total_sail_rate", Total_sail_rate);
                                cmd.Parameters.AddWithValue("@Total_sail_rate1", Total_sail_rate1);
                                cmd.Parameters.AddWithValue("@Total_sail_rate2", Total_sail_rate2);
                                cmd.Parameters.AddWithValue("@specification", ((TextBox)row.FindControl("Brand"))?.Text ?? "");
                                cmd.Parameters.AddWithValue("@Misc", ((TextBox)row.FindControl("Specification"))?.Text ?? "~");
                                cmd.Parameters.AddWithValue("@Type", ((Label)row.FindControl("Type"))?.Text ?? "");
                                cmd.Parameters.AddWithValue("@Unit", ((Label)row.FindControl("Unit"))?.Text ?? "");
                                cmd.Parameters.AddWithValue("@ProductOrServiceCat", ((Label)row.FindControl("ProductOrServiceCat"))?.Text ?? "");
                                cmd.Parameters.AddWithValue("@ItemRemarks", ((TextBox)row.FindControl("ItemRemarks"))?.Text ?? "");
                                cmd.Parameters.AddWithValue("@ItemNo", ((TextBox)row.FindControl("ItemNo"))?.Text ?? "");
                                cmd.Parameters.AddWithValue("@MaterialNo", ((TextBox)row.FindControl("MaterialNo"))?.Text ?? "");
                                cmd.Parameters.AddWithValue("@PackSize", ((TextBox)row.FindControl("PackSize"))?.Text ?? "");
                                cmd.Parameters.AddWithValue("@DeliveryDate", rbQt.Checked ? "" : ((TextBox)row.FindControl("DeliveryDate"))?.Text ?? "");
                                cmd.Parameters.AddWithValue("@Department", rbQt.Checked ? "" : ((TextBox)row.FindControl("Department"))?.Text ?? "");
                                cmd.Parameters.AddWithValue("@AddedById", userId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    decimal tcsAmount = ParseDecimal(txt_tcs_amnt.Text);
                    decimal tcsPercent = ParseDecimal(txt_tcs_percent.Text);
                    decimal deliveryAmount = ParseDecimal(txt_delivery_amnt.Text);
                    decimal freightPercent = ParseDecimal(txt_freight_percent.Text);
                    decimal otherAmount = ParseDecimal(txt_othr_amnt.Text);
                    decimal finalNet = Math.Round(new_Gross_amount + tcsAmount + deliveryAmount + otherAmount, 2);

                    using (SqlCommand cmd = new SqlCommand(@"INSERT INTO tbl_Quotation 
                    (Quotation_no, Quotation_date, Client_Id, Gross, Service_tax, Net_amount, Status1, Status2, Sl_no, status3, service_tax1, sub_total, cgstOrsgst, igst, PlaceofSupply, PaymentStatus, ReferenceData, ReferenceName, ReferenceId, ReferenceDate, ValidityDays, DeliveryTenure, PackingCharges, Remarks, DetailedView, RecordType, DO_Number, PO_Number, PO_Date, Validity_StartDate, Validity_EndDate, AddedById, DiscountView, TCS_Amount, TCS_Percent, Freight_Amount, Freight_VAT_Percent, OtherCharge_Name, OtherCharge_Amount, IsLatest, Version)
                    VALUES (@QNo, @QDate, @CId, @Gross, @STax, @Net, 'No', 'No', @Sl, 'No', @STax1, @Sub, @CGST, @IGST, @POS, 'No', @RefD, @RefN, @RefI, @RefDt, @VDays, @DTenure, @PCharge, @Rem, @DView, @RType, @DO, @PO, @PODate, @VStart, @VEnd, @UserId, @DiscView, @TCSA, @TCSP, @FrA, @FrP, @OthN, @OthA, 1, 1)", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@QNo", newRecordID);
                        cmd.Parameters.AddWithValue("@QDate", GetSafeDate(txtquotationDate.Text));
                        cmd.Parameters.AddWithValue("@CId", cmbClient.SelectedValue);
                        cmd.Parameters.AddWithValue("@Gross", Math.Round(new_Gross_amount, 2));
                        cmd.Parameters.AddWithValue("@STax", Math.Round(new_total_Service, 2));
                        cmd.Parameters.AddWithValue("@Net", finalNet);
                        cmd.Parameters.AddWithValue("@Sl", slNo);
                        cmd.Parameters.AddWithValue("@STax1", Math.Round(new_total_Service, 2));
                        cmd.Parameters.AddWithValue("@Sub", new_sub_total);
                        cmd.Parameters.AddWithValue("@CGST", RadioButtonGst.SelectedValue == "1" ? "YES" : "");
                        cmd.Parameters.AddWithValue("@IGST", RadioButtonGst.SelectedValue == "0" ? "YES" : "");
                        cmd.Parameters.AddWithValue("@POS", ddlPlaceOfSupply.SelectedItem.Text);
                        cmd.Parameters.AddWithValue("@RefD", rbYes.Checked ? "Yes" : "No");
                        cmd.Parameters.AddWithValue("@RefN", rbYes.Checked ? txt_clientrefname.Text : "N/A");
                        cmd.Parameters.AddWithValue("@RefI", rbYes.Checked ? txt_clientrefid.Text : "N/A");
                        cmd.Parameters.AddWithValue("@RefDt", rbYes.Checked ? GetSafeDate(txt_clientrefdate.Text) : (object)"1900-01-01");
                        cmd.Parameters.AddWithValue("@VDays", txt_valdays.Text);
                        cmd.Parameters.AddWithValue("@DTenure", DDL_DeliveryTerms.SelectedValue == "4" ? txt_deltrms.Text : DDL_DeliveryTerms.SelectedItem.Text);
                        cmd.Parameters.AddWithValue("@PCharge", DDL_pkgfrwd.SelectedValue == "3" ? txt_pkgfrwd.Text : DDL_pkgfrwd.SelectedItem.Text);
                        cmd.Parameters.AddWithValue("@Rem", txt_remarks.Text);
                        cmd.Parameters.AddWithValue("@DView", DDL_ItemViewType.SelectedItem.Text);
                        cmd.Parameters.AddWithValue("@RType", rbPo.Checked ? "Purchase Order" : "Quotation");
                        cmd.Parameters.AddWithValue("@DO", rbPo.Checked ? txb_donumber.Text : "N/A");
                        cmd.Parameters.AddWithValue("@PO", rbPo.Checked ? txb_ponumber.Text : "N/A");
                        cmd.Parameters.AddWithValue("@PODate", rbPo.Checked ? GetSafeDate(txb_podate.Text) : (object)"1900-01-01");
                        cmd.Parameters.AddWithValue("@VStart", rbPo.Checked ? GetSafeDate(txb_strtdt.Text) : (object)"1900-01-01");
                        cmd.Parameters.AddWithValue("@VEnd", rbPo.Checked ? GetSafeDate(txb_enddt.Text) : (object)"1900-01-01");
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@DiscView", DDL_DiscountView.SelectedItem.Text);
                        cmd.Parameters.AddWithValue("@TCSA", tcsAmount);
                        cmd.Parameters.AddWithValue("@TCSP", tcsPercent);
                        cmd.Parameters.AddWithValue("@FrA", deliveryAmount);
                        cmd.Parameters.AddWithValue("@FrP", freightPercent);
                        cmd.Parameters.AddWithValue("@OthN", TextBox1.Text.Trim());
                        cmd.Parameters.AddWithValue("@OthA", otherAmount);
                        cmd.ExecuteNonQuery();
                    }

                    insertPaymentPhaseNew(newRecordID, conn, trans);
                    insertprimaryServiceNew(newRecordID, conn, trans);

                    trans.Commit();
                    lbl_recordno.Text = newRecordID;
                    ShowAlert("New Record Created Successfully: " + newRecordID, false);
                    btnSabe.Visible = btnNew.Visible = false;
                }
                catch (Exception ex)
                {
                    try { trans?.Rollback(); } catch { }
                    StringBuilder errorMsg = new StringBuilder();
                    errorMsg.AppendLine("An error occurred: " + ex.Message);
                    if (ex.InnerException != null) errorMsg.AppendLine("<br/>Inner: " + ex.InnerException.ToString());
                    ShowAlert(errorMsg.ToString(), true);
                }
            }
        }

        // ================= SUPPORT METHODS =================

        private decimal ParseDecimal(string text)
        {
            decimal val;
            if (decimal.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val)) return val;
            return 0m;
        }

        private object GetSafeDate(string dateText)
        {
            DateTime dt;
            if (DateTime.TryParseExact(dateText, "dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt)) return dt;
            if (DateTime.TryParse(dateText, out dt)) return dt;
            return "1900-01-01";
        }

        private string FormatDate(string input)
        {
            DateTime dt;
            if (DateTime.TryParseExact(input, "dd-MMM-yyyy", null, System.Globalization.DateTimeStyles.None, out dt)) return dt.ToString("dd-MMM-yyyy");
            if (DateTime.TryParse(input, out dt)) return dt.ToString("dd-MMM-yyyy");
            return "";
        }

        private void Bindquotationno()
        {
            string currentNo = lbl_recordno.Text.Trim();
            string newQuotationNo = "";
            char lastChar = currentNo[currentNo.Length - 1];

            if (!char.IsDigit(lastChar))
            {
                char nextLetter = (char)(lastChar + 1);
                newQuotationNo = currentNo.Substring(0, currentNo.Length - 1) + nextLetter;
            }
            else
            {
                newQuotationNo = currentNo + "A";
            }

            while (QuotationNoExists(newQuotationNo))
            {
                lastChar = newQuotationNo[newQuotationNo.Length - 1];
                newQuotationNo = newQuotationNo.Substring(0, newQuotationNo.Length - 1) + (char)(lastChar + 1);
            }

            lblqno.Text = newQuotationNo;
        }

        private bool QuotationNoExists(string quotationNo)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM tbl_Quotation WHERE Quotation_no = @QuotationNo", con))
            {
                cmd.Parameters.AddWithValue("@QuotationNo", quotationNo); con.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        private int idreturn()
        {
            int b = 0; DbCL.Sqlconnection(); DbCL.ConnectDb();
            string d = txtquotationDate.Text, m = d.Substring(3, 3), y = d.Substring(7, 4), d4, d5, d6;
            if (m == "Jan" || m == "Feb" || m == "Mar") { d4 = (Convert.ToInt32(y) - 1).ToString(); d5 = "31-Mar-" + d4; d6 = "31-Mar-" + y; }
            else { d4 = (Convert.ToInt32(y) + 1).ToString(); d5 = "31-Mar-" + y; d6 = "31-Mar-" + d4; }
            using (SqlCommand cmd = new SqlCommand("select Sl_no from tbl_Quotation where ID=(select max(ID) from tbl_Quotation where cast(Quotation_date as datetime) between '" + d5 + "' and '" + d6 + "')", DbCL.Conn))
            using (SqlDataReader re = cmd.ExecuteReader()) { if (re.Read()) b = Convert.ToInt32(re["Sl_no"]); }
            DbCL.Conn.Close(); return b;
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
                        bool s = false; foreach (DataRow r in dtPhasefees.Rows) { if (r["PaymentPhase"].ToString() == pt) s = true; }
                        if (!s) { DataRow dr = dtPhasefees.NewRow(); dr[0] = pt; dr[1] = ""; dr[2] = (pt == "Full & Final Instalment" || pt == "Payment After Delivery" || pt == "100% Against PI") ? "100" : ""; dtPhasefees.Rows.Add(dr); }
                    }
                    else
                    {
                        dtPhasefees.Columns.Add("PaymentPhase"); dtPhasefees.Columns.Add("PhaseDesc"); dtPhasefees.Columns.Add("AmountPer");
                        DataRow dr = dtPhasefees.NewRow(); dr[0] = pt; dr[1] = ""; dr[2] = (pt == "Full & Final Instalment" || pt == "Payment After Delivery" || pt == "100% Against PI") ? "100" : ""; dtPhasefees.Rows.Add(dr);
                    }
                }
            }
            GridView3.DataSource = dtPhasefees; GridView3.DataBind(); ViewState["phaseAmountData"] = dtPhasefees;
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
                else { ((TextBox)r.Cells[0].FindControl("AmountPer")).Text = (100 - t).ToString(); }
            }
        }

        protected void GridView3_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            DataTable d = (DataTable)ViewState["phaseAmountData"]; d.Rows[e.RowIndex].Delete();
            ViewState["phaseAmountData"] = d.Rows.Count > 0 ? d : null;
            GridView3.DataSource = (DataTable)ViewState["phaseAmountData"]; GridView3.DataBind();
        }

        private void insertPaymentPhaseNew(string qutno, SqlConnection conn, SqlTransaction trans)
        {
            // Clear existing phases to prevent duplicates on update
            using (SqlCommand delCmd = new SqlCommand("DELETE FROM tbl_QutPaymentPhase WHERE qut_no = @qno", conn, trans))
            {
                delCmd.Parameters.AddWithValue("@qno", qutno);
                delCmd.ExecuteNonQuery();
            }

            foreach (GridViewRow r in GridView3.Rows)
            {
                using (SqlCommand cmd = new SqlCommand("INSERT INTO tbl_QutPaymentPhase(qut_no, phase_type, PhaseDesc, amountper) VALUES (@qut_no, @pt, @pd, @ap)", conn, trans))
                {
                    cmd.Parameters.AddWithValue("@qut_no", qutno);
                    cmd.Parameters.AddWithValue("@pt", ((Label)r.Cells[1].FindControl("PaymentPhase")).Text);
                    cmd.Parameters.AddWithValue("@pd", ((TextBox)r.Cells[2].FindControl("PhaseDesc")).Text);
                    cmd.Parameters.AddWithValue("@ap", GridView3.Rows.Count == 1 ? "100" : ((TextBox)r.Cells[0].FindControl("AmountPer")).Text);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void insertprimaryServiceNew(string qutno, SqlConnection conn, SqlTransaction trans)
        {
            // Clear existing to prevent duplicates on update
            using (SqlCommand delCmd = new SqlCommand("DELETE FROM tbl_QutPrimaryService WHERE qut_no = @qno", conn, trans)) { delCmd.Parameters.AddWithValue("@qno", qutno); delCmd.ExecuteNonQuery(); }
            using (SqlCommand delCmd = new SqlCommand("DELETE FROM tbl_QuoPserTerm WHERE qutno = @qno", conn, trans)) { delCmd.Parameters.AddWithValue("@qno", qutno); delCmd.ExecuteNonQuery(); }
            using (SqlCommand delCmd = new SqlCommand("DELETE FROM tbl_QuoPriSerTogather WHERE qutno = @qno", conn, trans)) { delCmd.Parameters.AddWithValue("@qno", qutno); delCmd.ExecuteNonQuery(); }

            string ps = ""; int i = 0;
            foreach (GridViewRow r in gridps.Rows)
            {
                string pc = ((Label)r.Cells[0].FindControl("ProductCatagory")).Text;
                using (SqlCommand cmd = new SqlCommand("INSERT INTO tbl_QutPrimaryService(qut_no, PrimaryService) VALUES (@q, @ps)", conn, trans))
                { cmd.Parameters.AddWithValue("@q", qutno); cmd.Parameters.AddWithValue("@ps", pc); cmd.ExecuteNonQuery(); }

                using (SqlCommand cmd = new SqlCommand("SELECT PrimaryServiceTerms FROM tbl_PrimaryServiceTerms WHERE PrimaryService=@p", conn, trans))
                {
                    cmd.Parameters.AddWithValue("@p", pc);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable(); da.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            using (SqlCommand c = new SqlCommand("INSERT INTO tbl_QuoPserTerm (qutno, PServiceName, PSerTer) VALUES (@q, @pn, @pt)", conn, trans))
                            { c.Parameters.AddWithValue("@q", qutno); c.Parameters.AddWithValue("@pn", pc); c.Parameters.AddWithValue("@pt", dr[0]); c.ExecuteNonQuery(); }
                        }
                    }
                }
                pc = "“" + pc + "”";
                if (i == 0) ps = pc; else if (i == 1) ps += " and " + pc; else ps += " , " + pc;
                i++;
            }
            if (!string.IsNullOrEmpty(ps))
            {
                using (SqlCommand cmd = new SqlCommand("INSERT INTO tbl_QuoPriSerTogather (qutno, PServiceName) VALUES (@q, @ps)", conn, trans))
                { cmd.Parameters.AddWithValue("@q", qutno); cmd.Parameters.AddWithValue("@ps", ps); cmd.ExecuteNonQuery(); }
            }
        }

        protected void RadioButtonGst_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToggleGridColumns();
        }

        private void updatedueamountdetails(double netamount)
        {
            string a = findtotalamount();
            double amount = netamount - Convert.ToDouble(a);
            DbCL.executeRdr("update tbl_invoice_due set Due_amount='" + amount.ToString() + "' where qutation_no='" + lblqno.Text + "'");
        }

        private string findtotalamount()
        {
            string amount = "0";
            DbCL.Sqlconnection(); DbCL.ConnectDb();
            using (SqlCommand cmd = new SqlCommand("select sum(cast(Given_amount as real)) as amount from tbl_invoice_payment where Quotation_No='" + lblqno.Text + "'", DbCL.Conn))
            using (SqlDataReader re = cmd.ExecuteReader())
            {
                if (re.Read() && re["amount"].ToString() != "") amount = re["amount"].ToString();
            }
            DbCL.Conn.Close(); return amount;
        }

        protected void ToggleGridColumns()
        {
            if (gd_Service_Product.Columns.Count > 20)
            {
                bool isQuotation = rbQt.Checked;
                // Updated Column Indexes based on the new human-readable Grid Layout
                gd_Service_Product.Columns[22].Visible = !isQuotation; // Del Date
                gd_Service_Product.Columns[23].Visible = !isQuotation; // Dept 
            }
        }

        // ================= DATA PRE-LOADING METHODS FOR EDIT =================

        private void BindPaymentPhases(string qutno)
        {
            string query = "SELECT phase_type AS PaymentPhase, PhaseDesc, amountper AS AmountPer FROM tbl_QutPaymentPhase WHERE qut_no = @qutno";
            SqlParameter[] param = { new SqlParameter("@qutno", qutno) };

            DataTable dt = DbCL.SPreturn_dt(query, param);

            if (dt != null && dt.Rows.Count > 0)
            {
                GridView3.DataSource = dt;
                GridView3.DataBind();
                ViewState["phaseAmountData"] = dt; // Preserve for the grid
            }
            else
            {
                GridView3.DataSource = null;
                GridView3.DataBind();
            }
        }

        private void LoadPrimaryServices(string qutNo)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("ProductCatagory", typeof(string)));

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                string query = "SELECT PrimaryService FROM tbl_QutPrimaryService WHERE qut_no = @qut_no";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@qut_no", qutNo);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DataRow dr = dt.NewRow();
                            dr["ProductCatagory"] = reader["PrimaryService"].ToString();
                            dt.Rows.Add(dr);
                        }
                    }
                }
            }

            gridps.DataSource = dt;
            gridps.DataBind();
            ViewState["pService"] = dt;
        }

        private void bindphaseType(string qutno)
        {
            string str = "SELECT id, PaymentPhase FROM tbl_PaymentPhase ORDER BY id";
            DataTable dtphasetype = DbCL.SPreturn_dt(str, null);

            listPhaseType.Items.Clear();

            string selectedQuery = "SELECT phase_type FROM tbl_QutPaymentPhase WHERE qut_no = @qutno";
            SqlParameter[] param = { new SqlParameter("@qutno", qutno) };
            DataTable selectedPhases = DbCL.SPreturn_dt(selectedQuery, param);

            // Extract selected phases into a list safely (Without LINQ to avoid framework issues)
            List<string> selectedValues = new List<string>();
            if (selectedPhases != null)
            {
                foreach (DataRow row in selectedPhases.Rows)
                {
                    selectedValues.Add(row["phase_type"].ToString());
                }
            }

            foreach (DataRow row in dtphasetype.Rows)
            {
                string value = row["PaymentPhase"].ToString();
                ListItem item = new ListItem(value);

                if (selectedValues.Contains(value))
                {
                    item.Selected = true;
                }

                listPhaseType.Items.Add(item);
            }
        }
    }
}