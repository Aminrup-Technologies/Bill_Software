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
    public partial class delete_purchaseorder : System.Web.UI.Page
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
                cmbvendor.Items.Clear();
                cmbvendor.Items.Add("--Select--");
                DbCL.Sqlconnection();
                DbCL.ConnectDb();
                using (SqlCommand cmd = new SqlCommand("SELECT Client_Name FROM tbl_Client WHERE CompanyID=@CompanyID ORDER BY Client_Name", DbCL.Conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                            cmbvendor.Items.Add(rdr[0].ToString());
                    }
                }
                DbCL.Conn.Close();
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            }

        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string cmdstring = @"select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.RecordType='Purchase Order' AND tbl_Quotation.CompanyID=@CompanyID";

            List<SqlParameter> sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID)
            };

            if (RadioButtonList1.SelectedIndex == 0)
            {
                BuindCompanyId();
                cmdstring += " AND tbl_Quotation.Client_Id=@ClientId";
                sqlParams.Add(new SqlParameter("@ClientId", lblclientId.Text));
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                cmdstring += " AND cast(tbl_Quotation.Quotation_date as datetime) between @FromDate and @ToDate";
                sqlParams.Add(new SqlParameter("@FromDate", txttodate.Text));
                sqlParams.Add(new SqlParameter("@ToDate", txtfromDate.Text));
            }
            else
            {
                BuindCompanyId();
                cmdstring += " AND tbl_Quotation.Client_Id=@ClientId AND cast(tbl_Quotation.Quotation_date as datetime) between @FromDate and @ToDate";
                sqlParams.Add(new SqlParameter("@ClientId", lblclientId.Text));
                sqlParams.Add(new SqlParameter("@FromDate", txttodate.Text));
                sqlParams.Add(new SqlParameter("@ToDate", txtfromDate.Text));
            }

            cmdstring += " order by tbl_Quotation.ID desc";
            Buinddatagrid(cmdstring, sqlParams.ToArray());
            btnSertch.Visible = false;

        }

        private void Buinddatagrid(string cmdstring, SqlParameter[] parameters)
        {
            DataTable dt = new DataTable();
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
            }
            DbCL.Conn.Close();
            if (dt.Rows.Count > 0)
            {
                DataList1.DataSource = dt;
                DataList1.DataBind();
            }
            else
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "No Data Found...";
            }
        }

        private void BuindCompanyId()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            using (SqlCommand cmd = new SqlCommand("SELECT Client_Id FROM tbl_Client WHERE Client_Name=@ClientName AND CompanyID=@CompanyID", DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@ClientName", cmbvendor.Text);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                using (SqlDataReader re = cmd.ExecuteReader())
                {
                    if (re.Read())
                    {
                        lblclientId.Text = re["Client_Id"].ToString();
                    }
                }
            }
            DbCL.Conn.Close();
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/delete_purchaseorder.aspx");

        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Quotation_no = Convert.ToString(e.CommandArgument);

            string ID = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "View")
            {
                string qdate = buindalldata(ID);
                if (string.IsNullOrEmpty(qdate))
                    return;

                DateTime fromdate = DateTime.Parse(Convert.ToDateTime(qdate).ToShortDateString());
                DateTime todate = DateTime.Parse(Convert.ToDateTime("12-Jun-2018").ToShortDateString());
                if (fromdate > todate)
                {
                    Response.Redirect("/corporate/business/print/NewPurchaseOrder.aspx?ID=" + ID);
                }
                else
                {
                    Response.Redirect("/corporate/business/print/NewPurchaseOrder.aspx?ID=" + ID);
                }
                //string url = "/corporate/business/print/NewQuotation.aspx?ID=" + ID;
                //Response.Write("<script type='text/javascript'>window.open('" + url + "');</script>");
            }

            if (e.CommandName == "Delete")
            {

                string query = "select Status1,Status2,PaymentStatus from tbl_Quotation where Quotation_no=@Quotation_no AND CompanyID=@CompanyID";
                SqlParameter[] pram = {
                new SqlParameter("@Quotation_no",Quotation_no),
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
                        if (pro == "Yes")
                        {
                            status = "Proforma Invoice";
                        }
                        if (inv == "Yes")
                        {
                            status = status + " Tax Invoice";
                        }
                        if (pay == "Yes")
                        {
                            status = status + " Payment Invoice";
                        }
                        status = "Delete " + status;

                        PanelError.Visible = true;
                        lblErrorMsg.Text = status;
                    }
                    else
                    {
                        int companyId = CompanyContext.CurrentCompanyID;
                        DbCL.ExecuteNonQuery("delete from tbl_Quotation where Quotation_no=@QuotationNo AND CompanyID=@CompanyID",
                            new[] { new SqlParameter("@QuotationNo", Quotation_no), new SqlParameter("@CompanyID", companyId) });
                        DbCL.ExecuteNonQuery("delete from tbl_Quotaion_details where Quotation_no=@QuotationNo AND CompanyID=@CompanyID",
                            new[] { new SqlParameter("@QuotationNo", Quotation_no), new SqlParameter("@CompanyID", companyId) });
                        DbCL.ExecuteNonQuery("delete from tbl_quotation_vat where Quotation_no=@QuotationNo",
                            new[] { new SqlParameter("@QuotationNo", Quotation_no) });

                        DbCL.ExecuteNonQuery("delete from tbl_QutPrimaryService where qut_no=@QuotationNo AND CompanyID=@CompanyID",
                            new[] { new SqlParameter("@QuotationNo", Quotation_no), new SqlParameter("@CompanyID", companyId) });
                        DbCL.ExecuteNonQuery("delete from tbl_QutPaymentPhase where qut_no=@QuotationNo AND CompanyID=@CompanyID",
                            new[] { new SqlParameter("@QuotationNo", Quotation_no), new SqlParameter("@CompanyID", companyId) });

                        DbCL.ExecuteNonQuery("delete from tbl_QuoPserTerm where qutno=@QuotationNo AND CompanyID=@CompanyID",
                            new[] { new SqlParameter("@QuotationNo", Quotation_no), new SqlParameter("@CompanyID", companyId) });
                        DbCL.ExecuteNonQuery("delete from tbl_QutSiteAddress where qut_no=@QuotationNo",
                            new[] { new SqlParameter("@QuotationNo", Quotation_no) });

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
            string query = "select Quotation_no,Quotation_date,Client_Id,sub_total,Service_tax,Net_amount,cgstOrsgst,igst from tbl_Quotation where ID=@ID AND CompanyID=@CompanyID";
            SqlParameter[] pram = {
            new SqlParameter("@ID",ID),
            new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID)
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
