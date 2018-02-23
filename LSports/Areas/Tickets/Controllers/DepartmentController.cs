using System.Web.Mvc;
using LSports.Framework.DataAccess.Repositories;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Areas.Tickets.Controllers
{
    //[Authorize(Roles = TicRoles.Admin)]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IUserDepartmentRepository _userDepartmentRepository;

        public DepartmentController() : this(new DepartmentRepository())
        {
        }


        public DepartmentController(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
            _userDepartmentRepository = new UserDepartmentRepository();
        }


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
            var list = _departmentRepository.GetList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Update(Department record)
        {
            _departmentRepository.Update(record);
            return Json(record);
        }



        [HttpPost]
        public JsonResult Insert(Department record)
        {
            _departmentRepository.Insert(record);
            return Json(record);
        }


        [HttpPost]
        public JsonResult Delete(int id)
        {
            _departmentRepository.Delete(id);
            return Json("0");
        }



        [HttpPost]
        public JsonResult ListForDepartment(int id)
        {
            return Json(_userDepartmentRepository.GetListByDepartmentId(id), JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Method to update UserDepartments. 
        /// </summary>
        /// <param name="data">Collection of UserDepartments to add/delete/update. It also contains Department Data</param>
        /// <returns></returns>
        public ActionResult UpdateUserDepartments(UserDepartmentCollectionForDepartment data)
        {
            _userDepartmentRepository.Update(data);
            return Json("Ok");
        }


        //public ActionResult AddDepartmentWithUserDepartments(UserDepartmentCollectionForDepartment data)
        //{
        //    return Json("Ok");
        //}
    }
}