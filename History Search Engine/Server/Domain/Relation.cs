using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Domain
{
    public class Relation
    {
        public virtual int FileId { get; set; }
        public virtual int KeywordId { get; set; }
        public virtual int Score { get; set; }
    }
}