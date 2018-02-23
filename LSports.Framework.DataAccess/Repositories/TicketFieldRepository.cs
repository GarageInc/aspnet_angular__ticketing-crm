using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models;

namespace LSports.Framework.DataAccess.Repositories
{
    public class TicketFieldRepository : ITicketFieldRepository
    {
        public IList<TicketFieldModel> GetList(int? ticketId)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from ticketField in model.TicketFields
                         join customField in model.tic_CustomFields on ticketField.CustomFieldId equals customField.Id
                         select new TicketFieldModel
                         {
                             Id = ticketField.Id,
                             Value = ticketField.Value,
                             CustomField = new CustomField
                             {
                                 Id = customField.Id,
                             },
                             TextValue = ticketField.TextValue,
                             TicketId = ticketField.TicketId
                         }).ToList();

                return q;
            }
        }


        public TicketFieldModel Insert(TicketFieldModel ticketField)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var newRecord = new TicketField
                {
                    Value = ticketField.Value,
                    CustomFieldId = ticketField.CustomFieldId,
                    TextValue = ticketField.TextValue,
                    TicketId = ticketField.TicketId,
                };

                model.TicketFields.Add(newRecord);
                model.SaveChanges();
                ticketField.Id = newRecord.Id;

                return ticketField;
            }
        }


        public TicketFieldModel UpdateOrInsert(TicketFieldModel ticketField)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToEdit = model.TicketFields.FirstOrDefault(rec => rec.TicketId == ticketField.TicketId && rec.CustomFieldId == ticketField.CustomFieldId);

                if (recordToEdit != null)
                {
                    recordToEdit.TicketId = ticketField.TicketId;
                    recordToEdit.CustomFieldId = ticketField.CustomFieldId;
                    recordToEdit.Value = ticketField.Value;
                    recordToEdit.TextValue = ticketField.TextValue;
                }
                else
                {
                    recordToEdit = new TicketField
                    {
                        Value = ticketField.Value,
                        CustomFieldId = ticketField.CustomFieldId,
                        TextValue = ticketField.TextValue,
                        TicketId = ticketField.TicketId,
                    };

                    model.TicketFields.Add(recordToEdit);
                }

                model.SaveChanges();
                ticketField.Id = recordToEdit.Id;

                return ticketField;
            }
        }


        public void Delete(int id)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToDelete = model.TicketFields.First(rec => rec.Id == id);
                model.TicketFields.Remove(recordToDelete);

                model.SaveChanges();
            }
        }

        public IList<TicketFieldModel> GetTicketFields(int ticketId)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from customField in model.tic_CustomFields
                         join customFieldType in model.tic_CustomFieldTypes on customField.CustomeFieldTypeId equals
                             customFieldType.Id
                         join ticketField in model.TicketFields on customField.Id equals
                             ticketField.CustomFieldId

                         join customDropdownField in model.tic_CustomFields on customField.DropdownCustomFieldId equals customDropdownField.Id into customDropDown
                         from outCustomDropDown in customDropDown.DefaultIfEmpty()
                         where customField.IsActive
                         where ticketField.TicketId == ticketId
                         orderby ticketField.Id
                         select new TicketFieldModel
                         {
                             CustomField = new CustomField
                             {
                                 Id = customField.Id,
                                 Name = customField.DropdownCustomFieldId != null ? outCustomDropDown.Name : customField.Name,
                                 Title = customField.DropdownCustomFieldId != null ? outCustomDropDown.Title : customField.Title,
                                 DbFilterFieldName = customField.DbFilterFieldName,
                                 DbTableIdFieldName = customField.DbTableIdFieldName ?? outCustomDropDown.DbTableIdFieldName,
                                 DbTableName = customField.DbTableName ?? outCustomDropDown.DbTableName,
                                 DbTableTextFieldName = customField.DbTableTextFieldName ?? outCustomDropDown.DbTableTextFieldName,
                                 PlaceholderText = customField.PlaceholderText,
                                 Identifier = customField.Identifier,
                                 StepNumber = customField.StepNumber,
                                 RootCustomFieldId = customField.RootCustomFieldId,
                                 CustomFieldType = new CustomFieldType
                                 {
                                     Id = customFieldType.Id,
                                     Name = customFieldType.Name,
                                 },
                                 CustomFieldTypeId = customFieldType.Id
                             },
                             CustomFieldId = customField.Id,
                             Value = ticketField.Value,
                             TextValue = ticketField.TextValue,
                             TicketId = ticketId,
                             Id = ticketField.Id
                         }).ToList();

                return q;
            }
        }

        public NameValueCollection GetTicketFieldsSubstitutions(int ticketId, string ticketUrl, string reply = "", string comment = "")
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from customField in model.tic_CustomFields
                         join ticketField in model.TicketFields on customField.Id equals
                             ticketField.CustomFieldId

                         where customField.IsActive
                         where ticketField.TicketId == ticketId
                         orderby ticketField.Id
                         select new TicketFieldModel
                         {
                             CustomFieldId = customField.Id,
                             CustomField = new CustomField { Id = customField.Id, Title = customField.Title},
                             Value = ticketField.Value,
                             Field = customField.Identifier,
                             TextValue = ticketField.TextValue,
                             TicketId = ticketId,
                             Id = ticketField.Id
                         }).ToList();

                var result = new NameValueCollection();
                var customFields = new List<TicketFieldModel>();

                foreach (var ticketFieldModel in q)
                {
                    if (!string.IsNullOrEmpty(ticketFieldModel.Field))
                    {
                        result.Add(ticketFieldModel.Field, ticketFieldModel.Value);
                        customFields.Add(ticketFieldModel);
                    }
                }

                var ticket = model.Tickets.FirstOrDefault(t => t.Id == ticketId);
                var customer = model.Customers.FirstOrDefault(c => c.Id == ticket.CustomerId);
                var customerContact = model.CustomerContacts.FirstOrDefault(cc => cc.Id == ticket.CustomerUserId);
                var lastLogRecord = model.TicketsLogs.Where(l => l.TicketId == ticketId).OrderByDescending(l => l.CreationDate).FirstOrDefault();
                var customFieldValues = "<div style=\"margin-bottom: 10px; width:600px;\">";

                foreach (var customField in customFields)
                {
                    customFieldValues += "<div style=\"width: 50%; float: left\">";
                    customFieldValues += string.Format("<i>{0}: </i><b>{1}</b>", customField.CustomField.Title, customField.TextValue);
                    customFieldValues += "</div>";
                }
                customFieldValues += "</div>";

                result.Add("%TicketId%", ticketId.ToString());
                result.Add("%TicketUrl%", ticketUrl);
                result.Add("%ReplyText%", reply);
                result.Add("%CommentText%", comment);
                result.Add("%PrevStatus%", ticket.PrevStatus);
                result.Add("%NewStatus%", ticket.NewStatus);
                result.Add("%PrevAssignee%", ticket.PrevAssignee);
                result.Add("%NewAssignee%", ticket.NewAssignee);
                result.Add("%CreatedOn%", ticket.CreationDate.ToString("yyyy-MM-dd hh:mm"));
                result.Add("%CreatedBy%", (customerContact.FirstName ?? "") + " " + (customerContact.LastName ?? ""));
                result.Add("%Customer%", customer.Company);
                result.Add("%LastTicketLogRecord%", lastLogRecord == null ? "" : HttpUtility.HtmlDecode(lastLogRecord.EntryExtendedValue));
                result.Add("%TicketCustomFieldValues%", customFieldValues);


                return result;
            }
        }
    }
}
