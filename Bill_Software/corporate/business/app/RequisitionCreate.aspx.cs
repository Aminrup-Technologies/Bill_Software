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

    public partial class WebForm66 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        public DataTable first_datatable;
        public static DataTable Dt = new DataTable("Table");
        public static decimal Gross_amount = 0;
        public static decimal Service_tax = 0;
        public static decimal total_sail_rate_details = 0;
        public static decimal total_Service = 0;
        public static decimal sub_total = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                Gross_amount = 0;
                Service_tax = 0;
                total_sail_rate_details = 0;
                total_Service = 0;
                sub_total = 0;
                Dt = new DataTable("Table");
                DbCL.FillCombo(cmbClient, "select Client_Name from tbl_Client order by Client_Name");
                txtquotationDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Panel1.Visible = true;
            cmbClient.Enabled = false;
            BindListitem();
            //BindclientID();
            Bindquotationno();
            txtquotationDate.Enabled = false;
            RadioButtonList1.Enabled = false;
            Label1.Text = "1";
        }


        private void BindListitem()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {

                cmdstring = "select Product_Name from tbl_parentProduct order by Product_Name";
            }
            else
            {
                cmdstring = "select Service_name from tbl_Service order by Service_name";
            }
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

        protected void Button2_Click(object sender, EventArgs e)
        {
            Panel2.Visible = true;
            if (RadioButtonList1.SelectedIndex == 0)
            {
                string cmdstring = "select Product_code,Sub_Prod_Name,Sail_Rate,Tax_Rate from tbl_Product where Product_Name='" + cmbproduct_service.Text + "'";
                Binddata1(cmdstring);
            }
            else
            {
                string cmdstring = "select Service_code,Service_name,Sail_rate,Tax_rate  from tbl_Service where Service_name='" + cmbproduct_service.Text + "'";
                Binddata1(cmdstring);
            }
            cmbproduct_service.SelectedIndex = 0;


            gd_Service_Product.DataSource = Dt;
            gd_Service_Product.DataBind();
            ViewState["dt"] = Dt;
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
            DataTable dt;
            dt = first_datatable;

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
            DataTable dt;
            dt = first_datatable;
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


        private void Bindquotationno()
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
            f = "I2I/" + f + "/";
            int j = idreturn();
            j = j + 1;
            f = f + j.ToString();
            lblqno.Text = f.ToString();

        }
        private int idreturn()
        {
            string a = null;
            int b = 0;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            
            string cmdstring = "select id from tbl_requisitionBankDetails where id=(select max(id) from tbl_requisitionBankDetails)";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                a = re["id"].ToString();
                b = Convert.ToInt32(a);
            }
            else
            {
                b = 0;
            }
            DbCL.Conn.Close();
            return b;

        }

      

        protected void Button3_Click(object sender, EventArgs e)
        {
            //int j = idreturn();
            //j = j + 1;
            //int i = 0;

            DataTable dt1;

           
            string date1 = "";
            string no = "";
            string bank = "";
            string ifsc = "";

            string cgstorigst = "";

            dt1 = (DataTable)ViewState["dt"];
            if (dt1 != null)
            {
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    CheckBox chk = (CheckBox)(gd_Service_Product.Rows[i].FindControl("chk"));
                    if (chk.Checked == true)
                    {
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

                        double QtitySalerateTotal = Math.Round((Qtity * Salerate),2);//Retrive Two floating point number

                        double gst = Math.Round(((QtitySalerateTotal * serviceTaxRate) / 100),2);

                        double cgst = Math.Round((gst/2), 2);
                        double sgst = cgst;

                        double gstPlusAmount = QtitySalerateTotal + gst;


                        string query = "insert into tbl_requisition(requeno,ProductCode,ProductName,Baserate,quantity,gstper,productAmo,gstamo,cgstamo,sgstmo,productAmoGstmo) values (@requeno,@ProductCode,@ProductName,@Baserate,@quantity,@gstper,@productAmo,@gstamo,@cgstamo,@sgstmo,@productAmoGstmo)";
                        SqlParameter[] pram = {
                            new SqlParameter("@requeno",lblqno.Text),
                            new SqlParameter("@ProductCode",Ser_pro_code),
                            new SqlParameter("@ProductName",proname),
                            new SqlParameter("@Baserate",Salerate),
                            new SqlParameter("@quantity",Qtity),
                            new SqlParameter("@gstper",serviceTaxRate),
                            new SqlParameter("@productAmo",QtitySalerateTotal),
                            new SqlParameter("@gstamo",gst),
                            new SqlParameter("@cgstamo",cgst),
                            new SqlParameter("@sgstmo",sgst),
                            new SqlParameter("@productAmoGstmo",gstPlusAmount),
                        };

                        DbCL.SPExecDB(query, pram);
                    }
                }

                if (RadioButtonList2.SelectedIndex == 0)
                {

                    date1 = " Dated:" + txtcashDate.Text;

                }
                else if (RadioButtonList2.SelectedIndex == 3)
                {
                    date1 = " Dated:" + txtneftdate.Text;
                    no = txtneftnumber.Text + ",";
                    //bank = txtbankname1.Text;
                    ifsc= txtifscCode.Text;

                }
                else
                {
                    date1 = " Dated:" + txtdddate.Text;
                    no = txtDDno.Text + ",";
                    bank = txtBankName.Text;
                }

                if (radioGstType.SelectedIndex==0)
                {
                    cgstorigst = "cgst";
                }
                else
                {
                    cgstorigst = "igst";
                }
                string clientname = cmbClient.Text;
                string clientaddress = searchaddress(clientname);

                string query1 = "insert into tbl_requisitionBankDetails(requeno,reqDate,CompName,address,paytype,chkno,bankname,ifscCode,date,cgstorsgst) values (@requeno,@reqDate,@CompName,@address,@paytype,@chkno,@bankname,@ifscCode,@date,@cgstorsgst)";
                SqlParameter[] pram1 = {
                                              new SqlParameter("@requeno",lblqno.Text),
                                              new SqlParameter("@reqDate",txtquotationDate.Text),
                                              new SqlParameter("@CompName",cmbClient.Text),
                                              new SqlParameter("@address",clientaddress),
                                              new SqlParameter("@paytype",RadioButtonList2.Text),
                                              new SqlParameter("@chkno",no),
                                              new SqlParameter("@bankname",bank),
                                              new SqlParameter("@ifscCode",ifsc),
                                              new SqlParameter("@date",date1),
                                              new SqlParameter("@cgstorsgst",cgstorigst)
                                       };
                DbCL.SPExecDB(query1, pram1);
            }
        }

        private string searchaddress(string clientname)
        {
            string add = "";
            string query = "select (Address1 +' '+ Address2+', '+City+', '+pin) as MainAdd,State,Rep_Name,Rep_Desig,Rep_phone,Rep_email from tbl_Client where Client_Name=@Client_Name"; 
            SqlParameter[] pram = {
                new SqlParameter("@Client_Name",clientname)
            };
            SqlDataReader rdr = DbCL.SPReturnRdr(query, pram);
            if (rdr.Read())
            {
                add = rdr["MainAdd"].ToString();
            }
            return add;
        }

        protected void RadioButtonList2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (RadioButtonList2.SelectedIndex == 0)
            {
                First.Visible = true;
                Second.Visible = false;
                Third.Visible = false;
            }
            else if (RadioButtonList2.SelectedIndex == 3)
            {
                First.Visible = false;
                Second.Visible = false;
                Third.Visible = true;

            }
            else
            {
                First.Visible = false;
                Second.Visible = true;
                Third.Visible = false;

            }
        }
    }
}