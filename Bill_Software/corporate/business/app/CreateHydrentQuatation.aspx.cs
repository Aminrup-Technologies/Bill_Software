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
    public partial class CreateHydrentQuatation : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();

        public string invdate = "";

        public DataTable first_datatable;
        public static DataTable Dt = new DataTable("Table");
        public static decimal Gross_amount = 0;
        public static decimal Service_tax = 0;
        public static decimal total_sail_rate_details = 0;
        public static decimal total_Service = 0;
        public static decimal sub_total = 0;
        public string clientid = "";

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
                //Dt = null;
                Dt = new DataTable("Table");
                DbCL.FillCombo(cmbClient, "select Client_Name from tbl_Client order by Client_Name");

                txtquotationDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");

            }
        }

        protected void cmbClient_SelectedIndexChanged(object sender, EventArgs e)
        {
            string queryPS = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                queryPS = "select Product_code,Product_Name,short_form,Tax_Rate,base_rate from tbl_HydrantProduct";
            }
            else
            {
                queryPS = "select Service_code as Product_code ,Service_name as Product_Name,Sail_rate as base_rate,Tax_rate  from tbl_Service";
            }

            //string query = "select Product_code,Product_Name,short_form,Tax_Rate,base_rate from tbl_HydrantProduct";
            DataTable dtp = new DataTable();
            dtp = DbCL.SPreturn_dt(queryPS, null);
            if (dtp.Rows.Count > 0)
            {
                gd_Service_Product.DataSource = dtp;
                gd_Service_Product.DataBind();
                ViewState["dtpv"] = dtp;
            }

        }

        protected void Button3_Click(object sender, EventArgs e)
        {

            string qno= Bindquotationno();
            clientid = BindclientID();
            int j = idreturn();
            j = j + 1;
            int i = 0;

            DataTable dt1;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            dt1 = (DataTable)ViewState["dtpv"];
            if (dt1 != null)
            {
                for (i = 0; i <= dt1.Rows.Count - 1; i++)
                {

                    CheckBox chk = (CheckBox)(gd_Service_Product.Rows[i].FindControl("chk"));
                    if (chk.Checked == true)
                    {
                            string Ser_pro_code = ((Label)gd_Service_Product.Rows[i].FindControl("Product_code")).Text;
                            string Ser_pro_Name = ((Label)gd_Service_Product.Rows[i].FindControl("Product_Name")).Text;
                            string specification = ((TextBox)gd_Service_Product.Rows[i].FindControl("specification")).Text;

                            string base_rate = ((TextBox)gd_Service_Product.Rows[i].FindControl("base_rate")).Text;
                            string service_Tax_Rate = ((Label)gd_Service_Product.Rows[i].FindControl("Tax_Rate")).Text;
                            string Quantity = ((TextBox)gd_Service_Product.Rows[i].FindControl("Quantity")).Text;

                            double baserate = Convert.ToDouble(base_rate);
                            double ServiceTaxRate = Convert.ToDouble(service_Tax_Rate);
                            int quantity = Convert.ToInt32(Quantity);


                            double Service_tax_Amount = Convert.ToDouble(baserate * ServiceTaxRate / 100);
                            double Service_tax_Amount_total = Convert.ToDouble(quantity * Service_tax_Amount);
                            double total_amount = Convert.ToInt32(baserate * quantity);

                            double Gross_amount = Math.Round(Service_tax_Amount, 2);

                            insertvatamount(qno, Ser_pro_code, Ser_pro_Name, baserate, ServiceTaxRate, quantity, Service_tax_Amount, Service_tax_Amount_total, total_amount, specification);

                        //double pesscore = (double)(40 * maximum_score / 100);
                        //double pesscore = Convert.ToDouble(baserate * ServiceTaxRate / 100);



                        //decimal d = Convert.ToDecimal(service_Tax_Rate) + 100;
                        //    decimal b = d * Convert.ToDecimal(Sale_rate) / 100;
                        //    decimal service = (Convert.ToDecimal(service_Tax_Rate) * Convert.ToDecimal(Quantity) * Convert.ToDecimal(Sale_rate)) / 100;

                        //    decimal c = b * Convert.ToDecimal(Quantity);
                        //    decimal g = Convert.ToDecimal(Quantity) * Convert.ToDecimal(Sale_rate);
                        //    sub_total = sub_total + g;
                        //    insertvatamount(service, service_Tax_Rate);

                        //    Gross_amount = Gross_amount + c;
                        //    Gross_amount = Math.Round()
                        //    total_Service = total_Service + service;
                        //    cmd.CommandText = ("insert into tbl_Quotaion_details(Sl_no,Quotation_no,Product_id,Product_name,Quantity,sail_rate,Service_tax_rate,Total_sail_rate,Total_sail_rate1,Total_sail_rate2,specification)values('" + h.ToString() + "','" + lblqno.Text + "','" + Ser_pro_code + "','" + Ser_pro_Name + "','" + Quantity + "','" + Sale_rate + "','" + service_Tax_Rate + "','" + b + "','" + c + "','" + g + "','" + specification.ToString() + "')");
                        //    cmd.ExecuteNonQuery();



                    }

                }
            }


            string query = "insert into tbl_qsHydrentQuotation(Quotation_no, Q_date, ClientId, slno,invStatus) values(@Quotation_no, @Q_date, @ClientId, @slno, @invStatus)";
            SqlParameter[] pram = {
                                     new SqlParameter("@Quotation_no", lblqno.Text),
                                     new SqlParameter("@Q_date", txtquotationDate.Text),
                                     new SqlParameter("@ClientId", clientid),
                                     new SqlParameter("@slno", j.ToString()),
                                     new SqlParameter("@invStatus", "No")
                                  };
            DbCL.SPExecDB(query, pram);

          // DbCL.Conn.Close();
          // Service_tax = Gross_amount % 1;
          //total_sail_rate_details = Gross_amount;
          //total_sail_rate_details = Math.Round(total_sail_rate_details);
          //total_Service = Math.Round(total_Service);
          // DbCL.executeRdr("insert into tbl_Quotation(Quotation_no,Quotation_date,Client_Id,Gross,Service_tax,Net_amount,Status1,Status2,Sl_no,status3,service_tax1,sub_total)values('" + lblqno.Text + "','" + txtquotationDate.Text + "','" + lblclientID.Text + "','" + Gross_amount + "','" + Service_tax + "','" + total_sail_rate_details + "','No','No','" + j.ToString() + "','No','" + total_Service + "','" + sub_total + "')");
            
            lblOk.Text = "Data Save Successfully.....";
            PanelOK.Visible = true;
            Button3.Visible = false;
        }

        private string BindclientID()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Id from tbl_Client where Client_Name='" + cmbClient.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                clientid = re["Client_Id"].ToString();
            }
            DbCL.Conn.Close();

            return clientid;
        }

        private void insertvatamount(string qno, string ser_pro_code, string ser_pro_Name, double baserate, double serviceTaxRate, int quantity, double service_tax_Amount, double service_tax_Amount_total, double total_amount, string specification)
        {
            string query = "insert into tbl_qsHydrentDetails(Quotation_no,Product_id,Product_name,Quantity,base_rate,Service_tax_rate,Service_tax_Amount,Service_tax_Amount_total,total_amount,specification) values (@Quotation_no,@Product_id,@Product_name,@Quantity,@base_rate,@Service_tax_rate,@Service_tax_Amount,@Service_tax_Amount_total,@total_amount,@specification)";
            SqlParameter[] pram = {
                                     new SqlParameter("@Quotation_no", qno),
                                     new SqlParameter("@Product_id", ser_pro_code),
                                     new SqlParameter("@Product_name", ser_pro_Name),
                                     new SqlParameter("@Quantity", quantity),
                                     new SqlParameter("@base_rate", baserate),
                                     new SqlParameter("@Service_tax_rate", serviceTaxRate),
                                     new SqlParameter("@Service_tax_Amount", service_tax_Amount),
                                     new SqlParameter("@Service_tax_Amount_total", service_tax_Amount_total),
                                     new SqlParameter("@total_amount", total_amount),
                                     new SqlParameter("@specification", specification),
                                  };
            DbCL.SPExecDB(query, pram);
        }

        private string Bindquotationno()
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

            //int j = idreturn();
            //j = j + 1;

            //f = "INV/" + f + "/" + invdate+ "/";
            //f = f + j.ToString();

            f = "I2I/" + f + "/";
            string ss = findmonth();
            f = f + ss;
            int j = idreturn();
            j = j + 1;
            f = f + j.ToString();
            lblqno.Text = f.ToString();
            return f;
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

                invdate = date4.Substring(2, 2) + "-" + date3.Substring(2, 2);
                Session["invdate"] = invdate;
            }
            else
            {
                date4 = ((Convert.ToInt32(date3) + 1)).ToString();
                date5 = "31-Mar-" + date3;
                date6 = "31-Mar-" + date4;

                invdate = date3.Substring(2, 2) + "-" + date4.Substring(2, 2);
                Session["invdate"] = invdate;
            }
            string cmdstring = "select slno from tbl_qsHydrentQuotation where id=(select max(id) from tbl_qsHydrentQuotation where cast(Q_date as datetime) between '" + date5.ToString() + "' and '" + date6.ToString() + "')";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                a = re["slno"].ToString();
                b = Convert.ToInt32(a);
            }
            else
            {
                b = 0;
            }
            DbCL.Conn.Close();
            return b;
        }
    }
}