using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Text;

namespace Bill_Software.corporate.business.app
{
    public partial class EditPurchase : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ToString();

        public DataTable first_datatable;
        public static DataTable Dt = new DataTable("Table");
        public static double tota_purchesrate1 = 0;
        public static double total_tax_rate_details = 0;
        DataTable dtproductWithCat = new DataTable();
        private List<string> vatRates;
        private List<string> serviceTaxRates;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                DbCL.FillCombo(cmbvendor, "select Vendor_Name from tbl_Vendor order by Vendor_Name");
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            }
        }

        private void BuindCompanyId(string VendorName)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Vendor_Id from tbl_Vendor where Vendor_Name='" + VendorName + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblclientId.Text = re["Vendor_Id"].ToString();
                lbl_vendorname.Text = VendorName;
            }
            DbCL.Conn.Close();
        }

        private void BuindCompanyName(string VendorId)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Vendor_Name from tbl_Vendor where  Vendor_Id='" + VendorId + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lbl_vendorname.Text = re["Vendor_Name"].ToString();
                lblclientId.Text = VendorId;
            }
            DbCL.Conn.Close();
        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            BuindCompanyId(cmbvendor.Text.ToString());
            string cmdstring = "";
            if (RadioButtonList2.SelectedIndex == 0)
            {        
                cmdstring = "select tbl_Purches.Purches_Id, tbl_Purches.TimeStamp, tbl_Purches.Purches_date,tbl_Purches.Total_purches_rate,tbl_Purches.Total_Tax_rate, tbl_Purches.Invoice_No, tbl_Purches.Purches_date, tbl_Purches.BuyerOrderNo, tbl_Purches.OrderDate, tbl_Purches.ShippedToStoreName, tbl_Vendor.Vendor_Name from tbl_Purches inner join tbl_Vendor on tbl_Purches.Client_Id=tbl_Vendor.Vendor_Id where tbl_Purches.Client_Id='" + lblclientId.Text + "' order by CAST(tbl_Purches.Purches_date as date) DESC";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList2.SelectedIndex == 1)
            {
                cmdstring = "select tbl_Purches.Purches_Id, tbl_Purches.TimeStamp, tbl_Purches.Purches_date,tbl_Purches.Total_purches_rate,tbl_Purches.Total_Tax_rate, tbl_Purches.Invoice_No, tbl_Purches.Purches_date, tbl_Purches.BuyerOrderNo, tbl_Purches.OrderDate, tbl_Purches.ShippedToStoreName, tbl_Vendor.Vendor_Name from tbl_Purches inner join tbl_Vendor on tbl_Purches.Client_Id=tbl_Vendor.Vendor_Id where cast(tbl_Purches.Purches_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by CAST(tbl_Purches.Purches_date as date) DESC";
                Buinddatagrid(cmdstring);
            }
            else
            {
                cmdstring = "select tbl_Purches.Purches_Id, tbl_Purches.TimeStamp, tbl_Purches.Purches_date,tbl_Purches.Total_purches_rate,tbl_Purches.Total_Tax_rate, tbl_Purches.Invoice_No, tbl_Purches.Purches_date, tbl_Purches.BuyerOrderNo, tbl_Purches.OrderDate, tbl_Purches.ShippedToStoreName, tbl_Vendor.Vendor_Name from tbl_Purches inner join tbl_Vendor on tbl_Purches.Client_Id=tbl_Vendor.Vendor_Id where tbl_Purches.Client_Id='" + lblclientId.Text + "' and cast(tbl_Purches.Purches_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by CAST(tbl_Purches.Purches_date as date) DESC";
                Buinddatagrid(cmdstring);
            }
            btnSertch.Visible = false;
        }

        private void Buinddatagrid(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                Buinddatagrid1(cmdstring);
            }
            else
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "No Data Found...";
            }
            DbCL.Conn.Close();
        }

        private void Buinddatagrid1(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            SqlCommand cmd1 = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd1.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();

        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/EditPurchase.aspx");
        }

        protected void BindShippedToDropdown()
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ToString()))
            {
                string query = "SELECT StoreId, StoreName, StoreAddress FROM Stores WHERE IsActive = 1";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    row["StoreName"] = $"{row["StoreName"]} [{row["StoreAddress"]}]";
                }

                DDL_ShippedTo.DataSource = dt;
                DDL_ShippedTo.DataTextField = "StoreName";
                DDL_ShippedTo.DataValueField = "StoreId";
                DDL_ShippedTo.DataBind();

                DDL_ShippedTo.Items.Insert(0, new ListItem("-- Select Store --", ""));
            }
        }

        private void LoadTaxRates()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            // Fetch VAT Rates
            string vatQuery = "Select Vat_Rate from tbl_Vat_Master";
            SqlCommand vatCmd = new SqlCommand(vatQuery, DbCL.Conn);
            SqlDataReader vatRdr = vatCmd.ExecuteReader();

            vatRates = new List<string> { "NA" }; // Initialize with "NA"
            while (vatRdr.Read())
            {
                vatRates.Add(vatRdr[0].ToString());
            }
            vatRdr.Close();

            // Fetch Service Tax Rates
            string serviceTaxQuery = "Select Service_tax from tbl_Service_master";
            SqlCommand serviceTaxCmd = new SqlCommand(serviceTaxQuery, DbCL.Conn);
            SqlDataReader serviceTaxRdr = serviceTaxCmd.ExecuteReader();

            serviceTaxRates = new List<string> { "NA" }; // Initialize with "NA"
            while (serviceTaxRdr.Read())
            {
                serviceTaxRates.Add(serviceTaxRdr[0].ToString());
            }
            serviceTaxRdr.Close();

            DbCL.Conn.Close();
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Purches_Id = Convert.ToString(e.CommandArgument);
            if (e.CommandName == "Select")
            {
                BindShippedToDropdown();
                LoadTaxRates();

                SelectorGridRow.Visible = false;
                ModifierGridRow.Visible = true;
                Panel_Edit.Visible = true;

                
                lbl_purchaseid.Text = Purches_Id.ToString();
                LoadPurchase(Purches_Id);
                //LoadPurchaseDetails(Purches_Id);
                LoadPurchaseData(Purches_Id);
            }
        }

        private void LoadPurchase(string purchesId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmdHeader = new SqlCommand("SELECT * FROM tbl_Purches WHERE Purches_Id=@Purches_Id", conn);
                cmdHeader.Parameters.AddWithValue("@Purches_Id", purchesId);

                using (SqlDataReader dr = cmdHeader.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        // Dates
                        txtPurchesDate.Text = dr["Purches_date"] != DBNull.Value
                            ? Convert.ToDateTime(dr["Purches_date"]).ToString("dd-MMM-yyyy")
                            : "";

                        txt_stockadddate.Text = dr["Stock_Add_Date"] != DBNull.Value
                            ? Convert.ToDateTime(dr["Stock_Add_Date"]).ToString("dd-MMM-yyyy")
                            : "";

                        txt_refordrdate.Text = dr["OrderDate"] != DBNull.Value
                            ? Convert.ToDateTime(dr["OrderDate"]).ToString("dd-MMM-yyyy")
                            : "";

                        // Strings
                        lblvendor_id.Text = dr["Client_Id"]?.ToString() ?? "";
                        BuindCompanyName(lblvendor_id.Text);
                        txt_invno.Text = dr["Invoice_No"]?.ToString() ?? "";
                        txt_reforder.Text = dr["BuyerOrderNo"]?.ToString() ?? "";
                        txt_narration.Text = dr["Narration"]?.ToString() ?? "";
                        txt_inv_amount.Text = dr["InvoiceAmnt"]?.ToString() ?? "";

                        // Dropdowns / Radio
                        RadioButtonList1.SelectedValue = dr["Purches_Type"]?.ToString() ?? "Product";

                        if (DDL_ShippedTo.Items.FindByValue(dr["ShippedToStoreId"]?.ToString()) != null)
                            DDL_ShippedTo.SelectedValue = dr["ShippedToStoreId"].ToString();

                        if (DDL_vat_parsentage.Items.FindByValue(dr["Delivery_Rate"]?.ToString()) != null)
                            DDL_vat_parsentage.SelectedValue = dr["Delivery_Rate"].ToString();

                        // Decimal / Numeric
                        txt_tcs_amnt.Text = dr["TCS_Amount"]?.ToString() ?? "0";
                        txt_tcs_percent.Text = dr["TCS_Rate"]?.ToString() ?? "0";
                        txt_delivery_amnt.Text = dr["Delivery_Amount"]?.ToString() ?? "0";
                        txt_othr_amnt1.Text = dr["otherAmount1"]?.ToString() ?? "0";
                        txt_othr_amnt2.Text = dr["otherAmount2"]?.ToString() ?? "0";

                        // Other amount names
                        TextBox1.Text = dr["otherAmount1_name"]?.ToString() ?? "";
                        TextBox2.Text = dr["otherAmount2_name"]?.ToString() ?? "";
                    }
                }
            }
        }

        private void UpdatePurchase(string purchesId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string query = @"
                    UPDATE tbl_Purches
                    SET 
                        Purches_date       = @Purches_date,
                        Stock_Add_Date     = @Stock_Add_Date,
                        OrderDate          = @OrderDate,
                        Client_Id          = @Client_Id,
                        Invoice_No         = @Invoice_No,
                        BuyerOrderNo       = @BuyerOrderNo,
                        Narration          = @Narration,
                        InvoiceAmnt        = @InvoiceAmnt,
                        Purches_Type       = @Purches_Type,
                        ShippedToStoreId   = @ShippedToStoreId,
                        Delivery_Rate      = @Delivery_Rate,
                        TCS_Amount         = @TCS_Amount,
                        TCS_Rate           = @TCS_Rate,
                        Delivery_Amount    = @Delivery_Amount,
                        otherAmount1       = @otherAmount1,
                        otherAmount2       = @otherAmount2,
                        otherAmount1_name  = @otherAmount1_name,
                        otherAmount2_name  = @otherAmount2_name,
                        LastModifiedBy     = @LastModifiedBy,
                        LastModifiedDate   = GETDATE()
                    WHERE Purches_Id = @Purches_Id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Purches_Id", purchesId);

                        // Dates
                        cmd.Parameters.AddWithValue("@Purches_date", string.IsNullOrWhiteSpace(txtPurchesDate.Text) ? (object)DBNull.Value : DateTime.Parse(txtPurchesDate.Text));
                        cmd.Parameters.AddWithValue("@Stock_Add_Date", string.IsNullOrWhiteSpace(txt_stockadddate.Text) ? (object)DBNull.Value : DateTime.Parse(txt_stockadddate.Text));
                        cmd.Parameters.AddWithValue("@OrderDate", string.IsNullOrWhiteSpace(txt_refordrdate.Text) ? (object)DBNull.Value : DateTime.Parse(txt_refordrdate.Text));

                        // Strings
                        cmd.Parameters.AddWithValue("@Client_Id", lblvendor_id.Text);
                        cmd.Parameters.AddWithValue("@Invoice_No", txt_invno.Text);
                        cmd.Parameters.AddWithValue("@BuyerOrderNo", txt_reforder.Text);
                        cmd.Parameters.AddWithValue("@Narration", txt_narration.Text);

                        // Decimal / Numeric
                        cmd.Parameters.AddWithValue("@InvoiceAmnt", string.IsNullOrWhiteSpace(txt_inv_amount.Text) ? 0 : decimal.Parse(txt_inv_amount.Text));
                        cmd.Parameters.AddWithValue("@Purches_Type", RadioButtonList1.SelectedValue);
                        cmd.Parameters.AddWithValue("@ShippedToStoreId", string.IsNullOrEmpty(DDL_ShippedTo.SelectedValue) ? (object)DBNull.Value : DDL_ShippedTo.SelectedValue);
                        cmd.Parameters.AddWithValue("@Delivery_Rate", string.IsNullOrEmpty(DDL_vat_parsentage.SelectedValue) ? (object)DBNull.Value : DDL_vat_parsentage.SelectedValue);
                        cmd.Parameters.AddWithValue("@TCS_Amount", string.IsNullOrWhiteSpace(txt_tcs_amnt.Text) ? 0 : decimal.Parse(txt_tcs_amnt.Text));
                        cmd.Parameters.AddWithValue("@TCS_Rate", string.IsNullOrWhiteSpace(txt_tcs_percent.Text) ? 0 : decimal.Parse(txt_tcs_percent.Text));
                        cmd.Parameters.AddWithValue("@Delivery_Amount", string.IsNullOrWhiteSpace(txt_delivery_amnt.Text) ? 0 : decimal.Parse(txt_delivery_amnt.Text));
                        cmd.Parameters.AddWithValue("@otherAmount1", string.IsNullOrWhiteSpace(txt_othr_amnt1.Text) ? 0 : decimal.Parse(txt_othr_amnt1.Text));
                        cmd.Parameters.AddWithValue("@otherAmount2", string.IsNullOrWhiteSpace(txt_othr_amnt2.Text) ? 0 : decimal.Parse(txt_othr_amnt2.Text));

                        // Other amount names
                        cmd.Parameters.AddWithValue("@otherAmount1_name", TextBox1.Text);
                        cmd.Parameters.AddWithValue("@otherAmount2_name", TextBox2.Text);

                        // Audit info
                        cmd.Parameters.AddWithValue("@LastModifiedBy", Session["UserID"]?.ToString() ?? "System");

                        cmd.ExecuteNonQuery();

                        msg_updt_purches.Text = "Successfully Modified";
                    }
                }
            }
            catch (Exception ex)
            {
                msg_updt_purches.Text = ex.Message;
            }
        }

        private void LoadPurchaseData(string purchesId)
        {
            var dt = new DataTable();
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ToString();

            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand(@"
                SELECT
                    Id,
                    Purches_id,
                    sl_no                      AS OrderNo,
                    Product_id                 AS Ser_pro_code,
                    Product_name               AS Ser_pro_Name,
                    specification              AS Specification,
                    Quantity,
                    vendor_rate,
                    DiscountPercent,
                    DiscountAmount,
                    TaxableAmount,
                    tax_applicable             AS TaxApplicable,
                    tax_rate                   AS VatPercent,
                    purches_rate,
                    vat_amount,
                    total_purches_rate
                FROM tbl_purches_details
                WHERE Purches_id = @Purches_id
                ORDER BY sl_no ASC;", conn))
            {
                cmd.Parameters.AddWithValue("@Purches_id", purchesId);
                new SqlDataAdapter(cmd).Fill(dt);
            }

            if (dt.Rows.Count == 0)
            {
                Panel_DBDataItems.Visible = false;
                DB_DataGrid.Visible = false;
                gd_Service_Product.DataSource = null;
                gd_Service_Product.DataBind();
                return;
            }

            Panel_DBDataItems.Visible = true;
            DB_DataGrid.Visible = true;

            gd_Service_Product.DataSource = dt;
            gd_Service_Product.DataBind();

            ViewState["dt"] = dt; // keep if you need it later
        }

        protected void gd_Service_Product_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DropDownList dp1 = (DropDownList)e.Row.Cells[4].FindControl("vat_parsentage");
                dp1.Items.Clear();

                if (RadioButtonList1.SelectedIndex == 0)
                {
                    dp1.Items.AddRange(vatRates.Select(rate => new ListItem(rate)).ToArray());
                }
                else
                {
                    dp1.Items.AddRange(serviceTaxRates.Select(rate => new ListItem(rate)).ToArray());
                }
            }
        }

        protected void gd_Service_Product_RowEditing(object sender, GridViewEditEventArgs e)
        {
            //gd_Service_Product.EditIndex = e.NewEditIndex;
            //LoadPurchaseData(CurrentPurchesId()); // rebind in edit mode

            DataTable dt = ViewState["dt"] as DataTable;
            gd_Service_Product.EditIndex = e.NewEditIndex;
            gd_Service_Product.DataSource = dt;
            gd_Service_Product.DataBind();

            
        }

        protected void gd_Service_Product_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            //gd_Service_Product.EditIndex = -1;
            //LoadPurchaseData(CurrentPurchesId());

            DataTable dt = ViewState["dt"] as DataTable;
            gd_Service_Product.EditIndex = -1;
            gd_Service_Product.DataSource = dt;
            gd_Service_Product.DataBind();

            
        }

        protected void gd_Service_Product_RowUpdating_old(object sender, GridViewUpdateEventArgs e)
        {
            var row = gd_Service_Product.Rows[e.RowIndex];

            // keys
            int id = Convert.ToInt32(gd_Service_Product.DataKeys[e.RowIndex].Values["Id"]);
            string purchesId = Convert.ToString(gd_Service_Product.DataKeys[e.RowIndex].Values["Purches_id"]);

            // controls
            var txtSpecification = (TextBox)row.FindControl("txtSpecification");
            var txtQuantity = (TextBox)row.FindControl("txtQuantity");
            var txtVendorRate = (TextBox)row.FindControl("txtVendorRate");
            var txtDiscountPercent = (TextBox)row.FindControl("txtDiscountPercent");
            var txtDiscountAmount = (TextBox)row.FindControl("txtDiscountAmount");
            var rblTaxApplicable = (RadioButtonList)row.FindControl("rblTaxApplicable");
            var ddlVatPercentage = (DropDownList)row.FindControl("ddlVatPercentage");
            var txtOrder = (TextBox)row.FindControl("txtOrder");

            // parsing
            double qty = ParseD(txtQuantity?.Text);
            double rate = ParseD(txtVendorRate?.Text);
            double dPct = ParseD(txtDiscountPercent?.Text);
            double dAmtIn = ParseD(txtDiscountAmount?.Text);
            string taxAp = rblTaxApplicable?.SelectedValue ?? "No";
            double vatPct = ParseD(ddlVatPercentage?.SelectedValue);
            int orderNo = (int)Math.Max(0, Math.Round(ParseD(txtOrder?.Text)));

            // calculations
            double gross = qty * rate;
            double discount = dPct > 0 ? gross * dPct / 100.0 : dAmtIn;
            if (discount < 0) discount = 0;
            if (discount > gross) discount = gross;

            double taxable = gross - discount;
            double vat = string.Equals(taxAp, "Yes", StringComparison.OrdinalIgnoreCase) ? taxable * vatPct / 100.0 : 0.0;
            double total = taxable + vat;

            // update DB
            string cs = ConfigurationManager.ConnectionStrings["DbConn"].ToString();
            using (var conn = new SqlConnection(cs))
            using (var cmd = new SqlCommand(@"
                UPDATE tbl_purches_details
                   SET specification       = @specification,
                       Quantity            = @Quantity,
                       vendor_rate         = @vendor_rate,
                       DiscountPercent     = @DiscountPercent,
                       DiscountAmount      = @DiscountAmount,
                       TaxableAmount       = @TaxableAmount,
                       tax_applicable      = @tax_applicable,
                       tax_rate            = @tax_rate,
                       purches_rate        = @purches_rate,
                       vat_amount          = @vat_amount,
                       total_purches_rate  = @total_purches_rate,
                       sl_no               = @sl_no
                 WHERE Id = @Id AND Purches_id = @Purches_id;", conn))
            {
                cmd.Parameters.AddWithValue("@specification", (object)(txtSpecification?.Text ?? ""));
                cmd.Parameters.AddWithValue("@Quantity", qty);
                cmd.Parameters.AddWithValue("@vendor_rate", rate);
                cmd.Parameters.AddWithValue("@DiscountPercent", dPct);
                cmd.Parameters.AddWithValue("@DiscountAmount", discount);
                cmd.Parameters.AddWithValue("@TaxableAmount", taxable);
                cmd.Parameters.AddWithValue("@tax_applicable", taxAp);
                cmd.Parameters.AddWithValue("@tax_rate", vatPct);
                cmd.Parameters.AddWithValue("@purches_rate", gross);
                cmd.Parameters.AddWithValue("@vat_amount", vat);
                cmd.Parameters.AddWithValue("@total_purches_rate", total);
                cmd.Parameters.AddWithValue("@sl_no", orderNo);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Purches_id", purchesId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            // refresh totals in header
            RecalcAndUpdateHeader(purchesId);

            gd_Service_Product.EditIndex = -1;
            LoadPurchaseData(purchesId);
        }

        protected void gd_Service_Product_RowUpdating_old2(object sender, GridViewUpdateEventArgs e)
        {
            var row = gd_Service_Product.Rows[e.RowIndex];

            // keys
            int id = Convert.ToInt32(gd_Service_Product.DataKeys[e.RowIndex].Values["Id"]);
            string purchesId = Convert.ToString(gd_Service_Product.DataKeys[e.RowIndex].Values["Purches_id"]);

            // edited controls
            var txtSpecification = (TextBox)row.FindControl("txtSpecification");
            var txtQuantity = (TextBox)row.FindControl("txtQuantity");
            var txtVendorRate = (TextBox)row.FindControl("txtVendorRate");
            var txtDiscountPercent = (TextBox)row.FindControl("txtDiscountPercent");
            var txtDiscountAmount = (TextBox)row.FindControl("txtDiscountAmount");
            var rblTaxApplicable = (RadioButtonList)row.FindControl("rblTaxApplicable");
            var ddlVatPercentage = (DropDownList)row.FindControl("ddlVatPercentage");
            var txtOrder = (TextBox)row.FindControl("txtOrder");

            // parse + compute (new values)
            double qty = ParseD(txtQuantity?.Text);
            double rate = ParseD(txtVendorRate?.Text);
            double dPct = ParseD(txtDiscountPercent?.Text);
            double dAmtIn = ParseD(txtDiscountAmount?.Text);
            string taxAp = rblTaxApplicable?.SelectedValue ?? "No";
            double vatPct = ParseD(ddlVatPercentage?.SelectedValue);
            int orderNo = (int)Math.Max(0, Math.Round(ParseD(txtOrder?.Text)));

            double purchesRate = qty * rate;
            double discount = dPct > 0 ? purchesRate * dPct / 100.0 : dAmtIn;
            if (discount < 0) discount = 0;
            if (discount > purchesRate) discount = purchesRate;

            double taxable = purchesRate - discount;
            double vat = string.Equals(taxAp, "Yes", StringComparison.OrdinalIgnoreCase) ? (taxable * vatPct / 100.0) : 0.0;
            double total = taxable + vat;

            string cs = ConfigurationManager.ConnectionStrings["DbConn"].ToString();

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    // --- 1) Read the old line first (so we know the old qty and the exact product/store) ---
                    string productId = null, productName = null, storeId = null;
                    double oldQty = 0, oldRate = 0, oldVatPct = 0;
                    DateTime? oldShippedDate = null;

                    using (var cmdGet = new SqlCommand(@"
                        SELECT Product_id, Product_name, Quantity, vendor_rate, tax_rate, ShippedToLoc, ShippedDate
                        FROM tbl_purches_details
                        WHERE Id = @Id AND Purches_id = @pid;", conn, tx))
                    {
                        cmdGet.Parameters.AddWithValue("@Id", id);
                        cmdGet.Parameters.AddWithValue("@pid", purchesId);

                        using (var rdr = cmdGet.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                productId = rdr["Product_id"]?.ToString();
                                productName = rdr["Product_name"]?.ToString();
                                oldQty = ParseD(rdr["Quantity"]?.ToString());
                                oldRate = ParseD(rdr["vendor_rate"]?.ToString());
                                oldVatPct = ParseD(rdr["tax_rate"]?.ToString());
                                storeId = rdr["ShippedToLoc"]?.ToString();
                                DateTime sd;
                                if (rdr["ShippedDate"] != DBNull.Value && DateTime.TryParse(rdr["ShippedDate"].ToString(), out sd))
                                {
                                    oldShippedDate = sd;
                                }

                            }
                        }
                    }

                    if (string.IsNullOrWhiteSpace(productId) || string.IsNullOrWhiteSpace(storeId))
                    {
                        tx.Rollback();
                        throw new InvalidOperationException("Cannot resolve Product/Store for the row being updated.");
                    }

                    // --- 2) Update details row ---
                    using (var cmd = new SqlCommand(@"
                    UPDATE tbl_purches_details
                       SET specification       = @specification,
                           Quantity            = @Quantity,
                           vendor_rate         = @vendor_rate,
                           DiscountPercent     = @DiscountPercent,
                           DiscountAmount      = @DiscountAmount,
                           TaxableAmount       = @TaxableAmount,
                           tax_applicable      = @tax_applicable,
                           tax_rate            = @tax_rate,
                           purches_rate        = @purches_rate,
                           vat_amount          = @vat_amount,
                           total_purches_rate  = @total_purches_rate,
                           sl_no               = @sl_no
                     WHERE Id = @Id AND Purches_id = @Purches_id;", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@specification", (object)(txtSpecification?.Text ?? ""));
                        cmd.Parameters.AddWithValue("@Quantity", qty);
                        cmd.Parameters.AddWithValue("@vendor_rate", rate);
                        cmd.Parameters.AddWithValue("@DiscountPercent", dPct);
                        cmd.Parameters.AddWithValue("@DiscountAmount", discount);
                        cmd.Parameters.AddWithValue("@TaxableAmount", taxable);
                        cmd.Parameters.AddWithValue("@tax_applicable", taxAp);
                        cmd.Parameters.AddWithValue("@tax_rate", vatPct);
                        cmd.Parameters.AddWithValue("@purches_rate", purchesRate);
                        cmd.Parameters.AddWithValue("@vat_amount", vat);
                        cmd.Parameters.AddWithValue("@total_purches_rate", total);
                        cmd.Parameters.AddWithValue("@sl_no", orderNo);
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@Purches_id", purchesId);
                        cmd.ExecuteNonQuery();
                    }

                    // --- 3) Adjust stock by the delta (newQty - oldQty) for Product_id + Store ---
                    double qtyDiff = qty - oldQty;
                    if (Math.Abs(qtyDiff) > 0.0001)
                    {
                        AdjustStockDelta(
                            conn, tx,
                            productId: productId,
                            productName: productName,
                            storeId: storeId,                                // use the same store used when the line was inserted
                            storeName: DDL_ShippedTo.SelectedItem?.Text ?? "", // best available name
                            shippedDate: oldShippedDate ?? SafeDate(txt_stockadddate.Text),
                            delta: qtyDiff,
                            rate: rate,
                            vatPct: vatPct,
                            userId: Convert.ToString(Session["USERID"] ?? "System")
                        );
                    }

                    tx.Commit();
                }
            }

            // --- 4) Refresh header totals + rebind grid ---
            RecalcAndUpdateHeader(purchesId);
            gd_Service_Product.EditIndex = -1;
            LoadPurchaseData(purchesId);
        }

        protected void gd_Service_Product_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            var dt = ViewState["dt"] as DataTable;
            if (dt == null || e.RowIndex < 0 || e.RowIndex >= gd_Service_Product.Rows.Count)
                return;

            GridViewRow row = gd_Service_Product.Rows[e.RowIndex];

            // DataKeys
            int id = 0;
            int.TryParse(Convert.ToString(gd_Service_Product.DataKeys[e.RowIndex]["Id"]), out id);
            string purchesId = Convert.ToString(gd_Service_Product.DataKeys[e.RowIndex]["Purches_id"]);

            // Edit controls (ensure IDs match your EditItemTemplate)
            var txtSpecification = (TextBox)row.FindControl("txtSpecification");
            var txtQuantity = (TextBox)row.FindControl("txtQuantity");
            var txtVendorRate = (TextBox)row.FindControl("txtVendorRate");
            var txtDiscountPercent = (TextBox)row.FindControl("txtDiscountPercent");
            var txtDiscountAmount = (TextBox)row.FindControl("txtDiscountAmount");
            var rblTaxApplicable = (RadioButtonList)row.FindControl("rblTaxApplicable");
            var ddlVatPercentage = (DropDownList)row.FindControl("ddlVatPercentage");
            var txtOrder = (TextBox)row.FindControl("txtOrder");

            // Parse
            decimal qty = ParseM(txtQuantity?.Text);
            decimal rate = ParseM(txtVendorRate?.Text);
            decimal dPct = ParseM(txtDiscountPercent?.Text);
            decimal dAmtIn = ParseM(txtDiscountAmount?.Text);
            string taxAp = rblTaxApplicable?.SelectedValue ?? "No";
            decimal vatPct = ParseM(ddlVatPercentage?.SelectedValue);
            int orderNo = ParseInt(txtOrder?.Text);

            // Compute
            decimal gross = qty * rate;
            decimal discount = dPct > 0 ? Math.Round(gross * dPct / 100m, 2) : dAmtIn;
            if (discount < 0) discount = 0;
            if (discount > gross) discount = gross;

            decimal taxable = gross - discount;
            decimal vat = string.Equals(taxAp, "Yes", StringComparison.OrdinalIgnoreCase)
                            ? Math.Round(taxable * vatPct / 100m, 2)
                            : 0m;
            decimal total = taxable + vat;

            // Update the in-memory row (index aligns with binding since we bound dt directly)
            DataRow dr = dt.Rows[e.RowIndex];
            dr["Specification"] = (txtSpecification?.Text ?? "").Trim();
            dr["Quantity"] = qty;
            dr["vendor_rate"] = rate;
            dr["DiscountPercent"] = dPct;
            dr["DiscountAmount"] = discount;
            dr["TaxableAmount"] = taxable;
            dr["TaxApplicable"] = taxAp;
            dr["VatPercent"] = vatPct;
            dr["OrderNo"] = orderNo;
            // If your dt also has computed columns and you want to keep them:
            if (!dt.Columns.Contains("purches_rate")) dt.Columns.Add("purches_rate", typeof(decimal));
            if (!dt.Columns.Contains("vat_amount")) dt.Columns.Add("vat_amount", typeof(decimal));
            if (!dt.Columns.Contains("total_purches_rate")) dt.Columns.Add("total_purches_rate", typeof(decimal));
            dr["purches_rate"] = gross;
            dr["vat_amount"] = vat;
            dr["total_purches_rate"] = total;

            // Persist back to ViewState first
            ViewState["dt"] = dt;

            // Optional: update DB immediately only for existing rows (Id > 0)
            if (id > 0)
            {
                string cs = ConfigurationManager.ConnectionStrings["DbConn"].ToString();
                using (var conn = new SqlConnection(cs))
                using (var cmd = new SqlCommand(@"
                UPDATE tbl_purches_details
                   SET specification       = @specification,
                       Quantity            = @Quantity,
                       vendor_rate         = @vendor_rate,
                       DiscountPercent     = @DiscountPercent,
                       DiscountAmount      = @DiscountAmount,
                       TaxableAmount       = @TaxableAmount,
                       tax_applicable      = @tax_applicable,
                       tax_rate            = @tax_rate,
                       purches_rate        = @purches_rate,
                       vat_amount          = @vat_amount,
                       total_purches_rate  = @total_purches_rate,
                       sl_no               = @sl_no
                 WHERE Id = @Id AND Purches_id = @Purches_id;", conn))
                {
                    cmd.Parameters.AddWithValue("@specification", dr["Specification"]);
                    cmd.Parameters.AddWithValue("@Quantity", dr["Quantity"]);
                    cmd.Parameters.AddWithValue("@vendor_rate", dr["vendor_rate"]);
                    cmd.Parameters.AddWithValue("@DiscountPercent", dr["DiscountPercent"]);
                    cmd.Parameters.AddWithValue("@DiscountAmount", dr["DiscountAmount"]);
                    cmd.Parameters.AddWithValue("@TaxableAmount", dr["TaxableAmount"]);
                    cmd.Parameters.AddWithValue("@tax_applicable", dr["TaxApplicable"]);
                    cmd.Parameters.AddWithValue("@tax_rate", dr["VatPercent"]);
                    cmd.Parameters.AddWithValue("@purches_rate", dr["purches_rate"]);
                    cmd.Parameters.AddWithValue("@vat_amount", dr["vat_amount"]);
                    cmd.Parameters.AddWithValue("@total_purches_rate", dr["total_purches_rate"]);
                    cmd.Parameters.AddWithValue("@sl_no", dr["OrderNo"]);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Purches_id", purchesId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            // For Id == 0 (new rows), don’t insert yet—user will hit “Save All” later.

            // Rebind from ViewState
            gd_Service_Product.EditIndex = -1;
            gd_Service_Product.DataSource = dt;
            gd_Service_Product.DataBind();
        }

        private decimal ParseM(object value)
        {
            if (value == null)
                return 0m;

            decimal result;
            if (decimal.TryParse(value.ToString().Trim(), out result))
                return result;

            return 0m;
        }

        private int ParseInt(string value)
        {
            int result;
            if (int.TryParse(value, out result))
                return result;
            return 0; // default if parse fails
        }

        private decimal ParseDecimal(string value)
        {
            decimal result;
            if (decimal.TryParse(value, out result))
                return result;
            return 0m; // default if parse fails
        }


        private static DateTime SafeDate(string s)
        {
            DateTime d;
            if (!string.IsNullOrWhiteSpace(s) && DateTime.TryParse(s, out d))
            {
                return d;
            }
            return DateTime.Now;
        }


        private static void AdjustStockDelta(
            SqlConnection conn, SqlTransaction tx,
            string productId, string productName,
            string storeId, string storeName, DateTime shippedDate,
            double delta, double rate, double vatPct, string userId)
        {
            // Update existing row if present; else insert only when delta > 0
            using (var cmd = new SqlCommand(@"
                IF EXISTS (SELECT 1 FROM tbl_stock WHERE Product_id=@Product_id AND ShippedToStoreId=@StoreId AND ISNUMERIC(Quantity)=1)
                BEGIN
                    UPDATE tbl_stock
                       SET Quantity          = CAST(CAST(Quantity AS DECIMAL(18,3)) + @Delta AS VARCHAR(50)),
                           Sail_Rate         = @Rate,
                           Service_tax_rate  = @VatRate,
                           ModifiedOn        = GETDATE(),
                           ModifiedByUserId  = @UserId,
                           ShippedToStoreName= @StoreName,
                           ShippedDate       = @ShippedDate
                     WHERE Product_id=@Product_id AND ShippedToStoreId=@StoreId AND ISNUMERIC(Quantity)=1;
                END
                ELSE
                BEGIN
                    IF (@Delta > 0)
                    INSERT INTO tbl_stock
                        (Product_id, Product_name, Quantity, Sail_Rate, Service_tax_rate, ShippedToStoreId, ShippedToStoreName, ShippedDate, ModifiedByUserId, ModifiedOn)
                    VALUES
                        (@Product_id, @Product_name, CAST(@Delta AS VARCHAR(50)), @Rate, @VatRate, @StoreId, @StoreName, @ShippedDate, @UserId, GETDATE());
                END
                ", conn, tx))
            {
                cmd.Parameters.AddWithValue("@Product_id", productId ?? "");
                cmd.Parameters.AddWithValue("@Product_name", productName ?? "");
                cmd.Parameters.AddWithValue("@StoreId", storeId ?? "");
                cmd.Parameters.AddWithValue("@StoreName", storeName ?? "");
                cmd.Parameters.AddWithValue("@ShippedDate", shippedDate);
                cmd.Parameters.AddWithValue("@Delta", delta);
                // tbl_stock keeps rates as VARCHAR(50); sending as strings avoids implicit conversions
                cmd.Parameters.AddWithValue("@Rate", rate.ToString("0.####"));
                cmd.Parameters.AddWithValue("@VatRate", vatPct.ToString("0.##"));
                cmd.Parameters.AddWithValue("@UserId", userId ?? "System");
                cmd.ExecuteNonQuery();
            }
        }


        protected void gd_Service_Product_RowDeleting_old(object sender, GridViewDeleteEventArgs e)
        {
            int id = Convert.ToInt32(gd_Service_Product.DataKeys[e.RowIndex].Values["Id"]);
            string purchesId = Convert.ToString(gd_Service_Product.DataKeys[e.RowIndex].Values["Purches_id"]);

            string cs = ConfigurationManager.ConnectionStrings["DbConn"].ToString();
            using (var conn = new SqlConnection(cs))
            using (var cmd = new SqlCommand(@"DELETE FROM tbl_purches_details WHERE Id = @Id AND Purches_id = @Purches_id;", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Purches_id", purchesId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            RecalcAndUpdateHeader(purchesId);
            LoadPurchaseData(purchesId);
        }

        protected void gd_Service_Product_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                DataTable dt = ViewState["dt"] as DataTable;
                if (dt != null && dt.Rows.Count > 0)
                {
                    dt.Rows.RemoveAt(e.RowIndex);
                    ViewState["dt"] = dt;
                    gd_Service_Product.DataSource = dt;
                    gd_Service_Product.DataBind();
                }
            }
            catch (Exception ex)
            {
                msg_products.Text = "Error while deleting row: " + ex.Message;
                msg_products.ForeColor = System.Drawing.Color.Red;
            }
        }


        private static double ParseD(string s)
        {
            double d; return double.TryParse(s, out d) ? d : 0.0;
        }

        private string CurrentPurchesId()
        {
            return lbl_purchaseid.Text.Trim();
        }

        private void RecalcAndUpdateHeader(string purchesId)
        {
            if (string.IsNullOrWhiteSpace(purchesId)) return;

            string cs = ConfigurationManager.ConnectionStrings["DbConn"].ToString();
            double grandTotal = 0.0, grandTax = 0.0;

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();

                using (var cmdSum = new SqlCommand(@"SELECT ISNULL(SUM(total_purches_rate),0) AS GT, ISNULL(SUM(vat_amount),0) AS GTax FROM tbl_purches_details WHERE Purches_id=@pid;", conn))
                {
                    cmdSum.Parameters.AddWithValue("@pid", purchesId);
                    using (var rdr = cmdSum.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            grandTotal = Convert.ToDouble(rdr["GT"]);
                            grandTax = Convert.ToDouble(rdr["GTax"]);
                        }
                    }
                }

                using (var cmdUpd = new SqlCommand(@"UPDATE tbl_Purches SET Total_purches_rate = @GT, Total_Tax_rate = @GTax WHERE Purches_Id = @pid;", conn))
                {
                    cmdUpd.Parameters.AddWithValue("@GT", grandTotal);
                    cmdUpd.Parameters.AddWithValue("@GTax", grandTax);
                    cmdUpd.Parameters.AddWithValue("@pid", purchesId);
                    cmdUpd.ExecuteNonQuery();
                }
            }
        }


        protected void btn_UpdatePurchase_Click(object sender, EventArgs e)
        {
            UpdatePurchase(lbl_purchaseid.Text.ToString());
        }

        protected void btn_AddProducts_Click(object sender, EventArgs e)
        {
            DB_DataGrid.Visible = false;

            BindListitem();
            AddProdcuts.Visible = true;
            Panel_Selector.Visible = true;
        }

        private void BindListitem()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                cmdstring = "select ProductOrServiceCat from tbl_NewparentProduct order by Id";
            }
            else
            {
                cmdstring = "select Service_name  from tbl_Service order by Service_name";
            }
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

        protected void btn_viewProds_Click(object sender, EventArgs e)
        {
            LoadTaxRates();
            ProductSelector_row.Visible = true;
            ProductSelector_btnrow.Visible = true;

            if (RadioButtonList1.SelectedIndex == 0)
            {
                string cmdstring = "select Id, Product_code, ProductID, ProductOrServiceCat, Brand, ProductName, Specification, Type, Sail_Rate, Tax_Rate, Unit from tbl_NewProduct where ProductOrServiceCat=@ProductOrServiceCat order by Id, ProductName";
                SqlParameter[] pram = {
                new SqlParameter("@ProductOrServiceCat",cmbproduct_service.Text) };

                dtproductWithCat = DbCL.SPreturn_dt(cmdstring, pram);
                if (dtproductWithCat.Rows.Count > 0)
                {
                    gridProdWithCat.DataSource = dtproductWithCat;
                    gridProdWithCat.DataBind();
                    ViewState["dtprocat"] = dtproductWithCat;
                }

                if (DDL_vat_parsentage.Items.Count == 0)
                {
                    DDL_vat_parsentage.Items.AddRange(vatRates.Select(rate => new ListItem(rate)).ToArray());
                }
            }
            //----------Below is for binding Services----------------//
            //else
            //{
            //    string cmdstring = "select Service_code,Service_name from tbl_Service where Service_name='" + cmbproduct_service.Text + "'";
            //    Binddata1(cmdstring);
            //}

            cmbproduct_service.SelectedIndex = 0;

            gd_Service_Product.DataSource = Dt;
            gd_Service_Product.DataBind();
            ViewState["products"] = Dt;
        }

        protected void btn_selector_Click(object sender, EventArgs e)
        {
            Try_One();
            //Try_Two();

            DB_DataGrid.Visible = true;
            Panel_DBDataItems.Visible = true;
            AddProdcuts.Visible = false;
            Panel_Selector.Visible = false;
        }

        private void Try_One()
        {
            // 1) Grab existing editable table (the one bound to gd_Service_Product)
            DataTable dtExisting = ViewState["dt"] as DataTable;
            if (dtExisting == null)
            {
                dtExisting = new DataTable();
                dtExisting.Columns.Add("Id", typeof(int));
                dtExisting.Columns.Add("Purches_id", typeof(string));
                dtExisting.Columns.Add("Ser_pro_code", typeof(string));   // ProductID
                dtExisting.Columns.Add("Ser_pro_Name", typeof(string));   // ProductName
                dtExisting.Columns.Add("Specification", typeof(string));
                dtExisting.Columns.Add("Quantity", typeof(decimal));
                dtExisting.Columns.Add("vendor_rate", typeof(decimal));
                dtExisting.Columns.Add("DiscountPercent", typeof(decimal));
                dtExisting.Columns.Add("DiscountAmount", typeof(decimal));
                dtExisting.Columns.Add("TaxableAmount", typeof(decimal));
                dtExisting.Columns.Add("TaxApplicable", typeof(string));  // Yes/No
                dtExisting.Columns.Add("VatPercent", typeof(decimal));    // Tax_Rate
                dtExisting.Columns.Add("OrderNo", typeof(int));
            }

            // 2) derive current purchase id if needed
            //string purchesId = findpurchesId(); // your existing method
            string purchesId = lbl_purchaseid.Text.ToString();

            // 3) helper
            Func<string, decimal> ParseM = s =>
            {
                decimal d;
                return decimal.TryParse((s ?? "").Trim(), out d) ? d : 0m;
            };


            // 4) find next order number
            int nextOrder = 1;
            if (dtExisting.Rows.Count > 0)
            {
                int maxOrder = 0;
                foreach (DataRow r in dtExisting.Rows)
                {
                    if (r.Table.Columns.Contains("OrderNo"))
                    {
                        int o;
                        if (int.TryParse(Convert.ToString(r["OrderNo"]), out o))
                            if (o > maxOrder) maxOrder = o;
                    }
                }
                nextOrder = maxOrder + 1;
            }

            // 5) iterate catalog grid and append only checked rows
            for (int i = 0; i < gridProdWithCat.Rows.Count; i++)
            {
                var row = gridProdWithCat.Rows[i];
                var chk = row.FindControl("chkdtp") as CheckBox;   // <-- correct checkbox ID
                if (chk == null || !chk.Checked) continue;

                // read only the needed columns FROM THE GRID ROW (labels)
                string productId = ((Label)row.FindControl("ProductID"))?.Text?.Trim() ?? "";
                string productName = ((Label)row.FindControl("ProductName"))?.Text?.Trim() ?? "";
                string spec = ((Label)row.FindControl("Specification"))?.Text?.Trim() ?? "";
                string sailRateTxt = ((Label)row.FindControl("Sail_Rate"))?.Text?.Trim() ?? "0";
                string taxRateTxt = ((Label)row.FindControl("Tax_Rate"))?.Text?.Trim() ?? "0";

                // dedupe based on ProductID -> Ser_pro_code
                bool exists = dtExisting.AsEnumerable()
                    .Any(r => string.Equals(Convert.ToString(r["Ser_pro_code"]), productId, StringComparison.OrdinalIgnoreCase));
                if (exists) continue;

                decimal rate = ParseM(sailRateTxt);
                decimal vat = ParseM(taxRateTxt);
                decimal qty = 1m;                   // default quantity for new rows
                decimal discPct = 0m, discAmt = 0m;  // defaults

                decimal purchesRate = qty * rate;
                decimal taxable = purchesRate - discAmt;      // since discPct=0, discAmt=0
                string taxApplicable = vat > 0 ? "Yes" : "No";

                // build new row for editable grid
                DataRow nr = dtExisting.NewRow();
                nr["Id"] = 0;                 // unsaved/new
                nr["Purches_id"] = purchesId;
                nr["Ser_pro_code"] = productId;
                nr["Ser_pro_Name"] = productName;
                nr["Specification"] = spec;
                nr["Quantity"] = qty;
                nr["vendor_rate"] = rate;
                nr["DiscountPercent"] = discPct;
                nr["DiscountAmount"] = discAmt;
                nr["TaxableAmount"] = taxable;
                nr["TaxApplicable"] = taxApplicable;
                nr["VatPercent"] = vat;
                nr["OrderNo"] = nextOrder++;

                dtExisting.Rows.Add(nr);
            }

            // 6) persist + bind
            ViewState["dt"] = dtExisting;
            gd_Service_Product.EditIndex = -1;
            gd_Service_Product.DataSource = dtExisting;
            gd_Service_Product.DataBind();
        }

        private void Try_Two()
        {
            DataTable dt = StageTable;

            foreach (GridViewRow row in gridProdWithCat.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkdtp");
                if (chk != null && chk.Checked)
                {
                    int productId = Convert.ToInt32(gridProdWithCat.DataKeys[row.RowIndex].Value);
                    string productName = row.Cells[2].Text;

                    // Avoid duplicates
                    if (!dt.AsEnumerable().Any(r => r.Field<int>("ProductId") == productId))
                    {
                        DataRow dr = dt.NewRow();
                        dr["ProductId"] = productId;
                        dr["ProductName"] = productName;
                        dr["Specification"] = "";
                        dr["Quantity"] = 0;
                        dr["vendor_rate"] = 0;
                        dr["DiscountPercent"] = 0;
                        dr["DiscountAmount"] = 0;
                        dr["TaxableAmount"] = 0;
                        dr["TaxApplicable"] = "No";
                        dr["VatPercent"] = 0;
                        dr["OrderNo"] = 1;

                        dt.Rows.Add(dr);
                    }
                }
            }

            StageTable = dt;
            gd_Service_Product.DataSource = dt;
            gd_Service_Product.DataBind();
        }

        private DataTable StageTable
        {
            get { return ViewState["StageTable"] as DataTable; }
            set { ViewState["StageTable"] = value; }
        }

        private void InitStageTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ProductId", typeof(int));
            dt.Columns.Add("ProductName", typeof(string));
            dt.Columns.Add("Specification", typeof(string));
            dt.Columns.Add("Quantity", typeof(decimal));
            dt.Columns.Add("vendor_rate", typeof(decimal));
            dt.Columns.Add("DiscountPercent", typeof(decimal));
            dt.Columns.Add("DiscountAmount", typeof(decimal));
            dt.Columns.Add("TaxableAmount", typeof(decimal));
            dt.Columns.Add("TaxApplicable", typeof(string));
            dt.Columns.Add("VatPercent", typeof(decimal));
            dt.Columns.Add("OrderNo", typeof(int));

            StageTable = dt;
        }

        protected void btn_submit_Click(object sender, EventArgs e)
        {
            DataTable dt = ViewState["dt"] as DataTable;
            if (dt == null || dt.Rows.Count == 0)
                return;

            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ToString();
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            int id = Convert.ToInt32(row["Id"]);
                            if (id == 0) // new rows only
                            {
                                int rowsAffected = 0;

                                // --- Insert into tbl_purches_details ---
                                // Parse numbers safely
                                decimal qty = Convert.ToDecimal(row["Quantity"]);
                                decimal rate = Convert.ToDecimal(row["vendor_rate"]);
                                decimal discPct = Convert.ToDecimal(row["DiscountPercent"]);
                                decimal discAmt = Convert.ToDecimal(row["DiscountAmount"]);
                                decimal taxable = Convert.ToDecimal(row["TaxableAmount"]);
                                decimal vatPct = Convert.ToDecimal(row["VatPercent"]);

                                // Correct calculations
                                decimal purches_rate = rate;   // unit rate
                                decimal total_beforeDisc = qty * rate;
                                decimal total_afterDisc = taxable;   // or recalc as (total_beforeDisc - discAmt)
                                decimal vatAmt = (vatPct / 100m) * total_afterDisc;
                                decimal total_purches_rate = total_afterDisc + vatAmt;

                                using (var cmd = new SqlCommand(@"
INSERT INTO tbl_purches_details 
    (sl_no, Purches_id, Product_id, Product_name, vendor_rate, tax_applicable, tax_rate, Quantity, purches_rate, total_purches_rate, vat_amount, specification, DiscountPercent, DiscountAmount, TaxableAmount, ShippedToLoc, ShippedDate, Purches_date, Client_id) 
VALUES 
    (@sl_no, @Purches_id, @Product_id, @Product_name, @vendor_rate, @tax_applicable, @tax_rate, @Quantity, @purches_rate, @total_purches_rate, @vat_amount, @specification, @DiscountPercent, @DiscountAmount, @TaxableAmount, @ShippedToLoc, @ShippedDate, @Purches_date, @Client_id)", conn, tx))
                                {
                                    cmd.Parameters.AddWithValue("@sl_no", row["OrderNo"]);
                                    cmd.Parameters.AddWithValue("@Purches_id", row["Purches_id"]);
                                    cmd.Parameters.AddWithValue("@Product_id", row["Ser_pro_code"]);
                                    cmd.Parameters.AddWithValue("@Product_name", row["Ser_pro_Name"]);
                                    cmd.Parameters.AddWithValue("@vendor_rate", rate);
                                    cmd.Parameters.AddWithValue("@tax_applicable", row["TaxApplicable"]);
                                    cmd.Parameters.AddWithValue("@tax_rate", vatPct);
                                    cmd.Parameters.AddWithValue("@Quantity", qty);
                                    cmd.Parameters.AddWithValue("@purches_rate", purches_rate);
                                    cmd.Parameters.AddWithValue("@total_purches_rate", total_purches_rate);
                                    cmd.Parameters.AddWithValue("@vat_amount", vatAmt);
                                    cmd.Parameters.AddWithValue("@specification", row["Specification"]);
                                    cmd.Parameters.AddWithValue("@DiscountPercent", discPct);
                                    cmd.Parameters.AddWithValue("@DiscountAmount", discAmt);
                                    cmd.Parameters.AddWithValue("@TaxableAmount", total_afterDisc);
                                    cmd.Parameters.AddWithValue("@ShippedToLoc", DDL_ShippedTo.SelectedValue);
                                    cmd.Parameters.AddWithValue("@ShippedDate", SafeDate(txt_stockadddate.Text));
                                    cmd.Parameters.AddWithValue("@Purches_date", txtPurchesDate.Text);
                                    cmd.Parameters.AddWithValue("@Client_id", lblvendor_id.Text);

                                    rowsAffected = cmd.ExecuteNonQuery();
                                }

                                double qtyStock = Convert.ToDouble(row["Quantity"]);
                                double rateStock = Convert.ToDouble(row["vendor_rate"]);
                                double vatPctStock = Convert.ToDouble(row["VatPercent"]);
                                // --- Adjust Stock ONLY if insert succeeded ---
                                if (rowsAffected > 0)
                                {
                                    AdjustStockDelta(
                                        conn, tx,
                                        row["Ser_pro_code"].ToString(),
                                        row["Ser_pro_Name"].ToString(),
                                        DDL_ShippedTo.SelectedValue,
                                        DDL_ShippedTo.SelectedItem?.Text ?? "",
                                        SafeDate(txt_stockadddate.Text),
                                        delta: qtyStock, // reuse qty
                                        rate: rateStock, // reuse rate
                                        vatPct: vatPctStock, // reuse vatPct
                                        userId: Convert.ToString(Session["USERID"] ?? "System")
                                    );
                                }
                                else
                                {
                                    throw new Exception("Insert failed for product: " + row["Ser_pro_Name"]);
                                }

                            }
                        }

                        tx.Commit();
                        RecalcAndUpdateHeader(lbl_purchaseid.Text.ToString());
                        LoadPurchaseData(lbl_purchaseid.Text.ToString());
                        btn_AddProducts.Enabled = false;
                        btn_submit.Enabled = false;
                        btn_UpdatePurchase.Enabled = false;
                        msg_products.Text = "Purchase & stock updates committed successfully!";
                        msg_products.ForeColor = System.Drawing.Color.Green;
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        msg_products.Text = "Error: " + ex.Message;
                        msg_products.ForeColor = System.Drawing.Color.Red;
                    }
                }
            }
        }



        // --- helpers for parsing in .NET 4.5.2 ---
        private static int SafeInt(object o)
        {
            int n; return int.TryParse(Convert.ToString(o ?? "0"), out n) ? n : 0;
        }
        private static decimal SafeDec(object o)
        {
            decimal d; return decimal.TryParse(Convert.ToString(o ?? "0"), out d) ? d : 0m;
        }
        //private static DateTime SafeDate(string s)
        //{
        //    DateTime d; return DateTime.TryParse(s, out d) ? d : DateTime.Now;
        //}

    }
}