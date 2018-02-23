using System.Collections.Specialized;
using System.Threading.Tasks;

namespace LSports.Services.Interfaces
{
    public interface IEmailService
    {
        Task Send(string emailTo, string subject, string bodyHtml,
            NameValueCollection substitutions = null);
    }
}