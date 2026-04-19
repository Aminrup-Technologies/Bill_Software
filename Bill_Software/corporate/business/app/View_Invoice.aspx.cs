using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm27 : System.Web.UI.Page
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
                // Feature: Default to the 1st day of the Current Month
                DateTime now = DateTime.Now;
                txtFromDate.Text = new DateTime(now.Year, now.Month, 1).ToString("dd-MMM-yyyy");
                txtToDate.Text = now.ToString("dd-MMM-yyyy");

                BindData();
            }
        }

        private void BindData()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            try
            {
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
                 WHERE 1=1 ";

                SqlCommand cmd = new SqlCommand();

                // -------------------------------------------------------------------------
                // 1. Company Context Filter Enforced
                // Note: Based on the schema provided, tbl_Invoice doesn't natively have CompanyID.
                // Assuming it's tracked via an added column or prefixed in ExtInvoiceNo. 
                // Enable the line below if the DB schema is updated to include CompanyID.
                // -------------------------------------------------------------------------
                query += " AND a.CompanyID = @CompanyID ";
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

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
                if (!string.IsNullOrWhiteSpace(txtSearchClient.Text))
                {
                    query += " AND b.Client_Name LIKE @Client ";
                    cmd.Parameters.AddWithValue("@Client", "%" + txtSearchClient.Text.Trim() + "%");
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

                query += " ORDER BY TRY_CONVERT(DATE, a.Invoice_Date, 106) DESC, a.ID DESC;";

                cmd.CommandText = query;
                cmd.Connection = DbCL.Conn;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptInvoices.DataSource = dt;
                rptInvoices.DataBind();

                ShowMsg("", true);
            }
            catch (Exception ex)
            {
                ShowMsg("We encountered an issue while loading the invoices. Please try again.", false);
            }
            finally
            {
                if (DbCL.Conn.State == ConnectionState.Open) DbCL.Conn.Close();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            BindData();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearchInv.Text = "";
            txtSearchExt.Text = "";
            txtSearchClient.Text = "";
            DateTime now = DateTime.Now;
            txtFromDate.Text = new DateTime(now.Year, now.Month, 1).ToString("dd-MMM-yyyy");
            txtToDate.Text = now.ToString("dd-MMM-yyyy");
            BindData();
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            // Re-run the filtered query but include Line Item Details for maximum Excel utility
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            try
            {
                string query = @"SELECT 
                    a.Invoice_No AS [Invoice Number], 
                    a.Invoice_Date AS [Invoice Date], 
                    a.ExtInvoiceNo AS [ERP Ref], 
                    b.Client_Name AS [Client Name], 
                    d.Product_id AS [Item Code], 
                    d.Product_Code AS [HSN Code], 
                    d.Product_name AS [Item Name], 
                    d.Quantity AS [Qty], 
                    d.sail_rate AS [Rate], 
                    ISNULL(d.Total_sail_rate2, (TRY_CAST(d.Quantity AS FLOAT) * TRY_CAST(d.sail_rate AS FLOAT))) AS [Taxable Value],
                    d.Service_tax_rate AS [GST %],
                    d.Total_sail_rate1 AS [Item Net Value],
                    a.Net_Amount AS [Invoice Grand Total],
                    a.AddedById AS [Created By]
                    FROM tbl_Invoice AS a 
                    LEFT JOIN tbl_Client AS b ON b.Client_Id = a.Client_ID 
                    LEFT JOIN tbl_Invoice_details AS d ON d.Invoice_No = a.Invoice_No 
                    WHERE 1=1 ";

                SqlCommand cmd = new SqlCommand();

                query += " AND a.CompanyID = @CompanyID ";
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

                // -- Apply exact same filters as UI --
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
                if (!string.IsNullOrWhiteSpace(txtSearchClient.Text))
                {
                    query += " AND b.Client_Name LIKE @Client ";
                    cmd.Parameters.AddWithValue("@Client", "%" + txtSearchClient.Text.Trim() + "%");
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

                query += " ORDER BY TRY_CONVERT(DATE, a.Invoice_Date, 106) DESC, a.Invoice_No DESC;";

                cmd.CommandText = query;
                cmd.Connection = DbCL.Conn;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dtExport = new DataTable();
                da.Fill(dtExport);

                if (dtExport.Rows.Count > 0)
                {
                    ExportDataTableToExcel(dtExport, "Tax_Invoices_Export_" + DateTime.Now.ToString("yyyyMMdd"));

                    // PROJECT FLMX RULE: PROACTIVE NOTIFICATION LOGGING
                    InsertSystemNotification(
                        "Invoices Exported",
                        $"Successfully exported {dtExport.Rows.Count} invoice line items to Excel.",
                        "Invoice",
                        "Info",
                        Session["USERID"].ToString()
                    );
                }
                else
                {
                    ShowMsg("No records found to export.", false);
                }
            }
            catch (Exception ex)
            {
                ShowMsg("Error during export: " + ex.Message, false);
            }
            finally
            {
                if (DbCL.Conn.State == ConnectionState.Open) DbCL.Conn.Close();
            }
        }

        private void ExportDataTableToExcel(DataTable dt, string filename)
        {
            // 1. Change extension to .csv
            string attachment = $"attachment; filename={filename}.csv";
            Response.ClearContent();
            Response.AddHeader("content-disposition", attachment);

            // 2. Change content type to standard CSV
            Response.ContentType = "text/csv";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // 3. Write Columns
            string[] columnNames = new string[dt.Columns.Count];
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                columnNames[i] = EscapeCsvField(dt.Columns[i].ColumnName);
            }
            sb.AppendLine(string.Join(",", columnNames));

            // 4. Write Rows
            foreach (DataRow dr in dt.Rows)
            {
                string[] fields = new string[dt.Columns.Count];
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    fields[i] = EscapeCsvField(dr[i].ToString());
                }
                sb.AppendLine(string.Join(",", fields));
            }

            Response.Write(sb.ToString());
            Response.End();
        }

        // Helper method to ensure commas or quotes inside your data don't break the Excel columns
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";

            // If the data contains a comma, quote, or newline, we must wrap it in quotes for Excel
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\r") || field.Contains("\n"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
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
            catch (Exception ex)
            {
                // Silently swallow logging errors to prevent breaking the main application flow
            }
        }

        private void ShowMsg(string msg, bool isOk)
        {
            PanelMsg.Visible = !string.IsNullOrEmpty(msg);
            lblMsg.Text = msg;
            PanelMsg.Style["background-color"] = isOk ? "#dff0d8" : "#f8d7da";
            PanelMsg.Style["border"] = isOk ? "1px solid #d4edda" : "1px solid #f5c6cb";
            lblMsg.ForeColor = isOk ? System.Drawing.Color.DarkGreen : System.Drawing.Color.DarkRed;
        }
    }
}