using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm27 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();

        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. Prevent Ghost Execution if User is Not Logged In
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                // 2. Default to Last 30 Days to save database resources on initial load
                txtFromDate.Text = DateTime.Now.AddDays(-30).ToString("dd-MMM-yyyy");
                txtToDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");

                BindData();
            }
        }

        private void BindData()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            try
            {
                // Core Query incorporating all requested financial and tracking columns
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

                // Dynamic Search Filters preventing SQL Injection
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

                // Safely comparing Varchar Dates
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

                // Final Ordering
                query += " ORDER BY TRY_CONVERT(DATE, a.Invoice_Date, 106) DESC, a.ID DESC;";

                cmd.CommandText = query;
                cmd.Connection = DbCL.Conn;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptInvoices.DataSource = dt;
                rptInvoices.DataBind();

                ShowMsg("", true); // Clear errors
            }
            catch (Exception ex)
            {
                // Soft Error Handling
                ShowMsg("We encountered an issue while loading the invoices. Please try again.", false);

                // WriteLog("View Invoice Grid Error: " + ex.ToString()); // Uncomment if you have your WriteLog method here
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
            txtFromDate.Text = DateTime.Now.AddDays(-30).ToString("dd-MMM-yyyy");
            txtToDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            BindData();
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