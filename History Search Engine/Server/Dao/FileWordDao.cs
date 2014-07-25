using NHibernate;
using Reference.Model;
using System;
using System.IO;
using System.Data;
using System.Text;
using System.Collections;

using System.Collections.Generic;

namespace Server.Dao
{
    public class FileWordDao : AbstractDao
    {
        private StringBuilder builder = new StringBuilder();

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

        public void DeleteFileWordAll(FileWord model)
        {
            builder.Clear();
            builder
                .AppendFormat(" DELETE FROM TBL_FILE_WORD")
                .AppendFormat("  WHERE USR_ID  = '{0}'", model.UserId)
                .AppendFormat("    AND FILE_ID = '{1}'", model.FileId);

            Session.Clear();
            Session.Delete(builder.ToString());
            Session.Flush();
        }

        public FileWord ReadFileWord(FileWord model)
        {
            Session.Clear();

            return Session.QueryOver<FileWord>()
               .Where(m
                   => (m.UserId == model.UserId)
                   && (m.FileId == model.FileId)
                   && (m.FileWordId == model.FileWordId))
               .SingleOrDefault<FileWord>();
        }

        public FileWord ReadFileWordUsingWord(FileWord model)
        {
            Session.Clear();

            ISQLQuery query = Session.CreateSQLQuery(
                   " SELECT *" +
                   "   FROM TBL_FILE_WORD" +
                   "  WHERE USR_ID  = :userId" +
                   "    AND FILE_ID = :fileId" +
                   "    AND FILE_WD = :fileWord");
            query.SetParameter("userId", model.UserId);
            query.SetParameter("fileId", model.FileId);
            query.SetParameter("fileWord", model.Word);
            query.AddEntity(typeof(FileWord));

            return (FileWord)query.UniqueResult();
            /*
            return Session.QueryOver<FileWord>()
               .Where(m
                   => (m.UserId == model.UserId)
                   && (m.FileId == model.FileId)
                   && (m.Word == model.Word))
               .SingleOrDefault<FileWord>();
            */
        }

        public IList<FileWord> ReadFileWordList(FileModel model)
        {
            Session.Clear();

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
            Session.Clear();

            ISQLQuery query = Session.CreateSQLQuery(
                " SELECT ISNULL(MAX(FILE_WD_ID), 0)" +
                "   FROM TBL_FILE_WORD" +
                "  WHERE USR_ID  = :userId" +
                "    AND FILE_ID = :fileId");
            query.SetParameter("userId", model.UserId);
            query.SetParameter("fileId", model.FileId);

            int result = -1;

            try
            {
                result = (int)query.UniqueResult();
                System.Console.Out.WriteLine("max: " + result);

                return result;
            }
            catch (System.Exception ex)
            {
                System.Console.Out.WriteLine(ex.Message);
                System.Console.Out.WriteLine(ex.StackTrace);
                return result;
            }



        }
    }
}
