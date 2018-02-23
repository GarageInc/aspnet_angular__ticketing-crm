using System;
using System.Collections.Generic;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.Models.Enums;

namespace LSports.Framework.DataAccess.Repositories.Interfaces
{
    public interface ITicketRepository
    {
        IList<TicketModel> GetList();

        TicketModel Insert(TicketModel ticket);

        TicketModel Update(TicketModel ticket);

        TicketModel UpdateBasic(TicketModel ticket);

        void Delete(int id);

        void SetTicketStatus(int ticketId, TicketStatusId ticketStatus);

        string GetTicketStatusName(int ticketId);

        TicketModel GetTicketById(int ticketId);

        IList<TicketModel> GetTicketsForCustomer(string userId);

        IList<TicketModel> GetTicketsByDepartmentIdAndStatuses(List<int> departmentIds, IList<TicketStatusId> ticketStatusIds);

        IList<TicketModel> GetTicketsByDepartmentIdAndAssignee(List<int> departmentIds, string userId);

        IList<TicketModel> GetTicketsByDepartmentId(List<int> departmentIds);

        void AttachFilesToTicket(int ticketId, int customFieldId, IList<int> fileIds);

        void DeleteFilesFromTicket(int ticketId, int customFieldId, IList<int> fileIds);

        IList<int> GetAttachedFiles(int ticketId);

        IList<TicketAttachment> GetTicketAttachedFiles(int ticketId);

        TicketModel GetBlankTicketById(int ticketId);

        IList<CustomFieldValue> GetCustomFieldValues(int ticketId);

        TicketModel GetTicketDefaultValues();

        IList<int> GetUpdatedTickets(DateTime fromTime);

        void UpdateTicketDate(int ticketId);
    }
}
