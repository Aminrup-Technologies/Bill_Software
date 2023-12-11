using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm58 : System.Web.UI.Page
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

                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                //DbCL.FillCombo(cmbcompany, "select (TieupCompanyID+'-'+TieupCompany_Name) as TieupCompany_Name from tbl_Tieup_Company order by TieupCompanyID");
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
                PanelOK.Visible = false;

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
            Response.Redirect("~/corporate/business/app/Delete_patty_cash_expenses.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string payment_id = Convert.ToString(e.CommandArgument);



            if (e.CommandName == "Delete")
            {
                string id1 = "";
                string cash_status = "";
                string payment_amount = "";
                DbCL.Sqlconnection();
                DbCL.ConnectDb();
                string cmdstring = "select id,cash_status,payment_amount from tbl_patty_cash_expenses where payment_id='" + payment_id + "'";
                SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
                SqlDataReader re = cmd.ExecuteReader();
                if (re.Read())
                {
                    id1 = re["id"].ToString();
                    cash_status = re["cash_status"].ToString();
                    payment_amount = re["payment_amount"].ToString();
                }
                DbCL.Conn.Close();
                string closing_balance = Findclosingbalance();
                double a = 0;
                string b = "";
                if (cash_status == "Cash In")
                {
                    a = Convert.ToDouble(closing_balance) - Convert.ToDouble(payment_amount);
                    b = a.ToString();
                    DbCL.executeRdr("update tbl_patty_cash_expenses set closing_balance=(cast(closing_balance as int)-'" + payment_amount.ToString() + "') where id>'" + id1.ToString() + "'");
                }
                else
                {
                    a = Convert.ToDouble(closing_balance) + Convert.ToDouble(payment_amount);
                    b = a.ToString();
                    DbCL.executeRdr("update tbl_patty_cash_expenses set closing_balance=(cast(closing_balance as int)+'" + payment_amount.ToString() + "') where id>'" + id1.ToString() + "'");
                }
                DbCL.executeRdr("update tlb_closing_balance set Closing_balance='" + b.ToString() + "'");
                DbCL.executeRdr("delete from tbl_patty_cash_expenses where payment_id='" + payment_id.ToString() + "'");
                DataList1.Visible = false;
                PanelOK.Visible = true;
                lblOk.Text = "Data Deleted Successfully...";

            }
        }

        private string Findclosingbalance()
        {
            string closing_balance = "";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring1 = "select Closing_balance from tlb_closing_balance";
            SqlCommand cmd = new SqlCommand(cmdstring1, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                closing_balance = re["Closing_balance"].ToString();
            }
            DbCL.Conn.Close();
            return closing_balance;
        }
    }
}