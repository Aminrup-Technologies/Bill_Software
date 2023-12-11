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
    public partial class WebForm46 : System.Web.UI.Page
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
                DbCL.FillCombo(cmbvendor, "select Vendor_Name from tbl_Vendor order by Vendor_Name");
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
                cmdstring = "select tbl_Purches.Purches_Id,tbl_Purches.Purches_date,tbl_Purches.Total_purches_rate,tbl_Vendor.Vendor_Name from tbl_Purches inner join tbl_Vendor on tbl_Purches.Client_Id=tbl_Vendor.Vendor_Id where tbl_Vendor.Vendor_Id='" + lblclientId.Text + "'  order by cast(tbl_Purches.Purches_date as datetime) desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                cmdstring = "select tbl_Purches.Purches_Id,tbl_Purches.Purches_date,tbl_Purches.Total_purches_rate,tbl_Vendor.Vendor_Name from tbl_Purches inner join tbl_Vendor on tbl_Purches.Client_Id=tbl_Vendor.Vendor_Id where  cast(tbl_Purches.Purches_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(tbl_Purches.Purches_date as datetime) desc";
                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                cmdstring = "select tbl_Purches.Purches_Id,tbl_Purches.Purches_date,tbl_Purches.Total_purches_rate,tbl_Vendor.Vendor_Name from tbl_Purches inner join tbl_Vendor on tbl_Purches.Client_Id=tbl_Vendor.Vendor_Id where  tbl_Vendor.Vendor_Id='" + lblclientId.Text + "' and cast(tbl_Purches.Purches_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(tbl_Purches.Purches_date as datetime) desc";
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
            string cmdstring = "select Vendor_Id from tbl_Vendor where Vendor_Name='" + cmbvendor.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblclientId.Text = re["Vendor_Id"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/add_payment_purchess.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Purches_Id = Convert.ToString(e.CommandArgument);



            if (e.CommandName == "Select")
            {
                Panel1.Visible = true;

                Binddetails(Purches_Id);
                Bindpriviouspayment(Purches_Id);
                Binddue(Purches_Id);
            }
        }

        private void Binddue(string Purches_Id)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Due_amount from tbl_purches_due where Purches_Id='" + Purches_Id.ToString() + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lbldue_amount.Text = re["Due_amount"].ToString();

            }
            
            DbCL.Conn.Close();
        }

        private void Bindpriviouspayment(string Purches_Id)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Payment_ID,Payment_Date,Given_amount,type from tbl_Purchess_payment where Purchess_ID='" + Purches_Id.ToString() + "'";
            SqlCommand cmd1 = new SqlCommand(cmdstring, DbCL.Conn);
            DataList2.DataSource = cmd1.ExecuteReader();
            DataList2.DataBind();
            DbCL.Conn.Close();
        }

        private void Binddetails(string Purches_Id)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Purches where Purches_Id='" + Purches_Id.ToString() + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblpuechess_id.Text = re["Purches_Id"].ToString();
                lblpuechess_Date.Text = re["Purches_date"].ToString();
                lblvendor_id.Text = re["Client_Id"].ToString();
                lblpaayment_amount.Text = re["Total_purches_rate"].ToString();
                
            }
            DbCL.Conn.Close();
            BindclientName();
        }

        private void BindclientName()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Vendor_Name from tbl_Vendor where Vendor_Id='" + lblvendor_id.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblvendor_Name.Text = re["Vendor_Name"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            if (Convert.ToDouble(lbldue_amount.Text) == 0)
            {
                lblErrorMsg.Text = "Full Amount is Paid for the Purchesse..";
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

            if (RadioButtonList2.SelectedIndex == 0)
            {

                date1 = dated + txtcashDate.Text;

            }
            else if (RadioButtonList2.SelectedIndex == 3)
            {
                date1 = dated + txtneftdate.Text;
                no = txtneftnumber.Text + comma;
                bank = txtbankname1.Text;

            }
            else
            {
                date1 = dated + txtdddate.Text;
                no = txtDDno.Text + comma;
                bank = txtBankName.Text;
            }
            string cmdstring = "insert into tbl_Purchess_payment(Payment_ID,Payment_Date,Purchess_ID,Purchess_Date,Client_Id,Net_amount,Given_amount,type,Ch_no,Ch_bank,Ch_date,Due_amount)values(@Payment_ID,@Payment_Date,@Purchess_ID,@Purchess_Date,@Client_Id,@Net_amount,@Given_amount,@type,@Ch_no,@Ch_bank,@Ch_date,@Due_amount)";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 0;
            cmd.Parameters.AddWithValue("@Payment_ID", paymentid.ToString());
            cmd.Parameters.AddWithValue("@Payment_Date", txtpaymentdate.Text);
            cmd.Parameters.AddWithValue("@Purchess_ID", lblpuechess_id.Text);
            cmd.Parameters.AddWithValue("@Purchess_Date", lblpuechess_Date.Text);
            cmd.Parameters.AddWithValue("@Client_Id", lblvendor_id.Text);
            cmd.Parameters.AddWithValue("@Net_amount", lblpaayment_amount.Text);


            cmd.Parameters.AddWithValue("@Given_amount", txtpaymentamount.Text);
            cmd.Parameters.AddWithValue("@type", RadioButtonList2.Text);
            cmd.Parameters.AddWithValue("@Ch_no", no.ToString());
            cmd.Parameters.AddWithValue("@Ch_bank", bank.ToString());
            cmd.Parameters.AddWithValue("@Ch_date", date1.ToString());
            cmd.Parameters.AddWithValue("@Due_amount", due1.ToString());

            cmd.ExecuteNonQuery();
            DbCL.Conn.Close();
            DbCL.executeRdr("update tbl_purches_due set Due_amount='" + due1.ToString() + "' where Purches_Id='" + lblpuechess_id.Text + "'");
        }
        private string BindpaymentId()
        {
            string paymentIDdetai = "";
            string aa = "";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdString1 = "select ID,Payment_ID from tbl_Purchess_payment where ID=(select max(ID)from tbl_Purchess_payment)";
            SqlCommand com1 = new SqlCommand(cmdString1, DbCL.Conn);
            SqlDataReader DR1 = com1.ExecuteReader();
            if (DR1.Read())
            {
                aa = DR1.GetValue(1).ToString();
                string bb = aa.Substring(3);
                int k = Convert.ToInt32(bb);
                k = k + 1;
                string q = Convert.ToString(k);
                paymentIDdetai = "PN0" + q;
            }
            else
            {
                paymentIDdetai = "PN01";
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

            }
        }
    }
}