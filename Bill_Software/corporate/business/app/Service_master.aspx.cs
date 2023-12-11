using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm7 : System.Web.UI.Page
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
                DbCL.FillCombo(cmbtax, "select Service_tax from tbl_Service_master order by ID");
                Binddata();

            }

        }
        private void Binddata()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Service";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string service_code = Findservicecode();
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            DbCL.executeRdr("insert into tbl_Service(Service_code,Service_name,Sail_rate,Tax_rate) values ('" + service_code.ToString() + "','" + txtServiceName.Text + "','"+ txtSalerate.Text +"','"+ cmbtax.Text +"')");
            PanelOK.Visible = true;
            lblOk.Text = "Data Save Successfully...";
            DbCL.Conn.Close();
            txtServiceName.Text = "";
            //txtPurchesRate.Text = "";
            txtSalerate.Text = "";
            cmbtax.SelectedIndex = 0;
            Binddata();

        }
        private string Findservicecode()
        {
            string serviceId = "";
            string aa = "";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdString1 = "select Id,Service_code from tbl_Service where Id=(select max(Id)from tbl_Service)";
            SqlCommand com1 = new SqlCommand(cmdString1, DbCL.Conn);
            SqlDataReader DR1 = com1.ExecuteReader();
            if (DR1.Read())
            {
                aa = DR1.GetValue(1).ToString();
                string bb = aa.Substring(5);
                int k = Convert.ToInt32(bb);
                k = k + 1;
                string q = Convert.ToString(k);
                serviceId = "SER00" + q;
            }
            else
            {
                serviceId = "SER001";
            }

            DbCL.Conn.Close();
            return serviceId;
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Id = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Delete")
            {
                DbCL.executeRdr("delete from tbl_Service where Id='" + Id + "'");
                PanelOK.Visible = true;
                lblOk.Text = "Data Deleted Successfully...";
            }
            else if (e.CommandName == "Edit")
            {
                Response.Redirect("Service_update.aspx?Id=" + Id);

            }

            Binddata();

        }
    }
}