using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class View_PO : System.Web.UI.Page
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
                BindPOList();
            }
        }

        private void BindPOList()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                string sql = @"
            SELECT
                H.PO_Id,
                H.PO_No,
                H.ReqNo,
                H.PO_Status,
                H.CreatedOn,
                V.Vendor_Name,
                U.Name AS CreatedByName
            FROM tbl_PO_Header H
            LEFT JOIN tbl_Vendor V ON V.Id = H.VendorId
            LEFT JOIN tbl_login U ON U.User_Id = H.CreatedBy
            WHERE H.CompanyID = @CompanyID";

                if (!string.IsNullOrWhiteSpace(txtDocNo.Text))
                    sql += " AND (H.PO_No LIKE '%' + @DocNo + '%' OR H.ReqNo LIKE '%' + @DocNo + '%')";
                if (!string.IsNullOrWhiteSpace(txtFromDate.Text))
                    sql += " AND CAST(H.CreatedOn AS DATE) >= @FromDate";
                if (!string.IsNullOrWhiteSpace(txtToDate.Text))
                    sql += " AND CAST(H.CreatedOn AS DATE) <= @ToDate";
                if (!string.IsNullOrWhiteSpace(ddlStatus.SelectedValue))
                    sql += " AND H.PO_Status = @Status";

                sql += " ORDER BY H.CreatedOn DESC";

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
                        DataListPO.DataSource = dt;
                        DataListPO.DataBind();
                    }
                }
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            DataListPO.PageIndex = 0;
            BindPOList();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtDocNo.Text = "";
            txtFromDate.Text = "";
            txtToDate.Text = "";
            ddlStatus.SelectedIndex = 0;
            DataListPO.PageIndex = 0;
            BindPOList();
        }

        protected void DataListPO_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            DataListPO.PageIndex = e.NewPageIndex;
            BindPOList();
        }

        protected void DataListPO_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "View") return;

            int poId;
            if (!int.TryParse(e.CommandArgument.ToString(), out poId)) return;
            if (!OwnsPO(poId)) return;
            Response.Redirect("View_PO_Details.aspx?poId=" + poId);
        }

        protected void DataListPO_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            Label lblSl = (Label)e.Row.FindControl("lblSlNo");
            lblSl.Text = ((DataListPO.PageIndex * DataListPO.PageSize) + e.Row.RowIndex + 1).ToString();

            Label lblStatus = (Label)e.Row.FindControl("lblStatus");
            switch (lblStatus.Text)
            {
                case "Draft": lblStatus.ForeColor = System.Drawing.Color.DarkOrange; break;
                case "Released": lblStatus.ForeColor = System.Drawing.Color.Green; break;
                case "Cancelled": lblStatus.ForeColor = System.Drawing.Color.Red; break;
            }
        }

        private bool OwnsPO(int poId)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT 1 FROM tbl_PO_Header WHERE PO_Id = @PO_Id AND CompanyID = @CompanyID", con))
            {
                cmd.Parameters.AddWithValue("@PO_Id", poId);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                con.Open();
                return cmd.ExecuteScalar() != null;
            }
        }
    }
}
