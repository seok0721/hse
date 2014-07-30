using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Reference.Utility;
using Client.Service.Network;

namespace Client.View
{
    public partial class LoginView : Form
    {

        private UserProtocolInterpretor mClient;

        public LoginView(UserProtocolInterpretor client)
        {
            

            mClient = client;
            InitializeComponent();

            mClient.Init();
            if (mClient.Connect()) { 
                ChangeStatus("아이디와 비밀번호를 입력하세요.", true);
            }
            else
            {
                ChangeStatus("왕석이한테 서버 키라고 해요", false);
            }
            
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void ChangeStatus(String message, Boolean good)
        {
            LabelStatus.ForeColor = (good)?Color.Green : Color.Red;
            LabelStatus.Text = message;
        }

        private void ButtonLogin_Click(object sender, EventArgs e)
        {
            string userId = TextBoxUserId.Text;
            string password = TextBoxPassword.Text;

            if (mClient.Login(userId, HashUtils.HashMD5(password)))
            {
                new SearchUI(mClient);
                this.Close();
            }
            else
            {
                ChangeStatus("로그인 실패", false);
            }
        }
    }
}
