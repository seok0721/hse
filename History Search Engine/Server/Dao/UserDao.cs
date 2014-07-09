using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Server.Domain;

namespace Server.Dao
{
    public class UserDao : DataAccessObject
    {
        public String CreateUser(User model)
        {
            return (String)Session.Save(model);
        }

        public void UpdateUser(User model)
        {
            Session.Update(model);
        }

        public void DeleteUser(User model)
        {
            Session.Delete(model);
        }

        public User ReadUser(String UserId)
        {
            return Session.Get<User>(UserId);
        }
    }
}
