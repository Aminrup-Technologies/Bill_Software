using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class Generate_PO_Preview : System.Web.UI.Page
    {
        protected ScriptManager ScriptManager1;
        protected UpdatePanel upBillShip;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
                return;
            }

            if (!IsPostBack)
            {
                if (Request.QueryString["reqNo"] == null)
                {
                    Response.Redirect("Generate_PO_From_PR.aspx");
                    return;
                }

                string reqNo = Request.QueryString["reqNo"].ToString();

                LoadBillToMasters();
                LoadShipToMasters();
                LoadPreview(reqNo);
            }
        }

        #region Data Loading

        private void LoadBillToMasters()
        {
            int companyId = CompanyContext.CurrentCompanyID;
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                using (SqlCommand cmd1 = new SqlCommand(
                    "SELECT ID, Name FROM tbl_Company WHERE ID = @CompanyID", con))
                {
                    cmd1.Parameters.AddWithValue("@CompanyID", companyId);
                    SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
                    DataTable dtCompany = new DataTable();
                    da1.Fill(dtCompany);
                    ddlBillToCompany.DataSource = dtCompany;
                    ddlBillToCompany.DataTextField = "Name";
                    ddlBillToCompany.DataValueField = "ID";
                    ddlBillToCompany.DataBind();
                }

                using (SqlCommand cmd2 = new SqlCommand(
                    "SELECT Id, StoreName FROM Stores WHERE IsActive = 1", con))
                {
                    SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                    DataTable dtStore = new DataTable();
                    da2.Fill(dtStore);
                    ddlBillToStore.DataSource = dtStore;
                    ddlBillToStore.DataTextField = "StoreName";
                    ddlBillToStore.DataValueField = "Id";
                    ddlBillToStore.DataBind();
                }
            }
        }

        private void LoadShipToMasters()
        {
            int companyId = CompanyContext.CurrentCompanyID;
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                using (SqlCommand cmdStore = new SqlCommand(
                    "SELECT Id, StoreName FROM Stores WHERE IsActive = 1", con))
                {
                    SqlDataAdapter daStore = new SqlDataAdapter(cmdStore);
                    DataTable dtStore = new DataTable();
                    daStore.Fill(dtStore);
                    ddlShipToStore.DataSource = dtStore;
                    ddlShipToStore.DataTextField = "StoreName";
                    ddlShipToStore.DataValueField = "Id";
                    ddlShipToStore.DataBind();
                }

                using (SqlCommand cmdClient = new SqlCommand(
                    "SELECT Id, Client_Name FROM tbl_Client WHERE CompanyID = @CompanyID", con))
                {
                    cmdClient.Parameters.AddWithValue("@CompanyID", companyId);
                    SqlDataAdapter daClient = new SqlDataAdapter(cmdClient);
                    DataTable dtClient = new DataTable();
                    daClient.Fill(dtClient);
                    ddlShipToClient.DataSource = dtClient;
                    ddlShipToClient.DataTextField = "Client_Name";
                    ddlShipToClient.DataValueField = "Id";
                    ddlShipToClient.DataBind();
                }
            }
        }

        private void LoadPreview(string reqNo)
        {
            int companyId = CompanyContext.CurrentCompanyID;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmdHdr = new SqlCommand(
                    "SELECT ReqNo, clientName FROM tbl_RequisitionMain WHERE ReqNo = @ReqNo AND CompanyID = @CompanyID", con);
                cmdHdr.Parameters.AddWithValue("@ReqNo", reqNo);
                cmdHdr.Parameters.AddWithValue("@CompanyID", companyId);
                con.Open();
                using (SqlDataReader dr = cmdHdr.ExecuteReader())
                {
                    if (!dr.Read())
                    {
                        Response.Redirect("Generate_PO_From_PR.aspx");
                        return;
                    }
                    lblPrevReqNo.Text = dr["ReqNo"].ToString();
                    lblPrevVendor.Text = dr["clientName"] == DBNull.Value ? "" : dr["clientName"].ToString();
                }
            }

            LoadPreviewTotals(reqNo);

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                using (SqlCommand cmdItems = new SqlCommand(@"
                SELECT ItemOrder AS SlNo, ProductName, Qnty, Rate, TaxableAmount,
                    CAST(CASE WHEN ISNULL(IsTaxApplicable, 0) = 1
                         THEN ISNULL(TaxableAmount, 0) * ISNULL(CAST(gstrate AS DECIMAL(5,2)), 0) / 100.0
                         ELSE 0 END AS DECIMAL(18,2)) AS TaxAmount,
                    CAST(ISNULL(TaxableAmount, 0) +
                         CASE WHEN ISNULL(IsTaxApplicable, 0) = 1
                              THEN ISNULL(TaxableAmount, 0) * ISNULL(CAST(gstrate AS DECIMAL(5,2)), 0) / 100.0
                              ELSE 0 END AS DECIMAL(18,2)) AS NetAmount
                FROM tbl_RequisitionNew
                WHERE ReqNo = @ReqNo AND CompanyID = @CompanyID
                ORDER BY ItemOrder", con))
                {
                    cmdItems.Parameters.AddWithValue("@ReqNo", reqNo);
                    cmdItems.Parameters.AddWithValue("@CompanyID", companyId);
                    SqlDataAdapter da = new SqlDataAdapter(cmdItems);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvPreviewItems.DataSource = dt;
                    gvPreviewItems.DataBind();
                }
            }
        }

        private void LoadPreviewTotals(string reqNo)
        {
            ViewState["TotalQty"] = 0; ViewState["TotalTaxable"] = 0;
            ViewState["TotalGST"] = 0; ViewState["TotalNet"] = 0;

            int companyId = CompanyContext.CurrentCompanyID;
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(@"
                SELECT SUM(Qnty) AS TotalQty,
                       SUM(TaxableAmount) AS TotalTaxable,
                       SUM(CASE WHEN ISNULL(IsTaxApplicable, 0) = 1
                                THEN ISNULL(TaxableAmount, 0) * ISNULL(CAST(gstrate AS DECIMAL(5,2)), 0) / 100.0
                                ELSE 0 END) AS TotalGST,
                       SUM(ISNULL(TaxableAmount, 0) +
                            CASE WHEN ISNULL(IsTaxApplicable, 0) = 1
                                 THEN ISNULL(TaxableAmount, 0) * ISNULL(CAST(gstrate AS DECIMAL(5,2)), 0) / 100.0
                                 ELSE 0 END) AS TotalNet
                FROM tbl_RequisitionNew
                WHERE ReqNo = @ReqNo AND CompanyID = @CompanyID", con);

                cmd.Parameters.AddWithValue("@ReqNo", reqNo);
                cmd.Parameters.AddWithValue("@CompanyID", companyId);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    ViewState["TotalQty"] = dr["TotalQty"];
                    ViewState["TotalTaxable"] = dr["TotalTaxable"];
                    ViewState["TotalGST"] = dr["TotalGST"];
                    ViewState["TotalNet"] = dr["TotalNet"];
                }
            }
        }

        protected void gvPreviewItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Footer)
            {
                decimal qty = ViewState["TotalQty"] == DBNull.Value ? 0 : Convert.ToDecimal(ViewState["TotalQty"]);
                decimal taxable = ViewState["TotalTaxable"] == DBNull.Value ? 0 : Convert.ToDecimal(ViewState["TotalTaxable"]);
                decimal gst = ViewState["TotalGST"] == DBNull.Value ? 0 : Convert.ToDecimal(ViewState["TotalGST"]);
                decimal net = ViewState["TotalNet"] == DBNull.Value ? 0 : Convert.ToDecimal(ViewState["TotalNet"]);

                e.Row.Cells[0].Text = "Total";
                e.Row.Cells[0].ColumnSpan = 2;
                e.Row.Cells[0].HorizontalAlign = HorizontalAlign.Right;
                e.Row.Cells[1].Visible = false;

                e.Row.Cells[2].Text = qty.ToString("N2");
                e.Row.Cells[4].Text = taxable.ToString("N2");
                e.Row.Cells[5].Text = gst.ToString("N2");
                e.Row.Cells[6].Text = net.ToString("N2");
                e.Row.Font.Bold = true;
            }
        }

        #endregion

        #region Form Events & Validation

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

        private bool ValidateForm()
        {
            if (rblBillToType.SelectedIndex == -1)
            {
                lblError.Text = "Please select a 'Bill To' option.";
                return false;
            }
            if (rblShipToType.SelectedIndex == -1)
            {
                lblError.Text = "Please select a 'Ship To' option.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtEngineerName.Text))
            {
                lblError.Text = "Engineer Name is mandatory.";
                return false;
            }
            return true;
        }

        #endregion

        #region Actions

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("Generate_PO_From_PR.aspx");
        }

        protected void btnCreatePO_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            if (Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
                return;
            }

            string reqNo = lblPrevReqNo.Text;
            int companyId = CompanyContext.CurrentCompanyID;
            string userId = Session["USERID"].ToString();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    using (SqlCommand cmdOwn = new SqlCommand(
                        "SELECT 1 FROM tbl_RequisitionMain R WHERE R.ReqNo = @ReqNo AND R.CompanyID = @CompanyID AND R.Status = 'Approved' AND NOT EXISTS (SELECT 1 FROM tbl_PO_Header P WHERE P.ReqNo = R.ReqNo AND P.CompanyID = R.CompanyID)", con, tran))
                    {
                        cmdOwn.Parameters.AddWithValue("@ReqNo", reqNo);
                        cmdOwn.Parameters.AddWithValue("@CompanyID", companyId);
                        if (cmdOwn.ExecuteScalar() == null)
                            throw new Exception("Approved PR not found, or a PO already exists for this PR.");
                    }

                    SqlCommand cmdGenerate = new SqlCommand("sp_GeneratePO_FromReqNo", con, tran);
                    cmdGenerate.CommandType = CommandType.StoredProcedure;
                    cmdGenerate.Parameters.AddWithValue("@ReqNo", reqNo);
                    cmdGenerate.Parameters.AddWithValue("@UserId", userId);
                    cmdGenerate.Parameters.AddWithValue("@CompanyID", companyId);
                    cmdGenerate.ExecuteNonQuery();

                    using (SqlCommand cmdFixItems = new SqlCommand(@"
                        UPDATE I SET I.CompanyID = H.CompanyID
                        FROM tbl_PO_Items I
                        INNER JOIN tbl_PO_Header H ON H.PO_Id = I.PO_Id
                        WHERE H.ReqNo = @ReqNo AND H.CompanyID = @CompanyID
                          AND ISNULL(I.CompanyID, 0) <> H.CompanyID", con, tran))
                    {
                        cmdFixItems.Parameters.AddWithValue("@ReqNo", reqNo);
                        cmdFixItems.Parameters.AddWithValue("@CompanyID", companyId);
                        cmdFixItems.ExecuteNonQuery();
                    }

                    int newPoId = 0;
                    SqlCommand cmdGetId = new SqlCommand(
                        "SELECT TOP 1 PO_Id FROM tbl_PO_Header WHERE ReqNo = @ReqNo AND CompanyID = @CompanyID ORDER BY PO_Id DESC", con, tran);
                    cmdGetId.Parameters.AddWithValue("@ReqNo", reqNo);
                    cmdGetId.Parameters.AddWithValue("@CompanyID", companyId);
                    object result = cmdGetId.ExecuteScalar();

                    if (result != null)
                        newPoId = Convert.ToInt32(result);
                    else
                        throw new Exception("Could not retrieve the newly generated PO ID.");

                    SqlCommand cmdUpdate = new SqlCommand(@"
                        UPDATE tbl_PO_Header
                        SET EngineerName = @Engineer, DispatchMode = @DispatchMode, 
                            DispatchUpto = @DispatchUpto, DeliveryBasis = @DeliveryBasis, 
                            FreightTerms = @FreightTerms, Remarks = @Remarks
                        WHERE PO_Id = @PO_Id AND CompanyID = @CompanyID", con, tran);

                    cmdUpdate.Parameters.AddWithValue("@Engineer", txtEngineerName.Text.Trim());
                    cmdUpdate.Parameters.AddWithValue("@DispatchMode", ddlDispatchMode.SelectedValue);
                    cmdUpdate.Parameters.AddWithValue("@DispatchUpto", txtDispatchUpto.Text.Trim());
                    cmdUpdate.Parameters.AddWithValue("@DeliveryBasis", ddlDeliveryBasis.SelectedValue);
                    cmdUpdate.Parameters.AddWithValue("@FreightTerms", ddlFreightTerms.SelectedValue);
                    cmdUpdate.Parameters.AddWithValue("@Remarks", txtRemarks.Text.Trim());
                    cmdUpdate.Parameters.AddWithValue("@PO_Id", newPoId);
                    cmdUpdate.Parameters.AddWithValue("@CompanyID", companyId);
                    if (cmdUpdate.ExecuteNonQuery() == 0)
                        throw new Exception("PO update failed: record not found for this company.");

                    SavePartySnapshots(con, tran, newPoId, companyId);

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    lblError.Text = "An error occurred while generating the PO: " + ex.Message;
                    return;
                }
            }

            Response.Redirect("Generate_PO_From_PR.aspx");
        }

        #endregion

        #region Party Snapshots Saving Logic

        private void SavePartySnapshots(SqlConnection con, SqlTransaction tran, int poId, int companyId)
        {
            SqlCommand del = new SqlCommand(@"
                DELETE S FROM tbl_PO_PartySnapshot S
                INNER JOIN tbl_PO_Header H ON H.PO_Id = S.PO_Id
                WHERE S.PO_Id = @PO_Id AND H.CompanyID = @CompanyID", con, tran);
            del.Parameters.AddWithValue("@PO_Id", poId);
            del.Parameters.AddWithValue("@CompanyID", companyId);
            del.ExecuteNonQuery();

            InsertVendorSnapshot(con, tran, poId, companyId);

            if (rblBillToType.SelectedValue == "Company")
                InsertCompanySnapshot(con, tran, poId, "BillTo", Convert.ToInt32(ddlBillToCompany.SelectedValue), companyId);
            else
                InsertStoreSnapshot(con, tran, poId, "BillTo", Convert.ToInt32(ddlBillToStore.SelectedValue));

            if (rblShipToType.SelectedValue == "Store")
                InsertStoreSnapshot(con, tran, poId, "ShipTo", Convert.ToInt32(ddlShipToStore.SelectedValue));
            else
                InsertClientSnapshot(con, tran, poId, "ShipTo", Convert.ToInt32(ddlShipToClient.SelectedValue), companyId);
        }

        private void InsertVendorSnapshot(SqlConnection con, SqlTransaction tran, int poId, int companyId)
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
                JOIN tbl_Vendor V ON V.Id = H.VendorId AND V.CompanyID = H.CompanyID
                WHERE H.PO_Id = @PO_Id AND H.CompanyID = @CompanyID
            ", con, tran);

            cmd.Parameters.AddWithValue("@PO_Id", poId);
            cmd.Parameters.AddWithValue("@CompanyID", companyId);
            cmd.ExecuteNonQuery();
        }

        private void InsertCompanySnapshot(SqlConnection con, SqlTransaction tran, int poId, string role, int companyRowId, int companyId)
        {
            SqlCommand cmd = new SqlCommand(@"
                INSERT INTO tbl_PO_PartySnapshot
                (PO_Id, PartyRole, SourceTable, SourceId, Name, Address)
                SELECT
                    @PO_Id, @Role, 'tbl_Company', ID, Name, Address
                FROM tbl_Company
                WHERE ID = @ID AND ID = @CompanyID
            ", con, tran);

            cmd.Parameters.AddWithValue("@PO_Id", poId);
            cmd.Parameters.AddWithValue("@Role", role);
            cmd.Parameters.AddWithValue("@ID", companyRowId);
            cmd.Parameters.AddWithValue("@CompanyID", companyId);
            cmd.ExecuteNonQuery();
        }

        private void InsertStoreSnapshot(SqlConnection con, SqlTransaction tran, int poId, string role, int storeId)
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

        private void InsertClientSnapshot(SqlConnection con, SqlTransaction tran, int poId, string role, int clientId, int companyId)
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
                WHERE C.Id = @ID AND C.CompanyID = @CompanyID
            ", con, tran);

            cmd.Parameters.AddWithValue("@PO_Id", poId);
            cmd.Parameters.AddWithValue("@Role", role);
            cmd.Parameters.AddWithValue("@ID", clientId);
            cmd.Parameters.AddWithValue("@CompanyID", companyId);
            cmd.ExecuteNonQuery();
        }

        #endregion
    }
}
