using NHibernate;
using Reference.Model;
using System;
using System.Collections.Generic;

namespace Server.Dao
{
    public class FileDao : AbstractDao
    {
        public FileModel CreateFile(FileModel model)
        {
            FileModel rtn;

            Session.Clear();
            rtn = Session.Save(model) as FileModel;
            Session.Flush();

            return rtn;
        }

        public void UpdateFile(FileModel model)
        {
            Session.Clear();
            Session.Update(model);
            Session.Flush();
        }

        public void DeleteFile(FileModel model)
        {
            Session.Clear();
            Session.Delete(model);
            Session.Flush();
        }

        public FileModel ReadFile(FileModel model)
        {
            return Session.QueryOver<FileModel>()
               .Where(m
                   => (m.UserId == model.UserId)
                   && (m.FileId == model.FileId))
               .SingleOrDefault<FileModel>();
        }

        public FileModel ReadFileUsingUniqueId(FileModel model)
        {
            return Session.QueryOver<FileModel>()
               .Where(m
                   => (m.UserId == model.UserId
                   && (m.UniqueId == model.UniqueId)))
               .SingleOrDefault<FileModel>();
        }

        public IList<FileModel> ReadFileList(String userId)
        {
            ISQLQuery query = Session.CreateSQLQuery(
                " SELECT *" +
                "   FROM TBL_FILE" +
                "  WHERE USR_ID = :userId" +
                "  ORDER BY USR_ID, FILE_ID");
            query.SetParameter("userId", userId);
            query.AddEntity(typeof(FileModel));

            return query.List<FileModel>();
        }

        public int ReadMaxFileId(String userId)
        {
            ISQLQuery query = Session.CreateSQLQuery(
                " SELECT ISNULL(MAX(FILE_ID), 0)" +
                "   FROM TBL_FILE" +
                "  WHERE USR_ID = :userId");
            query.SetParameter("userId", userId);

            return (int)query.UniqueResult();
        }
    }
}
