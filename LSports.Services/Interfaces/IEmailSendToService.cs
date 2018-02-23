using System.Collections.Generic;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Services.Interfaces
{
    public interface IEmailSendToService
    {
        List<string> GetUsersToNotify(int ticketId, IList<EmailTemplateSendToBrief> sendTo);
    }
}
