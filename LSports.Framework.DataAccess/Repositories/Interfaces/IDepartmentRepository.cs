using System;
using System.Collections.Generic;
using System.Linq;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Framework.DataAccess.Repositories.Interfaces
{
    public interface IDepartmentRepository
    {
        IList<Department> GetList();


        Department Insert(Department department);


        Department Update(Department department);


        void Delete(int id);

        bool IsDepartmentNameUnique(string name, int id);

        Department GetItemById(int departmentId);

        IList<Department> GetDepartmentsByUserId(string userId);

        IList<UserListItem> GetDepartmentUsers(int departmentId);
    }
}
