using LSports.Framework.DataAccess.Repositories;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models.CustomClasses;
using LSports.Services.Interfaces;

namespace LSports.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IEmailTemplateRepository _emailTemplateRepository;

        public EmailTemplateService() : this(new EmailTemplateRepository())
        {
        }


        public EmailTemplateService(IEmailTemplateRepository emailTemplateRepository)
        {
            _emailTemplateRepository = emailTemplateRepository;
           
        }



        public MailTemplate Insert(MailTemplate template)
        {
            _emailTemplateRepository.Insert(template);
            _emailTemplateRepository.InsertSendToRecords(template);
            return template;
        }



        public MailTemplate Update(MailTemplate template)
        {
            _emailTemplateRepository.Update(template);
            _emailTemplateRepository.InsertSendToRecords(template);
            return template;
        }
    }
}
