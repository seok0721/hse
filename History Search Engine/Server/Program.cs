using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Server.Utility;
using Server.Domain;
using NHibernate;
using NHibernate.Cfg;
using System.Security.Cryptography;
using Server.Service.Network;
using log4net;

namespace Server
{
    class Program
    {
        private static ILog logger = LogManager.GetLogger(typeof(Program));
        private static ISessionFactory sessionFactory;

        static void Main(string[] args)
        {
            InitHibernate();
            StartNetworkingService();
        }

        private static void StartNetworkingService()
        {
            logger.Debug("Start networking service.");
            ProtocolInterpretor serverPI = new ProtocolInterpretor();
            serverPI.Session = sessionFactory.OpenSession();
            serverPI.RunServer();
        }

        private static void InitHibernate()
        {
            logger.Debug("Create hibernate session factory.");
            sessionFactory = new Configuration().Configure().BuildSessionFactory();
            
            // ISessionFactory sf = new Configuration().Configure().BuildSessionFactory();
            
            // using (ISession ss = sf.OpenSession())
            // using (ITransaction tx = ss.BeginTransaction())
            {
                /*
                var user = new Dictionary<String, Object>();
                user["userId"] = "seok0721";
                user["userName"] = "이왕석";
                user["password"] = MD5.Create("0000");
                user["email"] = "seok0721@gmail.com";
                */
                /*
                User user = ss.Get<User>("seok0721");
                MD5 hash = MD5.Create();
                byte[] ba = hash.ComputeHash(Encoding.UTF8.GetBytes("0000"));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < ba.Length; i++)
                {
                    sb.Append(ba[i].ToString("x2").ToUpper());
                }
                user.Password = sb.ToString();
                ss.Delete(user);
                Object obj = ss.Save(user);
                Console.Out.WriteLine(obj);
                */
                /*
                FileStatistic fs = new FileStatistic();
                fs.FileId = 1;
                fs.Sequence = 1;
                fs.FirstIODateTime = DateTime.Now;
                fs.LastIODateTime = DateTime.Now;
                fs.IOCount = 0;
                ss.Delete(fs);
                
                //ss.Save("User", user);

                tx.Commit();
                */
            }
        }
    }
}
