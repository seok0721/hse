using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Cfg;

namespace Server
{
    public class NHibernateLoader
    {
        private static NHibernateLoader instance = new NHibernateLoader();
        private Configuration config = new Configuration();
        private ISessionFactory factory;
        private ISession session;

        public static NHibernateLoader Instance
        {
            get
            {
                return instance;
            }
        }

        public ISession Session
        {
            get
            {
                return session;
            }
        }

        private NHibernateLoader()
        {
            factory = config.Configure().BuildSessionFactory();
            session = factory.OpenSession();
        }
    }
}
