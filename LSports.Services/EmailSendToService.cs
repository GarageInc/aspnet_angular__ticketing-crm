using System.Collections.Generic;
using System.Linq;
using LSports.Framework.DataAccess.Repositories;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.Models.Enums;
using LSports.Services.Interfaces;

namespace LSports.Services
{
    public class EmailSendToService : IEmailSendToService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUserDepartmentRepository _userDepartmentRepository;
        private readonly IUserRepository _userRepository;

        public EmailSendToService() : this(new TicketRepository(), new CustomerRepository(), new UserDepartmentRepository(), new UserRepository())
        {
        }

        public EmailSendToService(ITicketRepository ticketRepository, ICustomerRepository customerRepository, IUserDepartmentRepository userDepartmentRepository, IUserRepository userRepository)
        {
            _ticketRepository = ticketRepository;
            _customerRepository = customerRepository;
            _userDepartmentRepository = userDepartmentRepository;
            _userRepository = userRepository;
        }

        public List<string> GetUsersToNotify(int ticketId, IList<EmailTemplateSendToBrief> sendTo)
        {
            var ticket = _ticketRepository.GetTicketById(ticketId);
            var result = new List<string>();

            foreach (var sendToBrief in sendTo)
            {
                var sendToId = (EmailSendToId)sendToBrief.EmailSendToId;

                switch (sendToId)
                {
                    case EmailSendToId.Customer:

                        var customerContacts = _customerRepository.GetCustomerContactsByCustomerId(ticket.Customer.Id);
                        result.AddRange(customerContacts.Select(c => c.EMail).ToList());

                        break;
                    case EmailSendToId.Staff:

                        var userDepartments = _userDepartmentRepository.GetListByDepartmentId(ticket.DepartmentId.Value);
                        result.AddRange(userDepartments.Where(ud => ud.DepartmentRole?.Id == DepartmentRoleId.Staff).Select(ud => ud.User.Email).ToList());

                        break;
                    case EmailSendToId.DepartmentManager:

                        userDepartments = _userDepartmentRepository.GetListByDepartmentId(ticket.DepartmentId.Value);
                        result.AddRange(userDepartments.Where(ud => ud.DepartmentRole?.Id == DepartmentRoleId.DepartmentManager).Select(ud => ud.User.Email).ToList());

                        break;
                    case EmailSendToId.Adminstrator:

                        var users = _userRepository.GetUsersByRole(TicRoles.Admin).Select(u => u.Email).ToList();
                        result.AddRange(users);

                        break;
                }
            }

            result = result.Distinct().ToList();

            return result;
        }
    }
}
