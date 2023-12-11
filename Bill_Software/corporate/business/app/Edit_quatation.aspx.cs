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
    public partial class WebForm65 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        public DataTable first_datatable;
        public static DataTable Dt = new DataTable("Table");
        public static decimal Gross_amount = 0;
        public static decimal Service_tax = 0;
        public static decimal total_sail_rate_details = 0;
        public static decimal total_Service = 0;
        public static decimal sub_total = 0;

        DataTable dtproductWithCat = new DataTable();
        DataTable dtPCat = new DataTable();
        DataTable dtpro = new DataTable();
        DataTable dtProInvPay = new DataTable();
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
                Gross_amount = 0;
                Service_tax = 0;
                total_sail_rate_details = 0;
                total_Service = 0;
                sub_total = 0;
            }
        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                BuindCompanyId();
                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Client_Id='" + lblclientId.Text + "' and tbl_Quotation.Status2='No' order by cast(tbl_Quotation.Quotation_date as datetime) desc";
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.Client_Id='" + lblclientId.Text + "' order by tbl_Quotation.ID desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' and tbl_Quotation.Status2='No' order by cast(tbl_Quotation.Quotation_date as datetime) desc";
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by tbl_Quotation.ID desc";
                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Client_Id='" + lblclientId.Text + "' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' and tbl_Quotation.Status2='No' order by cast(tbl_Quotation.Quotation_date as datetime) desc";
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.Client_Id='" + lblclientId.Text + "' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by tbl_Quotation.ID desc";
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
            Response.Redirect("~/corporate/business/app/Edit_quatation.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Quotation_no = Convert.ToString(e.CommandArgument);
            if (e.CommandName == "Select")
            {
                lblqno.Text = Quotation_no.ToString();
                //string status = searchDate(Quotation_no);

                Panel1.Visible = true;
                string cmdstring = "select Product_id as Product_code,Product_name as ProductName,Type, sail_rate as Sail_Rate,Service_tax_rate as Tax_Rate,Unit,specification as Brand,Quantity,ProductOrServiceCat from tbl_Quotaion_details where Quotation_no=@Quotation_no";
                SqlParameter[] pram = { new SqlParameter("@Quotation_no", Quotation_no) };
                dtpro = DbCL.SPreturn_dt(cmdstring,pram);
                //Binddata1(cmdstring, status, Quotation_no);
                if (dtpro.Rows.Count>0)
                {
                    string Product_code = "";
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
                            Product_code = dtpro.Rows[i]["Product_code"].ToString();
                            ProductName = dtpro.Rows[i]["ProductName"].ToString();
                            Brandspecification = dtpro.Rows[i]["Brand"].ToString();
                            Quantity = dtpro.Rows[i]["Quantity"].ToString();
                            Sail_Rate = dtpro.Rows[i]["Sail_Rate"].ToString();
                            Tax_Rate = dtpro.Rows[i]["Tax_Rate"].ToString();
                            Type = dtpro.Rows[i]["Type"].ToString();
                            Unit = dtpro.Rows[i]["Unit"].ToString();

                            ProductOrServiceCat = dtpro.Rows[i]["ProductOrServiceCat"].ToString();

                            if (ViewState["PhaseProductData"] != null)
                            {
                                dtPCat = (DataTable)ViewState["PhaseProductData"];
                                int count = dtPCat.Rows.Count + 1;

                                SearchProductCatwise(count, Product_code, ProductName, Brandspecification, Quantity, Sail_Rate, Tax_Rate, Type, Unit, ProductOrServiceCat);

                            }
                            else
                            {
                                SearchProductCatwise(1, Product_code, ProductName, Brandspecification, Quantity, Sail_Rate, Tax_Rate, Type, Unit, ProductOrServiceCat);
                            }
                    }

                }

                Bindcombo();
            }
        }

        private void Bindcombo()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "";

            cmdstring = "select ProductOrServiceCat from tbl_NewparentProduct order by ProductOrServiceCat asc";

            cmbproduct_service.Items.Clear();
            cmbproduct_service.Items.Add("--Select--");
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            while (re.Read())
            {
                cmbproduct_service.Items.Add(re.GetValue(0).ToString());
            }
            DbCL.Conn.Close();
        }

        //private string searchDate(string qno)
        //{
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    string status_value = "";
        //    string Lst = "30-Jun-2017";
        //    string CmdString = "select Quotation_no from tbl_Quotation where Quotation_no='" + qno + "' and (CONVERT(datetime, Quotation_date, 103) > CONVERT(datetime, '" + Lst + "', 103))";
        //    SqlCommand cmd = new SqlCommand(CmdString, DbCL.Conn);
        //    SqlDataReader re1 = cmd.ExecuteReader();
        //    if (re1.Read())
        //    {
        //        status_value = "YES";
        //    }
        //    else
        //    {
        //        status_value = "NO";
        //    }
        //    DbCL.Conn.Close();
        //    return status_value;
        //}

        //private string searchDate(string quotation_no)
        //{
        //    string date = "";
        //    string query = "select Quotation_date from tbl_Quotation where Quotation_no=@Quotation_no";
        //    SqlParameter[] pram = {
        //        new SqlParameter("@Quotation_no",quotation_no),
        //    };
        //    SqlDataReader rdr = DbCL.SPReturnRdr(query, pram);
        //    if (rdr.Read())
        //    {
        //        date= rdr["Quotation_date"].ToString();
        //    }
        //    return date;
        //}

        //private void Binddata1(string cmdstring, string status, string Quotation_no)
        //{
        //    string ProOrSer = bindProOrSer(Quotation_no);
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    SqlCommand com1 = new SqlCommand(cmdstring, DbCL.Conn);
        //    SqlDataAdapter da = new SqlDataAdapter(com1);
        //    SqlDataReader dr = com1.ExecuteReader();

        //    while (dr.Read())
        //    {
        //        string Product_code = dr["Product_code"].ToString();
        //        string Product_Name = dr["Product_Name"].ToString();
        //        string Sail_Rate = dr["Sail_Rate"].ToString();
        //        string Tax_Rate = dr["Tax_Rate"].ToString();
        //        string Quantity = dr["Quantity"].ToString(); 
        //        DataTable dt = binddatatable(Product_code, Product_Name, Sail_Rate, Tax_Rate, Quantity);
        //        first_datatable = dt;
        //        if (Label1.Text == "1")
        //        {
        //            newgrid1();
        //            Label2.Text = ProOrSer.ToString();
        //            Bindcombo(ProOrSer, status);
        //        }
        //        else
        //        {
        //            newgrid();
        //        }
        //        Label1.Text = (Convert.ToInt32(Label1.Text) + 1).ToString();

        //    }
        //    DbCL.Conn.Close();
        //}

        //private string bindProOrSer(string quotation_no)
        //{
        //    string a = "";
        //    string query = "select ProSer from tbl_Quotation where Quotation_no=@Quotation_no";
        //    SqlParameter[] pram = {
        //        new SqlParameter("@Quotation_no",quotation_no)
        //    };
        //    SqlDataReader rdrt = DbCL.SPReturnRdr(query, pram);
        //    if (rdrt.Read())
        //    {
        //        a= rdrt["ProSer"].ToString();
        //    }
        //    return a;
        //}

        //private void Bindcombo(string ProOrSer, string status)
        //{
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    string cmdstring = "";

        //    if (status == "YES")
        //    {  
        //        cmdstring = "select ProductOrServiceCat from tbl_NewparentProduct order by ProductOrServiceCat asc";
        //    }

        //    else
        //    {
        //        if (ProOrSer == "Product")
        //        {
        //            cmdstring = "select distinct(Product_Name) from tbl_Product order by Product_Name";
        //        }
        //        else if (ProOrSer == "Service")
        //        {
        //            cmdstring = "select Service_name  from tbl_Service order by Service_name";
        //        }
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

        //private DataTable binddatatable(string Product_code, string Product_Name, string Sail_Rate, string Tax_Rate,string Quantity)
        //{
        //    DataTable dt = new DataTable("Table");
        //    DataRow dr = null;
        //    DataColumn Ser_pro_code = new DataColumn("Ser_pro_code", typeof(string));
        //    dt.Columns.Add(Ser_pro_code);

        //    DataColumn Ser_pro_Name = new DataColumn("Ser_pro_Name", typeof(string));
        //    dt.Columns.Add(Ser_pro_Name);

        //    DataColumn Sale_rate = new DataColumn("Sale_rate", typeof(string));
        //    dt.Columns.Add(Sale_rate);
        //    DataColumn service_Tax_Rate = new DataColumn("service_Tax_Rate", typeof(string));
        //    dt.Columns.Add(service_Tax_Rate);
        //    DataColumn Total_quanty = new DataColumn("Total_quanty", typeof(string));
        //    dt.Columns.Add(Total_quanty);
        //    dr = dt.NewRow();

        //    dr["Ser_pro_code"] = Product_code.ToString();
        //    dr["Ser_pro_Name"] = Product_Name.ToString();
        //    dr["Sale_rate"] = Sail_Rate.ToString();
        //    dr["service_Tax_Rate"] = Tax_Rate.ToString();
        //    dr["Total_quanty"] = Quantity.ToString();
        //    dt.Rows.Add(dr);
        //    return dt;



        //}

        //private void newgrid1()
        //{
        //    DataTable dt;
        //    dt = first_datatable;

        //    DataRow dr = null;
        //    DataColumn Ser_pro_code = new DataColumn("Ser_pro_code", typeof(string));
        //    Dt.Columns.Add(Ser_pro_code);

        //    DataColumn Ser_pro_Name = new DataColumn("Ser_pro_Name", typeof(string));
        //    Dt.Columns.Add(Ser_pro_Name);
        //    DataColumn Vendor_rate = new DataColumn("Vendor_rate", typeof(string));
        //    Dt.Columns.Add(Vendor_rate);
        //    DataColumn Sale_rate = new DataColumn("Sale_rate", typeof(string));
        //    Dt.Columns.Add(Sale_rate);
        //    DataColumn service_Tax_Rate = new DataColumn("service_Tax_Rate", typeof(string));
        //    Dt.Columns.Add(service_Tax_Rate);
        //    DataColumn Total_quanty = new DataColumn("Total_quanty", typeof(string));
        //    Dt.Columns.Add(Total_quanty);

        //    for (int i = 0; i <= dt.Rows.Count - 1; i++)
        //    {
        //        string Ser_pro_code1 = (String)first_datatable.Rows[i][0];
        //        string Ser_pro_Name1 = (String)first_datatable.Rows[i][1]; ;

        //        string Sale_rate1 = (String)first_datatable.Rows[i][2];
        //        string service_Tax_Rate1 = (String)first_datatable.Rows[i][3];
        //        string Total_quanty1 = (String)first_datatable.Rows[i][4];
        //        dr = Dt.NewRow();
        //        dr["Ser_pro_code"] = Ser_pro_code1.ToString();
        //        dr["Ser_pro_Name"] = Ser_pro_Name1.ToString();

        //        dr["Sale_rate"] = Sale_rate1.ToString();
        //        dr["service_Tax_Rate"] = service_Tax_Rate1.ToString();
        //        dr["Total_quanty"] = Total_quanty1.ToString();
        //        Dt.Rows.Add(dr);



        //    }

        //}

        //private void newgrid()
        //{
        //    DataTable dt;
        //    dt = first_datatable;
        //    DataRow dr = null;
        //    for (int i = 0; i <= dt.Rows.Count - 1; i++)
        //    {
        //        string Ser_pro_code1 = (String)first_datatable.Rows[i][0];
        //        string Ser_pro_Name1 = (String)first_datatable.Rows[i][1]; 
        //        string Sale_rate1 = (String)first_datatable.Rows[i][2];
        //        string service_Tax_Rate1 = (String)first_datatable.Rows[i][3];
        //        string Total_quanty1 = (String)first_datatable.Rows[i][4];
        //        dr = Dt.NewRow();
        //        dr["Ser_pro_code"] = Ser_pro_code1.ToString();
        //        dr["Ser_pro_Name"] = Ser_pro_Name1.ToString();
        //        dr["Sale_rate"] = Sale_rate1.ToString();
        //        dr["service_Tax_Rate"] = service_Tax_Rate1.ToString();
        //        dr["Total_quanty"] = Total_quanty1.ToString();
        //        Dt.Rows.Add(dr);

        //    }
        //}


        //private void newgrid3()
        //{
        //    DataTable dt;
        //    dt = first_datatable;

        //    DataRow dr = null;
        //    DataColumn Ser_pro_code = new DataColumn("Ser_pro_code", typeof(string));
        //    Dt.Columns.Add(Ser_pro_code);

        //    DataColumn Ser_pro_Name = new DataColumn("Ser_pro_Name", typeof(string));
        //    Dt.Columns.Add(Ser_pro_Name);
        //    DataColumn Vendor_rate = new DataColumn("Vendor_rate", typeof(string));
        //    Dt.Columns.Add(Vendor_rate);
        //    DataColumn Sale_rate = new DataColumn("Sale_rate", typeof(string));
        //    Dt.Columns.Add(Sale_rate);
        //    DataColumn service_Tax_Rate = new DataColumn("service_Tax_Rate", typeof(string));
        //    Dt.Columns.Add(service_Tax_Rate);
        //    DataColumn Total_quanty = new DataColumn("Total_quanty", typeof(string));
        //    Dt.Columns.Add(Total_quanty);

        //    for (int i = 0; i <= dt.Rows.Count - 1; i++)
        //    {
        //        string Ser_pro_code1 = (String)first_datatable.Rows[i][0];
        //        string Ser_pro_Name1 = (String)first_datatable.Rows[i][1]; ;

        //        string Sale_rate1 = (String)first_datatable.Rows[i][2];
        //        string service_Tax_Rate1 = (String)first_datatable.Rows[i][3];

        //        dr = Dt.NewRow();
        //        dr["Ser_pro_code"] = Ser_pro_code1.ToString();
        //        dr["Ser_pro_Name"] = Ser_pro_Name1.ToString();
        //        dr["Sale_rate"] = Sale_rate1.ToString();
        //        dr["service_Tax_Rate"] = service_Tax_Rate1.ToString();

        //        Dt.Rows.Add(dr);



        //    }

        //}

        //private void newgrid2()
        //{
        //    DataTable dt;
        //    dt = first_datatable;
        //    DataRow dr = null;
        //    for (int i = 0; i <= dt.Rows.Count - 1; i++)
        //    {
        //        string Ser_pro_code1 = (String)first_datatable.Rows[i][0];
        //        string Ser_pro_Name1 = (String)first_datatable.Rows[i][1]; ;

        //        string Sale_rate1 = (String)first_datatable.Rows[i][2];
        //        string service_Tax_Rate1 = (String)first_datatable.Rows[i][3];

        //        dr = Dt.NewRow();
        //        dr["Ser_pro_code"] = Ser_pro_code1.ToString();
        //        dr["Ser_pro_Name"] = Ser_pro_Name1.ToString();

        //        dr["Sale_rate"] = Sale_rate1.ToString();
        //        dr["service_Tax_Rate"] = service_Tax_Rate1.ToString();

        //        Dt.Rows.Add(dr);



        //    }
        //}

        protected void Button2_Click(object sender, EventArgs e)
        {

            gridProdWithCat.Visible = true;

            string cmdstring = "select Id,Product_code,ProductName,Type,Sail_Rate,Tax_Rate,Unit,Brand,ProductOrServiceCat from tbl_NewProduct where ProductOrServiceCat=@ProductOrServiceCat order by Type,ProductName";
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


            ////string code = Label2.Text;
            ////string product_service = code.Substring(0, 1);
            ////string qno1 = lblqno.Text;
            ////string datrstatus= searchDate(qno1);
            ////if (datrstatus=="YES")
            ////{
            ////    string cmdstring = "select Product_code,Sub_Prod_Name,Sail_Rate,Tax_Rate from tbl_NewProduct where Product_Name='" + cmbproduct_service.Text + "'";
            ////    Bindproduct(cmdstring);
            ////}
            ////else {

            ////    if (product_service.ToString() == "P")
            ////    {
            ////        string cmdstring = "select Product_code,Sub_Prod_Name,Sail_Rate,Tax_Rate from tbl_Product where Product_Name='" + cmbproduct_service.Text + "'";
            ////        Bindproduct(cmdstring);
            ////    }
            ////    else if (product_service.ToString() == "S")
            ////    {
            ////        string cmdstring = "select Service_code,Service_name,Sail_rate,Tax_rate  from tbl_Service where Service_name='" + cmbproduct_service.Text + "'";
            ////        Bindproduct(cmdstring);
            ////    }
            ////}
            ////cmbproduct_service.SelectedIndex = 0;
            ////gd_Service_Product.DataSource = Dt;
            ////gd_Service_Product.DataBind();
            ////ViewState["dt"] = Dt;
        }

        //private void Bindproduct(string cmdstring)
        //{
            
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    SqlCommand com1 = new SqlCommand(cmdstring, DbCL.Conn);
        //    SqlDataAdapter da = new SqlDataAdapter(com1);
        //    SqlDataReader dr = com1.ExecuteReader();

        //    if (dr.Read())
        //    {
        //        DataTable dt = DbCL.GetDataTable(cmdstring);
        //        first_datatable = dt;
        //        if (Label1.Text == "1")
        //        {
        //            newgrid3();
        //        }
        //        else
        //        {
        //            newgrid2();
        //        }
        //        Label1.Text = (Convert.ToInt32(Label1.Text) + 1).ToString();

        //    }
        //    DbCL.Conn.Close();
        //}

        protected void btnSabe_Click(object sender, EventArgs e)
        {
            string qno = lblqno.Text;
            string query = "select Status1,Status2,PaymentStatus from tbl_Quotation where Quotation_no=@Quotation_no";
            SqlParameter[] pram = {
                new SqlParameter("@Quotation_no",qno)
            };
            
            dtProInvPay = DbCL.SPreturn_dt(query,pram);
            if (dtProInvPay.Rows.Count>0)
            {
                string status = "";
                string pro = dtProInvPay.Rows[0]["Status1"].ToString();
                string inv = dtProInvPay.Rows[0]["Status2"].ToString();
                string pay = dtProInvPay.Rows[0]["PaymentStatus"].ToString();
                if (pro=="Yes" || inv== "Yes" || pay== "Yes")
                {
                    if (pro == "Yes")
                    {
                        status = "Proforma Invoice";
                    }
                    if (inv == "Yes")
                    {
                        status = status + " Tax Invoice";
                    }
                    if (pay == "Yes")
                    {
                        status = status + " Payment Invoice";
                    }
                    status = "Delete " + status;

                    PanelError.Visible = true;
                    lblErrorMsg.Text = status;
                }
                else
                {
                    double gross = 0;
                    double netamount = 0;
                    double totaltax = 0;
                    double subtotal = 0;
                    double taxamo = 0;


                    DbCL.executeRdr("delete from tbl_Quotaion_details where Quotation_no='" + lblqno.Text + "'");
                    DbCL.executeRdr("delete from tbl_quotation_vat where Quotation_no='" + lblqno.Text + "'");
                    int i = 0;
                    int h = 1;
                    DataTable dt1;
                    DbCL.Sqlconnection();
                    DbCL.ConnectDb();
                    dt1 = (DataTable)ViewState["PhaseProductData"];
                    if (dt1 != null)
                    {

                        for (i = 0; i <= dt1.Rows.Count - 1; i++)
                        {
                            SqlTransaction trans = null;
                            SqlConnection conn = null;
                            SqlCommand cmd = null;
                            try
                            {
                                CheckBox chk = (CheckBox)(gd_Service_Product.Rows[i].FindControl("chk"));
                                if (chk.Checked == true)
                                {
                                    string cnnString = System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ToString();
                                    conn = new SqlConnection(cnnString);
                                    cmd = new SqlCommand { CommandType = CommandType.Text, Connection = conn };
                                    conn.Open();
                                    trans = conn.BeginTransaction();
                                    cmd.Transaction = trans;

                                    //string Ser_pro_code = ((Label)gd_Service_Product.Rows[i].FindControl("Ser_pro_code")).Text;
                                    //string Ser_pro_Name = ((Label)gd_Service_Product.Rows[i].FindControl("Ser_pro_Name")).Text;
                                    //string specification = ((TextBox)gd_Service_Product.Rows[i].FindControl("specification")).Text;
                                    //string Quantity = ((TextBox)gd_Service_Product.Rows[i].FindControl("Total_quanty")).Text;
                                    //string Sale_rate = ((TextBox)gd_Service_Product.Rows[i].FindControl("Sale_rate")).Text;
                                    //string service_Tax_Rate = ((Label)gd_Service_Product.Rows[i].FindControl("service_Tax_Rate")).Text;

                                    string Product_code = ((Label)gd_Service_Product.Rows[i].FindControl("Product_code")).Text;
                                    string ProductName = ((Label)gd_Service_Product.Rows[i].FindControl("ProductName")).Text;
                                    string Brand = ((Label)gd_Service_Product.Rows[i].FindControl("Brand")).Text;
                                    string Quantity = ((TextBox)gd_Service_Product.Rows[i].FindControl("Quantity")).Text;
                                    string Sail_Rate = ((TextBox)gd_Service_Product.Rows[i].FindControl("Sail_Rate")).Text;
                                    string Tax_Rate = ((Label)gd_Service_Product.Rows[i].FindControl("Tax_Rate")).Text;

                                    string Type = ((Label)gd_Service_Product.Rows[i].FindControl("Type")).Text;
                                    string Unit = ((Label)gd_Service_Product.Rows[i].FindControl("Unit")).Text;

                                    string ProductOrServiceCat = ((Label)gd_Service_Product.Rows[i].FindControl("ProductOrServiceCat")).Text;

                                    double quantity = 0;
                                    double sailrate = 0;
                                    double taxrate = 0;
                                    double Total_sail_rate = 0;
                                    double Total_sail_rate1 = 0;
                                    double Total_sail_rate2 = 0;



                                    if (RadioDiscountInflation.SelectedIndex == 0)
                                    {
                                        sailrate = Convert.ToDouble(Sail_Rate);
                                        sailrate = Math.Round(((Convert.ToDouble(txtPercentage.Text) * sailrate) / 100), 2);
                                        sailrate = Math.Round((Convert.ToDouble(Sail_Rate) - sailrate), 2);
                                    }
                                    else if (RadioDiscountInflation.SelectedIndex == 1)
                                    {
                                        sailrate = Convert.ToDouble(Sail_Rate);
                                        sailrate = Math.Round(((Convert.ToDouble(txtPercentage.Text) * sailrate) / 100), 2);
                                        sailrate = Math.Round((Convert.ToDouble(Sail_Rate) + sailrate), 2);
                                    }
                                    else if (RadioDiscountInflation.SelectedIndex == 2)
                                    {
                                        sailrate = Convert.ToDouble(Sail_Rate);
                                    }


                                    quantity = Convert.ToDouble(Quantity);
                                    //sailrate = Convert.ToDouble(Sail_Rate);
                                    taxrate = Convert.ToDouble(Tax_Rate);

                                    Total_sail_rate = sailrate + Math.Round(((sailrate * taxrate) / 100), 2);
                                    Total_sail_rate1 = Math.Round((Total_sail_rate * quantity), 2);
                                    Total_sail_rate2 = Math.Round((sailrate * quantity), 2);

                                    subtotal = subtotal + Total_sail_rate2;
                                    subtotal= Math.Round(subtotal);

                                    gross = gross + Total_sail_rate1;
                                    netamount = gross;

                                    netamount=Math.Round(netamount);

                                    taxamo = Math.Round(((sailrate * taxrate) / 100), 2);
                                    totaltax = totaltax + taxamo;
                                    totaltax = Math.Round(totaltax);


                                    //decimal d = Convert.ToDecimal(Tax_Rate) + 100;
                                    //decimal b = d * Convert.ToDecimal(Sail_Rate) / 100;
                                    decimal service = (Convert.ToDecimal(Tax_Rate) * Convert.ToDecimal(Quantity) * Convert.ToDecimal(Sail_Rate)) / 100;
                                    //decimal c = b * Convert.ToDecimal(Quantity);
                                    //decimal g = Convert.ToDecimal(Quantity) * Convert.ToDecimal(Sail_Rate);

                                    //sub_total = sub_total + g;

                                    insertvatamount(service, Tax_Rate);

                                    //Gross_amount = Gross_amount + c;
                                    //Gross_amount = Math.Round(Gross_amount, 2);
                                    //total_Service = total_Service + service;

                                    //cmd.CommandText = ("insert into tbl_Quotaion_details(Sl_no,Quotation_no,Product_id,Product_name,Quantity,sail_rate,Service_tax_rate,Total_sail_rate,Total_sail_rate1,Total_sail_rate2,specification,Type,Unit)values('" + h.ToString() + "','" + lblqno.Text + "','" + Product_code + "','" + ProductName + "','" + Quantity + "','" + sailrate.ToString() + "','" + Tax_Rate + "','" + b + "','" + c + "','" + g + "','" + Brand.ToString() + ",'" + Type.ToString() + ",'" + Unit.ToString() + "')");
                                    cmd.CommandText = ("insert into tbl_Quotaion_details(Sl_no,Quotation_no,Product_id,Product_name,Quantity,sail_rate,Service_tax_rate,Total_sail_rate,Total_sail_rate1,Total_sail_rate2,specification,Type,Unit,ProductOrServiceCat)values('" + h.ToString() + "','" + lblqno.Text + "','" + Product_code + "','" + ProductName + "','" + Quantity + "','" + sailrate + "','" + Tax_Rate + "','" + Total_sail_rate + "','" + Total_sail_rate1 + "','" + Total_sail_rate2 + "','" + Brand.ToString() + "','" + Type.ToString() + "','" + Unit.ToString() + "','" + ProductOrServiceCat.ToString() + "')");

                                    cmd.ExecuteNonQuery();
                                    trans.Commit();
                                    conn.Close();
                                    trans.Dispose();
                                    conn.Dispose();
                                    cmd.Dispose();
                                    h = h + 1;

                                }
                            }
                            catch (Exception ex)
                            {
                                i = 1;
                                if (trans != null) trans.Rollback();
                                throw ex;
                            }
                            finally
                            {
                                if (conn != null) conn.Close();
                            }

                        }
                    }

                    DbCL.Conn.Close();
                    Service_tax = Convert.ToDecimal(gross) % 1;
                    //total_sail_rate_details = Gross_amount;
                    //total_sail_rate_details = Math.Round(total_sail_rate_details);
                    //total_Service = Math.Round(total_Service);
                    DbCL.executeRdr("update tbl_Quotation set Gross='" + gross + "',Service_tax='" + Service_tax + "',Net_amount='" + netamount + "',service_tax1='" + totaltax + "',sub_total='" + subtotal + "' where Quotation_no='" + lblqno.Text + "'");

                    //DbCL.executeRdr("update tbl_Quotation set Gross='" + Gross_amount + "',Service_tax='" + Service_tax + "',Net_amount='" + total_sail_rate_details + "',service_tax1='" + total_Service + "',sub_total='" + sub_total + "' where Quotation_no='" + lblqno.Text + "'");
                    //DbCL.executeRdr("insert into tbl_Quotation(Quotation_no,Quotation_date,Client_Id,Gross,Service_tax,Net_amount,Status1,Status2,Sl_no,status3,service_tax1,sub_total)values('" + lblqno.Text + "','" + txtquotationDate.Text + "','" + lblclientID.Text + "','" + Gross_amount + "','" + Service_tax + "','" + total_sail_rate_details + "','No','No','" + j.ToString() + "','No','" + total_Service + "','" + sub_total + "')");

                    updatedueamountdetails(netamount);
                    lblOk.Text = "Data Save Successfully.....";
                    PanelOK.Visible = true;
                    btnSabe.Visible = false;
                }
            }

        }

        private void updatedueamountdetails(double netamount)
        {
            string a = findtotalamount();
            double amount = netamount - Convert.ToDouble(a);
            DbCL.executeRdr("update tbl_invoice_due set Due_amount='" + amount.ToString() + "' where qutation_no='" + lblqno.Text + "'");

        }

        private string findtotalamount()
        {
            DbCL.Sqlconnection();
            string amount = "0";
            DbCL.ConnectDb();
            
            string cmdstring = "select sum(cast(Given_amount as real)) as amount from tbl_invoice_payment where Quotation_No='" + lblqno.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                amount = re["amount"].ToString();
                if (amount!="")
                {
                    amount = re["amount"].ToString();
                }
                else
                {
                    amount = "0";
                }
            }
            else
            
            DbCL.Conn.Close();
            return amount;
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

        protected void btnAddProduct_Click(object sender, EventArgs e)
        {
            gridProdWithCat.Visible = false;
            if (ViewState["dtprocat"] != null)
            {
                DataTable dtpro = new DataTable();
                dtpro = ViewState["dtprocat"] as DataTable;

                ////string Ser_pro_code = "";
                ////string Ser_pro_Name = "";
                ////string specification = "";

                ////string Sale_rate = "";
                ////string service_Tax_Rate = "";


                string Product_code = "";
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
                        Product_code = ((Label)gridProdWithCat.Rows[i].FindControl("Product_code")).Text;
                        ProductName = ((Label)gridProdWithCat.Rows[i].FindControl("ProductName")).Text;
                        Brandspecification = ((Label)gridProdWithCat.Rows[i].FindControl("Brand")).Text;
                        Quantity = ((Label)gridProdWithCat.Rows[i].FindControl("Quantity")).Text;
                        Sail_Rate = ((Label)gridProdWithCat.Rows[i].FindControl("Sail_Rate")).Text;
                        Tax_Rate = ((Label)gridProdWithCat.Rows[i].FindControl("Tax_Rate")).Text;
                        Type = ((Label)gridProdWithCat.Rows[i].FindControl("Type")).Text;
                        Unit = ((Label)gridProdWithCat.Rows[i].FindControl("Unit")).Text;

                        ProductOrServiceCat = ((Label)gridProdWithCat.Rows[i].FindControl("ProductOrServiceCat")).Text;

                        if (ViewState["PhaseProductData"] != null)
                        {
                            dtPCat = (DataTable)ViewState["PhaseProductData"];
                            int count = dtPCat.Rows.Count + 1;

                            SearchProductCatwise(count, Product_code, ProductName, Brandspecification, Quantity, Sail_Rate, Tax_Rate, Type, Unit, ProductOrServiceCat);

                        }
                        else
                        {
                            SearchProductCatwise(1, Product_code, ProductName, Brandspecification, Quantity, Sail_Rate, Tax_Rate, Type, Unit, ProductOrServiceCat);
                        }
                    }
                }
            }
        }

        private void SearchProductCatwise(int count, string Product_code, string ProductName, string Brandspecification, string Quantity, string Sail_Rate, string Tax_Rate, string Type, string Unit, string ProductOrServiceCat)
        {
            DataRow dr;
            if (count == 1)
            {
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
                    }
                }
                dr = dtPCat.NewRow();
                dr[0] = Product_code;
                dr[1] = ProductName;
                dr[2] = Sail_Rate;
                dr[3] = Tax_Rate;
                dr[4] = Quantity;
                dr[5] = Brandspecification;
                dr[6] = Type;
                dr[7] = Unit;
                dr[8] = ProductOrServiceCat;

                dtPCat.Rows.Add(dr);
            }
            else
            {
                dr = dtPCat.NewRow();
                dr[0] = Product_code;
                dr[1] = ProductName;
                dr[2] = Sail_Rate;
                dr[3] = Tax_Rate;
                dr[4] = Quantity;
                dr[5] = Brandspecification;
                dr[6] = Type;
                dr[7] = Unit;
                dr[8] = ProductOrServiceCat;

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