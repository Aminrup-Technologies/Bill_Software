using System;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;

namespace Bill_Software.corporate.business.app
{
    public enum QuickActionTokenFailure
    {
        None = 0,
        MissingToken,
        Tampered,
        Expired,
        AlreadyUsed,
        Unavailable,
        InvalidPayload
    }

    /// <summary>
    /// Issues and consumes AES QuickAction URL tokens with UTC TTL and one-time use
    /// via dbo.QuickActionTokens (SHA-256 hash). Missing table fails closed (SQL 208).
    /// </summary>
    public static class QuickActionTokenService
    {
        /// <summary>Default 7 days — managers often act on leave mail after a delay.</summary>
        public static int TokenLifetimeMinutes
        {
            get { return AppSecrets.GetInt("QuickActionTokenMinutes", 10080); }
        }

        public static bool TryIssue(
            string requestId,
            string requestType,
            string action,
            string managerId,
            int companyId,
            out string urlToken,
            out string error)
        {
            urlToken = null;
            error = null;

            if (string.IsNullOrWhiteSpace(requestId)
                || string.IsNullOrWhiteSpace(requestType)
                || string.IsNullOrWhiteSpace(action)
                || string.IsNullOrWhiteSpace(managerId)
                || companyId <= 0)
            {
                error = "Unable to create a secure action link.";
                return false;
            }

            if (requestType != "Leave" && requestType != "Reg")
            {
                error = "Unable to create a secure action link.";
                return false;
            }

            if (action != "Approve" && action != "Reject")
            {
                error = "Unable to create a secure action link.";
                return false;
            }

            DateTime expiresUtc = DateTime.UtcNow.AddMinutes(TokenLifetimeMinutes);
            string payload =
                "ReqID=" + Uri.EscapeDataString(requestId.Trim())
                + "&Type=" + Uri.EscapeDataString(requestType)
                + "&Action=" + Uri.EscapeDataString(action)
                + "&ManagerID=" + Uri.EscapeDataString(managerId.Trim())
                + "&CompanyID=" + companyId.ToString(CultureInfo.InvariantCulture)
                + "&Exp=" + expiresUtc.Ticks.ToString(CultureInfo.InvariantCulture);

            string token = SecurityHelper.EncryptToUrlToken(payload);
            if (string.IsNullOrEmpty(token))
            {
                error = "Unable to create a secure action link.";
                return false;
            }

            string tokenHash = PasswordHasher.Sha256Hex(token);

            try
            {
                using (var cn = new SqlConnection(AppSecrets.DbConnectionString))
                {
                    cn.Open();
                    using (var tran = cn.BeginTransaction())
                    {
                        using (var cmdInv = new SqlCommand(
                            @"UPDATE dbo.QuickActionTokens
                              SET UsedAtUtc = SYSUTCDATETIME()
                              WHERE CompanyID = @CompanyID
                                AND RequestType = @RequestType
                                AND RequestID = @RequestID
                                AND Action = @Action
                                AND UsedAtUtc IS NULL", cn, tran))
                        {
                            cmdInv.Parameters.Add("@CompanyID", SqlDbType.Int).Value = companyId;
                            cmdInv.Parameters.Add("@RequestType", SqlDbType.VarChar, 10).Value = requestType;
                            cmdInv.Parameters.Add("@RequestID", SqlDbType.Int).Value = Convert.ToInt32(requestId.Trim(), CultureInfo.InvariantCulture);
                            cmdInv.Parameters.Add("@Action", SqlDbType.VarChar, 10).Value = action;
                            cmdInv.ExecuteNonQuery();
                        }

                        using (var cmdIns = new SqlCommand(
                            @"INSERT INTO dbo.QuickActionTokens
                                (TokenHash, RequestID, RequestType, Action, ManagerID, CompanyID, ExpiresAtUtc)
                              VALUES
                                (@TokenHash, @RequestID, @RequestType, @Action, @ManagerID, @CompanyID, @ExpiresAtUtc)", cn, tran))
                        {
                            cmdIns.Parameters.Add("@TokenHash", SqlDbType.Char, 64).Value = tokenHash;
                            cmdIns.Parameters.Add("@RequestID", SqlDbType.Int).Value = Convert.ToInt32(requestId.Trim(), CultureInfo.InvariantCulture);
                            cmdIns.Parameters.Add("@RequestType", SqlDbType.VarChar, 10).Value = requestType;
                            cmdIns.Parameters.Add("@Action", SqlDbType.VarChar, 10).Value = action;
                            cmdIns.Parameters.Add("@ManagerID", SqlDbType.NVarChar, 50).Value = managerId.Trim();
                            cmdIns.Parameters.Add("@CompanyID", SqlDbType.Int).Value = companyId;
                            cmdIns.Parameters.Add("@ExpiresAtUtc", SqlDbType.DateTime).Value = expiresUtc;
                            cmdIns.ExecuteNonQuery();
                        }

                        tran.Commit();
                    }
                }

                urlToken = token;
                return true;
            }
            catch (FormatException)
            {
                error = "Unable to create a secure action link.";
                return false;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 208)
                {
                    error = "Action links are temporarily unavailable. Please contact your administrator.";
                    return false;
                }
                throw;
            }
        }

        /// <summary>
        /// Decrypts, enforces Exp TTL, and marks the token used on the caller's transaction.
        /// Rollback restores one-time usability if the approval write fails later.
        /// </summary>
        public static QuickActionTokenFailure TryConsume(
            SqlConnection conn,
            SqlTransaction tran,
            string urlToken,
            out NameValueCollection parameters)
        {
            parameters = null;

            if (conn == null || tran == null)
                return QuickActionTokenFailure.Unavailable;

            if (string.IsNullOrWhiteSpace(urlToken))
                return QuickActionTokenFailure.MissingToken;

            string decrypted = SecurityHelper.DecryptFromUrlToken(urlToken.Trim());
            if (string.IsNullOrEmpty(decrypted))
                return QuickActionTokenFailure.Tampered;

            NameValueCollection parsed = HttpUtility.ParseQueryString(decrypted);
            string reqId = parsed["ReqID"];
            string reqType = parsed["Type"];
            string action = parsed["Action"];
            string managerId = parsed["ManagerID"];
            string companyIdRaw = parsed["CompanyID"];
            string expRaw = parsed["Exp"];

            int companyId;
            int requestId;
            if (string.IsNullOrWhiteSpace(reqId)
                || string.IsNullOrWhiteSpace(reqType)
                || string.IsNullOrWhiteSpace(action)
                || string.IsNullOrWhiteSpace(managerId)
                || !int.TryParse(companyIdRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out companyId)
                || companyId <= 0
                || !int.TryParse(reqId, NumberStyles.Integer, CultureInfo.InvariantCulture, out requestId)
                || requestId <= 0
                || (reqType != "Leave" && reqType != "Reg")
                || (action != "Approve" && action != "Reject"))
            {
                return QuickActionTokenFailure.InvalidPayload;
            }

            if (string.IsNullOrWhiteSpace(expRaw))
                return QuickActionTokenFailure.Expired;

            long expTicks;
            if (!long.TryParse(expRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out expTicks) || expTicks <= 0)
                return QuickActionTokenFailure.InvalidPayload;

            DateTime expiresUtc;
            try
            {
                expiresUtc = new DateTime(expTicks, DateTimeKind.Utc);
            }
            catch (ArgumentOutOfRangeException)
            {
                return QuickActionTokenFailure.InvalidPayload;
            }

            if (DateTime.UtcNow > expiresUtc)
                return QuickActionTokenFailure.Expired;

            string tokenHash = PasswordHasher.Sha256Hex(urlToken.Trim());

            try
            {
                using (var cmdUse = new SqlCommand(
                    @"UPDATE dbo.QuickActionTokens
                      SET UsedAtUtc = SYSUTCDATETIME()
                      WHERE TokenHash = @TokenHash
                        AND UsedAtUtc IS NULL
                        AND ExpiresAtUtc > SYSUTCDATETIME()
                        AND CompanyID = @CompanyID
                        AND RequestID = @RequestID
                        AND RequestType = @RequestType
                        AND Action = @Action", conn, tran))
                {
                    cmdUse.Parameters.Add("@TokenHash", SqlDbType.Char, 64).Value = tokenHash;
                    cmdUse.Parameters.Add("@CompanyID", SqlDbType.Int).Value = companyId;
                    cmdUse.Parameters.Add("@RequestID", SqlDbType.Int).Value = requestId;
                    cmdUse.Parameters.Add("@RequestType", SqlDbType.VarChar, 10).Value = reqType;
                    cmdUse.Parameters.Add("@Action", SqlDbType.VarChar, 10).Value = action;

                    if (cmdUse.ExecuteNonQuery() == 1)
                    {
                        parameters = parsed;
                        return QuickActionTokenFailure.None;
                    }
                }

                using (var cmdLookup = new SqlCommand(
                    @"SELECT TOP 1 UsedAtUtc, ExpiresAtUtc
                      FROM dbo.QuickActionTokens
                      WHERE TokenHash = @TokenHash", conn, tran))
                {
                    cmdLookup.Parameters.Add("@TokenHash", SqlDbType.Char, 64).Value = tokenHash;
                    using (SqlDataReader rdr = cmdLookup.ExecuteReader())
                    {
                        if (!rdr.Read())
                            return QuickActionTokenFailure.Expired;

                        if (rdr["UsedAtUtc"] != DBNull.Value)
                            return QuickActionTokenFailure.AlreadyUsed;

                        DateTime rowExpires = Convert.ToDateTime(rdr["ExpiresAtUtc"]);
                        if (DateTime.UtcNow > rowExpires)
                            return QuickActionTokenFailure.Expired;

                        return QuickActionTokenFailure.AlreadyUsed;
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 208)
                    return QuickActionTokenFailure.Unavailable;
                throw;
            }
        }

        public static void ShowFailure(Action<string, string, string> showMessage, QuickActionTokenFailure failure)
        {
            if (showMessage == null)
                return;

            switch (failure)
            {
                case QuickActionTokenFailure.MissingToken:
                    showMessage("error", "Invalid Link", "No secure token was provided.");
                    break;
                case QuickActionTokenFailure.Tampered:
                    showMessage("error", "Tampered Link", "This link is invalid or corrupted.");
                    break;
                case QuickActionTokenFailure.Expired:
                    showMessage("error", "Link Expired",
                        "This approval link has expired or is from an older email. Ask HR to resend the request, or approve it in Flame-Ex after signing in.");
                    break;
                case QuickActionTokenFailure.AlreadyUsed:
                    showMessage("error", "Link Already Used",
                        "This approval link was already used. If the request is still pending, ask HR to resend a fresh email or use the in-app Approvals screen.");
                    break;
                case QuickActionTokenFailure.Unavailable:
                    showMessage("error", "Temporarily Unavailable",
                        "Email action links are temporarily unavailable. Please sign in to Flame-Ex to approve or reject this request.");
                    break;
                default:
                    showMessage("error", "Invalid Link", "This link could not be validated.");
                    break;
            }
        }
    }
}
