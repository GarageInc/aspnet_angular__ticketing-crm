using System.Collections.Generic;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Services.Interfaces
{
    public interface IUserService
    {
        IEnumerable<UserListItem> GetUsersByRoles(IEnumerable<string> roles);
        IEnumerable<UserListItem> GetList();
        Staff Insert(Staff record);
        void Update(Staff record);
        void Delete(string id);
        bool IsEmailUnique(string email, string id);
    }
}
