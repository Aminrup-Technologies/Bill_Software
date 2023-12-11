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
    public partial class WebForm39 : System.Web.UI.Page
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
            //string cmdstring = "select top(50) tbl_Chalan.Chalan_No,tbl_Chalan.Chalan_Date,tbl_Chalan.Quotation_No,tbl_Chalan.Quotation_Date,tbl_Client.Client_Name from tbl_Chalan inner join tbl_Client on tbl_Chalan.Client_ID=tbl_Client.Client_Id  order by tbl_Chalan.ID desc";
            string cmdstring = "select top(50) a.ID,a.Chalan_No,a.Chalan_Date,a.Quotation_No,a.Quotation_Date,a.Client_ID,b.Client_Name,c.PServiceName from tbl_Chalan as a left outer join tbl_QuoPriSerTogather as c on a.Quotation_No=c.qutno left outer join tbl_Client as b on b.Client_Id=a.Client_ID order by a.ID desc";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }
    }
}