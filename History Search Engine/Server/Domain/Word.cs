using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Domain
{
    public class Word
    {
        public virtual int ID { get; set; }
        public virtual int DocumentId { get; set; }
        public virtual String Value { get; set; }
        public virtual int Count { get; set; }

        public override bool Equals(object obj)
        {
            var model = obj as Word;

            return ((this.ID == model.ID) && (this.DocumentId == model.ID));
        }

        public override int GetHashCode()
        {
            return (ID + "|" + DocumentId).GetHashCode();
        }
    }
}
