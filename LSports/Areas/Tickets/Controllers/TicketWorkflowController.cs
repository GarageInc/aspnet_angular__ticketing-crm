using System.Collections.Generic;
using System.Web.Mvc;
using LSports.Framework.DataAccess.Repositories;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Areas.Tickets.Controllers
{
    public class TicketWorkflowController : Controller
    {
        private readonly ITicketWorkflowRepository _ticketWorkflowRepository;

        public TicketWorkflowController() : this(new TicketWorkflowRepository())
        {
        }

        public TicketWorkflowController(ITicketWorkflowRepository ticketWorkflowRepository)
        {
            _ticketWorkflowRepository = ticketWorkflowRepository;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Central()
        {
            return View();
        }

        // GET: TicketType
        public JsonResult List()
        {
            var list = _ticketWorkflowRepository.GetList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Update(TicketWorkflow record)
        {
            _ticketWorkflowRepository.Update(record);
            return Json(record);
        }


        [HttpPost]
        public JsonResult Insert(TicketWorkflow record)
        {
            _ticketWorkflowRepository.Insert(record);
            return Json(record);
        }


        [HttpPost]
        public JsonResult Delete(int id)
        {
            _ticketWorkflowRepository.Delete(id);
            return Json("0");
        }



        /// <summary>
        /// Action to update Form Templates Sort Order
        /// </summary>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateSortOrder(IList<SortOrderItem> sortOrder)
        {
            _ticketWorkflowRepository.UpdateSortOrder(sortOrder);
            return Json("0");
        }


        [HttpPost]
        public JsonResult Validate(TicketWorkflow record)
        {
            var errors = _ticketWorkflowRepository.Validate(record);
            return Json(errors);
        }
    }
}