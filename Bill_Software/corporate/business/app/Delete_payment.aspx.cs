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
    public partial class WebForm37 : System.Web.UI.Page
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
                //cmdstring = "select tbl_Client.Client_Name,tbl_invoice_payment.Payment_ID,tbl_invoice_payment.Payment_Date,tbl_invoice_payment.Invoice_No,tbl_invoice_payment.Quotation_No,tbl_invoice_payment.Net_amount,tbl_invoice_payment.Given_amount,tbl_invoice_payment.type,tbl_invoice_payment.Ch_no,tbl_invoice_payment.Ch_date from tbl_invoice_payment inner join tbl_Client on tbl_invoice_payment.Client_Id=tbl_Client.Client_Id where tbl_invoice_payment.Client_Id='" + lblclientId.Text + "' order by cast(tbl_invoice_payment.Payment_Date as datetime) desc";
                cmdstring = "select a.ID,a.Payment_ID,a.Payment_Date,a.Invoice_No,a.Invoice_Date,a.Quotation_No,a.Quotation_Date,a.Net_amount,a.mailDate,a.subtotal,a.Due_amount,a.Given_amount,a.type,a.Ch_no,a.Ch_date,a.tds,(a.Net_amount - a.subtotal) as Gst,b.Client_Name,c.PServiceName from tbl_invoice_payment as a left outer join tbl_QuoPriSerTogather as c on a.Quotation_No = c.qutno left outer join tbl_Client as b on b.Client_Id = a.Client_ID where a.Client_Id='" + lblclientId.Text + "' order by a.ID desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                //cmdstring = "select tbl_Client.Client_Name,tbl_invoice_payment.Payment_ID,tbl_invoice_payment.Payment_Date,tbl_invoice_payment.Invoice_No,tbl_invoice_payment.Quotation_No,tbl_invoice_payment.Net_amount,tbl_invoice_payment.Given_amount,tbl_invoice_payment.type,tbl_invoice_payment.Ch_no,tbl_invoice_payment.Ch_date from tbl_invoice_payment inner join tbl_Client on tbl_invoice_payment.Client_Id=tbl_Client.Client_Id where cast(tbl_invoice_payment.Payment_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(tbl_invoice_payment.Payment_Date as datetime) desc";
                cmdstring = "select a.ID,a.Payment_ID,a.Payment_Date,a.Invoice_No,a.Invoice_Date,a.Quotation_No,a.Quotation_Date,a.Net_amount,a.mailDate,a.subtotal,a.Due_amount,a.Given_amount,a.type,a.Ch_no,a.Ch_date,a.tds,(a.Net_amount - a.subtotal) as Gst,b.Client_Name,c.PServiceName from tbl_invoice_payment as a left outer join tbl_QuoPriSerTogather as c on a.Quotation_No = c.qutno left outer join tbl_Client as b on b.Client_Id = a.Client_ID where cast(a.Payment_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by a.ID desc";
                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                //cmdstring = "select tbl_Client.Client_Name,tbl_invoice_payment.Payment_ID,tbl_invoice_payment.Payment_Date,tbl_invoice_payment.Invoice_No,tbl_invoice_payment.Quotation_No,tbl_invoice_payment.Net_amount,tbl_invoice_payment.Given_amount,tbl_invoice_payment.type,tbl_invoice_payment.Ch_no,tbl_invoice_payment.Ch_date from tbl_invoice_payment inner join tbl_Client on tbl_invoice_payment.Client_Id=tbl_Client.Client_Id where tbl_invoice_payment.Client_Id='" + lblclientId.Text + "' and cast(tbl_invoice_payment.Payment_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(tbl_invoice_payment.Payment_Date as datetime) desc";
                cmdstring = "select a.ID,a.Payment_ID,a.Payment_Date,a.Invoice_No,a.Invoice_Date,a.Quotation_No,a.Quotation_Date,a.Net_amount,a.mailDate,a.subtotal,a.Due_amount,a.Given_amount,a.type,a.Ch_no,a.Ch_date,a.tds,(a.Net_amount - a.subtotal) as Gst,b.Client_Name,c.PServiceName from tbl_invoice_payment as a left outer join tbl_QuoPriSerTogather as c on a.Quotation_No = c.qutno left outer join tbl_Client as b on b.Client_Id = a.Client_ID where a.Client_Id='" + lblclientId.Text + "' and cast(a.Payment_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by a.ID desc";
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
            Response.Redirect("~/corporate/business/app/Delete_payment.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Payment_ID = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Delete")
            {

              string qno= updatedue(Payment_ID);

                //DbCL.executeRdr("update tbl_Quotation set PaymentStatus='Yes' where Quotation_no='" + lblQuotation_no.Text + "'");

                DbCL.executeRdr("delete from tbl_invoice_payment where Payment_ID='" + Payment_ID + "'");
                string OthQut=checkotherpayment(qno);
                if (OthQut=="")
                {
                    DbCL.executeRdr("update tbl_Quotation set PaymentStatus='No' where Quotation_no='" + qno + "'");
                }

                if (Session["invno"]!=null)
                {
                    string invno = Session["invno"].ToString();
                    CheckOtherPayExit(invno);
                }
                PanelOK.Visible = true;
                lblOk.Text = "Data Deleted Successfully...";
                DataList1.Visible = false;
            }
        }

        private string checkotherpayment(string qno)
        {
            string qvalue = "";
            string query = "select ID,Quotation_No from tbl_invoice_payment where Quotation_No=@Quotation_No";
            SqlParameter[] pram = {
                new SqlParameter("@Quotation_No",qno)
            };
            SqlDataReader rdc = DbCL.SPReturnRdr(query,pram);
            if (rdc.Read())
            {
                qvalue = rdc["ID"].ToString();
            }
            return qvalue;
        }

        private void CheckOtherPayExit(string invno)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Invoice_No,Given_amount,Net_amount from tbl_invoice_payment where Invoice_No='" + invno + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
               
            }
            else {

                DbCL.executeRdr("update tbl_Invoice set status1='No' where Invoice_No='" + invno + "'");
            }
            DbCL.Conn.Close();
        }

        private string updatedue(string Payment_ID)
        {
            string qno = "";

            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Invoice_No,Given_amount,Net_amount,Quotation_No from tbl_invoice_payment where Payment_ID='" + Payment_ID + "'";
            SqlCommand cmd = new SqlCommand(cmdstring,DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if(re.Read())
            {
                string invoiceNo = re["Invoice_No"].ToString();
                string qutationNo = re["Quotation_No"].ToString();
                qno = qutationNo;
                Session["invno"] = invoiceNo;
                string invoice_Amount = re["Net_amount"].ToString();

                double invoiceAmount = Convert.ToDouble(invoice_Amount);

                string GivenAmount = re["Given_amount"].ToString();
                string Dueamount = finddueamount(qutationNo);
                double c = Convert.ToDouble(GivenAmount) + Convert.ToDouble(Dueamount);
                string d = c.ToString();
                //if (invoice_Amount == d)
                if (invoiceAmount == c)
                {
                    //DbCL.executeRdr("delete from tbl_invoice_due where Invoice_no='" + invoiceNo.ToString() + "'");
                    DbCL.executeRdr("delete from tbl_invoice_due where qutation_no='" + qutationNo.ToString() + "'");

                }
                else
                {
                    DbCL.executeRdr("update tbl_invoice_due set Due_amount='" + d.ToString() + "' where qutation_no='" + qutationNo.ToString() + "'");
                }

            }
            DbCL.Conn.Close();


            return qno;
            
        }

        private string finddueamount(string invoiceNo)
        {
            string due = "0";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Due_amount from tbl_invoice_due where qutation_no='" + invoiceNo.ToString() + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                due = re["Due_amount"].ToString();
            }
            DbCL.Conn.Close();
            return due;
        }

        
    }
}