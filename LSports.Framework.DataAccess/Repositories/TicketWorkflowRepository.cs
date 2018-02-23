using System;
using System.Linq;
using System.Collections.Generic;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models;

namespace LSports.Framework.DataAccess.Repositories
{
    public class TicketWorkflowRepository : ITicketWorkflowRepository
    {
        public IList<TicketWorkflow> GetList()
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from ticketWorkflow in model.tic_TicketWorkflow

                         join issueType in model.tic_IssueTypes on ticketWorkflow.IssueTypeId equals issueType.Id into inIssueType
                         from outIssueType in inIssueType.DefaultIfEmpty()

                         join product in model.tic_Products on ticketWorkflow.ProductId equals product.Id into inProduct
                         from outProduct in inProduct.DefaultIfEmpty()

                         join productCategory in model.tic_ProductCategories on ticketWorkflow.ProductCategoryId equals productCategory.Id into inProductCategory
                         from outProductCategory in inProductCategory.DefaultIfEmpty()

                         join ticketType in model.tic_TicketTypes on ticketWorkflow.TicketTypeId equals ticketType.Id into inTicketType
                         from outTicketType in inTicketType.DefaultIfEmpty()


                         join department in model.tic_Departments on ticketWorkflow.DepartmentId equals department.Id
                         where ticketWorkflow.IsActive
                         orderby ticketWorkflow.SortOrder
                         select new TicketWorkflow
                         {
                             Id = ticketWorkflow.Id,
                             IssueType = outIssueType == null ? null : new IssueType
                             {
                                 Id = outIssueType.Id,
                                 Name = outIssueType.Name
                             },
                             Product = outProduct == null ? null : new Product
                             {
                                 Id = outProduct.Id,
                                 Name = outProduct.Name
                             },
                             ProductCategory = outProductCategory == null ? null : new ProductCategory
                             {
                                 Id = outProductCategory.Id,
                                 Name = outProductCategory.Name
                             },
                             TicketType = outTicketType == null ? null : new TicketType
                             {
                                 Id = outTicketType.Id,
                                 Name = outTicketType.Name
                             },
                             Department = new Department
                             {
                                 Id = department.Id,
                                 Name = department.Name
                             },
                             CustomerPriorityId = ticketWorkflow.CustomerPriorityId,
                             SortOrder = ticketWorkflow.SortOrder
                         }).ToList();

                return q;
            }
        }

        public TicketWorkflow Insert(TicketWorkflow workflow)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var newRecord = new tic_TicketWorkflow
                {
                    IssueTypeId = workflow.IssueType.Id == 0 ? null : (int?)workflow.IssueType.Id,
                    TicketTypeId = workflow.TicketType.Id == 0 ? null : (int?)workflow.TicketType.Id,
                    ProductId = workflow.Product.Id == 0 ? null : (int?)workflow.Product.Id,
                    ProductCategoryId = workflow.ProductCategory.Id == 0 ? null : (int?)workflow.ProductCategory.Id,
                    CustomerPriorityId = workflow.CustomerPriorityId == 0 ? null : workflow.CustomerPriorityId,
                    DepartmentId = workflow.Department.Id,
                    CreationDate = DateTime.Now,
                    CreatedBy = "Admin",
                    SortOrder = workflow.SortOrder,
                    IsActive = true
                };

                model.tic_TicketWorkflow.Add(newRecord);
                model.SaveChanges();
                workflow.Id = newRecord.Id;

                return workflow;
            }
        }

        public TicketWorkflow Update(TicketWorkflow workflow)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToEdit = model.tic_TicketWorkflow.First(rec => rec.Id == workflow.Id);
                recordToEdit.IssueTypeId = workflow.IssueType.Id == 0 ? null : (int?)workflow.IssueType.Id;
                recordToEdit.TicketTypeId = workflow.TicketType.Id == 0 ? null : (int?)workflow.TicketType.Id;
                recordToEdit.ProductId = workflow.Product.Id == 0 ? null : (int?)workflow.Product.Id;
                recordToEdit.ProductCategoryId = workflow.ProductCategory.Id == 0 ? null : (int?)workflow.ProductCategory.Id;
                recordToEdit.DepartmentId = workflow.Department.Id;
                recordToEdit.CustomerPriorityId = workflow.CustomerPriorityId == 0 ? null : workflow.CustomerPriorityId;
                recordToEdit.SortOrder = workflow.SortOrder;
                recordToEdit.UpdatedBy = "Admin";
                recordToEdit.LastUpdate = DateTime.Now;

                model.SaveChanges();

                return workflow;
            }
        }

        public void Delete(int id)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToEdit = model.tic_TicketWorkflow.First(rec => rec.Id == id);
                recordToEdit.SortOrder = -1;
                recordToEdit.IsActive = false;
                recordToEdit.UpdatedBy = "Admin";
                recordToEdit.LastUpdate = DateTime.Now;

                model.SaveChanges();
            }
        }

        public void UpdateSortOrder(IList<SortOrderItem> sortOrder)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                foreach (var sortOrderItem in sortOrder)
                {
                    var recordToEdit = model.tic_TicketWorkflow.First(rec => rec.Id == sortOrderItem.Id);
                    recordToEdit.SortOrder = sortOrderItem.SortOrder;
                    recordToEdit.UpdatedBy = "Admin";
                    recordToEdit.LastUpdate = DateTime.Now;
                }

                model.SaveChanges();
            }
        }

        public TicketWorkflow GetFirstMathingRule(TicketModel record)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from ticketWorkflow in model.tic_TicketWorkflow
                         join department in model.tic_Departments on ticketWorkflow.DepartmentId equals department.Id
                         where ticketWorkflow.IsActive
                         where ticketWorkflow.IssueTypeId == record.IssueType.Id || ticketWorkflow.IssueTypeId == null
                         where ticketWorkflow.ProductId == record.Product.Id || ticketWorkflow.ProductId == null
                         where ticketWorkflow.ProductCategoryId == record.ProductCategory.Id || ticketWorkflow.ProductCategoryId == null
                         where ticketWorkflow.TicketTypeId == record.TicketType.Id || ticketWorkflow.TicketTypeId == null
                         where ticketWorkflow.CustomerPriorityId == record.PriorityId || ticketWorkflow.CustomerPriorityId == null
                         orderby ticketWorkflow.SortOrder
                         select new TicketWorkflow
                         {
                             Id = ticketWorkflow.Id,
                             Department = new Department
                             {
                                 Id = department.Id,
                                 Name = department.Name
                             },
                         }).FirstOrDefault();

                return q;
            }
        }

        public IList<Error> Validate(TicketWorkflow record)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                record.IssueTypeId = record.IssueType?.Id;
                if (record.IssueTypeId == 0) record.IssueTypeId = null;

                record.ProductCategoryId = record.ProductCategory?.Id;
                if (record.ProductCategoryId == 0) record.ProductCategoryId = null;

                if (record.CustomerPriorityId == 0) record.CustomerPriorityId = null;

                record.TicketTypeId = record.TicketType?.Id;
                if (record.TicketTypeId == 0) record.TicketTypeId = null;

                record.ProductId = record.Product?.Id;
                if (record.ProductId == 0) record.ProductId = null;

                var q =
                    model.tic_TicketWorkflow.FirstOrDefault(
                        rec =>
                            rec.IsActive 
                            && rec.IssueTypeId == record.IssueTypeId 
                            && rec.ProductCategoryId == record.ProductCategoryId
                            && rec.CustomerPriorityId == record.CustomerPriorityId 
                            && rec.ProductId == record.ProductId
                            && rec.TicketTypeId == record.TicketTypeId
                            && rec.Id != record.Id);

                if (q == null) return null;

                return new List<Error> {new Error {Code = 1, ErrorText = "Workflow rule should be unique."} };
            }
        }
    }
}