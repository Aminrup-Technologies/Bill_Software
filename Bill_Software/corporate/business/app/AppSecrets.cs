using System;
using System.Configuration;
using System.Text;

namespace Bill_Software.corporate.business.app
{
    /// <summary>
    /// Resolves secrets and credential settings from configuration.
    /// Does not invent values. Missing required keys fail closed.
    /// </summary>
    public static class AppSecrets
    {
        public static string DbConnectionString
        {
            get
            {
                ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings["DbConn"];
                if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
                    throw new InvalidOperationException("DbConn connection string is not configured.");
                return setting.ConnectionString;
            }
        }

        public static string GetAppSetting(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Configuration key is required.", "key");
            return ConfigurationManager.AppSettings[key];
        }

        public static int GetInt(string key, int defaultValue)
        {
            int parsed;
            string raw = GetAppSetting(key);
            if (string.IsNullOrWhiteSpace(raw) || !int.TryParse(raw, out parsed) || parsed <= 0)
                return defaultValue;
            return parsed;
        }

        /// <summary>
        /// AES-256 key for QuickAction URL tokens. Prefer AppSettings["UrlTokenAesKey"]
        /// (exactly 32 UTF-8 bytes). If omitted, the previously compiled key is used so
        /// existing mailed links keep decrypting. Do not log the key material.
        /// </summary>
        public static byte[] GetUrlTokenAesKey()
        {
            string configured = GetAppSetting("UrlTokenAesKey");
            if (!string.IsNullOrWhiteSpace(configured))
            {
                byte[] key = Encoding.UTF8.GetBytes(configured);
                if (key.Length != 32)
                    throw new InvalidOperationException("UrlTokenAesKey must be exactly 32 UTF-8 bytes.");
                return key;
            }

            // Compiled fallback: identical to the pre-Phase 0B SecurityHelper literal.
            // Operators must copy that same operational value into UrlTokenAesKey on the
            // server before a later change drops this fallback. Do not rotate here.
            return Encoding.UTF8.GetBytes("FlmxSecureKey2026!@#$1234567890X");
        }
    }
}
