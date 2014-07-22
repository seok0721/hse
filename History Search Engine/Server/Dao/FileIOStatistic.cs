using NHibernate;
using Reference.Model;
using System.Collections.Generic;

namespace Server.Dao
{
    public class FileIOStatisticDao : AbstractDao
    {
        public FileIOStatistic CreateFileIOStatistic(FileIOStatistic model)
        {
            FileIOStatistic rtn;

            Session.Clear();
            rtn = Session.Save(model) as FileIOStatistic;
            Session.Flush();

            return rtn;
        }

        public void UpdateFileIOStatistic(FileIOStatistic model)
        {
            Session.Clear();
            Session.Update(model);
            Session.Flush();
        }

        public void DeleteFileIOStatistic(FileIOStatistic model)
        {
            Session.Clear();
            Session.Delete(model);
            Session.Flush();
        }

        public FileIOStatistic ReadFileIOStatistic(FileIOStatistic model)
        {
            return Session.QueryOver<FileIOStatistic>()
               .Where(m
                   => (m.UserId == model.UserId)
                   && (m.FileId == model.FileId)
                   && (m.FileIOStatisticSequence == model.FileIOStatisticSequence))
               .SingleOrDefault<FileIOStatistic>();
        }

        public IList<FileIOStatistic> ReadFileIOStatisticList(FileModel model)
        {
            ISQLQuery query = Session.CreateSQLQuery(
                " SELECT *" +
                "   FROM TBL_FILE_IO_STAT" +
                "  WHERE USR_ID  = :userId" +
                "    AND FILE_ID = :fileId" +
                "  ORDER BY USR_ID, FILE_ID");
            query.SetParameter("userId", model.UserId);
            query.SetParameter("fileId", model.FileId);
            query.AddEntity(typeof(FileIOStatistic));

            return query.List<FileIOStatistic>();
        }

        public int ReadMaxFileIOStatisticSequence(FileModel model)
        {
            ISQLQuery query = Session.CreateSQLQuery(
                " SELECT ISNULL(MAX(FILE_IO_STAT_SEQ), 0)" +
                "   FROM TBL_FILE_IO_STAT" +
                "  WHERE USR_ID  = :userId" +
                "    AND FILE_ID = :fileId");
            query.SetParameter("userId", model.UserId);
            query.SetParameter("fileId", model.FileId);

            return (int)query.UniqueResult();
        }
    }
}
