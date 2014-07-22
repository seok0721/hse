using NHibernate;
using Reference.Model;
using System;
using System.Collections.Generic;

namespace Server.Dao
{
    public class HtmlDao : AbstractDao
    {
        public HtmlModel CreateHtml(HtmlModel model)
        {
            HtmlModel rtn;

            Session.Clear();
            rtn = Session.Save(model) as HtmlModel;
            Session.Flush();

            return rtn;
        }

        public void UpdateHtml(HtmlModel model)
        {
            Session.Clear();
            Session.Update(model);
            Session.Flush();
        }

        public void DeleteHtml(HtmlModel model)
        {
            Session.Clear();
            Session.Delete(model);
            Session.Flush();
        }

        public HtmlModel ReadHtml(HtmlModel model)
        {
            return Session.QueryOver<HtmlModel>()
               .Where(m
                   => (m.UserId == model.UserId)
                   && (m.HtmlId == model.HtmlId))
               .SingleOrDefault<HtmlModel>();
        }

        public IList<HtmlModel> ReadHtmlList(String userId)
        {
            ISQLQuery query = Session.CreateSQLQuery(
                " SELECT *" +
                "   FROM TBL_HTML" +
                "  WHERE USR_ID = :userId" +
                "  ORDER BY USR_ID, HTML_ID");
            query.SetParameter("userId", userId);
            query.AddEntity(typeof(HtmlModel));

            return query.List<HtmlModel>();
        }

        public int ReadMaxHtmlId(String userId)
        {
            ISQLQuery query = Session.CreateSQLQuery(
                " SELECT ISNULL(MAX(HTML_ID), 0)" +
                "   FROM TBL_HTML" +
                "  WHERE USR_ID = :userId");
            query.SetParameter("userId", userId);

            return (int)query.UniqueResult();
        }
    }
}
