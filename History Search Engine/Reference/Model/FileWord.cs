using System;

namespace Reference.Model
{
    public class FileWord
    {
        public virtual String UserId { get; set; }
        public virtual int FileId { get; set; }
        public virtual int FileWordId { get; set; }
        public virtual String Word { get; set; }
        public virtual int WordCount { get; set; }

        public override bool Equals(object obj)
        {
            FileWord model = obj as FileWord;

            return ((this.UserId == model.UserId)
                && (this.FileId == model.FileId)
                && (this.FileWordId == model.FileWordId));
        }

        public override int GetHashCode()
        {
            return String.Format("%s|%d|%d", UserId, FileId, FileWordId).GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("{0},{1},{2},{3},{4}", UserId, FileId, FileWordId, Word, WordCount);
        }
    }
}
