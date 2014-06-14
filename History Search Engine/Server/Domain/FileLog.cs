using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Domain
{
    public class FileLog
    {
        public virtual int FileId { get; set; }
        public virtual String IOType { get; set; }
        public virtual DateTime IODateTime { get; set; }
    }
}
