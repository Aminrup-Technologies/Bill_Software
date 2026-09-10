using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;

namespace Bill_Software.corporate.business.app
{
    public sealed class AuthorizedCompany
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

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

        public static bool UserCanAccessCurrentCompany()
        {
            return UserCanAccessCompany(CompanyContext.CurrentCompanyID);
        }

        public static bool UserCanAccessCompany(int companyId)
        {
            HttpContext ctx = HttpContext.Current;
            if (companyId <= 0) return false;
            if (!TryValidateSession(ctx)) return false;

            int userDbId = ResolveUserDbId(ctx);
            if (userDbId <= 0) return false;

            const string sql = @"
                SELECT TOP 1 1
                FROM dbo.UserCompanyAccess a
                INNER JOIN dbo.tbl_Company c ON c.ID = a.CompanyID
                WHERE a.UserId = @UserDbId
                  AND a.CompanyID = @CompanyID
                  AND a.IsActive = 1
                  AND (c.IsActive = 1 OR c.IsActive IS NULL)";

            try
            {
                using (var cn = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add(new SqlParameter("@UserDbId", SqlDbType.Int) { Value = userDbId });
                    cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId });
                    cn.Open();
                    return cmd.ExecuteScalar() != null;
                }
            }
            catch (SqlException)
            {
                return false;
            }
        }

        public static List<AuthorizedCompany> GetAuthorizedCompanies()
        {
            var list = new List<AuthorizedCompany>();
            HttpContext ctx = HttpContext.Current;
            if (!TryValidateSession(ctx)) return list;

            int userDbId = ResolveUserDbId(ctx);
            if (userDbId <= 0) return list;

            const string sql = @"
                SELECT c.ID, c.Name
                FROM dbo.UserCompanyAccess a
                INNER JOIN dbo.tbl_Company c ON c.ID = a.CompanyID
                WHERE a.UserId = @UserDbId
                  AND a.IsActive = 1
                  AND (c.IsActive = 1 OR c.IsActive IS NULL)
                ORDER BY c.ID ASC";

            try
            {
                using (var cn = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add(new SqlParameter("@UserDbId", SqlDbType.Int) { Value = userDbId });
                    cn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            if (rdr.IsDBNull(0)) continue;
                            list.Add(new AuthorizedCompany
                            {
                                Id = Convert.ToInt32(rdr[0]),
                                Name = rdr.IsDBNull(1) ? string.Empty : rdr[1].ToString()
                            });
                        }
                    }
                }
            }
            catch (SqlException)
            {
                list.Clear();
            }

            return list;
        }

        /// <summary>
        /// One membership: that company. Multiple: tbl_login.CompanyID only if it
        /// is already an active membership. Zero or ambiguous: 0 (fail closed;
        /// do not pick the first company).
        /// </summary>
        public static int ResolveInitialCompanyId()
        {
            List<AuthorizedCompany> companies = GetAuthorizedCompanies();
            if (companies.Count == 0) return 0;
            if (companies.Count == 1) return companies[0].Id;

            int homeCompanyId = GetLoginHomeCompanyId();
            if (homeCompanyId <= 0) return 0;

            for (int i = 0; i < companies.Count; i++)
            {
                if (companies[i].Id == homeCompanyId)
                    return homeCompanyId;
            }

            return 0;
        }

        public static void ClearUnauthorizedCompanySession()
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null || ctx.Session == null) return;

            int companyId = CompanyContext.CurrentCompanyID;
            if (companyId <= 0) return;
            if (!UserCanAccessCompany(companyId))
                ctx.Session["CompanyID"] = null;
        }

        /// <summary>
        /// Inserts the user's home-tenant membership when missing. Does not
        /// grant any other company. Used by AddUser when tbl_login.CompanyID
        /// is already written for the same identity.
        /// </summary>
        public static bool TryEnsureHomeMembership(SqlConnection cn, SqlTransaction tran, int userDbId, int companyId)
        {
            if (cn == null || userDbId <= 0 || companyId <= 0)
                return false;

            using (var cmd = new SqlCommand(
                @"SELECT TOP 1 1 FROM dbo.tbl_Company
                  WHERE ID = @CompanyID AND (IsActive = 1 OR IsActive IS NULL)", cn, tran))
            {
                cmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = companyId;
                if (cmd.ExecuteScalar() == null)
                    return false;
            }

            using (var cmd = new SqlCommand(
                @"SELECT TOP 1 1 FROM dbo.UserCompanyAccess
                  WHERE UserId = @UserId AND CompanyID = @CompanyID", cn, tran))
            {
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userDbId;
                cmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = companyId;
                if (cmd.ExecuteScalar() != null)
                    return true;
            }

            using (var cmd = new SqlCommand(
                @"INSERT INTO dbo.UserCompanyAccess (UserId, CompanyID, IsActive)
                  VALUES (@UserId, @CompanyID, 1)", cn, tran))
            {
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userDbId;
                cmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = companyId;
                return cmd.ExecuteNonQuery() == 1;
            }
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
            HttpContext ctx = HttpContext.Current;
            if (!TryValidateSession(ctx))
            {
                RedirectLogin(ctx);
                return false;
            }
            if (requireCompany && !EnsureAuthorizedCompanyContext(ctx))
                return false;
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
            HttpContext ctx = HttpContext.Current;
            if (!TryValidateSession(ctx))
            {
                RedirectLogin(ctx);
                return false;
            }
            if (requireCompany && !EnsureAuthorizedCompanyContext(ctx))
                return false;
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
            Deny(HttpContext.Current, 403);
            return false;
        }

        public static void EnsureWebMethod()
        {
            HttpContext ctx = HttpContext.Current;
            if (!TryValidateSession(ctx))
                throw new HttpException(401, "Unauthorized");
            if (!HasCompanyContext())
                throw new HttpException(401, "Unauthorized");
            if (!UserCanAccessCurrentCompany())
            {
                if (ctx.Session != null)
                    ctx.Session["CompanyID"] = null;
                throw new HttpException(403, "Unauthorized");
            }
        }

        public static void EnsureWebMethodPermission(string permissionKey)
        {
            EnsureWebMethod();
            if (!HasPermission(permissionKey))
                throw new HttpException(403, "Unauthorized");
        }

        public static bool TryParsePositiveInt(string raw, out int value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(raw)) return false;
            return int.TryParse(raw, out value) && value > 0;
        }

        public static string CurrentUserId()
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null || ctx.Session == null || ctx.Session["USERID"] == null)
                return null;
            string userId = ctx.Session["USERID"].ToString();
            return string.IsNullOrWhiteSpace(userId) ? null : userId;
        }

        public static bool VisitBelongsToCurrentCompany(int visitId)
        {
            return VisitExists(visitId, false);
        }

        public static bool UserOwnsVisit(int visitId)
        {
            return VisitExists(visitId, true);
        }

        /// <summary>
        /// Owner of the visit, or holder of srch_dailyrpts for a visit in the
        /// current company. Does not use ReportingManagerId (Decision #8 STOP).
        /// </summary>
        public static bool UserCanViewVisit(int visitId)
        {
            if (UserOwnsVisit(visitId)) return true;
            return UserCanManageCompanyVisits() && VisitBelongsToCurrentCompany(visitId);
        }

        public static bool UserCanEditOwnVisit(int visitId)
        {
            return UserOwnsVisit(visitId);
        }

        public static bool UserCanManageCompanyVisits()
        {
            if (!ResourceContextReady()) return false;
            return HasPermission("srch_dailyrpts");
        }

        public static bool UserCanApproveVisit(int visitId)
        {
            if (!UserCanManageCompanyVisits()) return false;
            return VisitBelongsToCurrentCompany(visitId);
        }

        public static bool UserCanApproveExpense(int expenseId, int visitId)
        {
            if (expenseId <= 0 || visitId <= 0 || !UserCanManageCompanyVisits())
                return false;

            const string sql = @"
                SELECT TOP 1 1
                FROM dbo.tbl_Expenses e
                INNER JOIN dbo.tbl_SalesVisitReport v ON v.Id = e.VisitId
                WHERE e.Id = @ExpenseId
                  AND e.VisitId = @VisitId
                  AND v.CompanyID = @CompanyID";

            return ScalarExists(sql, expenseId, visitId);
        }

        public static bool ClientInCurrentCompany(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId) || !ResourceContextReady())
                return false;
            const string sql = "SELECT TOP 1 1 FROM dbo.tbl_Client WHERE Client_Id = @Id AND CompanyID = @CompanyID";
            return ScalarExistsTextId(sql, clientId.Trim());
        }

        public static bool VendorInCurrentCompany(string vendorId)
        {
            if (string.IsNullOrWhiteSpace(vendorId) || !ResourceContextReady())
                return false;
            const string sql = "SELECT TOP 1 1 FROM dbo.tbl_Vendor WHERE Vendor_Id = @Id AND CompanyID = @CompanyID";
            return ScalarExistsTextId(sql, vendorId.Trim());
        }

        public static bool UserCanApprovePendingLeave(int requestId)
        {
            return PendingCompanyRequest(
                requestId,
                "SELECT TOP 1 1 FROM dbo.tbl_LeaveRequests WHERE RequestID = @Id AND CompanyID = @CompanyID AND RequestStatus = 'Pending'");
        }

        public static bool UserCanApprovePendingRegularization(int requestId)
        {
            return PendingCompanyRequest(
                requestId,
                "SELECT TOP 1 1 FROM dbo.tbl_AttendanceRegularization WHERE RequestID = @Id AND CompanyID = @CompanyID AND RequestStatus = 'Pending'");
        }

        private static bool PendingCompanyRequest(int requestId, string sql)
        {
            if (requestId <= 0 || !ResourceContextReady()) return false;
            if (!HasPermission("AdminApprovalDashboard")) return false;
            return ScalarExistsIntId(sql, requestId);
        }

        private static bool ResourceContextReady()
        {
            HttpContext ctx = HttpContext.Current;
            if (!TryValidateSession(ctx)) return false;
            if (!UserCanAccessCurrentCompany()) return false;
            return CurrentUserId() != null && CompanyContext.CurrentCompanyID > 0;
        }

        private static bool VisitExists(int visitId, bool requireOwner)
        {
            if (visitId <= 0 || !ResourceContextReady()) return false;

            string sql = requireOwner
                ? @"SELECT TOP 1 1 FROM dbo.tbl_SalesVisitReport
                    WHERE Id = @Id AND CompanyID = @CompanyID AND CreatedByCode = @UserId"
                : @"SELECT TOP 1 1 FROM dbo.tbl_SalesVisitReport
                    WHERE Id = @Id AND CompanyID = @CompanyID";

            try
            {
                using (var cn = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = visitId });
                    cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyContext.CurrentCompanyID });
                    if (requireOwner)
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 100) { Value = CurrentUserId() });
                    cn.Open();
                    return cmd.ExecuteScalar() != null;
                }
            }
            catch (SqlException)
            {
                return false;
            }
        }

        private static bool ScalarExists(string sql, int expenseId, int visitId)
        {
            try
            {
                using (var cn = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add(new SqlParameter("@ExpenseId", SqlDbType.Int) { Value = expenseId });
                    cmd.Parameters.Add(new SqlParameter("@VisitId", SqlDbType.Int) { Value = visitId });
                    cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyContext.CurrentCompanyID });
                    cn.Open();
                    return cmd.ExecuteScalar() != null;
                }
            }
            catch (SqlException)
            {
                return false;
            }
        }

        private static bool ScalarExistsTextId(string sql, string id)
        {
            try
            {
                using (var cn = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.NVarChar, 100) { Value = id });
                    cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyContext.CurrentCompanyID });
                    cn.Open();
                    return cmd.ExecuteScalar() != null;
                }
            }
            catch (SqlException)
            {
                return false;
            }
        }

        private static bool ScalarExistsIntId(string sql, int id)
        {
            try
            {
                using (var cn = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
                    cmd.Parameters.Add(new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyContext.CurrentCompanyID });
                    cn.Open();
                    return cmd.ExecuteScalar() != null;
                }
            }
            catch (SqlException)
            {
                return false;
            }
        }

        private static bool EnsureAuthorizedCompanyContext(HttpContext ctx)
        {
            int companyId = CompanyContext.CurrentCompanyID;
            if (companyId <= 0)
            {
                Deny(ctx, 401);
                return false;
            }
            if (!UserCanAccessCompany(companyId))
            {
                if (ctx != null && ctx.Session != null)
                    ctx.Session["CompanyID"] = null;
                Deny(ctx, 403);
                return false;
            }
            return true;
        }

        private static int ResolveUserDbId(HttpContext ctx)
        {
            if (ctx == null || ctx.Session == null || ctx.Session["UserDbId"] == null)
                return 0;

            int userDbId;
            if (!int.TryParse(Convert.ToString(ctx.Session["UserDbId"]), out userDbId) || userDbId <= 0)
                return 0;
            return userDbId;
        }

        private static int GetLoginHomeCompanyId()
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null || ctx.Session == null || ctx.Session["USERID"] == null)
                return 0;

            int userDbId = ResolveUserDbId(ctx);
            if (userDbId <= 0) return 0;

            try
            {
                using (var cn = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand(
                    "SELECT CompanyID FROM dbo.tbl_login WHERE Id = @UserDbId AND User_Id = @UserId", cn))
                {
                    cmd.Parameters.Add(new SqlParameter("@UserDbId", SqlDbType.Int) { Value = userDbId });
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar, 100) { Value = ctx.Session["USERID"].ToString() });
                    cn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                        return 0;
                    int homeId = Convert.ToInt32(result);
                    return homeId > 0 ? homeId : 0;
                }
            }
            catch (SqlException)
            {
                return 0;
            }
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
