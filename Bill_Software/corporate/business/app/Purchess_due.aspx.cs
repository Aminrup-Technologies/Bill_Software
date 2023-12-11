using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm53 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        int countre = 1;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                Bindgridi1();
                
                if (countre == 1)
                {
                    PanelOK.Visible = true;
                    lblOk.Text = "No Payments Is Due...";
                }
                else
                {
                    PanelOK.Visible = false;
                }

            }
        }

        private void Bindgridi1()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string CmdString = "SELECT tbl_Purches.Purches_Id,tbl_Purches.Total_purches_rate,tbl_Purches.Purches_date,tbl_Vendor.Vendor_Name,tbl_purches_due.Due_amount,tbl_Purches.Purches_Id as Purches_Id FROM tbl_Purches INNER JOIN tbl_purches_due ON tbl_Purches.Purches_Id =tbl_purches_due.Purches_Id INNER JOIN tbl_Vendor ON tbl_Purches.Client_Id =tbl_Vendor.Vendor_Id";
            SqlCommand com1 = new SqlCommand(CmdString, DbCL.Conn);
            SqlDataAdapter da = new SqlDataAdapter(com1);
            SqlDataReader dr = com1.ExecuteReader();

            if (dr.Read())
            {

                DataList1.DataSource = DbCL.GetDataTable(CmdString);
                DataList1.DataBind();
                first_div.Visible = true;
                countre = countre + 1;

            }
            else
            {
                first_div.Visible = false;
            }
            DbCL.Conn.Close();
        }
    }
}