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
    public partial class WebForm34 : System.Web.UI.Page
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
                txtpaymentdate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtcashDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtdddate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtneftdate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            }
        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                BuindCompanyId();
                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_No,tbl_Quotation.Quotation_Date,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Client_Id='" + lblclientId.Text + "'   order by tbl_Quotation.ID desc";
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.Client_Id='" + lblclientId.Text + "' order by tbl_Quotation.ID desc";

                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                //cmdstring = "select tbl_Quotation.Quotation_No,tbl_Quotation.Quotation_Date,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where  cast(tbl_Quotation.Quotation_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "'  order by tbl_Quotation.ID desc";
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by tbl_Quotation.ID desc";

                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                //cmdstring = "select tbl_Quotation.Quotation_No,tbl_Quotation.Quotation_Date,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where  tbl_Quotation.Client_Id='" + lblclientId.Text + "' and cast(tbl_Quotation.Quotation_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "'  order by tbl_Quotation.ID desc";
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.Client_Id='" + lblclientId.Text + "' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by tbl_Quotation.ID desc";
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
            Response.Redirect("~/corporate/business/app/add_payment.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Quotation_no = Convert.ToString(e.CommandArgument);
            if (e.CommandName == "Select")
            {
                Panel1.Visible = true;
                Binddetails(Quotation_no);
                Bindpriviouspayment(Quotation_no);
                Binddue(Quotation_no);
            }
        }

        private void Binddue(string Invoice_No)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Due_amount from tbl_invoice_due where qutation_no='" + lblQuotation_no.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lbldue_amount.Text = re["Due_amount"].ToString();
            }
            else
            {
                lbldue_amount.Text = lblNet_amount.Text;
            }
            DbCL.Conn.Close();
        }

        private void Bindpriviouspayment(string Invoice_No)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Payment_ID,Payment_Date,Given_amount,type from tbl_invoice_payment where Quotation_No='" + Invoice_No.ToString() + "'";
            SqlCommand cmd1 = new SqlCommand(cmdstring, DbCL.Conn);
            DataList2.DataSource = cmd1.ExecuteReader();
            DataList2.DataBind();
            DbCL.Conn.Close();
        }

        private void Binddetails(string Invoice_No)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Quotation where Quotation_no='" + Invoice_No.ToString() + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblClient_Id.Text = re["Client_Id"].ToString();
                lblQuotation_no.Text = re["Quotation_No"].ToString();
                lblQuotation_date.Text = re["Quotation_Date"].ToString();
                //lblGross_amount.Text = re["Gross"].ToString();
                lblservicetax.Text = re["service_tax1"].ToString();
                lblNet_amount.Text = re["Net_Amount"].ToString();
                //lblInvoice_no.Text = re["Invoice_No"].ToString();
                //lblInvoice_Date.Text = re["Invoice_Date"].ToString();
                lblsubtotal.Text = re["sub_total"].ToString();
                //lbldiscount.Text = re["discount"].ToString();
            }
            DbCL.Conn.Close();
            BindclientName();
            
        }
        private void BindclientName()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Name from tbl_Client where Client_Id='" + lblClient_Id.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblClientName.Text = re["Client_Name"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            if (Convert.ToDouble(lbldue_amount.Text) == 0)
            {
                lblErrorMsg.Text = "Full Amount is Paid for the Quotations..";
                PanelError.Visible = true;
            }
            else if (Convert.ToDouble(lbldue_amount.Text) < Convert.ToDouble(txtpaymentamount.Text))
            {
                lblErrorMsg.Text = "Due Amount Is less Than Given Amount...";
                PanelError.Visible = true;
            }
            else
            {
                InserttotalDate();
                PanelOK.Visible = true;
                lblOk.Text = "Data Save Successfully...";
                Button1.Visible = false;
                PanelError.Visible = false;
            }
        }

        private void InserttotalDate()
        {
            string paymentid = BindpaymentId();
            
            string comma = ",";
            string dated = " Dated:";
            string date1 = "";
            string no = "";
            string bank = "";
            double due = Convert.ToDouble(lbldue_amount.Text) - Convert.ToDouble(txtpaymentamount.Text);
            string due1 = due.ToString();

            // copy from quantum add_Bill.aspx

            string currancy = "";
            string rs_value = "";
            string given_amount = "";
            string given_amount1 = "";
            double tdsamount = 0;
            string tdsamount1 = "";

            if (lblcurrancy.Text == "INR")
            {
                currancy = RadioButtonList1.Text;
                rs_value = "Rs.";
                //given_amount = fetchgivenanount();
                given_amount = txtpaymentamount.Text;
                given_amount1 = given_amount.ToString();

                tdsamount = Convert.ToDouble(txttdsamount.Text);
                //tdsamount = Convert.ToDouble(txtgivenamount.Text) - Convert.ToDouble(given_amount);
                tdsamount1 = Convert.ToString(tdsamount);

                if (RadioButtonList2.SelectedIndex == 0)
                {
                    date1 = dated + txtcashDate.Text;
                }
                else if (RadioButtonList2.SelectedIndex == 3)
                {
                    date1 = dated + txtneftdate.Text;
                    no = txtneftnumber.Text + comma;
                    //bank = txtbankname1.Text;


                }
            }
            else
            {
                date1 = dated + txtdddate.Text;
                no = txtDDno.Text + comma;
                bank = txtBankName.Text;
                currancy = lblcurrancy1.Text;
                rs_value = "";
                given_amount = txtpaymentamount.Text;
                given_amount1 = txtpaymentamount.Text;

            }
            string cmdstring = "insert into tbl_invoice_payment(Payment_ID,Payment_Date,Quotation_No,Quotation_Date,Client_Id,Sub_total,Service_tax,Net_amount,Given_amount,type,Ch_no,Ch_bank,Ch_date,Due_amount,subtotal,tds,currancy)values(@Payment_ID,@Payment_Date,@Quotation_No,@Quotation_Date,@Client_Id,@Sub_total,@Service_tax,@Net_amount,@Given_amount,@type,@Ch_no,@Ch_bank,@Ch_date,@Due_amount,@subtotal,@tds,@currancy)";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 0;
            cmd.Parameters.AddWithValue("@Payment_ID", paymentid.ToString());
            cmd.Parameters.AddWithValue("@Payment_Date", txtpaymentdate.Text);
            //cmd.Parameters.AddWithValue("@Invoice_No", lblInvoice_no.Text);
            //cmd.Parameters.AddWithValue("@Invoice_Date", lblInvoice_Date.Text);
            cmd.Parameters.AddWithValue("@Quotation_No", lblQuotation_no.Text);
            cmd.Parameters.AddWithValue("@Quotation_Date", lblQuotation_date.Text);

            cmd.Parameters.AddWithValue("@Client_Id", lblClient_Id.Text);
            cmd.Parameters.AddWithValue("@Sub_total", lblsubtotal.Text);
            cmd.Parameters.AddWithValue("@Service_tax", lblservicetax.Text);
            cmd.Parameters.AddWithValue("@Net_amount", lblNet_amount.Text);

            cmd.Parameters.AddWithValue("@Given_amount", txtpaymentamount.Text);
            cmd.Parameters.AddWithValue("@type", RadioButtonList2.Text);
            cmd.Parameters.AddWithValue("@Ch_no", no.ToString());
            cmd.Parameters.AddWithValue("@Ch_bank", bank.ToString());
            cmd.Parameters.AddWithValue("@Ch_date", date1.ToString());
            cmd.Parameters.AddWithValue("@Due_amount", due1.ToString());
            cmd.Parameters.AddWithValue("@subtotal", lblsubtotal.Text);
            cmd.Parameters.AddWithValue("@tds", RadioButtonList2.Text);
            cmd.Parameters.AddWithValue("@currancy", lblcurrancy.Text);
            //cmd.Parameters.AddWithValue("@discount", lbldiscount.Text);

            cmd.ExecuteNonQuery();
            DbCL.Conn.Close();

            

            insertdue(due1);
            updatedetails();

            DbCL.executeRdr("UPDATE Table_A SET Table_A.Invoice_No = Table_B.Invoice_No FROM tbl_invoice_payment AS Table_A INNER JOIN tbl_Invoice AS Table_B ON Table_A.Quotation_no = Table_B.Quotation_No and Table_A.Invoice_No IS NULL and Due_amount='0.00'");
            //DbCL.executeRdr("update tbl_Invoice set status1='Yes' where Invoice_No='" + lblInvoice_no.Text + "'");
            DbCL.executeRdr("update tbl_Quotation set PaymentStatus='Yes' where Quotation_no='" + lblQuotation_no.Text + "'");
        }

        private void updatedetails()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
             
            string cmdstring = "select Invoice_No,Invoice_Date from tbl_Invoice where Quotation_No='" + lblQuotation_no.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                string inno = re["Invoice_No"].ToString();
                string indate = re["Invoice_Date"].ToString();
                DbCL.executeRdr("UPDATE tbl_invoice_payment SET Invoice_No='"+ inno .ToString() + "',Invoice_Date='"+ indate.ToString()  + "' where Quotation_No='+ lblQuotation_no.Text +'");
            }
            DbCL.Conn.Close();
        }

        private void insertdsdetails(string payment_id, string tdsamount1, string date)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdsrting = "insert into tbl_invoice_payment_tds(invoice_no,payment_id,quotation_no,service_tax,total_amount,payment_date,tds_amount,tds_rate,client_name,invoice_date)values(@invoice_no,@payment_id,@quotation_no,@service_tax,@total_amount,@payment_date,@tds_amount,@tds_rate,@client_name,@invoice_date)";

            SqlCommand cmd = new SqlCommand(cmdsrting, DbCL.Conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@invoice_no", lblinvoiceno.Text);
            cmd.Parameters.AddWithValue("@payment_id", payment_id.ToString());
            cmd.Parameters.AddWithValue("@quotation_no", lblQuotation_no.Text);
            cmd.Parameters.AddWithValue("@service_tax", lblservicetax.Text);
            cmd.Parameters.AddWithValue("@total_amount", lblsubtotal.Text);
            //cmd.Parameters.AddWithValue("@primary_service", lblprimaryservice.Text);
           // cmd.Parameters.AddWithValue("@secondary_service", lblservice.Text);
            cmd.Parameters.AddWithValue("@payment_date", date.ToString());
            cmd.Parameters.AddWithValue("@tds_amount", tdsamount1.ToString());
            cmd.Parameters.AddWithValue("@tds_rate", cmbtdsvalue.Text);
            cmd.Parameters.AddWithValue("@client_name", cmbvendor.Text);
            cmd.Parameters.AddWithValue("@invoice_date", lblinvoicedate.Text);
            //cmd.Parameters.AddWithValue("@client_id", lblid.Text);
            cmd.ExecuteNonQuery();
            //cmd1.ExecuteNonQuery();
            DbCL.Conn.Close();

        }

        private void insertdue(string due1)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Due_amount from tbl_invoice_due where qutation_no='" + lblQuotation_no.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            SqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                DbCL.executeRdr("update tbl_invoice_due set Due_amount='" + due1.ToString() + "' where qutation_no='" + lblQuotation_no.Text + "'");
            }
            else
            {
                DbCL.executeRdr("insert into tbl_invoice_due(qutation_no,Due_amount)values('" + lblQuotation_no.Text + "','" + due1 + "')");
            }
            DbCL.Conn.Close();

        }

        private string BindpaymentId()
        {
            string paymentIDdetai = "";
            string aa = "";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdString1 = "select ID,Payment_ID from tbl_invoice_payment where ID=(select max(ID)from tbl_invoice_payment)";
            SqlCommand com1 = new SqlCommand(cmdString1, DbCL.Conn);
            SqlDataReader DR1 = com1.ExecuteReader();
            if (DR1.Read())
            {
                aa = DR1.GetValue(1).ToString();
                string bb = aa.Substring(3);
                int k = Convert.ToInt32(bb);
                k = k + 1;
                string q = Convert.ToString(k);
                paymentIDdetai = "P00" + q;
            }
            else
            {
                paymentIDdetai = "P001";
            }

            DbCL.Conn.Close();
            return paymentIDdetai;
        }

        protected void RadioButtonList2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (RadioButtonList2.SelectedIndex == 0)
            {
                First.Visible = true;
                Second.Visible = false;
                Third.Visible = false;
                txttdsamount.Visible = true;
                txttdsamount.Text = (Convert.ToDouble(lbldue_amount.Text) - Convert.ToDouble(txtpaymentamount.Text)).ToString();


            }
            else if (RadioButtonList2.SelectedIndex == 3)
            {
                First.Visible = false;
                Second.Visible = false;
                Third.Visible = true;

            }
            else
            {
                First.Visible = false;
                Second.Visible = true;
                Third.Visible = false;
                txttdsamount.Visible = false;
                txttdsamount.Text = "0";

            }
        }

        protected void txttdsamount_TextChanged(object sender, EventArgs e)
        {

        }

    }
}