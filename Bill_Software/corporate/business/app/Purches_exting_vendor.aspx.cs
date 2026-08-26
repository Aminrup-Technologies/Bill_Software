using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm11 : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ToString();
        DB_UTILITY DbCL = new DB_UTILITY();

        // --- GRID STATE HOLDER ---
        private DataTable GridData
        {
            get
            {
                if (ViewState["GridData"] == null)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Ser_pro_code");
                    dt.Columns.Add("Ser_pro_Name");
                    dt.Columns.Add("sepecification");
                    dt.Columns.Add("Quantity");
                    dt.Columns.Add("Vendor_rate");
                    dt.Columns.Add("DiscountPercent");
                    dt.Columns.Add("DiscountAmount");
                    dt.Columns.Add("TaxableAmount");
                    dt.Columns.Add("TaxApplicable");
                    dt.Columns.Add("VatRate");
                    dt.Columns.Add("Order", typeof(int));
                    ViewState["GridData"] = dt;
                }
                return (DataTable)ViewState["GridData"];
            }
            set { ViewState["GridData"] = value; }
        }

        private List<string> vatRates;

        // --- PAGE LOAD ---
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
                return;
            }

            LoadTaxRates();

            if (!IsPostBack)
            {
                GridData = null;
                LoadInitialData();
            }
        }

        private void LoadInitialData()
        {
            try
            {
                txtPurchesDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txt_stockadddate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtpaymentdate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtcashDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");

                DbCL.FillCombo(cmbvendor, "select Vendor_Name from tbl_Vendor order by Vendor_Name");
                BindShippedToDropdown();
                LoadCategories();
                BindProductServiceDropdown();
            }
            catch (Exception ex)
            {
                ShowError("Init Error: " + ex.Message);
            }
        }

        // --- DROPDOWN LOADERS ---
        private void LoadCategories()
        {
            ddlCategory.Items.Clear();
            ddlCategory.Items.Add(new ListItem("-- All Categories --", "0"));

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT DISTINCT ProductOrServiceCat FROM tbl_NewparentProduct ORDER BY ProductOrServiceCat", conn);
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    if (rdr["ProductOrServiceCat"] != DBNull.Value)
                        ddlCategory.Items.Add(rdr["ProductOrServiceCat"].ToString());
                }
            }
        }

        private void LoadTaxRates()
        {
            vatRates = new List<string> { "0" };
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Vat_Rate FROM tbl_Vat_Master", conn))
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read()) vatRates.Add(rdr[0].ToString());
                }
            }
        }

        protected void BindShippedToDropdown()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT StoreId, StoreName, StoreAddress FROM Stores WHERE IsActive = 1";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    row["StoreName"] = string.Format("{0} [{1}]", row["StoreName"], row["StoreAddress"]);
                }

                DDL_ShippedTo.DataSource = dt;
                DDL_ShippedTo.DataTextField = "StoreName";
                DDL_ShippedTo.DataValueField = "StoreId";
                DDL_ShippedTo.DataBind();
                DDL_ShippedTo.Items.Insert(0, new ListItem("-- Select Store --", "0"));
            }
        }

        // --- VENDOR & PRODUCT SELECTION ---
        protected void cmbvendor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbvendor.SelectedIndex == 0) return;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM tbl_Vendor WHERE Vendor_Name = @Name", conn);
                cmd.Parameters.AddWithValue("@Name", cmbvendor.SelectedItem.Text);
                SqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.Read())
                {
                    lblvendor_id.Text = rdr["Vendor_Id"].ToString();
                    txtAddress1.Text = rdr["Address1"].ToString();

                    if (rdr["City"] != DBNull.Value) cmbcity.Text = rdr["City"].ToString();
                    if (rdr["pin"] != DBNull.Value) txtPin.Text = rdr["pin"].ToString();
                    if (rdr["State"] != DBNull.Value) cmbState.Text = rdr["State"].ToString();
                }
            }
        }

        protected void RadioButtonList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            divCategory.Visible = (RadioButtonList1.SelectedValue == "Product");
            BindProductServiceDropdown();
        }

        protected void ddlCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindProductServiceDropdown();
        }

        private void BindProductServiceDropdown()
        {
            string query = "";
            bool isProduct = (RadioButtonList1.SelectedValue == "Product");

            cmbproduct_service.Items.Clear();
            cmbproduct_service.Items.Add(new ListItem("-- Select Item --", "0"));

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                if (isProduct)
                {
                    string catFilter = "";
                    if (ddlCategory.SelectedIndex > 0)
                    {
                        catFilter = " WHERE ProductOrServiceCat = @Cat ";
                        cmd.Parameters.AddWithValue("@Cat", ddlCategory.SelectedItem.Text);
                    }
                    query = string.Format("SELECT ProductName FROM tbl_NewProduct {0} ORDER BY ProductName", catFilter);
                }
                else
                {
                    query = "SELECT Service_name FROM tbl_Service ORDER BY Service_name";
                }

                cmd.CommandText = query;
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    cmbproduct_service.Items.Add(rdr[0].ToString());
                }
            }
        }

        // --- ADD ITEM BUTTON (With Validation & Reset) ---
        protected void Button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbproduct_service.SelectedIndex <= 0)
                {
                    ShowError("Please select an item from the list.");
                    return;
                }

                UpdateStateFromGrid();

                string itemName = cmbproduct_service.SelectedItem.Text;
                string query = "";

                if (RadioButtonList1.SelectedValue == "Product")
                    query = "SELECT ProductID as Code, ProductName as Name FROM tbl_NewProduct WHERE ProductName = @Name";
                else
                    query = "SELECT Service_code as Code, Service_name as Name FROM tbl_Service WHERE Service_name = @Name";

                DataTable dtItem = new DataTable();
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Name", itemName);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dtItem);
                }

                if (dtItem.Rows.Count > 0)
                {
                    DataTable currentGrid = GridData;
                    DataRow newRow = currentGrid.NewRow();

                    newRow["Ser_pro_code"] = dtItem.Rows[0]["Code"];
                    newRow["Ser_pro_Name"] = dtItem.Rows[0]["Name"];
                    newRow["Order"] = currentGrid.Rows.Count + 1;

                    newRow["Quantity"] = "1";
                    newRow["Vendor_rate"] = "0";
                    newRow["DiscountPercent"] = "0";
                    newRow["DiscountAmount"] = "0.00";
                    newRow["TaxableAmount"] = "0.00";
                    newRow["TaxApplicable"] = "No";
                    newRow["VatRate"] = "0";
                    newRow["sepecification"] = "";

                    currentGrid.Rows.Add(newRow);

                    GridData = currentGrid;
                    BindGrid();

                    cmbproduct_service.SelectedIndex = 0;
                    string script = string.Format("document.getElementById('txtItemFilter').value=''; filterDropdown('txtItemFilter', '{0}');", cmbproduct_service.ClientID);
                    ScriptManager.RegisterStartupScript(this, GetType(), "resetSearch", script, true);

                    PanelError.Visible = false;
                }
                else
                {
                    ShowError("Item details not found in database.");
                }
            }
            catch (Exception ex)
            {
                ShowError("Add Item Error: " + ex.Message);
            }
        }

        // --- GRID STATE MANAGEMENT (Prevents Data Loss) ---
        private void UpdateStateFromGrid()
        {
            DataTable dt = GridData;
            for (int i = 0; i < gd_Service_Product.Rows.Count; i++)
            {
                if (i >= dt.Rows.Count) break;

                GridViewRow row = gd_Service_Product.Rows[i];

                TextBox txtSpec = (TextBox)row.FindControl("sepecification");
                TextBox txtQty = (TextBox)row.FindControl("Quantity");
                TextBox txtRate = (TextBox)row.FindControl("Vendor_rate");
                TextBox txtDiscP = (TextBox)row.FindControl("DiscountPercent");
                TextBox txtDiscA = (TextBox)row.FindControl("DiscountAmount");
                TextBox txtTaxable = (TextBox)row.FindControl("TaxableAmount");
                RadioButtonList rblTax = (RadioButtonList)row.FindControl("RadioButtonList1");
                DropDownList ddlVat = (DropDownList)row.FindControl("vat_parsentage");
                TextBox txtOrder = (TextBox)row.FindControl("txtOrder");

                if (txtSpec != null) dt.Rows[i]["sepecification"] = txtSpec.Text;
                if (txtQty != null) dt.Rows[i]["Quantity"] = string.IsNullOrEmpty(txtQty.Text) ? "0" : txtQty.Text;
                if (txtRate != null) dt.Rows[i]["Vendor_rate"] = string.IsNullOrEmpty(txtRate.Text) ? "0" : txtRate.Text;
                if (txtDiscP != null) dt.Rows[i]["DiscountPercent"] = string.IsNullOrEmpty(txtDiscP.Text) ? "0" : txtDiscP.Text;
                if (txtDiscA != null) dt.Rows[i]["DiscountAmount"] = string.IsNullOrEmpty(txtDiscA.Text) ? "0" : txtDiscA.Text;
                if (txtTaxable != null) dt.Rows[i]["TaxableAmount"] = string.IsNullOrEmpty(txtTaxable.Text) ? "0" : txtTaxable.Text;

                if (rblTax != null) dt.Rows[i]["TaxApplicable"] = rblTax.SelectedValue;
                if (ddlVat != null) dt.Rows[i]["VatRate"] = ddlVat.SelectedValue;

                if (txtOrder != null)
                    dt.Rows[i]["Order"] = string.IsNullOrEmpty(txtOrder.Text) ? (i + 1) : Convert.ToInt32(txtOrder.Text);
            }
            GridData = dt;
        }

        // --- GRID ROW ACTIONS (Move/Remove) ---
        protected void gd_Service_Product_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName == "MoveUp" || e.CommandName == "MoveDown" || e.CommandName == "RemoveItem")
                {
                    UpdateStateFromGrid();

                    DataTable dt = GridData;
                    int index = Convert.ToInt32(e.CommandArgument);

                    if (e.CommandName == "MoveUp" && index > 0)
                    {
                        DataRow temp = dt.NewRow();
                        temp.ItemArray = dt.Rows[index].ItemArray;
                        dt.Rows.RemoveAt(index);
                        dt.Rows.InsertAt(temp, index - 1);
                    }
                    else if (e.CommandName == "MoveDown" && index < dt.Rows.Count - 1)
                    {
                        DataRow temp = dt.NewRow();
                        temp.ItemArray = dt.Rows[index].ItemArray;
                        dt.Rows.RemoveAt(index);
                        dt.Rows.InsertAt(temp, index + 1);
                    }
                    else if (e.CommandName == "RemoveItem")
                    {
                        dt.Rows.RemoveAt(index);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++) dt.Rows[i]["Order"] = i + 1;

                    GridData = dt;
                    BindGrid();
                    ScriptManager.RegisterStartupScript(this, GetType(), "recalc", "calculateGrandTotal();", true);
                }
            }
            catch (Exception ex)
            {
                ShowError("Grid Action Error: " + ex.Message);
            }
        }

        private void BindGrid()
        {
            gd_Service_Product.DataSource = GridData;
            gd_Service_Product.DataBind();
        }

        protected void gd_Service_Product_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DropDownList ddlTax = (DropDownList)e.Row.FindControl("vat_parsentage");
                if (ddlTax != null)
                {
                    ddlTax.Items.Clear();
                    foreach (string rate in vatRates) ddlTax.Items.Add(new ListItem(rate, rate));
                }

                DataRowView drv = (DataRowView)e.Row.DataItem;
                if (drv != null)
                {
                    ((TextBox)e.Row.FindControl("sepecification")).Text = drv["sepecification"].ToString();
                    ((TextBox)e.Row.FindControl("Quantity")).Text = drv["Quantity"].ToString();
                    ((TextBox)e.Row.FindControl("Vendor_rate")).Text = drv["Vendor_rate"].ToString();
                    ((TextBox)e.Row.FindControl("DiscountPercent")).Text = drv["DiscountPercent"].ToString();
                    ((TextBox)e.Row.FindControl("DiscountAmount")).Text = drv["DiscountAmount"].ToString();
                    ((TextBox)e.Row.FindControl("TaxableAmount")).Text = drv["TaxableAmount"].ToString();
                    ((TextBox)e.Row.FindControl("txtOrder")).Text = drv["Order"].ToString();

                    RadioButtonList rbl = (RadioButtonList)e.Row.FindControl("RadioButtonList1");
                    if (rbl != null && drv["TaxApplicable"] != DBNull.Value && !string.IsNullOrEmpty(drv["TaxApplicable"].ToString()))
                    {
                        rbl.SelectedValue = drv["TaxApplicable"].ToString();
                    }

                    if (ddlTax != null && drv["VatRate"] != DBNull.Value && !string.IsNullOrEmpty(drv["VatRate"].ToString()))
                    {
                        if (ddlTax.Items.FindByValue(drv["VatRate"].ToString()) != null)
                            ddlTax.SelectedValue = drv["VatRate"].ToString();
                    }
                }
            }
        }

        // --- BUTTON: UPDATE & RECALCULATE ---
        protected void btnRecalculate_Click(object sender, EventArgs e)
        {
            UpdateStateFromGrid();
            BindGrid();
            ScriptManager.RegisterStartupScript(this, GetType(), "recalc", "calculateGrandTotal();", true);
        }

        protected void Button4_Click(object sender, EventArgs e)
        {
            string invNo = txt_invno.Text.Trim();
            if (string.IsNullOrEmpty(invNo)) { ShowError("Enter Invoice No"); return; }

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM tbl_Purches WHERE Invoice_No = @Inv", conn);
                    cmd.Parameters.AddWithValue("@Inv", invNo);
                    int count = (int)cmd.ExecuteScalar();

                    if (count > 0)
                    {
                        lblErrorMsg.Text = "Duplicate Invoice Number found!";
                        PanelError.Visible = true; PanelOK.Visible = false;
                    }
                    else
                    {
                        lblOk.Text = "Invoice Number is valid.";
                        PanelOK.Visible = true; PanelError.Visible = false;
                    }
                }
            }
            catch (Exception ex) { ShowError("Validation Failed: " + ex.Message); }
        }

        protected void RadioButtonList2_SelectedIndexChanged(object sender, EventArgs e)
        {
            PaymentDetailsPanel.Visible = RadioButtonList3.SelectedValue == "Yes";
            First.Visible = false; Second.Visible = false; Third.Visible = false;

            if (RadioButtonList2.SelectedValue == "Cash") First.Visible = true;
            else if (RadioButtonList2.SelectedValue == "Cheque" || RadioButtonList2.SelectedValue == "DD") Second.Visible = true;
            else Third.Visible = true;
        }

        // --- SAVE TRANSACTION ---
        protected void Button3_Click(object sender, EventArgs e)
        {
            UpdateStateFromGrid();

            if (GridData.Rows.Count == 0)
            {
                ShowError("Please add at least one product.");
                return;
            }

            string purchesId = GenerateID("tbl_Purches", "Purches_Id", "PR");
            string paymentId = GenerateID("tbl_Purchess_payment", "Payment_ID", "PN");

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    double grandTotal = 0;
                    double totalTax = 0;

                    foreach (DataRow row in GridData.Rows)
                    {
                        string pCode = row["Ser_pro_code"].ToString();
                        string pName = row["Ser_pro_Name"].ToString();

                        double qty = 0, rate = 0, discP = 0, vatRate = 0;
                        double.TryParse(row["Quantity"].ToString(), out qty);
                        double.TryParse(row["Vendor_rate"].ToString(), out rate);
                        double.TryParse(row["DiscountPercent"].ToString(), out discP);
                        double.TryParse(row["VatRate"].ToString(), out vatRate);

                        string taxApp = row["TaxApplicable"].ToString();
                        string spec = row["sepecification"].ToString();
                        string order = row["Order"].ToString();

                        if (qty <= 0) continue;

                        double baseAmount = qty * rate;
                        double discAmt = baseAmount * (discP / 100);
                        double taxable = baseAmount - discAmt;
                        double taxAmt = (taxApp == "Yes") ? taxable * (vatRate / 100) : 0;
                        double lineTotal = taxable + taxAmt;

                        grandTotal += lineTotal;
                        totalTax += taxAmt;

                        string sqlDet = @"INSERT INTO tbl_purches_details 
                            (sl_no, Purches_id, Product_id, Product_name, vendor_rate, tax_applicable, tax_rate, Quantity, 
                             purches_rate, total_purches_rate, vat_amount, specification, DiscountPercent, DiscountAmount, 
                             TaxableAmount, ShippedToLoc, ShippedDate, Purches_date, Client_id) 
                            VALUES (@sl, @pid, @prodid, @pname, @vrate, @taxapp, @trate, @qty, @prate, @totrate, @vat, @spec, 
                             @discP, @discA, @taxable, @store, @sdate, @pdate, @clid)";

                        SqlCommand cmdDet = new SqlCommand(sqlDet, conn, trans);
                        cmdDet.Parameters.AddWithValue("@sl", order);
                        cmdDet.Parameters.AddWithValue("@pid", purchesId);
                        cmdDet.Parameters.AddWithValue("@prodid", pCode);
                        cmdDet.Parameters.AddWithValue("@pname", pName);
                        cmdDet.Parameters.AddWithValue("@vrate", rate);
                        cmdDet.Parameters.AddWithValue("@taxapp", taxApp);
                        cmdDet.Parameters.AddWithValue("@trate", vatRate);
                        cmdDet.Parameters.AddWithValue("@qty", qty);
                        cmdDet.Parameters.AddWithValue("@prate", baseAmount);
                        cmdDet.Parameters.AddWithValue("@totrate", lineTotal);
                        cmdDet.Parameters.AddWithValue("@vat", taxAmt);
                        cmdDet.Parameters.AddWithValue("@spec", spec);
                        cmdDet.Parameters.AddWithValue("@discP", discP);
                        cmdDet.Parameters.AddWithValue("@discA", discAmt);
                        cmdDet.Parameters.AddWithValue("@taxable", taxable);
                        cmdDet.Parameters.AddWithValue("@store", DDL_ShippedTo.SelectedValue);
                        cmdDet.Parameters.AddWithValue("@sdate", txt_stockadddate.Text);
                        cmdDet.Parameters.AddWithValue("@pdate", txtPurchesDate.Text);
                        cmdDet.Parameters.AddWithValue("@clid", lblvendor_id.Text);
                        cmdDet.ExecuteNonQuery();

                        if (RadioButtonList1.SelectedValue == "Product")
                        {
                            UpdateStock(pCode, pName, qty, rate, vatRate, DDL_ShippedTo.SelectedValue, DDL_ShippedTo.SelectedItem.Text, txt_stockadddate.Text, conn, trans);
                        }
                    }

                    double delAmt = 0, tcsAmt = 0, oth1 = 0, oth2 = 0;
                    double.TryParse(txt_delivery_amnt.Text, out delAmt);
                    double.TryParse(txt_tcs_amnt.Text, out tcsAmt);
                    double.TryParse(txt_othr_amnt1.Text, out oth1);
                    double.TryParse(txt_othr_amnt2.Text, out oth2);

                    grandTotal += (delAmt + tcsAmt + oth1 + oth2);

                    string sqlHead = @"INSERT INTO tbl_Purches 
                        (Purches_Id, Client_Id, Total_purches_rate, Total_Tax_rate, Purches_date, Purches_Type, 
                         Invoice_No, Stock_Add_Date, Narration, InvoiceAmnt, TCS_Amount, TCS_Rate, Delivery_Amount, 
                         Delivery_Rate, otherAmount1_name, otherAmount1, otherAmount2_name, otherAmount2, 
                         AddedById, CreatedDate, ShippedToStoreId, ShippedToStoreName, BuyerOrderNo, OrderDate) 
                        VALUES (@pid, @clid, @tot, @tax, @pdate, @ptype, @inv, @sdate, @narr, @invAmt, @tcs, @tcsR, @del, 
                         @delR, @oth1n, @oth1, @oth2n, @oth2, @uid, GETDATE(), @store, @storeName, @bno, @bdate)";

                    SqlCommand cmdHead = new SqlCommand(sqlHead, conn, trans);
                    cmdHead.Parameters.AddWithValue("@pid", purchesId);
                    cmdHead.Parameters.AddWithValue("@clid", lblvendor_id.Text);
                    cmdHead.Parameters.AddWithValue("@tot", grandTotal);
                    cmdHead.Parameters.AddWithValue("@tax", totalTax);
                    cmdHead.Parameters.AddWithValue("@pdate", txtPurchesDate.Text);
                    cmdHead.Parameters.AddWithValue("@ptype", RadioButtonList1.SelectedValue);
                    cmdHead.Parameters.AddWithValue("@inv", txt_invno.Text);
                    cmdHead.Parameters.AddWithValue("@sdate", txt_stockadddate.Text);
                    cmdHead.Parameters.AddWithValue("@narr", txt_narration.Text);
                    cmdHead.Parameters.AddWithValue("@invAmt", grandTotal);
                    cmdHead.Parameters.AddWithValue("@tcs", tcsAmt);
                    cmdHead.Parameters.AddWithValue("@tcsR", txt_tcs_percent.Text);
                    cmdHead.Parameters.AddWithValue("@del", delAmt);
                    cmdHead.Parameters.AddWithValue("@delR", DDL_vat_parsentage.SelectedValue);
                    cmdHead.Parameters.AddWithValue("@oth1", oth1);
                    cmdHead.Parameters.AddWithValue("@oth2", oth2);
                    cmdHead.Parameters.AddWithValue("@oth1n", TextBox1.Text.Trim());
                    cmdHead.Parameters.AddWithValue("@oth2n", TextBox2.Text.Trim());
                    cmdHead.Parameters.AddWithValue("@bno", txt_reforder.Text.Trim());
                    cmdHead.Parameters.AddWithValue("@bdate", txt_refordrdate.Text.Trim());

                    string uid = "admin";
                    if (Session["USERID"] != null) uid = Session["USERID"].ToString();
                    cmdHead.Parameters.AddWithValue("@uid", uid);

                    cmdHead.Parameters.AddWithValue("@store", DDL_ShippedTo.SelectedValue);
                    cmdHead.Parameters.AddWithValue("@storeName", DDL_ShippedTo.SelectedItem.Text);
                    cmdHead.ExecuteNonQuery();

                    double paidAmt = 0;
                    if (RadioButtonList3.SelectedValue == "Yes")
                    {
                        double.TryParse(txtpaymentamount.Text, out paidAmt);
                        string sqlPay = "INSERT INTO tbl_Purchess_payment (Payment_ID, Payment_Date, Purchess_ID, Purches_Date, Client_Id, Net_amount, Given_amount, type, Ch_no, Ch_bank, Ch_date, Due_amount) VALUES (@pid, @pdate, @purid, @purdate, @clid, @net, @given, @type, @chno, @chbank, @chdate, @due)";
                        string chNo = "", chBank = "", chDate = "";
                        if (RadioButtonList2.SelectedValue == "Cheque" || RadioButtonList2.SelectedValue == "DD") { chNo = txtDDno.Text; chBank = txtBankName.Text; chDate = txtdddate.Text; }
                        else if (RadioButtonList2.SelectedValue == "Online") { chNo = txtneftnumber.Text; chBank = txtbankname1.Text; chDate = txtneftdate.Text; }

                        SqlCommand cmdPay = new SqlCommand(sqlPay, conn, trans);
                        cmdPay.Parameters.AddWithValue("@pid", paymentId);
                        cmdPay.Parameters.AddWithValue("@pdate", txtpaymentdate.Text);
                        cmdPay.Parameters.AddWithValue("@purid", purchesId);
                        cmdPay.Parameters.AddWithValue("@purdate", txtPurchesDate.Text);
                        cmdPay.Parameters.AddWithValue("@clid", lblvendor_id.Text);
                        cmdPay.Parameters.AddWithValue("@net", grandTotal);
                        cmdPay.Parameters.AddWithValue("@given", paidAmt);
                        cmdPay.Parameters.AddWithValue("@type", RadioButtonList2.SelectedValue);
                        cmdPay.Parameters.AddWithValue("@chno", chNo);
                        cmdPay.Parameters.AddWithValue("@chbank", chBank);
                        cmdPay.Parameters.AddWithValue("@chdate", chDate);
                        cmdPay.Parameters.AddWithValue("@due", grandTotal - paidAmt);
                        cmdPay.ExecuteNonQuery();
                    }

                    SqlCommand cmdDue = new SqlCommand("INSERT INTO tbl_purches_due (Purches_Id, Due_amount) VALUES (@pid, @due)", conn, trans);
                    cmdDue.Parameters.AddWithValue("@pid", purchesId);
                    cmdDue.Parameters.AddWithValue("@due", grandTotal - paidAmt);
                    cmdDue.ExecuteNonQuery();

                    trans.Commit();

                    lblOk.Text = string.Format("Purchase Saved Successfully! ID: {0}", purchesId);
                    PanelOK.Visible = true;
                    PanelError.Visible = false;
                    Button3.Enabled = false;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    ShowError("Transaction Failed: " + ex.Message);
                }
            }
        }

        private void UpdateStock(string pid, string name, double qty, double rate, double tax, string sid, string sname, string date, SqlConnection c, SqlTransaction t)
        {
            string sqlS = @"UPDATE tbl_stock 
                            SET Quantity_Num = ISNULL(Quantity_Num, 0) + @q,
                                Quantity = CAST(ISNULL(Quantity_Num, 0) + @q AS NVARCHAR(50)),
                                Sail_Rate = @r,
                                Service_tax_rate = @t,
                                ShippedDate = @d,
                                ModifiedOn = GETDATE(),
                                ModifiedByUserId = @u
                            WHERE Product_id = @p AND ShippedToStoreId = @s";

            string uid = "admin";
            if (Session["USERID"] != null) uid = Session["USERID"].ToString();

            SqlCommand cmdS = new SqlCommand(sqlS, c, t);
            cmdS.Parameters.AddWithValue("@q", qty);
            cmdS.Parameters.AddWithValue("@r", rate);
            cmdS.Parameters.AddWithValue("@t", tax);
            cmdS.Parameters.AddWithValue("@d", date);
            cmdS.Parameters.AddWithValue("@u", uid);
            cmdS.Parameters.AddWithValue("@p", pid);
            cmdS.Parameters.AddWithValue("@s", sid);
            int rows = cmdS.ExecuteNonQuery();

            if (rows == 0)
            {
                string sqlI = @"INSERT INTO tbl_stock (Product_id, Product_name, Quantity, Sail_Rate, Service_tax_rate, ShippedToStoreId, ShippedToStoreName, ShippedDate, Quantity_Num, AddedOn, ModifiedByUserId) 
                                VALUES (@p, @n, CAST(@q AS NVARCHAR(50)), @r, @t, @s, @sn, @d, @q, GETDATE(), @u)";
                SqlCommand cmdI = new SqlCommand(sqlI, c, t);
                cmdI.Parameters.AddWithValue("@p", pid);
                cmdI.Parameters.AddWithValue("@n", name);
                cmdI.Parameters.AddWithValue("@q", qty);
                cmdI.Parameters.AddWithValue("@r", rate);
                cmdI.Parameters.AddWithValue("@t", tax);
                cmdI.Parameters.AddWithValue("@s", sid);
                cmdI.Parameters.AddWithValue("@sn", sname);
                cmdI.Parameters.AddWithValue("@d", date);
                cmdI.Parameters.AddWithValue("@u", uid);
                cmdI.ExecuteNonQuery();
            }

            string sqlM = @"UPDATE tbl_NewProduct 
                            SET Quantity_Num = ISNULL(Quantity_Num, 0) + @q,
                                Quantity = CAST(ISNULL(Quantity_Num, 0) + @q AS NVARCHAR(100)),
                                Sail_Rate = @r,
                                Tax_Rate = @t,
                                ModifiedOn = GETDATE(),
                                ModifiedByUserId = @u
                            WHERE ProductID = @p";
            SqlCommand cmdM = new SqlCommand(sqlM, c, t);
            cmdM.Parameters.AddWithValue("@q", qty);
            cmdM.Parameters.AddWithValue("@r", rate);
            cmdM.Parameters.AddWithValue("@t", tax);
            cmdM.Parameters.AddWithValue("@u", uid);
            cmdM.Parameters.AddWithValue("@p", pid);
            cmdM.ExecuteNonQuery();
        }

        private string GenerateID(string table, string column, string prefix)
        {
            string newId = prefix + "00001";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(string.Format("SELECT TOP 1 {0} FROM {1} ORDER BY ID DESC", column, table), conn);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    string lastId = result.ToString();
                    string numPart = lastId.Replace(prefix, "");
                    int num = 0;
                    if (int.TryParse(numPart, out num))
                    {
                        newId = prefix + (num + 1).ToString("D5");
                    }
                }
            }
            return newId;
        }

        private void ShowError(string msg)
        {
            lblErrorMsg.Text = msg;
            PanelError.Visible = true;
            PanelOK.Visible = false;
        }
    }
}

/*
=======================================================================
File: Purches_exting_vendor.aspx.cs
Revised On: 16-Mar-2026
Description: Purchase Wizard Backend (Step 1 to 4)
Updates:
- Added Data Persistence (UpdateStateFromGrid) to prevent grid data loss on re-order/remove.
- Fixed Decimal tracking for stock accuracy via 'Quantity_Num'.
- Restored Audit tracking parameters (AddedOn, ModifiedOn, ModifiedByUserId) in UpdateStock method.
=======================================================================
*/
