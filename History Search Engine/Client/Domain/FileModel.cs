using System;

namespace Client.Domain
{
    public class FileModel
    {
        public virtual int ID { get; set; }
        public virtual String UniqueId { get; set; }
        public virtual String UserId { get; set; }
        public virtual String Path { get; set; }
        public virtual String Name { get; set; }
        public virtual long Size { get; set; }
        public virtual DateTime LastUpdateTime { get; set; }
        public virtual char RemoveYn { get; set; }

        public override string ToString()
        {
            return String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", ID, UniqueId, UserId, Path, Name, Size,
                LastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss"), RemoveYn);
        }

        public static FileModel FromString(String str)
        {
            FileModel model = new FileModel();
            String[] split = str.Split(',');

            model.ID = (split[0] == String.Empty) ? 0 : int.Parse(split[0]);
            model.UniqueId = (split[1] == String.Empty) ? null : split[1];
            model.UserId = (split[2] == String.Empty) ? null : split[2];
            model.Path = (split[3] == String.Empty) ? null : split[3];
            model.Name = (split[4] == String.Empty) ? null : split[4];
            model.Size = (split[5] == String.Empty) ? 0 : long.Parse(split[5]);
            model.LastUpdateTime = (split[6] == String.Empty) ? DateTime.MinValue : DateTime.Parse(split[6]);
            model.RemoveYn = (split[7] == String.Empty) ? '?' : Char.Parse(split[7]);

            return model;
        }
    }
}
