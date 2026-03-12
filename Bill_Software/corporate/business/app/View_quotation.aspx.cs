using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;
using System.IO;
using ClosedXML.Excel; // Our newly installed library!
using System.Web.UI;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm23 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        // You can keep your dtmain and cultureInfo if you plan to use them later, 
        // but I have cleaned them up here since they weren't being used in the current flow.

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                // Updated query using OUTER APPLY to fix the empty Product Category issue
                string cmdstring = @"
                    SELECT 
                        s.PServiceName, 
                        q.ID,
                        q.service_tax1, 
                        q.sub_total, 
                        q.Quotation_no, 
                        q.Quotation_date, 
                        q.Gross, 
                        q.Service_tax, 
                        q.Net_amount,
                        q.cgstOrsgst,
                        c.Client_Name 
                    FROM tbl_Quotation q
                    LEFT OUTER JOIN tbl_Client c ON q.Client_Id = c.Client_Id 
                    OUTER APPLY (
                        SELECT TOP 1 PServiceName 
                        FROM tbl_QuoPriSerTogather 
                        WHERE qutno = q.Quotation_no 
                        ORDER BY TimeStamp DESC
                    ) s
                    WHERE q.RecordType = 'Quotation' 
                      AND MONTH(CAST(q.Quotation_date as date)) = MONTH(GETDATE()) 
                      AND YEAR(CAST(q.Quotation_date as date)) = YEAR(GETDATE())
                    ORDER BY CAST(q.Quotation_date as date) DESC";

                Binddata(cmdstring);
            }
        }

        private void Binddata(string query)
        {
            DataList1.DataSource = null;
            DataList1.DataBind();

            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(query, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        protected void DataList1_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Label lblSlNo = (Label)e.Item.FindControl("lblSlNo");
                if (lblSlNo != null)
                {
                    lblSlNo.Text = (e.Item.ItemIndex + 1).ToString();
                }
            }
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            if (e.CommandName == "View")
            {
                string ID = Convert.ToString(e.CommandArgument);
                Response.Redirect("/corporate/business/print/NewQuotation.aspx?ID=" + ID);
            }
        }

        // --- EXCEL EXPORT METHOD ---
        protected void btnExport_Click_OLD(object sender, EventArgs e)
        {
            // Join Master, Header, and Detail tables for current month
            string query = @"
                SELECT 
                    q.Quotation_no AS [Quotation Number], 
                    q.Quotation_date AS [Quotation Date], 
                    c.Client_Name AS [Client Name], 
                    qd.Product_name AS [Product / Service Name],
                    qd.specification AS [Specification],
                    qd.Quantity AS [Quantity],
                    qd.sail_rate AS [Unit Rate],
                    qd.Total_sail_rate AS [Line Total],
                    q.sub_total AS [Quotation Sub Total],
                    q.service_tax1 AS [GST Amount],
                    q.Net_amount AS [Quotation Net Amount]
                FROM tbl_Quotation q
                LEFT JOIN tbl_Client c ON q.Client_Id = c.Client_Id
                LEFT JOIN tbl_Quotaion_details qd ON q.Quotation_no = qd.Quotation_no AND qd.IsDeleted = 0
                WHERE q.RecordType = 'Quotation' 
                  AND MONTH(CAST(q.Quotation_date as date)) = MONTH(GETDATE()) 
                  AND YEAR(CAST(q.Quotation_date as date)) = YEAR(GETDATE())
                ORDER BY CAST(q.Quotation_date as date) DESC";

            DataTable dtExport = new DataTable();

            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            using (SqlCommand cmd = new SqlCommand(query, DbCL.Conn))
            {
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
                    // Add DataTable as a worksheet
                    var ws = wb.Worksheets.Add(dtExport, "Current_Month_Quotations");

                    // Format Header Row
                    ws.Row(1).Style.Font.Bold = true;
                    ws.Row(1).Style.Fill.BackgroundColor = XLColor.Navy;
                    ws.Row(1).Style.Font.FontColor = XLColor.White;

                    // Auto-size all columns for better readability
                    ws.Columns().AdjustToContents();

                    // Prepare Response for download
                    Response.Clear();
                    Response.Buffer = true;
                    Response.Charset = "";
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment;filename=Quotations_" + DateTime.Now.ToString("MMM_yyyy") + ".xlsx");

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
                // Optional: Alert the user that there is no data to export
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('No data available for the current month.');", true);
            }
        }

        // --- ENRICHED EXCEL EXPORT METHOD ---
        protected void btnExport_Click(object sender, EventArgs e)
        {
            // A comprehensive query mapping all data points from your MagicianNew method
            string query = @"
        SELECT 
            -- Document Header Details
            q.RecordType AS [Record Type],
            q.Quotation_no AS [Document Number], 
            q.Quotation_date AS [Document Date], 
            c.Client_Name AS [Client Name], 
            q.PlaceofSupply AS [Place of Supply],
            q.ReferenceName AS [Client Ref Name],
            q.ReferenceId AS [Client Ref ID],
            q.ReferenceDate AS [Client Ref Date],
            
            -- Line Item Details
            qd.ProductOrServiceCat AS [Category],
            qd.Product_name AS [Product/Service Name],
            qd.Product_Code AS [Product ID],
            qd.Product_id AS [HSN Code],
            qd.specification AS [Brand],
            qd.Misc AS [Specification],
            qd.ItemNo AS [Item No],
            qd.MaterialNo AS [Material No],
            qd.PackSize AS [Pack Size],
            qd.Type AS [Item Type],
            qd.Unit AS [Unit of Measure],
            
            -- Rates and Quantities
            qd.Quantity AS [Quantity],
            qd.sail_rate AS [Base Rate],
            qd.discount_rate AS [Discount %],
            qd.new_sailrate AS [Discounted Rate],
            qd.Service_tax_rate AS [Item Tax %],
            qd.Total_sail_rate2 AS [Line Total (Before Tax)],
            qd.Total_sail_rate1 AS [Line Total (After Tax)],
            qd.DeliveryDate AS [Line Delivery Date],
            qd.Department AS [Department],
            qd.ItemRemarks AS [Item Remarks],

            -- Document Level Financials
            q.sub_total AS [Doc Sub Total],
            q.service_tax1 AS [Doc Tax Amount],
            CASE WHEN q.cgstOrsgst = 'YES' THEN 'Yes' ELSE 'No' END AS [Is CGST/SGST],
            CASE WHEN q.igst = 'YES' THEN 'Yes' ELSE 'No' END AS [Is IGST],
            q.TCS_Percent AS [TCS %],
            q.TCS_Amount AS [TCS Amount],
            q.Freight_VAT_Percent AS [Freight Tax %],
            q.Freight_Amount AS [Freight Amount],
            q.OtherCharge_Name AS [Other Charge Name],
            q.OtherCharge_Amount AS [Other Charge Amount],
            q.Net_amount AS [Doc Net Amount],

            -- Terms and Constraints
            q.ValidityDays AS [Validity Days],
            q.DeliveryTenure AS [Delivery Tenure],
            q.PackingCharges AS [Packing Charges],
            q.Remarks AS [Doc Remarks],
            q.DO_Number AS [DO Number],
            q.PO_Number AS [PO Number],
            q.PO_Date AS [PO Date],
            q.Validity_StartDate AS [Validity Start],
            q.Validity_EndDate AS [Validity End]

        FROM tbl_Quotation q
        LEFT JOIN tbl_Client c ON q.Client_Id = c.Client_Id
        LEFT JOIN tbl_Quotaion_details qd ON q.Quotation_no = qd.Quotation_no AND qd.IsDeleted = 0
        WHERE q.RecordType = 'Quotation' 
          AND MONTH(CAST(q.Quotation_date as date)) = MONTH(GETDATE()) 
          AND YEAR(CAST(q.Quotation_date as date)) = YEAR(GETDATE())
        ORDER BY CAST(q.Quotation_date as date) DESC, CAST(qd.Sl_no as int) ASC";

            DataTable dtExport = new DataTable();

            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            using (SqlCommand cmd = new SqlCommand(query, DbCL.Conn))
            {
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
                    // Adding the DataTable this way automatically creates an Excel Table with built-in filters!
                    var ws = wb.Worksheets.Add(dtExport, "Current_Month_Data");

                    // 1. Format Header Row (Overriding the default table style to match your website)
                    var headerRow = ws.Row(1);
                    headerRow.Style.Font.Bold = true;
                    headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#19658A");
                    headerRow.Style.Font.FontColor = XLColor.White;
                    headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // 2. Freeze the top row so it stays visible when scrolling down
                    ws.SheetView.FreezeRows(1);

                    // 3. Format Number Columns (Assuming columns 20 through 27, and 30 through 37 contain rates/totals)
                    var numericColumns = new int[] { 20, 21, 22, 23, 24, 25, 26, 30, 31, 35, 36, 38, 40 };
                    foreach (int col in numericColumns)
                    {
                        ws.Column(col).Style.NumberFormat.Format = "#,##0.00";
                    }

                    // 4. Auto-size all columns to fit the data perfectly
                    ws.Columns().AdjustToContents();

                    // 5. Ensure some specific wide text columns don't get too overwhelmingly wide
                    ws.Column(10).Width = 30; // Product Name
                    ws.Column(14).Width = 30; // Specifications
                    ws.Column(44).Width = 40; // Doc Remarks
                    ws.Style.Alignment.WrapText = true; // Wrap text for long descriptions

                    // Prepare Response for download
                    Response.Clear();
                    Response.Buffer = true;
                    Response.Charset = "";
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment;filename=Detailed_Quotations_" + DateTime.Now.ToString("MMM_yyyy") + ".xlsx");

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
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('No data available for the current month.');", true);
            }
        }
    }
}