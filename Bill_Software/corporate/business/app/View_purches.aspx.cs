using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Text;
using System.Web.Services;
using System.Web.Script.Services;
using System.Collections.Generic;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm20 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();

        protected void Page_Load(object sender, EventArgs e)
        {
            // Security: Stop Ghost Execution
            if (Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                // Default filter: Load last 30 days for performance
                txtFromDate.Text = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
                txtToDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                BindData();
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e) { BindData(); }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            txtFromDate.Text = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
            txtToDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            BindData();
        }

        private void BindData()
        {
            try
            {
                pnlMessage.Visible = false; // Resolved CS0103
                StringBuilder query = new StringBuilder();

                query.Append(@"SELECT p.Purches_Id, 
                       CONVERT(VARCHAR, p.TimeStamp, 106) as DisplayDate, 
                       ISNULL(v.Vendor_Name, 'Unknown Vendor') as Vendor_Name, 
                       p.Invoice_No, p.BuyerOrderNo, p.Total_Tax_rate, p.Total_purches_rate 
                       FROM tbl_Purches p 
                       INNER JOIN tbl_Vendor v ON p.Client_Id = v.Vendor_Id 
                       WHERE 1=1 ");

                SqlCommand cmd = new SqlCommand();

                // Keyword Search (Resolved CS0103 for txtSearch)
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    query.Append(" AND (p.Invoice_No LIKE @key OR p.BuyerOrderNo LIKE @key OR p.Purches_Id LIKE @key) ");
                    cmd.Parameters.AddWithValue("@key", "%" + txtSearch.Text.Trim() + "%");
                }

                // Vendor Search
                if (!string.IsNullOrWhiteSpace(txtVendorSearch.Text))
                {
                    query.Append(" AND v.Vendor_Name LIKE @vname ");
                    cmd.Parameters.AddWithValue("@vname", "%" + txtVendorSearch.Text.Trim() + "%");
                }

                // Date Range
                if (!string.IsNullOrWhiteSpace(txtFromDate.Text) && !string.IsNullOrWhiteSpace(txtToDate.Text))
                {
                    query.Append(" AND CAST(p.TimeStamp AS DATE) BETWEEN @from AND @to ");
                    cmd.Parameters.AddWithValue("@from", txtFromDate.Text);
                    cmd.Parameters.AddWithValue("@to", txtToDate.Text);
                }

                query.Append(" ORDER BY p.Purches_Id DESC");

                DbCL.Sqlconnection();
                DbCL.ConnectDb();
                cmd.Connection = DbCL.Conn;
                cmd.CommandText = query.ToString();

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptPurchase.DataSource = dt;
                rptPurchase.DataBind();
            }
            catch (Exception ex)
            {
                lblUserMessage.Text = "An error occurred during data retrieval."; // Resolved CS0103
                pnlMessage.Visible = true;
                // LogError(ex);
            }
            finally
            {
                DbCL.Conn.Close();
            }
        }

        // Backward compatibility helper for ASPX
        protected string ISNULL_Text(object val, string fallback)
        {
            string s = Convert.ToString(val);
            return string.IsNullOrWhiteSpace(s) ? fallback : s;
        }

        private void LogError(Exception ex)
        {
            // Implementation for local file logging or database logging
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static List<string> GetSearchSuggestions(string prefix)
        {
            List<string> suggestions = new List<string>();

            // Use your connection string from web.config
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ToString();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // This query fetches distinct matches from IDs, Invoices, and Order numbers
                string sql = @"SELECT val FROM (
                        SELECT CAST(Purches_Id AS VARCHAR) as val FROM tbl_Purches WHERE CAST(Purches_Id AS VARCHAR) LIKE @prefix + '%'
                        UNION
                        SELECT Invoice_No FROM tbl_Purches WHERE Invoice_No LIKE @prefix + '%'
                        UNION
                        SELECT BuyerOrderNo FROM tbl_Purches WHERE BuyerOrderNo LIKE @prefix + '%'
                      ) t ORDER BY val";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@prefix", prefix);
                    conn.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            suggestions.Add(sdr["val"].ToString());
                        }
                    }
                }
            }
            return suggestions;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static List<string> GetVendorSuggestions(string prefix)
        {
            List<string> suggestions = new List<string>();
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ToString();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // Query specifically for unique vendor names
                string sql = "SELECT DISTINCT Vendor_Name FROM tbl_Vendor WHERE Vendor_Name LIKE @prefix + '%'";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@prefix", prefix);
                    conn.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            suggestions.Add(sdr["Vendor_Name"].ToString());
                        }
                    }
                }
            }
            return suggestions;
        }
    }
}