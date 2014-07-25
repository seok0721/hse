using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Client.Service.Network;
using Client.Service.Http;
using Client.Service.File;

namespace Client.View
{
    public partial class TrayView : Form
    {
        private UserProtocolInterpretor mClient;
        private static HttpPacketCapture packetCapture = new HttpPacketCapture();

        private static HttpBodyParser parser = new HttpBodyParser();

        public TrayView(UserProtocolInterpretor client)
        {
            mClient = client;
            init();
            InitializeComponent();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ShowMessage(String title, String message)
        {
            TrayIcon.BalloonTipText = message;
            TrayIcon.BalloonTipIcon = ToolTipIcon.Info;
            TrayIcon.BalloonTipTitle = title;
            TrayIcon.ShowBalloonTip(500);
        }

        private void init()
        {

            parser.StartNode = "//body";

            packetCapture.Start(1000);
            packetCapture.OnHttpPacketArrival += HttpPacketArriveEvent;

            OnChangedHandler onChangeHandler = new OnChangedHandler(OnChanged);
            OnRenamedHandler onRenamedHandler = new OnRenamedHandler(OnRenamed);

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
                if (pair.Key == "Content-Type" && pair.Value.Contains("text/html") && packet.Content != null)
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

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            ShowMessage("파일이 변경 되었습니다.", e.Name);
            mClient.StoreFile(new FileInfo(e.FullPath));
            
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            ShowMessage("파일 이름이 변경되었습니다.",
                "파일 " + e.OldName + "가" + e.Name + "로 변경되었습니다.");
        }

    }
}
