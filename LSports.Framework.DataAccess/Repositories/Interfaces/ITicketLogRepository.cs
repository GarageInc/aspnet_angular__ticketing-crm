using System.Collections.Generic;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Framework.DataAccess.Repositories.Interfaces
{
    public interface ITicketLogRepository
    {
        IList<TicketLog> GetList(int? ticketId, bool shouldHideInternalData, bool shouldHideCustomerData);


        TicketLog Insert(int ticketId, int entryTypeId, string briefEntryValue, string fullEntryValue, string actorUserId);


        void Delete(int id);
    }
}
