using System;

namespace Reference.Model
{
    public class FileIOStatistic
    {
        public virtual String UserId { get; set; }
        public virtual int FileId { get; set; }
        public virtual int FileIOStatisticSequence { get; set; }
        public virtual int IOCount { get; set; }
        public virtual DateTime BeginFileIOTime { get; set; }
        public virtual DateTime EndFileIOTime { get; set; }

        public override bool Equals(object obj)
        {
            FileIOStatistic model = obj as FileIOStatistic;

            return ((this.UserId == model.UserId)
                && (this.FileId == model.FileId)
                && (this.FileIOStatisticSequence == model.FileIOStatisticSequence));
        }

        public override int GetHashCode()
        {
            return String.Format("%s|%d|%d", UserId, FileId, FileIOStatisticSequence).GetHashCode();
        }
    }
}
