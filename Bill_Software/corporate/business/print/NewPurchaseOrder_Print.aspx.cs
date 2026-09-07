using System;
using System.Web.UI;
using Bill_Software.corporate.business.app;

namespace Bill_Software.corporate.business.print
{
    public partial class NewPurchaseOrder_Print : System.Web.UI.Page
    {
        protected bool ShowLetterhead { get; private set; }
        protected bool AutoPrint { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthGuard.EnsurePrint(this, "tbl_Quotation.ID", Request.QueryString["ID"])) return;


            ShowLetterhead = Request.QueryString["letterhead"] != "0";
            AutoPrint = Request.QueryString["autoprint"] == "1";

            if (!IsPostBack)
            {
                new PurchaseOrderPrintHelper(this).Bind(Request.QueryString["ID"]);
            }
        }
    }
}
