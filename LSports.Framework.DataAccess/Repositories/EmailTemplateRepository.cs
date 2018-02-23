using System;
using System.Collections.Generic;
using System.Linq;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models;

namespace LSports.Framework.DataAccess.Repositories
{
    public class EmailTemplateRepository : IEmailTemplateRepository
    {
        public IList<MailTemplate> GetList()
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from mailTemplate in model.tic_EmailTemplate
                         join emailAction in model.tic_EmailActions on mailTemplate.EmailActionKey equals emailAction.Key

                         where mailTemplate.IsActive
                         orderby mailTemplate.Name
                         select new MailTemplate
                         {
                             Id = mailTemplate.Id,
                             EmailAction = emailAction.Name,
                             EmailActionKey = mailTemplate.EmailActionKey,
                             EmailSubject = mailTemplate.EmailSubject,
                             EmailTemplate = mailTemplate.EmailTemplate,
                             Name = mailTemplate.Name
                         }).ToList();

                //Add SendTo collection
                foreach (var item in q)
                {
                    item.EmailTemplateSendTo = (from emailTemplateSendTo in model.tic_EmailTemplateSendTo
                        where emailTemplateSendTo.IsActive
                        where emailTemplateSendTo.EmailTemplateId == item.Id
                        join sendTo in model.tic_EmailSendTo on emailTemplateSendTo.EmailSendToId equals sendTo.Id
                        select new EmailTemplateSendToBrief
                        {
                            Id = emailTemplateSendTo.EmailSendToId,
                            SendTo = sendTo.SendTo,
                            EmailTemplateId = emailTemplateSendTo.EmailTemplateId,
                            EmailSendToId = emailTemplateSendTo.EmailSendToId
                        }).ToList();

                    item.EmailTemplateSendToString = string.Join(", ",
                        item.EmailTemplateSendTo.Select(rec => rec.SendTo));
                }


                return q;
            }
        }

        public MailTemplate Insert(MailTemplate template)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var newRecord = new tic_EmailTemplate
                {
                    EmailActionKey = template.EmailActionKey,
                    EmailSubject = template.EmailSubject,
                    EmailTemplate = template.EmailTemplate,
                    Name = template.Name,
                    CreatedBy = "Admin",
                    CreationDate = DateTime.Now,
                    IsActive = true,
                };

                model.tic_EmailTemplate.Add(newRecord);
                model.SaveChanges();
                template.Id = newRecord.Id;

                return template;
            }
        }


        public MailTemplate InsertSendToRecords(MailTemplate template)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = model.tic_EmailTemplateSendTo.Where(rec => rec.IsActive && rec.EmailTemplateId == template.Id);
                //Delete existing records
                foreach (var rec in q)
                {
                    rec.IsActive = false;
                    rec.LastUpdate = DateTime.Now;
                }

                foreach (var recToAdd in template.EmailTemplateSendTo)
                {
                    var record = q.FirstOrDefault(rec => rec.EmailSendToId == recToAdd.EmailSendToId);
                    if (record != null)
                    {
                        record.IsActive = true;
                        record.LastUpdate = DateTime.Now;
                    }
                    else
                    {
                        var newRecord = new tic_EmailTemplateSendTo();
                        newRecord.CreationDate = DateTime.Now;
                        newRecord.CreatedBy = "Admin";
                        newRecord.EmailTemplateId = template.Id;
                        newRecord.EmailSendToId = recToAdd.Id;
                        newRecord.IsActive = true;
                        model.tic_EmailTemplateSendTo.Add(newRecord);
                        model.SaveChanges();
                       // recToAdd.Id = newRecord.Id;
                    }
                }

                model.SaveChanges();

                return template;
            }
        }



        public MailTemplate Update(MailTemplate template)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToEdit = model.tic_EmailTemplate.First(rec => rec.Id == template.Id);
                recordToEdit.EmailActionKey = template.EmailActionKey;
                recordToEdit.EmailSubject = template.EmailSubject;
                recordToEdit.EmailTemplate = template.EmailTemplate;
                recordToEdit.Name = template.Name;
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
                var recordToDelete = model.tic_EmailTemplate.First(rec => rec.Id == id);
                recordToDelete.UpdatedBy = "Admin";
                recordToDelete.LastUpdate = DateTime.Now;
                recordToDelete.IsActive = false;

                model.SaveChanges();
            }
        }

        public IList<EmailSendTo> GetSendEmailToList()
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from emailSendTo in model.tic_EmailSendTo
                         orderby emailSendTo.Id
                         select new EmailSendTo
                         {
                             Id = emailSendTo.Id,
                             SendTo = emailSendTo.SendTo,
                         }).ToList();

                return q;
            }
        }

        public IList<EmailAction> GetEmailActionsList()
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from actions in model.tic_EmailActions
                         orderby actions.Key
                         select new EmailAction
                         {
                             Id = actions.Key,
                             Name = actions.Name,
                         }).ToList();

                return q;
            }
        }

        public IList<MailTemplate> GetEmailTemplatesByAction(string action)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var item = (from mailTemplate in model.tic_EmailTemplate
                    join emailAction in model.tic_EmailActions on mailTemplate.EmailActionKey equals emailAction.Key
                    where mailTemplate.EmailActionKey == action
                    where mailTemplate.IsActive
                    orderby mailTemplate.Name
                    select new MailTemplate
                    {
                        Id = mailTemplate.Id,
                        EmailAction = emailAction.Name,
                        EmailActionKey = mailTemplate.EmailActionKey,
                        EmailSubject = mailTemplate.EmailSubject,
                        EmailTemplate = mailTemplate.EmailTemplate,
                        Name = mailTemplate.Name
                    }).ToList();

                //Add SendTo collection
                foreach (var mailTemplate in item)
                {
                    var sendToList = (from emailTemplateSendTo in model.tic_EmailTemplateSendTo
                        join sendTo in model.tic_EmailSendTo on emailTemplateSendTo.EmailSendToId equals sendTo.Id
                        where emailTemplateSendTo.IsActive
                        where emailTemplateSendTo.EmailTemplateId == mailTemplate.Id
                        select new EmailTemplateSendToBrief
                        {
                            Id = emailTemplateSendTo.Id,
                            SendTo = sendTo.SendTo,
                            EmailTemplateId = emailTemplateSendTo.EmailTemplateId,
                            EmailSendToId = emailTemplateSendTo.EmailSendToId
                        }).ToList();

                    mailTemplate.EmailTemplateSendTo = sendToList;
                }

                return item;
            }
        }

        public bool IsEmailTemplateNameUnique(int id, string name)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var record = model.tic_EmailTemplate.FirstOrDefault(rec => rec.Name == name && rec.IsActive && rec.Id != id);

                return record == null;
            }
        }
    }
}