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
    public partial class WebForm5 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                string ID = Request.QueryString["ID"];
                Binddata(ID);

            }

        }
        private void Binddata(string ID)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_employee where ID='" + ID + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblId.Text = re["ID"].ToString();
                txtempid.Text = re["Emp_ID"].ToString();
                txtempname.Text = re["Name"].ToString();
                txtempdepertment.Text = re["Depertment"].ToString();
                txtempdesignation.Text = re["Designation"].ToString();
                Session["COMPANYID"] = re["Company_ID"].ToString();
            }
            DbCL.Conn.Close();

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            //string a = "Yes";
            DbCL.executeRdr("update tbl_employee set Emp_ID='" + txtempid.Text + "',Name='" + txtempname.Text + "',Designation='" + txtempdesignation.Text + "',Depertment='" + txtempdepertment.Text + "' where ID='" + lblId.Text + "'");
            int length = FileUpload1.PostedFile.ContentLength;
            byte[] imgbyte = new byte[length];
            HttpPostedFile img = FileUpload1.PostedFile;
            img.InputStream.Read(imgbyte, 0, length);
            if (FileUpload1.HasFile)
            {
                DbCL.Sqlconnection();
                DbCL.ConnectDb();
                string cmdString1 = "update tbl_employee set imgdata=@imgdata,image_status=@image_status where ID='" + lblId.Text + "'";
                SqlCommand cmd1 = new SqlCommand(cmdString1, DbCL.Conn);
                cmd1.CommandType = CommandType.Text;
                cmd1.CommandTimeout = 0;
                cmd1.Parameters.AddWithValue("@imgdata", SqlDbType.Image).Value = imgbyte;
                cmd1.Parameters.AddWithValue("@image_status", "Yes");
                cmd1.ExecuteNonQuery();
            }

            PanelOK.Visible = true;
            lblOk.Text = "Data update Successfully.....";
            Button1.Visible = false;

        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/admin/Show_data1.aspx");

        }
    }
}