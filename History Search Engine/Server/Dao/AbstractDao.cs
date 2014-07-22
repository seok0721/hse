using NHibernate;
using NHibernate.Cfg;

namespace Server.Dao
{
    public abstract class AbstractDao
    {
        public ISession Session { get; set; }
    }
}