using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Server.Domain;

namespace Server.Dao
{
    public class DocumentDao : DataAccessObject
    {
        public int CreateDocument(Document model)
        {
            return (int)Session.Save(model);
        }

        public void UpdateDocument(Document model)
        {
            Session.Update(model);
        }

        public void DeleteDocument(Document model)
        {
            Session.Delete(model);
        }

        public Document ReadDocument(String id)
        {
            return Session.Get<Document>(id);
        }
    }
}
