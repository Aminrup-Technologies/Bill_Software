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
    public partial class WebForm31 : System.Web.UI.Page
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
            //string cmdstring = "select top(50) tbl_Proforma.ID,tbl_Proforma.Invoice_No,tbl_Proforma.Invoice_Date,tbl_Proforma.Quotation_No,tbl_Proforma.Quotation_Date,tbl_Proforma.Net_Amount,tbl_Client.Client_Name from tbl_Proforma inner join tbl_Client on tbl_Proforma.Client_ID=tbl_Client.Client_Id  order by tbl_Proforma.ID desc";
            //string cmdstring = "select top(50) tbl_QuoPriSerTogather.PServiceName,tbl_Proforma.ID,tbl_Proforma.Invoice_No,tbl_Proforma.Invoice_Date,tbl_Proforma.Quotation_No,tbl_Proforma.Quotation_Date,tbl_Proforma.Net_Amount,tbl_Proforma.subtotal,(tbl_Proforma.Net_Amount-tbl_Proforma.subtotal) as Gst, tbl_Proforma.mail_Date, tbl_Client.Client_Name from tbl_Proforma inner join tbl_Client on tbl_Proforma.Client_ID=tbl_Client.Client_Id INNER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no  order by tbl_Proforma.ID desc";
            string cmdstring = "select a.ID,a.Invoice_No,a.Invoice_Date,a.Quotation_No,a.Quotation_Date,a.Net_Amount,a.mail_Date,a.subtotal,(a.Net_Amount-a.subtotal) as Gst,b.Client_Name,c.PServiceName from tbl_Proforma as a left outer join tbl_QuoPriSerTogather as c on a.Quotation_No=c.qutno left outer join tbl_Client as b on b.Client_Id=a.Client_ID order by a.ID desc";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }
    }
}