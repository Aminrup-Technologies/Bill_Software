using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;

namespace Bill_Software.admin
{
    public partial class WebForm3 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        public static string ComID;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DbCL.FillCombo(cmbCompany, "select (ComID+'-'+Name) as Name from tbl_Company order by ID");

            }
        }
        private void Binddata()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_employee where Company_ID='" + ComID.ToString() + "' order by ID";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();

        }
        private void Binddata1()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_employee where Company_ID='" + ComID.ToString() + "' order by ID";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                Image2.Visible = true;
                Session["COMPANYID"] = ComID.ToString();
            }
            else
            {
                Image2.Visible = false;
            }
            DbCL.Conn.Close();

        }
        private void GetCompanycodeCode()
        {
            string ComID1 = cmbCompany.Text;
            string[] words = ComID1.Split('-');
            foreach (string word in words)
            {
                ComID = Convert.ToString(word);
                break;
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            GetCompanycodeCode();
            Binddata();
            Binddata1();
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string ID = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Edit")
            {
                Response.Redirect("update_image.aspx?Id=" + ID);

            }
        }
    }
}