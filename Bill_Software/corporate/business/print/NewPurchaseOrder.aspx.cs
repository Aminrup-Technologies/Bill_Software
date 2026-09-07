using System;
using System.Web.UI;
using Bill_Software.corporate.business.app;

namespace Bill_Software.corporate.business.print
{
    public partial class NewPurchaseOrder : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!AuthGuard.EnsurePrint(this, "tbl_Quotation.ID", Request.QueryString["ID"])) return;


            if (!IsPostBack)
            {
                new PurchaseOrderPrintHelper(this).Bind(Request.QueryString["ID"]);
            }
        }
    }
}
