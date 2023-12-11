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
    public partial class WebForm7 : System.Web.UI.Page
    {
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
            DbCL.executeRdr("delete from tbl_employee where Company_ID='" + ComID + "'");
            Button1.Visible = false;
            PanelOK.Visible = true;
            lblOk.Text = "Data Deleted Sucessfully...";
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
    }
}