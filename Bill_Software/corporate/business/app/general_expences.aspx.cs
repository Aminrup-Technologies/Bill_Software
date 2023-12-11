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
    public partial class WebForm43 : System.Web.UI.Page
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
                txtpaymetdate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtcreditcardno.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtcashDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtdddate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtneftdate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                DbCL.FillCombo(cmbexpenceshead, "Select Expencess_Name from tbl_Expences order by ID");
                //DbCL.FillCombo(cmbcompany, "select (TieupCompanyID+'-'+TieupCompany_Name) as TieupCompany_Name from tbl_Tieup_Company order by TieupCompanyID");
            }
        }

        protected void RadioButtonList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (RadioButtonList1.SelectedIndex == 0)
            {
                First.Visible = true;
                Second.Visible = false;
                Third.Visible = false;
                Four.Visible = false;
            }
            else if (RadioButtonList1.SelectedIndex == 3)
            {
                First.Visible = false;
                Second.Visible = false;
                Third.Visible = true;
                Four.Visible = false;

            }
            else if (RadioButtonList1.SelectedIndex == 4)
            {
                First.Visible = false;
                Second.Visible = false;
                Third.Visible = false;
                Four.Visible = true;

            }
            else
            {
                First.Visible = false;
                Second.Visible = true;
                Third.Visible = false;
                Four.Visible = false;
            }
        }

        protected void btnsave_Click(object sender, EventArgs e)
        {
            string pament_id = findpaymentid();
            string dd_chk_no = "";
            string dd_chk_bank = "";
            string dd_date = "";
            string body = txtnaration.Text;
            if (RadioButtonList1.SelectedIndex == 0)
            {
                dd_chk_no = "";
                dd_chk_bank = "";
                dd_date = txtcashDate.Text;

            }
            else if (RadioButtonList1.SelectedIndex == 3)
            {
                dd_chk_no = txtneftnumber.Text;
                dd_chk_bank = txtbankname1.Text;
                dd_date = txtneftdate.Text;

            }
            else if (RadioButtonList1.SelectedIndex == 4)
            {
                dd_chk_no = txtcraditcard.Text;
                dd_chk_bank = txtcardholdername.Text;
                dd_date = txtcreditcardno.Text;

            }
            else
            {
                dd_chk_no = txtDDno.Text;
                dd_chk_bank = txtBankName.Text;
                dd_date = txtdddate.Text;

            }
            
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "insert into tlb_General_expences(pament_made_id,expencess_head,pament_made_date,amount,pament_made_to,pament_made_mode,chk_dd_no,drawn_bank,chk_date,naration,emp_id)values(@pament_made_id,@expencess_head,@pament_made_date,@amount,@pament_made_to,@pament_made_mode,@chk_dd_no,@drawn_bank,@chk_date,@naration,@emp_id)";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.Parameters.AddWithValue("@pament_made_id", pament_id);
            cmd.Parameters.AddWithValue("@expencess_head", cmbexpenceshead.Text);
            cmd.Parameters.AddWithValue("@pament_made_date", txtpaymetdate.Text);
            cmd.Parameters.AddWithValue("@amount", txtamount.Text);
            cmd.Parameters.AddWithValue("@pament_made_to", txtpaymentmadeto.Text);
            cmd.Parameters.AddWithValue("@pament_made_mode", RadioButtonList1.Text);
            cmd.Parameters.AddWithValue("@chk_dd_no", dd_chk_no);
            cmd.Parameters.AddWithValue("@drawn_bank", dd_chk_bank);
            cmd.Parameters.AddWithValue("@chk_date", dd_date);



            cmd.Parameters.AddWithValue("@naration", body);
            cmd.Parameters.AddWithValue("@emp_id", Session["USERID"].ToString());
            //cmd.Parameters.AddWithValue("@Tyeup_company", Colabaration.ToString());
            cmd.ExecuteNonQuery();
            DbCL.Conn.Close();

            PanelOK.Visible = true;
            lblOk.Text = "Data Save Successfully...";
            btnsave.Visible = false;
        }

        private string findpaymentid()
        {
            string id = "";
            string aa = "";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdString1 = "select pament_made_id from tlb_General_expences where id=(select max(id)from tlb_General_expences)";
            SqlCommand com1 = new SqlCommand(cmdString1, DbCL.Conn);
            SqlDataReader DR1 = com1.ExecuteReader();
            if (DR1.Read())
            {
                aa = DR1["pament_made_id"].ToString();
                string bb = aa.Substring(5);
                int k = Convert.ToInt32(bb);
                k = k + 1;
                string q = Convert.ToString(k);
                id = "PM000" + q;
            }
            else
            {
                id = "PM0001";
            }
            DbCL.Conn.Close();
            return id;
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/general_expences.aspx");
        }
    }
}