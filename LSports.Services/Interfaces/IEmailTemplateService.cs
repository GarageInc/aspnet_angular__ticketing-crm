using LSports.Framework.Models.CustomClasses;

namespace LSports.Services.Interfaces
{
    public interface IEmailTemplateService
    {
        MailTemplate Update(MailTemplate template);
        MailTemplate Insert(MailTemplate template);

    }
}
