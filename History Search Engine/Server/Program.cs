using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Server.DataSource;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            IDataSource dataSource = new SqlDataSource("server=tcp:211.189.19.81,14133", "hse", "sa", "sa11");
            dataSource.GetConnection();
        }
    }
}
