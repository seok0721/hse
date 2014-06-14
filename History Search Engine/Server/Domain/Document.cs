using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Domain
{
    public class Document
    {
        public virtual int DocumentId { get; set; }
        public virtual String URL { get; set; }
        public virtual DateTime CreatedDateTime { get; set; }
    }
}