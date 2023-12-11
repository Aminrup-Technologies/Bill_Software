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
    public partial class WebForm61 : System.Web.UI.Page
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
                DbCL.FillCombo(cmbvendor, "select Client_Name from tbl_Client order by Client_Name");
                DbCL.FillCombo(cmbState, "select State_Name from tbl_State order by State_Name");
                DbCL.FillCombo(cmbcity, "select City_Name from tbl_City order by City_Name");
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {

            BuindCompanyId();
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "insert into tbl_Factory(Client_id,Factory_name,Address1,Address2,city,State,pin)values(@Client_id,@Factory_name,@Address1,@Address2,@city,@State,@pin)";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@Client_id", lblclientId.Text);
            //cmd.Parameters.AddWithValue("@Factory_name", txtFactoryName.Text);
            cmd.Parameters.AddWithValue("@Factory_name", ddlfactoryName.Text);
            cmd.Parameters.AddWithValue("@Address1", txtAddress1.Text);
            cmd.Parameters.AddWithValue("@Address2", txtaddress2.Text);
            cmd.Parameters.AddWithValue("@city", cmbcity.Text);
            cmd.Parameters.AddWithValue("@State", cmbState.Text);
            cmd.Parameters.AddWithValue("@pin", txtpin.Text);
            cmd.ExecuteNonQuery();
            DbCL.Conn.Close();

            
            PanelOK.Visible = true;
            lblOk.Text = "Data Save Successfully...";

            cmbvendor.SelectedIndex = 0;
            txtAddress1.Text = "";
            txtaddress2.Text = "";
            //txtFactoryName.Text = "";
            txtpin.Text = "";
            cmbcity.SelectedIndex = 0;
            cmbState.SelectedIndex = 0;

        }

        private void BuindCompanyId()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Id from tbl_Client where Client_Name='" + cmbvendor.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblclientId.Text = re["Client_Id"].ToString();
            }
            DbCL.Conn.Close();
        }

     
    }
}