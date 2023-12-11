using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm56 : System.Web.UI.Page
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


                txtpaymetdate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                DbCL.FillCombo(cmbexpenceshead, "Select Expencess_Name from tbl_Expences order by ID");
                Bindclosingbalance();
                //DbCL.FillCombo(cmbcompany, "select (TieupCompanyID+'-'+TieupCompany_Name) as TieupCompany_Name from tbl_Tieup_Company order by TieupCompanyID");
            }

        }
        private void Bindclosingbalance()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Closing_balance from tlb_closing_balance";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblclosingbalance.Text = re["Closing_balance"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void btnsave_Click(object sender, EventArgs e)
        {
            double first_amount = Convert.ToDouble(lblclosingbalance.Text);
            double second_amount = Convert.ToDouble(txtamount.Text);
            if (cmbcashstatus.SelectedIndex == 1)
            {
                if (second_amount <= first_amount)
                {
                    insertdata();
                    PanelOK.Visible = true;
                    lblOk.Text = "Data Save Successfully...";
                    btnsave.Visible = false;
                    PanelError.Visible = false;

                }
                else
                {
                    PanelOK.Visible = false;
                    lblErrorMsg.Text = "Closing Amount is Less than given amount....";
                    PanelError.Visible = true;


                }


            }
            else
            {
                insertdata();
                PanelOK.Visible = true;
                lblOk.Text = "Data Save Successfully...";
                btnsave.Visible = false;
                PanelError.Visible = false;


            }

        }


        private string findpaymentId()
        {
            string id = null;
            string aa = null;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdString1 = "select payment_id from tbl_patty_cash_expenses where id=(select max(id)from tbl_patty_cash_expenses)";
            SqlCommand com1 = new SqlCommand(cmdString1, DbCL.Conn);
            SqlDataReader DR1 = com1.ExecuteReader();
            if (DR1.Read())
            {
                aa = DR1["payment_id"].ToString();
                string bb = aa.Substring(4);
                int k = Convert.ToInt32(bb);
                k = k + 1;
                string q = Convert.ToString(k);
                id = "C000" + q;
            }
            else
            {
                id = "C0001";
            }
            DbCL.Conn.Close();
            return id;
        }

        private void insertdata()
        {
            string pament_id = findpaymentId();
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            double a = 0;
            string b = "";
            string type = "";
            if (cmbcashstatus.SelectedIndex == 0)
            {
                a = Convert.ToDouble(lblclosingbalance.Text) + Convert.ToDouble(txtamount.Text);
                b = a.ToString();
                type = "Credit";
            }
            else
            {
                a = Convert.ToDouble(lblclosingbalance.Text) - Convert.ToDouble(txtamount.Text);
                b = a.ToString();
                type = "Debit";
            }
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            string cmdstring = "insert into tbl_patty_cash_expenses(payment_id,cash_status,expences_head,payment_date,payment_made_to,payment_amount,payment_mode,naration,closing_balance,type_collum,emp_id)values(@payment_id,@cash_status,@expences_head,@payment_date,@payment_made_to,@payment_amount,@payment_mode,@naration,@closing_balance,@type_collum,@emp_id)";

            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.Parameters.AddWithValue("@payment_id", pament_id);
            cmd.Parameters.AddWithValue("@cash_status", cmbcashstatus.Text);
            cmd.Parameters.AddWithValue("@expences_head", cmbexpenceshead.Text);
            cmd.Parameters.AddWithValue("@payment_date", txtpaymetdate.Text);
            cmd.Parameters.AddWithValue("@payment_made_to", txtpaymentmadeto.Text);
            cmd.Parameters.AddWithValue("@payment_amount", txtamount.Text);
            cmd.Parameters.AddWithValue("@payment_mode", RadioButtonList1.Text);
            cmd.Parameters.AddWithValue("@naration", txtnaration.Text);
            cmd.Parameters.AddWithValue("@closing_balance", b.ToString());
            cmd.Parameters.AddWithValue("@type_collum", type.ToString());
            cmd.Parameters.AddWithValue("@emp_id", Session["USERID"].ToString());
            //cmd.Parameters.AddWithValue("@tiup_company", Colabaration.ToString());
            cmd.ExecuteNonQuery();
            DbCL.Conn.Close();
            DbCL.executeRdr("update tlb_closing_balance set Closing_balance='" + b.ToString() + "'");
            Datalist(pament_id);
        }

        public void Datalist(string pament_id)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select payment_id from tbl_patty_cash_expenses where payment_id='" + pament_id.ToString() + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList20.DataSource = cmd.ExecuteReader();
            DataList20.DataBind();
            DbCL.Conn.Close();

        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/patty_cash_expences.aspx");
        }
    }
}