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
                Context.ApplicationInstance.CompleteRequest(); // The modern, safe way to stop execution
                return; // Exit the method immediately
            }

            if (!IsPostBack)
            {
                LoadClients();
                LoadCategories();
                txtInvoiceDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                ViewState["PhaseProductData"] = CreateCartTable();
            }
        }

        #region STEP 1: SETUP
        private void LoadClients()
        {
            DbCL.FillCombo(cmbClient, "select Client_Name from tbl_Client order by Client_Name");
            cmbClient.Items.Insert(0, new ListItem("-- Select Client --", "0"));
        }

        protected void cmbClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbClient.SelectedIndex > 0)
            {
                try
                {
                    DbCL.Sqlconnection();
                    DbCL.ConnectDb();
                    SqlCommand cmd = new SqlCommand("select Client_Id from tbl_Client where Client_Name=@Name", DbCL.Conn);
                    cmd.Parameters.AddWithValue("@Name", cmbClient.SelectedItem.Text);
                    object res = cmd.ExecuteScalar();
                    if (res != null)
                    {
                        lblClientID.Text = res.ToString();
                        LoadAddresses(lblClientID.Text);
                    }
                }
                catch (Exception ex)
                {
                    WriteLog("CLIENT SELECT ERROR: " + ex.ToString());
                    ShowMsg("We couldn't load the details for this client. Please refresh and try again.", false);
                }
                finally
                {
                    if (DbCL.Conn.State == ConnectionState.Open) DbCL.Conn.Close();
                }
            }
        }

        private void LoadAddresses(string cid)
        {
            lstAddresses.Items.Clear();
            DataTable dt1 = DbCL.ReturnDataTable("select Address1+', '+City+', '+pin+', '+State from tbl_Client where Client_Id='" + cid + "'");
            foreach (DataRow dr in dt1.Rows) lstAddresses.Items.Add(dr[0].ToString());

            DataTable dt2 = DbCL.ReturnDataTable("select Address+', '+State+', '+City+', '+pin from tbl_ClientRegAddress where Client_Id='" + cid + "'");
            foreach (DataRow dr in dt2.Rows) lstAddresses.Items.Add(dr[0].ToString());

            if (lstAddresses.Items.Count > 0) lstAddresses.Items[0].Selected = true;
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            if (cmbClient.SelectedIndex == 0) { ShowMsg("Select Client", false); return; }
            if (lstAddresses.SelectedIndex == -1) { ShowMsg("Select Address", false); return; }

            ShowMsg("", true);
            mvInvoice.ActiveViewIndex = 1;
        }
        #endregion

        #region STEP 2: PRODUCTS
        private void LoadCategories()
        {
            DbCL.FillCombo(cmbCategory, "select distinct ProductOrServiceCat from tbl_NewparentProduct order by ProductOrServiceCat");
            cmbCategory.Items.Insert(0, new ListItem("All Categories", "All"));
        }

        protected void cmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindProductGrid();
        }

        private void BindProductGrid()
        {
            string qry = "select ProductID, Product_code, ProductName, Brand, Unit, Sail_Rate, Tax_Rate, Quantity from tbl_NewProduct";

            // Use parameterized query instead of direct concatenation
            if (cmbCategory.SelectedIndex > 0)
            {
                qry += " WHERE ProductOrServiceCat = @Cat";
            }
            qry += " ORDER BY ProductName";

            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            try
            {
                SqlCommand cmd = new SqlCommand(qry, DbCL.Conn);
                if (cmbCategory.SelectedIndex > 0)
                {
                    cmd.Parameters.AddWithValue("@Cat", cmbCategory.SelectedItem.Text);
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gridProdWithCat.DataSource = dt;
                gridProdWithCat.DataBind();
            }
            catch (Exception ex)
            {
                WriteLog("BindProductGrid Error: " + ex.ToString());
                ShowMsg("Could not load products. Please try again.", false);
            }
            finally
            {
                if (DbCL.Conn.State == ConnectionState.Open) DbCL.Conn.Close();
            }
        }

        protected void btnAddToCart_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)ViewState["PhaseProductData"];
            bool added = false;

            foreach (GridViewRow row in gridProdWithCat.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked)
                {
                    string pid = gridProdWithCat.DataKeys[row.RowIndex].Value.ToString();
                    DataRow[] exist = dt.Select("ProductID='" + pid + "'");

                    if (exist.Length == 0)
                    {
                        DataRow dr = dt.NewRow();
                        dr["ProductID"] = pid;

                        // FIX: Correct Column Indexing & HTML Decoding
                        dr["Product_code"] = HttpUtility.HtmlDecode(row.Cells[2].Text);
                        dr["ProductName"] = HttpUtility.HtmlDecode(row.Cells[3].Text);
                        dr["Brand"] = HttpUtility.HtmlDecode(row.Cells[4].Text);

                        // Extract Rate & Tax from TemplateFields
                        Label lblRate = (Label)row.FindControl("lblBaseRate");
                        Label lblTax = (Label)row.FindControl("lblGstRate");

                        dr["Sail_Rate"] = lblRate != null ? lblRate.Text : "0";
                        dr["Tax_Rate"] = lblTax != null ? lblTax.Text : "0";

                        dr["IQuantity"] = 1;
                        dr["Discount_Rate"] = 0;
                        dt.Rows.Add(dr);
                        added = true;
                    }
                    chk.Checked = false;
                }
            }

            if (added)
            {
                ViewState["PhaseProductData"] = dt;
                BindCartGrid();
                lblClientDisplay.Text = cmbClient.SelectedItem.Text;
                lblTaxModeDisplay.Text = rbTaxType.SelectedItem.Text;
                mvInvoice.ActiveViewIndex = 2;

                ScriptManager.RegisterStartupScript(this, GetType(), "calc", "setTimeout(function(){ var rows=document.getElementById('" + gd_Cart.ClientID + "').getElementsByTagName('tr'); for(var i=1;i<rows.length;i++){ var t=rows[i].querySelector(\"input[id*='txtQty']\"); if(t) CalculateRow(t,'MAIN'); } }, 500);", true);
            }
            else
            {
                ShowMsg("No new items selected.", false);
            }
        }

        protected void btnBackSetup_Click(object sender, EventArgs e)
        {
            mvInvoice.ActiveViewIndex = 0;
        }
        #endregion

        #region STEP 3: REVIEW
        private DataTable CreateCartTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ProductID");
            dt.Columns.Add("Product_code");
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
                    DataRow r = dt.NewRow(); r.ItemArray = dt.Rows[idx].ItemArray;
                    dt.Rows.RemoveAt(idx); dt.Rows.InsertAt(r, idx - 1);
                }
                else if (e.CommandName == "MoveDown" && idx < dt.Rows.Count - 1)
                {
                    DataRow r = dt.NewRow(); r.ItemArray = dt.Rows[idx].ItemArray;
                    dt.Rows.RemoveAt(idx); dt.Rows.InsertAt(r, idx + 1);
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

                if (tQty == null) continue;

                decimal q = 0, r = 0, d = 0;
                if (decimal.TryParse(tQty.Text, out q)) dt.Rows[i]["IQuantity"] = q;
                if (decimal.TryParse(tRate.Text, out r)) dt.Rows[i]["Sail_Rate"] = r;
                if (decimal.TryParse(tDisc.Text, out d)) dt.Rows[i]["Discount_Rate"] = d;
            }
        }

        protected void btnBackProd_Click(object sender, EventArgs e)
        {
            UpdateCartFromGrid((DataTable)ViewState["PhaseProductData"]);
            mvInvoice.ActiveViewIndex = 1;
        }
        #endregion

        #region SAVE LOGIC
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (ViewState["PhaseProductData"] == null) { ShowMsg("Session Timeout", false); return; }

                UpdateCartFromGrid((DataTable)ViewState["PhaseProductData"]);
                DataTable dt = (DataTable)ViewState["PhaseProductData"];
                //if (dt.Rows.Count == 0) { ShowMsg("Cart is empty", false); return; }
                // NEW: Prevent saving invoices with 0 quantity
                decimal totalCartQty = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    totalCartQty += Convert.ToDecimal(dr["IQuantity"]);
                }

                if (totalCartQty <= 0)
                {
                    ShowMsg("Please enter a valid quantity greater than 0 for your items.", false);
                    return;
                }
                string invNo = GenerateInvoiceNo();
                string uid = Session["USERID"] != null ? Session["USERID"].ToString() : "System";
                int slNo = GetSlNo();

                decimal gGross = 0, gDisc = 0, gTax = 0, gNet = 0;

                foreach (DataRow dr in dt.Rows)
                {
                    decimal q = Convert.ToDecimal(dr["IQuantity"]);
                    decimal r = Convert.ToDecimal(dr["Sail_Rate"]);
                    decimal dPer = Convert.ToDecimal(dr["Discount_Rate"]);
                    decimal tPer = Convert.ToDecimal(dr["Tax_Rate"]);

                    // FORCE ROUNDING TO 2 DECIMAL PLACES
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

                // Round extras too
                frt = Math.Round(frt, 2);
                oth = Math.Round(oth, 2);
                gNet += frt + oth;
                gNet = Math.Round(gNet, 2);

                // FIX: Fallback Connection String
                string connStr = "";
                if (DbCL.Conn != null && !string.IsNullOrEmpty(DbCL.Conn.ConnectionString))
                    connStr = DbCL.Conn.ConnectionString;
                else if (System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"] != null)
                    connStr = System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
                else
                    throw new Exception("Connection string not found.");

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    try
                    {
                        // 1. Header
                        string sqlH = "INSERT INTO tbl_Invoice (Invoice_No, Invoice_Date, Quotation_No, ExtInvoiceNo, Client_ID, Gross, discount, sub_total, Service_Tax1, Net_Amount, Sl_no, Delivery_Amount, otherAmount1, status1, status2, cgstOrsgst, igst, AddedById, PServiceName, addressfor) VALUES (@Inv, @Date, @PO, @ERP, @CID, @Gr, @Di, @Sub, @Tax, @Net, @Sl, @Frt, @Oth, 'No', 'Active', @Intra, @Inter, @User, @primarycat, @addressfor)";
                        SqlCommand cmdH = new SqlCommand(sqlH, conn, tran);
                        cmdH.Parameters.AddWithValue("@Inv", invNo);
                        cmdH.Parameters.AddWithValue("@Date", txtInvoiceDate.Text);
                        cmdH.Parameters.AddWithValue("@PO", txtPONo.Text.Trim());
                        cmdH.Parameters.AddWithValue("@ERP", txtERPRef.Text.Trim());
                        cmdH.Parameters.AddWithValue("@CID", lblClientID.Text);

                        // Explicit Types for Money
                        cmdH.Parameters.Add("@Gr", SqlDbType.Decimal).Value = gGross;
                        cmdH.Parameters.Add("@Di", SqlDbType.Decimal).Value = gDisc;
                        cmdH.Parameters.Add("@Sub", SqlDbType.Decimal).Value = Math.Round(gGross - gDisc, 2);
                        cmdH.Parameters.AddWithValue("@Tax", gTax.ToString("0.00"));
                        cmdH.Parameters.Add("@Net", SqlDbType.Decimal).Value = gNet;

                        cmdH.Parameters.AddWithValue("@Sl", slNo.ToString());
                        cmdH.Parameters.Add("@Frt", SqlDbType.Decimal).Value = frt;
                        cmdH.Parameters.Add("@Oth", SqlDbType.Decimal).Value = oth;
                        cmdH.Parameters.AddWithValue("@Intra", rbTaxType.SelectedValue == "1" ? "YES" : (object)DBNull.Value);
                        cmdH.Parameters.AddWithValue("@Inter", rbTaxType.SelectedValue == "0" ? "YES" : (object)DBNull.Value);
                        cmdH.Parameters.AddWithValue("@User", uid); //PServiceName, addressfor
                        cmdH.Parameters.AddWithValue("@primarycat", cmbCategory.SelectedItem.Text.ToString());
                        cmdH.Parameters.AddWithValue("@addressfor", "Corporate office");
                        cmdH.ExecuteNonQuery();

                        // 2. Details & Stock Update
                        foreach (DataRow dr in dt.Rows)
                        {
                            decimal q = Convert.ToDecimal(dr["IQuantity"]);
                            decimal r = Convert.ToDecimal(dr["Sail_Rate"]);
                            decimal dPer = Convert.ToDecimal(dr["Discount_Rate"]);
                            decimal tPer = Convert.ToDecimal(dr["Tax_Rate"]);

                            decimal rowGross = Math.Round(q * r, 2);
                            decimal rowDisc = Math.Round((rowGross * dPer) / 100, 2);
                            decimal taxable = Math.Round(rowGross - rowDisc, 2);
                            decimal rowTax = Math.Round((taxable * tPer) / 100, 2);
                            decimal rowNet = Math.Round(taxable + rowTax, 2);

                            string sqlD = "INSERT INTO tbl_Invoice_details (Invoice_No, Quotation_no, Product_id, Product_Code, Product_name, Quantity, sail_rate, discountRate, Service_tax_rate, Total_sail_rate1, Total_sail_rate2, specification, AddedById) VALUES (@Inv, @PO, @PID, @HSN, @Name, @Qty, @Rate, @DPer, @TPer, @Net, @Base, @Brand, @User)";
                            SqlCommand cmdD = new SqlCommand(sqlD, conn, tran);
                            cmdD.Parameters.AddWithValue("@Inv", invNo);
                            cmdD.Parameters.AddWithValue("@PO", txtPONo.Text.Trim());
                            cmdD.Parameters.AddWithValue("@PID", dr["ProductID"]);
                            cmdD.Parameters.AddWithValue("@HSN", dr["Product_code"]);
                            cmdD.Parameters.AddWithValue("@Name", dr["ProductName"]);

                            //cmdD.Parameters.Add("@Qty", SqlDbType.Decimal).Value = q;
                            cmdD.Parameters.AddWithValue("@Qty", q.ToString("0.####")); // Preserves up to 4 decimals as string
                            cmdD.Parameters.Add("@Rate", SqlDbType.Decimal).Value = r;
                            cmdD.Parameters.Add("@DPer", SqlDbType.Decimal).Value = dPer;
                            //cmdD.Parameters.Add("@TPer", SqlDbType.Decimal).Value = tPer;                           
                            cmdD.Parameters.AddWithValue("@TPer", tPer.ToString("0.00")); // Pass as string
                            cmdD.Parameters.Add("@Net", SqlDbType.Decimal).Value = rowNet;
                            cmdD.Parameters.Add("@Base", SqlDbType.Decimal).Value = taxable;

                            cmdD.Parameters.AddWithValue("@Brand", dr["Brand"]);
                            cmdD.Parameters.AddWithValue("@User", uid);
                            cmdD.ExecuteNonQuery();

                            // Stock Deduct
                            string sqlStock = @"UPDATE tbl_NewProduct 
                                SET Quantity = CAST((CAST(ISNULL(Quantity, '0') AS DECIMAL(18,4)) - @Qty) AS NVARCHAR(100)), 
                                    Quantity_Num = ISNULL(Quantity_Num, 0) - @Qty 
                                WHERE ProductID = @PID";

                            SqlCommand cmdS = new SqlCommand(sqlStock, conn, tran);

                            // Safe decimal parameter with precision mapping to your database
                            SqlParameter paramQty = new SqlParameter("@Qty", SqlDbType.Decimal);
                            paramQty.Precision = 18;
                            paramQty.Scale = 4; // Matches the database decimal(18,4)
                            paramQty.Value = q;
                            cmdS.Parameters.Add(paramQty);

                            cmdS.Parameters.Add("@PID", SqlDbType.VarChar).Value = dr["ProductID"].ToString();
                            cmdS.ExecuteNonQuery();
                        }

                        // 3. Address
                        foreach (ListItem itm in lstAddresses.Items)
                        {
                            if (itm.Selected)
                            {
                                SqlCommand cmdA = new SqlCommand("INSERT INTO tbl_InvSiteAddress (invoice_no, SiteAddress) VALUES (@Inv, @Addr)", conn, tran);
                                cmdA.Parameters.AddWithValue("@Inv", invNo);
                                cmdA.Parameters.AddWithValue("@Addr", itm.Text);
                                cmdA.ExecuteNonQuery();
                            }
                        }

                        tran.Commit();
                        ShowMsg("Success! Invoice Generated: " + invNo, true);
                        WriteLog("Invoice: " + invNo);

                        ViewState["PhaseProductData"] = CreateCartTable();
                        mvInvoice.ActiveViewIndex = 0;
                        txtPONo.Text = ""; txtERPRef.Text = "";
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw; // FIX: Use 'throw;' instead of 'throw ex;' to preserve the exact line number of the error
                    }
                }
            }
            catch (Exception ex)
            {
                // FIX: Log the actual technical error to your file
                WriteLog("Invoice Save Error: " + ex.ToString());

                // FIX: Show a soft, non-technical message to the user
                // 2. Show a soft, friendly message to the user
                ShowMsg("Oops! We encountered a slight issue while generating the invoice. Please try again or contact support if the issue persists.", false);
            }
        }
        #endregion

        #region HELPERS
        private void ShowMsg(string msg, bool ok)
        {
            PanelMsg.Visible = true;
            lblMsg.Text = msg;
            // Soften the colors slightly for better UI/UX
            PanelMsg.Style["border-color"] = ok ? "#d6e9c6" : "#ebccd1";
            PanelMsg.Style["background-color"] = ok ? "#dff0d8" : "#f2dede";
            lblMsg.ForeColor = ok ? System.Drawing.Color.DarkGreen : System.Drawing.Color.DarkRed;
        }

        private string GenerateInvoiceNo()
        {
            DateTime dt = DateTime.Parse(txtInvoiceDate.Text);
            string yy = "";
            if (dt.Month >= 4) yy = dt.Year.ToString().Substring(2) + "-" + (dt.Year + 1).ToString().Substring(2);
            else yy = (dt.Year - 1).ToString().Substring(2) + "-" + dt.Year.ToString().Substring(2);
            return "INV/C/" + yy + "/" + GetSlNo();
        }

        private int GetSlNo()
        {
            try
            {
                // TRY_CAST safely ignores text values (like "INV-ABC") and only finds the max of actual numbers
                string query = "SELECT ISNULL(MAX(TRY_CAST(Sl_no AS INT)), 0) + 1 FROM tbl_Invoice";
                DataTable dt = DbCL.ReturnDataTable(query);

                if (dt.Rows.Count > 0 && dt.Rows[0][0] != DBNull.Value)
                {
                    return Convert.ToInt32(dt.Rows[0][0]);
                }
            }
            catch (Exception ex)
            {
                // Log the error silently and let the system fallback to 1
                WriteLog("Error generating Serial Number (GetSlNo): " + ex.ToString());
            }

            return 1; // Fallback serial number
        }

        private void WriteLog(string txt)
        {
            try
            {
                string p = Server.MapPath("~/Uploads/InvoiceLogs/Log.txt");
                if (!Directory.Exists(Path.GetDirectoryName(p))) Directory.CreateDirectory(Path.GetDirectoryName(p));
                File.AppendAllText(p, DateTime.Now + ": " + txt + Environment.NewLine);
            }
            catch { }
        }
        #endregion
    }
}