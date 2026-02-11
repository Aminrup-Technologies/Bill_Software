using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class View_PO : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindPOList();
            }
        }

        //private void BindPOList()
        //{
        //    using (SqlConnection con = new SqlConnection(
        //        ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
        //    {
        //        SqlDataAdapter da = new SqlDataAdapter(
        //            "SELECT * FROM tbl_PO_Header ORDER BY CreatedOn DESC", con);

        //        DataTable dt = new DataTable();
        //        da.Fill(dt);

        //        DataListPO.DataSource = dt;
        //        DataListPO.DataBind();
        //    }
        //}

        private void BindPOList()
        {
            using (SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(@"
            SELECT
                H.PO_Id,
                H.PO_No,
                H.ReqNo,
                H.PO_Status,
                H.CreatedOn,
                V.Vendor_Name,
                U.Name AS CreatedByName
            FROM tbl_PO_Header H
            LEFT JOIN tbl_Vendor V
                ON V.Id = H.VendorId
            LEFT JOIN tbl_login U
                ON U.User_Id = H.CreatedBy
            ORDER BY H.CreatedOn DESC
        ", con);

                DataTable dt = new DataTable();
                da.Fill(dt);

                DataListPO.DataSource = dt;
                DataListPO.DataBind();
            }
        }



        protected void DataListPO_ItemCommand(object source, DataListCommandEventArgs e)
        {
            if (e.CommandName == "View")
            {
                Response.Redirect("View_PO_Details.aspx?poId=" + e.CommandArgument);
            }
        }

        protected void DataListPO_ItemDataBound(object sender, DataListItemEventArgs e)
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

                    case "Released":
                        lblStatus.ForeColor = System.Drawing.Color.Green;
                        break;

                    case "Cancelled":
                        lblStatus.ForeColor = System.Drawing.Color.Red;
                        break;
                }
            }
        }
    }
}