using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.Services;

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
                    WHERE tbl_Quotation.RecordType != 'Quotation' ");

                List<SqlParameter> sqlParams = new List<SqlParameter>();

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
                Response.Redirect("/corporate/business/print/NewPurchaseOrder.aspx?ID=" + Convert.ToString(e.CommandArgument), false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        // --- WEB METHODS FOR AJAX AUTOCOMPLETE ---

        [WebMethod]
        public static List<string> GetClientNames(string prefix)
        {
            return GetAutocompleteData("SELECT DISTINCT Client_Name FROM tbl_Client WHERE Client_Name LIKE @prefix", prefix);
        }

        [WebMethod]
        public static List<string> GetQuotationNos(string prefix)
        {
            return GetAutocompleteData("SELECT DISTINCT Quotation_no FROM tbl_Quotation WHERE RecordType != 'Quotation' AND Quotation_no LIKE @prefix", prefix);
        }

        [WebMethod]
        public static List<string> GetArcPoDoNos(string prefix)
        {
            // We use UNION to combine unique matches from BOTH the PO_Number and DO_Number columns
            string query = @"
                SELECT DISTINCT PO_Number FROM tbl_Quotation 
                WHERE RecordType != 'Quotation' AND PO_Number LIKE @prefix AND PO_Number IS NOT NULL AND PO_Number != ''
                UNION
                SELECT DISTINCT DO_Number FROM tbl_Quotation 
                WHERE RecordType != 'Quotation' AND DO_Number LIKE @prefix AND DO_Number IS NOT NULL AND DO_Number != ''";

            return GetAutocompleteData(query, prefix);
        }

        // Generic helper method to fetch autocomplete data
        private static List<string> GetAutocompleteData(string query, string prefix)
        {
            List<string> suggestions = new List<string>();
            DB_UTILITY db = new DB_UTILITY();

            try
            {
                db.Sqlconnection();
                db.ConnectDb();
                using (SqlCommand cmd = new SqlCommand(query, db.Conn))
                {
                    cmd.Parameters.AddWithValue("@prefix", "%" + prefix + "%");
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