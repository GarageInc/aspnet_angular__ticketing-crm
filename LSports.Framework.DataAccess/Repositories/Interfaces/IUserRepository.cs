using System;
using System.Collections.Generic;
using System.Linq;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Framework.DataAccess.Repositories.Interfaces
{
    public interface IUserRepository
    {
        User GetUserById(string userId);

        User GetUserByName(string userName);

        IList<User> GetUsersByRole(string role);
    }
}
