using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using ClosedXML.Excel;
using System.Text;

namespace Bill_Software.corporate.business.app
{
    public partial class Search_purchaseorder : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dtmain = new DataTable();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                // Note: Consider filtering Client List by CompanyID as well if Client_Name overlaps across tenants
                DbCL.FillCombo(cmbvendor, "select Client_Name from tbl_Client order by Client_Name");
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            }
        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0) // Only Client
            {
                BuindCompanyId();
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.Client_Id='" + lblclientId.Text + "' and tbl_Quotation.RecordType='Purchase Order' and tbl_Quotation.CompanyID = " + CompanyContext.CurrentCompanyID + " order by tbl_Quotation.ID desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1) // Only Date
            {
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' and tbl_Quotation.RecordType='Purchase Order' and tbl_Quotation.CompanyID = " + CompanyContext.CurrentCompanyID + " order by tbl_Quotation.ID desc";
                Buinddatagrid(cmdstring);
            }
            else // Client & Date
            {
                BuindCompanyId();
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.Client_Id='" + lblclientId.Text + "' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' and tbl_Quotation.RecordType='Purchase Order' and tbl_Quotation.CompanyID = " + CompanyContext.CurrentCompanyID + " order by tbl_Quotation.ID desc";
                Buinddatagrid(cmdstring);
            }
            btnSertch.Visible = false;
        }

        // --- ENRICHED EXCEL EXPORT METHOD ---
        protected void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                PanelError.Visible = false;

                // Get Client ID if needed
                if (RadioButtonList1.SelectedIndex == 0 || RadioButtonList1.SelectedIndex == 2)
                {
                    BuindCompanyId();
                }

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
                    WHERE q.RecordType = 'Purchase Order' 
                      AND q.CompanyID = @CompanyID ");

                List<SqlParameter> sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID)
                };

                // Add conditional logic based on Search Type
                if (RadioButtonList1.SelectedIndex == 0) // Only Client
                {
                    query.Append(" AND q.Client_Id = @ClientId ");
                    sqlParams.Add(new SqlParameter("@ClientId", lblclientId.Text));
                }
                else if (RadioButtonList1.SelectedIndex == 1) // Only Date
                {
                    query.Append(" AND CAST(q.Quotation_date as datetime) BETWEEN CAST(@FromDate as datetime) AND CAST(@ToDate as datetime) ");
                    sqlParams.Add(new SqlParameter("@FromDate", txttodate.Text));
                    sqlParams.Add(new SqlParameter("@ToDate", txtfromDate.Text + " 23:59:59"));
                }
                else // Client & Date
                {
                    query.Append(" AND q.Client_Id = @ClientId AND CAST(q.Quotation_date as datetime) BETWEEN CAST(@FromDate as datetime) AND CAST(@ToDate as datetime) ");
                    sqlParams.Add(new SqlParameter("@ClientId", lblclientId.Text));
                    sqlParams.Add(new SqlParameter("@FromDate", txttodate.Text));
                    sqlParams.Add(new SqlParameter("@ToDate", txtfromDate.Text + " 23:59:59"));
                }

                query.Append(" ORDER BY CAST(q.Quotation_date as datetime) DESC, CAST(qd.Sl_no as int) ASC");

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
                        var ws = wb.Worksheets.Add(dtExport, "Filtered_PO_Data");

                        var headerRow = ws.Row(1);
                        headerRow.Style.Font.Bold = true;
                        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#19658A");
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
                        Response.AddHeader("content-disposition", "attachment;filename=" + CompanyContext.CurrentCompanyCode + "_PO_Export_" + DateTime.Now.ToString("dd_MMM_yyyy") + ".xlsx");

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
                    PanelError.Visible = true;
                    lblErrorMsg.Text = "No data available to export for the selected filters.";
                }
            }
            catch (Exception ex)
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "An error occurred while generating the export file.";
            }
        }

        private void Buinddatagrid(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                Buinddatagrid1(cmdstring);
            }
            else
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "No Data Found...";
            }
            DbCL.Conn.Close();
        }

        private void Buinddatagrid1(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd1 = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd1.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        private void BuindCompanyId()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Id from tbl_Client where Client_Name='" + cmbvendor.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblclientId.Text = re["Client_Id"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/Search_purchaseorder.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            // Only handle the intended command
            if (!string.Equals(e.CommandName, "View", StringComparison.OrdinalIgnoreCase))
                return;

            // Validate/normalize ID
            var id = Convert.ToString(e.CommandArgument)?.Trim();
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            DateTime fromDate;
            var rawDate = buindalldata(id);
            if (!DateTime.TryParse(rawDate, CultureInfo.GetCultureInfo("en-IN"),
                                   DateTimeStyles.AssumeLocal, out fromDate))
            {
                fromDate = DateTime.MinValue;
            }

            DateTime cutoverDate;
            if (!DateTime.TryParseExact("12-Jun-2018", "dd-MMM-yyyy",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out cutoverDate))
            {
                cutoverDate = new DateTime(2018, 6, 12);
            }

            string page = "~/corporate/business/print/NewPurchaseOrder.aspx";

            // Build a safe, app-root-relative URL
            var url = ResolveClientUrl(page) + "?ID=" + Server.UrlEncode(id);

            string script = $"window.open('{url}', '_blank');";
            if (ScriptManager.GetCurrent(Page) != null)
                ScriptManager.RegisterStartupScript(Page, GetType(), "openPO_" + id, script, true);
            else
                ClientScript.RegisterStartupScript(GetType(), "openPO_" + id, script, true);
        }

        private string buindalldata(string ID)
        {
            string qdate = "";
            string query = "select Quotation_no,Quotation_date,Client_Id,sub_total,Service_tax,Net_amount,cgstOrsgst,igst from tbl_Quotation where ID=@ID";
            SqlParameter[] pram = {
            new SqlParameter("@id",ID)
            };
            dtmain = DbCL.SPreturn_dt(query, pram);
            if (dtmain.Rows.Count > 0)
            {
                string qutno = dtmain.Rows[0]["Quotation_no"].ToString();
                qdate = dtmain.Rows[0]["Quotation_date"].ToString();
            }
            return qdate;
        }
    }
}