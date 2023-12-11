using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm78 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
               // DbCL.FillCombo(ddlPrimaryService, "select PrimaryService from tbl_PrimaryService order by id");
                DbCL.FillCombo(ddlPrimaryService, "select ProductOrServiceCat from tbl_NewparentProduct order by id");
                Binddata();
            }
        }

        private void Binddata()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select id,PrimaryService,PrimaryServiceTerms from tbl_PrimaryServiceTerms order by id";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string query = "insert into tbl_PrimaryServiceTerms (PrimaryService,PrimaryServiceTerms) values (@PrimaryService,@PrimaryServiceTerms)";
            SqlParameter[] pram = {
                new SqlParameter("@PrimaryService",ddlPrimaryService.Text),
                new SqlParameter("@PrimaryServiceTerms",txtPrimaryServiceTerms.Text),
            };
            int jh=DbCL.SPExecDB(query, pram);
            if (jh>0)
            {
                PanelOK.Visible = true;
                lblOk.Text = "Data Save Successfully...";
                txtPrimaryServiceTerms.Text = "";
            }
            Binddata();
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string ID = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Delete")
            {
                DbCL.executeRdr("delete from tbl_PrimaryServiceTerms where id='" + ID + "'");
                PanelOK.Visible = true;
                lblOk.Text = "Data Deleted Successfully...";
            }
            Binddata();
        }
    }
}