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
    public partial class WebForm5 : System.Web.UI.Page
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
                DbCL.FillCombo(cmbState, "select State_Name from tbl_State order by State_Name");
                DbCL.FillCombo(cmbcity, "select City_Name from tbl_City order by City_Name");

            }

        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string companyID = findcompanyId();
            string cmdstring = "insert into tbl_Vendor(Vendor_Id,Vendor_Name,Address1,Address2,City,pin,State,Com_web_site,Com_email,Com_phone,Com_Fax,Rep_Name,Rep_Desig,Rep_phone,Rep_email,Service_tax_No,Pan_No,Vat_No)values(@Vendor_Id,@Vendor_Name,@Address1,@Address2,@City,@pin,@State,@Com_web_site,@Com_email,@Com_phone,@Com_Fax,@Rep_Name,@Rep_Desig,@Rep_phone,@Rep_email,@Service_tax_No,@Pan_No,@Vat_No)";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 0;
            cmd.Parameters.AddWithValue("@Vendor_Id", companyID);
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
            lblOk.Text = "Data Save Successfully...";
            btnSave.Visible = false;

        }
        private string findcompanyId()
        {
            string ComId = "";
            string aa = "";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdString1 = "select Id,Vendor_Id from tbl_Vendor where Id=(select max(Id)from tbl_Vendor)";
            SqlCommand com1 = new SqlCommand(cmdString1, DbCL.Conn);
            SqlDataReader DR1 = com1.ExecuteReader();
            if (DR1.Read())
            {
                aa = DR1.GetValue(1).ToString();
                string bb = aa.Substring(5);
                int k = Convert.ToInt32(bb);
                k = k + 1;
                string q = Convert.ToString(k);
                ComId = "VEN00" + q;
            }
            else
            {
                ComId = "VEN001";
            }

            DbCL.Conn.Close();
            return ComId;
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/New_vendor.aspx");

        }
    }
}