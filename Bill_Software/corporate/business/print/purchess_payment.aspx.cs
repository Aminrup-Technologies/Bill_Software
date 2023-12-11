using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.print
{
    public partial class purchess_payment : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        protected void Page_Load(object sender, EventArgs e)
        {
            string Payment_ID = Request.QueryString["Payment_ID"];

            lblpayment_id.Text = Payment_ID.ToString();
            buindalldata();
            Buindamount();
        }

        private void Buindamount()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select sl_no,(Product_name+' '+specification) as Product_name,Quantity,vendor_rate,tax_rate,purches_rate from tbl_purches_details where Purches_id='" + lblpurches_id.Text + "' order by Id";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        private void buindalldata()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Purchess_payment where Payment_ID='" + lblpayment_id.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblpurches_id.Text = re["Purchess_ID"].ToString();
                lblpurches_date.Text = re["Purchess_Date"].ToString();
                lblpayment_date.Text = re["Payment_Date"].ToString();
                string net_amount = re["Net_amount"].ToString();
               
                lblpurches_rate.Text = net_amount.ToString();
                lbldue.Text = re["Due_amount"].ToString();
                double payment_amount = Convert.ToDouble(lblpurches_rate.Text) - Convert.ToDouble(lbldue.Text);
                string payment_amount1 = payment_amount.ToString()+".00";
                lblPayment_amount.Text = payment_amount1.ToString();
                string clientid = re["Client_Id"].ToString();
                Bindclientdetails(clientid);
                string Iddetails = re["ID"].ToString();
                buindpaymentdetails(Iddetails);
            }


        }

        private void buindpaymentdetails(string Iddetails)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Given_Amount,type,Ch_no,Ch_bank,Ch_date from tbl_Purchess_payment where Purchess_ID='" + lblpurches_id.Text + "' and ID<=" + Iddetails + " order by ID";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList10.DataSource = cmd.ExecuteReader();
            DataList10.DataBind();
            DbCL.Conn.Close();
        }
        private void Bindclientdetails(string clientid)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Vendor where Vendor_Id='" + clientid + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblcompanyName.Text = re["Vendor_Name"].ToString();
                lbladdress1.Text = re["Address1"].ToString();
                lbladdress2.Text = re["Address2"].ToString();
                if (lbladdress2.Text == "")
                {
                    lbladdress2.Visible = false;
                }
                else
                {
                    lbladdress2.Visible = true;
                }
                lblcity.Text = re["City"].ToString();
                lblPin.Text = re["pin"].ToString();
                lblstate.Text = re["State"].ToString();
                //lblrepresentativeName.Text = re["Rep_Name"].ToString();
                //lblrepresentativedesignation.Text = re["Rep_Desig"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {

        }

        protected void Button2_Click(object sender, EventArgs e)
        {

        }
    }
}