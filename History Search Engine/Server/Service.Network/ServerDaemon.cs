using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Reference.Model;
using Reference.Protocol;
using Reference.Utility;
using log4net;

namespace Server.Service.Network
{
    public class ServerDaemon
    {
        private ILog logger = LogManager.GetLogger(typeof(ServerDaemon));
        private Properties properties = new Properties();
        private Socket socket;

        /// <summary>
        /// 검색 엔진 서버를 초기화 합니다.
        /// </summary>
        public void Init()
        {
            logger.Info("검색 엔진 서버 초기화");
        }

        /// <summary>
        /// 검색 엔진 서버를 시작합니다.
        /// </summary>
        public void Start()
        {
            properties.Load(AppDomain.CurrentDomain.BaseDirectory + "config.properties");

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.Bind(new IPEndPoint(IPAddress.Any, int.Parse(properties["SERVER_PORT"])));
            socket.Listen(int.Parse(properties["SERVER_BACKLOG"]));

            logger.InfoFormat("검색 엔진 서버 시작, {0}", properties["SERVER_PORT"]);

            while (true)
            {
                ServerProtocolInterpretor serverPI = new ServerProtocolInterpretor();
                serverPI.Socket = socket.Accept();
                serverPI.Properties = properties;
                serverPI.Init();

                logger.InfoFormat("새 사용자 접속, {0}", serverPI.Socket.RemoteEndPoint.ToString());

                Task task = new Task(serverPI.Start);
                task.Start();
            }
        }
    }
}
