using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.Globalization;
using System.Threading;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm85 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dtPSer = new DataTable();
        DataTable dtrep = new DataTable();
        DataTable dtClient = new DataTable();
        DataTable dypay = new DataTable();
        CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
        public string InvoiceNo = "";

        public string mailstr = "";
        public string maillink = "";
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

            }
        }

       
        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                BuindCompanyId();
                //cmdstring = "select tbl_Client.Client_Name,tbl_invoice_payment.Payment_ID,tbl_invoice_payment.Payment_Date,tbl_invoice_payment.Invoice_No,tbl_invoice_payment.Quotation_No,tbl_invoice_payment.Net_amount,tbl_invoice_payment.Given_amount,tbl_invoice_payment.type,tbl_invoice_payment.Ch_no,tbl_invoice_payment.Ch_date from tbl_invoice_payment inner join tbl_Client on tbl_invoice_payment.Client_Id=tbl_Client.Client_Id where tbl_invoice_payment.Client_Id='" + lblclientId.Text + "' and tbl_invoice_payment.Due_amount='0.00' order by cast(tbl_invoice_payment.Payment_Date as datetime) desc";
                cmdstring = "select a.ID,a.Payment_ID,a.Payment_Date,a.Invoice_No,a.Invoice_Date,a.Quotation_No,a.Quotation_Date,a.Net_amount,a.mailDate,a.subtotal,a.Due_amount,a.Given_amount,a.type,a.Ch_no,a.Ch_date,a.tds,(a.Net_amount - a.subtotal) as Gst,b.Client_Name,c.PServiceName from tbl_invoice_payment as a left outer join tbl_QuoPriSerTogather as c on a.Quotation_No = c.qutno left outer join tbl_Client as b on b.Client_Id = a.Client_ID where a.Client_Id='" + lblclientId.Text + "' order by a.ID desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                //cmdstring = "select tbl_Client.Client_Name,tbl_invoice_payment.Payment_ID,tbl_invoice_payment.Payment_Date,tbl_invoice_payment.Invoice_No,tbl_invoice_payment.Quotation_No,tbl_invoice_payment.Net_amount,tbl_invoice_payment.Given_amount,tbl_invoice_payment.type,tbl_invoice_payment.Ch_no,tbl_invoice_payment.Ch_date from tbl_invoice_payment inner join tbl_Client on tbl_invoice_payment.Client_Id=tbl_Client.Client_Id where cast(tbl_invoice_payment.Payment_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' and tbl_invoice_payment.Due_amount='0.00' order by cast(tbl_invoice_payment.Payment_Date as datetime) desc";
                cmdstring = "select a.ID,a.Payment_ID,a.Payment_Date,a.Invoice_No,a.Invoice_Date,a.Quotation_No,a.Quotation_Date,a.Net_amount,a.mailDate,a.subtotal,a.Due_amount,a.Given_amount,a.type,a.Ch_no,a.Ch_date,a.tds,(a.Net_amount - a.subtotal) as Gst,b.Client_Name,c.PServiceName from tbl_invoice_payment as a left outer join tbl_QuoPriSerTogather as c on a.Quotation_No = c.qutno left outer join tbl_Client as b on b.Client_Id = a.Client_ID where cast(a.Payment_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by a.ID desc";
                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                //cmdstring = "select tbl_Client.Client_Name,tbl_invoice_payment.Payment_ID,tbl_invoice_payment.Payment_Date,tbl_invoice_payment.Invoice_No,tbl_invoice_payment.Quotation_No,tbl_invoice_payment.Net_amount,tbl_invoice_payment.Given_amount,tbl_invoice_payment.type,tbl_invoice_payment.Ch_no,tbl_invoice_payment.Ch_date from tbl_invoice_payment inner join tbl_Client on tbl_invoice_payment.Client_Id=tbl_Client.Client_Id where tbl_invoice_payment.Client_Id='" + lblclientId.Text + "' and cast(tbl_invoice_payment.Payment_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' and tbl_invoice_payment.Due_amount='0.00' order by cast(tbl_invoice_payment.Payment_Date as datetime) desc";
                cmdstring = "select a.ID,a.Payment_ID,a.Payment_Date,a.Invoice_No,a.Invoice_Date,a.Quotation_No,a.Quotation_Date,a.Net_amount,a.mailDate,a.subtotal,a.Due_amount,a.Given_amount,a.type,a.Ch_no,a.Ch_date,a.tds,(a.Net_amount - a.subtotal) as Gst,b.Client_Name,c.PServiceName from tbl_invoice_payment as a left outer join tbl_QuoPriSerTogather as c on a.Quotation_No = c.qutno left outer join tbl_Client as b on b.Client_Id = a.Client_ID where  a.Client_Id='" + lblclientId.Text + "' and cast(a.Payment_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by a.ID desc";
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

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Payment_ID = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Select")
            {
                string ClientId = lblclientId.Text;

                Session["Payment_ID"] = Payment_ID;
                PaymentDetails(Payment_ID);
                string Quotation_no = bindqno(Payment_ID);
                string Invoice_No = Session["Invoice_No"].ToString();
                buindPrimaryServicewithQno(ClientId, Quotation_no);
                bindRepDetails(ClientId, Invoice_No);
                bindClientDetails(ClientId, Invoice_No);

            }
        }

        private void PaymentDetails(string payment_ID)
        {
            string query = "select Invoice_No,Net_amount,type,Ch_no,Ch_bank,Ch_date,Given_amount,Due_amount,Invoice_No,Quotation_No from tbl_invoice_payment where Payment_ID=@Payment_ID";
            SqlParameter[] pram = {
                new SqlParameter("@Payment_ID",payment_ID),
            };
            dypay = DbCL.SPreturn_dt(query, pram);
            if (dypay.Rows.Count > 0)
            {
                Session["Net_amount"] = dypay.Rows[0]["Net_amount"].ToString();
                Session["Given_amount"] = dypay.Rows[0]["Given_amount"].ToString();
                

                Session["type"] = dypay.Rows[0]["type"].ToString();
                Session["Ch_no"] = dypay.Rows[0]["Ch_no"].ToString();
                //Session["PayInvoice_No"] = dypay.Rows[0]["Invoice_No"].ToString();
                Session["Ch_bank"] = dypay.Rows[0]["Ch_bank"].ToString();
                Session["Ch_date"] = dypay.Rows[0]["Ch_date"].ToString();
                Session["Due_amount"]= dypay.Rows[0]["Due_amount"].ToString();
                InvoiceNo = dypay.Rows[0]["Invoice_No"].ToString();
                if (InvoiceNo!="")
                {
                    Session["InvoiceNo"] = InvoiceNo;
                }
                Session["Quotation_No"] = dypay.Rows[0]["Quotation_No"].ToString();

                //string type = dypay.Rows[0]["type"].ToString();
                
            }
        }

        private void bindClientDetails(string clientId, string invoice_No)
        {
            string query = "select Client_Id,Client_Name,Address1,City,pin,State,Com_web_site from tbl_Client where Client_Id=@Client_Id";
            SqlParameter[] pram = {
                new SqlParameter("@Client_Id",clientId),
            };
            dtClient = DbCL.SPreturn_dt(query, pram);
            if (dtClient.Rows.Count > 0)
            {
                string clientname = dtClient.Rows[0]["Client_Name"].ToString();
                Session["client"] = clientname;
            }
        }

        private void bindRepDetails(string clientId, string invoice_No)
        {
            string query = "select Representative_name,Designation,Phone_no,Email,RepTitle,RepLastName from tbl_representative where Copany_Id=@Copany_Id";
            SqlParameter[] pram = {
                new SqlParameter("@Copany_Id",clientId),
            };
            dtrep = DbCL.SPreturn_dt(query, pram);
            if (dtrep.Rows.Count > 0)
            {
                PanelRep.Visible = true;
                GridRep.DataSource = dtrep;
                ViewState["dt"] = dtrep;
                GridRep.DataBind();
            }
        }

        private void buindPrimaryServicewithQno(string clientId, string quotation_no)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            string cmdstring = "select count(*) from tbl_QutPrimaryService where qut_no='" + quotation_no.ToString() + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            Int32 count = Convert.ToInt32(cmd.ExecuteScalar());
            generatelavel(count, quotation_no);
            DbCL.Conn.Close();




            //string query = "select PrimaryService from tbl_QutPrimaryService where qut_no=@qut_no";
            //SqlParameter[] pram = {
            //    new SqlParameter("@qut_no",quotation_no),
            //};
            //dtPSer = DbCL.SPreturn_dt(query, pram);
            //if (dtPSer.Rows.Count > 0)
            //{
            //    string PrimaryService = "";
            //    for (int i = 0; i < dtPSer.Rows.Count; i++)
            //    {
            //        string Service = dtPSer.Rows[i]["PrimaryService"].ToString();
            //        TextInfo textInfo1 = cultureInfo.TextInfo;
            //        Service = textInfo1.ToTitleCase(Service.ToString().ToLower());
            //        if (i == 0)
            //        {
            //            PrimaryService = Service;
            //        }
            //        else if (i == 1)
            //        {
            //            PrimaryService = PrimaryService + " and " + Service;
            //        }
            //        else
            //        {
            //            PrimaryService = PrimaryService + " , " + Service;
            //        }
            //    }

            //    Session["PrimaryService"] = PrimaryService.ToString();
            //}
        }




        private void generatelavel(int count, string qutno)
        {
            string PrimaryService = "";

            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string service = null;
            int flag = 1;
            string cmdstring = "select PrimaryService from tbl_QutPrimaryService where qut_no='" + qutno.ToString() + "' order by id";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            while (re.Read())
            {
                TextInfo textInfo1 = cultureInfo.TextInfo;

                string name = re["PrimaryService"].ToString();
                name = textInfo1.ToTitleCase(name.ToString().ToLower());
                name = "“" + name + "”";
                if (count == 1 || flag == 1)
                {
                    //service = "“" + re["PrimaryService"].ToString() + "”";
                    service = name;
                    flag = flag + 1;
                }

                else if (flag == count)
                {
                    service = service + " & " + name.ToString();
                    flag = flag + 1;

                }
                else
                {
                    service = service + ", " + name.ToString();
                    flag = flag + 1;
                }
            }
            DbCL.Conn.Close();


            PrimaryService = service.ToString();
            Session["PrimaryService"] = PrimaryService.ToString();

        }

        private string bindqno(string Payment_ID)
        {
            string qno = "";
            string query = "select Invoice_No,Quotation_No from tbl_invoice_payment where Payment_ID=@Payment_ID";
            SqlParameter[] pram = {
                new SqlParameter("@Payment_ID",Payment_ID)
            };
            SqlDataReader rdr = DbCL.SPReturnRdr(query, pram);
            if (rdr.Read())
            {
                qno = rdr["Quotation_No"].ToString();
                Session["Invoice_No"] = rdr["Invoice_No"].ToString();
            }
            return qno;
        }

        protected void btnreset_Click(object sender, EventArgs e)
        {

        }

        protected void BtnSendMail_Click(object sender, EventArgs e)
        {
            DataTable dt1;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            dt1 = (DataTable)ViewState["dt"];
            if (dt1 != null)
            {
                Int32 i;
                for (i = 0; i <= dt1.Rows.Count - 1; i++)
                {
                    CheckBox chk = (CheckBox)(GridRep.Rows[i].FindControl("chk"));
                    if (chk.Checked == true)
                    {
                        string LastName = ((Label)GridRep.Rows[i].FindControl("re_lname")).Text;
                        string RepTitle = ((Label)GridRep.Rows[i].FindControl("re_tilal")).Text;
                        string RepEmai = ((Label)GridRep.Rows[i].FindControl("re_email")).Text;
                        //string RepEmai = "advisory@aminruptechnologies.co.in";
                        //SendMail(RepEmai, RepTitle, LastName);

                        string status = "Yes";
                        DateTime now = DateTime.Now;
                        string date1 = (now.ToString("dd")) + "-" + (now.ToString("MM")) + "-" + (now.ToString("yyyy"));

                        //DbCL.executeRdr("Update tbl_Proforma set mailStatus='" + status + "',mail_Date='" + date1 + "' where Invoice_No='" + Session["Invoice_No"].ToString() + "'");
                        DbCL.executeRdr("Update tbl_invoice_payment set mailStatus='" + status + "',mailDate='" + date1 + "' where Payment_ID='" + Session["Payment_ID"].ToString() + "'");

                    }
                }
            }

            DbCL.Conn.Close();

            PanelOK.Visible = true;
            lblOk.Text = "Email Send Successfully.....";
            DataList1.Visible = false;
            GridRep.Visible = false;
        }

        private bool SendMail(string repEmai, string repTitle, string lastName)
        {
            try
            {
                StringBuilder mad = new StringBuilder();

                string cname = Session["client"].ToString();

                SmtpClient smtpClient = new SmtpClient();
                MailMessage message = new MailMessage();
                MailAddress fromAddress = new MailAddress("info@aminruptechnologies.co.in", "Aminrup Technologies");
                smtpClient.Host = "199.79.63.186";

                smtpClient.Port = 25;
                smtpClient.EnableSsl = false;
                smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                smtpClient.Credentials = new NetworkCredential("info@aminruptechnologies.co.in", "drL^x089");
                smtpClient.Timeout = 20000;

                message.From = fromAddress;
                message.To.Add(repEmai);

                //Session["Net_amount"] = dypay.Rows[0]["Net_amount"].ToString();
                //Session["Given_amount"] = dypay.Rows[0]["Given_amount"].ToString();
                //Session["type"] = dypay.Rows[0]["type"].ToString();
                //Session["Ch_no"] = dypay.Rows[0]["Ch_no"].ToString();
                //Session["PayInvoice_No"] = dypay.Rows[0]["Invoice_No"].ToString();
                //Session["Ch_bank"] = dypay.Rows[0]["Ch_bank"].ToString();
                //Session["Ch_date"] = dypay.Rows[0]["Ch_date"].ToString();
                //string sb = "";

                if (Session["Due_amount"].ToString() == "0.00")
                {
                    string s = "http://i2isoft.aminruptechnologies.co.in/corporate/business/print/NewPaymentInvoice.aspx?Payment_ID=" + Session["Payment_ID"].ToString();
                    
                    message.Subject = "Acknowledgement for the Receipt of " + Session["Given_amount"].ToString() + " against Tax Invoice Number " + Session["InvoiceNo"].ToString() + " for " + Session["PrimaryService"].ToString() + " at " + cname + " ";
                    //sb = "We are pleased to acknowledge the receipt of the Full & Final Amount of " + Session["Net_amount"].ToString() + " against " + Session["type"].ToString() + " " + Session["Ch_date"].ToString() + " issued from " + Session["Ch_bank"].ToString() + " against the " + Session["PrimaryService"].ToString() + " at " + cname + " ";

                    

                    string dear = "Dear ";
                    string mail_head = dear + "" + repTitle + " " + lastName + ",";


                    mad.Append("<html><table style='width:100%;'>");
                    mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>" + mail_head.ToString() + "</span></td></tr>");
                    mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>Thank you for providing Aminrup Technologies an opportunity to serve you.</span></td></tr>");
                    // mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>" + mailstr + "</span></td></tr>");

                    if (Session["type"].ToString() == "Cash")
                    {
                        mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>We are pleased to acknowledge the receipt of Full & Final Amount of Rs " + Session["Given_amount"].ToString() + " in Cash from your Organization, " + Session["Ch_date"].ToString() + " against Tax Invoice Number " + Session["InvoiceNo"].ToString() + " for " + Session["PrimaryService"].ToString() + ".</span></td></tr>");

                    }
                    else if (Session["type"].ToString() == "Cheque")
                    {
                        mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>We are pleased to acknowledge the receipt of Full & Final Amount of Rs " + Session["Given_amount"].ToString() + " against Cheque Number " + Session["Ch_no"].ToString() + " " + Session["Ch_date"].ToString() + " issued from " + Session["Ch_bank"].ToString() + " against Tax Invoice Number " + Session["InvoiceNo"].ToString() + " for " + Session["PrimaryService"].ToString() + " from " + cname + ".</span></td></tr>");

                    }
                    else if (Session["type"].ToString() == "DD")
                    {
                        mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>We are pleased to acknowledge the receipt of Full & Final Amount of Rs. " + Session["Given_amount"].ToString() + " against Banker’s Cheque Number " + Session["Ch_no"].ToString() + " " + Session["Ch_date"].ToString() + " issued from " + Session["Ch_bank"] + " against Tax Invoice Number " + Session["InvoiceNo"].ToString() + " for " + Session["PrimaryService"].ToString() + " " + cname + ".</span></td></tr>");

                    }
                    else if (Session["type"].ToString() == "Online Fund Transfer")
                    {
                        mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>We are pleased to acknowledge the receipt of Full & Final Amount of Rs. " + Session["Given_amount"].ToString() + " from your Bank Account against Online Fund Transfer, Transaction Reference ID " + Session["Ch_no"].ToString() + " " + Session["Ch_date"].ToString() + " against Tax Invoice Number " + Session["InvoiceNo"].ToString() + " for " + Session["PrimaryService"].ToString() + " from " + cname + ".</span></td></tr>");

                    }

                    mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>Please Click on the Link “" + s.ToString() + "” to Open & View the Payment Invoice((Payment Invoice Number: " + Session["InvoiceNo"].ToString() + ") with the detailed description of the Payments received by Aminrup Technologies from " + cname + ". In case if you wish to convert the Web Page to PDF Format, Please Click on the “Control + P” Keys on your Computer Keyboard and select the “PDF” or “Microsoft Print to PDF” Option whichever is available in the “Print Dialogue Box” and Click “OK” to Save the File in PDF Format on your System.</td></tr>");
                    mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>We greatly appreciate your Prompt Payment against the raised Tax Invoice and look forward to serving you at all times.</span></td></tr>");
                    mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>Should you have any questions, please do not hesitate to contact us.</span></td></tr>");
                    mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>Thanks & Regards,</span></td></tr>");
                    mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>Mr. Avijit Das<br>CEO <br>Aminrup Technologies<br>Tel: +91 91 9674897316<br></span><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold; text-decoration:none'><a href='#' style='text-decoration:none;color:#c8152a;'>E-mail: info@aminruptechnologies.co.in</a></span><br><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold; text-decoration:none'><a href='' style='text-decoration:none;color:#c8152a;'>URL: www.aminruptechnologies.co.in</a></span></td></tr>");
                    mad.Append("</table></html>");

                }
                else
                {
                    message.Subject = "Acknowledgement for the Receipt of Advance Payment of " + Session["Given_amount"].ToString() + " against Quotation Number " + Session["Quotation_No"].ToString() + " for " + Session["PrimaryService"].ToString() + " at " + cname + " ";
                   // sb = "We are pleased to acknowledge the receipt of Advance Payment of " + Session["Given_amount"].ToString() + " against " + Session["type"].ToString() + " " + Session["Ch_date"].ToString() + " issued from " + Session["Ch_bank"].ToString() + " Quotation Number " + Session["Quotation_No"].ToString() + " for " + Session["PrimaryService"].ToString() + " at " + cname + ".";
                   
                    string dear = "Dear ";
                    string mail_head = dear + "" + repTitle + " " + lastName + ",";


                    mad.Append("<html><table style='width:100%;'>");
                    mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>" + mail_head.ToString() + "</span></td></tr>");
                    mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>Thank you for providing Aminrup Technologies an opportunity to serve you.</span></td></tr>");
                   // mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>" + mailstr + "</span></td></tr>");

                    if (Session["type"].ToString() == "Cash")
                    {
                        mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>We are pleased to acknowledge the receipt Advance Payment of Rs " + Session["Given_amount"].ToString() + " in Cash from your Organization, " + Session["Ch_date"].ToString() + " against Quotation Number " + Session["Quotation_No"].ToString() + " for " + Session["PrimaryService"].ToString() + ".</span></td></tr>");
                       
                    }
                    else if (Session["type"].ToString() == "Cheque")
                    {
                        mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>We are pleased to acknowledge the receipt of Advance Payment of Rs " + Session["Given_amount"].ToString() + " against Cheque Number " + Session["Ch_no"].ToString() + " " + Session["Ch_date"].ToString() + " issued from " + Session["Ch_bank"].ToString() + " against Quotation Number " + Session["Quotation_No"].ToString() + " for " + Session["PrimaryService"].ToString() + " from " + cname + ".</span></td></tr>");
                        
                    }
                    else if (Session["type"].ToString() == "DD")
                    {
                        mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>We are pleased to acknowledge the receipt of Advance Payment of Rs. " + Session["Given_amount"].ToString() + " against Banker’s Cheque Number " + Session["Ch_no"].ToString() + " " + Session["Ch_date"].ToString() + " issued from " + Session["Ch_bank"] + " against Quotation Number " + Session["Quotation_No"].ToString() + " for " + Session["PrimaryService"].ToString() + " " + cname + ".</span></td></tr>");
                        
                    }
                    else if (Session["type"].ToString() == "Online Fund Transfer")
                    {
                        mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>We are pleased to acknowledge the receipt of Advance Payment of Rs. " + Session["Given_amount"].ToString() + " from your Bank Account against Online Fund Transfer, Transaction Reference ID " + Session["Ch_no"].ToString() + " " + Session["Ch_date"].ToString() + " against Quotation Number " + Session["Quotation_No"].ToString() + " for " + Session["PrimaryService"].ToString() + " from " + cname + ".</span></td></tr>");
                       
                    }

                    mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>We greatly appreciate your Prompt Payment against the raised Quotation and look forward to serving you at all times.</span></td></tr>");
                    mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>Should you have any questions, please do not hesitate to contact us.</span></td></tr>");
                    mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>Thanks & Regards,</span></td></tr>");
                    mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>Mr. Avijit Das<br>CEO <br>Aminrup Technologies<br>Tel: +91 91 9674897316<br></span><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold; text-decoration:none'><a href='#' style='text-decoration:none;color:#c8152a;'>E-mail: info@aminruptechnologies.co.in</a></span><br><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold; text-decoration:none'><a href='' style='text-decoration:none;color:#c8152a;'>URL: www.aminruptechnologies.co.in</a></span></td></tr>");
                    mad.Append("</table></html>");
                }


               

                message.Bcc.Add(new MailAddress("info@aminruptechnologies.co.in"));
                message.Bcc.Add(new MailAddress("advisory@aminruptechnologies.co.in"));
                message.Bcc.Add(new MailAddress("info@aminruptechnologies.co.in"));
                message.Bcc.Add(new MailAddress("akansha@aminruptechnologies.co.in"));
                message.Bcc.Add(new MailAddress("support@aminruptechnologies.co.in"));


                message.IsBodyHtml = true;
                message.Body = mad.ToString();
                smtpClient.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                string error = "Send Email Failed." + ex.Message;
                return false;
            }
        }
    }
}