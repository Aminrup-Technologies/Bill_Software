using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app
{
    public partial class vw_dailyrpts : System.Web.UI.Page
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
                Binddata();

            }
        }

        private void Binddata()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select top(50) * from tbl_SalesVisitReport order by Id desc";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList2.DataSource = cmd.ExecuteReader();
            DataList2.DataBind();
            DbCL.Conn.Close();
        }
    }
}