using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Domain
{
    public class User
    {
        public virtual String ID { get; set; }
        public virtual String Name { get; set; }
        public virtual String Password { get; set; }
        public virtual String Email { get; set; }
    }
}