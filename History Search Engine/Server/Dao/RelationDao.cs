using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Server.Domain;

namespace Server.Dao
{
    public class RelationDao : DataAccessObject
    {
        public Relation CreateRelation(Relation model)
        {
            return (Relation)Session.Save(model);
        }

        public void UpdateRelation(Relation model)
        {
            Session.Update(model);
        }

        public void DeleteRelation(Relation model)
        {
            Session.Delete(model);
        }

        public Relation ReadRelation(int fileId, int wordId)
        {
            return Session.QueryOver<Relation>()
                .Where(m => ((m.FileId == fileId) && (m.WordId == wordId)))
                .SingleOrDefault();
        }

        public IList<Relation> ReadRelationListUsingFile(int fileId)
        {
            return Session.QueryOver<Relation>()
                .Where(model => model.FileId == fileId)
                .List<Relation>();
        }

        public IList<Relation> ReadRelationListUsingWord(int wordId)
        {
            return Session.QueryOver<Relation>()
                .Where(model => model.WordId == wordId)
                .List<Relation>();
        }
    }
}
