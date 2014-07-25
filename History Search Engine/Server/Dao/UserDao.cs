using Reference.Model;
using System;

namespace Server.Dao
{
    public class UserDao : AbstractDao
    {
        public String CreateUser(UserModel model)
        {
            String rtn;

            Session.Clear();
            rtn = Session.Save(model) as String;
            Session.Flush();

            return rtn;
        }

        public void UpdateUser(UserModel model)
        {
            Session.Clear();
            Session.Update(model);
            Session.Flush();
        }

        public void DeleteUser(UserModel model)
        {
            Session.Clear();
            Session.Delete(model);
            Session.Flush();
        }

        public UserModel ReadUser(String userId)
        {
            return Session.Get<UserModel>(userId);
        }
    }
}
