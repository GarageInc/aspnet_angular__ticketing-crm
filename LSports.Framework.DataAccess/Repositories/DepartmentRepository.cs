using System;
using System.Collections.Generic;
using System.Linq;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.DataAccess.Repositories.Interfaces;

namespace LSports.Framework.DataAccess.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private const string Admin = "Admin";

        public IList<Department> GetList()
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from department in model.tic_Departments
                         where department.IsActive
                         orderby department.Name
                         select new Department
                         {
                             Id = department.Id,
                             Name = department.Name,
                             CanSeeCustomerDetails = department.CanSeeCustomerDetails
                         }).ToList();

                return q;
            }
        }


        public Department Insert(Department department)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var newRecord = new tic_Departments()
                {
                    Name = department.Name,
                    CreatedBy = Admin,
                    CreationDate = DateTime.Now,
                    IsActive = true
                };

                model.tic_Departments.Add(newRecord);
                model.SaveChanges();
                department.Id = newRecord.Id;

                return department;
            }
        }


        public Department Update(Department department)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToEdit = model.tic_Departments.First(rec => rec.Id == department.Id);
                recordToEdit.UpdatedBy = Admin;
                recordToEdit.LastUpdate = DateTime.Now;
                recordToEdit.Name = department.Name;
                recordToEdit.CanSeeCustomerDetails = department.CanSeeCustomerDetails;

                model.SaveChanges();

                return department;
            }
        }


        public void Delete(int id)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToDelete = model.tic_Departments.First(rec => rec.Id == id);
                recordToDelete.UpdatedBy = Admin;
                recordToDelete.LastUpdate = DateTime.Now;
                recordToDelete.IsActive = false;

                model.SaveChanges();
            }
        }

        public bool IsDepartmentNameUnique(string name, int id)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var record = model.tic_Departments.FirstOrDefault(rec => rec.Name == name && rec.IsActive && rec.Id != id);

                return record == null;
            }
        }

        public Department GetItemById(int departmentId)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from department in model.tic_Departments
                         where department.IsActive
                         where department.Id == departmentId
                         orderby department.Name
                         select new Department
                         {
                             Id = department.Id,
                             Name = department.Name,
                             CanSeeCustomerDetails = department.CanSeeCustomerDetails
                         }).FirstOrDefault();

                return q;
            }
        }


        public IList<Department> GetDepartmentsByUserId(string userId)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from department in model.tic_Departments
                         join userDepartment in model.tic_UserDepartments on department.Id equals userDepartment.DepartmentId
                         where userDepartment.IsActive
                         where department.IsActive
                         where userDepartment.UserId == userId
                         orderby department.Name
                         select new Department
                         {
                             Id = department.Id,
                             Name = department.Name,
                             CanSeeCustomerDetails = department.CanSeeCustomerDetails
                         }).ToList();

                return q;
            }
        }


        public IList<UserListItem> GetDepartmentUsers(int departmentId)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = from userDepartment in model.tic_UserDepartments
                    join user in model.users on userDepartment.UserId equals user.Id
                    where userDepartment.IsActive
                    where userDepartment.DepartmentId == departmentId
                    orderby user.FirstName + " " + user.LastName
                    select new UserListItem
                    {
                        Id = user.Id,
                        Name = user.UserName,
                        LastName = user.LastName,
                        FirstName = user.LastName,
                        IsAdministrator = false
                    };
                return q.ToList();
            }
        }
    }
}
