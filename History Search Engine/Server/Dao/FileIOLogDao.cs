using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Server.Domain;

using NHibernate;
using NHibernate.Criterion;

namespace Server.Dao
{
    public class FileIOLogDao : DataAccessObject
    {
        public FileIOLog CreateFileIOLog(FileIOLog model)
        {
            return (FileIOLog)Session.Save(model);
        }

        public void UpdateFileIOLog(FileIOLog model)
        {
            Session.Update(model);
        }

        public void DeleteFileIOLog(FileIOLog model)
        {
            Session.Delete(model);
        }

        public FileIOLog ReadFileIOLog(int id, int sequence)
        {
            return Session.QueryOver<FileIOLog>()
                .Where(m => ((m.ID == id) && (m.Sequence == sequence)))
                .SingleOrDefault<FileIOLog>();
        }

        public IList<FileIOLog> ReadFileIOLogList(int id)
        {
            return Session.QueryOver<FileIOLog>()
                .Where(m => (m.ID == id))
                .OrderBy(m => m.ID).Asc
                .ThenBy(m => m.Sequence).Desc
                .List<FileIOLog>();
        }

        public int ReadMaxSequence(int id)
        {
            IQuery query = Session.CreateSQLQuery("SELECT ISNULL(MAX(IO_LOG_SEQ), 0) FROM TBL_FILE_IO_LOG WHERE FILE_ID = :FILE_ID");
            query.SetParameter("FILE_ID", id);

            return (int)query.UniqueResult();
        }
    }
}