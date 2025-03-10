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
    public partial class WebForm70 : System.Web.UI.Page
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
                DbCL.FillCombo(cmbtax, "select Vat_Rate from tbl_New_Vat_Master order by ID");
                string Id = Request.QueryString["Id"];
                Binddata(Id);

                MakeFieldsReadOnly();
            }

        }

        private void MakeFieldsReadOnly()
        {
            txtSubProductsName.ReadOnly = true;
            txtproducttype.ReadOnly = true;
            TextBox1.ReadOnly = true;
            txtBrand.ReadOnly = true;
            txtProductCode.ReadOnly = true;
            txtUnit.ReadOnly = true;
            TextBox2.ReadOnly = true;
            TextBox3.ReadOnly = true;
            txtSalerate.ReadOnly = true;
            txtfromDate.ReadOnly = true;
            TextBox4.ReadOnly = true;

            // Disable DropDownLists
            //cmdProduct.Enabled = false;
            ddlProOrSer.Enabled = false;
            cmbtax.Enabled = false;
        }


        private void Binddata(string Id)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Id,Product_code, ProductOrServiceCat, Sail_Rate, Tax_Rate, Product_catagory, ProductName, Type, Unit, Brand, parentId, Specification, Quantity, MOQ_Value, SaleNote, ExpiryDate, TimeStamp, ProductID from tbl_NewProduct where Id='" + Id + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblid.Text = re["Id"].ToString();
                txtProductCode.Text = re["Product_code"].ToString();
                lblproductname.Text = re["ProductOrServiceCat"].ToString();
                txtSubProductsName.Text = re["ProductName"].ToString();
                txtBrand.Text= re["Brand"].ToString();
                ddlProOrSer.Text= re["Type"].ToString();
                txtUnit.Text= re["Unit"].ToString();
                txtSalerate.Text = re["Sail_Rate"].ToString();
                cmbtax.Text = re["Tax_Rate"].ToString();

                TextBox1.Text = re["Specification"].ToString();
                TextBox2.Text = re["Quantity"].ToString();
                TextBox3.Text = re["MOQ_Value"].ToString();
                TextBox4.Text = re["SaleNote"].ToString();
                txtproducttype.Text = re["Product_catagory"].ToString();
                DateTime expiryDate = re.IsDBNull(re.GetOrdinal("ExpiryDate"))
                                        ? DateTime.Today // Use today's date if NULL
                                        : re.GetDateTime(re.GetOrdinal("ExpiryDate"));

                txtfromDate.Text = expiryDate.ToString("dd-MMM-yyyy");
                //cmbtax.Text = re["ParentId"].ToString();
            }
            DbCL.Conn.Close();
        }


        protected void btnSave_Click(object sender, EventArgs e)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            string updateQuery = "UPDATE tbl_NewProduct SET " +
                                 "Product_code = @ProductCode, " +
                                 "Sail_Rate = @SaleRate, " +
                                 "Tax_Rate = @TaxRate, " +
                                 "Product_catagory = @ProductCategory, " +
                                 "ProductName = @ProductName, " +
                                 "Type = @Type, " +
                                 "Unit = @Unit, " +
                                 "Brand = @Brand, " +
                                 "Specification = @Specification, " +
                                 "Quantity = @Quantity, " +
                                 "MOQ_Value = @MOQValue, " +
                                 "SaleNote = @SaleNote, " +
                                 "ExpiryDate = @ExpiryDate, " +
                                 "ModifiedByUserId = @ModifiedByUserId, " +
                                 "ModifiedOn = @ModifiedOn " +
                                 "WHERE Id = @Id";

            // Create the SQL command for updating tbl_NewProduct
            SqlCommand cmdUpdateProduct = new SqlCommand(updateQuery, DbCL.Conn);

            // Adding parameters with values
            cmdUpdateProduct.Parameters.AddWithValue("@ProductCode", txtProductCode.Text);
            //cmdUpdateProduct.Parameters.AddWithValue("@ProductOrServiceCat", cmdProduct.SelectedItem.Text);
            cmdUpdateProduct.Parameters.AddWithValue("@SaleRate", txtSalerate.Text);
            cmdUpdateProduct.Parameters.AddWithValue("@TaxRate", cmbtax.SelectedItem.Text);
            cmdUpdateProduct.Parameters.AddWithValue("@ProductCategory", txtproducttype.Text);
            cmdUpdateProduct.Parameters.AddWithValue("@ProductName", txtSubProductsName.Text);
            cmdUpdateProduct.Parameters.AddWithValue("@Type", ddlProOrSer.SelectedItem.Text);
            cmdUpdateProduct.Parameters.AddWithValue("@Unit", txtUnit.Text);
            cmdUpdateProduct.Parameters.AddWithValue("@Brand", txtBrand.Text);
            //cmdUpdateProduct.Parameters.AddWithValue("@ParentId", Convert.ToInt32(Session["pid"]));  // Assuming parentId is session-based
            cmdUpdateProduct.Parameters.AddWithValue("@Specification", TextBox1.Text);
            cmdUpdateProduct.Parameters.AddWithValue("@Quantity", TextBox2.Text);
            cmdUpdateProduct.Parameters.AddWithValue("@MOQValue", TextBox3.Text);
            cmdUpdateProduct.Parameters.AddWithValue("@SaleNote", TextBox4.Text);
            cmdUpdateProduct.Parameters.AddWithValue("@ExpiryDate", txtfromDate.Text);  // Assuming the format is correct
            //cmdUpdateProduct.Parameters.AddWithValue("@ProductID", productid);  // Assuming productid is a valid variable
            cmdUpdateProduct.Parameters.AddWithValue("@Id", lblid.Text);  // Assuming lblid contains the record ID to be updated
            cmdUpdateProduct.Parameters.AddWithValue("@ModifiedByUserId", Session["USERID"].ToString());
            cmdUpdateProduct.Parameters.AddWithValue("@ModifiedOn", DateTime.Now);
            // Execute the update command
            cmdUpdateProduct.ExecuteNonQuery();

            // Display success message
            PanelOK.Visible = true;
            lblOk.Text = "Data Updated Successfully...";

            // Close the connection
            DbCL.Conn.Close();

            // Hide the save button
            btnSave.Visible = false;
        }


        //protected void btnSave_Click(object sender, EventArgs e)
        //{
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    DbCL.executeRdr("update tbl_NewProduct set Product_code='"+ txtProductCode.Text + "', ProductName='" + txtSubProductsName.Text + "',Sail_Rate='" + txtSalerate.Text + "',Tax_Rate='" + cmbtax.Text + "',Brand='" + txtBrand.Text + "',Type='" + ddlProOrSer.Text + "',Unit='" + txtUnit.Text + "' where Id='" + lblid.Text + "'");
        //    PanelOK.Visible = true;
        //    lblOk.Text = "Data Update Successfully...";
        //    DbCL.Conn.Close();
        //    btnSave.Visible = false;

        //}

        protected void btnedit_Click(object sender, EventArgs e)
        {
            MakeFieldsEditable();
            //txtProductCode.Enabled = true;
            //lblproductname.Enabled = true;
            //txtSubProductsName.Enabled = true;
            //txtBrand.Enabled = true;
            //ddlProOrSer.Enabled = true;
            //txtUnit.Enabled = true;
            //txtSalerate.Enabled = true;
            //cmbtax.Enabled = true;


            btnSave.Visible = true;
            btnedit.Visible = false;
        }


        private void MakeFieldsEditable()
        {
            txtSubProductsName.ReadOnly = false;
            txtproducttype.ReadOnly = false;
            TextBox1.ReadOnly = false;
            txtBrand.ReadOnly = false;
            txtProductCode.ReadOnly = false;
            txtUnit.ReadOnly = false;
            TextBox2.ReadOnly = false;
            TextBox3.ReadOnly = false;
            txtSalerate.ReadOnly = false;
            txtfromDate.ReadOnly = false;
            TextBox4.ReadOnly = false;

            // Enable DropDownLists
            //cmdProduct.Enabled = true;
            ddlProOrSer.Enabled = true;
            cmbtax.Enabled = true;
        }


        protected void btnback_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/newproduct_master.aspx");
        }
    }
}