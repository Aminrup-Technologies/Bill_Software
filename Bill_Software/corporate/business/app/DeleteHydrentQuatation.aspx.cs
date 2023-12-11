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
    public partial class DeleteHydrentQuatation : System.Web.UI.Page
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
                DbCL.FillCombo(cmbvendor, "select Client_Name from tbl_Client order by Client_Name");
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            }
        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {

                cmdstring = "select b.id,a.Client_Name,b.Quotation_no,b.Q_date from tbl_Client as a inner join tbl_qsHydrentQuotation as b on a.Client_Id=b.ClientId  where a.Client_Name='" + cmbvendor.Text + "' order by cast(b.Q_date as datetime) desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                cmdstring = "select b.id,a.Client_Name,b.Quotation_no,b.Q_date from tbl_Client as a inner join tbl_qsHydrentQuotation as b on a.Client_Id=b.ClientId where cast(b.Q_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(b.Q_date as datetime) desc";
                Buinddatagrid(cmdstring);
            }
            else
            {
                cmdstring = "select b.id,a.Client_Name,b.Quotation_no,b.Q_date from tbl_Client as a inner join tbl_qsHydrentQuotation as b on a.Client_Id=b.ClientId where a.Client_Name='" + cmbvendor.Text + "' and cast(b.Q_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(b.Q_date as datetime) desc";
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
                DataList1.DataSource = re;
                DataList1.DataBind();
            }
            else
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "No Data Found...";

            }
            DbCL.Conn.Close();
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/DeleteHydrentQuatation.aspx");
        }


        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Quotation_no = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Delete")
            {
                DbCL.executeRdr("delete from tbl_qsHydrentQuotation where Quotation_no='" + Quotation_no + "'");
                DbCL.executeRdr("delete from tbl_qsHydrentDetails where Quotation_no='" + Quotation_no + "'");

                searchinv(Quotation_no);

                PanelOK.Visible = true;
                lblOk.Text = "Data Deleted Successfully...";
                DataList1.Visible = false;
            }
        }

        private void searchinv(string quotation_no)
        {
            string query = "select Invoice_No from tbl_HydrentInvoice where Quotation_No=@Quotation_No";
            SqlParameter[] pram = {  new SqlParameter("@Quotation_No", quotation_no) };
            SqlDataReader rdr= DbCL.SPReturnRdr(query, pram);
            if (rdr.Read())
            {
                DbCL.executeRdr("delete from tbl_HydrentInvoice where Quotation_No='" + quotation_no + "'");
            }

        }
    }
}