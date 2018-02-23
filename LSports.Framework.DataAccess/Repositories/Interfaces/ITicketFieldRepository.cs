using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Framework.DataAccess.Repositories.Interfaces
{
    public interface ITicketFieldRepository
    {
        IList<TicketFieldModel> GetList(int? ticketId);


        TicketFieldModel Insert(TicketFieldModel ticketField);


        TicketFieldModel UpdateOrInsert(TicketFieldModel ticketField);


        void Delete(int id);

        IList<TicketFieldModel> GetTicketFields(int ticketId);

        NameValueCollection GetTicketFieldsSubstitutions(int ticketId, string ticketUrl, string reply = "", string comment = "");
    }
}
