using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Domain
{
    public class Keyword
    {
        public virtual int KeywordId { get; set; }
        public virtual int DocumentId { get; set; }
        public virtual String Word { get; set; }
        public virtual int WordCount { get; set; }
    }
}
