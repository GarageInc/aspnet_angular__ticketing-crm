using System.Collections.Generic;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Services.Interfaces
{
    public interface IMenuService
    {
        IList<RootMenuItem> GetMenuItems(string username, bool isAdmin);
    }
}
