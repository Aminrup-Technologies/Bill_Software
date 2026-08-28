using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class Generate_PO_From_PR : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
                return;
            }

            if (!IsPostBack)
            {
                BindPRList();
            }
        }

        private void BindPRList()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                string sql = @"
                SELECT R.ReqNo, R.clientName, R.Vendor, R.VendorId, R.NetAmount, R.ApprovedBy, R.ApprovedOn, R.CreatedBy, R.CreatedOn, R.Status
                FROM tbl_RequisitionMain R
                WHERE R.CompanyID = @CompanyID
                  AND R.Status = 'Approved'
                  AND NOT EXISTS (
                      SELECT 1 FROM tbl_PO_Header P
                      WHERE P.ReqNo = R.ReqNo AND P.CompanyID = R.CompanyID
                  )";

                if (!string.IsNullOrWhiteSpace(txtDocNo.Text))
                    sql += " AND R.ReqNo LIKE '%' + @DocNo + '%'";
                if (!string.IsNullOrWhiteSpace(txtFromDate.Text))
                    sql += " AND CAST(ISNULL(R.ApprovedOn, R.CreatedOn) AS DATE) >= @FromDate";
                if (!string.IsNullOrWhiteSpace(txtToDate.Text))
                    sql += " AND CAST(ISNULL(R.ApprovedOn, R.CreatedOn) AS DATE) <= @ToDate";
                if (!string.IsNullOrWhiteSpace(ddlStatus.SelectedValue))
                    sql += " AND R.Status = @Status";

                sql += " ORDER BY R.ApprovedOn DESC";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    if (!string.IsNullOrWhiteSpace(txtDocNo.Text))
                        cmd.Parameters.AddWithValue("@DocNo", txtDocNo.Text.Trim());
                    if (!string.IsNullOrWhiteSpace(txtFromDate.Text))
                        cmd.Parameters.AddWithValue("@FromDate", DateTime.Parse(txtFromDate.Text.Trim()));
                    if (!string.IsNullOrWhiteSpace(txtToDate.Text))
                        cmd.Parameters.AddWithValue("@ToDate", DateTime.Parse(txtToDate.Text.Trim()));
                    if (!string.IsNullOrWhiteSpace(ddlStatus.SelectedValue))
                        cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        DataList1.DataSource = dt;
                        DataList1.DataBind();
                    }
                }
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            DataList1.PageIndex = 0;
            BindPRList();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtDocNo.Text = "";
            txtFromDate.Text = "";
            txtToDate.Text = "";
            ddlStatus.SelectedValue = "Approved";
            DataList1.PageIndex = 0;
            BindPRList();
        }

        protected void DataList1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            DataList1.PageIndex = e.NewPageIndex;
            BindPRList();
        }

        protected void DataList1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            Label lblSl = (Label)e.Row.FindControl("lblSlNo");
            lblSl.Text = ((DataList1.PageIndex * DataList1.PageSize) + e.Row.RowIndex + 1).ToString();
        }

        protected void DataList1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Preview") return;

            string reqNo = e.CommandArgument.ToString();
            if (Session["USERID"] == null) { Response.Redirect("~/index.aspx"); return; }
            if (!OwnsApprovedPR(reqNo)) return;
            Response.Redirect("Generate_PO_Preview.aspx?reqNo=" + Server.UrlEncode(reqNo));
        }

        private bool OwnsApprovedPR(string reqNo)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT 1 FROM tbl_RequisitionMain R WHERE R.ReqNo = @ReqNo AND R.CompanyID = @CompanyID AND R.Status = 'Approved' AND NOT EXISTS (SELECT 1 FROM tbl_PO_Header P WHERE P.ReqNo = R.ReqNo AND P.CompanyID = R.CompanyID)", con))
            {
                cmd.Parameters.AddWithValue("@ReqNo", reqNo);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                con.Open();
                return cmd.ExecuteScalar() != null;
            }
        }
    }
}
