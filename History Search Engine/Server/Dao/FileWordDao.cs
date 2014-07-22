using NHibernate;
using Reference.Model;
using System.Collections.Generic;

namespace Server.Dao
{
    public class FileWordDao : AbstractDao
    {
        public FileWord CreateFileWord(FileWord model)
        {
            FileWord rtn;

            Session.Clear();
            rtn = Session.Save(model) as FileWord;
            Session.Flush();

            return rtn;
        }

        public void UpdateFileWord(FileWord model)
        {
            Session.Clear();
            Session.Update(model);
            Session.Flush();
        }

        public void DeleteFileWord(FileWord model)
        {
            Session.Clear();
            Session.Delete(model);
            Session.Flush();
        }

        public FileWord ReadFileWord(FileWord model)
        {
            return Session.QueryOver<FileWord>()
               .Where(m
                   => (m.UserId == model.UserId)
                   && (m.FileId == model.FileId)
                   && (m.FileWordId == model.FileWordId))
               .SingleOrDefault<FileWord>();
        }

        public IList<FileWord> ReadFileWordList(FileModel model)
        {
            ISQLQuery query = Session.CreateSQLQuery(
                " SELECT *" +
                "   FROM TBL_FILE_WORD" +
                "  WHERE USR_ID  = :userId" +
                "    AND FILE_ID = :fileId" +
                "  ORDER BY USR_ID, FILE_ID");
            query.SetParameter("userId", model.UserId);
            query.SetParameter("fileId", model.FileId);
            query.AddEntity(typeof(FileWord));

            return query.List<FileWord>();
        }

        public int ReadMaxFileWordId(FileModel model)
        {
            ISQLQuery query = Session.CreateSQLQuery(
                " SELECT ISNULL(MAX(FILE_WD_ID), 0)" +
                "   FROM TBL_FILE_WORD" +
                "  WHERE USR_ID  = :userId" +
                "    AND FILE_ID = :fileId");
            query.SetParameter("userId", model.UserId);
            query.SetParameter("fileId", model.FileId);

            return (int)query.UniqueResult();
        }
    }
}
