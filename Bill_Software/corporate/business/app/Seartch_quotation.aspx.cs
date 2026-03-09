using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm24 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dtmain = new DataTable();

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
            BuindCompanyId();

            // We changed LEFT JOIN to OUTER APPLY to grab only the TOP 1 latest service!
            string cmdstring = @"
                SELECT 
                    q.ID,
                    q.Quotation_no,
                    q.Quotation_date,
                    q.service_tax1,
                    q.sub_total,
                    q.Gross,
                    q.Service_tax,
                    q.Net_amount,
                    q.mailStatusDate,
                    s.PServiceName AS Services,
                    c.Client_Name
                FROM tbl_Quotation q
                LEFT JOIN tbl_Client c 
                    ON q.Client_Id = c.Client_Id
                OUTER APPLY (
                    SELECT TOP 1 PServiceName 
                    FROM tbl_QuoPriSerTogather 
                    WHERE qutno = q.Quotation_no 
                    ORDER BY TimeStamp DESC
                ) s
                WHERE q.RecordType = 'Quotation'
            ";

            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;

            if (RadioButtonList1.SelectedIndex == 0)
            {
                cmdstring += " AND q.Client_Id = @ClientId";
                cmd.Parameters.AddWithValue("@ClientId", lblclientId.Text);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                cmdstring += " AND CAST(q.Quotation_date AS DATETIME) BETWEEN @FromDate AND @ToDate";
                cmd.Parameters.AddWithValue("@FromDate", txtfromDate.Text);
                cmd.Parameters.AddWithValue("@ToDate", txttodate.Text);
            }
            else
            {
                cmdstring += " AND q.Client_Id = @ClientId";
                cmdstring += " AND CAST(q.Quotation_date AS DATETIME) BETWEEN @FromDate AND @ToDate";

                cmd.Parameters.AddWithValue("@ClientId", lblclientId.Text);
                cmd.Parameters.AddWithValue("@FromDate", txtfromDate.Text);
                cmd.Parameters.AddWithValue("@ToDate", txttodate.Text);
            }

            // Notice we completely removed the GROUP BY clause because it's no longer needed!
            cmdstring += " ORDER BY CAST(q.Quotation_date AS DATE) DESC";

            cmd.CommandText = cmdstring;

            BuinddatagridNew(cmd);

            btnSertch.Visible = false;
        }

        protected void btnSertch_Click_OLD(object sender, EventArgs e)
        {
            BuindCompanyId();

            string cmdstring = @"
                SELECT 
                    q.ID,
                    q.Quotation_no,
                    q.Quotation_date,
                    q.service_tax1,
                    q.sub_total,
                    q.Gross,
                    q.Service_tax,
                    q.Net_amount,
                    q.mailStatusDate,
                    STRING_AGG(s.PServiceName, ', ') AS Services,
                    c.Client_Name
                FROM tbl_Quotation q
                LEFT JOIN tbl_Client c 
                    ON q.Client_Id = c.Client_Id
                LEFT JOIN tbl_QuoPriSerTogather s 
                    ON s.qutno = q.Quotation_no
                WHERE q.RecordType = 'Quotation'
            ";

            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;

            if (RadioButtonList1.SelectedIndex == 0)
            {
                cmdstring += " AND q.Client_Id = @ClientId";
                cmd.Parameters.AddWithValue("@ClientId", lblclientId.Text);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                cmdstring += " AND CAST(q.Quotation_date AS DATETIME) BETWEEN @FromDate AND @ToDate";
                // BUG FIX: Swapped txtfromDate and txttodate to match the correct parameters
                cmd.Parameters.AddWithValue("@FromDate", txtfromDate.Text);
                cmd.Parameters.AddWithValue("@ToDate", txttodate.Text);
            }
            else
            {
                cmdstring += " AND q.Client_Id = @ClientId";
                cmdstring += " AND CAST(q.Quotation_date AS DATETIME) BETWEEN @FromDate AND @ToDate";

                cmd.Parameters.AddWithValue("@ClientId", lblclientId.Text);
                // BUG FIX: Swapped txtfromDate and txttodate to match the correct parameters
                cmd.Parameters.AddWithValue("@FromDate", txtfromDate.Text);
                cmd.Parameters.AddWithValue("@ToDate", txttodate.Text);
            }

            cmdstring += @"
            GROUP BY
                q.ID,
                q.Quotation_no,
                q.Quotation_date,
                q.service_tax1,
                q.sub_total,
                q.Gross,
                q.Service_tax,
                q.Net_amount,
                q.mailStatusDate,
                c.Client_Name
            ORDER BY CAST(q.Quotation_date AS DATE) DESC";

            cmd.CommandText = cmdstring;

            BuinddatagridNew(cmd);

            btnSertch.Visible = false;
        }

        // Keep using your brilliant newer method!
        public void BuinddatagridNew(SqlCommand cmd)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                cmd.Connection = con;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                da.Fill(dt);

                DataList1.DataSource = dt;
                DataList1.DataBind();
            }
        }

        private void BuindCompanyId()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            // BUG FIX: Parameterized this query to prevent SQL Injection and syntax errors from quotes in names
            string cmdstring = "select Client_Id from tbl_Client where Client_Name = @ClientName";
            using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@ClientName", cmbvendor.Text);

                // BUG FIX: Wrapped SqlDataReader in a Using block to ensure it closes properly
                using (SqlDataReader re = cmd.ExecuteReader())
                {
                    if (re.Read())
                    {
                        lblclientId.Text = re["Client_Id"].ToString();
                    }
                }
            }
            DbCL.Conn.Close();
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/Seartch_quotation.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string ID = Convert.ToString(e.CommandArgument);
            string qdate = buindalldata(ID);

            // BUG FIX: Safe parsing of the date string to prevent crashes if qdate is empty

            DateTime fromdate;
            if (DateTime.TryParse(qdate, out fromdate))
            {
                // Cleaned up the target date logic
                DateTime todate = new DateTime(2018, 6, 12);

                if (e.CommandName == "View")
                {
                    if (fromdate > todate)
                    {
                        Response.Redirect("/corporate/business/print/NewQuotation.aspx?ID=" + ID);
                    }
                    else
                    {
                        Response.Redirect("/corporate/business/print/Quotation.aspx?ID=" + ID);
                    }
                }
            }
        }

        private string buindalldata(string ID)
        {
            string qdate = "";
            string query = "select Quotation_no,Quotation_date,Client_Id,sub_total,Service_tax,Net_amount,cgstOrsgst,igst from tbl_Quotation where ID=@ID";
            SqlParameter[] pram = {
                // Formatting consistency check
                new SqlParameter("@ID", ID)
            };

            dtmain = DbCL.SPreturn_dt(query, pram);

            if (dtmain.Rows.Count > 0)
            {
                qdate = dtmain.Rows[0]["Quotation_date"].ToString();
            }
            return qdate;
        }
    }
}