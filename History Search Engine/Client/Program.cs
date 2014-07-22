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
using Client.View;
using System.Windows.Forms;
using System.IO;

namespace Client
{
    public class Program
    {
        private static ILog logger = LogManager.GetLogger(typeof(Program));
 
        private static ProtocolInterpretor clientPI;

        [STAThread]
        public static void Main(string[] args)
        {
            clientPI = new ProtocolInterpretor();
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new LoginView(clientPI).Show();
            Application.Run();
        }

       

       
    }
}
