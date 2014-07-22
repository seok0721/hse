namespace Client.View
{
    partial class LoginView
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
            this.LabelUserId = new System.Windows.Forms.Label();
            this.LabelPassword = new System.Windows.Forms.Label();
            this.TextBoxUserId = new System.Windows.Forms.TextBox();
            this.TextBoxPassword = new System.Windows.Forms.TextBox();
            this.ButtonLogin = new System.Windows.Forms.Button();
            this.LabelStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LabelUserId
            // 
            this.LabelUserId.AutoSize = true;
            this.LabelUserId.Font = new System.Drawing.Font("나눔고딕", 8.999999F);
            this.LabelUserId.Location = new System.Drawing.Point(21, 15);
            this.LabelUserId.Name = "LabelUserId";
            this.LabelUserId.Size = new System.Drawing.Size(40, 14);
            this.LabelUserId.TabIndex = 0;
            this.LabelUserId.Text = "아이디";
            // 
            // LabelPassword
            // 
            this.LabelPassword.AutoSize = true;
            this.LabelPassword.Font = new System.Drawing.Font("나눔고딕", 9F);
            this.LabelPassword.Location = new System.Drawing.Point(21, 42);
            this.LabelPassword.Name = "LabelPassword";
            this.LabelPassword.Size = new System.Drawing.Size(51, 14);
            this.LabelPassword.TabIndex = 1;
            this.LabelPassword.Text = "비밀번호";
            // 
            // TextBoxUserId
            // 
            this.TextBoxUserId.Location = new System.Drawing.Point(81, 12);
            this.TextBoxUserId.Name = "TextBoxUserId";
            this.TextBoxUserId.Size = new System.Drawing.Size(127, 21);
            this.TextBoxUserId.TabIndex = 2;
            // 
            // TextBoxPassword
            // 
            this.TextBoxPassword.Location = new System.Drawing.Point(81, 39);
            this.TextBoxPassword.Name = "TextBoxPassword";
            this.TextBoxPassword.Size = new System.Drawing.Size(127, 21);
            this.TextBoxPassword.TabIndex = 3;
            this.TextBoxPassword.UseSystemPasswordChar = true;
            // 
            // ButtonLogin
            // 
            this.ButtonLogin.Font = new System.Drawing.Font("나눔고딕", 9F);
            this.ButtonLogin.Location = new System.Drawing.Point(214, 12);
            this.ButtonLogin.Name = "ButtonLogin";
            this.ButtonLogin.Size = new System.Drawing.Size(49, 50);
            this.ButtonLogin.TabIndex = 4;
            this.ButtonLogin.Text = "로그인";
            this.ButtonLogin.UseVisualStyleBackColor = true;
            this.ButtonLogin.Click += new System.EventHandler(this.ButtonLogin_Click);
            // 
            // LabelStatus
            // 
            this.LabelStatus.AutoSize = true;
            this.LabelStatus.Font = new System.Drawing.Font("나눔고딕", 7F);
            this.LabelStatus.ForeColor = System.Drawing.Color.Red;
            this.LabelStatus.Location = new System.Drawing.Point(21, 73);
            this.LabelStatus.Name = "LabelStatus";
            this.LabelStatus.Size = new System.Drawing.Size(95, 11);
            this.LabelStatus.TabIndex = 5;
            this.LabelStatus.Text = "서버를 찾는중 입니다.";
            // 
            // LoginView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(287, 92);
            this.Controls.Add(this.LabelStatus);
            this.Controls.Add(this.ButtonLogin);
            this.Controls.Add(this.TextBoxPassword);
            this.Controls.Add(this.TextBoxUserId);
            this.Controls.Add(this.LabelPassword);
            this.Controls.Add(this.LabelUserId);
            this.Font = new System.Drawing.Font("궁서체", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Name = "LoginView";
            this.Text = "진지한 로그인";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LabelUserId;
        private System.Windows.Forms.Label LabelPassword;
        private System.Windows.Forms.TextBox TextBoxUserId;
        private System.Windows.Forms.TextBox TextBoxPassword;
        private System.Windows.Forms.Button ButtonLogin;
        private System.Windows.Forms.Label LabelStatus;
    }
}