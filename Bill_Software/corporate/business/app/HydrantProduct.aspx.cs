using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app
{
    public partial class HydrantProduct : System.Web.UI.Page
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
                string qu = "select Vat_Rate from tbl_Vat_Master order by ID";

                DbCL.Sppopulate_Combo(qu, null, ddltaxrate);

                Binddata();
            }
        }

        private void Binddata()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_HydrantProduct";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string product_code = Findproductcode();
            if (ddltaxrate.SelectedIndex != -1)
            {
                string query = "insert into tbl_HydrantProduct(Product_code,Product_Name,short_form,Tax_Rate,base_rate) values (@Product_code,@Product_Name,@short_form,@Tax_Rate,@base_rate)";
                SqlParameter[] pram = {

                    new SqlParameter("@Product_code",product_code),
                    new SqlParameter("@Product_Name",txtProductsName.Text),
                    new SqlParameter("@short_form",txtShortFrom.Text),
                    new SqlParameter("@Tax_Rate",ddltaxrate.Text),
                    new SqlParameter("@base_rate",txtBaseRate.Text)

                };

                DbCL.SPExecDB(query, pram);
                Binddata();
                //
                //DbCL.Sqlconnection();
                //DbCL.ConnectDb();
                //DbCL.executeRdr("insert into tbl_HydrantProduct(Product_code,Product_Name,short_form,Tax_Rate,base_rate) values ('" + product_code.ToString() + "','" + txtProductsName.Text + "','" + txtShortFrom.Text + "','" + cmbtax.Text + "','" + txtBaseRate.Text + "')");
                //PanelOK.Visible = true;
                //lblOk.Text = "Data Save Successfully...";
                //DbCL.Conn.Close();
                //txtProductsName.Text = "";
                //txtShortFrom.Text = "";
                //Binddata();
            }
        }

        private string Findproductcode()
        {
            string ProId = "";
            string aa = "";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdString1 = "select Id,Product_code from tbl_HydrantProduct where Id=(select max(Id)from tbl_HydrantProduct)";
            SqlCommand com1 = new SqlCommand(cmdString1, DbCL.Conn);
            SqlDataReader DR1 = com1.ExecuteReader();
            if (DR1.Read())
            {
                aa = DR1.GetValue(1).ToString();
                string bb = aa.Substring(5);
                int k = Convert.ToInt32(bb);
                k = k + 1;
                string q = Convert.ToString(k);
                ProId = "PRD00" + q;
            }
            else
            {
                ProId = "PRD001";
            }

            DbCL.Conn.Close();
            return ProId;
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Id = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Delete")
            {
                DbCL.executeRdr("delete from tbl_HydrantProduct where Id='" + Id + "'");
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