using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm57 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/corporate/CustomError/CustomError.aspx");
            }
            if (!IsPostBack)
            {
                //DbCL.FillCombo(cmbcompany, "select (TieupCompanyID+'-'+TieupCompany_Name) as TieupCompany_Name from tbl_Tieup_Company order by TieupCompanyID");
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");

            }
        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {

                cmdstring = "SELECT payment_id,cash_status,expences_head,payment_date,payment_made_to,payment_amount,payment_mode,emp_id FROM tbl_patty_cash_expenses where cast(payment_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by id";
                Buinddatagrid(cmdstring);
            }
            else
            {

                cmdstring = "SELECT payment_id,cash_status,expences_head,payment_date,payment_made_to,payment_amount,payment_mode,emp_id FROM tbl_patty_cash_expenses where cash_status='" + cmbcashstatus.Text + "' and cast(payment_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by id";
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
                lblErrorMsg.Text = "No Data Found..";

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

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/view_patty_cash_expenses.aspx");
        }
    }
}