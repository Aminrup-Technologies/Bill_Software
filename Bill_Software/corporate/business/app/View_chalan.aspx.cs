using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Services;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm39 : System.Web.UI.Page
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
                // Load the top 100 recent Challans by default to prevent performance lag
                Binddata(true);
            }
        }

        // --- WEB METHODS FOR AUTO-SUGGEST ---
        [WebMethod]
        public static List<string> GetClientNames(string prefixText)
        {
            List<string> clientNames = new List<string>();
            string query = "SELECT TOP 15 Client_Name FROM tbl_Client WHERE Client_Name LIKE @Prefix ORDER BY Client_Name";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
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

        [WebMethod]
        public static List<string> GetDocumentNumbers(string prefixText)
        {
            List<string> docNumbers = new List<string>();
            string query = @"
                SELECT TOP 15 DocNo FROM (
                    SELECT Chalan_No AS DocNo FROM tbl_Chalan WHERE Chalan_No LIKE @Prefix
                    UNION
                    SELECT Quotation_no AS DocNo FROM tbl_Quotation WHERE Quotation_no LIKE @Prefix
                    UNION
                    SELECT PO_Number AS DocNo FROM tbl_Quotation WHERE PO_Number LIKE @Prefix AND PO_Number <> 'N/A' AND PO_Number <> ''
                    UNION
                    SELECT DO_Number AS DocNo FROM tbl_Quotation WHERE DO_Number LIKE @Prefix AND DO_Number <> 'N/A' AND DO_Number <> ''
                ) AS TempDocs
                ORDER BY DocNo";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
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

        // --- SEARCH ACTIONS ---
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Binddata(false);
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtClientName.Text = string.Empty;
            txtDocNumber.Text = string.Empty;
            txtFromDate.Text = string.Empty;
            txtToDate.Text = string.Empty;
            PanelError.Visible = false;

            Binddata(true);
        }

        // --- DATA BINDING LOGIC ---
        private void Binddata(bool isInitialLoad)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            // Use TOP 100 for initial load to protect performance. If searching, return all matches.
            string topClause = isInitialLoad ? "TOP 100" : "";

            string baseQuery = $@"
                SELECT {topClause} a.ID, a.Chalan_No, a.Chalan_Date, a.Quotation_No, a.Quotation_Date, 
                       a.Client_ID, b.Client_Name, ps.Services AS PServiceName, q.DO_Number, q.PO_Number 
                FROM tbl_Chalan AS a 
                LEFT JOIN (SELECT qutno, STRING_AGG(PServiceName, ', ') AS Services FROM tbl_QuoPriSerTogather GROUP BY qutno) AS ps ON a.Quotation_No = ps.qutno 
                LEFT JOIN tbl_Client AS b ON b.Client_Id = a.Client_ID 
                LEFT JOIN tbl_Quotation AS q ON a.Quotation_No = q.Quotation_No 
                WHERE 1=1 ";

            List<SqlParameter> parameters = new List<SqlParameter>();

            // 1. Client Filter
            if (!string.IsNullOrEmpty(txtClientName.Text.Trim()))
            {
                baseQuery += " AND b.Client_Name LIKE @ClientName ";
                parameters.Add(new SqlParameter("@ClientName", "%" + txtClientName.Text.Trim() + "%"));
            }

            // 2. Date Filter
            if (!string.IsNullOrEmpty(txtFromDate.Text) && !string.IsNullOrEmpty(txtToDate.Text))
            {
                baseQuery += " AND CAST(a.Chalan_Date AS datetime) BETWEEN @FromDate AND @ToDate ";
                parameters.Add(new SqlParameter("@FromDate", txtFromDate.Text));
                parameters.Add(new SqlParameter("@ToDate", txtToDate.Text));
            }

            // 3. Smart Document Filter (Searches Challan, QTN, PO, and DO)
            string docSearch = txtDocNumber.Text.Trim();
            if (!string.IsNullOrEmpty(docSearch))
            {
                baseQuery += " AND (a.Chalan_No LIKE @DocNo OR a.Quotation_No LIKE @DocNo OR q.PO_Number LIKE @DocNo OR q.DO_Number LIKE @DocNo) ";
                parameters.Add(new SqlParameter("@DocNo", "%" + docSearch + "%"));
            }

            baseQuery += " ORDER BY CAST(a.Chalan_Date AS DATE) DESC, a.ID DESC;";

            using (SqlCommand cmd = new SqlCommand(baseQuery, DbCL.Conn))
            {
                if (parameters.Count > 0)
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }

                using (SqlDataReader re = cmd.ExecuteReader())
                {
                    if (re.HasRows)
                    {
                        DataList1.DataSource = re;
                        DataList1.DataBind();
                        PanelError.Visible = false;
                    }
                    else
                    {
                        DataList1.DataSource = null;
                        DataList1.DataBind();
                        PanelError.Visible = true;
                        lblErrorMsg.Text = isInitialLoad ? "No recent Challans found." : "No records found matching your search criteria.";
                    }
                }
            }
            DbCL.Conn.Close();
        }

        // --- ROW FORMATTING (UI/UX) ---
        protected void DataList1_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                // Set Serial Number dynamically
                Label lblSlNo = (Label)e.Item.FindControl("lblSlNo");
                if (lblSlNo != null)
                {
                    lblSlNo.Text = (e.Item.ItemIndex + 1).ToString();
                }

                // Apply Color Coding to "Days Left / Timeline"
                Label lblDaysLeft = (Label)e.Item.FindControl("lblDaysLeft");
                object chalanDateObj = DataBinder.Eval(e.Item.DataItem, "Chalan_Date");
                DateTime chalanDate;

                if (lblDaysLeft != null && chalanDateObj != null && DateTime.TryParse(chalanDateObj.ToString(), out chalanDate))
                {
                    int daysLeft = (chalanDate - DateTime.Today).Days;

                    if (daysLeft < 0)
                    {
                        // Past Delivery Date - Red text
                        lblDaysLeft.Text = $"<span class='badge-red'>{Math.Abs(daysLeft)} days ago</span>";
                    }
                    else if (daysLeft == 0)
                    {
                        // Delivering Today - Blue text
                        lblDaysLeft.Text = "<span class='badge-blue'>Today</span>";
                    }
                    else
                    {
                        // Future Delivery Date - Green text
                        lblDaysLeft.Text = $"<span class='badge-green'>in {daysLeft} days</span>";
                    }
                }
            }
        }
    }
}