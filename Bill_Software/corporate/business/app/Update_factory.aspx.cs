using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm63 : System.Web.UI.Page
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
                string ID = Request.QueryString["ID"];
                lblID.Text = ID.ToString();
                DbCL.FillCombo(cmbState, "select State_Name from tbl_State order by State_Name");
                DbCL.FillCombo(cmbcity, "select City_Name from tbl_City order by City_Name");
                BindData1();
                Binddata2();
            }

        }

        private void Binddata2()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Name from tbl_Client where  Client_Id='"+ lblclientId.Text +"'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblclientName.Text = re["Client_Name"].ToString();

            }
            DbCL.Conn.Close();
            
        }

        private void BindData1()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_id,Factory_name,Address1,Address2,city,State,pin from tbl_Factory where ID='" + lblID.Text +"'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblclientId.Text = re["Client_id"].ToString();
                lblFactoryName.Text = re["Factory_name"].ToString();
                txtAddress1.Text = re["Address1"].ToString();
                txtaddress2.Text = re["Address2"].ToString();
                txtpin.Text = re["pin"].ToString();
                cmbcity.Text = re["city"].ToString();
                cmbState.Text = re["State"].ToString();
            }
            DbCL.Conn.Close();
            
        }

        //protected void btnUpdate_Click(object sender, EventArgs e)
        //{

        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    string cmdstring = "Update  tbl_Factory set Address1=@Address1,Address2=@Address2,city=@city,State=@State,pin=@pin where ID='"+ lblID.Text +"'";
        //    SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
        //    cmd.CommandType = CommandType.Text;
        //    //cmd.Parameters.AddWithValue("@Client_id", lblclientId.Text);
        //    //cmd.Parameters.AddWithValue("@Factory_name", txtFactoryName.Text);
        //    cmd.Parameters.AddWithValue("@Address1", txtAddress1.Text);
        //    cmd.Parameters.AddWithValue("@Address2", txtaddress2.Text);
        //    cmd.Parameters.AddWithValue("@city", cmbcity.Text);
        //    cmd.Parameters.AddWithValue("@State", cmbState.Text);
        //    cmd.Parameters.AddWithValue("@pin", txtpin.Text);
        //    cmd.ExecuteNonQuery();
        //    DbCL.Conn.Close();


        //    PanelOK.Visible = true;
        //    lblOk.Text = "Data Update Successfully...";
        //    txtAddress1.Enabled = false;
        //    txtaddress2.Enabled = false;
        //    txtpin.Enabled = false;
        //    cmbcity.Enabled = false;
        //    cmbState.Enabled = false;
        //    btnUpdate.Visible = false;
        //    btnEdit.Visible = true;

        //}

        protected void btnEdit_Click(object sender, EventArgs e)
        {
            txtAddress1.Enabled = true;
            txtaddress2.Enabled = true;
            txtpin.Enabled = true;
            cmbcity.Enabled = true;
            cmbState.Enabled = true;
            btnUpdate.Visible = true;
            btnEdit.Visible = false;

        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("ShowFactory.aspx?Client_Id=" + lblclientId.Text);

        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "Update  tbl_Factory set Address1=@Address1,Address2=@Address2,city=@city,State=@State,pin=@pin where ID='" + lblID.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.CommandType = CommandType.Text;
            //cmd.Parameters.AddWithValue("@Client_id", lblclientId.Text);
            //cmd.Parameters.AddWithValue("@Factory_name", txtFactoryName.Text);
            cmd.Parameters.AddWithValue("@Address1", txtAddress1.Text);
            cmd.Parameters.AddWithValue("@Address2", txtaddress2.Text);
            cmd.Parameters.AddWithValue("@city", cmbcity.Text);
            cmd.Parameters.AddWithValue("@State", cmbState.Text);
            cmd.Parameters.AddWithValue("@pin", txtpin.Text);
            cmd.ExecuteNonQuery();
            DbCL.Conn.Close();


            PanelOK.Visible = true;
            lblOk.Text = "Data Update Successfully...";
            txtAddress1.Enabled = false;
            txtaddress2.Enabled = false;
            txtpin.Enabled = false;
            cmbcity.Enabled = false;
            cmbState.Enabled = false;
            btnUpdate.Visible = false;
            btnEdit.Visible = true;

        }
    }
}