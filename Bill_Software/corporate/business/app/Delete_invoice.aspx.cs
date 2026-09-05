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
    public partial class WebForm29 : System.Web.UI.Page
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
                LoadClientCombo();
                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");

            }

        }

        private void LoadClientCombo()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            cmbvendor.Items.Clear();
            cmbvendor.Items.Add("--Select--");
            using (SqlCommand cmd = new SqlCommand("select Client_Name from tbl_Client where CompanyID=@CompanyID order by Client_Name", DbCL.Conn))
            {
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        cmbvendor.Items.Add(rdr[0].ToString());
                    }
                }
            }
            DbCL.Conn.Close();
        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string cmdstring = "";
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            sqlParams.Add(new SqlParameter("@CompanyID", CompanyContext.CurrentCompanyID));

            if (RadioButtonList1.SelectedIndex == 0)
            {
                BuindCompanyId();
                //cmdstring = "select  tbl_Invoice.ID,tbl_Invoice.Invoice_No,tbl_Invoice.Invoice_Date,tbl_Invoice.Quotation_No,tbl_Invoice.Quotation_Date,tbl_Invoice.Net_Amount,tbl_Client.Client_Name from tbl_Invoice inner join tbl_Client on tbl_Invoice.Client_ID=tbl_Client.Client_Id where tbl_Invoice.Client_ID='" + lblclientId.Text + "' and tbl_Invoice.status1='No' order by cast(tbl_Invoice.Invoice_Date as datetime) desc";
                cmdstring = "select a.ID,a.Invoice_No,a.Invoice_Date,a.Quotation_No,a.Quotation_Date,a.Net_Amount,a.mailDate,a.sub_total,(a.Net_Amount-a.sub_total) as Gst,b.Client_Name,c.PServiceName from tbl_Invoice as a left outer join tbl_QuoPriSerTogather as c on a.Quotation_No=c.qutno AND c.CompanyID=@CompanyID left outer join tbl_Client as b on b.Client_Id=a.Client_ID AND b.CompanyID=@CompanyID  where a.CompanyID=@CompanyID and a.Client_ID=@ClientId order by a.ID desc";
                sqlParams.Add(new SqlParameter("@ClientId", lblclientId.Text));
                Buinddatagrid(cmdstring, sqlParams);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                //cmdstring = "select  tbl_Invoice.ID,tbl_Invoice.Invoice_No,tbl_Invoice.Invoice_Date,tbl_Invoice.Quotation_No,tbl_Invoice.Quotation_Date,tbl_Invoice.Net_Amount,tbl_Client.Client_Name from tbl_Invoice inner join tbl_Client on tbl_Invoice.Client_ID=tbl_Client.Client_Id where cast(tbl_Invoice.Invoice_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' and tbl_Invoice.status1='No' order by cast(tbl_Invoice.Invoice_Date as datetime) desc";
                cmdstring = "select a.ID,a.Invoice_No,a.Invoice_Date,a.Quotation_No,a.Quotation_Date,a.Net_Amount,a.mailDate,a.sub_total,(a.Net_Amount-a.sub_total) as Gst,b.Client_Name,c.PServiceName from tbl_Invoice as a left outer join tbl_QuoPriSerTogather as c on a.Quotation_No=c.qutno AND c.CompanyID=@CompanyID left outer join tbl_Client as b on b.Client_Id=a.Client_ID AND b.CompanyID=@CompanyID  where a.CompanyID=@CompanyID and cast(a.Invoice_Date as datetime) between @FromDate and @ToDate order by a.ID desc";
                sqlParams.Add(new SqlParameter("@FromDate", txttodate.Text));
                sqlParams.Add(new SqlParameter("@ToDate", txtfromDate.Text));

                Buinddatagrid(cmdstring, sqlParams);
            }
            else
            {
                BuindCompanyId();
                //cmdstring = "select  tbl_Invoice.ID,tbl_Invoice.Invoice_No,tbl_Invoice.Invoice_Date,tbl_Invoice.Quotation_No,tbl_Invoice.Quotation_Date,tbl_Invoice.Net_Amount,tbl_Client.Client_Name from tbl_Invoice inner join tbl_Client on tbl_Invoice.Client_ID=tbl_Client.Client_Id where tbl_Invoice.Client_ID='" + lblclientId.Text + "' and cast(tbl_Invoice.Invoice_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' and tbl_Invoice.status1='No' order by cast(tbl_Invoice.Invoice_Date as datetime) desc";
                cmdstring = "select a.ID,a.Invoice_No,a.Invoice_Date,a.Quotation_No,a.Quotation_Date,a.Net_Amount,a.mailDate,a.sub_total,(a.Net_Amount-a.sub_total) as Gst,b.Client_Name,c.PServiceName from tbl_Invoice as a left outer join tbl_QuoPriSerTogather as c on a.Quotation_No=c.qutno AND c.CompanyID=@CompanyID left outer join tbl_Client as b on b.Client_Id=a.Client_ID AND b.CompanyID=@CompanyID  where a.CompanyID=@CompanyID and a.Client_ID=@ClientId and cast(a.Invoice_Date as datetime) between @FromDate and @ToDate order by a.ID desc";
                sqlParams.Add(new SqlParameter("@ClientId", lblclientId.Text));
                sqlParams.Add(new SqlParameter("@FromDate", txttodate.Text));
                sqlParams.Add(new SqlParameter("@ToDate", txtfromDate.Text));

                Buinddatagrid(cmdstring, sqlParams);
            }
            btnSertch.Visible = false;

        }
        private void Buinddatagrid(string cmdstring, List<SqlParameter> sqlParams)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            AddParams(cmd, sqlParams);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                DbCL.Conn.Close();
                Buinddatagrid1(cmdstring, sqlParams);
            }
            else
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "No Data Found...";
                DbCL.Conn.Close();
            }
        }

        private void Buinddatagrid1(string cmdstring, List<SqlParameter> sqlParams)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            SqlCommand cmd1 = new SqlCommand(cmdstring, DbCL.Conn);
            AddParams(cmd1, sqlParams);
            DataList1.DataSource = cmd1.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();

        }

        private static void AddParams(SqlCommand cmd, List<SqlParameter> sqlParams)
        {
            if (sqlParams == null) return;
            foreach (SqlParameter p in sqlParams)
            {
                cmd.Parameters.Add(new SqlParameter(p.ParameterName, p.Value));
            }
        }

        private void BuindCompanyId()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Id from tbl_Client where Client_Name=@ClientName AND CompanyID=@CompanyID";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.Parameters.AddWithValue("@ClientName", cmbvendor.Text);
            cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblclientId.Text = re["Client_Id"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/Delete_invoice.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Invoice_No = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Delete")
            {
                if (!updatestock1(Invoice_No))
                {
                    PanelError.Visible = true;
                    lblErrorMsg.Text = "No Data Found...";
                    return;
                }

                Dictionary<string, object> delParams = new Dictionary<string, object>
                {
                    { "@Invoice_No", Invoice_No },
                    { "@CompanyID", CompanyContext.CurrentCompanyID }
                };

                DbCL.executeRdrNew("delete from tbl_Invoice where Invoice_No=@Invoice_No AND CompanyID=@CompanyID", delParams);
                DbCL.executeRdrNew("delete from tbl_Invoice_details where Invoice_No=@Invoice_No AND CompanyID=@CompanyID", delParams);
                DbCL.executeRdrNew("delete from tbl_InvSiteAddress where invoice_no=@Invoice_No AND CompanyID=@CompanyID", delParams);

                PanelOK.Visible = true;
                lblOk.Text = "Data Deleted Successfully...";
                DataList1.Visible = false;
            }
        }

        private bool updatestock1(string Invoice_No)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Quotation_No from tbl_Invoice where Invoice_No=@Invoice_No AND CompanyID=@CompanyID";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.Parameters.AddWithValue("@Invoice_No", Invoice_No);
            cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                string quotation_no = re["Quotation_No"].ToString();
                re.Close();
                Dictionary<string, object> qParams = new Dictionary<string, object>
                {
                    { "@Quotation_no", quotation_no },
                    { "@CompanyID", CompanyContext.CurrentCompanyID }
                };
                DbCL.executeRdrNew("update tbl_Quotation set Status2='No' where  Quotation_no=@Quotation_no AND CompanyID=@CompanyID", qParams);
                UpdateQuotationProductStatus(Invoice_No, quotation_no);
                //updatestock(quotation_no);
                DbCL.Conn.Close();
                return true;
            }
            DbCL.Conn.Close();
            return false;
        }

        private void UpdateQuotationProductStatus(string invoice_No, string quotation_no)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Product_id,Product_name from tbl_Invoice_details where Quotation_no=@Quotation_no and Invoice_No=@Invoice_No AND CompanyID=@CompanyID";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.Parameters.AddWithValue("@Quotation_no", quotation_no);
            cmd.Parameters.AddWithValue("@Invoice_No", invoice_No);
            cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
            SqlDataReader re = cmd.ExecuteReader();
            DataTable dtItems = new DataTable();
            dtItems.Load(re);
            DbCL.Conn.Close();

            foreach (DataRow row in dtItems.Rows)
            {
                string Product_id = row["Product_id"].ToString();
                string Product_name = row["Product_name"].ToString();
                Dictionary<string, object> itemParams = new Dictionary<string, object>
                {
                    { "@Quotation_no", quotation_no },
                    { "@Product_id", Product_id },
                    { "@Product_name", Product_name },
                    { "@CompanyID", CompanyContext.CurrentCompanyID }
                };
                DbCL.executeRdrNew("update tbl_Quotaion_details set InvStatus='No' where  Quotation_no=@Quotation_no and  Product_id=@Product_id and Product_name=@Product_name AND CompanyID=@CompanyID", itemParams);
            }
        }

        //private void updatestock(string quotation_no)
        //{
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    string cmdstring = "select Product_id,Product_name,Quantity from tbl_Quotaion_details where Quotation_no='" + quotation_no + "'";
        //    SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
        //    SqlDataReader re = cmd.ExecuteReader();
        //    while (re.Read())
        //    {
        //        string product_code = re["Product_id"].ToString();
        //        string Product_name = re["Product_name"].ToString();
        //        string Quantity = re["Quantity"].ToString();
        //        updatestock1(product_code, Product_name, Quantity);
        //    }
        //    DbCL.Conn.Close();
        //}
        //private void updatestock1(string product_code, string Product_name, string Quantity)
        //{
        //    DbCL.executeRdr("update tbl_stock set Quantity=(cast(Quantity as int)+'" + Quantity.ToString() + "') where Product_id='" + product_code.ToString() + "' and Product_name='" + Product_name.ToString() + "'");
        //}
    }
}
