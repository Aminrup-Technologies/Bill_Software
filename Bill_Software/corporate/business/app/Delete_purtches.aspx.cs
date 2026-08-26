using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm22 : System.Web.UI.Page
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
                DbCL.FillCombo(cmbvendor, "select Vendor_Name from tbl_Vendor order by Vendor_Name");
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");

            }

        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                BuindCompanyId();
                cmdstring = "select tbl_Purches.Purches_Id,tbl_Purches.Purches_date,tbl_Purches.Total_purches_rate,tbl_Purches.Total_Tax_rate,tbl_Vendor.Vendor_Name from tbl_Purches inner join tbl_Vendor on tbl_Purches.Client_Id=tbl_Vendor.Vendor_Id where tbl_Purches.Client_Id='" + lblclientId.Text + "' order by cast(tbl_Purches.Purches_date as datetime) desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                cmdstring = "select tbl_Purches.Purches_Id,tbl_Purches.Purches_date,tbl_Purches.Total_purches_rate,tbl_Purches.Total_Tax_rate,tbl_Vendor.Vendor_Name from tbl_Purches inner join tbl_Vendor on tbl_Purches.Client_Id=tbl_Vendor.Vendor_Id where TRY_CAST(tbl_Purches.Purches_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by TRY_CAST(tbl_Purches.Purches_date as datetime) desc";
                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                cmdstring = "select tbl_Purches.Purches_Id,tbl_Purches.Purches_date,tbl_Purches.Total_purches_rate,tbl_Purches.Total_Tax_rate,tbl_Vendor.Vendor_Name from tbl_Purches inner join tbl_Vendor on tbl_Purches.Client_Id=tbl_Vendor.Vendor_Id where tbl_Purches.Client_Id='" + lblclientId.Text + "' and TRY_CAST(tbl_Purches.Purches_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by TRY_CAST(tbl_Purches.Purches_date as datetime) desc";
                Buinddatagrid(cmdstring);
            }
            btnSertch.Visible = false;

        }
        //private void Buinddatagrid(string cmdstring)
        //{
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
        //    SqlDataReader re = cmd.ExecuteReader();
        //    if (re.Read())
        //    {
        //        Buinddatagrid1(cmdstring);
        //    }
        //    else
        //    {
        //        PanelError.Visible = true;
        //        lblErrorMsg.Text = "No Data Found...";

        //    }
        //    DbCL.Conn.Close();
        //}

        private void Buinddatagrid(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                DataList1.DataSource = reader;
                DataList1.DataBind();
            }
            else
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "No Data Found...";
            }

            reader.Close(); // Important to close reader explicitly
            DbCL.Conn.Close();
        }


        private void Buinddatagrid1(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            SqlCommand cmd1 = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd1.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();

        }

        private void BuindCompanyId()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Vendor_Id from tbl_Vendor where Vendor_Name='" + cmbvendor.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblclientId.Text = re["Vendor_Id"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/Delete_purtches.aspx");
        }

        //protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        //{
        //    string Purches_Id = Convert.ToString(e.CommandArgument);

        //    if (e.CommandName == "Delete")
        //    {
        //        updatestock(Purches_Id);
        //        DbCL.executeRdr("delete from tbl_purches_details where Purches_id='" + Purches_Id + "'");
        //        DbCL.executeRdr("delete from tbl_Purches where Purches_Id='" + Purches_Id + "'");
        //        DbCL.executeRdr("delete from tbl_purches_due where Purches_Id='" + Purches_Id + "'");
        //        PanelOK.Visible = true;
        //        lblOk.Text = "Data Deleted Successfully...";
        //        DataList1.Visible = false;
        //    }
        //}

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Purches_Id = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Delete")
            {
                bool success = DeletePurchaseWithStockUpdate(Purches_Id);

                if (success)
                {
                    PanelOK.Visible = true;
                    lblOk.Text = "Data Deleted Successfully...";
                    DataList1.Visible = false;
                }
                else
                {
                    PanelOK.Visible = true;
                    lblOk.Text = "An error occurred during deletion. Please check logs.";
                }
            }
        }

        private bool DeletePurchaseWithStockUpdate(string purchesId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ToString()))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        string updateStock = @"
                    UPDATE s
                    SET s.Quantity = CAST(CAST(s.Quantity AS DECIMAL(18,3)) - CAST(pd.Quantity AS DECIMAL(18,3)) AS VARCHAR(50))
                    FROM tbl_stock s
                    INNER JOIN tbl_purches_details pd 
                        ON s.Product_id = pd.Product_id AND s.ShippedToStoreId = pd.ShippedToLoc
                    WHERE pd.Purches_id = @PurchesId;

                    UPDATE np
                    SET np.Quantity = CAST(CAST(np.Quantity AS DECIMAL(18,3)) - CAST(pd.Quantity AS DECIMAL(18,3)) AS NVARCHAR(100))
                    FROM tbl_NewProduct np
                    INNER JOIN tbl_purches_details pd 
                        ON np.ProductID = pd.Product_id
                    WHERE pd.Purches_id = @PurchesId;";

                        SqlCommand updateCmd = new SqlCommand(updateStock, conn, transaction);
                        updateCmd.Parameters.AddWithValue("@PurchesId", purchesId);
                        updateCmd.ExecuteNonQuery();

                        string deleteDue = "DELETE FROM tbl_purches_due WHERE Purches_Id = @PurchesId;";
                        string deleteDetails = "DELETE FROM tbl_purches_details WHERE Purches_id = @PurchesId;";
                        string deleteMain = "DELETE FROM tbl_Purches WHERE Purches_Id = @PurchesId;";

                        foreach (string query in new[] { deleteDue, deleteDetails, deleteMain })
                        {
                            SqlCommand deleteCmd = new SqlCommand(query, conn, transaction);
                            deleteCmd.Parameters.AddWithValue("@PurchesId", purchesId);
                            deleteCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        // Log the error message
                        Console.WriteLine("Transaction failed: " + ex.Message);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle connection errors
                Console.WriteLine("Connection failed: " + ex.Message);
                return false;
            }
        }


        private void updatestock(string Purches_Id)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Product_id,Product_name,Quantity from tbl_purches_details where Purches_id='" + Purches_Id + "'";
            SqlCommand cmd = new SqlCommand(cmdstring,DbCL.Conn);
            SqlDataReader re=cmd.ExecuteReader();
            while(re.Read())
            {
                string product_code = re["Product_id"].ToString();
                string Product_name = re["Product_name"].ToString();
                string Quantity = re["Quantity"].ToString();
                updatestock1(product_code, Product_name, Quantity);
            }
            DbCL.Conn.Close();
        }

        private void updatestock1(string product_code, string Product_name, string Quantity)
        {
            //DbCL.executeRdr("update tbl_stock set Quantity=(cast(Quantity as int)-'" + Quantity.ToString() + "') where Product_id='" + product_code.ToString() + "' and Product_name='" + Product_name.ToString() + "'");


            string sql = @"UPDATE s SET s.Quantity = CAST(CAST(s.Quantity AS DECIMAL(18,3)) - CAST(pd.Quantity AS DECIMAL(18,3)) AS VARCHAR(50))
                FROM tbl_stock s INNER JOIN tbl_purches_details pd ON s.Product_id = pd.Product_id AND s.ShippedToStoreId = pd.ShippedToLoc WHERE pd.Purches_id = @PurchesId";

            var parameters = new Dictionary<string, object>
            {
                { "@PurchesId", "PR0042" }  // You can replace this with a dynamic value
            };

            DbCL.executeRdrNew(sql, parameters);


            string sql2 = @"
                UPDATE np
                SET np.Quantity = CAST(CAST(np.Quantity AS DECIMAL(18,3)) - CAST(pd.Quantity AS DECIMAL(18,3)) AS NVARCHAR(100))
                FROM tbl_NewProduct np
                INNER JOIN tbl_purches_details pd 
                    ON np.ProductID = pd.Product_id
                WHERE pd.Purches_id = @PurchesId";

                        var parameters2 = new Dictionary<string, object>
            {
                { "@PurchesId", "PR0042" }  // Replace with dynamic value as needed
            };

            DbCL.executeRdrNew(sql2, parameters2);
        }
    }
}