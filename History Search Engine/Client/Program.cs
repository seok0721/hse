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

        private static HttpBodyParser parser = new HttpBodyParser();

        public static void Main(string[] args)
        {
            parser.StartNode = "//body";

            packetCapture.Start(1000);

            packetCapture.OnHttpPacketArrival += HttpPacketArriveEvent;
            //StartNetworkingService();
        }

        static void HttpPacketArriveEvent(object sender, HttpPacketArriveEvnetArgs e)
        {
            HttpPacket packet = e.Packet;

            foreach (KeyValuePair<string, string> pair in packet.Header)
            {
                if (pair.Key == "Content-Type" && pair.Value == "text/html")
                {
                    List<string> texts = parser.parse(packet.Content);

                    if (texts != null)
                    {
                        for (int i = 0; i < texts.Count; i++)
                        {
                            Console.WriteLine(texts[i]);
                        }
                    }
                }
            }
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
