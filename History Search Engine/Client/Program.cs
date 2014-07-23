using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Client.Service.Network;
using System.IO;

namespace Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Out.WriteLine(Reference.Utility.HashUtils.HashMD5("0000"));
            /*
            UserProtocolInterpretor userPI = new UserProtocolInterpretor();
            userPI.Init();

            if (!userPI.Connect())
            {
                Environment.Exit(1);
            }

            userPI.Login("admin", "0000");
            userPI.StoreFile(new FileInfo("c:\\Users\\이왕석\\test.txt"));
            userPI.Logout();

            userPI.Init();
            if (!userPI.Connect())
            {
                Environment.Exit(1);
            }

            userPI.Login("admin", "0000");

            FileInfo loadFileInfo = new FileInfo("c:\\Users\\이왕석\\test2.txt");

            if (loadFileInfo.Exists)
            {
                loadFileInfo.Delete();
            }

            userPI.RetriveFile(1, "c:\\Users\\이왕석", "test2.txt");
            userPI.Logout();
            */
        }
    }
}
