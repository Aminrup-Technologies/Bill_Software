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
    public partial class WebForm70 : System.Web.UI.Page
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
                DbCL.FillCombo(cmbtax, "select Vat_Rate from tbl_New_Vat_Master order by ID");
                string Id = Request.QueryString["Id"];
                Binddata(Id);
            }

        }

        private void Binddata(string Id)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Id,Product_code,ProductOrServiceCat,Sail_Rate,Tax_Rate,ProductName,Type,Unit,Brand,parentId from tbl_NewProduct where Id='" + Id + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblid.Text = re["Id"].ToString();
                txtProductCode.Text = re["Product_code"].ToString();
                lblproductname.Text = re["ProductOrServiceCat"].ToString();
                txtSubProdName.Text = re["ProductName"].ToString();
                txtBrand.Text= re["Brand"].ToString();
                ddlProOrSer.Text= re["Type"].ToString();
                txtUnit.Text= re["Unit"].ToString();
                txtSalerate.Text = re["Sail_Rate"].ToString();
                cmbtax.Text = re["Tax_Rate"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            DbCL.executeRdr("update tbl_NewProduct set Product_code='"+ txtProductCode.Text + "', ProductName='" + txtSubProdName.Text + "',Sail_Rate='" + txtSalerate.Text + "',Tax_Rate='" + cmbtax.Text + "',Brand='" + txtBrand.Text + "',Type='" + ddlProOrSer.Text + "',Unit='" + txtUnit.Text + "' where Id='" + lblid.Text + "'");
            PanelOK.Visible = true;
            lblOk.Text = "Data Update Successfully...";
            DbCL.Conn.Close();
            btnSave.Visible = false;

        }

        protected void btnedit_Click(object sender, EventArgs e)
        {

            txtProductCode.Enabled = true;
            lblproductname.Enabled = true;
            txtSubProdName.Enabled = true;
            txtBrand.Enabled = true;
            ddlProOrSer.Enabled = true;
            txtUnit.Enabled = true;
            txtSalerate.Enabled = true;
            cmbtax.Enabled = true;


            btnSave.Visible = true;
            btnedit.Visible = false;
        }

        protected void btnback_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/newproduct_master.aspx");
        }
    }
}