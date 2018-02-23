
using System.Collections.Generic;
using System.Threading.Tasks;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.Models.Enums;
using LSports.ViewModels;

namespace LSports.Services.Interfaces
{
    public interface ITicketService
    {
        Task<TicketWithLastLogRecord> CreateTicket(TicketModel ticket);

        Task<TicketLog> UpdateTicket(TicketModel ticket);

        Task<TicketLog> CloseTicket(TicketModel ticket, string comment, int? statusId);

        Task<TicketLog> AssignToAnotherUser(int ticketId, string userId, string comment);

        Task<TicketLog> AssignToAnotherDepartment(int ticketId, int departmentId, string comment);

        Task<TicketLog> AddStaffReply(int ticketId, string reply);

        Task<TicketLog> AddStaffComment(int ticketId, string comment);

        Task<TicketLog> AddCustomerReply(int ticketId, string reply);

        Task<TicketLog> ReopenTicket(int ticketId);

        Task<TicketLog> UnassignTicket(int ticketId);

        Task<TicketLog> AssignToMe(int ticketId);

        Dictionary<string, IList<TicketViewModel>> GetTicketsModelForStaff(IList<UserDepartment> userDepartments,
            User user);

        IList<TicketModel> GetTicketsModelByFilters(int? departmentId, string status, string assignee, bool isAdmin);

        Task<TicketLog> UpdateTicketFields(int ticketId, IList<TicketFieldModel> ticketFields, bool isNewTicket);

        IList<TicketFieldModel> GetTicketFields(int ticketId);
        TicketTemplateViewModel CreateBlankTicket(TicketModel ticket);
        TicketLog AttachFilesToTicket(int ticketId, int customFieldId, IList<int> fileIds);
        TicketLog DeleteFilesFromTicket(int ticketId, int customFieldId, IList<int> fileIds);
        IList<ControlEnum> GetTicketButtonsAndControls(TicketModel ticket, string userId, IList<string> userRoles);

        void CreateTicketFromEmail(string @from, string body, string subject, IList<int> fileIds, string ticketUrlBase);
        Task UpdateTicketFromEmail(int ticketId, string reply, IList<int> fileIds, string ticketUrl);
    }
}
