using System;
using System.Collections.Generic;
using System.Linq;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models.Enums;

namespace LSports.Framework.DataAccess.Repositories
{
    public class TicketLogRepository : ITicketLogRepository
    {
        public IList<TicketLog> GetList(int? ticketId, bool shouldHideInternalData, bool shouldHideCustomerData)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from ticketsLog in model.TicketsLogs
                         where !ticketId.HasValue || ticketId.Value == ticketsLog.TicketId
                         orderby ticketsLog.CreationDate
                         select new TicketLog
                         {
                             Id = ticketsLog.Id,
                             EntryValue =  
                                ((new List<int> {(int)LogEntryTypeId.TicketCreated, (int)LogEntryTypeId.ReplyAddedByCustomer, (int)LogEntryTypeId.TicketEdited}.Contains(ticketsLog.EntryTypeId) &&
                                     shouldHideCustomerData) 
                                    || shouldHideInternalData)
                                ? ticketsLog.EntryValue : ticketsLog.EntryExtendedValue,
                             EntryTypeId = ticketsLog.EntryTypeId,
                             ActorId = ticketsLog.ActorId,
                             TicketId = ticketsLog.TicketId,
                             LogDate = ticketsLog.CreationDate
                         });

                if(shouldHideInternalData)
                    //Remove comments and assignee data
                    q = q.Where(rec=> ! (new[] { (int?)LogEntryTypeId.AssigneeChanged, (int?) LogEntryTypeId.CommentAdded}).Contains(rec.EntryTypeId ));

                return q.ToList();
            }
        }


        public TicketLog Insert(int ticketId, int entryTypeId, string briefEntryValue, string  fullEntryValue, string actorUserId)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var newRecord = new TicketsLog()
                {
                    TicketId = ticketId,
                    EntryTypeId = entryTypeId,
                    EntryValue = briefEntryValue,
                    CreationDate = DateTime.Now,
                    EntryExtendedValue = fullEntryValue,
                    ActorUserId = actorUserId

                };

                model.TicketsLogs.Add(newRecord);
                model.SaveChanges();

                return new TicketLog
                {
                    Id = newRecord.Id,
                    TicketId = ticketId,
                    EntryTypeId = entryTypeId,
                    EntryValue = briefEntryValue,
                    LogDate = newRecord.CreationDate,
                    EntryExtendedValue = fullEntryValue
                };
            }
        }


        public void Delete(int id)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToDelete = model.TicketsLogs.First(rec => rec.Id == id);
                model.TicketsLogs.Remove(recordToDelete);

                model.SaveChanges();
            }
        }
    }
}
