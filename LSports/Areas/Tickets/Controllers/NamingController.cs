using System.Web.Mvc;
using LSports.Framework.DataAccess.Repositories;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Services;
using LSports.Services.Interfaces;

namespace LSports.Areas.Tickets.Controllers
{
    public class NamingController : Controller
    {
        private readonly IIssueTypeRepository _issueTypeRepository;
        private readonly ITicketTypeRepository _ticketTypeRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly ICustomFieldRepository _customFieldRepository;
        private readonly IUserService _userService;
        private readonly IEmailTemplateRepository _emailTemplateRepository;


        public NamingController() : this(new IssueTypeRepository(), new TicketTypeRepository(), new DepartmentRepository(), new UserService(), new CustomFieldRepository(), new EmailTemplateRepository())
        {
        }


        public NamingController(IIssueTypeRepository issueTypeRepository, ITicketTypeRepository ticketTypeRepository, IDepartmentRepository departmentRepository, IUserService userService, ICustomFieldRepository customFieldRepository, IEmailTemplateRepository emailTemplateRepository)
        {
            _issueTypeRepository = issueTypeRepository;
            _ticketTypeRepository = ticketTypeRepository;
            _departmentRepository = departmentRepository;
            _customFieldRepository = customFieldRepository;
            _userService = userService;
            _emailTemplateRepository = emailTemplateRepository;
        }


        // GET: NameUnique
        public JsonResult IsIssueTypeNameUnique(string name, int id)
        {
            return Json(_issueTypeRepository.IsIssueTypeNameUnique(name, id), JsonRequestBehavior.AllowGet);
        }


        public JsonResult IsTicketTypeNameUnique(string name, int id)
        {
            return Json(_ticketTypeRepository.IsTicketTypeNameUnique(name, id), JsonRequestBehavior.AllowGet);
        }


        public JsonResult IsDepartmentNameUnique(string name, int id)
        {
            return Json(_departmentRepository.IsDepartmentNameUnique(name, id), JsonRequestBehavior.AllowGet);
        }


        public JsonResult IsEmailUnique(string email, string id)
        {
            return Json(_userService.IsEmailUnique(email, id), JsonRequestBehavior.AllowGet);
        }


        public JsonResult IsCustomFieldNameUnique(string name, int id)
        {
            return Json(_customFieldRepository.IsCustomFieldNameUnique(name, id), JsonRequestBehavior.AllowGet);
        }


        public JsonResult IsCustomFieldIdentifierUnique(string identifier, int id)
        {
            return Json(_customFieldRepository.IsCustomFieldIdentifierUnique(identifier, id), JsonRequestBehavior.AllowGet);
        }


        public JsonResult IsCustomFieldValuesUnique(string identifier, string name, int id)
        {
            return Json(_customFieldRepository.IsCustomFieldValuesUnique(name, identifier, id), JsonRequestBehavior.AllowGet);
        }


        public JsonResult IsEmailTemplateNameUnique(string name, int id)
        {
            return Json(_emailTemplateRepository.IsEmailTemplateNameUnique(id, name), JsonRequestBehavior.AllowGet);
        }
    }
}