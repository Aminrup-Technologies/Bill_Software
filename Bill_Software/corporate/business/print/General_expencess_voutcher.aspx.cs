using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;

namespace Bill_Software.corporate.business.print
{
    public partial class General_expencess_voutcher : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string pament_made_id = (Request.QueryString["pament_made_id"].ToString());
                biiltdata(pament_made_id);
                //buindfactorydetails();
            }
        }
        private void biiltdata(string pament_made_id)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tlb_General_expences where pament_made_id='" + pament_made_id.ToString() + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                //lblcash_catagory.Text = re["cash_status"].ToString();
                lblpayment_date.Text = re["pament_made_date"].ToString();
                lblpayment_made_to.Text = re["pament_made_to"].ToString();
                lblexpences_head.Text = re["expencess_head"].ToString();
                lblnaration_head.Text = re["naration"].ToString();
                lblpayment_type.Text = re["pament_made_mode"].ToString();
                if (lblpayment_type.Text == "Credit Card")
                {
                    lblinstumant.Text = "Card No";

                }
                else
                {
                    lblinstumant.Text = "Instrument No";
                }
                lblamount.Text = re["amount"].ToString();
                lblamount1.Text = re["amount"].ToString();
                lblinstument_no.Text = re["chk_dd_no"].ToString();
                lblinstument_date.Text = re["chk_date"].ToString();
                //lblemp_Id.Text = re["emp_id"].ToString();
                //string collarabrotion = re["Tyeup_company"].ToString();
                //Image21.ImageUrl = "~/corporate/business/app/Payment_heder.ashx?tiup_company=" + collarabrotion;
            }
            DbCL.Conn.Close();
            string word = MoneyConvDS.MoneyConvFn(lblamount1.Text);
            lblword.Text = word.ToString();
        }
    }
}