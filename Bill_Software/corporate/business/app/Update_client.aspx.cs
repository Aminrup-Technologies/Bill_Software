using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm17 : System.Web.UI.Page
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
                string Client_Id = Request.QueryString["Client_Id"];
                lblvendor_id.Text = Client_Id.ToString();
                DbCL.FillCombo(cmbState, "select State_Name from tbl_State order by State_Name");
                DbCL.FillCombo(cmbcity, "select City_Name from tbl_City order by City_Name");
                DbCL.FillCombo(ddlRegState, "select State_Name from tbl_State order by State_Name");
                DbCL.FillCombo(ddlRegCity, "select City_Name from tbl_City order by City_Name");
                DbCL.FillCombo(cmbIndustry, "select IndustryName from tbl_Industry");
                Binddate();
            }

        }
        private void Binddate()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Client where Client_Id='" + lblvendor_id.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                txtvendorName.Text = re["Client_Name"].ToString();
                //txtAddress1.Text = re["Address1"].ToString();
                txtAddress1.Text = re["Address1"].ToString();
                txtPhone.Text = re["Com_phone"].ToString();
                cmbcity.Text = re["City"].ToString();
                txtPin.Text = re["pin"].ToString();
                cmbState.Text = re["State"].ToString();
               
                txtWebsite.Text = re["Com_web_site"].ToString();
                txtEmail.Text = re["Com_email"].ToString();
                txtFax.Text = re["Com_Fax"].ToString();
              
                txtservicetax_no.Text = re["Service_tax_no"].ToString();//gstno
                txtpanno.Text = re["Pan_no"].ToString();

                cmbIndustry.Text= re["Industry"].ToString(); 
                //txtRepresentativeName.Text = re["Rep_Name"].ToString();
                //txtRepresantativeDesig.Text = re["Rep_Desig"].ToString();
                //txtRepresentativePhone.Text = re["Rep_phone"].ToString();
                //txtRepresentativeEmail.Text = re["Rep_email"].ToString();


                ////txtGstNo.Text = re["clientgstno"].ToString();
                ////txtvatno.Text = re["Vat_no"].ToString();
            }
            DbCL.Conn.Close();
            bindregaddress();
        }

        private void bindregaddress()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Address,State,City,Phno,pin from tbl_ClientRegAddress where Client_Id='" + lblvendor_id.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                txtRegAddress.Text = re["Address"].ToString();
                ddlRegState.Text = re["State"].ToString();
                ddlRegCity.Text = re["City"].ToString();
                txtRegPin.Text = re["pin"].ToString();
                txtRegPhno.Text = re["Phno"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void btnEdit_Click(object sender, EventArgs e)
        {
            //txtAddress1.Enabled = true;
            txtAddress1.Enabled = true;
            txtEmail.Enabled = true;
            txtFax.Enabled = true;
            txtPhone.Enabled = true;
            txtPin.Enabled = true;

            txtRegAddress.Enabled = true;
            ddlRegState.Enabled = true;
            ddlRegCity.Enabled = true;
            txtRegPin.Enabled = true;
            txtRegPhno.Enabled = true;


            //txtRepresantativeDesig.Enabled = true;
            //txtRepresentativeEmail.Enabled = true;
            //txtRepresentativeName.Enabled = true;
            //txtRepresentativePhone.Enabled = true;
            txtvendorName.Enabled = true;
            txtWebsite.Enabled = true;
            txtservicetax_no.Enabled = true;
            txtpanno.Enabled = true;
            
            ////txtGstNo.Enabled = true;
            ////txtvatno.Enabled = true;
            cmbcity.Enabled = true;
            cmbState.Enabled = true;
            cmbIndustry.Enabled = true;
            btnUpdate.Visible = true;
            btnEdit.Visible = false;

        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            //string cmdstring = "update tbl_Client set Client_Name=@Client_Name,Address1=@Address1,Address2=@Address2,City=@City,pin=@pin,State=@State,Com_web_site=@Com_web_site,Com_email=@Com_email,Com_phone=@Com_phone,Com_Fax=@Com_Fax,Rep_Name=@Rep_Name,Rep_Desig=@Rep_Desig,Rep_phone=@Rep_phone,Rep_email=@Rep_email,Service_tax_no=@Service_tax_no,Pan_no=@Pan_no where Client_Id='" + lblvendor_id.Text + "'";

            string cmdstring = "update tbl_Client set Client_Name=@Client_Name,Address1=@Address1,City=@City,pin=@pin,State=@State,Com_web_site=@Com_web_site,Com_email=@Com_email,Com_phone=@Com_phone,Com_Fax=@Com_Fax,Service_tax_no=@Service_tax_no,Pan_no=@Pan_no where Client_Id='" + lblvendor_id.Text + "'";

            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 0;
            ////cmd.Parameters.AddWithValue("@Vendor_Id", companyID);
            cmd.Parameters.AddWithValue("@Client_Name", txtvendorName.Text.Trim());
            //cmd.Parameters.AddWithValue("@Address1", txtAddress1.Text);
            cmd.Parameters.AddWithValue("@Address1", txtAddress1.Text);
            cmd.Parameters.AddWithValue("@City", cmbcity.Text.Trim());
            cmd.Parameters.AddWithValue("@pin", txtPin.Text.Trim());
            cmd.Parameters.AddWithValue("@State", cmbState.Text);
            cmd.Parameters.AddWithValue("@Com_web_site", txtWebsite.Text);
            cmd.Parameters.AddWithValue("@Com_email", txtEmail.Text);
            cmd.Parameters.AddWithValue("@Com_phone", txtPhone.Text);
            ////cmd.Parameters.AddWithValue("@EAC_NACE_Code", cmbEacCode.Text);
            cmd.Parameters.AddWithValue("@Com_Fax", txtFax.Text);
            //cmd.Parameters.AddWithValue("@Rep_Name", txtRepresentativeName.Text);
            //cmd.Parameters.AddWithValue("@Rep_Desig", txtRepresantativeDesig.Text);
            //cmd.Parameters.AddWithValue("@Rep_phone", txtRepresentativePhone.Text);
            //cmd.Parameters.AddWithValue("@Rep_email", txtRepresentativeEmail.Text);
            cmd.Parameters.AddWithValue("@Service_tax_no", txtservicetax_no.Text);
            cmd.Parameters.AddWithValue("@Pan_no", txtpanno.Text);
            ////cmd.Parameters.AddWithValue("@clientgstno", txtGstNo.Text); 
            ////cmd.Parameters.AddWithValue("@Vat_no", txtvatno.Text);
            cmd.ExecuteNonQuery();
            DbCL.Conn.Close();

            updateregaddress();

            PanelOK.Visible = true;
            lblOk.Text = "Data Update Successfully...";
            btnUpdate.Visible = false;

        }

        private void updateregaddress()
        {
            string query = "update tbl_ClientRegAddress set Address=@Address,State=@State,City=@City,Phno=@Phno,pin=@pin where Client_Id=@Client_Id";
            SqlParameter[] pram = {
                new SqlParameter("@Address",txtRegAddress.Text),
                new SqlParameter("@State",ddlRegState.Text),
                new SqlParameter("@City",ddlRegCity.Text),
                new SqlParameter("@Phno",txtRegPhno.Text),
                new SqlParameter("@pin",txtRegPin.Text),
                new SqlParameter("@Client_Id",lblvendor_id.Text),
            };
            DbCL.SPExecDB(query, pram);
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/View_client.aspx");

        }
    }
}