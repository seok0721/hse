namespace Client.View
{
    partial class SearchUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchUI));
            this.TBSearch = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.TopMatch = new System.Windows.Forms.Label();
            this.FileName = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.TrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.열기ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.종료ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Other1 = new System.Windows.Forms.Label();
            this.Other2 = new System.Windows.Forms.Label();
            this.Other3 = new System.Windows.Forms.Label();
            this.Other4 = new System.Windows.Forms.Label();
            this.OtherPB4 = new System.Windows.Forms.PictureBox();
            this.OtherPB3 = new System.Windows.Forms.PictureBox();
            this.OtherPB2 = new System.Windows.Forms.PictureBox();
            this.OtherPB1 = new System.Windows.Forms.PictureBox();
            this.TopMatchIcon = new System.Windows.Forms.PictureBox();
            this.MainIcon = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.label5 = new System.Windows.Forms.Label();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.label6 = new System.Windows.Forms.Label();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OtherPB4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.OtherPB3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.OtherPB2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.OtherPB1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TopMatchIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MainIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            this.SuspendLayout();
            // 
            // TBSearch
            // 
            this.TBSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.TBSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TBSearch.Font = new System.Drawing.Font("나눔고딕", 18F);
            this.TBSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(16)))), ((int)(((byte)(16)))));
            this.TBSearch.Location = new System.Drawing.Point(60, 12);
            this.TBSearch.Name = "TBSearch";
            this.TBSearch.Size = new System.Drawing.Size(617, 28);
            this.TBSearch.TabIndex = 0;
            this.TBSearch.Text = "검색을 합니다";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightGray;
            this.panel1.Location = new System.Drawing.Point(0, 56);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(720, 1);
            this.panel1.TabIndex = 2;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.LightGray;
            this.panel2.Location = new System.Drawing.Point(270, 56);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1, 581);
            this.panel2.TabIndex = 3;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(230)))), ((int)(((byte)(231)))));
            this.panel3.Controls.Add(this.label1);
            this.panel3.Location = new System.Drawing.Point(0, 56);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(271, 19);
            this.panel3.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("나눔고딕", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.label1.Location = new System.Drawing.Point(17, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 14);
            this.label1.TabIndex = 0;
            this.label1.Text = "Top Match";
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(230)))), ((int)(((byte)(231)))));
            this.panel4.Controls.Add(this.label2);
            this.panel4.Location = new System.Drawing.Point(0, 111);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(271, 19);
            this.panel4.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("나눔고딕", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.label2.Location = new System.Drawing.Point(17, 2);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 14);
            this.label2.TabIndex = 1;
            this.label2.Text = "Other Files";
            // 
            // TopMatch
            // 
            this.TopMatch.Font = new System.Drawing.Font("나눔고딕", 9F, System.Drawing.FontStyle.Bold);
            this.TopMatch.Location = new System.Drawing.Point(36, 87);
            this.TopMatch.Name = "TopMatch";
            this.TopMatch.Size = new System.Drawing.Size(228, 16);
            this.TopMatch.TabIndex = 8;
            this.TopMatch.Text = "WordSample.docx";
            // 
            // FileName
            // 
            this.FileName.Font = new System.Drawing.Font("나눔고딕", 15F, System.Drawing.FontStyle.Bold);
            this.FileName.Location = new System.Drawing.Point(272, 301);
            this.FileName.Name = "FileName";
            this.FileName.Size = new System.Drawing.Size(429, 30);
            this.FileName.TabIndex = 8;
            this.FileName.Text = "WordSample.docx";
            this.FileName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.FileName.Click += new System.EventHandler(this.label4_Click);
            // 
            // TrayIcon
            // 
            this.TrayIcon.ContextMenuStrip = this.contextMenuStrip1;
            this.TrayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("TrayIcon.Icon")));
            this.TrayIcon.Text = "삼성 자료저장소";
            this.TrayIcon.Visible = true;
            this.TrayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TrayIcon_MouseDoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.열기ToolStripMenuItem,
            this.종료ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(99, 48);
            // 
            // 열기ToolStripMenuItem
            // 
            this.열기ToolStripMenuItem.Name = "열기ToolStripMenuItem";
            this.열기ToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.열기ToolStripMenuItem.Text = "열기";
            this.열기ToolStripMenuItem.Click += new System.EventHandler(this.열기ToolStripMenuItem_Click);
            // 
            // 종료ToolStripMenuItem
            // 
            this.종료ToolStripMenuItem.Name = "종료ToolStripMenuItem";
            this.종료ToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.종료ToolStripMenuItem.Text = "종료";
            this.종료ToolStripMenuItem.Click += new System.EventHandler(this.종료ToolStripMenuItem_Click);
            // 
            // Other1
            // 
            this.Other1.Font = new System.Drawing.Font("나눔고딕", 9F, System.Drawing.FontStyle.Bold);
            this.Other1.Location = new System.Drawing.Point(36, 142);
            this.Other1.Name = "Other1";
            this.Other1.Size = new System.Drawing.Size(228, 16);
            this.Other1.TabIndex = 8;
            this.Other1.Text = "WordSample.docx";
            this.Other1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Other1_MouseMove);
            // 
            // Other2
            // 
            this.Other2.Font = new System.Drawing.Font("나눔고딕", 9F, System.Drawing.FontStyle.Bold);
            this.Other2.Location = new System.Drawing.Point(36, 175);
            this.Other2.Name = "Other2";
            this.Other2.Size = new System.Drawing.Size(228, 16);
            this.Other2.TabIndex = 8;
            this.Other2.Text = "WordSample.docx";
            this.Other2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Other2_MouseMove);
            // 
            // Other3
            // 
            this.Other3.Font = new System.Drawing.Font("나눔고딕", 9F, System.Drawing.FontStyle.Bold);
            this.Other3.Location = new System.Drawing.Point(36, 208);
            this.Other3.Name = "Other3";
            this.Other3.Size = new System.Drawing.Size(228, 16);
            this.Other3.TabIndex = 8;
            this.Other3.Text = "WordSample.docx";
            this.Other3.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Other3_MouseMove);
            // 
            // Other4
            // 
            this.Other4.Font = new System.Drawing.Font("나눔고딕", 9F, System.Drawing.FontStyle.Bold);
            this.Other4.Location = new System.Drawing.Point(36, 241);
            this.Other4.Name = "Other4";
            this.Other4.Size = new System.Drawing.Size(228, 16);
            this.Other4.TabIndex = 8;
            this.Other4.Text = "WordSample.docx";
            this.Other4.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Other4_MouseMove);
            // 
            // OtherPB4
            // 
            this.OtherPB4.Image = global::Client.Properties.Resources.docSmallIcon2;
            this.OtherPB4.Location = new System.Drawing.Point(9, 237);
            this.OtherPB4.Name = "OtherPB4";
            this.OtherPB4.Size = new System.Drawing.Size(20, 20);
            this.OtherPB4.TabIndex = 6;
            this.OtherPB4.TabStop = false;
            // 
            // OtherPB3
            // 
            this.OtherPB3.Image = global::Client.Properties.Resources.xlsxSmallIcon;
            this.OtherPB3.Location = new System.Drawing.Point(9, 204);
            this.OtherPB3.Name = "OtherPB3";
            this.OtherPB3.Size = new System.Drawing.Size(20, 20);
            this.OtherPB3.TabIndex = 6;
            this.OtherPB3.TabStop = false;
            // 
            // OtherPB2
            // 
            this.OtherPB2.Image = global::Client.Properties.Resources.docSmallIcon2;
            this.OtherPB2.Location = new System.Drawing.Point(9, 171);
            this.OtherPB2.Name = "OtherPB2";
            this.OtherPB2.Size = new System.Drawing.Size(20, 20);
            this.OtherPB2.TabIndex = 6;
            this.OtherPB2.TabStop = false;
            // 
            // OtherPB1
            // 
            this.OtherPB1.Image = global::Client.Properties.Resources.pptSmallIcon;
            this.OtherPB1.Location = new System.Drawing.Point(9, 138);
            this.OtherPB1.Name = "OtherPB1";
            this.OtherPB1.Size = new System.Drawing.Size(20, 20);
            this.OtherPB1.TabIndex = 6;
            this.OtherPB1.TabStop = false;
            // 
            // TopMatchIcon
            // 
            this.TopMatchIcon.Image = global::Client.Properties.Resources.docSmallIcon2;
            this.TopMatchIcon.Location = new System.Drawing.Point(9, 83);
            this.TopMatchIcon.Name = "TopMatchIcon";
            this.TopMatchIcon.Size = new System.Drawing.Size(20, 20);
            this.TopMatchIcon.TabIndex = 6;
            this.TopMatchIcon.TabStop = false;
            // 
            // MainIcon
            // 
            this.MainIcon.Image = global::Client.Properties.Resources.xlsxIcon;
            this.MainIcon.Location = new System.Drawing.Point(385, 87);
            this.MainIcon.Name = "MainIcon";
            this.MainIcon.Size = new System.Drawing.Size(200, 202);
            this.MainIcon.TabIndex = 5;
            this.MainIcon.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox1.Image = global::Client.Properties.Resources.icon3;
            this.pictureBox1.Location = new System.Drawing.Point(17, 13);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(25, 23);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("나눔고딕", 9F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(36, 424);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(228, 16);
            this.label3.TabIndex = 14;
            this.label3.Text = "WordSample.docx";
            this.label3.Visible = false;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("나눔고딕", 9F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(36, 391);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(228, 16);
            this.label4.TabIndex = 15;
            this.label4.Text = "WordSample.docx";
            this.label4.Visible = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::Client.Properties.Resources.docSmallIcon2;
            this.pictureBox2.Location = new System.Drawing.Point(9, 420);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(20, 20);
            this.pictureBox2.TabIndex = 10;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Visible = false;
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("나눔고딕", 9F, System.Drawing.FontStyle.Bold);
            this.label5.Location = new System.Drawing.Point(36, 358);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(228, 16);
            this.label5.TabIndex = 16;
            this.label5.Text = "WordSample.docx";
            this.label5.Visible = false;
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = global::Client.Properties.Resources.xlsxSmallIcon;
            this.pictureBox3.Location = new System.Drawing.Point(9, 387);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(20, 20);
            this.pictureBox3.TabIndex = 11;
            this.pictureBox3.TabStop = false;
            this.pictureBox3.Visible = false;
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("나눔고딕", 9F, System.Drawing.FontStyle.Bold);
            this.label6.Location = new System.Drawing.Point(36, 325);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(228, 16);
            this.label6.TabIndex = 17;
            this.label6.Text = "WordSample.docx";
            this.label6.Visible = false;
            // 
            // pictureBox4
            // 
            this.pictureBox4.Image = global::Client.Properties.Resources.docSmallIcon2;
            this.pictureBox4.Location = new System.Drawing.Point(9, 354);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(20, 20);
            this.pictureBox4.TabIndex = 12;
            this.pictureBox4.TabStop = false;
            this.pictureBox4.Visible = false;
            // 
            // pictureBox5
            // 
            this.pictureBox5.Image = global::Client.Properties.Resources.pptSmallIcon;
            this.pictureBox5.Location = new System.Drawing.Point(9, 321);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new System.Drawing.Size(20, 20);
            this.pictureBox5.TabIndex = 13;
            this.pictureBox5.TabStop = false;
            this.pictureBox5.Visible = false;
            // 
            // SearchUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.ClientSize = new System.Drawing.Size(704, 462);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.pictureBox4);
            this.Controls.Add(this.pictureBox5);
            this.Controls.Add(this.Other4);
            this.Controls.Add(this.Other3);
            this.Controls.Add(this.OtherPB4);
            this.Controls.Add(this.Other2);
            this.Controls.Add(this.OtherPB3);
            this.Controls.Add(this.Other1);
            this.Controls.Add(this.OtherPB2);
            this.Controls.Add(this.TopMatch);
            this.Controls.Add(this.OtherPB1);
            this.Controls.Add(this.FileName);
            this.Controls.Add(this.TopMatchIcon);
            this.Controls.Add(this.MainIcon);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.TBSearch);
            this.Controls.Add(this.pictureBox1);
            this.Name = "SearchUI";
            this.Opacity = 0.9D;
            this.Text = "3";
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.OtherPB4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.OtherPB3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.OtherPB2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.OtherPB1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TopMatchIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MainIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox TBSearch;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox MainIcon;
        private System.Windows.Forms.PictureBox TopMatchIcon;
        private System.Windows.Forms.Label TopMatch;
        private System.Windows.Forms.Label FileName;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.NotifyIcon TrayIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 열기ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 종료ToolStripMenuItem;
        private System.Windows.Forms.PictureBox OtherPB1;
        private System.Windows.Forms.Label Other1;
        private System.Windows.Forms.PictureBox OtherPB2;
        private System.Windows.Forms.Label Other2;
        private System.Windows.Forms.PictureBox OtherPB3;
        private System.Windows.Forms.Label Other3;
        private System.Windows.Forms.PictureBox OtherPB4;
        private System.Windows.Forms.Label Other4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.PictureBox pictureBox5;
    }
}