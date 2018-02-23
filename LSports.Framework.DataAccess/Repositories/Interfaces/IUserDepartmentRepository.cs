using System;
using System.Collections.Generic;
using System.Linq;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Framework.DataAccess.Repositories.Interfaces
{
    public interface IUserDepartmentRepository
    {
        IList<UserDepartment> GetListByUserId(string userId);

        IList<UserDepartment> GetListByDepartmentId(int departmentId);

        IList<UserDepartment> Update(IList<UserDepartment> userDepartments);

        void Update(UserDepartmentCollectionForDepartment data);

        void Update(UserDepartmentCollectionForStaff data);

        void Delete(IList<UserDepartment> userDepartments);

        void Delete(int id);
    }
}
