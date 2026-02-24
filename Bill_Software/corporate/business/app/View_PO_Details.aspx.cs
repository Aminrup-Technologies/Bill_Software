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
                int poId;
                if (string.IsNullOrEmpty(poIdStr) || !int.TryParse(poIdStr, out poId))
                {
                    ShowError("Invalid PO reference.");
                    return;
                }

                LoadPO(poId);
            }
        }

        private void LoadPO(int poId)
        {
            lblPO_Id.Text = poId.ToString();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                con.Open();

                /* ================= 1. PO HEADER & OPERATIONAL DETAILS ================= */
                SqlCommand cmdHdr = new SqlCommand(@"
                    SELECT H.PO_Id, H.PO_No, H.ReqNo, H.PO_Date,
                           H.PO_Status, H.IsLocked, V.Vendor_Name,
                           H.EngineerName, H.DispatchMode, H.DispatchUpto, 
                           H.DeliveryBasis, H.FreightTerms, H.Remarks
                    FROM tbl_PO_Header H
                    LEFT JOIN tbl_Vendor V ON V.Id = H.VendorId
                    WHERE H.PO_Id = @PO_Id", con);

                cmdHdr.Parameters.AddWithValue("@PO_Id", poId);

                using (SqlDataReader dr = cmdHdr.ExecuteReader())
                {
                    if (!dr.Read())
                    {
                        ShowError("PO not found.");
                        return;
                    }

                    // Header Info
                    lblPONo.Text = dr["PO_No"].ToString();
                    lblReqNo.Text = dr["ReqNo"].ToString();
                    lblStatus.Text = dr["PO_Status"].ToString();
                    lblPODate.Text = Convert.ToDateTime(dr["PO_Date"]).ToString("dd-MMM-yyyy");
                    lblVendor.Text = dr["Vendor_Name"]?.ToString();

                    // Operational Details (Maker's Input)
                    lblEngineerName.Text = dr["EngineerName"]?.ToString();
                    lblDispatchMode.Text = dr["DispatchMode"]?.ToString();
                    lblDispatchUpto.Text = dr["DispatchUpto"]?.ToString();
                    lblDeliveryBasis.Text = dr["DeliveryBasis"]?.ToString();
                    lblFreightTerms.Text = dr["FreightTerms"]?.ToString();
                    lblRemarks.Text = dr["Remarks"]?.ToString();

                    bool isLocked = Convert.ToBoolean(dr["IsLocked"]);
                    ApplyStatusUI(lblStatus.Text, isLocked);
                }

                /* ================= 2. PARTY SNAPSHOTS (BILL TO / SHIP TO) ================= */
                SqlCommand cmdSnap = new SqlCommand(@"
                    SELECT PartyRole, SourceTable, Name, Address, City, State, Pin 
                    FROM tbl_PO_PartySnapshot 
                    WHERE PO_Id = @PO_Id", con);
                cmdSnap.Parameters.AddWithValue("@PO_Id", poId);

                using (SqlDataReader drSnap = cmdSnap.ExecuteReader())
                {
                    while (drSnap.Read())
                    {
                        string role = drSnap["PartyRole"].ToString();
                        string type = drSnap["SourceTable"].ToString() == "tbl_Company" ? "Company" :
                                      drSnap["SourceTable"].ToString() == "Stores" ? "Store" : "Client";

                        string address = drSnap["Address"].ToString();
                        if (drSnap["City"] != DBNull.Value && !string.IsNullOrWhiteSpace(drSnap["City"].ToString()))
                            address += ", " + drSnap["City"].ToString();

                        if (role == "BillTo")
                        {
                            lblBillToType.Text = type;
                            lblBillToName.Text = drSnap["Name"].ToString();
                            lblBillToAddress.Text = address;
                        }
                        else if (role == "ShipTo")
                        {
                            lblShipToType.Text = type;
                            lblShipToName.Text = drSnap["Name"].ToString();
                            lblShipToAddress.Text = address;
                        }
                    }
                }

                /* ================= 3. PO ITEMS ================= */
                SqlDataAdapter da = new SqlDataAdapter(@"
                    SELECT ProductName, Quantity, Rate, DiscountPercent, DiscountAmount, 
                           TaxableAmount, TaxRate, TaxAmount, NetAmount
                    FROM tbl_PO_Items
                    WHERE PO_Id = @PO_Id
                    ORDER BY ItemOrder", con);

                da.SelectCommand.Parameters.AddWithValue("@PO_Id", poId);
                DataTable dtItems = new DataTable();
                da.Fill(dtItems);

                gdPOItems.DataSource = dtItems;
                gdPOItems.DataBind();

                /* ================= 4. UI SUMMARY ================= */
                lblGross.Text = _totalTaxable.ToString("N2");
                lblGST.Text = _totalGST.ToString("N2");
                lblNet.Text = _totalNet.ToString("N2");
            }
        }

        private void ApplyStatusUI(string status, bool isLocked)
        {
            bool isDraft = (status == "Draft");

            pnlReleaseActions.Visible = isDraft && !isLocked;
            btnPrintPO.Visible = !isDraft;

            if (!isDraft)
            {
                btnPrintPO.PostBackUrl = "~/corporate/business/print/Print_PO.aspx?poId=" + lblPO_Id.Text;
            }
        }

        protected void btnReleasePO_Click(object sender, EventArgs e)
        {
            int poId = Convert.ToInt32(lblPO_Id.Text);

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                try
                {
                    con.Open();
                    // Just execute the final release stored procedure! Maker already saved everything else.
                    SqlCommand cmd = new SqlCommand("sp_ReleasePO_Final", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PO_Id", poId);
                    cmd.Parameters.AddWithValue("@UserId", Session["USERID"].ToString());

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    ShowError("Release failed: " + ex.Message);
                    return;
                }
            }

            LoadPO(poId);
            ShowSuccess("PO has been successfully approved, released, and locked.");
        }

        protected void gdPOItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                _totalQty += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "Quantity"));
                _totalDiscount += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "DiscountAmount") ?? 0);
                _totalTaxable += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "TaxableAmount"));
                _totalGST += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "TaxAmount"));
                _totalNet += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "NetAmount"));
            }

            if (e.Row.RowType == DataControlRowType.Footer)
            {
                e.Row.Cells[0].Text = "Total";
                e.Row.Cells[0].ColumnSpan = 2;
                e.Row.Cells[1].Visible = false;

                e.Row.Cells[2].Text = _totalQty.ToString("N2");
                e.Row.Cells[5].Text = _totalDiscount.ToString("N2");
                e.Row.Cells[6].Text = _totalTaxable.ToString("N2");
                e.Row.Cells[8].Text = _totalGST.ToString("N2");
                e.Row.Cells[9].Text = _totalNet.ToString("N2");

                e.Row.Font.Bold = true;
                e.Row.BackColor = System.Drawing.ColorTranslator.FromHtml("#f0f8ff");

                e.Row.Cells[2].HorizontalAlign = HorizontalAlign.Right;
                e.Row.Cells[5].HorizontalAlign = HorizontalAlign.Right;
                e.Row.Cells[6].HorizontalAlign = HorizontalAlign.Right;
                e.Row.Cells[8].HorizontalAlign = HorizontalAlign.Right;
                e.Row.Cells[9].HorizontalAlign = HorizontalAlign.Right;
            }
        }

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
    }
}