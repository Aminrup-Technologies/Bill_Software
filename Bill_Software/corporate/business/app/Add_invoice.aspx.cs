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

    public partial class WebForm26 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dtProduct = new DataTable();

        private List<string> vatRates;
        private List<string> serviceTaxRates;


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
                txtinvoiceDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            }

        }

        private void LoadTaxRates()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            // Fetch VAT Rates
            string vatQuery = "Select Vat_Rate from tbl_Vat_Master";
            SqlCommand vatCmd = new SqlCommand(vatQuery, DbCL.Conn);
            SqlDataReader vatRdr = vatCmd.ExecuteReader();

            vatRates = new List<string> { "NA" }; // Initialize with "NA"
            while (vatRdr.Read())
            {
                vatRates.Add(vatRdr[0].ToString());
            }
            vatRdr.Close();

            // Fetch Service Tax Rates
            string serviceTaxQuery = "Select Service_tax from tbl_Service_master";
            SqlCommand serviceTaxCmd = new SqlCommand(serviceTaxQuery, DbCL.Conn);
            SqlDataReader serviceTaxRdr = serviceTaxCmd.ExecuteReader();

            serviceTaxRates = new List<string> { "NA" }; // Initialize with "NA"
            while (serviceTaxRdr.Read())
            {
                serviceTaxRates.Add(serviceTaxRdr[0].ToString());
            }
            serviceTaxRdr.Close();

            DbCL.Conn.Close();
        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                BuindCompanyId();
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName, tbl_Quotation.ID, tbl_Quotation.service_tax1, tbl_Quotation.sub_total, tbl_Quotation.DO_Number, tbl_Quotation.PO_Number, tbl_Quotation.Quotation_no, tbl_Quotation.Quotation_date, tbl_Quotation.Gross, tbl_Quotation.Service_tax, tbl_Quotation.Net_amount, tbl_Quotation.mailStatusDate, tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.Client_Id='" + lblclientId.Text + "' order by tbl_Quotation.ID desc";

                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Client_Id='" + lblclientId.Text + "' and tbl_Quotation.Status2='No' order by tbl_Quotation.ID desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Status2='No' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by tbl_Quotation.ID desc";
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName, tbl_Quotation.ID, tbl_Quotation.service_tax1, tbl_Quotation.sub_total, tbl_Quotation.DO_Number, tbl_Quotation.PO_Number, tbl_Quotation.Quotation_no, tbl_Quotation.Quotation_date, tbl_Quotation.Gross, tbl_Quotation.Service_tax, tbl_Quotation.Net_amount, tbl_Quotation.mailStatusDate, tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by tbl_Quotation.ID desc";

                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName, tbl_Quotation.ID, tbl_Quotation.service_tax1, tbl_Quotation.sub_total, tbl_Quotation.DO_Number, tbl_Quotation.PO_Number, tbl_Quotation.Quotation_no, tbl_Quotation.Quotation_date, tbl_Quotation.Gross, tbl_Quotation.Service_tax, tbl_Quotation.Net_amount, tbl_Quotation.mailStatusDate, tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.Client_Id='" + lblclientId.Text + "' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by tbl_Quotation.ID desc";

                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Status2='No' and tbl_Quotation.Client_Id='" + lblclientId.Text + "' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by tbl_Quotation.ID desc";
                Buinddatagrid(cmdstring);
            }
            btnSertch.Visible = false;

        }
        private void Buinddatagrid(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                Buinddatagrid1(cmdstring);
            }
            else
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "No Data Found...";
            }
            DbCL.Conn.Close();
        }

        private void Buinddatagrid1(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd1 = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd1.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
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

        protected void btnreset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/Add_invoice.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Quotation_no = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Select")
            {
                LoadTaxRates();

                Panel1.Visible = true;
                Binddetails(Quotation_no);
                BindAllProduct(Quotation_no);

                DataList1.Visible = false;

                DDL_vat_parsentage.Items.AddRange(vatRates.Select(rate => new ListItem(rate)).ToArray());
            }
        }

        private void BindAllProduct_old(string quotation_no)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Id,Sl_no,Quotation_no,Product_id,Product_name,Quantity,sail_rate,Service_tax_rate,Total_sail_rate,Total_sail_rate1,Total_sail_rate2,purchess_rate,specification,InvStatus from tbl_Quotaion_details where Quotation_no=@quotation_no order by id";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlParameter[] pram = {
                new SqlParameter("@Quotation_no",quotation_no),
            };
            dtProduct = DbCL.SPreturn_dt(cmdstring, pram);

            if (dtProduct.Rows.Count > 0)
            {
                Gridview_Product.DataSource = dtProduct;
                Gridview_Product.DataBind();
                ViewState["dt"] = dtProduct;
            }
            DbCL.Conn.Close();
        }

        private string bindChalanno(string quotation_no)
        {
            string chano = "";
            string query = "select Invoice_No from tbl_Invoice_details where Quotation_no=@quotation_no";
            SqlParameter[] pram = {
                new SqlParameter("@quotation_no",quotation_no)
            };
            SqlDataReader rdr = DbCL.SPReturnRdr(query, pram);
            int i = 0;
            while (rdr.Read())
            {
                string Chano1 = rdr["Invoice_No"].ToString();
                Chano1 = "'" + Chano1 + "'";
                if (i == 0)
                {
                    chano = Chano1;
                }
                else
                {
                    chano = chano + " , " + Chano1;
                }
                i++;
            }
            return chano;
        }

        private void BindAllProduct(string quotation_no)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            string invoiceNos = "";
            invoiceNos = bindChalanno(quotation_no);
            if (invoiceNos == "")
            {
                invoiceNos = "('')";
            }
            else
            {
                invoiceNos = "(" + invoiceNos + ")";
            }

            string cmdstring = @"select Id,Sl_no,Quotation_no,Product_id,Product_Code,Product_name,Quantity,sail_rate,Service_tax_rate,Total_sail_rate,Total_sail_rate1,Total_sail_rate2,purchess_rate,specification,InvStatus from tbl_Quotaion_details where Quotation_no=@quotation_no order by id";

            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlParameter[] pram = { new SqlParameter("@quotation_no", quotation_no) };
            dtProduct = DbCL.SPreturn_dt(cmdstring, pram);

            if (dtProduct.Rows.Count > 0)
            {
                DataTable dtProductModified = new DataTable();
                dtProductModified.Columns.Add("Quotation_no", typeof(string));
                dtProductModified.Columns.Add("Product_id", typeof(string));  //HSN CODE
                dtProductModified.Columns.Add("Product_Code", typeof(string)); // ID
                dtProductModified.Columns.Add("Product_name", typeof(string));
                dtProductModified.Columns.Add("Quantity", typeof(string));
                dtProductModified.Columns.Add("DeliveredQnt", typeof(string));
                dtProductModified.Columns.Add("SQuantity", typeof(string));
                dtProductModified.Columns.Add("RemainQny", typeof(string));
                dtProductModified.Columns.Add("sail_rate", typeof(string));
                dtProductModified.Columns.Add("Service_tax_rate", typeof(string));
                dtProductModified.Columns.Add("specification", typeof(string));
                dtProductModified.Columns.Add("Total_sail_rate1", typeof(string));
                dtProductModified.Columns.Add("Total_sail_rate2", typeof(string));
                dtProductModified.Columns.Add("InvStatus", typeof(string));

                foreach (DataRow row in dtProduct.Rows)
                {
                    string HSN = row["Product_id"].ToString(); //HSN
                    string ProductID = row["Product_Code"].ToString(); // ID
                    string product_name = row["Product_name"].ToString();
                    string quantity = row["Quantity"].ToString();
                    string sail_rate = row["sail_rate"].ToString();
                    string service_tax_rate = row["Service_tax_rate"].ToString();
                    string specification = row["specification"].ToString();
                    string total_sail_rate1 = row["Total_sail_rate1"].ToString();
                    string total_sail_rate2 = row["Total_sail_rate2"].ToString();
                    string invStatus = row["InvStatus"].ToString();

                    // Get delivered quantity
                    string deliveredQnt = BindPreQnt(product_name, quotation_no, invoiceNos);
                    string remainQnt = (Convert.ToInt32(quantity) - Convert.ToInt32(deliveredQnt)).ToString();
                    string stockqnty = findstock2(ProductID, product_name);

                    DataRow dr = dtProductModified.NewRow();
                    dr["Quotation_no"] = quotation_no;
                    dr["Product_id"] = ProductID;
                    dr["Product_Code"] = HSN;
                    dr["Product_name"] = product_name;
                    dr["Quantity"] = quantity;
                    dr["DeliveredQnt"] = deliveredQnt;
                    dr["SQuantity"] = stockqnty;
                    dr["RemainQny"] = remainQnt;
                    dr["sail_rate"] = sail_rate;
                    dr["Service_tax_rate"] = service_tax_rate;
                    dr["specification"] = specification;
                    dr["Total_sail_rate1"] = total_sail_rate1;
                    dr["Total_sail_rate2"] = total_sail_rate2;
                    dr["InvStatus"] = invStatus;

                    dtProductModified.Rows.Add(dr);
                }

                Gridview_Product.DataSource = dtProductModified;
                Gridview_Product.DataBind();
                ViewState["dt"] = dtProductModified;
            }
            DbCL.Conn.Close();
        }


        private string GetInvoiceNos(string quotation_no)
        {
            string invoiceNos = "";

            string query = @"SELECT STUFF((SELECT DISTINCT ',' + CAST(Invoice_No AS VARCHAR) FROM tbl_Invoice_details WHERE Quotation_no = @quotation_no FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '') AS InvoiceNos";

            SqlCommand cmd = new SqlCommand(query, DbCL.Conn);
            cmd.Parameters.AddWithValue("@quotation_no", quotation_no);

            object result = cmd.ExecuteScalar();
            if (result != null)
            {
                invoiceNos = result.ToString();
            }

            return invoiceNos;
        }

        private void ShowErrorMessage(string message)
        {
            PanelError.Visible = true;
            lblErrorMsg.Text = message;
        }


        private void ShowErrorMessageSpcl(string message)
        {
            Panel2.Visible = true;
            Label3.Text = message;
        }

        private void ShowSucessMessageSpcl(string message)
        {
            Panel3.Visible = true;
            Label5.Text = message;
        }

        private string BindPreQnt(string product_name, string quotation_no, string chalanno)
        {
            string deliQnt = "0";
            string query = "select sum(CAST(Quantity as int)) as DeliveredQnt, Product_name from tbl_Invoice_details where Invoice_No in " + chalanno + " and Product_name='" + product_name + "' group by Product_name";
            SqlDataReader rdr1 = DbCL.SPReturnRdr(query, null);
            if (rdr1.Read())
            {
                deliQnt = rdr1["DeliveredQnt"].ToString();
            }
            return deliQnt;
        }

        private string BindPreQnt1(string product_name, string quotation_no, string invoiceNos)
        {
            string deliQnt = "0";

            try
            {
                invoiceNos = "'" + invoiceNos.Replace(",", "','") + "'";
                // **Fix: Ensure invoiceNos format**
                //invoiceNos = "'" + invoiceNos.Replace(",", "','") + "'"; // Format: 'INV1','INV2'

                string query = $@"SELECT SUM(CAST(Quantity AS INT)) AS DeliveredQnt FROM tbl_Invoice_details WHERE Invoice_No IN (SELECT value FROM STRING_SPLIT(@invoiceNos, ',')) AND Product_name = @product_name GROUP BY Product_name";

                SqlCommand cmd = new SqlCommand(query, DbCL.Conn);
                cmd.Parameters.AddWithValue("@invoiceNos", invoiceNos);
                cmd.Parameters.AddWithValue("@product_name", product_name);

                if (DbCL.Conn.State == ConnectionState.Closed)
                {
                    DbCL.Conn.Open();
                }

                SqlDataReader rdr1 = cmd.ExecuteReader();
                if (rdr1.Read() && rdr1["DeliveredQnt"] != DBNull.Value)
                {
                    deliQnt = rdr1["DeliveredQnt"].ToString();
                }

                rdr1.Close();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("SQL Error: " + ex.Message);
            }
            finally
            {
                if (DbCL.Conn.State == ConnectionState.Open)
                {
                    DbCL.Conn.Close();
                }
            }

            return deliQnt;
        }


        private string BindPreQnt_old(string product_name, string quotation_no, string invoiceNos)
        {
            string deliQnt = "0";

            string query = $@"SELECT SUM(CAST(Quantity AS INT)) AS DeliveredQnt FROM tbl_Invoice_details WHERE Invoice_No IN " + invoiceNos + " AND Product_name = @product_name GROUP BY Product_name";

            SqlCommand cmd = new SqlCommand(query, DbCL.Conn);
            cmd.Parameters.AddWithValue("@product_name", product_name);

            SqlDataReader rdr1 = cmd.ExecuteReader();
            if (rdr1.Read())
            {
                deliQnt = rdr1["DeliveredQnt"].ToString();
            }
            rdr1.Close();

            return deliQnt;
        }

        private void Binddetails(string Quotation_no)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Quotation where Quotation_no='" + Quotation_no.ToString() + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblClient_Id.Text = re["Client_Id"].ToString();
                string qutno = re["Quotation_no"].ToString();
                lblQuotation_no.Text = qutno.ToString();
                lblQuotation_date.Text = re["Quotation_date"].ToString();

                lblGross_amount.Text = re["Gross"].ToString();
                lblservicetax.Text = re["Service_tax"].ToString();
                lblNet_amount.Text = re["Net_amount"].ToString();
                lblservicetax0.Text = re["service_tax1"].ToString();
                lblsubtotal.Text = re["sub_total"].ToString();
                string clientcode = lblClient_Id.Text;

                bindServiceDetails(qutno);
                bindFactoryAddress(clientcode);
            }
            DbCL.Conn.Close();
            BindclientName();
            cmbaddressfor.Items.Add("Corporate office");
            DbCL.FillCombo10(cmbaddressfor, "select Factory_name from tbl_Factory where Client_id='" + lblClient_Id.Text + "' order by Factory_name");

            //BindInvoiceNo();
        }

        private void bindFactoryAddress(string clientcode)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Address1+', '+City+', '+pin+', '+State from tbl_Client where Client_Id='" + clientcode + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            SqlDataReader DR1 = cmd.ExecuteReader();
            while (DR1.Read())
            {
                FactoryAddress.Items.Add(DR1.GetValue(0).ToString());
            }
            DbCL.Conn.Close();
            bindRegAddress(clientcode);
            bindAddress(clientcode);
        }

        private void bindRegAddress(string clientcode)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Address+', '+State+', '+City+', '+pin as regadd from tbl_ClientRegAddress where Client_Id='" + clientcode + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            SqlDataReader DR1 = cmd.ExecuteReader();
            while (DR1.Read())
            {
                FactoryAddress.Items.Add(DR1.GetValue(0).ToString());
            }
            DbCL.Conn.Close();
        }

        private void bindAddress(string clientcode)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select [Address1] +', '+ [Address2]+', '+[city]+', '+[State]+', '+[pin] as address from tbl_Factory where Client_id='" + clientcode + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            SqlDataReader DR1 = cmd.ExecuteReader();
            while (DR1.Read())
            {
                FactoryAddress.Items.Add(DR1.GetValue(0).ToString());
            }
            DbCL.Conn.Close();
        }

        private string BindInvoiceNo()
        {
            //string p = null;
            string c = lblClientName.Text.Trim();
            string f = c.Substring(0, 1);
            //string tt;
            //for (int i = 0; i < c.Length; i++)
            //{
            //    p = c.Substring(i, 1);
            //    if (p == " ")
            //    {
            //        tt = c.Substring((i + 1), 1);
            //        if (tt == "(")
            //        {
            //            tt = c.Substring((i + 2), 1);
            //        }
            //        f = f + tt;
            //    }
            //}
            f = "INV/" + f + "/";
            string ss = findmonth();
            f = f + ss;
            int j = idreturn();
            j = j + 1;
            f = f + j.ToString();
            return f;
        }

        private int idreturn()
        {
            string a = null;
            int b = 0;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string date1 = txtinvoiceDate.Text;
            string date2 = date1.Substring(3, 3);
            string date3 = date1.Substring(7, 4);
            string date4, date5, date6;
            if (date2 == "Jan" || date2 == "Feb" || date2 == "Mar")
            {
                date4 = ((Convert.ToInt32(date3) - 1)).ToString();
                date5 = "31-Mar-" + date4;
                date6 = "31-Mar-" + date3;
            }
            else
            {
                date4 = ((Convert.ToInt32(date3) + 1)).ToString();
                date5 = "31-Mar-" + date3;
                date6 = "31-Mar-" + date4;
            }
            string cmdstring = "select Sl_no from tbl_Invoice where ID=(select max(ID) from tbl_Invoice where cast(Invoice_Date as datetime) between '" + date5.ToString() + "' and '" + date6.ToString() + "')";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                a = re["Sl_no"].ToString();
                b = Convert.ToInt32(a);
            }
            else
            {
                b = 0;
            }
            DbCL.Conn.Close();
            return b;

        }

        private string findmonth()
        {
            string MonthName = "-";
            string a = txtinvoiceDate.Text.Substring(3, 3);
            string b = txtinvoiceDate.Text.Substring(9, 2);
            if (a == "Jan" || a == "Feb" || a == "Mar")
            {
                MonthName = (Convert.ToInt32(b) - 1).ToString() + "-" + b + "/";
            }
            else
            {
                MonthName = b + "-" + (Convert.ToInt32(b) + 1).ToString() + "/";
            }
            return MonthName;
        }

        private void BindclientName()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Name from tbl_Client where Client_Id='" + lblClient_Id.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblClientName.Text = re["Client_Name"].ToString();
            }
            DbCL.Conn.Close();
        }

        private void Executer()
        {
            if (FactoryAddress.GetSelectedIndices().Length == 0)
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "Please select a delivery address.";
                return;
            }

            string stockCheck = "Yes"; // Placeholder for actual stock check logic
            if (stockCheck != "Yes")
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "Insufficient stock. Please check inventory before proceeding.";
                return;
            }

            string quotationNo = lblQuotation_no.Text;
            string dueAmount = "0.00"; // Placeholder for actual due amount check
            if (dueAmount != "0.00")
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "Previous invoice payment not received.";
                return;
            }

            string invoiceNo = BindInvoiceNo();
            int serialNo = idreturn() + 1;
            int checkedCount = 0;
            string status = InsertSelectedProductNew(invoiceNo, quotationNo, ref checkedCount);

            List<string> validStatuses = new List<string> { "Completed", "Partial", "Partial + No Stock", "Pending", "Pending + No Stock", "Partial + No Stock + Pending" };
            if (checkedCount > 0 && validStatuses.Contains(status))
            {
                double invTotalWithGst = Session["InvTotalAmountWithGst"] != null ? Convert.ToDouble(Session["InvTotalAmountWithGst"]) : 0;
                double invTotalWithoutGst = Session["InvTotalAmountWithOutGst"] != null ? Convert.ToDouble(Session["InvTotalAmountWithOutGst"]) : 0;
                double totalGstAmount = Session["invTotalGstAmount"] != null ? Convert.ToDouble(Session["invTotalGstAmount"]) : 0;
                double discount = string.IsNullOrEmpty(txtDiscount.Text) ? 0 : Convert.ToDouble(txtDiscount.Text);
                Session["NetAmount"] = Math.Round(invTotalWithGst) - discount;

                decimal tcsAmount = 0.00m, tcsrate = 0.00m, deliveryAmount = 0.00m, otherAmount1 = 0.00m;
                decimal.TryParse(txt_tcs_amnt.Text.Trim(), out tcsAmount);
                decimal.TryParse(txt_tcs_percent.Text.Trim(), out tcsrate);
                decimal.TryParse(txt_delivery_amnt.Text.Trim(), out deliveryAmount);
                decimal.TryParse(txt_othr_amnt.Text.Trim(), out otherAmount1);
                string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";

                if (invTotalWithGst > 0)
                {
                    string query = "INSERT INTO tbl_Invoice (Invoice_No, Invoice_Date, Quotation_No, Quotation_Date, Client_ID, Gross, Net_Amount, Sl_no, Service_Tax1, sub_total, discount, addressfor, status1, status2, TCS_Amount, TCS_Rate, Delivery_Amount, Delivery_Rate, otherAmount1_name, otherAmount1, AddedById, PServiceName) " +
                                   "VALUES (@Invoice_No, @Invoice_Date, @Quotation_No, @Quotation_Date, @Client_ID, @Gross, @Net_Amount, @Sl_no, @Service_Tax1, @sub_total, @discount, @addressfor, 'No', 'Active', @TCS_Amount, @TCS_Rate, @Delivery_Amount, @Delivery_Rate, @otherAmount1_name, @otherAmount1, @AddedById, @PServiceName)";

                    SqlParameter[] parameters = {
                        new SqlParameter("@Invoice_No", invoiceNo),
                        new SqlParameter("@Invoice_Date", txtinvoiceDate.Text),
                        new SqlParameter("@Quotation_No", quotationNo),
                        new SqlParameter("@Quotation_Date", lblQuotation_date.Text),
                        new SqlParameter("@Client_ID", lblClient_Id.Text),
                        new SqlParameter("@Gross", invTotalWithGst),
                        new SqlParameter("@Net_Amount", Session["NetAmount"]),
                        new SqlParameter("@Sl_no", serialNo),
                        new SqlParameter("@Service_Tax1", totalGstAmount),
                        new SqlParameter("@sub_total", invTotalWithoutGst),
                        new SqlParameter("@discount", discount),
                        new SqlParameter("@addressfor", cmbaddressfor.Text),
                        new SqlParameter("@TCS_Amount", tcsAmount),
                        new SqlParameter("@TCS_Rate", tcsrate),
                        new SqlParameter("@Delivery_Amount", deliveryAmount),
                        new SqlParameter("@Delivery_Rate", DDL_vat_parsentage.SelectedValue),
                        new SqlParameter("@otherAmount1_name", TextBox1.Text.Trim()),
                        new SqlParameter("@otherAmount1", otherAmount1),
                        new SqlParameter("@AddedById", userId),
                        new SqlParameter("@PServiceName", lbl_servicename.Text.ToString()),

                    };
                    DbCL.SPExecDB(query, parameters);
                }

                if (status == "Completed")
                {
                    DbCL.executeRdr("UPDATE tbl_Quotation SET Status2 = 'Yes' WHERE Quotation_no = '" + quotationNo + "'");
                }

                insertCorRegFacAddress(invoiceNo);
                PanelOK.Visible = true;
                lblOk.Text = "Data saved successfully.";
            }
            else
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = status == "No Stock" ? "Error: Some items are out of stock. Please check inventory before proceeding." : "Error: Unable to process invoice. Please check stock and quotation details.";
            }

            Button1.Visible = false;
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Executer();
        }

        private void Executer_Old()
        {
            if (FactoryAddress.GetSelectedIndices().Length > 0)
            {
                //string stock_check = findstock();
                string stock_check = "Yes";
                if (stock_check == "Yes")
                {
                    string quno = lblQuotation_no.Text;
                    //string dueamount = bindpaymentDetails(quno);
                    string dueamount = "0.00";
                    if (dueamount == "0.00")
                    {
                        string invoice_no = BindInvoiceNo();
                        int j = idreturn();
                        j = j + 1;

                        //Checker = InsertSelectedProduct(invoice_no, quno);

                        int checkedCount = 0; string Checker = "Completed";
                        //string Checker = InsertSelectedProduct(invoice_no, quno, ref checkedCount);

                        List<string> validStatuses = new List<string> { "Completed", "Partial", "Partial + No Stock", "Pending", "Pending + No Stock" };
                        if (checkedCount > 0 && validStatuses.Contains(Checker))
                        {

                            double invTotalWithGst = Session["InvTotalAmountWithGst"] != null ? Convert.ToDouble(Session["InvTotalAmountWithGst"]) : 0;
                            double invTotalWithoutGst = Session["InvTotalAmountWithOutGst"] != null ? Convert.ToDouble(Session["InvTotalAmountWithOutGst"]) : 0;
                            double totalGstAmount = Session["invTotalGstAmount"] != null ? Convert.ToDouble(Session["invTotalGstAmount"]) : 0;
                            double discount = string.IsNullOrEmpty(txtDiscount.Text) ? 0 : Convert.ToDouble(txtDiscount.Text);

                            // Calculate Net Amount after discount
                            Session["NetAmount"] = Math.Round(invTotalWithGst) - discount;

                            // Ensure that values are valid before inserting into the database
                            if (invTotalWithGst > 0)
                            {
                                string query = "INSERT INTO tbl_Invoice (Invoice_No, Invoice_Date, Quotation_No, Quotation_Date, Client_ID, Gross, Net_Amount, Sl_no, Service_Tax1, sub_total, discount, addressfor, status1, status2) " +
                                               "VALUES (@Invoice_No, @Invoice_Date, @Quotation_No, @Quotation_Date, @Client_ID, @Gross, @Net_Amount, @Sl_no, @Service_Tax1, @sub_total, @discount, @addressfor, 'No', 'Active')";

                                SqlParameter[] pram = {
                                        new SqlParameter("@Invoice_No", invoice_no.ToString()),
                                        new SqlParameter("@Invoice_Date", txtinvoiceDate.Text),
                                        new SqlParameter("@Quotation_No", lblQuotation_no.Text),
                                        new SqlParameter("@Quotation_Date", lblQuotation_date.Text),
                                        new SqlParameter("@Client_ID", lblClient_Id.Text),
                                        new SqlParameter("@Gross", invTotalWithGst),
                                        new SqlParameter("@Net_Amount", Session["NetAmount"]),
                                        new SqlParameter("@Sl_no", j.ToString()),
                                        new SqlParameter("@Service_Tax1", totalGstAmount),
                                        new SqlParameter("@sub_total", invTotalWithoutGst),
                                        new SqlParameter("@discount", discount),
                                        new SqlParameter("@addressfor", cmbaddressfor.Text)
                                    };

                                DbCL.SPExecDB(query, pram);
                            }

                            //// Calculate Net Amount after discount
                            //Session["NetAmount"] = Math.Round(Convert.ToDouble(Session["InvTotalAmountWithGst"])) - Convert.ToDouble(txtDiscount.Text);

                            //// Insert invoice record
                            //DbCL.executeRdr("INSERT INTO tbl_Invoice (Invoice_No, Invoice_Date, Quotation_No, Quotation_Date, Client_ID, Gross, Net_Amount, Sl_no, Service_Tax1, sub_total, discount, addressfor, status1, status2) " +
                            //                "VALUES ('" + invoice_no.ToString() + "','" + txtinvoiceDate.Text + "','" + lblQuotation_no.Text + "','" + lblQuotation_date.Text + "','" +
                            //                lblClient_Id.Text + "','" + Session["InvTotalAmountWithGst"].ToString() + "','" + Session["NetAmount"].ToString() + "','" +
                            //                j.ToString() + "','" + Session["invTotalGstAmount"].ToString() + "','" + Session["InvTotalAmountWithOutGst"].ToString() + "','" +
                            //                txtDiscount.Text + "','" + cmbaddressfor.Text + "','No','Active')");

                            // Update Quotation status if fully invoiced
                            if (Checker == "Completed")
                            {
                                DbCL.executeRdr("UPDATE tbl_Quotation SET Status2 = 'Yes' WHERE Quotation_no = '" + lblQuotation_no.Text + "'");
                            }

                            // Update stock after invoicing
                            //updatestock();

                            // Insert address details for the invoice
                            insertCorRegFacAddress(invoice_no);

                            PanelOK.Visible = true;
                            lblOk.Text = "Data Saved Successfully...";
                        }
                        else if (Checker == "No Stock")
                        {
                            PanelError.Visible = true;
                            lblErrorMsg.Text = "Error: Some items are out of stock. Please check inventory before proceeding.";
                        }
                        else
                        {
                            PanelError.Visible = true;
                            lblErrorMsg.Text = "Error: Unable to process invoice. Please check stock and quotation details.";
                        }
                    }
                    else
                    {
                        PanelError.Visible = true;
                        lblErrorMsg.Text = "Previous Invoice Payment Not Received...";
                    }
                    Button1.Visible = false;
                }
                else
                {
                    PanelError.Visible = true;
                    lblErrorMsg.Text = "You don't have suffiecient stock....";
                }
            }
            else
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "Please Select Delivery Address....";
            }
        }

        private void insertCorRegFacAddress(string invoice_no)
        {
            int selectedSite = 0;

            string listsite_details = null;
            int slno22 = 1;
            for (int i = 0; i < FactoryAddress.Items.Count; i++)
            {
                if (FactoryAddress.Items[i].Selected)
                {
                    selectedSite = selectedSite + 1;
                    listsite_details = FactoryAddress.Items[i].Text;

                    string query = "insert into tbl_InvSiteAddress(invoice_no,SiteAddress) values (@invoice_no,@SiteAddress)";
                    SqlParameter[] pram = {
                         new SqlParameter("@invoice_no",invoice_no),
                         new SqlParameter("@SiteAddress",listsite_details)
                    };

                    DbCL.SPExecDB(query, pram);
                    slno22 = slno22 + 1;
                }
            }
        }

        private string bindpaymentDetails(string quno)
        {
            string due = "";
            string query = "select Due_amount from tbl_invoice_payment where Quotation_No=@Quotation_No";
            SqlParameter[] pram = {
                new SqlParameter("@Quotation_No",quno)
            };
            SqlDataReader rdr = DbCL.SPReturnRdr(query, pram);
            while (rdr.Read())
            {
                due = rdr["Due_amount"].ToString();
            }
            return due;
        }

        private void bindServiceDetails(string quno)
        {
            string query = "select PrimaryService from tbl_QutPrimaryService where qut_no=@Quotation_No";
            SqlParameter[] pram = {
                new SqlParameter("@Quotation_No",quno)
            };
            SqlDataReader rdr = DbCL.SPReturnRdr(query, pram);
            while (rdr.Read())
            {
                lbl_servicename.Text = rdr["PrimaryService"].ToString();
            }
        }

        private void ChecktotalInvAmount(out double totalQutamount, string quno)
        {
            string query = "select sum(cast(Total_sail_rate1 as real)) as totalQutamount from tbl_Invoice_details where Quotation_no=@Quotation_no";
            SqlParameter[] pram = {
                new SqlParameter("@Quotation_no",quno)
            };
            SqlDataReader rdr = DbCL.SPReturnRdr(query, pram);
            if (rdr.Read())
            {
                totalQutamount = Math.Round(Convert.ToDouble(rdr["totalQutamount"]), 2);

                //totalinvamount = Convert.ToDouble(rdr["totalinvvalue"]);
            }
            else
            {
                totalQutamount = 0;
            }
        }

        //private void ChecktotalInvAmount(out double totalinvamount,string quno)
        //{
        //    string query = "select sum(cast(Gross as decimal)) from tbl_Invoice where  Quotation_No='I2I/COPL/14-15/7'";

        //}

        private string InsertSelectedProduct_old(string invoice_no, string quno)
        {
            string Status = String.Empty;
            int completedCount = 0; // Count of completed rows
            int checkedCount = 0;   // Count of checked rows
            int noStockCount = 0;   // Count of rows with no stock

            double InvTotalAmountWithGst = 0;
            double InvTotalAmountWithOutGst = 0;
            double invTotalGstAmount = 0;

            List<string> errorMessages = new List<string>();

            DataTable dt1;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            dt1 = (DataTable)ViewState["dt"];
            if (dt1 != null)
            {
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    List<string> missingFields = new List<string>();
                    CheckBox chk = (CheckBox)(Gridview_Product.Rows[i].FindControl("chk"));
                    if (chk.Checked == true)
                    {
                        
                        checkedCount++;
                        try
                        {
                            string ProductCode = ((Label)Gridview_Product.Rows[i].FindControl("Product_id")).Text;
                            string ProductName = ((Label)Gridview_Product.Rows[i].FindControl("Product_name")).Text;
                            string Quantity = ((Label)Gridview_Product.Rows[i].FindControl("Quantity")).Text;
                            string DQnt = ((Label)Gridview_Product.Rows[i].FindControl("DeliveredQnt")).Text;
                            string SQnt = ((Label)Gridview_Product.Rows[i].FindControl("SQuantity")).Text;
                            string RQty = ((TextBox)Gridview_Product.Rows[i].FindControl("Qty")).Text;
                            string SailRate = ((Label)Gridview_Product.Rows[i].FindControl("sail_rate")).Text;
                            string GstPercentage = ((Label)Gridview_Product.Rows[i].FindControl("Service_tax_rate")).Text;
                            string AmountWithGst = ((Label)Gridview_Product.Rows[i].FindControl("Total_sail_rate1")).Text;
                            string specifai = ((Label)Gridview_Product.Rows[i].FindControl("specification")).Text;
                            string AmountWithOutGst = ((Label)Gridview_Product.Rows[i].FindControl("Total_sail_rate2")).Text;
                            string InvStatus = ((Label)Gridview_Product.Rows[i].FindControl("InvStatus")).Text;

                            if (InvStatus != "Yes")
                            {
                                double quotedQuantity = string.IsNullOrEmpty(Quantity) ? 0 : Convert.ToDouble(Quantity);
                                double deliveredQuantity = string.IsNullOrEmpty(DQnt) ? 0 : Convert.ToDouble(DQnt);
                                double stockQuantity = string.IsNullOrEmpty(SQnt) ? 0 : Convert.ToDouble(SQnt);
                                double remainingQuantity = string.IsNullOrEmpty(RQty) ? 0 : Convert.ToDouble(RQty);
                                if (remainingQuantity <= stockQuantity)
                                {
                                    double sailRate = string.IsNullOrEmpty(SailRate) ? 0 : Convert.ToDouble(SailRate);
                                    double gstPercentage = string.IsNullOrEmpty(GstPercentage) ? 0 : Convert.ToDouble(GstPercentage);

                                    double invoiceQuantity = (quotedQuantity == remainingQuantity) ? quotedQuantity : remainingQuantity;
                                    double amountWithoutGst = invoiceQuantity * sailRate;
                                    double gstAmount = (amountWithoutGst * gstPercentage) / 100;
                                    double amountWithGst = amountWithoutGst + gstAmount;

                                    string formattedAmountWithoutGst = amountWithoutGst.ToString("0.00");
                                    string formattedAmountWithGst = amountWithGst.ToString("0.00");

                                    InvTotalAmountWithGst = InvTotalAmountWithGst + Convert.ToDouble(amountWithGst);
                                    InvTotalAmountWithOutGst = InvTotalAmountWithOutGst + Convert.ToDouble(amountWithoutGst);
                                    invTotalGstAmount = invTotalGstAmount + (InvTotalAmountWithGst - InvTotalAmountWithOutGst);

                                    Session["InvTotalAmountWithGst"] = InvTotalAmountWithGst;
                                    Session["InvTotalAmountWithOutGst"] = InvTotalAmountWithOutGst;
                                    Session["invTotalGstAmount"] = invTotalGstAmount;

                                    string query = "insert into tbl_Invoice_details(Quotation_no,Invoice_No,Product_id,Product_name,Quantity,sail_rate,Service_tax_rate,Total_sail_rate1,Total_sail_rate2,specification) values (@Quotation_no,@Invoice_No,@Product_id,@Product_name,@Quantity,@sail_rate,@Service_tax_rate,@Total_sail_rate1,@Total_sail_rate2,@specification)";
                                    SqlParameter[] pram = {
                                        new SqlParameter("@Quotation_no",quno),
                                        new SqlParameter("@Invoice_No",invoice_no),
                                        new SqlParameter("@Product_id",ProductCode),
                                        new SqlParameter("@Product_name",ProductName),
                                        new SqlParameter("@Quantity",Quantity),
                                        new SqlParameter("@sail_rate",SailRate),
                                        new SqlParameter("@Service_tax_rate",GstPercentage),
                                        new SqlParameter("@Total_sail_rate1",amountWithGst),
                                        new SqlParameter("@specification",specifai),
                                        new SqlParameter("@Total_sail_rate2",amountWithoutGst)
                                    };
                                    DbCL.SPExecDB(query, pram);

                                    string invStatus = ((deliveredQuantity + remainingQuantity) == quotedQuantity) ? "Yes" : "No";
                                    updateqtableforproduct(quno, ProductCode, ProductName, invStatus);
                                }
                                else
                                {
                                    noStockCount++;
                                    Status = "No Stock";
                                    missingFields.Add("You don't have suffiecient stock....");
                                }
                            }
                            else
                            {
                                completedCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            string message = ex.ToString();
                        }
                    }
                    if (missingFields.Count > 0)
                    {
                        errorMessages.Add($"Row {i + 1}: {string.Join(", ", missingFields)} ");
                        continue;
                    }
                }
                if (errorMessages.Count > 0)
                {
                    ShowErrorMessage(string.Join("<br>", errorMessages)); // "<br>" works for web-based UI
                }
            }
            // Final status check
            //Status = (checkedCount > 0 && completedCount == checkedCount) ? "Completed" : "Pending";
            if (checkedCount > 0)
            {
                if (noStockCount > 0)
                {
                    Status = "No Stock"; // If any checked row is out of stock
                }
                else if (completedCount == checkedCount)
                {
                    Status = "Completed"; // If all checked rows are invoiced
                }
            }
            return Status;
        }

        private string InsertSelectedProduct_old2(string invoice_no, string quno, ref int checkedCount)
        {
            string Status = string.Empty;
            int completedCount = 0; // Count of completed rows
            int noStockCount = 0;   // Count of rows with no stock
            int pendingCount = 0;   // Count of pending invoice rows

            double InvTotalAmountWithGst = 0;
            double InvTotalAmountWithOutGst = 0;
            double invTotalGstAmount = 0;

            List<string> errorMessages = new List<string>();

            DataTable dt1;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            dt1 = (DataTable)ViewState["dt"];

            if (dt1 != null)
            {
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    CheckBox chk = (CheckBox)(Gridview_Product.Rows[i].FindControl("chk"));
                    if (chk.Checked)
                    {
                        checkedCount++; // ✅ Using ref variable
                        try
                        {
                            List<string> missingFields = new List<string>();

                            string ProductId = ((Label)Gridview_Product.Rows[i].FindControl("Product_Code")).Text;
                            string ProductCode = ((Label)Gridview_Product.Rows[i].FindControl("Product_id")).Text;
                            string ProductName = ((Label)Gridview_Product.Rows[i].FindControl("Product_name")).Text;
                            string Quantity = ((Label)Gridview_Product.Rows[i].FindControl("Quantity")).Text;
                            string DQnt = ((Label)Gridview_Product.Rows[i].FindControl("DeliveredQnt")).Text;
                            string SQnt = ((Label)Gridview_Product.Rows[i].FindControl("SQuantity")).Text;
                            string RQty = ((TextBox)Gridview_Product.Rows[i].FindControl("Qty")).Text;
                            string SailRate = ((Label)Gridview_Product.Rows[i].FindControl("sail_rate")).Text;
                            string GstPercentage = ((Label)Gridview_Product.Rows[i].FindControl("Service_tax_rate")).Text;
                            string specifai = ((Label)Gridview_Product.Rows[i].FindControl("specification")).Text;
                            string InvStatus = ((Label)Gridview_Product.Rows[i].FindControl("InvStatus")).Text;

                            double quotedQuantity = string.IsNullOrEmpty(Quantity) ? 0 : Convert.ToDouble(Quantity);
                            double deliveredQuantity = string.IsNullOrEmpty(DQnt) ? 0 : Convert.ToDouble(DQnt);
                            double stockQuantity = string.IsNullOrEmpty(SQnt) ? 0 : Convert.ToDouble(SQnt);
                            double remainingQuantity = string.IsNullOrEmpty(RQty) ? 0 : Convert.ToDouble(RQty);

                            // 🚀 **Scenario 1: Already Invoiced**
                            if (InvStatus == "Yes")
                            {
                                completedCount++;
                                missingFields.Add($"Product '{ProductName}' is already invoiced.");
                            }
                            else
                            {
                                // 🚀 **Scenario 2: Insufficient Stock**
                                if (remainingQuantity > stockQuantity)
                                {
                                    noStockCount++;
                                    missingFields.Add($"Insufficient stock for {ProductName} (Required: {remainingQuantity}, Available: {stockQuantity})");
                                }
                                // 🚀 **Scenario 3: Zero Remaining Quantity**
                                else if (remainingQuantity == 0)
                                {
                                    pendingCount++;
                                    missingFields.Add($"Zero Quantity for {ProductName}. Please enter a valid quantity.");
                                }
                                else
                                {
                                    // ✅ **Process valid invoicing scenario**
                                    double sailRate = string.IsNullOrEmpty(SailRate) ? 0 : Convert.ToDouble(SailRate);
                                    double gstPercentage = string.IsNullOrEmpty(GstPercentage) ? 0 : Convert.ToDouble(GstPercentage);

                                    double invoiceQuantity = remainingQuantity;
                                    double amountWithoutGst = invoiceQuantity * sailRate;
                                    double gstAmount = (amountWithoutGst * gstPercentage) / 100;
                                    double amountWithGst = amountWithoutGst + gstAmount;

                                    InvTotalAmountWithGst += amountWithGst;
                                    InvTotalAmountWithOutGst += amountWithoutGst;
                                    invTotalGstAmount += gstAmount;

                                    Session["InvTotalAmountWithGst"] = InvTotalAmountWithGst;
                                    Session["InvTotalAmountWithOutGst"] = InvTotalAmountWithOutGst;
                                    Session["invTotalGstAmount"] = invTotalGstAmount;

                                    // Insert into database
                                    string query = "INSERT INTO tbl_Invoice_details (Quotation_no, Invoice_No, Product_id, Product_Code, Product_name, Quantity, sail_rate, Service_tax_rate, Total_sail_rate1, Total_sail_rate2, specification) " +
                                                   "VALUES (@Quotation_no, @Invoice_No, @Product_id, @Product_Code, @Product_name, @Quantity, @sail_rate, @Service_tax_rate, @Total_sail_rate1, @Total_sail_rate2, @specification)";

                                    SqlParameter[] pram = {
                                            new SqlParameter("@Quotation_no", quno),
                                            new SqlParameter("@Invoice_No", invoice_no),
                                            new SqlParameter("@Product_id", ProductCode),
                                            new SqlParameter("@Product_Code", ProductId),
                                            new SqlParameter("@Product_name", ProductName),
                                            new SqlParameter("@Quantity", invoiceQuantity.ToString()),
                                            new SqlParameter("@sail_rate", sailRate),
                                            new SqlParameter("@Service_tax_rate", gstPercentage),
                                            new SqlParameter("@Total_sail_rate1", amountWithGst),
                                            new SqlParameter("@specification", specifai),
                                            new SqlParameter("@Total_sail_rate2", amountWithoutGst)
                                        };

                                    DbCL.SPExecDB(query, pram);

                                    // Update invoice status
                                    string invStatus = ((deliveredQuantity + remainingQuantity) == quotedQuantity) ? "Yes" : "No";
                                    updateqtableforproduct(quno, ProductCode, ProductName, invStatus);

                                    // Update stock
                                    updatestock1(ProductId, ProductName, RQty);
                                }
                            }

                            //Add missing field errors if any exist
                            if (missingFields.Count > 0)
                            {
                                errorMessages.Add($"Row {i + 1}: {string.Join(", ", missingFields)}");
                            }
                        }
                        catch (Exception ex)
                        {
                            errorMessages.Add($"Row {i + 1}: {ex.Message}");
                        }
                    }
                }

                //Display all collected errors at once
                if (errorMessages.Count > 0)
                {
                    ShowErrorMessageSpcl(string.Join("<br>", errorMessages));
                }
            }
            Status = DetermineStatus(completedCount, pendingCount, noStockCount, checkedCount);
            return Status;
        }

        private string InsertSelectedProduct_Old3(string invoice_no, string quno, ref int checkedCount)
        {
            string Status = string.Empty;
            int completedCount = 0;
            int noStockCount = 0;
            int pendingCount = 0;
            double InvTotalAmountWithGst = 0;
            double InvTotalAmountWithOutGst = 0;
            double invTotalGstAmount = 0;
            List<string> errorMessages = new List<string>();

            DataTable dt1;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            dt1 = (DataTable)ViewState["dt"];

            if (dt1 != null)
            {
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    CheckBox chk = (CheckBox)(Gridview_Product.Rows[i].FindControl("chk"));
                    if (chk.Checked)
                    {
                        checkedCount++;
                        try
                        {
                            List<string> missingFields = new List<string>();

                            string ProductId = ((Label)Gridview_Product.Rows[i].FindControl("Product_Code")).Text;
                            string ProductCode = ((Label)Gridview_Product.Rows[i].FindControl("Product_id")).Text;
                            string ProductName = ((Label)Gridview_Product.Rows[i].FindControl("Product_name")).Text;
                            string Quantity = ((Label)Gridview_Product.Rows[i].FindControl("Quantity")).Text;
                            string DQnt = ((Label)Gridview_Product.Rows[i].FindControl("DeliveredQnt")).Text;
                            string SQnt = ((Label)Gridview_Product.Rows[i].FindControl("SQuantity")).Text;
                            string RQty = ((TextBox)Gridview_Product.Rows[i].FindControl("Qty")).Text;
                            string SailRate = ((Label)Gridview_Product.Rows[i].FindControl("sail_rate")).Text;
                            string GstPercentage = ((Label)Gridview_Product.Rows[i].FindControl("Service_tax_rate")).Text;
                            string specifai = ((Label)Gridview_Product.Rows[i].FindControl("specification")).Text;
                            string InvStatus = ((Label)Gridview_Product.Rows[i].FindControl("InvStatus")).Text;

                            double quotedQuantity = string.IsNullOrEmpty(Quantity) ? 0 : Convert.ToDouble(Quantity);
                            double deliveredQuantity = string.IsNullOrEmpty(DQnt) ? 0 : Convert.ToDouble(DQnt);
                            double stockQuantity = string.IsNullOrEmpty(SQnt) ? 0 : Convert.ToDouble(SQnt);
                            double remainingQuantity = string.IsNullOrEmpty(RQty) ? 0 : Convert.ToDouble(RQty);

                            // ✅ Already Invoiced Check
                            if (InvStatus == "Yes")
                            {
                                completedCount++;
                                missingFields.Add($"⚠️ Product '{ProductName}' (Code: {ProductId}) is already invoiced.");
                            }
                            else
                            {
                                // ✅ Insufficient Stock Check
                                if (remainingQuantity > stockQuantity)
                                {
                                    noStockCount++;
                                    missingFields.Add($"❌ Insufficient stock for '{ProductName}' (Code: {ProductId}) - Required: {remainingQuantity}, Available: {stockQuantity}");
                                }
                                // ✅ Zero Quantity Check
                                else if (remainingQuantity == 0)
                                {
                                    pendingCount++;
                                    missingFields.Add($"❌ Invalid quantity for '{ProductName}' (Code: {ProductId}) - Please enter a valid amount.");
                                }
                                else
                                {
                                    // ✅ Process Invoicing
                                    double sailRate = string.IsNullOrEmpty(SailRate) ? 0 : Convert.ToDouble(SailRate);
                                    double gstPercentage = string.IsNullOrEmpty(GstPercentage) ? 0 : Convert.ToDouble(GstPercentage);
                                    double amountWithoutGst = remainingQuantity * sailRate;
                                    double gstAmount = (amountWithoutGst * gstPercentage) / 100;
                                    double amountWithGst = amountWithoutGst + gstAmount;

                                    // ✅ Update Session Values
                                    InvTotalAmountWithGst += amountWithGst;
                                    InvTotalAmountWithOutGst += amountWithoutGst;
                                    invTotalGstAmount += gstAmount;

                                    Session["InvTotalAmountWithGst"] = InvTotalAmountWithGst;
                                    Session["InvTotalAmountWithOutGst"] = InvTotalAmountWithOutGst;
                                    Session["invTotalGstAmount"] = invTotalGstAmount;

                                    // ✅ Insert Invoice Details (Using List<SqlParameter>)
                                    string query = @"INSERT INTO tbl_Invoice_details 
                                (Quotation_no, Invoice_No, Product_id, Product_Code, Product_name, Quantity, sail_rate, 
                                Service_tax_rate, Total_sail_rate1, Total_sail_rate2, specification) 
                                VALUES 
                                (@Quotation_no, @Invoice_No, @Product_id, @Product_Code, @Product_name, @Quantity, 
                                @sail_rate, @Service_tax_rate, @Total_sail_rate1, @Total_sail_rate2, @specification)";

                                    List<SqlParameter> pram = new List<SqlParameter>
                                    {
                                        new SqlParameter("@Quotation_no", quno),
                                        new SqlParameter("@Invoice_No", invoice_no),
                                        new SqlParameter("@Product_id", ProductCode),
                                        new SqlParameter("@Product_Code", ProductId),
                                        new SqlParameter("@Product_name", ProductName),
                                        new SqlParameter("@Quantity", remainingQuantity),
                                        new SqlParameter("@sail_rate", sailRate),
                                        new SqlParameter("@Service_tax_rate", gstPercentage),
                                        new SqlParameter("@Total_sail_rate1", amountWithGst),
                                        new SqlParameter("@Total_sail_rate2", amountWithoutGst),
                                        new SqlParameter("@specification", specifai)
                                    };

                                    DbCL.SPExecDB(query, pram.ToArray());

                                    // ✅ Update Invoice Status
                                    string invStatus = ((deliveredQuantity + remainingQuantity) == quotedQuantity) ? "Yes" : "No";
                                    updateqtableforproduct(quno, ProductCode, ProductName, invStatus);

                                    // ✅ Update Stock
                                    updatestock1(ProductId, ProductName, RQty);
                                }
                            }

                            // ✅ Add Missing Field Errors (If Any)
                            if (missingFields.Count > 0)
                            {
                                errorMessages.Add(string.Join("<br>", missingFields));
                            }
                        }
                        catch (Exception ex)
                        {
                            errorMessages.Add($"⚠️ Error processing Product '{Gridview_Product.Rows[i].FindControl("Product_name")}' - {ex.Message}");
                        }
                    }
                }

                // ✅ Show All Error Messages at Once
                if (errorMessages.Count > 0)
                {
                    ShowErrorMessageSpcl(string.Join("<br>", errorMessages));
                }
            }

            // ✅ Determine Final Status
            Status = DetermineStatus(completedCount, pendingCount, noStockCount, checkedCount);
            return Status;
        }

        private string InsertSelectedProductNew(string invoice_no, string quno, ref int checkedCount)
        {
            string Status = string.Empty;
            int completedCount = 0;
            int noStockCount = 0;
            int pendingCount = 0;
            double InvTotalAmountWithGst = 0;
            double InvTotalAmountWithOutGst = 0;
            double invTotalGstAmount = 0;

            List<string> errorMessages = new List<string>();
            List<string> successLogs = new List<string>();

            DataTable dt1;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            dt1 = (DataTable)ViewState["dt"];

            if (dt1 != null)
            {
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    CheckBox chk = (CheckBox)(Gridview_Product.Rows[i].FindControl("chk"));
                    if (chk.Checked)
                    {
                        checkedCount++;
                        try
                        {
                            string ProductId = ((Label)Gridview_Product.Rows[i].FindControl("Product_id")).Text;
                            string ProductName = ((Label)Gridview_Product.Rows[i].FindControl("Product_name")).Text;
                            string Quantity = ((Label)Gridview_Product.Rows[i].FindControl("Quantity")).Text;
                            string DQnt = ((Label)Gridview_Product.Rows[i].FindControl("DeliveredQnt")).Text;
                            string SQnt = ((Label)Gridview_Product.Rows[i].FindControl("SQuantity")).Text;
                            string RQty = ((TextBox)Gridview_Product.Rows[i].FindControl("Qty")).Text;
                            string SailRate = ((Label)Gridview_Product.Rows[i].FindControl("sail_rate")).Text;
                            string GstPercentage = ((Label)Gridview_Product.Rows[i].FindControl("Service_tax_rate")).Text;
                            string InvStatus = ((Label)Gridview_Product.Rows[i].FindControl("InvStatus")).Text;

                            double quotedQuantity = string.IsNullOrEmpty(Quantity) ? 0 : Convert.ToDouble(Quantity);
                            double deliveredQuantity = string.IsNullOrEmpty(DQnt) ? 0 : Convert.ToDouble(DQnt);
                            double stockQuantity = string.IsNullOrEmpty(SQnt) ? 0 : Convert.ToDouble(SQnt);
                            double remainingQuantity = string.IsNullOrEmpty(RQty) ? 0 : Convert.ToDouble(RQty);

                            if (InvStatus == "Yes")
                            {
                                completedCount++;
                                errorMessages.Add($"⚠️ Product '{ProductName}' (Code: {ProductId}) is already invoiced.");
                            }
                            else if (remainingQuantity == 0)
                            {
                                pendingCount++;
                                errorMessages.Add($"❌ Invalid quantity for '{ProductName}' (Code: {ProductId}) - Please enter a valid QTY.");
                            }
                            else if (remainingQuantity > stockQuantity)
                            {
                                noStockCount++;
                                pendingCount++;
                                errorMessages.Add($"❌ Insufficient stock for '{ProductName}' (Code: {ProductId}) - Required: {remainingQuantity}, Available: {stockQuantity}.");
                            }
                            else
                            {
                                // Calculate amounts
                                double sailRate = string.IsNullOrEmpty(SailRate) ? 0 : Convert.ToDouble(SailRate);
                                double gstPercentage = string.IsNullOrEmpty(GstPercentage) ? 0 : Convert.ToDouble(GstPercentage);
                                double amountWithoutGst = remainingQuantity * sailRate;
                                double gstAmount = (amountWithoutGst * gstPercentage) / 100;
                                double amountWithGst = amountWithoutGst + gstAmount;

                                InvTotalAmountWithGst += amountWithGst;
                                InvTotalAmountWithOutGst += amountWithoutGst;
                                invTotalGstAmount += gstAmount;

                                // Update session values
                                Session["InvTotalAmountWithGst"] = InvTotalAmountWithGst;
                                Session["InvTotalAmountWithOutGst"] = InvTotalAmountWithOutGst;
                                Session["invTotalGstAmount"] = invTotalGstAmount;

                                // Insert into invoice table
                                string query = @"INSERT INTO tbl_Invoice_details 
                        (Quotation_no, Invoice_No, Product_id, Product_Code, Product_name, Quantity, sail_rate, Service_tax_rate, Total_sail_rate1, Total_sail_rate2, specification) 
                        VALUES 
                        (@Quotation_no, @Invoice_No, @Product_id, @Product_Code, @Product_name, @Quantity, @sail_rate, @Service_tax_rate, @Total_sail_rate1, @Total_sail_rate2, @specification)";

                                List<SqlParameter> pram = new List<SqlParameter>
                        {
                            new SqlParameter("@Quotation_no", quno),
                            new SqlParameter("@Invoice_No", invoice_no),
                            new SqlParameter("@Product_id", ProductId),
                            new SqlParameter("@Product_Code", ((Label)Gridview_Product.Rows[i].FindControl("Product_Code")).Text),
                            new SqlParameter("@Product_name", ProductName),
                            new SqlParameter("@Quantity", remainingQuantity),
                            new SqlParameter("@sail_rate", sailRate),
                            new SqlParameter("@Service_tax_rate", gstPercentage),
                            new SqlParameter("@Total_sail_rate1", amountWithGst),
                            new SqlParameter("@Total_sail_rate2", amountWithoutGst),
                            new SqlParameter("@specification", ((Label)Gridview_Product.Rows[i].FindControl("specification")).Text)
                        };

                                DbCL.SPExecDB(query, pram.ToArray());

                                successLogs.Add($"✅ Product '{ProductName}' (Code: {ProductId}) successfully invoiced with Quantity: {remainingQuantity}, Amount: {amountWithGst}");

                                // Update invoice status
                                string invStatus = ((deliveredQuantity + remainingQuantity) == quotedQuantity) ? "Yes" : "No";
                                updateqtableforproduct(quno, ProductId, ProductName, invStatus);

                                // Update stock
                                updatestock1(ProductId, ProductName, RQty);
                                successLogs.Add($"✅ Stock updated for '{ProductName}' (Code: {ProductId}) - Deducted: {remainingQuantity}");

                                if (invStatus == "No")
                                {
                                    pendingCount++;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            errorMessages.Add($"⚠️ Error processing Product '{Gridview_Product.Rows[i].FindControl("Product_name")}' - {ex.Message}");
                        }
                    }
                }

                // Show error messages if any
                if (errorMessages.Count > 0)
                {
                    ShowErrorMessageSpcl(string.Join("<br>", errorMessages));
                }

                // Show success messages if any
                if (successLogs.Count > 0)
                {
                    ShowSucessMessageSpcl(string.Join("<br>", successLogs));
                }
            }

            Status = DetermineStatus(completedCount, pendingCount, noStockCount, checkedCount);
            return Status;
        }


        private string InsertSelectedProduct_old270325(string invoice_no, string quno, ref int checkedCount)
        {
            string Status = string.Empty;
            int completedCount = 0;
            int noStockCount = 0;
            int pendingCount = 0;
            double InvTotalAmountWithGst = 0;
            double InvTotalAmountWithOutGst = 0;
            double invTotalGstAmount = 0;

            List<string> errorMessages = new List<string>();
            List<string> successLogs = new List<string>(); // ✅ Success log list

            DataTable dt1;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            dt1 = (DataTable)ViewState["dt"];

            if (dt1 != null)
            {
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    CheckBox chk = (CheckBox)(Gridview_Product.Rows[i].FindControl("chk"));
                    if (chk.Checked)
                    {
                        checkedCount++;
                        try
                        {
                            List<string> missingFields = new List<string>();

                            string HSN = ((Label)Gridview_Product.Rows[i].FindControl("Product_Code")).Text; //----Product HSN Code ___
                            string ProductId = ((Label)Gridview_Product.Rows[i].FindControl("Product_id")).Text; //--------Product ID
                            string ProductName = ((Label)Gridview_Product.Rows[i].FindControl("Product_name")).Text;
                            string Quantity = ((Label)Gridview_Product.Rows[i].FindControl("Quantity")).Text;
                            string DQnt = ((Label)Gridview_Product.Rows[i].FindControl("DeliveredQnt")).Text;
                            string SQnt = ((Label)Gridview_Product.Rows[i].FindControl("SQuantity")).Text;
                            string RQty = ((TextBox)Gridview_Product.Rows[i].FindControl("Qty")).Text;
                            string SailRate = ((Label)Gridview_Product.Rows[i].FindControl("sail_rate")).Text;
                            string GstPercentage = ((Label)Gridview_Product.Rows[i].FindControl("Service_tax_rate")).Text;
                            string specifai = ((Label)Gridview_Product.Rows[i].FindControl("specification")).Text;
                            string InvStatus = ((Label)Gridview_Product.Rows[i].FindControl("InvStatus")).Text;

                            double quotedQuantity = string.IsNullOrEmpty(Quantity) ? 0 : Convert.ToDouble(Quantity);
                            double deliveredQuantity = string.IsNullOrEmpty(DQnt) ? 0 : Convert.ToDouble(DQnt);
                            double stockQuantity = string.IsNullOrEmpty(SQnt) ? 0 : Convert.ToDouble(SQnt);
                            double remainingQuantity = string.IsNullOrEmpty(RQty) ? 0 : Convert.ToDouble(RQty);

                            if (InvStatus == "Yes")
                            {
                                completedCount++;
                                missingFields.Add($"⚠️ Product '{ProductName}' (Code: {ProductId}) is already invoiced.");
                            }
                            else
                            {
                                if (remainingQuantity > stockQuantity)
                                {
                                    noStockCount++;
                                    missingFields.Add($"❌ Insufficient stock for '{ProductName}' (Code: {ProductId}) - Required: {remainingQuantity}, Available: {stockQuantity}");
                                    // ✅ Increment pendingCount when stock is insufficient, as this product is still pending
                                    pendingCount++;
                                }
                                else if (remainingQuantity == 0 && stockQuantity != 0 && deliveredQuantity == quotedQuantity)
                                {
                                    completedCount++;
                                    string invStatus = ((deliveredQuantity + remainingQuantity) == quotedQuantity) ? "Yes" : "No";
                                    updateqtableforproduct(quno, ProductId, ProductName, invStatus);
                                    missingFields.Add($"⚠️ Product '{ProductName}' (Code: {ProductId}) is already invoiced.");
                                }
                                else if (remainingQuantity == 0 && stockQuantity != 0)
                                {
                                    pendingCount++;
                                    missingFields.Add($"❌ Invalid quantity for '{ProductName}' (Code: {ProductId}) - Please enter a valid QTY.");
                                }
                                else if (remainingQuantity == 0 && stockQuantity == 0)
                                {
                                    pendingCount++;
                                    noStockCount++;
                                    missingFields.Add($"❌ NO Stock, Invalid quantity for '{ProductName}' (Code: {ProductId}) - Please enter a valid QTY.");
                                }
                                else
                                {
                                    double sailRate = string.IsNullOrEmpty(SailRate) ? 0 : Convert.ToDouble(SailRate);
                                    double gstPercentage = string.IsNullOrEmpty(GstPercentage) ? 0 : Convert.ToDouble(GstPercentage);
                                    double amountWithoutGst = remainingQuantity * sailRate;
                                    double gstAmount = (amountWithoutGst * gstPercentage) / 100;
                                    double amountWithGst = amountWithoutGst + gstAmount;

                                    InvTotalAmountWithGst += amountWithGst;
                                    InvTotalAmountWithOutGst += amountWithoutGst;
                                    invTotalGstAmount += gstAmount;

                                    Session["InvTotalAmountWithGst"] = InvTotalAmountWithGst;
                                    Session["InvTotalAmountWithOutGst"] = InvTotalAmountWithOutGst;
                                    Session["invTotalGstAmount"] = invTotalGstAmount;

                                    string query = @"INSERT INTO tbl_Invoice_details (Quotation_no, Invoice_No, Product_id, Product_Code, Product_name, Quantity, sail_rate, Service_tax_rate, Total_sail_rate1, Total_sail_rate2, specification) 
                                    VALUES (@Quotation_no, @Invoice_No, @Product_id, @Product_Code, @Product_name, @Quantity, @sail_rate, @Service_tax_rate, @Total_sail_rate1, @Total_sail_rate2, @specification)";

                                    List<SqlParameter> pram = new List<SqlParameter>
                                    {
                                        new SqlParameter("@Quotation_no", quno),
                                        new SqlParameter("@Invoice_No", invoice_no),
                                        new SqlParameter("@Product_id", ProductId),
                                        new SqlParameter("@Product_Code", HSN),
                                        new SqlParameter("@Product_name", ProductName),
                                        new SqlParameter("@Quantity", remainingQuantity),
                                        new SqlParameter("@sail_rate", sailRate),
                                        new SqlParameter("@Service_tax_rate", gstPercentage),
                                        new SqlParameter("@Total_sail_rate1", amountWithGst),
                                        new SqlParameter("@Total_sail_rate2", amountWithoutGst),
                                        new SqlParameter("@specification", specifai)
                                    };

                                    DbCL.SPExecDB(query, pram.ToArray());

                                    successLogs.Add($"✅ Product '{ProductName}' (Code: {ProductId}) successfully invoiced with Quantity: {remainingQuantity}, Amount: {amountWithGst}");

                                    string invStatus = ((deliveredQuantity + remainingQuantity) == quotedQuantity) ? "Yes" : "No";
                                    updateqtableforproduct(quno, ProductId, ProductName, invStatus);
                                    //successLogs.Add($"✅ Invoice status updated for '{ProductName}' (Code: {ProductId}) - Status: {invStatus}");

                                    updatestock1(ProductId, ProductName, RQty);
                                    successLogs.Add($"✅ Stock updated for '{ProductName}' (Code: {ProductId}) - Deducted: {remainingQuantity}");

                                    if (invStatus == "No")
                                    {
                                        pendingCount++;
                                    }
                                }
                            }

                            if (missingFields.Count > 0)
                            {
                                errorMessages.Add(string.Join("<br>", missingFields));
                            }
                        }
                        catch (Exception ex)
                        {
                            errorMessages.Add($"⚠️ Error processing Product '{Gridview_Product.Rows[i].FindControl("Product_name")}' - {ex.Message}");
                        }
                    }
                }

                if (errorMessages.Count > 0)
                {
                    ShowErrorMessageSpcl(string.Join("<br>", errorMessages));
                }

                if (successLogs.Count > 0)
                {
                    ShowSucessMessageSpcl(string.Join("<br>", successLogs));
                }
            }

            Status = DetermineStatusNew(completedCount, pendingCount, noStockCount, checkedCount);
            return Status;
        }

        private string DetermineStatusNew(int completedCount, int pendingCount, int noStockCount, int checkedCount)
        {
            if (completedCount == checkedCount && pendingCount == 0 && noStockCount == 0)
                return "Completed"; // ✅ All items invoiced successfully

            if (completedCount > 0 && pendingCount > 0 && noStockCount == 0)
                return "Partial"; // ✅ Some invoiced, some still pending

            if (completedCount > 0 && noStockCount > 0 && pendingCount == 0)
                return "Partial + No Stock"; // ✅ Some invoiced, some out of stock

            if (completedCount > 0 && pendingCount > 0 && noStockCount > 0)
                return "Partial + No Stock + Pending"; // ✅ Some invoiced, some pending, some out of stock

            if (completedCount == 0 && pendingCount > 0 && noStockCount == 0)
                return "Pending"; // ✅ All pending, but stock is available

            if (completedCount == 0 && pendingCount > 0 && noStockCount > 0)
                return "Pending + No Stock"; // ✅ All pending, and some items are out of stock

            if (completedCount == 0 && pendingCount == 0 && noStockCount > 0)
                return "No Stock"; // ✅ No items can be invoiced due to no stock

            return "No Action"; // ✅ Default case if no changes happened
        }


        private string DetermineStatus(int completedCount, int pendingCount, int noStockCount, int checkedCount)
        {
            bool hasCompleted = completedCount > 0;
            bool hasPending = pendingCount > 0;
            bool hasNoStock = noStockCount > 0;

            if (hasCompleted && !hasPending && !hasNoStock)
                return "Completed"; // ✅ All items invoiced

            if (hasCompleted && hasPending && !hasNoStock)
                return "Partial"; // ✅ Some invoiced, some pending

            if (hasCompleted && hasNoStock && !hasPending) // ✅ Covers both "Partial + No Stock" cases
                return "Partial + No Stock"; // Some invoiced, some out of stock

            if (hasCompleted && hasNoStock && hasPending) // ✅ Covers both "Partial + No Stock" cases
                return "Partial + No Stock + Pending"; // Some invoiced, some out of stock

            if (!hasCompleted && hasPending && !hasNoStock)
                return "Pending"; // ✅ All pending

            if (!hasCompleted && hasPending && hasNoStock)
                return "Pending + No Stock"; // ✅ All pending, some out of stock

            if (!hasCompleted && !hasPending && hasNoStock)
                return "No Stock"; // ✅ All out of stock

            return "No Action"; // ✅ Default case
        }


        private string DetermineStatus_old(int completedCount, int pendingCount, int noStockCount, int checkedCount)
        {
            bool hasCompleted = completedCount > 0;
            bool hasPending = pendingCount > 0;
            bool hasNoStock = noStockCount > 0;

            if (hasCompleted && !hasPending && !hasNoStock)
            {
                return "Completed"; // All items are invoiced
            }
            else if (hasCompleted && hasPending && !hasNoStock)
            {
                return "Partial"; // Some invoiced, some pending
            }
            else if (hasCompleted && hasPending && hasNoStock)
            {
                return "Partial + No Stock"; // Some invoiced, some pending, some out of stock
            }
            else if (!hasCompleted && hasPending && !hasNoStock)
            {
                return "Pending"; // All pending
            }
            else if (!hasCompleted && !hasPending && hasNoStock)
            {
                return "No Stock"; // All out of stock
            }
            else if (hasCompleted && !hasPending && hasNoStock)
            {
                return "Partial + No Stock"; // Some invoiced, some out of stock
            }
            else if (!hasCompleted && hasPending && hasNoStock) // ✅ NEW CONDITION ADDED
            {
                return "Pending + No Stock"; // All pending, and some out of stock
            }
            else
            {
                return "No Action"; // Default case
            }
        }

        private void updateqtableforproduct(string quno, string productCode, string productName, string status)
        {
            string query = "update tbl_Quotaion_details set InvStatus=@InvStatus where Quotation_no=@Quotation_no and Product_Code=@Product_Code and Product_name=@Product_name";
            SqlParameter[] pram =
                {
                   new SqlParameter("@InvStatus",status),
                   new SqlParameter("@Quotation_no",quno),
                   new SqlParameter("@Product_Code",productCode),
                   new SqlParameter("@Product_name",productName)
                };
            DbCL.SPExecDB(query, pram);
        }

        private void updatestock()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Product_id,Product_name,Quantity from tbl_Quotaion_details where Quotation_no='" + lblQuotation_no.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            while (re.Read())
            {
                string product_code = re["Product_id"].ToString();
                string Product_name = re["Product_name"].ToString();
                string Quantity = re["Quantity"].ToString();
                updatestock1(product_code, Product_name, Quantity);
            }
            DbCL.Conn.Close();
        }

        private void updatestock1(string product_code, string Product_name, string Quantity)
        {
            DbCL.executeRdr("UPDATE tbl_stock SET Quantity = (CAST(Quantity AS FLOAT) - " + Quantity.ToString() + ") " +
                "WHERE Product_id = '" + product_code.ToString() + "' AND Product_name = '" + Product_name.ToString() + "'");

        }

        private string findstock()
        {
            string stock = "Yes";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Quotaion_details where Quotation_no='" + lblQuotation_no.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            while (re.Read())
            {
                string product_code = re["Product_Code"].ToString();
                string product_Name = re["Product_name"].ToString();
                string Quantity = re["Quantity"].ToString();
                string status = findstock1(product_code, product_Name, Quantity);
                if (status == "No")
                {
                    stock = "No";
                }
            }
            DbCL.Conn.Close();
            return stock;

        }

        //private string findstock1(string product_code, string product_Name, string Quantity)
        //{
        //    string stock = "Yes";
        //    string Qt;
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    string cmdstring = "select Quantity from tbl_stock where Product_id='" + product_code + "' and Product_name='" + product_Name + "'";
        //    SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
        //    SqlDataReader re = cmd.ExecuteReader();
        //    if (re.Read())
        //    {
        //        Qt = re["Quantity"].ToString();
        //    }
        //    else
        //    {
        //        Qt = "0";
        //    }
        //    int Qt1 = Convert.ToInt32(Qt);
        //    int Qt2 = Convert.ToInt32(Quantity);
        //    if (Qt1 >= Qt2)
        //    {
        //        stock = "Yes";
        //    }
        //    else
        //    {
        //        stock = "No";
        //    }
        //    DbCL.Conn.Close();
        //    return stock;

        //}

        private string findstock1(string product_code, string product_Name, string Quantity)
        {
            string stock = "No"; // Default to "No"
            int availableQuantity = 0;

            try
            {
                DbCL.Sqlconnection();
                DbCL.ConnectDb();

                string cmdstring = "SELECT Quantity FROM tbl_stock WHERE Product_id = @Product_id AND Product_name = @ProductName";
                using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
                {
                    cmd.Parameters.AddWithValue("@Product_id", product_code ?? string.Empty);
                    cmd.Parameters.AddWithValue("@ProductName", product_Name ?? string.Empty);

                    using (SqlDataReader re = cmd.ExecuteReader())
                    {
                        if (re.Read())
                        {
                            int qty = 0;
                            // Ensure Quantity is not null or empty, default to 0
                            availableQuantity = re["Quantity"] != DBNull.Value && int.TryParse(re["Quantity"].ToString(), out qty) ? qty : 0;
                        }
                    }
                }
                int qt2 = 0;
                // Ensure Quantity input is treated correctly
                int requestedQuantity = int.TryParse(Quantity, out qt2) ? qt2 : 0;

                if (availableQuantity >= requestedQuantity)
                {
                    stock = "Yes";
                }
            }
            catch
            {
                // Log or handle the exception as needed
                //Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                if (DbCL.Conn != null && DbCL.Conn.State == ConnectionState.Open)
                {
                    DbCL.Conn.Close();
                }
            }

            return stock;
        }

        private string findstock2(string product_code, string product_Name)
        {
            string availableQuantity = "0";
            try
            {
                DbCL.Sqlconnection();
                DbCL.ConnectDb();

                string cmdstring = "SELECT Quantity FROM tbl_stock WHERE Product_id = @ProductId AND Product_name = @ProductName";
                using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
                {
                    cmd.Parameters.AddWithValue("@ProductId", product_code ?? string.Empty);
                    cmd.Parameters.AddWithValue("@ProductName", product_Name ?? string.Empty);

                    using (SqlDataReader re = cmd.ExecuteReader())
                    {
                        if (re.Read())
                        {
                            // Ensure Quantity is not null or empty, default to "0"
                            availableQuantity = re["Quantity"] != DBNull.Value && !string.IsNullOrEmpty(re["Quantity"].ToString()) ? re["Quantity"].ToString() : "0";
                        }
                    }
                }
            }
            catch
            {
                // Log or handle the exception as needed
                //Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                if (DbCL.Conn != null && DbCL.Conn.State == ConnectionState.Open)
                {
                    DbCL.Conn.Close();
                }
            }

            return availableQuantity;
        }

    }
}