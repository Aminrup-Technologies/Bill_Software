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
    public partial class WebForm25 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dtmain = new DataTable();
        DataTable dtProInvPay = new DataTable();

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
            // --- INJECTION 1: Securing all search queries by CompanyID ---
            if (RadioButtonList1.SelectedIndex == 0)
            {
                BuindCompanyId();
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.Client_Id='" + lblclientId.Text + "' AND tbl_Quotation.CompanyID=" + CompanyContext.CurrentCompanyID + " order by tbl_Quotation.ID desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' AND tbl_Quotation.CompanyID=" + CompanyContext.CurrentCompanyID + " order by tbl_Quotation.ID desc";
                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.Client_Id='" + lblclientId.Text + "' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' AND tbl_Quotation.CompanyID=" + CompanyContext.CurrentCompanyID + " order by tbl_Quotation.ID desc";
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
            Response.Redirect("~/corporate/business/app/Delete_Quotation.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Quotation_no = Convert.ToString(e.CommandArgument);
            string ID = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "View")
            {
                string qdate = buindalldata(ID);

                // Safety check in case the user tried to view a record outside their company 
                // (buindalldata will return an empty string if company doesn't match)
                if (string.IsNullOrEmpty(qdate)) return;

                DateTime fromdate = DateTime.Parse(Convert.ToDateTime(qdate).ToShortDateString());
                DateTime todate = DateTime.Parse(Convert.ToDateTime("12-Jun-2018").ToShortDateString());
                if (fromdate > todate)
                {
                    Response.Redirect("/corporate/business/print/NewQuotation.aspx?ID=" + ID);
                }
                else
                {
                    Response.Redirect("/corporate/business/print/Quotation.aspx?ID=" + ID);
                }
            }

            if (e.CommandName == "Delete")
            {
                // --- INJECTION 2: Verify the record belongs to the active company before processing ---
                string query = "select Status1,Status2,PaymentStatus from tbl_Quotation where Quotation_no=@Quotation_no AND CompanyID=@CompanyID";
                SqlParameter[] pram = {
                    new SqlParameter("@Quotation_no", Quotation_no),
                    new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID)
                };

                dtProInvPay = DbCL.SPreturn_dt(query, pram);
                if (dtProInvPay.Rows.Count > 0)
                {
                    string status = "";
                    string pro = dtProInvPay.Rows[0]["Status1"].ToString();
                    string inv = dtProInvPay.Rows[0]["Status2"].ToString();
                    string pay = dtProInvPay.Rows[0]["PaymentStatus"].ToString();

                    if (pro == "Yes" || inv == "Yes" || pay == "Yes")
                    {
                        if (pro == "Yes") status = "Proforma Invoice";
                        if (inv == "Yes") status = status + " Tax Invoice";
                        if (pay == "Yes") status = status + " Payment Invoice";

                        status = "Delete " + status;

                        PanelError.Visible = true;
                        lblErrorMsg.Text = status;
                    }
                    else
                    {
                        // Proceed with deletion safely, as ownership was verified in the query above
                        DbCL.executeRdr("delete from tbl_Quotation where Quotation_no='" + Quotation_no + "'");
                        DbCL.executeRdr("delete from tbl_Quotaion_details where Quotation_no='" + Quotation_no + "'");
                        DbCL.executeRdr("delete from tbl_quotation_vat where Quotation_no='" + Quotation_no + "'");
                        DbCL.executeRdr("delete from tbl_QutPrimaryService where qut_no='" + Quotation_no + "'");
                        DbCL.executeRdr("delete from tbl_QutPaymentPhase where qut_no='" + Quotation_no + "'");
                        DbCL.executeRdr("delete from tbl_QuoPserTerm where qutno='" + Quotation_no + "'");
                        DbCL.executeRdr("delete from tbl_QutSiteAddress where qut_no='" + Quotation_no + "'");

                        PanelOK.Visible = true;
                        lblOk.Text = "Data Deleted Successfully...";
                        DataList1.Visible = false;
                    }
                }
            }
        }

        private string buindalldata(string ID)
        {
            string qdate = "";
            // --- INJECTION 3: Scoping data fetch by CompanyID ---
            string query = "select Quotation_no,Quotation_date,Client_Id,sub_total,Service_tax,Net_amount,cgstOrsgst,igst from tbl_Quotation where ID=@ID AND CompanyID=@CompanyID";
            SqlParameter[] pram = {
                new SqlParameter("@ID", ID),
                new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID)
            };

            dtmain = DbCL.SPreturn_dt(query, pram);
            if (dtmain.Rows.Count > 0)
            {
                qdate = dtmain.Rows[0]["Quotation_date"].ToString();
            }
            return qdate;
        }
    }
}