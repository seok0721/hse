using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Domain
{
    public class FileIOStat
    {
        public virtual int ID { get; set; }
        public virtual int Sequence { get; set; }
        public virtual int IOCount { get; set; }
        public virtual DateTime HeadIOTime { get; set; }
        public virtual DateTime TailIOTime { get; set; }

        public override bool Equals(object obj)
        {
            var model = obj as FileIOStat;

            return ((this.ID == model.ID) && (this.Sequence == model.Sequence));
        }

        public override int GetHashCode()
        {
            return (ID + "|" + Sequence).GetHashCode();
        }
    }
}
