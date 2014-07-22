using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Service.Network;
using log4net;
using Client.Utility;
using Client.Service.Http;
using Client.Service.File;
using System.IO;

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

            OnChangedHandler onChangeHandler = new OnChangedHandler(Program.OnChanged);
            OnRenamedHandler onRenamedHandler = new OnRenamedHandler(Program.OnRenamed);

            IOTracker track = new IOTracker("C:\\", onChangeHandler, onRenamedHandler);

            track.AddFileType("txt");
            track.AddFileType("ppt");
            try
            {
                track.StartWatch();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.ToString());
            }

            //StartNetworkingService();
        }

        /// <summary>
        /// Event handler for handling HTTP packet capture event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void HttpPacketArriveEvent(object sender, HttpPacketArriveEvnetArgs e)
        {
            HttpPacket packet = e.Packet;

            foreach (KeyValuePair<string, string> pair in packet.Header)
            {
                // Use the packet which contain HTML code.
                if (pair.Key == "Content-Type" && pair.Value == "text/html")
                {
                    // Parsing the content of packet.
                    List<string> texts = parser.parse(packet.Content);

                    if (texts != null)
                    {
                        for (int i = 0; i < texts.Count; i++)
                        {
                            //Console.WriteLine(texts[i]);
                        }
                    }
                }
            }
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            //Console.Out.WriteLine("File Changed : " + e.Name);
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            //Console.Out.WriteLine("File Name Changed : " + e.OldName + "to " + e.Name);
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
