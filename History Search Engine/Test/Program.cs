using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client.View;
using Client.Service.Network;
using Client.Service.Http;
using Client.Service.File;
using Reference.Utility;


namespace Test
{
    static class Program
    {



        [STAThread]
        public static void Main(string[] args)
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            UserProtocolInterpretor upi = new UserProtocolInterpretor();
            upi.Init();

            upi.Connect();

            upi.Login("admin", HashUtils.HashMD5("0000"));
            


            new SearchUI(upi).Show();
            //new TestUI().Show();
            Application.Run();
        }

    }


}
