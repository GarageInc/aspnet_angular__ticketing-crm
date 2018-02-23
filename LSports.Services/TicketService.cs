using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using LSports.Framework.DataAccess;
using LSports.Framework.DataAccess.Repositories;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.Models.Enums;
using LSports.Services.Interfaces;
using LSports.ViewModels;

namespace LSports.Services
{
    public class TicketService : ITicketService
    {
        private readonly IEmailService _emailService;
        private readonly IEmailSendToService _emailSendToService;
        private readonly ITicketRepository _ticketRepository;
        private readonly ITicketStatusRepository _ticketStatusRepository;
        private readonly ITicketWorkflowRepository _ticketWorkflowRepository;
        private readonly ITicketLogRepository _ticketLogRepository;
        private readonly ITicketFieldRepository _ticketFieldRepository;
        private readonly IFormTemplateRepository _formTemplateRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IEmailTemplateRepository _emailTemplateRepository;
        private readonly IUserDepartmentRepository _userDepartmentRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IDatabaseRepository _databaseRepository;
        private readonly IUserRepository _userRepository;
        private readonly string _ticketDetailsUrl;




        public TicketService():this(new EmailService(), new TicketRepository(), new TicketLogRepository(), new DepartmentRepository(), new UserRepository(), new EmailSendToService(), new EmailTemplateRepository(), new UserDepartmentRepository(), new TicketWorkflowRepository(), new FormTemplateRepository(), new TicketFieldRepository(), new DatabaseRepository(), new CustomerRepository(), new TicketStatusRepository())
        {
            //_ticketDetailsUrl = ticketDetailsUrl;
        }

        public TicketService(IEmailService emailService, ITicketRepository ticketRepository, ITicketLogRepository ticketLogRepository, IDepartmentRepository departmentRepository, IUserRepository userRepository, IEmailSendToService emailSendToService, IEmailTemplateRepository emailTemplateRepository, IUserDepartmentRepository userDepartmentRepository, ITicketWorkflowRepository ticketWorkflowRepository, IFormTemplateRepository formTemplateRepository, ITicketFieldRepository ticketFieldRepository, IDatabaseRepository databaseRepository, ICustomerRepository customerRepository, ITicketStatusRepository ticketStatusRepository)
        {
            _emailService = emailService;
            _ticketRepository = ticketRepository;
            _ticketLogRepository = ticketLogRepository;
            _departmentRepository = departmentRepository;
            _userRepository = userRepository;
            _emailSendToService = emailSendToService;
            _emailTemplateRepository = emailTemplateRepository;
            _userDepartmentRepository = userDepartmentRepository;
            _ticketWorkflowRepository = ticketWorkflowRepository;
            _formTemplateRepository = formTemplateRepository;
            _ticketFieldRepository = ticketFieldRepository;
            _databaseRepository = databaseRepository;
            _customerRepository = customerRepository;
            _ticketStatusRepository = ticketStatusRepository;
        }

        private User GetCurrentUser()
        {
            var user = _userRepository.GetUserByName(HttpContext.Current.User.Identity.Name);
            return user;
        }

        private bool IsCustomer()
        {
            return HttpContext.Current.User.IsInRole(TicRoles.CustomerContact);
        }



        public async Task<TicketLog> AddCustomerReply(int ticketId, string reply)
        {
            var user = GetCurrentUser();

            //Add record to the TicketsLog table
            var log = _ticketLogRepository.Insert(ticketId, (int)LogEntryTypeId.ReplyAddedByCustomer, $"Customer replied: {reply}", user.Name + $" replied: {reply}", user.Id);

            //Change ticket ststus to waiting for staff reply
            _ticketRepository.SetTicketStatus(ticketId, TicketStatusId.WaitingForStaffReply);
            _ticketRepository.UpdateTicketDate(ticketId);

            //Find and Complile email template (replace all %field% with values)
            var emailTemplates = _emailTemplateRepository.GetEmailTemplatesByAction(EmailActionId.CustomerReplyAdded);

            var substitutions = _ticketFieldRepository.GetTicketFieldsSubstitutions(ticketId, GetTicketUrl(ticketId), HttpUtility.HtmlDecode(reply));

            await SendEmails(ticketId, emailTemplates, substitutions);

            return log;
        }

        

        public async Task<TicketLog> AddStaffComment(int ticketId, string comment)
        {
            var user = GetCurrentUser();

            //Add record to the TicketsLog table
            var log = _ticketLogRepository.Insert(ticketId, (int)LogEntryTypeId.CommentAdded, $"Staff commented: {comment}", user.Name + $" commented: {comment}", user.Id);
            _ticketRepository.UpdateTicketDate(ticketId);

            //Find and Complile email template (replace all %field% with values)
            var emailTemplates = _emailTemplateRepository.GetEmailTemplatesByAction(EmailActionId.CommentAdded);

            var substitutions = _ticketFieldRepository.GetTicketFieldsSubstitutions(ticketId, GetTicketUrl(ticketId), string.Empty, HttpUtility.HtmlDecode(comment));

            await SendEmails(ticketId, emailTemplates, substitutions);

            return log;
        }

        public async Task<TicketLog> AddStaffReply(int ticketId, string reply)
        {
            var user = GetCurrentUser();

            //Add record to the TicketsLog table
            var log = _ticketLogRepository.Insert(ticketId, (int)LogEntryTypeId.ReplyAddedByStaff, $"Staff replied: {reply}", user.Name + $" replied: {reply}", user.Id);
            _ticketRepository.UpdateTicketDate(ticketId);

            //Change ticket ststus to waiting for customer reply
            _ticketRepository.SetTicketStatus(ticketId, TicketStatusId.WaitingForCustomerReply);

            //Find and Complile email template (replace all %field% with values)
            var emailTemplates = _emailTemplateRepository.GetEmailTemplatesByAction(EmailActionId.StaffReplyAdded);

            foreach (var emailTemplate in emailTemplates)
            {
                emailTemplate.EmailTemplate += $"<br/>-----TicketId: {ticketId}-----";
            }

            var substitutions = _ticketFieldRepository.GetTicketFieldsSubstitutions(ticketId, GetTicketUrl(ticketId), HttpUtility.HtmlDecode(reply));

            await SendEmails(ticketId, emailTemplates, substitutions);

            return log;
        }

        public async Task<TicketLog> AssignToAnotherDepartment(int ticketId, int departmentId, string comment)
        {
            //Update ticket in the database
            var ticket = _ticketRepository.GetTicketById(ticketId);

            var department = _departmentRepository.GetItemById(departmentId);
            var prevDepartment = ticket.DepartmentId.HasValue
                ? _departmentRepository.GetItemById(ticket.DepartmentId.Value)
                : new Department { Name = "" };

            ticket.DepartmentId = departmentId;
            ticket.PrevAssignee = prevDepartment.Name;
            ticket.NewAssignee = department.Name;
            ticket.PrevStatus = ticket.NewStatus;

            _ticketRepository.Update(ticket);

            var user = GetCurrentUser();
            //Add record to the TicketsLog table
            var log = _ticketLogRepository.Insert(ticket.Id, (int)LogEntryTypeId.AssigneeChanged, $"Ticket was assigned to department: {department.Name}", $"Ticket was assigned to department: {department.Name} by {user.Name}", user.Id);
            _ticketRepository.UpdateTicketDate(ticketId);

            //Find and Complile email template (replace all %field% with values
            var emailTemplates = _emailTemplateRepository.GetEmailTemplatesByAction(EmailActionId.TicketEdited);

            var substitutions = _ticketFieldRepository.GetTicketFieldsSubstitutions(ticketId, GetTicketUrl(ticketId), string.Empty, HttpUtility.HtmlDecode(comment));

            await SendEmails(ticketId, emailTemplates, substitutions);

            return log;
        }

        public async Task<TicketLog> AssignToAnotherUser(int ticketId, string userId, string comment)
        {
            //Update ticket in the database
            var ticket = _ticketRepository.GetTicketById(ticketId);
            var prevUserId = ticket.AssignedTo;
            ticket.AssignedTo = userId;

            if (ticket.TicketStatus.Id == (int) TicketStatusId.Open)
            {
                ticket.TicketStatus.Id = (int) TicketStatusId.InProgress;
                ticket.PrevStatus = ticket.TicketStatus.Name;
                ticket.NewStatus = TicketStatusId.InProgress.ToString();
            }

            var user = _userRepository.GetUserById(userId);
            var prevUser = !string.IsNullOrEmpty(prevUserId) ? _userRepository.GetUserById(prevUserId) : new User();
            ticket.PrevAssignee = prevUser.Name;
            ticket.NewAssignee = user.Name;
            
            _ticketRepository.Update(ticket);

            //Add record to the TicketsLog table
            var log = _ticketLogRepository.Insert(ticket.Id, (int)LogEntryTypeId.AssigneeChanged, $"Ticket was assigned to another user", $"Ticket was assigned to: {user.Name}", user.Id);
            _ticketRepository.UpdateTicketDate(ticketId);

            //Find and Complile email template (replace all %field% with values
            var emailTemplates = _emailTemplateRepository.GetEmailTemplatesByAction(EmailActionId.AssigneeChanged);

            var substitutions = _ticketFieldRepository.GetTicketFieldsSubstitutions(ticketId, GetTicketUrl(ticketId), string.Empty, HttpUtility.HtmlDecode(comment));

            await SendEmails(ticketId, emailTemplates, substitutions);

            if (HttpContext.Current.User.IsInRole(TicRoles.Admin))
                log.EntryValue = log.EntryExtendedValue;

            return log;
        }

        public async Task<TicketLog> CloseTicket(TicketModel ticket, string comment, int? statusId)
        {
            //Update ticket in the database
            if (statusId == null)
                statusId = (int)TicketStatusId.ClosedByUser;

            var prevStatus = _ticketRepository.GetTicketStatusName(ticket.Id);
            _ticketRepository.SetTicketStatus(ticket.Id, (TicketStatusId)statusId);

            var ticketStatus = _ticketStatusRepository.GetClosedTicketStatuses().FirstOrDefault(ts => ts.Id == statusId.Value);

            var ticketById = _ticketRepository.GetTicketById(ticket.Id);
            ticketById.PrevStatus = prevStatus;
            ticketById.NewStatus = ticketStatus.Name;
            ticket.PrevAssignee = ticket.NewAssignee;
            _ticketRepository.UpdateBasic(ticketById);

            var user = GetCurrentUser();
            //Add record to the TicketsLog table
            var log = _ticketLogRepository.Insert(ticket.Id, (int)LogEntryTypeId.StatusChanged, "Ticket status changed to: Closed", $"Ticket status changed to: Closed by {user.Name}. Reason: {ticketStatus.Name}. Comment: {comment}", user.Id);
            _ticketRepository.UpdateTicketDate(ticket.Id);

            //Find and Complile email template (replace all %field% with values
            var emailTemplates = _emailTemplateRepository.GetEmailTemplatesByAction(EmailActionId.StatusChanged);

            var substitutions = _ticketFieldRepository.GetTicketFieldsSubstitutions(ticket.Id, GetTicketUrl(ticket.Id), string.Empty, HttpUtility.HtmlDecode(comment));

            await SendEmails(ticket.Id, emailTemplates, substitutions);

            return log;
        }

        public TicketTemplateViewModel CreateBlankTicket(TicketModel ticket)
        {
            var result = new TicketTemplateViewModel();

            result.TicketModel = _ticketRepository.Insert(ticket);

            var formTemplate = _formTemplateRepository.GetFirstMatchingTemplate(ticket);
            formTemplate.CustomFields = _formTemplateRepository.GetCustomFields(formTemplate.Id);

            result.FormTemplate = formTemplate;

            return result;

        }

        public TicketLog AttachFilesToTicket(int ticketId, int customFieldId, IList<int> fileIds)
        {
            _ticketRepository.AttachFilesToTicket(ticketId, customFieldId, fileIds);

            var user = GetCurrentUser();
            var log = _ticketLogRepository.Insert(ticketId, (int)LogEntryTypeId.TicketEdited, "Ticket was updated: some attachments added.", $"Ticket was updated: some attachments added by {user.Name}.", user.Id);
            _ticketRepository.UpdateTicketDate(ticketId);

            return log;
        }

        public TicketLog DeleteFilesFromTicket(int ticketId, int customFieldId, IList<int> fileIds)
        {
            _ticketRepository.DeleteFilesFromTicket(ticketId, customFieldId, fileIds);

            var user = GetCurrentUser();
            var log = _ticketLogRepository.Insert(ticketId, (int)LogEntryTypeId.TicketEdited, "Ticket was updated: some attachments deleted", $"Ticket was updated: some attachments deleted by {user.Name}", user.Id);
            _ticketRepository.UpdateTicketDate(ticketId);

            return log;
        }


        public async Task<TicketWithLastLogRecord> CreateTicket(TicketModel ticket)
        {
            var user = _userRepository.GetUserByName(HttpContext.Current.User.Identity.Name);
            var customerContact = _customerRepository.GetCustomerContactByUserId(user.Id);

            ticket.Customer = new CustomerModel {Id = customerContact.CustomerId};
            ticket.CustomerContact = new CustomerContactModel { Id = customerContact.Id};
            ticket.PriorityId = customerContact.Priority;
            ticket.TicketStatus = new TicketStatus {Id = (int) TicketStatusId.Open};
            ticket.PrevAssignee = ticket.NewAssignee = customerContact.Name;
            ticket.PrevStatus = ticket.NewStatus = TicketStatusId.Open.ToString();

            //Detemine what department ticket should be assigned to, based on TicketWorkflow table data
            var ticketWorkflowRule = _ticketWorkflowRepository.GetFirstMathingRule(ticket);
            ticket.Department = new Department {Id = ticketWorkflowRule.Department.Id};

            //Add ticket to the database
            _ticketRepository.Update(ticket);

            //Add record to the TicketsLog table
            var log = _ticketLogRepository.Insert(ticket.Id, (int)LogEntryTypeId.TicketCreated, "Ticket was created", $"Ticket was created by {user.Name}", user.Id);
            _ticketRepository.UpdateTicketDate(ticket.Id);

            var result = new TicketWithLastLogRecord
            {
                TicketModel = ticket,
                LastLog = log
            };

            //Find and Complile email template (replace all %field% with values
            var emailTemplates = _emailTemplateRepository.GetEmailTemplatesByAction(EmailActionId.TicketCreated);

            var substitutions = _ticketFieldRepository.GetTicketFieldsSubstitutions(ticket.Id, GetTicketUrl(ticket.Id));

            //Send email notifications asynchronously
            await SendEmails(ticket.Id, emailTemplates, substitutions);

            return result;
        }


        public async Task<TicketLog> ReopenTicket(int ticketId)
        {
            var prevStatus = _ticketRepository.GetTicketStatusName(ticketId);
            //Update ticket status to Open
            _ticketRepository.SetTicketStatus(ticketId, TicketStatusId.Open);
            var ticket = _ticketRepository.GetTicketById(ticketId);

            ticket.PrevStatus = prevStatus;
            ticket.NewStatus = TicketStatusId.Open.ToString();
            ticket.PrevAssignee = ticket.NewAssignee;
            _ticketRepository.UpdateBasic(ticket);

            var user = GetCurrentUser();
            //Add record to the TicketsLog table
            var log = _ticketLogRepository.Insert(ticketId, (int)LogEntryTypeId.StatusChanged, "Ticket status was changed to: Open", $"Ticket status was changed to: Open by {user.Name}", user.Id);
            _ticketRepository.UpdateTicketDate(ticketId);

            //Find and Complile email template (replace all %field% with values)
            var emailTemplates = _emailTemplateRepository.GetEmailTemplatesByAction(EmailActionId.StatusChanged);

            var substitutions = _ticketFieldRepository.GetTicketFieldsSubstitutions(ticketId, GetTicketUrl(ticketId));
            
            if (IsCustomer())
                await UnassignTicket(ticketId);

            await SendEmails(ticketId, emailTemplates, substitutions);

            return log;
        }


        public async Task<TicketLog> UpdateTicket(TicketModel ticket)
        {
            //Update ticket in the database
            _ticketRepository.Update(ticket);

            var user = GetCurrentUser();
            //Add record to the TicketsLog table
            var log = _ticketLogRepository.Insert(ticket.Id, (int)LogEntryTypeId.TicketEdited, "Ticket was updated", $"Ticket was updated by {user.Name}", user.Id);
            _ticketRepository.UpdateTicketDate(ticket.Id);

            //Find and Complile email template (replace all %field% with values)
            var emailTemplates = _emailTemplateRepository.GetEmailTemplatesByAction(EmailActionId.TicketEdited);

            var substitutions = _ticketFieldRepository.GetTicketFieldsSubstitutions(ticket.Id, GetTicketUrl(ticket.Id));

            await SendEmails(ticket.Id, emailTemplates, substitutions);

            return log;
        }


        public async Task<TicketLog> UnassignTicket(int ticketId)
        {
            //Update ticket in the database
            var ticket = _ticketRepository.GetTicketById(ticketId);
            var prevAssignee = string.IsNullOrEmpty(ticket.AssignedTo)
                ? new User()
                : _userRepository.GetUserById(ticket.AssignedTo);
            ticket.AssignedTo = null;
            ticket.PrevAssignee = prevAssignee.Name;
            ticket.NewAssignee = "";
            ticket.PrevStatus = ticket.NewStatus;
            _ticketRepository.UpdateBasic(ticket);

            var user = GetCurrentUser();
            //Add record to the TicketsLog table
            var log = _ticketLogRepository.Insert(ticket.Id, (int)LogEntryTypeId.AssigneeChanged, "Ticket was unassigned", $"Ticket was unassigned by {user.Name}", user.Id);
            _ticketRepository.UpdateTicketDate(ticketId);

            //Find and Complile email template (replace all %field% with values)
            var emailTemplates = _emailTemplateRepository.GetEmailTemplatesByAction(EmailActionId.AssigneeChanged);

            var substitutions = _ticketFieldRepository.GetTicketFieldsSubstitutions(ticketId, GetTicketUrl(ticketId));

            await SendEmails(ticketId, emailTemplates, substitutions);

            return log;
        }


        //private NameValueCollection GetCustomSubstitutions(int ticketId)
        //{

        //}


        public async Task<TicketLog> AssignToMe(int ticketId)
        {
            //Update ticket in the database
            var user = _userRepository.GetUserByName(HttpContext.Current.User.Identity.Name);
            var ticket = _ticketRepository.GetTicketById(ticketId);

            var prevAssignee = string.IsNullOrEmpty(ticket.AssignedTo)
                ? new User()
                : _userRepository.GetUserById(ticket.AssignedTo);
            ticket.AssignedTo = user.Id;
            ticket.PrevAssignee = prevAssignee.Name;
            ticket.NewAssignee = user.Name;

            if (ticket.TicketStatus.Id == (int) TicketStatusId.Open)
            {
                ticket.PrevStatus = "Open";
                ticket.NewStatus = "In Progress";
                ticket.TicketStatus.Id = (int) TicketStatusId.InProgress;
            }

            _ticketRepository.Update(ticket);

            //Add record to the TicketsLog table
            var log = _ticketLogRepository.Insert(ticket.Id, (int)LogEntryTypeId.AssigneeChanged, "Ticket was assigned" , $"Ticket was assigned to: {user.Name}", user.Id);
            _ticketRepository.UpdateTicketDate(ticketId);

            //Find and Complile email template (replace all %field% with values)
            var emailTemplates = _emailTemplateRepository.GetEmailTemplatesByAction(EmailActionId.AssigneeChanged);

            var substitutions = _ticketFieldRepository.GetTicketFieldsSubstitutions(ticketId, GetTicketUrl(ticketId));

            await SendEmails(ticketId, emailTemplates, substitutions);

            if (HttpContext.Current.User.IsInRole(TicRoles.Admin))
                log.EntryValue = log.EntryExtendedValue;

            return log;
        }


        public async Task<TicketLog> UpdateTicketFields(int ticketId, IList<TicketFieldModel> ticketFields, bool isNewTicket)
        {
            TicketLog log = null;
            //Update ticketFields in the database
            foreach (var ticketField in ticketFields)
            {
                _ticketFieldRepository.UpdateOrInsert(ticketField);
            }

            if (!isNewTicket)
            {
                var user = GetCurrentUser();
                //Add record to the TicketsLog table
                log = _ticketLogRepository.Insert(ticketId, (int)LogEntryTypeId.TicketEdited, "Ticket was updated", $"Ticket was updated by {user.Name}", user.Id);
                _ticketRepository.UpdateTicketDate(ticketId);

                //Find and Complile email template (replace all %field% with values)
                var emailTemplates = _emailTemplateRepository.GetEmailTemplatesByAction(EmailActionId.TicketEdited);

                var substitutions = _ticketFieldRepository.GetTicketFieldsSubstitutions(ticketId, GetTicketUrl(ticketId));

                await SendEmails(ticketId, emailTemplates, substitutions);

            }
            return log;
        }


        public Dictionary<string, IList<TicketViewModel>> GetTicketsModelForStaff(IList<UserDepartment> userDepartments, User user)
        {
            var model = new Dictionary<string, IList<TicketViewModel>>();

            int opened = 0, closed = 0, assignedToMe = 0, unassigned = 0, all = 0;

            foreach (var userDepartment in userDepartments)
            {
                var departmentIds = new List<int>
                {
                    userDepartment.Department.Id
                };
                var openedTickets = _ticketRepository.GetTicketsByDepartmentIdAndStatuses(
                    departmentIds, new List<TicketStatusId> { TicketStatusId.Open }).Count;

                var ticketsAssignedToMe = _ticketRepository.GetTicketsByDepartmentIdAndAssignee(
                    departmentIds, user.Id).Count;

                var ticketsUnassigned = _ticketRepository.GetTicketsByDepartmentIdAndAssignee(
                    departmentIds, null).Count;

                var closedTickets = _ticketRepository.GetTicketsByDepartmentIdAndStatuses(
                    departmentIds,
                    new List<TicketStatusId>
                    {
                        TicketStatusId.ClosedByUser,
                        TicketStatusId.ClosedDidNotHaveAnyIssues,
                        TicketStatusId.ClosedIrrelevantAnymore,
                        TicketStatusId.ClosedIssueWasResolved
                    }).Count;

                var allTickets = _ticketRepository.GetTicketsByDepartmentId(departmentIds).Count();

                if (userDepartment.DepartmentRole.Id == DepartmentRoleId.Staff)
                {
                    model.Add(userDepartment.Department.Name, new List<TicketViewModel>
                    {
                        new TicketViewModel
                        {
                            Name = $"Opened Tickets ({openedTickets})",
                            ModelAttributes = new {departmentId = userDepartment.Department.Id, status = "open"}
                        },
                        new TicketViewModel
                        {
                            Name = $"Assigned to me ({ticketsAssignedToMe})",
                            ModelAttributes = new {departmentId = userDepartment.Department.Id, assignee = "assignedtome"}
                        },
                        new TicketViewModel
                        {
                            Name = $"Unassigned ({ticketsUnassigned})",
                            ModelAttributes = new {departmentId = userDepartment.Department.Id, assignee = "unassigned"}
                        },
                    });

                    opened += openedTickets;
                    assignedToMe += ticketsAssignedToMe;
                    unassigned += ticketsUnassigned;
                }
                else
                {
                    model.Add(userDepartment.Department.Name, new List<TicketViewModel>
                    {
                        new TicketViewModel
                        {
                            Name = $"All Tickets ({allTickets})",
                            ModelAttributes = new {departmentId = userDepartment.Department.Id}
                        },
                        new TicketViewModel
                        {
                            Name = $"Opened Tickets ({openedTickets})",
                            ModelAttributes = new {departmentId = userDepartment.Department.Id, status = "open"}
                        },
                        new TicketViewModel
                        {
                            Name = $"Finished Tickets ({closedTickets})",
                            ModelAttributes = new {departmentId = userDepartment.Department.Id, status = "closed"}
                        },
                        new TicketViewModel
                        {
                            Name = $"Assigned to me ({ticketsAssignedToMe})",
                            ModelAttributes = new {departmentId = userDepartment.Department.Id, assignee = "assignedtome"}
                        },
                        new TicketViewModel
                        {
                            Name = $"Unassigned ({ticketsUnassigned})",
                            ModelAttributes = new {departmentId = userDepartment.Department.Id, assignee = "unassigned"}
                        },
                    });

                    opened += openedTickets;
                    assignedToMe += ticketsAssignedToMe;
                    unassigned += ticketsUnassigned;
                    closed += closedTickets;
                    all += allTickets;
                }
            }

            model.Add("All Departments", new List<TicketViewModel>
            {
                new TicketViewModel {Name = $"All Tickets ({all})"},
                new TicketViewModel {Name = $"Opened Tickets ({opened})", ModelAttributes = new {status = "open"}},
                new TicketViewModel {Name = $"Finished Tickets ({closed})", ModelAttributes = new {status = "closed"}},
                new TicketViewModel
                {
                    Name = $"Assigned to me ({assignedToMe})",
                    ModelAttributes = new {assignee = "assignedtome"}
                },
                new TicketViewModel {Name = $"Unassigned ({unassigned})", ModelAttributes = new {assignee = "unassigned"}},
            });
            return model;
        }



        public IList<TicketModel> GetTicketsModelByFilters(int? departmentId, string status, string assignee, bool isAdmin)
        {
            IList<TicketModel> result;

            var user = _userRepository.GetUserByName(HttpContext.Current.User.Identity.Name);

            List<UserDepartment> userDepartments;

            if (!isAdmin)
                userDepartments = _userDepartmentRepository.GetListByUserId(user.Id).ToList();
            else
                userDepartments = _departmentRepository.GetList()
                        .Select(
                            rec =>
                                new UserDepartment
                                {
                                    Department = new Department { Id = rec.Id, Name = rec.Name },
                                    DepartmentRole = new DepartmentRole { Id = DepartmentRoleId.DepartmentManager }
                                }).ToList();


            var departmentIds = departmentId.HasValue
                ? new List<int> { departmentId.Value }
                : userDepartments.Select(d => d.Department.Id).ToList();

            if (HttpContext.Current.User.IsInRole(TicRoles.CustomerContact))
            {
                result = _ticketRepository.GetTicketsForCustomer(user.Id);

                return result;
            }

            if (status == "open")
                result = _ticketRepository.GetTicketsByDepartmentIdAndStatuses(
                    departmentIds, new List<TicketStatusId> { TicketStatusId.Open });
            else if (status == "closed")
                result = _ticketRepository.GetTicketsByDepartmentIdAndStatuses(
                    departmentIds,
                    new List<TicketStatusId>
                    {
                        TicketStatusId.ClosedByUser,
                        TicketStatusId.ClosedDidNotHaveAnyIssues,
                        TicketStatusId.ClosedIrrelevantAnymore,
                        TicketStatusId.ClosedIssueWasResolved
                    });
            else if (assignee == "unassigned")
                result = _ticketRepository.GetTicketsByDepartmentIdAndAssignee(
                    departmentIds,
                    null);
            else if (assignee == "assignedtome")
                result = _ticketRepository.GetTicketsByDepartmentIdAndAssignee(
                    departmentIds,
                    user.Id);
            else
                result = _ticketRepository.GetTicketsByDepartmentId(
                    departmentIds);

            return result;
        }
        
        public IList<TicketFieldModel> GetTicketFields(int ticketId)
        {
            var result = _ticketFieldRepository.GetTicketFields(ticketId);

            foreach (var ticketFieldModel in result)
            {
                var customFieldTypeId = (CustomFieldTypeId)ticketFieldModel.CustomField.CustomFieldTypeId;

                switch (customFieldTypeId)
                {
                    case CustomFieldTypeId.Dropdown:
                    case CustomFieldTypeId.MultipleSelection:

                        ticketFieldModel.OriginalValues =
                                _databaseRepository.GetDropdownValuesFromTable(ticketFieldModel.CustomField.DbTableName,
                                    ticketFieldModel.CustomField.DbTableIdFieldName,
                                    ticketFieldModel.CustomField.DbTableTextFieldName);

                        break;
                    case CustomFieldTypeId.RelatedDropdowns:

                        var currentStepNumber = ticketFieldModel.CustomField.StepNumber;
                        if (currentStepNumber == null)
                            ticketFieldModel.OriginalValues =
                                _databaseRepository.GetDropdownValuesFromTable(ticketFieldModel.CustomField.DbTableName,
                                    ticketFieldModel.CustomField.DbTableIdFieldName,
                                    ticketFieldModel.CustomField.DbTableTextFieldName);
                        else
                        {
                            var previousField = result[result.IndexOf(ticketFieldModel) - 1];
                            ticketFieldModel.OriginalValues =
                                _databaseRepository.GetDropdownValuesFromTable(ticketFieldModel.CustomField.DbTableName,
                                    ticketFieldModel.CustomField.DbTableIdFieldName,
                                    ticketFieldModel.CustomField.DbTableTextFieldName, ticketFieldModel.CustomField.DbFilterFieldName, previousField.Value);
                        }

                        break;

                }
                
            }

            return result;
        }

        #region Private Helpers

        private async Task SendEmails(int ticketId, IList<MailTemplate> emailTemplates, NameValueCollection substitutions)
        {
            foreach (var emailTemplate in emailTemplates)
            {
                //Determine who should be notified by email
                var usersToNotify = _emailSendToService.GetUsersToNotify(ticketId, emailTemplate.EmailTemplateSendTo);

                //Notify by email
                //This EmailService works asynchronously
                foreach (var user in usersToNotify)
                {
                    await _emailService.Send(user, emailTemplate.EmailSubject, emailTemplate.EmailTemplate, substitutions);
                }
            }
        }




        public IList<ControlEnum> GetTicketButtonsAndControls(TicketModel ticket, string userId, IList<string> userRoles)
        {
            var result = new List<ControlEnum>();
            var ticketIsClosed = ticket.TicketStatus.Category == "Closed";

            if (userRoles.Contains(TicRoles.Staff) || userRoles.Contains(TicRoles.Admin))
            //If user is staff or department maanager
            {
                var departmentRole = _userDepartmentRepository.GetListByUserId(userId).FirstOrDefault(rec=>rec.Department.Id == ticket.DepartmentId);
                var assignedToMe = (ticket.AssignedTo == userId);
                var unassigned = (string.IsNullOrEmpty(ticket.AssignedTo));


                /*if (departmentRole == null)
                {
                    return new List<ControlEnum>();
                }*/


                if (userRoles.Contains(TicRoles.Admin) || departmentRole.DepartmentRole.Id == DepartmentRoleId.DepartmentManager)
                // If user is department manager
                {
                    //result.Add(ControlEnum.BtnEditTicket);
                    result.Add(ControlEnum.BtnReply);
                    result.Add(ControlEnum.BtnComment);
                    result.Add(ControlEnum.BtnAssignToAnotherDepartment);
                    result.Add(ControlEnum.BtnAssignToAnotherUser);

                    if (!assignedToMe)
                        result.Add(ControlEnum.BtnAssgnToMe);

                    result.Add(ControlEnum.LblAssignedTo);

                    if (!unassigned)
                        result.Add(ControlEnum.BtnUnassign);

                    if (ticketIsClosed)
                        result.Add(ControlEnum.BtnReopenTicket);

                    if (!ticketIsClosed)
                        result.Add(ControlEnum.BtnCloseTicket);



                    return result;
                }


                if (departmentRole.DepartmentRole.Id == DepartmentRoleId.Staff)
                // If user is staff
                {
                   // result.Add(ControlEnum.BtnEditTicket);
                    result.Add(ControlEnum.BtnReply);
                    result.Add(ControlEnum.BtnComment);
                    //result.Add(ControlEnum.BtnAssignToAnotherDepartment);
                    //result.Add(ControlEnum.BtnAssignToAnotherUser);
                    //result.Add(ControlEnum.BtnReopenTicket);
                    if (!assignedToMe)
                        result.Add(ControlEnum.BtnAssgnToMe);

                    if (!unassigned)
                        result.Add(ControlEnum.BtnUnassign);

                    if (!ticketIsClosed)
                    {
                        result.Add(ControlEnum.BtnCloseTicket);
                    }

                    return result;
                }
            }



            if (userRoles.Contains(TicRoles.CustomerContact))
            //If user is customer contact
            {
                var isTicketClosed = new List<int>
                {
                    (int) TicketStatusId.ClosedByUser,
                    (int) TicketStatusId.ClosedDidNotHaveAnyIssues,
                    (int) TicketStatusId.ClosedIrrelevantAnymore,
                    (int) TicketStatusId.ClosedIssueWasResolved
                }.Contains(ticket.TicketStatus.Id);

                if (!isTicketClosed)
                {
                    result.Add(ControlEnum.BtnEditTicket);
                    result.Add(ControlEnum.BtnReply);
                }
                //result.Add(ControlEnum.BtnComment);
                //result.Add(ControlEnum.BtnAssignToAnotherDepartment);
                //result.Add(ControlEnum.BtnAssignToAnotherUser);
                //result.Add(ControlEnum.BtnUnassign);
                if(ticketIsClosed)
                    result.Add(ControlEnum.BtnReopenTicket);

                if(! ticketIsClosed)
                    result.Add(ControlEnum.BtnCloseTicket);
                //result.Add(ControlEnum.LblAssignedTo);
                //result.Add(ControlEnum.BtnAssgnToMe);
                return result;
            }

            return new List<ControlEnum>();
        }

        public void CreateTicketFromEmail(string @from, string body, string subject, IList<int> fileIds, string ticketUrlBase)
        {
            //Get default values for the new ticket from TicketDefault table
            var defaultTicket = _ticketRepository.GetTicketDefaultValues();

            //Get customer contact id and customer id by email from
            var customerContact = _customerRepository.GetCustomerContactByEmail(from);

            //If there is no customer contact with such email create new customer contact for the customer with Id = -1 (Unknown)
            if(customerContact == null) customerContact = _customerRepository.AddCustomerContact(new CustomerContactModel {EMail = from, CustomerId = -1, Priority = defaultTicket.PriorityId});

            //Create ticket
            var ticket = new TicketModel
            {
                CustomerId = customerContact.CustomerId,
                CustomerUserId = customerContact.Id,
                CreationDate = DateTime.Now,
                IssueTypeId = defaultTicket.IssueTypeId,
                IssueType = new IssueType { Id = defaultTicket.IssueTypeId},
                PriorityId = defaultTicket.PriorityId,
                ProductId = defaultTicket.ProductId,
                Product = new Product { Id = defaultTicket.ProductId},
                TypeId = defaultTicket.TypeId,
                TicketType = new TicketType { Id = defaultTicket.TypeId },
                ProductCategoryId = defaultTicket.ProductCategoryId,
                ProductCategory = new ProductCategory { Id = defaultTicket.ProductCategoryId},
                StatusId = (int) TicketStatusId.Open
            };

            var ticketWorkflowRule = _ticketWorkflowRepository.GetFirstMathingRule(ticket);
            ticket.Department = new Department { Id = ticketWorkflowRule.Department.Id };
            ticket.DepartmentId = ticketWorkflowRule.Department.Id;

            _ticketRepository.Insert(ticket);

            //Add custom field value for the ticket (from body)
            _ticketFieldRepository.Insert(new TicketFieldModel()
            {
                CustomFieldId = -1,
                CustomField = new CustomField {Id = -1},
                Value = body,
                TextValue = body,
                TicketId = ticket.Id
            });

            //Add attchements if they exists
            if (fileIds != null && fileIds.Count > 0)
                _ticketFieldRepository.Insert(new TicketFieldModel()
                {
                    CustomFieldId = -2,
                    CustomField = new CustomField { Id = -2 },
                    Value = string.Join(",", fileIds),
                    TextValue = "Attachments",
                    TicketId = ticket.Id
                });

            //Add log record
            var log = _ticketLogRepository.Insert(ticket.Id, (int)LogEntryTypeId.TicketCreated, "Ticket was created by email", $"Ticket was created from {from}", "");

            //Find and Complile email template (replace all %field% with values
            var emailTemplates = _emailTemplateRepository.GetEmailTemplatesByAction(EmailActionId.TicketCreated);

            var substitutions = _ticketFieldRepository.GetTicketFieldsSubstitutions(ticket.Id, ticketUrlBase + ticket.Id);

            //Send email notifications asynchronously
            SendEmails(ticket.Id, emailTemplates, substitutions);

        }

        public async Task UpdateTicketFromEmail(int ticketId, string reply, IList<int> fileIds, string ticketUrl)
        {
            //Add record to the TicketsLog table
            var log = _ticketLogRepository.Insert(ticketId, (int)LogEntryTypeId.ReplyAddedByCustomer, $"Customer replied: {reply}", $"Customer replied: {reply}", null);

            //Change ticket ststus to waiting for staff reply
            _ticketRepository.SetTicketStatus(ticketId, TicketStatusId.WaitingForStaffReply);
            _ticketRepository.UpdateTicketDate(ticketId);

            //Find and Complile email template (replace all %field% with values)
            var emailTemplates = _emailTemplateRepository.GetEmailTemplatesByAction(EmailActionId.CustomerReplyAdded);

            var ticket = _ticketRepository.GetTicketById(ticketId);

            //Add attachments if they exists
            if (fileIds != null && fileIds.Count > 0)
            {
                var ticketAttachmentFields = _ticketFieldRepository.GetTicketFields(ticketId).FirstOrDefault(tf => tf.CustomFieldId == -2);
                if (ticketAttachmentFields != null)
                {
                    _ticketFieldRepository.UpdateOrInsert(new TicketFieldModel()
                    {
                        CustomFieldId = -2,
                        CustomField = new CustomField { Id = -2 },
                        Value = string.Join(",", new List<string> {ticketAttachmentFields.Value, string.Join(",", fileIds)}),
                        TextValue = "Attachments",
                        TicketId = ticket.Id
                    });
                }
                else
                {
                    _ticketFieldRepository.Insert(new TicketFieldModel()
                    {
                        CustomFieldId = -2,
                        CustomField = new CustomField {Id = -2},
                        Value = string.Join(",", fileIds),
                        TextValue = "Attachments",
                        TicketId = ticket.Id
                    });
                }
            }

            var substitutions = _ticketFieldRepository.GetTicketFieldsSubstitutions(ticketId, ticketUrl, reply);

            await SendEmails(ticketId, emailTemplates, substitutions);
        }


        private string GetTicketUrl(int ticketId)
        {
            var result = string.Format("{0}://{1}{2}/Tickets/Ticket?ticketId={3}",
                                    HttpContext.Current.Request.Url.Scheme,
                                    HttpContext.Current.Request.Url.Host,
                                    HttpContext.Current.Request.Url.Port == 80
                                        ? string.Empty
                                        : ":" + HttpContext.Current.Request.Url.Port,
                                    ticketId
                                    );

            return result;
        }

        #endregion
    }
}
