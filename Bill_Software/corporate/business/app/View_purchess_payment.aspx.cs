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
    public partial class WebForm47 : System.Web.UI.Page
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
            string cmdstring = "select  top(50) tbl_Vendor.Vendor_Name,tbl_Purchess_payment.Payment_ID,tbl_Purchess_payment.Payment_Date,tbl_Purchess_payment.Purchess_ID,tbl_Purchess_payment.Purchess_Date,tbl_Purchess_payment.Net_amount,tbl_Purchess_payment.Given_amount,tbl_Purchess_payment.type,tbl_Purchess_payment.Ch_no,tbl_Purchess_payment.Ch_date from tbl_Purchess_payment inner join tbl_Vendor on tbl_Purchess_payment.Client_Id=tbl_Vendor.Vendor_Id order by tbl_Purchess_payment.ID desc";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }
    }
}