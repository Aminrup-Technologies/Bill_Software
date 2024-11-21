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
    public partial class WebForm82 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dtrep = new DataTable();
        DataTable dtPSer = new DataTable();
        DataTable dtClient = new DataTable();
        DataTable dtmain = new DataTable();
        CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;

        //StringBuilder mad = new StringBuilder();
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
        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Quotation_no = Convert.ToString(e.CommandArgument);
            string ID = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "View")
            {
                string qdate = buindalldata(ID);

                DateTime fromdate = DateTime.Parse(Convert.ToDateTime(qdate).ToShortDateString());
                DateTime todate = DateTime.Parse(Convert.ToDateTime("12-Jun-2018").ToShortDateString());
                if (fromdate > todate)
                {
                    Response.Redirect("/corporate/business/print/NewQuotation.aspx?ID=" + ID);
                }
                else
                {
                    Response.Redirect("/corporate/business/print/Quotation.aspx?ID=" + ID);
                }
                //string url = "/corporate/business/print/NewQuotation.aspx?ID=" + ID;
                //Response.Write("<script type='text/javascript'>window.open('" + url + "');</script>");
            }

            if (e.CommandName == "Select")
            {
                string ClientId = lblclientId.Text;
                Session["Quotation_no"] = Quotation_no;
                bindQid(Quotation_no);
                buindPrimaryServicewithQno(ClientId, Quotation_no);
                bindRepDetails(ClientId, Quotation_no);
                bindClientDetails(ClientId, Quotation_no);
            }
        }

        private string buindalldata(string iD)
        {
            string qdate = "";
            string query = "select Quotation_no,Quotation_date,Client_Id,sub_total,Service_tax,Net_amount,cgstOrsgst,igst from tbl_Quotation where ID=@ID";
            SqlParameter[] pram = {
            new SqlParameter("@id",ID)
            };
            dtmain = DbCL.SPreturn_dt(query, pram);
            if (dtmain.Rows.Count > 0)
            {
                string qutno = dtmain.Rows[0]["Quotation_no"].ToString();

                qdate = dtmain.Rows[0]["Quotation_date"].ToString();

            }
            return qdate;
        }

        private void bindQid(string quotation_no)
        {
            string query = "select ID,Quotation_no from tbl_Quotation where Quotation_no=@Quotation_no";
            SqlParameter[] pram = {
                new SqlParameter("@Quotation_no",quotation_no)
            };
            SqlDataReader rdr = DbCL.SPReturnRdr(query, pram);
            if (rdr.Read())
            {
                Session["ID"] = rdr["ID"].ToString();
            }
        }

        private void bindClientDetails(string clientId, string quotation_no)
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

        private void bindRepDetails(string clientId, string quotation_no)
        {
            string query = "select Representative_name,Designation,Phone_no,Email,RepTitle,RepLastName from tbl_representative where Copany_Id=@Copany_Id";
            SqlParameter[] pram = {
                new SqlParameter("@Copany_Id",clientId),
            };
            dtrep = DbCL.SPreturn_dt(query, pram);
            if (dtrep.Rows.Count>0)
            {
                PanelRep.Visible = true;
                GridRep.DataSource = dtrep;
                ViewState["dt"] = dtrep;
                GridRep.DataBind();
            }
        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                BuindCompanyId();
                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Client_Id='" + lblclientId.Text + "' order by cast(tbl_Quotation.Quotation_date as datetime) desc";
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.Client_Id='" + lblclientId.Text + "' order by tbl_Quotation.ID desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(tbl_Quotation.Quotation_date as datetime) desc";
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by tbl_Quotation.ID desc";
                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Client_Id='" + lblclientId.Text + "' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by cast(tbl_Quotation.Quotation_date as datetime) desc";
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
            Response.Redirect("~/corporate/business/app/Set_quatation.aspx");
        }
            
        protected void BtnGivePermition_Click(object sender, EventArgs e)
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

                        DbCL.executeRdr("Update tbl_Quotation set mailStatus='" + status + "',mailStatusDate='" + date1 + "' where Quotation_no='" + Session["Quotation_no"].ToString() + "'");

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
                //smtpClient.Host = "103.21.58.28";
                smtpClient.Host = "199.79.63.186";
                smtpClient.Port = 25;
                smtpClient.EnableSsl = false;
                smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                smtpClient.Credentials = new NetworkCredential("info@aminruptechnologies.co.in", "drL^x089");
                smtpClient.Timeout = 20000;

                message.From = fromAddress;
                message.To.Add(repEmai);

                message.Subject = "Quotation for " + Session["PrimaryService"].ToString() + " at " + cname + " ";
                string dear = "Dear ";
                string mail_head = dear + "" + repTitle + " " + lastName + ",";


                //string s = "www.i2isoft.aminruptechnologies.co.in/corporate/business/print/NewQuotation.aspx?qutno_no=" + Session["Quotation_no"].ToString();
                //string s = "http://localhost:1216/corporate/business/print/NewQuotation.aspx?qutno_no=" + Session["Quotation_no"].ToString();
                string s = "http://i2isoft.aminruptechnologies.co.in/corporate/business/print/NewQuotation.aspx?ID=" + Session["ID"].ToString();

                

                mad.Append("<html><table style='width:100%;'>");
                mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>" + mail_head.ToString() + "</span></td></tr>");
                mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>Thank you for showing interest in our Organization.</span></td></tr>");
                mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>We are pleased to offer our Quotation detailing the Technical & Commercial Terms for the " + Session["PrimaryService"].ToString() + " at your Organization.</span></td></tr>");
                mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>Please Click on the Link “" + s.ToString() + "” to Open & View the Quotation (Quotation Number: " + Session["Quotation_no"].ToString() + "). In case if you wish to convert the Web Page to PDF Format, Please Click on the “Control + P” Keys on your Computer Keyboard and select the “PDF” or “Microsoft Print to PDF” Option whichever is available in the “Print Dialogue Box” and Click “OK” to Save the File in PDF Format on your System.</td></tr>");
                mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>A formal Work Order against our Proposed Offer along with the Advance Payment as specified in the Payment Terms will be required to freeze on the Contract.</span></td></tr>");
                mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>We greatly appreciate your patronage and look forward to serving you at all times.</span></td></tr>");
                mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>Should you have any questions, please do not hesitate to contact us.</span></td></tr>");
                mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>Thanks & Regards,</span></td></tr>");
                mad.Append("<tr><td style='padding:5px 0px;'><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold;'>Mr. Avijit Das<br>CEO <br>Aminrup Technologies<br>Tel: +91 91 96748 97316<br></span><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold; text-decoration:none'><a href='#' style='text-decoration:none;color:#c8152a;'>E-mail: info@aminruptechnologies.co.in</a></span><br><span style='text-align:left; color:#c8152a; font:italic normal 12px/15px Century Gothic;font-weight:bold; text-decoration:none'><a href='' style='text-decoration:none;color:#c8152a;'>URL: www.aminruptechnologies.co.in</a></span></td></tr>");
                mad.Append("</table></html>");

                message.Bcc.Add(new MailAddress("info@aminruptechnologies.co.in"));
                message.Bcc.Add(new MailAddress("advisory@aminruptechnologies.co.in"));
                message.Bcc.Add(new MailAddress("info@aminruptechnologies.co.in"));
                message.Bcc.Add(new MailAddress("support@aminruptechnologies.co.in"));
                message.Bcc.Add(new MailAddress("akansha@aminruptechnologies.co.in"));

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