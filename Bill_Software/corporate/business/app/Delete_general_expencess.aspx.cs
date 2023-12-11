using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm45 : System.Web.UI.Page
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
                DbCL.FillCombo(cmbexpenceshead, "Select Expencess_Name from tbl_Expences order by ID");
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

                cmdstring = "SELECT pament_made_id,expencess_head,pament_made_date,pament_made_to,amount,pament_made_mode,chk_dd_no,emp_id FROM tlb_General_expences where cast(pament_made_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "'  order by cast(pament_made_date as datetime) desc";
                Buinddatagrid(cmdstring);
            }
            else
            {

                cmdstring = "SELECT pament_made_id,expencess_head,pament_made_date,pament_made_to,amount,pament_made_mode,chk_dd_no,emp_id FROM tlb_General_expences where expencess_head='" + cmbexpenceshead.Text + "' and cast(pament_made_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "'  order by cast(pament_made_date as datetime) desc";
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
            Response.Redirect("~/corporate/business/app/Delete_general_expencess.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string pament_made_id = Convert.ToString(e.CommandArgument);



            if (e.CommandName == "Delete")
            {
                DbCL.executeRdr("delete from tlb_General_expences  where pament_made_id='" + pament_made_id.ToString() + "'");
                DataList1.Visible = false;
                PanelOK.Visible = true;
                lblOk.Text = "Data Deleted Successfully...";

            }
        }
    }
}