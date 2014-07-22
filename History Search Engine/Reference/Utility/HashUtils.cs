using System;
using System.Security.Cryptography;
using System.Text;

namespace Reference.Utility
{
    public class HashUtils
    {
        private static StringBuilder builder = new StringBuilder();
        private static MD5 md5 = MD5.Create();

        public static String HashMD5(byte[] buffer)
        {
            builder.Clear();

            foreach (byte b in md5.ComputeHash(buffer))
            {
                builder.Append(b.ToString("x2").ToUpper());
            }

            return builder.ToString();
        }

        public static String HashMD5(String text)
        {
            return HashMD5(Encoding.UTF8.GetBytes(text));
        }

        private HashUtils() { }
    }
}