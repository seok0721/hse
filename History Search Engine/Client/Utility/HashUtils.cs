using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Client.Utility
{
    public class HashUtils
    {
        private static StringBuilder builder = new StringBuilder();
        private static MD5 md5 = MD5.Create();

        public static String HashMD5(String text)
        {
            builder.Clear();

            foreach(byte b in md5.ComputeHash(Encoding.UTF8.GetBytes(text)))
            {
                builder.Append(b.ToString("x2").ToUpper());
            }

            return builder.ToString();
        }

        private HashUtils()
        {

        }
    }
}