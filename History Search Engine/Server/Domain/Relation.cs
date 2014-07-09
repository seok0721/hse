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
        public virtual int WordId { get; set; }
        public virtual int Score { get; set; }

        public override bool Equals(object obj)
        {
            var model = obj as Relation;

            return ((this.FileId == model.FileId) && (this.WordId == model.WordId));
        }

        public override int GetHashCode()
        {
            return (FileId + "|" + WordId).GetHashCode();
        }
    }
}