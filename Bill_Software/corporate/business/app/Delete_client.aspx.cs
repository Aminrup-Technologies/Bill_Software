using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm18 : SecurePage
    {
        protected override string RequiredPermissionKey { get { return "Delete_client"; } }

        private static string ConnString
        {
            get { return ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                Bindcombo();
                BindGrid();
            }
        }

        private void BindGrid()
        {
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(
                "SELECT Client_Id, Client_Name FROM dbo.tbl_Client WHERE CompanyID = @CompanyID ORDER BY Client_Name", cn))
            {
                cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyContext.CurrentCompanyID });
                cn.Open();
                DataList1.DataSource = cmd.ExecuteReader();
                DataList1.DataBind();
            }
        }

        private void BindGrid1()
        {
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(
                "SELECT Client_Id, Client_Name FROM dbo.tbl_Client WHERE CompanyID = @CompanyID AND Client_Name = @ClientName ORDER BY Client_Name", cn))
            {
                cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyContext.CurrentCompanyID });
                cmd.Parameters.Add(new SqlParameter("@ClientName", SqlDbType.NVarChar, 200) { Value = cmbvendor.Text });
                cn.Open();
                DataList1.DataSource = cmd.ExecuteReader();
                DataList1.DataBind();
            }
        }

        private void Bindcombo()
        {
            cmbvendor.Items.Clear();
            cmbvendor.Items.Add("ALL");
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(
                "SELECT Client_Name FROM dbo.tbl_Client WHERE CompanyID = @CompanyID ORDER BY Client_Name", cn))
            {
                cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyContext.CurrentCompanyID });
                cn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                        cmbvendor.Items.Add(rdr[0].ToString());
                }
            }
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            if (e.CommandName != "Delete")
                return;

            string clientId = Convert.ToString(e.CommandArgument);
            if (!AuthGuard.ClientInCurrentCompany(clientId))
                return;

            using (var cn = new SqlConnection(ConnString))
            {
                cn.Open();
                using (var cmd = new SqlCommand(
                    "DELETE FROM dbo.tbl_Client WHERE Client_Id = @ClientId AND CompanyID = @CompanyID", cn))
                {
                    cmd.Parameters.Add(new SqlParameter("@ClientId", SqlDbType.NVarChar, 100) { Value = clientId });
                    cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyContext.CurrentCompanyID });
                    if (cmd.ExecuteNonQuery() <= 0)
                        return;
                }
                using (var cmd = new SqlCommand(
                    "DELETE FROM dbo.tbl_ClientRegAddress WHERE Client_Id = @ClientId AND CompanyID = @CompanyID", cn))
                {
                    cmd.Parameters.Add(new SqlParameter("@ClientId", SqlDbType.NVarChar, 100) { Value = clientId });
                    cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyContext.CurrentCompanyID });
                    cmd.ExecuteNonQuery();
                }
            }

            PanelOK.Visible = true;
            lblOk.Text = "Data Deleted Successfully...";
            DataList1.Visible = false;
        }

        protected void cmbvendor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbvendor.SelectedIndex == 0)
            {
                BindGrid();
            }
            else
            {
                BindGrid1();
            }
            PanelOK.Visible = false;
        }
    }
}
