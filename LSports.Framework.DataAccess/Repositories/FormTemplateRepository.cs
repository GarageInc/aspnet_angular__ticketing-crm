using System;
using System.Collections.Generic;
using System.Linq;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models;

namespace LSports.Framework.DataAccess.Repositories
{
    public class FormTemplateRepository : IFormTemplateRepository
    {
        public IList<FormTemplate> GetList()
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from formTemplate in model.tic_FormTemplates

                         join issueType in model.tic_IssueTypes on formTemplate.IssueTypeId equals issueType.Id into inIssueType
                         from outIssueType in inIssueType.DefaultIfEmpty()

                         join product in model.tic_Products on formTemplate.ProductId equals product.Id into inProduct
                         from outProduct in inProduct.DefaultIfEmpty()

                         join productCategory in model.tic_ProductCategories on formTemplate.ProductCategoryId equals productCategory.Id into inProductCategory
                         from outProductCategory in inProductCategory.DefaultIfEmpty()

                         join ticketType in model.tic_TicketTypes on formTemplate.TicketTypeId equals ticketType.Id into inTicketType
                         from outTicketType in inTicketType.DefaultIfEmpty()

                         where formTemplate.IsActive
                         orderby formTemplate.SortOrder
                         select new FormTemplate
                         {
                             Id = formTemplate.Id,
                             Name = formTemplate.Name,
                             SortOrder = formTemplate.SortOrder,
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
                             }
                         }).ToList();

                return q;
            }
        }

        public FormTemplate Insert(FormTemplate template)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var newRecord = new tic_FormTemplates
                {
                    Name = template.Name,
                    IssueTypeId = template.IssueType.Id == 0 ? (int?)null : template.IssueType.Id,
                    TicketTypeId = template.TicketType.Id == 0 ? (int?)null : template.TicketType.Id,
                    ProductId = template.Product.Id == 0 ? (int?)null : template.Product.Id,
                    ProductCategoryId = template.ProductCategory.Id == 0 ? (int?)null : template.ProductCategory.Id,
                    SortOrder = template.SortOrder,
                    CreatedBy = "Admin",
                    CreationDate = DateTime.Now,
                    IsActive = true,
                };

                model.tic_FormTemplates.Add(newRecord);
                model.SaveChanges();
                template.Id = newRecord.Id;

                return template;
            }
        }

        public FormTemplate Update(FormTemplate template)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToEdit = model.tic_FormTemplates.First(rec => rec.Id == template.Id);
                recordToEdit.Name = template.Name;
                recordToEdit.IssueTypeId = template.IssueType.Id == 0 ? (int?)null : template.IssueType.Id;
                recordToEdit.TicketTypeId = template.TicketType.Id == 0 ? (int?)null : template.TicketType.Id;
                recordToEdit.ProductId = template.Product.Id == 0 ? (int?)null : template.Product.Id;
                recordToEdit.ProductCategoryId = template.ProductCategory.Id == 0 ? (int?)null : template.ProductCategory.Id;
                recordToEdit.SortOrder = template.SortOrder;
                recordToEdit.UpdatedBy = "Admin";
                recordToEdit.LastUpdate = DateTime.Now;

                model.SaveChanges();

                return template;
            }
        }

        public void Delete(int id)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToDelete = model.tic_FormTemplates.First(rec => rec.Id == id);
                recordToDelete.UpdatedBy = "Admin";
                recordToDelete.LastUpdate = DateTime.Now;
                recordToDelete.IsActive = false;

                model.SaveChanges();
            }
        }

        public void UpdateSortOrder(IList<SortOrderItem> sortOrder)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                foreach (var sortOrderItem in sortOrder)
                {
                    var recordToEdit = model.tic_FormTemplates.First(rec => rec.Id == sortOrderItem.Id);
                    recordToEdit.SortOrder = sortOrderItem.SortOrder;
                    recordToEdit.UpdatedBy = "Admin";
                    recordToEdit.LastUpdate = DateTime.Now;
                }

                model.SaveChanges();
            }
        }

        public IList<FormTemplateCustomField> GetCustomFields(int formtemplateId)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from customField in model.tic_CustomFields
                    join customFieldType in model.tic_CustomFieldTypes on customField.CustomeFieldTypeId equals
                        customFieldType.Id
                    join formTemplateCustomField in model.tic_FormTemplateCustomFields on customField.Id equals
                        formTemplateCustomField.CustomFieldId

                    join customDropdownField in model.tic_CustomFields on customField.DropdownCustomFieldId equals customDropdownField.Id into customDropDown
                    from outCustomDropDown in customDropDown.DefaultIfEmpty()
                    where customField.IsActive
                    where formTemplateCustomField.FormTemplateId == formtemplateId
                    orderby formTemplateCustomField.SortOrder
                    select new FormTemplateCustomField
                    {
                        CustomField = new CustomField
                        {
                            Id = customField.Id,
                            Name = customField.Name,
                            DbFilterFieldName = customField.DbFilterFieldName,
                            DbTableIdFieldName = customField.DbTableIdFieldName ?? outCustomDropDown.DbTableIdFieldName,
                            DbTableName = customField.DbTableName ?? outCustomDropDown.DbTableName,
                            DbTableTextFieldName = customField.DbTableTextFieldName ?? outCustomDropDown.DbTableTextFieldName,
                            PlaceholderText = customField.PlaceholderText,
                            Title = customField.Title,
                            Identifier = customField.Identifier,
                            StepNumber = customField.StepNumber,
                            RootCustomFieldId = customField.RootCustomFieldId,
                            CustomFieldType = new CustomFieldType
                            {
                                Id = customFieldType.Id,
                                Name = customFieldType.Name,
                            },
                        },
                        SortOrder = formTemplateCustomField.SortOrder
                    }).ToList();

                return q;
            }
        }

        public void UpdateCustomFields(int formTemplateId, IList<FormTemplateCustomField> fields)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                foreach (var formCustomField in model.tic_FormTemplateCustomFields.Where(fc => fc.FormTemplateId == formTemplateId).ToList())
                {
                    formCustomField.IsActive = false;
                }

                foreach (var customFieldViewModel in fields)
                {
                    var formCustomField =
                        model.tic_FormTemplateCustomFields.FirstOrDefault(
                            fc =>
                                fc.FormTemplateId == formTemplateId &&
                                fc.CustomFieldId == customFieldViewModel.CustomField.Id);

                    //Check if there's no such field in the database
                    if (formCustomField == null)
                    {
                        formCustomField = new tic_FormTemplateCustomFields
                        {
                            CustomFieldId = customFieldViewModel.CustomField.Id,
                            FormTemplateId = formTemplateId,
                            IsActive = true,
                            CreationDate = DateTime.Now,
                            CreatedBy = "Admin",
                            SortOrder = customFieldViewModel.SortOrder
                        };
                        model.tic_FormTemplateCustomFields.Add(formCustomField);
                    }
                    else
                    {
                        formCustomField.IsActive = true;
                        formCustomField.UpdatedBy = "Admin";
                        formCustomField.LastUpdate = DateTime.Now;
                        formCustomField.SortOrder = customFieldViewModel.SortOrder;
                    }
                }

                model.SaveChanges();
            }
        }

        public FormTemplate GetFirstMatchingTemplate(TicketModel record)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from formTemplate in model.tic_FormTemplates

                         join issueType in model.tic_IssueTypes on formTemplate.IssueTypeId equals issueType.Id into inIssueType
                         from outIssueType in inIssueType.DefaultIfEmpty()

                         join product in model.tic_Products on formTemplate.ProductId equals product.Id into inProduct
                         from outProduct in inProduct.DefaultIfEmpty()

                         join productCategory in model.tic_ProductCategories on formTemplate.ProductCategoryId equals productCategory.Id into inProductCategory
                         from outProductCategory in inProductCategory.DefaultIfEmpty()

                         join ticketType in model.tic_TicketTypes on formTemplate.TicketTypeId equals ticketType.Id into inTicketType
                         from outTicketType in inTicketType.DefaultIfEmpty()

                         where formTemplate.IsActive
                         where outIssueType == null || outIssueType.Id == record.IssueType.Id
                         where outProduct == null || outProduct.Id == record.Product.Id
                         where outProductCategory == null || outProductCategory.Id == record.ProductCategory.Id
                         where outTicketType == null || outTicketType.Id == record.TicketType.Id
                         orderby formTemplate.SortOrder
                         select new FormTemplate
                         {
                             Id = formTemplate.Id,
                         }).FirstOrDefault();

                return q;
            }
        }

        public IList<Error> Validate(FormTemplate record)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                record.IssueTypeId = record.IssueType?.Id;
                if (record.IssueTypeId == 0) record.IssueTypeId = null;

                record.ProductCategoryId = record.ProductCategory?.Id;
                if (record.ProductCategoryId == 0) record.ProductCategoryId = null;

                record.TicketTypeId = record.TicketType?.Id;
                if (record.TicketTypeId == 0) record.TicketTypeId = null;

                record.ProductId = record.Product?.Id;
                if (record.ProductId == 0) record.ProductId = null;

                var q =
                    model.tic_FormTemplates.FirstOrDefault(
                        rec =>
                            rec.IsActive 
                            && rec.IssueTypeId == record.IssueTypeId 
                            && rec.ProductCategoryId == record.ProductCategoryId
                            && rec.ProductId == record.ProductId
                            && rec.TicketTypeId == record.TicketTypeId
                            && rec.Id != record.Id);

                var result = new List<Error>();

                //if (q == null) return null;

                if(q!=null) result.Add( new Error { Code = 1, ErrorText = "Form template rule should be unique." } );


                var theSameNameRecords =
                    model.tic_FormTemplates.FirstOrDefault(
                        rec => rec.IsActive && rec.Name == record.Name && rec.Id != record.Id);

                if(theSameNameRecords != null) result.Add(new Error { Code = 2, ErrorText = "Form template name is not unique."});

                if (result.Count == 0) return null;
                else return result;
            }
        }
    }
}