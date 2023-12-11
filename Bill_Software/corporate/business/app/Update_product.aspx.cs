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
    public partial class WebForm54 : System.Web.UI.Page
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
                DbCL.FillCombo(cmbtax, "select Vat_Rate from tbl_Vat_Master order by ID");
                string Id = Request.QueryString["Id"];
                Binddata(Id);
            }

        }

        private void Binddata(string Id)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Product where Id='" + Id + "'";
            SqlCommand cmd = new SqlCommand(cmdstring,DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if(re.Read())
            {
                lblid.Text = re["Id"].ToString();
                lblproductid.Text = re["Product_code"].ToString();
                lblproductname.Text = re["Product_Name"].ToString();
                txtSubProdName.Text = re["Sub_Prod_Name"].ToString();
                txtSalerate.Text = re["Sail_Rate"].ToString();
                cmbtax.Text = re["Tax_Rate"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            DbCL.executeRdr("update tbl_Product set Sub_Prod_Name='" + txtSubProdName.Text + "',Sail_Rate='" + txtSalerate.Text + "',Tax_Rate='"+ cmbtax.Text +"' where Id='"+ lblid.Text +"'");
            //DbCL.executeRdr("insert into tbl_Product(Product_code,Product_Name,Purches_Rate,Sail_Rate,Tax_Rate) values ('" + product_code.ToString() + "','" + txtProductsName.Text + "','" + txtPurchesRate.Text + "','" + txtSalerate.Text + "','" + cmbtax.Text + "')");
            PanelOK.Visible = true;
            lblOk.Text = "Data Update Successfully...";
            DbCL.Conn.Close();
            btnSave.Visible = false;
            
        }

        protected void btnedit_Click(object sender, EventArgs e)
        {
            //txtPurchesRate.Enabled = true;
            txtSalerate.Enabled = true;
            cmbtax.Enabled = true;
            btnSave.Visible = true;
            btnedit.Visible = false;
        }

        protected void btnback_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/product_master.aspx");
        }
    }
}