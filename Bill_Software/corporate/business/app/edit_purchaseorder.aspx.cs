using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;

namespace Bill_Software.corporate.business.app
{
    public partial class edit_purchaseorder : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();

        DataTable dtphasetype = new DataTable();
        DataTable dtPhasefees = new DataTable();
        DataTable dtPCat1 = new DataTable();
        DataTable dtPservice = new DataTable();
        DataTable dtproductWithCat = new DataTable();
        DataTable dtPCat = new DataTable();
        DataTable dtpro = new DataTable();

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
                bindphaseType();

                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            }
        }

        // =========================================================================
        // INITIALIZATION & BINDING
        // =========================================================================
        private void BindDropdowns()
        {
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 1. ISOLATED RESOURCE: Sales Persons
                string salesQuery = "SELECT Id, (Name + ' [' + User_Id + ']') AS DisplayName FROM tbl_login WHERE IsActive = 1 AND CompanyID = @CompanyID ORDER BY Name";
                using (SqlCommand cmd = new SqlCommand(salesQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dtSales = new DataTable();
                        da.Fill(dtSales);

                        cmbSalesPerson.DataSource = dtSales;
                        cmbSalesPerson.DataTextField = "DisplayName";
                        cmbSalesPerson.DataValueField = "Id";
                        cmbSalesPerson.DataBind();
                        cmbSalesPerson.Items.Insert(0, new ListItem("-- Select Sales Person --", "0"));
                    }
                }

                // 2. ISOLATED RESOURCE: Clients
                string clientQuery = "SELECT Client_Id, Client_Name FROM tbl_Client WHERE CompanyID = @CompanyID ORDER BY Client_Name";
                using (SqlCommand cmd = new SqlCommand(clientQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dtClients = new DataTable();
                        da.Fill(dtClients);

                        cmbvendor.DataSource = dtClients;
                        cmbvendor.DataTextField = "Client_Name";
                        cmbvendor.DataValueField = "Client_Name";
                        cmbvendor.DataBind();
                        cmbvendor.Items.Insert(0, new ListItem("--Select Client to Search--", "0"));

                        cmbClient.DataSource = dtClients;
                        cmbClient.DataTextField = "Client_Name";
                        cmbClient.DataValueField = "Client_Id";
                        cmbClient.DataBind();
                        cmbClient.Items.Insert(0, new ListItem("--Select Client--", "0"));
                    }
                }
            }

            DbCL.FillCombo(ddlPlaceOfSupply, "Select City_Name from tbl_City order by City_Name asc");
        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string clientId = GetClientId(cmbvendor.Text);

            string cmdstring = @"
            SELECT s.PServiceName, q.ID, q.service_tax1, q.sub_total, q.Quotation_no, 
                   q.Quotation_date, q.Gross, q.Service_tax, q.Net_amount, q.mailStatusDate, 
                   c.Client_Name, q.RecordType 
            FROM tbl_Quotation q
            LEFT OUTER JOIN tbl_Client c ON q.Client_Id = c.Client_Id 
            OUTER APPLY (
                SELECT TOP 1 PServiceName FROM tbl_QuoPriSerTogather 
                WHERE qutno = q.Quotation_no AND CompanyID = q.CompanyID ORDER BY TimeStamp DESC
            ) s
            WHERE q.CompanyID = @CompanyID AND q.RecordType = 'Purchase Order'";

            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

            if (RadioButtonList1.SelectedIndex == 0)
            {
                cmdstring += " AND q.Client_Id = @ClientId ";
                cmd.Parameters.AddWithValue("@ClientId", clientId);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                cmdstring += " AND CAST(q.Quotation_date AS DATETIME) BETWEEN @FromDate AND @ToDate ";
                cmd.Parameters.AddWithValue("@FromDate", txtfromDate.Text);
                cmd.Parameters.AddWithValue("@ToDate", txttodate.Text);
            }
            else
            {
                cmdstring += " AND q.Client_Id = @ClientId AND CAST(q.Quotation_date AS DATETIME) BETWEEN @FromDate AND @ToDate ";
                cmd.Parameters.AddWithValue("@ClientId", clientId);
                cmd.Parameters.AddWithValue("@FromDate", txtfromDate.Text);
                cmd.Parameters.AddWithValue("@ToDate", txttodate.Text);
            }

            cmdstring += " ORDER BY q.ID DESC";
            cmd.CommandText = cmdstring;

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
                    PanelError.Visible = false;
                }
                else
                {
                    DataList1.DataSource = null;
                    DataList1.DataBind();
                    PanelError.Visible = true;
                    lblErrorMsg.Text = "No Data Found...";
                }
            }
            DbCL.Conn.Close();
            cmd.Dispose();
        }

        private string GetClientId(string clientName)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT Client_Id FROM tbl_Client WHERE Client_Name=@name AND CompanyID=@CompanyID", con))
            {
                cmd.Parameters.AddWithValue("@name", clientName);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                con.Open();
                object res = cmd.ExecuteScalar();
                return res != null ? res.ToString() : "";
            }
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/edit_purchaseorder.aspx");
        }

        // =========================================================================
        // DATA LOADING FOR EDIT
        // =========================================================================
        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Quotation_no = Convert.ToString(e.CommandArgument);
            lbl_recordno.Text = Quotation_no;

            if (e.CommandName == "Select")
            {
                lblqno.Text = Quotation_no.ToString();

                string cmdstring = @"select Product_id as ProductID, Product_Code as Product_code, Product_name as ProductName, Type, 
                                     sail_rate as Sail_Rate, Service_tax_rate as Tax_Rate, Unit, Quantity, ProductOrServiceCat, 
                                     specification as Brand, Misc as specification, ItemNo, MaterialNo, PackSize, ItemRemarks, 
                                     discount_rate, Sl_no, DeliveryDate, Department 
                                     from tbl_Quotaion_details 
                                     where Quotation_no=@Quotation_no AND CompanyID=@CompanyID AND IsLatest = 1 AND IsDeleted = 0 
                                     order by CAST(Sl_no as int)";

                SqlParameter[] pram = {
                    new SqlParameter("@Quotation_no", Quotation_no),
                    new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID)
                };

                dtpro = DbCL.SPreturn_dt(cmdstring, pram);

                if (dtpro.Rows.Count > 0)
                {
                    DataTable dtPCatTemp = new DataTable();
                    dtPCatTemp.Columns.Add("ProductId"); dtPCatTemp.Columns.Add("Product_code");
                    dtPCatTemp.Columns.Add("ProductName"); dtPCatTemp.Columns.Add("Specification");
                    dtPCatTemp.Columns.Add("Sail_Rate"); dtPCatTemp.Columns.Add("Tax_Rate");
                    dtPCatTemp.Columns.Add("Quantity"); dtPCatTemp.Columns.Add("Brand");
                    dtPCatTemp.Columns.Add("Type"); dtPCatTemp.Columns.Add("Unit");
                    dtPCatTemp.Columns.Add("ProductOrServiceCat"); dtPCatTemp.Columns.Add("ItemNo");
                    dtPCatTemp.Columns.Add("MaterialNo"); dtPCatTemp.Columns.Add("PackSize");
                    dtPCatTemp.Columns.Add("ItemRemarks"); dtPCatTemp.Columns.Add("Sl_no");
                    dtPCatTemp.Columns.Add("discount_rate"); dtPCatTemp.Columns.Add("DeliveryDate");
                    dtPCatTemp.Columns.Add("Department");

                    for (int i = 0; i < dtpro.Rows.Count; i++)
                    {
                        DataRow dr = dtPCatTemp.NewRow();
                        dr["ProductId"] = dtpro.Rows[i]["ProductID"].ToString();
                        dr["Product_code"] = dtpro.Rows[i]["Product_code"].ToString();
                        dr["ProductName"] = dtpro.Rows[i]["ProductName"].ToString();
                        dr["Brand"] = dtpro.Rows[i]["Brand"].ToString();
                        dr["Specification"] = dtpro.Rows[i]["specification"].ToString();
                        dr["Quantity"] = dtpro.Rows[i]["Quantity"].ToString();
                        dr["Sail_Rate"] = dtpro.Rows[i]["Sail_Rate"].ToString();
                        dr["Tax_Rate"] = dtpro.Rows[i]["Tax_Rate"].ToString();
                        dr["Type"] = dtpro.Rows[i]["Type"].ToString();
                        dr["Unit"] = dtpro.Rows[i]["Unit"].ToString();
                        dr["ProductOrServiceCat"] = dtpro.Rows[i]["ProductOrServiceCat"].ToString();
                        dr["ItemNo"] = dtpro.Rows[i]["ItemNo"].ToString();
                        dr["MaterialNo"] = dtpro.Rows[i]["MaterialNo"].ToString();
                        dr["PackSize"] = dtpro.Rows[i]["PackSize"].ToString();
                        dr["ItemRemarks"] = dtpro.Rows[i]["ItemRemarks"].ToString();
                        dr["Sl_no"] = dtpro.Rows[i]["Sl_no"].ToString();
                        dr["discount_rate"] = dtpro.Rows[i]["discount_rate"].ToString();
                        dr["DeliveryDate"] = dtpro.Rows[i]["DeliveryDate"].ToString();
                        dr["Department"] = dtpro.Rows[i]["Department"].ToString();
                        dtPCatTemp.Rows.Add(dr);
                    }

                    ViewState["POCart"] = dtPCatTemp;
                    gd_Service_Product.DataSource = dtPCatTemp;
                    gd_Service_Product.DataBind();
                }

                Bindcombo();
                BindQuotationDetails(Quotation_no);
                BindPaymentPhases(Quotation_no);
                LoadPrimaryServices(Quotation_no);
                ToggleGridColumns();

                mvPOWizard.ActiveViewIndex = 1;
            }
        }

        protected void BindQuotationDetails(string quotationNo)
        {
            Quotation q = GetQuotationByNo(quotationNo);
            if (q != null)
            {
                DateTime tempDate;
                if (DateTime.TryParseExact(q.QuotationDate, "dd-MMM-yyyy", null, System.Globalization.DateTimeStyles.None, out tempDate))
                    txtquotationDate.Text = tempDate.ToString("dd-MMM-yyyy");
                else
                    txtquotationDate.Text = q.QuotationDate;

                txt_valdays.Text = q.ValidityDays.ToString();

                SetDropdownValue(DDL_ItemViewType, q.DetailedView);
                SetDropdownValue(DDL_DiscountView, q.DiscountView);
                SetDropdownValue(ddlPlaceOfSupply, q.PlaceOfSupply);

                if (cmbClient.Items.FindByValue(q.ClientId) != null) cmbClient.SelectedValue = q.ClientId;

                if (q.SalesPersonId.HasValue && cmbSalesPerson.Items.FindByValue(q.SalesPersonId.Value.ToString()) != null)
                {
                    cmbSalesPerson.SelectedValue = q.SalesPersonId.Value.ToString();
                }

                string delTenure = q.DeliveryTenure;
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

                string pkgCharges = q.PackingCharges;
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

                txt_remarks.Text = q.Remarks;
                txt_tcs_amnt.Text = q.TCS_Amount != null ? Convert.ToDecimal(q.TCS_Amount).ToString("0.00") : "0.00";
                txt_tcs_percent.Text = q.TCS_Percent != null ? Convert.ToDecimal(q.TCS_Percent).ToString("0.00") : "0.00";
                txt_delivery_amnt.Text = q.Freight_Amount != null ? Convert.ToDecimal(q.Freight_Amount).ToString("0.00") : "0.00";
                txt_freight_percent.Text = q.Freight_VAT_Percent != null ? Convert.ToDecimal(q.Freight_VAT_Percent).ToString("0.00") : "0.00";
                TextBox1.Text = q.OtherCharge_Name;
                txt_othr_amnt.Text = q.OtherCharge_Amount != null ? Convert.ToDecimal(q.OtherCharge_Amount).ToString("0.00") : "0.00";

                if (q.RecordType == "Quotation")
                {
                    PO_DataInputs.Visible = false;
                    rbQt.Checked = true; rbPo.Checked = false;
                    txb_ponumber.Text = ""; txb_donumber.Text = ""; txb_podate.Text = ""; txb_strtdt.Text = ""; txb_enddt.Text = "";
                }
                else
                {
                    txb_ponumber.Text = q.PO_Number; txb_donumber.Text = q.DO_Number;
                    txb_podate.Text = FormatDate(q.PO_Date);
                    txb_strtdt.Text = FormatDate(q.ValidityStartDate);
                    txb_enddt.Text = FormatDate(q.ValidityEndDate);
                    rbQt.Checked = false; rbPo.Checked = true;
                    PO_DataInputs.Visible = true;
                }

                if (q.ReferenceData == "Yes")
                {
                    rbYes.Checked = true; rbNo.Checked = false;
                    txt_clientrefname.Text = q.ReferenceName;
                    txt_clientrefid.Text = q.ReferenceId;
                    txt_clientrefdate.Text = FormatDate(q.ReferenceDate);
                }
                else
                {
                    rbYes.Checked = false; rbNo.Checked = true;
                    txt_clientrefname.Text = "N/A"; txt_clientrefid.Text = "N/A"; txt_clientrefdate.Text = "01-Jan-2000";
                }

                RadioButtonGst.SelectedValue = q.CGSTorSGST.ToString() == "Yes" ? "1" : "0";
            }
        }

        protected void cmbClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbClient.SelectedValue == "0" || string.IsNullOrEmpty(cmbClient.SelectedValue))
            {
                pnlClientPreview.Visible = false;
                return;
            }

            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT Client_Id, Client_Name, Address1, City, pin, State, Service_tax_no, Pan_no, PlaceofSupply 
                                 FROM tbl_Client WHERE Client_Id = @ClientParam AND CompanyID = @CompanyID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientParam", cmbClient.SelectedValue);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            lblPreviewName.Text = dr["Client_Name"]?.ToString();
                            lblPreviewERPCode.Text = dr["Client_Id"]?.ToString();

                            List<string> addressParts = new List<string>();
                            if (dr["Address1"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["Address1"].ToString()))
                                addressParts.Add(dr["Address1"].ToString().Trim());
                            if (dr["City"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["City"].ToString()))
                                addressParts.Add(dr["City"].ToString().Trim());

                            string zipCode = dr["pin"] != DBNull.Value ? dr["pin"].ToString().Trim() : "";
                            string finalAddress = string.Join(", ", addressParts);
                            if (!string.IsNullOrWhiteSpace(zipCode)) finalAddress += " - " + zipCode;

                            lblPreviewAddress.Text = string.IsNullOrWhiteSpace(finalAddress) ? "Address not provided." : finalAddress;
                            lblPreviewState.Text = dr["State"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["State"].ToString()) ? dr["State"].ToString() : "N/A";
                            lblPreviewPOS.Text = dr["PlaceofSupply"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["PlaceofSupply"].ToString()) ? dr["PlaceofSupply"].ToString() : "N/A";
                            lblPreviewGST.Text = dr["Service_tax_no"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["Service_tax_no"].ToString()) ? dr["Service_tax_no"].ToString() : "N/A";
                            lblPreviewPAN.Text = dr["Pan_no"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["Pan_no"].ToString()) ? dr["Pan_no"].ToString() : "N/A";

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

        // =========================================================================
        // WIZARD NAVIGATION
        // =========================================================================

        private void SaveCartToViewState()
        {
            DataTable dt = ViewState["POCart"] as DataTable;
            if (dt == null) return;

            for (int i = 0; i < gd_Service_Product.Rows.Count; i++)
            {
                GridViewRow row = gd_Service_Product.Rows[i];
                if (row.RowType == DataControlRowType.DataRow)
                {
                    Func<string, string> GetGridValue = (controlId) =>
                    {
                        System.Web.UI.Control ctrl = row.FindControl(controlId);
                        if (ctrl is TextBox) return ((TextBox)ctrl).Text;
                        if (ctrl is Label) return ((Label)ctrl).Text;
                        return "";
                    };

                    dt.Rows[i]["Quantity"] = GetGridValue("Quantity");
                    dt.Rows[i]["Sail_Rate"] = GetGridValue("Sail_Rate");
                    dt.Rows[i]["discount_rate"] = GetGridValue("Discount_Rate");
                    dt.Rows[i]["Tax_Rate"] = GetGridValue("Tax_Rate");
                    dt.Rows[i]["Specification"] = GetGridValue("Specification");
                    dt.Rows[i]["ItemRemarks"] = GetGridValue("ItemRemarks");
                    dt.Rows[i]["Sl_no"] = GetGridValue("txtOrder");
                    dt.Rows[i]["Brand"] = GetGridValue("Brand");
                    dt.Rows[i]["PackSize"] = GetGridValue("PackSize");
                    dt.Rows[i]["ItemNo"] = GetGridValue("ItemNo");
                    dt.Rows[i]["MaterialNo"] = GetGridValue("MaterialNo");

                    if (dt.Columns.Contains("DeliveryDate"))
                    {
                        dt.Rows[i]["DeliveryDate"] = GetGridValue("DeliveryDate");
                        dt.Rows[i]["Department"] = GetGridValue("Department");
                    }
                }
            }
            ViewState["POCart"] = dt;
        }

        private void ResequenceCart(DataTable dt)
        {
            if (dt == null) return;
            string orderCol = dt.Columns.Contains("Sl_no") ? "Sl_no" : (dt.Columns.Contains("ItemOrder") ? "ItemOrder" : null);
            if (orderCol == null) return;
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i][orderCol] = (i + 1).ToString();
        }

        protected void gd_Service_Product_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            SaveCartToViewState();
            DataTable dt = (DataTable)ViewState["POCart"];
            if (dt == null) return;
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
            else if (e.CommandName == "Remove")
            {
                dt.Rows.RemoveAt(index);
            }
            else return;

            ResequenceCart(dt);
            ViewState["POCart"] = dt;
            gd_Service_Product.DataSource = dt;
            gd_Service_Product.DataBind();
            ToggleGridColumns();
        }

        // =========================================================================
        // CORRECTED WIZARD NAVIGATION & CATALOG LOGIC
        // =========================================================================

        protected void btnNext1_Click(object sender, EventArgs e)
        {
            PanelError.Visible = false;
            mvPOWizard.ActiveViewIndex = 2; // Move to Catalog
        }

        protected void btnPrev2_Click(object sender, EventArgs e) { mvPOWizard.ActiveViewIndex = 1; }

        protected void btnSkipCatalog_Click(object sender, EventArgs e)
        {
            DataTable dt = ViewState["POCart"] as DataTable;
            if (dt != null)
            {
                gd_Service_Product.DataSource = dt;
                gd_Service_Product.DataBind();
                ToggleGridColumns();
            }
            mvPOWizard.ActiveViewIndex = 3;
        }

        protected void btnAddProduct_Click(object sender, EventArgs e)
        {
            PanelError.Visible = false;
            DataTable dtCatalog = ViewState["dtprocat"] as DataTable;
            if (dtCatalog == null) return;

            // 1. Initialize Cart structure if empty
            if (ViewState["POCart"] == null)
            {
                dtPCat = new DataTable();
                dtPCat.Columns.Add("ProductId"); dtPCat.Columns.Add("Product_code");
                dtPCat.Columns.Add("ProductName"); dtPCat.Columns.Add("Specification");
                dtPCat.Columns.Add("Sail_Rate"); dtPCat.Columns.Add("Tax_Rate");
                dtPCat.Columns.Add("Quantity"); dtPCat.Columns.Add("Brand");
                dtPCat.Columns.Add("Type"); dtPCat.Columns.Add("Unit");
                dtPCat.Columns.Add("ProductOrServiceCat"); dtPCat.Columns.Add("ItemNo");
                dtPCat.Columns.Add("MaterialNo"); dtPCat.Columns.Add("PackSize");
                dtPCat.Columns.Add("ItemRemarks"); dtPCat.Columns.Add("Sl_no");
                dtPCat.Columns.Add("discount_rate"); dtPCat.Columns.Add("DeliveryDate");
                dtPCat.Columns.Add("Department");
            }
            else
            {
                dtPCat = (DataTable)ViewState["POCart"];
            }

            bool itemsAdded = false; // Declaration fixed!

            // 2. Loop through the Catalog grid to find checked items
            for (int i = 0; i < gridProdWithCat.Rows.Count; i++)
            {
                CheckBox chk = (CheckBox)gridProdWithCat.Rows[i].FindControl("chkdtp");
                if (chk != null && chk.Checked)
                {
                    DataRow dr = dtPCat.NewRow();

                    // Map from Catalog Grid to Cart DataRow
                    dr["ProductId"] = ((Label)gridProdWithCat.Rows[i].FindControl("ProductID"))?.Text;
                    dr["Product_code"] = ((Label)gridProdWithCat.Rows[i].FindControl("Product_code"))?.Text;
                    dr["ProductName"] = ((Label)gridProdWithCat.Rows[i].FindControl("ProductName"))?.Text;
                    dr["Brand"] = ((Label)gridProdWithCat.Rows[i].FindControl("Brand"))?.Text;
                    dr["Specification"] = ((TextBox)gridProdWithCat.Rows[i].FindControl("Specification"))?.Text;
                    dr["Quantity"] = ((TextBox)gridProdWithCat.Rows[i].FindControl("Quantity"))?.Text;
                    dr["Sail_Rate"] = ((TextBox)gridProdWithCat.Rows[i].FindControl("Sail_Rate"))?.Text;
                    dr["Tax_Rate"] = ((Label)gridProdWithCat.Rows[i].FindControl("Tax_Rate"))?.Text;
                    dr["Unit"] = ((Label)gridProdWithCat.Rows[i].FindControl("Unit"))?.Text;
                    dr["ProductOrServiceCat"] = ((Label)gridProdWithCat.Rows[i].FindControl("ProductOrServiceCat"))?.Text;

                    // Defaults for new items
                    dr["discount_rate"] = "0";
                    dr["ItemNo"] = ""; dr["MaterialNo"] = ""; dr["PackSize"] = "";
                    dr["ItemRemarks"] = ""; dr["Sl_no"] = (dtPCat.Rows.Count + 1).ToString();
                    dr["DeliveryDate"] = ""; dr["Department"] = "";

                    dtPCat.Rows.Add(dr);
                    itemsAdded = true;
                }
            }

            if (itemsAdded)
            {
                ViewState["POCart"] = dtPCat;
                gd_Service_Product.DataSource = dtPCat;
                gd_Service_Product.DataBind();

                TakePservice(cmbproduct_service.Text);
                ToggleGridColumns();
                PanelError.Visible = false;
                lblOk.Text = "Selected products added to cart.";
                PanelOK.Visible = true;
            }
            else
            {
                lblErrorMsg.Text = "Please select at least one item to add.";
                PanelError.Visible = true;
            }
        }

        protected void btnAddMoreProducts_Click(object sender, EventArgs e)
        {
            SaveCartToViewState();
            mvPOWizard.ActiveViewIndex = 2; // Go back to Catalog
        }

        protected void btnPrev3_Click(object sender, EventArgs e)
        {
            SaveCartToViewState();
            mvPOWizard.ActiveViewIndex = 2;
        }

        protected void btnNext3_Click(object sender, EventArgs e)
        {
            PanelError.Visible = false;
            SaveCartToViewState();
            DataTable currentCart = (DataTable)ViewState["POCart"];

            if (currentCart == null || currentCart.Rows.Count == 0)
            {
                lblErrorMsg.Text = "Cart is empty!";
                PanelError.Visible = true;
                return;
            }

            decimal gross = 0m, tax = 0m;
            foreach (DataRow dr in currentCart.Rows)
            {
                decimal qty = ParseDecimal(dr["Quantity"]?.ToString());
                decimal rate = ParseDecimal(dr["Sail_Rate"]?.ToString());
                decimal disc = ParseDecimal(dr["discount_rate"]?.ToString());
                decimal taxPct = ParseDecimal(dr["Tax_Rate"]?.ToString());
                decimal taxable = qty * (rate - (rate * disc / 100m));
                tax += (taxable * taxPct) / 100m;
                gross += taxable + ((taxable * taxPct) / 100m);
            }
            lblGrossAmt.Text = Math.Round(gross, 2).ToString("0.00");
            lblTaxAmt.Text = Math.Round(tax, 2).ToString("0.00");

            bindphaseType(lblqno.Text);
            mvPOWizard.ActiveViewIndex = 4;
        }

        protected void btnPrev4_Click(object sender, EventArgs e) { mvPOWizard.ActiveViewIndex = 3; }

        // Fixed ID return for older C# versions
        private int idreturn_New(string prefix)
        {
            int lastNumber = 0;
            string query = "SELECT TOP 1 Quotation_no FROM tbl_Quotation WHERE Quotation_no LIKE @Prefix + '%' AND CompanyID = @CompanyID ORDER BY Id DESC";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Prefix", prefix);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                con.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    string qNo = result.ToString().Trim();
                    string[] parts = qNo.Split('/');
                    int pNum = 0; // Declare outside for C# 5.0
                    if (parts.Length >= 4 && int.TryParse(parts[parts.Length - 1], out pNum))
                    {
                        lastNumber = pNum;
                    }
                }
            }
            return lastNumber;
        }

        // =========================================================================
        // CATALOG & CART MANAGEMENT
        // =========================================================================
        private void Bindcombo()
        {
            string cmdstring = "select ProductOrServiceCat from tbl_NewparentProduct WHERE CompanyID=@CompanyID order by ProductOrServiceCat asc";
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

        protected void Button2_Click(object sender, EventArgs e)
        {
            gridProdWithCat.Visible = true;
            string cmdstring = "select Id, Product_code, ProductID,ProductOrServiceCat,Brand, ProductName,Specification,Type,Sail_Rate,Tax_Rate,Unit from tbl_NewProduct where ProductOrServiceCat=@ProductOrServiceCat AND CompanyID=@CompanyID order by Id,ProductName";
            SqlParameter[] pram = {
                new SqlParameter("@ProductOrServiceCat", cmbproduct_service.Text),
                new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID)
            };
            dtproductWithCat = DbCL.SPreturn_dt(cmdstring, pram);
            if (dtproductWithCat.Rows.Count > 0)
            {
                gridProdWithCat.DataSource = dtproductWithCat;
                gridProdWithCat.DataBind();
                ViewState["dtprocat"] = dtproductWithCat;
            }
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
            foreach (DataRow row in dtPCat1.Rows)
            {
                if (row["ProductCatagory"].ToString() == pservice)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                DataRow dr = dtPCat1.NewRow();
                dr["ProductCatagory"] = pservice;
                dtPCat1.Rows.Add(dr);
            }

            gridps.DataSource = dtPCat1;
            gridps.DataBind();
            ViewState["pService"] = dtPCat1;
        }

        protected void ToggleGridColumns()
        {
            if (gd_Service_Product.Columns.Count > 13)
            {
                bool isPOChecked = rbPo.Checked;
                gd_Service_Product.Columns[13].Visible = isPOChecked; // Delivery Date
                gd_Service_Product.Columns[14].Visible = isPOChecked; // Department
            }
        }

        // =========================================================================
        // TERMS & PHASES
        // =========================================================================

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
                        bool s = false;
                        foreach (DataRow r in dtPhasefees.Rows)
                        {
                            if (r["PaymentPhase"].ToString() == pt) s = true;
                        }
                        if (!s)
                        {
                            DataRow dr = dtPhasefees.NewRow();
                            dr[0] = pt; dr[1] = "";
                            dr[2] = (pt == "Full & Final Instalment" || pt == "Payment After Delivery" || pt == "100% Against PI") ? "100" : "";
                            dtPhasefees.Rows.Add(dr);
                        }
                    }
                    else
                    {
                        dtPhasefees = new DataTable();
                        dtPhasefees.Columns.Add("PaymentPhase");
                        dtPhasefees.Columns.Add("PhaseDesc");
                        dtPhasefees.Columns.Add("AmountPer");

                        DataRow dr = dtPhasefees.NewRow();
                        dr[0] = pt; dr[1] = "";
                        dr[2] = (pt == "Full & Final Instalment" || pt == "Payment After Delivery" || pt == "100% Against PI") ? "100" : "";
                        dtPhasefees.Rows.Add(dr);
                    }
                }
            }
            GridView3.DataSource = dtPhasefees;
            GridView3.DataBind();
            ViewState["phaseAmountData"] = dtPhasefees;
        }

        protected void AmountPer_TextChanged(object sender, EventArgs e)
        {
            amountCalculation();
        }

        public void amountCalculation()
        {
            double total = 0;
            foreach (GridViewRow gvr in GridView3.Rows)
            {
                string PaymentPhase = ((Label)gvr.Cells[1].FindControl("PaymentPhase")).Text;
                if (PaymentPhase != "Full & Final Instalment")
                {
                    TextBox tb = (TextBox)gvr.Cells[0].FindControl("AmountPer");

                    double parsedSum;
                    if (double.TryParse(tb.Text.Trim(), out parsedSum))
                    {
                        total += parsedSum;
                    }
                }
                else
                {
                    TextBox tb = (TextBox)gvr.Cells[0].FindControl("AmountPer");
                    tb.Text = (100 - total).ToString();
                }
            }
        }

        protected void GridView3_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            if (ViewState["phaseAmountData"] != null)
            {
                DataTable d = (DataTable)ViewState["phaseAmountData"];
                d.Rows[e.RowIndex].Delete();
                ViewState["phaseAmountData"] = d.Rows.Count > 0 ? d : null;
                GridView3.DataSource = (DataTable)ViewState["phaseAmountData"];
                GridView3.DataBind();
            }
        }

        // =========================================================================
        // FINAL SAVE & UPDATE LOGIC
        // =========================================================================

        protected void btnSabe_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateBeforePersist()) return;
                DataUpdaterMethod();
            }
            catch (Exception ex) { ShowErrorAlert(ex); }
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateBeforePersist()) return;
                MagicianNew();
            }
            catch (Exception ex) { ShowErrorAlert(ex); }
        }

        private bool ValidateBeforePersist()
        {
            PanelError.Visible = false;
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
                return false;
            }
            if (cmbClient.SelectedValue == "0" || string.IsNullOrEmpty(cmbClient.SelectedValue))
            {
                lblErrorMsg.Text = "Please select a Client.";
                PanelError.Visible = true;
                return false;
            }
            if (cmbSalesPerson.SelectedValue == "0" || string.IsNullOrEmpty(cmbSalesPerson.SelectedValue))
            {
                lblErrorMsg.Text = "Please assign a Sales Person.";
                PanelError.Visible = true;
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtquotationDate.Text) || ddlPlaceOfSupply.SelectedIndex < 0)
            {
                lblErrorMsg.Text = "Document Date and Place of Supply are required.";
                PanelError.Visible = true;
                return false;
            }
            SaveCartToViewState();
            DataTable cart = ViewState["POCart"] as DataTable;
            if (cart == null || cart.Rows.Count == 0)
            {
                lblErrorMsg.Text = "Cart is empty.";
                PanelError.Visible = true;
                return false;
            }

            // Loaded PO is authoritative. Heal lblqno if a failed "Save as New Revision" left an unused number.
            if (GetQuotationByNo(lbl_recordno.Text) == null)
            {
                lblErrorMsg.Text = "Record not found for this company.";
                PanelError.Visible = true;
                return false;
            }
            if (GetQuotationByNo(lblqno.Text) == null)
                lblqno.Text = lbl_recordno.Text;

            return true;
        }

        private void ShowErrorAlert(Exception ex)
        {
            StringBuilder errorMsg = new StringBuilder();
            errorMsg.AppendLine("An error occurred: " + ex.Message);
            if (ex.InnerException != null) errorMsg.AppendLine("<br/>Inner: " + ex.InnerException.ToString());
            lblErrorMsg.Text = errorMsg.ToString();
            PanelError.Visible = true;
        }

        private void DataUpdaterMethod()
        {
            string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";
            string qno = lblqno.Text;
            int companyId = CompanyContext.CurrentCompanyID;

            string query = "select Status1, Status2, PaymentStatus from tbl_Quotation where Quotation_no=@Quotation_no AND CompanyID=@CompanyID";
            SqlParameter[] pram = {
                new SqlParameter("@Quotation_no", qno),
                new SqlParameter("@CompanyID", companyId)
            };

            DataTable dtProInvPay = DbCL.SPreturn_dt(query, pram);
            if (dtProInvPay.Rows.Count > 0)
            {
                string pro = dtProInvPay.Rows[0]["Status1"].ToString();
                string inv = dtProInvPay.Rows[0]["Status2"].ToString();
                string pay = dtProInvPay.Rows[0]["PaymentStatus"].ToString();

                if (pro == "Yes" || inv == "Yes" || pay == "Yes")
                {
                    lblErrorMsg.Text = "Cannot update. Please delete associated invoices/DO first.";
                    PanelError.Visible = true;
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
                    lockCmd.Parameters.Add(returnCode);
                    lockCmd.ExecuteNonQuery();

                    if ((int)returnCode.Value < 0) throw new Exception("Unable to acquire lock. Another user may be editing this document.");
                }

                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    string versionQuery = "SELECT ISNULL(MAX(Version), 0) + 1 FROM tbl_Quotaion_details WHERE Quotation_no = @Quotation_no AND CompanyID=@CompanyID";
                    int newVersion;
                    using (SqlCommand vCmd = new SqlCommand(versionQuery, conn, trans))
                    {
                        vCmd.Parameters.AddWithValue("@Quotation_no", qno);
                        vCmd.Parameters.AddWithValue("@CompanyID", companyId);
                        newVersion = Convert.ToInt32(vCmd.ExecuteScalar());
                    }

                    string softDeleteQuery = @"UPDATE tbl_Quotaion_details SET IsDeleted = 1, IsLatest = 0, DeletedById = @DeletedById, DeletedOn = GETDATE()
                                               WHERE Quotation_no = @Quotation_no AND CompanyID = @CompanyID AND IsDeleted = 0 AND IsLatest = 1";
                    using (SqlCommand sCmd = new SqlCommand(softDeleteQuery, conn, trans))
                    {
                        sCmd.Parameters.AddWithValue("@Quotation_no", qno);
                        sCmd.Parameters.AddWithValue("@CompanyID", companyId);
                        sCmd.Parameters.AddWithValue("@DeletedById", userId);
                        sCmd.ExecuteNonQuery();
                    }

                    // Soft-delete then insert lines from ViewState POCart (preserve cart Sl_no)
                    DataTable dtCart = (DataTable)ViewState["POCart"];
                    if (dtCart != null)
                    {
                        foreach (DataRow drCart in dtCart.Rows)
                        {
                            string ProductId = drCart["ProductId"]?.ToString().Trim() ?? "";
                            string Product_code = drCart["Product_code"]?.ToString().Trim() ?? "";
                            string ProductName = drCart["ProductName"]?.ToString().Trim() ?? "";
                            string Brand = drCart["Brand"]?.ToString().Trim() ?? "";
                            string ProductOrServiceCat = drCart["ProductOrServiceCat"]?.ToString().Trim() ?? "";
                            string Type = drCart["Type"]?.ToString().Trim() ?? "";
                            string Unit = drCart["Unit"]?.ToString().Trim() ?? "";
                            string Specification = drCart["Specification"]?.ToString().Trim() ?? "~";
                            string ItemRemarks = drCart["ItemRemarks"]?.ToString().Trim() ?? "";
                            string ItemNo = drCart["ItemNo"]?.ToString().Trim() ?? "";
                            string MaterialNo = drCart["MaterialNo"]?.ToString().Trim() ?? "";
                            string PackSize = drCart["PackSize"]?.ToString().Trim() ?? "";
                            string DeliveryDate = drCart["DeliveryDate"]?.ToString().Trim() ?? "";
                            string Department = drCart["Department"]?.ToString().Trim() ?? "";
                            string h = drCart["Sl_no"]?.ToString().Trim() ?? "0";

                            decimal Quantity = ParseDecimal(drCart["Quantity"]?.ToString());
                            decimal Sail_Rate = ParseDecimal(drCart["Sail_Rate"]?.ToString());
                            decimal Tax_Rate = ParseDecimal(drCart["Tax_Rate"]?.ToString());
                            decimal disc = ParseDecimal(drCart["discount_rate"]?.ToString());

                            decimal discounted_rate = Sail_Rate - (Sail_Rate * disc / 100);
                            decimal taxMultiplier = (Tax_Rate + 100) / 100;
                            decimal Total_sail_rate = taxMultiplier * discounted_rate;
                            decimal Total_sail_rate1 = Total_sail_rate * Quantity;
                            decimal Total_sail_rate2 = discounted_rate * Quantity;
                            decimal Service_tax = (Tax_Rate * Quantity * discounted_rate) / 100;

                            new_sub_total += Total_sail_rate2;
                            new_total_Service += Service_tax;
                            new_Gross_amount = Math.Round(new_Gross_amount + Total_sail_rate1, 2);

                            using (SqlCommand cmd = new SqlCommand(@"INSERT INTO tbl_Quotaion_details 
                            (Sl_no, Quotation_no, Product_id, Product_Code, Product_name, Quantity, sail_rate, Service_tax_rate, discount_rate, new_sailrate,
                             Total_sail_rate, Total_sail_rate1, Total_sail_rate2, specification, Misc, InvStatus, Type, Unit, ProductOrServiceCat,
                             ItemRemarks, ItemNo, MaterialNo, PackSize, DeliveryDate, Department, AddedById, AddedOn, Version, IsDeleted, IsLatest, CompanyID)
                            VALUES 
                            (@Sl_no, @Quotation_no, @Product_id, @Product_Code, @Product_name, @Quantity, @sail_rate, @Service_tax_rate, @discount_rate, @new_sailrate,
                             @Total_sail_rate, @Total_sail_rate1, @Total_sail_rate2, @specification, @Misc, 'No', @Type, @Unit, @ProductOrServiceCat,
                             @ItemRemarks, @ItemNo, @MaterialNo, @PackSize, @DeliveryDate, @Department, @AddedById, GETDATE(), @Version, 0, 1, @CompanyID)", conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@Sl_no", h);
                                cmd.Parameters.AddWithValue("@Quotation_no", qno);
                                cmd.Parameters.AddWithValue("@Product_id", ProductId);
                                cmd.Parameters.AddWithValue("@Product_Code", Product_code);
                                cmd.Parameters.AddWithValue("@Product_name", ProductName);
                                cmd.Parameters.AddWithValue("@Quantity", Quantity);
                                cmd.Parameters.AddWithValue("@sail_rate", Sail_Rate);
                                cmd.Parameters.AddWithValue("@Service_tax_rate", Tax_Rate);
                                cmd.Parameters.AddWithValue("@discount_rate", disc);
                                cmd.Parameters.AddWithValue("@new_sailrate", discounted_rate);
                                cmd.Parameters.AddWithValue("@Total_sail_rate", Total_sail_rate);
                                cmd.Parameters.AddWithValue("@Total_sail_rate1", Total_sail_rate1);
                                cmd.Parameters.AddWithValue("@Total_sail_rate2", Total_sail_rate2);
                                cmd.Parameters.AddWithValue("@specification", Brand);
                                cmd.Parameters.AddWithValue("@Misc", Specification);
                                cmd.Parameters.AddWithValue("@Type", Type);
                                cmd.Parameters.AddWithValue("@Unit", Unit);
                                cmd.Parameters.AddWithValue("@ProductOrServiceCat", ProductOrServiceCat);
                                cmd.Parameters.AddWithValue("@ItemRemarks", ItemRemarks);
                                cmd.Parameters.AddWithValue("@ItemNo", ItemNo);
                                cmd.Parameters.AddWithValue("@MaterialNo", MaterialNo);
                                cmd.Parameters.AddWithValue("@PackSize", PackSize);
                                cmd.Parameters.AddWithValue("@DeliveryDate", DeliveryDate);
                                cmd.Parameters.AddWithValue("@Department", Department);
                                cmd.Parameters.AddWithValue("@AddedById", userId);
                                cmd.Parameters.AddWithValue("@Version", newVersion);
                                cmd.Parameters.AddWithValue("@CompanyID", companyId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    decimal tcsAmount = ParseDecimal(txt_tcs_amnt.Text);
                    decimal deliveryAmount = ParseDecimal(txt_delivery_amnt.Text);
                    decimal otherAmount = ParseDecimal(txt_othr_amnt.Text);
                    decimal finalNet = Math.Round(new_Gross_amount + tcsAmount + deliveryAmount + otherAmount, 2);

                    string itemview = DDL_ItemViewType.SelectedItem.Text?.Trim();
                    if (string.IsNullOrEmpty(itemview) || itemview == "--SELECT--")
                    {
                        Quotation existing = GetQuotationByNo(qno);
                        if (existing != null && !string.IsNullOrEmpty(existing.DetailedView))
                            itemview = existing.DetailedView;
                    }

                    string updateHeader = @"UPDATE tbl_Quotation SET Gross = @Gross, Service_tax = @STax, Net_amount = @Net, service_tax1 = @STax1, sub_total = @SubT, ModifiedById = @ModBy, ModifiedOn = GETDATE(), SalesPersonId = @SalesPersonId, DetailedView = @DView 
                                            WHERE Quotation_no = @QNo AND CompanyID = @CompanyID";

                    using (SqlCommand hCmd = new SqlCommand(updateHeader, conn, trans))
                    {
                        hCmd.Parameters.AddWithValue("@Gross", Math.Round(new_Gross_amount, 2));
                        hCmd.Parameters.AddWithValue("@STax", Math.Round(new_total_Service, 2));
                        hCmd.Parameters.AddWithValue("@Net", finalNet);
                        hCmd.Parameters.AddWithValue("@STax1", Math.Round(new_total_Service, 2));
                        hCmd.Parameters.AddWithValue("@SubT", new_sub_total);
                        hCmd.Parameters.AddWithValue("@ModBy", userId);
                        hCmd.Parameters.AddWithValue("@QNo", qno);
                        hCmd.Parameters.AddWithValue("@SalesPersonId", cmbSalesPerson.SelectedValue == "0" ? DBNull.Value : (object)cmbSalesPerson.SelectedValue);
                        hCmd.Parameters.AddWithValue("@DView", (object)itemview ?? DBNull.Value);
                        hCmd.Parameters.AddWithValue("@CompanyID", companyId);
                        hCmd.ExecuteNonQuery();
                    }

                    insertPaymentPhaseNew(qno, conn, trans);
                    insertprimaryServiceNew(qno, conn, trans);

                    InsertSystemNotification(trans, conn, "Document Updated", $"Document {qno} (Version {newVersion}) was successfully updated.", "PURCHASE", "INFO");

                    trans.Commit();
                    updatedueamountdetails((double)finalNet);

                    lblOk.Text = "Data Updated Successfully! Version: " + newVersion;
                    PanelOK.Visible = true;
                    btnSabe.Visible = btnNew.Visible = false;
                }
                catch (Exception ex)
                {
                    try { trans?.Rollback(); } catch { }
                    throw ex;
                }
            }
        }

        private void MagicianNew()
        {
            string oldRecordID = lbl_recordno.Text.Trim();
            Bindquotationno();
            string newRecordID = lblqno.Text;
            int slNo = idreturn() + 1;
            string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";
            int companyId = CompanyContext.CurrentCompanyID;

            decimal new_sub_total = 0, new_total_Service = 0, new_Gross_amount = 0;
            string cnnString = ConfigurationManager.ConnectionStrings["DbConn"].ToString();

            try
            {
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
                    lockCmd.Parameters.Add(returnCode);
                    lockCmd.ExecuteNonQuery();

                    if ((int)returnCode.Value < 0) throw new Exception("Unable to acquire lock. Another user may be processing this.");
                }

                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    string archiveOld = "UPDATE tbl_Quotation SET IsLatest = 0 WHERE Quotation_no = @OldQuote AND CompanyID = @CompanyID";
                    using (SqlCommand cmd = new SqlCommand(archiveOld, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@OldQuote", oldRecordID);
                        cmd.Parameters.AddWithValue("@CompanyID", companyId);
                        cmd.ExecuteNonQuery();
                    }

                    // Insert lines from ViewState POCart (preserve cart Sl_no)
                    DataTable dtCart = (DataTable)ViewState["POCart"];
                    if (dtCart != null)
                    {
                        foreach (DataRow drCart in dtCart.Rows)
                        {
                            string ProductId = drCart["ProductId"]?.ToString().Trim() ?? "";
                            string Product_code = drCart["Product_code"]?.ToString().Trim() ?? "";
                            string ProductName = drCart["ProductName"]?.ToString().Trim() ?? "";
                            string Brand = drCart["Brand"]?.ToString().Trim() ?? "";
                            string ProductOrServiceCat = drCart["ProductOrServiceCat"]?.ToString().Trim() ?? "";
                            string Type = drCart["Type"]?.ToString().Trim() ?? "";
                            string Unit = drCart["Unit"]?.ToString().Trim() ?? "";
                            string Specification = drCart["Specification"]?.ToString().Trim() ?? "~";
                            string ItemRemarks = drCart["ItemRemarks"]?.ToString().Trim() ?? "";
                            string ItemNo = drCart["ItemNo"]?.ToString().Trim() ?? "";
                            string MaterialNo = drCart["MaterialNo"]?.ToString().Trim() ?? "";
                            string PackSize = drCart["PackSize"]?.ToString().Trim() ?? "";
                            string DeliveryDate = drCart["DeliveryDate"]?.ToString().Trim() ?? "";
                            string Department = drCart["Department"]?.ToString().Trim() ?? "";
                            string h = drCart["Sl_no"]?.ToString().Trim() ?? "0";

                            decimal Quantity = ParseDecimal(drCart["Quantity"]?.ToString());
                            decimal Sail_Rate = ParseDecimal(drCart["Sail_Rate"]?.ToString());
                            decimal Tax_Rate = ParseDecimal(drCart["Tax_Rate"]?.ToString());
                            decimal disc = ParseDecimal(drCart["discount_rate"]?.ToString());

                            decimal discounted_rate = Sail_Rate - (Sail_Rate * disc / 100);
                            decimal taxMultiplier = (Tax_Rate + 100) / 100;
                            decimal Total_sail_rate = taxMultiplier * discounted_rate;
                            decimal Total_sail_rate1 = Total_sail_rate * Quantity;
                            decimal Total_sail_rate2 = discounted_rate * Quantity;
                            decimal Service_tax = (Tax_Rate * Quantity * discounted_rate) / 100;

                            new_sub_total += Total_sail_rate2;
                            new_total_Service += Service_tax;
                            new_Gross_amount = Math.Round(new_Gross_amount + Total_sail_rate1, 2);

                            using (SqlCommand cmd = new SqlCommand(@"INSERT INTO tbl_Quotaion_details 
                            (Sl_no, Quotation_no, Product_id, Product_Code, Product_name, Quantity, sail_rate, Service_tax_rate, discount_rate, new_sailrate, 
                             Total_sail_rate, Total_sail_rate1, Total_sail_rate2, specification, Misc, InvStatus, Type, Unit, ProductOrServiceCat, 
                             ItemRemarks, ItemNo, MaterialNo, PackSize, DeliveryDate, Department, AddedById, AddedOn, Version, IsDeleted, IsLatest, CompanyID) 
                            VALUES 
                            (@Sl_no, @Quotation_no, @Product_id, @Product_Code, @Product_name, @Quantity, @sail_rate, @Service_tax_rate, @discount_rate, @new_sailrate, 
                             @Total_sail_rate, @Total_sail_rate1, @Total_sail_rate2, @specification, @Misc, 'No', @Type, @Unit, @ProductOrServiceCat, 
                             @ItemRemarks, @ItemNo, @MaterialNo, @PackSize, @DeliveryDate, @Department, @AddedById, GETDATE(), 1, 0, 1, @CompanyID)", conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@Sl_no", h);
                                cmd.Parameters.AddWithValue("@Quotation_no", newRecordID);
                                cmd.Parameters.AddWithValue("@Product_id", ProductId);
                                cmd.Parameters.AddWithValue("@Product_Code", Product_code);
                                cmd.Parameters.AddWithValue("@Product_name", ProductName);
                                cmd.Parameters.AddWithValue("@Quantity", Quantity);
                                cmd.Parameters.AddWithValue("@sail_rate", Sail_Rate);
                                cmd.Parameters.AddWithValue("@Service_tax_rate", Tax_Rate);
                                cmd.Parameters.AddWithValue("@discount_rate", disc);
                                cmd.Parameters.AddWithValue("@new_sailrate", discounted_rate);
                                cmd.Parameters.AddWithValue("@Total_sail_rate", Total_sail_rate);
                                cmd.Parameters.AddWithValue("@Total_sail_rate1", Total_sail_rate1);
                                cmd.Parameters.AddWithValue("@Total_sail_rate2", Total_sail_rate2);
                                cmd.Parameters.AddWithValue("@specification", Brand);
                                cmd.Parameters.AddWithValue("@Misc", Specification);
                                cmd.Parameters.AddWithValue("@Type", Type);
                                cmd.Parameters.AddWithValue("@Unit", Unit);
                                cmd.Parameters.AddWithValue("@ProductOrServiceCat", ProductOrServiceCat);
                                cmd.Parameters.AddWithValue("@ItemRemarks", ItemRemarks);
                                cmd.Parameters.AddWithValue("@ItemNo", ItemNo);
                                cmd.Parameters.AddWithValue("@MaterialNo", MaterialNo);
                                cmd.Parameters.AddWithValue("@PackSize", PackSize);
                                cmd.Parameters.AddWithValue("@DeliveryDate", DeliveryDate);
                                cmd.Parameters.AddWithValue("@Department", Department);
                                cmd.Parameters.AddWithValue("@AddedById", userId);
                                cmd.Parameters.AddWithValue("@CompanyID", companyId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    int validDays = 0;
                    int.TryParse(txt_valdays.Text?.Trim(), out validDays);
                    string deliveryTenure = DDL_DeliveryTerms.SelectedValue == "4" ? txt_deltrms.Text?.Trim() : DDL_DeliveryTerms.SelectedItem.Text;
                    string packageForwarding = DDL_pkgfrwd.SelectedValue == "3" ? txt_pkgfrwd.Text?.Trim() : DDL_pkgfrwd.SelectedItem.Text;
                    string remarks = txt_remarks.Text?.Trim();
                    string itemview = DDL_ItemViewType.SelectedItem.Text?.Trim();
                    string referenceOption = rbYes.Checked ? "Yes" : "No";
                    string referenceName = referenceOption == "No" ? "N/A" : txt_clientrefname.Text?.Trim();
                    string referenceId = referenceOption == "No" ? "N/A" : txt_clientrefid.Text?.Trim();
                    string referenceDate = referenceOption == "No" ? "1900-01-01" : txt_clientrefdate.Text?.Trim();

                    string recordtyp = rbPo.Checked ? "Purchase Order" : "Quotation";
                    string DO_number = recordtyp == "Quotation" ? "N/A" : txb_donumber.Text?.Trim();
                    string PO_number = recordtyp == "Quotation" ? "N/A" : txb_ponumber.Text?.Trim();
                    string PO_Date = recordtyp == "Quotation" ? "1900-01-01" : txb_podate.Text?.Trim();
                    string ValStart_Date = recordtyp == "Quotation" ? "1900-01-01" : txb_strtdt.Text?.Trim();
                    string ValEnd_Date = recordtyp == "Quotation" ? "1900-01-01" : txb_enddt.Text?.Trim();

                    decimal tcsAmount = ParseDecimal(txt_tcs_amnt.Text);
                    decimal tcsPercent = ParseDecimal(txt_tcs_percent.Text);
                    decimal deliveryAmount = ParseDecimal(txt_delivery_amnt.Text);
                    decimal freightPercent = ParseDecimal(txt_freight_percent.Text);
                    decimal otherAmount = ParseDecimal(txt_othr_amnt.Text);
                    decimal finalNet = Math.Round(new_Gross_amount + tcsAmount + deliveryAmount + otherAmount, 2);

                    using (SqlCommand cmd = new SqlCommand(@"INSERT INTO tbl_Quotation 
                    (Quotation_no, Quotation_date, Client_Id, Gross, Service_tax, Net_amount, Status1, Status2, Sl_no, status3, service_tax1, sub_total, cgstOrsgst, igst, PlaceofSupply, PaymentStatus, ReferenceData, ReferenceName, ReferenceId, ReferenceDate, ValidityDays, DeliveryTenure, PackingCharges, Remarks, DetailedView, RecordType, DO_Number, PO_Number, PO_Date, Validity_StartDate, Validity_EndDate, AddedById, DiscountView, TCS_Amount, TCS_Percent, Freight_Amount, Freight_VAT_Percent, OtherCharge_Name, OtherCharge_Amount, IsLatest, Version, SalesPersonId, CompanyID)
                    VALUES (@QNo, @QDate, @CId, @Gross, @STax, @Net, 'No', 'No', @Sl, 'No', @STax1, @Sub, @CGST, @IGST, @POS, 'No', @RefD, @RefN, @RefI, @RefDt, @VDays, @DTenure, @PCharge, @Rem, @DView, @RType, @DO, @PO, @PODate, @VStart, @VEnd, @UserId, @DiscView, @TCSA, @TCSP, @FrA, @FrP, @OthN, @OthA, 1, 1, @SalesPersonId, @CompanyID)", conn, trans))
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
                        cmd.Parameters.AddWithValue("@POS", ddlPlaceOfSupply.Text?.Trim());
                        cmd.Parameters.AddWithValue("@RefD", referenceOption);
                        cmd.Parameters.AddWithValue("@RefN", referenceName);
                        cmd.Parameters.AddWithValue("@RefI", referenceId);
                        cmd.Parameters.AddWithValue("@RefDt", GetSafeDate(referenceDate));
                        cmd.Parameters.AddWithValue("@VDays", validDays);
                        cmd.Parameters.AddWithValue("@DTenure", deliveryTenure);
                        cmd.Parameters.AddWithValue("@PCharge", packageForwarding);
                        cmd.Parameters.AddWithValue("@Rem", remarks);
                        cmd.Parameters.AddWithValue("@DView", itemview);
                        cmd.Parameters.AddWithValue("@RType", recordtyp);
                        cmd.Parameters.AddWithValue("@DO", DO_number);
                        cmd.Parameters.AddWithValue("@PO", PO_number);
                        cmd.Parameters.AddWithValue("@PODate", GetSafeDate(PO_Date));
                        cmd.Parameters.AddWithValue("@VStart", GetSafeDate(ValStart_Date));
                        cmd.Parameters.AddWithValue("@VEnd", GetSafeDate(ValEnd_Date));
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@DiscView", DDL_DiscountView.SelectedItem.Text?.Trim());
                        cmd.Parameters.AddWithValue("@TCSA", tcsAmount);
                        cmd.Parameters.AddWithValue("@TCSP", tcsPercent);
                        cmd.Parameters.AddWithValue("@FrA", deliveryAmount);
                        cmd.Parameters.AddWithValue("@FrP", freightPercent);
                        cmd.Parameters.AddWithValue("@OthN", TextBox1.Text.Trim());
                        cmd.Parameters.AddWithValue("@OthA", otherAmount);
                        cmd.Parameters.AddWithValue("@SalesPersonId", cmbSalesPerson.SelectedValue == "0" ? DBNull.Value : (object)cmbSalesPerson.SelectedValue);
                        cmd.Parameters.AddWithValue("@CompanyID", companyId);
                        cmd.ExecuteNonQuery();
                    }

                    insertPaymentPhaseNew(newRecordID, conn, trans);
                    insertprimaryServiceNew(newRecordID, conn, trans);

                    InsertSystemNotification(trans, conn, "New Document Version Generated", $"Revision {newRecordID} generated successfully from {oldRecordID}.", "PURCHASE", "SUCCESS");

                    trans.Commit();

                    lblOk.Text = "New Record Created Successfully: " + newRecordID;
                    PanelOK.Visible = true;
                    btnSabe.Visible = btnNew.Visible = false;
                }
                catch (Exception ex)
                {
                    try { trans?.Rollback(); } catch { }
                    throw ex;
                }
            }
            }
            catch
            {
                lblqno.Text = oldRecordID;
                throw;
            }
        }

        // ================= SUPPORT DB METHODS =================

        private decimal ParseDecimal(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0m;
            decimal val = 0;
            if (decimal.TryParse(text, out val)) return val;
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
            if (string.IsNullOrWhiteSpace(input)) return "";
            DateTime dt;
            if (DateTime.TryParseExact(input, "dd-MMM-yyyy", null, System.Globalization.DateTimeStyles.None, out dt))
                return dt.ToString("dd-MMM-yyyy");
            if (DateTime.TryParse(input, out dt))
                return dt.ToString("dd-MMM-yyyy");
            return "";
        }

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

        private bool QuotationNoExists(string quotationNo)
        {
            string query = "SELECT COUNT(*) FROM tbl_Quotation WHERE Quotation_no = @QuotationNo AND CompanyID = @CompanyID";
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@QuotationNo", quotationNo);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                con.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        private int idreturn()
        {
            int b = 0;
            string d = txtquotationDate.Text, m = d.Substring(3, 3), y = d.Substring(7, 4), d4, d5, d6;
            if (m == "Jan" || m == "Feb" || m == "Mar") { d4 = (Convert.ToInt32(y) - 1).ToString(); d5 = "31-Mar-" + d4; d6 = "31-Mar-" + y; }
            else { d4 = (Convert.ToInt32(y) + 1).ToString(); d5 = "31-Mar-" + y; d6 = "31-Mar-" + d4; }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(@"SELECT Sl_no FROM tbl_Quotation WHERE ID = (
                SELECT MAX(ID) FROM tbl_Quotation WHERE CAST(Quotation_date AS DATETIME) BETWEEN @d5 AND @d6 AND CompanyID = @CompanyID)", con))
            {
                cmd.Parameters.AddWithValue("@d5", d5);
                cmd.Parameters.AddWithValue("@d6", d6);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                con.Open();
                object res = cmd.ExecuteScalar();
                if (res != null && res != DBNull.Value) b = Convert.ToInt32(res);
            }
            return b;
        }

        private string findmonth()
        {
            string MonthName = "-";
            string a = txtquotationDate.Text.Substring(3, 3);
            string b = txtquotationDate.Text.Substring(9, 2);
            if (a == "Jan" || a == "Feb" || a == "Mar")
                MonthName = (Convert.ToInt32(b) - 1).ToString() + "-" + b + "/";
            else
                MonthName = b + "-" + (Convert.ToInt32(b) + 1).ToString() + "/";
            return MonthName;
        }

        private void insertPaymentPhaseNew(string qutno, SqlConnection conn, SqlTransaction trans)
        {
            int companyId = CompanyContext.CurrentCompanyID;

            using (SqlCommand delCmd = new SqlCommand("DELETE FROM tbl_QutPaymentPhase WHERE qut_no = @qno AND CompanyID = @CompanyID", conn, trans))
            {
                delCmd.Parameters.AddWithValue("@qno", qutno);
                delCmd.Parameters.AddWithValue("@CompanyID", companyId);
                delCmd.ExecuteNonQuery();
            }

            int totalRows = GridView3.Rows.Count;
            foreach (GridViewRow gvr in GridView3.Rows)
            {
                string phasetype = ((Label)gvr.Cells[1].FindControl("PaymentPhase")).Text;
                string phasedesc = ((TextBox)gvr.Cells[2].FindControl("PhaseDesc")).Text;
                string amo = ((TextBox)gvr.Cells[0].FindControl("AmountPer")).Text;
                if (totalRows == 1) amo = "100";

                using (SqlCommand cmd = new SqlCommand("INSERT INTO tbl_QutPaymentPhase(qut_no, phase_type, PhaseDesc, amountper, TimeStamp, CompanyID) VALUES (@qut_no, @phase_type, @PhaseDesc, @amountper, GETDATE(), @CompanyID)", conn, trans))
                {
                    cmd.Parameters.AddWithValue("@qut_no", qutno);
                    cmd.Parameters.AddWithValue("@phase_type", phasetype);
                    cmd.Parameters.AddWithValue("@PhaseDesc", phasedesc);
                    cmd.Parameters.AddWithValue("@amountper", amo);
                    cmd.Parameters.AddWithValue("@CompanyID", companyId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void insertprimaryServiceNew(string qutno, SqlConnection conn, SqlTransaction trans)
        {
            int companyId = CompanyContext.CurrentCompanyID;

            using (SqlCommand delCmd = new SqlCommand("DELETE FROM tbl_QutPrimaryService WHERE qut_no = @qno AND CompanyID = @CompanyID", conn, trans)) { delCmd.Parameters.AddWithValue("@qno", qutno); delCmd.Parameters.AddWithValue("@CompanyID", companyId); delCmd.ExecuteNonQuery(); }
            using (SqlCommand delCmd = new SqlCommand("DELETE FROM tbl_QuoPserTerm WHERE qutno = @qno AND CompanyID = @CompanyID", conn, trans)) { delCmd.Parameters.AddWithValue("@qno", qutno); delCmd.Parameters.AddWithValue("@CompanyID", companyId); delCmd.ExecuteNonQuery(); }
            using (SqlCommand delCmd = new SqlCommand("DELETE FROM tbl_QuoPriSerTogather WHERE qutno = @qno AND CompanyID = @CompanyID", conn, trans)) { delCmd.Parameters.AddWithValue("@qno", qutno); delCmd.Parameters.AddWithValue("@CompanyID", companyId); delCmd.ExecuteNonQuery(); }

            string ps = ""; int i = 0;
            foreach (GridViewRow r in gridps.Rows)
            {
                string pc = ((Label)r.Cells[0].FindControl("ProductCatagory")).Text;
                using (SqlCommand cmd = new SqlCommand("INSERT INTO tbl_QutPrimaryService(qut_no, PrimaryService, TimeStamp, CompanyID) VALUES (@q, @ps, GETDATE(), @CompanyID)", conn, trans))
                { cmd.Parameters.AddWithValue("@q", qutno); cmd.Parameters.AddWithValue("@ps", pc); cmd.Parameters.AddWithValue("@CompanyID", companyId); cmd.ExecuteNonQuery(); }

                using (SqlCommand cmd = new SqlCommand("SELECT PrimaryServiceTerms FROM tbl_PrimaryServiceTerms WHERE PrimaryService=@p", conn, trans))
                {
                    cmd.Parameters.AddWithValue("@p", pc);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable(); da.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            using (SqlCommand c = new SqlCommand("INSERT INTO tbl_QuoPserTerm (qutno, PServiceName, PSerTer, TimeStamp, CompanyID) VALUES (@q, @pn, @pt, GETDATE(), @CompanyID)", conn, trans))
                            { c.Parameters.AddWithValue("@q", qutno); c.Parameters.AddWithValue("@pn", pc); c.Parameters.AddWithValue("@pt", dr[0]); c.Parameters.AddWithValue("@CompanyID", companyId); c.ExecuteNonQuery(); }
                        }
                    }
                }
                pc = "“" + pc + "”";
                if (i == 0) ps = pc; else if (i == 1) ps += " and " + pc; else ps += " , " + pc;
                i++;
            }
            if (!string.IsNullOrEmpty(ps))
            {
                using (SqlCommand cmd = new SqlCommand("INSERT INTO tbl_QuoPriSerTogather (qutno, PServiceName, TimeStamp, CompanyID) VALUES (@q, @ps, GETDATE(), @CompanyID)", conn, trans))
                { cmd.Parameters.AddWithValue("@q", qutno); cmd.Parameters.AddWithValue("@ps", ps); cmd.Parameters.AddWithValue("@CompanyID", companyId); cmd.ExecuteNonQuery(); }
            }
        }

        private void updatedueamountdetails(double netamount)
        {
            string a = findtotalamount();
            double amount = netamount - Convert.ToDouble(a);
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(@"UPDATE d SET Due_amount = @Due
                FROM tbl_invoice_due d
                INNER JOIN tbl_Quotation q ON q.Quotation_no = d.qutation_no AND q.CompanyID = @CompanyID
                WHERE d.qutation_no = @QNo", con))
            {
                cmd.Parameters.AddWithValue("@Due", amount.ToString());
                cmd.Parameters.AddWithValue("@QNo", lblqno.Text);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private string findtotalamount()
        {
            string amount = "0";
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(@"SELECT SUM(CAST(p.Given_amount AS real)) AS amount
                FROM tbl_invoice_payment p
                INNER JOIN tbl_Quotation q ON q.Quotation_no = p.Quotation_No AND q.CompanyID = @CompanyID
                WHERE p.Quotation_No = @QNo", con))
            {
                cmd.Parameters.AddWithValue("@QNo", lblqno.Text);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                con.Open();
                object res = cmd.ExecuteScalar();
                if (res != null && res != DBNull.Value && !string.IsNullOrEmpty(res.ToString())) amount = res.ToString();
            }
            return amount;
        }

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

        private void BindPaymentPhases(string qutno)
        {
            string query = "SELECT phase_type AS PaymentPhase, PhaseDesc, amountper AS AmountPer FROM tbl_QutPaymentPhase WHERE qut_no = @qutno AND CompanyID = @CompanyID";
            SqlParameter[] param = {
                new SqlParameter("@qutno", qutno),
                new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID)
            };

            DataTable dt = DbCL.SPreturn_dt(query, param);

            if (dt != null && dt.Rows.Count > 0)
            {
                GridView3.DataSource = dt;
                GridView3.DataBind();
                ViewState["phaseAmountData"] = dt;
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
                string query = "SELECT PrimaryService FROM tbl_QutPrimaryService WHERE qut_no = @qut_no AND CompanyID = @CompanyID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@qut_no", qutNo);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
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

        private void bindphaseType(string qutno = "")
        {
            string str = "select id, PaymentPhase from tbl_PaymentPhase order by id";
            dtphasetype = DbCL.SPreturn_dt(str, null);
            listPhaseType.Items.Clear();

            List<string> selectedValues = new List<string>();
            if (!string.IsNullOrEmpty(qutno))
            {
                string selectedQuery = "SELECT phase_type FROM tbl_QutPaymentPhase WHERE qut_no = @qutno AND CompanyID = @CompanyID";
                SqlParameter[] param = {
                    new SqlParameter("@qutno", qutno),
                    new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID)
                };
                DataTable selectedPhases = DbCL.SPreturn_dt(selectedQuery, param);

                if (selectedPhases != null)
                {
                    selectedValues = selectedPhases.AsEnumerable().Select(row => row["phase_type"].ToString()).ToList();
                }
            }

            if (dtphasetype != null)
            {
                foreach (DataRow row in dtphasetype.Rows)
                {
                    string value = row["PaymentPhase"].ToString();
                    ListItem item = new ListItem(value);
                    if (selectedValues.Contains(value)) item.Selected = true;
                    listPhaseType.Items.Add(item);
                }
            }
        }

        private void SetDropdownValue(DropDownList ddl, string text)
        {
            if (ddl == null || string.IsNullOrEmpty(text)) return;
            ListItem item = ddl.Items.FindByText(text);
            if (item == null) item = ddl.Items.FindByValue(text);
            ddl.ClearSelection();
            if (item != null) item.Selected = true;
        }

        public Quotation GetQuotationByNo(string quotationNo)
        {
            Quotation result = null;
            string query = @"SELECT * FROM tbl_Quotation WHERE Quotation_no = @Quotation_no AND CompanyID = @CompanyID";
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Quotation_no", quotationNo);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    result = new Quotation
                    {
                        ID = Convert.ToInt32(reader["ID"]),
                        QuotationNo = reader["Quotation_no"].ToString(),
                        QuotationDate = reader["Quotation_date"].ToString(),
                        ClientId = reader["Client_Id"].ToString(),
                        Gross = reader["Gross"] as decimal?,
                        ServiceTax = reader["Service_tax"] as decimal?,
                        NetAmount = reader["Net_amount"] as decimal?,
                        Status1 = reader["Status1"].ToString(),
                        Status2 = reader["Status2"].ToString(),
                        Status3 = reader["status3"].ToString(),
                        SlNo = reader["Sl_no"] as int?,
                        ServiceTax1 = reader["service_tax1"] as decimal?,
                        SubTotal = reader["sub_total"] as decimal?,
                        CGSTorSGST = reader["cgstOrsgst"] as decimal?,
                        IGST = reader["igst"] as decimal?,
                        ProSer = reader["ProSer"].ToString(),
                        MailStatusDate = reader["mailStatusDate"] as DateTime?,
                        PlaceOfSupply = reader["PlaceofSupply"].ToString(),
                        MailStatus = reader["mailStatus"].ToString(),
                        PaymentStatus = reader["PaymentStatus"].ToString(),
                        ReferenceName = reader["ReferenceName"].ToString(),
                        ReferenceData = reader["ReferenceData"].ToString(),
                        ReferenceId = reader["ReferenceId"].ToString(),
                        ReferenceDate = reader["ReferenceDate"].ToString(),
                        ValidityDays = reader["ValidityDays"].ToString(),
                        DeliveryTenure = reader["DeliveryTenure"].ToString(),
                        PackingCharges = reader["PackingCharges"].ToString(),
                        Remarks = reader["Remarks"].ToString(),
                        DetailedView = reader["DetailedView"].ToString(),
                        RecordType = reader["RecordType"].ToString(),
                        DO_Number = reader["DO_Number"].ToString(),
                        PO_Number = reader["PO_Number"].ToString(),
                        PO_Date = reader["PO_Date"].ToString(),
                        ValidityStartDate = reader["Validity_StartDate"].ToString(),
                        ValidityEndDate = reader["Validity_EndDate"].ToString(),
                        AddedById = reader["AddedById"] as int?,
                        AddedOn = reader["AddedOn"] as DateTime?,
                        ModifiedById = reader["ModifiedById"] as int?,
                        ModifiedOn = reader["ModifiedOn"] as DateTime?,
                        DeletedById = reader["DeletedById"] as int?,
                        DeletedOn = reader["DeletedOn"] as DateTime?,
                        TimsStamp = reader["TimsStamp"] as DateTime?,
                        DiscountView = reader["DiscountView"].ToString(),
                        SalesPersonId = reader["SalesPersonId"] != DBNull.Value ? (int?)Convert.ToInt32(reader["SalesPersonId"]) : null,
                        TCS_Amount = reader["TCS_Amount"] as decimal?,
                        TCS_Percent = reader["TCS_Percent"] as decimal?,
                        Freight_Amount = reader["Freight_Amount"] as decimal?,
                        Freight_VAT_Percent = reader["Freight_VAT_Percent"] as decimal?,
                        OtherCharge_Name = reader["OtherCharge_Name"].ToString(),
                        OtherCharge_Amount = reader["OtherCharge_Amount"] as decimal?
                    };
                }
                reader.Close();
            }
            return result;
        }

        public class Quotation
        {
            public int ID { get; set; }
            public string QuotationNo { get; set; }
            public string QuotationDate { get; set; }
            public string ClientId { get; set; }
            public decimal? Gross { get; set; }
            public decimal? ServiceTax { get; set; }
            public decimal? NetAmount { get; set; }
            public string Status1 { get; set; }
            public string Status2 { get; set; }
            public string Status3 { get; set; }
            public int? SlNo { get; set; }
            public decimal? ServiceTax1 { get; set; }
            public decimal? SubTotal { get; set; }
            public decimal? CGSTorSGST { get; set; }
            public decimal? IGST { get; set; }
            public string ProSer { get; set; }
            public DateTime? MailStatusDate { get; set; }
            public string PlaceOfSupply { get; set; }
            public string MailStatus { get; set; }
            public string PaymentStatus { get; set; }
            public string ReferenceName { get; set; }
            public string ReferenceData { get; set; }
            public string ReferenceId { get; set; }
            public string ReferenceDate { get; set; }
            public string ValidityDays { get; set; }
            public string DeliveryTenure { get; set; }
            public string PackingCharges { get; set; }
            public string Remarks { get; set; }
            public string DetailedView { get; set; }
            public string RecordType { get; set; }
            public string DO_Number { get; set; }
            public string PO_Number { get; set; }
            public string PO_Date { get; set; }
            public string ValidityStartDate { get; set; }
            public string ValidityEndDate { get; set; }
            public int? AddedById { get; set; }
            public DateTime? AddedOn { get; set; }
            public int? ModifiedById { get; set; }
            public DateTime? ModifiedOn { get; set; }
            public int? DeletedById { get; set; }
            public DateTime? DeletedOn { get; set; }
            public DateTime? TimsStamp { get; set; }
            public string DiscountView { get; set; }
            public int? SalesPersonId { get; set; }
            public decimal? TCS_Amount { get; set; }
            public decimal? TCS_Percent { get; set; }
            public decimal? Freight_Amount { get; set; }
            public decimal? Freight_VAT_Percent { get; set; }
            public string OtherCharge_Name { get; set; }
            public decimal? OtherCharge_Amount { get; set; }
        }
    }
}