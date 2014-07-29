using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Client.Service.Network;
using System.IO;
using Reference.Model;
using Client.Utility;
using Client.Service.Http;
using Client.Service.File;
namespace Client.View
{
    public partial class SearchUI : Form
    {

        private static HttpPacketCapture packetCapture = new HttpPacketCapture();
        private static HttpBodyParser parser = new HttpBodyParser();

        private const string DRAG_SOURCE_PREFIX = "DragNDrop__Temp__";
        private object objDragItem;
        private FileSystemWatcher tempDirectoryWatcher;
        public Hashtable watchers = null;
        private bool itemDragStart = false;
        string dragItemTempFileName = string.Empty;

        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;
        private UserProtocolInterpretor mClient;

        private volatile int timer = 0;

        private int ObjectHeight = 90;

        private volatile IList<FileModel> fileLists;
        private volatile IList<String> urlLists;

        GlobalKeyboardHook ghk = new GlobalKeyboardHook();

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect, // x-coordinate of upper-left corner
            int nTopRect, // y-coordinate of upper-left corner
            int nRightRect, // x-coordinate of lower-right corner
            int nBottomRect, // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
        );

        public SearchUI(UserProtocolInterpretor client)
        {
            InitializeComponent();
            mClient = client;

            this.FormBorderStyle = FormBorderStyle.None;
            //Height = 55;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            TBSearch.KeyUp += new KeyEventHandler(TBSearch_KeyUp);

            //TBSearch.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            TopMatch.MouseDown += TopMatch_MouseDown;
            TopMatch.MouseMove += TopMatch_MouseMove;
            TopMatch.DoubleClick += TopMatch_DoubleClick;

            ghk.HookedKeys.Add(Keys.F12);
            ghk.KeyDown += new KeyEventHandler(Hooking);

            init();


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

        void TopMatch_DoubleClick(object sender, EventArgs e)
        {
            string pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pathDownload = Path.Combine(pathUser, "Downloads");
            mClient.RetriveFile(fileLists[0].FileId, pathDownload, fileLists[0].Name);

        }


        private void waitForKeyboardStop()
        {

            timer = 5;
            while (--timer >= 0)
            {
                Thread.Sleep(100);
            }




            this.Invoke(new MethodInvoker(delegate()
            {
                if (TBSearch.Text.Length == 0)
                {
                    Height = 55;
                }
                else
                {

                    mClient.RetriveFileList(TBSearch.Text, out fileLists, out urlLists);
                    if (fileLists != null && fileLists.Count > 0)
                    {

                        TopMatch.Text = fileLists[0].Name;
                        FileName.Text = fileLists[0].Name;
                        Height = 600;
                    }
                    else
                    {
                        Height = 55;
                    }
                }

                Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 10, 10));
                timer = 0;
                return;
            }));

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);

            if (message.Msg == WM_NCHITTEST && (int)message.Result == HTCLIENT)
                message.Result = (IntPtr)HTCAPTION;
        }

        private void TBSearch_KeyUp(object sender, KeyEventArgs e)
        {

            if (timer == 0)
            {
                Thread thread = new Thread(waitForKeyboardStop);
                thread.Start();
            }
            else
            {
                timer = 5;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        private void ClearDragData()
        {
            try
            {
                if (File.Exists(dragItemTempFileName))
                    File.Delete(dragItemTempFileName);
                objDragItem = null;
                dragItemTempFileName = string.Empty;
                itemDragStart = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "DragNDrop Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TopMatch_MouseDown(object sender, MouseEventArgs e)
        {           //Cears the Drag Data
            ClearDragData();
            if (e.Button == MouseButtons.Left)
            {
                objDragItem = TopMatch.Text;
                itemDragStart = true;
            }
        }

        private void TopMatch_MouseMove(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.None)
            {
                return;
            }
            if (itemDragStart && objDragItem != null)
            {

                dragItemTempFileName = string.Format("{0}{1}{2}.tmp", Path.GetTempPath(), DRAG_SOURCE_PREFIX, TopMatch.Text);
                try
                {
                    //MessageBox.Show(dragItemTempFileName, dragItemTempFileName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CreateDragItemTempFile(dragItemTempFileName);
                    string[] fileList = new string[] { dragItemTempFileName };
                    DataObject fileDragData = new DataObject(DataFormats.FileDrop, fileList);
                    DoDragDrop(fileDragData, DragDropEffects.Move); MessageBox.Show("", "DragNDrop Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ClearDragData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "DragNDrop Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        private void CreateDragItemTempFile(string fileName)
        {
            FileStream fsDropFile = null;
            try
            {
                fsDropFile = new FileStream(dragItemTempFileName, FileMode.Create);
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message, "DragNDrop error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (fsDropFile != null)
                {
                    fsDropFile.Flush();
                    fsDropFile.Close();
                    fsDropFile.Dispose();
                }
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        void Hooking(object sender, KeyEventArgs e)
        {
            Visible = !Visible;
        }

        private static void HttpPacketArriveEvent(object sender, HttpPacketArriveEvnetArgs e)
        {
            HttpPacket packet = e.Packet;

            // If the http connection use GET method, get HOST name from packet
            if (packet.Protocol.Contains("GET"))
            {
                System.Diagnostics.Debug.WriteLine(packet.Header["Host"]);
            }
            // Or it use HTTP method, get contents of packet
            else if (packet.Protocol.Contains("HTTP"))
            {
                System.Diagnostics.Debug.WriteLine(packet.Header["Content-Type"]);
                if (packet.Header["Content-Type"].Contains("text/html") && packet.Content != null)
                {
                    try
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
                    catch (ArgumentException argExep)
                    {
                        // Write the detail of exception using logger
                        System.Diagnostics.Debug.WriteLine(argExep.Message);
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

        private void TrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
        }

        private void 종료ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void 열기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
        }


    }

}