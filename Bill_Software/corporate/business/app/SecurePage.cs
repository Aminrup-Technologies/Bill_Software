using System;
using System.Web.UI;

namespace Bill_Software.corporate.business.app
{
    public class SecurePage : Page
    {
        protected virtual bool RequireCompanyContext { get { return true; } }
        protected virtual string RequiredPermissionKey { get { return null; } }

        protected override void OnInit(EventArgs e)
        {
            if (!AuthGuard.EnsurePage(this, RequireCompanyContext, RequiredPermissionKey))
                return;
            base.OnInit(e);
        }
    }
}
