using NHibernate;
using Reference.Model;
using System.Collections.Generic;

namespace Server.Dao
{
    public class HtmlWordDao : AbstractDao
    {
        public HtmlWord CreateHtmlWord(HtmlWord model)
        {
            HtmlWord rtn;

            Session.Clear();
            rtn = Session.Save(model) as HtmlWord;
            Session.Flush();

            return rtn;
        }

        public void UpdateHtmlWord(HtmlWord model)
        {
            Session.Clear();
            Session.Update(model);
            Session.Flush();
        }

        public void DeleteHtmlWord(HtmlWord model)
        {
            Session.Clear();
            Session.Delete(model);
            Session.Flush();
        }

        public HtmlWord ReadHtmlWord(HtmlWord model)
        {
            return Session.QueryOver<HtmlWord>()
               .Where(m
                   => (m.UserId == model.UserId)
                   && (m.HtmlId == model.HtmlId)
                   && (m.HtmlWordId == model.HtmlWordId))
               .SingleOrDefault<HtmlWord>();
        }

        public IList<HtmlWord> ReadHtmlWordList(HtmlModel model)
        {
            ISQLQuery query = Session.CreateSQLQuery(
                " SELECT *" +
                "   FROM TBL_HTML_WORD" +
                "  WHERE USR_ID  = :userId" +
                "    AND HTML_ID = :htmlId" +
                "  ORDER BY USR_ID, HTML_ID");
            query.SetParameter("userId", model.UserId);
            query.SetParameter("htmlId", model.HtmlId);
            query.AddEntity(typeof(HtmlWord));

            return query.List<HtmlWord>();
        }

        public int ReadMaxHtmlWordId(HtmlModel model)
        {
            ISQLQuery query = Session.CreateSQLQuery(
                " SELECT ISNULL(MAX(HTML_WD_ID), 0)" +
                "   FROM TBL_HTML_WORD" +
                "  WHERE USR_ID  = :userId" +
                "    AND HTML_ID = :htmlId");
            query.SetParameter("userId", model.UserId);
            query.SetParameter("htmlId", model.HtmlId);

            return (int)query.UniqueResult();
        }
    }
}
