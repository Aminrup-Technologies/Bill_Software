using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;

namespace Bill_Software
{
    public partial class SessionKeepAlive : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        protected void Page_Load(object sender, EventArgs e)
        {
            string UserName = Session["USERID"].ToString();
            string cmdString = "select Name,User_Id from tbl_login where User_Id='" + UserName.ToString() + "'";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdString, DbCL.Conn);
            SqlDataReader Rdr;
            Rdr = cmd.ExecuteReader();
            if (Rdr.Read())
            {
                lblName.Text = Rdr["Name"].ToString();
                Session["USERID"] = Rdr["User_Id"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            string UserName = Session["USERID"].ToString();
            string cmdString = "select Name,User_Id from tbl_login where User_Id='" + UserName.ToString() + "'";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdString, DbCL.Conn);
            SqlDataReader Rdr;
            Rdr = cmd.ExecuteReader();
            if (Rdr.Read())
            {
                lblName.Text = Rdr["Name"].ToString();
                Session["USERID"] = Rdr["User_Id"].ToString();
            }
            DbCL.Conn.Close();
        }
    }
}