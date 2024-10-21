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
                try
                {
                    // Initialize the database connection
                    DbCL.Sqlconnection();
                    DbCL.ConnectDb();

                    // Create the SQL query with parameters
                    string query = "INSERT INTO tbl_NewProduct(Product_code, ProductOrServiceCat, Sail_Rate, Tax_Rate, Product_catagory, ProductName, " +
                                   "Type, Unit, Brand, parentId, Specification, Quantity, MOQ_Value, SaleNote, ExpiryDate, TimeStamp) " +
                                   "VALUES (@ProductCode, @ProductOrServiceCat, @SaleRate, @TaxRate, @Product_catagory, @ProductName, @Type, @Unit, @Brand, @ParentId, " +
                                   "@Specification, @Quantity, @MOQValue, @SaleNote, @ExpiryDate, GETDATE())";

                    // Create the SQL command and assign the parameters
                    SqlCommand cmd = new SqlCommand(query, DbCL.Conn);
                    cmd.Parameters.AddWithValue("@ProductCode", txtProductCode.Text);
                    cmd.Parameters.AddWithValue("@ProductOrServiceCat", cmdProduct.SelectedItem.Text);
                    cmd.Parameters.AddWithValue("@SaleRate", txtSalerate.Text);
                    cmd.Parameters.AddWithValue("@TaxRate", cmbtax.SelectedItem.Text);
                    cmd.Parameters.AddWithValue("@Product_catagory", txtproducttype.Text);
                    cmd.Parameters.AddWithValue("@ProductName", txtSubProductsName.Text);
                    cmd.Parameters.AddWithValue("@Type", ddlProOrSer.SelectedItem.Text);
                    cmd.Parameters.AddWithValue("@Unit", txtUnit.Text);
                    cmd.Parameters.AddWithValue("@Brand", txtBrand.Text);
                    cmd.Parameters.AddWithValue("@ParentId", Convert.ToInt32(Session["pid"]));
                    cmd.Parameters.AddWithValue("@Specification", TextBox1.Text);
                    cmd.Parameters.AddWithValue("@Quantity", TextBox2.Text);
                    cmd.Parameters.AddWithValue("@MOQValue", TextBox3.Text);
                    cmd.Parameters.AddWithValue("@SaleNote", TextBox4.Text);

                    DateTime expiryDate;
                    // Handle the expiry date (ensure valid format or handle nulls)
                    if (DateTime.TryParse(txtfromDate.Text, out expiryDate))
                    {
                        cmd.Parameters.AddWithValue("@ExpiryDate", expiryDate);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@ExpiryDate", DBNull.Value); // Insert NULL if the date is invalid
                    }

                    // Execute the query
                    cmd.ExecuteNonQuery();

                    // Show success message
                    PanelOK.Visible = true;
                    lblOk.Text = "Data saved successfully!";
                }
                catch (Exception ex)
                {
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