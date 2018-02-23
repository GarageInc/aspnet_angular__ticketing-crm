using System;
using System.Collections.Generic;
using System.Linq;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.DataAccess.Repositories.Interfaces;

namespace LSports.Framework.DataAccess.Repositories
{
    public class TicketTypeRepository : ITicketTypeRepository
    {
        public IList<TicketType> GetList()
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from ticketType in model.tic_TicketTypes
                         join icon in model.tic_Icons on ticketType.IconId equals icon.Id
                         where ticketType.IsActive
                         select new TicketType
                         {
                             Id = ticketType.Id,
                             Name = ticketType.Name,
                             Icon = new _Icon
                             {
                                 Id = icon.Id,
                                 Icon = icon.Icon,
                                 Name = icon.Name
                             }
                         }).ToList();

                return q;
            }
        }


        public TicketType Insert(TicketType ticketType)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var newRecord = new tic_TicketTypes()
                {
                    Name = ticketType.Name,
                    CreatedBy = "Admin",
                    CreationDate = DateTime.Now,
                    IsActive = true,
                    IconId = ticketType.Icon.Id
                };

                model.tic_TicketTypes.Add(newRecord);
                model.SaveChanges();
                ticketType.Id = newRecord.Id;

                return ticketType;
            }
        }


        public TicketType Update(TicketType ticketType)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToEdit = model.tic_TicketTypes.First(rec => rec.Id == ticketType.Id);
                recordToEdit.UpdatedBy = "Admin";
                recordToEdit.LastUpdate = DateTime.Now;
                recordToEdit.IconId = ticketType.Icon.Id;
                recordToEdit.Name = ticketType.Name;

                model.SaveChanges();

                return ticketType;
            }
        }


        public void Delete(int id)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToDelete = model.tic_TicketTypes.First(rec => rec.Id == id);
                recordToDelete.UpdatedBy = "Admin";
                recordToDelete.LastUpdate = DateTime.Now;
                recordToDelete.IsActive = false;

                model.SaveChanges();
            }
        }

        public bool IsTicketTypeNameUnique(string name, int id)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var record = model.tic_TicketTypes.FirstOrDefault(rec => rec.Name == name && rec.IsActive && rec.Id != id);

                return record == null;
            }
        }
    }
}
