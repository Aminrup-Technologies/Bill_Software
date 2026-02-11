using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class View_PR : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindPRList();
            }

        }

        //private void BindPRList()
        //{
        //    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
        //    {
        //        SqlDataAdapter da = new SqlDataAdapter(@"SELECT * FROM tbl_RequisitionMain ORDER BY CreatedOn DESC", con);

        //        DataTable dt = new DataTable();
        //        da.Fill(dt);

        //        DataList1.DataSource = dt;
        //        DataList1.DataBind();
        //    }
        //}

        private void BindPRList()
        {
            using (SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
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
                ORDER BY RM.CreatedOn DESC";

                SqlDataAdapter da = new SqlDataAdapter(sql, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                DataList1.DataSource = dt;
                DataList1.DataBind();
            }
        }


        protected void DataList1_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item ||
                e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Label lblSl = (Label)e.Item.FindControl("lblSlNo");
                lblSl.Text = (e.Item.ItemIndex + 1).ToString();

                Label lblStatus = (Label)e.Item.FindControl("lblStatus");

                switch (lblStatus.Text)
                {
                    case "Draft":
                        lblStatus.ForeColor = System.Drawing.Color.DarkOrange;
                        break;
                    case "Submitted":
                        lblStatus.ForeColor = System.Drawing.Color.Blue;
                        break;
                    case "Approved":
                        lblStatus.ForeColor = System.Drawing.Color.Green;
                        break;
                    case "Cancelled":
                        lblStatus.ForeColor = System.Drawing.Color.Red;
                        break;
                }
            }
        }


        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            if (e.CommandName == "View")
            {
                string reqNo = e.CommandArgument.ToString();
                Response.Redirect("View_PR_Details.aspx?reqNo=" + reqNo);
            }
        }



    }
}