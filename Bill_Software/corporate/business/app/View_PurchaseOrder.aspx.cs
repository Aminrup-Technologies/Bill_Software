/* _Updated_PO_Export_CompanyFilter_
When: 05-Apr-2026
Why: Filtered records by CompanyContext to isolate data & implemented line-item Excel Export.
What: Added CompanyID to queries, added ClosedXML Export logic matching search filters.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.IO;
using ClosedXML.Excel;
using System.Web.UI;

namespace Bill_Software.corporate.business.app
{
    public partial class View_PurchaseOrder : System.Web.UI.Page
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
                // Default load: Last 30 days
                txtDateFrom.Text = DateTime.Now.AddDays(-30).ToString("dd-MMM-yyyy");
                txtDateTo.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                BindData();
            }
        }

        private void BindData()
        {
            try
            {
                pnlError.Visible = false;

                StringBuilder query = new StringBuilder();
                query.Append(@"
                    SELECT 
                        ISNULL(tbl_QuoPriSerTogather.PServiceName, 'N/A') AS PServiceName, 
                        tbl_Quotation.Client_Id, tbl_Quotation.ID, 
                        ISNULL(tbl_Quotation.service_tax1, '0.00') AS service_tax1, 
                        ISNULL(tbl_Quotation.sub_total, '0.00') AS sub_total, 
                        tbl_Quotation.Quotation_no, tbl_Quotation.Quotation_date, 
                        ISNULL(tbl_Quotation.Gross, '0.00') AS Gross, 
                        tbl_Quotation.Service_tax, 
                        ISNULL(tbl_Quotation.Net_amount, '0.00') AS Net_amount, 
                        tbl_Quotation.cgstOrsgst, 
                        ISNULL(tbl_Client.Client_Name, 'Unknown') AS Client_Name, 
                        ISNULL(tbl_Quotation.PO_Number, '-') AS PO_Number, 
                        ISNULL(tbl_Quotation.DO_Number, '-') AS DO_Number, 
                        tbl_Quotation.Validity_StartDate, tbl_Quotation.Validity_EndDate, 
                        tbl_Quotation.AddedById, tbl_Quotation.TimsStamp, 
                        ISNULL(tbl_login.Name, 'System') AS AddedByName 
                    FROM tbl_Quotation 
                    LEFT JOIN tbl_Client ON tbl_Quotation.Client_Id = tbl_Client.Client_Id 
                    LEFT JOIN tbl_QuoPriSerTogather ON tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no 
                        AND tbl_QuoPriSerTogather.TimeStamp = tbl_Quotation.TimsStamp 
                    LEFT JOIN tbl_login ON tbl_login.User_Id = tbl_Quotation.AddedById 
                    WHERE tbl_Quotation.RecordType != 'Quotation' 
                      AND tbl_Quotation.CompanyID = @CompanyID ");

                List<SqlParameter> sqlParams = new List<SqlParameter>();
                sqlParams.Add(new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID));

                // Dynamic Filters
                if (!string.IsNullOrWhiteSpace(txtQuotationNo.Text))
                {
                    query.Append(" AND tbl_Quotation.Quotation_no LIKE @QuoNo ");
                    sqlParams.Add(new SqlParameter("@QuoNo", "%" + txtQuotationNo.Text.Trim() + "%"));
                }

                if (!string.IsNullOrWhiteSpace(txtArcPoDo.Text))
                {
                    query.Append(" AND (tbl_Quotation.PO_Number LIKE @ArcPoDo OR tbl_Quotation.DO_Number LIKE @ArcPoDo) ");
                    sqlParams.Add(new SqlParameter("@ArcPoDo", "%" + txtArcPoDo.Text.Trim() + "%"));
                }

                if (!string.IsNullOrWhiteSpace(txtCustomerName.Text))
                {
                    query.Append(" AND tbl_Client.Client_Name LIKE @ClientName ");
                    sqlParams.Add(new SqlParameter("@ClientName", "%" + txtCustomerName.Text.Trim() + "%"));
                }

                if (!string.IsNullOrWhiteSpace(txtDateFrom.Text) && !string.IsNullOrWhiteSpace(txtDateTo.Text))
                {
                    query.Append(" AND TRY_CAST(tbl_Quotation.Quotation_date AS datetime) BETWEEN TRY_CAST(@FromDate AS datetime) AND TRY_CAST(@ToDate AS datetime) ");
                    sqlParams.Add(new SqlParameter("@FromDate", txtDateFrom.Text.Trim()));
                    sqlParams.Add(new SqlParameter("@ToDate", txtDateTo.Text.Trim() + " 23:59:59"));
                }

                query.Append(" ORDER BY TRY_CAST(tbl_Quotation.Quotation_date AS datetime) DESC");

                rptPurchaseOrders.DataSource = null;
                rptPurchaseOrders.DataBind();

                DbCL.Sqlconnection();
                DbCL.ConnectDb();

                using (SqlCommand cmd = new SqlCommand(query.ToString(), DbCL.Conn))
                {
                    cmd.Parameters.AddRange(sqlParams.ToArray());
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        rptPurchaseOrders.DataSource = reader;
                        rptPurchaseOrders.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                lblErrorMessage.Text = "An error occurred while loading the data.";
            }
            finally
            {
                if (DbCL.Conn != null && DbCL.Conn.State == ConnectionState.Open) DbCL.Conn.Close();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e) { BindData(); }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtQuotationNo.Text = "";
            txtArcPoDo.Text = "";
            txtCustomerName.Text = "";
            txtDateFrom.Text = DateTime.Now.AddDays(-30).ToString("dd-MMM-yyyy");
            txtDateTo.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            BindData();
        }

        protected void rptPurchaseOrders_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "View")
            {
                string id = Convert.ToString(e.CommandArgument);
                SqlParameter[] pram = {
                    new SqlParameter("@ID", id),
                    new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID)
                };
                DataTable dt = DbCL.SPreturn_dt("SELECT ID FROM tbl_Quotation WHERE ID=@ID AND CompanyID=@CompanyID", pram);
                if (dt == null || dt.Rows.Count == 0)
                    return;

                Response.Redirect("/corporate/business/print/NewPurchaseOrder.aspx?ID=" + id, false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        // --- ENRICHED EXCEL EXPORT METHOD ---
        protected void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append(@"
                    SELECT 
                        q.RecordType AS [Record Type],
                        q.Quotation_no AS [Document Number], 
                        q.PO_Number AS [PO Number],
                        q.DO_Number AS [DO Number],
                        q.Quotation_date AS [Document Date], 
                        c.Client_Name AS [Client Name], 
                        q.PlaceofSupply AS [Place of Supply],
                        q.ReferenceName AS [Client Ref Name],
                        q.ReferenceId AS [Client Ref ID],
                        
                        qd.ProductOrServiceCat AS [Category],
                        qd.Product_name AS [Product/Service Name],
                        qd.Product_Code AS [Product ID],
                        qd.Product_id AS [HSN Code],
                        qd.specification AS [Brand/Specification],
                        qd.Quantity AS [Quantity],
                        qd.Unit AS [Unit of Measure],
                        
                        qd.sail_rate AS [Base Rate],
                        qd.discount_rate AS [Discount %],
                        qd.new_sailrate AS [Discounted Rate],
                        qd.Service_tax_rate AS [Item Tax %],
                        qd.Total_sail_rate2 AS [Line Total (Before Tax)],
                        qd.Total_sail_rate1 AS [Line Total (After Tax)],
                        
                        q.sub_total AS [Doc Sub Total],
                        q.service_tax1 AS [Doc Tax Amount],
                        CASE WHEN q.cgstOrsgst = 'YES' THEN 'Yes' ELSE 'No' END AS [Is CGST/SGST],
                        CASE WHEN q.igst = 'YES' THEN 'Yes' ELSE 'No' END AS [Is IGST],
                        q.Net_amount AS [Doc Net Amount],

                        q.ValidityDays AS [Validity Days],
                        q.DeliveryTenure AS [Delivery Tenure],
                        q.Remarks AS [Doc Remarks]
                    FROM tbl_Quotation q
                    LEFT JOIN tbl_Client c ON q.Client_Id = c.Client_Id
                    LEFT JOIN tbl_Quotaion_details qd ON q.Quotation_no = qd.Quotation_no AND qd.IsDeleted = 0
                    WHERE q.RecordType != 'Quotation' 
                      AND q.CompanyID = @CompanyID ");

                List<SqlParameter> sqlParams = new List<SqlParameter>();
                sqlParams.Add(new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID));

                // Apply current UI filters to Export
                if (!string.IsNullOrWhiteSpace(txtQuotationNo.Text))
                {
                    query.Append(" AND q.Quotation_no LIKE @QuoNo ");
                    sqlParams.Add(new SqlParameter("@QuoNo", "%" + txtQuotationNo.Text.Trim() + "%"));
                }
                if (!string.IsNullOrWhiteSpace(txtArcPoDo.Text))
                {
                    query.Append(" AND (q.PO_Number LIKE @ArcPoDo OR q.DO_Number LIKE @ArcPoDo) ");
                    sqlParams.Add(new SqlParameter("@ArcPoDo", "%" + txtArcPoDo.Text.Trim() + "%"));
                }
                if (!string.IsNullOrWhiteSpace(txtCustomerName.Text))
                {
                    query.Append(" AND c.Client_Name LIKE @ClientName ");
                    sqlParams.Add(new SqlParameter("@ClientName", "%" + txtCustomerName.Text.Trim() + "%"));
                }
                if (!string.IsNullOrWhiteSpace(txtDateFrom.Text) && !string.IsNullOrWhiteSpace(txtDateTo.Text))
                {
                    query.Append(" AND TRY_CAST(q.Quotation_date AS datetime) BETWEEN TRY_CAST(@FromDate AS datetime) AND TRY_CAST(@ToDate AS datetime) ");
                    sqlParams.Add(new SqlParameter("@FromDate", txtDateFrom.Text.Trim()));
                    sqlParams.Add(new SqlParameter("@ToDate", txtDateTo.Text.Trim() + " 23:59:59"));
                }

                query.Append(" ORDER BY TRY_CAST(q.Quotation_date AS datetime) DESC, CAST(qd.Sl_no as int) ASC");

                DataTable dtExport = new DataTable();

                DbCL.Sqlconnection();
                DbCL.ConnectDb();
                using (SqlCommand cmd = new SqlCommand(query.ToString(), DbCL.Conn))
                {
                    cmd.Parameters.AddRange(sqlParams.ToArray());
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dtExport);
                    }
                }
                DbCL.Conn.Close();

                if (dtExport.Rows.Count > 0)
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add(dtExport, "Purchase_Orders");

                        var headerRow = ws.Row(1);
                        headerRow.Style.Font.Bold = true;
                        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a6083");
                        headerRow.Style.Font.FontColor = XLColor.White;
                        headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        ws.SheetView.FreezeRows(1);

                        // Format Numeric Columns
                        var numericColumns = new int[] { 15, 17, 18, 19, 20, 21, 22, 23, 24, 27 };
                        foreach (int col in numericColumns)
                        {
                            ws.Column(col).Style.NumberFormat.Format = "#,##0.00";
                        }

                        ws.Columns().AdjustToContents();
                        ws.Column(11).Width = 35; // Product Name
                        ws.Column(14).Width = 30; // Specifications
                        ws.Column(30).Width = 40; // Doc Remarks
                        ws.Style.Alignment.WrapText = true;

                        Response.Clear();
                        Response.Buffer = true;
                        Response.Charset = "";
                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        Response.AddHeader("content-disposition", "attachment;filename=" + CompanyContext.CurrentCompanyCode + "_PurchaseOrders_" + DateTime.Now.ToString("MMM_yyyy") + ".xlsx");

                        using (MemoryStream MyMemoryStream = new MemoryStream())
                        {
                            wb.SaveAs(MyMemoryStream);
                            MyMemoryStream.WriteTo(Response.OutputStream);
                            Response.Flush();
                            Response.End();
                        }
                    }
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('No data available for the selected filters.');", true);
                }
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                lblErrorMessage.Text = "An error occurred while exporting the data.";
            }
        }

        // --- WEB METHODS FOR AJAX AUTOCOMPLETE ---

        [WebMethod(EnableSession = true)]
        public static List<string> GetClientNames(string prefix)
        {
            return GetAutocompleteData("SELECT DISTINCT Client_Name FROM tbl_Client WHERE CompanyID=@CompanyID AND Client_Name LIKE @prefix", prefix);
        }

        [WebMethod(EnableSession = true)]
        public static List<string> GetQuotationNos(string prefix)
        {
            return GetAutocompleteData("SELECT DISTINCT Quotation_no FROM tbl_Quotation WHERE RecordType != 'Quotation' AND CompanyID=@CompanyID AND Quotation_no LIKE @prefix", prefix);
        }

        [WebMethod(EnableSession = true)]
        public static List<string> GetArcPoDoNos(string prefix)
        {
            string query = @"
                SELECT DISTINCT PO_Number FROM tbl_Quotation 
                WHERE RecordType != 'Quotation' AND CompanyID=@CompanyID AND PO_Number LIKE @prefix AND PO_Number IS NOT NULL AND PO_Number != ''
                UNION
                SELECT DISTINCT DO_Number FROM tbl_Quotation 
                WHERE RecordType != 'Quotation' AND CompanyID=@CompanyID AND DO_Number LIKE @prefix AND DO_Number IS NOT NULL AND DO_Number != ''";

            return GetAutocompleteData(query, prefix);
        }

        private static List<string> GetAutocompleteData(string query, string prefix)
        {
            List<string> suggestions = new List<string>();
            if (HttpContext.Current == null || HttpContext.Current.Session == null || HttpContext.Current.Session["USERID"] == null)
                return suggestions;

            DB_UTILITY db = new DB_UTILITY();

            try
            {
                db.Sqlconnection();
                db.ConnectDb();
                using (SqlCommand cmd = new SqlCommand(query, db.Conn))
                {
                    cmd.Parameters.AddWithValue("@prefix", "%" + prefix + "%");
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            suggestions.Add(sdr[0].ToString());
                        }
                    }
                }
            }
            catch { /* Ignore minor autocomplete failures */ }
            finally { if (db.Conn != null && db.Conn.State == ConnectionState.Open) db.Conn.Close(); }

            return suggestions;
        }
    }
}