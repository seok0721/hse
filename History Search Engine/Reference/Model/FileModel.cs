using System;
using System.IO;
using Reference.Utility;

namespace Reference.Model
{
    public class FileModel
    {
        public virtual String UserId { get; set; }
        public virtual int FileId { get; set; }
        public virtual String UniqueId { get; set; }
        public virtual String Path { get; set; }
        public virtual String Name { get; set; }
        public virtual long Size { get; set; }
        public virtual DateTime LastUpdateTime { get; set; }
        public virtual char RemoveYN { get; set; }

        public static FileModel FromFileInfo(FileInfo fileInfo)
        {
            FileModel model = new FileModel();

            model.UniqueId = FileUtils.GetUniqueIdentifier(fileInfo.FullName);
            model.Path = fileInfo.DirectoryName;
            model.Name = fileInfo.Name;
            model.Size = fileInfo.Length;
            model.LastUpdateTime = FileUtils.GetLastWriteTime(fileInfo.FullName);
            model.RemoveYN = fileInfo.Exists ? 'N' : 'Y';

            return model;
        }

        public static FileModel FromString(String fileModelString)
        {
            FileModel model = new FileModel();
            String[] split = fileModelString.Split('|');

            model.UserId = split[0];
            model.FileId = int.Parse(split[1]);
            model.UniqueId = split[2];
            model.Path = split[3];
            model.Name = split[4];
            model.Size = long.Parse(split[5]);
            model.LastUpdateTime = DateTime.Parse(split[6]);
            model.RemoveYN = char.Parse(split[7]);

            return model;
        }

        public override bool Equals(object obj)
        {
            FileModel model = obj as FileModel;

            return ((this.UserId == model.UserId)
                && (this.FileId == model.FileId));
        }

        public override int GetHashCode()
        {
            return String.Format("{0}|{1}", UserId, FileId).GetHashCode();
        }

        public override string ToString()
        {
            Console.Out.WriteLine(UserId +""+ FileId+""+
                UniqueId+""+ Path+""+ Name+""+ Size+""+ LastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss")+""+ RemoveYN);
            return String.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                UserId, FileId, UniqueId, Path, Name, Size, LastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss"), RemoveYN);
        }
    }
}
