using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm53 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        int countre = 1;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                Bindgridi1();
                
                if (countre == 1)
                {
                    PanelOK.Visible = true;
                    lblOk.Text = "No Payments Is Due...";
                }
                else
                {
                    PanelOK.Visible = false;
                }

            }
        }

        private void Bindgridi1()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            //string CmdString = "SELECT tbl_Purches.Purches_Id,tbl_Purches.Total_purches_rate,tbl_Purches.Purches_date,tbl_Vendor.Vendor_Name,tbl_purches_due.Due_amount,tbl_Purches.Purches_Id as Purches_Id FROM tbl_Purches INNER JOIN tbl_purches_due ON tbl_Purches.Purches_Id =tbl_purches_due.Purches_Id INNER JOIN tbl_Vendor ON tbl_Purches.Client_Id =tbl_Vendor.Vendor_Id";

            //string CmdString = "SELECT p.Purches_Id, p.Purches_date, v.Vendor_Name, p.BuyerOrderNo AS OrderNo, p.OrderDate, p.ShippedToStoreName AS Destination, p.Total_purches_rate AS TaxableAmount,p.Total_Tax_rate AS TaxAmount, p.InvoiceAmnt AS TotalAmount, d.Due_amount, p.AddedById, l.Name AS AddedByName, p.TimeStamp AS CreatedOn FROM tbl_Purches AS p INNER JOIN tbl_purches_due AS d ON p.Purches_Id = d.Purches_Id INNER JOIN tbl_Vendor AS v ON p.Client_Id = v.Vendor_Id LEFT JOIN tbl_login AS l ON p.AddedById = l.User_Id ORDER BY p.Purches_date DESC;";

            string CmdString = @"SELECT p.Purches_Id, p.Purches_date, p.Purches_Type, p.Invoice_No, p.Stock_Add_Date, p.Narration, p.TimeStamp AS CreatedOn, p.CreatedDate, p.BuyerOrderNo AS OrderNo, p.OrderDate, p.ShippedToStoreId, p.ShippedToStoreName AS Destination, p.Total_purches_rate AS TotalAmount, p.Total_Tax_rate AS TaxAmount, p.InvoiceAmnt AS TaxableAmount, p.TCS_Rate, p.TCS_Amount, p.Delivery_Rate, p.Delivery_Amount, p.otherAmount1_name, p.otherAmount1, p.otherAmount2_name, p.otherAmount2, v.Vendor_Name, d.Due_amount, p.AddedById, l.Name AS AddedByName FROM tbl_Purches AS p INNER JOIN tbl_purches_due AS d ON p.Purches_Id = d.Purches_Id INNER JOIN tbl_Vendor AS v ON p.Client_Id = v.Vendor_Id LEFT JOIN tbl_login AS l ON p.AddedById = l.User_Id ORDER BY CAST(p.Purches_date as Date) DESC;";
            SqlCommand com1 = new SqlCommand(CmdString, DbCL.Conn);
            SqlDataAdapter da = new SqlDataAdapter(com1);
            SqlDataReader dr = com1.ExecuteReader();

            if (dr.Read())
            {

                DataList1.DataSource = DbCL.GetDataTable(CmdString);
                DataList1.DataBind();
                first_div.Visible = true;
                countre = countre + 1;

            }
            else
            {
                first_div.Visible = false;
            }
            DbCL.Conn.Close();
        }
    }
}