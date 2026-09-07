using System;
using System.Web.UI;

namespace Bill_Software.corporate.business.app
{
    public class SecurePage : Page
    {
        protected virtual bool RequireCompanyContext { get { return true; } }
        protected virtual string RequiredPermissionKey { get { return null; } }

        /// <summary>
        /// When set, the user must have at least one of these existing PermissionKey values.
        /// Used only for pages that are children of more than one menu item.
        /// </summary>
        protected virtual string[] RequiredAnyPermissionKeys { get { return null; } }

        protected override void OnInit(EventArgs e)
        {
            string[] anyKeys = RequiredAnyPermissionKeys;
            if (anyKeys != null && anyKeys.Length > 0)
            {
                if (!AuthGuard.EnsurePageAny(this, RequireCompanyContext, anyKeys))
                    return;
            }
            else if (!AuthGuard.EnsurePage(this, RequireCompanyContext, RequiredPermissionKey))
                return;
            base.OnInit(e);
        }
    }
}
