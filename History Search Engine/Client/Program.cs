using Client.Service.Network;
using Client.View;
using Client.Utility;
using log4net;
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Reflection;
using System.Text;
using Client.Domain;

namespace Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //new UserConsoleInterface().Start();;
            Console.Out.WriteLine(new DateTime(0) == new DateTime(0));
            Console.Out.WriteLine(String.Format("{0}", new DateTime(0).ToString("yyyy-MM-dd")));
        }
    }
}