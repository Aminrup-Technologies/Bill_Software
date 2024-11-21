using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Globalization;
using System.Threading;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm84 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dtrep = new DataTable();
        DataTable dtClient = new DataTable();
        DataTable dtPSer = new DataTable();
        CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
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
                //cmdstring = "select  tbl_Proforma.ID,tbl_Proforma.Invoice_No,tbl_Proforma.Invoice_Date,tbl_Proforma.Quotation_No,tbl_Proforma.Quotation_Date,tbl_Proforma.Net_Amount,tbl_Client.Client_Name from tbl_Proforma inner join tbl_Client on tbl_Proforma.Client_ID=tbl_Client.Client_Id where tbl_Proforma.Client_ID='" + lblclientId.Text + "' order by cast(tbl_Proforma.Invoice_Date as datetime) desc";
                cmdstring = "select a.ID,a.Invoice_No,a.Invoice_Date,a.Quotation_No,a.Quotation_Date,a.Net_Amount,a.mail_Date,a.subtotal,(a.Net_Amount-a.subtotal) as Gst,b.Client_Name,c.PServiceName from tbl_Proforma as a left outer join tbl_QuoPriSerTogather as c on a.Quotation_No=c.qutno left outer join tbl_Client as b on b.Client_Id=a.Client_ID where a.Client_ID='" + lblclientId.Text + "' order by a.ID desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                //cmdstring = "select  tbl_Proforma.ID,tbl_Proforma.Invoice_No,tbl_Proforma.Invoice_Date,tbl_Proforma.Quotation_No,tbl_Proforma.Quotation_Date,tbl_Proforma.Net_Amount,tbl_Client.Client_Name from tbl_Proforma inner join tbl_Client on tbl_Proforma.Client_ID=tbl_Client.Client_Id where cast(tbl_Proforma.Invoice_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(tbl_Proforma.Invoice_Date as datetime) desc";
                cmdstring = "select a.ID,a.Invoice_No,a.Invoice_Date,a.Quotation_No,a.Quotation_Date,a.Net_Amount,a.mail_Date,a.subtotal,(a.Net_Amount-a.subtotal) as Gst,b.Client_Name,c.PServiceName from tbl_Proforma as a left outer join tbl_QuoPriSerTogather as c on a.Quotation_No=c.qutno left outer join tbl_Client as b on b.Client_Id=a.Client_ID where cast(a.Invoice_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by a.ID desc";
                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                //cmdstring = "select  tbl_Proforma.ID,tbl_Proforma.Invoice_No,tbl_Proforma.Invoice_Date,tbl_Proforma.Quotation_No,tbl_Proforma.Quotation_Date,tbl_Proforma.Net_Amount,tbl_Client.Client_Name from tbl_Proforma inner join tbl_Client on tbl_Proforma.Client_ID=tbl_Client.Client_Id where tbl_Proforma.Client_ID='" + lblclientId.Text + "' and cast(tbl_Proforma.Invoice_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(tbl_Proforma.Invoice_Date as datetime) desc";
                cmdstring = "select a.ID,a.Invoice_No,a.Invoice_Date,a.Quotation_No,a.Quotation_Date,a.Net_Amount,a.mail_Date,a.subtotal,(a.Net_Amount-a.subtotal) as Gst,b.Client_Name,c.PServiceName from tbl_Proforma as a left outer join tbl_QuoPriSerTogather as c on a.Quotation_No=c.qutno left outer join tbl_Client as b on b.Client_Id=a.Client_ID where a.Client_ID='" + lblclientId.Text + "' and cast(a.Invoice_Date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by a.ID desc";
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
            Response.Redirect("~/corporate/business/app/ProformaMail.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string ID = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Select")
            {
                string ClientId = lblclientId.Text;
                Session["ID"] = ID;
                string Quotation_no = bindqno(ID);
                string Invoice_No=Session["Invoice_No"].ToString();
                buindPrimaryServicewithQno(ClientId, Quotation_no);
                bindRepDetails(ClientId, Invoice_No);
                bindClientDetails(ClientId, Invoice_No);
            }
        }

        private string bindqno(string id)
        {
            string qno = "";
            string query = "select Quotation_No,Invoice_No from tbl_Proforma where ID=@ID";
            SqlParameter[] pram = {
                new SqlParameter("@ID",id)
            };
            SqlDataReader rdr = DbCL.SPReturnRdr(query, pram);
            if (rdr.Read())
            {
                qno = rdr["Quotation_No"].ToString();
                Session["Invoice_No"]= rdr["Invoice_No"].ToString();
            }

            return qno;
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

                        DbCL.executeRdr("Update tbl_Proforma set mailStatus='" + status + "',mail_Date='" + date1 + "' where Invoice_No='" + Session["Invoice_No"].ToString() + "'");

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

                message.Subject = "Proforma Invoice for " + Session["PrimaryService"].ToString() + " at " + cname + " ";
                string dear = "Dear ";
                string mail_head = dear + "" + repTitle + " " + lastName + ",";


                //string s = "www.i2isoft.aminruptechnologies.co.in/corporate/business/print/NewProformaInvoice.aspx?ID=" + Session["ID"].ToString();
                string s = "http://i2isoft.aminruptechnologies.co.in/corporate/business/print/NewProformaInvoice.aspx?ID=" + Session["ID"].ToString();

                //string sd = "http://i2isoft.aminruptechnologies.co.in/corporate/business/print/NewInvoice.aspx?ID=" + Session["ID"].ToString();


                mad.Append("<html><table style='width:100%;'>");
                mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>" + mail_head.ToString() + "</span></td></tr>");
                mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>Thank you for providing Aminrup Technologies an opportunity to serve you.</span></td></tr>");
                mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>We are pleased to offer our Pro Forma Invoice for the  " + Session["PrimaryService"].ToString() + " at your Organization.</span></td></tr>");
                mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>Please Click on the Link “" + s.ToString() + "” to Open & View the Pro Forma Invoice (Invoice Number: " + Session["Invoice_No"].ToString() + "). In case if you wish to convert the Web Page to PDF Format, Please Click on the “Control + P” Keys on your Computer Keyboard and select the “PDF” or “Microsoft Print to PDF” Option whichever is available in the “Print Dialogue Box” and Click “OK” to Save the File in PDF Format on your System.</td></tr>");
                mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>We greatly appreciate your Prompt Payment against the raised Pro Invoice and look forward to serving you at all times.</span></td></tr>");
                mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>Should you have any questions, please do not hesitate to contact us.</span></td></tr>");
                mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>Thanks & Regards,</span></td></tr>");
                mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>Mr. Avijit Das<br>CEO <br>Aminrup Technologies<br>Tel: +91 91 96748 97316<br></span><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold; text-decoration:none'><a href='#' style='text-decoration:none;color:#c8152a;'>E-mail: info@aminruptechnologies.co.in</a></span><br><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold; text-decoration:none'><a href='' style='text-decoration:none;color:#c8152a;'>URL: www.aminruptechnologies.co.in</a></span></td></tr>");
                mad.Append("</table></html>");

                message.Bcc.Add(new MailAddress("info@aminruptechnologies.co.in"));
                message.Bcc.Add(new MailAddress("advisory@aminruptechnologies.co.in"));
                message.Bcc.Add(new MailAddress("Info@aminruptechnologies.co.in"));
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