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
    public partial class WebForm22 : System.Web.UI.Page
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
                DbCL.FillCombo(cmbvendor, "select Vendor_Name from tbl_Vendor order by Vendor_Name");
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");

            }

        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                BuindCompanyId();
                cmdstring = "select tbl_Purches.Purches_Id,tbl_Purches.Purches_date,tbl_Purches.Total_purches_rate,tbl_Purches.Total_Tax_rate,tbl_Vendor.Vendor_Name from tbl_Purches inner join tbl_Vendor on tbl_Purches.Client_Id=tbl_Vendor.Vendor_Id where tbl_Purches.Client_Id='" + lblclientId.Text + "' order by cast(tbl_Purches.Purches_date as datetime) desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                cmdstring = "select tbl_Purches.Purches_Id,tbl_Purches.Purches_date,tbl_Purches.Total_purches_rate,tbl_Purches.Total_Tax_rate,tbl_Vendor.Vendor_Name from tbl_Purches inner join tbl_Vendor on tbl_Purches.Client_Id=tbl_Vendor.Vendor_Id where cast(tbl_Purches.Purches_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(tbl_Purches.Purches_date as datetime) desc";
                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                cmdstring = "select tbl_Purches.Purches_Id,tbl_Purches.Purches_date,tbl_Purches.Total_purches_rate,tbl_Purches.Total_Tax_rate,tbl_Vendor.Vendor_Name from tbl_Purches inner join tbl_Vendor on tbl_Purches.Client_Id=tbl_Vendor.Vendor_Id where tbl_Purches.Client_Id='" + lblclientId.Text + "' and cast(tbl_Purches.Purches_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(tbl_Purches.Purches_date as datetime) desc";
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
                lblErrorMsg.Text = "No Data Found...";

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

        private void BuindCompanyId()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Vendor_Id from tbl_Vendor where Vendor_Name='" + cmbvendor.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblclientId.Text = re["Vendor_Id"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/Delete_purtches.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Purches_Id = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Delete")
            {
                updatestock(Purches_Id);
                DbCL.executeRdr("delete from tbl_purches_details where Purches_id='" + Purches_Id + "'");
                DbCL.executeRdr("delete from tbl_Purches where Purches_Id='" + Purches_Id + "'");
                DbCL.executeRdr("delete from tbl_purches_due where Purches_Id='" + Purches_Id + "'");
                PanelOK.Visible = true;
                lblOk.Text = "Data Deleted Successfully...";
                DataList1.Visible = false;
            }
        }

        private void updatestock(string Purches_Id)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Product_id,Product_name,Quantity from tbl_purches_details where Purches_id='" + Purches_Id + "'";
            SqlCommand cmd = new SqlCommand(cmdstring,DbCL.Conn);
            SqlDataReader re=cmd.ExecuteReader();
            while(re.Read())
            {
                string product_code = re["Product_id"].ToString();
                string Product_name = re["Product_name"].ToString();
                string Quantity = re["Quantity"].ToString();
                updatestock1(product_code, Product_name, Quantity);
            }
            DbCL.Conn.Close();
        }

        private void updatestock1(string product_code, string Product_name, string Quantity)
        {
            DbCL.executeRdr("update tbl_stock set Quantity=(cast(Quantity as int)-'" + Quantity.ToString() + "') where Product_id='" + product_code.ToString() + "' and Product_name='" + Product_name.ToString() + "'");
        }
    }
}