using System.Collections.Generic;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Framework.DataAccess.Repositories.Interfaces
{
    public interface ICustomerPriorityRepository
    {
        IList<DropdownItem> GetList();
    }
}
