using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class Generate_PO_From_PR : System.Web.UI.Page
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
                SqlDataAdapter da = new SqlDataAdapter(@"
                SELECT ReqNo, clientName, Vendor, VendorId, NetAmount, ApprovedBy, ApprovedOn, CreatedBy, CreatedOn
                FROM tbl_RequisitionMain
                WHERE Status = 'Approved'
                ORDER BY ApprovedOn DESC", con);

                DataTable dt = new DataTable();
                da.Fill(dt);
                DataList1.DataSource = dt;
                DataList1.DataBind();
            }
        }

        protected void DataList1_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Label lblSl = (Label)e.Item.FindControl("lblSlNo");
                lblSl.Text = (e.Item.ItemIndex + 1).ToString();
            }
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            if (e.CommandName == "Preview")
            {
                string reqNo = e.CommandArgument.ToString();
                // Redirect to the new dedicated page, passing the PR number
                Response.Redirect($"Generate_PO_Preview.aspx?reqNo={reqNo}");
            }
        }
    }
}