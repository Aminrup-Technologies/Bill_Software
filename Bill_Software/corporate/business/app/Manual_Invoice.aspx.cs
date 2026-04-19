using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class Manual_Invoice : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }

            if (!IsPostBack)
            {
                LoadClients();
                LoadCategories();
                LoadSalesPersons();

                txtInvoiceDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                ViewState["PhaseProductData"] = CreateCartTable();
            }
        }

        #region STEP 1: SETUP (CLIENT & DETAILS)
        private void LoadClients()
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT Client_Id, Client_Name FROM tbl_Client WHERE CompanyID = @CompanyID ORDER BY Id", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        cmbClient.DataSource = dt;
                        cmbClient.DataTextField = "Client_Name";
                        cmbClient.DataValueField = "Client_Id";
                        cmbClient.DataBind();
                    }
                }
            }
            cmbClient.Items.Insert(0, new ListItem("-- Select Client --", "0"));
        }

        private void LoadSalesPersons()
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT User_Id, Name FROM tbl_login WHERE (User_Id NOT IN ('admin', 'AT01')) and CompanyID = @CompanyID and IsActive=1 ORDER BY Name", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        dt.Columns.Add("DisplayFormat", typeof(string), "Name + ' (' + User_Id + ')'");
                        cmbSalesPerson.DataSource = dt;
                        cmbSalesPerson.DataTextField = "DisplayFormat";
                        cmbSalesPerson.DataValueField = "User_Id";
                        cmbSalesPerson.DataBind();
                    }
                }
            }
            cmbSalesPerson.Items.Insert(0, new ListItem("-- Select Sales Person --", ""));
        }

        protected void cmbClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbClient.SelectedIndex > 0)
            {
                DbCL.Sqlconnection();
                DbCL.ConnectDb();
                try
                {
                    SqlCommand cmd = new SqlCommand("SELECT Client_Id FROM tbl_Client WHERE Client_Name=@Name AND CompanyID=@CompanyID", DbCL.Conn);
                    cmd.Parameters.AddWithValue("@Name", cmbClient.SelectedItem.Text);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    object res = cmd.ExecuteScalar();
                    if (res != null)
                    {
                        lblClientID.Text = res.ToString();
                        LoadAddresses(lblClientID.Text);
                    }
                }
                catch { }
                finally { DbCL.Conn.Close(); }
            }
        }

        private void LoadAddresses(string cid)
        {
            lstAddresses.Items.Clear();
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                conn.Open();
                SqlCommand cmd1 = new SqlCommand("SELECT Address1+', '+City+', '+pin+', '+State FROM tbl_Client WHERE Client_Id=@CID AND CompanyID=@CompanyID", conn);
                cmd1.Parameters.AddWithValue("@CID", cid);
                cmd1.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                using (SqlDataReader dr = cmd1.ExecuteReader())
                {
                    while (dr.Read()) lstAddresses.Items.Add(dr[0].ToString());
                }

                SqlCommand cmd2 = new SqlCommand("SELECT Address+', '+State+', '+City+', '+pin FROM tbl_ClientRegAddress WHERE Client_Id=@CID AND CompanyID=@CompanyID", conn);
                cmd2.Parameters.AddWithValue("@CID", cid);
                cmd2.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                using (SqlDataReader dr = cmd2.ExecuteReader())
                {
                    while (dr.Read()) lstAddresses.Items.Add(dr[0].ToString());
                }
            }
            if (lstAddresses.Items.Count > 0) lstAddresses.Items[0].Selected = true;
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            if (cmbClient.SelectedIndex == 0) { ShowMsg("Please select a Client.", false); return; }
            if (lstAddresses.SelectedIndex == -1) { ShowMsg("Please select at least one Address.", false); return; }
            if (rbTaxType.SelectedIndex == -1) { ShowMsg("Action Blocked: You must select a Tax Type (Intra or Inter).", false); return; }
            if (cmbSalesPerson.SelectedIndex <= 0 || string.IsNullOrEmpty(cmbSalesPerson.SelectedValue)) { ShowMsg("Please select a Sales Person.", false); return; }

            ShowMsg("", true);
            mvInvoice.ActiveViewIndex = 1;
        }
        #endregion

        #region STEP 2: PRODUCT SELECTION
        private void LoadCategories()
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT DISTINCT ProductOrServiceCat FROM tbl_NewparentProduct WHERE CompanyID=@CompanyID ORDER BY ProductOrServiceCat", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        cmbCategory.DataSource = dt;
                        cmbCategory.DataTextField = "ProductOrServiceCat";
                        cmbCategory.DataValueField = "ProductOrServiceCat";
                        cmbCategory.DataBind();
                    }
                }
            }
            cmbCategory.Items.Insert(0, new ListItem("All Categories", "All"));
        }

        protected void cmbCategory_SelectedIndexChanged(object sender, EventArgs e) { BindProductGrid(); }

        private void BindProductGrid()
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                string qry = "SELECT ProductID, Product_code, ProductName, Brand, Unit, Sail_Rate, Tax_Rate, Quantity FROM tbl_NewProduct WHERE CompanyID=@CompanyID";
                if (cmbCategory.SelectedIndex > 0) qry += " AND ProductOrServiceCat = @Cat";
                qry += " ORDER BY ProductName";

                using (SqlCommand cmd = new SqlCommand(qry, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    if (cmbCategory.SelectedIndex > 0) cmd.Parameters.AddWithValue("@Cat", cmbCategory.SelectedItem.Text);

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        gridProdWithCat.DataSource = dt;
                        gridProdWithCat.DataBind();
                    }
                }
            }
        }

        protected void btnAddToCart_Click(object sender, EventArgs e)
        {
            DataTable dtCart = (DataTable)ViewState["PhaseProductData"];
            bool itemAdded = false;

            foreach (GridViewRow row in gridProdWithCat.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked)
                {
                    string pid = gridProdWithCat.DataKeys[row.RowIndex].Value.ToString();

                    DataRow[] exist = dtCart.Select("ProductID='" + pid + "'");
                    if (exist.Length == 0)
                    {
                        DataRow dr = dtCart.NewRow();
                        dr["ProductID"] = pid;
                        dr["Product_code"] = HttpUtility.HtmlDecode(row.Cells[2].Text);
                        dr["ProductName"] = HttpUtility.HtmlDecode(row.Cells[3].Text);
                        dr["Brand"] = HttpUtility.HtmlDecode(row.Cells[4].Text); // Storing the Spec natively

                        Label lblRate = (Label)row.FindControl("lblBaseRate");
                        Label lblTax = (Label)row.FindControl("lblGstRate");

                        decimal sRate = 0, tRate = 0;
                        if (lblRate != null) decimal.TryParse(lblRate.Text, out sRate);
                        if (lblTax != null) decimal.TryParse(lblTax.Text, out tRate);

                        dr["Sail_Rate"] = sRate;
                        dr["Tax_Rate"] = tRate;
                        dr["IQuantity"] = 1;
                        dr["Discount_Rate"] = 0;
                        dtCart.Rows.Add(dr);
                        itemAdded = true;
                    }
                    chk.Checked = false;
                }
            }

            if (itemAdded)
            {
                ViewState["PhaseProductData"] = dtCart;
                BindCartGrid();
                lblClientDisplay.Text = cmbClient.SelectedItem.Text;
                lblTaxModeDisplay.Text = rbTaxType.SelectedItem.Text;
                mvInvoice.ActiveViewIndex = 2;
                ScriptManager.RegisterStartupScript(this, GetType(), "calc", "setTimeout(function(){ var rows=document.getElementById('" + gd_Cart.ClientID + "').getElementsByTagName('tr'); for(var i=1;i<rows.length;i++){ var t=rows[i].querySelector(\"input[id*='txtQty']\"); if(t) CalculateRow(t,'MAIN'); } }, 500);", true);
            }
            else
            {
                ShowMsg("No new items were selected, or items are already in the cart.", false);
            }
        }

        protected void btnBackSetup_Click(object sender, EventArgs e) { mvInvoice.ActiveViewIndex = 0; }
        #endregion

        #region STEP 3: REVIEW & CART MANAGEMENT
        private DataTable CreateCartTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ProductID"); // The True ID (PRD...)
            dt.Columns.Add("Product_code"); // The HSN Code
            dt.Columns.Add("ProductName");
            dt.Columns.Add("Brand");
            dt.Columns.Add("IQuantity", typeof(decimal));
            dt.Columns.Add("Sail_Rate", typeof(decimal));
            dt.Columns.Add("Tax_Rate", typeof(decimal));
            dt.Columns.Add("Discount_Rate", typeof(decimal));
            return dt;
        }

        private void BindCartGrid()
        {
            gd_Cart.DataSource = (DataTable)ViewState["PhaseProductData"];
            gd_Cart.DataBind();
        }

        protected void gd_Cart_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "MoveUp" || e.CommandName == "MoveDown" || e.CommandName == "Remove")
            {
                int idx = Convert.ToInt32(e.CommandArgument);
                DataTable dt = (DataTable)ViewState["PhaseProductData"];
                UpdateCartFromGrid(dt);

                if (e.CommandName == "Remove") dt.Rows[idx].Delete();
                else if (e.CommandName == "MoveUp" && idx > 0)
                {
                    DataRow r = dt.NewRow();
                    r.ItemArray = dt.Rows[idx].ItemArray;
                    dt.Rows.RemoveAt(idx);
                    dt.Rows.InsertAt(r, idx - 1);
                }
                else if (e.CommandName == "MoveDown" && idx < dt.Rows.Count - 1)
                {
                    DataRow r = dt.NewRow();
                    r.ItemArray = dt.Rows[idx].ItemArray;
                    dt.Rows.RemoveAt(idx);
                    dt.Rows.InsertAt(r, idx + 1);
                }

                dt.AcceptChanges();
                ViewState["PhaseProductData"] = dt;
                BindCartGrid();
                ScriptManager.RegisterStartupScript(this, GetType(), "recalc", "setTimeout(function(){ var rows=document.getElementById('" + gd_Cart.ClientID + "').getElementsByTagName('tr'); for(var i=1;i<rows.length;i++){ var t=rows[i].querySelector(\"input[id*='txtQty']\"); if(t) CalculateRow(t,'MAIN'); } }, 500);", true);
            }
        }

        private void UpdateCartFromGrid(DataTable dt)
        {
            for (int i = 0; i < gd_Cart.Rows.Count; i++)
            {
                GridViewRow row = gd_Cart.Rows[i];
                TextBox tQty = (TextBox)row.FindControl("txtQty");
                TextBox tRate = (TextBox)row.FindControl("txtRate");
                TextBox tDisc = (TextBox)row.FindControl("txtDiscPer");
                TextBox tSpec = (TextBox)row.FindControl("txtSpec"); // NEW: Grab updated specs

                if (tQty == null) continue;

                decimal q = 0, r = 0, d = 0;
                decimal.TryParse(tQty.Text, out q);
                decimal.TryParse(tRate.Text, out r);
                decimal.TryParse(tDisc.Text, out d);

                dt.Rows[i]["IQuantity"] = q;
                dt.Rows[i]["Sail_Rate"] = r;
                dt.Rows[i]["Discount_Rate"] = d;
                if (tSpec != null) dt.Rows[i]["Brand"] = tSpec.Text;
            }
        }

        protected void btnBackProd_Click(object sender, EventArgs e)
        {
            UpdateCartFromGrid((DataTable)ViewState["PhaseProductData"]);
            mvInvoice.ActiveViewIndex = 1;
        }
        #endregion

        #region SAVE TO DATABASE
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (ViewState["PhaseProductData"] == null)
                {
                    ShowMsg("Session Timeout. Please reload.", false);
                    return;
                }

                UpdateCartFromGrid((DataTable)ViewState["PhaseProductData"]);
                DataTable dtCart = (DataTable)ViewState["PhaseProductData"];

                if (dtCart.Rows.Count == 0)
                {
                    ShowMsg("Cart is empty. Please select products.", false);
                    return;
                }

                string uid = Session["USERID"] != null ? Session["USERID"].ToString() : "System";
                decimal gGross = 0, gDisc = 0, gTax = 0, gNet = 0;

                // 1. Calculate Server Side Math
                foreach (DataRow dr in dtCart.Rows)
                {
                    decimal q = Convert.ToDecimal(dr["IQuantity"]);
                    if (q <= 0) continue; // FIX: Skip empty quantities during math

                    decimal r = Convert.ToDecimal(dr["Sail_Rate"]);
                    decimal dPer = Convert.ToDecimal(dr["Discount_Rate"]);
                    decimal tPer = Convert.ToDecimal(dr["Tax_Rate"]);

                    decimal rowGross = Math.Round(q * r, 2);
                    decimal rowDisc = Math.Round((rowGross * dPer) / 100, 2);
                    decimal taxable = Math.Round(rowGross - rowDisc, 2);
                    decimal rowTax = Math.Round((taxable * tPer) / 100, 2);

                    gGross += rowGross;
                    gDisc += rowDisc;
                    gTax += rowTax;
                    gNet += (taxable + rowTax);
                }

                decimal frt = 0, oth = 0;
                decimal.TryParse(txtFreight.Text, out frt);
                decimal.TryParse(txtOtherCharge.Text, out oth);

                gNet += Math.Round(frt, 2) + Math.Round(oth, 2);
                gNet = Math.Round(gNet, 2);

                // FIX: Server-Side Block for Zero Total
                if (gNet <= 0)
                {
                    ShowMsg("Action Blocked: Cannot save an invoice with a total of zero.", false);
                    return;
                }

                string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

                // 2. Perform Database Transaction
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    try
                    {
                        int slNo = 1;
                        SqlCommand cmdSl = new SqlCommand("SELECT ISNULL(MAX(CAST(CASE WHEN ISNULL(Sl_no, '') = '' OR ISNUMERIC(Sl_no) = 0 THEN '0' ELSE Sl_no END AS INT)), 0) + 1 FROM tbl_Invoice WHERE CompanyID=@CompanyID", conn, tran);
                        cmdSl.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        object slRes = cmdSl.ExecuteScalar();
                        if (slRes != null && slRes != DBNull.Value) slNo = Convert.ToInt32(slRes);

                        string invNo = GenerateInvoiceNo(slNo);

                        // A. Insert Header
                        string sqlH = "INSERT INTO tbl_Invoice (Invoice_No, Invoice_Date, Quotation_No, ExtInvoiceNo, Client_ID, Gross, discount, sub_total, Service_Tax1, Net_Amount, Sl_no, Delivery_Amount, otherAmount1, otherAmount1_name, status1, status2, cgstOrsgst, igst, AddedById, CompanyID, SalesPersonCode) VALUES (@Inv, @Date, @PO, @ERP, @CID, @Gr, @Di, @Sub, @Tax, @Net, @Sl, @Frt, @Oth, @OthName, 'No', 'Active', @Intra, @Inter, @User, @CompanyID, @SalesPerson)";
                        SqlCommand cmdH = new SqlCommand(sqlH, conn, tran);
                        cmdH.Parameters.AddWithValue("@Inv", invNo);
                        cmdH.Parameters.AddWithValue("@Date", txtInvoiceDate.Text);

                        string strPo = string.IsNullOrWhiteSpace(txtPONo.Text) ? "N/A" : txtPONo.Text.Trim();
                        cmdH.Parameters.AddWithValue("@PO", strPo);
                        cmdH.Parameters.AddWithValue("@ERP", txtERPRef.Text.Trim());
                        cmdH.Parameters.AddWithValue("@CID", lblClientID.Text);

                        cmdH.Parameters.Add("@Gr", SqlDbType.Decimal).Value = gGross;
                        cmdH.Parameters.Add("@Di", SqlDbType.Decimal).Value = gDisc;
                        cmdH.Parameters.Add("@Sub", SqlDbType.Decimal).Value = Math.Round(gGross - gDisc, 2);
                        cmdH.Parameters.Add("@Tax", SqlDbType.Decimal).Value = gTax;
                        cmdH.Parameters.Add("@Net", SqlDbType.Decimal).Value = gNet;

                        cmdH.Parameters.AddWithValue("@Sl", slNo);
                        cmdH.Parameters.Add("@Frt", SqlDbType.Decimal).Value = frt;
                        cmdH.Parameters.Add("@Oth", SqlDbType.Decimal).Value = oth;
                        cmdH.Parameters.AddWithValue("@OthName", "Other Charges");

                        cmdH.Parameters.AddWithValue("@Intra", rbTaxType.SelectedValue == "1" ? "YES" : (object)DBNull.Value);
                        cmdH.Parameters.AddWithValue("@Inter", rbTaxType.SelectedValue == "0" ? "YES" : (object)DBNull.Value);
                        cmdH.Parameters.AddWithValue("@User", uid);

                        cmdH.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        string selectedSalesPerson = cmbSalesPerson.SelectedIndex > 0 ? cmbSalesPerson.SelectedValue : "";
                        cmdH.Parameters.AddWithValue("@SalesPerson", selectedSalesPerson);

                        cmdH.ExecuteNonQuery();

                        // B. Insert Details & Update Stock
                        foreach (DataRow dr in dtCart.Rows)
                        {
                            decimal q = Convert.ToDecimal(dr["IQuantity"]);
                            if (q <= 0) continue; // FIX: Do not insert zero-quantity rows

                            decimal r = Convert.ToDecimal(dr["Sail_Rate"]);
                            decimal dPer = Convert.ToDecimal(dr["Discount_Rate"]);
                            decimal tPer = Convert.ToDecimal(dr["Tax_Rate"]);

                            decimal rowGross = Math.Round(q * r, 2);
                            decimal rowDisc = Math.Round((rowGross * dPer) / 100, 2);
                            decimal taxable = Math.Round(rowGross - rowDisc, 2);
                            decimal rowTax = Math.Round((taxable * tPer) / 100, 2);
                            decimal rowNet = Math.Round(taxable + rowTax, 2);

                            // EXPLICIT GHOST DATA FIX MAPPING:
                            // @PID = TrueID ("PRD..."), @HSN = HSN Code ("0" / numbers)
                            string sqlD = "INSERT INTO tbl_Invoice_details (Invoice_No, Quotation_no, Product_id, Product_Code, Product_name, Quantity, sail_rate, discountRate, Service_tax_rate, Total_sail_rate1, Total_sail_rate2, specification, AddedById, CompanyID) VALUES (@Inv, @PO, @PID, @HSN, @Name, @Qty, @Rate, @DPer, @TPer, @Net, @Base, @Brand, @User, @CompanyID)";
                            SqlCommand cmdD = new SqlCommand(sqlD, conn, tran);
                            cmdD.Parameters.AddWithValue("@Inv", invNo);
                            cmdD.Parameters.AddWithValue("@PO", strPo);

                            cmdD.Parameters.AddWithValue("@PID", dr["ProductID"]); // Maps TrueID -> Product_id
                            cmdD.Parameters.AddWithValue("@HSN", dr["Product_code"]); // Maps TrueHSN -> Product_Code

                            cmdD.Parameters.AddWithValue("@Name", dr["ProductName"]);
                            cmdD.Parameters.Add("@Qty", SqlDbType.Decimal).Value = q;
                            cmdD.Parameters.Add("@Rate", SqlDbType.Decimal).Value = r;
                            cmdD.Parameters.Add("@DPer", SqlDbType.Decimal).Value = dPer;
                            cmdD.Parameters.Add("@TPer", SqlDbType.Decimal).Value = tPer;
                            cmdD.Parameters.Add("@Net", SqlDbType.Decimal).Value = rowNet;
                            cmdD.Parameters.Add("@Base", SqlDbType.Decimal).Value = taxable;
                            cmdD.Parameters.AddWithValue("@Brand", dr["Brand"]);
                            cmdD.Parameters.AddWithValue("@User", uid);
                            cmdD.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                            cmdD.ExecuteNonQuery();

                            // MANUAL INVOICE ALWAYS DEDUCTS STOCK (No Challan Check Needed Here)
                            string sqlStock = "UPDATE tbl_NewProduct SET Quantity = CAST(CASE WHEN ISNULL(Quantity, '') = '' THEN '0' ELSE Quantity END AS DECIMAL(18,2)) - @Qty WHERE ProductID = @PID AND CompanyID = @CompanyID";
                            SqlCommand cmdS = new SqlCommand(sqlStock, conn, tran);
                            cmdS.Parameters.Add("@Qty", SqlDbType.Decimal).Value = q;
                            cmdS.Parameters.Add("@PID", SqlDbType.VarChar).Value = dr["ProductID"].ToString(); // Maps TrueID -> ProductID
                            cmdS.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                            cmdS.ExecuteNonQuery();
                        }

                        // C. Insert Addresses
                        foreach (ListItem itm in lstAddresses.Items)
                        {
                            if (itm.Selected)
                            {
                                SqlCommand cmdA = new SqlCommand("INSERT INTO tbl_InvSiteAddress (invoice_no, SiteAddress, CompanyID) VALUES (@Inv, @Addr, @CompanyID)", conn, tran);
                                cmdA.Parameters.AddWithValue("@Inv", invNo);
                                cmdA.Parameters.AddWithValue("@Addr", itm.Text);
                                cmdA.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                                cmdA.ExecuteNonQuery();
                            }
                        }

                        // D. System Notification Logging
                        InsertSystemNotification("New Manual Tax Invoice Generated", $"Invoice #{invNo} generated for {cmbClient.SelectedItem.Text} ({gNet.ToString("C")}).", "INVOICE", "Success", uid, CompanyContext.CurrentCompanyID, conn, tran);

                        tran.Commit();
                        ShowMsg("Success! Manual Invoice Generated: " + invNo, true);
                        WriteLog("Manual Invoice successfully generated: " + invNo);

                        // Clear Data
                        ViewState["PhaseProductData"] = CreateCartTable();
                        gd_Cart.DataSource = null;
                        gd_Cart.DataBind();
                        txtPONo.Text = "";
                        txtERPRef.Text = "";
                        cmbSalesPerson.SelectedIndex = -1;
                        txtFreight.Text = "0";
                        txtOtherCharge.Text = "0";

                        mvInvoice.ActiveViewIndex = 0;
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMsg("Error: " + ex.Message, false);
                WriteLog(ex.ToString());
            }
        }

        private void InsertSystemNotification(string title, string message, string module, string type, string userId, int companyId, SqlConnection conn, SqlTransaction tran)
        {
            try
            {
                string sql = "INSERT INTO tbl_SystemNotification (Title, Message, Module, Type, UserID, CreatedDate, IsRead, CompanyID) VALUES (@Title, @Msg, @Mod, @Type, @User, GETDATE(), 0, @Comp)";
                SqlCommand cmd = new SqlCommand(sql, conn, tran);
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Msg", message);
                cmd.Parameters.AddWithValue("@Mod", module);
                cmd.Parameters.AddWithValue("@Type", type);
                cmd.Parameters.AddWithValue("@User", userId);
                cmd.Parameters.AddWithValue("@Comp", companyId);
                cmd.ExecuteNonQuery();
            }
            catch { }
        }
        #endregion

        #region UTILITIES
        private void ShowMsg(string msg, bool ok)
        {
            PanelMsg.Visible = !string.IsNullOrEmpty(msg);
            lblMsg.Text = msg;
            lblMsg.ForeColor = ok ? System.Drawing.Color.Green : System.Drawing.Color.Red;
        }

        private string GenerateInvoiceNo(int slNo)
        {
            DateTime dt;
            if (!DateTime.TryParse(txtInvoiceDate.Text, out dt)) dt = DateTime.Now;

            string yy = "";
            if (dt.Month >= 4) yy = dt.Year.ToString().Substring(2) + "-" + (dt.Year + 1).ToString().Substring(2);
            else yy = (dt.Year - 1).ToString().Substring(2) + "-" + dt.Year.ToString().Substring(2);

            return "INV/C/" + yy + "/" + slNo;
        }

        private void WriteLog(string txt)
        {
            try
            {
                string p = Server.MapPath("~/Uploads/InvoiceLogs/Log.txt");
                if (!Directory.Exists(Path.GetDirectoryName(p))) Directory.CreateDirectory(Path.GetDirectoryName(p));
                File.AppendAllText(p, DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + " : " + txt + Environment.NewLine);
            }
            catch { }
        }
        #endregion
    }
}