using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Service.Network;
using log4net;
using Client.Utility;
using Client.Http_Parser;

namespace Client
{
    public class Program
    {
        private static ILog logger = LogManager.GetLogger(typeof(Program));
        private static HttpPacketCapture packetCapture = new HttpPacketCapture();

        public static void Main(string[] args)
        {
            packetCapture.Start(1000);
            //StartNetworkingService();
        }

        private static void StartNetworkingService()
        {
            ProtocolInterpretor clientPI;

            try
            {
                clientPI = new ProtocolInterpretor();
                clientPI.RunClient();
                bool isLogin = clientPI.Login("seok0721", HashUtils.HashMD5("0000"));

                logger.Info("Is login? : " + isLogin);
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
        }
    }
}
