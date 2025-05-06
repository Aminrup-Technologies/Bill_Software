using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm20 : System.Web.UI.Page
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
            string cmdstring = "select top(50) tbl_Purches.Purches_Id, tbl_Purches.TimeStamp, tbl_Purches.Purches_date,tbl_Purches.Total_purches_rate,tbl_Purches.Total_Tax_rate, tbl_Purches.Invoice_No, tbl_Purches.Purches_date, tbl_Purches.BuyerOrderNo, tbl_Purches.OrderDate, tbl_Purches.ShippedToStoreName, tbl_Vendor.Vendor_Name from tbl_Purches inner join tbl_Vendor on tbl_Purches.Client_Id=tbl_Vendor.Vendor_Id order by tbl_Purches.Purches_date DESC";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }
    }
}