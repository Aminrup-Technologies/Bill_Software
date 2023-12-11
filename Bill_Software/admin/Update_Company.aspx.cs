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
    public partial class WebForm4 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string ComID = Request.QueryString["ComID"];
                lblcomId.Text = ComID.ToString();
                Binddata();
            }
        }
        private void Binddata()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Company";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                txtcompanyname.Text = re["Name"].ToString();
                txtAddress.Text = re["Address"].ToString();
            }
            DbCL.Conn.Close();

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            DbCL.executeRdr("update tbl_Company set Name='" + txtcompanyname.Text + "',Address='"+ txtAddress.Text +"' where ComID='" + lblcomId.Text + "'");
            if (FileUpload1.HasFile)
            {
                DbCL.Sqlconnection();
                DbCL.ConnectDb();
                int length = FileUpload1.PostedFile.ContentLength;
                byte[] imgbyte = new byte[length];
                HttpPostedFile img = FileUpload1.PostedFile;
                img.InputStream.Read(imgbyte, 0, length);
                string cmdstring = "update tbl_Company set Signe=@Signe where ComID='" + lblcomId.Text + "'";
                SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddWithValue("@Signe", SqlDbType.Image).Value = imgbyte;
                cmd.ExecuteNonQuery();
                DbCL.Conn.Close();

            }

            PanelOK.Visible = true;
            lblOk.Text = "Data Update Successfully...";
            Button1.Visible = false;

        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/admin/add_company.aspx");
        }
    }
}