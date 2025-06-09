using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm38 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        public static DataTable dt_first;
        DataTable MainDt = new DataTable();
        DataTable dtPCat = new DataTable();

        private int totalQuoted = 0;
        private int totalDelivered = 0;
        private int totalDue = 0;

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
                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Client_Id='" + lblclientId.Text + "'  order by tbl_Quotation.ID desc";
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.DO_Number, tbl_Quotation.PO_Number, tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id = tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no and tbl_QuoPriSerTogather.TimeStamp = tbl_Quotation.TimsStamp where tbl_Quotation.Client_Id = '" + lblclientId.Text + "' ORDER BY CAST(tbl_Quotation.Quotation_date AS datetime) DESC";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where  cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by tbl_Quotation.ID desc";
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total, tbl_Quotation.DO_Number, tbl_Quotation.PO_Number, tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no and tbl_QuoPriSerTogather.TimeStamp = tbl_Quotation.TimsStamp where cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' ORDER BY CAST(tbl_Quotation.Quotation_date AS datetime) DESC";
                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where  tbl_Quotation.Client_Id='" + lblclientId.Text + "' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by tbl_Quotation.ID desc";
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total, tbl_Quotation.DO_Number, tbl_Quotation.PO_Number, tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no and tbl_QuoPriSerTogather.TimeStamp = tbl_Quotation.TimsStamp where tbl_Quotation.Client_Id='" + lblclientId.Text + "' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' ORDER BY CAST(tbl_Quotation.Quotation_date AS datetime) DESC";
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
            Response.Redirect("~/corporate/business/app/add_chalan.aspx");
        }

        //protected void Button1_Click(object sender, EventArgs e)
        //{

        //    if (FactoryAddress.SelectedIndex!=-1)
        //    {


        //    string invoice_no = BindInvoiceNo();
        //    int k = 1;
        //    DataTable dt1;

        //    //dt1 = dt_first;
        //    dt1 =(DataTable) ViewState["ViewQProductData"];
        //    if (dt1 != null)
        //    {
        //        Int32 i;
        //        for (i = 0; i <= dt1.Rows.Count - 1; i++)
        //        {
        //            CheckBox chk = (CheckBox)(gd_Quotation.Rows[i].FindControl("chk"));

        //            if (chk.Checked == true)
        //            {

        //                //string Product_id = ((Label)gd_Quotation.Rows[i].FindControl("Product_id")).Text;
        //                //string Product_name = ((Label)gd_Quotation.Rows[i].FindControl("Product_name")).Text;

        //                string Product_id = ((Label)gd_Quotation.Rows[i].FindControl("Product_code")).Text;
        //                string Product_name = ((Label)gd_Quotation.Rows[i].FindControl("ProductName")).Text;

        //                string Qty = ((TextBox)gd_Quotation.Rows[i].FindControl("Qty")).Text;
        //                int quantity = Convert.ToInt32(Qty);
        //                if (quantity>0)
        //                {
        //                    DbCL.executeRdr("insert into tbl_Challan_details(Sl_no,Challan_no,Product_id,Product_name,Quantity)values('" + k.ToString() + "','" + invoice_no.ToString() + "','" + Product_id + "','" + Product_name + "','" + Qty + "')");
        //                    k = k + 1;
        //                }
        //                //DbCL.executeRdr("insert into tbl_Challan_details(Sl_no,Challan_no,Product_id,Product_name,Quantity)values('" + k.ToString() + "','" + invoice_no.ToString() + "','" + Product_id + "','" + Product_name + "','" + Qty + "')");
        //                //k = k + 1;
        //            }


        //        }
        //    }

        //    int j = idreturn();
        //    j = j + 1;
        //    DbCL.executeRdr("insert into tbl_Chalan(Chalan_No,Chalan_Date,Quotation_No,Quotation_Date,Client_ID,Sl_no)values('" + invoice_no.ToString() + "','" + txtinvoiceDate.Text + "','" + lblQuotation_no.Text + "','" + lblQuotation_date.Text + "','" + lblClient_Id.Text + "','" + j.ToString() + "')");
        //    //DbCL.executeRdr("update tbl_Quotation set Status3='Yes' where Quotation_no='" + lblQuotation_no.Text + "'");

        //    insertCorRegFacAddress(invoice_no);
        //    Button1.Visible = false;
        //    PanelOK.Visible = true;
        //    lblOk.Text = "Data Save Successfull...";

        //    }
        //    PanelError.Visible = true;
        //    lblErrorMsg.Text = "Please Select Delivery Address....";
        //}

        protected void Button1_Click(object sender, EventArgs e)
        {
            // Ensure at least one address is selected
            if (FactoryAddress.GetSelectedIndices().Length > 0)
            {
                string invoice_no = BindInvoiceNo();
                int k = 1;
                DataTable dt1;

                dt1 = (DataTable)ViewState["ViewQProductData"];
                if (dt1 != null)
                {
                    for (int i = 0; i < dt1.Rows.Count; i++)
                    {
                        CheckBox chk = (CheckBox)(gd_Quotation.Rows[i].FindControl("chk"));

                        if (chk.Checked)
                        {
                            string Product_id = ((Label)gd_Quotation.Rows[i].FindControl("Product_code")).Text;
                            string Product_Code = ((Label)gd_Quotation.Rows[i].FindControl("product_id")).Text;
                            string Product_name = ((Label)gd_Quotation.Rows[i].FindControl("ProductName")).Text;

                            string itemno = ((Label)gd_Quotation.Rows[i].FindControl("ItemNo")).Text;
                            string materialno = ((Label)gd_Quotation.Rows[i].FindControl("MaterialNo")).Text;
                            string packsize = ((Label)gd_Quotation.Rows[i].FindControl("PackSize")).Text;

                            string Qty = ((TextBox)gd_Quotation.Rows[i].FindControl("Qty")).Text;
                            int quantity = Convert.ToInt32(Qty);

                            if (quantity > 0)
                            {
                                DbCL.executeRdr($@"INSERT INTO tbl_Challan_details(Sl_no, Challan_no, Product_id, Product_code, Product_name, Quantity, ItemNo, MaterialNo, PackSize) VALUES ('{k}', '{invoice_no}', '{Product_id}', '{Product_Code}', '{Product_name}', '{Qty}', '{itemno}', '{materialno}', '{packsize}')");
                                k++;
                            }
                        }
                    }
                }

                int j = idreturn_OLD() + 1;
                DbCL.executeRdr($"INSERT INTO tbl_Chalan(Chalan_No, Chalan_Date, Quotation_No, Quotation_Date, Client_ID, Sl_no) " +
                                $"VALUES ('{invoice_no}', '{txtinvoiceDate.Text}', '{lblQuotation_no.Text}', '{lblQuotation_date.Text}', '{lblClient_Id.Text}', '{j}')");

                insertCorRegFacAddress(invoice_no);
                Button1.Visible = false;

                // If no selection, show error message
                PanelError.Visible = false;
                lblErrorMsg.Text = String.Empty;

                PanelOK.Visible = true;
                lblOk.Text = "Data Save Successful...";

                return; // Stop execution here if successful
            }

            // If no selection, show error message
            PanelError.Visible = true;
            lblErrorMsg.Text = "Please Select Delivery Address....";
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

                    string query = "insert into tbl_ChaSiteAddress(Cha_no,SiteAddress) values (@Cha_no,@SiteAddress)";
                    SqlParameter[] pram = {
                         new SqlParameter("@Cha_no",invoice_no),
                         new SqlParameter("@SiteAddress",listsite_details)
                    };

                    DbCL.SPExecDB(query, pram);
                    slno22 = slno22 + 1;
                }
            }
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Quotation_no = Convert.ToString(e.CommandArgument);
            if (e.CommandName == "Select")
            {
                Panel1.Visible = true;
                Panel2.Visible = false;
                Binddetails(Quotation_no);
                Bindquotationdetails(Quotation_no);
            }

        }

        private void Bindquotationdetails(string Quotation_no)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select Sl_no,Product_id,Product_Code, (Product_name+' '+specification) as Product_name,Quantity,sail_rate, Service_tax_rate,Total_sail_rate2, ItemNo, MaterialNo, PackSize from tbl_Quotaion_details where Quotation_no='" + Quotation_no + "' order by Sl_no";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                MainDt = DbCL.GetDataTable(cmdstring);
                //dt_first = dt;

                for (int i = 0; i < MainDt.Rows.Count; i++)
                {

                    string Sl_no = MainDt.Rows[i]["Sl_no"].ToString();
                    string Product_id = MainDt.Rows[i]["Product_id"].ToString();
                    string Product_Code = MainDt.Rows[i]["Product_Code"].ToString();
                    string Product_name = MainDt.Rows[i]["Product_name"].ToString();
                    string Quantity = MainDt.Rows[i]["Quantity"].ToString();
                    string sail_rate = MainDt.Rows[i]["sail_rate"].ToString();
                    string Service_tax_rate = MainDt.Rows[i]["Service_tax_rate"].ToString();
                    string Total_sail_rate2 = MainDt.Rows[i]["Total_sail_rate2"].ToString();

                    string ItemNo = MainDt.Rows[i]["ItemNo"].ToString();
                    string MaterialNo = MainDt.Rows[i]["MaterialNo"].ToString();
                    string PackSize = MainDt.Rows[i]["PackSize"].ToString();

                    if (ViewState["ViewQProductData"] != null)
                    {
                            dtPCat = (DataTable)ViewState["ViewQProductData"];
                            int count = dtPCat.Rows.Count + 1;

                            SearchProductCatwise(count, Sl_no, Product_id, Product_Code, Product_name, Quantity, sail_rate, Service_tax_rate, Total_sail_rate2, Quotation_no, ItemNo, MaterialNo, PackSize);

                    }
                    else
                    {
                            SearchProductCatwise(1, Sl_no, Product_id, Product_Code, Product_name, Quantity, sail_rate, Service_tax_rate, Total_sail_rate2, Quotation_no, ItemNo, MaterialNo, PackSize);
                    }
                    
                }


                //gd_Quotation.DataSource = DbCL.GetDataTable(cmdstring);
                //gd_Quotation.DataBind();

            }

            DbCL.Conn.Close();
        }

        private void SearchProductCatwise(int count, string sl_no, string product_id, string Product_Code, string product_name, string quantity, string sail_rate, string service_tax_rate, string total_sail_rate2,string Quotation_no, string ItemNo, string MaterialNo, string PackSize)
        {
            string Chalanno = "";
            Chalanno = bindChalanno(Quotation_no);
            if (Chalanno=="")
            {
                Chalanno = "('')";
            }
            else {
                Chalanno = "(" + Chalanno + ")";
            }
            

            string DeliveredQnt = "";
            DeliveredQnt = bindPreQnt(product_name, Quotation_no, Chalanno, ItemNo);


            string RemainQnt = "";
            RemainQnt = (Convert.ToInt32(quantity) - Convert.ToInt32(DeliveredQnt)).ToString();

            DataRow dr;
            if (count == 1)
            {
                dtPCat.Columns.Add(new DataColumn("Product_id", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Product_code", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("ProductName", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Quantity", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("DeliveredQnt", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("RemainQny", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("ItemNo", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("MaterialNo", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("PackSize", typeof(string)));

            }

            if (ViewState["ViewQProductData"] != null)
            {
                for (int i = 0; i < dtPCat.Rows.Count + 1; i++)
                {
                    dtPCat = (DataTable)ViewState["ViewQProductData"];
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
                dr[0] = product_id;
                dr[1] = Product_Code;
                dr[2] = product_name;
                dr[3] = quantity;
                dr[4] = DeliveredQnt;
                dr[5] = RemainQnt;
                dr[6] = ItemNo;
                dr[7] = MaterialNo;
                dr[8] = PackSize;


                dtPCat.Rows.Add(dr);
            }
            else
            {
                dr = dtPCat.NewRow();
                dr[0] = product_id;
                dr[1] = Product_Code;
                dr[2] = product_name;
                dr[3] = quantity;
                dr[4] = DeliveredQnt;
                dr[5] = RemainQnt;
                dr[6] = ItemNo;
                dr[7] = MaterialNo;
                dr[8] = PackSize;

                dtPCat.Rows.Add(dr);

            }
            if (ViewState["ViewQProductData"] != null)
            {
                gd_Quotation.DataSource = (DataTable)ViewState["ViewQProductData"];
                gd_Quotation.DataBind();
            }
            else
            {
                gd_Quotation.DataSource = dtPCat;
                gd_Quotation.DataBind();
            }
            ViewState["ViewQProductData"] = dtPCat;
        }

        private string bindPreQnt(string product_name, string quotation_no, string chalanno, string itemno)
        {
            string deliQnt = "0";
            string query = "select sum(CAST(Quantity as int)) as DeliveredQnt,Product_name from tbl_Challan_details where Challan_no in "+ chalanno + " and Product_name='"+ product_name + "' and ItemNo= '"+ itemno + "' group by ItemNo,Product_name ";
            SqlDataReader rdr1 = DbCL.SPReturnRdr(query, null);
            if (rdr1.Read())
            {
                deliQnt = rdr1["DeliveredQnt"].ToString();
            }
            return deliQnt;
        }

        private string bindChalanno(string quotation_no)
        {
            string chano = "";
            string query = "select Chalan_No from tbl_Chalan where Quotation_no=@quotation_no";
            SqlParameter[] pram = {
                new SqlParameter("@quotation_no",quotation_no)
            };
            SqlDataReader rdr = DbCL.SPReturnRdr(query, pram);
            int i = 0;
            while (rdr.Read())
            {
                string Chano1 = rdr["Chalan_No"].ToString();
                Chano1 = "'" + Chano1 + "'";
                if (i == 0)
                {
                    chano = Chano1;
                }
                else
                {
                    chano = chano + " , " + Chano1;
                }
                i++;
            }
            return chano;
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
                lbl_ponumber.Text = re["PO_Number"].ToString();
                lbl_donumber.Text = re["DO_Number"].ToString();

                string clientcode= re["Client_Id"].ToString();
                //Call
                bindFactoryAddress(clientcode);
            }
            DbCL.Conn.Close();

            //Call
            BindclientName();

            //Call
            BindInvoiceNo();

            //cmbaddressfor.Items.Add("Corporate office");
            //DbCL.FillCombo10(cmbaddressfor, "select Factory_name from tbl_Factory where Client_id='" + lblClient_Id.Text + "' order by Factory_name");
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

        private string BindInvoiceNo()
        {
            string prefix = "CHL/FE/";
            string finYear = findmonth();  // e.g., "24-25/"
            string fullPrefix = prefix + finYear;

            int nextNumber = idreturn(fullPrefix);
            string invoiceNo;

            do
            {
                nextNumber += 1;
                invoiceNo = fullPrefix + nextNumber.ToString();
            }
            while (InvoiceNoExists(invoiceNo));  // Ensure no duplicate

            return invoiceNo;
        }


        private string BindInvoiceNo_OLD()
        {
            string f = "";
            
            f = "CHL/FE"  + "/";
            string ss = findmonth();
            f = f + ss;
            int j = idreturn_OLD();
            j = j + 1;
            f = f + j.ToString();
            return f;
        }

        private int idreturn(string prefix)
        {
            int lastNumber = 0;
            string query = "SELECT TOP 1 Chalan_No FROM tbl_Chalan " +
                           "WHERE Chalan_No LIKE @Prefix + '%' " +
                           "ORDER BY ID DESC";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Prefix", prefix);
                con.Open();
                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    string invoiceNo = result.ToString().Trim();
                    string[] parts = invoiceNo.Split('/');
                    int parsedNumber = 0;
                    if (parts.Length >= 4 && int.TryParse(parts[parts.Length - 1], out parsedNumber))
                    {
                        lastNumber = parsedNumber;
                    }
                }
            }

            return lastNumber;
        }

        private bool InvoiceNoExists(string invoiceNo)
        {
            bool exists = false;
            string query = "SELECT COUNT(*) FROM tbl_Chalan WHERE Chalan_No = @Chalan_No";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Chalan_No", invoiceNo);
                con.Open();

                int count = (int)cmd.ExecuteScalar();
                exists = count > 0;
            }

            return exists;
        }


        private int idreturn_OLD()
        {
            string a = null;
            int b = 0;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select top(1) Sl_no from tbl_Chalan order by ID desc";
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

        protected void gd_Quotation_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Reset totals at first row
                if (e.Row.RowIndex == 0)
                {
                    totalQuoted = 0;
                    totalDelivered = 0;
                    totalDue = 0;
                }

                Label lblQuoted = (Label)e.Row.FindControl("Quantity");
                Label lblDelivered = (Label)e.Row.FindControl("DeliveredQnt");
                TextBox txtDue = (TextBox)e.Row.FindControl("Qty");

                TableCell quotedCell = lblQuoted?.Parent as TableCell;
                TableCell deliveredCell = lblDelivered?.Parent as TableCell;

                int quoted = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "Quantity"));
                int delivered = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "DeliveredQnt"));
                int due = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "RemainQny"));

                totalQuoted += quoted;
                totalDelivered += delivered;
                totalDue += due;

                // Apply color formatting
                if (lblQuoted != null && lblDelivered != null && quotedCell != null && deliveredCell != null)
                {
                    if (quoted == delivered)
                    {
                        lblQuoted.ForeColor = System.Drawing.Color.DarkBlue;
                        lblDelivered.ForeColor = System.Drawing.Color.DarkBlue;
                        quotedCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#e6f2ff");
                        deliveredCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#e6f2ff");
                    }
                    else if (quoted > delivered)
                    {
                        lblQuoted.ForeColor = System.Drawing.Color.Red;
                        lblDelivered.ForeColor = System.Drawing.Color.OrangeRed;
                        quotedCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#ffe6e6");
                        deliveredCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#fff0e6");
                    }
                    else // delivered > quoted
                    {
                        lblQuoted.ForeColor = System.Drawing.Color.Orange;
                        lblDelivered.ForeColor = System.Drawing.Color.Green;
                        quotedCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#fff0cc");
                        deliveredCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#e6ffe6");
                    }
                }
            }

            if (e.Row.RowType == DataControlRowType.Footer)
            {
                Label lblTotalQuoted = (Label)e.Row.FindControl("lblTotalQuoted");
                Label lblTotalDelivered = (Label)e.Row.FindControl("lblTotalDelivered");
                Label lblTotalDue = (Label)e.Row.FindControl("lblTotalDue");

                if (lblTotalQuoted != null) lblTotalQuoted.Text = totalQuoted.ToString();
                if (lblTotalDelivered != null) lblTotalDelivered.Text = totalDelivered.ToString();
                if (lblTotalDue != null) lblTotalDue.Text = totalDue.ToString();
            }
        }

    }
}