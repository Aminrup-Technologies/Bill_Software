using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;

namespace Bill_Software.corporate.business.app
{
    public static class AuthGuard
    {
        private static string ConnString
        {
            get { return ConfigurationManager.ConnectionStrings["DbConn"].ConnectionString; }
        }

        public static bool TryValidateSession(HttpContext ctx)
        {
            if (ctx == null || ctx.Session == null) return false;
            if (ctx.Session["USERID"] == null || ctx.Session["SessionToken"] == null) return false;

            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand("SELECT IsActive FROM dbo.ActiveSessions WHERE SessionToken = @Token", cn))
            {
                cmd.Parameters.AddWithValue("@Token", ctx.Session["SessionToken"].ToString());
                cn.Open();
                object result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value || !Convert.ToBoolean(result))
                {
                    ctx.Session.Clear();
                    ctx.Session.Abandon();
                    return false;
                }
            }
            return true;
        }

        public static bool HasCompanyContext()
        {
            return CompanyContext.CurrentCompanyID > 0;
        }

        public static bool HasPermission(string permissionKey)
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null || ctx.Session == null || ctx.Session["USERID"] == null) return false;
            if (string.IsNullOrEmpty(permissionKey) || !HasCompanyContext()) return false;

            const string sql = @"
                SELECT TOP 1 1
                FROM dbo.Permissions p
                INNER JOIN dbo.RolePermissions rp ON p.PermissionId = rp.PermissionId
                INNER JOIN dbo.UserRoles ur ON rp.RoleId = ur.RoleId
                INNER JOIN dbo.tbl_login u ON ur.UserId = u.Id AND u.CompanyID = @CompanyID
                WHERE u.User_Id = @UserId AND p.PermissionKey = @Key";

            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 100) { Value = ctx.Session["USERID"].ToString() });
                cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyContext.CurrentCompanyID });
                cmd.Parameters.Add(new SqlParameter("@Key", SqlDbType.NVarChar, 100) { Value = permissionKey });
                cn.Open();
                return cmd.ExecuteScalar() != null;
            }
        }

        public static bool RecordInCompany(string mapKey, string id)
        {
            if (string.IsNullOrEmpty(id) || !HasCompanyContext()) return false;
            string sql = TenantSql(mapKey);
            if (sql == null) return false;

            using (var cn = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.NVarChar, 100) { Value = id });
                cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyContext.CurrentCompanyID });
                cn.Open();
                return cmd.ExecuteScalar() != null;
            }
        }

        public static bool EnsurePage(Page page, bool requireCompany, string permissionKey)
        {
            HttpContext ctx = page != null ? page.Context : HttpContext.Current;
            if (!TryValidateSession(ctx))
            {
                RedirectLogin(ctx);
                return false;
            }
            if (requireCompany && !HasCompanyContext())
            {
                Deny(ctx, 401);
                return false;
            }
            if (!string.IsNullOrEmpty(permissionKey) && !HasPermission(permissionKey))
            {
                Deny(ctx, 403);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Session + company, then 403 unless the user has at least one listed key.
        /// Empty/null key list fails closed. Does not change EnsurePage.
        /// </summary>
        public static bool EnsurePageAny(Page page, bool requireCompany, string[] permissionKeys)
        {
            HttpContext ctx = page != null ? page.Context : HttpContext.Current;
            if (!TryValidateSession(ctx))
            {
                RedirectLogin(ctx);
                return false;
            }
            if (requireCompany && !HasCompanyContext())
            {
                Deny(ctx, 401);
                return false;
            }
            if (permissionKeys == null || permissionKeys.Length == 0)
            {
                Deny(ctx, 403);
                return false;
            }
            for (int i = 0; i < permissionKeys.Length; i++)
            {
                if (!string.IsNullOrEmpty(permissionKeys[i]) && HasPermission(permissionKeys[i]))
                    return true;
            }
            Deny(ctx, 403);
            return false;
        }

        public static bool EnsurePrint(Page page, string mapKey, string id)
        {
            if (!EnsurePage(page, true, null)) return false;
            if (string.IsNullOrEmpty(id)) return true;
            if (RecordInCompany(mapKey, id)) return true;
            Deny(page.Context, 403);
            return false;
        }

        public static void EnsureWebMethod()
        {
            HttpContext ctx = HttpContext.Current;
            if (!TryValidateSession(ctx) || !HasCompanyContext())
                throw new HttpException(401, "Unauthorized");
        }

        public static void EnsureWebMethodPermission(string permissionKey)
        {
            EnsureWebMethod();
            if (!HasPermission(permissionKey))
                throw new HttpException(403, "Unauthorized");
        }

        private static void RedirectLogin(HttpContext ctx)
        {
            ctx.Response.Redirect("~/index.aspx", true);
        }

        private static void Deny(HttpContext ctx, int status)
        {
            ctx.Response.Clear();
            ctx.Response.StatusCode = status;
            ctx.Response.TrySkipIisCustomErrors = true;
            ctx.Response.Write("Unauthorized");
            ctx.ApplicationInstance.CompleteRequest();
            ctx.Response.End();
        }

        private static string TenantSql(string key)
        {
            switch (key)
            {
                case "tbl_Invoice.ID":
                    return "SELECT 1 FROM tbl_Invoice WHERE ID=@Id AND CompanyID=@CompanyID";
                case "tbl_Quotation.ID":
                    return "SELECT 1 FROM tbl_Quotation WHERE ID=@Id AND CompanyID=@CompanyID";
                case "tbl_Chalan.Chalan_No":
                    return "SELECT 1 FROM tbl_Chalan WHERE Chalan_No=@Id AND CompanyID=@CompanyID";
                case "tbl_Proforma.ID":
                    return "SELECT 1 FROM tbl_Proforma WHERE ID=@Id AND CompanyID=@CompanyID";
                case "tbl_RequisitionMain.ReqNo":
                    return "SELECT 1 FROM tbl_RequisitionMain WHERE ReqNo=@Id AND CompanyID=@CompanyID";
                case "tbl_PO_Header.PO_Id":
                    return "SELECT 1 FROM tbl_PO_Header WHERE PO_Id=@Id AND CompanyID=@CompanyID";
                case "tbl_invoice_payment.Payment_ID":
                    return @"SELECT 1 FROM tbl_invoice_payment p
                             INNER JOIN tbl_Quotation q ON p.Quotation_No = q.Quotation_no AND q.CompanyID=@CompanyID
                             WHERE p.Payment_ID=@Id";
                case "tbl_Purches.Purches_Id":
                    return @"SELECT 1 FROM tbl_Purches p
                             INNER JOIN tbl_Vendor v ON p.Client_Id = v.Vendor_Id AND v.CompanyID=@CompanyID
                             WHERE p.Purches_Id=@Id";
                case "tbl_HydrentInvoice.ID":
                    return @"SELECT 1 FROM tbl_HydrentInvoice h
                             INNER JOIN tbl_Client c ON h.Client_ID = c.Client_Id AND c.CompanyID=@CompanyID
                             WHERE h.ID=@Id";
                case "tbl_qsHydrentQuotation.Quotation_no":
                    return @"SELECT 1 FROM tbl_qsHydrentQuotation q
                             INNER JOIN tbl_Client c ON q.ClientId = c.Client_Id AND c.CompanyID=@CompanyID
                             WHERE q.Quotation_no=@Id";
                default:
                    return null;
            }
        }
    }
}
