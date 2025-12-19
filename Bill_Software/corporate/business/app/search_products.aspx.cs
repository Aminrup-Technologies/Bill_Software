using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using System.Web.Script.Services;
using System.Configuration;

namespace Bill_Software.corporate.business.app
{
    public partial class search_products : System.Web.UI.Page
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
                //Products pre-loading in Page laod have been disabled initially and later only top 100 items are laoded
                Binddata();
            }

        }

        private void Binddata()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select top 100 * from tbl_NewProduct where ViewMode=1 and DeleteMode=0 order by Id desc";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
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
                BinddataByServiceCategory(pid);
            }
        }

        private void BinddataByServiceCategory(int ParentId)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_NewProduct where parentId=@parentId order by Id asc";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.Parameters.AddWithValue("@parentId", ParentId);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        [System.Web.Services.WebMethod]
        [System.Web.Script.Services.ScriptMethod]
        public static object GetProductDetails(int productId)
        {
            string cs = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

            using (SqlConnection con = new SqlConnection(cs))
            {
                string sql = @"
                    SELECT ProductName, Brand, ProductOrServiceCat,
                           Unit, Sail_Rate, Tax_Rate,
                           Specification
                    FROM tbl_NewProduct
                    WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@Id", productId);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    return new
                    {
                        ProductName = dr["ProductName"].ToString(),
                        Brand = dr["Brand"].ToString(),
                        Category = dr["ProductOrServiceCat"].ToString(),
                        Unit = dr["Unit"].ToString(),
                        Rate = dr["Sail_Rate"].ToString(),
                        GST = dr["Tax_Rate"].ToString(),
                        Spec = dr["Specification"].ToString(),
                        Image = "../../../Images/no_image.jpg",      // placeholder
                        OEMUrl = "#"      // placeholder
                    };

                }
            }
            return null;
        }

        [System.Web.Services.WebMethod]
        [System.Web.Script.Services.ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static List<object> SearchProducts(string keyword)
        {
            List<object> list = new List<object>();
            string cs = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

            using (SqlConnection con = new SqlConnection(cs))
            {
                string sql = @"
            SELECT Id, ProductName, Brand, Sail_Rate, Tax_Rate,
                NormalizedProductName, ProductOrServiceCat, Type
            FROM tbl_NewProduct
            WHERE ViewMode = 1
              AND DeleteMode = 0
              AND (
                    NormalizedProductName LIKE '%' + @kw + '%'
                 OR Brand LIKE '%' + @kw + '%'
                 OR Product_code LIKE '%' + @kw + '%'
                 OR NormalizedCategory LIKE '%' + @kw + '%'
              )
            ORDER BY ProductName";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@kw", keyword.ToLower());

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new
                    {
                        Id = dr["Id"],
                        ProductName = dr["ProductName"].ToString(),
                        Brand = dr["Brand"].ToString(),
                        Rate = dr["Sail_Rate"].ToString(),
                        GST = dr["Tax_Rate"].ToString(),
                        Category = dr["ProductOrServiceCat"].ToString(),
                        Type = dr["Type"].ToString()
                    });
                }
            }
            return list;
        }

    }
}