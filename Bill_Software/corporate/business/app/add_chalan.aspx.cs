using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm38 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        private int totalQuoted = 0;
        private int totalDelivered = 0;
        private int totalDue = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                //DbCL.FillCombo(cmbvendor, "select Client_Name from tbl_Client order by Client_Name");

                // Keep the default dates empty so users can search strictly by PO/RecordType if they want.
                // Or you can retain the original logic: DateTime.Now.ToString("dd-MMM-yyyy")
                txtfromDate.Text = "";
                txttodate.Text = "";
                txtinvoiceDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            }
        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            // 1. SAFETY FIRST: Clear old ViewState and hide the details panel
            ViewState["ViewQProductData"] = null;
            Panel1.Visible = false;

            // 1. Build Smart Search Query using parameters
            string baseQuery = @"
                SELECT tbl_QuoPriSerTogather.PServiceName, tbl_Quotation.ID, tbl_Quotation.service_tax1, 
                       tbl_Quotation.sub_total, tbl_Quotation.DO_Number, tbl_Quotation.PO_Number, 
                       tbl_Quotation.Quotation_no, tbl_Quotation.Quotation_date, tbl_Quotation.Gross, 
                       tbl_Quotation.Service_tax, tbl_Quotation.Net_amount, tbl_Quotation.mailStatusDate, 
                       tbl_Client.Client_Name 
                FROM tbl_Quotation 
                LEFT OUTER JOIN tbl_Client ON tbl_Quotation.Client_Id = tbl_Client.Client_Id 
                LEFT OUTER JOIN tbl_QuoPriSerTogather ON tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no 
                     AND tbl_QuoPriSerTogather.TimeStamp = tbl_Quotation.TimsStamp 
                WHERE 1=1 ";

            List<SqlParameter> parameters = new List<SqlParameter>();

            // Client Filter
            //if (cmbvendor.SelectedIndex > 0 && !string.IsNullOrEmpty(cmbvendor.SelectedValue))
            //{
            //    BuindCompanyId(); // Translates name to ID
            //    baseQuery += " AND tbl_Quotation.Client_Id = @ClientId ";
            //    parameters.Add(new SqlParameter("@ClientId", lblclientId.Text));
            //}

            // REPLACE your Client Filter block with this:
            if (!string.IsNullOrEmpty(txtClientName.Text.Trim()))
            {
                BuindCompanyId(); // Translates name to ID
                baseQuery += " AND tbl_Quotation.Client_Id = @ClientId ";
                parameters.Add(new SqlParameter("@ClientId", lblclientId.Text));
            }

            // Record Type Filter (Quotation vs Purchase Order)
            if (cmbRecordType.SelectedIndex > 0 && !string.IsNullOrEmpty(cmbRecordType.SelectedValue))
            {
                baseQuery += " AND tbl_Quotation.RecordType = @RecordType ";
                parameters.Add(new SqlParameter("@RecordType", cmbRecordType.SelectedValue));
            }

            // Date Filter
            if (!string.IsNullOrEmpty(txtfromDate.Text) && !string.IsNullOrEmpty(txttodate.Text))
            {
                baseQuery += " AND CAST(tbl_Quotation.Quotation_date AS datetime) BETWEEN @FromDate AND @ToDate ";
                parameters.Add(new SqlParameter("@FromDate", txtfromDate.Text));
                parameters.Add(new SqlParameter("@ToDate", txttodate.Text));
            }

            // Smart Document Filter (PO / DO / QTN No)
            string docSearch = txtDocNumber.Text.Trim();
            if (!string.IsNullOrEmpty(docSearch))
            {
                baseQuery += " AND (tbl_Quotation.Quotation_no LIKE @DocNo OR tbl_Quotation.PO_Number LIKE @DocNo OR tbl_Quotation.DO_Number LIKE @DocNo) ";
                parameters.Add(new SqlParameter("@DocNo", "%" + docSearch + "%"));
            }

            baseQuery += " ORDER BY CAST(tbl_Quotation.Quotation_date AS datetime) DESC";

            Buinddatagrid(baseQuery, parameters.ToArray());
        }

        private void Buinddatagrid(string cmdstring, SqlParameter[] parameters)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                using (SqlDataReader re = cmd.ExecuteReader())
                {
                    if (re.HasRows)
                    {
                        DataList1.DataSource = re;
                        DataList1.DataBind();
                        PanelError.Visible = false;
                        lblErrorMsg.Text = String.Empty;
                    }
                    else
                    {
                        DataList1.DataSource = null;
                        DataList1.DataBind();
                        PanelError.Visible = true;
                        lblErrorMsg.Text = "No Data Found based on your search filters...";
                    }
                }
            }
            DbCL.Conn.Close();
        }

        private void BuindCompanyId()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Id from tbl_Client where Client_Name=@ClientName";
            using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@ClientName", txtClientName.Text.Trim());
                using (SqlDataReader re = cmd.ExecuteReader())
                {
                    if (re.Read())
                    {
                        lblclientId.Text = re["Client_Id"].ToString();
                    }
                }
            }
            DbCL.Conn.Close();
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/add_chalan.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            try
            {
                string Quotation_no = Convert.ToString(e.CommandArgument);
                if (e.CommandName == "Select")
                {
                    // 1. SAFETY FIRST: Clear the old ViewState before loading new data
                    ViewState["ViewQProductData"] = null;

                    // 2. Setup the UI Panels
                    Panel1.Visible = true;
                    Panel2.Visible = false; // Hide the search results
                    PanelError.Visible = false;
                    PanelOK.Visible = false;

                    // 3. Load the fresh data
                    Binddetails(Quotation_no);
                    Bindquotationdetails(Quotation_no);
                }
            }
            catch (Exception ex)
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "An error occurred while loading details. Please contact admin.";
            }
        }

        // PERFORMANCE FIX: Pre-fetch all delivered quantities for this document to avoid N+1 query lag
        private Dictionary<string, decimal> GetAllDeliveredQuantities(string quotationNo)
        {
            Dictionary<string, decimal> deliveredDict = new Dictionary<string, decimal>();

            string query = @"
                SELECT cd.ItemNo, cd.Product_id, SUM(CAST(cd.Quantity as decimal(18,2))) as DeliveredQnt 
                FROM tbl_Challan_details cd
                INNER JOIN tbl_Chalan c ON cd.Challan_no = c.Chalan_No
                WHERE c.Quotation_No = @QuotationNo
                GROUP BY cd.ItemNo, cd.Product_id";

            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            using (SqlCommand cmd = new SqlCommand(query, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@QuotationNo", quotationNo);
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        string itemNo = rdr["ItemNo"].ToString();
                        string productId = rdr["Product_id"].ToString();
                        decimal qty = rdr["DeliveredQnt"] != DBNull.Value ? Convert.ToDecimal(rdr["DeliveredQnt"]) : 0;

                        string key = $"{itemNo}_{productId}";
                        deliveredDict[key] = qty;
                    }
                }
            }
            DbCL.Conn.Close();
            return deliveredDict;
        }

        private void Bindquotationdetails(string Quotation_no)
        {
            // 1. Fetch delivered quantities upfront
            Dictionary<string, decimal> deliveredQtys = GetAllDeliveredQuantities(Quotation_no);

            // 2. Setup Memory DataTable
            DataTable dtPCat = new DataTable();
            dtPCat.Columns.Add(new DataColumn("Product_id", typeof(string)));
            dtPCat.Columns.Add(new DataColumn("Product_code", typeof(string)));
            dtPCat.Columns.Add(new DataColumn("ProductName", typeof(string)));
            dtPCat.Columns.Add(new DataColumn("Quantity", typeof(string)));
            dtPCat.Columns.Add(new DataColumn("DeliveredQnt", typeof(string)));
            dtPCat.Columns.Add(new DataColumn("RemainQny", typeof(string)));
            dtPCat.Columns.Add(new DataColumn("ItemNo", typeof(string)));
            dtPCat.Columns.Add(new DataColumn("MaterialNo", typeof(string)));
            dtPCat.Columns.Add(new DataColumn("PackSize", typeof(string)));
            dtPCat.Columns.Add(new DataColumn("Department", typeof(string)));
            dtPCat.Columns.Add(new DataColumn("DeliveryDate", typeof(string)));

            // 3. Process records locally without hitting DB in a loop
            string cmdstring = "select Sl_no,Product_id,Product_Code, (Product_name+' '+specification) as Product_name,Quantity,sail_rate, Service_tax_rate,Total_sail_rate2, ItemNo, MaterialNo, PackSize, Department, DeliveryDate from tbl_Quotaion_details where Quotation_no=@Quotation_no and IsDeleted=0 and IsLatest=1 order by CAST(Sl_no as int)";

            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@Quotation_no", Quotation_no);
                using (SqlDataReader re = cmd.ExecuteReader())
                {
                    while (re.Read())
                    {
                        string Product_id = re["Product_id"].ToString();
                        string Product_Code = re["Product_Code"].ToString();
                        string Product_name = re["Product_name"].ToString();
                        string ItemNo = re["ItemNo"].ToString();

                        decimal qtyInt = 0;
                        decimal.TryParse(re["Quantity"] == DBNull.Value ? "0" : re["Quantity"].ToString().Trim(), out qtyInt);

                        decimal deliveredInt = 0;
                        string dictKey = $"{ItemNo}_{Product_id}";
                        if (deliveredQtys.ContainsKey(dictKey))
                        {
                            deliveredInt = deliveredQtys[dictKey];
                        }

                        decimal RemainQnt = qtyInt - deliveredInt;

                        DataRow dr = dtPCat.NewRow();
                        dr[0] = Product_id;
                        dr[1] = Product_Code;
                        dr[2] = Product_name;
                        dr[3] = qtyInt.ToString();
                        dr[4] = deliveredInt.ToString();
                        dr[5] = RemainQnt.ToString();
                        dr[6] = ItemNo;
                        dr[7] = re["MaterialNo"].ToString();
                        dr[8] = re["PackSize"].ToString();
                        dr[9] = re["Department"].ToString();
                        dr[10] = re["DeliveryDate"].ToString();

                        dtPCat.Rows.Add(dr);
                    }
                }
            }
            DbCL.Conn.Close();

            // 4. Bind and store in ViewState ONCE
            gd_Quotation.DataSource = dtPCat;
            gd_Quotation.DataBind();
            ViewState["ViewQProductData"] = dtPCat;
        }

        private void Binddetails(string Quotation_no)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Quotation where Quotation_no=@Quotation_no";
            using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@Quotation_no", Quotation_no);
                using (SqlDataReader re = cmd.ExecuteReader())
                {
                    if (re.Read())
                    {
                        lblClient_Id.Text = re["Client_Id"].ToString();
                        lblQuotation_no.Text = re["Quotation_no"].ToString();
                        lblQuotation_date.Text = re["Quotation_date"].ToString();
                        lblGross_amount.Text = re["Gross"].ToString();
                        lblservicetax.Text = re["Service_tax"].ToString();
                        lblNet_amount.Text = re["Net_amount"].ToString();
                        lbl_ponumber.Text = re["PO_Number"].ToString();
                        lbl_donumber.Text = re["DO_Number"].ToString();

                        string clientcode = re["Client_Id"].ToString();
                        bindFactoryAddress(clientcode);
                    }
                }
            }
            DbCL.Conn.Close();
            BindclientName();
        }

        private void bindFactoryAddress(string clientcode)
        {
            FactoryAddress.Items.Clear(); // Clear before adding to avoid duplication on re-click
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Address1+', '+City+', '+pin+', '+State from tbl_Client where Client_Id='" + clientcode + "'";
            using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
            {
                using (SqlDataReader DR1 = cmd.ExecuteReader())
                {
                    while (DR1.Read())
                    {
                        FactoryAddress.Items.Add(DR1.GetValue(0).ToString());
                    }
                }
            }
            DbCL.Conn.Close();
            bindRegAddress(clientcode);
            bindAddress(clientcode);
        }

        private void bindAddress(string clientcode)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select [Address1] +', '+ [Address2]+', '+[city]+', '+[State]+', '+[pin] as address from tbl_Factory where Client_id='" + clientcode + "'";
            using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
            {
                using (SqlDataReader DR1 = cmd.ExecuteReader())
                {
                    while (DR1.Read())
                    {
                        FactoryAddress.Items.Add(DR1.GetValue(0).ToString());
                    }
                }
            }
            DbCL.Conn.Close();
        }

        private void bindRegAddress(string clientcode)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Address+', '+State+', '+City+', '+pin as regadd from tbl_ClientRegAddress where Client_Id='" + clientcode + "'";
            using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
            {
                using (SqlDataReader DR1 = cmd.ExecuteReader())
                {
                    while (DR1.Read())
                    {
                        FactoryAddress.Items.Add(DR1.GetValue(0).ToString());
                    }
                }
            }
            DbCL.Conn.Close();
        }

        private void BindclientName()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Name from tbl_Client where Client_Id='" + lblClient_Id.Text + "'";
            using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
            {
                using (SqlDataReader re = cmd.ExecuteReader())
                {
                    if (re.Read())
                    {
                        lblClientName.Text = re["Client_Name"].ToString();
                    }
                }
            }
            DbCL.Conn.Close();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            // 1. Ensure at least one address is selected
            if (FactoryAddress.GetSelectedIndices().Length == 0)
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "Please Select Delivery Address....";
                return;
            }

            // 2. Generate the necessary IDs BEFORE starting the transaction
            string invoice_no = BindInvoiceNo();
            int j = idreturn_OLD() + 1;

            DataTable dt1 = (DataTable)ViewState["ViewQProductData"];
            if (dt1 == null || dt1.Rows.Count == 0)
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "No product data available to create a Challan.";
                return;
            }

            // 3. Set up the SQL Connection and Transaction
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                con.Open();

                // Begin the transaction!
                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        // --- INSERT 1: Main Challan Record ---
                        string queryChalan = @"INSERT INTO tbl_Chalan (Chalan_No, Chalan_Date, Quotation_No, Quotation_Date, Client_ID, Sl_no) 
                                       VALUES (@ChalanNo, @ChalanDate, @QuotationNo, @QuotationDate, @ClientID, @SlNo)";
                        using (SqlCommand cmd = new SqlCommand(queryChalan, con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ChalanNo", invoice_no);
                            cmd.Parameters.AddWithValue("@ChalanDate", txtinvoiceDate.Text);
                            cmd.Parameters.AddWithValue("@QuotationNo", lblQuotation_no.Text);
                            cmd.Parameters.AddWithValue("@QuotationDate", lblQuotation_date.Text);
                            cmd.Parameters.AddWithValue("@ClientID", lblClient_Id.Text);
                            cmd.Parameters.AddWithValue("@SlNo", j);
                            cmd.ExecuteNonQuery();
                        }

                        // --- INSERT 2: Challan Details (Looping through Grid) ---
                        string queryDetails = @"INSERT INTO tbl_Challan_details (Sl_no, Challan_no, Product_id, Product_code, Product_name, Quantity, ItemNo, MaterialNo, PackSize) 
                                        VALUES (@SlNoDetail, @ChallanNo, @ProductId, @ProductCode, @ProductName, @Quantity, @ItemNo, @MaterialNo, @PackSize)";

                        int k = 1;
                        for (int i = 0; i < dt1.Rows.Count; i++)
                        {
                            CheckBox chk = (CheckBox)gd_Quotation.Rows[i].FindControl("chk");

                            if (chk != null && chk.Checked)
                            {
                                string QtyText = ((TextBox)gd_Quotation.Rows[i].FindControl("Qty")).Text?.Trim() ?? "";
                                decimal quantity = 0;
                                decimal.TryParse(QtyText, out quantity);

                                if (quantity > 0)
                                {
                                    using (SqlCommand cmd = new SqlCommand(queryDetails, con, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@SlNoDetail", k);
                                        cmd.Parameters.AddWithValue("@ChallanNo", invoice_no);
                                        cmd.Parameters.AddWithValue("@ProductId", ((Label)gd_Quotation.Rows[i].FindControl("Product_code")).Text);
                                        cmd.Parameters.AddWithValue("@ProductCode", ((Label)gd_Quotation.Rows[i].FindControl("product_id")).Text);
                                        cmd.Parameters.AddWithValue("@ProductName", ((Label)gd_Quotation.Rows[i].FindControl("ProductName")).Text);
                                        cmd.Parameters.AddWithValue("@Quantity", QtyText);
                                        cmd.Parameters.AddWithValue("@ItemNo", ((Label)gd_Quotation.Rows[i].FindControl("ItemNo")).Text);
                                        cmd.Parameters.AddWithValue("@MaterialNo", ((Label)gd_Quotation.Rows[i].FindControl("MaterialNo")).Text);
                                        cmd.Parameters.AddWithValue("@PackSize", ((Label)gd_Quotation.Rows[i].FindControl("PackSize")).Text);

                                        cmd.ExecuteNonQuery();
                                    }
                                    k++;
                                }
                            }
                        }

                        // --- INSERT 3: Factory/Site Addresses ---
                        string queryAddress = "INSERT INTO tbl_ChaSiteAddress(Cha_no, SiteAddress) VALUES (@Cha_no, @SiteAddress)";
                        for (int i = 0; i < FactoryAddress.Items.Count; i++)
                        {
                            if (FactoryAddress.Items[i].Selected)
                            {
                                using (SqlCommand cmd = new SqlCommand(queryAddress, con, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@Cha_no", invoice_no);
                                    cmd.Parameters.AddWithValue("@SiteAddress", FactoryAddress.Items[i].Text);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        // 4. COMMIT THE TRANSACTION (If we made it this far, everything worked!)
                        transaction.Commit();

                        // 5. Update UI for Success
                        Button1.Visible = false;
                        PanelError.Visible = false;
                        lblErrorMsg.Text = String.Empty;
                        PanelOK.Visible = true;
                        lblOk.Text = $"Data Saved Successfully! Generated Challan No: {invoice_no}";

                        // 6. Clear state to prevent duplicate submissions
                        ViewState["ViewQProductData"] = null;
                        gd_Quotation.DataSource = null;
                        gd_Quotation.DataBind();
                    }
                    catch (Exception ex)
                    {
                        // SOMETHING FAILED! Roll back all database changes
                        transaction.Rollback();

                        PanelError.Visible = true;
                        lblErrorMsg.Text = "An error occurred while saving the Challan. No data was saved. Error: " + ex.Message;
                        // Note: In a production environment, log 'ex' using your logging framework rather than displaying it fully to the user.
                    }
                }
            }
        }

        private void insertCorRegFacAddress(string invoice_no)
        {
            for (int i = 0; i < FactoryAddress.Items.Count; i++)
            {
                if (FactoryAddress.Items[i].Selected)
                {
                    string listsite_details = FactoryAddress.Items[i].Text;
                    string query = "insert into tbl_ChaSiteAddress(Cha_no,SiteAddress) values (@Cha_no,@SiteAddress)";
                    SqlParameter[] pram = {
                         new SqlParameter("@Cha_no",invoice_no),
                         new SqlParameter("@SiteAddress",listsite_details)
                    };
                    DbCL.SPExecDB(query, pram);
                }
            }
        }

        private string BindInvoiceNo()
        {
            string prefix = "CHL/FE/";
            string finYear = findmonth();
            string fullPrefix = prefix + finYear;
            int nextNumber = idreturn(fullPrefix);
            string invoiceNo;

            do
            {
                nextNumber += 1;
                invoiceNo = fullPrefix + nextNumber.ToString();
            }
            while (InvoiceNoExists(invoiceNo));

            return invoiceNo;
        }

        private int idreturn(string prefix)
        {
            int lastNumber = 0;
            string query = "SELECT TOP 1 Chalan_No FROM tbl_Chalan WHERE Chalan_No LIKE @Prefix + '%' ORDER BY ID DESC";
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Prefix", prefix);
                con.Open();
                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    string invoiceNo = result.ToString().Trim();
                    string[] parts = invoiceNo.Split('/');
                    int parsedNumber = 0;
                    if (parts.Length >= 4 && int.TryParse(parts[parts.Length - 1], out parsedNumber))
                    {
                        lastNumber = parsedNumber;
                    }
                }
            }
            return lastNumber;
        }

        private bool InvoiceNoExists(string invoiceNo)
        {
            bool exists = false;
            string query = "SELECT COUNT(*) FROM tbl_Chalan WHERE Chalan_No = @Chalan_No";
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Chalan_No", invoiceNo);
                con.Open();
                int count = (int)cmd.ExecuteScalar();
                exists = count > 0;
            }
            return exists;
        }

        private int idreturn_OLD()
        {
            string a = null;
            int b = 0;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select top(1) Sl_no from tbl_Chalan order by ID desc";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                a = re["Sl_no"].ToString();
                b = Convert.ToInt32(a);
            }
            else
            {
                b = 0;
            }
            DbCL.Conn.Close();
            return b;
        }

        private string findmonth()
        {
            string MonthName = "-";
            string a = txtinvoiceDate.Text.Substring(3, 3);
            string b = txtinvoiceDate.Text.Substring(9, 2);
            if (a == "Jan" || a == "Feb" || a == "Mar")
            {
                MonthName = (Convert.ToInt32(b) - 1).ToString() + "-" + b + "/";
            }
            else
            {
                MonthName = b + "-" + (Convert.ToInt32(b) + 1).ToString() + "/";
            }
            return MonthName;
        }

        protected void gd_Quotation_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (e.Row.RowIndex == 0)
                {
                    totalQuoted = 0;
                    totalDelivered = 0;
                    totalDue = 0;
                }

                Label lblQuoted = (Label)e.Row.FindControl("Quantity");
                Label lblDelivered = (Label)e.Row.FindControl("DeliveredQnt");
                TextBox txtDue = (TextBox)e.Row.FindControl("Qty");

                TableCell quotedCell = lblQuoted?.Parent as TableCell;
                TableCell deliveredCell = lblDelivered?.Parent as TableCell;

                int quoted = 0;
                int delivered = 0;
                int due = 0;

                int.TryParse(Convert.ToString(DataBinder.Eval(e.Row.DataItem, "Quantity")).Trim(), out quoted);
                int.TryParse(Convert.ToString(DataBinder.Eval(e.Row.DataItem, "DeliveredQnt")).Trim(), out delivered);
                int.TryParse(Convert.ToString(DataBinder.Eval(e.Row.DataItem, "RemainQny")).Trim(), out due);

                totalQuoted += quoted;
                totalDelivered += delivered;
                totalDue += due;

                if (lblQuoted != null && lblDelivered != null && quotedCell != null && deliveredCell != null)
                {
                    if (quoted == delivered)
                    {
                        lblQuoted.ForeColor = System.Drawing.Color.DarkBlue;
                        lblDelivered.ForeColor = System.Drawing.Color.DarkBlue;
                        quotedCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#e6f2ff");
                        deliveredCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#e6f2ff");
                    }
                    else if (quoted > delivered)
                    {
                        lblQuoted.ForeColor = System.Drawing.Color.Red;
                        lblDelivered.ForeColor = System.Drawing.Color.OrangeRed;
                        quotedCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#ffe6e6");
                        deliveredCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#fff0e6");
                    }
                    else
                    {
                        lblQuoted.ForeColor = System.Drawing.Color.Orange;
                        lblDelivered.ForeColor = System.Drawing.Color.Green;
                        quotedCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#fff0cc");
                        deliveredCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#e6ffe6");
                    }
                }
            }

            if (e.Row.RowType == DataControlRowType.Footer)
            {
                Label lblTotalQuoted = (Label)e.Row.FindControl("lblTotalQuoted");
                Label lblTotalDelivered = (Label)e.Row.FindControl("lblTotalDelivered");
                Label lblTotalDue = (Label)e.Row.FindControl("lblTotalDue");

                if (lblTotalQuoted != null) lblTotalQuoted.Text = totalQuoted.ToString();
                if (lblTotalDelivered != null) lblTotalDelivered.Text = totalDelivered.ToString();
                if (lblTotalDue != null) lblTotalDue.Text = totalDue.ToString();
            }
        }

        [WebMethod]
        public static List<string> GetDocumentNumbers(string prefixText)
        {
            List<string> docNumbers = new List<string>();

            // Search across Quotation_no, PO_Number, and DO_Number
            // We filter out 'N/A' and empty values to keep the suggestions clean
            string query = @"
                SELECT TOP 15 DocNo FROM (
                    SELECT Quotation_no AS DocNo FROM tbl_Quotation WHERE Quotation_no LIKE @Prefix
                    UNION
                    SELECT PO_Number AS DocNo FROM tbl_Quotation WHERE PO_Number LIKE @Prefix AND PO_Number <> 'N/A' AND PO_Number <> ''
                    UNION
                    SELECT DO_Number AS DocNo FROM tbl_Quotation WHERE DO_Number LIKE @Prefix AND DO_Number <> 'N/A' AND DO_Number <> ''
                ) AS TempDocs
                ORDER BY DocNo";

            // Use your existing connection string from Web.config
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Adding % wildcard to match the prefix anywhere in the document string
                    cmd.Parameters.AddWithValue("@Prefix", "%" + prefixText + "%");
                    conn.Open();

                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            docNumbers.Add(sdr["DocNo"].ToString());
                        }
                    }
                }
            }
            return docNumbers;
        }

        [WebMethod]
        public static List<string> GetClientNames(string prefixText)
        {
            List<string> clientNames = new List<string>();

            // Fetch the top 15 matching client names
            string query = "SELECT TOP 15 Client_Name FROM tbl_Client WHERE Client_Name LIKE @Prefix ORDER BY Client_Name";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // % wildcard allows searching parts of the name
                    cmd.Parameters.AddWithValue("@Prefix", "%" + prefixText + "%");
                    conn.Open();

                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            clientNames.Add(sdr["Client_Name"].ToString());
                        }
                    }
                }
            }
            return clientNames;
        }
    }
}