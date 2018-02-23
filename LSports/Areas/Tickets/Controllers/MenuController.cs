using System.Web.Mvc;
using LSports.Framework.Models;
using LSports.Services;
using LSports.Services.Interfaces;

namespace LSports.Areas.Tickets.Controllers
{
    public class MenuController : Controller
    {
        private readonly IMenuService _menuService;

        public MenuController() : this(new MenuService())
        {
            
        }

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        // GET: Menu
        public JsonResult Staff()
        {
            var list = _menuService.GetMenuItems(System.Web.HttpContext.Current.User.Identity.Name, HttpContext.User.IsInRole(TicRoles.Admin));
            foreach(var rootItem in list)
                foreach (var menuItem in rootItem.Subitems)
                    menuItem.Url = Url.Action("Index", "Ticket", menuItem.Parameters);

            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}