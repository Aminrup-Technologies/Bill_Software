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
    public partial class WebForm69 : System.Web.UI.Page
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
                DbCL.FillCombo(cmdProduct, "select ProductOrServiceCat from tbl_NewparentProduct order by id");
                DbCL.FillCombo(cmbtax, "select Vat_Rate from tbl_New_Vat_Master order by ID");
                Binddata();
            }

        }

        private void Binddata()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Id,Product_code,ProductOrServiceCat,Sail_Rate,Tax_Rate,Type,ProductName,Unit,Brand,parentId from tbl_NewProduct order by ProductOrServiceCat asc";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (txtProductCode.Text != "")
            {
                //string product_code = Session["ProductCode"].ToString();
                //string gstRate = Session["gstRate"].ToString();
                DbCL.Sqlconnection();
                DbCL.ConnectDb();
                DbCL.executeRdr("insert into tbl_NewProduct(Product_code,ProductOrServiceCat,Sail_Rate,Tax_Rate,ProductName,Type,Unit,Brand,parentId) values ('" + txtProductCode.Text.ToString() + "','" + cmdProduct.Text + "','" + txtSalerate.Text + "','" + cmbtax.Text.ToString() + "','" + txtSubProductsName.Text + "','" + ddlProOrSer.Text + "','" + txtUnit.Text + "','" + txtBrand.Text + "','" + Convert.ToInt32(Session["pid"]) + "')");
                PanelOK.Visible = true;
                lblOk.Text = "Data Save Successfully...";
                DbCL.Conn.Close();
            }
            Binddata();

        }
     
        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Id = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Delete")
            {
                DbCL.executeRdr("delete from tbl_NewProduct where Id='" + Id + "'");
                PanelOK.Visible = true;
                lblOk.Text = "Data Deleted Successfully...";
            }
            else if (e.CommandName == "Edit")
            {
                Response.Redirect("NewUpdate_product.aspx?Id=" + Id);
            }
            Binddata();
        }

        protected void cmdProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            string query = "select id from tbl_NewparentProduct where ProductOrServiceCat=@ProductOrServiceCat";
            SqlParameter[] pram = {
                new SqlParameter("@ProductOrServiceCat",cmdProduct.Text)
            };
            DataTable dt = new DataTable();
            dt = DbCL.SPreturn_dt(query, pram);
            if (dt.Rows.Count > 0)
            {
                int pid = Convert.ToInt32(dt.Rows[0]["id"]);
                //string ProductCode = dt.Rows[0]["ProductCode"].ToString();
                //string gstRate = dt.Rows[0]["gstRate"].ToString();
                //Session["ProductCode"] = ProductCode;
                //Session["gstRate"] = gstRate;
                Session["pid"] = pid;
            }
        }
    }
}