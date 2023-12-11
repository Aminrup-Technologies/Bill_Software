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
    public partial class WebForm55 : System.Web.UI.Page
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
                string Id = Request.QueryString["Id"];
                Binddata(Id);

            }

        }
        private void Binddata(string Id)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Service where Id='" + Id + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblid.Text = re["Id"].ToString();
                lblproductid.Text = re["Service_code"].ToString();
                lblproductname.Text = re["Service_name"].ToString();
                txtPurchesRate.Text = re["Purches_rate"].ToString();
                txtSalerate.Text = re["Sail_rate"].ToString();
                cmbtax.Text = re["Tax_rate"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            DbCL.executeRdr("update tbl_Service set Purches_rate='" + txtPurchesRate.Text + "',Sail_rate='" + txtSalerate.Text + "',Tax_rate='" + cmbtax.Text + "' where Id='" + lblid.Text + "'");
            //DbCL.executeRdr("insert into tbl_Product(Product_code,Product_Name,Purches_Rate,Sail_Rate,Tax_Rate) values ('" + product_code.ToString() + "','" + txtProductsName.Text + "','" + txtPurchesRate.Text + "','" + txtSalerate.Text + "','" + cmbtax.Text + "')");
            PanelOK.Visible = true;
            lblOk.Text = "Data Update Successfully...";
            DbCL.Conn.Close();
            btnSave.Visible = false;

        }

        protected void btnedit_Click(object sender, EventArgs e)
        {
            txtPurchesRate.Enabled = true;
            txtSalerate.Enabled = true;
            cmbtax.Enabled = true;
            btnSave.Visible = true;
            btnedit.Visible = false;

        }

        protected void btnback_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/Service_master.aspx");
        }
    }
}