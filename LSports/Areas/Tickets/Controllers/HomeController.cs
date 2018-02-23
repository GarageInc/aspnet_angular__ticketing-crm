using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AspNet.Identity.MySQL;
using LSports.Framework.Models;
using LSports.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace LSports.Areas.Tickets.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var roleManager = HttpContext.GetOwinContext().Get<ApplicationRoleManager>();

            //IdentityRole adminRole;
            //if (!roleManager.RoleExists(TicRoles.Admin))
            //{
            //    adminRole = new IdentityRole(TicRoles.Admin);
            //    roleManager.Create(adminRole);
            //}
            //else
            //{
            //    adminRole = roleManager.FindByName(TicRoles.Admin);
            //}

            ////Create Loan Officer role if it does not exist
            IdentityRole staffRole;
            if (!roleManager.RoleExists(TicRoles.Staff))
            {
                staffRole = new IdentityRole(TicRoles.Staff);
                roleManager.Create(staffRole);
            }
            else
            {
                staffRole = roleManager.FindByName(TicRoles.Staff);
            }

            ////Create Borrower role if it does not exist
            //IdentityRole customerContactRole;
            //if (!roleManager.RoleExists(TicRoles.CustomerContact))
            //{
            //    customerContactRole = new IdentityRole(TicRoles.CustomerContact);
            //    roleManager.Create(customerContactRole);
            //}
            //else
            //{
            //    customerContactRole = roleManager.FindByName(TicRoles.CustomerContact);
            //}

            //var user = userManager.FindByName("admin@ambaha.com");
            //// Add super admin user
            //if (userManager.FindByName("admin@ambaha.com") == null)
            //{
            //    #region Add Super Admin

            //    var userToInsert = new ApplicationUser
            //    {
            //        UserName = "admin@ambaha.com",
            //        Email = "admin@ambaha.com",
            //        FirstName = "Tic",
            //        LastName = "Super Admin"
            //    };
            //    userManager.Create(userToInsert, "Ambaha@2016");

            //    var rolesForUser = userManager.GetRoles(userToInsert.Id);
            //    if (!rolesForUser.Contains(adminRole.Name))
            //    {
            //        userManager.AddToRole(userToInsert.Id, adminRole.Name);
            //    }

            //    #endregion
            //}

            for (int i = 1; i <= 6; i++)
            {
                // Add super admin user
                var userToInsert = userManager.FindByName($"staff{i}@ambaha.com");
                if (userToInsert == null)
                {
                    userToInsert = new ApplicationUser
                    {
                        UserName = $"staff{i}@ambaha.com",
                        Email = $"staff{i}@ambaha.com",
                        FirstName = "Tic",
                        LastName = $"Staff {i}"
                    };
                    userManager.Create(userToInsert, "Staff@2016");
                }
                var rolesForUser = userManager.GetRoles(userToInsert.Id);
                if (!rolesForUser.Contains(staffRole.Name))
                {
                    userManager.AddToRole(userToInsert.Id, staffRole.Name);
                }
            }

            //for (int i = 1; i <= 6; i++)
            //{
            //    // Add super admin user
            //    if (userManager.FindByName($"customer{i}@ambaha.com") == null)
            //    {
            //        #region Add Super Admin

            //        var userToInsert = new ApplicationUser
            //        {
            //            UserName = $"customer{i}@ambaha.com",
            //            Email = $"customer{i}@ambaha.com",
            //            FirstName = "Tic",
            //            LastName = $"Customer Contact {i}"
            //        };
            //        userManager.Create(userToInsert, "Customer@2016");

            //        var rolesForUser = userManager.GetRoles(userToInsert.Id);
            //        if (!rolesForUser.Contains(customerContactRole.Name))
            //        {
            //            userManager.AddToRole(userToInsert.Id, customerContactRole.Name);
            //        }

            //        #endregion
            //    }
            //}

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}