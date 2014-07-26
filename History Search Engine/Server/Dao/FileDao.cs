using NHibernate;
using Reference.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

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

        public ArrayList ReadFileList(String userId, String[] keywordArray)
        {
            builder.Clear();
            builder
                .AppendFormat(" SELECT CAST(A.FILE_ID AS VARCHAR) +")
                .AppendFormat("        '|' + A.FILE_NM +")
	            .AppendFormat("        '|' + CAST(A.FILE_SZ AS VARCHAR) +")
                .AppendFormat("        '|' + CONVERT(VARCHAR, A.LAST_UPDATE_TM, 120)")
                .AppendFormat("   FROM")
                .AppendFormat("   (")
                .AppendFormat("     SELECT A.USR_ID")
                .AppendFormat("          , A.FILE_ID")
                .AppendFormat("          , A.FILE_NM")
                .AppendFormat("          , A.FILE_SZ")
                .AppendFormat("          , A.FILE_RANK")
                .AppendFormat("          , DENSE_RANK() OVER(PARTITION BY A.FILE_RANK ORDER BY A.LAST_UPDATE_TM DESC) TIME_RANK")
                .AppendFormat("          , LAST_UPDATE_TM")
                .AppendFormat("          , B.HTML_SCORE *")
                .AppendFormat("            CASE WHEN A.LAST_UPDATE_TM BETWEEN DATEADD(MINUTE,  -5, B.CREATE_TM) AND B.CREATE_TM")
                .AppendFormat("                   OR A.LAST_UPDATE_TM BETWEEN B.CREATE_TM AND DATEADD(MINUTE,  5, B.CREATE_TM)")
                .AppendFormat("                 THEN 1.00")
                .AppendFormat("                 WHEN A.LAST_UPDATE_TM BETWEEN DATEADD(MINUTE, -10, B.CREATE_TM) AND B.CREATE_TM")
                .AppendFormat("                   OR A.LAST_UPDATE_TM BETWEEN B.CREATE_TM AND DATEADD(MINUTE, 10, B.CREATE_TM)")
                .AppendFormat("                 THEN 0.98")
                .AppendFormat("                 WHEN A.LAST_UPDATE_TM BETWEEN DATEADD(MINUTE, -15, B.CREATE_TM) AND B.CREATE_TM")
                .AppendFormat("                   OR A.LAST_UPDATE_TM BETWEEN B.CREATE_TM AND DATEADD(MINUTE, 15, B.CREATE_TM)")
                .AppendFormat("                 THEN 0.90")
                .AppendFormat("                 WHEN A.LAST_UPDATE_TM BETWEEN DATEADD(MINUTE, -20, B.CREATE_TM) AND B.CREATE_TM")
                .AppendFormat("                   OR A.LAST_UPDATE_TM BETWEEN B.CREATE_TM AND DATEADD(MINUTE, 20, B.CREATE_TM)")
                .AppendFormat("                 THEN 0.80")
                .AppendFormat("                 WHEN A.LAST_UPDATE_TM BETWEEN DATEADD(MINUTE, -25, B.CREATE_TM) AND B.CREATE_TM")
                .AppendFormat("                   OR A.LAST_UPDATE_TM BETWEEN B.CREATE_TM AND DATEADD(MINUTE, 25, B.CREATE_TM)")
                .AppendFormat("                 THEN 0.67")
                .AppendFormat("                 WHEN A.LAST_UPDATE_TM BETWEEN DATEADD(MINUTE, -30, B.CREATE_TM) AND B.CREATE_TM")
                .AppendFormat("                   OR A.LAST_UPDATE_TM BETWEEN B.CREATE_TM AND DATEADD(MINUTE, 30, B.CREATE_TM)")
                .AppendFormat("                 THEN 0.54")
                .AppendFormat("                 WHEN A.LAST_UPDATE_TM BETWEEN DATEADD(MINUTE, -35, B.CREATE_TM) AND B.CREATE_TM")
                .AppendFormat("                   OR A.LAST_UPDATE_TM BETWEEN B.CREATE_TM AND DATEADD(MINUTE, 35, B.CREATE_TM)")
                .AppendFormat("                 THEN 0.41")
                .AppendFormat("                 WHEN A.LAST_UPDATE_TM BETWEEN DATEADD(MINUTE, -40, B.CREATE_TM) AND B.CREATE_TM")
                .AppendFormat("                   OR A.LAST_UPDATE_TM BETWEEN B.CREATE_TM AND DATEADD(MINUTE, 40, B.CREATE_TM)")
                .AppendFormat("                 THEN 0.29")
                .AppendFormat("                 WHEN A.LAST_UPDATE_TM BETWEEN DATEADD(MINUTE, -45, B.CREATE_TM) AND B.CREATE_TM")
                .AppendFormat("                   OR A.LAST_UPDATE_TM BETWEEN B.CREATE_TM AND DATEADD(MINUTE, 45, B.CREATE_TM)")
                .AppendFormat("                 THEN 0.20")
                .AppendFormat("                 WHEN A.LAST_UPDATE_TM BETWEEN DATEADD(MINUTE, -50, B.CREATE_TM) AND B.CREATE_TM")
                .AppendFormat("                   OR A.LAST_UPDATE_TM BETWEEN B.CREATE_TM AND DATEADD(MINUTE, 50, B.CREATE_TM)")
                .AppendFormat("                 THEN 0.13")
                .AppendFormat("                 WHEN A.LAST_UPDATE_TM BETWEEN DATEADD(MINUTE, -55, B.CREATE_TM) AND B.CREATE_TM")
                .AppendFormat("                   OR A.LAST_UPDATE_TM BETWEEN B.CREATE_TM AND DATEADD(MINUTE, 55, B.CREATE_TM)")
                .AppendFormat("                 THEN 0.08")
                .AppendFormat("                 WHEN A.LAST_UPDATE_TM BETWEEN DATEADD(MINUTE, -60, B.CREATE_TM) AND B.CREATE_TM")
                .AppendFormat("                   OR A.LAST_UPDATE_TM BETWEEN B.CREATE_TM AND DATEADD(MINUTE, 60, B.CREATE_TM)")
                .AppendFormat("                 THEN 0.05")
                .AppendFormat("                 ELSE 0")
                .AppendFormat("             END GAUSSIAN_SCORE")
                .AppendFormat("       FROM")
                .AppendFormat("       (");

            for (int i = 0; i < keywordArray.Length; i++)
            {
                if (i > 0)
                {
                    builder.AppendFormat(" UNION");
                }

                builder
                    .AppendFormat(" SELECT A.USR_ID")
                    .AppendFormat("      , A.FILE_ID")
                    .AppendFormat("      , A.FILE_NM")
                    .AppendFormat("      , A.FILE_SZ")
                    .AppendFormat("      , B.FILE_WD_CNT")
                    .AppendFormat("      , DENSE_RANK() OVER(ORDER BY B.FILE_WD_CNT DESC) FILE_RANK")
                    .AppendFormat("      , B.FILE_WD")
                    .AppendFormat("      , A.LAST_UPDATE_TM")
                    .AppendFormat("   FROM TBL_FILE A")
                    .AppendFormat("  INNER JOIN TBL_FILE_WORD B")
                    .AppendFormat("     ON B.USR_ID  = A.USR_ID")
                    .AppendFormat("    AND B.FILE_ID = A.FILE_ID")
                    .AppendFormat("    AND LOWER(B.FILE_WD) = LOWER('{0}')", keywordArray[i].Replace("\'", ""))
                    .AppendFormat("  WHERE A.USR_ID = :userId");
            }

            builder
                .AppendFormat("    ) A")
                .AppendFormat("    LEFT OUTER JOIN")
                .AppendFormat("    (");

            for (int i = 0; i < keywordArray.Length; i++)
            {
                if (i > 0)
                {
                    builder.AppendFormat(" UNION");
                }

                builder
                    .AppendFormat(" SELECT DISTINCT")
                    .AppendFormat("        B.USR_ID")
                    .AppendFormat("      , B.HTML_ID")
                    .AppendFormat("      , DENSE_RANK() OVER(ORDER BY B.HTML_WD_CNT) HTML_SCORE")
                    .AppendFormat("      , B.HTML_WD")
                    .AppendFormat("      , A.CREATE_TM")
                    .AppendFormat("   FROM TBL_HTML A")
                    .AppendFormat("  INNER JOIN TBL_HTML_WORD B")
                    .AppendFormat("     ON B.USR_ID  = A.USR_ID")
                    .AppendFormat("    AND B.HTML_ID = A.HTML_ID")
                    .AppendFormat("    AND LOWER(B.HTML_WD) = LOWER('{0}')", keywordArray[i].Replace("\'", ""))
                    .AppendFormat("  WHERE A.USR_ID = :userId");
            }

            builder
                .AppendFormat("       ) B")
                .AppendFormat("         ON B.USR_ID = A.USR_ID")
                .AppendFormat("   ) A")
                .AppendFormat("  GROUP BY A.FILE_ID")
                .AppendFormat("         , A.FILE_NM")
                .AppendFormat("         , A.FILE_SZ")
                .AppendFormat("         , A.LAST_UPDATE_TM")
                .AppendFormat("  ORDER BY MAX(GAUSSIAN_SCORE) DESC,")
                .AppendFormat("           MIN(FILE_RANK),")
                .AppendFormat("           MIN(TIME_RANK)");

            ISQLQuery query = Session.CreateSQLQuery(builder.ToString());
            query.SetParameter("userId", userId);

            return (ArrayList)query.List();
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
