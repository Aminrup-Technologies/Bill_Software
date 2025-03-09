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
    public partial class Bill : System.Web.UI.MasterPage
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dtm = new DataTable();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lbl_crntyr.Text = DateTime.Now.Year.ToString();
                GetMenuControl();
            }
            HttpContext.Current.Response.Cache.SetAllowResponseInBrowserHistory(false);
            HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            HttpContext.Current.Response.Cache.SetNoStore();

            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx", false);
            }
            GetAdminName();

        }

        private void GetAdminName()
        {
            string UserName = Session["USERID"].ToString();
            string cmdString = "select Name from tbl_login where User_Id='" + UserName + "'";
            DbCL.Sqlconnection();
            DbCL.ConnectDb();
            SqlCommand cmd = new SqlCommand(cmdString, DbCL.Conn);
            SqlDataReader Rdr;
            Rdr = cmd.ExecuteReader();
            if (Rdr.Read())
            {
                lblName.Text = Rdr["Name"].ToString();
            }
            DbCL.Conn.Close();
        }

        private void GetMenuControl()
        {
            string UserName = Session["USERID"].ToString();
            string query = "SELECT * FROM vw_FullDesignation WHERE User_Id=@User_Id";
            SqlParameter[] pram = {
                new SqlParameter("@User_Id",UserName)
            };
            dtm = DbCL.SPreturn_dt(query,pram);
            if (dtm.Rows.Count>0)
            {
                if (dtm.Rows[0]["Home"].ToString() == "Yes")
                {
                    Home.Visible = true;
                }
                else if (dtm.Rows[0]["Home"].ToString() == "No")
                {
                    Home.Visible = false;
                }

                if (dtm.Rows[0]["home1"].ToString() == "Yes")
                {
                    home1.Visible = true;
                }
                else if (dtm.Rows[0]["home1"].ToString() == "No")
                {
                    home1.Visible = false;
                }

                if (dtm.Rows[0]["settings"].ToString() == "Yes")
                {
                    settings.Visible = true;
                }
                else if (dtm.Rows[0]["settings"].ToString() == "No")
                {
                    settings.Visible = false;
                }

                if (dtm.Rows[0]["Dashboard"].ToString() == "Yes")
                {
                    Dashboard.Visible = true;
                }
                else if (dtm.Rows[0]["Dashboard"].ToString() == "No")
                {
                    Dashboard.Visible = false;
                }

                if (dtm.Rows[0]["Data_Mastering"].ToString() == "Yes")
                {
                    Data_Mastering.Visible = true;
                }
                else if (dtm.Rows[0]["Data_Mastering"].ToString() == "No")
                {
                    Data_Mastering.Visible = false;
                }

                if (dtm.Rows[0]["master_State"].ToString() == "Yes")
                {
                    master_State.Visible = true;
                }
                else if (dtm.Rows[0]["master_State"].ToString() == "No")
                {
                    master_State.Visible = false;
                }

                if (dtm.Rows[0]["master_city"].ToString() == "Yes")
                {
                    master_city.Visible = true;
                }
                else if (dtm.Rows[0]["master_city"].ToString() == "No")
                {
                    master_city.Visible = false;
                }

                if (dtm.Rows[0]["AddIndustry"].ToString() == "Yes")
                {
                    AddIndustry.Visible = true;
                }
                else if (dtm.Rows[0]["AddIndustry"].ToString() == "No")
                {
                    AddIndustry.Visible = false;
                }

                if (dtm.Rows[0]["PaymentPhase"].ToString() == "Yes")
                {
                    PaymentPhase.Visible = true;
                }
                else if (dtm.Rows[0]["PaymentPhase"].ToString() == "No")
                {
                    PaymentPhase.Visible = false;
                }

                if (dtm.Rows[0]["AddPrimaryService"].ToString() == "Yes")
                {
                    AddPrimaryService.Visible = true;
                }
                else if (dtm.Rows[0]["AddPrimaryService"].ToString() == "No")
                {
                    AddPrimaryService.Visible = false;
                }

                if (dtm.Rows[0]["PrimaryServiceTerms"].ToString() == "Yes")
                {
                    PrimaryServiceTerms.Visible = true;
                }
                else if (dtm.Rows[0]["PrimaryServiceTerms"].ToString() == "No")
                {
                    PrimaryServiceTerms.Visible = false;
                }

                if (dtm.Rows[0]["productparent"].ToString() == "Yes")
                {
                    productparent.Visible = true;
                }
                else if (dtm.Rows[0]["productparent"].ToString() == "No")
                {
                    productparent.Visible = false;
                }

                if (dtm.Rows[0]["product_master"].ToString() == "Yes")
                {
                    product_master.Visible = true;
                }
                else if (dtm.Rows[0]["product_master"].ToString() == "No")
                {
                    product_master.Visible = false;
                }

                if (dtm.Rows[0]["newproductparent"].ToString() == "Yes")
                {
                    newproductparent.Visible = true;
                }
                else if (dtm.Rows[0]["newproductparent"].ToString() == "No")
                {
                    newproductparent.Visible = false;
                }

                if (dtm.Rows[0]["newproduct_master"].ToString() == "Yes")
                {
                    newproduct_master.Visible = true;
                }
                else if (dtm.Rows[0]["newproduct_master"].ToString() == "No")
                {
                    newproduct_master.Visible = false;
                }

                if (dtm.Rows[0]["Service_master"].ToString() == "Yes")
                {
                    Service_master.Visible = true;
                }
                else if (dtm.Rows[0]["Service_master"].ToString() == "No")
                {
                    Service_master.Visible = false;
                }

                if (dtm.Rows[0]["Vat_master"].ToString() == "Yes")
                {
                    Vat_master.Visible = true;
                }
                else if (dtm.Rows[0]["Vat_master"].ToString() == "No")
                {
                    Vat_master.Visible = false;
                }

                if (dtm.Rows[0]["Service_Tax_Master"].ToString() == "Yes")
                {
                    Service_Tax_Master.Visible = true;
                }
                else if (dtm.Rows[0]["Service_Tax_Master"].ToString() == "No")
                {
                    Service_Tax_Master.Visible = false;
                }

                if (dtm.Rows[0]["Expenses_Head"].ToString() == "Yes")
                {
                    Expenses_Head.Visible = true;
                }
                else if (dtm.Rows[0]["Expenses_Head"].ToString() == "No")
                {
                    Expenses_Head.Visible = false;
                }

                if (dtm.Rows[0]["Vendor"].ToString() == "Yes")
                {
                    Vendor.Visible = true;
                }
                else if (dtm.Rows[0]["Vendor"].ToString() == "No")
                {
                    Vendor.Visible = false;
                }

                if (dtm.Rows[0]["New_vendor"].ToString() == "Yes")
                {
                    New_vendor.Visible = true;
                }
                else if (dtm.Rows[0]["New_vendor"].ToString() == "No")
                {
                    New_vendor.Visible = false;
                }

                if (dtm.Rows[0]["View_vendor"].ToString() == "Yes")
                {
                    View_vendor.Visible = true;
                }
                else if (dtm.Rows[0]["View_vendor"].ToString() == "No")
                {
                    View_vendor.Visible = false;
                }

                if (dtm.Rows[0]["Delete_vendor"].ToString() == "Yes")
                {
                    Delete_vendor.Visible = true;
                }
                else if (dtm.Rows[0]["Delete_vendor"].ToString() == "No")
                {
                    Delete_vendor.Visible = false;
                }

                if (dtm.Rows[0]["Purches_exting_vendor"].ToString() == "Yes")
                {
                    Purches_exting_vendor.Visible = true;
                }
                else if (dtm.Rows[0]["Purches_exting_vendor"].ToString() == "No")
                {
                    Purches_exting_vendor.Visible = false;
                }

                if (dtm.Rows[0]["View_purches"].ToString() == "Yes")
                {
                    View_purches.Visible = true;
                }
                else if (dtm.Rows[0]["View_purches"].ToString() == "No")
                {
                    View_purches.Visible = false;
                }

                if (dtm.Rows[0]["seartch_purtch"].ToString() == "Yes")
                {
                    seartch_purtch.Visible = true;
                }
                else if (dtm.Rows[0]["seartch_purtch"].ToString() == "No")
                {
                    seartch_purtch.Visible = false;
                }

                if (dtm.Rows[0]["Delete_purtches"].ToString() == "Yes")
                {
                    Delete_purtches.Visible = true;
                }
                else if (dtm.Rows[0]["Delete_purtches"].ToString() == "No")
                {
                    Delete_purtches.Visible = false;
                }

                if (dtm.Rows[0]["Purchess_payment"].ToString() == "Yes")
                {
                    Purchess_payment.Visible = true;
                }
                else if (dtm.Rows[0]["Purchess_payment"].ToString() == "No")
                {
                    Purchess_payment.Visible = false;
                }

                if (dtm.Rows[0]["add_payment_purchess"].ToString() == "Yes")
                {
                    add_payment_purchess.Visible = true;
                }
                else if (dtm.Rows[0]["add_payment_purchess"].ToString() == "No")
                {
                    add_payment_purchess.Visible = false;
                }

                if (dtm.Rows[0]["View_purchess_payment"].ToString() == "Yes")
                {
                    View_purchess_payment.Visible = true;
                }
                else if (dtm.Rows[0]["View_purchess_payment"].ToString() == "No")
                {
                    View_purchess_payment.Visible = false;
                }

                if (dtm.Rows[0]["Seartch_purchess_payments"].ToString() == "Yes")
                {
                    Seartch_purchess_payments.Visible = true;
                }
                else if (dtm.Rows[0]["Seartch_purchess_payments"].ToString() == "No")
                {
                    Seartch_purchess_payments.Visible = false;
                }

                if (dtm.Rows[0]["Delete_purches_payment"].ToString() == "Yes")
                {
                    Delete_purches_payment.Visible = true;
                }
                else if (dtm.Rows[0]["Delete_purches_payment"].ToString() == "No")
                {
                    Delete_purches_payment.Visible = false;
                }

                if (dtm.Rows[0]["Client"].ToString() == "Yes")
                {
                    Client.Visible = true;
                }
                else if (dtm.Rows[0]["Client"].ToString() == "No")
                {
                    Client.Visible = false;
                }

                if (dtm.Rows[0]["New_client"].ToString() == "Yes")
                {
                    New_client.Visible = true;
                }
                else if (dtm.Rows[0]["New_client"].ToString() == "No")
                {
                    New_client.Visible = false;
                }

                if (dtm.Rows[0]["View_client"].ToString() == "Yes")
                {
                    View_client.Visible = true;
                }
                else if (dtm.Rows[0]["View_client"].ToString() == "No")
                {
                    View_client.Visible = false;
                }

                if (dtm.Rows[0]["Delete_client"].ToString() == "Yes")
                {
                    Delete_client.Visible = true;
                }
                else if (dtm.Rows[0]["Delete_client"].ToString() == "No")
                {
                    Delete_client.Visible = false;
                }

                if (dtm.Rows[0]["Representative"].ToString() == "Yes")
                {
                    Representative.Visible = true;
                }
                else if (dtm.Rows[0]["Representative"].ToString() == "No")
                {
                    Representative.Visible = false;
                }

                if (dtm.Rows[0]["AddFactory"].ToString() == "Yes")
                {
                    AddFactory.Visible = true;
                }
                else if (dtm.Rows[0]["AddFactory"].ToString() == "No")
                {
                    AddFactory.Visible = false;
                }

                if (dtm.Rows[0]["Quotatio"].ToString() == "Yes")
                {
                    Quotatio.Visible = true;
                }
                else if (dtm.Rows[0]["Quotatio"].ToString() == "No")
                {
                    Quotatio.Visible = false;
                }

                if (dtm.Rows[0]["Create_quotation"].ToString() == "Yes")
                {
                    Create_quotation.Visible = true;
                }
                else if (dtm.Rows[0]["Create_quotation"].ToString() == "No")
                {
                    Create_quotation.Visible = false;
                }

                if (dtm.Rows[0]["View_quotation"].ToString() == "Yes")
                {
                    View_quotation.Visible = true;
                }
                else if (dtm.Rows[0]["View_quotation"].ToString() == "No")
                {
                    View_quotation.Visible = false;
                }

                if (dtm.Rows[0]["Seartch_quotation"].ToString() == "Yes")
                {
                    Seartch_quotation.Visible = true;
                }
                else if (dtm.Rows[0]["Seartch_quotation"].ToString() == "No")
                {
                    Seartch_quotation.Visible = false;
                }

                if (dtm.Rows[0]["Delete_Quotation"].ToString() == "Yes")
                {
                    Delete_Quotation.Visible = true;
                }
                else if (dtm.Rows[0]["Delete_Quotation"].ToString() == "No")
                {
                    Delete_Quotation.Visible = false;
                }

                if (dtm.Rows[0]["Edit_quatation"].ToString() == "Yes")
                {
                    Edit_quatation.Visible = true;
                }
                else if (dtm.Rows[0]["Edit_quatation"].ToString() == "No")
                {
                    Edit_quatation.Visible = false;
                }

                if (dtm.Rows[0]["challan"].ToString() == "Yes")
                {
                    challan.Visible = true;
                }
                else if (dtm.Rows[0]["challan"].ToString() == "No")
                {
                    challan.Visible = false;
                }

                if (dtm.Rows[0]["add_chalan"].ToString() == "Yes")
                {
                    add_chalan.Visible = true;
                }
                else if (dtm.Rows[0]["add_chalan"].ToString() == "No")
                {
                    add_chalan.Visible = false;
                }

                if (dtm.Rows[0]["View_chalan"].ToString() == "Yes")
                {
                    View_chalan.Visible = true;
                }
                else if (dtm.Rows[0]["View_chalan"].ToString() == "No")
                {
                    View_chalan.Visible = false;
                }

                if (dtm.Rows[0]["seartch_chalan"].ToString() == "Yes")
                {
                    seartch_chalan.Visible = true;
                }
                else if (dtm.Rows[0]["seartch_chalan"].ToString() == "No")
                {
                    seartch_chalan.Visible = false;
                }

                if (dtm.Rows[0]["Delete_chalan"].ToString() == "Yes")
                {
                    Delete_chalan.Visible = true;
                }
                else if (dtm.Rows[0]["Delete_chalan"].ToString() == "No")
                {
                    Delete_chalan.Visible = false;
                }

                if (dtm.Rows[0]["proforma"].ToString() == "Yes")
                {
                    proforma.Visible = true;
                }
                else if (dtm.Rows[0]["proforma"].ToString() == "No")
                {
                    proforma.Visible = false;
                }

                if (dtm.Rows[0]["Add_proforma"].ToString() == "Yes")
                {
                    Add_proforma.Visible = true;
                }
                else if (dtm.Rows[0]["Add_proforma"].ToString() == "No")
                {
                    Add_proforma.Visible = false;
                }

                if (dtm.Rows[0]["View_proforma"].ToString() == "Yes")
                {
                    View_proforma.Visible = true;
                }
                else if (dtm.Rows[0]["View_proforma"].ToString() == "No")
                {
                    View_proforma.Visible = false;
                }

                if (dtm.Rows[0]["Seartch_proforma"].ToString() == "Yes")
                {
                    Seartch_proforma.Visible = true;
                }
                else if (dtm.Rows[0]["Seartch_proforma"].ToString() == "No")
                {
                    Seartch_proforma.Visible = false;
                }

                if (dtm.Rows[0]["Delete_proforma"].ToString() == "Yes")
                {
                    Delete_proforma.Visible = true;
                }
                else if (dtm.Rows[0]["Delete_proforma"].ToString() == "No")
                {
                    Delete_proforma.Visible = false;
                }

                if (dtm.Rows[0]["Invoice"].ToString() == "Yes")
                {
                    Invoice.Visible = true;
                }
                else if (dtm.Rows[0]["Invoice"].ToString() == "No")
                {
                    Invoice.Visible = false;
                }

                if (dtm.Rows[0]["Add_invoice"].ToString() == "Yes")
                {
                    Add_invoice.Visible = true;
                }
                else if (dtm.Rows[0]["Add_invoice"].ToString() == "No")
                {
                    Add_invoice.Visible = false;
                }

                if (dtm.Rows[0]["View_Invoice"].ToString() == "Yes")
                {
                    View_Invoice.Visible = true;
                }
                else if (dtm.Rows[0]["View_Invoice"].ToString() == "No")
                {
                    View_Invoice.Visible = false;
                }

                if (dtm.Rows[0]["seartch_invoice"].ToString() == "Yes")
                {
                    seartch_invoice.Visible = true;
                }
                else if (dtm.Rows[0]["seartch_invoice"].ToString() == "No")
                {
                    seartch_invoice.Visible = false;
                }

                if (dtm.Rows[0]["Delete_invoice"].ToString() == "Yes")
                {
                    Delete_invoice.Visible = true;
                }
                else if (dtm.Rows[0]["Delete_invoice"].ToString() == "No")
                {
                    Delete_invoice.Visible = false;
                }

                if (dtm.Rows[0]["Block_invoice"].ToString() == "Yes")
                {
                    Block_invoice.Visible = true;
                }
                else if (dtm.Rows[0]["Block_invoice"].ToString() == "No")
                {
                    Block_invoice.Visible = false;
                }

                if (dtm.Rows[0]["Payment"].ToString() == "Yes")
                {
                    Payment.Visible = true;
                }
                else if (dtm.Rows[0]["Payment"].ToString() == "No")
                {
                    Payment.Visible = false;
                }

                if (dtm.Rows[0]["add_payment"].ToString() == "Yes")
                {
                    add_payment.Visible = true;
                }
                else if (dtm.Rows[0]["add_payment"].ToString() == "No")
                {
                    add_payment.Visible = false;
                }

                if (dtm.Rows[0]["View_payment"].ToString() == "Yes")
                {
                    View_payment.Visible = true;
                }
                else if (dtm.Rows[0]["View_payment"].ToString() == "No")
                {
                    View_payment.Visible = false;
                }

                if (dtm.Rows[0]["seartch_payment"].ToString() == "Yes")
                {
                    seartch_payment.Visible = true;
                }
                else if (dtm.Rows[0]["seartch_payment"].ToString() == "No")
                {
                    seartch_payment.Visible = false;
                }

                if (dtm.Rows[0]["Delete_payment"].ToString() == "Yes")
                {
                    Delete_payment.Visible = true;
                }
                else if (dtm.Rows[0]["Delete_payment"].ToString() == "No")
                {
                    Delete_payment.Visible = false;
                }

                if (dtm.Rows[0]["Epencess"].ToString() == "Yes")
                {
                    Epencess.Visible = true;
                }
                else if (dtm.Rows[0]["Epencess"].ToString() == "No")
                {
                    Epencess.Visible = false;
                }

                if (dtm.Rows[0]["general_expences"].ToString() == "Yes")
                {
                    general_expences.Visible = true;
                }
                else if (dtm.Rows[0]["general_expences"].ToString() == "No")
                {
                    general_expences.Visible = false;
                }

                if (dtm.Rows[0]["patty_cash_expences"].ToString() == "Yes")
                {
                    patty_cash_expences.Visible = true;
                }
                else if (dtm.Rows[0]["patty_cash_expences"].ToString() == "No")
                {
                    patty_cash_expences.Visible = false;
                }

                if (dtm.Rows[0]["view_expencess_head"].ToString() == "Yes")
                {
                    view_expencess_head.Visible = true;
                }
                else if (dtm.Rows[0]["view_expencess_head"].ToString() == "No")
                {
                    view_expencess_head.Visible = false;
                }

                if (dtm.Rows[0]["view_patty_cash_expenses"].ToString() == "Yes")
                {
                    view_patty_cash_expenses.Visible = true;
                }
                else if (dtm.Rows[0]["view_patty_cash_expenses"].ToString() == "No")
                {
                    view_patty_cash_expenses.Visible = false;
                }

                if (dtm.Rows[0]["Delete_general_expencess"].ToString() == "Yes")
                {
                    Delete_general_expencess.Visible = true;
                }
                else if (dtm.Rows[0]["Delete_general_expencess"].ToString() == "No")
                {
                    Delete_general_expencess.Visible = false;
                }

                if (dtm.Rows[0]["Delete_patty_cash_expenses"].ToString() == "Yes")
                {
                    Delete_general_expencess.Visible = true;
                }
                else if (dtm.Rows[0]["Delete_patty_cash_expenses"].ToString() == "No")
                {
                    Delete_general_expencess.Visible = false;
                }


                if (dtm.Rows[0]["Reports"].ToString() == "Yes")
                {
                    Reports.Visible = true;
                }
                else if (dtm.Rows[0]["Reports"].ToString() == "No")
                {
                    Reports.Visible = false;
                }

                if (dtm.Rows[0]["Payment_due"].ToString() == "Yes")
                {
                    Payment_due.Visible = true;
                }
                else if (dtm.Rows[0]["Payment_due"].ToString() == "No")
                {
                    Payment_due.Visible = false;
                }

                if (dtm.Rows[0]["Purchess_due"].ToString() == "Yes")
                {
                    Purchess_due.Visible = true;
                }
                else if (dtm.Rows[0]["Purchess_due"].ToString() == "No")
                {
                    Purchess_due.Visible = false;
                }

                if (dtm.Rows[0]["PurchaseRequisition"].ToString() == "Yes")
                {
                    PurchaseRequisition.Visible = true;
                }
                else if (dtm.Rows[0]["PurchaseRequisition"].ToString() == "No")
                {
                    PurchaseRequisition.Visible = false;
                }

                if (dtm.Rows[0]["RequisitionManual"].ToString() == "Yes")
                {
                    RequisitionManual.Visible = true;
                }
                else if (dtm.Rows[0]["RequisitionManual"].ToString() == "No")
                {
                    RequisitionManual.Visible = false;
                }

                if (dtm.Rows[0]["RequisitionManualView"].ToString() == "Yes")
                {
                    RequisitionManualView.Visible = true;
                }
                else if (dtm.Rows[0]["RequisitionManualView"].ToString() == "No")
                {
                    RequisitionManualView.Visible = false;
                }

                if (dtm.Rows[0]["RequisitionManualSearch"].ToString() == "Yes")
                {
                    RequisitionManualSearch.Visible = true;
                }
                else if (dtm.Rows[0]["RequisitionManualSearch"].ToString() == "No")
                {
                    RequisitionManualSearch.Visible = false;
                }

                if (dtm.Rows[0]["RequisitionManualDelete"].ToString() == "Yes")
                {
                    RequisitionManualDelete.Visible = true;
                }
                else if (dtm.Rows[0]["RequisitionManualDelete"].ToString() == "No")
                {
                    RequisitionManualDelete.Visible = false;
                }

                if (dtm.Rows[0]["Users"].ToString() == "Yes")
                {
                    Users.Visible = true;
                }
                else if (dtm.Rows[0]["Users"].ToString() == "No")
                {
                    Users.Visible = false;
                }

                if (dtm.Rows[0]["AddUser"].ToString() == "Yes")
                {
                    AddUser.Visible = true;
                }
                else if (dtm.Rows[0]["AddUser"].ToString() == "No")
                {
                    AddUser.Visible = false;
                }

                if (dtm.Rows[0]["ViewUser"].ToString() == "Yes")
                {
                    ViewUser.Visible = true;
                }
                else if (dtm.Rows[0]["ViewUser"].ToString() == "No")
                {
                    ViewUser.Visible = false;
                }

                if (dtm.Rows[0]["SetQuatation"].ToString() == "Yes")
                {
                    SetQuatation.Visible = true;
                }
                else if (dtm.Rows[0]["SetQuatation"].ToString() == "No")
                {
                    SetQuatation.Visible = false;
                }

                if (dtm.Rows[0]["ProformaMail"].ToString() == "Yes")
                {
                    ProformaMail.Visible = true;
                }
                else if (dtm.Rows[0]["ProformaMail"].ToString() == "No")
                {
                    ProformaMail.Visible = false;
                }

                if (dtm.Rows[0]["InvoiceMail"].ToString() == "Yes")
                {
                    InvoiceMail.Visible = true;
                }
                else if (dtm.Rows[0]["InvoiceMail"].ToString() == "No")
                {
                    InvoiceMail.Visible = false;
                }

                if (dtm.Rows[0]["PaymentMail"].ToString() == "Yes")
                {
                    PaymentMail.Visible = true;
                }
                else if (dtm.Rows[0]["PaymentMail"].ToString() == "No")
                {
                    PaymentMail.Visible = false;
                }

                if (dtm.Rows[0]["FinalPaymentInvoice"].ToString() == "Yes")
                {
                    PaymentMail.Visible = true;
                }
                else if (dtm.Rows[0]["FinalPaymentInvoice"].ToString() == "No")
                {
                    PaymentMail.Visible = false;
                }
            }
        }

        protected void btnLogOut_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/index.aspx", false);
        }
    }
}