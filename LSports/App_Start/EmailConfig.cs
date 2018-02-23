using System;
using System.Collections.Generic;
using S22.Imap;
using System.Net.Mail;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using LSports.Services;
using LSports.Services.Interfaces;
using File = LSports.Framework.Models.CustomClasses.File;

namespace LSports
{
    public partial class Startup
    {
        private static ImapClient _imapClient;
        private static readonly ITicketService _ticketService = new TicketService();
        private static readonly IFileService _fileService = new FileService();
        private static string _ticketUrlBase;

        public void ConfigureEmail()
        {
            var emailServer = ConfigurationManager.AppSettings["EmailImapServer"];
            var port = ConfigurationManager.AppSettings["EmailImapPort"];
            var userName = ConfigurationManager.AppSettings["EmailUserName"];
            var password = ConfigurationManager.AppSettings["EmailPassword"];

            _imapClient = new ImapClient(emailServer, Convert.ToInt32(port), userName,
                password, AuthMethod.Login, true);
            
            // We should make sure IDLE is actually supported by the server.
            if (_imapClient.Supports("IDLE") == false)
            {
                Console.WriteLine("Server does not support IMAP IDLE");
                return;
            }

            // We want to be informed when new messages arrive.
            _imapClient.NewMessage += new EventHandler<IdleMessageEventArgs>(OnNewMessage);

            // Put calling thread to sleep. This is just so the example program does
            // not immediately exit.
            //System.Threading.Thread.Sleep(60000);
            _ticketUrlBase = string.Format("{0}/Tickets/Ticket?ticketId=",
                                    ConfigurationManager.AppSettings["HostName"]);
        }

        static void OnNewMessage(object sender, IdleMessageEventArgs e)
        {
            Console.WriteLine("A new message has been received. Message has UID: " +
                e.MessageUID);

            // Fetch the new message's headers and print the subject line
            MailMessage m = e.Client.GetMessage(e.MessageUID, FetchOptions.Normal);

            var fileIds = new List<int>();

            if (string.IsNullOrEmpty(m.Headers["Auto-Submitted"]) && m.From.Address != ConfigurationManager.AppSettings["EmailUserName"])
            //if it's not a autoresponder such as Undelivered Mail Returned to Sender
            {
                if (m.Body.Contains("-----TicketId"))
                {
                    var list = new List<File>();
                    if (m.Attachments.Count > 0)
                    {
                        foreach (var attachment in m.Attachments)
                            list.Add(_fileService.SaveFile(attachment.Name, (MemoryStream)attachment.ContentStream));
                        fileIds = list.Select(rec => rec.Id).ToList();
                    }

                    var ticketIdMatch = Regex.Match(m.Body, @"[-]{5}TicketId: [\d]+[-]{5}").Value.Replace("-----", "").Replace("TicketId: ", "");
                    var ticketId = int.Parse(ticketIdMatch);

                    var bodyInitialArray = m.Body.Split(new string[] {"\r\n\r\n"}, StringSplitOptions.RemoveEmptyEntries);
                    var bodyFinalArray = new List<string>();

                    foreach (var element in bodyInitialArray)
                    {
                        if (element.Contains(ConfigurationManager.AppSettings["EmailUserName"]))
                            break;
                        bodyFinalArray.Add(element);
                    }

                    var reply = string.Join("\r\n", bodyFinalArray);

                    _ticketService.UpdateTicketFromEmail(ticketId, reply, fileIds, _ticketUrlBase);
                }
                else
                {
                    var list = new List<File>();
                    if (m.Attachments.Count > 0)
                    {
                        foreach (var attachment in m.Attachments)
                            list.Add(_fileService.SaveFile(attachment.Name, (MemoryStream) attachment.ContentStream));
                        fileIds = list.Select(rec => rec.Id).ToList();
                    }

                    _ticketService.CreateTicketFromEmail(m.From.Address, m.Body, m.Subject, fileIds, _ticketUrlBase);
                }
            }

        }
    }
}