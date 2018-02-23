using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using LSports.Framework.DataAccess.Repositories;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.Models.Enums;
using LSports.Services;
using LSports.Services.Interfaces;
using LSports.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace LSports.Areas.Tickets.Controllers
{
    [Authorize(Roles = TicRoles.Staff + "," + TicRoles.CustomerContact +","+TicRoles.Admin)]
    public class TicketController : Controller
    {

        

        private IList<ControlEnum> GetMockControls(TicketModel ticket, string userId, IList<string> userRoles)
        {
            return _ticketService.GetTicketButtonsAndControls(ticket, userId, userRoles);
            /*var result = new List<ControlEnum>();
            result.Add(ControlEnum.BtnEditTicket);
            result.Add(ControlEnum.BtnReply);
            result.Add(ControlEnum.BtnComment);
            result.Add(ControlEnum.BtnAssignToAnotherDepartment);
            result.Add(ControlEnum.BtnAssignToAnotherUser);
            result.Add(ControlEnum.BtnUnassign);
            result.Add(ControlEnum.BtnReopenTicket);
            result.Add(ControlEnum.BtnCloseTicket);
            result.Add(ControlEnum.LblAssignedTo);
            result.Add(ControlEnum.BtnAssgnToMe);

            return result;*/

        }

        private TicketScreenViewModel PrepareTicketScreenViewModel(TicketLog log, int ticketId, string userId,
            IList<string> roles)
        {
            var ticket = _ticketRepository.GetTicketById(ticketId);
            var showCustomerDetails = !ticket.DepartmentId.HasValue || _departmentRepository.GetItemById(ticket.DepartmentId.Value).CanSeeCustomerDetails;
            var result = new TicketScreenViewModel
            {
                Log = log,
                Ticket = ticket,
                CustomFieldValues = _ticketRepository.GetCustomFieldValues(ticketId),
                Attchments = _ticketRepository.GetTicketAttachedFiles(ticketId),
                ShowCustomerDetails = showCustomerDetails,
            };
            result.Controls = GetMockControls(result.Ticket, userId, UserManager.GetRoles(userId));

            if (roles.Contains(TicRoles.Staff) || roles.Contains(TicRoles.Admin))
            {
                result.InstructionsTextHtml = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
                result.InstructionsLinksHtml = "<div class=\"col - lg - 11\"><a>Quick link 1</a></ div > ";
            }

            return result;
        }


        private readonly ITicketService _ticketService;
        private readonly ITicketRepository _ticketRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly ITicketLogRepository _ticketLogRepository;
        private readonly IFormTemplateRepository _formTemplateRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserDepartmentRepository _userDepartmentRepository;
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public TicketController() : this(new TicketRepository(), new FormTemplateRepository(), new UserDepartmentRepository(), new TicketLogRepository(), new TicketService(), new UserRepository(), new DepartmentRepository())
        {
        }

        public TicketController(ITicketRepository ticketRepository, IFormTemplateRepository formTemplateRepository, IUserDepartmentRepository userDepartmentRepository, ITicketLogRepository ticketLogRepository, ITicketService ticketService, IUserRepository userRepository, IDepartmentRepository departmentRepository)
        {
            _ticketRepository = ticketRepository;
            _formTemplateRepository = formTemplateRepository;
            _userDepartmentRepository = userDepartmentRepository;
            _ticketLogRepository = ticketLogRepository;
            _ticketService = ticketService;
            _userRepository = userRepository;
            _departmentRepository = departmentRepository;
        }

        public ActionResult Index(int? ticketId)
        {
            var user = _userRepository.GetUserByName(HttpContext.User.Identity.Name);
            var userDepartments = _userDepartmentRepository.GetListByUserId(user.Id);

            var model = _ticketService.GetTicketsModelForStaff(userDepartments, user);

            ViewBag.tickets = model;

            ViewBag.userDepartments = userDepartments;

            return View();
        }


        public JsonResult TicketInstructionsAndCustomFields(int ticketId)
        {
            var userId = User.Identity.GetUserId();
            var result = PrepareTicketScreenViewModel(new TicketLog(), ticketId, userId, UserManager.GetRoles(userId));

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Central()
        {
            //ViewBag.IsCustomer = HttpContext.User.IsInRole(TicRoles.CustomerContact);
            return View();
        }

        public ActionResult AhaFileAttachment()
        {
            return View();
        }

        public ActionResult CreateTicketWizard()
        {
            return View();
        }



        public JsonResult TicketsUpdated(long secondsAgo)
        {
            var ticketIds = _ticketRepository.GetUpdatedTickets(DateTime.Now.AddSeconds(-secondsAgo));
            
            return Json(ticketIds.Count > 0, JsonRequestBehavior.AllowGet);
        }


        // GET: TicketType
        public JsonResult List(int? departmentId, string status, string assignee)
        {
            var result = _ticketService.GetTicketsModelByFilters(departmentId, status, assignee, HttpContext.User.IsInRole(TicRoles.Admin));

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetTicketLogs(int ticketId)
        {
            var ticket = _ticketRepository.GetTicketById(ticketId);
            var department = _departmentRepository.GetItemById(ticket.DepartmentId??0);

            var result = _ticketLogRepository.GetList(ticketId, HttpContext.User.IsInRole(TicRoles.CustomerContact), !department.CanSeeCustomerDetails && !HttpContext.User.IsInRole(TicRoles.Admin));

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public JsonResult EditTicketFields(int ticketId)
        {
            var result = _ticketService.GetTicketFields(ticketId);

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetAttachedFiles(int ticketId)
        {
            var result = _ticketRepository.GetTicketAttachedFiles(ticketId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public async Task<JsonResult> Update(TicketModel record)
        {
            await _ticketService.UpdateTicket(record);

            return Json(record);
        }


        [HttpPost]
        public  JsonResult Insert(TicketModel record)
        {
            var result = _ticketService.CreateBlankTicket(record);

            return Json(result);
        }



        public JsonResult Controls(int ticketId)
        {
            var userId = User.Identity.GetUserId();
            return Json(GetMockControls(_ticketRepository.GetTicketById(ticketId), userId, UserManager.GetRoles(userId)), JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public JsonResult Delete(int id)
        {
            _ticketRepository.Delete(id);
            return Json("0");
        }


        [HttpPost]
        public async Task<JsonResult> CloseTicket(TicketModel ticket, string comment, int? statusId)
        {
            var log = await _ticketService.CloseTicket(ticket, comment, statusId);

            var userId = User.Identity.GetUserId();
            var result = PrepareTicketScreenViewModel(log, ticket.Id, userId, UserManager.GetRoles(userId));

            return Json(result);
        }


        [HttpPost]
        public async Task<JsonResult> AssignToAnotherUser(int ticketId, string userId, string comment)
        {
            var log = await _ticketService.AssignToAnotherUser(ticketId, userId, comment);

            var userGiud = User.Identity.GetUserId();
            var result = PrepareTicketScreenViewModel(log, ticketId, userGiud, UserManager.GetRoles(userId));

            return Json(result);
        }


        [HttpPost]
        public async Task<JsonResult> AssignToAnotherDepartment(int ticketId, int departmentId, string comment)
        {
            var log = await _ticketService.AssignToAnotherDepartment(ticketId, departmentId, comment);

            var userId = User.Identity.GetUserId();
            var result = PrepareTicketScreenViewModel(log, ticketId, userId, UserManager.GetRoles(userId));

            return Json(result);
        }


        [HttpPost]
        public async Task<JsonResult> AddStaffReply(int ticketId, string reply)
        {
            var log = await _ticketService.AddStaffReply(ticketId, reply);

            var userId = User.Identity.GetUserId();
            var result = PrepareTicketScreenViewModel(log, ticketId, userId, UserManager.GetRoles(userId));

            return Json(result);
        }


        [HttpPost]
        public async Task<JsonResult> AddStaffComment(int ticketId, string comment)
        {
            var log = await _ticketService.AddStaffComment(ticketId, comment);

            var userId = User.Identity.GetUserId();
            var result = PrepareTicketScreenViewModel(log, ticketId, userId, UserManager.GetRoles(userId));

            return Json(result);
        }



        /// <summary>
        /// It automatically detects if it's a staff reply or if it's a customer reply
        /// </summary>
        /// <param name="ticketId"></param>
        /// <param name="reply"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> AddReply(int ticketId, string reply)
        {
            TicketLog log;
            var roles = UserManager.GetRoles(User.Identity.GetUserId());

            if (roles.Contains(TicRoles.Staff) || roles.Contains(TicRoles.Admin))
                log = await _ticketService.AddStaffReply(ticketId, reply);
            else
                log = await _ticketService.AddCustomerReply(ticketId, reply);

            var userId = User.Identity.GetUserId();
            var result = PrepareTicketScreenViewModel(log, ticketId, userId, UserManager.GetRoles(userId));

            return Json(result);
        }


        [HttpPost]
        public async Task<JsonResult> AddCustomerReply(int ticketId, string reply)
        {
            var log = await _ticketService.AddCustomerReply(ticketId, reply);

            var userId = User.Identity.GetUserId();
            var result = PrepareTicketScreenViewModel(log, ticketId, userId, UserManager.GetRoles(userId));

            return Json(result);
        }


        [HttpPost]
        public async Task<JsonResult> ReopenTicket(int ticketId)
        {
            var log = await _ticketService.ReopenTicket(ticketId);

            var userId = User.Identity.GetUserId();
            var result = PrepareTicketScreenViewModel(log, ticketId, userId, UserManager.GetRoles(userId));

            return Json(result);
        }


        [HttpPost]
        public async Task<JsonResult> UnassignTicket(int ticketId)
        {
            var log = await _ticketService.UnassignTicket(ticketId);

            var userId = User.Identity.GetUserId();
            var result = PrepareTicketScreenViewModel(log, ticketId, userId, UserManager.GetRoles(userId));

            return Json(result);
        }


        [HttpPost]
        public async Task<JsonResult> AssignToMe(int ticketId)
        {
            var log = await _ticketService.AssignToMe(ticketId);

            var userId = User.Identity.GetUserId();
            var result = PrepareTicketScreenViewModel(log, ticketId, userId, UserManager.GetRoles(userId));

            return Json(result);
        }


        [HttpPost]
        public async Task<JsonResult> SubmitTicketFields(int ticketId, IList<TicketFieldModel> customFields, bool isNewTicket)
        {
            TicketLog log = null;

            if (isNewTicket)
            {
                var ticket = _ticketRepository.GetBlankTicketById(ticketId);
                var ticketData = await _ticketService.CreateTicket(ticket);
                log = ticketData.LastLog;
            }

            var updateLog = await _ticketService.UpdateTicketFields(ticketId, customFields, isNewTicket);
            if (log == null) log = updateLog;

            var userId = User.Identity.GetUserId();
            var result = PrepareTicketScreenViewModel(log, ticketId, userId, UserManager.GetRoles(userId));

            return Json(result);
        }


        [HttpPost]
        public JsonResult AttachTicketFiles(int ticketId, int customFieldId, IList<int> fileIds)
        {
            var log = _ticketService.AttachFilesToTicket(ticketId, customFieldId, fileIds);

            return Json(log);
        }


        [HttpPost]
        public JsonResult DeleteTicketFiles(int ticketId, int customFieldId, IList<int> fileIds)
        {
            var log = _ticketService.DeleteFilesFromTicket(ticketId, customFieldId, fileIds);

            return Json(log);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ticketId"></param>
        /// <param name="departmentId"></param>
        /// <param name="status">open or closed</param>
        /// <param name="assignee">assignedtome or unassigned or null</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ShouldBeInTheGrid(int ticketId, int? departmentId, string status, string assignee)
        {
            var list = _ticketService.GetTicketsModelByFilters(departmentId, status, assignee, HttpContext.User.IsInRole(TicRoles.Admin));
            var result = list.FirstOrDefault(rec => rec.Id == ticketId);

            return Json(result != null);
        }
    }
}