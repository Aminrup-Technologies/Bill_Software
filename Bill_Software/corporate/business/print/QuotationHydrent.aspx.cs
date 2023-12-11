using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.print
{
    public partial class QuotationHydrent : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dtamo = new DataTable();
        DataTable dtVat = new DataTable();
        DataTable dtVatfinal = new DataTable();

        public string psid = "";
        public string taxorvat = "";

        public string proOrser = "";

        public string str = "";

        public int gdfdh = 0;



        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string Quotation_no = Request.QueryString["Quotation_no"];

                //lblQno.Text = Quotation_no.ToString();
                buindalldata(Quotation_no);

                Bindtaxdata(Quotation_no);

                Buindamount(Quotation_no);
                BindVatamount(Quotation_no);

                bindSubTotalandGrandtotal(Quotation_no);
            }
        }

        private void Bindtaxdata(string quotation_no)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string idstring = "";
            string idstring1 = "";
            string cmdstring = "select Product_id from tbl_qsHydrentDetails where Quotation_no='" + quotation_no + "' order by id";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                idstring = re["Product_id"].ToString();
                idstring1 = idstring.Substring(0, 1);
                proOrser = idstring1;
                if (idstring1 == "P")
                {
                    //lbltaxstring.Text = "Vat No: ";
                    //lbltaxno.Text = "19629770012";

                    psid = "P ID";
                    taxorvat = "Vat";
                }
                else
                {
                    //lbltaxstring.Text = "Service Tax No: ";
                    //lbltaxno.Text = "AAEFI5315ESD001";
                    psid = "S ID";
                    taxorvat = "Tax";
                }
            }
            DbCL.Conn.Close();
        }

        private void BindVatamount(string quotation_no)
        {
            string query = "select distinct(Service_tax_rate) from tbl_qsHydrentDetails where Quotation_no=@Quotation_no";
            SqlParameter[] pram = { new SqlParameter("@Quotation_no", quotation_no) };
            dtVat = DbCL.SPreturn_dt(query, pram);
            if (dtVat.Rows.Count > 0)
            {
                for (int i=0; i<dtVat.Rows.Count; i++)
                {
                    string vatper = dtVat.Rows[i]["Service_tax_rate"].ToString();
                    binddata(quotation_no, vatper);
                }
            }
        }

        private void binddata(string quotation_no, string vatper)
        {
            string query = "select sum(Service_tax_Amount_total) as vattotal from tbl_qsHydrentDetails where Service_tax_rate=@Service_tax_rate and Quotation_no=@Quotation_no";
            SqlParameter[] pram = {
                new SqlParameter("@Quotation_no", quotation_no),
                new SqlParameter("@Service_tax_rate", vatper),
            };
            dtVatfinal = DbCL.SPreturn_dt(query, pram);
            if (dtVatfinal.Rows.Count > 0)
            {
                double valt = Math.Round(Convert.ToDouble(dtVatfinal.Rows[0]["vattotal"]), 2);

                if (proOrser == "P")
                {
                    str += "<tr><td style ='width:66.7%; border:none; text-align:right; font:arial; padding:5px 20px 5px 0;' font-weight:bold;>Vat ";
                }
                else
                {
                    str += "<tr><td style ='width:66.7%; border:none; text-align:right; font:arial; padding:5px 20px 5px 0;' font-weight:bold;>Tax ";
                }
                str += vatper + "%";
                str += "<td style = 'width: 33.3 %; border-top:none; text-align:right; font:arial; padding:0px 20px 0px 2px;'>";
                str += valt.ToString();
                str += "</td></tr>";
            }

            lblvatamo.Text = str.ToString();
        }

        private void bindSubTotalandGrandtotal(string quotation_no)
        {
            string query = " SELECT SUM(Service_tax_Amount_total) as 'Service_tax_Amount_total', SUM(total_amount) as 'total_amount',(SUM(Service_tax_Amount_total) + SUM(total_amount)) as 'Total' FROM tbl_qsHydrentDetails where Quotation_no =@Quotation_no";
            SqlParameter[] pram = { new SqlParameter("@Quotation_no", quotation_no) };
            dtamo = DbCL.SPreturn_dt(query, pram);
            if (dtamo.Rows.Count>0)
            {
                lblSubtotal.Text = dtamo.Rows[0]["total_amount"].ToString();

                double grandtotal = Math.Round(Convert.ToDouble(dtamo.Rows[0]["Total"]), 2);


                //double gghgh = (double)dtamo.Rows[0]["Total"];

               // decimal grandtotal2 = Math.Round(Convert.ToDecimal(dtamo.Rows[0]["Total"]), 2);
                //decimal Service_tax = grandtotal2 % 1;

               


                double decimalpoints = Math.Abs(grandtotal - Math.Floor(grandtotal));

                if (decimalpoints > 0.5)
                {
                    gdfdh= (int)Math.Round(grandtotal);
                }
                else
                {
                    gdfdh= (int)Math.Floor(grandtotal);
                }

                lblstax.Text = Math.Round(decimalpoints, 2).ToString();
                lblstax0.Text = Math.Round(decimalpoints, 2).ToString();


                lblnetamount.Text= gdfdh.ToString();

                string word = MoneyConvDS.MoneyConvFn(lblnetamount.Text);
                lblword.Text = word.ToString();

               //double amount = Math.Floor(Convert.ToDouble(dtamo.Rows[0]["Total"]));
                //double  Service_tax = grandtotal % 1;

                

            }
        }

        private void Buindamount(string quotation_no)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Product_id,(Product_name+''+specification) as Product_name,Quantity,base_rate,Service_tax_rate,Service_tax_Amount,Service_tax_Amount_total,total_amount from tbl_qsHydrentDetails where Quotation_no='"+ quotation_no + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }


        private void buindalldata(string quotation_no)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Quotation_no,Q_date,ClientId from tbl_qsHydrentQuotation where Quotation_no='" + quotation_no + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblQno.Text = re["Quotation_no"].ToString();
                lblQdate.Text = re["Q_date"].ToString();
                string clientid = re["ClientId"].ToString();
                Bindclientdetails(clientid);
                //lblSubtotal.Text = re["sub_total"].ToString();
                //lblstax.Text = re["Service_tax"].ToString();
                //lblstax0.Text = re["Service_tax"].ToString();
                //lblnetamount.Text = re["Net_amount"].ToString();

            }

            //string word = MoneyConvDS.MoneyConvFn(lblnetamount.Text);
            //lblword.Text = word.ToString();
            DbCL.Conn.Close();
        }

        private void Bindclientdetails(string clientid)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Name,Address1,Address2,City,pin,State,Rep_Name,Rep_Desig from tbl_Client where Client_Id='" + clientid + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblcompanyName.Text = re["Client_Name"].ToString();
                lbladdress1.Text = re["Address1"].ToString();
                lbladdress2.Text = re["Address2"].ToString();
                if (lbladdress2.Text == "")
                {
                    lbladdress2.Visible = false;
                }
                else
                {
                    lbladdress2.Visible = true;
                }
                lblcity.Text = re["City"].ToString();
                lblPin.Text = re["pin"].ToString();
                lblstate.Text = re["State"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {

        }

        protected void Button2_Click(object sender, EventArgs e)
        {

        }
    }
}