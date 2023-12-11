using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Globalization;
using System.Threading;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm23 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dtmain = new DataTable();
        CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
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
            //string PrimaryService = "";
            //string service = null;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            //string cmdstring = "select top(50) tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id  order by tbl_Quotation.ID desc";
            string cmdstring = "select top(50) tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no order by tbl_Quotation.ID desc";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            //SqlDataReader re = cmd.ExecuteReader();
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            //while (re.Read())
            //{
            //    PrimaryService = re["PServiceName"].ToString();
            //}

                DbCL.Conn.Close();
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string ID = Convert.ToString(e.CommandArgument);
            string qdate = buindalldata(ID);

            DateTime fromdate = DateTime.Parse(Convert.ToDateTime(qdate).ToShortDateString());
            DateTime todate = DateTime.Parse(Convert.ToDateTime("12-Jun-2018").ToShortDateString());
            if (e.CommandName == "View")
            {
                if (fromdate > todate)
                {
                    Response.Redirect("/corporate/business/print/NewQuotation.aspx?ID=" + ID);
                }
                else
                {
                    Response.Redirect("/corporate/business/print/Quotation.aspx?ID=" + ID);
                }
                //string url = "/corporate/business/print/NewQuotation.aspx?ID=" + ID;
                //Response.Write("<script type='text/javascript'>window.open('" + url + "');</script>");
            }


        }

        private string buindalldata(string ID)
        {
            string qdate = "";
            string query = "select Quotation_no,Quotation_date,Client_Id,sub_total,Service_tax,Net_amount,cgstOrsgst,igst from tbl_Quotation where ID=@ID";
            SqlParameter[] pram = {
            new SqlParameter("@id",ID)
            };
            dtmain = DbCL.SPreturn_dt(query, pram);
            if (dtmain.Rows.Count > 0)
            {
                string qutno = dtmain.Rows[0]["Quotation_no"].ToString();

                qdate = dtmain.Rows[0]["Quotation_date"].ToString();

            }
            return qdate;
        }

    }
}