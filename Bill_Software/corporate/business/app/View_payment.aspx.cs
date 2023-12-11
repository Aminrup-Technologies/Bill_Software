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
    public partial class WebForm35 : System.Web.UI.Page
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
            //string cmdstring = "select  top(50) tbl_Client.Client_Name,tbl_invoice_payment.Payment_ID,tbl_invoice_payment.Payment_Date,tbl_invoice_payment.Invoice_No,tbl_invoice_payment.Quotation_No,tbl_invoice_payment.Net_amount,tbl_invoice_payment.Given_amount,tbl_invoice_payment.type,tbl_invoice_payment.Ch_no,tbl_invoice_payment.Ch_date from tbl_invoice_payment inner join tbl_Client on tbl_invoice_payment.Client_Id=tbl_Client.Client_Id order by tbl_invoice_payment.ID desc";
            string cmdstring = "select a.ID,a.Payment_ID,a.Payment_Date,a.Invoice_No,a.Invoice_Date,a.Quotation_No,a.Quotation_Date,a.Net_amount,a.mailDate,a.subtotal,a.Due_amount,a.Given_amount,a.type,a.Ch_no,a.Ch_date,a.tds,(a.Net_amount - a.subtotal) as Gst,b.Client_Name,c.PServiceName from tbl_invoice_payment as a left outer join tbl_QuoPriSerTogather as c on a.Quotation_No=c.qutno left outer join tbl_Client as b on b.Client_Id=a.Client_ID order by a.ID desc";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }
    }
}