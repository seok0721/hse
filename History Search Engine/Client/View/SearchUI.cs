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

namespace Client.View
{
    public partial class SearchUI : Form
    {

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

            //TBSearch.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            TopMatch.MouseDown += TopMatch_MouseDown;
            TopMatch.MouseMove += TopMatch_MouseMove;
            TopMatch.DoubleClick += TopMatch_DoubleClick;
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
                    if (fileLists!= null && fileLists.Count > 0)
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
            if (e.Button == MouseButtons.Left )
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
                
                dragItemTempFileName = string.Format("{0}{1}{2}.tmp",Path.GetTempPath() , DRAG_SOURCE_PREFIX, TopMatch.Text);
                try {
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
    }
}