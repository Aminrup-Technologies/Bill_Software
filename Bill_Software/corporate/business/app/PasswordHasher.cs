using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Bill_Software.corporate.business.app
{
    public enum PasswordVerifyResult
    {
        Invalid = 0,
        ValidHash = 1,
        ValidLegacyUpgrade = 2
    }

    public static class PasswordHasher
    {
        public const int Iterations = 100000;
        public const int SaltSize = 16;
        public const int HashSize = 32;

        public static void Create(string password, out byte[] hash, out byte[] salt)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password is required.", "password");

            salt = CryptoRandom.GetBytes(SaltSize);
            using (var derive = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                hash = derive.GetBytes(HashSize);
            }
        }

        public static bool VerifyPbkdf2(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (string.IsNullOrEmpty(password) || !HasUsableHash(storedHash, storedSalt))
                return false;

            try
            {
                using (var derive = new Rfc2898DeriveBytes(password, storedSalt, Iterations))
                {
                    byte[] computed = derive.GetBytes(storedHash.Length);
                    return CryptoRandom.FixedTimeEquals(computed, storedHash);
                }
            }
            catch (CryptographicException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public static bool HasUsableHash(byte[] storedHash, byte[] storedSalt)
        {
            return storedHash != null && storedSalt != null
                && storedHash.Length > 0 && storedSalt.Length > 0;
        }

        /// <summary>
        /// Hash present: PBKDF2 only (never plaintext). Hash missing: one-time
        /// plaintext match is allowed so the caller can upgrade and null Password.
        /// </summary>
        public static PasswordVerifyResult Verify(string candidate, byte[] storedHash, byte[] storedSalt, string leftoverPlaintext)
        {
            if (HasUsableHash(storedHash, storedSalt))
                return VerifyPbkdf2(candidate, storedHash, storedSalt)
                    ? PasswordVerifyResult.ValidHash
                    : PasswordVerifyResult.Invalid;

            if (!string.IsNullOrEmpty(leftoverPlaintext) && CryptoRandom.FixedTimeEquals(leftoverPlaintext, candidate))
                return PasswordVerifyResult.ValidLegacyUpgrade;

            return PasswordVerifyResult.Invalid;
        }

        public static bool TryUpgradeLegacyPassword(int userDbId, string verifiedPlaintext)
        {
            if (userDbId <= 0 || string.IsNullOrEmpty(verifiedPlaintext))
                return false;

            byte[] hash;
            byte[] salt;
            Create(verifiedPlaintext, out hash, out salt);

            using (var cn = new SqlConnection(AppSecrets.DbConnectionString))
            using (var cmd = new SqlCommand(
                @"UPDATE tbl_login
                  SET PasswordHash = @Hash, PasswordSalt = @Salt, Password = NULL
                  WHERE Id = @Id", cn))
            {
                cmd.Parameters.Add("@Hash", SqlDbType.VarBinary, HashSize).Value = hash;
                cmd.Parameters.Add("@Salt", SqlDbType.VarBinary, SaltSize).Value = salt;
                cmd.Parameters.AddWithValue("@Id", userDbId);
                cn.Open();
                return cmd.ExecuteNonQuery() == 1;
            }
        }

        public static string Sha256Hex(string value)
        {
            if (value == null)
                value = string.Empty;

            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
                var sb = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++)
                    sb.Append(hash[i].ToString("X2"));
                return sb.ToString();
            }
        }
    }
}
