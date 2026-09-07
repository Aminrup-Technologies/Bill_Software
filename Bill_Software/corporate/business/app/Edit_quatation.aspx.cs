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
                string cmdstring = "select Product_id as ProductID, Product_Code as Product_code, Product_name as ProductName, Type, sail_rate as Sail_Rate, Service_tax_rate as Tax_Rate, Unit, Quantity, ProductOrServiceCat, specification as Brand, Misc as specification, ItemNo, MaterialNo, PackSize, ItemRemarks, discount_rate, Sl_no, DeliveryDate, Department from tbl_Quotaion_details where Quotation_no=@Quotation_no AND IsLatest = 1 AND IsDeleted = 0 order by CAST(Sl_no as int)";
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
                ToggleGridColumns();
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
            public string CGSTorSGST { get; set; }
            public string IGST { get; set; }
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
            public decimal? TCSAmount { get; set; }
            public decimal? TCSPercent { get; set; }
            public decimal? FreightAmount { get; set; }
            public decimal? FreightVATPercent { get; set; }
            public string OtherChargeName { get; set; }
            public decimal? OtherChargeAmount { get; set; }

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
                        CGSTorSGST = reader["cgstOrsgst"].ToString(),
                        IGST = reader["igst"].ToString(),
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
                        // Add these specifically:
                        DiscountView = reader["DiscountView"]?.ToString(),

                        // Ensure your new financial fields are also mapped here
                        TCSAmount = reader["TCS_Amount"] != DBNull.Value ? Convert.ToDecimal(reader["TCS_Amount"]) : (decimal?)null,
                        TCSPercent = reader["TCS_Percent"] != DBNull.Value ? Convert.ToDecimal(reader["TCS_Percent"]) : (decimal?)null,
                        FreightAmount = reader["Freight_Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Freight_Amount"]) : (decimal?)null,
                        FreightVATPercent = reader["Freight_VAT_Percent"] != DBNull.Value ? Convert.ToDecimal(reader["Freight_VAT_Percent"]) : (decimal?)null,
                        OtherChargeName = reader["OtherCharge_Name"].ToString(),
                        OtherChargeAmount = reader["OtherCharge_Amount"] != DBNull.Value ? Convert.ToDecimal(reader["OtherCharge_Amount"]) : (decimal?)null
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
                // 1. Date Handling
                DateTime tempDate;
                if (DateTime.TryParseExact(q.QuotationDate, "dd-MMM-yyyy", null, System.Globalization.DateTimeStyles.None, out tempDate))
                {
                    txtquotationDate.Text = tempDate.ToString("dd-MMM-yyyy");
                }
                else
                {
                    txtquotationDate.Text = q.QuotationDate;
                }

                // 2. Metadata & Client
                txt_valdays.Text = q.ValidityDays?.ToString() ?? "0";

                ListItem itemView = DDL_ItemViewType.Items.FindByText(q.DetailedView);
                if (itemView != null) { DDL_ItemViewType.ClearSelection(); itemView.Selected = true; }

                if (cmbClient.Items.FindByValue(q.ClientId) != null)
                {
                    cmbClient.SelectedValue = q.ClientId;
                }

                ListItem discView = DDL_DiscountView.Items.FindByText(q.DiscountView);
                if (discView != null) { DDL_DiscountView.ClearSelection(); discView.Selected = true; }

                // 3. Delivery Tenure Logic
                ListItem deliveryItem = DDL_DeliveryTerms.Items.FindByText(q.DeliveryTenure);
                DDL_DeliveryTerms.ClearSelection();
                if (deliveryItem != null)
                {
                    deliveryItem.Selected = true;
                    manualInputRow.Style["display"] = "none";
                }
                else
                {
                    DDL_DeliveryTerms.SelectedValue = "4"; // Manual Input
                    txt_deltrms.Text = q.DeliveryTenure;
                    manualInputRow.Style["display"] = "";
                }

                // 4. Package Forwarding Logic
                ListItem pkgItem = DDL_pkgfrwd.Items.FindByText(q.PackingCharges);
                DDL_pkgfrwd.ClearSelection();
                if (pkgItem != null)
                {
                    pkgItem.Selected = true;
                    manualInputPkgRow.Style["display"] = "none";
                }
                else
                {
                    DDL_pkgfrwd.SelectedValue = "3"; // Manual Input
                    txt_pkgfrwd.Text = q.PackingCharges;
                    manualInputPkgRow.Style["display"] = "";
                }

                txt_remarks.Text = q.Remarks;

                // 5. Financial Fields (TCS, Freight, Other)
                txt_tcs_amnt.Text = q.TCSAmount.HasValue ? q.TCSAmount.Value.ToString("0.00") : "0.00";
                txt_tcs_percent.Text = q.TCSPercent.HasValue ? q.TCSPercent.Value.ToString("0.00") : "0.00";
                txt_delivery_amnt.Text = q.FreightAmount.HasValue ? q.FreightAmount.Value.ToString("0.00") : "0.00";
                txt_freight_percent.Text = q.FreightVATPercent.HasValue ? q.FreightVATPercent.Value.ToString("0.00") : "0.00";

                TextBox1.Text = q.OtherChargeName ?? string.Empty;
                txt_othr_amnt.Text = q.OtherChargeAmount.HasValue ? q.OtherChargeAmount.Value.ToString("0.00") : "0.00";

                // 6. Record Type & PO Fields
                if (q.RecordType == "Quotation")
                {
                    rbQt.Checked = true;
                    rbPo.Checked = false;
                    PO_DataInputs.Visible = false;
                    txb_ponumber.Text = txb_donumber.Text = txb_podate.Text = txb_strtdt.Text = txb_enddt.Text = "";
                }
                else
                {
                    rbQt.Checked = false;
                    rbPo.Checked = true;
                    PO_DataInputs.Visible = true;
                    txb_ponumber.Text = q.PO_Number;
                    txb_donumber.Text = q.DO_Number;
                    txb_podate.Text = FormatDate(q.PO_Date);
                    txb_strtdt.Text = FormatDate(q.ValidityStartDate);
                    txb_enddt.Text = FormatDate(q.ValidityEndDate);
                }

                // Static Protection
                rbQt.Enabled = rbPo.Enabled = false;

                // 7. Grid Parity Call
                ToggleGridColumns();

                // 8. Supply & GST
                ListItem supplyItem = ddlPlaceOfSupply.Items.FindByText(q.PlaceOfSupply);
                if (supplyItem != null) { ddlPlaceOfSupply.ClearSelection(); supplyItem.Selected = true; }

                if (!string.IsNullOrEmpty(q.CGSTorSGST) && q.CGSTorSGST.Trim().ToUpper() == "YES")
                {
                    RadioButtonGst.SelectedValue = "1"; // Select CGST/SGST
                }
                else
                {
                    RadioButtonGst.SelectedValue = "0"; // Default to IGST if column is empty or NULL
                }

                // 9. Reference Data Logic
                if (q.ReferenceData == "Yes")
                {
                    rbYes.Checked = true;
                    rbNo.Checked = false;
                    txt_clientrefname.Text = q.ReferenceName;
                    txt_clientrefid.Text = q.ReferenceId;
                    txt_clientrefname.ReadOnly = txt_clientrefid.ReadOnly = txt_clientrefdate.ReadOnly = false;

                    DateTime refDt;
                    txt_clientrefdate.Text = DateTime.TryParseExact(q.ReferenceDate, "dd-MMM-yyyy", null, System.Globalization.DateTimeStyles.None, out refDt)
                        ? refDt.ToString("dd-MMM-yyyy") : q.ReferenceDate;
                }
                else
                {
                    rbYes.Checked = false;
                    rbNo.Checked = true;
                    txt_clientrefname.Text = "N/A";
                    txt_clientrefid.Text = "N/A";
                    txt_clientrefdate.Text = "01-Jan-2000";
                    txt_clientrefname.ReadOnly = txt_clientrefid.ReadOnly = txt_clientrefdate.ReadOnly = true;
                }
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

        private object GetSafeDate(string dateText)
        {
            DateTime dt;
            // Tries to parse your specific format (dd-MMM-yyyy) or general formats
            if (DateTime.TryParseExact(dateText, "dd-MMM-yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out dt))
            {
                return dt;
            }
            if (DateTime.TryParse(dateText, out dt))
            {
                return dt;
            }
            // Return SQL-safe min date if empty or invalid
            return "1900-01-01";
        }

        private void DataUpdaterMethod()
        {
            string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";
            string qno = lblqno.Text;

            // 1. Validation Check (Proforma/Tax/Payment)
            string query = "select Status1, Status2, PaymentStatus from tbl_Quotation where Quotation_no=@Quotation_no";
            SqlParameter[] checkParam = { new SqlParameter("@Quotation_no", qno) };
            DataTable dtProInvPay = DbCL.SPreturn_dt(query, checkParam);

            if (dtProInvPay.Rows.Count > 0)
            {
                string pro = dtProInvPay.Rows[0]["Status1"].ToString();
                string inv = dtProInvPay.Rows[0]["Status2"].ToString();
                string pay = dtProInvPay.Rows[0]["PaymentStatus"].ToString();

                if (pro == "Yes" || inv == "Yes" || pay == "Yes")
                {
                    lblErrorMsg.Text = "Cannot update. Please delete associated invoices first.";
                    PanelError.Visible = true;
                    return;
                }
            }

            decimal new_sub_total = 0, new_total_Service = 0, new_Gross_amount = 0;

            // 2. Determine Next Integer Version
            string versionQuery = "SELECT ISNULL(MAX(Version), 0) + 1 FROM tbl_Quotaion_details WHERE Quotation_no = @Quotation_no";
            SqlParameter[] versionParam = { new SqlParameter("@Quotation_no", qno) };
            int newVersion = Convert.ToInt32(DbCL.ExecuteScalar(versionQuery, versionParam));

            // 3. Soft delete old latest records
            string softDeleteQuery = @"UPDATE tbl_Quotaion_details SET IsDeleted = 1, IsLatest = 0, DeletedById = @DeletedById, DeletedOn = GETDATE()
                              WHERE Quotation_no = @Quotation_no AND IsDeleted = 0 AND IsLatest = 1";
            SqlParameter[] softParams = {
        new SqlParameter("@Quotation_no", qno),
        new SqlParameter("@DeletedById", userId)
    };
            DbCL.ExecuteNonQuery(softDeleteQuery, softParams);

            // 4. Loop through Grid to Insert New Versions
            for (int i = 0; i < gd_Service_Product.Rows.Count; i++)
            {
                CheckBox chk = (CheckBox)gd_Service_Product.Rows[i].FindControl("chk");
                if (chk != null && chk.Checked)
                {
                    // Data Extraction
                    string ProductId = ((Label)gd_Service_Product.Rows[i].FindControl("ProductID"))?.Text ?? "";
                    string Product_code = ((Label)gd_Service_Product.Rows[i].FindControl("Product_code"))?.Text ?? "";
                    string ProductName = ((Label)gd_Service_Product.Rows[i].FindControl("ProductName"))?.Text ?? "";
                    string Brand = ((Label)gd_Service_Product.Rows[i].FindControl("Brand"))?.Text ?? "";
                    string ProductOrServiceCat = ((Label)gd_Service_Product.Rows[i].FindControl("ProductOrServiceCat"))?.Text ?? "";
                    string Type = ((Label)gd_Service_Product.Rows[i].FindControl("Type"))?.Text ?? "";
                    string Unit = ((Label)gd_Service_Product.Rows[i].FindControl("Unit"))?.Text ?? "";
                    string Specification = ((TextBox)gd_Service_Product.Rows[i].FindControl("Specification"))?.Text ?? "~";
                    string ItemNo = ((TextBox)gd_Service_Product.Rows[i].FindControl("ItemNo"))?.Text ?? "";
                    string MaterialNo = ((TextBox)gd_Service_Product.Rows[i].FindControl("MaterialNo"))?.Text ?? "";
                    string PackSize = ((TextBox)gd_Service_Product.Rows[i].FindControl("PackSize"))?.Text ?? "";
                    string ItemRemarks = ((TextBox)gd_Service_Product.Rows[i].FindControl("ItemRemarks"))?.Text ?? "";
                    string DeliveryDate = ((TextBox)gd_Service_Product.Rows[i].FindControl("DeliveryDate"))?.Text ?? "";
                    string Department = ((TextBox)gd_Service_Product.Rows[i].FindControl("Department"))?.Text ?? "";
                    string slNo = ((TextBox)gd_Service_Product.Rows[i].FindControl("txtOrder"))?.Text ?? "0";

                    decimal Quantity = ParseDecimal(((TextBox)gd_Service_Product.Rows[i].FindControl("Quantity")).Text);
                    decimal Sail_Rate = ParseDecimal(((TextBox)gd_Service_Product.Rows[i].FindControl("Sail_Rate")).Text);
                    decimal Tax_Rate = ParseDecimal(((Label)gd_Service_Product.Rows[i].FindControl("Tax_Rate")).Text);
                    decimal Discount_Rate = ParseDecimal(((TextBox)gd_Service_Product.Rows[i].FindControl("Discount_Rate")).Text);

                    // Calculations
                    decimal discounted_rate = Sail_Rate - (Sail_Rate * Discount_Rate / 100);
                    decimal rowTaxAmt = (Tax_Rate * discounted_rate) / 100;
                    decimal rateIncl = discounted_rate + rowTaxAmt;

                    new_sub_total += (discounted_rate * Quantity);
                    new_total_Service += (rowTaxAmt * Quantity);
                    new_Gross_amount += (rateIncl * Quantity);

                    SqlParameter[] insertParams = {
                new SqlParameter("@Sl_no", slNo),
                new SqlParameter("@Quotation_no", qno),
                new SqlParameter("@Product_id", ProductId),
                new SqlParameter("@Product_Code", Product_code),
                new SqlParameter("@Product_name", ProductName),
                new SqlParameter("@Quantity", Quantity),
                new SqlParameter("@sail_rate", Sail_Rate),
                new SqlParameter("@Service_tax_rate", Tax_Rate),
                new SqlParameter("@discount_rate", Discount_Rate),
                new SqlParameter("@new_sailrate", discounted_rate),
                new SqlParameter("@Total_sail_rate", rateIncl),
                new SqlParameter("@Total_sail_rate1", rateIncl * Quantity),
                new SqlParameter("@Total_sail_rate2", discounted_rate * Quantity),
                new SqlParameter("@specification", Brand),
                new SqlParameter("@Misc", Specification),
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
                 @Total_sail_rate, @Total_sail_rate1, @Total_sail_rate2, @specification, @Misc, 'No', @Type, @Unit, @ProductOrServiceCat, 
                 @ItemRemarks, @ItemNo, @MaterialNo, @PackSize, @DeliveryDate, @Department, @AddedById, GETDATE(), @Version, 0, 1)";

                    DbCL.ExecuteNonQuery(insertQry, insertParams);
                }
            }

            // 5. Update Header Table (tbl_Quotation)
            decimal tcsAmount = ParseDecimal(txt_tcs_amnt.Text);
            decimal tcsPercent = ParseDecimal(txt_tcs_percent.Text);
            decimal deliveryAmount = ParseDecimal(txt_delivery_amnt.Text);
            decimal freightPercent = ParseDecimal(txt_freight_percent.Text);
            decimal otherAmount = ParseDecimal(txt_othr_amnt.Text);
            string otherChargeName = TextBox1.Text.Trim();

            decimal finalNet = Math.Round(new_Gross_amount + tcsAmount + deliveryAmount + otherAmount, 2);

            string updateHeader = @"UPDATE tbl_Quotation SET 
        Gross = @Gross, Service_tax = @STax, Net_amount = @Net, service_tax1 = @STax1, sub_total = @SubT,
        ValidityDays = @VDays, DeliveryTenure = @DTenure, PackingCharges = @PCharge, 
        cgstOrsgst = @CGST, igst = @IGST, PlaceofSupply = @POS, 
        ReferenceData = @RefD, ReferenceName = @RefN, ReferenceId = @RefI, ReferenceDate = @RefDt,
        Remarks = @Remarks, DetailedView = @DView, DiscountView = @DiscView, 
        RecordType = @RType, DO_Number = @DONum, PO_Number = @PONum, PO_Date = @PODt, 
        Validity_StartDate = @VStart, Validity_EndDate = @VEnd,
        TCS_Amount = @TCSA, TCS_Percent = @TCSP, Freight_Amount = @FrA, Freight_VAT_Percent = @FrP, 
        OtherCharge_Name = @OthName, OtherCharge_Amount = @OthAmnt,
        ModifiedById = @ModBy, ModifiedOn = GETDATE() 
        WHERE Quotation_no = @QNo";

            SqlParameter[] headParams = {
        new SqlParameter("@Gross", new_Gross_amount),
        new SqlParameter("@STax", new_total_Service),
        new SqlParameter("@Net", finalNet),
        new SqlParameter("@STax1", new_total_Service),
        new SqlParameter("@SubT", new_sub_total),
        new SqlParameter("@VDays", txt_valdays.Text),
        new SqlParameter("@DTenure", DDL_DeliveryTerms.SelectedValue == "4" ? txt_deltrms.Text : DDL_DeliveryTerms.SelectedItem.Text),
        new SqlParameter("@PCharge", DDL_pkgfrwd.SelectedValue == "3" ? txt_pkgfrwd.Text : DDL_pkgfrwd.SelectedItem.Text),
        new SqlParameter("@CGST", RadioButtonGst.SelectedValue == "1" ? "YES" : ""),
        new SqlParameter("@IGST", RadioButtonGst.SelectedValue == "0" ? "YES" : ""),
        new SqlParameter("@POS", ddlPlaceOfSupply.SelectedItem.Text),
        new SqlParameter("@RefD", rbYes.Checked ? "Yes" : "No"),
        new SqlParameter("@RefN", rbYes.Checked ? txt_clientrefname.Text : "N/A"),
        new SqlParameter("@RefI", rbYes.Checked ? txt_clientrefid.Text : "N/A"),
        new SqlParameter("@RefDt", rbYes.Checked ? GetSafeDate(txt_clientrefdate.Text) : "1900-01-01"),
        new SqlParameter("@Remarks", txt_remarks.Text),
        new SqlParameter("@DView", DDL_ItemViewType.SelectedItem.Text),
        new SqlParameter("@DiscView", DDL_DiscountView.SelectedItem.Text),
        new SqlParameter("@RType", rbPo.Checked ? "Purchase Order" : "Quotation"),
        new SqlParameter("@DONum", rbPo.Checked ? txb_donumber.Text : "N/A"),
        new SqlParameter("@PONum", rbPo.Checked ? txb_ponumber.Text : "N/A"),
        new SqlParameter("@PODt", rbPo.Checked ? GetSafeDate(txb_podate.Text) : "1900-01-01"),
        new SqlParameter("@VStart", rbPo.Checked ? GetSafeDate(txb_strtdt.Text) : "1900-01-01"),
        new SqlParameter("@VEnd", rbPo.Checked ? GetSafeDate(txb_enddt.Text) : "1900-01-01"),
        new SqlParameter("@TCSA", tcsAmount),
        new SqlParameter("@TCSP", tcsPercent),
        new SqlParameter("@FrA", deliveryAmount),
        new SqlParameter("@FrP", freightPercent),
        new SqlParameter("@OthName", otherChargeName),
        new SqlParameter("@OthAmnt", otherAmount),
        new SqlParameter("@ModBy", userId),
        new SqlParameter("@QNo", qno)
    };

            DbCL.ExecuteNonQuery(updateHeader, headParams);

            updatedueamountdetails(Convert.ToDouble(finalNet));
            lblOk.Text = "Data Updated Successfully (Version " + newVersion + ")";
            PanelOK.Visible = true;
            btnSabe.Visible = false;
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
            //ToggleGridColumns();
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
                if (dtPCat.Columns.Contains("DeliveryDate") == false)
                {
                    dtPCat.Columns.Add("DeliveryDate", typeof(string));
                    dtPCat.Columns.Add("Department", typeof(string));
                }
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
            // When adding the row, ensure these are mapped:
            dr["DeliveryDate"] = DeliveryDate; // from parameter
            dr["Department"] = Department;     // from parameter

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

        //private void Bindquotationno_OLD()
        //{
        //    string prefix = "QTN/FE/";
        //    if (rbPo.Checked)
        //    {
        //        prefix = "PO/FE/";
        //    }

        //    string ss = findmonth();  // e.g., "24-25/"
        //    int j = idreturn_New(prefix + ss);  // Get last used number for that prefix

        //    string quotationNo;
        //    do
        //    {
        //        j += 1;
        //        quotationNo = prefix + ss + j.ToString();
        //    }
        //    while (QuotationNoExists(quotationNo));  // Keep looping if already exists

        //    lblqno.Text = quotationNo;
        //}

        private void Bindquotationno()
        {
            // 1. Get the record number currently being edited from the label
            string currentNo = lbl_recordno.Text.Trim(); // e.g., "PO/FE/25-26/68" or "PO/FE/25-26/68A"

            string newQuotationNo = "";

            // 2. Identify the last character
            char lastChar = currentNo[currentNo.Length - 1];

            // 3. Logic: If the last character is NOT a digit, it's already a version (A, B, C...)
            if (!char.IsDigit(lastChar))
            {
                // Increment the letter (A -> B, B -> C)
                char nextLetter = (char)(lastChar + 1);
                newQuotationNo = currentNo.Substring(0, currentNo.Length - 1) + nextLetter;
            }
            else
            {
                // It's the original base record (ends in a number), so start with 'A'
                newQuotationNo = currentNo + "A";
            }

            // 4. Final Safety: Check if this generated number somehow already exists in DB
            while (QuotationNoExists(newQuotationNo))
            {
                lastChar = newQuotationNo[newQuotationNo.Length - 1];
                newQuotationNo = newQuotationNo.Substring(0, newQuotationNo.Length - 1) + (char)(lastChar + 1);
            }

            lblqno.Text = newQuotationNo;
        }

        private bool QuotationNoExists_New(string qno)
        {
            string query = "SELECT COUNT(*) FROM tbl_Quotation WHERE Quotation_no = @QNo";
            SqlParameter[] p = { new SqlParameter("@QNo", qno) };
            return Convert.ToInt32(DbCL.ExecuteScalar(query, p)) > 0;
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
            // 1. Generate the new ID (e.g., QT/68A)
            Bindquotationno();
            string newRecordID = lblqno.Text; // Use this variable for consistency

            // 2. Setup Variables
            string CGSTSGSTSTATUS = RadioButtonGst.SelectedValue == "1" ? "YES" : "";
            string IGSTSTATUS = RadioButtonGst.SelectedValue == "0" ? "YES" : "";
            int slNo = idreturn() + 1;
            string userId = HttpContext.Current.Session["USERID"]?.ToString() ?? "FLM03";

            decimal new_sub_total = 0, new_total_Service = 0, new_Gross_amount = 0;
            string cnnString = System.Configuration.ConfigurationManager.ConnectionStrings["DbConn"].ToString();

            using (SqlConnection conn = new SqlConnection(cnnString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    // PART A: Insert Item Details from GridView
                    for (int i = 0; i < gd_Service_Product.Rows.Count; i++)
                    {
                        CheckBox chk = (CheckBox)gd_Service_Product.Rows[i].FindControl("chk");
                        if (chk != null && chk.Checked)
                        {
                            // Data Extraction
                            string ProductId = ((Label)gd_Service_Product.Rows[i].FindControl("ProductID"))?.Text ?? "";
                            string Product_code = ((Label)gd_Service_Product.Rows[i].FindControl("Product_code"))?.Text ?? "";
                            string ProductName = ((Label)gd_Service_Product.Rows[i].FindControl("ProductName"))?.Text ?? "";
                            string Brand = ((Label)gd_Service_Product.Rows[i].FindControl("Brand"))?.Text ?? "";
                            string ProductOrServiceCat = ((Label)gd_Service_Product.Rows[i].FindControl("ProductOrServiceCat"))?.Text ?? "";
                            string Type = ((Label)gd_Service_Product.Rows[i].FindControl("Type"))?.Text ?? "";
                            string Unit = ((Label)gd_Service_Product.Rows[i].FindControl("Unit"))?.Text ?? "";
                            string Specification = ((TextBox)gd_Service_Product.Rows[i].FindControl("Specification"))?.Text ?? "~";
                            string ItemNo = ((TextBox)gd_Service_Product.Rows[i].FindControl("ItemNo"))?.Text ?? "";
                            string MaterialNo = ((TextBox)gd_Service_Product.Rows[i].FindControl("MaterialNo"))?.Text ?? "";
                            string PackSize = ((TextBox)gd_Service_Product.Rows[i].FindControl("PackSize"))?.Text ?? "";
                            string ItemRemarks = ((TextBox)gd_Service_Product.Rows[i].FindControl("ItemRemarks"))?.Text ?? "";
                            string DeliveryDate = ((TextBox)gd_Service_Product.Rows[i].FindControl("DeliveryDate"))?.Text ?? "";
                            string Department = ((TextBox)gd_Service_Product.Rows[i].FindControl("Department"))?.Text ?? "";
                            string sortOrder = ((TextBox)gd_Service_Product.Rows[i].FindControl("txtOrder"))?.Text ?? "0";

                            decimal Quantity = ParseDecimal(((TextBox)gd_Service_Product.Rows[i].FindControl("Quantity")).Text);
                            decimal Sail_Rate = ParseDecimal(((TextBox)gd_Service_Product.Rows[i].FindControl("Sail_Rate")).Text);
                            decimal Tax_Rate = ParseDecimal(((Label)gd_Service_Product.Rows[i].FindControl("Tax_Rate")).Text);
                            decimal Discount_Rate = ParseDecimal(((TextBox)gd_Service_Product.Rows[i].FindControl("Discount_Rate")).Text);

                            // Calculations
                            decimal discounted_rate = Sail_Rate - (Sail_Rate * Discount_Rate / 100);
                            decimal rowTaxAmt = (Tax_Rate * discounted_rate) / 100;
                            decimal rateIncl = discounted_rate + rowTaxAmt;

                            new_sub_total += (discounted_rate * Quantity);
                            new_total_Service += (rowTaxAmt * Quantity);
                            new_Gross_amount += (rateIncl * Quantity);

                            string itemInsert = @"INSERT INTO tbl_Quotaion_details 
                        (Sl_no, Quotation_no, Product_id, Product_Code, Product_name, Quantity, sail_rate, Service_tax_rate, Total_sail_rate, Total_sail_rate1, Total_sail_rate2, specification, Misc, InvStatus, Type, Unit, ProductOrServiceCat, discount_rate, new_sailrate, ItemRemarks, ItemNo, MaterialNo, PackSize, DeliveryDate, Department, AddedById, Version, IsLatest, IsDeleted) 
                        VALUES (@Sl_no, @Quotation_no, @Product_id, @Product_Code, @Product_name, @Quantity, @sail_rate, @Service_tax_rate, @Total_sail_rate, @Total_sail_rate1, @Total_sail_rate2, @specification, @Misc, 'No', @Type, @Unit, @ProductOrServiceCat, @discount_rate, @new_sailrate, @ItemRemarks, @ItemNo, @MaterialNo, @PackSize, @DeliveryDate, @Department, @AddedById, 1, 1, 0)";

                            using (SqlCommand cmd = new SqlCommand(itemInsert, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@Sl_no", sortOrder);
                                cmd.Parameters.AddWithValue("@Quotation_no", newRecordID);
                                cmd.Parameters.AddWithValue("@Product_id", ProductId);
                                cmd.Parameters.AddWithValue("@Product_Code", Product_code);
                                cmd.Parameters.AddWithValue("@Product_name", ProductName);
                                cmd.Parameters.AddWithValue("@Quantity", Quantity);
                                cmd.Parameters.AddWithValue("@sail_rate", Sail_Rate);
                                cmd.Parameters.AddWithValue("@Service_tax_rate", Tax_Rate);
                                cmd.Parameters.AddWithValue("@Total_sail_rate", rateIncl);
                                cmd.Parameters.AddWithValue("@Total_sail_rate1", rateIncl * Quantity);
                                cmd.Parameters.AddWithValue("@Total_sail_rate2", discounted_rate * Quantity);
                                cmd.Parameters.AddWithValue("@specification", Brand);
                                cmd.Parameters.AddWithValue("@Misc", Specification);
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

                    // PART B: Calculate Header Totals
                    decimal tcsAmount = ParseDecimal(txt_tcs_amnt.Text);
                    decimal tcsPercent = ParseDecimal(txt_tcs_percent.Text);
                    decimal deliveryAmount = ParseDecimal(txt_delivery_amnt.Text);
                    decimal freightPercent = ParseDecimal(txt_freight_percent.Text);
                    decimal otherAmount = ParseDecimal(txt_othr_amnt.Text);

                    decimal finalNet = Math.Round(new_Gross_amount + tcsAmount + deliveryAmount + otherAmount, 2);

                    // PART C: Insert Header
                    string headerInsert = @"INSERT INTO tbl_Quotation 
                (Quotation_no, Quotation_date, Client_Id, Gross, Service_tax, Net_amount, Status1, Status2, Sl_no, status3, service_tax1, sub_total, cgstOrsgst, igst, PlaceofSupply, PaymentStatus, ReferenceData, ReferenceName, ReferenceId, ReferenceDate, ValidityDays, DeliveryTenure, PackingCharges, Remarks, DetailedView, RecordType, DO_Number, PO_Number, PO_Date, Validity_StartDate, Validity_EndDate, AddedById, DiscountView, TCS_Amount, TCS_Percent, Freight_Amount, Freight_VAT_Percent, OtherCharge_Name, OtherCharge_Amount, IsLatest, Version)
                VALUES (@QNo, @QDate, @CId, @Gross, @STax, @Net, 'No', 'No', @Sl, 'No', @STax1, @Sub, @CGST, @IGST, @POS, 'No', @RefD, @RefN, @RefI, @RefDt, @VDays, @DTenure, @PCharge, @Rem, @DView, @RType, @DO, @PO, @PODate, @VStart, @VEnd, @UserId, @DiscView, @TCSA, @TCSP, @FrA, @FrP, @OthN, @OthA, 1, 1)";

                    using (SqlCommand cmd = new SqlCommand(headerInsert, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@QNo", newRecordID);
                        cmd.Parameters.AddWithValue("@QDate", GetSafeDate(txtquotationDate.Text));
                        cmd.Parameters.AddWithValue("@CId", cmbClient.SelectedValue);
                        cmd.Parameters.AddWithValue("@Gross", new_Gross_amount);
                        cmd.Parameters.AddWithValue("@STax", new_total_Service);
                        cmd.Parameters.AddWithValue("@Net", finalNet);
                        cmd.Parameters.AddWithValue("@Sl", slNo);
                        cmd.Parameters.AddWithValue("@STax1", new_total_Service);
                        cmd.Parameters.AddWithValue("@Sub", new_sub_total);
                        cmd.Parameters.AddWithValue("@CGST", CGSTSGSTSTATUS);
                        cmd.Parameters.AddWithValue("@IGST", IGSTSTATUS);
                        cmd.Parameters.AddWithValue("@POS", ddlPlaceOfSupply.SelectedItem.Text);
                        cmd.Parameters.AddWithValue("@RefD", rbYes.Checked ? "Yes" : "No");
                        cmd.Parameters.AddWithValue("@RefN", rbYes.Checked ? txt_clientrefname.Text : "N/A");
                        cmd.Parameters.AddWithValue("@RefI", rbYes.Checked ? txt_clientrefid.Text : "N/A");
                        cmd.Parameters.AddWithValue("@RefDt", rbYes.Checked ? GetSafeDate(txt_clientrefdate.Text) : "1900-01-01");
                        cmd.Parameters.AddWithValue("@VDays", txt_valdays.Text);
                        cmd.Parameters.AddWithValue("@DTenure", DDL_DeliveryTerms.SelectedValue == "4" ? txt_deltrms.Text : DDL_DeliveryTerms.SelectedItem.Text);
                        cmd.Parameters.AddWithValue("@PCharge", DDL_pkgfrwd.SelectedValue == "3" ? txt_pkgfrwd.Text : DDL_pkgfrwd.SelectedItem.Text);
                        cmd.Parameters.AddWithValue("@Rem", txt_remarks.Text);
                        cmd.Parameters.AddWithValue("@DView", DDL_ItemViewType.SelectedItem.Text);
                        cmd.Parameters.AddWithValue("@RType", rbPo.Checked ? "Purchase Order" : "Quotation");
                        cmd.Parameters.AddWithValue("@DO", rbPo.Checked ? txb_donumber.Text : "N/A");
                        cmd.Parameters.AddWithValue("@PO", rbPo.Checked ? txb_ponumber.Text : "N/A");
                        cmd.Parameters.AddWithValue("@PODate", rbPo.Checked ? GetSafeDate(txb_podate.Text) : "1900-01-01");
                        cmd.Parameters.AddWithValue("@VStart", rbPo.Checked ? GetSafeDate(txb_strtdt.Text) : "1900-01-01");
                        cmd.Parameters.AddWithValue("@VEnd", rbPo.Checked ? GetSafeDate(txb_enddt.Text) : "1900-01-01");
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@DiscView", DDL_DiscountView.SelectedItem.Text);
                        cmd.Parameters.AddWithValue("@TCSA", tcsAmount);
                        cmd.Parameters.AddWithValue("@TCSP", tcsPercent);
                        cmd.Parameters.AddWithValue("@FrA", deliveryAmount);
                        cmd.Parameters.AddWithValue("@FrP", freightPercent);
                        cmd.Parameters.AddWithValue("@OthN", TextBox1.Text.Trim());
                        cmd.Parameters.AddWithValue("@OthA", otherAmount);
                        cmd.ExecuteNonQuery();
                    }

                    // PART D: Auxiliary Data
                    insertPaymentPhaseNew(newRecordID, conn, trans);
                    insertprimaryServiceNew(newRecordID, conn, trans);

                    trans.Commit();

                    // Sync labels
                    lbl_recordno.Text = newRecordID;

                    lblOk.Text = "New Record Created: " + newRecordID;
                    PanelOK.Visible = true;
                    btnSabe.Visible = btnNew.Visible = false;
                }
                catch (Exception ex)
                {
                    if (trans != null) trans.Rollback();
                    lblErrorMsg.Text = "Error: " + ex.Message;
                    PanelError.Visible = true;
                }
            }
        }

        private decimal ParseDecimal(string text)
        {
            decimal value;
            // Changed to NumberStyles.Any to accept Leading/Trailing spaces and Negative numbers
            if (decimal.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
            {
                return value;
            }
            // fallback to 0
            return 0m;
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

                string query = "INSERT INTO tbl_QutPrimaryService(qut_no, PrimaryService, TimeStamp, CompanyID) VALUES (@qut_no, @PrimaryService, GETDATE(), @CompanyID)";
                using (SqlCommand cmd = new SqlCommand(query, conn, trans))
                {
                    cmd.Parameters.AddWithValue("@qut_no", qutno);
                    cmd.Parameters.AddWithValue("@PrimaryService", ProductCatagory);
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyContext.CurrentCompanyID);
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

        protected void RadioButtonGst_SelectedIndexChanged(object sender, EventArgs e)
        {
            // If the user changes GST type, we re-run the column toggle or calculations if necessary
            ToggleGridColumns();
        }
    }
}