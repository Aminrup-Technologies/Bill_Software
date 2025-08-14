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
    public partial class WebForm27 : System.Web.UI.Page
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

            //string cmdstring = "select top(50) tbl_Invoice.ID,tbl_Invoice.Invoice_No,tbl_Invoice.Invoice_Date,tbl_Invoice.Quotation_No,tbl_Invoice.Quotation_Date,tbl_Invoice.Net_Amount,tbl_Client.Client_Name,tbl_Invoice.status2 from tbl_Invoice inner join tbl_Client on tbl_Invoice.Client_ID=tbl_Client.Client_Id  order by tbl_Invoice.ID desc";

            //string cmdstring = "select a.ID,a.Invoice_No,a.Invoice_Date,a.Quotation_No,a.Quotation_Date,a.Net_Amount,a.mailDate,a.sub_total,(a.Net_Amount-a.sub_total) as Gst,b.Client_Name,c.PServiceName from tbl_Invoice as a left outer join tbl_QuoPriSerTogather as c on a.Quotation_No=c.qutno left outer join tbl_Client as b on b.Client_Id=a.Client_ID order by a.ID desc";

            //string cmdstring = "select a.ID,a.Invoice_No,a.Invoice_Date,a.Quotation_No,a.Quotation_Date,a.Net_Amount, Service_Tax1 as Gst,a.mailDate, (a.Net_Amount-Service_Tax1) as sub_total, b.Client_Name,c.PServiceName from tbl_Invoice as a left outer join tbl_QuoPriSerTogather as c on a.Quotation_No=c.qutno left outer join tbl_Client as b on b.Client_Id=a.Client_ID order by a.ID desc";

            string cmdstring = "SELECT a.ID, a.Invoice_No, a.Invoice_Date, a.Quotation_No, a.Quotation_Date, TRY_CAST(a.Net_Amount AS FLOAT) AS Net_Amount, TRY_CAST(Service_Tax1 AS FLOAT) AS Gst, a.mailDate, TRY_CAST(a.Net_Amount AS FLOAT) - TRY_CAST(Service_Tax1 AS FLOAT) AS sub_total, b.Client_Name, c.PServiceName FROM tbl_Invoice AS a LEFT OUTER JOIN tbl_QuoPriSerTogather AS c ON a.Quotation_No = c.qutno LEFT OUTER JOIN tbl_Client AS b ON b.Client_Id = a.Client_ID ORDER BY CAST(a.Invoice_Date as Date) DESC;";

            string cmdstringnew = "SELECT a.ID, a.Invoice_No, a.Invoice_Date, a.Quotation_No, a.Quotation_Date, TRY_CAST(a.Net_Amount AS FLOAT) AS Net_Amount, TRY_CAST(a.Service_Tax1 AS FLOAT) AS Gst, a.mailDate, TRY_CAST(a.Net_Amount AS FLOAT) - TRY_CAST(a.Service_Tax1 AS FLOAT) AS sub_total, b.Client_Name, c.PServiceName, q.PO_Number, q.DO_Number, q.Validity_StartDate, q.Validity_EndDate, q.AddedById, l.Name AS AddedByName, a.TimeStamp FROM tbl_Invoice AS a LEFT JOIN tbl_Client AS b ON b.Client_Id = a.Client_ID LEFT JOIN tbl_QuoPriSerTogather AS c ON a.Quotation_No = c.qutno LEFT JOIN tbl_Quotation AS q ON q.Quotation_no = a.Quotation_No LEFT JOIN tbl_login AS l ON l.User_Id = q.AddedById ORDER BY CAST(a.Invoice_Date AS DATE) DESC;";
            SqlCommand cmd = new SqlCommand(cmdstringnew, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        protected void DataList1_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Label lblSlNo = (Label)e.Item.FindControl("lblSlNo");
                if (lblSlNo != null)
                {
                    lblSlNo.Text = (e.Item.ItemIndex + 1).ToString();
                }
            }
        }
    }
}