using System;
using System.Web.UI;

namespace Bill_Software.corporate.business.app
{
    public partial class SwitchUser : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Route retained. ImpersonationRuntime is gated on SwitchUser=false and
            // is not invoked from this page. Session-only AuthGuard. No identity swap.
            AuthGuard.EnsurePage(this, false, null);
        }
    }
}
