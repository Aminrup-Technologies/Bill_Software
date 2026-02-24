using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bill_Software.corporate.business.app
{
    public partial class Generate_PO_Preview : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
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
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlDataAdapter da1 = new SqlDataAdapter("SELECT ID, Name FROM tbl_Company", con);
                DataTable dtCompany = new DataTable();
                da1.Fill(dtCompany);
                ddlBillToCompany.DataSource = dtCompany;
                ddlBillToCompany.DataTextField = "Name";
                ddlBillToCompany.DataValueField = "ID";
                ddlBillToCompany.DataBind();

                SqlDataAdapter da2 = new SqlDataAdapter("SELECT Id, StoreName FROM Stores WHERE IsActive=1", con);
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
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlDataAdapter daStore = new SqlDataAdapter("SELECT Id, StoreName FROM Stores WHERE IsActive=1", con);
                DataTable dtStore = new DataTable();
                daStore.Fill(dtStore);
                ddlShipToStore.DataSource = dtStore;
                ddlShipToStore.DataTextField = "StoreName";
                ddlShipToStore.DataValueField = "Id";
                ddlShipToStore.DataBind();

                SqlDataAdapter daClient = new SqlDataAdapter("SELECT Id, Client_Name FROM tbl_Client", con);
                DataTable dtClient = new DataTable();
                daClient.Fill(dtClient);
                ddlShipToClient.DataSource = dtClient;
                ddlShipToClient.DataTextField = "Client_Name";
                ddlShipToClient.DataValueField = "Id";
                ddlShipToClient.DataBind();
            }
        }

        private void LoadPreview(string reqNo)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                // Header
                SqlCommand cmdHdr = new SqlCommand("SELECT ReqNo, clientName FROM tbl_RequisitionMain WHERE ReqNo = @ReqNo", con);
                cmdHdr.Parameters.AddWithValue("@ReqNo", reqNo);
                con.Open();
                using (SqlDataReader dr = cmdHdr.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        lblPrevReqNo.Text = dr["ReqNo"].ToString();
                        lblPrevVendor.Text = dr["clientName"] == DBNull.Value ? "" : dr["clientName"].ToString();
                    }
                }
            }

            LoadPreviewTotals(reqNo);

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                // Items
                SqlDataAdapter da = new SqlDataAdapter(@"
                SELECT ItemOrder AS SlNo, ProductName, Qnty, Rate, TaxableAmount, TaxAmount, NetAmount
                FROM tbl_RequisitionNew
                WHERE ReqNo = @ReqNo
                ORDER BY ItemOrder", con);

                da.SelectCommand.Parameters.AddWithValue("@ReqNo", reqNo);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvPreviewItems.DataSource = dt;
                gvPreviewItems.DataBind();
            }
        }

        private void LoadPreviewTotals(string reqNo)
        {
            ViewState["TotalQty"] = 0; ViewState["TotalTaxable"] = 0;
            ViewState["TotalGST"] = 0; ViewState["TotalNet"] = 0;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(@"
                SELECT SUM(Qnty) AS TotalQty, SUM(TaxableAmount) AS TotalTaxable, 
                       SUM(TaxAmount) AS TotalGST, SUM(NetAmount) AS TotalNet
                FROM tbl_RequisitionNew
                WHERE ReqNo = @ReqNo", con);

                cmd.Parameters.AddWithValue("@ReqNo", reqNo);
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
            {
                return;
            }

            string reqNo = lblPrevReqNo.Text;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    // 1. Generate the PO Draft
                    SqlCommand cmdGenerate = new SqlCommand("sp_GeneratePO_FromReqNo", con, tran);
                    cmdGenerate.CommandType = CommandType.StoredProcedure;
                    cmdGenerate.Parameters.AddWithValue("@ReqNo", reqNo);
                    cmdGenerate.Parameters.AddWithValue("@UserId", Session["USERID"].ToString());
                    cmdGenerate.ExecuteNonQuery();

                    // 2. Fetch the newly generated PO_Id
                    int poId = 0;
                    SqlCommand cmdGetId = new SqlCommand("SELECT TOP 1 PO_Id FROM tbl_PO_Header WHERE ReqNo = @ReqNo ORDER BY PO_Id DESC", con, tran);
                    cmdGetId.Parameters.AddWithValue("@ReqNo", reqNo);
                    object result = cmdGetId.ExecuteScalar();

                    if (result != null)
                    {
                        poId = Convert.ToInt32(result);
                    }
                    else
                    {
                        throw new Exception("Could not retrieve the newly generated PO ID.");
                    }

                    // 3. Update the Draft with the Operational Details
                    SqlCommand cmdUpdate = new SqlCommand(@"
                        UPDATE tbl_PO_Header
                        SET EngineerName = @Engineer, DispatchMode = @DispatchMode, 
                            DispatchUpto = @DispatchUpto, DeliveryBasis = @DeliveryBasis, 
                            FreightTerms = @FreightTerms, Remarks = @Remarks
                        WHERE PO_Id = @PO_Id", con, tran);

                    cmdUpdate.Parameters.AddWithValue("@Engineer", txtEngineerName.Text.Trim());
                    cmdUpdate.Parameters.AddWithValue("@DispatchMode", ddlDispatchMode.SelectedValue);
                    cmdUpdate.Parameters.AddWithValue("@DispatchUpto", txtDispatchUpto.Text.Trim());
                    cmdUpdate.Parameters.AddWithValue("@DeliveryBasis", ddlDeliveryBasis.SelectedValue);
                    cmdUpdate.Parameters.AddWithValue("@FreightTerms", ddlFreightTerms.SelectedValue);
                    cmdUpdate.Parameters.AddWithValue("@Remarks", txtRemarks.Text.Trim());
                    cmdUpdate.Parameters.AddWithValue("@PO_Id", poId);
                    cmdUpdate.ExecuteNonQuery();

                    // 4. Save the Party Snapshots exactly like View_PO_Details
                    SavePartySnapshots(con, tran, poId);

                    // Commit everything
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    lblError.Text = "An error occurred while generating the PO: " + ex.Message;
                    return;
                }
            }

            // Redirect back to list page upon success
            Response.Redirect("Generate_PO_From_PR.aspx");
        }

        #endregion

        #region Party Snapshots Saving Logic

        private void SavePartySnapshots(SqlConnection con, SqlTransaction tran, int poId)
        {
            // Always clean existing (safety if reattempt)
            SqlCommand del = new SqlCommand("DELETE FROM tbl_PO_PartySnapshot WHERE PO_Id = @PO_Id", con, tran);
            del.Parameters.AddWithValue("@PO_Id", poId);
            del.ExecuteNonQuery();

            // 1️⃣ Vendor (always from PO header)
            InsertVendorSnapshot(con, tran, poId);

            // 2️⃣ Bill To
            if (rblBillToType.SelectedValue == "Company")
                InsertCompanySnapshot(con, tran, poId, "BillTo", Convert.ToInt32(ddlBillToCompany.SelectedValue));
            else
                InsertStoreSnapshot(con, tran, poId, "BillTo", Convert.ToInt32(ddlBillToStore.SelectedValue));

            // 3️⃣ Ship To
            if (rblShipToType.SelectedValue == "Store")
                InsertStoreSnapshot(con, tran, poId, "ShipTo", Convert.ToInt32(ddlShipToStore.SelectedValue));
            else
                InsertClientSnapshot(con, tran, poId, "ShipTo", Convert.ToInt32(ddlShipToClient.SelectedValue));
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

        private void InsertCompanySnapshot(SqlConnection con, SqlTransaction tran, int poId, string role, int companyId)
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

        private void InsertClientSnapshot(SqlConnection con, SqlTransaction tran, int poId, string role, int clientId)
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

        #endregion
    }
}