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

    public partial class WebForm26 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dtProduct = new DataTable();
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
                txtinvoiceDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            }

        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                BuindCompanyId();
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.Client_Id='" + lblclientId.Text + "' order by tbl_Quotation.ID desc";

                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Client_Id='" + lblclientId.Text + "' and tbl_Quotation.Status2='No' order by tbl_Quotation.ID desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Status2='No' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by tbl_Quotation.ID desc";
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by tbl_Quotation.ID desc";

                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.Client_Id='" + lblclientId.Text + "' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by tbl_Quotation.ID desc";

                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Status2='No' and tbl_Quotation.Client_Id='" + lblclientId.Text + "' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by tbl_Quotation.ID desc";
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
            Response.Redirect("~/corporate/business/app/Add_invoice.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Quotation_no = Convert.ToString(e.CommandArgument);

            if (e.CommandName == "Select")
            {
                Panel1.Visible = true;
                Binddetails(Quotation_no);
                BindAllProduct(Quotation_no);

                DataList1.Visible = false;
            }
        }

        private void BindAllProduct(string quotation_no)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Id,Sl_no,Quotation_no,Product_id,Product_name,Quantity,sail_rate,Service_tax_rate,Total_sail_rate,Total_sail_rate1,Total_sail_rate2,purchess_rate,specification,InvStatus from tbl_Quotaion_details where Quotation_no=@quotation_no order by id";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlParameter[] pram = {
                new SqlParameter("@Quotation_no",quotation_no),
            };
            dtProduct = DbCL.SPreturn_dt(cmdstring, pram);

            if (dtProduct.Rows.Count > 0)
            {
                Gridview_Product.DataSource = dtProduct;
                Gridview_Product.DataBind();
                ViewState["dt"] = dtProduct;
            }
            DbCL.Conn.Close();
        }

        private void Binddetails(string Quotation_no)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Quotation where Quotation_no='" + Quotation_no.ToString() + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblClient_Id.Text = re["Client_Id"].ToString();
                lblQuotation_no.Text = re["Quotation_no"].ToString();
                lblQuotation_date.Text = re["Quotation_date"].ToString();

                lblGross_amount.Text = re["Gross"].ToString();
                lblservicetax.Text = re["Service_tax"].ToString();
                lblNet_amount.Text = re["Net_amount"].ToString();
                lblservicetax0.Text = re["service_tax1"].ToString();
                lblsubtotal.Text = re["sub_total"].ToString();
                string clientcode = lblClient_Id.Text;
                bindFactoryAddress(clientcode);
            }
            DbCL.Conn.Close();
            BindclientName();
            cmbaddressfor.Items.Add("Corporate office");
            DbCL.FillCombo10(cmbaddressfor, "select Factory_name from tbl_Factory where Client_id='" + lblClient_Id.Text + "' order by Factory_name");

            //BindInvoiceNo();
        }

        private void bindFactoryAddress(string clientcode)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Address1+', '+City+', '+pin+', '+State from tbl_Client where Client_Id='" + clientcode + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            SqlDataReader DR1 = cmd.ExecuteReader();
            while (DR1.Read())
            {
                FactoryAddress.Items.Add(DR1.GetValue(0).ToString());
            }
            DbCL.Conn.Close();
            bindRegAddress(clientcode);
            bindAddress(clientcode);
        }

        private void bindRegAddress(string clientcode)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Address+', '+State+', '+City+', '+pin as regadd from tbl_ClientRegAddress where Client_Id='" + clientcode + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            SqlDataReader DR1 = cmd.ExecuteReader();
            while (DR1.Read())
            {
                FactoryAddress.Items.Add(DR1.GetValue(0).ToString());
            }
            DbCL.Conn.Close();
        }

        private void bindAddress(string clientcode)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select [Address1] +', '+ [Address2]+', '+[city]+', '+[State]+', '+[pin] as address from tbl_Factory where Client_id='" + clientcode + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            SqlDataReader DR1 = cmd.ExecuteReader();
            while (DR1.Read())
            {
                FactoryAddress.Items.Add(DR1.GetValue(0).ToString());
            }
            DbCL.Conn.Close();
        }

        private string BindInvoiceNo()
        {
            //string p = null;
            string c = lblClientName.Text.Trim();
            string f = c.Substring(0, 1);
            //string tt;
            //for (int i = 0; i < c.Length; i++)
            //{
            //    p = c.Substring(i, 1);
            //    if (p == " ")
            //    {
            //        tt = c.Substring((i + 1), 1);
            //        if (tt == "(")
            //        {
            //            tt = c.Substring((i + 2), 1);
            //        }
            //        f = f + tt;
            //    }
            //}
            f = "INV/" + f + "/";
            string ss = findmonth();
            f = f + ss;
            int j = idreturn();
            j = j + 1;
            f = f + j.ToString();
            return f;
        }

        private int idreturn()
        {
            string a = null;
            int b = 0;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string date1 = txtinvoiceDate.Text;
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
            string cmdstring = "select Sl_no from tbl_Invoice where ID=(select max(ID) from tbl_Invoice where cast(Invoice_Date as datetime) between '" + date5.ToString() + "' and '" + date6.ToString() + "')";
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
            string a = txtinvoiceDate.Text.Substring(3, 3);
            string b = txtinvoiceDate.Text.Substring(9, 2);
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

        private void BindclientName()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Client_Name from tbl_Client where Client_Id='" + lblClient_Id.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lblClientName.Text = re["Client_Name"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            if (FactoryAddress.GetSelectedIndices().Length > 0)
            {
                string stock_check = findstock();
                if (stock_check == "Yes")
                {
                    string quno = lblQuotation_no.Text;
                    string dueamount = bindpaymentDetails(quno);
                    if (dueamount == "0.00")
                    {
                        string invoice_no = BindInvoiceNo();
                        int j = idreturn();
                        j = j + 1;

                        InsertSelectedProduct(invoice_no, quno);

                        Session["NetAmount"] = Math.Round(Convert.ToDouble(Session["InvTotalAmountWithGst"])) - Convert.ToDouble(txtDiscount.Text);

                        //DbCL.executeRdr("insert into tbl_Invoice(Invoice_No,Invoice_Date,Quotation_No,Quotation_Date,Client_ID,Gross,Service_Tax,Net_Amount,Sl_no,Service_Tax1,sub_total,discount,addressfor,status1,status2)values('" + invoice_no.ToString() + "','" + txtinvoiceDate.Text + "','" + lblQuotation_no.Text + "','" + lblQuotation_date.Text + "','" + lblClient_Id.Text + "','" + lblGross_amount.Text + "','" + lblservicetax.Text + "','" + Net_amount + "','" + j.ToString() + "','" + lblservicetax0.Text + "','" + lblsubtotal.Text + "','" + txtDiscount.Text + "','" + cmbaddressfor.Text + "','No','Active')");

                        DbCL.executeRdr("insert into tbl_Invoice(Invoice_No,Invoice_Date,Quotation_No,Quotation_Date,Client_ID,Gross,Net_Amount,Sl_no,Service_Tax1,sub_total,discount,addressfor,status1,status2)values('" + invoice_no.ToString() + "','" + txtinvoiceDate.Text + "','" + lblQuotation_no.Text + "','" + lblQuotation_date.Text + "','" + lblClient_Id.Text + "','" + Session["InvTotalAmountWithGst"].ToString() + "','" + Session["NetAmount"].ToString() + "','" + j.ToString() + "','" + Session["invTotalGstAmount"].ToString() + "','" + Session["InvTotalAmountWithOutGst"].ToString() + "','" + txtDiscount.Text + "','" + cmbaddressfor.Text + "','No','Active')");

                        //DbCL.executeRdr("UPDATE Table_A SET Table_A.Invoice_No = Table_B.Invoice_No FROM tbl_invoice_payment AS Table_A INNER JOIN tbl_Invoice AS Table_B ON Table_A.Quotation_no = Table_B.Quotation_No and Table_A.Invoice_No IS NULL and Due_amount='0.00'");

                        //double totalQutamount = 0;

                        double totalQuotationValue = Convert.ToDouble(lblGross_amount.Text);

                        //ChecktotalInvAmount(out totalQutamount, quno);

                        //if (totalQutamount == totalQuotationValue)
                        //{

                        //}

                        DbCL.executeRdr("update tbl_Quotation set Status2='Yes' where Quotation_no='"+ lblQuotation_no.Text +"'");
                        updatestock();

                        insertCorRegFacAddress(invoice_no);

                        PanelOK.Visible = true;
                        lblOk.Text = "Data Save Successfull...";
                    }
                    else
                    {
                        PanelError.Visible = true;
                        lblErrorMsg.Text = "Previous Invoice Payment Not Received...";
                    }
                    Button1.Visible = false;
                }
                else
                {
                    PanelError.Visible = true;
                    lblErrorMsg.Text = "You don't have suffiecient stock....";
                }
            }
            else
            {
                PanelError.Visible = true;
                lblErrorMsg.Text = "Please Select Delivery Address....";
            }
        }

        private void insertCorRegFacAddress(string invoice_no)
        {
            int selectedSite = 0;

            string listsite_details = null;
            int slno22 = 1;
            for (int i = 0; i < FactoryAddress.Items.Count; i++)
            {
                if (FactoryAddress.Items[i].Selected)
                {
                    selectedSite = selectedSite + 1;
                    listsite_details = FactoryAddress.Items[i].Text;

                    string query = "insert into tbl_InvSiteAddress(invoice_no,SiteAddress) values (@invoice_no,@SiteAddress)";
                    SqlParameter[] pram = {
                         new SqlParameter("@invoice_no",invoice_no),
                         new SqlParameter("@SiteAddress",listsite_details)
                    };

                    DbCL.SPExecDB(query, pram);
                    slno22 = slno22 + 1;
                }
            }
        }

        private string bindpaymentDetails(string quno)
        {
            string due = "";
            string query = "select Due_amount from tbl_invoice_payment where Quotation_No=@Quotation_No";
            SqlParameter[] pram = {
                new SqlParameter("@Quotation_No",quno)
            };
            SqlDataReader rdr = DbCL.SPReturnRdr(query, pram);
            while (rdr.Read())
            {
                due = rdr["Due_amount"].ToString();
            }
            return due;
        }

        private void ChecktotalInvAmount(out double totalQutamount, string quno)
        {
            string query = "select sum(cast(Total_sail_rate1 as real)) as totalQutamount from tbl_Invoice_details where Quotation_no=@Quotation_no";
            SqlParameter[] pram = {
                new SqlParameter("@Quotation_no",quno)
            };
            SqlDataReader rdr = DbCL.SPReturnRdr(query, pram);
            if (rdr.Read())
            {
                totalQutamount = Math.Round(Convert.ToDouble(rdr["totalQutamount"]), 2);

                //totalinvamount = Convert.ToDouble(rdr["totalinvvalue"]);
            }
            else
            {
                totalQutamount = 0;
            }
        }

        //private void ChecktotalInvAmount(out double totalinvamount,string quno)
        //{
        //    string query = "select sum(cast(Gross as decimal)) from tbl_Invoice where  Quotation_No='I2I/COPL/14-15/7'";

        //}

        private void InsertSelectedProduct(string invoice_no, string quno)
        {

            double InvTotalAmountWithGst = 0;
            double InvTotalAmountWithOutGst = 0;
            double invTotalGstAmount = 0;

            DataTable dt1;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            dt1 = (DataTable)ViewState["dt"];
            if (dt1 != null)
            {
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    CheckBox chk = (CheckBox)(Gridview_Product.Rows[i].FindControl("chk"));
                    if (chk.Checked == true)
                    {
                        try
                        {
                            string ProductCode = ((Label)Gridview_Product.Rows[i].FindControl("Product_id")).Text;
                            string ProductName = ((Label)Gridview_Product.Rows[i].FindControl("Product_name")).Text;
                            string Quantity = ((Label)Gridview_Product.Rows[i].FindControl("Quantity")).Text;
                            string SailRate = ((Label)Gridview_Product.Rows[i].FindControl("sail_rate")).Text;
                            string GstPercentage = ((Label)Gridview_Product.Rows[i].FindControl("Service_tax_rate")).Text;
                            string AmountWithGst = ((Label)Gridview_Product.Rows[i].FindControl("Total_sail_rate1")).Text;
                            string specifai = ((Label)Gridview_Product.Rows[i].FindControl("specification")).Text;
                            string AmountWithOutGst = ((Label)Gridview_Product.Rows[i].FindControl("Total_sail_rate2")).Text;
                            string InvStatus = ((Label)Gridview_Product.Rows[i].FindControl("InvStatus")).Text;
                            //b = Math.Round(b, 2);
                            if (InvStatus != "Yes")
                            {
                                InvTotalAmountWithGst = InvTotalAmountWithGst + Convert.ToDouble(AmountWithGst);
                                InvTotalAmountWithOutGst = InvTotalAmountWithOutGst + Convert.ToDouble(AmountWithOutGst);
                                invTotalGstAmount = invTotalGstAmount + (InvTotalAmountWithGst - InvTotalAmountWithOutGst);

                                Session["InvTotalAmountWithGst"] = InvTotalAmountWithGst;
                                Session["InvTotalAmountWithOutGst"] = InvTotalAmountWithOutGst;
                                Session["invTotalGstAmount"] = invTotalGstAmount;

                                string query = "insert into tbl_Invoice_details(Quotation_no,Invoice_No,Product_id,Product_name,Quantity,sail_rate,Service_tax_rate,Total_sail_rate1,Total_sail_rate2,specification) values (@Quotation_no,@Invoice_No,@Product_id,@Product_name,@Quantity,@sail_rate,@Service_tax_rate,@Total_sail_rate1,@Total_sail_rate2,@specification)";
                                SqlParameter[] pram = {
                                new SqlParameter("@Quotation_no",quno),
                                new SqlParameter("@Invoice_No",invoice_no),
                                new SqlParameter("@Product_id",ProductCode),
                                new SqlParameter("@Product_name",ProductName),
                                new SqlParameter("@Quantity",Quantity),
                                new SqlParameter("@sail_rate",SailRate),
                                new SqlParameter("@Service_tax_rate",GstPercentage),
                                new SqlParameter("@Total_sail_rate1",AmountWithGst),
                                new SqlParameter("@specification",specifai),
                                new SqlParameter("@Total_sail_rate2",AmountWithOutGst)
                            };
                                DbCL.SPExecDB(query, pram);

                                updateqtableforproduct(quno, ProductCode, ProductName);
                            }
                        }
                        catch (Exception ex)
                        {
                            string message = ex.ToString();
                        }

                    }
                }
            }
        }

        private void updateqtableforproduct(string quno, string productCode, string productName)
        {
            string query = "update tbl_Quotaion_details set InvStatus=@InvStatus where Quotation_no=@Quotation_no and Product_id=@Product_id and Product_name=@Product_name";
            SqlParameter[] pram =
                {
                   new SqlParameter("@InvStatus","Yes"),
                   new SqlParameter("@Quotation_no",quno),
                   new SqlParameter("@Product_id",productCode),
                   new SqlParameter("@Product_name",productName)
                };
            DbCL.SPExecDB(query, pram);
        }

        private void updatestock()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Product_id,Product_name,Quantity from tbl_Quotaion_details where Quotation_no='" + lblQuotation_no.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            while (re.Read())
            {
                string product_code = re["Product_id"].ToString();
                string Product_name = re["Product_name"].ToString();
                string Quantity = re["Quantity"].ToString();
                updatestock1(product_code, Product_name, Quantity);
            }
            DbCL.Conn.Close();
        }

        private void updatestock1(string product_code, string Product_name, string Quantity)
        {
            DbCL.executeRdr("update tbl_stock set Quantity=(cast(Quantity as int)-'" + Quantity.ToString() + "') where Product_id='" + product_code.ToString() + "' and Product_name='" + Product_name.ToString() + "'");
        }

        private string findstock()
        {
            string stock = "Yes";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Quotaion_details where Quotation_no='" + lblQuotation_no.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            while (re.Read())
            {
                string product_code = re["Product_id"].ToString();
                string product_Name = re["Product_name"].ToString();
                string Quantity = re["Quantity"].ToString();
                string status = findstock1(product_code, product_Name, Quantity);
                if (status == "No")
                {
                    stock = "No";
                }
            }
            DbCL.Conn.Close();
            return stock;

        }

        //private string findstock1(string product_code, string product_Name, string Quantity)
        //{
        //    string stock = "Yes";
        //    string Qt;
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    string cmdstring = "select Quantity from tbl_stock where Product_id='" + product_code + "' and Product_name='" + product_Name + "'";
        //    SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
        //    SqlDataReader re = cmd.ExecuteReader();
        //    if (re.Read())
        //    {
        //        Qt = re["Quantity"].ToString();
        //    }
        //    else
        //    {
        //        Qt = "0";
        //    }
        //    int Qt1 = Convert.ToInt32(Qt);
        //    int Qt2 = Convert.ToInt32(Quantity);
        //    if (Qt1 >= Qt2)
        //    {
        //        stock = "Yes";
        //    }
        //    else
        //    {
        //        stock = "No";
        //    }
        //    DbCL.Conn.Close();
        //    return stock;

        //}

        private string findstock1(string product_code, string product_Name, string Quantity)
        {
            string stock = "No"; // Default to "No"
            int availableQuantity = 0;

            try
            {
                DbCL.Sqlconnection();
                DbCL.ConnectDb();

                string cmdstring = "SELECT Quantity FROM tbl_stock WHERE Product_id = @ProductId AND Product_name = @ProductName";
                using (SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn))
                {
                    cmd.Parameters.AddWithValue("@ProductId", product_code ?? string.Empty);
                    cmd.Parameters.AddWithValue("@ProductName", product_Name ?? string.Empty);

                    using (SqlDataReader re = cmd.ExecuteReader())
                    {
                        if (re.Read())
                        {
                            int qty = 0;
                            // Ensure Quantity is not null or empty, default to 0
                            availableQuantity = re["Quantity"] != DBNull.Value && int.TryParse(re["Quantity"].ToString(), out qty) ? qty : 0;
                        }
                    }
                }
                int qt2 = 0;
                // Ensure Quantity input is treated correctly
                int requestedQuantity = int.TryParse(Quantity, out qt2) ? qt2 : 0;

                if (availableQuantity >= requestedQuantity)
                {
                    stock = "Yes";
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                //Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                if (DbCL.Conn != null && DbCL.Conn.State == ConnectionState.Open)
                {
                    DbCL.Conn.Close();
                }
            }

            return stock;
        }

    }
}