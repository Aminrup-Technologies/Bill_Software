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
    public partial class Deletehydrent_invoice : System.Web.UI.Page
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

                cmdstring = "select a.ID, a.Invoice_No, a.Invoice_Date, a.Quotation_No, a.Quotation_Date, a.Net_amount, a.DiscountAmount, b.Client_Name from tbl_HydrentInvoice as a inner join tbl_Client as b on a.Client_ID = b.Client_Id where b.Client_Name='" + cmbvendor.Text + "' order by cast(a.Invoice_Date as datetime) desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                cmdstring = "select a.ID, a.Invoice_No, a.Invoice_Date, a.Quotation_No, a.Quotation_Date, a.Net_amount, a.DiscountAmount, b.Client_Name from tbl_HydrentInvoice as a inner join tbl_Client as b on a.Client_ID = b.Client_Id where cast(a.Invoice_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(a.Invoice_Date as datetime) desc";
                Buinddatagrid(cmdstring);
            }
            else
            {
                cmdstring = "select a.ID, a.Invoice_No, a.Invoice_Date, a.Quotation_No, a.Quotation_Date, a.Net_amount, a.DiscountAmount, b.Client_Name from tbl_HydrentInvoice as a inner join tbl_Client as b on a.Client_ID = b.Client_Id where b.Client_Name='" + cmbvendor.Text + "' and cast(a.Invoice_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(a.Invoice_Date as datetime) desc";
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
            Response.Redirect("~/corporate/business/app/Deletehydrent_invoice.aspx");
        }


        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Invoice_No = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Delete")
            {

                updatestock1(Invoice_No);
                DbCL.executeRdr("delete from tbl_HydrentInvoice where Invoice_No='" + Invoice_No + "'");

                PanelOK.Visible = true;
                lblOk.Text = "Data Deleted Successfully...";
                DataList1.Visible = false;
            }
        }

        private void updatestock1(string Invoice_No)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Quotation_No from tbl_HydrentInvoice where Invoice_No='" + Invoice_No + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                string quotation_no = re["Quotation_No"].ToString();
                DbCL.executeRdr("update tbl_qsHydrentQuotation set invStatus='No' where  Quotation_no='" + quotation_no.ToString() + "'");
                
            }
            DbCL.Conn.Close();
        }
    }

}