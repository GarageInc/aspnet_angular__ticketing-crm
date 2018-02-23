using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using LSports.Framework.DataAccess.Repositories;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models;
using LSports.Framework.Models.CustomClasses;
using LSports.Services;
using LSports.Services.Interfaces;

namespace LSports.Areas.Tickets.Controllers
{
    public class StaffManagementController : Controller
    {
        private readonly IUserDepartmentRepository _userDepartmentRepository;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        // private readonly StaffRepository _staffRepository
        public StaffManagementController() : this(new UserService(), new UserDepartmentRepository(), new EmailService())
        {
        }

        public StaffManagementController(IUserService userService, IUserDepartmentRepository userDepartmentRepository, IEmailService emailService)
        {
            _userService = userService;
            _userDepartmentRepository = userDepartmentRepository;
            _emailService = emailService;
        }



        // GET: StaffManagement
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Central()
        {
            return View();
        }


        public JsonResult List()
        {
            var list = _userService.GetUsersByRoles(new List<string> { TicRoles.Staff, TicRoles.Admin });

            return Json(list, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Method to update UserDepartments. 
        /// </summary>
        /// <param name="data">Collection of UserDepartments to add/delete/update. It also contains User Detials</param>
        /// <returns></returns>
        public ActionResult UpdateUserDepartments(UserDepartmentCollectionForStaff data)
        {
            _userDepartmentRepository.Update(data);

            return Json("Ok");
        }


        public JsonResult ActionResultListForUser(string userId)
        {
            var list = _userDepartmentRepository.GetListByUserId(userId);

            return Json(list, JsonRequestBehavior.AllowGet);
        }




        [HttpPost]
        public JsonResult Update(Staff record)
        {
            _userService.Update(record);

            return Json(record);
        }


        [HttpPost]
        public async Task<JsonResult> Insert(Staff record)
        {
            _userService.Insert(record);

            var emailText = HttpUtility.UrlDecode(ConfigurationManager.AppSettings["EmailTemplateForRegisteredStaff"]);

            var substitutions = new NameValueCollection
            {
                {"%url%", Url.Action("Login", "Account", null, Request.Url?.Scheme).Replace("/Tickets", "")},
                {"%firstname%", record.FirstName},
                {"%login%", record.UserName},
                {"%password%", record.Password}
            };

            await _emailService.Send(record.UserName, "Ticketing System Registration", emailText, substitutions);

            return Json(record);
        }


        [HttpPost]
        public JsonResult Delete(string id)
        {
            _userService.Delete(id);

            return Json("0");
        }
    }
}