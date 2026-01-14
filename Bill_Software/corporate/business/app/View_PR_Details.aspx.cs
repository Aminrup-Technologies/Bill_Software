using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace Bill_Software.corporate.business.app
{
    public partial class View_PR_Details : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        public DataTable first_datatable;
        public static DataTable Dt = new DataTable("Table");
        private List<string> vatRates;
        private List<string> serviceTaxRates;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                string reqNo = Request.QueryString["reqNo"];
                if (!string.IsNullOrEmpty(reqNo))
                {
                    DbCL.FillCombo(cmbvendor, "select Vendor_Name from tbl_Vendor order by Vendor_Name");
                    LoadPR(reqNo);
                }
                else
                {
                    //Hide full page
                }

            }
        }

        private void LoadPR(string reqNo)
        {
            CurrentReqNo = reqNo;

            using (SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                con.Open();

                // Header
                SqlCommand cmdHdr = new SqlCommand("SELECT * FROM tbl_RequisitionMain WHERE ReqNo=@ReqNo", con);
                cmdHdr.Parameters.AddWithValue("@ReqNo", reqNo);

                SqlDataReader dr = cmdHdr.ExecuteReader();
                if (dr.Read())
                {
                    lblReqNo.Text = reqNo;
                    lblStatus.Text = dr["Status"].ToString();
                    cmbvendor.Text = dr["clientName"].ToString();
                    String VendorId = dr["VendorId"].ToString();
                    BindVendor(VendorId);
                    //if (dr["Status"].ToString() != "Draft")
                    //{
                    //    ApplyStatusUI(dr["Status"].ToString());
                    //}
                    ApplyStatusUI(dr["Status"].ToString());
                }
                dr.Close();

                SqlDataAdapter da = new SqlDataAdapter("SELECT id, ProductId AS Ser_pro_code, ProductName as Ser_pro_Name, ParentCategoryId, Description, Qnty, Rate, DiscountPercent, DiscountAmount, TaxableAmount, IsTaxApplicable, gstrate, ItemOrder FROM tbl_RequisitionNew WHERE ReqNo = @ReqNo ORDER BY ItemOrder", con);
                da.SelectCommand.Parameters.AddWithValue("@ReqNo", reqNo);

                DataTable dt = new DataTable();
                da.Fill(dt);
                gd_Service_Product.DataSource = dt;
                gd_Service_Product.DataBind();

                Panel2.Visible = true;
            }
            CalculatePRSummary_DB(CurrentReqNo);
            //ApplyStatusUI(lblStatus.Text);


        }

        private void ShowSuccess(string message)
        {
            PanelOK.Visible = true;
            PanelError.Visible = false;

            lblOk.Text = message;
        }

        private void ShowError(string message)
        {
            PanelError.Visible = true;
            PanelOK.Visible = false;

            lblErrorMsg.Text = message;
        }

        private void ClearMessages()
        {
            PanelOK.Visible = false;
            PanelError.Visible = false;

            lblOk.Text = "";
            lblErrorMsg.Text = "";
        }

        protected void cmbvendor_SelectedIndexChanged(object sender, EventArgs e)
        {
            Label1.Visible = true;
            RadioButtonList1.Visible = true;
            Button1.Visible = true;
            PurchaseType_Row.Visible = true;

            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Vendor where Vendor_Name='" + cmbvendor.Text + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lbl_vendordbid.Text = re["Id"].ToString();
                lblvendor_id.Text = re["Vendor_Id"].ToString();
                txtAddress1.Text = re["Address1"].ToString();
                txtAddress2.Text = re["Address2"].ToString();
                cmbcity.Text = re["City"].ToString();
                txtPin.Text = re["pin"].ToString();
                cmbState.Text = re["State"].ToString();
                txtWebsite.Text = re["Com_web_site"].ToString();
                txtEmail.Text = re["Com_email"].ToString();
                txtPhone.Text = re["Com_phone"].ToString();
                txtFax.Text = re["Com_Fax"].ToString();
                txtRepresentativeName.Text = re["Rep_Name"].ToString();
                txtRepresantativeDesig.Text = re["Rep_Desig"].ToString();
                txtRepresentativePhone.Text = re["Rep_phone"].ToString();
                txtRepresentativeEmail.Text = re["Rep_email"].ToString();
                txtservicetaxNo.Text = re["Service_tax_No"].ToString();
                txtpanNo.Text = re["Pan_No"].ToString();
                txtvat.Text = re["Vat_No"].ToString();
            }
            DbCL.Conn.Close();
        }

        protected void BindVendor(String VendorId)
        {
            Label1.Visible = true;
            RadioButtonList1.Visible = true;
            Button1.Visible = true;
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            string cmdstring = "select * from tbl_Vendor where Id='" + VendorId + "'";
            SqlCommand cmd = new SqlCommand(cmdstring, DbCL.Conn);
            SqlDataReader re = cmd.ExecuteReader();
            if (re.Read())
            {
                lbl_vendordbid.Text = re["Id"].ToString();
                cmbvendor.ClearSelection();

                ListItem item = cmbvendor.Items.FindByText(re["Vendor_Name"].ToString());
                if (item != null)
                {
                    item.Selected = true;
                }

                lblvendor_id.Text = re["Vendor_Id"].ToString();
                txtAddress1.Text = re["Address1"].ToString();
                txtAddress2.Text = re["Address2"].ToString();
                cmbcity.Text = re["City"].ToString();
                txtPin.Text = re["pin"].ToString();
                cmbState.Text = re["State"].ToString();
                txtWebsite.Text = re["Com_web_site"].ToString();
                txtEmail.Text = re["Com_email"].ToString();
                txtPhone.Text = re["Com_phone"].ToString();
                txtFax.Text = re["Com_Fax"].ToString();
                txtRepresentativeName.Text = re["Rep_Name"].ToString();
                txtRepresantativeDesig.Text = re["Rep_Desig"].ToString();
                txtRepresentativePhone.Text = re["Rep_phone"].ToString();
                txtRepresentativeEmail.Text = re["Rep_email"].ToString();
                txtservicetaxNo.Text = re["Service_tax_No"].ToString();
                txtpanNo.Text = re["Pan_No"].ToString();
                txtvat.Text = re["Vat_No"].ToString();
            }
            DbCL.Conn.Close();
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

            string cmdstring = "SELECT Id, ProductOrServiceCat FROM tbl_NewparentProduct ORDER BY Id";

            SqlDataAdapter da = new SqlDataAdapter(cmdstring, DbCL.Conn);
            DataTable dt = new DataTable();
            da.Fill(dt);

            cmbproduct_service.DataSource = dt;
            cmbproduct_service.DataTextField = "ProductOrServiceCat";
            cmbproduct_service.DataValueField = "Id";
            cmbproduct_service.DataBind();

            cmbproduct_service.Items.Insert(0, new ListItem("--Select--", "0"));

            DbCL.Conn.Close();
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            string prevValue = cmbproduct_service.SelectedValue;

            LoadTaxRates();

            int selectedId = Convert.ToInt32(prevValue);
            string selectedText = cmbproduct_service.SelectedItem.Text;

            gridtable.Visible = true;
            Panel2.Visible = true;
            SearchBox_Row.Visible = true;
            SearchBox_Msg.Visible = true;
            Modifier_Msg_Row.Visible = true;

            if (RadioButtonList1.SelectedIndex == 0)
            {
                Binddata1("SELECT ProductID, ProductName FROM tbl_NewProduct WHERE ParentId = " + selectedId);
            }
            else
            {
                Binddata1("SELECT Service_code, Service_name FROM tbl_Service WHERE Service_name = '" + selectedText + "'");
            }

            gd_Service_Product.DataSource = Dt;
            gd_Service_Product.DataBind();
            ViewState["dt"] = Dt;

            cmbproduct_service.SelectedValue = prevValue; // restore


        }

        private void LoadTaxRates()
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();

            // Fetch VAT Rates
            string vatQuery = "Select Vat_Rate from tbl_Vat_Master";
            SqlCommand vatCmd = new SqlCommand(vatQuery, DbCL.Conn);
            SqlDataReader vatRdr = vatCmd.ExecuteReader();

            vatRates = new List<string> { "NA" }; // Initialize with "NA"
            while (vatRdr.Read())
            {
                vatRates.Add(vatRdr[0].ToString());
            }
            vatRdr.Close();

            // Fetch Service Tax Rates
            string serviceTaxQuery = "Select Service_tax from tbl_Service_master";
            SqlCommand serviceTaxCmd = new SqlCommand(serviceTaxQuery, DbCL.Conn);
            SqlDataReader serviceTaxRdr = serviceTaxCmd.ExecuteReader();

            serviceTaxRates = new List<string> { "NA" }; // Initialize with "NA"
            while (serviceTaxRdr.Read())
            {
                serviceTaxRates.Add(serviceTaxRdr[0].ToString());
            }
            serviceTaxRdr.Close();

            DbCL.Conn.Close();
        }

        private void Binddata1(string cmdstring)
        {
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            try
            {
                // Create and configure the SqlCommand
                SqlCommand com1 = new SqlCommand(cmdstring, DbCL.Conn);

                // Use SqlDataAdapter to fill the DataTable directly
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(com1);
                da.Fill(dt); // Fill the DataTable with data

                // Check if the DataTable has rows to process
                if (dt.Rows.Count > 0)
                {
                    first_datatable = dt;

                    // Call the appropriate grid function based on Label2.Text
                    if (Label2.Text == "1")
                    {
                        newgrid1();
                    }
                    else
                    {
                        newgrid();
                    }

                    // Update Label2 to ensure the function executes only once
                    Label2.Text = (Convert.ToInt32(Label2.Text) + 1).ToString();
                }
            }
            finally
            {
                // Close the database connection in the finally block to ensure it always closes
                DbCL.Conn.Close();
            }
        }

        private void newgrid1()
        {
            DataTable dt = first_datatable;

            // Ensure Dt is initialized
            if (Dt == null)
                Dt = new DataTable();

            // Clear existing columns and rows if needed
            Dt.Clear();
            Dt.Columns.Clear();

            // Add necessary columns
            Dt.Columns.Add("Ser_pro_code", typeof(string));
            Dt.Columns.Add("Ser_pro_Name", typeof(string));

            //// Add the "Order" column here
            if (!Dt.Columns.Contains("Order"))
            {
                Dt.Columns.Add("Order", typeof(int));
            }

            // Add rows from first_datatable
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = Dt.NewRow();
                dr["Ser_pro_code"] = dt.Rows[i][0].ToString();
                dr["Ser_pro_Name"] = dt.Rows[i][1].ToString();

                // Initialize order, e.g., by default assign sequential order
                //dr["Order"] = i + 1;

                Dt.Rows.Add(dr);
            }
        }

        private void newgrid()
        {
            DataTable dt = first_datatable;

            if (Dt == null)
                Dt = new DataTable();

            Dt.Clear();
            Dt.Columns.Clear();

            // Add columns
            Dt.Columns.Add("Ser_pro_code", typeof(string));
            Dt.Columns.Add("Ser_pro_Name", typeof(string));

            // Add the Order column
            Dt.Columns.Add("Order", typeof(int));

            // Fill rows with default Order values (1-based index)
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = Dt.NewRow();
                dr["Ser_pro_code"] = dt.Rows[i][0].ToString();
                dr["Ser_pro_Name"] = dt.Rows[i][1].ToString();
                //dr["Order"] = i + 1;  // default ordering sequence
                Dt.Rows.Add(dr);
            }
        }

        private void AppendJs(WebControl ctrl, string eventName, string js)
        {
            if (ctrl == null) return;

            string existing = ctrl.Attributes[eventName];

            if (string.IsNullOrEmpty(existing))
            {
                ctrl.Attributes.Add(eventName, js);
            }
            else if (!existing.Contains(js))
            {
                ctrl.Attributes[eventName] = existing + ";" + js;
            }
        }


        private void WireModificationTracking(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                TextBox tb = ctrl as TextBox;
                if (tb != null && !tb.ReadOnly)
                {
                    AppendJs(tb, "onkeyup", "markRowModified(this)");
                    AppendJs(tb, "onchange", "markRowModified(this)");
                }

                DropDownList ddl = ctrl as DropDownList;
                if (ddl != null)
                {
                    AppendJs(ddl, "onchange", "markRowModified(this)");
                }

                RadioButtonList rbl = ctrl as RadioButtonList;
                if (rbl != null)
                {
                    AppendJs(rbl, "onchange", "markRowModified(this)");
                }

                if (ctrl.HasControls())
                    WireModificationTracking(ctrl);
            }
        }


        private void WireModificationTracking_OLD(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                // TextBox
                TextBox tb = ctrl as TextBox;
                if (tb != null && !tb.ReadOnly && tb.ID != "TaxableAmount")
                {
                    tb.Attributes["onchange"] = "markRowModified(this)";
                    tb.Attributes["onkeyup"] = "markRowModified(this)";
                }

                // DropDownList
                DropDownList ddl = ctrl as DropDownList;
                if (ddl != null)
                {
                    ddl.Attributes["onchange"] = "markRowModified(this)";
                }

                // RadioButtonList
                RadioButtonList rbl = ctrl as RadioButtonList;
                if (rbl != null)
                {
                    rbl.Attributes["onchange"] = "markRowModified(this)";
                }

                // Recursive call
                if (ctrl.HasControls())
                {
                    WireModificationTracking(ctrl);
                }
            }
        }



        protected void gd_Service_Product_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            foreach (Control c in e.Row.Controls)
            {
                WireModificationTracking(e.Row);
            }

            DataRowView drv = (DataRowView)e.Row.DataItem;

            // --- Textboxes ---
            ((TextBox)e.Row.FindControl("sepecification")).Text =
                drv["Description"]?.ToString();

            ((TextBox)e.Row.FindControl("Quantity")).Text =
                drv["Qnty"]?.ToString();

            ((TextBox)e.Row.FindControl("Vendor_rate")).Text =
                drv["Rate"]?.ToString();

            ((TextBox)e.Row.FindControl("DiscountPercent")).Text =
                drv["DiscountPercent"]?.ToString();

            ((TextBox)e.Row.FindControl("DiscountAmount")).Text =
                drv["DiscountAmount"]?.ToString();

            ((TextBox)e.Row.FindControl("TaxableAmount")).Text =
                drv["TaxableAmount"]?.ToString();

            ((TextBox)e.Row.FindControl("txtOrder")).Text =
                drv["ItemOrder"]?.ToString();

            // --- Tax Applicable ---
            RadioButtonList rbl =
                (RadioButtonList)e.Row.FindControl("RadioButtonList1");

            bool isTaxApplicable =
                drv["IsTaxApplicable"] != DBNull.Value &&
                Convert.ToBoolean(drv["IsTaxApplicable"]);

            rbl.SelectedValue = isTaxApplicable ? "Yes" : "No";

            // --- GST DropDown ---
            DropDownList ddl =
                (DropDownList)e.Row.FindControl("vat_parsentage");

            BindGSTDropdown(ddl); // MUST bind first

            if (drv["gstrate"] != DBNull.Value)
            {
                ddl.SelectedValue = drv["gstrate"].ToString();
            }

            // --- Reset Modified Flag ---
            HiddenField hdn =
                (HiddenField)e.Row.FindControl("hdnIsModified");

            hdn.Value = "0"; // reloaded from DB, not modified
        }

        private void BindGSTDropdown(DropDownList ddl)
        {
            if (ddl.Items.Count > 0) return;

            ddl.Items.Add(new ListItem("--Select--", ""));
            ddl.Items.Add(new ListItem("0", "0"));
            ddl.Items.Add(new ListItem("5", "5"));
            ddl.Items.Add(new ListItem("12", "12"));
            ddl.Items.Add(new ListItem("18", "18"));
            ddl.Items.Add(new ListItem("28", "28"));
        }


        protected void gd_Service_Product_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "DeleteItem") return;
            //if (lblStatus.Text != "Draft") return;
            if (lblStatus.Text != "Draft")
            {
                ShowError("This PR can no longer be modified.");
                return;
            }

            int rowId = Convert.ToInt32(e.CommandArgument);

            using (SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("DELETE FROM tbl_RequisitionNew WHERE id=@id", con);
                cmd.Parameters.AddWithValue("@id", rowId);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            LoadPR(CurrentReqNo); // refresh grid
            CalculatePRSummary_DB(CurrentReqNo);
        }



        private string CurrentReqNo
        {
            get { return ViewState["ReqNo"]?.ToString(); }
            set { ViewState["ReqNo"] = value; }
        }


        private string SaveOrUpdatePRHeader(SqlConnection con, SqlTransaction tran)
        {
            string reqNo = CurrentReqNo;

            if (string.IsNullOrEmpty(reqNo))
            {
                // NEW PR
                reqNo = GenerateReqNo(con, tran);

                SqlCommand cmd = new SqlCommand(@"
                INSERT INTO tbl_RequisitionMain
                (ReqNo, VendorId, Vendor, Status, CreatedBy, CreatedOn)
                VALUES
                (@ReqNo, @VendorId, @Vendor, 'Draft', @User, GETDATE())", con, tran);
                cmd.Parameters.AddWithValue("@ReqNo", reqNo);
                cmd.Parameters.AddWithValue("@VendorId", lblvendor_id.Text);
                cmd.Parameters.AddWithValue("@Vendor", cmbvendor.SelectedItem.Text);
                cmd.Parameters.AddWithValue("@User", Session["USERID"].ToString());

                cmd.ExecuteNonQuery();
            }
            else
            {
                // UPDATE EXISTING DRAFT
                SqlCommand cmd = new SqlCommand(@"
                UPDATE tbl_RequisitionMain SET VendorId=@VendorId, Vendor=@Vendor WHERE ReqNo=@ReqNo AND Status='Draft'", con, tran);
                cmd.Parameters.AddWithValue("@ReqNo", reqNo);
                cmd.Parameters.AddWithValue("@VendorId", lblvendor_id.Text);
                cmd.Parameters.AddWithValue("@Vendor", cmbvendor.SelectedItem.Text);
                cmd.ExecuteNonQuery();
            }

            return reqNo;
        }

        private DataTable BuildRequisitionItemTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ProductId", typeof(string));
            dt.Columns.Add("ProductName", typeof(string));
            dt.Columns.Add("ParentCategoryId", typeof(int));
            dt.Columns.Add("Description", typeof(string));
            dt.Columns.Add("Qnty", typeof(decimal));
            dt.Columns.Add("Rate", typeof(decimal));
            dt.Columns.Add("DiscountPercent", typeof(decimal));
            dt.Columns.Add("DiscountAmount", typeof(decimal));
            dt.Columns.Add("IsTaxApplicable", typeof(bool));
            dt.Columns.Add("GST", typeof(decimal));
            dt.Columns.Add("ItemOrder", typeof(int));
            return dt;
        }

        private bool ValidateGridRow(GridViewRow row, out string errorMessage)
        {
            errorMessage = "";

            decimal qty = ToDecimal(row, "Quantity") ?? 0;
            decimal rate = ToDecimal(row, "Vendor_rate") ?? -1;

            decimal? discPct = ToDecimal(row, "DiscountPercent");
            decimal? discAmt = ToDecimal(row, "DiscountAmount");

            bool isTaxApplicable = GetTaxApplicable(row);
            decimal gst = GetGSTRate(row, isTaxApplicable);

            string productName = ((Label)row.FindControl("Ser_pro_Name"))?.Text ?? "Item";

            if (qty <= 0)
            {
                errorMessage = $"Quantity must be greater than zero for {productName}.";
                return false;
            }

            if (rate < 0)
            {
                errorMessage = $"Rate cannot be negative for {productName}.";
                return false;
            }

            if (discPct.HasValue && discPct < 0 ||
                discAmt.HasValue && discAmt < 0)
            {
                errorMessage = $"Discount cannot be negative for {productName}.";
                return false;
            }

            //if (discPct.HasValue && discAmt.HasValue)
            //{
            //    errorMessage = $"Enter either Discount % OR Discount Amount (not both) for {productName}.";
            //    return false;
            //}

            if (isTaxApplicable && gst <= 0)
            {
                errorMessage = $"GST must be selected for taxable item {productName}.";
                return false;
            }

            return true;
        }

        private bool GetTaxApplicable(GridViewRow row)
        {
            RadioButtonList rbl =
                row.FindControl("RadioButtonList1") as RadioButtonList;

            if (rbl == null || rbl.SelectedItem == null)
                return false;

            return rbl.SelectedItem.Text.Equals("Yes",
                StringComparison.OrdinalIgnoreCase);
        }

        private decimal GetGSTRate(GridViewRow row, bool isTaxApplicable)
        {
            if (!isTaxApplicable)
                return 0;

            DropDownList ddl =
                row.FindControl("vat_parsentage") as DropDownList;

            if (ddl == null || string.IsNullOrEmpty(ddl.SelectedValue))
                return 0;

            decimal gst;
            return decimal.TryParse(ddl.SelectedValue, out gst) ? gst : 0;
        }

        private decimal? ToDecimal(GridViewRow row, string controlId)
        {
            TextBox txt = row.FindControl(controlId) as TextBox;
            if (txt == null || string.IsNullOrWhiteSpace(txt.Text))
                return null;

            decimal val;
            return decimal.TryParse(txt.Text.Trim(), out val) ? val : (decimal?)null;
        }

        private int ToInt(GridViewRow row, string controlId)
        {
            TextBox txt = row.FindControl(controlId) as TextBox;
            if (txt == null || string.IsNullOrWhiteSpace(txt.Text))
                return 0;

            int val;
            return int.TryParse(txt.Text.Trim(), out val) ? val : 0;
        }

        private void RebindGrid()
        {
            using (SqlConnection con =
                new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(@"
                SELECT id, ProductId AS Ser_pro_code,
                       ProductName AS Ser_pro_Name,
                       Description, Qnty, Rate,
                       DiscountPercent, DiscountAmount,
                       TaxableAmount, IsTaxApplicable,
                       gstrate, ItemOrder
                FROM tbl_RequisitionNew
                WHERE ReqNo = @ReqNo
                ORDER BY ItemOrder", con);

                cmd.Parameters.AddWithValue("@ReqNo", lblReqNo.Text);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gd_Service_Product.DataSource = dt;
                gd_Service_Product.DataBind();
            }
        }

        private string GetTaxApplicable_OLD(GridViewRow row)
        {
            RadioButtonList rbl = (RadioButtonList)row.FindControl("RadioButtonList1");
            return rbl?.SelectedValue ?? "No";
        }

        private decimal GetGSTRate(GridViewRow row, string taxApplicable)
        {
            if (taxApplicable != "Yes")
                return 0;

            DropDownList ddl = (DropDownList)row.FindControl("vat_parsentage");
            return ddl == null ? 0 : decimal.Parse(ddl.SelectedValue);
        }

        private string GenerateReqNo(SqlConnection con, SqlTransaction tran)
        {
            SqlCommand cmd = new SqlCommand(
                "SELECT ISNULL(MAX(Id),0)+1 FROM tbl_RequisitionMain",
                con, tran);

            int nextId = (int)cmd.ExecuteScalar();
            return "PR/" + DateTime.Now.Year + "/" + nextId.ToString("D5");
        }

        private bool validateModifiedRows_Server()
        {
            foreach (GridViewRow row in gd_Service_Product.Rows)
            {
                HiddenField hdn = (HiddenField)row.FindControl("hdnIsModified");
                if (hdn?.Value != "1") continue;

                if (string.IsNullOrWhiteSpace(
                    ((TextBox)row.FindControl("Quantity")).Text))
                    return false;
            }
            return true;
        }



        protected void Button3_Click(object sender, EventArgs e)
        {
            btnSubmit_Click(sender, e);
        }

        //protected void btnSubmit_Click(object sender, EventArgs e)
        //{
        //    if (string.IsNullOrEmpty(CurrentReqNo))
        //    {
        //        ShowError("Please save the PR before submitting.");
        //        return;
        //    }

        //    CalculatePRSummary_DB(CurrentReqNo);
        //    if (decimal.Parse(lblNet.Text) <= 0)
        //    {
        //        ShowError("PR total amount must be greater than zero.");
        //        return;
        //    }

        //    using (SqlConnection con = new SqlConnection(
        //        ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
        //    {
        //        SqlCommand cmd = new SqlCommand("sp_SubmitRequisition", con);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.Parameters.AddWithValue("@ReqNo", CurrentReqNo);
        //        cmd.Parameters.AddWithValue("@UserId", Session["USERID"].ToString());
        //        con.Open();
        //        cmd.ExecuteNonQuery();
        //    }

        //    ApplyStatusUI("Submitted");
        //    ShowSuccess("PR submitted successfully for approval.");
        //}

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentReqNo))
            {
                ShowError("Please save the PR before submitting.");
                return;
            }

            using (SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    // 1️⃣ Lock totals (authoritative)
                    UpdatePRTotals_OnSubmit(con, tran, CurrentReqNo);

                    // 2️⃣ Submit PR
                    SqlCommand cmd = new SqlCommand("sp_SubmitRequisition", con, tran);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ReqNo", CurrentReqNo);
                    cmd.Parameters.AddWithValue("@UserId", Session["USERID"].ToString());
                    cmd.ExecuteNonQuery();

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ShowError("Submit failed: " + ex.Message);
                    return;
                }
            }

            ApplyStatusUI("Submitted");
            ShowSuccess("PR submitted successfully with locked totals.");
        }


        private void UpdatePRTotals_OnSubmit(
    SqlConnection con,
    SqlTransaction tran,
    string reqNo)
        {
            SqlCommand cmdCalc = new SqlCommand(@"
        SELECT
            SUM(Qnty * Rate),
            SUM(ISNULL(DiscountAmount,0)),
            SUM(TaxableAmount),
            SUM(
                CASE WHEN IsTaxApplicable = 1
                     THEN TaxableAmount * gstrate / 100
                     ELSE 0
            END)
        FROM tbl_RequisitionNew
        WHERE ReqNo = @ReqNo", con, tran);

            cmdCalc.Parameters.AddWithValue("@ReqNo", reqNo);

            decimal gross = 0, discount = 0, taxable = 0, gst = 0;

            using (SqlDataReader dr = cmdCalc.ExecuteReader())
            {
                if (dr.Read())
                {
                    gross = dr.IsDBNull(0) ? 0 : dr.GetDecimal(0);
                    discount = dr.IsDBNull(1) ? 0 : dr.GetDecimal(1);
                    taxable = dr.IsDBNull(2) ? 0 : dr.GetDecimal(2);
                    gst = dr.IsDBNull(3) ? 0 : dr.GetDecimal(3);
                }
            }

            if (taxable <= 0)
                throw new Exception("PR total amount must be greater than zero.");

            SqlCommand cmdUpdate = new SqlCommand(@"
        UPDATE tbl_RequisitionMain
        SET
            GrossAmount    = @Gross,
            DiscountAmount = @Discount,
            TaxableAmount  = @Taxable,
            GSTAmount      = @GST,
            NetAmount      = @Net
        WHERE ReqNo = @ReqNo", con, tran);

            cmdUpdate.Parameters.AddWithValue("@Gross", gross);
            cmdUpdate.Parameters.AddWithValue("@Discount", discount);
            cmdUpdate.Parameters.AddWithValue("@Taxable", taxable);
            cmdUpdate.Parameters.AddWithValue("@GST", gst);
            cmdUpdate.Parameters.AddWithValue("@Net", taxable + gst);
            cmdUpdate.Parameters.AddWithValue("@ReqNo", reqNo);

            cmdUpdate.ExecuteNonQuery();
        }




        private void ApplyStatusUI_OLD(string status)
        {
            lblStatus.Text = status;

            btnSaveDraft.Visible = (status == "Draft");
            Button3.Visible = (status == "Draft"); // Save / Submit
            btnReorder.Visible = (status == "Draft");

            // After submit
            if (status != "Draft")
            {
                btnSaveDraft.Enabled = false;
                Button3.Enabled = false;
                btnReorder.Enabled = false;
            }
        }

        private void ApplyStatusUI(string status)
        {
            lblStatus.Text = status;

            bool isDraft = (status == "Draft");

            btnSaveDraft.Enabled = isDraft;
            Button3.Enabled = isDraft;   // Submit
            btnReorder.Enabled = isDraft;
            btnCancelPR.Visible = isDraft;

            Label1.Visible = isDraft;
            RadioButtonList1.Visible = isDraft;
            Button1.Visible = isDraft;
            Button1.Enabled = isDraft;
            PurchaseType_Row.Visible = isDraft;

            SearchBox_Row.Visible = isDraft;
            SearchBox_Msg.Visible = isDraft;

            Modifier_Msg_Row.Visible = isDraft;

            //gd_Service_Product.Visible = isDraft;
            cmbvendor.Enabled = isDraft;
        }

        protected void btnSaveEdit_Click(object sender, EventArgs e)
        {
            if (lblStatus.Text != "Draft")
            {
                ShowError("This PR can no longer be modified.");
                return;
            }

            ClearMessages();

            string userId = Session["USERID"]?.ToString();
            if (string.IsNullOrEmpty(userId))
            {
                ShowError("Session expired.");
                return;
            }

            DataTable dt = BuildRequisitionItemTable();

            foreach (GridViewRow row in gd_Service_Product.Rows)
            {
                HiddenField hdn = row.FindControl("hdnIsModified") as HiddenField;
                if (hdn == null || hdn.Value != "1") continue;

                HiddenField hdnCat = row.FindControl("hdnParentCategoryId") as HiddenField;
                if (hdnCat == null || string.IsNullOrWhiteSpace(hdnCat.Value))
                {
                    ShowError("Invalid product category mapping.");
                    return;
                }


                string error;
                if (!ValidateGridRow(row, out error))
                {
                    ShowError(error);
                    return;
                }

                dt.Rows.Add(
                    ((Label)row.FindControl("Ser_pro_code")).Text,
                    ((Label)row.FindControl("Ser_pro_Name")).Text,
                    Convert.ToInt32(((HiddenField)row.FindControl("hdnParentCategoryId")).Value),
                    ((TextBox)row.FindControl("sepecification")).Text,
                    ToDecimal(row, "Quantity") ?? 0,
                    ToDecimal(row, "Vendor_rate") ?? 0,
                    ToDecimal(row, "DiscountPercent"),
                    ToDecimal(row, "DiscountAmount"),
                    GetTaxApplicable(row),
                    GetGSTRate(row, GetTaxApplicable(row)),
                    ToInt(row, "txtOrder")
                );
            }

            if (dt.Rows.Count == 0)
            {
                ShowError("No modified rows to save.");
                return;
            }

            using (SqlConnection con =
                new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    SqlCommand cmd = new SqlCommand(
                        "sp_RequisitionItem_BulkUpsert", con, tran);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ClientName", cmbvendor.SelectedItem.Text);
                    cmd.Parameters.AddWithValue("@ReqNo", lblReqNo.Text);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    SqlParameter tvp =
                        cmd.Parameters.AddWithValue("@Items", dt);
                    tvp.SqlDbType = SqlDbType.Structured;
                    tvp.TypeName = "dbo.RequisitionItem_TVP";

                    cmd.ExecuteNonQuery();
                    tran.Commit();

                    ShowSuccess("Modified items saved successfully.");
                    RebindGrid(); // reload from DB
                    CalculatePRSummary_DB(lblReqNo.Text.ToString());
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ShowError("Save failed: " + ex.Message);
                }
            }
        }



        protected void btnCancelPR_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_CancelRequisition", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ReqNo", CurrentReqNo);
                cmd.Parameters.AddWithValue("@CancelledBy", Session["USERID"].ToString());
                cmd.Parameters.AddWithValue("@CancelReason", "Cancelled by user");

                con.Open();
                cmd.ExecuteNonQuery();
            }

            ApplyStatusUI("Cancelled");
            ShowSuccess("PR cancelled successfully.");
        }

        private void CalculatePRSummary_DB(string reqNo)
        {
            using (SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(@"
                SELECT
                    CAST(SUM(Qnty * Rate) AS DECIMAL(18,2)) AS GrossAmount,
                    CAST(SUM(ISNULL(DiscountAmount,0)) AS DECIMAL(18,2)) AS DiscountAmount,
                    CAST(SUM(TaxableAmount) AS DECIMAL(18,2)) AS TaxableAmount,
                    CAST(SUM(
                        CASE WHEN IsTaxApplicable = 1
                        THEN TaxableAmount * gstrate / 100
                        ELSE 0 END
                    ) AS DECIMAL(18,2)) AS GSTAmount
                FROM tbl_RequisitionNew
                WHERE ReqNo = @ReqNo;", con);

                cmd.Parameters.AddWithValue("@ReqNo", reqNo);
                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    decimal gross = dr.IsDBNull(0) ? 0 : dr.GetDecimal(0);
                    decimal discount = dr.IsDBNull(1) ? 0 : dr.GetDecimal(1);
                    decimal taxable = dr.IsDBNull(2) ? 0 : dr.GetDecimal(2);
                    decimal gst = dr.IsDBNull(3) ? 0 : dr.GetDecimal(3);

                    lblGross.Text = gross.ToString("N2");
                    lblDiscount.Text = discount.ToString("N2");
                    lblTaxable.Text = taxable.ToString("N2");
                    lblGST.Text = gst.ToString("N2");
                    lblNet.Text = (taxable + gst).ToString("N2");
                }
            }
        }

    }
}