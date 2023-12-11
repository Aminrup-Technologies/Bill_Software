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
    public partial class WebForm6 : System.Web.UI.Page
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
                DbCL.FillCombo(cmdProduct, "select Product_Name from tbl_parentProduct order by id");
                Binddata();
            }
        }
        private void Binddata()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Product";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            //string product_code = Findproductcode();
            if (Session["ProductCode"]!=null && Session["gstRate"] !=null)
            {
                string product_code = Session["ProductCode"].ToString();
                string gstRate = Session["gstRate"].ToString();

                DbCL.Sqlconnection();
                DbCL.ConnectDb();
                DbCL.executeRdr("insert into tbl_Product(Product_code,Product_Name,Sail_Rate,Tax_Rate,Sub_Prod_Name,parentId) values ('" + product_code.ToString() + "','" + cmdProduct.Text + "','" + txtSalerate.Text + "','" + gstRate + "','" + txtSubProductsName.Text + "','" + Convert.ToInt32(Session["pid"]) + "')");
                PanelOK.Visible = true;
                lblOk.Text = "Data Save Successfully...";
                DbCL.Conn.Close();
            }
            //txtProductsName.Text = "";
            //txtPurchesRate.Text = "";
            txtSalerate.Text = "";
            //cmbtax.SelectedIndex = 0;
            Binddata();

        }
        //private string Findproductcode()
        //{
        //    string ProId = "";
        //    string aa = "";
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    string cmdString1 = "select Id,Product_code from tbl_Product where Id=(select max(Id)from tbl_Product)";
        //    SqlCommand com1 = new SqlCommand(cmdString1, DbCL.Conn);
        //    SqlDataReader DR1 = com1.ExecuteReader();
        //    if (DR1.Read())
        //    {
        //        aa = DR1.GetValue(1).ToString();
        //        string bb = aa.Substring(5);
        //        int k = Convert.ToInt32(bb);
        //        k = k + 1;
        //        string q = Convert.ToString(k);
        //        ProId = "PRO00" + q;
        //    }
        //    else
        //    {
        //        ProId = "PRO001";
        //    }

        //    DbCL.Conn.Close();
        //    return ProId;
        //}

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Id = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Delete")
            {
                DbCL.executeRdr("delete from tbl_Product where Id='" + Id + "'");
                PanelOK.Visible = true;
                lblOk.Text = "Data Deleted Successfully...";
            }
            else if (e.CommandName == "Edit")
            {
                Response.Redirect("Update_product.aspx?Id=" + Id);
            }
            Binddata();
        }

        protected void cmdProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            string query = "select id,ProductCode,gstRate from tbl_parentProduct where Product_Name=@Product_Name";
            SqlParameter[] pram = {
                new SqlParameter("@Product_Name",cmdProduct.Text)
            };
            DataTable dt = new DataTable();
            dt = DbCL.SPreturn_dt(query, pram);
            if (dt.Rows.Count>0)
            {
                int pid =Convert.ToInt32(dt.Rows[0]["id"]);
                string ProductCode = dt.Rows[0]["ProductCode"].ToString();
                string gstRate = dt.Rows[0]["gstRate"].ToString();
                Session["ProductCode"] = ProductCode;
                Session["gstRate"] = gstRate;
                Session["pid"] = pid;
            }
        }
    }
}