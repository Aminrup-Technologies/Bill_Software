using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace Bill_Software.corporate.business.app
{
    /// <summary>
    /// Writes dbo.AuthAudit. Callers pass an open connection; impersonation
    /// Start/Close include the row in the same transaction (fail closed).
    /// </summary>
    public static class SecurityAudit
    {
        public const string ImpersonationIntentIssued = "ImpersonationIntentIssued";
        public const string ImpersonationStart = "ImpersonationStart";
        public const string ImpersonationStartDenied = "ImpersonationStartDenied";
        public const string ImpersonationClosed = "ImpersonationClosed";
        public const string ImpersonationCloseNoOp = "ImpersonationCloseNoOp";

        public static void Write(SqlConnection cn, SqlTransaction tran, int? userId, string eventType, string details)
        {
            if (cn == null)
                throw new ArgumentNullException("cn");
            if (string.IsNullOrWhiteSpace(eventType))
                throw new ArgumentException("EventType is required.", "eventType");

            string trimmedType = eventType.Trim();
            if (trimmedType.Length > 100)
                trimmedType = trimmedType.Substring(0, 100);

            string trimmedDetails = details;
            if (trimmedDetails != null && trimmedDetails.Length > 1000)
                trimmedDetails = trimmedDetails.Substring(0, 1000);

            using (var cmd = new SqlCommand(
                @"INSERT INTO dbo.AuthAudit (UserId, EventType, IPAddress, Details)
                  VALUES (@UserId, @EventType, @IPAddress, @Details)", cn, tran))
            {
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId.HasValue ? (object)userId.Value : DBNull.Value;
                cmd.Parameters.Add("@EventType", SqlDbType.NVarChar, 100).Value = trimmedType;
                cmd.Parameters.Add("@IPAddress", SqlDbType.NVarChar, 50).Value = (object)ClientIp() ?? DBNull.Value;
                cmd.Parameters.Add("@Details", SqlDbType.NVarChar, 1000).Value = (object)trimmedDetails ?? DBNull.Value;
                cmd.ExecuteNonQuery();
            }
        }

        private static string ClientIp()
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null || ctx.Request == null)
                return null;

            string forwarded = ctx.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(forwarded))
            {
                int comma = forwarded.IndexOf(',');
                string first = comma >= 0 ? forwarded.Substring(0, comma) : forwarded;
                first = first.Trim();
                if (first.Length > 50)
                    first = first.Substring(0, 50);
                return first;
            }

            string remote = ctx.Request.ServerVariables["REMOTE_ADDR"];
            if (string.IsNullOrEmpty(remote))
                return null;
            if (remote.Length > 50)
                remote = remote.Substring(0, 50);
            return remote;
        }
    }
}
