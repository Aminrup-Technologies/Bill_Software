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
    public partial class purches_bill : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        protected void Page_Load(object sender, EventArgs e)
        {
            string Purches_Id = Request.QueryString["Purches_Id"];

            lblpurches_id.Text = Purches_Id.ToString();
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
            string cmdstring = "select * from tbl_Purches where Purches_Id='" + lblpurches_id.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblpurches_date.Text = re["Purches_date"].ToString();
                lblpurches_rate.Text = re["Total_purches_rate"].ToString();
                lblsail_rate.Text = re["Total_Tax_rate"].ToString();
                string type = re["Purches_Type"].ToString();
                if (type == "Product")
                {
                    //labeltax1.Text = "Vat";
                    labeltax1.Text = "GST";
                }
                else
                {
                    //labeltax1.Text = "Service Tax";
                    labeltax1.Text = "GST";
                }
                string clientid = re["Client_Id"].ToString();
                Bindclientdetails(clientid);
                

            }
            

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