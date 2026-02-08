using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Services;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm50 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Session guard only
            if (HttpContext.Current.Session["USERID"] == null)
            {
                HttpContext.Current.Response.Redirect("~/index.aspx");
            }
        }

        // 🔍 Product Search (uses NormalizedProductName inside SP)
        [WebMethod]
        public static List<ProductResult> SearchProducts(string search)
        {
            List<ProductResult> list = new List<ProductResult>();

            if (string.IsNullOrWhiteSpace(search))
                return list;

            DB_UTILITY db = new DB_UTILITY();
            db.Sqlconnection();
            db.ConnectDb();

            SqlCommand cmd = new SqlCommand("sp_SearchProductsFast", db.Conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Search", search);

            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new ProductResult
                {
                    ProductID = dr["ProductID"].ToString(),
                    ProductName = dr["ProductName"].ToString(),
                    CategoryName = dr["CategoryName"].ToString()
                });
            }

            db.Conn.Close();
            return list;
        }

        // 📦 Store-wise stock (lazy load)
        [WebMethod]
        public static List<StoreStock> GetStock(string productId)
        {
            List<StoreStock> list = new List<StoreStock>();

            if (string.IsNullOrWhiteSpace(productId))
                return list;

            DB_UTILITY db = new DB_UTILITY();
            db.Sqlconnection();
            db.ConnectDb();

            SqlCommand cmd = new SqlCommand("sp_GetProductStockByStore", db.Conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ProductID", productId);

            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new StoreStock
                {
                    StoreName = dr["ShippedToStoreName"].ToString(),
                    StockQty = dr["StockQty"].ToString()
                });
            }

            db.Conn.Close();
            return list;
        }
    }

    // DTOs
    public class ProductResult
    {
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
    }

    public class StoreStock
    {
        public string StoreName { get; set; }
        public string StockQty { get; set; }
    }
}
