using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm30 : System.Web.UI.Page
    {
        private string ConnString => ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                // Ponytail #3 + #1: Parameterized, CompanyID-scoped
                using (var cn = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand("SELECT Client_Name FROM tbl_Client WHERE CompanyID = @CompanyID ORDER BY Client_Name", cn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        cmbvendor.DataSource = dt;
                        cmbvendor.DataTextField = "Client_Name";
                        cmbvendor.DataBind();
                    }
                }
                cmbvendor.Items.Insert(0, new ListItem("--Select Client--", ""));

                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtinvoiceDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            }
        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                // Only Client
                string clientId = GetClientId(cmbvendor.Text);
                if (string.IsNullOrEmpty(clientId)) { ShowError("Client not found."); return; }
                cmdstring = "SELECT qut.PServiceName, qut.ID, qut.service_tax1, qut.sub_total, qut.Quotation_no, qut.Quotation_date, qut.Gross, qut.Service_tax, qut.Net_amount, qut.mailStatusDate, c.Client_Name FROM tbl_Quotation qut LEFT OUTER JOIN tbl_Client c ON qut.Client_Id = c.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather qut2 ON qut2.qutno = qut.Quotation_no WHERE qut.Client_Id = @ClientId AND qut.CompanyID = @CompanyID ORDER BY qut.ID DESC";
                Binddatagrid1(cmdstring, clientId, null, null);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                // Only Date
                cmdstring = "SELECT qut.PServiceName, qut.ID, qut.service_tax1, qut.sub_total, qut.Quotation_no, qut.Quotation_date, qut.Gross, qut.Service_tax, qut.Net_amount, qut.mailStatusDate, c.Client_Name FROM tbl_Quotation qut LEFT OUTER JOIN tbl_Client c ON qut.Client_Id = c.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather qut2 ON qut2.qutno = qut.Quotation_no WHERE CAST(qut.Quotation_date AS datetime) BETWEEN @FromDate AND @ToDate AND qut.CompanyID = @CompanyID ORDER BY qut.ID DESC";
                Binddatagrid1(cmdstring, null, txttodate.Text, txtfromDate.Text);
            }
            else
            {
                // Client & Date
                string clientId = GetClientId(cmbvendor.Text);
                if (string.IsNullOrEmpty(clientId)) { ShowError("Client not found."); return; }
                cmdstring = "SELECT qut.PServiceName, qut.ID, qut.service_tax1, qut.sub_total, qut.Quotation_no, qut.Quotation_date, qut.Gross, qut.Service_tax, qut.Net_amount, qut.mailStatusDate, c.Client_Name FROM tbl_Quotation qut LEFT OUTER JOIN tbl_Client c ON qut.Client_Id = c.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather qut2 ON qut2.qutno = qut.Quotation_no WHERE qut.Client_Id = @ClientId AND CAST(qut.Quotation_date AS datetime) BETWEEN @FromDate AND @ToDate AND qut.CompanyID = @CompanyID ORDER BY qut.ID DESC";
                Binddatagrid1(cmdstring, clientId, txttodate.Text, txtfromDate.Text);
            }
            btnSertch.Visible = false;
        }

        private void Binddatagrid1(string cmdstring, string clientId, string fromDate, string toDate)
        {
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(cmdstring, cn))
            {
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                if (clientId != null) cmd.Parameters.AddWithValue("@ClientId", clientId);
                if (fromDate != null) cmd.Parameters.AddWithValue("@FromDate", fromDate);
                if (toDate != null) cmd.Parameters.AddWithValue("@ToDate", toDate);
                cn.Open();

                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        // Has results — re-bind the DataList
                        rdr.Close();
                        using (var cmd2 = new SqlCommand(cmdstring, cn))
                        {
                            cmd2.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                            if (clientId != null) cmd2.Parameters.AddWithValue("@ClientId", clientId);
                            if (fromDate != null) cmd2.Parameters.AddWithValue("@FromDate", fromDate);
                            if (toDate != null) cmd2.Parameters.AddWithValue("@ToDate", toDate);
                            DataList1.DataSource = cmd2.ExecuteReader();
                            DataList1.DataBind();
                        }
                    }
                    else
                    {
                        ShowError("No Data Found...");
                    }
                }
            }
        }

        private string GetClientId(string clientName)
        {
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand("SELECT Client_Id FROM tbl_Client WHERE Client_Name = @Name AND CompanyID = @CompanyID", cn))
            {
                cmd.Parameters.AddWithValue("@Name", clientName);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                cn.Open();
                var result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : null;
            }
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/Add_proforma.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            if (e.CommandName == "Select")
            {
                string Quotation_no = Convert.ToString(e.CommandArgument);
                Panel1.Visible = true;
                Binddetails(Quotation_no);
            }
        }

        private void Binddetails(string Quotation_no)
        {
            // Ponytail #3 + #1: Parameterized, CompanyID-scoped
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand("SELECT * FROM tbl_Quotation WHERE Quotation_no = @QNo AND CompanyID = @CompanyID", cn))
            {
                cmd.Parameters.AddWithValue("@QNo", Quotation_no);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                cn.Open();
                using (var re = cmd.ExecuteReader())
                {
                    if (re.Read())
                    {
                        lblClient_Id.Text = re["Client_Id"].ToString();
                        lblQuotation_no.Text = re["Quotation_no"].ToString();
                        lblQuotation_date.Text = re["Quotation_date"].ToString();
                        lblGross_amount.Text = re["Gross"].ToString();
                        lblservicetax.Text = re["Service_tax"].ToString();
                        lblNet_amount.Text = re["Net_amount"].ToString();
                        lblsubtotal.Text = re["sub_total"].ToString();
                    }
                }
            }
            BindclientName();
        }

        private string BindInvoiceNo()
        {
            string c = lblClientName.Text.Trim();
            string f = c.Substring(0, 1);
            f = "PINV/" + f + "/";
            string ss = findmonth();
            f = f + ss;
            int j = idreturn();
            j = j + 1;
            f = f + j.ToString();
            return f;
        }

        private int idreturn()
        {
            int b = 0;
            string date1 = txtinvoiceDate.Text;
            string date2 = date1.Substring(3, 3);
            string date3 = date1.Substring(7, 4);
            string date5, date6;
            if (date2 == "Jan" || date2 == "Feb" || date2 == "Mar")
            {
                date5 = "31-Mar-" + (Convert.ToInt32(date3) - 1).ToString();
                date6 = "31-Mar-" + date3;
            }
            else
            {
                date5 = "31-Mar-" + date3;
                date6 = "31-Mar-" + (Convert.ToInt32(date3) + 1).ToString();
            }

            // Ponytail #3 + #1: Parameterized, CompanyID-scoped
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(
                "SELECT Sl_no FROM tbl_Proforma WHERE ID = (SELECT MAX(ID) FROM tbl_Proforma WHERE CAST(Invoice_Date AS datetime) BETWEEN @Date5 AND @Date6 AND CompanyID = @CompanyID)", cn))
            {
                cmd.Parameters.AddWithValue("@Date5", date5);
                cmd.Parameters.AddWithValue("@Date6", date6);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                cn.Open();
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    b = Convert.ToInt32(result);
            }
            return b;
        }

        private string findmonth()
        {
            string a = txtinvoiceDate.Text.Substring(3, 3);
            string b = txtinvoiceDate.Text.Substring(9, 2);
            if (a == "Jan" || a == "Feb" || a == "Mar")
                return (Convert.ToInt32(b) - 1).ToString() + "-" + b + "/";
            return b + "-" + (Convert.ToInt32(b) + 1).ToString() + "/";
        }

        private void BindclientName()
        {
            // Ponytail #3 + #1: Parameterized, CompanyID-scoped
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand("SELECT Client_Name FROM tbl_Client WHERE Client_Id = @ClientId AND CompanyID = @CompanyID", cn))
            {
                cmd.Parameters.AddWithValue("@ClientId", lblClient_Id.Text);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                cn.Open();
                using (var re = cmd.ExecuteReader())
                {
                    if (re.Read())
                        lblClientName.Text = re["Client_Name"].ToString();
                }
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            string invoice_no = BindInvoiceNo();
            int j = idreturn() + 1;

            // Ponytail #3 + #1: Parameterized, CompanyID-scoped, using transaction
            using (var cn = new SqlConnection(ConnString))
            {
                cn.Open();
                using (var tran = cn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(
                            "INSERT INTO tbl_Proforma (Invoice_No, Invoice_Date, Quotation_No, Quotation_Date, Client_ID, Gross, Service_Tax, Net_Amount, Sl_no, subtotal, CompanyID) " +
                            "VALUES (@InvoiceNo, @InvoiceDate, @QuotationNo, @QuotationDate, @ClientId, @Gross, @ServiceTax, @NetAmount, @SlNo, @Subtotal, @CompanyID)", cn, tran))
                        {
                            cmd.Parameters.AddWithValue("@InvoiceNo", invoice_no);
                            cmd.Parameters.AddWithValue("@InvoiceDate", txtinvoiceDate.Text);
                            cmd.Parameters.AddWithValue("@QuotationNo", lblQuotation_no.Text);
                            cmd.Parameters.AddWithValue("@QuotationDate", lblQuotation_date.Text);
                            cmd.Parameters.AddWithValue("@ClientId", lblClient_Id.Text);
                            cmd.Parameters.AddWithValue("@Gross", lblGross_amount.Text);
                            cmd.Parameters.AddWithValue("@ServiceTax", lblservicetax.Text);
                            cmd.Parameters.AddWithValue("@NetAmount", lblNet_amount.Text);
                            cmd.Parameters.AddWithValue("@SlNo", j);
                            cmd.Parameters.AddWithValue("@Subtotal", lblsubtotal.Text);
                            cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new SqlCommand(
                            "UPDATE tbl_Quotation SET Status1 = 'Yes' WHERE Quotation_no = @QNo AND CompanyID = @CompanyID", cn, tran))
                        {
                            cmd.Parameters.AddWithValue("@QNo", lblQuotation_no.Text);
                            cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                            cmd.ExecuteNonQuery();
                        }

                        // Revenue Milestone Logic: PI generation marks visit as Productive
                        try
                        {
                            decimal netAmt = Convert.ToDecimal(lblNet_amount.Text);
                            using (var cmdVisit = new SqlCommand(
                                @"UPDATE v SET v.IsProductive = 1, 
                                      v.RevenueRealized = ISNULL(v.RevenueRealized, 0) + @NetAmt
                                  FROM tbl_SalesVisitReport v
                                  INNER JOIN tbl_Quotation q ON v.Id = q.VisitId AND q.CompanyID = @CompanyID
                                  WHERE q.Quotation_no = @QNo AND v.CompanyID = @CompanyID", cn, tran))
                            {
                                cmdVisit.Parameters.AddWithValue("@NetAmt", netAmt);
                                cmdVisit.Parameters.AddWithValue("@QNo", lblQuotation_no.Text);
                                cmdVisit.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                                cmdVisit.ExecuteNonQuery();
                            }
                        }
                        catch { /* Soft catch: milestone logging failure must not crash PI creation */ }

                        tran.Commit();
                        Button1.Visible = false;
                        PanelOK.Visible = true;
                        lblOk.Text = "Proforma Invoice created successfully.";
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        private void ShowError(string msg)
        {
            PanelError.Visible = true;
            lblErrorMsg.Text = msg;
        }
    }
}
