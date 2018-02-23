using System.Collections.Generic;

namespace LSports.Framework.Models.CustomClasses
{
    public class UserDepartmentCollectionForDepartment : Department
    {
        public IList<UserDepartment> ItemsToAdd { get; set; }
        public IList<UserDepartment> ItemsToDelete { get; set; }
        public IList<UserDepartment> ItemsToUpdate { get; set; }
    }
}
