using NHibernate;
using Reference.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Dao
{
    public class FileDao : AbstractDao
    {
        private StringBuilder builder = new StringBuilder();

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

        public IList<FileModel> ReadFileList(String userId, String[] keywordArray)
        {
            builder.Clear();
            builder
                .AppendFormat(" SELECT USR_ID, FILE_ID, UNIQUE_ID, FILE_PATH,")
                .AppendFormat("        FILE_NM, FILE_SZ, LAST_UPDATE_TM, REMOVE_YN")
                .AppendFormat("     FROM ")
                .AppendFormat("     (")
                .AppendFormat("     SELECT MAX(C.FILE_WD_CNT) ID,")
                .AppendFormat("            B.USR_ID, B.FILE_ID, B.UNIQUE_ID, B.FILE_PATH,")
                .AppendFormat("            B.FILE_NM, B.FILE_SZ, B.LAST_UPDATE_TM, B.REMOVE_YN")
                .AppendFormat("       FROM TBL_USER A")
                .AppendFormat("      INNER JOIN TBL_FILE B")
                .AppendFormat("         ON B.USR_ID  = A.USR_ID")
                .AppendFormat("      INNER JOIN TBL_FILE_WORD C")
                .AppendFormat("         ON C.USR_ID  = B.USR_ID")
                .AppendFormat("        AND C.FILE_ID = B.FILE_ID")
                .AppendFormat("        AND")
                .AppendFormat("          (");

            for (int i = 0; i < keywordArray.Length; i++)
            {
                if (keywordArray[i].Length == 0)
                {
                    continue;
                }

                if (i == 0)
                {
                    builder
                        .AppendFormat("    C.FILE_WD LIKE '%{0}%'", keywordArray[i]);
                }
                else
                {
                    builder
                        .AppendFormat(" OR C.FILE_WD LIKE '%{0}%'", keywordArray[i]);
                }
            }

            builder
                .AppendFormat("          )")
                .AppendFormat("      WHERE A.USR_ID  = '{0}'", userId)
                .AppendFormat("   GROUP BY B.USR_ID,")
                .AppendFormat("            B.FILE_ID,")
                .AppendFormat("            B.UNIQUE_ID,")
                .AppendFormat("            B.FILE_PATH,")
                .AppendFormat("            B.FILE_NM,")
                .AppendFormat("            B.FILE_SZ,")
                .AppendFormat("            B.LAST_UPDATE_TM,")
                .AppendFormat("            B.REMOVE_YN")
                .AppendFormat("     ) A")
                .AppendFormat("  ORDER BY ID DESC");

            ISQLQuery query = Session.CreateSQLQuery(builder.ToString());
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
