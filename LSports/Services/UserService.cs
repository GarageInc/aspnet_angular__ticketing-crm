using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LSports.Framework.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using LSports.Framework.Models.CustomClasses;
using LSports.Services.Interfaces;

namespace LSports.Services
{
    public class UserService : IUserService
    {
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public IEnumerable<UserListItem> GetUsersByRoles(IEnumerable<string> roles)
        {
            var list = UserManager.GetUsersByRoles(roles).OrderBy(u => u.Name);

            return list;
        }

        public IEnumerable<UserListItem> GetList()
        {
            return UserManager.Users.OrderBy(u => u.UserName).Select(u => new UserListItem { Id = u.Id, Name = u.UserName, IsAdministrator = UserManager.IsInRole(u.Id, TicRoles.Admin)});
        }

        public Staff Insert(Staff record)
        {
            var applicationUser = new ApplicationUser
            {
                Email = record.UserName,
                UserName = record.UserName,
                FirstName = record.FirstName,
                LastName = record.LastName,
            };

            record.Password = Guid.NewGuid().ToString().Split('-')[0];

            UserManager.Create(applicationUser, record.Password);

            var roleToAdd = record.IsAdministrator ? TicRoles.Admin : TicRoles.Staff;

            UserManager.AddToRole(applicationUser.Id, roleToAdd);

            record.Id = applicationUser.Id;

            return record;
        }

        public void Update(Staff record)
        {
            var userToUpdate = UserManager.FindById(record.Id);

            userToUpdate.FirstName = record.FirstName;
            userToUpdate.LastName = record.LastName;
            userToUpdate.Email = record.UserName;
            userToUpdate.UserName = record.UserName;

            if (record.IsAdministrator)
            {
                UserManager.RemoveFromRole(record.Id, TicRoles.Staff);
                UserManager.RemoveFromRole(record.Id, TicRoles.Admin);

                UserManager.AddToRole(record.Id, TicRoles.Admin);
            }
            else
            {
                UserManager.RemoveFromRole(record.Id, TicRoles.Staff);
                UserManager.RemoveFromRole(record.Id, TicRoles.Admin);

                UserManager.AddToRole(record.Id, TicRoles.Staff);
            }

            UserManager.Update(userToUpdate);
        }

        public void Delete(string id)
        {
            var userToDelete = UserManager.FindById(id);

            UserManager.Delete(userToDelete);
        }

        public bool IsEmailUnique(string email, string id)
        {
            var user = UserManager.FindByEmail(email);

            return user == null || user.Id == id;
        }
    }
}