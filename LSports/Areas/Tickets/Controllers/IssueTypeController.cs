using System.Web.Mvc;
using LSports.Framework.DataAccess.Repositories;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Areas.Tickets.Controllers
{
    [Authorize]
    public class IssueTypeController : Controller
    {
        private readonly IIssueTypeRepository _issueTypeRepository;

        public IssueTypeController() : this(new IssueTypeRepository())
        {
        }

        public IssueTypeController(IIssueTypeRepository issueTypeRepository)
        {
            _issueTypeRepository = issueTypeRepository;
        }

        public ActionResult Index()
        {
            if (User.IsInRole(TicRoles.Staff) || User.IsInRole(TicRoles.CustomerContact))
            {
                return RedirectToAction("Index", "Ticket");
            }

            return View();
        }

        public ActionResult Central()
        {
            return View();
        }

        public JsonResult List()
        {
            var list = _issueTypeRepository.GetList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Update(IssueType record)
        {
            _issueTypeRepository.Update(record);
            return Json(record);
        }


        [HttpPost]
        public JsonResult Insert(IssueType record)
        {
            _issueTypeRepository.Insert(record);
            return Json(record);
        }


        [HttpPost]
        public JsonResult Delete(int id)
        {
            _issueTypeRepository.Delete(id);
            return Json("0");
        }
    }
}