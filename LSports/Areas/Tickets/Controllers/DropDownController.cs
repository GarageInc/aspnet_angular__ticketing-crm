using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LSports.Framework.DataAccess.Repositories;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models;
using LSports.Framework.Models.CustomClasses;
using LSports.Services;
using LSports.Services.Interfaces;

namespace LSports.Areas.Tickets.Controllers
{
    public class DropDownController : Controller
    {
        private readonly IIconRepository _iconRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IUserService _userService;
        private readonly IDepartmentRoleRepository _departmentRoleRepository;
        private readonly IDatabaseRepository _databaseRepository;
        private readonly ICustomFieldRepository _customFieldRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly ITicketTypeRepository _ticketTypeRepository;
        private readonly IIssueTypeRepository _issueTypeRepository;
        private readonly IEmailTemplateRepository _emailTemplateRepository;
        private readonly ICustomerPriorityRepository _customerPriorityRepository;
        private readonly ITicketStatusRepository _ticketStatusRepository;
        private readonly ITicketRepository _ticketRepository;


        public DropDownController() : this(new IconRepository(), new ProductRepository(), new ProductCategoryRepository(), new UserService(), 
            new DepartmentRoleRepository(), new DatabaseRepository(), new CustomFieldRepository(), new DepartmentRepository(), new IssueTypeRepository(), new TicketTypeRepository(), new EmailTemplateRepository(),
            new CustomerPriorityRepository(), new TicketStatusRepository(), new TicketRepository())
        {
        }

        public DropDownController(IIconRepository iconRepository, IProductRepository productRepository, IProductCategoryRepository productCategoryRepository, IUserService userService,
            IDepartmentRoleRepository departmentRoleRepository, IDatabaseRepository databaseRepository, ICustomFieldRepository customFieldRepository, IDepartmentRepository departmentRepository, 
            IIssueTypeRepository issueTypeRepository, ITicketTypeRepository ticketTypeRepository, IEmailTemplateRepository emailTemplateRepository,
            ICustomerPriorityRepository customerPriorityRepository, ITicketStatusRepository ticketStatusRepository, ITicketRepository ticketRepository)
        {
            _iconRepository = iconRepository;
            _productRepository = productRepository;
            _departmentRoleRepository = departmentRoleRepository;
            _userService = userService;
            _productCategoryRepository = productCategoryRepository;
            _databaseRepository = databaseRepository;
            _customFieldRepository = customFieldRepository;
            _departmentRepository = departmentRepository;
            _issueTypeRepository = issueTypeRepository;
            _ticketTypeRepository = ticketTypeRepository;
            _emailTemplateRepository = emailTemplateRepository;
            _customerPriorityRepository = customerPriorityRepository;
            _ticketStatusRepository = ticketStatusRepository;
            _ticketRepository = ticketRepository;
        }

        public  JsonResult Icons()
        {
            var list = _iconRepository.GetList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult TicketTypes()
        {
            var list = _ticketTypeRepository.GetList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult Products()
        {
            var list = _productRepository.GetList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult ProductCategories()
        {
            var list = _productCategoryRepository.GetList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult IssueTypes()
        {
            var list = _issueTypeRepository.GetList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult Departments()
        {
            var list = _departmentRepository.GetList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Users()
        {
            var list = _userService.GetList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DepartmentUsers(int departmentId, int ticketId)
        {
            var ticket = _ticketRepository.GetTicketById(ticketId);
            var list = _departmentRepository.GetDepartmentUsers(departmentId);

            if (!string.IsNullOrEmpty(ticket.AssignedTo))
            {
                var itemToDelete = list.FirstOrDefault(u => u.Id == ticket.AssignedTo);
                if (itemToDelete != null)
                    list.Remove(itemToDelete);
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult Staff()
        {
            var list = _userService.GetUsersByRoles(new List<string> { TicRoles.Staff});
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult DepartmentRoles()
        {
            var list = _departmentRoleRepository.GetList();
            
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult SendEmailTos()
        {
            var list = _emailTemplateRepository.GetSendEmailToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult SendEmailActions()
        {
            var list = _emailTemplateRepository.GetEmailActionsList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult CustomFields()
        {
            var list = _customFieldRepository.GetList(null);

            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult Priorities()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };

            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult TableNames()
        {
            var list = _databaseRepository.GetTableNames();

            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult TableFieldNames(string dbTableName)
        {
            var list = _databaseRepository.GetTableFieldNames(dbTableName);

            return Json(list, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// Action returns values for the dropdown lists of custom fields.
        /// </summary>
        /// <param name="dbTableName">CASE SENSISTIVE! The value should be exacltly the same as in the database</param>
        /// <param name="dbTableIdFieldName">CASE SENSISTIVE! The value should be exacltly the same as in the database</param>
        /// <param name="dbTableTextFieldName">CASE SENSISTIVE! The value should be exacltly the same as in the database</param>
        /// <param name="dbFilterFieldName">Optional parameter. This ia the name of the filter field. CASE SENSISTIVE! The value should be exacltly the same as in the database</param>
        /// <param name="dbFilterFieldValue">Optional parameter. This is the value of the filter field. CASE SENSISTIVE! The value should be exacltly the same as in the database</param>
        /// <returns></returns>
        public JsonResult TableFieldValues(string dbTableName, string dbTableIdFieldName, string dbTableTextFieldName, string dbFilterFieldName, string dbFilterFieldValue)
        {
            var list = new List<DropdownItem>();

            if (string.IsNullOrEmpty(dbFilterFieldName) || string.IsNullOrEmpty(dbFilterFieldValue))
                list = _databaseRepository.GetDropdownValuesFromTable(dbTableName, dbTableIdFieldName, dbTableTextFieldName).ToList();
            else
                list = _databaseRepository.GetDropdownValuesFromTable(dbTableName, dbTableIdFieldName, dbTableTextFieldName, dbFilterFieldName, dbFilterFieldValue).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }



        public JsonResult CustomFieldTypes()
        {
            var list = _customFieldRepository.GetCustomFieldTypes();

            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult CustomFieldDropdowns()
        {
            var list = _customFieldRepository.GetCustomFieldDropdowns();

            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult CustomFieldIdentifiersForEmailTemplate()
        {
            var list = _customFieldRepository.GetCustomFieldIdentifiers();

            list.Add(new CustomFieldIdentifier {Id = "TicketId", Name = "%Ticket Id%", TypeId = 8, TypeName = "Numeric"});
            list.Add(new CustomFieldIdentifier { Id = "ReplyText", Name = "%Reply Text%", TypeId = 1, TypeName = "Text" });
            list.Add(new CustomFieldIdentifier { Id = "CommentText", Name = "%Comment Text%", TypeId = 1, TypeName = "Text" });
            list.Add(new CustomFieldIdentifier { Id = "TicketUrl", Name = "%Ticket Url%", TypeId = 1, TypeName = "Text" });
            list.Add(new CustomFieldIdentifier { Id = "PrevStatus", Name = "%Prev Status%", TypeId = 1, TypeName = "Text" });
            list.Add(new CustomFieldIdentifier { Id = "NewStatus", Name = "%New Status%", TypeId = 1, TypeName = "Text" });
            list.Add(new CustomFieldIdentifier { Id = "PrevAssignee", Name = "%Prev Assignee%", TypeId = 1, TypeName = "Text" });
            list.Add(new CustomFieldIdentifier { Id = "NewAssignee", Name = "%New Assignee%", TypeId = 1, TypeName = "Text" });
            list.Add(new CustomFieldIdentifier { Id = "TicketCustomFieldValues", Name = "%Ticket Custom Field Values%", TypeId = 1, TypeName = "Text" });
            list.Add(new CustomFieldIdentifier { Id = "CreatedBy", Name = "%Created By%", TypeId = 1, TypeName = "Text" });
            list.Add(new CustomFieldIdentifier { Id = "Customer", Name = "%Customer%", TypeId = 1, TypeName = "Text" });
            list.Add(new CustomFieldIdentifier { Id = "CreatedOn", Name = "%Created On%", TypeId = 1, TypeName = "Text" });
            list.Add(new CustomFieldIdentifier { Id = "LastTicketLogRecord", Name = "%Last Ticket Log Record%", TypeId = 1, TypeName = "Text" });

            return Json(list.OrderBy(rec=>rec.Name), JsonRequestBehavior.AllowGet);
        }


        public JsonResult CustomerPriorities()
        {
            var list = _customerPriorityRepository.GetList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult ClosedTicketStatuses()
        {
            var list = _ticketStatusRepository.GetClosedTicketStatuses();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

    }
}