

namespace LSports.Framework.Models.CustomClasses
{
    public class EmailTemplateSendToBrief
    {
        public int Id { get; set; }
        public int EmailTemplateId { get; set; }
        public int EmailSendToId { get; set; }
        public string SendTo { get; set; }
    }
}
