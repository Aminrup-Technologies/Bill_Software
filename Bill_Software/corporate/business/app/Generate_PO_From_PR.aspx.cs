using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Bill_Software.corporate.business.app
{
    public partial class Generate_PO_From_PR : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindPRList();
            }
            else if (ViewState["PreviewReqNo"] != null)
            {
                pnlPreview.Visible = true; // ✅ keep preview visible
            }
        }

        private void BindPRList()
        {
            using (SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(@"
                SELECT
                    ReqNo,
                    clientName,
                    Vendor,
                    VendorId,
                    NetAmount,
                    ApprovedBy,
                    ApprovedOn,
                    CreatedBy,
                    CreatedOn
                FROM tbl_RequisitionMain
                WHERE Status = 'Approved'
                ORDER BY ApprovedOn DESC", con);

                DataTable dt = new DataTable();
                da.Fill(dt);

                DataList1.DataSource = dt;
                DataList1.DataBind();
            }
        }

        protected void DataList1_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item ||
                e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Label lblSl = (Label)e.Item.FindControl("lblSlNo");
                lblSl.Text = (e.Item.ItemIndex + 1).ToString();
            }
        }


        protected void DataList1_ItemCommand(object source, DataListCommandEventArgs e)
        {
            string reqNo = e.CommandArgument.ToString();

            if (e.CommandName == "Preview")
            {
                //ShowPreview(reqNo);
                LoadPreview(reqNo);
            }
            else if (e.CommandName == "Convert")
            {
                //GeneratePO(reqNo);
            }
        }

        private void ShowPreview(string reqNo)
        {
            pnlPreview.Visible = true;
            ViewState["PreviewReqNo"] = reqNo;

            using (SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                con.Open();

                // Header
                SqlCommand cmdHdr = new SqlCommand(
                    "SELECT Vendor FROM tbl_RequisitionMain WHERE ReqNo=@ReqNo", con);
                cmdHdr.Parameters.AddWithValue("@ReqNo", reqNo);

                lblPrevReqNo.Text = reqNo;
                lblPrevVendor.Text = cmdHdr.ExecuteScalar().ToString();

                // Items
                SqlDataAdapter da = new SqlDataAdapter(@"
            SELECT ProductName, Qnty, Rate, gstrate,
                   (Qnty * Rate) AS NetAmount
            FROM tbl_RequisitionNew
            WHERE ReqNo=@ReqNo", con);

                da.SelectCommand.Parameters.AddWithValue("@ReqNo", reqNo);

                DataTable dt = new DataTable();
                da.Fill(dt);

                ViewState["PreviewItems"] = dt;   // ✅ STORE HERE

                gvPreviewItems.DataSource = dt;
                gvPreviewItems.DataBind();
            }
        }

        private void LoadPreview(string reqNo)
        {
            pnlPreview.Visible = true;              // ✅ SHOW preview panel
            ViewState["PreviewReqNo"] = reqNo;       // ✅ REQUIRED for Create PO

            using (SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(@"
            SELECT
                ItemOrder AS SlNo,
                ProductName,
                Qnty,
                Rate,
                TaxableAmount,
                TaxAmount,
                NetAmount
            FROM tbl_RequisitionNew
            WHERE ReqNo = @ReqNo
            ORDER BY ItemOrder", con);

                da.SelectCommand.Parameters.AddWithValue("@ReqNo", reqNo);

                DataTable dt = new DataTable();
                da.Fill(dt);

                ViewState["PreviewItems"] = dt;

                gvPreviewItems.DataSource = dt;
                gvPreviewItems.DataBind();
            }

            LoadPreviewTotals(reqNo);
            gvPreviewItems.DataBind();
        }


        private void LoadPreviewTotals(string reqNo)
        {
            ViewState["TotalQty"] = 0;
            ViewState["TotalTaxable"] = 0;
            ViewState["TotalGST"] = 0;
            ViewState["TotalNet"] = 0;


            using (SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(@"
            SELECT
                SUM(Qnty)         AS TotalQty,
                SUM(TaxableAmount) AS TotalTaxable,
                SUM(TaxAmount)     AS TotalGST,
                SUM(NetAmount)     AS TotalNet
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



        protected void btnCreatePO_Click(object sender, EventArgs e)
        {
            string reqNo = ViewState["PreviewReqNo"]?.ToString();
            if (string.IsNullOrEmpty(reqNo)) return;

            using (SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_GeneratePO_FromReqNo", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ReqNo", reqNo);
                cmd.Parameters.AddWithValue("@UserId", Session["USERID"].ToString());

                con.Open();
                cmd.ExecuteNonQuery();
            }

            pnlPreview.Visible = false;
            BindPRList();
        }

        protected void btnCancelPreview_Click(object sender, EventArgs e)
        {
            ViewState.Remove("PreviewReqNo");
            ViewState.Remove("PreviewItems");
            pnlPreview.Visible = false;

        }

        //protected void gvPreviewItems_RowDataBound(object sender, GridViewRowEventArgs e)
        //{
        //    if (e.Row.RowType == DataControlRowType.Footer)
        //    {
        //        DataTable dt = ViewState["PreviewItems"] as DataTable;
        //        if (dt == null || dt.Rows.Count == 0)
        //            return;

        //        decimal total = 0;

        //        foreach (DataRow row in dt.Rows)
        //        {
        //            if (row["NetAmount"] != DBNull.Value)
        //                total += Convert.ToDecimal(row["NetAmount"]);
        //        }

        //        e.Row.Cells[0].Text = "Total";
        //        e.Row.Cells[0].ColumnSpan = 4;
        //        e.Row.Cells[0].HorizontalAlign = HorizontalAlign.Right;

        //        e.Row.Cells[1].Text = total.ToString("N2");
        //        e.Row.Cells[1].CssClass = "num amount";
        //    }
        //}

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



    }
}