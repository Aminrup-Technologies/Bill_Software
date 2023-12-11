using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm89 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dtphasetype = new DataTable();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                Bindcombo();
                BindcomboVendor();
                // BindGrid();
                // bindphaseType();
            }
        }


        private void Bindcombo()
        {
            cmbclient.Items.Add("SELECT");
            DbCL.FillCombo10(cmbclient, "select Client_Name from tbl_Client order by Client_Name");
        }

        //private void bindphaseType()
        //{
        //    string str = "select PrimaryService from tbl_PrimaryServiceTerms order by id";
        //    dtphasetype = DbCL.SPreturn_dt(str, null);
        //    if (dtphasetype.Rows.Count > 0)
        //    {
        //        ListBox1.Items.Clear();
        //        for (int i = 0; i < dtphasetype.Rows.Count; i++)
        //        {
        //            ListBox1.Items.Add(dtphasetype.Rows[i]["PrimaryService"].ToString());
        //        }
        //    }
        //}

        //protected void Button1_Click(object sender, EventArgs e)
        //{
        //   // bindphaseType();
        //}

        protected void Button2_Click(object sender, EventArgs e)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            BuindCompanyId();
            string cmdstring = "";
            cmdstring = "select tbl_Client.Client_Name,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Net_amount,tbl_Quotation.sub_total,(tbl_Quotation.Net_Amount-tbl_Quotation.sub_total) as Gst,tbl_QuoPriSerTogather.PServiceName from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.Client_Id='" + lblclientId.Text + "'";
            Buinddatagrid(cmdstring);

            //BindclientName();

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
                //PanelError.Visible = true;
                //lblErrorMsg.Text = "No Data Found...";

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
            string cmdstring = "select Client_Id from tbl_Client where Client_Name='" + cmbclient.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblclientId.Text = re["Client_Id"].ToString();
            }
            DbCL.Conn.Close();
        }

        private void BindcomboVendor()
        {
            cmbvendor.Items.Add("SELECT");
            DbCL.FillCombo10(cmbvendor, "select Vendor_Name from tbl_Vendor order by Vendor_Name");
        }

        private void Binddetails(string Quotation_no)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Quotation where Quotation_no='" + Quotation_no.ToString() + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {

                lblQuotation_no.Text = re["Quotation_no"].ToString();
                lblQuotation_date.Text = re["Quotation_date"].ToString();
                amtwithoutgst.Text = re["sub_total"].ToString();
                //amountgst.Text = re["Gst"].ToString();
                amtwithgst.Text = re["Net_Amount"].ToString();

            }
            DbCL.Conn.Close();

        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Quotation_no = Convert.ToString(e.CommandArgument);
            if (e.CommandName == "Select")
            {
                Panel3.Visible = true;
                Binddetails(Quotation_no);

            }
        }

        protected void gst_TextChanged(object sender, EventArgs e)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Service_tax_No from tbl_Vendor where Vendor_Name='" + cmbvendor.Text + "' and gstornongst='With GST'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            //gst.Text = re["Service_tax_No"].ToString();
            if (re.Read())
            {
                gst.Text = re["Service_tax_No"].ToString();
                Panel4.Visible = true;

            }
            else
            {
                gst.Text = "Without GST Vendor";
                Panel5.Visible = true;
            }
            DbCL.Conn.Close();
        }


    }
}
    