using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm28 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadClients();
                // Dates are left intentionally blank here so the user can query all history unless they specify a date
            }
        }

        // -------------------------------------------------------------------------
        // Dropdown: Enforces CompanyContext Segregation
        // -------------------------------------------------------------------------
        private void LoadClients()
        {
            try
            {
                DbCL.Sqlconnection();
                DbCL.ConnectDb();

                string query = "SELECT Client_Name FROM tbl_Client WHERE CompanyID = @CompanyID ORDER BY Client_Name";
                using (SqlCommand cmd = new SqlCommand(query, DbCL.Conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        cmbvendor.Items.Clear();
                        cmbvendor.Items.Add(new ListItem("-- All Clients --", ""));
                        while (dr.Read())
                        {
                            cmbvendor.Items.Add(dr["Client_Name"].ToString());
                        }
                    }
                }
            }
            finally
            {
                if (DbCL.Conn.State == ConnectionState.Open) DbCL.Conn.Close();
            }
        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            BindData();
        }

        private void BindData()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            try
            {
                // Unified Modern Query (Same rich output as View_Invoice)
                string query = @"SELECT a.ID, a.Invoice_No, a.Invoice_Date, a.Quotation_No, a.Quotation_Date, 
                 a.ExtInvoiceNo, a.Gross, a.discount, a.Delivery_Amount, a.otherAmount1, 
                 a.cgstOrsgst, a.igst, 
                 TRY_CAST(a.Net_Amount AS FLOAT) AS Net_Amount, 
                 TRY_CAST(a.Service_Tax1 AS FLOAT) AS Gst, 
                 a.mailDate, 
                 TRY_CAST(a.Net_Amount AS FLOAT) - TRY_CAST(a.Service_Tax1 AS FLOAT) AS sub_total, 
                 b.Client_Name, c.PServiceName, q.PO_Number, q.DO_Number, 
                 q.Validity_StartDate, q.Validity_EndDate, a.AddedById, 
                 ISNULL(l.Name, a.AddedById) AS AddedByName, a.TimeStamp 
                 FROM tbl_Invoice AS a 
                 LEFT JOIN tbl_Client AS b ON b.Client_Id = a.Client_ID 
                 LEFT JOIN tbl_QuoPriSerTogather AS c ON a.Quotation_No = c.qutno 
                 LEFT JOIN tbl_Quotation AS q ON q.Quotation_no = a.Quotation_No 
                 LEFT JOIN tbl_login AS l ON l.User_Id = a.AddedById 
                 WHERE a.CompanyID = @CompanyID ";

                SqlCommand cmd = new SqlCommand();
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                AppendInvoiceFilters(ref query, cmd);

                query += " ORDER BY TRY_CONVERT(DATE, a.Invoice_Date, 106) DESC, a.ID DESC;";

                cmd.CommandText = query;
                cmd.Connection = DbCL.Conn;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptInvoices.DataSource = dt;
                rptInvoices.DataBind();

                ShowMsg($"Search completed. Found {dt.Rows.Count} records.", true);

                // PROJECT FLMX RULE: PROACTIVE NOTIFICATION LOGGING
                InsertSystemNotification(
                    "Advanced Invoice Search",
                    $"User searched and found {dt.Rows.Count} invoice records.",
                    "Search",
                    "Info",
                    Session["USERID"].ToString()
                );
            }
            catch (Exception ex)
            {
                ShowMsg("We encountered an issue during the search. Please try again.", false);
            }
            finally
            {
                if (DbCL.Conn.State == ConnectionState.Open) DbCL.Conn.Close();
            }
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            DataTable dtExport = new DataTable();

            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            try
            {
                string query = @"SELECT 
                    a.Invoice_No AS [Invoice Number], 
                    a.Invoice_Date AS [Invoice Date], 
                    a.ExtInvoiceNo AS [ERP Ref], 
                    b.Client_Name AS [Client Name], 
                    a.Quotation_No AS [Source Reference], 
                    q.PO_Number AS [PO Number], 
                    q.DO_Number AS [DO Number], 
                    ps.PrimaryService AS [Primary Service], 
                    d.Product_id AS [Item Code], 
                    d.Product_Code AS [HSN Code], 
                    d.Product_name AS [Item Name], 
                    d.Quantity AS [Qty], 
                    d.sail_rate AS [Rate], 
                    d.discountRate AS [Line Discount %], 
                    ISNULL(d.Total_sail_rate2, (TRY_CAST(d.Quantity AS FLOAT) * TRY_CAST(d.sail_rate AS FLOAT))) AS [Taxable Value],
                    d.Service_tax_rate AS [GST %],
                    d.Total_sail_rate1 AS [Item Net Value],
                    a.Service_Tax1 AS [Invoice GST Amount],
                    a.Net_Amount AS [Invoice Grand Total],
                    a.Delivery_Amount AS [Freight],
                    a.otherAmount1 AS [Other Charges],
                    a.Quotation_Date AS [Quotation Date],
                    a.mailDate AS [Mail Date],
                    CASE WHEN a.cgstOrsgst = 'YES' THEN 'CGST/SGST' WHEN a.igst = 'YES' THEN 'IGST' ELSE 'TAX' END AS [Tax Type],
                    a.TimeStamp AS [Created Timestamp],
                    a.AddedById AS [Created By]
                    FROM tbl_Invoice AS a 
                    LEFT JOIN tbl_Client AS b ON b.Client_Id = a.Client_ID AND b.CompanyID = @CompanyID 
                    LEFT JOIN tbl_Invoice_details AS d ON d.Invoice_No = a.Invoice_No AND d.CompanyID = @CompanyID 
                    LEFT JOIN tbl_Quotation AS q ON q.Quotation_No = a.Quotation_No AND q.CompanyID = @CompanyID 
                    LEFT JOIN (
                        SELECT
                            p1.qut_no,
                            STUFF((
                                SELECT ', ' + p2.PrimaryService
                                FROM tbl_QutPrimaryService p2
                                WHERE p2.qut_no = p1.qut_no
                                  AND p2.CompanyID = @CompanyID
                                FOR XML PATH(''), TYPE
                            ).value('.', 'nvarchar(max)'), 1, 2, '') AS PrimaryService
                        FROM tbl_QutPrimaryService p1
                        WHERE p1.CompanyID = @CompanyID
                        GROUP BY p1.qut_no
                    ) ps ON ps.qut_no = a.Quotation_No
                    WHERE a.CompanyID = @CompanyID ";

                SqlCommand cmd = new SqlCommand();
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                AppendInvoiceFilters(ref query, cmd);

                query += " ORDER BY TRY_CONVERT(DATE, a.Invoice_Date, 106) DESC, a.Invoice_No DESC;";

                cmd.CommandText = query;
                cmd.Connection = DbCL.Conn;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dtExport);
            }
            catch (Exception ex)
            {
                ShowMsg("Error during export: " + ex.Message, false);
                return;
            }
            finally
            {
                if (DbCL.Conn.State == ConnectionState.Open) DbCL.Conn.Close();
            }

            if (dtExport.Rows.Count == 0)
            {
                ShowMsg("No records found to export.", false);
                return;
            }

            InvoiceListHelper.PrepareInvoiceExport(dtExport);
            InvoiceListHelper.ExportXlsx(
                Response,
                dtExport,
                "Invoice_Lines",
                CompanyContext.CurrentCompanyCode + "_Advanced_Search_Invoices_" + DateTime.Now.ToString("yyyyMMdd"));
        }

        private void AppendInvoiceFilters(ref string query, SqlCommand cmd)
        {
            if (cmbvendor.SelectedIndex > 0)
            {
                query += " AND b.Client_Name = @ClientName ";
                cmd.Parameters.AddWithValue("@ClientName", cmbvendor.SelectedItem.Text);
            }
            if (!string.IsNullOrWhiteSpace(txtSearchInv.Text))
            {
                query += " AND a.Invoice_No LIKE @InvNo ";
                cmd.Parameters.AddWithValue("@InvNo", "%" + txtSearchInv.Text.Trim() + "%");
            }
            if (!string.IsNullOrWhiteSpace(txtSearchExt.Text))
            {
                query += " AND a.ExtInvoiceNo LIKE @ExtNo ";
                cmd.Parameters.AddWithValue("@ExtNo", "%" + txtSearchExt.Text.Trim() + "%");
            }
            if (!string.IsNullOrWhiteSpace(txtFromDate.Text))
            {
                query += " AND TRY_CONVERT(DATE, a.Invoice_Date, 106) >= TRY_CONVERT(DATE, @FromDate, 106) ";
                cmd.Parameters.AddWithValue("@FromDate", txtFromDate.Text.Trim());
            }
            if (!string.IsNullOrWhiteSpace(txtToDate.Text))
            {
                query += " AND TRY_CONVERT(DATE, a.Invoice_Date, 106) <= TRY_CONVERT(DATE, @ToDate, 106) ";
                cmd.Parameters.AddWithValue("@ToDate", txtToDate.Text.Trim());
            }
        }

        protected string FmtDate(object v) { return InvoiceListHelper.FmtDate(v); }
        protected string FmtMail(object v) { return InvoiceListHelper.FmtMail(v); }
        protected string FmtStamp(object v) { return InvoiceListHelper.FmtStamp(v); }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            cmbvendor.SelectedIndex = 0;
            txtSearchInv.Text = "";
            txtSearchExt.Text = "";
            txtFromDate.Text = "";
            txtToDate.Text = "";
            rptInvoices.DataSource = null;
            rptInvoices.DataBind();
            ShowMsg("", true);
        }

        private void ShowMsg(string msg, bool isOk)
        {
            PanelMsg.Visible = !string.IsNullOrEmpty(msg);
            lblMsg.Text = msg;
            PanelMsg.Style["background-color"] = isOk ? "#dff0d8" : "#f8d7da";
            PanelMsg.Style["border"] = isOk ? "1px solid #d4edda" : "1px solid #f5c6cb";
            lblMsg.ForeColor = isOk ? System.Drawing.Color.DarkGreen : System.Drawing.Color.DarkRed;
        }

        // ---------------------------------------------------------------------------
        // PROJECT FLMX: MANDATORY PROACTIVE NOTIFICATION LOGGING METHOD
        // ---------------------------------------------------------------------------
        private void InsertSystemNotification(string title, string message, string moduleType, string level, string userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    string query = @"INSERT INTO tbl_SystemNotification 
                                     (Title, Message, ModuleType, AlertLevel, CreatedBy, CreatedDate, IsActive) 
                                     VALUES (@Title, @Message, @ModuleType, @AlertLevel, @CreatedBy, GETDATE(), 1)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Title", title);
                        cmd.Parameters.AddWithValue("@Message", message);
                        cmd.Parameters.AddWithValue("@ModuleType", moduleType);
                        cmd.Parameters.AddWithValue("@AlertLevel", level);
                        cmd.Parameters.AddWithValue("@CreatedBy", userId);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }
    }
}
