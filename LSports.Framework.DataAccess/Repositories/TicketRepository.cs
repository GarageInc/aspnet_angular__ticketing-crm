using System;
using System.Collections.Generic;
using System.Linq;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models.Enums;

namespace LSports.Framework.DataAccess.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        public IList<TicketModel> GetList()
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = GetFullList(model).ToList();

                return q;
            }
        }

        public TicketModel Insert(TicketModel ticket)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var newRecord = new Ticket
                {
                    IssueTypeId = ticket.IssueType.Id,
                    TypeId = ticket.TicketType.Id,
                    ProductId = ticket.Product.Id,
                    ProductCategoryId = ticket.ProductCategory.Id,
                    CustomerId = ticket.CustomerId,
                    CustomerUserId = ticket.CustomerUserId,
                    PriorityId = ticket.PriorityId,
                    StatusId = ticket.StatusId,
                    DepartmentId = ticket.DepartmentId,
                    PackageId = string.Empty,
                    CreationDate = DateTime.Now,
                };

                model.Tickets.Add(newRecord);
                model.SaveChanges();
                ticket.Id = newRecord.Id;

                return ticket;
            }
        }

        public TicketModel Update(TicketModel ticket)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToEdit = model.Tickets.First(rec => rec.Id == ticket.Id);
                recordToEdit.IssueTypeId = ticket.IssueType.Id;
                recordToEdit.TypeId = ticket.TicketType.Id;
                recordToEdit.ProductId = ticket.Product.Id;
                recordToEdit.ProductCategoryId = ticket.ProductCategory.Id;
                recordToEdit.CustomerId = ticket.Customer.Id;
                recordToEdit.CustomerUserId = ticket.CustomerContact.Id;
                recordToEdit.PriorityId = ticket.PriorityId;
                recordToEdit.AssignedTo = ticket.AssignedTo;
                recordToEdit.StatusId = ticket.TicketStatus.Id;
                recordToEdit.PrevAssignee = ticket.PrevAssignee;
                recordToEdit.NewAssignee = ticket.NewAssignee;
                recordToEdit.PrevStatus = ticket.PrevStatus;
                recordToEdit.NewStatus = ticket.NewStatus;
                recordToEdit.LastUpdate = DateTime.Now;
                if (ticket.DepartmentId != 0 && ticket.DepartmentId != null)
                    recordToEdit.DepartmentId = ticket.DepartmentId;

                if (ticket.Department != null)
                    if(ticket.Department.Id != 0)
                        recordToEdit.DepartmentId = ticket.Department.Id;

                model.SaveChanges();

                return ticket;
            }
        }

        public TicketModel UpdateBasic(TicketModel ticket)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToEdit = model.Tickets.First(rec => rec.Id == ticket.Id);
                recordToEdit.AssignedTo = ticket.AssignedTo;
                recordToEdit.StatusId = ticket.TicketStatus.Id;
                recordToEdit.PrevAssignee = ticket.PrevAssignee;
                recordToEdit.NewAssignee = ticket.NewAssignee;
                recordToEdit.PrevStatus = ticket.PrevStatus;
                recordToEdit.NewStatus = ticket.NewStatus;
                recordToEdit.LastUpdate = DateTime.Now;

                model.SaveChanges();

                return ticket;
            }
        }

        public void Delete(int id)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToDelete = model.Tickets.First(rec => rec.Id == id);
                recordToDelete.LastUpdate = DateTime.Now;

                model.Tickets.Remove(recordToDelete);

                model.SaveChanges();
            }
        }

        public void SetTicketStatus(int ticketId, TicketStatusId ticketStatus)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var ticket = model.Tickets.First(rec => rec.Id == ticketId);

                ticket.StatusId = (int) ticketStatus;
                ticket.LastUpdate = DateTime.Now;

                model.SaveChanges();
            }
        }

        public string GetTicketStatusName(int ticketId)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var ticket = model.Tickets.First(rec => rec.Id == ticketId);

                var ticketStatus = model.tic_TicketStatuses.First(t => t.Id == ticket.StatusId);

                return ticketStatus.Name;
            }
        }

        public TicketModel GetTicketById(int ticketId)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = GetFullList(model).FirstOrDefault(m => m.Id == ticketId);

                return q;
            }
        }


        public TicketModel GetBlankTicketById(int ticketId)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from ticket in model.Tickets
                         join issueType in model.tic_IssueTypes on ticket.IssueTypeId equals issueType.Id
                         join product in model.tic_Products on ticket.ProductId equals product.Id
                         join productCategory in model.tic_ProductCategories on ticket.ProductCategoryId equals
                             productCategory.Id
                         join ticketType in model.tic_TicketTypes on ticket.TypeId equals ticketType.Id
                         where ticket.Id == ticketId
                         select new TicketModel
                         {
                             Id = ticket.Id,
                             IssueType = new IssueType
                             {
                                 Id = issueType.Id,
                                 Name = issueType.Name
                             },
                             Product = new Product
                             {
                                 Id = product.Id,
                                 Name = product.Name
                             },
                             ProductCategory = new ProductCategory
                             {
                                 Id = productCategory.Id,
                                 Name = productCategory.Name
                             },
                             TicketType = new TicketType
                             {
                                 Id = ticketType.Id,
                                 Name = ticketType.Name
                             },
                             CreationDate = ticket.CreationDate,
                             PriorityId = ticket.PriorityId,
                             DepartmentId = ticket.DepartmentId,
                             AssignedTo = ticket.AssignedTo,
                             
                         });

                return q.FirstOrDefault();
            }
        }

        public IList<CustomFieldValue> GetCustomFieldValues(int ticketId)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from ticketField in model.TicketFields
                    join customField in model.tic_CustomFields on ticketField.CustomFieldId equals customField.Id
                    where ticketField.TicketId == ticketId
                    //where customField.CustomeFieldTypeId == (int)CustomFieldTypeId.RelatedDropdowns
                    where customField.RootCustomFieldId == null
                    select new CustomFieldValueInternal()
                    {
                        Title = customField.Title,
                        TextValue = ticketField.TextValue,
                        CustomFieldId = customField.Id,
                        CustomFieldTypeId = customField.CustomeFieldTypeId
                    }).ToList();

                foreach (var item in q.Where(rec => rec.CustomFieldTypeId == (int) CustomFieldTypeId.RelatedDropdowns))
                {
                    var list = from ticketField in model.TicketFields
                        join customField in model.tic_CustomFields on ticketField.CustomFieldId equals customField.Id
                        where customField.IsActive
                        where ticketField.TicketId == ticketId
                        where customField.Id == item.CustomFieldId || customField.RootCustomFieldId == item.CustomFieldId
                        orderby customField.StepNumber
                        select ticketField.TextValue;

                    item.TextValue = string.Join(", ", list.ToList());
                }

                var result = q.Select(rec => new CustomFieldValue {Value = rec.TextValue, Title = rec.Title});

                return result.ToList();
            }
        }

        public TicketModel GetTicketDefaultValues()
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = model.tic_TicketDefault.Select(rec => new TicketModel
                {
                    Id = rec.Id,
                    IssueTypeId = rec.IssueTypeId,
                    PriorityId = rec.PriorityId,
                    ProductCategoryId = rec.ProductCategoryId,
                    ProductId = rec.ProductId,
                    TypeId = rec.TypeId,
                    PackageId = rec.PackageId
                });

                return q.FirstOrDefault();
            }
        }


        public IList<TicketModel> GetTicketsByDepartmentIdAndStatuses(List<int> departmentIds, IList<TicketStatusId> ticketStatusIds)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q =
                    GetFullList(model)
                        .Where(m => ticketStatusIds.Select(st => (int)st).Contains(m.TicketStatus.Id) && departmentIds.Contains(m.DepartmentId.Value))
                        .ToList();

                return q;
            }
        }


        public IList<TicketModel> GetTicketsByDepartmentIdAndAssignee(List<int> departmentIds, string userId)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q =
                    GetFullList(model)
                        .Where(m => m.AssignedTo == userId && departmentIds.Contains(m.DepartmentId.Value))
                        .ToList();

                return q;
            }
        }

        public IList<TicketModel> GetTicketsByDepartmentId(List<int> departmentIds)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q =
                    GetFullList(model)
                        .Where(m => departmentIds.Contains(m.DepartmentId.Value))
                        .ToList();

                return q;
            }
        }


        public IList<TicketModel> GetTicketsForCustomer(string userId)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = GetFullList(model).Where(m => m.CustomerContact.UserId == userId).ToList();

                return q;
            }
        }


        private IEnumerable<TicketModel> GetFullList(gb_ts_stagingEntities model)
        {
            var q = (from ticket in model.Tickets
                join issueType in model.tic_IssueTypes on ticket.IssueTypeId equals issueType.Id
                join product in model.tic_Products on ticket.ProductId equals product.Id
                join productCategory in model.tic_ProductCategories on ticket.ProductCategoryId equals
                    productCategory.Id
                join ticketType in model.tic_TicketTypes on ticket.TypeId equals ticketType.Id
                join customer in model.Customers on ticket.CustomerId equals customer.Id
                join customerContact in model.CustomerContacts on ticket.CustomerUserId equals customerContact.Id
                join ticketStatus in model.tic_TicketStatuses on ticket.StatusId equals ticketStatus.Id
                join user in model.users on ticket.AssignedTo equals user.Id into joinedUserQuery
                from joinedUser in joinedUserQuery.DefaultIfEmpty()
                orderby ticket.Id
                select new TicketModel
                {
                    Id = ticket.Id,
                    IssueType = new IssueType
                    {
                        Id = issueType.Id,
                        Name = issueType.Name
                    },
                    Product = new Product
                    {
                        Id = product.Id,
                        Name = product.Name
                    },
                    ProductCategory = new ProductCategory
                    {
                        Id = productCategory.Id,
                        Name = productCategory.Name
                    },
                    TicketType = new TicketType
                    {
                        Id = ticketType.Id,
                        Name = ticketType.Name
                    },
                    Customer = new CustomerModel
                    {
                        Id = customer.Id,
                        Company = customer.Company
                    },
                    CustomerContact = new CustomerContactModel
                    {
                        Id = customerContact.Id,
                        FirstName = customerContact.FirstName,
                        LastName = customerContact.LastName,
                        UserId = customerContact.UserId,
                    },
                    TicketStatus = new TicketStatus
                    {
                        Id = ticketStatus.Id,
                        Name = ticketStatus.Name,
                        Category = ticketStatus.Category
                    },
                    CreationDate = ticket.CreationDate,
                    PriorityId = ticket.PriorityId,
                    DepartmentId = ticket.DepartmentId,
                    AssignedTo = ticket.AssignedTo,
                    PrevStatus = ticket.PrevStatus,
                    NewStatus = ticket.NewStatus,
                    PrevAssignee = ticket.PrevAssignee,
                    NewAssignee = ticket.NewAssignee,
                    AssignedToUser = joinedUser != null
                        ? new CustomUser
                        {
                            Id = joinedUser.Id,
                            FirstName = joinedUser.FirstName,
                            LastName = joinedUser.LastName,
                            Email = joinedUser.Email
                        }
                        : new CustomUser
                        {
                            Id = null,
                            FirstName = string.Empty,
                            LastName = string.Empty,
                            Email = string.Empty
                        }
                });

            return q;
        }

        public void AttachFilesToTicket(int ticketId, int customFieldId, IList<int> fileIds)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from ticketField in model.TicketFields
                    join customField in model.tic_CustomFields on ticketField.CustomFieldId equals customField.Id
                    where customField.Id == customFieldId
                    where ticketField.TicketId == ticketId
                    where customField.CustomeFieldTypeId == (int) CustomFieldTypeId.Attachments
                    select ticketField).FirstOrDefault();

                if (q != null)
                {
                    if (!string.IsNullOrEmpty(q.Value))
                    {
                        var currentFileIds = q.Value.Split(',').Select(int.Parse);
                        var result = fileIds.Union(currentFileIds);
                        q.Value = string.Join(",", result);
                        
                    }
                    else
                    {
                        q.Value = string.Join(",", fileIds);

                    }
                    model.SaveChanges();
                }
                else
                {
                    var newRecord = new TicketField
                    {
                        CustomFieldId = customFieldId,
                        Value = string.Join(",", fileIds),
                        Field = customFieldId.ToString()
                    };
                    model.TicketFields.Add(newRecord);
                    model.SaveChanges();
                }
            }
        }

        public void DeleteFilesFromTicket(int ticketId, int customFieldId, IList<int> fileIds)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from ticketField in model.TicketFields
                    join customField in model.tic_CustomFields on ticketField.CustomFieldId equals customField.Id
                    where customField.Id == customFieldId
                    where ticketField.TicketId == ticketId
                    where customField.CustomeFieldTypeId == (int) CustomFieldTypeId.Attachments
                    select ticketField).FirstOrDefault();

                if (q != null)
                {
                    var currentFileIds = q.Value.Split(',').Select(int.Parse);
                    var result = currentFileIds.Except(fileIds);
                    q.Value = string.Join(",", result);
                    model.SaveChanges();
                }

            }
        }

        public IList<int> GetAttachedFiles(int ticketId)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from ticketField in model.TicketFields
                         join customField in model.tic_CustomFields on ticketField.CustomFieldId equals customField.Id
                         where ticketField.TicketId == ticketId
                         where customField.CustomeFieldTypeId == (int)CustomFieldTypeId.Attachments
                         select ticketField).FirstOrDefault();

                if (q != null && q.Value != null)
                {
                    var currentFileIds = q.Value.Split(',').Select(int.Parse).ToList();

                    return currentFileIds;
                }

                return new List<int>();
            }
        }



        public IList<TicketAttachment> GetTicketAttachedFiles(int ticketId)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from ticketField in model.TicketFields
                         join customField in model.tic_CustomFields on ticketField.CustomFieldId equals customField.Id
                         where ticketField.TicketId == ticketId
                         where customField.CustomeFieldTypeId == (int)CustomFieldTypeId.Attachments
                         select new 
                         {
                            TicketField = ticketField,
                            CustomField = customField
                         }
                        ).FirstOrDefault();

                if (q != null && !string.IsNullOrEmpty(q.TicketField.Value))
                {
                    var currentFileIds = q.TicketField.Value.Split(',').Select(int.Parse).ToList();

                    var files = from file in model.tic_Files
                        where currentFileIds.Contains(file.Id)
                        select new TicketAttachment
                        {
                            CustomFieldId = q.CustomField.Id,
                            FileId = file.Id,
                            FileName = file.Name,
                            CustomFieldTitle = q.CustomField.Title,
                            TicketFieldId = q.TicketField.Id
                        };

                    return files.ToList();
                }

                return new List<TicketAttachment>();
            }
        }



        public IList<int> GetUpdatedTickets(DateTime fromTime)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from ticket in model.Tickets
                    where ticket.CreationDate >= fromTime || ticket.LastUpdate >= fromTime
                    where ticket.StatusId > 0
                    select ticket.Id).ToList();
                return q;
            }
        }


        public void UpdateTicketDate(int ticketId)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var record = model.Tickets.FirstOrDefault(rec => rec.Id == ticketId);

                if (record == null) return;

                record.LastUpdate = DateTime.Now;
                model.SaveChanges();
            }
        } 
    }
}
