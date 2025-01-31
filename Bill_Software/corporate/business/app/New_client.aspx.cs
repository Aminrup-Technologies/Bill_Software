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
    public partial class WebForm15 : System.Web.UI.Page
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
                //DbCL.FillCombo(cmbcity, "select City_Name from tbl_City order by City_Name");
                DbCL.FillCombo(ddlRegState, "select State_Name from tbl_State order by State_Name");
                //DbCL.FillCombo(ddlRegCity, "select City_Name from tbl_City order by City_Name");
                DbCL.FillCombo(cmbIndustry, "select IndustryName from tbl_Industry");
                //DbCL.FillCombo(ddlplaceofSupply, "select City_Name from tbl_City order by City_Name");

                findcompanyId();
            }

        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string companyID = findcompanyId();
            //string cmdstring = "insert into tbl_Client(Client_Id,Client_Name,Address1,Address2,City,pin,State,Com_web_site,Com_email,Com_phone,Com_Fax,Rep_Name,Rep_Desig,Rep_phone,Rep_email,Service_tax_no,Pan_no)values(@Client_Id,@Client_Name,@Address1,@Address2,@City,@pin,@State,@Com_web_site,@Com_email,@Com_phone,@Com_Fax,@Rep_Name,@Rep_Desig,@Rep_phone,@Rep_email,@Service_tax_no,@Pan_no)";
            string cmdstring = "insert into tbl_Client(Client_Id,Client_Name,Address1,City,pin,State,Com_web_site,Com_email,Com_phone,Com_Fax,Service_tax_no,Pan_no,Industry,PlaceofSupply)values(@Client_Id,@Client_Name,@Address1,@City,@pin,@State,@Com_web_site,@Com_email,@Com_phone,@Com_Fax,@Service_tax_no,@Pan_no,@Industry,@PlaceofSupply)";

            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 0;
            cmd.Parameters.AddWithValue("@Client_Id", companyID);
            cmd.Parameters.AddWithValue("@Client_Name", txtvendorName.Text.Trim());
            //cmd.Parameters.AddWithValue("@Address1", txtAddress1.Text);
            cmd.Parameters.AddWithValue("@Address1", txtAddress1.Text);
            //cmd.Parameters.AddWithValue("@City", cmbcity.Text.Trim());
            cmd.Parameters.AddWithValue("@City", txtCity.Text.Trim());
            cmd.Parameters.AddWithValue("@pin", txtPin.Text.Trim());
            cmd.Parameters.AddWithValue("@State", cmbState.Text);
            cmd.Parameters.AddWithValue("@Com_web_site", txtWebsite.Text);
            cmd.Parameters.AddWithValue("@Com_email", txtEmail.Text);
            cmd.Parameters.AddWithValue("@Com_phone", txtPhone.Text);
            cmd.Parameters.AddWithValue("@Com_Fax", txtFax.Text);
            //cmd.Parameters.AddWithValue("@Rep_Name", txtRepresentativeName.Text);
            //cmd.Parameters.AddWithValue("@Rep_Desig", txtRepresantativeDesig.Text);
            //cmd.Parameters.AddWithValue("@Rep_phone", txtRepresentativePhone.Text);
           // cmd.Parameters.AddWithValue("@Rep_email", txtRepresentativeEmail.Text);
            cmd.Parameters.AddWithValue("@Service_tax_no", txtservicetax_no.Text);
            cmd.Parameters.AddWithValue("@Pan_no", txtpanno.Text);
            cmd.Parameters.AddWithValue("@Industry", cmbIndustry.Text);
            //cmd.Parameters.AddWithValue("PlaceofSupply", ddlplaceofSupply.Text);
            cmd.Parameters.AddWithValue("PlaceofSupply", txtplaceofSupply.Text);

            cmd.ExecuteNonQuery();
            DbCL.Conn.Close();

            InsertCity();
            insertRegoffAdd(companyID);

            PanelOK.Visible = true;
            lblOk.Text = "Data Save Successfully...";
            btnSave.Visible = false;

        }

        private void InsertCity()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            // Check if the combination of City_Name and State_Name already exists
            string checkQuery = "SELECT COUNT(*) FROM tbl_City WHERE City_Name = @CityName AND State_Name = @StateName";
            SqlCommand checkCmd = new SqlCommand(checkQuery, DbCL.Conn);
            checkCmd.Parameters.AddWithValue("@CityName", txtCity.Text);
            checkCmd.Parameters.AddWithValue("@StateName", cmbState.Text);

            int count = (int)checkCmd.ExecuteScalar();

            if (count > 0)
            {
                PanelOK.Visible = true;
                lblOk.Text = "Record already exists!";
            }
            else
            {
                // Insert new record
                string insertQuery = "INSERT INTO tbl_City(City_Name, State_Name) VALUES (@CityName, @StateName)";
                SqlCommand insertCmd = new SqlCommand(insertQuery, DbCL.Conn);
                insertCmd.Parameters.AddWithValue("@CityName", txtCity.Text);
                insertCmd.Parameters.AddWithValue("@StateName", cmbState.Text);
                insertCmd.ExecuteNonQuery();

                PanelOK.Visible = true;
                lblOk.Text = "Data saved successfully...";
            }
            DbCL.Conn.Close();
        }


        private void InsertRegCity()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            // Check if the combination of City_Name and State_Name already exists
            string checkQuery = "SELECT COUNT(*) FROM tbl_City WHERE City_Name = @CityName AND State_Name = @StateName";
            SqlCommand checkCmd = new SqlCommand(checkQuery, DbCL.Conn);
            checkCmd.Parameters.AddWithValue("@CityName", txtRegCity.Text);
            checkCmd.Parameters.AddWithValue("@StateName", ddlRegState.Text);

            int count = (int)checkCmd.ExecuteScalar();

            if (count > 0)
            {
                PanelOK.Visible = true;
                lblOk.Text = "Record already exists!";
            }
            else
            {
                // Insert new record
                string insertQuery = "INSERT INTO tbl_City(City_Name, State_Name) VALUES (@CityName, @StateName)";
                SqlCommand insertCmd = new SqlCommand(insertQuery, DbCL.Conn);
                insertCmd.Parameters.AddWithValue("@CityName", txtRegCity.Text);
                insertCmd.Parameters.AddWithValue("@StateName", ddlRegState.Text);
                insertCmd.ExecuteNonQuery();

                PanelOK.Visible = true;
                lblOk.Text = "Data saved successfully...";
            }
            DbCL.Conn.Close();
        }

        private void insertRegoffAdd(string companyID)
        {
            string query = "insert into tbl_ClientRegAddress(Client_Id,Address,State,City,Phno,pin) values (@Client_Id,@Address,@State,@City,@Phno,@pin)";
            SqlParameter[] pram = { 
            new SqlParameter("@Client_Id", companyID),
            new SqlParameter("@Address", txtRegAddress.Text),
            new SqlParameter("@State",ddlRegState.Text),
            new SqlParameter("@City",txtRegCity.Text),
            //new SqlParameter("@City",ddlRegCity.Text),
            new SqlParameter("@Phno",txtRegPhno.Text),
            new SqlParameter("@pin",txtRegPin.Text),
            };

            DbCL.SPExecDB(query, pram);

            InsertRegCity();
        }

        private string findcompanyId()
        {
            string ComId = "";
            string aa = "";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdString1 = "select Id,Client_Id from tbl_Client where Id=(select max(Id)from tbl_Client)";
            SqlCommand com1 = new SqlCommand(cmdString1, DbCL.Conn);
            SqlDataReader DR1 = com1.ExecuteReader();
            if (DR1.Read())
            {
                aa = DR1.GetValue(1).ToString();
                string bb = aa.Substring(2);
                int k = Convert.ToInt32(bb);
                k = k + 1;
                string q = Convert.ToString(k);
                ComId = "AD" + q;
            }
            else
            {
                ComId = "AD01";
            }

            DbCL.Conn.Close();
            lbl_nxtclientid.Text = ComId;
            return ComId;
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/New_client.aspx");
        }
    }
}