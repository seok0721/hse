using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Server.Domain
{
    public class FileModel
    {
        public virtual int FileId { get; set; }
        public virtual String UserId { get; set; }
        public virtual String FileName { get; set; }
        public virtual String FilePath { get; set; }
    }
}
