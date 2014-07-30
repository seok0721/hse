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

        private volatile PictureBox  [] otherFileIcons = new PictureBox[4];
        private volatile Label[] otherFiles = new Label[4];

        private IOTracker track = null;

        //GlobalKeyboardHook ghk = new GlobalKeyboardHook();

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
            Height = 55;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            TBSearch.KeyUp += new KeyEventHandler(TBSearch_KeyUp);

            ////TBSearch.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            TopMatch.MouseDown += TopMatch_MouseDown;
            TopMatch.MouseMove += TopMatch_MouseMove;
            TopMatch.DoubleClick += TopMatch_DoubleClick;

            otherFileIcons[0] = OtherPB1;
            otherFileIcons[1] = OtherPB2;
            otherFileIcons[2] = OtherPB3;
            otherFileIcons[3] = OtherPB4;

            otherFiles[0] = Other1;
            otherFiles[1] = Other2;
            otherFiles[2] = Other3;
            otherFiles[3] = Other4;

            for (int i = 0; i < 4; i++)
            {
                otherFileIcons[i].Hide();
                otherFiles[i].Hide();
            }

            //ghk.HookedKeys.Add(Keys.F12);
            //ghk.KeyDown += new KeyEventHandler(Hooking);

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

            track = new IOTracker("C:\\", onChangeHandler, onRenamedHandler);



            track.AddFileType("docx",
                 (s, e) =>
                 {
                     ShowMessage("파일이 변경 되었습니다.", e.Name);
                     if (mClient.StoreFile(new FileInfo(e.FullPath.Replace("~$", ""))))
                     {
                         mClient.StoreFileWord(
                             new FileInfo(e.FullPath.Replace("~$", "")),
                             new MSWordReader(e.FullPath.Replace("~$", "")).Read().Replace("\r\n", " ")
                             .Replace("\r", " ").Replace("\n", " "));

                     }
                 },
                 (s, e) =>
                 {
                     ShowMessage("Rename", e.OldFullPath + "to " + e.FullPath);


                 }
                 );

            track.AddFileType("pptx",
                (s, e) =>
                {
                    ShowMessage("파일이 변경 되었습니다.", e.Name);
                    if (mClient.StoreFile(new FileInfo(e.FullPath.Replace("~$", ""))))
                    {
                        mClient.StoreFileWord(
                            new FileInfo(e.FullPath.Replace("~$", "")),
                            new MSPowerPointReader(e.FullPath.Replace("~$", "")).Read().Replace("\r\n", " ")
                            .Replace("\r", " ").Replace("\n", " "));

                    }
                },
                (s, e) =>
                {
                    ShowMessage("Rename", e.OldFullPath + "to " + e.FullPath);
                }
                );

            //track.AddFileType("xlsx",
            //    (s, e) =>
            //    {
            //        ShowMessage("파일이 변경 되었습니다.", e.Name);
            //        if (mClient.StoreFile(new FileInfo(e.FullPath.Replace("~$", ""))))
            //        {
            //            mClient.StoreFileWord(
            //                new FileInfo(e.FullPath.Replace("~$", "")),
            //                new MSExcelReader(e.FullPath.Replace("~$", "")).Read().Replace("\r\n", " ")
            //                .Replace("\r", " ").Replace("\n", " "));

            //        }
            //    },
            //    (s, e) =>
            //    {
            //        ShowMessage("Rename", e.OldFullPath + "to " + e.FullPath);
            //    }
            //    );
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
            track.StopWatch();
            if (mClient.RetriveFile(fileLists[0].FileId, pathDownload, fileLists[0].Name))
            {
                System.Diagnostics.Process.Start(Path.Combine(pathDownload, fileLists[0].Name));
            }
            track.StartWatch();

        }

        void Other1_DoubleClick(object sender, EventArgs e)
        {
            string pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pathDownload = Path.Combine(pathUser, "Downloads");
            track.StopWatch();
            if (mClient.RetriveFile(fileLists[0].FileId, pathDownload, fileLists[1].Name))
            {
                System.Diagnostics.Process.Start(Path.Combine(pathDownload, fileLists[1].Name));
            }
            track.StartWatch();

        }

        void Other2_DoubleClick(object sender, EventArgs e)
        {
            string pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pathDownload = Path.Combine(pathUser, "Downloads");
            track.StopWatch();
            if (mClient.RetriveFile(fileLists[0].FileId, pathDownload, fileLists[2].Name))
            {
                System.Diagnostics.Process.Start(Path.Combine(pathDownload, fileLists[2].Name));
            }
            track.StartWatch();

        }

        void Other3_DoubleClick(object sender, EventArgs e)
        {
            string pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pathDownload = Path.Combine(pathUser, "Downloads");
            track.StopWatch();
            if (mClient.RetriveFile(fileLists[0].FileId, pathDownload, fileLists[3].Name))
            {
                System.Diagnostics.Process.Start(Path.Combine(pathDownload, fileLists[3].Name));
            }
            track.StartWatch();

        }

        void Other4_DoubleClick(object sender, EventArgs e)
        {
            string pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string pathDownload = Path.Combine(pathUser, "Downloads");
            track.StopWatch();
            if (mClient.RetriveFile(fileLists[0].FileId, pathDownload, fileLists[4].Name))
            {
                System.Diagnostics.Process.Start(Path.Combine(pathDownload, fileLists[4].Name));
            }
            track.StartWatch();

        }

        /// <summary>
        /// 키보드 입력이 0.5초정도 멈추면 검색을 하게 해줌.
        /// </summary>
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
                    // 실제 검색 처리하는 부분
                    mClient.RetriveFileList(TBSearch.Text, out fileLists, out urlLists);
                    if (fileLists != null && fileLists.Count > 0)
                    {
                        int i = 0;
                        TopMatch.Text = fileLists[0].Name;
                        FileName.Text = fileLists[0].Name;
                        if (fileLists[0].Name.EndsWith("pptx"))
                        {
                            MainIcon.Image = Properties.Resources.pptIcon;
                            TopMatchIcon.Image = Properties.Resources.pptSmallIcon;
                            
                        }
                        else if (fileLists[0].Name.EndsWith("docx"))
                        {
                            MainIcon.Image = Properties.Resources.docIcon;
                            TopMatchIcon.Image = Properties.Resources.docSmallIcon2;

                        }
                        else if (fileLists[0].Name.EndsWith("xlsx"))
                        {
                            MainIcon.Image = Properties.Resources.xlsxIcon;
                            TopMatchIcon.Image = Properties.Resources.xlsxSmallIcon;

                        }
                        

                        
                        for (i = 1; i < fileLists.Count && i -1 < 4; i++)
                        {
                            otherFiles[i - 1].Text = fileLists[i].Name;
                            otherFiles[i - 1].Show();
                            if (fileLists[i].Name.EndsWith("pptx"))
                            {
                                otherFileIcons[i-1].Image = Properties.Resources.pptSmallIcon;

                            }
                            else if (fileLists[i].Name.EndsWith("docx"))
                            {
                                otherFileIcons[i-1].Image = Properties.Resources.docSmallIcon2;
                            }
                            else if (fileLists[i].Name.EndsWith("xlsx"))
                            {
                                otherFileIcons[i - 1].Image = Properties.Resources.xlsxSmallIcon;
                            }
                            otherFileIcons[i - 1].Show();
                        }
                        for (; i < 5; i++)
                        {
                            otherFiles[i - 1].Hide();
                            otherFileIcons[i - 1].Hide();
                        }
                        Height = 500;
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


        /// <summary>
        /// 키보드 입력마다 검색되게 하기 위한 이벤트 핸들러.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TBSearch_KeyUp(object sender, KeyEventArgs e)
        {

            // 하지만 키보드 입력중에 검색되면 느려지는것을 고려.
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
                    CreateDragItemTempFile(dragItemTempFileName);
                    FileSystemWatcher tempWatcher = new FileSystemWatcher();
                    tempWatcher.Created += new FileSystemEventHandler((S, E) =>
                    {
                       
                        string currentDirectory = Path.GetDirectoryName(E.FullPath);

                        track.StopWatch();
                        mClient.RetriveFile(fileLists[0].FileId, currentDirectory, fileLists[0].Name);
                        track.StartWatch();
                        File.Delete(string.Format("{0}\\{1}{2}.tmp", currentDirectory, DRAG_SOURCE_PREFIX, TopMatch.Text));
                    });
                    tempWatcher.Path = "C:\\";
                    tempWatcher.Filter = "*.tmp";

                    tempWatcher.IncludeSubdirectories = true;
                    tempWatcher.EnableRaisingEvents = true;

                    //MessageBox.Show(dragItemTempFileName, dragItemTempFileName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    
                    string[] fileList = new string[] { dragItemTempFileName };
                    DataObject fileDragData = new DataObject(DataFormats.FileDrop, fileList);
                    DoDragDrop(fileDragData, DragDropEffects.Move);
                    tempWatcher.EnableRaisingEvents = false;
                    ClearDragData();
                   
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "DragNDrop Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        private void Other1_MouseMove(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.None)
            {
                return;
            }
            if (itemDragStart && objDragItem != null)
            {

                dragItemTempFileName = string.Format("{0}{1}{2}.tmp", Path.GetTempPath(), DRAG_SOURCE_PREFIX, Other1.Text);
                try
                {
                    CreateDragItemTempFile(dragItemTempFileName);
                    FileSystemWatcher tempWatcher = new FileSystemWatcher();
                    tempWatcher.Created += new FileSystemEventHandler((S, E) =>
                    {

                        string currentDirectory = Path.GetDirectoryName(E.FullPath);

                        track.StopWatch();
                        mClient.RetriveFile(fileLists[0].FileId, currentDirectory, fileLists[0].Name);
                        track.StartWatch();
                        File.Delete(string.Format("{0}\\{1}{2}.tmp", currentDirectory, DRAG_SOURCE_PREFIX, Other1.Text));
                    });
                    tempWatcher.Path = "C:\\";
                    tempWatcher.Filter = "*.tmp";

                    tempWatcher.IncludeSubdirectories = true;
                    tempWatcher.EnableRaisingEvents = true;

                    //MessageBox.Show(dragItemTempFileName, dragItemTempFileName, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    string[] fileList = new string[] { dragItemTempFileName };
                    DataObject fileDragData = new DataObject(DataFormats.FileDrop, fileList);
                    DoDragDrop(fileDragData, DragDropEffects.Move);
                    tempWatcher.EnableRaisingEvents = false;
                    ClearDragData();


                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "DragNDrop Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }
        private void Other2_MouseMove(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.None)
            {
                return;
            }
            if (itemDragStart && objDragItem != null)
            {

                dragItemTempFileName = string.Format("{0}{1}{2}.tmp", Path.GetTempPath(), DRAG_SOURCE_PREFIX, Other2.Text);
                try
                {
                    CreateDragItemTempFile(dragItemTempFileName);
                    FileSystemWatcher tempWatcher = new FileSystemWatcher();
                    tempWatcher.Created += new FileSystemEventHandler((S, E) =>
                    {

                        string currentDirectory = Path.GetDirectoryName(E.FullPath);

                        track.StopWatch();
                        mClient.RetriveFile(fileLists[0].FileId, currentDirectory, fileLists[0].Name);
                        track.StartWatch();
                        File.Delete(string.Format("{0}\\{1}{2}.tmp", currentDirectory, DRAG_SOURCE_PREFIX, Other2.Text));
                    });
                    tempWatcher.Path = "C:\\";
                    tempWatcher.Filter = "*.tmp";

                    tempWatcher.IncludeSubdirectories = true;
                    tempWatcher.EnableRaisingEvents = true;

                    //MessageBox.Show(dragItemTempFileName, dragItemTempFileName, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    string[] fileList = new string[] { dragItemTempFileName };
                    DataObject fileDragData = new DataObject(DataFormats.FileDrop, fileList);
                    DoDragDrop(fileDragData, DragDropEffects.Move);
                    tempWatcher.EnableRaisingEvents = false;
                    ClearDragData();


                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "DragNDrop Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }
        private void Other3_MouseMove(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.None)
            {
                return;
            }
            if (itemDragStart && objDragItem != null)
            {

                dragItemTempFileName = string.Format("{0}{1}{2}.tmp", Path.GetTempPath(), DRAG_SOURCE_PREFIX, Other3.Text);
                try
                {
                    CreateDragItemTempFile(dragItemTempFileName);
                    FileSystemWatcher tempWatcher = new FileSystemWatcher();
                    tempWatcher.Created += new FileSystemEventHandler((S, E) =>
                    {

                        string currentDirectory = Path.GetDirectoryName(E.FullPath);

                        track.StopWatch();
                        mClient.RetriveFile(fileLists[0].FileId, currentDirectory, fileLists[0].Name);
                        track.StartWatch();
                        File.Delete(string.Format("{0}\\{1}{2}.tmp", currentDirectory, DRAG_SOURCE_PREFIX, Other3.Text));
                    });
                    tempWatcher.Path = "C:\\";
                    tempWatcher.Filter = "*.tmp";

                    tempWatcher.IncludeSubdirectories = true;
                    tempWatcher.EnableRaisingEvents = true;

                    //MessageBox.Show(dragItemTempFileName, dragItemTempFileName, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    string[] fileList = new string[] { dragItemTempFileName };
                    DataObject fileDragData = new DataObject(DataFormats.FileDrop, fileList);
                    DoDragDrop(fileDragData, DragDropEffects.Move);
                    tempWatcher.EnableRaisingEvents = false;
                    ClearDragData();


                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "DragNDrop Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }
        private void Other4_MouseMove(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.None)
            {
                return;
            }
            if (itemDragStart && objDragItem != null)
            {

                dragItemTempFileName = string.Format("{0}{1}{2}.tmp", Path.GetTempPath(), DRAG_SOURCE_PREFIX, Other4.Text);
                try
                {
                    CreateDragItemTempFile(dragItemTempFileName);
                    FileSystemWatcher tempWatcher = new FileSystemWatcher();
                    tempWatcher.Created += new FileSystemEventHandler((S, E) =>
                    {

                        string currentDirectory = Path.GetDirectoryName(E.FullPath);

                        track.StopWatch();
                        mClient.RetriveFile(fileLists[0].FileId, currentDirectory, fileLists[0].Name);
                        track.StartWatch();
                        File.Delete(string.Format("{0}\\{1}{2}.tmp", currentDirectory, DRAG_SOURCE_PREFIX, Other4.Text));
                    });
                    tempWatcher.Path = "C:\\";
                    tempWatcher.Filter = "*.tmp";

                    tempWatcher.IncludeSubdirectories = true;
                    tempWatcher.EnableRaisingEvents = true;

                    //MessageBox.Show(dragItemTempFileName, dragItemTempFileName, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    string[] fileList = new string[] { dragItemTempFileName };
                    DataObject fileDragData = new DataObject(DataFormats.FileDrop, fileList);
                    DoDragDrop(fileDragData, DragDropEffects.Move);
                    tempWatcher.EnableRaisingEvents = false;
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