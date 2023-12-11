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
    public partial class productparent : System.Web.UI.Page
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
                Binddata();
            }
        }

        private void Binddata()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select id,Product_Name,Product_Type,ProductCode,gstRate from tbl_parentProduct";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (txtParentProducts.Text!="" && txtproductCode.Text!="")
            {
                string query = "insert into tbl_parentProduct(Product_Name,Product_Type,ProductCode,gstRate) values (@Product_Name,@Product_Type,@ProductCode,@gstRate)";
                SqlParameter[] pram = {
                                          new SqlParameter("@Product_Name",txtParentProducts.Text),
                                          new SqlParameter("@Product_Type",cmbType.Text),
                                          new SqlParameter("@ProductCode",txtproductCode.Text),
                                          new SqlParameter("@gstRate",cmbtax.Text),
                                          
                                      };
                DbCL.SPExecDB(query, pram);
                PanelOK.Visible = true;
                lblOk.Text = "Data Save Successfully...";
                Binddata();
            }
           
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Id = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Delete")
            {
                DbCL.executeRdr("delete from tbl_parentProduct where id='" + Id + "'");
                PanelOK.Visible = true;
                lblOk.Text = "Data Deleted Successfully...";
            }
            //else if (e.CommandName == "Edit")
            //{
            //    Response.Redirect("Update_product.aspx?Id=" + Id);
            //}
            Binddata();
        }
    }
}