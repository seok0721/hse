using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NHibernate;

namespace Server.Dao
{
    public abstract class DataAccessObject
    {
        public ISession Session { get; set; }
    }
}