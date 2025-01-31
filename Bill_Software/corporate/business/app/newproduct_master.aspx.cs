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

                //Products pre-loading in Page laod have been disabled initially and later only top 100 items are laoded
                Binddata();
            }

        }

        private void Binddata()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select top 100 Id,Product_code,ProductOrServiceCat,Product_catagory,Sail_Rate,Tax_Rate,Type,ProductName,Unit,Brand,parentId from tbl_NewProduct where ViewMode=1 and DeleteMode=0 order by Id desc";
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

                    // Initialize database connection
                    DbCL.Sqlconnection();
                    DbCL.ConnectDb();

                    // Start a new transaction
                    transaction = DbCL.Conn.BeginTransaction();

                    // SQL query for tbl_NewProduct
                    string queryNewProduct = @"
                            INSERT INTO tbl_NewProduct 
                            (Product_code, ProductOrServiceCat, Sail_Rate, Tax_Rate, Product_catagory, ProductName, 
                            Type, Unit, Brand, parentId, Specification, Quantity, MOQ_Value, SaleNote, ExpiryDate, 
                            TimeStamp, ProductID, AddedbyUserId, AddedOn) 
                            VALUES 
                            (@ProductCode, @ProductOrServiceCat, @SaleRate, @TaxRate, @Product_catagory, @ProductName, 
                            @Type, @Unit, @Brand, @ParentId, @Specification, @Quantity, @MOQ_Value, @SaleNote, @ExpiryDate, 
                            GETDATE(), @ProductID, @AddedbyUserId, @AddedOn)";

                    SqlCommand cmdNewProduct = new SqlCommand(queryNewProduct, DbCL.Conn, transaction);
                    cmdNewProduct.Parameters.AddWithValue("@ProductCode", txtProductCode.Text);
                    cmdNewProduct.Parameters.AddWithValue("@ProductOrServiceCat", cmdProduct.SelectedItem.Text);
                    cmdNewProduct.Parameters.AddWithValue("@SaleRate", Convert.ToDecimal(txtSalerate.Text));
                    cmdNewProduct.Parameters.AddWithValue("@TaxRate", Convert.ToDecimal(cmbtax.SelectedItem.Text));
                    cmdNewProduct.Parameters.AddWithValue("@Product_catagory", txtproducttype.Text);
                    cmdNewProduct.Parameters.AddWithValue("@ProductName", txtSubProductsName.Text);
                    cmdNewProduct.Parameters.AddWithValue("@Type", ddlProOrSer.SelectedItem.Text);
                    cmdNewProduct.Parameters.AddWithValue("@Unit", txtUnit.Text);
                    cmdNewProduct.Parameters.AddWithValue("@Brand", txtBrand.Text);
                    cmdNewProduct.Parameters.AddWithValue("@ParentId", Convert.ToInt32(Session["pid"]));
                    cmdNewProduct.Parameters.AddWithValue("@Specification", TextBox1.Text);
                    cmdNewProduct.Parameters.AddWithValue("@Quantity", Convert.ToInt32(TextBox2.Text));
                    cmdNewProduct.Parameters.AddWithValue("@MOQ_Value", Convert.ToInt32(TextBox3.Text));
                    cmdNewProduct.Parameters.AddWithValue("@SaleNote", TextBox4.Text);
                    cmdNewProduct.Parameters.AddWithValue("@ProductID", productid);
                    cmdNewProduct.Parameters.AddWithValue("@AddedbyUserId", Session["USERID"].ToString());
                    cmdNewProduct.Parameters.AddWithValue("@AddedOn", DateTime.Now);

                    DateTime expiryDate;
                    if (DateTime.TryParse(txtfromDate.Text, out expiryDate))
                    {
                        cmdNewProduct.Parameters.AddWithValue("@ExpiryDate", expiryDate);
                    }
                    else
                    {
                        cmdNewProduct.Parameters.AddWithValue("@ExpiryDate", DBNull.Value);
                    }

                    // Execute tbl_NewProduct Insert
                    cmdNewProduct.ExecuteNonQuery();

                    // SQL query for tbl_stock
                    string queryStock = @"
                    INSERT INTO tbl_stock 
                    (Product_id, Product_name, Quantity, Sail_Rate, Service_tax_rate) 
                    VALUES 
                    (@ProductID, @ProductName, @Quantity, @SaleRate, @TaxRate)";

                    SqlCommand cmdStock = new SqlCommand(queryStock, DbCL.Conn, transaction);
                    cmdStock.Parameters.AddWithValue("@ProductID", productid);
                    cmdStock.Parameters.AddWithValue("@ProductName", txtSubProductsName.Text);
                    cmdStock.Parameters.AddWithValue("@Quantity", Convert.ToInt32(TextBox2.Text));
                    cmdStock.Parameters.AddWithValue("@SaleRate", Convert.ToDecimal(txtSalerate.Text));
                    cmdStock.Parameters.AddWithValue("@TaxRate", Convert.ToDecimal(cmbtax.SelectedItem.Text));

                    // Execute tbl_stock Insert
                    cmdStock.ExecuteNonQuery();

                    // Commit transaction if both insertions succeed
                    transaction.Commit();

                    PanelOK.Visible = true;
                    lblOk.Text = "Data saved successfully into both tables!";
                }
                catch (Exception ex)
                {
                    // Rollback the transaction if an error occurs
                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }

                    lblOk.Text = "Error: " + ex.Message;
                    PanelOK.Visible = true;
                }
                finally
                {
                    // Close the connection
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

        //protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        //{
        //    string Id = Convert.ToString(e.CommandArgument);

        //    if (e.CommandName == "Delete")
        //    {
        //        DbCL.executeRdr("UPDATE tbl_NewProduct SET ViewMode=0, DeleteMode=1, DeletedOn=@DeletedOn, DeletedByUserId=@DeletedByUserId where Id='" + Id + "'");
        //        PanelOK.Visible = true;
        //        lblOk.Text = "Data Deleted Successfully...";
        //    }
        //    else if (e.CommandName == "Edit")
        //    {
        //        Response.Redirect("NewUpdate_product.aspx?Id=" + Id);
        //    }
        //    Binddata();
        //}

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Id = Convert.ToString(e.CommandArgument);

            try
            {
                if (e.CommandName == "Delete")
                {
                    string query = "UPDATE tbl_NewProduct SET ViewMode=0, DeleteMode=1, DeletedOn=@DeletedOn, DeletedByUserId=@DeletedByUserId WHERE Id=@Id";

                    // Initialize database connection
                    DbCL.Sqlconnection();
                    DbCL.ConnectDb();

                    using (SqlCommand cmd = new SqlCommand(query, DbCL.Conn))
                    {
                        cmd.Parameters.AddWithValue("@DeletedOn", DateTime.Now);
                        cmd.Parameters.AddWithValue("@DeletedByUserId", Session["USERID"].ToString());
                        cmd.Parameters.AddWithValue("@Id", Id);

                        cmd.ExecuteNonQuery();
                    }

                    PanelOK.Visible = true;
                    lblOk.Text = "Data Deleted Successfully...";
                }
                else if (e.CommandName == "Edit")
                {
                    Response.Redirect("NewUpdate_product.aspx?Id=" + Id);
                }
            }
            catch (Exception ex)
            {
                PanelOK.Visible = true;
                lblOk.Text = "Error: " + ex.Message;
            }
            finally
            {
                // Ensure proper database connection handling
                if (DbCL.Conn != null && DbCL.Conn.State == ConnectionState.Open)
                {
                    DbCL.Conn.Close();
                }
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
                BinddataByServiceCategory(pid);
            }
        }

        private void BinddataByServiceCategory(int ParentId)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Id,Product_code,ProductOrServiceCat,Product_catagory,Sail_Rate,Tax_Rate,Type,ProductName,Unit,Brand,parentId from tbl_NewProduct where parentId=@parentId order by Id asc";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.Parameters.AddWithValue("@parentId", ParentId);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

    }
}