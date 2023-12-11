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
    public partial class hydrentInvoiceSeller : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dtamo = new DataTable();
        DataTable dtVat = new DataTable();
        DataTable dtVatfinal = new DataTable();

        double nettotalafterdiscount = 0;

        public string proOrser = "";

        public string str = "";

        public int gdfdh = 0;
        public string psid = "";
        public string taxorvat = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string ID = Request.QueryString["ID"];
                buindalldata(ID);
            }
        }

        private void buindalldata(string ID)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Invoice_No,Invoice_Date,Quotation_No,Quotation_Date,Client_ID,Net_amount,DiscountAmount,addressfor from tbl_HydrentInvoice where ID='" + ID + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblInvoiceNo.Text = re["Invoice_No"].ToString();
                lbldate.Text = re["Invoice_Date"].ToString();
                lblQno.Text = re["Quotation_No"].ToString();
                string quotano = lblQno.Text;

                Bindtaxdata(quotano);



                string clientid = re["Client_ID"].ToString();
                string addressfor = re["addressfor"].ToString();

                Bindclientdetails(clientid, addressfor);


                lbldiscount.Text = re["DiscountAmount"].ToString();
                string discount_amount = re["DiscountAmount"].ToString();

                lblnetamount.Text = re["Net_amount"].ToString();
                double discount_amount1 = Convert.ToDouble(discount_amount);
                if (discount_amount1 == 0)
                {
                    discount_row.Visible = false;
                }
                else
                {
                    discount_row.Visible = true;
                }

                Buindamount(quotano);
                BindVatamount(quotano);

                bindSubTotalandGrandtotal(quotano, discount_amount1);


            }
            DbCL.Conn.Close();


        }

        private void bindSubTotalandGrandtotal(string quotano, double discount_amount1)
        {
            string query = "SELECT SUM(Service_tax_Amount_total) as 'Service_tax_Amount_total', SUM(total_amount) as 'total_amount',(SUM(Service_tax_Amount_total) + SUM(total_amount)) as 'Total' FROM tbl_qsHydrentDetails where Quotation_no =@Quotation_no";
            SqlParameter[] pram = { new SqlParameter("@Quotation_no", quotano) };
            dtamo = DbCL.SPreturn_dt(query, pram);
            if (dtamo.Rows.Count > 0)
            {
                lblSubtotal.Text = dtamo.Rows[0]["total_amount"].ToString();

                double grandtotal = Math.Round(Convert.ToDouble(dtamo.Rows[0]["Total"]), 2);

                if (discount_amount1 != 0)
                {
                    nettotalafterdiscount = Math.Abs(grandtotal - discount_amount1);
                }
                else
                {

                    nettotalafterdiscount = grandtotal;
                }

                double decimalpoints = Math.Abs(nettotalafterdiscount - Math.Floor(nettotalafterdiscount));

                if (decimalpoints > 0.5)
                {
                    gdfdh = (int)Math.Round(nettotalafterdiscount);
                }
                else
                {
                    gdfdh = (int)Math.Floor(nettotalafterdiscount);
                }

                lblstax.Text = Math.Round(decimalpoints, 2).ToString();
                lblstax0.Text = Math.Round(decimalpoints, 2).ToString();


                lblnetamount.Text = gdfdh.ToString();

                string word = MoneyConvDS.MoneyConvFn(lblnetamount.Text);
                lblword.Text = word.ToString();
            }
        }

        private void BindVatamount(string quotano)
        {
            string query = "select distinct(Service_tax_rate) from tbl_qsHydrentDetails where Quotation_no=@Quotation_no";
            SqlParameter[] pram = { new SqlParameter("@Quotation_no", quotano) };
            dtVat = DbCL.SPreturn_dt(query, pram);
            if (dtVat.Rows.Count > 0)
            {
                for (int i = 0; i < dtVat.Rows.Count; i++)
                {
                    string vatper = dtVat.Rows[i]["Service_tax_rate"].ToString();
                    binddata(quotano, vatper);
                }
            }
        }

        private void binddata(string quotano, string vatper)
        {
            string query = "select sum(Service_tax_Amount_total) as vattotal from tbl_qsHydrentDetails where Service_tax_rate=@Service_tax_rate and Quotation_no=@Quotation_no";
            SqlParameter[] pram = {
                new SqlParameter("@Quotation_no", quotano),
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
                str += "<td style ='width:33.3 %; border-top:none; text-align:right; font:arial; padding:0px 20px 0px 2px;font-weight:bold'>";
                str += valt.ToString();
                str += "</td></tr>";
            }
            lblvatamo.Text = str.ToString();
        }

        private void Buindamount(string quotano)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Product_id,(Product_name+''+specification) as Product_name,Quantity,base_rate,Service_tax_rate,Service_tax_Amount,Service_tax_Amount_total,total_amount from tbl_qsHydrentDetails where Quotation_no='" + quotano + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            DataList1.DataSource = cmd.ExecuteReader();
            DataList1.DataBind();
            DbCL.Conn.Close();
        }

        private void Bindclientdetails(string clientid, string addressfor)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Name,Address1,Address2,City,pin,State,Rep_Name,Rep_Desig,Vat_no from tbl_Client where Client_Id='" + clientid + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblcompanyName.Text = re["Client_Name"].ToString();
                if (addressfor == "Corporate office")
                {
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
                else
                {
                    Bindaddress(clientid, addressfor);
                }

                //lblrepresentativeName.Text = re["Rep_Name"].ToString();
                //lblrepresentativedesignation.Text = re["Rep_Desig"].ToString();
                string vatno = re["Vat_no"].ToString();
                if (vatno == "")
                {
                    lblClientVat.Visible = false;
                }
                else
                {
                    lblClientVat.Visible = true;
                }

                lblClientVat.Text = "Buyer's Vat No: " + vatno;
            }
            DbCL.Conn.Close();
        }

        private void Bindaddress(string clientid, string addressfor)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Address1,Address2,city,State,pin from tbl_Factory where Client_id='" + clientid.ToString() + "' and Factory_name='" + addressfor.ToString() + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {

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
                lblcity.Text = re["city"].ToString();
                lblPin.Text = re["pin"].ToString();
                lblstate.Text = re["State"].ToString();


            }
            DbCL.Conn.Close();
        }

        private void Bindtaxdata(string quotano)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string idstring = "";
            string idstring1 = "";
            string cmdstring = "select Product_id from tbl_qsHydrentDetails where Quotation_no='" + quotano + "' order by id";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                idstring = re["Product_id"].ToString();
                idstring1 = idstring.Substring(0, 1);
                proOrser = idstring1;
                if (idstring1 == "P")
                {
                    lbltaxstring.Text = "Vat No: ";
                    lbltaxno.Text = "19629770012";
                    psid = "P ID";
                    taxorvat = "Vat";
                }
                else
                {
                    lbltaxstring.Text = "Service Tax No: ";
                    lbltaxno.Text = "AAEFI5315ESD001";
                    psid = "S ID";
                    taxorvat = "Tax";
                }
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