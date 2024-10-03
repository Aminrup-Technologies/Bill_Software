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
    public partial class WebForm81 : System.Web.UI.Page
    {
        DB_UTILITY DbCL = new DB_UTILITY();
        DataTable dtuse = new DataTable();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["USERID"] == null)
            {
                Response.Redirect("~/index.aspx");
            }
            if (!IsPostBack)
            {
                string EMPID = Request.QueryString["User_Id"];
                
                
                BindDesignation(EMPID);
            }
        }

        private void BindDesignation(string empid)
        {
            string query = "select User_Id,Name,Home,home1,settings,Dashboard,Data_Mastering,master_State,master_city,AddIndustry,PaymentPhase,AddPrimaryService,PrimaryServiceTerms,productparent,product_master,newproductparent,newproduct_master,Service_master,Vat_master,Service_Tax_Master,Expenses_Head,Vendor,New_vendor,View_vendor,Delete_vendor,Purches_exting_vendor,View_purches,seartch_purtch,Delete_purtches,Purchess_payment,add_payment_purchess,View_purchess_payment,Seartch_purchess_payments,Delete_purches_payment,Client,New_client,View_client,Delete_client,Representative,AddFactory,Quotatio,Create_quotation,View_quotation,Seartch_quotation,Delete_Quotation,Edit_quatation,challan,add_chalan,View_chalan,seartch_chalan,Delete_chalan,proforma,Add_proforma,View_proforma,Seartch_proforma,Delete_proforma,Invoice,Add_invoice,View_Invoice,seartch_invoice,Delete_invoice,Block_invoice,Payment,add_payment,View_payment,seartch_payment,Delete_payment,Epencess,general_expences,patty_cash_expences,view_expencess_head,view_patty_cash_expenses,Delete_general_expencess,Delete_patty_cash_expenses,Reports,Payment_due,Purchess_due,PurchaseRequisition,RequisitionManual,RequisitionManualView,RequisitionManualSearch,RequisitionManualDelete,Users,AddUser,ViewUser,SetQuatation,ProformaMail,InvoiceMail,PaymentMail,FinalPaymentInvoice,PaymentsDue from tbl_Designation where User_Id=@User_Id";
            SqlParameter[] pram = {
                new SqlParameter("@User_Id",empid)
            };

            dtuse = DbCL.SPreturn_dt(query, pram);
            if (dtuse.Rows.Count>0)
            {
                lblEmpId.Text = empid;
                lblEmpName.Text= dtuse.Rows[0]["Name"].ToString();

                Home.Text = dtuse.Rows[0]["Home"].ToString();
                home1.Text = dtuse.Rows[0]["home1"].ToString();
                settings.Text = dtuse.Rows[0]["settings"].ToString();
                Dashboard.Text = dtuse.Rows[0]["Dashboard"].ToString();
                Data_Mastering.Text = dtuse.Rows[0]["Data_Mastering"].ToString();
                master_State.Text = dtuse.Rows[0]["master_State"].ToString();
                master_city.Text = dtuse.Rows[0]["master_city"].ToString();
                AddIndustry.Text = dtuse.Rows[0]["AddIndustry"].ToString();
                PaymentPhase.Text = dtuse.Rows[0]["PaymentPhase"].ToString();
                AddPrimaryService.Text = dtuse.Rows[0]["AddPrimaryService"].ToString();
                PrimaryServiceTerms.Text = dtuse.Rows[0]["PrimaryServiceTerms"].ToString();
                productparent.Text = dtuse.Rows[0]["productparent"].ToString();
                product_master.Text = dtuse.Rows[0]["product_master"].ToString();
                newproductparent.Text = dtuse.Rows[0]["newproductparent"].ToString();
                newproduct_master.Text = dtuse.Rows[0]["newproduct_master"].ToString();
                Service_master.Text = dtuse.Rows[0]["Service_master"].ToString();
                Vat_master.Text = dtuse.Rows[0]["Vat_master"].ToString();
                Service_Tax_Master.Text = dtuse.Rows[0]["Service_Tax_Master"].ToString();
                Expenses_Head.Text = dtuse.Rows[0]["Expenses_Head"].ToString();
                Vendor.Text = dtuse.Rows[0]["Vendor"].ToString();
                New_vendor.Text = dtuse.Rows[0]["New_vendor"].ToString();
                View_vendor.Text = dtuse.Rows[0]["View_vendor"].ToString();
                Delete_vendor.Text = dtuse.Rows[0]["Delete_vendor"].ToString();
                Purches_exting_vendor.Text = dtuse.Rows[0]["Purches_exting_vendor"].ToString();
                View_purches.Text = dtuse.Rows[0]["View_purches"].ToString();
                seartch_purtch.Text = dtuse.Rows[0]["seartch_purtch"].ToString();
                Delete_purtches.Text = dtuse.Rows[0]["Delete_purtches"].ToString();
                Purchess_payment.Text = dtuse.Rows[0]["Purchess_payment"].ToString();
                add_payment_purchess.Text = dtuse.Rows[0]["add_payment_purchess"].ToString();
                View_purchess_payment.Text = dtuse.Rows[0]["View_purchess_payment"].ToString();
                Seartch_purchess_payments.Text = dtuse.Rows[0]["Seartch_purchess_payments"].ToString();
                Delete_purches_payment.Text = dtuse.Rows[0]["Delete_purches_payment"].ToString();
                Client.Text = dtuse.Rows[0]["Client"].ToString();
                New_client.Text = dtuse.Rows[0]["New_client"].ToString();
                View_client.Text = dtuse.Rows[0]["View_client"].ToString();
                Delete_client.Text = dtuse.Rows[0]["Delete_client"].ToString();
                Representative.Text = dtuse.Rows[0]["Representative"].ToString();
                AddFactory.Text = dtuse.Rows[0]["AddFactory"].ToString();
                Quotatio.Text = dtuse.Rows[0]["Quotatio"].ToString();
                Create_quotation.Text = dtuse.Rows[0]["Create_quotation"].ToString();
                View_quotation.Text = dtuse.Rows[0]["View_quotation"].ToString();
                Seartch_quotation.Text = dtuse.Rows[0]["Seartch_quotation"].ToString();
                Delete_Quotation.Text = dtuse.Rows[0]["Delete_Quotation"].ToString();
                Edit_quatation.Text = dtuse.Rows[0]["Edit_quatation"].ToString();
                challan.Text = dtuse.Rows[0]["challan"].ToString();
                add_chalan.Text = dtuse.Rows[0]["add_chalan"].ToString();
                View_chalan.Text = dtuse.Rows[0]["View_chalan"].ToString();
                seartch_chalan.Text = dtuse.Rows[0]["seartch_chalan"].ToString();
                Delete_chalan.Text = dtuse.Rows[0]["Delete_chalan"].ToString();
                proforma.Text = dtuse.Rows[0]["proforma"].ToString();
                Add_proforma.Text = dtuse.Rows[0]["Add_proforma"].ToString();
                View_proforma.Text = dtuse.Rows[0]["View_proforma"].ToString();
                Seartch_proforma.Text = dtuse.Rows[0]["Seartch_proforma"].ToString();
                Delete_proforma.Text = dtuse.Rows[0]["Delete_proforma"].ToString();
                Invoice.Text = dtuse.Rows[0]["Invoice"].ToString();
                Add_invoice.Text = dtuse.Rows[0]["Add_invoice"].ToString();
                View_Invoice.Text = dtuse.Rows[0]["View_Invoice"].ToString();
                seartch_invoice.Text = dtuse.Rows[0]["seartch_invoice"].ToString();
                Delete_invoice.Text = dtuse.Rows[0]["Delete_invoice"].ToString();
                Block_invoice.Text = dtuse.Rows[0]["Block_invoice"].ToString();
                Payment.Text = dtuse.Rows[0]["Payment"].ToString();
                add_payment.Text = dtuse.Rows[0]["add_payment"].ToString();
                View_payment.Text = dtuse.Rows[0]["View_payment"].ToString();
                seartch_payment.Text = dtuse.Rows[0]["seartch_payment"].ToString();
                Delete_payment.Text = dtuse.Rows[0]["Delete_payment"].ToString();
                Epencess.Text = dtuse.Rows[0]["Epencess"].ToString();
                general_expences.Text = dtuse.Rows[0]["general_expences"].ToString();
                patty_cash_expences.Text = dtuse.Rows[0]["patty_cash_expences"].ToString();
                view_expencess_head.Text = dtuse.Rows[0]["view_expencess_head"].ToString();
                view_patty_cash_expenses.Text = dtuse.Rows[0]["view_patty_cash_expenses"].ToString();
                Delete_general_expencess.Text = dtuse.Rows[0]["Delete_general_expencess"].ToString();
                Delete_patty_cash_expenses.Text = dtuse.Rows[0]["Delete_patty_cash_expenses"].ToString();
                Reports.Text = dtuse.Rows[0]["Reports"].ToString();
                Payment_due.Text = dtuse.Rows[0]["Payment_due"].ToString();
                Purchess_due.Text = dtuse.Rows[0]["Purchess_due"].ToString();
                PurchaseRequisition.Text = dtuse.Rows[0]["PurchaseRequisition"].ToString();
                RequisitionManual.Text = dtuse.Rows[0]["RequisitionManual"].ToString();
                RequisitionManualView.Text = dtuse.Rows[0]["RequisitionManualView"].ToString();
                RequisitionManualSearch.Text = dtuse.Rows[0]["RequisitionManualSearch"].ToString();
                RequisitionManualDelete.Text = dtuse.Rows[0]["RequisitionManualDelete"].ToString();

                Users.Text = dtuse.Rows[0]["Users"].ToString();
                AddUser.Text = dtuse.Rows[0]["AddUser"].ToString();
                ViewUser.Text = dtuse.Rows[0]["ViewUser"].ToString();


                SetQuatation.Text = dtuse.Rows[0]["SetQuatation"].ToString();
                ProformaMail.Text = dtuse.Rows[0]["ProformaMail"].ToString();

                InvoiceMail.Text = dtuse.Rows[0]["InvoiceMail"].ToString();
                PaymentMail.Text = dtuse.Rows[0]["PaymentMail"].ToString();
                FinalPaymentInvoice.Text = dtuse.Rows[0]["FinalPaymentInvoice"].ToString();

                PaymentsDue.Text = dtuse.Rows[0]["PaymentsDue"].ToString();

                
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (lblEmpId.Text != "")
            {

                string query = "update tbl_Designation set Home=@Home,home1=@home1,settings=@settings,Dashboard=@Dashboard,Data_Mastering=@Data_Mastering,master_State=@master_State,master_city=@master_city,AddIndustry=@AddIndustry,PaymentPhase=@PaymentPhase,AddPrimaryService=@AddPrimaryService,PrimaryServiceTerms=@PrimaryServiceTerms,productparent=@productparent,product_master=@product_master,newproductparent=@newproductparent,newproduct_master=@newproduct_master,Service_master=@Service_master,Vat_master=@Vat_master,Service_Tax_Master=@Service_Tax_Master,Expenses_Head=@Expenses_Head,Vendor=@Vendor,New_vendor=@New_vendor,View_vendor=@View_vendor,Delete_vendor=@Delete_vendor,Purches_exting_vendor=@Purches_exting_vendor,View_purches=@View_purches,seartch_purtch=@seartch_purtch,Delete_purtches=@Delete_purtches,Purchess_payment=@Purchess_payment,add_payment_purchess=@add_payment_purchess,View_purchess_payment=@View_purchess_payment,Seartch_purchess_payments=@Seartch_purchess_payments,Delete_purches_payment=@Delete_purches_payment,Client=@Client,New_client=@New_client,View_client=@View_client,Delete_client=@Delete_client,Representative=@Representative,AddFactory=@AddFactory,Quotatio=@Quotatio,Create_quotation=@Create_quotation,View_quotation=@View_quotation,Seartch_quotation=@Seartch_quotation,Delete_Quotation=@Delete_Quotation,Edit_quatation=@Edit_quatation,challan=@challan,add_chalan=@add_chalan,View_chalan=@View_chalan,seartch_chalan=@seartch_chalan,Delete_chalan=@Delete_chalan,proforma=@proforma,Add_proforma=@Add_proforma,View_proforma=@View_proforma,Seartch_proforma=@Seartch_proforma,Delete_proforma=@Delete_proforma,Invoice=@Invoice,Add_invoice=@Add_invoice,View_Invoice=@View_Invoice,seartch_invoice=@seartch_invoice,Delete_invoice=@Delete_invoice,Block_invoice=@Block_invoice,Payment=@Payment,add_payment=@add_payment,View_payment=@View_payment,seartch_payment=@seartch_payment,Delete_payment=@Delete_payment,Epencess=@Epencess,general_expences=@general_expences,patty_cash_expences=@patty_cash_expences,view_expencess_head=@view_expencess_head,view_patty_cash_expenses=@view_patty_cash_expenses,Delete_general_expencess=@Delete_general_expencess,Delete_patty_cash_expenses=@Delete_patty_cash_expenses,Reports=@Reports,Payment_due=@Payment_due,Purchess_due=@Purchess_due,PurchaseRequisition=@PurchaseRequisition,RequisitionManual=@RequisitionManual,RequisitionManualView=@RequisitionManualView,RequisitionManualSearch=@RequisitionManualSearch,RequisitionManualDelete=@RequisitionManualDelete,AddUser=@AddUser,Users=@Users,ViewUser=@ViewUser,SetQuatation=@SetQuatation,ProformaMail=@ProformaMail,InvoiceMail=@InvoiceMail,PaymentMail=@PaymentMail,FinalPaymentInvoice=@FinalPaymentInvoice,PaymentsDue=@PaymentsDue where User_Id=@User_Id";
                SqlParameter[] pram = {

                    new SqlParameter("@Home",Home.Text),
                    new SqlParameter("@home1",home1.Text),
                    new SqlParameter("@settings",settings.Text),
                    new SqlParameter("@Dashboard",Dashboard.Text),
                    new SqlParameter("@Data_Mastering",Data_Mastering.Text),
                    new SqlParameter("@master_State",master_State.Text),
                    new SqlParameter("@master_city",master_city.Text),
                    new SqlParameter("@AddIndustry",AddIndustry.Text),
                    new SqlParameter("@PaymentPhase",PaymentPhase.Text),
                    new SqlParameter("@AddPrimaryService",AddPrimaryService.Text),
                    new SqlParameter("@PrimaryServiceTerms",PrimaryServiceTerms.Text),
                    new SqlParameter("@productparent",productparent.Text),
                    new SqlParameter("@product_master",product_master.Text),
                    new SqlParameter("@newproductparent",newproductparent.Text),
                    new SqlParameter("@newproduct_master",newproduct_master.Text),
                    new SqlParameter("@Service_master",Service_master.Text),
                    new SqlParameter("@Vat_master",Vat_master.Text),
                    new SqlParameter("@Service_Tax_Master",Service_Tax_Master.Text),
                    new SqlParameter("@Expenses_Head",Expenses_Head.Text),
                    new SqlParameter("@Vendor",Vendor.Text),
                    new SqlParameter("@New_vendor",New_vendor.Text),
                    new SqlParameter("@View_vendor",View_vendor.Text),
                    new SqlParameter("@Delete_vendor",Delete_vendor.Text),
                    new SqlParameter("@Purches_exting_vendor",Purches_exting_vendor.Text),
                    new SqlParameter("@View_purches",View_purches.Text),
                    new SqlParameter("@seartch_purtch",seartch_purtch.Text),
                    new SqlParameter("@Delete_purtches",Delete_purtches.Text),
                    new SqlParameter("@Purchess_payment",Purchess_payment.Text),
                    new SqlParameter("@add_payment_purchess",add_payment_purchess.Text),
                    new SqlParameter("@View_purchess_payment",View_purchess_payment.Text),
                    new SqlParameter("@Seartch_purchess_payments",Seartch_purchess_payments.Text),
                    new SqlParameter("@Delete_purches_payment",Delete_purches_payment.Text),
                    new SqlParameter("@Client",Client.Text),
                    new SqlParameter("@New_client",New_client.Text),
                    new SqlParameter("@View_client",View_client.Text),
                    new SqlParameter("@Delete_client",Delete_client.Text),
                    new SqlParameter("@Representative",Representative.Text),
                    new SqlParameter("@AddFactory",AddFactory.Text),
                    new SqlParameter("@Quotatio",Quotatio.Text),
                    new SqlParameter("@Create_quotation",Create_quotation.Text),
                    new SqlParameter("@View_quotation",View_quotation.Text),
                    new SqlParameter("@Seartch_quotation",Seartch_quotation.Text),
                    new SqlParameter("@Delete_Quotation",Delete_Quotation.Text),
                    new SqlParameter("@Edit_quatation",Edit_quatation.Text),
                    new SqlParameter("@challan",challan.Text),
                    new SqlParameter("@add_chalan",add_chalan.Text),
                    new SqlParameter("@View_chalan",View_chalan.Text),
                    new SqlParameter("@seartch_chalan",seartch_chalan.Text),
                    new SqlParameter("@Delete_chalan",Delete_chalan.Text),
                    new SqlParameter("@proforma",proforma.Text),
                    new SqlParameter("@Add_proforma",Add_proforma.Text),
                    new SqlParameter("@View_proforma",View_proforma.Text),
                    new SqlParameter("@Seartch_proforma",Seartch_proforma.Text),
                    new SqlParameter("@Delete_proforma",Delete_proforma.Text),
                    new SqlParameter("@Invoice",Invoice.Text),
                    new SqlParameter("@Add_invoice",Add_invoice.Text),
                    new SqlParameter("@View_Invoice",View_Invoice.Text),
                    new SqlParameter("@seartch_invoice",seartch_invoice.Text),
                    new SqlParameter("@Delete_invoice",Delete_invoice.Text),
                    new SqlParameter("@Block_invoice",Block_invoice.Text),
                    new SqlParameter("@Payment",Payment.Text),
                    new SqlParameter("@add_payment",add_payment.Text),
                    new SqlParameter("@View_payment",View_payment.Text),
                    new SqlParameter("@seartch_payment",seartch_payment.Text),
                    new SqlParameter("@Delete_payment",Delete_payment.Text),
                    new SqlParameter("@Epencess",Epencess.Text),
                    new SqlParameter("@general_expences",general_expences.Text),
                    new SqlParameter("@patty_cash_expences",patty_cash_expences.Text),
                    new SqlParameter("@view_expencess_head",view_expencess_head.Text),
                    new SqlParameter("@view_patty_cash_expenses",view_patty_cash_expenses.Text),
                    new SqlParameter("@Delete_general_expencess",Delete_general_expencess.Text),
                    new SqlParameter("@Delete_patty_cash_expenses",Delete_patty_cash_expenses.Text),
                    new SqlParameter("@Reports",Reports.Text),
                    new SqlParameter("@Payment_due",Payment_due.Text),
                    new SqlParameter("@Purchess_due",Purchess_due.Text),
                    new SqlParameter("@PurchaseRequisition",PurchaseRequisition.Text),
                    new SqlParameter("@RequisitionManual",RequisitionManual.Text),
                    new SqlParameter("@RequisitionManualView",RequisitionManualView.Text),
                    new SqlParameter("@RequisitionManualSearch",RequisitionManualSearch.Text),
                    new SqlParameter("@RequisitionManualDelete",RequisitionManualDelete.Text),

                    new SqlParameter("@Users",Users.Text),
                    new SqlParameter("@AddUser",AddUser.Text),
                    new SqlParameter("@ViewUser",ViewUser.Text),

                    new SqlParameter("@User_Id",lblEmpId.Text),


                    new SqlParameter("@SetQuatation",SetQuatation.Text),
                    new SqlParameter("@ProformaMail",ProformaMail.Text),
                    new SqlParameter("@InvoiceMail",InvoiceMail.Text),
                    new SqlParameter("@PaymentMail",PaymentMail.Text),
                    new SqlParameter("@FinalPaymentInvoice",FinalPaymentInvoice.Text),

                    new SqlParameter("@PaymentsDue",PaymentsDue.Text)

                };

                DbCL.SPExecDB(query, pram);

                PanelOK.Visible = true;
                lblOk.Text = "Data Save Successfully...";
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/corporate/business/app/ViewUser.aspx");
        }
    }
}