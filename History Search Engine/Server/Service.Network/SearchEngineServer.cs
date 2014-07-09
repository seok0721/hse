using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Server.Utility;
using NHibernate;
using Server.Domain;
using log4net;
using Server.Dao;

namespace Server.Service.Network
{
    /// <summary>
    /// 사용자의 접속을 승인하고, 파일 관련 연산을 제공하고 파일-HTTP 응답의 관계를 매핑해주는 서버입니다.
    /// </summary>
    public class SearchEngineServer
    {
        private ILog logger = LogManager.GetLogger(typeof(SearchEngineServer));
        private Properties props = new Properties();
        private FileDao fileDao = new FileDao();
        private FileIOLogDao fileIOLogDao = new FileIOLogDao();
        private UserDao userDao = new UserDao();
        private Socket socket;

        public ISession Session { get; set; }

        /// <summary>
        /// 검색 엔진 서버를 초기화 합니다.
        /// </summary>
        public void Init()
        {
            logger.Info("검색 엔진 서버 초기화");

            fileDao.Session = Session;
            userDao.Session = Session;
            fileIOLogDao.Session = Session;
        }

        /// <summary>
        /// 검색 엔진 서버를 시작합니다.
        /// </summary>
        public void Start()
        {
            props.Load(AppDomain.CurrentDomain.BaseDirectory + "config.properties");

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.Bind(new IPEndPoint(IPAddress.Any, int.Parse(props["server.port"])));
            socket.Listen(int.Parse(props["server.backlog"]));

            logger.InfoFormat("검색 엔진 서버 시작, {0}", props["server.port"]);

            while (true)
            {
                ProtocolInterpretor serverPI = new ProtocolInterpretor();
                serverPI.Socket = socket.Accept();
                serverPI.Properties = props;
                serverPI.FileDao = fileDao;
                serverPI.FileIOLogDao = fileIOLogDao;
                serverPI.UserDao = userDao;
                serverPI.Init();
                
                logger.InfoFormat("새 사용자 접속, {0}", serverPI.Socket.RemoteEndPoint.ToString());

                Task task = new Task(serverPI.Start);
                task.Start();
            }
        }
    }
}