using System;

namespace Reference.Model
{
    public class FileIOLog
    {
        public virtual String UserId { get; set; }
        public virtual int FileId { get; set; }
        public virtual int FileIOLogSequence { get; set; }
        public virtual String IOType { get; set; }
        public virtual DateTime IOTime { get; set; }

        public override bool Equals(object obj)
        {
            FileIOLog model = obj as FileIOLog;

            return ((this.UserId == model.UserId)
                && (this.FileId == model.FileId)
                && (this.FileIOLogSequence == model.FileIOLogSequence));
        }

        public override int GetHashCode()
        {
            return String.Format("%s|%d|%d", UserId, FileId, FileIOLogSequence).GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("%s|%d|%d|%c|%s",
                UserId, FileId, FileIOLogSequence, IOType, IOTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}
