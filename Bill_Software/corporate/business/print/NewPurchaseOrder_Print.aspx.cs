using System;
using System.Web.UI;

namespace Bill_Software.corporate.business.print
{
    public partial class NewPurchaseOrder_Print : System.Web.UI.Page
    {
        protected bool ShowLetterhead { get; private set; }
        protected bool AutoPrint { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["USERID"] == null || Session["CompanyID"] == null)
            {
                Response.Redirect("~/index.aspx");
                return;
            }

            ShowLetterhead = Request.QueryString["letterhead"] != "0";
            AutoPrint = Request.QueryString["autoprint"] == "1";

            if (!IsPostBack)
            {
                new PurchaseOrderPrintHelper(this).Bind(Request.QueryString["ID"]);
            }
        }
    }
}
