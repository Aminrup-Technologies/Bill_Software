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
        public static List<ProductResult> SearchProducts(string search, string category)
        {
            List<ProductResult> list = new List<ProductResult>();

            // We removed the "string.IsNullOrWhiteSpace" check so "Show All" works

            DB_UTILITY db = new DB_UTILITY();
            db.Sqlconnection();
            db.ConnectDb();

            try
            {
                SqlCommand cmd = new SqlCommand("sp_SearchProductsFast", db.Conn);
                cmd.CommandType = CommandType.StoredProcedure;

                // Pass both parameters to the Stored Procedure
                cmd.Parameters.AddWithValue("@Search", search ?? "");
                cmd.Parameters.AddWithValue("@Category", category ?? "");

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
            }
            catch (Exception ex)
            {
                // Log error if needed
            }
            finally
            {
                db.Conn.Close();
            }

            return list;
        }

        // 📦 Store-wise stock (lazy load)
        [WebMethod]
        public static List<StoreStock> GetStock(string productId)
        {
            List<StoreStock> list = new List<StoreStock>();
            if (string.IsNullOrWhiteSpace(productId)) return list;

            try
            {
                DB_UTILITY db = new DB_UTILITY();
                // Ensure db.Sqlconnection() and ConnectDb() are inside the using if possible, 
                // or ensure you close the connection in a finally block.
                db.Sqlconnection();
                db.ConnectDb();

                using (SqlCommand cmd = new SqlCommand("sp_GetProductStockByStore", db.Conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProductID", productId);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new StoreStock
                            {
                                StoreName = dr["ShippedToStoreName"].ToString(),
                                StockQty = dr["StockQty"].ToString()
                            });
                        }
                    }
                }
                db.Conn.Close(); // Explicitly close
            }
            catch (Exception ex)
            {
                // Log error (e.g., LogError(ex))
                throw new Exception("Database error occurred.");
            }

            return list;
        }

        [WebMethod]
        public static List<string> GetCategories()
        {
            List<string> list = new List<string>();

            DB_UTILITY db = new DB_UTILITY();
            db.Sqlconnection();
            db.ConnectDb();

            try
            {
                // Use the new Stored Procedure we created earlier
                SqlCommand cmd = new SqlCommand("sp_GetProductCategories", db.Conn);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    // Adding the category name string to the list
                    list.Add(dr["ProductOrServiceCat"].ToString());
                }
            }
            catch (Exception ex)
            {
                // Handle or log error
            }
            finally
            {
                db.Conn.Close();
            }

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
