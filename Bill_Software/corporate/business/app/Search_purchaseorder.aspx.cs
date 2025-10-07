using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace Bill_Software.corporate.business.app
{
    public partial class Search_purchaseorder : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dtmain = new DataTable();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                DbCL.FillCombo(cmbvendor, "select Client_Name from tbl_Client order by Client_Name");
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");

            }

        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                BuindCompanyId();
                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Client_Id='" + lblclientId.Text + "' order by cast(tbl_Quotation.Quotation_date as datetime) desc";
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.Client_Id='" + lblclientId.Text + "' and tbl_Quotation.RecordType='Purchase Order' order by tbl_Quotation.ID desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(tbl_Quotation.Quotation_date as datetime) desc";
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' and tbl_Quotation.RecordType='Purchase Order' order by tbl_Quotation.ID desc";
                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Client_Id='" + lblclientId.Text + "' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(tbl_Quotation.Quotation_date as datetime) desc";
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.Client_Id='" + lblclientId.Text + "' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' and tbl_Quotation.RecordType='Purchase Order' order by tbl_Quotation.ID desc";
                Buinddatagrid(cmdstring);
            }
            btnSertch.Visible = false;

        }

        private void Buinddatagrid(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                Buinddatagrid1(cmdstring);
            }
            else
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "No Data Found...";

            }
            DbCL.Conn.Close();
        }

        private void Buinddatagrid1(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            SqlCommand cmd1 = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd1.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();

        }

        private void BuindCompanyId()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Id from tbl_Client where Client_Name='" + cmbvendor.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblclientId.Text = re["Client_Id"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/Search_purchaseorder.aspx");

        }

        protected void DataList1_ItemCommand_OLD(object source, DataListCommandEventArgs e)
        {
            string ID = Convert.ToString(e.CommandArgument);
            string qdate = buindalldata(ID);

            DateTime fromdate = DateTime.Parse(Convert.ToDateTime(qdate).ToShortDateString());
            DateTime todate = DateTime.Parse(Convert.ToDateTime("12-Jun-2018").ToShortDateString());
            if (e.CommandName == "View")
            {
                //if (fromdate > todate)
                //{
                //    Response.Redirect("/corporate/business/print/NewPurchaseOrder.aspx?ID=" + ID);
                //}
                //else
                //{
                //    Response.Redirect("/corporate/business/print/NewPurchaseOrder.aspx?ID=" + ID);
                //}
                string url = "/corporate/business/print/NewPurchaseOrder.aspx?ID=" + ID;
                Response.Write("<script type='text/javascript'>window.open('" + url + "');</script>");
            }
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            // Only handle the intended command
            if (!string.Equals(e.CommandName, "View", StringComparison.OrdinalIgnoreCase))
                return;

            // Validate/normalize ID
            var id = Convert.ToString(e.CommandArgument)?.Trim();
            if (string.IsNullOrEmpty(id))
            {
                // Optionally show a message to the user here
                return;
            }

            // Safely get and parse the document date returned by buindalldata(id)
            // Adjust culture/format to whatever your data actually returns.
            DateTime fromDate;
            var rawDate = buindalldata(id);
            if (!DateTime.TryParse(rawDate, CultureInfo.GetCultureInfo("en-IN"),
                                   DateTimeStyles.AssumeLocal, out fromDate))
            {
                // Fallback/handling if parse fails; choose what makes sense for you
                fromDate = DateTime.MinValue;
            }

            // Fixed, culture-invariant comparison date (12-Jun-2018)
            DateTime cutoverDate;
            if (!DateTime.TryParseExact("12-Jun-2018", "dd-MMM-yyyy",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out cutoverDate))
            {
                cutoverDate = new DateTime(2018, 6, 12);
            }

            // If you need branching by date, do it here:
            // string page = (fromDate > cutoverDate)
            //     ? "~/corporate/business/print/NewPurchaseOrder.aspx"
            //     : "~/corporate/business/print/OldPurchaseOrder.aspx";

            string page = "~/corporate/business/print/NewPurchaseOrder.aspx";

            // Build a safe, app-root-relative URL
            var url = ResolveClientUrl(page) + "?ID=" + Server.UrlEncode(id);

            // Open in a new tab/window via registered script (works with/without UpdatePanel)
            string script = $"window.open('{url}', '_blank');";
            if (ScriptManager.GetCurrent(Page) != null)
                ScriptManager.RegisterStartupScript(Page, GetType(), "openPO_" + id, script, true);
            else
                ClientScript.RegisterStartupScript(GetType(), "openPO_" + id, script, true);

            // If you prefer a same-tab redirect instead, use:
            // Response.Redirect(url, false);
            // Context.ApplicationInstance.CompleteRequest();
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