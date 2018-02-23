using System.Collections;
using System.Collections.Generic;

namespace LSports.Framework.Models.CustomClasses
{
    public class MailTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string EmailTemplate { get; set; }
        public string EmailAction { get; set; }
        public string EmailSubject { get; set; }
        public string EmailActionKey { get; set; }
        public IList<EmailTemplateSendToBrief> EmailTemplateSendTo { get; set; }
        public string EmailTemplateSendToString { get; set; }
    }
}
