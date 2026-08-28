using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Bill_Software.corporate.business.app
{
    public static class SecurityHelper
    {
        // 🚨 In a production environment, store this Key in Web.Config
        // It must be exactly 32 bytes (256 bits)
        private static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("FlmxSecureKey2026!@#$1234567890X");

        public static string EncryptToUrlToken(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = EncryptionKey;
                aes.GenerateIV(); // Create a new Initialization Vector per encryption

                using (MemoryStream ms = new MemoryStream())
                {
                    // Write IV to the beginning of the stream so we can extract it during decryption
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }

                    // Convert to URL-Safe Base64
                    string base64 = Convert.ToBase64String(ms.ToArray());
                    return base64.Replace("+", "-").Replace("/", "_").Replace("=", "");
                }
            }
        }

        public static string DecryptFromUrlToken(string urlSafeToken)
        {
            try
            {
                // Restore standard Base64 characters
                string base64 = urlSafeToken.Replace("-", "+").Replace("_", "/");
                switch (base64.Length % 4)
                {
                    case 2: base64 += "=="; break;
                    case 3: base64 += "="; break;
                }

                byte[] cipherBytes = Convert.FromBase64String(base64);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = EncryptionKey;

                    // Extract IV from the first 16 bytes
                    byte[] iv = new byte[aes.BlockSize / 8];
                    Array.Copy(cipherBytes, 0, iv, 0, iv.Length);
                    aes.IV = iv;

                    using (MemoryStream ms = new MemoryStream(cipherBytes, iv.Length, cipherBytes.Length - iv.Length))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch
            {
                return null; // Decryption failed (tampered token)
            }
        }
    }
}