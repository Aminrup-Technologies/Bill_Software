using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
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

        private void BindPRList()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(@"SELECT * FROM tbl_RequisitionMain ORDER BY CreatedOn DESC", con);

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