using System.Collections.Generic;
using System.Linq;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.DataAccess.Repositories.Interfaces;

namespace LSports.Framework.DataAccess.Repositories
{
    public class TicketStatusRepository : ITicketStatusRepository
    {
        public IList<TicketStatus> GetClosedTicketStatuses()
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from ticketStatus in model.tic_TicketStatuses
                         where ticketStatus.Category == "Closed"
                         select new TicketStatus
                         {
                             Id = ticketStatus.Id,
                             Name = ticketStatus.Name,
                             Category = ticketStatus.Category,
                         }).ToList();

                return q;
            }
        }
    }
}
