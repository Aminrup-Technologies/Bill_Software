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
    public partial class WebForm88 : System.Web.UI.Page
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
                //cmdstring = "select a.ID,a.Payment_ID,a.Payment_Date,a.Invoice_No,a.Invoice_Date,a.Quotation_No,a.Quotation_Date,a.Net_amount,a.mailDate,a.subtotal,a.Due_amount,(a.Net_amount - a.subtotal) as Gst,b.Client_Name,c.PServiceName from tbl_invoice_payment as a left outer join tbl_QuoPriSerTogather as c on a.Quotation_No = c.qutno left outer join tbl_Client as b on b.Client_Id = a.Client_ID where a.Client_Id='" + lblclientId.Text + "' and a.Due_amount='0.00' order by a.ID desc";
                cmdstring = "select a.ID,a.Quotation_no,a.Quotation_date,a.Net_amount,a.sub_total,a.service_tax1, sum(cast(b.Given_amount as real)) as givenamo,c.Client_Name,(cast(a.Net_amount as real) - sum(cast(b.Given_amount as real))) AS dueamo from tbl_invoice_payment as b inner join tbl_Quotation as a on a.Quotation_no=b.Quotation_No inner join tbl_Client as c on c.Client_Id=b.Client_Id where c.Client_Id='" + lblclientId.Text + "' group by a.Quotation_No,a.Net_amount,a.sub_total,a.service_tax1,c.Client_Name,a.ID,a.Quotation_date order by a.ID desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                //cmdstring = "select a.ID,a.Payment_ID,a.Payment_Date,a.Invoice_No,a.Invoice_Date,a.Quotation_No,a.Quotation_Date,a.Net_amount,a.mailDate,a.subtotal,a.Due_amount,(a.Net_amount - a.subtotal) as Gst,b.Client_Name,c.PServiceName from tbl_invoice_payment as a left outer join tbl_QuoPriSerTogather as c on a.Quotation_No = c.qutno left outer join tbl_Client as b on b.Client_Id = a.Client_ID where a.Due_amount='0.00' and cast(a.Payment_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by a.ID desc";
                cmdstring = "select a.ID,a.Quotation_no,a.Quotation_date,a.Net_amount,a.sub_total,a.service_tax1, sum(cast(b.Given_amount as real)) as givenamo,c.Client_Name,(cast(a.Net_amount as real) - sum(cast(b.Given_amount as real))) AS dueamo from tbl_invoice_payment as b inner join tbl_Quotation as a on a.Quotation_no=b.Quotation_No inner join tbl_Client as c on c.Client_Id=b.Client_Id where cast(a.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' group by a.Quotation_No,a.Net_amount,a.sub_total,a.service_tax1,c.Client_Name,a.ID,a.Quotation_date order by a.ID desc";
                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                //cmdstring = "select a.ID,a.Payment_ID,a.Payment_Date,a.Invoice_No,a.Invoice_Date,a.Quotation_No,a.Quotation_Date,a.Net_amount,a.mailDate,a.subtotal,a.Due_amount,(a.Net_amount - a.subtotal) as Gst,b.Client_Name,c.PServiceName from tbl_invoice_payment as a left outer join tbl_QuoPriSerTogather as c on a.Quotation_No = c.qutno left outer join tbl_Client as b on b.Client_Id = a.Client_ID where a.Due_amount='0.00' and a.Client_Id='" + lblclientId.Text + "' and cast(a.Payment_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by a.ID desc";
                cmdstring = "select a.ID,a.Quotation_no,a.Quotation_date,a.Net_amount,a.sub_total,a.service_tax1, sum(cast(b.Given_amount as real)) as givenamo,c.Client_Name,(cast(a.Net_amount as real) - sum(cast(b.Given_amount as real))) AS dueamo from tbl_invoice_payment as b inner join tbl_Quotation as a on a.Quotation_no=b.Quotation_No inner join tbl_Client as c on c.Client_Id=b.Client_Id where c.Client_Id='" + lblclientId.Text + "' and cast(a.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' group by a.Quotation_No,a.Net_amount,a.sub_total,a.service_tax1,c.Client_Name,a.ID,a.Quotation_date order by a.ID desc";
                Buinddatagrid(cmdstring);
            }
            btnSertch.Visible = false;
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/PaymentsDue.aspx");
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

    }
}