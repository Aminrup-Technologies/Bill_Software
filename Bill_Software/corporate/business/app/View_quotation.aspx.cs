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
                //if (Session["SelectedRecordType"] == null)
                //{
                //    // Fresh load — default to Quotation
                //    rbQt.Checked = true;
                //    rbPo.Checked = false;
                //    Session["SelectedRecordType"] = "Quotation";  // Set default in session
                //}
                //else
                //{
                //    // Coming back from another page
                //    string selectedType = Convert.ToString(Session["SelectedRecordType"]);
                //    if (selectedType == "PO")
                //    {
                //        rbPo.Checked = true;
                //        rbQt.Checked = false;
                //    }
                //    else
                //    {
                //        rbQt.Checked = true;
                //        rbPo.Checked = false;
                //    }
                //}

                //rbQt.Checked = true;
                string cmdstring = "select top(50) tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.cgstOrsgst,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.RecordType='Quotation' order by tbl_Quotation.ID desc";
                Binddata(cmdstring);

                //Binder();
            }
        }

        //protected void RecordTypeChanged(object sender, EventArgs e)
        //{
        //    Binder();
        //}

        //private void Binder()
        //{
        //    if (rbPo.Checked)
        //    {
        //        string cmdstring = "select top(50) tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.cgstOrsgst,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.RecordType!='Quotation' order by tbl_Quotation.ID desc";
        //        Binddata(cmdstring);
        //    }
        //    else if (rbQt.Checked == true)
        //    {
        //        string cmdstring = "select top(50) tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.cgstOrsgst,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.RecordType='Quotation' order by tbl_Quotation.ID desc";
        //        Binddata(cmdstring);
        //    }
        //}

        private void Binddata(string query)
        {
            // Clear existing data
            DataList1.DataSource = null;
            DataList1.DataBind();


            DbCL.Sqlconnection();
            DbCL.ConnectDb();            
            SqlCommand cmd = new SqlCommand(query, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            if (e.CommandName == "View")
            {
                string ID = Convert.ToString(e.CommandArgument);
                Response.Redirect("/corporate/business/print/NewQuotation.aspx?ID=" + ID);
            }

            //string qdate = buindalldata(ID);
            //DateTime fromdate = DateTime.Parse(Convert.ToDateTime(qdate).ToShortDateString());
            //DateTime todate = DateTime.Parse(Convert.ToDateTime("12-Jun-2018").ToShortDateString());
            //if (e.CommandName == "View")
            //{
            //    if (fromdate > todate)
            //    {
            //        Response.Redirect("/corporate/business/print/NewQuotation.aspx?ID=" + ID);
            //    }
            //    else
            //    {
            //        Response.Redirect("/corporate/business/print/Quotation.aspx?ID=" + ID);
            //    }
            //    //string url = "/corporate/business/print/NewQuotation.aspx?ID=" + ID;
            //    //Response.Write("<script type='text/javascript'>window.open('" + url + "');</script>");
            //}

            //string url = "/corporate/business/print/Quotation.aspx?ID=" + ID;
            //string script = $"<script type='text/javascript'>window.location.href='{url}';</script>";
            //ClientScript.RegisterStartupScript(this.GetType(), "RedirectScript", script);
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