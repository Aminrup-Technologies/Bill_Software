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
    public partial class WebForm32 : System.Web.UI.Page
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
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");

            }
        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string baseQuery = @"SELECT a.ID, a.Invoice_No, a.Invoice_Date, a.Quotation_No, a.Quotation_Date, a.Net_Amount, a.mail_Date, a.subtotal, (a.Net_Amount - a.subtotal) AS Gst, b.Client_Name, c.PServiceName 
                FROM tbl_Proforma AS a 
                LEFT OUTER JOIN tbl_QuoPriSerTogather AS c ON a.Quotation_No = c.qutno 
                LEFT OUTER JOIN tbl_Client AS b ON b.Client_Id = a.Client_ID 
                WHERE 1=1";

            if (RadioButtonList1.SelectedIndex == 0 || RadioButtonList1.SelectedIndex == 2)
            {
                BuindCompanyId();
                baseQuery += " AND a.Client_ID = @ClientId";
            }
            if (RadioButtonList1.SelectedIndex == 1 || RadioButtonList1.SelectedIndex == 2)
            {
                baseQuery += " AND CAST(a.Invoice_Date AS datetime) BETWEEN @FromDate AND @ToDate";
            }
            baseQuery += " ORDER BY a.ID DESC";

            Buinddatagrid(baseQuery);
            btnSertch.Visible = false;
        }
        private void Buinddatagrid(string cmdstring)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(cmdstring, conn))
            {
                // Add parameters based on what was set in btnSertch_Click
                if (cmdstring.Contains("@ClientId"))
                    cmd.Parameters.AddWithValue("@ClientId", lblclientId.Text);
                if (cmdstring.Contains("@FromDate"))
                {
                    cmd.Parameters.AddWithValue("@FromDate", txttodate.Text);
                    cmd.Parameters.AddWithValue("@ToDate", txtfromDate.Text);
                }

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        DataList1.DataSource = dt;
                        DataList1.DataBind();
                    }
                    else
                    {
                        PanelError.Visible = true;
                        lblErrorMsg.Text = "No Data Found...";
                    }
                }
            }
        }

        private void BuindCompanyId()
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                string cmdstring = "SELECT Client_Id FROM tbl_Client WHERE Client_Name = @Name AND CompanyID = @CompanyID";
                using (SqlCommand cmd = new SqlCommand(cmdstring, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", cmbvendor.Text);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        lblclientId.Text = result.ToString();
                    }
                }
            }
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/Seartch_proforma.aspx");
        }
    }
}