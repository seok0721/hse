using System;

namespace Reference.Model
{
    public class UserModel
    {
        public virtual String UserId { get; set; }
        public virtual String Name { get; set; }
        public virtual String Password { get; set; }
        public virtual String Email { get; set; }
    }
}
