using System.Web.Mvc;
using LSports.Framework.DataAccess.Repositories;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Areas.Tickets.Controllers
{
    public class TicketTypeController : Controller
    {
        private readonly ITicketTypeRepository _ticketTypeRepository;

        public TicketTypeController() : this(new TicketTypeRepository())
        {
        }

        public TicketTypeController(ITicketTypeRepository ticketTypeRepository)
        {
            _ticketTypeRepository = ticketTypeRepository;
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
            var list = _ticketTypeRepository.GetList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Update(TicketType record)
        {
            _ticketTypeRepository.Update(record);
            return Json(record);
        }


        [HttpPost]
        public JsonResult Insert(TicketType record)
        {
            _ticketTypeRepository.Insert(record);
            return Json(record);
        }


        [HttpPost]
        public JsonResult Delete(int id)
        {
            _ticketTypeRepository.Delete(id);
            return Json("0");
        }
    }
}