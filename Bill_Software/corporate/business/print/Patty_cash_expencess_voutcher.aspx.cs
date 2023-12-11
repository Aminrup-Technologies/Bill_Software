using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.print
{
    public partial class Patty_cash_expencess_voutcher : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string payment_id = (Request.QueryString["payment_id"].ToString());
                biiltdata(payment_id);
                //buindfactorydetails();
            }
        }

        private void biiltdata(string payment_id)
        {

            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_patty_cash_expenses where payment_id='" + payment_id.ToString() + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblcash_catagory.Text = re["cash_status"].ToString();
                lblpayment_date.Text = re["payment_date"].ToString();
                lblpayment_made_to.Text = re["payment_made_to"].ToString();
                lblexpences_head.Text = re["expences_head"].ToString();
                lblnaration_head.Text = re["naration"].ToString();
                lblpayment_type.Text = re["payment_mode"].ToString();
                lblamount.Text = re["payment_amount"].ToString();
                lblamount1.Text = re["payment_amount"].ToString();
                //string collarabrotion = re["tiup_company"].ToString();
                //Image21.ImageUrl = "~/corporate/business/app/Payment_heder.ashx?tiup_company=" + collarabrotion;
                //lblinstument_no.Text = re["payment_amount_number"].ToString();
                //lblinstument_date.Text = re["payment_amount_date"].ToString();
                //lblemp_Id.Text = (re["emp_id"].ToString()).ToUpper();
            }
            DbCL.Conn.Close();
            string word = MoneyConvDS.MoneyConvFn(lblamount1.Text);
            lblword.Text = word.ToString();
        }
    }
}