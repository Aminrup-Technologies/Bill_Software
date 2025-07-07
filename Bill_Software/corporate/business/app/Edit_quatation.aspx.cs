using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;

namespace Bill_Software.corporate.business.app
{
    public partial class WebForm65 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        public DataTable first_datatable;
        public static DataTable Dt = new DataTable("Table");
        public static decimal Gross_amount = 0;
        public static decimal Service_tax = 0;
        public static decimal total_sail_rate_details = 0;
        public static decimal total_Service = 0;
        public static decimal sub_total = 0;

        DataTable dtphasetype = new DataTable();
        DataTable dtPhasefees = new DataTable();
        DataTable dtPCat1 = new DataTable();
        DataTable dtPservice = new DataTable();
        DataTable dtproductWithCat = new DataTable();
        DataTable dtPCat = new DataTable();
        DataTable dtpro = new DataTable();
        DataTable dtProInvPay = new DataTable();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                DbCL.FillCombo(cmbvendor, "select Client_Name from tbl_Client order by Client_Name");
                //DbCL.FillCombo(cmbClient, "select Client_Name from tbl_Client order by Client_Name");
                DbCL.FillComboNew(cmbClient, "SELECT Client_Id, Client_Name FROM tbl_Client ORDER by Client_Name");
                DbCL.FillCombo(ddlPlaceOfSupply, "Select City_Name from tbl_City order by City_Name asc");
                bindphaseType();

                txtfromDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                txttodate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
                Gross_amount = 0;
                Service_tax = 0;
                total_sail_rate_details = 0;
                total_Service = 0;
                sub_total = 0;
            }
        }

        protected void btnSertch_Click(object sender, EventArgs e)
        {
            string cmdstring = "";
            if (RadioButtonList1.SelectedIndex == 0)
            {
                BuindCompanyId();
                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Client_Id='" + lblclientId.Text + "' and tbl_Quotation.Status2='No' order by cast(tbl_Quotation.Quotation_date as datetime) desc";
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where tbl_Quotation.Client_Id='" + lblclientId.Text + "' order by tbl_Quotation.ID desc";
                Buinddatagrid(cmdstring);
            }
            else if (RadioButtonList1.SelectedIndex == 1)
            {
                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' and tbl_Quotation.Status2='No' order by cast(tbl_Quotation.Quotation_date as datetime) desc";
                cmdstring = "select tbl_QuoPriSerTogather.PServiceName,tbl_Quotation.ID,tbl_Quotation.service_tax1,tbl_Quotation.sub_total,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Quotation.mailStatusDate,tbl_Client.Client_Name from tbl_Quotation LEFT OUTER join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id LEFT OUTER JOIN tbl_QuoPriSerTogather on tbl_QuoPriSerTogather.qutno = tbl_Quotation.Quotation_no where cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' order by tbl_Quotation.ID desc";
                Buinddatagrid(cmdstring);
            }
            else
            {
                BuindCompanyId();
                //cmdstring = "select tbl_Quotation.ID,tbl_Quotation.Quotation_no,tbl_Quotation.Quotation_date,tbl_Quotation.Gross,tbl_Quotation.Service_tax,tbl_Quotation.Net_amount,tbl_Client.Client_Name from tbl_Quotation inner join tbl_Client on tbl_Quotation.Client_Id=tbl_Client.Client_Id where tbl_Quotation.Client_Id='" + lblclientId.Text + "' and cast(tbl_Quotation.Quotation_date as datetime) between '" + txttodate.Text + "' and '" + txtfromDate.Text + "' and tbl_Quotation.Status2='No' order by cast(tbl_Quotation.Quotation_date as datetime) desc";
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
            Response.Redirect("~/corporate/business/app/Edit_quatation.aspx");
        }

        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string Quotation_no = Convert.ToString(e.CommandArgument);
            lbl_recordno.Text = Quotation_no;
            if (e.CommandName == "Select")
            {
                SelectorGridRow.Visible = false;
                lblqno.Text = Quotation_no.ToString();
                //string status = searchDate(Quotation_no);

                Panel1.Visible = true;
                string cmdstring = "select Product_id as ProductID, Product_Code as Product_code, Product_name as ProductName, Type, sail_rate as Sail_Rate, Service_tax_rate as Tax_Rate, Unit, Quantity, ProductOrServiceCat, specification as Brand, Misc as specification, ItemNo, MaterialNo, PackSize, ItemRemarks, discount_rate, Sl_no, DeliveryDate, Department from tbl_Quotaion_details where Quotation_no=@Quotation_no AND IsLatest = 1 AND IsDeleted = 0";
                SqlParameter[] pram = { new SqlParameter("@Quotation_no", Quotation_no) };
                dtpro = DbCL.SPreturn_dt(cmdstring, pram);
                //Binddata1(cmdstring, status, Quotation_no);
                if (dtpro.Rows.Count > 0)
                {
                    for (int i = 0; i < dtpro.Rows.Count; i++)
                    {
                        string ProductId = dtpro.Rows[i]["ProductID"].ToString();
                        string Product_code = dtpro.Rows[i]["Product_code"].ToString();
                        string ProductName = dtpro.Rows[i]["ProductName"].ToString();
                        string Brandspecification = dtpro.Rows[i]["Brand"].ToString();
                        string Specification = dtpro.Rows[i]["specification"].ToString();
                        string Quantity = dtpro.Rows[i]["Quantity"].ToString();
                        string Sail_Rate = dtpro.Rows[i]["Sail_Rate"].ToString();
                        string Tax_Rate = dtpro.Rows[i]["Tax_Rate"].ToString();
                        string Type = dtpro.Rows[i]["Type"].ToString();
                        string Unit = dtpro.Rows[i]["Unit"].ToString();
                        string ProductOrServiceCat = dtpro.Rows[i]["ProductOrServiceCat"].ToString();
                        string ItemNo = dtpro.Rows[i]["ItemNo"].ToString();
                        string MaterialNo = dtpro.Rows[i]["MaterialNo"].ToString();
                        string PackSize = dtpro.Rows[i]["PackSize"].ToString();
                        string ItemRemarks = dtpro.Rows[i]["ItemRemarks"].ToString();
                        string Slno = dtpro.Rows[i]["Sl_no"].ToString();
                        string DiscountRate = dtpro.Rows[i]["discount_rate"].ToString();
                        string DeliveryDate = dtpro.Rows[i]["DeliveryDate"].ToString();
                        string Department = dtpro.Rows[i]["Department"].ToString();

                        if (ViewState["PhaseProductData"] != null)
                        {
                            //dtPCat = (DataTable)ViewState["PhaseProductData"];

                            if (ViewState["PhaseProductData"] != null)
                                dtPCat = (DataTable)ViewState["PhaseProductData"];
                            else
                                dtPCat = new DataTable();

                            int count = dtPCat.Rows.Count + 1;

                            SearchProductCatwise_NEW(count, Product_code, ProductId, ProductName, Brandspecification,Specification, Quantity, Sail_Rate, Tax_Rate, Type, Unit, ProductOrServiceCat,ItemNo, MaterialNo, PackSize, ItemRemarks, Slno, DiscountRate, DeliveryDate, Department);
                            //SearchProductCatwise(count, ProductId, Product_code, ProductName, Brandspecification, Specification, Quantity, Sail_Rate, Tax_Rate, Type, Unit, ProductOrServiceCat);
                        }
                        else
                        {
                            SearchProductCatwise_NEW(1, Product_code, ProductId, ProductName, Brandspecification,Specification, Quantity, Sail_Rate, Tax_Rate, Type, Unit, ProductOrServiceCat,ItemNo, MaterialNo, PackSize, ItemRemarks, Slno, DiscountRate, DeliveryDate, Department);
                            //SearchProductCatwise(1, ProductId, Product_code, ProductName, Brandspecification, Specification, Quantity, Sail_Rate, Tax_Rate, Type, Unit, ProductOrServiceCat);
                        }
                    }

                }

                Bindcombo();
                BindClientDetails(Quotation_no);
                //bindphaseType(Quotation_no);
                BindPaymentPhases(Quotation_no);
                LoadPrimaryServices(Quotation_no);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "checkAllRows", "checkAllOnLoad();", true);
            }
        }

        private void BindPaymentPhases(string qutno)
        {
            string query = "SELECT phase_type AS PaymentPhase, PhaseDesc, amountper AS AmountPer FROM tbl_QutPaymentPhase WHERE qut_no = @qutno";
            SqlParameter[] param = { new SqlParameter("@qutno", qutno) };

            DataTable dt = DbCL.SPreturn_dt(query, param);

            if (dt != null && dt.Rows.Count > 0)
            {
                GridView3.DataSource = dt;
                GridView3.DataBind();
            }
            else
            {
                GridView3.DataSource = null;
                GridView3.DataBind();
            }
        }


        private void LoadPrimaryServices(string qutNo)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("ProductCatagory", typeof(string)));

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                string query = "SELECT PrimaryService FROM tbl_QutPrimaryService WHERE qut_no = @qut_no";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@qut_no", qutNo);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DataRow dr = dt.NewRow();
                            dr["ProductCatagory"] = reader["PrimaryService"].ToString();
                            dt.Rows.Add(dr);
                        }
                    }
                }
            }

            // Bind to GridView and store in ViewState
            gridps.DataSource = dt;
            gridps.DataBind();
            ViewState["pService"] = dt;
        }


        private void bindphaseType(string qutno)
        {
            string str = "SELECT id, PaymentPhase FROM tbl_PaymentPhase ORDER BY id";
            DataTable dtphasetype = DbCL.SPreturn_dt(str, null);

            listPhaseType.Items.Clear();

            // Fetch selected values for the current qut_no
            string selectedQuery = "SELECT phase_type FROM tbl_QutPaymentPhase WHERE qut_no = @qutno";
            SqlParameter[] param = { new SqlParameter("@qutno", qutno) };
            DataTable selectedPhases = DbCL.SPreturn_dt(selectedQuery, param);

            List<string> selectedValues = selectedPhases.AsEnumerable()
                                             .Select(row => row["phase_type"].ToString())
                                             .ToList();

            foreach (DataRow row in dtphasetype.Rows)
            {
                string value = row["PaymentPhase"].ToString();
                ListItem item = new ListItem(value);

                if (selectedValues.Contains(value))
                {
                    item.Selected = true;
                }

                listPhaseType.Items.Add(item);
            }
        }


        public class Quotation
        {
            public int ID { get; set; }
            public string QuotationNo { get; set; }
            public string QuotationDate { get; set; }
            public string ClientId { get; set; }
            public decimal? Gross { get; set; }
            public decimal? ServiceTax { get; set; }
            public decimal? NetAmount { get; set; }
            public string Status1 { get; set; }
            public string Status2 { get; set; }
            public string Status3 { get; set; }
            public int? SlNo { get; set; }
            public decimal? ServiceTax1 { get; set; }
            public decimal? SubTotal { get; set; }
            public decimal? CGSTorSGST { get; set; }
            public decimal? IGST { get; set; }
            public string ProSer { get; set; }
            public DateTime? MailStatusDate { get; set; }
            public string PlaceOfSupply { get; set; }
            public string MailStatus { get; set; }
            public string PaymentStatus { get; set; }
            public string ReferenceName { get; set; }
            public string ReferenceData { get; set; }
            public string ReferenceId { get; set; }
            public string ReferenceDate { get; set; }
            public string ValidityDays { get; set; }
            public string DeliveryTenure { get; set; }
            public string PackingCharges { get; set; }
            public string Remarks { get; set; }
            public string DetailedView { get; set; }
            public string RecordType { get; set; }
            public string DO_Number { get; set; }
            public string PO_Number { get; set; }
            public string PO_Date { get; set; }
            public string ValidityStartDate { get; set; }
            public string ValidityEndDate { get; set; }
            public int? AddedById { get; set; }
            public DateTime? AddedOn { get; set; }
            public int? ModifiedById { get; set; }
            public DateTime? ModifiedOn { get; set; }
            public int? DeletedById { get; set; }
            public DateTime? DeletedOn { get; set; }
            public DateTime? TimsStamp { get; set; }
            public string DiscountView { get; set; }
        }

        private void BindClientDetails(string Quotation_no)
        {
            BindQuotationDetails(Quotation_no);
            
        }

        public Quotation GetQuotationByNo(string quotationNo)
        {
            Quotation result = null;
            string connStr = ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString;
            string query = @"SELECT * FROM tbl_Quotation WHERE Quotation_no = @Quotation_no";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Quotation_no", quotationNo);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    result = new Quotation
                    {
                        ID = Convert.ToInt32(reader["ID"]),
                        QuotationNo = reader["Quotation_no"].ToString(),
                        QuotationDate = reader["Quotation_date"].ToString(),
                        ClientId = reader["Client_Id"].ToString(),
                        Gross = reader["Gross"] as decimal?,
                        ServiceTax = reader["Service_tax"] as decimal?,
                        NetAmount = reader["Net_amount"] as decimal?,
                        Status1 = reader["Status1"].ToString(),
                        Status2 = reader["Status2"].ToString(),
                        Status3 = reader["status3"].ToString(),
                        SlNo = reader["Sl_no"] as int?,
                        ServiceTax1 = reader["service_tax1"] as decimal?,
                        SubTotal = reader["sub_total"] as decimal?,
                        CGSTorSGST = reader["cgstOrsgst"] as decimal?,
                        IGST = reader["igst"] as decimal?,
                        ProSer = reader["ProSer"].ToString(),
                        MailStatusDate = reader["mailStatusDate"] as DateTime?,
                        PlaceOfSupply = reader["PlaceofSupply"].ToString(),
                        MailStatus = reader["mailStatus"].ToString(),
                        PaymentStatus = reader["PaymentStatus"].ToString(),
                        ReferenceName = reader["ReferenceName"].ToString(),
                        ReferenceData = reader["ReferenceData"].ToString(),
                        ReferenceId = reader["ReferenceId"].ToString(),
                        ReferenceDate = reader["ReferenceDate"].ToString(),
                        ValidityDays = reader["ValidityDays"].ToString(),
                        DeliveryTenure = reader["DeliveryTenure"].ToString(),
                        PackingCharges = reader["PackingCharges"].ToString(),
                        Remarks = reader["Remarks"].ToString(),
                        DetailedView = reader["DetailedView"].ToString(),
                        RecordType = reader["RecordType"].ToString(),
                        DO_Number = reader["DO_Number"].ToString(),
                        PO_Number = reader["PO_Number"].ToString(),
                        PO_Date = reader["PO_Date"].ToString(),
                        ValidityStartDate = reader["Validity_StartDate"].ToString(),
                        ValidityEndDate = reader["Validity_EndDate"].ToString(),
                        AddedById = reader["AddedById"] as int?,
                        AddedOn = reader["AddedOn"] as DateTime?,
                        ModifiedById = reader["ModifiedById"] as int?,
                        ModifiedOn = reader["ModifiedOn"] as DateTime?,
                        DeletedById = reader["DeletedById"] as int?,
                        DeletedOn = reader["DeletedOn"] as DateTime?,
                        TimsStamp = reader["TimsStamp"] as DateTime?,
                        DiscountView = reader["DiscountView"].ToString()
                    };
                }
                reader.Close();
            }
            return result;
        }

        protected void BindQuotationDetails(string quotationNo)
        {
            Quotation q = GetQuotationByNo(quotationNo);
            if (q != null)
            {
                DateTime tempDate;
                if (DateTime.TryParseExact(q.QuotationDate, "dd-MMM-yyyy", null, System.Globalization.DateTimeStyles.None, out tempDate))
                {
                    txtquotationDate.Text = tempDate.ToString("dd-MMM-yyyy");
                }
                else
                {
                    txtquotationDate.Text = q.QuotationDate; // fallback if parsing fails
                }

                txt_valdays.Text = q.ValidityDays.ToString();
                ListItem item = DDL_ItemViewType.Items.FindByText(q.DetailedView);
                if (item != null)
                {
                    DDL_ItemViewType.ClearSelection();
                    item.Selected = true;
                }

                if (cmbClient.Items.FindByValue(q.ClientId) != null)
                {
                    cmbClient.SelectedValue = q.ClientId;
                }

                ListItem item1 = DDL_DiscountView.Items.FindByText(q.DiscountView);
                if (item1 != null) { DDL_DiscountView.ClearSelection(); item1.Selected = true; }

                ListItem item2 = DDL_DeliveryTerms.Items.FindByText(q.DeliveryTenure);
                if (item2 != null) { DDL_DeliveryTerms.ClearSelection(); item2.Selected = true; }

                ListItem item3 = DDL_pkgfrwd.Items.FindByText(q.PackingCharges);
                if (item3 != null) { DDL_pkgfrwd.ClearSelection(); item3.Selected = true; }

                txt_remarks.Text = q.Remarks;

                if (q.RecordType.ToString() == "Quotation")
                {
                    PO_DataInputs.Visible = false;
                    rbQt.Checked = true;
                    rbPo.Checked = false;
                    rbQt.Enabled = false;
                    rbPo.Enabled = false;

                    txb_ponumber.Text = "";
                    txb_donumber.Text = "";
                    txb_podate.Text = "";
                    txb_strtdt.Text = "";
                    txb_enddt.Text = "";
                }
                else
                {
                    txb_ponumber.Text = q.PO_Number;
                    txb_donumber.Text = q.DO_Number;
                    txb_podate.Text = FormatDate(q.PO_Date.ToString());
                    txb_strtdt.Text = FormatDate(q.ValidityStartDate.ToString());
                    txb_enddt.Text = FormatDate(q.ValidityEndDate.ToString());

                    rbQt.Enabled = false;
                    rbPo.Enabled = false;
                    rbQt.Checked = false;
                    rbPo.Checked = true;
                    PO_DataInputs.Visible = true;
                    
                }

                ListItem item4 = ddlPlaceOfSupply.Items.FindByText(q.PlaceOfSupply);
                if (item4 != null)
                {
                    ddlPlaceOfSupply.ClearSelection();
                    item4.Selected = true;
                }

                if (q.ReferenceData.ToString() == "Yes")
                {
                    rbYes.Checked = true;
                    rbNo.Checked = false;

                    txt_clientrefname.Text = q.ReferenceName.ToString();
                    txt_clientrefid.Text = q.ReferenceId.ToString();
                    DateTime dt;
                    if (DateTime.TryParseExact(q.ReferenceDate, "dd-MMM-yyyy", null, System.Globalization.DateTimeStyles.None, out dt))
                    {
                        txt_clientrefdate.Text = dt.ToString("dd-MMM-yyyy");
                    }
                    else
                    {
                        txt_clientrefdate.Text = ""; // fallback for invalid or empty values
                    }
                }
                else
                {
                    rbYes.Checked = false;
                    rbNo.Checked = true;

                    txt_clientrefname.Text = "N/A";
                    txt_clientrefid.Text = "N/A";
                    txt_clientrefdate.Text = "01-Jan-2000";
                }

                if (q.CGSTorSGST.ToString() == "Yes")
                {
                    RadioButtonGst.SelectedValue = "1";
                }
                else
                {
                    RadioButtonGst.SelectedValue = "0";
                }
            }
            else
            {
                // Handle not found case
            }
        }

        private void bindphaseType()
        {
            string str = "select id, PaymentPhase from tbl_PaymentPhase order by id";
            dtphasetype = DbCL.SPreturn_dt(str, null);
            if (dtphasetype.Rows.Count > 0)
            {
                listPhaseType.Items.Clear();
                for (int i = 0; i < dtphasetype.Rows.Count; i++)
                {
                    listPhaseType.Items.Add(dtphasetype.Rows[i]["PaymentPhase"].ToString());
                }
            }
        }

        private string FormatDate(string input)
        {
            DateTime dt;
            if (DateTime.TryParseExact(input, "dd-MMM-yyyy", null, System.Globalization.DateTimeStyles.None, out dt))
                return dt.ToString("dd-MMM-yyyy");
            return "";
        }

        private void Bindcombo()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "";

            cmdstring = "select ProductOrServiceCat from tbl_NewparentProduct order by ProductOrServiceCat asc";

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

        //private string searchDate(string qno)
        //{
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    string status_value = "";
        //    string Lst = "30-Jun-2017";
        //    string CmdString = "select Quotation_no from tbl_Quotation where Quotation_no='" + qno + "' and (CONVERT(datetime, Quotation_date, 103) > CONVERT(datetime, '" + Lst + "', 103))";
        //    SqlCommand cmd = new SqlCommand(CmdString, DbCL.Conn);
        //    SqlDataReader re1 = cmd.ExecuteReader();
        //    if (re1.Read())
        //    {
        //        status_value = "YES";
        //    }
        //    else
        //    {
        //        status_value = "NO";
        //    }
        //    DbCL.Conn.Close();
        //    return status_value;
        //}

        //private string searchDate(string quotation_no)
        //{
        //    string date = "";
        //    string query = "select Quotation_date from tbl_Quotation where Quotation_no=@Quotation_no";
        //    SqlParameter[] pram = {
        //        new SqlParameter("@Quotation_no",quotation_no),
        //    };
        //    SqlDataReader rdr = DbCL.SPReturnRdr(query, pram);
        //    if (rdr.Read())
        //    {
        //        date= rdr["Quotation_date"].ToString();
        //    }
        //    return date;
        //}

        //private void Binddata1(string cmdstring, string status, string Quotation_no)
        //{
        //    string ProOrSer = bindProOrSer(Quotation_no);
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    SqlCommand com1 = new SqlCommand(cmdstring, DbCL.Conn);
        //    SqlDataAdapter da = new SqlDataAdapter(com1);
        //    SqlDataReader dr = com1.ExecuteReader();

        //    while (dr.Read())
        //    {
        //        string Product_code = dr["Product_code"].ToString();
        //        string Product_Name = dr["Product_Name"].ToString();
        //        string Sail_Rate = dr["Sail_Rate"].ToString();
        //        string Tax_Rate = dr["Tax_Rate"].ToString();
        //        string Quantity = dr["Quantity"].ToString(); 
        //        DataTable dt = binddatatable(Product_code, Product_Name, Sail_Rate, Tax_Rate, Quantity);
        //        first_datatable = dt;
        //        if (Label1.Text == "1")
        //        {
        //            newgrid1();
        //            Label2.Text = ProOrSer.ToString();
        //            Bindcombo(ProOrSer, status);
        //        }
        //        else
        //        {
        //            newgrid();
        //        }
        //        Label1.Text = (Convert.ToInt32(Label1.Text) + 1).ToString();

        //    }
        //    DbCL.Conn.Close();
        //}

        //private string bindProOrSer(string quotation_no)
        //{
        //    string a = "";
        //    string query = "select ProSer from tbl_Quotation where Quotation_no=@Quotation_no";
        //    SqlParameter[] pram = {
        //        new SqlParameter("@Quotation_no",quotation_no)
        //    };
        //    SqlDataReader rdrt = DbCL.SPReturnRdr(query, pram);
        //    if (rdrt.Read())
        //    {
        //        a= rdrt["ProSer"].ToString();
        //    }
        //    return a;
        //}

        //private void Bindcombo(string ProOrSer, string status)
        //{
        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    string cmdstring = "";

        //    if (status == "YES")
        //    {  
        //        cmdstring = "select ProductOrServiceCat from tbl_NewparentProduct order by ProductOrServiceCat asc";
        //    }

        //    else
        //    {
        //        if (ProOrSer == "Product")
        //        {
        //            cmdstring = "select distinct(Product_Name) from tbl_Product order by Product_Name";
        //        }
        //        else if (ProOrSer == "Service")
        //        {
        //            cmdstring = "select Service_name  from tbl_Service order by Service_name";
        //        }
        //    }


        //    cmbproduct_service.Items.Clear();
        //    cmbproduct_service.Items.Add("--Select--");
        //    SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
        //    SqlDataReader re = cmd.ExecuteReader();
        //    while (re.Read())
        //    {
        //        cmbproduct_service.Items.Add(re.GetValue(0).ToString());
        //    }
        //    DbCL.Conn.Close();
        //}

        //private DataTable binddatatable(string Product_code, string Product_Name, string Sail_Rate, string Tax_Rate,string Quantity)
        //{
        //    DataTable dt = new DataTable("Table");
        //    DataRow dr = null;
        //    DataColumn Ser_pro_code = new DataColumn("Ser_pro_code", typeof(string));
        //    dt.Columns.Add(Ser_pro_code);

        //    DataColumn Ser_pro_Name = new DataColumn("Ser_pro_Name", typeof(string));
        //    dt.Columns.Add(Ser_pro_Name);

        //    DataColumn Sale_rate = new DataColumn("Sale_rate", typeof(string));
        //    dt.Columns.Add(Sale_rate);
        //    DataColumn service_Tax_Rate = new DataColumn("service_Tax_Rate", typeof(string));
        //    dt.Columns.Add(service_Tax_Rate);
        //    DataColumn Total_quanty = new DataColumn("Total_quanty", typeof(string));
        //    dt.Columns.Add(Total_quanty);
        //    dr = dt.NewRow();

        //    dr["Ser_pro_code"] = Product_code.ToString();
        //    dr["Ser_pro_Name"] = Product_Name.ToString();
        //    dr["Sale_rate"] = Sail_Rate.ToString();
        //    dr["service_Tax_Rate"] = Tax_Rate.ToString();
        //    dr["Total_quanty"] = Quantity.ToString();
        //    dt.Rows.Add(dr);
        //    return dt;



        //}

        //private void newgrid1()
        //{
        //    DataTable dt;
        //    dt = first_datatable;

        //    DataRow dr = null;
        //    DataColumn Ser_pro_code = new DataColumn("Ser_pro_code", typeof(string));
        //    Dt.Columns.Add(Ser_pro_code);

        //    DataColumn Ser_pro_Name = new DataColumn("Ser_pro_Name", typeof(string));
        //    Dt.Columns.Add(Ser_pro_Name);
        //    DataColumn Vendor_rate = new DataColumn("Vendor_rate", typeof(string));
        //    Dt.Columns.Add(Vendor_rate);
        //    DataColumn Sale_rate = new DataColumn("Sale_rate", typeof(string));
        //    Dt.Columns.Add(Sale_rate);
        //    DataColumn service_Tax_Rate = new DataColumn("service_Tax_Rate", typeof(string));
        //    Dt.Columns.Add(service_Tax_Rate);
        //    DataColumn Total_quanty = new DataColumn("Total_quanty", typeof(string));
        //    Dt.Columns.Add(Total_quanty);

        //    for (int i = 0; i <= dt.Rows.Count - 1; i++)
        //    {
        //        string Ser_pro_code1 = (String)first_datatable.Rows[i][0];
        //        string Ser_pro_Name1 = (String)first_datatable.Rows[i][1]; ;

        //        string Sale_rate1 = (String)first_datatable.Rows[i][2];
        //        string service_Tax_Rate1 = (String)first_datatable.Rows[i][3];
        //        string Total_quanty1 = (String)first_datatable.Rows[i][4];
        //        dr = Dt.NewRow();
        //        dr["Ser_pro_code"] = Ser_pro_code1.ToString();
        //        dr["Ser_pro_Name"] = Ser_pro_Name1.ToString();

        //        dr["Sale_rate"] = Sale_rate1.ToString();
        //        dr["service_Tax_Rate"] = service_Tax_Rate1.ToString();
        //        dr["Total_quanty"] = Total_quanty1.ToString();
        //        Dt.Rows.Add(dr);



        //    }

        //}

        //private void newgrid()
        //{
        //    DataTable dt;
        //    dt = first_datatable;
        //    DataRow dr = null;
        //    for (int i = 0; i <= dt.Rows.Count - 1; i++)
        //    {
        //        string Ser_pro_code1 = (String)first_datatable.Rows[i][0];
        //        string Ser_pro_Name1 = (String)first_datatable.Rows[i][1]; 
        //        string Sale_rate1 = (String)first_datatable.Rows[i][2];
        //        string service_Tax_Rate1 = (String)first_datatable.Rows[i][3];
        //        string Total_quanty1 = (String)first_datatable.Rows[i][4];
        //        dr = Dt.NewRow();
        //        dr["Ser_pro_code"] = Ser_pro_code1.ToString();
        //        dr["Ser_pro_Name"] = Ser_pro_Name1.ToString();
        //        dr["Sale_rate"] = Sale_rate1.ToString();
        //        dr["service_Tax_Rate"] = service_Tax_Rate1.ToString();
        //        dr["Total_quanty"] = Total_quanty1.ToString();
        //        Dt.Rows.Add(dr);

        //    }
        //}


        //private void newgrid3()
        //{
        //    DataTable dt;
        //    dt = first_datatable;

        //    DataRow dr = null;
        //    DataColumn Ser_pro_code = new DataColumn("Ser_pro_code", typeof(string));
        //    Dt.Columns.Add(Ser_pro_code);

        //    DataColumn Ser_pro_Name = new DataColumn("Ser_pro_Name", typeof(string));
        //    Dt.Columns.Add(Ser_pro_Name);
        //    DataColumn Vendor_rate = new DataColumn("Vendor_rate", typeof(string));
        //    Dt.Columns.Add(Vendor_rate);
        //    DataColumn Sale_rate = new DataColumn("Sale_rate", typeof(string));
        //    Dt.Columns.Add(Sale_rate);
        //    DataColumn service_Tax_Rate = new DataColumn("service_Tax_Rate", typeof(string));
        //    Dt.Columns.Add(service_Tax_Rate);
        //    DataColumn Total_quanty = new DataColumn("Total_quanty", typeof(string));
        //    Dt.Columns.Add(Total_quanty);

        //    for (int i = 0; i <= dt.Rows.Count - 1; i++)
        //    {
        //        string Ser_pro_code1 = (String)first_datatable.Rows[i][0];
        //        string Ser_pro_Name1 = (String)first_datatable.Rows[i][1]; ;

        //        string Sale_rate1 = (String)first_datatable.Rows[i][2];
        //        string service_Tax_Rate1 = (String)first_datatable.Rows[i][3];

        //        dr = Dt.NewRow();
        //        dr["Ser_pro_code"] = Ser_pro_code1.ToString();
        //        dr["Ser_pro_Name"] = Ser_pro_Name1.ToString();
        //        dr["Sale_rate"] = Sale_rate1.ToString();
        //        dr["service_Tax_Rate"] = service_Tax_Rate1.ToString();

        //        Dt.Rows.Add(dr);



        //    }

        //}

        //private void newgrid2()
        //{
        //    DataTable dt;
        //    dt = first_datatable;
        //    DataRow dr = null;
        //    for (int i = 0; i <= dt.Rows.Count - 1; i++)
        //    {
        //        string Ser_pro_code1 = (String)first_datatable.Rows[i][0];
        //        string Ser_pro_Name1 = (String)first_datatable.Rows[i][1]; ;

        //        string Sale_rate1 = (String)first_datatable.Rows[i][2];
        //        string service_Tax_Rate1 = (String)first_datatable.Rows[i][3];

        //        dr = Dt.NewRow();
        //        dr["Ser_pro_code"] = Ser_pro_code1.ToString();
        //        dr["Ser_pro_Name"] = Ser_pro_Name1.ToString();

        //        dr["Sale_rate"] = Sale_rate1.ToString();
        //        dr["service_Tax_Rate"] = service_Tax_Rate1.ToString();

        //        Dt.Rows.Add(dr);



        //    }
        //}

        protected void Button2_Click(object sender, EventArgs e)
        {
            gridProdWithCat.Visible = true;

            //string cmdstring = "select Id,Product_code,ProductName,Type,Sail_Rate,Tax_Rate,Unit,Brand,ProductOrServiceCat from tbl_NewProduct where ProductOrServiceCat=@ProductOrServiceCat order by Type,ProductName";
            string cmdstring = "select Id, Product_code, ProductID,ProductOrServiceCat,Brand, ProductName,Specification,Type,Sail_Rate,Tax_Rate,Unit from tbl_NewProduct where ProductOrServiceCat=@ProductOrServiceCat order by Id,ProductName";
            SqlParameter[] pram = {
                new SqlParameter("@ProductOrServiceCat",cmbproduct_service.Text)
            };
            dtproductWithCat = DbCL.SPreturn_dt(cmdstring, pram);
            if (dtproductWithCat.Rows.Count > 0)
            {
                gridProdWithCat.DataSource = dtproductWithCat;
                gridProdWithCat.DataBind();
                ViewState["dtprocat"] = dtproductWithCat;
            }


            ////string code = Label2.Text;
            ////string product_service = code.Substring(0, 1);
            ////string qno1 = lblqno.Text;
            ////string datrstatus= searchDate(qno1);
            ////if (datrstatus=="YES")
            ////{
            ////    string cmdstring = "select Product_code,Sub_Prod_Name,Sail_Rate,Tax_Rate from tbl_NewProduct where Product_Name='" + cmbproduct_service.Text + "'";
            ////    Bindproduct(cmdstring);
            ////}
            ////else {

            ////    if (product_service.ToString() == "P")
            ////    {
            ////        string cmdstring = "select Product_code,Sub_Prod_Name,Sail_Rate,Tax_Rate from tbl_Product where Product_Name='" + cmbproduct_service.Text + "'";
            ////        Bindproduct(cmdstring);
            ////    }
            ////    else if (product_service.ToString() == "S")
            ////    {
            ////        string cmdstring = "select Service_code,Service_name,Sail_rate,Tax_rate  from tbl_Service where Service_name='" + cmbproduct_service.Text + "'";
            ////        Bindproduct(cmdstring);
            ////    }
            ////}
            ////cmbproduct_service.SelectedIndex = 0;
            ////gd_Service_Product.DataSource = Dt;
            ////gd_Service_Product.DataBind();
            ////ViewState["dt"] = Dt;
        }

        //private void Bindproduct(string cmdstring)
        //{

        //    DbCL.Sqlconnection();
        //    DbCL.ConnectDb();
        //    SqlCommand com1 = new SqlCommand(cmdstring, DbCL.Conn);
        //    SqlDataAdapter da = new SqlDataAdapter(com1);
        //    SqlDataReader dr = com1.ExecuteReader();

        //    if (dr.Read())
        //    {
        //        DataTable dt = DbCL.GetDataTable(cmdstring);
        //        first_datatable = dt;
        //        if (Label1.Text == "1")
        //        {
        //            newgrid3();
        //        }
        //        else
        //        {
        //            newgrid2();
        //        }
        //        Label1.Text = (Convert.ToInt32(Label1.Text) + 1).ToString();

        //    }
        //    DbCL.Conn.Close();
        //}

        protected void btnSabe_Click(object sender, EventArgs e)
        {
            DataUpdaterMethod();
        }

        // Updated Method: DataUpdaterMethod()
        private void DataUpdaterMethod()
        {
            string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";
            string qno = lblqno.Text;
            string query = "select Status1, Status2, PaymentStatus from tbl_Quotation where Quotation_no=@Quotation_no";
            SqlParameter[] pram = {
                new SqlParameter("@Quotation_no", qno)
            };

            DataTable dtProInvPay = DbCL.SPreturn_dt(query, pram);
            if (dtProInvPay.Rows.Count > 0)
            {
                string pro = dtProInvPay.Rows[0]["Status1"].ToString();
                string inv = dtProInvPay.Rows[0]["Status2"].ToString();
                string pay = dtProInvPay.Rows[0]["PaymentStatus"].ToString();

                if (pro == "Yes" || inv == "Yes" || pay == "Yes")
                {
                    string status = string.Join(" ", new[] {
                        pro == "Yes" ? "Proforma Invoice" : null,
                        inv == "Yes" ? "Tax Invoice" : null,
                        pay == "Yes" ? "Payment Invoice" : null
                    }.Where(x => x != null));

                    PanelError.Visible = true;
                    lblErrorMsg.Text = "Delete " + status;
                    return;
                }

                decimal new_sub_total = 0, new_total_Service = 0, new_Gross_amount = 0;

                string versionQuery = "SELECT ISNULL(MAX(Version), 0) + 1 FROM tbl_Quotaion_details WHERE Quotation_no = @Quotation_no";
                        SqlParameter[] versionParam = {
                    new SqlParameter("@Quotation_no", qno)
                };
                int newVersion = Convert.ToInt32(DbCL.ExecuteScalar(versionQuery, versionParam));

                // Soft delete old
                string softDeleteQuery = @"UPDATE tbl_Quotaion_details SET IsDeleted = 1, IsLatest = 0, DeletedById = @DeletedById, DeletedOn = GETDATE()
                                  WHERE Quotation_no = @Quotation_no AND IsDeleted = 0 AND IsLatest = 1";
                SqlParameter[] softParams = {
                    new SqlParameter("@Quotation_no", qno),
                    new SqlParameter("@DeletedById", userId)
                };
                DbCL.ExecuteNonQuery(softDeleteQuery, softParams);

                for (int i = 0; i < gd_Service_Product.Rows.Count; i++)
                {
                    CheckBox chk = (CheckBox)gd_Service_Product.Rows[i].FindControl("chk");
                    if (chk.Checked)
                    {
                        string ProductId = ((Label)gd_Service_Product.Rows[i].FindControl("ProductID"))?.Text?.Trim() ?? ""; //ID
                        string Product_code = ((Label)gd_Service_Product.Rows[i].FindControl("Product_code"))?.Text?.Trim() ?? ""; //HSN
                        string ProductName = ((Label)gd_Service_Product.Rows[i].FindControl("ProductName"))?.Text?.Trim() ?? "";
                        string Brand = ((Label)gd_Service_Product.Rows[i].FindControl("Brand"))?.Text?.Trim() ?? "";
                        string ProductOrServiceCat = ((Label)gd_Service_Product.Rows[i].FindControl("ProductOrServiceCat"))?.Text?.Trim() ?? "";
                        string Type = ((Label)gd_Service_Product.Rows[i].FindControl("Type"))?.Text?.Trim() ?? "";
                        string Unit = ((Label)gd_Service_Product.Rows[i].FindControl("Unit"))?.Text?.Trim() ?? "";
                        string InvStatus = "No";
                        string Specification = ((TextBox)gd_Service_Product.Rows[i].FindControl("Specification"))?.Text?.Trim() ?? "~";
                        string ItemNo = ((TextBox)gd_Service_Product.Rows[i].FindControl("ItemNo"))?.Text?.Trim() ?? "";
                        string MaterialNo = ((TextBox)gd_Service_Product.Rows[i].FindControl("MaterialNo"))?.Text?.Trim() ?? "";
                        string PackSize = ((TextBox)gd_Service_Product.Rows[i].FindControl("PackSize"))?.Text?.Trim() ?? "";
                        string ItemRemarks = ((TextBox)gd_Service_Product.Rows[i].FindControl("ItemRemarks"))?.Text?.Trim() ?? "";
                        string DeliveryDate = ((TextBox)gd_Service_Product.Rows[i].FindControl("DeliveryDate"))?.Text?.Trim() ?? "";
                        string Department = ((TextBox)gd_Service_Product.Rows[i].FindControl("Department"))?.Text?.Trim() ?? "";
                        string h = ((TextBox)gd_Service_Product.Rows[i].FindControl("txtOrder"))?.Text?.Trim() ?? "0";

                        decimal Quantity = Convert.ToDecimal(((TextBox)gd_Service_Product.Rows[i].FindControl("Quantity"))?.Text);
                        decimal Sail_Rate = Convert.ToDecimal(((TextBox)gd_Service_Product.Rows[i].FindControl("Sail_Rate"))?.Text);
                        decimal Tax_Rate = Convert.ToDecimal(((Label)gd_Service_Product.Rows[i].FindControl("Tax_Rate"))?.Text);
                        decimal disc;
                        decimal Discount_Rate = decimal.TryParse(((TextBox)gd_Service_Product.Rows[i].FindControl("Discount_Rate"))?.Text, out disc) ? disc : 0;

                        decimal discounted_rate = Sail_Rate - (Sail_Rate * Discount_Rate / 100);
                        decimal taxMultiplier = (Tax_Rate + 100) / 100;
                        decimal Total_sail_rate = taxMultiplier * discounted_rate;
                        decimal Total_sail_rate1 = Total_sail_rate * Quantity;
                        decimal Total_sail_rate2 = discounted_rate * Quantity;
                        decimal Service_tax = (Tax_Rate * Quantity * discounted_rate) / 100;

                        new_sub_total += Total_sail_rate2;
                        new_total_Service += Service_tax;
                        new_Gross_amount = Math.Round(new_Gross_amount + Total_sail_rate1, 2);

                        SqlParameter[] insertParams = {
                            new SqlParameter("@Sl_no", h),
                            new SqlParameter("@Quotation_no", qno),
                            new SqlParameter("@Product_id", ProductId),
                            new SqlParameter("@Product_Code", Product_code),
                            new SqlParameter("@Product_name", ProductName),
                            new SqlParameter("@Quantity", Quantity),
                            new SqlParameter("@sail_rate", Sail_Rate),
                            new SqlParameter("@Service_tax_rate", Tax_Rate),
                            new SqlParameter("@discount_rate", Discount_Rate),
                            new SqlParameter("@new_sailrate", discounted_rate),
                            new SqlParameter("@Total_sail_rate", Total_sail_rate),
                            new SqlParameter("@Total_sail_rate1", Total_sail_rate1),
                            new SqlParameter("@Total_sail_rate2", Total_sail_rate2),
                            new SqlParameter("@specification", Brand),
                            new SqlParameter("@Misc", Specification),
                            new SqlParameter("@InvStatus", InvStatus),
                            new SqlParameter("@Type", Type),
                            new SqlParameter("@Unit", Unit),
                            new SqlParameter("@ProductOrServiceCat", ProductOrServiceCat),
                            new SqlParameter("@ItemRemarks", ItemRemarks),
                            new SqlParameter("@ItemNo", ItemNo),
                            new SqlParameter("@MaterialNo", MaterialNo),
                            new SqlParameter("@PackSize", PackSize),
                            new SqlParameter("@DeliveryDate", DeliveryDate),
                            new SqlParameter("@Department", Department),
                            new SqlParameter("@AddedById", userId),
                            new SqlParameter("@Version", newVersion)
                        };

                        string insertQry = @"INSERT INTO tbl_Quotaion_details
                    (Sl_no, Quotation_no, Product_id, Product_Code, Product_name, Quantity, sail_rate, Service_tax_rate, discount_rate, new_sailrate,
                     Total_sail_rate, Total_sail_rate1, Total_sail_rate2, specification, Misc, InvStatus, Type, Unit, ProductOrServiceCat,
                     ItemRemarks, ItemNo, MaterialNo, PackSize, DeliveryDate, Department, AddedById, AddedOn, Version, IsDeleted, IsLatest)
                    VALUES
                    (@Sl_no, @Quotation_no, @Product_id, @Product_Code, @Product_name, @Quantity, @sail_rate, @Service_tax_rate, @discount_rate, @new_sailrate,
                     @Total_sail_rate, @Total_sail_rate1, @Total_sail_rate2, @specification, @Misc, @InvStatus, @Type, @Unit, @ProductOrServiceCat,
                     @ItemRemarks, @ItemNo, @MaterialNo, @PackSize, @DeliveryDate, @Department, @AddedById, GETDATE(), @Version, 0, 1)";

                        DbCL.ExecuteNonQuery(insertQry, insertParams);
                    }
                }

                DbCL.executeRdr("UPDATE tbl_Quotation SET Gross = '" + new_Gross_amount + "', Service_tax = '" + (new_Gross_amount % 1) +
                                 "', Net_amount = '" + new_Gross_amount + "', service_tax1 = '" + new_total_Service +
                                 "', sub_total = '" + new_sub_total + "' WHERE Quotation_no = '" + qno + "'");

                double NewGross = Convert.ToDouble(new_Gross_amount);
                updatedueamountdetails(NewGross);

                lblOk.Text = "Data Updated Successfully.....";
                PanelOK.Visible = true;
                btnSabe.Visible = false;
            }
        }


        private void DataUpdaterMethod_OLD()
        {
            string qno = lblqno.Text;
            string query = "select Status1,Status2,PaymentStatus from tbl_Quotation where Quotation_no=@Quotation_no";
            SqlParameter[] pram = {
                new SqlParameter("@Quotation_no",qno)
            };

            dtProInvPay = DbCL.SPreturn_dt(query, pram);
            if (dtProInvPay.Rows.Count > 0)
            {
                string status = "";
                string pro = dtProInvPay.Rows[0]["Status1"].ToString();
                string inv = dtProInvPay.Rows[0]["Status2"].ToString();
                string pay = dtProInvPay.Rows[0]["PaymentStatus"].ToString();
                if (pro == "Yes" || inv == "Yes" || pay == "Yes")
                {
                    if (pro == "Yes")
                    {
                        status = "Proforma Invoice";
                    }
                    if (inv == "Yes")
                    {
                        status = status + " Tax Invoice";
                    }
                    if (pay == "Yes")
                    {
                        status = status + " Payment Invoice";
                    }
                    status = "Delete " + status;

                    PanelError.Visible = true;
                    lblErrorMsg.Text = status;
                }
                else
                {
                    double gross = 0;
                    double netamount = 0;
                    double totaltax = 0;
                    double subtotal = 0;
                    double taxamo = 0;


                    DbCL.executeRdr("delete from tbl_Quotaion_details where Quotation_no='" + lblqno.Text + "'");
                    DbCL.executeRdr("delete from tbl_quotation_vat where Quotation_no='" + lblqno.Text + "'");

                    int i = 0;
                    int h = 1;
                    DataTable dt1;
                    DbCL.Sqlconnection();
                    DbCL.ConnectDb();
                    dt1 = (DataTable)ViewState["PhaseProductData"];
                    if (dt1 != null)
                    {

                        for (i = 0; i <= dt1.Rows.Count - 1; i++)
                        {
                            SqlTransaction trans = null;
                            SqlConnection conn = null;
                            SqlCommand cmd = null;
                            try
                            {
                                CheckBox chk = (CheckBox)(gd_Service_Product.Rows[i].FindControl("chk"));
                                if (chk.Checked == true)
                                {
                                    string cnnString = System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ToString();
                                    conn = new SqlConnection(cnnString);
                                    cmd = new SqlCommand { CommandType = CommandType.Text, Connection = conn };
                                    conn.Open();
                                    trans = conn.BeginTransaction();
                                    cmd.Transaction = trans;

                                    string Product_code = ((Label)gd_Service_Product.Rows[i].FindControl("Product_code")).Text;
                                    string ProductName = ((Label)gd_Service_Product.Rows[i].FindControl("ProductName")).Text;
                                    string Brand = ((Label)gd_Service_Product.Rows[i].FindControl("Brand")).Text;
                                    string Quantity = ((TextBox)gd_Service_Product.Rows[i].FindControl("Quantity")).Text;
                                    string Sail_Rate = ((TextBox)gd_Service_Product.Rows[i].FindControl("Sail_Rate")).Text;
                                    string Tax_Rate = ((Label)gd_Service_Product.Rows[i].FindControl("Tax_Rate")).Text;

                                    string Type = ((Label)gd_Service_Product.Rows[i].FindControl("Type")).Text;
                                    string Unit = ((Label)gd_Service_Product.Rows[i].FindControl("Unit")).Text;

                                    string ProductOrServiceCat = ((Label)gd_Service_Product.Rows[i].FindControl("ProductOrServiceCat")).Text;

                                    double quantity = 0;
                                    double sailrate = 0;
                                    double taxrate = 0;
                                    double Total_sail_rate = 0;
                                    double Total_sail_rate1 = 0;
                                    double Total_sail_rate2 = 0;

                                    if (RadioDiscountInflation.SelectedIndex == 0)
                                    {
                                        sailrate = Convert.ToDouble(Sail_Rate);
                                        sailrate = Math.Round(((Convert.ToDouble(txtPercentage.Text) * sailrate) / 100), 2);
                                        sailrate = Math.Round((Convert.ToDouble(Sail_Rate) - sailrate), 2);
                                    }
                                    else if (RadioDiscountInflation.SelectedIndex == 1)
                                    {
                                        sailrate = Convert.ToDouble(Sail_Rate);
                                        sailrate = Math.Round(((Convert.ToDouble(txtPercentage.Text) * sailrate) / 100), 2);
                                        sailrate = Math.Round((Convert.ToDouble(Sail_Rate) + sailrate), 2);
                                    }
                                    else if (RadioDiscountInflation.SelectedIndex == 2)
                                    {
                                        sailrate = Convert.ToDouble(Sail_Rate);
                                    }

                                    quantity = Convert.ToDouble(Quantity);
                                    taxrate = Convert.ToDouble(Tax_Rate);

                                    Total_sail_rate = sailrate + Math.Round(((sailrate * taxrate) / 100), 2);
                                    Total_sail_rate1 = Math.Round((Total_sail_rate * quantity), 2);
                                    Total_sail_rate2 = Math.Round((sailrate * quantity), 2);

                                    subtotal = subtotal + Total_sail_rate2;
                                    subtotal = Math.Round(subtotal);

                                    gross = gross + Total_sail_rate1;
                                    netamount = gross;

                                    netamount = Math.Round(netamount);

                                    taxamo = Math.Round(((sailrate * taxrate) / 100), 2);
                                    totaltax = totaltax + taxamo;
                                    totaltax = Math.Round(totaltax);

                                    decimal service = (Convert.ToDecimal(Tax_Rate) * Convert.ToDecimal(Quantity) * Convert.ToDecimal(Sail_Rate)) / 100;

                                    insertvatamount(service, Tax_Rate);
                                    cmd.CommandText = ("insert into tbl_Quotaion_details(Sl_no,Quotation_no,Product_id,Product_name,Quantity,sail_rate,Service_tax_rate,Total_sail_rate,Total_sail_rate1,Total_sail_rate2,specification,Type,Unit,ProductOrServiceCat)values('" + h.ToString() + "','" + lblqno.Text + "','" + Product_code + "','" + ProductName + "','" + Quantity + "','" + sailrate + "','" + Tax_Rate + "','" + Total_sail_rate + "','" + Total_sail_rate1 + "','" + Total_sail_rate2 + "','" + Brand.ToString() + "','" + Type.ToString() + "','" + Unit.ToString() + "','" + ProductOrServiceCat.ToString() + "')");

                                    cmd.ExecuteNonQuery();
                                    trans.Commit();
                                    conn.Close();
                                    trans.Dispose();
                                    conn.Dispose();
                                    cmd.Dispose();
                                    h = h + 1;

                                }
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
                    Service_tax = Convert.ToDecimal(gross) % 1;

                    DbCL.executeRdr("update tbl_Quotation set Gross='" + gross + "',Service_tax='" + Service_tax + "',Net_amount='" + netamount + "',service_tax1='" + totaltax + "',sub_total='" + subtotal + "' where Quotation_no='" + lblqno.Text + "'");

                    updatedueamountdetails(netamount);

                    lblOk.Text = "Data Updated Successfully.....";
                    PanelOK.Visible = true;
                    btnSabe.Visible = false;
                }
            }
        }

        private void updatedueamountdetails(double netamount)
        {
            string a = findtotalamount();
            double amount = netamount - Convert.ToDouble(a);
            DbCL.executeRdr("update tbl_invoice_due set Due_amount='" + amount.ToString() + "' where qutation_no='" + lblqno.Text + "'");

        }

        private string findtotalamount()
        {
            DbCL.Sqlconnection();
            string amount = "0";
            DbCL.ConnectDb();

            string cmdstring = "select sum(cast(Given_amount as real)) as amount from tbl_invoice_payment where Quotation_No='" + lblqno.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                amount = re["amount"].ToString();
                if (amount != "")
                {
                    amount = re["amount"].ToString();
                }
                else
                {
                    amount = "0";
                }
            }
            else

                DbCL.Conn.Close();
            return amount;
        }

        private void insertvatamount(decimal service, string service_Tax_Rate)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string service1 = service.ToString();
            string cmdstring = "select * from tbl_quotation_vat where Quotation_no='" + lblqno.Text + "' and Vat_rate='" + service_Tax_Rate.ToString() + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                DbCL.executeRdr("update tbl_quotation_vat set Vat_amount=(cast(Vat_amount as real)+'" + service1.ToString() + "') where Quotation_no='" + lblqno.Text + "' and Vat_rate='" + service_Tax_Rate.ToString() + "'");
            }
            else
            {
                DbCL.executeRdr("insert into tbl_quotation_vat(Quotation_no,Vat_rate,Vat_amount)values('" + lblqno.Text + "','" + service_Tax_Rate.ToString() + "','" + service1.ToString() + "')");
            }
            DbCL.Conn.Close();

        }

        protected void btnAddProduct_Click_OLD(object sender, EventArgs e)
        {
            gridProdWithCat.Visible = false;
            if (ViewState["dtprocat"] != null)
            {
                DataTable dtpro = new DataTable();
                dtpro = ViewState["dtprocat"] as DataTable;

                ////string Ser_pro_code = "";
                ////string Ser_pro_Name = "";
                ////string specification = "";

                ////string Sale_rate = "";
                ////string service_Tax_Rate = "";


                string Product_code = "";
                string ProductName = "";
                string Brandspecification = "";
                string Type = "";
                string Sail_Rate = "";
                string Tax_Rate = "";
                string Unit = "";
                string Quantity = "";
                string ProductOrServiceCat = "";

                for (int i = 0; i < dtpro.Rows.Count; i++)
                {
                    CheckBox chkdtp = (CheckBox)(gridProdWithCat.Rows[i].FindControl("chkdtp"));
                    if (chkdtp.Checked == true)
                    {
                        Product_code = ((Label)gridProdWithCat.Rows[i].FindControl("Product_code")).Text;
                        ProductName = ((Label)gridProdWithCat.Rows[i].FindControl("ProductName")).Text;
                        Brandspecification = ((Label)gridProdWithCat.Rows[i].FindControl("Brand")).Text;
                        Quantity = ((Label)gridProdWithCat.Rows[i].FindControl("Quantity")).Text;
                        Sail_Rate = ((Label)gridProdWithCat.Rows[i].FindControl("Sail_Rate")).Text;
                        Tax_Rate = ((Label)gridProdWithCat.Rows[i].FindControl("Tax_Rate")).Text;
                        Type = ((Label)gridProdWithCat.Rows[i].FindControl("Type")).Text;
                        Unit = ((Label)gridProdWithCat.Rows[i].FindControl("Unit")).Text;

                        ProductOrServiceCat = ((Label)gridProdWithCat.Rows[i].FindControl("ProductOrServiceCat")).Text;

                        if (ViewState["PhaseProductData"] != null)
                        {
                            dtPCat = (DataTable)ViewState["PhaseProductData"];
                            int count = dtPCat.Rows.Count + 1;

                            SearchProductCatwise_OLD(count, Product_code, ProductName, Brandspecification, Quantity, Sail_Rate, Tax_Rate, Type, Unit, ProductOrServiceCat);

                        }
                        else
                        {
                            SearchProductCatwise_OLD(1, Product_code, ProductName, Brandspecification, Quantity, Sail_Rate, Tax_Rate, Type, Unit, ProductOrServiceCat);
                        }
                    }
                }
            }
        }

        private void SearchProductCatwise_OLD(int count, string Product_code, string ProductName, string Brandspecification, string Quantity, string Sail_Rate, string Tax_Rate, string Type, string Unit, string ProductOrServiceCat)
        {
            DataRow dr;
            if (count == 1)
            {
                dtPCat.Columns.Add(new DataColumn("Product_code", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("ProductName", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Sail_Rate", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Tax_Rate", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Quantity", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Brand", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Type", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Unit", typeof(string)));

                dtPCat.Columns.Add(new DataColumn("ProductOrServiceCat", typeof(string)));



            }
            if (ViewState["PhaseProductData"] != null)
            {
                for (int i = 0; i < dtPCat.Rows.Count + 1; i++)
                {
                    dtPCat = (DataTable)ViewState["PhaseProductData"];
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
                dr[0] = Product_code;
                dr[1] = ProductName;
                dr[2] = Sail_Rate;
                dr[3] = Tax_Rate;
                dr[4] = Quantity;
                dr[5] = Brandspecification;
                dr[6] = Type;
                dr[7] = Unit;
                dr[8] = ProductOrServiceCat;

                dtPCat.Rows.Add(dr);
            }
            else
            {
                dr = dtPCat.NewRow();
                dr[0] = Product_code;
                dr[1] = ProductName;
                dr[2] = Sail_Rate;
                dr[3] = Tax_Rate;
                dr[4] = Quantity;
                dr[5] = Brandspecification;
                dr[6] = Type;
                dr[7] = Unit;
                dr[8] = ProductOrServiceCat;

                dtPCat.Rows.Add(dr);

            }
            if (ViewState["PhaseProductData"] != null)
            {
                gd_Service_Product.DataSource = (DataTable)ViewState["PhaseProductData"];
                gd_Service_Product.DataBind();
            }
            else
            {
                gd_Service_Product.DataSource = dtPCat;
                gd_Service_Product.DataBind();
            }
            ViewState["PhaseProductData"] = dtPCat;
        }

        //------------------------Below are Added on 04-07-2025-----------------//
        protected void btnAddProduct_Click(object sender, EventArgs e)
        {
            gridProdWithCat.Visible = true;
            if (ViewState["dtprocat"] != null)
            {
                DataTable dtpro = new DataTable();
                dtpro = ViewState["dtprocat"] as DataTable;

                string Product_code = ""; //HSN Code
                string ProductId = ""; // Product ID
                string ProductName = "";
                string Brandspecification = "";
                string Specification = "";
                string Type = "";
                string Sail_Rate = "";
                string Tax_Rate = "";
                string Unit = "";
                string Quantity = "";
                string ProductOrServiceCat = "";


                for (int i = 0; i < dtpro.Rows.Count; i++)
                {
                    CheckBox chkdtp = (CheckBox)(gridProdWithCat.Rows[i].FindControl("chkdtp"));
                    if (chkdtp.Checked == true)
                    {
                        ProductId = ((Label)gridProdWithCat.Rows[i].FindControl("ProductID")).Text;
                        Product_code = ((Label)gridProdWithCat.Rows[i].FindControl("Product_code")).Text;
                        ProductName = ((Label)gridProdWithCat.Rows[i].FindControl("ProductName")).Text;
                        Brandspecification = ((Label)gridProdWithCat.Rows[i].FindControl("Brand")).Text;
                        Specification = ((Label)gridProdWithCat.Rows[i].FindControl("Specification")).Text;
                        //Quantity = ((TextBox)gridProdWithCat.Rows[i].FindControl("Quantity")).Text;
                        Quantity = ((Label)gridProdWithCat.Rows[i].FindControl("Quantity")).Text;
                        //Sail_Rate = ((TextBox)gridProdWithCat.Rows[i].FindControl("Sail_Rate")).Text;
                        Sail_Rate = ((Label)gridProdWithCat.Rows[i].FindControl("Sail_Rate")).Text;
                        Tax_Rate = ((Label)gridProdWithCat.Rows[i].FindControl("Tax_Rate")).Text;
                        Type = ((Label)gridProdWithCat.Rows[i].FindControl("Type")).Text;
                        Unit = ((Label)gridProdWithCat.Rows[i].FindControl("Unit")).Text;
                        ProductOrServiceCat = ((Label)gridProdWithCat.Rows[i].FindControl("ProductOrServiceCat")).Text;

                        if (ViewState["PhaseProductData"] != null)
                        {
                            dtPCat = (DataTable)ViewState["PhaseProductData"];
                            int count = dtPCat.Rows.Count + 1;

                            SearchProductCatwise_NEW(count, ProductId, Product_code, ProductName, Brandspecification, Specification, Quantity, Sail_Rate, Tax_Rate, Type, Unit, ProductOrServiceCat,"","","","","","","","");

                        }
                        else
                        {
                            SearchProductCatwise_NEW(1, ProductId, Product_code, ProductName, Brandspecification, Specification, Quantity, Sail_Rate, Tax_Rate, Type, Unit, ProductOrServiceCat, "", "", "", "", "", "", "", "");
                        }
                    }
                }


                string pservice = cmbproduct_service.Text.ToString();
                if (ViewState["pService"] != null)
                {
                    dtPservice = (DataTable)ViewState["pService"];
                    int count1 = dtPservice.Rows.Count + 1;

                    string service = "";
                    string status = "NO";
                    for (int j = 0; j < dtPservice.Rows.Count; j++)
                    {
                        service = dtPservice.Rows[j]["ProductCatagory"].ToString();
                        if (service == pservice)
                        {
                            status = "YES";
                        }
                    }
                    if (status == "NO")
                    {
                        TakePservice(count1, pservice);
                    }
                }
                else
                {
                    TakePservice(1, pservice);
                }
            }

            // added on 30-Jan-2025, To hide the Products Grid after selection of Products for Quotes Creation
            gridProdWithCat.Visible = false;
            btnAddProduct.Enabled = false;
        }

        private void SearchProductCatwise(int count, string Product_code, string ProductId, string ProductName, string Brandspecification, string Specification, string Quantity, string Sail_Rate, string Tax_Rate, string Type, string Unit, string ProductOrServiceCat)
        {
            DataRow dr;

            if (count == 1)
            {
                dtPCat.Columns.Add(new DataColumn("ProductId", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Product_code", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("ProductName", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Specification", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Sail_Rate", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Tax_Rate", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Quantity", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Brand", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Type", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Unit", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("ProductOrServiceCat", typeof(string)));

                dtPCat.Columns.Add(new DataColumn("DeliveryDate", typeof(string)));
                dtPCat.Columns.Add(new DataColumn("Department", typeof(string)));
            }

            if (ViewState["PhaseProductData"] != null)
            {
                dtPCat = (DataTable)ViewState["PhaseProductData"];
            }

            dr = dtPCat.NewRow();
            dr["ProductId"] = ProductId;
            dr["Product_code"] = Product_code;
            dr["ProductName"] = ProductName;
            dr["Specification"] = Specification;
            dr["Sail_Rate"] = Sail_Rate;
            dr["Tax_Rate"] = Tax_Rate;
            dr["Quantity"] = Quantity;
            dr["Brand"] = Brandspecification;
            dr["Type"] = Type;
            dr["Unit"] = Unit;
            dr["ProductOrServiceCat"] = ProductOrServiceCat;

            dr["DeliveryDate"] = "";
            dr["Department"] = "";

            dtPCat.Rows.Add(dr);

            gd_Service_Product.DataSource = dtPCat;
            gd_Service_Product.DataBind();

            ViewState["PhaseProductData"] = dtPCat;

            // **Set Column Visibility Based on Radio Button Selection**
            ToggleGridColumns();
        }

        private void TakePservice(int count1, string pservice)
        {
            DataRow dr;
            if (count1 == 1)
            {
                dtPCat1.Columns.Add(new DataColumn("ProductCatagory", typeof(string)));
            }
            if (ViewState["pService"] != null)
            {
                for (int i = 0; i < dtPCat1.Rows.Count + 1; i++)
                {
                    dtPCat1 = (DataTable)ViewState["pService"];
                    if (dtPCat1.Rows.Count > 0)
                    {
                        dr = dtPCat1.NewRow();
                        dr[0] = dtPCat1.Rows[0][0].ToString();

                    }
                }
                dr = dtPCat1.NewRow();
                dr[0] = pservice;
                dtPCat1.Rows.Add(dr);
            }
            else
            {
                dr = dtPCat1.NewRow();
                dr[0] = pservice;
                dtPCat1.Rows.Add(dr);

            }

            if (ViewState["pService"] != null)
            {
                gridps.DataSource = (DataTable)ViewState["pService"];
                gridps.DataBind();
            }
            else
            {
                gridps.DataSource = dtPCat1;
                gridps.DataBind();
            }
            ViewState["pService"] = dtPCat1;
        }

        protected void ToggleGridColumns()
        {
            if (gd_Service_Product.Columns.Count > 18) // Make sure columns exist
            {
                bool isPOChecked = rbPo.Checked; // rbPo is checked means show the columns

                gd_Service_Product.Columns[17].Visible = isPOChecked; // Delivery Date
                gd_Service_Product.Columns[18].Visible = isPOChecked; // Department

                foreach (GridViewRow row in gd_Service_Product.Rows)
                {
                    TextBox txtDeliveryDate = (TextBox)row.FindControl("DeliveryDate");
                    TextBox txtDepartment = (TextBox)row.FindControl("Department");

                    RequiredFieldValidator rfvDeliveryDate = (RequiredFieldValidator)row.FindControl("rfvDeliveryDate");
                    RequiredFieldValidator rfvDepartment = (RequiredFieldValidator)row.FindControl("rfvDepartment");

                    if (rfvDeliveryDate != null)
                        rfvDeliveryDate.Enabled = isPOChecked;

                    if (rfvDepartment != null)
                        rfvDepartment.Enabled = isPOChecked;
                }
            }
        }


        //DataTable dtPCat
        //{
        //    get
        //    {
        //        if (ViewState["PhaseProductData"] != null)
        //            return (DataTable)ViewState["PhaseProductData"];
        //        else
        //            return new DataTable();
        //    }
        //    set
        //    {
        //        ViewState["PhaseProductData"] = value;
        //    }
        //}

        private void SearchProductCatwise_NEW(int count, string Product_code, string ProductId, string ProductName,string Brand, string Specification, string Quantity, string Sail_Rate,string Tax_Rate, string Type, string Unit, string ProductOrServiceCat, string ItemNo, string MaterialNo, string PackSize, string ItemRemarks, string Slno, string DiscountRate, string DeliveryDate, string Department)
        {
            if (dtPCat.Columns.Count == 0)
            {
                dtPCat.Columns.Add("ProductId");
                dtPCat.Columns.Add("Product_code");
                dtPCat.Columns.Add("ProductName");
                dtPCat.Columns.Add("Specification");
                dtPCat.Columns.Add("Sail_Rate");
                dtPCat.Columns.Add("Tax_Rate");
                dtPCat.Columns.Add("Quantity");
                dtPCat.Columns.Add("Brand");
                dtPCat.Columns.Add("Type");
                dtPCat.Columns.Add("Unit");
                dtPCat.Columns.Add("ProductOrServiceCat");
                dtPCat.Columns.Add("ItemNo");
                dtPCat.Columns.Add("MaterialNo");
                dtPCat.Columns.Add("PackSize");
                dtPCat.Columns.Add("ItemRemarks");
                dtPCat.Columns.Add("Sl_no");
                dtPCat.Columns.Add("discount_rate");
                dtPCat.Columns.Add("DeliveryDate");
                dtPCat.Columns.Add("Department");
            }

            DataRow dr = dtPCat.NewRow();
            dr["ProductId"] = ProductId;
            dr["Product_code"] = Product_code;
            dr["ProductName"] = ProductName;
            dr["Specification"] = Specification;
            dr["Sail_Rate"] = Sail_Rate;
            dr["Tax_Rate"] = Tax_Rate;
            dr["Quantity"] = Quantity;
            dr["Brand"] = Brand;
            dr["Type"] = Type;
            dr["Unit"] = Unit;
            dr["ProductOrServiceCat"] = ProductOrServiceCat;
            dr["ItemNo"] = ItemNo;
            dr["MaterialNo"] = MaterialNo;
            dr["PackSize"] = PackSize;
            dr["ItemRemarks"] = ItemRemarks;
            dr["Sl_no"] = Slno;
            dr["discount_rate"] = DiscountRate;
            dr["DeliveryDate"] = DeliveryDate;
            dr["Department"] = Department;

            dtPCat.Rows.Add(dr);

            gd_Service_Product.DataSource = dtPCat;
            gd_Service_Product.DataBind();

            ViewState["PhaseProductData"] = dtPCat;

            // **Set Column Visibility Based on Radio Button Selection**
            ToggleGridColumns();
        }

        protected void listPhaseType_TextChanged(object sender, EventArgs e)
        {
            bindOurPhaseAmount();
        }
        private void bindOurPhaseAmount()
        {
            string phasetypename = null;
            string phasedesc = null;
            for (int i = 0; i < listPhaseType.Items.Count; i++)
            {
                if (listPhaseType.Items[i].Selected)
                {
                    if (ViewState["phaseAmountData"] != null)
                    {
                        dtPhasefees = (DataTable)ViewState["phaseAmountData"];
                        int count = dtPhasefees.Rows.Count + 1;

                        phasetypename = listPhaseType.Items[i].Text;
                        //phasedesc = bindphasedesc(phasetypename);
                        phasedesc = "";



                        string service = "";
                        string status = "NO";
                        for (int j = 0; j < dtPhasefees.Rows.Count; j++)
                        {
                            service = dtPhasefees.Rows[j]["PaymentPhase"].ToString();
                            if (service == phasetypename)
                            {
                                status = "YES";
                            }
                        }
                        if (status == "NO")
                        {
                            SearchPaymentPhaseFees(count, phasetypename, phasedesc);
                        }
                    }
                    else
                    {
                        phasetypename = listPhaseType.Items[i].Text;
                        //phasedesc = bindphasedesc(phasetypename);
                        phasedesc = "";
                        SearchPaymentPhaseFees(1, phasetypename, phasedesc);
                    }
                }
            }
        }
        private void SearchPaymentPhaseFees(int count, string phasetypename, string phasedesc)
        {
            DataRow dr;
            if (count == 1)
            {
                dtPhasefees.Columns.Add(new DataColumn("PaymentPhase", typeof(string)));
                dtPhasefees.Columns.Add(new DataColumn("PhaseDesc", typeof(string)));
                dtPhasefees.Columns.Add(new DataColumn("AmountPer", typeof(string)));
            }
            if (ViewState["phaseAmountData"] != null)
            {
                for (int i = 0; i < dtPhasefees.Rows.Count + 1; i++)
                {
                    dtPhasefees = (DataTable)ViewState["phaseAmountData"];
                    if (dtPhasefees.Rows.Count > 0)
                    {
                        dr = dtPhasefees.NewRow();
                        dr[0] = dtPhasefees.Rows[0][0].ToString();
                        dr[1] = dtPhasefees.Rows[0][1].ToString();
                        dr[2] = dtPhasefees.Rows[0][2].ToString();
                    }
                }
                dr = dtPhasefees.NewRow();
                dr[0] = phasetypename;
                dr[1] = phasedesc;
                if (phasetypename == "Full & Final Instalment")
                {
                    dr[2] = "100";
                }
                else
                {
                    dr[2] = "";
                }


                dtPhasefees.Rows.Add(dr);
            }
            else
            {
                dr = dtPhasefees.NewRow();
                dr[0] = phasetypename;
                dr[1] = phasedesc;
                if (phasetypename == "Payment After Delivery")
                {
                    dr[2] = "100";
                }
                else if (phasetypename == "100% Against PI")
                {
                    dr[2] = "100";
                }
                else
                {
                    dr[2] = "";
                }
                dtPhasefees.Rows.Add(dr);

            }
            if (ViewState["phaseAmountData"] != null)
            {
                GridView3.DataSource = (DataTable)ViewState["phaseAmountData"];
                GridView3.DataBind();
            }
            else
            {
                GridView3.DataSource = dtPhasefees;
                GridView3.DataBind();
            }
            ViewState["phaseAmountData"] = dtPhasefees;
        }

        protected void AmountPer_TextChanged(object sender, EventArgs e)
        {
            //Label2.Text = "Sumanta";
            amountCalculation();
        }

        public void amountCalculation()
        {
            double total = 0;
            foreach (GridViewRow gvr in GridView3.Rows)
            {
                string name = gvr.Cells[0].Text;

                string PaymentPhase = ((Label)gvr.Cells[0].FindControl("PaymentPhase")).Text;

                if (PaymentPhase != "Full & Final Instalment")
                {
                    TextBox tb = (TextBox)gvr.Cells[1].FindControl("AmountPer");
                    double sum;
                    if (double.TryParse(tb.Text.Trim(), out sum))
                    {
                        total += sum;
                    }
                }
                else
                {
                    double fulfinal = Convert.ToDouble(100);
                    double netamount = fulfinal - total;

                    TextBox tb = (TextBox)gvr.Cells[1].FindControl("AmountPer");
                    tb.Text = netamount.ToString();

                }
            }
        }

        protected void GridView3_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int index = Convert.ToInt32(e.RowIndex);
            DataTable dtphs = ViewState["phaseAmountData"] as DataTable;
            dtphs.Rows[index].Delete();
            if (dtphs.Rows.Count > 0)
            {
                ViewState["phaseAmountData"] = dtphs;
            }
            else
            {
                ViewState["phaseAmountData"] = null;
                dtphs = null;
            }

            GridView3.DataSource = (DataTable)ViewState["phaseAmountData"];
            GridView3.DataBind();
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            MagicianNew();
            Panel1.Visible = false;
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

        private int idreturn_New(string prefix)
        {
            int lastNumber = 0;
            string query = "SELECT TOP 1 Quotation_no FROM tbl_Quotation " +
                           "WHERE Quotation_no LIKE @Prefix + '%' " +
                           "ORDER BY Id DESC";  // Or based on date if preferred

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Prefix", prefix);
                con.Open();
                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    //string quotationNo = result.ToString();
                    string quotationNo = result.ToString().Trim();

                    // Extract number part after prefix and financial year
                    //string[] parts = quotationNo.Split('/');
                    int parsedNumber = 0;

                    //if (parts.Length == 4 && int.TryParse(parts[3], out parsedNumber))
                    //{
                    //    lastNumber = parsedNumber;
                    //}

                    string[] parts = quotationNo.Split('/');
                    if (parts.Length >= 4 && int.TryParse(parts[parts.Length - 1], out parsedNumber))
                    {
                        lastNumber = parsedNumber;
                    }
                }
            }

            return lastNumber;
        }

        private void Bindquotationno()
        {
            string prefix = "QTN/FE/";
            if (rbPo.Checked)
            {
                prefix = "PO/FE/";
            }

            string ss = findmonth();  // e.g., "24-25/"
            int j = idreturn_New(prefix + ss);  // Get last used number for that prefix

            string quotationNo;
            do
            {
                j += 1;
                quotationNo = prefix + ss + j.ToString();
            }
            while (QuotationNoExists(quotationNo));  // Keep looping if already exists

            lblqno.Text = quotationNo;
        }

        private bool QuotationNoExists(string quotationNo)
        {
            bool exists = false;
            string query = "SELECT COUNT(*) FROM tbl_Quotation WHERE Quotation_no = @QuotationNo";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@QuotationNo", quotationNo);
                con.Open();

                int count = (int)cmd.ExecuteScalar();
                exists = count > 0;
            }

            return exists;
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
            }
            else
            {
                date4 = ((Convert.ToInt32(date3) + 1)).ToString();
                date5 = "31-Mar-" + date3;
                date6 = "31-Mar-" + date4;
            }
            string cmdstring = "select Sl_no from tbl_Quotation where ID=(select max(ID) from tbl_Quotation where cast(Quotation_date as datetime) between '" + date5.ToString() + "' and '" + date6.ToString() + "')";
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

        private void MagicianNew()
        {
            Bindquotationno();

            string CGSTSGSTSTATUS = RadioButtonGst.SelectedIndex == 0 ? "YES" : "";
            string IGSTSTATUS = RadioButtonGst.SelectedIndex != 0 ? "YES" : "";

            int slNo = idreturn() + 1;
            //int h = 0;

            string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";
            DataTable dt1 = (DataTable)ViewState["PhaseProductData"];

            decimal new_sub_total = 0, new_total_Service = 0, new_Gross_amount = 0, total_sail_rate_details = 0;

            string cnnString = System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ToString();

            using (SqlConnection conn = new SqlConnection(cnnString))
            {
                conn.Open();

                using (SqlCommand lockCmd = new SqlCommand("sp_getapplock", conn))
                {
                    lockCmd.CommandType = CommandType.StoredProcedure;
                    lockCmd.Parameters.AddWithValue("@Resource", "Lock_Quotation_" + lblqno.Text);
                    lockCmd.Parameters.AddWithValue("@LockMode", "Exclusive");
                    lockCmd.Parameters.AddWithValue("@LockOwner", "Session");
                    lockCmd.Parameters.AddWithValue("@DbPrincipal", "public");

                    SqlParameter returnCode = new SqlParameter("@return_value", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                    lockCmd.Parameters.Add(returnCode);

                    lockCmd.ExecuteNonQuery();

                    int result = (int)returnCode.Value;
                    if (result < 0)
                    {
                        throw new Exception("Unable to acquire lock. Another user may be editing this quotation.");
                    }
                }

                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    if (dt1 != null)
                    {
                        for (int i = 0; i < dt1.Rows.Count; i++)
                        {
                            CheckBox chk = (CheckBox)(gd_Service_Product.Rows[i].FindControl("chk"));
                            if (chk.Checked)
                            {
                                //h++;

                                // Get GridView controls safely
                                string ProductId = ((Label)gd_Service_Product.Rows[i].FindControl("ProductID"))?.Text?.Trim() ?? "";
                                string Product_code = ((Label)gd_Service_Product.Rows[i].FindControl("Product_code"))?.Text?.Trim() ?? "";
                                string ProductName = ((Label)gd_Service_Product.Rows[i].FindControl("ProductName"))?.Text?.Trim() ?? "";
                                string Brand = ((Label)gd_Service_Product.Rows[i].FindControl("Brand"))?.Text?.Trim() ?? "";
                                string ProductOrServiceCat = ((Label)gd_Service_Product.Rows[i].FindControl("ProductOrServiceCat"))?.Text?.Trim() ?? "";
                                string Type = ((Label)gd_Service_Product.Rows[i].FindControl("Type"))?.Text?.Trim() ?? "";
                                string Unit = ((Label)gd_Service_Product.Rows[i].FindControl("Unit"))?.Text?.Trim() ?? "";
                                string InvStatus = "No";
                                string Specification = ((TextBox)gd_Service_Product.Rows[i].FindControl("Specification"))?.Text?.Trim() ?? "~";
                                string ItemNo = ((TextBox)gd_Service_Product.Rows[i].FindControl("ItemNo"))?.Text?.Trim() ?? "";
                                string MaterialNo = ((TextBox)gd_Service_Product.Rows[i].FindControl("MaterialNo"))?.Text?.Trim() ?? "";
                                string PackSize = ((TextBox)gd_Service_Product.Rows[i].FindControl("PackSize"))?.Text?.Trim() ?? "";
                                string ItemRemarks = ((TextBox)gd_Service_Product.Rows[i].FindControl("ItemRemarks"))?.Text?.Trim() ?? "";
                                string DeliveryDate = ((TextBox)gd_Service_Product.Rows[i].FindControl("DeliveryDate"))?.Text?.Trim() ?? "";
                                string Department = ((TextBox)gd_Service_Product.Rows[i].FindControl("Department"))?.Text?.Trim() ?? "";
                                string h = ((TextBox)gd_Service_Product.Rows[i].FindControl("txtOrder")).Text;
                                decimal qty;
                                if (!decimal.TryParse(((TextBox)gd_Service_Product.Rows[i].FindControl("Quantity"))?.Text?.Trim(), out qty))
                                    throw new ArgumentException("Invalid Quantity");
                                decimal Quantity = qty;

                                decimal rate;
                                if (!decimal.TryParse(((TextBox)gd_Service_Product.Rows[i].FindControl("Sail_Rate"))?.Text?.Trim(), out rate))
                                    throw new ArgumentException("Invalid Sail Rate");
                                decimal Sail_Rate = rate;

                                decimal tax;
                                if (!decimal.TryParse(((Label)gd_Service_Product.Rows[i].FindControl("Tax_Rate"))?.Text?.Trim(), out tax))
                                    throw new ArgumentException("Invalid Tax Rate");
                                decimal Tax_Rate = tax;

                                decimal disc;
                                decimal Discount_Rate = decimal.TryParse(((TextBox)gd_Service_Product.Rows[i].FindControl("Discount_Rate"))?.Text?.Trim(), out disc) ? disc : 0;

                                // Calculations
                                decimal discounted_rate = Sail_Rate - (Sail_Rate * Discount_Rate / 100);
                                decimal taxMultiplier = (Tax_Rate + 100) / 100;
                                decimal Total_sail_rate = taxMultiplier * discounted_rate;
                                decimal Total_sail_rate1 = Total_sail_rate * Quantity;
                                decimal Total_sail_rate2 = discounted_rate * Quantity;
                                decimal Service_tax = (Tax_Rate * Quantity * discounted_rate) / 100;

                                new_sub_total += Total_sail_rate2;
                                new_total_Service += Service_tax;
                                new_Gross_amount = Math.Round(new_Gross_amount + Total_sail_rate1, 2);

                                using (SqlCommand cmd = new SqlCommand(@"INSERT INTO tbl_Quotaion_details (Sl_no, Quotation_no, Product_id, Product_Code, Product_name, Quantity, sail_rate, Service_tax_rate, Total_sail_rate, Total_sail_rate1, Total_sail_rate2, specification, Misc, InvStatus, Type, Unit, ProductOrServiceCat, discount_rate, new_sailrate, ItemRemarks, ItemNo, MaterialNo, PackSize, DeliveryDate, Department, AddedById) VALUES (@Sl_no, @Quotation_no, @Product_id, @Product_Code, @Product_name, @Quantity, @sail_rate, @Service_tax_rate, @Total_sail_rate, @Total_sail_rate1, @Total_sail_rate2, @specification, @Misc, @InvStatus, @Type, @Unit, @ProductOrServiceCat, @discount_rate, @new_sailrate, @ItemRemarks, @ItemNo, @MaterialNo, @PackSize, @DeliveryDate, @Department, @AddedById)", conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@Sl_no", h);
                                    cmd.Parameters.AddWithValue("@Quotation_no", lblqno.Text);
                                    cmd.Parameters.AddWithValue("@Product_id", ProductId);
                                    cmd.Parameters.AddWithValue("@Product_Code", Product_code);
                                    cmd.Parameters.AddWithValue("@Product_name", ProductName);
                                    cmd.Parameters.AddWithValue("@Quantity", Quantity);
                                    cmd.Parameters.AddWithValue("@sail_rate", Sail_Rate);
                                    cmd.Parameters.AddWithValue("@Service_tax_rate", Tax_Rate);
                                    cmd.Parameters.AddWithValue("@Total_sail_rate", Total_sail_rate);
                                    cmd.Parameters.AddWithValue("@Total_sail_rate1", Total_sail_rate1);
                                    cmd.Parameters.AddWithValue("@Total_sail_rate2", Total_sail_rate2);
                                    cmd.Parameters.AddWithValue("@specification", Brand);
                                    cmd.Parameters.AddWithValue("@Misc", Specification);
                                    cmd.Parameters.AddWithValue("@InvStatus", InvStatus);
                                    cmd.Parameters.AddWithValue("@Type", Type);
                                    cmd.Parameters.AddWithValue("@Unit", Unit);
                                    cmd.Parameters.AddWithValue("@ProductOrServiceCat", ProductOrServiceCat);
                                    cmd.Parameters.AddWithValue("@discount_rate", Discount_Rate);
                                    cmd.Parameters.AddWithValue("@new_sailrate", discounted_rate);
                                    cmd.Parameters.AddWithValue("@ItemRemarks", ItemRemarks);
                                    cmd.Parameters.AddWithValue("@ItemNo", ItemNo);
                                    cmd.Parameters.AddWithValue("@MaterialNo", MaterialNo);
                                    cmd.Parameters.AddWithValue("@PackSize", PackSize);
                                    cmd.Parameters.AddWithValue("@DeliveryDate", DeliveryDate);
                                    cmd.Parameters.AddWithValue("@Department", Department);
                                    cmd.Parameters.AddWithValue("@AddedById", userId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    // Capture quotation metadata
                    int vDays;
                    if (!int.TryParse(txt_valdays.Text?.Trim(), out vDays))
                    {
                        throw new ArgumentException("Invalid Validity Days");
                    }
                    int validDays = vDays;

                    string deliveryTenure = DDL_DeliveryTerms.SelectedValue == "4" ? txt_deltrms.Text?.Trim() : DDL_DeliveryTerms.SelectedItem.Text;
                    string packageForwarding = DDL_pkgfrwd.SelectedValue == "3" ? txt_pkgfrwd.Text?.Trim() : DDL_pkgfrwd.SelectedItem.Text;
                    string remarks = txt_remarks.Text?.Trim();
                    string itemview = DDL_ItemViewType.SelectedItem.Text?.Trim();
                    string referenceOption = rbYes.Checked ? "Yes" : "No";
                    string referenceName = referenceOption == "No" ? "N/A" : txt_clientrefname.Text?.Trim();
                    string referenceId = referenceOption == "No" ? "N/A" : txt_clientrefid.Text?.Trim();
                    string referenceDate = referenceOption == "No" ? "1900-01-01" : txt_clientrefdate.Text?.Trim();

                    string recordtyp = rbPo.Checked ? "Purchase Order" : "Quotation";
                    string DO_number = recordtyp == "Quotation" ? "N/A" : txb_donumber.Text?.Trim();
                    string PO_number = recordtyp == "Quotation" ? "N/A" : txb_ponumber.Text?.Trim();
                    string PO_Date = recordtyp == "Quotation" ? "1900-01-01" : txb_podate.Text?.Trim();
                    string ValStart_Date = recordtyp == "Quotation" ? "1900-01-01" : txb_strtdt.Text?.Trim();
                    string ValEnd_Date = recordtyp == "Quotation" ? "1900-01-01" : txb_enddt.Text?.Trim();

                    total_sail_rate_details = Math.Round(new_Gross_amount, 2);
                    new_total_Service = Math.Round(new_total_Service, 2);

                    using (SqlCommand cmd = new SqlCommand(@"INSERT INTO tbl_Quotation 
                (Quotation_no, Quotation_date, Client_Id, Gross, Service_tax, Net_amount, Status1, Status2, Sl_no, status3, service_tax1, sub_total, cgstOrsgst, igst, PlaceofSupply, PaymentStatus, ReferenceData, ReferenceName, ReferenceId, ReferenceDate, ValidityDays, DeliveryTenure, PackingCharges, Remarks, DetailedView, RecordType, DO_Number, PO_Number, PO_Date, Validity_StartDate, Validity_EndDate, AddedById, DiscountView)
                VALUES (@Quotation_no, @Quotation_date, @Client_Id, @Gross, @Service_tax, @Net_amount, 'No', 'No', @Sl_no, 'No', @service_tax1, @sub_total, @cgstOrsgst, @igst, @PlaceofSupply, 'No', @ReferenceData, @ReferenceName, @ReferenceId, @ReferenceDate, @ValidityDays, @DeliveryTenure, @PackingCharges, @Remarks, @DetailedView, @RecordType, @DO_Number, @PO_Number, @PO_Date, @Validity_StartDate, @Validity_EndDate, @AddedById, @DiscountView)", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@Quotation_no", lblqno.Text?.Trim());
                        cmd.Parameters.AddWithValue("@Quotation_date", txtquotationDate.Text?.Trim());
                        cmd.Parameters.AddWithValue("@Client_Id", cmbClient.SelectedValue.ToString());
                        cmd.Parameters.AddWithValue("@Gross", new_Gross_amount);
                        cmd.Parameters.AddWithValue("@Service_tax", new_total_Service);
                        cmd.Parameters.AddWithValue("@Net_amount", total_sail_rate_details);
                        cmd.Parameters.AddWithValue("@Sl_no", slNo);
                        cmd.Parameters.AddWithValue("@service_tax1", new_total_Service);
                        cmd.Parameters.AddWithValue("@sub_total", new_sub_total);
                        cmd.Parameters.AddWithValue("@cgstOrsgst", CGSTSGSTSTATUS);
                        cmd.Parameters.AddWithValue("@igst", IGSTSTATUS);
                        cmd.Parameters.AddWithValue("@PlaceofSupply", ddlPlaceOfSupply.Text?.Trim());
                        cmd.Parameters.AddWithValue("@ReferenceData", referenceOption);
                        cmd.Parameters.AddWithValue("@ReferenceName", referenceName);
                        cmd.Parameters.AddWithValue("@ReferenceId", referenceId);
                        cmd.Parameters.AddWithValue("@ReferenceDate", referenceDate);
                        cmd.Parameters.AddWithValue("@ValidityDays", validDays);
                        cmd.Parameters.AddWithValue("@DeliveryTenure", deliveryTenure);
                        cmd.Parameters.AddWithValue("@PackingCharges", packageForwarding);
                        cmd.Parameters.AddWithValue("@Remarks", remarks);
                        cmd.Parameters.AddWithValue("@DetailedView", itemview);
                        cmd.Parameters.AddWithValue("@RecordType", recordtyp);
                        cmd.Parameters.AddWithValue("@DO_Number", DO_number);
                        cmd.Parameters.AddWithValue("@PO_Number", PO_number);
                        cmd.Parameters.AddWithValue("@PO_Date", PO_Date);
                        cmd.Parameters.AddWithValue("@Validity_StartDate", ValStart_Date);
                        cmd.Parameters.AddWithValue("@Validity_EndDate", ValEnd_Date);
                        cmd.Parameters.AddWithValue("@AddedById", userId);
                        cmd.Parameters.AddWithValue("@DiscountView", DDL_DiscountView.SelectedItem.Text?.Trim());
                        cmd.ExecuteNonQuery();
                    }


                    insertPaymentPhaseNew(lblqno.Text, conn, trans);
                    insertprimaryServiceNew(lblqno.Text, conn, trans);

                    trans.Commit();

                    lblOk.Text = "Data Saved Successfully.....!  Agianst Record No :" + lblqno.Text.ToString();
                    PanelOK.Visible = true;
                    //Button3.Visible = false;
                }
                //catch (Exception ex)
                //{
                //    try { trans?.Rollback(); } catch { }
                //    lblErrorMsg.Text = "Error occurred: " + ex.Message;
                //    PanelError.Visible = true;
                //}

                catch (Exception ex)
                {
                    try { trans?.Rollback(); } catch { }

                    // Build a more complete error message
                    StringBuilder errorMsg = new StringBuilder();
                    errorMsg.AppendLine("An error occurred:");
                    errorMsg.AppendLine(ex.Message);

                    if (ex.InnerException != null)
                    {
                        errorMsg.AppendLine("Inner Exception:");
                        errorMsg.AppendLine(ex.InnerException.ToString());
                    }

                    errorMsg.AppendLine("Stack Trace:");
                    errorMsg.AppendLine(ex.StackTrace);

                    lblErrorMsg.Text = errorMsg.ToString().Replace(Environment.NewLine, "<br/>");
                    PanelError.Visible = true;
                }
            }
        }

        private void insertPaymentPhaseNew(string qutno, SqlConnection conn, SqlTransaction trans)
        {
            int totalRows = GridView3.Rows.Count;

            foreach (GridViewRow gvr in GridView3.Rows)
            {
                string phasetype = ((Label)gvr.Cells[0].FindControl("PaymentPhase")).Text;
                string phasedesc = ((TextBox)gvr.Cells[1].FindControl("PhaseDesc")).Text;
                string amo = ((TextBox)gvr.Cells[2].FindControl("AmountPer")).Text;

                // If only 1 row exists, set amo to "100"
                if (totalRows == 1)
                {
                    amo = "100";
                }

                string query = "INSERT INTO tbl_QutPaymentPhase(qut_no, phase_type, PhaseDesc, amountper) VALUES (@qut_no, @phase_type, @PhaseDesc, @amountper)";

                using (SqlCommand cmd = new SqlCommand(query, conn, trans))
                {
                    cmd.Parameters.AddWithValue("@qut_no", qutno);
                    cmd.Parameters.AddWithValue("@phase_type", phasetype);
                    cmd.Parameters.AddWithValue("@PhaseDesc", phasedesc);
                    cmd.Parameters.AddWithValue("@amountper", amo);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void insertprimaryServiceNew(string qutno, SqlConnection conn, SqlTransaction trans)
        {
            string PrimaryService = "";
            int i = 0;

            foreach (GridViewRow gvr in gridps.Rows)
            {
                string ProductCatagory = ((Label)gvr.Cells[0].FindControl("ProductCatagory")).Text;

                string query = "INSERT INTO tbl_QutPrimaryService(qut_no, PrimaryService) VALUES (@qut_no, @PrimaryService)";
                using (SqlCommand cmd = new SqlCommand(query, conn, trans))
                {
                    cmd.Parameters.AddWithValue("@qut_no", qutno);
                    cmd.Parameters.AddWithValue("@PrimaryService", ProductCatagory);
                    cmd.ExecuteNonQuery();
                }

                insertPrimaryServiceDescNew(qutno, ProductCatagory, conn, trans);

                ProductCatagory = "“" + ProductCatagory + "”";
                if (i == 0)
                {
                    PrimaryService = ProductCatagory;
                }
                else if (i == 1)
                {
                    PrimaryService = PrimaryService + " and " + ProductCatagory;
                }
                else
                {
                    PrimaryService = PrimaryService + " , " + ProductCatagory;
                }

                i++;
            }

            insertServiceTogetherNew(qutno, PrimaryService, conn, trans);
        }

        private void insertPrimaryServiceDescNew(string qutno, string ProductCatagory, SqlConnection conn, SqlTransaction trans)
        {
            string query = "SELECT PrimaryServiceTerms FROM tbl_PrimaryServiceTerms WHERE PrimaryService=@PrimaryService";

            using (SqlCommand cmd = new SqlCommand(query, conn, trans))
            {
                cmd.Parameters.AddWithValue("@PrimaryService", ProductCatagory);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dtSTerm = new DataTable();
                    da.Fill(dtSTerm);

                    foreach (DataRow row in dtSTerm.Rows)
                    {
                        string pSerTer = row["PrimaryServiceTerms"].ToString();

                        string query1 = "INSERT INTO tbl_QuoPserTerm (qutno, PServiceName, PSerTer) VALUES (@qutno, @PServiceName, @PSerTer)";
                        using (SqlCommand cmd1 = new SqlCommand(query1, conn, trans))
                        {
                            cmd1.Parameters.AddWithValue("@qutno", qutno);
                            cmd1.Parameters.AddWithValue("@PServiceName", ProductCatagory);
                            cmd1.Parameters.AddWithValue("@PSerTer", pSerTer);
                            cmd1.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private void insertServiceTogetherNew(string qutno, string primaryService, SqlConnection conn, SqlTransaction trans)
        {
            string query = "INSERT INTO tbl_QuoPriSerTogather (qutno, PServiceName) VALUES (@qutno, @PServiceName)";

            using (SqlCommand cmd = new SqlCommand(query, conn, trans))
            {
                cmd.Parameters.AddWithValue("@qutno", qutno);
                cmd.Parameters.AddWithValue("@PServiceName", primaryService);
                cmd.ExecuteNonQuery();
            }
        }

    }
}