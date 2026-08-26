using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web;
using System.Collections.Generic;
using System.Web.Services;
using System.Text;


namespace Bill_Software.corporate.business.app
{
    public partial class WebForm26 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["USERID"] == null) Response.Redirect("~/index.aspx");

            if (!IsPostBack)
            {
                txtinvoiceDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                LoadClients();
                LoadSalesPersons();
            }
        }

        #region DATA BINDING (CLIENT & SALES PERSON)
        private void LoadClients()
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT Client_Id, Client_Name FROM tbl_Client WHERE CompanyID = @CompanyID ORDER BY Client_Name", conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        cmbvendor.DataSource = dt;
                        cmbvendor.DataTextField = "Client_Name";
                        cmbvendor.DataValueField = "Client_Name";
                        cmbvendor.DataBind();
                    }
                }
            }
            cmbvendor.Items.Insert(0, new ListItem("-- All Clients --", "0"));
        }

        private void BindInvoiceHistory(string docNo)
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                string query = @"
                    SELECT 
                        i.ID, 
                        i.Invoice_No, 
                        i.Invoice_Date, 
                        i.Quotation_No, 
                        '' AS ExtInvoiceNo, 
                        '' AS PServiceName, 
                        '' AS PO_Number, 
                        '' AS DO_Number, 
                        i.Gross, 
                        i.discount, 
                        i.sub_total, 
                        i.cgstOrsgst, 
                        i.igst, 
                        (ISNULL(i.Net_Amount, 0) - ISNULL(i.sub_total, 0)) AS Gst,
                        i.Delivery_Amount, 
                        i.otherAmount1, 
                        i.Net_Amount,
                        '' AS Validity_StartDate, 
                        '' AS Validity_EndDate,
                        c.Client_Name, 
                        l.Name AS AddedByName,
                        i.TimeStamp
                    FROM tbl_Invoice i
                    LEFT JOIN tbl_Client c ON i.Client_ID = c.Client_Id
                    LEFT JOIN tbl_login l ON i.AddedById = l.User_Id
                    WHERE i.Quotation_No = @RefNo AND i.CompanyID = @CompanyID
                    ORDER BY i.ID DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@RefNo", docNo);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        rptInvoices.DataSource = dt;
                        rptInvoices.DataBind();
                    }
                }
            }
        }

        private void LoadSalesPersons()
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                string query = "SELECT User_Id, Name FROM tbl_login WHERE User_Id NOT IN ('admin', 'AT01') AND CompanyID = @CompanyID AND IsActive = 1 ORDER BY Name";
                using (SqlCommand cmd = new SqlCommand(query, conn))
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

        protected void cmbvendor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbvendor.SelectedIndex > 0)
                LoadClientDataByName(cmbvendor.SelectedItem.Text);
            else
            {
                List_BillingAddress.Items.Clear();
                pnlAddress.Visible = false;
                lblclientId.Text = "";
            }
        }

        private void LoadClientDataByName(string clientName)
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Client_Id FROM tbl_Client WHERE Client_Name=@Name AND CompanyID=@CompanyID", conn);
                cmd.Parameters.AddWithValue("@Name", clientName);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                object res = cmd.ExecuteScalar();
                if (res != null)
                {
                    lblclientId.Text = res.ToString();
                    LoadAddresses(lblclientId.Text, conn);
                }
            }
        }

        private void LoadAddresses(string cid, SqlConnection conn)
        {
            List_BillingAddress.Items.Clear();
            List_ShippingAddress.Items.Clear();

            // 1. Load Main HQ Address
            SqlCommand cmd1 = new SqlCommand("SELECT Address1+', '+City+', '+pin+', '+State FROM tbl_Client WHERE Client_Id=@CID AND CompanyID=@CompanyID", conn);
            cmd1.Parameters.AddWithValue("@CID", cid);
            cmd1.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
            using (SqlDataReader dr = cmd1.ExecuteReader())
            {
                while (dr.Read())
                {
                    List_BillingAddress.Items.Add(dr[0].ToString());
                    List_ShippingAddress.Items.Add(dr[0].ToString());
                }
            }

            // 2. Load Branch/Site Addresses
            SqlCommand cmd2 = new SqlCommand("SELECT Address+', '+State+', '+City+', '+pin FROM tbl_ClientRegAddress WHERE Client_Id=@CID AND CompanyID=@CompanyID", conn);
            cmd2.Parameters.AddWithValue("@CID", cid);
            cmd2.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
            using (SqlDataReader dr = cmd2.ExecuteReader())
            {
                while (dr.Read())
                {
                    List_BillingAddress.Items.Add(dr[0].ToString());
                    List_ShippingAddress.Items.Add(dr[0].ToString());
                }
            }

            pnlAddress.Visible = true;
            if (List_BillingAddress.Items.Count > 0)
            {
                List_BillingAddress.SelectedIndex = 0;
                List_ShippingAddress.SelectedIndex = 0;
            }

            // Run UI Sync
            ScriptManager.RegisterStartupScript(this, GetType(), "syncAddr", "setTimeout(syncAddresses, 100);", true);
        }
        #endregion

        #region SMART SEARCH & GRID BINDING
        //protected void btnSertch_Click(object sender, EventArgs e)
        //{
        //    string docType = ddlDocType.SelectedValue;
        //    string qry = "";
        //    string colDate = "";
        //    string recordType = "";
        //    string searchDoc = txtSourceDocNo.Text.Trim();

        //    DateTime fromDate = DateTime.MinValue, toDate = DateTime.MinValue;
        //    bool hasFrom = false, hasTo = false;

        //    if (docType == "Quotation" || docType == "Purchase Order")
        //    {
        //        colDate = "t.Quotation_date";
        //        recordType = docType == "Quotation" ? "Quotation" : "Purchase Order";
        //        string extRefColumn = docType == "Purchase Order" ? "t.PO_Number" : "t.Quotation_no";

        //        qry = $@"SELECT TOP 100 t.Quotation_no AS DocNo, t.Quotation_date AS DocDate, ISNULL(c.Client_Name, 'Unknown') AS Client_Name, 
        //                 ISNULL({extRefColumn}, 'N/A') AS ExtRef, ISNULL(l.Name, 'System') AS CreatedBy, ISNULL(t.status1, 'Pending') AS Status,
        //                 t.Net_amount 
        //                 FROM tbl_Quotation t 
        //                 LEFT JOIN tbl_Client c ON t.Client_Id = c.Client_Id 
        //                 LEFT JOIN tbl_login l ON t.AddedById = l.User_Id
        //                 WHERE t.CompanyID = @CompanyID AND t.RecordType = @RecordType";
        //    }
        //    else if (docType == "Delivery Challan")
        //    {
        //        colDate = "t.Chalan_Date";
        //        qry = $@"SELECT TOP 100 t.Chalan_No AS DocNo, t.Chalan_Date AS DocDate, ISNULL(c.Client_Name, 'Unknown') AS Client_Name, 
        //                 ISNULL(t.PO_Number, 'N/A') AS ExtRef, ISNULL(l.Name, 'System') AS CreatedBy, ISNULL(t.Status, 'Pending') AS Status,
        //                 0.00 AS Net_amount 
        //                 FROM tbl_Chalan t 
        //                 LEFT JOIN tbl_Client c ON t.Client_ID = c.Client_Id 
        //                 LEFT JOIN tbl_login l ON t.AddedById = l.User_Id
        //                 WHERE t.CompanyID = @CompanyID";
        //    }
        //    else if (docType == "Proforma")
        //    {
        //        colDate = "t.Invoice_Date";
        //        qry = $@"SELECT TOP 100 t.Invoice_No AS DocNo, t.Invoice_Date AS DocDate, ISNULL(c.Client_Name, 'Unknown') AS Client_Name, 
        //                 ISNULL(t.PO_Number, 'N/A') AS ExtRef, ISNULL(l.Name, 'System') AS CreatedBy, ISNULL(t.status1, 'Pending') AS Status,
        //                 CAST(ISNULL(t.Net_Amount, '0') AS DECIMAL(18,2)) AS Net_amount 
        //                 FROM tbl_Proforma t 
        //                 LEFT JOIN tbl_Client c ON t.Client_ID = c.Client_Id 
        //                 LEFT JOIN tbl_login l ON t.AddedById = l.User_Id
        //                 WHERE t.CompanyID = @CompanyID";
        //    }

        //    if (!string.IsNullOrEmpty(searchDoc))
        //    {
        //        if (docType == "Delivery Challan") qry += " AND t.Chalan_No LIKE '%' + @DocRef + '%'";
        //        else if (docType == "Proforma") qry += " AND t.Invoice_No LIKE '%' + @DocRef + '%'";
        //        else if (docType == "Purchase Order") qry += " AND t.PO_Number LIKE '%' + @DocRef + '%'";
        //        else qry += " AND t.Quotation_no LIKE '%' + @DocRef + '%'";
        //    }
        //    else
        //    {
        //        if (cmbvendor.SelectedIndex > 0) qry += " AND t.Client_Id = @CID";

        //        hasFrom = DateTime.TryParse(txtfromDate.Text, out fromDate);
        //        hasTo = DateTime.TryParse(txttodate.Text, out toDate);

        //        if (hasFrom && hasTo) qry += $" AND CAST({colDate} AS DATE) >= @From AND CAST({colDate} AS DATE) <= @To";
        //        else if (hasFrom) qry += $" AND CAST({colDate} AS DATE) >= @From";
        //        else if (hasTo) qry += $" AND CAST({colDate} AS DATE) <= @To";
        //    }

        //    qry += $" ORDER BY CAST({colDate} AS DATE) DESC";

        //    using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
        //    {
        //        using (SqlCommand cmd = new SqlCommand(qry, conn))
        //        {
        //            cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
        //            if (!string.IsNullOrEmpty(recordType)) cmd.Parameters.AddWithValue("@RecordType", recordType);

        //            if (!string.IsNullOrEmpty(searchDoc))
        //            {
        //                cmd.Parameters.AddWithValue("@DocRef", searchDoc);
        //            }
        //            else
        //            {
        //                if (cmbvendor.SelectedIndex > 0) cmd.Parameters.AddWithValue("@CID", lblclientId.Text);
        //                if (hasFrom) cmd.Parameters.AddWithValue("@From", fromDate);
        //                if (hasTo) cmd.Parameters.AddWithValue("@To", toDate);
        //            }

        //            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
        //            {
        //                DataTable dt = new DataTable();
        //                sda.Fill(dt);
        //                gvSearchDocs.DataSource = dt;
        //                gvSearchDocs.DataBind();
        //            }
        //        }
        //    }
        //}


        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string docType = ddlDocType.SelectedValue;
            string qry = "";
            string colDate = "";
            string recordType = "";
            string searchDoc = txtSourceDocNo.Text.Trim();

            DateTime fromDate = DateTime.MinValue, toDate = DateTime.MinValue;
            bool hasFrom = false, hasTo = false;

            // ==========================================
            // 1. DYNAMIC SELECT QUERIES & EXT-REF FORMATTING
            // ==========================================
            if (docType == "Quotation" || docType == "Purchase Order")
            {
                colDate = "t.Quotation_date";
                recordType = docType == "Quotation" ? "Quotation" : "Purchase Order";

                // For POs, explicitly show both PO and DO numbers in the ExtRef column
                string extRefColumn = docType == "Purchase Order"
                    ? "ISNULL(t.PO_Number, 'N/A') + ' / ' + ISNULL(t.DO_Number, 'N/A')"
                    : "ISNULL(t.PO_Number, 'N/A')";

                qry = $@"SELECT TOP 100 t.Quotation_no AS DocNo, t.Quotation_date AS DocDate, ISNULL(c.Client_Name, 'Unknown') AS Client_Name, 
                         {extRefColumn} AS ExtRef, ISNULL(l.Name, 'System') AS CreatedBy, ISNULL(t.status1, 'Pending') AS Status,
                         t.Net_amount 
                         FROM tbl_Quotation t 
                         LEFT JOIN tbl_Client c ON t.Client_Id = c.Client_Id 
                         LEFT JOIN tbl_login l ON t.AddedById = l.User_Id
                         WHERE t.CompanyID = @CompanyID AND t.RecordType = @RecordType";
            }
            else if (docType == "Delivery Challan")
            {
                colDate = "t.Chalan_Date";

                // FIX: Swapped PO_Number for Quotation_No. 
                // FIX: Removed tbl_login join and hardcoded 'System' and 'Delivered' since those columns don't exist in tbl_Chalan.
                qry = $@"SELECT TOP 100 
                         t.Chalan_No AS DocNo, 
                         t.Chalan_Date AS DocDate, 
                         ISNULL(c.Client_Name, 'Unknown') AS Client_Name, 
                         ISNULL(t.Quotation_No, 'N/A') AS ExtRef, 
                         'System' AS CreatedBy, 
                         'Delivered' AS Status,
                         0.00 AS Net_amount 
                         FROM tbl_Chalan t 
                         LEFT JOIN tbl_Client c ON t.Client_ID = c.Client_Id 
                         WHERE t.CompanyID = @CompanyID";
            }
            else if (docType == "Proforma")
            {
                colDate = "t.Invoice_Date";

                // FIX: Swapped PO_Number for Quotation_No.
                // FIX: Removed tbl_login join. Hardcoded 'System' and 'Generated' to satisfy the GridView schema.
                qry = $@"SELECT TOP 100 
                         t.Invoice_No AS DocNo, 
                         t.Invoice_Date AS DocDate, 
                         ISNULL(c.Client_Name, 'Unknown') AS Client_Name, 
                         ISNULL(t.Quotation_No, 'N/A') AS ExtRef, 
                         'System' AS CreatedBy, 
                         'Generated' AS Status,
                         CAST(ISNULL(NULLIF(t.Net_Amount, ''), '0') AS DECIMAL(18,2)) AS Net_amount 
                         FROM tbl_Proforma t 
                         LEFT JOIN tbl_Client c ON t.Client_ID = c.Client_Id 
                         WHERE t.CompanyID = @CompanyID";
            }

            // ==========================================
            // 2. THE OMNI-SEARCH FILTERS
            // ==========================================
            if (!string.IsNullOrEmpty(searchDoc))
            {
                if (docType == "Delivery Challan")
                    qry += " AND (t.Chalan_No LIKE '%' + @DocRef + '%' OR t.Quotation_No LIKE '%' + @DocRef + '%')";

                // FIX: Search against Quotation_No instead of PO_Number for Proformas
                else if (docType == "Proforma")
                    qry += " AND (t.Invoice_No LIKE '%' + @DocRef + '%' OR t.Quotation_No LIKE '%' + @DocRef + '%')";

                else if (docType == "Purchase Order")
                    qry += " AND (t.Quotation_no LIKE '%' + @DocRef + '%' OR t.PO_Number LIKE '%' + @DocRef + '%' OR t.DO_Number LIKE '%' + @DocRef + '%')";

                else // Quotation
                    qry += " AND (t.Quotation_no LIKE '%' + @DocRef + '%' OR t.PO_Number LIKE '%' + @DocRef + '%')";
            }
            else
            {
                if (cmbvendor.SelectedIndex > 0) qry += " AND t.Client_Id = @CID";

                hasFrom = DateTime.TryParse(txtfromDate.Text, out fromDate);
                hasTo = DateTime.TryParse(txttodate.Text, out toDate);

                if (hasFrom && hasTo) qry += $" AND CAST({colDate} AS DATE) >= @From AND CAST({colDate} AS DATE) <= @To";
                else if (hasFrom) qry += $" AND CAST({colDate} AS DATE) >= @From";
                else if (hasTo) qry += $" AND CAST({colDate} AS DATE) <= @To";
            }

            qry += $" ORDER BY CAST({colDate} AS DATE) DESC";

            // ==========================================
            // 3. SECURE EXECUTION
            // ==========================================
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(qry, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    if (!string.IsNullOrEmpty(recordType)) cmd.Parameters.AddWithValue("@RecordType", recordType);

                    if (!string.IsNullOrEmpty(searchDoc))
                    {
                        // Secure Injection: SQL evaluates string parameters properly, preventing the numeric casting crash
                        cmd.Parameters.AddWithValue("@DocRef", searchDoc);
                    }
                    else
                    {
                        if (cmbvendor.SelectedIndex > 0) cmd.Parameters.AddWithValue("@CID", lblclientId.Text);
                        if (hasFrom) cmd.Parameters.AddWithValue("@From", fromDate);
                        if (hasTo) cmd.Parameters.AddWithValue("@To", toDate);
                    }

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        gvSearchDocs.DataSource = dt;
                        gvSearchDocs.DataBind();
                    }
                }
            }
        }

        protected void btnResetSearch_Click(object sender, EventArgs e)
        {
            // 1. Clear text inputs
            txtSourceDocNo.Text = string.Empty;
            txtfromDate.Text = string.Empty;
            txttodate.Text = string.Empty;

            // 2. Reset Dropdowns (Safely)
            if (cmbvendor.Items.Count > 0)
            {
                cmbvendor.ClearSelection();
                cmbvendor.SelectedIndex = 0;
            }

            if (ddlDocType.Items.Count > 0)
            {
                ddlDocType.ClearSelection();
                ddlDocType.SelectedIndex = 0; // Usually defaults back to 'Quotation'
            }

            // 3. Clear any selected client ID references
            lblclientId.Text = string.Empty;

            // 4. Fire the search event again to load the default top 100 unfiltered records
            btnSertch_Click(sender, e);

            // 5. Re-trigger the JS to fix the Omni-Search placeholder text
            ScriptManager.RegisterStartupScript(this, GetType(), "ResetUI", "setTimeout(updateDocPlaceholder, 100);", true);
        }

        protected void gvSearchDocs_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // 1. Early Exit: Reduces deep nesting
            if (e.CommandName != "SelectDoc") return;

            GridViewRow row = (GridViewRow)(((Button)e.CommandSource).NamingContainer);
            string clientName = HttpUtility.HtmlDecode(row.Cells[2].Text);

            // 2. Sync Client Dropdown safely
            if (cmbvendor.SelectedItem == null || cmbvendor.SelectedItem.Text.Trim() != clientName.Trim())
            {
                ListItem item = cmbvendor.Items.FindByText(clientName);
                if (item == null)
                {
                    foreach (ListItem li in cmbvendor.Items)
                    {
                        if (li.Text.Trim().Equals(clientName.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            item = li; break;
                        }
                    }
                }

                if (item != null)
                {
                    cmbvendor.ClearSelection();
                    item.Selected = true;
                    LoadClientDataByName(item.Text);
                }
                else
                {
                    ShowMsg($"Client '{clientName}' not found in active list.", false);
                    return;
                }
            }

            // 3. Address Validations
            if (List_BillingAddress.Items.Count == 0) { ShowMsg("No address found for this client.", false); return; }
            if (List_BillingAddress.SelectedIndex == -1) List_BillingAddress.SelectedIndex = 0;

            // ==========================================
            // 🚀 INITIALIZE CONTEXT STATE
            // ==========================================
            string docNo = e.CommandArgument.ToString();
            string docType = ddlDocType.SelectedValue;
            int companyId = CompanyContext.CurrentCompanyID;

            ViewState["SelectedDocNo"] = docNo;
            ViewState["SelectedDocType"] = docType;
            ViewState["RemovedItems"] = null;
            btnRestore.Visible = false;
            hdnRefNo.Value = docNo;

            // Map UI Headers (LHS & RHS)
            lblConfirmClient.Text = cmbvendor.SelectedItem != null && cmbvendor.SelectedIndex > 0 ? cmbvendor.SelectedItem.Text : clientName;
            lblBillingAddress.Text = List_BillingAddress.SelectedItem != null ? List_BillingAddress.SelectedItem.Text : "Billing Address Not Provided";
            lblConfirmAddress.Text = List_ShippingAddress.SelectedItem != null ? List_ShippingAddress.SelectedItem.Text : "Shipping Address Not Provided";

            lblConfirmDoc.Text = $"{docType.ToUpper()} NO: {docNo}";
            lblDocTypeView.Text = docType;

            // ==========================================
            // 🚀 SINGLE-CONNECTION DATABASE FETCH
            // ==========================================
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open(); // Open once, use for both MetaData and Line Items

                // --- A. Fetch MetaData (RHS Card) ---
                FetchDocumentMetadata(conn, docNo, docType, companyId);

                // --- B. Fetch Line Items ---
                string itemQuery = GetItemQueryByDocType(docType);

                using (SqlCommand cmd = new SqlCommand(itemQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Ref", docNo);
                    cmd.Parameters.AddWithValue("@CompanyID", companyId);

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);

                        // ==========================================
                        // 🚀 SMART GATEKEEPER & AUTO-FILTER
                        // ==========================================
                        decimal totalPendingForDoc = 0;

                        // Loop backward to safely delete rows from the DataTable in-memory
                        for (int i = dt.Rows.Count - 1; i >= 0; i--)
                        {
                            decimal pending = 0;
                            if (dt.Rows[i]["PendingQty"] != DBNull.Value)
                                decimal.TryParse(dt.Rows[i]["PendingQty"].ToString(), out pending);

                            if (pending <= 0)
                            {
                                dt.Rows[i].Delete(); // Strip out fully billed items
                            }
                            else
                            {
                                totalPendingForDoc += pending;
                            }
                        }
                        dt.AcceptChanges();

                        // Gatekeeper Block
                        if (totalPendingForDoc <= 0 || dt.Rows.Count == 0)
                        {
                            ShowMsg($"Document {docNo} has already been fully invoiced. No pending items remain.", false);
                            return; // Hard Stop: Keep user on Tab 1
                        }

                        // Push to ViewState & Bind
                        ViewState["InvoiceItems"] = dt;
                        BindProductsGrid();
                    }
                }
            }

            // Finalize transition to Tab 2
            BindInvoiceHistory(docNo);
            mvInvoice.ActiveViewIndex = 1;
        }

        // ==========================================
        // 🛠️ HELPER METHODS (Add these just below)
        // ==========================================

        private void FetchDocumentMetadata(SqlConnection conn, string docNo, string docType, int companyId)
        {
            string metaSql = "";
            if (docType == "Quotation" || docType == "Purchase Order")
            {
                metaSql = @"SELECT DO_Number, PO_Number, CONVERT(varchar, PO_Date, 106) as PODate, 
                           CONVERT(varchar, Validity_StartDate, 106) as VStart, CONVERT(varchar, Validity_EndDate, 106) as VEnd 
                    FROM tbl_Quotation WHERE Quotation_no = @DocNo AND CompanyID = @CompanyID";
            }
            else if (docType == "Delivery Challan")
            {
                metaSql = @"SELECT '' as DO_Number, PO_Number, CONVERT(varchar, PO_Date, 106) as PODate, 
                           '' as VStart, '' as VEnd 
                    FROM tbl_Chalan WHERE Chalan_No = @DocNo AND CompanyID = @CompanyID";
            }

            if (string.IsNullOrEmpty(metaSql)) return;

            try
            {
                using (SqlCommand cmdMeta = new SqlCommand(metaSql, conn))
                {
                    cmdMeta.Parameters.AddWithValue("@DocNo", docNo);
                    cmdMeta.Parameters.AddWithValue("@CompanyID", companyId);

                    using (SqlDataReader drMeta = cmdMeta.ExecuteReader())
                    {
                        if (drMeta.Read())
                        {
                            // 1. Safely extract strings (C# 5.0 compatible)
                            string doNum = drMeta["DO_Number"] != DBNull.Value ? drMeta["DO_Number"].ToString() : "";
                            string poNum = drMeta["PO_Number"] != DBNull.Value ? drMeta["PO_Number"].ToString() : "";
                            string poDate = drMeta["PODate"] != DBNull.Value ? drMeta["PODate"].ToString() : "";
                            string vStart = drMeta["VStart"] != DBNull.Value ? drMeta["VStart"].ToString() : "";
                            string vEnd = drMeta["VEnd"] != DBNull.Value ? drMeta["VEnd"].ToString() : "";

                            // 2. Assign to labels with whitespace validation
                            lblConfirmDO.Text = !string.IsNullOrWhiteSpace(doNum) ? doNum : "N/A";
                            lblConfirmPONum.Text = !string.IsNullOrWhiteSpace(poNum) ? poNum : "N/A";
                            lblConfirmPODate.Text = !string.IsNullOrWhiteSpace(poDate) ? poDate : "N/A";
                            lblConfirmValStart.Text = !string.IsNullOrWhiteSpace(vStart) ? vStart : "N/A";
                            lblConfirmValEnd.Text = !string.IsNullOrWhiteSpace(vEnd) ? vEnd : "N/A";
                        }
                    }
                }
            }
            catch
            {
                // Fail gracefully
                lblConfirmDO.Text = "N/A"; lblConfirmPONum.Text = "N/A"; lblConfirmPODate.Text = "N/A";
                lblConfirmValStart.Text = "N/A"; lblConfirmValEnd.Text = "N/A";
            }
        }

        private string GetItemQueryByDocType(string docType)
        {
            if (docType == "Quotation" || docType == "Purchase Order")
            {
                return @"
                    SELECT 
                        qd.Product_Code AS TrueID, qd.Product_id AS TrueHSN, qd.Product_name, 
                        CAST(qd.Quantity AS DECIMAL(18,2)) AS QuotedQty,
                        ISNULL((SELECT SUM(CAST(id.Quantity AS DECIMAL(18,2))) FROM tbl_Invoice_details id INNER JOIN tbl_Invoice inv ON id.Invoice_No = inv.Invoice_No AND id.CompanyID = inv.CompanyID WHERE id.Quotation_no = qd.Quotation_no AND id.Product_id = qd.Product_Code AND id.CompanyID = @CompanyID AND inv.status2 = 'Active'), 0) AS InvoicedQty,
                        (CAST(qd.Quantity AS DECIMAL(18,2)) - ISNULL((SELECT SUM(CAST(id.Quantity AS DECIMAL(18,2))) FROM tbl_Invoice_details id INNER JOIN tbl_Invoice inv ON id.Invoice_No = inv.Invoice_No AND id.CompanyID = inv.CompanyID WHERE id.Quotation_no = qd.Quotation_no AND id.Product_id = qd.Product_Code AND id.CompanyID = @CompanyID AND inv.status2 = 'Active'), 0)) AS PendingQty,
                        qd.sail_rate, qd.discount_rate AS discountRate, qd.Service_tax_rate, qd.specification, ISNULL(np.Quantity, '0') AS AvailableStock,
                        qd.ItemNo, qd.MaterialNo, qd.PackSize, qd.Unit, qd.DeliveryDate, qd.Department, qd.ItemRemarks
                    FROM tbl_Quotaion_details qd
                    LEFT JOIN tbl_NewProduct np ON np.ProductID = qd.Product_Code AND np.CompanyID = @CompanyID
                    WHERE qd.Quotation_no = @Ref AND qd.CompanyID = @CompanyID";
            }
            if (docType == "Proforma")
            {
                return @"
                SELECT 
                    pd.Product_id AS TrueID, pd.Product_Code AS TrueHSN, pd.Product_name, 
                    CAST(pd.Quantity AS DECIMAL(18,2)) AS QuotedQty,
                    ISNULL((SELECT SUM(CAST(id.Quantity AS DECIMAL(18,2))) FROM tbl_Invoice_details id INNER JOIN tbl_Invoice inv ON id.Invoice_No = inv.Invoice_No AND id.CompanyID = inv.CompanyID WHERE id.Quotation_no = pd.Invoice_No AND id.Product_id = pd.Product_id AND id.CompanyID = @CompanyID AND inv.status2 = 'Active'), 0) AS InvoicedQty,
                    (CAST(pd.Quantity AS DECIMAL(18,2)) - ISNULL((SELECT SUM(CAST(id.Quantity AS DECIMAL(18,2))) FROM tbl_Invoice_details id INNER JOIN tbl_Invoice inv ON id.Invoice_No = inv.Invoice_No AND id.CompanyID = inv.CompanyID WHERE id.Quotation_no = pd.Invoice_No AND id.Product_id = pd.Product_id AND id.CompanyID = @CompanyID AND inv.status2 = 'Active'), 0)) AS PendingQty,
                    pd.Rate AS sail_rate, 0 AS discountRate, pd.Tax_Rate AS Service_tax_rate, pd.ProductOrServiceCat AS specification, ISNULL(np.Quantity, '0') AS AvailableStock,
                    '' AS ItemNo, '' AS MaterialNo, '' AS PackSize, '' AS Unit, '' AS DeliveryDate, '' AS Department, '' AS ItemRemarks
                FROM tbl_Proforma_Details pd
                LEFT JOIN tbl_NewProduct np ON np.ProductID = pd.Product_id AND np.CompanyID = @CompanyID
                WHERE pd.Invoice_No = @Ref AND pd.CompanyID = @CompanyID";
            }

            // Default Delivery Challan
            return @"
                SELECT 
                    cd.Product_id AS TrueID, cd.Product_code AS TrueHSN, cd.Product_name, 
                    CAST(cd.Quantity AS DECIMAL(18,2)) AS QuotedQty,
                    ISNULL((SELECT SUM(CAST(id.Quantity AS DECIMAL(18,2))) FROM tbl_Invoice_details id INNER JOIN tbl_Invoice inv ON id.Invoice_No = inv.Invoice_No AND id.CompanyID = inv.CompanyID WHERE id.Quotation_no = cd.Challan_no AND id.Product_id = cd.Product_id AND id.CompanyID = @CompanyID AND inv.status2 = 'Active'), 0) AS InvoicedQty,
                    (CAST(cd.Quantity AS DECIMAL(18,2)) - ISNULL((SELECT SUM(CAST(id.Quantity AS DECIMAL(18,2))) FROM tbl_Invoice_details id INNER JOIN tbl_Invoice inv ON id.Invoice_No = inv.Invoice_No AND id.CompanyID = inv.CompanyID WHERE id.Quotation_no = cd.Challan_no AND id.Product_id = cd.Product_id AND id.CompanyID = @CompanyID AND inv.status2 = 'Active'), 0)) AS PendingQty,
                    ISNULL(qd.sail_rate, 0) AS sail_rate, ISNULL(qd.discount_rate, 0) AS discountRate, ISNULL(qd.Service_tax_rate, 0) AS Service_tax_rate, ISNULL(qd.specification, '') AS specification, ISNULL(np.Quantity, '0') AS AvailableStock,
                    '' AS ItemNo, '' AS MaterialNo, '' AS PackSize, '' AS Unit, '' AS DeliveryDate, '' AS Department, '' AS ItemRemarks
                FROM tbl_Challan_details cd
                LEFT JOIN tbl_Chalan ch ON cd.Challan_no = ch.Chalan_No AND ch.CompanyID = @CompanyID
                LEFT JOIN tbl_Quotaion_details qd ON ch.Quotation_No = qd.Quotation_no AND qd.Product_Code = cd.Product_id AND qd.CompanyID = @CompanyID
                LEFT JOIN tbl_NewProduct np ON np.ProductID = cd.Product_id AND np.CompanyID = @CompanyID
                WHERE cd.Challan_no = @Ref AND cd.CompanyID = @CompanyID";
        }
        #endregion

        #region STEP 2: PRODUCT MANAGEMENT, BULK/SINGLE DELETE & UNDO
        private void BindProductsGrid()
        {
            DataTable dtActive = ViewState["InvoiceItems"] as DataTable;
            DataTable dtRemoved = ViewState["RemovedItems"] as DataTable;

            GridView1.DataSource = dtActive;
            GridView1.DataBind();

            // Update item counters in header
            lblActiveCount.Text = dtActive != null ? dtActive.Rows.Count.ToString() : "0";
            lblRemovedCount.Text = dtRemoved != null ? dtRemoved.Rows.Count.ToString() : "0";

            ScriptManager.RegisterStartupScript(this, GetType(), "calc", "setTimeout(function(){ var rows=document.getElementById('" + GridView1.ClientID + "').getElementsByTagName('tr'); for(var i=1;i<rows.length;i++){ var t=rows[i].querySelector(\"input[id*='txtqnty']\"); if(t) CalculateRow(t,'MAIN'); } }, 500);", true);
        }

        protected void gvGrid1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "RemoveItem")
            {
                int index = Convert.ToInt32(e.CommandArgument);
                DataTable dt = (DataTable)ViewState["InvoiceItems"];
                SyncGridToTable(dt);

                DataTable dtRemoved = ViewState["RemovedItems"] as DataTable ?? dt.Clone();
                dtRemoved.ImportRow(dt.Rows[index]);
                ViewState["RemovedItems"] = dtRemoved;

                dt.Rows[index].Delete();
                dt.AcceptChanges();
                ViewState["InvoiceItems"] = dt;

                BindProductsGrid();
                btnRestore.Visible = true;
            }
        }

        protected void btnRemoveBulk_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)ViewState["InvoiceItems"];
            if (dt == null) return;

            SyncGridToTable(dt);

            DataTable dtRemoved = ViewState["RemovedItems"] as DataTable ?? dt.Clone();
            bool itemsRemoved = false;

            for (int i = GridView1.Rows.Count - 1; i >= 0; i--)
            {
                GridViewRow row = GridView1.Rows[i];
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked)
                {
                    dtRemoved.ImportRow(dt.Rows[i]);
                    dt.Rows[i].Delete();
                    itemsRemoved = true;
                }
            }

            if (itemsRemoved)
            {
                ViewState["RemovedItems"] = dtRemoved;
                dt.AcceptChanges();
                ViewState["InvoiceItems"] = dt;

                BindProductsGrid();
                btnRestore.Visible = true;
                ShowMsg("Selected items removed. You can undo this action if needed.", true);
            }
            else
            {
                ShowMsg("Please check the box next to at least one item to remove.", false);
            }
        }

        // NEW: Instantly clean up all rows where Bill Qty is 0
        protected void btnRemoveZeroQty_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)ViewState["InvoiceItems"];
            if (dt == null) return;

            // 1. Sync any typing the user just did before cleaning
            SyncGridToTable(dt);

            DataTable dtRemoved = ViewState["RemovedItems"] as DataTable ?? dt.Clone();
            bool itemsRemoved = false;
            int removedCount = 0;

            // 2. Loop backwards to safely delete multiple rows
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                decimal qty = 0;
                if (dt.Rows[i]["PendingQty"] != DBNull.Value)
                {
                    decimal.TryParse(dt.Rows[i]["PendingQty"].ToString(), out qty);
                }

                // If Bill Qty is 0 (or somehow negative), move it to the recycle bin
                if (qty <= 0)
                {
                    dtRemoved.ImportRow(dt.Rows[i]);
                    dt.Rows[i].Delete();
                    itemsRemoved = true;
                    removedCount++;
                }
            }

            // 3. Process the cleanup
            if (itemsRemoved)
            {
                ViewState["RemovedItems"] = dtRemoved; // Save to Undo memory
                dt.AcceptChanges();
                ViewState["InvoiceItems"] = dt;

                BindProductsGrid();
                btnRestore.Visible = true; // Show the Undo button
                ShowMsg($"Cleaned up {removedCount} zero-quantity items.", true);
            }
            else
            {
                ShowMsg("Grid is already clean! No zero-quantity items found.", false);
            }
        }

        protected void btnRestore_Click(object sender, EventArgs e)
        {
            DataTable dtActive = (DataTable)ViewState["InvoiceItems"];
            DataTable dtRemoved = (DataTable)ViewState["RemovedItems"];

            if (dtActive != null && dtRemoved != null && dtRemoved.Rows.Count > 0)
            {
                SyncGridToTable(dtActive);

                foreach (DataRow row in dtRemoved.Rows)
                {
                    dtActive.ImportRow(row);
                }

                ViewState["InvoiceItems"] = dtActive;
                ViewState["RemovedItems"] = null;

                BindProductsGrid();
                btnRestore.Visible = false;
                ShowMsg("Restored items successfully!", true);
            }
        }

        private void SyncGridToTable(DataTable dt)
        {
            for (int i = 0; i < GridView1.Rows.Count; i++)
            {
                GridViewRow row = GridView1.Rows[i];
                TextBox tQty = (TextBox)row.FindControl("txtqnty");
                TextBox tRate = (TextBox)row.FindControl("txtsailrate");
                TextBox tDisc = (TextBox)row.FindControl("txtDiscPer");
                TextBox tSpec = (TextBox)row.FindControl("txtdes");

                if (tQty != null && i < dt.Rows.Count)
                {
                    decimal q = 0, r = 0, d = 0;
                    decimal.TryParse(tQty.Text, out q);
                    decimal.TryParse(tRate.Text, out r);
                    decimal.TryParse(tDisc.Text, out d);

                    dt.Rows[i]["PendingQty"] = q;
                    dt.Rows[i]["sail_rate"] = r;
                    dt.Rows[i]["discountRate"] = d;
                    dt.Rows[i]["specification"] = tSpec.Text;
                }
            }
        }

        protected void btnBackSetup_Click(object sender, EventArgs e) { mvInvoice.ActiveViewIndex = 0; }
        #endregion

        #region FINAL SAVE TO DB
        protected void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                SyncGridToTable((DataTable)ViewState["InvoiceItems"]);

                if (string.IsNullOrEmpty(txtExtInvoiceDate.Text)) { ShowMsg("Action Blocked: Please provide an Ext. ERP Date.", false); return; }

                string uid = Session["USERID"] != null ? Session["USERID"].ToString() : "System";
                string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

                string docType = ViewState["SelectedDocType"]?.ToString() ?? "Quotation";
                string selNo = ViewState["SelectedDocNo"]?.ToString();
                string refNo = string.IsNullOrEmpty(selNo) ? "N/A" : selNo;

                decimal gGross = 0, gDisc = 0, gTax = 0, gNet = 0;

                foreach (GridViewRow row in GridView1.Rows)
                {
                    decimal q = 0, r = 0, dPer = 0, tPer = 0;
                    decimal.TryParse(((TextBox)row.FindControl("txtqnty")).Text, out q);

                    if (q <= 0) continue;

                    decimal.TryParse(((TextBox)row.FindControl("txtsailrate")).Text, out r);
                    decimal.TryParse(((TextBox)row.FindControl("txtDiscPer")).Text, out dPer);
                    decimal.TryParse(((Label)row.FindControl("lblGstRate")).Text, out tPer);

                    decimal rowGross = Math.Round(q * r, 2);
                    decimal rowDisc = Math.Round((rowGross * dPer) / 100, 2);
                    decimal taxable = Math.Round(rowGross - rowDisc, 2);
                    decimal rowTax = Math.Round((taxable * tPer) / 100, 2);

                    gGross += rowGross; gDisc += rowDisc; gTax += rowTax; gNet += (taxable + rowTax);
                }

                decimal frt = 0, oth = 0;
                decimal.TryParse(txt_delivery_amnt.Text, out frt);
                decimal.TryParse(txt_othr_amnt.Text, out oth);

                gNet += Math.Round(frt, 2) + Math.Round(oth, 2);
                gNet = Math.Round(gNet, 2);

                if (gNet <= 0) return;

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

                        DateTime dt = DateTime.Parse(txtinvoiceDate.Text);
                        string yy = dt.Month >= 4 ? dt.Year.ToString().Substring(2) + "-" + (dt.Year + 1).ToString().Substring(2) : (dt.Year - 1).ToString().Substring(2) + "-" + dt.Year.ToString().Substring(2);
                        string invNo = "INV/C/" + yy + "/" + slNo;

                        string intra = RadioButtonGst.SelectedIndex == 0 ? "YES" : "";
                        string inter = RadioButtonGst.SelectedIndex == 1 ? "YES" : "";

                        string sqlH = "INSERT INTO tbl_Invoice (Invoice_No, Invoice_Date, Quotation_No, Client_ID, Gross, discount, sub_total, Service_Tax1, Net_Amount, Sl_no, Delivery_Amount, otherAmount1_name, otherAmount1, status1, status2, cgstOrsgst, igst, AddedById, CompanyID, SalesPersonCode, ExtInvoiceNo, ExtInvoiceDate, BillingAddress) VALUES (@Inv, @Date, @PO, @CID, @Gr, @Di, @Sub, @Tax, @Net, @Sl, @Frt, @OthName, @Oth, 'No', 'Active', @Intra, @Inter, @User, @CompanyID, @SalesPerson, @ExtNo, @ExtDate, @BillingAddress)";
                        SqlCommand cmdH = new SqlCommand(sqlH, conn, tran);
                        cmdH.Parameters.AddWithValue("@Inv", invNo);
                        cmdH.Parameters.AddWithValue("@Date", txtinvoiceDate.Text);
                        cmdH.Parameters.AddWithValue("@PO", refNo);
                        cmdH.Parameters.AddWithValue("@CID", lblclientId.Text);
                        cmdH.Parameters.Add("@Gr", SqlDbType.Decimal).Value = gGross;
                        cmdH.Parameters.Add("@Di", SqlDbType.Decimal).Value = gDisc;
                        cmdH.Parameters.Add("@Sub", SqlDbType.Decimal).Value = Math.Round(gGross - gDisc, 2);
                        cmdH.Parameters.Add("@Tax", SqlDbType.Decimal).Value = gTax;
                        cmdH.Parameters.Add("@Net", SqlDbType.Decimal).Value = gNet;
                        cmdH.Parameters.AddWithValue("@Sl", slNo);
                        cmdH.Parameters.Add("@Frt", SqlDbType.Decimal).Value = frt;
                        cmdH.Parameters.AddWithValue("@OthName", TextBox1.Text.Trim());
                        cmdH.Parameters.Add("@Oth", SqlDbType.Decimal).Value = oth;
                        cmdH.Parameters.AddWithValue("@Intra", string.IsNullOrEmpty(intra) ? (object)DBNull.Value : intra);
                        cmdH.Parameters.AddWithValue("@Inter", string.IsNullOrEmpty(inter) ? (object)DBNull.Value : inter);
                        cmdH.Parameters.AddWithValue("@User", uid);
                        cmdH.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        cmdH.Parameters.AddWithValue("@SalesPerson", cmbSalesPerson.SelectedValue);
                        cmdH.Parameters.AddWithValue("@ExtNo", string.IsNullOrWhiteSpace(txtExtInvoiceNo.Text) ? (object)DBNull.Value : txtExtInvoiceNo.Text.Trim());
                        cmdH.Parameters.AddWithValue("@ExtDate", string.IsNullOrWhiteSpace(txtExtInvoiceDate.Text) ? (object)DBNull.Value : txtExtInvoiceDate.Text.Trim());
                        cmdH.Parameters.AddWithValue("@BillingAddress", List_BillingAddress.SelectedItem != null ? List_BillingAddress.SelectedItem.Text : "N/A");
                        cmdH.ExecuteNonQuery();

                        // 1. Grab the memory table to safely read underlying data (ignoring HTML formatting)
                        DataTable dtItems = (DataTable)ViewState["InvoiceItems"];

                        // 2. Use a standard FOR loop so we can match GridView rows exactly to memory rows
                        for (int i = 0; i < GridView1.Rows.Count; i++)
                        {
                            GridViewRow row = GridView1.Rows[i];
                            DataRow memRow = dtItems.Rows[i]; // Matches exact row index

                            decimal q = 0, r = 0, dPer = 0, tPer = 0, totalRowDisc = 0;
                            decimal.TryParse(((TextBox)row.FindControl("txtqnty")).Text, out q);

                            if (q <= 0) continue;

                            decimal.TryParse(((TextBox)row.FindControl("txtsailrate")).Text, out r);
                            decimal.TryParse(((TextBox)row.FindControl("txtDiscPer")).Text, out dPer);
                            decimal.TryParse(((TextBox)row.FindControl("txtDiscAmt")).Text, out totalRowDisc); // NEW: Read exact amount from UI
                            decimal.TryParse(((Label)row.FindControl("lblGstRate")).Text, out tPer);

                            // SECURE MATH: Use the exact discount amount from the frontend
                            decimal rowGross = Math.Round(q * r, 2);
                            decimal taxable = Math.Round(rowGross - totalRowDisc, 2); // Avoids % rounding drift
                            decimal rowTax = Math.Round((taxable * tPer) / 100, 2);
                            decimal rowNet = Math.Round(taxable + rowTax, 2);

                            // Pull Product Info securely from Memory Table
                            string trueProductID = memRow["TrueID"].ToString();
                            string pname = memRow["Product_name"].ToString();
                            string hsnCode = memRow["TrueHSN"].ToString();
                            string spec = ((TextBox)row.FindControl("txtdes")).Text;

                            string sqlD = "INSERT INTO tbl_Invoice_details (Invoice_No, Quotation_no, Product_id, Product_Code, Product_name, Quantity, sail_rate, discountRate, Service_tax_rate, Total_sail_rate1, Total_sail_rate2, specification, AddedById, CompanyID) VALUES (@Inv, @RefNo, @PID, @HSN, @Name, @Qty, @Rate, @DPer, @TPer, @Net, @Base, @Brand, @User, @CompanyID)";

                            SqlCommand cmdD = new SqlCommand(sqlD, conn, tran);
                            cmdD.Parameters.AddWithValue("@Inv", invNo);
                            cmdD.Parameters.AddWithValue("@RefNo", refNo);
                            cmdD.Parameters.AddWithValue("@PID", trueProductID);
                            cmdD.Parameters.AddWithValue("@HSN", hsnCode);
                            cmdD.Parameters.AddWithValue("@Name", HttpUtility.HtmlDecode(pname));
                            cmdD.Parameters.Add("@Qty", SqlDbType.Decimal).Value = q;
                            cmdD.Parameters.Add("@Rate", SqlDbType.Decimal).Value = r;
                            cmdD.Parameters.Add("@DPer", SqlDbType.Decimal).Value = dPer; // Preserved for legacy records
                            cmdD.Parameters.Add("@TPer", SqlDbType.Decimal).Value = tPer;

                            // Standardizing the Columns: Net -> Rate1, Taxable Base -> Rate2
                            cmdD.Parameters.Add("@Net", SqlDbType.Decimal).Value = rowNet;
                            cmdD.Parameters.Add("@Base", SqlDbType.Decimal).Value = taxable;

                            cmdD.Parameters.AddWithValue("@Brand", spec);
                            cmdD.Parameters.AddWithValue("@User", uid);
                            cmdD.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                            cmdD.ExecuteNonQuery();

                            // Stock Deduction
                            if (docType != "Delivery Challan")
                            {
                                string sqlStock = "UPDATE tbl_NewProduct SET Quantity = CAST(CASE WHEN ISNULL(Quantity, '') = '' THEN '0' ELSE Quantity END AS DECIMAL(18,2)) - @Qty WHERE ProductID = @TruePID AND CompanyID = @CompanyID";
                                SqlCommand cmdS = new SqlCommand(sqlStock, conn, tran);
                                cmdS.Parameters.Add("@Qty", SqlDbType.Decimal).Value = q;
                                cmdS.Parameters.Add("@TruePID", SqlDbType.VarChar).Value = trueProductID;
                                cmdS.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                                cmdS.ExecuteNonQuery();
                            }
                        }

                        foreach (ListItem itm in List_BillingAddress.Items)
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

                        // Save Shipping Address
                        foreach (ListItem itm in List_ShippingAddress.Items)
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

                        InsertSystemNotification("Tax Invoice Generated", $"Invoice #{invNo} created for {cmbvendor.SelectedItem.Text} from {docType} {refNo}.", "INVOICE", "Success", uid, CompanyContext.CurrentCompanyID, conn, tran);

                        tran.Commit();
                        ShowMsg("Success! Invoice Generated: " + invNo, true);

                        ViewState["InvoiceItems"] = null;
                        ViewState["RemovedItems"] = null;
                        GridView1.DataSource = null;
                        GridView1.DataBind();
                        txt_delivery_amnt.Text = "0"; txt_othr_amnt.Text = "0"; TextBox1.Text = ""; cmbSalesPerson.SelectedIndex = -1; txtExtInvoiceNo.Text = ""; txtExtInvoiceDate.Text = "";
                        btnRestore.Visible = false;
                        mvInvoice.ActiveViewIndex = 0;
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback(); throw ex;
                    }
                }
            }
            catch (Exception ex) { ShowMsg("Error: " + ex.Message, false); }
        }

        private void InsertSystemNotification(string title, string message, string module, string type, string userId, int companyId, SqlConnection conn, SqlTransaction tran)
        {
            // Mapped exactly to your provided SQL schema
            string sql = @"INSERT INTO tbl_SystemNotification 
                           (Title, Message, ModuleCode, Severity, StartDate, EndDate, IsActive, CreatedBy, CompanyID) 
                           VALUES 
                           (@Title, @Msg, @Mod, @Type, GETDATE(), DATEADD(day, 30, GETDATE()), 1, @User, @Comp)";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);
            cmd.Parameters.AddWithValue("@Title", title);
            cmd.Parameters.AddWithValue("@Msg", message);
            cmd.Parameters.AddWithValue("@Mod", module);
            cmd.Parameters.AddWithValue("@Type", type); // Maps to 'Severity'
            cmd.Parameters.AddWithValue("@User", userId);
            cmd.Parameters.AddWithValue("@Comp", companyId);

            cmd.ExecuteNonQuery();
        }

        private void ShowMsg(string msg, bool isSuccess)
        {
            if (string.IsNullOrEmpty(msg)) return;
            string icon = isSuccess ? "success" : "error";

            // FIX: Strip out quotes and line breaks so SQL exceptions don't break JavaScript execution
            string cleanMsg = msg.Replace("'", "\\'").Replace("\r", " ").Replace("\n", " ");
            int timer = isSuccess ? 4000 : 6000;

            // FIX: Rendered as a single line string literal to guarantee safe browser execution
            string script = "Swal.fire({ title: '" + cleanMsg + "', icon: '" + icon + "', toast: true, position: 'top-end', showConfirmButton: false, timer: " + timer + " });";

            ScriptManager.RegisterStartupScript(this, GetType(), "swalMsg", script, true);
        }

        #region AJAX ITEM RECONCILIATION
        [WebMethod(EnableSession = true)]
        public static string GetReconciliation(string refNo, string productId)
        {
            if (HttpContext.Current.Session["USERID"] == null) return "<div style='color:red;'>Session Expired. Please reload.</div>";

            int companyId = CompanyContext.CurrentCompanyID;

            StringBuilder html = new StringBuilder();
            html.Append("<table style='width:100%; font-size:13px; text-align:left; border-collapse:collapse;'>");
            html.Append("<thead><tr><th style='background:#006699; color:white; padding:8px; border:1px solid #ddd;'>Invoice No</th><th style='background:#006699; color:white; padding:8px; border:1px solid #ddd;'>Date</th><th style='background:#006699; color:white; padding:8px; text-align:right; border:1px solid #ddd;'>Qty Billed</th></tr></thead>");
            html.Append("<tbody>");

            decimal totalActiveQty = 0;

            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                // Added h.status2 to explicitly fetch the status
                string sql = @"SELECT h.Invoice_No, CONVERT(varchar, h.Invoice_Date, 106) as InvDate, CAST(d.Quantity AS DECIMAL(18,2)) as Qty, h.status2 
                               FROM tbl_Invoice_details d
                               INNER JOIN tbl_Invoice h ON d.Invoice_No = h.Invoice_No AND d.CompanyID = h.CompanyID
                               WHERE d.Quotation_no = @RefNo AND d.Product_id = @PID AND d.CompanyID = @CompID
                               ORDER BY h.ID ASC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RefNo", refNo);
                    cmd.Parameters.AddWithValue("@PID", productId);
                    cmd.Parameters.AddWithValue("@CompID", companyId);

                    conn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        bool hasData = false;
                        while (dr.Read())
                        {
                            hasData = true;
                            decimal q = Convert.ToDecimal(dr["Qty"]);
                            string status = dr["status2"].ToString();

                            string trStyle = "";
                            string statusBadge = "";

                            // Determine if Invoice is Valid or Cancelled
                            if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                            {
                                totalActiveQty += q;
                            }
                            else
                            {
                                trStyle = "text-decoration: line-through; color: #999; background: #fdfdfd;";
                                statusBadge = " <span style='background:#dc3545; color:white; padding:2px 4px; border-radius:3px; font-size:9px;'>Cancelled</span>";
                            }

                            html.AppendFormat("<tr style='{0}'><td style='padding:8px; border-bottom:1px solid #eee;'><strong>{1}</strong>{4}</td><td style='padding:8px; border-bottom:1px solid #eee;'>{2}</td><td style='padding:8px; border-bottom:1px solid #eee; text-align:right; color:#dc3545; font-weight:bold;'>{3}</td></tr>",
                                trStyle, dr["Invoice_No"], dr["InvDate"], q, statusBadge);
                        }

                        if (!hasData)
                        {
                            html.Append("<tr><td colspan='3' style='text-align:center; padding:15px; color:#666;'>No previous billing history found for this item.</td></tr>");
                        }
                    }
                }
            }

            if (totalActiveQty > 0)
            {
                html.AppendFormat("<tr style='background:#f8fafc;'><td colspan='2' style='padding:8px; text-align:right; font-weight:bold;'>Valid Active Total:</td><td style='padding:8px; text-align:right; font-weight:bold; color:#dc3545; font-size:15px;'>{0}</td></tr>", totalActiveQty);
            }

            html.Append("</tbody></table>");
            return html.ToString();
        }
        #endregion
        #endregion
    }
}