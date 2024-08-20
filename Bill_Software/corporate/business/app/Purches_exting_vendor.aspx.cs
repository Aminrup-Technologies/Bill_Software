using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm11 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        
        public DataTable first_datatable;
        public static DataTable Dt = new DataTable("Table");
        public static double tota_purchesrate1 = 0;
        public static double total_tax_rate_details = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                
                tota_purchesrate1 = 0;
                total_tax_rate_details = 0;
                Dt = new DataTable("Table");
                DbCL.FillCombo(cmbState, "select State_Name from tbl_State order by State_Name");
                DbCL.FillCombo(cmbcity, "select City_Name from tbl_City order by City_Name");
                DbCL.FillCombo(cmbvendor, "select Vendor_Name from tbl_Vendor order by Vendor_Name");
                txtPurchesDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtcashDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtdddate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtneftdate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txtpaymentdate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Label1.Visible = false;
            RadioButtonList1.Visible = false;
            Button1.Visible = false;
            Panel1.Visible = true;
            BindListitem();
        }

        private void BindListitem()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                cmdstring = "select Product_Name from tbl_Product order by Product_Name";
            }
            else
            {
                cmdstring = "select Service_name  from tbl_Service order by Service_name";
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
                string cmdstring = "select Product_code,Product_Name from tbl_Product where Product_Name='" + cmbproduct_service.Text + "'";
                Binddata1(cmdstring);
            }
            else
            {
                string cmdstring = "select Service_code,Service_name  from tbl_Service where Service_name='" + cmbproduct_service.Text + "'";
                Binddata1(cmdstring);
            }
            cmbproduct_service.SelectedIndex = 0;

            //string listProduct_Service1 = null;
            //for (int i = 0; i <= listProduct_Service.Items.Count - 1; i++)
            //{
            //    if (listProduct_Service.Items[i].Selected)
            //    {

            //        listProduct_Service1 = listProduct_Service.Items[i].Text;
            //        if (RadioButtonList1.SelectedIndex == 0)
            //        {
            //            string cmdstring = "select Product_code,Product_Name,Purches_Rate,Sail_Rate,Tax_Rate from tbl_Product where Product_Name='" + listProduct_Service1.ToString() + "'";
            //            Binddata1(cmdstring);
            //        }
            //        else
            //        {
            //            string cmdstring = "select Service_code,Service_name,Purches_rate,Sail_rate,Tax_rate  from tbl_Service where Service_name='" + listProduct_Service1.ToString() + "'";
            //            Binddata1(cmdstring);
            //        }

            //    }
            //}
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
                if (Label2.Text == "1")
                {
                    newgrid1();
                }
                else
                {
                    newgrid();
                }
                Label2.Text = (Convert.ToInt32(Label2.Text) + 1).ToString();
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
            //DataColumn Vendor_rate = new DataColumn("Vendor_rate", typeof(string));
            //Dt.Columns.Add(Vendor_rate);
            //DataColumn Sale_rate = new DataColumn("Sale_rate", typeof(string));
            //Dt.Columns.Add(Sale_rate);
            //DataColumn service_Tax_Rate = new DataColumn("service_Tax_Rate", typeof(string));
            //Dt.Columns.Add(service_Tax_Rate);

            for (int i = 0; i <= dt.Rows.Count - 1; i++)
            {
                string Ser_pro_code1 = (String)first_datatable.Rows[i][0];
                string Ser_pro_Name1 = (String)first_datatable.Rows[i][1]; ;
                //string Vendor_rate1 = (String)first_datatable.Rows[i][2];
                //string Sale_rate1 = (String)first_datatable.Rows[i][3];
                //string service_Tax_Rate1 = (String)first_datatable.Rows[i][4];
                dr = Dt.NewRow();
                dr["Ser_pro_code"] = Ser_pro_code1.ToString();
                dr["Ser_pro_Name"] = Ser_pro_Name1.ToString();
                //dr["Vendor_rate"] = Vendor_rate1.ToString();
                //dr["Sale_rate"] = Sale_rate1.ToString();
                //dr["service_Tax_Rate"] = service_Tax_Rate1.ToString();
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
                string Ser_pro_Name1 = (String)first_datatable.Rows[i][1]; ;
                //string Vendor_rate1 = (String)first_datatable.Rows[i][2];
                //string Sale_rate1 = (String)first_datatable.Rows[i][3];
                //string service_Tax_Rate1 = (String)first_datatable.Rows[i][4];
                dr = Dt.NewRow();
                dr["Ser_pro_code"] = Ser_pro_code1.ToString();
                dr["Ser_pro_Name"] = Ser_pro_Name1.ToString();
                //dr["Vendor_rate"] = Vendor_rate1.ToString();
                //dr["Sale_rate"] = Sale_rate1.ToString();
                //dr["service_Tax_Rate"] = service_Tax_Rate1.ToString();
                Dt.Rows.Add(dr);



            }
        }

        protected void gd_Service_Product_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //DropDownList dp = (DropDownList)e.Row.Cells[6].FindControl("service_Tax_Rate");
                DropDownList dp1 = (DropDownList)e.Row.Cells[4].FindControl("vat_parsentage");


                DbCL.Sqlconnection();

                DbCL.ConnectDb();
                string cmdString = "";
                if (RadioButtonList1.SelectedIndex == 0)
                {
                    cmdString = "Select Vat_Rate from tbl_Vat_Master";
                }
                else
                {
                    cmdString = "Select Service_tax from tbl_Service_master";
                }
                SqlCommand cmd = new SqlCommand(cmdString, DbCL.Conn);

                SqlDataReader Rdr;
                Rdr = cmd.ExecuteReader();
                dp1.Items.Add("NA");
                while (Rdr.Read())
                {
                    dp1.Items.Add(Rdr[0].ToString());
                }

                DbCL.Conn.Close();
                //DbCL.Sqlconnection();

                //DbCL.ConnectDb();
                //string cmdString1 = "Select Service_tax from tbl_Service_master";
                //SqlCommand cmd1 = new SqlCommand(cmdString1, DbCL.Conn);

                //SqlDataReader Rdr1;
                //Rdr1 = cmd1.ExecuteReader();

                //while (Rdr1.Read())
                //{
                //    dp.Items.Add(Rdr1["Service_tax"].ToString());
                //}

                //DbCL.Conn.Close();
            }
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

        protected void Button3_Click(object sender, EventArgs e)
        {
            string purchesid = findpurchesId();
            int i = 0;

            //string dd_chk_no = null;
            //string dd_chk_bank = null;
            //string dd_date = null;
            //if (RadioButtonList2.SelectedIndex == 0)
            //{
            //    dd_chk_no = "";
            //    dd_chk_bank = "";
            //    dd_date = txtcashDate.Text;

            //}
            //else if (RadioButtonList2.SelectedIndex == 3)
            //{
            //    dd_chk_no = txtneftnumber.Text;
            //    dd_chk_bank = txtbankname1.Text;
            //    dd_date = txtneftdate.Text;

            //}
            //else
            //{
            //    dd_chk_no = txtDDno.Text;
            //    dd_chk_bank = txtBankName.Text;
            //    dd_date = txtdddate.Text;

            //}

            DataTable dt1;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();


            dt1 = (DataTable)ViewState["dt"];
            if (dt1 != null)
            {

                for (i = 0; i <= dt1.Rows.Count - 1; i++)
                {




                    SqlTransaction trans = null;
                    SqlConnection conn = null;
                    SqlCommand cmd = null;
                    try
                    {
                        string cnnString = System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ToString();
                        conn = new SqlConnection(cnnString);

                        cmd = new SqlCommand { CommandType = CommandType.Text, Connection = conn };
                        conn.Open();
                        trans = conn.BeginTransaction();
                        cmd.Transaction = trans;
                        int j = i + 1;
                        string Ser_pro_code = ((Label)gd_Service_Product.Rows[i].FindControl("Ser_pro_code")).Text;
                        string Ser_pro_Name = ((Label)gd_Service_Product.Rows[i].FindControl("Ser_pro_Name")).Text;
                        string Vendor_rate = ((TextBox)gd_Service_Product.Rows[i].FindControl("Vendor_rate")).Text;
                        string tax_app = ((RadioButtonList)gd_Service_Product.Rows[i].FindControl("RadioButtonList1")).Text;
                        string vat_parsentage = ((DropDownList)gd_Service_Product.Rows[i].FindControl("vat_parsentage")).Text;
                        string Quantity = ((TextBox)gd_Service_Product.Rows[i].FindControl("Quantity")).Text;
                        string sepecification = ((TextBox)gd_Service_Product.Rows[i].FindControl("sepecification")).Text;
                        //string Sale_rate = ((Label)gd_Service_Product.Rows[i].FindControl("Sale_rate")).Text;
                        //string service_Tax_Rate = ((Label)gd_Service_Product.Rows[i].FindControl("service_Tax_Rate")).Text;
                        string parches_rate = (Convert.ToDouble(Vendor_rate) * Convert.ToDouble(Quantity)).ToString();
                        double tax_rete;
                        if (tax_app == "Yes")
                        {
                            double a = (Convert.ToDouble(parches_rate) * Convert.ToDouble(vat_parsentage)) / 100;
                            //a = Math.Round(a);
                            tax_rete = a;

                        }
                        else
                        {
                            tax_rete = 0;
                        }
                        double parches_rate111 = Convert.ToDouble(tax_rete) + Convert.ToDouble(parches_rate);
                        tota_purchesrate1 = tota_purchesrate1 + Convert.ToDouble(tax_rete) + Convert.ToDouble(parches_rate);
                        //double b = Convert.ToDouble(Sale_rate) * Convert.ToDouble(Quantity);

                        //double total_sail_rate = b;
                        //double c = b * Convert.ToDouble(service_Tax_Rate) / 100;
                        //double total_sail_rate1 = c;
                        //double total_sail_rate2 = (b + c);
                        total_tax_rate_details = total_tax_rate_details + tax_rete;
                        cmd.CommandText = ("insert into tbl_purches_details(sl_no,Purches_id,Product_id,Product_name,vendor_rate,tax_applicable,tax_rate,Quantity,purches_rate,total_purches_rate,vat_amount,specification,Purches_date,Client_id)values('" + j.ToString() + "','" + purchesid + "','" + Ser_pro_code + "','" + Ser_pro_Name + "','" + Vendor_rate + "','" + tax_app + "','" + vat_parsentage + "','" + Quantity + "','" + parches_rate + "','" + parches_rate111 + "','" + tax_rete + "','" + sepecification.ToString() + "','" + txtPurchesDate.Text + "','" + lblvendor_id.Text + "')");
                        cmd.ExecuteNonQuery();
                        //updatestock(Ser_pro_code, Ser_pro_Name, Quantity, Sale_rate, service_Tax_Rate);

                        trans.Commit();
                        conn.Close();

                        trans.Dispose();
                        conn.Dispose();
                        cmd.Dispose();

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
            tota_purchesrate1 = Math.Round(tota_purchesrate1);
            total_tax_rate_details = Math.Round(total_tax_rate_details);
            DbCL.executeRdr("insert into tbl_Purches(Purches_Id,Client_Id,Total_purches_rate,Total_Tax_rate,Purches_date,Purches_Type)values('" + purchesid + "','" + lblvendor_id.Text + "','" + tota_purchesrate1.ToString() + "','" + total_tax_rate_details.ToString() + "','" + txtPurchesDate.Text + "','" + RadioButtonList1.Text + "')");
            DbCL.executeRdr("insert into tbl_purches_due(Purches_Id,Due_amount)values('" + purchesid + "','" + tota_purchesrate1 + "')");
            Button3.Visible = false;
            txtPurchesDate.Enabled = false;
            if (RadioButtonList3.SelectedIndex == 0)
            {
                lblpuechess_id.Text = purchesid.ToString();
                lblpaayment_amount.Text = tota_purchesrate1.ToString();
                Panel3.Visible = true;

            }
            else
            {
                lblOk.Text = "Data Save Successfully.....";
                PanelOK.Visible = true;

            }


        }

        private void updatestock(string Ser_pro_code, string Ser_pro_Name, string Quantity1, string Sale_rate, string service_Tax_Rate)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Product_id from tbl_stock where  Product_id='" + Ser_pro_code + "'";
            SqlCommand cmd10 = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd10.ExecuteReader();
            if (re.Read())
            {
                DbCL.executeRdr("update tbl_stock set Quantity=(cast(Quantity as int)+'" + Quantity1.ToString() + "'),Sail_Rate='" + Sale_rate + "',Service_tax_rate='" + service_Tax_Rate + "' where Product_id='" + Ser_pro_code + "' and Product_name='" + Ser_pro_Name + "'");
            }
            else
            {
                DbCL.executeRdr("insert into tbl_stock(Product_id,Product_name,Quantity,Sail_Rate,Service_tax_rate)values('" + Ser_pro_code + "','" + Ser_pro_Name + "','" + Quantity1 + "','" + Sale_rate + "','" + service_Tax_Rate + "')");
            }
            DbCL.Conn.Close();
        }

        private string findpurchesId()
        {
            string PurID = "";
            string aa = "";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdString1 = "select ID,Purches_Id from tbl_Purches where ID=(select max(ID)from tbl_Purches)";
            SqlCommand com1 = new SqlCommand(cmdString1, DbCL.Conn);
            SqlDataReader DR1 = com1.ExecuteReader();
            if (DR1.Read())
            {
                aa = DR1.GetValue(1).ToString();
                string bb = aa.Substring(4);
                int k = Convert.ToInt32(bb);
                k = k + 1;
                string q = Convert.ToString(k);
                PurID = "PR00" + q;
            }
            else
            {
                PurID = "PR001";
            }

            DbCL.Conn.Close();
            return PurID;
        }

        protected void cmbvendor_SelectedIndexChanged(object sender, EventArgs e)
        {
            Label1.Visible = true;
            RadioButtonList1.Visible = true;
            Button1.Visible = true;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Vendor where Vendor_Name='"+ cmbvendor.Text +"'";
            SqlCommand cmd = new SqlCommand(cmdstring,DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if(re.Read())
            {
                lblvendor_id.Text = re["Vendor_Id"].ToString();
                txtAddress1.Text=re["Address1"].ToString();
                txtAddress2.Text=re["Address2"].ToString();
                cmbcity.Text=re["City"].ToString();
                txtPin.Text=re["pin"].ToString();
                cmbState.Text = re["State"].ToString();
                txtWebsite.Text=re["Com_web_site"].ToString();
                txtEmail.Text=re["Com_email"].ToString();
                txtPhone.Text=re["Com_phone"].ToString();
                txtFax.Text=re["Com_Fax"].ToString();
                txtRepresentativeName.Text=re["Rep_Name"].ToString();
                txtRepresantativeDesig.Text=re["Rep_Desig"].ToString();
                txtRepresentativePhone.Text=re["Rep_phone"].ToString();
                txtRepresentativeEmail.Text=re["Rep_email"].ToString();
                txtservicetaxNo.Text = re["Service_tax_No"].ToString();
                txtpanNo.Text = re["Pan_No"].ToString();
                txtvat.Text = re["Vat_No"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void btnpurchess_save_Click(object sender, EventArgs e)
        {
            if (Convert.ToDouble(lblpaayment_amount.Text) < Convert.ToDouble(txtpaymentamount.Text))
            {
                lblErrorMsg.Text = "Due Amount Is less Than Given Amount...";
                PanelError.Visible = true;
            }
            else
            {
                InserttotalDate();
                PanelOK.Visible = true;
                lblOk.Text = "Data Save Successfully...";
                btnpurchess_save.Visible = false;
                PanelError.Visible = false;

            }

            
        }

        private void InserttotalDate()
        {
            string paymentid = BindpaymentId();

            string comma = ",";
            string dated = " Dated:";
            string date1 = "";
            string no = "";
            string bank = "";
            double due = Convert.ToDouble(lblpaayment_amount.Text) - Convert.ToDouble(txtpaymentamount.Text);
            string due1 = due.ToString();

            if (RadioButtonList2.SelectedIndex == 0)
            {

                date1 = dated + txtcashDate.Text;

            }
            else if (RadioButtonList2.SelectedIndex == 3)
            {
                date1 = dated + txtneftdate.Text;
                no = txtneftnumber.Text + comma;
                bank = txtbankname1.Text;

            }
            else
            {
                date1 = dated + txtdddate.Text;
                no = txtDDno.Text + comma;
                bank = txtBankName.Text;
            }
            string cmdstring = "insert into tbl_Purchess_payment(Payment_ID,Payment_Date,Purchess_ID,Purchess_Date,Client_Id,Net_amount,Given_amount,type,Ch_no,Ch_bank,Ch_date,Due_amount)values(@Payment_ID,@Payment_Date,@Purchess_ID,@Purchess_Date,@Client_Id,@Net_amount,@Given_amount,@type,@Ch_no,@Ch_bank,@Ch_date,@Due_amount)";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 0;
            cmd.Parameters.AddWithValue("@Payment_ID", paymentid.ToString());
            cmd.Parameters.AddWithValue("@Payment_Date", txtpaymentdate.Text);
            cmd.Parameters.AddWithValue("@Purchess_ID", lblpuechess_id.Text);
            cmd.Parameters.AddWithValue("@Purchess_Date", txtPurchesDate.Text);
            cmd.Parameters.AddWithValue("@Client_Id", lblvendor_id.Text);
            cmd.Parameters.AddWithValue("@Net_amount", lblpaayment_amount.Text);

            
            cmd.Parameters.AddWithValue("@Given_amount", txtpaymentamount.Text);
            cmd.Parameters.AddWithValue("@type", RadioButtonList2.Text);
            cmd.Parameters.AddWithValue("@Ch_no", no.ToString());
            cmd.Parameters.AddWithValue("@Ch_bank", bank.ToString());
            cmd.Parameters.AddWithValue("@Ch_date", date1.ToString());
            cmd.Parameters.AddWithValue("@Due_amount", due1.ToString());

            cmd.ExecuteNonQuery();
            DbCL.Conn.Close();
            DbCL.executeRdr("update tbl_purches_due set Due_amount='" + due1.ToString() + "' where Purches_Id='" + lblpuechess_id.Text + "'");
        }
        private string BindpaymentId()
        {
            string paymentIDdetai = "";
            string aa = "";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdString1 = "select ID,Payment_ID from tbl_Purchess_payment where ID=(select max(ID)from tbl_Purchess_payment)";
            SqlCommand com1 = new SqlCommand(cmdString1, DbCL.Conn);
            SqlDataReader DR1 = com1.ExecuteReader();
            if (DR1.Read())
            {
                aa = DR1.GetValue(1).ToString();
                string bb = aa.Substring(3);
                int k = Convert.ToInt32(bb);
                k = k + 1;
                string q = Convert.ToString(k);
                paymentIDdetai = "PN0" + q;
            }
            else
            {
                paymentIDdetai = "PN01";
            }

            DbCL.Conn.Close();
            return paymentIDdetai;
        }
    }
}