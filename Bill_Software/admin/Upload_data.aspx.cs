using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Data.OleDb;

namespace Bill_Software.admin
{
    public partial class WebForm6 : System.Web.UI.Page
    {
        DataSet ds;
        DataTable Dt;
        DB_UTILITY DbCL = new DB_UTILITY();
        public static string ComID;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DbCL.FillCombo(cmbCompany, "select (ComID+'-'+Name) as Name from tbl_Company order by ID");

            }

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            GetCompanycodeCode();
            ImporttoDatatable();
            InsertData();
            string FileName = FileUpload1.FileName;
            string path = string.Concat(Server.MapPath("~/Document/" + FileUpload1.FileName));
            File.Delete(path);
            Button1.Visible = false;
            PanelOK.Visible = true;
            lblOk.Text = "Data Save Sucessfully...";
            cmbCompany.SelectedIndex = 0;

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
        private void ImporttoDatatable()
        {
            try
            {
                if (FileUpload1.HasFile)
                {
                    string FileName = FileUpload1.FileName;
                    string path = string.Concat(Server.MapPath("~/Document/" + FileUpload1.FileName));
                    FileUpload1.PostedFile.SaveAs(path);
                    OleDbConnection OleDbcon = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=Excel 12.0;");
                    OleDbCommand cmd = new OleDbCommand("SELECT * FROM [Sheet1$]", OleDbcon);
                    OleDbDataAdapter objAdapter1 = new OleDbDataAdapter(cmd);
                    ds = new DataSet();
                    objAdapter1.Fill(ds);
                    Dt = ds.Tables[0];
                }
            }
            catch (Exception ex)
            {

            }

        }





        private void InsertData()
        {

            for (int i = 0; i < Dt.Rows.Count; i++)
            {
                DbCL.Sqlconnection();
                DbCL.ConnectDb();
                DataRow row = Dt.Rows[i];
                int columnCount = Dt.Columns.Count;
                string[] columns = new string[columnCount];
                for (int j = 0; j < columnCount; j++)
                {
                    columns[j] = row[j].ToString();
                }
                string sql = "INSERT INTO tbl_employee(Emp_ID,Name,Designation,Depertment,image_status,Company_ID)";
                sql += "VALUES('" + columns[0] + "','" + columns[1] + "','" + columns[2] + "','" + columns[3] + "','No','" + ComID.ToString() + "')";
                SqlCommand cmd = new SqlCommand(sql, DbCL.Conn);
                cmd.ExecuteNonQuery();
                DbCL.Conn.Close();
            }

        }
    }
}