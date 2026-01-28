using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class View_PO_Details : System.Web.UI.Page
    {
        decimal _totalQty = 0;
        decimal _totalDiscount = 0;
        decimal _totalTaxable = 0;
        decimal _totalGST = 0;
        decimal _totalNet = 0;

        #region Page Load

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
                return;
            }

            if (!IsPostBack)
            {
                string poIdStr = Request.QueryString["poId"];
                if (string.IsNullOrEmpty(poIdStr))
                {
                    ShowError("Invalid PO reference.");
                    return;
                }

                int poId;
                if (!int.TryParse(poIdStr, out poId))
                {
                    ShowError("Invalid PO Id.");
                    return;
                }

                LoadPO(poId);

                if (lblStatus.Text == "Draft")
                {
                    LoadBillToMasters();
                    LoadShipToMasters();
                }

            }
        }

        #endregion

        #region Load PO

        private void LoadPO(int poId)
        {
            lblPO_Id.Text = poId.ToString();

            using (SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                con.Open();

                /* ================= PO HEADER ================= */
                SqlCommand cmdHdr = new SqlCommand(@"
                    SELECT H.PO_Id, H.PO_No, H.ReqNo, H.PO_Date,
                           H.PO_Status, H.IsLocked,
                           V.Vendor_Name
                    FROM tbl_PO_Header H
                    LEFT JOIN tbl_Vendor V ON V.Id = H.VendorId
                    WHERE H.PO_Id = @PO_Id", con);

                cmdHdr.Parameters.AddWithValue("@PO_Id", poId);

                SqlDataReader dr = cmdHdr.ExecuteReader();
                if (!dr.Read())
                {
                    dr.Close();
                    ShowError("PO not found.");
                    return;
                }

                lblPONo.Text = dr["PO_No"].ToString();
                lblReqNo.Text = dr["ReqNo"].ToString();
                lblStatus.Text = dr["PO_Status"].ToString();
                lblPODate.Text = Convert.ToDateTime(dr["PO_Date"])
                                    .ToString("dd-MMM-yyyy");
                lblVendor.Text = dr["Vendor_Name"]?.ToString();

                bool isLocked = Convert.ToBoolean(dr["IsLocked"]);
                dr.Close();

                /* ================= PO ITEMS ================= */
                SqlDataAdapter da = new SqlDataAdapter(@"
                    SELECT
                    ProductName,
                    Quantity,
                    Rate,
                    DiscountPercent,
                    DiscountAmount,
                    TaxableAmount,
                    TaxRate,
                    TaxAmount,
                    NetAmount
                FROM tbl_PO_Items
                WHERE PO_Id = @PO_Id
                ORDER BY ItemOrder", con);

                da.SelectCommand.Parameters.AddWithValue("@PO_Id", poId);

                DataTable dtItems = new DataTable();
                da.Fill(dtItems);

                gdPOItems.DataSource = dtItems;
                gdPOItems.DataBind();

                /* ================= PO SUMMARY ================= */
                CalculatePOSummary(con, poId);

                /* ================= UI STATE ================= */
                ApplyStatusUI(lblStatus.Text, isLocked);
            }
        }

        #endregion

        private void LoadBillToMasters()
        {
            using (SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlDataAdapter da1 =
                    new SqlDataAdapter("SELECT ID, Name FROM tbl_Company", con);

                DataTable dtCompany = new DataTable();
                da1.Fill(dtCompany);

                ddlBillToCompany.DataSource = dtCompany;
                ddlBillToCompany.DataTextField = "Name";
                ddlBillToCompany.DataValueField = "ID";
                ddlBillToCompany.DataBind();

                SqlDataAdapter da2 =
                    new SqlDataAdapter("SELECT Id, StoreName FROM Stores WHERE IsActive=1", con);

                DataTable dtStore = new DataTable();
                da2.Fill(dtStore);

                ddlBillToStore.DataSource = dtStore;
                ddlBillToStore.DataTextField = "StoreName";
                ddlBillToStore.DataValueField = "Id";
                ddlBillToStore.DataBind();
            }
        }

        private void LoadShipToMasters()
        {
            using (SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlDataAdapter daStore =
                    new SqlDataAdapter("SELECT Id, StoreName FROM Stores WHERE IsActive=1", con);

                DataTable dtStore = new DataTable();
                daStore.Fill(dtStore);

                ddlShipToStore.DataSource = dtStore;
                ddlShipToStore.DataTextField = "StoreName";
                ddlShipToStore.DataValueField = "Id";
                ddlShipToStore.DataBind();

                SqlDataAdapter daClient =
                    new SqlDataAdapter("SELECT Id, Client_Name FROM tbl_Client", con);

                DataTable dtClient = new DataTable();
                daClient.Fill(dtClient);

                ddlShipToClient.DataSource = dtClient;
                ddlShipToClient.DataTextField = "Client_Name";
                ddlShipToClient.DataValueField = "Id";
                ddlShipToClient.DataBind();
            }
        }

        protected void rblBillToType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlBillToCompany.Enabled = (rblBillToType.SelectedValue == "Company");
            ddlBillToStore.Enabled = (rblBillToType.SelectedValue == "Store");
        }

        protected void rblShipToType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlShipToStore.Enabled = (rblShipToType.SelectedValue == "Store");
            ddlShipToClient.Enabled = (rblShipToType.SelectedValue == "Client");
        }

        private void ApplyStatusUI(string status, bool isLocked)
        {
            bool isDraft = status == "Draft";

            pnlPODetails.Visible = isDraft && !isLocked;

            btnReleasePO.Visible = isDraft && !isLocked;
            btnPrintPO.Visible = !isDraft;

            if (!isDraft)
            {
                btnPrintPO.PostBackUrl =
                    "Print_PO.aspx?poId=" + lblPO_Id.Text;
            }
        }

        private bool ValidateBeforeRelease()
        {
            if (string.IsNullOrWhiteSpace(txtEngineerName.Text))
            {
                ShowError("Engineer Name is mandatory.");
                return false;
            }

            if (rblBillToType.SelectedIndex == -1)
            {
                ShowError("Please select Bill To.");
                return false;
            }

            if (rblShipToType.SelectedIndex == -1)
            {
                ShowError("Please select Ship To.");
                return false;
            }

            return true;
        }

        protected void btnReleasePO_Click(object sender, EventArgs e)
        {
            if (!ValidateBeforeRelease())
                return;

            int poId = Convert.ToInt32(lblPO_Id.Text);

            using (SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    SavePartySnapshots(con, tran, poId);
                    SaveReleaseDetails(con, tran, poId);

                    SqlCommand cmd = new SqlCommand("sp_ReleasePO_Final", con, tran);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PO_Id", poId);
                    cmd.Parameters.AddWithValue("@UserId", Session["USERID"].ToString());
                    cmd.ExecuteNonQuery();

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ShowError("Release failed: " + ex.Message);
                    return;
                }
            }

            LoadPO(poId);
            ShowSuccess("PO released and locked successfully.");
        }

        private void SavePartySnapshots(SqlConnection con, SqlTransaction tran, int poId)
        {
            // Always clean existing (safety if reattempt)
            SqlCommand del = new SqlCommand(
                "DELETE FROM tbl_PO_PartySnapshot WHERE PO_Id = @PO_Id",
                con, tran);
            del.Parameters.AddWithValue("@PO_Id", poId);
            del.ExecuteNonQuery();

            // 1️⃣ Vendor (always from PO header)
            InsertVendorSnapshot(con, tran, poId);

            // 2️⃣ Bill To
            if (rblBillToType.SelectedValue == "Company")
                InsertCompanySnapshot(con, tran, poId, "BillTo",
                    Convert.ToInt32(ddlBillToCompany.SelectedValue));
            else
                InsertStoreSnapshot(con, tran, poId, "BillTo",
                    Convert.ToInt32(ddlBillToStore.SelectedValue));

            // 3️⃣ Ship To
            if (rblShipToType.SelectedValue == "Store")
                InsertStoreSnapshot(con, tran, poId, "ShipTo",
                    Convert.ToInt32(ddlShipToStore.SelectedValue));
            else
                InsertClientSnapshot(con, tran, poId, "ShipTo",
                    Convert.ToInt32(ddlShipToClient.SelectedValue));
        }

        private void InsertVendorSnapshot(SqlConnection con, SqlTransaction tran, int poId)
        {
            SqlCommand cmd = new SqlCommand(@"
        INSERT INTO tbl_PO_PartySnapshot
        (PO_Id, PartyRole, SourceTable, SourceId,
         Name, Address, City, State, Pin,
         GSTNo, PANNo, ContactPerson, ContactNo, Email)
        SELECT
            H.PO_Id,
            'Vendor',
            'tbl_Vendor',
            V.Id,
            V.Vendor_Name,
            CONCAT(ISNULL(V.Address1,''),' ',ISNULL(V.Address2,'')),
            V.City,
            V.State,
            V.pin,
            V.Vat_No,
            V.Pan_No,
            V.Rep_Name,
            V.Rep_phone,
            V.Com_email
        FROM tbl_PO_Header H
        JOIN tbl_Vendor V ON V.Id = H.VendorId
        WHERE H.PO_Id = @PO_Id
    ", con, tran);

            cmd.Parameters.AddWithValue("@PO_Id", poId);
            cmd.ExecuteNonQuery();
        }

        private void InsertCompanySnapshot(SqlConnection con, SqlTransaction tran,
    int poId, string role, int companyId)
        {
            SqlCommand cmd = new SqlCommand(@"
        INSERT INTO tbl_PO_PartySnapshot
        (PO_Id, PartyRole, SourceTable, SourceId, Name, Address)
        SELECT
            @PO_Id, @Role, 'tbl_Company', ID, Name, Address
        FROM tbl_Company
        WHERE ID = @ID
    ", con, tran);

            cmd.Parameters.AddWithValue("@PO_Id", poId);
            cmd.Parameters.AddWithValue("@Role", role);
            cmd.Parameters.AddWithValue("@ID", companyId);
            cmd.ExecuteNonQuery();
        }

        private void InsertStoreSnapshot(SqlConnection con, SqlTransaction tran,
            int poId, string role, int storeId)
        {
            SqlCommand cmd = new SqlCommand(@"
        INSERT INTO tbl_PO_PartySnapshot
        (PO_Id, PartyRole, SourceTable, SourceId,
         Name, Address, ContactPerson, ContactNo, Email)
        SELECT
            @PO_Id, @Role, 'Stores', Id,
            StoreName, StoreAddress,
            StoreManagerName, Mobile, Email
        FROM Stores
        WHERE Id = @ID
    ", con, tran);

            cmd.Parameters.AddWithValue("@PO_Id", poId);
            cmd.Parameters.AddWithValue("@Role", role);
            cmd.Parameters.AddWithValue("@ID", storeId);
            cmd.ExecuteNonQuery();
        }

        private void InsertClientSnapshot(SqlConnection con, SqlTransaction tran,
            int poId, string role, int clientId)
        {
            SqlCommand cmd = new SqlCommand(@"
        INSERT INTO tbl_PO_PartySnapshot
        (PO_Id, PartyRole, SourceTable, SourceId,
         Name, Address, City, State, Pin,
         GSTNo, PANNo, ContactPerson, ContactNo, Email)
        SELECT
            @PO_Id, @Role, 'tbl_Client', C.Id,
            C.Client_Name,
            C.Address1,
            C.City,
            C.State,
            C.pin,
            C.clientgstno,
            C.Pan_no,
            C.Rep_Name,
            C.Rep_phone,
            C.Com_email
        FROM tbl_Client C
        WHERE C.Id = @ID
    ", con, tran);

            cmd.Parameters.AddWithValue("@PO_Id", poId);
            cmd.Parameters.AddWithValue("@Role", role);
            cmd.Parameters.AddWithValue("@ID", clientId);
            cmd.ExecuteNonQuery();
        }

        private void SaveReleaseDetails(SqlConnection con, SqlTransaction tran, int poId)
        {
            SqlCommand cmd = new SqlCommand(@"
        UPDATE tbl_PO_Header
        SET
            EngineerName   = @Engineer,
            DispatchMode   = @DispatchMode,
            DispatchUpto   = @DispatchUpto,
            DeliveryBasis  = @DeliveryBasis,
            FreightTerms   = @FreightTerms,
            Remarks        = @Remarks
        WHERE PO_Id = @PO_Id
          AND PO_Status = 'Draft'
    ", con, tran);

            cmd.Parameters.AddWithValue("@Engineer", txtEngineerName.Text.Trim());
            cmd.Parameters.AddWithValue("@DispatchMode", ddlDispatchMode.SelectedValue);
            cmd.Parameters.AddWithValue("@DispatchUpto", txtDispatchUpto.Text.Trim());
            cmd.Parameters.AddWithValue("@DeliveryBasis", ddlDeliveryBasis.SelectedValue);
            cmd.Parameters.AddWithValue("@FreightTerms", ddlFreightTerms.SelectedValue);
            cmd.Parameters.AddWithValue("@Remarks", txtRemarks.Text.Trim());
            cmd.Parameters.AddWithValue("@PO_Id", poId);

            cmd.ExecuteNonQuery();
        }


        #region Summary Calculation

        private void CalculatePOSummary(SqlConnection con, int poId)
        {
            SqlCommand cmd = new SqlCommand(@"
                SELECT
                    CAST(SUM(Quantity * Rate) AS DECIMAL(18,2)) AS Gross,
                    CAST(SUM(ISNULL(TaxAmount,0)) AS DECIMAL(18,2)) AS GST,
                    CAST(SUM(ISNULL(NetAmount,0)) AS DECIMAL(18,2)) AS Net
                FROM tbl_PO_Items
                WHERE PO_Id = @PO_Id", con);

            cmd.Parameters.AddWithValue("@PO_Id", poId);

            SqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                //lblGross.Text = dr.IsDBNull(0) ? "0.00" : dr.GetDecimal(0).ToString("N2");
                //lblGST.Text = dr.IsDBNull(1) ? "0.00" : dr.GetDecimal(1).ToString("N2");
                //lblNet.Text = dr.IsDBNull(2) ? "0.00" : dr.GetDecimal(2).ToString("N2");

                lblGross.Text = _totalTaxable.ToString("N2");
                lblGST.Text = _totalGST.ToString("N2");
                lblNet.Text = _totalNet.ToString("N2");

            }
            dr.Close();
        }

        #endregion

        #region UI State

        private void ApplyStatusUI_OLD(string status, bool isLocked)
        {
            bool isDraft = status.Equals("Draft", StringComparison.OrdinalIgnoreCase);

            btnReleasePO.Visible = isDraft && !isLocked;
            btnPrintPO.Visible = !isDraft;

            if (!isDraft)
            {
                btnPrintPO.PostBackUrl =
                    "Print_PO.aspx?poId=" + lblPO_Id.Text;
            }

            // Status color
            switch (status)
            {
                case "Draft":
                    lblStatus.ForeColor = System.Drawing.Color.DarkOrange;
                    break;

                case "Released":
                    lblStatus.ForeColor = System.Drawing.Color.Green;
                    break;

                case "Cancelled":
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                    break;
            }
        }

        #endregion

        #region Release PO

        protected void btnReleasePO_Click_OLD(object sender, EventArgs e)
        {
            if (lblStatus.Text != "Draft")
            {
                ShowError("This PO has already been processed.");
                return;
            }


            int poId = Convert.ToInt32(lblPO_Id.Text);

            try
            {
                using (SqlConnection con = new SqlConnection(
                    ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("sp_ReleasePO", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PO_Id", poId);
                    cmd.Parameters.AddWithValue("@UserId",
                        Session["USERID"].ToString());

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                lblStatus.Text = "Released";
                ApplyStatusUI("Released", true);

                ShowSuccess("PO released successfully. It is now locked for printing.");
            }
            catch (Exception ex)
            {
                ShowError("Release failed: " + ex.Message);
            }
        }

        #endregion

        #region Message Helpers

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

        #endregion

        protected void gdPOItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                _totalQty += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "Quantity"));
                _totalDiscount += Convert.ToDecimal(
                    DataBinder.Eval(e.Row.DataItem, "DiscountAmount") ?? 0);

                _totalTaxable += Convert.ToDecimal(
                    DataBinder.Eval(e.Row.DataItem, "TaxableAmount"));

                _totalGST += Convert.ToDecimal(
                    DataBinder.Eval(e.Row.DataItem, "TaxAmount"));

                _totalNet += Convert.ToDecimal(
                    DataBinder.Eval(e.Row.DataItem, "NetAmount"));
            }

            if (e.Row.RowType == DataControlRowType.Footer)
            {
                // Label cell
                e.Row.Cells[0].Text = "Total";
                e.Row.Cells[0].ColumnSpan = 2;
                e.Row.Cells[1].Visible = false;

                // Qty
                e.Row.Cells[2].Text = _totalQty.ToString("N2");

                // Discount Amount
                e.Row.Cells[5].Text = _totalDiscount.ToString("N2");

                // Taxable Amount
                e.Row.Cells[6].Text = _totalTaxable.ToString("N2");

                // Tax Amount
                e.Row.Cells[8].Text = _totalGST.ToString("N2");

                // Net Amount (After Tax)
                e.Row.Cells[9].Text = _totalNet.ToString("N2");

                // Formatting
                e.Row.Font.Bold = true;
                e.Row.BackColor = System.Drawing.ColorTranslator.FromHtml("#f0f8ff");

                e.Row.Cells[2].HorizontalAlign = HorizontalAlign.Right;
                e.Row.Cells[5].HorizontalAlign = HorizontalAlign.Right;
                e.Row.Cells[6].HorizontalAlign = HorizontalAlign.Right;
                e.Row.Cells[8].HorizontalAlign = HorizontalAlign.Right;
                e.Row.Cells[9].HorizontalAlign = HorizontalAlign.Right;
            }
        }

    }
}