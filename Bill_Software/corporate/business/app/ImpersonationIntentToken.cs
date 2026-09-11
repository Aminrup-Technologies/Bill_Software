using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Bill_Software.corporate.business.app
{
    public sealed class ImpersonationIntent
    {
        public Guid ImpersonationId { get; set; }
        public int ActorUserId { get; set; }
        public int TargetUserId { get; set; }
        public int CompanyID { get; set; }
        public Guid ActorSessionToken { get; set; }
        public DateTime ExpiresUtc { get; set; }
    }

    /// <summary>
    /// HMAC-SHA256 intent token. ImpersonationId is chosen here so Start INSERT
    /// uses that PK; replay hits the unique key (INV-16).
    /// </summary>
    public static class ImpersonationIntentToken
    {
        private const string Version = "v1";

        public static string Issue(ImpersonationIntent intent)
        {
            if (intent == null)
                throw new ArgumentNullException("intent");

            string payload = EncodePayload(intent);
            string mac = ToBase64Url(ComputeMac(Encoding.UTF8.GetBytes(payload)));
            return ToBase64Url(Encoding.UTF8.GetBytes(payload)) + "." + mac;
        }

        public static bool TryVerify(string token, out ImpersonationIntent intent)
        {
            intent = null;
            if (string.IsNullOrWhiteSpace(token))
                return false;

            int dot = token.IndexOf('.');
            if (dot <= 0 || dot == token.Length - 1)
                return false;

            byte[] payloadBytes;
            byte[] macBytes;
            if (!TryFromBase64Url(token.Substring(0, dot), out payloadBytes))
                return false;
            if (!TryFromBase64Url(token.Substring(dot + 1), out macBytes))
                return false;

            byte[] expected = ComputeMac(payloadBytes);
            if (!CryptoRandom.FixedTimeEquals(expected, macBytes))
                return false;

            string payload = Encoding.UTF8.GetString(payloadBytes);
            return TryParsePayload(payload, out intent);
        }

        private static string EncodePayload(ImpersonationIntent intent)
        {
            return string.Join("|", new[]
            {
                Version,
                intent.ImpersonationId.ToString("N"),
                intent.ActorUserId.ToString(CultureInfo.InvariantCulture),
                intent.TargetUserId.ToString(CultureInfo.InvariantCulture),
                intent.CompanyID.ToString(CultureInfo.InvariantCulture),
                intent.ActorSessionToken.ToString("N"),
                intent.ExpiresUtc.Ticks.ToString(CultureInfo.InvariantCulture)
            });
        }

        private static bool TryParsePayload(string payload, out ImpersonationIntent intent)
        {
            intent = null;
            string[] parts = payload.Split('|');
            if (parts.Length != 7 || parts[0] != Version)
                return false;

            Guid impersonationId;
            int actorUserId;
            int targetUserId;
            int companyId;
            Guid actorSessionToken;
            long ticks;
            if (!Guid.TryParseExact(parts[1], "N", out impersonationId))
                return false;
            if (!int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out actorUserId) || actorUserId <= 0)
                return false;
            if (!int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out targetUserId) || targetUserId <= 0)
                return false;
            if (!int.TryParse(parts[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out companyId) || companyId <= 0)
                return false;
            if (!Guid.TryParseExact(parts[5], "N", out actorSessionToken))
                return false;
            if (!long.TryParse(parts[6], NumberStyles.Integer, CultureInfo.InvariantCulture, out ticks) || ticks <= 0)
                return false;

            intent = new ImpersonationIntent
            {
                ImpersonationId = impersonationId,
                ActorUserId = actorUserId,
                TargetUserId = targetUserId,
                CompanyID = companyId,
                ActorSessionToken = actorSessionToken,
                ExpiresUtc = new DateTime(ticks, DateTimeKind.Utc)
            };
            return true;
        }

        private static byte[] ComputeMac(byte[] payload)
        {
            using (var hmac = new HMACSHA256(AppSecrets.GetUrlTokenAesKey()))
            {
                return hmac.ComputeHash(payload);
            }
        }

        private static string ToBase64Url(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static bool TryFromBase64Url(string text, out byte[] bytes)
        {
            bytes = null;
            if (string.IsNullOrEmpty(text))
                return false;

            string padded = text.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
                case 1: return false;
            }

            try
            {
                bytes = Convert.FromBase64String(padded);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
