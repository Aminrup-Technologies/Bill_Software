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
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                Binddata();
            }

        }

        private void Binddata()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Id,Product_code,ProductOrServiceCat,Product_catagory,Sail_Rate,Tax_Rate,Type,ProductName,Unit,Brand,parentId from tbl_NewProduct order by Id asc";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }


        private string findProductId()
        {
            string PurID = "";
            string aa = "";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdString1 = "select Id,ProductID from tbl_NewProduct where Id=(select max(Id)from tbl_NewProduct)";
            SqlCommand com1 = new SqlCommand(cmdString1, DbCL.Conn);
            SqlDataReader DR1 = com1.ExecuteReader();
            if (DR1.Read())
            {
                aa = DR1.GetValue(1).ToString();
                string bb = aa.Substring(3); // Get the numeric part of ProductID, skipping "PRD"
                int k = Convert.ToInt32(bb);
                k = k + 1; // Increment the numeric part
                PurID = "PRD" + k.ToString().PadLeft(2, '0'); // Pad with leading zeros to ensure two digits
            }
            else
            {
                PurID = "PRD01"; // Start with "PRD01" if no records exist
            }

            DbCL.Conn.Close();
            return PurID;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            //if (txtProductCode.Text != "")
            //{
            //    //string product_code = Session["ProductCode"].ToString();
            //    //string gstRate = Session["gstRate"].ToString();
            //    DbCL.Sqlconnection();
            //    DbCL.ConnectDb();
            //    DbCL.executeRdr("insert into tbl_NewProduct(Product_code,ProductOrServiceCat,Sail_Rate,Tax_Rate,ProductName,Type,Unit,Brand,parentId) values ('" + txtProductCode.Text.ToString() + "','" + cmdProduct.Text + "','" + txtSalerate.Text + "','" + cmbtax.Text.ToString() + "','" + txtSubProductsName.Text + "','" + ddlProOrSer.Text + "','" + txtUnit.Text + "','" + txtBrand.Text + "','" + Convert.ToInt32(Session["pid"]) + "')");
            //    PanelOK.Visible = true;
            //    lblOk.Text = "Data Save Successfully...";
            //    DbCL.Conn.Close();
            //}

            if (!string.IsNullOrEmpty(txtProductCode.Text))
            {
                SqlTransaction transaction = null;

                try
                {
                    string productid = findProductId();

                    // Initialize the database connection
                    DbCL.Sqlconnection();
                    DbCL.ConnectDb();

                    // Start a new transaction
                    transaction = DbCL.Conn.BeginTransaction();

                    // Create the SQL query for tbl_NewProduct
                    string queryNewProduct = "INSERT INTO tbl_NewProduct(Product_code, ProductOrServiceCat, Sail_Rate, Tax_Rate, Product_catagory, ProductName, " +
                                             "Type, Unit, Brand, parentId, Specification, Quantity, MOQ_Value, SaleNote, ExpiryDate, TimeStamp, ProductID) " +
                                             "VALUES (@ProductCode, @ProductOrServiceCat, @SaleRate, @TaxRate, @Product_catagory, @ProductName, @Type, @Unit, @Brand, @ParentId, " +
                                             "@Specification, @Quantity, @MOQValue, @SaleNote, @ExpiryDate, GETDATE(), @ProductID)";

                    // Create the SQL command for tbl_NewProduct
                    SqlCommand cmdNewProduct = new SqlCommand(queryNewProduct, DbCL.Conn, transaction);
                    cmdNewProduct.Parameters.AddWithValue("@ProductCode", txtProductCode.Text);
                    cmdNewProduct.Parameters.AddWithValue("@ProductOrServiceCat", cmdProduct.SelectedItem.Text);
                    cmdNewProduct.Parameters.AddWithValue("@SaleRate", txtSalerate.Text);
                    cmdNewProduct.Parameters.AddWithValue("@TaxRate", cmbtax.SelectedItem.Text);
                    cmdNewProduct.Parameters.AddWithValue("@Product_catagory", txtproducttype.Text);
                    cmdNewProduct.Parameters.AddWithValue("@ProductName", txtSubProductsName.Text);
                    cmdNewProduct.Parameters.AddWithValue("@Type", ddlProOrSer.SelectedItem.Text);
                    cmdNewProduct.Parameters.AddWithValue("@Unit", txtUnit.Text);
                    cmdNewProduct.Parameters.AddWithValue("@Brand", txtBrand.Text);
                    cmdNewProduct.Parameters.AddWithValue("@ParentId", Convert.ToInt32(Session["pid"]));
                    cmdNewProduct.Parameters.AddWithValue("@Specification", TextBox1.Text);
                    cmdNewProduct.Parameters.AddWithValue("@Quantity", TextBox2.Text);
                    cmdNewProduct.Parameters.AddWithValue("@MOQValue", TextBox3.Text);
                    cmdNewProduct.Parameters.AddWithValue("@SaleNote", TextBox4.Text);
                    cmdNewProduct.Parameters.AddWithValue("@ProductID", productid);

                    DateTime expiryDate;
                    if (DateTime.TryParse(txtfromDate.Text, out expiryDate))
                    {
                        cmdNewProduct.Parameters.AddWithValue("@ExpiryDate", expiryDate);
                    }
                    else
                    {
                        cmdNewProduct.Parameters.AddWithValue("@ExpiryDate", DBNull.Value);
                    }

                    // Execute the query for tbl_NewProduct
                    cmdNewProduct.ExecuteNonQuery();

                    // Now insert relevant data into tbl_stock
                    string queryStock = "INSERT INTO tbl_stock (Product_id, Product_name, Quantity, Sail_Rate, Service_tax_rate) " +
                                        "VALUES (@ProductID, @ProductName, @Quantity, @SaleRate, @TaxRate)";

                    // Create the SQL command for tbl_stock
                    SqlCommand cmdStock = new SqlCommand(queryStock, DbCL.Conn, transaction);
                    cmdStock.Parameters.AddWithValue("@ProductID", productid);
                    cmdStock.Parameters.AddWithValue("@ProductName", txtSubProductsName.Text);
                    cmdStock.Parameters.AddWithValue("@Quantity", TextBox2.Text);
                    cmdStock.Parameters.AddWithValue("@SaleRate", txtSalerate.Text);
                    cmdStock.Parameters.AddWithValue("@TaxRate", cmbtax.SelectedItem.Text);

                    // Execute the query for tbl_stock
                    cmdStock.ExecuteNonQuery();

                    // Commit the transaction if both inserts are successful
                    transaction.Commit();

                    // Show success message
                    PanelOK.Visible = true;
                    lblOk.Text = "Data saved successfully into both tables!";
                }
                catch (Exception ex)
                {
                    // Rollback the transaction if any error occurs
                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }

                    // Handle any exceptions
                    lblOk.Text = "Error: " + ex.Message;
                    PanelOK.Visible = true;
                }
                finally
                {
                    // Close the database connection
                    DbCL.Conn.Close();
                }
            }
            else
            {
                lblOk.Text = "Please enter a Product Code.";
                PanelOK.Visible = true;
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