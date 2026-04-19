using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web;
using System.Collections.Generic;

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
                List_SiteAddress.Items.Clear();
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
            List_SiteAddress.Items.Clear();
            SqlCommand cmd1 = new SqlCommand("SELECT Address1+', '+City+', '+pin+', '+State FROM tbl_Client WHERE Client_Id=@CID AND CompanyID=@CompanyID", conn);
            cmd1.Parameters.AddWithValue("@CID", cid);
            cmd1.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
            using (SqlDataReader dr = cmd1.ExecuteReader()) { while (dr.Read()) List_SiteAddress.Items.Add(dr[0].ToString()); }

            SqlCommand cmd2 = new SqlCommand("SELECT Address+', '+State+', '+City+', '+pin FROM tbl_ClientRegAddress WHERE Client_Id=@CID AND CompanyID=@CompanyID", conn);
            cmd2.Parameters.AddWithValue("@CID", cid);
            cmd2.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
            using (SqlDataReader dr = cmd2.ExecuteReader()) { while (dr.Read()) List_SiteAddress.Items.Add(dr[0].ToString()); }

            pnlAddress.Visible = true;
            if (List_SiteAddress.Items.Count > 0) List_SiteAddress.Items[0].Selected = true;
        }
        #endregion

        #region SMART SEARCH & GRID BINDING
        protected void btnSertch_Click_OLD(object sender, EventArgs e)
        {
            ShowMsg("", true);
            string baseQuery = @"
                SELECT TOP 100
                    STRING_AGG(tbl_QuoPriSerTogather.PServiceName, ', ') AS PServiceName,
                    tbl_Quotation.ID, 
                    tbl_Quotation.service_tax1, 
                    tbl_Quotation.sub_total, 
                    tbl_Quotation.DO_Number, 
                    tbl_Quotation.PO_Number, 
                    tbl_Quotation.Quotation_no, 
                    tbl_Quotation.Quotation_date, 
                    tbl_Quotation.Gross, 
                    tbl_Quotation.Service_tax, 
                    tbl_Quotation.Net_amount, 
                    tbl_Quotation.mailStatusDate, 
                    ISNULL(tbl_Client.Client_Name, 'Unknown') AS Client_Name 
                FROM tbl_Quotation
                LEFT OUTER JOIN tbl_Client ON tbl_Quotation.Client_Id = tbl_Client.Client_Id 
                LEFT OUTER JOIN tbl_QuoPriSerTogather ON tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no
                WHERE tbl_Quotation.CompanyID = @CompanyID";

            List<string> filters = new List<string>();

            if (cmbvendor.SelectedIndex > 0)
                filters.Add("tbl_Quotation.Client_Id = @CID");

            DateTime fromDate, toDate;
            bool hasFrom = DateTime.TryParse(txtfromDate.Text, out fromDate);
            bool hasTo = DateTime.TryParse(txttodate.Text, out toDate);

            if (hasFrom && hasTo) filters.Add("CAST(tbl_Quotation.Quotation_date AS DATE) >= @From AND CAST(tbl_Quotation.Quotation_date AS DATE) <= @To");
            else if (hasFrom) filters.Add("CAST(tbl_Quotation.Quotation_date AS DATE) >= @From");
            else if (hasTo) filters.Add("CAST(tbl_Quotation.Quotation_date AS DATE) <= @To");

            string whereClause = filters.Count > 0 ? " AND " + string.Join(" AND ", filters) : "";

            string groupAndOrder = @"
                GROUP BY 
                    tbl_Quotation.ID, tbl_Quotation.service_tax1, tbl_Quotation.sub_total, tbl_Quotation.DO_Number, 
                    tbl_Quotation.PO_Number, tbl_Quotation.Quotation_no, tbl_Quotation.Quotation_date, 
                    tbl_Quotation.Gross, tbl_Quotation.Service_tax, tbl_Quotation.Net_amount, 
                    tbl_Quotation.mailStatusDate, tbl_Client.Client_Name
                ORDER BY tbl_Quotation.ID DESC";

            string finalQuery = baseQuery + whereClause + groupAndOrder;

            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(finalQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    if (cmbvendor.SelectedIndex > 0) cmd.Parameters.AddWithValue("@CID", lblclientId.Text);
                    if (hasFrom) cmd.Parameters.AddWithValue("@From", fromDate);
                    if (hasTo) cmd.Parameters.AddWithValue("@To", toDate);

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

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            ShowMsg("", true);
            string docType = ddlDocType.SelectedValue;
            string qry = "";
            string colDate = "";

            // 1. ROUTE TO THE CORRECT TABLE AND COLUMN NAMES
            if (docType == "Quotation" || docType == "Purchase Order")
            {
                colDate = "Quotation_date";
                string recordType = docType == "Quotation" ? "Quotation" : "Purchase Order";
                qry = $@"SELECT TOP 100 t.Quotation_no AS DocNo, t.Quotation_date AS DocDate, ISNULL(c.Client_Name, 'Unknown') AS Client_Name, t.Net_amount 
                         FROM tbl_Quotation t LEFT JOIN tbl_Client c ON t.Client_Id = c.Client_Id 
                         WHERE t.CompanyID = @CompanyID AND t.RecordType = '{recordType}'";
            }
            else if (docType == "Delivery Challan")
            {
                colDate = "Chalan_Date";
                // Challans don't store Net_Amount, so we return 0.00 for the UI grid
                qry = $@"SELECT TOP 100 t.Chalan_No AS DocNo, t.Chalan_Date AS DocDate, ISNULL(c.Client_Name, 'Unknown') AS Client_Name, 0.00 AS Net_amount 
                         FROM tbl_Chalan t LEFT JOIN tbl_Client c ON t.Client_ID = c.Client_Id 
                         WHERE t.CompanyID = @CompanyID";
            }
            else if (docType == "Proforma")
            {
                colDate = "Invoice_Date";
                qry = $@"SELECT TOP 100 t.Invoice_No AS DocNo, t.Invoice_Date AS DocDate, ISNULL(c.Client_Name, 'Unknown') AS Client_Name, CAST(ISNULL(t.Net_Amount, '0') AS DECIMAL(18,2)) AS Net_amount 
                         FROM tbl_Proforma t LEFT JOIN tbl_Client c ON t.Client_ID = c.Client_Id 
                         WHERE t.CompanyID = @CompanyID";
            }

            // 2. APPLY FILTERS
            if (cmbvendor.SelectedIndex > 0) qry += " AND t.Client_Id = @CID"; // Assuming all tables have Client_ID or Client_Id

            DateTime fromDate, toDate;
            bool hasFrom = DateTime.TryParse(txtfromDate.Text, out fromDate);
            bool hasTo = DateTime.TryParse(txttodate.Text, out toDate);

            if (hasFrom && hasTo) qry += $" AND CAST(t.{colDate} AS DATE) >= @From AND CAST(t.{colDate} AS DATE) <= @To";
            else if (hasFrom) qry += $" AND CAST(t.{colDate} AS DATE) >= @From";
            else if (hasTo) qry += $" AND CAST(t.{colDate} AS DATE) <= @To";

            qry += $" ORDER BY CAST(t.{colDate} AS DATE) DESC";

            // 3. EXECUTE
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(qry, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    if (cmbvendor.SelectedIndex > 0) cmd.Parameters.AddWithValue("@CID", lblclientId.Text);
                    if (hasFrom) cmd.Parameters.AddWithValue("@From", fromDate);
                    if (hasTo) cmd.Parameters.AddWithValue("@To", toDate);

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

        protected void gvSearchDocs_RowCommand_OLD(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "SelectDoc")
            {
                GridViewRow row = (GridViewRow)(((Button)e.CommandSource).NamingContainer);
                string clientName = HttpUtility.HtmlDecode(row.Cells[2].Text);

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
                        ShowMsg($"Client '{clientName}' not found in active list.", false); return;
                    }
                }

                if (cmbSalesPerson.SelectedIndex <= 0) { ShowMsg("Please select a Sales Person before proceeding.", false); return; }
                if (List_SiteAddress.Items.Count == 0) { ShowMsg("No address found for this client.", false); return; }
                if (List_SiteAddress.SelectedIndex == -1) List_SiteAddress.SelectedIndex = 0;

                string docNo = e.CommandArgument.ToString();
                ViewState["SelectedDocNo"] = docNo;

                // Load items directly from Quotation Details AND fetch live stock via LEFT JOIN
                lblRefDoc.Text = $"QUOTATION NO: {docNo}";
                using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    string query = @"
                        SELECT 
                            qd.Product_id, 
                            qd.Product_Code, 
                            qd.Product_name, 
                            CAST(qd.Quantity AS DECIMAL(18,2)) AS QuotedQty,
                            
                            -- Calculate how many of this exact item have already been invoiced against this Quote
                            ISNULL((SELECT SUM(CAST(Quantity AS DECIMAL(18,2))) 
                                    FROM tbl_Invoice_details id 
                                    WHERE id.Quotation_no = qd.Quotation_no 
                                    AND id.Product_Code = qd.Product_Code 
                                    AND id.CompanyID = @CompanyID), 0) AS InvoicedQty,
                            
                            -- Pending = Quoted - Invoiced
                            (CAST(qd.Quantity AS DECIMAL(18,2)) - 
                             ISNULL((SELECT SUM(CAST(Quantity AS DECIMAL(18,2))) 
                                     FROM tbl_Invoice_details id 
                                     WHERE id.Quotation_no = qd.Quotation_no 
                                     AND id.Product_Code = qd.Product_Code 
                                     AND id.CompanyID = @CompanyID), 0)) AS PendingQty,

                            qd.sail_rate, 
                            qd.discount_rate AS discountRate, 
                            qd.Service_tax_rate, 
                            qd.specification,
                            ISNULL(np.Quantity, '0') AS AvailableStock
                        FROM tbl_Quotaion_details qd
                        LEFT JOIN tbl_NewProduct np ON qd.Product_Code = np.ProductID AND np.CompanyID = @CompanyID
                        WHERE qd.Quotation_no = @Ref AND qd.CompanyID = @CompanyID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Ref", docNo);
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            sda.Fill(dt);
                            ViewState["InvoiceItems"] = dt;
                            BindProductsGrid();
                        }
                    }
                }
                // Call this right before switching the view!
                BindInvoiceHistory(docNo);

                ShowMsg("", true);
                mvInvoice.ActiveViewIndex = 1;
            }
        }

        protected void gvSearchDocs_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "SelectDoc")
            {
                GridViewRow row = (GridViewRow)(((Button)e.CommandSource).NamingContainer);
                string clientName = HttpUtility.HtmlDecode(row.Cells[2].Text);

                if (cmbvendor.SelectedItem == null || cmbvendor.SelectedItem.Text.Trim() != clientName.Trim())
                {
                    ListItem item = cmbvendor.Items.FindByText(clientName);
                    if (item == null) { foreach (ListItem li in cmbvendor.Items) { if (li.Text.Trim().Equals(clientName.Trim(), StringComparison.OrdinalIgnoreCase)) { item = li; break; } } }
                    if (item != null) { cmbvendor.ClearSelection(); item.Selected = true; LoadClientDataByName(item.Text); }
                    else { ShowMsg($"Client '{clientName}' not found in active list.", false); return; }
                }

                //if (cmbSalesPerson.SelectedIndex <= 0) { ShowMsg("Please select a Sales Person before proceeding.", false); return; }
                if (List_SiteAddress.Items.Count == 0) { ShowMsg("No address found for this client.", false); return; }
                if (List_SiteAddress.SelectedIndex == -1) List_SiteAddress.SelectedIndex = 0;

                string docNo = e.CommandArgument.ToString();
                string docType = ddlDocType.SelectedValue;

                ViewState["SelectedDocNo"] = docNo;
                ViewState["SelectedDocType"] = docType;
                lblRefDoc.Text = $"{docType.ToUpper()} NO: {docNo}";

                using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    string query = "";

                    if (docType == "Quotation" || docType == "Purchase Order")
                    {
                        // MAPPING: tbl_Quotaion_details -> Product_Code = ID, Product_id = HSN
                        query = @"
                            SELECT 
                                qd.Product_Code AS TrueID, 
                                qd.Product_id AS TrueHSN, 
                                qd.Product_name, 
                                CAST(qd.Quantity AS DECIMAL(18,2)) AS QuotedQty,
                                ISNULL((SELECT SUM(CAST(Quantity AS DECIMAL(18,2))) FROM tbl_Invoice_details id WHERE id.Quotation_no = qd.Quotation_no AND id.Product_id = qd.Product_Code AND id.CompanyID = @CompanyID), 0) AS InvoicedQty,
                                (CAST(qd.Quantity AS DECIMAL(18,2)) - ISNULL((SELECT SUM(CAST(Quantity AS DECIMAL(18,2))) FROM tbl_Invoice_details id WHERE id.Quotation_no = qd.Quotation_no AND id.Product_id = qd.Product_Code AND id.CompanyID = @CompanyID), 0)) AS PendingQty,
                                qd.sail_rate, qd.discount_rate AS discountRate, qd.Service_tax_rate, qd.specification, ISNULL(np.Quantity, '0') AS AvailableStock
                            FROM tbl_Quotaion_details qd
                            LEFT JOIN tbl_NewProduct np ON np.ProductID = qd.Product_Code AND np.CompanyID = @CompanyID
                            WHERE qd.Quotation_no = @Ref AND qd.CompanyID = @CompanyID";
                    }
                    else if (docType == "Proforma")
                    {
                        // MAPPING: tbl_Proforma_Details -> Product_id = ID, Product_Code = HSN
                        query = @"
                            SELECT 
                                pd.Product_id AS TrueID, 
                                pd.Product_Code AS TrueHSN, 
                                pd.Product_name, 
                                CAST(pd.Quantity AS DECIMAL(18,2)) AS QuotedQty,
                                ISNULL((SELECT SUM(CAST(Quantity AS DECIMAL(18,2))) FROM tbl_Invoice_details id WHERE id.Quotation_no = pd.Invoice_No AND id.Product_id = pd.Product_id AND id.CompanyID = @CompanyID), 0) AS InvoicedQty,
                                (CAST(pd.Quantity AS DECIMAL(18,2)) - ISNULL((SELECT SUM(CAST(Quantity AS DECIMAL(18,2))) FROM tbl_Invoice_details id WHERE id.Quotation_no = pd.Invoice_No AND id.Product_id = pd.Product_id AND id.CompanyID = @CompanyID), 0)) AS PendingQty,
                                pd.Rate AS sail_rate, 0 AS discountRate, pd.Tax_Rate AS Service_tax_rate, pd.ProductOrServiceCat AS specification, ISNULL(np.Quantity, '0') AS AvailableStock
                            FROM tbl_Proforma_Details pd
                            LEFT JOIN tbl_NewProduct np ON np.ProductID = pd.Product_id AND np.CompanyID = @CompanyID
                            WHERE pd.Invoice_No = @Ref AND pd.CompanyID = @CompanyID";
                    }
                    else if (docType == "Delivery Challan")
                    {
                        // MAPPING: tbl_Challan_details -> Product_id = ID, Product_code = HSN
                        query = @"
                            SELECT 
                                cd.Product_id AS TrueID, 
                                cd.Product_code AS TrueHSN, 
                                cd.Product_name, 
                                CAST(cd.Quantity AS DECIMAL(18,2)) AS QuotedQty,
                                ISNULL((SELECT SUM(CAST(Quantity AS DECIMAL(18,2))) FROM tbl_Invoice_details id WHERE id.Quotation_no = cd.Challan_no AND id.Product_id = cd.Product_id AND id.CompanyID = @CompanyID), 0) AS InvoicedQty,
                                (CAST(cd.Quantity AS DECIMAL(18,2)) - ISNULL((SELECT SUM(CAST(Quantity AS DECIMAL(18,2))) FROM tbl_Invoice_details id WHERE id.Quotation_no = cd.Challan_no AND id.Product_id = cd.Product_id AND id.CompanyID = @CompanyID), 0)) AS PendingQty,
                                ISNULL(qd.sail_rate, 0) AS sail_rate, ISNULL(qd.discount_rate, 0) AS discountRate, ISNULL(qd.Service_tax_rate, 0) AS Service_tax_rate, ISNULL(qd.specification, '') AS specification, ISNULL(np.Quantity, '0') AS AvailableStock
                            FROM tbl_Challan_details cd
                            LEFT JOIN tbl_Chalan ch ON cd.Challan_no = ch.Chalan_No AND ch.CompanyID = @CompanyID
                            LEFT JOIN tbl_Quotaion_details qd ON ch.Quotation_No = qd.Quotation_no AND qd.Product_Code = cd.Product_id AND qd.CompanyID = @CompanyID
                            LEFT JOIN tbl_NewProduct np ON np.ProductID = cd.Product_id AND np.CompanyID = @CompanyID
                            WHERE cd.Challan_no = @Ref AND cd.CompanyID = @CompanyID";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Ref", docNo);
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            sda.Fill(dt);
                            ViewState["InvoiceItems"] = dt;
                            BindProductsGrid();
                        }
                    }
                }

                BindInvoiceHistory(docNo);
                ShowMsg("", true);
                mvInvoice.ActiveViewIndex = 1;
            }
        }
        #endregion

        #region STEP 2: PRODUCT MANAGEMENT & FINAL WIZARD
        private void BindProductsGrid()
        {
            DataTable dt = (DataTable)ViewState["InvoiceItems"];
            GridView1.DataSource = dt;
            GridView1.DataBind();
            ScriptManager.RegisterStartupScript(this, GetType(), "calc", "setTimeout(function(){ var rows=document.getElementById('" + GridView1.ClientID + "').getElementsByTagName('tr'); for(var i=1;i<rows.length;i++){ var t=rows[i].querySelector(\"input[id*='txtqnty']\"); if(t) CalculateRow(t,'MAIN'); } }, 500);", true);
        }

        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "RemoveItem")
            {
                int index = Convert.ToInt32(e.CommandArgument);
                DataTable dt = (DataTable)ViewState["InvoiceItems"];
                SyncGridToTable(dt);
                dt.Rows[index].Delete();
                dt.AcceptChanges();
                ViewState["InvoiceItems"] = dt;
                BindProductsGrid();
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

                    // FIX: Replaced "Quantity" with "PendingQty" to match the new Partial Invoicing schema
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
                // NEW: Validate Master Details BEFORE doing any math or saving
                if (cmbSalesPerson.SelectedIndex <= 0)
                {
                    ShowMsg("Action Blocked: Please select a Sales Person.", false);
                    return;
                }
                if (string.IsNullOrEmpty(txtinvoiceDate.Text))
                {
                    ShowMsg("Action Blocked: Please provide an Invoice Date.", false);
                    return;
                }

                SyncGridToTable((DataTable)ViewState["InvoiceItems"]);

                string uid = Session["USERID"] != null ? Session["USERID"].ToString() : "System";
                string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

                // Fetch the exact document type so we know how to handle physical stock
                string docType = ViewState["SelectedDocType"]?.ToString() ?? "Quotation";
                string selNo = ViewState["SelectedDocNo"]?.ToString();
                string refNo = string.IsNullOrEmpty(selNo) ? "N/A" : selNo;

                decimal gGross = 0, gDisc = 0, gTax = 0, gNet = 0;

                // LOOP 1: Calculate Totals
                foreach (GridViewRow row in GridView1.Rows)
                {
                    decimal q = 0, r = 0, dPer = 0, tPer = 0;
                    decimal.TryParse(((TextBox)row.FindControl("txtqnty")).Text, out q);

                    // OPTIMIZATION: Skip math entirely if quantity is 0
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

                // Server-Side Block for Zero Total
                if (gNet <= 0)
                {
                    ShowMsg("Action Blocked: Cannot save an invoice with a total of zero.", false);
                    return;
                }

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

                        // Insert Header (Treating Quotation_No as the universal Source Reference column)
                        string sqlH = "INSERT INTO tbl_Invoice (Invoice_No, Invoice_Date, Quotation_No, Client_ID, Gross, discount, sub_total, Service_Tax1, Net_Amount, Sl_no, Delivery_Amount, otherAmount1_name, otherAmount1, status1, status2, cgstOrsgst, igst, AddedById, CompanyID, SalesPersonCode) VALUES (@Inv, @Date, @PO, @CID, @Gr, @Di, @Sub, @Tax, @Net, @Sl, @Frt, @OthName, @Oth, 'No', 'Active', @Intra, @Inter, @User, @CompanyID, @SalesPerson)";
                        SqlCommand cmdH = new SqlCommand(sqlH, conn, tran);
                        cmdH.Parameters.AddWithValue("@Inv", invNo);
                        cmdH.Parameters.AddWithValue("@Date", txtinvoiceDate.Text);
                        cmdH.Parameters.AddWithValue("@PO", refNo); // Saving the universally captured refNo
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
                        cmdH.ExecuteNonQuery();

                        // LOOP 2: Insert Details & Deduct Stock
                        foreach (GridViewRow row in GridView1.Rows)
                        {
                            decimal q = 0, r = 0, dPer = 0, tPer = 0;
                            decimal.TryParse(((TextBox)row.FindControl("txtqnty")).Text, out q);

                            // Essential skip logic for partial invoicing
                            if (q <= 0) continue;

                            decimal.TryParse(((TextBox)row.FindControl("txtsailrate")).Text, out r);
                            decimal.TryParse(((TextBox)row.FindControl("txtDiscPer")).Text, out dPer);
                            decimal.TryParse(((Label)row.FindControl("lblGstRate")).Text, out tPer);

                            decimal rowGross = Math.Round(q * r, 2);
                            decimal rowDisc = Math.Round((rowGross * dPer) / 100, 2);
                            decimal taxable = Math.Round(rowGross - rowDisc, 2);
                            decimal rowTax = Math.Round((taxable * tPer) / 100, 2);
                            decimal rowNet = Math.Round(taxable + rowTax, 2);

                            // Pulling from the newly standardized UI columns
                            string trueProductID = row.Cells[0].Text; // Gets TrueID
                            string pname = row.Cells[1].Text;
                            string hsnCode = row.Cells[2].Text;       // Gets TrueHSN
                            string spec = ((TextBox)row.FindControl("txtdes")).Text;

                            // INSERT tbl_Invoice_details (Product_id = ID, Product_Code = HSN)
                            string sqlD = "INSERT INTO tbl_Invoice_details (Invoice_No, Quotation_no, Product_id, Product_Code, Product_name, Quantity, sail_rate, discountRate, Service_tax_rate, Total_sail_rate1, Total_sail_rate2, specification, AddedById, CompanyID) VALUES (@Inv, @RefNo, @PID, @HSN, @Name, @Qty, @Rate, @DPer, @TPer, @Net, @Base, @Brand, @User, @CompanyID)";

                            SqlCommand cmdD = new SqlCommand(sqlD, conn, tran);
                            cmdD.Parameters.AddWithValue("@Inv", invNo);
                            cmdD.Parameters.AddWithValue("@RefNo", refNo);
                            cmdD.Parameters.AddWithValue("@PID", trueProductID); // Maps TrueID -> Product_id
                            cmdD.Parameters.AddWithValue("@HSN", hsnCode);
                            cmdD.Parameters.AddWithValue("@Name", HttpUtility.HtmlDecode(pname));
                            cmdD.Parameters.Add("@Qty", SqlDbType.Decimal).Value = q;
                            cmdD.Parameters.Add("@Rate", SqlDbType.Decimal).Value = r;
                            cmdD.Parameters.Add("@DPer", SqlDbType.Decimal).Value = dPer;
                            cmdD.Parameters.Add("@TPer", SqlDbType.Decimal).Value = tPer;
                            cmdD.Parameters.Add("@Net", SqlDbType.Decimal).Value = rowNet;
                            cmdD.Parameters.Add("@Base", SqlDbType.Decimal).Value = taxable;
                            cmdD.Parameters.AddWithValue("@Brand", spec);
                            cmdD.Parameters.AddWithValue("@User", uid);
                            cmdD.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                            cmdD.ExecuteNonQuery();

                            // UPDATE tbl_NewProduct (ProductID = ID)
                            if (docType != "Delivery Challan")
                            {
                                string sqlStock = "UPDATE tbl_NewProduct SET Quantity = CAST(CASE WHEN ISNULL(Quantity, '') = '' THEN '0' ELSE Quantity END AS DECIMAL(18,2)) - @Qty WHERE ProductID = @TruePID AND CompanyID = @CompanyID";
                                SqlCommand cmdS = new SqlCommand(sqlStock, conn, tran);
                                cmdS.Parameters.Add("@Qty", SqlDbType.Decimal).Value = q;
                                cmdS.Parameters.Add("@TruePID", SqlDbType.VarChar).Value = trueProductID; // Maps TrueID -> ProductID
                                cmdS.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                                cmdS.ExecuteNonQuery();
                            }
                        }

                        foreach (ListItem itm in List_SiteAddress.Items)
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

                        ViewState["InvoiceItems"] = null; GridView1.DataSource = null; GridView1.DataBind();
                        txt_delivery_amnt.Text = "0"; txt_othr_amnt.Text = "0"; TextBox1.Text = ""; cmbSalesPerson.SelectedIndex = -1;
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
            try
            {
                string sql = "INSERT INTO tbl_SystemNotification (Title, Message, Module, Type, UserID, CreatedDate, IsRead, CompanyID) VALUES (@Title, @Msg, @Mod, @Type, @User, GETDATE(), 0, @Comp)";
                SqlCommand cmd = new SqlCommand(sql, conn, tran);
                cmd.Parameters.AddWithValue("@Title", title); cmd.Parameters.AddWithValue("@Msg", message);
                cmd.Parameters.AddWithValue("@Mod", module); cmd.Parameters.AddWithValue("@Type", type);
                cmd.Parameters.AddWithValue("@User", userId); cmd.Parameters.AddWithValue("@Comp", companyId);
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        private void ShowMsg(string msg, bool ok)
        {
            PanelMsg.Visible = !string.IsNullOrEmpty(msg);
            lblMsg.Text = msg;
            lblMsg.ForeColor = ok ? System.Drawing.Color.Green : System.Drawing.Color.Red;

            if (!ok && !string.IsNullOrEmpty(msg))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "ScrollToMsg", "window.scrollTo({top: 0, behavior: 'smooth'});", true);
            }
        }
        #endregion
    }
}