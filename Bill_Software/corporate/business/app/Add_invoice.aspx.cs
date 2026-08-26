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
                object cid = cmd.ExecuteScalar();
                if (cid != null)
                {
                    lblclientId.Text = cid.ToString();
                    LoadAddresses(cid.ToString(), conn);
                }
            }
        }

        private void LoadAddresses(string cid, SqlConnection conn)
        {
            List_BillingAddress.Items.Clear();
            string addrQuery = "SELECT Billing_Address FROM tbl_Client_BillingAddress WHERE Client_Id=@CID AND CompanyID=@CompanyID";
            using (SqlCommand cmd = new SqlCommand(addrQuery, conn))
            {
                cmd.Parameters.AddWithValue("@CID", cid);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        List_BillingAddress.Items.Add(dr["Billing_Address"].ToString());
                }
            }
            if (List_BillingAddress.Items.Count > 0)
                pnlAddress.Visible = true;
        }
        #endregion

        //protected void btnSertch_Click(object sender, EventArgs e)
        //{
        //    string cmdstring = "";
        //    if (RadioButtonList1.SelectedIndex == 0)
        //    {
        //        BuindCompanyId();
        //        cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.Client_Id='" + lblclientId.Text + "' order by tbl_Quotation.ID desc";

        //        //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Client_Id='" + lblclientId.Text + "' and tbl_Quotation.Status2='No' order by tbl_Quotation.ID desc";
        //        Buinddatagrid(cmdstring);
        //    }
        //    else if (RadioButtonList1.SelectedIndex == 1)
        //    {
        //        //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Status2='No' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by tbl_Quotation.ID desc";
        //        cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by tbl_Quotation.ID desc";

        //        Buinddatagrid(cmdstring);
        //    }
        //    else
        //    {
        //        BuindCompanyId();
        //        cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.Client_Id='" + lblclientId.Text + "' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by tbl_Quotation.ID desc";

        //        //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Status2='No' and tbl_Quotation.Client_Id='" + lblclientId.Text + "' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by tbl_Quotation.ID desc";
        //        Buinddatagrid(cmdstring);
        //    }
        //    btnSertch.Visible = false;

        //}
        //private void Buinddatagrid(string cmdstring)
        //{
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
        //    SqlDataReader re = cmd.ExecuteReader();
        //    if (re.Read())
        //    {
        //        Buinddatagrid1(cmdstring);
        //    }
        //    else
        //    {
        //        PanelError.Visible = true;
        //        lblErrorMsg.Text = "No Data Found...";
        //    }
        //    DbCL.Conn.Close();
        //}

        //private void Buinddatagrid1(string cmdstring)
        //{
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    SqlCommand cmd1 = new SqlCommand(cmdstring, DbCL.Conn);
        //    DataList1.DataSource = cmd1.ExecuteReader();
        //    DataList1.DataBind();
        //    DbCL.Conn.Close();
        //}

        //private void BuindCompanyId()
        //{
        //    string sql = "select CompanyID from tbl_login where User_Id='" + Session["USERID"].ToString() + "'";
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    SqlCommand cmd = new SqlCommand(sql, DbCL.Conn);
        //    lblclientId.Text = cmd.ExecuteScalar().ToString();
        //    DbCL.Conn.Close();
        //}

        #region DOCUMENT SEARCH
        protected void btnSertch_Click(object sender, EventArgs e)
        {
            pnlSearchResults.Visible = false;
            lblSearchMsg.Text = "";

            string docNo = txtSearchDocNo.Text.Trim();
            string vendorName = cmbvendor.SelectedValue;
            DateTime fromDate, toDate;

            if (!DateTime.TryParse(txtfromDate.Text, out fromDate)) fromDate = DateTime.Now.AddMonths(-6);
            if (!DateTime.TryParse(txttodate.Text, out toDate)) toDate = DateTime.Now;

            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                conn.Open();
                int companyId = CompanyContext.CurrentCompanyID;

                StringBuilder sb = new StringBuilder();
                sb.Append(@"
                    SELECT 
                        d.DocumentNo, d.DocumentType, d.DocumentDate, d.ClientName,
                        d.TotalAmount, d.Status, d.AddedByName,
                        ISNULL((SELECT COUNT(*) FROM vw_InvDocLineItems i WHERE i.RefDocumentNo = d.DocumentNo AND i.CompanyID = @CID AND i.IsInvoiced = 1), 0) AS InvoicedCount,
                        ISNULL((SELECT COUNT(*) FROM vw_InvDocLineItems i WHERE i.RefDocumentNo = d.DocumentNo AND i.CompanyID = @CID AND i.IsInvoiced = 0), 0) AS PendingCount
                    FROM vw_InvDocumentSearch d
                    WHERE d.CompanyID = @CID AND d.Status = 'Active'");

                List<SqlParameter> parms = new List<SqlParameter>();
                parms.Add(new SqlParameter("@CID", companyId));

                if (!string.IsNullOrEmpty(docNo))
                {
                    sb.Append(" AND d.DocumentNo LIKE @DocNo");
                    parms.Add(new SqlParameter("@DocNo", "%" + docNo + "%"));
                }
                if (!string.IsNullOrEmpty(vendorName) && vendorName != "0")
                {
                    sb.Append(" AND d.ClientName = @Vendor");
                    parms.Add(new SqlParameter("@Vendor", vendorName));
                }
                sb.Append(" AND CAST(d.DocumentDate AS DATE) BETWEEN @FD AND @TD");
                parms.Add(new SqlParameter("@FD", fromDate));
                parms.Add(new SqlParameter("@TD", toDate));

                sb.Append(" ORDER BY d.DocumentDate DESC");

                using (SqlCommand cmd = new SqlCommand(sb.ToString(), conn))
                {
                    cmd.Parameters.AddRange(parms.ToArray());
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            gvSearchDocs.DataSource = dt;
                            gvSearchDocs.DataBind();
                            pnlSearchResults.Visible = true;
                        }
                        else
                        {
                            lblSearchMsg.Text = "No documents found matching your criteria.";
                            lblSearchMsg.ForeColor = System.Drawing.Color.OrangeRed;
                            pnlSearchMsg.Visible = true;
                        }
                    }
                }
            }
        }

        protected void btnResetSearch_Click(object sender, EventArgs e)
        {
            txtSearchDocNo.Text = "";
            txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            cmbvendor.SelectedIndex = 0;
            pnlSearchResults.Visible = false;
            pnlSearchMsg.Visible = false;
        }

        protected void gvSearchDocs_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Select") return;
            int idx = Convert.ToInt32(e.CommandArgument);
            GridViewRow row = gvSearchDocs.Rows[idx];
            string docNo = row.Cells[0].Text.Trim();
            string docType = row.Cells[1].Text.Trim();
            string clientName = row.Cells[3].Text.Trim();
            hdnSelectedDocNo.Value = docNo;
            hdnSelectedDocType.Value = docType;

            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                conn.Open();
                int companyId = CompanyContext.CurrentCompanyID;

                // 1. Fetch Document Metadata (Invoice History)
                FetchDocumentMetadata(conn, docNo, docType, companyId);

                // 2. Fetch Line Items (items to invoice)
                string itemQuery = GetItemQueryByDocType(docType);
                using (SqlCommand cmd = new SqlCommand(itemQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Ref", docNo);
                    cmd.Parameters.AddWithValue("@CompanyID", companyId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        dtItems = new DataTable();
                        sda.Fill(dtItems);
                    }
                }

                // 3. Refresh invoice history from DB (overrides any stale memory state)
                BindInvoiceHistory(docNo);

                // 4. Bind the Products Grid
                BindProductsGrid();

                // 5. Update UI State
                lblClientName.Text = clientName;
                lblSelectedDoc.Text = docNo + " (" + docType + ")";
                btnProceedToInvoice.Visible = true;
                btnBackSetup.Visible = true;
                mvInvoice.ActiveViewIndex = 1;
            }
        }

        private void FetchDocumentMetadata(SqlConnection conn, string docNo, string docType, int companyId)
        {
            string metaQuery = "";
            if (docType == "Quotation" || docType == "Purchase Order")
                metaQuery = "SELECT TOP 1 Gross, discount, sub_total, Net_Amount, Quotation_Date AS DocDate FROM tbl_Quotation WHERE Quotation_no = @DocNo AND CompanyID = @CID";
            else if (docType == "Proforma")
                metaQuery = "SELECT TOP 1 Gross, discount, sub_total, Net_Amount, Invoice_Date AS DocDate FROM tbl_Proforma WHERE Invoice_No = @DocNo AND CompanyID = @CID";
            else
                metaQuery = "SELECT TOP 1 Gross, discount, sub_total, Net_Amount, Challan_Date AS DocDate FROM tbl_Chalan WHERE Chalan_No = @DocNo AND CompanyID = @CID";

            using (SqlCommand cmd = new SqlCommand(metaQuery, conn))
            {
                cmd.Parameters.AddWithValue("@DocNo", docNo);
                cmd.Parameters.AddWithValue("@CID", companyId);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        decimal gross = dr["Gross"] != DBNull.Value ? Convert.ToDecimal(dr["Gross"]) : 0;
                        decimal discount = dr["discount"] != DBNull.Value ? Convert.ToDecimal(dr["discount"]) : 0;
                        decimal netAmt = dr["Net_Amount"] != DBNull.Value ? Convert.ToDecimal(dr["Net_Amount"]) : 0;
                        DateTime docDate = dr["DocDate"] != DBNull.Value ? Convert.ToDateTime(dr["DocDate"]) : DateTime.Now;

                        // Update Memory Row 0
                        if (dtItems.Rows.Count > 0)
                        {
                            dtItems.Rows[0]["Gross"] = gross;
                            dtItems.Rows[0]["discount"] = discount;
                            dtItems.Rows[0]["Net_Amount"] = netAmt;
                        }

                        txtinvoiceDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                        txtInDocDate.Text = docDate.ToString("dd-MMM-yyyy");
                    }
                }
            }
        }
        #endregion

        #region PRODUCT FETCHING (GETITEMQUERYBYDOCTYPE) — 3-WAY BRANCHING SQL
        private string GetItemQueryByDocType(string docType)
        {
            if (docType == "Quotation" || docType == "Purchase Order")
            {
                return @"
                    SELECT 
                        qd.Product_Code AS TrueID, qd.Product_id AS TrueHSN, qd.Product_name, 
                        CAST(qd.Quantity AS DECIMAL(18,2)) AS QuotedQty,
                        ISNULL((SELECT SUM(CAST(id.Quantity AS DECIMAL(18,2))) FROM tbl_Invoice_details id INNER JOIN tbl_Invoice iv ON id.Invoice_No = iv.Invoice_No WHERE id.Quotation_no = qd.Quotation_no AND id.Product_id = qd.Product_Code AND ISNULL(id.ItemNo, '') = ISNULL(qd.ItemNo, '') AND id.CompanyID = @CompanyID AND iv.status2 = 'Active'), 0) AS InvoicedQty,
                        CAST(qd.Quantity AS DECIMAL(18,2)) - ISNULL((SELECT SUM(CAST(id.Quantity AS DECIMAL(18,2))) FROM tbl_Invoice_details id INNER JOIN tbl_Invoice iv ON id.Invoice_No = iv.Invoice_No WHERE id.Quotation_no = qd.Quotation_no AND id.Product_id = qd.Product_Code AND ISNULL(id.ItemNo, '') = ISNULL(qd.ItemNo, '') AND id.CompanyID = @CompanyID AND iv.status2 = 'Active'), 0) AS PendingQty,
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
                    ISNULL((SELECT SUM(CAST(id.Quantity AS DECIMAL(18,2))) FROM tbl_Invoice_details id INNER JOIN tbl_Invoice iv ON id.Invoice_No = iv.Invoice_No WHERE id.Quotation_no = pd.Proforma_No AND id.Product_id = pd.Product_id AND ISNULL(id.ItemNo, '') = ISNULL(pd.ItemNo, '') AND id.CompanyID = @CompanyID AND iv.status2 = 'Active'), 0) AS InvoicedQty,
                    CAST(pd.Quantity AS DECIMAL(18,2)) - ISNULL((SELECT SUM(CAST(id.Quantity AS DECIMAL(18,2))) FROM tbl_Invoice_details id INNER JOIN tbl_Invoice iv ON id.Invoice_No = iv.Invoice_No WHERE id.Quotation_no = pd.Proforma_No AND id.Product_id = pd.Product_id AND ISNULL(id.ItemNo, '') = ISNULL(pd.ItemNo, '') AND id.CompanyID = @CompanyID AND iv.status2 = 'Active'), 0) AS PendingQty,
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
                    ISNULL((SELECT SUM(CAST(id.Quantity AS DECIMAL(18,2))) FROM tbl_Invoice_details id INNER JOIN tbl_Invoice iv ON id.Invoice_No = iv.Invoice_No WHERE id.Quotation_no = cd.Challan_No AND id.Product_id = cd.Product_id AND ISNULL(id.ItemNo, '') = ISNULL(cd.ItemNo, '') AND id.CompanyID = @CompanyID AND iv.status2 = 'Active'), 0) AS InvoicedQty,
                    CAST(cd.Quantity AS DECIMAL(18,2)) - ISNULL((SELECT SUM(CAST(id.Quantity AS DECIMAL(18,2))) FROM tbl_Invoice_details id INNER JOIN tbl_Invoice iv ON id.Invoice_No = iv.Invoice_No WHERE id.Quotation_no = cd.Challan_No AND id.Product_id = cd.Product_id AND ISNULL(id.ItemNo, '') = ISNULL(cd.ItemNo, '') AND id.CompanyID = @CompanyID AND iv.status2 = 'Active'), 0) AS PendingQty,
                    ISNULL(qd.sail_rate, 0) AS sail_rate, ISNULL(qd.discount_rate, 0) AS discountRate, ISNULL(qd.Service_tax_rate, 0) AS Service_tax_rate, ISNULL(qd.specification, '') AS specification, ISNULL(np.Quantity, '0') AS AvailableStock,
                    '' AS ItemNo, '' AS MaterialNo, '' AS PackSize, '' AS Unit, '' AS DeliveryDate, '' AS Department, '' AS ItemRemarks
                FROM tbl_Challan_details cd
                LEFT JOIN tbl_Chalan ch ON cd.Challan_no = ch.Chalan_No AND ch.CompanyID = @CompanyID
                LEFT JOIN tbl_Quotaion_details qd ON ch.Quotation_No = qd.Quotation_no AND qd.Product_Code = cd.Product_id AND qd.CompanyID = @CompanyID
                LEFT JOIN tbl_NewProduct np ON np.ProductID = cd.Product_id AND np.CompanyID = @CompanyID
                WHERE cd.Challan_no = @Ref AND cd.CompanyID = @CompanyID";
        }
        #endregion

        #region PRODUCTS GRID BINDING & EDITING (MEMORY TABLE)
        private void BindProductsGrid()
        {
            GridView1.DataSource = dtItems;
            GridView1.DataBind();
        }

        protected void gvGrid1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // Row editing handled by standard controls
        }

        protected void btnRemoveBulk_Click(object sender, EventArgs e)
        {
            List<int> removeIndices = new List<int>();
            foreach (GridViewRow row in GridView1.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkSelect");
                if (chk != null && chk.Checked)
                    removeIndices.Add(row.RowIndex);
            }

            foreach (int idx in removeIndices.OrderByDescending(x => x))
            {
                if (idx < dtItems.Rows.Count)
                    dtItems.Rows[idx].Delete();
            }

            SyncGridToTable(dtItems);
            BindProductsGrid();
            ShowMsg("Selected items removed.", true);
        }

        protected void btnRemoveZeroQty_Click(object sender, EventArgs e)
        {
            // Sync GridView TextBoxes back to Memory Table before filtering
            SyncGridToTable(dtItems);

            List<DataRow> toRemove = new List<DataRow>();
            foreach (DataRow row in dtItems.Rows)
            {
                decimal qty = 0;
                decimal.TryParse(row["QuotedQty"].ToString(), out qty);
                if (qty <= 0)
                    toRemove.Add(row);
            }

            foreach (DataRow row in toRemove)
                dtItems.Rows.Remove(row);

            SyncGridToTable(dtItems);
            BindProductsGrid();
            ShowMsg("Zero-quantity items removed.", true);
        }

        protected void btnRestore_Click(object sender, EventArgs e)
        {
            // Re-fetch from DB using the saved document reference
            string docNo = hdnSelectedDocNo.Value;
            string docType = hdnSelectedDocType.Value;
            if (string.IsNullOrEmpty(docNo)) return;

            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                conn.Open();
                int companyId = CompanyContext.CurrentCompanyID;
                string itemQuery = GetItemQueryByDocType(docType);
                using (SqlCommand cmd = new SqlCommand(itemQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Ref", docNo);
                    cmd.Parameters.AddWithValue("@CompanyID", companyId);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        dtItems = new DataTable();
                        sda.Fill(dtItems);
                    }
                }
            }
            BindProductsGrid();
            ShowMsg("Items restored from source document.", true);
        }

        private void SyncGridToTable(DataTable dt)
        {
            foreach (GridViewRow row in GridView1.Rows)
            {
                if (row.RowIndex >= dt.Rows.Count) break;
                DataRow dr = dt.Rows[row.RowIndex];

                decimal q = 0, r = 0, dPer = 0, tPer = 0, totalRowDisc = 0;
                decimal.TryParse(((TextBox)row.FindControl("txtqnty")).Text, out q);
                decimal.TryParse(((TextBox)row.FindControl("txtsailrate")).Text, out r);
                decimal.TryParse(((TextBox)row.FindControl("txtDiscPer")).Text, out dPer);
                decimal.TryParse(((TextBox)row.FindControl("txtDiscAmt")).Text, out totalRowDisc);
                decimal.TryParse(((Label)row.FindControl("lblGstRate")).Text, out tPer);

                dr["QuotedQty"] = q;
                dr["sail_rate"] = r;
                dr["discountRate"] = dPer;
                dr["Service_tax_rate"] = tPer;

                // Recalculate dependent columns
                decimal gross = Math.Round(q * r, 2);
                decimal taxable = Math.Round(gross - totalRowDisc, 2);
                dr["Gross"] = gross;
                dr["discount"] = totalRowDisc;
                dr["Net_Amount"] = Math.Round(taxable + (taxable * tPer / 100), 2);
                dr["Total_Rate1"] = taxable;
                dr["Total_Rate2"] = gross;
            }
        }

        protected void btnBackSetup_Click(object sender, EventArgs e) { mvInvoice.ActiveViewIndex = 0; }
        #endregion

        #region SAVE INVOICE (Button1_Click — REAL PERSISTENCE)
        protected void Button1_Click(object sender, EventArgs e)
        {
            // 0. Null Guard — Prevents "Thread was being aborted" crash
            if (dtItems == null || dtItems.Rows.Count == 0)
            {
                ShowMsg("No items to save. Please add items first.", false);
                return;
            }

            // Sync the GridView TextBoxes to the Memory Table (so the user's edits are saved)
            SyncGridToTable(dtItems);

            // 1. SECURITY: Read values from server-side session — NOT from Label.Text
            string invNo = txtInInvoiceNo.Text.Trim();
            string clientName = lblClientName.Text.Trim();
            string uid = Session["USERID"].ToString();
            int companyId = CompanyContext.CurrentCompanyID;
            DateTime invDate;
            if (!DateTime.TryParse(txtinvoiceDate.Text, out invDate)) invDate = DateTime.Now;
            DateTime extDate;
            if (!DateTime.TryParse(txtInExtDate.Text, out extDate)) extDate = DateTime.MinValue;
            string extNo = txtInExtNo.Text.Trim();
            string billingAddress = List_BillingAddress.SelectedValue;
            string salesPersonCode = cmbSalesPerson.SelectedValue;

            // Server-side Validate: Check if Invoice Number already exists
            using (SqlConnection chkConn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                chkConn.Open();
                SqlCommand chkCmd = new SqlCommand("SELECT COUNT(*) FROM tbl_Invoice WHERE Invoice_No = @Inv AND CompanyID = @CID", chkConn);
                chkCmd.Parameters.AddWithValue("@Inv", invNo);
                chkCmd.Parameters.AddWithValue("@CID", companyId);
                int existingCount = (int)chkCmd.ExecuteScalar();
                if (existingCount > 0)
                {
                    ShowMsg("Invoice Number '" + invNo + "' already exists! Please use a unique number.", false);
                    return;
                }
            }

            string refNo = hdnSelectedDocNo.Value;

            // 2. Start SQL Transaction (Atomic Operation)
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    // Calculate Header Totals from the Memory Table
                    decimal headerGross = 0, headerDiscount = 0, headerSubTotal = 0, headerNet = 0;
                    foreach (DataRow rowG in dtItems.Rows)
                    {
                        headerGross += rowG["Gross"] != DBNull.Value ? Convert.ToDecimal(rowG["Gross"]) : 0;
                        headerDiscount += rowG["discount"] != DBNull.Value ? Convert.ToDecimal(rowG["discount"]) : 0;
                        headerNet += rowG["Net_Amount"] != DBNull.Value ? Convert.ToDecimal(rowG["Net_Amount"]) : 0;
                    }
                    headerSubTotal = Math.Round(headerNet, 2);

                    decimal cgstOrsgst = 0, igst = 0;
                    string stateCodeClient = "", stateCodeCompany = "";
                    using (SqlCommand cmdState = new SqlCommand("SELECT TOP 1 StateCode FROM tbl_Client WHERE Client_Name = @Name AND CompanyID = @CID", conn, tran))
                    {
                        cmdState.Parameters.AddWithValue("@Name", clientName);
                        cmdState.Parameters.AddWithValue("@CID", companyId);
                        object sc = cmdState.ExecuteScalar();
                        if (sc != null) stateCodeClient = sc.ToString();
                    }
                    using (SqlCommand cmdStateC = new SqlCommand("SELECT TOP 1 StateCode FROM tbl_CompanyDetails WHERE CompanyID = @CID", conn, tran))
                    {
                        cmdStateC.Parameters.AddWithValue("@CID", companyId);
                        object sc = cmdStateC.ExecuteScalar();
                        if (sc != null) stateCodeCompany = sc.ToString();
                    }

                    if (stateCodeClient == stateCodeCompany)
                    {
                        decimal halfGst = Math.Round(headerSubTotal / 2, 2);
                        cgstOrsgst = halfGst;
                        igst = 0;
                    }
                    else
                    {
                        cgstOrsgst = 0;
                        igst = headerSubTotal;
                    }

                    // INSERT into tbl_Invoice (Header)
                    string sqlH = "INSERT INTO tbl_Invoice (Invoice_No, Invoice_Date, Quotation_No, Client_ID, Gross, discount, sub_total, Service_Tax1, Net_Amount, Sl_no, Delivery_Amount, otherAmount1_name, otherAmount1, status1, status2, cgstOrsgst, igst, AddedById, CompanyID, SalesPersonCode, ExtInvoiceNo, ExtInvoiceDate, BillingAddress) VALUES (@Inv, @Date, @PO, @CID, @Gr, @Di, @Sub, @Tax, @Net, @Sl, @Frt, @OthName, @Oth, 'No', 'Active', @Intra, @Inter, @User, @CompanyID, @SalesPerson, @ExtNo, @ExtDate, @BillingAddress)";

                    using (SqlCommand cmdH = new SqlCommand(sqlH, conn, tran))
                    {
                        cmdH.Parameters.AddWithValue("@Inv", invNo);
                        cmdH.Parameters.AddWithValue("@Date", invDate);
                        cmdH.Parameters.AddWithValue("@PO", refNo);
                        cmdH.Parameters.AddWithValue("@CID", clientName);
                        cmdH.Parameters.AddWithValue("@Gr", headerGross);
                        cmdH.Parameters.AddWithValue("@Di", headerDiscount);
                        cmdH.Parameters.AddWithValue("@Sub", headerSubTotal);
                        cmdH.Parameters.AddWithValue("@Tax", 0);
                        cmdH.Parameters.AddWithValue("@Net", headerNet);
                        cmdH.Parameters.AddWithValue("@Sl", 0);
                        cmdH.Parameters.AddWithValue("@Frt", 0);
                        cmdH.Parameters.AddWithValue("@OthName", DBNull.Value);
                        cmdH.Parameters.AddWithValue("@Oth", 0);
                        cmdH.Parameters.AddWithValue("@Intra", cgstOrsgst);
                        cmdH.Parameters.AddWithValue("@Inter", igst);
                        cmdH.Parameters.AddWithValue("@User", uid);
                        cmdH.Parameters.AddWithValue("@CompanyID", companyId);
                        cmdH.Parameters.AddWithValue("@SalesPerson", salesPersonCode);
                        cmdH.Parameters.AddWithValue("@ExtNo", extNo);
                        cmdH.Parameters.AddWithValue("@ExtDate", extDate == DateTime.MinValue ? (object)DBNull.Value : extDate);
                        cmdH.Parameters.AddWithValue("@BillingAddress", billingAddress);
                        cmdH.ExecuteNonQuery();
                    }

                    // 3. Loop through GridView Rows to INSERT into tbl_Invoice_details (Line Items)
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
                        string itemNo = memRow["ItemNo"] != DBNull.Value ? memRow["ItemNo"].ToString() : "";

                        string sqlD = "INSERT INTO tbl_Invoice_details (Invoice_No, Quotation_no, Product_id, Product_Code, Product_name, Quantity, sail_rate, discountRate, Service_tax_rate, Total_sail_rate1, Total_sail_rate2, specification, ItemNo, AddedById, CompanyID) VALUES (@Inv, @RefNo, @PID, @HSN, @Name, @Qty, @Rate, @DPer, @TPer, @Net, @Base, @Brand, @ItemNo, @User, @CompanyID)";

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
                        cmdD.Parameters.AddWithValue("@ItemNo", itemNo);
                        cmdD.Parameters.AddWithValue("@User", uid);
                        cmdD.Parameters.AddWithValue("@CompanyID", companyId);
                        cmdD.ExecuteNonQuery();
                    }

                    // 4. INSERT into tbl_InvSiteAddress
                    if (!string.IsNullOrEmpty(billingAddress))
                    {
                        string[] addresses = billingAddress.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        int addrSeq = 0;
                        foreach (string addr in addresses)
                        {
                            string trimmedAddr = addr.Trim();
                            if (!string.IsNullOrEmpty(trimmedAddr))
                            {
                                SqlCommand cmdA = new SqlCommand("INSERT INTO tbl_InvSiteAddress (invoice_no, SiteAddress, CompanyID) VALUES (@Inv, @Addr, @CompanyID)", conn, tran);
                                cmdA.Parameters.AddWithValue("@Inv", invNo);
                                cmdA.Parameters.AddWithValue("@Addr", trimmedAddr);
                                cmdA.Parameters.AddWithValue("@CompanyID", companyId);
                                cmdA.ExecuteNonQuery();
                                addrSeq++;
                            }
                        }
                    }

                    // 5. Write to InvoiceLogFile (File-based Audit Log)
                    string logDirectory = Server.MapPath("~/Uploads/InvoiceLogs");
                    if (!System.IO.Directory.Exists(logDirectory))
                        System.IO.Directory.CreateDirectory(logDirectory);

                    string logFilePath = System.IO.Path.Combine(logDirectory, "Log.txt");
                    StringBuilder logEntry = new StringBuilder();
                    logEntry.AppendLine("==================================================");
                    logEntry.AppendLine("Timestamp      : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    logEntry.AppendLine("Invoice No     : " + invNo);
                    logEntry.AppendLine("Client         : " + clientName);
                    logEntry.AppendLine("Ref Doc No     : " + refNo);
                    logEntry.AppendLine("Ref Doc Type   : " + hdnSelectedDocType.Value);
                    logEntry.AppendLine("User           : " + uid);
                    logEntry.AppendLine("Gross          : " + headerGross.ToString("F2"));
                    logEntry.AppendLine("Discount       : " + headerDiscount.ToString("F2"));
                    logEntry.AppendLine("Sub Total      : " + headerSubTotal.ToString("F2"));
                    logEntry.AppendLine("Net Amount     : " + headerNet.ToString("F2"));
                    logEntry.AppendLine("CGST/SGST      : " + cgstOrsgst.ToString("F2"));
                    logEntry.AppendLine("IGST           : " + igst.ToString("F2"));
                    logEntry.AppendLine("Intra/Inter    : " + (stateCodeClient == stateCodeCompany ? "Intra-State" : "Inter-State"));

                    logEntry.AppendLine("--- LINE ITEMS ---");
                    foreach (DataRow rowL in dtItems.Rows)
                    {
                        decimal lineQty = rowL["QuotedQty"] != DBNull.Value ? Convert.ToDecimal(rowL["QuotedQty"]) : 0;
                        if (lineQty <= 0) continue;
                        logEntry.AppendLine("Product: " + rowL["Product_name"] + " | Qty: " + lineQty + " | Rate: " + (rowL["sail_rate"] != DBNull.Value ? rowL["sail_rate"].ToString() : "0") + " | Net: " + (rowL["Net_Amount"] != DBNull.Value ? rowL["Net_Amount"].ToString() : "0"));
                    }

                    logEntry.AppendLine("==================================================");
                    logEntry.AppendLine("");
                    System.IO.File.AppendAllText(logFilePath, logEntry.ToString());

                    // 6. CRM Pipeline Integration — Mark Sales Visit as Productive (Quotation/PO only)
                    bool isCrmLogged = false;
                    string docType = hdnSelectedDocType.Value;
                    if (docType == "Quotation" || docType == "Purchase Order")
                    {
                        string sqlVisitUpdate = @"
                            UPDATE v
                            SET v.IsProductive = 1, 
                                v.RevenueRealized = ISNULL(v.RevenueRealized, 0) + @NetAmt
                            FROM tbl_SalesVisitReport v
                            INNER JOIN tbl_Quotation q ON v.Id = q.VisitId AND q.CompanyID = @CompanyID
                            WHERE q.Quotation_no = @RefNo 
                              AND v.CompanyID = @CompanyID";

                        using (SqlCommand cmdVisit = new SqlCommand(sqlVisitUpdate, conn, tran))
                        {
                            cmdVisit.Parameters.Add("@NetAmt", SqlDbType.Decimal).Value = headerNet;
                            cmdVisit.Parameters.AddWithValue("@RefNo", refNo);
                            cmdVisit.Parameters.AddWithValue("@CompanyID", companyId);
                            int rowsAffected = cmdVisit.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                InsertSystemNotification("CRM", "Success", "Sales Target Achieved!", $"Invoice generated for {headerNet}. Revenue successfully tied to the original Sales Visit.", uid, companyId, conn, tran);
                                isCrmLogged = true;
                            }
                        }
                    }

                    // 7. Commit Transaction
                    tran.Commit();

                    // === PROACTIVE NOTIFICATION LOGGING (Step 7) ===
                    try
                    {
                        if (!isCrmLogged)
                        {
                            InsertSystemNotification(
                                "New Invoice Created",
                                "Invoice " + invNo + " created for " + clientName + " (Ref: " + refNo + ") Amount: " + headerNet.ToString("N2"),
                                "Invoice",
                                "Info",
                                uid,
                                companyId,
                                conn,
                                null
                            );
                        }
                    }
                    catch { /* Notification failure should not crash invoice save */ }

                    // 7. Show success message and reset form
                    ShowMsg("Invoice " + invNo + " saved successfully! Net Amount: " + headerNet.ToString("F2"), true);
                    txtInInvoiceNo.Text = "";
                    hdnSelectedDocNo.Value = "";
                    hdnSelectedDocType.Value = "";
                    mvInvoice.ActiveViewIndex = 0;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ShowMsg("Error saving invoice: " + ex.Message, false);
                }
            }
        }

        private void InsertSystemNotification(string title, string message, string module, string type, string userId, int companyId, SqlConnection conn, SqlTransaction tran)
        {
            string query = @"INSERT INTO tbl_SystemNotifications (Title, Message, Module, Type, UserId, CompanyID, IsRead, CreatedAt) 
                           VALUES (@Title, @Message, @Module, @Type, @UserId, @CompanyID, 0, GETDATE())";

            bool needsClose = false;
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
                needsClose = true;
            }

            using (SqlCommand cmd = new SqlCommand(query, conn, tran))
            {
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Message", message);
                cmd.Parameters.AddWithValue("@Module", module);
                cmd.Parameters.AddWithValue("@Type", type);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@CompanyID", companyId);
                cmd.ExecuteNonQuery();
            }

            if (needsClose) conn.Close();
        }

        private void ShowMsg(string msg, bool isSuccess)
        {
            pnlMsg.Visible = true;
            lblMsg.Text = msg;
            lblMsg.CssClass = isSuccess ? "alert alert-success" : "alert alert-danger";
        }

        public static string GetReconciliation(string refNo, string productId)
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                conn.Open();
                string query = @"SELECT 
                    ISNULL(SUM(CASE WHEN inv.status2 = 'Active' THEN CAST(id.Quantity AS DECIMAL(18,2)) ELSE 0 END), 0) AS InvoicedQty,
                    ISNULL((SELECT CAST(qd.Quantity AS DECIMAL(18,2)) FROM tbl_Quotaion_details qd WHERE qd.Quotation_no = @RefNo AND qd.Product_id = @ProductID AND qd.CompanyID = (SELECT TOP 1 CompanyID FROM tbl_Invoice_details WHERE Quotation_no = @RefNo)), 0) AS QuotedQty
                FROM tbl_Invoice_details id
                INNER JOIN tbl_Invoice inv ON id.Invoice_No = inv.Invoice_No AND id.CompanyID = inv.CompanyID
                WHERE id.Quotation_no = @RefNo AND id.Product_id = @ProductID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@RefNo", refNo);
                    cmd.Parameters.AddWithValue("@ProductID", productId);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            decimal invoiced = Convert.ToDecimal(dr["InvoicedQty"]);
                            decimal quoted = Convert.ToDecimal(dr["QuotedQty"]);
                            decimal pending = quoted - invoiced;
                            return "Quoted: " + quoted.ToString("F2") + " | Invoiced: " + invoiced.ToString("F2") + " | Pending: " + pending.ToString("F2");
                        }
                    }
                }
            }
            return "N/A";
        }
    }
}
