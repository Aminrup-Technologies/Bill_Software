using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;

namespace Bill_Software.admin
{
    public partial class Total_id_card : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Binddate();
            }
        }
        private void Binddate()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "SELECT  tbl_employee.Emp_ID,tbl_Company.Address,tbl_employee.ID,tbl_Company.ComID,tbl_Company.Name,tbl_employee.Name AS EmployeeName,tbl_employee.Designation,tbl_employee.Depertment FROM tbl_Company INNER JOIN tbl_employee ON tbl_Company.ComID =tbl_employee.Company_ID where tbl_Company.ComID='" + Session["COMPANYID"].ToString() + "' order by tbl_employee.ID";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        
    }
}