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
    public partial class WebForm19 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dtf = new DataTable();
        DataTable dtnd = new DataTable();
        DataTable dt3rd = new DataTable();
        DataTable dtphasetype = new DataTable();
        DataTable dtPhasefees = new DataTable();
        DataTable dtSTerm = new DataTable();
        DataTable dtPCat = new DataTable();

        DataTable dtPservice = new DataTable();
        DataTable dtPCat1 = new DataTable();

        public int count = 1;
        public DataTable first_datatable;
        public static DataTable Dt = new DataTable("Table");
        //public static decimal Gross_amount = 0;
        public static decimal Service_tax = 0;
        public static decimal total_sail_rate_details = 0;
        //public static decimal total_Service = 0;
        //public static decimal sub_total = 0;

        public static decimal new_sub_total = 0;
        public static decimal new_Gross_amount = 0;
        public static decimal discounted_rate = 0;
        public static decimal new_total_Service = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                //Gross_amount = 0;
                new_Gross_amount = 0;
                Service_tax = 0;
                total_sail_rate_details = 0;
                //total_Service = 0;
                //sub_total = 0;

                new_total_Service = 0;
                new_sub_total = 0;

                // Set default values on initial page load
                txt_clientrefname.Text = "N/A";
                txt_clientrefid.Text = "N/A";
                txt_clientrefdate.Text = "01-Jan-2000";

                // Ensure the fields are disabled by default
                txt_clientrefname.Attributes["disabled"] = "disabled";
                txt_clientrefid.Attributes["disabled"] = "disabled";
                txt_clientrefdate.Attributes["disabled"] = "disabled";

                //Dt = null;
                Dt = new DataTable("Table");
                DbCL.FillCombo(cmbClient, "select Client_Name from tbl_Client order by Client_Name");
                DbCL.FillCombo(ddlPlaceOfSupply, "Select City_Name from tbl_City order by City_Name asc");
                txtquotationDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");

            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {

            Panel1.Visible = true;
            cmbClient.Enabled = false;
            //if (RadioButtonList2.SelectedIndex == 0)
            //{
            BindListitemNew();
            //}
            //else
            //{
            //    //BindListitem();
            //}

            BindclientID();
            //Bindquotationno();

            string clientcode = lblclientID.Text;
            //bindFactoryAddress(clientcode);

            txtquotationDate.Enabled = false;
            //RadioButtonList1.Enabled = false;
            Label1.Text = "1";
        }

        private void BindListitemNew()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "";
            //if (RadioButtonList1.SelectedIndex == 0)
            //{
            //cmdstring = "select Product_Name from tbl_Product order by Product_Name";
            cmdstring = "select ProductOrServiceCat from tbl_NewparentProduct order by ProductOrServiceCat";
            //}

            //else
            //{
            //    cmdstring = "select Service_name from tbl_Service order by Service_name";
            //}

            cmbproduct_service.Items.Clear();
            cmbproduct_service.Items.Add("--Select--");
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            while (re.Read())
            {
                cmbproduct_service.Items.Add(re.GetValue(0).ToString());
                //listProduct_Service.Items.Add(re.GetValue(0).ToString());
            }
            DbCL.Conn.Close();

        }

        private void Bindquotationno_old()
        {
            string p = null;
            string c = cmbClient.Text.Trim();
            string f = c.Substring(0, 1);
            string tt;
            for (int i = 0; i < c.Length; i++)
            {
                p = c.Substring(i, 1);
                if (p == " ")
                {
                    tt = c.Substring((i + 1), 1);
                    if (tt == "(")
                    {
                        tt = c.Substring((i + 2), 1);
                    }
                    f = f + tt;
                }
            }
            //f = "I2I/"+f+"/";
            f = "" + f + "/";
            string ss = findmonth();
            f = f + ss;
            int j = idreturn();
            j = j + 1;
            f = f + j.ToString();
            lblqno.Text = f.ToString();

        }

        private void Bindquotationno()
        {
            string prefix = "QTN/FE/";  // Default prefix
            string ss = findmonth();  // Get financial year format (e.g., "24-25/")
            int j = idreturn();  // Get last serial number
            j = j + 1;  // Increment serial number
            string quotationNo = prefix + ss + j.ToString();  // Construct final quotation number
            lblqno.Text = quotationNo;  // Assign to label
        }

        private int idreturn()
        {
            string a = null;
            int b = 0;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string date1 = txtquotationDate.Text;
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
            string cmdstring = "select Sl_no from tbl_Quotation where ID=(select max(ID) from tbl_Quotation where cast(Quotation_date as datetime) between '" + date5.ToString() + "' and '" + date6.ToString() + "')";
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
            string a = txtquotationDate.Text.Substring(3, 3);
            string b = txtquotationDate.Text.Substring(9, 2);
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

        private void BindclientID()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Id from tbl_Client where Client_Name='" + cmbClient.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblclientID.Text = re["Client_Id"].ToString();
            }
            DbCL.Conn.Close();

        }
        //private void BindListitem()
        //{
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    string cmdstring = "";
        //    if (RadioButtonList1.SelectedIndex == 0)
        //    {
        //        cmdstring = "select Product_Name from tbl_parentProduct order by Product_Name";
        //    }
        //    else
        //    {
        //        cmdstring = "select Service_name from tbl_Service order by Service_name";
        //    }
        //    cmbproduct_service.Items.Clear();
        //    cmbproduct_service.Items.Add("--Select--");
        //    SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
        //    SqlDataReader re = cmd.ExecuteReader();
        //    while (re.Read())
        //    {
        //        cmbproduct_service.Items.Add(re.GetValue(0).ToString());
        //    }
        //    DbCL.Conn.Close();

        //}

        protected void Button2_Click(object sender, EventArgs e)
        {
            Panel2.Visible = true;
            //Panel3.Visible = true;
            gridProdWithCat.Visible = true;

            DataTable dtproductWithCat = new DataTable();
            //string cmdstring = "select Product_code as Ser_pro_code,Sub_Prod_Name as Ser_pro_Name,Sail_Rate as Sale_rate,Tax_Rate as service_Tax_Rate from tbl_NewProduct where Product_Name='" + cmbproduct_service.Text + "'";

            // string cmdstring = "select Id,Product_code,ProductOrServiceCat,ProductName,Type,Sail_Rate,Tax_Rate,Unit,Brand from tbl_NewProduct where ProductOrServiceCat=@ProductOrServiceCat";
            //string cmdstring = "select Id,Product_code,ProductOrServiceCat,ProductName,Type,Sail_Rate,Tax_Rate,Unit,Brand from tbl_NewProduct where ProductOrServiceCat=@ProductOrServiceCat order by Type,ProductName";
            string cmdstring = "select Id, Product_code, ProductID,ProductOrServiceCat,ProductName,Type,Sail_Rate,Tax_Rate,Unit,Brand from tbl_NewProduct where ProductOrServiceCat=@ProductOrServiceCat order by Type,ProductName";

            SqlParameter[] pram = {
                new SqlParameter("@ProductOrServiceCat",cmbproduct_service.Text)
            };
            dtproductWithCat = DbCL.SPreturn_dt(cmdstring, pram);
            if (dtproductWithCat.Rows.Count > 0)
            {
                gridProdWithCat.DataSource = dtproductWithCat;
                gridProdWithCat.DataBind();
                ViewState["dtprocat"] = dtproductWithCat;
            }

            //if (RadioButtonList2.SelectedIndex == 0)
            //{
            //    string cmdstring = "select Product_code,Sub_Prod_Name,Sail_Rate,Tax_Rate from tbl_NewProduct where Product_Name='" + cmbproduct_service.Text + "'";
            //    Binddata1(cmdstring);
            //}
            //else {

            //    if (RadioButtonList1.SelectedIndex == 0)
            //    {
            //        string cmdstring = "select Product_code,Sub_Prod_Name,Sail_Rate,Tax_Rate from tbl_Product where Product_Name='" + cmbproduct_service.Text + "'";
            //        Binddata1(cmdstring);
            //    }
            //    else
            //    {
            //        string cmdstring = "select Service_code,Service_name,Sail_rate,Tax_rate  from tbl_Service where Service_name='" + cmbproduct_service.Text + "'";
            //        Binddata1(cmdstring);
            //    }
            //}

            //cmbproduct_service.SelectedIndex = 0;

            //gd_Service_Product.DataSource = Dt;
            //gd_Service_Product.DataBind();
            //ViewState["dt"] = Dt;

            //bindservice();
            bindphaseType();

            btnAddProduct.Enabled = true;

        }

        //private void bindFactoryAddress(string clientcode)
        //{
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    string cmdstring = "select Address1+', '+City+', '+pin+', '+State from tbl_Client where Client_Id='" + clientcode + "'";
        //    SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
        //    SqlDataAdapter da = new SqlDataAdapter(cmd);
        //    SqlDataReader DR1 = cmd.ExecuteReader();
        //    while (DR1.Read())
        //    {
        //        FactoryAddress.Items.Add(DR1.GetValue(0).ToString());
        //    }
        //    DbCL.Conn.Close();
        //    bindRegAddress(clientcode);
        //    bindAddress(clientcode);
        //}

        //private void bindRegAddress(string clientcode)
        //{
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    string cmdstring = "select Address+', '+State+', '+City+', '+pin as regadd from tbl_ClientRegAddress where Client_Id='" + clientcode + "'";
        //    SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
        //    SqlDataAdapter da = new SqlDataAdapter(cmd);
        //    SqlDataReader DR1 = cmd.ExecuteReader();
        //    while (DR1.Read())
        //    {
        //        FactoryAddress.Items.Add(DR1.GetValue(0).ToString());
        //    }
        //    DbCL.Conn.Close();
        //}

        //private void bindAddress(string clientcode)
        //{
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    string cmdstring = "select [Address1] +', '+ [Address2]+', '+[city]+', '+[State]+', '+[pin] as address from tbl_Factory where Client_id='" + clientcode + "'";
        //    SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
        //    SqlDataAdapter da = new SqlDataAdapter(cmd);
        //    SqlDataReader DR1 = cmd.ExecuteReader();
        //    while (DR1.Read())
        //    {
        //        FactoryAddress.Items.Add(DR1.GetValue(0).ToString());
        //    }
        //    DbCL.Conn.Close();
        //}

        //private void bindservice()
        //{
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    string cmdstring = "select PrimaryService from tbl_PrimaryService order by id";
        //    SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
        //    SqlDataAdapter da = new SqlDataAdapter(cmd);
        //    SqlDataReader DR1 = cmd.ExecuteReader();
        //    while (DR1.Read())
        //    {
        //        listOfPrimaryService.Items.Add(DR1.GetValue(0).ToString());
        //    }
        //    DbCL.Conn.Close();
        //}

        private void bindphaseType()
        {
            string str = "select id, PaymentPhase from tbl_PaymentPhase order by id";
            dtphasetype = DbCL.SPreturn_dt(str, null);
            if (dtphasetype.Rows.Count > 0)
            {
                listPhaseType.Items.Clear();
                for (int i = 0; i < dtphasetype.Rows.Count; i++)
                {
                    listPhaseType.Items.Add(dtphasetype.Rows[i]["PaymentPhase"].ToString());
                }
            }
        }


        protected void listPhaseType_TextChanged(object sender, EventArgs e)
        {
            bindOurPhaseAmount();
        }


        private void bindOurPhaseAmount()
        {
            string phasetypename = null;
            string phasedesc = null;
            for (int i = 0; i < listPhaseType.Items.Count; i++)
            {
                if (listPhaseType.Items[i].Selected)
                {
                    if (ViewState["phaseAmountData"] != null)
                    {
                        dtPhasefees = (DataTable)ViewState["phaseAmountData"];
                        int count = dtPhasefees.Rows.Count + 1;

                        phasetypename = listPhaseType.Items[i].Text;
                        //phasedesc = bindphasedesc(phasetypename);
                        phasedesc = "";



                        string service = "";
                        string status = "NO";
                        for (int j = 0; j < dtPhasefees.Rows.Count; j++)
                        {
                            service = dtPhasefees.Rows[j]["PaymentPhase"].ToString();
                            if (service == phasetypename)
                            {
                                status = "YES";
                            }
                        }
                        if (status == "NO")
                        {
                            SearchPaymentPhaseFees(count, phasetypename, phasedesc);
                        }
                    }
                    else
                    {
                        phasetypename = listPhaseType.Items[i].Text;
                        //phasedesc = bindphasedesc(phasetypename);
                        phasedesc = "";
                        SearchPaymentPhaseFees(1, phasetypename, phasedesc);


                    }
                }
            }
        }

        private void SearchPaymentPhaseFees(int count, string phasetypename, string phasedesc)
        {
            DataRow dr;
            if (count == 1)
            {
                dtPhasefees.Columns.Add(new DataColumn("PaymentPhase", typeof(string)));
                dtPhasefees.Columns.Add(new DataColumn("PhaseDesc", typeof(string)));
                dtPhasefees.Columns.Add(new DataColumn("AmountPer", typeof(string)));
            }
            if (ViewState["phaseAmountData"] != null)
            {
                for (int i = 0; i < dtPhasefees.Rows.Count + 1; i++)
                {
                    dtPhasefees = (DataTable)ViewState["phaseAmountData"];
                    if (dtPhasefees.Rows.Count > 0)
                    {
                        dr = dtPhasefees.NewRow();
                        dr[0] = dtPhasefees.Rows[0][0].ToString();
                        dr[1] = dtPhasefees.Rows[0][1].ToString();
                        dr[2] = dtPhasefees.Rows[0][2].ToString();
                    }
                }
                dr = dtPhasefees.NewRow();
                dr[0] = phasetypename;
                dr[1] = phasedesc;
                if (phasetypename == "Full & Final Instalment")
                {
                    dr[2] = "100";
                }
                else
                {
                    dr[2] = "";
                }


                dtPhasefees.Rows.Add(dr);
            }
            else
            {
                dr = dtPhasefees.NewRow();
                dr[0] = phasetypename;
                dr[1] = phasedesc;
                if (phasetypename == "Full & Final Instalment")
                {
                    dr[2] = "100";
                }
                else if (phasetypename == "100% Against PI")
                {
                    dr[2] = "100";
                }
                else
                {
                    dr[2] = "";
                }
                dtPhasefees.Rows.Add(dr);

            }
            if (ViewState["phaseAmountData"] != null)
            {
                GridView3.DataSource = (DataTable)ViewState["phaseAmountData"];
                GridView3.DataBind();
            }
            else
            {
                GridView3.DataSource = dtPhasefees;
                GridView3.DataBind();
            }
            ViewState["phaseAmountData"] = dtPhasefees;
        }


        protected void AmountPer_TextChanged(object sender, EventArgs e)
        {

            //Label2.Text = "Sumanta";
            amountCalculation();
        }

        public void amountCalculation()
        {
            double total = 0;
            foreach (GridViewRow gvr in GridView3.Rows)
            {
                string name = gvr.Cells[0].Text;

                string PaymentPhase = ((Label)gvr.Cells[0].FindControl("PaymentPhase")).Text;

                if (PaymentPhase != "Full & Final Instalment")
                {
                    TextBox tb = (TextBox)gvr.Cells[1].FindControl("AmountPer");
                    double sum;
                    if (double.TryParse(tb.Text.Trim(), out sum))
                    {
                        total += sum;
                    }
                }
                else
                {
                    double fulfinal = Convert.ToDouble(100);
                    double netamount = fulfinal - total;

                    TextBox tb = (TextBox)gvr.Cells[1].FindControl("AmountPer");
                    tb.Text = netamount.ToString();

                }
            }
        }

        private void Binddata1(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand com1 = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataAdapter da = new SqlDataAdapter(com1);
            SqlDataReader dr = com1.ExecuteReader();

            if (dr.Read())
            {
                DataTable dt = DbCL.GetDataTable(cmdstring);
                first_datatable = dt;
                if (Label1.Text == "1")
                {
                    newgrid1();
                }
                else
                {
                    newgrid();
                }
                Label1.Text = (Convert.ToInt32(Label1.Text) + 1).ToString();
            }
            DbCL.Conn.Close();

        }

        private void newgrid1()
        {
            DataTable dt = first_datatable;

            DataRow dr = null;
            DataColumn Ser_pro_code = new DataColumn("Ser_pro_code", typeof(string));
            Dt.Columns.Add(Ser_pro_code);

            DataColumn Ser_pro_Name = new DataColumn("Ser_pro_Name", typeof(string));
            Dt.Columns.Add(Ser_pro_Name);

            DataColumn Sale_rate = new DataColumn("Sale_rate", typeof(string));
            Dt.Columns.Add(Sale_rate);
            DataColumn service_Tax_Rate = new DataColumn("service_Tax_Rate", typeof(string));
            Dt.Columns.Add(service_Tax_Rate);

            for (int i = 0; i <= dt.Rows.Count - 1; i++)
            {
                string Ser_pro_code1 = (String)first_datatable.Rows[i][0];
                string Ser_pro_Name1 = (String)first_datatable.Rows[i][1];
                string Sale_rate1 = (String)first_datatable.Rows[i][2];
                string service_Tax_Rate1 = (String)first_datatable.Rows[i][3];

                dr = Dt.NewRow();
                dr["Ser_pro_code"] = Ser_pro_code1.ToString();
                dr["Ser_pro_Name"] = Ser_pro_Name1.ToString();
                dr["Sale_rate"] = Sale_rate1.ToString();
                dr["service_Tax_Rate"] = service_Tax_Rate1.ToString();
                Dt.Rows.Add(dr);

            }
        }

        private void newgrid()
        {
            DataTable dt = first_datatable;
            DataRow dr = null;
            for (int i = 0; i <= dt.Rows.Count - 1; i++)
            {
                string Ser_pro_code1 = (String)first_datatable.Rows[i][0];
                string Ser_pro_Name1 = (String)first_datatable.Rows[i][1];
                string Sale_rate1 = (String)first_datatable.Rows[i][2];
                string service_Tax_Rate1 = (String)first_datatable.Rows[i][3];
                dr = Dt.NewRow();
                dr["Ser_pro_code"] = Ser_pro_code1.ToString();
                dr["Ser_pro_Name"] = Ser_pro_Name1.ToString();
                dr["Sale_rate"] = Sale_rate1.ToString();
                dr["service_Tax_Rate"] = service_Tax_Rate1.ToString();
                Dt.Rows.Add(dr);
            }
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            Bindquotationno();

            String CGSTSGSTSTATUS = "";
            String IGSTSTATUS = "";

            if (RadioButtonGst.SelectedIndex == 0)
            {
                CGSTSGSTSTATUS = "YES";
            }
            else
            {
                IGSTSTATUS = "YES";
            }

            //if (CHKCGSTSGST.Checked)
            //{
            //    CGSTSGSTSTATUS = "YES";
            //}
            //if (CHKIGST.Checked)
            //{
            //    IGSTSTATUS = "YES";
            //}


            int j = idreturn();
            j = j + 1;
            int i = 0;

            DataTable dt1;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            //dt1 = (DataTable)ViewState["dt"];
            dt1 = (DataTable)ViewState["PhaseProductData"];
            if (dt1 != null)
            {
                int h = 0;
                for (i = 0; i <= dt1.Rows.Count - 1; i++)
                {

                    CheckBox chk = (CheckBox)(gd_Service_Product.Rows[i].FindControl("chk"));
                    if (chk.Checked == true)
                    {
                        SqlTransaction trans = null;
                        SqlConnection conn = null;
                        //SqlCommand cmd = null;
                        try
                        {
                            h = h + 1;
                            string cnnString = System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ToString();

                            using (conn = new SqlConnection(cnnString))
                            {
                                conn.Open();
                                using (trans = conn.BeginTransaction())
                                using (SqlCommand cmd = new SqlCommand { CommandType = CommandType.Text, Connection = conn, Transaction = trans })
                                {
                                    try
                                    {
                                        // Retrieve values from GridView with null checks  ProductID
                                        string ProductId = ((Label)gd_Service_Product.Rows[i].FindControl("ProductID"))?.Text?.Trim() ?? ""; //Product ID
                                        string Product_code = ((Label)gd_Service_Product.Rows[i].FindControl("Product_code"))?.Text?.Trim() ?? ""; //HSN Code
                                        string ProductName = ((Label)gd_Service_Product.Rows[i].FindControl("ProductName"))?.Text?.Trim() ?? "";
                                        string Brand = ((Label)gd_Service_Product.Rows[i].FindControl("Brand"))?.Text?.Trim() ?? "";
                                        string ProductOrServiceCat = ((Label)gd_Service_Product.Rows[i].FindControl("ProductOrServiceCat"))?.Text?.Trim() ?? "";
                                        string Type = ((Label)gd_Service_Product.Rows[i].FindControl("Type"))?.Text?.Trim() ?? "";
                                        string Unit = ((Label)gd_Service_Product.Rows[i].FindControl("Unit"))?.Text?.Trim() ?? "";
                                        string InvStatus = "No";

                                        // Optional Fields
                                        string ItemNo = ((TextBox)gd_Service_Product.Rows[i].FindControl("ItemNo"))?.Text?.Trim() ?? "";
                                        string MaterialNo = ((TextBox)gd_Service_Product.Rows[i].FindControl("MaterialNo"))?.Text?.Trim() ?? "";
                                        string PackSize = ((TextBox)gd_Service_Product.Rows[i].FindControl("PackSize"))?.Text?.Trim() ?? "";
                                        string ItemRemarks = ((TextBox)gd_Service_Product.Rows[i].FindControl("ItemRemarks"))?.Text?.Trim() ?? "";

                                        // Validate and convert numeric fields
                                        decimal Quantity = .0m;
                                        if (!decimal.TryParse(((TextBox)gd_Service_Product.Rows[i].FindControl("Quantity"))?.Text?.Trim(), out Quantity) || Quantity <= 0)
                                            throw new ArgumentException("Invalid Quantity");
                                        decimal Sail_Rate = .0m;
                                        if (!decimal.TryParse(((TextBox)gd_Service_Product.Rows[i].FindControl("Sail_Rate"))?.Text?.Trim(), out Sail_Rate) || Sail_Rate < 0)
                                            throw new ArgumentException("Invalid Sail Rate");
                                        decimal Tax_Rate = .0m;
                                        if (!decimal.TryParse(((Label)gd_Service_Product.Rows[i].FindControl("Tax_Rate"))?.Text?.Trim(), out Tax_Rate) || Tax_Rate < 0)
                                            throw new ArgumentException("Invalid Tax Rate");
                                        decimal Discount_Rate = .0m;
                                        if (!decimal.TryParse(((TextBox)gd_Service_Product.Rows[i].FindControl("Discount_Rate"))?.Text?.Trim(), out Discount_Rate))
                                            Discount_Rate = 0; // Default to 0 if empty

                                        // Calculate discounted rate
                                        decimal discounted_rate = Sail_Rate - (Sail_Rate * Discount_Rate / 100);
                                        decimal taxMultiplier = (Tax_Rate + 100) / 100;

                                        // Calculate service tax and total rates
                                        decimal Total_sail_rate = taxMultiplier * discounted_rate;
                                        decimal Total_sail_rate1 = Total_sail_rate * Quantity;
                                        decimal Total_sail_rate2 = discounted_rate * Quantity;
                                        decimal Service_tax = (Tax_Rate * Quantity * discounted_rate) / 100;

                                        // Update subtotal amounts
                                        new_sub_total += Total_sail_rate2;
                                        new_total_Service += Service_tax;
                                        new_Gross_amount = Math.Round(new_Gross_amount + Total_sail_rate1, 2);

                                        // Insert into database using parameterized query
                                        cmd.CommandText = @"
                                        INSERT INTO tbl_Quotaion_details 
                                        (Sl_no, Quotation_no, Product_id, Product_Code, Product_name, Quantity, sail_rate, Service_tax_rate, 
                                         Total_sail_rate, Total_sail_rate1, Total_sail_rate2, specification, InvStatus, Type, Unit, 
                                         ProductOrServiceCat, discount_rate, new_sailrate, ItemRemarks, ItemNo, MaterialNo, PackSize) 
                                        VALUES 
                                        (@Sl_no, @Quotation_no, @Product_id, @Product_Code, @Product_name, @Quantity, @sail_rate, @Service_tax_rate, 
                                         @Total_sail_rate, @Total_sail_rate1, @Total_sail_rate2, @specification, @InvStatus, @Type, @Unit, 
                                         @ProductOrServiceCat, @discount_rate, @new_sailrate, @ItemRemarks, @ItemNo, @MaterialNo, @PackSize)";

                                        // Add parameters
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("@Sl_no", h);
                                        cmd.Parameters.AddWithValue("@Quotation_no", lblqno.Text);
                                        cmd.Parameters.AddWithValue("@Product_Code", ProductId);
                                        cmd.Parameters.AddWithValue("@Product_id", Product_code);
                                        cmd.Parameters.AddWithValue("@Product_name", ProductName);
                                        cmd.Parameters.AddWithValue("@Quantity", Quantity);
                                        cmd.Parameters.AddWithValue("@sail_rate", Sail_Rate);
                                        cmd.Parameters.AddWithValue("@Service_tax_rate", Tax_Rate);
                                        cmd.Parameters.AddWithValue("@Total_sail_rate", Total_sail_rate);
                                        cmd.Parameters.AddWithValue("@Total_sail_rate1", Total_sail_rate1);
                                        cmd.Parameters.AddWithValue("@Total_sail_rate2", Total_sail_rate2);
                                        cmd.Parameters.AddWithValue("@specification", Brand);
                                        cmd.Parameters.AddWithValue("@InvStatus", InvStatus);
                                        cmd.Parameters.AddWithValue("@Type", Type);
                                        cmd.Parameters.AddWithValue("@Unit", Unit);
                                        cmd.Parameters.AddWithValue("@ProductOrServiceCat", ProductOrServiceCat);
                                        cmd.Parameters.AddWithValue("@discount_rate", Discount_Rate);
                                        cmd.Parameters.AddWithValue("@new_sailrate", discounted_rate);
                                        cmd.Parameters.AddWithValue("@ItemRemarks", ItemRemarks);
                                        cmd.Parameters.AddWithValue("@ItemNo", ItemNo);
                                        cmd.Parameters.AddWithValue("@MaterialNo", MaterialNo);
                                        cmd.Parameters.AddWithValue("@PackSize", PackSize);

                                        // Execute query
                                        cmd.ExecuteNonQuery();
                                        trans.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        trans.Rollback();
                                        throw new Exception("Database transaction failed: " + ex.Message);
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            i = 1;

                            if (trans != null)
                            {
                                try
                                {
                                    trans.Rollback();  // Rollback the transaction safely
                                }
                                catch (Exception rollbackEx)
                                {
                                    // Log rollback error if necessary
                                    Console.WriteLine("Rollback failed: " + rollbackEx.Message);
                                }
                            }

                            throw; // Preserves the original exception stack trace
                        }
                        finally
                        {
                            if (conn != null)
                            {
                                try
                                {
                                    conn.Close();   // Close connection safely
                                    conn.Dispose(); // Free up resources
                                }
                                catch (Exception closeEx)
                                {
                                    // Log any connection close errors
                                    Console.WriteLine("Error while closing connection: " + closeEx.Message);
                                }
                            }
                        }

                    }

                }
            }

            try
            {
                int validDays;
                if (!int.TryParse(txt_valdays.Text?.Trim(), out validDays))
                {
                    throw new ArgumentException("Invalid value for Validity Days.");
                }

                string deliveryTenure = DDL_DeliveryTerms.SelectedValue == "4" ? txt_deltrms.Text?.Trim() : DDL_DeliveryTerms.SelectedItem.Text;
                string packageForwarding = DDL_pkgfrwd.SelectedValue == "3" ? txt_pkgfrwd.Text?.Trim() : DDL_pkgfrwd.SelectedItem.Text;
                string remarks = txt_remarks.Text?.Trim();
                string itemview = DDL_ItemViewType.SelectedItem.Text?.Trim();
                string referenceOption = rbYes.Checked ? "Yes" : "No";
                // Set default values if "No" is selected
                string referenceName = referenceOption == "No" ? "N/A" : txt_clientrefname.Text?.Trim();
                string referenceId = referenceOption == "No" ? "N/A" : txt_clientrefid.Text?.Trim();
                string referenceDate = referenceOption == "No" ? "1900-01-01" : txt_clientrefdate.Text?.Trim();

                total_sail_rate_details = new_Gross_amount;
                total_sail_rate_details = Math.Round(total_sail_rate_details, 2);
                new_total_Service = Math.Round(new_total_Service, 2);
                DbCL.Conn.Close();

                using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ToString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO tbl_Quotation " +
                        "(Quotation_no, Quotation_date, Client_Id, Gross, Service_tax, Net_amount, Status1, Status2, Sl_no, status3, service_tax1, sub_total, cgstOrsgst, igst, PlaceofSupply, PaymentStatus, ReferenceData, ReferenceName, ReferenceId, ReferenceDate, ValidityDays, DeliveryTenure, PackingCharges, Remarks, DetailedView) " +
                        "VALUES (@Quotation_no, @Quotation_date, @Client_Id, @Gross, @Service_tax, @Net_amount, 'No', 'No', @Sl_no, 'No', @service_tax1, @sub_total, @cgstOrsgst, @igst, @PlaceofSupply, 'No', @ReferenceData, @ReferenceName, @ReferenceId, @ReferenceDate, @ValidityDays, @DeliveryTenure, @PackingCharges, @Remarks, @DetailedView)", conn))
                    {
                        cmd.Parameters.AddWithValue("@Quotation_no", lblqno.Text?.Trim());
                        cmd.Parameters.AddWithValue("@Quotation_date", txtquotationDate.Text?.Trim());
                        cmd.Parameters.AddWithValue("@Client_Id", lblclientID.Text?.Trim());
                        cmd.Parameters.AddWithValue("@Gross", new_Gross_amount);
                        cmd.Parameters.AddWithValue("@Service_tax", Service_tax);
                        cmd.Parameters.AddWithValue("@Net_amount", total_sail_rate_details);
                        cmd.Parameters.AddWithValue("@Sl_no", j);
                        cmd.Parameters.AddWithValue("@service_tax1", new_total_Service);
                        cmd.Parameters.AddWithValue("@sub_total", new_sub_total);
                        cmd.Parameters.AddWithValue("@cgstOrsgst", CGSTSGSTSTATUS);
                        cmd.Parameters.AddWithValue("@igst", IGSTSTATUS);
                        cmd.Parameters.AddWithValue("@PlaceofSupply", ddlPlaceOfSupply.Text?.Trim());
                        cmd.Parameters.AddWithValue("@ReferenceData", referenceOption);
                        //cmd.Parameters.AddWithValue("@ReferenceName", txt_clientrefname.Text?.Trim());
                        //cmd.Parameters.AddWithValue("@ReferenceId", txt_clientrefid.Text?.Trim());
                        //cmd.Parameters.AddWithValue("@ReferenceDate", txt_clientrefdate.Text?.Trim());
                        cmd.Parameters.AddWithValue("@ReferenceName", referenceName);
                        cmd.Parameters.AddWithValue("@ReferenceId", referenceId);
                        cmd.Parameters.AddWithValue("@ReferenceDate", referenceDate);
                        cmd.Parameters.AddWithValue("@ValidityDays", validDays);
                        cmd.Parameters.AddWithValue("@DeliveryTenure", deliveryTenure);
                        cmd.Parameters.AddWithValue("@PackingCharges", packageForwarding);
                        cmd.Parameters.AddWithValue("@Remarks", remarks);
                        cmd.Parameters.AddWithValue("@DetailedView", itemview);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }


            //int validDays = int.Parse(txt_valdays.Text.Trim());
            //string deliveryTenure = DDL_DeliveryTerms.SelectedValue == "4" ? txt_deltrms.Text.Trim() : DDL_DeliveryTerms.SelectedItem.Text;
            //string packageForwarding = DDL_pkgfrwd.SelectedValue == "3" ? txt_pkgfrwd.Text.Trim() : DDL_pkgfrwd.SelectedItem.Text;
            //string remarks = txt_remarks.Text.Trim();
            ////Newly Added on 23-02-2025
            //string itemview = DDL_ItemViewType.SelectedItem.Text.ToString();

            //DbCL.Conn.Close();
            //Service_tax = new_Gross_amount % 1;
            //total_sail_rate_details = new_Gross_amount;
            //total_sail_rate_details = Math.Round(total_sail_rate_details, 2);
            //new_total_Service = Math.Round(new_total_Service, 2);

            //DbCL.executeRdr("insert into tbl_Quotation(Quotation_no,Quotation_date,Client_Id,Gross,Service_tax,Net_amount,Status1,Status2,Sl_no,status3,service_tax1,sub_total,cgstOrsgst,igst,PlaceofSupply,PaymentStatus,ReferenceName,ReferenceId,ReferenceDate,ValidityDays,DeliveryTenure,PackingCharges, Remarks, DetailedView)values('" + lblqno.Text + "','" + txtquotationDate.Text + "','" + lblclientID.Text + "','" + new_Gross_amount + "','" + Service_tax + "','" + total_sail_rate_details + "','No','No','" + j.ToString() + "','No','" + new_total_Service + "','" + new_sub_total + "','" + CGSTSGSTSTATUS + "','" + IGSTSTATUS + "','" + ddlPlaceOfSupply.Text + "','No','"+txt_clientrefname.Text.ToString()+ "','" + txt_clientrefid.Text.ToString() + "','" + txt_clientrefdate.Text.ToString() + "','" + validDays.ToString() + "','" + deliveryTenure.ToString() + "','" + packageForwarding.ToString() + "','" + remarks.ToString() + "','" + itemview.ToString() + "')");


            string qutno = lblqno.Text;
            insertPaymentPhase(qutno);
            insertprimaryService(qutno);

            //insertCorRegFacAddress(qutno);

            lblOk.Text = "Data Save Successfully.....";
            PanelOK.Visible = true;
            Button3.Visible = false;


        }

        private void insertPaymentPhase(string qutno)
        {
            foreach (GridViewRow gvr in GridView3.Rows)
            {
                string phasetype = ((Label)gvr.Cells[0].FindControl("PaymentPhase")).Text;
                string phasedesc = ((TextBox)gvr.Cells[1].FindControl("PhaseDesc")).Text;
                string amo = ((TextBox)gvr.Cells[2].FindControl("AmountPer")).Text;

                string query = "insert into tbl_QutPaymentPhase(qut_no,phase_type,PhaseDesc,amountper) values (@qut_no,@phase_type,@PhaseDesc,@amountper)";
                SqlParameter[] pram = {
                new SqlParameter("@qut_no",qutno),
                new SqlParameter("@phase_type",phasetype),
                new SqlParameter("@PhaseDesc",phasedesc),
                new SqlParameter("@amountper",amo)
                };

                DbCL.SPExecDB(query, pram);
            }
        }

        //private void insertCorRegFacAddress(string qutno)
        //{
        //    int selectedSite = 0;

        //    string listsite_details = null;
        //    int slno22 = 1;
        //    for (int i = 0; i < FactoryAddress.Items.Count; i++)
        //    {
        //        if (FactoryAddress.Items[i].Selected)
        //        {
        //            selectedSite = selectedSite + 1;
        //            listsite_details = FactoryAddress.Items[i].Text;

        //            string query = "insert into tbl_QutSiteAddress(qut_no,SiteAddress) values (@qut_no,@SiteAddress)";
        //            SqlParameter[] pram = {
        //                 new SqlParameter("@qut_no",qutno),
        //                 new SqlParameter("@SiteAddress",listsite_details)
        //            };

        //            DbCL.SPExecDB(query, pram);
        //            slno22 = slno22 + 1;
        //        }
        //    }
        //}
        private void insertprimaryService(string qutno)
        {
            //int selectedService = 0;

            //string listsite_details = null;
            //int slno22 = 1;
            //for (int i = 0; i < listOfPrimaryService.Items.Count; i++)
            //{
            //    if (listOfPrimaryService.Items[i].Selected)
            //    {
            //        selectedService = selectedService + 1;
            //        listsite_details = listOfPrimaryService.Items[i].Text;

            //        string query = "insert into tbl_QutPrimaryService(qut_no,PrimaryService) values (@qut_no,@PrimaryService)";
            //        SqlParameter[] pram = {
            //             new SqlParameter("@qut_no",qutno),
            //             new SqlParameter("@PrimaryService",listsite_details)
            //        };
            //        DbCL.SPExecDB(query, pram);
            //        slno22 = slno22 + 1;

            //        insertPrimaryServiceDesc(qutno, listsite_details);
            //    }
            //}

            string PrimaryService = "";
            int i = 0;
            foreach (GridViewRow gvr in gridps.Rows)
            {
                string ProductCatagory = ((Label)gvr.Cells[0].FindControl("ProductCatagory")).Text;

                string query = "insert into tbl_QutPrimaryService(qut_no,PrimaryService) values (@qut_no,@PrimaryService)";
                SqlParameter[] pram = {
                             new SqlParameter("@qut_no",qutno),
                             new SqlParameter("@PrimaryService",ProductCatagory)
                        };
                DbCL.SPExecDB(query, pram);

                insertPrimaryServiceDesc(qutno, ProductCatagory);

                ProductCatagory = "“" + ProductCatagory + "”";
                if (i == 0)
                {
                    PrimaryService = ProductCatagory;
                }
                else if (i == 1)
                {
                    PrimaryService = PrimaryService + " and " + ProductCatagory;
                }
                else
                {
                    PrimaryService = PrimaryService + " , " + ProductCatagory;
                }

                i++;
            }

            insertServiceTogether(qutno, PrimaryService);


        }

        private void insertServiceTogether(string qutno, string primaryService)
        {
            string query = "insert into tbl_QuoPriSerTogather (qutno,PServiceName) values (@qutno,@PServiceName)";
            SqlParameter[] pram = {
                          new SqlParameter("@PServiceName",primaryService),
                          new SqlParameter("@qutno",qutno),
                    };
            DbCL.SPExecDB(query, pram);
        }

        private void insertPrimaryServiceDesc(string qutno, string ProductCatagory)
        {
            string query = "select PrimaryServiceTerms from tbl_PrimaryServiceTerms where PrimaryService=@PrimaryService";
            SqlParameter[] pram = {
                new SqlParameter("@PrimaryService",ProductCatagory),
            };
            dtSTerm = DbCL.SPreturn_dt(query, pram);
            if (dtSTerm.Rows.Count > 0)
            {
                for (int i = 0; i < dtSTerm.Rows.Count; i++)
                {
                    string pSerTer = dtSTerm.Rows[i]["PrimaryServiceTerms"].ToString();

                    string query1 = "insert into tbl_QuoPserTerm (qutno,PServiceName,PSerTer) values (@qutno,@PServiceName,@PSerTer)";
                    SqlParameter[] pram1 = {
                          new SqlParameter("@PServiceName",ProductCatagory),
                          new SqlParameter("@PSerTer",pSerTer),
                          new SqlParameter("@qutno",qutno),
                    };
                    DbCL.SPExecDB(query1, pram1);
                }
            }
        }



        private void insertvatamount(decimal service, string service_Tax_Rate)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string service1 = service.ToString();
            string cmdstring = "select * from tbl_quotation_vat where Quotation_no='" + lblqno.Text + "' and Vat_rate='" + service_Tax_Rate.ToString() + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                DbCL.executeRdr("update tbl_quotation_vat set Vat_amount=(cast(Vat_amount as real)+'" + service1.ToString() + "') where Quotation_no='" + lblqno.Text + "' and Vat_rate='" + service_Tax_Rate.ToString() + "'");
            }
            else
            {
                DbCL.executeRdr("insert into tbl_quotation_vat(Quotation_no,Vat_rate,Vat_amount)values('" + lblqno.Text + "','" + service_Tax_Rate.ToString() + "','" + service1.ToString() + "')");
            }
            DbCL.Conn.Close();

        }

        protected void GridView3_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int index = Convert.ToInt32(e.RowIndex);
            DataTable dtphs = ViewState["phaseAmountData"] as DataTable;
            dtphs.Rows[index].Delete();
            if (dtphs.Rows.Count > 0)
            {
                ViewState["phaseAmountData"] = dtphs;
            }
            else
            {
                ViewState["phaseAmountData"] = null;
                dtphs = null;
            }

            GridView3.DataSource = (DataTable)ViewState["phaseAmountData"];
            GridView3.DataBind();
        }

        protected void btnAddProduct_Click(object sender, EventArgs e)
        {
            //Panel4.Visible = true;
            //Panel3.Visible = false;
            gridProdWithCat.Visible = true;
            if (ViewState["dtprocat"] != null)
            {
                DataTable dtpro = new DataTable();
                dtpro = ViewState["dtprocat"] as DataTable;

                //string Ser_pro_code = "";
                //string Ser_pro_Name = "";
                //string specification = "";

                //string Sale_rate = "";
                //string service_Tax_Rate = "";


                string Product_code = ""; //HSN Code
                string ProductId = ""; // Product ID
                string ProductName = "";
                string Brandspecification = "";
                string Type = "";
                string Sail_Rate = "";
                string Tax_Rate = "";
                string Unit = "";
                string Quantity = "";
                string ProductOrServiceCat = "";


                for (int i = 0; i < dtpro.Rows.Count; i++)
                {
                    CheckBox chkdtp = (CheckBox)(gridProdWithCat.Rows[i].FindControl("chkdtp"));
                    if (chkdtp.Checked == true)
                    {
                        ProductId = ((Label)gridProdWithCat.Rows[i].FindControl("ProductID")).Text;
                        Product_code = ((Label)gridProdWithCat.Rows[i].FindControl("Product_code")).Text;
                        ProductName = ((Label)gridProdWithCat.Rows[i].FindControl("ProductName")).Text;
                        //Brandspecification = ((TextBox)gridProdWithCat.Rows[i].FindControl("Brand")).Text;
                        Brandspecification = ((Label)gridProdWithCat.Rows[i].FindControl("Brand")).Text;
                        //Quantity = ((TextBox)gridProdWithCat.Rows[i].FindControl("Quantity")).Text;
                        Quantity = ((Label)gridProdWithCat.Rows[i].FindControl("Quantity")).Text;
                        //Sail_Rate = ((TextBox)gridProdWithCat.Rows[i].FindControl("Sail_Rate")).Text;
                        Sail_Rate = ((Label)gridProdWithCat.Rows[i].FindControl("Sail_Rate")).Text;
                        Tax_Rate = ((Label)gridProdWithCat.Rows[i].FindControl("Tax_Rate")).Text;
                        Type = ((Label)gridProdWithCat.Rows[i].FindControl("Type")).Text;
                        Unit = ((Label)gridProdWithCat.Rows[i].FindControl("Unit")).Text;
                        ProductOrServiceCat = ((Label)gridProdWithCat.Rows[i].FindControl("ProductOrServiceCat")).Text;

                        if (ViewState["PhaseProductData"] != null)
                        {
                            dtPCat = (DataTable)ViewState["PhaseProductData"];
                            int count = dtPCat.Rows.Count + 1;

                            SearchProductCatwise(count, ProductId, Product_code, ProductName, Brandspecification, Quantity, Sail_Rate, Tax_Rate, Type, Unit, ProductOrServiceCat);

                        }
                        else
                        {
                            SearchProductCatwise(1, ProductId, Product_code, ProductName, Brandspecification, Quantity, Sail_Rate, Tax_Rate, Type, Unit, ProductOrServiceCat);
                        }
                    }
                }


                string pservice = cmbproduct_service.Text;
                if (ViewState["pService"] != null)
                {
                    dtPservice = (DataTable)ViewState["pService"];
                    int count1 = dtPservice.Rows.Count + 1;
                    //TakePservice(count1, pservice);

                    string service = "";
                    string status = "NO";
                    for (int j = 0; j < dtPservice.Rows.Count; j++)
                    {
                        service = dtPservice.Rows[j]["ProductCatagory"].ToString();
                        if (service == pservice)
                        {
                            status = "YES";
                        }
                    }
                    if (status == "NO")
                    {
                        TakePservice(count1, pservice);
                    }
                }
                else
                {
                    TakePservice(1, pservice);
                }
            }


            //private void factoryaddress(string id)
            //{
            //    DbCL.Sqlconnection();
            //    DbCL.ConnectDb();
            //    string cmdstring = "select Factory_name,Address1,Address2,city,State,pin from tbl_Factory where Client_id="+id.ToString()+"";
            //    SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            //    SqlDataAdapter da = new SqlDataAdapter(cmd);
            //    SqlDataReader DR1 = cmd.ExecuteReader();
            //    while (DR1.Read())
            //    {
            //        //cmbfactoryaddress.Items.Add(DR1["factory_name"].ToString());
            //        string fac = DR1["Factory_name"].ToString();
            //        string add1 = DR1["Address1"].ToString();
            //        string add2 = DR1["Address2"].ToString();
            //        string city = DR1["city"].ToString();
            //        string State = DR1["State"].ToString();
            //        string pin = DR1["pin"].ToString();
            //        string d = a + ", " + b + " - " + c;
            //        listsite.Items.Add(d);
            //    }
            //    DbCL.Conn.Close();
            //}


            // added on 30-Jan-2025, To hide the Products Grid after selection of Products for Quotes Creation
            gridProdWithCat.Visible = false;
            btnAddProduct.Enabled = false;
        }

        private void TakePservice(int count1, string pservice)
        {
            DataRow dr;
            if (count1 == 1)
            {
                dtPCat1.Columns.Add(new DataColumn("ProductCatagory", typeof(string)));
            }
            if (ViewState["pService"] != null)
            {
                for (int i = 0; i < dtPCat1.Rows.Count + 1; i++)
                {
                    dtPCat1 = (DataTable)ViewState["pService"];
                    if (dtPCat1.Rows.Count > 0)
                    {
                        dr = dtPCat1.NewRow();
                        dr[0] = dtPCat1.Rows[0][0].ToString();

                    }
                }
                dr = dtPCat1.NewRow();
                dr[0] = pservice;
                dtPCat1.Rows.Add(dr);
            }
            else
            {
                dr = dtPCat1.NewRow();
                dr[0] = pservice;
                dtPCat1.Rows.Add(dr);

            }
            if (ViewState["pService"] != null)
            {
                gridps.DataSource = (DataTable)ViewState["pService"];
                gridps.DataBind();
            }
            else
            {
                gridps.DataSource = dtPCat1;
                gridps.DataBind();
            }
            ViewState["pService"] = dtPCat1;
        }

        private void SearchProductCatwise(int count, string Product_code, string ProductId, string ProductName, string Brandspecification, string Quantity, string Sail_Rate, string Tax_Rate, string Type, string Unit, string ProductOrServiceCat)
        {
            DataRow dr;
            if (count == 1)
            {
                dtPCat.Columns.Add(new DataColumn("ProductId", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Product_code", typeof(string))); 
                dtPCat.Columns.Add(new DataColumn("ProductName", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Sail_Rate", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Tax_Rate", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Quantity", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Brand", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Type", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Unit", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("ProductOrServiceCat", typeof(string)));

            }
            if (ViewState["PhaseProductData"] != null)
            {
                for (int i = 0; i < dtPCat.Rows.Count + 1; i++)
                {
                    dtPCat = (DataTable)ViewState["PhaseProductData"];
                    if (dtPCat.Rows.Count > 0)
                    {
                        dr = dtPCat.NewRow();
                        dr[0] = dtPCat.Rows[0][0].ToString();
                        dr[1] = dtPCat.Rows[0][1].ToString();
                        dr[2] = dtPCat.Rows[0][2].ToString();
                        dr[3] = dtPCat.Rows[0][3].ToString();
                        dr[4] = dtPCat.Rows[0][4].ToString();
                        dr[5] = dtPCat.Rows[0][5].ToString();
                        dr[6] = dtPCat.Rows[0][6].ToString();
                        dr[7] = dtPCat.Rows[0][7].ToString();
                        dr[8] = dtPCat.Rows[0][8].ToString();
                        dr[9] = dtPCat.Rows[0][8].ToString();
                    }
                }
                dr = dtPCat.NewRow();
                dr[0] = Product_code;
                dr[1] = ProductId;
                dr[2] = ProductName;
                dr[3] = Sail_Rate;
                dr[4] = Tax_Rate;
                dr[5] = Quantity;
                dr[6] = Brandspecification;
                dr[7] = Type;
                dr[8] = Unit;
                dr[9] = ProductOrServiceCat;

                dtPCat.Rows.Add(dr);
            }
            else
            {
                dr = dtPCat.NewRow();
                dr[0] = Product_code;
                dr[1] = ProductId;
                dr[2] = ProductName;
                dr[3] = Sail_Rate;
                dr[4] = Tax_Rate;
                dr[5] = Quantity;
                dr[6] = Brandspecification;
                dr[7] = Type;
                dr[8] = Unit;
                dr[9] = ProductOrServiceCat;
                dtPCat.Rows.Add(dr);

            }
            if (ViewState["PhaseProductData"] != null)
            {
                gd_Service_Product.DataSource = (DataTable)ViewState["PhaseProductData"];
                gd_Service_Product.DataBind();
            }
            else
            {
                gd_Service_Product.DataSource = dtPCat;
                gd_Service_Product.DataBind();
            }
            ViewState["PhaseProductData"] = dtPCat;
        }
    }
}