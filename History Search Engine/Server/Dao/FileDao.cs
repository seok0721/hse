using NHibernate;
using NHibernate.Criterion;
using Server.Domain;
using System;
using System.Collections.Generic;

namespace Server.Dao
{
    public class FileDao : DataAccessObject
    {
        public int CreateFile(FileModel model)
        {
            return (int)Session.Save(model);
        }

        public void UpdateFile(FileModel model)
        {
            Session.Update(model);
        }

        public void DeleteFile(FileModel model)
        {
            Session.Delete(model);
        }

        public FileModel ReadFile(int fileId)
        {
            return Session.Get<FileModel>(fileId);
        }

        public FileModel ReadFileUsingUniqueId(String uniqueId)
        {
            ISQLQuery query = Session.CreateSQLQuery("SELECT FILE_ID, UNIQUE_ID, USR_ID, FILE_PATH, FILE_NM, FILE_SIZE, LAST_UPDATE_TIME, REMOVE_YN FROM TBL_FILE WHERE UNIQUE_ID = :UID");
            query.SetParameter("UID", uniqueId);
            query.AddEntity(typeof(FileModel));

            return query.UniqueResult<FileModel>();
        }

        public int ReadMaxFileId()
        {
            IQuery query = Session.CreateSQLQuery("SELECT ISNULL(MAX(FILE_ID), 0) FROM TBL_FILE");

            return (int)query.UniqueResult();
        }

        public IList<FileModel> ReadFileList(String userId)
        {
            return Session.QueryOver<FileModel>()
                .Where(m => (m.UserId == userId))
                .List<FileModel>();
        }

        public IList<FileModel> ReadFileListLikeName(String userId, String fileName)
        {
            IQueryOver<FileModel, FileModel> query = Session.QueryOver<FileModel>();
            query.Where(m => m.UserId == userId);
            query.And(Restrictions.On<FileModel>(m => m.Name).IsLike(fileName, MatchMode.Anywhere));

            return query.List();
        }
    }
}