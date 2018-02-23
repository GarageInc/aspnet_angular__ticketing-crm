using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LSports.Services.Interfaces;

namespace LSports.Services
{
    public class EmailService : IEmailService
    {
        private SmtpClient _client;
        private readonly string _sendFromEmail = ConfigurationManager.AppSettings["EmailUserName"];
        private readonly string _sendFromName = ConfigurationManager.AppSettings["EmailFromName"];

        public EmailService()
        {
            InitializeClient();
        }

        private void InitializeClient()
        {
            var emailServer = ConfigurationManager.AppSettings["EmailSmtpServer"];
            var port = ConfigurationManager.AppSettings["EmailSmtpPort"];
            var userName = ConfigurationManager.AppSettings["EmailUserName"];
            var password = ConfigurationManager.AppSettings["EmailPassword"];

            _client = new SmtpClient(emailServer, int.Parse(port))
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(userName, password),
                EnableSsl = true
            };
        }

        public async Task Send(string emailTo, string subject, string bodyHtml, NameValueCollection substitutions = null)
        {
            var mailMessage = new MailMessage {From = new MailAddress(_sendFromEmail, _sendFromName)};
            mailMessage.To.Add(new MailAddress(emailTo));
            mailMessage.Subject = subject;
            mailMessage.Body = bodyHtml;
            mailMessage.IsBodyHtml = true;

            if (substitutions != null)
            foreach (var key in substitutions.AllKeys)
            {
                mailMessage.Body = mailMessage.Body.Replace(key, substitutions[key]);
                mailMessage.Subject = mailMessage.Subject.Replace(key, substitutions[key]);
            }
            mailMessage.Body = Regex.Replace(mailMessage.Body, @"%[^\s]+%", " ");
            mailMessage.Subject = Regex.Replace(mailMessage.Subject, @"%[^\s]+%", " ");

            await _client.SendMailAsync(mailMessage);
        }

    }
}