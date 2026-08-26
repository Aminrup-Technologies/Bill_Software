using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class View_PR : System.Web.UI.Page
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
                SELECT
                    RM.ReqNo,
                    RM.clientName,
                    RM.Status,
                    RM.CreatedBy        AS CreatedById,
                    U1.Name             AS CreatedByName,
                    RM.CreatedOn,
                    RM.SubmittedBy      AS SubmittedById,
                    U2.Name             AS SubmittedByName,
                    RM.SubmittedOn,
                    RM.ApprovedBy       AS ApprovedById,
                    U3.Name             AS ApprovedByName,
                    RM.ApprovedOn
                FROM tbl_RequisitionMain RM
                LEFT JOIN tbl_login U1 ON U1.User_Id = RM.CreatedBy
                LEFT JOIN tbl_login U2 ON U2.User_Id = RM.SubmittedBy
                LEFT JOIN tbl_login U3 ON U3.User_Id = RM.ApprovedBy
                WHERE RM.CompanyID = @CompanyID";

                if (!string.IsNullOrWhiteSpace(txtDocNo.Text))
                    sql += " AND RM.ReqNo LIKE '%' + @DocNo + '%'";
                if (!string.IsNullOrWhiteSpace(txtFromDate.Text))
                    sql += " AND CAST(RM.CreatedOn AS DATE) >= @FromDate";
                if (!string.IsNullOrWhiteSpace(txtToDate.Text))
                    sql += " AND CAST(RM.CreatedOn AS DATE) <= @ToDate";
                if (!string.IsNullOrWhiteSpace(ddlStatus.SelectedValue))
                    sql += " AND RM.Status = @Status";

                sql += " ORDER BY RM.CreatedOn DESC";

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
                        gvPR.DataSource = dt;
                        gvPR.DataBind();
                    }
                }
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            gvPR.PageIndex = 0;
            BindPRList();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtDocNo.Text = "";
            txtFromDate.Text = "";
            txtToDate.Text = "";
            ddlStatus.SelectedIndex = 0;
            gvPR.PageIndex = 0;
            BindPRList();
        }

        protected void gvPR_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvPR.PageIndex = e.NewPageIndex;
            BindPRList();
        }

        protected void gvPR_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            Label lblSl = (Label)e.Row.FindControl("lblSlNo");
            lblSl.Text = ((gvPR.PageIndex * gvPR.PageSize) + e.Row.RowIndex + 1).ToString();

            Label lblStatus = (Label)e.Row.FindControl("lblStatus");
            switch (lblStatus.Text)
            {
                case "Draft": lblStatus.ForeColor = System.Drawing.Color.DarkOrange; break;
                case "Submitted": lblStatus.ForeColor = System.Drawing.Color.Blue; break;
                case "Approved": lblStatus.ForeColor = System.Drawing.Color.Green; break;
                case "Cancelled":
                case "Rejected": lblStatus.ForeColor = System.Drawing.Color.Red; break;
            }
        }

        protected void gvPR_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "View") return;

            string reqNo = e.CommandArgument.ToString();
            if (!OwnsPR(reqNo)) return;
            Response.Redirect("View_PR_Details.aspx?reqNo=" + Server.UrlEncode(reqNo));
        }

        private bool OwnsPR(string reqNo)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT 1 FROM tbl_RequisitionMain WHERE ReqNo = @ReqNo AND CompanyID = @CompanyID", con))
            {
                cmd.Parameters.AddWithValue("@ReqNo", reqNo);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                con.Open();
                return cmd.ExecuteScalar() != null;
            }
        }
    }
}
