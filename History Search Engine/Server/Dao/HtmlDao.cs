using NHibernate;
using Reference.Model;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Server.Dao
{
    public class HtmlDao : AbstractDao
    {
        private StringBuilder builder = new StringBuilder();

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

        public ArrayList ReadHtmlList(String userId, String[] keywordArray)
        {
            builder.Clear();
            builder
                .AppendFormat(" SELECT HTML_URL")
                .AppendFormat("   FROM")
                .AppendFormat("   (");

            for (int i = 0; i < keywordArray.Length; i++)
            {
                if (i > 0)
                {
                    builder.AppendFormat(" UNION");
                }

                builder
                    .AppendFormat("     SELECT A.HTML_URL")
                    .AppendFormat("          , DENSE_RANK() OVER(ORDER BY B.HTML_WD_CNT) HTML_SCORE")
                    .AppendFormat("       FROM TBL_HTML A")
                    .AppendFormat("      INNER JOIN TBL_HTML_WORD B")
                    .AppendFormat("         ON B.USR_ID  = A.USR_ID")
                    .AppendFormat("        AND B.HTML_ID = A.HTML_ID")
                    .AppendFormat("        AND LOWER(B.HTML_WD) LIKE LOWER('{0}') + '%'", keywordArray[i].Replace("\'", ""))
                    .AppendFormat("      WHERE A.USR_ID = :userId");
            }

            builder
                .AppendFormat("   ) A")
                .AppendFormat("  GROUP BY HTML_URL")
                .AppendFormat("  ORDER BY MAX(HTML_SCORE) DESC");

            ISQLQuery query = Session.CreateSQLQuery(builder.ToString());
            query.SetParameter("userId", userId);

            return (ArrayList)query.List();
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
