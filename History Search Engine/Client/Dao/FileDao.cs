using Client.Domain;
using NHibernate;
using System;
using System.Collections.Generic;

namespace Client.Dao
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
            IQuery query = Session.CreateQuery("SELECT * FROM TBL_FILE WHERE UNIQUE_ID = :UID");
            query.SetParameter("UID", uniqueId);

            return query.UniqueResult() as FileModel;
        }

        public int ReadMaxFileId()
        {
            IQuery query = Session.CreateQuery("SELECT ISNULL(MAX(FILE_ID), 0) FROM TBL_FILE");

            return (int)query.UniqueResult();
        }

        public IList<FileModel> ReadFileListFromUser(String userId)
        {
            return Session.QueryOver<FileModel>()
                .Where(m => (m.UserId == userId))
                .List<FileModel>();
        }
    }
}