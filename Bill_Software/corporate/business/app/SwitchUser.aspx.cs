using System;
using System.Web.UI;

namespace Bill_Software.corporate.business.app
{
    public partial class SwitchUser : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // PR-1: retain the route, do not activate impersonation.
            // Session-only gate via AuthGuard. No permission key, no SQL, no identity swap.
            AuthGuard.EnsurePage(this, false, null);
        }
    }
}
