using System.Collections.Generic;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Framework.DataAccess.Repositories
{
    public class CustomerPriorityRepository : ICustomerPriorityRepository
    {
        public IList<DropdownItem> GetList()
        {
            var list = new List<DropdownItem>();
            for(int i=1; i <= 5; i++)
                list.Add(new DropdownItem {Id = i.ToString(), Name = i.ToString()});

            return list;

        }
    }
}
