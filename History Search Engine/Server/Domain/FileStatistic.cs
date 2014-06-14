using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Domain
{
    public class FileStatistic
    {
        public virtual int FileId { get; set; }
        public virtual int Sequence { get; set; }
        public virtual DateTime FirstIODateTime { get; set; }
        public virtual DateTime LastIODateTime { get; set; }
        public virtual int IOCount { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var t = obj as FileStatistic;

            if (t == null)
            {
                return false;
            }

            if (FileId != t.FileId || Sequence != t.Sequence)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return (FileId + "|" + Sequence).GetHashCode();
        }
    }
}
