using NHibernate;
using Reference.Model;
using System.Collections.Generic;

namespace Server.Dao
{
    public class FileIOLogDao : AbstractDao
    {
        public FileIOLog CreateFileIOLog(FileIOLog model)
        {
            FileIOLog rtn;

            Session.Clear();
            rtn = Session.Save(model) as FileIOLog;
            Session.Flush();

            return rtn;
        }

        public void UpdateFileIOLog(FileIOLog model)
        {
            Session.Clear();
            Session.Update(model);
            Session.Flush();
        }

        public void DeleteFileIOLog(FileIOLog model)
        {
            Session.Clear();
            Session.Delete(model);
            Session.Flush();
        }

        public FileIOLog ReadFileIOLog(FileIOLog model)
        {
            return Session.QueryOver<FileIOLog>()
               .Where(m
                   => (m.UserId == model.UserId)
                   && (m.FileId == model.FileId)
                   && (m.FileIOLogSequence == model.FileIOLogSequence))
               .SingleOrDefault<FileIOLog>();
        }

        public IList<FileIOLog> ReadFileIOLogList(FileModel model)
        {
            ISQLQuery query = Session.CreateSQLQuery(
                " SELECT *" +
                "   FROM TBL_FILE_IO_LOG" +
                "  WHERE USR_ID  = :userId" +
                "    AND FILE_ID = :fileId" +
                "  ORDER BY USR_ID, FILE_ID");
            query.SetParameter("userId", model.UserId);
            query.SetParameter("fileId", model.FileId);
            query.AddEntity(typeof(FileIOLog));

            return query.List<FileIOLog>();
        }

        public int ReadMaxFileIOLogSequence(FileModel model)
        {
            ISQLQuery query = Session.CreateSQLQuery(
                " SELECT ISNULL(MAX(FILE_IO_LOG_SEQ), 0)" +
                "   FROM TBL_FILE_IO_LOG" +
                "  WHERE USR_ID  = :userId" +
                "    AND FILE_ID = :fileId");
            query.SetParameter("userId", model.UserId);
            query.SetParameter("fileId", model.FileId);

            System.Console.Out.WriteLine(query.UniqueResult());

            return (int)query.UniqueResult();
        }
    }
}
