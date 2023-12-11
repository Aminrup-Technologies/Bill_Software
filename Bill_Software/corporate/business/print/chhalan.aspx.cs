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
    public partial class chhalan : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        protected void Page_Load(object sender, EventArgs e)
        {
            string Chalan_No = Request.QueryString["Chalan_No"];

            lblchallan_no.Text = Chalan_No.ToString();
            buindalldata();
            Buindamount();

        }
        private void Buindamount()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Challan_details where Challan_no='" + lblchallan_no.Text + "' order by Id";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        private void buindalldata()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Chalan where Chalan_No='" + lblchallan_no.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblchallan_date.Text = re["Chalan_Date"].ToString();
                lblQno.Text = re["Quotation_No"].ToString();
                lblQdate.Text = re["Quotation_Date"].ToString();
                string clientid = re["Client_ID"].ToString();
                string addressfor = re["addressfor"].ToString();
                Bindclientdetails(clientid, addressfor);
            }
            

        }

        private void Bindclientdetails(string clientid,string addressfor)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Name,Address1,Address2,City,pin,State,Rep_Name,Rep_Desig from tbl_Client where Client_Id='" + clientid + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblcompanyName.Text = re["Client_Name"].ToString();

                if (addressfor == "Corporate office")
                {
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

                }
                else
                {
                    Bindaddress(clientid, addressfor);
                }


                
                //lblrepresentativeName.Text = re["Rep_Name"].ToString();
                //lblrepresentativedesignation.Text = re["Rep_Desig"].ToString();
            }
            DbCL.Conn.Close();
        }

        private void Bindaddress(string clientid, string addressfor)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Address1,Address2,city,State,pin from tbl_Factory where Client_id='" + clientid.ToString() + "' and Factory_name='" + addressfor.ToString() + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {

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
                lblcity.Text = re["city"].ToString();
                lblPin.Text = re["pin"].ToString();
                lblstate.Text = re["State"].ToString();


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