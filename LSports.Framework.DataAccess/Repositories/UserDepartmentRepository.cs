using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.DataAccess.Repositories.Interfaces;

namespace LSports.Framework.DataAccess.Repositories
{
    public class UserDepartmentRepository : IUserDepartmentRepository
    {
        private const string Admin = "Admin";

        public IList<UserDepartment> GetListByUserId(string userId)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from userDepartment in model.tic_UserDepartments
                         join department in model.tic_Departments on userDepartment.DepartmentId equals department.Id
                         join departmentRole in model.tic_DepartmentRoles on userDepartment.DepartmentRoleId equals departmentRole.Id
                         where userDepartment.IsActive && department.IsActive
                         where userDepartment.UserId == userId
                         select new UserDepartment
                         {
                             Id = userDepartment.Id,
                             Department = new Department
                             {
                                 Id = department.Id,
                                 Name = department.Name,
                                 CanSeeCustomerDetails = department.CanSeeCustomerDetails
                             },
                             DepartmentRole = new DepartmentRole
                             {
                                 Id = departmentRole.Id,
                                 Name = departmentRole.RoleName
                             }
                         }).ToList();

                return q;
            }
        }



        public IList<UserDepartment> GetListByDepartmentId(int departmentId)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from userDepartment in model.tic_UserDepartments
                         join user in model.users on userDepartment.UserId equals user.Id
                         join departmentRole in model.tic_DepartmentRoles on userDepartment.DepartmentRoleId equals departmentRole.Id
                         where userDepartment.IsActive
                         where userDepartment.DepartmentId == departmentId
                         select new UserDepartment
                         {
                             Id = userDepartment.Id,
                             User = new CustomUser
                             {
                                 Id = user.Id,
                                 FirstName = user.FirstName,
                                 LastName = user.LastName,
                                 Email = user.Email
                             },
                             DepartmentRole = new DepartmentRole
                             {
                                 Id = departmentRole.Id,
                                 Name = departmentRole.RoleName
                             }
                         }).ToList();

                return q;
            }
        }


        public void Insert(IList<UserDepartment> userDepartments)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                foreach (var userDepartment in userDepartments)
                {
                    var newRecord = new tic_UserDepartments
                    {
                        UserId = userDepartment.UserId,
                        DepartmentRoleId = userDepartment.DepartmentRoleId,
                        CreatedBy = "Admin",
                        CreationDate = DateTime.Now,
                        IsActive = true,
                        DepartmentId = userDepartment.DepartmentId
                    };

                    model.tic_UserDepartments.Add(newRecord);
                }

                model.SaveChanges();
            }
        }


        public void Update(UserDepartmentCollectionForDepartment data)
        {
            var updateCollection = data.ItemsToUpdate;
            var deleteCollection = data.ItemsToDelete;
            var insertCollection = data.ItemsToAdd;

            if (updateCollection != null)
                Update(updateCollection);
            if (deleteCollection != null)
                Delete(deleteCollection);
            if (insertCollection != null)
                Insert(insertCollection);
        }


        public void Update(UserDepartmentCollectionForStaff data)
        {
            var updateCollection = data.ItemsToUpdate;
            var deleteCollection = data.ItemsToDelete;
            var insertCollection = data.ItemsToAdd;

            if (updateCollection != null)
                Update(updateCollection);
            if (deleteCollection != null)
                Delete(deleteCollection);
            if (insertCollection != null)
                Insert(insertCollection);
        }


        public IList<UserDepartment> Update(IList<UserDepartment> userDepartments)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                foreach (var userDepartment in userDepartments)
                {
                    var recordToEdit = model.tic_UserDepartments.First(rec => rec.Id == userDepartment.Id);
                    recordToEdit.UserId = userDepartment.UserId;
                    recordToEdit.DepartmentRoleId = userDepartment.DepartmentRoleId;
                    recordToEdit.UpdatedBy = Admin;
                    recordToEdit.LastUpdate = DateTime.Now;
                    
                }

                model.SaveChanges();

                return userDepartments;
            }
        }


        public void Delete(IList<UserDepartment> userDepartments)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                foreach (var userDepartment in userDepartments)
                {
                    var recordToDelete = model.tic_UserDepartments.First(rec => rec.Id == userDepartment.Id);
                    recordToDelete.UpdatedBy = Admin;
                    recordToDelete.LastUpdate = DateTime.Now;
                    recordToDelete.IsActive = false;
                }

                model.SaveChanges();
            }
        }


        public void Delete(int id)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToDelete = model.tic_UserDepartments.First(rec => rec.Id == id);
                recordToDelete.UpdatedBy = Admin;
                recordToDelete.LastUpdate = DateTime.Now;
                recordToDelete.IsActive = false;

                model.SaveChanges();
            }
        }
    }
}
