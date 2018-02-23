using System.Web.Mvc;

namespace LSports.Areas.Tickets
{
    public class TicketsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Tickets";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Tickets_default",
                "Tickets/{controller}/{action}/{id}",
                new { controller = "Ticket", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}