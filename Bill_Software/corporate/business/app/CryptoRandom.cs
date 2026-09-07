using System;
using System.Security.Cryptography;
using System.Text;

namespace Bill_Software.corporate.business.app
{
    public static class CryptoRandom
    {
        public static byte[] GetBytes(int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException("count");

            byte[] buffer = new byte[count];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(buffer);
            }
            return buffer;
        }

        /// <summary>
        /// Uniform numeric OTP/code using rejection sampling (no modulo bias).
        /// </summary>
        public static string GenerateNumericCode(int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException("length");

            char[] digits = new char[length];
            byte[] buffer = new byte[1];
            using (var rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < length; i++)
                {
                    byte value;
                    do
                    {
                        rng.GetBytes(buffer);
                        value = buffer[0];
                    } while (value > 249);

                    digits[i] = (char)('0' + (value % 10));
                }
            }
            return new string(digits);
        }

        public static string GenerateHexToken(int byteLength)
        {
            byte[] bytes = GetBytes(byteLength);
            var sb = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
                sb.Append(bytes[i].ToString("X2"));
            return sb.ToString();
        }

        public static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }

        public static bool FixedTimeEquals(string a, string b)
        {
            if (a == null || b == null)
                return false;

            byte[] ba = Encoding.UTF8.GetBytes(a);
            byte[] bb = Encoding.UTF8.GetBytes(b);
            if (ba.Length != bb.Length)
            {
                FixedTimeEquals(ba, ba);
                return false;
            }
            return FixedTimeEquals(ba, bb);
        }
    }
}
