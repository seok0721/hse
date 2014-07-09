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
    public class FileIOStatDao : DataAccessObject
    {
        public FileIOStat CreateFileIOStat(FileIOStat model)
        {
            return (FileIOStat)Session.Save(model);
        }

        public void UpdateFileIOStat(FileIOStat model)
        {
            Session.Update(model);
        }

        public void DeleteFileIOStat(FileIOStat model)
        {
            Session.Delete(model);
        }

        public FileIOStat ReadFileIOStat(int id, int sequence)
        {
            return Session.QueryOver<FileIOStat>()
                .Where(m => ((m.ID == id) && (m.Sequence == sequence)))
                .SingleOrDefault<FileIOStat>();
        }

        public IList<FileIOStat> ReadFileIOStatList(int id)
        {
            return Session.QueryOver<FileIOStat>()
                .Where(m => (m.ID == id))
                .OrderBy(m => m.ID).Asc
                .ThenBy(m => m.Sequence).Desc
                .List<FileIOStat>();
        }

        public int ReadMaxSequence(int id)
        {
            IQuery query = Session.CreateSQLQuery("SELECT ISNULL(MAX(IO_STAT_SEQ), 0) FROM TBL_FILE_IO WHERE FILE_ID = :FILE_ID");
            query.SetParameter("FILE_ID", id);

            return (int)query.UniqueResult();
        }
    }
}