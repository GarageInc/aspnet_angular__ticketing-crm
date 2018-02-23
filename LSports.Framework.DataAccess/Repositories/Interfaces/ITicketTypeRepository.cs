using System.Collections.Generic;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Framework.DataAccess.Repositories.Interfaces
{
    public interface ITicketTypeRepository
    {
        IList<TicketType> GetList();


        TicketType Insert(TicketType ticketType);


        TicketType Update(TicketType ticketType);


        void Delete(int id);
        bool IsTicketTypeNameUnique(string name, int id);
    }
}
