using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Web.Script.Serialization;

using Server.Utility;
using Server.Domain;
using Server.Service.Network;
using Server.Dao;

using NHibernate;
using NHibernate.Cfg;

using log4net;

namespace Server
{
    /// <summary>
    /// 서버 프로그램의 엔트리 포인트
    /// </summary>
    public class Program
    {
        private ILog logger = LogManager.GetLogger(typeof(Program));
        private ISessionFactory sessionFactory;
        private SearchEngineServer server;

        /// <summary>
        /// 서버 프로그램을 시작합니다.
        /// </summary>
        public void Start()
        {
            InitHibernate();
            StartNetworkService();
        }

        /// <summary>
        /// 사용자가 접속하고 사용할 수 있는 네트워크 서비스를 시작합니다.
        /// </summary>
        private void StartNetworkService()
        {
            logger.Debug("네트워크 서비스 시작");

            server = new SearchEngineServer();
            server.Session = sessionFactory.OpenSession();
            server.Init();
            server.Start();
        }

        /// <summary>
        /// ORM(Object Relation Mapping) 프레임워크인 하이버네이트를 초기화 합니다.
        /// </summary>
        private void InitHibernate()
        {
            logger.Info("하이버네이트 초기화");

            sessionFactory = new Configuration().Configure().BuildSessionFactory();
        }

        public static void Main(string[] args)
        {
            new Program().Start();
        }
    }
}