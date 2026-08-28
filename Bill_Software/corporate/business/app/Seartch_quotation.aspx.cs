using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.UI.WebControls;
using System.IO;
using ClosedXML.Excel;
using System.Web.UI;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm24 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                BindClients();
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            }
        }

        private void BindClients()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            using (SqlCommand cmd = new SqlCommand("SELECT Client_Name FROM tbl_Client ORDER BY Client_Name", DbCL.Conn))
            {
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    cmbvendor.DataSource = dr;
                    cmbvendor.DataTextField = "Client_Name";
                    cmbvendor.DataValueField = "Client_Name";
                    cmbvendor.DataBind();
                }
            }
            DbCL.Conn.Close();

            cmbvendor.Items.Insert(0, new ListItem("", ""));
        }

        protected void cmbvendor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cmbvendor.SelectedValue))
            {
                pnlClientDetails.Visible = false;
                lblclientId.Text = "";
                return;
            }

            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string query = "SELECT Client_Id, Address1, City, State, pin, Pan_no, Service_tax_no FROM tbl_Client WHERE Client_Name = @ClientName";

            using (SqlCommand cmd = new SqlCommand(query, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@ClientName", cmbvendor.SelectedValue);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        lblclientId.Text = dr["Client_Id"].ToString();

                        string address = dr["Address1"].ToString();
                        string city = dr["City"].ToString();
                        string state = dr["State"].ToString();
                        string pin = dr["pin"].ToString();

                        lblCAddress.Text = string.IsNullOrWhiteSpace(address) ? "N/A" : address;
                        string cityStatePin = $"{city}, {state} - {pin}".Trim(new char[] { ' ', '-', ',' });
                        lblCCityState.Text = string.IsNullOrWhiteSpace(cityStatePin) ? "N/A" : cityStatePin;
                        lblCPan.Text = string.IsNullOrWhiteSpace(dr["Pan_no"].ToString()) ? "N/A" : dr["Pan_no"].ToString();
                        lblCGST.Text = string.IsNullOrWhiteSpace(dr["Service_tax_no"].ToString()) ? "N/A" : dr["Service_tax_no"].ToString();

                        pnlClientDetails.Visible = true;
                    }
                    else
                    {
                        pnlClientDetails.Visible = false;
                    }
                }
            }
            DbCL.Conn.Close();
        }

        // --- SHARED QUERY BUILDER WITH TENANT ISOLATION ---
        private SqlCommand GetSearchCommand(bool isExport)
        {
            BuindCompanyId();

            SqlCommand cmd = new SqlCommand();
            string cmdstring = "";

            if (!isExport)
            {
                // --- INJECTION 1: Added CompanyID filter to the UI Grid Query ---
                cmdstring = @"
                    SELECT 
                        q.ID, q.Quotation_no, q.Quotation_date, q.service_tax1, q.sub_total, 
                        q.Gross, q.Service_tax, q.Net_amount, q.mailStatusDate, 
                        s.PServiceName AS Services, c.Client_Name
                    FROM tbl_Quotation q
                    LEFT JOIN tbl_Client c ON q.Client_Id = c.Client_Id
                    OUTER APPLY (
                        SELECT TOP 1 PServiceName 
                        FROM tbl_QuoPriSerTogather 
                        WHERE qutno = q.Quotation_no 
                        ORDER BY TimeStamp DESC
                    ) s
                    WHERE q.RecordType = 'Quotation' AND q.CompanyID = @CompanyID ";
            }
            else
            {
                // --- INJECTION 2: Added CompanyID filter to the Excel Export Query ---
                cmdstring = @"
                    SELECT 
                        q.RecordType AS [Record Type], q.Quotation_no AS [Document Number], q.Quotation_date AS [Document Date], 
                        c.Client_Name AS [Client Name], q.PlaceofSupply AS [Place of Supply],
                        q.ReferenceName AS [Client Ref Name], q.ReferenceId AS [Client Ref ID], q.ReferenceDate AS [Client Ref Date],
                        qd.ProductOrServiceCat AS [Category], qd.Product_name AS [Product/Service Name], qd.Product_Code AS [Product ID],
                        qd.Product_id AS [HSN Code], qd.specification AS [Brand], qd.Misc AS [Specification],
                        qd.ItemNo AS [Item No], qd.MaterialNo AS [Material No], qd.PackSize AS [Pack Size], qd.Type AS [Item Type], qd.Unit AS [Unit of Measure],
                        qd.Quantity AS [Quantity], qd.sail_rate AS [Base Rate], qd.discount_rate AS [Discount %], qd.new_sailrate AS [Discounted Rate],
                        qd.Service_tax_rate AS [Item Tax %], qd.Total_sail_rate2 AS [Line Total (Before Tax)], qd.Total_sail_rate1 AS [Line Total (After Tax)],
                        qd.DeliveryDate AS [Line Delivery Date], qd.Department AS [Department], qd.ItemRemarks AS [Item Remarks],
                        q.sub_total AS [Doc Sub Total], q.service_tax1 AS [Doc Tax Amount],
                        CASE WHEN q.cgstOrsgst = 'YES' THEN 'Yes' ELSE 'No' END AS [Is CGST/SGST], CASE WHEN q.igst = 'YES' THEN 'Yes' ELSE 'No' END AS [Is IGST],
                        q.TCS_Percent AS [TCS %], q.TCS_Amount AS [TCS Amount], q.Freight_VAT_Percent AS [Freight Tax %], q.Freight_Amount AS [Freight Amount],
                        q.OtherCharge_Name AS [Other Charge Name], q.OtherCharge_Amount AS [Other Charge Amount], q.Net_amount AS [Doc Net Amount],
                        q.ValidityDays AS [Validity Days], q.DeliveryTenure AS [Delivery Tenure], q.PackingCharges AS [Packing Charges],
                        q.Remarks AS [Doc Remarks], q.DO_Number AS [DO Number], q.PO_Number AS [PO Number], q.PO_Date AS [PO Date],
                        q.Validity_StartDate AS [Validity Start], q.Validity_EndDate AS [Validity End]
                    FROM tbl_Quotation q
                    LEFT JOIN tbl_Client c ON q.Client_Id = c.Client_Id
                    LEFT JOIN tbl_Quotaion_details qd ON q.Quotation_no = qd.Quotation_no AND qd.IsDeleted = 0
                    WHERE q.RecordType = 'Quotation' AND q.CompanyID = @CompanyID ";
            }

            // Always bind the active company parameter
            cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);

            string searchType = RadioButtonList1.SelectedValue;

            if (searchType == "Client")
            {
                cmdstring += " AND q.Client_Id = @ClientId";
                cmd.Parameters.AddWithValue("@ClientId", lblclientId.Text);
            }
            else if (searchType == "Date")
            {
                cmdstring += " AND CAST(q.Quotation_date AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)";
                cmd.Parameters.AddWithValue("@FromDate", txtfromDate.Text);
                cmd.Parameters.AddWithValue("@ToDate", txttodate.Text);
            }
            else if (searchType == "ClientDate")
            {
                cmdstring += " AND q.Client_Id = @ClientId AND CAST(q.Quotation_date AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)";
                cmd.Parameters.AddWithValue("@ClientId", lblclientId.Text);
                cmd.Parameters.AddWithValue("@FromDate", txtfromDate.Text);
                cmd.Parameters.AddWithValue("@ToDate", txttodate.Text);
            }
            else if (searchType == "QutNo")
            {
                cmdstring += " AND q.Quotation_no LIKE '%' + @QutNo + '%'";
                cmd.Parameters.AddWithValue("@QutNo", txtQutNo.Text.Trim());
            }

            if (isExport)
            {
                cmdstring += " ORDER BY CAST(q.Quotation_date AS DATE) DESC, CAST(qd.Sl_no as int) ASC";
            }
            else
            {
                cmdstring += " ORDER BY CAST(q.Quotation_date AS DATE) DESC";
            }

            cmd.CommandText = cmdstring;
            cmd.CommandType = CommandType.Text;
            return cmd;
        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            SqlCommand cmd = GetSearchCommand(isExport: false);
            BuinddatagridNew(cmd);

            btnExport.Visible = true;
        }

        public void BuinddatagridNew(SqlCommand cmd)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                cmd.Connection = con;
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    DataList1.DataSource = dt;
                    DataList1.DataBind();
                }
            }
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            SqlCommand cmd = GetSearchCommand(isExport: true);
            DataTable dtExport = new DataTable();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                cmd.Connection = con;
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dtExport);
                }
            }

            if (dtExport.Rows.Count > 0)
            {
                using (XLWorkbook wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add(dtExport, "Search_Results");

                    var headerRow = ws.Row(1);
                    headerRow.Style.Font.Bold = true;
                    headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#19658A");
                    headerRow.Style.Font.FontColor = XLColor.White;
                    headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.SheetView.FreezeRows(1);

                    var numericColumns = new int[] { 20, 21, 22, 23, 24, 25, 26, 30, 31, 35, 36, 38, 40 };
                    foreach (int col in numericColumns)
                    {
                        ws.Column(col).Style.NumberFormat.Format = "#,##0.00";
                    }

                    ws.Columns().AdjustToContents();
                    ws.Column(10).Width = 30;
                    ws.Column(14).Width = 30;
                    ws.Column(44).Width = 40;
                    ws.Style.Alignment.WrapText = true;

                    Response.Clear();
                    Response.Buffer = true;
                    Response.Charset = "";
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                    // --- INJECTION 3: Dynamic File Naming based on active company ---
                    Response.AddHeader("content-disposition", "attachment;filename=" + CompanyContext.CurrentCompanyCode + "_Quotation_Search_Results_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx");

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
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('No data found for this search.');", true);
            }
        }

        private void BuindCompanyId()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Id from tbl_Client where Client_Name = @ClientName";
            using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@ClientName", cmbvendor.Text);
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
            Response.Redirect("~/corporate/business/app/Seartch_quotation.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string ID = Convert.ToString(e.CommandArgument);
            string qdate = buindalldata(ID);

            DateTime fromdate;
            if (DateTime.TryParse(qdate, out fromdate))
            {
                DateTime todate = new DateTime(2018, 6, 12);
                if (e.CommandName == "View")
                {
                    if (fromdate > todate)
                    {
                        Response.Redirect("/corporate/business/print/NewQuotation.aspx?ID=" + ID);
                    }
                    else
                    {
                        Response.Redirect("/corporate/business/print/Quotation.aspx?ID=" + ID);
                    }
                }
            }
        }

        private string buindalldata(string ID)
        {
            string qdate = "";
            // --- INJECTION 4: Strict cross-tenant check. Cannot fetch data outside active CompanyID ---
            string query = "select Quotation_date from tbl_Quotation where ID=@ID AND CompanyID=@CompanyID";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            using (SqlCommand cmd = new SqlCommand(query, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@ID", ID);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    qdate = result.ToString();
                }
            }
            DbCL.Conn.Close();
            return qdate;
        }
    }
}