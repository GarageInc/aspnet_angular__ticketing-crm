using System.Collections.Generic;
using LSports.Framework.Models;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Framework.DataAccess.Repositories.Interfaces
{
    public interface IEmailTemplateRepository
    {
        IList<MailTemplate> GetList();


        MailTemplate Insert(MailTemplate template);


        MailTemplate Update(MailTemplate template);


        void Delete(int id);

        IList<EmailSendTo> GetSendEmailToList();

        IList<EmailAction> GetEmailActionsList();

        IList<MailTemplate> GetEmailTemplatesByAction(string action);

        MailTemplate InsertSendToRecords(MailTemplate template);

        bool IsEmailTemplateNameUnique(int id, string name);
    }
}
