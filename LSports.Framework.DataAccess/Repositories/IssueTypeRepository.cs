using System;
using System.Collections.Generic;
using LSports.Framework.Models.CustomClasses;
using System.Linq;
using LSports.Framework.DataAccess.Repositories.Interfaces;

namespace LSports.Framework.DataAccess.Repositories
{
    public class IssueTypeRepository : IIssueTypeRepository
    {
        private const string Admin = "Admin";

        public IList<IssueType> GetList()
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from issueType in model.tic_IssueTypes
                        join icon in model.tic_Icons on issueType.IconId equals icon.Id
                        where issueType.IsActive
                        orderby issueType.Name
                        select new IssueType
                        {
                            Id = issueType.Id,
                            Name = issueType.Name,
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


        public IssueType Insert(IssueType issueType)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var newRecord = new tic_IssueTypes()
                {
                    Name = issueType.Name,
                    CreatedBy = Admin,
                    CreationDate = DateTime.Now,
                    IconId = issueType.Icon.Id,
                    IsActive = true
                };

                model.tic_IssueTypes.Add(newRecord);
                model.SaveChanges();
                issueType.Id = newRecord.Id;

                return issueType;
            }
        }


        public IssueType Update(IssueType issueType)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToEdit = model.tic_IssueTypes.First(rec => rec.Id == issueType.Id);
                recordToEdit.UpdatedBy = "Admin";
                recordToEdit.LastUpdate = DateTime.Now;
                recordToEdit.IconId = issueType.Icon.Id;
                recordToEdit.Name = issueType.Name;

                model.SaveChanges();

                return issueType;

            }
        }


        public void Delete(int id)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToDelete = model.tic_IssueTypes.First(rec => rec.Id == id);
                recordToDelete.UpdatedBy = Admin;
                recordToDelete.LastUpdate = DateTime.Now;
                recordToDelete.IsActive = false;

                model.SaveChanges();
            }
        }

        public bool IsIssueTypeNameUnique(string name, int id)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var record = model.tic_IssueTypes.FirstOrDefault(rec => rec.Name == name && rec.IsActive && rec.Id != id);

                return record == null;
            }
        }
    }
}