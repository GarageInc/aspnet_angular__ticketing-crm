using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using LSports.Framework.DataAccess.Repositories;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models.CustomClasses;
using LSports.Services;
using LSports.Services.Interfaces;

namespace LSports.Areas.Tickets.Controllers
{
    public class EmailTemplateController : Controller
    {
        private readonly IEmailTemplateRepository _emailTemplateRepository;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;


        public EmailTemplateController() : this(new EmailTemplateRepository(), new UserRepository(), new EmailService(), new EmailTemplateService())
        {
        }


        public EmailTemplateController(IEmailTemplateRepository emailTemplateRepository, IUserRepository userRepository, IEmailService emailService, IEmailTemplateService emailTemplateService)
        {
            _emailTemplateRepository = emailTemplateRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
        }

        // GET: FormTemplate
        public ActionResult Index()
        {
            return View();
        }


        public JsonResult List()
        {
            var list = _emailTemplateRepository.GetList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Update(MailTemplate mailTemplate)
        {
            _emailTemplateService.Update(mailTemplate);
            return Json(mailTemplate);
        }



        [HttpPost]
        public JsonResult Insert(MailTemplate mailTemplate)
        {
            _emailTemplateService.Insert(mailTemplate);
            return Json(mailTemplate);
        }


        [HttpPost]
        public JsonResult Delete(int id)
        {
            _emailTemplateRepository.Delete(id);
            return Json("0");
        }


        [HttpPost]
        public async Task<JsonResult> SendTestEmail(string bodyHtml)
        {
            var user = _userRepository.GetUserByName(HttpContext.User.Identity.Name);

            bodyHtml = HttpUtility.UrlDecode(bodyHtml);

            await _emailService.Send(user.Email, "Test Email", bodyHtml);

            return Json("0");
        }



    }
}