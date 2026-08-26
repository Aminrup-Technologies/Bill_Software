using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class Approve_PR : System.Web.UI.Page
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
                SELECT ReqNo, clientName, CreatedBy, CreatedOn, Status, SubmittedOn
                FROM tbl_RequisitionMain
                WHERE CompanyID = @CompanyID
                  AND Status = 'Submitted'";

                if (!string.IsNullOrWhiteSpace(txtDocNo.Text))
                    sql += " AND ReqNo LIKE '%' + @DocNo + '%'";
                if (!string.IsNullOrWhiteSpace(txtFromDate.Text))
                    sql += " AND CAST(ISNULL(SubmittedOn, CreatedOn) AS DATE) >= @FromDate";
                if (!string.IsNullOrWhiteSpace(txtToDate.Text))
                    sql += " AND CAST(ISNULL(SubmittedOn, CreatedOn) AS DATE) <= @ToDate";
                if (!string.IsNullOrWhiteSpace(ddlStatus.SelectedValue))
                    sql += " AND Status = @Status";

                sql += " ORDER BY SubmittedOn DESC";

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
            ddlStatus.SelectedValue = "Submitted";
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

        protected void DataList1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string reqNo = e.CommandArgument.ToString();
            if (!OwnsSubmittedPR(reqNo)) return;

            if (e.CommandName == "View")
            {
                Response.Redirect("View_PR_Details.aspx?reqNo=" + Server.UrlEncode(reqNo) + "&mode=approve");
                return;
            }

            if (e.CommandName == "Approve")
            {
                ApprovePR(reqNo);
            }
        }

        private bool OwnsSubmittedPR(string reqNo)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT 1 FROM tbl_RequisitionMain WHERE ReqNo = @ReqNo AND CompanyID = @CompanyID AND Status = 'Submitted'", con))
            {
                cmd.Parameters.AddWithValue("@ReqNo", reqNo);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                con.Open();
                return cmd.ExecuteScalar() != null;
            }
        }

        private void ApprovePR(string reqNo)
        {
            if (Session["USERID"] == null) { Response.Redirect("~/index.aspx"); return; }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();
                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_Requisition_Approve", con, tran))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ReqNo", reqNo);
                        cmd.Parameters.AddWithValue("@ApproverUserId", Session["USERID"].ToString());
                        cmd.Parameters.AddWithValue("@Action", "Approved");
                        cmd.Parameters.AddWithValue("@Remarks", "");
                        cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                        cmd.ExecuteNonQuery();
                    }

                    InsertSystemNotification("PR Approved",
                        $"PR {reqNo} approved.", "SUCCESS", con, tran);

                    tran.Commit();
                    BindPRList();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        private void InsertSystemNotification(string title, string message, string severity, SqlConnection conn, SqlTransaction trans)
        {
            string sql = @"INSERT INTO tbl_SystemNotification
                           (Title, Message, ModuleCode, Severity, StartDate, EndDate, IsActive, CreatedBy, CompanyID)
                           VALUES
                           (@Title, @Msg, @Mod, @Severity, GETDATE(), DATEADD(day, 30, GETDATE()), 1, @User, @Comp)";
            using (SqlCommand cmd = new SqlCommand(sql, conn, trans))
            {
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Msg", message);
                cmd.Parameters.AddWithValue("@Mod", "PURCHASE");
                cmd.Parameters.AddWithValue("@Severity", severity);
                cmd.Parameters.AddWithValue("@User", Session["USERID"] != null ? Session["USERID"].ToString() : "System");
                cmd.Parameters.AddWithValue("@Comp", CompanyContext.CurrentCompanyID);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
