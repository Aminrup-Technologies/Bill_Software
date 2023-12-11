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
    public partial class WebForm59 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dtrep = new DataTable();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                DbCL.FillCombo(cmbvendor, "select Client_Name from tbl_Client order by Client_Name");
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            BuindCompanyId();
            DbCL.executeRdr("insert into tbl_representative(Copany_Id,Representative_name,Designation,Phone_no,Email,RepTitle,RepLastName) values ('" + lblclientId.Text + "','"+ txtRepresentativeName.Text+"','"+ txtRepresantativeDesig.Text +"','"+ txtRepresentativePhone.Text +"','"+ txtRepresentativeEmail.Text + "','" + ddlRepTitle.Text + "','" + txtLastName.Text + "')");
            PanelOK.Visible = true;
            lblOk.Text = "Data Save Successfully...";
            
            cmbvendor.SelectedIndex = 0;
            txtRepresantativeDesig.Text = "";
            txtRepresentativeEmail.Text = "";
            txtRepresentativeName.Text = "";
            txtRepresentativePhone.Text = "";
            txtLastName.Text = "";
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

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string ID = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Delete")
            {
                DbCL.executeRdr("delete from tbl_representative where ID='" + ID + "'");
                PanelOK.Visible = true;
                lblOk.Text = "Data Deleted Successfully...";
            }
            if (e.CommandName == "Edit")
            {
                
            }
            Binddata();
        }

        private void Binddata()
        {
            string query = "select ID,Copany_Id,Representative_name,Designation,Phone_no,Email,RepTitle,RepLastName from tbl_representative oredr by Representative_name asc";
            SqlParameter[] pram = { };
            dtrep=DbCL.SPreturn_dt(query, pram);
            if (dtrep.Rows.Count>0)
            {
                DataList1.DataSource = dtrep;
                DataList1.DataBind();
            }
        }
    }
}