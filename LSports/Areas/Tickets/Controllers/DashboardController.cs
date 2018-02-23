using System.Web.Mvc;

namespace LSports.Areas.Tickets.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult DashboardHeader()
        {
            return View();
        }
        public ActionResult DashboardSidebar()
        {
            return View();
        }
        public ActionResult DashboardRightSidebar()
        {
            return View();
        }
    }
}