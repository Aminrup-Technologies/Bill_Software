using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;

namespace Bill_Software.Print
{
    public partial class id_card1 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string ID = Request.QueryString["ID"];
                Binddate(ID);
            }
        }
        private void Binddate(string ID)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_employee where ID='" + ID + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblname.Text = re["Name"].ToString();
                lbldesignation.Text = re["Designation"].ToString();
                lbldepertment.Text = re["Depertment"].ToString();
                string comid = re["Company_ID"].ToString();
                BindCompanydetails(comid);
                string ID1 = re["ID"].ToString();
                string a = re["image_status"].ToString();
                if (a == "Yes")
                {
                    Image2.ImageUrl = "~/admin/personal_image.ashx?ID=" + ID1;
                }

            }
            DbCL.Conn.Close();
        }

        private void BindCompanydetails(string comid)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Company where ComID='" + comid + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblcompanyname.Text = re["Name"].ToString();
                lbladdress.Text = re["Address"].ToString();
                string ComID = re["ComID"].ToString();
                Image1.ImageUrl = "~/admin/Company.ashx?ComID=" + ComID;
            }
            DbCL.Conn.Close();
        }
    }
}