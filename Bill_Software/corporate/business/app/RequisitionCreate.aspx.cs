using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm66 : System.Web.UI.Page
    {
        // Ponytail #2: ALL cart/summary state is ViewState-backed, NOT static.
        private string ConnString => ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;

        private DataTable CartData
        {
            get { return ViewState["CartData"] as DataTable; }
            set { ViewState["CartData"] = value; }
        }

        private decimal GrossAmount
        {
            get { return ViewState["GrossAmount"] != null ? (decimal)ViewState["GrossAmount"] : 0m; }
            set { ViewState["GrossAmount"] = value; }
        }

        private decimal ServiceTax
        {
            get { return ViewState["ServiceTax"] != null ? (decimal)ViewState["ServiceTax"] : 0m; }
            set { ViewState["ServiceTax"] = value; }
        }

        private decimal TotalSailRateDetails
        {
            get { return ViewState["TotalSailRateDetails"] != null ? (decimal)ViewState["TotalSailRateDetails"] : 0m; }
            set { ViewState["TotalSailRateDetails"] = value; }
        }

        private decimal TotalService
        {
            get { return ViewState["TotalService"] != null ? (decimal)ViewState["TotalService"] : 0m; }
            set { ViewState["TotalService"] = value; }
        }

        private decimal SubTotal
        {
            get { return ViewState["SubTotal"] != null ? (decimal)ViewState["SubTotal"] : 0m; }
            set { ViewState["SubTotal"] = value; }
        }

        // Transient — used only within a single method call, never persisted across postbacks
        private DataTable first_datatable;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                GrossAmount = 0;
                ServiceTax = 0;
                TotalSailRateDetails = 0;
                TotalService = 0;
                SubTotal = 0;
                CartData = new DataTable("Table");

                // Ponytail #3 + #1: Parameterized, CompanyID-scoped
                using (var cn = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand("SELECT Client_Name FROM tbl_Client WHERE CompanyID = @CompanyID ORDER BY Client_Name", cn))
                {
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        cmbClient.DataSource = dt;
                        cmbClient.DataTextField = "Client_Name";
                        cmbClient.DataBind();
                    }
                }
                cmbClient.Items.Insert(0, new ListItem("--Select Client--", ""));

                txtquotationDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Panel1.Visible = true;
            cmbClient.Enabled = false;
            BindListitem();
            Bindquotationno();
            txtquotationDate.Enabled = false;
            RadioButtonList1.Enabled = false;
            Label1.Text = "1";
        }

        private void BindListitem()
        {
            cmbproduct_service.Items.Clear();
            cmbproduct_service.Items.Add("--Select--");

            string sql = RadioButtonList1.SelectedIndex == 0
                ? "SELECT Product_Name FROM tbl_parentProduct WHERE CompanyID = @CompanyID ORDER BY Product_Name"
                : "SELECT Service_name FROM tbl_Service ORDER BY Service_name";

            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                if (RadioButtonList1.SelectedIndex == 0)
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                cn.Open();
                using (var re = cmd.ExecuteReader())
                {
                    while (re.Read())
                        cmbproduct_service.Items.Add(re.GetValue(0).ToString());
                }
            }
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            Panel2.Visible = true;

            // Ponytail #3: Parameterized queries replace string concatenation
            string sql;
            SqlParameter param;
            if (RadioButtonList1.SelectedIndex == 0)
            {
                sql = "SELECT Product_code, Sub_Prod_Name, Sail_Rate, Tax_Rate FROM tbl_Product WHERE Product_Name = @Name AND CompanyID = @CompanyID";
                param = new SqlParameter("@Name", cmbproduct_service.Text);
            }
            else
            {
                sql = "SELECT Service_code, Service_name, Sail_rate, Tax_rate FROM tbl_Service WHERE Service_name = @Name";
                param = new SqlParameter("@Name", cmbproduct_service.Text);
            }

            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(param);
                if (RadioButtonList1.SelectedIndex == 0)
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        // Re-read into a DataTable for the grid
                        dr.Close();
                        using (var cmd2 = new SqlCommand(sql, cn))
                        {
                            cmd2.Parameters.Add(param);
                            if (RadioButtonList1.SelectedIndex == 0)
                                cmd2.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                            using (var da = new SqlDataAdapter(cmd2))
                            {
                                first_datatable = new DataTable();
                                da.Fill(first_datatable);
                            }
                        }

                        if (Label1.Text == "1")
                            newgrid1();
                        else
                            newgrid();

                        Label1.Text = (Convert.ToInt32(Label1.Text) + 1).ToString();
                    }
                }
            }

            cmbproduct_service.SelectedIndex = 0;
            gd_Service_Product.DataSource = CartData;
            gd_Service_Product.DataBind();
            ViewState["dt"] = CartData;
        }

        private void newgrid1()
        {
            CartData.Columns.Add("Ser_pro_code", typeof(string));
            CartData.Columns.Add("Ser_pro_Name", typeof(string));
            CartData.Columns.Add("Sale_rate", typeof(string));
            CartData.Columns.Add("service_Tax_Rate", typeof(string));
            AppendRows();
        }

        private void newgrid()
        {
            AppendRows();
        }

        private void AppendRows()
        {
            foreach (DataRow src in first_datatable.Rows)
            {
                DataRow dr = CartData.NewRow();
                dr["Ser_pro_code"] = src[0].ToString();
                dr["Ser_pro_Name"] = src[1].ToString();
                dr["Sale_rate"] = src[2].ToString();
                dr["service_Tax_Rate"] = src[3].ToString();
                CartData.Rows.Add(dr);
            }
        }

        private void Bindquotationno()
        {
            string c = cmbClient.Text.Trim();
            string f = c.Substring(0, 1);
            for (int i = 0; i < c.Length; i++)
            {
                if (c.Substring(i, 1) == " ")
                {
                    int next = i + 1;
                    if (next < c.Length)
                    {
                        string tt = c.Substring(next, 1);
                        if (tt == "(" && next + 1 < c.Length)
                            tt = c.Substring(next + 1, 1);
                        f = f + tt;
                    }
                }
            }
            f = "I2I/" + f + "/";
            int j = idreturn();
            j = j + 1;
            f = f + j.ToString();
            lblqno.Text = f.ToString();
        }

        private int idreturn()
        {
            int b = 0;
            // Ponytail #3 + #1: Parameterized, CompanyID-scoped
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(
                "SELECT TOP 1 id FROM tbl_requisitionBankDetails WHERE CompanyID = @CompanyID ORDER BY id DESC", cn))
            {
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                cn.Open();
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    b = Convert.ToInt32(result);
            }
            return b;
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            DataTable dt1 = ViewState["dt"] as DataTable;
            if (dt1 == null) return;

            string date1 = "", no = "", bank = "", ifsc = "", cgstorigst = "";

            using (var cn = new SqlConnection(ConnString))
            {
                cn.Open();
                using (var tran = cn.BeginTransaction())
                {
                    try
                    {
                        for (int i = 0; i < dt1.Rows.Count; i++)
                        {
                            CheckBox chk = (CheckBox)(gd_Service_Product.Rows[i].FindControl("chk"));
                            if (chk == null || !chk.Checked) continue;

                            string Ser_pro_code = ((Label)gd_Service_Product.Rows[i].FindControl("Ser_pro_code")).Text;
                            string Ser_pro_Name = ((Label)gd_Service_Product.Rows[i].FindControl("Ser_pro_Name")).Text;
                            string specification = ((TextBox)gd_Service_Product.Rows[i].FindControl("specification")).Text;
                            string proname = Ser_pro_Name + specification;
                            string Quantity = ((TextBox)gd_Service_Product.Rows[i].FindControl("Quantity")).Text;
                            string Sale_rate = ((TextBox)gd_Service_Product.Rows[i].FindControl("Sale_rate")).Text;
                            string service_Tax_Rate = ((Label)gd_Service_Product.Rows[i].FindControl("service_Tax_Rate")).Text;

                            int Qtity = Convert.ToInt32(Quantity);
                            double Salerate = Convert.ToDouble(Sale_rate);
                            double serviceTaxRate = Convert.ToDouble(service_Tax_Rate);
                            double QtitySalerateTotal = Math.Round((Qtity * Salerate), 2);
                            double gst = Math.Round(((QtitySalerateTotal * serviceTaxRate) / 100), 2);
                            double cgst = Math.Round((gst / 2), 2);
                            double gstPlusAmount = QtitySalerateTotal + gst;

                            // Ponytail #3 + #1: Parameterized, CompanyID-scoped
                            using (var cmd = new SqlCommand(
                                "INSERT INTO tbl_requisition (requeno, ProductCode, ProductName, Baserate, quantity, gstper, productAmo, gstamo, cgstamo, sgstmo, productAmoGstmo, CompanyID) " +
                                "VALUES (@requeno, @ProductCode, @ProductName, @Baserate, @quantity, @gstper, @productAmo, @gstamo, @cgstamo, @sgstmo, @productAmoGstmo, @CompanyID)", cn, tran))
                            {
                                cmd.Parameters.AddWithValue("@requeno", lblqno.Text);
                                cmd.Parameters.AddWithValue("@ProductCode", Ser_pro_code);
                                cmd.Parameters.AddWithValue("@ProductName", proname);
                                cmd.Parameters.AddWithValue("@Baserate", Salerate);
                                cmd.Parameters.AddWithValue("@quantity", Qtity);
                                cmd.Parameters.AddWithValue("@gstper", serviceTaxRate);
                                cmd.Parameters.AddWithValue("@productAmo", QtitySalerateTotal);
                                cmd.Parameters.AddWithValue("@gstamo", gst);
                                cmd.Parameters.AddWithValue("@cgstamo", cgst);
                                cmd.Parameters.AddWithValue("@sgstmo", cgst);
                                cmd.Parameters.AddWithValue("@productAmoGstmo", gstPlusAmount);
                                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        if (RadioButtonList2.SelectedIndex == 0)
                            date1 = " Dated:" + txtcashDate.Text;
                        else if (RadioButtonList2.SelectedIndex == 3)
                        {
                            date1 = " Dated:" + txtneftdate.Text;
                            no = txtneftnumber.Text + ",";
                            ifsc = txtifscCode.Text;
                        }
                        else
                        {
                            date1 = " Dated:" + txtdddate.Text;
                            no = txtDDno.Text + ",";
                            bank = txtBankName.Text;
                        }

                        cgstorigst = radioGstType.SelectedIndex == 0 ? "cgst" : "igst";
                        string clientaddress = searchaddress(cmbClient.Text);

                        using (var cmd1 = new SqlCommand(
                            "INSERT INTO tbl_requisitionBankDetails (requeno, reqDate, CompName, address, paytype, chkno, bankname, ifscCode, date, cgstorsgst, CompanyID) " +
                            "VALUES (@requeno, @reqDate, @CompName, @address, @paytype, @chkno, @bankname, @ifscCode, @date, @cgstorsgst, @CompanyID)", cn, tran))
                        {
                            cmd1.Parameters.AddWithValue("@requeno", lblqno.Text);
                            cmd1.Parameters.AddWithValue("@reqDate", txtquotationDate.Text);
                            cmd1.Parameters.AddWithValue("@CompName", cmbClient.Text);
                            cmd1.Parameters.AddWithValue("@address", clientaddress);
                            cmd1.Parameters.AddWithValue("@paytype", RadioButtonList2.Text);
                            cmd1.Parameters.AddWithValue("@chkno", no);
                            cmd1.Parameters.AddWithValue("@bankname", bank);
                            cmd1.Parameters.AddWithValue("@ifscCode", ifsc);
                            cmd1.Parameters.AddWithValue("@date", date1);
                            cmd1.Parameters.AddWithValue("@cgstorsgst", cgstorigst);
                            cmd1.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                            cmd1.ExecuteNonQuery();
                        }

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        private string searchaddress(string clientname)
        {
            string add = "";
            // Ponytail #3 + #1: Parameterized, CompanyID-scoped
            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(
                "SELECT (Address1 + ' ' + Address2 + ', ' + City + ', ' + pin) AS MainAdd FROM tbl_Client WHERE Client_Name = @Client_Name AND CompanyID = @CompanyID", cn))
            {
                cmd.Parameters.AddWithValue("@Client_Name", clientname);
                cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
                cn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                        add = rdr["MainAdd"].ToString();
                }
            }
            return add;
        }

        protected void RadioButtonList2_SelectedIndexChanged(object sender, EventArgs e)
        {
            First.Visible = RadioButtonList2.SelectedIndex == 0;
            Second.Visible = RadioButtonList2.SelectedIndex != 0 && RadioButtonList2.SelectedIndex != 3;
            Third.Visible = RadioButtonList2.SelectedIndex == 3;
        }
    }
}
