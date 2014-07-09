using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Domain
{
    public class FileIOLog
    {
        public virtual int ID { get; set; }
        public virtual int Sequence { get; set; }
        public virtual char IOType { get; set; }
        public virtual DateTime IOTime { get; set; }

        public override bool Equals(object obj)
        {
            var model = obj as FileIOLog;

            return ((this.ID == model.ID) && (this.Sequence == model.Sequence));
        }

        public override int GetHashCode()
        {
            return (ID + "|" + Sequence).GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("{0},{1},{2},{3}", ID, Sequence, IOType, IOTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        public static FileIOLog FromString(String value)
        {
            FileIOLog model = new FileIOLog();
            String[] split = value.Split(',');

            model.ID = (split[0] == "null") ? 0 : int.Parse(split[0]);
            model.Sequence = (split[1] == "null") ? 0 : int.Parse(split[1]);
            model.IOType = (split[2] == "null") ? '?' : char.Parse(split[2]);
            model.IOTime = (split[3] == "null") ? DateTime.MinValue : DateTime.Parse(split[3]);

            return model;
        }
    }
}
