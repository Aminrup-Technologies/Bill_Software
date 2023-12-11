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
    public partial class WebForm1 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Buinddata();

            }
        }
        private void Buinddata()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Company";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            string ComID = findcomid();
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            int length = FileUpload1.PostedFile.ContentLength;
            byte[] imgbyte = new byte[length];
            HttpPostedFile img = FileUpload1.PostedFile;
            img.InputStream.Read(imgbyte, 0, length);
            string cmdstring = "insert into tbl_Company(ComID,Name,Address,Signe)values(@ComID,@Name,@Address,@Signe)";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 0;
            cmd.Parameters.AddWithValue("@ComID", ComID);
            cmd.Parameters.AddWithValue("@Name", txtcompanyname.Text);
            cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
            cmd.Parameters.AddWithValue("@Signe", SqlDbType.Image).Value = imgbyte;
            cmd.ExecuteNonQuery();
            DbCL.Conn.Close();
            PanelOK.Visible = true;
            lblOk.Text = "Data Save Successfully...";
            txtcompanyname.Text = "";

            Buinddata();
        }
        private string findcomid()
        {
            string cmid = "";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select ComID from tbl_Company where ID=(select max(ID) from tbl_Company)";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                string cmid2 = re["ComID"].ToString();
                int cmid1 = Convert.ToInt32(cmid2.Substring(2)) + 1;
                cmid = "CM" + cmid1.ToString();

            }
            else
            {
                cmid = "CM1";
            }
            DbCL.Conn.Close();
            return cmid;
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string ComID = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Delete")
            {
                DbCL.executeRdr("delete from tbl_Company where ComID='" + ComID + "'");
                DbCL.executeRdr("delete from tbl_employee where Company_ID='" + ComID + "'");
                PanelOK.Visible = true;
                lblOk.Text = "Data Deleted Successfullu...";
                Buinddata();
            }
            if (e.CommandName == "Edit")
            {
                Response.Redirect("Update_Company.aspx?ComID=" + ComID);
            }
        }

        

    }
}