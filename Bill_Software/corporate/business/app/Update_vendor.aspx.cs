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
    public partial class WebForm12 : System.Web.UI.Page
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
                string Vendor_Id = Request.QueryString["Vendor_Id"];
                lblvendor_id.Text = Vendor_Id.ToString();
                DbCL.FillCombo(cmbState, "select State_Name from tbl_State order by State_Name");
                DbCL.FillCombo(cmbcity, "select City_Name from tbl_City order by City_Name");
                Binddate();
            }

        }

        private void Binddate()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Vendor where Vendor_Id='"+ lblvendor_id.Text +"'";
            SqlCommand cmd = new SqlCommand(cmdstring,DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if(re.Read())
            {
                txtvendorName.Text = re["Vendor_Name"].ToString();
                txtAddress1.Text = re["Address1"].ToString();
                txtAddress2.Text = re["Address2"].ToString();
                cmbcity.Text = re["City"].ToString();
                txtPin.Text = re["pin"].ToString();
                cmbState.Text = re["State"].ToString();
                txtWebsite.Text = re["Com_web_site"].ToString();
                txtEmail.Text = re["Com_email"].ToString();
                txtPhone.Text = re["Com_phone"].ToString();
                txtFax.Text = re["Com_Fax"].ToString();
                txtRepresentativeName.Text = re["Rep_Name"].ToString();
                txtRepresantativeDesig.Text = re["Rep_Desig"].ToString();
                txtRepresentativePhone.Text = re["Rep_phone"].ToString();
                txtRepresentativeEmail.Text = re["Rep_email"].ToString();
                txtservicetaxNo.Text=re["Service_tax_No"].ToString();
                txtpanNo.Text=re["Pan_No"].ToString();
                txtvat.Text = re["Vat_No"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            string cmdstring = "update tbl_Vendor set Vendor_Name=@Vendor_Name,Address1=@Address1,Address2=@Address2,City=@City,pin=@pin,State=@State,Com_web_site=@Com_web_site,Com_email=@Com_email,Com_phone=@Com_phone,Com_Fax=@Com_Fax,Rep_Name=@Rep_Name,Rep_Desig=@Rep_Desig,Rep_phone=@Rep_phone,Rep_email=@Rep_email,Service_tax_No=@Service_tax_No,Pan_No=@Pan_No,Vat_No=@Vat_No where Vendor_Id='" + lblvendor_id.Text + "'";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 0;
            //cmd.Parameters.AddWithValue("@Vendor_Id", companyID);
            cmd.Parameters.AddWithValue("@Vendor_Name", txtvendorName.Text.Trim());
            cmd.Parameters.AddWithValue("@Address1", txtAddress1.Text);
            cmd.Parameters.AddWithValue("@Address2", txtAddress2.Text);
            cmd.Parameters.AddWithValue("@City", cmbcity.Text.Trim());
            cmd.Parameters.AddWithValue("@pin", txtPin.Text.Trim());
            cmd.Parameters.AddWithValue("@State", cmbState.Text);
            cmd.Parameters.AddWithValue("@Com_web_site", txtWebsite.Text);
            cmd.Parameters.AddWithValue("@Com_email", txtEmail.Text);
            cmd.Parameters.AddWithValue("@Com_phone", txtPhone.Text);
            //cmd.Parameters.AddWithValue("@EAC_NACE_Code", cmbEacCode.Text);
            cmd.Parameters.AddWithValue("@Com_Fax", txtFax.Text);
            cmd.Parameters.AddWithValue("@Rep_Name", txtRepresentativeName.Text);
            cmd.Parameters.AddWithValue("@Rep_Desig", txtRepresantativeDesig.Text);
            cmd.Parameters.AddWithValue("@Rep_phone", txtRepresentativePhone.Text);
            cmd.Parameters.AddWithValue("@Rep_email", txtRepresentativeEmail.Text);
            cmd.Parameters.AddWithValue("@Service_tax_No", txtservicetaxNo.Text);
            cmd.Parameters.AddWithValue("@Pan_No", txtpanNo.Text);
            cmd.Parameters.AddWithValue("@Vat_No", txtvat.Text);
            cmd.ExecuteNonQuery();
            DbCL.Conn.Close();
            PanelOK.Visible = true;
            lblOk.Text = "Data Update Successfully...";
            btnUpdate.Visible = false;

        }

        protected void btnEdit_Click(object sender, EventArgs e)
        {
            txtAddress1.Enabled = true;
            txtAddress2.Enabled = true;
            txtEmail.Enabled = true;
            txtFax.Enabled = true;
            txtPhone.Enabled = true;
            txtPin.Enabled = true;
            txtRepresantativeDesig.Enabled = true;
            txtRepresentativeEmail.Enabled = true;
            txtRepresentativeName.Enabled = true;
            txtRepresentativePhone.Enabled = true;
            txtvendorName.Enabled = true;
            txtWebsite.Enabled = true;
            txtservicetaxNo.Enabled = true;
            txtpanNo.Enabled = true;
            txtvat.Enabled = true;
            cmbcity.Enabled = true;
            cmbState.Enabled = true;
            btnUpdate.Visible = true;
            btnEdit.Visible = false;

        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/View_vendor.aspx");

        }

        

        
    }
}