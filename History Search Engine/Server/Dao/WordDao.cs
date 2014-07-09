using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Server.Domain;

namespace Server.Dao
{
    public class WordDao : DataAccessObject
    {
        public int CreateWord(Word model)
        {
            return (int)Session.Save(model);
        }

        public void UpdateWord(Word model)
        {
            Session.Update(model);
        }

        public void DeleteWord(Word model)
        {
            Session.Delete(model);
        }

        public Word ReadWord(String id)
        {
            return Session.Get<Word>(id);
        }

        public IList<Word> ReadWordListUsingDocument(int id)
        {
            return Session.QueryOver<Word>()
                .Where(m => m.DocumentId == id)
                .List<Word>();
        }
    }
}
