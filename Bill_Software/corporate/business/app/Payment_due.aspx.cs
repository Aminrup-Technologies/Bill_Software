using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm52 : System.Web.UI.Page
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

                DbCL.FillCombo(cmbvendor, "select Client_Name from tbl_Client order by Client_Name");
                DbCL.FillCombo(cmbvendor1, "select Client_Name from tbl_Client order by Client_Name");
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtfromDate1.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate1.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                // Bindgridi1();
                // Bindgridi2();
                //if (countre == 1)
                //{
                //    PanelOK.Visible = true;
                //    lblOk.Text = "No Payments Is Due...";
                //}
                //else
                //{
                //    PanelOK.Visible = false;
                //}

            }
        }

        private void Bindgridi2(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string CmdString = "SELECT tbl_Invoice.Invoice_No,tbl_Invoice.Invoice_Date,tbl_Invoice.Quotation_No,tbl_Invoice.Quotation_Date,tbl_Invoice.Net_Amount,tbl_Client.Client_Name,tbl_Invoice.Net_Amount,tbl_Invoice.Invoice_No as Invoice_No  FROM  tbl_invoice_due right outer join tbl_Invoice on tbl_Invoice.Invoice_No = tbl_invoice_due.Invoice_no inner join  tbl_Client  ON tbl_Client.Client_Id = tbl_Invoice.Client_ID  where tbl_invoice_due.Due_amount is Null";
            SqlCommand com1 = new SqlCommand(CmdString, DbCL.Conn);
            SqlDataAdapter da = new SqlDataAdapter(com1);
            SqlDataReader dr = com1.ExecuteReader();

            if (dr.Read())
            {

                DataList2.DataSource = DbCL.GetDataTable(CmdString);
                DataList2.DataBind();
                Second_div.Visible = true;
                countre = countre + 1;

            }
            else
            {
                Second_div.Visible = false;
            }
            DbCL.Conn.Close();
        }

        private void Bindgridi1(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string CmdString = "SELECT tbl_Invoice.Invoice_No,tbl_Invoice.Invoice_Date,tbl_Invoice.Quotation_No,tbl_Invoice.Quotation_Date,tbl_Invoice.Net_Amount,tbl_Client.Client_Name,tbl_invoice_due.Due_amount FROM tbl_Client INNER JOIN tbl_Invoice ON tbl_Client.Client_Id = tbl_Invoice.Client_ID INNER JOIN tbl_invoice_due ON tbl_Invoice.Invoice_No = tbl_invoice_due.Invoice_no where tbl_invoice_due.Due_amount <>'0'";
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


        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                BuindCompanyId();
                //cmdstring = "select tbl_Client.Client_Name,tbl_invoice_payment.Payment_ID,tbl_invoice_payment.Payment_Date,tbl_invoice_payment.Invoice_No,tbl_invoice_payment.Quotation_No,tbl_invoice_payment.Net_amount,tbl_invoice_payment.Given_amount,tbl_invoice_payment.type,tbl_invoice_payment.Ch_no,tbl_invoice_payment.Ch_date from tbl_invoice_payment inner join tbl_Client on tbl_invoice_payment.Client_Id=tbl_Client.Client_Id where tbl_invoice_payment.Client_Id='" + lblclientId.Text + "' order by cast(tbl_invoice_payment.Payment_Date as datetime) desc";
                cmdstring = "SELECT tbl_Invoice.Invoice_No,tbl_Invoice.Invoice_Date,tbl_Invoice.Quotation_No,tbl_Invoice.Quotation_Date,tbl_Invoice.Net_Amount,tbl_Client.Client_Name,tbl_invoice_due.Due_amount FROM tbl_Client INNER JOIN tbl_Invoice ON tbl_Client.Client_Id = tbl_Invoice.Client_ID INNER JOIN tbl_invoice_due ON tbl_Invoice.Invoice_No = tbl_invoice_due.Invoice_no where tbl_invoice_due.Due_amount <>'0' and tbl_Client.Client_Id='" + lblclientId.Text + "'";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                //cmdstring = "select tbl_Client.Client_Name,tbl_invoice_payment.Payment_ID,tbl_invoice_payment.Payment_Date,tbl_invoice_payment.Invoice_No,tbl_invoice_payment.Quotation_No,tbl_invoice_payment.Net_amount,tbl_invoice_payment.Given_amount,tbl_invoice_payment.type,tbl_invoice_payment.Ch_no,tbl_invoice_payment.Ch_date from tbl_invoice_payment inner join tbl_Client on tbl_invoice_payment.Client_Id=tbl_Client.Client_Id where cast(tbl_invoice_payment.Payment_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(tbl_invoice_payment.Payment_Date as datetime) desc";
                cmdstring = "SELECT tbl_Invoice.Invoice_No,tbl_Invoice.Invoice_Date,tbl_Invoice.Quotation_No,tbl_Invoice.Quotation_Date,tbl_Invoice.Net_Amount,tbl_Client.Client_Name,tbl_invoice_due.Due_amount FROM tbl_Client INNER JOIN tbl_Invoice ON tbl_Client.Client_Id = tbl_Invoice.Client_ID INNER JOIN tbl_invoice_due ON tbl_Invoice.Invoice_No = tbl_invoice_due.Invoice_no where tbl_invoice_due.Due_amount <>'0' and cast(tbl_Invoice.Invoice_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "'";
                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                //cmdstring = "select tbl_Client.Client_Name,tbl_invoice_payment.Payment_ID,tbl_invoice_payment.Payment_Date,tbl_invoice_payment.Invoice_No,tbl_invoice_payment.Quotation_No,tbl_invoice_payment.Net_amount,tbl_invoice_payment.Given_amount,tbl_invoice_payment.type,tbl_invoice_payment.Ch_no,tbl_invoice_payment.Ch_date from tbl_invoice_payment inner join tbl_Client on tbl_invoice_payment.Client_Id=tbl_Client.Client_Id where tbl_invoice_payment.Client_Id='" + lblclientId.Text + "' and cast(tbl_invoice_payment.Payment_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(tbl_invoice_payment.Payment_Date as datetime) desc";
                cmdstring = "SELECT tbl_Invoice.Invoice_No,tbl_Invoice.Invoice_Date,tbl_Invoice.Quotation_No,tbl_Invoice.Quotation_Date,tbl_Invoice.Net_Amount,tbl_Client.Client_Name,tbl_invoice_due.Due_amount FROM tbl_Client INNER JOIN tbl_Invoice ON tbl_Client.Client_Id = tbl_Invoice.Client_ID INNER JOIN tbl_invoice_due ON tbl_Invoice.Invoice_No = tbl_invoice_due.Invoice_no where tbl_invoice_due.Due_amount <>'0' and tbl_Client.Client_Id='" + lblclientId.Text + "' and cast(tbl_Invoice.Invoice_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "'";
                Buinddatagrid(cmdstring);
            }
            btnSertch.Visible = false;
        }


        protected void btnSertch1_Click(object sender, EventArgs e)
        {
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                BuindCompanyId1();
                //cmdstring = "select tbl_Client.Client_Name,tbl_invoice_payment.Payment_ID,tbl_invoice_payment.Payment_Date,tbl_invoice_payment.Invoice_No,tbl_invoice_payment.Quotation_No,tbl_invoice_payment.Net_amount,tbl_invoice_payment.Given_amount,tbl_invoice_payment.type,tbl_invoice_payment.Ch_no,tbl_invoice_payment.Ch_date from tbl_invoice_payment inner join tbl_Client on tbl_invoice_payment.Client_Id=tbl_Client.Client_Id where tbl_invoice_payment.Client_Id='" + lblclientId.Text + "' order by cast(tbl_invoice_payment.Payment_Date as datetime) desc";
                cmdstring = "SELECT tbl_Invoice.Invoice_No,tbl_Invoice.Invoice_Date,tbl_Invoice.Quotation_No,tbl_Invoice.Quotation_Date,tbl_Invoice.Net_Amount,tbl_Client.Client_Name,tbl_Invoice.Net_Amount,tbl_Invoice.Invoice_No as Invoice_No  FROM  tbl_invoice_due right outer join tbl_Invoice on tbl_Invoice.Invoice_No = tbl_invoice_due.Invoice_no inner join  tbl_Client  ON tbl_Client.Client_Id = tbl_Invoice.Client_ID  where tbl_invoice_due.Due_amount is Null and tbl_Client.Client_Id='" + lblclientId.Text + "'";
                Buinddatagrid1(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                //cmdstring = "select tbl_Client.Client_Name,tbl_invoice_payment.Payment_ID,tbl_invoice_payment.Payment_Date,tbl_invoice_payment.Invoice_No,tbl_invoice_payment.Quotation_No,tbl_invoice_payment.Net_amount,tbl_invoice_payment.Given_amount,tbl_invoice_payment.type,tbl_invoice_payment.Ch_no,tbl_invoice_payment.Ch_date from tbl_invoice_payment inner join tbl_Client on tbl_invoice_payment.Client_Id=tbl_Client.Client_Id where cast(tbl_invoice_payment.Payment_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(tbl_invoice_payment.Payment_Date as datetime) desc";
                cmdstring = "SELECT tbl_Invoice.Invoice_No,tbl_Invoice.Invoice_Date,tbl_Invoice.Quotation_No,tbl_Invoice.Quotation_Date,tbl_Invoice.Net_Amount,tbl_Client.Client_Name,tbl_Invoice.Net_Amount,tbl_Invoice.Invoice_No as Invoice_No  FROM  tbl_invoice_due right outer join tbl_Invoice on tbl_Invoice.Invoice_No = tbl_invoice_due.Invoice_no inner join  tbl_Client  ON tbl_Client.Client_Id = tbl_Invoice.Client_ID  where tbl_invoice_due.Due_amount is Null and cast(tbl_Invoice.Invoice_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "'";
                Buinddatagrid1(cmdstring);
            }
            else
            {
                BuindCompanyId1();
                //cmdstring = "select tbl_Client.Client_Name,tbl_invoice_payment.Payment_ID,tbl_invoice_payment.Payment_Date,tbl_invoice_payment.Invoice_No,tbl_invoice_payment.Quotation_No,tbl_invoice_payment.Net_amount,tbl_invoice_payment.Given_amount,tbl_invoice_payment.type,tbl_invoice_payment.Ch_no,tbl_invoice_payment.Ch_date from tbl_invoice_payment inner join tbl_Client on tbl_invoice_payment.Client_Id=tbl_Client.Client_Id where tbl_invoice_payment.Client_Id='" + lblclientId.Text + "' and cast(tbl_invoice_payment.Payment_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(tbl_invoice_payment.Payment_Date as datetime) desc";
                cmdstring = "SELECT tbl_Invoice.Invoice_No,tbl_Invoice.Invoice_Date,tbl_Invoice.Quotation_No,tbl_Invoice.Quotation_Date,tbl_Invoice.Net_Amount,tbl_Client.Client_Name,tbl_Invoice.Net_Amount,tbl_Invoice.Invoice_No as Invoice_No  FROM  tbl_invoice_due right outer join tbl_Invoice on tbl_Invoice.Invoice_No = tbl_invoice_due.Invoice_no inner join  tbl_Client  ON tbl_Client.Client_Id = tbl_Invoice.Client_ID  where tbl_invoice_due.Due_amount is Null and tbl_Client.Client_Id='" + lblclientId.Text + "' and cast(tbl_Invoice.Invoice_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "'";
                Buinddatagrid1(cmdstring);
            }
            btnSertch1.Visible = false;
        }
        private void Buinddatagrid(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                Bindgridi1(cmdstring);
            }
            else
            {
                PanelOK.Visible = true;
                lblOk.Text = "No Data Found...";

            }
            DbCL.Conn.Close();
        }

        private void Buinddatagrid1(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                Bindgridi2(cmdstring);
            }
            else
            {
                PanelOK.Visible = true;
                lblOk.Text = "No Data Found...";

            }
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

        private void BuindCompanyId1()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Id from tbl_Client where Client_Name='" + cmbvendor1.Text + "'";
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
            Response.Redirect("~/corporate/business/app/Payment_due.aspx");
        }

        protected void btnreset2_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/Payment_due.aspx");
        }

    }
}